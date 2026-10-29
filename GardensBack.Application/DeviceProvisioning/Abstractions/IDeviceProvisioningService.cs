using GardensBack.Application.Common.Results;
using GardensBack.Application.DeviceProvisioning.Dtos.Device;
using GardensBack.Application.DeviceProvisioning.Dtos.Web;
using GardensBack.Domain.Entities;

namespace GardensBack.Application.DeviceProvisioning.Abstractions
{
    public interface IDeviceProvisioningService
    {
        Task<StartDeviceProvisioningResponse> StartProvisioning(int userId, StartDeviceProvisioningRequest request, CancellationToken ct);
        Task<ClaimResponse> RegisterClaim(string provisioningToken, ClaimRegistrationRequest request, CancellationToken ct);
        Task<ClaimResponse?> GetClaim(string provisioningToken, string requestId, CancellationToken ct);
        Task<MqttCredentialsEnvelopeResponse> RegisterMqttCredentials(string provisioningToken, MqttCredentialsRegistrationRequest request, CancellationToken ct);
        Task<MqttCredentialsEnvelopeResponse> GetMqttCredentials(string provisioningToken, string requestId, CancellationToken ct);
    }
}
