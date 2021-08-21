using APICore.Common.ApplicationServices;
using APICore.Data.Entities;
using Microsoft.Identity.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Security;
using System.Threading.Tasks;

namespace APICore.Azure.Impls
{
    public class AzureService : IAzureService
    {
        private static string[] Scopes { get; set; } =
            new string[] { "User.Read", "User.ReadBasic.All" };

        private IAppSecuritySettings _appSetting;
        private readonly AzureTokenAcquire _tokenAcquire;
        private readonly AzureWebAPICall _apiCall;

        public AzureService(IAppSecuritySettings appSetting)
        {
            _appSetting = appSetting;
            _tokenAcquire = new(PublicClientApplicationBuilder
                .CreateWithApplicationOptions(
                _appSetting.ApplicationSettings).Build());
            _apiCall = new AzureWebAPICall();
        }

        public async Task<(User user, string token, string refreshToken)> GetAADAuthToken(string user, string password)
        {
            AuthenticationResult authenticationResult =
                await _tokenAcquire.GetTokenAsync(Scopes, user, SecurePassword(password));

            User userEntity = null;
            string token = string.Empty;
            string refreshToken = string.Empty;
            
            if(authenticationResult != null)
            {
                userEntity = new User
                {
                    FullName = authenticationResult.Account.Username
                };

                token = authenticationResult.AccessToken;
                refreshToken = authenticationResult.AccessToken;
            }

            return (userEntity, token, refreshToken);
        }

        public async Task<JObject> CallAPI(string apiUrl, string accessToken)
        {
            return await _apiCall.CallAPI(apiUrl, accessToken);
        }

        private SecureString SecurePassword(string password)
        {
            var secPassword = new SecureString();

            foreach(var c in password)
            {
                secPassword.AppendChar(c);
            }

            return secPassword;
        }
    }
}
