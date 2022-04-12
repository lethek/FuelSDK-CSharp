using NUnit.Framework;
using System;
using System.Linq;

namespace FuelSDK
{
    [TestFixture]
    public class StackKeyTest : CommonTestFixture
    {
        private const string StackKeyErrorMessage = "Tenant specific endpoints don't support Stack Key property and this will property will be deprecated in next major release";

        [Test]
        public void ExceptionOccursIfTSEFormatIsUsedForSoapEndpoint()
        {
            var client = new ETClient(GetSettings());

            #pragma warning disable CS0618
            var exception = Assert.Throws<Exception>(
                () => { var stack = client.Stack; }
            );
            #pragma warning restore CS0618

            Assert.That(exception.Message, Is.EqualTo(StackKeyErrorMessage));
        }

        [Test]
        public void StackPropertyIsMarkedAsObsolete()
        {
            var type = typeof(ETClient);
            var obsoleteAttributes = (ObsoleteAttribute[])type.GetProperty("Stack").GetCustomAttributes(typeof(ObsoleteAttribute), false);

            Assert.AreEqual(1, obsoleteAttributes.Length);
            Assert.AreEqual(StackKeyErrorMessage, obsoleteAttributes[0].Message);
            Assert.AreEqual(false, obsoleteAttributes[0].IsError);
        }
    }
}
