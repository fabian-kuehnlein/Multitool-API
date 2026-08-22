using Moq;
using Multitool.Api.Controllers;
using Multitool.Application.Interfaces;
using Multitool.Application.Models.Category;
using Multitool.Tests.Shared;
using Multitool.Tests.Shared.Assertions;

namespace Multitool.Api.Tests.Controllers;

public class CategoryControllerTests
{
    private readonly Mock<ICategoryService> _categoryServiceMock;

    private List<CategoryDto> _getCategoriesResponse;
    private int _createCategoryResponse;

    private static readonly int ID = CategoryTestData.DefaultCategory.Id;

    public CategoryControllerTests()
    {
        _categoryServiceMock = new Mock<ICategoryService>();

        // Default responses
        _getCategoriesResponse = [CategoryTestData.DefaultCategoryDto];
        _createCategoryResponse = CategoryTestData.DefaultCategory.Id;
    }

    private CategoryController GetController()
    {
        _categoryServiceMock.Reset();

        _categoryServiceMock.Setup(s => s.GetCategoriesAsync())
            .ReturnsAsync(_getCategoriesResponse);

        _categoryServiceMock.Setup(s => s.CreateCategoryAsync(It.IsAny<CreateCategoryDto>()))
            .ReturnsAsync(_createCategoryResponse);

        _categoryServiceMock.Setup(s => s.UpdateCategoryAsync(It.IsAny<int>(), It.IsAny<UpdateCategoryDto>()))
            .Returns(Task.CompletedTask);

        _categoryServiceMock.Setup(s => s.DeleteCategoryAsync(It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        return new CategoryController(_categoryServiceMock.Object);
    }

    // GET api/Category/categories

    [Fact]
    public async Task GetCategories_WhenCategoriesExist_ReturnsOkWithCategories()
    {
        // Arrange
        var controller = GetController();

        // Act
        var result = await controller.GetCategories();

        // Assert
        AssertEx.Ok(result, _getCategoriesResponse);

        _categoryServiceMock.Verify(s => s.GetCategoriesAsync(), Times.Once);
        _categoryServiceMock.VerifyNoOtherCalls();
    }

    // POST api/Category/categories

    [Fact]
    public async Task CreateCategory_WhenDtoIsValid_ReturnsCreatedWithId()
    {
        // Arrange
        var dto = CategoryTestData.DefaultCreateCategoryDto;
        var controller = GetController();

        // Act
        var result = await controller.CreateCategory(dto);

        // Assert
        AssertEx.Created(result, _createCategoryResponse);

        _categoryServiceMock.Verify(s => s.CreateCategoryAsync(dto), Times.Once);
        _categoryServiceMock.VerifyNoOtherCalls();
    }

    // PUT api/Category/categories/{id}

    [Fact]
    public async Task UpdateCategory_WhenCategoryExists_ReturnsNoContent()
    {
        // Arrange
        var dto = CategoryTestData.DefaultUpdateCategoryDto;
        var controller = GetController();

        // Act
        var result = await controller.UpdateCategory(ID, dto);

        // Assert
        AssertEx.NoContent(result);

        _categoryServiceMock.Verify(s => s.UpdateCategoryAsync(ID, dto), Times.Once);
        _categoryServiceMock.VerifyNoOtherCalls();
    }

    // DELETE api/Category/categories/{id}

    [Fact]
    public async Task DeleteCategory_WhenCategoryExists_ReturnsNoContent()
    {
        // Arrange
        var controller = GetController();

        // Act
        var result = await controller.DeleteCategory(ID);

        // Assert
        AssertEx.NoContent(result);

        _categoryServiceMock.Verify(s => s.DeleteCategoryAsync(ID), Times.Once);
        _categoryServiceMock.VerifyNoOtherCalls();
    }
}
