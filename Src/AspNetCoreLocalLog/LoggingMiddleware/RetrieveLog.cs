using System;
using System.Resources;
using System.Threading.Tasks;
using AspNetCoreLocalLog.LogSink;

namespace AspNetCoreLocalLog.LoggingMiddleware
{
  public interface IRetrieveLog
  {
    public Task<bool> TryLogCommand(string command, IHttpOutput output);
  }
  public class RetrieveLog : IRetrieveLog
  {
    private readonly ICircularMemorySink source;

    public RetrieveLog(ICircularMemorySink source)
    {
      this.source = source;
    }

    public async Task<bool> TryLogCommand(string command, IHttpOutput output)
    {
      switch (command)
      {
        case "html":
          await WriteToHtml(output);
          return true;
      }
      return false;
    }

    private async Task WriteToHtml(IHttpOutput output)
    {
      await WritePageHeader(output);
      await WriteRows(output);
      await WritePageFooter(output);
    }

    private async Task WriteRows(IHttpOutput output)
    {
      foreach (var line in source.All())
      {
        // TODO: write the line to the log  
      }
      source.Clear();
    }

    private static Task WritePageHeader(IHttpOutput output) =>
      output.WriteAsync(
        "<html><head></head><body><h1>Quick Log</h1><table><tr><th>Date</th><th>Event</th></tr>");
    private static Task WritePageFooter(IHttpOutput output) => output.WriteAsync("</table></body></html>");
  }
}