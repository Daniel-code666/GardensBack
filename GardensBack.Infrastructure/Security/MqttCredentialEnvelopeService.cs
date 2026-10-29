using GardensBack.Application.DeviceProvisioning.Abstractions;
using GardensBack.Application.DeviceProvisioning.Constants;
using GardensBack.Application.DeviceProvisioning.Dtos.Device;
using GardensBack.Application.DeviceProvisioning.Dtos.Internal;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace GardensBack.Infrastructure.Security
{
    public class MqttCredentialEnvelopeService : IMqttCredentialEnvelopeService
    {
        private const int NonceSize = 12;
        private const int AuthenticationTagSize = 16;

        public MqttCredentialsEnvelopeResponse Encrypt(MqttCredentialsPayload payload, string deviceId, string requestId, string bootstrapKey)
        {
            payload.Validate();

            byte[] key;
            try
            {
                key = Convert.FromHexString(bootstrapKey);
            }
            catch (FormatException)
            {
                throw new ArgumentException("La clave temporal no tiene un formato hexadecimal válido.", nameof(bootstrapKey));
            }

            if (key.Length != 32)
            {
                CryptographicOperations.ZeroMemory(key);
                throw new ArgumentException("La clave temporal debe tener 256 bits.", nameof(bootstrapKey));
            }

            byte[] plaintext = JsonSerializer.SerializeToUtf8Bytes(payload, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            byte[] nonce = RandomNumberGenerator.GetBytes(NonceSize);
            byte[] ciphertext = new byte[plaintext.Length];
            byte[] authenticationTag = new byte[AuthenticationTagSize];
            byte[] additionalAuthenticatedData = BuildAdditionalAuthenticatedData(deviceId, requestId);

            try
            {
                using var aes = new AesGcm(key, AuthenticationTagSize);
                aes.Encrypt(nonce, plaintext, ciphertext, authenticationTag, additionalAuthenticatedData);

                var response = new MqttCredentialsEnvelopeResponse
                {
                    DeviceId = deviceId,
                    RequestId = requestId,
                    Nonce = Convert.ToHexStringLower(nonce),
                    Ciphertext = Convert.ToHexStringLower(ciphertext),
                    AuthenticationTag = Convert.ToHexStringLower(authenticationTag)
                };

                response.Validate();
                return response;
            }
            finally
            {
                CryptographicOperations.ZeroMemory(key);
                CryptographicOperations.ZeroMemory(plaintext);
            }
        }

        private static byte[] BuildAdditionalAuthenticatedData(string deviceId, string requestId)
        {
            var builder = new StringBuilder();
            AppendField(builder, DeviceProvisioningContract.Version.ToString());
            AppendField(builder, deviceId);
            AppendField(builder, requestId);
            return Encoding.UTF8.GetBytes(builder.ToString());
        }

        private static void AppendField(StringBuilder builder, string value)
        {
            builder.Append(value.Length);
            builder.Append(':');
            builder.Append(value);
            builder.Append('\n');
        }
    }
}
