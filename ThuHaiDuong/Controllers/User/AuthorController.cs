using Microsoft.AspNetCore.Mvc;
using ThuHaiDuong.Application.InterfaceService;
using ThuHaiDuong.Application.Payloads.InputModels.Author;
using ThuHaiDuong.Application.Payloads.ResultModels.User.Author;
using ThuHaiDuong.Shared.Models;

namespace ThuHaiDuong.Controllers.User;

[ApiController]
[Route("api/authors")]
public class AuthorController : ControllerBase
{
    private readonly IAuthorService _authorService;
 
    public AuthorController(IAuthorService authorService)
    {
        _authorService = authorService;
    }
 
    [HttpGet]
    public async Task<ActionResult<PagedResult<AuthorSummary>>> GetListAsync(
        [FromQuery] AuthorQuery query)
    {
        var result = await _authorService.GetListAsync(query);
        return Ok(result);
    }
 
    [HttpGet("{slug}")]
    public async Task<ActionResult<AuthorSummary>> GetBySlugAsync(string slug)
    {
        var result = await _authorService.GetBySlugAsync(slug);
        return Ok(result);
    }
}