using GardensBack.Domain.Entities;

namespace GardensBack.Application.DeviceProvisioning.Abstractions
{
    public interface IDeviceProvisioningRepository
    {
        Task<IReadOnlyList<MicrocontrollerDevice>> FindIdentityMatches(string deviceId,string hardwareId,string mqttClientId,CancellationToken ct);

        Task<MicrocontrollerDevice?> FindByDeviceId(string deviceId,CancellationToken ct);

        Task<DeviceClaim?> FindClaimByRequestId(string requestId,CancellationToken ct);

        Task<DeviceClaim?> FindClaimByBootstrapRequestId(string bootstrapRequestId,CancellationToken ct);

        Task<DeviceClaim?> FindClaimByTokenHash(string tokenHash,CancellationToken ct);

        Task AddDevice(MicrocontrollerDevice device, CancellationToken ct);

        Task AddMqttCredential(MqttCredential credential, CancellationToken ct);

        Task<int> SaveChanges(CancellationToken ct);
    }
}
