using UI.EmployerPortal.Razor.SharedComponents.Model;

namespace UI.EmployerPortal.Razor.SharedComponents.Helpers;

/// <summary>
/// Utility methods for comparing and working with <see cref="AddressModel"/> instances.
/// </summary>
public static class AddressHelper
{
    /// <summary>
    /// Returns true when all address fields of <paramref name="a"/> and <paramref name="b"/> are equal (case-insensitive).
    /// Used to detect whether a service-corrected address actually differs from the entered address.
    /// </summary>
    public static bool AddressesAreEqual(AddressModel a, AddressModel b)
        => string.Equals(a.AddressLine1, b.AddressLine1, StringComparison.OrdinalIgnoreCase)
        && string.Equals(a.AddressLine2, b.AddressLine2, StringComparison.OrdinalIgnoreCase)
        && string.Equals(a.City,         b.City,         StringComparison.OrdinalIgnoreCase)
        && string.Equals(a.State,        b.State,        StringComparison.OrdinalIgnoreCase)
        && string.Equals(a.Zip,          b.Zip,          StringComparison.OrdinalIgnoreCase)
        && string.Equals(a.Extension,    b.Extension,    StringComparison.OrdinalIgnoreCase);
}
