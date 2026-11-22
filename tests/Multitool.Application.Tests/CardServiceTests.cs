using Mapster;
using Moq;
using Multitool.Application.Interfaces;
using Multitool.Application.Models.Kanban;
using Multitool.Application.Services;
using Multitool.Domain.Entities.Categories;
using Multitool.Domain.Entities.Kanban;
using Multitool.Domain.Enums;
using Multitool.Domain.Exceptions;
using Multitool.Domain.Interfaces;
using Multitool.Tests.Shared;
using Multitool.Tests.Shared.Assertions;

namespace Multitool.Application.Tests;

public class CardServiceTests
{
    private readonly Mock<IBoardRepository> _boardRepositoryMock;
    private readonly Mock<ICardRepository> _cardRepositoryMock;
    private readonly Mock<IBoardAccessService> _boardAccessServiceMock;
    private readonly Mock<IKanbanCategoryService> _kanbanCategoryServiceMock;
    private readonly Mock<IUserContext> _userContextMock;

    private Card? _getCardResponse;
    private BoardColumn? _getColumnResponse;
    private Board? _getBoardWithDetailsResponse;
    private List<Category> _validateCategoriesResponse;
    private long _createCardResponse;

    private Card? _createdCard;
    private IReadOnlyList<Category>? _createdCategories;
    private Card? _updatedCard;
    private Card? _deletedCard;
    private Card? _movedCard;
    private long _movedTargetColumnId;
    private int _movedPosition;
    private Card? _categoriesCard;
    private IReadOnlyList<Category>? _replacedCategories;

    private const int USER_ID = 1;
    private const long BOARD_ID = 10;
    private const long COLUMN_ID = 20;
    private const long TARGET_COLUMN_ID = 21;
    private const long CARD_ID = 30;
    private const int CATEGORY_ID = 2;
    private const int OTHER_CATEGORY_ID = 4;

    public CardServiceTests()
    {
        MappingConfigBootstrapper.EnsureApplied();

        _boardRepositoryMock = new Mock<IBoardRepository>();
        _cardRepositoryMock = new Mock<ICardRepository>();
        _boardAccessServiceMock = new Mock<IBoardAccessService>();
        _kanbanCategoryServiceMock = new Mock<IKanbanCategoryService>();
        _userContextMock = new Mock<IUserContext>();

        _getCardResponse = CardTestData.CardWithColumn;
        _getColumnResponse = BoardTestData.DefaultColumnWithBoard;
        _getBoardWithDetailsResponse = BoardTestData.DefaultBoardWithDetails;
        _validateCategoriesResponse = [CategoryTestData.KanbanCategory];
        _createCardResponse = CARD_ID;
    }

    private CardService GetService()
    {
        _createdCard = null;
        _createdCategories = null;
        _updatedCard = null;
        _deletedCard = null;
        _movedCard = null;
        _movedTargetColumnId = 0;
        _movedPosition = 0;
        _categoriesCard = null;
        _replacedCategories = null;

        _boardRepositoryMock.Reset();
        _cardRepositoryMock.Reset();
        _boardAccessServiceMock.Reset();
        _kanbanCategoryServiceMock.Reset();
        _userContextMock.Reset();

        _userContextMock.SetupGet(u => u.UserId).Returns(USER_ID);

        _boardRepositoryMock.Setup(r => r.GetColumnByIdAsync(It.IsAny<long>()))
            .ReturnsAsync(_getColumnResponse);

        _boardRepositoryMock.Setup(r => r.GetBoardWithDetailsAsync(It.IsAny<long>(), It.IsAny<int>()))
            .ReturnsAsync(_getBoardWithDetailsResponse);

        _cardRepositoryMock.Setup(r => r.GetCardForOperationAsync(It.IsAny<long>()))
            .ReturnsAsync(_getCardResponse);

        _cardRepositoryMock.Setup(r => r.CreateCardAsync(It.IsAny<Card>(), It.IsAny<IReadOnlyList<Category>>()))
            .Callback<Card, IReadOnlyList<Category>>((card, categories) =>
            {
                _createdCard = card;
                _createdCategories = categories;
            })
            .ReturnsAsync(_createCardResponse);

        _cardRepositoryMock.Setup(r => r.UpdateCardAsync(It.IsAny<Card>()))
            .Callback<Card>(card => _updatedCard = card)
            .Returns(Task.CompletedTask);

        _cardRepositoryMock.Setup(r => r.DeleteCardAsync(It.IsAny<Card>()))
            .Callback<Card>(card => _deletedCard = card)
            .Returns(Task.CompletedTask);

        _cardRepositoryMock.Setup(r => r.MoveCardAsync(It.IsAny<Card>(), It.IsAny<long>(), It.IsAny<int>()))
            .Callback<Card, long, int>((card, targetColumnId, position) =>
            {
                _movedCard = card;
                _movedTargetColumnId = targetColumnId;
                _movedPosition = position;
            })
            .Returns(Task.CompletedTask);

        _cardRepositoryMock.Setup(r => r.ReplaceCardCategoriesAsync(It.IsAny<Card>(), It.IsAny<IReadOnlyList<Category>>()))
            .Callback<Card, IReadOnlyList<Category>>((card, categories) =>
            {
                _categoriesCard = card;
                _replacedCategories = categories;
            })
            .Returns(Task.CompletedTask);

        _kanbanCategoryServiceMock.Setup(s => s.ValidateBoardCategoriesAsync(
                It.IsAny<long>(), It.IsAny<IReadOnlyList<int>>()))
            .ReturnsAsync(_validateCategoriesResponse);

        return new CardService(
            _boardRepositoryMock.Object,
            _cardRepositoryMock.Object,
            _boardAccessServiceMock.Object,
            _kanbanCategoryServiceMock.Object,
            _userContextMock.Object);
    }

    private void AllowCardAccess()
    {
        _boardAccessServiceMock.Setup(s => s.EnsureAccessAsync(BoardAccess.Member, It.IsAny<Board>()))
            .Returns(Task.CompletedTask);
    }

    private void DenyCardAccess()
    {
        _boardAccessServiceMock.Setup(s => s.EnsureAccessAsync(BoardAccess.Member, It.IsAny<Board>()))
            .ThrowsAsync(new NotFoundException($"Board with ID {BOARD_ID} not found."));
    }

    private void AllowColumnAccess()
    {
        _boardAccessServiceMock.Setup(s => s.EnsureAccessAsync(BoardAccess.Column, COLUMN_ID))
            .Returns(Task.CompletedTask);
    }

    // GetCardByIdAsync

    [Fact]
    public async Task GetCardByIdAsync_WhenUserIsMember_ReturnsCardModel()
    {
        // Arrange
        var service = GetService();
        AllowCardAccess();

        // Act
        var result = await service.GetCardByIdAsync(CARD_ID);

        // Assert
        AssertEx.AreEqual(_getCardResponse!.Adapt<CardModel>(), result);

        _cardRepositoryMock.Verify(r => r.GetCardForOperationAsync(CARD_ID), Times.Once);
        _cardRepositoryMock.VerifyNoOtherCalls();

        _boardAccessServiceMock.Verify(s => s.EnsureAccessAsync(BoardAccess.Member, _getCardResponse!.Column.Board), Times.Once);
        _boardAccessServiceMock.VerifyNoOtherCalls();

        _boardRepositoryMock.VerifyNoOtherCalls();
        _kanbanCategoryServiceMock.VerifyNoOtherCalls();
        _userContextMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetCardByIdAsync_WhenCardDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        _getCardResponse = null;
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.GetCardByIdAsync(CARD_ID);

        // Assert
        await AssertEx.Throws<NotFoundException>(act, $"Card with ID {CARD_ID} not found.");

        _cardRepositoryMock.Verify(r => r.GetCardForOperationAsync(CARD_ID), Times.Once);
        _cardRepositoryMock.VerifyNoOtherCalls();

        _boardAccessServiceMock.VerifyNoOtherCalls();
        _boardRepositoryMock.VerifyNoOtherCalls();
        _kanbanCategoryServiceMock.VerifyNoOtherCalls();
        _userContextMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetCardByIdAsync_WhenUserIsNotMember_ThrowsNotFoundException()
    {
        // Arrange
        var service = GetService();
        DenyCardAccess();

        // Act
        Func<Task> act = async () => await service.GetCardByIdAsync(CARD_ID);

        // Assert
        await AssertEx.Throws<NotFoundException>(act, $"Board with ID {BOARD_ID} not found.");

        _cardRepositoryMock.Verify(r => r.GetCardForOperationAsync(CARD_ID), Times.Once);
        _cardRepositoryMock.VerifyNoOtherCalls();

        _boardAccessServiceMock.Verify(s => s.EnsureAccessAsync(BoardAccess.Member, It.IsAny<Board>()), Times.Once);
        _boardAccessServiceMock.VerifyNoOtherCalls();

        _boardRepositoryMock.VerifyNoOtherCalls();
        _kanbanCategoryServiceMock.VerifyNoOtherCalls();
        _userContextMock.VerifyNoOtherCalls();
    }

    // CreateCardAsync

    [Fact]
    public async Task CreateCardAsync_WithCategories_AppendsCardToColumn()
    {
        // Arrange
        var service = GetService();
        AllowColumnAccess();

        // Act
        var result = await service.CreateCardAsync(
            CardTestData.DefaultCreateCardRequest with { ColumnId = COLUMN_ID });

        // Assert
        Assert.Equal(_createCardResponse, result);
        Assert.NotNull(_createdCard);
        Assert.Equal(COLUMN_ID, _createdCard.ColumnId);
        Assert.Equal(CardTestData.DefaultCreateCardRequest.Title, _createdCard.Title);
        Assert.Equal(CardTestData.DefaultCreateCardRequest.Description, _createdCard.Description);
        Assert.Equal(1, _createdCard.Position);
        Assert.Equal(USER_ID, _createdCard.CreatedByUserId);
        Assert.Empty(_createdCard.Categories);
        AssertEx.CloseTo(_createdCard.CreatedAt, DateTime.Now, TimeSpan.FromSeconds(5));
        AssertEx.AreEqual(_validateCategoriesResponse, _createdCategories!);

        _boardAccessServiceMock.Verify(s => s.EnsureAccessAsync(BoardAccess.Column, COLUMN_ID), Times.Once);
        _boardAccessServiceMock.VerifyNoOtherCalls();

        _boardRepositoryMock.Verify(r => r.GetColumnByIdAsync(COLUMN_ID), Times.Once);
        _boardRepositoryMock.Verify(r => r.GetBoardWithDetailsAsync(BOARD_ID, USER_ID), Times.Once);
        _boardRepositoryMock.VerifyNoOtherCalls();

        _kanbanCategoryServiceMock.Verify(
            s => s.ValidateBoardCategoriesAsync(BOARD_ID, It.Is<IReadOnlyList<int>>(ids => ids.Count == 1 && ids[0] == CATEGORY_ID)),
            Times.Once);
        _kanbanCategoryServiceMock.VerifyNoOtherCalls();

        _cardRepositoryMock.Verify(r => r.CreateCardAsync(_createdCard, _validateCategoriesResponse), Times.Once);
        _cardRepositoryMock.VerifyNoOtherCalls();

        _userContextMock.VerifyGet(u => u.UserId, Times.Exactly(2));
    }

    [Fact]
    public async Task CreateCardAsync_WithoutCategoryIds_UsesEmptyCategoryList()
    {
        // Arrange
        var emptyColumnBoard = BoardTestData.DefaultBoardWithDetails;
        emptyColumnBoard.Columns[0].Cards.Clear();
        _getBoardWithDetailsResponse = emptyColumnBoard;
        _validateCategoriesResponse = [];
        var service = GetService();
        AllowColumnAccess();

        // Act
        await service.CreateCardAsync(CardTestData.DefaultCreateCardRequest with
        {
            ColumnId = COLUMN_ID,
            CategoryIds = null
        });

        // Assert
        Assert.NotNull(_createdCard);
        Assert.Equal(0, _createdCard.Position);
        Assert.Empty(_createdCategories!);

        _kanbanCategoryServiceMock.Verify(
            s => s.ValidateBoardCategoriesAsync(BOARD_ID, It.Is<IReadOnlyList<int>>(ids => ids.Count == 0)),
            Times.Once);
        _kanbanCategoryServiceMock.VerifyNoOtherCalls();

        _cardRepositoryMock.Verify(r => r.CreateCardAsync(_createdCard, _validateCategoriesResponse), Times.Once);
    }

    [Fact]
    public async Task CreateCardAsync_WhenUserHasNoColumnAccess_ThrowsNotFoundException()
    {
        // Arrange
        var service = GetService();
        _boardAccessServiceMock.Setup(s => s.EnsureAccessAsync(BoardAccess.Column, COLUMN_ID))
            .ThrowsAsync(new NotFoundException($"Column with ID {COLUMN_ID} not found."));

        // Act
        Func<Task> act = async () => await service.CreateCardAsync(
            CardTestData.DefaultCreateCardRequest with { ColumnId = COLUMN_ID });

        // Assert
        await AssertEx.Throws<NotFoundException>(act, $"Column with ID {COLUMN_ID} not found.");

        _boardAccessServiceMock.Verify(s => s.EnsureAccessAsync(BoardAccess.Column, COLUMN_ID), Times.Once);
        _boardAccessServiceMock.VerifyNoOtherCalls();

        _boardRepositoryMock.VerifyNoOtherCalls();
        _kanbanCategoryServiceMock.VerifyNoOtherCalls();
        _cardRepositoryMock.VerifyNoOtherCalls();
        _userContextMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateCardAsync_WhenColumnDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        _getColumnResponse = null;
        var service = GetService();
        AllowColumnAccess();

        // Act
        Func<Task> act = async () => await service.CreateCardAsync(
            CardTestData.DefaultCreateCardRequest with { ColumnId = COLUMN_ID });

        // Assert
        await AssertEx.Throws<NotFoundException>(act, $"Column with ID {COLUMN_ID} not found.");

        _boardRepositoryMock.Verify(r => r.GetColumnByIdAsync(COLUMN_ID), Times.Once);
        _boardRepositoryMock.VerifyNoOtherCalls();

        _kanbanCategoryServiceMock.VerifyNoOtherCalls();
        _cardRepositoryMock.VerifyNoOtherCalls();
        _userContextMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateCardAsync_WhenBoardDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        _getBoardWithDetailsResponse = null;
        var service = GetService();
        AllowColumnAccess();

        // Act
        Func<Task> act = async () => await service.CreateCardAsync(
            CardTestData.DefaultCreateCardRequest with { ColumnId = COLUMN_ID });

        // Assert
        await AssertEx.Throws<NotFoundException>(act, $"Board with ID {BOARD_ID} not found.");

        _boardRepositoryMock.Verify(r => r.GetColumnByIdAsync(COLUMN_ID), Times.Once);
        _boardRepositoryMock.Verify(r => r.GetBoardWithDetailsAsync(BOARD_ID, USER_ID), Times.Once);
        _boardRepositoryMock.VerifyNoOtherCalls();

        _kanbanCategoryServiceMock.VerifyNoOtherCalls();
        _cardRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateCardAsync_WhenTargetColumnIsNotOnBoard_ThrowsNotFoundException()
    {
        // Arrange
        _getBoardWithDetailsResponse = BoardTestData.DefaultBoard;
        var service = GetService();
        AllowColumnAccess();

        // Act
        Func<Task> act = async () => await service.CreateCardAsync(
            CardTestData.DefaultCreateCardRequest with { ColumnId = COLUMN_ID });

        // Assert
        await AssertEx.Throws<NotFoundException>(act, $"Column with ID {COLUMN_ID} not found.");

        _boardRepositoryMock.Verify(r => r.GetColumnByIdAsync(COLUMN_ID), Times.Once);
        _boardRepositoryMock.Verify(r => r.GetBoardWithDetailsAsync(BOARD_ID, USER_ID), Times.Once);
        _boardRepositoryMock.VerifyNoOtherCalls();

        _kanbanCategoryServiceMock.VerifyNoOtherCalls();
        _cardRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateCardAsync_WhenCategoryIsNotWhitelisted_PropagatesException()
    {
        // Arrange
        var service = GetService();
        AllowColumnAccess();
        _kanbanCategoryServiceMock.Setup(s => s.ValidateBoardCategoriesAsync(
                It.IsAny<long>(), It.IsAny<IReadOnlyList<int>>()))
            .ThrowsAsync(new CategoryNotAvailableForModuleException("Category is not available."));

        // Act
        Func<Task> act = async () => await service.CreateCardAsync(
            CardTestData.DefaultCreateCardRequest with { ColumnId = COLUMN_ID });

        // Assert
        await AssertEx.Throws<CategoryNotAvailableForModuleException>(act, "Category is not available.");

        _kanbanCategoryServiceMock.Verify(
            s => s.ValidateBoardCategoriesAsync(BOARD_ID, It.IsAny<IReadOnlyList<int>>()),
            Times.Once);
        _kanbanCategoryServiceMock.VerifyNoOtherCalls();

        _cardRepositoryMock.Verify(
            r => r.CreateCardAsync(It.IsAny<Card>(), It.IsAny<IReadOnlyList<Category>>()),
            Times.Never);
    }

    // UpdateCardAsync

    [Fact]
    public async Task UpdateCardAsync_WhenUserIsMember_UpdatesTitleAndDescription()
    {
        // Arrange
        var service = GetService();
        AllowCardAccess();

        // Act
        await service.UpdateCardAsync(CARD_ID, CardTestData.DefaultUpdateCardRequest);

        // Assert
        Assert.NotNull(_updatedCard);
        Assert.Same(_getCardResponse, _updatedCard);
        Assert.Equal(CardTestData.DefaultUpdateCardRequest.Title, _updatedCard.Title);
        Assert.Equal(CardTestData.DefaultUpdateCardRequest.Description, _updatedCard.Description);

        _cardRepositoryMock.Verify(r => r.GetCardForOperationAsync(CARD_ID), Times.Once);
        _cardRepositoryMock.Verify(r => r.UpdateCardAsync(_updatedCard), Times.Once);
        _cardRepositoryMock.VerifyNoOtherCalls();

        _boardAccessServiceMock.Verify(s => s.EnsureAccessAsync(BoardAccess.Member, It.IsAny<Board>()), Times.Once);
        _boardAccessServiceMock.VerifyNoOtherCalls();

        _boardRepositoryMock.VerifyNoOtherCalls();
        _kanbanCategoryServiceMock.VerifyNoOtherCalls();
        _userContextMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpdateCardAsync_WhenCardDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        _getCardResponse = null;
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.UpdateCardAsync(CARD_ID, CardTestData.DefaultUpdateCardRequest);

        // Assert
        await AssertEx.Throws<NotFoundException>(act, $"Card with ID {CARD_ID} not found.");

        _cardRepositoryMock.Verify(r => r.GetCardForOperationAsync(CARD_ID), Times.Once);
        _cardRepositoryMock.Verify(r => r.UpdateCardAsync(It.IsAny<Card>()), Times.Never);
        _cardRepositoryMock.VerifyNoOtherCalls();

        _boardAccessServiceMock.VerifyNoOtherCalls();
        _kanbanCategoryServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpdateCardAsync_WhenUserIsNotMember_ThrowsNotFoundException()
    {
        // Arrange
        var service = GetService();
        DenyCardAccess();

        // Act
        Func<Task> act = async () => await service.UpdateCardAsync(CARD_ID, CardTestData.DefaultUpdateCardRequest);

        // Assert
        await AssertEx.Throws<NotFoundException>(act, $"Board with ID {BOARD_ID} not found.");

        _cardRepositoryMock.Verify(r => r.GetCardForOperationAsync(CARD_ID), Times.Once);
        _cardRepositoryMock.Verify(r => r.UpdateCardAsync(It.IsAny<Card>()), Times.Never);
        _cardRepositoryMock.VerifyNoOtherCalls();

        _boardAccessServiceMock.Verify(s => s.EnsureAccessAsync(BoardAccess.Member, It.IsAny<Board>()), Times.Once);
        _boardAccessServiceMock.VerifyNoOtherCalls();

        _kanbanCategoryServiceMock.VerifyNoOtherCalls();
    }

    // DeleteCardAsync

    [Fact]
    public async Task DeleteCardAsync_WhenUserIsMember_DeletesCard()
    {
        // Arrange
        var service = GetService();
        AllowCardAccess();

        // Act
        await service.DeleteCardAsync(CARD_ID);

        // Assert
        AssertEx.AreEqual(_getCardResponse!, _deletedCard!);

        _cardRepositoryMock.Verify(r => r.GetCardForOperationAsync(CARD_ID), Times.Once);
        _cardRepositoryMock.Verify(r => r.DeleteCardAsync(_getCardResponse!), Times.Once);
        _cardRepositoryMock.VerifyNoOtherCalls();

        _boardAccessServiceMock.Verify(s => s.EnsureAccessAsync(BoardAccess.Member, It.IsAny<Board>()), Times.Once);
        _boardAccessServiceMock.VerifyNoOtherCalls();

        _boardRepositoryMock.VerifyNoOtherCalls();
        _kanbanCategoryServiceMock.VerifyNoOtherCalls();
        _userContextMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task DeleteCardAsync_WhenCardDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        _getCardResponse = null;
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.DeleteCardAsync(CARD_ID);

        // Assert
        await AssertEx.Throws<NotFoundException>(act, $"Card with ID {CARD_ID} not found.");

        _cardRepositoryMock.Verify(r => r.GetCardForOperationAsync(CARD_ID), Times.Once);
        _cardRepositoryMock.Verify(r => r.DeleteCardAsync(It.IsAny<Card>()), Times.Never);
        _cardRepositoryMock.VerifyNoOtherCalls();

        _boardAccessServiceMock.VerifyNoOtherCalls();
        _kanbanCategoryServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task DeleteCardAsync_WhenUserIsNotMember_ThrowsNotFoundException()
    {
        // Arrange
        var service = GetService();
        DenyCardAccess();

        // Act
        Func<Task> act = async () => await service.DeleteCardAsync(CARD_ID);

        // Assert
        await AssertEx.Throws<NotFoundException>(act, $"Board with ID {BOARD_ID} not found.");

        _cardRepositoryMock.Verify(r => r.GetCardForOperationAsync(CARD_ID), Times.Once);
        _cardRepositoryMock.Verify(r => r.DeleteCardAsync(It.IsAny<Card>()), Times.Never);
        _cardRepositoryMock.VerifyNoOtherCalls();

        _boardAccessServiceMock.Verify(s => s.EnsureAccessAsync(BoardAccess.Member, It.IsAny<Board>()), Times.Once);
        _boardAccessServiceMock.VerifyNoOtherCalls();

        _kanbanCategoryServiceMock.VerifyNoOtherCalls();
    }

    // MoveCardAsync

    [Fact]
    public async Task MoveCardAsync_WhenTargetColumnIsOnSameBoard_MovesCard()
    {
        // Arrange
        var card = CardTestData.CardWithColumn;
        card.Column.Board.Columns.Add(BoardTestData.SecondColumn);
        _getCardResponse = card;
        var service = GetService();
        AllowCardAccess();

        // Act
        await service.MoveCardAsync(CARD_ID, CardTestData.DefaultMoveCardRequest);

        // Assert
        AssertEx.AreEqual(card, _movedCard!);
        Assert.Equal(TARGET_COLUMN_ID, _movedTargetColumnId);
        Assert.Equal(1, _movedPosition);

        _cardRepositoryMock.Verify(r => r.GetCardForOperationAsync(CARD_ID), Times.Once);
        _cardRepositoryMock.Verify(r => r.MoveCardAsync(card, TARGET_COLUMN_ID, 1), Times.Once);
        _cardRepositoryMock.VerifyNoOtherCalls();

        _boardAccessServiceMock.Verify(s => s.EnsureAccessAsync(BoardAccess.Member, It.IsAny<Board>()), Times.Once);
        _boardAccessServiceMock.VerifyNoOtherCalls();

        _boardRepositoryMock.VerifyNoOtherCalls();
        _kanbanCategoryServiceMock.VerifyNoOtherCalls();
        _userContextMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task MoveCardAsync_WhenCardDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        _getCardResponse = null;
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.MoveCardAsync(CARD_ID, CardTestData.DefaultMoveCardRequest);

        // Assert
        await AssertEx.Throws<NotFoundException>(act, $"Card with ID {CARD_ID} not found.");

        _cardRepositoryMock.Verify(r => r.GetCardForOperationAsync(CARD_ID), Times.Once);
        _cardRepositoryMock.Verify(
            r => r.MoveCardAsync(It.IsAny<Card>(), It.IsAny<long>(), It.IsAny<int>()),
            Times.Never);
        _cardRepositoryMock.VerifyNoOtherCalls();

        _boardAccessServiceMock.VerifyNoOtherCalls();
        _kanbanCategoryServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task MoveCardAsync_WhenUserIsNotMember_ThrowsNotFoundException()
    {
        // Arrange
        var service = GetService();
        DenyCardAccess();

        // Act
        Func<Task> act = async () => await service.MoveCardAsync(CARD_ID, CardTestData.DefaultMoveCardRequest);

        // Assert
        await AssertEx.Throws<NotFoundException>(act, $"Board with ID {BOARD_ID} not found.");

        _cardRepositoryMock.Verify(r => r.GetCardForOperationAsync(CARD_ID), Times.Once);
        _cardRepositoryMock.Verify(
            r => r.MoveCardAsync(It.IsAny<Card>(), It.IsAny<long>(), It.IsAny<int>()),
            Times.Never);
        _cardRepositoryMock.VerifyNoOtherCalls();

        _boardAccessServiceMock.Verify(s => s.EnsureAccessAsync(BoardAccess.Member, It.IsAny<Board>()), Times.Once);
        _boardAccessServiceMock.VerifyNoOtherCalls();

        _kanbanCategoryServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task MoveCardAsync_WhenTargetColumnIsOnAnotherBoard_ThrowsNotFoundException()
    {
        // Arrange
        var service = GetService();
        AllowCardAccess();

        // Act
        Func<Task> act = async () => await service.MoveCardAsync(CARD_ID, CardTestData.DefaultMoveCardRequest);

        // Assert
        await AssertEx.Throws<NotFoundException>(act, $"Column with ID {TARGET_COLUMN_ID} not found.");

        _cardRepositoryMock.Verify(r => r.GetCardForOperationAsync(CARD_ID), Times.Once);
        _cardRepositoryMock.Verify(
            r => r.MoveCardAsync(It.IsAny<Card>(), It.IsAny<long>(), It.IsAny<int>()),
            Times.Never);
        _cardRepositoryMock.VerifyNoOtherCalls();

        _boardAccessServiceMock.Verify(s => s.EnsureAccessAsync(BoardAccess.Member, It.IsAny<Board>()), Times.Once);
        _boardAccessServiceMock.VerifyNoOtherCalls();

        _kanbanCategoryServiceMock.VerifyNoOtherCalls();
    }

    // SetCardCategoriesAsync

    [Fact]
    public async Task SetCardCategoriesAsync_WithWhitelistedIds_ReplacesCategories()
    {
        // Arrange
        _validateCategoriesResponse =
        [
            CategoryTestData.KanbanCategory,
            CategoryTestData.SecondKanbanCategory
        ];
        var service = GetService();
        AllowCardAccess();
        var request = new SetCardCategoriesRequest([CATEGORY_ID, OTHER_CATEGORY_ID]);

        // Act
        await service.SetCardCategoriesAsync(CARD_ID, request);

        // Assert
        AssertEx.AreEqual(_getCardResponse!, _categoriesCard!);
        AssertEx.AreEqual(_validateCategoriesResponse, _replacedCategories!);

        _kanbanCategoryServiceMock.Verify(
            s => s.ValidateBoardCategoriesAsync(BOARD_ID, request.CategoryIds),
            Times.Once);
        _kanbanCategoryServiceMock.VerifyNoOtherCalls();

        _cardRepositoryMock.Verify(r => r.GetCardForOperationAsync(CARD_ID), Times.Once);
        _cardRepositoryMock.Verify(
            r => r.ReplaceCardCategoriesAsync(_getCardResponse!, _validateCategoriesResponse),
            Times.Once);
        _cardRepositoryMock.VerifyNoOtherCalls();

        _boardAccessServiceMock.Verify(s => s.EnsureAccessAsync(BoardAccess.Member, It.IsAny<Board>()), Times.Once);
        _boardAccessServiceMock.VerifyNoOtherCalls();

        _boardRepositoryMock.VerifyNoOtherCalls();
        _userContextMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task SetCardCategoriesAsync_WhenCardDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        _getCardResponse = null;
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.SetCardCategoriesAsync(
            CARD_ID, CardTestData.DefaultSetCardCategoriesRequest);

        // Assert
        await AssertEx.Throws<NotFoundException>(act, $"Card with ID {CARD_ID} not found.");

        _kanbanCategoryServiceMock.VerifyNoOtherCalls();
        _cardRepositoryMock.Verify(r => r.GetCardForOperationAsync(CARD_ID), Times.Once);
        _cardRepositoryMock.Verify(
            r => r.ReplaceCardCategoriesAsync(It.IsAny<Card>(), It.IsAny<IReadOnlyList<Category>>()),
            Times.Never);
        _cardRepositoryMock.VerifyNoOtherCalls();

        _boardAccessServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task SetCardCategoriesAsync_WhenUserIsNotMember_ThrowsNotFoundException()
    {
        // Arrange
        var service = GetService();
        DenyCardAccess();

        // Act
        Func<Task> act = async () => await service.SetCardCategoriesAsync(
            CARD_ID, CardTestData.DefaultSetCardCategoriesRequest);

        // Assert
        await AssertEx.Throws<NotFoundException>(act, $"Board with ID {BOARD_ID} not found.");

        _boardAccessServiceMock.Verify(s => s.EnsureAccessAsync(BoardAccess.Member, It.IsAny<Board>()), Times.Once);
        _boardAccessServiceMock.VerifyNoOtherCalls();

        _kanbanCategoryServiceMock.VerifyNoOtherCalls();
        _cardRepositoryMock.Verify(r => r.GetCardForOperationAsync(CARD_ID), Times.Once);
        _cardRepositoryMock.Verify(
            r => r.ReplaceCardCategoriesAsync(It.IsAny<Card>(), It.IsAny<IReadOnlyList<Category>>()),
            Times.Never);
        _cardRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task SetCardCategoriesAsync_WhenCategoryIsNotWhitelisted_PropagatesException()
    {
        // Arrange
        var service = GetService();
        AllowCardAccess();
        _kanbanCategoryServiceMock.Setup(s => s.ValidateBoardCategoriesAsync(
                It.IsAny<long>(), It.IsAny<IReadOnlyList<int>>()))
            .ThrowsAsync(new CategoryNotAvailableForModuleException("Category is not available."));

        // Act
        Func<Task> act = async () => await service.SetCardCategoriesAsync(
            CARD_ID, CardTestData.DefaultSetCardCategoriesRequest);

        // Assert
        await AssertEx.Throws<CategoryNotAvailableForModuleException>(act, "Category is not available.");

        _kanbanCategoryServiceMock.Verify(
            s => s.ValidateBoardCategoriesAsync(BOARD_ID, It.IsAny<IReadOnlyList<int>>()),
            Times.Once);
        _kanbanCategoryServiceMock.VerifyNoOtherCalls();

        _cardRepositoryMock.Verify(
            r => r.ReplaceCardCategoriesAsync(It.IsAny<Card>(), It.IsAny<IReadOnlyList<Category>>()),
            Times.Never);
    }
}
