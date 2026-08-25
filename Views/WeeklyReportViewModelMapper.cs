using System.Text.Json;

namespace SIGRA.Views;

public class WeeklyReportViewModelMapper
{
    public WeeklyReportViewModel ToViewModel(WeeklyReportDto data)
    {
        var slaTotal = data.SlaCompliance.Entries.Sum(e => e.TotalCount);
        var slaCompliant = data.SlaCompliance.Entries.Sum(e => e.CompliantCount);
        var slaPercent = slaTotal > 0
            ? Math.Round(slaCompliant * 100.0 / slaTotal, 1)
            : 0;

        var chartData = new
        {
            ticketsByPriority = data.RequestsByApplication.Entries.Select(a => new { label = a.ApplicationName, value = a.Count }),
            dailyVolume = data.WeeklyRequests.Entries.Select(w => new { label = w.WeekStart.ToString("dd/MM"), value = w.Count }),
            slaBreakdown = new[]
            {
                new { label = "SLA respecté", value = slaCompliant },
                new { label = "SLA violé", value = slaTotal - slaCompliant }
            }
        };

        var topApp = data.RequestsByApplication.Entries.OrderByDescending(a => a.Count).FirstOrDefault();

        LastTwoWeeksEntryDto? lastWeek = null;
        LastTwoWeeksEntryDto? previousWeek = null;
        if (data.LastTwoWeeks.Entries.Count >= 2)
        {
            lastWeek = data.LastTwoWeeks.Entries[^1];
            previousWeek = data.LastTwoWeeks.Entries[^2];
        }

        return new WeeklyReportViewModel
        {
            WeekStartFormatted = data.WeekStart.ToString("dd/MM/yyyy"),
            WeekEndFormatted = data.WeekEnd.ToString("dd/MM/yyyy"),
            TotalTicketsCreated = data.WeeklyRequests.Total,
            TotalTicketsClosed = data.WeeklyRequests.Total,
            SlaRespectedPercent = slaPercent,
            MeanResolutionTimeHours = data.MeanResolutionTime.MeanTime,
            ChartDataJson = JsonSerializer.Serialize(chartData),
            ApplicationCount = data.RequestsByApplication.Entries.Count,
            TopApplication = topApp?.ApplicationName ?? string.Empty,
            TopApplicationCount = topApp?.Count ?? 0,
            LastWeekSlaRate = lastWeek?.SlaRate,
            LastWeekTicketCount = lastWeek?.Count,
            PreviousWeekSlaRate = previousWeek?.SlaRate,
            PreviousWeekTicketCount = previousWeek?.Count,
            SlaRateEvolution = data.LastTwoWeeks.SlaRateEvolution,
            TicketCountEvolution = data.LastTwoWeeks.TicketCountEvolution
        };
    }
}
