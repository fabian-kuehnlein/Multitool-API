using Moq;
using Multitool.Application.Services;
using Multitool.Domain.Entities.Kanban;
using Multitool.Domain.Enums;
using Multitool.Domain.Exceptions;
using Multitool.Domain.Interfaces;
using Multitool.Tests.Shared;
using Multitool.Tests.Shared.Assertions;

namespace Multitool.Application.Tests;

public class BoardAccessServiceTests
{
    private readonly Mock<IBoardRepository> _boardRepositoryMock;
    private readonly Mock<IUserContext> _userContextMock;

    private Board? _getBoardResponse;
    private BoardColumn? _getColumnResponse;

    private const int USER_ID = 1;
    private const long BOARD_ID = 10;
    private const long COLUMN_ID = 20;

    public BoardAccessServiceTests()
    {
        _boardRepositoryMock = new Mock<IBoardRepository>();
        _userContextMock = new Mock<IUserContext>();

        _getBoardResponse = BoardTestData.DefaultBoard;
        _getColumnResponse = BoardTestData.DefaultColumnWithBoard;
        _getColumnResponse.Board = _getBoardResponse;
    }

    private BoardAccessService GetService()
    {
        _boardRepositoryMock.Reset();
        _userContextMock.Reset();

        _userContextMock.SetupGet(u => u.UserId).Returns(USER_ID);

        _boardRepositoryMock.Setup(r => r.GetBoardForAccessCheckAsync(It.IsAny<long>()))
            .ReturnsAsync(_getBoardResponse);

        _boardRepositoryMock.Setup(r => r.GetColumnByIdAsync(It.IsAny<long>()))
            .ReturnsAsync(_getColumnResponse);

        return new BoardAccessService(_boardRepositoryMock.Object, _userContextMock.Object);
    }

    // EnsureAccessAsync

    [Fact]
    public async Task EnsureAccessAsync_WithBoardMemberPolicyAndUserIsMember_DoesNotThrow()
    {
        // Arrange
        var service = GetService();

        // Act
        await service.EnsureAccessAsync(BoardAccess.Member, BOARD_ID);

        // Assert
        _boardRepositoryMock.Verify(r => r.GetBoardForAccessCheckAsync(BOARD_ID), Times.Once);
        _boardRepositoryMock.VerifyNoOtherCalls();

        _userContextMock.VerifyGet(u => u.UserId, Times.Once);
        _userContextMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task EnsureAccessAsync_WithBoardMemberPolicyAndBoardDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        _getBoardResponse = null;
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.EnsureAccessAsync(BoardAccess.Member, BOARD_ID);

        // Assert
        await AssertEx.Throws<NotFoundException>(act, $"Board with ID {BOARD_ID} not found.");

        _boardRepositoryMock.Verify(r => r.GetBoardForAccessCheckAsync(BOARD_ID), Times.Once);
        _boardRepositoryMock.VerifyNoOtherCalls();

        _userContextMock.VerifyGet(u => u.UserId, Times.Once);
        _userContextMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task EnsureAccessAsync_WithBoardMemberPolicyAndUserIsNotMember_ThrowsNotFoundException()
    {
        // Arrange
        _getBoardResponse = BoardTestData.BoardWithOtherUserOnly;
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.EnsureAccessAsync(BoardAccess.Member, BOARD_ID);

        // Assert
        await AssertEx.Throws<NotFoundException>(act, $"Board with ID {BOARD_ID} not found.");

        _boardRepositoryMock.Verify(r => r.GetBoardForAccessCheckAsync(BOARD_ID), Times.Once);
        _boardRepositoryMock.VerifyNoOtherCalls();

        _userContextMock.VerifyGet(u => u.UserId, Times.Once);
        _userContextMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task EnsureAccessAsync_WithBoardOwnerPolicyAndUserIsOwner_DoesNotThrow()
    {
        // Arrange
        var service = GetService();

        // Act
        await service.EnsureAccessAsync(BoardAccess.Owner, BOARD_ID);

        // Assert
        _boardRepositoryMock.Verify(r => r.GetBoardForAccessCheckAsync(BOARD_ID), Times.Once);
        _boardRepositoryMock.VerifyNoOtherCalls();

        _userContextMock.VerifyGet(u => u.UserId, Times.Once);
        _userContextMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task EnsureAccessAsync_WithBoardOwnerPolicyAndUserIsMemberButNotOwner_ThrowsForbiddenException()
    {
        // Arrange
        _getBoardResponse = BoardTestData.OtherOwnerBoard;
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.EnsureAccessAsync(BoardAccess.Owner, BOARD_ID);

        // Assert
        await AssertEx.Throws<ForbiddenException>(
            act, $"User {USER_ID} is not the owner of board {BOARD_ID}.");

        _boardRepositoryMock.Verify(r => r.GetBoardForAccessCheckAsync(BOARD_ID), Times.Once);
        _boardRepositoryMock.VerifyNoOtherCalls();

        _userContextMock.VerifyGet(u => u.UserId, Times.Once);
        _userContextMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task EnsureAccessAsync_WithBoardOwnerPolicyAndUserIsNotMember_ThrowsNotFoundException()
    {
        // Arrange
        _getBoardResponse = BoardTestData.BoardWithOtherUserOnly;
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.EnsureAccessAsync(BoardAccess.Owner, BOARD_ID);

        // Assert
        await AssertEx.Throws<NotFoundException>(act, $"Board with ID {BOARD_ID} not found.");

        _boardRepositoryMock.Verify(r => r.GetBoardForAccessCheckAsync(BOARD_ID), Times.Once);
        _boardRepositoryMock.VerifyNoOtherCalls();

        _userContextMock.VerifyGet(u => u.UserId, Times.Once);
        _userContextMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task EnsureAccessAsync_WithColumnBoardMemberPolicyAndUserIsBoardMember_DoesNotThrow()
    {
        // Arrange
        var service = GetService();

        // Act
        await service.EnsureAccessAsync(BoardAccess.Column, COLUMN_ID);

        // Assert
        _boardRepositoryMock.Verify(r => r.GetColumnByIdAsync(COLUMN_ID), Times.Once);
        _boardRepositoryMock.VerifyNoOtherCalls();

        _userContextMock.VerifyGet(u => u.UserId, Times.Once);
        _userContextMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task EnsureAccessAsync_WithColumnBoardMemberPolicyAndUserIsNotBoardMember_ThrowsNotFoundException()
    {
        // Arrange
        var board = BoardTestData.BoardWithOtherUserOnly;
        _getColumnResponse = BoardTestData.DefaultColumnWithBoard;
        _getColumnResponse.Board = board;
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.EnsureAccessAsync(BoardAccess.Column, COLUMN_ID);

        // Assert
        await AssertEx.Throws<NotFoundException>(act, $"Board with ID {BOARD_ID} not found.");

        _boardRepositoryMock.Verify(r => r.GetColumnByIdAsync(COLUMN_ID), Times.Once);
        _boardRepositoryMock.VerifyNoOtherCalls();

        _userContextMock.VerifyGet(u => u.UserId, Times.Once);
        _userContextMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task EnsureAccessAsync_WithColumnBoardMemberPolicyAndColumnDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        _getColumnResponse = null;
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.EnsureAccessAsync(BoardAccess.Column, COLUMN_ID);

        // Assert
        await AssertEx.Throws<NotFoundException>(act, $"Column with ID {COLUMN_ID} not found.");

        _boardRepositoryMock.Verify(r => r.GetColumnByIdAsync(COLUMN_ID), Times.Once);
        _boardRepositoryMock.VerifyNoOtherCalls();

        _userContextMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task EnsureAccessAsync_WithUnknownPolicy_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var service = GetService();
        const BoardAccess unknownPolicy = (BoardAccess)999;

        // Act
        Func<Task> act = async () => await service.EnsureAccessAsync(unknownPolicy, BOARD_ID);

        // Assert
        await AssertEx.Throws<ArgumentOutOfRangeException>(act, "access");

        _boardRepositoryMock.VerifyNoOtherCalls();
        _userContextMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task EnsureAccessAsync_WithTrackedBoardAndBoardMemberPolicy_DoesNotThrow()
    {
        // Arrange
        var board = BoardTestData.DefaultBoard;
        var service = GetService();

        // Act
        await service.EnsureAccessAsync(BoardAccess.Member, board);

        // Assert
        _boardRepositoryMock.VerifyNoOtherCalls();

        _userContextMock.VerifyGet(u => u.UserId, Times.Once);
        _userContextMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task EnsureAccessAsync_WithTrackedBoardAndBoardOwnerPolicy_DoesNotThrow()
    {
        // Arrange
        var board = BoardTestData.DefaultBoard;
        var service = GetService();

        // Act
        await service.EnsureAccessAsync(BoardAccess.Owner, board);

        // Assert
        _boardRepositoryMock.VerifyNoOtherCalls();

        _userContextMock.VerifyGet(u => u.UserId, Times.Once);
        _userContextMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task EnsureAccessAsync_WithTrackedBoardAndUserIsNotMember_ThrowsNotFoundException()
    {
        // Arrange
        var board = BoardTestData.BoardWithOtherUserOnly;
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.EnsureAccessAsync(BoardAccess.Member, board);

        // Assert
        await AssertEx.Throws<NotFoundException>(act, $"Board with ID {BOARD_ID} not found.");

        _boardRepositoryMock.VerifyNoOtherCalls();

        _userContextMock.VerifyGet(u => u.UserId, Times.Once);
        _userContextMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task EnsureAccessAsync_WithTrackedBoardAndColumnBoardMemberPolicy_ThrowsArgumentException()
    {
        // Arrange
        var board = BoardTestData.DefaultBoard;
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.EnsureAccessAsync(BoardAccess.Column, board);

        // Assert
        await AssertEx.Throws<ArgumentException>(act, "requires a column context");

        _boardRepositoryMock.VerifyNoOtherCalls();
        _userContextMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task EnsureAccessAsync_WithTrackedColumnAndColumnBoardMemberPolicy_DoesNotThrow()
    {
        // Arrange
        var column = BoardTestData.DefaultColumnWithBoard;
        var service = GetService();

        // Act
        await service.EnsureAccessAsync(BoardAccess.Column, column);

        // Assert
        _boardRepositoryMock.VerifyNoOtherCalls();

        _userContextMock.VerifyGet(u => u.UserId, Times.Once);
        _userContextMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task EnsureAccessAsync_WithTrackedColumnAndBoardMemberPolicy_ThrowsArgumentException()
    {
        // Arrange
        var column = BoardTestData.DefaultColumnWithBoard;
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.EnsureAccessAsync(BoardAccess.Member, column);

        // Assert
        await AssertEx.Throws<ArgumentException>(act, "requires a board context");

        _boardRepositoryMock.VerifyNoOtherCalls();
        _userContextMock.VerifyNoOtherCalls();
    }
}
