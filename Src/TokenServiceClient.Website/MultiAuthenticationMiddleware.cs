using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace TokenServiceClient.Website
{
  public class MultiAuthenticationMiddleware
  {
    private readonly RequestDelegate next;
    private readonly IAuthenticationSchemeProvider schemes; 

    public MultiAuthenticationMiddleware(RequestDelegate next, IAuthenticationSchemeProvider schemes)
    {
      this.next = next;
      this.schemes = schemes;
    }


    public async Task Invoke(HttpContext context)
    {
      SetAuthenticationFeatures(context);
      if (await TryHandleAuthenticationSchemeRequest(context)) return;
      await AuthenticateUser(context);
      await next(context);
    }

    private async Task AuthenticateUser(HttpContext context)
    {
      foreach (var scheme in await schemes.GetAllSchemesAsync())
      {
        if (await TryAuthenticationModel(context, scheme)) return;
      }
    }

    private static async Task<bool> TryAuthenticationModel(HttpContext context, AuthenticationScheme scheme)
    {
      var result = await context.AuthenticateAsync(scheme.Name);
      if (result?.Principal != null)
      {
        context.User = result.Principal;
        return true;
      }
      return false;
    }

    /// <summary>
    /// Some authentication schemes need to be able to handle certian webrequests on their own.
    /// CookieAuthentication uses this to handle signout requests for example.
    /// </summary>
    private async Task<bool> TryHandleAuthenticationSchemeRequest(HttpContext context)
    {
      var handlers = context.RequestServices.GetRequiredService<IAuthenticationHandlerProvider>();
      foreach (var scheme in await schemes.GetRequestHandlerSchemesAsync())
      {
        if (await handlers.GetHandlerAsync(context, scheme.Name) is IAuthenticationRequestHandler handler)
        {
          if (await HandlerWithRefreshRetry(handler)) return true;
        }
      }

      return false;
    }

    private static async Task<bool> HandlerWithRefreshRetry(IAuthenticationRequestHandler handler)
    {
      return await HandleRefreshWithKeyRefresh(handler);
    }

    private static async Task<bool> HandleRefreshWithKeyRefresh(IAuthenticationRequestHandler handler)
    {
      try
      {
        return await handler.HandleRequestAsync();
      }
      catch (Exception e)
      {
        if (IsStaleKeyException(e) && handler is OpenIdConnectHandler openIdHandler)
        {
          RefreshStaleKeys(openIdHandler);
          // we only try one refresh which is why this is not a recursive call
          return await handler.HandleRequestAsync();
        }
        // otherwise
        throw;
      }
    }

    private static bool IsStaleKeyException(Exception e)
    {
      return e.Message.StartsWith("IDC10501");
    }

    private static void RefreshStaleKeys(OpenIdConnectHandler openIdHandler)
    {
      if (openIdHandler.Options.ConfigurationManager is ConfigurationManager<OpenIdConnectConfiguration> config)
      {
        config.RefreshInterval = TimeSpan.FromSeconds(1);
      }
      openIdHandler.Options.ConfigurationManager.RequestRefresh();
    }

    private void SetAuthenticationFeatures(HttpContext context)
    {
      context.Features.Set<IAuthenticationFeature>(new AuthenticationFeature
      {
        OriginalPath = context.Request.Path,
        OriginalPathBase = context.Request.PathBase
      });
    }
  }
}