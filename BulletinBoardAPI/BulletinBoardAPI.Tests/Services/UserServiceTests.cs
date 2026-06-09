using BulletinBoardAPI.Services;

namespace BulletinBoardAPI.Tests.Services;

public class UserServiceTests
{
    private readonly UserService _sut = new();

    [Fact]
    public void Register_NewEmail_ReturnsUserWithCorrectData()
    {
        var result = _sut.Register("john@example.com", "John", "Doe", "password123");

        Assert.NotNull(result);
        Assert.Equal("john@example.com", result.Email);
        Assert.Equal("John", result.FirstName);
        Assert.Equal("Doe", result.LastName);
    }

    [Fact]
    public void Register_FirstUser_GetsId1()
    {
        var result = _sut.Register("john@example.com", "John", "Doe", "password123");

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
    }

    [Fact]
    public void Register_SecondUser_GetsId2()
    {
        _sut.Register("first@example.com", "First", "User", "password123");
        var result = _sut.Register("second@example.com", "Second", "User", "password123");

        Assert.NotNull(result);
        Assert.Equal(2, result.Id);
    }

    [Fact]
    public void Register_DoesNotStorePasswordInPlainText()
    {
        var result = _sut.Register("john@example.com", "John", "Doe", "password123");

        Assert.NotNull(result);
        Assert.NotEqual("password123", result.PasswordHash);
    }

    [Fact]
    public void Register_DuplicateEmail_ReturnsNull()
    {
        _sut.Register("john@example.com", "John", "Doe", "password123");

        var result = _sut.Register("john@example.com", "Jane", "Doe", "other123");

        Assert.Null(result);
    }

    [Fact]
    public void Register_DuplicateEmailDifferentCase_ReturnsNull()
    {
        _sut.Register("john@example.com", "John", "Doe", "password123");

        var result = _sut.Register("JOHN@EXAMPLE.COM", "John", "Doe", "password123");

        Assert.Null(result);
    }

    [Fact]
    public void Authenticate_CorrectCredentials_ReturnsUser()
    {
        _sut.Register("john@example.com", "John", "Doe", "password123");

        var result = _sut.Authenticate("john@example.com", "password123");

        Assert.NotNull(result);
        Assert.Equal("john@example.com", result.Email);
    }

    [Fact]
    public void Authenticate_EmailDifferentCase_ReturnsUser()
    {
        _sut.Register("john@example.com", "John", "Doe", "password123");

        var result = _sut.Authenticate("JOHN@EXAMPLE.COM", "password123");

        Assert.NotNull(result);
    }

    [Fact]
    public void Authenticate_WrongPassword_ReturnsNull()
    {
        _sut.Register("john@example.com", "John", "Doe", "password123");

        var result = _sut.Authenticate("john@example.com", "wrongpassword");

        Assert.Null(result);
    }

    [Fact]
    public void Authenticate_UnknownEmail_ReturnsNull()
    {
        var result = _sut.Authenticate("nobody@example.com", "password123");

        Assert.Null(result);
    }

    [Fact]
    public void Authenticate_AfterRegisterReturnsNull_OriginalUserStillAuthenticated()
    {
        _sut.Register("john@example.com", "John", "Doe", "password123");
        _sut.Register("john@example.com", "Duplicate", "User", "other123"); // returns null, no-op

        var result = _sut.Authenticate("john@example.com", "password123");

        Assert.NotNull(result);
        Assert.Equal("John", result.FirstName);
    }
}
