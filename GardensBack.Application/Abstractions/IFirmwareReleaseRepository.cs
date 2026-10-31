using GardensBack.Domain.Entities;
using GardensBack.Domain.Enums;

namespace GardensBack.Application.Abstractions
{
    public interface IFirmwareReleaseRepository
    {
        Task<FirmwareRelease?> FindByReleaseId(string releaseId, CancellationToken ct);
        Task<bool> ExistsBoardVersion(string board, string version, CancellationToken ct);
        Task<bool> ExistsReleaseId(string releaseId, CancellationToken ct);
        Task<IReadOnlyList<FirmwareRelease>> GetAll(string? board, FirmwareReleaseStatus? status, CancellationToken ct);
        Task Add(FirmwareRelease release, CancellationToken ct);
        Task<int> SaveChanges(CancellationToken ct);
    }
}
