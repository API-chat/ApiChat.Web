using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;

namespace ApiChat.Web.Auth.Pages
{
    public class ApimDelegationModel : PageModel
    {
        private string _key;

        enum Operations
        {
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

        public ApimDelegationModel(IConfiguration configuration)
        {
            _key = configuration["Delegation:DelegationValidationKey"];
        }

        public IActionResult OnGet()
        {
            var operation = Enum.Parse<Operations>(Request.Query["operation"].FirstOrDefault());
            var returnUrl = Request.Query["returnUrl"].FirstOrDefault();
#if !DEBUG
            if (!TryValidation(returnUrl))
            {
                return Unauthorized();
            }
#endif
            
            switch (operation)
            {
                case Operations.SignIn:
                    return Redirect($"/MicrosoftIdentity/Account/Challenge?redirectUri={returnUrl}");
                case Operations.ChangePassword:
                    break;
                case Operations.ChangeProfile:
                    break;
                case Operations.CloseAccount:
                    break;
                case Operations.SignOut:
                    break;
                case Operations.Subscribe:
                    break;
                case Operations.Unsubscribe:
                    break;
                case Operations.Renew:
                    break;
                default:
                    break;
            }

            return Page();
        }

        private bool TryValidation(string returnUrl)
        {
            string salt = Request.Query["salt"].FirstOrDefault();
            string sig = Request.Query["sig"].FirstOrDefault();
            string signature;
            using var encoder = new HMACSHA512(Convert.FromBase64String(_key));

            signature = Convert.ToBase64String(encoder.ComputeHash(Encoding.UTF8.GetBytes(salt + "\n" + returnUrl)));
            // change to (salt + "\n" + productId + "\n" + userId) when delegating product subscription
            // compare signature to sig query parameter
            return string.Equals(signature, sig, StringComparison.Ordinal);
        }
    }
}
