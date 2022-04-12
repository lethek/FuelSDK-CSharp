using FuelSDK;

namespace objsamples
{
    partial class Program
    {
        static void TestET_Subscriber()
        {
            var myclient = CreateClient();

            Console.WriteLine("--- Testing Subscriber ---");
            var subscriberTestEmail = "CSharpSDKExample223@bh.exacttarget.com";

            Console.WriteLine("\n Create Subscriber");
            var postSub = new ETSubscriber
            {
                AuthStub = myclient,
                EmailAddress = subscriberTestEmail,
                Attributes = new[] { new ETProfileAttribute { Name = "First Name", Value = "ExactTarget Example" } },
            };
            var postResponse = postSub.Post();
            Console.WriteLine("Post Status: " + postResponse.Status.ToString());
            Console.WriteLine("Message: " + postResponse.Message);
            Console.WriteLine("Code: " + postResponse.Code.ToString());
            Console.WriteLine("Results Length: " + postResponse.Results.Length);

            if (postResponse.Results.Length > 0)
            {
                Console.WriteLine("--NewID: " + postResponse.Results[0].NewID.ToString());
                foreach (ETProfileAttribute attr in ((ETSubscriber)postResponse.Results[0].Object).Attributes)
                    Console.WriteLine("Name: " + attr.Name + ", Value: " + attr.Value);
            }

            Console.WriteLine("\n Retrieve newly created Subscriber");
            var getSub = new ETSubscriber
            {
                AuthStub = myclient,
                Props = new[] { "SubscriberKey", "EmailAddress", "Status" },
                SearchFilter = new SimpleFilterPart { Property = "SubscriberKey", SimpleOperator = SimpleOperators.equals, Value = new[] { subscriberTestEmail } },
            };
            var getResponse = getSub.Get();
            Console.WriteLine("Get Status: " + getResponse.Status.ToString());
            Console.WriteLine("Message: " + getResponse.Message);
            Console.WriteLine("Code: " + getResponse.Code.ToString());
            Console.WriteLine("Results Length: " + getResponse.Results.Length);
            foreach (ETSubscriber sub in getResponse.Results)
                Console.WriteLine("--EmailAddress: " + sub.EmailAddress + " Status: " + sub.Status.ToString());

            Console.WriteLine("\n Update Subscriber");
            var patchSub = new ETSubscriber
            {
                AuthStub = myclient,
                EmailAddress = subscriberTestEmail,
                Status = SubscriberStatus.Unsubscribed,
                Attributes = new[] { new ETProfileAttribute { Name = "First Name", Value = "ExactTarget Example" } },
            };
            var pathResponse = patchSub.Patch();
            Console.WriteLine("Patch Status: " + pathResponse.Status.ToString());
            Console.WriteLine("Message: " + pathResponse.Message);
            Console.WriteLine("Code: " + pathResponse.Code.ToString());
            Console.WriteLine("Results Length: " + pathResponse.Results.Length);
            foreach (ResultDetail rd in pathResponse.Results)
            {
                var sub = (ETSubscriber)rd.Object;
                Console.WriteLine("--EmailAddress: " + sub.EmailAddress + " Status: " + sub.Status.ToString());
            }

            Console.WriteLine("\n Retrieve Subscriber that should have status unsubscribed now");
            getResponse = getSub.Get();
            Console.WriteLine("Get Status: " + getResponse.Status.ToString());
            Console.WriteLine("Message: " + getResponse.Message);
            Console.WriteLine("Code: " + getResponse.Code.ToString());
            Console.WriteLine("Results Length: " + getResponse.Results.Length);
            foreach (ETSubscriber sub in getResponse.Results)
                Console.WriteLine("--EmailAddress: " + sub.EmailAddress + " Status: " + sub.Status.ToString());

            Console.WriteLine("\n Delete Subscriber");
            var deleteSub = new ETSubscriber
            {
                AuthStub = myclient,
                EmailAddress = subscriberTestEmail,
            };
            var deleteResponse = deleteSub.Delete();
            Console.WriteLine("Delete Status: " + deleteResponse.Status.ToString());
            Console.WriteLine("Message: " + deleteResponse.Message);
            Console.WriteLine("Code: " + deleteResponse.Code.ToString());
            Console.WriteLine("Results Length: " + deleteResponse.Results.Length);

            Console.WriteLine("\n Retrieve Subscriber to confirm deletion");
            getResponse = getSub.Get();
            Console.WriteLine("Get Status: " + getResponse.Status.ToString());
            Console.WriteLine("Message: " + getResponse.Message);
            Console.WriteLine("Code: " + getResponse.Code.ToString());
            Console.WriteLine("Results Length: " + getResponse.Results.Length);
            foreach (ETSubscriber sub in getResponse.Results)
                Console.WriteLine("--EmailAddress: " + sub.EmailAddress + " Status: " + sub.Status.ToString());
        }
    }
}
