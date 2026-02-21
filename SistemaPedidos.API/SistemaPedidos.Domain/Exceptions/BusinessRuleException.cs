using System.Runtime.Serialization;

namespace SistemaPedidos.Domain.Exceptions
{
    /// <summary>
    /// Excepción para violaciones de reglas de negocio.
    /// Mapeada a HTTP 400 Bad Request por el middleware.
    /// </summary>
    /// <remarks>
    /// Ejemplos de uso:
    /// - Cliente no existe en sistema externo
    /// - Total excede límite máximo permitido
    /// - Cliente no cumple requisitos de validación
    /// - Producto fuera de stock
    /// </remarks>
    [Serializable]
    public class BusinessRuleException : DomainException
    {
        /// <summary>
        /// Nombre de la regla de negocio violada.
        /// Ejemplos: CLIENTE_NO_ENCONTRADO, LIMITE_TOTAL_EXCEDIDO.
        /// </summary>
        public string RuleName { get; }

        /// <summary>
        /// Constructor con mensaje y nombre de regla violada.
        /// </summary>
        public BusinessRuleException(string message, string ruleName)
            : base(message, "BUSINESS_RULE_VIOLATION")
        {
            RuleName = ruleName;
            AddMetadata("RuleName", ruleName);
        }

        /// <summary>
        /// Constructor con mensaje, nombre de regla e inner exception.
        /// </summary>
        public BusinessRuleException(string message, string ruleName, Exception innerException)
            : base(message, "BUSINESS_RULE_VIOLATION", innerException)
        {
            RuleName = ruleName;
            AddMetadata("RuleName", ruleName);
        }

        /// <summary>
        /// Constructor para deserialización.
        /// </summary>
        protected BusinessRuleException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            RuleName = info.GetString(nameof(RuleName)) ?? ErrorCode;
        }

        /// <summary>
        /// Implementa serialización incluyendo RuleName.
        /// </summary>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(RuleName), RuleName);
        }
    }
}