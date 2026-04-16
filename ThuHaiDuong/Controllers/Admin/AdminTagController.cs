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
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,SuperAdmin")]
public class AdminTagController : ControllerBase
{
    private readonly ITagService _tagService;
 
    public AdminTagController(ITagService tagService)
    {
        _tagService = tagService;
    }
 
    [HttpGet]
    public async Task<ActionResult<PagedResult<TagResult>>> GetListAsync(
        [FromQuery] TagQuery query)
    {
        var result = await _tagService.GetListAsync(query);
        return Ok(result);
    }
 
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TagResult>> GetByIdAsync(Guid id)
    {
        var result = await _tagService.GetByIdAsync(id);
        return Ok(result);
    }
 
    [HttpPost]
    public async Task<ActionResult<TagResult>> CreateAsync(
        [FromBody] CreateTagInput input)
    {
        var result = await _tagService.CreateAsync(input);
        return CreatedAtAction(nameof(GetByIdAsync), new { id = result.Id }, result);
    }
 
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TagResult>> UpdateAsync(
        Guid id,
        [FromBody] UpdateTagInput input)
    {
        var result = await _tagService.UpdateAsync(id, input);
        return Ok(result);
    }
 
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        await _tagService.DeleteAsync(id);
        return NoContent();
    }
}