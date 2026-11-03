namespace UI.EmployerPortal.Web.Features.OtherFunctions.Models;

/// <summary>
/// UI document correspondence model.
/// </summary>
public class UIDocument
{
    /// <summary>
    /// Status Column
    /// </summary>
    public string Status { get; set; } = "View";

    /// <summary>
    /// Form Column
    /// </summary>
    public string Form { get; set; } = string.Empty;

    /// <summary>
    /// Description column
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Date Column
    /// </summary>
    public DateTime Date { get; set; }


    /// <summary>
    /// Id Column
    /// </summary>
    public string DocumentId { get; set; } = string.Empty;

    /// <summary>
    /// Generation Sk
    /// </summary>
    public int GenerationSK { get; set; }

}
