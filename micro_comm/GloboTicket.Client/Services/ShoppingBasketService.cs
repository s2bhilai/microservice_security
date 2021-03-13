﻿using GloboTicket.Web.Extensions;
using GloboTicket.Web.Models;
using GloboTicket.Web.Models.Api;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace GloboTicket.Web.Services
{
    public class ShoppingBasketService : IShoppingBasketService
    {
        private readonly HttpClient client;
        private readonly Settings settings;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ShoppingBasketService(HttpClient client, IHttpContextAccessor httpContextAccessor)
        {
            this.client = client;
            this._httpContextAccessor = httpContextAccessor;
        }

        public async Task<BasketLine> AddToBasket(Guid basketId, BasketLineForCreation basketLine)
        {
            //Since we have added savetokens = true, Tokens are saved into authentication cookie
            // headers, which ensures it's available on each request 
            //so the below stmt is able to get acces token
            var token = await _httpContextAccessor.HttpContext.GetTokenAsync("access_token");

            if (basketId == Guid.Empty)
            {
                client.SetBearerToken(token);
                Guid userid = Guid.Parse(_httpContextAccessor.HttpContext
                    .User.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value);

                var basketResponse = await client.PostAsJson("api/baskets", new BasketForCreation { UserId = userid });
                var basket = await basketResponse.ReadContentAs<Basket>();
                basketId = basket.BasketId;
            }

            client.SetBearerToken(token);
            var response = await client.PostAsJson($"api/baskets/{basketId}/basketlines", basketLine);
            return await response.ReadContentAs<BasketLine>();
        }

        public async Task<Basket> GetBasket(Guid basketId)
        {
            if (basketId == Guid.Empty)
                return null;

            client.SetBearerToken(await _httpContextAccessor.HttpContext.GetTokenAsync("access_token"));
            var response = await client.GetAsync($"api/baskets/{basketId}");
            return await response.ReadContentAs<Basket>();
        }

        public async Task<IEnumerable<BasketLine>> GetLinesForBasket(Guid basketId)
        {
            if (basketId == Guid.Empty)
                return new BasketLine[0];
            client.SetBearerToken(await _httpContextAccessor.HttpContext.GetTokenAsync("access_token"));
            var response = await client.GetAsync($"api/baskets/{basketId}/basketLines");
            return await response.ReadContentAs<BasketLine[]>();

        }

        public async Task UpdateLine(Guid basketId, BasketLineForUpdate basketLineForUpdate)
        {
            client.SetBearerToken(await _httpContextAccessor.HttpContext.GetTokenAsync("access_token"));
            await client.PutAsJson($"api/baskets/{basketId}/basketLines/{basketLineForUpdate.LineId}", basketLineForUpdate);
        }

        public async Task RemoveLine(Guid basketId, Guid lineId)
        {
            client.SetBearerToken(await _httpContextAccessor.HttpContext.GetTokenAsync("access_token"));
            await client.DeleteAsync($"api/baskets/{basketId}/basketLines/{lineId}");
        }

        public async Task ApplyCouponToBasket(Guid basketId, CouponForUpdate couponForUpdate)
        {
            client.SetBearerToken(await _httpContextAccessor.HttpContext.GetTokenAsync("access_token"));
            var response = await client.PutAsJson($"api/baskets/{basketId}/coupon", couponForUpdate);
            //return await response.ReadContentAs<Coupon>();
        }

        public async Task<BasketForCheckout> Checkout(Guid basketId, BasketForCheckout basketForCheckout)
        {
            client.SetBearerToken(await _httpContextAccessor.HttpContext.GetTokenAsync("access_token"));
            var response = await client.PostAsJson($"api/baskets/checkout", basketForCheckout);
            if(response.IsSuccessStatusCode)
                return await response.ReadContentAs<BasketForCheckout>();
            else
            {
                throw new Exception("Something went wrong placing your order. Please try again.");
            }
        }
    }
}
