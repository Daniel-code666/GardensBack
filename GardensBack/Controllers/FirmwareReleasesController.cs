using GardensBack.Application.FirmwareReleases.Abstractions;
using GardensBack.Application.FirmwareReleases.Dtos;
using GardensBack.Domain.Constants;
using GardensBack.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GardensBack.Controllers
{
    /// <summary>
    /// Administra las publicaciones de firmware disponibles para los dispositivos.
    /// </summary>
    /// <param name="service">Servicio de aplicación para la gestión de publicaciones de firmware.</param>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = BaseRoles.Admin)]
    public class FirmwareReleasesController(IFirmwareReleaseService service) : ControllerBase
    {
        private readonly IFirmwareReleaseService _service = service;

        /// <summary>
        /// Crea una publicación de firmware en estado borrador.
        /// </summary>
        /// <param name="request">Metadatos y datos de integridad del firmware.</param>
        /// <param name="ct">Token de cancelación de la solicitud.</param>
        /// <returns>La publicación de firmware creada.</returns>
        [HttpPost]
        public async Task<ActionResult<FirmwareReleaseResponse>> Create([FromBody] CreateFirmwareReleaseRequest request, CancellationToken ct)
            => Ok(await _service.Create(request, ct));

        /// <summary>
        /// Actualiza una publicación de firmware que permanece en estado borrador.
        /// </summary>
        /// <param name="request">Metadatos actualizados de la publicación.</param>
        /// <param name="ct">Token de cancelación de la solicitud.</param>
        /// <returns>La publicación de firmware actualizada.</returns>
        [HttpPut]
        public async Task<ActionResult<FirmwareReleaseResponse>> Update([FromBody] UpdateFirmwareReleaseRequest request, CancellationToken ct)
            => Ok(await _service.Update(request, ct));

        /// <summary>
        /// Obtiene una publicación de firmware mediante su identificador público.
        /// </summary>
        /// <param name="releaseId">Identificador público de la publicación.</param>
        /// <param name="ct">Token de cancelación de la solicitud.</param>
        /// <returns>La publicación de firmware encontrada.</returns>
        [HttpGet("{releaseId}")]
        public async Task<ActionResult<FirmwareReleaseResponse>> GetByReleaseId(string releaseId, CancellationToken ct)
            => Ok(await _service.GetByReleaseId(releaseId, ct));

        /// <summary>
        /// Obtiene las publicaciones de firmware, con filtros opcionales por placa y estado.
        /// </summary>
        /// <param name="board">Placa compatible por la cual filtrar.</param>
        /// <param name="status">Estado de publicación por el cual filtrar.</param>
        /// <param name="ct">Token de cancelación de la solicitud.</param>
        /// <returns>Las publicaciones de firmware que cumplen los filtros.</returns>
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<FirmwareReleaseResponse>>> GetAll([FromQuery] string? board, [FromQuery] FirmwareReleaseStatus? status, CancellationToken ct)
            => Ok(await _service.GetAll(board, status, ct));

        /// <summary>
        /// Publica una versión de firmware que se encuentra en estado borrador.
        /// </summary>
        /// <param name="releaseId">Identificador público de la publicación.</param>
        /// <param name="ct">Token de cancelación de la solicitud.</param>
        /// <returns>La publicación de firmware publicada.</returns>
        [HttpPost("{releaseId}/publish")]
        public async Task<ActionResult<FirmwareReleaseResponse>> Publish(string releaseId, CancellationToken ct)
            => Ok(await _service.Publish(releaseId, ct));

        /// <summary>
        /// Revoca una versión de firmware publicada.
        /// </summary>
        /// <param name="releaseId">Identificador público de la publicación.</param>
        /// <param name="ct">Token de cancelación de la solicitud.</param>
        /// <returns>La publicación de firmware revocada.</returns>
        [HttpPost("{releaseId}/revoke")]
        public async Task<ActionResult<FirmwareReleaseResponse>> Revoke(string releaseId, CancellationToken ct)
            => Ok(await _service.Revoke(releaseId, ct));
    }
}
