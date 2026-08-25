namespace SIGRA.Services;

public interface IWeeklyReportBuilder
{
    Task<WeeklyReportDto> BuildAsync(DateTime weekStart, DateTime weekEnd);
}

public class WeeklyReportBuilder : IWeeklyReportBuilder
{
    private readonly IReportService _reportService;

    public WeeklyReportBuilder(IReportService reportService)
    {
        _reportService = reportService;
    }

    public async Task<WeeklyReportDto> BuildAsync(DateTime weekStart, DateTime weekEnd)
    {
        var weeklyRequests = await _reportService.GetWeeklyRequestsAsync(weekStart, weekEnd);
        var byApplication = await _reportService.GetRequestsByApplicationAsync(weekStart, weekEnd);
        var slaCompliance = await _reportService.GetSlaComplianceAsync(weekStart, weekEnd);
        var meanResolutionTime = await _reportService.GetMeanResolutionTimeAsync(weekStart, weekEnd);
        var lastTwoWeeks = await _reportService.GetLastTwoWeeksAsync();

        return new WeeklyReportDto
        {
            WeekStart = weekStart,
            WeekEnd = weekEnd,
            WeeklyRequests = weeklyRequests,
            RequestsByApplication = byApplication,
            SlaCompliance = slaCompliance,
            MeanResolutionTime = meanResolutionTime,
            LastTwoWeeks = lastTwoWeeks
        };
    }
}
