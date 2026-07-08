using Microsoft.AspNetCore.Authorization;
using SIGRA.Services;

namespace SIGRA.Requirements;

public class AdUserRequirement : IAuthorizationRequirement { }

public class AdUserHandler : AuthorizationHandler<AdUserRequirement>
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AdUserHandler> _logger;

    public AdUserHandler(IServiceScopeFactory scopeFactory, ILogger<AdUserHandler> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AdUserRequirement requirement)
    {
        if (context.User?.Identity?.IsAuthenticated != true)
        {
            context.Fail();
            return;
        }
        var username = context.User?.Identity?.Name;

        if (string.IsNullOrEmpty(username))
        {
            _logger.LogWarning("Authentication failed: no username found.");
            context.Fail();
            return;
        }

        using var scope = _scopeFactory.CreateScope();
        var authService = scope.ServiceProvider.GetRequiredService<IUserAuthenticationService>();

        try
        {
            bool isAuthorized = await authService.IsUserAuthorizedAsync(username);

            if (isAuthorized)
                context.Succeed(requirement);
            else
                context.Fail();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking authorization for user '{Username}'", username);
            context.Fail();
            // OR re-throw depending on your error strategy
        }
    }
}
