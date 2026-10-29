using GardensBack.Application.Authentication.Dtos;

namespace GardensBack.Application.Authentication.Abstractions
{
    public interface IAuthService
    {
        Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct);
    }
}
