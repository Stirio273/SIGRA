namespace SIGRA.Services;

public interface IEmailService
{
    Task SendWeeklyReportAsync(byte[] pdfContent, WeeklyReportDto report);
}
