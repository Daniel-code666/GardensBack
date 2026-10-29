using GardensBack.Application.Authentication.Abstractions;
using GardensBack.Application.Authentication.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GardensBack.Controllers
{
    /// <summary>
    /// Expone las operaciones de autenticación de usuarios.
    /// </summary>
    /// <param name="authService">Servicio encargado de autenticar usuarios y emitir tokens.</param>
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        private readonly IAuthService _authService = authService;

        /// <summary>
        /// Autentica un usuario mediante correo y contraseña.
        /// </summary>
        /// <param name="login">Credenciales de acceso del usuario.</param>
        /// <param name="ct">Token de cancelación de la solicitud.</param>
        /// <returns>El token de acceso y los datos básicos del usuario autenticado.</returns>
        [AllowAnonymous]
        [HttpPost("Login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest login, CancellationToken ct)
            => Ok(await _authService.LoginAsync(login, ct));
    }
}
