using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Management.ApiManagement.Models;
using System.Web;
using ApiChat.Web.Auth.Services;
using RestSharp;
using Newtonsoft.Json;
using ApiChat.Web.Auth.Models;
using System.Collections.Generic;

namespace ApiChat.Web.Auth.Pages
{
    public class SignInDelegationModel : PageModel
    {
        public const string RequestQueryRedirectUrl = "redirectHostUri";
        public const string NameIdentifierSchemas = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";
        public const string GivenNameSchemas = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname";
        public const string SurnameSchemas = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname";
        private const string IdentitySchemas = "http://schemas.microsoft.com/identity/claims/identityprovider";
        public const string EMailAddress = "emails";
        private const string NewUser = "newUser";

        private const string IdpAccessToken = "idp_access_token";
        private const string ClaimsGitHub = "github.com";

        private const string GitHubApiForEmail = "https://api.github.com/user/emails";

        private readonly string _ssoUrl;
        private readonly IClientCredentialService _clientCredentialService;
        private readonly IApiManagementService _apiManagementService;

        public SignInDelegationModel(IConfiguration configuration, IClientCredentialService clientCredentialService, IApiManagementService apiManagementService)
        {
            _ssoUrl = configuration["ApiManagement:SSOUrl"];

            _clientCredentialService = clientCredentialService;
            _apiManagementService = apiManagementService;
        }

        public async Task<IActionResult> OnGet()
        {
            var returnUrl = Request.Query[RequestQueryRedirectUrl].FirstOrDefault();
            var user = HttpContext.User;

            if (!user.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            var token = await _clientCredentialService.GetAccessTokenAsync();

            var userId = HttpContext.User.FindFirst(NameIdentifierSchemas)?.Value;
            var isNew = HttpContext.User.FindFirst(NewUser)?.Value;

            if (bool.TryParse(isNew, out var result))
            {
                if (result)
                {
                    var provider = HttpContext.User.FindFirst(IdentitySchemas)?.Value;

                    var email = HttpContext.User.FindFirst(EMailAddress)?.Value;

                    if (string.Equals(provider, ClaimsGitHub))
                    {
                        email = await FindGitHubEmail(email);
                    }

                    var firstName = HttpContext.User.FindFirst(GivenNameSchemas)?.Value ?? string.Empty;
                    var lastName = HttpContext.User.FindFirst(SurnameSchemas)?.Value ?? string.Empty;
                    // Create corresponding account in API Management
                    await _apiManagementService.UserCreateOrUpdateAsync(token.Token, userId, new UserCreateParameters(email, firstName, lastName,
                        appType: "developerPortal", confirmation: "signup")); // we do not need invites - so let's skip invite email
                }
            }

            var tokenResult = await _apiManagementService.GetSharedAccessTokenAsync(token.Token, userId, new UserTokenParameters() { Expiry = DateTime.UtcNow.AddHours(3), KeyType = KeyType.Primary });

            var parameters = HttpUtility.ParseQueryString(string.Empty);
            parameters["token"] = tokenResult.Value;
            parameters["returnUrl"] = returnUrl;
            var urlBuider = new UriBuilder
            {
                Host = _ssoUrl,
                Scheme = "https",
                Path = "signin-sso",
                Query = parameters.ToString()
            };

            return Redirect(urlBuider.Uri.ToString());
        }

        private async Task<string> FindGitHubEmail(string email)
        {
            var accessToken = HttpContext.User.FindFirst(IdpAccessToken)?.Value;
            var client = new RestClient(GitHubApiForEmail);
            var request = new RestRequest(Method.GET);
            request.AddHeader("Authorization", $"Basic {Base64Encode("xakpc:" + accessToken)}");
            var gitResult = await client.ExecuteAsync(request);

            if (gitResult.IsSuccessful)
            {
                var emails = JsonConvert.DeserializeObject<List<EmailGitHub>>(gitResult.Content);
                email = emails.First(o => o.primary == true).email;
            }

            return email;
        }

        private static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
    }
}
