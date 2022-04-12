using FuelSDK;

namespace objsamples
{
    partial class Program
    {
        static void TestET_Endpoint()
        {
            var myclient = CreateClient();

            Console.WriteLine("--- Testing Endpoint ---");

            Console.WriteLine("\n Retrieve All Endpoints");
            var getEndpoint = new ETEndpoint
            {
                AuthStub = myclient,
            };
            var grEndpoint = getEndpoint.Get();

            Console.WriteLine("Get Status: " + grEndpoint.Status.ToString());
            Console.WriteLine("Message: " + grEndpoint.Message);
            Console.WriteLine("Code: " + grEndpoint.Code.ToString());
            Console.WriteLine("Results Length: " + grEndpoint.Results.Length);
            Console.WriteLine("MoreResults: " + grEndpoint.MoreResults.ToString());

            Console.WriteLine("\n Retrieve Single Endpoint by Type");
            var getSingleEndpoint = new ETEndpoint
            {
                AuthStub = myclient,
                Type = "soap",
            };
            var grSingleEndpoint = getSingleEndpoint.Get();

            Console.WriteLine("Get Status: " + grSingleEndpoint.Status.ToString());
            Console.WriteLine("Message: " + grSingleEndpoint.Message);
            Console.WriteLine("Code: " + grSingleEndpoint.Code.ToString());
            Console.WriteLine("Results Length: " + grSingleEndpoint.Results.Length);
            Console.WriteLine("MoreResults: " + grSingleEndpoint.MoreResults.ToString());
        }
    }
}
