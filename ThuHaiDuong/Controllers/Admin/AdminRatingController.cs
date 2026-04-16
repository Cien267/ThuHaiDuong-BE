using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThuHaiDuong.Application.InterfaceService;
using ThuHaiDuong.Application.Payloads.ResultModels.User.Rating;
using ThuHaiDuong.Shared.Models;

namespace ThuHaiDuong.Controllers;

[ApiController]
[Route("api/admin/ratings")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
    Roles = "Admin,SuperAdmin")]
public class AdminRatingController : ControllerBase
{
    private readonly IRatingService _ratingService;
 
    public AdminRatingController(IRatingService ratingService)
    {
        _ratingService = ratingService;
    }
 
    [HttpGet]
    public async Task<ActionResult<PagedResult<RatingResult>>> GetListAsync(
        [FromQuery] Guid storyId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _ratingService.GetListAdminAsync(storyId, page, pageSize);
        return Ok(result);
    }
 
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        await _ratingService.DeleteAsync(id);
        return NoContent();
    }
}