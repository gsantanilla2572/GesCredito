using GesCredito.Services.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace GesCredito.Web.Infrastructure;

/// <summary>Convierte las excepciones de negocio en respuestas ProblemDetails (RFC 9457).</summary>
public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext http, Exception ex, CancellationToken ct)
    {
        var (status, title) = ex switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Recurso no encontrado"),
            ConflictException => (StatusCodes.Status409Conflict, "Conflicto"),
            BusinessRuleException => (StatusCodes.Status422UnprocessableEntity, "Regla de negocio incumplida"),
            _ => (StatusCodes.Status500InternalServerError, "Error interno del servidor")
        };

        if (status == StatusCodes.Status500InternalServerError)
            logger.LogError(ex, "Error no controlado en {Path}", http.Request.Path);

        http.Response.StatusCode = status;
        await http.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = status == 500 ? "Ocurrió un error inesperado." : ex.Message,
            Instance = http.Request.Path
        }, ct);
        return true;
    }
}
