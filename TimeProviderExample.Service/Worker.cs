using System.Net.Sockets;
using TimeProviderExample.Service.Services;

namespace TimeProviderExample.Service;

public class Worker(
    ILogger<Worker> logger,
    TimeProviderService timeProviderService,
    UdpClientService udpClientService
    ) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var lastLogTime = DateTime.MinValue;

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = timeProviderService.GetUtcNow();
            udpClientService.Send(now.UtcTicks);

            if (logger.IsEnabled(LogLevel.Debug) && (DateTime.UtcNow - lastLogTime).TotalSeconds >= 1)
            {
                logger.LogDebug("Worker realtime running at: {time}", TimeProvider.System.GetUtcNow());
                logger.LogDebug("Worker simulation running at: {time}", now);
                lastLogTime = DateTime.UtcNow;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(10), TimeProvider.System, stoppingToken);
        }
    }
}