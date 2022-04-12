using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml.Linq;

using JWT;
using JWT.Algorithms;
using JWT.Serializers;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace FuelSDK;

/// <summary>
/// Client interface class which manages the authentication process 
/// </summary>
public class ETClient
{
    public static string SDKVersion = $"FuelSDK-C#-v{GitVersionInformation.NuGetVersion}";

    public string AuthToken { get; private set; }
    public SoapClient SoapClient { get; private set; }
    public string InternalAuthToken { get; private set; }
    public string RefreshKey { get; internal set; }
    public DateTime AuthTokenExpiration { get; private set; }
    public JObject Jwt { get; private set; }
    public string EnterpriseId { get; private set; }
    public string OrganizationId { get; private set; }

    public bool UseOAuth2Authentication => Settings.UseOAuth2Authentication;
    public string AuthEndPoint => Settings.AuthEndPoint;
    public string SoapEndPoint => Settings.SoapEndPoint;
    public string RestEndPoint => Settings.RestEndPoint;


    [Obsolete(StackKeyErrorMessage)]
    public string Stack {
        get => _stackKey ??= GetStackFromSoapEndPoint(new Uri(Settings.SoapEndPoint));
        private init => _stackKey = value;
    }
    private string _stackKey;


    public class RefreshState
    {
        public string RefreshKey { get; set; }
        public string EnterpriseId { get; set; }
        public string OrganizationId { get; set; }
        public string Stack { get; set; }
    }


    public ETClient(string jwt)
        : this(CreateOptions(new FuelSettings { Jwt = jwt }), null)
    { }


    public ETClient(FuelSettings settings, RefreshState refreshState = null)
        : this(CreateOptions(settings), refreshState)
    { }


    public ETClient(IOptions<FuelSettings> settings, RefreshState refreshState = null)
    {
        // Get configuration file and set variables
        Settings = (settings.Value ?? new FuelSettings())
            .WithDefaultAuthEndpoint(DefaultEndpoints.Auth)
            .WithDefaultRestEndpoint(DefaultEndpoints.Rest);

        if (String.IsNullOrEmpty(Settings.ApplicationType)) {
            Settings = Settings with { ApplicationType = "server" };
        }

        if (Settings.ApplicationType.Equals("public") || Settings.ApplicationType.Equals("web")) {
            if (String.IsNullOrEmpty(Settings.AuthorizationCode) || String.IsNullOrEmpty(Settings.RedirectURI)) {
                throw new Exception("AuthorizationCode or RedirectURI is null: For Public/Web Apps, AuthCode and Redirect URI must be provided in config file or passed when instantiating ETClient");
            }
        }

        if (Settings.ApplicationType.Equals("public")) {
            if (String.IsNullOrEmpty(Settings.ClientId)) {
                throw new Exception("clientId is null: Must be provided in config file or passed when instantiating ETClient");
            }
        } else {
            if (String.IsNullOrEmpty(Settings.ClientId) || String.IsNullOrEmpty(Settings.ClientSecret)) {
                throw new Exception("clientId or clientSecret is null: Must be provided in config file or passed when instantiating ETClient");
            }
        }

        // If JWT URL Parameter Used
        var organizationFind = false;
        if (refreshState != null) {
            RefreshKey = refreshState.RefreshKey;
            EnterpriseId = refreshState.EnterpriseId;
            OrganizationId = refreshState.OrganizationId;
            #pragma warning disable CS0618
            Stack = refreshState.Stack;
            #pragma warning restore CS0618
            RefreshToken();
        } else if (!String.IsNullOrEmpty(Settings.Jwt)) {
            if (String.IsNullOrEmpty(Settings.AppSignature)) {
                throw new Exception("Unable to utilize JWT for SSO without appSignature: Must be provided in config file or passed when instantiating ETClient");
            }

            var encodedJWT = Settings.Jwt.Trim();
            var decodedJWT = DecodeJWT(encodedJWT, Settings.AppSignature);
            var parsedJWT = JObject.Parse(decodedJWT);
            AuthToken = parsedJWT["request"]["user"]["oauthToken"].Value<string>().Trim();
            AuthTokenExpiration = DateTime.Now.AddSeconds(Int32.Parse(parsedJWT["request"]["user"]["expiresIn"].Value<string>().Trim()));
            InternalAuthToken = parsedJWT["request"]["user"]["internalOauthToken"].Value<string>().Trim();
            RefreshKey = parsedJWT["request"]["user"]["refreshToken"].Value<string>().Trim();
            Jwt = parsedJWT;
            var orgPart = parsedJWT["request"]["organization"];
            if (orgPart != null) {
                EnterpriseId = orgPart["enterpriseId"].Value<string>().Trim();
                OrganizationId = orgPart["id"].Value<string>().Trim();
                #pragma warning disable CS0618
                Stack = orgPart["stackKey"].Value<string>().Trim();
                #pragma warning restore CS0618
            }
        } else {
            RefreshToken();
            organizationFind = true;
        }

        FetchSoapEndpoint();

        // Create the SOAP binding for call with Oauth.
        SoapClient = new SoapClient(GetSoapBinding(), new EndpointAddress(new Uri(Settings.SoapEndPoint)));
        SoapClient.ClientCredentials.UserName.UserName = "*";
        SoapClient.ClientCredentials.UserName.Password = "*";

        // Find Organization Information
        if (organizationFind) {
            using var scope = new OperationContextScope(SoapClient.InnerChannel);

            // Add oAuth token to SOAP header.
            XNamespace ns = "http://exacttarget.com";
            var oauthElement = new XElement(ns + "oAuthToken", InternalAuthToken);
            var xmlHeader = UseOAuth2Authentication
                ? MessageHeader.CreateHeader("fueloauth", "http://exacttarget.com", AuthToken)
                : MessageHeader.CreateHeader("oAuth", "http://exacttarget.com", oauthElement);
            OperationContext.Current.OutgoingMessageHeaders.Add(xmlHeader);

            var httpRequest = new HttpRequestMessageProperty();
            OperationContext.Current.OutgoingMessageProperties.Add(HttpRequestMessageProperty.Name, httpRequest);
            httpRequest.Headers.Add(HttpRequestHeader.UserAgent, SDKVersion);

            var r = SoapClient.Retrieve(
                new RetrieveRequest {
                    ObjectType = "BusinessUnit",
                    Properties = new[] { "ID", "Client.EnterpriseID" }
                },
                out var _,
                out var results
            );

            if (r == "OK" && results.Length > 0) {
                EnterpriseId = results[0].Client.EnterpriseID.ToString();
                OrganizationId = results[0].ID.ToString();
            }
        }
    }


    internal string FetchRestAuth()
    {
        var returnedRestAuthEndpoint = new ETEndpoint { AuthStub = this, Type = "restAuth" }.Get();
        return returnedRestAuthEndpoint.Status && returnedRestAuthEndpoint.Results.Length == 1
            ? ((ETEndpoint)returnedRestAuthEndpoint.Results[0]).URL
            : throw new Exception("REST auth endpoint could not be determined");
    }


    private void FetchSoapEndpoint()
    {
        if (String.IsNullOrEmpty(Settings.SoapEndPoint) || (DateTime.Now > soapEndPointExpiration && fetchedSoapEndpoint != null)) {
            var soapEndPoint = DefaultEndpoints.Soap;
            try {
                var grSingleEndpoint = new ETEndpoint { AuthStub = this, Type = "soap" }.Get();
                if (grSingleEndpoint.Status && grSingleEndpoint.Results.Length == 1) {
                    //Find the appropriate endpoint for the account
                    soapEndPoint = ((ETEndpoint)grSingleEndpoint.Results[0]).URL;
                    fetchedSoapEndpoint = soapEndPoint;
                    soapEndPointExpiration = DateTime.Now.AddMinutes(cacheDurationInMinutes);
                }
            } catch {
                //Ignored
            }
            Settings = Settings with { SoapEndPoint = soapEndPoint };
        }
    }


    private string DecodeJWT(string jwt, string key)
    {
        IJsonSerializer serializer = new JsonNetSerializer();
        IDateTimeProvider provider = new UtcDateTimeProvider();
        IJwtValidator validator = new JwtValidator(serializer, provider);
        IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
        IJwtAlgorithm algorithm = new HMACSHA256Algorithm();
        IJwtDecoder decoder = new JwtDecoder(serializer, validator, urlEncoder, algorithm);
        return decoder.Decode(jwt, key, true);
    }


    private string GetStackFromSoapEndPoint(Uri uri)
    {
        var parts = uri.Host.Split('.');
        return parts.Length < 2 || !parts[0].Equals("webservice", StringComparison.OrdinalIgnoreCase)
            ? throw new Exception(StackKeyErrorMessage)
            : parts[1] == "exacttarget"
                ? "s1"
                : parts[1].ToLower();
    }


    private Binding GetSoapBinding()
    {
        var bindingCollection = new BindingElementCollection();
        if (!UseOAuth2Authentication) {
            bindingCollection.Add(SecurityBindingElement.CreateUserNameOverTransportBindingElement());
        }

        bindingCollection.AddRange(
            new TextMessageEncodingBindingElement {
                MessageVersion = MessageVersion.Soap12WSAddressingAugust2004,
                ReaderQuotas = {
                    MaxDepth = 32,
                    MaxStringContentLength = Int32.MaxValue,
                    MaxArrayLength = Int32.MaxValue,
                    MaxBytesPerRead = Int32.MaxValue,
                    MaxNameTableCharCount = Int32.MaxValue
                }
            },
            new HttpsTransportBindingElement {
                TransferMode = TransferMode.Buffered,
                MaxReceivedMessageSize = 655360000,
                MaxBufferSize = 655360000,
                KeepAliveEnabled = true
            }
        );

        return new CustomBinding(bindingCollection) {
            Name = "UserNameSoapBinding",
            Namespace = "Core.Soap",
            CloseTimeout = TimeSpan.FromMinutes(50),
            OpenTimeout = TimeSpan.FromMinutes(50),
            ReceiveTimeout = TimeSpan.FromMinutes(50),
            SendTimeout = TimeSpan.FromMinutes(50)
        };
    }


    public void RefreshToken(bool force = false)
    {
        if (UseOAuth2Authentication) {
            RefreshTokenWithOauth2(force);
            return;
        }

        // RefreshToken
        if (!String.IsNullOrEmpty(AuthToken) && DateTime.Now.AddSeconds(300) <= AuthTokenExpiration && !force) {
            return;
        }

        // Get an internalAuthToken using clientId and clientSecret
        var authEndpoint = new AuthEndpointUriBuilder(Settings).Build();

        // Build the request
        var request = (HttpWebRequest)WebRequest.Create(authEndpoint.Trim());
        request.Method = "POST";
        request.ContentType = "application/json";
        request.UserAgent = SDKVersion;

        using (var streamWriter = new StreamWriter(request.GetRequestStream())) {
            streamWriter.Write(
                String.IsNullOrEmpty(RefreshKey)
                    ? $"{{\"clientId\": \"{Settings.ClientId}\", \"clientSecret\": \"{Settings.ClientSecret}\", \"accessType\": \"offline\"}}"
                    : $"{{\"clientId\": \"{Settings.ClientId}\", \"clientSecret\": \"{Settings.ClientSecret}\", \"refreshToken\": \"{RefreshKey}\", \"scope\": \"cas:{InternalAuthToken}\" , \"accessType\": \"offline\"}}"
            );
        }

        // Get the response
        string responseFromServer;
        using (var response = (HttpWebResponse)request.GetResponse())
        using (var dataStream = response.GetResponseStream())
        using (var reader = new StreamReader(dataStream)) {
            responseFromServer = reader.ReadToEnd();
        }

        // Parse the response
        var parsedResponse = JObject.Parse(responseFromServer);
        InternalAuthToken = parsedResponse["legacyToken"].Value<string>().Trim();
        AuthToken = parsedResponse["accessToken"].Value<string>().Trim();
        AuthTokenExpiration = DateTime.Now.AddSeconds(int.Parse(parsedResponse["expiresIn"].Value<string>().Trim()));
        RefreshKey = parsedResponse["refreshToken"].Value<string>().Trim();
    }


    internal dynamic CreatePayload(FuelSettings settings)
    {
        dynamic payload = new JObject();

        payload.client_id = settings.ClientId;

        if (!settings.ApplicationType.Equals("public")) {
            payload.client_secret = settings.ClientSecret;
        }

        if (!String.IsNullOrEmpty(RefreshKey)) {
            payload.grant_type = "refresh_token";
            payload.refresh_token = RefreshKey;
        } else if (!settings.ApplicationType.Equals("server")) {
            payload.grant_type = "authorization_code";
            payload.code = settings.AuthorizationCode;
            payload.redirect_uri = settings.RedirectURI;
        } else {
            payload.grant_type = "client_credentials";
        }

        if (!String.IsNullOrEmpty(settings.AccountId)) {
            payload.account_id = settings.AccountId;
        }

        if (!String.IsNullOrEmpty(settings.Scope)) {
            payload.scope = settings.Scope;
        }

        return payload;
    }


    internal void RefreshTokenWithOauth2(bool force = false)
    {
        // RefreshToken
        if (!String.IsNullOrEmpty(AuthToken) && DateTime.Now.AddSeconds(300) <= AuthTokenExpiration && !force) {
            return;
        }

        var authEndpoint = Settings.AuthEndPoint + "/v2/token";
        var request = (HttpWebRequest)WebRequest.Create(authEndpoint.Trim());
        request.Method = "POST";
        request.ContentType = "application/json";
        request.UserAgent = SDKVersion;

        using (var streamWriter = new StreamWriter(request.GetRequestStream())) {
            dynamic payload = CreatePayload(Settings);
            streamWriter.Write(payload.ToString());
        }

        // Get the response
        string responseFromServer;
        using (var response = (HttpWebResponse)request.GetResponse())
        using (var dataStream = response.GetResponseStream())
        using (var reader = new StreamReader(dataStream)) {
            responseFromServer = reader.ReadToEnd();
        }

        // Parse the response
        var parsedResponse = JObject.Parse(responseFromServer);
        AuthToken = parsedResponse["access_token"].Value<string>().Trim();
        InternalAuthToken = parsedResponse["access_token"].Value<string>().Trim();
        AuthTokenExpiration = DateTime.Now.AddSeconds(int.Parse(parsedResponse["expires_in"].Value<string>().Trim()));
        Settings = Settings with {
            RestEndPoint = parsedResponse["rest_instance_url"].Value<string>().Trim(),
            SoapEndPoint = parsedResponse["soap_instance_url"].Value<string>().Trim() + "service.asmx"
        };
        if (parsedResponse["refresh_token"] != null) {
            RefreshKey = parsedResponse["refresh_token"].Value<string>().Trim();
        }
    }


    public FuelReturn AddSubscribersToList(string emailAddress, string subscriberKey, IEnumerable<int> listIDs)
        => ProcessAddSubscriberToList(emailAddress, subscriberKey, listIDs);


    public FuelReturn AddSubscribersToList(string emailAddress, IEnumerable<int> listIDs)
        => ProcessAddSubscriberToList(emailAddress, null, listIDs);


    protected FuelReturn ProcessAddSubscriberToList(string emailAddress, string subscriberKey, IEnumerable<int> listIDs)
    {
        var sub = new ETSubscriber {
            AuthStub = this,
            EmailAddress = emailAddress,
            Lists = listIDs.Select(x => (SubscriberList)new ETSubscriberList { ID = x }).ToArray(),
        };

        if (subscriberKey != null) {
            sub.SubscriberKey = subscriberKey;
        }

        var prAddSub = sub.Post();
        return !prAddSub.Status && prAddSub.Results.Length > 0 && prAddSub.Results[0].ErrorCode == 12014
            ? sub.Patch()
            : prAddSub;
    }


    public FuelReturn CreateDataExtensions(ETDataExtension[] dataExtensions)
    {
        var cleanedArray = new List<APIObject>();
        foreach (var de in dataExtensions) {
            de.AuthStub = this;
            de.Fields = de.Columns;
            de.Columns = null;
            cleanedArray.Add(de);
        }

        return new PostReturn(cleanedArray.ToArray());
    }


    private FuelSettings Settings {
        get => _settings.Value;
        set => _settings = CreateOptions(value);
    }
    private IOptions<FuelSettings> _settings;

    
    private static IOptions<T> CreateOptions<T>(T obj) where T : class
        => Microsoft.Extensions.Options.Options.Create(obj);


    private const long cacheDurationInMinutes = 10;
    private const string StackKeyErrorMessage = "Tenant specific endpoints don't support Stack Key property and this will property will be deprecated in next major release";

    private static DateTime soapEndPointExpiration;
    private static string fetchedSoapEndpoint;
}
