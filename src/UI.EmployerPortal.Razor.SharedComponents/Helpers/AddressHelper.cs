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
        => CompareField(a.AddressLine1, b.AddressLine1)
        && CompareField(a.AddressLine2, b.AddressLine2)
        && CompareField(a.City, b.City)
        && CompareField(a.State, b.State)
        && CompareField(a.Zip, b.Zip)
        && CompareField(a.Extension, b.Extension);

    private static bool CompareField(string? val1, string? val2)
        => string.Equals(val1?.Trim() ?? string.Empty, val2?.Trim() ?? string.Empty, StringComparison.OrdinalIgnoreCase);
}
