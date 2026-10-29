namespace GardensBack.Application.Common.Exceptions
{
    public abstract class AppException(string message) : Exception(message);
    public class ValidationException(string message) : AppException(message);
    public class UnathorizedException(string message) : AppException(message);
    public class NotFoundException(string message) : AppException(message);
    public class ConflictException(string message) : AppException(message);
}
