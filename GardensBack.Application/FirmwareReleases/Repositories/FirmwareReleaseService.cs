using AutoMapper;
using GardensBack.Application.Abstractions;
using GardensBack.Application.Common.Exceptions;
using GardensBack.Application.FirmwareReleases.Abstractions;
using GardensBack.Application.FirmwareReleases.Dtos;
using GardensBack.Domain.Entities;
using GardensBack.Domain.Enums;
using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;

namespace GardensBack.Application.FirmwareReleases.Repositories
{
    public class FirmwareReleaseService(IFirmwareReleaseRepository repository, IFirmwareReleasePublicationValidator validator, IMapper mapper) : IFirmwareReleaseService
    {
        private readonly IFirmwareReleaseRepository _repository = repository;
        private readonly IMapper _mapper = mapper;
        private readonly IFirmwareReleasePublicationValidator _validator = validator;

        public async Task<FirmwareReleaseResponse> Create(CreateFirmwareReleaseRequest request, bool publish, CancellationToken ct)
        {
            request.Validate();

            if (await _repository.ExistsReleaseId(request.ReleaseId, ct))
                throw new ConflictException("Ya existe release id");

            if (await _repository.ExistsBoardVersion(request.Board, request.Version, ct))
                throw new ConflictException("Ya existe esa combinación entre board + versión");

            byte[] content = await ValidateArtifactFromHttpRequest(request.Artifact, request.SizeBytes, ct);

            ValidateHashFromArtifact(content, request.Sha256);

            FirmwareRelease release = _mapper.Map<FirmwareRelease>(request);

            release.Status = FirmwareReleaseStatus.Draft;
            release.PublishedAt = null;
            release.Active = true;

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

            if (foundRelease.Artifact?.Content is byte[] content)
            {
                ValidateArtifactFromDb(content, foundRelease.SizeBytes);

                ValidateHashFromArtifact(content, foundRelease.Sha256);
            }
            else
                await _validator.Validate(foundRelease, ct);

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

            FirmwareRelease? foundRelease = await _repository.FindByReleaseId(request.ReleaseId, false, ct) ?? throw new NotFoundException("Firmware no encontrado");

            if (foundRelease.Status != FirmwareReleaseStatus.Draft)
                throw new ConflictException("Solo pueden modificarse publicaciones en estado Draft.");

            bool boardVersionChanged = foundRelease.Board != request.Board || foundRelease.Version != request.Version;

            if (boardVersionChanged && await _repository.ExistsBoardVersion(request.Board, request.Version, ct))
                throw new ConflictException("Ya existe esa versión para la placa indicada.");

            _mapper.Map(request, foundRelease);

            await _repository.SaveChanges(ct);

            return _mapper.Map<FirmwareReleaseResponse>(foundRelease);
        }

        public static byte[] ValidateArtifactFromDb(byte[] content, long sizeBytes)
        {
            if (content.LongLength != sizeBytes)
                throw new ConflictException("El tamaño del firmware no coincide con el tamaño reportado.");
            
            return content;
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

        private async static Task<byte[]> ValidateArtifactFromHttpRequest(IFormFile artifact, long sizeBytes, CancellationToken ct)
        {
            await using var stream = artifact.OpenReadStream();

            using var memoryStream = new MemoryStream();

            await stream.CopyToAsync(memoryStream, ct);

            byte[] content = memoryStream.ToArray();

            if (content.LongLength != sizeBytes)
                throw new ConflictException("El tamaño del firmware no coincide con el tamaño reportado.");

            return content;
        }

        private static FirmwareArtifact BuildArtifact(byte[] content, string fileName, string contentType, FirmwareRelease release)
            => new()
            {
                Content = content,
                FileName = fileName,
                ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType,
                FirmwareRelease = release
            };

        private static void ValidateHashFromArtifact(byte[] content, string requestHash)
        {
            var calculatedHash = Convert.ToHexString(SHA256.HashData(content)).ToLowerInvariant();

            if (!string.Equals(calculatedHash, requestHash, StringComparison.OrdinalIgnoreCase))
                throw new ConflictException("El hash del firmware no coincide con el hash reportado.");
        }
    }
}
