using System.ComponentModel.DataAnnotations;

using NUnit.Framework;

namespace FuelSDK
{
    [TestFixture]
    class FuelSettingsTest : CommonTestFixture
    {
        [Test]
        public void NoCustomConfigSection()
        {
            var section = GetCustomConfigurationSectionFromConfigFile(emptyConfigFileName);
            Assert.IsNull(section);
        }

        [Test]
        public void MissingRequiredAppSignaturePropertyFromConfigSection()
        {
            var settings = GetCustomConfigurationSectionFromConfigFile(missingRequiredAppSignaturePropertyConfigFileName);
            Assert.That(() => Validator.ValidateObject(settings, new ValidationContext(settings), true), Throws.TypeOf<ValidationException>());
        }

        [Test]
        public void MissingRequiredClientIdPropertyFromConfigSection()
        {
            var settings = GetCustomConfigurationSectionFromConfigFile(missingRequiredClientIdConfigFileName);
            Assert.That(() => Validator.ValidateObject(settings, new ValidationContext(settings), true), Throws.TypeOf<ValidationException>());
        }

        [Test]
        public void MissingSoapEndPointPropertyFromConfigSection()
        {
            var section = GetCustomConfigurationSectionFromConfigFile(requiredPropertiesOnlyConfigFileName);
            Assert.IsNull(section.SoapEndPoint);
        }

        [Test]
        public void MissingAuthEndPointPropertyFromConfigSection()
        {
            var section = GetCustomConfigurationSectionFromConfigFile(requiredPropertiesOnlyConfigFileName);
            Assert.IsNull(section.AuthEndPoint);
        }

        [Test]
        public void MissingRestEndPointPropertyFromConfigSection()
        {
            var section = GetCustomConfigurationSectionFromConfigFile(requiredPropertiesOnlyConfigFileName);
            Assert.IsNull(section.RestEndPoint);
        }

        [Test]
        public void AllPropertiesSetInConfigSection()
        {
            var section = GetCustomConfigurationSectionFromConfigFile(allPropertiesSetConfigFileName);
            Assert.AreEqual(section.AppSignature, "none");
            Assert.AreEqual(section.ClientId, "abc");
            Assert.AreEqual(section.ClientSecret, "cde");
            Assert.AreEqual(section.SoapEndPoint, "https://soapendpoint.com");
            Assert.AreEqual(section.AuthEndPoint, "https://authendpoint.com");
            Assert.AreEqual(section.RestEndPoint, "https://restendpoint.com");
        }

        [Test]
        public void AllPropertiesSetButAuthEndpointIsEmpty()
        {
            var section = GetCustomConfigurationSectionFromConfigFile(allPropertiesSetButAuthEndpointIsEmptyConfigFileName);
            var sectionWithDefaultAuthEndpoint = section.WithDefaultAuthEndpoint(DefaultEndpoints.Auth);

            Assert.AreEqual(DefaultEndpoints.Auth, sectionWithDefaultAuthEndpoint.AuthEndPoint);
        }

        [Test]
        public void AllPropertiesSetButRestEndpointIsEmpty()
        {
            var section = GetCustomConfigurationSectionFromConfigFile(allPropertiesSetButRestEndpointIsEmptyConfigFileName);
            var sectionWithDefaultRestEndpoint = section.WithDefaultRestEndpoint(DefaultEndpoints.Rest);

            Assert.AreEqual(DefaultEndpoints.Rest, sectionWithDefaultRestEndpoint.RestEndPoint);
        }

        [Test]
        public void WithDefaultsDoesNotOverwriteValuesSetInConfig()
        {
            var section = GetCustomConfigurationSectionFromConfigFile(allPropertiesSetConfigFileName);
            section = section
                .WithDefaultRestEndpoint(DefaultEndpoints.Rest)
                .WithDefaultAuthEndpoint(DefaultEndpoints.Auth);

            Assert.AreEqual(section.AuthEndPoint, "https://authendpoint.com");
            Assert.AreEqual(section.RestEndPoint, "https://restendpoint.com");
        }
    }
}
