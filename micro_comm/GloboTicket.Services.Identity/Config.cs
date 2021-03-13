// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Models;
using System.Collections.Generic;

namespace GloboTicket.Services.Identity
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
            new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
            };

        public static IEnumerable<ApiResource> ApiResources =>
            new ApiResource[]
            {
                //Audience in access token - shoppingbasket
                new ApiResource("shoppingbasket","GloboTicket APIs")
                {
                    Scopes = { "shoppingbasket.fullaccess" }
                },
                new ApiResource("eventcatalog","Event Catalog API")
                {
                    Scopes = { "eventcatalog.read", "eventcatalog.write", "eventcatalog.fullaccess" }
                },
                new ApiResource("discount","Discount API")
                {
                    Scopes = { "discount.fullaccess" }
                },
                new ApiResource("globoticketgateway","GloboTicket Gateway")
                {
                    Scopes = { "globoticketgateway.fullaccess" }
                }
            };

        public static IEnumerable<ApiScope> ApiScopes =>
            new ApiScope[]
            {
                new ApiScope("shoppingbasket.fullaccess"),
                new ApiScope("eventcatalog.read"),
                new ApiScope("eventcatalog.write"),
                new ApiScope("eventcatalog.fullaccess"),
                new ApiScope("discount.fullaccess"),
                new ApiScope("globoticketgateway.fullaccess")
            };

        public static IEnumerable<Client> Clients =>
            new Client[]
            {
                //Client Creds Flow
                new Client
                {
                    //Client Credentials Flow
                    ClientName = "GloboTicket Machine 2 Machine Client",
                    ClientId = "globoticketm2m",
                    ClientSecrets = { new Secret("secret".Sha256())},
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    AllowedScopes = { "eventcatalog.fullaccess" }
                },
                //Client with Auth Code PKCE
                new Client
                {
                    //Client Credentials Flow
                    ClientName = "GloboTicket Interactive Client",
                    ClientId = "globoticketinteractive",
                    ClientSecrets = { new Secret("secret".Sha256())},
                    AllowedGrantTypes = GrantTypes.Code,
                    RedirectUris = { "https://localhost:44336/signin-oidc" },
                    PostLogoutRedirectUris = { "https://localhost:44336/signout-callback-oidc" },
                    AllowedScopes = { "openid","profile", "shoppingbasket.fullaccess" }
                },
                //Combining the config for Cliet Creds and Auth Code
                 new Client
                {
                    ClientName = "GloboTicket Client",
                    ClientId = "globoticket",
                    ClientSecrets = { new Secret("secret".Sha256())},
                    AllowedGrantTypes = GrantTypes.CodeAndClientCredentials,
                    RedirectUris = { "https://localhost:44336/signin-oidc" },
                    PostLogoutRedirectUris = { "https://localhost:44336/signout-callback-oidc" },
                    AllowOfflineAccess = true,
                    AccessTokenLifetime = 60,
                    AllowedScopes = { 
                         "openid",
                         "profile", 
                         "shoppingbasket.fullaccess",
                         "eventcatalog.read",
                         "eventcatalog.write",
                         //"eventcatalog.fullaccess",
                         "globoticketgateway.fullaccess"
                     }
                },
                 new Client
                {
                    //Token Exchange Flow
                    ClientName = "Shopping Basket Token Exchange Client",
                    ClientId = "shoppingbaskettodownstreamtokenexchangeclient",
                    ClientSecrets = { new Secret("secret".Sha256())},
                    AllowedGrantTypes = new[] { "urn:ietf:params:oauth:grant-type:token-exchange" },
                    AllowedScopes = { "openid", "profile", "discount.fullaccess" }
                },
                 new Client
                {
                    //Token Exchange Flow
                    ClientName = "Gateway to Downstream Token Exchange Client",
                    ClientId = "gatewaytodownstreamtokenexchange",
                    ClientSecrets = { new Secret("secret".Sha256())},
                    RequireConsent = false,
                    AllowedGrantTypes = new[] { "urn:ietf:params:oauth:grant-type:token-exchange" },
                    AllowedScopes = { "openid", "profile", "eventcatalog.fullaccess" }
                },
                // m2m client credentials flow client
                new Client
                {
                    ClientId = "m2m.client",
                    ClientName = "Client Credentials Client",

                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    ClientSecrets = { new Secret("511536EF-F270-4058-80CA-1C89C192F69A".Sha256()) },

                    AllowedScopes = { "scope1" }
                },

                // interactive client using code flow + pkce
                new Client
                {
                    ClientId = "interactive",
                    ClientSecrets = { new Secret("49C1A7E1-0C79-4A89-A3D6-A37998FB86B0".Sha256()) },
                    
                    AllowedGrantTypes = GrantTypes.Code,

                    RedirectUris = { "https://localhost:44300/signin-oidc" },
                    FrontChannelLogoutUri = "https://localhost:44300/signout-oidc",
                    PostLogoutRedirectUris = { "https://localhost:44300/signout-callback-oidc" },

                    AllowOfflineAccess = true,
                    AllowedScopes = { "openid", "profile", "scope2" }
                },
            };
    }
}