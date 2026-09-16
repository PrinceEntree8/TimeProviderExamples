namespace TimeProviderExample.Client.Core;

/// <summary>
/// Marshals work onto the UI thread. WPF uses its Dispatcher,
/// Avalonia uses <c>Dispatcher.UIThread</c>.
/// </summary>
public interface IUiDispatcher
{
    void Invoke(Action action);
}
