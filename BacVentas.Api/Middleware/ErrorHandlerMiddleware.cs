using Microsoft.Data.SqlClient;

namespace BacVentas.Api.Middleware;

// Middleware = código que se ejecuta en CADA petición, antes y después del controlador.
// Este atrapa todos los errores en un solo lugar (manejo centralizado de errores)
// y siempre responde con el mismo formato: { "mensaje": "..." }.
public class ErrorHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlerMiddleware> _logger;

    public ErrorHandlerMiddleware(RequestDelegate next, ILogger<ErrorHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);   // continúa hacia el controlador
        }
        catch (Exception ex)
        {
            // El error de SQL puede venir directo o envuelto por EF (InnerException).
            var sqlEx = ex as SqlException ?? ex.InnerException as SqlException;

            if (sqlEx is not null && sqlEx.Number >= 50000)
            {
                // Error de negocio lanzado con THROW desde un SP (50001, 50002...).
                // Su mensaje es seguro y claro para mostrarlo al usuario.
                context.Response.StatusCode = sqlEx.Number == 50001
                    ? StatusCodes.Status404NotFound
                    : StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new { mensaje = sqlEx.Message });
                return;
            }

            // Error inesperado: se registra completo en el log, pero al usuario
            // solo se le da un mensaje genérico (no se exponen detalles técnicos).
            _logger.LogError(ex, "Error no controlado en {Ruta}", context.Request.Path);
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new { mensaje = "Ocurrió un error inesperado. Intente de nuevo." });
        }
    }
}
