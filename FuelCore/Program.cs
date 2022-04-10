using System.Collections.Specialized;
using FuelSDK;


var subdomain = "mcfy5ywq38zqg4p2w27r5m-091cq";

var client = new ETClient(new NameValueCollection {
    { "accountId", "534000890" },
    { "clientId", "u48ja4ltkg6erq4z0b93ja51" },
    { "clientSecret", "QRviaBpewdoxWkssLpMRlD8R" },
    { "authEndPoint", $"https://{subdomain}.auth.marketingcloudapis.com" },
    { "soapEndPoint", $"https://{subdomain}.soap.marketingcloudapis.com" },
    { "restEndPoint", $"https://{subdomain}.rest.marketingcloudapis.com" },
    { "useOAuth2Authentication", "true" },
    { "applicationType", "server" },
    //{"authorizationCode", "" },
    //{"redirectURI", "REDIRECT_URI_FOR_PUBLIC/WEB_APP" },
    //{"scope", "DATA_ACCESS_PERMISSIONS" }
});


var list = new ETList() {
    AuthStub = client
};

var allLists = list.Get();

Console.WriteLine($"Get Status: {allLists.Status}");
Console.WriteLine($"Message: {allLists.Message}");
Console.WriteLine($"Code: {allLists.Code}");
Console.WriteLine($"Results Length: {allLists.Results.Length}");

foreach (var resultList in allLists.Results.Cast<ETList>()) {
    Console.WriteLine($"--ID: {resultList.ID}, Name: {resultList.ListName}, Description: {resultList.Description}");
}

