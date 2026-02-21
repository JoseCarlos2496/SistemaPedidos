using System.Runtime.Serialization;

namespace SistemaPedidos.Domain.Exceptions
{
    /// <summary>
    /// Excepción para errores de validación de formato o rangos de datos de entrada.
    /// Mapeada a HTTP 422 Unprocessable Entity por el middleware.
    /// </summary>
    /// <remarks>
    /// Ejemplos de uso:
    /// - Campo requerido vacío
    /// - Valor fuera de rango permitido
    /// - Formato de dato inválido
    /// - Longitud de string fuera de límites
    /// </remarks>
    [Serializable]
    public class ValidationException : DomainException
    {
        /// <summary>
        /// Constructor con mensaje descriptivo del error de validación.
        /// </summary>
        public ValidationException(string message)
            : base(message, "VALIDATION_ERROR")
        {
        }

        /// <summary>
        /// Constructor con mensaje e inner exception.
        /// </summary>
        public ValidationException(string message, Exception innerException)
            : base(message, "VALIDATION_ERROR", innerException)
        {
        }

        /// <summary>
        /// Constructor para deserialización.
        /// </summary>
        protected ValidationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}