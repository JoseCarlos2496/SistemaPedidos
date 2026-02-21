using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SistemaPedidos.Domain.Exceptions;
using SistemaPedidos.Domain.Interfaces;
using SistemaPedidos.Infrastructure.Constants;
using SistemaPedidos.Infrastructure.Models;
using System.Net;
using System.Net.Http.Json;

namespace SistemaPedidos.Infrastructure.Services
{
    /// <summary>
    /// Servicio de validación de pedidos contra servicio externo JSONPlaceholder.
    /// Valida existencia de clientes y límites de montos.
    /// </summary>
    /// <remarks>
    /// Valida contra JSONPlaceholder API (https://jsonplaceholder.typicode.com).
    /// IDs válidos: 1-10. Otros IDs retornan 404 (BusinessRuleException).
    /// 
    /// Maneja todos los códigos HTTP posibles:
    /// - 200 OK: Cliente válido
    /// - 404 Not Found: Cliente no existe (BusinessRuleException)
    /// - 400/429: Error del servicio (ExternalServiceException)
    /// - 401/403: Error de autenticación (ConfigurationException)
    /// - 5xx: Servicio no disponible (ExternalServiceException)
    /// 
    /// Configuración requerida en appsettings.json:
    /// - ValidacionExterna:BaseUrl
    /// - ValidacionExterna:LimiteTotal
    /// - ValidacionExterna:Timeout
    /// </remarks>
    public class ValidacionExternaService : IValidacionExternaService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ValidacionExternaService> _logger;
        private readonly IConfiguration _configuration;

        private readonly string _baseUrl;
        private readonly decimal _limiteTotal;
        private readonly int _timeoutSeconds;

        public ValidacionExternaService(
            IHttpClientFactory httpClientFactory,
            ILogger<ValidacionExternaService> logger,
            IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _configuration = configuration;

            // Cargar configuración al inicializar
            _baseUrl = ObtenerConfiguracion<string>(ConfigurationKeys.VALIDACION_BASE_URL);
            _limiteTotal = ObtenerConfiguracion<decimal>(ConfigurationKeys.VALIDACION_LIMITE_TOTAL);
            _timeoutSeconds = ObtenerConfiguracion<int>(ConfigurationKeys.VALIDACION_TIMEOUT);

            _logger.LogInformation(
                "ValidacionExternaService inicializado - BaseUrl: {BaseUrl}, Timeout: {Timeout}s, Límite: ${Limite}",
                _baseUrl,
                _timeoutSeconds,
                _limiteTotal
            );
        }

        /// <summary>
        /// Valida un pedido contra reglas de negocio y servicio externo.
        /// Lanza excepciones específicas según el tipo de error.
        /// </summary>
        public async Task ValidarPedidoAsync(int clienteId, decimal total)
        {
            _logger.LogInformation(
                "Iniciando validación de pedido - Cliente: {ClienteId}, Total: ${Total}",
                clienteId,
                total
            );

            // Lanza BusinessRuleException si reglas no cumplen
            ValidarReglasNegocio(clienteId, total);
            
            // Lanza ExternalServiceException si servicio falla
            await ValidarClienteExternoAsync(clienteId);
            
            _logger.LogInformation(
                "Pedido validado exitosamente - Cliente: {ClienteId}, Total: ${Total}",
                clienteId,
                total
            );
        }

        /// <summary>
        /// Valida las reglas de negocio básicas.
        /// Lanza BusinessRuleException si alguna regla no se cumple.
        /// </summary>
        private void ValidarReglasNegocio(int clienteId, decimal total)
        {
            if (clienteId < ConfigurationKeys.CLIENTE_ID_MINIMO)
            {
                _logger.LogWarning("ClienteId inválido: {ClienteId}", clienteId);

                var ex = new BusinessRuleException(
                    $"El ID del cliente debe ser mayor o igual a {ConfigurationKeys.CLIENTE_ID_MINIMO}",
                    "CLIENTE_ID_INVALIDO"
                );
                ex.AddMetadata("ClienteId", clienteId);
                ex.AddMetadata("MinimoPermitido", ConfigurationKeys.CLIENTE_ID_MINIMO);
                throw ex;
            }

            if (total <= ConfigurationKeys.TOTAL_MINIMO)
            {
                _logger.LogWarning("Total inválido: {Total}", total);

                var ex = new BusinessRuleException(
                    $"El total del pedido debe ser mayor a ${ConfigurationKeys.TOTAL_MINIMO}",
                    "TOTAL_INVALIDO"
                );
                ex.AddMetadata("Total", total);
                ex.AddMetadata("MinimoPermitido", ConfigurationKeys.TOTAL_MINIMO);
                throw ex;
            }

            if (total > _limiteTotal)
            {
                _logger.LogWarning(
                    "Total ${Total} excede el límite permitido de ${Limite}",
                    total,
                    _limiteTotal
                );

                var ex = new BusinessRuleException(
                    $"El total del pedido (${total}) excede el límite máximo permitido (${_limiteTotal})",
                    "LIMITE_TOTAL_EXCEDIDO"
                );
                ex.AddMetadata("Total", total);
                ex.AddMetadata("LimiteMaximo", _limiteTotal);
                throw ex;
            }
        }

        /// <summary>
        /// Valida si el cliente existe en el servicio externo.
        /// Lanza ExternalServiceException según el código de estado HTTP.
        /// </summary>
        private async Task ValidarClienteExternoAsync(int clienteId)
        {
            var client = _httpClientFactory.CreateClient("JSONPlaceholder");

            try
            {
                _logger.LogInformation(
                    "Consultando servicio externo para cliente {ClienteId} en {BaseUrl}",
                    clienteId,
                    _baseUrl
                );

                var endpoint = $"{ConfigurationKeys.ENDPOINT_USUARIOS}/{clienteId}";
                var response = await client.GetAsync(endpoint);

                // ✅ Manejar diferentes códigos de estado HTTP
                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        await ValidarRespuestaExitosa(response, clienteId);
                        break;

                    case HttpStatusCode.NotFound:
                        _logger.LogWarning(
                            "Cliente {ClienteId} no encontrado en servicio externo (404)",
                            clienteId
                        );

                        var notFoundEx = new BusinessRuleException(
                            $"El cliente con ID {clienteId} no existe en el sistema",
                            "CLIENTE_NO_ENCONTRADO"
                        );
                        notFoundEx.AddMetadata("ClienteId", clienteId);
                        notFoundEx.AddMetadata("HttpStatusCode", 404);
                        throw notFoundEx;

                    case HttpStatusCode.BadRequest:
                        _logger.LogWarning(
                            "Solicitud incorrecta al servicio externo (400) - Cliente: {ClienteId}",
                            clienteId
                        );

                        var badRequestEx = new ExternalServiceException(
                            "El servicio de validación rechazó la solicitud por formato inválido",
                            "ServicioValidacion"
                        );
                        badRequestEx.AddMetadata("ClienteId", clienteId);
                        badRequestEx.AddMetadata("HttpStatusCode", 400);
                        throw badRequestEx;

                    case HttpStatusCode.Unauthorized:
                    case HttpStatusCode.Forbidden:
                        _logger.LogError(
                            "Error de autenticación/autorización con servicio externo ({StatusCode}) - Cliente: {ClienteId}",
                            response.StatusCode,
                            clienteId
                        );

                        var authEx = new ConfigurationException(
                            "Error de autenticación con el servicio de validación. Verifique las credenciales.",
                            "ServicioValidacionAuth"
                        );
                        authEx.AddMetadata("HttpStatusCode", (int)response.StatusCode);
                        throw authEx;

                    case HttpStatusCode.TooManyRequests:
                        _logger.LogWarning(
                            "Límite de solicitudes excedido en servicio externo (429) - Cliente: {ClienteId}",
                            clienteId
                        );

                        var rateLimitEx = new ExternalServiceException(
                            "El servicio de validación está experimentando alta demanda. Intente más tarde.",
                            "ServicioValidacion"
                        );
                        rateLimitEx.AddMetadata("ClienteId", clienteId);
                        rateLimitEx.AddMetadata("HttpStatusCode", 429);
                        rateLimitEx.AddMetadata("RetryAfter", response.Headers.RetryAfter?.ToString() ?? "desconocido");
                        throw rateLimitEx;

                    case HttpStatusCode.InternalServerError:
                    case HttpStatusCode.BadGateway:
                    case HttpStatusCode.ServiceUnavailable:
                    case HttpStatusCode.GatewayTimeout:
                        _logger.LogError(
                            "Servicio externo no disponible ({StatusCode}) - Cliente: {ClienteId}",
                            response.StatusCode,
                            clienteId
                        );

                        var serviceEx = new ExternalServiceException(
                            $"El servicio de validación no está disponible temporalmente (código {(int)response.StatusCode}). Intente más tarde.",
                            "ServicioValidacion"
                        );
                        serviceEx.AddMetadata("ClienteId", clienteId);
                        serviceEx.AddMetadata("HttpStatusCode", (int)response.StatusCode);
                        throw serviceEx;

                    default:
                        _logger.LogError(
                            "Código de estado HTTP inesperado del servicio externo: {StatusCode} - Cliente: {ClienteId}",
                            response.StatusCode,
                            clienteId
                        );

                        var unexpectedEx = new ExternalServiceException(
                            $"El servicio de validación respondió con un código inesperado ({(int)response.StatusCode})",
                            "ServicioValidacion"
                        );
                        unexpectedEx.AddMetadata("ClienteId", clienteId);
                        unexpectedEx.AddMetadata("HttpStatusCode", (int)response.StatusCode);
                        throw unexpectedEx;
                }
            }
            catch (BusinessRuleException)
            {
                throw;
            }
            catch (ExternalServiceException)
            {
                throw;
            }
            catch (ConfigurationException)
            {
                throw;
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(
                    httpEx,
                    "Error de conexión con el servicio externo para cliente {ClienteId}",
                    clienteId
                );

                var ex = new ExternalServiceException(
                    "No se pudo conectar con el servicio de validación. Verifique la conectividad de red.",
                    "ServicioValidacion",
                    httpEx
                );
                ex.AddMetadata("ClienteId", clienteId);
                throw ex;
            }
            catch (TaskCanceledException timeoutEx)
            {
                _logger.LogError(
                    timeoutEx,
                    "Timeout al consultar servicio externo para cliente {ClienteId} (límite: {Timeout}s)",
                    clienteId,
                    _timeoutSeconds
                );

                var ex = new ExternalServiceException(
                    $"El servicio de validación no respondió en el tiempo esperado ({_timeoutSeconds}s). El servicio puede estar sobrecargado.",
                    "ServicioValidacion",
                    timeoutEx
                );
                ex.AddMetadata("ClienteId", clienteId);
                ex.AddMetadata("TimeoutSeconds", _timeoutSeconds);
                throw ex;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error inesperado al procesar respuesta del servicio externo para cliente {ClienteId}",
                    clienteId
                );

                var domainEx = new ExternalServiceException(
                    $"Error inesperado al validar con el servicio externo: {ex.Message}",
                    "ServicioValidacion",
                    ex
                );
                domainEx.AddMetadata("ClienteId", clienteId);
                throw domainEx;
            }
        }

        /// <summary>
        /// Valida que la respuesta exitosa del servicio externo contenga datos válidos.
        /// </summary>
        private async Task ValidarRespuestaExitosa(HttpResponseMessage response, int clienteId)
        {
            UserResponse? usuario;

            try
            {
                usuario = await response.Content.ReadFromJsonAsync<UserResponse>();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al deserializar respuesta del servicio externo para cliente {ClienteId}",
                    clienteId
                );

                var deserializeEx = new ExternalServiceException(
                    "El servicio de validación retornó una respuesta en formato inválido",
                    "ServicioValidacion",
                    ex
                );
                deserializeEx.AddMetadata("ClienteId", clienteId);
                throw deserializeEx;
            }

            if (usuario == null || usuario.Id == 0)
            {
                _logger.LogWarning(
                    "Respuesta del servicio externo inválida o vacía para cliente {ClienteId}",
                    clienteId
                );

                var ex = new ExternalServiceException(
                    "El servicio de validación retornó datos incompletos o inválidos",
                    "ServicioValidacion"
                );
                ex.AddMetadata("ClienteId", clienteId);
                ex.AddMetadata("UsuarioRecibido", usuario?.Id ?? 0);
                throw ex;
            }

            _logger.LogInformation(
                "Cliente validado exitosamente - Id: {ClienteId}, Nombre: {NombreCliente}, Email: {Email}",
                usuario.Id,
                usuario.Name,
                usuario.Email
            );
        }

        /// <summary>
        /// Método genérico para obtener configuración desde appsettings.
        /// Lanza ConfigurationException si la configuración no existe o es inválida.
        /// </summary>
        private T ObtenerConfiguracion<T>(string key)
        {
            try
            {
                var value = _configuration[key];

                if (string.IsNullOrWhiteSpace(value))
                {
                    var errorMessage = $"La configuración '{key}' es requerida pero no se encontró en appsettings.json";
                    _logger.LogCritical(errorMessage);

                    throw new ConfigurationException(
                        errorMessage,
                        key
                    );
                }

                var convertedValue = (T)Convert.ChangeType(value, typeof(T));

                _logger.LogInformation(
                    "Configuración '{Key}' cargada exitosamente: {Value}",
                    key,
                    convertedValue
                );

                return convertedValue;
            }
            catch (ConfigurationException)
            {
                throw;
            }
            catch (FormatException formatEx)
            {
                var errorMessage = $"La configuración '{key}' tiene un formato inválido para tipo {typeof(T).Name}";
                _logger.LogCritical(formatEx, errorMessage);

                var ex = new ConfigurationException(errorMessage, key, formatEx);
                ex.AddMetadata("ValorActual", _configuration[key] ?? "null");
                ex.AddMetadata("TipoEsperado", typeof(T).Name);
                throw ex;
            }
            catch (InvalidCastException castEx)
            {
                var errorMessage = $"La configuración '{key}' no se pudo convertir al tipo {typeof(T).Name}";
                _logger.LogCritical(castEx, errorMessage);

                var ex = new ConfigurationException(errorMessage, key, castEx);
                ex.AddMetadata("ValorActual", _configuration[key] ?? "null");
                ex.AddMetadata("TipoEsperado", typeof(T).Name);
                throw ex;
            }
            catch (Exception ex)
            {
                var errorMessage = $"Error inesperado al leer la configuración '{key}'";
                _logger.LogCritical(ex, errorMessage);

                throw new ConfigurationException(errorMessage, key, ex);
            }
        }
    }
}