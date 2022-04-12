using System;


namespace FuelSDK;

public static class FuelSettingsExtensions
{
    /// <summary>
    /// Returns a new instance with the AuthEndPoint set to a specified default value if it was not already set.
    /// </summary>
    /// <param name="defaultAuthEndpoint">The default auth endpoint</param>
    /// <returns>The new <see cref="FuelSettings"/> instance</returns>
    public static FuelSettings WithDefaultAuthEndpoint(this FuelSettings self, string defaultAuthEndpoint)
        => String.IsNullOrEmpty(self.AuthEndPoint)
            ? self with { AuthEndPoint = defaultAuthEndpoint }
            : self with { };


    /// <summary>
    /// Returns a new instance with the RestEndPoint set to a specified default value if it was not already set.
    /// </summary>
    /// <param name="defaultRestEndpoint">The default auth endpoint</param>
    /// <returns>The new <see cref="FuelSettings"/> instance</returns>
    public static FuelSettings WithDefaultRestEndpoint(this FuelSettings self, string defaultRestEndpoint)
        => String.IsNullOrEmpty(self.RestEndPoint)
            ? self with { RestEndPoint = defaultRestEndpoint }
            : self with { };
}
