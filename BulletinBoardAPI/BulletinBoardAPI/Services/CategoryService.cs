using BulletinBoardAPI.Models;
using BulletinBoardAPI.Services.Interfaces;

namespace BulletinBoardAPI.Services;

public class CategoryService : ICategoryService
{
    private readonly IJsonStorageService<Category> _storage;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(IJsonStorageService<Category> storage, ILogger<CategoryService> logger)
    {
        _storage = storage;
        _logger = logger;
    }

    public List<Category> GetAll()
    {
        var categories = _storage.GetAll();
        _logger.LogInformation("Retrieved {Count} categories.", categories.Count);
        return categories;
    }

    public Category? GetById(int id)
    {
        var category = _storage.GetAll().FirstOrDefault(c => c.Id == id);
        if (category == null)
        {
            _logger.LogWarning("Category with ID {Id} was not found.", id);
        }

        return category;
    }

    public Category Create(string name)
    {
        _logger.LogInformation("Creating category '{Name}'.", name);
        return _storage.Modify(categories =>
        {
            if (categories.Any(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase)))
            {
                _logger.LogWarning("Duplicate category detected: '{Name}'.", name);
                throw new InvalidOperationException($"Category '{name}' already exists.");
            }

            var category = new Category
            {
                Id = categories.Count > 0 ? categories.Max(c => c.Id) + 1 : 1,
                Name = name
            };

            categories.Add(category);
            _logger.LogInformation("Category created with ID {Id}.", category.Id);
            return category;
        });
    }

    public Category? Update(int id, string name)
    {
        _logger.LogInformation("Updating category with ID {Id}.", id);
        return _storage.Modify(categories =>
        {
            var index = categories.FindIndex(c => c.Id == id);
            if (index == -1)
            {
                _logger.LogWarning("Category with ID {Id} not found for update.", id);
                return null;
            }

            if (categories.Any(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase) && c.Id != id))
            {
                _logger.LogWarning("Duplicate category name '{Name}' detected during update.", name);
                throw new InvalidOperationException($"Category '{name}' already exists.");
            }

            categories[index] = new Category { Id = id, Name = name };
            _logger.LogInformation("Category with ID {Id} updated successfully.", id);
            return categories[index];
        });
    }

    public bool Delete(int id)
    {
        _logger.LogInformation("Deleting category with ID {Id}.", id);
        return _storage.Modify(categories =>
        {
            var category = categories.FirstOrDefault(c => c.Id == id);
            if (category == null)
            {
                _logger.LogWarning("Category with ID {Id} not found for deletion.", id);
                return false;
            }

            categories.Remove(category);
            _logger.LogInformation("Category with ID {Id} deleted successfully.", id);
            return true;
        });
    }
}
