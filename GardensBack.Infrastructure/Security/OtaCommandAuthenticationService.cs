using GardensBack.Application.Common.Security;
using GardensBack.Application.FirmwareUpdates.Dtos;
using System.Globalization;
using System.Text;

namespace GardensBack.Infrastructure.Security
{
    public class OtaCommandAuthenticationService : ICommandAuthenticationService<OtaUpdateCommand>
    {
        public string Sign(OtaUpdateCommand command, byte[] key)
        {
            return HmacCommandSigner.Sign(BuildCanonicalPayload(command), key);
        }

        private static string BuildCanonicalPayload(OtaUpdateCommand command)
        {
            var builder = new StringBuilder();
            HmacCommandSigner.Append(builder, command.ContractVersion.ToString(CultureInfo.InvariantCulture));
            HmacCommandSigner.Append(builder, command.DeviceId);
            HmacCommandSigner.Append(builder, command.RequestId);
            HmacCommandSigner.Append(builder, command.CommandVersion.ToString(CultureInfo.InvariantCulture));
            HmacCommandSigner.Append(builder, command.ReleaseId);
            HmacCommandSigner.Append(builder, command.ManifestUrl);
            HmacCommandSigner.Append(builder, command.ExpiresAt);
            return builder.ToString();
        }
    }
}
