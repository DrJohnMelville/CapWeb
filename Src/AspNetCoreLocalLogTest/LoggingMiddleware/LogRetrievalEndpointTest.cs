using System;
using System.Net.Http;
using System.Threading.Tasks;
using AspNetCoreLocalLog.LoggingMiddleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Sdk;

namespace AspNetCoreLocalLogTest.LoggingMiddleware
{
  public sealed class LogRetrievalEndpointTest
  {
    private int finalCalls = 0;

    private readonly LogRetrievalEndpoint sut;
    private readonly TestServer server;
    private readonly HttpClient client;
    public LogRetrievalEndpointTest()
    {
      sut = new LogRetrievalEndpoint();
      sut.WithSecret("!!Secret");

      var builder = new WebHostBuilder()
        .UseEnvironment("Testing")
        .ConfigureServices(services =>
        {
          
        })
        .Configure(app =>
        {
          app.Use(sut.Process);
          app.Use(DefaultEndpoint);
        });
      
      server = new TestServer(builder);
      client = server.CreateClient();
    }

    private Task DefaultEndpoint(HttpContext context, Func<Task> neverCalled)
    {
      finalCalls++;
      return Task.CompletedTask;
    }

    [Fact]
    public async Task PassThroughTypical()
    {
      await client.GetAsync("/Home/Index");
      Assert.Equal(1, finalCalls);
      
    }

  }
}