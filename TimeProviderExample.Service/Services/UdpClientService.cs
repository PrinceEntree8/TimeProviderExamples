using System.Net;
using System.Net.Sockets;

namespace TimeProviderExample.Service.Services;

public class UdpClientService
{
    private readonly UdpClient _udpClient = new();
    private readonly IPEndPoint _remoteEndPoint = new(IPAddress.Parse("224.0.0.1"), 6000);

    public void Send(long value)
    {
        var data = BitConverter.GetBytes(value);
        _udpClient.Send(data, data.Length, _remoteEndPoint);
    }
}