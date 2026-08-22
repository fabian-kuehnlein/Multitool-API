using Moq;
using Multitool.Api.Controllers;
using Multitool.Application.Interfaces;
using Multitool.Application.Models.CustomTable;
using Multitool.Application.Models.Info;
using Multitool.Tests.Shared;
using Multitool.Tests.Shared.Assertions;

namespace Multitool.Api.Tests.Controllers;

public class CustomTableControllerTests
{
    private readonly Mock<ICustomTableService> _customTableServiceMock;

    private List<TableOverviewDto> _getTableListResponse;
    private TableDetail _getTableResponse;
    private long _createTableResponse;

    private static readonly long TABLE_ID = CustomTableTestData.DefaultTable.TableId;
    private static readonly long COLUMN_ID = CustomTableTestData.DefaultColumn.ColumnId;
    private static readonly long ROW_ID = CustomTableTestData.DefaultRow.RowId;

    public CustomTableControllerTests()
    {
        _customTableServiceMock = new Mock<ICustomTableService>();

        // Default responses
        _getTableListResponse = [CustomTableTestData.DefaultTableOverview];
        _getTableResponse = CustomTableTestData.DefaultTableDetail;
        _createTableResponse = CustomTableTestData.DefaultTable.TableId;
    }

    private CustomTableController GetController()
    {
        _customTableServiceMock.Reset();

        _customTableServiceMock.Setup(s => s.GetTableListAsync())
            .ReturnsAsync(_getTableListResponse);

        _customTableServiceMock.Setup(s => s.GetTableAsync(It.IsAny<long>()))
            .ReturnsAsync(_getTableResponse);

        _customTableServiceMock.Setup(s => s.CreateTableAsync(It.IsAny<CreateTableDto>()))
            .ReturnsAsync(_createTableResponse);

        _customTableServiceMock.Setup(s => s.UpdateTableAsync(It.IsAny<long>(), It.IsAny<UpdateTableDto>()))
            .Returns(Task.CompletedTask);

        _customTableServiceMock.Setup(s => s.DeleteTableAsync(It.IsAny<long>()))
            .Returns(Task.CompletedTask);

        _customTableServiceMock.Setup(s => s.CreateColumnAsync(It.IsAny<long>()))
            .Returns(Task.CompletedTask);

        _customTableServiceMock.Setup(s => s.UpdateColumnAsync(It.IsAny<long>(), It.IsAny<UpdateColumnDto>()))
            .Returns(Task.CompletedTask);

        _customTableServiceMock.Setup(s => s.UpdateColumnOrderAsync(It.IsAny<List<UpdateColumnOrderDto>>()))
            .Returns(Task.CompletedTask);

        _customTableServiceMock.Setup(s => s.DeleteColumnAsync(It.IsAny<long>(), It.IsAny<long>()))
            .Returns(Task.CompletedTask);

        _customTableServiceMock.Setup(s => s.CreateRowAsync(It.IsAny<long>()))
            .Returns(Task.CompletedTask);

        _customTableServiceMock.Setup(s => s.UpdateRowOrderAsync(It.IsAny<List<RowOrderUpdateDto>>()))
            .Returns(Task.CompletedTask);

        _customTableServiceMock.Setup(s => s.DeleteRowsAsync(It.IsAny<long>(), It.IsAny<List<long>>()))
            .Returns(Task.CompletedTask);

        _customTableServiceMock.Setup(s => s.UpsertCellAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<object?>()))
            .Returns(Task.CompletedTask);

        return new CustomTableController(_customTableServiceMock.Object);
    }

    // GET api/CustomTable/tables

    [Fact]
    public async Task GetTableList_WhenTablesExist_ReturnsOkWithList()
    {
        // Arrange
        var controller = GetController();

        // Act
        var result = await controller.GetTableList();

        // Assert
        AssertEx.Ok(result, _getTableListResponse);

        _customTableServiceMock.Verify(s => s.GetTableListAsync(), Times.Once);
        _customTableServiceMock.VerifyNoOtherCalls();
    }

    // GET api/CustomTable/tables/{id}

    [Fact]
    public async Task GetTable_WhenTableExists_ReturnsOkWithDetail()
    {
        // Arrange
        var controller = GetController();

        // Act
        var result = await controller.GetTable(TABLE_ID);

        // Assert
        AssertEx.Ok(result, _getTableResponse);

        _customTableServiceMock.Verify(s => s.GetTableAsync(TABLE_ID), Times.Once);
        _customTableServiceMock.VerifyNoOtherCalls();
    }

    // POST api/CustomTable/tables

    [Fact]
    public async Task CreateTable_WhenDtoIsValid_ReturnsCreatedWithId()
    {
        // Arrange
        var newTable = CustomTableTestData.DefaultCreateTableDto;
        var controller = GetController();

        // Act
        var result = await controller.CreateTable(newTable);

        // Assert
        AssertEx.Created(result, _createTableResponse);

        _customTableServiceMock.Verify(s => s.CreateTableAsync(newTable), Times.Once);
        _customTableServiceMock.VerifyNoOtherCalls();
    }

    // PUT api/CustomTable/tables/{id}

    [Fact]
    public async Task UpdateTable_WhenTableExists_ReturnsNoContent()
    {
        // Arrange
        var updateTable = CustomTableTestData.DefaultUpdateTableDto;
        var controller = GetController();

        // Act
        var result = await controller.UpdateTable(TABLE_ID, updateTable);

        // Assert
        AssertEx.NoContent(result);

        _customTableServiceMock.Verify(s => s.UpdateTableAsync(TABLE_ID, updateTable), Times.Once);
        _customTableServiceMock.VerifyNoOtherCalls();
    }

    // DELETE api/CustomTable/tables/{id}

    [Fact]
    public async Task DeleteTable_WhenTableExists_ReturnsNoContent()
    {
        // Arrange
        var controller = GetController();

        // Act
        var result = await controller.DeleteTable(TABLE_ID);

        // Assert
        AssertEx.NoContent(result);

        _customTableServiceMock.Verify(s => s.DeleteTableAsync(TABLE_ID), Times.Once);
        _customTableServiceMock.VerifyNoOtherCalls();
    }

    // POST api/CustomTable/tables/{tableId}/columns

    [Fact]
    public async Task CreateColumn_WhenTableExists_ReturnsNoContent()
    {
        // Arrange
        var controller = GetController();

        // Act
        var result = await controller.CreateColumn(TABLE_ID);

        // Assert
        AssertEx.NoContent(result);

        _customTableServiceMock.Verify(s => s.CreateColumnAsync(TABLE_ID), Times.Once);
        _customTableServiceMock.VerifyNoOtherCalls();
    }

    // PUT api/CustomTable/columns/{id}

    [Fact]
    public async Task UpdateColumn_WhenColumnExists_ReturnsNoContent()
    {
        // Arrange
        var updateColumn = CustomTableTestData.DefaultUpdateColumnDto;
        var controller = GetController();

        // Act
        var result = await controller.UpdateColumn(COLUMN_ID, updateColumn);

        // Assert
        AssertEx.NoContent(result);

        _customTableServiceMock.Verify(s => s.UpdateColumnAsync(COLUMN_ID, updateColumn), Times.Once);
        _customTableServiceMock.VerifyNoOtherCalls();
    }

    // PUT api/CustomTable/columns/order

    [Fact]
    public async Task UpdateColumnOrder_WhenColumnsAreValid_ReturnsNoContent()
    {
        // Arrange
        var columnOrders = new List<UpdateColumnOrderDto> { new(COLUMN_ID, 0) };
        var controller = GetController();

        // Act
        var result = await controller.UpdateColumnOrder(columnOrders);

        // Assert
        AssertEx.NoContent(result);

        _customTableServiceMock.Verify(s => s.UpdateColumnOrderAsync(columnOrders), Times.Once);
        _customTableServiceMock.VerifyNoOtherCalls();
    }

    // DELETE api/CustomTable/tables/{tableId}/columns/{columnId}

    [Fact]
    public async Task DeleteColumn_WhenColumnExists_ReturnsNoContent()
    {
        // Arrange
        var controller = GetController();

        // Act
        var result = await controller.DeleteColumn(TABLE_ID, COLUMN_ID);

        // Assert
        AssertEx.NoContent(result);

        _customTableServiceMock.Verify(s => s.DeleteColumnAsync(TABLE_ID, COLUMN_ID), Times.Once);
        _customTableServiceMock.VerifyNoOtherCalls();
    }

    // POST api/CustomTable/tables/{tableId}/rows

    [Fact]
    public async Task CreateRow_WhenTableExists_ReturnsNoContent()
    {
        // Arrange
        var controller = GetController();

        // Act
        var result = await controller.CreateRow(TABLE_ID);

        // Assert
        AssertEx.NoContent(result);

        _customTableServiceMock.Verify(s => s.CreateRowAsync(TABLE_ID), Times.Once);
        _customTableServiceMock.VerifyNoOtherCalls();
    }

    // PUT api/CustomTable/rows/order

    [Fact]
    public async Task UpdateRowOrder_WhenRowsAreValid_ReturnsNoContent()
    {
        // Arrange
        var rowOrders = new List<RowOrderUpdateDto> { new(ROW_ID, 0) };
        var controller = GetController();

        // Act
        var result = await controller.UpdateRowOrder(rowOrders);

        // Assert
        AssertEx.NoContent(result);

        _customTableServiceMock.Verify(s => s.UpdateRowOrderAsync(rowOrders), Times.Once);
        _customTableServiceMock.VerifyNoOtherCalls();
    }

    // DELETE api/CustomTable/tables/{tableId}/rows

    [Fact]
    public async Task DeleteRows_WhenRowsExist_ReturnsNoContent()
    {
        // Arrange
        var rowIds = new List<long> { ROW_ID, ROW_ID + 1 };
        var controller = GetController();

        // Act
        var result = await controller.DeleteRows(TABLE_ID, rowIds);

        // Assert
        AssertEx.NoContent(result);

        _customTableServiceMock.Verify(s => s.DeleteRowsAsync(TABLE_ID, rowIds), Times.Once);
        _customTableServiceMock.VerifyNoOtherCalls();
    }

    // PUT api/CustomTable/rows/{rowId}/cells/{columnId}

    [Fact]
    public async Task SetCell_WhenCellIsValid_ReturnsNoContent()
    {
        // Arrange
        const string value = "value";
        var controller = GetController();

        // Act
        var result = await controller.SetCell(ROW_ID, COLUMN_ID, value);

        // Assert
        AssertEx.NoContent(result);

        _customTableServiceMock.Verify(s => s.UpsertCellAsync(ROW_ID, COLUMN_ID, value), Times.Once);
        _customTableServiceMock.VerifyNoOtherCalls();
    }
}
