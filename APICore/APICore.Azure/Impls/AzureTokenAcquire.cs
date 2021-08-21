using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace APICore.Azure.Impls
{
    public class AzureTokenAcquire
    {
        private readonly IPublicClientApplication _app;
        public AzureTokenAcquire(IPublicClientApplication app)
        {
            _app = app;
        }

        public async Task<AuthenticationResult> GetTokenAsync(
            IEnumerable<String> scopes, string userName, SecureString password)
        {
            AuthenticationResult result = null;

            var accounts = await _app.GetAccountsAsync();

            if(accounts.Any())
            {
                try
                {
                    result = await (_app as PublicClientApplication)
                    .AcquireTokenSilent(scopes, accounts.FirstOrDefault())
                    .ExecuteAsync();
                } catch (MsalUiRequiredException)
                {

                }
            }

            if(result == null)
            {
                result = await GetTokenUsingUserPassword(scopes, userName, password);
            }

            return result;
        }

        private async Task<AuthenticationResult> GetTokenUsingUserPassword(
            IEnumerable<string> scopes, string userName, SecureString password)
        {
            AuthenticationResult result = null;

            try
            {
                result = await _app.AcquireTokenByUsernamePassword(
                    scopes, userName, password).ExecuteAsync();
            }

            catch (MsalUiRequiredException) { }
            catch (MsalServiceException ex) when (ex.ErrorCode == "invalid_request") { }
            catch (MsalServiceException ex) when (ex.ErrorCode == "unauthorized_client") { }
            catch (MsalServiceException ex) when (ex.ErrorCode == "invalid_client") { }
            catch (MsalClientException ex) when (ex.ErrorCode == "unknown_user_type") {
                throw new ArgumentException("U/P: Wrong username", ex);
            }
            catch (MsalClientException ex) when (ex.ErrorCode == "user_realm_discovery_failed")
            {
                throw new ArgumentException("U/P: Wrong username", ex);
            }
            catch (MsalClientException ex) when (ex.ErrorCode == "unknown_user")
            {
                throw new ArgumentException("U/P: Wrong username", ex);
            }
            catch (MsalClientException ex) when (ex.ErrorCode == "parsing_wstrust_response_failed") { }

            return result;
        }
    }
}
