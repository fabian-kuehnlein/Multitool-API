using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Multitool.Application.Interfaces;
using Multitool.Application.Models.Kanban;

namespace Multitool.Api.Controllers;

[ApiController]
[Route("api/kanban")]
public class KanbanController(
    IBoardService boardService,
    ICardService cardService,
    IKanbanCategoryService kanbanCategoryService) : ControllerBase
{
    /// <summary>Returns all boards the current user is a member of.</summary>
    [Authorize]
    [HttpGet("boards")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetBoardsOverview()
    {
        var boards = await boardService.GetBoardsOverviewAsync();
        return Ok(boards);
    }

    /// <summary>Creates a board with default columns for the current user.</summary>
    [Authorize]
    [HttpPost("boards")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateBoard([FromBody] CreateBoardRequest request)
    {
        var id = await boardService.CreateBoardAsync(request);
        return StatusCode(StatusCodes.Status201Created, id);
    }

    /// <summary>Returns the full board including columns, cards, members and the category whitelist.</summary>
    [Authorize]
    [HttpGet("boards/{boardId}")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetBoardById(long boardId)
    {
        var board = await boardService.GetBoardByIdAsync(boardId);
        return Ok(board);
    }

    /// <summary>Updates the name and description of an owned board.</summary>
    [Authorize]
    [HttpPut("boards/{boardId}")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateBoard(long boardId, [FromBody] UpdateBoardRequest request)
    {
        await boardService.UpdateBoardAsync(boardId, request);
        return NoContent();
    }

    /// <summary>Deletes an owned board and its columns/cards.</summary>
    [Authorize]
    [HttpDelete("boards/{boardId}")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteBoard(long boardId)
    {
        await boardService.DeleteBoardAsync(boardId);
        return NoContent();
    }

    /// <summary>Adds a column at the end of an accessible board.</summary>
    [Authorize]
    [HttpPost("boards/{boardId}/columns")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateColumn(long boardId, [FromBody] string name)
    {
        await boardService.CreateColumnAsync(boardId, name);
        return NoContent();
    }

    /// <summary>Renames an accessible column.</summary>
    [Authorize]
    [HttpPut("columns/{columnId}")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateColumn(long columnId, [FromBody] string name)
    {
        await boardService.UpdateColumnAsync(columnId, name);
        return NoContent();
    }

    /// <summary>Applies the new positions for all supplied columns of a board in one request.</summary>
    [Authorize]
    [HttpPut("boards/{boardId}/columns/order")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateColumnOrder(
        long boardId, [FromBody] Dictionary<long, int> positions)
    {
        await boardService.UpdateColumnOrderAsync(boardId, positions);
        return NoContent();
    }

    /// <summary>Deletes an accessible column; its cards are removed by database cascade.</summary>
    [Authorize]
    [HttpDelete("columns/{columnId}")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteColumn(long columnId)
    {
        await boardService.DeleteColumnAsync(columnId);
        return NoContent();
    }

    /// <summary>Creates a card at the end of the target column.</summary>
    [Authorize]
    [HttpPost("boards/{boardId}/cards")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateCard([FromBody] CreateCardRequest request)
    {
        var id = await cardService.CreateCardAsync(request);
        return StatusCode(StatusCodes.Status201Created, id);
    }

    /// <summary>Returns a single card for a member of its board.</summary>
    [Authorize]
    [HttpGet("cards/{cardId}")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCardById(long cardId)
    {
        var card = await cardService.GetCardByIdAsync(cardId);
        return Ok(card);
    }

    /// <summary>Updates the title and description of a card.</summary>
    [Authorize]
    [HttpPut("cards/{cardId}")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateCard(long cardId, [FromBody] UpdateCardRequest request)
    {
        await cardService.UpdateCardAsync(cardId, request);
        return NoContent();
    }

    /// <summary>Deletes an accessible card.</summary>
    [Authorize]
    [HttpDelete("cards/{cardId}")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteCard(long cardId)
    {
        await cardService.DeleteCardAsync(cardId);
        return NoContent();
    }

    /// <summary>Moves a card inside its board and clamps the resulting position.</summary>
    [Authorize]
    [HttpPatch("cards/{cardId}/move")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> MoveCard(long cardId, [FromBody] MoveCardRequest request)
    {
        await cardService.MoveCardAsync(cardId, request);
        return NoContent();
    }

    /// <summary>Replaces the card categories with whitelisted categories of its board.</summary>
    [Authorize]
    [HttpPut("cards/{cardId}/categories")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SetCardCategories(long cardId, [FromBody] SetCardCategoriesRequest request)
    {
        await cardService.SetCardCategoriesAsync(cardId, request);
        return NoContent();
    }

    /// <summary>Adds an owner category to the board whitelist.</summary>
    [Authorize]
    [HttpPost("boards/{boardId}/categories/{categoryId}")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddBoardCategory(long boardId, int categoryId)
    {
        await kanbanCategoryService.AddCategoryToBoardAsync(boardId, categoryId);
        return NoContent();
    }

    /// <summary>Removes a category from the board whitelist.</summary>
    [Authorize]
    [HttpDelete("boards/{boardId}/categories/{categoryId}")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RemoveBoardCategory(long boardId, int categoryId)
    {
        await kanbanCategoryService.RemoveCategoryFromBoardAsync(boardId, categoryId);
        return NoContent();
    }

    /// <summary>Lists the members of an accessible board.</summary>
    [Authorize]
    [HttpGet("boards/{boardId}/members")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetBoardMembers(long boardId)
    {
        var members = await boardService.GetBoardMembersAsync(boardId);
        return Ok(members);
    }

    /// <summary>Adds an active user to an owned board by username.</summary>
    [Authorize]
    [HttpPost("boards/{boardId}/members")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddBoardMember(long boardId, [FromBody] string username)
    {
        await boardService.AddBoardMemberAsync(boardId, username);
        return NoContent();
    }

    /// <summary>Removes a user from an owned board.</summary>
    [Authorize]
    [HttpDelete("boards/{boardId}/members/{userId}")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RemoveBoardMember(long boardId, int userId)
    {
        await boardService.RemoveBoardMemberAsync(boardId, userId);
        return NoContent();
    }
}
