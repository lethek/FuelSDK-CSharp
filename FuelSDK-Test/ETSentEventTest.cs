﻿using NUnit.Framework;
using System;

namespace FuelSDK
{
    class ETSentEventTest : CommonTestFixture
    {
        ETClient client;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            client = new ETClient(GetSettings());
        }

        [Test]
        public void SentEventGet()
        {
            var filterDate = DateTime.Now.AddDays(-30);
            var se = new ETSentEvent
            {
                AuthStub = client,
                SearchFilter = new SimpleFilterPart { Property = "EventDate", SimpleOperator = SimpleOperators.greaterThan, DateValue = new[] { filterDate } },
                Props = new[] { "SendID", "SubscriberKey", "EventDate", "Client.ID", "EventType", "BatchID", "TriggeredSendDefinitionObjectID", "PartnerKey" },
            };
            var response = se.Get();
            Assert.AreEqual(response.Code, 200);
            Assert.AreEqual(response.Status, true);

        }
    }
}
