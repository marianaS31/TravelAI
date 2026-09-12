using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelAI.DTOs;
using TravelAI.Interfaces;

namespace TravelAI.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("registar")]
        [AllowAnonymous]
        public async Task<IActionResult> Registar(RegistarRequestDTO dto)
        {
            try
            {
                var resultado = await _authService.RegistarAsync(dto);
                return Ok(resultado);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginRequestDTO dto)
        {
            try
            {
                var resultado = await _authService.LoginAsync(dto);
                return Ok(resultado);
            }
            catch (InvalidOperationException ex)
            {
                return Unauthorized(new { erro = ex.Message });
            }
        }

        [HttpGet("me")]
        [Authorize]
        public IActionResult Me()
        {
            var utilizadorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var nome = User.FindFirst(ClaimTypes.Name)?.Value;
            return Ok(new { utilizadorId, email, nome });
        }
    }
}
