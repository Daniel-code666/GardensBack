using GardensBack.Application.DeviceProvisioning.Dtos.Device;
using GardensBack.Application.DeviceProvisioning.Dtos.Internal;

namespace GardensBack.Application.DeviceProvisioning.Abstractions
{
    public interface IMqttCredentialEnvelopeService
    {
        MqttCredentialsEnvelopeResponse Encrypt(MqttCredentialsPayload payload, string deviceId, string requestId, string bootstrapKey);
    }
}
