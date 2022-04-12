using NUnit.Framework;
using System;
using System.Reflection;

using FuelSDKCSharp;

using Microsoft.Extensions.Configuration;


namespace FuelSDK.Test
{
    [TestFixture]
    class ETClientTest : CommonTestFixture
    {
        [Test]
        public void TestSoapEndpointCaching()
        {
            var settings = GetSettings();

            if (settings.UseOAuth2Authentication) {
                // Test does not apply for legacy authentication
                Assert.Pass();
                return;
            }

            var client1 = new ETClient(settings);
            var client2 = new ETClient(settings);

            var client1SoapEndpointExpirationField = client1.GetType().GetField("soapEndPointExpiration", BindingFlags.NonPublic | BindingFlags.Static);
            var client2SoapEndpointExpirationField = client2.GetType().GetField("soapEndPointExpiration", BindingFlags.NonPublic | BindingFlags.Static);

            var client1SoapEndpointExpiration = (DateTime)client1SoapEndpointExpirationField.GetValue(null);
            var client2SoapEndpointExpiration = (DateTime)client2SoapEndpointExpirationField.GetValue(null);

            Assert.IsTrue(client1SoapEndpointExpiration > DateTime.MinValue);
            Assert.IsTrue(client2SoapEndpointExpiration > DateTime.MinValue);
            Assert.AreEqual(client1SoapEndpointExpiration, client2SoapEndpointExpiration);
        }
    }
}
