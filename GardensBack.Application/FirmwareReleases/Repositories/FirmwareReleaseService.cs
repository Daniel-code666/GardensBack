using AutoMapper;
using GardensBack.Application.Abstractions;
using GardensBack.Application.Common.Exceptions;
using GardensBack.Application.FirmwareReleases.Abstractions;
using GardensBack.Application.FirmwareReleases.Dtos;
using GardensBack.Domain.Entities;
using GardensBack.Domain.Enums;

namespace GardensBack.Application.FirmwareReleases.Repositories
{
    public class FirmwareReleaseService(IFirmwareReleaseRepository repository, IFirmwareReleasePublicationValidator validator, IMapper mapper) : IFirmwareReleaseService
    {
        private readonly IFirmwareReleaseRepository _repository = repository;
        private readonly IMapper _mapper = mapper;
        private readonly IFirmwareReleasePublicationValidator _validator = validator;

        public async Task<FirmwareReleaseResponse> Create(CreateFirmwareReleaseRequest request, CancellationToken ct)
        {
            request.Validate();

            if (await _repository.ExistsReleaseId(request.ReleaseId, ct))
                throw new ConflictException("Ya existe release id");

            if (await _repository.ExistsBoardVersion(request.Board, request.Version, ct))
                throw new ConflictException("Ya existe esa combinación entre board + versión");

            FirmwareRelease release = _mapper.Map<FirmwareRelease>(request);

            release.Status = FirmwareReleaseStatus.Draft;
            release.PublishedAt = null;
            release.Active = true;

            await _repository.Add(release, ct);

            return _mapper.Map<FirmwareReleaseResponse>(release);
        }

        public async Task<IReadOnlyList<FirmwareReleaseResponse>> GetAll(string? board, FirmwareReleaseStatus? status, CancellationToken ct)
            => _mapper.Map<IReadOnlyList<FirmwareReleaseResponse>>(await _repository.GetAll(board, status, ct));

        public async Task<FirmwareReleaseResponse> GetByReleaseId(string releaseId, CancellationToken ct)
        {
            FirmwareRelease release = await _repository.FindByReleaseId(releaseId, ct) ?? throw new NotFoundException("La publicación de firmware no fue encontrada.");

            return _mapper.Map<FirmwareReleaseResponse>(release);
        }

        public async Task<FirmwareReleaseResponse> Publish(string releaseId, CancellationToken ct)
        {
            ValidateReleaseId(releaseId);

            FirmwareRelease foundRelease = await _repository.FindByReleaseId(releaseId, ct) ?? throw new NotFoundException("La publicación de firmware no fue encontrada.");

            if (!foundRelease.Active)
                throw new ConflictException("La publicación de firmware se encuentra inactiva.");

            if (foundRelease.Status == FirmwareReleaseStatus.Published)
                return _mapper.Map<FirmwareReleaseResponse>(foundRelease);

            if (foundRelease.Status == FirmwareReleaseStatus.Revoked)
                throw new ConflictException("Una publicación revocada no puede volver a publicarse.");

            if (foundRelease.Status != FirmwareReleaseStatus.Draft)
                throw new ConflictException("La publicación no se encuentra en un estado válido para publicarse.");

            await _validator.Validate(foundRelease, ct);

            foundRelease.Status = FirmwareReleaseStatus.Published;
            foundRelease.PublishedAt = DateTime.UtcNow;

            await _repository.SaveChanges(ct);

            return _mapper.Map<FirmwareReleaseResponse>(foundRelease);
        }

        public async Task<FirmwareReleaseResponse> Revoke(string releaseId, CancellationToken ct)
        {
            ValidateReleaseId(releaseId);

            FirmwareRelease foundRelease = await _repository.FindByReleaseId(releaseId, ct) ?? throw new NotFoundException("Release no encontrado");

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

            FirmwareRelease? foundRelease = await _repository.FindByReleaseId(request.ReleaseId, ct) ?? throw new NotFoundException("Firmware no encontrado");

            if (foundRelease.Status != FirmwareReleaseStatus.Draft)
                throw new ConflictException("Solo pueden modificarse publicaciones en estado Draft.");

            bool boardVersionChanged = foundRelease.Board != request.Board || foundRelease.Version != request.Version;

            if (boardVersionChanged && await _repository.ExistsBoardVersion(request.Board, request.Version, ct))
                throw new ConflictException("Ya existe esa versión para la placa indicada.");

            _mapper.Map(request, foundRelease);

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
    }
}
