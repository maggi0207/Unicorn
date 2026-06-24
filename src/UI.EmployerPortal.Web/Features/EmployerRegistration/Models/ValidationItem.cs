namespace UI.EmployerPortal.Web.Features.EmployerRegistration.Models;

/// <summary>
/// ValidationItem
/// </summary>
/// <param name="Message"></param>
/// <param name="FieldId"></param>
public record ValidationItem(string Message, string FieldId = "");
