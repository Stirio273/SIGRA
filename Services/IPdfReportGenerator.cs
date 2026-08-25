namespace SIGRA.Services;

public interface IPdfReportGenerator
{
    byte[] GenerateWeeklyReport(WeeklyReportDto report);
    Task<byte[]> GenerateFromHtmlAsync(string html);
}
