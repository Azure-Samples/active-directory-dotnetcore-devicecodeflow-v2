// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Identity.Client;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace device_code_flow_console
{
    /// <summary>
    /// This sample signs-in a user in a two steps process:
    /// - it displays a URL and a code, and asks the user to navigate to the URL in a Web browser, and enter the code
    /// - then the user signs-in (and goes through multiple factor authentication if needed)
    /// and the sample displays information about the user by calling the Microsoft Graph in the name of the signed-in user
    /// 
    /// It uses the Device code flow, which is normally used for devices which don't have a Web browser (which is the case for a
    /// .NET Core app, iOT, etc ...)
    /// 
    /// For more information see https://aka.ms/msal-net-device-code-flow
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                RunAsync().GetAwaiter().GetResult();
            }
            catch(Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ResetColor();
            }

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }

        private static async Task RunAsync()
        {
            SampleConfiguration config = SampleConfiguration.ReadFromJsonFile("appsettings.json");
            var appConfig = config.PublicClientApplicationOptions;
            var app = PublicClientApplicationBuilder.CreateWithApplicationOptions(appConfig)
                                                    .Build();
            var httpClient = new HttpClient();

            MyInformation myInformation = new MyInformation(app, httpClient, config.MicrosoftGraphBaseEndpoint);
            await myInformation.DisplayMeAndMyManagerAsync();
        }
    }
}
