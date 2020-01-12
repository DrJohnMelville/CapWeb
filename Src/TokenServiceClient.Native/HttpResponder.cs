using System;
using System.IO;
using System.Threading.Tasks;

namespace TokenServiceClient.Native
{
  public class HttpResponder : IAsyncDisposable
  {
    private readonly Stream stream;
    private readonly StreamReader reader;
    private readonly StreamWriter writer;
        
    public HttpResponder(Stream stream)
    {
      this.stream = stream;
      reader = new StreamReader(stream);
      writer = new StreamWriter(stream);
    }

    public async ValueTask DisposeAsync()
    {
      await writer.DisposeAsync();
      reader.Dispose();
      await stream.DisposeAsync();
    }

    public async Task<string> HandleHttpRequest()
    {
      var readToEndAsync = await reader.ReadLineAsync();
      await WriteWebResponse();
      return readToEndAsync;

    }
    private Task WriteWebResponse() => 
      writer.WriteAsync("HTTP/1.0 200 0k\r\n\r\n\r\n<html><head></head><body>Please close this browser window and return to the app.</body></html>");
  }
}