using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace UI.EmployerPortal.Web.Logging.NavigationSession;

internal interface INavigationMonitor : IDisposable
{
    ValueTask Start();
}

internal sealed class NavigationMonitor : INavigationMonitor
{
    private bool _disposedValue;

    private readonly NavigationManager _navigationManager;
    private readonly INavigationSessionTracker _navigationSessionTracker;
    private IDisposable? _registration;

    public NavigationMonitor(NavigationManager navigationManager
        , INavigationSessionTracker navigationSessionTracker)
    {
        _navigationManager = navigationManager;
        _navigationSessionTracker = navigationSessionTracker;
    }

    public async ValueTask Start()
    {
        await _navigationSessionTracker.StartNavigationSessionIfRequiredAsync(DateTime.Now);
        _registration ??= _navigationManager.RegisterLocationChangingHandler(OnLocationChanged);
    }

    private async ValueTask OnLocationChanged(LocationChangingContext context)
    {
        var pageType = Resolve(context, _navigationManager);
        await _navigationSessionTracker.LogNavigationSessionActivity(pageType, "Load Page", DateTime.Now);
    }

    private void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _registration?.Dispose();
            }

            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private sealed record RouteRule(string Template, PageType PageType);

    private static readonly RouteRule[] Rules =
    {
            new("/manage-account/account-summary", PageType.AccountSummary),
            new("/manage-account/status-change", PageType.StatusChange),
            new("/manage-account/rate-summary", PageType.RateSummary),
            new("/employer-registration/UISubjectivity", PageType.UISubjectivity),
            new("/employer-registration/address-correction", PageType.AddressCorrection),
            new("/employer-registration/employer-registration-welcome", PageType.EmployerRegistrationWelcome),
            new("/billing-payments/payment-history", PageType.PaymentHistory),
            new("/billing-payments/payment-options", PageType.PaymentOptions),
            new("/billing-payments/request-refund", PageType.RefundRequest),
            new("/billing-payments/voluntary-contribution", PageType.VoluntaryContributionSummary),
            new("/esp/data-exchange/upload", PageType.ESPDataExchangeRequestUpload),
            new("/esp/data-exchange/response-files", PageType.ESPDataExchangeResponseDownload),
            new("/esp/data-exchange/register-ftps", PageType.FTPSSelfRegistration),
            new("/esp/payment-history", PageType.PaymentHistoryServiceProvider),
            new("/other-functions/upload-audit", PageType.AuditFileUpload),
            new("/audit-file-upload", PageType.AuditFileUpload),
            new("/other-functions/email-subscriptions", PageType.EmailSubscription),
            new("/manage-email-subscription-confirmation", PageType.EmailSubscriptionConfirmation),
            new("/sides-e-response", PageType.SIDESEResponse),
            new("/other-functions/940c", PageType.RecertificationRequest940C),
            new("/user-settings/account-access", PageType.AccountAccess),
            new("/manage-account/finance-method-change-rq", PageType.FinanceMethodChange),
            new("/manage-account/account-users/generate-key", PageType.SecurityKeyGeneration),
            new("/manage-account/manage-addresses", PageType.AddressManager),
            new("/tax-wage-report-adjustments/wage-adjustment-by-quarter", PageType.WageAdjustmentByQuarter),
            new("/tax-wage-report-adjustments/tax-report-adjustment", PageType.TaxAdjustment),
            new("/quarterly-tax/first-quarter-deferral", PageType.FirstQuarterDeferralElection),
            new("/quarterly-tax/tax-file-upload-summary", PageType.TaxFileUploadSummary),
            new("/quarterly-tax/test/tax-file-upload-summary", PageType.TaxFileUploadSummary),
            new("/quarterly-tax/tax-file-upload", PageType.TaxUpload),
            new("/quarterly-tax/test/tax-file-upload", PageType.TaxUpload),
            new("/quarterly-tax/wage-entry-report", PageType.WageEntry),
            new("/quarterly-tax/wage-report-file-upload-error-list", PageType.WageFileUploadErrors),
            new("/quarterly-tax/upload-summary", PageType.WageFileUploadSummary),
            new("/quarterly-tax/test/upload-summary", PageType.WageFileUploadSummary),
            new("/quarterly-tax/wage-upload", PageType.WageUpload),
            new("/quarterly-tax/test/wage-upload", PageType.WageUpload),
            new("/access-denied", PageType.Error403Page),
            new("/Error", PageType.ErrorPage),

            new("/billing-payments/pay-by-check-voluntary", PageType.PrintVoluntaryCoupon),
            new("/billing-payments/manage-bank-accounts", PageType.BankAccountView),
            new("/billing-payments/card-payment", PageType.CardPaymentDetail),
            new("/billing-payments/billing", PageType.BillDetail),
            new("/billing-payments/manage-contact", PageType.ContactInformation),
            new("/billing-payments/payment-history-details/{EftPaymentSk:int}", PageType.PaymentDetail),
            new("/billing-payments/bank-account-payment-ach", PageType.BankAccountEdit),
            new("/esp/manage-bank-accounts", PageType.BankAccountViewServiceProvider),
            new("/esp-home", PageType.ServiceProviderHome),
            new("/employer-registration", PageType.EmployerRegistrationMain),
            new("/employer-registration/steps", PageType.ContinueEmployerRegistration),
            new("/employer-dashboard", PageType.EmployerHomeMain),
            new("/home", PageType.AccountMain),
            new("/landing-page", PageType.EmployerHome),
            new("/esp-landing", PageType.ServiceProviderHome),
            new("/manage-account/account-details", PageType.AccountMaintenance),
            new("/manage-account/account-users", PageType.WorkerAccessMaintenance),
            new("/other-functions/download-transactions", PageType.DownloadOtherTaxableTransactions),
            new("/other-functions/reserve-balance", PageType.DiscontinueMailingRFBStatement),
            new("/other-functions/other-functions-page", PageType.EmployerHomeOtherFunctions),
            new("/other-functions/benefit-charges", PageType.DownloadBenefitMenu),
            new("/other-functions/download-benefit-charges", PageType.DownloadBenefitActivity),
            new("/tax-wage-report-adjustments/add-new-employees", PageType.WageAdjustmentAppend),
            new("/tax-wage-report-adjustments/adjustments", PageType.TaxWageAdjustmentHome),
            new("/quarterly-tax/previously-filed/details", PageType.TaxWageDetail),
            new("/quarterly-tax/previously-filed", PageType.TaxWageSummary),
            new("/quarterly-tax/select-report", PageType.TaxWageFilingHome),
            new("/quarterly-tax/tax-report-error-list", PageType.TaxFileUploadErrors),
            new("/quarterly-tax/tax-upload-details", PageType.TaxFileUploadErrorDetail),
            new("/quarterly-tax/test/tax-upload-details", PageType.TaxFileUploadErrorDetail),
            new("/quarterly-tax/tax-report-only", PageType.TaxEntry),
            new("/quarterly-tax/wage-upload-details", PageType.FileUploadSummaryDetail),
            new("/quarterly-tax/test/wage-upload-details", PageType.FileUploadSummaryDetail),
            new("/FileUploadPage", PageType.FileUploadHome),
            new("/not-found", PageType.ErrorPage),

            new("/levies/response-fraud/{CollectionItemSk:int}/calculate", PageType.LevyWagesFraudCalculator),
            new("/levies/response-wage/{CollectionItemSk:int}/calculate", PageType.LevyWagesNonFraudCalculator),
            new("/levies/response-tax/{CollectionItemSk:int}/confirm", PageType.LevyWagesConfirmation),
            new("/levies/response-bank/{CollectionItemSk:int}/confirm", PageType.LevyWagesConfirmation),
            new("/levies/response-wage/{CollectionItemSk:int}/confirm", PageType.LevyWagesConfirmation),
            new("/levies/response-fraud/{CollectionItemSk:int}/confirm", PageType.LevyWagesConfirmation),

            new("/appeals/{AppealId}", PageType.AppealDetails),
            new("/appeals/{AppealId}/documents", PageType.AppealDocuments),
            new("/appeals", PageType.Appeals),
            new("/appeals/{AppealId}/hearing-contact", PageType.AppealHearingContact),
            new("/appeals/{AppealId}/documents/submit", PageType.AppealSubmitDocuments),
            new("/appeals/{AppealId}/withdraw", PageType.AppealWithdraw),

            new("/billing-payments", PageType.BillingPaymentsHome),
            new("/billing-payments/billing-reimbursable", PageType.BillReimbursable),
            new("/billing-payments/pay-by-check", PageType.PayByCheck),
            new("/billing-payments/refund-confirmation", PageType.RefundConfirmation),

            new("/guest-dashboard-temporary", PageType.GuestAccessDashboard),

            new("/eforms", PageType.EForms),
            new("/eforms-details/{EFormTransactionSk:int}", PageType.EFormDetails),
            new("/eforms-confirmation/{EFormTransactionSk:int}", PageType.EFormSubmissionConfirmation),

            new("/employer-registration/summary", PageType.EmployerRegistrationSummary),
            new("/employer-registration/Verification", PageType.EmployerRegistrationVerification),
            new("/employer-registration/planned-path", PageType.EmployerRegistrationPlannedPath),
            new("/employer-registration/SuccessRegistration", PageType.EmployerRegistrationSuccess),

            new("/esp-dashboard", PageType.ESPDashboard),
            new("/esp/payment/esp-contact", PageType.ESPContact),
            new("/esp/account/get-access", PageType.ESPGetAccess),
            new("/esp/esp-make-payment-ach-information", PageType.ESPMakeACHPaymentInformation),
            new("/esp/payment-history-details/{EftPaymentSk:int}", PageType.ESPPaymentHistoryDetails),
            new("/esp/account/profile", PageType.ESPProfile),
            new("/esp/account/register", PageType.ESPRegistration),
            new("/esp/account/clients/add", PageType.ESPGetClientAccess),
            new("/esp/make-ach-payment", PageType.ESPMakeACHPayment),
            new("/esp/account/clients", PageType.ESPManageClientAccounts),

            new("/levies", PageType.DebtorLevies),
            new("/levies/response-wage/{CollectionItemSk:int}", PageType.LevyAnswerResponseWage),
            new("/levies/response-fraud/{CollectionItemSk:int}", PageType.LevyAnswerResponseFraud),
            new("/levies/response-wage/{CollectionItemSk:int}/representative", PageType.LevyAuthorizedRepresentativeWage),
            new("/levies/response-fraud/{CollectionItemSk:int}/representative", PageType.LevyAuthorizedRepresentativeFraud),
            new("/levies/bank-account-payment-ach", PageType.LevyBankAccountPaymentACH),
            new("/levies/response-bank/{CollectionItemSk:int}", PageType.LevyBankCalculator),
            new("/levies/details/{CollectionItemSk:int}", PageType.LevyDetails),
            new("/levies/details/{CollectionItemSk:int}/documents", PageType.LevyDocuments),
            new("/leviesPayments", PageType.LevyPayments),
            new("/levies/response-tax/{CollectionItemSk:int}", PageType.LevyTaxCalculator),

            new("/manage-account/corporate-officer-exclusion", PageType.CorporateOfficerExclusion),
            new("/manage-account/account-users/{WebUserSecuritySK:int}/edit", PageType.EditUserPermissions),
            new("/manage-account/account-users/generate-key/result", PageType.GeneratedAccessKey),
            new("/manage-account/account-users/generate-key/permissions", PageType.GenerateWorkerKeyPermissions),
            new("/Pages/AccessAccount", PageType.GetAccountAccess),
            new("/manage-account", PageType.ManageAccountHome),
            new("/manage-account/esp-accounts", PageType.ManageESPAccess),

            new("/other-functions/reserve-balance-form", PageType.MailBenefitChargesAdjustmentsForm),
            new("/other-functions/select-other-functions", PageType.SelectOtherFunctions),

            new("/tax-wage-report-adjustments/wage-adjustment-multiple-quarter", PageType.WageAdjustmentMultipleQuarter),
            new("/quarterly-tax/missing-reports", PageType.MissingWageAndTaxReports),
            new("/quarterly-tax/tax-and-wage-entry-report", PageType.TaxAndWageEntryReport),
            new("/quarterly-tax/zero-payroll-tax-report", PageType.ZeroPayrollTaxReport),

            new("/rules-of-conduct", PageType.RulesOfConduct),

            new("/secure-messages", PageType.SecureMessages),
            new("/secure-messages/{ThreadId}", PageType.SecureMessageThread),
            new("/secure-messages/new", PageType.NewSecureMessage),

            new("/ui-documents", PageType.UIDocuments),

            new("/user-settings", PageType.UserSettingsHome),
            new("/user-settings/communication-preferences", PageType.CommunicationPreferences),
            new("/user-settings/user-profile", PageType.UserProfile),
        };

    // Anything not in Rules above (Appeals, EForms, Secure Messages, most Levy detail
    // pages, and any route where no defensible PageType match was found) simply isn't
    // listed, and Resolve() below falls through to PageType.WebSessionTransfer (999).

    /// <summary>
    /// Resolves an incoming path (with or without leading slash, query string, or host)
    /// to its matching PageType, or PageType.WebSessionTransfer (999) if unknown.
    /// </summary>
    public static PageType Resolve(string routeOrUrl)
    {
        if (string.IsNullOrWhiteSpace(routeOrUrl))
        {
            return PageType.WebSessionTransfer;
        }

        var actualSegments = NormalizeToSegments(routeOrUrl);

        foreach (var rule in Rules)
        {
            var templateSegments = NormalizeToSegments(rule.Template);
            if (SegmentsMatch(templateSegments, actualSegments))
            {
                return rule.PageType;
            }
        }

        return PageType.WebSessionTransfer; // 999 - no known mapping
    }

    /// <summary>
    /// Convenience overload for use directly inside a NavigationManager location-changing
    /// handler
    /// </summary>
    public static PageType Resolve(LocationChangingContext context, NavigationManager navigationManager)
    {
        if (context is null)
        {
            return PageType.WebSessionTransfer;
        }

        var target = context.TargetLocation;

        if (navigationManager is not null)
        {
            try
            {
                target = navigationManager.ToBaseRelativePath(target);
            }
            catch
            {
                // Not an absolute/base-relative URI NavigationManager recognized -
                // fall back to using TargetLocation exactly as given.
            }
        }

        return Resolve(target);
    }

    private static string[] NormalizeToSegments(string path)
    {
        // Strip any scheme/host if a full URL was passed in, then query string/fragment.
        var idx = path.IndexOf("://", StringComparison.Ordinal);
        if (idx >= 0)
        {
            var afterScheme = path[(idx + 3)..];
            var slashIdx = afterScheme.IndexOf('/');
            path = slashIdx >= 0 ? afterScheme[slashIdx..] : string.Empty;
        }

        var clean = path.Split('?', '#')[0].Trim('/', ' ');
        return clean.Length == 0
            ? Array.Empty<string>()
            : clean.Split('/', StringSplitOptions.RemoveEmptyEntries);
    }

    private static bool SegmentsMatch(string[] template, string[] actual)
    {
        if (template.Length != actual.Length)
        {
            return false;
        }

        for (var i = 0; i < template.Length; i++)
        {
            var t = template[i];

            if (t.StartsWith("{", StringComparison.Ordinal) && t.EndsWith("}", StringComparison.Ordinal))
            {
                // Route parameter segment, e.g. {AppealId} or {EftPaymentSk:int}.
                if (string.IsNullOrEmpty(actual[i]))
                {
                    return false;
                }

                var colonIdx = t.IndexOf(':');
                if (colonIdx >= 0)
                {
                    var constraint = t.Substring(colonIdx + 1, t.Length - colonIdx - 2); // strip trailing '}'
                    if (constraint.Equals("int", StringComparison.OrdinalIgnoreCase) &&
                        !long.TryParse(actual[i], out _))
                    {
                        return false;
                    }
                }

                continue;
            }

            if (!string.Equals(t, actual[i], StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
    }
}
