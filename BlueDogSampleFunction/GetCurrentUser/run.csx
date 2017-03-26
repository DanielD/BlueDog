#r "..\bin\BlueDog.dll"


using System.Net;
using System.Collections.Generic;
using BlueDog;
using BlueDog.DataProvider;
using BlueDog.Models;

// Get current user

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{
    log.Info($"C# HTTP trigger function processed a request. RequestUri={req.RequestUri}");

    ServerConfiguration configuration = ServerConfiguration.FromAppSettings(log);
    BlueDogAuth bluedog = new BlueDogAuth(configuration, new DocumentDBProvider(configuration));

    return await bluedog.GetCurrentUser(req, log);
}