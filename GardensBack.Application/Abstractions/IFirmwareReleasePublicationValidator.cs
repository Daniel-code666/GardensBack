using GardensBack.Domain.Entities;

namespace GardensBack.Application.Abstractions
{
    public interface IFirmwareReleasePublicationValidator
    {
        Task Validate(FirmwareRelease release, CancellationToken ct);
    }
}
