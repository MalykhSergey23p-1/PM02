using System.Threading.Tasks;  // ← ЭТА СТРОКА ВАЖНА! Она добавляет поддержку Task<>
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductionAPI.Models;
using ProductionAPI.Services;

namespace ProductionAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.Register(request);

            if (result == null)
                return BadRequest(new { message = "Пользователь уже существует" });

            return Ok(result);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _authService.Login(request);

            if (result == null)
                return Unauthorized(new { message = "Неверное имя пользователя или пароль" });

            return Ok(result);
        }

        [HttpGet("verify")]
        [Authorize]
        public IActionResult Verify()
        {
            return Ok(new { message = "Токен действителен", user = User.Identity?.Name });
        }
    }
}