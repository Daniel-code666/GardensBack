using GardensBack.Application.Common.Results;
using GardensBack.Application.DeviceProvisioning.Abstractions;
using GardensBack.Application.DeviceProvisioning.Dtos.Web;
using Microsoft.AspNetCore.Mvc;

namespace GardensBack.Controllers
{
    using GardensBack.Application.DeviceProvisioning.Dtos.Device;
    using Microsoft.AspNetCore.Authorization;
    using System.IdentityModel.Tokens.Jwt;
    using System.Net.Http.Headers;
    using System.Security.Claims;

    /// <summary>
    /// Controlador para provisionar el dispositivo
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DeviceProvisioningController(IDeviceProvisioningService deviceService) : ControllerBase
    {
        private readonly IDeviceProvisioningService _deviceService = deviceService;

        /// <summary>
        /// Crea o reinicia el aprovisionamiento de un ESP identificado por el cliente serial.
        /// </summary>
        /// <param name="request">Identidad y datos descriptivos obtenidos del ESP.</param>
        /// <param name="ct">Token de cancelación de la solicitud.</param>
        /// <returns>Los datos temporales necesarios para construir el bootstrap serial.</returns>
        [HttpPost("start")]
        public async Task<ActionResult<StartDeviceProvisioningResponse>> Start([FromBody] StartDeviceProvisioningRequest request, CancellationToken ct)
        {
            if (!int.TryParse(User.FindFirstValue(JwtRegisteredClaimNames.Sub), out int userId))
                return Unauthorized();

            Response.Headers.CacheControl = "no-store";

            return Ok(await _deviceService.StartProvisioning(userId, request, ct));
        }

        /// <summary>
        /// Registra el claim para el broker
        /// </summary>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpPost("claims")]
        [AllowAnonymous]
        public async Task<ActionResult<ClaimResponse>> RegisterClaim([FromBody] ClaimRegistrationRequest request, CancellationToken ct)
        {
            // llamar a GetPorivisioningToken
            if (!TryGetProvisioningToken(out string? token))
                return Unauthorized();

            return Ok(await _deviceService.RegisterClaim(token, request, ct));
        }

        /// <summary>
        /// Devuelve, si existe, el claim
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("claims/{requestId}")]
        public async Task<ActionResult<ClaimResponse>> GetClaim(string requestId, CancellationToken ct)
        {
            if (!TryGetProvisioningToken(out string? token))
                return Unauthorized();

            return Ok(await _deviceService.GetClaim(token, requestId, ct));
        }

        /// <summary>
        /// Registra las credenciales para el broker
        /// </summary>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("mqtt-credentials")]
        public async Task<ActionResult<MqttCredentialsEnvelopeResponse>> RegisterMqttCredentials([FromBody] MqttCredentialsRegistrationRequest request, CancellationToken ct)
        {
            if (!TryGetProvisioningToken(out string token))
                return Unauthorized();

            return Ok(await _deviceService.RegisterMqttCredentials(token, request, ct));
        }

        /// <summary>
        /// Obtiene las credenciales para el broker
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("mqtt-credentials/{requestId}")]
        public async Task<ActionResult<MqttCredentialsEnvelopeResponse>> GetMqttCredentials(string requestId, CancellationToken ct)
        {
            if (!TryGetProvisioningToken(out string token))
                return Unauthorized();

            return Ok(await _deviceService.GetMqttCredentials(token, requestId, ct));
        }

        private bool TryGetProvisioningToken(out string provisioningToken)
        {
            provisioningToken = string.Empty;

            if (!AuthenticationHeaderValue.TryParse(Request.Headers.Authorization, out AuthenticationHeaderValue? authorization)
                || !authorization.Scheme.Equals("Bearer", StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(authorization.Parameter))
                return false;

            provisioningToken = authorization.Parameter;
            return true;
        }
    }
}
