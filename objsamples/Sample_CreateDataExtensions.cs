using FuelSDK;

namespace objsamples
{
    partial class Program
    {
        static void Test_CreateDataExtensions()
        {
            var myclient = CreateClient();

            Console.WriteLine("--- Testing CreateDataExtensions ---");

            var deOne = new ETDataExtension
            {
                Name = "HelperDEOne",
                CustomerKey = "HelperDEOne",
                Columns = new[] {
                    new ETDataExtensionColumn { Name = "Name", FieldType = DataExtensionFieldType.Text, IsPrimaryKey = true, MaxLength = 100, IsRequired = true },
                    new ETDataExtensionColumn { Name = "OtherField", FieldType = DataExtensionFieldType.Text } },
            };

            var deTwo = new ETDataExtension
            {
                Name = "HelperDETwo",
                CustomerKey = "HelperDETwo",
                Columns = new[] {
                    new ETDataExtensionColumn { Name = "Name", FieldType = DataExtensionFieldType.Text, IsPrimaryKey = true, MaxLength = 100, IsRequired = true }, 
                    new ETDataExtensionColumn { Name = "OtherField", FieldType = DataExtensionFieldType.Text } },
            };

            var aCoupleDEs = new[] { deOne, deTwo };

            var createReturn = myclient.CreateDataExtensions(aCoupleDEs);
            Console.WriteLine("Helper Status: " + createReturn.Status.ToString());
            Console.WriteLine("Message: " + createReturn.Message);
            Console.WriteLine("Code: " + createReturn.Code.ToString());

            if (createReturn.Status)
            {
                Console.WriteLine("\n Delete DEOne");
                var delDataExtension = new ETDataExtension
                {
                    CustomerKey = "HelperDEOne",
                    AuthStub = myclient,
                };
                var deleteResponse = delDataExtension.Delete();
                Console.WriteLine("Delete Status: " + deleteResponse.Status.ToString());
                Console.WriteLine("Message: " + deleteResponse.Message);
                Console.WriteLine("Code: " + deleteResponse.Code.ToString());
                Console.WriteLine("Results Length: " + deleteResponse.Results.Length);

                Console.WriteLine("\n Delete DETwo");
                var delDataExtension2 = new ETDataExtension
                {
                    CustomerKey = "HelperDETwo",
                    AuthStub = myclient,
                };
                var deleteResponse2 = delDataExtension2.Delete();
                Console.WriteLine("Delete Status: " + deleteResponse2.Status.ToString());
                Console.WriteLine("Message: " + deleteResponse2.Message);
                Console.WriteLine("Code: " + deleteResponse2.Code.ToString());
                Console.WriteLine("Results Length: " + deleteResponse2.Results.Length);
            }
        }
    }
}
