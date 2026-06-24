using UI.EmployerPortal.Generated.ServiceClients.TaxWageReportingService;
using UI.EmployerPortal.Web.Features.QuarterlyTax.Pages;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Services;
using UI.EmployerPortal.Web.Features.Shared.QuarterlyTax.Models;
using UI.EmployerPortal.Web.Startup.ResiliencyProtocols;

namespace UI.EmployerPortal.Web.Features.Shared.QuarterlyTax.Services;

/// <summary>
/// Defines the method for retrieving wage file upload summary and detail information
/// from the Tax Wage Reporting backend service
/// </summary>
public interface IWageFileUploadDetailService
{
    /// <summary>
    /// Retrieves a list of wage file uploads for a given user
    /// </summary>
    Task<IEnumerable<WageFileModel>> LoadWageFileUploads();

    /// <summary>
    /// Retrieves a list of test wage file uploads for a given user
    /// </summary>
    Task<IEnumerable<WageFileModel>> LoadTestWageFileUploads();

    /// <summary>
    /// Retrieves detail information for a specific wage file upload.
    /// </summary>
    /// <param name="fileUploadSK">
    ///The unique identifier (primary key) of the wage file upload.
    /// </param>
    /// <returns></returns>
    Task<WageUploadDetails?> LoadWageFileUploadDetails(int fileUploadSK);
}

/// <summary>
/// Provides the implementation for retrieving the wage file upload data
/// from Tax Wage Reporting WCF Service.
/// </summary>
internal class WageFileUploadDetailService : IWageFileUploadDetailService
{
    private readonly ITaxWageReportingService _taxWageUtilityService;
    private readonly IAsyncRetryPolicy<WageFileUploadDetailService> _retryPolicy;
    private readonly IUserAccountService _userAccountService;

    /// <summary> Initializes a new instance of the service class </summary>
    public WageFileUploadDetailService(
        ITaxWageReportingService taxWageUtilityService,
        IUserAccountService userAccountService,
        IAsyncRetryPolicy<WageFileUploadDetailService> retryPolicy)
    {
        _taxWageUtilityService = taxWageUtilityService;
        _userAccountService = userAccountService;
        _retryPolicy = retryPolicy;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<WageFileModel>> LoadWageFileUploads()
    {
        var result = new List<WageFileModel>();

        var request = new TaxWageFileUploadListRequest
        {
            SecureUserSK = _userAccountService.GetUserSKClaim()
        };

        var response = await _retryPolicy.ExecuteAsync(() =>
        {
            return _taxWageUtilityService.LoadWageFileUploadsAsync(request);
        });

        if (response?.WageFileUploadSummaries == null)
        {
            return result;
        }

        result = response.WageFileUploadSummaries.Where(x =>
        {
            return x != null;
        }).Select(MapSummaryProxyToModel).ToList();

        return result;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<WageFileModel>> LoadTestWageFileUploads()
    {
        var result = new List<WageFileModel>();

        var request = new TaxWageFileUploadListRequest
        {
            SecureUserSK = _userAccountService.GetUserSKClaim()
        };

        var response = await _retryPolicy.ExecuteAsync(() =>
        {
            return _taxWageUtilityService.LoadTestWageFileUploadsAsync(request);
        });

        if (response?.WageFileUploadSummaries == null)
        {
            return result;
        }

        result = response.WageFileUploadSummaries.Where(x =>
        {
            return x != null;
        }).Select(MapSummaryProxyToModel).ToList();

        return result;
    }

    /// <inhertDoc/>
    public async Task<WageUploadDetails?> LoadWageFileUploadDetails(int fileUploadSK)
    {
        if (fileUploadSK <= 0)
        {
            return null;
        }

        var request = new TaxWageFileUploadRequest
        {
            FileUploadSK = fileUploadSK
        };

        var response = await _retryPolicy.ExecuteAsync(() =>
        {
            return _taxWageUtilityService.LoadTaxWageFileUploadDetailAsync(request);
        });

        var proxy = response?.FileUploadDetails?.FirstOrDefault();
        if (proxy == null)
        {
            return null;
        }

        var model = MapDetailProxyToModel(proxy);

        model.FatalErrorRows = proxy.FatalErrors != null ? proxy.FatalErrors.Where(x =>
        {
            return x != null;
        }).Select(x =>
        {
            return MapErrorProxyToModel(x);
        })
        .ToList() : new List<WageUploadErrorRow>();

        model.NonFatalErrorRows = proxy.NonFatalErrors != null ? proxy.NonFatalErrors.Where(x =>
        {
            return x != null;
        }).Select(x =>
        {
            return MapErrorProxyToModel(x);
        })
            .ToList() : new List<WageUploadErrorRow>();

        BuildMessages(model);

        return model;
    }


    #region Mapping Methods
    private static WageFileModel MapSummaryProxyToModel(WageFileUploadSummaryProxy proxy)
    {
        return new WageFileModel
        {
            FileUploadSK = proxy.FileUploadSK,
            UploadDate = proxy.UploadDate,
            ContactName = proxy.ContactName ?? string.Empty,
            FileName = proxy.FileName ?? string.Empty,
            FileType = proxy.FileType ?? string.Empty,
            ConfirmationNumber = proxy.ConfirmationNumber ?? string.Empty,
            ErrorCount = proxy.ErrorCount,
            ProcessedDate = proxy.ProcessedDate
        };
    }

    private static WageUploadDetails MapDetailProxyToModel(FileUploadDetailProxy proxy)
    {
        var model = new WageUploadDetails
        {
            FileUploadSKDetail = (int) (proxy.FileUploadDetailSK ?? 0),
            FileName = proxy.OriginalFileName ?? string.Empty,
            UploadDate = proxy.UploadDate,
            UploadedBy = proxy.ADUserName ?? string.Empty,
            FileType = proxy.FileUploadTypeCodeDescription,
            FileSize = (int) (proxy.FileSizeBytes ?? 0),
            RecordCount = proxy.FileRecordCount ?? 0,
            IsValidFormat = proxy.ValidFormatFlag ?? false,
            NonFatalErrors = proxy.NonFatalErrorCount,
            FatalErrors = proxy.FatalErrorCount,
            FileStatus = proxy.FileUploadStatusCodeDescription ?? string.Empty,
            ReportsAccepted = proxy.ReportsAccepted,
            ReportsRejected = proxy.ReportsRejected
        };
        return model;
    }

    private static WageUploadErrorRow MapErrorProxyToModel(FileUploadErrorProxy proxy)
    {
        var quarterYear = $"{proxy.ReportQuarter}/{proxy.ReportYear}";

        return new WageUploadErrorRow
        {
            UIAccountNumber = proxy.UIAccountNumber ?? string.Empty,
            FEIN = proxy.FEIN ?? string.Empty,
            QuarterYear = quarterYear,
            SSN = proxy.SSN ?? string.Empty,
            ErrorNumber = proxy.ErrorNumber,
            Description = proxy.ErrorDescription ?? string.Empty,
            Data = proxy.LineText ?? string.Empty,
            LineNumber = proxy.LineNumber ?? 0,
        };
    }

    private static void BuildMessages(WageUploadDetails model)
    {
        if (model.FileStatus == "In Review")
        {
            model.FileErrors.Add("File has been uploaded but not yet processed.");
            model.Warnings.Add("File has been uploaded but not yet processed.");
            return;
        }

        if (model.FileStatus is "Processed" or "Manual Followup")
        {
            if (!model.FatalErrorRows.Any())
            {
                model.FileErrors.Add("The file had no fatal errors.");
            }

            if (!model.NonFatalErrorRows.Any())
            {
                model.Warnings.Add("The file had no non-fatal errors.");
            }
            return;
        }

        if (!model.FatalErrorRows.Any())
        {
            model.FileErrors.Add("The file had no fatal errors");
        }

        if (!model.NonFatalErrorRows.Any())
        {
            model.Warnings.Add("The file had no non-fatal errors");
        }


    }

    #endregion
}
