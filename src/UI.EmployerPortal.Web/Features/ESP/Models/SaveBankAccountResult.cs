namespace UI.EmployerPortal.Web.Features.ESP.Models;


/// <summary>
/// Represents the outcome of a save bank account operation.
/// </summary>
/// <param name="Success">True if the account was saved without rule violations.</param>
/// <param name="ErrorMessage">The first rule violation message when <paramref name="Success"/> is false.</param>
public sealed record SaveBankAccountResult(bool Success, string? ErrorMessage = null);
