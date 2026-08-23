using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Multitool.Application.Interfaces;
using Multitool.Application.Models;

namespace Multitool.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TodoController(ITodoService todoService) : ControllerBase
{
    /// <summary>
    /// Returns all todos.
    /// </summary>
    [Authorize]
    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTodos()
    {
        var todos = await todoService.GetTodosAsync();
        return Ok(todos);
    }

    /// <summary>
    /// Creates a new todo.
    /// </summary>
    [Authorize]
    [HttpPost]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateTodo([FromBody] CreateTodoDto createTodoDto)
    {
        var id = await todoService.CreateTodoAsync(createTodoDto);
        return StatusCode(StatusCodes.Status201Created, id);
    }

    /// <summary>
    /// Updates an existing todo.
    /// </summary>
    [Authorize]
    [HttpPut("{id}")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateTodo(int id, [FromBody] UpdateTodoDto updateTodoDto)
    {
        await todoService.UpdateTodoAsync(id, updateTodoDto);
        return NoContent();
    }

    /// <summary>
    /// Toggles the completion status of a todo.
    /// </summary>
    [Authorize]
    [HttpPatch("{id}/toggle")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ToggleTodo(int id)
    {
        await todoService.ToggleDoneAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Deletes a todo.
    /// </summary>
    [Authorize]
    [HttpDelete("{id}")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteTodo(int id)
    {
        await todoService.DeleteTodoAsync(id);
        return NoContent();
    }
}
