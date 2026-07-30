using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace TimeProviderExample.Service.Services;

public class UdpClientService
{
    private readonly UdpClient _udpClient = new();
    private readonly IPEndPoint _remoteEndPoint = new(IPAddress.Parse("239.0.0.1"), 6000);

    public UdpClientService()
    {
        // if device has multiple network interfaces, select the one with the correct IP address
        /*
         IPAddress localInterfaceIp = IPAddress.Parse("192.168.1.3");
        _udpClient.Client.SetSocketOption(
            SocketOptionLevel.IP,
            SocketOptionName.MulticastInterface,
            localInterfaceIp.GetAddressBytes());
        */
    }

    public TimeSpan Send(long value)
    {
        var data = BitConverter.GetBytes(value);
        var startTime = Stopwatch.GetTimestamp();
        _udpClient.Send(data, data.Length, _remoteEndPoint);
        return Stopwatch.GetElapsedTime(startTime);
    }
}