using System.Net.Sockets;
using TimeProviderExample.Service.Services;

namespace TimeProviderExample.Service;

public partial class Worker(
    ILogger<Worker> logger,
    TimeProviderService timeProviderService,
    UdpClientService udpClientService
    ) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var lastLogTime = DateTime.MinValue;
        var lastMetricLogTime = DateTime.UtcNow;
        var packetCount = 0;
        var totalSendTime = TimeSpan.Zero;

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = timeProviderService.GetUtcNow();
            var sendDuration = udpClientService.Send(now.UtcTicks);
            packetCount++;
            totalSendTime += sendDuration;

            if ((DateTime.UtcNow - lastLogTime).TotalSeconds >= 1)
            {
                LogWorkerRealtimeRunningAtTime(TimeProvider.System.GetUtcNow());
                LogWorkerSimulationRunningAtTime(now);
                lastLogTime = DateTime.UtcNow;
            }

            if ((DateTime.UtcNow - lastMetricLogTime).TotalSeconds >= 10)
            {
                var avgPackets = packetCount / 10.0;
                var avgSendTime = packetCount > 0 ? totalSendTime.TotalMilliseconds / packetCount : 0;
                LogAveragePacketsSentPerSecondAvgpacketsAverageSendTimeAvgsendtimeN2Ms(avgPackets, avgSendTime);

                packetCount = 0;
                totalSendTime = TimeSpan.Zero;
                lastMetricLogTime = DateTime.UtcNow;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(10), TimeProvider.System, stoppingToken);
        }
    }

    [LoggerMessage(LogLevel.Debug, "Worker realtime running at: {time}")]
    partial void LogWorkerRealtimeRunningAtTime(DateTimeOffset time);

    [LoggerMessage(LogLevel.Debug, "Worker simulation running at: {time}")]
    partial void LogWorkerSimulationRunningAtTime(DateTimeOffset time);

    [LoggerMessage(LogLevel.Information, "Average packets sent (per second): {avgPackets}, Average send time: {avgSendTime:N2}ms")]
    partial void LogAveragePacketsSentPerSecondAvgpacketsAverageSendTimeAvgsendtimeN2Ms(double avgPackets, double avgSendTime);
}