using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace SIGRA.Services;

public class FallbackAuthenticationOptions : AuthenticationSchemeOptions
{
}

public class FallbackAuthenticationHandler : AuthenticationHandler<FallbackAuthenticationOptions>
{
    private readonly IAuthenticationService _authenticationService;

    public FallbackAuthenticationHandler(
        IOptionsMonitor<FallbackAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
#pragma warning disable CS0618
        ISystemClock clock,
#pragma warning restore CS0618
        IAuthenticationService authenticationService)
        : base(options, logger, encoder, clock)
    {
        _authenticationService = authenticationService;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var negotiateResult = await _authenticationService.AuthenticateAsync(Context, "Negotiate");
        if (negotiateResult.Succeeded)
        {
            return negotiateResult;
        }

        var mockResult = await _authenticationService.AuthenticateAsync(Context, "Mock");
        if (mockResult.Succeeded)
        {
            return mockResult;
        }

        return negotiateResult ?? AuthenticateResult.Fail("Authentication failed.");
    }

    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        var negotiateResult = await _authenticationService.AuthenticateAsync(Context, "Negotiate");
        if (negotiateResult is { Succeeded: false } && negotiateResult.Failure is not null)
        {
            Response.StatusCode = StatusCodes.Status401Unauthorized;
            Response.Headers["WWW-Authenticate"] = "Negotiate";
            await Response.WriteAsync(string.Empty);
            return;
        }

        Response.StatusCode = StatusCodes.Status401Unauthorized;
        Response.Headers["WWW-Authenticate"] = "Mock";
        await Response.WriteAsJsonAsync(new
        {
            Error = "Unauthorized",
            Message = "Provide X-Mock-User header for development authentication."
        });
    }
}
