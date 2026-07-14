using MailKit;
using MailKit.Search;
using MimeKit;

namespace SIGRA.Services;

public class ImapSyncService
{
    private readonly ImapMailService _imapMailService;
    private readonly ImapSeenTracker _seenTracker;
    private readonly ILogger<ImapSyncService> _logger;

    public ImapSyncService(
        ImapMailService imapMailService,
        IConfiguration config,
        ILogger<ImapSyncService> logger)
    {
        _imapMailService = imapMailService;
        _logger = logger;

        var storagePath = config["Imap:SeenUidsStoragePath"] ?? "data/imap-seen-uids.txt";
        _seenTracker = new ImapSeenTracker(storagePath);
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
                return;
            }

            _logger.LogInformation("Found {Count} new message(s). Fetching content...", newUids.Count);

            var successfullyProcessedUids = new List<ulong>();

            foreach (var uid in newUids)
            {
                try
                {
                    var uniqueId = new UniqueId((uint)uid);
                    var message = await folder.GetMessageAsync(uniqueId, cancellationToken);
                    await ProcessMessageAsync(message);
                    successfullyProcessedUids.Add(uid);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        ex,
                        "Failed to fetch/process message with Uid {Uid}. Will retry on next sync.",
                        uid);
                }
            }

            if (successfullyProcessedUids.Count > 0)
            {
                _seenTracker.MarkSeen(successfullyProcessedUids);
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

        await Task.CompletedTask;
    }
}
