using Microsoft.AspNetCore.Mvc;
using SistemaPedidos.Application.DTOs;
using SistemaPedidos.Application.Interfaces;

namespace SistemaPedidos.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class PedidosController : ControllerBase
    {
        private readonly IPedidoService _pedidoService;
        private readonly ILogger<PedidosController> _logger;

        public PedidosController(
            IPedidoService pedidoService,
            ILogger<PedidosController> logger)
        {
            _pedidoService = pedidoService;
            _logger = logger;
        }

        /// <summary>
        /// Registra un nuevo pedido en el sistema
        /// </summary>
        /// <param name="request">Datos del pedido a registrar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Información del pedido creado</returns>
        /// <response code="201">Pedido creado exitosamente</response>
        /// <response code="400">Datos de entrada inválidos o pedido rechazado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpPost]
        [ProducesResponseType(typeof(PedidoResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PedidoResponse>> RegistrarPedido(
            [FromBody] PedidoRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "Recibiendo solicitud de pedido - Cliente: {ClienteId}, Usuario: {Usuario}",
                    request.ClienteId,
                    request.Usuario
                );

                // Validar modelo
                if (!ModelState.IsValid)
                {
                    var errores = ModelState
                        .Where(x => x.Value?.Errors.Any() ?? false)
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                        );

                    var errorResponse = new ErrorResponse
                    {
                        Mensaje = "Datos de entrada inválidos",
                        Errores = errores
                    };

                    _logger.LogWarning(
                        "Validación fallida para pedido - Cliente: {ClienteId}, Errores: {@Errores}",
                        request.ClienteId,
                        errores
                    );

                    return BadRequest(errorResponse);
                }

                // Validaciones adicionales
                if (request.Items == null || !request.Items.Any())
                {
                    return BadRequest(new ErrorResponse
                    {
                        Mensaje = "El pedido debe contener al menos un item"
                    });
                }

                if (request.Items.Any(item => item.Cantidad <= 0))
                {
                    return BadRequest(new ErrorResponse
                    {
                        Mensaje = "La cantidad de cada item debe ser mayor a 0"
                    });
                }

                if (request.Items.Any(item => item.Precio <= 0))
                {
                    return BadRequest(new ErrorResponse
                    {
                        Mensaje = "El precio de cada item debe ser mayor a 0"
                    });
                }

                // Procesar pedido
                var resultado = await _pedidoService.RegistrarPedidoAsync(request, cancellationToken);

                // Si no fue exitoso, retornar BadRequest
                if (!resultado.Exito)
                {
                    _logger.LogWarning(
                        "Pedido rechazado - Cliente: {ClienteId}, Motivo: {Mensaje}",
                        request.ClienteId,
                        resultado.Mensaje
                    );

                    return BadRequest(new ErrorResponse
                    {
                        Mensaje = resultado.Mensaje,
                        Detalle = $"Cliente: {resultado.ClienteId}, Total: {resultado.Total:C}"
                    });
                }

                _logger.LogInformation(
                    "Pedido creado exitosamente - PedidoId: {PedidoId}, Cliente: {ClienteId}, Total: {Total:C}",
                    resultado.PedidoId,
                    resultado.ClienteId,
                    resultado.Total
                );

                // Retornar 201 Created con la ubicación del recurso
                return CreatedAtAction(
                    nameof(RegistrarPedido),
                    new { id = resultado.PedidoId },
                    resultado
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error interno al procesar pedido - Cliente: {ClienteId}",
                    request?.ClienteId
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new ErrorResponse
                    {
                        Mensaje = "Error interno al procesar el pedido",
                        Detalle = ex.Message
                    }
                );
            }
        }

        /// <summary>
        /// Verifica el estado de salud del servicio
        /// </summary>
        /// <returns>Estado del servicio</returns>
        [HttpGet("health")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Health()
        {
            _logger.LogInformation("Health check ejecutado");

            return Ok(new
            {
                status = "healthy",
                service = "Sistema de Pedidos API",
                timestamp = DateTime.Now,
                version = "1.0.0"
            });
        }
    }
}