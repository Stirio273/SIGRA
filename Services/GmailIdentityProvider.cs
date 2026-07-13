using SIGRA.Data.Enums;
using SIGRA.Data.Models;
using SIGRA.Data.Repositories;

using System.Text.Json;

namespace SIGRA.Services;

public class GmailIdentityProvider : IImapIdentityProvider
{
    private readonly IConfiguration _config;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IServiceScopeFactory _scopeFactory;

    public GmailIdentityProvider(IConfiguration config, IHttpClientFactory httpClientFactory, IServiceScopeFactory scopeFactory)
    {
        _config = config;
        _httpClientFactory = httpClientFactory;
        _scopeFactory = scopeFactory;
    }

    public string GetMailboxIdentity() =>
        _config["Imap:Username"] ?? string.Empty;

    public Task<string?> GetAuthorizationUrlAsync(string? redirectUri = null, CancellationToken cancellationToken = default)
    {
        var clientId = _config["Imap:GmailOAuth2:ClientId"];
        var scopes = _config.GetSection("Imap:GmailOAuth2:Scopes").Get<string[]>() ?? new[] { "https://mail.google.com/" };
        redirectUri ??= _config["Imap:GmailOAuth2:RedirectUri"];

        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(redirectUri))
            return Task.FromResult<string?>(null);

        var scopeParam = string.Join(" ", scopes);
        var url = $"https://accounts.google.com/o/oauth2/v2/auth?client_id={Uri.EscapeDataString(clientId)}&redirect_uri={Uri.EscapeDataString(redirectUri)}&response_type=code&scope={Uri.EscapeDataString(scopeParam)}&access_type=offline&prompt=consent";

        return Task.FromResult<string?>(url);
    }

    public async Task<bool> ExchangeCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(code))
            return false;

        var clientId = _config["Imap:GmailOAuth2:ClientId"];
        var clientSecret = _config["Imap:GmailOAuth2:ClientSecret"];
        var redirectUri = _config["Imap:GmailOAuth2:RedirectUri"];

        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret) || string.IsNullOrEmpty(redirectUri))
            return false;

        var httpClient = _httpClientFactory.CreateClient();
        var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "https://oauth2.googleapis.com/token")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["code"] = code,
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret,
                ["redirect_uri"] = redirectUri,
                ["grant_type"] = "authorization_code"
            })
        };

        using var response = await httpClient.SendAsync(tokenRequest, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
            return false;

        var payload = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            using var doc = JsonDocument.Parse(payload);
            var root = doc.RootElement;

            if (!root.TryGetProperty("access_token", out var accessTokenElement))
                return false;

            var accessToken = accessTokenElement.GetString() ?? string.Empty;
            var expiresIn = 3600L;

            if (root.TryGetProperty("expires_in", out var expiresInElement) && expiresInElement.TryGetDouble(out var expiresInDouble))
            {
                expiresIn = (long)expiresInDouble;
            }

            var refreshToken = root.TryGetProperty("refresh_token", out var refreshTokenElement)
                ? refreshTokenElement.GetString()
                : null;

            var scopeValue = root.TryGetProperty("scope", out var scopeElement)
                ? scopeElement.GetString()
                : null;

            var email = GetMailboxIdentity();
            if (string.IsNullOrEmpty(email))
                return false;

            using var scope = _scopeFactory.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IServiceAccountTokenRepository>();

            var expiresAt = DateTime.UtcNow.AddSeconds(expiresIn);
            await repo.SaveAsync(email, OAuthProvider.Google, accessToken, refreshToken, expiresAt, scopeValue, cancellationToken);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<string> GetAuthorizationMaterialAsync(CancellationToken cancellationToken = default)
    {
        var email = GetMailboxIdentity();
        if (string.IsNullOrEmpty(email))
            throw new InvalidOperationException("No Gmail access token available. Please authenticate first.");

        using var scope = _scopeFactory.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IServiceAccountTokenRepository>();
        var encryption = scope.ServiceProvider.GetRequiredService<ITokenEncryptionService>();

        var entity = await repo.GetAsync(email, OAuthProvider.Google, cancellationToken);
        if (entity == null || string.IsNullOrEmpty(entity.EncryptedAccessToken))
            throw new InvalidOperationException("No Gmail access token available. Please authenticate first.");

        var accessToken = encryption.Decrypt(entity.EncryptedAccessToken);

        if (IsExpired(entity))
        {
            var refreshToken = entity.EncryptedRefreshToken != null
                ? encryption.Decrypt(entity.EncryptedRefreshToken)
                : null;

            if (string.IsNullOrEmpty(refreshToken))
                throw new InvalidOperationException("Gmail access token is expired and no refresh token is available. Please re-authenticate.");

            var refreshed = await RefreshAccessTokenAsync(refreshToken, cancellationToken).ConfigureAwait(false);
            if (!refreshed)
                throw new InvalidOperationException("Unable to refresh Gmail access token. Please re-authenticate.");

            entity = await repo.GetAsync(email, OAuthProvider.Google, cancellationToken);
            if (entity == null || string.IsNullOrEmpty(entity.EncryptedAccessToken))
                throw new InvalidOperationException("Gmail access token is empty. Please re-authenticate.");

            accessToken = encryption.Decrypt(entity.EncryptedAccessToken);
        }

        if (string.IsNullOrEmpty(accessToken))
            throw new InvalidOperationException("Gmail access token is empty. Please re-authenticate.");

        return accessToken;
    }

    private async Task<bool> RefreshAccessTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        try
        {
            var clientId = _config["Imap:GmailOAuth2:ClientId"];
            var clientSecret = _config["Imap:GmailOAuth2:ClientSecret"];

            if (string.IsNullOrEmpty(refreshToken) || string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
                return false;

            var httpClient = _httpClientFactory.CreateClient();
            var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "https://oauth2.googleapis.com/token")
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["refresh_token"] = refreshToken,
                    ["client_id"] = clientId,
                    ["client_secret"] = clientSecret,
                    ["grant_type"] = "refresh_token"
                })
            };

            using var response = await httpClient.SendAsync(tokenRequest, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                return false;

            var payload = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            using var doc = JsonDocument.Parse(payload);
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

            var email = GetMailboxIdentity();
            if (string.IsNullOrEmpty(email))
                return false;

            using var scope = _scopeFactory.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IServiceAccountTokenRepository>();

            var expiresAt = DateTime.UtcNow.AddSeconds(expiresIn);
            await repo.SaveAsync(email, OAuthProvider.Google, newAccessToken, newRefreshToken, expiresAt, scopeValue, cancellationToken);

            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool IsExpired(ServiceAccountToken entity)
    {
        if (entity.AccessTokenExpiresAt == null)
            return true;

        return DateTime.UtcNow >= entity.AccessTokenExpiresAt.Value;
    }
}
