using BulletinBoardAPI.Models;

namespace BulletinBoardAPI.Services.Interfaces;

public interface IUserService
{
    User? Register(string email, string firstName, string lastName, string password);
    User? Authenticate(string email, string password);
}
