using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace TokenServiceClient.Native
{
    public class AuthenticatedHttpHandler: DelegatingHandler
    {
        private readonly CapWebTokenHolder tokenHolder;
        
        public AuthenticatedHttpHandler(CapWebTokenHolder tokenHolder, HttpMessageHandler? innerHandler = null):
            base(innerHandler ?? new HttpClientHandler())
        {
            this.tokenHolder = tokenHolder;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, 
            CancellationToken cancellationToken)
        {
            if (AlmostExpired())
            {
                await tokenHolder.LoginAsync();
            }
            SetAuthenticationHeader(request);
            return await base.SendAsync(request, cancellationToken);
        }

        private void SetAuthenticationHeader(HttpRequestMessage request) => 
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenHolder.AccessToken);

        private bool AlmostExpired() => tokenHolder.ExpiresAt - DateTime.Now < TimeSpan.FromSeconds(30);
    }
}