using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ApiChat.Web.Auth.Services
{
    public class ValidationService : IValidationService
    {
        private readonly string _key;

        public ValidationService(IConfiguration configuration)
        {
            _key = configuration["ApiManagement:DelegationValidationKey"];
        }

        public bool TryValidation(HttpRequest request, string text)
        {
            string salt = request.Query["salt"].FirstOrDefault();
            string sig = request.Query["sig"].FirstOrDefault();
            string signature;
            using var encoder = new HMACSHA512(Convert.FromBase64String(_key));

            signature = Convert.ToBase64String(encoder.ComputeHash(Encoding.UTF8.GetBytes(salt + "\n" + text)));

            return string.Equals(signature, sig, StringComparison.Ordinal);
        }
    }

    public interface IValidationService
    {
        bool TryValidation(HttpRequest request, string text);
    }
}
