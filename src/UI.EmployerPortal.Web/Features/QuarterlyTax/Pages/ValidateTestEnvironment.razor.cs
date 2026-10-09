using Microsoft.AspNetCore.Components;


namespace UI.EmployerPortal.Web.Features.QuarterlyTax.Pages;

/// <summary>
/// Home page for File a Taxt and Wage Report Adjustments to select the Adjustment Type
/// </summary>
public partial class ValidateTestEnvironment : ComponentBase
{
    private string? _selectedValue;
    private bool _showValidationError;
    private readonly List<string> _testOptions = new()
    {
        "Test Wage Report File Upload",
        "View Test Wage Report File Upload Summary"

    };

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    private void HandleCardSelected(string label)
    {
        _selectedValue = label;
        _showValidationError = false;
    }

    private void HandleContinue()
    {
        if (string.IsNullOrEmpty(_selectedValue))
        {
            _showValidationError = true;
            return;
        }

        var destination = _selectedValue switch
        {
            "Test Wage Report File Upload" => "quarterly-tax/test/wage-upload",
            "View Test Wage Report File Upload Summary" => "quarterly-tax/test/upload-summary",
            _ => null
        };

        if (destination is not null)
        {
            NavigationManager.NavigateTo(destination);
        }
    }
}
