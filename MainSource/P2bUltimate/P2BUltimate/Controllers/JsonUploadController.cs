using Appraisal;
using Attendance;
using ExcelDataReader;
using Leave;
using Newtonsoft.Json;
using P2b.Global;
using P2B.PFTRUST;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using P2BUltimate.Process;
using P2BUltimate.Security;
using Payroll;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.IO;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using Training;

namespace P2BUltimate.Controllers
{
    public class JsonUploadController : Controller
    {

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Partial()
        {
            return View("~/Views/Shared/_JsonUpload.cshtml");
        }

        public class ExcelformatEntity
        {
            //public string Column0 { get; set; }     //Loc_Code 123123                                   LocCode   
            //public string Column1 { get; set; }   // Loan_Code 12312                                    LoanAdvanceHead
            //public string Column2 { get; set; }     //Act_Code 123123                                   AccNo
            //public string Column3 { get; set; }    // Principal_Amt_Actual
            //public string Column4 { get; set; }    // Interest_Projected
            //public string Column5 { get; set; }  //  Interest_Actual                                    MonthlyInterest 
            //public string Column6 { get; set; }   //  Total_Projected
            //public string Column7 { get; set; }   //  Rate_of_Interest_Actual
            //public string Column8 { get; set; }   //  Rate_of_Interest_Projected
            //public string Column9 { get; set; }   //  Install_Paid                                      InstallmentPaid
            //public string Column10 { get; set; }   // Principal_Balance_Amt                                
            //public string Column11 { get; set; }   //     Interest_Balance_Amt   
            //public string Column12 { get; set; }   //     Total_PaidAmt
            //public string Column13 { get; set; }    //    PrincipalAmt                                  MonthlyPricipalAmount
            //public string Column14 { get; set; }    //    Loan_Amount
            //public string Column15 { get; set; }   //     Principal_Amt_Projected
            //public string Column16 { get; set; }    //   Total_Balance_Amt 

            public string Column0 { get; set; }     //Loc_Code 123123                                   LocCode   
            public string Column1 { get; set; }   // Loan_Code 12312                                    LoanAdvanceHead
            public string Column2 { get; set; }     //Act_Code 123123                                   AccNo
            public string Column3 { get; set; }    //                                                  MonthlyInterest
            public string Column4 { get; set; }    //                                                  InstallmentPaid
            public string Column5 { get; set; }  //                                                     MonthlyPricipalAmount  
            public string Column6 { get; set; }   //                                                    paymonth 
            public string Column7 { get; set; }   //                                                    Emp_Code 
            public string Column8 { get; set; }   //LoanSanctionedAmount,loanappliedamt                 Principal_Balance_Amt
            public string Column9 { get; set; }

        }
        public List<String> AddtionalCol(String tablename, String fileExtension, List<string> PropList)
        {
            switch (tablename.ToUpper())
            {
                case "OTHEARNINGDEDUCTIONT":
                    if (PropList != null)
                    {
                        PropList.Add("EmpCode");
                        PropList.RemoveAll(a => a == "FuncStruct");
                        PropList.RemoveAll(a => a == "GeoStruct");
                        PropList.RemoveAll(a => a == "PayStruct");

                    }
                    return PropList;
                case "ITAXTRANST":
                    if (PropList != null)
                    {
                        PropList.Add("EmpCode");
                        PropList.RemoveAll(a => a == "FuncStruct");
                        PropList.RemoveAll(a => a == "GeoStruct");
                        PropList.RemoveAll(a => a == "PayStruct");

                    }
                    return PropList;
                case "OTHEREARNINGT":
                    if (PropList != null)
                    {
                        PropList.Add("EmpCode");
                        PropList.RemoveAll(a => a == "FuncStruct");
                        PropList.RemoveAll(a => a == "GeoStruct");
                        PropList.RemoveAll(a => a == "PayStruct");

                    }
                    return PropList;

                case "SALATTENDANCET":
                    if (PropList != null)
                    {
                        List<string> NdColumn = new List<string>();
                        NdColumn.Add("EmpCode");
                        NdColumn.Add("PaybleDays");
                        NdColumn.Add("PayMonth");
                        PropList = NdColumn;
                    }
                    return PropList;
                case "EMPOFF":
                    if (PropList != null)
                    {
                        PropList.RemoveAll(a => a == "NationalityID");
                        PropList.Add("EmpCode");
                        PropList.Add("AdharNo");
                        PropList.Add("DLNO");
                        PropList.Add("EDLINo");
                        PropList.Add("ESICNo");
                        PropList.Add("GINo");
                        PropList.Add("LWFNo");
                        PropList.Add("PANNo");
                        PropList.Add("PensionNo");
                        PropList.Add("PFNo");
                        PropList.Add("PPNO");
                        PropList.Add("PTNo");
                        PropList.Add("RCNo");
                        PropList.Add("UANNo");
                        PropList.Add("VCNo");
                    }
                    return PropList;

                case "PFTEMPLOYEELEDGER":
                    if (PropList != null)
                    {
                        List<string> neededColumn = new List<string>();

                        neededColumn.Add("EmployeePFTrust_Id");
                        neededColumn.Add("OwnOpenBal");
                        neededColumn.Add("OwnerOpenBal");
                        neededColumn.Add("VPFOpenBal");
                        neededColumn.Add("OwnIntOpenBal");
                        neededColumn.Add("OwnerIntOpenBal");
                        neededColumn.Add("VPFIntOpenBal");
                        neededColumn.Add("PostingDate");
                        neededColumn.Add("CalcDate");
                        neededColumn.Add("IsTDSAppl");

                        PropList = neededColumn;
                    }
                    return PropList;

                case "LOANADVREQUEST":
                    if (PropList != null)
                    {
                        if (fileExtension == "txt")
                        {
                            //LoanSanctionedAmount,loanappliedamt
                            PropList.RemoveAll(a => a == "CloserDate");
                            PropList.RemoveAll(a => a == "IsInstallment");
                            PropList.RemoveAll(a => a == "LoanAdvRepaymentT");
                            PropList.RemoveAll(a => a == "EmployeePayroll");
                            // PropList.RemoveAll(a => a == "EnforcementDate");
                            //PropList.RemoveAll(a => a == "IsActive");
                            PropList.RemoveAll(a => a == "LoanAppliedAmount");
                            PropList.RemoveAll(a => a == "LoanSubAccNo");
                            PropList.RemoveAll(a => a == "Narration");
                            PropList.RemoveAll(a => a == "RequisitionDate");
                            PropList.RemoveAll(a => a == "SanctionedDate");
                            PropList.RemoveAll(a => a == "TotalInstall");
                            //PropList.RemoveAll(a => a == "PayMonth");
                            PropList.Add("EmpCode");

                        }
                        else
                        {
                            PropList.RemoveAll(a => a == "CloserDate");
                            PropList.RemoveAll(a => a == "IsInstallment");
                            PropList.RemoveAll(a => a == "LoanAdvRepaymentT");

                            //PropList.Add("IsInstallment");
                            //PropList.Add("LoanAdvanceHead");
                            PropList.Add("EmpCode");
                        }
                    }
                    return PropList;
                case "EMPLOYEE":
                    if (PropList != null)
                    {
                        PropList.RemoveAll(a => a == "EmpAcademicInfo");
                        PropList.RemoveAll(a => a == "EmpmedicalInfo");
                        PropList.RemoveAll(a => a == "EmpOffInfo");
                        PropList.RemoveAll(a => a == "EmpSocialInfo");

                        PropList.RemoveAll(a => a == "FuncStruct");
                        PropList.RemoveAll(a => a == "GeoStruct");
                        PropList.RemoveAll(a => a == "Login");
                        PropList.RemoveAll(a => a == "Nominees");
                        PropList.RemoveAll(a => a == "PayStruct");
                        PropList.RemoveAll(a => a == "TimingCode");
                        PropList.RemoveAll(a => a == "ValidFromDate");
                        PropList.RemoveAll(a => a == "ValidToDate");
                        PropList.RemoveAll(a => a == "VisaDetails");
                        PropList.RemoveAll(a => a == "WorkExp");
                        PropList.RemoveAll(a => a == "ReportingStructRights");
                        PropList.RemoveAll(a => a == "PassportDetails");
                        PropList.RemoveAll(a => a == "InsuranceDetails");
                        PropList.RemoveAll(a => a == "LogRegister");
                        PropList.RemoveAll(a => a == "GuarantorDetails");
                        PropList.RemoveAll(a => a == "FamilyDetails");

                        PropList.RemoveAll(a => a == "ForeignTrips");
                        PropList.RemoveAll(a => a == "Photo");
                        PropList.RemoveAll(a => a == "PreCompExp");

                        PropList.RemoveAll(a => a == "EmpName");
                        PropList.RemoveAll(a => a == "FatherName");
                        PropList.RemoveAll(a => a == "HusbandName");
                        PropList.RemoveAll(a => a == "MotherName");
                        PropList.RemoveAll(a => a == "BeforeMarriageName");



                        PropList.Add("EmpTitle_EmpName");
                        PropList.Add("FName_EmpName");
                        PropList.Add("MName_EmpName");
                        PropList.Add("LName_EmpName");


                        PropList.Add("EmpTitle_FatherName");
                        PropList.Add("FName_FatherName");
                        PropList.Add("MName_FatherName");
                        PropList.Add("LName_FatherName");

                        PropList.Add("EmpTitle_HusbandName");
                        PropList.Add("FName_HusbandName");
                        PropList.Add("MName_HusbandName");
                        PropList.Add("LName_HusbandName");

                        PropList.Add("EmpTitle_MotherName");
                        PropList.Add("FName_MotherName");
                        PropList.Add("MName_MotherName");
                        PropList.Add("LName_MotherName");

                        PropList.Add("EmpTitle_BeforeMarriageName");
                        PropList.Add("FName_BeforeMarriageName");
                        PropList.Add("MName_BeforeMarriageName");
                        PropList.Add("LName_BeforeMarriageName");

                        PropList.Add("ServiceBookDates_BirthDate");
                        PropList.Add("ServiceBookDates_JoiningDate");
                        PropList.Add("ServiceBookDates_ProbationPeriod");
                        PropList.Add("ServiceBookDates_ProbationDate");
                        PropList.Add("ServiceBookDates_ConfirmPeriod");
                        PropList.Add("ServiceBookDates_ConfirmationDate");
                        PropList.Add("ServiceBookDates_LastIncrementDate");
                        PropList.Add("ServiceBookDates_LastPromotionDate");
                        PropList.Add("ServiceBookDates_LastTransferDate");
                        PropList.Add("ServiceBookDates_SeniorityDate");
                        PropList.Add("ServiceBookDates_SeniorityNo");
                        PropList.Add("ServiceBookDates_RetirementDate");
                        PropList.Add("ServiceBookDates_ResignationDate");
                        PropList.Add("ServiceBookDates_ResignReason");
                        PropList.Add("ServiceBookDates_ServiceLastDate");
                        PropList.Add("ServiceBookDates_PFJoingDate");
                        PropList.Add("ServiceBookDates_PensionJoingDate");
                        PropList.Add("ServiceBookDates_PFExitDate");
                        PropList.Add("ServiceBookDates_PensionExitDate");


                        PropList.Add("Corporate");
                        PropList.Add("Region");
                        PropList.Add("Company");
                        PropList.Add("Division");
                        PropList.Add("Location");
                        PropList.Add("Department");
                        PropList.Add("Group");
                        PropList.Add("Unit");
                        PropList.Add("Grade");
                        PropList.Add("Level");
                        PropList.Add("JobStatus");
                        PropList.Add("Job");
                        PropList.Add("Position");
                    }
                    return PropList;
                case "LOOKUP":
                    if (PropList != null)
                    {
                        PropList.Add("LookupValue"); PropList.RemoveAll(a => a == "LookupValues");
                        PropList.Add("LookupValData");
                    }
                    return PropList;
                case "LOANADVREPAYMENTT":
                    if (PropList != null)
                    {
                        PropList.RemoveAll(a => a == "InstallementDate");
                        PropList.RemoveAll(a => a == "InstallmentAmount");
                        PropList.RemoveAll(a => a == "InstallmentCount");
                        PropList.RemoveAll(a => a == "RepaymentDate");
                        PropList.RemoveAll(a => a == "TotalLoanBalance");
                        PropList.RemoveAll(a => a == "TotalLoanPaid");

                        //PropList.Add("InstallmentPaid");
                        PropList.Add("LoanAccBranch");
                        PropList.Add("LoanAdvanceHead");
                        PropList.Add("LoanAccNo");
                        PropList.Add("EmpCode");
                    }
                    return PropList;
                case "ITFORM24QFILEFORMATDEFINITION":
                    if (PropList != null)
                    {
                        // PropList.RemoveAll(a => a == "FullDetails");
                    }
                    return PropList;
                case "INCREMENTSERVICEBOOK":
                    if (PropList != null)
                    {
                        PropList.RemoveAll(a => a == "StagnancyCount");
                        PropList.RemoveAll(a => a == "IsRegularIncrDate");
                        PropList.RemoveAll(a => a == "StagnancyAppl");
                        PropList.RemoveAll(a => a == "ReleaseFlag");
                        PropList.RemoveAll(a => a == "EmployeeCTC");
                        PropList.RemoveAll(a => a == "OrignalIncrDate");
                        PropList.RemoveAll(a => a == "ReleaseDate");
                        PropList.RemoveAll(a => a == "FuncStruct");
                        PropList.RemoveAll(a => a == "GeoStruct");
                        PropList.RemoveAll(a => a == "IsHold");
                        PropList.RemoveAll(a => a == "PayStruct");
                        PropList.RemoveAll(a => a == "EmployeePayroll");
                        PropList.RemoveAll(a => a == "EmployeePayroll_Id");
                        PropList.Add("EmpCode");
                        PropList.RemoveAll(a => a == "Narration");

                        PropList.Add("Corporate");
                        PropList.Add("Region");
                        PropList.Add("Company");
                        PropList.Add("Division");
                        PropList.Add("Location");
                        PropList.Add("Department");
                        PropList.Add("Group");
                        PropList.Add("Unit");
                        PropList.Add("Grade");
                        PropList.Add("Level");
                        PropList.Add("EmpStatus");
                        PropList.Add("EmpActingStatus");
                        PropList.Add("Job");
                        PropList.Add("Position");
                    }
                    return PropList;
                case "PROMOTIONSERVICEBOOK":
                    if (PropList != null)
                    {
                        PropList.RemoveAll(a => a == "GeoStruct");
                        PropList.RemoveAll(a => a == "OldPayScale");
                        PropList.RemoveAll(a => a == "NewPayScale");
                        PropList.RemoveAll(a => a == "OldPayScaleAgreement");
                        PropList.RemoveAll(a => a == "NewPayScaleAgreement");
                        PropList.RemoveAll(a => a == "ReleaseDate");
                        PropList.RemoveAll(a => a == "EmployeeCTC");
                        PropList.RemoveAll(a => a == "StagnancyAppl");
                        PropList.RemoveAll(a => a == "StagnancyCount");
                        PropList.RemoveAll(a => a == "IncrOldBasic");
                        PropList.RemoveAll(a => a == "IncrNewBasic");
                        PropList.RemoveAll(a => a == "ReleaseFlag");
                        PropList.RemoveAll(a => a == "Narration");
                        PropList.RemoveAll(a => a == "EmployeePayroll");
                        PropList.RemoveAll(a => a == "EmployeePayroll_Id");
                        PropList.RemoveAll(a => a == "OldPayStruct");
                        PropList.RemoveAll(a => a == "OldFuncStruct");
                        PropList.RemoveAll(a => a == "OldJobStatus");
                        PropList.RemoveAll(a => a == "NewPayStruct");
                        PropList.RemoveAll(a => a == "NewFuncStruct");
                        PropList.RemoveAll(a => a == "NewJobStatus");
                        PropList.Add("EmpCode");
                        //PropList.Add("OldDept");
                        //PropList.Add("NewDept");


                        PropList.Add("Corporate");
                        PropList.Add("Region");
                        PropList.Add("Company");
                        PropList.Add("Division");
                        PropList.Add("Location");
                        PropList.Add("Department");
                        PropList.Add("Group");
                        PropList.Add("Unit");
                        PropList.Add("OldGrade");
                        PropList.Add("OldLevel");
                        PropList.Add("OldEmpStatus");
                        PropList.Add("OldEmpActingStatus");
                        PropList.Add("OldJob");
                        PropList.Add("OldPosition");
                        PropList.Add("NewGrade");
                        PropList.Add("NewLevel");
                        PropList.Add("NewEmpStatus");
                        PropList.Add("NewEmpActingStatus");
                        PropList.Add("NewJob");
                        PropList.Add("NewPosition");
                    }
                    return PropList;
                case "TRANSFERSERVICEBOOK":
                    if (PropList != null)
                    {
                        PropList.RemoveAll(a => a == "ReleaseFlag");
                        PropList.RemoveAll(a => a == "ReleaseDate");
                        PropList.RemoveAll(a => a == "EmployeeCTC");
                        PropList.RemoveAll(a => a == "Narration");
                        PropList.RemoveAll(a => a == "EmployeePayroll");
                        PropList.RemoveAll(a => a == "EmployeePayroll_Id");
                        PropList.RemoveAll(a => a == "OldPayStruct");
                        PropList.RemoveAll(a => a == "OldFuncStruct");
                        PropList.RemoveAll(a => a == "OldJobStatus");
                        PropList.RemoveAll(a => a == "NewPayStruct");
                        PropList.RemoveAll(a => a == "NewFuncStruct");
                        PropList.RemoveAll(a => a == "NewJobStatus");
                        PropList.RemoveAll(a => a == "OldGeoStruct");
                        PropList.RemoveAll(a => a == "NewGeoStruct");
                        PropList.Add("EmpCode");

                        PropList.Add("OldCorporate");
                        PropList.Add("OldRegion");
                        PropList.Add("OldCompany");
                        PropList.Add("OldDivision");
                        PropList.Add("OldLocation");
                        PropList.Add("OldDepartment");
                        PropList.Add("OldGroup");
                        PropList.Add("OldUnit");
                        PropList.Add("OldGrade");
                        PropList.Add("OldLevel");
                        PropList.Add("OldEmpStatus");
                        PropList.Add("OldEmpActingStatus");
                        PropList.Add("OldJob");
                        PropList.Add("OldPosition");
                        PropList.Add("NewGrade");
                        PropList.Add("NewLevel");
                        PropList.Add("NewEmpStatus");
                        PropList.Add("NewEmpActingStatus");
                        PropList.Add("NewJob");
                        PropList.Add("NewPosition");

                        PropList.Add("NewCorporate");
                        PropList.Add("NewRegion");
                        PropList.Add("NewCompany");
                        PropList.Add("NewDivision");
                        PropList.Add("NewLocation");
                        PropList.Add("NewDepartment");
                        PropList.Add("NewGroup");
                        PropList.Add("NewUnit");
                    }
                    return PropList;
                case "OTHERSERVICEBOOK":
                    if (PropList != null)
                    {
                        PropList.RemoveAll(a => a == "OldPayScale");
                        PropList.RemoveAll(a => a == "NewPayScale");
                        PropList.RemoveAll(a => a == "OldPayScaleAgreement");
                        PropList.RemoveAll(a => a == "NewPayScaleAgreement");
                        PropList.RemoveAll(a => a == "IsBasicChangeAppl");
                        PropList.RemoveAll(a => a == "ReleaseFlag");
                        PropList.RemoveAll(a => a == "ReleaseDate");
                        PropList.RemoveAll(a => a == "Narration");
                        PropList.RemoveAll(a => a == "EmployeeCTC");
                        PropList.RemoveAll(a => a == "EmployeePayroll");
                        PropList.RemoveAll(a => a == "EmployeePayroll_Id");
                        PropList.RemoveAll(a => a == "OldPayStruct");
                        PropList.RemoveAll(a => a == "OldFuncStruct");
                        PropList.RemoveAll(a => a == "OldJobStatus");
                        PropList.RemoveAll(a => a == "NewPayStruct");
                        PropList.RemoveAll(a => a == "NewFuncStruct");
                        PropList.RemoveAll(a => a == "NewJobStatus");
                        PropList.RemoveAll(a => a == "GeoStruct");
                        PropList.Add("EmpCode");

                        PropList.Add("Corporate");
                        PropList.Add("Region");
                        PropList.Add("Company");
                        PropList.Add("Division");
                        PropList.Add("Location");
                        PropList.Add("Department");
                        PropList.Add("Group");
                        PropList.Add("Unit");
                        PropList.Add("Grade");
                        PropList.Add("Level");
                        PropList.Add("OldEmpStatus");
                        PropList.Add("OldEmpActingStatus");
                        PropList.Add("OldJob");
                        PropList.Add("OldPosition");
                        PropList.Add("NewGrade");
                        PropList.Add("NewLevel");
                        PropList.Add("NewEmpStatus");
                        PropList.Add("NewEmpActingStatus");
                        PropList.Add("NewJob");
                        PropList.Add("NewPosition");

                    }
                    return PropList;
                case "PERKTRANST":
                    if (PropList != null)
                    {
                        PropList.Add("EmpCode");
                        PropList.RemoveAll(a => a == "FinancialYear");
                    }
                    return PropList;
                case "INSURANCEDETAILST":
                    if (PropList != null)
                    {
                        PropList.Add("EmpCode");
                        PropList.RemoveAll(a => a == "EmployeePayroll");
                    }
                    return PropList;
                case "LVNEWREQ":
                    if (PropList != null)
                    {
                        PropList.Add("EmpCode");
                        PropList.Add("LeaveCode");
                        PropList.RemoveAll(a => a == "LeaveCalendar");
                        PropList.RemoveAll(a => a == "LeaveHead");
                        PropList.RemoveAll(a => a == "FullDetails");
                        PropList.RemoveAll(a => a == "CompOffReq");
                        PropList.RemoveAll(a => a == "ContactNo");
                        PropList.RemoveAll(a => a == "CreditDays");
                        PropList.RemoveAll(a => a == "DebitDays");
                        PropList.RemoveAll(a => a == "FromDate");
                        PropList.RemoveAll(a => a == "FromStat");
                        PropList.RemoveAll(a => a == "Incharge");
                        PropList.RemoveAll(a => a == "InputMethod");
                        PropList.RemoveAll(a => a == "IsCancel");
                        PropList.RemoveAll(a => a == "IsLock");
                        PropList.RemoveAll(a => a == "LvOrignal");
                        PropList.RemoveAll(a => a == "LvWFDetails");
                        PropList.RemoveAll(a => a == "Narration");
                        PropList.RemoveAll(a => a == "Path");
                        PropList.RemoveAll(a => a == "Reason");
                        PropList.RemoveAll(a => a == "ReqDate");
                        PropList.RemoveAll(a => a == "ResumeDate");
                        PropList.RemoveAll(a => a == "ToDate");
                        PropList.RemoveAll(a => a == "ToStat");
                        PropList.RemoveAll(a => a == "TrClosed");
                        PropList.RemoveAll(a => a == "TrReject");
                        PropList.RemoveAll(a => a == "WFStatus");
                        PropList.RemoveAll(a => a == "PrefixSuffix");
                    }
                    return PropList;
                case "YEARLYPAYMENTT":
                    if (PropList != null)
                    {
                        PropList.Add("EmpCode");
                        PropList.RemoveAll(a => a == "FuncStruct");
                        PropList.RemoveAll(a => a == "GeoStruct");
                        PropList.RemoveAll(a => a == "PayStruct");
                        PropList.RemoveAll(a => a == "EmployeePayroll_Id");
                        PropList.RemoveAll(a => a == "EmployeePayroll");
                        PropList.RemoveAll(a => a == "ITChallan");
                        PropList.RemoveAll(a => a == "FinancialYear");
                        PropList.RemoveAll(a => a == "LvEncashReq");
                        PropList.RemoveAll(a => a == "Narration");
                    }
                    return PropList;
                case "ADDRESS":
                    if (PropList != null)
                    {
                        //PropList.RemoveAll(a => a == "Address1");
                        //PropList.RemoveAll(a => a == "Address2");
                        //PropList.RemoveAll(a => a == "Address3");
                        //PropList.RemoveAll(a => a == "Landmark");
                        //PropList.RemoveAll(a => a == "Country");
                        PropList.RemoveAll(a => a == "Area");
                        PropList.RemoveAll(a => a == "City");
                        PropList.RemoveAll(a => a == "Taluka");
                        PropList.RemoveAll(a => a == "District");
                        PropList.RemoveAll(a => a == "State");
                        PropList.RemoveAll(a => a == "StateRegion");
                        PropList.RemoveAll(a => a == "FullAddress");
                        PropList.RemoveAll(a => a == "Country");

                        PropList.Add("EmpCode");
                        PropList.Add("AddressType");
                        PropList.Add("AreaCode");
                        PropList.Add("AreaName");
                        PropList.Add("AreaPincode");
                        PropList.Add("CityCategory");
                        PropList.Add("CityCode");
                        PropList.Add("CityName");
                        PropList.Add("TalukaCode");
                        PropList.Add("TalukaName");
                        PropList.Add("DistrictCode");
                        PropList.Add("DistrictName");
                        PropList.Add("StateRegionCode");
                        PropList.Add("StateRegionName");
                        PropList.Add("StateCode");
                        PropList.Add("StateName");
                        PropList.Add("CountryCode");
                        PropList.Add("CountryName");
                        PropList.Add("EmailId");
                        PropList.Add("Website");
                        PropList.Add("FaxNo");
                        PropList.Add("MobileNo");
                        PropList.Add("STDCode");
                        PropList.Add("LandlineNo");
                    }
                    return PropList;
                case "LOCATION":
                    if (PropList != null)
                    {

                        PropList.RemoveAll(a => a == "LocationObj");
                        PropList.RemoveAll(a => a == "Address");
                        PropList.RemoveAll(a => a == "Incharge");
                        PropList.RemoveAll(a => a == "BusinessCategory");
                        PropList.RemoveAll(a => a == "Area");
                        PropList.RemoveAll(a => a == "City");
                        PropList.RemoveAll(a => a == "Taluka");
                        PropList.RemoveAll(a => a == "District");
                        PropList.RemoveAll(a => a == "State");
                        PropList.RemoveAll(a => a == "StateRegion");
                        PropList.RemoveAll(a => a == "FullAddress");
                        PropList.RemoveAll(a => a == "Country");

                        PropList.Remove("Address_Id");
                        PropList.Remove("BusinessCategory_Id");
                        PropList.Remove("ContactDetails_Id");
                        PropList.Remove("ContactDetails");
                        PropList.Remove("Department");
                        PropList.Remove("FullDetails");
                        PropList.Remove("GeoFencingParameter");
                        PropList.Remove("GeoFencingParameter_Id");
                        PropList.Remove("HolidayCalendar");
                        PropList.Remove("Incharge_Id");
                        PropList.Remove("WeeklyOffCalendar");
                        PropList.Remove("Type_Id");
                        PropList.Remove("LocationObj_Id");



                        PropList.Add("AreaCode");
                        PropList.Add("AreaName");
                        PropList.Add("AreaPincode");
                        PropList.Add("CityCategory");
                        PropList.Add("CityCode");
                        PropList.Add("CityName");
                        PropList.Add("TalukaCode");
                        PropList.Add("TalukaName");
                        PropList.Add("DistrictCode");
                        PropList.Add("DistrictName");
                        PropList.Add("StateRegionCode");
                        PropList.Add("StateRegionName");
                        PropList.Add("StateCode");
                        PropList.Add("StateName");
                        PropList.Add("CountryCode");
                        PropList.Add("CountryName");
                        PropList.Add("EmailId");
                        PropList.Add("Website");
                        PropList.Add("FaxNo");
                        PropList.Add("MobileNo");
                        PropList.Add("STDCode");
                        PropList.Add("LandlineNo");
                        PropList.Add("LocCode");
                        PropList.Add("LocDesc");
                        PropList.Add("Incharge");
                        PropList.Add("Address1");
                        PropList.Add("Address2");
                        PropList.Add("Address3");
                        PropList.Add("Landmark");



                    }
                    return PropList;

                case "ITINVESTMENTPAYMENT":
                    if (PropList != null)
                    {

                        PropList.RemoveAll(a => a == "EmployeePayroll_Id");
                        PropList.RemoveAll(a => a == "EmployeePayroll");
                        PropList.RemoveAll(a => a == "FinancialYear");
                        PropList.RemoveAll(a => a == "ITSection");
                        PropList.RemoveAll(a => a == "LoanAdvanceHead");
                        PropList.RemoveAll(a => a == "Narration");
                        PropList.RemoveAll(a => a == "TrClosed");
                        PropList.RemoveAll(a => a == "TrReject");
                        PropList.RemoveAll(a => a == "PrefixSuffix");
                        PropList.RemoveAll(a => a == "Reason");
                        PropList.RemoveAll(a => a == "WFStatus");
                        PropList.RemoveAll(a => a == "WFDetails");
                        PropList.RemoveAll(a => a == "InputMethod");
                        PropList.RemoveAll(a => a == "isCancel");
                        PropList.RemoveAll(a => a == "Path");
                        PropList.RemoveAll(a => a == "FullDetails");
                        PropList.Add("EmpCode");
                        PropList.Add("FinYearFrom");
                        PropList.Add("FinYearTo");
                    }
                    return PropList;

                case "ITSECTION10PAYMENT":
                    if (PropList != null)
                    {

                        PropList.RemoveAll(a => a == "EmployeePayroll_Id");
                        PropList.RemoveAll(a => a == "EmployeePayroll");
                        PropList.RemoveAll(a => a == "FinancialYear");
                        PropList.RemoveAll(a => a == "ITSection");
                        PropList.RemoveAll(a => a == "Narration");
                        PropList.Add("EmpCode");
                        PropList.Add("FinYearFrom");
                        PropList.Add("FinYearTo");
                    }
                    return PropList;


                case "ITSECTION24PAYMENT":
                    if (PropList != null)
                    {
                        PropList.Add("EmpCode");
                        PropList.Add("ITSectionType");
                    }
                    return PropList;

                case "EMPSALSTRUCT":
                    if (PropList != null)
                    {

                        PropList.RemoveAll(a => a == "EmployeePayroll_Id");
                        PropList.RemoveAll(a => a == "EmployeePayroll");
                        PropList.RemoveAll(a => a == "EndDate");
                        PropList.RemoveAll(a => a == "EmpSalStructDetails");
                        PropList.RemoveAll(a => a == "FullDetails");
                        PropList.RemoveAll(a => a == "PayDays");
                        PropList.Add("EmpCode");
                        PropList.Add("SalCode");
                        PropList.Add("Amount");
                    }
                    return PropList;
                default:
                    return null;
            }
        }
        public class ClosingCheck
        {
            public string Loc_Code { get; set; }
            public string Loan_head { get; set; }
            public string Acc_No { get; set; }
        }
        public ActionResult CheckIsQuit(string paymonth)
        {
            List<DataDump> testing = new List<DataDump>();
            List<LoanAdvRequest> loanadvreq = new List<LoanAdvRequest>();
            using (DataBaseContext db = new DataBaseContext())
            {

                testing = db.DataDump.Where(q => q.TableName == "LoanAdvRepaymentT").ToList();

                loanadvreq = db.LoanAdvRequest
                          .Include(q => q.LoanAdvRepaymentT)
                          .Include(q => q.LoanAdvanceHead)
                          .Include(q => q.EmployeePayroll.Employee.EmpName)
                          .Include(q => q.LoanAccBranch.LocationObj)
                          .AsNoTracking()
                          .Where(a => a.IsActive == true &&
                          a.LoanAdvRepaymentT.Any(q => q.PayMonth == paymonth)).ToList();
            }
            HttpPostedFileBase upload = null;
            if (System.Web.HttpContext.Current.Request.Files.AllKeys.Any())
            {
                var pic = System.Web.HttpContext.Current.Request.Files["upload"];
                upload = new HttpPostedFileWrapper(pic);
            }
            if (upload != null && upload.ContentLength > 0)
            {
                Stream stream = upload.InputStream;
                //req?
                Assembly oAssembly;
                Type type;
                var filename = upload.FileName.Split('.')[0];
                string fileExtension = upload.FileName.Split('.')[1];


                if (filename == "OtherEarningT")
                {
                    filename = "OthEarningDeductionT";
                }
                if (filename == "ITInvestmentPayment80C" || filename == "ITINVESTMENTPAYMENTSECTION80DTO80U" || filename == "ITINVESTMENTPAYMENT80CCCTO80CCF")
                {
                    filename = "ITInvestmentPayment";
                }

                oAssembly = Assembly.Load("Core");
                type = oAssembly.GetType("P2b.Global." + filename);
                if (type == null)
                {
                    oAssembly = Assembly.Load("Leave");
                    type = oAssembly.GetType("Leave." + filename);
                }
                if (type == null)
                {
                    oAssembly = Assembly.Load("Payroll");
                    type = oAssembly.GetType("Payroll." + filename);
                }
                if (type == null)
                {
                    oAssembly = Assembly.Load("Training");
                    type = oAssembly.GetType("Training." + filename);
                }
                if (type == null)
                {
                    return Json(new { responseText = "Invalid excel..!" }, JsonRequestBehavior.AllowGet);

                }
                Object instance = Activator.CreateInstance(type);
                var Prop = instance.GetType().GetProperties().ToList();
                List<string> PropList = new List<string>();
                foreach (var item in Prop)
                {
                    if (item.Name != "DBTrack" && item.Name != "Id" && item.Name != "RowVersion")
                    {
                        PropList.Add(item.Name);
                    }
                }

                var ListCol = AddtionalCol(filename, fileExtension, PropList);
                ListCol.Sort();
                List<ClosingCheck> ClosingCheckList = new List<ClosingCheck>();
                List<ExcelformatEntity> excList = new List<ExcelformatEntity>();

                ExcelformatEntity exc = new ExcelformatEntity()
                {
                    Column0 = "LoanAccBranch",
                    Column1 = "LoanAdvHead",
                    Column2 = "LoanAccountNo",
                    Column3 = "MonthlyIntrest",
                    Column4 = "MonthlyInstallmentAmount",
                    Column5 = "MonthlyPrincipleAmt",
                    Column6 = "EnforcementDate",
                    Column7 = "EmpCode",
                    Column8 = "LoanSanctionAmount",
                    Column9 = "IsActive",
                };
                excList.Add(exc);
                //}
                using (var streamReader = new StreamReader(stream, Encoding.UTF8))
                {
                    string line;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        ClosingCheck closechk = new ClosingCheck()
                            {
                                //LoanAccBranch
                                Loc_Code = line.Substring(Convert.ToInt16(testing[0].PositionNo) - 1, Convert.ToInt16(testing[0].PositionNoLength)).Trim(),
                                //LoanAdvanceHead
                                Loan_head = line.Substring(Convert.ToInt16(testing[1].PositionNo) - 1, Convert.ToInt16(testing[1].PositionNoLength)).Trim(),
                                // LoanAccNo
                                Acc_No = line.Substring(Convert.ToInt16(testing[2].PositionNo) - 1, Convert.ToInt16(testing[2].PositionNoLength)).Trim()
                            };

                        ClosingCheckList.Add(closechk);
                    }


                    List<LoanAdvRequest> loanadvreqNotExist = new List<LoanAdvRequest>();
                    foreach (var item in loanadvreq)
                    {
                        int countofexistance = 0;
                        foreach (var chk in ClosingCheckList)
                        {
                            if (item.LoanAccNo == chk.Acc_No && item.LoanAdvanceHead.Code == chk.Loan_head && item.LoanAccBranch.LocationObj.LocCode == chk.Loc_Code)
                            {
                                countofexistance = 1;
                                break;
                            }
                        }

                        if (countofexistance == 0)
                        {
                            loanadvreqNotExist.Add(item);
                            ExcelformatEntity excSingle = new ExcelformatEntity()
                            {
                                Column0 = item.LoanAccBranch.LocationObj.LocCode,
                                Column1 = item.LoanAdvanceHead.Code,
                                Column2 = item.LoanAccNo,
                                Column3 = item.MonthlyInterest.ToString(),
                                Column4 = item.MonthlyInstallmentAmount.ToString(),
                                Column5 = item.MonthlyPricipalAmount.ToString(),
                                Column6 = item.EnforcementDate.Value.ToShortDateString(),
                                Column7 = item.EmployeePayroll.Employee.EmpCode,
                                Column8 = item.LoanSanctionedAmount.ToString(),
                                Column9 = "false",
                            };
                            excList.Add(excSingle);
                        }
                    }
                }
                var jsona = JsonConvert.SerializeObject(excList, Formatting.Indented);
                if (jsona != null)
                {
                    return Json(new { json = jsona, col = ListCol }, JsonRequestBehavior.AllowGet);
                }
            }
            return null;
        }

        [HttpPost]
        public ActionResult Upload(string PaymonthforLoanadv, string loanfilepath, string Type)
        {
            List<string> Msg = new List<string>();
            HttpPostedFileBase upload = null;
            if (System.Web.HttpContext.Current.Request.Files.AllKeys.Any())
            {
                var pic = System.Web.HttpContext.Current.Request.Files["upload"];
                upload = new HttpPostedFileWrapper(pic);
            }
            if (upload != null && upload.ContentLength > 0)
            {
                Stream stream = upload.InputStream;
                Assembly oAssembly;
                Type type;
                var filename = upload.FileName.Split('.')[0];
                string fileExtension = upload.FileName.Split('.')[1];

                if (filename == "LOAN" && Type != null)
                {
                    string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\App_Data\\ExcelFormat";
                    bool exists = System.IO.Directory.Exists(requiredPath);
                    string localPath;
                    if (!exists)
                    {
                        localPath = new Uri(requiredPath).LocalPath;
                        System.IO.Directory.CreateDirectory(localPath);
                    }
                    string path = requiredPath + @"\LoanAdvRequest_" + Convert.ToDateTime("01/" + PaymonthforLoanadv).ToString("MMyyyy") + ".txt";
                    localPath = new Uri(path).LocalPath;

                    if (System.IO.File.Exists(localPath))
                    {
                        System.IO.File.Delete(localPath);
                    }

                    using (var fs = new FileStream(localPath, FileMode.OpenOrCreate))
                    {
                        ExcelDataReader.IExcelDataReader reader1 = null;
                        if (upload.FileName.EndsWith(".xls"))
                        {
                            reader1 = ExcelReaderFactory.CreateBinaryReader(stream);
                        }
                        else if (upload.FileName.EndsWith(".xlsx"))
                        {
                            reader1 = ExcelReaderFactory.CreateOpenXmlReader(stream);
                        }
                        else
                        {

                        }

                        StreamWriter str = new StreamWriter(fs);
                        str.BaseStream.Seek(0, SeekOrigin.Begin);
                        while (reader1.Read())
                        {
                            if (Convert.ToString(reader1.GetValue(0)) != "BranchCode" && Convert.ToString(reader1.GetValue(0)) != "")
                            {
                                str.WriteLine(Convert.ToString(reader1.GetValue(0)).PadLeft(6, ' ').ToString() +
                                    Convert.ToString(reader1.GetValue(1)).PadLeft(8, ' ').ToString() +
                                    Convert.ToString(reader1.GetValue(2)).PadLeft(8, ' ').ToString() +
                                    Convert.ToString(reader1.GetValue(3)).PadLeft(18, ' ').ToString() +
                                    Convert.ToString(reader1.GetValue(4)).PadLeft(18, ' ').ToString() +
                                    Convert.ToString(reader1.GetValue(5)).PadLeft(18, ' ').ToString() +
                                    Convert.ToString(reader1.GetValue(6)).PadLeft(18, ' ').ToString() +
                                    Convert.ToString(reader1.GetValue(7)).PadLeft(18, ' ').ToString() +
                                    Convert.ToString(reader1.GetValue(8)).PadLeft(18, ' ').ToString() +
                                    Convert.ToString(reader1.GetValue(9)).PadLeft(18, ' ').ToString() +
                                    Convert.ToString(reader1.GetValue(10)).PadLeft(7, ' ').ToString() +
                                    Convert.ToString(reader1.GetValue(11)).PadLeft(7, ' ').ToString() +
                                    Convert.ToString(reader1.GetValue(12)).PadLeft(18, ' ').ToString() +
                                    Convert.ToString(reader1.GetValue(13)).PadLeft(18, ' ').ToString() +
                                    Convert.ToString(reader1.GetValue(14)).PadLeft(18, ' ').ToString() +
                                    Convert.ToString(reader1.GetValue(15)).PadLeft(18, ' ').ToString() +
                                    Convert.ToString(reader1.GetValue(16)).PadLeft(18, ' ').ToString()

                                    );
                            }



                        }
                        reader1.Close();
                        str.Flush();
                        str.Close();
                        fs.Close();
                        string Loanfilename = localPath.Substring(localPath.LastIndexOf("\\") + 1);
                        if (Loanfilename != null || Loanfilename != "")
                        {
                            filename = Loanfilename.Split('_')[0];
                            fileExtension = Loanfilename.Split('.')[1];
                        }
                    }

                    // Excel file  afetr convert in notepad
                    Type type1;

                    if (filename == "OtherEarningT")
                    {
                        filename = "OthEarningDeductionT";
                    }
                    if (filename == "ITInvestmentPayment80C" || filename == "ITINVESTMENTPAYMENTSECTION80DTO80U" || filename == "ITINVESTMENTPAYMENT80CCCTO80CCF")
                    {
                        filename = "ITInvestmentPayment";
                    }
                    oAssembly = Assembly.Load("Core");
                    type1 = oAssembly.GetType("P2b.Global." + filename);

                    if (type1 == null)
                    {
                        oAssembly = Assembly.Load("Payroll");
                        type1 = oAssembly.GetType("Payroll." + filename);
                    }

                    Object instance1 = Activator.CreateInstance(type1);
                    var Prop1 = instance1.GetType().GetProperties().ToList();
                    List<string> PropList1 = new List<string>();
                    foreach (var item in Prop1)
                    {
                        if (item.Name != "DBTrack" && item.Name != "Id" && item.Name != "RowVersion")
                        {
                            PropList1.Add(item.Name);
                        }
                    }

                    var ListCol1 = AddtionalCol(filename, fileExtension, PropList1);
                    ListCol1.Sort();

                    try
                    {
                        List<DataDump> testing = new List<DataDump>();
                        List<ExcelformatEntity> excList = new List<ExcelformatEntity>();

                        ExcelformatEntity exc = new ExcelformatEntity()
                        {
                            Column0 = "LoanAccBranch",
                            Column1 = "LoanAdvHead",
                            Column2 = "LoanAccountNo",
                            Column3 = "MonthlyIntrest",
                            Column4 = "MonthlyInstallmentAmount",
                            Column5 = "MonthlyPrincipleAmt",
                            Column6 = "EnforcementDate",
                            Column7 = "EmpCode",
                            Column8 = "LoanSanctionAmount",
                            Column9 = "IsActive",
                        };
                        excList.Add(exc);
                        //}

                        if (Type != null)
                        {
                            if (PaymonthforLoanadv == null)
                            {
                                return Json(new { responseText = "Select Paymonth..!" }, JsonRequestBehavior.AllowGet);
                            }

                            // List<string> salhead = new List<string>();
                            List<SalHeadFormula> salheadList = new List<SalHeadFormula>();
                            string CompCode = null;
                            using (DataBaseContext db = new DataBaseContext())
                            {
                                var Id = Convert.ToInt32(SessionManager.CompanyId);
                                string _CompCode = db.Company.Where(e => e.Id == Id).SingleOrDefault().Code.ToUpper();
                                CompCode = _CompCode;
                                testing = db.DataDump.Where(q => q.TableName == "LoanAdvRepaymentT").ToList();

                                //var PayScaleAssignmentData = db.PayScaleAssignment
                                //     .Include(q => q.SalaryHead)
                                //     .Include(q => q.SalHeadFormula)
                                //     .Include(q => q.SalHeadFormula.Select(a => a.SalWages))
                                //     .Include(q => q.SalHeadFormula.Select(a => a.SalWages.RateMaster))
                                //     .Include(q => q.SalHeadFormula.Select(a => a.SalWages.RateMaster.Select(lm => lm.SalHead)))
                                //     .Where(r => r.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "PERK")
                                //   .Select(z => z.SalaryHead.Code)
                                //       .ToList();

                                //salheadList = db.SalHeadFormula
                                //    .Include(q => q.SalWages)
                                //    .Include(q => q.SalWages.RateMaster)
                                //    .Include(q => q.SalWages.RateMaster.Select(a => a.SalHead)).ToList();

                                //List<PerkTransT> Perktranst = db.PerkTransT
                                //      .Include(q => q.SalaryHead)
                                //      .Where(q => q.PayMonth == PaymonthforLoanadv && PayScaleAssignmentData.Contains(q.SalaryHead.Code))
                                //      .ToList();

                                //db.PerkTransT.RemoveRange(Perktranst);
                                //db.SaveChanges();

                            }



                            //  using (var streamReader = new StreamReader(stream, Encoding.UTF8))
                            using (var streamReader = new StreamReader(localPath))
                            {
                                string line;
                                Boolean LNcode = false;
                                string oldlNcode = "0";
                                while ((line = streamReader.ReadLine()) != null)
                                {
                                    int counter = 0;
                                    //do not delete
                                    #region test

                                    #endregion test
                                    //do not delete

                                    string Loc_Code = line.Substring(Convert.ToInt16(testing[0].PositionNo) - 1, Convert.ToInt16(testing[0].PositionNoLength)).Trim();
                                    string Loan_head = line.Substring(Convert.ToInt16(testing[1].PositionNo) - 1, Convert.ToInt16(testing[1].PositionNoLength)).Trim();
                                    string Acc_No = line.Substring(Convert.ToInt16(testing[2].PositionNo) - 1, Convert.ToInt16(testing[2].PositionNoLength)).Trim();

                                    string ChkData = line.Substring(Convert.ToInt16(testing[5].PositionNo) - 1, Convert.ToInt16(testing[5].PositionNoLength)).Trim();
                                    double MonthlyIntrest = ChkData == "" ? 0 : Convert.ToDouble(ChkData);

                                    ChkData = line.Substring(Convert.ToInt16(testing[12].PositionNo) - 1, Convert.ToInt16(testing[12].PositionNoLength)).Trim();
                                    double InstallmentPaid = ChkData == "" ? 0 : Convert.ToDouble(ChkData);

                                    ChkData = line.Substring(Convert.ToInt16(testing[10].PositionNo) - 1, Convert.ToInt16(testing[10].PositionNoLength)).Trim();
                                    double Total_LoanSanct_appliedamt = ChkData == "" ? 0 : Convert.ToDouble(ChkData);

                                    ChkData = line.Substring(Convert.ToInt16(testing[13].PositionNo) - 1, Convert.ToInt16(testing[13].PositionNoLength)).Trim();
                                    double Monthly_principleAmt = ChkData == "" ? 0 : Convert.ToDouble(ChkData);
                                    double ActualPerkAmt = 0;
                                    double ProjPerkAmt = 0;
                                    double BankIntrest = 0;
                                    double SIBIntrest = 0;
                                    double MonthlyPrinciple = 0;
                                    double MonthlyInterest = 0;
                                    double AnneigtyCPriPro = 0;
                                    double AnneigtyCPriAct = 0;
                                    double AnnintdedPro = 0;
                                    double AnnintdedAct = 0;
                                    if (CompCode == "ASBL")
                                    {
                                        ChkData = line.Substring(114, 18).Trim();
                                        ActualPerkAmt = ChkData == "" ? 0 : Convert.ToDouble(ChkData);

                                        ChkData = line.Substring(96, 18).Trim();
                                        ProjPerkAmt = ChkData == "" ? 0 : Convert.ToDouble(ChkData);

                                        ChkData = line.Substring(150, 7).Trim();
                                        BankIntrest = ChkData == "" ? 0 : Convert.ToDouble(ChkData);

                                        ChkData = line.Substring(157, 7).Trim();
                                        SIBIntrest = ChkData == "" ? 0 : Convert.ToDouble(ChkData);

                                        ChkData = line.Substring(218, 18).Trim();
                                        MonthlyPrinciple = ChkData == "" ? 0 : Convert.ToDouble(ChkData);

                                        ChkData = line.Substring(236, 17).Trim();
                                        MonthlyInterest = ChkData == "" ? 0 : Convert.ToDouble(ChkData);

                                        ChkData = line.Substring(24, 18).Trim();
                                        AnneigtyCPriPro = ChkData == "" ? 0 : Convert.ToDouble(ChkData);

                                        ChkData = line.Substring(42, 18).Trim();
                                        AnneigtyCPriAct = ChkData == "" ? 0 : Convert.ToDouble(ChkData);

                                        ChkData = line.Substring(60, 18).Trim();
                                        AnnintdedPro = ChkData == "" ? 0 : Convert.ToDouble(ChkData);

                                        ChkData = line.Substring(78, 18).Trim();
                                        AnnintdedAct = ChkData == "" ? 0 : Convert.ToDouble(ChkData);
                                    }
                                    else
                                    {


                                        ChkData = line.Substring(113, 18).Trim();
                                        ActualPerkAmt = ChkData == "" ? 0 : Convert.ToDouble(ChkData);

                                        ChkData = line.Substring(95, 18).Trim();
                                        ProjPerkAmt = ChkData == "" ? 0 : Convert.ToDouble(ChkData);

                                        ChkData = line.Substring(149, 7).Trim();
                                        BankIntrest = ChkData == "" ? 0 : Convert.ToDouble(ChkData);

                                        ChkData = line.Substring(156, 7).Trim();
                                        SIBIntrest = ChkData == "" ? 0 : Convert.ToDouble(ChkData);

                                        ChkData = line.Substring(217, 18).Trim();
                                        MonthlyPrinciple = ChkData == "" ? 0 : Convert.ToDouble(ChkData);

                                        ChkData = line.Substring(235, 17).Trim();
                                        MonthlyInterest = ChkData == "" ? 0 : Convert.ToDouble(ChkData);

                                        ChkData = line.Substring(23, 18).Trim();
                                        AnneigtyCPriPro = ChkData == "" ? 0 : Convert.ToDouble(ChkData);

                                        ChkData = line.Substring(41, 18).Trim();
                                        AnneigtyCPriAct = ChkData == "" ? 0 : Convert.ToDouble(ChkData);

                                        ChkData = line.Substring(59, 18).Trim();
                                        AnnintdedPro = ChkData == "" ? 0 : Convert.ToDouble(ChkData);

                                        ChkData = line.Substring(77, 18).Trim();
                                        AnnintdedAct = ChkData == "" ? 0 : Convert.ToDouble(ChkData);
                                    }
                                    string newLNcode = Loan_head.Trim();
                                    if (newLNcode != oldlNcode)
                                    {
                                        if (LNcode == false)
                                        {
                                            LNcode = true;

                                            using (DataBaseContext db = new DataBaseContext())
                                            {
                                                var LoanHeadIdp = db.LoanAdvanceHead.Include(e => e.PerkHead).Where(a => a.Code == Loan_head.Trim()).FirstOrDefault();
                                                if (LoanHeadIdp.PerkHead != null)
                                                {


                                                    var sdddp = LoanHeadIdp.PerkHead.Id;
                                                    if (sdddp != null)
                                                    {

                                                        PayScaleAssignment paysc = db.PayScaleAssignment
                                                            .Include(q => q.SalaryHead)
                                                             .Include(q => q.SalaryHead.Frequency)
                                                            .Where(qa => qa.SalaryHead.Id == sdddp)
                                                                .SingleOrDefault();
                                                        if (paysc != null)
                                                        {


                                                            int chksalhead = paysc.SalaryHead.Id;
                                                            if (paysc.SalaryHead.Frequency.LookupVal.ToUpper() == "MONTHLY")
                                                            {


                                                                List<PerkTransT> Perktranst = db.PerkTransT
                                                                  .Include(q => q.SalaryHead)
                                                                  .Where(q => q.PayMonth == PaymonthforLoanadv && paysc.SalaryHead.Id == chksalhead)
                                                                  .ToList();

                                                                db.PerkTransT.RemoveRange(Perktranst);
                                                                db.SaveChanges();
                                                            }
                                                            else
                                                            {
                                                                int CompId = Convert.ToInt32(SessionManager.CompanyId);
                                                                CompanyPayroll OCompanyPayroll = db.CompanyPayroll.Include(e => e.Company).Include(e => e.Company.Calendar).Include(e => e.Company.Calendar.Select(r => r.Name)).Where(e => e.Id == CompId).SingleOrDefault();
                                                                var OFinancia = OCompanyPayroll.Company.Calendar.Where(r => r.Default == true && r.Name.LookupVal.ToString().ToUpper() == "FINANCIALYEAR").SingleOrDefault();

                                                                DateTime FromPeriod = Convert.ToDateTime(OFinancia.FromDate);
                                                                DateTime ToPeriod = Convert.ToDateTime(OFinancia.ToDate);
                                                                for (DateTime mTempDate = FromPeriod; mTempDate <= ToPeriod; mTempDate = mTempDate.AddMonths(1))
                                                                {
                                                                    string paymon = Convert.ToDateTime(mTempDate).ToString("MM/yyyy");
                                                                    List<PerkTransT> Perktranst = db.PerkTransT
                                                                          .Include(q => q.SalaryHead)
                                                                          .Where(q => q.PayMonth == paymon && paysc.SalaryHead.Id == chksalhead)
                                                                          .ToList();

                                                                    db.PerkTransT.RemoveRange(Perktranst);
                                                                    db.SaveChanges();


                                                                }

                                                            }
                                                        }
                                                    }
                                                }
                                            }


                                        }
                                    }

                                    string rowno = Convert.ToString(counter);
                                    int checkifpresent = LoanAdvReqRep(Loc_Code.Trim(), Loan_head.Trim(), MonthlyIntrest, Monthly_principleAmt, InstallmentPaid, PaymonthforLoanadv, Acc_No, rowno,
                                        ActualPerkAmt, ProjPerkAmt, BankIntrest, SIBIntrest, salheadList, MonthlyPrinciple, MonthlyInterest, AnneigtyCPriPro, AnneigtyCPriAct, AnnintdedPro, AnnintdedAct);
                                    oldlNcode = Loan_head.Trim();
                                    counter++;
                                    if (checkifpresent == 1)
                                    {
                                        ExcelformatEntity excSingle = new ExcelformatEntity()
                                        {
                                            Column0 = Loc_Code.Trim(),
                                            Column1 = Loan_head.Trim(),
                                            Column2 = Acc_No,
                                            Column3 = MonthlyIntrest.ToString(),
                                            Column4 = InstallmentPaid.ToString(),
                                            Column5 = Monthly_principleAmt.ToString(),
                                            Column6 = PaymonthforLoanadv,
                                            //7 is for emp
                                            Column8 = Total_LoanSanct_appliedamt.ToString(),
                                            Column9 = "True"
                                        };
                                        excList.Add(excSingle);
                                    }
                                    //}
                                }
                            }
                        }
                        var jsona = JsonConvert.SerializeObject(excList, Formatting.Indented);
                        if (jsona != null)
                        {
                            return Json(new { json = jsona, col = ListCol1 }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    catch (Exception ex)
                    {

                        //  throw;
                        LogFile Logfile = new LogFile();
                        ErrorLog Err = new ErrorLog()
                        {
                            ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                            ExceptionMessage = ex.Message,
                            ExceptionStackTrace = ex.StackTrace,
                            LineNo = Convert.ToString(new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                            LogTime = DateTime.Now
                        };
                        Logfile.CreateLogFile(Err);
                        Msg.Add(ex.Message);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }



                    // Excel file after convert in notepad



                }

                if (filename == "OtherEarningT")
                {
                    filename = "OthEarningDeductionT";
                }
                if (filename == "ITInvestmentPayment80C" || filename == "ITINVESTMENTPAYMENTSECTION80DTO80U" || filename == "ITINVESTMENTPAYMENT80CCCTO80CCF")
                {
                    filename = "ITInvestmentPayment";
                }
                oAssembly = Assembly.Load("Core");
                type = oAssembly.GetType("P2b.Global." + filename);
                if (type == null)
                {
                    oAssembly = Assembly.Load("Leave");
                    type = oAssembly.GetType("Leave." + filename);
                }
                if (type == null)
                {
                    oAssembly = Assembly.Load("Payroll");
                    type = oAssembly.GetType("Payroll." + filename);
                }
                if (type == null)
                {
                    oAssembly = Assembly.Load("Training");
                    type = oAssembly.GetType("Training." + filename);
                }
                if (type == null)
                {
                    oAssembly = Assembly.Load("P2B.PFTRUST");
                    type = oAssembly.GetType("P2B.PFTRUST." + filename);
                }
                if (type == null)
                {
                    return Json(new { responseText = "Invalid excel..!" }, JsonRequestBehavior.AllowGet);

                }
                Object instance = Activator.CreateInstance(type);
                var Prop = instance.GetType().GetProperties().ToList();
                List<string> PropList = new List<string>();
                foreach (var item in Prop)
                {
                    if (item.Name != "DBTrack" && item.Name != "Id" && item.Name != "RowVersion")
                    {
                        PropList.Add(item.Name);
                    }
                }

                var ListCol = AddtionalCol(filename, fileExtension, PropList);
                ListCol.Sort();
                ExcelDataReader.IExcelDataReader reader = null;

                if (upload.FileName.EndsWith(".xls"))
                {
                    reader = ExcelReaderFactory.CreateBinaryReader(stream);
                }
                else if (upload.FileName.EndsWith(".xlsx"))
                {
                    reader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                }
                else if (upload.FileName.EndsWith(".txt"))
                {

                    try
                    {
                        List<DataDump> testing = new List<DataDump>();
                        List<ExcelformatEntity> excList = new List<ExcelformatEntity>();

                        ExcelformatEntity exc = new ExcelformatEntity()
                        {
                            Column0 = "LoanAccBranch",
                            Column1 = "LoanAdvHead",
                            Column2 = "LoanAccountNo",
                            Column3 = "MonthlyIntrest",
                            Column4 = "MonthlyInstallmentAmount",
                            Column5 = "MonthlyPrincipleAmt",
                            Column6 = "EnforcementDate",
                            Column7 = "EmpCode",
                            Column8 = "LoanSanctionAmount",
                            Column9 = "IsActive",
                        };
                        excList.Add(exc);
                        //}

                        if (Type != null)
                        {
                            if (PaymonthforLoanadv == null)
                            {
                                return Json(new { responseText = "Select Paymonth..!" }, JsonRequestBehavior.AllowGet);
                            }

                            // List<string> salhead = new List<string>();
                            List<SalHeadFormula> salheadList = new List<SalHeadFormula>();
                            using (DataBaseContext db = new DataBaseContext())
                            {
                                testing = db.DataDump.Where(q => q.TableName == "LoanAdvRepaymentT").ToList();

                                var PayScaleAssignmentData = db.PayScaleAssignment
                                     .Include(q => q.SalaryHead)
                                     .Include(q => q.SalHeadFormula)
                                     .Include(q => q.SalHeadFormula.Select(a => a.SalWages))
                                     .Include(q => q.SalHeadFormula.Select(a => a.SalWages.RateMaster))
                                     .Include(q => q.SalHeadFormula.Select(a => a.SalWages.RateMaster.Select(lm => lm.SalHead)))
                                     .Where(r => r.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "PERK")
                                   .Select(z => z.SalaryHead.Code)
                                       .ToList();


                                salheadList = db.SalHeadFormula
                                    .Include(q => q.SalWages)
                                    .Include(q => q.SalWages.RateMaster)
                                    .Include(q => q.SalWages.RateMaster.Select(a => a.SalHead)).ToList();

                                List<PerkTransT> Perktranst = db.PerkTransT
                                      .Include(q => q.SalaryHead)
                                      .Where(q => q.PayMonth == PaymonthforLoanadv && PayScaleAssignmentData.Contains(q.SalaryHead.Code))
                                      .ToList();

                                db.PerkTransT.RemoveRange(Perktranst);
                                db.SaveChanges();

                            }

                            using (var streamReader = new StreamReader(stream, Encoding.UTF8))
                            {
                                string line;
                                while ((line = streamReader.ReadLine()) != null)
                                {
                                    int counter = 0;
                                    //do not delete
                                    #region test
                                    //foreach (var item in testing)
                                    //{

                                    //midline()
                                    //       int PositionNo= Convert.ToInt16(item.PositionNo);
                                    //       int PositionNoLength= Convert.ToInt16(item.PositionNoLength);
                                    //ExcelformatEntity exc = new ExcelformatEntity();
                                    //      exc.= line.Substring(PositionNoLength - 1, PositionNoLength);

                                    // ExcelformatEntity exc = new ExcelformatEntity();

                                    // string sampleNum = "s" + (counter++).ToString("0");
                                    // exc.
                                    //sampleNum = line.Substring(Convert.ToInt16(item.PositionNo) - 1, Convert.ToInt16(item.PositionNoLength));


                                    // exc.Column1 = line.Substring(Convert.ToInt16(testing[1].PositionNo), Convert.ToInt16(testing[1].PositionNo));

                                    //  exc.Column0 = line.Substring(Convert.ToInt16(testing[0].PositionNo), Convert.ToInt16(testing[0].PositionNoLength));
                                    //exc.Column1 = line.Substring(Convert.ToInt16(testing[1].PositionNo), Convert.ToInt16(testing[1].PositionNoLength));
                                    //exc.Column2 = line.Substring(Convert.ToInt16(testing[2].PositionNo), Convert.ToInt16(testing[2].PositionNoLength));
                                    //exc.Column5 = line.Substring(Convert.ToInt16(testing[5].PositionNo), Convert.ToInt16(testing[5].PositionNoLength));
                                    //exc.Column9 = line.Substring(Convert.ToInt16(testing[9].PositionNo), Convert.ToInt16(testing[9].PositionNoLength));
                                    //exc.Column13 = line.Substring(Convert.ToInt16(testing[13].PositionNo), Convert.ToInt16(testing[13].PositionNoLength));

                                    //exc.Column2 = testing[2].TableName;
                                    //exc.Column3 = testing[3].TableName;
                                    //exc.Column4 = testing[4].TableName;
                                    //exc.Column5 = testing[5].TableName;
                                    //exc.Column6 = testing[6].TableName;
                                    //exc.Column7 = testing[7].TableName;
                                    //exc.Column8 = testing[8].TableName;
                                    //exc.Column9 = testing[9].TableName;
                                    //exc.Column10 = testing[10].TableName;
                                    //exc.Column11 = testing[11].TableName;
                                    //exc.Column12 = testing[12].TableName;
                                    //exc.Column13 = testing[13].TableName;
                                    //exc.Column14 = testing[14].TableName;
                                    //exc.Column15 = testing[15].TableName;
                                    //exc.Column16 = testing[16].TableName;
                                    //exc.Column17 = testing[17].TableName;
                                    #endregion test
                                    //do not delete

                                    string Loc_Code = line.Substring(Convert.ToInt16(testing[0].PositionNo) - 1, Convert.ToInt16(testing[0].PositionNoLength)).Trim();
                                    string Loan_head = line.Substring(Convert.ToInt16(testing[1].PositionNo) - 1, Convert.ToInt16(testing[1].PositionNoLength)).Trim();
                                    string Acc_No = line.Substring(Convert.ToInt16(testing[2].PositionNo) - 1, Convert.ToInt16(testing[2].PositionNoLength)).Trim();

                                    string ChkData = line.Substring(Convert.ToInt16(testing[5].PositionNo) - 1, Convert.ToInt16(testing[5].PositionNoLength)).Trim();
                                    double MonthlyIntrest = ChkData == "" ? 0 : Convert.ToDouble(ChkData);

                                    ChkData = line.Substring(Convert.ToInt16(testing[12].PositionNo) - 1, Convert.ToInt16(testing[12].PositionNoLength)).Trim();
                                    double InstallmentPaid = ChkData == "" ? 0 : Convert.ToDouble(ChkData);

                                    ChkData = line.Substring(Convert.ToInt16(testing[10].PositionNo) - 1, Convert.ToInt16(testing[10].PositionNoLength)).Trim();
                                    double Total_LoanSanct_appliedamt = ChkData == "" ? 0 : Convert.ToDouble(ChkData);

                                    ChkData = line.Substring(Convert.ToInt16(testing[13].PositionNo) - 1, Convert.ToInt16(testing[13].PositionNoLength)).Trim();
                                    double Monthly_principleAmt = ChkData == "" ? 0 : Convert.ToDouble(ChkData);

                                    ChkData = line.Substring(112, 18).Trim();
                                    double ActualPerkAmt = ChkData == "" ? 0 : Convert.ToDouble(ChkData);

                                    ChkData = line.Substring(94, 18).Trim();
                                    double ProjPerkAmt = ChkData == "" ? 0 : Convert.ToDouble(ChkData);

                                    ChkData = line.Substring(147, 7).Trim();
                                    double BankIntrest = ChkData == "" ? 0 : Convert.ToDouble(ChkData);

                                    ChkData = line.Substring(154, 7).Trim();
                                    double SIBIntrest = ChkData == "" ? 0 : Convert.ToDouble(ChkData);

                                    ChkData = line.Substring(217, 18).Trim();
                                    double MonthlyPrinciple = ChkData == "" ? 0 : Convert.ToDouble(ChkData);

                                    ChkData = line.Substring(235, 18).Trim();
                                    double MonthlyInterest = ChkData == "" ? 0 : Convert.ToDouble(ChkData);

                                    ChkData = line.Substring(24, 18).Trim();
                                    double AnneigtyCPriPro = ChkData == "" ? 0 : Convert.ToDouble(ChkData);

                                    ChkData = line.Substring(42, 18).Trim();
                                    double AnneigtyCPriAct = ChkData == "" ? 0 : Convert.ToDouble(ChkData);

                                    ChkData = line.Substring(60, 18).Trim();
                                    double AnnintdedPro = ChkData == "" ? 0 : Convert.ToDouble(ChkData);

                                    ChkData = line.Substring(78, 18).Trim();
                                    double AnnintdedAct = ChkData == "" ? 0 : Convert.ToDouble(ChkData);


                                    string rowno = Convert.ToString(counter);
                                    int checkifpresent = LoanAdvReqRep(Loc_Code.Trim(), Loan_head.Trim(), MonthlyIntrest, Monthly_principleAmt, InstallmentPaid, PaymonthforLoanadv, Acc_No, rowno,
                                        ActualPerkAmt, ProjPerkAmt, BankIntrest, SIBIntrest, salheadList, MonthlyPrinciple, MonthlyInterest, AnneigtyCPriPro, AnneigtyCPriAct, AnnintdedPro, AnnintdedAct);

                                    counter++;
                                    if (checkifpresent == 1)
                                    {
                                        ExcelformatEntity excSingle = new ExcelformatEntity()
                                        {
                                            Column0 = Loc_Code.Trim(),
                                            Column1 = Loan_head.Trim(),
                                            Column2 = Acc_No,
                                            Column3 = MonthlyIntrest.ToString(),
                                            Column4 = InstallmentPaid.ToString(),
                                            Column5 = Monthly_principleAmt.ToString(),
                                            Column6 = PaymonthforLoanadv,
                                            //7 is for emp
                                            Column8 = Total_LoanSanct_appliedamt.ToString(),
                                            Column9 = "True"
                                        };
                                        excList.Add(excSingle);
                                    }
                                    //}
                                }
                            }
                        }
                        var jsona = JsonConvert.SerializeObject(excList, Formatting.Indented);
                        if (jsona != null)
                        {
                            return Json(new { json = jsona, col = ListCol }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    catch (Exception ex)
                    {

                        throw;
                    }

                }
                else
                {
                    return Json(new { responseText = "Invalid Excel..!" }, JsonRequestBehavior.AllowGet);
                }
                DataSet result = reader.AsDataSet();
                reader.Close();
                var json = JsonConvert.SerializeObject(result.Tables[0], Formatting.Indented);

                //  return Json(new { json = json, col = ListCol }, JsonRequestBehavior.AllowGet);
                var jsonResult = Json(new { json = json, col = ListCol }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
            else
            {
                return Json(new { responseText = "Empty Excel..!" }, JsonRequestBehavior.AllowGet);
            }

        }
        public class returnCsvClass
        {
            public string line { get; set; }
        }
        public static CompanyPayroll _returnCompanyPayroll_IncomeTax(Int32 mCompanyId)
        {
            //Utility.DumpProcessStatus("_returnCompanyPayroll_IncomeTax");
            using (DataBaseContext db = new DataBaseContext())
            {
                return db.CompanyPayroll
               .Include(e => e.IncomeTax)
               .Include(e => e.IncomeTax.Select(r => r.FyCalendar))
               .Include(e => e.IncomeTax.Select(r => r.ITSection))
               .Include(e => e.IncomeTax.Select(r => r.ITSection.Select(s => s.ITInvestments)))
               .Include(e => e.IncomeTax.Select(r => r.ITSection.Select(s => s.ITInvestments.Select(m => m.ITSubInvestment))))
               .Include(e => e.IncomeTax.Select(r => r.ITSection.Select(s => s.ITInvestments.Select(m => m.SalaryHead))))
               .Include(e => e.IncomeTax.Select(r => r.ITSection.Select(s => s.LoanAdvanceHead)))
               .Include(e => e.IncomeTax.Select(r => r.ITSection.Select(s => s.LoanAdvanceHead.Select(d => d.SalaryHead))))
               .Include(e => e.IncomeTax.Select(r => r.ITSection.Select(s => s.LoanAdvanceHead.Select(d => d.ITLoan))))
               .Include(e => e.IncomeTax.Select(r => r.ITSection.Select(d => d.ITStandardITRebate)))
               .Include(e => e.IncomeTax.Select(r => r.ITSection.Select(d => d.ITSection10)))
               .Include(e => e.IncomeTax.Select(r => r.ITSection.Select(d => d.ITSection10.Select(s => s.Itsection10salhead))))
               .Include(e => e.IncomeTax.Select(r => r.ITSection.Select(d => d.ITSection10.Select(s => s.Itsection10salhead.Select(c => c.Frequency)))))
               .Include(e => e.IncomeTax.Select(r => r.ITSection.Select(d => d.ITSection10.Select(s => s.Itsection10salhead.Select(t => t.SalHead)))))
               .Include(e => e.IncomeTax.Select(r => r.ITSection.Select(d => d.ITSection10.Select(s => s.Itsection10salhead.Select(t => t.SalHead.SalHeadOperationType)))))
               .Include(e => e.IncomeTax.Select(r => r.ITSection.Select(d => d.ITSectionList)))
               .Include(e => e.IncomeTax.Select(r => r.ITSection.Select(d => d.ITSectionListType)))
               .Include(e => e.IncomeTax.Select(r => r.ITTDS))
               .Include(e => e.IncomeTax.Select(r => r.ITTDS.Select(d => d.Category))).Where(e => e.Company.Id == mCompanyId)//.AsParallel()
                    //.SingleOrDefault(); 
               .FirstOrDefault();
                //return a.Where(e => e.Company.Id == mCompanyId).AsParallel()
                //   .SingleOrDefault();
            }
        }

        public int LoanAdvReqRep(string LocCode, string LoanAdvHead, Double LMonthlyInterest, Double LMonthlyPricipalAmount, Double LInstallmentPaid, string LoanPayMonth,
            string LLoanAccNo, string rowno, Double ActualPerkAmt, Double ProjPerkAmt, Double BankIntrest, Double SBIIntrest, List<SalHeadFormula> salheadList, Double MonthlyPrinciple, Double MonthlyInterest, Double AnneigtyCPriPro, Double AnneigtyCPriAct, Double AnnintdedPro, Double AnnintdedAct)
        {
            try
            {


                if (LoanAdvHead == null || LoanPayMonth == null || LMonthlyInterest == null || LMonthlyPricipalAmount == null || LInstallmentPaid == null)
                {
                    return 0;
                    //return Json(new { data = rowno, success = false, responseText = "Column Shouldn't be null..!" }, JsonRequestBehavior.AllowGet);
                }
                using (TransactionScope ts = new TransactionScope())
                {
                    using (DataBaseContext db = new DataBaseContext())
                    {
                        DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };
                        var Id = Convert.ToInt32(SessionManager.CompanyId);
                        string _CompCode = db.Company.Where(e => e.Id == Id).SingleOrDefault().Code.ToUpper();

                        var LoanHeadId = db.LoanAdvanceHead.Include(e => e.PerkHead).Where(a => a.Code == LoanAdvHead).FirstOrDefault();

                        var LoanReq = LoanHeadId != null ? db.EmployeePayroll
                            .Include(a => a.LoanAdvRequest)
                            .Include(a => a.LoanAdvRequest.Select(e => e.LoanAdvanceHead))
                            .Include(a => a.LoanAdvRequest.Select(e => e.LoanAdvRepaymentT))
                            .Include(a => a.LoanAdvRequest.Select(e => e.LoanAccBranch.LocationObj))
                            .Where(a =>
                                //a.Employee.Id == id &&
                                        a.LoanAdvRequest.Any(e =>
                                            e.LoanAccBranch.LocationObj.LocCode == LocCode &&
                                            e.LoanAccNo == LLoanAccNo &&
                                                //e.IsActive == true &&
                                            e.LoanAdvanceHead.Id == LoanHeadId.Id
                                        )).SingleOrDefault() : null;
                        if (LoanReq == null)
                        {
                            return 1;
                            //Json(new { data = rowno, success = false, responseCode = 1, responseText = "Loan Request Not Found..!" }, JsonRequestBehavior.AllowGet);
                        }
                        var LoanRepayment = LoanReq != null ?
                            LoanReq.LoanAdvRequest
                            .Where(a => a.LoanAdvRepaymentT.Any(q => q.PayMonth == LoanPayMonth)
                                && a.LoanAccNo == LLoanAccNo
                                && a.LoanAccBranch.LocationObj.LocCode == LocCode
                                && a.LoanAdvanceHead.Id == LoanHeadId.Id
                                && a.IsActive == true
                            ).SingleOrDefault() : null;
                        if (LoanRepayment != null)
                        {


                            var LoanAdvRepaymentT_temp = LoanRepayment.LoanAdvRepaymentT.Where(a => a.PayMonth == LoanPayMonth).FirstOrDefault();

                            LoanAdvRepaymentT_temp.PayMonth = LoanPayMonth;
                            LoanAdvRepaymentT_temp.MonthlyInterest = Convert.ToDouble(MonthlyInterest);
                            LoanAdvRepaymentT_temp.MonthlyPricipalAmount = Convert.ToDouble(MonthlyPrinciple);
                            LoanAdvRepaymentT_temp.InstallmentPaid = Convert.ToDouble(LInstallmentPaid);
                            LoanAdvRepaymentT_temp.InstallmentAmount = Convert.ToDouble(LInstallmentPaid);

                            db.Entry(LoanAdvRepaymentT_temp).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();

                            //emp
                            //sec 80 C update
                            int Flag = 1;
                            EmployeePayroll OEmployeePayroll = db.EmployeePayroll
                                        .Where(a => a.Id == LoanReq.Id).SingleOrDefault();

                            int OFinancialYear = db.Calendar.Where(e => e.Name.LookupVal.ToUpper() == "FINANCIALYEAR" && e.Default == true).FirstOrDefault().Id;
                            CompanyPayroll OIncomeTax = IncomeTaxCalc._returnCompanyPayroll_IncomeTax_New(Id, OFinancialYear);
                            IncomeTax OITMaster = OIncomeTax.IncomeTax.Where(e => e.FyCalendar.Id == OFinancialYear).SingleOrDefault();
                            ITSection OITSection80C = OITMaster.ITSection.Where(e => e.ITSectionList.LookupVal.ToUpper() == "SECTION80C" &&
                                                e.ITSectionListType.LookupVal.ToUpper() == "REBATE").SingleOrDefault(); //rebate

                            List<ITInvestment> OITInvestments = OITSection80C.ITInvestments.ToList();
                            List<LoanAdvanceHead> OITInvestmentsLoan = OITSection80C.LoanAdvanceHead.Where(e => e.Id == LoanHeadId.Id).ToList();
                            List<ITSubInvestment> OITSubInvestments = OITInvestments.SelectMany(e => e.ITSubInvestment).ToList();
                            IList<ITInvestmentPayment> OEmpITInvestment = null;
                            EmployeePayroll OEmpInvest = db.EmployeePayroll.Where(e => e.Id == OEmployeePayroll.Id)
                                            .Include(e => e.ITInvestmentPayment)
                                            .Include(e => e.ITInvestmentPayment.Select(r => r.FinancialYear))
                                            .Include(e => e.ITInvestmentPayment.Select(r => r.ITSection))
                                            .Include(e => e.ITInvestmentPayment.Select(r => r.ITSection.ITSectionList))
                                            .Include(e => e.ITInvestmentPayment.Select(r => r.ITSection.ITSectionListType))
                                            .Include(e => e.ITInvestmentPayment.Select(r => r.ITSection.ITSectionList))//added 17042017 by prashant
                                            .Include(e => e.ITInvestmentPayment.Select(r => r.ITSubInvestmentPayment))
                                            .Include(e => e.ITInvestmentPayment.Select(r => r.LoanAdvanceHead))
                                            .Include(e => e.ITInvestmentPayment.Select(r => r.LoanAdvanceHead.ITLoan))
                                            .Include(e => e.ITInvestmentPayment.Select(r => r.LoanAdvanceHead.ITSection))
                                            .Include(e => e.ITInvestmentPayment.Select(r => r.LoanAdvanceHead.LoanAdvancePolicy))
                                            .Include(e => e.ITInvestmentPayment.Select(r => r.LoanAdvanceHead.SalaryHead))
                                            .Include(e => e.ITInvestmentPayment.Select(r => r.ITInvestment))
                                            .Include(e => e.ITInvestmentPayment.Select(r => r.ITInvestment.SalaryHead))
                                .SingleOrDefault();
                            if (OEmpInvest.ITInvestmentPayment != null)
                            {
                                OEmpITInvestment = OEmpInvest.ITInvestmentPayment.Where(e => e.FinancialYear.Id == OFinancialYear && e.ITSection.ITSectionListType.LookupVal.ToUpper() == "REBATE").ToList();
                                List<ITSubInvestmentPayment> OEmpITSubInvestment = OEmpInvest.ITInvestmentPayment
                                    .Where(e => e.FinancialYear.Id == OFinancialYear && e.ITSection.ITSectionListType.LookupVal.ToUpper() == "REBATE")
                                    .SelectMany(e => e.ITSubInvestmentPayment).ToList();
                            }

                            Calendar temp_OFinancialYear = db.Calendar.Where(e => e.Id == OFinancialYear).SingleOrDefault();
                            //var OITInvestmentsSalLoan = OITInvestmentsLoan.Where(e => e.SalaryHead != null && e.ITLoan != null && (e.ITLoan.PrincAppl == true ||
                            //                  e.ITLoan.IntPrincAppl == true)).Select(e => new { Id = e.Id, SalaryHead_Id = e.SalaryHead.Id }).ToList();

                            var OITInvestmentsSalLoan = OITInvestmentsLoan.Where(e => e.SalaryHead != null && e.ITLoan == null).Select(e => new { Id = e.Id, SalaryHead_Id = e.SalaryHead.Id }).ToList();

                            EmployeePayroll OEmpPay = db.EmployeePayroll.Where(e => e.Id == OEmployeePayroll.Id).Include(e => e.LoanAdvRequest.Select(r => r.LoanAdvRepaymentT))
                                .Include(e => e.LoanAdvRequest).Include(e => e.LoanAdvRequest.Select(r => r.LoanAdvanceHead)).SingleOrDefault();

                            if (OITInvestmentsSalLoan != null && OITInvestmentsSalLoan.Count() > 0)
                            {
                                List<ITInvestmentPayment> mList = new List<ITInvestmentPayment>();
                                foreach (var ca in OITInvestmentsSalLoan)
                                {
                                    List<LoanAdvRequest> OLoanAdvReqF = OEmpPay.LoanAdvRequest.Where(e => e.LoanAdvanceHead.Id == ca.Id).ToList();

                                    if (OLoanAdvReqF.Count() > 0)
                                    {
                                        //modify investment record
                                        //    var OEmpITInvestmentTemp1 = OEmpITInvestment.Where(e => e.LoanAdvanceHead != null).ToList();
                                        //   var OEmpITInvestmentTemp = OEmpITInvestment.Where(e => e.LoanAdvanceHead != null && e.LoanAdvanceHead.SalaryHead != null).ToList();
                                        ITInvestmentPayment OEmpSalInvestmentChk = OEmpITInvestment.Where(e => e.LoanAdvanceHead != null && e.LoanAdvanceHead.SalaryHead != null &&
                                            e.LoanAdvanceHead.SalaryHead.Id == ca.SalaryHead_Id).SingleOrDefault();
                                        if (OEmpSalInvestmentChk != null)
                                        {
                                            ITInvestmentPayment OEmpSalInvestmentObj = db.ITInvestmentPayment.Where(a => a.Id == OEmpSalInvestmentChk.Id).SingleOrDefault();
                                            OEmpSalInvestmentObj.ActualInvestment = AnneigtyCPriAct;
                                            OEmpSalInvestmentObj.DeclaredInvestment = AnneigtyCPriPro;
                                            db.Entry(OEmpSalInvestmentObj).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();
                                        }
                                        else
                                        {
                                            ITInvestmentPayment OEmpITInvestmentSave = new ITInvestmentPayment()
                                            {
                                                FinancialYear = db.Calendar.Where(e => e.Id == OFinancialYear).SingleOrDefault(),
                                                LoanAdvanceHead = db.LoanAdvanceHead.Where(a => a.Id == ca.Id).SingleOrDefault(),
                                                InvestmentDate = DateTime.Now.Date,
                                                ITSection = db.ITSection.Include(q => q.ITSectionListType).Include(q => q.ITSectionList).Include(q => q.ITSection10).Include(q => q.LoanAdvanceHead).Where(a => a.Id == OITSection80C.Id).SingleOrDefault(),
                                                ITSubInvestmentPayment = null,
                                                ActualInvestment = AnneigtyCPriAct,
                                                DeclaredInvestment = AnneigtyCPriPro,
                                                Narration = "Investment under Section 80C Loan through Salary",
                                                DBTrack = dbt,
                                            };

                                            if (Flag == 1)
                                            {
                                                db.ITInvestmentPayment.Add(OEmpITInvestmentSave);
                                                db.SaveChanges();
                                            }

                                            mList.Add(OEmpITInvestmentSave);
                                            OEmpITInvestment.Add(OEmpITInvestmentSave);

                                        }
                                    }




                                }
                                if (mList.Count() > 0 && Flag == 1)
                                {
                                    int OEmpId = OEmployeePayroll.Id;
                                    List<ITInvestmentPayment> OFAT = new List<ITInvestmentPayment>();
                                    //EmployeePayroll aa = db.EmployeePayroll.Include(e => e.ITInvestmentPayment).Where(a => a.Id == OEmpId).SingleOrDefault();

                                    EmployeePayroll aa = db.EmployeePayroll.Find(OEmployeePayroll.Id);
                                    if (aa.ITInvestmentPayment != null)
                                    { mList.AddRange(aa.ITInvestmentPayment); }

                                    aa.ITInvestmentPayment = mList;
                                    //OEmployeePayroll.DBTrack = dbt;
                                    db.EmployeePayroll.Attach(aa);
                                    db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    //db.Entry(aa).State = System.Data.Entity.EntityState.Detached;

                                }

                            }

                            //sec 24 interest update start

                            EmployeePayroll OEmpITSection24EmpData = db.EmployeePayroll
                .Include(e => e.ITSection24Payment)
                  .Include(e => e.ITSection24Payment.Select(r => r.FinancialYear))
                  .Include(e => e.ITSection24Payment.Select(r => r.ITSection))
                  .Include(e => e.ITSection24Payment.Select(r => r.ITSection.ITSectionList))
                  .Include(e => e.ITSection24Payment.Select(r => r.ITSection.ITSectionListType))
                  .Include(e => e.ITSection24Payment.Select(r => r.ITSection.ITSectionListType))

                  .Include(e => e.ITSection24Payment.Select(r => r.ITSection.LoanAdvanceHead))
                  .Include(e => e.ITSection24Payment.Select(r => r.LoanAdvanceHead))
                  .Include(e => e.ITSection24Payment.Select(r => r.LoanAdvanceHead.SalaryHead))
                  .Include(e => e.LoanAdvRequest)
                  .Include(e => e.LoanAdvRequest.Select(r => r.LoanAdvanceHead))
                  .Include(e => e.LoanAdvRequest.Select(r => r.LoanAdvanceHead.ITLoan))
                  .Include(e => e.LoanAdvRequest.Select(r => r.LoanAdvanceHead.SalaryHead))
                  .Include(e => e.LoanAdvRequest.Select(r => r.LoanAdvRepaymentT))
                  .Where(e => e.Id == OEmployeePayroll.Id)
                  .SingleOrDefault();

                            List<ITSection> OITSection24 = OITMaster.ITSection.Where(e => e.ITSectionList.LookupVal.ToUpper() == "SECTION24" && e.ITSectionListType.LookupVal.ToUpper() == "LOAN").ToList();


                            //  OITInvestmentsList = OITSection24.SelectMany(e => e.LoanAdvanceHead.Where(a => a.SalaryHead != null && (a.ITLoan.IntAppl == true || a.ITLoan.IntPrincAppl == true))).ToList();
                            List<LoanAdvanceHead> OITInvestmentsList = OITSection24.SelectMany(e => e.LoanAdvanceHead.Where(a => a.SalaryHead != null && a.Id == LoanHeadId.Id && a.ITLoan == null)).ToList();

                            List<ITSection24Payment> OEmpIT24Investment = OEmpITSection24EmpData.ITSection24Payment.Where(e => e.FinancialYear.Id == OFinancialYear && e.ITSection.ITSectionListType.LookupVal.ToUpper() == "LOAN" && e.LoanAdvanceHead.Id == LoanHeadId.Id).ToList();

                            foreach (LoanAdvanceHead OITInvestments1 in OITInvestmentsList)
                            {

                                List<ITSection24Payment> mList = new List<ITSection24Payment>();

                                ITSection24Payment OEmpSalInvestmentChk = OEmpIT24Investment.Where(e =>
                                                       e.LoanAdvanceHead != null &&
                                                       e.LoanAdvanceHead.SalaryHead != null &&
                                                       e.LoanAdvanceHead.SalaryHead.Id == OITInvestments1.SalaryHead.Id).SingleOrDefault();
                                if (OEmpSalInvestmentChk != null)
                                {
                                    ITSection24Payment OEmpSalInvestmentObj = db.ITSection24Payment.Where(a => a.Id == OEmpSalInvestmentChk.Id).SingleOrDefault();
                                    OEmpSalInvestmentObj.ActualInterest = AnnintdedAct;
                                    OEmpSalInvestmentObj.DeclaredInterest = AnnintdedPro;
                                    if (Flag == 1)
                                    {
                                        db.Entry(OEmpSalInvestmentObj).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                    }
                                    //mList.Add(OEmpSalInvestmentObj);

                                }
                                else
                                {

                                    ITSection OITSectiondata = OITSection24.Where(e => e.LoanAdvanceHead.Contains(OITInvestments1)).SingleOrDefault();

                                    ITSection24Payment OEmpITInvestmentSave = new ITSection24Payment()
                                    {
                                        FinancialYear = db.Calendar.Where(e => e.Id == OFinancialYear).SingleOrDefault(),
                                        LoanAdvanceHead = db.LoanAdvanceHead.Include(q => q.SalaryHead).Where(a => a.Id == OITInvestments1.Id).SingleOrDefault(),
                                        InvestmentDate = DateTime.Now.Date,
                                        ITSection = db.ITSection.Include(q => q.LoanAdvanceHead.Select(a => a.SalaryHead)).Where(a => a.Id == OITSectiondata.Id).SingleOrDefault(),
                                        PaymentName = OITInvestments1.Name,
                                        ActualInterest = AnnintdedAct,
                                        DeclaredInterest = AnnintdedPro,
                                        //ActualInterest = 0,
                                        //DeclaredInterest = 0,
                                        Narration = "Investment under Section 24 through Salary",
                                        DBTrack = dbt,
                                        SalaryApp = true

                                    };

                                    if (Flag == 1)
                                    {
                                        db.ITSection24Payment.Add(OEmpITInvestmentSave);
                                        db.SaveChanges();
                                    }

                                    mList.Add(OEmpITInvestmentSave);
                                    OEmpIT24Investment.Add(OEmpITInvestmentSave);

                                }

                                // }
                                if (mList.Count() > 0 && Flag == 1)
                                {
                                    int OEmpId = OEmployeePayroll.Id;

                                    EmployeePayroll aa = db.EmployeePayroll.Where(a => a.Id == OEmpId).SingleOrDefault();
                                    if (aa.ITSection24Payment != null)
                                    { mList.AddRange(aa.ITSection24Payment); }
                                    aa.ITSection24Payment = mList;
                                    //OEmployeePayroll.DBTrack = dbt;
                                    db.EmployeePayroll.Attach(aa);
                                    db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    //db.Entry(aa).State = System.Data.Entity.EntityState.Detached;

                                }
                            }

                            //sec 24 interest update end

                            var sddd = LoanHeadId.PerkHead.Id;
                            if (sddd != null)
                            {

                                var paysc = db.PayScaleAssignment
                                    .Include(q => q.SalaryHead)
                                     .Include(q => q.SalaryHead.Frequency)
                                    .Where(qa => qa.SalaryHead.Id == sddd)
                                        .SingleOrDefault();

                                if (paysc != null)
                                {
                                    var chksalhead = paysc.SalaryHead;

                                    EmployeePayroll empP = db.EmployeePayroll
                                          .Include(q => q.PerkTransT)
                                          .Include(q => q.PerkTransT.Select(qa => qa.SalaryHead))
                                         .Where(a => a.Id == LoanReq.Id).SingleOrDefault();

                                    var PerktranstData = empP.PerkTransT
                                                   .Where(q => q.PayMonth == LoanPayMonth && q.SalaryHead.Id == chksalhead.Id)
                                                   .SingleOrDefault();
                                    double ActualAmt = 0;
                                    double ProjectedAmt = 0;
                                    if (paysc.SalaryHead.Frequency.LookupVal.ToUpper() == "MONTHLY")
                                    {
                                        ActualAmt = ((MonthlyInterest / BankIntrest) * SBIIntrest - MonthlyInterest);
                                        ProjectedAmt = ((MonthlyInterest / BankIntrest) * SBIIntrest - MonthlyInterest);


                                    }
                                    if (paysc.SalaryHead.Frequency.LookupVal.ToUpper() == "YEARLY")
                                    {
                                        ActualAmt = ((ActualPerkAmt / BankIntrest) * SBIIntrest - ActualPerkAmt);
                                        ProjectedAmt = ((ProjPerkAmt / BankIntrest) * SBIIntrest - ProjPerkAmt);
                                    }

                                    ActualAmt = Math.Round(ActualAmt, 0);
                                    ProjectedAmt = Math.Round(ProjectedAmt, 0);

                                    if (PerktranstData == null)
                                    {
                                        List<PerkTransT> PerkTransTEmp = new List<PerkTransT>();
                                        PerkTransT PerkTrans = new PerkTransT()
                                        {
                                            ActualAmount = ActualAmt,
                                            ProjectedAmount = ProjectedAmt,
                                            PayMonth = LoanPayMonth,
                                            SalaryHead = chksalhead,
                                            DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                                        };
                                        db.PerkTransT.Add(PerkTrans);
                                        db.SaveChanges();
                                        PerkTransTEmp.Add(PerkTrans);

                                        var aa = db.EmployeePayroll.Find(LoanReq.Id);
                                        PerkTransTEmp.AddRange(aa.PerkTransT);
                                        aa.PerkTransT = PerkTransTEmp;
                                        //OEmployeePayroll.DBTrack = dbt;

                                        db.EmployeePayroll.Attach(aa);
                                        db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                        db.Entry(aa).State = System.Data.Entity.EntityState.Detached;

                                    }
                                    else
                                    {
                                        PerktranstData.ActualAmount = PerktranstData.ActualAmount + ActualAmt;
                                        PerktranstData.ProjectedAmount = PerktranstData.ProjectedAmount + ProjectedAmt;
                                        PerktranstData.SalaryHead = chksalhead;
                                        PerktranstData.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                        db.Entry(PerktranstData).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                    }

                                    if (paysc.SalaryHead.Frequency.LookupVal.ToUpper() == "MONTHLY")
                                    {
                                        var empsalstr = db.EmpSalStruct.Include(e => e.EmpSalStructDetails)
                                            .Include(e => e.EmpSalStructDetails.Select(eb => eb.SalaryHead))
                                            .Include(e => e.EmpSalStructDetails.Select(ec => ec.SalaryHead.SalHeadOperationType))
                                            .Where(e => e.EndDate == null && e.EmployeePayroll.Id == LoanReq.Id).SingleOrDefault();

                                        int ids = empsalstr.EmpSalStructDetails.Where(q => q.SalaryHead.Id == chksalhead.Id)
                                                       .SingleOrDefault().Id;

                                        List<EmpSalStructDetails> OEmpSalStructDetails = new List<EmpSalStructDetails>();
                                        EmpSalStructDetails OEmpSalStructDetailsObj = new EmpSalStructDetails();
                                        EmpSalStructDetails EmpSalStructDet = db.EmpSalStructDetails
                                                                          .Include(e => e.EmpSalStruct)
                                                                          .Include(e => e.EmpSalStruct.EmployeePayroll)
                                                                          .Include(e => e.SalaryHead)
                                                                          .Include(e => e.SalaryHead.SalHeadOperationType)
                                                                          .Include(e => e.PayScaleAssignment)
                                                                          .Include(e => e.PayScaleAssignment.SalHeadFormula)
                                                                          .Include(e => e.PayScaleAssignment.SalHeadFormula.Select(t => t.FormulaType))
                                                                          .Include(e => e.PayScaleAssignment.SalHeadFormula.Select(t => t.GeoStruct))
                                                                          .Include(e => e.PayScaleAssignment.SalHeadFormula.Select(t => t.PayStruct))
                                                                          .Include(e => e.PayScaleAssignment.SalHeadFormula.Select(t => t.FuncStruct))
                                                                          .Where(e => e.Id == ids).FirstOrDefault();

                                        EmpSalStructDet.DBTrack = new DBTrack
                                        {
                                            CreatedBy = EmpSalStructDet.DBTrack.CreatedBy == null ? null : EmpSalStructDet.DBTrack.CreatedBy,
                                            CreatedOn = EmpSalStructDet.DBTrack.CreatedOn == null ? null : EmpSalStructDet.DBTrack.CreatedOn,
                                            Action = "M",
                                            ModifiedBy = SessionManager.UserName,
                                            ModifiedOn = DateTime.Now
                                        };
                                        EmpSalStructDet.Amount = ActualAmt;
                                        db.EmpSalStructDetails.Attach(EmpSalStructDet);
                                        db.Entry(EmpSalStructDet).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                        db.Entry(EmpSalStructDet).State = System.Data.Entity.EntityState.Detached;

                                    }
                                }
                            }


                            ts.Complete();

                            return 2;
                        }
                        //else
                        //{
                        //    var repay = LoanReq.LoanAdvRequest.SingleOrDefault().LoanAdvRepaymentT;
                        //    oLoanAdvRepaymentT.PayMonth = LoanPayMonth;
                        //    oLoanAdvRepaymentT.MonthlyInterest = Convert.ToDouble(LMonthlyInterest);
                        //    oLoanAdvRepaymentT.MonthlyPricipalAmount = Convert.ToDouble(LMonthlyPricipalAmount);
                        //    oLoanAdvRepaymentT.InstallmentPaid = Convert.ToDouble(LInstallmentPaid);
                        //    oLoanAdvRepaymentT.InstallementDate = Convert.ToDateTime("01/" + LoanPayMonth);
                        //    oLoanAdvRepaymentT.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                        //    db.LoanAdvRepaymentT.Add(oLoanAdvRepaymentT);
                        //    db.SaveChanges();
                        //    repay.Add(oLoanAdvRepaymentT);
                        //    db.Entry(LoanReq).State = System.Data.Entity.EntityState.Modified;
                        //    db.SaveChanges();
                        //    ts.Complete();
                        //    return Json(new { data = rowno, success = true, responseText = "New Repayment Added In Existing Loan Requisition ..!" }, JsonRequestBehavior.AllowGet);
                        //}
                    }
                    return 1;
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        public ActionResult CreateCsvFile(List<returnCsvClass> returnCsvClassList, String ControllerName)
        {
            string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\ExcelError";
            bool exists = System.IO.Directory.Exists(requiredPath);
            string localPath;
            if (!exists)
            {
                localPath = new Uri(requiredPath).LocalPath;
                System.IO.Directory.CreateDirectory(localPath);
            }
            string path = requiredPath + @"\Excel_" + ControllerName + ".txt";
            localPath = new Uri(path).LocalPath;
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }

            FileStream fs = new FileStream(localPath, FileMode.OpenOrCreate);
            StreamWriter str = new StreamWriter(fs);
            str.BaseStream.Seek(0, SeekOrigin.End);
            foreach (var item in returnCsvClassList)
            {
                str.WriteLine(item.line);
            }
            str.Flush();
            str.Close();
            fs.Close();

            return Json(new { });

        }
        public ActionResult GenrateErrorFile(string data, string name)
        {
            var a = data.Split('&');
            var returnCsvClassList = new List<returnCsvClass>();
            foreach (var item in a)
            {
                returnCsvClassList.Add(new returnCsvClass
                {
                    line = item
                });
            }
            if (returnCsvClassList.Count > 0)
            {
                var File = CreateCsvFile(returnCsvClassList, name);
                return File;
            }
            else
            {
                return Json(null);
            }

        }
        public ActionResult DownloadFile(string name)
        {
            name = "Excel_" + name + ".txt";
            string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\ExcelError\\" + name;
            requiredPath = new Uri(requiredPath).LocalPath;

            System.IO.FileInfo file = new System.IO.FileInfo(requiredPath);
            if (file.Exists)
                return File(file.FullName, "text/plain", file.Name);
            else
                return HttpNotFound();
        }
        public List<string> ValidateObj(Object obj)
        {
            var errorList = new List<String>();
            var context = new ValidationContext(obj, serviceProvider: null, items: null);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(obj, context, results, true);
            if (!isValid)
            {
                foreach (var validationResult in results)
                {
                    errorList.Add(validationResult.ErrorMessage);
                }
                return errorList;
            }
            else
            {
                return errorList;
            }
        }
        public List<string> SaveNameOfEmp(string EmpTitle, string FName, string MName, string LName)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                NameSingle oNameSingle = new NameSingle();
                var errorlist = new List<string>();
                if (EmpTitle != null)
                {
                    var title = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "100").FirstOrDefault().LookupValues.Where(e => e.LookupVal == EmpTitle).FirstOrDefault(); //db.LookupValue.Where(e => e.LookupVal == EmpTitle).SingleOrDefault();
                    oNameSingle.EmpTitle = title;
                }
                if (FName != null)
                {
                    oNameSingle.FName = FName;
                }
                if (MName != null)
                {
                    oNameSingle.MName = MName;
                }
                if (LName != null)
                {
                    oNameSingle.LName = LName;
                }
                errorlist = ValidateObj(oNameSingle);
                if (errorlist != null && errorlist.Count > 0)
                {
                    return errorlist;
                }
                else
                {
                    if (!db.NameSingle.Any(e => e.FName == oNameSingle.FName && e.MName == oNameSingle.MName && e.LName == oNameSingle.LName))
                    {
                        try
                        {
                            db.NameSingle.Add(oNameSingle);
                            db.SaveChanges();
                        }
                        catch (Exception e)
                        {
                            errorlist.Add(e.InnerException.Message.ToString());
                        }
                    }
                    return errorlist;
                }
            }
        }
        public ActionResult ExcuteAll(FormCollection form, string ControlName, string PaymonthforLoanadv)
        {

            if (ControlName != "" && ControlName != null)
            {
                ControlName = ControlName.ToUpper();
                if (ControlName == "ITINVESTMENTPAYMENT80C" || ControlName == "ITINVESTMENTPAYMENTSECTION80DTO80U" || ControlName == "ITINVESTMENTPAYMENT80CCCTO80CCF")
                {
                    ControlName = "ITInvestmentPayment".ToUpper();
                }
                if (ControlName == "EMPLOYEEPFTRUST")
                {
                    ControlName = "PFTEMPLOYEELEDGER";
                }
                if (ControlName == "ITSECTION24PAYMENTSELFOCCUPIED" || ControlName == "ITSECTION24PAYMENTLAYOUT")
                {
                    ControlName = "ITSECTION24PAYMENT".ToUpper();
                }
            }
            else
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
           
            var errorList = new List<String>();
            String rowno = form["rowno"] != null ? form["rowno"] : null;
            string EmpSalDel = null;
            bool SalDel = false;
            try
            {
                string sEmpCode = form["EmpCode"] == null ? null : form["EmpCode"];
                List<int> ids = new List<int>();
                if (ControlName != "EMPLOYEE" && ControlName != "LOOKUP" && ControlName != "ITFORM24QFILEFORMATDEFINITION" && ControlName != "LOCATION" && ControlName != "PFTEMPLOYEELEDGER")
                {
                    List<string> EmpCodeList = new List<string>();
                    if (sEmpCode != null && sEmpCode != "")
                    {
                        EmpCodeList = Utility.StringIdsToListString(sEmpCode);
                    }
                    else
                    {
                        return Json(new { success = false, data = rowno, responseText = "Employee Not Found..!" }, JsonRequestBehavior.AllowGet);
                    }

                    using (DataBaseContext db = new DataBaseContext())
                    {
                        foreach (var item in EmpCodeList)
                        {
                            var aa = db.Employee.Where(e => e.EmpCode == item).Select(e => e.Id).FirstOrDefault();
                            if (aa != 0)
                            {
                                ids.Add(aa);
                            }
                        }
                    }
                    if (ids.Count == 0)
                    {
                        return Json(new { success = false, data = rowno, responseText = "Employee Not Exits..!" }, JsonRequestBehavior.AllowGet);

                    }
                }
                switch (ControlName)
                {
                    case "OTHEREARNINGT":
                        using (DataBaseContext db = new DataBaseContext())
                        {
                            OthEarningDeductionT OEDT = new OthEarningDeductionT();
                            string SalaryHeadlist = form["SalaryHead"] == "0" ? "" : form["SalaryHead"];
                            SalaryHeadlist = SalaryHeadlist.Replace("#", " ");
                            String PayMonth = form["PayMonth"] != null ? form["PayMonth"] : null;
                            String SalAmount = form["SalAmount"] != null ? form["SalAmount"] : null;
                            String TDSAmount = form["TDSAmount"] != null ? form["TDSAmount"] : null;
                            String Remark = form["Remark"] != null ? form["Remark"] : null;
                            if (SalAmount != null)
                            {
                                OEDT.SalAmount = Convert.ToDouble(SalAmount);
                            }
                            if (TDSAmount != null)
                            {
                                OEDT.TDSAmount = Convert.ToDouble(TDSAmount);
                            }
                            if (Remark != null)
                            {
                                OEDT.Remark = Convert.ToString(Remark);
                            }
                            else
                            {
                                OEDT.Remark = Convert.ToString(SalaryHeadlist);

                            }
                            if (PayMonth != null)
                            {
                                OEDT.PayMonth = PayMonth;
                            }
                            if (SalAmount != null)
                            {
                                OEDT.SalAmount = Convert.ToDouble(SalAmount);
                            }
                            if (TDSAmount != null)
                            {
                                OEDT.TDSAmount = Convert.ToDouble(TDSAmount);
                            }



                            string EmpCode = null;
                            string EmpRel = null;
                            string EmpCodeRetired = null;

                            List<String> SalHeadCodeList = new List<String>();
                            List<int> SalHead = new List<int>();
                            if (SalaryHeadlist != null && SalaryHeadlist != "")
                            {
                                SalHeadCodeList = Utility.StringIdsToListString(SalaryHeadlist);
                            }
                            foreach (var item in SalHeadCodeList)
                            {
                                var aa = db.SalaryHead.Where(e => e.Code == item).Select(e => e.Id).FirstOrDefault();
                                if (aa != 0)
                                {
                                    SalHead.Add(aa);
                                }
                            }
                            if (ids != null)
                            {

                                foreach (var i in ids)
                                {
                                    Employee OEmployee = null;
                                    EmployeePayroll OEmployeePayroll = null;
                                    OEmployeePayroll = db.EmployeePayroll
                                                   .Include(q => q.Employee)
                                                   .Include(q => q.Employee.ServiceBookDates)
                                                   .Include(e => e.OtherEarningDeductionT)
                                                   .Include(e => e.OtherEarningDeductionT.Select(r => r.SalaryHead))
                                                    .Where(e => e.Employee.Id == i).AsNoTracking().AsParallel().SingleOrDefault();
                                    if (OEmployeePayroll != null)
                                    {
                                        var OEmployeePayrollS = OEmployeePayroll.Employee.ServiceBookDates.ServiceLastDate;
                                        var OEmployeePayrollR = OEmployeePayroll.Employee.ServiceBookDates.ResignationDate;
                                        if (OEmployeePayrollS == null && OEmployeePayrollR == null)
                                        {
                                            OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct)
                                                .Where(r => r.Id == i).AsNoTracking().AsParallel().SingleOrDefault();


                                            foreach (int SH in SalHead)
                                            {
                                                var val = db.SalaryHead.Find(SH);
                                                OEDT.SalaryHead = val;

                                                var EmpSalT = OEmployeePayroll.OtherEarningDeductionT != null ? OEmployeePayroll.OtherEarningDeductionT.Where(e => e.PayMonth == PayMonth && e.SalaryHead.Id == OEDT.SalaryHead.Id) : null;
                                                if (EmpSalT != null && EmpSalT.Count() > 0)
                                                {
                                                    if (EmpCode == null || EmpCode == "")
                                                    {
                                                        EmpCode = OEmployee.EmpCode;
                                                    }
                                                    else
                                                    {
                                                        EmpCode = EmpCode + ", " + OEmployee.EmpCode;
                                                    }
                                                }

                                                //var OEmpSalRelT = db.EmployeePayroll.Where(e => e.Id == OEmployeePayroll.Id).Include(e => e.OtherEarningDeductionT).SingleOrDefault();
                                                var EmpSalRelT = db.SalaryT.Where(e => e.PayMonth == PayMonth && e.ReleaseDate != null && e.EmployeePayroll_Id == OEmployeePayroll.Id).AsNoTracking().AsParallel().SingleOrDefault();
                                                if (EmpSalRelT != null)
                                                {
                                                    if (EmpRel == null || EmpRel == "")
                                                    {
                                                        EmpRel = OEmployee.EmpCode;
                                                    }
                                                    else
                                                    {
                                                        EmpRel = EmpRel + ", " + OEmployee.EmpCode;
                                                    }
                                                }

                                                var EmpSalDelT = db.SalaryT.Where(e => e.PayMonth == PayMonth && e.ReleaseDate == null && e.EmployeePayroll_Id == OEmployeePayroll.Id).AsNoTracking().AsParallel().SingleOrDefault();
                                                if (EmpSalDelT != null)
                                                {
                                                    if (EmpSalDel == null || EmpSalDel == "")
                                                    {
                                                        EmpSalDel = OEmployee.EmpCode;
                                                    }
                                                    else
                                                    {
                                                        EmpSalDel = EmpSalDel + ", " + OEmployee.EmpCode;
                                                    }
                                                    SalaryGen.DeleteSalary(EmpSalDelT.Id, PayMonth);
                                                    SalDel = true;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (EmpCodeRetired == null)
                                            {
                                                EmpCodeRetired = OEmployeePayroll.Employee.EmpCode;
                                            }
                                            else
                                            {
                                                EmpCodeRetired = EmpCodeRetired + ", " + OEmployeePayroll.Employee.EmpCode;
                                            }
                                        }
                                    }
                                }
                            }

                            if (EmpCode != null)
                            {
                                return Json(new { data = rowno, success = false, responseText = "OtherEarning already exists for employee " + EmpCode + "." }, JsonRequestBehavior.AllowGet);
                            }
                            if (EmpRel != null)
                            {
                                return Json(new { data = rowno, success = false, responseText = "Salary released for employee " + EmpRel + ". You can't change Earning now." }, JsonRequestBehavior.AllowGet);
                            }
                            if (EmpCodeRetired != null)
                            {
                                return Json(new { data = rowno, success = false, responseText = "Employee is exited from the organisation " + EmpCode + "." }, JsonRequestBehavior.AllowGet);
                            }
                            if (!string.IsNullOrEmpty(sEmpCode))
                            {
                                foreach (var i in ids)
                                {
                                    EmployeePayroll OEmployeePayroll = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.OtherEarningDeductionT)
                                                       .Include(e => e.OtherEarningDeductionT.Select(r => r.SalaryHead)).Where(e => e.Employee.Id == i).SingleOrDefault();
                                    Employee OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct).Where(r => r.Id == i).SingleOrDefault();
                                    foreach (int SH in SalHead)
                                    {

                                        var val = db.SalaryHead.Find(SH);
                                        OEDT.SalaryHead = val;
                                        if (OEmployeePayroll.OtherEarningDeductionT.Any(e => e.PayMonth == PayMonth && e.SalaryHead.Id == OEDT.SalaryHead.Id))
                                        {
                                            return Json(new { data = rowno, success = false, responseText = "OtherEarning already exists for employee " + EmpCode + "." }, JsonRequestBehavior.AllowGet);
                                        }
                                        else
                                        {
                                            OEDT.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                                            OthEarningDeductionT ObjOEDT = new OthEarningDeductionT();
                                            {
                                                ObjOEDT.SalaryHead = OEDT.SalaryHead;
                                                ObjOEDT.PayMonth = OEDT.PayMonth;
                                                ObjOEDT.SalAmount = OEDT.SalAmount;
                                                ObjOEDT.TDSAmount = OEDT.TDSAmount;
                                                ObjOEDT.Remark = OEDT.Remark;
                                                ObjOEDT.GeoStruct = db.GeoStruct.Find(OEmployee.GeoStruct != null ? OEmployee.GeoStruct.Id : 0);
                                                ObjOEDT.FuncStruct = db.FuncStruct.Find(OEmployee.FuncStruct != null ? OEmployee.FuncStruct.Id : 0);
                                                ObjOEDT.PayStruct = db.PayStruct.Find(OEmployee.PayStruct.Id != null ? OEmployee.PayStruct.Id : 0);
                                                ObjOEDT.DBTrack = OEDT.DBTrack;

                                            }

                                            errorList = ValidateObj(ObjOEDT);
                                            if (errorList.Count > 0)
                                            {
                                                return Json(new { success = false, data = rowno, responseText = string.Join(",", errorList) }, JsonRequestBehavior.AllowGet);
                                            }

                                            using (TransactionScope ts = new TransactionScope())
                                            {

                                                db.OthEarningDeductionT.Add(ObjOEDT);
                                                db.SaveChanges();
                                                List<OthEarningDeductionT> OFAT = new List<OthEarningDeductionT>();
                                                OFAT.Add(db.OthEarningDeductionT.Find(ObjOEDT.Id));

                                                var aa = db.EmployeePayroll.Find(OEmployeePayroll.Id);
                                                OFAT.AddRange(aa.OtherEarningDeductionT);
                                                aa.OtherEarningDeductionT = OFAT;
                                                db.EmployeePayroll.Attach(aa);
                                                db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                                db.SaveChanges();
                                                db.Entry(aa).State = System.Data.Entity.EntityState.Detached;
                                                ts.Complete();
                                            }
                                        }

                                    }
                                }
                                if (SalDel == true)
                                {
                                    return Json(new { data = rowno, success = true, responseText = "Salary deleted for employee " + EmpSalDel + ". And Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
                                }
                                else
                                {
                                    return Json(new { data = rowno, success = true, responseText = "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
                                }

                            }
                            else
                            {
                                return Json(new { data = rowno, success = false, responseText = "Select Employee" }, JsonRequestBehavior.AllowGet);

                            }
                        }
                    case "OTHEARNINGDEDUCTIONT":
                        using (DataBaseContext db = new DataBaseContext())
                        {
                            OthEarningDeductionT OEDT = new OthEarningDeductionT();
                            string SalaryHeadlist = form["SalaryHead"] == "0" ? "" : form["SalaryHead"];
                            SalaryHeadlist = SalaryHeadlist.Replace("#", " ");
                            String PayMonth = form["PayMonth"] != null ? form["PayMonth"] : null;
                            String SalAmount = form["SalAmount"] != null ? form["SalAmount"] : null;
                            String TDSAmount = form["TDSAmount"] != null ? form["TDSAmount"] : null;
                            String Remark = form["Remark"] != null ? form["Remark"] : null;
                            if (SalAmount != null)
                            {
                                OEDT.SalAmount = Convert.ToDouble(SalAmount);
                            }
                            if (TDSAmount != null)
                            {
                                OEDT.TDSAmount = Convert.ToDouble(TDSAmount);
                            }
                            if (Remark != null)
                            {
                                OEDT.Remark = Convert.ToString(Remark);
                            }
                            else
                            {
                                OEDT.Remark = Convert.ToString(SalaryHeadlist);

                            }
                            if (PayMonth != null)
                            {
                                OEDT.PayMonth = PayMonth;
                            }
                            if (SalAmount != null)
                            {
                                OEDT.SalAmount = Convert.ToDouble(SalAmount);
                            }
                            if (TDSAmount != null)
                            {
                                OEDT.TDSAmount = Convert.ToDouble(TDSAmount);
                            }



                            string EmpCode = null;
                            string EmpCodeRetired = null;
                            string EmpRel = null;

                            List<String> SalHeadCodeList = new List<String>();
                            List<int> SalHead = new List<int>();
                            if (SalaryHeadlist != null && SalaryHeadlist != "")
                            {
                                SalHeadCodeList = Utility.StringIdsToListString(SalaryHeadlist);
                            }
                            foreach (var item in SalHeadCodeList)
                            {
                                var aa = db.SalaryHead.Where(e => e.Code == item).Select(e => e.Id).FirstOrDefault();
                                if (aa != 0)
                                {
                                    SalHead.Add(aa);
                                }
                            }
                            if (ids != null)
                            {

                                foreach (var i in ids)
                                {
                                    Employee OEmployee = null;
                                    EmployeePayroll OEmployeePayroll = null;
                                    //OEmployeePayroll = db.EmployeePayroll.Include(q => q.Employee).Include(q => q.Employee.ServiceBookDates)
                                    //    .Where(e => e.Employee.Id == i && (e.Employee.ServiceBookDates.ResignationDate == null || e.Employee.ServiceBookDates.ServiceLastDate == null)).AsNoTracking().AsParallel().SingleOrDefault();
                                    OEmployeePayroll = db.EmployeePayroll
                                                        .Include(q => q.Employee)
                                                        .Include(q => q.Employee.ServiceBookDates)
                                                        .Include(e => e.OtherEarningDeductionT)
                                                        .Include(e => e.OtherEarningDeductionT.Select(r => r.SalaryHead))
                                                         .Where(e => e.Employee.Id == i).AsNoTracking().AsParallel().SingleOrDefault();
                                    if (OEmployeePayroll != null)
                                    {
                                        var OEmployeePayrollS = OEmployeePayroll.Employee.ServiceBookDates.ServiceLastDate;
                                        var OEmployeePayrollR = OEmployeePayroll.Employee.ServiceBookDates.ResignationDate;
                                        if (OEmployeePayrollS == null && OEmployeePayrollR == null)
                                        {
                                            OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct)
                                                .Where(r => r.Id == i).AsNoTracking().AsParallel().SingleOrDefault();

                                            //var OEmpSalT = db.EmployeePayroll.Where(e => e.Id == OEmployeePayroll.Id).AsNoTracking().AsParallel().SingleOrDefault();
                                            foreach (int SH in SalHead)
                                            {
                                                var val = db.SalaryHead.Find(SH);
                                                // OEDT.SalaryHead = val;

                                                var EmpSalT = OEmployeePayroll.OtherEarningDeductionT != null ?
                                                    OEmployeePayroll.OtherEarningDeductionT.Where(e => e.PayMonth == PayMonth && e.SalaryHead.Id == val.Id) : null;
                                                if (EmpSalT != null && EmpSalT.Count() > 0)
                                                {
                                                    if (EmpCode == null)
                                                    {
                                                        EmpCode = OEmployee.EmpCode;
                                                    }
                                                    else
                                                    {
                                                        EmpCode = EmpCode + ", " + OEmployee.EmpCode;
                                                    }
                                                }

                                                //  var OEmpSalRelT = db.EmployeePayroll.Where(e => e.Id == OEmployeePayroll.Id).Include(e => e.OtherEarningDeductionT).SingleOrDefault();
                                                var EmpSalRelT = db.SalaryT.Where(e => e.PayMonth == PayMonth && e.ReleaseDate != null && e.EmployeePayroll_Id == OEmployeePayroll.Id).AsNoTracking().AsParallel().SingleOrDefault();

                                                if (EmpSalRelT != null)
                                                {
                                                    if (EmpRel == null)
                                                    {
                                                        EmpRel = OEmployee.EmpCode;
                                                    }
                                                    else
                                                    {
                                                        EmpRel = EmpRel + ", " + OEmployee.EmpCode;
                                                    }
                                                }

                                                var EmpSalDelT = db.SalaryT.Where(e => e.PayMonth == PayMonth && e.ReleaseDate == null && e.EmployeePayroll_Id == OEmployeePayroll.Id).AsNoTracking().AsParallel().SingleOrDefault();
                                                if (EmpSalDelT != null)
                                                {
                                                    if (EmpSalDel == null || EmpSalDel == "")
                                                    {
                                                        EmpSalDel = OEmployee.EmpCode;
                                                    }
                                                    else
                                                    {
                                                        EmpSalDel = EmpSalDel + ", " + OEmployee.EmpCode;
                                                    }
                                                    SalaryGen.DeleteSalary(EmpSalDelT.Id, PayMonth);
                                                    SalDel = true;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (EmpCodeRetired == null)
                                            {
                                                EmpCodeRetired = OEmployeePayroll.Employee.EmpCode;
                                            }
                                            else
                                            {
                                                EmpCodeRetired = EmpCodeRetired + ", " + OEmployeePayroll.Employee.EmpCode;
                                            }
                                        }
                                    }
                                }
                            }
                            if (EmpCodeRetired != null)
                            {
                                return Json(new { data = rowno, success = false, responseText = "Employee is exited from the organisation " + EmpCode + "." }, JsonRequestBehavior.AllowGet);
                            }
                            if (EmpCode != null)
                            {
                                return Json(new { data = rowno, responseText = "OtherEarningDeduction already exists for employee " + EmpCode + ".", success = false }, JsonRequestBehavior.AllowGet);
                            }
                            if (EmpRel != null)
                            {
                                return Json(new { data = rowno, success = false, responseText = "Salary released for employee " + EmpRel + ". You can't change Deduction now." }, JsonRequestBehavior.AllowGet);
                            }
                            if (!string.IsNullOrEmpty(sEmpCode))
                            {
                                foreach (var i in ids)
                                {
                                    EmployeePayroll OEmployeePayroll = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.OtherEarningDeductionT)
                                                        .Include(e => e.OtherEarningDeductionT.Select(r => r.SalaryHead)).Where(e => e.Employee.Id == i).SingleOrDefault();
                                    Employee OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct).Where(r => r.Id == i).SingleOrDefault();
                                    foreach (int SH in SalHead)
                                    {
                                        var val = db.SalaryHead.Find(SH);
                                        OEDT.SalaryHead = val;
                                        if (OEmployeePayroll.OtherEarningDeductionT.Any(e => e.PayMonth == PayMonth && e.SalaryHead.Id == OEDT.SalaryHead.Id))
                                        {
                                            return Json(new { data = rowno, success = false, responseText = "OtherEarning already exists for employee " + EmpCode + "." }, JsonRequestBehavior.AllowGet);
                                        }
                                        else
                                        {


                                            OEDT.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                                            OthEarningDeductionT ObjOEDT = new OthEarningDeductionT();
                                            {
                                                ObjOEDT.SalaryHead = OEDT.SalaryHead;
                                                ObjOEDT.PayMonth = OEDT.PayMonth;
                                                ObjOEDT.SalAmount = OEDT.SalAmount;
                                                ObjOEDT.TDSAmount = OEDT.TDSAmount;
                                                ObjOEDT.Remark = OEDT.Remark;
                                                ObjOEDT.GeoStruct = db.GeoStruct.Find(OEmployee.GeoStruct != null ? OEmployee.GeoStruct.Id : 0);
                                                ObjOEDT.FuncStruct = db.FuncStruct.Find(OEmployee.FuncStruct != null ? OEmployee.FuncStruct.Id : 0);
                                                ObjOEDT.PayStruct = db.PayStruct.Find(OEmployee.PayStruct != null ? OEmployee.PayStruct.Id : 0);
                                                ObjOEDT.DBTrack = OEDT.DBTrack;

                                            }
                                            errorList = ValidateObj(ObjOEDT);
                                            if (errorList.Count > 0)
                                            {
                                                return Json(new { success = false, data = rowno, responseText = string.Join(",", errorList) }, JsonRequestBehavior.AllowGet);

                                            }
                                            using (TransactionScope ts = new TransactionScope())
                                            {
                                                try
                                                {
                                                    db.OthEarningDeductionT.Add(ObjOEDT);
                                                    db.SaveChanges();
                                                    List<OthEarningDeductionT> OFAT = new List<OthEarningDeductionT>();
                                                    OFAT.Add(db.OthEarningDeductionT.Find(ObjOEDT.Id));
                                                    var aa = db.EmployeePayroll.Find(OEmployeePayroll.Id);
                                                    OFAT.AddRange(aa.OtherEarningDeductionT);
                                                    aa.OtherEarningDeductionT = OFAT;
                                                    db.EmployeePayroll.Attach(aa);
                                                    db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                                    db.SaveChanges();
                                                    db.Entry(aa).State = System.Data.Entity.EntityState.Detached;
                                                    ts.Complete();

                                                }
                                                catch (DataException ex)
                                                {
                                                    LogFile Logfile = new LogFile();
                                                    ErrorLog Err = new ErrorLog()
                                                    {
                                                        ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                                                        ExceptionMessage = ex.Message,
                                                        ExceptionStackTrace = ex.StackTrace,
                                                        LineNo = Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                                                        LogTime = DateTime.Now
                                                    };
                                                    Logfile.CreateLogFile(Err);
                                                    return Json(new { data = rowno, success = false, responseText = ex.InnerException.Message.ToString() }, JsonRequestBehavior.AllowGet);
                                                }
                                            }
                                        }
                                    }
                                }
                                if (SalDel == true)
                                {
                                    return Json(new { data = rowno, success = true, responseText = "Salary deleted for employee " + EmpRel + ". And Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
                                }
                                else
                                {
                                    return Json(new { data = rowno, success = true, responseText = "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
                                }

                            }
                            else
                            {
                                return Json(new { data = rowno, success = false, responseText = "Select Employee" }, JsonRequestBehavior.AllowGet);
                            }
                        }
                    case "INCRDUELIST":
                        using (DataBaseContext db = new DataBaseContext())
                        {
                            IncrementServiceBook incr = new IncrementServiceBook();
                            string SalaryHeadlist = form["SalaryHead"] == "0" ? "" : form["SalaryHead"];
                            String EmpCode = form["EmpCode"] != null ? form["EmpCode"] : null;
                            String ProcessIncrDate = form["ProcessIncrDate"] != null ? form["ProcessIncrDate"] : null;
                            String OrignalIncrDate = form["OrignalIncrDate"] != null ? form["OrignalIncrDate"] : null;
                            String ReleaseDate = form["ReleaseDate"] != null ? form["ReleaseDate"] : null;
                            String OldBasic = form["OldBasic"] != null ? form["OldBasic"] : null;
                            String NewBasic = form["NewBasic"] != null ? form["NewBasic"] : null;
                            string IncrActivity = form["IncrActivity"] != null ? form["IncrActivity"] : null;
                            if (IncrActivity != null)
                            {
                                var Incrid = db.IncrActivity.Where(e => e.Name.Replace(" ", "").ToUpper() == IncrActivity.Replace("#", "").ToUpper()).SingleOrDefault();
                                incr.IncrActivity = Incrid;
                            }
                            if (OldBasic != null)
                            {
                                incr.OldBasic = Convert.ToDouble(OldBasic);
                            }
                            if (NewBasic != null)
                            {
                                incr.NewBasic = Convert.ToDouble(NewBasic);

                            }
                            if (OrignalIncrDate != null)
                            {
                                incr.OrignalIncrDate = Convert.ToDateTime(OrignalIncrDate);
                            }
                            if (ProcessIncrDate != null)
                            {
                                incr.ProcessIncrDate = Convert.ToDateTime(ProcessIncrDate);
                            }
                            if (ReleaseDate != null)
                            {
                                incr.ReleaseDate = Convert.ToDateTime(ReleaseDate);
                            }
                            if (!string.IsNullOrEmpty(sEmpCode))
                            {
                                foreach (var i in ids)
                                {
                                    EmployeePayroll OEmployeePayroll = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.IncrementServiceBook).Where(e => e.Employee.Id == i).SingleOrDefault();
                                    Employee OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct).Where(r => r.Id == i).SingleOrDefault();
                                    //foreach (int SH in SalHead)
                                    //{


                                    incr.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                                    IncrementServiceBook ObjOEDT = new IncrementServiceBook();
                                    {
                                        ObjOEDT.StagnancyAppl = false;
                                        ObjOEDT.NewBasic = incr.NewBasic;
                                        ObjOEDT.OldBasic = incr.OldBasic;
                                        ObjOEDT.ProcessIncrDate = incr.ProcessIncrDate;
                                        ObjOEDT.ReleaseDate = incr.ReleaseDate;
                                        ObjOEDT.ReleaseFlag = true;
                                        ObjOEDT.OrignalIncrDate = incr.OrignalIncrDate;
                                        ObjOEDT.IsHold = false;
                                        ObjOEDT.StagnancyCount = 0;
                                        ObjOEDT.IncrActivity = incr.IncrActivity;
                                        ObjOEDT.GeoStruct = db.GeoStruct.Find(OEmployee.GeoStruct != null ? OEmployee.GeoStruct.Id : 0);
                                        ObjOEDT.FuncStruct = db.FuncStruct.Find(OEmployee.FuncStruct != null ? OEmployee.FuncStruct.Id : 0);
                                        ObjOEDT.PayStruct = db.PayStruct.Find(OEmployee.PayStruct != null ? OEmployee.PayStruct.Id : 0);
                                        ObjOEDT.DBTrack = incr.DBTrack;
                                        ObjOEDT.IsRegularIncrDate = true;

                                    }
                                    errorList = ValidateObj(ObjOEDT);
                                    if (errorList.Count > 0)
                                    {
                                        return Json(new { success = false, data = rowno, responseText = string.Join(",", errorList) }, JsonRequestBehavior.AllowGet);

                                    }
                                    using (TransactionScope ts = new TransactionScope())
                                    {
                                        try
                                        {
                                            db.IncrementServiceBook.Add(ObjOEDT);
                                            db.SaveChanges();
                                            List<IncrementServiceBook> OFAT = new List<IncrementServiceBook>();
                                            OFAT.Add(db.IncrementServiceBook.Find(ObjOEDT.Id));
                                            var aa = db.EmployeePayroll.Find(OEmployeePayroll.Id);
                                            OFAT.AddRange(aa.IncrementServiceBook);
                                            aa.IncrementServiceBook = OFAT;
                                            db.EmployeePayroll.Attach(aa);
                                            db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();
                                            db.Entry(aa).State = System.Data.Entity.EntityState.Detached;
                                            ts.Complete();

                                        }
                                        catch (DataException ex)
                                        {
                                            LogFile Logfile = new LogFile();
                                            ErrorLog Err = new ErrorLog()
                                            {
                                                ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                                                ExceptionMessage = ex.Message,
                                                ExceptionStackTrace = ex.StackTrace,
                                                LineNo = Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                                                LogTime = DateTime.Now
                                            };
                                            Logfile.CreateLogFile(Err);
                                            return Json(new { data = rowno, success = false, responseText = ex.InnerException.Message.ToString() }, JsonRequestBehavior.AllowGet);
                                        }
                                    }
                                    // }
                                }
                                return Json(new { data = rowno, success = true, responseText = "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);

                            }
                            else
                            {
                                return Json(new { data = rowno, success = false, responseText = "Select Employee" }, JsonRequestBehavior.AllowGet);
                            }

                        }

                    case "EMPOFF":
                        using (DataBaseContext db = new DataBaseContext())
                        {
                            if (db.Employee.Include(e => e.EmpOffInfo).Where(o => o.EmpCode == sEmpCode && o.EmpOffInfo != null).Count() > 0)
                            {
                                return Json(new { success = false, data = rowno, responseText = "EmpOff Already Exists." }, JsonRequestBehavior.AllowGet);
                            }
                            //if (sEmpCode != null && sEmpCode != "")
                            //{
                            //    EmpCodeList = Utility.StringIdsToListString(sEmpCode);
                            //}
                            //else
                            //{
                            //    return Json(new { success = false, data = rowno, responseText = "Employee Not Found..!" }, JsonRequestBehavior.AllowGet);
                            //}
                            //foreach (var item in ids)
                            //{
                            //    var aa = db.Employee.Where(e => e.Id == item).Select(e => e.Id).FirstOrDefault();
                            //    if (aa != 0)
                            //    {
                            //        ids.Add(aa);
                            //    }
                            //}
                            EmpOff EOff = new EmpOff();
                            string PayMode = form["PayMode"] == null ? null : form["PayMode"];
                            string Bank = form["Bank"] == null ? null : form["Bank"];
                            string Branch = form["Branch"] == null ? null : form["Branch"];
                            string AccountType = form["AccountType"] == null ? null : form["AccountType"];
                            string AccountNo = form["AccountNo"] == null ? null : form["AccountNo"];
                            string PayProcessGroup = form["PayProcessGroup"] == null ? null : form["PayProcessGroup"];
                            string PayScale = form["PayScale"] == null ? null : form["PayScale"];
                            string SelfHandicap = form["SelfHandicap"] == null ? null : form["SelfHandicap"];
                            string FamilyHandicap = form["FamilyHandicap"] == null ? null : form["FamilyHandicap"];
                            string HandicapRemark = form["HandicapRemark"] == null ? null : form["HandicapRemark"];

                            string ESICAppl = form["ESICAppl"] == null ? null : form["ESICAppl"];
                            string LWFAppl = form["LWFAppl"] == null ? null : form["LWFAppl"];
                            string PFAppl = form["PFAppl"] == null ? null : form["PFAppl"];
                            string PTAppl = form["PTAppl"] == null ? null : form["PTAppl"];
                            string VPFAppl = form["VPFAppl"] == null ? null : form["VPFAppl"];
                            string VPFPerc = form["VPFPerc"] == null ? null : form["VPFPerc"];
                            string VPFAmt = form["VPFAmt"] == null ? null : form["VPFAmt"];
                            string PensionAppl = form["PensionAppl"] == null ? null : form["PensionAppl"];
                            string PFTrust_EstablishmentId = form["PFTrust_EstablishmentId"] == null ? null : form["PFTrust_EstablishmentId"];


                            //Nationality Id
                            string AdharNo = form["AdharNo"] == null ? null : form["AdharNo"];
                            string DLNO = form["DLNO"] == null ? null : form["DLNO"];
                            string EDLINo = form["PayScale"] == null ? null : form["EDLINo"];
                            string ESICNo = form["ESICNo"] == null ? null : form["ESICNo"];
                            string GINo = form["GINo"] == null ? null : form["GINo"];
                            string LWFNo = form["LWFNo"] == null ? null : form["LWFNo"];
                            string PANNo = form["PANNo"] == null ? null : form["PANNo"];
                            string PensionNo = form["PensionNo"] == null ? null : form["PensionNo"];
                            string PFNo = form["PFNo"] == null ? null : form["PFNo"];
                            string PPNO = form["PPNO"] == null ? null : form["PPNO"];
                            string PTNo = form["PTNo"] == null ? null : form["PTNo"];
                            string RCNo = form["RCNo"] == null ? null : form["RCNo"];
                            string UANNo = form["UANNo"] == null ? null : form["UANNo"];
                            string VCNo = form["VCNo"] == null ? null : form["VCNo"];
                            try
                            {
                                var oNationalityID = new NationalityID();
                                oNationalityID.AdharNo = AdharNo;
                                oNationalityID.DLNO = DLNO;
                                oNationalityID.EDLINo = EDLINo;
                                oNationalityID.ESICNo = ESICNo;
                                oNationalityID.GINo = GINo;
                                oNationalityID.LWFNo = LWFNo;
                                oNationalityID.PANNo = PANNo;
                                oNationalityID.PensionNo = PensionNo;
                                oNationalityID.PFNo = PFNo;
                                oNationalityID.PPNO = PPNO;
                                oNationalityID.PTNo = PTNo;
                                oNationalityID.RCNo = RCNo;
                                oNationalityID.UANNo = UANNo;
                                oNationalityID.VCNo = VCNo;

                                errorList = ValidateObj(oNationalityID);
                                if (errorList.Count > 0)
                                {
                                    return Json(new { success = false, data = rowno, responseText = string.Join(",", errorList) }, JsonRequestBehavior.AllowGet);

                                }
                                db.NationalityID.Add(oNationalityID);
                                db.SaveChanges();
                                EOff.NationalityID = oNationalityID;
                                EOff.FamilyHandicap = FamilyHandicap != null ? Convert.ToBoolean(FamilyHandicap) : false;
                                EOff.HandicapRemark = HandicapRemark;
                                EOff.SelfHandicap = SelfHandicap != null ? Convert.ToBoolean(SelfHandicap) : false;

                                EOff.ESICAppl = ESICAppl != null ? Convert.ToBoolean(ESICAppl) : false;
                                EOff.LWFAppl = LWFAppl != null ? Convert.ToBoolean(LWFAppl) : false;
                                EOff.PFAppl = PFAppl != null ? Convert.ToBoolean(PFAppl) : false;
                                EOff.PTAppl = PTAppl != null ? Convert.ToBoolean(PTAppl) : false;
                                EOff.VPFAppl = VPFAppl != null ? Convert.ToBoolean(VPFAppl) : false;
                                EOff.PensionAppl = PensionAppl != null ? Convert.ToBoolean(PensionAppl) : false;
                                EOff.VPFAmt = VPFAmt != null ? Convert.ToDouble(VPFAmt) : 0;
                                EOff.VPFPerc = VPFPerc != null ? Convert.ToDouble(VPFPerc) : 0;
                                EOff.PFTrust_EstablishmentId = PFTrust_EstablishmentId != null && PFTrust_EstablishmentId != "" ? PFTrust_EstablishmentId : "";

                                if (AccountNo != null)
                                {
                                    EOff.AccountNo = AccountNo;
                                }
                                if (PayScale != null && PayScale != "")
                                {
                                    var value = db.PayScale.Find(int.Parse(PayScale));
                                    EOff.PayScale = value;

                                }

                                if (PayMode != null && PayMode != "")
                                {
                                    var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "404").FirstOrDefault().LookupValues.Where(e => e.LookupVal.ToUpper() == PayMode.ToUpper()).FirstOrDefault();//db.LookupValue.Where(e => e.LookupVal.ToUpper() == PayMode.ToUpper()).SingleOrDefault();
                                    EOff.PayMode = val;
                                }
                                if (Bank != null && Bank != "")
                                {
                                    var val = db.Bank.Where(e => e.Code.ToUpper() == Bank.ToUpper()).SingleOrDefault();
                                    EOff.Bank = val;
                                }

                                if (Branch != null && Branch != "")
                                {
                                    var val = db.Branch.Where(e => e.Code.ToUpper() == Branch.ToUpper()).SingleOrDefault();
                                    EOff.Branch = val;
                                }

                                if (AccountType != null && AccountType != "")
                                {
                                    var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "405").FirstOrDefault().LookupValues.Where(e => e.LookupVal.ToUpper() == PayMode.ToUpper()).FirstOrDefault();//db.LookupValue.Where(e => e.LookupVal.ToUpper() == AccountType.ToUpper()).SingleOrDefault();
                                    EOff.AccountType = val;
                                }

                                if (PayProcessGroup != null && PayProcessGroup != "")
                                {
                                    var val = db.PayProcessGroup.Where(e => e.Name.ToUpper() == PayProcessGroup.ToUpper()).SingleOrDefault();
                                    EOff.PayProcessGroup = val;
                                }

                            }
                            catch (Exception e)
                            {
                                return Json(new { success = false, data = rowno, responseText = e.InnerException.Message.ToString() }, JsonRequestBehavior.AllowGet);

                            }
                            EOff.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            EmpOff EmpOfficial = new EmpOff()
                            {
                                AccountNo = EOff.AccountNo == null ? null : EOff.AccountNo.Trim(),
                                AccountType = EOff.AccountType,
                                Bank = EOff.Bank,
                                Branch = EOff.Branch,
                                ESICAppl = EOff.ESICAppl,
                                FamilyHandicap = EOff.FamilyHandicap,
                                HandicapRemark = EOff.HandicapRemark,
                                LWFAppl = EOff.LWFAppl,
                                NationalityID = EOff.NationalityID,
                                PayMode = EOff.PayMode,
                                PayProcessGroup = EOff.PayProcessGroup,
                                //PayScale = EOff.PayScale,
                                PFAppl = EOff.PFAppl,
                                PTAppl = EOff.PTAppl,
                                SelfHandicap = EOff.SelfHandicap,
                                VPFAmt = EOff.VPFAmt,
                                VPFAppl = EOff.VPFAppl,
                                PensionAppl = EOff.PensionAppl,
                                PFTrust_EstablishmentId = EOff.PFTrust_EstablishmentId,
                                VPFPerc = EOff.VPFPerc,
                                PayScale = EOff.PayScale,
                                DBTrack = EOff.DBTrack
                            };
                            errorList = ValidateObj(EmpOfficial);
                            if (errorList.Count > 0)
                            {
                                return Json(new { success = false, data = rowno, responseText = string.Join(",", errorList) }, JsonRequestBehavior.AllowGet);

                            }
                            Employee OEmployee = db.Employee.Find(ids[0]);

                            using (TransactionScope ts = new TransactionScope())
                            {
                                try
                                {
                                    db.EmpOff.Add(EmpOfficial);
                                    var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, EOff.DBTrack);
                                    DT_EmpOff DT_EOff = (DT_EmpOff)rtn_Obj;
                                    DT_EOff.AccountType_Id = EOff.AccountType == null ? 0 : EOff.AccountType.Id;
                                    DT_EOff.Bank_Id = EOff.Bank == null ? 0 : EOff.Bank.Id;
                                    DT_EOff.Branch_Id = EOff.Branch == null ? 0 : EOff.Branch.Id;
                                    DT_EOff.NationalityID = EOff.NationalityID == null ? 0 : EOff.NationalityID.Id;
                                    DT_EOff.PayMode_Id = EOff.PayMode == null ? 0 : EOff.PayMode.Id;
                                    DT_EOff.PayProcessGroup_Id = EOff.PayProcessGroup == null ? 0 : EOff.PayProcessGroup.Id;
                                    //DT_EOff.PayScale_Id = EOff.PayScale == null ? 0 : EOff.PayScale.Id; 
                                    db.Create(DT_EOff);
                                    db.SaveChanges();


                                    if (OEmployee != null)
                                    {
                                        OEmployee.EmpOffInfo = EmpOfficial;
                                        db.Employee.Attach(OEmployee);
                                        db.Entry(OEmployee).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                        db.Entry(OEmployee).State = System.Data.Entity.EntityState.Detached;
                                    }
                                    ts.Complete();
                                    return Json(new { data = rowno, responseText = "Data Saved Successfully.", success = true }, JsonRequestBehavior.AllowGet);
                                }
                                catch (DbUpdateConcurrencyException)
                                {
                                    return RedirectToAction("Create", new { concurrencyError = true, id = EOff.Id });
                                }
                                catch (DataException e)
                                {
                                    return Json(new { success = false, data = rowno, responseText = e.InnerException.Message.ToString() }, JsonRequestBehavior.AllowGet);
                                }
                            }
                        }
                    case "SALATTENDANCET":

                        using (DataBaseContext db = new DataBaseContext())
                        {

                            string iEmpCode = form["EmpCode"] == "" ? "" : form["EmpCode"];
                            string iPayMonth = form["Paymonth"] == "" ? "" : form["Paymonth"];
                            string iPayableDays = form["PaybleDays"] == "" ? "" : form["PaybleDays"];

                            List<int> Ids = new List<int>(); 
                            Employee OEmployee = db.Employee
                                .Include(e => e.ServiceBookDates)
                                .Where(e => e.EmpCode == iEmpCode && e.ServiceBookDates.ServiceLastDate == null)
                                .SingleOrDefault();

                            if (OEmployee != null)
                            {
                                int Emp = OEmployee.Id;
                                Ids.Add(Emp); 
                            }

                            List<String> paychk = iPayMonth.Split('/').Select(e => e).ToList();
                            int monch = Convert.ToInt32(paychk[0]);
                            int yerch = Convert.ToInt32(paychk[1]);
                            int days = DateTime.DaysInMonth(yerch, monch);

                            int DaysInMonth = 0;
                            if (!string.IsNullOrEmpty(iPayMonth))
                            {
                                int mon = int.Parse(iPayMonth.Split('/')[0].StartsWith("0") == true ? iPayMonth.Split('/')[0].Remove(0, 1) : iPayMonth.Split('/')[0]);
                                DaysInMonth = System.DateTime.DaysInMonth(int.Parse(iPayMonth.Split('/')[1]), mon);
                            }

                            string Emppayconceptf30 = db.PayProcessGroup.Include(a => a.PayMonthConcept).AsNoTracking().OrderByDescending(e => e.Id).FirstOrDefault().PayMonthConcept.LookupVal;

                            int paydays = Convert.ToInt32(iPayableDays);

                            if (Emppayconceptf30 == "FIXED30DAYS")
                            {
                                if (paydays > 30)
                                {
                                    return Json(new { success = false, data = rowno, responseText = " Pay month concept is FIXED30DAYS. Max limit is 30 days ." }, JsonRequestBehavior.AllowGet);
                                }
                            }

                            if (Emppayconceptf30 == "CALENDAR")
                            {
                                if (paydays > days)
                                {
                                    return Json(new { success = false, data = rowno, responseText = " Pay month concept is CALENDAR. Max limit is " + days + " days ." }, JsonRequestBehavior.AllowGet);
                                }
                            }

                            var OSalT = db.SalaryT
                                .Include(e => e.EmployeePayroll)
                                .Include(e => e.EmployeePayroll.Employee)
                                .Where(e => Ids.Contains(e.EmployeePayroll.Employee.Id) && e.PayMonth == iPayMonth).ToList();

                            if (OSalT.Count()!=0)
                            {
                                return Json(new { success = false, data = rowno, responseText = " Salary Not Proceed ." }, JsonRequestBehavior.AllowGet);
                            }

                            var OSalAttendance = db.SalAttendanceT
                                .Include(e => e.EmployeePayroll)
                                .Include(e => e.EmployeePayroll.Employee)
                                .Where(e => Ids.Contains(e.EmployeePayroll.Employee.Id) && e.PayMonth == iPayMonth).ToList();

                            if (OSalAttendance.Count()!=0)
                            {
                                return Json(new { success = false, data = rowno, responseText = " Dublicate Record Is Not Insert ." }, JsonRequestBehavior.AllowGet);
                            }

                            SalAttendanceT ObjFAT = new SalAttendanceT();
                            {
                                ObjFAT.PayMonth = iPayMonth;
                                ObjFAT.PaybleDays = paydays;
                                ObjFAT.MonthDays = DaysInMonth;

                            }

                            using (TransactionScope ts = new TransactionScope())
                            {
                                try
                                {
                                    foreach (var i in Ids)
                                    {
                                        List<string> PayMnth = new List<string>();
                                        DateTime Paydate = Convert.ToDateTime("01/" + iPayMonth);

                                        var EmployeeDetails = db.Employee.Include(e => e.SalaryHoldDetails).Where(e => e.Id == i).AsNoTracking().FirstOrDefault().SalaryHoldDetails.Where(e => (e.FromDate >= Paydate) && (Paydate <= e.ToDate));

                                        if (EmployeeDetails.Count() == 0)
                                        {
                                            ObjFAT.PaybleDays = paydays;
                                        }
                                        else
                                        {
                                            ObjFAT.PaybleDays = 0;
                                        }

                                        var test = db.EmployeePayroll
                                       .Join(db.Employee, p => p.Employee.Id, pc => pc.Id, (p, pc) => new { p, pc })
                                      .Where(p => p.p.Employee.Id == i).AsNoTracking()
                                      .Select(m => new
                                      {
                                          GeoStruct_id = m.pc.GeoStruct.Id,
                                          PayStruct_id = m.pc.PayStruct.Id,
                                          FuncStruct_id = m.pc.FuncStruct.Id,
                                          Id = m.p.Id
                                      }).FirstOrDefault();

                                        ObjFAT.GeoStruct = db.GeoStruct.OrderBy(e => e.Id).Where(e => e.Id == test.GeoStruct_id).FirstOrDefault();

                                        ObjFAT.FuncStruct = db.FuncStruct.OrderBy(e => e.Id).Where(e => e.Id == test.FuncStruct_id).FirstOrDefault();

                                        ObjFAT.PayStruct = db.PayStruct.OrderBy(e => e.Id).Where(e => e.Id == test.PayStruct_id).FirstOrDefault();
                                        ObjFAT.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false, CreatedOn = DateTime.Now };

                                        db.SalAttendanceT.Add(ObjFAT);
                                        db.SaveChanges();
                                        List<SalAttendanceT> OFAT = new List<SalAttendanceT>();
                                        OFAT.Add(db.SalAttendanceT.Find(ObjFAT.Id));

                                        if (test == null)
                                        {
                                            EmployeePayroll OTEP = new EmployeePayroll()
                                            {
                                                Employee = db.Employee.Find(i),
                                                SalAttendance = OFAT,
                                                DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }

                                            };

                                            db.EmployeePayroll.Add(OTEP);
                                            db.SaveChanges();
                                        }
                                        else
                                        {
                                            var aa = db.EmployeePayroll.Find(test.Id);
                                            if (aa.SalAttendance != null)
                                            {
                                                OFAT.AddRange(aa.SalAttendance);
                                            }

                                            aa.SalAttendance = OFAT;
                                            db.EmployeePayroll.Attach(aa);
                                            db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();
                                            db.Entry(aa).State = System.Data.Entity.EntityState.Detached;

                                        }

                                    }


                                }
                                catch (Exception ex)
                                {
                                    LogFile Logfile = new LogFile();
                                    ErrorLog Err = new ErrorLog()
                                    {
                                        ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                                        ExceptionMessage = ex.Message,
                                        ExceptionStackTrace = ex.StackTrace,
                                        LineNo = Convert.ToString(new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                                        LogTime = DateTime.Now
                                    };
                                    Logfile.CreateLogFile(Err);

                                    return Json(new { data = rowno, responseText = ex.Message, success = false }, JsonRequestBehavior.AllowGet);
                                }
                                ts.Complete();
                            }
                            return Json(new { data = rowno, responseText = "Data Saved Successfully.", success = true }, JsonRequestBehavior.AllowGet);
                        }



                    case "PFTEMPLOYEELEDGER":
                        using (DataBaseContext db = new DataBaseContext())
                        {
                            string Empcodes = form["EmployeePFTrust_Id"] == null ? null : form["EmployeePFTrust_Id"];
                            var getEmpIds = db.Employee.Where(e => e.EmpCode == Empcodes).Select(s => s.Id).ToList();
                            if (getEmpIds.Count() > 0)
                            {
                                var alreadyExistChk = db.EmployeePFTrust.Include(e => e.Employee).Include(e => e.PFTEmployeeLedger).Where(o => getEmpIds.Contains(o.Employee.Id) && o.PFTEmployeeLedger.Count() > 0).FirstOrDefault();
                                if (alreadyExistChk != null)
                                {
                                    return Json(new { success = false, data = rowno, responseText = "EmployeePFTrust Already Exists." }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            EmployeePFTrust OEmployeePFTrust = null; //db.EmployeePFTrust.Include(e => e.Employee).Where(o => o.Employee.EmpCode == Empcodes).FirstOrDefault(); ;
                            Employee OEmployee = db.Employee.Where(e => e.EmpCode == Empcodes).FirstOrDefault();

                            OEmployeePFTrust
                              = db.EmployeePFTrust
                              .Include(e => e.PFTEmployeeLedger)
                              .Where(e => e.Employee.EmpCode == Empcodes).SingleOrDefault();

                            if (OEmployeePFTrust == null)
                            {
                                return Json(new { success = false, data = rowno, responseText = "EmployeePFTrust Id is not Generated in System." }, JsonRequestBehavior.AllowGet);
                            }

                            EmployeePFTrust pFTrust = new EmployeePFTrust();
                            string OwnOpenBal = form["OwnOpenBal"] == "0" ? "0" : form["OwnOpenBal"];
                            var PFMonthly = Convert.ToDouble(OwnOpenBal);
                            string OwnerOpenBal = form["OwnerOpenBal"] == "0" ? "0" : form["OwnerOpenBal"];
                            var OwnerMonthly = Convert.ToDouble(OwnerOpenBal);
                            string VPFOpenBal = form["VPFOpenBal"] == "0" ? "0" : form["VPFOpenBal"];
                            var AmountMonthly = Convert.ToDouble(VPFOpenBal);
                            string OwnIntOpenBal = form["OwnIntOpenBal"] == "0" ? "0" : form["OwnIntOpenBal"];
                            var PFInt = Convert.ToDouble(OwnIntOpenBal);
                            string OwnerIntOpenBal = form["OwnerIntOpenBal"] == "0" ? "0" : form["OwnerIntOpenBal"];
                            var OwnerInt = Convert.ToDouble(OwnerIntOpenBal);
                            string VPFIntOpenBal = form["VPFIntOpenBal"] == "0" ? "0" : form["VPFIntOpenBal"];
                            var vpfint = Convert.ToDouble(VPFIntOpenBal);
                            //13/10/2023 added
                            // string PfopenBal = form["PfopenBal"] == "0" ? "0" : form["PfopenBal"];
                            // string PfIntopenBal = form["PfIntopenBal"] == "0" ? "0" : form["PfIntopenBal"];
                            //13/10/2023 added
                            //string TotalIntOpenBal = form["TotalIntOpenBal"] == "0" ? "0" : form["TotalIntOpenBal"];
                            //var TotalInt = Convert.ToDouble(TotalIntOpenBal);
                            //var PassbookActivity = form["PassbookActivity"] == "0" ? "0" : form["PassbookActivity"];
                            string TDSAppl = form["TDSAppl"] == "0" ? "0" : form["TDSAppl"];

                            // int PassbookActId = Convert.ToInt32(PassbookActivity);

                            //var chkPassbook = db.LookupValue.Where(e => e.Id == PassbookActId).FirstOrDefault();
                            //if (chkPassbook != null)
                            //{
                            //    if (chkPassbook.LookupVal.ToUpper() != "PF BALANCE")
                            //    {
                            //        return Json(new { data = rowno, responseText = "Please Select PassbookActivity PF BALANCE.", success = true }, JsonRequestBehavior.AllowGet);
                            //    }
                            //}

                            string PostingDate = form["PostingDate"] == "0" ? "0" : form["PostingDate"];
                            var Posting = Convert.ToDateTime(PostingDate);

                            string CalcDate = form["CalcDate"] == "0" ? "0" : form["CalcDate"];
                            var Calc = Convert.ToDateTime(CalcDate);
                            //string TrustPFNo = form["TrustPFNo"] == "0" ? "0" : form["TrustPFNo"];

                            int comp_Id = 0;
                            comp_Id = Convert.ToInt32(Session["CompId"]);
                            var CompanyPayroll = new CompanyPayroll();
                            CompanyPayroll = db.CompanyPayroll.Where(e => e.Id == comp_Id).SingleOrDefault();

                            using (TransactionScope ts = new TransactionScope())
                            {
                                pFTrust.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                try
                                {
                                    List<PFTEmployeeLedger> PFTEmployeeLedger = new List<PFTEmployeeLedger>();

                                    PFTEmployeeLedger objPFTEmployeeLedger = new PFTEmployeeLedger()
                                    {
                                        OwnOpenBal = PFMonthly,
                                        OwnerOpenBal = OwnerMonthly,
                                        OwnIntOpenBal = PFInt,
                                        OwnerIntOpenBal = OwnerInt,
                                        VPFOpenBal = AmountMonthly,
                                        VPFIntOpenBal = vpfint,
                                        TotalIntOpenBal = OwnerInt + vpfint, //TotalInt,
                                        OwnCloseBal = PFMonthly,
                                        OwnerCloseBal = OwnerMonthly,
                                        OwnIntCloseBal = PFInt,
                                        OwnerIntCloseBal = OwnerInt,
                                        VPFCloseBal = AmountMonthly,
                                        VPFIntCloseBal = vpfint,
                                        TotalIntCloseBal = OwnerInt + vpfint, //TotalInt,
                                        PFOpenBal = PFMonthly + OwnerMonthly, //Convert.ToDouble(PfopenBal),
                                        PFCloseBal = PFMonthly + OwnerMonthly, //Convert.ToDouble(PfopenBal),
                                        PFIntOpenBal = OwnerInt,  //Convert.ToDouble(PfIntopenBal),
                                        PFIntCloseBal = OwnerInt, //Convert.ToDouble(PfIntopenBal),
                                        PassbookActivity = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "512").FirstOrDefault().LookupValues.Where(a => a.LookupVal.ToUpper() == "PF Balance".ToUpper()).SingleOrDefault(),
                                        PostingDate = Posting,
                                        CalcDate = Calc,
                                        MonthYear = Convert.ToDateTime(Posting).ToString("MM/yyyy"),
                                        IsTDSAppl = Convert.ToBoolean(TDSAppl),
                                        DBTrack = pFTrust.DBTrack,
                                        Narration = "Opening Balance"

                                    };
                                    db.PFTEmployeeLedger.Add(objPFTEmployeeLedger);
                                    //  PFTEmployeeLedger.Add(objPFTEmployeeLedger);
                                    db.SaveChanges();

                                    PFTEmployeeLedger.Add(db.PFTEmployeeLedger.Find(objPFTEmployeeLedger.Id));

                                    //if (OEmployeePFTrust == null)
                                    //{
                                    //    EmployeePFTrust OTEP = new EmployeePFTrust()
                                    //    {
                                    //        Employee = db.Employee.Find(OEmployee.Id),
                                    //        PFTEmployeeLedger = PFTEmployeeLedger,
                                    //        DBTrack = pFTrust.DBTrack

                                    //    };


                                    //    db.EmployeePFTrust.Add(OTEP);
                                    //    db.SaveChanges();
                                    //}
                                    //else
                                    //{
                                    var aa = db.EmployeePFTrust.Find(OEmployeePFTrust.Id);
                                    aa.PFTEmployeeLedger = PFTEmployeeLedger;
                                    db.EmployeePFTrust.Attach(aa);
                                    db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    db.Entry(aa).State = System.Data.Entity.EntityState.Detached;

                                    //}


                                    ts.Complete();
                                    return Json(new { data = rowno, responseText = "Data Saved Successfully.", success = true }, JsonRequestBehavior.AllowGet);

                                }
                                catch (Exception ex)
                                {
                                    LogFile Logfile = new LogFile();
                                    ErrorLog Err = new ErrorLog()
                                    {
                                        ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                                        ExceptionMessage = ex.Message,
                                        ExceptionStackTrace = ex.StackTrace,
                                        LineNo = Convert.ToString(new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                                        LogTime = DateTime.Now
                                    };
                                    Logfile.CreateLogFile(Err);

                                    return Json(new { data = rowno, responseText = ex.Message, success = false }, JsonRequestBehavior.AllowGet);
                                }

                            }



                        }

                    case "EMPLOYEE":
                        using (DataBaseContext db = new DataBaseContext())
                        {
                            if (db.Employee.Where(o => o.EmpCode == sEmpCode).Count() > 0)
                            {
                                return Json(new { success = false, data = rowno, responseText = "Employee Already Exists." }, JsonRequestBehavior.AllowGet);
                            }
                            Employee Emp = new Employee();
                            //basic
                            string EmpCode = form["EmpCode"] == "null" ? null : form["EmpCode"];
                            string Gender = form["Gender"] == "null" ? null : form["Gender"];
                            string CardCode = form["CardCode"] == "null" ? null : form["CardCode"];

                            if (EmpCode != null)
                            {
                                Emp.EmpCode = EmpCode;
                            }
                            if (Gender != null)
                            {
                                var lookupdata = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "129").SingleOrDefault();
                                Emp.Gender = lookupdata.LookupValues.Where(e => e.LookupVal.ToString().Trim().ToUpper() == Gender.ToString().Trim().ToUpper()).SingleOrDefault();
                            }
                            if (CardCode != null)
                            {
                                Emp.CardCode = CardCode;
                            }

                            //name
                            string EmpTitle_EmpName = form["EmpTitle_EmpName"] == "null" ? null : form["EmpTitle_EmpName"];
                            string FName_EmpName = form["FName_EmpName"] == "null" ? null : form["FName_EmpName"];
                            string MName_EmpName = form["MName_EmpName"] == "null" ? null : form["MName_EmpName"];
                            string LName_EmpName = form["LName_EmpName"] == "null" ? null : form["LName_EmpName"];
                            if (FName_EmpName != null)
                            {
                                FName_EmpName = FName_EmpName.Replace("#", " ");
                            }
                            if (MName_EmpName != null)
                            {
                                MName_EmpName = MName_EmpName.Replace("#", " ");
                            }
                            if (LName_EmpName != null)
                            {
                                LName_EmpName = LName_EmpName.Replace("#", " ");
                            }
                            // if (EmpTitle_EmpName != null && FName_EmpName != null && LName_EmpName != null)
                            if (EmpTitle_EmpName != null && FName_EmpName != null)// South bank Middle and last not mandatory
                            {
                                errorList = SaveNameOfEmp(EmpTitle_EmpName, FName_EmpName, MName_EmpName, LName_EmpName);
                                if (errorList.Count > 0)
                                {
                                    return Json(new { success = false, data = rowno, responseText = string.Join(",", errorList) }, JsonRequestBehavior.AllowGet);
                                }
                                else
                                {
                                    //Emp.EmpName = db.NameSingle.Where(e => e.FName == FName_EmpName && e.MName == MName_EmpName && e.LName == LName_EmpName).SingleOrDefault();
                                    Emp.EmpName = db.NameSingle.Where(e => e.FName == FName_EmpName && e.MName == MName_EmpName && e.LName == LName_EmpName).FirstOrDefault();

                                }
                            }

                            string EmpTitle_FatherName = form["EmpTitle_FatherName"] == "null" ? null : form["EmpTitle_FatherName"];
                            string FName_FatherName = form["FName_FatherName"] == "null" ? null : form["FName_FatherName"];
                            string MName_FatherName = form["MName_FatherName"] == "null" ? null : form["MName_FatherName"];
                            string LName_FatherName = form["LName_FatherName"] == "null" ? null : form["LName_FatherName"];
                            if (EmpTitle_FatherName != null && FName_FatherName != null && LName_FatherName != null)
                            {
                                errorList = SaveNameOfEmp(EmpTitle_FatherName, FName_FatherName, MName_FatherName, LName_FatherName);
                                if (errorList.Count > 0)
                                {
                                    return Json(new { success = false, data = rowno, responseText = string.Join(",", errorList) }, JsonRequestBehavior.AllowGet);
                                }
                                else
                                {
                                    //Emp.FatherName = db.NameSingle.Where(e => e.FName == FName_FatherName && e.LName == LName_FatherName).SingleOrDefault();
                                    Emp.FatherName = db.NameSingle.Where(e => e.FName == FName_FatherName && e.MName == MName_FatherName && e.LName == LName_FatherName).FirstOrDefault();
                                }
                            }
                            string EmpTitle_HusbandName = form["EmpTitle_HusbandName"] == "null" ? null : form["EmpTitle_HusbandName"];
                            string FName_HusbandName = form["FName_HusbandName"] == "null" ? null : form["FName_HusbandName"];
                            string MName_HusbandName = form["MName_HusbandName"] == "null" ? null : form["MName_HusbandName"];
                            string LName_HusbandName = form["LName_HusbandName"] == "null" ? null : form["LName_HusbandName"];
                            if (EmpTitle_HusbandName != null && FName_HusbandName != null && LName_HusbandName != null)
                            {
                                errorList = SaveNameOfEmp(EmpTitle_HusbandName, FName_HusbandName, MName_HusbandName, LName_HusbandName);
                                if (errorList.Count > 0)
                                {
                                    return Json(new { success = false, data = rowno, responseText = string.Join(",", errorList) }, JsonRequestBehavior.AllowGet);

                                }
                                else
                                {
                                    //Emp.HusbandName = db.NameSingle.Where(e => e.FName == FName_HusbandName && e.LName == LName_HusbandName).SingleOrDefault();
                                    Emp.HusbandName = db.NameSingle.Where(e => e.FName == FName_HusbandName && e.MName == MName_HusbandName && e.LName == LName_HusbandName).FirstOrDefault();
                                }
                            }
                            string EmpTitle_MotherName = form["EmpTitle_MotherName"] == "null" ? null : form["EmpTitle_MotherName"];
                            string FName_MotherName = form["FName_MotherName"] == "null" ? null : form["FName_MotherName"];
                            string MName_MotherName = form["MName_MotherName"] == "null" ? null : form["MName_MotherName"];
                            string LName_MotherName = form["LName_MotherName"] == "null" ? null : form["LName_MotherName"];
                            if (EmpTitle_MotherName != null && FName_MotherName != null && LName_MotherName != null)
                            {
                                errorList = SaveNameOfEmp(EmpTitle_MotherName, FName_MotherName, MName_MotherName, LName_MotherName);
                                if (errorList.Count > 0)
                                {
                                    return Json(new { success = false, data = rowno, responseText = string.Join(",", errorList) }, JsonRequestBehavior.AllowGet);
                                }
                                else
                                {
                                    //Emp.MotherName = db.NameSingle.Where(e => e.FName == FName_MotherName && e.LName == LName_MotherName).SingleOrDefault();
                                    Emp.MotherName = db.NameSingle.Where(e => e.FName == FName_MotherName && e.MName == LName_MotherName && e.LName == LName_MotherName).FirstOrDefault();
                                }
                            }
                            string EmpTitle_BeforeMarriageName = form["EmpTitle_BeforeMarriageName"] == "null" ? null : form["EmpTitle_BeforeMarriageName"];
                            string FName_BeforeMarriageName = form["FName_BeforeMarriageName"] == "null" ? null : form["FName_BeforeMarriageName"];
                            string MName_BeforeMarriageName = form["MName_BeforeMarriageName"] == "null" ? null : form["MName_BeforeMarriageName"];
                            string LName_BeforeMarriageName = form["LName_BeforeMarriageName"] == "null" ? null : form["LName_BeforeMarriageName"];
                            if (EmpTitle_BeforeMarriageName != null && FName_BeforeMarriageName != null && LName_BeforeMarriageName != null)
                            {
                                errorList = SaveNameOfEmp(EmpTitle_BeforeMarriageName, FName_BeforeMarriageName, MName_BeforeMarriageName, LName_BeforeMarriageName);
                                if (errorList.Count > 0)
                                {
                                    return Json(new { success = false, data = rowno, responseText = string.Join(",", errorList) }, JsonRequestBehavior.AllowGet);
                                }
                                else
                                {
                                    //Emp.BeforeMarriageName = db.NameSingle.Where(e => e.FName == FName_BeforeMarriageName && e.LName == LName_BeforeMarriageName).SingleOrDefault();
                                    Emp.BeforeMarriageName = db.NameSingle.Where(e => e.FName == FName_BeforeMarriageName && e.MName == MName_BeforeMarriageName && e.LName == LName_BeforeMarriageName).FirstOrDefault();
                                }
                            }
                            ///dates
                            string ServiceBookDates_BirthDate = form["ServiceBookDates_BirthDate"] == "null" ? null : form["ServiceBookDates_BirthDate"];
                            string ServiceBookDates_JoiningDate = form["ServiceBookDates_JoiningDate"] == "null" ? null : form["ServiceBookDates_JoiningDate"];
                            string ServiceBookDates_ProbationPeriod = form["ServiceBookDates_ProbationPeriod"] == "null" ? null : form["ServiceBookDates_ProbationPeriod"];
                            string ServiceBookDates_ProbationDate = form["ServiceBookDates_ProbationDate"] == "null" ? null : form["ServiceBookDates_ProbationDate"];
                            string ServiceBookDates_ConfirmPeriod = form["ServiceBookDates_ConfirmPeriod"] == "null" ? null : form["ServiceBookDates_ConfirmPeriod"];
                            string ServiceBookDates_ConfirmationDate = form["ServiceBookDates_ConfirmationDate"] == "null" ? null : form["ServiceBookDates_ConfirmationDate"];
                            string ServiceBookDates_LastIncrementDate = form["ServiceBookDates_LastIncrementDate"] == "null" ? null : form["ServiceBookDates_LastIncrementDate"];
                            string ServiceBookDates_LastPromotionDate = form["ServiceBookDates_LastPromotionDate"] == "null" ? null : form["ServiceBookDates_LastPromotionDate"];
                            string ServiceBookDates_LastTransferDate = form["ServiceBookDates_LastTransferDate"] == "null" ? null : form["ServiceBookDates_LastTransferDate"];
                            string ServiceBookDates_SeniorityDate = form["ServiceBookDates_SeniorityDate"] == "null" ? null : form["ServiceBookDates_SeniorityDate"];
                            string ServiceBookDates_SeniorityNo = form["ServiceBookDates_SeniorityNo"] == "null" ? null : form["ServiceBookDates_SeniorityNo"];
                            string ServiceBookDates_RetirementDate = form["ServiceBookDates_RetirementDate"] == "null" ? null : form["ServiceBookDates_RetirementDate"];
                            string ServiceBookDates_ResignationDate = form["ServiceBookDates_ResignationDate"] == "null" ? null : form["ServiceBookDates_ResignationDate"];
                            string ServiceBookDates_ResignReason = form["ServiceBookDates_ResignReason"] == "null" ? null : form["ServiceBookDates_ResignReason"];
                            string ServiceBookDates_ServiceLastDate = form["ServiceBookDates_ServiceLastDate"] == "null" ? null : form["ServiceBookDates_ServiceLastDate"];
                            string ServiceBookDates_PFJoingDate = form["ServiceBookDates_PFJoingDate"] == "null" ? null : form["ServiceBookDates_PFJoingDate"];
                            string ServiceBookDates_PensionJoingDate = form["ServiceBookDates_PensionJoingDate"] == "null" ? null : form["ServiceBookDates_PensionJoingDate"];
                            string ServiceBookDates_PFExitDate = form["ServiceBookDates_PFExitDate"] == "null" ? null : form["ServiceBookDates_PFExitDate"];
                            string ServiceBookDates_PensionExitDate = form["ServiceBookDates_PensionExitDate"] == "null" ? null : form["ServiceBookDates_PensionExitDate"];
                            ServiceBookDates oServiceBookDates = new ServiceBookDates();
                            if (ServiceBookDates_BirthDate != null)
                            {
                                oServiceBookDates.BirthDate = Convert.ToDateTime(ServiceBookDates_BirthDate);
                            }
                            if (ServiceBookDates_JoiningDate != null)
                            {
                                oServiceBookDates.JoiningDate = Convert.ToDateTime(ServiceBookDates_JoiningDate);
                            }
                            if (ServiceBookDates_ProbationPeriod != null)
                            {
                                oServiceBookDates.ProbationPeriod = Convert.ToInt32(ServiceBookDates_ProbationPeriod);
                            }
                            if (ServiceBookDates_ProbationDate != null)
                            {
                                oServiceBookDates.ProbationDate = Convert.ToDateTime(ServiceBookDates_ProbationDate);
                            }
                            if (ServiceBookDates_ConfirmPeriod != null)
                            {
                                oServiceBookDates.ConfirmPeriod = Convert.ToInt32(ServiceBookDates_ConfirmPeriod);
                            }
                            if (ServiceBookDates_ConfirmationDate != null)
                            {
                                oServiceBookDates.ConfirmationDate = Convert.ToDateTime(ServiceBookDates_ConfirmationDate);
                            }
                            if (ServiceBookDates_LastIncrementDate != null)
                            {
                                oServiceBookDates.LastIncrementDate = Convert.ToDateTime(ServiceBookDates_LastIncrementDate);
                            }
                            if (ServiceBookDates_LastPromotionDate != null)
                            {
                                oServiceBookDates.LastPromotionDate = Convert.ToDateTime(ServiceBookDates_LastPromotionDate);
                            }
                            if (ServiceBookDates_LastTransferDate != null)
                            {
                                oServiceBookDates.LastTransferDate = Convert.ToDateTime(ServiceBookDates_LastTransferDate);
                            }
                            if (ServiceBookDates_SeniorityDate != null)
                            {
                                oServiceBookDates.SeniorityDate = Convert.ToDateTime(ServiceBookDates_SeniorityDate);
                            }
                            if (ServiceBookDates_SeniorityNo != null)
                            {
                                oServiceBookDates.SeniorityNo = ServiceBookDates_SeniorityNo;
                            }
                            if (ServiceBookDates_RetirementDate != null)
                            {
                                oServiceBookDates.RetirementDate = Convert.ToDateTime(ServiceBookDates_RetirementDate);
                            }

                            if (ServiceBookDates_ResignationDate != null)
                            {
                                oServiceBookDates.ResignationDate = Convert.ToDateTime(ServiceBookDates_ResignationDate);
                            }
                            if (ServiceBookDates_ResignReason != null)
                            {
                                oServiceBookDates.ResignReason = ServiceBookDates_ResignReason;
                            }
                            if (ServiceBookDates_ServiceLastDate != null)
                            {
                                oServiceBookDates.ServiceLastDate = Convert.ToDateTime(ServiceBookDates_ServiceLastDate);
                            }
                            if (ServiceBookDates_PFJoingDate != null)
                            {
                                oServiceBookDates.PFJoingDate = Convert.ToDateTime(ServiceBookDates_PFJoingDate);
                            }
                            if (ServiceBookDates_PensionJoingDate != null)
                            {
                                oServiceBookDates.PensionJoingDate = Convert.ToDateTime(ServiceBookDates_PensionJoingDate);
                            }
                            if (ServiceBookDates_PFExitDate != null)
                            {
                                oServiceBookDates.PFExitDate = Convert.ToDateTime(ServiceBookDates_PFExitDate);
                            }
                            if (ServiceBookDates_PensionExitDate != null)
                            {
                                oServiceBookDates.PensionExitDate = Convert.ToDateTime(ServiceBookDates_PensionExitDate);
                            }
                            errorList = ValidateObj(oServiceBookDates);
                            if (errorList != null && errorList.Count > 0)
                            {
                                return Json(new { success = false, data = rowno, responseText = string.Join(",", errorList) }, JsonRequestBehavior.AllowGet);
                            }
                            try
                            {
                                db.ServiceBookDates.Add(oServiceBookDates);
                                db.SaveChanges();
                                Emp.ServiceBookDates = oServiceBookDates;
                            }
                            catch (Exception e)
                            {
                                return Json(new { success = false, data = rowno, responseText = e.InnerException.Message.ToString() }, JsonRequestBehavior.AllowGet);
                            }
                            //personal
                            string MaritalStatus = form["MaritalStatus"] == "null" ? null : form["MaritalStatus"];
                            string Category = form["Category"] == "null" ? null : form["Category"];
                            string Caste = form["Caste"] == "null" ? null : form["Caste"];
                            string Religion = form["Religion"] == "null" ? null : form["Religion"];
                            string BloodGroup = form["BloodGroup"] == "null" ? null : form["BloodGroup"];
                            if (MaritalStatus != null)
                            {
                                var lookupdata = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "128").SingleOrDefault();
                                var qurey = lookupdata.LookupValues.Where(e => e.LookupVal.ToString().Trim().ToUpper() == MaritalStatus.ToString().Trim().ToUpper()).SingleOrDefault();
                                if (qurey != null)
                                {
                                    Emp.MaritalStatus = qurey;
                                }
                            }

                            //Emp Address 
                            string ResAddr = form["ResAddr"] == "null" ? null : form["ResAddr"];
                            string ResContact = form["ResContact"] == "null" ? null : form["ResContact"];
                            string PerAddr = form["PerAddr"] == "null" ? null : form["PerAddr"];
                            string PerContact = form["PerContact"] == "null" ? null : form["PerContact"];
                            string CorAddr = form["CorAddr"] == "null" ? null : form["CorAddr"];
                            string CorContact = form["CorContact"] == "null" ? null : form["CorContact"];
                            if (ResAddr != null)
                            {
                                var id = Convert.ToInt32(ResAddr);
                                var qurey = db.Address.Where(e => e.Id == id).SingleOrDefault();
                                Emp.ResAddr = qurey;
                            }
                            if (PerAddr != null)
                            {
                                var id = Convert.ToInt32(PerAddr);
                                var qurey = db.Address.Where(e => e.Id == id).SingleOrDefault();
                                Emp.PerAddr = qurey;
                            }
                            if (CorAddr != null)
                            {
                                var id = Convert.ToInt32(CorAddr);
                                var qurey = db.Address.Where(e => e.Id == id).SingleOrDefault();
                                Emp.CorAddr = qurey;

                            }
                            if (ResContact != null)
                            {
                                var id = Convert.ToInt32(ResContact);
                                var qurey = db.ContactDetails.Where(e => e.Id == id).SingleOrDefault();
                                Emp.ResContact = qurey;

                            }
                            if (PerContact != null)
                            {
                                var id = Convert.ToInt32(PerContact);
                                var qurey = db.ContactDetails.Where(e => e.Id == id).SingleOrDefault();
                                Emp.PerContact = qurey;

                            }
                            if (CorContact != null)
                            {
                                var id = Convert.ToInt32(CorContact);
                                var qurey = db.ContactDetails.Where(e => e.Id == id).SingleOrDefault();
                                Emp.CorContact = qurey;

                            }
                            //GeoStruct

                            string Corporate = form["Corporate"] == "null" ? null : form["Corporate"];
                            string Region = form["Region"] == "null" ? null : form["Region"];
                            string Company = form["Company"] == "null" ? null : form["Company"];
                            string Division = form["Division"] == "null" ? null : form["Division"];
                            string Location = form["Location"] == "null" ? null : form["Location"];
                            string Department = form["Department"] == "null" ? null : form["Department"];
                            string Group = form["Group"] == "null" ? null : form["Group"];
                            string Unit = form["Unit"] == "null" ? null : form["Unit"];

                            var Get_Geo_Id = db.GeoStruct.Include(e => e.Company)
                                .Include(e => e.Corporate).Include(e => e.Department).Include(e => e.Department.DepartmentObj)
                                .Include(e => e.Division).Include(e => e.Group).Include(e => e.Location).Include(e => e.Location.LocationObj).Include(e => e.Region).Include(e => e.Unit)
                                .ToList();
                            if (Corporate != null)
                            {
                                var id = Corporate.Trim();
                                Get_Geo_Id = Get_Geo_Id.Where(e => e.Corporate != null && e.Corporate.Code == id).ToList();
                            }
                            if (Region != null)
                            {
                                var id = Region.Trim();
                                Get_Geo_Id = Get_Geo_Id.Where(e => e.Region != null && e.Region.Code == id).ToList();
                            }
                            if (Company != null)
                            {
                                var id = Company.Trim();
                                Get_Geo_Id = Get_Geo_Id.Where(e => e.Company != null && e.Company.Code == id).ToList();
                            }
                            if (Division != null)
                            {
                                var id = Division.Trim();
                                Get_Geo_Id = Get_Geo_Id.Where(e => e.Division != null && e.Division.Code == id).ToList();
                            }
                            if (Location != null)
                            {
                                var id = Location.Trim();
                                Get_Geo_Id = Get_Geo_Id.Where(e => e.Location != null && e.Location.LocationObj != null && e.Location.LocationObj.LocCode == id).ToList();
                            }
                            if (Department != null)
                            {
                                var id = Department.Trim();
                                Get_Geo_Id = Get_Geo_Id.Where(e => e.Department != null && e.Department.DepartmentObj != null && e.Department.DepartmentObj.DeptCode == id).ToList();
                            }
                            if (Group != null)
                            {
                                var id = Group.Trim();
                                Get_Geo_Id = Get_Geo_Id.Where(e => e.Group != null && e.Group.Code == id).ToList();
                            }
                            if (Unit != null)
                            {
                                var id = Unit.Trim();
                                Get_Geo_Id = Get_Geo_Id.Where(e => e.Unit != null && e.Unit.Code == id).ToList();
                            }
                            if (Get_Geo_Id.Count == 0)
                            {
                                errorList.Add("Location Code is not Configure ");
                                return Json(new { success = false, data = rowno, responseText = string.Join(",", errorList) }, JsonRequestBehavior.AllowGet);
                            }
                            if (Get_Geo_Id != null && Get_Geo_Id.Count >= 1)
                            {
                                Emp.GeoStruct = Get_Geo_Id[0];
                            }

                            //paystruct
                            var Get_Pay_Id = db.PayStruct.Include(e => e.Company).Include(e => e.Grade).Include(e => e.JobStatus).Include(e => e.Level).ToList();
                            string Grade = form["Grade"] == "null" ? null : form["Grade"];
                            string Level = form["Level"] == "null" ? null : form["Level"];
                            string JobStatus = form["JobStatus"] == "null" ? null : form["JobStatus"];
                            if (Grade != null)
                            {
                                var id = Grade.Trim();
                                Get_Pay_Id = Get_Pay_Id.Where(e => e.Grade != null && e.Grade.Code == id).ToList();
                            }
                            if (Level != null)
                            {
                                var id = Level.Trim();
                                Get_Pay_Id = Get_Pay_Id.Where(e => e.Level != null && e.Level.Code == id).ToList();
                            }
                            if (JobStatus != null)
                            {
                                var id = Convert.ToInt32(JobStatus.Trim());
                                Get_Pay_Id = Get_Pay_Id.Where(e => e.JobStatus != null && e.JobStatus.Id == id).ToList();
                            }
                            if (Get_Pay_Id.Count == 0)
                            {
                                errorList.Add("Grade Code is not Configure ");
                                return Json(new { success = false, data = rowno, responseText = string.Join(",", errorList) }, JsonRequestBehavior.AllowGet);
                            }
                            if (Get_Pay_Id != null && Get_Pay_Id.Count >= 1)
                            {
                                Emp.PayStruct = Get_Pay_Id[0];
                            }
                            //Funstrct
                            string Job = form["Job"] == "null" ? null : form["Job"];
                            string Position = form["Position"] == "null" ? null : form["Position"];
                            var Get_Fun_Id = db.FuncStruct.Include(e => e.Company).Include(e => e.Job).Include(e => e.JobPosition).ToList();
                            if (Job != null)
                            {
                                var id = Job.Trim();
                                Get_Fun_Id = Get_Fun_Id.Where(e => e.Job != null && e.Job.Code == id).ToList();
                            }
                            if (Position != null)
                            {
                                var id = Position.Trim();
                                Get_Fun_Id = Get_Fun_Id.Where(e => e.JobPosition != null && e.JobPosition.JobPositionCode == id).ToList();
                            }

                            if (Get_Fun_Id.Count == 0)
                            {
                                errorList.Add("Job Code is not Configure ");
                                return Json(new { success = false, data = rowno, responseText = string.Join(",", errorList) }, JsonRequestBehavior.AllowGet);
                            }

                            if (Get_Fun_Id != null && Get_Fun_Id.Count >= 1)
                            {
                                Emp.FuncStruct = Get_Fun_Id[0];
                            }

                            var Id = Convert.ToInt32(SessionManager.CompanyId);
                            var CompCode = db.Company.Where(e => e.Id == Id).SingleOrDefault();
                            if (CompCode != null)
                            {
                                if (CompCode.Code.ToUpper() == "KDCC" && CompCode.RegistrationDate != null)
                                {
                                    string dob = "01/01/1940";
                                    if (Emp.ServiceBookDates.JoiningDate < CompCode.RegistrationDate.Value.Date)
                                    {
                                        errorList.Add("Joining date Should be Grater Than bank Registration Date");
                                        return Json(new { success = false, data = rowno, responseText = string.Join(",", errorList) }, JsonRequestBehavior.AllowGet);
                                    }
                                    if (Emp.ServiceBookDates.BirthDate < Convert.ToDateTime(dob).Date)
                                    {
                                        errorList.Add("Birth date Should be Grater Than 01/01/1940");
                                        return Json(new { success = false, data = rowno, responseText = string.Join(",", errorList) }, JsonRequestBehavior.AllowGet);
                                    }

                                }
                            }
                            using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, new System.TimeSpan(0, 60, 0)))
                            {
                                Emp.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                                Employee employee = new Employee()
                                {
                                    EmpCode = Emp.EmpCode == "null" ? null : Emp.EmpCode.Trim(),
                                    EmpName = Emp.EmpName,
                                    FatherName = Emp.FatherName != null ? Emp.FatherName : null,
                                    HusbandName = Emp.HusbandName != null ? Emp.FatherName : null,
                                    MotherName = Emp.MotherName != null ? Emp.FatherName : null,
                                    BeforeMarriageName = Emp.BeforeMarriageName != null ? Emp.FatherName : null,
                                    BirthPlace = Emp.BirthPlace == null ? null : Emp.BirthPlace.Trim(),
                                    CardCode = Emp.CardCode != null ? Emp.CardCode : null,
                                    CorAddr = Emp.CorAddr != null ? Emp.CorAddr : null,
                                    CorContact = Emp.CorContact != null ? Emp.CorContact : null,
                                    EMPRESIGNSTAT = Emp.EMPRESIGNSTAT != null ? Emp.EMPRESIGNSTAT : false,
                                    FuncStruct = Emp.FuncStruct != null ? Emp.FuncStruct : null,
                                    Gender = Emp.Gender != null ? Emp.Gender : null,
                                    GeoStruct = Emp.GeoStruct != null ? Emp.GeoStruct : null,
                                    MaritalStatus = Emp.MaritalStatus != null ? Emp.MaritalStatus : null,
                                    PayStruct = Emp.PayStruct != null ? Emp.PayStruct : null,
                                    PerAddr = Emp.PerAddr != null ? Emp.PerAddr : null,
                                    PerContact = Emp.PerContact != null ? Emp.PerContact : null,
                                    ResAddr = Emp.ResAddr != null ? Emp.ResAddr : null,
                                    ResContact = Emp.ResContact != null ? Emp.ResContact : null,
                                    ServiceBookDates = Emp.ServiceBookDates != null ? Emp.ServiceBookDates : null,
                                    DBTrack = Emp.DBTrack
                                };
                                errorList = ValidateObj(employee);
                                if (errorList.Count > 0)
                                {
                                    return Json(new { success = false, data = rowno, responseText = string.Join(",", errorList) }, JsonRequestBehavior.AllowGet);

                                }
                                try
                                {
                                    db.Employee.Add(employee);
                                    var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, Emp.DBTrack);
                                    DT_Employee DT_Emp = (DT_Employee)rtn_Obj;
                                    DT_Emp.EmpName_Id = Emp.EmpName == null ? 0 : Emp.EmpName.Id;
                                    DT_Emp.MotherName_Id = Emp.MotherName == null ? 0 : Emp.MotherName.Id;
                                    DT_Emp.FatherName_Id = Emp.FatherName == null ? 0 : Emp.FatherName.Id;
                                    DT_Emp.HusbandName_Id = Emp.HusbandName == null ? 0 : Emp.HusbandName.Id;
                                    DT_Emp.BeforeMarriageName_Id = Emp.BeforeMarriageName == null ? 0 : Emp.BeforeMarriageName.Id;
                                    DT_Emp.GeoStruct_Id = Emp.GeoStruct == null ? 0 : Emp.GeoStruct.Id;
                                    DT_Emp.FuncStruct_Id = Emp.FuncStruct == null ? 0 : Emp.FuncStruct.Id;
                                    DT_Emp.PayStruct_Id = Emp.PayStruct == null ? 0 : Emp.PayStruct.Id;
                                    DT_Emp.CorAddr_Id = Emp.CorAddr == null ? 0 : Emp.CorAddr.Id;
                                    DT_Emp.CorContact_Id = Emp.CorContact == null ? 0 : Emp.CorContact.Id;
                                    DT_Emp.PerAddr_Id = Emp.PerAddr == null ? 0 : Emp.PerAddr.Id;
                                    DT_Emp.PerContact_Id = Emp.PerContact == null ? 0 : Emp.PerContact.Id;
                                    DT_Emp.ResAddr_Id = Emp.ResAddr == null ? 0 : Emp.ResAddr.Id;
                                    DT_Emp.ResContact_Id = Emp.ResContact == null ? 0 : Emp.ResContact.Id;
                                    DT_Emp.ServiceBookDates_Id = Emp.ServiceBookDates == null ? 0 : Emp.ServiceBookDates.Id;
                                    //  DT_Emp.Nominees_Id = Emp.Nominees == null ? 0 : Emp.Nominees.Id;
                                    DT_Emp.EmpmedicalInfo_Id = Emp.EmpmedicalInfo == null ? 0 : Emp.EmpmedicalInfo.Id;
                                    DT_Emp.EmpSocialInfo_Id = Emp.EmpSocialInfo == null ? 0 : Emp.EmpSocialInfo.Id;
                                    DT_Emp.EmpAcademicInfo_Id = Emp.EmpAcademicInfo == null ? 0 : Emp.EmpAcademicInfo.Id;
                                    //DT_Emp.PassportDetails_Id = Emp.PassportDetails == null ? 0 : Emp.PassportDetails.Id;
                                    //DT_Emp.VisaDetails_Id = Emp.VisaDetails == null ? 0 : Emp.VisaDetails.Id;
                                    //DT_Emp.ForeignTrips_Id = Emp.ForeignTrips == null ? 0 : Emp.ForeignTrips.Id;
                                    //DT_Emp.FamilyDetails_Id = Emp.FamilyDetails == null ? 0 : Emp.FamilyDetails.Id;
                                    //DT_Emp.GuarantorDetails_Id = Emp.GuarantorDetails == null ? 0 : Emp.GuarantorDetails.Id;
                                    //DT_Emp.InsuranceDetails_Id = Emp.InsuranceDetails == null ? 0 : Emp.InsuranceDetails.Id;
                                    DT_Emp.Gender_Id = Emp.Gender == null ? 0 : Emp.Gender.Id;
                                    DT_Emp.MaritalStatus_Id = Emp.MaritalStatus == null ? 0 : Emp.MaritalStatus.Id;

                                    db.Create(DT_Emp);
                                    db.SaveChanges();
                                    //employee.GeoStruct.Company.Id

                                    if (Convert.ToString(Session["CompId"]) != null)
                                    {
                                        var oEmployeePayroll = new EmployeePayroll();
                                        oEmployeePayroll.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                        oEmployeePayroll.Employee = employee;
                                        db.SaveChanges();

                                        var oEmployeeLeave = new EmployeeLeave();
                                        oEmployeeLeave.Employee = employee;
                                        oEmployeeLeave.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                        db.EmployeeLeave.Add(oEmployeeLeave);
                                        db.SaveChanges();

                                        var oEmployeeAppraisal = new EmployeeAppraisal();
                                        oEmployeeAppraisal.Employee = employee;
                                        oEmployeeAppraisal.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                        db.EmployeeAppraisal.Add(oEmployeeAppraisal);
                                        db.SaveChanges();

                                        var oEmployeeTraining = new EmployeeTraining();
                                        oEmployeeTraining.Employee = employee;
                                        oEmployeeTraining.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                        db.EmployeeTraining.Add(oEmployeeTraining);
                                        db.SaveChanges();

                                        var oEmployeeAttendance = new EmployeeAttendance();
                                        oEmployeeAttendance.Employee = employee;
                                        oEmployeeAttendance.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                        db.EmployeeAttendance.Add(oEmployeeAttendance);
                                        db.SaveChanges();

                                        //attach comp leave
                                        var id = int.Parse(Convert.ToString(Session["CompId"]));
                                        var CompLvData = db.CompanyLeave.Where(e => e.Company.Id == id).SingleOrDefault();
                                        List<EmployeeLeave> ListEmployeeLeave = new List<EmployeeLeave>();
                                        ListEmployeeLeave.Add(oEmployeeLeave);
                                        CompLvData.EmployeeLeave = ListEmployeeLeave;
                                        db.Entry(CompLvData).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();


                                        var company_data = db.CompanyPayroll.Where(e => e.Company.Id == id).SingleOrDefault();
                                        List<EmployeePayroll> emp = new List<EmployeePayroll>();
                                        emp.Add(oEmployeePayroll);
                                        company_data.EmployeePayroll = emp;
                                        db.Entry(company_data).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();

                                        var companyTraining = db.CompanyTraining.Where(e => e.Company.Id == id).SingleOrDefault();
                                        List<EmployeeTraining> empTraining = new List<EmployeeTraining>();
                                        empTraining.Add(oEmployeeTraining);
                                        companyTraining.EmployeeTraining = empTraining;
                                        db.Entry(company_data).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();

                                        var companyAppraisal = db.CompanyAppraisal.Where(e => e.Company.Id == id).SingleOrDefault();
                                        List<EmployeeAppraisal> empApparaisal = new List<EmployeeAppraisal>();
                                        empApparaisal.Add(oEmployeeAppraisal);
                                        companyAppraisal.EmployeeAppraisal = empApparaisal;
                                        db.Entry(company_data).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();

                                        var companyAttendance = db.CompanyAttendance.Where(e => e.Company.Id == id).SingleOrDefault();
                                        List<EmployeeAttendance> empAttendance = new List<EmployeeAttendance>();
                                        empAttendance.Add(oEmployeeAttendance);
                                        companyAttendance.EmployeeAttendance = empAttendance;
                                        db.Entry(company_data).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();

                                    }
                                    ts.Complete();
                                    return this.Json(new { success = true, data = rowno, responseText = "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
                                }
                                catch (DbUpdateConcurrencyException)
                                {
                                    return RedirectToAction("Create", new { concurrencyError = true, id = Emp.Id });
                                }
                                catch (DataException e)
                                {
                                    return this.Json(new { success = false, data = rowno, responseText = e.InnerException.Message.ToString() }, JsonRequestBehavior.AllowGet);
                                }
                            }
                        }
                    case "LOCATION":
                        using (DataBaseContext db = new DataBaseContext())
                        {
                            int comp_Id = 0;
                            comp_Id = Convert.ToInt32(Session["CompId"]);
                            var Company = new Company();
                            Company = db.Company.Where(e => e.Id == comp_Id).SingleOrDefault();

                            string LocCode = form["LocCode"] == null ? null : form["LocCode"];
                            string LocDesc = form["LocDesc"] == null ? null : form["LocDesc"].Replace("#", " ").Trim();
                            string OpeningDate = form["OpeningDate"] == null ? null : form["OpeningDate"];
                            string Type = form["Type"] == null ? null : form["Type"].Replace("#", " ").Trim();
                            string Address1 = form["Address1"] == null ? "" : form["Address1"].Replace("#", " ").Trim();
                            string Address2 = form["Address2"] == null ? "" : form["Address2"].Replace("#", " ").Trim();
                            string Address3 = form["Address3"] == null ? "" : form["Address3"].Replace("#", " ").Trim();
                            string Landmark = form["Landmark"] == null ? "" : form["Landmark"].Replace("#", " ").Trim();
                            string AreaCode = form["AreaCode"] == null ? "" : form["AreaCode"];
                            string AreaName = form["AreaName"] == null ? "" : form["AreaName"].Replace("#", " ").Trim();
                            int AreaPincode = form["AreaPincode"] == null ? 0 : Convert.ToInt32(form["AreaPincode"]);
                            string CityCategory = form["CityCategory"] == null ? "" : form["CityCategory"];
                            string CityCode = form["CityCode"] == null ? "" : form["CityCode"];
                            string CityName = form["CityName"] == null ? "" : form["CityName"].Replace("#", " ").Trim();
                            string TalukaCode = form["TalukaCode"] == null ? "" : form["TalukaCode"];
                            string TalukaName = form["TalukaName"] == null ? "" : form["TalukaName"].Replace("#", " ").Trim();
                            string DistrictCode = form["DistrictCode"] == null ? "" : form["DistrictCode"];
                            string DistrictName = form["DistrictName"] == null ? "" : form["DistrictName"].Replace("#", " ").Trim();
                            string StateRegionCode = form["StateRegionCode"] == null ? "" : form["StateRegionCode"];
                            string StateRegionName = form["StateRegionName"] == null ? "" : form["StateRegionName"].Replace("#", " ").Trim();
                            string StateCode = form["StateCode"] == null ? "" : form["StateCode"];
                            string StateName = form["StateName"] == null ? "" : form["StateName"].Replace("#", " ").Trim();
                            string CountryCode = form["CountryCode"] == null ? "" : form["CountryCode"];
                            string CountryName = form["CountryName"] == null ? "" : form["CountryName"].Replace("#", " ").Trim();
                            string EmailId = form["EmailId"] == null ? "" : form["EmailId"];
                            string Website = form["Website"] == null ? "" : form["Website"];
                            string FaxNo = form["FaxNo"] == null ? "" : form["FaxNo"];
                            string MobileNo = form["MobileNo"] == null ? "" : form["MobileNo"];
                            string STDCode = form["STDCode"] == null ? "" : form["STDCode"];
                            string LandlineNo = form["LandlineNo"] == null ? "" : form["LandlineNo"];

                            Location loc = new Location();
                            string Incharge_DDL = form["Incharge"] == null ? null : form["Incharge"];
                            if (Incharge_DDL != null && Incharge_DDL != "")
                            {
                                var value = db.Employee.Where(e => e.EmpCode == Incharge_DDL).FirstOrDefault();
                                if (value != null)
                                {
                                    loc.Incharge_Id = value.Id;
                                }


                            }
                            if (Type != null)
                            {
                                var val = db.LookupValue.Where(q => q.LookupVal == Type).FirstOrDefault();
                                if (val != null)
                                {
                                    loc.Type_Id = val.Id;
                                }
                            }


                            if (OpeningDate != null)
                            {
                                loc.OpeningDate = Convert.ToDateTime(OpeningDate);
                            }


                            using (TransactionScope ts = new TransactionScope())
                            {
                                loc.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                                LocationObj Locationobj = new LocationObj();
                                LocationObj OLocationobj = null;
                                if (LocCode != "" && LocDesc != "")
                                {
                                    //*********** Check LocCode already exists **************
                                    LocationObj checkExists = db.LocationObj.Where(e => e.LocCode == LocCode).FirstOrDefault();
                                    if (checkExists != null)
                                    {
                                        errorList.Add("LocCode is already Exists ");
                                        return Json(new { success = false, data = rowno, responseText = string.Join(",", errorList) }, JsonRequestBehavior.AllowGet);
                                    }

                                    OLocationobj = db.LocationObj.Where(e => e.LocCode.ToUpper().ToString() == LocCode.ToUpper().ToString() && e.LocDesc.ToUpper().ToString() == LocDesc.ToUpper().ToString()).FirstOrDefault();

                                    if (OLocationobj == null)
                                    {
                                        Locationobj = new LocationObj()
                                        {
                                            LocCode = LocCode,
                                            LocDesc = LocDesc,
                                            DBTrack = loc.DBTrack
                                        };

                                        errorList = ValidateObj(Locationobj);
                                        if (errorList.Count > 0)
                                        {
                                            return Json(new { success = false, data = rowno, responseText = string.Join(",", errorList) }, JsonRequestBehavior.AllowGet);
                                        }

                                        db.LocationObj.Add(Locationobj);
                                        db.SaveChanges();
                                        loc.LocationObj_Id = Locationobj.Id;
                                    }
                                    else
                                    {
                                        loc.LocationObj_Id = OLocationobj.Id;
                                    }

                                }

                                Address Address = new Address();
                                Address.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                Area area = new Area();
                                Area OArea = null;
                                if (AreaCode != "" && AreaName != "")
                                {
                                    OArea = db.Area.Where(e => e.Code.ToUpper() == AreaCode.ToUpper() && e.Name.ToUpper() == AreaName.ToUpper() && e.PinCode == AreaPincode).FirstOrDefault();
                                    if (OArea == null)
                                    {
                                        area = new Area()
                                        {
                                            Code = AreaCode,
                                            Name = AreaName,
                                            PinCode = AreaPincode,
                                            DBTrack = Address.DBTrack
                                        };

                                        errorList = ValidateObj(area);
                                        if (errorList.Count > 0)
                                        {
                                            return Json(new { success = false, data = rowno, responseText = string.Join(",", errorList) }, JsonRequestBehavior.AllowGet);
                                        }

                                        db.Area.Add(area);
                                        db.SaveChanges();
                                        Address.Area = area;
                                    }
                                    else
                                    {
                                        Address.Area = OArea;
                                    }
                                }
                                City city = new City();
                                City OCity = null;
                                if (CityCode != null && CityName != "")
                                {
                                    OCity = db.City.Include(e => e.Areas).Where(e => e.Code.ToUpper() == CityCode.ToUpper() && e.Name.ToUpper() == CityName.ToUpper()).FirstOrDefault();
                                    int CatId = db.City.Include(e => e.Category).Where(e => e.Category.LookupVal.ToUpper() == CityCategory).FirstOrDefault() != null ? db.City.Include(e => e.Category).Where(e => e.Category.LookupVal.ToUpper() == CityCategory.ToUpper()).FirstOrDefault().Category.Id : 0;
                                    if (OCity == null)
                                    {
                                        city = new City()
                                        {
                                            Code = CityCode,
                                            Name = CityName,
                                            Category = db.LookupValue.Find(CatId),
                                            DBTrack = Address.DBTrack
                                        };
                                        errorList = ValidateObj(city);
                                        if (errorList.Count > 0)
                                        {
                                            return Json(new { success = false, data = rowno, responseText = string.Join(",", errorList) }, JsonRequestBehavior.AllowGet);
                                        }

                                        db.City.Add(city);
                                        db.SaveChanges();

                                        List<Area> OAreaList = new List<Area>();
                                        if (area.Id != 0)
                                        {
                                            OAreaList.Add(area);
                                            if (city.Areas != null && city.Areas.Count() > 0)
                                            {
                                                OAreaList.AddRange(city.Areas);
                                            }
                                            city.Areas = OAreaList;
                                            db.City.Attach(city);
                                            db.Entry(city).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();

                                        }

                                        Address.City = city;
                                    }
                                    else
                                    {
                                        List<Area> OAreaList = new List<Area>();

                                        if (area.Id != 0)
                                        {
                                            OAreaList.Add(area);
                                            if (OCity.Areas.Count() > 0 && !OCity.Areas.Any(t => t.Code.ToUpper() == area.Code.ToUpper()))
                                            {
                                                OAreaList.AddRange(OCity.Areas);
                                                OCity.Areas = OAreaList;
                                                db.City.Attach(OCity);
                                                db.Entry(OCity).State = System.Data.Entity.EntityState.Modified;
                                                db.SaveChanges();
                                                Address.City = OCity;
                                            }
                                        }



                                        Address.City = OCity;

                                    }
                                }

                                Taluka taluka = new Taluka();
                                Taluka OTaluka = null;
                                if (TalukaCode != "" && TalukaName != "")
                                {
                                    OTaluka = db.Taluka.Include(e => e.Cities).Where(e => e.Code.ToUpper() == TalukaCode.ToUpper() && e.Name.ToUpper() == TalukaName.ToUpper()).FirstOrDefault();
                                    if (OTaluka == null)
                                    {
                                        taluka = new Taluka()
                                        {
                                            Code = TalukaCode,
                                            Name = TalukaName,
                                            DBTrack = Address.DBTrack
                                        };

                                        errorList = ValidateObj(taluka);
                                        if (errorList.Count > 0)
                                        {
                                            return Json(new { success = false, data = rowno, responseText = string.Join(",", errorList) }, JsonRequestBehavior.AllowGet);
                                        }

                                        db.Taluka.Add(taluka);
                                        db.SaveChanges();

                                        List<City> OACityList = new List<City>();
                                        if (city.Id != 0)
                                        {
                                            OACityList.Add(city);
                                        }

                                        if (taluka.Cities != null && taluka.Cities.Count() > 0)
                                        {
                                            OACityList.AddRange(taluka.Cities);
                                        }
                                        taluka.Cities = OACityList;
                                        db.Taluka.Attach(taluka);
                                        db.Entry(taluka).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                        Address.Taluka = taluka;
                                    }
                                    else
                                    {

                                        List<City> OACityList = new List<City>();
                                        if (city.Id != 0)
                                        {
                                            OACityList.Add(city);
                                            if (OTaluka.Cities.Count() > 0 && !OTaluka.Cities.Any(t => t.Code.ToUpper() == city.Code.ToUpper()))
                                            {
                                                OTaluka.Cities = OACityList;
                                                db.Taluka.Attach(OTaluka);
                                                db.Entry(OTaluka).State = System.Data.Entity.EntityState.Modified;
                                                db.SaveChanges();
                                            }

                                        }
                                        Address.Taluka = OTaluka;
                                    }
                                }

                                District district = new District();
                                District ODistrict = null;
                                if (DistrictCode != "" && DistrictName != "")
                                {
                                    ODistrict = db.District.Include(e => e.Talukas).Where(e => e.Code.ToUpper() == DistrictCode.ToUpper() && e.Name.ToUpper() == DistrictName.ToUpper()).FirstOrDefault();
                                    if (ODistrict == null)
                                    {
                                        district = new District()
                                        {
                                            Code = DistrictCode,
                                            Name = DistrictName,
                                            DBTrack = Address.DBTrack
                                        };
                                        errorList = ValidateObj(district);
                                        if (errorList.Count > 0)
                                        {
                                            return Json(new { success = false, data = rowno, responseText = string.Join(",", errorList) }, JsonRequestBehavior.AllowGet);
                                        }

                                        db.District.Add(district);
                                        db.SaveChanges();

                                        List<Taluka> OTalukaList = new List<Taluka>();
                                        OTalukaList.Add(taluka);
                                        if (district.Talukas != null && district.Talukas.Count() > 0)
                                        {
                                            OTalukaList.AddRange(district.Talukas);
                                        }
                                        district.Talukas = OTalukaList;
                                        db.District.Attach(district);
                                        db.Entry(district).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                        Address.District = district;
                                    }
                                    else
                                    {
                                        List<Taluka> OTalukaList = new List<Taluka>();

                                        if (taluka.Id != 0)
                                        {
                                            if (ODistrict.Talukas.Count() > 0 && !ODistrict.Talukas.Any(t => t.Code.ToUpper() == taluka.Code.ToUpper()))
                                            {
                                                OTalukaList.AddRange(ODistrict.Talukas);

                                                OTalukaList.Add(taluka);

                                                ODistrict.Talukas = OTalukaList;
                                                db.District.Attach(ODistrict);
                                                db.Entry(ODistrict).State = System.Data.Entity.EntityState.Modified;
                                                db.SaveChanges();
                                            }
                                        }
                                        Address.District = ODistrict;
                                    }
                                }

                                StateRegion stateRegion = new StateRegion();
                                StateRegion OStateRegion = null;
                                if (StateRegionCode != "" && StateRegionName != "")
                                {
                                    OStateRegion = db.StateRegion.Include(e => e.Districts).Where(e => e.Code.ToUpper() == StateRegionCode.ToUpper() && e.Name.ToUpper() == StateRegionName.ToUpper()).FirstOrDefault();
                                    if (OStateRegion == null)
                                    {
                                        stateRegion = new StateRegion()
                                        {
                                            Code = StateRegionCode,
                                            Name = StateRegionName,
                                            DBTrack = Address.DBTrack
                                        };

                                        errorList = ValidateObj(stateRegion);
                                        if (errorList.Count > 0)
                                        {
                                            return Json(new { success = false, data = rowno, responseText = string.Join(",", errorList) }, JsonRequestBehavior.AllowGet);
                                        }

                                        db.StateRegion.Add(stateRegion);
                                        db.SaveChanges();
                                        if (district.Id != 0)
                                        {

                                            List<District> OADistrictList = new List<District>();
                                            OADistrictList.Add(district);
                                            if (stateRegion.Districts != null && stateRegion.Districts.Count() > 0)
                                            {
                                                OADistrictList.AddRange(stateRegion.Districts);
                                            }
                                            stateRegion.Districts = OADistrictList;
                                            db.StateRegion.Attach(stateRegion);
                                            db.Entry(stateRegion).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();
                                        }
                                        Address.StateRegion = stateRegion;
                                    }
                                    else
                                    {
                                        List<District> OADistrictList = new List<District>();

                                        if (OStateRegion.Districts.Count() > 0 && !OStateRegion.Districts.Any(t => t.Code.ToUpper() == ODistrict.Code.ToUpper()))
                                        {
                                            OADistrictList.AddRange(OStateRegion.Districts);
                                            OADistrictList.Add(district);
                                            OStateRegion.Districts = OADistrictList;
                                            db.StateRegion.Attach(OStateRegion);
                                            db.Entry(OStateRegion).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();
                                        }
                                        Address.StateRegion = OStateRegion;
                                    }
                                }


                                State state = new State();
                                State OState = null;
                                if (StateCode != "" && StateName != "")
                                {
                                    OState = db.State.Include(e => e.StateRegions).Where(e => e.Code.ToUpper() == StateCode.ToUpper() && e.Name.ToUpper() == StateName.ToUpper()).FirstOrDefault();
                                    if (OState == null)
                                    {
                                        state = new State()
                                        {
                                            Code = StateCode,
                                            Name = StateName,
                                            DBTrack = Address.DBTrack
                                        };

                                        errorList = ValidateObj(state);
                                        if (errorList.Count > 0)
                                        {
                                            return Json(new { success = false, data = rowno, responseText = string.Join(",", errorList) }, JsonRequestBehavior.AllowGet);
                                        }

                                        db.State.Add(state);
                                        db.SaveChanges();

                                        if (stateRegion.Id != 0)
                                        {

                                            List<StateRegion> OStateRegionList = new List<StateRegion>();
                                            OStateRegionList.Add(stateRegion);
                                            if (state.StateRegions != null && state.StateRegions.Count() > 0)
                                            {
                                                OStateRegionList.AddRange(state.StateRegions);
                                            }
                                            state.StateRegions = OStateRegionList;
                                            db.State.Attach(state);
                                            db.Entry(state).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();
                                        }
                                        Address.State = state;
                                    }
                                    else
                                    {
                                        List<StateRegion> OStateRegionList = new List<StateRegion>();

                                        if (stateRegion.Id != 0)
                                        {
                                            OStateRegionList.Add(stateRegion);
                                        }
                                        if (OState.StateRegions.Count() > 0 && !OState.StateRegions.Any(t => t.Code == stateRegion.Code))
                                        {
                                            OStateRegionList.AddRange(OState.StateRegions);
                                            OState.StateRegions = OStateRegionList;
                                            db.State.Attach(OState);
                                            db.Entry(OState).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();
                                        }

                                        Address.State = OState;
                                    }

                                }


                                Country country = new Country();
                                Country OCountry = null;
                                if (CountryCode != "")
                                {
                                    OCountry = db.Country.Include(e => e.States).Where(e => e.Code.ToUpper() == CountryCode).FirstOrDefault();

                                    if (OCountry == null)
                                    {
                                        country = new Country()
                                        {
                                            Code = CountryCode,
                                            Name = CountryName,
                                            DBTrack = Address.DBTrack
                                        };
                                        errorList = ValidateObj(country);
                                        if (errorList.Count > 0)
                                        {
                                            return Json(new { success = false, data = rowno, responseText = string.Join(",", errorList) }, JsonRequestBehavior.AllowGet);
                                        }
                                        db.Country.Add(country);
                                        db.SaveChanges();

                                        if (state.Id != 0)
                                        {
                                            List<State> OStateList = new List<State>();
                                            OStateList.Add(state);
                                            if (country.States != null && country.States.Count() > 0)
                                            {
                                                OStateList.AddRange(country.States);
                                            }
                                            country.States = OStateList;
                                            db.State.Attach(state);
                                            db.Entry(state).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();
                                        }
                                        Address.Country = country;
                                    }
                                    else
                                    {
                                        List<State> OStateList = new List<State>();
                                        //OStateList.Add(state);
                                        if (OCountry.States.Count() > 0 && !OCountry.States.Any(t => t.Code == state.Code))
                                        {
                                            OStateList.AddRange(OCountry.States);
                                            OCountry.States = OStateList;
                                            db.Country.Attach(OCountry);
                                            db.Entry(OCountry).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();
                                        }

                                        Address.Country = OCountry;
                                    }
                                }

                                ContactDetails ContDetails = new ContactDetails();
                                ContactNumbers OContNos = new ContactNumbers();
                                List<ContactNumbers> OContNolist = new List<ContactNumbers>();
                                ContactDetails OContDetails = null;
                                if (EmailId != null || OContNos != null)
                                { OContDetails = db.ContactDetails.Where(e => e.EmailId == EmailId).FirstOrDefault(); }


                                Address OAddress = null;


                                if (Address1 != "" && Address2 != "" && Address3 != "" && Landmark != "")
                                {
                                    OAddress = db.Address.Where(e => e.Address1.ToUpper() == Address1.ToUpper() && e.Address2.ToUpper() == Address2.ToUpper() && e.Address3.ToUpper() == Address3.ToUpper() && e.Landmark.ToUpper() == Landmark.ToUpper()).FirstOrDefault();
                                    if (OAddress == null)
                                    {
                                        Address = new Address()
                                        {
                                            Address1 = Address1,
                                            Address2 = Address2,
                                            Address3 = Address3,
                                            Landmark = Landmark,
                                            Country = Address.Country,
                                            State = Address.State,
                                            StateRegion = Address.StateRegion,
                                            District = Address.District,
                                            Taluka = Address.Taluka,
                                            Area = Address.Area,
                                            City = Address.City,
                                            DBTrack = loc.DBTrack


                                        };

                                        errorList = ValidateObj(Locationobj);
                                        if (errorList.Count > 0)
                                        {
                                            return Json(new { success = false, data = rowno, responseText = string.Join(",", errorList) }, JsonRequestBehavior.AllowGet);
                                        }

                                        db.Address.Add(Address);
                                        db.SaveChanges();
                                        loc.Address_Id = Address.Id;
                                    }
                                    else
                                    {
                                        loc.Address_Id = OAddress.Id;
                                    }

                                }


                                try
                                {
                                    Location Objloc = new Location();
                                    {

                                        Objloc.Incharge_Id = loc.Incharge_Id;
                                        Objloc.Type_Id = loc.Type_Id;
                                        Objloc.OpeningDate = loc.OpeningDate;
                                        Objloc.LocationObj_Id = loc.LocationObj_Id;
                                        Objloc.Address_Id = loc.Address_Id;
                                        Objloc.DBTrack = loc.DBTrack;

                                    }

                                    errorList = ValidateObj(Objloc);
                                    if (errorList.Count > 0)
                                    {
                                        return Json(new { success = false, data = rowno, responseText = string.Join(",", errorList) }, JsonRequestBehavior.AllowGet);
                                    }

                                    db.Location.Add(Objloc);
                                    db.SaveChanges();
                                    var Objjob = new List<Location>();
                                    Objjob.Add(Objloc);
                                    Company.Location = Objjob;
                                    db.Company.Attach(Company);
                                    db.Entry(Company).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    db.Entry(Company).State = System.Data.Entity.EntityState.Detached;

                                    ts.Complete();
                                }
                                catch (DataException ex)
                                {
                                    LogFile Logfile = new LogFile();
                                    ErrorLog Err = new ErrorLog()
                                    {
                                        ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                                        ExceptionMessage = ex.Message,
                                        ExceptionStackTrace = ex.StackTrace,
                                        LineNo = Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                                        LogTime = DateTime.Now
                                    };
                                    Logfile.CreateLogFile(Err);
                                    return Json(new { sucess = false, responseText = ex.InnerException }, JsonRequestBehavior.AllowGet);
                                }
                                catch (Exception ex)
                                {
                                    LogFile Logfile = new LogFile();
                                    ErrorLog Err = new ErrorLog()
                                    {
                                        ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                                        ExceptionMessage = ex.Message,
                                        ExceptionStackTrace = ex.StackTrace,
                                        LineNo = Convert.ToString(new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                                        LogTime = DateTime.Now
                                    };
                                    Logfile.CreateLogFile(Err);
                                }
                            }
                        }
                        return Json(new { data = rowno, success = true, responseText = "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);



                    case "ITFORM24QFILEFORMATDEFINITION":
                        using (DataBaseContext db = new DataBaseContext())
                        {
                            ITForm24QFileFormatDefinition c = new ITForm24QFileFormatDefinition();

                            string SrNo = form["SrNo"] == null ? null : form["SrNo"];
                            string Form24QFileType = form["Form24QFileType"] == null ? null : form["Form24QFileType"];
                            string DataType = form["DataType"] == null ? null : form["DataType"];
                            string Sizeit = form["Size"] == null ? null : form["Size"];
                            string ExcelColNo = form["ExcelColNo"] == null ? null : form["ExcelColNo"];
                            string Field = form["Field"] == null ? null : form["Field"];
                            string InputType = form["InputType"] == null ? null : form["InputType"];
                            string NarrationIt24 = form["Narration"] == null ? null : form["Narration"];

                            if (Form24QFileType != null)
                            {
                                var look = db.Lookup.Include(q => q.LookupValues).Where(q => q.Code == "608").Select(q => q.LookupValues).SingleOrDefault();
                                var val = look.Where(q => q.LookupVal == Form24QFileType).SingleOrDefault();
                                // var val = db.LookupValue.Where(q=>q.LookupVal==Form24QFileType).SingleOrDefault();
                                if (val != null)
                                {
                                    c.Form24QFileType = val;
                                }
                            }
                            if (DataType != null)
                            {
                                var val = db.LookupValue.Where(q => q.LookupVal == DataType).SingleOrDefault();
                                if (val != null)
                                {
                                    c.DataType = val;
                                }
                            }
                            if (SrNo != null)
                            {
                                c.SrNo = Convert.ToInt32(SrNo);
                            }
                            if (ExcelColNo != null)
                            {
                                c.ExcelColNo = ExcelColNo;
                            }
                            if (Field != null)
                            {
                                var rep = Field.Replace("#", " ");
                                c.Field = rep;
                            }
                            if (InputType != null)
                            {
                                c.InputType = InputType;
                            }
                            if (NarrationIt24 != null)
                            {
                                c.Narration = NarrationIt24;
                            }
                            if (Sizeit != null)
                            {
                                c.Size = Convert.ToInt32(Sizeit);
                            }
                            using (TransactionScope ts = new TransactionScope())
                            {
                                c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                ITForm24QFileFormatDefinition Itform = new ITForm24QFileFormatDefinition()
                                {
                                    SrNo = c.SrNo,
                                    Form24QFileType = c.Form24QFileType,
                                    DataType = c.DataType,
                                    ExcelColNo = c.ExcelColNo,
                                    Field = c.Field,
                                    InputType = c.InputType,
                                    Narration = c.Narration,
                                    Size = c.Size,
                                    DBTrack = c.DBTrack,
                                    DataType_Id = c.DataType.Id,
                                    Form24QFileType_Id = c.Form24QFileType.Id
                                };
                                try
                                {

                                    db.ITForm24QFileFormatDefinition.Add(Itform);
                                    //var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, c.DBTrack);
                                    db.SaveChanges();
                                    ts.Complete();
                                    return this.Json(new { success = false, data = rowno, responseText = "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
                                }

                                catch (DbUpdateConcurrencyException)
                                {
                                    return RedirectToAction("Create", new { concurrencyError = true, id = c.Id });
                                }
                                catch (DataException)
                                {

                                    return this.Json(new { success = false, data = rowno, responseText = "Unable to create. Try again, and if the problem persists contact your system administrator.." }, JsonRequestBehavior.AllowGet);
                                }
                            }

                        }
                    case "LOANADVREQUEST":

                        //string CloserDate = form["CloserDate"] == null ? null : form["CloserDate"];
                        string EnforcementDate = form["EnforcementDate"] == null ? null : form["EnforcementDate"];                      //01/paymonth
                        // string IsInstallment = form["IsInstallment"] == null ? null : form["IsInstallment"];                            //ok
                        string IsActive = form["IsActive"] == null ? null : form["IsActive"];
                        string EmpCodeE = form["EmpCode"] == null ? null : form["EmpCode"];                                              //ok
                        string LoanAccBranch = form["LoanAccBranch"] == null ? null : form["LoanAccBranch"];                            //ok
                        string LoanAccNo = form["LoanAccNo"] == null ? null : form["LoanAccNo"];                                        //ok
                        string LoanAdvanceHead = form["LoanAdvanceHead"] == null ? null : form["LoanAdvanceHead"];                      //ok
                        Double LoanAppliedAmount = form["LoanSanctionedAmount"] == null ? 0 : Convert.ToDouble(form["LoanSanctionedAmount"]);
                        Double LoanSanctionedAmount = form["LoanSanctionedAmount"] == null ? 0 : Convert.ToDouble(form["LoanSanctionedAmount"]);
                        string LoanSubAccNo = "";                       //form["LoanSubAccNo"] == null ? null : form["LoanSubAccNo"];
                        Double MonthlyInstallmentAmount = form["MonthlyInstallmentAmount"] == null ? 0 : Convert.ToDouble(form["MonthlyInstallmentAmount"]);
                        Double MonthlyInterest = form["MonthlyInterest"] == null ? 0 : Convert.ToDouble(form["MonthlyInterest"]);                       //ok
                        Double MonthlyPricipalAmount = form["MonthlyPricipalAmount"] == null ? 0 : Convert.ToDouble(form["MonthlyPricipalAmount"]);     //ok
                        // DateTime RequisitionDate = DateTime.Now;
                        // DateTime SanctionedDate = DateTime.Now;
                        //string Narration = form["Narration"] == null ? null : form["Narration"];
                        string SanctionedDate = form["SanctionedDate"] == null ? null : form["SanctionedDate"];                         //sys date
                        string RequisitionDate = form["RequisitionDate"] == null ? null : form["RequisitionDate"];                      //sys date
                        //string LoanAccBranch = form["LoanAccBranchlist"] == null ? null : form["LoanAccBranchlist"];

                        if (IsActive == "false")
                        {
                            using (TransactionScope ts = new TransactionScope())
                            {
                                using (DataBaseContext db = new DataBaseContext())
                                {
                                    var id = Convert.ToInt32(ids[0]);
                                    var OEmployeePayroll = db.EmployeePayroll
                                        .Include(e => e.Employee)
                                        .Include(e => e.LoanAdvRequest)
                                        .Include(e => e.LoanAdvRequest.Select(a => a.LoanAdvRepaymentT))
                                        .Include(e => e.LoanAdvRequest.Select(a => a.LoanAdvanceHead))
                                        .Include(e => e.LoanAdvRequest.Select(a => a.LoanAccBranch))
                                        .Include(e => e.LoanAdvRequest.Select(a => a.LoanAccBranch.LocationObj))
                                        .Where(e => e.Employee.Id == id).SingleOrDefault();

                                    var EmpSalDelT = db.SalaryT.Where(e => e.PayMonth == PaymonthforLoanadv && e.ReleaseDate == null && e.EmployeePayroll_Id == OEmployeePayroll.Id).AsNoTracking().AsParallel().SingleOrDefault();
                                    if (EmpSalDelT != null)
                                    {
                                        if (EmpSalDel == null || EmpSalDel == "")
                                        {
                                            EmpSalDel = OEmployeePayroll.Employee.EmpCode;
                                        }
                                        else
                                        {
                                            EmpSalDel = EmpSalDel + ", " + OEmployeePayroll.Employee.EmpCode;
                                        }
                                        SalaryGen.DeleteSalary(EmpSalDelT.Id, PaymonthforLoanadv);
                                        SalDel = true;
                                    }

                                    LoanAdvRequest LoanAdvReq = OEmployeePayroll.LoanAdvRequest.Where(q => q.LoanAccNo == LoanAccNo
                                          && q.LoanAdvanceHead.Code == LoanAdvanceHead
                                          && q.LoanAccBranch.LocationObj.LocCode == LoanAccBranch).SingleOrDefault();

                                    DateTime paymon = Convert.ToDateTime(PaymonthforLoanadv);
                                    List<LoanAdvRepaymentT> LoanAdvRepaymentTList = LoanAdvReq.LoanAdvRepaymentT.Where(q => q.InstallementDate >= paymon).ToList();
                                    //List<int>Loanrepids=LoanAdvRepaymentTList.Select(q=>q.Id).ToList();
                                    List<LoanAdvRepaymentT> LoanAdvRepaymentTListRemain = LoanAdvReq.LoanAdvRepaymentT.Where(q => !LoanAdvRepaymentTList.Contains(q)).ToList();
                                    if (LoanAdvRepaymentTList != null && LoanAdvRepaymentTList.Count() > 0)
                                    {
                                        db.LoanAdvRepaymentT.RemoveRange(LoanAdvRepaymentTList);
                                        db.SaveChanges();
                                    }

                                    LoanAdvReq.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                    LoanAdvReq.IsActive = false;
                                    LoanAdvReq.LoanAdvRepaymentT = LoanAdvRepaymentTListRemain;
                                    //db save
                                    db.LoanAdvRequest.Attach(LoanAdvReq);
                                    db.Entry(LoanAdvReq).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    db.Entry(LoanAdvReq).State = System.Data.Entity.EntityState.Detached;
                                    ts.Complete();
                                    if (SalDel == true)
                                    {
                                        return Json(new { data = rowno, success = true, responseText = "Salary deleted for employee " + EmpSalDel + ". And Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
                                    }
                                    else
                                    {
                                        return Json(new { data = rowno, success = true, responseText = "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
                                    }



                                    //LoanAdvRequest LoanAdvRequest = new LoanAdvRequest()
                                    //{
                                    //    CloserDate = LoanAdvReq.CloserDate,
                                    //    EnforcementDate = LoanAdvReq.EnforcementDate,
                                    //    IsActive = false,
                                    //    IsInstallment = LoanAdvReq.IsInstallment,
                                    //    LoanAccBranch = LoanAdvReq.LoanAccBranch,
                                    //    LoanAccNo = LoanAdvReq.LoanAccNo,
                                    //    LoanAdvanceHead = LoanAdvReq.LoanAdvanceHead,
                                    //    LoanAppliedAmount = LoanAdvReq.LoanAppliedAmount,
                                    //    LoanSanctionedAmount = LoanAdvReq.LoanSanctionedAmount,
                                    //    LoanSubAccNo = LoanAdvReq.LoanSubAccNo,
                                    //    MonthlyInstallmentAmount = LoanAdvReq.MonthlyInstallmentAmount,
                                    //    MonthlyInterest = LoanAdvReq.MonthlyInterest,
                                    //    MonthlyPricipalAmount = LoanAdvReq.MonthlyPricipalAmount,
                                    //    Narration = LoanAdvReq.Narration,
                                    //    RequisitionDate = LoanAdvReq.RequisitionDate,
                                    //    SanctionedDate = LoanAdvReq.SanctionedDate,
                                    //    TotalInstall = LoanAdvReq.TotalInstall,
                                    //    DBTrack = LoanAdvReq.DBTrack
                                    //};
                                }
                            }
                        }
                        else
                        {
                            LoanAdvRequest LoanAdvReq = new LoanAdvRequest();

                            double TotalInstallment = LoanSanctionedAmount / MonthlyInstallmentAmount;

                            using (DataBaseContext db = new DataBaseContext())
                            {
                                if (LoanAdvanceHead != null)
                                {
                                    var code = LoanAdvanceHead;
                                    var val = db.LoanAdvanceHead.Where(e => e.Code == code).SingleOrDefault();
                                    if (val == null)
                                    {
                                        return Json(new { success = false, data = rowno, responseText = "Loan Head Not Found" }, JsonRequestBehavior.AllowGet);
                                    }
                                    LoanAdvReq.LoanAdvanceHead = val;
                                }
                                else
                                {
                                    return Json(new { success = false, data = rowno, responseText = "Please select LoanAdvanceHead." }, JsonRequestBehavior.AllowGet);
                                }
                                if (LoanAccBranch != null)
                                {
                                    var code = LoanAccBranch;
                                    var val = db.Location.Include(e => e.LocationObj).Where(e => e.LocationObj.LocCode == code).SingleOrDefault();
                                    if (val == null)
                                    {
                                        return Json(new { success = false, data = rowno, responseText = "LoanAccBranch NOt Found" }, JsonRequestBehavior.AllowGet);
                                    }
                                    LoanAdvReq.LoanAccBranch = val;
                                }

                                var id = Convert.ToInt32(ids[0]);
                                var OEmployeePayroll = db.EmployeePayroll
                                    .Include(e => e.Employee)
                                    .Include(e => e.LoanAdvRequest)
                                    .Include(e => e.LoanAdvRequest.Select(a => a.LoanAdvRepaymentT))
                                    .Include(e => e.LoanAdvRequest.Select(a => a.LoanAdvanceHead))
                                    .Include(e => e.LoanAdvRequest.Select(a => a.LoanAccBranch))
                                    .Include(e => e.LoanAdvRequest.Select(a => a.LoanAccBranch.LocationObj))
                                    .Where(e => e.Employee.Id == id).SingleOrDefault();

                                var EmpSalDelT = db.SalaryT.Where(e => e.PayMonth == PaymonthforLoanadv && e.ReleaseDate == null && e.EmployeePayroll_Id == OEmployeePayroll.Id).AsNoTracking().AsParallel().SingleOrDefault();
                                if (EmpSalDelT != null)
                                {
                                    if (EmpSalDel == null || EmpSalDel == "")
                                    {
                                        EmpSalDel = OEmployeePayroll.Employee.EmpCode;
                                    }
                                    else
                                    {
                                        EmpSalDel = EmpSalDel + ", " + OEmployeePayroll.Employee.EmpCode;
                                    }
                                    SalaryGen.DeleteSalary(EmpSalDelT.Id, PaymonthforLoanadv);
                                    SalDel = true;
                                }



                                if (EnforcementDate != null)
                                {
                                    var val = Convert.ToDateTime(EnforcementDate);
                                    LoanAdvReq.EnforcementDate = val;
                                }
                                if (SanctionedDate != null)
                                {
                                    var val = Convert.ToDateTime(SanctionedDate);
                                    LoanAdvReq.SanctionedDate = val;
                                }
                                if (RequisitionDate != null)
                                {
                                    var val = Convert.ToDateTime(RequisitionDate);
                                    LoanAdvReq.RequisitionDate = val;
                                }


                                if (LoanAccNo != null)
                                {
                                    var val = Convert.ToString(LoanAccNo);
                                    LoanAdvReq.LoanAccNo = val;
                                }
                                LoanAdvReq.IsActive = true;
                                LoanAdvReq.LoanAppliedAmount = LoanAppliedAmount;
                                LoanAdvReq.LoanSanctionedAmount = LoanSanctionedAmount;
                                LoanAdvReq.LoanSubAccNo = LoanSubAccNo;
                                LoanAdvReq.MonthlyInstallmentAmount = MonthlyInstallmentAmount;
                                LoanAdvReq.MonthlyInterest = MonthlyInterest;
                                LoanAdvReq.MonthlyPricipalAmount = MonthlyPricipalAmount;
                                LoanAdvReq.Narration = "Via Excel Upload";
                                //LoanAdvReq.RequisitionDate = RequisitionDate;
                                // LoanAdvReq.SanctionedDate = RequisitionDate;
                                LoanAdvReq.TotalInstall = Convert.ToInt32(TotalInstallment);



                                //Already Exits Check
                                List<LoanAdvRequest> oLoanChk = new List<LoanAdvRequest>();
                                oLoanChk = db.EmployeePayroll.Where(e => e.Employee.Id == id)
                                    .SelectMany(e => e.LoanAdvRequest.Where(a =>
                                        // a.RequisitionDate == LoanAdvReq.RequisitionDate &&
                                        a.IsActive == true &&
                                        a.LoanAccNo == LoanAdvReq.LoanAccNo && a.LoanAccBranch.LocationObj.LocCode == LoanAdvReq.LoanAccBranch.LocationObj.LocCode &&
                                        a.LoanAdvanceHead.Id == LoanAdvReq.LoanAdvanceHead.Id)).ToList();
                                if (oLoanChk.Count > 0)
                                {
                                    return Json(new { success = false, data = rowno, responseText = "Loan Already Exits..!" }, JsonRequestBehavior.AllowGet);

                                }
                                using (TransactionScope ts = new TransactionScope())
                                {

                                    LoanAdvReq.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                                    LoanAdvRequest LoanAdvRequest = new LoanAdvRequest()
                                    {
                                        CloserDate = LoanAdvReq.CloserDate,
                                        EnforcementDate = LoanAdvReq.EnforcementDate,
                                        IsActive = LoanAdvReq.IsActive,
                                        IsInstallment = LoanAdvReq.IsInstallment,
                                        LoanAccBranch = LoanAdvReq.LoanAccBranch,
                                        LoanAccNo = LoanAdvReq.LoanAccNo,
                                        LoanAdvanceHead = LoanAdvReq.LoanAdvanceHead,
                                        LoanAppliedAmount = LoanAdvReq.LoanAppliedAmount,
                                        LoanSanctionedAmount = LoanAdvReq.LoanSanctionedAmount,
                                        LoanSubAccNo = LoanAdvReq.LoanSubAccNo,
                                        MonthlyInstallmentAmount = LoanAdvReq.MonthlyInstallmentAmount,
                                        MonthlyInterest = LoanAdvReq.MonthlyInterest,
                                        MonthlyPricipalAmount = LoanAdvReq.MonthlyPricipalAmount,
                                        Narration = LoanAdvReq.Narration,
                                        RequisitionDate = LoanAdvReq.RequisitionDate,
                                        SanctionedDate = LoanAdvReq.SanctionedDate,
                                        TotalInstall = LoanAdvReq.TotalInstall,
                                        DBTrack = LoanAdvReq.DBTrack
                                    };
                                    errorList = ValidateObj(LoanAdvRequest);
                                    if (errorList.Count > 0)
                                    {
                                        return Json(new { success = false, data = rowno, responseText = string.Join(",", errorList) }, JsonRequestBehavior.AllowGet);
                                    }
                                    try
                                    {
                                        List<LoanAdvRepaymentT> LoanAdvRepaymentTDet = new List<LoanAdvRepaymentT>();

                                        if (LoanAdvRequest.EnforcementDate != null && LoanAdvReq.TotalInstall != 0)
                                        {
                                            double AmoutPaidTillNow = 0;
                                            for (int i = 0; i <= LoanAdvReq.TotalInstall - 1; i++)
                                            {
                                                if (i == LoanAdvReq.TotalInstall - 1 && TotalInstallment > LoanAdvReq.TotalInstall - 1)
                                                {
                                                    LoanAdvReq.MonthlyInstallmentAmount = LoanAdvRequest.LoanSanctionedAmount - AmoutPaidTillNow;
                                                }
                                                // double TotalLoanPaid = 0;

                                                string Month = LoanAdvRequest.EnforcementDate.Value.AddMonths(i).Month.ToString().Length == 1 ? "0" + LoanAdvRequest.EnforcementDate.Value.AddMonths(i).Month.ToString() : LoanAdvRequest.EnforcementDate.Value.AddMonths(i).Month.ToString();
                                                string PayMonth = Month + "/" + LoanAdvRequest.EnforcementDate.Value.AddMonths(i).Year;

                                                AmoutPaidTillNow = AmoutPaidTillNow + LoanAdvRequest.MonthlyInstallmentAmount;

                                                LoanAdvRepaymentT LoanAdvRepayT = new LoanAdvRepaymentT()
                                                {
                                                    InstallementDate = LoanAdvRequest.EnforcementDate.Value.AddMonths(i),
                                                    InstallmentAmount = LoanAdvReq.MonthlyInstallmentAmount,
                                                    InstallmentCount = i + 1,
                                                    InstallmentPaid = LoanAdvReq.MonthlyPricipalAmount + LoanAdvReq.MonthlyInterest,
                                                    PayMonth = PayMonth,
                                                    TotalLoanBalance = LoanAdvRequest.LoanSanctionedAmount - AmoutPaidTillNow,
                                                    TotalLoanPaid = AmoutPaidTillNow,
                                                    MonthlyInterest = LoanAdvReq.MonthlyInterest,
                                                    MonthlyPricipalAmount = LoanAdvReq.MonthlyPricipalAmount,
                                                    DBTrack = LoanAdvReq.DBTrack
                                                };
                                                errorList = ValidateObj(LoanAdvRepayT);
                                                if (errorList.Count > 0)
                                                {
                                                    return Json(new { success = false, data = rowno, responseText = string.Join(",", errorList) }, JsonRequestBehavior.AllowGet);

                                                }
                                                LoanAdvRepaymentTDet.Add(LoanAdvRepayT);
                                            }

                                        }
                                        LoanAdvRequest.LoanAdvRepaymentT = LoanAdvRepaymentTDet;
                                        db.LoanAdvRequest.Add(LoanAdvRequest);
                                        db.SaveChanges();
                                        var OEmployeePayroll1 = db.EmployeePayroll.Include(e => e.Employee).Where(e => e.Employee.Id == id).SingleOrDefault();

                                        List<LoanAdvRequest> LoanAdvReqList = new List<LoanAdvRequest>();
                                        LoanAdvReqList.Add(LoanAdvRequest);
                                        LoanAdvReqList.AddRange(OEmployeePayroll1.LoanAdvRequest);
                                        OEmployeePayroll1.LoanAdvRequest = LoanAdvReqList;
                                        db.EmployeePayroll.Attach(OEmployeePayroll1);
                                        db.Entry(OEmployeePayroll1).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                        db.Entry(OEmployeePayroll1).State = System.Data.Entity.EntityState.Detached;

                                        ts.Complete();
                                        if (SalDel == true)
                                        {
                                            return Json(new { data = rowno, success = true, responseText = "Salary deleted for employee " + EmpSalDel + ". And Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
                                        }
                                        else
                                        {
                                            return Json(new { data = rowno, success = true, responseText = "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
                                        }

                                    }
                                    catch (DbUpdateConcurrencyException)
                                    {
                                        return RedirectToAction("Create", new { concurrencyError = true, id = LoanAdvReq.Id });
                                    }
                                    catch (DataException e)
                                    {
                                        return this.Json(new { success = false, data = rowno, responseText = e.InnerException.Message.ToString() }, JsonRequestBehavior.AllowGet);
                                    }
                                }
                            }
                        }
                    case "ITAXTRANST":
                        string Msg = "";
                        using (DataBaseContext db = new DataBaseContext())
                        {
                            ITaxTransT ITX = new ITaxTransT();
                            string PayMonth = form["PayMonth"] == null ? null : form["PayMonth"];
                            string TaxPaid = form["TaxPaid"] == null ? null : form["TaxPaid"];

                            Employee OEmployee = null;
                            EmployeePayroll OEmployeePayroll = null;
                            string EmpCode = null;
                            string EmpRel = null;

                            if (ids != null)
                            {
                                foreach (var i in ids)
                                {
                                    OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct)
                                               .Where(r => r.Id == i).SingleOrDefault();

                                    OEmployeePayroll = db.EmployeePayroll.Where(e => e.Employee.Id == i).SingleOrDefault();

                                    var OEmpSalT = db.EmployeePayroll.Where(e => e.Id == OEmployeePayroll.Id).Include(e => e.ITaxTransT).SingleOrDefault();
                                    var EmpSalT = OEmpSalT.ITaxTransT != null ? OEmpSalT.ITaxTransT.Where(e => e.PayMonth == PayMonth) : null;
                                    if (EmpSalT != null && EmpSalT.Count() > 0)
                                    {
                                        if (EmpCode == null || EmpCode == "")
                                        {
                                            EmpCode = OEmployee.EmpCode;
                                        }
                                        else
                                        {
                                            EmpCode = EmpCode + ", " + OEmployee.EmpCode;
                                        }
                                    }

                                    var OEmpSalRelT = db.EmployeePayroll.Where(e => e.Id == OEmployeePayroll.Id).Include(e => e.ITaxTransT).SingleOrDefault();
                                    var EmpSalRelT = OEmpSalRelT.SalaryT != null ? OEmpSalRelT.SalaryT.Where(e => e.PayMonth == PayMonth && e.ReleaseDate != null) : null;

                                    if (EmpSalRelT != null && EmpSalRelT.Count() > 0)
                                    {
                                        if (EmpRel == null || EmpRel == "")
                                        {
                                            EmpRel = OEmployee.EmpCode;
                                        }
                                        else
                                        {
                                            EmpRel = EmpRel + ", " + OEmployee.EmpCode;
                                        }
                                    }
                                }
                            }
                            if (EmpCode != null)
                            {
                                Msg = " ITaxTrans already exists for employee " + EmpCode + ". ";
                                return this.Json(new { success = false, data = rowno, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                            if (EmpRel != null)
                            {
                                Msg = " Salary released for employee " + EmpRel + ". You can't change ITaxTrans now. ";
                                return this.Json(new { success = false, data = rowno, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                            if (PayMonth != null && PayMonth != "")
                            {
                                ITX.PayMonth = PayMonth;
                                int mon = int.Parse(PayMonth.Split('/')[0].StartsWith("0") == true ? PayMonth.Split('/')[0].Remove(0, 1) : PayMonth.Split('/')[0]);
                                int DaysInMonth = System.DateTime.DaysInMonth(int.Parse(PayMonth.Split('/')[1]), mon);

                            }

                            //if (PayableDays != null && PayableDays != "")
                            //{
                            //    var Payable_days = int.Parse(PayableDays);
                            //}
                            if (TaxPaid != null)
                            {
                                ITX.TaxPaid = Convert.ToDouble(TaxPaid);
                            }

                            ITX.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            ITaxTransT ObjITX = new ITaxTransT();
                            {
                                ObjITX.PayMonth = ITX.PayMonth;
                                ObjITX.TaxPaid = ITX.TaxPaid;
                                ObjITX.DBTrack = ITX.DBTrack;
                                ObjITX.Mode = "MAN";
                            }
                            if (ids != null)
                            {
                                foreach (var i in ids)
                                {
                                    OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct)
                                                .Where(r => r.Id == i).SingleOrDefault();

                                    OEmployeePayroll
                                    = db.EmployeePayroll
                                  .Where(e => e.Employee.Id == i).SingleOrDefault();

                                    ObjITX.GeoStruct = db.GeoStruct.Find(OEmployee.GeoStruct.Id);

                                    ObjITX.FuncStruct = db.FuncStruct.Find(OEmployee.FuncStruct.Id);


                                    ObjITX.PayStruct = db.PayStruct.Find(OEmployee.PayStruct.Id == null ? 0 : OEmployee.PayStruct.Id);


                                    using (TransactionScope ts = new TransactionScope())
                                    {
                                        db.ITaxTransT.Add(ObjITX);
                                        db.SaveChanges();
                                        List<ITaxTransT> OITAX = new List<ITaxTransT>();
                                        OITAX.Add(db.ITaxTransT.Find(ObjITX.Id));

                                        if (OEmployeePayroll == null)
                                        {
                                            EmployeePayroll OTEP = new EmployeePayroll()
                                            {
                                                Employee = db.Employee.Find(OEmployee.Id),
                                                ITaxTransT = OITAX,
                                                DBTrack = ITX.DBTrack
                                            };
                                            db.EmployeePayroll.Add(OTEP);
                                            db.SaveChanges();
                                        }
                                        else
                                        {
                                            var aa = db.EmployeePayroll.Find(OEmployeePayroll.Id);
                                            OITAX.AddRange(aa.ITaxTransT);
                                            aa.ITaxTransT = OITAX;
                                            //OEmployeePayroll.DBTrack = dbt;

                                            db.EmployeePayroll.Attach(aa);
                                            db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();
                                            db.Entry(aa).State = System.Data.Entity.EntityState.Detached;
                                        }
                                        ts.Complete();
                                    }
                                }
                                if (SalDel == true)
                                {
                                    return Json(new { data = rowno, success = true, responseText = "Salary deleted for employee " + EmpRel + ". And Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
                                }
                                else
                                {
                                    return Json(new { data = rowno, success = true, responseText = "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            Msg = "Unable to create...";
                            return this.Json(new { success = false, data = rowno, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    case "LOOKUP":
                        Lookup oLookup = new Lookup();
                        List<LookupValue> lookupval = new List<LookupValue>();
                        string Values1 = form["LookupValue"];
                        string Values2 = form["LookupValData"];
                        string Code = form["Code"] != null ? form["Code"] : null;
                        string Name = form["Name"] != null ? form["Name"] : null;

                        string Values = Values1.Replace("#", " ").Trim();
                        string ValuesData = Values2.Replace("#", " ").Trim();
                        using (DataBaseContext db = new DataBaseContext())
                        {
                            using (TransactionScope ts = new TransactionScope())
                            {


                                if (Values != null)
                                {
                                    var lookup = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == Code).SingleOrDefault();
                                    if (lookup != null && lookup.LookupValues.Any(e => e.LookupVal == Values))
                                    {
                                        var LookupVal = lookup.LookupValues.Where(e => e.LookupVal == Values).SingleOrDefault();
                                        lookupval.Add(LookupVal);
                                    }
                                    else
                                    {
                                        LookupValue oLookupValue = new LookupValue
                                        {
                                            DeleteValue = false,
                                            DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false },
                                            IsActive = true,
                                            LookupVal = Values,
                                            LookupValData = ValuesData != null ? ValuesData : "",
                                        };
                                        db.LookupValue.Add(oLookupValue);
                                        db.SaveChanges();
                                        lookupval.Add(oLookupValue);
                                    }
                                    oLookup.LookupValues = lookupval;
                                }
                                else
                                {
                                    oLookup.LookupValues = null;
                                }




                                if (Code != null)
                                {
                                    oLookup.Code = form["Code"];
                                }
                                if (Name != null)
                                {
                                    oLookup.Name = form["Name"];
                                }
                                if (ModelState.IsValid)
                                {

                                    if (db.Lookup.Any(o => o.Code == oLookup.Code))
                                    {
                                        //update
                                        var Lookup = db.Lookup.Where(e => e.Code == oLookup.Code).SingleOrDefault();
                                        if (Lookup.LookupValues.Count == 0)
                                        {

                                            Lookup.LookupValues = oLookup.LookupValues;
                                        }
                                        else
                                        {

                                            Lookup.LookupValues.Add(oLookup.LookupValues.FirstOrDefault());

                                        }

                                        db.Entry(Lookup).State = System.Data.Entity.EntityState.Modified;

                                    }
                                    else
                                    {
                                        oLookup.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                        Lookup lookup = new Lookup()
                                        {
                                            Code = oLookup.Code == null ? "" : oLookup.Code.Trim(),
                                            Name = oLookup.Name == null ? "" : oLookup.Name.Replace("#", " ").Trim(),
                                            LookupValues = oLookup.LookupValues,
                                            DBTrack = oLookup.DBTrack
                                        };
                                        db.Lookup.Add(lookup);
                                    }
                                    db.SaveChanges();
                                    ts.Complete();
                                    return this.Json(new { success = true, data = rowno, responseText = "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);

                                }
                                else
                                {
                                    StringBuilder sb = new StringBuilder("");
                                    foreach (ModelState modelState in ModelState.Values)
                                    {
                                        foreach (ModelError error in modelState.Errors)
                                        {
                                            sb.Append(error.ErrorMessage);
                                            sb.Append("." + "\n");
                                        }
                                    }
                                    var errorMsg = sb.ToString();
                                    return this.Json(new { success = false, data = rowno, responseText = errorMsg }, JsonRequestBehavior.AllowGet);
                                }
                            }
                        }
                    case "LOANADVREPAYMENTT":
                        LoanAdvRepaymentT oLoanAdvRepaymentT = new LoanAdvRepaymentT();

                        string LoanAdvHead = form["LoanAdvanceHead"] != null ? form["LoanAdvanceHead"].Trim() : null;
                        string LoanPayMonth = form["PayMonth"] != null ? form["PayMonth"].Trim() : null;
                        string LMonthlyInterest = form["MonthlyInterest"] != null ? form["MonthlyInterest"].Trim() : null;
                        string LMonthlyPricipalAmount = form["MonthlyPricipalAmount"] != null ? form["MonthlyPricipalAmount"].Trim() : null;
                        string LInstallmentPaid = form["InstallmentPaid"] != null ? form["InstallmentPaid"].Trim() : null;
                        string LLoanAccNo = form["LoanAccNo"] != null ? form["LoanAccNo"].Trim() : null;
                        string LLoanAccBranch = form["LoanAccBranch"] != null ? form["LoanAccBranch"].Trim() : null;
                        //string EmpSalDelL = null;
                        //bool SalDelL = false;

                        if (LoanAdvHead == null || LoanPayMonth == null || LMonthlyInterest == null || LMonthlyPricipalAmount == null || LInstallmentPaid == null)
                        {
                            return Json(new { data = rowno, success = false, responseText = "Column Shouldn't be null..!" }, JsonRequestBehavior.AllowGet);
                        }
                        using (DataBaseContext db = new DataBaseContext())
                        {
                            using (TransactionScope ts = new TransactionScope())
                            {
                                var LoanHeadId = db.LoanAdvanceHead.Where(a => a.Code == LoanAdvHead).FirstOrDefault();
                                var id = ids[0];
                                //var LoanReq = LoanHeadId != null ? db.EmployeePayroll
                                //    .Include(a => a.LoanAdvRequest)
                                //    .Include(a => a.LoanAdvRequest.Select(e => e.LoanAdvanceHead))
                                //    .Include(a => a.LoanAdvRequest.Select(e => e.LoanAdvRepaymentT))
                                //    .Where(a => a.Employee.Id == id &&
                                //                a.LoanAdvRequest.Any(e =>
                                //                    e.LoanAccBranch.LocationObj.LocCode == LLoanAccBranch &&
                                //                    e.LoanAccNo == LLoanAccNo.Trim() &&
                                //                    e.IsActive == true &&
                                //                    e.LoanAdvanceHead.Id == LoanHeadId.Id
                                //                )).SingleOrDefault() : null;
                                var OEmployeePayroll = db.EmployeePayroll
                                       .Include(e => e.Employee)
                                       .Include(e => e.LoanAdvRequest)
                                       .Include(e => e.LoanAdvRequest.Select(a => a.LoanAdvRepaymentT))
                                       .Include(e => e.LoanAdvRequest.Select(a => a.LoanAdvanceHead))
                                       .Include(e => e.LoanAdvRequest.Select(a => a.LoanAccBranch))
                                       .Include(e => e.LoanAdvRequest.Select(a => a.LoanAccBranch.LocationObj))
                                       .Where(e => e.Employee.Id == id).SingleOrDefault();

                                var EmpSalDelT = db.SalaryT.Where(e => e.PayMonth == LoanPayMonth && e.ReleaseDate == null && e.EmployeePayroll_Id == OEmployeePayroll.Id).AsNoTracking().AsParallel().SingleOrDefault();
                                if (EmpSalDelT != null)
                                {
                                    if (EmpSalDel == null || EmpSalDel == "")
                                    {
                                        EmpSalDel = OEmployeePayroll.Employee.EmpCode;
                                    }
                                    else
                                    {
                                        EmpSalDel = EmpSalDel + ", " + OEmployeePayroll.Employee.EmpCode;
                                    }
                                    SalaryGen.DeleteSalary(EmpSalDelT.Id, LoanPayMonth);
                                    SalDel = true;
                                }

                                using (DataBaseContext db1 = new DataBaseContext())
                                {
                                    var OEmployeePayrollnew = db1.EmployeePayroll
                                           .Include(e => e.Employee)
                                           .Include(e => e.LoanAdvRequest)
                                           .Include(e => e.LoanAdvRequest.Select(a => a.LoanAdvRepaymentT))
                                           .Include(e => e.LoanAdvRequest.Select(a => a.LoanAdvanceHead))
                                           .Include(e => e.LoanAdvRequest.Select(a => a.LoanAccBranch))
                                           .Include(e => e.LoanAdvRequest.Select(a => a.LoanAccBranch.LocationObj))
                                           .Where(e => e.Employee.Id == id).SingleOrDefault();

                                    var LoanReq = OEmployeePayrollnew.LoanAdvRequest.Where(q => q.LoanAccNo == LLoanAccNo.Trim()
                                            && q.LoanAdvanceHead.Id == LoanHeadId.Id && q.IsActive == true
                                            && q.LoanAccBranch.LocationObj.LocCode == LLoanAccBranch).SingleOrDefault();
                                    if (LoanReq == null)
                                    {
                                        return Json(new { data = rowno, success = false, responseCode = 1, responseText = "Loan Request Not Found..!" }, JsonRequestBehavior.AllowGet);
                                    }
                                    //var LoanRepayment = LoanReq != null ?
                                    //    LoanReq.LoanAdvRequest.Where(a => a.LoanAdvRepaymentT.Any(q => q.PayMonth == LoanPayMonth)).SingleOrDefault() : null;
                                    var LoanRepayment = LoanReq != null ?
                                        LoanReq.LoanAdvRepaymentT.Where(a => a.PayMonth == LoanPayMonth).SingleOrDefault() : null;

                                    if (LoanRepayment != null)
                                    {
                                        // var LoanAdvRepaymentT_temp = LoanRepayment.LoanAdvRepaymentT.Where(a => a.PayMonth == LoanPayMonth).FirstOrDefault();
                                        var LoanAdvRepaymentT_temp = LoanRepayment;

                                        LoanAdvRepaymentT_temp.PayMonth = LoanPayMonth;
                                        LoanAdvRepaymentT_temp.MonthlyInterest = Convert.ToDouble(LMonthlyInterest);
                                        LoanAdvRepaymentT_temp.MonthlyPricipalAmount = Convert.ToDouble(LMonthlyPricipalAmount);
                                        // LoanAdvRepaymentT_temp.InstallmentPaid = Convert.ToDouble(LInstallmentPaid); Installmentamt considered in salary gen
                                        LoanAdvRepaymentT_temp.InstallmentAmount = Convert.ToDouble(LInstallmentPaid);
                                        db1.LoanAdvRepaymentT.Attach(LoanAdvRepaymentT_temp);
                                        db1.Entry(LoanAdvRepaymentT_temp).State = System.Data.Entity.EntityState.Modified;
                                        db1.SaveChanges();
                                        ts.Complete();
                                        if (SalDel == true)
                                        {
                                            return Json(new { data = rowno, success = true, responseText = "Salary deleted for employee " + EmpSalDel + ". And Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
                                        }
                                        else
                                        {
                                            return Json(new { data = rowno, success = true, responseText = "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
                                        }

                                    }
                                    else
                                    {
                                        //   var repay = LoanReq.LoanAdvRequest.SingleOrDefault().LoanAdvRepaymentT;
                                        var repay = LoanReq.LoanAdvRepaymentT;
                                        oLoanAdvRepaymentT.PayMonth = LoanPayMonth;
                                        oLoanAdvRepaymentT.MonthlyInterest = Convert.ToDouble(LMonthlyInterest);
                                        oLoanAdvRepaymentT.MonthlyPricipalAmount = Convert.ToDouble(LMonthlyPricipalAmount);
                                        //oLoanAdvRepaymentT.InstallmentPaid = Convert.ToDouble(LInstallmentPaid);
                                        oLoanAdvRepaymentT.InstallmentAmount = Convert.ToDouble(LInstallmentPaid);
                                        oLoanAdvRepaymentT.InstallementDate = Convert.ToDateTime("01/" + LoanPayMonth);
                                        oLoanAdvRepaymentT.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                        db1.LoanAdvRepaymentT.Add(oLoanAdvRepaymentT);
                                        db1.SaveChanges();
                                        repay.Add(oLoanAdvRepaymentT);
                                        db1.Entry(LoanReq).State = System.Data.Entity.EntityState.Modified;
                                        db1.SaveChanges();
                                        ts.Complete();
                                        if (SalDel == true)
                                        {
                                            return Json(new { data = rowno, success = true, responseText = "Salary deleted for employee " + EmpSalDel + ". And New Repayment Added In Existing Loan Requisition ..!" }, JsonRequestBehavior.AllowGet);
                                        }
                                        else
                                        {
                                            return Json(new { data = rowno, success = true, responseText = "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
                                        }
                                    }
                                }
                            }
                        }
                    case "INSURANCEDETAILST":
                        string PolicyNo = form["PolicyNo"] == "0" ? "" : form["PolicyNo"];
                        String Frequency = form["Frequency"] != null ? form["Frequency"] : null;
                        String FromDate = form["FromDate"] != null ? form["FromDate"] : null;
                        String ToDate = form["ToDate"] != null ? form["ToDate"] : null;
                        String OperationStatus = form["OperationStatus"] != null ? form["OperationStatus"] : null;
                        String Premium = form["Premium"] != null ? form["Premium"] : null;
                        String SumAssured = form["SumAssured"] != null ? form["SumAssured"] : null;
                        String InsuranceProduct = form["InsuranceProduct"] != null ? form["InsuranceProduct"] : null;
                        InsuranceDetailsT ID = new InsuranceDetailsT();
                        using (DataBaseContext db = new DataBaseContext())
                        {
                            using (TransactionScope ts = new TransactionScope())
                            {
                                if (InsuranceProduct != null && InsuranceProduct != "")
                                {
                                    var Val = db.InsuranceProduct.Find(int.Parse(InsuranceProduct));
                                    ID.InsuranceProduct = Val;
                                }
                                if (OperationStatus != null && OperationStatus != "")
                                {
                                    var lookupdata = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "488").SingleOrDefault();
                                    var Val = lookupdata.LookupValues.Where(e => e.LookupVal.ToString().ToUpper() == OperationStatus.ToUpper()).SingleOrDefault();
                                    ID.OperationStatus = Val;
                                }
                                if (Frequency != null && Frequency != "")
                                {
                                    var lookupdata = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "421").SingleOrDefault();
                                    var Val = lookupdata.LookupValues.Where(e => e.LookupVal.ToString().ToUpper() == Frequency.ToUpper()).SingleOrDefault();
                                    ID.Frequency = Val;
                                }

                                ID.FromDate = Convert.ToDateTime(FromDate);
                                ID.ToDate = Convert.ToDateTime(ToDate);
                                ID.PolicyNo = PolicyNo;
                                ID.Premium = Convert.ToDouble(Premium);
                                ID.SumAssured = Convert.ToDouble(SumAssured);

                                ID.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                                Employee OEmployee = null;
                                EmployeePayroll OEmployeePayroll = null;

                                InsuranceDetailsT ObjID = new InsuranceDetailsT();
                                {
                                    ObjID.Frequency = ID.Frequency;
                                    ObjID.FromDate = ID.FromDate;
                                    ObjID.InsuranceProduct = ID.InsuranceProduct;
                                    ObjID.ToDate = ID.ToDate;
                                    ObjID.PolicyNo = ID.PolicyNo;
                                    ObjID.Premium = ID.Premium;
                                    ObjID.SumAssured = ID.SumAssured;
                                    ObjID.OperationStatus = ID.OperationStatus;
                                    ObjID.DBTrack = ID.DBTrack;

                                }
                                var i = ids[0];
                                OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct)
                                           .Where(r => r.Id == i).SingleOrDefault();

                                OEmployeePayroll
                                = db.EmployeePayroll
                              .Where(e => e.Employee.Id == i).SingleOrDefault();

                                //try
                                //{
                                db.InsuranceDetailsT.Add(ObjID);
                                db.SaveChanges();
                                List<InsuranceDetailsT> OFAT = new List<InsuranceDetailsT>();
                                OFAT.Add(db.InsuranceDetailsT.Find(ObjID.Id));

                                if (OEmployeePayroll == null)
                                {
                                    EmployeePayroll OTEP = new EmployeePayroll()
                                    {
                                        Employee = db.Employee.Find(OEmployee.Id),
                                        InsuranceDetailsT = OFAT,
                                        DBTrack = ID.DBTrack

                                    };


                                    db.EmployeePayroll.Add(OTEP);
                                    db.SaveChanges();
                                }
                                else
                                {
                                    var aa = db.EmployeePayroll.Find(OEmployeePayroll.Id);
                                    aa.InsuranceDetailsT = OFAT;
                                    //OEmployeePayroll.DBTrack = dbt;
                                    db.EmployeePayroll.Attach(aa);
                                    db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    db.Entry(aa).State = System.Data.Entity.EntityState.Detached;

                                }
                                ts.Complete();
                                //return Json(new Object[] { "", "", "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
                                List<string> Msgs = new List<string>();
                                return Json(new { data = rowno, success = true, responseText = "Data Saved Successfully..!" }, JsonRequestBehavior.AllowGet);

                                //}
                            }
                        }


                    case "PERKTRANST":
                        using (DataBaseContext db = new DataBaseContext())
                        {
                            PerkTransT perk = new PerkTransT();
                            string SalaryHeadlist = form["SalaryHead"] == "0" ? "" : form["SalaryHead"];
                            String PayMonth = form["PayMonth"] != null ? form["PayMonth"] : null;
                            String ActualAmount = form["ActualAmount"] != null ? form["ActualAmount"] : null;
                            String ProjectedAmount = form["ProjectedAmount"] != null ? form["ProjectedAmount"] : null;

                            if (SalaryHeadlist != null)
                            {
                                SalaryHead Incrid = db.SalaryHead.Where(e => e.Code.ToUpper() == SalaryHeadlist.ToUpper()).SingleOrDefault();
                                perk.SalaryHead = Incrid;
                            }
                            if (ProjectedAmount != null)
                            {
                                perk.ProjectedAmount = Convert.ToDouble(ProjectedAmount);
                            }
                            if (ActualAmount != null)
                            {
                                perk.ActualAmount = Convert.ToDouble(ActualAmount);
                            }
                            if (PayMonth != null)
                            {
                                perk.PayMonth = PayMonth;
                            }
                            Calendar FinancialYear = db.Calendar.Include(a => a.Name).Where(a => a.Name.LookupVal.ToUpper() == "FINANCIALYEAR" && a.Default == true).SingleOrDefault();
                            if (!string.IsNullOrEmpty(sEmpCode))
                            {
                                perk.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                PerkTransT ObjOEDT = new PerkTransT();

                                ObjOEDT.SalaryHead = perk.SalaryHead;
                                ObjOEDT.ProjectedAmount = perk.ProjectedAmount;
                                ObjOEDT.ActualAmount = perk.ActualAmount;
                                ObjOEDT.PayMonth = perk.PayMonth;
                                ObjOEDT.FinancialYear = FinancialYear;
                                ObjOEDT.DBTrack = perk.DBTrack;

                                errorList = ValidateObj(ObjOEDT);
                                if (errorList.Count > 0)
                                {
                                    return Json(new { success = false, data = rowno, responseText = string.Join(",", errorList) }, JsonRequestBehavior.AllowGet);
                                }
                                db.PerkTransT.Add(ObjOEDT);
                                db.SaveChanges();
                                foreach (var i in ids)
                                {
                                    EmployeePayroll OEmployeePayroll = db.EmployeePayroll
                                        .Include(e => e.Employee)
                                        .Include(e => e.PerkTransT)
                                        .Where(e => e.Employee.Id == i).SingleOrDefault();

                                    var iSalaryt = db.SalaryT.Where(r => r.EmployeePayroll_Id == OEmployeePayroll.Id && r.PayMonth == PayMonth).ToList();
                                    if (iSalaryt.Count() > 0)
                                    {
                                        return Json(new { data = rowno, success = true, responseText = "Salary is processed you can not upload " }, JsonRequestBehavior.AllowGet);
                                    }

                                    using (TransactionScope ts = new TransactionScope())
                                    {
                                        try
                                        {
                                            List<PerkTransT> OFAT = new List<PerkTransT>();
                                            OFAT.Add(db.PerkTransT.Find(ObjOEDT.Id));
                                            var aa = db.EmployeePayroll.Find(OEmployeePayroll.Id);
                                            OFAT.AddRange(aa.PerkTransT);
                                            aa.PerkTransT = OFAT;
                                            db.EmployeePayroll.Attach(aa);
                                            db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();
                                            db.Entry(aa).State = System.Data.Entity.EntityState.Detached;
                                            ts.Complete();

                                        }
                                        catch (DataException ex)
                                        {
                                            LogFile Logfile = new LogFile();
                                            ErrorLog Err = new ErrorLog()
                                            {
                                                ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                                                ExceptionMessage = ex.Message,
                                                ExceptionStackTrace = ex.StackTrace,
                                                LineNo = Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                                                LogTime = DateTime.Now
                                            };
                                            Logfile.CreateLogFile(Err);
                                            return Json(new { data = rowno, success = false, responseText = ex.InnerException.Message.ToString() }, JsonRequestBehavior.AllowGet);
                                        }
                                    }
                                    // }
                                }
                                return Json(new { data = rowno, success = true, responseText = "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);

                            }
                            else
                            {
                                return Json(new { data = rowno, success = false, responseText = "Select Employee" }, JsonRequestBehavior.AllowGet);
                            }

                        }
                    case "LVNEWREQ":
                        using (DataBaseContext db = new DataBaseContext())
                        {
                            LvNewReq LVOB = new LvNewReq();
                            String LvHeadList = form["LeaveCode"] != null ? form["LeaveCode"] : null;
                            String OpenBal = form["OpenBal"] == "0" ? "0" : form["OpenBal"];
                            String CloseBal = form["CloseBal"] == "0" ? "0" : form["CloseBal"];

                            String LvOccurances = form["LvOccurances"] == "0" ? "0" : form["LvOccurances"];
                            String LVCount = form["LVCount"] == "0" ? "0" : form["LVCount"];
                            String LvLapsed = form["LvLapsed"] == "0" ? "0" : form["LvLapsed"];

                            String PrefixCount = form["PrefixCount"] == "0" ? "0" : form["PrefixCount"];
                            String SufixCount = form["SufixCount"] == "0" ? "0" : form["SufixCount"];
                            String LvCreditDate = form["LvCreditDate"] != null ? form["LvCreditDate"] : null;

                            String LvCreditNextDate = form["LvCreditNextDate"] != null ? form["LvCreditNextDate"] : null;


                            if (OpenBal != null && OpenBal != "")
                            {
                                LVOB.OpenBal = Convert.ToDouble(OpenBal);
                            }
                            if (CloseBal != null && CloseBal != "")
                            {
                                LVOB.CloseBal = Convert.ToDouble(CloseBal);
                            }

                            if (LvOccurances != null && LvOccurances != "")
                            {
                                LVOB.LvOccurances = Convert.ToDouble(LvOccurances);
                            }
                            if (LVCount != null && LVCount != "")
                            {
                                LVOB.LVCount = Convert.ToDouble(LVCount);
                            }

                            if (PrefixCount != null && PrefixCount != "")
                            {
                                LVOB.PrefixCount = Convert.ToDouble(PrefixCount);
                            }
                            if (SufixCount != null && SufixCount != "")
                            {
                                LVOB.SufixCount = Convert.ToDouble(SufixCount);
                            }

                            if (LvCreditDate != null && LvCreditDate != "")
                            {
                                LVOB.LvCreditDate = Convert.ToDateTime(LvCreditDate);
                            }


                            if (LvCreditNextDate != null && LvCreditNextDate != "")
                            {
                                LVOB.LvCreditNextDate = Convert.ToDateTime(LvCreditNextDate);
                            }






                            string EmpCode = null;
                            EmployeeLeave OEmployeePayroll = null;
                            List<String> LvHeadCodeList = new List<String>();
                            List<int> LvHead = new List<int>();
                            if (LvHeadList != null && LvHeadList != "")
                            {
                                LvHeadCodeList = Utility.StringIdsToListString(LvHeadList);
                            }
                            foreach (var item in LvHeadCodeList)
                            {
                                var aa = db.LvHead.Where(e => e.LvCode == item).Select(e => e.Id).FirstOrDefault();
                                if (aa != 0)
                                {
                                    LvHead.Add(aa);
                                }
                            }

                            if (ids != null)
                            {
                                foreach (var i in ids)
                                {
                                    Employee OEmployee = null;
                                    EmployeeLeave OEmployeeLeave = null;
                                    OEmployeeLeave = db.EmployeeLeave
                                                   .Include(q => q.Employee)
                                                   .Include(q => q.Employee.ServiceBookDates)
                                                   .Include(e => e.LvOpenBal)
                                                   .Include(e => e.LvOpenBal.Select(r => r.LvHead))
                                                   .Where(e => e.Employee.Id == i).AsNoTracking().AsParallel().SingleOrDefault();
                                    if (OEmployeeLeave != null)
                                    {
                                        var OEmployeeLeaveS = OEmployeeLeave.Employee.ServiceBookDates.ServiceLastDate;
                                        var OEmployeeLeaveR = OEmployeeLeave.Employee.ServiceBookDates.ResignationDate;
                                        if (OEmployeeLeaveS == null && OEmployeeLeaveR == null)
                                        {
                                            OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct)
                                                .Where(r => r.Id == i).AsNoTracking().AsParallel().SingleOrDefault();

                                            OEmployeePayroll
                                              = db.EmployeeLeave
                                              .Include(e => e.LvNewReq)
                                              .Include(e => e.LvNewReq.Select(t => t.LeaveHead))
                                              .Include(e => e.LvNewReq.Select(t => t.LeaveCalendar))
                                              .Where(e => e.Employee.Id == i).SingleOrDefault();

                                            foreach (int LH in LvHead)
                                            {
                                                var val = db.LvHead.Find(LH);
                                                LVOB.LeaveHead = val;

                                                var EmpLeavT = OEmployeeLeave.LvOpenBal != null ? OEmployeeLeave.LvOpenBal.Where(e => e.LvHead.Id == LVOB.LeaveHead.Id) : null;
                                                if (EmpLeavT != null && EmpLeavT.Count() > 0)
                                                {
                                                    if (EmpCode == null || EmpCode == "")
                                                    {
                                                        EmpCode = OEmployee.EmpCode;
                                                    }
                                                    else
                                                    {
                                                        EmpCode = EmpCode + ", " + OEmployee.EmpCode;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            if (EmpCode != null)
                            {
                                return Json(new { data = rowno, success = false, responseText = "LeaveOpenBal already exists for employee " + EmpCode + "." }, JsonRequestBehavior.AllowGet);
                            }

                            if (!string.IsNullOrEmpty(sEmpCode))
                            {
                                foreach (var i in ids)
                                {
                                    EmployeeLeave OEmployeeLeave = db.EmployeeLeave.Include(e => e.Employee).Include(e => e.LvOpenBal)
                                                       .Include(e => e.LvOpenBal.Select(r => r.LvHead)).Where(e => e.Employee.Id == i).SingleOrDefault();

                                    Employee OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct).Where(r => r.Id == i).SingleOrDefault(); //

                                    if (LvHead.Count == 0)
                                    {
                                        return Json(new { data = rowno, success = false, responseText = "Please enter valid Leave head" }, JsonRequestBehavior.AllowGet);
                                    }

                                    foreach (int LH in LvHead)
                                    {
                                        var val = db.LvHead.Find(LH);
                                        LVOB.LeaveHead = val;

                                        LVOB.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                                        LVOB.LeaveCalendar = db.Calendar.Where(e => e.Name.LookupVal.ToUpper() == "LEAVECALENDAR" && e.Default == true).FirstOrDefault();



                                        LvNewReq lvnew = new LvNewReq()
                                        {
                                            ReqDate = System.DateTime.Now,
                                            OpenBal = LVOB.OpenBal,
                                            LVCount = LVOB.LVCount,
                                            LvOccurances = LVOB.LvOccurances,
                                            TrClosed = false,
                                            Narration = "Leave Opening Balance",
                                            WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "0").FirstOrDefault(),//db.LookupValue.Where(e => e.LookupVal == "0").SingleOrDefault(),
                                            DBTrack = LVOB.DBTrack,
                                            LeaveCalendar = LVOB.LeaveCalendar,
                                            LeaveHead = LVOB.LeaveHead,
                                            CloseBal = LVOB.CloseBal,
                                            LvCreditDate = LVOB.LvCreditDate,
                                            LvCreditNextDate = Convert.ToDateTime(LvCreditNextDate)
                                        };
                                        errorList = ValidateObj(lvnew);
                                        if (errorList.Count > 0)
                                        {
                                            return Json(new { data = rowno, success = false, responseText = string.Join(",", errorList) }, JsonRequestBehavior.AllowGet);
                                        }
                                        //
                                        var CheckNewLvCodeInLvNewreq = OEmployeePayroll.LvNewReq.Where(e => e.LeaveHead.Id == LVOB.LeaveHead.Id && e.LeaveCalendar.Id == LVOB.LeaveCalendar.Id).Count();
                                        if (CheckNewLvCodeInLvNewreq > 0)
                                        {
                                            Msg = "This Leave Head Allready Exist in Current Year!!!";
                                            return Json(new Utility.JsonReturnClass { data = rowno, success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                        }

                                        var CheckFirstRecordInLvNewreq = OEmployeePayroll.LvNewReq.Where(e => e.LeaveHead.Id == LVOB.LeaveHead.Id).Count();
                                        if (CheckFirstRecordInLvNewreq > 0)
                                        {
                                            Msg = "You can't upload this leave head data!!!";
                                            return Json(new Utility.JsonReturnClass { data = rowno, success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                        }

                                        //  var emplo = Convert.ToInt32(EmpCode);
                                        //var datafor = db.EmployeeLeave.Include(e => e.LvOpenBal)
                                        //    .Where(a => a.Employee.Id == emplo &&
                                        //        a.LvOpenBal.Any(aa => (aa.LvHead.Id == LVOB.LvHead.Id)
                                        //            && (aa.LvCalendar.Id == LVOB.LvCalendar.Id))).Count();

                                        //if (datafor > 0)
                                        //{
                                        //    Msg = " The OpenBal For this employee with " + LVOB.LvHead.LvName + ", And " + LVOB.LvCalendar.FullDetails + " Already Generated";
                                        //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        //}

                                        using (TransactionScope ts = new TransactionScope())
                                        {
                                            try
                                            {

                                                lvnew.GeoStruct = db.GeoStruct.Find(OEmployee.GeoStruct.Id);
                                                lvnew.PayStruct = db.PayStruct.Find(OEmployee.PayStruct.Id);
                                                lvnew.FuncStruct = db.FuncStruct.Find(OEmployee.FuncStruct.Id);

                                                db.LvNewReq.Add(lvnew);
                                                db.SaveChanges();
                                                List<LvNewReq> OLVNEW = new List<LvNewReq>();

                                                OLVNEW.Add(lvnew);
                                                if (OEmployeePayroll == null)
                                                {
                                                    EmployeeLeave OTEP = new EmployeeLeave()
                                                    {
                                                        Employee = db.Employee.Find(OEmployee.Id),
                                                        LvNewReq = OLVNEW,
                                                        DBTrack = LVOB.DBTrack
                                                    };
                                                    db.EmployeeLeave.Add(OTEP);
                                                    db.SaveChanges();
                                                }
                                                else
                                                {
                                                    var aa = db.EmployeeLeave.Find(OEmployeePayroll.Id);
                                                    OLVNEW.AddRange(aa.LvNewReq);
                                                    aa.LvNewReq = OLVNEW;
                                                    //OEmployeePayroll.DBTrack = dbt;
                                                    db.EmployeeLeave.Attach(aa);
                                                    db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                                    db.SaveChanges();
                                                    db.Entry(aa).State = System.Data.Entity.EntityState.Detached;

                                                }
                                                ts.Complete();


                                            }
                                            catch (DataException ex)
                                            {
                                                return Json(new { data = rowno, success = false, responseText = ex.InnerException }, JsonRequestBehavior.AllowGet);
                                            }
                                        }
                                    }
                                }
                                return Json(new { data = rowno, success = true, responseText = "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                return Json(new { data = rowno, success = false, responseText = "Select Employee" }, JsonRequestBehavior.AllowGet);
                            }
                        }
                    case "YEARLYPAYMENTT":
                        using (DataBaseContext db = new DataBaseContext())
                        {
                            YearlyPaymentT YRPT = new YearlyPaymentT();
                            String SalaryHeadlist = form["SalaryHead"] != null ? form["SalaryHead"] : null;
                            String ProcessMonth = form["ProcessMonth"] != null ? form["ProcessMonth"] : null;
                            String PeriodFrom = form["FromPeriod"] != null ? form["FromPeriod"] : null;
                            String PeriodTo = form["ToPeriod"] != null ? form["ToPeriod"] : null;
                            String AmountPaid = form["AmountPaid"] == "0" ? "0" : form["AmountPaid"];
                            String TDSAmount = form["TDSAmount"] == "0" ? "0" : form["TDSAmount"];
                            String OtherDeduction = form["OtherDeduction"] == "0" ? "0" : form["OtherDeduction"];
                            //String Narration = form["Narration"] != null ? form["Narration"] : null;
                            String PayMonth = form["PayMonth"] != null ? form["PayMonth"] : null;
                            String ReleaseDate = form["ReleaseDate"] != null ? form["ReleaseDate"] : null;
                            String ReleaseFlag = form["ReleaseFlag"] == "0" ? "1" : form["ReleaseFlag"];


                            if (ProcessMonth != null)
                            {
                                YRPT.ProcessMonth = Convert.ToDateTime(ProcessMonth).ToString("MM/yyyy");
                            }
                            if (PeriodFrom != null)
                            {
                                YRPT.FromPeriod = Convert.ToDateTime(PeriodFrom);
                            }
                            if (PeriodTo != null)
                            {
                                YRPT.ToPeriod = Convert.ToDateTime(PeriodTo);
                            }
                            if (AmountPaid != null)
                            {
                                YRPT.AmountPaid = Convert.ToDouble(AmountPaid);
                            }
                            if (TDSAmount != null)
                            {
                                YRPT.TDSAmount = Convert.ToDouble(TDSAmount);
                            }
                            if (OtherDeduction != null)
                            {
                                YRPT.OtherDeduction = Convert.ToDouble(OtherDeduction);
                            }
                            //if (Narration != null)
                            //{
                            //    YRPT.Narration = Convert.ToString(Narration);
                            //}
                            //if (PayMonth != null)
                            //{
                            //    YRPT.PayMonth = PayMonth;
                            //}
                            if (PayMonth != null && PayMonth != "")
                            {
                                YRPT.PayMonth = Convert.ToDateTime(PayMonth).ToString("MM/yyyy");
                            }
                            if (ReleaseDate != null)
                            {
                                YRPT.ReleaseDate = Convert.ToDateTime(ReleaseDate); ;
                            }
                            if (ReleaseFlag != null)
                            {
                                YRPT.ReleaseFlag = Convert.ToBoolean(ReleaseFlag);
                            }


                            string EmpCode = null;
                            List<String> SalaryHeadCodeList = new List<String>();
                            List<int> SalHead = new List<int>();
                            if (SalaryHeadlist != null && SalaryHeadlist != "")
                            {
                                SalaryHeadCodeList = Utility.StringIdsToListString(SalaryHeadlist);
                            }
                            foreach (var item in SalaryHeadCodeList)
                            {
                                var aa = db.SalaryHead.Where(e => e.Code == item).Select(e => e.Id).FirstOrDefault();
                                if (aa != 0)
                                {
                                    SalHead.Add(aa);
                                }
                            }

                            if (ids != null)
                            {
                                foreach (var i in ids)
                                {
                                    Employee OEmployee = null;
                                    EmployeePayroll OEmployeePayroll = null;
                                    OEmployeePayroll = db.EmployeePayroll
                                                   .Include(q => q.Employee)
                                                   .Include(q => q.Employee.ServiceBookDates)
                                                   .Include(e => e.YearlyPaymentT)
                                                   .Include(e => e.YearlyPaymentT.Select(r => r.SalaryHead))
                                                   .Where(e => e.Employee.Id == i).AsNoTracking().AsParallel().SingleOrDefault();
                                    if (OEmployeePayroll != null)
                                    {
                                        var OEmployeeLeaveS = OEmployeePayroll.Employee.ServiceBookDates.ServiceLastDate;
                                        var OEmployeeLeaveR = OEmployeePayroll.Employee.ServiceBookDates.ResignationDate;
                                        if (OEmployeeLeaveS == null && OEmployeeLeaveR == null)
                                        {
                                            OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct)
                                                .Where(r => r.Id == i).AsNoTracking().AsParallel().SingleOrDefault();

                                            foreach (int SH in SalHead)
                                            {
                                                var val = db.SalaryHead.Find(SH);
                                                YRPT.SalaryHead = val;

                                                var EmpSalT = OEmployeePayroll.YearlyPaymentT != null ? OEmployeePayroll.YearlyPaymentT.Where(e => e.PayMonth == PayMonth && e.SalaryHead.Id == YRPT.SalaryHead.Id && e.FromPeriod.Value.ToShortDateString() == PeriodFrom.ToString()) : null;
                                                if (EmpSalT != null && EmpSalT.Count() > 0)
                                                {
                                                    if (EmpCode == null || EmpCode == "")
                                                    {
                                                        EmpCode = OEmployee.EmpCode;
                                                    }
                                                    else
                                                    {
                                                        EmpCode = EmpCode + ", " + OEmployee.EmpCode;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            if (EmpCode != null)
                            {
                                return Json(new { data = rowno, success = false, responseText = "YearlyPaymentT already exists for employee " + EmpCode + "." }, JsonRequestBehavior.AllowGet);
                            }

                            if (!string.IsNullOrEmpty(sEmpCode))
                            {
                                foreach (var i in ids)
                                {

                                    EmployeePayroll OEmployeePayroll = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.YearlyPaymentT)
                                                      .Include(e => e.YearlyPaymentT.Select(r => r.SalaryHead)).Where(e => e.Employee.Id == i).SingleOrDefault();

                                    Employee OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct).Include(e => e.ServiceBookDates).Where(r => r.Id == i).SingleOrDefault();

                                    if (SalHead.Count == 0)
                                    {
                                        return Json(new { data = rowno, success = false, responseText = "Please enter valid Salary head" }, JsonRequestBehavior.AllowGet);
                                    }
                                    foreach (int SH in SalHead)
                                    {
                                        var val = db.SalaryHead.Find(SH);
                                        if (val.InPayslip == true)
                                        {
                                            var salaryt = db.SalaryT.Where(e => e.EmployeePayroll_Id == OEmployeePayroll.Id && e.PayMonth == YRPT.PayMonth).SingleOrDefault();
                                            if (salaryt != null)
                                            {
                                                return Json(new { data = rowno, success = false, responseText = val.Name + " head appear in payslip and salary processed for month " + YRPT.PayMonth + ". Hence can't upload." }, JsonRequestBehavior.AllowGet);
                                            }
                                        }
                                        YRPT.SalaryHead = val;
                                        YRPT.Narration = "Upload Excel for " + SalaryHeadlist;
                                        YRPT.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                                        int OFinancialYear = db.Calendar.Where(e => e.Name.LookupVal.ToUpper() == "FINANCIALYEAR" && e.Default == true).FirstOrDefault().Id;

                                        YearlyPaymentT ObjYRPT = new YearlyPaymentT();
                                        {
                                            ObjYRPT.SalaryHead = YRPT.SalaryHead;
                                            ObjYRPT.ProcessMonth = YRPT.ProcessMonth;
                                            ObjYRPT.FromPeriod = YRPT.FromPeriod;
                                            ObjYRPT.ToPeriod = YRPT.ToPeriod;
                                            ObjYRPT.AmountPaid = YRPT.AmountPaid;
                                            ObjYRPT.TDSAmount = YRPT.TDSAmount;
                                            ObjYRPT.OtherDeduction = YRPT.OtherDeduction;
                                            ObjYRPT.Narration = YRPT.Narration;
                                            ObjYRPT.PayMonth = YRPT.PayMonth;
                                            ObjYRPT.ReleaseDate = YRPT.ReleaseDate;
                                            ObjYRPT.ReleaseFlag = YRPT.ReleaseFlag;
                                            ObjYRPT.GeoStruct = db.GeoStruct.Find(OEmployee.GeoStruct != null ? OEmployee.GeoStruct.Id : 0);
                                            ObjYRPT.FuncStruct = db.FuncStruct.Find(OEmployee.FuncStruct != null ? OEmployee.FuncStruct.Id : 0);
                                            ObjYRPT.PayStruct = db.PayStruct.Find(OEmployee.PayStruct.Id != null ? OEmployee.PayStruct.Id : 0);
                                            ObjYRPT.DBTrack = YRPT.DBTrack;
                                            ObjYRPT.FinancialYear = db.Calendar.Where(e => e.Id == OFinancialYear).SingleOrDefault();
                                        }

                                        errorList = ValidateObj(ObjYRPT);
                                        if (errorList.Count > 0)
                                        {
                                            return Json(new { success = false, data = rowno, responseText = string.Join(",", errorList) }, JsonRequestBehavior.AllowGet);
                                        }

                                        using (TransactionScope ts = new TransactionScope())
                                        {
                                            try
                                            {
                                                List<YearlyPaymentT> OFAT = new List<YearlyPaymentT>();

                                                db.YearlyPaymentT.Add(ObjYRPT);
                                                db.SaveChanges();
                                                OFAT.Add(ObjYRPT);


                                                if (OEmployeePayroll == null)
                                                {
                                                    EmployeePayroll OTEP = new EmployeePayroll()
                                                    {
                                                        Employee = db.Employee.Find(OEmployee.Id),
                                                        YearlyPaymentT = OFAT,
                                                        DBTrack = YRPT.DBTrack
                                                    };

                                                    db.EmployeePayroll.Add(OTEP);
                                                    db.SaveChanges();
                                                }
                                                else
                                                {
                                                    var aa = db.EmployeePayroll.Find(OEmployeePayroll.Id);
                                                    OFAT.AddRange(OEmployeePayroll.YearlyPaymentT);
                                                    aa.YearlyPaymentT = OFAT;

                                                    //OEmployeePayroll.DBTrack = dbt;
                                                    db.EmployeePayroll.Attach(aa);
                                                    db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                                    db.SaveChanges();
                                                    db.Entry(aa).State = System.Data.Entity.EntityState.Detached;
                                                }

                                                ts.Complete();
                                                //return Json(new { sucess = true, responseText = "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
                                                //List<string> Msgs = new List<string>();
                                                //Msgs.Add("  Data Saved successfully  ");
                                                //return Json(new Utility.JsonReturnClass { success = true, responseText = Msgs }, JsonRequestBehavior.AllowGet);
                                            }
                                            catch (DataException ex)
                                            {
                                                LogFile Logfile = new LogFile();
                                                ErrorLog Err = new ErrorLog()
                                                {
                                                    ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                                                    ExceptionMessage = ex.Message,
                                                    ExceptionStackTrace = ex.StackTrace,
                                                    LineNo = Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                                                    LogTime = DateTime.Now
                                                };
                                                Logfile.CreateLogFile(Err);
                                                return Json(new { sucess = false, responseText = ex.InnerException }, JsonRequestBehavior.AllowGet);
                                            }
                                            catch (Exception ex)
                                            {
                                                LogFile Logfile = new LogFile();
                                                ErrorLog Err = new ErrorLog()
                                                {
                                                    ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                                                    ExceptionMessage = ex.Message,
                                                    ExceptionStackTrace = ex.StackTrace,
                                                    LineNo = Convert.ToString(new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                                                    LogTime = DateTime.Now
                                                };
                                                Logfile.CreateLogFile(Err);

                                                // Msg.Add(ex.Message);
                                                //return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                            }
                                        }
                                    }
                                }
                                return Json(new { data = rowno, success = true, responseText = "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                return Json(new { data = rowno, success = false, responseText = "Select Employee" }, JsonRequestBehavior.AllowGet);
                            }
                        }
                    case "ITINVESTMENTPAYMENT":
                        using (DataBaseContext db = new DataBaseContext())
                        {
                            ITInvestmentPayment ITIP = new ITInvestmentPayment();

                            String ITInvestmentslist = form["ITInvestment"] == "0" ? "" : form["ITInvestment"].Replace("#", " ").Trim();
                            String ITSubInvestmentPaymentlist = form["ITSubInvestmentPayment"] == "0" ? "" : form["ITSubInvestmentPayment"];
                            String InvestmentDate = form["InvestmentDate"] != null ? form["InvestmentDate"] : null;
                            String ActualInvestment = form["ActualInvestment"] == "0" ? "0" : form["ActualInvestment"];
                            String DeclaredInvestment = form["DeclaredInvestment"] == "0" ? "0" : form["DeclaredInvestment"];


                            if (InvestmentDate != null)
                            {
                                ITIP.InvestmentDate = Convert.ToDateTime(InvestmentDate);
                            }
                            if (ActualInvestment != null)
                            {
                                ITIP.ActualInvestment = Convert.ToDouble(ActualInvestment);
                            }
                            if (DeclaredInvestment != null)
                            {
                                ITIP.DeclaredInvestment = Convert.ToDouble(DeclaredInvestment);
                            }


                            string EmpCode = null;
                            List<String> ITInvestmentsCodeList = new List<String>();
                            List<int> ITInvestmentsHead = new List<int>();
                            if (ITInvestmentslist != null && ITInvestmentslist != "")
                            {
                                ITInvestmentsCodeList = Utility.StringIdsToListString(ITInvestmentslist);
                            }
                            foreach (var item in ITInvestmentsCodeList)
                            {
                                var aa = db.ITInvestment.Where(e => e.ITInvestmentName == item).Select(e => e.Id).FirstOrDefault();
                                //var aa = db.ITInvestment.Where(e => e.ITInvestmentName == item && e.SalaryHead.Id == ).Select(e => e.Id).FirstOrDefault();
                                if (aa != 0)
                                {
                                    //   var salHead = db.SalaryHead.Where(e => e.Code == item).Select(e => e.Id).FirstOrDefault();

                                    var ab = db.ITInvestment.Where(e => e.IsSalaryHead == false && e.Id == aa).Select(x => x.Id).FirstOrDefault();
                                    if (ab != 0)
                                    {
                                        ITInvestmentsHead.Add(ab);
                                    }
                                    else
                                    {
                                        Msg = "ITInvestment Salaray head available ";
                                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    }
                                }
                            }

                            //List<String> ITSubInvestmentPaymentCodeList = new List<String>();
                            //List<int> ITSubInvestmentPaymentHead = new List<int>();
                            //if (ITSubInvestmentPaymentlist != null && ITSubInvestmentPaymentlist != "")
                            //{
                            //    ITSubInvestmentPaymentCodeList = Utility.StringIdsToListString(ITSubInvestmentPaymentlist);
                            //}
                            //foreach (var item in ITSubInvestmentPaymentCodeList)
                            //{
                            //    var ab = db.ITSubInvestment.Where(e => e.SubInvestmentName == item).Select(e => e.Id).FirstOrDefault();
                            //    if (ab != 0)
                            //    {
                            //        ITSubInvestmentPaymentHead.Add(ab);
                            //    }
                            //}

                            if (ids != null)
                            {
                                foreach (var i in ids)
                                {
                                    Employee OEmployee = null;
                                    EmployeePayroll OEmployeePayroll = null;
                                    OEmployeePayroll = db.EmployeePayroll
                                                 .Include(q => q.Employee)
                                                 .Include(q => q.Employee.ServiceBookDates)
                                                 .Include(e => e.ITInvestmentPayment)
                                                 .Include(e => e.ITInvestmentPayment.Select(r => r.ITInvestment))
                                                 .Include(e => e.ITInvestmentPayment.Select(r => r.ITSubInvestmentPayment))
                                                 .Where(e => e.Employee.Id == i).AsNoTracking().AsParallel().SingleOrDefault();

                                    if (OEmployeePayroll != null)
                                    {
                                        var OEmployeePayrollS = OEmployeePayroll.Employee.ServiceBookDates.ServiceLastDate;
                                        var OEmployeePayrolleR = OEmployeePayroll.Employee.ServiceBookDates.ResignationDate;
                                        if (OEmployeePayrollS == null && OEmployeePayrolleR == null)
                                        {
                                            OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct)
                                           .Where(r => r.Id == i).AsNoTracking().AsParallel().SingleOrDefault();

                                            foreach (int ITH in ITInvestmentsHead)
                                            {
                                                var val = db.ITInvestment.Find(ITH);
                                                ITIP.ITInvestment = val;

                                                var EmpSalT = OEmployeePayroll.ITInvestmentPayment != null ? OEmployeePayroll.ITInvestmentPayment.Where(e => e.ITInvestment.Id == ITIP.ITInvestment.Id) : null;
                                                if (EmpSalT != null && EmpSalT.Count() > 0)
                                                {
                                                    if (EmpCode == null || EmpCode == "")
                                                    {
                                                        EmpCode = OEmployee.EmpCode;
                                                    }
                                                    else
                                                    {
                                                        EmpCode = EmpCode + ", " + OEmployee.EmpCode;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            if (EmpCode != null)
                            {
                                return Json(new { data = rowno, success = false, responseText = "ITInvestmentPayment already exists for employee " + EmpCode + "." }, JsonRequestBehavior.AllowGet);
                            }

                            if (!string.IsNullOrEmpty(sEmpCode))
                            {
                                foreach (var i in ids)
                                {
                                    int OEmployee = db.Employee.AsNoTracking().Where(r => r.Id == i).Select(q => q.Id).SingleOrDefault();

                                    EmployeePayroll OEmployeePayroll = db.EmployeePayroll
                                                      .Include(e => e.ITInvestmentPayment)
                                                      .Include(e => e.ITInvestmentPayment.Select(q => q.ITInvestment)).Include(e => e.ITInvestmentPayment.Select(q => q.ITSection))
                                                      .Include(e => e.ITInvestmentPayment.Select(r => r.FinancialYear)).AsNoTracking().Where(e => e.Employee.Id == OEmployee).SingleOrDefault();

                                    if (ITInvestmentsHead.Count == 0)
                                    {
                                        return Json(new { data = rowno, success = false, responseText = "Please enter valid ITInvestment head" }, JsonRequestBehavior.AllowGet);
                                    }

                                    foreach (int SH in ITInvestmentsHead)
                                    {
                                        var val = db.ITInvestment.Find(SH);
                                        ITIP.ITInvestment = val;

                                        ITIP.Narration = "Upload Excel Data";
                                        ITIP.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                                        int OFinancialYear = db.Calendar.Where(e => e.Name.LookupVal.ToUpper() == "FINANCIALYEAR" && e.Default == true).FirstOrDefault().Id;
                                        var OITSection = db.ITInvestment.Include(r => r.ITSection).Where(e => e.Id == val.Id).SingleOrDefault().ITSection;

                                        //var Osection = investmentsection.Select(r => r.ITInvestments.Where(e => e.Id == val.Id)).SingleOrDefault();

                                        if (OITSection == null)
                                        {
                                            return Json(new { data = rowno, success = false, responseText = "Investment name not bind with section " }, JsonRequestBehavior.AllowGet);

                                        }
                                        // int OITSection80C = db.ITSection.Include(E => E.ITSectionList).Where(e => e.ITSectionList.LookupVal.ToUpper() == "SECTION80C").FirstOrDefault().Id;

                                        ITInvestmentPayment ObjITIP = new ITInvestmentPayment();
                                        {
                                            ObjITIP.FinancialYear = db.Calendar.Where(e => e.Id == OFinancialYear).SingleOrDefault();
                                            //ObjITIP.ITSection = Osection.Select(e => e.ITSection_Id).FirstOrDefault(); //db.ITSection.Where(e => e.Id == OITSection80C).SingleOrDefault();
                                            ObjITIP.ITSection = OITSection;
                                            ObjITIP.ActualInvestment = ITIP.ActualInvestment;
                                            ObjITIP.DeclaredInvestment = ITIP.DeclaredInvestment;
                                            ObjITIP.InvestmentDate = ITIP.InvestmentDate;
                                            ObjITIP.ITInvestment = ITIP.ITInvestment;
                                            ObjITIP.ITSubInvestmentPayment = ITIP.ITSubInvestmentPayment;
                                            ObjITIP.Narration = ITIP.Narration;
                                            ObjITIP.DBTrack = ITIP.DBTrack;

                                        }

                                        errorList = ValidateObj(ObjITIP);
                                        if (errorList.Count > 0)
                                        {
                                            return Json(new { success = false, data = rowno, responseText = string.Join(",", errorList) }, JsonRequestBehavior.AllowGet);
                                        }

                                        using (TransactionScope ts = new TransactionScope())
                                        {
                                            try
                                            {
                                                db.ITInvestmentPayment.Add(ObjITIP);
                                                db.SaveChanges();
                                                List<ITInvestmentPayment> OFAT = new List<ITInvestmentPayment>();
                                                OFAT.Add(db.ITInvestmentPayment.Find(ObjITIP.Id));

                                                if (OEmployeePayroll == null)
                                                {
                                                    EmployeePayroll OTEP = new EmployeePayroll()
                                                    {
                                                        Employee = db.Employee.Find(OEmployee),
                                                        ITInvestmentPayment = OFAT,
                                                        DBTrack = ITIP.DBTrack
                                                    };
                                                    db.EmployeePayroll.Add(OTEP);
                                                    db.SaveChanges();
                                                }
                                                else
                                                {
                                                    EmployeePayroll aa = db.EmployeePayroll.Include(e => e.ITInvestmentPayment).Where(e => e.Employee.Id == OEmployee).SingleOrDefault();
                                                    OFAT.AddRange(aa.ITInvestmentPayment);
                                                    aa.ITInvestmentPayment = OFAT;
                                                    //OEmployeePayroll.DBTrack = dbt;
                                                    db.EmployeePayroll.Attach(aa);
                                                    db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                                    db.SaveChanges();
                                                    db.Entry(aa).State = System.Data.Entity.EntityState.Detached;
                                                }
                                                ts.Complete();
                                                ////return Json(new Object[] { "", "", "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
                                                //List<string> Msgs = new List<string>();
                                                //Msgs.Add("  Data Saved successfully  ");
                                                //return Json(new Utility.JsonReturnClass { success = true, responseText = Msgs }, JsonRequestBehavior.AllowGet);

                                            }
                                            catch (DataException ex)
                                            {
                                                //LogFile Logfile = new LogFile();
                                                //ErrorLog Err = new ErrorLog()
                                                //{
                                                //    ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                                                //    ExceptionMessage = ex.Message,
                                                //    ExceptionStackTrace = ex.StackTrace,
                                                //    LineNo = Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                                                //    LogTime = DateTime.Now
                                                //};
                                                //Logfile.CreateLogFile(Err);
                                                //return Json(new { sucess = false, responseText = ex.InnerException }, JsonRequestBehavior.AllowGet);
                                            }
                                            catch (Exception ex)
                                            {
                                                LogFile Logfile = new LogFile();
                                                ErrorLog Err = new ErrorLog()
                                                {
                                                    ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                                                    ExceptionMessage = ex.Message,
                                                    ExceptionStackTrace = ex.StackTrace,
                                                    LineNo = Convert.ToString(new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                                                    LogTime = DateTime.Now
                                                };
                                                Logfile.CreateLogFile(Err);

                                                // Msg.Add(ex.Message);
                                                //return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                            }
                                        }
                                    }
                                }
                                return Json(new { data = rowno, success = true, responseText = "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                return Json(new { data = rowno, success = false, responseText = "Select Employee" }, JsonRequestBehavior.AllowGet);
                            }
                        }

                    case "INCREMENTSERVICEBOOK":
                        using (DataBaseContext db = new DataBaseContext())
                        {
                            IncrementServiceBook ISB = new IncrementServiceBook();
                            String ActivityNamelist = form["IncrActivity"] != null ? form["IncrActivity"] : null;
                            ActivityNamelist = ActivityNamelist.Replace("#", " ");
                            String ActivityDate = form["ProcessIncrDate"] != null ? form["ProcessIncrDate"] : null;
                            //String OldBasic = form["OldBasic"] == "0" ? "0" : form["OldBasic"];
                            //String NewBasic = form["NewBasic"] == "0" ? "0" : form["NewBasic"];
                            String OldBasic = form["OldBasic"] != null ? form["OldBasic"] : null;
                            String NewBasic = form["NewBasic"] != null ? form["NewBasic"] : null;


                            var date = Convert.ToDateTime(ActivityDate).ToString("MM/yyyy");
                            //if (ActivityNamelist != null)
                            //{
                            //    ISB. ActivityNamelist = ActivityNamelist;
                            //}
                            if (ActivityDate != null)
                            {
                                ISB.ProcessIncrDate = Convert.ToDateTime(ActivityDate);
                                ISB.ReleaseDate = Convert.ToDateTime(ActivityDate);
                                ISB.OrignalIncrDate = Convert.ToDateTime(ActivityDate);
                            }
                            if (OldBasic != null)
                            {
                                ISB.OldBasic = Convert.ToDouble(OldBasic);
                            }
                            if (NewBasic != null)
                            {
                                ISB.NewBasic = Convert.ToDouble(NewBasic);
                            }
                            //Kerla bank
                            string comp = "";
                            string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\App_Data\\Menu_Json";
                            bool exists = System.IO.Directory.Exists(requiredPath);
                            string localPath;
                            if (!exists)
                            {
                                localPath = new Uri(requiredPath).LocalPath;
                                System.IO.Directory.CreateDirectory(localPath);
                            }
                            string path = requiredPath + @"\SalRule" + ".txt";
                            localPath = new Uri(path).LocalPath;
                            if (System.IO.File.Exists(localPath))
                            {

                                using (var streamReader = new StreamReader(localPath))
                                {
                                    string line;

                                    while ((line = streamReader.ReadLine()) != null)
                                    {
                                        comp = line.Split('_')[0];
                                        if (comp != "KERALA")
                                        {

                                            if (ISB.OldBasic == ISB.NewBasic)
                                            {
                                                return Json(new { data = rowno, success = false, responseText = "OldBasic and NewBasic should not  be same." }, JsonRequestBehavior.AllowGet);
                                            }
                                        }
                                        else if (comp == "")
                                        {
                                            if (ISB.OldBasic == ISB.NewBasic)
                                            {
                                                return Json(new { data = rowno, success = false, responseText = "OldBasic and NewBasic should not  be same." }, JsonRequestBehavior.AllowGet);
                                            }
                                        }

                                    }
                                }
                            }
                            else
                            {
                                if (ISB.OldBasic == ISB.NewBasic)
                                {
                                    return Json(new { data = rowno, success = false, responseText = "OldBasic and NewBasic should not  be same." }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            string Corporate = form["Corporate"] == "null" ? null : form["Corporate"];
                            string Region = form["Region"] == "null" ? null : form["Region"];
                            string Company = form["Company"] == "null" ? null : form["Company"];
                            string Division = form["Division"] == "null" ? null : form["Division"];
                            string Location = form["Location"] == "null" ? null : form["Location"];
                            string Department = form["Department"] == "null" ? null : form["Department"];
                            string Group = form["Group"] == "null" ? null : form["Group"];
                            string Unit = form["Unit"] == "null" ? null : form["Unit"];

                            var Get_Geo_Id = db.GeoStruct.Include(e => e.Company)
                                .Include(e => e.Corporate).Include(e => e.Department).Include(e => e.Department.DepartmentObj)
                                .Include(e => e.Division).Include(e => e.Group).Include(e => e.Location).Include(e => e.Location.LocationObj).Include(e => e.Region).Include(e => e.Unit)
                                .ToList();
                            if (Corporate != null && Corporate != "#")
                            {
                                var id = Corporate.Trim();
                                Get_Geo_Id = Get_Geo_Id.Where(e => e.Corporate != null && e.Corporate.Code == id).ToList();
                            }
                            if (Region != null && Region != "#")
                            {
                                var id = Region.Trim();
                                Get_Geo_Id = Get_Geo_Id.Where(e => e.Region != null && e.Region.Code == id).ToList();
                            }
                            if (Company != null && Company != "#")
                            {
                                var id = Company.Trim();
                                Get_Geo_Id = Get_Geo_Id.Where(e => e.Company != null && e.Company.Code == id).ToList();
                            }
                            if (Division != null && Division != "#")
                            {
                                var id = Division.Trim();
                                Get_Geo_Id = Get_Geo_Id.Where(e => e.Division != null && e.Division.Code == id).ToList();
                            }
                            if (Location != null && Location != "#")
                            {
                                var id = Location.Trim();
                                Get_Geo_Id = Get_Geo_Id.Where(e => e.Location != null && e.Location.LocationObj != null && e.Location.LocationObj.LocCode == id).ToList();
                            }
                            if (Department != null && Department != "#")
                            {
                                var id = Department.Trim();
                                Get_Geo_Id = Get_Geo_Id.Where(e => e.Department != null && e.Department.DepartmentObj != null && e.Department.DepartmentObj.DeptCode == id).ToList();
                            }
                            if (Group != null && Group != "#")
                            {
                                var id = Group.Trim();
                                Get_Geo_Id = Get_Geo_Id.Where(e => e.Group != null && e.Group.Code == id).ToList();
                            }
                            if (Unit != null && Unit != "#")
                            {
                                var id = Unit.Trim();
                                Get_Geo_Id = Get_Geo_Id.Where(e => e.Unit != null && e.Unit.Code == id).ToList();
                            }

                            if (Get_Geo_Id != null && Get_Geo_Id.Count == 1)
                            {
                                ISB.GeoStruct = Get_Geo_Id[0];
                            }

                            if (ISB.GeoStruct == null)
                            {
                                return Json(new { data = rowno, success = false, responseText = "GeoStruct not found" }, JsonRequestBehavior.AllowGet);
                            }
                            //paystruct
                            var Get_Pay_Id = db.PayStruct.Include(e => e.Company).Include(e => e.Grade).Include(e => e.JobStatus).Include(e => e.JobStatus.EmpStatus).Include(e => e.JobStatus.EmpActingStatus).Include(e => e.Level).ToList();
                            string Grade = form["Grade"] == "null" ? null : form["Grade"];
                            string Level = form["Level"] == "null" ? null : form["Level"];
                            string EmpStatus = form["EmpStatus"] == "null" ? null : form["EmpStatus"];
                            string EmpActingStatus = form["EmpActingStatus"] == "null" ? null : form["EmpActingStatus"];
                            if (Grade != null && Grade != "#")
                            {
                                var id = Grade.Trim();
                                Get_Pay_Id = Get_Pay_Id.Where(e => e.Grade != null && e.Grade.Code == id).ToList();
                            }
                            if (Level != null && Level != "#")
                            {
                                var id = Level.Trim();
                                Get_Pay_Id = Get_Pay_Id.Where(e => e.Level != null && e.Level.Code == id).ToList();
                            }
                            if (EmpStatus != null && EmpStatus != "#")
                            {
                                string id = EmpStatus.Trim().ToUpper();
                                Get_Pay_Id = Get_Pay_Id.Where(e => e.JobStatus != null && e.JobStatus.EmpStatus != null && e.JobStatus.EmpStatus.LookupVal.ToUpper() == id).ToList();
                            }
                            if (EmpActingStatus != null && EmpActingStatus != "#")
                            {
                                string id = EmpActingStatus.Trim().ToUpper();
                                Get_Pay_Id = Get_Pay_Id.Where(e => e.JobStatus != null && e.JobStatus.EmpActingStatus != null && e.JobStatus.EmpActingStatus.LookupVal.ToUpper() == id).ToList();
                            }
                            if (Get_Pay_Id != null && Get_Pay_Id.Count == 1)
                            {
                                ISB.PayStruct = Get_Pay_Id[0];
                            }

                            if (ISB.PayStruct == null)
                            {
                                return Json(new { data = rowno, success = false, responseText = "PayStruct not found" }, JsonRequestBehavior.AllowGet);
                            }
                            //Funstrct
                            string Job = form["Job"] == "null" ? null : form["Job"];
                            string Position = form["Position"] == "null" ? null : form["Position"];
                            var Get_Fun_Id = db.FuncStruct.Include(e => e.Company).Include(e => e.Job).Include(e => e.JobPosition).ToList();
                            if (Job != null && Job != "#")
                            {
                                var id = Job.Trim();
                                Get_Fun_Id = Get_Fun_Id.Where(e => e.Job != null && e.Job.Code == id).ToList();
                            }
                            if (Position != null && Position != "#")
                            {
                                var id = Position.Trim();
                                Get_Fun_Id = Get_Fun_Id.Where(e => e.JobPosition != null && e.JobPosition.JobPositionCode == id).ToList();
                            }
                            if (Get_Fun_Id != null && Get_Fun_Id.Count == 1)
                            {
                                ISB.FuncStruct = Get_Fun_Id[0];
                            }

                            if (ISB.FuncStruct == null)
                            {
                                return Json(new { data = rowno, success = false, responseText = "FuncStruct not found" }, JsonRequestBehavior.AllowGet);
                            }

                            int CompId = 0;
                            if (SessionManager.UserName != null)
                            {
                                CompId = Convert.ToInt32(Session["CompId"]);
                            }

                            //var check = db.CPIEntryT.Where(e => e.PayMonth == date).ToList();

                            //if (check.Count() == 0)
                            //{
                            //    Msg = "Kindly run CPI first and then try again";
                            //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //}

                            string EmpCode = null;
                            List<String> ActivityNameCodeList = new List<String>();
                            List<int> ActivityHead = new List<int>();
                            if (ActivityNamelist != null && ActivityNamelist != "")
                            {
                                ActivityNameCodeList = Utility.StringIdsToListString(ActivityNamelist);
                            }
                            foreach (var item in ActivityNameCodeList)
                            {
                                var aa = db.IncrActivity.Where(e => e.Name == item).Select(e => e.Id).FirstOrDefault();
                                if (aa != 0)
                                {
                                    ActivityHead.Add(aa);
                                }
                            }

                            if (ids != null)
                            {
                                foreach (var i in ids)
                                {
                                    Employee OEmployee = null;
                                    EmployeePayroll OEmployeePayroll = null;
                                    OEmployeePayroll = db.EmployeePayroll
                                                   .Include(q => q.Employee)
                                                   .Include(q => q.Employee.ServiceBookDates)
                                                   .Include(e => e.IncrementServiceBook)
                                                   .Include(e => e.IncrementServiceBook.Select(r => r.IncrActivity))
                                                   .Where(e => e.Employee.Id == i).AsNoTracking().AsParallel().SingleOrDefault();

                                    if (OEmployeePayroll != null)
                                    {
                                        var OEmployeePayrollS = OEmployeePayroll.Employee.ServiceBookDates.ServiceLastDate;
                                        var OEmployeePayrollR = OEmployeePayroll.Employee.ServiceBookDates.ResignationDate;
                                        if (OEmployeePayrollS == null && OEmployeePayrollR == null)
                                        {
                                            OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct)
                                                .Where(r => r.Id == i).AsNoTracking().AsParallel().SingleOrDefault();

                                            foreach (int SH in ActivityHead)
                                            {
                                                var val = db.IncrActivity.Find(SH);
                                                ISB.IncrActivity = val;

                                                var EmpSalT = OEmployeePayroll.IncrementServiceBook != null ? OEmployeePayroll.IncrementServiceBook.Where(e => e.IncrActivity.Id == ISB.IncrActivity.Id && e.ProcessIncrDate.Value.ToShortDateString() == ActivityDate.ToString()) : null;
                                                if (EmpSalT != null && EmpSalT.Count() > 0)
                                                {
                                                    if (EmpCode == null || EmpCode == "")
                                                    {
                                                        EmpCode = OEmployee.EmpCode;
                                                    }
                                                    else
                                                    {
                                                        EmpCode = EmpCode + ", " + OEmployee.EmpCode;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            if (EmpCode != null)
                            {
                                return Json(new { data = rowno, success = false, responseText = "IncrementServiceBook already exists for employee " + EmpCode + "." }, JsonRequestBehavior.AllowGet);
                            }

                            if (!string.IsNullOrEmpty(sEmpCode))
                            {
                                foreach (var i in ids)
                                {
                                    Employee OEmployee = null;
                                    EmployeePayroll OEmployeePayroll = null;
                                    OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct).Include(e => e.PayStruct.JobStatus)
                                                                                .Include(e => e.EmpOffInfo)
                                                                                .Where(r => r.Id == i).SingleOrDefault();

                                    OEmployeePayroll = db.EmployeePayroll.Include(e => e.IncrementServiceBook).Where(e => e.Employee.Id == i).SingleOrDefault();

                                    if (OEmployeePayroll.IncrementServiceBook.Any(d => d.ProcessIncrDate.Value.ToShortDateString() == ActivityDate.ToString()))
                                    {
                                        {
                                            Msg = "Already increment for Date= " + ActivityDate;
                                            // return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                            return Json(new { data = rowno, success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        }
                                    }

                                    int IncrementPolicyId = 0;
                                    int IncrActivityId = 0;
                                    string LookupVal = "";

                                    if (ActivityNamelist != null && ActivityNamelist != "")
                                    {
                                        //IncrActivityId = int.Parse(ActivityNamelist);
                                        IncrActivityId = db.IncrActivity.Where(e => e.Name == ActivityNamelist).Select(e => e.Id).FirstOrDefault();
                                        //LookupVal = db.LookupValue.Where(e => e.Id == IncrActivityId).Select(e => e.LookupVal.ToUpper()).SingleOrDefault();
                                        LookupVal = db.IncrActivity.Where(e => e.Id == IncrActivityId).Select(e => e.IncrList.LookupVal.ToUpper()).SingleOrDefault();
                                        //IncrementServiceBook.IncrActivity = db.IncrActivity.Where(e => e.Id == IncrActivityId).SingleOrDefault();
                                    }

                                    //if (IncrementPolicy != null && IncrementPolicy != "")
                                    //{
                                    //    IncrementPolicyId = int.Parse(IncrementPolicy);
                                    //}

                                    //if (OEmployee.GeoStruct != null)
                                    //    ISB.GeoStruct = db.GeoStruct.Find(OEmployee.GeoStruct.Id);

                                    //if (OEmployee.FuncStruct != null)
                                    //    ISB.FuncStruct = db.FuncStruct.Find(OEmployee.FuncStruct.Id);

                                    //if (OEmployee.PayStruct != null)
                                    //    ISB.PayStruct = db.PayStruct.Find(OEmployee.PayStruct.Id);

                                    if (ActivityHead.Count == 0)
                                    {
                                        return Json(new { data = rowno, success = false, responseText = "Please enter valid ActivityName." }, JsonRequestBehavior.AllowGet);
                                    }

                                    foreach (int SH in ActivityHead)
                                    {
                                        var val = db.IncrActivity.Find(SH);
                                        ISB.IncrActivity = val;
                                        ISB.Narration = "Increment Activity";
                                        ISB.ReleaseFlag = true;

                                        ISB.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                                        IncrementServiceBook ObjISB = new IncrementServiceBook();
                                        {
                                            ObjISB.IncrActivity = ISB.IncrActivity;
                                            ObjISB.ProcessIncrDate = ISB.ProcessIncrDate;
                                            ObjISB.IsRegularIncrDate = ISB.IsRegularIncrDate;
                                            ObjISB.OldBasic = ISB.OldBasic;
                                            ObjISB.NewBasic = ISB.NewBasic;
                                            ObjISB.OrignalIncrDate = ISB.OrignalIncrDate;
                                            ObjISB.ReleaseDate = ISB.ReleaseDate;
                                            ObjISB.Narration = ISB.Narration;// ISB.Narration;
                                            ObjISB.ReleaseFlag = ISB.ReleaseFlag;
                                            ObjISB.IsHold = false;
                                            ObjISB.GeoStruct = ISB.GeoStruct;
                                            ObjISB.FuncStruct = ISB.FuncStruct;
                                            ObjISB.PayStruct = ISB.PayStruct;
                                            ObjISB.DBTrack = ISB.DBTrack;
                                        }

                                        errorList = ValidateObj(ObjISB);
                                        if (errorList.Count > 0)
                                        {
                                            return Json(new { success = false, data = rowno, responseText = string.Join(",", errorList) }, JsonRequestBehavior.AllowGet);
                                        }

                                        using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                             new System.TimeSpan(0, 30, 0)))
                                        {
                                            if (Session["CompId"] != null)
                                                ISB.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                            //IncrementServiceBook.DBTrack = new DBTrack();
                                            //  IncrementServiceBook.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                            else
                                                ISB.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                                            //if (LookupVal.ToUpper() == "REGULAR")
                                            //{
                                            //    Process.ServiceBook.ServiceBookProcess(CompId, "INCREMENT_PROCESS", null, null, null, null, OEmployeePayroll.Id, LookupVal, ISB.ProcessIncrDate, false, true, 0);
                                            //}
                                            //else
                                            //{
                                            //    Process.ServiceBook.ServiceBookProcess(CompId, "INCREMENT_PROCESS", null, null, null, null, OEmployeePayroll.Id, LookupVal, ISB.ProcessIncrDate, false, false, 0);
                                            //}
                                            List<IncrementServiceBook> OincrList = new List<IncrementServiceBook>();
                                            OincrList.Add(ISB);
                                            db.IncrementServiceBook.Add(ISB);

                                            db.SaveChanges();

                                            var a = db.EmployeePayroll.Include(e => e.IncrDataCalc).Where(e => e.Id == OEmployeePayroll.Id).SingleOrDefault();
                                            if (a != null && a.IncrDataCalc != null)
                                            {
                                                db.IncrDataCalc.Remove(a.IncrDataCalc);
                                                db.SaveChanges();
                                            }

                                            EmployeePayroll OEmpPay = db.EmployeePayroll.Find(OEmployeePayroll.Id);
                                            if (OEmpPay.IncrementServiceBook.Count() > 0)
                                            { OincrList.AddRange(OEmpPay.IncrementServiceBook); }
                                            OEmpPay.IncrementServiceBook = OincrList;
                                            db.EmployeePayroll.Attach(OEmpPay);
                                            db.Entry(OEmpPay).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();

                                            ts.Complete();
                                            // return Json(new Object[] { "", "", "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
                                            //    List<string> Msgs = new List<string>();
                                        }
                                    }
                                }
                                return Json(new { data = rowno, success = true, responseText = "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                return Json(new { data = rowno, success = false, responseText = "Select Employee" }, JsonRequestBehavior.AllowGet);
                            }
                        }

                    case "PROMOTIONSERVICEBOOK":
                        using (DataBaseContext db = new DataBaseContext())
                        {
                            PromotionServiceBook PSB = new PromotionServiceBook();
                            //String ActivityNamelist = form["IncrActivity"] != null ? form["IncrActivity"] : null;
                            String ActivityNamelist = form["PromotionActivity"] != null ? form["PromotionActivity"] : null;
                            ActivityNamelist = ActivityNamelist.Replace("#", " ");
                            String ActivityDate = form["ProcessPromoDate"] != null ? form["ProcessPromoDate"] : null;
                            String OldBasic = form["OldBasic"] != null ? form["OldBasic"] : null;
                            String NewBasic = form["NewBasic"] != null ? form["NewBasic"] : null;
                            String Fittment = form["Fittment"] != null ? form["Fittment"] : null;

                            String Narration = form["Narration"] != null ? form["Narration"] : null;
                            Narration = Narration.Replace("#", " ");

                            var date = Convert.ToDateTime(ActivityDate).ToString("MM/yyyy");
                            //if (ActivityNamelist != null)
                            //{
                            //    PSB. ActivityNamelist = ActivityNamelist;
                            //}
                            if (Narration != null)
                            {
                                PSB.Narration = Narration;
                            }

                            if (ActivityDate != null)
                            {
                                PSB.ProcessPromoDate = Convert.ToDateTime(ActivityDate);
                                PSB.ReleaseDate = Convert.ToDateTime(ActivityDate);
                            }
                            if (OldBasic != null)
                            {
                                PSB.OldBasic = Convert.ToDouble(OldBasic);
                            }
                            if (NewBasic != null)
                            {
                                PSB.NewBasic = Convert.ToDouble(NewBasic);
                            }
                            if (Fittment != null)
                            {
                                PSB.Fittment = Convert.ToDouble(Fittment);
                            }
                            //if (OldDesig != null)
                            //{
                            //    PSB.OldFuncStruct = Convert.ToDouble(OldFuncStruct);
                            //}
                            //if (NewDesig != null)
                            //{
                            //    PSB.NewFuncStruct = Convert.ToDouble(NewFuncStruct);
                            //}
                            //if (OldGrade != null)
                            //{
                            //    PSB.OldPayStruct = Convert.ToDouble(OldPayStruct);
                            //}
                            //if (NewGrade != null)
                            //{
                            //    PSB.NewPayStruct = Convert.ToDouble(NewPayStruct);
                            //}

                            int CompId = 0;
                            if (SessionManager.UserName != null)
                            {
                                CompId = Convert.ToInt32(Session["CompId"]);
                            }

                            //var check = db.CPIEntryT.Where(e => e.PayMonth == date).ToList();

                            //if (check.Count() == 0)
                            //{
                            //    Msg = "Kindly run CPI first and then try again";
                            //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //}

                            string EmpCode = null;
                            List<String> ActivityNameCodeList = new List<String>();
                            List<int> ActivityHead = new List<int>();
                            if (ActivityNamelist != null && ActivityNamelist != "")
                            {
                                ActivityNameCodeList = Utility.StringIdsToListString(ActivityNamelist);
                            }
                            foreach (var item in ActivityNameCodeList)
                            {
                                var aa = db.PromoActivity.Where(e => e.Name == item).Select(e => e.Id).FirstOrDefault();
                                if (aa != 0)
                                {
                                    ActivityHead.Add(aa);
                                }
                            }

                            if (ids != null)
                            {
                                foreach (var i in ids)
                                {
                                    Employee OEmployee = null;
                                    EmployeePayroll OEmployeePayroll = null;
                                    OEmployeePayroll = db.EmployeePayroll
                                                   .Include(q => q.Employee)
                                                   .Include(q => q.Employee.ServiceBookDates)
                                                   .Include(e => e.PromotionServiceBook)
                                                   .Include(e => e.PromotionServiceBook.Select(r => r.PromotionActivity))
                                                   .Where(e => e.Employee.Id == i).AsNoTracking().AsParallel().SingleOrDefault();

                                    if (OEmployeePayroll != null)
                                    {
                                        var OEmployeePayrollS = OEmployeePayroll.Employee.ServiceBookDates.ServiceLastDate;
                                        var OEmployeePayrollR = OEmployeePayroll.Employee.ServiceBookDates.ResignationDate;
                                        if (OEmployeePayrollS == null && OEmployeePayrollR == null)
                                        {
                                            OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct)
                                                .Where(r => r.Id == i).AsNoTracking().AsParallel().SingleOrDefault();

                                            foreach (int SH in ActivityHead)
                                            {
                                                var val = db.PromoActivity.Find(SH);
                                                PSB.PromotionActivity = val;

                                                var EmpSalT = OEmployeePayroll.PromotionServiceBook != null ? OEmployeePayroll.PromotionServiceBook.Where(e => e.PromotionActivity.Id == PSB.PromotionActivity.Id && e.ProcessPromoDate.Value.ToShortDateString() == ActivityDate.ToString()) : null;
                                                if (EmpSalT != null && EmpSalT.Count() > 0)
                                                {
                                                    if (EmpCode == null || EmpCode == "")
                                                    {
                                                        EmpCode = OEmployee.EmpCode;
                                                    }
                                                    else
                                                    {
                                                        EmpCode = EmpCode + ", " + OEmployee.EmpCode;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            if (EmpCode != null)
                            {
                                return Json(new { data = rowno, success = false, responseText = "PromotionServiceBook already exists for employee " + EmpCode + "." }, JsonRequestBehavior.AllowGet);
                            }

                            string Corporate = form["Corporate"] == "null" ? null : form["Corporate"];
                            string Region = form["Region"] == "null" ? null : form["Region"];
                            string Company = form["Company"] == "null" ? null : form["Company"];
                            string Division = form["Division"] == "null" ? null : form["Division"];
                            string Location = form["Location"] == "null" ? null : form["Location"];
                            string Department = form["Department"] == "null" ? null : form["Department"];
                            string Group = form["Group"] == "null" ? null : form["Group"];
                            string Unit = form["Unit"] == "null" ? null : form["Unit"];



                            var Get_Geo_Id = db.GeoStruct.Include(e => e.Company)
                                .Include(e => e.Corporate).Include(e => e.Department).Include(e => e.Department.DepartmentObj)
                                .Include(e => e.Division).Include(e => e.Group).Include(e => e.Location).Include(e => e.Location.LocationObj).Include(e => e.Region).Include(e => e.Unit)
                                .ToList();
                            if (Corporate != null && Corporate != "#")
                            {
                                var id = Corporate.Trim();
                                Get_Geo_Id = Get_Geo_Id.Where(e => e.Corporate != null && e.Corporate.Code == id).ToList();
                            }
                            if (Region != null && Region != "#")
                            {
                                var id = Region.Trim();
                                Get_Geo_Id = Get_Geo_Id.Where(e => e.Region != null && e.Region.Code == id).ToList();
                            }
                            if (Company != null && Company != "#")
                            {
                                var id = Company.Trim();
                                Get_Geo_Id = Get_Geo_Id.Where(e => e.Company != null && e.Company.Code == id).ToList();
                            }
                            if (Division != null && Division != "#")
                            {
                                var id = Division.Trim();
                                Get_Geo_Id = Get_Geo_Id.Where(e => e.Division != null && e.Division.Code == id).ToList();
                            }
                            if (Location != null && Location != "#")
                            {
                                var id = Location.Trim();
                                Get_Geo_Id = Get_Geo_Id.Where(e => e.Location != null && e.Location.LocationObj != null && e.Location.LocationObj.LocCode == id).ToList();
                            }
                            if (Department != null && Department != "#")
                            {
                                var id = Department.Trim();
                                Get_Geo_Id = Get_Geo_Id.Where(e => e.Department != null && e.Department.DepartmentObj != null && e.Department.DepartmentObj.DeptCode == id).ToList();
                            }
                            if (Group != null && Group != "#")
                            {
                                var id = Group.Trim();
                                Get_Geo_Id = Get_Geo_Id.Where(e => e.Group != null && e.Group.Code == id).ToList();
                            }
                            if (Unit != null && Unit != "#")
                            {
                                var id = Unit.Trim();
                                Get_Geo_Id = Get_Geo_Id.Where(e => e.Unit != null && e.Unit.Code == id).ToList();
                            }

                            if (Get_Geo_Id != null && Get_Geo_Id.Count == 1)
                            {
                                PSB.GeoStruct = Get_Geo_Id[0];
                            }

                            if (PSB.GeoStruct == null)
                            {
                                return Json(new { data = rowno, success = false, responseText = "GeoStruct not found." }, JsonRequestBehavior.AllowGet);
                            }
                            //paystruct
                            var Get_Pay_Id = db.PayStruct.Include(e => e.Company).Include(e => e.Grade).Include(e => e.JobStatus).Include(e => e.JobStatus.EmpStatus).Include(e => e.JobStatus.EmpActingStatus).Include(e => e.Level).ToList();
                            string Grade = form["OldGrade"] == "null" ? null : form["OldGrade"];
                            string Level = form["OldLevel"] == "null" ? null : form["OldLevel"];
                            string OldEmpStatus = form["OldEmpStatus"] == "null" ? null : form["OldEmpStatus"];
                            string OldEmpActingStatus = form["OldEmpActingStatus"] == "null" ? null : form["OldEmpActingStatus"];
                            if (Grade != null && Grade != "#")
                            {
                                var id = Grade.Trim();
                                Get_Pay_Id = Get_Pay_Id.Where(e => e.Grade != null && e.Grade.Code == id).ToList();
                            }
                            if (Level != null && Level != "#")
                            {
                                var id = Level.Trim();
                                Get_Pay_Id = Get_Pay_Id.Where(e => e.Level != null && e.Level.Code == id).ToList();
                            }

                            if (OldEmpStatus != null && OldEmpStatus != "#")
                            {
                                var id = OldEmpStatus.Trim().ToUpper();
                                Get_Pay_Id = Get_Pay_Id.Where(e => e.JobStatus != null && e.JobStatus.EmpStatus != null && e.JobStatus.EmpStatus.LookupVal.ToUpper() == id).ToList();
                            }
                            if (OldEmpActingStatus != null && OldEmpActingStatus != "#")
                            {
                                var id = OldEmpActingStatus.Trim().ToUpper();
                                Get_Pay_Id = Get_Pay_Id.Where(e => e.JobStatus != null && e.JobStatus.EmpActingStatus != null && e.JobStatus.EmpActingStatus.LookupVal.ToUpper() == id).ToList();
                            }
                            if (Get_Pay_Id != null && Get_Pay_Id.Count == 1)
                            {
                                PSB.OldPayStruct = Get_Pay_Id[0];
                            }

                            if (PSB.OldPayStruct == null)
                            {
                                return Json(new { data = rowno, success = false, responseText = "Old PayStruct not found." }, JsonRequestBehavior.AllowGet);
                            }


                            //Funstrct
                            string OldJob = form["OldJob"] == "null" ? null : form["OldJob"];
                            string OldPosition = form["OldPosition"] == "null" ? null : form["OldPosition"];
                            var Get_Fun_Id = db.FuncStruct.Include(e => e.Company).Include(e => e.Job).Include(e => e.JobPosition).ToList();
                            if (OldJob != null && OldJob != "#")
                            {
                                var id = OldJob.Trim();
                                Get_Fun_Id = Get_Fun_Id.Where(e => e.Job != null && e.Job.Code == id).ToList();
                            }
                            if (OldPosition != null && OldPosition != "#")
                            {
                                var id = OldPosition.Trim();
                                Get_Fun_Id = Get_Fun_Id.Where(e => e.JobPosition != null && e.JobPosition.JobPositionCode == id).ToList();
                            }
                            if (OldPosition == null)
                            {

                                Get_Fun_Id = Get_Fun_Id.Where(e => e.JobPosition == null).ToList();
                            }

                            if (Get_Fun_Id != null && Get_Fun_Id.Count == 1)
                            {
                                PSB.OldFuncStruct = Get_Fun_Id[0];
                            }
                            if (PSB.OldFuncStruct == null)
                            {
                                return Json(new { data = rowno, success = false, responseText = "Old FuncStruct not found." }, JsonRequestBehavior.AllowGet);
                            }
                            var Get_JobStatus_Id = db.JobStatus.Include(e => e.EmpStatus).Include(e => e.EmpActingStatus).ToList();
                            if (OldEmpStatus != null && OldEmpStatus != "#")
                            {
                                var id = OldEmpStatus.Trim().ToUpper();
                                Get_JobStatus_Id = Get_JobStatus_Id.Where(e => e.EmpStatus != null && e.EmpStatus.LookupVal.ToUpper() == id).ToList();
                            }
                            if (OldEmpActingStatus != null && OldEmpActingStatus != "#")
                            {
                                var id = OldEmpActingStatus.Trim().ToUpper();
                                Get_JobStatus_Id = Get_JobStatus_Id.Where(e => e.EmpActingStatus != null && e.EmpActingStatus.LookupVal.ToUpper() == id).ToList();
                            }


                            if (Get_JobStatus_Id != null && Get_JobStatus_Id.Count == 1)
                            {
                                PSB.OldJobStatus = Get_JobStatus_Id[0];
                            }

                            if (PSB.OldJobStatus == null)
                            {
                                return Json(new { data = rowno, success = false, responseText = "Old JobStatus not found." }, JsonRequestBehavior.AllowGet);
                            }
                            //kerlabank
                            string comp = "";
                            string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\App_Data\\Menu_Json";
                            bool exists = System.IO.Directory.Exists(requiredPath);
                            string localPath;
                            if (!exists)
                            {
                                localPath = new Uri(requiredPath).LocalPath;
                                System.IO.Directory.CreateDirectory(localPath);
                            }
                            string path = requiredPath + @"\SalRule" + ".txt";
                            localPath = new Uri(path).LocalPath;

                            if (System.IO.File.Exists(localPath))
                            {

                                using (var streamReader = new StreamReader(localPath))
                                {
                                    string line;

                                    while ((line = streamReader.ReadLine()) != null)
                                    {
                                        comp = line.Split('_')[0];
                                        if (comp != "KERALA")
                                        {
                                            if (PSB.OldBasic == PSB.NewBasic)
                                            {
                                                return Json(new { data = rowno, success = false, responseText = "OldBasic and NewBasic should not be same." }, JsonRequestBehavior.AllowGet);
                                            }
                                        }
                                        else if (comp == "")
                                        {
                                            if (PSB.OldBasic == PSB.NewBasic)
                                            {
                                                return Json(new { data = rowno, success = false, responseText = "OldBasic and NewBasic should not  be same." }, JsonRequestBehavior.AllowGet);
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (PSB.OldBasic == PSB.NewBasic)
                                {
                                    return Json(new { data = rowno, success = false, responseText = "OldBasic and NewBasic should not  be same." }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            var Get_Pay_IdNew = db.PayStruct.Include(e => e.Company).Include(e => e.Grade).Include(e => e.JobStatus).Include(e => e.Level).ToList();
                            string NewGrade = form["NewGrade"] == "null" ? null : form["NewGrade"];
                            string NewLevel = form["NewLevel"] == "null" ? null : form["NewLevel"];
                            string NewEmpStatus = form["NewEmpStatus"] == "null" ? null : form["NewEmpStatus"];
                            string NewEmpActingStatus = form["NewEmpActingStatus"] == "null" ? null : form["NewEmpActingStatus"];
                            if (NewGrade != null && NewGrade != "#")
                            {
                                var id = NewGrade.Trim();
                                Get_Pay_IdNew = Get_Pay_IdNew.Where(e => e.Grade != null && e.Grade.Code == id).ToList();
                            }
                            if (NewLevel != null && NewLevel != "#")
                            {
                                var id = NewLevel.Trim();
                                Get_Pay_IdNew = Get_Pay_IdNew.Where(e => e.Level != null && e.Level.Code == id).ToList();
                            }
                            if (NewEmpStatus != null && NewEmpStatus != "#")
                            {
                                var id = NewEmpStatus.Trim().ToUpper();
                                Get_Pay_IdNew = Get_Pay_IdNew.Where(e => e.JobStatus != null && e.JobStatus.EmpStatus != null && e.JobStatus.EmpStatus.LookupVal.ToUpper() == id).ToList();
                            }
                            if (NewEmpActingStatus != null && NewEmpActingStatus != "#")
                            {
                                var id = NewEmpActingStatus.Trim().ToUpper();
                                Get_Pay_IdNew = Get_Pay_IdNew.Where(e => e.JobStatus != null && e.JobStatus.EmpActingStatus != null && e.JobStatus.EmpActingStatus.LookupVal.ToUpper() == id).ToList();
                            }
                            if (Get_Pay_IdNew != null && Get_Pay_IdNew.Count == 1)
                            {
                                PSB.NewPayStruct = Get_Pay_IdNew[0];
                            }

                            if (PSB.NewPayStruct == null)
                            {
                                return Json(new { data = rowno, success = false, responseText = "New PayStruct not found." }, JsonRequestBehavior.AllowGet);
                            }

                            var Get_JobStatus_IdNew = db.JobStatus.Include(e => e.EmpStatus).Include(e => e.EmpActingStatus).ToList();
                            if (NewEmpStatus != null && NewEmpStatus != "#")
                            {
                                var id = NewEmpStatus.Trim().ToUpper();
                                Get_JobStatus_IdNew = Get_JobStatus_IdNew.Where(e => e.EmpStatus != null && e.EmpStatus.LookupVal.ToUpper() == id).ToList();
                            }
                            if (NewEmpActingStatus != null && NewEmpActingStatus != "#")
                            {
                                var id = NewEmpActingStatus.Trim().ToUpper();
                                Get_JobStatus_IdNew = Get_JobStatus_IdNew.Where(e => e.EmpActingStatus != null && e.EmpActingStatus.LookupVal.ToUpper() == id).ToList();
                            }


                            if (Get_JobStatus_IdNew != null && Get_JobStatus_IdNew.Count == 1)
                            {
                                PSB.NewJobStatus = Get_JobStatus_IdNew[0];
                            }


                            if (PSB.NewJobStatus == null)
                            {
                                return Json(new { data = rowno, success = false, responseText = "New JobStatus not found." }, JsonRequestBehavior.AllowGet);
                            }

                            string NewJob = form["NewJob"] == "null" ? null : form["NewJob"];
                            string NewPosition = form["NewPosition"] == "null" ? null : form["NewPosition"];
                            var Get_Fun_IdNew = db.FuncStruct.Include(e => e.Company).Include(e => e.Job).Include(e => e.JobPosition).ToList();
                            if (NewJob != null && NewJob != "#")
                            {
                                var id = NewJob.Trim();
                                Get_Fun_IdNew = Get_Fun_IdNew.Where(e => e.Job != null && e.Job.Code == id).ToList();
                            }
                            if (NewPosition != null && NewPosition != "#")
                            {
                                var id = NewPosition.Trim();
                                Get_Fun_IdNew = Get_Fun_IdNew.Where(e => e.JobPosition != null && e.JobPosition.JobPositionCode == id).ToList();
                            }

                            if (NewPosition == null)
                            {

                                Get_Fun_IdNew = Get_Fun_IdNew.Where(e => e.JobPosition == null).ToList();
                            }

                            if (Get_Fun_IdNew != null && Get_Fun_IdNew.Count == 1)
                            {
                                PSB.NewFuncStruct = Get_Fun_IdNew[0];
                            }

                            if (PSB.NewFuncStruct == null)
                            {
                                return Json(new { data = rowno, success = false, responseText = "New FuncStruct not found." }, JsonRequestBehavior.AllowGet);
                            }

                            if (!string.IsNullOrEmpty(sEmpCode))
                            {
                                foreach (var i in ids)
                                {
                                    Employee OEmployee = null;
                                    EmployeePayroll OEmployeePayroll = null;

                                    //int PayScaleAgrId = int.Parse(NewPayScaleAgreement);
                                    //var OPayScalAgreement = db.PayScaleAgreement.Where(e => e.Id == PayScaleAgrId).SingleOrDefault();

                                    OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct).Include(e => e.PayStruct.JobStatus)
                                                                                .Include(e => e.EmpOffInfo).Include(e => e.EmpOffInfo.PayScale)
                                                                                .Where(r => r.Id == i).SingleOrDefault();

                                    OEmployeePayroll = db.EmployeePayroll.Include(e => e.PromotionServiceBook).Where(e => e.Employee.Id == i).SingleOrDefault();

                                    if (OEmployeePayroll.PromotionServiceBook.Any(d => d.ProcessPromoDate.Value.ToShortDateString() == ActivityDate.ToString()))
                                    {
                                        {
                                            Msg = "Already PromotionServiceBook for Date= " + ActivityDate;
                                            //return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                            return Json(new { data = rowno, success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        }
                                    }



                                    if (OEmployee.EmpOffInfo.PayScale != null)
                                        PSB.OldPayScale = db.PayScale.Find(OEmployee.EmpOffInfo.PayScale.Id);

                                    if (OEmployee.EmpOffInfo.PayScale != null)
                                        PSB.NewPayScale = db.PayScale.Find(OEmployee.EmpOffInfo.PayScale.Id);


                                    int PromoActivityId = 0;
                                    string LookupVal = "";
                                    if (ActivityNamelist != null && ActivityNamelist != "")
                                    {
                                        PromoActivityId = db.PromoActivity.Where(e => e.Name == ActivityNamelist).Select(e => e.Id).FirstOrDefault();
                                        LookupVal = db.LookupValue.Where(e => e.Id == PromoActivityId).Select(e => e.LookupVal.ToUpper()).SingleOrDefault();
                                    }

                                    if (PSB.OldPayStruct == PSB.NewPayStruct)
                                    {
                                        Msg = "Can Not Promot To Same Grade.";
                                        //  return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        return Json(new { data = rowno, success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    }

                                    if (ActivityHead.Count == 0)
                                    {
                                        return Json(new { data = rowno, success = false, responseText = "Please enter valid ActivityName." }, JsonRequestBehavior.AllowGet);
                                    }

                                    foreach (int SH in ActivityHead)
                                    {
                                        var val = db.PromoActivity.Find(SH);
                                        PSB.PromotionActivity = val;
                                        PSB.Narration = "Promotion Activity";
                                        PSB.ReleaseFlag = true;

                                        PSB.NewPayScaleAgreement = db.PayScaleAgreement.SingleOrDefault();
                                        PSB.OldPayScaleAgreement = db.PayScaleAgreement.SingleOrDefault();

                                        PSB.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                                        PromotionServiceBook ObjPSB = new PromotionServiceBook();
                                        {
                                            ObjPSB.PromotionActivity = PSB.PromotionActivity;
                                            ObjPSB.ProcessPromoDate = PSB.ProcessPromoDate;
                                            ObjPSB.ReleaseDate = PSB.ReleaseDate;
                                            ObjPSB.OldBasic = PSB.OldBasic;
                                            ObjPSB.NewBasic = PSB.NewBasic;
                                            ObjPSB.Narration = PSB.Narration;
                                            ObjPSB.ReleaseFlag = PSB.ReleaseFlag;
                                            ObjPSB.GeoStruct = PSB.GeoStruct;
                                            ObjPSB.NewFuncStruct = PSB.NewFuncStruct;
                                            ObjPSB.OldFuncStruct = PSB.OldFuncStruct;
                                            ObjPSB.NewPayStruct = PSB.NewPayStruct;
                                            ObjPSB.OldPayStruct = PSB.OldPayStruct;
                                            ObjPSB.EmployeeCTC = null;
                                            ObjPSB.Fittment = PSB.Fittment;
                                            ObjPSB.IncrNewBasic = 0.0;
                                            ObjPSB.IncrOldBasic = 0.0;
                                            ObjPSB.NewPayScale = PSB.NewPayScale;
                                            ObjPSB.NewPayScaleAgreement = PSB.NewPayScaleAgreement;
                                            ObjPSB.OldJobStatus = PSB.OldJobStatus;
                                            ObjPSB.NewJobStatus = PSB.NewJobStatus;
                                            ObjPSB.OldPayScale = PSB.OldPayScale;
                                            ObjPSB.OldPayScaleAgreement = PSB.OldPayScaleAgreement;
                                            ObjPSB.StagnancyCount = 0;
                                            ObjPSB.StagnancyAppl = false;
                                            ObjPSB.Narration = PSB.Narration;
                                            ObjPSB.DBTrack = PSB.DBTrack;
                                        }

                                        errorList = ValidateObj(ObjPSB);
                                        if (errorList.Count > 0)
                                        {
                                            return Json(new { success = false, data = rowno, responseText = string.Join(",", errorList) }, JsonRequestBehavior.AllowGet);
                                        }

                                        using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                             new System.TimeSpan(0, 30, 0)))
                                        {
                                            PSB.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                            if (true)
                                            {
                                                //irregular

                                            }
                                            else
                                            {

                                            }

                                            db.PromotionServiceBook.Add(PSB);
                                            db.SaveChanges();


                                            List<PromotionServiceBook> PromoBkList = new List<PromotionServiceBook>();
                                            PromoBkList.AddRange(OEmployeePayroll.PromotionServiceBook);
                                            PromoBkList.Add(PSB);
                                            OEmployeePayroll.PromotionServiceBook = PromoBkList;
                                            db.EmployeePayroll.Attach(OEmployeePayroll);
                                            db.Entry(OEmployeePayroll).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();
                                            db.Entry(OEmployeePayroll).State = System.Data.Entity.EntityState.Detached;

                                            ts.Complete();
                                            //// return Json(new Object[] { "", "", "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
                                            ////    List<string> Msgs = new List<string>();
                                        }
                                    }
                                }
                                return Json(new { data = rowno, success = true, responseText = "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                return Json(new { data = rowno, success = false, responseText = "Select Employee" }, JsonRequestBehavior.AllowGet);
                            }
                        }

                    case "TRANSFERSERVICEBOOK":
                        using (DataBaseContext db = new DataBaseContext())
                        {
                            TransferServiceBook TSB = new TransferServiceBook();

                            String ActivityNamelist = form["TransActivity"] != null ? form["TransActivity"] : null;
                            ActivityNamelist = ActivityNamelist.Replace("#", " ");
                            String ActivityDate = form["ProcessTransDate"] != null ? form["ProcessTransDate"] : null;
                            //String OldFuncStruct = form["OldFuncStruct"] != null ? form["OldFuncStruct"] : null;
                            //String NewFuncStruct = form["NewFuncStruct"] != null ? form["NewFuncStruct"] : null;
                            //String OldPayStruct = form["OldPayStruct"] != null ? form["OldPayStruct"] : null;
                            //String NewPayStruct = form["NewPayStruct"] != null ? form["NewPayStruct"] : null;
                            //String OldGeoStruct = form["OldGeoStruct"] != null ? form["OldGeoStruct"] : null;
                            //String NewGeoStruct = form["NewGeoStruct"] != null ? form["NewGeoStruct"] : null;
                            //String OldDept = form["OldDept"] != null ? form["OldDept"] : null;
                            //String NewDept = form["NewDept"] != null ? form["NewDept"] : null;
                            //String OldJobstatus = form["OldJobstatus"] != null ? form["OldJobstatus"] : null;
                            //String NewJobstatus = form["NewJobstatus"] != null ? form["NewJobstatus"] : null;


                            var date = Convert.ToDateTime(ActivityDate).ToString("MM/yyyy");
                            //if (ActivityNamelist != null)
                            //{
                            //    PSB. ActivityNamelist = ActivityNamelist;
                            //}
                            if (ActivityDate != null)
                            {
                                TSB.ProcessTransDate = Convert.ToDateTime(ActivityDate);
                                TSB.ReleaseDate = Convert.ToDateTime(ActivityDate);
                            }

                            int CompId = 0;
                            if (SessionManager.UserName != null)
                            {
                                CompId = Convert.ToInt32(Session["CompId"]);
                            }

                            //var check = db.CPIEntryT.Where(e => e.PayMonth == date).ToList();

                            //if (check.Count() == 0)
                            //{
                            //    Msg = "Kindly run CPI first and then try again";
                            //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //}

                            string EmpCode = null;
                            List<String> ActivityNameCodeList = new List<String>();
                            List<int> ActivityHead = new List<int>();
                            if (ActivityNamelist != null && ActivityNamelist != "")
                            {
                                ActivityNameCodeList = Utility.StringIdsToListString(ActivityNamelist);
                            }
                            foreach (var item in ActivityNameCodeList)
                            {
                                var aa = db.TransActivity.Where(e => e.Name == item).Select(e => e.Id).FirstOrDefault();
                                if (aa != 0)
                                {
                                    ActivityHead.Add(aa);
                                }
                            }

                            if (ids != null)
                            {
                                foreach (var i in ids)
                                {
                                    Employee OEmployee = null;
                                    EmployeePayroll OEmployeePayroll = null;
                                    OEmployeePayroll = db.EmployeePayroll
                                                   .Include(q => q.Employee)
                                                   .Include(q => q.Employee.ServiceBookDates)
                                                   .Include(e => e.TransferServiceBook)
                                                   .Include(e => e.TransferServiceBook.Select(r => r.TransActivity))
                                                   .Where(e => e.Employee.Id == i).AsNoTracking().AsParallel().SingleOrDefault();

                                    if (OEmployeePayroll != null)
                                    {
                                        var OEmployeePayrollS = OEmployeePayroll.Employee.ServiceBookDates.ServiceLastDate;
                                        var OEmployeePayrollR = OEmployeePayroll.Employee.ServiceBookDates.ResignationDate;
                                        if (OEmployeePayrollS == null && OEmployeePayrollR == null)
                                        {
                                            OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct)
                                                .Where(r => r.Id == i).AsNoTracking().AsParallel().SingleOrDefault();

                                            foreach (int SH in ActivityHead)
                                            {
                                                var val = db.TransActivity.Find(SH);
                                                TSB.TransActivity = val;

                                                var EmpSalT = OEmployeePayroll.TransferServiceBook != null ? OEmployeePayroll.TransferServiceBook.Where(e => e.TransActivity.Id == TSB.TransActivity.Id && e.ProcessTransDate.Value.ToShortDateString() == ActivityDate.ToString()) : null;
                                                if (EmpSalT != null && EmpSalT.Count() > 0)
                                                {
                                                    if (EmpCode == null || EmpCode == "")
                                                    {
                                                        EmpCode = OEmployee.EmpCode;
                                                    }
                                                    else
                                                    {
                                                        EmpCode = EmpCode + ", " + OEmployee.EmpCode;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            if (EmpCode != null)
                            {
                                return Json(new { data = rowno, success = false, responseText = "TransferServiceBook already exists for employee " + EmpCode + "." }, JsonRequestBehavior.AllowGet);
                            }

                            string OldCorporate = form["OldCorporate"] == "null" ? null : form["OldCorporate"];
                            string OldRegion = form["OldRegion"] == "null" ? null : form["OldRegion"];
                            string OldCompany = form["OldCompany"] == "null" ? null : form["OldCompany"];
                            string OldDivision = form["OldDivision"] == "null" ? null : form["OldDivision"];
                            string OldLocation = form["OldLocation"] == "null" ? null : form["OldLocation"];
                            string OldDepartment = form["OldDepartment"] == "null" ? null : form["OldDepartment"];
                            string OldGroup = form["OldGroup"] == "null" ? null : form["OldGroup"];
                            string OldUnit = form["OldUnit"] == "null" ? null : form["OldUnit"];



                            var Get_Geo_Id = db.GeoStruct.Include(e => e.Company)
                                .Include(e => e.Corporate).Include(e => e.Department).Include(e => e.Department.DepartmentObj)
                                .Include(e => e.Division).Include(e => e.Group).Include(e => e.Location).Include(e => e.Location.LocationObj).Include(e => e.Region).Include(e => e.Unit)
                                .ToList();
                            if (OldCorporate != null && OldCorporate != "#")
                            {
                                var id = OldCorporate.Trim();
                                Get_Geo_Id = Get_Geo_Id.Where(e => e.Corporate != null && e.Corporate.Code == id).ToList();
                            }
                            if (OldRegion != null && OldRegion != "#")
                            {
                                var id = OldRegion.Trim();
                                Get_Geo_Id = Get_Geo_Id.Where(e => e.Region != null && e.Region.Code == id).ToList();
                            }
                            if (OldCompany != null && OldCompany != "#")
                            {
                                var id = OldCompany.Trim();
                                Get_Geo_Id = Get_Geo_Id.Where(e => e.Company != null && e.Company.Code == id).ToList();
                            }
                            if (OldDivision != null && OldDivision != "#")
                            {
                                var id = OldDivision.Trim();
                                Get_Geo_Id = Get_Geo_Id.Where(e => e.Division != null && e.Division.Code == id).ToList();
                            }
                            if (OldLocation != null && OldLocation != "#")
                            {
                                var id = OldLocation.Trim();
                                Get_Geo_Id = Get_Geo_Id.Where(e => e.Location != null && e.Location.LocationObj != null && e.Location.LocationObj.LocCode == id).ToList();
                            }
                            if (OldDepartment != null && OldDepartment != "#")
                            {
                                var id = OldDepartment.Trim();
                                Get_Geo_Id = Get_Geo_Id.Where(e => e.Department != null && e.Department.DepartmentObj != null && e.Department.DepartmentObj.DeptCode == id).ToList();
                            }
                            if (OldGroup != null && OldGroup != "#")
                            {
                                var id = OldGroup.Trim();
                                Get_Geo_Id = Get_Geo_Id.Where(e => e.Group != null && e.Group.Code == id).ToList();
                            }
                            if (OldUnit != null && OldUnit != "#")
                            {
                                var id = OldUnit.Trim();
                                Get_Geo_Id = Get_Geo_Id.Where(e => e.Unit != null && e.Unit.Code == id).ToList();
                            }

                            if (Get_Geo_Id != null && Get_Geo_Id.Count == 1)
                            {
                                TSB.OldGeoStruct = Get_Geo_Id[0];
                            }

                            if (TSB.OldGeoStruct == null)
                            {
                                return Json(new { data = rowno, success = false, responseText = "Old GeoStruct not found." }, JsonRequestBehavior.AllowGet);
                            }
                            //paystruct
                            var Get_Pay_Id = db.PayStruct.Include(e => e.Company).Include(e => e.Grade).Include(e => e.JobStatus).Include(e => e.JobStatus.EmpStatus).Include(e => e.JobStatus.EmpActingStatus).Include(e => e.Level).ToList();
                            string Grade = form["OldGrade"] == "null" ? null : form["OldGrade"];
                            string Level = form["OldLevel"] == "null" ? null : form["OldLevel"];
                            string OldEmpStatus = form["OldEmpStatus"] == "null" ? null : form["OldEmpStatus"];
                            string OldEmpActingStatus = form["OldEmpActingStatus"] == "null" ? null : form["OldEmpActingStatus"];
                            if (Grade != null && Grade != "#")
                            {
                                var id = Grade.Trim();
                                Get_Pay_Id = Get_Pay_Id.Where(e => e.Grade != null && e.Grade.Code == id).ToList();
                            }
                            if (Level != null && Level != "#")
                            {
                                var id = Level.Trim();
                                Get_Pay_Id = Get_Pay_Id.Where(e => e.Level != null && e.Level.Code == id).ToList();
                            }

                            if (OldEmpStatus != null && OldEmpStatus != "#")
                            {
                                var id = OldEmpStatus.Trim().ToUpper();
                                Get_Pay_Id = Get_Pay_Id.Where(e => e.JobStatus != null && e.JobStatus.EmpStatus != null && e.JobStatus.EmpStatus.LookupVal.ToUpper() == id).ToList();
                            }
                            if (OldEmpActingStatus != null && OldEmpActingStatus != "#")
                            {
                                var id = OldEmpActingStatus.Trim().ToUpper();
                                Get_Pay_Id = Get_Pay_Id.Where(e => e.JobStatus != null && e.JobStatus.EmpActingStatus != null && e.JobStatus.EmpActingStatus.LookupVal.ToUpper() == id).ToList();
                            }
                            if (Get_Pay_Id != null && Get_Pay_Id.Count == 1)
                            {
                                TSB.OldPayStruct = Get_Pay_Id[0];
                            }

                            if (TSB.OldPayStruct == null)
                            {
                                return Json(new { data = rowno, success = false, responseText = "Old PayStruct not found." }, JsonRequestBehavior.AllowGet);
                            }


                            //Funstrct
                            string OldJob = form["OldJob"] == "null" ? null : form["OldJob"];
                            string OldPosition = form["OldPosition"] == "null" ? null : form["OldPosition"];
                            var Get_Fun_Id = db.FuncStruct.Include(e => e.Company).Include(e => e.Job).Include(e => e.JobPosition).ToList();
                            if (OldJob != null && OldJob != "#")
                            {
                                var id = OldJob.Trim();
                                Get_Fun_Id = Get_Fun_Id.Where(e => e.Job != null && e.Job.Code == id).ToList();
                            }
                            if (OldPosition != null && OldPosition != "#")
                            {
                                var id = OldPosition.Trim();
                                Get_Fun_Id = Get_Fun_Id.Where(e => e.JobPosition != null && e.JobPosition.JobPositionCode == id).ToList();
                            }
                            if (Get_Fun_Id != null && Get_Fun_Id.Count == 1)
                            {
                                TSB.OldFuncStruct = Get_Fun_Id[0];
                            }
                            if (TSB.OldFuncStruct == null)
                            {
                                return Json(new { data = rowno, success = false, responseText = "Old FuncStruct not found." }, JsonRequestBehavior.AllowGet);
                            }


                            string NewCorporate = form["NewCorporate"] == "null" ? null : form["NewCorporate"];
                            string NewRegion = form["NewRegion"] == "null" ? null : form["NewRegion"];
                            string NewCompany = form["NewCompany"] == "null" ? null : form["NewCompany"];
                            string NewDivision = form["NewDivision"] == "null" ? null : form["NewDivision"];
                            string NewLocation = form["NewLocation"] == "null" ? null : form["NewLocation"];
                            string NewDepartment = form["NewDepartment"] == "null" ? null : form["NewDepartment"];
                            string NewGroup = form["NewGroup"] == "null" ? null : form["NewGroup"];
                            string NewUnit = form["NewUnit"] == "null" ? null : form["NewUnit"];



                            var Get_Geo_IdNew = db.GeoStruct.Include(e => e.Company)
                                .Include(e => e.Corporate).Include(e => e.Department).Include(e => e.Department.DepartmentObj)
                                .Include(e => e.Division).Include(e => e.Group).Include(e => e.Location).Include(e => e.Location.LocationObj).Include(e => e.Region).Include(e => e.Unit)
                                .ToList();
                            if (NewCorporate != null && NewCorporate != "#")
                            {
                                var id = NewCorporate.Trim();
                                Get_Geo_IdNew = Get_Geo_IdNew.Where(e => e.Corporate != null && e.Corporate.Code == id).ToList();
                            }
                            if (NewRegion != null && NewRegion != "#")
                            {
                                var id = NewRegion.Trim();
                                Get_Geo_IdNew = Get_Geo_IdNew.Where(e => e.Region != null && e.Region.Code == id).ToList();
                            }
                            if (NewCompany != null && NewCompany != "#")
                            {
                                var id = NewCompany.Trim();
                                Get_Geo_IdNew = Get_Geo_IdNew.Where(e => e.Company != null && e.Company.Code == id).ToList();
                            }
                            if (NewDivision != null && NewDivision != "#")
                            {
                                var id = NewDivision.Trim();
                                Get_Geo_IdNew = Get_Geo_IdNew.Where(e => e.Division != null && e.Division.Code == id).ToList();
                            }
                            if (NewLocation != null && NewLocation != "#")
                            {
                                var id = NewLocation.Trim();
                                Get_Geo_IdNew = Get_Geo_IdNew.Where(e => e.Location != null && e.Location.LocationObj != null && e.Location.LocationObj.LocCode == id).ToList();
                            }
                            if (NewDepartment != null && NewDepartment != "#")
                            {
                                var id = NewDepartment.Trim();
                                Get_Geo_IdNew = Get_Geo_IdNew.Where(e => e.Department != null && e.Department.DepartmentObj != null && e.Department.DepartmentObj.DeptCode == id).ToList();
                            }
                            if (NewGroup != null && NewGroup != "#")
                            {
                                var id = NewGroup.Trim();
                                Get_Geo_IdNew = Get_Geo_IdNew.Where(e => e.Group != null && e.Group.Code == id).ToList();
                            }
                            if (NewUnit != null && NewUnit != "#")
                            {
                                var id = NewUnit.Trim();
                                Get_Geo_IdNew = Get_Geo_IdNew.Where(e => e.Unit != null && e.Unit.Code == id).ToList();
                            }

                            if (Get_Geo_Id != null && Get_Geo_IdNew.Count == 1)
                            {
                                TSB.NewGeoStruct = Get_Geo_IdNew[0];
                            }




                            var Get_Pay_IdNew = db.PayStruct.Include(e => e.Company).Include(e => e.Grade).Include(e => e.JobStatus).Include(e => e.Level).ToList();
                            string NewGrade = form["NewGrade"] == "null" ? null : form["NewGrade"];
                            string NewLevel = form["NewLevel"] == "null" ? null : form["NewLevel"];
                            string NewEmpStatus = form["NewEmpStatus"] == "null" ? null : form["NewEmpStatus"];
                            string NewEmpActingStatus = form["NewEmpActingStatus"] == "null" ? null : form["NewEmpActingStatus"];
                            if (NewGrade != null && NewGrade != "#")
                            {
                                var id = NewGrade.Trim();
                                Get_Pay_IdNew = Get_Pay_IdNew.Where(e => e.Grade != null && e.Grade.Code == id).ToList();
                            }
                            if (NewLevel != null && NewLevel != "#")
                            {
                                var id = NewLevel.Trim();
                                Get_Pay_IdNew = Get_Pay_IdNew.Where(e => e.Level != null && e.Level.Code == id).ToList();
                            }
                            if (NewEmpStatus != null && NewEmpStatus != "#")
                            {
                                var id = NewEmpStatus.Trim().ToUpper();
                                Get_Pay_IdNew = Get_Pay_IdNew.Where(e => e.JobStatus != null && e.JobStatus.EmpStatus != null && e.JobStatus.EmpStatus.LookupVal.ToUpper() == id).ToList();
                            }
                            if (NewEmpActingStatus != null && NewEmpActingStatus != "#")
                            {
                                var id = NewEmpActingStatus.Trim().ToUpper();
                                Get_Pay_IdNew = Get_Pay_IdNew.Where(e => e.JobStatus != null && e.JobStatus.EmpActingStatus != null && e.JobStatus.EmpActingStatus.LookupVal.ToUpper() == id).ToList();
                            }
                            if (Get_Pay_IdNew != null && Get_Pay_IdNew.Count == 1)
                            {
                                TSB.NewPayStruct = Get_Pay_IdNew[0];
                            }

                            if (TSB.NewPayStruct == null)
                            {
                                return Json(new { data = rowno, success = false, responseText = "New PayStruct not found." }, JsonRequestBehavior.AllowGet);
                            }



                            string NewJob = form["NewJob"] == "null" ? null : form["NewJob"];
                            string NewPosition = form["NewPosition"] == "null" ? null : form["NewPosition"];
                            var Get_Fun_IdNew = db.FuncStruct.Include(e => e.Company).Include(e => e.Job).Include(e => e.JobPosition).ToList();
                            if (NewJob != null && NewJob != "#")
                            {
                                var id = NewJob.Trim();
                                Get_Fun_IdNew = Get_Fun_IdNew.Where(e => e.Job != null && e.Job.Code == id).ToList();
                            }
                            if (NewPosition != null && NewPosition != "#")
                            {
                                var id = NewPosition.Trim();
                                Get_Fun_IdNew = Get_Fun_IdNew.Where(e => e.JobPosition != null && e.JobPosition.JobPositionCode == id).ToList();
                            }
                            if (Get_Fun_IdNew != null && Get_Fun_IdNew.Count == 1)
                            {
                                TSB.NewFuncStruct = Get_Fun_IdNew[0];
                            }


                            if (!string.IsNullOrEmpty(sEmpCode))
                            {
                                foreach (var i in ids)
                                {
                                    Employee OEmployee = null;
                                    EmployeePayroll OEmployeePayroll = null;

                                    OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct).Include(e => e.PayStruct.JobStatus)
                                                                                .Include(e => e.EmpOffInfo).Include(e => e.EmpOffInfo.PayScale)
                                                                                .Where(r => r.Id == i).SingleOrDefault();

                                    OEmployeePayroll = db.EmployeePayroll.Include(e => e.TransferServiceBook).Where(e => e.Employee.Id == i).SingleOrDefault();

                                    if (OEmployeePayroll.TransferServiceBook.Any(d => d.ProcessTransDate.Value.ToShortDateString() == ActivityDate.ToString()))
                                    {
                                        {
                                            Msg = "Already TransferServiceBook for Date= " + ActivityDate;
                                            //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                            return Json(new { data = rowno, success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        }
                                    }


                                    int TransActivityId = 0;
                                    string LookupVal = "";
                                    if (ActivityNamelist != null && ActivityNamelist != "")
                                    {
                                        TransActivityId = db.TransActivity.Where(e => e.Name == ActivityNamelist).Select(e => e.Id).FirstOrDefault();
                                        LookupVal = db.LookupValue.Where(e => e.Id == TransActivityId).Select(e => e.LookupVal.ToUpper()).SingleOrDefault();
                                        TSB.TransActivity = db.TransActivity.Include(e => e.TranPolicy).Where(e => e.Id == TransActivityId).SingleOrDefault();
                                    }

                                    if (TSB.TransActivity == null)
                                    {
                                        return Json(new { data = rowno, success = false, responseText = "Transfer Policy not found." }, JsonRequestBehavior.AllowGet);
                                    }

                                    if (TSB.TransActivity.TranPolicy.IsFuncStructChange == true)
                                    {
                                        if (TSB.NewFuncStruct == null)
                                        {
                                            return Json(new { data = rowno, success = false, responseText = "New FuncStruct not found." }, JsonRequestBehavior.AllowGet);
                                        }
                                    }


                                    if (TSB.TransActivity.TranPolicy.IsDepartmentChange == true || TSB.TransActivity.TranPolicy.IsDivsionChange == true ||
                                     TSB.TransActivity.TranPolicy.IsGroupChange == true || TSB.TransActivity.TranPolicy.IsLocationChange == true ||
                                     TSB.TransActivity.TranPolicy.IsUnitChange == true)
                                    {
                                        if (TSB.NewGeoStruct == null)
                                        {
                                            return Json(new { data = rowno, success = false, responseText = "New GeoStruct not found." }, JsonRequestBehavior.AllowGet);
                                        }
                                    }






                                    if (TSB.OldGeoStruct == TSB.NewGeoStruct)
                                    {
                                        Msg = "Can Not Tranfer To Same Location and Same Department";
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    }
                                    if (ActivityHead.Count == 0)
                                    {
                                        return Json(new { data = rowno, success = false, responseText = "Please enter valid ActivityName." }, JsonRequestBehavior.AllowGet);
                                    }

                                    foreach (int SH in ActivityHead)
                                    {
                                        var val = db.TransActivity.Find(SH);
                                        TSB.TransActivity = val;
                                        TSB.Narration = "Transfer Activity";
                                        TSB.ReleaseFlag = true;

                                        //TSB.NewPayScaleAgreement = db.PayScaleAgreement.SingleOrDefault();
                                        //TSB.OldPayScaleAgreement = db.PayScaleAgreement.SingleOrDefault();

                                        TSB.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                                        TransferServiceBook ObjTSB = new TransferServiceBook();
                                        {
                                            ObjTSB.EmployeeCTC = null;
                                            ObjTSB.ProcessTransDate = TSB.ProcessTransDate;
                                            ObjTSB.ReleaseDate = TSB.ReleaseDate;
                                            ObjTSB.ReleaseFlag = TSB.ReleaseFlag;
                                            ObjTSB.Narration = TSB.Narration;
                                            ObjTSB.NewFuncStruct = TSB.NewFuncStruct;
                                            ObjTSB.OldFuncStruct = TSB.OldFuncStruct;
                                            ObjTSB.NewGeoStruct = TSB.NewGeoStruct;
                                            ObjTSB.OldGeoStruct = TSB.OldGeoStruct;
                                            ObjTSB.NewPayStruct = TSB.NewPayStruct;
                                            ObjTSB.OldPayStruct = TSB.OldPayStruct;
                                            ObjTSB.TransActivity = TSB.TransActivity;
                                            ObjTSB.DBTrack = TSB.DBTrack;
                                        }

                                        errorList = ValidateObj(ObjTSB);
                                        if (errorList.Count > 0)
                                        {
                                            return Json(new { success = false, data = rowno, responseText = string.Join(",", errorList) }, JsonRequestBehavior.AllowGet);
                                        }

                                        using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                    new System.TimeSpan(0, 30, 0)))
                                        {
                                            TSB.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                                            db.TransferServiceBook.Add(TSB);
                                            db.SaveChanges();


                                            List<TransferServiceBook> TransferBkList = new List<TransferServiceBook>();
                                            TransferBkList.AddRange(OEmployeePayroll.TransferServiceBook);
                                            TransferBkList.Add(TSB);
                                            OEmployeePayroll.TransferServiceBook = TransferBkList;
                                            db.EmployeePayroll.Attach(OEmployeePayroll);
                                            db.Entry(OEmployeePayroll).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();
                                            db.Entry(OEmployeePayroll).State = System.Data.Entity.EntityState.Detached;



                                            ts.Complete();
                                            //// return Json(new Object[] { "", "", "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
                                            ////    List<string> Msgs = new List<string>();
                                        }
                                    }
                                }
                                return Json(new { data = rowno, success = true, responseText = "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                return Json(new { data = rowno, success = false, responseText = "Select Employee" }, JsonRequestBehavior.AllowGet);
                            }
                        }

                    case "OTHERSERVICEBOOK":
                        using (DataBaseContext db = new DataBaseContext())
                        {
                            OtherServiceBook OSB = new OtherServiceBook();

                            String ActivityNamelist = form["OthServiceBookActivity"] != null ? form["OthServiceBookActivity"] : null;
                            ActivityNamelist = ActivityNamelist.Replace("#", " ");
                            String ActivityDate = form["ProcessOthDate"] != null ? form["ProcessOthDate"] : null;
                            String NewBasic = form["NewBasic"] != null ? form["NewBasic"] : null;


                            var date = Convert.ToDateTime(ActivityDate).ToString("MM/yyyy");

                            if (ActivityDate != null)
                            {
                                OSB.ProcessOthDate = Convert.ToDateTime(ActivityDate);
                                OSB.ReleaseDate = Convert.ToDateTime(ActivityDate);
                            }
                            if (NewBasic != null)
                            {
                                OSB.NewBasic = Convert.ToDouble(NewBasic);
                            }

                            int CompId = 0;
                            if (Session["CompId"] != null)
                            {
                                CompId = Convert.ToInt32(Session["CompId"]);
                            }

                            //var check = db.CPIEntryT.Where(e => e.PayMonth == date).ToList();

                            //if (check.Count() == 0)
                            //{
                            //    Msg = "Kindly run CPI first and then try again";
                            //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //}

                            string EmpCode = null;
                            List<String> ActivityNameCodeList = new List<String>();
                            List<int> ActivityHead = new List<int>();
                            if (ActivityNamelist != null && ActivityNamelist != "")
                            {
                                ActivityNameCodeList = Utility.StringIdsToListString(ActivityNamelist);
                            }
                            foreach (var item in ActivityNameCodeList)
                            {
                                var aa = db.OthServiceBookActivity.Where(e => e.Name == item).Select(e => e.Id).FirstOrDefault();
                                if (aa != 0)
                                {
                                    ActivityHead.Add(aa);
                                }
                            }

                            string Corporate = form["Corporate"] == "null" ? null : form["Corporate"];
                            string Region = form["Region"] == "null" ? null : form["Region"];
                            string Company = form["Company"] == "null" ? null : form["Company"];
                            string Division = form["Division"] == "null" ? null : form["Division"];
                            string Location = form["Location"] == "null" ? null : form["Location"];
                            string Department = form["Department"] == "null" ? null : form["Department"];
                            string Group = form["Group"] == "null" ? null : form["Group"];
                            string Unit = form["Unit"] == "null" ? null : form["Unit"];



                            var Get_Geo_Id = db.GeoStruct.Include(e => e.Company)
                                .Include(e => e.Corporate).Include(e => e.Department).Include(e => e.Department.DepartmentObj)
                                .Include(e => e.Division).Include(e => e.Group).Include(e => e.Location).Include(e => e.Location.LocationObj).Include(e => e.Region).Include(e => e.Unit)
                                .ToList();
                            if (Corporate != null && Corporate != "#")
                            {
                                var id = Corporate.Trim();
                                Get_Geo_Id = Get_Geo_Id.Where(e => e.Corporate != null && e.Corporate.Code == id).ToList();
                            }
                            if (Region != null && Region != "#")
                            {
                                var id = Region.Trim();
                                Get_Geo_Id = Get_Geo_Id.Where(e => e.Region != null && e.Region.Code == id).ToList();
                            }
                            if (Company != null && Company != "#")
                            {
                                var id = Company.Trim();
                                Get_Geo_Id = Get_Geo_Id.Where(e => e.Company != null && e.Company.Code == id).ToList();
                            }
                            if (Division != null && Division != "#")
                            {
                                var id = Division.Trim();
                                Get_Geo_Id = Get_Geo_Id.Where(e => e.Division != null && e.Division.Code == id).ToList();
                            }
                            if (Location != null && Location != "#")
                            {
                                var id = Location.Trim();
                                Get_Geo_Id = Get_Geo_Id.Where(e => e.Location != null && e.Location.LocationObj != null && e.Location.LocationObj.LocCode == id).ToList();
                            }
                            if (Department != null && Department != "#")
                            {
                                var id = Department.Trim();
                                Get_Geo_Id = Get_Geo_Id.Where(e => e.Department != null && e.Department.DepartmentObj != null && e.Department.DepartmentObj.DeptCode == id).ToList();
                            }
                            if (Group != null && Group != "#")
                            {
                                var id = Group.Trim();
                                Get_Geo_Id = Get_Geo_Id.Where(e => e.Group != null && e.Group.Code == id).ToList();
                            }
                            if (Unit != null && Unit != "#")
                            {
                                var id = Unit.Trim();
                                Get_Geo_Id = Get_Geo_Id.Where(e => e.Unit != null && e.Unit.Code == id).ToList();
                            }

                            if (Get_Geo_Id != null && Get_Geo_Id.Count == 1)
                            {
                                OSB.GeoStruct = Get_Geo_Id[0];
                            }

                            if (OSB.GeoStruct == null)
                            {
                                return Json(new { data = rowno, success = false, responseText = "GeoStruct not found." }, JsonRequestBehavior.AllowGet);
                            }
                            //paystruct
                            var Get_Pay_Id = db.PayStruct.Include(e => e.Company).Include(e => e.Grade).Include(e => e.JobStatus).Include(e => e.JobStatus.EmpStatus).Include(e => e.JobStatus.EmpActingStatus).Include(e => e.Level).ToList();
                            string Grade = form["OldGrade"] == "null" ? null : form["OldGrade"];
                            string Level = form["OldLevel"] == "null" ? null : form["OldLevel"];
                            string OldEmpStatus = form["OldEmpStatus"] == "null" ? null : form["OldEmpStatus"];
                            string OldEmpActingStatus = form["OldEmpActingStatus"] == "null" ? null : form["OldEmpActingStatus"];
                            if (Grade != null && Grade != "#")
                            {
                                var id = Grade.Trim();
                                Get_Pay_Id = Get_Pay_Id.Where(e => e.Grade != null && e.Grade.Code == id).ToList();
                            }
                            if (Level != null && Level != "#")
                            {
                                var id = Level.Trim();
                                Get_Pay_Id = Get_Pay_Id.Where(e => e.Level != null && e.Level.Code == id).ToList();
                            }

                            if (OldEmpStatus != null && OldEmpStatus != "#")
                            {
                                var id = OldEmpStatus.Trim().ToUpper();
                                Get_Pay_Id = Get_Pay_Id.Where(e => e.JobStatus != null && e.JobStatus.EmpStatus != null && e.JobStatus.EmpStatus.LookupVal.ToUpper() == id).ToList();
                            }
                            if (OldEmpActingStatus != null && OldEmpActingStatus != "#")
                            {
                                var id = OldEmpActingStatus.Trim().ToUpper();
                                Get_Pay_Id = Get_Pay_Id.Where(e => e.JobStatus != null && e.JobStatus.EmpActingStatus != null && e.JobStatus.EmpActingStatus.LookupVal.ToUpper() == id).ToList();
                            }
                            if (Get_Pay_Id != null && Get_Pay_Id.Count == 1)
                            {
                                OSB.OldPayStruct = Get_Pay_Id[0];
                            }

                            if (OSB.OldPayStruct == null)
                            {
                                return Json(new { data = rowno, success = false, responseText = "Old PayStruct not found." }, JsonRequestBehavior.AllowGet);
                            }


                            //Funstrct
                            string OldJob = form["OldJob"] == "null" ? null : form["OldJob"];
                            string OldPosition = form["OldPosition"] == "null" ? null : form["OldPosition"];
                            var Get_Fun_Id = db.FuncStruct.Include(e => e.Company).Include(e => e.Job).Include(e => e.JobPosition).ToList();
                            if (OldJob != null && OldJob != "#")
                            {
                                var id = OldJob.Trim();
                                Get_Fun_Id = Get_Fun_Id.Where(e => e.Job != null && e.Job.Code == id).ToList();
                            }
                            if (OldPosition != null && OldPosition != "#")
                            {
                                var id = OldPosition.Trim();
                                Get_Fun_Id = Get_Fun_Id.Where(e => e.JobPosition != null && e.JobPosition.JobPositionCode == id).ToList();
                            }
                            if (Get_Fun_Id != null && Get_Fun_Id.Count == 1)
                            {
                                OSB.OldFuncStruct = Get_Fun_Id[0];
                            }
                            if (OSB.OldFuncStruct == null)
                            {
                                return Json(new { data = rowno, success = false, responseText = "Old FuncStruct not found." }, JsonRequestBehavior.AllowGet);
                            }







                            var Get_Pay_IdNew = db.PayStruct.Include(e => e.Company).Include(e => e.Grade).Include(e => e.JobStatus).Include(e => e.Level).ToList();
                            string NewGrade = form["NewGrade"] == "null" ? null : form["NewGrade"];
                            string NewLevel = form["NewLevel"] == "null" ? null : form["NewLevel"];
                            string NewEmpStatus = form["NewEmpStatus"] == "null" ? null : form["NewEmpStatus"];
                            string NewEmpActingStatus = form["NewEmpActingStatus"] == "null" ? null : form["NewEmpActingStatus"];
                            if (NewGrade != null && NewGrade != "#")
                            {
                                var id = NewGrade.Trim();
                                Get_Pay_IdNew = Get_Pay_IdNew.Where(e => e.Grade != null && e.Grade.Code == id).ToList();
                            }
                            if (NewLevel != null && NewLevel != "#")
                            {
                                var id = NewLevel.Trim();
                                Get_Pay_IdNew = Get_Pay_IdNew.Where(e => e.Level != null && e.Level.Code == id).ToList();
                            }
                            if (NewEmpStatus != null && NewEmpStatus != "#")
                            {
                                var id = NewEmpStatus.Trim().ToUpper();
                                Get_Pay_IdNew = Get_Pay_IdNew.Where(e => e.JobStatus != null && e.JobStatus.EmpStatus != null && e.JobStatus.EmpStatus.LookupVal.ToUpper() == id).ToList();
                            }
                            if (NewEmpActingStatus != null && NewEmpActingStatus != "#")
                            {
                                var id = NewEmpActingStatus.Trim().ToUpper();
                                Get_Pay_IdNew = Get_Pay_IdNew.Where(e => e.JobStatus != null && e.JobStatus.EmpActingStatus != null && e.JobStatus.EmpActingStatus.LookupVal.ToUpper() == id).ToList();
                            }
                            if (Get_Pay_IdNew != null && Get_Pay_IdNew.Count == 1)
                            {
                                OSB.NewPayStruct = Get_Pay_IdNew[0];
                            }

                            //if (OSB.NewPayStruct == null)
                            //{
                            //    return Json(new { data = rowno, success = false, responseText = "New PayStruct not found." }, JsonRequestBehavior.AllowGet);
                            //}

                            var Get_JobStatus_IdNew = db.JobStatus.Include(e => e.EmpStatus).Include(e => e.EmpActingStatus).ToList();
                            if (NewEmpStatus != null && NewEmpStatus != "#")
                            {
                                var id = NewEmpStatus.Trim().ToUpper();
                                Get_JobStatus_IdNew = Get_JobStatus_IdNew.Where(e => e.EmpStatus != null && e.EmpStatus.LookupVal.ToUpper() == id).ToList();
                            }
                            if (NewEmpActingStatus != null && NewEmpActingStatus != "#")
                            {
                                var id = NewEmpActingStatus.Trim().ToUpper();
                                Get_JobStatus_IdNew = Get_JobStatus_IdNew.Where(e => e.EmpActingStatus != null && e.EmpActingStatus.LookupVal.ToUpper() == id).ToList();
                            }


                            string NewJob = form["NewJob"] == "null" ? null : form["NewJob"];
                            string NewPosition = form["NewPosition"] == "null" ? null : form["NewPosition"];
                            var Get_Fun_IdNew = db.FuncStruct.Include(e => e.Company).Include(e => e.Job).Include(e => e.JobPosition).ToList();
                            if (NewJob != null && NewJob != "#")
                            {
                                var id = NewJob.Trim();
                                Get_Fun_IdNew = Get_Fun_IdNew.Where(e => e.Job != null && e.Job.Code == id).ToList();
                            }
                            if (NewPosition != null && NewPosition != "#")
                            {
                                var id = NewPosition.Trim();
                                Get_Fun_IdNew = Get_Fun_IdNew.Where(e => e.JobPosition != null && e.JobPosition.JobPositionCode == id).ToList();
                            }
                            if (Get_Fun_IdNew != null && Get_Fun_IdNew.Count == 1)
                            {
                                OSB.NewFuncStruct = Get_Fun_IdNew[0];
                            }

                            //if (OSB.NewFuncStruct == null)
                            //{
                            //    return Json(new { data = rowno, success = false, responseText = "New FuncStruct not found." }, JsonRequestBehavior.AllowGet);
                            //}

                            if (ids != null)
                            {
                                foreach (var i in ids)
                                {
                                    Employee OEmployee = null;
                                    EmployeePayroll OEmployeePayroll = null;
                                    OEmployeePayroll = db.EmployeePayroll
                                                   .Include(q => q.Employee)
                                                   .Include(q => q.Employee.ServiceBookDates)
                                                   .Include(e => e.OtherServiceBook)
                                                   .Include(e => e.OtherServiceBook.Select(r => r.OthServiceBookActivity))
                                                   .Where(e => e.Employee.Id == i).AsNoTracking().AsParallel().SingleOrDefault();

                                    if (OEmployeePayroll != null)
                                    {
                                        var OEmployeePayrollS = OEmployeePayroll.Employee.ServiceBookDates.ServiceLastDate;
                                        var OEmployeePayrollR = OEmployeePayroll.Employee.ServiceBookDates.ResignationDate;
                                        if (OEmployeePayrollS == null && OEmployeePayrollR == null)
                                        {
                                            OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct)
                                                .Where(r => r.Id == i).AsNoTracking().AsParallel().SingleOrDefault();

                                            foreach (int SH in ActivityHead)
                                            {
                                                var val = db.OthServiceBookActivity.Find(SH);
                                                OSB.OthServiceBookActivity = val;

                                                var EmpSalT = OEmployeePayroll.OtherServiceBook != null ? OEmployeePayroll.OtherServiceBook.Where(e => e.OthServiceBookActivity.Id == OSB.OthServiceBookActivity.Id && e.ProcessOthDate.Value.ToShortDateString() == ActivityDate.ToString()) : null;
                                                if (EmpSalT != null && EmpSalT.Count() > 0)
                                                {
                                                    if (EmpCode == null || EmpCode == "")
                                                    {
                                                        EmpCode = OEmployee.EmpCode;
                                                    }
                                                    else
                                                    {
                                                        EmpCode = EmpCode + ", " + OEmployee.EmpCode;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            if (EmpCode != null)
                            {
                                return Json(new { data = rowno, success = false, responseText = "OtherServiceBook already exists for employee " + EmpCode + "." }, JsonRequestBehavior.AllowGet);
                            }

                            if (!string.IsNullOrEmpty(sEmpCode))
                            {
                                foreach (var i in ids)
                                {
                                    Employee OEmployee = null;
                                    EmployeePayroll OEmployeePayroll = null;

                                    OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct).Include(e => e.PayStruct.JobStatus)
                                                                                .Include(e => e.EmpOffInfo).Include(e => e.EmpOffInfo.PayScale)
                                                                                .Where(r => r.Id == i).SingleOrDefault();

                                    OEmployeePayroll = db.EmployeePayroll.Include(e => e.OtherServiceBook).Where(e => e.Employee.Id == i).SingleOrDefault();

                                    if (OEmployeePayroll.OtherServiceBook.Any(d => d.ProcessOthDate.Value.ToShortDateString() == ActivityDate.ToString()))
                                    {
                                        {
                                            Msg = "Already OtherServiceBook for Date= " + ActivityDate;
                                            //  return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                            return Json(new { data = rowno, success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        }
                                    }

                                    int OtherActivityId = 0;
                                    int ReasonId = 0;
                                    string LookupVal = "", Narration = "";
                                    if (ActivityNamelist != null && ActivityNamelist != "")
                                    {
                                        OtherActivityId = db.OthServiceBookActivity.Where(e => e.Name == ActivityNamelist).Select(e => e.Id).FirstOrDefault(); //int.Parse(ActivityNamelist);
                                        LookupVal = db.LookupValue.Where(e => e.Id == OtherActivityId).Select(e => e.LookupVal.ToUpper()).SingleOrDefault();
                                    }
                                    int PayScaleAgrId = db.PayScaleAgreement.Where(e => e.EndDate == null).Select(e => e.Id).SingleOrDefault();
                                    var Newagreement = db.PayScaleAgreement.Include(x => x.OthServiceBookActivity)
                                       .Include(x => x.OthServiceBookActivity.Select(y => y.OthServiceBookPolicy))
                                       .Include(x => x.OthServiceBookActivity.Select(y => y.OtherSerBookActList))
                                       .Where(x => x.Id == PayScaleAgrId).FirstOrDefault();

                                    if (OSB.OthServiceBookActivity.OthServiceBookPolicy.IsFuncStructChange == true)
                                    {
                                        if (OSB.NewFuncStruct == null)
                                        {
                                            Msg = "New FuncStruct not found.";
                                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        }
                                    }


                                    if (OSB.OthServiceBookActivity.OthServiceBookPolicy.IsPayJobStatusChange == true)
                                    {
                                        if (OSB.NewPayStruct == null)
                                        {
                                            Msg = "New PayStruct not found.";
                                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        }
                                    }



                                    if (OEmployee.EmpOffInfo != null && OEmployee.EmpOffInfo.PayScale != null)
                                        OSB.NewPayScale = db.PayScale.Find(OEmployee.EmpOffInfo.PayScale.Id);

                                    if (OEmployee.EmpOffInfo != null && OEmployee.EmpOffInfo.PayScale != null)
                                    {
                                        var PayScaleAgr = db.PayScaleAgreement.Where(e => e.PayScale.Id == OEmployee.EmpOffInfo.PayScale.Id && e.EndDate == null).SingleOrDefault();
                                        OSB.NewPayScaleAgreement = PayScaleAgr;
                                    }



                                    if (ActivityHead.Count == 0)
                                    {
                                        return Json(new { data = rowno, success = false, responseText = "Please enter valid ActivityName." }, JsonRequestBehavior.AllowGet);
                                    }

                                    foreach (int SH in ActivityHead)
                                    {
                                        var val = db.OthServiceBookActivity.Find(SH);
                                        OSB.OthServiceBookActivity = val;
                                        OSB.Narration = "Other Activity";
                                        OSB.ReleaseFlag = true;

                                        OSB.NewPayScaleAgreement = db.PayScaleAgreement.SingleOrDefault();
                                        OSB.OldPayScaleAgreement = db.PayScaleAgreement.SingleOrDefault();

                                        OSB.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                                        OtherServiceBook ObjOSB = new OtherServiceBook();
                                        {
                                            ObjOSB.EmployeeCTC = null;
                                            ObjOSB.ProcessOthDate = OSB.ProcessOthDate;
                                            ObjOSB.NewBasic = OSB.NewBasic;
                                            ObjOSB.ReleaseFlag = OSB.ReleaseFlag;
                                            ObjOSB.ReleaseDate = OSB.ReleaseDate;
                                            ObjOSB.Narration = OSB.Narration;
                                            ObjOSB.GeoStruct = OSB.GeoStruct;
                                            ObjOSB.NewFuncStruct = OSB.NewFuncStruct;
                                            ObjOSB.NewPayScale = OSB.NewPayScale;
                                            ObjOSB.NewPayScaleAgreement = OSB.NewPayScaleAgreement;
                                            ObjOSB.NewPayStruct = OSB.NewPayStruct;//db.PayStruct.Find(PromotionServiceBook.NewPayStruct), 
                                            ObjOSB.OldFuncStruct = OSB.OldFuncStruct;
                                            ObjOSB.OldPayScale = OSB.OldPayScale;
                                            ObjOSB.OldPayScaleAgreement = OSB.OldPayScaleAgreement;
                                            ObjOSB.OldPayStruct = OSB.OldPayStruct;
                                            ObjOSB.OthServiceBookActivity = OSB.OthServiceBookActivity;
                                            ObjOSB.DBTrack = OSB.DBTrack;
                                        }

                                        errorList = ValidateObj(ObjOSB);
                                        if (errorList.Count > 0)
                                        {
                                            return Json(new { success = false, data = rowno, responseText = string.Join(",", errorList) }, JsonRequestBehavior.AllowGet);
                                        }

                                        using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                    new System.TimeSpan(0, 30, 0)))
                                        {
                                            OSB.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                                            db.OtherServiceBook.Add(OSB);
                                            db.SaveChanges();

                                            List<OtherServiceBook> OthServBkList = new List<OtherServiceBook>();
                                            OthServBkList.Add(OSB);
                                            OthServBkList.AddRange(OEmployeePayroll.OtherServiceBook);

                                            OEmployeePayroll.OtherServiceBook = OthServBkList;
                                            db.EmployeePayroll.Attach(OEmployeePayroll);
                                            db.Entry(OEmployeePayroll).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();
                                            db.Entry(OEmployeePayroll).State = System.Data.Entity.EntityState.Detached;

                                            ts.Complete();
                                            //// return Json(new Object[] { "", "", "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
                                            ////    List<string> Msgs = new List<string>();
                                        }
                                    }
                                }
                                return Json(new { data = rowno, success = true, responseText = "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                return Json(new { data = rowno, success = false, responseText = "Select Employee" }, JsonRequestBehavior.AllowGet);
                            }
                        }

                    case "ITSECTION10PAYMENT":
                        using (DataBaseContext db = new DataBaseContext())
                        {
                            ITSection10Payment ITSec10Pay = new ITSection10Payment();

                            String ITSection10list = form["ITSection10"] == "0" ? "" : form["ITSection10"].Replace("#", " ").Trim();
                            String InvestmentDate = form["InvestmentDate"] != null ? form["InvestmentDate"] : null;
                            String ActualInvestment = form["ActualInvestment"] == "0" ? "0" : form["ActualInvestment"];
                            String DeclaredInvestment = form["DeclaredInvestment"] == "0" ? "0" : form["DeclaredInvestment"];
                            DateTime FinYearFrom = Convert.ToDateTime(form["FinYearFrom"]);
                            DateTime FinYearTo = Convert.ToDateTime(form["FinYearTo"]);

                            if (InvestmentDate != null)
                            {
                                ITSec10Pay.InvestmentDate = Convert.ToDateTime(InvestmentDate);
                            }
                            if (ActualInvestment != null)
                            {
                                ITSec10Pay.ActualInvestment = Convert.ToDouble(ActualInvestment);
                            }
                            if (DeclaredInvestment != null)
                            {
                                ITSec10Pay.DeclaredInvestment = Convert.ToDouble(DeclaredInvestment);
                            }

                            int CompId = 0;
                            if (Session["CompId"] != null)
                                CompId = int.Parse(Session["CompId"].ToString());


                            string EmpCode = null;
                            List<String> ITSection10CodeList = new List<String>();
                            List<int> ITSection10Head = new List<int>();
                            if (ITSection10list != null && ITSection10list != "")
                            {
                                ITSection10CodeList = Utility.StringIdsToListString(ITSection10list);
                            }
                            foreach (var item in ITSection10CodeList)
                            {
                                var aa = db.ITSection10.Where(e => e.ExemptionCode == item).Select(e => e.Id).FirstOrDefault();

                                if (aa != 0)
                                {
                                    if (aa != 0)
                                    {
                                        ITSection10Head.Add(aa);
                                    }
                                }
                            }

                            int OFinancialYear = db.Calendar.Where(e => e.Name.LookupVal.ToUpper() == "FINANCIALYEAR" && e.FromDate >= FinYearFrom && e.ToDate <= FinYearTo).FirstOrDefault().Id;

                            if (ids != null)
                            {
                                foreach (var i in ids)
                                {
                                    Employee OEmployee = null;
                                    EmployeePayroll OEmployeePayroll = null;
                                    OEmployeePayroll = db.EmployeePayroll
                                                 .Include(q => q.Employee)
                                                 .Include(q => q.Employee.ServiceBookDates)
                                                 .Include(e => e.ITSection10Payment)
                                                 .Include(e => e.ITSection10Payment.Select(r => r.ITSection10))
                                                 .Include(e => e.ITSection10Payment.Select(r => r.ITSection))
                                                 .Include(e => e.ITSection10Payment.Select(r => r.FinancialYear))
                                                 .Where(e => e.Employee.Id == i).AsNoTracking().AsParallel().SingleOrDefault();

                                    if (OEmployeePayroll != null)
                                    {
                                        var OEmployeePayrollS = OEmployeePayroll.Employee.ServiceBookDates.ServiceLastDate;
                                        var OEmployeePayrolleR = OEmployeePayroll.Employee.ServiceBookDates.ResignationDate;
                                        if (OEmployeePayrollS == null && OEmployeePayrolleR == null)
                                        {
                                            OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct)
                                           .Where(r => r.Id == i).AsNoTracking().AsParallel().SingleOrDefault();

                                            foreach (int ITH in ITSection10Head)
                                            {
                                                var val = db.ITSection10.Find(ITH);
                                                ITSec10Pay.ITSection10 = val;

                                                var EmpSalT = OEmployeePayroll.ITSection10Payment != null ? OEmployeePayroll.ITSection10Payment.Where(e => e.ITSection10.Id == ITSec10Pay.ITSection10.Id && e.FinancialYear.Id == OFinancialYear && e.InvestmentDate == ITSec10Pay.InvestmentDate) : null;
                                                if (EmpSalT != null && EmpSalT.Count() > 0)
                                                {
                                                    if (EmpCode == null || EmpCode == "")
                                                    {
                                                        EmpCode = OEmployee.EmpCode;
                                                    }
                                                    else
                                                    {
                                                        EmpCode = EmpCode + ", " + OEmployee.EmpCode;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            if (EmpCode != null)
                            {
                                return Json(new { data = rowno, success = false, responseText = "ITSection10Payment already exists for employee " + EmpCode + "." }, JsonRequestBehavior.AllowGet);
                            }

                            if (!string.IsNullOrEmpty(sEmpCode))
                            {
                                foreach (var i in ids)
                                {
                                    int OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct).Include(e => e.PayStruct.JobStatus)
                                   .Include(e => e.EmpOffInfo).Where(r => r.Id == i).Select(q => q.Id).SingleOrDefault();

                                    EmployeePayroll OEmployeePayroll = db.EmployeePayroll
                                                        .Include(e => e.ITSection10Payment)
                                                        .Include(e => e.ITSection10Payment.Select(q => q.ITSection10)).Include(e => e.ITSection10Payment.Select(q => q.ITSection))
                                                        .Include(e => e.ITSection10Payment.Select(r => r.FinancialYear))
                                                        .Where(e => e.Employee.Id == OEmployee).SingleOrDefault();

                                    if (ITSection10Head.Count == 0)
                                    {
                                        return Json(new { data = rowno, success = false, responseText = "Please enter valid ITSection10Head." }, JsonRequestBehavior.AllowGet);
                                    }

                                    foreach (int SH in ITSection10Head)
                                    {
                                        var val = db.ITSection10.Find(SH);
                                        ITSec10Pay.ITSection10 = val;

                                        ITSec10Pay.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };



                                        int OITSection = db.ITSection.Include(E => E.ITSectionList).Where(e => e.ITSectionList.LookupVal.ToUpper() == "SECTION10B").FirstOrDefault().Id;
                                        ITSec10Pay.Narration = "Upload data";
                                        ITSection10Payment ObjITsec10 = new ITSection10Payment();
                                        {
                                            ObjITsec10.FinancialYear = db.Calendar.Where(e => e.Id == OFinancialYear).SingleOrDefault(); ;
                                            ObjITsec10.InvestmentDate = ITSec10Pay.InvestmentDate;
                                            ObjITsec10.ITSection = db.ITSection.Where(e => e.Id == OITSection).SingleOrDefault();
                                            ObjITsec10.ITSection10 = ITSec10Pay.ITSection10;
                                            ObjITsec10.Narration = ITSec10Pay.Narration;
                                            ObjITsec10.ActualInvestment = ITSec10Pay.ActualInvestment;
                                            ObjITsec10.DeclaredInvestment = ITSec10Pay.DeclaredInvestment;
                                            ObjITsec10.DBTrack = ITSec10Pay.DBTrack;
                                        }

                                        errorList = ValidateObj(ObjITsec10);
                                        if (errorList.Count > 0)
                                        {
                                            return Json(new { success = false, data = rowno, responseText = string.Join(",", errorList) }, JsonRequestBehavior.AllowGet);
                                        }

                                        using (TransactionScope ts = new TransactionScope())
                                        {
                                            try
                                            {
                                                db.ITSection10Payment.Add(ObjITsec10);
                                                db.SaveChanges();

                                                List<ITSection10Payment> ITSection10PayList = new List<ITSection10Payment>();
                                                ITSection10PayList.Add(ObjITsec10);
                                                if (OEmployeePayroll.ITSection10Payment.Count > 0)
                                                {
                                                    ITSection10PayList.AddRange(OEmployeePayroll.ITSection10Payment);
                                                }
                                                OEmployeePayroll.ITSection10Payment = ITSection10PayList;
                                                db.EmployeePayroll.Attach(OEmployeePayroll);
                                                db.Entry(OEmployeePayroll).State = System.Data.Entity.EntityState.Modified;
                                                db.SaveChanges();
                                                db.Entry(OEmployeePayroll).State = System.Data.Entity.EntityState.Detached;

                                                ts.Complete();
                                            }
                                            catch (DataException ex)
                                            {
                                                //LogFile Logfile = new LogFile();
                                                //ErrorLog Err = new ErrorLog()
                                                //{
                                                //    ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                                                //    ExceptionMessage = ex.Message,
                                                //    ExceptionStackTrace = ex.StackTrace,
                                                //    LineNo = Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                                                //    LogTime = DateTime.Now
                                                //};
                                                //Logfile.CreateLogFile(Err);
                                                //return Json(new { sucess = false, responseText = ex.InnerException }, JsonRequestBehavior.AllowGet);
                                            }
                                            catch (Exception ex)
                                            {
                                                LogFile Logfile = new LogFile();
                                                ErrorLog Err = new ErrorLog()
                                                {
                                                    ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                                                    ExceptionMessage = ex.Message,
                                                    ExceptionStackTrace = ex.StackTrace,
                                                    LineNo = Convert.ToString(new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                                                    LogTime = DateTime.Now
                                                };
                                                Logfile.CreateLogFile(Err);
                                            }
                                        }
                                    }
                                }
                                return Json(new { data = rowno, success = true, responseText = "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                return Json(new { data = rowno, success = false, responseText = "Select Employee" }, JsonRequestBehavior.AllowGet);
                            }
                        }
                    case "ITSECTION24PAYMENT":
                        using (DataBaseContext db = new DataBaseContext())
                        {
                            ITSection24Payment c = new ITSection24Payment();

                            String PaymentName = form["PaymentName"] != null ? form["PaymentName"] : null;
                            String LoanAdvHdCode = form["LoanAdvanceHead"] == "0" ? "0" : form["LoanAdvanceHead"];
                            String ITSection = form["ITSection"] != null ? form["ITSection"] : null;
                            String ITSectionType = form["ITSectionType"] == "0" ? "0" : form["ITSectionType"];
                            String InvestmentDate = form["InvestmentDate"] == "0" ? "0" : form["InvestmentDate"];
                            String ActualInterest = form["ActualInterest"] != null ? form["ActualInterest"] : null;
                            String DeclaredInterest = form["DeclaredInterest"] == "0" ? "0" : form["DeclaredInterest"];
                            String Narration = form["Narration"] == "0" ? "0" : form["Narration"];

                            int OFinancialYear = db.Calendar.Where(e => e.Name.LookupVal.ToUpper() == "FINANCIALYEAR" && e.Default == true).FirstOrDefault().Id;

                            if (!string.IsNullOrEmpty(sEmpCode))
                            {
                                foreach (var i in ids)
                                {
                                    int OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct).Include(e => e.PayStruct.JobStatus)
                                   .Include(e => e.EmpOffInfo).Where(r => r.Id == i).Select(q => q.Id).SingleOrDefault();
                                    EmployeePayroll OEmployeePayroll = db.EmployeePayroll
                                                             .Include(e => e.Employee)
                                                             .Include(e => e.ITSection24Payment)
                                                             .Include(e => e.ITSection24Payment.Select(r => r.LoanAdvanceHead))
                                                             .Include(e => e.ITSection24Payment.Select(r => r.LoanAdvanceHead.SalaryHead))
                                                             .Include(e => e.ITSection24Payment.Select(r => r.LoanAdvanceHead.ITSection))
                                                             .Include(e => e.ITSection24Payment.Select(r => r.FinancialYear))
                                                             .Where(e => e.Employee.Id == OEmployee).SingleOrDefault();

                                    if (!string.IsNullOrEmpty(InvestmentDate))
                                    {
                                        c.InvestmentDate = Convert.ToDateTime(InvestmentDate);
                                    }
                                    if (!string.IsNullOrEmpty(ActualInterest))
                                    {
                                        c.ActualInterest = Convert.ToDouble(ActualInterest);
                                    }
                                    if (!string.IsNullOrEmpty(DeclaredInterest))
                                    {
                                        c.DeclaredInterest = Convert.ToDouble(DeclaredInterest);
                                    }

                                    LoanAdvanceHead OLoanAdvanceHead = null;
                                    if (!string.IsNullOrEmpty(LoanAdvHdCode))
                                    {
                                        OLoanAdvanceHead = db.LoanAdvanceHead.Where(e => e.Code == LoanAdvHdCode).SingleOrDefault();
                                        c.LoanAdvanceHead = OLoanAdvanceHead;

                                    }

                                    ITSection OITSection = null;
                                    if (!string.IsNullOrEmpty(ITSection) && !string.IsNullOrEmpty(ITSectionType))
                                    {
                                        OITSection = db.ITSection
                                        .Include(e => e.ITSectionList)
                                        .Include(e => e.ITSectionListType)
                                        .Where(e => e.ITSectionList.LookupVal.ToUpper() == ITSection.ToUpper() && e.ITSectionListType.LookupVal.ToUpper() == ITSectionType.ToUpper()).SingleOrDefault();
                                        c.ITSection = OITSection;

                                    }

                                    c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                    c.PaymentName = PaymentName.Replace("#", " ");
                                    c.Narration = Narration;
                                    c.FinancialYear = db.Calendar.Where(e => e.Id == OFinancialYear).SingleOrDefault();

                                    if (OEmployeePayroll != null && OLoanAdvanceHead != null && OITSection != null)
                                    {
                                        ITSection24Payment dbITSection24Payment = db.ITSection24Payment
                                        .Include(e => e.LoanAdvanceHead)
                                        .Include(e => e.ITSection)
                                        .Include(e => e.EmployeePayroll)
                                        .Where(e => e.EmployeePayroll.Id == OEmployeePayroll.Id && e.ITSection.Id == OITSection.Id && e.LoanAdvanceHead.Id == OLoanAdvanceHead.Id).FirstOrDefault();

                                        if (dbITSection24Payment != null)
                                        {
                                            DateTime? FormInvestmentDate = Convert.ToDateTime(InvestmentDate);

                                            if (dbITSection24Payment.InvestmentDate == FormInvestmentDate)
                                            {
                                                return Json(new { data = rowno, success = false, responseText = "Record already exists" }, JsonRequestBehavior.AllowGet);
                                            }
                                        }


                                    }

                                    ITSection24Payment ObjAddITSec24Payment = new ITSection24Payment();
                                    {
                                        ObjAddITSec24Payment.PaymentName = c.PaymentName;
                                        ObjAddITSec24Payment.LoanAdvanceHead = c.LoanAdvanceHead;
                                        ObjAddITSec24Payment.ITSection = c.ITSection;
                                        ObjAddITSec24Payment.InvestmentDate = c.InvestmentDate;
                                        ObjAddITSec24Payment.ActualInterest = c.ActualInterest;
                                        ObjAddITSec24Payment.DeclaredInterest = c.DeclaredInterest;
                                        ObjAddITSec24Payment.Narration = c.Narration;
                                        ObjAddITSec24Payment.FinancialYear = c.FinancialYear;
                                        ObjAddITSec24Payment.DBTrack = c.DBTrack;
                                    }

                                    using (TransactionScope ts = new TransactionScope())
                                    {
                                        try
                                        {
                                            db.ITSection24Payment.Add(ObjAddITSec24Payment);
                                            db.SaveChanges();

                                            List<ITSection24Payment> ITSection24PayList = new List<ITSection24Payment>();
                                            ITSection24PayList.Add(ObjAddITSec24Payment);
                                            if (OEmployeePayroll.ITSection24Payment.Count > 0)
                                            {
                                                ITSection24PayList.AddRange(OEmployeePayroll.ITSection24Payment);
                                            }
                                            OEmployeePayroll.ITSection24Payment = ITSection24PayList;
                                            db.EmployeePayroll.Attach(OEmployeePayroll);
                                            db.Entry(OEmployeePayroll).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();
                                            db.Entry(OEmployeePayroll).State = System.Data.Entity.EntityState.Detached;

                                            ts.Complete();
                                        }
                                        catch (Exception ex)
                                        {
                                            LogFile Logfile = new LogFile();
                                            ErrorLog Err = new ErrorLog()
                                            {
                                                ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                                                ExceptionMessage = ex.Message,
                                                ExceptionStackTrace = ex.StackTrace,
                                                LineNo = Convert.ToString(new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                                                LogTime = DateTime.Now
                                            };
                                            Logfile.CreateLogFile(Err);
                                        }

                                    }

                                }
                                return Json(new { data = rowno, success = true, responseText = "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                return Json(new { data = rowno, success = false, responseText = "Select Employee" }, JsonRequestBehavior.AllowGet);
                            }


                        }
                    case "ADDRESS":
                        using (DataBaseContext db = new DataBaseContext())
                        {
                            Address RAdd = new Address();

                            string AddressType = form["AddressType"] == null ? "" : form["AddressType"];
                            RAdd.Address1 = form["Address1"] == null ? "" : form["Address1"].Replace("#", " ").Trim();
                            RAdd.Address2 = form["Address2"] == null ? "" : form["Address2"].Replace("#", " ").Trim();
                            RAdd.Address3 = form["Address3"] == null ? "" : form["Address3"].Replace("#", " ").Trim();
                            RAdd.Landmark = form["Landmark"] == null ? "" : form["Landmark"].Replace("#", " ").Trim();
                            string AreaCode = form["AreaCode"] == null ? "" : form["AreaCode"];
                            string AreaName = form["AreaName"] == null ? "" : form["AreaName"];
                            int AreaPincode = form["AreaPincode"] == null ? 0 : Convert.ToInt32(form["AreaPincode"]);
                            string CityCategory = form["CityCategory"] == null ? "" : form["CityCategory"];
                            string CityCode = form["CityCode"] == null ? "" : form["CityCode"];
                            string CityName = form["CityName"] == null ? "" : form["CityName"];
                            string TalukaCode = form["TalukaCode"] == null ? "" : form["TalukaCode"];
                            string TalukaName = form["TalukaName"] == null ? "" : form["TalukaName"];
                            string DistrictCode = form["DistrictCode"] == null ? "" : form["DistrictCode"];
                            string DistrictName = form["DistrictName"] == null ? "" : form["DistrictName"];
                            string StateRegionCode = form["StateRegionCode"] == null ? "" : form["StateRegionCode"];
                            string StateRegionName = form["StateRegionName"] == null ? "" : form["StateRegionName"];
                            string StateCode = form["StateCode"] == null ? "" : form["StateCode"];
                            string StateName = form["StateName"] == null ? "" : form["StateName"];
                            string CountryCode = form["CountryCode"] == null ? "" : form["CountryCode"];
                            string CountryName = form["CountryName"] == null ? "" : form["CountryName"];
                            string EmailId = form["EmailId"] == null ? "" : form["EmailId"];
                            string Website = form["Website"] == null ? "" : form["Website"];
                            string FaxNo = form["FaxNo"] == null ? "" : form["FaxNo"];
                            string MobileNo = form["MobileNo"] == null ? "" : form["MobileNo"];
                            string STDCode = form["STDCode"] == null ? "" : form["STDCode"];
                            string LandlineNo = form["LandlineNo"] == null ? "" : form["LandlineNo"];

                            int CompId = 0;
                            if (Session["CompId"] != null)
                                CompId = int.Parse(Session["CompId"].ToString());




                            //if (ids != null)
                            //{
                            //    foreach (var i in ids)
                            //    {
                            //        EmployeePayroll OEmployeePayroll = db.EmployeePayroll
                            //          .Include(q => q.Employee)
                            //          .Include(q => q.Employee.ResAddr)
                            //          .Include(q => q.Employee.ResAddr.Area)
                            //          .Include(q => q.Employee.ResAddr.City)
                            //          .Include(q => q.Employee.ResAddr.Country)
                            //          .Include(q => q.Employee.ResAddr.District)
                            //          .Include(q => q.Employee.ResAddr.State)
                            //          .Include(q => q.Employee.ResAddr.StateRegion)
                            //          .Include(q => q.Employee.ResAddr.Taluka)
                            //          .Where(e => e.Employee.Id == i).AsParallel().SingleOrDefault();
                            //    }
                            //}
                            if (!string.IsNullOrEmpty(sEmpCode))
                            {
                                foreach (var i in ids)
                                {
                                    using (TransactionScope ts = new TransactionScope())
                                    {
                                        Employee OEmployee = db.Employee
                                            .Where(r => r.Id == i).SingleOrDefault();


                                        RAdd.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                                        Area area = new Area();
                                        Area OArea = null;
                                        if (AreaCode != "" && AreaName != "")
                                        {
                                            OArea = db.Area.Where(e => e.Code.ToUpper() == AreaCode.ToUpper() && e.Name.ToUpper() == AreaName.ToUpper() && e.PinCode == AreaPincode).FirstOrDefault();
                                            if (OArea == null)
                                            {
                                                area = new Area()
                                                {
                                                    Code = AreaCode,
                                                    Name = AreaName,
                                                    PinCode = AreaPincode,
                                                    DBTrack = RAdd.DBTrack
                                                };

                                                errorList = ValidateObj(area);
                                                if (errorList.Count > 0)
                                                {
                                                    return Json(new { success = false, data = rowno, responseText = string.Join(",", errorList) }, JsonRequestBehavior.AllowGet);
                                                }

                                                db.Area.Add(area);
                                                db.SaveChanges();
                                                RAdd.Area = area;
                                            }
                                            else
                                            {
                                                RAdd.Area = OArea;
                                            }
                                        }

                                        City city = new City();
                                        City OCity = null;
                                        if (CityCode != null && CityName != "")
                                        {
                                            OCity = db.City.Include(e => e.Areas).Where(e => e.Code.ToUpper() == CityCode.ToUpper() && e.Name.ToUpper() == CityName.ToUpper()).FirstOrDefault();
                                            int CatId = db.City.Include(e => e.Category).Where(e => e.Category.LookupVal.ToUpper() == CityCategory).FirstOrDefault() != null ? db.City.Include(e => e.Category).Where(e => e.Category.LookupVal.ToUpper() == CityCategory.ToUpper()).FirstOrDefault().Category.Id : 0;
                                            if (OCity == null)
                                            {
                                                city = new City()
                                                {
                                                    Code = CityCode,
                                                    Name = CityName,
                                                    Category = db.LookupValue.Find(CatId),
                                                    DBTrack = RAdd.DBTrack
                                                };
                                                errorList = ValidateObj(city);
                                                if (errorList.Count > 0)
                                                {
                                                    return Json(new { success = false, data = rowno, responseText = string.Join(",", errorList) }, JsonRequestBehavior.AllowGet);
                                                }

                                                db.City.Add(city);
                                                db.SaveChanges();

                                                List<Area> OAreaList = new List<Area>();
                                                if (area.Id != 0)
                                                {
                                                    OAreaList.Add(area);
                                                    if (city.Areas != null && city.Areas.Count() > 0)
                                                    {
                                                        OAreaList.AddRange(city.Areas);
                                                    }
                                                    city.Areas = OAreaList;
                                                    db.City.Attach(city);
                                                    db.Entry(city).State = System.Data.Entity.EntityState.Modified;
                                                    db.SaveChanges();

                                                }

                                                RAdd.City = city;
                                            }
                                            else
                                            {
                                                List<Area> OAreaList = new List<Area>();

                                                if (area.Id != 0)
                                                {
                                                    OAreaList.Add(area);
                                                    if (OCity.Areas.Count() > 0 && !OCity.Areas.Any(t => t.Code.ToUpper() == area.Code.ToUpper()))
                                                    {
                                                        OAreaList.AddRange(OCity.Areas);
                                                        //OAreaList.Add(area);
                                                        OCity.Areas = OAreaList;
                                                        db.City.Attach(OCity);
                                                        db.Entry(OCity).State = System.Data.Entity.EntityState.Modified;
                                                        db.SaveChanges();
                                                        RAdd.City = OCity;
                                                    }
                                                }



                                                RAdd.City = OCity;

                                            }
                                        }

                                        Taluka taluka = new Taluka();
                                        Taluka OTaluka = null;
                                        if (TalukaCode != "" && TalukaName != "")
                                        {
                                            OTaluka = db.Taluka.Include(e => e.Cities).Where(e => e.Code.ToUpper() == TalukaCode.ToUpper() && e.Name.ToUpper() == TalukaName.ToUpper()).FirstOrDefault();
                                            if (OTaluka == null)
                                            {
                                                taluka = new Taluka()
                                                {
                                                    Code = TalukaCode,
                                                    Name = TalukaName,
                                                    DBTrack = RAdd.DBTrack
                                                };

                                                errorList = ValidateObj(taluka);
                                                if (errorList.Count > 0)
                                                {
                                                    return Json(new { success = false, data = rowno, responseText = string.Join(",", errorList) }, JsonRequestBehavior.AllowGet);
                                                }

                                                db.Taluka.Add(taluka);
                                                db.SaveChanges();

                                                List<City> OACityList = new List<City>();
                                                if (city.Id != 0)
                                                {
                                                    OACityList.Add(city);
                                                }

                                                if (taluka.Cities != null && taluka.Cities.Count() > 0)
                                                {
                                                    OACityList.AddRange(taluka.Cities);
                                                }
                                                taluka.Cities = OACityList;
                                                db.Taluka.Attach(taluka);
                                                db.Entry(taluka).State = System.Data.Entity.EntityState.Modified;
                                                db.SaveChanges();
                                                RAdd.Taluka = taluka;
                                            }
                                            else
                                            {

                                                List<City> OACityList = new List<City>();
                                                if (city.Id != 0)
                                                {
                                                    OACityList.Add(city);
                                                    if (OTaluka.Cities.Count() > 0 && !OTaluka.Cities.Any(t => t.Code.ToUpper() == city.Code.ToUpper()))
                                                    {
                                                        OTaluka.Cities = OACityList;
                                                        db.Taluka.Attach(OTaluka);
                                                        db.Entry(OTaluka).State = System.Data.Entity.EntityState.Modified;
                                                        db.SaveChanges();
                                                    }

                                                }
                                                RAdd.Taluka = OTaluka;
                                            }
                                        }

                                        District district = new District();
                                        District ODistrict = null;
                                        if (DistrictCode != "" && DistrictName != "")
                                        {
                                            ODistrict = db.District.Include(e => e.Talukas).Where(e => e.Code.ToUpper() == DistrictCode.ToUpper() && e.Name.ToUpper() == DistrictName.ToUpper()).FirstOrDefault();
                                            if (ODistrict == null)
                                            {
                                                district = new District()
                                                {
                                                    Code = DistrictCode,
                                                    Name = DistrictName,
                                                    DBTrack = RAdd.DBTrack
                                                };
                                                errorList = ValidateObj(district);
                                                if (errorList.Count > 0)
                                                {
                                                    return Json(new { success = false, data = rowno, responseText = string.Join(",", errorList) }, JsonRequestBehavior.AllowGet);
                                                }

                                                db.District.Add(district);
                                                db.SaveChanges();

                                                List<Taluka> OTalukaList = new List<Taluka>();
                                                OTalukaList.Add(taluka);
                                                if (district.Talukas != null && district.Talukas.Count() > 0)
                                                {
                                                    OTalukaList.AddRange(district.Talukas);
                                                }
                                                district.Talukas = OTalukaList;
                                                db.District.Attach(district);
                                                db.Entry(district).State = System.Data.Entity.EntityState.Modified;
                                                db.SaveChanges();
                                                RAdd.District = district;
                                            }
                                            else
                                            {
                                                List<Taluka> OTalukaList = new List<Taluka>();

                                                if (taluka.Id != 0)
                                                {
                                                    if (ODistrict.Talukas.Count() > 0 && !ODistrict.Talukas.Any(t => t.Code.ToUpper() == taluka.Code.ToUpper()))
                                                    {
                                                        OTalukaList.AddRange(ODistrict.Talukas);

                                                        OTalukaList.Add(taluka);

                                                        ODistrict.Talukas = OTalukaList;
                                                        db.District.Attach(ODistrict);
                                                        db.Entry(ODistrict).State = System.Data.Entity.EntityState.Modified;
                                                        db.SaveChanges();
                                                    }
                                                }
                                                RAdd.District = ODistrict;
                                            }
                                        }

                                        StateRegion stateRegion = new StateRegion();
                                        StateRegion OStateRegion = null;
                                        if (StateRegionCode != "" && StateRegionName != "")
                                        {
                                            OStateRegion = db.StateRegion.Include(e => e.Districts).Where(e => e.Code.ToUpper() == StateRegionCode.ToUpper() && e.Name.ToUpper() == StateRegionName.ToUpper()).FirstOrDefault();
                                            if (OStateRegion == null)
                                            {
                                                stateRegion = new StateRegion()
                                                {
                                                    Code = StateRegionCode,
                                                    Name = StateRegionName,
                                                    DBTrack = RAdd.DBTrack
                                                };

                                                errorList = ValidateObj(stateRegion);
                                                if (errorList.Count > 0)
                                                {
                                                    return Json(new { success = false, data = rowno, responseText = string.Join(",", errorList) }, JsonRequestBehavior.AllowGet);
                                                }

                                                db.StateRegion.Add(stateRegion);
                                                db.SaveChanges();
                                                if (district.Id != 0)
                                                {

                                                    List<District> OADistrictList = new List<District>();
                                                    OADistrictList.Add(district);
                                                    if (stateRegion.Districts != null && stateRegion.Districts.Count() > 0)
                                                    {
                                                        OADistrictList.AddRange(stateRegion.Districts);
                                                    }
                                                    stateRegion.Districts = OADistrictList;
                                                    db.StateRegion.Attach(stateRegion);
                                                    db.Entry(stateRegion).State = System.Data.Entity.EntityState.Modified;
                                                    db.SaveChanges();
                                                }
                                                RAdd.StateRegion = stateRegion;
                                            }
                                            else
                                            {
                                                List<District> OADistrictList = new List<District>();

                                                if (OStateRegion.Districts.Count() > 0 && !OStateRegion.Districts.Any(t => t.Code.ToUpper() == ODistrict.Code.ToUpper()))
                                                {
                                                    OADistrictList.AddRange(OStateRegion.Districts);
                                                    OADistrictList.Add(district);
                                                    OStateRegion.Districts = OADistrictList;
                                                    db.StateRegion.Attach(OStateRegion);
                                                    db.Entry(OStateRegion).State = System.Data.Entity.EntityState.Modified;
                                                    db.SaveChanges();
                                                }
                                                RAdd.StateRegion = OStateRegion;
                                            }
                                        }


                                        State state = new State();
                                        State OState = null;
                                        if (StateCode != "" && StateName != "")
                                        {
                                            OState = db.State.Include(e => e.StateRegions).Where(e => e.Code.ToUpper() == StateCode.ToUpper() && e.Name.ToUpper() == StateName.ToUpper()).FirstOrDefault();
                                            if (OState == null)
                                            {
                                                state = new State()
                                                {
                                                    Code = StateCode,
                                                    Name = StateName,
                                                    DBTrack = RAdd.DBTrack
                                                };

                                                errorList = ValidateObj(state);
                                                if (errorList.Count > 0)
                                                {
                                                    return Json(new { success = false, data = rowno, responseText = string.Join(",", errorList) }, JsonRequestBehavior.AllowGet);
                                                }

                                                db.State.Add(state);
                                                db.SaveChanges();

                                                if (stateRegion.Id != 0)
                                                {

                                                    List<StateRegion> OStateRegionList = new List<StateRegion>();
                                                    OStateRegionList.Add(stateRegion);
                                                    if (state.StateRegions != null && state.StateRegions.Count() > 0)
                                                    {
                                                        OStateRegionList.AddRange(state.StateRegions);
                                                    }
                                                    state.StateRegions = OStateRegionList;
                                                    db.State.Attach(state);
                                                    db.Entry(state).State = System.Data.Entity.EntityState.Modified;
                                                    db.SaveChanges();
                                                }
                                                RAdd.State = state;
                                            }
                                            else
                                            {
                                                List<StateRegion> OStateRegionList = new List<StateRegion>();

                                                //if (stateRegion.Code == null)
                                                //{
                                                //    OStateRegionList.Add(OStateRegion);
                                                //    OState.StateRegions = OStateRegionList;
                                                //    db.State.Attach(OState);
                                                //    db.Entry(OState).State = System.Data.Entity.EntityState.Modified;
                                                //    db.SaveChanges();
                                                //}
                                                //else
                                                //{ OStateRegionList.Add(stateRegion); }
                                                if (stateRegion.Id != 0)
                                                {
                                                    OStateRegionList.Add(stateRegion);
                                                }
                                                if (OState.StateRegions.Count() > 0 && !OState.StateRegions.Any(t => t.Code == stateRegion.Code))
                                                {
                                                    OStateRegionList.AddRange(OState.StateRegions);
                                                    OState.StateRegions = OStateRegionList;
                                                    db.State.Attach(OState);
                                                    db.Entry(OState).State = System.Data.Entity.EntityState.Modified;
                                                    db.SaveChanges();
                                                }

                                                RAdd.State = OState;
                                            }

                                        }


                                        Country country = new Country();
                                        Country OCountry = null;
                                        if (CountryCode != "")
                                        {
                                            OCountry = db.Country.Include(e => e.States).Where(e => e.Code.ToUpper() == CountryCode).FirstOrDefault();

                                            if (OCountry == null)
                                            {
                                                country = new Country()
                                                {
                                                    Code = CountryCode,
                                                    Name = CountryName,
                                                    DBTrack = RAdd.DBTrack
                                                };
                                                errorList = ValidateObj(country);
                                                if (errorList.Count > 0)
                                                {
                                                    return Json(new { success = false, data = rowno, responseText = string.Join(",", errorList) }, JsonRequestBehavior.AllowGet);
                                                }
                                                db.Country.Add(country);
                                                db.SaveChanges();

                                                if (state.Id != 0)
                                                {
                                                    List<State> OStateList = new List<State>();
                                                    OStateList.Add(state);
                                                    if (country.States != null && country.States.Count() > 0)
                                                    {
                                                        OStateList.AddRange(country.States);
                                                    }
                                                    country.States = OStateList;
                                                    db.State.Attach(state);
                                                    db.Entry(state).State = System.Data.Entity.EntityState.Modified;
                                                    db.SaveChanges();
                                                }
                                                RAdd.Country = country;
                                            }
                                            else
                                            {
                                                List<State> OStateList = new List<State>();
                                                OStateList.Add(state);
                                                if (OCountry.States.Count() > 0 && !OCountry.States.Any(t => t.Code == RAdd.State.Code))
                                                {
                                                    OStateList.AddRange(OCountry.States);
                                                    OCountry.States = OStateList;
                                                    db.Country.Attach(OCountry);
                                                    db.Entry(OCountry).State = System.Data.Entity.EntityState.Modified;
                                                    db.SaveChanges();
                                                }

                                                RAdd.Country = OCountry;
                                            }
                                        }



                                        ContactDetails ContDetails = new ContactDetails();
                                        ContactNumbers OContNos = new ContactNumbers();
                                        List<ContactNumbers> OContNolist = new List<ContactNumbers>();
                                        ContactDetails OContDetails = null;
                                        if (EmailId != null || OContNos != null)
                                        { OContDetails = db.ContactDetails.Where(e => e.EmailId == EmailId).FirstOrDefault(); }


                                        //using (TransactionScope ts = new TransactionScope())
                                        //{
                                        try
                                        {
                                            Address ObjAdd = new Address();
                                            {
                                                ObjAdd.Address1 = RAdd.Address1;
                                                ObjAdd.Address2 = RAdd.Address2;
                                                ObjAdd.Address3 = RAdd.Address3;
                                                ObjAdd.Landmark = RAdd.Landmark;
                                                ObjAdd.Country = RAdd.Country; //db.Country.Where(e => e.Name.ToUpper() == "INDIA").SingleOrDefault();// RAdd.Country;// db.Country.Where(e => e.Id == OCountry).SingleOrDefault();
                                                ObjAdd.State = RAdd.State; //db.State.Where(e => e.Code == StateCode && e.Name == StateName).SingleOrDefault(); // RAdd.State;//db.State.Where(e => e.Id == OState).SingleOrDefault();
                                                ObjAdd.StateRegion = RAdd.StateRegion;//db.StateRegion.Where(e => e.Id == OStateRegion).SingleOrDefault();
                                                ObjAdd.District = RAdd.District;//db.District.Where(e => e.Id == ODistrict).SingleOrDefault();
                                                ObjAdd.Taluka = RAdd.Taluka;//db.Taluka.Where(e => e.Id == OTaluka).SingleOrDefault();
                                                ObjAdd.City = RAdd.City;// db.City.Where(e => e.Id == OCity).SingleOrDefault();
                                                ObjAdd.Area = RAdd.Area;//db.Area.Where(e => e.Id == OArea).SingleOrDefault();
                                                ObjAdd.DBTrack = RAdd.DBTrack;
                                            }

                                            errorList = ValidateObj(ObjAdd);
                                            if (errorList.Count > 0)
                                            {
                                                return Json(new { success = false, data = rowno, responseText = string.Join(",", errorList) }, JsonRequestBehavior.AllowGet);
                                            }

                                            db.Address.Add(ObjAdd);
                                            db.SaveChanges();


                                            if (OContDetails == null)
                                            {
                                                if ((OContNolist != null && OContNolist.Count > 0) || EmailId != "null")
                                                {
                                                    errorList = ValidateObj(OContNos);
                                                    if (errorList.Count > 0)
                                                    {
                                                        return Json(new { success = false, data = rowno, responseText = string.Join(",", errorList) }, JsonRequestBehavior.AllowGet);
                                                    }

                                                    errorList = ValidateObj(ContDetails);
                                                    if (errorList.Count > 0)
                                                    {
                                                        return Json(new { success = false, data = rowno, responseText = string.Join(",", errorList) }, JsonRequestBehavior.AllowGet);
                                                    }
                                                    //OContNos = new ContactNumbers()
                                                    //{
                                                    //   // LandlineNo = LandlineNo,
                                                    //    MobileNo = MobileNo,
                                                    //    //STDCode = STDCode,
                                                    //    DBTrack = RAdd.DBTrack
                                                    //};
                                                    //OContNolist.Add(OContNos);
                                                    ContDetails = new ContactDetails()
                                                    {
                                                        ContactNumbers = OContNolist,
                                                        EmailId = EmailId,
                                                        FaxNo = FaxNo,
                                                        Website = Website,
                                                        DBTrack = RAdd.DBTrack
                                                    };
                                                    db.ContactDetails.Add(ContDetails);
                                                    db.SaveChanges();
                                                }
                                            }

                                            if (AddressType == "R")
                                            {
                                                if (ObjAdd != null)
                                                    OEmployee.ResAddr = ObjAdd;
                                                if (ContDetails.DBTrack != null)
                                                    OEmployee.ResContact = ContDetails;
                                            }
                                            else if (AddressType == "P")
                                            {
                                                if (ObjAdd != null)
                                                    OEmployee.PerAddr = ObjAdd;
                                                if (ContDetails.DBTrack != null)
                                                    OEmployee.PerContact = ContDetails;
                                            }
                                            else if (AddressType == "C")
                                            {
                                                if (ObjAdd != null)
                                                    OEmployee.CorAddr = ObjAdd;
                                                if (ContDetails.DBTrack != null)
                                                    OEmployee.CorContact = ContDetails;
                                            }

                                            db.Employee.Attach(OEmployee);
                                            db.Entry(OEmployee).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();
                                            db.Entry(OEmployee).State = System.Data.Entity.EntityState.Detached;

                                            ts.Complete();
                                        }
                                        catch (DataException ex)
                                        {
                                            LogFile Logfile = new LogFile();
                                            ErrorLog Err = new ErrorLog()
                                            {
                                                ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                                                ExceptionMessage = ex.Message,
                                                ExceptionStackTrace = ex.StackTrace,
                                                LineNo = Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                                                LogTime = DateTime.Now
                                            };
                                            Logfile.CreateLogFile(Err);
                                            return Json(new { sucess = false, responseText = ex.InnerException }, JsonRequestBehavior.AllowGet);
                                        }
                                        catch (Exception ex)
                                        {
                                            LogFile Logfile = new LogFile();
                                            ErrorLog Err = new ErrorLog()
                                            {
                                                ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                                                ExceptionMessage = ex.Message,
                                                ExceptionStackTrace = ex.StackTrace,
                                                LineNo = Convert.ToString(new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                                                LogTime = DateTime.Now
                                            };
                                            Logfile.CreateLogFile(Err);
                                        }
                                    }
                                }
                                return Json(new { data = rowno, success = true, responseText = "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                return Json(new { data = rowno, success = false, responseText = "Select Employee" }, JsonRequestBehavior.AllowGet);
                            }
                        }
                    case "EMPSALSTRUCT":
                        using (DataBaseContext db = new DataBaseContext())
                        {
                            //EmpSalStruct ISB = new EmpSalStruct();
                            String SalHeadlist = form["SalCode"] != null ? form["SalCode"] : null;
                            SalHeadlist = SalHeadlist.Replace("#", " ");

                            String Amount = form["Amount"] != null ? form["Amount"] : null;
                            String EffectiveDate = form["EffectiveDate"] != null ? form["EffectiveDate"] : null;
                            String DesigCode = form["DesigCode"] != null ? form["DesigCode"] : null;
                            String LocCode = form["LocCode"] != null ? form["LocCode"] : null;
                            String GradeCode = form["GradeCode"] != null ? form["GradeCode"] : null;

                            DateTime date = Convert.ToDateTime(EffectiveDate).Date;


                            int CompId = 0;
                            if (SessionManager.UserName != null)
                            {
                                CompId = Convert.ToInt32(Session["CompId"]);
                            }

                            //var check = db.CPIEntryT.Where(e => e.PayMonth == date).ToList();

                            //if (check.Count() == 0)
                            //{
                            //    Msg = "Kindly run CPI first and then try again";
                            //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //}

                            string EmpCode = null;
                            List<String> ActivityNameCodeList = new List<String>();
                            int SalHead = 0;
                            //if (SalHeadlist != null && SalHeadlist != "")
                            //{
                            //    ActivityNameCodeList = Utility.StringIdsToListString(SalHeadlist);
                            //}
                            //foreach (var item in ActivityNameCodeList)
                            //{
                            var aa = db.SalaryHead.Where(e => e.Code == SalHeadlist).Select(e => e.Id).FirstOrDefault();
                            if (aa != 0)
                            {
                                SalHead = aa;
                            }
                            //}

                            //if (ids != null)
                            //{
                            //    foreach (var i in ids)
                            //    {
                            //        Employee OEmployee = null;
                            //        EmployeePayroll OEmployeePayroll = null;
                            //        PayScaleAgreement OPayScalAgreement = null;

                            //        OEmployeePayroll = db.EmployeePayroll
                            //                       .Include(q => q.Employee)
                            //                       .Include(q => q.Employee.ServiceBookDates)
                            //                       .Include(e => e.IncrementServiceBook)
                            //                       .Include(e => e.IncrementServiceBook.Select(r => r.IncrActivity))
                            //                       .Where(e => e.Employee.Id == i).AsNoTracking().AsParallel().SingleOrDefault();

                            //        OPayScalAgreement = db.PayScaleAgreement.FirstOrDefault();
                            //    }
                            //}

                            if (EmpCode != null)
                            {
                                return Json(new { data = rowno, success = false, responseText = "IncrementServiceBook already exists for employee " + EmpCode + "." }, JsonRequestBehavior.AllowGet);
                            }
                            int Comp_Id = int.Parse(Session["CompId"].ToString());
                            var ComPanyLeave_Id = db.CompanyLeave.Where(e => e.Company.Id == Comp_Id).SingleOrDefault();
                            if (!string.IsNullOrEmpty(sEmpCode))
                            {
                                foreach (var i in ids)
                                {

                                    EmployeePayroll OEmployeePayroll = null;
                                    PayScaleAgreement OPayScalAgreement = db.PayScaleAgreement.FirstOrDefault();
                                    string paymonth = date.ToString("MM/yyyy");
                                    OEmployeePayroll = db.EmployeePayroll.Where(e => e.Employee_Id == i).FirstOrDefault();

                                    var check = db.CPIEntryT.Where(e => e.PayMonth == paymonth && e.EmployeePayroll_Id == OEmployeePayroll.Id).SingleOrDefault();

                                    if (check == null)
                                    {
                                        return Json(new { data = rowno, success = false, responseText = "Cpi entry not exists for employee " + sEmpCode + "." }, JsonRequestBehavior.AllowGet);

                                    }




                                    EmployeeLeave OEmployeeLeave = db.EmployeeLeave.Where(e => e.Employee_Id == i).FirstOrDefault();

                                    EmpSalStruct OEmpSalStruct = db.EmpSalStruct.Include(e => e.EmpSalStructDetails).Include(e => e.EmpSalStructDetails.Select(t => t.SalHeadFormula))
                                        .Include(e => e.EmpSalStructDetails.Select(t => t.SalaryHead)).Include(e => e.EmpSalStructDetails.Select(t => t.SalaryHead.SalHeadOperationType))
                                        .Where(e => e.EmployeePayroll_Id == OEmployeePayroll.Id && e.EndDate == null).FirstOrDefault();

                                    List<EmpSalStructDetails> OEmpsalstructDetList = new List<EmpSalStructDetails>();

                                    using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                         new System.TimeSpan(0, 30, 0)))
                                    {
                                        if (OEmpSalStruct != null && OEmpSalStruct.EmpSalStructDetails.Count() > 0 && (OEmpSalStruct.EffectiveDate.Value.ToString("MM/yyyy") == paymonth))
                                        {
                                            if (OEmpSalStruct.EmpSalStructDetails.Any(e => e.SalaryHead_Id == SalHead))
                                            {
                                                EmpSalStructDetails OEmpSalStructDet = db.EmpSalStructDetails.Where(e => e.SalaryHead_Id == SalHead && e.EmpSalStruct_Id == OEmpSalStruct.Id).FirstOrDefault();
                                                OEmpSalStructDet.Amount = Convert.ToDouble(Amount);
                                                db.EmpSalStructDetails.Attach(OEmpSalStructDet);
                                                db.Entry(OEmpSalStructDet).State = System.Data.Entity.EntityState.Modified;
                                                db.SaveChanges();
                                            }
                                            else
                                            {
                                                PayScaleAssignment OScaleAssign = db.PayScaleAssignment.Where(e => e.PayScaleAgreement_Id == OPayScalAgreement.Id && e.SalaryHead_Id == SalHead).FirstOrDefault();
                                                if (OScaleAssign != null)
                                                {
                                                    EmpSalStructDetails OEmpSalStructDetailsNEw = new EmpSalStructDetails()
                                                    {
                                                        Amount = Convert.ToDouble(Amount),
                                                        DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false },
                                                        PayScaleAssignment = OScaleAssign == null ? null : db.PayScaleAssignment.Where(e => e.Id == OScaleAssign.Id).FirstOrDefault(),
                                                        SalaryHead = db.SalaryHead.Include(e => e.SalHeadOperationType).Include(e => e.RoundingMethod).Where(e => e.Id == SalHead).SingleOrDefault(),// .Where(e => e.Id == ca.SalaryHead.Id).SingleOrDefault(),
                                                        SalHeadFormula = null,

                                                    };
                                                    OEmpsalstructDetList.Add(OEmpSalStructDetailsNEw);
                                                    OEmpsalstructDetList.AddRange(OEmpSalStruct.EmpSalStructDetails);
                                                    OEmpSalStruct.EmpSalStructDetails = OEmpsalstructDetList;
                                                    db.EmpSalStruct.Attach(OEmpSalStruct);
                                                    db.Entry(OEmpSalStruct).State = System.Data.Entity.EntityState.Modified;
                                                    db.SaveChanges();
                                                }

                                            }
                                        }
                                        else
                                        {

                                            var OEmpSalStruct1 = db.EmpSalStruct.Where(e => e.EmployeePayroll_Id == OEmployeePayroll.Id).ToList();
                                            // if structure any record avilable then should create structure as per disscuss with sunil 
                                            // var salaryt = db.SalaryT.Where(e => e.EmployeePayroll_Id == OEmployeePayroll.Id && e.PayMonth == paymonth).SingleOrDefault();
                                            if (OEmpSalStruct1 == null && OEmpSalStruct1.Count() == 0)
                                            {

                                                SalaryHeadGenProcess.EmployeeSalaryStructCreationTest(OEmployeePayroll, i, OPayScalAgreement.Id, date, Comp_Id);
                                                ServiceBook.EmployeeLTCStructCreationTest(OEmployeePayroll, i, OPayScalAgreement.Id, date, Comp_Id);
                                                ServiceBook.EmployeePolicyStructCreationTest(OEmployeePayroll, i, OPayScalAgreement.Id, date, Comp_Id);
                                                LeaveStructureProcess.EmployeeLeaveStructCreationTest(OEmployeeLeave, i, OPayScalAgreement.Id, date, ComPanyLeave_Id.Id);
                                            }
                                        }
                                        ts.Complete();
                                        // return Json(new Object[] { "", "", "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
                                        //    List<string> Msgs = new List<string>();
                                    }

                                }
                                return Json(new { data = rowno, success = true, responseText = "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                return Json(new { data = rowno, success = false, responseText = "Select Employee" }, JsonRequestBehavior.AllowGet);
                            }
                        }
                    default:
                        return Json(new { data = rowno, success = false, responseText = "case not found" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogFile Logfile = new LogFile();
                ErrorLog Err = new ErrorLog()
                {
                    ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                    ExceptionMessage = ex.Message,
                    ExceptionStackTrace = ex.StackTrace,
                    LineNo = Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                    LogTime = DateTime.Now
                };
                Logfile.CreateLogFile(Err);
                return Json(new { data = rowno, success = false, responseText = ex.Message.ToString() }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
