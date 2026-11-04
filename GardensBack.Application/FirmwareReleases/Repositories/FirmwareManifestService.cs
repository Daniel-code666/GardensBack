using AutoMapper;
using GardensBack.Application.Abstractions;
using GardensBack.Application.Common.Exceptions;
using GardensBack.Application.FirmwareReleases.Abstractions;
using GardensBack.Application.FirmwareReleases.Dtos;
using GardensBack.Domain.Entities;

namespace GardensBack.Application.FirmwareReleases.Repositories
{
    public class FirmwareManifestService(IFirmwareReleaseRepository firmwareRelease, IFirmwareReleasePublicationValidator validator, IMapper mapper) : IFirmwareManifestService
    {
        private readonly IFirmwareReleaseRepository _firmwareRelease = firmwareRelease;
        private readonly IFirmwareReleasePublicationValidator _validator = validator;
        private readonly IMapper _mapper = mapper;

        public async Task<FirmwareManifestResponse> GetPublishedManifest(string releaseId, CancellationToken ct)
        {
            ValidateReleaseId(releaseId);

            FirmwareRelease foundFirmware = await _firmwareRelease.FindPublishedByReleaseId(releaseId, false, ct) ?? throw new NotFoundException("Publicacion no encontrada");

            return _mapper.Map<FirmwareManifestResponse>(foundFirmware);
        }

        public async Task<byte[]> GetArtifactToDownload(string releaseId, CancellationToken ct)
        {
            ValidateReleaseId(releaseId);

            FirmwareRelease foundFirmware = await _firmwareRelease.FindPublishedByReleaseId(releaseId, true, ct) ?? throw new NotFoundException("Publicacion no encontrada");

            if (foundFirmware.Artifact != null && foundFirmware.Artifact.Content is byte[] content)
            {
                _validator.ValidateArtifactFromDb(content, foundFirmware.SizeBytes);
                _validator.ValidateHashFromArtifact(content, foundFirmware.Sha256);

                return content;
            }

            throw new NotFoundException("No se encontro el artefacto para la publicacion solicitada");
        }

        private static void ValidateReleaseId(string releaseId)
        {
            if (string.IsNullOrEmpty(releaseId))
                throw new ValidationException("Release Id obligatorio");

            if (releaseId.Length > 64)
                throw new ValidationException("El ReleaseId no puede superar 64 caracteres.");

            bool valid = releaseId.All(character => char.IsAsciiLetterOrDigit(character) || character == '-' || character == '_');

            if (!valid)
                throw new ValidationException("El ReleaseId contiene caracteres no permitidos.");
        }
    }
}
