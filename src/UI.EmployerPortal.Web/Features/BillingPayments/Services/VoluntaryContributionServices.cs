using UI.EmployerPortal.Generated.ServiceClients.VoluntaryContributionService;
using UI.EmployerPortal.Web.Features.BillingPayments.Models;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Services;
using UI.EmployerPortal.Web.Features.Shared.QuarterlyTax.Models;
using UI.EmployerPortal.Web.Startup.ResiliencyProtocols;
using recalcRequest = UI.EmployerPortal.Generated.ServiceClients.VoluntaryContributionService.RecalculateSavingsTaxInfoRequest;
using voluntaryRequest = UI.EmployerPortal.Generated.ServiceClients.VoluntaryContributionService.VoluntaryRequest;
namespace UI.EmployerPortal.Web.Features.BillingPayments.Services;
/// <summary>
/// VoluntaryContributionServices
/// </summary>
public interface IVoluntaryContributionServices
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    Task<VoluntaryContribution?> GetVoluntaryContributionDetail(voluntaryRequest request);
    /// <summary>
    /// CheckDeferralEligibilityAsync
    /// </summary>
    /// <param name="employerSk"></param>
    /// <param name="securesk"></param>
    /// <returns></returns>
    Task<VoluntaryContributionIneligibe> GetVoluntaryContributionIneligible(int employerSk, int securesk);
    /// <summary>
    /// Show VC Info Based period
    /// </summary>
    /// <returns></returns>
    Task<bool> ShowVoluntaryInformation();
    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    Task<VoluntaryContribution?> GetRecalculateVoluntaryContributionDetail(recalcRequest request);
}
internal class VoluntaryContributionServices : IVoluntaryContributionServices
{

    private readonly IAsyncRetryPolicy<BillDetailServices> _retryPolicy;
    private readonly IVoluntaryContributionService _voluntaryContributionService;
    private readonly IUserAccountService _userAccountService;

    public VoluntaryContributionServices(
             IAsyncRetryPolicy<BillDetailServices> retryPolicy,
            IUserAccountService userAccountService,
            IVoluntaryContributionService voluntaryContributionService)
    {
        _retryPolicy = retryPolicy;
        _userAccountService = userAccountService;
        _voluntaryContributionService = voluntaryContributionService;
    }
    public async Task<VoluntaryContribution?> GetVoluntaryContributionDetail(voluntaryRequest request)
    {
        var response = await _retryPolicy.ExecuteAsync(() =>
        {
            return _voluntaryContributionService.VoluntaryContributionCalculatorAsync(request);


        });
        return response?.RuleViolations == null ? (VoluntaryContribution?) null : MapVCDetailtoModel(response);
        ;
    }
    private static VoluntaryContribution MapVCDetailtoModel(VoluntaryContributionCalculatorResponse response)
    {
        return new VoluntaryContribution
        {
            EstimatedTaxablePayroll = response.EstimatedTaxablePayroll,
            PaymentAmount = response.PaymentAmount,
            NetTaxSavings = response.NetTaxSavings,
            ReserveFundBalance = response.ReserveFundBalance,
            ReserveFundPercentage = response.ReserveFundPercentage,
            TaxRateForYear = response.TaxRateForYear,
            TaxSavingsBasedOnEstimatedPayroll = response.TaxSavingsBasedOnEstimatedPayroll,
            TaxablePayRoll = response.TaxablePayRoll,
            VcRequired = response.VcRequired,
            Lowerrate = response.VoluntaryNextLowerRate
        };
    }

    public async Task<VoluntaryContributionIneligibe> GetVoluntaryContributionIneligible(int employerSk, int securesk)
    {
        var secureusersk = _userAccountService.GetUserSKClaim();
        var request = new EmployerRequest
        {
            EmployerSK = employerSk,
            SecureUserSK = secureusersk
        };

        var eligibilityResponse = await _voluntaryContributionService.IsEligibleForVoluntaryContributionAsync(request);
        var eligible = eligibilityResponse.Eligibility.IsEligible;

        var ruleViolations = new List<RuleViolationItem>();
        if (eligibilityResponse.RuleViolations.Length > 0)
        {
            foreach (var viloation in eligibilityResponse.RuleViolations)
            {
                ruleViolations.Add(new RuleViolationItem { RuleID = viloation.RuleID, RuleViolation = viloation.RuleViolation });
            }
        }

        return new VoluntaryContributionIneligibe
        {
            Eligible = eligible,
            RuleViolations = ruleViolations,
        };
    }
    public async Task<VoluntaryContribution?> GetRecalculateVoluntaryContributionDetail(recalcRequest request)
    {

        var response = await _retryPolicy.ExecuteAsync(() =>
        {
            return _voluntaryContributionService.GetReCalculatedTaxSavingsAsync(request);


        });
        var ruleViolations = new List<RuleViolationItem>();
        if (response.RuleViolations.Length > 0)
        {
            foreach (var viloation in response.RuleViolations)
            {
                ruleViolations.Add(new RuleViolationItem { RuleID = viloation.RuleID, RuleViolation = viloation.RuleViolation });
            }
        }
        return response?.RuleViolations == null ? (VoluntaryContribution?) null : MapRecalcVCDetailtoModel(response);
        ;
    }
    private static VoluntaryContribution MapRecalcVCDetailtoModel(VoluntaryRecalculatedTaxInfo response)
    {
        return new VoluntaryContribution
        {
            DisclaimerText = response.DisclaimerText,
            EstimatedTaxablePayroll = response.EstimatedTaxablePayroll,
            PaymentAmount = response.PaymentAmount,
            NetTaxSavings = response.NetTaxSavings,
            ReserveFundBalance = response.ReserveFundBalance,
            ReserveFundPercentage = response.ReserveFundPercentage,
            TaxRateForYear = response.TaxRateForYear,
            TaxSavingsBasedOnEstimatedPayroll = decimal.TryParse(response.Savings, out var saving) ? saving : 0m,
            TaxablePayRoll = response.TaxablePayRoll,
            VcRequired = response.VcRequired,
            Lowerrate = response.NextLowerRate


        };
    }
    public async Task<bool> ShowVoluntaryInformation()
    {


        var eligibilityResponse = await _voluntaryContributionService.ShowVoluntaryInformationAsync();


        var ruleViolations = new List<RuleViolationItem>();
        if (eligibilityResponse.RuleViolations.Length > 0)
        {
            foreach (var viloation in eligibilityResponse.RuleViolations)
            {
                ruleViolations.Add(new RuleViolationItem { RuleID = viloation.RuleID, RuleViolation = viloation.RuleViolation });
            }
        }

        return eligibilityResponse.Value;
    }
}
