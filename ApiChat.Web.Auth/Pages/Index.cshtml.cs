using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiChat.Web.Auth.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Azure.Management.ApiManagement.Models;
using Microsoft.Extensions.Logging;

namespace ApiChat.Web.Auth.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IApiManagementService _apiManagementService;
        private readonly IClientCredentialService _clientCredentialService;

        public IndexModel(ILogger<IndexModel> logger, IClientCredentialService clientCredentialService, IApiManagementService apiManagementService)
        {
            _logger = logger;
            _clientCredentialService = clientCredentialService;
            _apiManagementService = apiManagementService;
        }


        public async Task<IActionResult> OnGet()
        {
            var firstName = HttpContext.User.FindFirst(SignInDelegationModel.GivenNameSchemas)?.Value ?? string.Empty;
            var lastName = HttpContext.User.FindFirst(SignInDelegationModel.SurnameSchemas)?.Value ?? string.Empty;
            if(!string.IsNullOrWhiteSpace(firstName) || !string.IsNullOrWhiteSpace(lastName))
            {
                var token = await _clientCredentialService.GetAccessTokenAsync();

                var userId = HttpContext.User.FindFirst(SignInDelegationModel.NameIdentifierSchemas)?.Value;

                if (!string.IsNullOrEmpty(userId))
                {
                    await _apiManagementService.UserUpdateAsync(token.Token, userId, new UserUpdateParameters(firstName: firstName, lastName: lastName));
                }
            }

            return Redirect("https://developers.api.chat/");
        }
    }
}
