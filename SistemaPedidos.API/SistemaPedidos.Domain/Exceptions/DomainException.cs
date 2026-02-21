using System.Runtime.Serialization;

namespace SistemaPedidos.Domain.Exceptions
{
    /// <summary>
    /// Excepción base abstracta para todas las excepciones del dominio.
    /// Define estructura común: ErrorCode, Metadata y serialización.
    /// </summary>
    /// <remarks>
    /// Clases derivadas:
    /// - ValidationException (422)
    /// - BusinessRuleException (400)
    /// - ExternalServiceException (503)
    /// - DatabaseException (500)
    /// - TransactionException (500)
    /// - ConfigurationException (500)
    /// 
    /// Mapeadas a códigos HTTP por GlobalExceptionHandlerMiddleware.
    /// </remarks>
    [Serializable]
    public abstract class DomainException : Exception
    {
        /// <summary>
        /// Código de error específico del dominio.
        /// Ejemplos: VALIDATION_ERROR, CLIENTE_NO_ENCONTRADO, DATABASE_ERROR.
        /// </summary>
        public string ErrorCode { get; }

        /// <summary>
        /// Diccionario de metadata contextual del error.
        /// Contiene información adicional como IDs, valores, límites.
        /// </summary>
        public Dictionary<string, object> Metadata { get; }

        /// <summary>
        /// Constructor base con mensaje y código de error.
        /// </summary>
        protected DomainException(string message, string errorCode)
            : base(message)
        {
            ErrorCode = errorCode;
            Metadata = new Dictionary<string, object>();
        }

        /// <summary>
        /// Constructor base con mensaje, código de error e inner exception.
        /// </summary>
        protected DomainException(string message, string errorCode, Exception innerException)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
            Metadata = new Dictionary<string, object>();
        }

        /// <summary>
        /// Constructor para deserialización.
        /// </summary>
        protected DomainException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            ErrorCode = info.GetString(nameof(ErrorCode)) ?? string.Empty;
            Metadata = (Dictionary<string, object>?)info.GetValue(nameof(Metadata), typeof(Dictionary<string, object>))
                ?? new Dictionary<string, object>();
        }

        /// <summary>
        /// Agrega metadata contextual al error.
        /// </summary>
        /// <param name="key">Clave de metadata (ej: ClienteId, Total)</param>
        /// <param name="value">Valor de metadata</param>
        public void AddMetadata(string key, object value)
        {
            Metadata[key] = value;
        }

        /// <summary>
        /// Implementa serialización para remoting/logging.
        /// </summary>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(ErrorCode), ErrorCode);
            info.AddValue(nameof(Metadata), Metadata);
        }
    }
}