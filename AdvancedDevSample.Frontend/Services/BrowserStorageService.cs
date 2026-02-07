using Microsoft.JSInterop;

namespace AdvancedDevSample.Frontend.Services;

public class BrowserStorageService
{
    private readonly IJSRuntime _jsRuntime;

    public BrowserStorageService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public ValueTask SetItemAsync(string key, string value)
    {
        return _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", key, value);
    }

    public ValueTask<string?> GetItemAsync(string key)
    {
        return _jsRuntime.InvokeAsync<string?>("sessionStorage.getItem", key);
    }

    public ValueTask RemoveItemAsync(string key)
    {
        return _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", key);
    }
}
