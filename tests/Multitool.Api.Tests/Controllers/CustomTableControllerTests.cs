using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Multitool.Api.Controllers;
using Multitool.Application.Interfaces;
using Multitool.Application.Models.CustomTable;
using Multitool.Tests.Shared;

namespace Multitool.Api.Tests.Controllers;

public class CustomTableControllerTests
{
    private readonly Mock<ICustomTableService> _serviceMock;
    private readonly CustomTableController _sut;

    public CustomTableControllerTests()
    {
        _serviceMock = new Mock<ICustomTableService>();
        _sut = new CustomTableController(_serviceMock.Object);
    }

    // GET api/CustomTable/tables

    [Fact]
    public async Task GetTableList_WhenTablesExist_ReturnsOkWithList()
    {
        // Arrange
        var list = new List<TableOverviewDto> { CustomTableTestData.DefaultTableOverview };
        _serviceMock.Setup(s => s.GetTableListAsync()).ReturnsAsync(list);

        // Act
        var result = await _sut.GetTableList();

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(list);
    }

    // GET api/CustomTable/tables/{id}

    [Fact]
    public async Task GetTable_WhenTableExists_ReturnsOkWithDetail()
    {
        // Arrange
        var detail = CustomTableTestData.DefaultTableDetail;
        _serviceMock.Setup(s => s.GetTableAsync(detail.TableId)).ReturnsAsync(detail);

        // Act
        var result = await _sut.GetTable(detail.TableId);

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(detail);
    }

    // POST api/CustomTable/tables

    [Fact]
    public async Task CreateTable_WhenDtoIsValid_ReturnsOkWithId()
    {
        // Arrange
        var dto = CustomTableTestData.DefaultCreateTableDto;
        _serviceMock.Setup(s => s.CreateTableAsync(dto)).ReturnsAsync(1L);

        // Act
        var result = await _sut.CreateTable(dto);

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(1L);
    }

    // PUT api/CustomTable/tables/{id}

    [Fact]
    public async Task UpdateTable_WhenTableExists_ReturnsNoContent()
    {
        // Arrange
        const long tableId = 1;
        var dto = CustomTableTestData.DefaultUpdateTableDto;

        // Act
        var result = await _sut.UpdateTable(tableId, dto);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _serviceMock.Verify(s => s.UpdateTableAsync(tableId, dto), Times.Once);
    }

    // DELETE api/CustomTable/tables/{id}

    [Fact]
    public async Task DeleteTable_WhenTableExists_ReturnsNoContent()
    {
        // Arrange
        const long tableId = 1;

        // Act
        var result = await _sut.DeleteTable(tableId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _serviceMock.Verify(s => s.DeleteTableAsync(tableId), Times.Once);
    }

    // POST api/CustomTable/tables/{tableId}/columns

    [Fact]
    public async Task CreateColumn_WhenTableExists_ReturnsNoContent()
    {
        // Arrange
        const long tableId = 1;

        // Act
        var result = await _sut.CreateColumn(tableId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _serviceMock.Verify(s => s.CreateColumnAsync(tableId), Times.Once);
    }

    // PUT api/CustomTable/columns/{id}

    [Fact]
    public async Task UpdateColumn_WhenColumnExists_ReturnsNoContent()
    {
        // Arrange
        const long columnId = 1;
        var dto = CustomTableTestData.DefaultUpdateColumnDto;

        // Act
        var result = await _sut.UpdateColumn(columnId, dto);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _serviceMock.Verify(s => s.UpdateColumnAsync(columnId, dto), Times.Once);
    }

    // PUT api/CustomTable/columns/order

    [Fact]
    public async Task UpdateColumnOrder_WhenColumnsAreValid_ReturnsNoContent()
    {
        // Arrange
        var list = new List<UpdateColumnOrderDto> { new(1, 0) };

        // Act
        var result = await _sut.UpdateColumnOrder(list);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _serviceMock.Verify(s => s.UpdateColumnOrderAsync(list), Times.Once);
    }

    // DELETE api/CustomTable/tables/{tableId}/columns/{columnId}

    [Fact]
    public async Task DeleteColumn_WhenColumnExists_ReturnsNoContent()
    {
        // Arrange
        const long tableId = 1;
        const long columnId = 2;

        // Act
        var result = await _sut.DeleteColumn(tableId, columnId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _serviceMock.Verify(s => s.DeleteColumnAsync(tableId, columnId), Times.Once);
    }

    // POST api/CustomTable/tables/{tableId}/rows

    [Fact]
    public async Task CreateRow_WhenTableExists_ReturnsNoContent()
    {
        // Arrange
        const long tableId = 1;

        // Act
        var result = await _sut.CreateRow(tableId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _serviceMock.Verify(s => s.CreateRowAsync(tableId), Times.Once);
    }

    // PUT api/CustomTable/rows/order

    [Fact]
    public async Task UpdateRowOrder_WhenRowsAreValid_ReturnsNoContent()
    {
        // Arrange
        var list = new List<RowOrderUpdateDto> { new(1, 0) };

        // Act
        var result = await _sut.UpdateRowOrder(list);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _serviceMock.Verify(s => s.UpdateRowOrderAsync(list), Times.Once);
    }

    // DELETE api/CustomTable/tables/{tableId}/rows

    [Fact]
    public async Task DeleteRows_WhenRowsExist_ReturnsNoContent()
    {
        // Arrange
        const long tableId = 1;
        var ids = new List<long> { 1, 2 };

        // Act
        var result = await _sut.DeleteRows(tableId, ids);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _serviceMock.Verify(s => s.DeleteRowsAsync(tableId, ids), Times.Once);
    }

    // PUT api/CustomTable/rows/{rowId}/cells/{columnId}

    [Fact]
    public async Task SetCell_WhenCellIsValid_ReturnsNoContent()
    {
        // Arrange
        const long rowId = 1;
        const long columnId = 2;
        const string value = "value";

        // Act
        var result = await _sut.SetCell(rowId, columnId, value);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _serviceMock.Verify(s => s.UpsertCellAsync(rowId, columnId, value), Times.Once);
    }
}
