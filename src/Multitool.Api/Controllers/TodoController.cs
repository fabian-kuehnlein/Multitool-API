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
        var todos = await todoService.GetAllTodosAsync();
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
    public async Task<IActionResult> CreateTodo([FromBody] CreateTodoDto createTodoDto)
    {
        var createdTodo = await todoService.CreateTodoAsync(createTodoDto);
        return CreatedAtAction(nameof(GetTodos), new { id = createdTodo.Id }, createdTodo);
    }

    /// <summary>
    /// Updates an existing todo.
    /// </summary>
    [Authorize]
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTodo(int id)
    {
        await todoService.DeleteTodoAsync(id);
        return NoContent();
    }
}
