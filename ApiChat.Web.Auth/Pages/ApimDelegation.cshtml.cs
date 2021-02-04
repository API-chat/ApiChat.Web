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
using System.Web;
using ApiChat.Web.Auth.Services;

namespace ApiChat.Web.Auth.Pages
{
    public class ApimDelegationModel : PageModel
    {
        private readonly IValidationService _validationService;

        public string RedirectUrl { get; private set; }
        public bool Unavailable { get; private set; }

        public enum Operations
        {
            SignUp,
            SignIn,
            ChangePassword,
            ChangeProfile,
            CloseAccount,
            SignOut,

            /// <summary>
            /// a request to subscribe the user to a given product with provided ID (see below)
            /// </summary>
            Subscribe,
            /// <summary>
            /// a request to unsubscribe a user from a product
            /// </summary>
            Unsubscribe,
            /// <summary>
            /// a request to renew a subscription(for example, that may be expiring)
            /// </summary>
            Renew

        }

        public ApimDelegationModel(IValidationService validationService)
        {
            _validationService = validationService;
        }

        public async Task<IActionResult> OnGet()
        {
            static bool IsOneOf(Operations enumeration, params Operations[] enums)
            {
                return enums.Contains(enumeration);
            };

            var operation = Enum.Parse<Operations>(Request.Query["operation"].FirstOrDefault());
            var returnUrl = Request.Query["returnUrl"].FirstOrDefault();

            if (IsOneOf(operation, Operations.ChangePassword, Operations.ChangeProfile, Operations.SignOut, Operations.CloseAccount))
            {
                var userId = HttpContext.User.FindFirst(SignInDelegationModel.NameIdentifierSchemas)?.Value;
                if (userId != null && !_validationService.TryValidation(Request, userId))
                {
                    return Unauthorized();
                }
            }

            if (IsOneOf(operation, Operations.SignIn, Operations.SignUp))
            {
                if (!_validationService.TryValidation(Request, returnUrl))
                {
                    return Unauthorized();
                }
            }

            if (IsOneOf(operation, Operations.Renew, Operations.Unsubscribe))
            {
                var subscriptionId = Request.Query["subscriptionId"].FirstOrDefault();
                var userId = Request.Query["userId"].FirstOrDefault();
                if (!_validationService.TryValidation(Request, subscriptionId + "\n" + userId)){
                    return BadRequest();
                }
            }

                switch (operation)
            {
                case Operations.SignUp:
                case Operations.SignIn:
                    var parameters = HttpUtility.ParseQueryString(string.Empty);
                    parameters[SignInDelegationModel.RequestQueryRedirectUrl] = returnUrl;
                   
                    var urlBuider = new UriBuilder("https", Request.Host.Host, (int)Request.Host.Port)
                    {
                        Path = "SignInDelegation",
                        Query = parameters.ToString()
                    };
                    var returnUrlAfterSignIn = urlBuider.Uri.ToString();
                    return Redirect($"/MicrosoftIdentity/Account/Challenge?redirectUri={returnUrlAfterSignIn}");
                case Operations.ChangePassword:
                    return Redirect($"/MicrosoftIdentity/Account/ResetPassword");
                case Operations.ChangeProfile:
                    return Redirect($"/MicrosoftIdentity/Account/EditProfile");
                case Operations.CloseAccount:
                    break;
                case Operations.SignOut:
                    return Redirect($"/MicrosoftIdentity/Account/SignOut");
                case Operations.Subscribe:
                case Operations.Unsubscribe:
                    var urlPaddle = new UriBuilder("https", Request.Host.Host, (int)Request.Host.Port)
                    {
                        Path = "PaddlePay",
                        Query = Request.QueryString.Value
                    };
                    return Redirect(urlPaddle.Uri.ToString());                    
                case Operations.Renew:
                    break;
                default:
                    break;
            }

            RedirectUrl = returnUrl ?? "https://developers.api.chat/";
            Unavailable = true;
            return Page();
        }
    }
}
