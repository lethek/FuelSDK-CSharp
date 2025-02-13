﻿using Microsoft.Extensions.Configuration;

namespace FuelSDK;

public abstract class CommonTestFixture
{
    protected FuelSettings GetSettings()
        => new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddUserSecrets<CommonTestFixture>(true)
            .AddEnvironmentVariables("FUELSDK_")
            .Build()
            .Get<FuelSettings>();
}
