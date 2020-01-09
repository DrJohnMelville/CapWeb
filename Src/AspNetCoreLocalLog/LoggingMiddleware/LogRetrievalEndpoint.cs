using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace AspNetCoreLocalLog.LoggingMiddleware
{
  public interface IConfigureLogRetrieval
  {
    IConfigureLogRetrieval WithSecret(string secret);
  }
  public sealed class LogRetrievalEndpoint: IConfigureLogRetrieval
  {
    public Task Process(HttpContext context, Func<Task> next)
    {
      return next();
    }

    public IConfigureLogRetrieval WithSecret(string secret)
    {
      return this;
    }
  }
}