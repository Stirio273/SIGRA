using MailKit;
using MailKit.Net.Imap;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;
using SIGRA.Services;

namespace SIGRA.Services;

public class ImapMailService
{
    private readonly IConfiguration _config;
    private readonly IImapIdentityProvider? _identityProvider;

    public record ImapPreset(string Host, int Port, SecureSocketOptions SecureSocketOptions);

    private static readonly IReadOnlyDictionary<string, ImapPreset> ProviderPresets = new Dictionary<string, ImapPreset>(StringComparer.OrdinalIgnoreCase)
    {
        ["Outlook"] = new("outlook.office365.com", 993, SecureSocketOptions.SslOnConnect),
        ["Gmail"] = new("imap.gmail.com", 993, SecureSocketOptions.SslOnConnect),
        ["Yahoo"] = new("imap.mail.yahoo.com", 993, SecureSocketOptions.SslOnConnect),
        ["ICloud"] = new("imap.mail.me.com", 993, SecureSocketOptions.SslOnConnect),
        ["Local"] = new("localhost", 143, SecureSocketOptions.StartTls)
    };

    public ImapMailService(IConfiguration config, IImapIdentityProvider? identityProvider = null)
    {
        _config = config;
        _identityProvider = identityProvider;
    }

    public async Task<ImapClient> ConnectAsync(CancellationToken cancellationToken = default)
    {
        var client = new ImapClient();

        var providerKey = _config["Imap:Provider"];
        var host = _config["Imap:Host"];
        var portRaw = _config["Imap:Port"];
        var useSslRaw = _config["Imap:UseSsl"];
        var secureOptionsRaw = _config["Imap:SecureSocketOptions"];
        var useOAuth2 = _config.GetValue<bool?>("Imap:UseOAuth2") ?? false;

        var safeHost = ResolveHost(providerKey, host);
        var (port, secureOptions) = ResolvePortAndSecureSocket(providerKey, portRaw, useSslRaw, secureOptionsRaw);

        try
        {
            await client.ConnectAsync(safeHost, port, secureOptions, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"IMAP connection failed ({safeHost}:{port}). {ex.Message}", ex);
        }

        if (useOAuth2)
        {
            string accessToken = _identityProvider is not null
                ? await _identityProvider.GetAuthorizationMaterialAsync(cancellationToken)
                : _config["Imap:AccessToken"]
                  ?? throw new InvalidOperationException("OAuth2 requires an IImapIdentityProvider or Imap:AccessToken.");

            var oauth2 = new SaslMechanismOAuth2(
                _config["Imap:Username"] ?? string.Empty,
                accessToken);

            await client.AuthenticateAsync(oauth2, cancellationToken);
        }
        else
        {
            var username = _config["Imap:Username"];
            var password = _config["Imap:Password"];
            await client.AuthenticateAsync(username, password, cancellationToken);
        }

        return client;
    }

    public string GetFolderName() => _config["Imap:Folder"] ?? "INBOX";

    public static (string Subject, string From, DateTimeOffset Received, bool IsRead, string Body) MapToMessageSummary(MimeMessage message)
    {
        var body = message.TextBody ?? message.HtmlBody ?? string.Empty;
        return (
            message.Subject,
            message.From?.ToString() ?? string.Empty,
            message.Date.UtcDateTime,
            message.MessageId != null,
            body
        );
    }

    private string ResolveHost(string? providerKey, string? host)
    {
        if (!string.IsNullOrEmpty(host))
            return host;

        if (!string.IsNullOrEmpty(providerKey) && ProviderPresets.TryGetValue(providerKey, out var preset))
            return preset.Host;

        throw new InvalidOperationException("IMAP host is not configured.");
    }

    private (int Port, SecureSocketOptions SecureSocketOptions) ResolvePortAndSecureSocket(
        string? providerKey, string? portRaw, string? useSslRaw, string? secureOptionsRaw)
    {
        int port = 993;
        SecureSocketOptions secureOptions = SecureSocketOptions.SslOnConnect;

        if (!string.IsNullOrEmpty(portRaw) && int.TryParse(portRaw, out var parsedPort))
            port = parsedPort;
        else if (!string.IsNullOrEmpty(providerKey) && ProviderPresets.TryGetValue(providerKey, out var preset))
            port = preset.Port;

        if (!string.IsNullOrEmpty(secureOptionsRaw) &&
            Enum.TryParse<SecureSocketOptions>(secureOptionsRaw, true, out var explicitOptions))
        {
            secureOptions = explicitOptions;
        }
        else if (!string.IsNullOrEmpty(useSslRaw) && bool.TryParse(useSslRaw, out var useSsl))
        {
            secureOptions = useSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls;
        }
        else if (!string.IsNullOrEmpty(providerKey) && ProviderPresets.TryGetValue(providerKey, out var preset))
        {
            secureOptions = preset.SecureSocketOptions;
        }

        return (port, secureOptions);
    }
}
