using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Multitool.Api.Controllers;
using Multitool.Application.Interfaces;
using Multitool.Application.Models.CustomTable;
using Multitool.Tests.Shared;

namespace Multitool.Api.Tests;

public class CustomTableControllerTests
{
    private readonly Mock<ICustomTableService> _serviceMock;
    private readonly CustomTableController _sut;

    public CustomTableControllerTests()
    {
        _serviceMock = new Mock<ICustomTableService>();
        _sut = new CustomTableController(_serviceMock.Object);
    }

    [Fact]
    public async Task GetTableList_ReturnsOk_WithList()
    {
        var list = new List<TableOverview> { CustomTableTestData.DefaultTableOverview };
        _serviceMock.Setup(s => s.GetTableListAsync()).ReturnsAsync(list);

        var result = await _sut.GetTableList();

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(list);
    }

    [Fact]
    public async Task GetTable_ReturnsOk_WithDetail()
    {
        var detail = CustomTableTestData.DefaultTableDetail;
        _serviceMock.Setup(s => s.GetTableAsync(detail.TableId)).ReturnsAsync(detail);

        var result = await _sut.GetTable(detail.TableId);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(detail);
    }

    [Fact]
    public async Task CreateTable_ReturnsOk_WithId()
    {
        var dto = CustomTableTestData.DefaultCreateTableDto;
        _serviceMock.Setup(s => s.CreateTableAsync(dto)).ReturnsAsync(1L);

        var result = await _sut.CreateTable(dto);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(1L);
    }

    [Fact]
    public async Task UpdateTable_ReturnsNoContent()
    {
        var dto = CustomTableTestData.DefaultUpdateTableDto;
        
        var result = await _sut.UpdateTable(1, dto);

        result.Should().BeOfType<NoContentResult>();
        _serviceMock.Verify(s => s.UpdateTableAsync(1, dto), Times.Once);
    }

    [Fact]
    public async Task DeleteTable_ReturnsNoContent()
    {
        var result = await _sut.DeleteTable(1);

        result.Should().BeOfType<NoContentResult>();
        _serviceMock.Verify(s => s.DeleteTableAsync(1), Times.Once);
    }

    [Fact]
    public async Task CreateColumn_ReturnsNoContent()
    {
        var result = await _sut.CreateColumn(1);

        result.Should().BeOfType<NoContentResult>();
        _serviceMock.Verify(s => s.CreateColumnAsync(1), Times.Once);
    }

    [Fact]
    public async Task UpdateColumn_ReturnsNoContent()
    {
        var dto = CustomTableTestData.DefaultUpdateColumnDto;

        var result = await _sut.UpdateColumn(1, dto);

        result.Should().BeOfType<NoContentResult>();
        _serviceMock.Verify(s => s.UpdateColumnAsync(1, dto), Times.Once);
    }

    [Fact]
    public async Task UpdateColumnOrder_ReturnsNoContent()
    {
        var list = new List<UpdateColumnOrderDto> { new(1, 0) };

        var result = await _sut.UpdateColumnOrder(list);

        result.Should().BeOfType<NoContentResult>();
        _serviceMock.Verify(s => s.UpdateColumnOrderAsync(list), Times.Once);
    }

    [Fact]
    public async Task DeleteColumn_ReturnsNoContent()
    {
        var result = await _sut.DeleteColumn(1, 2);

        result.Should().BeOfType<NoContentResult>();
        _serviceMock.Verify(s => s.DeleteColumnAsync(1, 2), Times.Once);
    }

    [Fact]
    public async Task CreateRow_ReturnsNoContent()
    {
        var result = await _sut.CreateRow(1);

        result.Should().BeOfType<NoContentResult>();
        _serviceMock.Verify(s => s.CreateRowAsync(1), Times.Once);
    }

    [Fact]
    public async Task UpdateRowOrder_ReturnsNoContent()
    {
        var list = new List<Multitool.Domain.Entities.CustomTable.RowOrderUpdateDto> { new(1, 0) };

        var result = await _sut.UpdateRowOrder(list);

        result.Should().BeOfType<NoContentResult>();
        _serviceMock.Verify(s => s.UpdateRowOrderAsync(list), Times.Once);
    }

    [Fact]
    public async Task DeleteRows_ReturnsNoContent()
    {
        var ids = new List<long> { 1, 2 };

        var result = await _sut.DeleteRows(1, ids);

        result.Should().BeOfType<NoContentResult>();
        _serviceMock.Verify(s => s.DeleteRowsAsync(1, ids), Times.Once);
    }

    [Fact]
    public async Task SetCell_ReturnsNoContent()
    {
        var result = await _sut.SetCell(1, 2, "value");

        result.Should().BeOfType<NoContentResult>();
        _serviceMock.Verify(s => s.UpsertCellAsync(1, 2, "value"), Times.Once);
    }
}
