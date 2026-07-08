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
        var list = new List<TableOverviewDto> { CustomTableTestData.DefaultTableOverview };
        _serviceMock.Setup(s => s.GetTableListAsync()).ReturnsAsync(list);

        var result = await _sut.GetTableList();

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(list);
    }

    // GET api/CustomTable/tables/{id}

    [Fact]
    public async Task GetTable_WhenTableExists_ReturnsOkWithDetail()
    {
        var detail = CustomTableTestData.DefaultTableDetail;
        _serviceMock.Setup(s => s.GetTableAsync(detail.TableId)).ReturnsAsync(detail);

        var result = await _sut.GetTable(detail.TableId);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(detail);
    }

    // POST api/CustomTable/tables

    [Fact]
    public async Task CreateTable_WhenDtoIsValid_ReturnsOkWithId()
    {
        var dto = CustomTableTestData.DefaultCreateTableDto;
        _serviceMock.Setup(s => s.CreateTableAsync(dto)).ReturnsAsync(1L);

        var result = await _sut.CreateTable(dto);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(1L);
    }

    // PUT api/CustomTable/tables/{id}

    [Fact]
    public async Task UpdateTable_WhenTableExists_ReturnsNoContent()
    {
        var dto = CustomTableTestData.DefaultUpdateTableDto;
        
        var result = await _sut.UpdateTable(1, dto);

        result.Should().BeOfType<NoContentResult>();
        _serviceMock.Verify(s => s.UpdateTableAsync(1, dto), Times.Once);
    }

    // DELETE api/CustomTable/tables/{id}

    [Fact]
    public async Task DeleteTable_WhenTableExists_ReturnsNoContent()
    {
        var result = await _sut.DeleteTable(1);

        result.Should().BeOfType<NoContentResult>();
        _serviceMock.Verify(s => s.DeleteTableAsync(1), Times.Once);
    }

    // POST api/CustomTable/tables/{tableId}/columns

    [Fact]
    public async Task CreateColumn_WhenTableExists_ReturnsNoContent()
    {
        var result = await _sut.CreateColumn(1);

        result.Should().BeOfType<NoContentResult>();
        _serviceMock.Verify(s => s.CreateColumnAsync(1), Times.Once);
    }

    // PUT api/CustomTable/columns/{id}

    [Fact]
    public async Task UpdateColumn_WhenColumnExists_ReturnsNoContent()
    {
        var dto = CustomTableTestData.DefaultUpdateColumnDto;

        var result = await _sut.UpdateColumn(1, dto);

        result.Should().BeOfType<NoContentResult>();
        _serviceMock.Verify(s => s.UpdateColumnAsync(1, dto), Times.Once);
    }

    // PUT api/CustomTable/columns/order

    [Fact]
    public async Task UpdateColumnOrder_WhenColumnsAreValid_ReturnsNoContent()
    {
        var list = new List<UpdateColumnOrderDto> { new(1, 0) };

        var result = await _sut.UpdateColumnOrder(list);

        result.Should().BeOfType<NoContentResult>();
        _serviceMock.Verify(s => s.UpdateColumnOrderAsync(list), Times.Once);
    }

    // DELETE api/CustomTable/tables/{tableId}/columns/{columnId}

    [Fact]
    public async Task DeleteColumn_WhenColumnExists_ReturnsNoContent()
    {
        var result = await _sut.DeleteColumn(1, 2);

        result.Should().BeOfType<NoContentResult>();
        _serviceMock.Verify(s => s.DeleteColumnAsync(1, 2), Times.Once);
    }

    // POST api/CustomTable/tables/{tableId}/rows

    [Fact]
    public async Task CreateRow_WhenTableExists_ReturnsNoContent()
    {
        var result = await _sut.CreateRow(1);

        result.Should().BeOfType<NoContentResult>();
        _serviceMock.Verify(s => s.CreateRowAsync(1), Times.Once);
    }

    // PUT api/CustomTable/rows/order

    [Fact]
    public async Task UpdateRowOrder_WhenRowsAreValid_ReturnsNoContent()
    {
        var list = new List<RowOrderUpdateDto> { new(1, 0) };

        var result = await _sut.UpdateRowOrder(list);

        result.Should().BeOfType<NoContentResult>();
        _serviceMock.Verify(s => s.UpdateRowOrderAsync(list), Times.Once);
    }

    // DELETE api/CustomTable/tables/{tableId}/rows

    [Fact]
    public async Task DeleteRows_WhenRowsExist_ReturnsNoContent()
    {
        var ids = new List<long> { 1, 2 };

        var result = await _sut.DeleteRows(1, ids);

        result.Should().BeOfType<NoContentResult>();
        _serviceMock.Verify(s => s.DeleteRowsAsync(1, ids), Times.Once);
    }

    // PUT api/CustomTable/rows/{rowId}/cells/{columnId}

    [Fact]
    public async Task SetCell_WhenCellIsValid_ReturnsNoContent()
    {
        var result = await _sut.SetCell(1, 2, "value");

        result.Should().BeOfType<NoContentResult>();
        _serviceMock.Verify(s => s.UpsertCellAsync(1, 2, "value"), Times.Once);
    }
}
