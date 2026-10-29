using GardensBack.Application.Abstractions;
using GardensBack.Application.Common.Security;
using GardensBack.Domain.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GardensBack.Authentication
{
    /// <summary>
    /// Registra la autenticación JWT y la política de autorización predeterminada de la API.
    /// </summary>
    public static class AuthenticationRegistration
    {
        /// <summary>
        /// Configura la autenticación Bearer, la validación de tokens y la autorización global.
        /// </summary>
        /// <param name="services">Colección de servicios de la aplicación.</param>
        /// <param name="configuration">Configuración que contiene la sección JWT.</param>
        /// <returns>La misma colección de servicios para continuar la configuración.</returns>
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<JwtOptions>()
                .Bind(configuration.GetSection(JwtOptions.SectionName))
                .Validate(x => !string.IsNullOrWhiteSpace(x.Issuer), "Configure Jwt: Issuer")
                .Validate(x => !string.IsNullOrWhiteSpace(x.Audience), "Configure Jwt: Audience")
                .Validate(x => !string.IsNullOrWhiteSpace(x.SigningKey) && Encoding.UTF8.GetByteCount(x.SigningKey) >= 32,
                    "Configure Jwt:SigningKey con una clave secreta aleatoria de al menos 32 bytes mediante User Secrets o Jwt__SigningKey.")
                .Validate(x => x.ExpirationMinutes is >= 1 and <= 1440, "Jwt:ExpirationMinutes debe estar entre 1 y 1440.")
                .ValidateOnStart();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
            services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
                .Configure<IOptions<JwtOptions>>((options, jwtOptions) =>
                {
                    var jwt = jwtOptions.Value;
                    options.MapInboundClaims = false;
                    options.IncludeErrorDetails = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwt.Issuer,
                        ValidateAudience = true,
                        ValidAudience = jwt.Audience,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
                        ValidateLifetime = true,
                        RequireExpirationTime = true,
                        RequireSignedTokens = true,
                        ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
                        ClockSkew = TimeSpan.FromSeconds(30),
                        NameClaimType = JwtRegisteredClaimNames.Name,
                        RoleClaimType = "role"
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = async context =>
                        {
                            if (!int.TryParse(context.Principal?.FindFirstValue(JwtRegisteredClaimNames.Sub), out var userId))
                            {
                                context.Fail("Invalid identity");
                                return;
                            }

                            var users = context.HttpContext.RequestServices.GetRequiredService<IUserRepository>();
                            var result = await users.GetUserById(userId, context.HttpContext.RequestAborted);

                            if (!result.Successful || result.Result is null)
                            {
                                context.Fail("El usuario ya no está disponible. Inicie sesión nuevamente.");
                                return;
                            }

                            User user = result.Result;

                            if (!user.Active || user.Role is null || !user.Role.Active || context.Principal?.FindFirstValue("role") != user.Role.Name
                                || !int.TryParse(context.Principal?.FindFirstValue("token_version"), out var version) || version != user.TokenVersion)
                            {
                                context.Fail("El usuario o su rol ya no están disponibles. Inicie sesión nuevamente.");
                            }
                        }                        
                    };
                });

            services.AddAuthorizationBuilder().SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

            return services;
        }
    }
}
