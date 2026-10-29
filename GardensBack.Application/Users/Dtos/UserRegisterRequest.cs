using GardensBack.Application.Common.Validation;
using System.ComponentModel.DataAnnotations;

namespace GardensBack.Application.Users.Dtos
{
    public class UserRegisterRequest
    {
        public int? Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(254)]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        public required string Password { get; set; }

        [Required]
        public required int RoleId { get; set; }

        public string RoleName { get; set; } = string.Empty;

        public void Validate()
            => ValidationUtilities.ValidateObject(this);
    }
}
