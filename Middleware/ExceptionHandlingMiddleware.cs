using SIGRA.Domain.Exceptions;

namespace SIGRA.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Laisse passer la requête normalement
            await _next(context);
        }
        catch (NotFoundException ex)
        {
            // Intercepte et retourne un 404 propre
            _logger.LogWarning(ex.Message);

            context.Response.StatusCode = StatusCodes.Status404NotFound;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsJsonAsync(new
            {
                StatusCode = 404,
                Message = ex.Message
            });
        }
        catch (ValidationException ex)
        {
            // Intercepte et retourne un 400
            _logger.LogWarning(ex.Message);

            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsJsonAsync(new
            {
                StatusCode = 400,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            // Intercepte toute autre erreur → 500
            _logger.LogError(ex, "Erreur inattendue");

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsJsonAsync(new
            {
                StatusCode = 500,
                Message = "Une erreur interne est survenue"
            });
        }
    }
}
