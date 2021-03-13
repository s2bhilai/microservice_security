using GloboTicket.Web.Models;
using GloboTicket.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace GloboTicket.Web
{
    public class Startup
    {
        private readonly IHostEnvironment environment;
        private readonly IConfiguration config;

        public Startup(IConfiguration configuration, IHostEnvironment environment)
        {
            config = configuration;
            this.environment = environment;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpContextAccessor();

            var requireAuthenticatedUserPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();

            var builder = services.AddControllersWithViews(options =>
            {
                options.Filters.Add(new AuthorizeFilter(requireAuthenticatedUserPolicy));
            });

            if (environment.IsDevelopment())
                builder.AddRazorRuntimeCompilation();

            services.AddHttpClient<IEventCatalogService, EventCatalogService>(c =>
            {
                c.BaseAddress = new Uri(config["ApiConfigs:EventCatalog:Uri"]);
            });
                
            services.AddHttpClient<IShoppingBasketService, ShoppingBasketService>(c => 
                c.BaseAddress = new Uri(config["ApiConfigs:ShoppingBasket:Uri"]));
            services.AddHttpClient<IOrderService, OrderService>(c =>
                c.BaseAddress = new Uri(config["ApiConfigs:Order:Uri"]));
            services.AddHttpClient<IDiscountService, DiscountService>(c =>
                c.BaseAddress = new Uri(config["ApiConfigs:Discount:Uri"]));

            services.AddSingleton<Settings>();

            //services.AddAccessTokenManagement();

            //Configuring auth code grant flow
            
            //Cookie Auth --> Once an identity token is validated and transformed to get claims identity,claims willbe 
            //stored in an encrypted cookie and will be used in subsequent requests to web app

            //When part of our app require authentication, openIdConnect will be triggered by default 
            // as it's default scheme
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            }).AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme,options =>
            {
                //Middleware by default requests openid,profile scopes

                //Ensures successful result of the authentication will be stored in the cookie
                // matching the scheme so this way authentication middleware links up
                // cookie and openid middleware
                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.Authority = "https://localhost:5010/";
                options.ClientId = "globoticket";
                options.ResponseType = "code"; // Also automaticallly enables PKCE
                options.SaveTokens = true;
                options.ClientSecret = "secret";

                // Ensures claims sub,given name,family name etc related to scopes are fetched from 
                // userinfo endpoint as by default those claims are not present in identity token
                options.GetClaimsFromUserInfoEndpoint = true;

                //With this we will get access token with globoticket as audience and 
                //globoticket.fullaccess as scope
                options.Scope.Add("shoppingbasket.fullaccess");   
                options.Scope.Add("globoticketgateway.fullaccess");
                //options.Scope.Add("eventcatalog.fullaccess"); as we will use token exchange
                options.Scope.Add("offline_access");
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=EventCatalog}/{action=Index}/{id?}");
            });
        }
    }
}
