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
    AccommodationAndFoodService = 112,

    /// <summary>
    /// Accommodation and Food Service - Food (Carryout-Delivery) (ID: 23)
    /// </summary>
    AccommodationFoodCarryoutDelivery = 23,

    /// <summary>
    /// Accommodation and Food Service - Food and/or Drink Establishments (ID: 22)
    /// </summary>
    AccommodationFoodDrinkEstablishments = 22,

    /// <summary>
    /// Accommodation and Food Service - Lodging (ID: 21)
    /// </summary>
    AccommodationLodging = 21,

    /// <summary>
    /// Agriculture - Raising Crops/Food (ID: 24)
    /// </summary>
    AgricultureRaisingCropsFood = 24,

    /// <summary>
    /// Agriculture - Raising Livestock (ID: 25)
    /// </summary>
    AgricultureRaisingLivestock = 25,

    /// <summary>
    /// Agriculture (Farming) (ID: 117)
    /// </summary>
    AgricultureFarming = 117,

    /// <summary>
    /// Childcare/Daycare Center (ID: 127)
    /// </summary>
    ChildcareDaycareCenter = 127,

    /// <summary>
    /// Construction/Specialty Trade Related (ID: 120)
    /// </summary>
    ConstructionSpecialtyTradeRelated = 120,

    /// <summary>
    /// Construction/Specialty Trade Related - Electronics installations (ID: 39)
    /// </summary>
    ConstructionSpecialtyTradeElectronicsInstallations = 39,

    /// <summary>
    /// Construction/Specialty Trade Related - Flooring(Except Hardwood) (ID: 40)
    /// </summary>
    ConstructionSpecialtyTradeFlooringExceptHardwood = 40,

    /// <summary>
    /// Construction/Specialty Trade Related - Heating and Cooling (ID: 41)
    /// </summary>
    ConstructionSpecialtyTradeHeatingAndCooling = 41,

    /// <summary>
    /// Construction/Specialty Trade Related - Whitewashing (ID: 42)
    /// </summary>
    ConstructionSpecialtyTradeWhitewashing = 42,

    /// <summary>
    /// Construction/Specialty Trades (ID: 109)
    /// </summary>
    ConstructionSpecialtyTrades = 109,

    /// <summary>
    /// Construction/Specialty Trades - Carpentry (ID: 26)
    /// </summary>
    ConstructionSpecialtyTradesCarpentry = 26,

    /// <summary>
    /// Construction/Specialty Trades - Concrete (ID: 27)
    /// </summary>
    ConstructionSpecialtyTradesConcrete = 27,

    /// <summary>
    /// Construction/Specialty Trades - Earth Moving (ID: 28)
    /// </summary>
    ConstructionSpecialtyTradesEarthMoving = 28,

    /// <summary>
    /// Construction/Specialty Trades - Electricians (ID: 29)
    /// </summary>
    ConstructionSpecialtyTradesElectricians = 29,

    /// <summary>
    /// Construction/Specialty Trades - Hardwood flooring (ID: 30)
    /// </summary>
    ConstructionSpecialtyTradesHardwoodFlooring = 30,

    /// <summary>
    /// Construction/Specialty Trades - Iron work (ID: 31)
    /// </summary>
    ConstructionSpecialtyTradesIronWork = 31,

    /// <summary>
    /// Construction/Specialty Trades - Painters (ID: 32)
    /// </summary>
    ConstructionSpecialtyTradesPainters = 32,

    /// <summary>
    /// Construction/Specialty Trades - Plumbers (ID: 33)
    /// </summary>
    ConstructionSpecialtyTradesPlumbers = 33,

    /// <summary>
    /// Construction/Specialty Trades - Remodeling, Repair, Additions (ID: 34)
    /// </summary>
    ConstructionSpecialtyTradesRemodelingRepairAdditions = 34,

    /// <summary>
    /// Construction/Specialty Trades - Road work (ID: 35)
    /// </summary>
    ConstructionSpecialtyTradesRoadWork = 35,

    /// <summary>
    /// Construction/Specialty Trades - Roofing (ID: 36)
    /// </summary>
    ConstructionSpecialtyTradesRoofing = 36,

    /// <summary>
    /// Construction/Specialty Trades - Siding (ID: 37)
    /// </summary>
    ConstructionSpecialtyTradesSiding = 37,

    /// <summary>
    /// Construction/Specialty Trades - Utility Construction (ID: 38)
    /// </summary>
    ConstructionSpecialtyTradesUtilityConstruction = 38,

    /// <summary>
    /// Consultants (ID: 134)
    /// </summary>
    Consultants = 134,

    /// <summary>
    /// Domestic - Employ Nanny or Babysitter in Your Own Home (ID: 45)
    /// </summary>
    DomesticEmployNannyOrBabysitter = 45,

    /// <summary>
    /// Domestic - Fiscal Agent Electing to be Employer (ID: 137)
    /// </summary>
    DomesticFiscalAgentElectingToBeEmployer = 137,

    /// <summary>
    /// Domestic - Recipient of Home Help (Cleaning, Laundry, Lawn Mowing, etc.) (ID: 44)
    /// </summary>
    DomesticRecipientOfHomeHelp = 44,

    /// <summary>
    /// Domestic - Recipient of In-Home Healthcare (ID: 43)
    /// </summary>
    DomesticRecipientOfInHomeHealthcare = 43,

    /// <summary>
    /// Education, Recreation, and Training (ID: 121)
    /// </summary>
    EducationRecreationAndTraining = 121,

    /// <summary>
    /// Education, Recreation, and Training - Dance Studio (ID: 46)
    /// </summary>
    EducationRecreationDanceStudio = 46,

    /// <summary>
    /// Education, Recreation, and Training - Gymnastics (ID: 47)
    /// </summary>
    EducationRecreationGymnastics = 47,

    /// <summary>
    /// Education, Recreation, and Training - Preschool (ID: 48)
    /// </summary>
    EducationRecreationPreschool = 48,

    /// <summary>
    /// Education, Recreation, and Training - Private School (All Public Schools Use Government Agency) (ID: 49)
    /// </summary>
    EducationRecreationPrivateSchool = 49,

    /// <summary>
    /// Education, Recreation, and Training - Sports Facility (ID: 50)
    /// </summary>
    EducationRecreationSportsFacility = 50,

    /// <summary>
    /// Education, Recreation, and Training - Theatres and Related (Actors, Ushers, Technical Staff) (ID: 51)
    /// </summary>
    EducationRecreationTheatres = 51,

    /// <summary>
    /// Education, Recreation, and Training - Tutoring (ID: 52)
    /// </summary>
    EducationRecreationTutoring = 52,

    /// <summary>
    /// Employer Services (ID: 122)
    /// </summary>
    EmployerServices = 122,

    /// <summary>
    /// Employer Services - Employee Leasing Company (ID: 53)
    /// </summary>
    EmployerServicesEmployeeLeasingCompany = 53,

    /// <summary>
    /// Employer Services - Payroll Service (ID: 54)
    /// </summary>
    EmployerServicesPayrollService = 54,

    /// <summary>
    /// Employer Services - Professional Employer Organization (PEO) (ID: 55)
    /// </summary>
    EmployerServicesProfessionalEmployerOrganization = 55,

    /// <summary>
    /// Employer Services - Temporary Help Service (ID: 56)
    /// </summary>
    EmployerServicesTemporaryHelpService = 56,

    /// <summary>
    /// Finance, Insurance, and Legal (ID: 115)
    /// </summary>
    FinanceInsuranceAndLegal = 115,

    /// <summary>
    /// Finance, Insurance, and Legal - Accountants (ID: 57)
    /// </summary>
    FinanceInsuranceLegalAccountants = 57,

    /// <summary>
    /// Finance, Insurance, and Legal - Bank (ID: 58)
    /// </summary>
    FinanceInsuranceLegalBank = 58,

    /// <summary>
    /// Finance, Insurance, and Legal - Check cashing (ID: 59)
    /// </summary>
    FinanceInsuranceLegalCheckCashing = 59,

    /// <summary>
    /// Finance, Insurance, and Legal - Credit Union (ID: 60)
    /// </summary>
    FinanceInsuranceLegalCreditUnion = 60,

    /// <summary>
    /// Finance, Insurance, and Legal - Insurance (ID: 61)
    /// </summary>
    FinanceInsuranceLegalInsurance = 61,

    /// <summary>
    /// Finance, Insurance, and Legal - Investment firm (ID: 62)
    /// </summary>
    FinanceInsuranceLegalInvestmentFirm = 62,

    /// <summary>
    /// Finance, Insurance, and Legal - Lawyers (ID: 63)
    /// </summary>
    FinanceInsuranceLegalLawyers = 63,

    /// <summary>
    /// Finance, Insurance, and Legal - Savings and Loan (ID: 64)
    /// </summary>
    FinanceInsuranceLegalSavingsAndLoan = 64,

    /// <summary>
    /// Finance, Insurance, and Legal - Title Company (ID: 65)
    /// </summary>
    FinanceInsuranceLegalTitleCompany = 65,

    /// <summary>
    /// Government Agency (ID: 119)
    /// </summary>
    GovernmentAgency = 119,

    /// <summary>
    /// Health Care and Social Assistance (ID: 106)
    /// </summary>
    HealthCareAndSocialAssistance = 106,

    /// <summary>
    /// Healthcare and Social Assistance - Chiropractors (ID: 66)
    /// </summary>
    HealthcareChiropractors = 66,

    /// <summary>
    /// Healthcare and Social Assistance - Clinics (ID: 67)
    /// </summary>
    HealthcareClinics = 67,

    /// <summary>
    /// Healthcare and Social Assistance - Counseling (ID: 68)
    /// </summary>
    HealthcareCounseling = 68,

    /// <summary>
    /// Healthcare and Social Assistance - Dental (ID: 69)
    /// </summary>
    HealthcareDental = 69,

    /// <summary>
    /// Healthcare and Social Assistance - Hospital (ID: 70)
    /// </summary>
    HealthcareHospital = 70,

    /// <summary>
    /// Healthcare and Social Assistance - Massage Therapy (ID: 71)
    /// </summary>
    HealthcareMassageTherapy = 71,

    /// <summary>
    /// Healthcare and Social Assistance - Physicians (ID: 72)
    /// </summary>
    HealthcarePhysicians = 72,

    /// <summary>
    /// Healthcare and Social Assistance - Provider of In-Home Healthcare (ID: 129)
    /// </summary>
    HealthcareProviderOfInHomeHealthcare = 129,

    /// <summary>
    /// IT Services/Consulting (ID: 135)
    /// </summary>
    ITServicesConsulting = 135,

    /// <summary>
    /// Manufacturing (ID: 114)
    /// </summary>
    Manufacturing = 114,

    /// <summary>
    /// Other (*Specify) (ID: 118)
    /// </summary>
    Other = 118,

    /// <summary>
    /// Real Estate (ID: 113)
    /// </summary>
    RealEstate = 113,

    /// <summary>
    /// Real Estate - Management (ID: 73)
    /// </summary>
    RealEstateManagement = 73,

    /// <summary>
    /// Real Estate - Sales (ID: 74)
    /// </summary>
    RealEstateSales = 74,

    /// <summary>
    /// Rental and Leasing (ID: 110)
    /// </summary>
    RentalAndLeasing = 110,

    /// <summary>
    /// Rental and Leasing - Clothing (ID: 75)
    /// </summary>
    RentalLeasingClothing = 75,

    /// <summary>
    /// Rental and Leasing - Equipment (ID: 76)
    /// </summary>
    RentalLeasingEquipment = 76,

    /// <summary>
    /// Rental and Leasing - Household goods (ID: 77)
    /// </summary>
    RentalLeasingHouseholdGoods = 77,

    /// <summary>
    /// Rental and Leasing - Housing (ID: 78)
    /// </summary>
    RentalLeasingHousing = 78,

    /// <summary>
    /// Rental and Leasing - Real Property (ID: 79)
    /// </summary>
    RentalLeasingRealProperty = 79,

    /// <summary>
    /// Residential Care Facility (CBRF) (ID: 128)
    /// </summary>
    ResidentialCareFacility = 128,

    /// <summary>
    /// Retail (ID: 116)
    /// </summary>
    Retail = 116,

    /// <summary>
    /// Retail - Clothing (ID: 80)
    /// </summary>
    RetailClothing = 80,

    /// <summary>
    /// Retail - Department Store (ID: 81)
    /// </summary>
    RetailDepartmentStore = 81,

    /// <summary>
    /// Retail - Food Sales (ID: 82)
    /// </summary>
    RetailFoodSales = 82,

    /// <summary>
    /// Retail - Furniture (ID: 83)
    /// </summary>
    RetailFurniture = 83,

    /// <summary>
    /// Retail - Hardware (ID: 84)
    /// </summary>
    RetailHardware = 84,

    /// <summary>
    /// Retail - Motor Vehicles (ID: 85)
    /// </summary>
    RetailMotorVehicles = 85,

    /// <summary>
    /// Retail - Pharmacy (ID: 86)
    /// </summary>
    RetailPharmacy = 86,

    /// <summary>
    /// Retail - Specialty (Meat, Jewelry, Tobacco) (ID: 87)
    /// </summary>
    RetailSpecialty = 87,

    /// <summary>
    /// Sales (ID: 123)
    /// </summary>
    Sales = 123,

    /// <summary>
    /// Services (ID: 124)
    /// </summary>
    Services = 124,

    /// <summary>
    /// Services - Drycleaners (ID: 88)
    /// </summary>
    ServicesDrycleaners = 88,

    /// <summary>
    /// Services - Investigators (ID: 89)
    /// </summary>
    ServicesInvestigators = 89,

    /// <summary>
    /// Services - Landscapers (ID: 90)
    /// </summary>
    ServicesLandscapers = 90,

    /// <summary>
    /// Services - Laundromats (ID: 91)
    /// </summary>
    ServicesLaundromats = 91,

    /// <summary>
    /// Services - Loggers (ID: 92)
    /// </summary>
    ServicesLoggers = 92,

    /// <summary>
    /// Services - Messengers (ID: 93)
    /// </summary>
    ServicesMessengers = 93,

    /// <summary>
    /// Services - Movers (ID: 94)
    /// </summary>
    ServicesMovers = 94,

    /// <summary>
    /// Services - Provider of Home Services (Cleaning, Lawn Mowing, Laundry, Nanny/Babysitter) (ID: 130)
    /// </summary>
    ServicesProviderOfHomeServices = 130,

    /// <summary>
    /// Services - Security Companies (ID: 95)
    /// </summary>
    ServicesSecurityCompanies = 95,

    /// <summary>
    /// Services - Tattoos and Piercing (ID: 96)
    /// </summary>
    ServicesTattoosAndPiercing = 96,

    /// <summary>
    /// Services - Travel Agents (ID: 97)
    /// </summary>
    ServicesTravelAgents = 97,

    /// <summary>
    /// Services - Tree Service (ID: 98)
    /// </summary>
    ServicesTreeService = 98,

    /// <summary>
    /// Services - Undertakers (ID: 99)
    /// </summary>
    ServicesUndertakers = 99,

    /// <summary>
    /// Services - Veterinary Clinics (ID: 100)
    /// </summary>
    ServicesVeterinaryClinics = 100,

    /// <summary>
    /// Software Development (ID: 136)
    /// </summary>
    SoftwareDevelopment = 136,

    /// <summary>
    /// Transportation and Warehousing (ID: 111)
    /// </summary>
    TransportationAndWarehousing = 111,

    /// <summary>
    /// Transportation and Warehousing - Buses (ID: 101)
    /// </summary>
    TransportationWarehousingBuses = 101,

    /// <summary>
    /// Transportation and Warehousing - Storage and Storage Units (ID: 102)
    /// </summary>
    TransportationWarehousingStorageAndStorageUnits = 102,

    /// <summary>
    /// Transportation and Warehousing - Taxis (ID: 103)
    /// </summary>
    TransportationWarehousingTaxis = 103,

    /// <summary>
    /// Transportation and Warehousing - Truckers (ID: 104)
    /// </summary>
    TransportationWarehousingTruckers = 104,

    /// <summary>
    /// Transportation and Warehousing - Warehouses (ID: 105)
    /// </summary>
    TransportationWarehousingWarehouses = 105,

    /// <summary>
    /// Wholesale - Agent/Broker (ID: 107)
    /// </summary>
    WholesaleAgentBroker = 107,

    /// <summary>
    /// Wholesale - Other (ID: 108)
    /// </summary>
    WholesaleOther = 108
}
