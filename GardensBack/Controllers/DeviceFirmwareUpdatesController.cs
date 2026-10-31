using GardensBack.Application.FirmwareUpdates.Abstractions;
using GardensBack.Application.FirmwareUpdates.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace GardensBack.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DeviceFirmwareUpdatesController(IDeviceFirmwareUpdateService service) : ControllerBase
    {
        private readonly IDeviceFirmwareUpdateService _service = service;

        /// <summary>
        /// COntrolador de las actualizaciones del firmware del esp
        /// </summary>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<DeviceFirmwareUpdateResponse>> RequestUpdate([FromBody] RequestDeviceFirmwareUpdateRequest request, CancellationToken ct)
        {
            if (!int.TryParse(User.FindFirstValue(JwtRegisteredClaimNames.Sub), out int userId))
                return Unauthorized();

            DeviceFirmwareUpdateResponse response =
                await _service.RequestUpdate(request, userId, ct);

            return Accepted(response);
        }
    }
}
