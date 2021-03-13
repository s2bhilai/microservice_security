using GloboTicket.Services.ShoppingBasket.Extensions;
using GloboTicket.Services.ShoppingBasket.Models;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace GloboTicket.Services.ShoppingBasket.Services
{
    public class DiscountService: IDiscountService
    {
        private readonly HttpClient client;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private string _accessToken;

        public DiscountService(HttpClient client, IHttpContextAccessor httpContextAccessor)
        {
            this.client = client;
            _httpContextAccessor = httpContextAccessor;
            //this.client.DefaultRequestHeaders.Add("Accept", "...");
            //this.client.BaseAddress = new Uri("...");
        }

        private async Task<string> GetToken()
        {
            if(!string.IsNullOrWhiteSpace(_accessToken))
            {
                return _accessToken;
            }

            var discoveryDocumentResponse = await client.GetDiscoveryDocumentAsync("https://localhost:5010/");

            if(discoveryDocumentResponse.IsError)
            {
                throw new Exception(discoveryDocumentResponse.Error);
            }

            var customParams = new Dictionary<string, string>
            {
                { "subject_token_type","urn:ietf:params:oauth:token-type:access_token" },
                { "subject_token", await _httpContextAccessor.HttpContext.GetTokenAsync("access_token") },
                { "scope", "openid profile discount.fullaccess" }
            };

            var tokenResponse = await client.RequestTokenAsync(new TokenRequest()
            {
                Address = discoveryDocumentResponse.TokenEndpoint,
                GrantType = "urn:ietf:params:oauth:grant-type:token-exchange",
                Parameters = customParams,
                ClientId = "shoppingbaskettodownstreamtokenexchangeclient",
                ClientSecret = "secret"
            });

            if(tokenResponse.IsError)
            {
                throw new Exception(tokenResponse.Error);
            }

            _accessToken = tokenResponse.AccessToken;
            return _accessToken;
        }

        public async Task<Coupon> GetCoupon(Guid couponId)
        {
            client.SetBearerToken(await GetToken());
            var response = await client.GetAsync($"/api/discount/{couponId}");
            return await response.ReadContentAs<Coupon>();
        }

        public async Task<Coupon> GetCouponWithError(Guid couponId)
        {
            var response = await client.GetAsync($"/api/discount/error/{couponId}");
            return await response.ReadContentAs<Coupon>();
        }
    }
}
