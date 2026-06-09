using BulletinBoardAPI.Models;

namespace BulletinBoardAPI.Services.Interfaces;

public interface ICategoryService
{
    List<Category> GetAll();
    Category? GetById(int id);
    Category Create(string name);
    Category? Update(int id, string name);
    bool Delete(int id);
}
