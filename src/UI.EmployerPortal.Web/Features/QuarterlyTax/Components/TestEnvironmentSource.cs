using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;

namespace UI.EmployerPortal.Web.Features.QuarterlyTax.Components;

/// <summary>
/// Which environment an upload screen is serving.
///
/// The live and test screens are the SAME components, so something has to carry the environment. That
/// used to be <c>?source=test-environment</c>, which was easy to route and easy to lose — a link built
/// without it silently served live data on a screen the user believed was test, and it cost the side
/// menu the ability to tell the two apart at all. The environment now lives in the ROUTE
/// (<c>quarterly-tax/test/…</c>), so it is part of the screen's identity: visible in the address bar,
/// preserved by bookmarks and history, and matchable by the side menu as a plain path.
/// </summary>
public static class TestEnvironmentSource
{
    /// <summary>Route segment that marks a test screen. Everything below it is the test environment.</summary>
    public const string RoutePrefix = "quarterly-tax/test";

    /// <summary>Segment the test routes are nested under.</summary>
    private const string LivePrefix = "quarterly-tax/";


    /// <summary>Legacy query key. Still read so existing bookmarks and portal links keep working.</summary>
    public const string QueryKey = "source";

    /// <summary>Legacy query value.</summary>
    public const string TestValue = "test-environment";

    /// <summary>
    /// Whether the current URL is a test screen — by route, or by the legacy query string.
    /// </summary>
    public static bool IsTest(NavigationManager navigationManager)
    {
        var relative = navigationManager.ToBaseRelativePath(navigationManager.Uri);
        var path = relative.Split('?')[0].Split('#')[0].TrimEnd('/');

        if (IsTestPath(path))
        {
            return true;
        }

        var query = QueryHelpers.ParseQuery(new Uri(navigationManager.Uri).Query);
        return query.TryGetValue(QueryKey, out var source) && IsTest(source);
    }

    /// <summary>
    /// Whether a base-relative path is under the test route.
    /// </summary>
    public static bool IsTestPath(string path)
    {
        var trimmed = path.TrimStart('/');
        return trimmed.Equals(RoutePrefix, StringComparison.OrdinalIgnoreCase)
            || trimmed.StartsWith(RoutePrefix + "/", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Whether a bound <c>source</c> query parameter selects the test environment. Legacy links only —
    /// prefer <see cref="IsTest(NavigationManager)"/>, which also understands the route.
    /// </summary>
    public static bool IsTest(string? source) =>
        string.Equals(source, TestValue, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Maps a live route to its test equivalent, keeping any query string intact.
    /// </summary>
    /// <remarks>
    /// Losing the environment mid-flow is worse than never having it: the user believes they are still
    /// in the test environment while looking at live data. Every hop between the upload screens goes
    /// through here so the environment survives the journey.
    /// </remarks>
    public static string Preserve(string route, bool isTest)
    {
        if (!isTest)
        {
            return route;
        }

        var trimmed = route.TrimStart('/');

        return IsTestPath(trimmed)
            ? trimmed
            : trimmed.StartsWith(LivePrefix, StringComparison.OrdinalIgnoreCase)
            ? string.Concat(RoutePrefix, "/", trimmed.AsSpan(LivePrefix.Length))
            : trimmed;
    }

    /// <summary>
    /// Prefixes a page HEADING in test mode — "Test Tax Report File Upload Details".
    /// </summary>
    /// <remarks>
    /// Plain prose rather than the bracketed tab form: this is the first thing read on the page, and
    /// it is what a user compares against the menu entry they clicked ("Test Tax File Upload Summary").
    /// The banner can be scrolled past; the heading is the answer to "which screen am I on" in a
    /// screenshot, and it survives being read aloud.
    /// </remarks>
    public static string Heading(string heading, bool isTest) => isTest ? $"Test {heading}" : heading;

    /// <summary>
    /// Prefixes a page title in test mode, so the browser tab and history are distinguishable when
    /// both environments are open side by side — which is how the two get confused.
    /// </summary>
    public static string Title(string title, bool isTest) => isTest ? $"[TEST] {title}" : title;
}
