using GardensBack.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace GardensBack.Application.Abstractions
{
    public interface IFirmwareReleasePublicationValidator
    {
        Task Validate(FirmwareRelease release, CancellationToken ct);
        byte[] ValidateArtifactFromDb(byte[] content, long sizeBytes);
        void ValidateHashFromArtifact(byte[] content, string requestHash);
        Task<byte[]> ValidateArtifactFromHttpRequest(IFormFile artifact, long sizeBytes, CancellationToken ct);
    }
}
