using GardensBack.Application.DeviceInventory.Abstractions;
using GardensBack.Application.DeviceInventory.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace GardensBack.Controllers
{
    /// <summary>
    /// Controlador para la gestión de inventario de dispositivos.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DeviceInventoryController(IDeviceInventoryService service) : ControllerBase
    {
        private readonly IDeviceInventoryService _service = service;

        /// <summary>
        /// Crea un sensor/Actuador
        /// </summary>
        /// <param name="device"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpPost("CreateDevice")]
        public async Task<ActionResult<DeviceInventoryGeneralDto>> CreateDevice([FromBody] DeviceInventoryGeneralDto device, CancellationToken ct)
        {
            if (!TryGetAuthenticatedUserId(out int userId))
                return Unauthorized();

            return Ok(await _service.AddDevice(device, userId, ct));
        }

        /// <summary>
        /// Obtiene un dispoistivo por su id
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpGet("GetDeviceById/{deviceId}")]
        public async Task<ActionResult<DeviceInventoryGeneralDto>> GetDeviceById([FromRoute] int deviceId, CancellationToken ct)
        {
            if (!TryGetAuthenticatedUserId(out int userId))
                return Unauthorized();

            return Ok(await _service.GetDeviceById(deviceId, userId, ct));
        }

        /// <summary>
        /// Devuelve los dispositivos asociados al microcontrolador especificado. Se puede filtrar por un conjunto de ids de dispositivos.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpGet("GetDevices")]
        public async Task<ActionResult<IReadOnlyCollection<DeviceInventoryGeneralDto>>> GetAllDevicesByMicrocontrollerId([FromBody] DeviceInventoryFilters request,
            CancellationToken ct)
        {
            if (!TryGetAuthenticatedUserId(out int userId))
                return Unauthorized();

            return Ok(await _service.GetAllDevicesByMicrocontrollerId(request, userId, ct));
        }

        /// <summary>
        /// Devuelve una lista con los dispositivos y su telemetría según su id. Se puede limitar el número de resultados devueltos
        /// </summary>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpGet("GetDevicesWithTelemetry")]
        public async Task<ActionResult<IEnumerable<DeviceTelemetryDto>>> GetTelemetryForDevices([FromBody] GetTelemetryForDevicesDto request, CancellationToken ct)
        {
            if (!TryGetAuthenticatedUserId(out int userId))
                return Unauthorized();

            return Ok(await _service.GetTelemetryForDevices(request, userId, ct));
        }

        /// <summary>
        /// Cambia el estado de un dispositivo
        /// </summary>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpPut("SetDeviceState")]
        public async Task<ActionResult<DeviceInventoryGeneralDto>> SetDeviceState(SetDevicesStateDto request, CancellationToken ct)
        {
            if (!TryGetAuthenticatedUserId(out int userId))
                return Unauthorized();

            return Ok(await _service.SetDeviceState(request.DevicesIds, request.State, userId, ct));
        }

        /// <summary>
        /// Actualiza los datos editables de un sensor o actuador.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpPut("UpdateDevice")]
        public async Task<ActionResult<DeviceInventoryGeneralDto>> UpdateDevice([FromBody] DeviceInventoryGeneralDto device, CancellationToken ct)
        {
            if (!TryGetAuthenticatedUserId(out int userId))
                return Unauthorized();

            return Ok(await _service.UpdateDevice(device, userId, ct));
        }

        /// <summary>
        /// Publica un comando para uno o varios dispositivos pertenecientes al mismo microcontrolador.
        /// </summary>
        [HttpPost("Commands")]
        [ProducesResponseType(typeof(DeviceCommandExecutionResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(DeviceCommandExecutionResponseDto), StatusCodes.Status202Accepted)]
        public async Task<ActionResult<DeviceCommandExecutionResponseDto>> ExecuteCommand(
            [FromBody] DeviceCommandRequestDto request,
            [FromQuery] bool waitForResponse = false,
            CancellationToken ct = default)
        {
            if (!TryGetAuthenticatedUserId(out int userId))
                return Unauthorized();

            DeviceCommandExecutionResponseDto response = await _service.CreateDeviceCommand(
                request,
                userId,
                waitForResponse,
                ct);

            if (response.Responded)
                return Ok(response);

            return AcceptedAtAction(
                nameof(GetDeviceCommandById),
                new { commandId = response.Command.Id },
                response);
        }

        /// <summary>
        /// Consulta el estado de un comando por su identificador interno.
        /// </summary>
        [HttpGet("Commands/{commandId:int}")]
        public async Task<ActionResult<DeviceCommandResponseDto>> GetDeviceCommandById([FromRoute] int commandId, CancellationToken ct)
        {
            if (!TryGetAuthenticatedUserId(out int userId))
                return Unauthorized();

            return Ok(await _service.GetDeviceCommandById(commandId, userId, ct));
        }

        /// <summary>
        /// Consulta el estado de un comando por el identificador MQTT del microcontrolador y el requestId.
        /// </summary>
        [HttpGet("Commands/by-request/{deviceId}/{requestId}")]
        public async Task<ActionResult<DeviceCommandResponseDto>> GetDeviceCommandByRequest([FromRoute] string deviceId, [FromRoute] string requestId, CancellationToken ct)
        {
            if (!TryGetAuthenticatedUserId(out int userId))
                return Unauthorized();

            return Ok(await _service.GetDeviceCommandByRequest(deviceId, requestId, userId, ct));
        }

        private bool TryGetAuthenticatedUserId(out int userId)
            => int.TryParse(User.FindFirstValue(JwtRegisteredClaimNames.Sub), out userId);
    }
}
