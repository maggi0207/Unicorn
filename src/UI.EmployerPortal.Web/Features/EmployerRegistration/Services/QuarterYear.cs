namespace UI.EmployerPortal.Web.Features.EmployerRegistration.Services;

/// <summary>
/// The calendar Quarter and Year
/// </summary>
public class QuarterYear
{
    /// <summary>
    /// The calender Quarter.  Only values 1-4 are valid
    /// </summary>
    public int Quarter { get; set; }

    /// <summary>
    /// The calendar Year
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bool IsValid()
    {
        return Quarter > 0 && Quarter <= 4 && Year > 0;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"{Quarter}/{Year}";
    }
}
