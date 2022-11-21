using System;
using System.Threading;
using Microsoft.AspNetCore.Components;

namespace ExpensesBook.Pages;

public class BasePage : ComponentBase, IDisposable
{
    private readonly Lazy<CancellationTokenSource> _cts = new(() => new CancellationTokenSource(), isThreadSafe: false);

    protected CancellationToken Token => _cts.Value.Token;

    public void Dispose()
    {
        if (_cts.IsValueCreated)
        {
            _cts.Value.Cancel();
        }
    }
}
