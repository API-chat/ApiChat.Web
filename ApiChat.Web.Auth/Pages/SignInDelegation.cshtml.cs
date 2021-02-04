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
        private const string NewUser = "newUser";

        private const string IdpAccessToken = "idp_access_token";
        private const string ClaimsGitHub = "github.com";

        private const string GitHubApiForEmail = "https://api.github.com/user/emails";

        private readonly string _ssoUrl;

        private readonly IContextService _contextService;
        private readonly IApiManagementService _apiManagementService;

        public SignInDelegationModel(IConfiguration configuration, IContextService contextService, IApiManagementService apiManagementService)
        {
            _ssoUrl = configuration["ApiManagement:SSOUrl"];

            _contextService = contextService;

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

            var userId = HttpContext.User.FindFirst(NameIdentifierSchemas)?.Value;
            var isNew = HttpContext.User.FindFirst(NewUser)?.Value;

            if (bool.TryParse(isNew, out var result))
            {
                if (result)
                {
                    string email = await _contextService.GetEmailUser(HttpContext);

                    var firstName = HttpContext.User.FindFirst(GivenNameSchemas)?.Value ?? "-";
                    var lastName = HttpContext.User.FindFirst(SurnameSchemas)?.Value ?? "-";
                    // Create corresponding account in API Management
                    await _apiManagementService.UserCreateOrUpdateAsync(userId, new UserCreateParameters(email, firstName, lastName,
                       confirmation: "signup")); // we do not need invites - so let's skip invite email
                }
            }

            var tokenResult = await _apiManagementService.GetSharedAccessTokenAsync(userId, new UserTokenParameters() { Expiry = DateTime.UtcNow.AddHours(3), KeyType = KeyType.Primary });

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
    }
}
