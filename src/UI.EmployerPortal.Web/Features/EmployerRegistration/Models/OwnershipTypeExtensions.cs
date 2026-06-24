using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace UI.EmployerPortal.Web.Features.EmployerRegistration.Models;

/// <summary>
/// Extension methods for OwnershipType enum
/// </summary>
public static class OwnershipTypeExtensions
{
    /// <summary>
    /// Gets the display name from the Display attribute
    /// </summary>
    public static string GetDisplayName(this OwnershipType ownershipType)
    {
        var displayAttribute = ownershipType
            .GetType()
            .GetMember(ownershipType.ToString())
            .FirstOrDefault()
            ?.GetCustomAttribute<DisplayAttribute>();

        return displayAttribute?.Name ?? ownershipType.ToString();
    }
}
