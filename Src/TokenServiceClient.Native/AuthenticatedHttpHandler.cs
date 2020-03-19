using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;
using TokenServiceClient.Native.PersistentToken;

namespace TokenServiceClient.Native
{
    public class AuthenticatedHttpHandler: DelegatingHandler
    {
        private readonly IPersistentAccessToken tokenHolder;
        
        public AuthenticatedHttpHandler(IPersistentAccessToken tokenHolder, HttpMessageHandler? innerHandler = null):
            base(innerHandler ?? new HttpClientHandler())
        {
            this.tokenHolder = tokenHolder;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, 
            CancellationToken cancellationToken)
        {
            SetAuthenticationHeader(request, (await tokenHolder.CurrentAccessToken()).AccessToken);
            return await base.SendAsync(request, cancellationToken);
        }

        private void SetAuthenticationHeader(HttpRequestMessage request, string token) => 
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }
}