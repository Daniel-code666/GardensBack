using GardensBack.Application.FirmwareReleases.Dtos;
using GardensBack.Domain.Enums;

namespace GardensBack.Application.FirmwareReleases.Abstractions
{
    public interface IFirmwareReleaseService
    {
        Task<FirmwareReleaseResponse> Create(CreateFirmwareReleaseRequest request, CancellationToken ct);
        Task<FirmwareReleaseResponse> Update(UpdateFirmwareReleaseRequest request, CancellationToken ct);
        Task<FirmwareReleaseResponse> GetByReleaseId(string releaseId, CancellationToken ct);
        Task<IReadOnlyList<FirmwareReleaseResponse>> GetAll(string? board, FirmwareReleaseStatus? status, CancellationToken ct);
        Task<FirmwareReleaseResponse> Publish(string releaseId, CancellationToken ct);
        Task<FirmwareReleaseResponse> Revoke(string releaseId, CancellationToken ct);
    }
}
