using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using SIGRA.Controllers;
using SIGRA.Data;
using SIGRA.Data.Models;
using SIGRA.Data.Repositories;

namespace SIGRA.Services;

public class TicketService : ITicketService
{
    private readonly AppDbContext _context;
    private readonly ITicketRepository _ticketRepository;
    private readonly IStatutRepository _statutRepository;
    private readonly IEmailsSourceRepository _emailSourceRepository;
    private readonly IPiecesJointeRepository _pieceJointeRepository;
    private readonly IStorageService _storageService;
    private readonly IConfiguration _config;
    private readonly ILogger<TicketService> _logger;
    private readonly IUserAuthenticationService _userAuthenticationService;

    public TicketService(
        AppDbContext context,
        ITicketRepository ticketRepository,
        IStatutRepository statutRepository,
        IEmailsSourceRepository emailSourceRepository,
        IPiecesJointeRepository pieceJointeRepository,
        IStorageService storageService,
        IConfiguration config,
        ILogger<TicketService> logger,
        IUserAuthenticationService userAuthenticationService)
    {
        _context = context;
        _ticketRepository = ticketRepository;
        _statutRepository = statutRepository;
        _emailSourceRepository = emailSourceRepository;
        _pieceJointeRepository = pieceJointeRepository;
        _storageService = storageService;
        _config = config;
        _logger = logger;
        _userAuthenticationService = userAuthenticationService;
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
            ticket = await _ticketRepository.GetByIdAsync(resolvedTicketId.Value, cancellationToken)
                ?? throw new InvalidOperationException($"Ticket {resolvedTicketId.Value} not found for conversation.");
        }
        else
        {
            isFirstEmail = true;
            resolvedConversationId = conversationId ?? Guid.NewGuid().ToString();

            var statutId = _config.GetValue<int?>("TicketDefaults:StatutId");
            if (!statutId.HasValue)
            {
                statutId = await _statutRepository.GetIdStatutByDefaultAsync(cancellationToken);
                if (!statutId.HasValue)
                    throw new InvalidOperationException("Default Statut not found.");
            }

            ticket = new Ticket
            {
                NumeroTicket = await GenerateTempTicketNumber(),
                IdStatut = statutId.Value,
                DemandeurEmail = mailInfo.SenderEmail,
                DemandeurDirection = string.IsNullOrWhiteSpace(mailInfo.Sender) ? mailInfo.SenderEmail : mailInfo.Sender,
                DateCreation = mailInfo.SentDate.UtcDateTime,
                DureeSla = 0
            };

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
            CorpsEmail = mailInfo.Body,
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
            IdStatut = req.IdStatut,
            IdTechnicienAssigne = req.IdTechnicienAssigne,
            DemandeurEmail = req.DemandeurEmail,
            DemandeurDirection = req.DemandeurDirection,
            DureeSla = req.DureeSla,
            DateCreation = DateTime.UtcNow
        };

        return await _ticketRepository.CreateAsync(ticket);
    }

    public async Task<Ticket?> GetByIdAsync(int id)
    {
        return await _ticketRepository.GetByIdAsync(id);
    }

    public async Task<IReadOnlyList<Ticket>> GetAllAsync()
    {
        return await _ticketRepository.GetAllAsync();
    }

    public async Task<IReadOnlyList<Ticket>> GetByTechnicianAsync(Guid technicianUserGuid)
    {
        return await _ticketRepository.GetByTechnicianAsync(technicianUserGuid);
    }

    public async Task<PagedResult<Ticket>> GetPagedAsync(int pageNumber, int pageSize)
    {
        return await _ticketRepository.GetPagedAsync(pageNumber, pageSize);
    }

    public async Task<bool> UpdateAsync(int id, UpdateTicketRequest req)
    {
        var ticket = await _ticketRepository.GetByIdAsync(id);
        if (ticket == null)
            return false;

        ticket.IdApplication = req.IdApplication;
        ticket.IdCriticite = req.IdCriticite;
        ticket.IdStatut = req.IdStatut;
        ticket.IdTechnicienAssigne = req.IdTechnicienAssigne;
        ticket.DemandeurEmail = req.DemandeurEmail;
        ticket.DemandeurDirection = req.DemandeurDirection;
        ticket.DateCloture = req.DateCloture;
        ticket.DureeSla = req.DureeSla;

        await _ticketRepository.UpdateAsync(ticket);
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        await _ticketRepository.DeleteAsync(id);
        return true;
    }

    public async Task<bool> AssignAsync(int ticketId, int? technicianId, string currentUserEmail)
    {
        var ticket = await _ticketRepository.GetByIdAsync(ticketId);
        if (ticket == null)
            return false;

        var currentUser = await _userAuthenticationService.GetAuthorizedUserAsync(currentUserEmail);
        if (currentUser == null)
            return false;

        var isAdmin = currentUser.IdRoleNavigation.Libelle.Equals("Administrateur", StringComparison.OrdinalIgnoreCase);
        var isTechnicien = currentUser.IdRoleNavigation.Libelle.Equals("Technicien", StringComparison.OrdinalIgnoreCase);

        if (!isAdmin && (!isTechnicien || technicianId != currentUser.IdUtilisateur))
            return false;

        ticket.IdTechnicienAssigne = technicianId;
        await _ticketRepository.UpdateAsync(ticket);
        return true;
    }
}
