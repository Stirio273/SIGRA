using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using SIGRA.Domain.Options;

namespace SIGRA.Services;

public class EmailService : IEmailService
{
    private readonly SMTPOptions _smtpOptions;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IOptions<SMTPOptions> smtpOptions,
        ILogger<EmailService> logger)
    {
        _smtpOptions = smtpOptions.Value;
        _logger = logger;
    }

    public async Task SendWeeklyReportAsync(byte[] pdfContent, WeeklyReportDto report)
    {
        using var message = new MailMessage
        {
            From = new MailAddress(_smtpOptions.FromAddress, "Système de Tickets"),
            Subject = $"Rapport hebdomadaire — Semaine du {report.WeekStart:dd/MM/yyyy}",
            Body = BuildEmailBody(report),
            IsBodyHtml = true
        };

        foreach (var recipient in _smtpOptions.Recipients)
            message.To.Add(recipient);

        using var stream = new MemoryStream(pdfContent);
        var attachment = new Attachment(
            stream,
            $"rapport-hebdomadaire-{report.WeekStart:yyyy-MM-dd}.pdf",
            "application/pdf");

        message.Attachments.Add(attachment);

        using var client = new SmtpClient(_smtpOptions.Host, _smtpOptions.Port)
        {
            Credentials = new NetworkCredential(_smtpOptions.Username, _smtpOptions.Password),
            EnableSsl = _smtpOptions.EnableSsl
        };

        await client.SendMailAsync(message);

        _logger.LogInformation(
            "Email envoyé à {count} destinataire(s).",
            _smtpOptions.Recipients.Length);
    }

    private static string BuildEmailBody(WeeklyReportDto report)
    {
        return $"""
            <html>
            <body style="font-family: Arial, sans-serif;">
                <h2>Rapport hebdomadaire</h2>
                <p>Bonjour,</p>
                <p>Veuillez trouver ci-joint le rapport hebdomadaire pour la période
                du <strong>{report.WeekStart:dd/MM/yyyy}</strong> 
                au <strong>{report.WeekEnd:dd/MM/yyyy}</strong>.</p>

                <ul>
                    <li>Total demandes : <strong>{report.WeeklyRequests.Total}</strong></li>
                    <li>Taux SLA moyen : <strong>{report.SlaCompliance.AverageComplianceRate:0.0}%</strong></li>
                </ul>

                <p>Cordialement,<br/>Le système de tickets</p>
            </body>
            </html>
            """;
    }
}
