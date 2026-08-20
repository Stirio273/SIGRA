using SIGRA.Data.Repositories;

namespace SIGRA.Services;

public class ReportService : IReportService
{
    private readonly IReportRepository _reportRepository;
    private readonly ILogger<ReportService> _logger;

    public ReportService(
       IReportRepository reportRepository, ILogger<ReportService> logger)
    {
        _reportRepository = reportRepository;
        _logger = logger;
    }

    // Demandes par semaine
    public async Task<WeeklyRequestsReportDto> GetWeeklyRequestsAsync(
       DateTime from,
       DateTime to)
    {
        var weeklyRequestsReport = await _reportRepository.GetWeeklyRequestsAsync(from, to);
        return weeklyRequestsReport;
    }

    // Répartition par application
    public async Task<RequestsByApplicationReportDto> GetRequestsByApplicationAsync(
       DateTime from,
       DateTime to)
    {
        var requestsByApplicationReport = await _reportRepository.GetRequestsByApplicationAsync(from, to);
        return requestsByApplicationReport;
    }

    // Évolution du respect des SLA
    public async Task<SlaComplianceReportDto> GetSlaComplianceAsync(
        DateTime from,
        DateTime to,
        int? idClasseService = null)
    {
        var slaComplianceReport = await _reportRepository.GetSlaComplianceAsync(from, to, idClasseService);
        return slaComplianceReport;
    }

    // Temps moyen de résolution
    public async Task<MeanResolutionTimeDto> GetMeanResolutionTimeAsync(
        DateTime from,
        DateTime to)
    {
        var meanResolutionTime = await _reportRepository.GetMeanResolutionTimeAsync(from, to);
        return meanResolutionTime;
    }

    public async Task<LastTwoWeeksReportDto> GetLastTwoWeeksAsync()
    {
        var lastTwoWeeks = await _reportRepository.GetLastTwoWeeksAsync();
        return lastTwoWeeks;
    }
}