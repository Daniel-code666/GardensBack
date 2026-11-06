using GardensBack.Application.Common.Security;
using GardensBack.Application.DeviceInventory.Dtos;
using System.Globalization;
using System.Text;

namespace GardensBack.Infrastructure.Security
{
    public class DeviceActionAuthenticationService : ICommandAuthenticationService<DeviceActionMessage>
    {
        public string Sign(DeviceActionMessage command, byte[] key)
            => HmacCommandSigner.Sign(BuildCanonicalPayload(command), key);

        private static string BuildCanonicalPayload(DeviceActionMessage command)
        {
            var builder = new StringBuilder();
            HmacCommandSigner.Append(builder, command.ContractVersion.ToString(CultureInfo.InvariantCulture));
            HmacCommandSigner.Append(builder, command.DeviceId);
            HmacCommandSigner.Append(builder, command.RequestId);
            HmacCommandSigner.Append(builder, command.CommandVersion.ToString(CultureInfo.InvariantCulture));
            HmacCommandSigner.Append(builder, command.ExpiresAt.ToString(CultureInfo.InvariantCulture));
            HmacCommandSigner.Append(builder, command.Command.ToString());
            HmacCommandSigner.Append(builder, command.Params.Count.ToString(CultureInfo.InvariantCulture));

            foreach (DeviceActionParameter parameter in command.Params)
            {
                HmacCommandSigner.Append(builder, parameter.DeviceId.ToString(CultureInfo.InvariantCulture));
                HmacCommandSigner.Append(builder, parameter.DeviceType);
                HmacCommandSigner.Append(builder, parameter.State.HasValue ? (parameter.State.Value ? "1" : "0") : "-");
                HmacCommandSigner.Append(builder, parameter.Value?.ToString(CultureInfo.InvariantCulture) ?? "-");
            }

            return builder.ToString();
        }
    }
}
