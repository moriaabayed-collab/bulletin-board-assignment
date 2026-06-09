using BulletinBoardAPI.Models;

namespace BulletinBoardAPI.Services.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user);
}
