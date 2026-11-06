using GardensBack.Domain.Enums;
using System.Text.Json.Serialization;

namespace GardensBack.Application.DeviceInventory.Dtos
{
    public class DeviceActionMessage
    {
        public string Type { get; set; } = "device_action";
        public int ContractVersion { get; set; } = 1;
        public required string DeviceId { get; set; }
        public required string RequestId { get; set; }
        public uint CommandVersion { get; set; }
        public uint ExpiresAt { get; set; }
        public DeviceCommandType Command { get; set; }
        public IReadOnlyCollection<DeviceActionParameter> Params { get; set; } = [];
        public required string AuthenticationTag { get; set; }
    }

    public class DeviceActionParameter
    {
        public int DeviceId { get; set; }
        public required string DeviceType { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? State { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? Value { get; set; }
    }
}
