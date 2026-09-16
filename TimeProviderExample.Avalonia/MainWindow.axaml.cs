using Avalonia.Controls;
using TimeProviderExample.Client.Core;

namespace TimeProviderExample.Avalonia;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        Closed += (_, _) => (DataContext as IDisposable)?.Dispose();
    }
}
