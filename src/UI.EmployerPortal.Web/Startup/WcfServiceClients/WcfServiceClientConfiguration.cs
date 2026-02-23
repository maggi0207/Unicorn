using System.ServiceModel;

namespace UI.EmployerPortal.Web.Startup.WcfServiceClients;

internal sealed record WcfServiceClientConfiguration
{
    public static readonly WcfServiceClientConfiguration DefaultConfiguration = new()
    {
        ClientCredentialTypeValue = HttpClientCredentialType.Windows.ToString(),
        MaxReceivedMessageSize    = 2_000_000,
        SecurityModeValue         = BasicHttpSecurityMode.Transport.ToString(),
    };

    public string? Url { get; init; }

    public int? MaxReceivedMessageSize { get; init; }

    public string? SecurityModeValue { get; init; }

    public string? ClientCredentialTypeValue { get; init; }

    // Computed from SecurityModeValue — not part of record equality
    internal BasicHttpSecurityMode SecurityMode =>
        Enum.TryParse<BasicHttpSecurityMode>(SecurityModeValue, out var mode)
            ? mode
            : BasicHttpSecurityMode.Transport;

    // Computed from ClientCredentialTypeValue — not part of record equality
    internal HttpClientCredentialType ClientCredentialType =>
        Enum.TryParse<HttpClientCredentialType>(ClientCredentialTypeValue, out var type)
            ? type
            : HttpClientCredentialType.Windows;
}
