using System.ComponentModel.DataAnnotations;

namespace GardensBack.Application.Common.Results
{
    public class OperationResult<T>
    {
        [Required]
        public required string OperationName { get; set; }
        public required bool Successful { get; set; }
        public int? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
        public T? Result { get; set; }

        public static OperationResult<T> Success(string operationName, T? result)
            => new()
            {
                OperationName = operationName,
                Successful = true,
                Result = result
            };

        public static OperationResult<T> Failure(string operationName, int errorCode, string errorMessage)
            => new()
            { 
                Successful = false,
                OperationName = operationName,
                ErrorMessage = errorMessage,
                ErrorCode = errorCode,
            };
    }
}
