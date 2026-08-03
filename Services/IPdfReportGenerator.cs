namespace SIGRA.Services;

public interface IPdfReportGenerator
{
    byte[] GenerateWeeklyReport(WeeklyReportDto report);
}
