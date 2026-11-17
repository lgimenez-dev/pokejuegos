using PokeJuegos.Application;

namespace PokeJuegos.Api.Middleware;

/// <summary>
/// ASP.NET Core middleware that catches any unhandled exception and returns a structured HTTP 500 response.
/// In Development environments the response includes the exception message and stack trace.
/// </summary>
public class ExceptionHandlingMiddleware(RequestDelegate next, IHostEnvironment env)
{
    /// <summary>
    /// Invokes the next middleware in the pipeline and handles any unhandled exception.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";

            object error = env.IsDevelopment()
                ? new { message = ErrorMessages.InternalServerError, exception = ex.Message, stackTrace = ex.StackTrace }
                : (object)new { message = ErrorMessages.InternalServerError };

            await context.Response.WriteAsJsonAsync(new { errors = new[] { error } });
        }
    }
}

/// <summary>
/// Extension methods for registering <see cref="ExceptionHandlingMiddleware"/> in the request pipeline.
/// </summary>
public static class ExceptionHandlingMiddlewareExtensions
{
    /// <summary>
    /// Adds <see cref="ExceptionHandlingMiddleware"/> to the application pipeline.
    /// Should be the first middleware registered so it catches exceptions from all subsequent components.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The same <see cref="IApplicationBuilder"/> for chaining.</returns>
    public static IApplicationBuilder UseExceptionHandlingMiddleware(this IApplicationBuilder app) =>
        app.UseMiddleware<ExceptionHandlingMiddleware>();
}
