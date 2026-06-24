using Microsoft.AspNetCore.Components;

namespace UI.EmployerPortal.Web.Features.Shared.Layout;

/// <summary>
/// 
/// </summary>
public partial class TagVersion
{
    private const string TagVersionEnvVariableName = "TAG_VERSION";

    /// <summary>
    /// 
    /// </summary>
    [Inject]
    public IConfiguration Configuration { get; set; } = default!;

    /// <summary>
    /// 
    /// </summary>
    [Inject]
    public IWebHostEnvironment Environment { get; set; } = default!;

    private bool _isDisplayed = false;
    private string _tagVersion = string.Empty;

    /// <summary>
    /// 
    /// </summary>
    protected override void OnInitialized()
    {
        _tagVersion = Configuration.GetValue<string?>(TagVersionEnvVariableName) ?? _tagVersion;
        _isDisplayed = Environment.IsDevelopment() && !string.IsNullOrWhiteSpace(_tagVersion);
    }
}
