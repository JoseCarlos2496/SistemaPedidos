using Microsoft.AspNetCore.Mvc;
using SistemaPedidos.Application.DTOs;
using SistemaPedidos.Application.Interfaces;

namespace SistemaPedidos.API.Controllers
{
    /// <summary>
    /// Controlador REST para la gestión de pedidos.
    /// Expone endpoints HTTP para crear, consultar y verificar el estado del sistema de pedidos.
    /// Implementa el patrón API REST siguiendo las mejores prácticas de ASP.NET Core 8.
    /// </summary>
    /// <remarks>
    /// Este controlador delega toda la lógica de negocio al servicio de aplicación (IPedidoService).
    /// Su única responsabilidad es:
    /// - Validar el ModelState de las peticiones HTTP
    /// - Invocar los servicios de aplicación
    /// - Mapear resultados a respuestas HTTP apropiadas
    /// 
    /// El manejo de excepciones está centralizado en GlobalExceptionHandlerMiddleware.
    /// </remarks>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class PedidosController : ControllerBase
    {
        private readonly IPedidoService _pedidoService;
        private readonly ILogger<PedidosController> _logger;

        /// <summary>
        /// Constructor del controlador de pedidos.
        /// </summary>
        /// <param name="pedidoService">Servicio de aplicación para operaciones de pedidos</param>
        /// <param name="logger">Logger para registro de eventos HTTP</param>
        public PedidosController(
            IPedidoService pedidoService,
            ILogger<PedidosController> logger)
        {
            _pedidoService = pedidoService;
            _logger = logger;
        }

        /// <summary>
        /// Registra un nuevo pedido en el sistema de forma transaccional.
        /// </summary>
        /// <remarks>
        /// Este endpoint realiza las siguientes operaciones:
        /// 1. Valida el formato de los datos de entrada (ModelState)
        /// 2. Valida las reglas de negocio del pedido
        /// 3. Verifica la existencia del cliente en el servicio externo
        /// 4. Calcula el total del pedido
        /// 5. Guarda el pedido en base de datos dentro de una transacción
        /// 6. Registra auditoría completa de la operación
        /// 
        /// El proceso es completamente transaccional: si cualquier paso falla,
        /// se ejecuta rollback automático y se retorna un error apropiado.
        /// 
        /// Ejemplo de request válido:
        /// {
        /// 	"clienteId": 1,
        /// 	"usuario": "usuario.prueba",
        /// 	"items": [
        /// 		{
        /// 			"productoId": 1,
        /// 			"cantidad": 2,
        /// 			"precio": 10
        /// 
        ///         },
        /// 		{
        /// 			"productoId": 2,
        /// 			"cantidad": 1,
        /// 			"precio": 20
        /// 		}
        /// 	]
        /// }
        /// </code>
        /// </remarks>
        /// <param name="request">Datos del pedido incluyendo cliente, usuario y lista de items</param>
        /// <param name="cancellationToken">Token para cancelación cooperativa de la operación asíncrona</param>
        /// <returns>
        /// ActionResult con PedidoResponse conteniendo:
        /// - PedidoId: ID generado por la base de datos
        /// - ClienteId: ID del cliente que realizó el pedido
        /// - Fecha: Timestamp de creación
        /// - Total: Monto total calculado
        /// - Usuario: Usuario que registró el pedido
        /// - CantidadItems: Número de líneas en el pedido
        /// </returns>
        /// <response code="201">Pedido creado exitosamente. Retorna Location header con URL del recurso creado.</response>
        /// <response code="400">Error de regla de negocio (ej: cliente no existe, límite excedido)</response>
        /// <response code="422">Datos de entrada con formato inválido o valores fuera de rango permitido</response>
        /// <response code="500">Error interno del servidor (base de datos, transacción, configuración)</response>
        /// <response code="503">Servicio de validación externa temporalmente no disponible</response>
        [HttpPost]
        [ProducesResponseType(typeof(PedidoResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status503ServiceUnavailable)]
        public async Task<ActionResult<PedidoResponse>> RegistrarPedido(
            [FromBody] PedidoRequest request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "=== REQUEST RECIBIDO === Cliente: {ClienteId}, Usuario: {Usuario}",
                request?.ClienteId,
                request?.Usuario
            );

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState inválido");
                return UnprocessableEntity(CrearErrorResponseValidacion());
            }

            var resultado = await _pedidoService.RegistrarPedidoAsync(request!, cancellationToken);

            _logger.LogInformation(
                "=== PEDIDO CREADO === PedidoId: {PedidoId}, Total: ${Total}",
                resultado.PedidoId,
                resultado.Total
            );

            return CreatedAtAction(
                nameof(ObtenerPedido),
                new { id = resultado.PedidoId },
                resultado
            );
        }

        /// <summary>
        /// Obtiene un pedido específico por su identificador único.
        /// </summary>
        /// <remarks>
        /// Este es un endpoint placeholder para satisfacer la generación del Location header
        /// en el método CreatedAtAction del endpoint de registro de pedidos.
        /// 
        /// ESTADO ACTUAL: No implementado
        /// TODO: Implementar lógica de consulta cuando se requiera funcionalidad de lectura.
        /// </remarks>
        /// <param name="id">Identificador único del pedido a consultar</param>
        /// <returns>
        /// Información completa del pedido incluyendo sus items.
        /// Actualmente retorna 404 Not Found con mensaje de no implementado.
        /// </returns>
        /// <response code="200">Pedido encontrado y retornado exitosamente</response>
        /// <response code="404">Pedido no encontrado o endpoint no implementado</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(PedidoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PedidoResponse>> ObtenerPedido(int id)
        {
            await Task.CompletedTask;
            return NotFound(new ErrorResponse
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Endpoint no implementado aún",
                ErrorCode = "NOT_IMPLEMENTED",
                Timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Verifica el estado de salud del servicio de pedidos (health check).
        /// </summary>
        /// <remarks>
        /// Este endpoint se utiliza para:
        /// - Monitoreo de disponibilidad del servicio
        /// - Load balancers para verificar instancias activas
        /// - Health checks de Kubernetes/Docker
        /// - Pipelines de CI/CD
        /// 
        /// Retorna información básica del servicio:
        /// - Estado: "healthy" si el servicio responde
        /// - Nombre del servicio
        /// - Timestamp actual
        /// - Versión de la API
        /// - Ambiente de ejecución (Development/Production)
        /// 
        /// NOTA: Este health check es básico. Para verificación completa de dependencias
        /// (base de datos, servicios externos), considerar implementar IHealthCheck de ASP.NET Core.
        /// </remarks>
        /// <returns>
        /// Objeto JSON con información del estado del servicio
        /// </returns>
        /// <response code="200">Servicio operando correctamente</response>
        [HttpGet("health")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Health()
        {
            return Ok(new
            {
                status = "healthy",
                service = "Sistema de Pedidos API",
                timestamp = DateTime.UtcNow,
                version = "1.0.0",
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
            });
        }

        /// <summary>
        /// Crea una respuesta de error estandarizada para errores de validación del ModelState.
        /// </summary>
        /// <remarks>
        /// Este método procesa el ModelState de ASP.NET Core y extrae todos los errores
        /// de validación de Data Annotations, mapeándolos a un formato estructurado.
        /// 
        /// El formato de salida agrupa los errores por campo:
        /// {
        ///   "ClienteId": ["El ClienteId es requerido", "El ClienteId debe ser mayor a 0"],
        ///   "Usuario": ["El Usuario es requerido"]
        /// }
        /// 
        /// Este método se invoca solo cuando ModelState.IsValid es false.
        /// </remarks>
        /// <returns>
        /// ErrorResponse con código 422, mensaje descriptivo y diccionario de errores por campo
        /// </returns>
        private ErrorResponse CrearErrorResponseValidacion()
        {
            var errores = ModelState
                .Where(x => x.Value?.Errors.Any() ?? false)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                );

            return new ErrorResponse
            {
                StatusCode = StatusCodes.Status422UnprocessableEntity,
                Message = "Los datos de entrada no cumplen con el formato requerido",
                ErrorCode = "VALIDATION_ERROR",
                Timestamp = DateTime.UtcNow,
                ValidationErrors = errores
            };
        }
    }
}