using System.Net.Http;
using System.Net.Http.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace TimeProviderExample.Wpf;

public partial class MainViewModel : ObservableObject, IDisposable
{
    private readonly TimeProvider _timeProvider;
    private readonly Timer _timer;
    private readonly HttpClient _httpClient;

    [ObservableProperty]
    public partial string CurrentDate { get; set; } = "---";

    [ObservableProperty]
    public partial string CurrentTime { get; set; } = "---";

    [ObservableProperty]
    public partial string RealTime { get; set; } = DateTime.Now.ToString("G");

    [ObservableProperty]
    public partial double Scale { get; set; } = 1.0;

    [ObservableProperty]
    public partial DateTime SelectedDate { get; set; } = DateTime.Now;

    [ObservableProperty]
    public partial TimeSpan SelectedTime { get; set; } = DateTime.Now.TimeOfDay;

    public MainViewModel(TimeProvider timeProvider, HttpClient httpClient)
    {
        _timeProvider = timeProvider;
        _httpClient = httpClient;
        _timer = new Timer(UpdateDateTime, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(100));
    }

    [RelayCommand]
    private void SetScale(double value)
    {
        Scale = value;
    }

    [RelayCommand]
    private void SpawnMainWindow()
    {
        var mainWindow = App.ServiceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private void UpdateDateTime(object? state)
    {
        var now = _timeProvider.GetUtcNow().LocalDateTime;
        CurrentDate = now.ToString("d");
        CurrentTime = now.ToString("T");
        RealTime = TimeProvider.System.GetUtcNow().LocalDateTime.ToString("G");
    }

    async partial void OnScaleChanged(double value)
    {
        try
        {
            Scale = value switch
            {
                < 0.1 => 0.1,
                > 256 => 256,
                _ => Scale
            };

            try
            {
                await _httpClient.PostAsJsonAsync("http://localhost:5000/time/scale", Scale);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating scale: {ex.Message}");
            }
        }
        catch (Exception)
        {
            // ignored
        }
    }

    partial void OnSelectedTimeChanged(TimeSpan value)
    {
        UpdateTime();
    }

    partial void OnSelectedDateChanged(DateTime value)
    {
        UpdateTime();
    }

    private async void UpdateTime()
    {
        try
        {
            var combined = SelectedDate.Date + SelectedTime;
            var dateTimeOffset = new DateTimeOffset(combined);
            await _httpClient.PostAsJsonAsync("http://localhost:5000/time", dateTimeOffset);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating time: {ex.Message}");
        }
    }

    public void Dispose()
    {
        _timer.Dispose();
    }
}
