using System.IO;
using System.Reflection;

using FuelSDKCSharp;

using Microsoft.Extensions.Configuration;


namespace FuelSDK.Test
{
    public class CustomConfigSectionBasedTest
    {
        protected readonly string emptyConfigFileName = "empty.json";
        protected readonly string missingRequiredAppSignaturePropertyConfigFileName = "missingRequiredAppSignatureProperty.json";
        protected readonly string missingRequiredClientIdConfigFileName = "missingRequiredClientIdProperty.json";
        protected readonly string missingRequiredClientSecretConfigFileName = "missingRequiredClientSecretProperty.json";
        protected readonly string requiredPropertiesOnlyConfigFileName = "requiredPropertiesOnly.json";
        protected readonly string allPropertiesSetConfigFileName = "allPropertiesSet.json";
        protected readonly string allPropertiesSetButAuthEndpointIsEmptyConfigFileName = "allPropertiesSetButAuthEndpointIsEmpty.json";
        protected readonly string allPropertiesSetButRestEndpointIsEmptyConfigFileName = "allPropertiesSetButRestEndpointIsEmpty.json";
        protected readonly string authEndpointMissingLegacyQueryParamFileName = "authEndpointMissingLegacyQueryParam.json";
        protected readonly string authEndpointWithLegacyQueryParamFileName = "authEndpointWithLegacyQueryParam.json";
        protected readonly string authEndpointWithMultipleQueryParamsButMissingLegacyParamFileName = "authEndpointWithMultipleQueryParamsButMissingLegacyParam.json";
        protected readonly string authEndpointWithMultipleQueryParamsIncludingLegacyParamFileName = "authEndpointWithMultipleQueryParamsButMissingLegacyParam.json";


        protected FuelSettings GetCustomConfigurationSectionFromConfigFile(string configFileName)
            => new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(ExecutingAssemblyDir, "ConfigFiles", configFileName))
                .Build()
                .Get<FuelSettings>();


        private string ExecutingAssemblyDir => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    }
}
