using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThuHaiDuong.Application.InterfaceService;
using ThuHaiDuong.Application.Payloads.InputModels.User;
using ThuHaiDuong.Application.Payloads.ResultModels.User;
using ThuHaiDuong.Shared.Models;

namespace ThuHaiDuong.Controllers
{
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }
        
        [HttpGet("all")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetAllUser()
        {
            var result = await _userService.GetAllUserAsync();
            return Ok(result);
        }
        
        [HttpGet]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<PagedResult<UserResult>>> GetListPUsersAsync([FromQuery] UserQuery query)
        {
            var result = await _userService.GetListUsersAsync(query);
            return Ok(result);
        }
        
        [HttpGet("{id}")]
        public async Task<ActionResult<UserResult>> GetUserByIdAsync(Guid id)
        {
            var result = await _userService.GetUserByIdAsync(id);
            return Ok(result);
        }
        
        [HttpPost]
        public async Task<ActionResult<UserResult>> CreateUserAsync([FromBody] CreateUserInput request)
        {
            var result = await _userService.CreateUserAsync(request);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<UserResult>> UpdateUserAsync(Guid id, [FromBody] UpdateUserInput request)
        {
            var result = await _userService.UpdateUserAsync(id, request);
            return Ok(result);
        }
        
        [HttpPut("{id}/roles")]
        public async Task<ActionResult<UserResult>> UpdateRolesAsync(Guid id, [FromBody] UpdateRolesInput request)
        {
            var result = await _userService.UpdateRolesAsync(id, request);
            return Ok(result);
        }
        
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserAsync(Guid id)
        {
            await _userService.DeleteUserAsync(id);
            return NoContent();
        }
    }
}
