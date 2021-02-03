using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ApiChat.Web.Auth.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        public string AadB2CInstance { get; }
        public string AamProfile { get; }

        public IndexModel(ILogger<IndexModel> logger, IConfiguration configuration)
        {
            _logger = logger;

            AadB2CInstance = configuration["AzureAdB2C:Instance"];
            AamProfile = configuration["ApiManagement:ProfileUrl"];
        }

        public IActionResult OnGet()
        {
            if (Request.Headers.ContainsKey("Referer") && Request.Headers["Referer"].Equals(AadB2CInstance))
            {
                return Redirect(AamProfile);
            }

            return Page();
        }
    }
}
