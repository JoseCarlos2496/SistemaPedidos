namespace SistemaPedidos.Infrastructure.Constants
{
    /// <summary>
    /// Constantes para claves de configuración y valores predeterminados del sistema.
    /// Centraliza configuración para evitar magic strings.
    /// </summary>
    /// <remarks>
    /// Usadas para:
    /// - Leer configuración desde appsettings.json (ValidacionExterna:*)
    /// - Validaciones de negocio (límites y rangos)
    /// - Endpoints de servicios externos
    /// 
    /// Patrón: Configuración centralizada en constantes.
    /// </remarks>
    public static class ConfigurationKeys
    {
        /// <summary>
        /// Clave para URL base del servicio de validación externa.
        /// Valor en appsettings: "ValidacionExterna:BaseUrl"
        /// </summary>
        public const string VALIDACION_BASE_URL = "ValidacionExterna:BaseUrl";

        /// <summary>
        /// Clave para límite máximo de total de pedido.
        /// Valor en appsettings: "ValidacionExterna:LimiteTotal"
        /// </summary>
        public const string VALIDACION_LIMITE_TOTAL = "ValidacionExterna:LimiteTotal";

        /// <summary>
        /// Clave para timeout en segundos del HttpClient.
        /// Valor en appsettings: "ValidacionExterna:Timeout"
        /// </summary>
        public const string VALIDACION_TIMEOUT = "ValidacionExterna:TimeoutSeconds";

        /// <summary>
        /// Endpoint relativo para consulta de usuarios.
        /// Se concatena con BaseUrl: {BaseUrl}/users/{id}
        /// </summary>
        public const string ENDPOINT_USUARIOS = "users";

        /// <summary>
        /// ClienteId mínimo permitido (validación de negocio).
        /// ClienteId debe ser >= 1.
        /// </summary>
        public const int CLIENTE_ID_MINIMO = 1;

        /// <summary>
        /// Total mínimo permitido (validación de negocio).
        /// Total debe ser > 0.
        /// </summary>
        public const decimal TOTAL_MINIMO = 0;
    }
}