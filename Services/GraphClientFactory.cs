using Microsoft.Graph;
using Microsoft.Kiota.Abstractions.Authentication;
using SIGRA.Services.Helper;

namespace SIGRA.Services;

public class GraphClientFactory
{
    private readonly MailAuthService _mailAuthService;

    public GraphClientFactory(MailAuthService mailAuthService)
    {
        _mailAuthService = mailAuthService;
    }

    public GraphServiceClient CreateClient()
    {
        // Custom token provider using our MailAuthService
        var tokenProvider = new TokenProvider(_mailAuthService);
        var authProvider = new BaseBearerTokenAuthenticationProvider(tokenProvider);
        return new GraphServiceClient(authProvider);
    }
}