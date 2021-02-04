using Microsoft.Azure.Management.ApiManagement;
using Microsoft.Azure.Management.ApiManagement.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Rest;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ApiChat.Web.Auth.Services
{
    public class ApiManagementService : IApiManagementService
    {
        private readonly string _subscriptionId;
        private readonly string _resourceGroupName;
        private readonly string _serviceName;

        public ApiManagementService(IConfiguration configuration)
        {
            _subscriptionId = configuration["ApiManagement:SubscriptionId"];
            _resourceGroupName = configuration["ApiManagement:ResourceGroupName"];
            _serviceName = configuration["ApiManagement:ServiceName"];
        }

        public async Task UserCreateOrUpdateAsync(string token, string userId, UserCreateParameters parameters)
        {
            try
            {
                var apiManagement = new ApiManagementClient(new TokenCredentials(token))
                {
                    SubscriptionId = _subscriptionId
                };

                await apiManagement.User.CreateOrUpdateAsync(_resourceGroupName, _serviceName, userId, parameters);
            }
            catch (Exception ex)
            {
                Trace.TraceInformation(ex.ToString());
                throw;
            }
            
        }

        public async Task UserUpdateAsync(string token, string userId, UserUpdateParameters parameters)
        {
            var apiManagement = new ApiManagementClient(new TokenCredentials(token))
            {
                SubscriptionId = _subscriptionId
            };

            await apiManagement.User.UpdateAsync(_resourceGroupName, _serviceName, userId, parameters, "*");
        }

        public async Task<UserTokenResult> GetSharedAccessTokenAsync(string token, string userId, UserTokenParameters parameters)
        {
            var apiManagement = new ApiManagementClient(new TokenCredentials(token))
            {
                SubscriptionId = _subscriptionId
            };

            return await apiManagement.User.GetSharedAccessTokenAsync(_resourceGroupName, _serviceName, userId, parameters);
        }

        public async Task SubscribtionCreateOrUpdateAsync(string token, string sid, SubscriptionCreateParameters parameters)
        {
            try
            {
                var apiManagement = new ApiManagementClient(new TokenCredentials(token))
                {
                    SubscriptionId = _subscriptionId
                };

                await apiManagement.Subscription.CreateOrUpdateAsync(_resourceGroupName, _serviceName, sid, parameters);
            }
            catch (Exception ex)
            {
                Trace.TraceInformation(ex.ToString());
                throw;
            }
        }
    }

    public interface IApiManagementService
    {
        Task UserCreateOrUpdateAsync(string token, string userId, UserCreateParameters parameters);
        Task UserUpdateAsync(string token, string userId, UserUpdateParameters parameters);
        Task<UserTokenResult> GetSharedAccessTokenAsync(string token, string userId, UserTokenParameters parameters);
        Task SubscribtionCreateOrUpdateAsync(string token, string sid, SubscriptionCreateParameters parameters);
    }
}
