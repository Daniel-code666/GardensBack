using GardensBack.Application.DeviceProvisioning.Dtos.Internal;

namespace GardensBack.Application.DeviceProvisioning.Abstractions
{
    using GardensBack.Application.DeviceProvisioning.Dtos;

    public interface IProvisioningTokenService
    {
        GeneratedProvisioningToken Generate();

        string ComputeHash(string token);

        bool Verify(string token, string expectedHash);
    }
}
