using GardensBack.Application.DeviceProvisioning.Abstractions;
using GardensBack.Application.DeviceProvisioning.Dtos.Internal;
using System.Security.Cryptography;
using System.Text;

namespace GardensBack.Infrastructure.Security
{
    public class ProvisioningTokenService : IProvisioningTokenService
    {
        public string ComputeHash(string token)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(token);
            return Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
        }

        public GeneratedProvisioningToken Generate()
        {
            byte[] randomBytes = RandomNumberGenerator.GetBytes(32);
            string token = Convert.ToBase64String(randomBytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');

            return new GeneratedProvisioningToken(token, ComputeHash(token));
        }

        public bool Verify(string token, string expectedHash)
        {
            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(expectedHash))
                return false;

            try
            {
                byte[] actual = Convert.FromHexString(ComputeHash(token));
                byte[] expected = Convert.FromHexString(expectedHash);
                return actual.Length == expected.Length &&
                       CryptographicOperations.FixedTimeEquals(actual, expected);
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
