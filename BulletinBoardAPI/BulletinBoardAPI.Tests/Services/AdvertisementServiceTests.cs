using BulletinBoardAPI.Models;
using BulletinBoardAPI.Services;
using BulletinBoardAPI.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;

namespace BulletinBoardAPI.Tests.Services;

public class AdvertisementServiceTests
{
    private readonly Mock<IJsonStorageService<Advertisement>> _storageMock = new();
    private readonly Mock<ICategoryService> _categoryServiceMock = new();
    private readonly Mock<ILogger<AdvertisementService>> _loggerMock = new();
    private readonly AdvertisementService _sut;
    private readonly List<Advertisement> _data = new();
    private readonly Category _category = new() { Id = 1, Name = "Tech" };

    public AdvertisementServiceTests()
    {
        _storageMock.Setup(s => s.GetAll()).Returns(() => new List<Advertisement>(_data));
        _storageMock.Setup(s => s.Modify(It.IsAny<Func<List<Advertisement>, Advertisement>>()))
                    .Returns<Func<List<Advertisement>, Advertisement>>(fn => fn(_data));
        _storageMock.Setup(s => s.Modify(It.IsAny<Func<List<Advertisement>, Advertisement?>>()))
                    .Returns<Func<List<Advertisement>, Advertisement?>>(fn => fn(_data));
        _storageMock.Setup(s => s.Modify(It.IsAny<Func<List<Advertisement>, bool>>()))
                    .Returns<Func<List<Advertisement>, bool>>(fn => fn(_data));

        _categoryServiceMock.Setup(c => c.GetById(_category.Id)).Returns(_category);

        _sut = new AdvertisementService(_storageMock.Object, _categoryServiceMock.Object, _loggerMock.Object);
    }

    private Advertisement MakeAd(int id = 1) => new Advertisement
    {
        Id = id,
        Title = "Laptop",
        Description = "Good laptop",
        CategoryId = _category.Id,
        Contact = "test@test.com"
    };

    [Fact]
    public void GetAll_ReturnsAllAdvertisements()
    {
        _data.AddRange([MakeAd(1), MakeAd(2)]);

        var result = _sut.GetAll();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void GetById_ExistingId_ReturnsAdvertisement()
    {
        _data.Add(MakeAd(1));

        var result = _sut.GetById(1);

        Assert.NotNull(result);
        Assert.Equal("Laptop", result.Title);
    }

    [Fact]
    public void GetById_NonExistingId_ReturnsNull()
    {
        var result = _sut.GetById(99);

        Assert.Null(result);
    }

    [Fact]
    public void GetByCategory_ValidCategory_ReturnsFilteredAds()
    {
        _data.Add(MakeAd(1));
        _data.Add(new Advertisement { Id = 2, Title = "Car", CategoryId = 2, Contact = "x" });

        var result = _sut.GetByCategory(_category.Id);

        Assert.Single(result);
        Assert.Equal("Laptop", result[0].Title);
    }

    [Fact]
    public void GetByCategory_InvalidCategory_ThrowsInvalidOperationException()
    {
        _categoryServiceMock.Setup(c => c.GetById(99)).Returns((Category?)null);

        Assert.Throws<InvalidOperationException>(() => _sut.GetByCategory(99));
    }

    [Fact]
    public void Create_ValidAd_AddsAndReturnsAd()
    {
        var ad = MakeAd();
        ad.Id = 0;

        var result = _sut.Create(ad);

        Assert.Equal(1, result.Id);
        Assert.Single(_data);
    }

    [Fact]
    public void Create_SecondAd_GetsMaxIdPlusOne()
    {
        _data.Add(MakeAd(5));
        var newAd = new Advertisement { Title = "Phone", CategoryId = _category.Id, Contact = "b@b.com" };

        var result = _sut.Create(newAd);

        Assert.Equal(6, result.Id);
    }

    [Fact]
    public void Create_InvalidCategory_ThrowsInvalidOperationException()
    {
        _categoryServiceMock.Setup(c => c.GetById(99)).Returns((Category?)null);
        var ad = new Advertisement { Title = "Thing", CategoryId = 99, Contact = "x" };

        Assert.Throws<InvalidOperationException>(() => _sut.Create(ad));
    }

    [Fact]
    public void Create_DuplicateAd_ThrowsInvalidOperationException()
    {
        var ad = MakeAd(1);
        _data.Add(ad);
        var duplicate = MakeAd(0);

        Assert.Throws<InvalidOperationException>(() => _sut.Create(duplicate));
    }

    [Fact]
    public void Update_ExistingId_UpdatesAndReturnsAd()
    {
        _data.Add(MakeAd(1));
        var updated = new Advertisement { Title = "Phone", CategoryId = _category.Id, Contact = "new@x.com" };

        var result = _sut.Update(1, updated);

        Assert.NotNull(result);
        Assert.Equal("Phone", result.Title);
        Assert.Equal(1, result.Id);
    }

    [Fact]
    public void Update_NonExistingId_ReturnsNull()
    {
        var ad = MakeAd(99);

        var result = _sut.Update(99, ad);

        Assert.Null(result);
    }

    [Fact]
    public void Update_InvalidCategory_ThrowsInvalidOperationException()
    {
        _data.Add(MakeAd(1));
        _categoryServiceMock.Setup(c => c.GetById(99)).Returns((Category?)null);
        var ad = new Advertisement { Title = "X", CategoryId = 99, Contact = "x" };

        Assert.Throws<InvalidOperationException>(() => _sut.Update(1, ad));
    }

    [Fact]
    public void Update_DuplicateMatchingOtherAd_ThrowsInvalidOperationException()
    {
        _data.Add(MakeAd(1));
        var other = new Advertisement { Id = 2, Title = "Phone", CategoryId = _category.Id, Description = "D", Contact = "y@y.com" };
        _data.Add(other);
        var duplicate = new Advertisement { Title = "Phone", CategoryId = _category.Id, Description = "D", Contact = "y@y.com" };

        Assert.Throws<InvalidOperationException>(() => _sut.Update(1, duplicate));
    }

    [Fact]
    public void Update_EnforcesIdFromParameter()
    {
        _data.Add(MakeAd(1));
        var ad = new Advertisement { Id = 999, Title = "Phone", CategoryId = _category.Id, Contact = "x" };

        var result = _sut.Update(1, ad);

        Assert.Equal(1, result!.Id);
    }

    [Fact]
    public void Delete_ExistingId_RemovesAndReturnsTrue()
    {
        _data.Add(MakeAd(1));

        var result = _sut.Delete(1);

        Assert.True(result);
        Assert.Empty(_data);
    }

    [Fact]
    public void Delete_NonExistingId_ReturnsFalse()
    {
        var result = _sut.Delete(99);

        Assert.False(result);
    }
}
