using System;
using Avalonia.Threading;
using TimeProviderExample.Client.Core;

namespace TimeProviderExample.Avalonia;

/// <summary>
/// <see cref="IUiDispatcher"/> implementation backed by Avalonia's UI thread dispatcher.
/// </summary>
public sealed class AvaloniaUiDispatcher : IUiDispatcher
{
    public void Invoke(Action action)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            action();
        }
        else
        {
            Dispatcher.UIThread.Invoke(action);
        }
    }
}
