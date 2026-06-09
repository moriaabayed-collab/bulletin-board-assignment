using BulletinBoardAPI.Models;
using BulletinBoardAPI.Services.Interfaces;

namespace BulletinBoardAPI.Services;

public class AdvertisementService : IAdvertisementService
{
    private readonly IJsonStorageService<Advertisement> _storage;
    private readonly ICategoryService _categoryService;
    private readonly ILogger<AdvertisementService> _logger;

    public AdvertisementService(IJsonStorageService<Advertisement> storage, ICategoryService categoryService, ILogger<AdvertisementService> logger)
    {
        _storage = storage;
        _categoryService = categoryService;
        _logger = logger;
    }

    public List<Advertisement> GetAll()
    {
        var ads = _storage.GetAll();
        _logger.LogInformation("Retrieved {Count} advertisements.", ads.Count);
        return ads;
    }

    public List<Advertisement> GetByCategory(int categoryId)
    {
        _logger.LogInformation("Fetching advertisements for category {CategoryId}.", categoryId);
        ValidateCategoryExists(categoryId);

        var ads = _storage.GetAll()
            .Where(a => a.CategoryId == categoryId)
            .ToList();
        
        _logger.LogInformation("Found {Count} advertisements in category {CategoryId}.", ads.Count, categoryId);
        return ads;
    }

    public Advertisement? GetById(int id)
    {
        var ad = _storage.GetAll().FirstOrDefault(a => a.Id == id);
        if (ad == null)
        {
            _logger.LogWarning("Advertisement with ID {Id} was not found.", id);
        }

        return ad;
    }

    public Advertisement Create(Advertisement advertisement)
    {
        _logger.LogInformation("Creating advertisement with title '{Title}'.", advertisement.Title);
        ValidateCategoryExists(advertisement.CategoryId);

        return _storage.Modify(ads =>
        {
            if (ads.Any(a => a.Title == advertisement.Title &&
                             a.Description == advertisement.Description &&
                             a.CategoryId == advertisement.CategoryId &&
                             a.Contact == advertisement.Contact))
            {
                _logger.LogWarning("Duplicate advertisement detected: '{Title}'.", advertisement.Title);
                throw new InvalidOperationException("Advertisement already exists.");
            }

            advertisement.Id = ads.Count > 0 ? ads.Max(a => a.Id) + 1 : 1;
            ads.Add(advertisement);
            _logger.LogInformation("Advertisement created with ID {Id}.", advertisement.Id);
            return advertisement;
        });
    }

    public Advertisement? Update(int id, Advertisement advertisement)
    {
        _logger.LogInformation("Updating advertisement with ID {Id}.", id);
        advertisement.Id = id; // fix #6: enforce Id regardless of caller

        return _storage.Modify(ads =>
        {
            var index = ads.FindIndex(a => a.Id == id);
            if (index == -1)
            {
                _logger.LogWarning("Advertisement with ID {Id} not found for update.", id);
                return null;
            }

            ValidateCategoryExists(advertisement.CategoryId); // fix #2: after existence check

            if (ads.Any(a => a.Title == advertisement.Title &&
                             a.Description == advertisement.Description &&
                             a.CategoryId == advertisement.CategoryId &&
                             a.Contact == advertisement.Contact &&
                             a.Id != id))
            {
                _logger.LogWarning("Duplicate advertisement detected during update for ID {Id}.", id);
                throw new InvalidOperationException("Advertisement already exists.");
            }

            ads[index] = advertisement;
            _logger.LogInformation("Advertisement with ID {Id} updated successfully.", id);
            return ads[index];
        });
    }

    public bool Delete(int id)
    {
        _logger.LogInformation("Deleting advertisement with ID {Id}.", id);
        return _storage.Modify(ads =>
        {
            var ad = ads.FirstOrDefault(a => a.Id == id);
            if (ad == null)
            {
                _logger.LogWarning("Advertisement with ID {Id} not found for deletion.", id);
                return false;
            }

            ads.Remove(ad);
            _logger.LogInformation("Advertisement with ID {Id} deleted successfully.", id);
            return true;
        });
    }

    private void ValidateCategoryExists(int categoryId)
    {
        if (_categoryService.GetById(categoryId) == null)
        {
            _logger.LogWarning("Category with ID {CategoryId} does not exist.", categoryId);
            throw new InvalidOperationException($"Category with id {categoryId} not found.");
        }
    }
}
