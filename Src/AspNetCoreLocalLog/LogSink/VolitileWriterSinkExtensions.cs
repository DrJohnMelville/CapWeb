using System;
using AspNetCoreLocalLog.LoggingMiddleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Sinks.SystemConsole.Themes;

namespace AspNetCoreLocalLog.LogSink
{
  public static class VolitileWriterSinkExtensions
  {
    // public static LoggerConfiguration VolitileSink(this LoggerSinkConfiguration lsc,
    //   ICircularMemorySink target) =>
    //   lsc.Sink(new VolitileSerilogSink(target));

    public static void AddLogRetrieval(this IServiceCollection servicies)
    {
      servicies.AddSingleton<VolitileSerilogSink>();
      servicies.AddSingleton<ICircularMemorySink, CircularMemorySink>();
      servicies.AddSingleton<LogRetrievalEndpoint>();
      servicies.AddSingleton<IRetrieveLog, RetrieveLog>();
      
    }
    public static IConfigureLogRetrieval UseLogRetrieval(this IApplicationBuilder builder, 
      Action<LoggerConfiguration>? configureLogger = null)
    {
      SetupLogger(configureLogger, builder.ApplicationServices.GetService<VolitileSerilogSink>());
      return AddLogRetrievaliddleware(builder);
    }

    private static void SetupLogger(Action<LoggerConfiguration>? configureLogger, VolitileSerilogSink sink)
    {
      var factory = new LoggerConfiguration();
      configureLogger?.Invoke(factory);
      Log.Logger = factory
        .WriteTo.Console(
          outputTemplate:
          "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}",
          theme: AnsiConsoleTheme.Literate)
        .WriteTo.Sink(sink)
        .CreateLogger();
    }

    private static IConfigureLogRetrieval AddLogRetrievaliddleware(IApplicationBuilder builder)
    {
      var ret = builder.ApplicationServices.GetService<LogRetrievalEndpoint>();
      builder.Use(ret.Process);
      return ret;
    }
  }
}