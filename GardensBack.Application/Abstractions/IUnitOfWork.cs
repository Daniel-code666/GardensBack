using GardensBack.Application.Common.Results;

namespace GardensBack.Application.Abstractions
{
    public interface IUnitOfWork
    {
        Task<OperationResult<T>> ExecuteCommandAsync<T>(string operationName, Func<CancellationToken, Task<T>> operation, CancellationToken ct);
    }
}
