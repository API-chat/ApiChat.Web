using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using Microsoft.Rest;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace ApiChat.Web.Auth.Services
{
    public class ApiManagementTokenProvider : ITokenProvider
    {
        private readonly IConfiguration _config;

        private const string ScopeApiManagement = "https://management.azure.com/.default";

        private readonly string _tenantId;
        private readonly string _clientId;
        private readonly string _clientSecret;

        public ApiManagementTokenProvider(IConfiguration configuration)
        {
            _tenantId = configuration["AzureSdkClient:TenantId"];
            _clientId = configuration["AzureSdkClient:ClientId"];
            _clientSecret = configuration["AzureSdkClient:ClientSecret"];
        }

        public async Task<AuthenticationHeaderValue> GetAuthenticationHeaderAsync(CancellationToken cancellationToken)
        {
            // For app only authentication, we need the specific tenant id in the authority url
            var tenantSpecificUrl = $"https://login.microsoftonline.com/{_tenantId}/";

            // Create a confidential client to authorize the app with the AAD app
            IConfidentialClientApplication clientApp = ConfidentialClientApplicationBuilder
                                                                            .Create(_clientId)
                                                                            .WithClientSecret(_clientSecret)
                                                                            .WithAuthority(tenantSpecificUrl)
                                                                            .Build();
            // Make a client call if Access token is not available in cache
            var authenticationResult = await clientApp
                .AcquireTokenForClient(new List<string> { ScopeApiManagement })
                .ExecuteAsync();

            return new AuthenticationHeaderValue("Bearer", authenticationResult.AccessToken);
        }
    }
}