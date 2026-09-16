using Microsoft.Extensions.DependencyInjection;
using TimeProviderExample.Client.Core;

namespace TimeProviderExample.Wpf;

/// <summary>
/// <see cref="IMainWindowFactory"/> implementation that spawns new WPF windows.
/// Resolution and <c>Show()</c> are marshalled onto the UI thread.
/// </summary>
public sealed class WpfMainWindowFactory(IServiceProvider serviceProvider, IUiDispatcher dispatcher) : IMainWindowFactory
{
    public void ShowNewWindow()
    {
        dispatcher.Invoke(() =>
        {
            var mainWindow = serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        });
    }
}
