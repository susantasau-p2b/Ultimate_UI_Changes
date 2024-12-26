using ExcelDataReader;
using Newtonsoft.Json;
using P2b.Global;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using P2BUltimate.Security;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Reflection;
using Payroll;
using System.Diagnostics;
using Leave;
using System.ComponentModel.DataAnnotations;
using Appraisal;
using Training;
using Attendance;
using P2BUltimate.Process;
using System.Web.Services;

namespace P2BUltimate.Controllers
{
    public class LoanInterfaceController : Controller
    {

        public ActionResult hidebutton()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var comp = db.Company.SingleOrDefault();
                if (comp.Code.ToUpper() == "PDCC")
                {
                    List<string> mylist = new List<string>(new string[] { "CheckNewRequest", "Save", "Empdiv", "Loandatadiv" });
                    return Json(mylist, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return null;
                }

            }

        }

        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/LoanInterface/index.cshtml");
            //E:\Ultimate\New folder\Development Source New\Payroll-Leave-Apprisal-Attendance-Traning-Recuritment\MainSource\P2bUltimate\P2BUltimate\Views\Payroll\MainViews\LoanInterface\index.cshtml
        }

        public ActionResult GetEmpCode(int EmpId)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                string EmpCode = db.Employee.Where(e => e.Id == EmpId).FirstOrDefault().EmpCode;

                return Json(EmpCode, JsonRequestBehavior.AllowGet);

            }
        }

        public class HomeController : Controller
        {
            DataBaseContext db = new DataBaseContext();
            public ActionResult Index(string Searchby, string search)
            {
                if (Searchby == "AccountNo")
                {
                    var model = db.Employee.Where(emp => emp.EmpCode == search || search == null).ToList();
                    return View(model);

                }
                else
                {
                    var model = db.Employee.Where(emp => emp.EmpCode.StartsWith(search) || search == null).ToList();
                    return View(model);
                }
            }
        }

        public ActionResult Partial()
        {
            return View("~/Views/Shared/Payroll/_LoanInterface.cshtml");

        }

        public class new_loan_req
        {
            public string EmpCode { get; set; }
            public string BranchCodeL { get; set; }

            public string ProductL { get; set; }

            public string AccountNoL { get; set; }

            public double InstallmentAmountL { get; set; }

            public double PrincipalOutStandL { get; set; }
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
                        PropList.RemoveAll(a => a == "FuncStruct");
                        PropList.RemoveAll(a => a == "GeoStruct");
                        PropList.RemoveAll(a => a == "IsHold");
                        PropList.RemoveAll(a => a == "PayStruct");
                        PropList.Add("EmpCode");
                        PropList.RemoveAll(a => a == "Narration");
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
            if (PaymonthforLoanadv == null)
            {
                return Json(new { responseText = "Select Paymonth..!" }, JsonRequestBehavior.AllowGet);
            }

            using (DataBaseContext db = new DataBaseContext())
            {
                var Id = Convert.ToInt32(SessionManager.CompanyId);
                string _CompCode = db.Company.Where(e => e.Id == Id).SingleOrDefault().Code.ToUpper();
                var SalExprocess = db.SalaryT.Where(e => e.PayMonth == PaymonthforLoanadv && e.ReleaseDate == null).ToList();
                if (SalExprocess.Count() > 0)
                {
                    return Json(new { responseText = "Salary is processed for this month..!" }, JsonRequestBehavior.AllowGet);
                }
                var SalEx = db.SalaryT.Where(e => e.PayMonth == PaymonthforLoanadv && e.ReleaseDate != null).ToList();
                if (SalEx.Count() > 0)
                {
                    return Json(new { responseText = "Salary is released for this month..!" }, JsonRequestBehavior.AllowGet);
                }
                string requiredPaths = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\App_Data\\Menu_Json";
                bool existss = System.IO.Directory.Exists(requiredPaths);
                string localPaths;
                if (!existss)
                {
                    localPaths = new Uri(requiredPaths).LocalPath;
                    System.IO.Directory.CreateDirectory(localPaths);
                }
                string paths = requiredPaths + @"\LoanInterfacePerkCalc" + ".ini";
                localPaths = new Uri(paths).LocalPath;
                if (!System.IO.File.Exists(localPaths))
                {

                    using (var fs = new FileStream(localPaths, FileMode.OpenOrCreate))
                    {
                        StreamWriter str = new StreamWriter(fs);
                        str.BaseStream.Seek(0, SeekOrigin.Begin);

                        str.Flush();
                        str.Close();
                        fs.Close();
                    }


                }

                string FileCompCode = "";
                string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\App_Data\\Menu_Json";
                bool exists = System.IO.Directory.Exists(requiredPath);
                string localPath;
                if (!exists)
                {
                    localPath = new Uri(requiredPath).LocalPath;
                    System.IO.Directory.CreateDirectory(localPath);
                }
                string path = requiredPath + @"\LoanInterfacePerkCalc" + ".ini";
                localPath = new Uri(path).LocalPath;
                using (var streamReader = new StreamReader(localPath))
                {
                    string line;

                    while ((line = streamReader.ReadLine()) != null)
                    {
                        var comp = line;
                        FileCompCode = comp;
                    }
                }

                if (_CompCode == FileCompCode)
                {
                    string co = UploadPDCC(upload, PaymonthforLoanadv);
                    string Dismsg = "";
                    if (co == "2")
                        Dismsg = "Data Processed Successfully..!";
                    else if (co == "0")
                        Dismsg = "Column Shouldn't be null..!";
                    else if (co == "1")
                        Dismsg = "Loan Request Not Found..!";
                    else if (co == "3")
                        Dismsg = "Please define Loan advance policy..!";
                    else if (co == "4")
                        Dismsg = "Updated bank interest is not defined in Loan advance policy..!";
                    else
                        Dismsg = co;
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Dismsg }, JsonRequestBehavior.AllowGet);

                }
            }
            if (upload != null && upload.ContentLength > 0)
            {
                Stream stream = upload.InputStream;
                Assembly oAssembly;
                Type type;
                var filename = upload.FileName.Split('.')[0];
                string fileExtension = upload.FileName.Split('.')[1];

                if (filename != null && Type != null && ((upload.FileName.EndsWith(".xls")) || (upload.FileName.EndsWith(".xlsx"))))
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

                    // Excel file  after convert in notepad
                    Type type1;

                    if (filename == "OtherEarningT")
                    {
                        filename = "OthEarningDeductionT";
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
                        List<new_loan_req> oNewLoanReqList = new List<new_loan_req>();

                        //ExcelformatEntity exc = new ExcelformatEntity()
                        //{
                        //    Column0 = "LoanAccBranch",
                        //    Column1 = "LoanAdvHead",
                        //    Column2 = "LoanAccountNo",
                        //    Column3 = "MonthlyIntrest",
                        //    Column4 = "MonthlyInstallmentAmount",
                        //    Column5 = "MonthlyPrincipleAmt",
                        //    Column6 = "EnforcementDate",
                        //    Column7 = "EmpCode",
                        //    Column8 = "LoanSanctionAmount",
                        //    Column9 = "IsActive",
                        //};
                        //excList.Add(exc);
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

                                var PayScaleAssignmentData = db.PayScaleAssignment
                                     .Include(q => q.SalaryHead)
                                     .Include(q => q.SalHeadFormula)
                                     .Include(q => q.SalHeadFormula.Select(a => a.SalWages))
                                     .Include(q => q.SalHeadFormula.Select(a => a.SalWages.RateMaster))
                                     .Include(q => q.SalHeadFormula.Select(a => a.SalWages.RateMaster.Select(lm => lm.SalHead)))
                                     .Where(r => r.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "PERK")
                                   .Select(z => z.SalaryHead.Code)
                                       .ToList();

                                // salhead = PayScaleAssignmentData.Select(z => z.SalaryHead.Code).ToList();

                                //RateMasterSalaryHead = PayScaleAssignmentData
                                //   .SelectMany(z => z.SalHeadFormula)
                                //   .SelectMany(c => c.SalWages.RateMaster.Select(lm => lm.SalHead.Code)).Distinct()
                                //    .ToList();
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
                                    double Principleoutstnd = 0;

                                    if (Loc_Code == "")
                                    {
                                        return Json(new { responseText = "Invalid Loan Code!!!" }, JsonRequestBehavior.AllowGet);
                                    }
                                    if (Loan_head == "")
                                    {
                                        return Json(new { responseText = "Invalid Loan Head!!!" }, JsonRequestBehavior.AllowGet);
                                    }
                                    if (Acc_No == "")
                                    {
                                        return Json(new { responseText = "Invalid Account No!!!" }, JsonRequestBehavior.AllowGet);
                                    }

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

                                        ChkData = line.Substring(182, 18).Trim();
                                        Principleoutstnd = ChkData == "" ? 0 : Convert.ToDouble(ChkData);
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

                                        ChkData = line.Substring(181, 18).Trim();
                                        Principleoutstnd = ChkData == "" ? 0 : Convert.ToDouble(ChkData);
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
                                    if (Type == "Proc")
                                    {
                                        int checkifpresent = LoanAdvReqRep(Loc_Code.Trim(), Loan_head.Trim(), MonthlyIntrest, Monthly_principleAmt, InstallmentPaid, PaymonthforLoanadv, Acc_No, rowno,
                                            ActualPerkAmt, ProjPerkAmt, BankIntrest, SIBIntrest, salheadList, AnneigtyCPriPro, AnneigtyCPriAct, AnnintdedPro, AnnintdedAct); //MonthlyPrinciple, MonthlyInterest,
                                    }
                                    else
                                    {
                                        new_loan_req OnewLoan = new new_loan_req();
                                        OnewLoan = LoanAdvReqRepcheck(Loc_Code.Trim(), Loan_head.Trim(), MonthlyIntrest, Monthly_principleAmt, InstallmentPaid, PaymonthforLoanadv, Acc_No, rowno,
                                                 ActualPerkAmt, ProjPerkAmt, BankIntrest, SIBIntrest, salheadList, AnneigtyCPriPro, AnneigtyCPriAct, AnnintdedPro, AnnintdedAct, Principleoutstnd); //MonthlyPrinciple, MonthlyInterest,

                                        oldlNcode = Loan_head.Trim();

                                        counter++;
                                        if (OnewLoan != null)
                                        {
                                            oNewLoanReqList.Add(OnewLoan);
                                        }
                                    }
                                    //}
                                }
                            }
                        }
                        if (Type == "Proc")
                        {
                            return Json(new Utility.JsonReturnClass { success = true, responseText = "Data processed successfully." }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json(new { oNewLoanReqList }, JsonRequestBehavior.AllowGet);
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
                //if (type == null)
                //{
                //    return Json(new { responseText = "Invalid excel..!" }, JsonRequestBehavior.AllowGet);
                //}
                //Object instance = Activator.CreateInstance(type);
                //var Prop = instance.GetType().GetProperties().ToList();
                //List<string> PropList = new List<string>();
                //foreach (var item in Prop)
                //{
                //    if (item.Name != "DBTrack" && item.Name != "Id" && item.Name != "RowVersion")
                //    {
                //        PropList.Add(item.Name);
                //    }
                //}

                //var ListCol = AddtionalCol(filename, fileExtension, PropList);
                //ListCol.Sort();
                //ExcelDataReader.IExcelDataReader reader = null;


                if (filename != null && Type != null && ((upload.FileName.EndsWith(".txt") || (upload.FileName.EndsWith(".TXT")))))
                {

                    try
                    {
                        List<DataDump> testing = new List<DataDump>();
                        List<ExcelformatEntity> excList = new List<ExcelformatEntity>();
                        List<new_loan_req> oNewLoanReqList = new List<new_loan_req>();

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

                                // salhead = PayScaleAssignmentData.Select(z => z.SalaryHead.Code).ToList();

                                //RateMasterSalaryHead = PayScaleAssignmentData
                                //   .SelectMany(z => z.SalHeadFormula)
                                //   .SelectMany(c => c.SalWages.RateMaster.Select(lm => lm.SalHead.Code)).Distinct()
                                //    .ToList();
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
                                Boolean LNcode = false;
                                string oldlNcode = "0";

                                while ((line = streamReader.ReadLine()) != null)
                                {

                                    //do not delete
                                    #region test

                                    #endregion test
                                    //do not delete
                                    int counter = 0;
                                    // line.streamReader.trim();

                                    if (line.Trim() != "" && line.Trim() != null)
                                    {

                                        string Loc_Code = line.Substring(Convert.ToInt16(testing[0].PositionNo) - 1, Convert.ToInt16(testing[0].PositionNoLength)).Trim();
                                        string Loan_head = line.Substring(Convert.ToInt16(testing[1].PositionNo) - 1, Convert.ToInt16(testing[1].PositionNoLength)).Trim();
                                        string Acc_No = line.Substring(Convert.ToInt16(testing[2].PositionNo) - 1, Convert.ToInt16(testing[2].PositionNoLength)).Trim();

                                        string ChkData = line.Substring(Convert.ToInt16(testing[5].PositionNo) - 1, Convert.ToInt16(testing[5].PositionNoLength)).Trim();
                                        double MonthlyIntrest = ChkData == "" ? 0 : Convert.ToDouble(ChkData);

                                        ChkData = line.Substring(Convert.ToInt16(testing[12].PositionNo) - 1, Convert.ToInt16(testing[12].PositionNoLength)).Trim();
                                        double InstallmentPaid = ChkData == "" ? 0 : Convert.ToDouble(ChkData);

                                        //ChkData = line.Substring(Convert.ToInt16(testing[10].PositionNo) - 1, Convert.ToInt16(testing[10].PositionNoLength)).Trim();
                                        //double Total_LoanSanct_appliedamt = ChkData == "" ? 0 : Convert.ToDouble(ChkData);

                                        ChkData = line.Substring(Convert.ToInt16(testing[13].PositionNo) - 1, Convert.ToInt16(testing[13].PositionNoLength)).Trim();
                                        double Monthly_principleAmt = ChkData == "" ? 0 : Convert.ToDouble(ChkData);


                                        //ChkData = line.Substring(112, 18).Trim();
                                        ChkData = line.Substring(Convert.ToInt16(testing[7].PositionNo) - 1, Convert.ToInt16(testing[7].PositionNoLength)).Trim();
                                        double ActualPerkAmt = ChkData == "" ? 0 : Convert.ToDouble(ChkData);

                                        // ChkData = line.Substring(94, 18).Trim();
                                        ChkData = line.Substring(Convert.ToInt16(testing[6].PositionNo) - 1, Convert.ToInt16(testing[6].PositionNoLength)).Trim();
                                        double ProjPerkAmt = ChkData == "" ? 0 : Convert.ToDouble(ChkData);

                                        //ChkData = line.Substring(147, 7).Trim();
                                        ChkData = line.Substring(Convert.ToInt16(testing[10].PositionNo) - 1, Convert.ToInt16(testing[10].PositionNoLength)).Trim();
                                        double BankIntrest = ChkData == "" ? 0 : Convert.ToDouble(ChkData);

                                        // ChkData = line.Substring(154, 7).Trim();
                                        ChkData = line.Substring(Convert.ToInt16(testing[11].PositionNo) - 1, Convert.ToInt16(testing[11].PositionNoLength)).Trim();
                                        double SIBIntrest = ChkData == "" ? 0 : Convert.ToDouble(ChkData);

                                        //   ChkData = line.Substring(217, 18).Trim();
                                        //ChkData = line.Substring(Convert.ToInt16(testing[15].PositionNo) - 1, Convert.ToInt16(testing[15].PositionNoLength)).Trim();
                                        //double MonthlyPrinciple = ChkData == "" ? 0 : Convert.ToDouble(ChkData);

                                        // ChkData = line.Substring(235, 18).Trim();
                                        //ChkData = line.Substring(Convert.ToInt16(testing[16].PositionNo) - 1, Convert.ToInt16(testing[16].PositionNoLength)).Trim();
                                        //double MonthlyInterest = ChkData == "" ? 0 : Convert.ToDouble(ChkData);


                                        //  ChkData = line.Substring(24, 18).Trim();
                                        ChkData = line.Substring(Convert.ToInt16(testing[3].PositionNo) - 1, Convert.ToInt16(testing[3].PositionNoLength)).Trim();
                                        double AnneigtyCPriPro = ChkData == "" ? 0 : Convert.ToDouble(ChkData);

                                        // ChkData = line.Substring(42, 18).Trim();
                                        ChkData = line.Substring(Convert.ToInt16(testing[4].PositionNo) - 1, Convert.ToInt16(testing[4].PositionNoLength)).Trim();
                                        double AnneigtyCPriAct = ChkData == "" ? 0 : Convert.ToDouble(ChkData);

                                        // ChkData = line.Substring(60, 18).Trim();
                                        ChkData = line.Substring(Convert.ToInt16(testing[5].PositionNo) - 1, Convert.ToInt16(testing[5].PositionNoLength)).Trim();
                                        double AnnintdedPro = ChkData == "" ? 0 : Convert.ToDouble(ChkData);

                                        // ChkData = line.Substring(78, 18).Trim();
                                        ChkData = line.Substring(Convert.ToInt16(testing[6].PositionNo) - 1, Convert.ToInt16(testing[6].PositionNoLength)).Trim();
                                        double AnnintdedAct = ChkData == "" ? 0 : Convert.ToDouble(ChkData);

                                        // ChkData = line.Substring(180 - 1, 18).Trim();
                                        ChkData = line.Substring(Convert.ToInt16(testing[13].PositionNo) - 1, Convert.ToInt16(testing[13].PositionNoLength)).Trim();
                                        double Principleoutstnd = ChkData == "" ? 0 : Convert.ToDouble(ChkData);


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
                                        if (Type == "Proc")
                                        {
                                            //int checkifpresent = LoanAdvReqRep(Loc_Code.Trim(), Loan_head.Trim(), MonthlyIntrest, Monthly_principleAmt, InstallmentPaid, PaymonthforLoanadv, Acc_No, rowno,
                                            //    ActualPerkAmt, ProjPerkAmt, BankIntrest, SIBIntrest, salheadList, MonthlyPrinciple, MonthlyInterest, AnneigtyCPriPro, AnneigtyCPriAct, AnnintdedPro, AnnintdedAct);
                                            int checkifpresent = LoanAdvReqRep(Loc_Code.Trim(), Loan_head.Trim(), MonthlyIntrest, Monthly_principleAmt, InstallmentPaid, PaymonthforLoanadv, Acc_No, rowno,
                                               ActualPerkAmt, ProjPerkAmt, BankIntrest, SIBIntrest, salheadList, AnneigtyCPriPro, AnneigtyCPriAct, AnnintdedPro, AnnintdedAct);

                                        }
                                        else
                                        {
                                            new_loan_req OnewLoan = new new_loan_req();
                                            //OnewLoan = LoanAdvReqRepcheck(Loc_Code.Trim(), Loan_head.Trim(), MonthlyIntrest, Monthly_principleAmt, InstallmentPaid, PaymonthforLoanadv, Acc_No, rowno,
                                            //         ActualPerkAmt, ProjPerkAmt, BankIntrest, SIBIntrest, salheadList, MonthlyPrinciple, MonthlyInterest, AnneigtyCPriPro, AnneigtyCPriAct, AnnintdedPro, AnnintdedAct, Principleoutstnd);

                                            OnewLoan = LoanAdvReqRepcheck(Loc_Code.Trim(), Loan_head.Trim(), MonthlyIntrest, Monthly_principleAmt, InstallmentPaid, PaymonthforLoanadv, Acc_No, rowno,
                                                    ActualPerkAmt, ProjPerkAmt, BankIntrest, SIBIntrest, salheadList, AnneigtyCPriPro, AnneigtyCPriAct, AnnintdedPro, AnnintdedAct, Principleoutstnd);
                                            oldlNcode = Loan_head.Trim();

                                            counter++;
                                            if (OnewLoan != null)
                                            {
                                                oNewLoanReqList.Add(OnewLoan);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if (Type == "Proc")
                        {
                            return Json(new Utility.JsonReturnClass { success = true, responseText = "Data processed successfully." }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        { return Json(new { oNewLoanReqList }, JsonRequestBehavior.AllowGet); }

                    }
                    catch (Exception ex)
                    {

                        // throw;
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
                }
                else
                {
                    return Json(new { responseText = "Invalid Excel..!" }, JsonRequestBehavior.AllowGet);
                }
                //DataSet result = reader.AsDataSet();
                //reader.Close();
                //var json = JsonConvert.SerializeObject(result.Tables[0], Formatting.Indented);

                //return Json(new { json = json, col = ListCol }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { responseText = "Empty Excel..!" }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost]
        public ActionResult CreateLoan(List<new_loan_req> data, string PayMonthF)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> MSG = new List<string>();

                try
                {
                    LoanAdvRequest LoanAdvReq = new LoanAdvRequest();
                    foreach (new_loan_req item in data)
                    {
                        if (item.ProductL != null && item.ProductL != "")
                        {
                            LoanAdvanceHead LoanAdvanceHead = db.LoanAdvanceHead.Include(e => e.PerkHead).Where(a => a.Code == item.ProductL).FirstOrDefault();
                            LoanAdvReq.LoanAdvanceHead = LoanAdvanceHead;
                        }
                        if (item.BranchCodeL != null && item.BranchCodeL != "")
                        {
                            var val = db.Location.Include(e => e.LocationObj).Where(e => e.LocationObj.LocCode == item.BranchCodeL).FirstOrDefault();
                            LoanAdvReq.LoanAccBranch = val;
                        }
                        using (TransactionScope ts = new TransactionScope())
                        {
                            LoanAdvReq.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            LoanAdvReq.TotalInstall = Convert.ToInt32(item.PrincipalOutStandL / item.InstallmentAmountL);
                            LoanAdvRequest LoanAdvRequest = new LoanAdvRequest()
                            {
                                CloserDate = null,
                                EnforcementDate = Convert.ToDateTime("01/" + PayMonthF),
                                IsActive = true,
                                IsInstallment = false,
                                LoanAccBranch = LoanAdvReq.LoanAccBranch,
                                LoanAccNo = item.AccountNoL,
                                LoanAdvanceHead = LoanAdvReq.LoanAdvanceHead,
                                LoanAppliedAmount = item.PrincipalOutStandL,
                                LoanSanctionedAmount = item.PrincipalOutStandL,
                                LoanSubAccNo = "",
                                MonthlyInstallmentAmount = item.InstallmentAmountL,
                                MonthlyInterest = 0,
                                MonthlyPricipalAmount = item.PrincipalOutStandL,
                                Narration = "Loan Interface",
                                RequisitionDate = DateTime.Now,
                                SanctionedDate = DateTime.Now,
                                TotalInstall = LoanAdvReq.TotalInstall,
                                DBTrack = LoanAdvReq.DBTrack
                            };
                            try
                            {


                                List<LoanAdvRepaymentT> LoanAdvRepaymentTDet = new List<LoanAdvRepaymentT>();

                                if (LoanAdvRequest.EnforcementDate != null && LoanAdvReq.TotalInstall != 0)
                                {

                                    for (int i = 0; i <= LoanAdvReq.TotalInstall - 1; i++)
                                    {
                                        double TotalLoanPaid = 0;

                                        string Month = LoanAdvRequest.EnforcementDate.Value.AddMonths(i).Month.ToString().Length == 1 ? "0" + LoanAdvRequest.EnforcementDate.Value.AddMonths(i).Month.ToString() : LoanAdvRequest.EnforcementDate.Value.AddMonths(i).Month.ToString();
                                        string PayMonth = Month + "/" + LoanAdvRequest.EnforcementDate.Value.AddMonths(i).Year;
                                        LoanAdvRepaymentT LoanAdvRepayT = new LoanAdvRepaymentT()
                                        {
                                            InstallementDate = LoanAdvRequest.EnforcementDate.Value.AddMonths(i),
                                            InstallmentAmount = LoanAdvReq.MonthlyInstallmentAmount,
                                            InstallmentCount = i + 1,
                                            InstallmentPaid = LoanAdvReq.MonthlyPricipalAmount + LoanAdvReq.MonthlyInterest,
                                            PayMonth = PayMonth,
                                            TotalLoanBalance = LoanAdvRequest.LoanSanctionedAmount,//LoanAdvRequest.LoanSanctionedAmount - installment_Paid,
                                            TotalLoanPaid = 0,//installment_Paid
                                            DBTrack = LoanAdvReq.DBTrack,
                                            MonthlyInterest = LoanAdvReq.MonthlyInterest,
                                            MonthlyPricipalAmount = LoanAdvReq.MonthlyPricipalAmount
                                        };
                                        TotalLoanPaid = TotalLoanPaid + LoanAdvRepayT.InstallmentPaid;
                                        LoanAdvRepaymentTDet.Add(LoanAdvRepayT);
                                    }

                                }
                                LoanAdvRequest.LoanAdvRepaymentT = LoanAdvRepaymentTDet;
                                db.LoanAdvRequest.Add(LoanAdvRequest);
                                db.SaveChanges();

                                var OEmployeePayroll = db.EmployeePayroll.Include(e => e.Employee).Where(e => e.Employee.EmpCode == item.EmpCode).SingleOrDefault();

                                List<LoanAdvRequest> LoanAdvReqList = new List<LoanAdvRequest>();
                                LoanAdvReqList.Add(LoanAdvRequest);
                                OEmployeePayroll.LoanAdvRequest = LoanAdvReqList;
                                db.EmployeePayroll.Attach(OEmployeePayroll);
                                db.Entry(OEmployeePayroll).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(OEmployeePayroll).State = System.Data.Entity.EntityState.Detached;

                                ts.Complete();

                                // return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                                List<string> Msgs = new List<string>();
                                Msgs.Add("Data Saved successfully");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msgs }, JsonRequestBehavior.AllowGet);
                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = LoanAdvReq.Id });
                            }
                            catch (DataException /* dex */)
                            {
                                return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
                            }
                        }
                    }

                    return Json(MSG, JsonRequestBehavior.AllowGet);

                }
                catch (Exception e)
                {
                    MSG.Add(e.InnerException.Message.ToString());
                    return Json(MSG, JsonRequestBehavior.AllowGet);

                }

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
           string LLoanAccNo, string rowno, Double ActualPerkAmt, Double ProjPerkAmt, Double BankIntrest, Double SBIIntrest, List<SalHeadFormula> salheadList, Double AnneigtyCPriPro, Double AnneigtyCPriAct, Double AnnintdedPro, Double AnnintdedAct) //Double MonthlyPrinciple, Double MonthlyInterest,
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
                            //LoanAdvRepaymentT_temp.MonthlyInterest = Convert.ToDouble(MonthlyInterest);
                            //LoanAdvRepaymentT_temp.MonthlyPricipalAmount = Convert.ToDouble(MonthlyPrinciple);
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

                            var sddd = LoanHeadId.PerkHead != null ? LoanHeadId.PerkHead.Id : 0;
                            if (sddd != null && sddd != 0)
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
                                        //ActualAmt = ((MonthlyInterest / BankIntrest) * SBIIntrest - MonthlyInterest);
                                        //ProjectedAmt = ((MonthlyInterest / BankIntrest) * SBIIntrest - MonthlyInterest);


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


        public new_loan_req LoanAdvReqRepcheck(string LocCode, string LoanAdvHead, Double LMonthlyInterest, Double LMonthlyPricipalAmount, Double LInstallmentPaid, string LoanPayMonth,
           string LLoanAccNo, string rowno, Double ActualPerkAmt, Double ProjPerkAmt, Double BankIntrest, Double SBIIntrest, List<SalHeadFormula> salheadList, Double AnneigtyCPriPro, Double AnneigtyCPriAct, Double AnnintdedPro, Double AnnintdedAct, double Principleoutstnd) // Double MonthlyPrinciple, Double MonthlyInterest,
        {
            try
            {

                new_loan_req ORetClass = new new_loan_req();
                if (LoanAdvHead == null || LoanPayMonth == null || LMonthlyInterest == null || LMonthlyPricipalAmount == null || LInstallmentPaid == null)
                {
                    return null;
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
                            ORetClass = new new_loan_req()
                            {
                                BranchCodeL = LocCode,
                                ProductL = LoanAdvHead,
                                AccountNoL = LLoanAccNo,
                                InstallmentAmountL = LInstallmentPaid,
                                PrincipalOutStandL = Principleoutstnd

                            };
                            return ORetClass;
                            //Json(new { data = rowno, success = false, responseCode = 1, responseText = "Loan Request Not Found..!" }, JsonRequestBehavior.AllowGet);
                        }


                    }
                    return null;
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public ActionResult ChkProcess(string typeofbtn, string month)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                bool selected = false;
                List<SalaryT> query = db.EmployeePayroll.SelectMany(r => r.SalaryT).ToList();
                var a = query.Where(t => t.PayMonth == month).ToList();
                if (a.Count > 0)
                {
                    selected = true;
                }
                var data = new
                {
                    status = selected,
                };
                return Json(data, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Polulate_AmountChk(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (!String.IsNullOrEmpty(Session["CompId"].ToString()))
                {
                    List<string> a = new List<string>();
                    if (data.ToString().Split('/')[0] == "03")
                    {
                        a.Add("Actual");
                    }
                    else
                    {
                        a.Add("Actual");
                        a.Add("Declared");
                    }

                    SelectList s = new SelectList(a);
                    return Json(s, JsonRequestBehavior.AllowGet);
                }
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Polulate_PayProcessGroup(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (!String.IsNullOrEmpty(Session["CompId"].ToString()))
                {
                    int CompId = int.Parse(Session["CompId"].ToString());
                    var query = db.Company.Include(e => e.PayProcessGroup).Where(e => e.Id == CompId).SingleOrDefault();
                    var selected = (Object)null;
                    selected = query.PayProcessGroup.Select(e => e.Id).FirstOrDefault();
                    if (data2 != "" && data != "0" && data2 != "0")
                    {
                        selected = Convert.ToInt32(data2);
                    }

                    SelectList s = new SelectList(query.PayProcessGroup, "Id", "FullDetails", selected);
                    return Json(s, JsonRequestBehavior.AllowGet);
                }
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult ByDefaultLoadEmp()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var comp_id = Session["CompId"].ToString();
                List<string> GeoStruct = new List<string>();
                List<string> PayStruct = new List<string>();
                List<string> FunStruct = new List<string>();
                if (comp_id != null)
                {
                    var id = Convert.ToInt32(comp_id);
                    var geo_id = db.GeoStruct.Where(e => e.Company != null && e.Company.Id == id).OrderBy(e => e.Id).AsNoTracking().Distinct().ToList();
                    if (geo_id.Count > 0)
                    {
                        foreach (var ca in geo_id.Select(e => e.Id.ToString()))
                        {
                            GeoStruct.Add(ca);
                        }
                    }

                    var pay_id = db.PayStruct.Where(e => e.Company != null && e.Company.Id == id).OrderBy(e => e.Id).AsNoTracking().Distinct().ToList();
                    if (pay_id.Count > 0)
                    {
                        foreach (var ca in pay_id.Select(e => e.Id.ToString()))
                        {
                            PayStruct.Add(ca);
                        }
                    }

                    var fun_id = db.FuncStruct.Where(e => e.Company != null && e.Company.Id == id).OrderBy(e => e.Id).AsNoTracking().Distinct().ToList();
                    if (fun_id.Count > 0)
                    {
                        foreach (var ca in fun_id.Select(e => e.Id.ToString()))
                        {
                            FunStruct.Add(ca);
                        }
                    }
                }
                else
                {
                    GeoStruct = null;
                    PayStruct = null;
                    FunStruct = null;
                }
                var jsondata = new
                {
                    GeoStruct = GeoStruct,
                    PayStruct = PayStruct,
                    FunStruct = FunStruct
                };
                return Json(jsondata, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult CheckSessionExitance()
        {
            if (String.IsNullOrEmpty(SessionManager.UserName))
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
        }
        public class P2BGridData
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
            public string PayMonth { get; set; }
            public double TotalEarning { get; set; }
            public double TotalDeduction { get; set; }
            public double TotalNet { get; set; }
            public string ReleaseDate { get; set; }
        }

        public ActionResult P2BGrid(P2BGrid_Parameters gp)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                    int pageSize = gp.rows;
                    int totalPages = 0;
                    int totalRecords = 0;
                    var jsonData = (Object)null;

                    IEnumerable<P2BGridData> SalaryList = null;
                    List<P2BGridData> model = new List<P2BGridData>();
                    P2BGridData view = null;
                    string PayMonth = "";
                    string Month = "";
                    if (gp.filter != null)
                        PayMonth = gp.filter;
                    else
                    {
                        if (DateTime.Now.Date.Month < 10)
                            Month = "0" + DateTime.Now.Date.Month;
                        else
                            Month = DateTime.Now.Date.Month.ToString();
                        PayMonth = Month + "/" + DateTime.Now.Date.Year;
                    }

                    //var BindEmpList = db.EmployeePayroll.Include(e => e.Employee.EmpName).Include(e => e.SalaryT)
                    //    .Where(e => e.SalaryT.Any(u => u.PayMonth != "01/2017")).ToList();

                    //var BindEmpList = db.EmployeePayroll.Include(e => e.Employee.EmpName).Include(e => e.SalaryT.Where(q => q.PayMonth == PayMonth)).AsNoTracking().AsParallel().ToList();
                    var BindEmpList = db.EmployeePayroll.Include(e => e.Employee.EmpName).Include(e => e.SalaryT).AsNoTracking().AsParallel().ToList();

                    foreach (var z in BindEmpList)
                    {
                        var chkk = z.SalaryT.Where(q => q.PayMonth == PayMonth).SingleOrDefault();
                        if (chkk != null)
                        {
                            //foreach (var Sal in z.SalaryT)
                            //{
                            //    if (Sal.PayMonth == PayMonth)
                            //    {
                            view = new P2BGridData()
                            {
                                Id = z.Employee.Id,
                                Code = z.Employee.EmpCode,
                                Name = z.Employee.EmpName.FullNameFML,
                                PayMonth = chkk.PayMonth,
                                TotalEarning = chkk.TotalEarning,
                                TotalDeduction = chkk.TotalDeduction,
                                TotalNet = chkk.TotalNet,
                                ReleaseDate = chkk.ReleaseDate == null ? "" : chkk.ReleaseDate.Value.ToShortDateString()
                            };
                            model.Add(view);
                            //    }
                            //}
                        }

                    }

                    SalaryList = model;

                    IEnumerable<P2BGridData> IE;
                    if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                    {
                        IE = SalaryList;
                        if (gp.searchOper.Equals("eq"))
                        {
                            jsonData = IE.Where(e => (e.Code.ToString().Contains(gp.searchString))
                                  || (e.Name.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                                  || (e.PayMonth.ToString().Contains(gp.searchString.ToUpper()))
                                  || (e.TotalEarning.ToString().Contains(gp.searchString))
                                  || (e.TotalDeduction.ToString().Contains(gp.searchString))
                                  || (e.TotalNet.ToString().Contains(gp.searchString))
                                  || (e.ReleaseDate.ToString().Contains(gp.searchString))
                                  || (e.Id.ToString().Contains(gp.searchString))
                                  )
                              .Select(a => new Object[] { a.Code, a.Name, a.PayMonth, a.TotalEarning, a.TotalDeduction, a.TotalNet, a.ReleaseDate, a.Id }).ToList();



                            //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Code, a.Name, a.PayMonth, a.TotalEarning, a.TotalDeduction, a.TotalNet, a.ReleaseDate, a.Id }).ToList();
                        }
                        totalRecords = IE.Count();
                    }
                    else
                    {
                        IE = SalaryList;
                        Func<P2BGridData, dynamic> orderfuc;
                        if (gp.sidx == "Id")
                        {
                            orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                        }
                        else
                        {
                            orderfuc = (c => gp.sidx == "EmpCode" ? c.Code.ToString() :
                                             gp.sidx == "Name" ? c.Name.ToString() :
                                             gp.sidx == "PayMonth" ? c.PayMonth.ToString() :
                                             gp.sidx == "TotalEarning" ? c.TotalEarning.ToString() :
                                             gp.sidx == "TotalDeduction" ? c.TotalDeduction.ToString() :
                                             gp.sidx == "TotalNet" ? c.TotalDeduction.ToString() :
                                             gp.sidx == "ReleaseDate" ? c.ReleaseDate.ToString() : "");
                        }
                        if (gp.sord == "asc")
                        {
                            IE = IE.OrderBy(orderfuc);
                            jsonData = IE.Select(a => new Object[] { a.Code, a.Name, a.PayMonth, a.TotalEarning, a.TotalDeduction, a.TotalNet, a.ReleaseDate, a.Id }).ToList();
                        }
                        else if (gp.sord == "desc")
                        {
                            IE = IE.OrderByDescending(orderfuc);
                            jsonData = IE.Select(a => new Object[] { a.Code, a.Name, a.PayMonth, a.TotalEarning, a.TotalDeduction, a.TotalNet, a.ReleaseDate, a.Id }).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Code, a.Name, a.PayMonth, a.TotalEarning, a.TotalDeduction, a.TotalNet, a.ReleaseDate, a.Id }).ToList();
                        }
                        totalRecords = SalaryList.Count();
                    }
                    if (totalRecords > 0)
                    {
                        totalPages = (int)Math.Ceiling((float)totalRecords / (float)gp.rows);
                    }
                    if (gp.page > totalPages)
                    {
                        gp.page = totalPages;
                    }
                    var JsonData = new
                    {
                        page = gp.page,
                        rows = jsonData,
                        records = totalRecords,
                        total = totalPages
                    };
                    return Json(JsonData, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }


        public string UploadPDCC(HttpPostedFileBase upload, string PayMonth)
        {
            string checkifpresent = "";
            if (upload != null && upload.ContentLength > 0)
            {
                Stream stream = upload.InputStream;

                var filename = upload.FileName.Split('.')[0];
                string fileExtension = upload.FileName.Split('.')[1];
                string LLoanAccNo = "";
                List<DataDump> testing = new List<DataDump>();
                using (DataBaseContext db = new DataBaseContext())
                {

                    testing = db.DataDump.Where(q => q.TableName == "LoanAdvRepaymentT").OrderBy(e => e.SeqNo).ToList();

                    var PayScaleAssignmentData = db.PayScaleAssignment
                                   .Include(q => q.SalaryHead)
                                    .Include(q => q.SalaryHead.SalHeadOperationType)
                                   .Where(r => r.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "PERK")
                                 .Select(z => z.SalaryHead.Code)
                                     .ToList();

                    List<PerkTransT> Perktranst = db.PerkTransT
                          .Include(q => q.SalaryHead)
                          .Where(q => q.PayMonth == PayMonth && PayScaleAssignmentData.Contains(q.SalaryHead.Code))
                          .ToList();

                    db.PerkTransT.RemoveRange(Perktranst);
                    db.SaveChanges();


                }

                try
                {
                    using (var streamReader = new StreamReader(stream, Encoding.UTF8))
                    {
                        string line;
                        while ((line = streamReader.ReadLine()) != null)
                        {
                            int counter = 0;
                            if (line.Trim() != "" && line.Trim() != null)
                            {
                                string Loc_Code = line.Substring(Convert.ToInt16(testing[0].PositionNo) - 1, Convert.ToInt16(testing[0].PositionNoLength)).Trim();
                                string Loan_head = line.Substring(Convert.ToInt16(testing[1].PositionNo) - 1, Convert.ToInt16(testing[1].PositionNoLength)).Trim();
                                string Acc_No = line.Substring(Convert.ToInt16(testing[2].PositionNo) - 1, Convert.ToInt16(testing[2].PositionNoLength)).Trim();
                                LLoanAccNo = Acc_No.ToString().PadLeft(8, '0');
                                string ChkData = "";

                                ChkData = line.Substring(Convert.ToInt16(testing[6].PositionNo) - 1, Convert.ToInt16(testing[6].PositionNoLength)).Trim();
                                double BankIntrest = ChkData == "" ? 0 : Convert.ToDouble(ChkData);

                                ChkData = line.Substring(Convert.ToInt16(testing[11].PositionNo) - 1, Convert.ToInt16(testing[11].PositionNoLength)).Trim();
                                double Principleoutstnd = ChkData == "" ? 0 : Convert.ToDouble(ChkData.Replace("-", ""));

                                ChkData = line.Substring(Convert.ToInt16(testing[12].PositionNo) - 1, Convert.ToInt16(testing[12].PositionNoLength)).Trim();
                                double Interestoutstnd = ChkData == "" ? 0 : Convert.ToDouble(ChkData.Replace("-", ""));

                                string rowno = Convert.ToString(counter);


                                checkifpresent = LoanProcessPDCC(Loc_Code.Trim(), Loan_head.Trim(), PayMonth, LLoanAccNo, rowno, BankIntrest, Principleoutstnd, Interestoutstnd).ToString();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogFile Logfile = new LogFile();
                    ErrorLog Err = new ErrorLog()
                    {
                        ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                        ExceptionMessage = ex.Message + "-" + LLoanAccNo,
                        ExceptionStackTrace = ex.StackTrace,
                        LineNo = Convert.ToString(new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                        LogTime = DateTime.Now
                    };
                    Logfile.CreateLogFile(Err);
                    return ex.Message;

                }
            }

            return checkifpresent;

        }

        public int LoanProcessPDCC(string LocCode, string LoanAdvHead, string LoanPayMonth, string LLoanAccNo, string rowno, Double BankIntrest, Double Principleoutstnd, double Interestoutstnd)
        {
            try
            {
                if (LoanAdvHead == null || LoanPayMonth == null || Principleoutstnd == null || Interestoutstnd == null)
                {
                    return 0;
                    //return Json(new { data = rowno, success = false, responseText = "Column Shouldn't be null..!" }, JsonRequestBehavior.AllowGet);
                }
                using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, new System.TimeSpan(0, 60, 0)))
                {
                    using (DataBaseContext db = new DataBaseContext())
                    {
                        Double MonthlyPrinciple = 0, MonthlyInterest = 0, ActualPrinciple = 0, ActualInterest = 0, ProjPrinciple = 0, ProjInterest = 0;


                        DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };
                        var Id = Convert.ToInt32(SessionManager.CompanyId);
                        string _CompCode = db.Company.Where(e => e.Id == Id).FirstOrDefault().Code.ToUpper();

                        var LoanHeadId = db.LoanAdvanceHead.Include(e => e.PerkHead).Include(e => e.LoanAdvancePolicy).Where(a => a.Code == LoanAdvHead).FirstOrDefault();

                        #region SBI and Bank interest rate
                        DateTime FileDate = Convert.ToDateTime("01/" + LoanPayMonth);
                        double SBIIntrest = 0;
                        double TblBankInt = 0;
                        if (LoanHeadId.LoanAdvancePolicy != null && LoanHeadId.LoanAdvancePolicy.Count() > 0)
                        {
                            SBIIntrest = LoanHeadId.LoanAdvancePolicy.Where(e => FileDate >= e.EffectiveDate.Value && FileDate <= e.EndDate.Value).FirstOrDefault().GovtIntRate;
                            TblBankInt = LoanHeadId.LoanAdvancePolicy.Where(e => FileDate >= e.EffectiveDate.Value && FileDate <= e.EndDate.Value).FirstOrDefault().IntRate;
                            if (BankIntrest != TblBankInt)
                            {
                                return 4; //"Updated bank interest is not defined in Loan advance policy."
                            }
                        }
                        else
                        {
                            return 3; //"Please define Loan advance policy."
                        }
                        #endregion SBI and Bank interest rate
                        //var LoanReq = db.LoanAdvRequest.Include(e => e.LoanAccBranch.LocationObj).Include(e => e.LoanAdvRepaymentT)
                        //    .Where(e => e.LoanAccBranch.LocationObj.LocCode == LocCode && e.LoanAccNo == LLoanAccNo && e.LoanAdvanceHead_Id == LoanHeadId.Id && e.IsActive == true).FirstOrDefault();
                        var LoanReq = db.LoanAdvRequest.Include(e => e.LoanAccBranch.LocationObj).Include(e => e.LoanAdvRepaymentT).Include(e => e.LoanAdvRepaymentTSettlement)
                            .Where(e => e.LoanAccBranch.LocationObj.LocCode == LocCode && e.LoanAccNo == LLoanAccNo && e.LoanAdvanceHead_Id == LoanHeadId.Id && e.IsActive == true).FirstOrDefault();

                        

                        if (LoanReq == null)
                        {
                            return 1;
                            //Json(new { data = rowno, success = false, responseCode = 1, responseText = "Loan Request Not Found..!" }, JsonRequestBehavior.AllowGet);
                        }
                        //var LoanRepayment = LoanReq != null ?
                        //    LoanReq.LoanAdvRepaymentT.Where(q => q.PayMonth == LoanPayMonth).FirstOrDefault() : null;
                        var LoanRepayment = LoanReq != null ?
                            db.LoanAdvRepaymentT.Where(q => q.PayMonth == LoanPayMonth && q.LoanAdvRequest_Id == LoanReq.Id).FirstOrDefault() : null;

                        List<LoanAdvRepaymentT> LoanAdvRepaymentTDet = new List<LoanAdvRepaymentT>();
                        #region EMI->Monthly Principal and Interest Adjustment as per outstanding Interest and Principal
                        MonthlyInterest = LoanReq.MonthlyInterest; ;
                        MonthlyPrinciple = LoanReq.MonthlyPricipalAmount;
                        var OldMonthlyInterest = LoanReq.MonthlyInterest; ;
                        var OldMonthlyPrinciple = LoanReq.MonthlyPricipalAmount;
                        //Outstanding Interest check
                        if (Interestoutstnd == 0)
                        {
                            MonthlyInterest = 0;
                            MonthlyPrinciple = LoanReq.MonthlyPricipalAmount + LoanReq.MonthlyInterest;
                        }
                        else if (LoanReq.MonthlyInterest >= Interestoutstnd)
                        {
                            MonthlyInterest = Interestoutstnd;
                            MonthlyPrinciple =( Principleoutstnd <= (LoanReq.MonthlyPricipalAmount + LoanReq.MonthlyInterest - Interestoutstnd)) == true ? Principleoutstnd : (LoanReq.MonthlyPricipalAmount + LoanReq.MonthlyInterest- Interestoutstnd);
                        }
                        else if (LoanReq.MonthlyInterest < Interestoutstnd)
                        {
                            MonthlyInterest = LoanReq.MonthlyInterest;
                            if (LoanReq.MonthlyPricipalAmount < Principleoutstnd)
                            {
                                MonthlyPrinciple = LoanReq.MonthlyPricipalAmount;
                            }
                            else
                            {
                                MonthlyPrinciple = Principleoutstnd;
                            }
                        }
                        else if (LoanReq.MonthlyPricipalAmount < Principleoutstnd)
                        {
                            MonthlyPrinciple = Principleoutstnd;
                            
                        }
                        ////Outstanding Principal check
                        //if (Principleoutstnd == 0 && Interestoutstnd==0)
                        //{
                        //    MonthlyInterest = 0;
                        //    MonthlyPrinciple = 0;
                        //}
                        //else if (MonthlyPrinciple <= Principleoutstnd)
                        //{
                        //    MonthlyPrinciple = MonthlyPrinciple;
                        //    MonthlyInterest = MonthlyInterest;
                        //}
                        //else if (MonthlyPrinciple > Principleoutstnd)
                        //{
                        //    //assumption Interest is recovered first : taken care
                        //    //MonthlyInterest = 0;
                        //    //MonthlyPrinciple = Principleoutstnd;
                        //    if (MonthlyInterest != 0)
                        //    { 
                        //        if((Principleoutstnd + MonthlyInterest)>MonthlyPrinciple )
                        //        {
                        //            MonthlyInterest = Principleoutstnd + MonthlyInterest - MonthlyPrinciple;
                        //        }
                        //        else
                        //        {
                        //            MonthlyPrinciple = Principleoutstnd + MonthlyInterest;
                        //            MonthlyInterest = 0;
                        //        }
                        //    }
                        //}
                        #endregion EMI->Monthly Principal and Interest Adjustment as per outstanding Interest and Principal

                        #region modify LoanRepayment data
                        // if (LoanRepayment != null && (MonthlyInterest != OldMonthlyInterest || MonthlyPrinciple != OldMonthlyPrinciple))//OldMonthlyInterest and MonthlyInterest are same (same as principle )so repayment multiple entry is going on so code comment
                       if (LoanRepayment != null)
                        {
                            var LoanAdvRepaymentT_temp = LoanRepayment;
                            LoanRepayment.PayMonth = LoanPayMonth;
                            LoanRepayment.MonthlyInterest = MonthlyInterest;
                            LoanRepayment.MonthlyPricipalAmount = MonthlyPrinciple;
                            LoanRepayment.InstallmentPaid = MonthlyPrinciple + MonthlyInterest;
                            LoanRepayment.InstallmentAmount = MonthlyPrinciple + MonthlyInterest;

                            db.Entry(LoanRepayment).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            
                        }
                        else
                        {
                            if (LoanRepayment == null)
                            {
                                double TotalLoanPaid = 0;
                                LoanAdvRepaymentT LoanAdvRepayT = new LoanAdvRepaymentT()
                                {
                                    InstallementDate = LoanReq.EnforcementDate,
                                    InstallmentAmount = MonthlyPrinciple + MonthlyInterest,
                                    InstallmentCount = 1,
                                    InstallmentPaid = MonthlyPrinciple + MonthlyInterest,
                                    PayMonth = LoanPayMonth,
                                    TotalLoanBalance = LoanReq.LoanSanctionedAmount,//LoanAdvRequest.LoanSanctionedAmount - installment_Paid,
                                    TotalLoanPaid = 0,//installment_Paid
                                    DBTrack = LoanReq.DBTrack,
                                    MonthlyInterest = MonthlyInterest,
                                    MonthlyPricipalAmount = MonthlyPrinciple,
                                    LoanAdvRequest_Id = LoanReq.Id//foreign key
                                };
                                TotalLoanPaid = TotalLoanPaid + LoanAdvRepayT.InstallmentPaid;
                                db.LoanAdvRepaymentT.Add(LoanAdvRepayT);
                                //db.Entry(LoanRepayment).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();

                                //LoanAdvRepaymentTDet.Add(LoanAdvRepayT);
                                //LoanAdvRepaymentTDet.AddRange(LoanReq.LoanAdvRepaymentT);
                                //LoanReq.LoanAdvRepaymentT = LoanAdvRepaymentTDet;
                                LoanRepayment = LoanAdvRepayT;
                            }

                        }
                        #endregion modify LoanRepayment data
                        //emp
                        //sec 80 C update

                        #region balance month calculation
                        int Flag = 1;
                        EmployeePayroll OEmployeePayroll = db.EmployeePayroll
                                    .Where(a => a.Id == LoanReq.EmployeePayroll_Id).FirstOrDefault();


                        Employee OEmployee = db.Employee
                       .Include(e => e.ServiceBookDates)
                       .Where(r => r.Id == OEmployeePayroll.Employee_Id).FirstOrDefault();

                        var LoanReqPrevClose = db.LoanAdvRequest.Include(e => e.LoanAccBranch.LocationObj).Include(e => e.LoanAdvRepaymentT).Include(e => e.LoanAdvRepaymentTSettlement)
                            .Where(e => e.LoanAdvanceHead_Id == LoanHeadId.Id && e.EmployeePayroll_Id == OEmployeePayroll.Id && e.IsActive == false).ToList();


                        int CompId = Convert.ToInt32(SessionManager.CompanyId);
                        CompanyPayroll OCompanyPayroll = db.CompanyPayroll.Include(e => e.Company).Include(e => e.Company.Calendar).Include(e => e.Company.Calendar.Select(r => r.Name)).Where(e => e.Company_Id == CompId).SingleOrDefault();
                        var OFinancia = OCompanyPayroll.Company.Calendar.Where(r => r.Default == true && r.Name.LookupVal.ToString().ToUpper() == "FINANCIALYEAR").SingleOrDefault();

                        DateTime FromPeriod = Convert.ToDateTime(OFinancia.FromDate);
                        DateTime ToPeriod = Convert.ToDateTime(OFinancia.ToDate);

                        if (OEmployee.ServiceBookDates.JoiningDate >= OFinancia.FromDate)
                        {
                            FromPeriod = Convert.ToDateTime(OEmployee.ServiceBookDates.JoiningDate);
                        }
                        else if (OEmployee.ServiceBookDates.ServiceLastDate >= OFinancia.FromDate &&
                           OEmployee.ServiceBookDates.ServiceLastDate <= OFinancia.ToDate)
                        {
                            ToPeriod = Convert.ToDateTime(OEmployee.ServiceBookDates.ServiceLastDate);
                        }
                        else if (OEmployee.ServiceBookDates.RetirementDate >= OFinancia.FromDate &&
                           OEmployee.ServiceBookDates.RetirementDate <= OFinancia.ToDate)
                        {
                            ToPeriod = Convert.ToDateTime(OEmployee.ServiceBookDates.RetirementDate);
                        }

                        String mPeriodRange = "";
                        List<string> mPeriod = new List<string>();
                        DateTime mEndDate = Convert.ToDateTime("01/" + Convert.ToDateTime(ToPeriod).ToString("MM/yyyy")).AddMonths(1).Date;
                        mEndDate = mEndDate.AddDays(-1).Date;
                        for (DateTime mTempDate = FromPeriod; mTempDate <= mEndDate; mTempDate = mTempDate.AddMonths(1))
                        {
                            if (mPeriodRange == "")
                            {
                                mPeriodRange = Convert.ToDateTime(mTempDate).ToString("MM/yyyy");
                                mPeriod.Add(Convert.ToDateTime(mTempDate).ToString("MM/yyyy"));
                            }
                            else
                            {
                                mPeriodRange = mPeriodRange + "," + Convert.ToDateTime(mTempDate).ToString("MM/yyyy");
                                mPeriod.Add(Convert.ToDateTime(mTempDate).ToString("MM/yyyy"));
                            }
                        }
                        String OLastMonth = "";
                        String OMonthChk = "";
                        List<SalaryT> OSalaryTEmp = db.SalaryT.AsNoTracking().Where(e => e.EmployeePayroll_Id == OEmployeePayroll.Id && e.FinnanceYearId_Id == OFinancia.Id).ToList();
                        //if (OSalaryTC != null)
                        //{ OSalaryTEmp.Add(OSalaryTC); }

                        if (OSalaryTEmp.Count > 0)
                        {
                            SalaryT OSalaryTC = OSalaryTEmp.Where(e => e.PayMonth == LoanPayMonth).FirstOrDefault();
                            if (OSalaryTC != null)
                            {
                                OMonthChk = OSalaryTEmp.Max(e => Convert.ToDateTime("01/" + e.PayMonth).Date).ToString("MM/yyyy");
                                if (OMonthChk == OSalaryTC.PayMonth)
                                {
                                    OMonthChk = OSalaryTEmp.Max(e => Convert.ToDateTime("01/" + e.PayMonth).Date).AddMonths(-1).ToString("MM/yyyy");
                                }
                                else
                                {
                                    OMonthChk = OSalaryTEmp.Max(e => Convert.ToDateTime("01/" + e.PayMonth).Date).ToString("MM/yyyy");
                                }
                            }
                            else
                            {
                                OMonthChk = OSalaryTEmp.Max(e => Convert.ToDateTime("01/" + e.PayMonth).Date).ToString("MM/yyyy");
                            }


                        }
                        else // salary not process or new join
                        {
                            OMonthChk = DateTime.Now.AddMonths(-1).ToString("MM/yyyy");
                            OMonthChk = Convert.ToDateTime(FromPeriod.AddMonths(-1)).ToString("MM/yyyy");
                        }

                        if (OMonthChk == DateTime.Now.ToString("MM/yyyy"))
                        {
                            OLastMonth = DateTime.Now.ToString("MM/yyyy");
                        }
                        else
                        {
                            OLastMonth = OMonthChk;//ProcessDate.AddMonths(-1).ToString("MM/yyyy");
                        }

                        if (ToPeriod.Date < DateTime.Now.Date)
                        {
                            if (OSalaryTEmp.Where(e => e.PayMonth == ToPeriod.ToString("MM/yyyy")).SingleOrDefault() != null)
                            {
                                OLastMonth = ToPeriod.ToString("MM/yyyy");
                            }
                            else
                            {
                                OLastMonth = OMonthChk;
                            }
                        }

                        double mBalMonths = (ToPeriod.Month + ToPeriod.Year * 12) - (Convert.ToDateTime("01/" + OLastMonth).Month + Convert.ToDateTime("01/" + OLastMonth).Year * 12);
                        #endregion Balance month end
                        //----------balance month end
                        #region Interest Projection
                        DateTime mLoanEndDate = OFinancia.ToDate.Value;
                        if (LoanReq.CloserDate < mLoanEndDate)//check for loan closerdate
                        {
                            mLoanEndDate = LoanReq.CloserDate.Value;
                        }
                        ActualInterest = LoanReq.LoanAdvRepaymentT.Where(t => t.InstallementDate >= OFinancia.FromDate && t.InstallementDate <= mLoanEndDate && t.SalaryT_Id != null).Sum(e => e.MonthlyInterest);
                        
                        double ActualInterestSettlement = 0, ActualPrincipleSettlement=0;
                        ActualInterestSettlement = LoanReq.LoanAdvRepaymentTSettlement.Where(t => t.InstallementDate >= OFinancia.FromDate && t.InstallementDate <= mLoanEndDate && t.RepaymentDate != null).Sum(e => e.MonthlyInterest);
                        ActualPrincipleSettlement = LoanReq.LoanAdvRepaymentTSettlement.Where(t => t.InstallementDate >= OFinancia.FromDate && t.InstallementDate <= mLoanEndDate && t.RepaymentDate != null).Sum(e => e.MonthlyPricipalAmount);
                        // Previous Close Loan start
                        double PreviousCloseActualPrinciple = 0;
                        double PreviousCloseActualInterest = 0;
                        double PreviousCloseActualInterestSettlement = 0, PreviousCloseActualPrincipleSettlement = 0;
                        foreach (var LoanReqPrevCloseData in LoanReqPrevClose)
                        {
                            PreviousCloseActualPrinciple = PreviousCloseActualPrinciple + LoanReqPrevCloseData.LoanAdvRepaymentT.Where(t => t.InstallementDate >= OFinancia.FromDate && t.InstallementDate <= mLoanEndDate && t.RepaymentDate != null && t.PayMonth != LoanPayMonth).Sum(e => e.MonthlyPricipalAmount);

                            PreviousCloseActualInterest = PreviousCloseActualInterest + LoanReqPrevCloseData.LoanAdvRepaymentT.Where(t => t.InstallementDate >= OFinancia.FromDate && t.InstallementDate <= mLoanEndDate && t.SalaryT_Id != null).Sum(e => e.MonthlyInterest);
                            PreviousCloseActualInterestSettlement = PreviousCloseActualInterestSettlement + LoanReqPrevCloseData.LoanAdvRepaymentTSettlement.Where(t => t.InstallementDate >= OFinancia.FromDate && t.InstallementDate <= mLoanEndDate && t.RepaymentDate != null).Sum(e => e.MonthlyInterest);
                            PreviousCloseActualPrincipleSettlement = PreviousCloseActualPrincipleSettlement + LoanReqPrevCloseData.LoanAdvRepaymentTSettlement.Where(t => t.InstallementDate >= OFinancia.FromDate && t.InstallementDate <= mLoanEndDate && t.RepaymentDate != null).Sum(e => e.MonthlyPricipalAmount);

                        }
                        ActualInterest = ActualInterest + PreviousCloseActualInterest + PreviousCloseActualInterestSettlement;

                        
                        // Previous Close Loan end
                        
                        //check value during debugging
                        mBalMonths = (mLoanEndDate.Month + mLoanEndDate.Year * 12) - (Convert.ToDateTime("01/" + OLastMonth).Month + Convert.ToDateTime("01/" + OLastMonth).Year * 12);

                        if (mBalMonths < 0)
                            mBalMonths = 0;
                        //projecion on Loanrepayment data
                        //double ProjInterestCal =  MonthlyInterest * 12;
                        double ProjInterestCalForPerk = 0;
                        double ProjInterestCal = MonthlyInterest * mBalMonths;
                        if (ProjInterestCal <= Interestoutstnd)
                        {
                            //projected interest less than outstanding interest
                            if (ActualInterest != 0)
                            {
                                Interestoutstnd = ProjInterestCal;
                                ProjInterest = ActualInterest + ActualInterestSettlement + ProjInterestCal;
                            }
                            else
                            {
                                Interestoutstnd = ProjInterestCal;
                                ProjInterest = Interestoutstnd + ActualInterestSettlement;
                            }
                        }
                        else
                        {
                            if (ActualInterest != 0)
                            {
                                //var mTotActInvstpro = LoanReq.LoanAdvRepaymentT.OrderBy(x => x.InstallementDate).Where(t => t.InstallementDate >= OFinancia.FromDate && t.InstallementDate <= mLoanEndDate && t.RepaymentDate != null).LastOrDefault();
                                //ProjInterest = (mTotActInvstpro.MonthlyInterest * mBalMonths);
                                ProjInterest = (ActualInterest + ActualInterestSettlement + Interestoutstnd);
                            }
                            else
                            {
                                //ProjInterest = (MonthlyInterest * mBalMonths); 
                                ProjInterest = (Interestoutstnd);
                            }
                        }
                        #endregion Interest Projection
                        #region Principal Projection
                        ActualPrinciple = LoanReq.LoanAdvRepaymentT.Where(t => t.InstallementDate >= OFinancia.FromDate && t.InstallementDate <= mLoanEndDate && t.RepaymentDate != null && t.PayMonth != LoanPayMonth).Sum(e => e.MonthlyPricipalAmount);
                        
                        ActualPrinciple = ActualPrinciple + PreviousCloseActualPrinciple + PreviousCloseActualPrincipleSettlement;

                        double ProjPrincipleCal = MonthlyPrinciple * mBalMonths;
                        if (ProjPrincipleCal <= Principleoutstnd)
                        {
                            if (ActualPrinciple != 0)
                            {
                                //adjustment of interest into principal in case of interestoutstanding zero
                                //ProjPrinciple = ActualPrinciple + (MonthlyPrinciple * mBalMonths) + (MonthlyInterest * mBalMonths) - (ActualInterest + Interestoutstnd);
                                //principal ceiling check
                                double CeilChk = (MonthlyPrinciple * mBalMonths) + ((MonthlyInterest * mBalMonths) - (ProjInterest - ActualInterest));
                                if (CeilChk > Principleoutstnd)
                                {
                                    CeilChk = Principleoutstnd;
                                }
                                ProjPrinciple = ActualPrinciple + ActualPrincipleSettlement + CeilChk;
                            }
                            else
                            {
                                double CeilChk = Principleoutstnd + ActualPrincipleSettlement + ((MonthlyInterest * mBalMonths) - (ProjInterest));
                                if (CeilChk > Principleoutstnd)
                                {
                                    CeilChk = Principleoutstnd;
                                }

                                ProjPrinciple = CeilChk;
                                Principleoutstnd = ProjPrincipleCal;
                            }
                        }
                        else
                        {
                            if (ActualPrinciple != 0)
                            {
                                //var mTotActInvstpro = LoanReq.LoanAdvRepaymentT.OrderBy(x => x.InstallementDate).Where(t => t.InstallementDate >= OFinancia.FromDate && t.InstallementDate <= mLoanEndDate && t.RepaymentDate != null).LastOrDefault();
                                //ProjInterest = (mTotActInvstpro.MonthlyInterest * mBalMonths);
                                ProjPrinciple = (ActualPrinciple + ActualPrincipleSettlement + Principleoutstnd);
                            }
                            else
                            {
                                //ProjInterest = (MonthlyInterest * mBalMonths); 
                                ProjPrinciple = (Principleoutstnd);
                            }
                        }

                        #endregion Principal Projection
                        //if (ActualPrinciple != 0)
                        //{
                        //    var mTotActInvstpro = LoanReq.LoanAdvRepaymentT.OrderBy(x => x.InstallementDate).Where(t => t.InstallementDate >= OFinancia.FromDate && t.InstallementDate <= mLoanEndDate && t.RepaymentDate != null).LastOrDefault();
                        //    ProjPrinciple = (mTotActInvstpro.MonthlyPricipalAmount * mBalMonths);
                        //}
                        //else
                        //{
                        //    ProjPrinciple = (MonthlyPrinciple * mBalMonths);
                        //}
                        //ActualPrinciple = ActualPrinciple + MonthlyPrinciple;

                     

                        int OFinancialYear = db.Calendar.Where(e => e.Name.LookupVal.ToUpper() == "FINANCIALYEAR" && e.Default == true).FirstOrDefault().Id;
                        CompanyPayroll OIncomeTax = IncomeTaxCalc._returnCompanyPayroll_IncomeTax_New(Id, OFinancialYear);
                        IncomeTax OITMaster = OIncomeTax.IncomeTax.Where(e => e.FyCalendar.Id == OFinancialYear).SingleOrDefault();
                        ITSection OITSection80C = OITMaster.ITSection.Where(e => e.ITSectionList.LookupVal.ToUpper() == "SECTION80C" &&
                                            e.ITSectionListType.LookupVal.ToUpper() == "REBATE").FirstOrDefault(); //rebate

                        List<ITInvestment> OITInvestments = OITSection80C.ITInvestments.ToList();
                        List<LoanAdvanceHead> OITInvestmentsLoan = OITSection80C.LoanAdvanceHead.Where(e => e.Id == LoanHeadId.Id).ToList();
                        List<ITSubInvestment> OITSubInvestments = OITInvestments.SelectMany(e => e.ITSubInvestment).ToList();
                        //IList<ITInvestmentPayment> OEmpITInvestment = null;
                        var OEmpITInvestment = db.ITInvestmentPayment.Where(e => e.EmployeePayroll_Id == OEmployeePayroll.Id).Select(z => new
                        {
                            Id = z.Id,
                            FinancialYearId = z.FinancialYear_Id,
                            LoanAdvanceHeadSalaryHead_Id = z.LoanAdvanceHead.SalaryHead_Id,
                            ITSectionListTypeLookupVal = z.ITSection.ITSectionListType.LookupVal,
                            ITInvestments = z.ITSection.ITInvestments.ToList(),
                            OEmpITSubInvestment = z.ITSubInvestmentPayment.ToList()
                        }).Where(e => e.FinancialYearId == OFinancialYear && e.ITSectionListTypeLookupVal.ToUpper() == "REBATE").ToList();

                        //EmployeePayroll OEmpInvest = db.EmployeePayroll.Where(e => e.Id == OEmployeePayroll.Id)
                        //                .Include(e => e.ITInvestmentPayment)
                        //                .Include(e => e.ITInvestmentPayment.Select(r => r.FinancialYear))
                        //                .Include(e => e.ITInvestmentPayment.Select(r => r.ITSection))
                        //                .Include(e => e.ITInvestmentPayment.Select(r => r.ITSection.ITSectionList))
                        //                .Include(e => e.ITInvestmentPayment.Select(r => r.ITSection.ITSectionListType))
                        //                .Include(e => e.ITInvestmentPayment.Select(r => r.ITSection.ITSectionList))//added 17042017 by prashant
                        //                .Include(e => e.ITInvestmentPayment.Select(r => r.ITSubInvestmentPayment))
                        //                .Include(e => e.ITInvestmentPayment.Select(r => r.LoanAdvanceHead))
                        //                .Include(e => e.ITInvestmentPayment.Select(r => r.LoanAdvanceHead.ITLoan))
                        //                .Include(e => e.ITInvestmentPayment.Select(r => r.LoanAdvanceHead.ITSection))
                        //                .Include(e => e.ITInvestmentPayment.Select(r => r.LoanAdvanceHead.LoanAdvancePolicy))
                        //                .Include(e => e.ITInvestmentPayment.Select(r => r.LoanAdvanceHead.SalaryHead))
                        //                .Include(e => e.ITInvestmentPayment.Select(r => r.ITInvestment))
                        //                .Include(e => e.ITInvestmentPayment.Select(r => r.ITInvestment.SalaryHead))
                        //    .SingleOrDefault();

                        //if (OEmpInvest.ITInvestmentPayment != null)
                        //{
                        //    OEmpITInvestment = OEmpInvest.ITInvestmentPayment.Where(e => e.FinancialYear.Id == OFinancialYear && e.ITSection.ITSectionListType.LookupVal.ToUpper() == "REBATE").ToList();
                        //    List<ITSubInvestmentPayment> OEmpITSubInvestment = OEmpInvest.ITInvestmentPayment
                        //        .Where(e => e.FinancialYear.Id == OFinancialYear && e.ITSection.ITSectionListType.LookupVal.ToUpper() == "REBATE")
                        //        .SelectMany(e => e.ITSubInvestmentPayment).ToList();
                        //}

                        //  Calendar temp_OFinancialYear = db.Calendar.Where(e => e.Id == OFinancialYear).SingleOrDefault();

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

                                    //ITInvestmentPayment OEmpSalInvestmentChk = OEmpITInvestment.Where(e => e.LoanAdvanceHead != null && e.LoanAdvanceHead.SalaryHead != null &&
                                    //    e.LoanAdvanceHead.SalaryHead.Id == ca.SalaryHead_Id).SingleOrDefault();20/082024

                                    var OEmpSalInvestmentChk = OEmpITInvestment.Where(e => e.LoanAdvanceHeadSalaryHead_Id == ca.SalaryHead_Id).SingleOrDefault();

                                    if (OEmpSalInvestmentChk != null)
                                    {
                                        ITInvestmentPayment OEmpSalInvestmentObj = db.ITInvestmentPayment.Where(a => a.Id == OEmpSalInvestmentChk.Id).SingleOrDefault();
                                        OEmpSalInvestmentObj.ActualInvestment = ActualPrinciple;
                                        OEmpSalInvestmentObj.DeclaredInvestment = ProjPrinciple;
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
                                            ActualInvestment = ActualPrinciple,
                                            DeclaredInvestment = ProjPrinciple,
                                            Narration = "Investment under Section 80C Loan through Salary",
                                            DBTrack = dbt,
                                            EmployeePayroll_Id = OEmpPay.Id
                                        };

                                        if (Flag == 1)
                                        {
                                            db.ITInvestmentPayment.Add(OEmpITInvestmentSave);
                                            db.SaveChanges();
                                        }

                                        mList.Add(OEmpITInvestmentSave);
                                        

                                    }
                                }

                            }

                            if (mList.Count() > 0 && Flag == 1)
                            {
                                //commented modified today
                                //List<ITInvestmentPayment> OFAT = new List<ITInvestmentPayment>();

                                //EmployeePayroll aa = db.EmployeePayroll.Find(OEmployeePayroll.Id);
                                //if (aa.ITInvestmentPayment != null)
                                //{ mList.AddRange(aa.ITInvestmentPayment); }

                                //aa.ITInvestmentPayment = mList;
                                ////OEmployeePayroll.DBTrack = dbt;
                                //db.EmployeePayroll.Attach(aa);
                                //db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                //db.SaveChanges();
                                //db.Entry(aa).State = System.Data.Entity.EntityState.Detached;

                            }

                        }

                        //sec 24 interest update start

                        //            EmployeePayroll OEmpITSection24EmpData = db.EmployeePayroll
                        //.Include(e => e.ITSection24Payment)
                        //  .Include(e => e.ITSection24Payment.Select(r => r.FinancialYear))
                        //  .Include(e => e.ITSection24Payment.Select(r => r.ITSection))
                        //  .Include(e => e.ITSection24Payment.Select(r => r.ITSection.ITSectionList))
                        //  .Include(e => e.ITSection24Payment.Select(r => r.ITSection.ITSectionListType))
                        //  .Include(e => e.ITSection24Payment.Select(r => r.ITSection.ITSectionListType))

                        //  .Include(e => e.ITSection24Payment.Select(r => r.ITSection.LoanAdvanceHead))
                        //  .Include(e => e.ITSection24Payment.Select(r => r.LoanAdvanceHead))
                        //  .Include(e => e.ITSection24Payment.Select(r => r.LoanAdvanceHead.SalaryHead))
                        //  .Include(e => e.LoanAdvRequest)
                        //  .Include(e => e.LoanAdvRequest.Select(r => r.LoanAdvanceHead))
                        //  .Include(e => e.LoanAdvRequest.Select(r => r.LoanAdvanceHead.ITLoan))
                        //  .Include(e => e.LoanAdvRequest.Select(r => r.LoanAdvanceHead.SalaryHead))
                        //  .Include(e => e.LoanAdvRequest.Select(r => r.LoanAdvRepaymentT))
                        //  .Where(e => e.Id == OEmployeePayroll.Id)
                        //  .SingleOrDefault();

                        var OEmpITSection24EmpData = db.ITSection24Payment.Where(e => e.EmployeePayroll_Id == OEmployeePayroll.Id).Select(r => new
                        {
                            Id = r.Id,
                            FinancialYearId = r.FinancialYear_Id,
                            LoanAdvanceHeadId = r.LoanAdvanceHead_Id,
                            SalaryHeadId = r.LoanAdvanceHead.SalaryHead_Id,
                            ITSectionListTypeLookupVal = r.ITSection.ITSectionListType.LookupVal,

                        }).ToList();


                        List<ITSection> OITSection24 = OITMaster.ITSection.Where(e => e.ITSectionList.LookupVal.ToUpper() == "SECTION24" && e.ITSectionListType.LookupVal.ToUpper() == "LOAN").ToList();

                        //  OITInvestmentsList = OITSection24.SelectMany(e => e.LoanAdvanceHead.Where(a => a.SalaryHead != null && (a.ITLoan.IntAppl == true || a.ITLoan.IntPrincAppl == true))).ToList();
                        List<LoanAdvanceHead> OITInvestmentsList = OITSection24.SelectMany(e => e.LoanAdvanceHead.Where(a => a.SalaryHead != null && a.Id == LoanHeadId.Id && a.ITLoan == null)).ToList();

                        //List<ITSection24Payment> OEmpIT24Investment = OEmpITSection24EmpData.ITSection24Payment.Where(e => e.FinancialYear.Id == OFinancialYear && e.ITSection.ITSectionListType.LookupVal.ToUpper() == "LOAN" && e.LoanAdvanceHead.Id == LoanHeadId.Id).ToList();20/08/2024
                        var OEmpIT24Investment = OEmpITSection24EmpData.Where(e => e.FinancialYearId == OFinancialYear && e.ITSectionListTypeLookupVal.ToUpper() == "LOAN" && e.LoanAdvanceHeadId == LoanHeadId.Id).ToList();
                        // ActualInterest = ActualInterest + MonthlyInterest;

                        foreach (LoanAdvanceHead OITInvestments1 in OITInvestmentsList)
                        {

                            List<ITSection24Payment> mList = new List<ITSection24Payment>();

                            //ITSection24Payment OEmpSalInvestmentChk = OEmpIT24Investment.Where(e =>
                            //                       e.LoanAdvanceHead != null &&
                            //                       e.LoanAdvanceHead.SalaryHead != null &&
                            //                       e.LoanAdvanceHead.SalaryHead.Id == OITInvestments1.SalaryHead.Id).SingleOrDefault();20/08/2024


                            var OEmpSalInvestmentChk = OEmpIT24Investment.Where(e => e.SalaryHeadId == OITInvestments1.SalaryHead.Id).SingleOrDefault();
                                                  
                            if (OEmpSalInvestmentChk != null)
                            {
                                ITSection24Payment OEmpSalInvestmentObj = db.ITSection24Payment.Where(a => a.Id == OEmpSalInvestmentChk.Id).SingleOrDefault();
                                OEmpSalInvestmentObj.ActualInterest = ActualInterest;
                                OEmpSalInvestmentObj.DeclaredInterest = ProjInterest;
                                if (Flag == 1)
                                {
                                    db.Entry(OEmpSalInvestmentObj).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                }

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
                                    ActualInterest = ActualInterest,
                                    DeclaredInterest = ProjInterest,
                                    //ActualInterest = 0,
                                    //DeclaredInterest = 0,
                                    Narration = "Investment under Section 24 through Salary",
                                    DBTrack = dbt,
                                    SalaryApp = true,
                                    EmployeePayroll_Id = OEmpPay.Id

                                };

                                if (Flag == 1)
                                {
                                    db.ITSection24Payment.Add(OEmpITInvestmentSave);
                                    db.SaveChanges();
                                }

                                mList.Add(OEmpITInvestmentSave);
                                
                            }

                            // }
                            if (mList.Count() > 0 && Flag == 1)
                            {
                                //commented today
                                //EmployeePayroll aa = db.EmployeePayroll.Where(a => a.Id == OEmployeePayroll.Id).FirstOrDefault();
                                //if (aa.ITSection24Payment != null)
                                //{ mList.AddRange(aa.ITSection24Payment); }
                                //aa.ITSection24Payment = mList;
                                ////OEmployeePayroll.DBTrack = dbt;
                                //db.EmployeePayroll.Attach(aa);
                                //db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                //db.SaveChanges();
                                //db.Entry(aa).State = System.Data.Entity.EntityState.Detached;

                            }
                        }

                        //sec 24 interest update end

                        var sddd = LoanHeadId.PerkHead != null ? LoanHeadId.PerkHead.Id : 0;
                        if (sddd != null && sddd != 0)
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
                                     .Where(a => a.Id == OEmployeePayroll.Id).FirstOrDefault();

                                var PerktranstData = empP.PerkTransT
                                               .Where(q => q.PayMonth == LoanPayMonth && q.SalaryHead.Id == chksalhead.Id)
                                               .FirstOrDefault();


                                double PerkActualAmt = 0;
                                double PerkProjectedAmt = 0;
                                double PerkAmtuploadmonth = 0;

                                LoanAdvRequest LoanAdvRequest = db.LoanAdvRequest.Include(e => e.LoanAdvRepaymentT).Include(e => e.LoanAccBranch).Include(e => e.LoanAccBranch.LocationObj)
                                      .Where(e => e.EmployeePayroll_Id == OEmployeePayroll.Id && e.LoanAdvanceHead_Id == LoanHeadId.Id && e.LoanAccNo == LLoanAccNo
                                      && e.LoanAccBranch.LocationObj.LocCode == LocCode && e.IsActive == true).FirstOrDefault();


                                if (LoanAdvRequest.CloserDate < mLoanEndDate)
                                {
                                    mLoanEndDate = LoanAdvRequest.CloserDate.Value;
                                }


                                if (paysc.SalaryHead.Frequency.LookupVal.ToUpper() == "MONTHLY")
                                {
                                    double IntDiff = SBIIntrest - BankIntrest;
                                    PerkProjectedAmt = Math.Round(((Interestoutstnd / BankIntrest) * IntDiff), 0);

                                    PerkActualAmt = ((MonthlyInterest / BankIntrest) * IntDiff);
                                    PerkActualAmt = Math.Round(PerkActualAmt, 0);
                                }
                                else if (paysc.SalaryHead.Frequency.LookupVal.ToUpper() == "YEARLY")
                                {
                                    LoanHeadId = db.LoanAdvanceHead.Include(e => e.PerkHead).Include(e => e.LoanAdvancePolicy).Where(a => a.Code == LoanAdvHead).FirstOrDefault();

                                    if (LoanHeadId.LoanAdvancePolicy != null && LoanHeadId.LoanAdvancePolicy.Count() > 0)
                                    {

                                        for (DateTime mTempDate = FromPeriod; mTempDate <= mEndDate; mTempDate = mTempDate.AddMonths(1))
                                        {
                                            var LoanPolicy = LoanHeadId.LoanAdvancePolicy.Where(e => mTempDate >= e.EffectiveDate.Value && FileDate <= e.EndDate.Value).ToList();
                                            foreach (var item in LoanPolicy)
                                            {
                                                var diffMonths = (mEndDate.Month + mEndDate.Year * 12) - (mTempDate.Month + mTempDate.Year * 12);
                                                double IntDiff = item.GovtIntRate - item.IntRate;
                                                if (LoanReq.LoanAdvRepaymentT != null && LoanReq.LoanAdvRepaymentT.Count() > 0)
                                                {
                                                    // loand upload month perk value add in perktranst
                                                    DateTime uploadmonth = Convert.ToDateTime("01/" + LoanPayMonth);
                                                    var MonthlyInterestObjloanuploadmonth = LoanReq.LoanAdvRepaymentT.Where(e => e.PayMonth == uploadmonth.ToString("MM/yyyy") && e.SalaryT_Id == null).FirstOrDefault();
                                                    var MonthlyInterestSettlementObjloanuploadmonth = LoanReq.LoanAdvRepaymentTSettlement.Where(e => e.PayMonth == uploadmonth.ToString("MM/yyyy")).FirstOrDefault();
                                                    if (MonthlyInterestObjloanuploadmonth != null || MonthlyInterestSettlementObjloanuploadmonth != null)
                                                    {
                                                        PerkAmtuploadmonth = ((MonthlyInterestObjloanuploadmonth == null ? 0 : MonthlyInterestObjloanuploadmonth.MonthlyInterest) + (MonthlyInterestSettlementObjloanuploadmonth == null ? 0 : MonthlyInterestSettlementObjloanuploadmonth.MonthlyInterest) / item.IntRate) * IntDiff;
                                                    }

                                                    var MonthlyInterestObj = LoanReq.LoanAdvRepaymentT.Where(e => e.PayMonth == mTempDate.ToString("MM/yyyy") && e.SalaryT_Id != null).FirstOrDefault();
                                                    var MonthlyInterestSettlementObj = LoanReq.LoanAdvRepaymentTSettlement.Where(e => e.PayMonth == mTempDate.ToString("MM/yyyy")).FirstOrDefault();

                                                    if (MonthlyInterestObj != null || MonthlyInterestSettlementObj!=null)
                                                    {
                                                        //calculate int diff sbi rate & bank rate
                                                        MonthlyInterest = MonthlyInterestObj == null ? 0 : MonthlyInterestObj.MonthlyInterest;

                                                        PerkActualAmt = PerkActualAmt + (((MonthlyInterest + (MonthlyInterestSettlementObj==null?0:MonthlyInterestSettlementObj.MonthlyInterest)) / item.IntRate) * IntDiff);
                                                        
                                                        //MonthlyInterest = LoanReq.LoanAdvRepaymentT.Where(e => e.PayMonth == mTempDate.ToString("MM/yyyy")).FirstOrDefault().MonthlyInterest;
                                                        
                                                        //ActualInterest = ActualInterest + MonthlyInterest;
                                                        //ProjInterest = ActualInterest + (MonthlyInterest * diffMonths);

                                                        


                                                        //PerkProjectedAmt = ((ProjInterest / item.IntRate) * IntDiff);
                                                        //PerkProjectedAmt = Math.Round(PerkProjectedAmt, 0);

                                                        //PerkActualAmt = PerkActualAmt + ((MonthlyInterest / item.IntRate) * IntDiff);
                                                        //PerkActualAmt = Math.Round(PerkActualAmt, 0);
                                                    }
                                                    else
                                                    {
                                                        PerkProjectedAmt = PerkActualAmt + ((Interestoutstnd / item.IntRate) * IntDiff);
                                                        mTempDate = mEndDate.AddMonths(1);
                                                        break;
                                                    }

                                                }

                                            }
                                        }

                                    }


                                }

                                if (PerktranstData == null)
                                {
                                    List<PerkTransT> PerkTransTEmp = new List<PerkTransT>();
                                    PerkTransT PerkTrans = new PerkTransT()
                                    {
                                        //ActualAmount = PerkActualAmt,
                                        ActualAmount = Math.Round(PerkAmtuploadmonth + 0.001, 0),
                                        ProjectedAmount =Math.Round(PerkProjectedAmt+0.001,0),
                                        PayMonth = LoanPayMonth,
                                        SalaryHead = chksalhead,
                                        FinancialYear = db.Calendar.Find(OFinancia.Id),
                                        DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                                    };
                                    db.PerkTransT.Add(PerkTrans);
                                    db.SaveChanges();
                                    PerkTransTEmp.Add(PerkTrans);

                                    var aa = db.EmployeePayroll.Find(OEmployeePayroll.Id);
                                    if (aa.PerkTransT != null)
                                    {
                                        PerkTransTEmp.AddRange(aa.PerkTransT);
                                    }

                                    aa.PerkTransT = PerkTransTEmp;
                                    //OEmployeePayroll.DBTrack = dbt;

                                    db.EmployeePayroll.Attach(aa);
                                    db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    db.Entry(aa).State = System.Data.Entity.EntityState.Detached;

                                }
                                else
                                {
                                    PerktranstData.ActualAmount = Math.Round(PerkAmtuploadmonth + 0.001, 0);
                                    PerktranstData.ProjectedAmount = Math.Round(PerkProjectedAmt+0.001,0);
                                    PerktranstData.SalaryHead = chksalhead;
                                    PerktranstData.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                    db.Entry(PerktranstData).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                }

                                if (paysc.SalaryHead.Frequency.LookupVal.ToUpper() == "MONTHLY")
                                {
                                    var empsalstr = db.EmpSalStruct.Include(e => e.EmpSalStructDetails)
                                        .Where(e => e.EndDate == null && e.EmployeePayroll_Id == OEmployeePayroll.Id).FirstOrDefault();

                                    int ids = empsalstr.EmpSalStructDetails.Where(q => q.SalaryHead_Id == chksalhead.Id)
                                                   .FirstOrDefault().Id;


                                    EmpSalStructDetails EmpSalStructDet = db.EmpSalStructDetails
                                                                      .Where(e => e.Id == ids).FirstOrDefault();

                                    EmpSalStructDet.DBTrack = new DBTrack
                                    {
                                        CreatedBy = EmpSalStructDet.DBTrack.CreatedBy == null ? null : EmpSalStructDet.DBTrack.CreatedBy,
                                        CreatedOn = EmpSalStructDet.DBTrack.CreatedOn == null ? null : EmpSalStructDet.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };
                                    EmpSalStructDet.Amount = PerkActualAmt;
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

                }
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}