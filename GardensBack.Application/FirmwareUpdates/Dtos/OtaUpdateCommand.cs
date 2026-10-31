using System.Text.Json.Serialization;

using GardensBack.Application.FirmwareUpdates.Constants;

namespace GardensBack.Application.FirmwareUpdates.Dtos
{
    public class OtaUpdateCommand
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = FirmwareUpdateContract.UpdateRequestType;

        [JsonPropertyName("contractVersion")]
        public int ContractVersion { get; set; } = FirmwareUpdateContract.Version;

        [JsonPropertyName("requestId")]
        public required string RequestId { get; set; }

        [JsonPropertyName("deviceId")]
        public required string DeviceId { get; set; }

        [JsonPropertyName("commandVersion")]
        public uint CommandVersion { get; set; }

        [JsonPropertyName("releaseId")]
        public required string ReleaseId { get; set; }

        [JsonPropertyName("manifestUrl")]
        public required string ManifestUrl { get; set; }

        [JsonPropertyName("expiresAt")]
        public required string ExpiresAt { get; set; }

        [JsonPropertyName("authenticationTag")]
        public required string AuthenticationTag { get; set; }
    }
}
