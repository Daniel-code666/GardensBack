using GardensBack.Application.Common.Results;
using GardensBack.Application.Users.Dtos;

namespace GardensBack.Application.Users.Abstractions
{
    public interface IUserService
    {
        Task<OperationResult<UserRegisterRequest>> CreateUser(UserRegisterRequest user, CancellationToken ct);
        Task<OperationResult<InitialUserRegisterRequest>> Register(InitialUserRegisterRequest user, CancellationToken ct);
        Task<OperationResult<UserRegisterRequest>> CreateUserMiddleware(UserRegisterRequest user, CancellationToken ct);
        Task<OperationResult<UserRegisterRequest>> UpdateUser(UserRegisterRequest user, int requestedByUserId, bool isAdmin, CancellationToken ct);
        Task<OperationResult<int>> DeleteUser(int id, int requestedByUserId, bool isAdmin, CancellationToken ct);
        Task<OperationResult<UserRegisterRequest>> GetUserById(int id, int requestedByUserId, bool isAdmin, CancellationToken ct);
        Task<OperationResult<UserRegisterRequest>> GetUserByEmail(string email, int requestedByUserId, bool isAdmin, CancellationToken ct);
        Task<OperationResult<IEnumerable<UserRegisterRequest>>> GetAllUsers(int? roleId, string? roleName, CancellationToken ct);
    }
}
