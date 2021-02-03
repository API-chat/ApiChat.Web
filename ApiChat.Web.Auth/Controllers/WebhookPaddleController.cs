using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Linq;
using ApiChat.Web.Auth.Services;
using Microsoft.Azure.Management.ApiManagement.Models;
using ApiChat.Web.Auth.Pages;
using System;
using DalSoft.Hosting.BackgroundQueue;
using System.Diagnostics;

namespace ApiChat.Web.Auth.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class WebhookPaddleController : ControllerBase
    {
        private readonly IApiManagementService _apiManagementService;
        private readonly IClientCredentialService _clientCredentialService;
        private readonly BackgroundQueue _backgroundQueue;

        public WebhookPaddleController(IApiManagementService apiManagementService, IClientCredentialService clientCredentialService, BackgroundQueue backgroundQueue)
        {
            _apiManagementService = apiManagementService;
            _clientCredentialService = clientCredentialService;
            _backgroundQueue = backgroundQueue;
        }

        [HttpPost]
        public IActionResult Post()
        {
            var request = new PaddleWebhookRequest(Request.Form);

            var productId = request.Passthrough.Split(':').Last();
            var userId = request.Passthrough.Split(':').First();

            // subscription processing moved to background worker for quick reply
            _backgroundQueue.Enqueue(async ct =>
            {
                Trace.TraceInformation($"Subscription alert: {request.AlertId}|{request.AlertName}|{request.Status}; User:{request.UserId}; Sub:{request.SubscriptionId}; Plan:{request.SubscriptionPlanId}");
                switch (request.AlertName)
                {                    
                    case "subscription_created":
                        if (request.Status == "active")
                        {
                            await AddOrUpdateProductSubscribtion(productId, userId, request.SubscriptionId, SubscriptionState.Active);
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
                                await AddOrUpdateProductSubscribtion(productId, userId, request.SubscriptionId, SubscriptionState.Cancelled);
                                break;
                            default:
                                await AddOrUpdateProductSubscribtion(productId, userId, request.SubscriptionId, SubscriptionState.Suspended);
                                break;
                        }

                        break;
                    case "subscription_cancelled":
                        if (request.Status == "paused")
                        {
                            await AddOrUpdateProductSubscribtion(productId, userId, request.SubscriptionId, SubscriptionState.Suspended);
                        }
                        if (request.Status == "deleted")
                        {
                            await AddOrUpdateProductSubscribtion(productId, userId, request.SubscriptionId, SubscriptionState.Cancelled);
                        }
                        break;
                    default:
                        break;
                }
            });

            return Ok();
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
                OwnerId = $"/users/{userId}",
                State = state
            };

            await _apiManagementService.SubscribtionCreateOrUpdateAsync(token.Token, subscriptionId, parameters);

            return Ok();
        }
    }
}
