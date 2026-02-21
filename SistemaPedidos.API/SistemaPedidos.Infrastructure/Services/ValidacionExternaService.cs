using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SistemaPedidos.Domain.Interfaces;
using SistemaPedidos.Infrastructure.Constants;
using SistemaPedidos.Infrastructure.Models;
using System.Net;
using System.Net.Http.Json;

namespace SistemaPedidos.Infrastructure.Services
{
    public class ValidacionExternaService : IValidacionExternaService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ValidacionExternaService> _logger;
        private readonly IConfiguration _configuration;
        
        // Variables de instancia que se cargan desde configuración
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
                "ValidacionExternaService inicializado - BaseUrl: {BaseUrl}, Timeout: {Timeout}s, Límite: {Limite:C}",
                _baseUrl,
                _timeoutSeconds,
                _limiteTotal
            );
        }

        public async Task<bool> ValidarPedidoAsync(int clienteId, decimal total)
        {
            try
            {
                _logger.LogInformation(
                    "Iniciando validación de pedido - Cliente: {ClienteId}, Total: {Total:C}",
                    clienteId,
                    total
                );

                // Validaciones básicas usando valores predeterminados
                if (clienteId < ConfigurationKeys.CLIENTE_ID_MINIMO)
                {
                    _logger.LogWarning("Cliente inválido: {ClienteId}", clienteId);
                    return false;
                }

                if (total <= ConfigurationKeys.TOTAL_MINIMO)
                {
                    _logger.LogWarning("Total inválido: {Total}", total);
                    return false;
                }

                if (total > _limiteTotal)
                {
                    _logger.LogWarning(
                        "Total {Total:C} excede el límite permitido de {Limite:C}",
                        total,
                        _limiteTotal
                    );
                    return false;
                }

                // Validar cliente con servicio externo
                var clienteValido = await ValidarClienteExternoAsync(clienteId);

                if (!clienteValido)
                {
                    _logger.LogWarning(
                        "Cliente {ClienteId} no existe en el servicio externo",
                        clienteId
                    );
                    return false;
                }

                _logger.LogInformation(
                    "Pedido validado exitosamente - Cliente: {ClienteId}, Total: {Total:C}",
                    clienteId,
                    total
                );

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error inesperado al validar pedido - Cliente: {ClienteId}",
                    clienteId
                );
                return false;
            }
        }

        private async Task<bool> ValidarClienteExternoAsync(int clienteId)
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

                // Si el cliente no existe, JSONPlaceholder devuelve 404
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    _logger.LogWarning(
                        "Cliente {ClienteId} no encontrado en servicio externo",
                        clienteId
                    );
                    return false;
                }

                // Validar que la respuesta sea exitosa
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "Servicio externo respondió con código {StatusCode}",
                        response.StatusCode
                    );
                    return false;
                }

                var usuario = await response.Content.ReadFromJsonAsync<UserResponse>();

                if (usuario == null || usuario.Id == 0)
                {
                    _logger.LogWarning(
                        "Respuesta del servicio externo inválida para cliente {ClienteId}",
                        clienteId
                    );
                    return false;
                }

                _logger.LogInformation(
                    "Cliente validado exitosamente - Id: {ClienteId}, Nombre: {NombreCliente}, Email: {Email}",
                    usuario.Id,
                    usuario.Name,
                    usuario.Email
                );

                return true;
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(
                    httpEx,
                    "Error de conexión con el servicio externo para cliente {ClienteId}",
                    clienteId
                );
                return false;
            }
            catch (TaskCanceledException timeoutEx)
            {
                _logger.LogError(
                    timeoutEx,
                    "Timeout al consultar servicio externo para cliente {ClienteId}",
                    clienteId
                );
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al procesar respuesta del servicio externo para cliente {ClienteId}",
                    clienteId
                );
                return false;
            }
        }

        /// <summary>
        /// Método genérico para obtener configuración desde appsettings
        /// Lanza excepción si la configuración no existe
        /// </summary>
        private T ObtenerConfiguracion<T>(string key)
        {
            try
            {
                var value = _configuration[key];
                
                if (string.IsNullOrWhiteSpace(value))
                {
                    var errorMessage = $"La configuración '{key}' es requerida pero no se encontró en appsettings.json. " +
                                      $"Por favor, agregue la clave '{key}' con un valor válido.";
                    
                    _logger.LogCritical(errorMessage);
                    throw new InvalidOperationException(errorMessage);
                }

                // Convertir el valor al tipo deseado
                var convertedValue = (T)Convert.ChangeType(value, typeof(T));
                
                _logger.LogInformation(
                    "Configuración '{Key}' cargada exitosamente: {Value}",
                    key,
                    convertedValue
                );
                
                return convertedValue;
            }
            catch (InvalidOperationException)
            {
                // Re-lanzar excepciones de configuración faltante
                throw;
            }
            catch (FormatException formatEx)
            {
                var errorMessage = $"La configuración '{key}' tiene un formato inválido. " +
                                  $"No se pudo convertir a tipo {typeof(T).Name}. " +
                                  $"Valor actual: '{_configuration[key]}'";
                
                _logger.LogCritical(formatEx, errorMessage);
                throw new InvalidOperationException(errorMessage, formatEx);
            }
            catch (InvalidCastException castEx)
            {
                var errorMessage = $"La configuración '{key}' no se pudo convertir al tipo {typeof(T).Name}. " +
                                  $"Valor actual: '{_configuration[key]}'";
                
                _logger.LogCritical(castEx, errorMessage);
                throw new InvalidOperationException(errorMessage, castEx);
            }
            catch (Exception ex)
            {
                var errorMessage = $"Error inesperado al leer la configuración '{key}': {ex.Message}";
                
                _logger.LogCritical(ex, errorMessage);
                throw new InvalidOperationException(errorMessage, ex);
            }
        }
    }
}