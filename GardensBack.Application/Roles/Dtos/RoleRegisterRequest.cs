using GardensBack.Application.Common.Validation;
using System.ComponentModel.DataAnnotations;

namespace GardensBack.Application.Roles.Dtos
{
    public class RoleRegisterRequest
    {
        public int? Id { get; set; }
        [Required]
        public required string Name { get; set; }
        [Required]
        public required string Description { get; set; }
        [Required]
        public required bool AdministrativeRole { get; set; } = false;
        public void Validate()
            => ValidationUtilities.ValidateObject(this);
    }
}
