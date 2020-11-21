// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace device_code_flow_console
{
    /// <summary>
    /// Security token provider using the Device Code flow
    /// </summary>
    public class PublicAppUsingDeviceCodeFlow
    {
        /// <summary>
        /// Constructor of a public application leveraging Device Code Flow to sign-in a user
        /// </summary>
        /// <param name="app">MSAL.NET Public client application</param>
        /// <param name="httpClient">HttpClient used to call the protected Web API</param>
        /// <remarks>
        /// For more information see https://aka.ms/msal-net-device-code-flow
        /// </remarks>
        public PublicAppUsingDeviceCodeFlow(IPublicClientApplication app)
        {
            App = app;
        }
        protected IPublicClientApplication App { get; private set; }

        /// <summary>
        /// Acquires a token from the token cache, or device code flow
        /// </summary>
        /// <returns>An AuthenticationResult if the user successfully signed-in, or otherwise <c>null</c></returns>
        public async Task<AuthenticationResult> AcquireATokenFromCacheOrDeviceCodeFlowAsync(IEnumerable<String> scopes)
        {
            AuthenticationResult result = null;
            var accounts = await App.GetAccountsAsync();

            if (accounts.Any())
            {
                try
                {
                    // Attempt to get a token from the cache (or refresh it silently if needed)
                    result = await App.AcquireTokenSilent(scopes, accounts.FirstOrDefault())
                        .ExecuteAsync();
                }
                catch (MsalUiRequiredException)
                {
                }
            }

            // Cache empty or no token for account in the cache, attempt by device code flow
            if (result == null)
            {
                result = await GetTokenForWebApiUsingDeviceCodeFlowAsync(scopes);
            }

            return result;
        }

        /// <summary>
        /// Gets an access token so that the application accesses the web api in the name of the user
        /// who signs-in on a separate device
        /// </summary>
        /// <returns>An authentication result, or null if the user canceled sign-in, or did not sign-in on a separate device
        /// after a timeout (15 mins)</returns>
        private async Task<AuthenticationResult> GetTokenForWebApiUsingDeviceCodeFlowAsync(IEnumerable<string> scopes)
        {
            AuthenticationResult result;
            try
            {
                result = await App.AcquireTokenWithDeviceCode(scopes,
                    deviceCodeCallback =>
                    {
                        // This will print the message on the console which tells the user where to go sign-in using 
                        // a separate browser and the code to enter once they sign in.
                        // The AcquireTokenWithDeviceCodeAsync() method will poll the server after firing this
                        // device code callback to look for the successful login of the user via that browser.
                        // This background polling (whose interval and timeout data is also provided as fields in the 
                        // deviceCodeCallback class) will occur until:
                        // * The user has successfully logged in via browser and entered the proper code
                        // * The timeout specified by the server for the lifetime of this code (typically ~15 minutes) has been reached
                        // * The developing application calls the Cancel() method on a CancellationToken sent into the method.
                        //   If this occurs, an OperationCanceledException will be thrown (see catch below for more details).
                        Console.WriteLine(deviceCodeCallback.Message);
                        return Task.FromResult(0);
                    }).ExecuteAsync();
            }
            catch (MsalServiceException ex)
            {
                // Kind of errors you could have (in errorCode and ex.Message)
                string errorCode = ex.ErrorCode;

                // AADSTS50059: No tenant-identifying information found in either the request or implied by any provided credentials.
                // Mitigation: as explained in the message from Azure AD, the authority needs to be tenanted. you have probably created
                // your public client application with the following authorities:
                // https://login.microsoftonline.com/common or https://login.microsoftonline.com/organizations

                // AADSTS90133: Device Code flow is not supported under /common or /consumers endpoint.
                // Mitigation: as explained in the message from Azure AD, the authority needs to be tenanted

                // AADSTS90002: Tenant <tenantId or domain you used in the authority> not found. This may happen if there are 
                // no active subscriptions for the tenant. Check with your subscription administrator.
                // Mitigation: if you have an active subscription for the tenant this might be that you have a typo in the 
                // tenantId (GUID) or tenant domain name, update the 

                // The issues above are typically programming / app configuration errors, they need to be fixed
                throw;
            }
            catch (OperationCanceledException)
            {
                // If you use an override with a CancellationToken, and call the Cancel() method on it, then this may be triggered
                // to indicate that the operation was cancelled. 
                // See https://docs.microsoft.com/en-us/dotnet/standard/threading/cancellation-in-managed-threads 
                // for more detailed information on how C# supports cancellation in managed threads.
                result = null;
            }
            catch (MsalClientException ex)
            {
                string errorCode = ex.ErrorCode;

                // Verification code expired before contacting the server
                // This exception will occur if the user does not manage to sign-in before a time out (15 mins) and the
                // call to `AcquireTokenWithDeviceCodeAsync` is not cancelled in between
                result = null;
            }
            return result;
        }
    }
}
