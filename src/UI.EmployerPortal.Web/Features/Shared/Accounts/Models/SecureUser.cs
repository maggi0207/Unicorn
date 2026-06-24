namespace UI.EmployerPortal.Web.Features.Shared.Accounts.Models;

internal sealed record SecureUser
{
    public int SecureUserSK { get; init; }
    public string? UserID { get; init; }
    public string? EmailAddress { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? OktaUUID { get; init; }
    public string? WIUID { get; init; }
    public string? Domain { get; init; }
    public string? WebSessionUUID { get; init; }
}
