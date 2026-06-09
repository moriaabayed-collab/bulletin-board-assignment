using BulletinBoardAPI.Models;
using BulletinBoardAPI.Models.Mappers;
using BulletinBoardAPI.Models.Requests;

namespace BulletinBoardAPI.Tests.Models;

public class AdvertisementMapperTests
{
    private readonly AdvertisementRequest _request = new()
    {
        Title = "Laptop",
        Description = "Good one",
        CategoryId = 1,
        Price = 999.99m,
        Currency = "USD",
        Location = "NYC",
        Contact = "a@a.com"
    };

    [Fact]
    public void ToAdvertisement_MapsAllFields()
    {
        var result = _request.ToAdvertisement();

        Assert.Equal("Laptop", result.Title);
        Assert.Equal("Good one", result.Description);
        Assert.Equal(1, result.CategoryId);
        Assert.Equal(999.99m, result.Price);
        Assert.Equal("USD", result.Currency);
        Assert.Equal("NYC", result.Location);
        Assert.Equal("a@a.com", result.Contact);
        Assert.NotNull(result.LastUpdate);
    }

    [Fact]
    public void ToAdvertisement_WithId_SetsId()
    {
        var result = _request.ToAdvertisement(42);

        Assert.Equal(42, result.Id);
        Assert.Equal("Laptop", result.Title);
    }

    [Fact]
    public void ToResponse_WithCategory_MapsAllFields()
    {
        var ad = new Advertisement { Id = 1, Title = "Laptop", CategoryId = 1, Contact = "a@a.com", Price = 100m };
        var category = new Category { Id = 1, Name = "Tech" };

        var result = ad.ToResponse(category);

        Assert.Equal(1, result.Id);
        Assert.Equal("Laptop", result.Title);
        Assert.Equal(category, result.Category);
        Assert.Equal(100m, result.Price);
    }

    [Fact]
    public void ToResponse_NullCategory_CategoryIsNull()
    {
        var ad = new Advertisement { Id = 1, Title = "Laptop", CategoryId = 999, Contact = "a@a.com" };

        var result = ad.ToResponse(null);

        Assert.Null(result.Category);
    }

    [Fact]
    public void ToResponseList_MapsEachAdToCorrectCategory()
    {
        var cats = new List<Category>
        {
            new() { Id = 1, Name = "Tech" },
            new() { Id = 2, Name = "Cars" }
        };
        var ads = new List<Advertisement>
        {
            new() { Id = 1, Title = "Laptop", CategoryId = 1, Contact = "x" },
            new() { Id = 2, Title = "BMW", CategoryId = 2, Contact = "y" }
        };

        var result = ads.ToResponseList(cats);

        Assert.Equal(2, result.Count);
        Assert.Equal("Tech", result[0].Category!.Name);
        Assert.Equal("Cars", result[1].Category!.Name);
    }

    [Fact]
    public void ToResponseList_UnknownCategoryId_CategoryIsNull()
    {
        var ads = new List<Advertisement>
        {
            new() { Id = 1, Title = "X", CategoryId = 99, Contact = "x" }
        };

        var result = ads.ToResponseList(new List<Category>());

        Assert.Null(result[0].Category);
    }
}
