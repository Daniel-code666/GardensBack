using GardensBack.Application.Abstractions;
using GardensBack.Application.Common.Exceptions;
using GardensBack.Application.FirmwareReleases.Configuration;
using GardensBack.Application.FirmwareReleases.Constants;
using GardensBack.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Buffers;
using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace GardensBack.Infrastructure.Firmware
{
    public sealed class FirmwareReleasePublicationValidator(IOptions<FirmwareReleaseValidationOptions> options) : IFirmwareReleasePublicationValidator
    {
        private readonly FirmwareReleaseValidationOptions _options = options.Value;

        public void ValidateRelease(FirmwareRelease release, CancellationToken ct)
        {
            ArgumentNullException.ThrowIfNull(release);
            ct.ThrowIfCancellationRequested();

            ValidateMetadata(release);
            ValidateSignature(release);
        }

        public byte[] ValidateArtifactFromDb(FirmwareRelease release, byte[] content)
        {
            if (content.LongLength != release.SizeBytes)
                throw new ConflictException("El tamaño del firmware no coincide con el tamaño reportado.");

            byte[] expectedHash = Convert.FromHexString(release.Sha256);
            byte[] actualHash = SHA256.HashData(content);

            if (!CryptographicOperations.FixedTimeEquals(expectedHash, actualHash))
                throw new ValidationException("El SHA-256 del binario no coincide con el firmware registrado.");

            return content;
        }

        private void ValidateMetadata(FirmwareRelease release)
        {
            if (release.ContractVersion != FirmwareReleaseContract.Version)
                throw new ValidationException("La versión del contrato de firmware no es compatible.");

            if (!IsValidIdentifier(release.ReleaseId))
                throw new ValidationException("El ReleaseId no es válido.");

            if (!IsSemanticVersion(release.Version))
                throw new ValidationException("La versión del firmware no es válida.");

            if (string.IsNullOrWhiteSpace(release.Board) || Encoding.UTF8.GetByteCount(release.Board) > 64)
                throw new ValidationException("La placa del firmware no es válida.");

            long maximumSize = Math.Min(_options.MaximumFirmwareSizeBytes, uint.MaxValue);
            if (release.SizeBytes <= 0 || release.SizeBytes > maximumSize)
                throw new ValidationException("El tamaño del firmware no se encuentra dentro del límite permitido.");

            if (!IsLowerHex(release.Sha256, 64))
                throw new ValidationException("El SHA-256 del firmware no es válido.");

            if (string.IsNullOrWhiteSpace(release.Signature) || release.Signature.Length > 160 ||
                release.Signature.Length % 2 != 0 || !IsLowerHex(release.Signature))
            {
                throw new ValidationException("La firma del firmware no es válida.");
            }

            if (!Uri.TryCreate(release.FirmwareUrl, UriKind.Absolute, out Uri? firmwareUri))
                throw new ValidationException("La URL del firmware no es válida.");

            bool isHttps = firmwareUri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase);
            bool isAllowedHttp = _options.AllowHttp &&
                firmwareUri.Scheme.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase);

            if (!isHttps && !isAllowedHttp)
                throw new ValidationException("La URL del firmware debe utilizar un esquema permitido.");
        }

        private void ValidateSignature(FirmwareRelease release)
        {
            byte[] canonicalPayload = Encoding.UTF8.GetBytes(BuildCanonicalPayload(release));
            byte[] digest = SHA256.HashData(canonicalPayload);
            byte[] signature = Convert.FromHexString(release.Signature);

            using ECDsa ecdsa = ECDsa.Create();
            ecdsa.ImportFromPem(_options.PublicKeyPem);

            if (!ecdsa.VerifyHash(digest, signature, DSASignatureFormat.Rfc3279DerSequence))
                throw new ValidationException("La firma del firmware no corresponde a los metadatos de la publicación.");
        }

        private static string BuildCanonicalPayload(FirmwareRelease release)
        {
            StringBuilder payload = new(512);
            AppendField(payload, release.ContractVersion.ToString(CultureInfo.InvariantCulture));
            AppendField(payload, release.ReleaseId);
            AppendField(payload, release.Version);
            AppendField(payload, release.Board);
            AppendField(payload, release.SizeBytes.ToString(CultureInfo.InvariantCulture));
            AppendField(payload, release.Sha256);
            return payload.ToString();
        }

        private static void AppendField(StringBuilder payload, string value)
        {
            payload.Append(Encoding.UTF8.GetByteCount(value).ToString(CultureInfo.InvariantCulture));
            payload.Append(':');
            payload.Append(value);
            payload.Append('\n');
        }

        private static bool IsValidIdentifier(string? value)
            => !string.IsNullOrWhiteSpace(value) && value.Length <= 64 && value.All(character =>
                char.IsAsciiLetterOrDigit(character) || character == '-' || character == '_');

        private static bool IsSemanticVersion(string? value)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length > 32)
                return false;

            string[] parts = value.Split('.', StringSplitOptions.None);
            return parts.Length == 3 && parts.All(part => part.Length > 0 && part.All(char.IsAsciiDigit));
        }

        private static bool IsLowerHex(string? value, int? requiredLength = null)
            => !string.IsNullOrEmpty(value) && (!requiredLength.HasValue || value.Length == requiredLength.Value) &&
               value.All(character => character is >= '0' and <= '9' or >= 'a' and <= 'f');
    }
}
