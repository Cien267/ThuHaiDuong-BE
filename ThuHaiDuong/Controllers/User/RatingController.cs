using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThuHaiDuong.Application.InterfaceService;
using ThuHaiDuong.Application.Payloads.InputModels.User.Rating;
using ThuHaiDuong.Application.Payloads.ResultModels.User.Rating;

namespace ThuHaiDuong.Controllers.User;

[ApiController]
[Route("api/ratings")]
public class RatingController : ControllerBase
{
    private readonly IRatingService _ratingService;
 
    public RatingController(IRatingService ratingService)
    {
        _ratingService = ratingService;
    }
 
    private Guid? CurrentUserId => User.Identity?.IsAuthenticated == true
        ? Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!)
        : null;
 
    [HttpGet("stories/{storyId:guid}")]
    public async Task<ActionResult<RatingSummary>> GetSummaryAsync(Guid storyId)
    {
        var result = await _ratingService.GetSummaryAsync(storyId, CurrentUserId);
        return Ok(result);
    }
 
    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<RatingResult>> CreateAsync(
        [FromBody] CreateRatingInput input)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _ratingService.CreateAsync(userId, input);
        return CreatedAtAction(nameof(GetSummaryAsync), new { storyId = result.StoryId }, result);
    }
}