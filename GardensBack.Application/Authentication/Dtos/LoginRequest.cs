using GardensBack.Application.Common.Validation;
using System.ComponentModel.DataAnnotations;

namespace GardensBack.Application.Authentication.Dtos
{
    public class LoginRequest
    {
        [Required, EmailAddress, StringLength(254)]
        public string Email { get; set; } = string.Empty;

        [Required, StringLength(128)]
        public string Password { get; set; } = string.Empty;

        public void Validate()
            => ValidationUtilities.ValidateObject(this);
    }
}
