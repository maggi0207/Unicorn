namespace UI.EmployerPortal.Web.Features.ManageAccount.Models;

/// <summary>
/// PermissionGroup
/// </summary>
public class PermissionGroup
{
    /// <summary>
    /// WebControlSK
    /// </summary>
    public int WebControlSK { get; set; }

    /// <summary>
    /// Name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// IsSelected
    /// </summary>
    public bool IsSelected { get; set; }

    /// <summary>
    /// Items
    /// </summary>
    public List<PermissionItem> Items { get; set; } = [];

    /// <summary>
    /// SubText — description shown under the group label
    /// </summary>
    public string SubText { get; set; } = string.Empty;

    /// <summary>
    /// HasSelectAll — when true, the group is rendered as a section with a "Select All [Name]"
    /// checkbox followed by child items. When false, the group itself is the only selectable
    /// row ("Select [Name]") and no children are rendered.
    /// </summary>
    public bool HasSelectAll { get; set; } = true;

    /// <summary>
    /// IsInterminate
    /// </summary>
    public bool IsIndeterminate => Items.Any(i =>
    {
        return i.IsSelected;
    }) && !Items.All(i =>
    {
        return i.IsSelected;
    });
}

/// <summary>
/// PermissionItem
/// </summary>
public class PermissionItem
{
    /// <summary>
    /// WebControlSK
    /// </summary>
    public int WebControlSK { get; set; }

    /// <summary>
    /// Name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// SubText — description shown under the item label
    /// </summary>
    public string SubText { get; set; } = string.Empty;

    /// <summary>
    /// IsSelected
    /// </summary>
    public bool IsSelected { get; set; }
}
