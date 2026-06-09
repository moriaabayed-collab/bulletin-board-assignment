using BulletinBoardAPI.Models;
using BulletinBoardAPI.Services;
using BulletinBoardAPI.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;

namespace BulletinBoardAPI.Tests.Services;

public class CategoryServiceTests
{
    private readonly Mock<IJsonStorageService<Category>> _storageMock = new();
    private readonly Mock<ILogger<CategoryService>> _loggerMock = new();
    private readonly CategoryService _sut;
    private readonly List<Category> _data = new();

    public CategoryServiceTests()
    {
        _storageMock.Setup(s => s.GetAll()).Returns(() => new List<Category>(_data));
        _storageMock.Setup(s => s.Modify(It.IsAny<Func<List<Category>, Category>>()))
                    .Returns<Func<List<Category>, Category>>(fn => fn(_data));
        _storageMock.Setup(s => s.Modify(It.IsAny<Func<List<Category>, Category?>>()))
                    .Returns<Func<List<Category>, Category?>>(fn => fn(_data));
        _storageMock.Setup(s => s.Modify(It.IsAny<Func<List<Category>, bool>>()))
                    .Returns<Func<List<Category>, bool>>(fn => fn(_data));

        _sut = new CategoryService(_storageMock.Object, _loggerMock.Object);
    }

    [Fact]
    public void GetAll_ReturnsAllCategories()
    {
        _data.AddRange([new Category { Id = 1, Name = "Tech" }, new Category { Id = 2, Name = "Cars" }]);

        var result = _sut.GetAll();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void GetById_ExistingId_ReturnsCategory()
    {
        _data.Add(new Category { Id = 1, Name = "Tech" });

        var result = _sut.GetById(1);

        Assert.NotNull(result);
        Assert.Equal("Tech", result.Name);
    }

    [Fact]
    public void GetById_NonExistingId_ReturnsNull()
    {
        var result = _sut.GetById(99);

        Assert.Null(result);
    }

    [Fact]
    public void Create_NewName_AddsAndReturnsCategory()
    {
        var result = _sut.Create("Tech");

        Assert.Equal("Tech", result.Name);
        Assert.Equal(1, result.Id);
        Assert.Single(_data);
    }

    [Fact]
    public void Create_FirstCategory_GetsId1()
    {
        var result = _sut.Create("Electronics");

        Assert.Equal(1, result.Id);
    }

    [Fact]
    public void Create_SecondCategory_GetsMaxIdPlusOne()
    {
        _data.Add(new Category { Id = 5, Name = "Existing" });

        var result = _sut.Create("New");

        Assert.Equal(6, result.Id);
    }

    [Fact]
    public void Create_DuplicateName_ThrowsInvalidOperationException()
    {
        _data.Add(new Category { Id = 1, Name = "Tech" });

        Assert.Throws<InvalidOperationException>(() => _sut.Create("Tech"));
    }

    [Fact]
    public void Create_DuplicateNameDifferentCase_ThrowsInvalidOperationException()
    {
        _data.Add(new Category { Id = 1, Name = "Tech" });

        Assert.Throws<InvalidOperationException>(() => _sut.Create("TECH"));
    }

    [Fact]
    public void Update_ExistingId_UpdatesAndReturnsCategory()
    {
        _data.Add(new Category { Id = 1, Name = "Tech" });

        var result = _sut.Update(1, "Electronics");

        Assert.NotNull(result);
        Assert.Equal("Electronics", result.Name);
        Assert.Equal(1, result.Id);
    }

    [Fact]
    public void Update_NonExistingId_ReturnsNull()
    {
        var result = _sut.Update(99, "Anything");

        Assert.Null(result);
    }

    [Fact]
    public void Update_DuplicateNameOnDifferentCategory_ThrowsInvalidOperationException()
    {
        _data.AddRange([
            new Category { Id = 1, Name = "Tech" },
            new Category { Id = 2, Name = "Cars" }
        ]);

        Assert.Throws<InvalidOperationException>(() => _sut.Update(1, "Cars"));
    }

    [Fact]
    public void Update_SameNameOnSameCategory_Succeeds()
    {
        _data.Add(new Category { Id = 1, Name = "Tech" });

        var result = _sut.Update(1, "Tech");

        Assert.NotNull(result);
        Assert.Equal("Tech", result.Name);
    }

    [Fact]
    public void Delete_ExistingId_RemovesAndReturnsTrue()
    {
        _data.Add(new Category { Id = 1, Name = "Tech" });

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
