using SIGRA.Services;

namespace SIGRA.Middleware;

public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuthenticationMiddleware> _logger;

    public AuthenticationMiddleware(RequestDelegate next, ILogger<AuthenticationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IUserAuthenticationService authService)
    {
        // Skip middleware for anonymous endpoints
        var endpoint = context.GetEndpoint();
        var allowAnonymous = endpoint?.Metadata.GetMetadata<Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute>();

        if (allowAnonymous != null)
        {
            await _next(context);
            return;
        }

        // Get AD username from authenticated identity
        var username = context.User?.Identity?.Name;

        if (string.IsNullOrEmpty(username))
        {
            _logger.LogWarning("Request rejected: No authenticated user found.");
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new
            {
                Error = "Unauthorized",
                Message = "No authenticated user found."
            });
            return;
        }

        // Check if user exists in PostgreSQL
        var isAuthorized = await authService.IsUserAuthorizedAsync(username);

        if (!isAuthorized)
        {
            _logger.LogWarning("Request rejected: User '{Username}' is not in the authorized list.", username);
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new
            {
                Error = "Forbidden",
                Message = $"User '{username}' is not authorized to access this application."
            });
            return;
        }

        _logger.LogInformation("User '{Username}' passed authorization check.", username);

        // Proceed to next middleware/controller
        await _next(context);
    }
}

// Extension method for clean registration in Program.cs
public static class AuthorizedUserMiddlewareExtensions
{
    public static IApplicationBuilder UseAuthorizedUserMiddleware(this IApplicationBuilder app)
    {
        return app.UseMiddleware<AuthenticationMiddleware>();
    }
}
