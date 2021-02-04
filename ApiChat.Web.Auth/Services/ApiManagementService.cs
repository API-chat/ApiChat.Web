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
        private readonly ITokenProvider _tokenProvider;

        public ApiManagementService(IConfiguration configuration, ApiManagementTokenProvider tokenProvider)
        {
            _subscriptionId = configuration["ApiManagement:SubscriptionId"];
            _resourceGroupName = configuration["ApiManagement:ResourceGroupName"];
            _serviceName = configuration["ApiManagement:ServiceName"];
            _tokenProvider = tokenProvider;
        }

        public async Task UserCreateOrUpdateAsync(string userId, UserCreateParameters parameters)
        {
            try
            {
                var apiManagement = new ApiManagementClient(new TokenCredentials(_tokenProvider))
                {
                    SubscriptionId = _subscriptionId
                };

                parameters.AppType = "developerPortal";
                await apiManagement.User.CreateOrUpdateAsync(_resourceGroupName, _serviceName, userId, parameters);
            }
            catch (Exception ex)
            {
                Trace.TraceInformation(ex.ToString());
                throw;
            }
            
        }

        public async Task UserUpdateAsync(string userId, UserUpdateParameters parameters)
        {
            try
            {
                if (string.IsNullOrEmpty(parameters.FirstName))
                {
                    parameters.FirstName = "-";
                }
                if (string.IsNullOrEmpty(parameters.LastName))
                {
                    parameters.LastName = "-";
                }

                var apiManagement = new ApiManagementClient(new TokenCredentials(_tokenProvider))
                {
                    SubscriptionId = _subscriptionId
                };

                await apiManagement.User.UpdateAsync(_resourceGroupName, _serviceName, userId, parameters, "*");
            }
            catch (Exception ex)
            {
                Trace.TraceInformation(ex.ToString());
                throw;
            }
        }

        public async Task<UserTokenResult> GetSharedAccessTokenAsync(string userId, UserTokenParameters parameters)
        {
            var apiManagement = new ApiManagementClient(new TokenCredentials(_tokenProvider))
            {
                SubscriptionId = _subscriptionId
            };

            return await apiManagement.User.GetSharedAccessTokenAsync(_resourceGroupName, _serviceName, userId, parameters);
        }

        public async Task SubscribtionCreateOrUpdateAsync( string sid, SubscriptionCreateParameters parameters)
        {
            try
            {
                var apiManagement = new ApiManagementClient(new TokenCredentials(_tokenProvider))
                {
                    SubscriptionId = _subscriptionId
                };

                await apiManagement.Subscription.CreateOrUpdateAsync(_resourceGroupName, _serviceName, sid, parameters, true);
            }
            catch (Exception ex)
            {
                Trace.TraceInformation(ex.ToString());
                throw;
            }
        }

        public async Task<UserContract> GetUser(string userId)
        {
            var apiManagement = new ApiManagementClient(new TokenCredentials(_tokenProvider))
            {
                SubscriptionId = _subscriptionId
            };

            return await apiManagement.User.GetAsync(_resourceGroupName, _serviceName, userId);
        }
    }

    public interface IApiManagementService
    {
        Task<UserContract> GetUser(string userId);
        Task UserCreateOrUpdateAsync(string userId, UserCreateParameters parameters);
        Task UserUpdateAsync(string userId, UserUpdateParameters parameters);
        Task<UserTokenResult> GetSharedAccessTokenAsync(string userId, UserTokenParameters parameters);
        Task SubscribtionCreateOrUpdateAsync(string sid, SubscriptionCreateParameters parameters);
    }
}
