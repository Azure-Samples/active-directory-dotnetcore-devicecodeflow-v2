/*
 The MIT License (MIT)

Copyright (c) 2015 Microsoft Corporation

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using Microsoft.Identity.Client;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace device_code_flow_console
{
    class Program
    {
        static void Main(string[] args)
        {
            AuthenticationConfig config = AuthenticationConfig.ReadFromJsonFile("appsettings.json");
            CallMicrosoftGraphMeEndpoint(config).Wait();
        }

        /// <summary>
        /// Scopes to request access to the protected Web API (here Microsoft Graph)
        /// </summary>
        public static string[] Scopes { get; set; } = new string[] { "user.read" };

        /// <summary>
        /// URL for the Web API to access (here Microsoft Graph)
        /// </summary>
        public static string WebApiUrl { get; set; } = "https://graph.microsoft.com/v1.0/me";

        /// <summary>
        /// Calls the Microsoft Graph me endpoint
        /// </summary>
        /// <param name="config">authentication config</param>
        private static async Task CallMicrosoftGraphMeEndpoint(AuthenticationConfig config)
        {
            AuthenticationResult result = await GetTokenFromGraphUsingDeviceCodeFlow(config);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{result.Account.Username} successfully signed-in");

            await CallGraph(result);

        }

        private static async Task CallGraph(AuthenticationResult result)
        {
            if (result != null)
            {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", result.AccessToken);

                HttpResponseMessage response = await client.GetAsync(WebApiUrl);
                if (response.IsSuccessStatusCode)
                {
                    string json = await client.GetStringAsync(WebApiUrl);
                    dynamic me = JsonConvert.DeserializeObject(json);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Display(me);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"{response.StatusCode}: {response.ReasonPhrase}");
                }
                Console.ResetColor();

            }
        }

        /// <summary>
        /// Display the result of the call
        /// </summary>
        /// <param name="me"></param>
        private static void Display(dynamic me)
        {
            
        }

        /// <summary>
        /// Gets an access token so that the application access the web api (here MIcrosoft graph) in the name of the user
        /// who signs-in on a separate device
        /// </summary>
        /// <param name="config">authentication config</param>
        /// <returns>An authentication result, or null if the user canceled sign-in, or </returns>
        static async Task<AuthenticationResult> GetTokenFromGraphUsingDeviceCodeFlow(AuthenticationConfig config)
        {
            PublicClientApplication app = new PublicClientApplication(config.ClientId, config.Authority);
            AuthenticationResult result;
            try
            {
                result = await app.AcquireTokenWithDeviceCodeAsync(Scopes,
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
                    });
            }
            catch (MsalServiceException ex)
            {
                string errorCode = ex.ErrorCode;

                // Kind of errors you could have (in errorCode and ex.Message)

                // AADSTS50059: No tenant-identifying information found in either the request or implied by any provided credentials.
                // Mitigation: as explained in the message from Azure AD, the authoriy needs to be tenanted. you have probably created
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
