using GardensBack.Application.Abstractions;
using GardensBack.Domain.Entities;
using GardensBack.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GardensBack.Infrastructure.Persistance.Repositories
{
    public class FirmwareReleaseRepository(ApplicationDbContext db) : IFirmwareReleaseRepository
    {
        private readonly ApplicationDbContext _db = db;

        public async Task<FirmwareRelease> Add(FirmwareRelease release, CancellationToken ct)
        {
            var entry = await _db.FirmwareReleases.AddAsync(release, ct);
            return entry.Entity;
        }

        public async Task<bool> ExistsBoardVersion(string board, string version, CancellationToken ct)
            => await _db.FirmwareReleases.AnyAsync(x => x.Board == board && x.Version == version, ct);

        public async Task<bool> ExistsReleaseId(string releasedId, CancellationToken ct)
            => await _db.FirmwareReleases.AnyAsync(x => x.ReleaseId == releasedId, ct);

        public async Task<FirmwareRelease?> FindByReleaseId(string releaseId, bool includeLastArtifact, CancellationToken ct)
        {
            IQueryable<FirmwareRelease> query = _db.FirmwareReleases;

            if (includeLastArtifact)
                query = query.Include(x => x.Artifact);
            
            return await query.FirstOrDefaultAsync(x => x.ReleaseId == releaseId, ct);
        }

        public async Task<IReadOnlyList<FirmwareRelease>> GetAll(string? board, FirmwareReleaseStatus? status, CancellationToken ct)
            => await _db.FirmwareReleases.Where(x => (string.IsNullOrEmpty(board) || x.Board == board) && (!status.HasValue || x.Status == status.Value)).ToListAsync(ct);

        public async Task<FirmwareRelease?> FindPublishedByReleaseId(string releaseId, bool includeLastArtifact, CancellationToken ct)
        {
            IQueryable<FirmwareRelease> query = _db.FirmwareReleases.Where(x => x.ReleaseId == releaseId && x.Status == FirmwareReleaseStatus.Published);

            if (includeLastArtifact)
                query = query.Include(x => x.Artifact);
            
            return await query.FirstOrDefaultAsync(ct);
        }

        public async Task AddFirmwareArtifact(FirmwareArtifact artifact, CancellationToken ct)
            => await _db.FirmwareArtifacts.AddAsync(artifact, ct);

        public async Task<int> SaveChanges(CancellationToken ct)
            => await _db.SaveChangesAsync(ct);
    }
}
