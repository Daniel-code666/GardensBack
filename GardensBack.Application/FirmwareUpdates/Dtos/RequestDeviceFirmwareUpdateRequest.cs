using GardensBack.Application.Common.Validation;
using System.ComponentModel.DataAnnotations;

namespace GardensBack.Application.FirmwareUpdates.Dtos
{
    public class RequestDeviceFirmwareUpdateRequest
    {
        [Required, StringLength(64, MinimumLength = 1)]
        [RegularExpression(@"^[A-Za-z0-9_-]+$")]
        public required string DeviceId { get; set; }

        [Required, StringLength(64, MinimumLength = 1)]
        [RegularExpression(@"^[A-Za-z0-9_-]+$")]
        public required string ReleaseId { get; set; }

        public void Validate()
            => ValidationUtilities.ValidateObject(this);
    }
}
