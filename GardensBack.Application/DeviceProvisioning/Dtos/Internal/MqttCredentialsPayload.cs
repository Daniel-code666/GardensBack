using GardensBack.Application.Common.Validation;
using System.ComponentModel.DataAnnotations;

namespace GardensBack.Application.DeviceProvisioning.Dtos.Internal
{
    /// <summary>
    /// Representa las credenciales MQTT en texto plano únicamente durante su preparación para cifrado.
    /// </summary>
    public class MqttCredentialsPayload
    {
        [Required, StringLength(253, MinimumLength = 1)]
        public required string Host { get; set; }

        [Range(1, 65535)]
        public int Port { get; set; }

        [Required, StringLength(128, MinimumLength = 1)]
        public required string ClientId { get; set; }

        [Required, StringLength(128, MinimumLength = 1)]
        public required string User { get; set; }

        [Required, StringLength(256, MinimumLength = 1)]
        public required string Password { get; set; }

        [Range(typeof(uint), "1", "4294967295")]
        public uint CredentialVersion { get; set; }

        public void Validate()
            => ValidationUtilities.ValidateObject(this);
    }
}
