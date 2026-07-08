using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Multitool.Api.Controllers;
using Multitool.Application.Interfaces;
using Multitool.Domain.Entities.Category;
using Multitool.Tests.Shared;

namespace Multitool.Api.Tests.Controllers;

public class CategoryControllerTests
{
    private readonly Mock<ICategoryService> _serviceMock;
    private readonly CategoryController _sut;

    public CategoryControllerTests()
    {
        _serviceMock = new Mock<ICategoryService>();
        _sut = new CategoryController(_serviceMock.Object);
    }

    // GET api/Category/categories

    [Fact]
    public async Task GetCategories_WhenCategoriesExist_ReturnsOkWithCategories()
    {
        var categories = new List<Category> { CalendarTestData.DefaultCategory };
        _serviceMock.Setup(s => s.GetCategoriesAsync()).ReturnsAsync(categories);

        var result = await _sut.GetCategories();

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(categories);
    }
}
