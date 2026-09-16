using Microsoft.Extensions.DependencyInjection;
using TimeProviderExample.Client.Core;

namespace TimeProviderExample.Avalonia;

/// <summary>
/// <see cref="IMainWindowFactory"/> implementation that spawns new Avalonia windows.
/// Resolution and <c>Show()</c> are marshalled onto the UI thread.
/// </summary>
public sealed class AvaloniaMainWindowFactory(IServiceProvider serviceProvider, IUiDispatcher dispatcher) : IMainWindowFactory
{
    public void ShowNewWindow()
    {
        dispatcher.Invoke(() =>
        {
            var window = serviceProvider.GetRequiredService<MainWindow>();
            window.Show();
        });
    }
}
