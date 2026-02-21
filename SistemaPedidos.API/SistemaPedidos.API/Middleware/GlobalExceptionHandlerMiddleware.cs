using Microsoft.AspNetCore.Mvc;
using SistemaPedidos.Application.DTOs;
using SistemaPedidos.Domain.Exceptions;
using System.Net;
using System.Text.Json;

namespace SistemaPedidos.API.Middleware
{
    /// <summary>
    /// Middleware global para manejo centralizado de excepciones en toda la aplicación.
    /// </summary>
    /// <remarks>
    /// Este middleware intercepta TODAS las excepciones no controladas que ocurren
    /// en cualquier punto de la pipeline de ASP.NET Core y las convierte en respuestas
    /// HTTP apropiadas con código de estado y formato estandarizado.
    /// 
    /// ARQUITECTURA:
    /// - Se registra como el PRIMER middleware en Program.cs
    /// - Captura excepciones de controllers, servicios y capas inferiores
    /// - Mapea excepciones de dominio a códigos HTTP RESTful
    /// - Serializa respuestas en formato JSON consistente
    /// - Registra eventos en el sistema de logging
    /// 
    /// VENTAJAS:
    /// - Separación de responsabilidades (controllers no manejan excepciones)
    /// - Respuestas de error consistentes en toda la API
    /// - Código más limpio sin try-catch masivos
    /// - Centralización del logging de errores
    /// - Facilita testing y mantenimiento
    /// 
    /// MAPEO DE EXCEPCIONES A HTTP:
    /// - ValidationException → 422 Unprocessable Entity
    /// - BusinessRuleException → 400 Bad Request
    /// - ExternalServiceException → 503 Service Unavailable
    /// - DatabaseException → 500 Internal Server Error
    /// - TransactionException → 500 Internal Server Error
    /// - ConfigurationException → 500 Internal Server Error
    /// - OperationCanceledException → 499 Client Closed Request
    /// - DomainException → 500 Internal Server Error
    /// - Exception (genérica) → 500 Internal Server Error
    /// </remarks>
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

        /// <summary>
        /// Constructor del middleware.
        /// </summary>
        /// <param name="next">Delegado al siguiente middleware en la pipeline</param>
        /// <param name="logger">Logger para registro de excepciones</param>
        public GlobalExceptionHandlerMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Método de invocación del middleware que captura excepciones de la pipeline.
        /// </summary>
        /// <remarks>
        /// Este método se ejecuta para cada petición HTTP:
        /// 1. Intenta ejecutar el siguiente middleware/controller
        /// 2. Si ocurre una excepción, la captura y la procesa
        /// 3. Retorna una respuesta HTTP apropiada al cliente
        /// 
        /// El método NO re-lanza excepciones, garantizando que siempre
        /// se retorna una respuesta HTTP válida al cliente.
        /// </remarks>
        /// <param name="context">Contexto HTTP de la petición actual</param>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        /// <summary>
        /// Procesa una excepción y genera una respuesta HTTP apropiada.
        /// </summary>
        /// <remarks>
        /// Este método:
        /// 1. Analiza el tipo de excepción
        /// 2. Mapea a código de estado HTTP RESTful
        /// 3. Crea un ErrorResponse estandarizado
        /// 4. Registra el evento en el logger con nivel apropiado
        /// 5. Serializa y escribe la respuesta JSON
        /// 
        /// NIVELES DE LOGGING:
        /// - LogWarning: Errores de validación y reglas de negocio (controlables)
        /// - LogError: Errores de servicios externos y base de datos (recuperables)
        /// - LogCritical: Errores de configuración y no controlados (requieren atención)
        /// 
        /// El método garantiza que:
        /// - Siempre se retorna JSON válido
        /// - El statusCode está en el response y en el body
        /// - Se incluye timestamp UTC para trazabilidad
        /// - No se exponen detalles internos en producción
        /// </remarks>
        /// <param name="context">Contexto HTTP de la petición</param>
        /// <param name="exception">Excepción capturada</param>
        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var response = context.Response;
            response.ContentType = "application/json";

            var errorResponse = new ErrorResponse();

            switch (exception)
            {
                case ValidationException validationEx:
                    response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
                    errorResponse = new ErrorResponse
                    {
                        StatusCode = response.StatusCode,
                        Message = validationEx.Message,
                        ErrorCode = validationEx.ErrorCode,
                        Timestamp = DateTime.UtcNow,
                        Details = validationEx.Metadata
                    };
                    _logger.LogWarning(validationEx, "Validación fallida: {Message}", validationEx.Message);
                    break;

                case BusinessRuleException businessEx:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse = new ErrorResponse
                    {
                        StatusCode = response.StatusCode,
                        Message = businessEx.Message,
                        ErrorCode = businessEx.RuleName,
                        Timestamp = DateTime.UtcNow,
                        Details = businessEx.Metadata
                    };
                    _logger.LogWarning(businessEx, "Regla de negocio violada: {RuleName}", businessEx.RuleName);
                    break;

                case ExternalServiceException serviceEx:
                    response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                    errorResponse = new ErrorResponse
                    {
                        StatusCode = response.StatusCode,
                        Message = "Servicio temporal no disponible. Intente más tarde.",
                        ErrorCode = $"SERVICE_{serviceEx.ServiceName}_UNAVAILABLE",
                        Timestamp = DateTime.UtcNow
                    };
                    _logger.LogError(serviceEx, "Servicio externo no disponible: {ServiceName}", serviceEx.ServiceName);
                    break;

                case DatabaseException dbEx:
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    errorResponse = new ErrorResponse
                    {
                        StatusCode = response.StatusCode,
                        Message = "Error al procesar la operación en base de datos.",
                        ErrorCode = "DATABASE_ERROR",
                        Timestamp = DateTime.UtcNow
                    };
                    _logger.LogError(dbEx, "Error de base de datos: {Message}", dbEx.Message);
                    break;

                case TransactionException txEx:
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    errorResponse = new ErrorResponse
                    {
                        StatusCode = response.StatusCode,
                        Message = "Error en la transacción.",
                        ErrorCode = "TRANSACTION_ERROR",
                        Timestamp = DateTime.UtcNow
                    };
                    _logger.LogError(txEx, "Error de transacción: {Message}", txEx.Message);
                    break;

                case ConfigurationException configEx:
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    errorResponse = new ErrorResponse
                    {
                        StatusCode = response.StatusCode,
                        Message = "Error de configuración del sistema.",
                        ErrorCode = $"CONFIG_{configEx.ConfigKey}",
                        Timestamp = DateTime.UtcNow
                    };
                    _logger.LogCritical(configEx, "Error CRÍTICO de configuración: {ConfigKey}", configEx.ConfigKey);
                    break;

                case OperationCanceledException:
                    response.StatusCode = 499;
                    errorResponse = new ErrorResponse
                    {
                        StatusCode = response.StatusCode,
                        Message = "La operación fue cancelada.",
                        ErrorCode = "OPERATION_CANCELLED",
                        Timestamp = DateTime.UtcNow
                    };
                    _logger.LogWarning("Operación cancelada por el cliente");
                    break;

                case DomainException domainEx:
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    errorResponse = new ErrorResponse
                    {
                        StatusCode = response.StatusCode,
                        Message = "Error en el dominio de la aplicación.",
                        ErrorCode = domainEx.ErrorCode,
                        Timestamp = DateTime.UtcNow,
                        Details = domainEx.Metadata
                    };
                    _logger.LogError(domainEx, "Error de dominio: {ErrorCode}", domainEx.ErrorCode);
                    break;

                default:
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    errorResponse = new ErrorResponse
                    {
                        StatusCode = response.StatusCode,
                        Message = "Ha ocurrido un error inesperado.",
                        ErrorCode = "INTERNAL_ERROR",
                        Timestamp = DateTime.UtcNow
                    };
                    _logger.LogCritical(exception, "Error CRÍTICO no controlado: {Type}", exception.GetType().Name);
                    break;
            }

            var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await response.WriteAsync(jsonResponse);
        }
    }

    /// <summary>
    /// Métodos de extensión para registrar el middleware de manejo de excepciones.
    /// </summary>
    public static class GlobalExceptionHandlerMiddlewareExtensions
    {
        /// <summary>
        /// Agrega el middleware de manejo global de excepciones a la pipeline de ASP.NET Core.
        /// </summary>
        /// <remarks>
        /// IMPORTANTE: Este middleware debe registrarse como el PRIMERO en Program.cs,
        /// antes de cualquier otro middleware (Swagger, Authentication, etc.) para
        /// garantizar que captura todas las excepciones.
        /// 
        /// Ejemplo de uso en Program.cs:
        /// <code>
        /// app.UseGlobalExceptionHandler();
        /// app.UseSwagger();
        /// app.UseAuthentication();
        /// app.UseAuthorization();
        /// app.MapControllers();
        /// </code>
        /// </remarks>
        /// <param name="builder">Builder de la aplicación</param>
        /// <returns>Builder de la aplicación para encadenamiento fluido</returns>
        public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<GlobalExceptionHandlerMiddleware>();
        }
    }
}