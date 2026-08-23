using Mapster;
using Moq;
using Multitool.Application.Mappings;
using Multitool.Application.Models.Category;
using Multitool.Application.Services;
using Multitool.Domain.Entities.Category;
using Multitool.Domain.Exceptions;
using Multitool.Domain.Interfaces;
using Multitool.Tests.Shared;
using Multitool.Tests.Shared.Assertions;

namespace Multitool.Application.Tests;

public class CategoryServiceTests
{
    private readonly Mock<ICategoryRepository> _repositoryMock;

    private List<Category> _getCategoriesResponse;
    private Category? _getByIdResponse;
    private int _createCategoryResponse;

    private static readonly int ID = CategoryTestData.DefaultCategory.Id;

    private Category? _createdCategory;
    private Category? _updatedCategory;

    public CategoryServiceTests()
    {
        TypeAdapterConfig.GlobalSettings.Apply(new MappingConfig());

        _repositoryMock = new Mock<ICategoryRepository>();

        _getCategoriesResponse = new List<Category>();
        _getByIdResponse = CategoryTestData.DefaultCategory;
        _createCategoryResponse = ID;
    }

    private CategoryService GetService()
    {
        _createdCategory = null;
        _updatedCategory = null;

        _repositoryMock.Reset();

        _repositoryMock.Setup(r => r.GetCategoriesAsync())
            .ReturnsAsync(_getCategoriesResponse);

        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(_getByIdResponse);

        _repositoryMock.Setup(r => r.CreateCategoryAsync(It.IsAny<Category>()))
            .Callback<Category>(c => _createdCategory = c)
            .ReturnsAsync(_createCategoryResponse);

        _repositoryMock.Setup(r => r.UpdateCategoryAsync(It.IsAny<Category>()))
            .Callback<Category>(c => _updatedCategory = c)
            .Returns(Task.CompletedTask);

        return new CategoryService(_repositoryMock.Object);
    }

    // GetCategoriesAsync
    [Fact]
    public async Task GetCategoriesAsync_WhenCategoriesExist_ReturnsAllCategories()
    {
        // Arrange
        _getCategoriesResponse = new List<Category> { CategoryTestData.DefaultCategory };
        var service = GetService();

        // Act
        var result = await service.GetCategoriesAsync();

        // Assert
        AssertEx.AreEqual(result, _getCategoriesResponse.Adapt<List<CategoryDto>>());

        _repositoryMock.Verify(r => r.GetCategoriesAsync(), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetCategoriesAsync_WhenNoCategoriesExist_ThrowsNotFoundException()
    {
        // Arrange
        _getCategoriesResponse = new List<Category>();
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.GetCategoriesAsync();

        // Assert
        await AssertEx.Throws<NotFoundException>(act);

        _repositoryMock.Verify(r => r.GetCategoriesAsync(), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
    }

    // CreateCategoryAsync
    [Fact]
    public async Task CreateCategoryAsync_WhenDtoIsValid_CallsRepositoryAdd()
    {
        // Arrange
        var dto = CategoryTestData.DefaultCreateCategoryDto;
        var service = GetService();

        // Act
        var result = await service.CreateCategoryAsync(dto);

        // Assert
        AssertEx.AreEqual(result, ID);
        AssertEx.AreEqual(_createdCategory, new Category
        {
            Name = dto.Name,
            Color = dto.Color,
            ApplicableModules = dto.ApplicableModules
        });

        _repositoryMock.Verify(r => r.CreateCategoryAsync(It.Is<Category>(c =>
            c.Name == dto.Name &&
            c.Color == dto.Color &&
            c.ApplicableModules.SequenceEqual(dto.ApplicableModules)
        )), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
    }

    // UpdateCategoryAsync
    [Fact]
    public async Task UpdateCategoryAsync_WhenCategoryExists_UpdatesAllFields()
    {
        // Arrange
        var category = CategoryTestData.DefaultCategory;
        var dto = CategoryTestData.DefaultUpdateCategoryDto;

        _getByIdResponse = category;
        var service = GetService();

        // Act
        await service.UpdateCategoryAsync(category.Id, dto);

        // Assert
        AssertEx.AreEqual(_updatedCategory, new Category
        {
            Id = category.Id,
            Name = dto.Name,
            Color = dto.Color,
            ApplicableModules = dto.ApplicableModules
        });

        _repositoryMock.Verify(r => r.GetByIdAsync(ID), Times.Once);
        _repositoryMock.Verify(r => r.UpdateCategoryAsync(It.Is<Category>(c =>
            c.Id == ID &&
            c.Name == dto.Name &&
            c.Color == dto.Color &&
            c.ApplicableModules.SequenceEqual(dto.ApplicableModules)
        )), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpdateCategoryAsync_WhenCategoryDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        _getByIdResponse = null;
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.UpdateCategoryAsync(ID, CategoryTestData.DefaultUpdateCategoryDto);

        // Assert
        await AssertEx.Throws<NotFoundException>(act);

        _repositoryMock.Verify(r => r.GetByIdAsync(ID), Times.Once);
        _repositoryMock.Verify(r => r.UpdateCategoryAsync(It.IsAny<Category>()), Times.Never);
        _repositoryMock.VerifyNoOtherCalls();
    }

    // DeleteCategoryAsync
    [Fact]
    public async Task DeleteCategoryAsync_WhenMultipleActiveCategoriesExist_SoftDeletesCategory()
    {
        // Arrange
        var otherCategory = CategoryTestData.DefaultCategory;
        otherCategory.Id = 2;

        _getCategoriesResponse = new List<Category> { CategoryTestData.DefaultCategory, otherCategory };
        _getByIdResponse = CategoryTestData.DefaultCategory;
        var service = GetService();

        // Act
        await service.DeleteCategoryAsync(ID);

        // Assert
        AssertEx.AreEqual(_updatedCategory, new Category
        {
            Id = ID,
            Name = CategoryTestData.DefaultCategory.Name,
            Color = CategoryTestData.DefaultCategory.Color,
            ApplicableModules = CategoryTestData.DefaultApplicableModules,
            IsDeleted = true
        });

        _repositoryMock.Verify(r => r.GetCategoriesAsync(), Times.Once);
        _repositoryMock.Verify(r => r.GetByIdAsync(ID), Times.Once);
        _repositoryMock.Verify(r => r.UpdateCategoryAsync(It.Is<Category>(c =>
            c.Id == ID &&
            c.IsDeleted
        )), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task DeleteCategoryAsync_WhenOnlyOneActiveCategoryExists_ThrowsCannotDeleteLastCategoryException()
    {
        // Arrange
        _getCategoriesResponse = new List<Category> { CategoryTestData.DefaultCategory, CategoryTestData.DeletedCategory };
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.DeleteCategoryAsync(ID);

        // Assert
        await AssertEx.Throws<CannotDeleteLastCategoryException>(act);

        _repositoryMock.Verify(r => r.GetCategoriesAsync(), Times.Once);
        _repositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
        _repositoryMock.Verify(r => r.UpdateCategoryAsync(It.IsAny<Category>()), Times.Never);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task DeleteCategoryAsync_WhenCategoryDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var otherCategory = CategoryTestData.DefaultCategory;
        otherCategory.Id = 2;

        _getCategoriesResponse = new List<Category> { CategoryTestData.DefaultCategory, otherCategory };
        _getByIdResponse = null;
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.DeleteCategoryAsync(ID);

        // Assert
        await AssertEx.Throws<NotFoundException>(act);

        _repositoryMock.Verify(r => r.GetCategoriesAsync(), Times.Once);
        _repositoryMock.Verify(r => r.GetByIdAsync(ID), Times.Once);
        _repositoryMock.Verify(r => r.UpdateCategoryAsync(It.IsAny<Category>()), Times.Never);
        _repositoryMock.VerifyNoOtherCalls();
    }
}
