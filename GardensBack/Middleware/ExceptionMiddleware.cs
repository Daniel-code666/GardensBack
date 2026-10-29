using GardensBack.Application.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace GardensBack.Middleware
{
    /// <summary>
    /// Convierte las excepciones de la aplicación en respuestas HTTP compatibles con Problem Details.
    /// </summary>
    /// <param name="next">Siguiente componente de la canalización HTTP.</param>
    /// <param name="logger">Servicio utilizado para registrar excepciones.</param>
    public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        /// <summary>
        /// Procesa la solicitud y transforma las excepciones controladas o inesperadas.
        /// </summary>
        /// <param name="context">Contexto HTTP de la solicitud actual.</param>
        /// <returns>Una tarea que representa la ejecución del middleware.</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
            {
                // El consumidor cerró la conexión; no intentar escribir una respuesta.
            }
            catch (Exception ex)
            {
                if (context.Response.HasStarted)
                    throw;

                var (status, title) = ex switch
                {
                    ValidationException => (400, "Invalid data"),
                    UnathorizedException => (401, "Unathorized"),
                    NotFoundException => (404, "Not found"),
                    ConflictException => (409, "Conflict"),
                    _ => (500, "Server error")
                };

                if (status == 500)
                    logger.LogError(ex, "No controlled exception. TraceId: {TraceId}", context.TraceIdentifier);
                else
                    logger.LogWarning("Request revoked, status: {Status} -> {Message}", status, ex.Message);

                context.Response.Clear();

                if (status == 401)
                    context.Response.Headers.WWWAuthenticate = "Bearer";

                var problem = new ProblemDetails
                {
                    Status = status,
                    Title = title,
                    Detail = ex is AppException ? ex.Message : status == 500
                        ? "Ocurrió un error inesperado al procesar la solicitud." : "La solicitud no es válida.",
                    Instance = context.Request.Path
                };

                problem.Extensions["traceId"] = Activity.Current?.Id ?? context.TraceIdentifier;
                await Results.Problem(problem).ExecuteAsync(context);
            }
        }
    }
}
