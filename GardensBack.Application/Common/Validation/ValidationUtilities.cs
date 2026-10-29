using System.ComponentModel.DataAnnotations;

namespace GardensBack.Application.Common.Validation
{
    public class ValidationUtilities
    {
        public static void ValidateObject(object? request)
        {
            if (request is null)
                throw new ValidationException("El cuerpo de la petición es obligatorio.");

            var errors = new List<ValidationResult>();
            if (!Validator.TryValidateObject(request, new ValidationContext(request), errors, true))
                throw new ValidationException(string.Join(" ", errors.Select(x => x.ErrorMessage)));
        }

        public static void ValidateId(int id)
        {
            if (id <= 0)
                throw new ValidationException("El identificador del usuario debe ser positivo.");
        }
    }
}
