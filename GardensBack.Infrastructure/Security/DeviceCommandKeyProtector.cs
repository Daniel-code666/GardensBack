using GardensBack.Application.FirmwareUpdates.Abstractions;
using GardensBack.Application.FirmwareUpdates.Configuration;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

namespace GardensBack.Infrastructure.Security
{
    public class DeviceCommandKeyProtector(IOptions<DeviceCommandSecurityOptions> options) : IDeviceCommandKeyProtector
    {
        private const byte FormatVersion = 1;
        private const int NonceSize = 12;
        private const int TagSize = 16;
        private readonly byte[] _encryptionKey = Convert.FromBase64String(options.Value.EncryptionKey);

        public string Protect(string keyHex)
        {
            byte[] plaintext = Convert.FromHexString(keyHex);
            if (plaintext.Length != 32)
            {
                CryptographicOperations.ZeroMemory(plaintext);
                throw new ArgumentException("La clave de comandos debe tener 256 bits.", nameof(keyHex));
            }

            byte[] nonce = RandomNumberGenerator.GetBytes(NonceSize);
            byte[] ciphertext = new byte[plaintext.Length];
            byte[] tag = new byte[TagSize];
            byte[] protectedValue = new byte[1 + NonceSize + ciphertext.Length + TagSize];

            try
            {
                using var aes = new AesGcm(_encryptionKey, TagSize);
                aes.Encrypt(nonce, plaintext, ciphertext, tag);

                protectedValue[0] = FormatVersion;
                nonce.CopyTo(protectedValue, 1);
                ciphertext.CopyTo(protectedValue, 1 + NonceSize);
                tag.CopyTo(protectedValue, 1 + NonceSize + ciphertext.Length);
                return Convert.ToBase64String(protectedValue);
            }
            finally
            {
                CryptographicOperations.ZeroMemory(plaintext);
                CryptographicOperations.ZeroMemory(ciphertext);
                CryptographicOperations.ZeroMemory(tag);
                CryptographicOperations.ZeroMemory(protectedValue);
            }
        }

        public byte[] Unprotect(string protectedKey)
        {
            byte[] protectedValue = Convert.FromBase64String(protectedKey);
            if (protectedValue.Length != 1 + NonceSize + 32 + TagSize || protectedValue[0] != FormatVersion)
            {
                CryptographicOperations.ZeroMemory(protectedValue);
                throw new CryptographicException("La clave protegida del dispositivo no tiene un formato válido.");
            }

            byte[] plaintext = new byte[32];
            try
            {
                ReadOnlySpan<byte> nonce = protectedValue.AsSpan(1, NonceSize);
                ReadOnlySpan<byte> ciphertext = protectedValue.AsSpan(1 + NonceSize, 32);
                ReadOnlySpan<byte> tag = protectedValue.AsSpan(1 + NonceSize + 32, TagSize);
                using var aes = new AesGcm(_encryptionKey, TagSize);
                aes.Decrypt(nonce, ciphertext, tag, plaintext);
                return plaintext;
            }
            catch
            {
                CryptographicOperations.ZeroMemory(plaintext);
                throw;
            }
            finally
            {
                CryptographicOperations.ZeroMemory(protectedValue);
            }
        }
    }
}
