using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using IdentityServer4.Models;

namespace TokenService.Data.ClientData
{
    public class ClientSite
    {
        public string FriendlyName { get; set; } = "";
        public string ShortName { get; set; } = "";
        public string ClientSecret { get; set; } = "";
        public string BaseUri { get; set; } = "";
        public string RedirectExtenstions { get; set; } = "signin-oidc";
        public string FrontChannelLogoutExtension { get; set; } = "signout-oidc";
        public string PostLogoutRedirectExtensions { get; set; } = "signout-callback-oidc";
        public string AllowedScopes { get; set; } = "openid|profile";

        public IEnumerable<ApiResource> ApiResource()
        {
            return new []{new ApiResource($"api{ShortName}", FriendlyName)};
        }

        public Client[] Clients()
        {
            return new Client[]{WebClient(), AppClient()};
        }

        private Client WebClient()
        {    
            return new Client
            {
                ClientId = $"web{ShortName}",
                ClientName = FriendlyName,
                AllowedGrantTypes = GrantTypes.CodeAndClientCredentials,
                ClientSecrets = new[]{new Secret(ClientSecret.Sha256())},
                RequireClientSecret = true,
                RedirectUris = ExpandUriExtensions(RedirectExtenstions),
                FrontChannelLogoutUri = ExpandUriExtension(FrontChannelLogoutExtension),
                PostLogoutRedirectUris = ExpandUriExtensions(PostLogoutRedirectExtensions),
                AllowedScopes = BuildScopes()
            };
        }

        private ICollection<string> BuildScopes()
        {
            var ret = new HashSet<string>();
            foreach (var scope in AllowedScopes.Split('|'))
            {
                ret.Add(scope);
            }
            ret.Add("apiCapp");
            return ret;
        }

        private string ExpandUriExtension(string extension) => $"{BaseUri}/{extension}";

        public IList<string> ExpandUriExtensions(string extensions) =>
            extensions.Split('|', StringSplitOptions.RemoveEmptyEntries)
                .Select(i => $"{BaseUri}/{i}")
                .ToList();

        private Client AppClient()
        {
            return new Client
            {
                ClientId = $"app{ShortName}",
                ClientName = FriendlyName,
                ClientUri = BaseUri,
                AllowedGrantTypes = GrantTypes.Code,
                ClientSecrets = new[]{new Secret(ClientSecret.Sha256())},
                RequirePkce = true,
                RequireClientSecret = false,
                RedirectUris = ExpandUriExtensions(RedirectExtenstions),
                AllowedScopes = BuildScopes()
            };
        }
    }
}