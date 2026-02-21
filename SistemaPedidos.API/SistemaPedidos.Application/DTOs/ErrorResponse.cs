namespace SistemaPedidos.Application.DTOs
{
    /// <summary>
    /// DTO para respuestas de error estandarizadas en toda la API.
    /// Proporciona formato consistente para errores HTTP 4xx y 5xx.
    /// </summary>
    /// <remarks>
    /// Generado por GlobalExceptionHandlerMiddleware o controllers (validación ModelState).
    /// Serializado automáticamente a JSON por ASP.NET Core.
    /// Sigue parcialmente RFC 7807 (Problem Details for HTTP APIs).
    /// </remarks>
    public class ErrorResponse
    {
        /// <summary>
        /// Código de estado HTTP. Debe coincidir con el status code de la respuesta.
        /// Ejemplos: 400 (Bad Request), 422 (Unprocessable Entity), 500 (Internal Server Error), 503 (Service Unavailable).
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// Mensaje principal del error en lenguaje no técnico para usuario final.
        /// Genérico para 5xx, específico para 4xx. NO incluye detalles sensibles.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Código de error del dominio para identificación programática.
        /// Formato: MAYUSCULAS_CON_GUIONES. Ejemplos: VALIDATION_ERROR, CLIENTE_NO_ENCONTRADO, DATABASE_ERROR.
        /// Permite al cliente hacer switch/case sin parsear mensaje.
        /// </summary>
        public string? ErrorCode { get; set; }

        /// <summary>
        /// Timestamp UTC cuando ocurrió el error. Formato ISO 8601.
        /// Usado para correlación con logs del servidor.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Metadata contextual del error (IDs, valores, límites).
        /// Ejemplos: ClienteId, Total, ProductoId, HttpStatusCode.
        /// NO incluye datos sensibles ni stack traces.
        /// </summary>
        public Dictionary<string, object>? Details { get; set; }

        /// <summary>
        /// Errores de validación por campo desde ModelState.
        /// Key: nombre del campo, Value: array de mensajes de error.
        /// Usado principalmente en respuestas 422 (Unprocessable Entity).
        /// Ejemplo: { "ClienteId": ["El ClienteId es requerido"], "Usuario": ["Debe tener entre 3 y 100 caracteres"] }
        /// </summary>
        public Dictionary<string, string[]>? ValidationErrors { get; set; }
    }
}