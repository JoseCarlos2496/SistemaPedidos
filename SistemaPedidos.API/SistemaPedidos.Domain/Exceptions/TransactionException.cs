using System.Runtime.Serialization;

namespace SistemaPedidos.Domain.Exceptions
{
    /// <summary>
    /// Excepción para errores en manejo transaccional de base de datos.
    /// Mapeada a HTTP 500 Internal Server Error por el middleware.
    /// </summary>
    /// <remarks>
    /// Ejemplos de uso:
    /// - No se puede iniciar transacción (ya existe una activa)
    /// - Error al hacer commit
    /// - Error al hacer rollback
    /// - Timeout durante transacción
    /// - Deadlock detectado
    /// </remarks>
    [Serializable]
    public class TransactionException : DomainException
    {
        /// <summary>
        /// Constructor con mensaje descriptivo del error transaccional.
        /// </summary>
        public TransactionException(string message)
            : base(message, "TRANSACTION_ERROR")
        {
        }

        /// <summary>
        /// Constructor con mensaje e inner exception.
        /// </summary>
        public TransactionException(string message, Exception innerException)
            : base(message, "TRANSACTION_ERROR", innerException)
        {
        }

        /// <summary>
        /// Constructor para deserialización.
        /// </summary>
        protected TransactionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}