using GardensBack.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace GardensBack.Application.Abstractions
{
    public interface IFirmwareReleasePublicationValidator
    {
        /// <summary>
        /// Valida los metadatos del cuerpo de la petición
        /// </summary>
        /// <param name="release"></param>
        /// <param name="ct"></param>
        void ValidateRelease(FirmwareRelease release, CancellationToken ct);

        /// <summary>
        /// Valida el sha y el tamaño del archivo del firmware desde el artefacto convertido a byte[] comparando con lo almacenado en la base de datos.
        /// </summary>
        /// <param name="content"></param>
        /// <param name="requestHash"></param>
        byte[] ValidateArtifactFromDb(FirmwareRelease release, byte[] content);
    }
}
