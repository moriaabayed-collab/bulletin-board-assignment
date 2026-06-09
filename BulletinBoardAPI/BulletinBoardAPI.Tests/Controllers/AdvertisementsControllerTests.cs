using BulletinBoardAPI.Controllers;
using BulletinBoardAPI.Models;
using BulletinBoardAPI.Models.Requests;
using BulletinBoardAPI.Models.Responses;
using BulletinBoardAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace BulletinBoardAPI.Tests.Controllers;

public class AdvertisementsControllerTests
{
    private readonly Mock<IAdvertisementService> _adServiceMock = new();
    private readonly Mock<ICategoryService> _catServiceMock = new();
    private readonly AdvertisementsController _sut;
    private readonly Category _category = new() { Id = 1, Name = "Tech" };

    public AdvertisementsControllerTests()
    {
        _sut = new AdvertisementsController(
            _adServiceMock.Object,
            _catServiceMock.Object,
            new Mock<ILogger<AdvertisementsController>>().Object);

        _catServiceMock.Setup(c => c.GetAll()).Returns([_category]);
        _catServiceMock.Setup(c => c.GetById(_category.Id)).Returns(_category);
    }

    private Advertisement MakeAd(int id = 1) => new()
    {
        Id = id,
        Title = "Laptop",
        CategoryId = _category.Id,
        Contact = "a@a.com"
    };

    private AdvertisementRequest MakeRequest() => new()
    {
        Title = "Laptop",
        CategoryId = _category.Id,
        Contact = "a@a.com"
    };

    [Fact]
    public void GetAll_NoFilter_ReturnsOkWithAllAds()
    {
        _adServiceMock.Setup(s => s.GetAll()).Returns([MakeAd()]);

        var result = _sut.GetAll(null);

        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public void GetAll_WithCategoryFilter_ReturnsFilteredAds()
    {
        _adServiceMock.Setup(s => s.GetByCategory(_category.Id)).Returns([MakeAd()]);

        var result = _sut.GetAll(_category.Id);

        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public void GetAll_InvalidCategoryFilter_ReturnsNotFound()
    {
        _adServiceMock.Setup(s => s.GetByCategory(99)).Throws(new InvalidOperationException("Category with id 99 not found."));

        var result = _sut.GetAll(99);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public void GetById_ExistingId_ReturnsOk()
    {
        var ad = MakeAd();
        _adServiceMock.Setup(s => s.GetById(1)).Returns(ad);

        var result = _sut.GetById(1);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<AdvertisementResponse>(ok.Value);
        Assert.Equal("Laptop", response.Title);
    }

    [Fact]
    public void GetById_NonExistingId_ReturnsNotFound()
    {
        _adServiceMock.Setup(s => s.GetById(99)).Returns((Advertisement?)null);

        var result = _sut.GetById(99);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public void Create_ValidRequest_ReturnsCreatedAtAction()
    {
        var ad = MakeAd();
        _adServiceMock.Setup(s => s.Create(It.IsAny<Advertisement>())).Returns(ad);

        var result = _sut.Create(MakeRequest());

        var createdAt = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(_sut.GetById), createdAt.ActionName);
    }

    [Fact]
    public void Create_InvalidCategory_ReturnsBadRequest()
    {
        _adServiceMock.Setup(s => s.Create(It.IsAny<Advertisement>()))
                      .Throws(new InvalidOperationException("Category with id 99 not found."));

        var result = _sut.Create(MakeRequest());

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public void Create_DuplicateAd_ReturnsBadRequest()
    {
        _adServiceMock.Setup(s => s.Create(It.IsAny<Advertisement>()))
                      .Throws(new InvalidOperationException("Advertisement already exists."));

        var result = _sut.Create(MakeRequest());

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public void Update_ExistingId_ReturnsOk()
    {
        var updated = MakeAd();
        _adServiceMock.Setup(s => s.Update(1, It.IsAny<Advertisement>())).Returns(updated);

        var result = _sut.Update(1, MakeRequest());

        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public void Update_NonExistingId_ReturnsNotFound()
    {
        _adServiceMock.Setup(s => s.Update(99, It.IsAny<Advertisement>())).Returns((Advertisement?)null);

        var result = _sut.Update(99, MakeRequest());

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public void Update_InvalidCategory_ReturnsBadRequest()
    {
        _adServiceMock.Setup(s => s.Update(1, It.IsAny<Advertisement>()))
                      .Throws(new InvalidOperationException("Category with id 99 not found."));

        var result = _sut.Update(1, MakeRequest());

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public void Delete_ExistingId_ReturnsNoContent()
    {
        _adServiceMock.Setup(s => s.Delete(1)).Returns(true);

        var result = _sut.Delete(1);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public void Delete_NonExistingId_ReturnsNotFound()
    {
        _adServiceMock.Setup(s => s.Delete(99)).Returns(false);

        var result = _sut.Delete(99);

        Assert.IsType<NotFoundObjectResult>(result);
    }
}
