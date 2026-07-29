using System.Net;
using System.Net.Sockets;

namespace TimeProviderExample.Wpf;

public class UdpTimeProvider : TimeProvider, IDisposable
{
    private readonly UdpClient _udpClient = new();
    private long _lastTicks = DateTimeOffset.UtcNow.UtcTicks;
    private long _packetCount = 0;
    private readonly CancellationTokenSource _cts = new();

    public long PacketCount => Interlocked.Read(ref _packetCount);

    public UdpTimeProvider(string multicastAddress, int port)
    {
        _udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        _udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, port));
        _udpClient.JoinMulticastGroup(IPAddress.Parse(multicastAddress));

        Task.Run(ListenAsync, _cts.Token);
    }

    private async Task ListenAsync()
    {
        try
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                var result = await _udpClient.ReceiveAsync(_cts.Token);
                if (result.Buffer.Length != sizeof(long)) continue;
                var ticks = BitConverter.ToInt64(result.Buffer, 0);
                Interlocked.Exchange(ref _lastTicks, ticks);
                Interlocked.Increment(ref _packetCount);
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            // Log or handle appropriately
            Console.WriteLine($"UDP Error: {ex.Message}");
        }
    }

    public override DateTimeOffset GetUtcNow()
    {
        return new DateTimeOffset(Interlocked.Read(ref _lastTicks), TimeSpan.Zero);
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
        _udpClient.Dispose();
    }
}
