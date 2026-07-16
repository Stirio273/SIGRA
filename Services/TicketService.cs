using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SIGRA.Data.Models;
using SIGRA.Data.Repositories;

namespace SIGRA.Services;

public class TicketService : ITicketService
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IStatutRepository _statutRepository;
    private readonly IEmailsSourceRepository _emailSourceRepository;
    private readonly IPiecesJointeRepository _pieceJointeRepository;
    private readonly IConfiguration _config;
    private readonly ILogger<TicketService> _logger;

    public TicketService(
        ITicketRepository ticketRepository,
        IStatutRepository statutRepository,
        IConfiguration config,
        ILogger<TicketService> logger)
    {
        _ticketRepository = ticketRepository;
        _statutRepository = statutRepository;
        _config = config;
        _logger = logger;
    }

    public async Task<Ticket> CreateTicketFromEmailAsync(
        ImapMailService.MailInfo mailInfo,
        string messageId,
        string? conversationId = null,
        CancellationToken cancellationToken = default)
    {
        // var defaultAppId = _config.GetValue<int?>("TicketDefaults:ApplicationId");
        // if (!defaultAppId.HasValue)
        // {
        //     defaultAppId = await _ticketRepository.GetFirstActiveApplicationIdAsync(cancellationToken);
        //     if (!defaultAppId.HasValue)
        //         throw new InvalidOperationException("No active application configured for ticket creation.");
        // }

        // var typeDemandeId = _config.GetValue<int?>("TicketDefaults:TypeDemandeId");
        // if (!typeDemandeId.HasValue)
        // {
        //     typeDemandeId = await _ticketRepository.GetTypeDemandeIdAsync("Demande", cancellationToken);
        //     if (!typeDemandeId.HasValue)
        //         throw new InvalidOperationException("Default TypeDemande 'Demande' not found.");
        // }

        // var criticiteId = _config.GetValue<int?>("TicketDefaults:CriticiteId");
        // if (!criticiteId.HasValue)
        // {
        //     criticiteId = await _ticketRepository.GetCriticiteIdAsync("Normale", cancellationToken);
        //     if (!criticiteId.HasValue)
        //         throw new InvalidOperationException("Default Criticite 'Normale' not found.");
        // }

        var statutId = _config.GetValue<int?>("TicketDefaults:StatutId");
        if (!statutId.HasValue)
        {
            statutId = await _statutRepository.GetIdStatutByDefaultAsync(cancellationToken);
            if (!statutId.HasValue)
                throw new InvalidOperationException("Default Statut not found.");
        }

        var ticket = new Ticket
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

        var emailSource = new EmailsSource
        {
            IdTicket = ticket.IdTicket,
            MessageIdGraph = messageId,
            ConversationIdGraph = conversationId ?? Guid.NewGuid().ToString(),
            Expediteur = mailInfo.SenderEmail,
            Objet = mailInfo.Subject,
            CorpsEmail = mailInfo.Body,
            DateReception = mailInfo.SentDate.UtcDateTime,
            EstEmailInitial = true
        };

        await _emailSourceRepository.CreateEmailSourceAsync(emailSource, cancellationToken);

        var attachmentsRoot = _config["Ticket:AttachmentsPath"] ?? "data/attachments";
        var ticketDir = Path.Combine(attachmentsRoot, ticket.IdTicket.ToString());
        Directory.CreateDirectory(ticketDir);

        foreach (var attachment in mailInfo.Attachments)
        {
            var safeFileName = Path.GetFileName(attachment.FileName);
            var relativePath = Path.Combine(ticket.IdTicket.ToString(), safeFileName);

            var pieceJointe = new PiecesJointe
            {
                IdEmailSource = emailSource.IdEmailSource,
                NomFichier = safeFileName,
                Chemin = relativePath,
                TailleOctets = attachment.Size,
                TypeMime = attachment.ContentType
            };

            await _pieceJointeRepository.CreatePieceJointeAsync(pieceJointe, cancellationToken);
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
}
