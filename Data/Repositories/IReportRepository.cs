using SIGRA.Data.Models;

namespace SIGRA.Data.Repositories;

public interface IReportRepository
{
    Task<WeeklyRequestsReportDto> GetWeeklyRequestsAsync(
        DateTime from,
        DateTime to);

    Task<RequestsByApplicationReportDto> GetRequestsByApplicationAsync(
        DateTime from,
        DateTime to);

    Task<SlaComplianceReportDto> GetSlaComplianceAsync(
        DateTime from,
        DateTime to);
}