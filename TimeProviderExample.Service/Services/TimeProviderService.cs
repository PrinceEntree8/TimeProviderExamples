using System;
using System.Threading;

namespace TimeProviderExample.Service.Services;

/// <summary>
/// This service provides time-related functionality.
/// Creates an internal time provider with scale functionalities and event OnTickSeconds that provides a tick event every second (scaled).
/// </summary>
public class TimeProviderService
{
    /// <summary>
    /// This class represents an internal time provider with scale functionalities and event OnTickSeconds that provides a tick event every second (scaled).
    /// </summary>
    private class InternalTimeProvider : System.TimeProvider
    {
        private DateTimeOffset _initialSystemTime;
        private DateTimeOffset _startTime;
        private double _accumulatedScaledTicks;
        private double _scale = 1.0;

        public event Action? ScaleChanged;

        public double Scale
        {
            get => _scale;
            set
            {
                if (Math.Abs(_scale - value) < double.Epsilon) return;
                UpdateAccumulatedTime();
                _scale = value;
                ScaleChanged?.Invoke();
            }
        }

        public InternalTimeProvider(DateTimeOffset? startTime = null)
        {
            _initialSystemTime = base.GetUtcNow();
            _startTime = startTime ?? _initialSystemTime;
            _accumulatedScaledTicks = 0;
        }

        public void SetStartTime(DateTimeOffset startTime)
        {
            _initialSystemTime = base.GetUtcNow();
            _startTime = startTime;
            _accumulatedScaledTicks = 0;
            ScaleChanged?.Invoke(); // Resetting start time should also reset timers
        }

        private void UpdateAccumulatedTime()
        {
            var systemNow = base.GetUtcNow();
            var elapsed = systemNow - _initialSystemTime;
            _accumulatedScaledTicks += elapsed.Ticks * _scale;
            _initialSystemTime = systemNow;
        }

        public override DateTimeOffset GetUtcNow()
        {
            var systemNow = base.GetUtcNow();
            var elapsed = systemNow - _initialSystemTime;
            var currentScaledTicks = _accumulatedScaledTicks + (elapsed.Ticks * _scale);
            return _startTime.AddTicks((long)currentScaledTicks);
        }

        public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
        {
            // Scale the dueTime and period
            var scaledDueTime = Math.Abs(_scale) < double.Epsilon ? Timeout.InfiniteTimeSpan : dueTime / _scale;
            var scaledPeriod = Math.Abs(_scale) < double.Epsilon ? Timeout.InfiniteTimeSpan : period / _scale;
            
            return base.CreateTimer(callback, state, scaledDueTime, scaledPeriod);
        }

        public override long GetTimestamp()
        {
            return GetUtcNow().UtcTicks;
        }

        public override long TimestampFrequency => TimeSpan.TicksPerSecond;
    }

    private readonly InternalTimeProvider _timeProvider;
    private ITimer? _tickTimer;

    public event Action<DateTimeOffset>? OnTickSeconds;

    public double Scale
    {
        get => _timeProvider.Scale;
        set => _timeProvider.Scale = value;
    }

    public TimeProviderService(DateTimeOffset? startTime = null, double scale = 1.0)
    {
        _timeProvider = new InternalTimeProvider(startTime) { Scale = scale };
        _timeProvider.ScaleChanged += RecreateTickTimer;
        
        RecreateTickTimer();
    }

    private void RecreateTickTimer()
    {
        _tickTimer?.Dispose();
        
        if (_timeProvider.Scale > 0)
        {
            _tickTimer = _timeProvider.CreateTimer(_ => 
            {
                OnTickSeconds?.Invoke(_timeProvider.GetUtcNow());
            }, null, TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(1.0));
        }
        else
        {
            _tickTimer = null;
        }
    }

    public TimeProvider GetTimeProvider() => _timeProvider;

    public DateTimeOffset GetUtcNow() => _timeProvider.GetUtcNow();

    public void SetScale(double scale)
    {
        _timeProvider.Scale = scale;
    }

    public void SetStartTime(DateTimeOffset startTime)
    {
        _timeProvider.SetStartTime(startTime);
    }
}