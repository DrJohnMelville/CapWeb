using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace AspNetCoreLocalLog.LoggingMiddleware;

public interface IConfigureLogRetrieval
{
    IConfigureLogRetrieval WithSecret(string secret);
}

public partial class LogRetrievalEndpoint : IConfigureLogRetrieval
{
    private readonly IRetrieveLog logSource;

    public LogRetrievalEndpoint(IRetrieveLog logSource)
    {
        this.logSource = logSource;
    }

    public async Task Process(HttpContext context, Func<Task> next)
    {
        if (!await logSource.TryLogCommand(ParseCommand(context.Request.Path),
                new ResponseWrapper(context.Response)))
        {
            await next();
        }
    }

    [GeneratedRegex(@"^/QuickLog/([^/]+)/(.*)$")]
    private partial Regex LogUrlFinder();

    private string ParseCommand(string pathValue)
    {
        var match1 = LogUrlFinder().Match(pathValue);
        return match1 is { Success: true } match &&
               match.Groups[1].Value.Equals(secret, StringComparison.Ordinal)
            ? match.Groups[2].Value
            : "Not a Log Command";
    }

    private string secret = "DefaultSecret";

    public IConfigureLogRetrieval WithSecret(string secret)
    {
        this.secret = secret;
        return this;
    }
}