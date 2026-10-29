namespace GardensBack.Application.DeviceProvisioning.Dtos.Device
{
    using GardensBack.Application.Common.Validation;
    using GardensBack.Application.DeviceProvisioning.Constants;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Informa al ESP que la operación existe, pero todavía no tiene una respuesta definitiva.
    /// </summary>
    public class ProvisioningPendingResponse
    {
        [Required]
        [RegularExpression("^" + DeviceProvisioningContract.PendingStatus + "$")]
        public string Status { get; set; } = DeviceProvisioningContract.PendingStatus;

        public void Validate()
            => ValidationUtilities.ValidateObject(this);
    }
}
