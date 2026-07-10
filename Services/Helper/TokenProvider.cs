using Microsoft.Kiota.Abstractions.Authentication;

namespace SIGRA.Services.Helper;

public class TokenProvider : IAccessTokenProvider
{
    private readonly MailAuthService _mailAuthService;

    public TokenProvider(MailAuthService mailAuthService)
    {
        _mailAuthService = mailAuthService;
    }

    public async Task<string> GetAuthorizationTokenAsync(
        Uri uri,
        Dictionary<string, object> additionalAuthenticationContext = null,
        CancellationToken cancellationToken = default)
    {
        return await _mailAuthService.GetValidAccessTokenAsync();
    }

    public AllowedHostsValidator AllowedHostsValidator { get; } =
        new AllowedHostsValidator(new[] { "graph.microsoft.com" });
}