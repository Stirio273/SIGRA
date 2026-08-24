using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SIGRA.Data.Models;
using SIGRA.Data.Enums;
using SIGRA.Data.Repositories;
using SIGRA.Domain.Options;
using SIGRA.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace SIGRA.Services;

public class EmailService : IEmailService
{
    private readonly SMTPOptions _smtpOptions;
    private readonly ILogger<EmailService> _logger;
    private readonly IServiceAccountTokenRepository _tokenRepository;
    private readonly ITokenEncryptionService _encryption;
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;

    public EmailService(
        IOptions<SMTPOptions> smtpOptions,
        ILogger<EmailService> logger,
        IServiceAccountTokenRepository tokenRepository,
        ITokenEncryptionService encryption,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory)
    {
        _smtpOptions = smtpOptions.Value;
        _logger = logger;
        _tokenRepository = tokenRepository;
        _encryption = encryption;
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
    }

    public async Task SendWeeklyReportAsync(byte[] pdfContent, WeeklyReportDto report)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Système de Tickets", _smtpOptions.FromAddress));
        foreach (var recipient in _smtpOptions.Recipients)
            message.To.Add(MailboxAddress.Parse(recipient));
        message.Subject = $"Rapport hebdomadaire — Semaine du {report.WeekStart:dd/MM/yyyy}";

        var bodyBuilder = new BodyBuilder { HtmlBody = BuildEmailBody(report) };
        bodyBuilder.Attachments.Add(
            $"rapport-hebdomadaire-{report.WeekStart:yyyy-MM-dd}.pdf",
            pdfContent,
            new ContentType("application", "pdf"));
        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        var secureSocketOptions = _smtpOptions.EnableSsl
            ? (_smtpOptions.Port == 465 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls)
            : SecureSocketOptions.Auto;

        await client.ConnectAsync(_smtpOptions.Host, _smtpOptions.Port, secureSocketOptions).ConfigureAwait(false);

        if (_smtpOptions.UseOAuth2)
        {
            var accessToken = await GetValidAccessTokenAsync().ConfigureAwait(false);
            var oauth2 = new SaslMechanismOAuth2(_smtpOptions.Username, accessToken);
            await client.AuthenticateAsync(oauth2).ConfigureAwait(false);
        }
        else
        {
            await client.AuthenticateAsync(_smtpOptions.Username, _smtpOptions.Password).ConfigureAwait(false);
        }

        await client.SendAsync(message).ConfigureAwait(false);
        await client.DisconnectAsync(true).ConfigureAwait(false);

        _logger.LogInformation(
            "Email envoyé à {count} destinataire(s).",
            _smtpOptions.Recipients.Length);
    }

    private async Task<string> GetValidAccessTokenAsync()
    {
        var email = _smtpOptions.Username;
        if (string.IsNullOrEmpty(email))
            throw new InvalidOperationException("SMTP Username is not configured.");

        var entity = await _tokenRepository.GetAsync(email, OAuthProvider.Google);
        if (entity == null || string.IsNullOrEmpty(entity.EncryptedAccessToken))
            throw new InvalidOperationException("No Gmail access token available for SMTP. Please authenticate first.");

        var accessToken = _encryption.Decrypt(entity.EncryptedAccessToken);

        if (IsExpired(entity))
        {
            var refreshToken = entity.EncryptedRefreshToken != null
                ? _encryption.Decrypt(entity.EncryptedRefreshToken)
                : null;

            if (string.IsNullOrEmpty(refreshToken))
                throw new InvalidOperationException("Gmail access token is expired and no refresh token is available. Please re-authenticate.");

            var refreshed = await RefreshAccessTokenAsync(refreshToken);
            if (!refreshed)
                throw new InvalidOperationException("Unable to refresh Gmail access token. Please re-authenticate.");

            entity = await _tokenRepository.GetAsync(email, OAuthProvider.Google);
            if (entity == null || string.IsNullOrEmpty(entity.EncryptedAccessToken))
                throw new InvalidOperationException("Gmail access token is empty after refresh. Please re-authenticate.");

            accessToken = _encryption.Decrypt(entity.EncryptedAccessToken);
        }

        if (string.IsNullOrEmpty(accessToken))
            throw new InvalidOperationException("Gmail access token is empty. Please re-authenticate.");

        return accessToken;
    }

    private async Task<bool> RefreshAccessTokenAsync(string refreshToken)
    {
        try
        {
            var clientId = _configuration["Imap:GmailOAuth2:ClientId"];
            var clientSecret = _configuration["Imap:GmailOAuth2:ClientSecret"];

            if (string.IsNullOrEmpty(refreshToken) || string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
                return false;

            using var httpClient = _httpClientFactory.CreateClient();
            using var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "https://oauth2.googleapis.com/token")
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["refresh_token"] = refreshToken,
                    ["client_id"] = clientId,
                    ["client_secret"] = clientSecret,
                    ["grant_type"] = "refresh_token"
                })
            };

            using var response = await httpClient.SendAsync(tokenRequest);
            if (!response.IsSuccessStatusCode)
            {
                var errorPayload = await response.Content.ReadAsStringAsync();
                _logger.LogError("Gmail SMTP token refresh failed with status {StatusCode}. Response: {Payload}", (int)response.StatusCode, errorPayload);
                return false;
            }

            var payload = await response.Content.ReadAsStringAsync();
            using var doc = System.Text.Json.JsonDocument.Parse(payload);
            var root = doc.RootElement;

            if (!root.TryGetProperty("access_token", out var accessTokenElement))
                return false;

            var newAccessToken = accessTokenElement.GetString() ?? string.Empty;
            var expiresIn = 3600L;

            if (root.TryGetProperty("expires_in", out var expiresInElement) && expiresInElement.TryGetDouble(out var expiresInDouble))
            {
                expiresIn = (long)expiresInDouble;
            }

            var newRefreshToken = root.TryGetProperty("refresh_token", out var refreshTokenElement)
                ? refreshTokenElement.GetString()
                : refreshToken;

            var scopeValue = root.TryGetProperty("scope", out var scopeElement)
                ? scopeElement.GetString()
                : null;

            var email = _smtpOptions.Username;
            if (string.IsNullOrEmpty(email))
                return false;

            var expiresAt = DateTime.UtcNow.AddSeconds(expiresIn);
            await _tokenRepository.SaveAsync(email, OAuthProvider.Google, newAccessToken, newRefreshToken, expiresAt, scopeValue);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gmail SMTP token refresh failed.");
            return false;
        }
    }

    private static bool IsExpired(ServiceAccountToken entity)
    {
        if (entity.AccessTokenExpiresAt == null)
            return true;

        return DateTime.UtcNow >= entity.AccessTokenExpiresAt.Value;
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

                <p>Cordialement</p>
            </body>
            </html>
            """;
    }
}
