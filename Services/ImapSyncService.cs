using MailKit;
using MailKit.Search;
using MimeKit;

namespace SIGRA.Services;

public class ImapSyncService
{
    private readonly ImapMailService _imapMailService;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ImapSeenTracker _seenTracker;
    private readonly ImapFailedUidsTracker _failedUidsTracker;
    private readonly ILogger<ImapSyncService> _logger;

    public ImapSyncService(
        ImapMailService imapMailService,
        IServiceScopeFactory scopeFactory,
        IConfiguration config,
        ILogger<ImapSyncService> logger)
    {
        _imapMailService = imapMailService;
        _scopeFactory = scopeFactory;
        _logger = logger;

        var storagePath = config["Imap:SeenUidsStoragePath"] ?? "data/imap-seen-uids.txt";
        _seenTracker = new ImapSeenTracker(storagePath);

        var failedUidsPath = config["Imap:FailedUidsStoragePath"] ?? "data/imap-failed-uids.txt";
        _failedUidsTracker = new ImapFailedUidsTracker(failedUidsPath);
    }

    public async Task SyncMessagesAsync(CancellationToken cancellationToken = default)
    {
        using var client = await _imapMailService.ConnectAsync(cancellationToken);

        try
        {
            var folderName = _imapMailService.GetFolderName();
            var folder = client.GetFolder(folderName);

            if (folder == null)
            {
                _logger.LogError("Folder '{Folder}' not found on server.", folderName);
                return;
            }

            if (!folder.IsOpen)
            {
                await folder.OpenAsync(FolderAccess.ReadOnly, cancellationToken);
            }

            var allUids = await folder.SearchAsync(SearchQuery.All, cancellationToken);

            if (allUids.Count == 0)
            {
                _logger.LogInformation("IMAP sync completed. No messages found.");
                return;
            }

            var lastSeenUid = _seenTracker.LastSeenUid;

            if (lastSeenUid == 0)
            {
                _logger.LogInformation("First sync detected. Processing all {Count} messages...", allUids.Count);
            }
            else
            {
                _logger.LogInformation(
                    "Filtering {Total} messages. Last seen Uid: {LastSeenUid}",
                    allUids.Count,
                    lastSeenUid);
            }

            var newUids = _seenTracker.FilterSeen(allUids.Select(u => (ulong)u.Id)).ToList();

            if (newUids.Count == 0)
            {
                _logger.LogInformation("IMAP sync completed. No new messages found.");
            }

            if (newUids.Count == 0)
            {
                return;
            }

            _logger.LogInformation("Found {Count} message(s) to process.", newUids.Count);

            var successfullyProcessedUids = new List<ulong>();
            var permanentlyFailedUids = new List<ulong>();

            foreach (var uid in newUids)
            {
                try
                {
                    var uniqueId = new UniqueId((uint)uid);
                    var message = await folder.GetMessageAsync(uniqueId, cancellationToken);
                    await ProcessMessageAsync(message);
                    successfullyProcessedUids.Add(uid);
                    _failedUidsTracker.RecordSuccess(uid);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        ex,
                        "Failed to process message with Uid {Uid}. Recording for retry on next sync.", 
                        uid);
                    _failedUidsTracker.RecordFailure(uid);

                    if (!_failedUidsTracker.IsRetriable(uid))
                    {
                        _logger.LogError("Message with Uid {Uid} exceeded maximum retry attempts. Marking as seen.", uid);
                        permanentlyFailedUids.Add(uid);
                    }
                }
            }

            if (successfullyProcessedUids.Count > 0)
            {
                _seenTracker.MarkSeen(successfullyProcessedUids);
            }

            if (permanentlyFailedUids.Count > 0)
            {
                _seenTracker.MarkSeen(permanentlyFailedUids);
                foreach (var uid in permanentlyFailedUids)
                {
                    _failedUidsTracker.Remove(uid);
                }
            }

            _logger.LogInformation(
                "IMAP sync completed. {Processed} of {Total} new message(s) processed successfully.",
                successfullyProcessedUids.Count,
                newUids.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during IMAP sync");
            throw;
        }
        finally
        {
            await client.DisconnectAsync(true);
        }
    }

    private async Task ProcessMessageAsync(MimeMessage message)
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

        try
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var ticketService = scope.ServiceProvider.GetRequiredService<ITicketService>();
                var ticket = await ticketService.CreateTicketFromEmailAsync(
                    mailInfo,
                    messageId,
                    inReplyTo,
                    references,
                    cancellationToken: default);

                if (ticket != null)
                {
                    _logger.LogInformation(
                        "Ticket {TicketNumber} created for message: {Subject}",
                        ticket.NumeroTicket,
                        mailInfo.Subject);
                }
                else
                {
                    _logger.LogInformation(
                        "Email {MessageId} already processed. Skipping ticket creation.",
                        messageId);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to create ticket for message with MessageId {MessageId}: {Subject}",
                messageId,
                mailInfo.Subject);
            throw;
        }
    }
}
