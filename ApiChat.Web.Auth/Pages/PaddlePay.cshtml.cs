using System;
using System.Collections.Generic;
using System.Globalization;
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
        private readonly IContextService _contextService;

        public string VendorPaddle { get; }
        public int ProductId { get; private set; }
        public string UserId { get; private set; }
        public string Product { get; private set; }
        public string ProfileUrl { get; set; }
        public string Email { get; set; }
        public string Region { get; private set; }

        public static Dictionary<string, int> Products = new Dictionary<string, int> { 
            { "business", 631425 }, 
            { "team", 631424 }, 
            { "personal", 631423 } };

        public PaddlePayModel(IConfiguration configuration, IContextService contextService, IValidationService validationService)
        {
            _validationService = validationService;
            _contextService = contextService;

            VendorPaddle = configuration["Paddle:Vendor"];
            var ub = new UriBuilder("https", configuration["ApiManagement:SSOUrl"])
            {
                Path = "profile"
            };

            ProfileUrl = ub.ToString();
        }

        public async Task<IActionResult> OnGet()
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

            Email = await _contextService.GetEmailUser(HttpContext);

            var country = HttpContext.User.FindFirst("country")?.Value;
            if(country != null)
            {
                var regions = CultureInfo.GetCultures(CultureTypes.SpecificCultures).Select(x => new RegionInfo(x.LCID));
                Region = regions.FirstOrDefault(region => region.EnglishName.Contains(country))?.TwoLetterISORegionName;
            }

            return Page();
        }

        private bool TryProductValidation()
        {
            var productId = Request.Query["productId"].FirstOrDefault();
            var userId = Request.Query["userId"].FirstOrDefault();
            return _validationService.TryValidation(Request, productId + "\n" + userId);
        }
    }
}
