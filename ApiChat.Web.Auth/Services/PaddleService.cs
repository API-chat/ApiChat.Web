using ApiChat.Web.Auth.Controllers;
using ApiChat.Web.Auth.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiChat.Web.Auth.Services
{
    public class PaddleService : IPaddleService
    {
        private readonly RestClient _client;
        private readonly string _vendorId;
        private readonly string _authToken;

        public PaddleService(IConfiguration configuration)
        {
            _client = new RestClient(configuration["Paddle:Api"]);
            _vendorId = configuration["Paddle:Vendor"];
            _authToken = configuration["Paddle:AuthToken"];
        }

        public async Task<PaddleWebhookResponse> GetSubscriptionUsers(string subscriptionId)
        {
            var request = new RestRequest("subscription/users", Method.POST);

            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddParameter("application/x-www-form-urlencoded", $"vendor_id={_vendorId}&vendor_auth_code={_authToken}&subscription_id={subscriptionId}", ParameterType.RequestBody);

            var response = await _client.ExecuteAsync(request);

            if (response.IsSuccessful)
            {
                var user = JsonConvert.DeserializeObject<PaddleResponse>(response.Content);
                return user.Response;
            }

            return null;
        }
    }

    public interface IPaddleService
    {
        Task<PaddleWebhookResponse> GetSubscriptionUsers(string subscriptionId);
    }
}
