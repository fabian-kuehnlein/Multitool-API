using Mapster;
using Moq;
using Multitool.Application.Interfaces;
using Multitool.Application.Mappings;
using Multitool.Application.Models.Kanban;
using Multitool.Application.Services;
using Multitool.Domain.Entities.Config;
using Multitool.Domain.Entities.Kanban;
using Multitool.Domain.Enums;
using Multitool.Domain.Exceptions;
using Multitool.Domain.Interfaces;
using Multitool.Tests.Shared;
using Multitool.Tests.Shared.Assertions;

namespace Multitool.Application.Tests;

public class BoardServiceTests
{
    private readonly Mock<IBoardRepository> _boardRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IBoardAccessService> _boardAccessServiceMock;
    private readonly Mock<IUserContext> _userContextMock;

    private List<Board> _getBoardsResponse;
    private Board? _getBoardWithDetailsResponse;
    private Board? _getBoardByIdResponse;
    private Board? _getBoardForAccessCheckResponse;
    private BoardColumn? _getColumnResponse;
    private User? _getUserByIdResponse;
    private User? _getUserByUsernameResponse;
    private long _createBoardResponse;

    private Board? _createdBoard;
    private Board? _updatedBoard;
    private Board? _deletedBoard;
    private BoardColumn? _createdColumn;
    private BoardColumn? _updatedColumn;
    private BoardColumn? _deletedColumn;
    private Board? _memberBoard;
    private User? _addedMember;
    private User? _removedMember;

    private const int USER_ID = 1;
    private const int OTHER_USER_ID = 2;
    private const long BOARD_ID = 10;
    private const long OTHER_BOARD_ID = 11;
    private const long COLUMN_ID = 20;
    private const long OTHER_COLUMN_ID = 21;

    public BoardServiceTests()
    {
        MappingConfigBootstrapper.EnsureApplied();

        _boardRepositoryMock = new Mock<IBoardRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _boardAccessServiceMock = new Mock<IBoardAccessService>();
        _userContextMock = new Mock<IUserContext>();

        _getBoardsResponse = [];
        _getBoardWithDetailsResponse = BoardTestData.DefaultBoardWithDetails;
        _getBoardByIdResponse = BoardTestData.DefaultBoard;
        _getBoardForAccessCheckResponse = BoardTestData.DefaultBoard;
        _getColumnResponse = BoardTestData.DefaultColumn;
        _getUserByIdResponse = BoardTestData.DefaultMember;
        _getUserByUsernameResponse = BoardTestData.DefaultMember;
        _createBoardResponse = BOARD_ID;
    }

    private BoardService GetService()
    {
        _createdBoard = null;
        _updatedBoard = null;
        _deletedBoard = null;
        _createdColumn = null;
        _updatedColumn = null;
        _deletedColumn = null;
        _memberBoard = null;
        _addedMember = null;
        _removedMember = null;

        _boardRepositoryMock.Reset();
        _userRepositoryMock.Reset();
        _userContextMock.Reset();

        _userContextMock.SetupGet(u => u.UserId).Returns(USER_ID);

        _boardRepositoryMock.Setup(r => r.GetBoardOverviewAsync(It.IsAny<int>()))
            .ReturnsAsync(_getBoardsResponse);

        _boardRepositoryMock.Setup(r => r.GetBoardWithDetailsAsync(It.IsAny<long>(), It.IsAny<int>()))
            .ReturnsAsync(_getBoardWithDetailsResponse);

        _boardRepositoryMock.Setup(r => r.GetBoardByIdAsync(It.IsAny<long>()))
            .ReturnsAsync(_getBoardByIdResponse);

        _boardRepositoryMock.Setup(r => r.GetBoardForAccessCheckAsync(It.IsAny<long>()))
            .ReturnsAsync(_getBoardForAccessCheckResponse);

        _boardRepositoryMock.Setup(r => r.GetColumnByIdAsync(It.IsAny<long>()))
            .ReturnsAsync(_getColumnResponse);

        _boardRepositoryMock.Setup(r => r.CreateBoardAsync(It.IsAny<Board>()))
            .Callback<Board>(b => _createdBoard = b)
            .ReturnsAsync(_createBoardResponse);

        _boardRepositoryMock.Setup(r => r.UpdateBoardAsync(It.IsAny<Board>()))
            .Callback<Board>(b => _updatedBoard = b)
            .Returns(Task.CompletedTask);

        _boardRepositoryMock.Setup(r => r.DeleteBoardAsync(It.IsAny<Board>()))
            .Callback<Board>(b => _deletedBoard = b)
            .Returns(Task.CompletedTask);

        _boardRepositoryMock.Setup(r => r.CreateColumnAsync(It.IsAny<BoardColumn>()))
            .Callback<BoardColumn>(c => _createdColumn = c)
            .ReturnsAsync(COLUMN_ID);

        _boardRepositoryMock.Setup(r => r.UpdateColumnAsync(It.IsAny<BoardColumn>()))
            .Callback<BoardColumn>(c => _updatedColumn = c)
            .Returns(Task.CompletedTask);

        _boardRepositoryMock.Setup(r => r.DeleteColumnAsync(It.IsAny<BoardColumn>()))
            .Callback<BoardColumn>(c => _deletedColumn = c)
            .Returns(Task.CompletedTask);

        _boardRepositoryMock.Setup(r => r.UpdateColumnOrderAsync(
                It.IsAny<long>(), It.IsAny<IReadOnlyDictionary<long, int>>()))
            .ReturnsAsync(true);

        _boardRepositoryMock.Setup(r => r.AddBoardMemberAsync(It.IsAny<Board>(), It.IsAny<User>()))
            .Callback<Board, User>((board, user) =>
            {
                _memberBoard = board;
                _addedMember = user;
            })
            .Returns(Task.CompletedTask);

        _boardRepositoryMock.Setup(r => r.RemoveBoardMemberAsync(It.IsAny<Board>(), It.IsAny<User>()))
            .Callback<Board, User>((board, user) =>
            {
                _memberBoard = board;
                _removedMember = user;
            })
            .Returns(Task.CompletedTask);

        _userRepositoryMock.Setup(r => r.GetUserByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(_getUserByIdResponse);

        _userRepositoryMock.Setup(r => r.GetUserByUsernameAsync(It.IsAny<string>()))
            .ReturnsAsync(_getUserByUsernameResponse);

        return new BoardService(
            _boardRepositoryMock.Object,
            _userRepositoryMock.Object,
            _boardAccessServiceMock.Object,
            _userContextMock.Object);
    }

    // GetBoardsAsync

    [Fact]
    public async Task GetBoardsAsync_WhenUserHasBoards_ReturnsBoardOverviews()
    {
        // Arrange
        _getBoardsResponse = [BoardTestData.BoardWithTwoMembers];
        var service = GetService();

        // Act
        var result = await service.GetBoardsAsync();

        // Assert
        AssertEx.AreEqual(_getBoardsResponse.Adapt<List<BoardOverviewModel>>(), result);

        _boardRepositoryMock.Verify(r => r.GetBoardOverviewAsync(USER_ID), Times.Once);
        _boardRepositoryMock.VerifyNoOtherCalls();

        _userRepositoryMock.VerifyNoOtherCalls();
        _boardAccessServiceMock.VerifyNoOtherCalls();

        _userContextMock.VerifyGet(u => u.UserId, Times.Once);
        _userContextMock.VerifyNoOtherCalls();
    }

    // GetBoardByIdAsync

    [Fact]
    public async Task GetBoardByIdAsync_WhenUserIsOwner_ReturnsBoardModelWithIsOwnerTrue()
    {
        // Arrange
        _getBoardWithDetailsResponse = BoardTestData.DefaultBoardWithDetails;
        var service = GetService();

        // Act
        var result = await service.GetBoardByIdAsync(BOARD_ID);

        // Assert
        var expected = _getBoardWithDetailsResponse!.Adapt<BoardModel>() with { IsOwner = true };
        AssertEx.AreEqual(expected, result);

        _boardRepositoryMock.Verify(r => r.GetBoardWithDetailsAsync(BOARD_ID, USER_ID), Times.Once);
        _boardRepositoryMock.VerifyNoOtherCalls();

        _userRepositoryMock.VerifyNoOtherCalls();
        _boardAccessServiceMock.VerifyNoOtherCalls();

        _userContextMock.VerifyGet(u => u.UserId, Times.Once);
        _userContextMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetBoardByIdAsync_WhenUserIsMember_ReturnsBoardModelWithIsOwnerFalse()
    {
        // Arrange
        _getBoardWithDetailsResponse = BoardTestData.OtherOwnerBoardWithDetails;
        var service = GetService();

        // Act
        var result = await service.GetBoardByIdAsync(BOARD_ID);

        // Assert
        var expected = _getBoardWithDetailsResponse!.Adapt<BoardModel>() with { IsOwner = false };
        AssertEx.AreEqual(expected, result);

        _boardRepositoryMock.Verify(r => r.GetBoardWithDetailsAsync(BOARD_ID, USER_ID), Times.Once);
        _boardRepositoryMock.VerifyNoOtherCalls();

        _userRepositoryMock.VerifyNoOtherCalls();
        _boardAccessServiceMock.VerifyNoOtherCalls();

        _userContextMock.VerifyGet(u => u.UserId, Times.Once);
        _userContextMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetBoardByIdAsync_WhenBoardDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        _getBoardWithDetailsResponse = null;
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.GetBoardByIdAsync(BOARD_ID);

        // Assert
        await AssertEx.Throws<NotFoundException>(act, $"Board with ID {BOARD_ID} not found.");

        _boardRepositoryMock.Verify(r => r.GetBoardWithDetailsAsync(BOARD_ID, USER_ID), Times.Once);
        _boardRepositoryMock.VerifyNoOtherCalls();

        _userRepositoryMock.VerifyNoOtherCalls();
        _boardAccessServiceMock.VerifyNoOtherCalls();

        _userContextMock.VerifyGet(u => u.UserId, Times.Once);
        _userContextMock.VerifyNoOtherCalls();
    }

    // CreateBoardAsync

    [Fact]
    public async Task CreateBoardAsync_WhenRequestIsValid_CreatesBoardWithOwnerAsMemberAndDefaultColumns()
    {
        // Arrange
        var request = BoardTestData.DefaultCreateBoardRequest;
        var service = GetService();

        // Act
        var result = await service.CreateBoardAsync(request);

        // Assert
        AssertEx.AreEqual(_createBoardResponse, result);
        Assert.NotNull(_createdBoard);
        AssertEx.AreEqual(new Board
        {
            Name = request.Name,
            Description = request.Description,
            OwnerId = USER_ID,
            CreatedAt = _createdBoard!.CreatedAt,
            Members = [_getUserByIdResponse!],
            Columns = BoardTestData.DefaultColumns
        }, _createdBoard);
        AssertEx.CloseTo(_createdBoard.CreatedAt, DateTime.Now, TimeSpan.FromSeconds(5));

        _userRepositoryMock.Verify(r => r.GetUserByIdAsync(USER_ID), Times.Once);
        _userRepositoryMock.VerifyNoOtherCalls();

        _boardRepositoryMock.Verify(r => r.CreateBoardAsync(_createdBoard), Times.Once);
        _boardRepositoryMock.VerifyNoOtherCalls();

        _boardAccessServiceMock.VerifyNoOtherCalls();

        _userContextMock.VerifyGet(u => u.UserId, Times.Once);
        _userContextMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateBoardAsync_WhenCurrentUserDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        _getUserByIdResponse = null;
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.CreateBoardAsync(BoardTestData.DefaultCreateBoardRequest);

        // Assert
        await AssertEx.Throws<NotFoundException>(act, $"User with ID {USER_ID} not found.");

        _userRepositoryMock.Verify(r => r.GetUserByIdAsync(USER_ID), Times.Once);
        _userRepositoryMock.VerifyNoOtherCalls();

        _boardRepositoryMock.Verify(r => r.CreateBoardAsync(It.IsAny<Board>()), Times.Never);
        _boardRepositoryMock.VerifyNoOtherCalls();

        _boardAccessServiceMock.VerifyNoOtherCalls();

        _userContextMock.VerifyGet(u => u.UserId, Times.Once);
        _userContextMock.VerifyNoOtherCalls();
    }

    // UpdateBoardAsync

    [Fact]
    public async Task UpdateBoardAsync_WhenUserIsOwner_UpdatesNameAndDescription()
    {
        // Arrange
        _boardAccessServiceMock.Setup(s => s.EnsureAccessAsync(BoardAccess.Owner, BOARD_ID))
            .Returns(Task.CompletedTask);
        var service = GetService();
        var request = BoardTestData.DefaultUpdateBoardRequest;

        // Act
        await service.UpdateBoardAsync(BOARD_ID, request);

        // Assert
        Assert.NotNull(_updatedBoard);
        AssertEx.AreEqual("Renamed", _updatedBoard!.Name);
        AssertEx.AreEqual("New description", _updatedBoard.Description);

        _boardAccessServiceMock.Verify(s => s.EnsureAccessAsync(BoardAccess.Owner, BOARD_ID), Times.Once);
        _boardAccessServiceMock.VerifyNoOtherCalls();

        _boardRepositoryMock.Verify(r => r.GetBoardByIdAsync(BOARD_ID), Times.Once);
        _boardRepositoryMock.Verify(r => r.UpdateBoardAsync(_updatedBoard), Times.Once);
        _boardRepositoryMock.VerifyNoOtherCalls();

        _userRepositoryMock.VerifyNoOtherCalls();
        _userContextMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpdateBoardAsync_WhenUserIsNotOwner_ThrowsForbiddenException()
    {
        // Arrange
        _boardAccessServiceMock.Setup(s => s.EnsureAccessAsync(BoardAccess.Owner, BOARD_ID))
            .ThrowsAsync(new ForbiddenException("Forbidden"));
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.UpdateBoardAsync(
            BOARD_ID, BoardTestData.DefaultUpdateBoardRequest with { Description = null });

        // Assert
        await AssertEx.Throws<ForbiddenException>(act);

        _boardAccessServiceMock.Verify(s => s.EnsureAccessAsync(BoardAccess.Owner, BOARD_ID), Times.Once);
        _boardAccessServiceMock.VerifyNoOtherCalls();

        _boardRepositoryMock.Verify(r => r.GetBoardByIdAsync(It.IsAny<long>()), Times.Never);
        _boardRepositoryMock.Verify(r => r.UpdateBoardAsync(It.IsAny<Board>()), Times.Never);
        _boardRepositoryMock.VerifyNoOtherCalls();

        _userRepositoryMock.VerifyNoOtherCalls();
        _userContextMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpdateBoardAsync_WhenBoardDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        _boardAccessServiceMock.Setup(s => s.EnsureAccessAsync(BoardAccess.Owner, BOARD_ID))
            .Returns(Task.CompletedTask);
        _getBoardByIdResponse = null;
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.UpdateBoardAsync(
            BOARD_ID, BoardTestData.DefaultUpdateBoardRequest with { Description = null });

        // Assert
        await AssertEx.Throws<NotFoundException>(act, $"Board with ID {BOARD_ID} not found.");

        _boardAccessServiceMock.Verify(s => s.EnsureAccessAsync(BoardAccess.Owner, BOARD_ID), Times.Once);
        _boardAccessServiceMock.VerifyNoOtherCalls();

        _boardRepositoryMock.Verify(r => r.GetBoardByIdAsync(BOARD_ID), Times.Once);
        _boardRepositoryMock.Verify(r => r.UpdateBoardAsync(It.IsAny<Board>()), Times.Never);
        _boardRepositoryMock.VerifyNoOtherCalls();

        _userRepositoryMock.VerifyNoOtherCalls();
        _userContextMock.VerifyNoOtherCalls();
    }

    // DeleteBoardAsync

    [Fact]
    public async Task DeleteBoardAsync_WhenUserIsOwner_DeletesBoard()
    {
        // Arrange
        _boardAccessServiceMock.Setup(s => s.EnsureAccessAsync(BoardAccess.Owner, BOARD_ID))
            .Returns(Task.CompletedTask);
        var service = GetService();

        // Act
        await service.DeleteBoardAsync(BOARD_ID);

        // Assert
        AssertEx.AreEqual(_getBoardByIdResponse!, _deletedBoard!);

        _boardAccessServiceMock.Verify(s => s.EnsureAccessAsync(BoardAccess.Owner, BOARD_ID), Times.Once);
        _boardAccessServiceMock.VerifyNoOtherCalls();

        _boardRepositoryMock.Verify(r => r.GetBoardByIdAsync(BOARD_ID), Times.Once);
        _boardRepositoryMock.Verify(r => r.DeleteBoardAsync(_deletedBoard!), Times.Once);
        _boardRepositoryMock.VerifyNoOtherCalls();

        _userRepositoryMock.VerifyNoOtherCalls();
        _userContextMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task DeleteBoardAsync_WhenUserIsNotOwner_ThrowsForbiddenException()
    {
        // Arrange
        _boardAccessServiceMock.Setup(s => s.EnsureAccessAsync(BoardAccess.Owner, BOARD_ID))
            .ThrowsAsync(new ForbiddenException("Forbidden"));
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.DeleteBoardAsync(BOARD_ID);

        // Assert
        await AssertEx.Throws<ForbiddenException>(act);

        _boardAccessServiceMock.Verify(s => s.EnsureAccessAsync(BoardAccess.Owner, BOARD_ID), Times.Once);
        _boardAccessServiceMock.VerifyNoOtherCalls();

        _boardRepositoryMock.Verify(r => r.GetBoardByIdAsync(It.IsAny<long>()), Times.Never);
        _boardRepositoryMock.Verify(r => r.DeleteBoardAsync(It.IsAny<Board>()), Times.Never);
        _boardRepositoryMock.VerifyNoOtherCalls();

        _userRepositoryMock.VerifyNoOtherCalls();
        _userContextMock.VerifyNoOtherCalls();
    }

    // CreateColumnAsync

    [Fact]
    public async Task CreateColumnAsync_WhenBoardHasColumns_AppendsColumnAfterLastPosition()
    {
        // Arrange
        var board = BoardTestData.DefaultBoard;
        board.Columns.Add(BoardTestData.FirstColumn);
        board.Columns.Add(BoardTestData.SecondColumn);
        _getBoardWithDetailsResponse = board;
        _boardAccessServiceMock.Setup(s => s.EnsureAccessAsync(BoardAccess.Member, BOARD_ID)).Returns(Task.CompletedTask);
        var service = GetService();

        // Act
        await service.CreateColumnAsync(BOARD_ID, "New column");

        // Assert
        Assert.NotNull(_createdColumn);
        AssertEx.AreEqual(new BoardColumn
        {
            Id = _createdColumn!.Id,
            BoardId = BOARD_ID,
            Name = "New column",
            Position = 5
        }, _createdColumn);

        _boardAccessServiceMock.Verify(s => s.EnsureAccessAsync(BoardAccess.Member, BOARD_ID), Times.Once);
        _boardAccessServiceMock.VerifyNoOtherCalls();

        _boardRepositoryMock.Verify(r => r.GetBoardWithDetailsAsync(BOARD_ID, USER_ID), Times.Once);
        _boardRepositoryMock.Verify(r => r.CreateColumnAsync(_createdColumn), Times.Once);
        _boardRepositoryMock.VerifyNoOtherCalls();

        _userRepositoryMock.VerifyNoOtherCalls();

        _userContextMock.VerifyGet(u => u.UserId, Times.Once);
        _userContextMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateColumnAsync_WhenBoardHasNoColumns_CreatesColumnAtPositionZero()
    {
        // Arrange
        var board = BoardTestData.DefaultBoard;
        _getBoardWithDetailsResponse = board;
        _boardAccessServiceMock.Setup(s => s.EnsureAccessAsync(BoardAccess.Member, BOARD_ID)).Returns(Task.CompletedTask);
        var service = GetService();

        // Act
        await service.CreateColumnAsync(BOARD_ID, "New column");

        // Assert
        Assert.NotNull(_createdColumn);
        AssertEx.AreEqual(0, _createdColumn!.Position);

        _boardAccessServiceMock.Verify(s => s.EnsureAccessAsync(BoardAccess.Member, BOARD_ID), Times.Once);
        _boardAccessServiceMock.VerifyNoOtherCalls();

        _boardRepositoryMock.Verify(r => r.GetBoardWithDetailsAsync(BOARD_ID, USER_ID), Times.Once);
        _boardRepositoryMock.Verify(r => r.CreateColumnAsync(_createdColumn), Times.Once);
        _boardRepositoryMock.VerifyNoOtherCalls();

        _userRepositoryMock.VerifyNoOtherCalls();

        _userContextMock.VerifyGet(u => u.UserId, Times.Once);
        _userContextMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateColumnAsync_WhenBoardDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        _boardAccessServiceMock.Setup(s => s.EnsureAccessAsync(BoardAccess.Member, BOARD_ID))
            .Returns(Task.CompletedTask);
        _getBoardWithDetailsResponse = null;
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.CreateColumnAsync(
            BOARD_ID, "New column");

        // Assert
        await AssertEx.Throws<NotFoundException>(act, $"Board with ID {BOARD_ID} not found.");

        _boardAccessServiceMock.Verify(s => s.EnsureAccessAsync(BoardAccess.Member, BOARD_ID), Times.Once);
        _boardAccessServiceMock.VerifyNoOtherCalls();

        _boardRepositoryMock.Verify(r => r.GetBoardWithDetailsAsync(BOARD_ID, USER_ID), Times.Once);
        _boardRepositoryMock.Verify(r => r.CreateColumnAsync(It.IsAny<BoardColumn>()), Times.Never);
        _boardRepositoryMock.VerifyNoOtherCalls();

        _userRepositoryMock.VerifyNoOtherCalls();

        _userContextMock.VerifyGet(u => u.UserId, Times.Once);
        _userContextMock.VerifyNoOtherCalls();
    }

    // UpdateColumnAsync

    [Fact]
    public async Task UpdateColumnAsync_WhenColumnIsAccessible_UpdatesName()
    {
        // Arrange
        var column = BoardTestData.DefaultColumn;
        _getColumnResponse = column;
        var service = GetService();

        // Act
        await service.UpdateColumnAsync(COLUMN_ID, "Renamed");

        // Assert
        AssertEx.AreEqual(column, _updatedColumn!);
        AssertEx.AreEqual("Renamed", _updatedColumn!.Name);

        _boardAccessServiceMock.Verify(s => s.EnsureAccessAsync(BoardAccess.Column, COLUMN_ID), Times.Once);
        _boardAccessServiceMock.VerifyNoOtherCalls();

        _boardRepositoryMock.Verify(r => r.GetColumnByIdAsync(COLUMN_ID), Times.Once);
        _boardRepositoryMock.Verify(r => r.UpdateColumnAsync(column), Times.Once);
        _boardRepositoryMock.VerifyNoOtherCalls();

        _userRepositoryMock.VerifyNoOtherCalls();
        _userContextMock.VerifyNoOtherCalls();
    }

    // DeleteColumnAsync

    [Fact]
    public async Task DeleteColumnAsync_WhenColumnIsAccessible_DeletesColumn()
    {
        // Arrange
        var column = BoardTestData.DefaultColumn;
        _getColumnResponse = column;
        var service = GetService();

        // Act
        await service.DeleteColumnAsync(COLUMN_ID);

        // Assert
        AssertEx.AreEqual(column, _deletedColumn!);

        _boardAccessServiceMock.Verify(s => s.EnsureAccessAsync(BoardAccess.Column, COLUMN_ID), Times.Once);
        _boardAccessServiceMock.VerifyNoOtherCalls();

        _boardRepositoryMock.Verify(r => r.GetColumnByIdAsync(COLUMN_ID), Times.Once);
        _boardRepositoryMock.Verify(r => r.DeleteColumnAsync(column), Times.Once);
        _boardRepositoryMock.VerifyNoOtherCalls();

        _userRepositoryMock.VerifyNoOtherCalls();
        _userContextMock.VerifyNoOtherCalls();
    }

    // UpdateColumnOrderAsync

    [Fact]
    public async Task UpdateColumnOrderAsync_WhenAllColumnsBelongToBoard_UpdatesPositions()
    {
        // Arrange
        var positions = new Dictionary<long, int>
        {
            [COLUMN_ID] = 2,
            [OTHER_COLUMN_ID] = 0
        };
        _boardAccessServiceMock.Setup(s => s.EnsureAccessAsync(BoardAccess.Member, BOARD_ID)).Returns(Task.CompletedTask);
        var service = GetService();

        // Act
        await service.UpdateColumnOrderAsync(BOARD_ID, positions);

        // Assert
        _boardAccessServiceMock.Verify(s => s.EnsureAccessAsync(BoardAccess.Member, BOARD_ID), Times.Once);
        _boardAccessServiceMock.VerifyNoOtherCalls();

        _boardRepositoryMock.Verify(r => r.UpdateColumnOrderAsync(BOARD_ID, positions), Times.Once);
        _boardRepositoryMock.VerifyNoOtherCalls();

        _userRepositoryMock.VerifyNoOtherCalls();
        _userContextMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpdateColumnOrderAsync_WhenColumnBelongsToAnotherBoard_ThrowsNotFoundException()
    {
        // Arrange
        var positions = new Dictionary<long, int> { [COLUMN_ID] = 0 };
        _boardAccessServiceMock.Setup(s => s.EnsureAccessAsync(BoardAccess.Member, BOARD_ID)).Returns(Task.CompletedTask);
        var service = GetService();
        _boardRepositoryMock.Setup(r => r.UpdateColumnOrderAsync(BOARD_ID, positions)).ReturnsAsync(false);

        // Act
        Func<Task> act = async () => await service.UpdateColumnOrderAsync(BOARD_ID, positions);

        // Assert
        await AssertEx.Throws<NotFoundException>(act,
            $"One or more columns do not belong to board {BOARD_ID}.");

        _boardAccessServiceMock.Verify(s => s.EnsureAccessAsync(BoardAccess.Member, BOARD_ID), Times.Once);
        _boardAccessServiceMock.VerifyNoOtherCalls();

        _boardRepositoryMock.Verify(r => r.UpdateColumnOrderAsync(BOARD_ID, positions), Times.Once);
        _boardRepositoryMock.VerifyNoOtherCalls();

        _userRepositoryMock.VerifyNoOtherCalls();
        _userContextMock.VerifyNoOtherCalls();
    }

    // GetBoardMembersAsync

    [Fact]
    public async Task GetBoardMembersAsync_WhenBoardIsAccessible_ReturnsMembers()
    {
        // Arrange
        var board = BoardTestData.BoardWithTwoMembers;
        _getBoardForAccessCheckResponse = board;
        _boardAccessServiceMock.Setup(s => s.EnsureAccessAsync(BoardAccess.Member, BOARD_ID)).Returns(Task.CompletedTask);
        var service = GetService();

        // Act
        var result = await service.GetBoardMembersAsync(BOARD_ID);

        // Assert
        AssertEx.AreEqual(board.Members.Adapt<List<BoardMemberModel>>(), result);

        _boardAccessServiceMock.Verify(s => s.EnsureAccessAsync(BoardAccess.Member, BOARD_ID), Times.Once);
        _boardAccessServiceMock.VerifyNoOtherCalls();

        _boardRepositoryMock.Verify(r => r.GetBoardForAccessCheckAsync(BOARD_ID), Times.Once);
        _boardRepositoryMock.VerifyNoOtherCalls();
        _userRepositoryMock.VerifyNoOtherCalls();
        _userContextMock.VerifyNoOtherCalls();
    }

    // AddBoardMemberAsync

    [Fact]
    public async Task AddBoardMemberAsync_WhenCallerIsOwnerAndUserIsActive_AddsMember()
    {
        // Arrange
        var board = BoardTestData.DefaultBoard;
        _getBoardForAccessCheckResponse = board;
        _boardAccessServiceMock.Setup(s => s.EnsureAccessAsync(BoardAccess.Owner, BOARD_ID)).Returns(Task.CompletedTask);
        var newMember = BoardTestData.DefaultMember;
        newMember.Id = OTHER_USER_ID;
        newMember.Username = "newmember";
        _getUserByUsernameResponse = newMember;
        var service = GetService();

        // Act
        await service.AddBoardMemberAsync(BOARD_ID, "newmember");

        // Assert
        AssertEx.AreEqual(board, _memberBoard!);
        AssertEx.AreEqual(_getUserByUsernameResponse!, _addedMember!);

        _boardAccessServiceMock.Verify(s => s.EnsureAccessAsync(BoardAccess.Owner, BOARD_ID), Times.Once);
        _boardAccessServiceMock.VerifyNoOtherCalls();

        _userRepositoryMock.Verify(r => r.GetUserByUsernameAsync("newmember"), Times.Once);
        _userRepositoryMock.VerifyNoOtherCalls();

        _boardRepositoryMock.Verify(r => r.GetBoardForAccessCheckAsync(BOARD_ID), Times.Once);
        _boardRepositoryMock.Verify(r => r.AddBoardMemberAsync(board, _getUserByUsernameResponse!), Times.Once);
        _boardRepositoryMock.VerifyNoOtherCalls();

        _userContextMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task AddBoardMemberAsync_WhenCallerIsNotOwner_ThrowsForbiddenException()
    {
        // Arrange
        _boardAccessServiceMock.Setup(s => s.EnsureAccessAsync(BoardAccess.Owner, BOARD_ID))
            .ThrowsAsync(new ForbiddenException("Forbidden"));
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.AddBoardMemberAsync(
            BOARD_ID, "newmember");

        // Assert
        await AssertEx.Throws<ForbiddenException>(act);

        _boardAccessServiceMock.Verify(s => s.EnsureAccessAsync(BoardAccess.Owner, BOARD_ID), Times.Once);
        _boardAccessServiceMock.VerifyNoOtherCalls();

        _userRepositoryMock.Verify(r => r.GetUserByUsernameAsync(It.IsAny<string>()), Times.Never);
        _userRepositoryMock.VerifyNoOtherCalls();

        _boardRepositoryMock.Verify(r => r.AddBoardMemberAsync(It.IsAny<Board>(), It.IsAny<User>()), Times.Never);
        _boardRepositoryMock.VerifyNoOtherCalls();

        _userContextMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task AddBoardMemberAsync_WhenUserDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        _boardAccessServiceMock.Setup(s => s.EnsureAccessAsync(BoardAccess.Owner, BOARD_ID)).Returns(Task.CompletedTask);
        _getUserByUsernameResponse = null;
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.AddBoardMemberAsync(
            BOARD_ID, "unknown");

        // Assert
        await AssertEx.Throws<NotFoundException>(act, "User with username unknown not found.");

        _boardAccessServiceMock.Verify(s => s.EnsureAccessAsync(BoardAccess.Owner, BOARD_ID), Times.Once);
        _boardAccessServiceMock.VerifyNoOtherCalls();

        _userRepositoryMock.Verify(r => r.GetUserByUsernameAsync("unknown"), Times.Once);
        _userRepositoryMock.VerifyNoOtherCalls();

        _boardRepositoryMock.Verify(r => r.GetBoardForAccessCheckAsync(BOARD_ID), Times.Once);
        _boardRepositoryMock.Verify(r => r.AddBoardMemberAsync(It.IsAny<Board>(), It.IsAny<User>()), Times.Never);
        _boardRepositoryMock.VerifyNoOtherCalls();

        _userContextMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task AddBoardMemberAsync_WhenUserIsInactive_ThrowsInvalidOperationException()
    {
        // Arrange
        _boardAccessServiceMock.Setup(s => s.EnsureAccessAsync(BoardAccess.Owner, BOARD_ID)).Returns(Task.CompletedTask);
        var inactiveUser = BoardTestData.DefaultMember;
        inactiveUser.Id = OTHER_USER_ID;
        inactiveUser.Username = "inactive";
        inactiveUser.IsActive = false;
        _getUserByUsernameResponse = inactiveUser;
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.AddBoardMemberAsync(
            BOARD_ID, "inactive");

        // Assert
        await AssertEx.Throws<InvalidOperationException>(act, "User with username inactive is not active.");

        _boardAccessServiceMock.Verify(s => s.EnsureAccessAsync(BoardAccess.Owner, BOARD_ID), Times.Once);
        _boardAccessServiceMock.VerifyNoOtherCalls();

        _userRepositoryMock.Verify(r => r.GetUserByUsernameAsync("inactive"), Times.Once);
        _userRepositoryMock.VerifyNoOtherCalls();

        _boardRepositoryMock.Verify(r => r.GetBoardForAccessCheckAsync(BOARD_ID), Times.Once);
        _boardRepositoryMock.Verify(r => r.AddBoardMemberAsync(It.IsAny<Board>(), It.IsAny<User>()), Times.Never);
        _boardRepositoryMock.VerifyNoOtherCalls();

        _userContextMock.VerifyNoOtherCalls();
    }

    // RemoveBoardMemberAsync

    [Fact]
    public async Task RemoveBoardMemberAsync_WhenCallerIsOwner_RemovesMember()
    {
        // Arrange
        var board = BoardTestData.DefaultBoard;
        _getBoardForAccessCheckResponse = board;
        _boardAccessServiceMock.Setup(s => s.EnsureAccessAsync(BoardAccess.Owner, BOARD_ID)).Returns(Task.CompletedTask);
        var member = BoardTestData.DefaultMember;
        member.Id = OTHER_USER_ID;
        member.Username = "member";
        _getUserByIdResponse = member;
        var service = GetService();

        // Act
        await service.RemoveBoardMemberAsync(BOARD_ID, OTHER_USER_ID);

        // Assert
        AssertEx.AreEqual(board, _memberBoard!);
        AssertEx.AreEqual(_getUserByIdResponse!, _removedMember!);

        _boardAccessServiceMock.Verify(s => s.EnsureAccessAsync(BoardAccess.Owner, BOARD_ID), Times.Once);
        _boardAccessServiceMock.VerifyNoOtherCalls();

        _userRepositoryMock.Verify(r => r.GetUserByIdAsync(OTHER_USER_ID), Times.Once);
        _userRepositoryMock.VerifyNoOtherCalls();

        _boardRepositoryMock.Verify(r => r.GetBoardForAccessCheckAsync(BOARD_ID), Times.Once);
        _boardRepositoryMock.Verify(r => r.RemoveBoardMemberAsync(board, _getUserByIdResponse!), Times.Once);
        _boardRepositoryMock.VerifyNoOtherCalls();

        _userContextMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task RemoveBoardMemberAsync_WhenCallerIsNotOwner_ThrowsForbiddenException()
    {
        // Arrange
        _boardAccessServiceMock.Setup(s => s.EnsureAccessAsync(BoardAccess.Owner, BOARD_ID))
            .ThrowsAsync(new ForbiddenException("Forbidden"));
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.RemoveBoardMemberAsync(BOARD_ID, OTHER_USER_ID);

        // Assert
        await AssertEx.Throws<ForbiddenException>(act);

        _boardAccessServiceMock.Verify(s => s.EnsureAccessAsync(BoardAccess.Owner, BOARD_ID), Times.Once);
        _boardAccessServiceMock.VerifyNoOtherCalls();

        _userRepositoryMock.Verify(r => r.GetUserByIdAsync(It.IsAny<int>()), Times.Never);
        _userRepositoryMock.VerifyNoOtherCalls();

        _boardRepositoryMock.Verify(r => r.RemoveBoardMemberAsync(It.IsAny<Board>(), It.IsAny<User>()), Times.Never);
        _boardRepositoryMock.VerifyNoOtherCalls();

        _userContextMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task RemoveBoardMemberAsync_WhenUserDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        _boardAccessServiceMock.Setup(s => s.EnsureAccessAsync(BoardAccess.Owner, BOARD_ID)).Returns(Task.CompletedTask);
        _getUserByIdResponse = null;
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.RemoveBoardMemberAsync(BOARD_ID, OTHER_USER_ID);

        // Assert
        await AssertEx.Throws<NotFoundException>(act, $"User with ID {OTHER_USER_ID} not found.");

        _boardAccessServiceMock.Verify(s => s.EnsureAccessAsync(BoardAccess.Owner, BOARD_ID), Times.Once);
        _boardAccessServiceMock.VerifyNoOtherCalls();

        _userRepositoryMock.Verify(r => r.GetUserByIdAsync(OTHER_USER_ID), Times.Once);
        _userRepositoryMock.VerifyNoOtherCalls();

        _boardRepositoryMock.Verify(r => r.GetBoardForAccessCheckAsync(BOARD_ID), Times.Once);
        _boardRepositoryMock.Verify(r => r.RemoveBoardMemberAsync(It.IsAny<Board>(), It.IsAny<User>()), Times.Never);
        _boardRepositoryMock.VerifyNoOtherCalls();

        _userContextMock.VerifyNoOtherCalls();
    }
}
