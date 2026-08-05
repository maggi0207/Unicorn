using System.ComponentModel.DataAnnotations;

namespace UI.EmployerPortal.Web.Features.EmployerRegistration.Models;

/// <summary>
/// Principal Business Activity types matching legacy system IDs.
/// </summary>
public enum PrincipalBusinessActivityType
{
    /// <summary>
    /// No business activity selected (default/empty state)
    /// </summary>
    None = 0,

    /// <summary>
    /// Accommodation and Food Service (ID: 112)
    /// </summary>
    [Display(Name = "Accommodation and Food Service")]
    AccommodationAndFoodService = 112,

    /// <summary>
    /// Accommodation and Food Service - Food (Carryout-Delivery) (ID: 23)
    /// </summary>
    [Display(Name = "Accommodation and Food Service - Food (Carryout-Delivery)")]
    AccommodationFoodCarryoutDelivery = 23,

    /// <summary>
    /// Accommodation and Food Service - Food and/or Drink Establishments (ID: 22)
    /// </summary>
    [Display(Name = "Accommodation and Food Service - Food and/or Drink Establishments")]
    AccommodationFoodDrinkEstablishments = 22,

    /// <summary>
    /// Accommodation and Food Service - Lodging (ID: 21)
    /// </summary>
    [Display(Name = "Accommodation and Food Service - Lodging")]
    AccommodationLodging = 21,

    /// <summary>
    /// Agriculture - Raising Crops/Food (ID: 24)
    /// </summary>
    [Display(Name = "Agriculture - Raising Crops/Food")]
    AgricultureRaisingCropsFood = 24,

    /// <summary>
    /// Agriculture - Raising Livestock (ID: 25)
    /// </summary>
    [Display(Name = "Agriculture - Raising Livestock")]
    AgricultureRaisingLivestock = 25,

    /// <summary>
    /// Agriculture (Farming) (ID: 117)
    /// </summary>
    [Display(Name = "Agriculture (Farming)")]
    AgricultureFarming = 117,

    /// <summary>
    /// Childcare/Daycare Center (ID: 127)
    /// </summary>
    [Display(Name = "Childcare/Daycare Center")]
    ChildcareDaycareCenter = 127,

    /// <summary>
    /// Construction/Specialty Trade Related (ID: 120)
    /// </summary>
    [Display(Name = "Construction/Specialty Trade Related")]
    ConstructionSpecialtyTradeRelated = 120,

    /// <summary>
    /// Construction/Specialty Trade Related - Electronics installations (ID: 39)
    /// </summary>
    [Display(Name = "Construction/Specialty Trade Related - Electronics installations")]
    ConstructionSpecialtyTradeElectronicsInstallations = 39,

    /// <summary>
    /// Construction/Specialty Trade Related - Flooring(Except Hardwood) (ID: 40)
    /// </summary>
    [Display(Name = "Construction/Specialty Trade Related - Flooring(Except Hardwood)")]
    ConstructionSpecialtyTradeFlooringExceptHardwood = 40,

    /// <summary>
    /// Construction/Specialty Trade Related - Heating and Cooling (ID: 41)
    /// </summary>
    [Display(Name = "Construction/Specialty Trade Related - Heating and Cooling")]
    ConstructionSpecialtyTradeHeatingAndCooling = 41,

    /// <summary>
    /// Construction/Specialty Trade Related - Whitewashing (ID: 42)
    /// </summary>
    [Display(Name = "Construction/Specialty Trade Related - Whitewashing")]
    ConstructionSpecialtyTradeWhitewashing = 42,

    /// <summary>
    /// Construction/Specialty Trades (ID: 109)
    /// </summary>
    [Display(Name = "Construction/Specialty Trades")]
    ConstructionSpecialtyTrades = 109,

    /// <summary>
    /// Construction/Specialty Trades - Carpentry (ID: 26)
    /// </summary>
    [Display(Name = "Construction/Specialty Trades - Carpentry")]
    ConstructionSpecialtyTradesCarpentry = 26,

    /// <summary>
    /// Construction/Specialty Trades - Concrete (ID: 27)
    /// </summary>
    [Display(Name = "Construction/Specialty Trades - Concrete")]
    ConstructionSpecialtyTradesConcrete = 27,

    /// <summary>
    /// Construction/Specialty Trades - Earth Moving (ID: 28)
    /// </summary>
    [Display(Name = "Construction/Specialty Trades - Earth Moving")]
    ConstructionSpecialtyTradesEarthMoving = 28,

    /// <summary>
    /// Construction/Specialty Trades - Electricians (ID: 29)
    /// </summary>
    [Display(Name = "Construction/Specialty Trades - Electricians")]
    ConstructionSpecialtyTradesElectricians = 29,

    /// <summary>
    /// Construction/Specialty Trades - Hardwood flooring (ID: 30)
    /// </summary>
    [Display(Name = "Construction/Specialty Trades - Hardwood flooring")]
    ConstructionSpecialtyTradesHardwoodFlooring = 30,

    /// <summary>
    /// Construction/Specialty Trades - Iron work (ID: 31)
    /// </summary>
    [Display(Name = "Construction/Specialty Trades - Iron work")]
    ConstructionSpecialtyTradesIronWork = 31,

    /// <summary>
    /// Construction/Specialty Trades - Painters (ID: 32)
    /// </summary>
    [Display(Name = "Construction/Specialty Trades - Painters")]
    ConstructionSpecialtyTradesPainters = 32,

    /// <summary>
    /// Construction/Specialty Trades - Plumbers (ID: 33)
    /// </summary>
    [Display(Name = "Construction/Specialty Trades - Plumbers")]
    ConstructionSpecialtyTradesPlumbers = 33,

    /// <summary>
    /// Construction/Specialty Trades - Remodeling, Repair, Additions (ID: 34)
    /// </summary>
    [Display(Name = "Construction/Specialty Trades - Remodeling, Repair, Additions")]
    ConstructionSpecialtyTradesRemodelingRepairAdditions = 34,

    /// <summary>
    /// Construction/Specialty Trades - Road work (ID: 35)
    /// </summary>
    [Display(Name = "Construction/Specialty Trades - Road work")]
    ConstructionSpecialtyTradesRoadWork = 35,

    /// <summary>
    /// Construction/Specialty Trades - Roofing (ID: 36)
    /// </summary>
    [Display(Name = "Construction/Specialty Trades - Roofing")]
    ConstructionSpecialtyTradesRoofing = 36,

    /// <summary>
    /// Construction/Specialty Trades - Siding (ID: 37)
    /// </summary>
    [Display(Name = "Construction/Specialty Trades - Siding")]
    ConstructionSpecialtyTradesSiding = 37,

    /// <summary>
    /// Construction/Specialty Trades - Utility Construction (ID: 38)
    /// </summary>
    [Display(Name = "Construction/Specialty Trades - Utility Construction")]
    ConstructionSpecialtyTradesUtilityConstruction = 38,

    /// <summary>
    /// Consultants (ID: 134)
    /// </summary>
    [Display(Name = "Consultants")]
    Consultants = 134,

    /// <summary>
    /// Domestic - Employ Nanny or Babysitter in Your Own Home (ID: 45)
    /// </summary>
    [Display(Name = "Domestic - Employ Nanny or Babysitter in Your Own Home")]
    DomesticEmployNannyOrBabysitter = 45,

    /// <summary>
    /// Domestic - Fiscal Agent Electing to be Employer (ID: 137)
    /// </summary>
    [Display(Name = "Domestic - Fiscal Agent Electing to be Employer")]
    DomesticFiscalAgentElectingToBeEmployer = 137,

    /// <summary>
    /// Domestic - Recipient of Home Help (Cleaning, Laundry, Lawn Mowing, etc.) (ID: 44)
    /// </summary>
    [Display(Name = "Domestic - Recipient of Home Help (Cleaning, Laundry, Lawn Mowing, etc.)")]
    DomesticRecipientOfHomeHelp = 44,

    /// <summary>
    /// Domestic - Recipient of In-Home Healthcare (ID: 43)
    /// </summary>
    [Display(Name = "Domestic - Recipient of In-Home Healthcare")]
    DomesticRecipientOfInHomeHealthcare = 43,

    /// <summary>
    /// Education, Recreation, and Training (ID: 121)
    /// </summary>
    [Display(Name = "Education, Recreation, and Training")]
    EducationRecreationAndTraining = 121,

    /// <summary>
    /// Education, Recreation, and Training - Dance Studio (ID: 46)
    /// </summary>
    [Display(Name = "Education, Recreation, and Training - Dance Studio")]
    EducationRecreationDanceStudio = 46,

    /// <summary>
    /// Education, Recreation, and Training - Gymnastics (ID: 47)
    /// </summary>
    [Display(Name = "Education, Recreation, and Training - Gymnastics")]
    EducationRecreationGymnastics = 47,

    /// <summary>
    /// Education, Recreation, and Training - Preschool (ID: 48)
    /// </summary>
    [Display(Name = "Education, Recreation, and Training - Preschool")]
    EducationRecreationPreschool = 48,

    /// <summary>
    /// Education, Recreation, and Training - Private School (All Public Schools Use Government Agency) (ID: 49)
    /// </summary>
    [Display(Name = "Education, Recreation, and Training - Private School (All Public Schools Use Government Agency)")]
    EducationRecreationPrivateSchool = 49,

    /// <summary>
    /// Education, Recreation, and Training - Sports Facility (ID: 50)
    /// </summary>
    [Display(Name = "Education, Recreation, and Training - Sports Facility")]
    EducationRecreationSportsFacility = 50,

    /// <summary>
    /// Education, Recreation, and Training - Theatres and Related (Actors, Ushers, Technical Staff) (ID: 51)
    /// </summary>
    [Display(Name = "Education, Recreation, and Training - Theatres and Related (Actors, Ushers, Technical Staff)")]
    EducationRecreationTheatres = 51,

    /// <summary>
    /// Education, Recreation, and Training - Tutoring (ID: 52)
    /// </summary>
    [Display(Name = "Education, Recreation, and Training - Tutoring")]
    EducationRecreationTutoring = 52,

    /// <summary>
    /// Employer Services (ID: 122)
    /// </summary>
    [Display(Name = "Employer Services")]
    EmployerServices = 122,

    /// <summary>
    /// Employer Services - Employee Leasing Company (ID: 53)
    /// </summary>
    [Display(Name = "Employer Services - Employee Leasing Company")]
    EmployerServicesEmployeeLeasingCompany = 53,

    /// <summary>
    /// Employer Services - Payroll Service (ID: 54)
    /// </summary>
    [Display(Name = "Employer Services - Payroll Service")]
    EmployerServicesPayrollService = 54,

    /// <summary>
    /// Employer Services - Professional Employer Organization (PEO) (ID: 55)
    /// </summary>
    [Display(Name = "Employer Services - Professional Employer Organization (PEO)")]
    EmployerServicesProfessionalEmployerOrganization = 55,

    /// <summary>
    /// Employer Services - Temporary Help Service (ID: 56)
    /// </summary>
    [Display(Name = "Employer Services - Temporary Help Service")]
    EmployerServicesTemporaryHelpService = 56,

    /// <summary>
    /// Finance, Insurance, and Legal (ID: 115)
    /// </summary>
    [Display(Name = "Finance, Insurance, and Legal")]
    FinanceInsuranceAndLegal = 115,

    /// <summary>
    /// Finance, Insurance, and Legal - Accountants (ID: 57)
    /// </summary>
    [Display(Name = "Finance, Insurance, and Legal - Accountants")]
    FinanceInsuranceLegalAccountants = 57,

    /// <summary>
    /// Finance, Insurance, and Legal - Bank (ID: 58)
    /// </summary>
    [Display(Name = "Finance, Insurance, and Legal - Bank")]
    FinanceInsuranceLegalBank = 58,

    /// <summary>
    /// Finance, Insurance, and Legal - Check cashing (ID: 59)
    /// </summary>
    [Display(Name = "Finance, Insurance, and Legal - Check cashing")]
    FinanceInsuranceLegalCheckCashing = 59,

    /// <summary>
    /// Finance, Insurance, and Legal - Credit Union (ID: 60)
    /// </summary>
    [Display(Name = "Finance, Insurance, and Legal - Credit Union")]
    FinanceInsuranceLegalCreditUnion = 60,

    /// <summary>
    /// Finance, Insurance, and Legal - Insurance (ID: 61)
    /// </summary>
    [Display(Name = "Finance, Insurance, and Legal - Insurance")]
    FinanceInsuranceLegalInsurance = 61,

    /// <summary>
    /// Finance, Insurance, and Legal - Investment firm (ID: 62)
    /// </summary>
    [Display(Name = "Finance, Insurance, and Legal - Investment firm")]
    FinanceInsuranceLegalInvestmentFirm = 62,

    /// <summary>
    /// Finance, Insurance, and Legal - Lawyers (ID: 63)
    /// </summary>
    [Display(Name = "Finance, Insurance, and Legal - Lawyers")]
    FinanceInsuranceLegalLawyers = 63,

    /// <summary>
    /// Finance, Insurance, and Legal - Savings and Loan (ID: 64)
    /// </summary>
    [Display(Name = "Finance, Insurance, and Legal - Savings and Loan")]
    FinanceInsuranceLegalSavingsAndLoan = 64,

    /// <summary>
    /// Finance, Insurance, and Legal - Title Company (ID: 65)
    /// </summary>
    [Display(Name = "Finance, Insurance, and Legal - Title Company")]
    FinanceInsuranceLegalTitleCompany = 65,

    /// <summary>
    /// Government Agency (ID: 119)
    /// </summary>
    [Display(Name = "Government Agency")]
    GovernmentAgency = 119,

    /// <summary>
    /// Health Care and Social Assistance (ID: 106)
    /// </summary>
    [Display(Name = "Health Care and Social Assistance")]
    HealthCareAndSocialAssistance = 106,

    /// <summary>
    /// Healthcare and Social Assistance - Chiropractors (ID: 66)
    /// </summary>
    [Display(Name = "Healthcare and Social Assistance - Chiropractors")]
    HealthcareChiropractors = 66,

    /// <summary>
    /// Healthcare and Social Assistance - Clinics (ID: 67)
    /// </summary>
    [Display(Name = "Healthcare and Social Assistance - Clinics")]
    HealthcareClinics = 67,

    /// <summary>
    /// Healthcare and Social Assistance - Counseling (ID: 68)
    /// </summary>
    [Display(Name = "Healthcare and Social Assistance - Counseling")]
    HealthcareCounseling = 68,

    /// <summary>
    /// Healthcare and Social Assistance - Dental (ID: 69)
    /// </summary>
    [Display(Name = "Healthcare and Social Assistance - Dental")]
    HealthcareDental = 69,

    /// <summary>
    /// Healthcare and Social Assistance - Hospital (ID: 70)
    /// </summary>
    [Display(Name = "Healthcare and Social Assistance - Hospital")]
    HealthcareHospital = 70,

    /// <summary>
    /// Healthcare and Social Assistance - Massage Therapy (ID: 71)
    /// </summary>
    [Display(Name = "Healthcare and Social Assistance - Massage Therapy")]
    HealthcareMassageTherapy = 71,

    /// <summary>
    /// Healthcare and Social Assistance - Physicians (ID: 72)
    /// </summary>
    [Display(Name = "Healthcare and Social Assistance - Physicians")]
    HealthcarePhysicians = 72,

    /// <summary>
    /// Healthcare and Social Assistance - Provider of In-Home Healthcare (ID: 129)
    /// </summary>
    [Display(Name = "Healthcare and Social Assistance - Provider of In-Home Healthcare")]
    HealthcareProviderOfInHomeHealthcare = 129,

    /// <summary>
    /// IT Services/Consulting (ID: 135)
    /// </summary>
    [Display(Name = "IT Services/Consulting")]
    ITServicesConsulting = 135,

    /// <summary>
    /// Manufacturing (ID: 114)
    /// </summary>
    [Display(Name = "Manufacturing")]
    Manufacturing = 114,

    /// <summary>
    /// Other (*Specify) (ID: 118)
    /// </summary>
    [Display(Name = "Other (*Specify)")]
    Other = 118,

    /// <summary>
    /// Real Estate (ID: 113)
    /// </summary>
    [Display(Name = "Real Estate")]
    RealEstate = 113,

    /// <summary>
    /// Real Estate - Management (ID: 73)
    /// </summary>
    [Display(Name = "Real Estate - Management")]
    RealEstateManagement = 73,

    /// <summary>
    /// Real Estate - Sales (ID: 74)
    /// </summary>
    [Display(Name = "Real Estate - Sales")]
    RealEstateSales = 74,

    /// <summary>
    /// Rental and Leasing (ID: 110)
    /// </summary>
    [Display(Name = "Rental and Leasing")]
    RentalAndLeasing = 110,

    /// <summary>
    /// Rental and Leasing - Clothing (ID: 75)
    /// </summary>
    [Display(Name = "Rental and Leasing - Clothing")]
    RentalLeasingClothing = 75,

    /// <summary>
    /// Rental and Leasing - Equipment (ID: 76)
    /// </summary>
    [Display(Name = "Rental and Leasing - Equipment")]
    RentalLeasingEquipment = 76,

    /// <summary>
    /// Rental and Leasing - Household goods (ID: 77)
    /// </summary>
    [Display(Name = "Rental and Leasing - Household goods")]
    RentalLeasingHouseholdGoods = 77,

    /// <summary>
    /// Rental and Leasing - Housing (ID: 78)
    /// </summary>
    [Display(Name = "Rental and Leasing - Housing")]
    RentalLeasingHousing = 78,

    /// <summary>
    /// Rental and Leasing - Real Property (ID: 79)
    /// </summary>
    [Display(Name = "Rental and Leasing - Real Property")]
    RentalLeasingRealProperty = 79,

    /// <summary>
    /// Residential Care Facility (CBRF) (ID: 128)
    /// </summary>
    [Display(Name = "Residential Care Facility (CBRF)")]
    ResidentialCareFacility = 128,

    /// <summary>
    /// Retail (ID: 116)
    /// </summary>
    [Display(Name = "Retail")]
    Retail = 116,

    /// <summary>
    /// Retail - Clothing (ID: 80)
    /// </summary>
    [Display(Name = "Retail - Clothing")]
    RetailClothing = 80,

    /// <summary>
    /// Retail - Department Store (ID: 81)
    /// </summary>
    [Display(Name = "Retail - Department Store")]
    RetailDepartmentStore = 81,

    /// <summary>
    /// Retail - Food Sales (ID: 82)
    /// </summary>
    [Display(Name = "Retail - Food Sales")]
    RetailFoodSales = 82,

    /// <summary>
    /// Retail - Furniture (ID: 83)
    /// </summary>
    [Display(Name = "Retail - Furniture")]
    RetailFurniture = 83,

    /// <summary>
    /// Retail - Hardware (ID: 84)
    /// </summary>
    [Display(Name = "Retail - Hardware")]
    RetailHardware = 84,

    /// <summary>
    /// Retail - Motor Vehicles (ID: 85)
    /// </summary>
    [Display(Name = "Retail - Motor Vehicles")]
    RetailMotorVehicles = 85,

    /// <summary>
    /// Retail - Pharmacy (ID: 86)
    /// </summary>
    [Display(Name = "Retail - Pharmacy")]
    RetailPharmacy = 86,

    /// <summary>
    /// Retail - Specialty (Meat, Jewelry, Tobacco) (ID: 87)
    /// </summary>
    [Display(Name = "Retail - Specialty (Meat, Jewelry, Tobacco)")]
    RetailSpecialty = 87,

    /// <summary>
    /// Sales (ID: 123)
    /// </summary>
    [Display(Name = "Sales")]
    Sales = 123,

    /// <summary>
    /// Services (ID: 124)
    /// </summary>
    [Display(Name = "Services")]
    Services = 124,

    /// <summary>
    /// Services - Drycleaners (ID: 88)
    /// </summary>
    [Display(Name = "Services - Drycleaners")]
    ServicesDrycleaners = 88,

    /// <summary>
    /// Services - Investigators (ID: 89)
    /// </summary>
    [Display(Name = "Services - Investigators")]
    ServicesInvestigators = 89,

    /// <summary>
    /// Services - Landscapers (ID: 90)
    /// </summary>
    [Display(Name = "Services - Landscapers")]
    ServicesLandscapers = 90,

    /// <summary>
    /// Services - Laundromats (ID: 91)
    /// </summary>
    [Display(Name = "Services - Laundromats")]
    ServicesLaundromats = 91,

    /// <summary>
    /// Services - Loggers (ID: 92)
    /// </summary>
    [Display(Name = "Services - Loggers")]
    ServicesLoggers = 92,

    /// <summary>
    /// Services - Messengers (ID: 93)
    /// </summary>
    [Display(Name = "Services - Messengers")]
    ServicesMessengers = 93,

    /// <summary>
    /// Services - Movers (ID: 94)
    /// </summary>
    [Display(Name = "Services - Movers")]
    ServicesMovers = 94,

    /// <summary>
    /// Services - Provider of Home Services (Cleaning, Lawn Mowing, Laundry, Nanny/Babysitter) (ID: 130)
    /// </summary>
    [Display(Name = "Services - Provider of Home Services (Cleaning, Lawn Mowing, Laundry, Nanny/Babysitter)")]
    ServicesProviderOfHomeServices = 130,

    /// <summary>
    /// Services - Security Companies (ID: 95)
    /// </summary>
    [Display(Name = "Services - Security Companies")]
    ServicesSecurityCompanies = 95,

    /// <summary>
    /// Services - Tattoos and Piercing (ID: 96)
    /// </summary>
    [Display(Name = "Services - Tattoos and Piercing")]
    ServicesTattoosAndPiercing = 96,

    /// <summary>
    /// Services - Travel Agents (ID: 97)
    /// </summary>
    [Display(Name = "Services - Travel Agents")]
    ServicesTravelAgents = 97,

    /// <summary>
    /// Services - Tree Service (ID: 98)
    /// </summary>
    [Display(Name = "Services - Tree Service")]
    ServicesTreeService = 98,

    /// <summary>
    /// Services - Undertakers (ID: 99)
    /// </summary>
    [Display(Name = "Services - Undertakers")]
    ServicesUndertakers = 99,

    /// <summary>
    /// Services - Veterinary Clinics (ID: 100)
    /// </summary>
    [Display(Name = "Services - Veterinary Clinics")]
    ServicesVeterinaryClinics = 100,

    /// <summary>
    /// Software Development (ID: 136)
    /// </summary>
    [Display(Name = "Software Development")]
    SoftwareDevelopment = 136,

    /// <summary>
    /// Transportation and Warehousing (ID: 111)
    /// </summary>
    [Display(Name = "Transportation and Warehousing")]
    TransportationAndWarehousing = 111,

    /// <summary>
    /// Transportation and Warehousing - Buses (ID: 101)
    /// </summary>
    [Display(Name = "Transportation and Warehousing - Buses")]
    TransportationWarehousingBuses = 101,

    /// <summary>
    /// Transportation and Warehousing - Storage and Storage Units (ID: 102)
    /// </summary>
    [Display(Name = "Transportation and Warehousing - Storage and Storage Units")]
    TransportationWarehousingStorageAndStorageUnits = 102,

    /// <summary>
    /// Transportation and Warehousing - Taxis (ID: 103)
    /// </summary>
    [Display(Name = "Transportation and Warehousing - Taxis")]
    TransportationWarehousingTaxis = 103,

    /// <summary>
    /// Transportation and Warehousing - Truckers (ID: 104)
    /// </summary>
    [Display(Name = "Transportation and Warehousing - Truckers")]
    TransportationWarehousingTruckers = 104,

    /// <summary>
    /// Transportation and Warehousing - Warehouses (ID: 105)
    /// </summary>
    [Display(Name = "Transportation and Warehousing - Warehouses")]
    TransportationWarehousingWarehouses = 105,

    /// <summary>
    /// Wholesale - Agent/Broker (ID: 107)
    /// </summary>
    [Display(Name = "Wholesale - Agent/Broker")]
    WholesaleAgentBroker = 107,

    /// <summary>
    /// Wholesale - Other (ID: 108)
    /// </summary>
    [Display(Name = "Wholesale - Other")]
    WholesaleOther = 108
}
