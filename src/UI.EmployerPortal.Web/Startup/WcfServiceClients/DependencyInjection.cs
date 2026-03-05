using System.ServiceModel;
using UI.EmployerPortal.Generated.ServiceClients.AddressValidationService;
using UI.EmployerPortal.Web.Features.EmployerRegistration.Services;

namespace UI.EmployerPortal.Web.Startup.WcfServiceClients;

internal static class DependencyInjection
{
    internal static IServiceCollection AddWcfServiceClients(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var configurations = WcfServiceClientConfigurations.LoadFromConfiguration(configuration);

        // Register the generated SOAP client as a transient (each call gets a fresh client)
        services.AddTransient<IAddressValidationService>(x =>
        {
            var config = configurations.AddressValidationServiceConfiguration
                      ?? WcfServiceClientConfiguration.DefaultConfiguration;

            var binding = CreateBinding(config);

            var endpointAddress = string.IsNullOrEmpty(config.Url)
                ? new AddressValidationServiceClient().Endpoint.Address
                : new EndpointAddress(config.Url);

            return new AddressValidationServiceClient(binding, endpointAddress);
        });

        // Register our wrapper that calls the SOAP client
        services.AddScoped<IAddressValidationWrapper, AddressValidationService>();

        return services;
    }

    private static BasicHttpBinding CreateBinding(WcfServiceClientConfiguration config)
    {
        return new BasicHttpBinding
        {
            Security = new BasicHttpSecurity
            {
                Mode = config.SecurityMode,
                Transport = new HttpTransportSecurity
                {
                    ClientCredentialType = config.ClientCredentialType,
                },
            },
            MaxReceivedMessageSize = config.MaxReceivedMessageSize ?? 2_000_000,
        };
    }
}
