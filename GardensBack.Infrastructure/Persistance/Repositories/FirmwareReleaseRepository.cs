using GardensBack.Application.Abstractions;
using GardensBack.Domain.Entities;
using GardensBack.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GardensBack.Infrastructure.Persistance.Repositories
{
    public class FirmwareReleaseRepository(ApplicationDbContext db) : IFirmwareReleaseRepository
    {
        private readonly ApplicationDbContext _db = db;

        public async Task Add(FirmwareRelease release, CancellationToken ct)
        {
            await _db.FirmwareReleases.AddAsync(release, ct);

            await _db.SaveChangesAsync(ct);
        }

        public async Task<bool> ExistsBoardVersion(string board, string version, CancellationToken ct)
            => await _db.FirmwareReleases.AnyAsync(x => x.Board == board && x.Version == version, ct);

        public async Task<bool> ExistsReleaseId(string releasedId, CancellationToken ct)
            => await _db.FirmwareReleases.AnyAsync(x => x.ReleaseId == releasedId, ct);

        public async Task<FirmwareRelease?> FindByReleaseId(string releaseId, CancellationToken ct)
            => await _db.FirmwareReleases.FirstOrDefaultAsync(x => x.ReleaseId == releaseId, ct);

        public async Task<IReadOnlyList<FirmwareRelease>> GetAll(string? board, FirmwareReleaseStatus? status, CancellationToken ct)
            => await _db.FirmwareReleases.Where(x => (string.IsNullOrEmpty(board) || x.Board == board) && (!status.HasValue || x.Status == status.Value)).ToListAsync(ct);

        public async Task<FirmwareRelease?> FindPublishedByReleaseId(string releaseId, CancellationToken ct)
            => await _db.FirmwareReleases.AsNoTracking().SingleOrDefaultAsync(x => x.ReleaseId == releaseId && x.Active && x.Status == FirmwareReleaseStatus.Published, ct);

        public async Task<int> SaveChanges(CancellationToken ct)
            => await _db.SaveChangesAsync(ct);
    }
}
