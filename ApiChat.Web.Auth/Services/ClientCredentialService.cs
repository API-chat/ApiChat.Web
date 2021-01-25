using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiChat.Web.Auth.Services
{
    public class ClientCredentialService : IClientCredentialService
    {
        private const string ScopeApiManagement = "https://management.azure.com/.default";

        private readonly string _tenantId;
        private readonly string _clientId;
        private readonly string _clientSecret;

        public ClientCredentialService(IConfiguration configuration)
        {
            _tenantId = configuration["Authorization:TenantId"];
            _clientId = configuration["Authorization:ClientId"];
            _clientSecret = configuration["Authorization:ClientSecret"];
        }

        public async Task<AccessToken> GetAccessTokenAsync()
        {
            var credential = new ClientSecretCredential(_tenantId, _clientId, _clientSecret);
            return await credential.GetTokenAsync(new TokenRequestContext(new[] { ScopeApiManagement }));
        }
    }

    public interface IClientCredentialService
    {
        Task<AccessToken> GetAccessTokenAsync();
    }
}
