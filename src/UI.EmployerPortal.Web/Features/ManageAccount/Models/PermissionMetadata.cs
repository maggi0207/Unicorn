namespace UI.EmployerPortal.Web.Features.ManageAccount.Models;

/// <summary>
/// Per-control display metadata keyed by WebControlSK.
/// The WCF service does not return Sub-Text or HasSelectAll, so we maintain them here.
/// Update this lookup when new permission rows are added on the backend.
/// </summary>
public static class PermissionMetadata
{
    /// <summary>
    /// SubText shown under each permission's bold label. Empty string = label-only row.
    /// </summary>
    public static readonly IReadOnlyDictionary<int, string> SubText = new Dictionary<int, string>
    {
        // ── Parents ──────────────────────────────────────────────────────────
        [252] = "Includes filing quarterly reports, adjusting existing reports, viewing previously filed reports, elect First Quarter Deferral of taxes.",
        [253] = "Make Electronic Funds Transfer (EFT) payments online or print a payment coupon, view EFT payment history, edit or cancel pending EFT payments, manage EFT bank accounts, manage EFT contact information, view other payment options, view billing detail page.",
        [254] = "Allows requests for updates to and viewing of UI account information.",
        [255] = "Functions include: downloading Taxable Reserve Fund Statements, generating Tax Recertification 940C, requesting a refund of credit, registering a new business, and using the Voluntary Contribution Calculator.",
        [256] = "View and print correspondence mailed in the last six months, or change search criteria for viewing correspondence mailed in the last three years.",
        [257] = "Respond to UI information requests about benefit eligibility, like separations, earnings verifications, and other eligibility issues. View and appeal benefit determinations. Respond to secure messages about benefit eligibility.",
        [272] = "View and respond to levies.",

        // ── Children ─────────────────────────────────────────────────────────
        [258] = "Enter Quarterly Tax report, enter or upload Wage Reports for missing past quarters and current quarter. (Cannot view previously filed reports)",
        [259] = "View and Print previously filed Tax and Wage Reports that have posted to the account.",
        [260] = "Enter adjustments for previously filed tax and wage reports.",
        [261] = "Allow updates to employer information, manage employer addresses, elect corporate officer exclusion, close or re-open UI account.",
        [262] = "Provides viewing of account summary, rate information and correspondences.",
        [263] = "Download for viewing and printing Benefit Charges or the Taxable Reserve Fund Statement.",
        [264] = "Generate a Recertification 940C.",
        [265] = "Submit a request for existing unused credit to be refunded.",
        [266] = "Use the Voluntary Contribution Calculator to determine if it is advantageous to make a voluntary contribution.",
        [267] = "View letters that include benefit charge and benefit adjustment information that affected your UI employer account.",
        [268] = "View and print Recertification 940C",
        [269] = string.Empty,
        [270] = "View and submit Wage and Eligibility Forms",
        [271] = "View and submit Other Eligibility Forms",
    };

    /// <summary>
    /// Parents that have a "Select All [GroupName]" checkbox with child items underneath.
    /// Parents not in this set are treated as no-child parents — the parent itself is the
    /// single selectable row ("Select [GroupName]"), with no children.
    /// </summary>
    public static readonly IReadOnlySet<int> HasSelectAll = new HashSet<int>
    {
        252, // Quarterly Tax and Wage Reports
        254, // UI Account Information
        255, // Other Tax Permissions
        256, // Correspondence
        257, // Benefits Permissions
    };

    /// <summary>
    /// Overrides for display name when the WCF-returned Name doesn't match the desired UI label.
    /// Keyed by WebControlSK.
    /// </summary>
    public static readonly IReadOnlyDictionary<int, string> NameOverrides = new Dictionary<int, string>
    {
        [257] = "Benefits and SIDES E-Response",
    };

    /// <summary>
    /// GetSubText
    /// </summary>
    public static string GetSubText(int webControlSK)
    {
        return SubText.TryGetValue(webControlSK, out var text) ? text : string.Empty;
    }

    /// <summary>
    /// GetHasSelectAll
    /// </summary>
    public static bool GetHasSelectAll(int webControlSK)
    {
        return HasSelectAll.Contains(webControlSK);
    }

    /// <summary>
    /// GetDisplayName — returns the override (if any) or the supplied fallback.
    /// </summary>
    public static string GetDisplayName(int webControlSK, string fallback)
    {
        return NameOverrides.TryGetValue(webControlSK, out var name) ? name : fallback;
    }
}
