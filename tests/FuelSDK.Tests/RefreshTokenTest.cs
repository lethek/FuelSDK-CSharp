using NUnit.Framework;

using FuelSDKCSharp;


namespace FuelSDK
{
    /// <summary>
    /// These tests are specific for OAuth2 Public/Web Apps, So Config should be modified accordingly.
    /// </summary>
    [TestFixture]
    public class RefreshTokenTest : CommonTestFixture
    {
        [OneTimeSetUp]
        public void Setup()
        {
            _client = new ETClient(GetSettings());
            _settings = new FuelSettings {
                AuthorizationCode = "Auth_Code_For_OAuth2_WebApp",
                RedirectURI = "www.google.com",
                ClientId = "OAUTH2_CLIENTID",
                ClientSecret = "OAUTH2_CLIENT_SECRET"
            };
        }

        [Test]
        public void AuthTokenAndRefreshTokenShouldDifferIfRefreshTokenIsEnforced()
        {
            var token = _client.AuthToken;
            var refreshToken = _client.RefreshKey;
            _client.RefreshTokenWithOauth2(true);
            var token1 = _client.AuthToken;
            var refreshToken1 = _client.RefreshKey;


            Assert.AreNotEqual(token, token1);
            Assert.AreNotEqual(refreshToken, refreshToken1);
        }

        [Test]
        public void AuthPayloadShouldHavePublicAppAttributes()
        {
            var settings = _settings with { ApplicationType = "public" };
            dynamic payload = _client.CreatePayload(settings);

            Assert.AreEqual(payload.client_id.ToString(), settings.ClientId);
            Assert.AreEqual(payload.redirect_uri.ToString(), settings.RedirectURI);
            Assert.AreEqual(payload.code.ToString(), settings.AuthorizationCode);
            Assert.AreEqual(payload.grant_type.ToString(), "authorization_code");
        }

        [Test]
        public void AuthPayloadForPublicApp_ShouldNotHaveClientSecret()
        {
            var settings = _settings with { ApplicationType = "public" };
            dynamic payload = _client.CreatePayload(settings);

            Assert.AreEqual(payload.client_secret, null);
        }

        [Test]
        public void AuthPayloadShouldHaveWebAppAttributes()
        {
            var settings = _settings with { ApplicationType = "web" };
            dynamic payload = _client.CreatePayload(settings);

            Assert.AreEqual(payload.grant_type.ToString(), "authorization_code");
            Assert.AreEqual(payload.client_id.ToString(), settings.ClientId);
            Assert.AreEqual(payload.client_secret.ToString(), settings.ClientSecret);
            Assert.AreEqual(payload.redirect_uri.ToString(), settings.RedirectURI);
            Assert.AreEqual(payload.code.ToString(), settings.AuthorizationCode);
        }

        [Test]
        public void AuthPayloadShouldHaveServerAppAttributes()
        {
            var settings = _settings with { ApplicationType = "server" };
            dynamic payload = _client.CreatePayload(settings);

            Assert.AreEqual(payload.grant_type.ToString(), "client_credentials");
            Assert.AreEqual(payload.client_id.ToString(), settings.ClientId);
            Assert.AreEqual(payload.client_secret.ToString(), settings.ClientSecret);
        }

        [Test]
        public void AuthPayloadForServerApp_ShouldNotHaveCodeAndRedirectURI()
        {
            var settings = _settings with { ApplicationType = "server" };
            dynamic payload = _client.CreatePayload(settings);

            Assert.AreEqual(payload.code, null);
            Assert.AreEqual(payload.redirect_uri, null);
        }

        [Test]
        public void AuthPayloadWithRefreshToken_ShouldHaveRefreshTokenAttribute()
        {
            _client.RefreshKey = "REFRESH_KEY";

            var settings = _settings with { ApplicationType = "public" };
            dynamic payload = _client.CreatePayload(settings);

            Assert.AreEqual(payload.grant_type.ToString(), "refresh_token");
            Assert.AreEqual(payload.refresh_token.ToString(), _client.RefreshKey);
        }

        private FuelSettings _settings;
        private ETClient _client;
    }
}
