using System;


namespace FuelSDK;

public static class FuelSettingsExtensions
{
    /// <summary>
    /// If AuthEndPoint is not already set, returns a new instance with it set to the specified default.
    /// </summary>
    /// <param name="defaultAuthEndpoint">The default auth endpoint</param>
    /// <returns>The new <see cref="FuelSettings"/> instance</returns>
    public static FuelSettings WithDefaultAuthEndpoint(this FuelSettings self, string defaultAuthEndpoint)
        => String.IsNullOrEmpty(self.AuthEndPoint)
            ? self with { AuthEndPoint = defaultAuthEndpoint }
            : self;


    /// <summary>
    /// If RestEndPoint is not already set, returns a new instance with it set to the specified default.
    /// </summary>
    /// <param name="defaultRestEndpoint">The default auth endpoint</param>
    /// <returns>The new <see cref="FuelSettings"/> instance</returns>
    public static FuelSettings WithDefaultRestEndpoint(this FuelSettings self, string defaultRestEndpoint)
        => String.IsNullOrEmpty(self.RestEndPoint)
            ? self with { RestEndPoint = defaultRestEndpoint }
            : self;
}
