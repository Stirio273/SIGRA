using System.Text.Json;

namespace SIGRA.Views;

public class WeeklyReportViewModelMapper
{
    public WeeklyReportViewModel ToViewModel(WeeklyReportDto data)
    {
        var slaTotal = data.SlaRespected + data.SlaBreached;
        var slaPercent = slaTotal > 0
            ? Math.Round(data.SlaRespected * 100.0 / slaTotal, 1)
            : 0;

        var chartData = new
        {
            ticketsByPriority = data.TicketsByPriority.Select(p => new { label = p.Priority, value = p.Count }),
            dailyVolume = data.DailyCreatedVolume.Select(d => new { label = d.Date.ToString("dd/MM"), value = d.Count }),
            slaBreakdown = new[]
            {
                new { label = "SLA respecté", value = data.SlaRespected },
                new { label = "SLA violé", value = data.SlaBreached }
            }
        };

        return new WeeklyReportViewModel
        {
            WeekStartFormatted = data.WeekStart.ToString("dd/MM/yyyy"),
            WeekEndFormatted = data.WeekEnd.ToString("dd/MM/yyyy"),
            TotalTicketsCreated = data.TotalTicketsCreated,
            TotalTicketsClosed = data.TotalTicketsClosed,
            SlaRespectedPercent = slaPercent,
            ChartDataJson = JsonSerializer.Serialize(chartData)
            // Sérialisation JSON valide garantie — pas de risque
            // d'échappement cassé, contrairement à un remplacement de chaîne brut
        };
    }
}
