using GardensBack.Application.FirmwareReleases.Abstractions;
using GardensBack.Application.FirmwareReleases.Constants;
using GardensBack.Application.FirmwareReleases.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GardensBack.Controllers
{
    /// <summary>
    /// Entrega a los dispositivos los manifiestos de firmware publicados.
    /// </summary>
    /// <param name="service">Servicio de consulta de manifiestos.</param>
    [Route(FirmwareReleaseContract.ControllerRoute)]
    [ApiController]
    public class FirmwareManifestController(IFirmwareManifestService service) : ControllerBase
    {
        private readonly IFirmwareManifestService _service = service;

        /// <summary>
        /// Obtiene el manifiesto firmado de una publicación de firmware.
        /// </summary>
        /// <param name="releaseId">Identificador público de la publicación.</param>
        /// <param name="ct">Token de cancelación de la solicitud.</param>
        /// <returns>El manifiesto OTA consumido por el dispositivo.</returns>
        [AllowAnonymous]
        [HttpGet(FirmwareReleaseContract.ManifestRoute)]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        [Produces("application/json")]
        public async Task<ActionResult<FirmwareManifestResponse>> Get(string releaseId, CancellationToken ct)
            => Ok(await _service.GetPublishedManifest(releaseId, ct));
    }
}
