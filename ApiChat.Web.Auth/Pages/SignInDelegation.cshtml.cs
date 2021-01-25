using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Management.ApiManagement.Models;
using System.Web;
using ApiChat.Web.Auth.Services;

namespace ApiChat.Web.Auth.Pages
{
    public class SignInDelegationModel : PageModel
    {
        public const string RequestQueryRedirectUrl = "redirectHostUri";
        public const string NameIdentifierSchemas = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";
        private const string GivenNameSchemas = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname";
        private const string SurnameSchemas = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname";
        private const string EMailAddress = "emails";
        private const string NewUser = "newUser";

        private readonly string _ssoUrl;
        private readonly ClientCredentialService _clientCredentialService;
        private readonly IApiManagementService _apiManagementService;

        public SignInDelegationModel(IConfiguration configuration, ClientCredentialService clientCredentialService, IApiManagementService apiManagementService)
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
                    await _apiManagementService.UserCreateOrUpdateAsync(token.Token, userId, new UserCreateParameters(email, firstName, lastName));
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
    }
}
