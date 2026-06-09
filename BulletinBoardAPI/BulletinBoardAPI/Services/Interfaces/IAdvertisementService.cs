using BulletinBoardAPI.Models;

namespace BulletinBoardAPI.Services.Interfaces;

public interface IAdvertisementService
{
    List<Advertisement> GetAll();
    List<Advertisement> GetByCategory(int categoryId);
    Advertisement? GetById(int id);
    Advertisement Create(Advertisement advertisement);
    Advertisement? Update(int id, Advertisement advertisement);
    bool Delete(int id);
}
