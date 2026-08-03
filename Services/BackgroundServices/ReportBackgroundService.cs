using Microsoft.EntityFrameworkCore;
using SIGRA.Data;
using SIGRA.Data.Models;

namespace SIGRA.Services;

public class ReportBackgroundService : BackgroundService
{
    private readonly ILogger<ReportBackgroundService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public ReportBackgroundService(
        ILogger<ReportBackgroundService> logger,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Vérifie immédiatement au démarrage si un envoi a été manqué
        await TryGenerateAndSendReportAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = GetDelayUntilNextMonday8AM();

            _logger.LogInformation(
                "Prochain rapport hebdomadaire prévu dans {delay}.", delay);

            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }

            await TryGenerateAndSendReportAsync(stoppingToken);
        }
    }

    private async Task TryGenerateAndSendReportAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var reportBuilder = scope.ServiceProvider.GetRequiredService<IWeeklyReportBuilder>();
        var pdfGenerator = scope.ServiceProvider.GetRequiredService<IPdfReportGenerator>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

        var weekStart = GetCurrentOrLastMonday();
        var weekEnd = weekStart.AddDays(7);

        // Protection anti-doublon — même avec plusieurs instances
        var rapport = new Rapport
        {
            DateDebutSemaine = weekStart,
            TypeRapport = "Hebdomadaire",
            DateEnvoie = DateTime.UtcNow
        };

        db.Rapports.Add(rapport);

        try
        {
            // Tentative d'insertion — échoue si déjà envoyé (contrainte unique)
            await db.SaveChangesAsync(stoppingToken);
        }
        catch (DbUpdateException)
        {
            _logger.LogInformation(
                "Rapport déjà envoyé pour la semaine du {weekStart}, on ignore.",
                weekStart);
            return;
        }

        try
        {
            _logger.LogInformation("Génération du rapport hebdomadaire...");

            var report = await reportBuilder.BuildAsync(weekStart, weekEnd);
            var pdfContent = pdfGenerator.GenerateWeeklyReport(report);

            await emailService.SendWeeklyReportAsync(pdfContent, report);

            _logger.LogInformation("Rapport hebdomadaire envoyé avec succès.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Erreur lors de la génération/envoi du rapport hebdomadaire.");

            // On retire l'entrée pour permettre un nouvel essai
            // (au prochain redémarrage ou déclenchement manuel)
            db.Rapports.Remove(rapport);
            await db.SaveChangesAsync(stoppingToken);
        }
    }

    private static TimeSpan GetDelayUntilNextMonday8AM()
    {
        var now = DateTime.Now;
        var daysUntilMonday = ((int)DayOfWeek.Monday - (int)now.DayOfWeek + 7) % 7;
        var nextRun = now.Date.AddDays(daysUntilMonday).AddHours(8);

        if (nextRun <= now)
            nextRun = nextRun.AddDays(7);

        return nextRun - now;
    }

    private static DateTime GetCurrentOrLastMonday()
    {
        var now = DateTime.UtcNow;
        var diff = (7 + (now.DayOfWeek - DayOfWeek.Monday)) % 7;
        return now.Date.AddDays(-diff);
    }
}
