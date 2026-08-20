using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using SIGRA.Controllers;
using SIGRA.Data;
using SIGRA.Data.Enums;
using SIGRA.Data.Models;
using SIGRA.Data.Repositories;
using SIGRA.Domain;
using SIGRA.Services.Helper;
using SIGRA.Domain.Exceptions;
using SIGRA.Services.Handlers;

namespace SIGRA.Services;

public class TicketService : ITicketService
{
    private readonly AppDbContext _context;
    private readonly IBusinessTimeCalculator _businessTimeCalculator;
    private readonly ITicketSlaService _ticketSlaService;
    private readonly ITicketRepository _ticketRepository;
    private readonly IStatutRepository _statutRepository;
    private readonly IEmailsSourceRepository _emailSourceRepository;
    private readonly IPiecesJointeRepository _pieceJointeRepository;
    private readonly IStorageService _storageService;
    private readonly IConfiguration _config;
    private readonly ILogger<TicketService> _logger;
    private readonly IUserAuthenticationService _userAuthenticationService;
    // private readonly INotificationService _notificationService;
    private readonly IDomainEventDispatcher _eventDispatcher;

    public TicketService(
        AppDbContext context,
        IBusinessTimeCalculator businessTimeCalculator,
        ITicketSlaService ticketSlaService,
        ITicketRepository ticketRepository,
        IStatutRepository statutRepository,
        IEmailsSourceRepository emailSourceRepository,
        IPiecesJointeRepository pieceJointeRepository,
        IStorageService storageService,
        IConfiguration config,
        ILogger<TicketService> logger,
        IUserAuthenticationService userAuthenticationService,
        // INotificationService notificationService)
        IDomainEventDispatcher eventDispatcher)
    {
        _context = context;
        _businessTimeCalculator = businessTimeCalculator;
        _ticketSlaService = ticketSlaService;
        _ticketRepository = ticketRepository;
        _statutRepository = statutRepository;
        _emailSourceRepository = emailSourceRepository;
        _pieceJointeRepository = pieceJointeRepository;
        _storageService = storageService;
        _config = config;
        _logger = logger;
        _userAuthenticationService = userAuthenticationService;
        // _notificationService = notificationService;
        _eventDispatcher = eventDispatcher;
    }

    public async Task ReopenTicketAsync(ReopenTicketRequest request, int idAuteur)
    {
        var ticket = await _context.Tickets.Include(t => t.IdApplicationNavigation).FirstOrDefaultAsync(t => t.IdTicket == request.TicketId)
            ?? throw new NotFoundException($"Ticket {request.TicketId} introuvable.");

        // Validation de la transition via le state machine centralisé
        var autorisee = TicketStatusTransitions.IsValidTransition((TicketStatus)ticket.IdStatut, TicketStatus.Opened);
        if (!autorisee)
            return;

        if (string.IsNullOrWhiteSpace(request.Reason))
        {
            throw new ValidationException("Un motif de réouverture est obligatoire.");
        }

        var dateClotureOriginal = ticket.DateCloture
            ?? throw new InvalidOperationException("La date de clôture manquant sur un ticket clôturé.");

        // 1) Historiser l'événement de réouverture AVANT de modifier le ticket
        // _context.TicketReopenHistories.Add(new TicketReopenHistory
        // {
        //     Id = Guid.NewGuid(),
        //     TicketId = ticket.Id,
        //     OriginalClosedAt = originalClosedAt,
        //     ReopenedAt = DateTime.UtcNow,
        //     ReopenedByUserId = currentUserId,
        //     Reason = request.Reason,
        //     WasOriginalResolutionSlaBreached = ticket.WasResolutionSlaBreached
        // });

        // 2) Recalculer les deadlines SLA pour la nouvelle phase
        // var policy = await _slaPolicyProvider.GetPolicyAsync(ticket.Priority);
        var classeService = await _context.ClassesServices.AsNoTracking().FirstOrDefaultAsync(c => c.IdCs == ticket.IdApplicationNavigation.IdCs);
        var reopenDuration = TimeSpan.FromHours((double)classeService.DureeSlaReouverture);// policy.ReopenResolutionTime ?? policy.ResolutionTime;

        var newResolutionDeadline = await _businessTimeCalculator
            .AddBusinessTimeAsync(DateTime.UtcNow, reopenDuration);

        // 3) Mettre à jour l'état du ticket
        ticket.Reouvrir(newResolutionDeadline, idAuteur, request.Reason);

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Ticket {TicketId} réouvert par {UserId}. Motif: {Reason}. Nouvelle deadline: {Deadline}",
            ticket.IdTicket, idAuteur, request.Reason, newResolutionDeadline);

        // 4) Notifier les parties concernées
        await _eventDispatcher.DispatchAsync(ticket.DomainEvents);
        ticket.ClearDomainEvents();
    }

    public async Task<Ticket> GetFicheTicket(int idTicket)
    {
        var ticket = await _ticketRepository.GetFicheTicket(idTicket);
        return ticket;
    }

    public async Task TransferAsync(int ticketId, int idEntiteExterne, int idAuteur, string explication, bool estDefinitif)
    {
        var escalade = new Escalade
        {
            IdTicket = ticketId,
            IdEntiteExterne = idEntiteExterne,
            IdAuteur = idAuteur,
            DateEscalade = DateTime.UtcNow,
            Explication = explication,
            EstDefinitif = estDefinitif
        };
        await _context.Escalades.AddAsync(escalade);
        var ticket = await _context.Tickets.FindAsync(ticketId);
        if (ticket is null)
            throw new Exception("Ticket not found");
        ticket.Transferer(escalade);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> RespondRejectDemandAsync(int ticketId, int idValidateur, bool isRejected)
    {
        var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.IdTicket == ticketId, default);
        if (ticket is null)
            throw new Exception("Ticket not found");

        var rejet = await _context.Rejets.FirstOrDefaultAsync(r => r.IdTicket == ticketId && r.Decision == null, default);

        rejet = ticket.ValiderRejet(rejet, idValidateur, isRejected);

        await _context.SaveChangesAsync();
        return isRejected;
    }

    public async Task<Result> AskRejectAsync(int ticketId, int idAuteur, string justificatif)
    {
        var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.IdTicket == ticketId, default);
        if (ticket is null)
            return Result.Failure("Ticket not found", ErrorType.NotFound);

        if (ticket.IdStatut == (int)TicketStatus.PendingReject ||
            await _context.Rejets.AsNoTracking().AnyAsync(r => r.IdTicket == ticketId && r.Decision == null))
        {
            return Result.Failure("Rejection request already pending for this ticket.", ErrorType.Conflict);
        }

        var rejet = new Rejet
        {
            IdTicket = ticketId,
            IdAuteur = idAuteur,
            Justificatif = justificatif,
            DateProposition = DateTime.UtcNow
        };

        _context.Rejets.Add(rejet);
        ticket.AttendreRejet();

        await _context.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<PendingRejectResponse?> GetPendingRejectAsync(int ticketId)
    {
        return await _context.Rejets.AsNoTracking()
            .Where(r => (r.IdTicket == ticketId && r.Decision == null) || (r.IdTicket == ticketId && r.Decision == true))
            .OrderByDescending(r => r.DateProposition)
            .Select(r => new PendingRejectResponse(
                r.IdRejet,
                r.IdTicket,
                new TechnicienResponse(r.IdAuteurNavigation.Nom, r.IdAuteurNavigation.Prenom, "", Guid.Empty),
                r.Justificatif,
                r.DateProposition,
                r.IdValidateur,
                r.Decision,
                r.DateDecision))
            .FirstOrDefaultAsync();
    }

    public async Task<Ticket?> CreateTicketFromEmailAsync(
        MimeMessage message,
        string? conversationId = null,
        CancellationToken cancellationToken = default)
    {
        var mailInfo = ImapMailService.MapToMailInfo(message);

        _logger.LogInformation(
            "New message: {Subject} from {Sender} <{SenderEmail}> on {SentDate:u}",
            mailInfo.Subject,
            mailInfo.Sender,
            mailInfo.SenderEmail,
            mailInfo.SentDate);

        foreach (var attachment in mailInfo.Attachments)
        {
            _logger.LogInformation(
                "Attachment: {FileName} ({ContentType}) - {Size} bytes",
                attachment.FileName,
                attachment.ContentType,
                attachment.Size);
        }

        var messageId = message.MessageId ?? Guid.NewGuid().ToString();
        var inReplyTo = message.InReplyTo;
        var references = message.References?.ToList() ?? new List<string>();

        var existingEmail = await _emailSourceRepository.GetByMessageIdAsync(messageId, cancellationToken);
        if (existingEmail != null)
        {
            _logger.LogInformation("Email with MessageId {MessageId} already exists. Skipping.", messageId);
            return null;
        }

        string? resolvedConversationId = null;
        int? resolvedTicketId = null;
        var parentMessageIds = new List<string>();

        if (!string.IsNullOrEmpty(inReplyTo))
            parentMessageIds.Add(inReplyTo);
        if (references != null)
            parentMessageIds.AddRange(references);

        foreach (var parentMessageId in parentMessageIds)
        {
            var parentEmail = await _emailSourceRepository.GetByMessageIdAsync(parentMessageId, cancellationToken);
            if (parentEmail != null)
            {
                resolvedConversationId = parentEmail.ConversationIdGraph;
                resolvedTicketId = parentEmail.IdTicket;
                break;
            }
        }

        Ticket ticket;
        bool isFirstEmail;

        if (resolvedTicketId.HasValue)
        {
            isFirstEmail = false;
            ticket = await _context.Tickets.FindAsync(resolvedTicketId.Value, cancellationToken)
                ?? throw new InvalidOperationException($"Ticket {resolvedTicketId.Value} not found for conversation.");
        }
        else
        {
            isFirstEmail = true;
            resolvedConversationId = conversationId ?? Guid.NewGuid().ToString();

            // var statutId = _config.GetValue<int?>("TicketDefaults:StatutId");
            // if (!statutId.HasValue)
            // {
            //     statutId = await _statutRepository.GetIdStatutByDefaultAsync(cancellationToken);
            //     if (!statutId.HasValue)
            //         throw new InvalidOperationException("Default Statut not found.");
            // }

            ticket = new Ticket
            {
                NumeroTicket = await GenerateTempTicketNumber(),
                DemandeurEmail = mailInfo.SenderEmail,
                DemandeurDirection = string.IsNullOrWhiteSpace(mailInfo.Sender) ? mailInfo.SenderEmail : mailInfo.Sender,
                DateCreation = mailInfo.SentDate.UtcDateTime,
                DureeSla = 0
            };

            ticket.Creer();

            await _ticketRepository.CreateAsync(ticket, cancellationToken);

            _logger.LogInformation(
                "Ticket {TicketNumber} created from email: {Subject}",
                ticket.NumeroTicket,
                mailInfo.Subject);
        }

        var emailSource = new EmailsSource
        {
            IdTicket = ticket.IdTicket,
            MessageIdGraph = messageId,
            ConversationIdGraph = resolvedConversationId!,
            Expediteur = mailInfo.SenderEmail,
            Objet = mailInfo.Subject,
            CorpsEmail = EmailHelper.GetCleanBody(message),
            DateReception = mailInfo.SentDate.UtcDateTime,
            EstEmailInitial = isFirstEmail
        };

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await _emailSourceRepository.CreateEmailSourceAsync(emailSource, cancellationToken);

            string? fileUrl = null;
            var ticketDir = ticket.IdTicket.ToString();

            var attachmentParts = message.Attachments
              .OfType<MimePart>()
              .Select(part => part).ToList().AsReadOnly();

            foreach (var (attachment, part) in mailInfo.Attachments.Zip(attachmentParts))
            {
                try
                {
                    fileUrl = await _storageService.UploadFromEmailAsync((MimeContent)part.Content, attachment.FileName, attachment.ContentType, ticketDir);
                    var pieceJointe = new PiecesJointe
                    {
                        IdEmailSource = emailSource.IdEmailSource,
                        NomFichier = Path.GetFileName(attachment.FileName),
                        Chemin = fileUrl,
                        TailleOctets = attachment.Size,
                        TypeMime = attachment.ContentType
                    };

                    await _pieceJointeRepository.CreatePieceJointeAsync(pieceJointe, cancellationToken);
                }
                catch (Exception ex) when (fileUrl != null)
                {
                    _logger.LogError(ex,
                        "Échec de l'enregistrement en BDD. " +
                        "Suppression du fichier uploadé : {FileUrl}", fileUrl);

                    await _storageService.DeleteAsync(fileUrl);

                    throw;
                }

            }
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }

        return ticket;
    }

    private async Task<string> GenerateTempTicketNumber()
    {
        var now = DateTime.UtcNow;
        var sequence = await _ticketRepository.GetNextSequenceValueAsync();
        var ticketNumber = $"TICKET{now:yyyy}{now:MM}{sequence:D4}";
        return ticketNumber;
    }

    public async Task<Ticket> CreateAsync(CreateTicketRequest req)
    {
        var ticket = new Ticket
        {
            IdApplication = req.IdApplication,
            IdCriticite = req.IdCriticite,
            IdTechnicienAssigne = req.IdTechnicienAssigne,
            DemandeurEmail = req.DemandeurEmail,
            DemandeurDirection = req.DemandeurDirection,
            DureeSla = req.DureeSla,
            DateCreation = DateTime.UtcNow
        };

        ticket.PasserStatutSuivant((TicketStatus)req.IdStatut);

        return await _ticketRepository.CreateAsync(ticket);
    }

    public async Task<Ticket?> GetByIdAsync(int id)
    {
        return await _context.Tickets.FindAsync(id);
    }

    public async Task<IReadOnlyList<Ticket>> GetAllAsync()
    {
        return await _ticketRepository.GetAllAsync();
    }

    public async Task<IReadOnlyList<Ticket>> GetByTechnicianAsync(Guid technicianUserGuid)
    {
        return await _ticketRepository.GetByTechnicianAsync(technicianUserGuid);
    }

    public async Task<PagedResult<TicketResponse>> GetPagedAsync(TicketSearchRequest criteria)
    {
        return await _ticketRepository.GetPagedAsync(criteria);
    }

    public async Task<IReadOnlyList<Statut>> GetNextStatutsAsync(int idTicket, CancellationToken cancellationToken = default)
    {
        var ticket = await _context.Tickets.FindAsync(idTicket, cancellationToken);
        if (ticket == null)
            return Array.Empty<Statut>();

        var idStatutOrigine = ticket.IdStatut;
        var allowedStatus = TicketStatusTransitions.GetAllowedTransitions((TicketStatus)idStatutOrigine);
        return await _statutRepository.GetNextStatutsAsync(allowedStatus, cancellationToken);
    }

    public async Task<Result> UpdateAsync(int id, UpdateTicketRequest req)
    {
        var ticket = await _context.Tickets.FindAsync(id);
        if (ticket == null)
            return Result.Failure("Ticket not found", ErrorType.NotFound);

        // if (ticket.IdStatut != req.IdStatut)
        // {
        //     var autorisee = await _statutRepository.IsTransitionAutoriseeAsync(ticket.IdStatut, req.IdStatut);
        //     if (!autorisee)
        //         return false;
        // }

        // Passage en "attente client" - démarre une pause
        if (req.IdStatut == (int)TicketStatus.Pending)
        {
            _context.TicketSlaPauses.Add(new TicketSlaPause
            {
                IdTicket = ticket.IdTicket,
                PausedAt = DateTime.UtcNow
            });
        }

        // Sortie de "attente client" - termine la pause active
        if (ticket.IdStatut == (int)TicketStatus.Pending
            && req.IdStatut != (int)TicketStatus.Pending)
        {
            var activePause = await _context.TicketSlaPauses
                .Where(p => p.IdTicket == ticket.IdTicket && p.ResumedAt == null)
                .FirstOrDefaultAsync();

            if (activePause is not null)
            {
                activePause.ResumedAt = DateTime.UtcNow;

                // Décaler les deadlines de la durée de la pause
                var pauseDuration = activePause.ResumedAt.Value - activePause.PausedAt;

                ticket.DeadlineResolution = await _businessTimeCalculator
                    .AddBusinessTimeAsync((DateTime)ticket.DeadlineResolution, (TimeSpan)pauseDuration);

                // ticket.ResolutionDeadline = await _businessTimeCalculator
                //     .AddBusinessTimeAsync(ticket.ResolutionDeadline, pauseDuration);
            }
        }

        var historiqueStatut = new HistoriqueStatut
        {
            IdTicket = ticket.IdTicket,
            IdStatutPrecedent = ticket.IdStatut,
            IdStatutSuivant = req.IdStatut,
            IdAuteur = ticket.IdTechnicienAssigne ?? 0,
            DateHeure = DateTime.UtcNow
        };

        _context.HistoriqueStatuts.Add(historiqueStatut);

        // ticket.IdApplication = req.IdApplication;
        // ticket.IdCriticite = req.IdCriticite;
        var autorisee = ticket.PasserStatutSuivant((TicketStatus)req.IdStatut);
        if (autorisee.IsSuccess != true)
        {
            return autorisee;
        }
        // ticket.IdTechnicienAssigne = req.IdTechnicienAssigne;
        // ticket.DemandeurEmail = req.DemandeurEmail;
        // ticket.DemandeurDirection = req.DemandeurDirection;
        // ticket.DateCloture = req.DateCloture;
        // ticket.DureeSla = req.DureeSla;

        await _context.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> CloseAsync(int id)
    {
        var ticket = await _context.Tickets.FindAsync(id);
        if (ticket == null)
            return Result.Failure("Ticket not found", ErrorType.NotFound);

        ticket.Cloturer();
        await _context.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        await _ticketRepository.DeleteAsync(id);
        return true;
    }

    public async Task<Result> UpdateApplicationAsync(int ticketId, int? idApplication)
    {
        var ticket = await _context.Tickets.FindAsync(ticketId);
        if (ticket == null)
            return Result.Failure("Ticket not found", ErrorType.NotFound);

        if (idApplication.HasValue)
        {
            var app = await _context.Applications
                .Include(a => a.IdCsNavigation)
                .FirstOrDefaultAsync(a => a.IdApplication == idApplication.Value);
            if (app == null)
                return Result.Failure("Application not found", ErrorType.NotFound);

            ticket.IdApplication = idApplication;
            ticket.IdCriticite = app.IdCsNavigation.IdCriticite;
            ticket.DureeSla = app.IdCsNavigation.DureeSla;
            ticket.IdApplicationNavigation = app;
            ticket.DeadlineResolution = await _ticketSlaService.CalculateSlaAsync(ticket);
        }
        else
        {
            ticket.IdApplication = null;
            ticket.IdCriticite = null;
        }


        await _context.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> AssignAsync(IEnumerable<int> ticketIds, Guid? technicianUserGuid, string currentUserEmail)
    {
        var currentUser = await _userAuthenticationService.GetAuthorizedUserAsync(currentUserEmail);
        if (currentUser == null)
            return Result.Failure($"Cannot get the current user {currentUserEmail}", ErrorType.NotFound);

        var role = await _context.Roles.FirstOrDefaultAsync(r => r.IdRole == currentUser.IdRole);
        if (role == null)
            return Result.Failure($"This user {currentUserEmail} doesn't have any role", ErrorType.NotFound);

        var isAdmin = role.Libelle.Equals("Administrateur", StringComparison.OrdinalIgnoreCase);
        var isTechnicien = role.Libelle.Equals("Technicien", StringComparison.OrdinalIgnoreCase);

        if (!isAdmin && (!isTechnicien || technicianUserGuid != currentUser.UserGuid))
            return Result.Failure($"The current user {currentUserEmail} doesn't have rights to do this action", ErrorType.Conflict);

        int? technicienId = null;
        if (technicianUserGuid.HasValue)
        {
            var technicien = await _context.Utilisateurs.AsNoTracking()
                .Where(u => u.UserGuid == technicianUserGuid.Value)
                .Select(u => u.IdUtilisateur)
                .FirstOrDefaultAsync();
            if (technicien == 0)
                return Result.Failure($"Technician with email {currentUserEmail} not found", ErrorType.NotFound);
            technicienId = technicien;
        }

        var alreadyAssignedTickets = await _context.Tickets
            .AsNoTracking()
            .Where(t => ticketIds.Contains(t.IdTicket) && t.IdTechnicienAssigne != null)
            .Select(t => t.NumeroTicket)
            .ToListAsync();

        if (alreadyAssignedTickets.Any())
        {
            return Result.Failure(
                $"Ticket(s) already assigned: {string.Join(", ", alreadyAssignedTickets)}",
                ErrorType.Conflict);
        }

        await _context.Tickets
            .Where(t => ticketIds.Contains(t.IdTicket))
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(b => b.IdTechnicienAssigne, technicienId)
                .SetProperty(b => b.IdStatut, (int)TicketStatus.Opened));

        // await _notificationService.SendAsync(
        //     userId: technicienId ?? 0,
        //     idTicket: 0,
        //     title: "Ticket assigné",
        //     message: $"Des ticket vous ont été assigné.",
        //     eventType: "Assignation ticket"
        //     );

        return Result.Success();
    }

    public async Task<Result> ReassignAsync(IEnumerable<int> ticketIds, Guid? technicianUserGuid, string justification)
    {
        var tickets = await _context.Tickets
        .Where(x => ticketIds.Contains(x.IdTicket))
        .ToListAsync();

        var missingIds = ticketIds
        .Except(tickets.Select(x => x.IdTicket))
        .ToList();

        if (missingIds.Any())
            return Result.Failure($"Tickets not found : {string.Join(", ", missingIds)}", ErrorType.NotFound);

        int? technicienId = null;
        if (technicianUserGuid.HasValue)
        {
            var technicien = await _context.Utilisateurs.AsNoTracking()
                .Where(u => u.UserGuid == technicianUserGuid.Value)
                .Select(u => u.IdUtilisateur)
                .FirstOrDefaultAsync();
            if (technicien == 0)
                return Result.Failure($"Technician with guid {technicianUserGuid} not found", ErrorType.NotFound);
            technicienId = technicien;
        }

        var reassignations = new List<Reassignation>();

        foreach (var ticket in tickets)
        {
            var result = ticket.ReassignTo(technicienId, justification);
            if (!result.IsSuccess)
            {
                return result;
            }
            reassignations.Add(new Reassignation
            {
                IdTicket = ticket.IdTicket,
                IdAncienAssigne = ticket.IdTechnicienAssigne,
                IdNouvelAssigne = (int)technicienId,
                Motif = justification,
                IdAuteur = 0,
                DateReassignation = DateTime.UtcNow
            });
        }

        await _context.Reassignations.AddRangeAsync(reassignations);

        await _context.SaveChangesAsync();
        foreach (var ticket in tickets)
        {
            await _eventDispatcher.DispatchAsync(ticket.DomainEvents);
            ticket.ClearDomainEvents();
        }
        return Result.Success();
    }

    public async Task<IReadOnlyList<Statut>> GetListStatus()
    {
        return _context.Statuts.AsNoTracking().ToList();
    }
}
