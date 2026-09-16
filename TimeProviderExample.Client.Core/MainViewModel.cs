using System.Net.Http;
using System.Net.Http.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace TimeProviderExample.Client.Core;

public partial class MainViewModel : ObservableObject, IDisposable
{
    private readonly TimeProvider _timeProvider;
    private readonly Timer _timer;
    private readonly HttpClient _httpClient;
    private readonly IMainWindowFactory _windowFactory;
    private readonly IUiDispatcher _dispatcher;
    private long _lastPacketCount;
    private long _uiRefreshCount;
    private DateTimeOffset _lastFpsTimestamp = DateTimeOffset.UtcNow;
    private DateTimeOffset _lastUiFpsTimestamp = DateTimeOffset.UtcNow;

    [ObservableProperty]
    public partial string CurrentDate { get; set; } = "---";

    [ObservableProperty]
    public partial string CurrentTime { get; set; } = "---";

    [ObservableProperty]
    public partial string RealTime { get; set; } = DateTime.Now.ToString("G");

    [ObservableProperty]
    public partial double Fps { get; set; }

    [ObservableProperty]
    public partial double UiFps { get; set; }

    [ObservableProperty]
    public partial double Scale { get; set; } = 1.0;

    [ObservableProperty]
    public partial DateTime SelectedDate { get; set; } = DateTime.Now;

    [ObservableProperty]
    public partial TimeSpan SelectedTime { get; set; } = DateTime.Now.TimeOfDay;

    [ObservableProperty]
    public partial bool IsSidebarOpen { get; set; }

    public MainViewModel(
        TimeProvider timeProvider,
        HttpClient httpClient,
        IMainWindowFactory windowFactory,
        IUiDispatcher dispatcher)
    {
        _timeProvider = timeProvider;
        _httpClient = httpClient;
        _windowFactory = windowFactory;
        _dispatcher = dispatcher;
        _timer = new Timer(UpdateDateTime, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(10));
    }

    [RelayCommand]
    private void SetScale(double value)
    {
        Scale = value;
    }

    [RelayCommand]
    private void SpawnMainWindow()
    {
        _windowFactory.ShowNewWindow();
    }

    [RelayCommand]
    private void RestartPlay()
    {
        Scale = 1.0;
    }

    private void UpdateDateTime(object? state)
    {
        try
        {
            _dispatcher.Invoke(() =>
            {
                _uiRefreshCount++;
                var now = _timeProvider.GetUtcNow().LocalDateTime;
                CurrentDate = now.ToString("d");
                CurrentTime = now.ToString("T");
                RealTime = TimeProvider.System.GetUtcNow().LocalDateTime.ToString("G");

                var currentTime = DateTimeOffset.UtcNow;

                // Calculate UDP FPS
                if (_timeProvider is UdpTimeProvider udpProvider)
                {
                    var currentCount = udpProvider.PacketCount;
                    var elapsed = currentTime - _lastFpsTimestamp;

                    if (elapsed.TotalSeconds >= 1.0)
                    {
                        Fps = (currentCount - _lastPacketCount) / elapsed.TotalSeconds;
                        _lastPacketCount = currentCount;
                        _lastFpsTimestamp = currentTime;
                    }
                }

                // Calculate UI FPS
                var uiElapsed = currentTime - _lastUiFpsTimestamp;
                if (uiElapsed.TotalSeconds >= 1.0)
                {
                    UiFps = _uiRefreshCount / uiElapsed.TotalSeconds;
                    _uiRefreshCount = 0;
                    _lastUiFpsTimestamp = currentTime;
                }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating date/time: {ex.Message}");
        }
    }

    async partial void OnScaleChanged(double value)
    {
        try
        {
            Scale = value switch
            {
                < -256 => -256,
                > 256 => 256,
                _ => value
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
