using UI.EmployerPortal.Generated.ServiceClients.EmployerRegistrationService;
using UI.EmployerPortal.Web.Features.OtherFunctions.AuditFileUpload.Models;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Models;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Services;
using UI.EmployerPortal.Web.Features.Shared.FileUpload.Services;
using UI.EmployerPortal.Web.Features.Shared.QuarterlyTax.Services;
using UI.EmployerPortal.Web.Startup.ResiliencyProtocols;
using AuditFileUploadRequest = UI.EmployerPortal.Generated.ServiceClients.UploadService.AuditFileUploadRequest;
using ISessionManager = UI.EmployerPortal.Web.Features.Shared.Session.Managers.ISessionManager;

namespace UI.EmployerPortal.Web.Features.OtherFunctions.AuditFileUpload.Services;

/// <summary>
/// Coordinates the Audit File Upload flow: retrieving and saving the file contact
/// (PortalUtilityService.ObtainFileContact / SaveFileContact) and submitting the audit
/// file for the selected UI account (UploadService.CreateAuditFileUpload).
/// </summary>
public interface IAuditFileUploadService
{
    /// <summary>
    /// Looks up an employer by UI Account Number and FEIN.
    /// Returns <c>null</c> when no matching employer is found.
    /// </summary>
    Task<EmployerAccount?> LookupEmployerAsync(string uiAccountNumber, string fein);

    /// <summary>
    /// Retrieves any previously-saved file contact information for the current user so the
    /// form can be pre-populated. Returns <c>null</c> when no contact is on file.
    /// </summary>
    Task<AuditContactModel?> ObtainContactAsync();

    /// <summary>
    /// Persists the file contact information and submits the audit file for review.
    /// </summary>
    /// <param name="contact">Contact information entered/verified by the user.</param>
    /// <param name="uploadedFilePath">Server-side path of the file that was uploaded.</param>
    /// <param name="fileName">Original (display) file name.</param>
    /// <param name="recordCount"></param>
    /// <param name="employerAccountNotSavedInSession"></param>
    Task<AuditFileUploadResult> SubmitAuditFileUploadAsync(
        AuditContactModel contact,
        string uploadedFilePath,
        string fileName,
        int recordCount,
        EmployerAccount? employerAccountNotSavedInSession);

}

/// <inheritdoc />
internal class AuditFileUploadService : IAuditFileUploadService
{
    private readonly IContactInformationService _contactInformationService;
    private readonly IUserAccountService _userAccountService;
    private readonly IUploadServices _uploadServices;
    private readonly IEmployerRegistrationService _employerRegistrationService;
    private readonly ISessionManager _sessionManager;
    private readonly IAsyncRetryPolicy<AuditFileUploadService> _retryPolicy;

    /// <summary>
    /// File type code identifying an audit file upload (mirrors the WebUserFileTypeCodeSK
    /// values used for the wage / registration upload flows).
    /// </summary>
    private const int AuditFileTypeCodeSK = 4;

    /// <summary>
    /// Status code requesting that the uploaded file be received for review.
    /// </summary>
    private const int RequestedStatusCodeSK = 2;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuditFileUploadService"/> class.
    /// </summary>
    public AuditFileUploadService(
        IContactInformationService contactInformationService,
        IUserAccountService userAccountService,
        IUploadServices uploadServices,
        IEmployerRegistrationService employerRegistrationService,
        ISessionManager sessionManager,
        IAsyncRetryPolicy<AuditFileUploadService> retryPolicy)
    {
        _contactInformationService = contactInformationService;
        _userAccountService = userAccountService;
        _uploadServices = uploadServices;
        _employerRegistrationService = employerRegistrationService;
        _sessionManager = sessionManager;
        _retryPolicy = retryPolicy;
    }

    /// <inheritdoc />
    public async Task<EmployerAccount?> LookupEmployerAsync(string uiAccountNumber, string fein)
    {
        var request = new EmployerSearchRequest
        {
            UIAccountNumber = uiAccountNumber.Replace("-", string.Empty).Trim(),
            FEIN = fein.Replace("-", string.Empty).Trim(),
            MaxSearchResults = 1
        };

        var response = await _retryPolicy.ExecuteAsync(() =>
        {
            return _employerRegistrationService.SearchEmployersAsync(request);
        });

        var proxy = response?.EmployerProxies?.FirstOrDefault();

        return proxy is null
            ? null
            : new EmployerAccount
            {
                Id = proxy.CommonClientSK,
                LegalName = proxy.LegalName,
                UIAccountNo = proxy.UIAccountNumber,
                IsTaxable = proxy.IsTaxable
            };
    }

    /// <inheritdoc />
    public async Task<AuditContactModel?> ObtainContactAsync()
    {
        var secureUserSK = _userAccountService.GetUserSKClaim();
        var contact = await _contactInformationService.ObtainFileContact(secureUserSK);

        if (contact is null)
        {
            return null;
        }

        var model = new AuditContactModel();
        model.CopyFrom(contact);
        return model;
    }

    /// <inheritdoc />
    public async Task<AuditFileUploadResult> SubmitAuditFileUploadAsync(
       AuditContactModel contact,
       string uploadedFilePath,
       string fileName,
       int recordCount,
       EmployerAccount? employerAccountNotSavedInSession)
    {
        // 1. Persist the file contact before submitting the file.
        var contactResponse = await _contactInformationService.SaveFileContact(contact.ToContactModel());

        // 2. Submit the audit file for review (unless saving the contact reported errors).
        return contactResponse?.RuleViolations is { Length: > 0 } contactViolations
            ? new AuditFileUploadResult
            {
                FileName = fileName,
                RuleViolations = ToMessages(contactViolations.Select(v =>
                {
                    return v.RuleViolation;
                }))
            }
            : await CreateAuditFileUploadAsync(uploadedFilePath, fileName, recordCount, employerAccountNotSavedInSession);
    }

    /// <summary>
    /// Builds the audit file upload request for the selected UI account and submits it via
    /// UploadService.CreateAuditFileUpload, returning the server-issued confirmation number.
    /// </summary>
    private async Task<AuditFileUploadResult> CreateAuditFileUploadAsync(
        string uploadedFilePath,
        string fileName,
        int recordCount,
        EmployerAccount? employerAccountNotSavedInSession)
    {
        var sessionEmployer = await _sessionManager.GetAsync<SelectedEmployerAccount>();
        var commonClientSK = employerAccountNotSavedInSession?.Id ?? sessionEmployer?.EmployerAccount?.Id ?? 0;

        var uploadDateTime = DateTime.Now;

        var request = new AuditFileUploadRequest
        {
            CommonClientSK = commonClientSK,
            SecureUserSk = _userAccountService.GetUserSKClaim(),
            FilePath = uploadedFilePath,
            //FileName = fileName,
            ContentLength = recordCount,
            WebUserFileStatusCodeSK = RequestedStatusCodeSK,
            WebUserFileTypeCodeSK = AuditFileTypeCodeSK,
            IsTestFile = false,
            OriginalFileName = fileName
        };

        var response = await _retryPolicy.ExecuteAsync(() =>
        {
            return _uploadServices.SubmitAuditFileUploadAsync(request);
        });

        return response?.RuleViolations is { Length: > 0 } violations
            ? new AuditFileUploadResult
            {
                FileName = fileName,
                RuleViolations = ToMessages(violations.Select(v =>
                {
                    return v.RuleViolation;
                }))
            }
            : new AuditFileUploadResult
            {
                ConfirmationNumber = response?.AuditConfirmationNumber,
                FileName = fileName,
                UploadDateTime = uploadDateTime
            };
    }

    private static List<string> ToMessages(IEnumerable<string?> ruleViolations)
    {
        return ruleViolations
            .Select(v =>
            {
                return v ?? string.Empty;
            })
            .Where(m =>
            {
                return !string.IsNullOrWhiteSpace(m);
            })
            .ToList();
    }
}
