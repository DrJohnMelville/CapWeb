using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

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
        if (await handlers.GetHandlerAsync(context, scheme.Name) is IAuthenticationRequestHandler handler && 
            await handler.HandleRequestAsync())
        {
          return true;
        }
      }

      return false;
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