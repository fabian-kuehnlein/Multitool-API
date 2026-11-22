using Moq;
using Multitool.Api.Controllers;
using Multitool.Application.Interfaces;
using Multitool.Application.Models.Kanban;
using Multitool.Tests.Shared;
using Multitool.Tests.Shared.Assertions;

namespace Multitool.Api.Tests.Controllers;

public class KanbanControllerTests
{
    private readonly Mock<IBoardService> _boardServiceMock;
    private readonly Mock<ICardService> _cardServiceMock;
    private readonly Mock<IKanbanCategoryService> _kanbanCategoryServiceMock;

    private List<BoardOverviewModel> _getBoardsResponse;
    private BoardModel _getBoardResponse;
    private List<BoardMemberModel> _getMembersResponse;
    private CardModel _getCardResponse;
    private long _createBoardResponse;
    private long _createCardResponse;

    private const long BOARD_ID = 10;
    private const long COLUMN_ID = 20;
    private const long TARGET_COLUMN_ID = 21;
    private const long CARD_ID = 30;
    private const int CATEGORY_ID = 2;
    private const int USER_ID = 2;
    private const string COLUMN_NAME = "New column";
    private const string USERNAME = "user2";

    public KanbanControllerTests()
    {
        _boardServiceMock = new Mock<IBoardService>();
        _cardServiceMock = new Mock<ICardService>();
        _kanbanCategoryServiceMock = new Mock<IKanbanCategoryService>();

        _getBoardsResponse = [BoardTestData.DefaultBoardOverviewModel];
        _getBoardResponse = BoardTestData.DefaultBoardModel;
        _getMembersResponse = [BoardTestData.DefaultBoardMemberModel];
        _getCardResponse = CardTestData.DefaultCardModel;
        _createBoardResponse = BOARD_ID;
        _createCardResponse = CARD_ID;
    }

    private KanbanController GetController()
    {
        _boardServiceMock.Reset();
        _cardServiceMock.Reset();
        _kanbanCategoryServiceMock.Reset();

        _boardServiceMock.Setup(s => s.GetBoardsOverviewAsync())
            .ReturnsAsync(_getBoardsResponse);
        _boardServiceMock.Setup(s => s.CreateBoardAsync(It.IsAny<CreateBoardRequest>()))
            .ReturnsAsync(_createBoardResponse);
        _boardServiceMock.Setup(s => s.GetBoardByIdAsync(It.IsAny<long>()))
            .ReturnsAsync(_getBoardResponse);
        _boardServiceMock.Setup(s => s.UpdateBoardAsync(It.IsAny<long>(), It.IsAny<UpdateBoardRequest>()))
            .Returns(Task.CompletedTask);
        _boardServiceMock.Setup(s => s.DeleteBoardAsync(It.IsAny<long>()))
            .Returns(Task.CompletedTask);
        _boardServiceMock.Setup(s => s.CreateColumnAsync(It.IsAny<long>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        _boardServiceMock.Setup(s => s.UpdateColumnAsync(It.IsAny<long>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        _boardServiceMock.Setup(s => s.UpdateColumnOrderAsync(
                It.IsAny<long>(), It.IsAny<IReadOnlyDictionary<long, int>>()))
            .Returns(Task.CompletedTask);
        _boardServiceMock.Setup(s => s.DeleteColumnAsync(It.IsAny<long>()))
            .Returns(Task.CompletedTask);
        _boardServiceMock.Setup(s => s.GetBoardMembersAsync(It.IsAny<long>()))
            .ReturnsAsync(_getMembersResponse);
        _boardServiceMock.Setup(s => s.AddBoardMemberAsync(It.IsAny<long>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        _boardServiceMock.Setup(s => s.RemoveBoardMemberAsync(It.IsAny<long>(), It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        _cardServiceMock.Setup(s => s.CreateCardAsync(It.IsAny<CreateCardRequest>()))
            .ReturnsAsync(_createCardResponse);
        _cardServiceMock.Setup(s => s.GetCardByIdAsync(It.IsAny<long>()))
            .ReturnsAsync(_getCardResponse);
        _cardServiceMock.Setup(s => s.UpdateCardAsync(It.IsAny<long>(), It.IsAny<UpdateCardRequest>()))
            .Returns(Task.CompletedTask);
        _cardServiceMock.Setup(s => s.DeleteCardAsync(It.IsAny<long>()))
            .Returns(Task.CompletedTask);
        _cardServiceMock.Setup(s => s.MoveCardAsync(It.IsAny<long>(), It.IsAny<MoveCardRequest>()))
            .Returns(Task.CompletedTask);
        _cardServiceMock.Setup(s => s.SetCardCategoriesAsync(It.IsAny<long>(), It.IsAny<SetCardCategoriesRequest>()))
            .Returns(Task.CompletedTask);

        _kanbanCategoryServiceMock.Setup(s => s.AddCategoryToBoardAsync(It.IsAny<long>(), It.IsAny<int>()))
            .Returns(Task.CompletedTask);
        _kanbanCategoryServiceMock.Setup(s => s.RemoveCategoryFromBoardAsync(It.IsAny<long>(), It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        return new KanbanController(
            _boardServiceMock.Object,
            _cardServiceMock.Object,
            _kanbanCategoryServiceMock.Object);
    }

    // GET api/kanban/boards

    [Fact]
    public async Task GetBoards_ReturnsOkWithBoardOverviews()
    {
        // Arrange
        var controller = GetController();

        // Act
        var result = await controller.GetBoardsOverview();

        // Assert
        AssertEx.Ok(result, _getBoardsResponse);

        _boardServiceMock.Verify(s => s.GetBoardsOverviewAsync(), Times.Once);
        _boardServiceMock.VerifyNoOtherCalls();
        _cardServiceMock.VerifyNoOtherCalls();
        _kanbanCategoryServiceMock.VerifyNoOtherCalls();
    }

    // POST api/kanban/boards

    [Fact]
    public async Task CreateBoard_ReturnsCreatedWithId()
    {
        // Arrange
        var request = BoardTestData.DefaultCreateBoardRequest;
        var controller = GetController();

        // Act
        var result = await controller.CreateBoard(request);

        // Assert
        AssertEx.Created(result, _createBoardResponse);

        _boardServiceMock.Verify(s => s.CreateBoardAsync(request), Times.Once);
        _boardServiceMock.VerifyNoOtherCalls();
        _cardServiceMock.VerifyNoOtherCalls();
        _kanbanCategoryServiceMock.VerifyNoOtherCalls();
    }

    // GET api/kanban/boards/{boardId}

    [Fact]
    public async Task GetBoard_ReturnsOkWithBoard()
    {
        // Arrange
        var controller = GetController();

        // Act
        var result = await controller.GetBoardById(BOARD_ID);

        // Assert
        AssertEx.Ok(result, _getBoardResponse);

        _boardServiceMock.Verify(s => s.GetBoardByIdAsync(BOARD_ID), Times.Once);
        _boardServiceMock.VerifyNoOtherCalls();
        _cardServiceMock.VerifyNoOtherCalls();
        _kanbanCategoryServiceMock.VerifyNoOtherCalls();
    }

    // PUT api/kanban/boards/{boardId}

    [Fact]
    public async Task UpdateBoard_ReturnsNoContent()
    {
        // Arrange
        var request = BoardTestData.DefaultUpdateBoardRequest;
        var controller = GetController();

        // Act
        var result = await controller.UpdateBoard(BOARD_ID, request);

        // Assert
        AssertEx.NoContent(result);

        _boardServiceMock.Verify(s => s.UpdateBoardAsync(BOARD_ID, request), Times.Once);
        _boardServiceMock.VerifyNoOtherCalls();
        _cardServiceMock.VerifyNoOtherCalls();
        _kanbanCategoryServiceMock.VerifyNoOtherCalls();
    }

    // DELETE api/kanban/boards/{boardId}

    [Fact]
    public async Task DeleteBoard_ReturnsNoContent()
    {
        // Arrange
        var controller = GetController();

        // Act
        var result = await controller.DeleteBoard(BOARD_ID);

        // Assert
        AssertEx.NoContent(result);

        _boardServiceMock.Verify(s => s.DeleteBoardAsync(BOARD_ID), Times.Once);
        _boardServiceMock.VerifyNoOtherCalls();
        _cardServiceMock.VerifyNoOtherCalls();
        _kanbanCategoryServiceMock.VerifyNoOtherCalls();
    }

    // POST api/kanban/boards/{boardId}/columns

    [Fact]
    public async Task CreateColumn_ReturnsNoContent()
    {
        // Arrange
        var controller = GetController();

        // Act
        var result = await controller.CreateColumn(BOARD_ID, COLUMN_NAME);

        // Assert
        AssertEx.NoContent(result);

        _boardServiceMock.Verify(s => s.CreateColumnAsync(BOARD_ID, COLUMN_NAME), Times.Once);
        _boardServiceMock.VerifyNoOtherCalls();
        _cardServiceMock.VerifyNoOtherCalls();
        _kanbanCategoryServiceMock.VerifyNoOtherCalls();
    }

    // PUT api/kanban/columns/{columnId}

    [Fact]
    public async Task UpdateColumn_ReturnsNoContent()
    {
        // Arrange
        var controller = GetController();

        // Act
        var result = await controller.UpdateColumn(COLUMN_ID, COLUMN_NAME);

        // Assert
        AssertEx.NoContent(result);

        _boardServiceMock.Verify(s => s.UpdateColumnAsync(COLUMN_ID, COLUMN_NAME), Times.Once);
        _boardServiceMock.VerifyNoOtherCalls();
        _cardServiceMock.VerifyNoOtherCalls();
        _kanbanCategoryServiceMock.VerifyNoOtherCalls();
    }

    // PUT api/kanban/boards/{boardId}/columns/order

    [Fact]
    public async Task UpdateColumnOrder_ReturnsNoContent()
    {
        // Arrange
        var positions = new Dictionary<long, int>
        {
            [COLUMN_ID] = 0,
            [TARGET_COLUMN_ID] = 1
        };
        var controller = GetController();

        // Act
        var result = await controller.UpdateColumnOrder(BOARD_ID, positions);

        // Assert
        AssertEx.NoContent(result);

        _boardServiceMock.Verify(s => s.UpdateColumnOrderAsync(BOARD_ID, positions), Times.Once);
        _boardServiceMock.VerifyNoOtherCalls();
        _cardServiceMock.VerifyNoOtherCalls();
        _kanbanCategoryServiceMock.VerifyNoOtherCalls();
    }

    // DELETE api/kanban/columns/{columnId}

    [Fact]
    public async Task DeleteColumn_ReturnsNoContent()
    {
        // Arrange
        var controller = GetController();

        // Act
        var result = await controller.DeleteColumn(COLUMN_ID);

        // Assert
        AssertEx.NoContent(result);

        _boardServiceMock.Verify(s => s.DeleteColumnAsync(COLUMN_ID), Times.Once);
        _boardServiceMock.VerifyNoOtherCalls();
        _cardServiceMock.VerifyNoOtherCalls();
        _kanbanCategoryServiceMock.VerifyNoOtherCalls();
    }

    // POST api/kanban/boards/{boardId}/cards

    [Fact]
    public async Task CreateCard_ReturnsCreatedWithId()
    {
        // Arrange
        var request = CardTestData.DefaultCreateCardRequest;
        var controller = GetController();

        // Act
        var result = await controller.CreateCard(request);

        // Assert
        AssertEx.Created(result, _createCardResponse);

        _cardServiceMock.Verify(s => s.CreateCardAsync(request), Times.Once);
        _cardServiceMock.VerifyNoOtherCalls();
        _boardServiceMock.VerifyNoOtherCalls();
        _kanbanCategoryServiceMock.VerifyNoOtherCalls();
    }

    // GET api/kanban/cards/{cardId}

    [Fact]
    public async Task GetCard_ReturnsOkWithCard()
    {
        // Arrange
        var controller = GetController();

        // Act
        var result = await controller.GetCardById(CARD_ID);

        // Assert
        AssertEx.Ok(result, _getCardResponse);

        _cardServiceMock.Verify(s => s.GetCardByIdAsync(CARD_ID), Times.Once);
        _cardServiceMock.VerifyNoOtherCalls();
        _boardServiceMock.VerifyNoOtherCalls();
        _kanbanCategoryServiceMock.VerifyNoOtherCalls();
    }

    // PUT api/kanban/cards/{cardId}

    [Fact]
    public async Task UpdateCard_ReturnsNoContent()
    {
        // Arrange
        var request = CardTestData.DefaultUpdateCardRequest;
        var controller = GetController();

        // Act
        var result = await controller.UpdateCard(CARD_ID, request);

        // Assert
        AssertEx.NoContent(result);

        _cardServiceMock.Verify(s => s.UpdateCardAsync(CARD_ID, request), Times.Once);
        _cardServiceMock.VerifyNoOtherCalls();
        _boardServiceMock.VerifyNoOtherCalls();
        _kanbanCategoryServiceMock.VerifyNoOtherCalls();
    }

    // DELETE api/kanban/cards/{cardId}

    [Fact]
    public async Task DeleteCard_ReturnsNoContent()
    {
        // Arrange
        var controller = GetController();

        // Act
        var result = await controller.DeleteCard(CARD_ID);

        // Assert
        AssertEx.NoContent(result);

        _cardServiceMock.Verify(s => s.DeleteCardAsync(CARD_ID), Times.Once);
        _cardServiceMock.VerifyNoOtherCalls();
        _boardServiceMock.VerifyNoOtherCalls();
        _kanbanCategoryServiceMock.VerifyNoOtherCalls();
    }

    // PATCH api/kanban/cards/{cardId}/move

    [Fact]
    public async Task MoveCard_ReturnsNoContent()
    {
        // Arrange
        var request = CardTestData.DefaultMoveCardRequest;
        var controller = GetController();

        // Act
        var result = await controller.MoveCard(CARD_ID, request);

        // Assert
        AssertEx.NoContent(result);

        _cardServiceMock.Verify(s => s.MoveCardAsync(CARD_ID, request), Times.Once);
        _cardServiceMock.VerifyNoOtherCalls();
        _boardServiceMock.VerifyNoOtherCalls();
        _kanbanCategoryServiceMock.VerifyNoOtherCalls();
    }

    // PUT api/kanban/cards/{cardId}/categories

    [Fact]
    public async Task SetCardCategories_ReturnsNoContent()
    {
        // Arrange
        var request = CardTestData.DefaultSetCardCategoriesRequest;
        var controller = GetController();

        // Act
        var result = await controller.SetCardCategories(CARD_ID, request);

        // Assert
        AssertEx.NoContent(result);

        _cardServiceMock.Verify(s => s.SetCardCategoriesAsync(CARD_ID, request), Times.Once);
        _cardServiceMock.VerifyNoOtherCalls();
        _boardServiceMock.VerifyNoOtherCalls();
        _kanbanCategoryServiceMock.VerifyNoOtherCalls();
    }

    // POST api/kanban/boards/{boardId}/categories/{categoryId}

    [Fact]
    public async Task AddBoardCategory_ReturnsNoContent()
    {
        // Arrange
        var controller = GetController();

        // Act
        var result = await controller.AddBoardCategory(BOARD_ID, CATEGORY_ID);

        // Assert
        AssertEx.NoContent(result);

        _kanbanCategoryServiceMock.Verify(
            s => s.AddCategoryToBoardAsync(BOARD_ID, CATEGORY_ID), Times.Once);
        _kanbanCategoryServiceMock.VerifyNoOtherCalls();
        _boardServiceMock.VerifyNoOtherCalls();
        _cardServiceMock.VerifyNoOtherCalls();
    }

    // DELETE api/kanban/boards/{boardId}/categories/{categoryId}

    [Fact]
    public async Task RemoveBoardCategory_ReturnsNoContent()
    {
        // Arrange
        var controller = GetController();

        // Act
        var result = await controller.RemoveBoardCategory(BOARD_ID, CATEGORY_ID);

        // Assert
        AssertEx.NoContent(result);

        _kanbanCategoryServiceMock.Verify(
            s => s.RemoveCategoryFromBoardAsync(BOARD_ID, CATEGORY_ID), Times.Once);
        _kanbanCategoryServiceMock.VerifyNoOtherCalls();
        _boardServiceMock.VerifyNoOtherCalls();
        _cardServiceMock.VerifyNoOtherCalls();
    }

    // GET api/kanban/boards/{boardId}/members

    [Fact]
    public async Task GetBoardMembers_ReturnsOkWithMembers()
    {
        // Arrange
        var controller = GetController();

        // Act
        var result = await controller.GetBoardMembers(BOARD_ID);

        // Assert
        AssertEx.Ok(result, _getMembersResponse);

        _boardServiceMock.Verify(s => s.GetBoardMembersAsync(BOARD_ID), Times.Once);
        _boardServiceMock.VerifyNoOtherCalls();
        _cardServiceMock.VerifyNoOtherCalls();
        _kanbanCategoryServiceMock.VerifyNoOtherCalls();
    }

    // POST api/kanban/boards/{boardId}/members

    [Fact]
    public async Task AddBoardMember_ReturnsNoContent()
    {
        // Arrange
        var controller = GetController();

        // Act
        var result = await controller.AddBoardMember(BOARD_ID, USERNAME);

        // Assert
        AssertEx.NoContent(result);

        _boardServiceMock.Verify(s => s.AddBoardMemberAsync(BOARD_ID, USERNAME), Times.Once);
        _boardServiceMock.VerifyNoOtherCalls();
        _cardServiceMock.VerifyNoOtherCalls();
        _kanbanCategoryServiceMock.VerifyNoOtherCalls();
    }

    // DELETE api/kanban/boards/{boardId}/members/{userId}

    [Fact]
    public async Task RemoveBoardMember_ReturnsNoContent()
    {
        // Arrange
        var controller = GetController();

        // Act
        var result = await controller.RemoveBoardMember(BOARD_ID, USER_ID);

        // Assert
        AssertEx.NoContent(result);

        _boardServiceMock.Verify(s => s.RemoveBoardMemberAsync(BOARD_ID, USER_ID), Times.Once);
        _boardServiceMock.VerifyNoOtherCalls();
        _cardServiceMock.VerifyNoOtherCalls();
        _kanbanCategoryServiceMock.VerifyNoOtherCalls();
    }
}
