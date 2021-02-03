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
        private readonly IClientCredentialService _clientCredentialService;

        public IndexModel(IConfiguration configuration, IClientCredentialService clientCredentialService, IApiManagementService apiManagementService)
        {
            AadB2CInstance = configuration["AzureAdB2C:Instance"];
            AamProfile = configuration["ApiManagement:ProfileUrl"];
            _clientCredentialService = clientCredentialService;
            _apiManagementService = apiManagementService;
        }

        public async Task<IActionResult> OnGet()
        {
            var firstName = HttpContext.User.FindFirst(SignInDelegationModel.GivenNameSchemas)?.Value ?? string.Empty;
            var lastName = HttpContext.User.FindFirst(SignInDelegationModel.SurnameSchemas)?.Value ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(firstName) || !string.IsNullOrWhiteSpace(lastName))
            {
                var token = await _clientCredentialService.GetAccessTokenAsync();

                var userId = HttpContext.User.FindFirst(SignInDelegationModel.NameIdentifierSchemas)?.Value;

                if (!string.IsNullOrEmpty(userId))
                {
                    await _apiManagementService.UserUpdateAsync(token.Token, userId, new UserUpdateParameters(firstName: firstName, lastName: lastName));
                }
            }

            if (Request.Headers.ContainsKey("Referer") && Request.Headers["Referer"].Equals(AadB2CInstance))
            {
                return Redirect(AamProfile);
            }

            return Page();
        }
    }
}
