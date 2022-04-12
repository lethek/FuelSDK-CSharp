using NUnit.Framework;

namespace FuelSDK
{
    [TestFixture]
    public class StackKeyTest
    {
        private const string StackKeyErrorMessage = "Tenant specific endpoints don't support Stack Key property and this will property will be deprecated in next major release";

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
