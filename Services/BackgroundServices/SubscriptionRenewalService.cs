namespace SIGRA.Services;

public class SubscriptionRenewalService : BackgroundService
{
    private readonly WebhookService _webhookService;
    private readonly ILogger<SubscriptionRenewalService> _logger;

    public SubscriptionRenewalService(
        WebhookService webhookService,
        ILogger<SubscriptionRenewalService> logger)
    {
        _webhookService = webhookService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Renew subscription every 60 minutes
                await Task.Delay(TimeSpan.FromMinutes(60), stoppingToken);
                await _webhookService.CreateOrRenewSubscriptionAsync();
                _logger.LogInformation("Subscription renewed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error renewing subscription.");
            }
        }
    }
}