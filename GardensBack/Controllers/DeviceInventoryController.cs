using GardensBack.Application.DeviceInventory.Abstractions;
using GardensBack.Application.DeviceInventory.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GardensBack.Controllers
{
    /// <summary>
    /// Controlador para la gestión de inventario de dispositivos.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
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
            => Ok(await _service.AddDevice(device, ct));

        /// <summary>
        /// Obtiene un dispoistivo por su id
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpGet("GetDeviceById/{deviceId}")]
        public async Task<ActionResult<DeviceInventoryGeneralDto>> GetDeviceById([FromRoute] int deviceId, CancellationToken ct)
            => Ok(await _service.GetDeviceById(deviceId, ct));

        /// <summary>
        /// Devuelve los dispositivos asociados al microcontrolador especificado. Se puede filtrar por un conjunto de ids de dispositivos.
        /// </summary>
        /// <param name="microCtrlId"></param>
        /// <param name="devicesIds"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpGet("GetAllDevicesByMicroControllerId/{microCtrlId}")]
        public async Task<ActionResult<IReadOnlyCollection<DeviceInventoryGeneralDto>>> GetAllDevicesByMicrocontrollerId([FromRoute] int microCtrlId, [FromQuery] int[]? devicesIds, CancellationToken ct)
            => Ok(await _service.GetAllDevicesByMicrocontrollerId(microCtrlId, devicesIds, ct));

        /// <summary>
        /// Cambia el estado de un dispositivo
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="state"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpPut("ChangeDeviceState/{deviceId}/{state}")]
        public async Task<ActionResult<DeviceInventoryGeneralDto>> ChangeDeviceState([FromRoute] int deviceId, [FromRoute] bool state, CancellationToken ct)
            => Ok(await _service.ChangeDeviceState(deviceId, state, ct));

        /// <summary>
        /// Actualiza los datos editables de un sensor o actuador.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpPut("UpdateDevice")]
        public async Task<ActionResult<DeviceInventoryGeneralDto>> UpdateDevice([FromBody] DeviceInventoryGeneralDto device, CancellationToken ct)
            => Ok(await _service.UpdateDevice(device, ct));
    }
}
