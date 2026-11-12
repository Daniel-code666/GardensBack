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
        
        [StringLength(128)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public required int RoleId { get; set; }

        public string RoleName { get; set; } = string.Empty;
        public bool? Active { get; set; }

        public void Validate()
            => ValidationUtilities.ValidateObject(this);
    }
}
