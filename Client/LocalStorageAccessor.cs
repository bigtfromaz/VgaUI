using Microsoft.JSInterop;
using VgaUI.Shared;

namespace VgaUI.Client;
public sealed class LocalStorageAccessor : IAsyncDisposable
{
    private Lazy<IJSObjectReference> _accessorJsRef = new();
    private readonly IJSRuntime _jsRuntime;

    public LocalStorageAccessor(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    private async Task WaitForReference()
    {
        if (_accessorJsRef.IsValueCreated is false)
        {
            _accessorJsRef = new(await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "/js/LocalStorageAccessor.js"));
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_accessorJsRef.IsValueCreated)
        {
            await _accessorJsRef.Value.DisposeAsync();
        }
    }
    public async Task<string> GetValueAsyncString(string key)
    {
        await WaitForReference();
        var result = await _accessorJsRef.Value.InvokeAsync<string>("get", key);

        return result;
    }
    public async Task<T?> GetValueAsyncJson<T>(string key)
    {
        await WaitForReference();
        string result = await _accessorJsRef.Value.InvokeAsync<string>("get", key);
        if (result == null) 
            return default;
        else
        {
            T? x = Helpers.Deserialize<T>(result);
            return x;
        }
    }

    public async Task SetValueAsyncString<T>(string key, T value)
    {
        await WaitForReference();
        await _accessorJsRef.Value.InvokeVoidAsync("set", key, value);
    }
    public async Task SetValueAsyncJson<T>(string key, T value)
    {
        await WaitForReference();
        if (value != null)
        {
            await _accessorJsRef.Value.InvokeVoidAsync("set", key, Helpers.Serialize(value));
        }
        else
        {
            throw new Exception("");
        }
    }

    public async Task Clear()
    {
        await WaitForReference();
        await _accessorJsRef.Value.InvokeVoidAsync("clear");
    }

    public async Task RemoveAsync(string key)
    {
        await WaitForReference();
        await _accessorJsRef.Value.InvokeVoidAsync("remove", key);
    }
}