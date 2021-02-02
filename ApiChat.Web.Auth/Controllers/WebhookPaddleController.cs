using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Linq;
using ApiChat.Web.Auth.Services;
using Microsoft.Azure.Management.ApiManagement.Models;
using ApiChat.Web.Auth.Pages;
using System;

namespace ApiChat.Web.Auth.Controllers
{
    [Route("api/v1/[controller]")]
    public class WebhookPaddleController : Controller
    {
        private readonly IApiManagementService _apiManagementService;
        private readonly IClientCredentialService _clientCredentialService;

        public WebhookPaddleController(IApiManagementService apiManagementService, IClientCredentialService clientCredentialService)
        {
            _apiManagementService = apiManagementService;
            _clientCredentialService = clientCredentialService;
        }

        [HttpPost]
        public async Task<IActionResult> Index()
        {
            var request = new PaddleWebhookRequest(Request.Form);

            var productId = request.Passthrough.Split(':').Last();
            var userId = request.Passthrough.Split(':').First();

            switch (request.AlertName)
            {
                case "subscription_created":
                    if (request.Status == "active")
                    {
                        return await AddOrUpdateProductSubscribtion(productId, userId, request.SubscriptionId, SubscriptionState.Active);
                    }
                    break;
                case "subscription_updated":

                    var newPlan = PaddlePayModel.Products.First(o => o.Value == Convert.ToInt32(request.SubscriptionPlanId)).Key;
                    switch (request.Status)
                    {
                        case "active":
                            await AddOrUpdateProductSubscribtion(newPlan, userId, request.SubscriptionId, SubscriptionState.Active);

                            if (!string.IsNullOrEmpty(request.OldSubscriptionPlanId))
                            {
                                var oldPlan = PaddlePayModel.Products.First(o => o.Value == Convert.ToInt32(request.OldSubscriptionPlanId)).Key;
                                if (newPlan != oldPlan)
                                {

                                    await AddOrUpdateProductSubscribtion(oldPlan, userId, request.SubscriptionId, SubscriptionState.Cancelled);

                                }
                            }
                            break;
                        case "deleted":
                            return await AddOrUpdateProductSubscribtion(productId, userId, request.SubscriptionId, SubscriptionState.Cancelled);
                        default:
                            return await AddOrUpdateProductSubscribtion(productId, userId, request.SubscriptionId, SubscriptionState.Suspended);
                    }

                    break;
                case "subscription_cancelled":
                    if (request.Status == "paused")
                    {
                        return await AddOrUpdateProductSubscribtion(productId, userId, request.SubscriptionId, SubscriptionState.Suspended);
                    }
                    if (request.Status == "deleted")
                    {
                        return await AddOrUpdateProductSubscribtion(productId, userId, request.SubscriptionId, SubscriptionState.Cancelled);
                    }
                    break;
                default:
                    break;
            }

            return View();
        }

        private async Task<IActionResult> AddOrUpdateProductSubscribtion(string productId, string userId, string subscriptionId, SubscriptionState state)
        {
            if (string.IsNullOrWhiteSpace(productId)) return BadRequest(productId);
            if (string.IsNullOrWhiteSpace(userId)) return BadRequest(userId);
            if (string.IsNullOrWhiteSpace(subscriptionId)) return BadRequest(subscriptionId);

            var token = await _clientCredentialService.GetAccessTokenAsync();

            var parameters = new SubscriptionCreateParameters()
            {
                DisplayName = subscriptionId,
                Scope = $"/products/{productId}",
                OwnerId = userId,
                State = state
            };

            await _apiManagementService.SubscribtionCreateOrUpdateAsync(token.Token, subscriptionId, parameters);

            return Ok();
        }
    }
}
