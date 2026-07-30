using System.Net.Http;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace TimeProviderExample.Wpf;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public static IServiceProvider ServiceProvider { get; private set; } = null!;

    public App()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        ServiceProvider = services.BuildServiceProvider();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<HttpClient>();
        services.AddSingleton<TimeProvider>(_ => new UdpTimeProvider("239.0.0.1", 6000));
        services.AddTransient<MainViewModel>();
        services.AddTransient<MainWindow>();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }
}

