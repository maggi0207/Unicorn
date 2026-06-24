using Microsoft.AspNetCore.Components;
using Microsoft.FeatureManagement;

namespace UI.EmployerPortal.Web.Features.Shared.FeatureFlagging;

/// <summary>
/// 
/// </summary>
public partial class Feature
{
    /// <summary>
    /// 
    /// </summary>
    [Inject]
    public IFeatureManager FeatureManager { get; set; } = default!;

    /// <summary>
    /// 
    /// </summary>
    [Parameter]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 
    /// </summary>
    [Parameter]
    public string[] Names { get; set; } = [];

    /// <summary>
    /// 
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    private bool _isFeatureEnabled;

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    protected override async Task OnInitializedAsync()
    {
        _isFeatureEnabled = Names.Length == 0
            ? await FeatureManager.IsEnabledAsync(Name)
            : await IsAnyFeatureEnabledAsync([.. Names, Name]);
    }

    private async Task<bool> IsAnyFeatureEnabledAsync(string[] names)
    {
        var tasks = names
            .Where(x =>
            {
                return !string.IsNullOrWhiteSpace(x);
            })
            .Select(x =>
            {
                return FeatureManager.IsEnabledAsync(x);
            });

        var results = await Task.WhenAll(tasks);

        return results.Any(x =>
        {
            return x == true;
        });
    }
}
