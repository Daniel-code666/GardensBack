using GardensBack.Domain.Entities;
using GardensBack.Domain.Enums;

namespace GardensBack.Application.Abstractions
{
    public interface IFirmwareReleaseRepository
    {
        Task<FirmwareRelease?> FindByReleaseId(string releaseId, bool includeLastArtifact, CancellationToken ct);
        Task<bool> ExistsBoardVersion(string board, string version, CancellationToken ct);
        Task<bool> ExistsReleaseId(string releaseId, CancellationToken ct);
        Task<IReadOnlyList<FirmwareRelease>> GetAll(string? board, FirmwareReleaseStatus? status, CancellationToken ct);
        Task<FirmwareRelease> Add(FirmwareRelease release, CancellationToken ct);
        Task<int> SaveChanges(CancellationToken ct);
        Task<FirmwareRelease?> FindPublishedByReleaseId(string releaseId, bool includeLastArtifact, CancellationToken ct);
        Task AddFirmwareArtifact(FirmwareArtifact artifact, CancellationToken ct);
    }
}
