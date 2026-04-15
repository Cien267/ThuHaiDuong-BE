using Microsoft.AspNetCore.Mvc;
using ThuHaiDuong.Application.InterfaceService;
using ThuHaiDuong.Application.Payloads.ResultModels.User.Category;

namespace ThuHaiDuong.Controllers.User;

[ApiController]
[Route("api/categories")]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;
 
    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }
 
    [HttpGet("tree")]
    public async Task<ActionResult<List<CategorySummary>>> GetTreeAsync()
    {
        var result = await _categoryService.GetTreeAsync();
        return Ok(result);
    }
 
    [HttpGet("{slug}")]
    public async Task<ActionResult<CategorySummary>> GetBySlugAsync(string slug)
    {
        var result = await _categoryService.GetBySlugAsync(slug);
        return Ok(result);
    }
}