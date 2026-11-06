using System.Security.Cryptography;
using System.Text;

namespace GardensBack.Infrastructure.Security
{
    internal static class HmacCommandSigner
    {
        public static string Sign(string canonicalPayload, byte[] key)
        {
            if (key.Length != 32)
                throw new ArgumentException("La clave de comandos debe tener 256 bits.", nameof(key));

            byte[] payload = Encoding.UTF8.GetBytes(canonicalPayload);
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

        public static void Append(StringBuilder builder, string value)
        {
            builder.Append(Encoding.UTF8.GetByteCount(value));
            builder.Append(':');
            builder.Append(value);
            builder.Append('\n');
        }
    }
}
