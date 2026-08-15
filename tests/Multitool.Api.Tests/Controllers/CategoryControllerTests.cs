using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Multitool.Api.Controllers;
using Multitool.Application.Interfaces;
using Multitool.Application.Models;

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
        // Arrange
        var categories = new List<CategoryDto> { new() { Id = 1, Name = "Arbeit", Color = "#FF0000" } };
        _serviceMock.Setup(s => s.GetCategoriesAsync()).ReturnsAsync(categories);

        // Act
        var result = await _sut.GetCategories();

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(categories);
    }
}
