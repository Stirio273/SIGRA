using Microsoft.Identity.Client;

public class MailAuthService
{
    private readonly IConfiguration _config;
    private readonly IConfidentialClientApplication _msalClient;

    // In-memory token storage (use DB in production)
    private string _accessToken;
    private string _refreshToken;
    private DateTimeOffset _tokenExpiry;

    public MailAuthService(IConfiguration config)
    {
        _config = config;
        _msalClient = ConfidentialClientApplicationBuilder
            .Create(config["AzureAd:ClientId"])
            .WithClientSecret(config["AzureAd:ClientSecret"])
            .WithRedirectUri(config["AzureAd:RedirectUri"])
            .WithAuthority($"https://login.microsoftonline.com/consumers")
            .Build();
    }

    // Generate login URL for user
    public async Task<string> GetAuthorizationUrlAsync()
    {
        var scopes = _config.GetSection("AzureAd:Scopes").Get<string[]>();
        var authUrl = await _msalClient
            .GetAuthorizationRequestUrl(scopes)
            .ExecuteAsync();

        return authUrl.ToString();
    }

    // Exchange auth code for tokens
    public async Task<string> ExchangeCodeForTokenAsync(string code)
    {
        var scopes = _config.GetSection("AzureAd:Scopes").Get<string[]>();
        var result = await _msalClient
            .AcquireTokenByAuthorizationCode(scopes, code)
            .ExecuteAsync();

        _accessToken = result.AccessToken;
        _tokenExpiry = result.ExpiresOn;

        return _accessToken;
    }

    // Get valid access token (auto-refresh)
    public async Task<string> GetValidAccessTokenAsync()
    {
        var scopes = _config.GetSection("AzureAd:Scopes").Get<string[]>();
        var accounts = await _msalClient.GetAccountsAsync();
        var account = accounts.FirstOrDefault();

        if (account == null)
            throw new UnauthorizedAccessException("User not authenticated.");

        // MSAL handles token refresh automatically
        var result = await _msalClient
            .AcquireTokenSilent(scopes, account)
            .ExecuteAsync();

        return result.AccessToken;
    }
}
