using GardensBack.Application.DeviceProvisioning.Dtos.Device;

namespace GardensBack.Application.DeviceProvisioning.Abstractions
{
    public interface IClaimResponseSigner
    {
        string Sign(ClaimResponse response, string claimCode);
    }
}
