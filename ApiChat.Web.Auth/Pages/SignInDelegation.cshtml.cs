using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Management.ApiManagement;
using RestSharp;
using Newtonsoft.Json;
using Microsoft.Azure.Management.ApiManagement.Models;
using Azure.Identity;
using Microsoft.Rest;

namespace ApiChat.Web.Auth.Pages
{
    public class SignInDelegationModel : PageModel
    {
        public const string RequestQueryRedirectUrl = "redirectHostUri";
        private const string ScopeApiManagement = "https://management.azure.com/.default";
        private const string NameIdentifierSchemas = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";
        private const string GivenNameSchemas = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname";
        private const string SurnameSchemas = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname";
        private const string EMailAddress = "emails";
        private const string NewUser = "newUser";

        private readonly string _serviceName;
        private readonly string _ssoUrl;
        private readonly string _tenantId;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _subscriptionId;
        private readonly string _resourceGroupName;

        public SignInDelegationModel(IConfiguration configuration)
        {
            _subscriptionId = configuration["ApiManagement:SubscriptionId"];
            _resourceGroupName = configuration["ApiManagement:ResourceGroupName"];
            _serviceName = configuration["ApiManagement:ServiceName"];
            _ssoUrl = configuration["ApiManagement:SSOUrl"];
            _tenantId = configuration["Authorization:TenantId"];
            _clientId = configuration["Authorization:ClientId"];
            _clientSecret = configuration["Authorization:ClientSecret"];
        }

        public async Task<IActionResult> OnGet()
        {
            var returnUrl = Request.Query[RequestQueryRedirectUrl].FirstOrDefault();
            var user = HttpContext.User;
            if (!user.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            var credential = new ClientSecretCredential(_tenantId, _clientId, _clientSecret);
            var token = await credential.GetTokenAsync(new Azure.Core.TokenRequestContext(new[] { ScopeApiManagement }));

            var apiManagement = new ApiManagementClient(new TokenCredentials(token.Token));
            apiManagement.SubscriptionId = _subscriptionId;

            var userId = HttpContext.User.FindFirst(NameIdentifierSchemas).Value;
            var isNew = HttpContext.User.FindFirst(NewUser).Value;

            if (bool.TryParse(isNew, out var result))
            {
                if (result)
                {
                    var email = HttpContext.User.FindFirst(EMailAddress)?.Value;
                    var firstName = HttpContext.User.FindFirst(GivenNameSchemas)?.Value;
                    var lastName = HttpContext.User.FindFirst(SurnameSchemas)?.Value;
                    // Create corresponding account in API Management
                    await apiManagement.User.CreateOrUpdateAsync(_resourceGroupName, _serviceName, userId, new UserCreateParameters(email, firstName, lastName));
                }
            }

            var tokenResult = await apiManagement.User.GetSharedAccessTokenAsync(_resourceGroupName, _serviceName, userId, new UserTokenParameters() { Expiry = DateTime.UtcNow.AddHours(3), KeyType = KeyType.Primary });

            return Redirect($"{_ssoUrl}?token={tokenResult.Value}&returnUrl={returnUrl}");
        }
    }
}
