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

            var uids = await folder.SearchAsync(SearchQuery.All, cancellationToken);

            var newMessages = new List<MimeMessage>();
            ulong cycleMaxUid = 0;

            foreach (var uid in uids)
            {
                try
                {
                    var message = await folder.GetMessageAsync(uid, cancellationToken);
                    newMessages.Add(message);

                    cycleMaxUid = Math.Max(cycleMaxUid, uid.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to fetch message with Uid {Uid}", uid.Id);
                }
            }

            if (cycleMaxUid > 0)
                _seenTracker.MarkSeen(new[] { cycleMaxUid });

            foreach (var message in newMessages)
            {
                await ProcessMessageAsync(message);
            }

            _logger.LogInformation("IMAP sync completed. {Count} new messages found.", newMessages.Count);
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
