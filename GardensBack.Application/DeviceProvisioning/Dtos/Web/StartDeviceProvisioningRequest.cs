using GardensBack.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace GardensBack.Application.DeviceProvisioning.Dtos.Web
{
    using GardensBack.Application.Common.Validation;
    using GardensBack.Application.DeviceProvisioning.Constants;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Representa la identidad obtenida por Web Serial y los datos descriptivos introducidos por el usuario.
    /// </summary>
    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public class StartDeviceProvisioningRequest
    {
        [Required, StringLength(100, MinimumLength = 1)]
        public required string Name { get; set; }

        [Required, StringLength(500, MinimumLength = 1)]
        public required string Description { get; set; }

        [Range(DeviceProvisioningContract.Version, DeviceProvisioningContract.Version)]
        public int ContractVersion { get; set; }

        [Required, StringLength(DeviceProvisioningContract.MaximumBootstrapRequestIdLength, MinimumLength = 1)]
        [RegularExpression(DeviceProvisioningContract.IdentifierPattern)]
        public required string IdentifyRequestId { get; set; }

        [Required, StringLength(DeviceProvisioningContract.MaximumIdentifierLength, MinimumLength = 1)]
        [RegularExpression(DeviceProvisioningContract.IdentifierPattern)]
        public required string DeviceId { get; set; }

        [Required, RegularExpression(DeviceProvisioningContract.HardwareIdPattern)]
        public required string HardwareId { get; set; }

        [Required, StringLength(DeviceProvisioningContract.MaximumIdentifierLength, MinimumLength = 1)]
        [RegularExpression(DeviceProvisioningContract.IdentifierPattern)]
        public required string MqttClientId { get; set; }

        [Required, StringLength(64, MinimumLength = 1)]
        [RegularExpression(DeviceProvisioningContract.IdentifierPattern)]
        public required string Board { get; set; }

        [Required, StringLength(32, MinimumLength = 1)]
        [RegularExpression(DeviceProvisioningContract.FirmwareVersionPattern)]
        public required string FirmwareVersion { get; set; }

        [JsonRequired, EnumDataType(typeof(ProvisioningState))]
        public ProvisioningState ProvisioningState { get; set; }

        [JsonRequired]
        public bool WifiConfigured { get; set; }

        [JsonRequired]
        public bool MqttConfigured { get; set; }

        public void Validate()
            => ValidationUtilities.ValidateObject(this);
    }
}
