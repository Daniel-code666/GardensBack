using GardensBack.Application.DeviceConnectivity.Abstractions;
using GardensBack.Application.DeviceConnectivity.Dtos;
using GardensBack.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GardensBack.Controllers
{
    /// <summary>
    /// Administra las direcciones que se anuncian a los dispositivos durante su aprovisionamiento.
    /// </summary>
    /// <param name="service">Servicio de configuración de conectividad.</param>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = BaseRoles.Admin)]
    public class DeviceConnectivityConfigurationController(IDeviceConnectivityConfigurationService service) : ControllerBase
    {
        private readonly IDeviceConnectivityConfigurationService _service = service;

        /// <summary>
        /// Obtiene la configuración global de conectividad para dispositivos.
        /// </summary>
        /// <param name="ct">Token de cancelación de la solicitud.</param>
        /// <returns>La configuración que actualmente se anuncia a los dispositivos.</returns>
        [HttpGet]
        public async Task<ActionResult<DeviceConnectivityConfigurationResponse>> Get(CancellationToken ct)
            => Ok(await _service.Get(ct));

        /// <summary>
        /// Crea o actualiza la configuración global de conectividad para dispositivos.
        /// </summary>
        /// <param name="request">Direcciones y puertos que pueden utilizar los dispositivos.</param>
        /// <param name="ct">Token de cancelación de la solicitud.</param>
        /// <returns>La configuración guardada.</returns>
        [HttpPut]
        public async Task<ActionResult<DeviceConnectivityConfigurationResponse>> Save([FromBody] SaveDeviceConnectivityConfigurationRequest request, CancellationToken ct)
            => Ok(await _service.Save(request, ct));
    }
}
