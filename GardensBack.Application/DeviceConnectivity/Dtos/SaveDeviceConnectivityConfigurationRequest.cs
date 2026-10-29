using GardensBack.Application.Common.Validation;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace GardensBack.Application.DeviceConnectivity.Dtos
{
    public class SaveDeviceConnectivityConfigurationRequest : IValidatableObject
    {
        [Required, StringLength(253, MinimumLength = 1)]
        public required string ApiHost { get; set; }

        [Range(1, 65535)]
        public int ApiPort { get; set; }

        public bool ApiUseTls { get; set; }

        [Required, StringLength(253, MinimumLength = 1)]
        public required string MqttHost { get; set; }

        [Range(1, 65535)]
        public int MqttPort { get; set; }

        public bool MqttUseTls { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!IsDeviceAccessibleHost(ApiHost))
                yield return new ValidationResult("ApiHost debe ser una dirección IP o un nombre de host accesible por el dispositivo.", [nameof(ApiHost)]);

            if (!IsDeviceAccessibleHost(MqttHost))
                yield return new ValidationResult("MqttHost debe ser una dirección IP o un nombre de host accesible por el dispositivo.", [nameof(MqttHost)]);
        }

        public void Validate()
            => ValidationUtilities.ValidateObject(this);

        private static bool IsDeviceAccessibleHost(string? host)
        {
            if (string.IsNullOrWhiteSpace(host) || host != host.Trim())
                return false;

            if (host.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
                host.Equals("host.docker.internal", StringComparison.OrdinalIgnoreCase) ||
                host.Equals("0.0.0.0", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (IPAddress.TryParse(host, out IPAddress? address))
                return !IPAddress.IsLoopback(address) && !address.Equals(IPAddress.Any) && !address.Equals(IPAddress.IPv6Any);

            return Uri.CheckHostName(host) == UriHostNameType.Dns;
        }
    }
}
