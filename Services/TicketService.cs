using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SIGRA.Data.Models;
using SIGRA.Data.Repositories;
using SIGRA.Services;

namespace SIGRA.Services;

public class TicketService
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IConfiguration _config;
    private readonly ILogger<TicketService> _logger;

    public TicketService(
        ITicketRepository ticketRepository,
        IConfiguration config,
        ILogger<TicketService> logger)
    {
        _ticketRepository = ticketRepository;
        _config = config;
        _logger = logger;
    }

    public async Task<Ticket> CreateTicketFromEmailAsync(
        ImapMailService.MailInfo mailInfo,
        string messageId,
        string? conversationId = null,
        CancellationToken cancellationToken = default)
    {
        var defaultAppId = _config.GetValue<int?>("TicketDefaults:ApplicationId");
        if (!defaultAppId.HasValue)
        {
            defaultAppId = await _ticketRepository.GetFirstActiveApplicationIdAsync(cancellationToken);
            if (!defaultAppId.HasValue)
                throw new InvalidOperationException("No active application configured for ticket creation.");
        }

        var typeDemandeId = _config.GetValue<int?>("TicketDefaults:TypeDemandeId");
        if (!typeDemandeId.HasValue)
        {
            typeDemandeId = await _ticketRepository.GetTypeDemandeIdAsync("Demande", cancellationToken);
            if (!typeDemandeId.HasValue)
                throw new InvalidOperationException("Default TypeDemande 'Demande' not found.");
        }

        var criticiteId = _config.GetValue<int?>("TicketDefaults:CriticiteId");
        if (!criticiteId.HasValue)
        {
            criticiteId = await _ticketRepository.GetCriticiteIdAsync("Normale", cancellationToken);
            if (!criticiteId.HasValue)
                throw new InvalidOperationException("Default Criticite 'Normale' not found.");
        }

        var statutId = _config.GetValue<int?>("TicketDefaults:StatutId");
        if (!statutId.HasValue)
        {
            statutId = await _ticketRepository.GetStatutIdAsync("Nouveau", cancellationToken);
            if (!statutId.HasValue)
                throw new InvalidOperationException("Default Statut 'Nouveau' not found.");
        }

        var ticket = new Ticket
        {
            NumeroTicket = GenerateTempTicketNumber(),
            IdApplication = defaultAppId.Value,
            IdTypeDemande = typeDemandeId.Value,
            IdCriticite = criticiteId.Value,
            IdStatut = statutId.Value,
            DemandeurEmail = mailInfo.SenderEmail,
            DemandeurDirection = string.IsNullOrWhiteSpace(mailInfo.Sender) ? mailInfo.SenderEmail : mailInfo.Sender,
            DateCreation = mailInfo.SentDate.UtcDateTime,
            DureeSla = 0
        };

        await _ticketRepository.CreateAsync(ticket, cancellationToken);

        ticket.NumeroTicket = $"TKT-{ticket.IdTicket:D6}";
        await _ticketRepository.UpdateAsync(ticket, cancellationToken);

        _logger.LogInformation(
            "Ticket {TicketNumber} created from email: {Subject}",
            ticket.NumeroTicket,
            mailInfo.Subject);

        var emailSource = new EmailSource
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

        await _ticketRepository.CreateEmailSourceAsync(emailSource, cancellationToken);

        var attachmentsRoot = _config["Ticket:AttachmentsPath"] ?? "data/attachments";
        var ticketDir = Path.Combine(attachmentsRoot, ticket.IdTicket.ToString());
        Directory.CreateDirectory(ticketDir);

        foreach (var attachment in mailInfo.Attachments)
        {
            var safeFileName = Path.GetFileName(attachment.FileName);
            var relativePath = Path.Combine(ticket.IdTicket.ToString(), safeFileName);

            var pieceJointe = new PieceJointe
            {
                IdEmailSource = emailSource.IdEmailSource,
                NomFichier = safeFileName,
                Chemin = relativePath,
                TailleOctets = attachment.Size,
                TypeMime = attachment.ContentType
            };

            await _ticketRepository.CreatePieceJointeAsync(pieceJointe, cancellationToken);
        }

        return ticket;
    }

    private static string GenerateTempTicketNumber()
    {
        return $"TKT-{Guid.NewGuid():N}"[..30];
    }
}
