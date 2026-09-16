using System.Windows;
using TimeProviderExample.Client.Core;

namespace TimeProviderExample.Wpf;

/// <summary>
/// <see cref="IUiDispatcher"/> implementation backed by the WPF <see cref="System.Windows.Threading.Dispatcher"/>.
/// </summary>
public sealed class WpfUiDispatcher : IUiDispatcher
{
    public void Invoke(Action action)
    {
        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher is null || dispatcher.CheckAccess())
        {
            action();
        }
        else
        {
            dispatcher.Invoke(action);
        }
    }
}
