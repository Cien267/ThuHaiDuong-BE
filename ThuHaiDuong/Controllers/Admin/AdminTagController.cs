using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThuHaiDuong.Application.InterfaceService;
using ThuHaiDuong.Application.Payloads.InputModels.Tag;
using ThuHaiDuong.Application.Payloads.ResultModels.Tag;
using ThuHaiDuong.Shared.Models;

namespace ThuHaiDuong.Controllers;

[ApiController]
[Route("api/admin/tags")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class AdminTagController : ControllerBase
{
    private readonly ITagService _tagService;
 
    public AdminTagController(ITagService tagService)
    {
        _tagService = tagService;
    }
 
    [HttpGet]
    [Authorize(Roles = "Contributor,Admin,SuperAdmin")]
    public async Task<ActionResult<PagedResult<TagResult>>> GetListAsync(
        [FromQuery] TagQuery query)
    {
        var result = await _tagService.GetListAsync(query);
        return Ok(result);
    }
 
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<TagResult>> GetByIdAsync(Guid id)
    {
        var result = await _tagService.GetByIdAsync(id);
        return Ok(result);
    }
 
    [HttpPost]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<TagResult>> CreateAsync(
        [FromBody] CreateTagInput input)
    {
        var result = await _tagService.CreateAsync(input);
        return Ok(result);
    }
 
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<TagResult>> UpdateAsync(
        Guid id,
        [FromBody] UpdateTagInput input)
    {
        var result = await _tagService.UpdateAsync(id, input);
        return Ok(result);
    }
 
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        await _tagService.DeleteAsync(id);
        return NoContent();
    }
}