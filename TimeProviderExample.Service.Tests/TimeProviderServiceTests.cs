using System;
using System.Threading;
using System.Threading.Tasks;
using TimeProviderExample.Service.Services;
using Xunit;

namespace TimeProviderExample.Service.Tests;

public class TimeProviderServiceTests
{
    [Fact]
    public void Constructor_Default_SetsScaleToOneAndCurrentTime()
    {
        // Arrange & Act
        var service = new TimeProviderService();
        var now = DateTimeOffset.UtcNow;
        var providerNow = service.GetUtcNow();

        // Assert
        Assert.True(Math.Abs((providerNow - now).TotalSeconds) < 1.0);
    }

    [Fact]
    public void Constructor_CustomValues_SetsStartTimeAndScale()
    {
        // Arrange
        var startTime = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        double scale = 10.5;

        // Act
        var service = new TimeProviderService(startTime, scale);
        var providerNow = service.GetUtcNow();

        // Assert
        Assert.True(Math.Abs((providerNow - startTime).TotalSeconds) < 0.1);
    }

    [Fact]
    public async Task SetScale_ChangesTimeSpeed()
    {
        // Arrange
        var service = new TimeProviderService(DateTimeOffset.UtcNow, 10);
        var t1_scaled = service.GetUtcNow();
        
        // Act
        await Task.Delay(1000); // 1 real second
        var t2_scaled = service.GetUtcNow();
        var elapsed_scaled = (t2_scaled - t1_scaled).TotalSeconds;

        // Assert
        // With scale 10, 1 real second should be approx 10 scaled seconds
        Assert.InRange(elapsed_scaled, 8.0, 12.0);
    }

    [Fact]
    public void SetScale_SmoothTransition_NoJumps()
    {
        // Arrange
        var service = new TimeProviderService(DateTimeOffset.UtcNow, 10);
        
        // Act
        var before = service.GetUtcNow();
        service.SetScale(2);
        var after = service.GetUtcNow();
        
        // Assert
        var jump = (after - before).TotalMilliseconds;
        Assert.True(Math.Abs(jump) < 100);
    }

    [Fact]
    public void SetStartTime_UpdatesCurrentTime()
    {
        // Arrange
        var service = new TimeProviderService();
        var newStart = new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero);

        // Act
        service.SetStartTime(newStart);
        var providerNow = service.GetUtcNow();

        // Assert
        Assert.True(Math.Abs((providerNow - newStart).TotalSeconds) < 0.1);
    }

    [Fact]
    public async Task OnTickSeconds_FiresAtCorrectFrequency()
    {
        // Arrange
        double scale = 5.0;
        var service = new TimeProviderService(DateTimeOffset.UtcNow, scale);
        int ticks = 0;
        service.OnTickSeconds += _ => ticks++;

        // Act
        await Task.Delay(2000); // 2 real seconds * 5 scale = 10 scaled seconds -> ~10 ticks

        // Assert
        Assert.InRange(ticks, 7, 13);
    }

    [Fact]
    public void CreateTimer_ScalesDueTimeAndPeriod()
    {
        // Arrange
        double scale = 2.0;
        var service = new TimeProviderService(DateTimeOffset.UtcNow, scale);
        var provider = service.GetTimeProvider();
        int callCount = 0;
        var tcs = new TaskCompletionSource<bool>();

        // Act
        using var timer = provider.CreateTimer(_ => 
        {
            Interlocked.Increment(ref callCount);
            if (callCount >= 2) tcs.TrySetResult(true);
        }, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));

        // 1 scaled second / 2 scale = 0.5 real seconds.
        // 2 ticks = 0.5 (due) + 0.5 (period) = 1.0 real second total.
        bool completed = tcs.Task.Wait(TimeSpan.FromSeconds(3));

        // Assert
        Assert.True(completed, "Timer did not fire enough times in expected real time.");
        Assert.True(callCount >= 2);
    }

    [Fact]
    public void GetTimestamp_IsConsistentWithUtcNow()
    {
        // Arrange
        var service = new TimeProviderService(null, 10);
        var provider = service.GetTimeProvider();

        // Act
        var utcNow = provider.GetUtcNow();
        var timestamp = provider.GetTimestamp();
        var freq = provider.TimestampFrequency;

        // Assert
        var timestampInSeconds = (double)timestamp / freq;
        var utcNowInSeconds = (double)utcNow.UtcTicks / TimeSpan.TicksPerSecond;
        
        Assert.Equal(utcNowInSeconds, timestampInSeconds, 1);
    }

    [Fact]
    public async Task SetScale_Fractional_WorksCorrectly()
    {
        // Arrange
        double scale = 0.5;
        var service = new TimeProviderService(DateTimeOffset.UtcNow, scale);
        var t1_scaled = service.GetUtcNow();
        
        // Act
        await Task.Delay(2000); // 2 real seconds
        var t2_scaled = service.GetUtcNow();
        var elapsed_scaled = (t2_scaled - t1_scaled).TotalSeconds;

        // Assert
        // With scale 0.5, 2 real seconds should be approx 1 scaled second
        Assert.InRange(elapsed_scaled, 0.8, 1.2);
    }
}
