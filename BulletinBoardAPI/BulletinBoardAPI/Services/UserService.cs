using System.Security.Cryptography;
using BulletinBoardAPI.Models;
using BulletinBoardAPI.Services.Interfaces;

namespace BulletinBoardAPI.Services;

public class UserService : IUserService
{
    private readonly List<User> _users = [];
    private int _nextId = 1;

    public User? Register(string email, string firstName, string lastName, string password)
    {
        if (_users.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
        {
            return null;
        }

        var user = new User
        {
            Id = _nextId++,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            PasswordHash = HashPassword(password)
        };
        _users.Add(user);
        return user;
    }

    public User? Authenticate(string email, string password)
    {
        var user = _users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        if (user == null || !VerifyPassword(password, user.PasswordHash))
        {
            return null;
        }

        return user;
    }

    private static string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100_000, HashAlgorithmName.SHA256, 32);
        return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }

    private static bool VerifyPassword(string password, string storedHash)
    {
        var parts = storedHash.Split(':');
        var salt = Convert.FromBase64String(parts[0]);
        var expectedHash = Convert.FromBase64String(parts[1]);
        var actualHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100_000, HashAlgorithmName.SHA256, 32);
        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }
}
