---
services: active-directory
author: jmprieur
platforms: dotnet
level: 200
client: .NET Core 2.1 console app
service: Microsoft Graph
endpoint: AAD v2.0
---
# Invoking an API protected by Azure AD from a text-only device

[![Build status](https://identitydivision.visualstudio.com/IDDP/_apis/build/status/AAD%20Samples/.NET%20client%20samples/IDDP-ASP.NET%20Core-CI)](https://identitydivision.visualstudio.com/IDDP/_build/latest?definitionId=-1)

## About this sample

### Overview

This sample demonstrates how to leverage MSAL.NET from apps that **do not have the capability of offering an interactive authentication experience**. It enables these apps to:

- authenticate a user
- and call to a web API (in this case, the [Microsoft Graph](https://graph.microsoft.com))

The sample uses the OAuth2 **device code flow**. The app is built entirely on .NET Core, hence it can be ran as-is on Windows (including Nano Server), OSX, and Linux machines.

To emulate a device not capable of showing UX, the sample is packaged as a .NET Core console application.
The application signs users in with Azure Active Directory (Azure AD), using the Microsoft  Authentication Library for .NET (MSAL.NET) to obtain a JWT access token through the OAuth 2.0 protocol. The access token is then used to call the Microsoft Graph API to obtain information about the user who signed-in. The sample is structured so that you can call your own API

![Topology](./ReadmeFiles/Topology.png)

If you would like to get started immediately, skip this section and jump to *How To Run The Sample*.

### Scenario

The application obtains tokens through a two steps process especially designed for devices and operating systems that cannot display any UX. Examples of such applications are applications running on iOT, or Command-Line tools (CLI). The idea is that:

1. whenever a user authentication is required, the command-line app provides a code and asks the user to use another device (such as an internet-connected smartphone) to navigate to [https://microsoft.com/devicelogin](https://microsoft.com/devicelogin), where the user will be prompted to enter the code. That done, the web page will lead the user through a normal authentication experience, including consent prompts and multi factor authentication if necessary.

![UI](./ReadmeFiles/deviceCodeFlow.png)

2. Upon successful authentication, the command-line app will receive the required tokens through a back channel and will use it to perform the web API calls it needs. In this case, the sample displays information about the user who signed-in and their manager.

## About the code

The code for handling the token acquisition process is simple, as it boils down to calling the `AcquireTokenWithDeviceCodeAsync` method of `PublicClientApplication` to which you pass a callback that will display information to the user about where they should nativate to, and which code to enter to initiate a sign-in. See the `GetTokenForWebApiUsingDeviceCodeFlowAsync` method in `device-code-flow-console\PublicAppUsingDeviceCodeFlow.cs`.

```CSharp
async Task<AuthenticationResult> GetTokenForWebApiUsingDeviceCodeFlowAsync()

    AuthenticationResult result;
    try
    {
        result = await app.AcquireTokenWithDeviceCodeAsync(Scopes,
            deviceCodeCallback =>
            {
                Console.WriteLine(deviceCodeCallback.Message);
                return Task.FromResult(0);
                });
            }
        
        ...
        // error handling omited here (see sample for details)
        return result;
    }
```

## How to run this sample

To run this sample, you'll need:

- [Visual Studio 2017](https://aka.ms/vsdownload) or just the [.NET Core SDK](https://www.microsoft.com/net/learn/get-started)
- An Internet connection
- A Windows machine (necessary if you want to run the app on Windows)
- An OS X machine (necessary if you want to run the app on Mac)
- A Linux machine (necessary if you want to run the app on Linux)
- An Azure Active Directory (Azure AD) tenant. For more information on how to get an Azure AD tenant, see [How to get an Azure AD tenant](https://azure.microsoft.com/en-us/documentation/articles/active-directory-howto-tenant/)
- A user account in your Azure AD tenant. This sample will not work with a Microsoft account (formerly Windows Live account). Therefore, if you signed in to the [Azure portal](https://portal.azure.com) with a Microsoft account and have never created a user account in your directory before, you need to do that now.

### Step 1: Clone or download this repository

From your shell or command line:

```Shell
git clone https://github.com/Azure-Samples/active-directory-dotnetcore-devicecodeflow-v2.git
```

or download and exact the repository .zip file.

> Given that the name of the sample is pretty long, and so are the name of the referenced NuGet packages, you might want to clone it in a folder close to the root of your hard drive, to avoid file size limitations on Windows.

### Step 2: Setup .NET core

The .NET core [documentation pages](https://www.microsoft.com/net/learn/get-started) provide step by step instructions for installing .NET Core (the .NET Execution Environment) for your platform of choice.

### Step 3: Run the sample

#### If you prefer to use Visual Studio

Open the solution in Visual Studio, restore the NuGet packages, select the project, and start it in the debugger.

#### (otherwise) on any platform

Open a terminal and navigate to the project folder (`device-code-flow-console`).
Restore the packages with the following command:

```PowerShell
    dotnet restore
```

Launch the app by entering the following command:

```PowerShell
    dotnet run
```

#### Operating the sample

When you run the sample, you will be presented with a prompt telling you

> To sign in, use a web browser to open the page https://microsoft.com/devicelogin. Enter the code B7D3SVXHV to authenticate.

Then:

1. Open a browser on any device. For instance, the browser can be on the computer on which you are running the sample, or even your smartphone. Then navigate, as instructed, to [https://microsoft.com/devicelogin](https://microsoft.com/devicelogin)

2. Once there, type in the code provided by the app (in this sample, I am typing `B7D3SVXHV`) and hit enter. The web page will proceed to prompt you for authentication: please authenticate as a user (native or guest) in the tenant that you specified in the application. Note that, thanks to the fact that you are using an external browser or a different, browser capable device, you can authenticate without restrictions: for example, if your tenant requires you to authenticate using MFA, you are able to do so. That experience would not have been possible if you had to drive the authentication operations exclusively in the console.
3. Once you successfully authenticate, go back to the console app. You'll see that the app has now access to the token it needs to query the Microsoft Graph API and display information about the signed-in user.

### Optional: configure the sample as an app in your directory tenant

The instructions so far leveraged the Azure AD entry for the app in a Microsoft test tenant: given that the app is multitenant, anybody can run the sample against that app entry.
To register your project in your own Azure AD tenant, you can find instructions to manually provision the sample in your own tenant, so that you can exercise complete control on the app settings and behavior.

- either follow the manual steps 
- or use PowerShell scripts that:
  - **automatically** creates the Azure AD applications and related objects (passwords, permissions, dependencies) for you
  - modify the Visual Studio projects' configuration files.

If you want to use this automation:
1. On Windows run PowerShell and navigate to the root of the cloned directory
1. In PowerShell run:
   ```PowerShell
   Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope Process -Force
   ```
1. Run the script to create your Azure AD application and configure the code of the sample application accordinly. 
   ```PowerShell
   .\AppCreationScripts\Configure.ps1
   ```
   > Other ways of running the scripts are described in [App Creation Scripts](./AppCreationScripts/AppCreationScripts.md)

1. Open the Visual Studio solution and click start

If ou don't want to use this automation, follow the steps below

#### First step: choose the Azure AD tenant where you want to create your applications

As a first step you'll need to:

1. Sign in to the [Azure portal](https://portal.azure.com) using either a work or school account or a personal Microsoft account.
1. If your account is present in more than one Azure AD tenant, select `Directory + Subscription` at the top right corner in the menu on top of the page, and switch your portal session to the desired Azure AD tenant.   
1. In the left-hand navigation pane, select the **Azure Active Directory** service, and then select **App registrations (Preview)**.

> In the next steps, you might need the tenant name (or directory name) or the tenant ID (or directory ID). These are presented in the **Properties**
of the Azure Active Directory window respectively as *Name* and *Directory ID*

#### Register the client app (active-directory-dotnet-deviceprofile)

1. In **App registrations (Preview)** page, select **New registration**.
1. When the **Register an application page** appears, enter your application's registration information:
   - In the **Name** section, enter a meaningful application name that will be displayed to users of the app, for example `active-directory-dotnet-deviceprofile`.
   - In the **Supported account types** section, select **Accounts in any organizational directory**.
1. Select **Register** to create the application.
1. On the app **Overview** page, find the **Application (client) ID** value and record it for later. You'll need it to configure the Visual Studio configuration file for this project.
1. In the list of pages for the app, select **Manifest**, and:
   - In the manifest editor, set the ``allowPublicClient`` property to **true** 
   - Select **Save** in the bar above the manifest editor.
1. In the list of pages for the app, select **API permissions**
   - Click the **Add a permission** button and then,
   - Ensure that the **Microsoft APIs** tab is selected
   - In the *Commonly used Microsoft APIs* section, click on **Microsoft Graph**
   - In the **Delegated permissions** section, ensure that the right permissions are checked: **User.Read**, **User.ReadBasic.All**. Use the search box if necessary.
   - Select the **Add permissions** button

1. At this stage permissions are assigned correctly but the client app does not allow interaction. 
   Therefore no consent can be presented via a UI and accepted to use the service app. 
   Click the **Grant/revoke admin consent for {tenant}** button, and then select **Yes** when you are asked if you want to grant consent for the
   requested permissions for all account in the tenant.
   You need to be an Azure AD tenant admin to do this.

#### Configure the sample to use your Azure AD tenant

In the steps below, ClientID is the same as Application ID or AppId.

Open the solution in Visual Studio to configure the projects

#### Configure the client project

> Note: if you used the setup scripts, the changes below will have been applied for you

1. Open the `device-code-flow-console\appsettings.json` file
1. Find the app key `ClientId` and replace the existing value with the application ID (clientId) of the `active-directory-dotnet-deviceprofile` application copied from the Azure portal.
1. (Optional) Find the line where `Tenant` is set and replace the existing value with your tenant ID.

## Community Help and Support

Use [Stack Overflow](http://stackoverflow.com/questions/tagged/msal) to get support from the community.
Ask your questions on Stack Overflow first and browse existing issues to see if someone has asked your question before.
Make sure that your questions or comments are tagged with [`msal` `dotnet`].

If you find a bug in the sample, please raise the issue on [GitHub Issues](../../issues).

To provide a recommendation, visit the following [User Voice page](https://feedback.azure.com/forums/169401-azure-active-directory).

## Contributing

If you'd like to contribute to this sample, see [CONTRIBUTING.MD](/CONTRIBUTING.md).

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information, see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## More information

For more information, see MSAL.NET's conceptual documentation:

- [Device Code Flow for devices without a Web browser](https://aka.ms/msal-net-device-code-flow)
- [Customizing Token cache serialization](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/token-cache-serialization) (was not done in this sample, but you might want to add a serialized cache)
- [Quickstart: Register an application with the Microsoft identity platform (Preview)](https://docs.microsoft.com/azure/active-directory/develop/quickstart-register-app)
- [Quickstart: Configure a client application to access web APIs (Preview)](https://docs.microsoft.com/azure/active-directory/develop/quickstart-configure-app-access-web-apis)

- [Understanding Azure AD application consent experiences](https://docs.microsoft.com/en-us/azure/active-directory/develop/application-consent-experience)
- [Understand user and admin consent](https://docs.microsoft.com/en-us/azure/active-directory/develop/howto-convert-app-to-be-multi-tenant#understand-user-and-admin-consent)
- [Application and service principal objects in Azure Active Directory](https://docs.microsoft.com/en-us/azure/active-directory/develop/app-objects-and-service-principals)

For more information about the Microsoft identitity platform v2.0 (Azure AD v2.0), see:
- [https://aka.ms/aadv2](https://aka.ms/aadv2)
