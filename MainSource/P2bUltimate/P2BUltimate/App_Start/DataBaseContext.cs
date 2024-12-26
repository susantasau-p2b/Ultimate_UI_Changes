using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Web;
using P2b.Global;
using Payroll;
using Training;
using Attendance;using Leave;
using ReportPayroll;
using System.Configuration;
using Appraisal;
using Recruitment;
using System.Collections;
using P2BUltimate.Models;
using EMS;
using P2B.API.Models;
using P2B.MOBILE;
using IR;
using CMS_SPS;
using P2B.EExMS;
using P2B.PFTRUST;

namespace P2BUltimate.App_Start
{
    public class DataBaseContext : DbContext
    {

        /*Added by Rekha*/
        public DbSet<Company> Company { get; set; }
        public DbSet<CompanyExit> CompanyExit { get; set; }
        public DbSet<Address> Address { get; set; }
        public DbSet<Country> Country { get; set; }
        public DbSet<State> State { get; set; }
        public DbSet<StateRegion> StateRegion { get; set; }
        public DbSet<District> District { get; set; }
        public DbSet<Taluka> Taluka { get; set; }
        public DbSet<City> City { get; set; }
        public DbSet<Area> Area { get; set; }
        public DbSet<Corporate> Corporate { get; set; }
        public DbSet<Lookup> Lookup { get; set; }
        public DbSet<LookupValue> LookupValue { get; set; }
        public DbSet<Employee> Employee { get; set; }
        public DbSet<EmpOff> EmpOff { get; set; }
        public DbSet<ContactDetails> ContactDetails { get; set; }
        public DbSet<ChangePassword> ChangePassword { get; set; }
        public DbSet<ContactNumbers> ContactNumbers { get; set; }
        public DbSet<Wages> Wages { get; set; }
        public DbSet<RateMaster> RateMaster { get; set; }
        public DbSet<PayProcessGroup> PayProcessGroup { get; set; }
        public DbSet<PayrollPeriod> PayrollPeriod { get; set; }
        public DbSet<GeoStruct> GeoStruct { get; set; }
        public DbSet<FuncStruct> FuncStruct { get; set; }
        public DbSet<PayStruct> PayStruct { get; set; }
        public DbSet<ServiceBookDates> ServiceBookDates { get; set; }
        public DbSet<SalHeadFormula> SalHeadFormula { get; set; }
        public DbSet<WagesRange> WagesRange { get; set; }
        public DbSet<ServiceRange> ServiceRange { get; set; }
        public DbSet<DataDump> DataDump { get; set; }

        public DbSet<RetirementDay> RetirementDay { get; set; }
        public DbSet<AmountDependRule> AmountDependRule { get; set; }
        public DbSet<SlabDependRule> SlabDependRule { get; set; }
        public DbSet<NationalityID> NationalityID { get; set; }
        public DbSet<PercentDependRule> PercentDependRule { get; set; }

        public DbSet<LWFMaster> LWFMaster { get; set; }
        public DbSet<ServiceDependRule> ServiceDependRule { get; set; }
        public DbSet<FamilyDetails> FamilyDetails { get; set; }
        public DbSet<LoanAdvancePolicy> LoanAdvancePolicy { get; set; }
        public DbSet<LoanAdvanceHead> LoanAdvanceHead { get; set; }

        public DbSet<SalaryHead> SalaryHead { get; set; }
        public DbSet<ITLoan> ITLoan { get; set; }
        public DbSet<ITStandardITRebate> ITStandardITRebate { get; set; }
        public DbSet<Range> Range { get; set; }
        public DbSet<StatutoryEffectiveMonths> StatutoryEffectiveMonths { get; set; }
        public DbSet<ITSection10SalHeads> ITSection10SalHeads { get; set; }
        public DbSet<ITTDS> ITTDS { get; set; }
        public DbSet<ITSection10> ITSection10 { get; set; }
        public DbSet<ITSubInvestment> ITSubInvestment { get; set; }
        public DbSet<ITInvestment> ITInvestment { get; set; }
        public DbSet<DT_Corporate> DT_Corporate { get; set; }
        public DbSet<DT_Address> DT_Address { get; set; }
        public DbSet<DT_ContactDetails> DT_ContactDetails { get; set; }
        public DbSet<DT_ContactNumbers> DT_ContactNumbers { get; set; }
        public DbSet<DT_Area> DT_Area { get; set; }
        public DbSet<DT_City> DT_City { get; set; }
        public DbSet<DT_Country> DT_Country { get; set; }
        public DbSet<DT_District> DT_District { get; set; }
        public DbSet<DT_State> DT_State { get; set; }
        public DbSet<DT_Taluka> DT_Taluka { get; set; }
        public DbSet<DT_StateRegion> DT_StateRegion { get; set; }
        public DbSet<DT_Lookup> DT_Lookup { get; set; }
        public DbSet<DT_LookupValue> DT_LookupValue { get; set; }
        public DbSet<DT_Region> DT_Region { get; set; }
        public DbSet<DT_EmpOff> DT_EmpOff { get; set; }
        public DbSet<DT_PayrollPeriod> DT_PayrollPeriod { get; set; }
        public DbSet<DT_PayProcessGroup> DT_PayProcessGroup { get; set; }
        public DbSet<DT_Employee> DT_Employee { get; set; }
        public DbSet<DT_RateMaster> DT_RateMaster { get; set; }
        public DbSet<DT_Wages> DT_Wages { get; set; }
        public DbSet<DT_WagesRange> DT_WagesRange { get; set; }
        public DbSet<DT_ServiceRange> DT_ServiceRange { get; set; }
        public DbSet<DT_PayScale> DT_PayScale { get; set; }
        public DbSet<DT_PayScaleConfig> DT_PayScaleConfig { get; set; }
        public DbSet<DT_PayScaleConfigJobStatus> DT_PayScaleConfigJobStatus { get; set; }
        public DbSet<VDADependRule> VDADependRule { get; set; }
        public DbSet<BASICDependRule> BASICDependRule { get; set; }
        public DbSet<DT_Level> DT_Level { get; set; }
        public DbSet<DepartmentObj> DepartmentObj { get; set; }
        public DbSet<DT_EmpSalStruct> DT_EmpSalStruct { get; set; }
        public DbSet<EmpSalStruct> EmpSalStruct { get; set; }
        public DbSet<EmpSalStructDetails> EmpSalStructDetails { get; set; }
        public DbSet<PayScaleAssignment> PayScaleAssignment { get; set; }
        public DbSet<CPIUnitCalc> CPIUnitCalc { get; set; }
        public DbSet<YearlyPaymentT> YearlyPaymentT { get; set; }

        /// new added 04/11/2019
        public DbSet<HRAMonthRent> HRAMonthRent { get; set; }
        public DbSet<HRATransT> HRATransT { get; set; }
        public DbSet<HRAExemptionMaster> HRAExemptionMaster { get; set; }

        public DbSet<Login> Login { get; set; }
        public DbSet<PasswordPolicy> PasswordPolicy { get; set; }
        public DbSet<LogRegister> LogRegister { get; set; }
        public DbSet<AttendanceT> AttendanceT { get; set; }
        public DbSet<OthEarningDeductionT> OthEarningDeductionT { get; set; }
        public DbSet<Allergy> Allergy { get; set; }
        public DbSet<Calendar> Calendar { get; set; }
        public DbSet<Disease> Disease { get; set; }
        public DbSet<Doctor> Doctor { get; set; }
        public DbSet<Medicine> Medicine { get; set; }
        public DbSet<CurrencyConversion> CurrencyConversion { get; set; }
        public DbSet<BasicScale> BasicScale { get; set; }
        public DbSet<BasicScaleDetails> BasicScaleDetails { get; set; }
        public DbSet<EmpMedicalInfo> EmpMedicalInfo { get; set; }
        public DbSet<NameDetails> NameDetails { get; set; }
        public DbSet<NameSingle> NameSingle { get; set; }
        public DbSet<SocialActivities> SocialActivities { get; set; }
        public DbSet<PerkHead> PerkHead { get; set; }
        public DbSet<OtherSalaryHead> OtherSalaryHead { get; set; }
        public DbSet<EmpSocialInfo> EmpSocialInfo { get; set; }
        public DbSet<WorkExpDetails> WorkExpDetails { get; set; }
        public DbSet<FacultySpecialization> FacultySpecialization { get; set; }
        public DbSet<TrainingExpenses> TrainingExpenses { get; set; }
        public DbSet<TrainingInstitute> TrainingInstitute { get; set; }
        public DbSet<Venue> Venue { get; set; }
        public DbSet<Category> Category { get; set; }
        public DbSet<SubCategory> SubCategory { get; set; }
        public DbSet<RemarkConfig> RemarkConfig { get; set; }
        public DbSet<OTPolicy> OTPolicy { get; set; }
        public DbSet<TimingGroup> TimingGroup { get; set; }
        public DbSet<PayScaleAgreement> PayScaleAgreement { get; set; }
        public DbSet<ESICMaster> ESICMaster { get; set; }
        public DbSet<Grade> Grade { get; set; }
        public DbSet<Job> Job { get; set; }
        public DbSet<JobPosition> JobPosition { get; set; }
        public DbSet<JobStatus> JobStatus { get; set; }
        public DbSet<PassportDetails> PassportDetails { get; set; }
        public DbSet<TimingMonthlyRoaster> TimingMonthlyRoaster { get; set; }
        public DbSet<VisaDetails> VisaDetails { get; set; }
        public DbSet<EmergencyContact> EmergencyContact { get; set; }
        public DbSet<Holiday> Holiday { get; set; }
        public DbSet<Level> Level { get; set; }
        public DbSet<NegSalAct> NegSalAct { get; set; }
        public DbSet<PayScale> PayScale { get; set; }
        public DbSet<DT_Allergy> DT_Allergy { get; set; }
        public DbSet<DT_BasicScale> DT_BasicScale { get; set; }
        public DbSet<DT_BasicScaleDetails> DT_BasicScaleDetails { get; set; }
        public DbSet<DT_EmpMedicalInfo> DT_EmpMedicalInfo { get; set; }
        public DbSet<DT_Medicine> DT_Medicine { get; set; }
        public DbSet<DT_Disease> DT_Disease { get; set; }
        public DbSet<DT_Doctor> DT_Doctor { get; set; }
        public DbSet<DT_EmergencyContact> DT_EmergencyContact { get; set; }
        public DbSet<DT_EmpSocialInfo> DT_EmpSocialInfo { get; set; }
        public DbSet<DT_PerkHead> DT_PerkHead { get; set; }
        public DbSet<DT_OtherSalaryHead> DT_OtherSalaryHead { get; set; }
        public DbSet<DT_WorkExpDetails> DT_WorkExpDetails { get; set; }
        public DbSet<DT_FacultySpecialization> DT_FacultySpecialization { get; set; }
        public DbSet<DT_TrainingExpenses> DT_TrainingExpenses { get; set; }
        public DbSet<DT_Category> DT_Category { get; set; }
        public DbSet<DT_SubCategory> DT_SubCategory { get; set; }
        public DbSet<DT_TrainingInstitute> DT_TrainingInstitute { get; set; }
        public DbSet<DT_Venue> DT_Venue { get; set; }
        public DbSet<DT_RemarkConfig> DT_RemarkConfig { get; set; }
        public DbSet<DT_OTPolicy> DT_OTPolicy { get; set; }
        public DbSet<DT_PayScaleAgreement> DT_PayScaleAgreement { get; set; }
        public DbSet<DT_Holiday> DT_Holiday { get; set; }
        public DbSet<DT_Calendar> DT_Calendar { get; set; }
        public DbSet<DT_CurrencyConversion> DT_CurrencyConversion { get; set; }
        public DbSet<DT_Range> DT_Range { get; set; }
        public DbSet<DT_ESICMaster> DT_ESICMaster { get; set; }
        public DbSet<DT_Grade> DT_Grade { get; set; }
        public DbSet<DT_Job> DT_Job { get; set; }
        public DbSet<DT_JobPosition> DT_JobPosition { get; set; }
        public DbSet<DT_JobStatus> DT_JobStatus { get; set; }
        public DbSet<DT_PassportDetails> DT_PassportDetails { get; set; }
        public DbSet<DT_TimingMonthlyRoaster> DT_TimingMonthlyRoaster { get; set; }
        public DbSet<DT_VisaDetails> DT_VisaDetails { get; set; }
        public DbSet<DT_SuspensionSalPolicy> DT_SuspensionSalPolicy { get; set; }

        // public DbSet<DT_CandidateDocuments> DT_CandidateDocuments { get; set; }
        public DbSet<Division> Division { get; set; }
        public DbSet<Region> Region { get; set; }
        public DbSet<Location> Location { get; set; }
        public DbSet<LocationObj> LocationObj { get; set; }
        public DbSet<Department> Department { get; set; }
        public DbSet<Group> Group { get; set; }
        public DbSet<Unit> Unit { get; set; }
        public DbSet<NomineeBenefit> NomineeBenefit { get; set; }
        public DbSet<BenefitNominees> BenefitNominees { get; set; }
        public DbSet<ForeignTrips> ForeignTrips { get; set; }
        public DbSet<InsuranceDetails> InsuranceDetails { get; set; }
        public DbSet<Bank> Bank { get; set; }
        public DbSet<Branch> Branch { get; set; }
        public DbSet<PrevCompExp> PrevCompExp { get; set; }
        public DbSet<GuarantorDetails> GuarantorDetails { get; set; }
        public DbSet<PromoPolicy> PromoPolicy { get; set; }
        public DbSet<PromoActivity> PromoActivity { get; set; }
        //public DbSet<FacultyExternal> FacultyExternal { get; set; }
        //public DbSet<FacultyInternal> FacultyInternal { get; set; }
        public DbSet<TrainingSession> TrainingSession { get; set; }
        public DbSet<TrainingEvaluation> TrainingEvaluation { get; set; }
        public DbSet<TimingPolicy> TimingPolicy { get; set; }
        public DbSet<DT_Company> DT_Company { get; set; }
        public DbSet<DT_Division> DT_Division { get; set; }
        public DbSet<DT_Location> DT_Location { get; set; }
        public DbSet<DT_Department> DT_Department { get; set; }
        public DbSet<DT_Group> DT_Group { get; set; }
        public DbSet<DT_Unit> DT_Unit { get; set; }
        public DbSet<DT_NomineeBenefit> DT_NomineeBenefit { get; set; }
        public DbSet<DT_BenefitNominees> DT_BenefitNominees { get; set; }
        public DbSet<DT_ForeignTrips> DT_ForeignTrips { get; set; }
        public DbSet<DT_Insurance> DT_Insurance { get; set; }
        public DbSet<DT_InsuranceProduct> DT_InsuranceProduct { get; set; }
        public DbSet<DT_Bank> DT_Bank { get; set; }
        public DbSet<DT_Branch> DT_Branch { get; set; }
        public DbSet<DT_PrevCompExp> DT_PrevCompExp { get; set; }
        public DbSet<DT_GuarantorDetails> DT_GuarantorDetails { get; set; }
        public DbSet<DT_PromoPolicy> DT_PromoPolicy { get; set; }
        public DbSet<DT_PromoActivity> DT_PromoActivity { get; set; }
        public DbSet<DT_LoanAdvanceHead> DT_LoanAdvanceHead { get; set; }
        public DbSet<DT_SalaryHead> DT_SalaryHead { get; set; }
        public DbSet<DT_FacultyExternal> DT_FacultyExternal { get; set; }
        public DbSet<DT_FacultyInternal> DT_FacultyInternal { get; set; }
        public DbSet<DT_TrainingSession> DT_TrainingSession { get; set; }
        public DbSet<DT_TrainingEvaluation> DT_TrainingEvaluation { get; set; }
        public DbSet<DT_TimingPolicy> DT_TimingPolicy { get; set; }
        public DbSet<IncrActivity> IncrActivity { get; set; }
        public DbSet<IncomeTax> IncomeTax { get; set; }
        public DbSet<StagIncrPolicy> StagIncrPolicy { get; set; }
        public DbSet<IncrPolicy> IncrPolicy { get; set; }
        public DbSet<RegIncrPolicy> RegIncrPolicy { get; set; }
        public DbSet<NonRegIncrPolicy> NonRegIncrPolicy { get; set; }
        public DbSet<OthServiceBookActivity> OthServiceBookActivity { get; set; }
        public DbSet<OthServiceBookPolicy> OthServiceBookPolicy { get; set; }
        public DbSet<EmpAcademicInfo> EmpAcademicInfo { get; set; }
        public DbSet<Hobby> Hobby { get; set; }
        public DbSet<Skill> Skill { get; set; }
        public DbSet<LanguageSkill> LanguageSkill { get; set; }
        public DbSet<Language> Language { get; set; }
        public DbSet<Qualification> Qualification { get; set; }
        public DbSet<QualificationDetails> QualificationDetails { get; set; }
        public DbSet<Scolarship> Scolarship { get; set; }
        public DbSet<Awards> Awards { get; set; }
        public DbSet<ProgramList> ProgramList { get; set; }
        public DbSet<Budget> Budget { get; set; }
        public DbSet<BudgetParameters> BudgetParameters { get; set; }
        //public DbSet<OrgTraining> OrgTraining { get; set; }
        public DbSet<PTaxMaster> PTaxMaster { get; set; }
        public DbSet<PFMaster> PFMaster { get; set; }
        public DbSet<SalEarnDedT> SalEarnDedT { get; set; }
        public DbSet<PromotionServiceBook> PromotionServiceBook { get; set; }
        public DbSet<ServiceSecurity> ServiceSecurity { get; set; }
        public DbSet<Insurance> Insurance { get; set; }
        public DbSet<TimingPolicyBatchAssignment> TimingPolicyBatchAssignment { get; set; }
        public DbSet<ExtnRednServiceBook> ExtnRednServiceBook { get; set; }
        public DbSet<ExtnRednActivity> ExtnRednActivity { get; set; }
        public DbSet<ExtnRednPolicy> ExtnRednPolicy { get; set; }
        public DbSet<DT_ExtnRednPolicy> DT_ExtnRednPolicy { get; set; }
        //CompanyPayroll
        public DbSet<InsuranceProduct> InsuranceProduct { get; set; }
        public DbSet<IncrPromoPolicyDetails> IncrPromoPolicyDetails { get; set; }
        public DbSet<SalaryT> SalaryT { get; set; }
        public DbSet<CPIEntryT> CPIEntryT { get; set; }
        public DbSet<IncrPolicyDetails> IncrPolicyDetails { get; set; }
        public DbSet<IncrDataCalc> IncrDataCalc { get; set; }
        public DbSet<EmpPrevSalStructDetails> EmpPrevSalStructDetails { get; set; }
        public DbSet<EmpPrevSalStruct> EmpPrevSalStruct { get; set; }
        public DbSet<IncrementServiceBook> IncrementServiceBook { get; set; }
        public DbSet<ESICTransT> ESICTransT { get; set; }
        public DbSet<LWFTransT> LWFTransT { get; set; }
        public DbSet<PFTransT> PFTransT { get; set; }
        public DbSet<PTaxTransT> PTaxTransT { get; set; }
        public DbSet<FunctAttendanceT> FunctAttendanceT { get; set; }
        public DbSet<AnnualSalaryR> AnnualSalaryR { get; set; }
        public DbSet<GratuityT> GratuityT { get; set; }
        public DbSet<InsuranceDetailsT> InsuranceDetailsT { get; set; }
        public DbSet<ITaxTransT> ITaxTransT { get; set; }
        public DbSet<ITProjection> ITProjection { get; set; }
        public DbSet<ITSubInvestmentPayment> ITSubInvestmentPayment { get; set; }
        public DbSet<LoanAdvRequest> LoanAdvRequest { get; set; }
        public DbSet<PaySlipR> PaySlipR { get; set; }
        public DbSet<PFECRR> PFECRR { get; set; }
        public DbSet<SalaryArrearT> SalaryArrearT { get; set; }
        public DbSet<SalAttendanceT> SalAttendanceT { get; set; }
        public DbSet<CPIRule> CPIRule { get; set; }
        public DbSet<DT_CPIRule> DT_CPIRule { get; set; }
        public DbSet<CPIRuleDetails> CPIRuleDetails { get; set; }
        public DbSet<TransferServiceBook> TransferServiceBook { get; set; }
        public DbSet<EmployeePayroll> EmployeePayroll { get; set; }
        public DbSet<TransActivity> TransActivity { get; set; }
        public DbSet<Form16AllowExemMap> Form16AllowExemMap { get; set; }
        public DbSet<OtherServiceBook> OtherServiceBook { get; set; }
        public DbSet<LoanAdvRepaymentT> LoanAdvRepaymentT { get; set; }
        public DbSet<CompanyPayroll> CompanyPayroll { get; set; }
        public DbSet<DT_LocationObj> DT_LocationObj { get; set; }
        public DbSet<DT_DepartmentObj> DT_DepartmentObj { get; set; }
        public DbSet<DT_IncrActivity> DT_IncrActivity { get; set; }
        public DbSet<DT_PFMaster> DT_PFMaster { get; set; }
        public DbSet<DT_PTaxMaster> DT_PTaxMaster { get; set; }
        public DbSet<DT_RegIncrPolicy> DT_RegIncrPolicy { get; set; }
        public DbSet<DT_OthServiceBookPolicy> DT_OthServiceBookPolicy { get; set; }
        public DbSet<DT_OthServiceBookActivity> DT_OthServiceBookActivity { get; set; }
        public DbSet<LvEncashReq> LvEncashReq { get; set; }
        public DbSet<DT_StagIncrPolicy> DT_StagIncrPolicy { get; set; }
        public DbSet<DT_NonRegIncrPolicy> DT_NonRegIncrPolicy { get; set; }
        public DbSet<DT_IncrPolicy> DT_IncrPolicy { get; set; }
        public DbSet<LvHead> LvHead { get; set; }
        public DbSet<LvHeadFormula> LvHeadFormula { get; set; }
        public DbSet<DT_LvEncashReq> DT_LvEncashReq { get; set; }
        public DbSet<DT_LvCreditPolicy> DT_LvCreditPolicy { get; set; }
        public DbSet<DT_LvCancelReq> DT_LvCancelReq { get; set; }
        public DbSet<LvWFDetails> LvWFDetails { get; set; }
        public DbSet<LvOpenBal> LvOpenBal { get; set; }
        public DbSet<LvNewReq> LvNewReq { get; set; }
        public DbSet<LvEncashPolicy> LvEncashPolicy { get; set; }
        public DbSet<LvDebitPolicy> LvDebitPolicy { get; set; }
        public DbSet<LvCreditPolicy> LvCreditPolicy { get; set; }
        public DbSet<LvCancelReq> LvCancelReq { get; set; }
        public DbSet<LvBank> LvBank { get; set; }
        public DbSet<LvBankPolicy> LvBankPolicy { get; set; }
        public DbSet<LvBankOpenBal> LvBankOpenBal { get; set; }
        public DbSet<LvAssignment> LvAssignment { get; set; }
        public DbSet<EmployeeLvStructDetails> EmployeeLvStructDetails { get; set; }
        public DbSet<EmployeeLvStruct> EmployeeLvStruct { get; set; }
        public DbSet<DT_LvOpenBal> DT_LvOpenBal { get; set; }
        public DbSet<DT_LvHead> DT_LvHead { get; set; }
        public DbSet<DT_LvEncashPolicy> DT_LvEncashPolicy { get; set; }
        public DbSet<DT_LvAssignment> DT_LvAssignment { get; set; }
        public DbSet<DT_LvNewReq> DT_LvNewReq { get; set; }
        public DbSet<DT_LvBankPolicy> DT_LvBankPolicy { get; set; }
        public DbSet<DT_LvBankOpenBal> DT_LvBankOpenBal { get; set; }
        public DbSet<EmployeeLeave> EmployeeLeave { get; set; }
        public DbSet<TransPolicy> TransPolicy { get; set; }
        public DbSet<ITSection> ITSection { get; set; }
        public DbSet<BonusAct> BonusAct { get; set; }
        public DbSet<SuspensionSalPolicy> SuspensionSalPolicy { get; set; }
        public DbSet<GratuityAct> GratuityAct { get; set; }
        public DbSet<DT_TransPolicy> DT_TransPolicy { get; set; }
        public DbSet<DT_CPIEntryT> DT_CPIEntryT { get; set; }
        public DbSet<PFECRSummaryR> PFECRSummaryR { get; set; }
        public DbSet<AnnualSalaryDetailsR> AnnualSalaryDetailsR { get; set; }
        public DbSet<PaySlipDetailDedR> PaySlipDetailDedR { get; set; }
        public DbSet<PaySlipDetailEarnR> PaySlipDetailEarnR { get; set; }
        public DbSet<PaySlipDetailLeaveR> PaySlipDetailLeaveR { get; set; }
        public DbSet<HolidayCalendar> HolidayCalendar { get; set; }
        public DbSet<HolidayList> HolidayList { get; set; }
        public DbSet<WeeklyOffCalendar> WeeklyOffCalendar { get; set; }
        public DbSet<WeeklyOffList> WeeklyOffList { get; set; }
        public DbSet<CompanyLeave> CompanyLeave { get; set; }
        public DbSet<OtherEarningT> OtherEarningT { get; set; }
        public DbSet<GenericField100> GenericField100 { get; set; }
        public DbSet<DT_ITInvestmentPayment> DT_ITInvestmentPayment { get; set; }
        public DbSet<ITInvestmentPayment> ITInvestmentPayment { get; set; }
        public DbSet<DT_ITInvestment> DT_ITInvestment { get; set; }
        public DbSet<ITSection24Payment> ITSection24Payment { get; set; }
        public DbSet<DT_ITStandardITRebate> DT_ITStandardITRebate { get; set; }
        public DbSet<DT_ITSection> DT_ITSection { get; set; }
        public DbSet<DT_ITSection10> DT_ITSection10 { get; set; }
        public DbSet<ITSalaryHeadData> ITSalaryHeadData { get; set; }
        public DbSet<ITSection10Payment> ITSection10Payment { get; set; }
        public DbSet<ITReliefPayment> ITReliefPayment { get; set; }
        public DbSet<LvEncashPayment> LvEncashPayment { get; set; }
        public DbSet<SalaryArrearPFT> SalaryArrearPFT { get; set; }
        public DbSet<SalaryArrearPaymentT> SalaryArrearPaymentT { get; set; }
        public DbSet<BonusChkT> BonusChkT { get; set; }
        public DbSet<JVProcessDataSummary> JVProcessDataSummary { get; set; }
        public DbSet<ArrJVProcessData> ArrJVProcessData { get; set; }
        public DbSet<JVNonStandardEmp> JVNonStandardEmp { get; set; }
        public DbSet<JVProcessData> JVProcessData { get; set; }
        public DbSet<JVParameter> JVParameter { get; set; }
        public DbSet<ArrJVParameter> ArrJVParameter { get; set; }
        public DbSet<ArrJVProcessDataSummary> ArrJVProcessDataSummary { get; set; }
        public DbSet<ArrJVNonStandardEmp> ArrJVNonStandardEmp { get; set; }
        public DbSet<LoginDetails> LoginDetails { get; set; }
        public DbSet<ReportingStructRights> ReportingStructRights { get; set; }
        public DbSet<ReportingStruct> ReportingStruct { get; set; }
        public DbSet<AccessRights> AccessRights { get; set; }
        public DbSet<TimingWeeklySchedule> TimingWeeklySchedule { get; set; }
        public DbSet<EmpTimingMonthlyRoaster> EmpTimingMonthlyRoaster { get; set; }
        public DbSet<DT_TimingWeeklySchedule> DT_TimingWeeklySchedule { get; set; }
        public DbSet<DT_EmpTimingMonthlyRoaster> DT_EmpTimingMonthlyRoaster { get; set; }
        public DbSet<TrainingDetails> TrainingDetails { get; set; }
        public DbSet<TrainingSchedule> TrainingSchedule { get; set; }
        public DbSet<DT_ProgramList> DT_ProgramList { get; set; }
        public DbSet<DT_TrainingDetails> DT_TrainingDetails { get; set; }
        public DbSet<DT_OrgTraining> DT_OrgTraining { get; set; }
        public DbSet<DT_TrainingSchedule> DT_TrainingSchedule { get; set; }
        public DbSet<DT_EmpAcademicInfo> DT_EmpAcademicInfo { get; set; }
        public DbSet<DT_QualificationDetails> DT_QualificationDetails { get; set; }
        public DbSet<DT_Language> DT_Language { get; set; }
        public DbSet<DT_Qualification> DT_Qualification { get; set; }
        public DbSet<EmployeeAttendance> EmployeeAttendance { get; set; }
        public DbSet<ExcelMapping> ExcelMapping { get; set; }
        public DbSet<BasicLinkedDA> BasicLinkedDA { get; set; }
        public DbSet<DT_BasicLinkedDA> DT_BasicLinkedDA { get; set; }
        public DbSet<DT_TimingGroup> DT_TimingGroup { get; set; }
        public DbSet<TrainingPeriod> TrainingPeriod { get; set; }
        public DbSet<TrainingMaster> TrainingMaster { get; set; }
        public DbSet<ITForm16SigningPerson> ITForm16SigningPerson { get; set; }
        public DbSet<DT_ITForm16SigningPerson> DT_ITForm16SigningPerson { get; set; }

        public DbSet<AppCategory> AppCategory { get; set; }
        public DbSet<AppSubCategory> AppSubCategory { get; set; }
        public DbSet<AppEvalMethod> AppEvalMethod { get; set; }
        public DbSet<AppRatingObjective> AppRatingObjective { get; set; }
        public DbSet<AppAssignment> AppAssignment { get; set; }
        public DbSet<EmpAppEvaluation> EmpAppEvaluation { get; set; }
        public DbSet<EmpAppRatingConclusion> EmpAppRatingConclusion { get; set; }
        public DbSet<AppraisalPublish> AppraisalPublish { get; set; }
        public DbSet<EmployeeAppraisal> EmployeeAppraisal { get; set; }
        public DbSet<AppManualAssignment> AppManualAssignment { get; set; }
        public DbSet<EmpAppRating> EmpAppRating { get; set; }
        public DbSet<RecruitInitiator> RecruitInitiator { get; set; }
        public DbSet<RecruitExpenses> RecruitExpenses { get; set; }

        public DbSet<DT_ResumeCollection> DT_ResumeCollection { get; set; }
        public DbSet<DT_AppSubCategory> DT_AppSubCategory { get; set; }
        public DbSet<DT_AppCategory> DT_AppCategory { get; set; }
        public DbSet<DT_AppEvalMethod> DT_AppEvalMethod { get; set; }
        public DbSet<DT_AppRatingObjective> DT_AppRatingObjective { get; set; }
        public DbSet<DT_AppAssignment> DT_AppAssignment { get; set; }
        public DbSet<DT_EmpAppEvaluation> DT_EmpAppEvaluation { get; set; }
        public DbSet<DT_AppraisalPublish> DT_AppraisalPublish { get; set; }
        public DbSet<DT_AppManualAssignment> DT_AppManualAssignment { get; set; }
        public DbSet<DT_RecruitInitiator> DT_RecruitInitiator { get; set; }
        public DbSet<DT_Awards> DT_Awards { get; set; }
        public DbSet<DT_Hobby> DT_Hobby { get; set; }
        public DbSet<DT_LanguageSkill> DT_LanguageSkill { get; set; }
        public DbSet<DT_Scolarship> DT_Scolarship { get; set; }
        public DbSet<DT_Skill> DT_Skill { get; set; }
        public DbSet<DT_RecruitJoinParaProcessResult> DT_RecruitJoinParaProcessResult { get; set; }
        //Appraisal

        public DbSet<RecruitBatchInitiator> RecruitBatchInitiator { get; set; }
        public DbSet<DT_RecruitBatchInitiator> DT_RecruitBatchInitiator { get; set; }
        public DbSet<JobAgency> JobAgency { get; set; }
        public DbSet<JobInsideOrg> JobInsideOrg { get; set; }
        public DbSet<JobPortal> JobPortal { get; set; }
        public DbSet<JobNewsPaper> JobNewsPaper { get; set; }
        public DbSet<JobSource> JobSource { get; set; }
        public DbSet<RecruitEvaluationPara> RecruitEvaluationPara { get; set; }
        public DbSet<SelectionPanel> SelectionPanel { get; set; }
        public DbSet<RecruitEvaluationProcess> RecruitEvaluationProcess { get; set; }
        public DbSet<RecruitJoiningPara> RecruitJoiningPara { get; set; }
        public DbSet<CategoryPost> CategoryPost { get; set; }
        public DbSet<CategorySplPost> CategorySplPost { get; set; }
        public DbSet<DT_PostDetails> DT_PostDetails { get; set; }
        public DbSet<PostDetails> PostDetails { get; set; }


        public DbSet<CTCDefinition> CTCDefinition { get; set; }
        public DbSet<ManPowerBudget> ManPowerBudget { get; set; }
        public DbSet<ManPowerDetailsBatch> ManPowerDetailsBatch { get; set; }
        public DbSet<ManPowerPostData> ManPowerPostData { get; set; }
        public DbSet<ManpowerRequestPost> ManpowerRequestPost { get; set; }
        //public DbSet<DT_ManpowerRequestPost> DT_ManpowerRequestPost { get; set; }
        //public DbSet<ManPowerProvision> ManPowerProvision { get; set; }
        public DbSet<Candidate> Candidate { get; set; }
        public DbSet<EmpCTCStruct> EmpCTCStruct { get; set; }
        public DbSet<ResumeCollection> ResumeCollection { get; set; }
        public DbSet<ShortlistingCriteria> ShortlistingCriteria { get; set; }
        public DbSet<RecruitJoinParaProcessResult> RecruitJoinParaProcessResult { get; set; }
        public DbSet<RecruitEvaluationProcessResult> RecruitEvaluationProcessResult { get; set; }
        public DbSet<CandidateDocuments> CandidateDocuments { get; set; }
        public DbSet<PerkTransT> PerkTransT { get; set; }
        public DbSet<ITChallan> ITChallan { get; set; }
        public DbSet<ITForm16Quarter> ITForm16Quarter { get; set; }
        public DbSet<ITForm16QuarterEmpDetails> ITForm16QuarterEmpDetails { get; set; }
        public DbSet<ITChallanEmpDetails> ITChallanEmpDetails { get; set; }
        public DbSet<ITForm16Data> ITForm16Data { get; set; }
        public DbSet<ITForm16DataDetails> ITForm16DataDetails { get; set; }
        public DbSet<CompanyAttendance> CompanyAttendance { get; set; }
        public DbSet<ITForm12BACaptionMapping> ITForm12BACaptionMapping { get; set; }
        public DbSet<ITForm12BADataDetails> ITForm12BADataDetails { get; set; }
        public DbSet<ITForm24QFileFormatDefinition> ITForm24QFileFormatDefinition { get; set; }
        public DbSet<EmployeeTraining> EmployeeTraining { get; set; }
        public DbSet<ITForm24QData> ITForm24QData { get; set; }
        public DbSet<AppSubCategoryRating> AppSubCategoryRating { get; set; }
        public DbSet<AppCategoryRating> AppCategoryRating { get; set; }
        public DbSet<DT_AppSubCategoryRating> DT_AppSubCategoryRating { get; set; }
        public DbSet<DT_AppCategoryRating> DT_AppCategoryRating { get; set; }
        public DbSet<CompanyAppraisal> CompanyAppraisal { get; set; }
        public DbSet<P2BUltimate.Models.EmpDocument> EmpDocument { get; set; }
        public DbSet<DT_FamilyDetails> DT_FamilyDetails { get; set; }
        public DbSet<ReportingTimingStruct> ReportingTimingStruct { get; set; }
        
        public DbSet<EmpReportingTimingStruct> EmpReportingTimingStruct { get; set; }
        public DbSet<EmpTimingRoasterData> EmpTimingRoasterData { get; set; }
        public DbSet<MachineInterface> MachineInterface { get; set; }
        public DbSet<OrgTimingPolicyBatchAssignment> OrgTimingPolicyBatchAssignment { get; set; }
        public DbSet<ProcessedData> ProcessedData { get; set; }
        public DbSet<RawData> RawData { get; set; }
        public DbSet<RawDataFailure> RawDataFailure { get; set; }
        public DbSet<FacultyInternalExternal> FacultyInternalExternal { get; set; }
        public DbSet<AppraisalSchedule> AppraisalSchedule { get; set; }
        public DbSet<EmployeeDocuments> EmployeeDocuments { get; set; }
        public DbSet<OutDoorDutyReq> OutDoorDutyReq { get; set; }
        public DbSet<DT_SocialActivities> DT_SocialActivities { get; set; }
        public DbSet<ITForm24QDataDetails> ITForm24QDataDetails { get; set; }
        public DbSet<CompanyTraining> CompanyTraining { get; set; }
        public DbSet<CompanyRecruitment> CompanyRecruitment { get; set; }
        public DbSet<RecruitYearlyCalendar> RecruitYearlyCalendar { get; set; }
        public DbSet<NegSalData> NegSalData { get; set; }
        public DbSet<SalaryReconcilation> SalaryReconcilation { get; set; }
      //  public DbSet<DT_ShortlistingCriteria> DT_ShortlistingCriteria { get; set; }

        //Training
        public DbSet<TrainingProgramCalendar> TrainingProgramCalendar { get; set; }
        public DbSet<DT_TrainingProgramCalendar> DT_TrainingProgramCalendar { get; set; }
        public DbSet<DT_BudgetParameters> DT_BudgetParameters { get; set; }
        public DbSet<DT_EmpAssigPara> DT_EmpAssigPara { get; set; }
        public DbSet<EmpAssigPara> EmpAssigPara { get; set; }
        public DbSet<DT_TrainingMaterial> DT_TrainingMaterial { get; set; }
        public DbSet<TrainingMaterial> TrainingMaterial { get; set; }
        public DbSet<DT_YearlyProgramAssignment> DT_YearlyProgramAssignment { get; set; }
        public DbSet<YearlyProgramAssignment> YearlyProgramAssignment { get; set; }
        public DbSet<DT_YearlyTrainingCalendar> DT_YearlyTrainingCalendar { get; set; }
        public DbSet<YearlyTrainingCalendar> YearlyTrainingCalendar { get; set; }
        public DbSet<DT_EmpTrainingNeed> DT_EmpTrainingNeed { get; set; }
        public DbSet<EmpTrainingNeed> EmpTrainingNeed { get; set; }

        public DbSet<TrainingEmployeeSource> TrainingEmployeeSource { get; set; }
        public DbSet<DT_TrainingEmployeeSource> DT_TrainingEmployeeSource { get; set; }
        public DbSet<TrainigDetailSessionInfo> TrainigDetailSessionInfo { get; set; }
        public DbSet<DT_TrainigDetailSessionInfo> DT_TrainigDetailSessionInfo { get; set; }

        /// <email>
        public DbSet<Email> Email { get; set; }
        public DbSet<EmailFieldAssign> EmailFieldAssign { get; set; }
        public DbSet<EmailSendData> EmailSendData { get; set; }
        public DbSet<EmailAddress> EmailAddress { get; set; }
        public DbSet<EmailAttachment> EmailAttachment { get; set; }
        public DbSet<EmailServer> EmailServer { get; set; }
        public DbSet<SMS> SMS { get; set; }
        public DbSet<SMSServer> SMSServer { get; set; }
        public DbSet<SMSAddress> SMSAddress { get; set; }
        public DbSet<SMSSendData> SMSSendData { get; set; }
        public DbSet<IncrementHoldReleaseDetails> IncrementHoldReleaseDetails { get; set; }
        public DbSet<SalaryHoldDetails> SalaryHoldDetails { get; set; }

        public DbSet<PolicyAssignment> PolicyAssignment { get; set; }
        public DbSet<EmployeePolicyStruct> EmployeePolicyStruct { get; set; }
        public DbSet<EmployeePolicyStructDetails> EmployeePolicyStructDetails { get; set; }
        public DbSet<PolicyFormula> PolicyFormula { get; set; }

        public DbSet<CombinedLvHead> CombinedLvHead { get; set; }

        //LFC module
        public DbSet<GlobalLTCBlock> GlobalLTCBlock { get; set; }
        public DbSet<EmpLTCBlock> EmpLTCBlock { get; set; }
        public DbSet<EmpLTCBlockT> EmpLTCBlockT { get; set; }
        public DbSet<LTCAdvanceClaim> LTCAdvanceClaim { get; set; }
        public DbSet<LTCSettlementClaim> LTCSettlementClaim { get; set; }
        public DbSet<TravelEligibilityPolicy> TravelEligibilityPolicy { get; set; }
        public DbSet<HotelEligibilityPolicy> HotelEligibilityPolicy { get; set; }
        public DbSet<TravelHotelBooking> TravelHotelBooking { get; set; }
        public DbSet<TravelModeEligibilityPolicy> TravelModeEligibilityPolicy { get; set; }
        public DbSet<TravelModeRateCeilingPolicy> TravelModeRateCeilingPolicy { get; set; }
        public DbSet<JourneyObject> JourneyObject { get; set; }
        public DbSet<JourneyDetails> JourneyDetails { get; set; }
        public DbSet<HotelBookingRequest> HotelBookingRequest { get; set; }
        public DbSet<TicketBookingRequest> TicketBookingRequest { get; set; }
        public DbSet<VehicleBookingRequest> VehicleBookingRequest { get; set; }
        public DbSet<TADAAdvanceClaim> TADAAdvanceClaim { get; set; }
        public DbSet<TADASettlementClaim> TADASettlementClaim { get; set; }
        
        //Exit management system
        public DbSet<SeperationMaster> SeperationMaster { get; set; }
        public DbSet<SeperationPolicyAssignment> SeperationPolicyAssignment { get; set; }
        public DbSet<SeperationPolicyFormula> SeperationPolicyFormula { get; set; }
        public DbSet<EmployeeSeperationStruct> EmployeeSeperationStruct { get; set; }
        public DbSet<EmployeeSeperationStructDetails> EmployeeSeperationStructDetails { get; set; }
        public DbSet<ExitInterviewRequest> ExitInterviewRequest { get; set; }
        public DbSet<ExitProcess_CheckList_Object> ExitProcess_CheckList_Object { get; set; }
        public DbSet<ExitProcess_CheckList_Policy> ExitProcess_CheckList_Policy { get; set; }
        public DbSet<ExitProcess_Process_Policy> ExitProcess_Process_Policy { get; set; }
        public DbSet<ExitProcess_Config_Policy> ExitProcess_Config_Policy { get; set; }
        public DbSet<NoticePeriod_Object> NoticePeriod_Object { get; set; }
        public DbSet<ResignationRequest> ResignationRequest { get; set; }
        public DbSet<EmployeeExit> EmployeeExit { get; set; } 

        public DbSet<StagIncrDataCalc> StagIncrDataCalc { get; set; }
       
       

        public DbSet<LTCPolicyAssignment> LTCPolicyAssignment { get; set; }
        public DbSet<EmployeeLTCStruct> EmployeeLTCStruct { get; set; }
        public DbSet<EmployeeLTCStructDetails> EmployeeLTCStructDetails { get; set; }
        public DbSet<LTCFormula> LTCFormula { get; set; }
        public DbSet<CheckProcessStatus> CheckProcessStatus { get; set; }
        public DbSet<EmailField> EmailField { get; set; }
        public DbSet<DT_EmailField> DT_EmailField { get; set; }
        public DbSet<DT_Email> DT_Email { get; set; }
        public DbSet<EmailTemplateAssign> EmailTemplateAssign { get; set; }
        public DbSet<Android_Application> Android_Application { get; set; }
        public DbSet<UnitIdAssignment> UnitIdAssignment { get; set; }
        public DbSet<GeoFencing> GeoFencing { get; set; }
        public DbSet<AttendancePayrollPolicy> AttendancePayrollPolicy { get; set; }
        public DbSet<DT_TimingPolicyBatchAssignment> DT_TimingPolicyBatchAssignment { get; set; }
        public DbSet<LeaveDependPolicy> LeaveDependPolicy { get; set; }
        public DbSet<DT_ITForm24QFileFormatDefinition> DT_ITForm24QFileFormatDefinition { get; set; }
        public DbSet<FutureOD> FutureOD { get; set; }

        public DbSet<LvSharingPolicy> LvSharingPolicy { get; set; }
        public DbSet<ErrorLookup> ErrorLookup { get; set; }
        public DbSet<PrefixSuffixAction> PrefixSuffixAction { get; set; }
        public DbSet<P2b.Global.Mobile_Register> mobile_Registers { get; set; }
        public DbSet<APIUsers> aPIUsers { get; set; }
        public DbSet<OTPHistory> oTPHistories { get; set; }
        // IR Module
        public DbSet<OffenseObject> OffenseObject { get; set; }
        public DbSet<MisconductComplaint> MisconductComplaint { get; set; }
        public DbSet<EnquiryPanel> EnquiryPanel { get; set; }
        public DbSet<PreminaryEnquiry> PreminaryEnquiry { get; set; }
        public DbSet<PreminaryEnquiryAction> PreminaryEnquiryAction { get; set; }
        public DbSet<ChargeSheet> ChargeSheet { get; set; }
        public DbSet<ChargeSheetServing> ChargeSheetServing { get; set; }
        public DbSet<ChargeSheetServingStatus> ChargeSheetServingStatus { get; set; }
        public DbSet<ChargeSheetServingMode> ChargeSheetServingMode { get; set; }
        public DbSet<ChargeSheetReply> ChargeSheetReply { get; set; }
        public DbSet<ChargeSheetEnquiryNotice> ChargeSheetEnquiryNotice { get; set; }
        public DbSet<ChargeSheetEnquiryNoticeServing> ChargeSheetEnquiryNoticeServing { get; set; }
        public DbSet<ChargeSheetEnquiryProceedings> ChargeSheetEnquiryProceedings { get; set; }
        public DbSet<ChargeSheetEnquiryReport> ChargeSheetEnquiryReport { get; set; }
        public DbSet<PostEnquiryPrerquisite> PostEnquiryPrerquisite { get; set; }
        public DbSet<FinalShowCauseNotice> FinalShowCauseNotice { get; set; }
        public DbSet<FinalShowCauseNoticeServing> FinalShowCauseNoticeServing { get; set; }
        public DbSet<FinalShowCauseNoticeReply> FinalShowCauseNoticeReply { get; set; }
        public DbSet<FinalShowCauseNoticeClarification> FinalShowCauseNoticeClarification { get; set; }
        public DbSet<FinalShowCauseNoticeClarificarionServing> FinalShowCauseNoticeClarificarionServing { get; set; }
        public DbSet<PunishmentOrder> PunishmentOrder { get; set; }
        public DbSet<PunishmentOrderDelivery> PunishmentOrderDelivery { get; set; }
        public DbSet<PunishmentOrderApeal> PunishmentOrderApeal { get; set; }
        public DbSet<PunishmentOrderApealReply> PunishmentOrderApealReply { get; set; }
        public DbSet<PunishmentOrderImplementation> PunishmentOrderImplementation { get; set; }
        public DbSet<ComplaintApplicant> ComplaintApplicant { get; set; }
        public DbSet<NoticePeriodProcess> NoticePeriodProcess { get; set; }
        public DbSet<FFSSettlementReleaseDetailT> FFSSettlementReleaseDetailT { get; set; }
        public DbSet<FFSCheckListCompliance> FFSCheckListCompliance { get; set; }
        public DbSet<FFSSettlementDetailT> FFSSettlementDetailT { get; set; }
        public DbSet<SeperationProcessT> SeperationProcessT { get; set; }
        public DbSet<OfficiatingParameter> OfficiatingParameter { get; set; }
        public DbSet<OfficiatingServiceBook> OfficiatingServiceBook { get; set; }
        public DbSet<DT_OfficiatingParameter> DT_OfficiatingParameter { get; set; }
        public DbSet<DT_OfficiatingServiceBook> DT_OfficiatingServiceBook { get; set; }
        public DbSet<EmployeeIR> EmployeeIR { get; set; }
        public DbSet<JVFileName> JVFileName { get; set; }
        public DbSet<JVFileFormat> JVFileFormat { get; set; }
        public DbSet<JVField> JVField { get; set; }
        public DbSet<DT_JVFileFormat> DT_JVFileFormat { get; set; }
        public DbSet<DT_JVField> DT_JVField { get; set; }

        public DbSet<LvConvertPolicy> LvConvertPolicy { get; set; }

        public DbSet<LoginAccessGeostruct> LoginAccessGeostruct { get; set; }
        public DbSet<LoginAccessFuncstruct> LoginAccessFuncstruct { get; set; }
        public DbSet<LoginAccessPaystruct> LoginAccessPaystruct { get; set; }

        public DbSet<DT_FacultyInternalExternal> DT_FacultyInternalExternal { get; set; }
        public DbSet<EmpReportingData> EmpReportingData { get; set; }
        public DbSet<BA_GeoStructTarget> BA_GeoStructTarget { get; set; }
        public DbSet<BA_TargetT> BA_TargetT { get; set; }
        public DbSet<BA_GeoStructTargetT> BA_GeoStructTargetT { get; set; }
        public DbSet<BA_Category> BA_Category { get; set; }
        public DbSet<BA_SubCategory> BA_SubCategory { get; set; }
        public DbSet<BA_EmpTarget> BA_EmpTarget { get; set; }

        //Added by Anandrao on 17/04/2023
        public DbSet<EmpDisciplineProcedings> EmpDisciplineProcedings { get; set; }
        public DbSet<CompanyCMS_SPS> CompanyCMS_SPS { get; set; }
        public DbSet<CompetencyModel> CompetencyModel { get; set; }
        public DbSet<CompetencyEvaluationModel> CompetencyEvaluationModel { get; set; }
        public DbSet<AppraisalAttributeModelObject> AppraisalAttributeModelObject { get; set; }
        public DbSet<AppraisalAttributeModel> AppraisalAttributeModel { get; set; }
        public DbSet<AppraisalBusinessAppraisalModel> AppraisalBusinessAppraisalModel { get; set; }
        public DbSet<AppraisalKRAModel> AppraisalKRAModel { get; set; }
        public DbSet<AppraisalKRAModelObjectV> AppraisalKRAModelObjectV { get; set; }
        public DbSet<AppraisalPotentialModel> AppraisalPotentialModel { get; set; }    
        public DbSet<AppraisalBusinessAppraisalModelObject> AppraisalBusinessAppraisalModelObject { get; set; }
        public DbSet<AppraisalKRAModelObject> AppraisalKRAModelObject { get; set; }  
        public DbSet<AppraisalPotentialModelObject> AppraisalPotentialModelObject { get; set; }
        public DbSet<PastExperienceModel> PastExperienceModel { get; set; }
        public DbSet<PastExperienceModelObject> PastExperienceModelObject { get; set; }
        public DbSet<PersonnelModel> PersonnelModel { get; set; }
        public DbSet<PersonnelModelObject> PersonnelModelObject { get; set; }    
        public DbSet<QualificationModel> QualificationModel { get; set; }    
        public DbSet<ServiceModel> ServiceModel { get; set; }      
        public DbSet<SkillModel> SkillModel { get; set; }
        public DbSet<TrainingModel> TrainingModel { get; set; }
        public DbSet<ServiceModelObject> ServiceModelObject { get; set; }
        public DbSet<SkillModelObject> SkillModelObject { get; set; }
        public DbSet<TrainingModelObject> TrainingModelObject { get; set; }    
        public DbSet<QualificationModelObject> QualificationModelObject { get; set; }
        public DbSet<CompetencyModelAssignment> CompetencyModelAssignment { get; set; }
        public DbSet<AppraisalAttributeModelObjectV> AppraisalAttributeModelObjectV { get; set; }
        public DbSet<AppraisalBusinessAppraisalModelObjectV> AppraisalBusinessAppraisalModelObjectV { get; set; }
        public DbSet<AppraisalBusinessAppraisalModelObjectT> AppraisalBusinessAppraisalModelObjectT { get; set; }
        public DbSet<CompetencyEmployeeDataT> CompetencyEmployeeDataT { get; set; }
        public DbSet<CompetencyEmployeeDataGeneration> CompetencyEmployeeDataGeneration { get; set; }
        public DbSet<AppraisalAttributeModelObjectT> AppraisalAttributeModelObjectT { get; set; }     
        public DbSet<CompetencyModelAssignment_OrgStructure> CompetencyModelAssignment_OrgStructure { get; set; }
        public DbSet<CompetencyPostAction> CompetencyPostAction { get; set; }
        public DbSet<PostActionTraining> PostActionTraining { get; set; }
        public DbSet<SuccessionEvaluationModel> SuccessionEvaluationModel { get; set; }
        public DbSet<SuccessionModel> SuccessionModel { get; set; }
        public DbSet<SuccessionModelAssignment> SuccessionModelAssignment { get; set; }
        public DbSet<SuccessionEmployeeDataGeneration> SuccessionEmployeeDataGeneration { get; set; }
        public DbSet<SuccessionEmployeeDataT> SuccessionEmployeeDataT { get; set; }              
        public DbSet<SussessionModelAssignment_OrgStructure> SussessionModelAssignment_OrgStructure { get; set; }
        public DbSet<SuccessionPostAction> SuccessionPostAction { get; set; }
        public DbSet<DIARYAdvanceClaim> DIARYAdvanceClaim { get; set; }
        public DbSet<DIARYSettlementClaim> DIARYSettlementClaim { get; set; }

        public DbSet<Witness> Witness { get; set; }

        public DbSet<DAEligibilityPolicy> DAEligibilityPolicy { get; set; }
        public DbSet<DAObject> DAObject { get; set; }
        public DbSet<HotelObject> HotelObject { get; set; }
        public DbSet<MisExpenseObject> MisExpenseObject { get; set; }
        public DbSet<SeniorityT> SeniorityT { get; set; }

        public DbSet<ExpenseCalendar> ExpenseCalendar { get; set; }
        public DbSet<ExpenseBudget> ExpenseBudget { get; set; }
        public DbSet<EmployeeExpense> EmployeeExpense { get; set; }
        public DbSet<ExpenseT> ExpenseT { get; set; }
        public DbSet<ExMSWFDetails> ExMSWFDetails { get; set; }
        public DbSet<CompanyExpense> CompanyExpense { get; set; }

        public DbSet<AttendanceAbsentPolicy> AttendanceAbsentPolicy { get; set; }
        public DbSet<AttendanceLeavePriority> AttendanceLeavePriority { get; set; }
        public DbSet<AttendanceActionPolicyAssignment> AttendanceActionPolicyAssignment { get; set; }
        public DbSet<AttendanceActionPolicyFormula> AttendanceActionPolicyFormula { get; set; }
        public DbSet<EmployeeAttendanceActionPolicyStruct> EmployeeAttendanceActionPolicyStruct { get; set; }
        public DbSet<EmployeeAttendanceActionPolicyStructDetails> EmployeeAttendanceActionPolicyStructDetails { get; set; }

        public DbSet<Senioritypolicy> Senioritypolicy { get; set; }

        public DbSet<EmployeePFTrust> EmployeePFTrust { get; set; }
        public DbSet<PFTEmployeeLedger> PFTEmployeeLedger { get; set; }
        public DbSet<CompanyPFTrust> CompanyPFTrust { get; set; }
        public DbSet<LoanAdvRequestPFT> LoanAdvRequestPFT { get; set; }
        public DbSet<LoanAdvRepaymentTPFT> LoanAdvRepaymentTPFT { get; set; }
        public DbSet<LoanAdvancePolicyPFT> LoanAdvancePolicyPFT { get; set; }
        public DbSet<LoanAdvanceHeadPFT> LoanAdvanceHeadPFT { get; set; }
        public DbSet<LoanWFDetails> LoanWFDetails { get; set; }
        public DbSet<InterestRate> InterestRate { get; set; }
        public DbSet<InterestPolicies> InterestPolicies { get; set; }
        public DbSet<EnquiryPanelExternal> EnquiryPanelExternal { get; set; }
        public DbSet<LvBankLedger> LvBankLedger { get; set; }

        public DbSet<ObjectValue> ObjectValue { get; set; }
        public DbSet<AppraisalPotentialModelObjectV> AppraisalPotentialModelObjectV { get; set; }
        public DbSet<PastExperienceModelObjectV> PastExperienceModelObjectV { get; set; }
        public DbSet<PersonnelModelObjectV> PersonnelModelObjectV { get; set; }
        public DbSet<QualificationModelObjectV> QualificationModelObjectV { get; set; }
        public DbSet<ServiceModelObjectV> ServiceModelObjectV { get; set; }
        public DbSet<SkillModelObjectV> SkillModelObjectV { get; set; }
        public DbSet<TrainingModelObjectV> TrainingModelObjectV { get; set; }
        public DbSet<AppraisalPotentialModelObjectT> AppraisalPotentialModelObjectT { get; set; }
        public DbSet<AppraisalKRAModelObjectT> AppraisalKRAModelObjectT { get; set; }
        public DbSet<PastExperienceModelObjectT> PastExperienceModelObjectT { get; set; }
        public DbSet<PersonnelModelObjectT> PersonnelModelObjectT { get; set; }
        public DbSet<QualificationModelObjectT> QualificationModelObjectT { get; set; }
        public DbSet<ServiceModelObjectT> ServiceModelObjectT { get; set; }
        public DbSet<SkillModelObjectT> SkillModelObjectT { get; set; }
        public DbSet<TrainingModelObjectT> TrainingModelObjectT { get; set; }

        public DbSet<AppraisalAttributeModelObjectF> AppraisalAttributeModelObjectF { get; set; }
        public DbSet<AppraisalBusinessAppraisalModelObjectF> AppraisalBusinessAppraisalModelObjectF { get; set; }
        public DbSet<AppraisalPotentialModelObjectF> AppraisalPotentialModelObjectF { get; set; }
        public DbSet<AppraisalKRAModelObjectF> AppraisalKRAModelObjectF { get; set; }
        public DbSet<PastExperienceModelObjectF> PastExperienceModelObjectF { get; set; }
        public DbSet<PersonnelModelObjectF> PersonnelModelObjectF { get; set; }
        public DbSet<QualificationModelObjectF> QualificationModelObjectF { get; set; }
        public DbSet<ServiceModelObjectF> ServiceModelObjectF { get; set; }
        public DbSet<SkillModelObjectF> SkillModelObjectF { get; set; }
        public DbSet<TrainingModelObjectF> TrainingModelObjectF { get; set; }
        public DbSet<CompetencyBatchProcessT> CompetencyBatchProcessT { get; set; }
        public DbSet<SuccessionBatchProcessT> SuccessionBatchProcessT { get; set; }

        public DbSet<StatutoryEffectiveMonthsPFT> StatutoryEffectiveMonthsPFT { get; set; }
        public DbSet<PFTACCalendar> PFTACCalendar { get; set; }
        public DbSet<EmpSettlementPFT> EmpSettlementPFT { get; set; }
        public DbSet<PFTTDSMaster> PFTTDSMaster { get; set; }
        public DbSet<PFMasterPFT> PFMasterPFT { get; set; }

        public DbSet<EmployeeDMS_Doc> EmployeeDMS_Doc { get; set; }
        public DbSet<EmployeeDMS_SubDoc> EmployeeDMS_SubDoc { get; set; }
        public DbSet<DMSWFDetails> DMSWFDetails { get; set; }
        public DbSet<FunctionalAllowancePolicy> FunctionalAllowancePolicy { get; set; }
        public DbSet<DMS_Bulletin> DMS_Bulletin { get; set; }
        //public DbSet<LvCreditPolicyLvHead> LvCreditPolicyLvHead { get; set; }
        public DbSet<CompanyCTC> CompanyCTC { get; set; }
        public DbSet<RegimiPolicy> RegimiPolicy { get; set; }
        public DbSet<RegimiScheme> RegimiScheme { get; set; }
        public DbSet<DT_LoanAdvancePolicy> DT_LoanAdvancePolicy { get; set; }

        public DbSet<LoanAdvRepaymentTSettlement> LoanAdvRepaymentTSettlement { get; set; }
        public DbSet<BMSModuleTypePolicyAssignment> BMSModuleTypePolicyAssignment { get; set; }

        public DbSet<BMSPaymentReq> BMSPaymentReq { get; set; }
        public DbSet<OfficiatingPaymentT> OfficiatingPaymentT { get; set; }

        public DbSet<QueryParameter> QueryParameter { get; set; }

        public DbSet<LeaveEncashExemptionT> LeaveEncashExemptionT { get; set; }
        public DbSet<LvEncashExemptDetails> LvEncashExemptDetails { get; set; }
        public DbSet<LvEncashExemptEligibleDays> LvEncashExemptEligibleDays { get; set; }
        

        protected override void OnModelCreating(DbModelBuilder db)
        {
            this.Configuration.LazyLoadingEnabled = false;
            db.Conventions.Remove<PluralizingTableNameConvention>();
            var Migration = false;
            var migVal = ConfigurationManager.AppSettings["migration"];
            if (!String.IsNullOrEmpty(migVal))
            {
                var temp = Convert.ToBoolean(migVal);
                Migration = temp;
            }
            if (Migration)
            {
                Database.SetInitializer<DataBaseContext>(new MigrateDatabaseToLatestVersion<DataBaseContext, P2BUltimate.Migrations.Configuration>("DataBaseContext"));
            }
            else
            {
                Database.SetInitializer<DataBaseContext>(new CreateDatabaseIfNotExists<DataBaseContext>());
            }
            //  Database.SetInitializer<DataBaseContext>(new MigrateDatabaseToLatestVersion<DataBaseContext, P2BUltimate.Migrations.Configuration>("DataBaseContext"));

        }
        private void FixEfProviderServicesProblem()
        {
            // The Entity Framework provider type 'System.Data.Entity.SqlServer.SqlProviderServices, EntityFramework.SqlServer'
            // for the 'System.Data.SqlClient' ADO.NET provider could not be loaded. 
            // Make sure the provider assembly is available to the running application. 
            // See http://go.microsoft.com/fwlink/?LinkId=260882 for more information.
            var instance = System.Data.Entity.SqlServer.SqlProviderServices.Instance;
        }

    }
}