using System.Runtime.Serialization;

namespace SistemaPedidos.Domain.Exceptions
{
    /// <summary>
    /// Excepción para errores de base de datos.
    /// Mapeada a HTTP 500 Internal Server Error por el middleware.
    /// </summary>
    /// <remarks>
    /// Ejemplos de uso:
    /// - Error al ejecutar query SQL
    /// - Violación de restricción única (duplicate key)
    /// - Violación de clave foránea
    /// - Timeout de base de datos
    /// - ID generado inválido
    /// </remarks>
    [Serializable]
    public class DatabaseException : DomainException
    {
        /// <summary>
        /// Constructor con mensaje descriptivo del error de BD.
        /// </summary>
        public DatabaseException(string message)
            : base(message, "DATABASE_ERROR")
        {
        }

        /// <summary>
        /// Constructor con mensaje e inner exception (típicamente SqlException o DbUpdateException).
        /// </summary>
        public DatabaseException(string message, Exception innerException)
            : base(message, "DATABASE_ERROR", innerException)
        {
        }

        /// <summary>
        /// Constructor para deserialización.
        /// </summary>
        protected DatabaseException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}