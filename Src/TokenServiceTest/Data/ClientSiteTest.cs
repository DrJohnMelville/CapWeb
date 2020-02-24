using System.Linq;
using IdentityServer4.Models;
using TokenService.Data.ClientData;
using Xunit;

namespace TokenServiceTest.Data
{
    public class ClientSiteTest
    {
        private readonly ClientSite sut = new ClientSite
        {
            FriendlyName = "ClientApplication",
            ShortName = "CApp",
            ClientSecret = "The Secret",
            BaseUri = "https://www.Capp.Example.Com"
        };

        [Fact]
        public void ApiResourceTest()
        {
            var res = sut.ApiResource().First();
            Assert.Equal("apiCApp", res.Name);
            Assert.Equal("ClientApplication", res.DisplayName);
        }

        [Fact]
        public void Clients()
        {
            var clients = sut.Clients();
            Assert.Single(clients);
            Assert.True(clients.All(i=>i!=null));
            
        }

        [Fact]
        public void WebClientVerify()
        {
            var client = sut.Clients()[0];
            Assert.Equal("webCApp", client.ClientId);
            Assert.Equal("ClientApplication", client.ClientName);
            Assert.Equal(GrantTypes.CodeAndClientCredentials, client.AllowedGrantTypes);
            Assert.True(client.RequireClientSecret);
            Assert.Equal(new[]{"The Secret".Sha256()}, client.ClientSecrets.Select(i=>i.Value));
            Assert.Equal(new []{"https://www.Capp.Example.Com/signin-oidc", "http://127.0.0.1"}, client.RedirectUris);
            Assert.Equal("https://www.Capp.Example.Com/signout-oidc", client.FrontChannelLogoutUri);
            Assert.Equal("https://www.Capp.Example.Com/signout-callback-oidc", client.PostLogoutRedirectUris.First());
            Assert.Equal(new[] { "openid", "profile", "apiCApp", "offline_access" }, client.AllowedScopes);
        }

        [Fact]
        public void MultipleRedirectExtensions()
        {
            sut.RedirectExtenstions = "ex1|ex2/ex3";
            Assert.Equal(new[]
                {
                    "https://www.Capp.Example.Com/ex1", "https://www.Capp.Example.Com/ex2/ex3",
                    "http://127.0.0.1"
                },
                sut.Clients()[0].RedirectUris);
        }
    }
}