using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ApiChat.Web.Auth.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using static ApiChat.Web.Auth.Pages.ApimDelegationModel;

namespace ApiChat.Web.Auth.Pages
{
    public class PaddlePayModel : PageModel
    {
        private readonly IValidationService _validationService;

        public string VendorPaddle { get; }
        public int ProductId { get; private set; }
        public string UserId { get; private set; }
        public string Product { get; private set; }
        public string ProfileUrl { get; set; }
        public string Email { get; set; }

        public static Dictionary<string, int> Products = new Dictionary<string, int> { 
            { "business", 631425 }, 
            { "team", 631424 }, 
            { "personal", 631423 } };

        public PaddlePayModel(IConfiguration configuration, IValidationService validationService)
        {
            _validationService = validationService;

            VendorPaddle = configuration["Paddle:Vendor"];
            var ub = new UriBuilder("https", configuration["ApiManagement:SSOUrl"])
            {
                //Path = "profile",
                //Query = @"r=" + DateTime.Now.Ticks.ToString()
            };

            ProfileUrl = ub.ToString();
        }

        public IActionResult OnGet()
        {
            var operation = Enum.Parse<Operations>(Request.Query["operation"].FirstOrDefault());

            if (!TryProductValidation())
            {
                return Unauthorized();
            }
  
            UserId = HttpContext.User.FindFirst(SignInDelegationModel.NameIdentifierSchemas).Value;
            if (string.IsNullOrWhiteSpace(UserId)) return Unauthorized();

            Product = Request.Query["productId"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(Product)) return BadRequest(Product);
            ProductId = Products[Product];
            if (ProductId == 0) return BadRequest(Product);

            Email = HttpContext.User.FindFirst(SignInDelegationModel.EMailAddress)?.Value;

            return Page();
        }

        private bool TryProductValidation()
        {
            var productId = Request.Query["productId"].FirstOrDefault() ?? Request.Query["subscriptionId"].FirstOrDefault();
            var userId = Request.Query["userId"].FirstOrDefault();
            return _validationService.TryValidation(Request, productId + "\n" + userId);
        }
    }
}
