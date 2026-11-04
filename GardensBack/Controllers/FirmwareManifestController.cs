using GardensBack.Application.FirmwareReleases.Abstractions;
using GardensBack.Application.FirmwareReleases.Constants;
using GardensBack.Application.FirmwareReleases.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

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
        {
            FirmwareManifestResponse manifest = await _service.GetPublishedManifest(releaseId, ct);
            byte[] content = JsonSerializer.SerializeToUtf8Bytes(manifest);

            return File(content, "application/json");
        }

        /// <summary>
        /// Obtiene el .bin almacenado en db según el releaseId
        /// </summary>
        /// <param name="releaseId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet(FirmwareReleaseContract.ArtifactRoute)]
        public async Task<ActionResult<byte[]>> GetArtifact(string releaseId, CancellationToken ct)
        {
            byte[] artifact = await _service.GetArtifactToDownload(releaseId, ct);
            return File(artifact, "application/octet-stream");
        }
    }
}
