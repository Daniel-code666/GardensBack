using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace GardensBack.Domain.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum DeviceClaimStatus : int
    {
        Pending,
        Approved,
        Rejected,
        Expired,
        Cancelled
    }
}
