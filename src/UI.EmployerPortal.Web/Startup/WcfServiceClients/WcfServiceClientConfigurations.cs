namespace UI.EmployerPortal.Web.Startup.WcfServiceClients;

internal sealed record WcfServiceClientConfigurations
{
    public WcfServiceClientConfiguration? AddressValidationServiceConfiguration { get; init; }

    public static WcfServiceClientConfigurations LoadFromConfiguration(IConfiguration configuration)
    {
        var sectionName = "WcfServiceClient";
        return new WcfServiceClientConfigurations
        {
            AddressValidationServiceConfiguration = LoadConfiguration(sectionName, "AddressValidationService", configuration),
        };
    }

    private static WcfServiceClientConfiguration LoadConfiguration(
        string sectionName, string serviceName, IConfiguration configuration)
    {
        return new WcfServiceClientConfiguration
        {
            Url                       = configuration.GetValue<string?>($"{sectionName}:{serviceName}:Url"),
            MaxReceivedMessageSize    = configuration.GetValue<int?>($"{sectionName}:{serviceName}:MaxReceivedMessageSize"),
            ClientCredentialTypeValue = configuration.GetValue<string?>($"{sectionName}:{serviceName}:ClientCredentialType"),
            SecurityModeValue         = configuration.GetValue<string?>($"{sectionName}:{serviceName}:SecurityMode"),
        };
    }
}
