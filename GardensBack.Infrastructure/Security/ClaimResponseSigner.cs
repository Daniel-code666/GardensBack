using GardensBack.Application.DeviceProvisioning.Abstractions;
using GardensBack.Application.DeviceProvisioning.Dtos.Device;
using GardensBack.Application.DeviceProvisioning.Enums;
using System.Security.Cryptography;
using System.Text;

namespace GardensBack.Infrastructure.Security
{
    public class ClaimResponseSigner : IClaimResponseSigner
    {
        public string Sign(ClaimResponse response, string claimCode)
        {
            string payload = BuildCanonicalPayload(response);

            byte[] key = Encoding.UTF8.GetBytes(claimCode);
            byte[] content = Encoding.UTF8.GetBytes(payload);
            byte[] signature = HMACSHA256.HashData(key, content);

            return Convert.ToHexStringLower(signature);
        }

        private static string BuildCanonicalPayload(ClaimResponse response)
        {
            StringBuilder payload = new();

            AppendField(payload, response.ContractVersion.ToString());
            AppendField(payload, response.DeviceId);
            AppendField(payload, response.RequestId);
            AppendField(payload, GetDecisionValue(response.Decision));
            AppendField(payload, response.UserId);
            AppendField(payload, response.GardenId);
            AppendField(payload, response.ErrorCode);

            return payload.ToString();
        }

        private static void AppendField(StringBuilder payload, string value)
        {
            payload.Append(Encoding.UTF8.GetByteCount(value));
            payload.Append(':');
            payload.Append(value);
            payload.Append('\n');
        }

        private static string GetDecisionValue(ClaimDecision decision)
        {
            return decision switch
            {
                ClaimDecision.Approved => "approved",
                ClaimDecision.Rejected => "rejected",
                ClaimDecision.Expired => "expired",
                _ => throw new ArgumentOutOfRangeException(nameof(decision))
            };
        }
    }
}
