using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;

namespace SIGRA.Services;

public class ImapPollingService : BackgroundService
{
    private readonly ImapSyncService _imapSyncService;
    private readonly ILogger<ImapPollingService> _logger;
    private readonly IConfiguration _config;

    public ImapPollingService(
        ImapSyncService imapSyncService,
        IConfiguration config,
        ILogger<ImapPollingService> logger)
    {
        _imapSyncService = imapSyncService;
        _config = config;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var intervalMinutes = _config.GetValue<int?>("Imap:PollingIntervalMinutes") ?? 5;

        _logger.LogInformation("IMAP polling service started. Interval: {Interval} minutes.", intervalMinutes);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _imapSyncService.SyncMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during IMAP polling");
            }

            try
            {
                await Task.Delay(TimeSpan.FromMinutes(intervalMinutes), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }
}
