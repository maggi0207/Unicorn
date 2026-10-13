namespace UI.EmployerPortal.Web.Features.ManageAccount.Models;

/// <summary>
/// Per-control display metadata keyed by WebControlSK.
/// The WCF service does not return Sub-Text, so we maintain it here.
/// Update this lookup when new permission rows are added on the backend.
///
/// HasSelectAll used to live here as a hand-maintained set. It does not any more: whether a group
/// needs a "Select All" header follows from how many VISIBLE children it has, so
/// AccountUserService.MapControlsToGroups derives it. Hiding a child can now change how its parent
/// renders, which is the point - hiding Tax Appeals leaves Appeals with one child, and it collapses
/// to a single row rather than stranding a "Select All" above one lone checkbox.
///
/// PermissionMetadataContractTests holds the rest of this file against a captured service response.
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
        [273] = string.Empty,
        // 251 removed: it is the root every top-level group hangs off (ParentWebControlSK = 251) and
        // ObtainAllWebControls never returns it, so this entry could never be read. Its text was a
        // NAME, not sub-text - the same UCB-719 mix-up as the NameOverrides key below.

        // ── Children ─────────────────────────────────────────────────────────
        [258] = "Enter Quarterly Tax report, enter or upload Wage Reports for missing past quarters and current quarter. (Cannot view previously filed reports)",
        [259] = "View and Print previously filed Tax and Wage Reports that have posted to the account.",
        [260] = "Enter adjustments for previously filed tax and wage reports.",
        [261] = "Allow updates to employer information, manage employer addresses, select corporate officer exclusion, close or re-open UI account.",
        [262] = "Provides viewing of account summary, rate information and correspondences.",
        [263] = "Download for viewing and printing Benefit Charges or the Taxable Reserve Fund Statement.",
        [264] = "Generate a Recertification 940C.",
        [265] = "Submit a request for existing unused credit to be refunded.",
        [266] = "Use the Voluntary Contribution Calculator to determine if it is advantageous to make a voluntary contribution.",
        [267] = "View letters that include benefit charge and benefit adjustment information that affected your UI employer account.",
        [268] = "View and print Recertification 940C",
        [269] = string.Empty,   // was the literal text "string.Empty"; masked because 269 is hidden
        [270] = "View and submit Wage and Eligibility Forms",
        [271] = "View and submit Other Eligibility Forms",
        [274] = "View information about benefit appeals, like hearing notices and other documents. Upload evidence and submit withdrawals. Make sure to select \"Benefits and SIDES E-Response\" permission if the worker needs to submit benefit appeals.",
        [275] = "View information about tax appeals, like hearing notices and other documents. Upload evidence and submit withdrawals.",
        [276] = "Enter wage information requested on the UCB-719, Urgent Request for Wages.",
    };

    /// <summary>
    /// Overrides for display name when the WCF-returned Name doesn't match the desired UI label.
    /// Keyed by WebControlSK.
    /// </summary>
    public static readonly IReadOnlyDictionary<int, string> NameOverrides = new Dictionary<int, string>
    {
        [257] = "Benefits and SIDES E-Response",
        [276] = "Complete Urgent Request For Wages (Form UCB-719)"
    };

    /// <summary>
    /// Controls returned by the WCF service that must never be shown as a permission in the UI.
    /// </summary>
    public static readonly IReadOnlySet<int> HiddenWebControlSKs = new HashSet<int>
    {
        269, // Reimbursable Assurance Letters
        275, // Tax Appeals - leaves Appeals with one child, so that group collapses to a single row

        // Benefits and SIDES E-Response is granted by its parent (257) alone. Hiding both children
        // leaves that group with none, so it renders as a single "Select Benefits and SIDES
        // E-Response" row. 257 maps to BenefitsPermissions, which IS these two flags combined, so
        // every policy that reads them stays satisfied.
        270, // Wage and Eligibility Forms
        271, // Other Eligibility Forms
    };

    /// <summary>
    /// Explicit display order for top-level permission groups, since the WCF service does not
    /// guarantee the order the UI wants. Groups not listed here keep their natural (backend) order
    /// after the ones listed here.
    /// </summary>
    private static readonly List<int> RootGroupOrderList = new()
    {
        252, // Quarterly Tax and Wage Reports
        253, // Payments
        254, // UI Account Information
        255, // Other Tax Permissions        
        256, // Correspondence
        276, // Complete Urgent Request for Wages (Form UCB-719)
        257, // Benefits Permissions
        273, // Appeals
        272, // Levy Payments
    };

    /// <summary>
    /// The explicit order above, exposed read-only so it can be checked against what the service
    /// actually returns. Every other table here is public for the same reason.
    /// </summary>
    public static IReadOnlyList<int> RootGroupOrder => RootGroupOrderList;

    /// <summary>
    /// GetRootGroupOrder — index into <see cref="RootGroupOrder"/>, or int.MaxValue if not listed
    /// so unlisted groups sort after the ones with an explicit position.
    /// </summary>
    public static int GetRootGroupOrder(int webControlSK)
    {
        var index = RootGroupOrderList.IndexOf(webControlSK);
        return index == -1 ? int.MaxValue : index;
    }

    /// <summary>
    /// GetIsHidden
    /// </summary>
    public static bool GetIsHidden(int webControlSK)
    {
        return HiddenWebControlSKs.Contains(webControlSK);
    }

    /// <summary>
    /// GetSubText
    /// </summary>
    public static string GetSubText(int webControlSK)
    {
        return SubText.TryGetValue(webControlSK, out var text) ? text : string.Empty;
    }

    /// <summary>
    /// GetDisplayName — returns the override (if any) or the supplied fallback.
    /// </summary>
    public static string GetDisplayName(int webControlSK, string fallback)
    {
        return NameOverrides.TryGetValue(webControlSK, out var name) ? name : fallback;
    }
}
