using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace TokenServiceClient.Native
{
  public class LoopbackHttpListener : IDisposable
  {
    private readonly TcpListener listener = new TcpListener(IPAddress.Loopback, 0);
    public int Port => ((IPEndPoint) listener.LocalEndpoint).Port;
    public string RedirectUri => $"http://127.0.0.1:{Port}";

        
    public LoopbackHttpListener()
    {
      listener.Start();
    }

    public void Dispose() => listener.Stop();

    public async Task<string?> WaitForCallbackAsync()
    {
      using var httpClientConnection = await listener.AcceptTcpClientAsync();
      await using  var responder = new HttpResponder(httpClientConnection.GetStream());
      return await responder.HandleHttpRequest();
    }
  }
}