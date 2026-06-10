using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Multitool.Application.Interfaces;

namespace Multitool.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoryController(ICategoryService categoryService) : ControllerBase
{
    /// <summary>
    /// Returns a list of all available categories
    /// </summary>
    [Authorize]
    [HttpGet("categories")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await categoryService.GetCategoriesAsync();
        return Ok(categories);
    }
}
