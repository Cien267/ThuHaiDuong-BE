using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThuHaiDuong.Application.InterfaceService;
using ThuHaiDuong.Application.Payloads.InputModels.Category;
using ThuHaiDuong.Application.Payloads.ResultModels.Category;
using ThuHaiDuong.Shared.Models;

namespace ThuHaiDuong.Controllers;

[ApiController]
[Route("api/admin/categories")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,SuperAdmin")]
public class AdminCategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;
 
    public AdminCategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }
 
    [HttpGet]
    public async Task<ActionResult<PagedResult<CategoryResult>>> GetListAsync(
        [FromQuery] CategoryQuery query)
    {
        var result = await _categoryService.GetListAsync(query);
        return Ok(result);
    }
 
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CategoryResult>> GetByIdAsync(Guid id)
    {
        var result = await _categoryService.GetByIdAsync(id);
        return Ok(result);
    }
 
    [HttpPost]
    public async Task<ActionResult<CategoryResult>> CreateAsync(
        [FromBody] CreateCategoryInput input)
    {
        var result = await _categoryService.CreateAsync(input);
        return CreatedAtAction(nameof(GetByIdAsync), new { id = result.Id }, result);
    }
 
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CategoryResult>> UpdateAsync(
        Guid id,
        [FromBody] UpdateCategoryInput input)
    {
        var result = await _categoryService.UpdateAsync(id, input);
        return Ok(result);
    }
 
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        await _categoryService.DeleteAsync(id);
        return NoContent();
    }
}