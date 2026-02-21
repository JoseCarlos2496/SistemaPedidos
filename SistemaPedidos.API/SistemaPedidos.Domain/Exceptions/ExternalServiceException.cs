using System.Runtime.Serialization;

namespace SistemaPedidos.Domain.Exceptions
{
    /// <summary>
    /// Excepción para errores al comunicarse con servicios externos.
    /// Mapeada a HTTP 503 Service Unavailable por el middleware.
    /// </summary>
    /// <remarks>
    /// Ejemplos de uso:
    /// - Servicio externo no responde (timeout)
    /// - Error de conexión de red
    /// - Servicio retorna 5xx (error del servidor)
    /// - Servicio retorna 429 (rate limit)
    /// - Respuesta con formato inválido
    /// </remarks>
    [Serializable]
    public class ExternalServiceException : DomainException
    {
        /// <summary>
        /// Nombre del servicio externo que falló.
        /// Ejemplo: ServicioValidacion, APIProductos, PasarelaPago.
        /// </summary>
        public string ServiceName { get; }

        /// <summary>
        /// Constructor con mensaje y nombre del servicio.
        /// </summary>
        public ExternalServiceException(string message, string serviceName)
            : base(message, $"SERVICE_{serviceName}_ERROR")
        {
            ServiceName = serviceName;
        }

        /// <summary>
        /// Constructor con mensaje, nombre del servicio e inner exception.
        /// </summary>
        public ExternalServiceException(string message, string serviceName, Exception innerException)
            : base(message, $"SERVICE_{serviceName}_ERROR", innerException)
        {
            ServiceName = serviceName;
        }

        /// <summary>
        /// Constructor para deserialización.
        /// </summary>
        protected ExternalServiceException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            ServiceName = info.GetString(nameof(ServiceName)) ?? "Unknown";
        }

        /// <summary>
        /// Implementa serialización incluyendo ServiceName.
        /// </summary>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(ServiceName), ServiceName);
        }
    }
}