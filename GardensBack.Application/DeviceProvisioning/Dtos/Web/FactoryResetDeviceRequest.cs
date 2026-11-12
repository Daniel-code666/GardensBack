using GardensBack.Application.Common.Validation;
using GardensBack.Application.DeviceProvisioning.Constants;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GardensBack.Application.DeviceProvisioning.Dtos.Web
{
    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public class FactoryResetDeviceRequest
    {
        [Required, StringLength(DeviceProvisioningContract.MaximumIdentifierLength, MinimumLength = 1)]
        [RegularExpression(DeviceProvisioningContract.IdentifierPattern)]
        public required string DeviceId { get; set; }

        [Required, RegularExpression(DeviceProvisioningContract.HardwareIdPattern)]
        public required string HardwareId { get; set; }

        [Required, StringLength(DeviceProvisioningContract.MaximumIdentifierLength, MinimumLength = 1)]
        [RegularExpression(DeviceProvisioningContract.IdentifierPattern)]
        public required string MqttClientId { get; set; }

        public void Validate()
            => ValidationUtilities.ValidateObject(this);
    }
}
