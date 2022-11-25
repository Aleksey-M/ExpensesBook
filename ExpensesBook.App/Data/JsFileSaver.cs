using Microsoft.JSInterop;

namespace ExpensesBook.Data;

public sealed class JsFileSaver : IAsyncDisposable
{
    private readonly Lazy<Task<IJSObjectReference>> moduleTask;

    public JsFileSaver(IJSRuntime jsRuntime)
    {
        moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./_content/ExpensesBook.App/fileSaveAs.js").AsTask());
    }

    public async Task SaveJsonFile(string fileName, string jsonString)
    {
        var module = await moduleTask.Value;
        await module.InvokeVoidAsync("fileSaveAs", fileName, jsonString);
    }

    public async Task SaveFile(string fileName, string base64String)
    {
        var module = await moduleTask.Value;
        await module.InvokeVoidAsync("fileSaveAs", fileName, base64String, true);
    }

    public async ValueTask DisposeAsync()
    {
        if (moduleTask.IsValueCreated)
        {
            var module = await moduleTask.Value;
            await module.DisposeAsync();
        }
    }
}
