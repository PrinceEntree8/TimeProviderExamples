namespace TimeProviderExample.Client.Core;

/// <summary>
/// Abstracts showing a new main window so the shared view-model
/// stays independent of the concrete UI framework (WPF / Avalonia).
/// </summary>
public interface IMainWindowFactory
{
    void ShowNewWindow();
}
