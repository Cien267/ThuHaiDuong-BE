using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThuHaiDuong.Application.InterfaceService;
using ThuHaiDuong.Application.Payloads.InputModels.Auth;

namespace ThuHaiDuong.Controllers
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
        /*[HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterInput request)
        {
            return Ok(await _authService.RegisterAsync(request));
        }*/

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginInput request)
        {
            return Ok(await _authService.LoginAsync(request));
        }


        [HttpGet("me")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetUserInfo()
        {
            var result = await _authService.GetUserInfoAsync();
            return Ok(result);
        }
        
        [HttpPost("logout")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Logout()
        {
            await _authService.LogoutAsync();
            return NoContent();
        }


        [HttpPut("update-profile/{userId}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> UpdateUser(Guid userId, [FromBody] UpdateProfileInput request)
        {
            var result = await _authService.UpdateProfileAsync(userId, request);
            return Ok(result);
        }

        [HttpPut("change-password/{userId}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> ChangePassword([FromRoute] Guid userId, [FromBody] ChangePasswordInput request)
        {
            var result = await _authService.ChangePasswordAsync(userId, request);
            return Ok(result);
        }
    }
}
