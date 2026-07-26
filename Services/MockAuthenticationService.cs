using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace SIGRA.Services;

public class MockAuthenticationOptions : AuthenticationSchemeOptions
{
    public string HeaderName { get; set; } = "X-Mock-User";
}

public class MockAuthenticationHandler : AuthenticationHandler<MockAuthenticationOptions>
{
    public MockAuthenticationHandler(
        IOptionsMonitor<MockAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
#pragma warning disable CS0618
        ISystemClock clock)
        : base(options, logger, encoder, clock)
#pragma warning restore CS0618
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(Options.HeaderName, out var headerValues))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var username = headerValues.ToString();

        if (string.IsNullOrWhiteSpace(username))
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid mock user header."));
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Email, username),
            new Claim(ClaimTypes.NameIdentifier, username)
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = StatusCodes.Status401Unauthorized;
        Response.Headers["WWW-Authenticate"] = "Mock";
        return Response.WriteAsJsonAsync(new
        {
            Error = "Unauthorized",
            Message = "Provide X-Mock-User header for development authentication."
        });
    }
}
