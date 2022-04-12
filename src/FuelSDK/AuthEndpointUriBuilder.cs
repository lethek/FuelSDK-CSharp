using System;


namespace FuelSDK
{
    public class AuthEndpointUriBuilder
    {
        public AuthEndpointUriBuilder(FuelSettings settings)
            => _settings = settings;


        public string Build()
        {
            var uriBuilder = new UriBuilder(_settings.AuthEndPoint);

            if (uriBuilder.Query.ToLower().Contains(LegacyQuery)) {
                return uriBuilder.Uri.AbsoluteUri;
            }

            uriBuilder.Query = uriBuilder.Query.Length > 1
                ? uriBuilder.Query.Substring(1) + "&" + LegacyQuery
                : LegacyQuery;

            return uriBuilder.Uri.AbsoluteUri;
        }


        private readonly FuelSettings _settings;

        private const string LegacyQuery = "legacy=1";
    }
}
