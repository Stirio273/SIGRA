namespace SIGRA.Services;

public interface IReportService
{
    Task<WeeklyRequestsReportDto> GetWeeklyRequestsAsync(
        DateTime from,
        DateTime to);

    Task<RequestsByApplicationReportDto> GetRequestsByApplicationAsync(
        DateTime from,
        DateTime to);

    Task<SlaComplianceReportDto> GetSlaComplianceAsync(
        DateTime from,
        DateTime to,
        int? idClasseService = null);

    Task<MeanResolutionTimeDto> GetMeanResolutionTimeAsync(
        DateTime from,
        DateTime to);

    Task<LastTwoWeeksReportDto> GetLastTwoWeeksAsync();
}