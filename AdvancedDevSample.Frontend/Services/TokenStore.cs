using System.Text.Json;
using AdvancedDevSample.Frontend.Models;

namespace AdvancedDevSample.Frontend.Services;

public class TokenStore
{
    private const string StorageKey = "advanceddevsample.auth";

    private readonly BrowserStorageService _storage;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);
    private StoredAuthSession? _cached;

    public TokenStore(BrowserStorageService storage)
    {
        _storage = storage;
    }

    public async Task<StoredAuthSession?> GetAsync()
    {
        if (_cached is not null)
        {
            return _cached;
        }

        string? json = await _storage.GetItemAsync(StorageKey);
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            _cached = JsonSerializer.Deserialize<StoredAuthSession>(json, _jsonOptions);
            return _cached;
        }
        catch
        {
            await ClearAsync();
            return null;
        }
    }

    public async Task SaveAsync(StoredAuthSession session)
    {
        _cached = session;
        string json = JsonSerializer.Serialize(session, _jsonOptions);
        await _storage.SetItemAsync(StorageKey, json);
    }

    public async Task ClearAsync()
    {
        _cached = null;
        await _storage.RemoveItemAsync(StorageKey);
    }
}
