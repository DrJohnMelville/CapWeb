using AspNetCoreLocalLog.LoggingMiddleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Configuration;

namespace AspNetCoreLocalLog.LogSink
{
  public static class VolitileWriterSinkExtensions
  {
    public static LoggerConfiguration VolitileSink(this LoggerSinkConfiguration lsc,
      ICircularMemorySink target) =>
      lsc.Sink(new VolitileSerilogSink(target));

    public static IConfigureLogRetrieval UseLogRetrieval(this IApplicationBuilder builder)
    {
      var ret = builder.ApplicationServices.GetService<LogRetrievalEndpoint>();
      builder.Use(ret.Process);
      return ret;
    }
  }
}