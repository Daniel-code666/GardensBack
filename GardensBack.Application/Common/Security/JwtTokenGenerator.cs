using GardensBack.Domain.Entities;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Globalization;
using System.Text;

namespace GardensBack.Application.Common.Security
{
    public class JwtTokenGenerator(IOptions<JwtOptions> options)
    {
        private readonly JwtOptions _options = options.Value;

        public (string AccessToken, DateTime ExpiresAtUtc) CreateToken(User user)
        {
            var settings = _options;
            var now = DateTime.UtcNow;
            var expires = now.AddMinutes(settings.ExpirationMinutes);
            
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString(CultureInfo.InvariantCulture)),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
                new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture), ClaimValueTypes.Integer64),
                new Claim(JwtRegisteredClaimNames.Name, $"{user.Name}".Trim()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("role", user.Role.Name),
                new Claim("token_version", user.TokenVersion.ToString(CultureInfo.InvariantCulture))
            };

            var credentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SigningKey)), SecurityAlgorithms.HmacSha256);
            var jwt = new JwtSecurityToken(settings.Issuer, settings.Audience, claims, now, expires, credentials);

            return (new JwtSecurityTokenHandler().WriteToken(jwt), expires);
        }
    }
}
