using GardensBack.Application.Common.Messaging;
using GardensBack.Application.Common.Validation;
using GardensBack.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using AppValidationException = GardensBack.Application.Common.Exceptions.ValidationException;

namespace GardensBack.Application.DeviceInventory.Dtos
{
    public class DeviceActionResultMessage
    {
        [Required]
        public required string Type { get; set; }

        public int ContractVersion { get; set; }

        [Required, StringLength(64, MinimumLength = 1)]
        public required string RequestId { get; set; }

        [Required, StringLength(64, MinimumLength = 1)]
        public required string DeviceId { get; set; }

        [EnumDataType(typeof(DeviceCommandType))]
        public DeviceCommandType Command { get; set; }

        public bool Success { get; set; }

        [Required]
        public DeviceTelemetryMessage Telemetry { get; set; } = new();

        public void Validate()
        {
            ValidationUtilities.ValidateObject(this);

            if (!string.Equals(Type, "device_action_result", StringComparison.Ordinal))
                throw new AppValidationException("El tipo de respuesta del comando no es válido.");

            if (ContractVersion != 1)
                throw new AppValidationException("La versión del contrato de respuesta no es compatible.");

            if (!DeviceMqttTopics.IsValidDeviceId(DeviceId))
                throw new AppValidationException("El identificador del microcontrolador no es válido.");
        }
    }
}
