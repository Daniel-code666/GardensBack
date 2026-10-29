namespace GardensBack.Application.DeviceProvisioning.Dtos.Device
{
    using GardensBack.Application.Common.Validation;
    using GardensBack.Application.DeviceProvisioning.Constants;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Devuelve al ESP un código de error estable sin incluir tokens, claves ni detalles internos.
    /// </summary>
    public class ProvisioningErrorResponse
    {
        [Required, StringLength(DeviceProvisioningContract.MaximumErrorCodeLength, MinimumLength = 1)]
        [RegularExpression(DeviceProvisioningContract.ErrorCodePattern)]
        public required string ErrorCode { get; set; }

        [StringLength(200)]
        public string? Message { get; set; }

        public void Validate()
            => ValidationUtilities.ValidateObject(this);
    }
}
