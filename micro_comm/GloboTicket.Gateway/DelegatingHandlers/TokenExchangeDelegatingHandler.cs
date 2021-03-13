using IdentityModel.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GloboTicket.Gateway.DelegatingHandlers
{
    public class TokenExchangeDelegatingHandler: DelegatingHandler
    {
        private IHttpClientFactory _httpClientFactory;

        public TokenExchangeDelegatingHandler(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            //extract the current token
            var incomingToken = request.Headers.Authorization.Parameter;

            // exchange it
            var newToken = await ExchangeToken(incomingToken);

            // set it as bearer token i.e replace the incoming bearer token with our new one
            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", newToken);

            return await base.SendAsync(request, cancellationToken);
        }

        private async Task<string> ExchangeToken(string incomingToken)
        {
            var client = _httpClientFactory.CreateClient();

            var discoveryDocumentResponse = await client.GetDiscoveryDocumentAsync("https://localhost:5010/");

            if(discoveryDocumentResponse.IsError)
            {
                throw new Exception(discoveryDocumentResponse.Error);
            }

            var customParams = new Dictionary<string, string>
            {
                { "subject_token_type","urn:ietf:params:oauth:token-type:access_token" },
                { "subject_token",incomingToken },
                { "scope", "openid profile eventcatalog.fullaccess"}
            };

            var tokenResponse = await client.RequestTokenAsync(new TokenRequest()
            {
                Address = discoveryDocumentResponse.TokenEndpoint,
                GrantType = "urn:ietf:params:oauth:grant-type:token-exchange",
                Parameters = customParams,
                ClientId = "gatewaytodownstreamtokenexchange",
                ClientSecret = "secret"
            });

            if (tokenResponse.IsError)
                throw new Exception(tokenResponse.Error);

            return tokenResponse.AccessToken;
        }
    }
}
