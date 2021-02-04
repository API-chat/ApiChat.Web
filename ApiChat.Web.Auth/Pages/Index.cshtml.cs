using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiChat.Web.Auth.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Azure.Management.ApiManagement.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ApiChat.Web.Auth.Pages
{
    public class IndexModel : PageModel
    {
        public string AadB2CInstance { get; }
        public string AamProfile { get; }
        private readonly IApiManagementService _apiManagementService;

        public IndexModel(IConfiguration configuration, IApiManagementService apiManagementService)
        {
            AadB2CInstance = configuration["AzureAdB2C:Instance"];
            AamProfile = configuration["ApiManagement:ProfileUrl"];
            _apiManagementService = apiManagementService;
        }

        public async Task<IActionResult> OnGet()
        {
            if (Request.Headers.ContainsKey("Referer") && Request.Headers["Referer"].Equals(AadB2CInstance))
            {
                await SetProfile();
                return Redirect(AamProfile);
            }

            return Page();
        }

        private async Task SetProfile()
        {
            var firstName = HttpContext.User.FindFirst(SignInDelegationModel.GivenNameSchemas)?.Value ?? string.Empty;
            var lastName = HttpContext.User.FindFirst(SignInDelegationModel.SurnameSchemas)?.Value ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(firstName) || !string.IsNullOrWhiteSpace(lastName))
            {
                var userId = HttpContext.User.FindFirst(SignInDelegationModel.NameIdentifierSchemas)?.Value;

                if (!string.IsNullOrEmpty(userId) && (!string.IsNullOrEmpty(firstName)||!string.IsNullOrEmpty(lastName)) )
                {
                    await _apiManagementService.UserUpdateAsync(userId, new UserUpdateParameters(firstName: firstName, lastName: lastName));
                }
            }
        }
    }
}
