namespace UI.EmployerPortal.Web.Features.ManageAccount.Services;

/// <summary>
/// IGenerateAccessKeyFlow
/// </summary>
internal interface IGenerateAccessKeyFlow
{
    /// <summary>
    /// SelectedKeyType — "" | "manager" | "worker"
    /// </summary>
    string SelectedKeyType { get; set; }

    /// <summary>
    /// SelectedWebControlSKs — populated for worker flow
    /// </summary>
    int[] SelectedWebControlSKs { get; set; }

    /// <summary>
    /// GeneratedKey — set after the WCF call succeeds
    /// </summary>
    string? GeneratedKey { get; set; }

    /// <summary>
    /// Reset
    /// </summary>
    void Reset();
}

/// <summary>
/// GenerateAccessKeyFlow
/// </summary>
internal class GenerateAccessKeyFlow : IGenerateAccessKeyFlow
{
    /// <inheritdoc/>
    public string SelectedKeyType { get; set; } = string.Empty;

    /// <inheritdoc/>
    public int[] SelectedWebControlSKs { get; set; } = [];

    /// <inheritdoc/>
    public string? GeneratedKey { get; set; }

    /// <inheritdoc/>
    public void Reset()
    {
        SelectedKeyType = string.Empty;
        SelectedWebControlSKs = [];
        GeneratedKey = null;
    }
}
