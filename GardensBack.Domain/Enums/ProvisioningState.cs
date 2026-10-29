using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace GardensBack.Domain.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ProvisioningState : int
    {
        [JsonStringEnumMemberName("unprovisioned")]
        Unprovisioned,
        [JsonStringEnumMemberName("pending_claim")]
        PendingClaim,
        [JsonStringEnumMemberName("provisioned")]
        Provisioned
    }
}
