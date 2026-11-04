using AutoMapper;
using GardensBack.Application.Abstractions;
using GardensBack.Application.Common.Exceptions;
using GardensBack.Application.DeviceConnectivity.Abstractions;
using GardensBack.Application.FirmwareReleases.Abstractions;
using GardensBack.Application.FirmwareReleases.Dtos;
using GardensBack.Domain.Entities;
using GardensBack.Domain.Enums;
using Microsoft.AspNetCore.Http;
using System.Reflection.Metadata;

namespace GardensBack.Application.FirmwareReleases.Repositories
{
    public class FirmwareReleaseService(IFirmwareReleaseRepository repository, IFirmwareReleasePublicationValidator validator,
        IDeviceConnectivityConfigurationRepository deviceConfig, IMapper mapper) : IFirmwareReleaseService
    {
        private readonly IFirmwareReleaseRepository _repository = repository;
        private readonly IMapper _mapper = mapper;
        private readonly IFirmwareReleasePublicationValidator _validator = validator;
        private readonly IDeviceConnectivityConfigurationRepository _deviceConfig = deviceConfig;

        public async Task<FirmwareReleaseResponse> Create(CreateFirmwareReleaseRequest request, bool publish, CancellationToken ct)
        {
            DeviceConnectivityConfiguration? deviceConfig = await _deviceConfig.GetDefault(ct)
                ?? throw new ValidationException("No se encontró una configuración de conectividad por defecto.");

            if (!deviceConfig.Active)
                throw new ValidationException("La configuración de conectividad por defecto no se encuentra activa.");

            string scheme = deviceConfig.ApiUseTls ? Uri.UriSchemeHttps : Uri.UriSchemeHttp;
            

            request.Validate();

            if (await _repository.ExistsReleaseId(request.ReleaseId, ct))
                throw new ConflictException("Ya existe release id");

            if (await _repository.ExistsBoardVersion(request.Board, request.Version, ct))
                throw new ConflictException("Ya existe esa combinación entre board + versión");

            FirmwareRelease release = _mapper.Map<FirmwareRelease>(request);

            string artifactPath = $"/api/FirmwareReleases/{Uri.EscapeDataString(release.ReleaseId)}/artifact";

            release.Status = FirmwareReleaseStatus.Draft;
            release.PublishedAt = null;
            release.Active = true;
            release.FirmwareUrl = new UriBuilder(scheme, deviceConfig.ApiHost, deviceConfig.ApiPort, artifactPath).Uri.AbsoluteUri;

            _validator.ValidateRelease(release, ct);
            byte[] content = _validator.ValidateArtifactFromDb(release, await ConvertStreamToByteArray(request.Artifact, ct));

            release = await _repository.Add(release, ct);

            if (publish)
            {
                release.Status = FirmwareReleaseStatus.Published;
                release.PublishedAt = DateTime.UtcNow;
            }

            FirmwareArtifact artifact = BuildArtifact(content, request.Artifact.FileName, request.Artifact.ContentType, release);

            await _repository.AddFirmwareArtifact(artifact, ct);

            await _repository.SaveChanges(ct);

            return _mapper.Map<FirmwareReleaseResponse>(release);
        }

        public async Task<IReadOnlyList<FirmwareReleaseResponse>> GetAll(string? board, FirmwareReleaseStatus? status, CancellationToken ct)
            => _mapper.Map<IReadOnlyList<FirmwareReleaseResponse>>(await _repository.GetAll(board, status, ct));

        public async Task<FirmwareReleaseResponse> GetByReleaseId(string releaseId, CancellationToken ct)
        {
            FirmwareRelease release = await _repository.FindByReleaseId(releaseId, false, ct) ?? throw new NotFoundException("La publicación de firmware no fue encontrada.");

            return _mapper.Map<FirmwareReleaseResponse>(release);
        }

        public async Task<FirmwareReleaseResponse> Publish(string releaseId, CancellationToken ct)
        {
            ValidateReleaseId(releaseId);

            FirmwareRelease foundRelease = await _repository.FindByReleaseId(releaseId, true, ct) ?? throw new NotFoundException("La publicación de firmware no fue encontrada.");

            if (!foundRelease.Active)
                throw new ConflictException("La publicación de firmware se encuentra inactiva.");

            if (foundRelease.Status == FirmwareReleaseStatus.Published)
                return _mapper.Map<FirmwareReleaseResponse>(foundRelease);

            if (foundRelease.Status == FirmwareReleaseStatus.Revoked)
                throw new ConflictException("Una publicación revocada no puede volver a publicarse.");

            if (foundRelease.Status != FirmwareReleaseStatus.Draft)
                throw new ConflictException("La publicación no se encuentra en un estado válido para publicarse.");

            if (foundRelease.Artifact is null)
                throw new ConflictException("La publicación no tiene un artefacto asociado.");

            _validator.ValidateRelease(foundRelease, ct);
            _validator.ValidateArtifactFromDb(foundRelease, foundRelease.Artifact.Content);

            foundRelease.Status = FirmwareReleaseStatus.Published;
            foundRelease.PublishedAt = DateTime.UtcNow;

            await _repository.SaveChanges(ct);

            return _mapper.Map<FirmwareReleaseResponse>(foundRelease);
        }

        public async Task<FirmwareReleaseResponse> Revoke(string releaseId, CancellationToken ct)
        {
            ValidateReleaseId(releaseId);

            FirmwareRelease foundRelease = await _repository.FindByReleaseId(releaseId, false, ct) ?? throw new NotFoundException("Release no encontrado");

            if (foundRelease.Status == FirmwareReleaseStatus.Revoked)
                return _mapper.Map<FirmwareReleaseResponse>(foundRelease);

            if (foundRelease.Status == FirmwareReleaseStatus.Draft)
                throw new ConflictException("Release en draft");

            foundRelease.Status = FirmwareReleaseStatus.Revoked;

            await _repository.SaveChanges(ct);

            return _mapper.Map<FirmwareReleaseResponse>(foundRelease);
        }

        public async Task<FirmwareReleaseResponse> Update(UpdateFirmwareReleaseRequest request, CancellationToken ct)
        {
            request.Validate();

            FirmwareRelease? foundRelease = await _repository.FindByReleaseId(request.ReleaseId, true, ct) ?? throw new NotFoundException("Firmware no encontrado");

            if (foundRelease.Status != FirmwareReleaseStatus.Draft)
                throw new ConflictException("Solo pueden modificarse publicaciones en estado Draft.");

            bool boardVersionChanged = foundRelease.Board != request.Board || foundRelease.Version != request.Version;

            if (boardVersionChanged && await _repository.ExistsBoardVersion(request.Board, request.Version, ct))
                throw new ConflictException("Ya existe esa versión para la placa indicada.");

            if (foundRelease.Artifact is null)
                throw new ConflictException("La publicación no tiene un artefacto asociado.");

            _mapper.Map(request, foundRelease);

            _validator.ValidateRelease(foundRelease, ct);

            byte[] content = _validator.ValidateArtifactFromDb(foundRelease, await ConvertStreamToByteArray(request.Artifact, ct));

            foundRelease.Artifact.Content = content;

            await _repository.SaveChanges(ct);

            return _mapper.Map<FirmwareReleaseResponse>(foundRelease);
        }

        private static void ValidateReleaseId(string releaseId)
        {
            if (string.IsNullOrWhiteSpace(releaseId))
                throw new ValidationException("El ReleaseId es obligatorio.");

            if (releaseId.Length > 64)
                throw new ValidationException("El ReleaseId no puede superar 64 caracteres.");

            bool valid = releaseId.All(character =>
                char.IsAsciiLetterOrDigit(character) ||
                character == '-' ||
                character == '_');

            if (!valid)
                throw new ValidationException("El ReleaseId contiene caracteres no permitidos.");
        }

        private static FirmwareArtifact BuildArtifact(byte[] content, string fileName, string contentType, FirmwareRelease release)
            => new()
            {
                Content = content,
                FileName = fileName,
                ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType,
                FirmwareRelease = release
            };

        private static async Task<byte[]> ConvertStreamToByteArray(IFormFile artifact, CancellationToken ct)
        {
            await using Stream stream = artifact.OpenReadStream();
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream, ct);

            return memoryStream.ToArray();
        }
    }
}
