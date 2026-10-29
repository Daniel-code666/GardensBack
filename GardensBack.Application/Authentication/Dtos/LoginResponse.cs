using GardensBack.Application.Users.Dtos;

namespace GardensBack.Application.Authentication.Dtos
{
    public class LoginResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string TokenType { get; set; } = string.Empty;
        public DateTime ExpiresAtUtc { get; set; }
        public UserLoginResponse User { get; set; } = new UserLoginResponse();
    }
}
