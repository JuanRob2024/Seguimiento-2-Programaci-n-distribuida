using Microsoft.AspNetCore.Mvc;
using Sgcae.Application.Common;
using Sgcae.Domain.Common;

namespace Sgcae.Api.Middleware;

// EN: This middleware catches the exceptions of the inner layers and translates them into HTTP status codes.
//     This way the controllers stay clean and every error has the same JSON format (RFC 7807 ProblemDetails).
// ES: Este middleware captura las excepciones de las capas internas y las traduce a códigos de estado HTTP.
//     Así los controladores quedan limpios y todos los errores tienen el mismo formato JSON (RFC 7807 ProblemDetails).
public sealed class ExceptionHandlingMiddleware : IMiddleware
{
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger) => _logger = logger;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            // EN: Here I decide the HTTP status code depending on the type of exception.
            // ES: Aquí decido el código de estado HTTP según el tipo de excepción.
            var (status, title) = exception switch
            {
                DomainValidationException => (StatusCodes.Status400BadRequest, "Invalid data"),
                RequestValidationException => (StatusCodes.Status400BadRequest, "Invalid request"),
                NotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
                ConflictException => (StatusCodes.Status409Conflict, "Conflict"),
                BusinessRuleException => (StatusCodes.Status409Conflict, "Business rule violated"),
                _ => (StatusCodes.Status500InternalServerError, "Unexpected error")
            };

            if (status == StatusCodes.Status500InternalServerError)
            {
                _logger.LogError(exception, "Unhandled exception processing {Path}", context.Request.Path);
            }

            var problem = new ProblemDetails
            {
                Status = status,
                Title = title,
                // EN: I never expose internal details of unexpected errors to the client.
                // ES: Nunca expongo al cliente los detalles internos de los errores inesperados.
                Detail = status == StatusCodes.Status500InternalServerError
                    ? "An unexpected error occurred. Please contact the administrator."
                    : exception.Message,
                Instance = context.Request.Path
            };

            context.Response.StatusCode = status;
            await context.Response.WriteAsJsonAsync(problem, (System.Text.Json.JsonSerializerOptions?)null, "application/problem+json");
        }
    }
}
