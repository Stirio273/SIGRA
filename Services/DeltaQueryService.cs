using Microsoft.Graph;
using Microsoft.Graph.Admin.Exchange.Mailboxes.Item.Folders.Delta;
using Microsoft.Graph.Models;
using Microsoft.Kiota.Abstractions;

namespace SIGRA.Services;

public class DeltaQueryService
{
    private readonly GraphClientFactory _graphClientFactory;
    private readonly ILogger<DeltaQueryService> _logger;

    // Store delta link (use DB/cache in production)
    private string _deltaLink = null;

    public DeltaQueryService(
        GraphClientFactory graphClientFactory,
        ILogger<DeltaQueryService> logger)
    {
        _graphClientFactory = graphClientFactory;
        _logger = logger;
    }

    public async Task SyncMessagesAsync()
    {
        var graphClient = _graphClientFactory.CreateClient();

        try
        {
            List<Message> changedMessages = new();

            if (_deltaLink == null)
            {
                // First sync: get all messages + delta link
                _logger.LogInformation("Starting initial full sync...");
                await DoInitialSyncAsync(graphClient, changedMessages);
            }
            else
            {
                // Subsequent syncs: get only changes
                _logger.LogInformation("Starting incremental delta sync...");
                await DoIncrementalSyncAsync(graphClient, changedMessages);
            }

            // Process changed messages
            foreach (var message in changedMessages)
            {
                _logger.LogInformation(
                    "Changed message: {Subject} from {Sender}",
                    message.Subject,
                    message.From?.EmailAddress?.Address);

                // 👇 Add your business logic here
                await ProcessMessageAsync(message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during delta sync");
            throw;
        }
    }

    private async Task DoInitialSyncAsync(
        GraphServiceClient graphClient,
        List<Message> messages)
    {
        var response = await graphClient.Me
            .MailFolders["Inbox"]
            .Messages
            .Delta
            .GetAsDeltaGetResponseAsync(config =>
            {
                config.QueryParameters.Select =
                    new[] { "subject", "from", "receivedDateTime", "isRead", "body" };
                config.QueryParameters.Top = 50;
            });

        await ProcessDeltaResponseAsync(response, messages);
    }

    private async Task DoIncrementalSyncAsync(
        GraphServiceClient graphClient,
        List<Message> messages)
    {
        // Use stored delta link to get only changes
        var requestInfo = new RequestInformation
        {
            HttpMethod = Method.GET,
            UrlTemplate = _deltaLink
        };

        var response = await graphClient.Me
            .MailFolders["Inbox"]
            .Messages
            .Delta
            .GetAsDeltaGetResponseAsync();

        await ProcessDeltaResponseAsync(response, messages);
    }

    private async Task ProcessDeltaResponseAsync(
        dynamic response,
        List<Message> messages)
    {
        var pageIterator = PageIterator<Message,
            DeltaGetResponse>
            .CreatePageIterator(
                _graphClientFactory.CreateClient(),
                (DeltaGetResponse)response,
                (message) =>
                {
                    messages.Add(message);
                    return true;
                },
                (req) =>
                {
                    // Capture delta link from last page
                    return req;
                });

        await pageIterator.IterateAsync();

        // Store the delta link for next sync
        if (response.OdataDeltaLink != null)
            _deltaLink = response.OdataDeltaLink;
    }

    private async Task ProcessMessageAsync(Message message)
    {
        // 👇 Implement your business logic
        // e.g., save to DB, trigger workflow, send notification
        await Task.CompletedTask;
    }
}
