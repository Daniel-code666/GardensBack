using GardensBack.Application.Common.Results;
using GardensBack.Domain.Entities;

namespace GardensBack.Application.Abstractions
{
    public interface IUserRepository
    {
        Task<OperationResult<User>> GetUserByEmail(string email, CancellationToken ct);
        Task<OperationResult<IEnumerable<User>>> GetAllUsers(int? roleId, string? roleName, CancellationToken ct);
        Task<OperationResult<User>> CreateUser(User user, CancellationToken ct);
        Task<OperationResult<User>> CreateUserMiddleware(User user, CancellationToken ct);
        Task<OperationResult<int>> DeleteUser(int id, CancellationToken ct);
        Task<OperationResult<User>> GetUserById(int id, CancellationToken ct);
        Task<OperationResult<string>> GetUserEmailById(int id, CancellationToken ct);
        Task<bool> HasAdmin(CancellationToken ct);
        Task<bool> SaveChanges(CancellationToken ct);
    }
}
