namespace UI.EmployerPortal.Web.Features.EmployerRegistration.Models;

/// <summary>
/// Extension methods for PrincipalBusinessActivityType enum
/// </summary>
public static class PrincipalBusinessActivityTypeExtensions
{
    private static readonly Dictionary<PrincipalBusinessActivityType, string> DisplayNames = new()
    {
        { PrincipalBusinessActivityType.None, "" },
        { PrincipalBusinessActivityType.AccommodationAndFoodService, "Accommodation and Food Service" },
        { PrincipalBusinessActivityType.AccommodationFoodCarryoutDelivery, "Accommodation and Food Service - Food (Carryout-Delivery)" },
        { PrincipalBusinessActivityType.AccommodationFoodDrinkEstablishments, "Accommodation and Food Service - Food and/or Drink Establishments" },
        { PrincipalBusinessActivityType.AccommodationLodging, "Accommodation and Food Service - Lodging" },
        { PrincipalBusinessActivityType.AgricultureRaisingCropsFood, "Agriculture - Raising Crops/Food" },
        { PrincipalBusinessActivityType.AgricultureRaisingLivestock, "Agriculture - Raising Livestock" },
        { PrincipalBusinessActivityType.AgricultureFarming, "Agriculture (Farming)" },
        { PrincipalBusinessActivityType.ChildcareDaycareCenter, "Childcare/Daycare Center" },
        { PrincipalBusinessActivityType.ConstructionSpecialtyTradeRelated, "Construction/Specialty Trade Related" },
        { PrincipalBusinessActivityType.ConstructionSpecialtyTradeElectronicsInstallations, "Construction/Specialty Trade Related - Electronics installations" },
        { PrincipalBusinessActivityType.ConstructionSpecialtyTradeFlooringExceptHardwood, "Construction/Specialty Trade Related - Flooring(Except Hardwood)" },
        { PrincipalBusinessActivityType.ConstructionSpecialtyTradeHeatingAndCooling, "Construction/Specialty Trade Related - Heating and Cooling" },
        { PrincipalBusinessActivityType.ConstructionSpecialtyTradeWhitewashing, "Construction/Specialty Trade Related - Whitewashing" },
        { PrincipalBusinessActivityType.ConstructionSpecialtyTrades, "Construction/Specialty Trades" },
        { PrincipalBusinessActivityType.ConstructionSpecialtyTradesCarpentry, "Construction/Specialty Trades - Carpentry" },
        { PrincipalBusinessActivityType.ConstructionSpecialtyTradesConcrete, "Construction/Specialty Trades - Concrete" },
        { PrincipalBusinessActivityType.ConstructionSpecialtyTradesEarthMoving, "Construction/Specialty Trades - Earth Moving" },
        { PrincipalBusinessActivityType.ConstructionSpecialtyTradesElectricians, "Construction/Specialty Trades - Electricians" },
        { PrincipalBusinessActivityType.ConstructionSpecialtyTradesHardwoodFlooring, "Construction/Specialty Trades - Hardwood flooring" },
        { PrincipalBusinessActivityType.ConstructionSpecialtyTradesIronWork, "Construction/Specialty Trades - Iron work" },
        { PrincipalBusinessActivityType.ConstructionSpecialtyTradesPainters, "Construction/Specialty Trades - Painters" },
        { PrincipalBusinessActivityType.ConstructionSpecialtyTradesPlumbers, "Construction/Specialty Trades - Plumbers" },
        { PrincipalBusinessActivityType.ConstructionSpecialtyTradesRemodelingRepairAdditions, "Construction/Specialty Trades - Remodeling, Repair, Additions" },
        { PrincipalBusinessActivityType.ConstructionSpecialtyTradesRoadWork, "Construction/Specialty Trades - Road work" },
        { PrincipalBusinessActivityType.ConstructionSpecialtyTradesRoofing, "Construction/Specialty Trades - Roofing" },
        { PrincipalBusinessActivityType.ConstructionSpecialtyTradesSiding, "Construction/Specialty Trades - Siding" },
        { PrincipalBusinessActivityType.ConstructionSpecialtyTradesUtilityConstruction, "Construction/Specialty Trades - Utility Construction" },
        { PrincipalBusinessActivityType.Consultants, "Consultants" },
        { PrincipalBusinessActivityType.DomesticEmployNannyOrBabysitter, "Domestic - Employ Nanny or Babysitter in Your Own Home" },
        { PrincipalBusinessActivityType.DomesticFiscalAgentElectingToBeEmployer, "Domestic - Fiscal Agent Electing to be Employer" },
        { PrincipalBusinessActivityType.DomesticRecipientOfHomeHelp, "Domestic - Recipient of Home Help (Cleaning, Laundry, Lawn Mowing, etc.)" },
        { PrincipalBusinessActivityType.DomesticRecipientOfInHomeHealthcare, "Domestic - Recipient of In-Home Healthcare" },
        { PrincipalBusinessActivityType.EducationRecreationAndTraining, "Education, Recreation, and Training" },
        { PrincipalBusinessActivityType.EducationRecreationDanceStudio, "Education, Recreation, and Training - Dance Studio" },
        { PrincipalBusinessActivityType.EducationRecreationGymnastics, "Education, Recreation, and Training - Gymnastics" },
        { PrincipalBusinessActivityType.EducationRecreationPreschool, "Education, Recreation, and Training - Preschool" },
        { PrincipalBusinessActivityType.EducationRecreationPrivateSchool, "Education, Recreation, and Training - Private School (All Public Schools Use Government Agency)" },
        { PrincipalBusinessActivityType.EducationRecreationSportsFacility, "Education, Recreation, and Training - Sports Facility" },
        { PrincipalBusinessActivityType.EducationRecreationTheatres, "Education, Recreation, and Training - Theatres and Related (Actors, Ushers, Technical Staff)" },
        { PrincipalBusinessActivityType.EducationRecreationTutoring, "Education, Recreation, and Training - Tutoring" },
        { PrincipalBusinessActivityType.EmployerServices, "Employer Services" },
        { PrincipalBusinessActivityType.EmployerServicesEmployeeLeasingCompany, "Employer Services - Employee Leasing Company" },
        { PrincipalBusinessActivityType.EmployerServicesPayrollService, "Employer Services - Payroll Service" },
        { PrincipalBusinessActivityType.EmployerServicesProfessionalEmployerOrganization, "Employer Services - Professional Employer Organization (PEO)" },
        { PrincipalBusinessActivityType.EmployerServicesTemporaryHelpService, "Employer Services - Temporary Help Service" },
        { PrincipalBusinessActivityType.FinanceInsuranceAndLegal, "Finance, Insurance, and Legal" },
        { PrincipalBusinessActivityType.FinanceInsuranceLegalAccountants, "Finance, Insurance, and Legal - Accountants" },
        { PrincipalBusinessActivityType.FinanceInsuranceLegalBank, "Finance, Insurance, and Legal - Bank" },
        { PrincipalBusinessActivityType.FinanceInsuranceLegalCheckCashing, "Finance, Insurance, and Legal - Check cashing" },
        { PrincipalBusinessActivityType.FinanceInsuranceLegalCreditUnion, "Finance, Insurance, and Legal - Credit Union" },
        { PrincipalBusinessActivityType.FinanceInsuranceLegalInsurance, "Finance, Insurance, and Legal - Insurance" },
        { PrincipalBusinessActivityType.FinanceInsuranceLegalInvestmentFirm, "Finance, Insurance, and Legal - Investment firm" },
        { PrincipalBusinessActivityType.FinanceInsuranceLegalLawyers, "Finance, Insurance, and Legal - Lawyers" },
        { PrincipalBusinessActivityType.FinanceInsuranceLegalSavingsAndLoan, "Finance, Insurance, and Legal - Savings and Loan" },
        { PrincipalBusinessActivityType.FinanceInsuranceLegalTitleCompany, "Finance, Insurance, and Legal - Title Company" },
        { PrincipalBusinessActivityType.GovernmentAgency, "Government Agency" },
        { PrincipalBusinessActivityType.HealthCareAndSocialAssistance, "Health Care and Social Assistance" },
        { PrincipalBusinessActivityType.HealthcareChiropractors, "Healthcare and Social Assistance - Chiropractors" },
        { PrincipalBusinessActivityType.HealthcareClinics, "Healthcare and Social Assistance - Clinics" },
        { PrincipalBusinessActivityType.HealthcareCounseling, "Healthcare and Social Assistance - Counseling" },
        { PrincipalBusinessActivityType.HealthcareDental, "Healthcare and Social Assistance - Dental" },
        { PrincipalBusinessActivityType.HealthcareHospital, "Healthcare and Social Assistance - Hospital" },
        { PrincipalBusinessActivityType.HealthcareMassageTherapy, "Healthcare and Social Assistance - Massage Therapy" },
        { PrincipalBusinessActivityType.HealthcarePhysicians, "Healthcare and Social Assistance - Physicians" },
        { PrincipalBusinessActivityType.HealthcareProviderOfInHomeHealthcare, "Healthcare and Social Assistance - Provider of In-Home Healthcare" },
        { PrincipalBusinessActivityType.ITServicesConsulting, "IT Services/Consulting" },
        { PrincipalBusinessActivityType.Manufacturing, "Manufacturing" },
        { PrincipalBusinessActivityType.Other, "Other (*Specify)" },
        { PrincipalBusinessActivityType.RealEstate, "Real Estate" },
        { PrincipalBusinessActivityType.RealEstateManagement, "Real Estate - Management" },
        { PrincipalBusinessActivityType.RealEstateSales, "Real Estate - Sales" },
        { PrincipalBusinessActivityType.RentalAndLeasing, "Rental and Leasing" },
        { PrincipalBusinessActivityType.RentalLeasingClothing, "Rental and Leasing - Clothing" },
        { PrincipalBusinessActivityType.RentalLeasingEquipment, "Rental and Leasing - Equipment" },
        { PrincipalBusinessActivityType.RentalLeasingHouseholdGoods, "Rental and Leasing - Household goods" },
        { PrincipalBusinessActivityType.RentalLeasingHousing, "Rental and Leasing - Housing" },
        { PrincipalBusinessActivityType.RentalLeasingRealProperty, "Rental and Leasing - Real Property" },
        { PrincipalBusinessActivityType.ResidentialCareFacility, "Residential Care Facility (CBRF)" },
        { PrincipalBusinessActivityType.Retail, "Retail" },
        { PrincipalBusinessActivityType.RetailClothing, "Retail - Clothing" },
        { PrincipalBusinessActivityType.RetailDepartmentStore, "Retail - Department Store" },
        { PrincipalBusinessActivityType.RetailFoodSales, "Retail - Food Sales" },
        { PrincipalBusinessActivityType.RetailFurniture, "Retail - Furniture" },
        { PrincipalBusinessActivityType.RetailHardware, "Retail - Hardware" },
        { PrincipalBusinessActivityType.RetailMotorVehicles, "Retail - Motor Vehicles" },
        { PrincipalBusinessActivityType.RetailPharmacy, "Retail - Pharmacy" },
        { PrincipalBusinessActivityType.RetailSpecialty, "Retail - Specialty (Meat, Jewelry, Tobacco)" },
        { PrincipalBusinessActivityType.Sales, "Sales" },
        { PrincipalBusinessActivityType.Services, "Services" },
        { PrincipalBusinessActivityType.ServicesDrycleaners, "Services - Drycleaners" },
        { PrincipalBusinessActivityType.ServicesInvestigators, "Services - Investigators" },
        { PrincipalBusinessActivityType.ServicesLandscapers, "Services - Landscapers" },
        { PrincipalBusinessActivityType.ServicesLaundromats, "Services - Laundromats" },
        { PrincipalBusinessActivityType.ServicesLoggers, "Services - Loggers" },
        { PrincipalBusinessActivityType.ServicesMessengers, "Services - Messengers" },
        { PrincipalBusinessActivityType.ServicesMovers, "Services - Movers" },
        { PrincipalBusinessActivityType.ServicesProviderOfHomeServices, "Services - Provider of Home Services (Cleaning, Lawn Mowing, Laundry, Nanny/Babysitter)" },
        { PrincipalBusinessActivityType.ServicesSecurityCompanies, "Services - Security Companies" },
        { PrincipalBusinessActivityType.ServicesTattoosAndPiercing, "Services - Tattoos and Piercing" },
        { PrincipalBusinessActivityType.ServicesTravelAgents, "Services - Travel Agents" },
        { PrincipalBusinessActivityType.ServicesTreeService, "Services - Tree Service" },
        { PrincipalBusinessActivityType.ServicesUndertakers, "Services - Undertakers" },
        { PrincipalBusinessActivityType.ServicesVeterinaryClinics, "Services - Veterinary Clinics" },
        { PrincipalBusinessActivityType.SoftwareDevelopment, "Software Development" },
        { PrincipalBusinessActivityType.TransportationAndWarehousing, "Transportation and Warehousing" },
        { PrincipalBusinessActivityType.TransportationWarehousingBuses, "Transportation and Warehousing - Buses" },
        { PrincipalBusinessActivityType.TransportationWarehousingStorageAndStorageUnits, "Transportation and Warehousing - Storage and Storage Units" },
        { PrincipalBusinessActivityType.TransportationWarehousingTaxis, "Transportation and Warehousing - Taxis" },
        { PrincipalBusinessActivityType.TransportationWarehousingTruckers, "Transportation and Warehousing - Truckers" },
        { PrincipalBusinessActivityType.TransportationWarehousingWarehouses, "Transportation and Warehousing - Warehouses" },
        { PrincipalBusinessActivityType.WholesaleAgentBroker, "Wholesale - Agent/Broker" },
        { PrincipalBusinessActivityType.WholesaleOther, "Wholesale - Other" }
    };

    /// <summary>
    /// Gets the display name for the business activity type
    /// </summary>
    public static string GetDisplayName(this PrincipalBusinessActivityType type)
    {
        return DisplayNames.TryGetValue(type, out var name) ? name : type.ToString();
    }

    /// <summary>
    /// Checks if the activity type is construction-related
    /// </summary>
    public static bool IsConstructionRelated(this PrincipalBusinessActivityType type)
    {
        return type switch
        {
            PrincipalBusinessActivityType.ConstructionSpecialtyTradeRelated or
            PrincipalBusinessActivityType.ConstructionSpecialtyTradeElectronicsInstallations or
            PrincipalBusinessActivityType.ConstructionSpecialtyTradeFlooringExceptHardwood or
            PrincipalBusinessActivityType.ConstructionSpecialtyTradeHeatingAndCooling or
            PrincipalBusinessActivityType.ConstructionSpecialtyTradeWhitewashing or
            PrincipalBusinessActivityType.ConstructionSpecialtyTrades or
            PrincipalBusinessActivityType.ConstructionSpecialtyTradesCarpentry or
            PrincipalBusinessActivityType.ConstructionSpecialtyTradesConcrete or
            PrincipalBusinessActivityType.ConstructionSpecialtyTradesEarthMoving or
            PrincipalBusinessActivityType.ConstructionSpecialtyTradesElectricians or
            PrincipalBusinessActivityType.ConstructionSpecialtyTradesHardwoodFlooring or
            PrincipalBusinessActivityType.ConstructionSpecialtyTradesIronWork or
            PrincipalBusinessActivityType.ConstructionSpecialtyTradesPainters or
            PrincipalBusinessActivityType.ConstructionSpecialtyTradesPlumbers or
            PrincipalBusinessActivityType.ConstructionSpecialtyTradesRemodelingRepairAdditions or
            PrincipalBusinessActivityType.ConstructionSpecialtyTradesRoadWork or
            PrincipalBusinessActivityType.ConstructionSpecialtyTradesRoofing or
            PrincipalBusinessActivityType.ConstructionSpecialtyTradesSiding or
            PrincipalBusinessActivityType.ConstructionSpecialtyTradesUtilityConstruction => true,
            _ => false
        };
    }
}
