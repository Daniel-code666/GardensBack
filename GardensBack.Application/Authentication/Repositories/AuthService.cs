using GardensBack.Application.Abstractions;
using GardensBack.Application.Authentication.Abstractions;
using GardensBack.Application.Authentication.Dtos;
using GardensBack.Application.Common.Exceptions;
using GardensBack.Application.Common.Security;
using GardensBack.Application.Users.Dtos;
using GardensBack.Domain.Entities;

namespace GardensBack.Application.Authentication.Repositories
{
    using AutoMapper;

    public class AuthService(IUserRepository userRepository, JwtTokenGenerator tokenGenerator, IMapper mapper) : IAuthService
    {
        private readonly IUserRepository _userRepository = userRepository;
        private readonly JwtTokenGenerator _tokenGenerator = tokenGenerator;
        private readonly IMapper _mapper = mapper;

        public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct)
        {
            ValidateRequest(request);

            var result = await _userRepository.GetUserByEmail(request.Email, ct);

            if (result.Successful && result.Result is not null)
            {
                User user = result.Result;

                if (!PasswordUtility.Verify(user, request.Password))
                    throw new UnathorizedException("Wrong password");

                (string accessToken, DateTime expiresAtUtc) = _tokenGenerator.CreateToken(user);

                return new LoginResponse
                {
                    AccessToken = accessToken,
                    TokenType = "Bearer",
                    ExpiresAtUtc = expiresAtUtc,
                    User = _mapper.Map<UserLoginResponse>(user)
                };
            }

            throw new NotFoundException("User not found");
        }

        private static void ValidateRequest(LoginRequest request)
        {
            if (request is null) throw new ValidationException("Body required");

            request.Validate();
        }
    }
}
