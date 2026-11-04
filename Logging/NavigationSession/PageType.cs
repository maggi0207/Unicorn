using System.Reflection;
using System.Text.Json.Serialization;
using UI.EmployerPortal.Web.Features.Shared.Session.Models;

namespace UI.EmployerPortal.Web.Logging.NavigationSession;

[JsonConverter(typeof(PageTypeJsonConverter))]
internal sealed class PageType : ISessionModel
{
    private PageType(int value, string name)
    {
        Value = value;
        Name = name;
    }

    public int Value { get; }
    public string Name { get; }

    private static readonly Lazy<IReadOnlyDictionary<int, PageType>> ByValue = new(BuildLookup);

    /// <summary>Resolves a <see cref="PageType"/> from its <see cref="Value"/>, or null when unknown.</summary>
    public static PageType? FromValue(int value)
    {
        return ByValue.Value.GetValueOrDefault(value);
    }

    private static IReadOnlyDictionary<int, PageType> BuildLookup()
    {
        var lookup = new Dictionary<int, PageType>();

        foreach (var field in typeof(PageType).GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(field => field.FieldType == typeof(PageType)))
        {
            var pageType = (PageType) field.GetValue(null)!;
            lookup[pageType.Value] = pageType;
        }

        foreach (var property in typeof(PageType).GetProperties(BindingFlags.Public | BindingFlags.Static)
            .Where(property => property.PropertyType == typeof(PageType)))
        {
            var pageType = (PageType) property.GetValue(null)!;
            lookup[pageType.Value] = pageType;
        }

        return lookup;
    }

    public static PageType AccountMain = new(1, "Account Main");
    public static PageType AccountAccess = new(2, "Account Access");
    public static PageType AccountSelection = new(3, "Account Selection");
    public static PageType EmployerHome = new(4, "Employer Home");
    public static PageType ErrorPage = new(5, "Error Page");
    public static PageType Logout = new(6, "Logout");
    public static PageType AddEmployee = new(7, "Add Employee");
    public static PageType WageFileUploadSummary = new(8, "Wage File Upload Summary");
    public static PageType FileUploadSummaryDetail = new(9, "File Upload Summary Detail");
    public static PageType ModifyEmployee = new(10, "Modify Employee");
    public static PageType TaxEntry = new(11, "Tax Entry");
    public static PageType TaxEntryReadOnly = new(12, "Tax Entry Read Only");
    public static PageType TaxUpload = new(13, "Tax Upload");
    public static PageType TaxWageVerification = new(14, "Tax Wage Verification");
    public static PageType TaxWageDetail = new(15, "Tax Wage Detail");
    public static PageType TaxWageFilingHome = new(16, "Tax Wage Filing Home");
    public static PageType TaxWageFilingInfo = new(17, "Tax Wage Filing Info");
    public static PageType TaxWageConfirmation = new(18, "Tax Wage Confirmation");
    public static PageType TaxWageReportEntryIncomplete = new(19, "Tax Wage Report Entry Incomplete");
    public static PageType TaxWageSummary = new(20, "Tax Wage Summary");
    public static PageType WageEntry = new(21, "Wage Entry");
    public static PageType WageEntryCancelConfirm = new(22, "Wage Entry Cancel Confirm");
    public static PageType WageUpload = new(23, "Wage Upload");
    public static PageType AccountChangeRequest = new(24, "Account Change Request");
    public static PageType AccountMaintenance = new(25, "Account Maintenance");
    public static PageType AccountSummary = new(26, "Account Summary");
    public static PageType COEElection = new(27, "COE Election");
    public static PageType COEReasonableness = new(28, "COE Reasonableness");
    public static PageType FinanceMethodChange = new(29, "Finance Method Change");
    public static PageType RateSummary = new(30, "Rate Summary");
    public static PageType RecertificationRequest940C = new(31, "Recertification Request 940C");
    public static PageType RefundRequest = new(32, "Refund Request");
    public static PageType StatusChange = new(33, "Status Change");
    public static PageType VoluntaryContributionSummary = new(34, "Voluntary Contribution Summary");
    public static PageType ContinueEmployerRegistration = new(35, "Continue Employer Registration");
    public static PageType EmployerRegistrationExit = new(36, "Employer Registration Exit");
    public static PageType EmployerRegistrationWelcome = new(37, "Employer Registration Welcome");
    public static PageType EmployerRegistrationMain = new(38, "Employer Registration Main");
    public static PageType EmployeePayrollQuestions = new(39, "Employee Payroll Questions");
    public static PageType QuestionSummaryPage = new(40, "Question Summary Page");
    public static PageType OwnershipQuestions = new(41, "Ownership Questions");
    public static PageType BusinessInformation = new(42, "Business Information");
    public static PageType BusinessContact = new(43, "Business Contact");
    public static PageType BusinessActivity = new(44, "Business Activity");
    public static PageType UISubjectivity = new(45, "UI Subjectivity");
    public static PageType AddressCorrection = new(46, "Address Correction");
    public static PageType IDCorrespondence = new(47, "ID Correspondence");
    public static PageType EmployerNextSteps = new(48, "Employer Next Steps");
    public static PageType IDCorrespondenceLoader = new(49, "ID Correspondence Loader");
    public static PageType BusinessTransferMain = new(50, "Business Transfer Main");
    public static PageType ContinuationOfBusiness = new(51, "Continuation of Business");
    public static PageType PredecessorInformation = new(52, "Predecessor Information");
    public static PageType RelationshipAssetsAndMethod = new(53, "Relationship Assets And Method");
    public static PageType SourceInformation = new(54, "Source Information");
    public static PageType SuccessorInformation = new(55, "Successor Information");
    public static PageType TotalOrPartialTransfer = new(56, "Total Or Partial Transfer");
    public static PageType TransferExitIncomplete = new(57, "Transfer Exit Incomplete");
    public static PageType TransferRequestComplete = new(58, "Transfer Request Complete");
    public static PageType TransferRequestConfirmation = new(59, "Transfer Request Confirmation");
    public static PageType UIExperience = new(60, "UI Experience");
    public static PageType TaxPaymentAgreementPlan = new(61, "TaxPayment Agreement Plan");
    public static PageType CorrespondencePreview = new(62, "Correspondence Preview");
    public static PageType CorrespondenceViewRecreate = new(63, "Correspondence View Recreate");
    public static PageType AccountVerification = new(64, "Account Verification");
    public static PageType Comments = new(65, "Comments");
    public static PageType BillDetail = new(66, "Bill Detail");
    public static PageType PrintCoupon = new(67, "Print Coupon");
    public static PageType Error403Page = new(68, "Error 403 Page");
    public static PageType AddressManager = new(69, "Address Manager");
    public static PageType TaxFileUploadErrorDetail = new(70, "Tax File Upload Error Detail");
    public static PageType TaxFileUploadErrorAllDetails = new(71, "Tax File Upload Error All Details");
    public static PageType TaxEntryCancelConfirm = new(72, "Tax Entry Cancel Confirm");
    public static PageType ChangeAccess = new(73, "Change Access");
    public static PageType SystemTimeoutPage = new(74, "System Timeout Page");
    public static PageType ServiceProviderHome = new(75, "Service Provider Home");
    public static PageType FileUploadPendingPayments = new(76, "File Upload Pending Payments");
    public static PageType FileUploadInitiatePayment = new(77, "File Upload Initiate Payment");
    public static PageType FileUploadCancelPayment = new(78, "File Upload Cancel Payment");
    public static PageType FileUploadPaymentConfirmation = new(79, "File Upload Payment Confirmation");
    public static PageType TaxFileUploadSummary = new(80, "Tax File Upload Summary");
    public static PageType CorrespondenceSelection = new(81, "Correspondence Selection");
    public static PageType FileUploadHome = new(82, "File Upload Home");
    public static PageType COEConfirmation = new(83, "COE Confirmation");
    public static PageType CorrespondenceLoader = new(84, "Correspondence Loader");
    public static PageType FileUploadCancelFile = new(85, "File Upload Cancel File");
    public static PageType TaxFileUploadErrors = new(86, "Tax File Upload Errors");
    public static PageType WageFileUploadErrors = new(88, "Wage File Upload Errors");
    public static PageType CorrespondenceLink = new(89, "Correspondence Link");
    public static PageType FirstQuarterDeferralElection = new(90, "First Quarter Deferral Election");
    public static PageType OtherEFTPaymentOptions = new(91, "Other EFT Payment Options");
    public static PageType TaxWageAdjustmentHome = new(92, "Tax Wage Adjustment Home");
    public static PageType TaxAdjustment = new(93, "Tax Adjustment");
    public static PageType TaxAdjustmentVerify = new(94, "Tax Adjustment Verify");
    public static PageType TaxAdjustmentConfirmation = new(95, "Tax Adjustment Confirmation");
    public static PageType WageAdjustmentByQuarter = new(96, "Wage Adjustment By Quarter");
    public static PageType WageAdjustmentBySSN = new(97, "Wage Adjustment By SSN");
    public static PageType WageAdjustmentAppend = new(98, "Wage Adjustment Append");
    public static PageType WageAdjustmentByQuarterVerify = new(99, "Wage Adjustment By Quarter Verify");
    public static PageType WageAdjustmentConfirmation = new(100, "Wage Adjustment Confirmation");
    public static PageType WageAdjustmentBySSNVerify = new(101, "Wage Adjustment By SSN Verify");
    public static PageType WageAdjustmentAppendVerify = new(102, "Wage Adjustment Append Verify");
    public static PageType TaxUploadConfirmation = new(103, "Tax Upload Confirmation");
    public static PageType WageUploadConfirmation = new(104, "Wage Upload Confirmation");
    public static PageType ESPDataExchangeRequestUpload = new(105, "ESP Data Exchange Request Upload");
    public static PageType ESPDataExchangeRequestUploadConfirmation = new(106, "ESP Data Exchange Request Upload Confirmation");
    public static PageType ESPDataExchangeResponseDownload = new(107, "ESP Data Exchange Response Download");
    public static PageType WageUploadType = new(108, "Wage Upload Type");
    public static PageType AdjustWageReportAppendWagesCancelConfirm = new(109, "Adjust Wage Report Append Wages Cancel Confirm");
    public static PageType AdjustWageReportByQuarterCancelConfirm = new(110, "Adjust Wage Report By Quarter Cancel Confirm");
    public static PageType AdjustWageReportBySSNCancelConfirm = new(111, "Adjust Wage Report By SSN Cancel Confirm");
    public static PageType CorrespondenceImagePreview = new(112, "Correspondence Image Preview");
    public static PageType UCB719Home = new(113, "UCB719Home");
    public static PageType UCB719Detail = new(114, "UCB719Detail");
    public static PageType UCB719Confirmation = new(115, "UCB719Confirmation");
    public static PageType UCB719Verification = new(116, "UCB719Verification");
    public static PageType FTPSSelfRegistration = new(117, "FTPS Self Registration");
    public static PageType FTPSSelfRegistrationPost = new(118, "FTPS Self Registration Post");
    public static PageType UCB719CancelConfirm = new(119, "UCB719CancelConfirm");
    public static PageType FTPSSelfRegistrationWelcome = new(120, "FTPS Self Registration Welcome");
    public static PageType WorkerAccessMaintenance = new(121, "Worker Access Maintenance");
    public static PageType BankAccountView = new(122, "Bank Account View");
    public static PageType BankAccountEdit = new(123, "Bank Account Edit");
    public static PageType BankAccountRemove = new(124, "Bank Account Remove");
    public static PageType BankAccountConfirm = new(125, "Bank Account Confirm");
    public static PageType PaymentHistory = new(128, "Payment History");
    public static PageType PaymentEdit = new(129, "Payment Edit");
    public static PageType PaymentVerifiy = new(130, "Payment Verifiy");
    public static PageType PaymentConfirm = new(131, "Payment Confirm");
    public static PageType PaymentDetail = new(132, "Payment Detail");
    public static PageType PaymentCancel = new(133, "Payment Cancel");
    public static PageType RecurringPaymentView = new(134, "Recurring Payment View");
    public static PageType RecurringPaymentEdit = new(135, "Recurring Payment Edit");
    public static PageType RecurringPaymentConfirmation = new(136, "Recurring Payment Confirmation");
    public static PageType ContactInformation = new(137, "Contact Information");
    public static PageType ContactInformationConfirmation = new(138, "Contact Information Confirmation");
    public static PageType EmployerHomeQuarterlyReports = new(139, "EmployerHome_QuarterlyReports");
    public static PageType WorkerNoAccess = new(140, "Worker No Access");
    public static PageType PaymentInformation = new(141, "Payment Information");
    public static PageType EmployerHomeAccountAccess = new(142, "EmployerHome_AccountAccess");
    public static PageType EmployerHomePayments = new(143, "EmployerHome_Payments");
    public static PageType EmployerHomeAccountMaintenance = new(144, "EmployerHome_AccountMaintenance");
    public static PageType EmployerHomeMain = new(145, "EmployerHome_Main");
    public static PageType PaymentOptions = new(146, "PaymentOptions");
    public static PageType PaymentEditServiceProvider = new(147, "Payment Edit Service Provider");
    public static PageType BankAccountViewServiceProvider = new(148, "Bank Account View Service Provider");
    public static PageType PaymentHistoryServiceProvider = new(149, "Payment History Service Provider");
    public static PageType ManagerSystemInfo = new(150, "Manager System Info");
    public static PageType ManagerWorkerInfo = new(151, "Manager Worker Info");
    public static PageType PortalMessage = new(152, "Portal Message");
    public static PageType DownloadOtherTaxableTransactions = new(153, "Download Other Taxable Transactions");
    public static PageType IATConfirmation = new(154, "IAT Confirmation");
    public static PageType DownloadBenefitMenu = new(155, "Download Benefit Menu");
    public static PageType DownloadBenefitActivity = new(156, "Download Benefit Activity");
    public static PageType DiscontinueMailingRFBStatement = new(157, "Discontinue Mailing RFB Statement");
    public static PageType BankAccountDetail = new(158, "Bank Account Detail");
    public static PageType LevyWagesFraudCalculator = new(159, "Levy Wages Fraud Calculator");
    public static PageType LevyWagesNonFraudCalculator = new(160, "Levy Wages Non Fraud Calculator");
    public static PageType EmployerHomeOtherFunctions = new(161, "EmployerHome_OtherFunctions");
    public static PageType VoluntaryPaymentAcknowledgement = new(162, "Voluntary Payment Acknowledgement");
    public static PageType BillDetailReadonly = new(163, "Bill Detail Readonly");
    public static PageType PrintVoluntaryCoupon = new(165, "Print Voluntary Coupon");
    public static PageType LevyWagesConfirmation = new(166, "Levy Wages Confirmation");
    public static PageType TaxFilingConfirmation = new(167, "Tax Filing Confirmation");
    public static PageType WageUploadPostTaxConfirmation = new(168, "Wage Upload Post Tax Confirmation");
    public static PageType SecurityKeyGeneration = new(169, "Security Key Generation");
    public static PageType SecurityAccessConfirmation = new(170, "Security Access Confirmation");
    public static PageType AccountAccessRevokeVerification = new(171, "Account Access Revoke Verification");
    public static PageType AccountAccessRevokeConfirmation = new(172, "Account Access Revoke Confirmation");
    public static PageType AccountAccessConfirmation = new(173, "Account Access Confirmation");
    public static PageType EmailSubscriptionNotification = new(174, "Email Subscription Notification");
    public static PageType EmailSubscription = new(175, "Email Subscription");
    public static PageType EmailSubscriptionConfirmation = new(176, "Email Subscription Confirmation");
    public static PageType AuditFileUpload = new(177, "Audit File Upload");
    public static PageType AuditFileUploadConfirmation = new(178, "Audit File Upload Confirmation");
    public static PageType AccountNumberConfirmation = new(179, "Account Number Confirmation");
    public static PageType RequestAccountAccessMailer = new(180, "Request Account Access Mailer");
    public static PageType SIDESEResponse = new(181, "SIDES E-Response");
    public static PageType AuditFileUploadContact = new(182, "Audit File Upload Contact");
    public static PageType CardPaymentServiceFeeDisclosure = new(183, "Card Payment Service Fee Disclosure");
    public static PageType CardPaymentPortalEPayRegistration = new(184, "Card Payment Portal EPay Registration");
    public static PageType CardPaymentDetail = new(185, "Card Payment Detail");
    public static PageType CardPaymentVerification = new(186, "Card Payment Verification");
    public static PageType CardPaymentConfirmation = new(187, "Card Payment Confirmation");
    public static PageType CardPaymentError = new(188, "Card Payment Error");
    public static PageType PaymentPortalContact = new(500, "Payment Portal Contact");
    public static PageType PaymentPortalPaymentVerification = new(501, "Payment Portal PaymentVerification");
    public static PageType PaymentPortalPaymentConfirmation = new(502, "Payment Portal PaymentConfirmation");
    public static PageType PaymentPortalPaymentError = new(503, "Payment Portal PaymentError");
    public static PageType PaymentPortalSessionTimeout = new(504, "Payment Portal SessionTimeout");
    public static PageType PaymentPortalPaymentSelection = new(505, "Payment Portal PaymentSelection");
    public static PageType PaymentPortalMaxInvalidLookupAttempts = new(506, "Payment Portal MaxInvalidLookupAttempts");
    public static PageType PaymentPortalError = new(507, "Payment Portal Error");
    public static PageType PaymentPortalLogon = new(508, "Payment Portal Logon");
    public static PageType PaymentPortalRegistration = new(509, "Payment Portal Registration");
    public static PageType PaymentPortalLogout = new(510, "Payment Portal Logout");
    public static PageType AppealDetails { get; } = new(600, "Appeal Details");
    public static PageType AppealDocuments { get; } = new(601, "Appeal Documents");
    public static PageType Appeals { get; } = new(602, "Appeals");
    public static PageType AppealHearingContact { get; } = new(603, "Appeal Hearing Contact");
    public static PageType AppealSubmitDocuments { get; } = new(604, "Appeal Submit Documents");
    public static PageType AppealWithdraw { get; } = new(605, "Appeal Withdraw");
    public static PageType BillingPaymentsHome { get; } = new(610, "Billing Payments Home");
    public static PageType BillReimbursable { get; } = new(611, "Bill Reimbursable");
    public static PageType PayByCheck { get; } = new(612, "Pay By Check");
    public static PageType RefundConfirmation { get; } = new(613, "Refund Confirmation");
    public static PageType GuestAccessDashboard { get; } = new(620, "Guest Access Dashboard");
    public static PageType EForms { get; } = new(630, "EForms");
    public static PageType EFormDetails { get; } = new(631, "EForm Details");
    public static PageType EFormSubmissionConfirmation { get; } = new(632, "EForm Submission Confirmation");
    public static PageType EmployerRegistrationSummary { get; } = new(640, "Employer Registration Summary");
    public static PageType EmployerRegistrationVerification { get; } = new(641, "Employer Registration Verification");
    public static PageType EmployerRegistrationPlannedPath { get; } = new(642, "Employer Registration Planned Path");
    public static PageType EmployerRegistrationSuccess { get; } = new(643, "Employer Registration Success");
    public static PageType ESPDashboard { get; } = new(650, "ESP Dashboard");
    public static PageType ESPContact { get; } = new(651, "ESP Contact");
    public static PageType ESPGetAccess { get; } = new(652, "ESP Get Access");
    public static PageType ESPMakeACHPaymentInformation { get; } = new(653, "ESP Make ACH Payment Information");
    public static PageType ESPPaymentHistoryDetails { get; } = new(654, "ESP Payment History Details");
    public static PageType ESPProfile { get; } = new(655, "ESP Profile");
    public static PageType ESPRegistration { get; } = new(656, "ESP Registration");
    public static PageType ESPGetClientAccess { get; } = new(657, "ESP Get Client Access");
    public static PageType ESPMakeACHPayment { get; } = new(658, "ESP Make ACH Payment");
    public static PageType ESPManageClientAccounts { get; } = new(659, "ESP Manage Client Accounts");
    public static PageType DebtorLevies { get; } = new(670, "Debtor Levies");
    public static PageType LevyAnswerResponseWage { get; } = new(671, "Levy Answer Response Wage");
    public static PageType LevyAnswerResponseFraud { get; } = new(672, "Levy Answer Response Fraud");
    public static PageType LevyAuthorizedRepresentativeWage { get; } = new(673, "Levy Authorized Representative Wage");
    public static PageType LevyAuthorizedRepresentativeFraud { get; } = new(674, "Levy Authorized Representative Fraud");
    public static PageType LevyBankAccountPaymentACH { get; } = new(675, "Levy Bank Account Payment ACH");
    public static PageType LevyBankCalculator { get; } = new(676, "Levy Bank Calculator");
    public static PageType LevyDetails { get; } = new(677, "Levy Details");
    public static PageType LevyDocuments { get; } = new(678, "Levy Documents");
    public static PageType LevyPayments { get; } = new(679, "Levy Payments");
    public static PageType LevyTaxCalculator { get; } = new(680, "Levy Tax Calculator");
    public static PageType CorporateOfficerExclusion { get; } = new(690, "Corporate Officer Exclusion");
    public static PageType EditUserPermissions { get; } = new(691, "Edit User Permissions");
    public static PageType GeneratedAccessKey { get; } = new(692, "Generated Access Key");
    public static PageType GenerateWorkerKeyPermissions { get; } = new(693, "Generate Worker Key Permissions");
    public static PageType GetAccountAccess { get; } = new(694, "Get Account Access");
    public static PageType ManageAccountHome { get; } = new(695, "Manage Account Home");
    public static PageType ManageESPAccess { get; } = new(696, "Manage ESP Access");
    public static PageType MailBenefitChargesAdjustmentsForm { get; } = new(700, "Mail Benefit Charges Adjustments Form");
    public static PageType SelectOtherFunctions { get; } = new(701, "Select Other Functions");
    public static PageType UIDocumentsWeeklyReserve { get; } = new(702, "UI Documents Weekly Reserve");
    public static PageType WageAdjustmentMultipleQuarter { get; } = new(710, "Wage Adjustment Multiple Quarter");
    public static PageType MissingWageAndTaxReports { get; } = new(711, "Missing Wage And Tax Reports");
    public static PageType TaxAndWageEntryReport { get; } = new(712, "Tax And Wage Entry Report");
    public static PageType ZeroPayrollTaxReport { get; } = new(713, "Zero Payroll Tax Report");
    public static PageType RulesOfConduct { get; } = new(720, "Rules Of Conduct");
    public static PageType SecureMessages { get; } = new(730, "Secure Messages");
    public static PageType SecureMessageThread { get; } = new(731, "Secure Message Thread");
    public static PageType NewSecureMessage { get; } = new(732, "New Secure Message");
    public static PageType UIDocuments { get; } = new(740, "UI Documents");
    public static PageType UserSettingsHome { get; } = new(750, "User Settings Home");
    public static PageType CommunicationPreferences { get; } = new(751, "Communication Preferences");
    public static PageType UserProfile { get; } = new(752, "User Profile");
    public static PageType WebSessionTransfer = new(999, "Web Session Transfer");
}
