using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;

namespace ApiChat.Web.Auth.Areas.MicrosoftIdentity.Pages.Account
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// Because Microsoft.Identity.Web.UI is a Reusable Class Library (RCL), any page can be overridden just by adding it to your web app in the same location.
    /// Full path: Areas/MicrosoftIdentity/Pages/Account/SignedOut.cshtml
    /// </remarks>
    public class SignedOutModel : PageModel
    {
        public Uri AamHome { get; set; }

        public SignedOutModel(IConfiguration configuration)
        {
            var ub = new UriBuilder("https", configuration["ApiManagement:SSOUrl"]);
            AamHome = ub.Uri;
        }

        public IActionResult OnGet()
        {
            return Redirect(AamHome.ToString());
        }
    }
}
