using System.ComponentModel.DataAnnotations;

namespace GardensBack.Application.DeviceProvisioning.Dtos.Web
{
    using GardensBack.Application.Common.Validation;
    using GardensBack.Application.DeviceProvisioning.Constants;

    /// <summary>
    /// Contiene lo que la aplicación necesita para construir el mensaje device_bootstrap
    /// </summary>
    public class StartDeviceProvisioningResponse : IValidatableObject
    {
        public Guid DevicePublicId { get; set; }

        [Required, StringLength(DeviceProvisioningContract.MaximumIdentifierLength, MinimumLength = 1)]
        [RegularExpression(DeviceProvisioningContract.IdentifierPattern)]
        public required string DeviceId { get; set; }

        [Required, StringLength(DeviceProvisioningContract.MaximumBootstrapRequestIdLength, MinimumLength = 1)]
        [RegularExpression(DeviceProvisioningContract.IdentifierPattern)]
        public required string BootstrapRequestId { get; set; }

        [Range(DeviceProvisioningContract.Version, DeviceProvisioningContract.Version)]
        public int ContractVersion { get; set; } = DeviceProvisioningContract.Version;

        [Required, StringLength(256, MinimumLength = 12), Url]
        public required string ServiceBaseUrl { get; set; }

        [Required, StringLength(512, MinimumLength = 16)]
        public required string ProvisioningToken { get; set; }

        public DateTime ExpiresAtUtc { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (DevicePublicId == Guid.Empty)
                yield return new ValidationResult("DevicePublicId es obligatorio.", [nameof(DevicePublicId)]);

            if (ExpiresAtUtc == default || ExpiresAtUtc.Kind != DateTimeKind.Utc)
                yield return new ValidationResult("ExpiresAtUtc debe contener una fecha UTC válida.", [nameof(ExpiresAtUtc)]);

            if (!Uri.TryCreate(ServiceBaseUrl, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp) ||
                !string.IsNullOrEmpty(uri.Query) || !string.IsNullOrEmpty(uri.Fragment))
            {
                yield return new ValidationResult("ServiceBaseUrl debe ser una URL HTTP o HTTPS absoluta, sin query ni fragmento.", [nameof(ServiceBaseUrl)]);
            }
        }

        public void Validate()
            => ValidationUtilities.ValidateObject(this);
    }
}
