using UI.EmployerPortal.Generated.ServiceClients.TaxWageAdjustmentService;
using UI.EmployerPortal.Web.Features.Shared.QuarterlyTax.Models;
using UI.EmployerPortal.Web.Startup.ResiliencyProtocols;
using BoolResponse = UI.EmployerPortal.Generated.ServiceClients.TaxWageAdjustmentService.BoolResponse;

namespace UI.EmployerPortal.Web.Features.Shared.QuarterlyTax.Services;

/// <summary>
///
/// </summary>

public interface IWageAdjustmentReasonsService
{
    /// <summary>
    ///
    /// </summary>
    Task<IEnumerable<WageAdjustmentReasonsModel>> GetWageAdjustmentReasons();

    /// <summary>
    ///
    /// </summary>
    Task<WageAdjustmentBySSNResponse> GetWageAdjustmentBySSNDetail(WageAdjustmentBySNNRequest request);


    /// <summary>
    ///
    /// </summary>
    Task<WageDetailResponse> GetWageDetailsBySSN(WageDetailByUserRequest request);

    /// <summary>
    ///
    /// </summary>
    Task<SaveWageAdjustmentResponse> SaveWageAdjustmentBySSN(WageAdjustmentBySSNDetailRequest request);

    /// <summary>
    ///
    /// </summary>

    Task<BoolResponse> SavePendingWageReportAdjustmentByEmployeeAsync(WageAdjustmentBySSNDetailRequest request);
    /// <summary>
    ///
    /// </summary>
    Task<BoolResponse> DeletePendingWageReportAdjustmentByEmployeeAsync(WageAdjustmentDeletePendingRequest request);
}

/// <summary>
///
/// </summary>
internal class WageAdjustmentReasonsService : IWageAdjustmentReasonsService
{
    private readonly ITaxWageAdjustmentService _taxWageAdjustmentService;
    private readonly IAsyncRetryPolicy<WageAdjustmentReasonsService> _retryPolicy;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="taxWageAdjustmentService"></param>
    /// <param name="retryPolicy"></param>
    public WageAdjustmentReasonsService(ITaxWageAdjustmentService taxWageAdjustmentService,
        IAsyncRetryPolicy<WageAdjustmentReasonsService> retryPolicy)

    {
        _taxWageAdjustmentService = taxWageAdjustmentService;
        _retryPolicy = retryPolicy;
    }

    /// <summary>
    /// Get Adjustment Reasons
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<IEnumerable<WageAdjustmentReasonsModel>> GetWageAdjustmentReasons()
    {

        List<WageAdjustmentReasonsModel> wageAdjustmentReasonsModel = new();
        var reasons = await _retryPolicy.ExecuteAsync(() =>
        {
            return _taxWageAdjustmentService.ObtainWageAdjustmentReasonsAsync();
        });

        foreach (var reason in reasons)
        {
            WageAdjustmentReasonsModel model = new();
            model.ReasonText = reason.ReasonText;
            model.CodeSk = reason.CodeSK;

            wageAdjustmentReasonsModel.Add(model);
        }

        return wageAdjustmentReasonsModel;
    }


    public async Task<WageAdjustmentBySSNResponse> GetWageAdjustmentBySSNDetail(WageAdjustmentBySNNRequest request)
    {

        var res = _taxWageAdjustmentService.LoadWageAdjustmentBySSNDetailAsync(request);

        var wageAdjustments = await _retryPolicy.ExecuteAsync(() =>
        {
            return _taxWageAdjustmentService.LoadWageAdjustmentBySSNDetailAsync(request);
        });

        return wageAdjustments;
    }

    public async Task<WageDetailResponse> GetWageDetailsBySSN(WageDetailByUserRequest request)
    {
        var wageAdjustments = await _retryPolicy.ExecuteAsync(() =>
        {
            return _taxWageAdjustmentService.SearchWageDetailAsync(request);
        });

        return wageAdjustments;
    }


    public async Task<SaveWageAdjustmentResponse> SaveWageAdjustmentBySSN(WageAdjustmentBySSNDetailRequest request)
    {
        return await _retryPolicy.ExecuteAsync(() =>
        {
            return _taxWageAdjustmentService.SaveWageAdjustmentBySSNAsync(request);
        });
    }

    public async Task<BoolResponse> SavePendingWageReportAdjustmentByEmployeeAsync(WageAdjustmentBySSNDetailRequest request)
    {
        var serviceResponse = await _retryPolicy.ExecuteAsync(() =>
        {
            return _taxWageAdjustmentService.SavePendingWageReportAdjustmentByEmployeeAsync(request);
        });

        return serviceResponse;


    }

    public async Task<BoolResponse> DeletePendingWageReportAdjustmentByEmployeeAsync(WageAdjustmentDeletePendingRequest request)
    {
        var response = await _retryPolicy.ExecuteAsync(() =>
        {
            return _taxWageAdjustmentService.DeletePendingWageReportAdjustmentByEmployeeAsync(request);
        });
        return response;
    }
}
