using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThuHaiDuong.Application.InterfaceService;
using ThuHaiDuong.Application.Payloads.InputModels.Author;
using ThuHaiDuong.Application.Payloads.ResultModels.Author;
using ThuHaiDuong.Shared.Models;

namespace ThuHaiDuong.Controllers;

[ApiController]
[Route("api/admin/authors")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class AdminAuthorController : ControllerBase
{
    private readonly IAuthorService _authorService;
 
    public AdminAuthorController(IAuthorService authorService)
    {
        _authorService = authorService;
    }
 
    [HttpGet]
    [Authorize(Roles = "Contributor,Admin,SuperAdmin")]
    public async Task<ActionResult<PagedResult<AuthorResult>>> GetListAsync(
        [FromQuery] AuthorQuery query)
    {
        var result = await _authorService.GetListAdminAsync(query);
        return Ok(result);
    }
 
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<AuthorResult>> GetByIdAsync(Guid id)
    {
        var result = await _authorService.GetByIdAsync(id);
        return Ok(result);
    }
 
    [HttpPost]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<AuthorResult>> CreateAsync(
        [FromBody] CreateAuthorInput input)
    {
        var result = await _authorService.CreateAsync(input);
        return Ok(result);
    }
 
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<AuthorResult>> UpdateAsync(
        Guid id,
        [FromBody] UpdateAuthorInput input)
    {
        var result = await _authorService.UpdateAsync(id, input);
        return Ok(result);
    }
 
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        await _authorService.DeleteAsync(id);
        return NoContent();
    }
}