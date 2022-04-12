using NUnit.Framework;

namespace FuelSDK.Test
{
    [TestFixture]
    public class AuthEndpointUriBuilderTest : CustomConfigSectionBasedTest
    {
        [Test]
        public void BuilderAddsLegacyQueryParamWhenNoParamsArePresent()
        {
            var configSection = GetCustomConfigurationSectionFromConfigFile(authEndpointMissingLegacyQueryParamFileName);

            AuthEndpointUriBuilder builder = new AuthEndpointUriBuilder(configSection);
            var uri = builder.Build();

            Assert.AreEqual("https://authendpoint.com/v1/requestToken?legacy=1", uri);
        }

        [Test]
        public void BuilderReturnsCorrectAuthEndpointUriWhenLegacyQueryIsPresent()
        {
            var configSection = GetCustomConfigurationSectionFromConfigFile(authEndpointWithLegacyQueryParamFileName);

            AuthEndpointUriBuilder builder = new AuthEndpointUriBuilder(configSection);
            var uri = builder.Build();

            Assert.AreEqual("https://authendpoint.com/v1/requestToken?legacy=1", uri);
        }

        [Test]
        public void BuilderAddsLegacyQueryParamWhenOtherParamsArePresent()
        {
            var configSection = GetCustomConfigurationSectionFromConfigFile(authEndpointWithMultipleQueryParamsButMissingLegacyParamFileName);

            AuthEndpointUriBuilder builder = new AuthEndpointUriBuilder(configSection);
            var uri = builder.Build();

            Assert.AreEqual("https://authendpoint.com/v1/requestToken?param1=1&legacy=1", uri);
        }

        [Test]
        public void BuilderReturnsCorrectAuthEndpointUriWhenLegacyQueryIsPresentAlongwithOtherQueryParams()
        {
            var configSection = GetCustomConfigurationSectionFromConfigFile(authEndpointWithMultipleQueryParamsIncludingLegacyParamFileName);

            AuthEndpointUriBuilder builder = new AuthEndpointUriBuilder(configSection);
            var uri = builder.Build();

            Assert.AreEqual("https://authendpoint.com/v1/requestToken?param1=1&legacy=1", uri);
        }
    }
}
