namespace UI.EmployerPortal.Web.Features.ESP.Models;



/// <summary>
/// A country entry returned from the EFT payment service code table.
/// Carries the short ISO code so the form can reliably identify USA and Canada.
/// </summary>
/// <param name="Value">The CodeSK as a string — used as the dropdown option value.</param>
/// <param name="Text">The long description — displayed in the dropdown.</param>
/// <param name="ShortCode">The short ISO code, e.g. "US" or "CA".</param>
public sealed record BankCountryOption(string Value, string Text, string ShortCode);
