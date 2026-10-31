using GardensBack.Application.FirmwareReleases.Dtos;

namespace GardensBack.Application.FirmwareReleases.Abstractions
{
    public interface IFirmwareManifestService
    {
        Task<FirmwareManifestResponse> GetPublishedManifest(string releaseId, CancellationToken ct);
    }
}
