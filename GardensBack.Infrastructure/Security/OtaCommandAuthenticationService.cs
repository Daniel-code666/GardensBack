using GardensBack.Application.FirmwareUpdates.Abstractions;
using GardensBack.Application.FirmwareUpdates.Dtos;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace GardensBack.Infrastructure.Security
{
    public class OtaCommandAuthenticationService : IOtaCommandAuthenticationService
    {
        public string Sign(OtaUpdateCommand command, byte[] key)
        {
            if (key.Length != 32)
                throw new ArgumentException("La clave de comandos debe tener 256 bits.", nameof(key));

            byte[] payload = Encoding.UTF8.GetBytes(BuildCanonicalPayload(command));
            try
            {
                byte[] signature = HMACSHA256.HashData(key, payload);
                try
                {
                    return Convert.ToHexStringLower(signature);
                }
                finally
                {
                    CryptographicOperations.ZeroMemory(signature);
                }
            }
            finally
            {
                CryptographicOperations.ZeroMemory(payload);
            }
        }

        private static string BuildCanonicalPayload(OtaUpdateCommand command)
        {
            var builder = new StringBuilder();
            Append(builder, command.ContractVersion.ToString(CultureInfo.InvariantCulture));
            Append(builder, command.DeviceId);
            Append(builder, command.RequestId);
            Append(builder, command.CommandVersion.ToString(CultureInfo.InvariantCulture));
            Append(builder, command.ReleaseId);
            Append(builder, command.ManifestUrl);
            Append(builder, command.ExpiresAt);
            return builder.ToString();
        }

        private static void Append(StringBuilder builder, string value)
        {
            builder.Append(Encoding.UTF8.GetByteCount(value));
            builder.Append(':');
            builder.Append(value);
            builder.Append('\n');
        }
    }
}
