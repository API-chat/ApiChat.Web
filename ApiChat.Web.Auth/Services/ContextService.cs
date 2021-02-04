using ApiChat.Web.Auth.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiChat.Web.Auth.Services
{
    public interface IContextService
    {
        Task<string> GetEmailUser(HttpContext httpContext);
    }

    public class ContextService : IContextService
    {
        public const string EMailAddress = "emails";

        private const string IdpAccessToken = "idp_access_token";
        private const string ClaimsGitHub = "github.com";
        private const string IdentitySchemas = "http://schemas.microsoft.com/identity/claims/identityprovider";
        private const string GitHubApiForEmail = "https://api.github.com/user/emails";

        public async Task<string> GetEmailUser(HttpContext httpContext)
        {
            var provider = httpContext.User.FindFirst(IdentitySchemas)?.Value;

            var email = httpContext.User.FindFirst(EMailAddress)?.Value;

            if (string.Equals(provider, ClaimsGitHub))
            {
                email = await FindGitHubEmail(httpContext, email);
            }

            return email;
        }

        private async Task<string> FindGitHubEmail(HttpContext httpContext, string email)
        {
            var accessToken = httpContext.User.FindFirst(IdpAccessToken)?.Value;
            var client = new RestClient(GitHubApiForEmail);
            var request = new RestRequest(Method.GET);
            request.AddHeader("Authorization", $"Basic {Base64Encode("xakpc:" + accessToken)}");
            var gitResult = await client.ExecuteAsync(request);

            if (gitResult.IsSuccessful)
            {
                var emails = JsonConvert.DeserializeObject<List<EmailGitHub>>(gitResult.Content);
                email = emails.First(o => o.primary == true).email;
            }

            return email;
        }

        private static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
    }
}
