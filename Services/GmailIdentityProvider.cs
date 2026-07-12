using Google.Apis.Auth;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Responses;
using Microsoft.Extensions.Configuration;
using SIGRA.Services;
using System.Net.Http.Headers;
using System.Text.Json;

namespace SIGRA.Services;

public class GmailIdentityProvider : IImapIdentityProvider
{
    private readonly IConfiguration _config;
    private readonly IHttpClientFactory _httpClientFactory;

    public GmailIdentityProvider(IConfiguration config, IHttpClientFactory httpClientFactory)
    {
        _config = config;
        _httpClientFactory = httpClientFactory;
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

            var tokenResponse = new TokenResponse
            {
                AccessToken = accessTokenElement.GetString() ?? string.Empty,
                IssuedUtc = DateTime.UtcNow
            };

            if (root.TryGetProperty("expires_in", out var expiresInElement) && expiresInElement.TryGetDouble(out var expiresIn))
            {
                tokenResponse.ExpiresInSeconds = (long)expiresIn;
            }
            else
            {
                tokenResponse.ExpiresInSeconds = 3600;
            }

            if (root.TryGetProperty("refresh_token", out var refreshTokenElement))
            {
                tokenResponse.RefreshToken = refreshTokenElement.GetString();
            }

            if (root.TryGetProperty("scope", out var scopeElement))
            {
                tokenResponse.Scope = scopeElement.GetString();
            }

            if (root.TryGetProperty("token_type", out var tokenTypeElement))
            {
                tokenResponse.TokenType = tokenTypeElement.GetString();
            }

            File.WriteAllText("data/gmail-token.json", JsonSerializer.Serialize(tokenResponse));
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<string> GetAuthorizationMaterialAsync(CancellationToken cancellationToken = default)
    {
        TokenResponse? token = null;

        if (File.Exists("data/gmail-token.json"))
        {
            try
            {
                token = JsonSerializer.Deserialize<TokenResponse>(await File.ReadAllTextAsync("data/gmail-token.json", cancellationToken));
            }
            catch { }
        }

        if (token == null || string.IsNullOrEmpty(token.AccessToken))
            throw new InvalidOperationException("No Gmail access token available. Please authenticate first.");

        if (IsExpired(token))
        {
            if (string.IsNullOrEmpty(token.RefreshToken))
                throw new InvalidOperationException("Gmail access token is expired and no refresh token is available. Please re-authenticate.");

            var refreshed = await RefreshAccessTokenAsync(token, cancellationToken).ConfigureAwait(false);
            if (!refreshed)
                throw new InvalidOperationException("Unable to refresh Gmail access token. Please re-authenticate.");

            token = JsonSerializer.Deserialize<TokenResponse>(await File.ReadAllTextAsync("data/gmail-token.json", cancellationToken));
        }

        if (string.IsNullOrEmpty(token.AccessToken))
            throw new InvalidOperationException("Gmail access token is empty. Please re-authenticate.");

        return token.AccessToken;
    }

    private async Task<bool> RefreshAccessTokenAsync(TokenResponse token, CancellationToken cancellationToken)
    {
        try
        {
            var clientId = _config["Imap:GmailOAuth2:ClientId"];
            var clientSecret = _config["Imap:GmailOAuth2:ClientSecret"];

            if (string.IsNullOrEmpty(token.RefreshToken) || string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
                return false;

            var httpClient = _httpClientFactory.CreateClient();
            var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "https://oauth2.googleapis.com/token")
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["refresh_token"] = token.RefreshToken,
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

            var newToken = new TokenResponse
            {
                AccessToken = accessTokenElement.GetString() ?? string.Empty,
                RefreshToken = token.RefreshToken,
                IssuedUtc = DateTime.UtcNow
            };

            if (root.TryGetProperty("expires_in", out var expiresInElement) && expiresInElement.TryGetDouble(out var expiresIn))
            {
                newToken.ExpiresInSeconds = (long)expiresIn;
            }
            else
            {
                newToken.ExpiresInSeconds = 3600;
            }

            if (root.TryGetProperty("scope", out var scopeElement))
            {
                newToken.Scope = scopeElement.GetString();
            }

            if (root.TryGetProperty("token_type", out var tokenTypeElement))
            {
                newToken.TokenType = tokenTypeElement.GetString();
            }

            File.WriteAllText("data/gmail-token.json", JsonSerializer.Serialize(newToken));
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool IsExpired(TokenResponse? token)
    {
        if (token == null || token.IssuedUtc == default)
            return true;

        var lifetime = token.ExpiresInSeconds > 0 ? token.ExpiresInSeconds : 3600;
        var expiry = token.IssuedUtc.AddSeconds((double)lifetime);
        return DateTime.UtcNow >= expiry;
    }
}
