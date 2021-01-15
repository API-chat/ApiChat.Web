using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ApiChat.Web.Auth.Pages
{
    public class ApimDelegationModel : PageModel
    {
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

        public IActionResult OnGet()
        {
            var operation = Enum.Parse<Operations>(Request.Query["operation"].FirstOrDefault());
            var returnUrl = Request.Query["returnUrl"].FirstOrDefault();

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
    }
}
