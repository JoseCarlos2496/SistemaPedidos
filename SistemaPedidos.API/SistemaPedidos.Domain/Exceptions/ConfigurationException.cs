using System.Runtime.Serialization;

namespace SistemaPedidos.Domain.Exceptions
{
    /// <summary>
    /// Excepción para errores de configuración del sistema.
    /// Mapeada a HTTP 500 Internal Server Error por el middleware.
    /// Logueada como CRITICAL ya que requiere intervención inmediata.
    /// </summary>
    /// <remarks>
    /// Ejemplos de uso:
    /// - Configuración faltante en appsettings.json
    /// - Formato de configuración inválido
    /// - Connection string inválido
    /// - URL de servicio externo no configurada
    /// - Credenciales de autenticación faltantes
    /// </remarks>
    [Serializable]
    public class ConfigurationException : DomainException
    {
        /// <summary>
        /// Clave de configuración que tiene el problema.
        /// Ejemplo: ValidacionExterna:BaseUrl, ConnectionStrings:DefaultConnection.
        /// </summary>
        public string ConfigKey { get; }

        /// <summary>
        /// Constructor con mensaje y clave de configuración problemática.
        /// </summary>
        public ConfigurationException(string message, string configKey)
            : base(message, $"CONFIG_{configKey}")
        {
            ConfigKey = configKey;
        }

        /// <summary>
        /// Constructor con mensaje, clave de configuración e inner exception.
        /// </summary>
        public ConfigurationException(string message, string configKey, Exception innerException)
            : base(message, $"CONFIG_{configKey}", innerException)
        {
            ConfigKey = configKey;
        }

        /// <summary>
        /// Constructor para deserialización.
        /// </summary>
        protected ConfigurationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            ConfigKey = info.GetString(nameof(ConfigKey)) ?? "Unknown";
        }

        /// <summary>
        /// Implementa serialización incluyendo ConfigKey.
        /// </summary>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(ConfigKey), ConfigKey);
        }
    }
}