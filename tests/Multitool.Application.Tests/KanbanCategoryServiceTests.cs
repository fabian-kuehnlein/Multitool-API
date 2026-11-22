using Mapster;
using Moq;
using Multitool.Application.Interfaces;
using Multitool.Application.Mappings;
using Multitool.Application.Services;
using Multitool.Domain.Entities.Categories;
using Multitool.Domain.Entities.Kanban;
using Multitool.Domain.Enums;
using Multitool.Domain.Exceptions;
using Multitool.Domain.Interfaces;
using Multitool.Tests.Shared;
using Multitool.Tests.Shared.Assertions;

namespace Multitool.Application.Tests;

public class KanbanCategoryServiceTests
{
    private readonly Mock<IBoardRepository> _boardRepositoryMock;
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IBoardAccessService> _boardAccessServiceMock;

    private Board? _getBoardForAccessCheckResponse;
    private List<Category> _getBoardCategoriesResponse;
    private List<Category> _getKanbanCategoriesForOwnerResponse;

    private Board? _whitelistBoard;
    private Category? _addedCategory;
    private Category? _removedCategory;

    private const int USER_ID = 1;
    private const long BOARD_ID = 10;
    private const int CATEGORY_ID = 2;
    private const int OTHER_CATEGORY_ID = 4;

    public KanbanCategoryServiceTests()
    {
        TypeAdapterConfig.GlobalSettings.Apply(new MappingConfig());

        _boardRepositoryMock = new Mock<IBoardRepository>();
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _boardAccessServiceMock = new Mock<IBoardAccessService>();

        _getBoardForAccessCheckResponse = BoardTestData.BoardWithCategories;
        _getBoardCategoriesResponse =
        [
            CategoryTestData.KanbanCategory,
            CategoryTestData.SecondKanbanCategory
        ];
        _getKanbanCategoriesForOwnerResponse =
        [
            CategoryTestData.KanbanCategory,
            CategoryTestData.SecondKanbanCategory
        ];
    }

    private KanbanCategoryService GetService()
    {
        _whitelistBoard = null;
        _addedCategory = null;
        _removedCategory = null;

        _boardRepositoryMock.Reset();
        _categoryRepositoryMock.Reset();

        _boardRepositoryMock.Setup(r => r.GetBoardForAccessCheckAsync(It.IsAny<long>()))
            .ReturnsAsync(_getBoardForAccessCheckResponse);

        _boardRepositoryMock.Setup(r => r.GetBoardCategoriesForValidationAsync(It.IsAny<long>()))
            .ReturnsAsync(_getBoardCategoriesResponse);

        _boardRepositoryMock.Setup(r => r.AddBoardCategoryAsync(It.IsAny<Board>(), It.IsAny<Category>()))
            .Callback<Board, Category>((board, category) =>
            {
                _whitelistBoard = board;
                _addedCategory = category;
            })
            .Returns(Task.CompletedTask);

        _boardRepositoryMock.Setup(r => r.RemoveBoardCategoryAsync(It.IsAny<Board>(), It.IsAny<Category>()))
            .Callback<Board, Category>((board, category) =>
            {
                _whitelistBoard = board;
                _removedCategory = category;
            })
            .Returns(Task.CompletedTask);

        _categoryRepositoryMock.Setup(r => r.GetKanbanCategoriesForOwnerAsync(It.IsAny<int>()))
            .ReturnsAsync(_getKanbanCategoriesForOwnerResponse);

        return new KanbanCategoryService(
            _boardRepositoryMock.Object,
            _categoryRepositoryMock.Object,
            _boardAccessServiceMock.Object);
    }

    // ValidateBoardCategoriesAsync

    [Fact]
    public async Task ValidateBoardCategoriesAsync_WhenAllIdsAreWhitelisted_ReturnsTrackedCategories()
    {
        // Arrange
        _boardAccessServiceMock.Setup(s => s.EnsureAccessAsync(BoardAccess.Member, BOARD_ID))
            .Returns(Task.CompletedTask);
        var service = GetService();

        // Act
        var result = await service.ValidateBoardCategoriesAsync(BOARD_ID, [CATEGORY_ID, OTHER_CATEGORY_ID]);

        // Assert
        AssertEx.AreEqual(_getBoardCategoriesResponse, result);

        _boardAccessServiceMock.Verify(s => s.EnsureAccessAsync(BoardAccess.Member, BOARD_ID), Times.Once);
        _boardAccessServiceMock.VerifyNoOtherCalls();

        _boardRepositoryMock.Verify(r => r.GetBoardCategoriesForValidationAsync(BOARD_ID), Times.Once);
        _boardRepositoryMock.VerifyNoOtherCalls();

        _categoryRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ValidateBoardCategoriesAsync_WhenIdsContainDuplicates_ReturnsEachCategoryOnce()
    {
        // Arrange
        _boardAccessServiceMock.Setup(s => s.EnsureAccessAsync(BoardAccess.Member, BOARD_ID))
            .Returns(Task.CompletedTask);
        var service = GetService();

        // Act
        var result = await service.ValidateBoardCategoriesAsync(BOARD_ID, [CATEGORY_ID, CATEGORY_ID]);

        // Assert
        AssertEx.AreEqual(new List<Category> { CategoryTestData.KanbanCategory }, result);

        _boardAccessServiceMock.Verify(s => s.EnsureAccessAsync(BoardAccess.Member, BOARD_ID), Times.Once);
        _boardAccessServiceMock.VerifyNoOtherCalls();

        _boardRepositoryMock.Verify(r => r.GetBoardCategoriesForValidationAsync(BOARD_ID), Times.Once);
        _boardRepositoryMock.VerifyNoOtherCalls();

        _categoryRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ValidateBoardCategoriesAsync_WhenIdIsNotWhitelisted_ThrowsCategoryNotAvailableForModuleException()
    {
        // Arrange
        _boardAccessServiceMock.Setup(s => s.EnsureAccessAsync(BoardAccess.Member, BOARD_ID))
            .Returns(Task.CompletedTask);
        _getBoardCategoriesResponse = [CategoryTestData.KanbanCategory];
        var service = GetService();

        // Act
        Func<Task> act = async () =>
            await service.ValidateBoardCategoriesAsync(BOARD_ID, [CATEGORY_ID, OTHER_CATEGORY_ID]);

        // Assert
        await AssertEx.Throws<CategoryNotAvailableForModuleException>(
            act, $"Category with ID {OTHER_CATEGORY_ID} is not available for board {BOARD_ID}.");

        _boardAccessServiceMock.Verify(s => s.EnsureAccessAsync(BoardAccess.Member, BOARD_ID), Times.Once);
        _boardAccessServiceMock.VerifyNoOtherCalls();

        _boardRepositoryMock.Verify(r => r.GetBoardCategoriesForValidationAsync(BOARD_ID), Times.Once);
        _boardRepositoryMock.VerifyNoOtherCalls();

        _categoryRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ValidateBoardCategoriesAsync_WhenUserIsNotMember_ThrowsNotFoundException()
    {
        // Arrange
        _boardAccessServiceMock.Setup(s => s.EnsureAccessAsync(BoardAccess.Member, BOARD_ID))
            .ThrowsAsync(new NotFoundException($"Board with ID {BOARD_ID} not found."));
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.ValidateBoardCategoriesAsync(BOARD_ID, [CATEGORY_ID]);

        // Assert
        await AssertEx.Throws<NotFoundException>(act, $"Board with ID {BOARD_ID} not found.");

        _boardAccessServiceMock.Verify(s => s.EnsureAccessAsync(BoardAccess.Member, BOARD_ID), Times.Once);
        _boardAccessServiceMock.VerifyNoOtherCalls();

        _boardRepositoryMock.Verify(r => r.GetBoardCategoriesForValidationAsync(It.IsAny<long>()), Times.Never);
        _boardRepositoryMock.VerifyNoOtherCalls();

        _categoryRepositoryMock.VerifyNoOtherCalls();
    }

    // AddCategoryToBoardAsync

    [Fact]
    public async Task AddCategoryToBoardAsync_WhenOwnerAndCategoryIsAvailable_AddsCategoryToWhitelist()
    {
        // Arrange
        _boardAccessServiceMock.Setup(s => s.EnsureAccessAsync(BoardAccess.Owner, BOARD_ID))
            .Returns(Task.CompletedTask);
        var service = GetService();

        // Act
        await service.AddCategoryToBoardAsync(BOARD_ID, CATEGORY_ID);

        // Assert
        AssertEx.AreEqual(BoardTestData.BoardWithCategories, _whitelistBoard);
        AssertEx.AreEqual(CategoryTestData.KanbanCategory, _addedCategory);

        _boardAccessServiceMock.Verify(s => s.EnsureAccessAsync(BoardAccess.Owner, BOARD_ID), Times.Once);
        _boardAccessServiceMock.VerifyNoOtherCalls();

        _boardRepositoryMock.Verify(r => r.GetBoardForAccessCheckAsync(BOARD_ID), Times.Once);
        _boardRepositoryMock.Verify(r => r.AddBoardCategoryAsync(_whitelistBoard!, _addedCategory!), Times.Once);
        _boardRepositoryMock.VerifyNoOtherCalls();

        _categoryRepositoryMock.Verify(r => r.GetKanbanCategoriesForOwnerAsync(USER_ID), Times.Once);
        _categoryRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task AddCategoryToBoardAsync_WhenUserIsNotOwner_ThrowsForbiddenException()
    {
        // Arrange
        _boardAccessServiceMock.Setup(s => s.EnsureAccessAsync(BoardAccess.Owner, BOARD_ID))
            .ThrowsAsync(new ForbiddenException($"User {USER_ID} is not the owner of board {BOARD_ID}."));
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.AddCategoryToBoardAsync(BOARD_ID, CATEGORY_ID);

        // Assert
        await AssertEx.Throws<ForbiddenException>(act, $"User {USER_ID} is not the owner of board {BOARD_ID}.");

        _boardAccessServiceMock.Verify(s => s.EnsureAccessAsync(BoardAccess.Owner, BOARD_ID), Times.Once);
        _boardAccessServiceMock.VerifyNoOtherCalls();

        _boardRepositoryMock.Verify(r => r.GetBoardForAccessCheckAsync(It.IsAny<long>()), Times.Never);
        _boardRepositoryMock.Verify(r => r.AddBoardCategoryAsync(It.IsAny<Board>(), It.IsAny<Category>()), Times.Never);
        _boardRepositoryMock.VerifyNoOtherCalls();

        _categoryRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task AddCategoryToBoardAsync_WhenCategoryIsNotAvailable_ThrowsAndDoesNotModifyWhitelist()
    {
        // Arrange
        _boardAccessServiceMock.Setup(s => s.EnsureAccessAsync(BoardAccess.Owner, BOARD_ID))
            .Returns(Task.CompletedTask);
        _getKanbanCategoriesForOwnerResponse = [CategoryTestData.SecondKanbanCategory];
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.AddCategoryToBoardAsync(BOARD_ID, CATEGORY_ID);

        // Assert
        await AssertEx.Throws<CategoryNotAvailableForModuleException>(
            act, $"Category with ID {CATEGORY_ID} is not available for the kanban module.");

        _boardAccessServiceMock.Verify(s => s.EnsureAccessAsync(BoardAccess.Owner, BOARD_ID), Times.Once);
        _boardAccessServiceMock.VerifyNoOtherCalls();

        _boardRepositoryMock.Verify(r => r.GetBoardForAccessCheckAsync(BOARD_ID), Times.Once);
        _boardRepositoryMock.Verify(r => r.AddBoardCategoryAsync(It.IsAny<Board>(), It.IsAny<Category>()), Times.Never);
        _boardRepositoryMock.VerifyNoOtherCalls();

        _categoryRepositoryMock.Verify(r => r.GetKanbanCategoriesForOwnerAsync(USER_ID), Times.Once);
        _categoryRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task AddCategoryToBoardAsync_WhenBoardDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        _boardAccessServiceMock.Setup(s => s.EnsureAccessAsync(BoardAccess.Owner, BOARD_ID))
            .Returns(Task.CompletedTask);
        _getBoardForAccessCheckResponse = null;
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.AddCategoryToBoardAsync(BOARD_ID, CATEGORY_ID);

        // Assert
        await AssertEx.Throws<NotFoundException>(act, $"Board with ID {BOARD_ID} not found.");

        _boardAccessServiceMock.Verify(s => s.EnsureAccessAsync(BoardAccess.Owner, BOARD_ID), Times.Once);
        _boardAccessServiceMock.VerifyNoOtherCalls();

        _boardRepositoryMock.Verify(r => r.GetBoardForAccessCheckAsync(BOARD_ID), Times.Once);
        _boardRepositoryMock.Verify(r => r.AddBoardCategoryAsync(It.IsAny<Board>(), It.IsAny<Category>()), Times.Never);
        _boardRepositoryMock.VerifyNoOtherCalls();

        _categoryRepositoryMock.VerifyNoOtherCalls();
    }

    // RemoveCategoryFromBoardAsync

    [Fact]
    public async Task RemoveCategoryFromBoardAsync_WhenOwnerAndCategoryIsAvailable_RemovesCategoryFromWhitelist()
    {
        // Arrange
        _boardAccessServiceMock.Setup(s => s.EnsureAccessAsync(BoardAccess.Owner, BOARD_ID))
            .Returns(Task.CompletedTask);
        var service = GetService();

        // Act
        await service.RemoveCategoryFromBoardAsync(BOARD_ID, CATEGORY_ID);

        // Assert
        AssertEx.AreEqual(BoardTestData.BoardWithCategories, _whitelistBoard);
        AssertEx.AreEqual(CategoryTestData.KanbanCategory, _removedCategory);

        _boardAccessServiceMock.Verify(s => s.EnsureAccessAsync(BoardAccess.Owner, BOARD_ID), Times.Once);
        _boardAccessServiceMock.VerifyNoOtherCalls();

        _boardRepositoryMock.Verify(r => r.GetBoardForAccessCheckAsync(BOARD_ID), Times.Once);
        _boardRepositoryMock.Verify(r => r.RemoveBoardCategoryAsync(_whitelistBoard!, _removedCategory!), Times.Once);
        _boardRepositoryMock.VerifyNoOtherCalls();

        _categoryRepositoryMock.Verify(r => r.GetKanbanCategoriesForOwnerAsync(USER_ID), Times.Once);
        _categoryRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task RemoveCategoryFromBoardAsync_WhenUserIsNotOwner_ThrowsForbiddenException()
    {
        // Arrange
        _boardAccessServiceMock.Setup(s => s.EnsureAccessAsync(BoardAccess.Owner, BOARD_ID))
            .ThrowsAsync(new ForbiddenException($"User {USER_ID} is not the owner of board {BOARD_ID}."));
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.RemoveCategoryFromBoardAsync(BOARD_ID, CATEGORY_ID);

        // Assert
        await AssertEx.Throws<ForbiddenException>(act, $"User {USER_ID} is not the owner of board {BOARD_ID}.");

        _boardAccessServiceMock.Verify(s => s.EnsureAccessAsync(BoardAccess.Owner, BOARD_ID), Times.Once);
        _boardAccessServiceMock.VerifyNoOtherCalls();

        _boardRepositoryMock.Verify(r => r.GetBoardForAccessCheckAsync(It.IsAny<long>()), Times.Never);
        _boardRepositoryMock.Verify(r => r.RemoveBoardCategoryAsync(It.IsAny<Board>(), It.IsAny<Category>()), Times.Never);
        _boardRepositoryMock.VerifyNoOtherCalls();

        _categoryRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task RemoveCategoryFromBoardAsync_WhenCategoryIsNotAvailable_ThrowsAndDoesNotModifyWhitelist()
    {
        // Arrange
        _boardAccessServiceMock.Setup(s => s.EnsureAccessAsync(BoardAccess.Owner, BOARD_ID))
            .Returns(Task.CompletedTask);
        _getKanbanCategoriesForOwnerResponse = [CategoryTestData.SecondKanbanCategory];
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.RemoveCategoryFromBoardAsync(BOARD_ID, CATEGORY_ID);

        // Assert
        await AssertEx.Throws<CategoryNotAvailableForModuleException>(
            act, $"Category with ID {CATEGORY_ID} is not available for the kanban module.");

        _boardAccessServiceMock.Verify(s => s.EnsureAccessAsync(BoardAccess.Owner, BOARD_ID), Times.Once);
        _boardAccessServiceMock.VerifyNoOtherCalls();

        _boardRepositoryMock.Verify(r => r.GetBoardForAccessCheckAsync(BOARD_ID), Times.Once);
        _boardRepositoryMock.Verify(r => r.RemoveBoardCategoryAsync(It.IsAny<Board>(), It.IsAny<Category>()), Times.Never);
        _boardRepositoryMock.VerifyNoOtherCalls();

        _categoryRepositoryMock.Verify(r => r.GetKanbanCategoriesForOwnerAsync(USER_ID), Times.Once);
        _categoryRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task RemoveCategoryFromBoardAsync_WhenBoardDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        _boardAccessServiceMock.Setup(s => s.EnsureAccessAsync(BoardAccess.Owner, BOARD_ID))
            .Returns(Task.CompletedTask);
        _getBoardForAccessCheckResponse = null;
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.RemoveCategoryFromBoardAsync(BOARD_ID, CATEGORY_ID);

        // Assert
        await AssertEx.Throws<NotFoundException>(act, $"Board with ID {BOARD_ID} not found.");

        _boardAccessServiceMock.Verify(s => s.EnsureAccessAsync(BoardAccess.Owner, BOARD_ID), Times.Once);
        _boardAccessServiceMock.VerifyNoOtherCalls();

        _boardRepositoryMock.Verify(r => r.GetBoardForAccessCheckAsync(BOARD_ID), Times.Once);
        _boardRepositoryMock.Verify(r => r.RemoveBoardCategoryAsync(It.IsAny<Board>(), It.IsAny<Category>()), Times.Never);
        _boardRepositoryMock.VerifyNoOtherCalls();

        _categoryRepositoryMock.VerifyNoOtherCalls();
    }
}
