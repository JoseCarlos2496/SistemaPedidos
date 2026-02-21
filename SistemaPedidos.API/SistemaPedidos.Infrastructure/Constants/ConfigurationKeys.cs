namespace SistemaPedidos.Infrastructure.Constants
{
    /// <summary>
    /// Keys de configuración para appsettings.json
    /// </summary>
    public static class ConfigurationKeys
    {
        // Keys de ValidacionExterna
        public const string VALIDACION_BASE_URL = "ValidacionExterna:BaseUrl";
        public const string VALIDACION_TIMEOUT = "ValidacionExterna:TimeoutSeconds";
        public const string VALIDACION_LIMITE_TOTAL = "ValidacionExterna:LimiteTotal";
        public const string VALIDACION_REINTENTOS = "ValidacionExterna:ReintentosMaximos";

        // Otros valores predeterminados
        public const int CLIENTE_ID_MINIMO = 1;
        public const decimal TOTAL_MINIMO = 0m;

        // Endpoints
        public const string ENDPOINT_USUARIOS = "/users";
        
        // Keys de ConnectionStrings
        public const string CONNECTION_STRING_DEFAULT = "ConnectionStrings:DefaultConnection";
        
        // Keys de Logging
        public const string LOGGING_LEVEL = "Logging:LogLevel:Default";

    }
}