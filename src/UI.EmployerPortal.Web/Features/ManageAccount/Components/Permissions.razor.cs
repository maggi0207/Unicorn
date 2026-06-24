using Microsoft.AspNetCore.Components;
using UI.EmployerPortal.Web.Features.ManageAccount.Models;

namespace UI.EmployerPortal.Web.Features.ManageAccount.Components;

/// <summary>
/// Permissions
/// </summary>
public partial class Permissions
{
    /// <summary>
    /// Groups
    /// </summary>
    [Parameter]
    public List<PermissionGroup> Groups { get; set; } = [];

    /// <summary>
    /// ShowError
    /// </summary>
    [Parameter]
    public bool ShowError { get; set; }

    /// <summary>
    /// OnSelectionChanged
    /// </summary>
    [Parameter]
    public EventCallback OnSelectionChanged { get; set; }

    private bool _selectAllOverride;
    private readonly HashSet<int> _expandedGroups = [];

    /// <inheritdoc/>
    protected override void OnParametersSet()
    {
        // Default every group to expanded the first time we see it.
        foreach (var group in Groups)
        {
            _expandedGroups.Add(group.WebControlSK);
        }
    }

    private bool IsAllSelected
        => _selectAllOverride || (Groups.Count > 0 && Groups.All(g =>
        {
            return g.HasSelectAll
                ? g.Items.Count > 0 && g.Items.All(i =>
                {
                    return i.IsSelected;
                })
                : g.IsSelected;
        }));

    private bool IsExpanded(int webControlSK)
    {
        return _expandedGroups.Contains(webControlSK);
    }

    private void ToggleGroup(int webControlSK)
    {
        if (!_expandedGroups.Add(webControlSK))
        {
            _expandedGroups.Remove(webControlSK);
        }
    }

    private async Task HandleSelectAll(ChangeEventArgs e)
    {
        var isChecked = (bool) (e.Value ?? false);
        _selectAllOverride = isChecked;
        foreach (var group in Groups)
        {
            if (group.HasSelectAll)
            {
                group.IsSelected = isChecked;
                foreach (var item in group.Items)
                {
                    item.IsSelected = isChecked;
                }
            }
            else
            {
                group.IsSelected = isChecked;
            }
        }
        await OnSelectionChanged.InvokeAsync();
    }

    private async Task HandleGroupChange(PermissionGroup group, bool isChecked)
    {
        group.IsSelected = isChecked;
        foreach (var item in group.Items)
        {
            item.IsSelected = isChecked;
        }
        _selectAllOverride = false;
        await OnSelectionChanged.InvokeAsync();
    }

    private async Task HandleItemChange(PermissionGroup group, PermissionItem item, bool isChecked)
    {
        item.IsSelected = isChecked;
        group.IsSelected = group.Items.All(i =>
        {
            return i.IsSelected;
        });
        _selectAllOverride = false;
        await OnSelectionChanged.InvokeAsync();
    }

    private async Task HandleNoChildParentChange(PermissionGroup group, bool isChecked)
    {
        group.IsSelected = isChecked;
        _selectAllOverride = false;
        await OnSelectionChanged.InvokeAsync();
    }

    /// <summary>
    /// GetSelectedWebControlSKs — returns SKs of every selected item, plus the SK of any
    /// no-child parent group that is itself selected (since it's the only selectable row).
    /// </summary>
    /// <returns></returns>
    public int[] GetSelectedWebControlSKs()
    {
        var fromItems = Groups.SelectMany(g =>
        {
            return g.Items.Where(i =>
            {
                return i.IsSelected;
            }).Select(i =>
            {
                return i.WebControlSK;
            });
        });

        var fromNoChildParents = Groups.Where(g =>
        {
            return !g.HasSelectAll && g.IsSelected;
        }).Select(g =>
        {
            return g.WebControlSK;
        });

        return [.. fromItems.Concat(fromNoChildParents)];
    }

    /// <summary>
    /// HasAnySelection
    /// </summary>
    /// <returns></returns>
    public bool HasAnySelection()
    {
        return Groups.Any(g =>
        {
            return g.HasSelectAll
                ? g.Items.Any(i =>
                {
                    return i.IsSelected;
                })
                : g.IsSelected;
        });
    }

    // Dependency-rule SK constants (see WebSecurityControls.txt)
    private const int SK_VIEW = 262;
    private const int SK_EMPLOYER_ABSTRACT_RECERT_REQUEST = 264;          // under Other Tax Permissions
    private const int SK_EMPLOYER_ACCOUNT_ABSTRACT_RECERT = 268;          // under Correspondence
    private const int SK_BENEFIT_CHARGES_ADJUSTMENTS = 267;
    private const int SK_REIMBURSABLE_ASSURANCE = 269;

    /// <summary>
    /// Validate — returns dependency-rule violations for the current selection.
    /// Empty list = valid.
    /// </summary>
    public List<string> Validate()
    {
        var errors = new List<string>();
        var selected = new HashSet<int>(GetSelectedWebControlSKs());

        // Rule 1: 'Employer Abstract Recertification Request 940C' (264) requires
        //         'View' (262) and 'Employer Account Abstract Recertification 940C' (268).
        if (selected.Contains(SK_EMPLOYER_ABSTRACT_RECERT_REQUEST))
        {
            var missing = new List<string>();
            if (!selected.Contains(SK_VIEW))
            {
                missing.Add("'View'");
            }
            if (!selected.Contains(SK_EMPLOYER_ACCOUNT_ABSTRACT_RECERT))
            {
                missing.Add("'Employer Account Abstract Recertification 940C'");
            }
            if (missing.Count > 0)
            {
                errors.Add($"Granting 'Employer Abstract Recertification Request 940C' requires {string.Join(" and ", missing)} to also be selected.");
            }
        }

        // Rule 2: any Correspondence child (267, 268, 269) requires 'View' (262).
        var correspondenceChildren = new[] { SK_BENEFIT_CHARGES_ADJUSTMENTS, SK_EMPLOYER_ACCOUNT_ABSTRACT_RECERT, SK_REIMBURSABLE_ASSURANCE };
        var hasCorrespondenceChild = correspondenceChildren.Any(selected.Contains);
        if (hasCorrespondenceChild && !selected.Contains(SK_VIEW))
        {
            errors.Add("Granting any Correspondence permission requires 'View' to also be selected.");
        }

        return errors;
    }
}
