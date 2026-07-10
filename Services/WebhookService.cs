using Microsoft.Graph.Models;

namespace SIGRA.Services;

public class WebhookService
{
    private readonly GraphClientFactory _graphClientFactory;
    private readonly IConfiguration _config;
    private readonly ILogger<WebhookService> _logger;

    private string _subscriptionId = null;

    public WebhookService(
        GraphClientFactory graphClientFactory,
        IConfiguration config,
        ILogger<WebhookService> logger)
    {
        _graphClientFactory = graphClientFactory;
        _config = config;
        _logger = logger;
    }

    public async Task CreateOrRenewSubscriptionAsync()
    {
        var graphClient = _graphClientFactory.CreateClient();

        var subscription = new Subscription
        {
            ChangeType = "created,updated",
            NotificationUrl = _config["Webhook:NotificationUrl"],
            Resource = "me/mailFolders/Inbox/messages",
            ExpirationDateTime = DateTimeOffset.UtcNow.AddMinutes(
                int.Parse(_config["Webhook:ExpirationMinutes"])),
            ClientState = "SecretClientState123" // Validate in webhook handler
        };

        if (_subscriptionId == null)
        {
            // Create new subscription
            var result = await graphClient.Subscriptions.PostAsync(subscription);
            _subscriptionId = result.Id;
            _logger.LogInformation("Webhook subscription created: {Id}", _subscriptionId);
        }
        else
        {
            // Renew existing subscription
            await graphClient.Subscriptions[_subscriptionId].PatchAsync(new Subscription
            {
                ExpirationDateTime = subscription.ExpirationDateTime
            });
            _logger.LogInformation("Webhook subscription renewed: {Id}", _subscriptionId);
        }
    }

    public async Task DeleteSubscriptionAsync()
    {
        if (_subscriptionId == null) return;

        var graphClient = _graphClientFactory.CreateClient();
        await graphClient.Subscriptions[_subscriptionId].DeleteAsync();
        _subscriptionId = null;
        _logger.LogInformation("Webhook subscription deleted.");
    }
}
