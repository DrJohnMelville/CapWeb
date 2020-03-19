using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using IdentityModel.OidcClient.Browser;

namespace TokenServiceClient.Native
{
  public class SystemBrowser : IBrowser
  {
    private readonly LoopbackHttpListener listener;

    public SystemBrowser(int port = 0)
    {
      listener = new LoopbackHttpListener(port);
    }

    public string RedirectUri => listener.RedirectUri;

    public async Task<BrowserResult> InvokeAsync(BrowserOptions options, CancellationToken cancellationToken = new CancellationToken())
    {
      using (listener)
      {
        OpenBrowser(options.StartUrl);

        try
        {
          var result = await listener.WaitForCallbackAsync();
          if (String.IsNullOrWhiteSpace(result))
          {
            return new BrowserResult { ResultType = BrowserResultType.UnknownError, Error = "Empty response." };
          }

          return new BrowserResult { Response = result, ResultType = BrowserResultType.Success };
        }
        catch (TaskCanceledException ex)
        {
          return new BrowserResult { ResultType = BrowserResultType.Timeout, Error = ex.Message };
        }
        catch (Exception ex)
        {
          return new BrowserResult { ResultType = BrowserResultType.UnknownError, Error = ex.Message };
        }
      }
    }

    public static void OpenBrowser(string url)
    {
      try
      {
        Process.Start( new ProcessStartInfo(url) {UseShellExecute = true});
      }
      catch
      {
        // hack because of this: https://github.com/dotnet/corefx/issues/10361
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
          url = url.Replace("&", "^&");
          Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
          Process.Start("xdg-open", url);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
          Process.Start("open", url);
        }
        else
        {
          throw;
        }
      }
    }
  }
}