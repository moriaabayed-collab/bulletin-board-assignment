using System.Text.Json;
using BulletinBoardAPI.Services.Interfaces;

namespace BulletinBoardAPI.Services;

public class JsonStorageService<T> : IJsonStorageService<T>
{
    private readonly string _filePath;
    private readonly ReaderWriterLockSlim _lock = new();
    private readonly ILogger<JsonStorageService<T>> _logger;
    private static readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true
    };

    public JsonStorageService(string filePath, ILogger<JsonStorageService<T>> logger)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path cannot be empty.");
        }

        _filePath = filePath;
        _logger = logger;

        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
            _logger.LogInformation("Created data directory: {Directory}.", directory);
        }

        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, "[]");
            _logger.LogInformation("Created empty store file: {FilePath}.", filePath);
        }
    }

    public List<T> GetAll()
    {
        _lock.EnterReadLock();
        try
        {
            var items = ReadFromDisk();
            _logger.LogDebug("Read {Count} items from {FilePath}.", items.Count, _filePath);
            return items;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public void Modify(Action<List<T>> modifier)
    {
        _lock.EnterWriteLock();
        try
        {
            var items = ReadFromDisk();
            modifier(items);
            
            // only reached if modifier did not throw
            File.WriteAllText(_filePath, JsonSerializer.Serialize(items, _options));
            _logger.LogDebug("Saved {Count} items to {FilePath}.", items.Count, _filePath);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public TResult Modify<TResult>(Func<List<T>, TResult> modifier)
    {
        _lock.EnterWriteLock();
        try
        {
            var items = ReadFromDisk();
            var result = modifier(items);
            
            // only reached if modifier did not throw
            File.WriteAllText(_filePath, JsonSerializer.Serialize(items, _options));
            _logger.LogDebug("Saved {Count} items to {FilePath}.", items.Count, _filePath);
            return result;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    private List<T> ReadFromDisk()
    {
        if (!File.Exists(_filePath))
        {
            _logger.LogError("Store file not found: {FilePath}.", _filePath);
            throw new FileNotFoundException($"File: {_filePath} not found.");
        }

        try
        {
            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<List<T>>(json, _options) ?? [];
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize {FilePath}. Returning empty list.", _filePath);
            return [];
        }
    }

    public void Dispose() => _lock.Dispose();
}
