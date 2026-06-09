using BulletinBoardAPI.Controllers;
using BulletinBoardAPI.Models;
using BulletinBoardAPI.Models.Requests;
using BulletinBoardAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace BulletinBoardAPI.Tests.Controllers;

public class CategoriesControllerTests
{
    private readonly Mock<ICategoryService> _serviceMock = new();
    private readonly CategoriesController _sut;

    public CategoriesControllerTests()
    {
        _sut = new CategoriesController(_serviceMock.Object, new Mock<ILogger<CategoriesController>>().Object);
    }

    [Fact]
    public void GetAll_ReturnsOkWithCategories()
    {
        var categories = new List<Category> { new() { Id = 1, Name = "Tech" } };
        _serviceMock.Setup(s => s.GetAll()).Returns(categories);

        var result = _sut.GetAll();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(categories, ok.Value);
    }

    [Fact]
    public void GetById_ExistingId_ReturnsOk()
    {
        var category = new Category { Id = 1, Name = "Tech" };
        _serviceMock.Setup(s => s.GetById(1)).Returns(category);

        var result = _sut.GetById(1);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(category, ok.Value);
    }

    [Fact]
    public void GetById_NonExistingId_ReturnsNotFound()
    {
        _serviceMock.Setup(s => s.GetById(99)).Returns((Category?)null);

        var result = _sut.GetById(99);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public void Create_ValidRequest_ReturnsCreatedAtAction()
    {
        var created = new Category { Id = 1, Name = "Tech" };
        _serviceMock.Setup(s => s.Create("Tech")).Returns(created);

        var result = _sut.Create(new CategoryRequest { Name = "Tech" });

        var createdAt = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(_sut.GetById), createdAt.ActionName);
        Assert.Equal(created, createdAt.Value);
    }

    [Fact]
    public void Create_DuplicateName_ReturnsBadRequest()
    {
        _serviceMock.Setup(s => s.Create("Tech")).Throws(new InvalidOperationException("Category 'Tech' already exists."));

        var result = _sut.Create(new CategoryRequest { Name = "Tech" });

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public void Update_ExistingId_ReturnsOk()
    {
        var updated = new Category { Id = 1, Name = "Electronics" };
        _serviceMock.Setup(s => s.Update(1, "Electronics")).Returns(updated);

        var result = _sut.Update(1, new CategoryRequest { Name = "Electronics" });

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(updated, ok.Value);
    }

    [Fact]
    public void Update_NonExistingId_ReturnsNotFound()
    {
        _serviceMock.Setup(s => s.Update(99, It.IsAny<string>())).Returns((Category?)null);

        var result = _sut.Update(99, new CategoryRequest { Name = "X" });

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public void Update_DuplicateName_ReturnsBadRequest()
    {
        _serviceMock.Setup(s => s.Update(1, "Cars")).Throws(new InvalidOperationException("Category 'Cars' already exists."));

        var result = _sut.Update(1, new CategoryRequest { Name = "Cars" });

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public void Delete_ExistingId_ReturnsNoContent()
    {
        _serviceMock.Setup(s => s.Delete(1)).Returns(true);

        var result = _sut.Delete(1);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public void Delete_NonExistingId_ReturnsNotFound()
    {
        _serviceMock.Setup(s => s.Delete(99)).Returns(false);

        var result = _sut.Delete(99);

        Assert.IsType<NotFoundObjectResult>(result);
    }
}
