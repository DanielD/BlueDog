# BlueDog
BlueDog is a simple set of Azure Functions for user authentication. Serves as the basis for simple scalable back-end services where you only pay for what you use!

See my blog post: (Using Azure Functions and Document DB for Simple User Authentication).

This is a pre-alpha example of a simple user registration and authentication system using Azure Functions. It implements basic functions: register, login, getcurrentuser. 

__Please submit issues, suggestions and requests. Pull requests are even better!!__


## Features

### Simple to add register and login

Simply create an Http Trigger function, then add the call:

```
#r "..\bin\BlueDog.dll"

using System.Net;
using System.Collections.Generic;
using BlueDog;
using BlueDog.DataProvider;
using BlueDog.Models;

// Register

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{
    log.Info($"C# HTTP trigger function processed a request. RequestUri={req.RequestUri}");

    ServerConfiguration configuration = ServerConfiguration.FromAppSettings(log);
    BlueDogAuth bluedog = new BlueDogAuth(configuration, new DocumentDBProvider(configuration));

    return await bluedog.Register(req, log);
}
```

### Simple data layer for easy abstraction and testing

Database calls are abstracted behind an interface which helps unit testing. I use moq for this. Relatively easy to switch to a different
kind of database by providing a different implementation.



## Functions

__Register__ registers a new user. Takes two parameters: _email_ and _password_

__Login__ logs in a user and returns a Json Web Token. Takes two parameters _email_ and _password_

__GetCurrentUser__ returns the currently logged in user. Takes one parameter  _jwt_  (the JWT returned from __Login__) This shows how to validate the JWT to make sure the user 
is logged on.

There are 4 additional methods: startpasswordrset, completepasswordreset, startemailchange, completeemailchange that are designed to be used in conjunction with a web server to provides pages and send emails. These methods just perform the logic. These will be detailed in a future blog post, but are included here.

__*** Note:__ This code is pre-alpha quality. There has been some testing, but should not be put into production without more testing.


## Using

To test and build you will need the Azure Tools for Visual Studio:

https://blogs.msdn.microsoft.com/webdev/2016/12/01/visual-studio-tools-for-azure-functions/

1. You will need to set up a Document DB account. 
1. Create a database on Document DB.
1. Create a collection for users. 'UserCollection' is a reasonable name. :-)
1. After building, you can upload to an the BlueDogSampleFunction to Azure Function.
1. To run locally, change values in appsettings.json according to your DocumentDB account.
1. To run on Azure Functions, add and change values in the Azure Function Configuration. Add all the keys that are in appsettings.json.


Set the BlueDogSampleFunction project as startup and run. This example will work with get or post. It is easier to test get w/ query parameters.


## Next Steps
 
List of things to do:
 
1. Much more testing
1. Better setup documentation
1. Better documentation in general.
1. Changing email address and password. (Implemented but not tested)
1. Controlling what gets returned from the GetCurrentUser call.
1. Common UX patterns such as the onboarding process (see User Experience:  Inviting users to your Android or iOS app Part 1 and Part 2)
1. Support other kinds of databases.
1. OAuth 2.0 and/or OpenId connect. Since this supports JWTs, it shouldn’t be too hard to add the extra steps that allow for OpenId connect.
1. Let’s encrypt support. There is a Microsoft Web API plugin that can do this. and since Azure Functions use Web APIs, it isn’t too hard to get it hooked up.


__Curtis Shipley__ 

_curtis@saltydogtechnology.com_

