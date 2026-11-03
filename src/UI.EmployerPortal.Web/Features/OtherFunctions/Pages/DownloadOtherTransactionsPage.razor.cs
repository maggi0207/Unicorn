using Microsoft.AspNetCore.Components;
using UI.EmployerPortal.Razor.SharedComponents.Inputs;
using UI.EmployerPortal.Web.Features.Dashboard;
using UI.EmployerPortal.Web.Features.OtherFunctions.Models;
using UI.EmployerPortal.Web.Features.OtherFunctions.Services;
namespace UI.EmployerPortal.Web.Features.OtherFunctions.Pages;
/// <summary>
/// Download Other Transactions Page
/// </summary>
public partial class DownloadOtherTransactionsPage
{
    [Inject] private IDashboardOrchestrator DashboardOrchestrator { get; set; } = default!;
    [Inject] private IDownloadTransactionService DownloadTransactionService { get; set; } = default!;
    private bool _isLoading = true;
    /// <summary>
    /// </summary>
    public DownloadTransactionModel DownloadTransactionModel { get; set; } = new();
    ///<inheritdoc />
    protected override async Task OnAuthorizedInitAsync()
    {
        var dateRangeResponse = await DownloadTransactionService.GetBenefitWeekDateRangesAsync();
        if (dateRangeResponse != null)
        {
            DownloadTransactionModel.BenefitDateRangeModel.FiscalYear = new DateModel
            {
                Start = DateOnly.FromDateTime(dateRangeResponse.BenefitWeekDateRanges.FiscalYearStart),
                End = DateOnly.FromDateTime(dateRangeResponse.BenefitWeekDateRanges.FiscalYearEnd)
            };
            DownloadTransactionModel.BenefitDateRangeModel.QuarterYear = new DateModel
            {
                Start = DateOnly.FromDateTime(dateRangeResponse.BenefitWeekDateRanges.QuarterStart),
                End = DateOnly.FromDateTime(dateRangeResponse.BenefitWeekDateRanges.QuarterEnd)
            };
            DownloadTransactionModel.BenefitDateRangeModel.CurrentWeek = new DateModel
            {
                Start = DateOnly.FromDateTime(dateRangeResponse.BenefitWeekDateRanges.WeekStart),
                End = DateOnly.FromDateTime(dateRangeResponse.BenefitWeekDateRanges.WeekEnd)
            };
        }
        DownloadTransactionModel.QuarterOptions = new List<SelectOption>();
        DownloadTransactionModel.QuarterOptions.Add(new SelectOption { Value = "1", Text = "Text (Tab delimited) (*.txt)" });
        DownloadTransactionModel.QuarterOptions.Add(new SelectOption { Value = "2", Text = "CSV (*.csv)" });
        DownloadTransactionModel.QuarterOptions.Add(new SelectOption { Value = "6", Text = "Excel (*.xls)" });
        DownloadTransactionModel.QuarterOptions.Add(new SelectOption { Value = "3", Text = "Excel Workbook (*.xlsx)" });
        DownloadTransactionModel.QuarterOptions.Add(new SelectOption { Value = "4", Text = "Print (*.prn)" });
        DownloadTransactionModel.QuarterOptions.Add(new SelectOption { Value = "5", Text = "XML Data (*.xml)" });

        DownloadTransactionModel.FileTypeSelected = "3"; // default to xlsx

        DownloadTransactionModel.DownloadReportRequest = new DownloadReportRequestModel
        {
            EmployerSK = (await DownloadTransactionService.GetEmployerSK()).ToString(),
            HighSSN = "999999999",
            LowSSN = "10021252",
            SecureUserSK = DownloadTransactionService.GetSecureUserSK()
        };
        _isLoading = false;
    }
}
