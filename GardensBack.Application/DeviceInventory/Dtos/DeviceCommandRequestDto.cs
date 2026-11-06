using GardensBack.Application.Common.Validation;
using GardensBack.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using AppValidationException = GardensBack.Application.Common.Exceptions.ValidationException;

namespace GardensBack.Application.DeviceInventory.Dtos
{
    public class DeviceCommandRequestDto
    {
        [Required, MinLength(1), MaxLength(16)]
        public required int[] DeviceIds { get; set; }

        [EnumDataType(typeof(DeviceCommandType))]
        public DeviceCommandType Command { get; set; }

        public bool? State { get; set; }

        public ActuatorMode? Mode { get; set; }

        public void Validate()
        {
            ValidationUtilities.ValidateObject(this);

            if (DeviceIds.Any(id => id <= 0))
                throw new AppValidationException("Los identificadores de dispositivos deben ser mayores que cero.");

            switch (Command)
            {
                case DeviceCommandType.GetSensorOrActuatorDataById:
                    if (State.HasValue || Mode.HasValue)
                        throw new AppValidationException("El comando de lectura no admite estado ni modo.");
                    break;

                case DeviceCommandType.SetWaterPumpState:
                    if (!State.HasValue)
                        throw new AppValidationException("El estado de la bomba es obligatorio.");
                    if (Mode.HasValue)
                        throw new AppValidationException("El comando de estado no admite un modo de control.");
                    break;

                case DeviceCommandType.SetWaterPumpControlMode:
                    if (!Mode.HasValue)
                        throw new AppValidationException("El modo de control de la bomba es obligatorio.");
                    if (State.HasValue)
                        throw new AppValidationException("El comando de modo no admite un estado.");
                    break;

                default:
                    throw new AppValidationException("El comando solicitado no es válido.");
            }
        }
    }
}
