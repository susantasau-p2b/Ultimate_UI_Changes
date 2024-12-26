
using P2b.Global;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using P2BUltimate.Process;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Payroll;
using P2BUltimate.Security;
using System.Diagnostics;
using System.IO;

namespace P2BUltimate.Controllers.Payroll.MainController
{
    public class PFECRSummaryRController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/PFECRSummaryR/Index.cshtml");
        }

        public ActionResult P2BGrid(P2BGrid_Parameters gp)
        {
            try
            {
                DataBaseContext db = new DataBaseContext();
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                var jsonData = (Object)null;
                IEnumerable<PFECRSummaryR> PFECRSummaryR = null;
                if (gp.IsAutho == true)
                {
                    PFECRSummaryR = db.PFECRSummaryR.Include(e => e.PFECRR).Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    PFECRSummaryR = db.PFECRSummaryR.Include(e => e.PFECRR).AsNoTracking().ToList();
                }

                IEnumerable<PFECRSummaryR> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = PFECRSummaryR;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.Wage_Month.ToString().Contains(gp.searchString))
                            || (e.Return_Month.ToString().Contains(gp.searchString))
                            || (e.Total_UANs.ToString().Contains(gp.searchString))
                            || (e.ECRPaymentReleaseDate.ToString().Contains(gp.searchString))
                            || (e.SalECRFileName.ToUpper().Contains(gp.searchString.ToUpper()))
                            || (e.Id.ToString().Contains(gp.searchString))
                            ).Select(a => new { a.Wage_Month, a.Return_Month, a.Total_UANs, a.ECRPaymentReleaseDate, a.SalECRFileName, a.Id }).ToList();
                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.Wage_Month), Convert.ToString(a.Return_Month), Convert.ToString(a.Total_UANs), Convert.ToString(a.ECRPaymentReleaseDate), a.SalECRFileName, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = PFECRSummaryR;
                    Func<PFECRSummaryR, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "Wage_Month" ? c.Wage_Month.ToString() :
                                         gp.sidx == "Return_Month" ? c.Return_Month.ToString() :
                                         gp.sidx == "Total_UANs" ? c.Total_UANs.ToString() :
                                         gp.sidx == "ECRPaymentReleaseDate" ? c.ECRPaymentReleaseDate.ToString() :
                                         gp.sidx == "ECRFileName" ? c.SalECRFileName.ToString() : ""
                                         );
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.Wage_Month), Convert.ToString(a.Return_Month), Convert.ToString(a.Total_UANs), Convert.ToString(a.ECRPaymentReleaseDate), a.SalECRFileName, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Wage_Month == null ? "" : Convert.ToString(a.Wage_Month), a.Return_Month == null ? "" : Convert.ToString(a.Return_Month), a.Total_UANs == null ? "" : Convert.ToString(a.Total_UANs), a.ECRPaymentReleaseDate == null ? "" : Convert.ToString(a.ECRPaymentReleaseDate), a.SalECRFileName, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Wage_Month == null ? "" : Convert.ToString(a.Wage_Month), a.Return_Month == null ? "" : Convert.ToString(a.Return_Month), a.Total_UANs == null ? "" : Convert.ToString(a.Total_UANs), a.ECRPaymentReleaseDate == null ? "" : Convert.ToString(a.ECRPaymentReleaseDate), a.SalECRFileName, a.Id }).ToList();
                    }
                    totalRecords = PFECRSummaryR.Count();
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

        [HttpPost]
        public ActionResult Edit(int data)
        {
            //string tableName = "Corporate";

            //    // Fetch the table records dynamically
            //    var tableData = db.GetType()
            //    .GetProperty(tableName)
            //    .GetValue(db, null);
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.PFECRSummaryR
                    .Include(e => e.PFECRR)
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        Administrative_Charges_AC2 = e.Administrative_Charges_AC2,
                        Administrative_Charges_AC22 = e.Administrative_Charges_AC22,
                        SalECRFileName = e.SalECRFileName,
                        ArrECRFileName = e.ArrECRFileName,
                        RetECRFileName = e.RetECRFileName,
                        JoinECRFileName = e.JoinECRFileName,
                        ECRPaymentReleaseDate = e.ECRPaymentReleaseDate,
                        ECRProcessDate = e.ECRProcessDate,
                        EDLI_Contribution_AC21 = e.EDLI_Contribution_AC21,
                        Establishment_ID = e.Establishment_ID,
                        Establishment_Name = e.Establishment_Name,
                        Exemption_Status = e.Exemption_Status,
                        Inspection_Charges_AC2 = e.Inspection_Charges_AC2,
                        Inspection_Charges_AC22 = e.Inspection_Charges_AC22,
                        Return_Month = e.Return_Month,
                        Total_Employees = e.Total_Employees,
                        Total_Employees_Excluded = e.Total_Employees_Excluded,
                        Total_Gross_Wages_Excluded = e.Total_Employees_Excluded,
                        Total_UANs = e.Total_UANs,
                        Wage_Month = e.Wage_Month,
                        Action = e.DBTrack.Action
                    }).ToList();

                //var filename = db.PFECRSummaryR.Where(e => e.Id == data).Select(e => e.ArrECRFileName).SingleOrDefault();
                //StreamReader sr = new StreamReader(Path.Combine(Server.MapPath("~"), @filename));
                //String FileText = sr.ReadToEnd().ToString();
                //System.Diagnostics.Process.Start("notepad.exe", @filename);
                //sr.Close();


                var Corp = db.PFECRSummaryR.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                Session["Id"] = Corp.Id;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, "", "", Auth, JsonRequestBehavior.AllowGet });
            }
        }

        public List<int> one_ids(string form)
        {
            string itsec_id = form;
            List<int> ids = itsec_id.Split(',').Select(e => int.Parse(e)).ToList();
            return (ids);
        }
        public ActionResult Create(PFECRSummaryR P, FormCollection form) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    int compId = 0;
                    compId = Convert.ToInt32(Session["CompId"]);
                    int WageMonth = form["WageMonth_drop"] == "0" ? 0 : Convert.ToInt32(form["WageMonth_drop"]);

                    var salt = db.PFECRR.Where(e => e.Id == WageMonth).SingleOrDefault();

                    string mPayMonth = salt.Wage_Month;
                    string mReturnMonth = P.Return_Month;

                    /////new added
                    Employee OEmployee = null;
                    EmployeePayroll OEmployeePayroll = null;
                    List<string> MsgCheck = new List<string>();
                    
                    var ids = db.Employee.Select(e => e.Id).ToList();

                    foreach (var i in ids)
                    {
                        OEmployee = db.Employee.Include(q => q.EmpName).Where(r => r.Id == i).AsNoTracking().AsParallel().SingleOrDefault();
                        OEmployeePayroll = db.EmployeePayroll.Include(e => e.Employee.ServiceBookDates).Where(e => e.Employee.Id == OEmployee.Id).AsNoTracking().AsParallel().SingleOrDefault();

                        if (OEmployeePayroll.Employee.ServiceBookDates.RetirementDate.Value.ToString("MM/yyyy") == mPayMonth && OEmployeePayroll.Employee.ServiceBookDates.ServiceLastDate == null)
                        {
                            {
                                MsgCheck.Add("Please Extend the Retirement Date Or Do Retired or Resign Activity First in this month for employee : " + OEmployee.EmpCode + " " + OEmployee.EmpName.FullNameFML);
                            }
                        }
                    }
                    if (MsgCheck.Count > 0)
                    {
                        return Json(new Utility.JsonReturnClass { success = false, responseText = MsgCheck }, JsonRequestBehavior.AllowGet);
                    }
                    ////

                    using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                       new System.TimeSpan(0, 30, 0)))
                    {

                        using (DataBaseContext db2 = new DataBaseContext())
                        {
                            //if (db.PFECRSummaryR.Any(o => o.Wage_Month == mPayMonth))
                            //{
                            //    return Json(new Object[] { "", "", "PFECR For This Month Already Generated.", JsonRequestBehavior.AllowGet });
                            //}
                            PayrollReportGen.GeneratePFECR(compId, mPayMonth, mReturnMonth);

                        }
                        ts.Complete();
                        //return Json(new Object[] { "", "", "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
                        // List<string> Msgs = new List<string>();
                        Msg.Add("Data Saved successfully");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception ex)
                {

                    // List<string> Msg = new List<string>();
                    Msg.Add(ex.Message);
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
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                }

            }
        }

        public ActionResult ReleaseProcess(string forwardata, string PayMonth, int data, PFECRSummaryR P)
        {
            List<int> ids = null;
            using (DataBaseContext db = new DataBaseContext())
            {
                if (forwardata != null && forwardata != "0" && forwardata != "false")
                {
                    ids = Utility.StringIdsToListIds(forwardata);
                }
                var value = P.ECRPaymentReleaseDate;
                using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                   new System.TimeSpan(0, 30, 0)))
                {
                    try
                    {
                        using (DataBaseContext db2 = new DataBaseContext())
                        {

                            var SalT = db.PFECRSummaryR.Include(e => e.PFECRR).Where(e => e.Id == data).SingleOrDefault();

                            if (SalT != null)
                            {
                                SalT.ECRPaymentReleaseDate = P.ECRPaymentReleaseDate;
                                db.PFECRSummaryR.Attach(SalT);
                                db.Entry(SalT).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                TempData["RowVersion"] = SalT.RowVersion;
                                db.Entry(SalT).State = System.Data.Entity.EntityState.Detached;
                            }
                        }
                        ts.Complete();
                        return Json(new { success = true, responseText = "PFECRSummary released ." }, JsonRequestBehavior.AllowGet);
                    }
                    catch (Exception ex)
                    {
                        List<string> Msg = new List<string>();
                        Msg.Add(ex.Message);
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
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
        }

        public ActionResult GetWageMonth(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                var salt = db.SalaryT.Include(e => e.PFECRR).Where(e => e.ProcessDate != null && e.PFECRR != null).AsNoTracking().AsParallel().ToList();
                var fall = salt.Select(e => e.PFECRR).GroupBy(e => e.Wage_Month, (key, e) => e.FirstOrDefault());

                var selected = (Object)null;
                if (data != "" && data != "0" && data != "0")
                {
                    selected = Convert.ToInt32(data);
                }

                SelectList s = new SelectList(fall, "Id", "Wage_Month", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }


        }


        public ActionResult ViewArrEcrFile()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    int id = Convert.ToInt32(Session["Id"]);
                    string FilePath = db.PFECRSummaryR.Where(e => e.Id == id).Select(e => e.ArrECRFileName).SingleOrDefault();

                    string filename = FilePath.Substring(FilePath.LastIndexOf("\\") + 1);
                    string content = string.Empty;
                    using (var stream = new StreamReader(Server.MapPath("~/PFECRRFile/" + filename)))
                    {
                        content = stream.ReadToEnd();
                        System.Diagnostics.Process.Start("notepad.exe", Server.MapPath("~/PFECRRFile/" + filename));
                    }

                    //return View("~/Views/Payroll/MainViews/PFECRSummaryR/Index.cshtml");
                    return RedirectToAction("Index", "PFECRSummaryR");
                }
                catch (Exception ex)
                {
                    List<string> Msg = new List<string>();
                    Msg.Add(ex.Message);
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
                    //return View("~/Views/Payroll/MainViews/PFECRSummaryR/Index.cshtml");
                    return RedirectToAction("Index", "PFECRSummaryR");
                }
            }
        }

        public ActionResult ViewSalEcrFile()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    int id = Convert.ToInt32(Session["Id"]);
                    string FilePath = db.PFECRSummaryR.Where(e => e.Id == id).Select(e => e.SalECRFileName).SingleOrDefault();
                    string filename = FilePath.Substring(FilePath.LastIndexOf("\\") + 1);
                    string content = string.Empty;
                    using (var stream = new StreamReader(Server.MapPath("~/PFECRRFile/" + filename)))
                    {
                        content = stream.ReadToEnd();
                        System.Diagnostics.Process.Start("notepad.exe", Server.MapPath("~/PFECRRFile/" + filename));
                    }

                    // return View("~/Views/Payroll/MainViews/PFECRSummaryR/Index.cshtml");
                    return RedirectToAction("Index", "PFECRSummaryR");

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
                    //return View("~/Views/Payroll/MainViews/PFECRSummaryR/Index.cshtml");
                    return RedirectToAction("Index", "PFECRSummaryR");
                }
            }
        }

        public string Download(int id)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {

                    string FilePath = db.PFECRSummaryR.Where(e => e.Id == id).Select(e => e.SalECRFileName).SingleOrDefault();

                    string filename = FilePath.Substring(FilePath.LastIndexOf("\\") + 1);
                    //string content = string.Empty;
                    //using (var stream = new StreamReader(Server.MapPath("~/PFECRRFile/" + filename)))
                    //{
                    //    content = stream.ReadToEnd();
                    //    //System.Diagnostics.Process.Start("notepad.exe", Server.MapPath("~/PFECRRFile/" + filename));
                    //}
                    //using (WebClient wc = new WebClient())
                    //{
                    //   // wc.DownloadProgressChanged += wc_DownloadProgressChanged;
                    //    wc.DownloadFileAsync(new System.Uri(filename),
                    //    "");
                    //}
                    //return View("~/Views/Payroll/MainViews/PFECRSummaryR/Index.cshtml");
                    return FilePath;
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
                    //return View("~/Views/Payroll/MainViews/PFECRSummaryR/Index.cshtml");
                    return null;
                    //return RedirectToAction("Index", "PFECRSummaryR");
                }
            }
        }

        public ActionResult DownloadFile(string fileName)
        {
            string localPath = new Uri(fileName).LocalPath;
            System.IO.FileInfo file = new System.IO.FileInfo(localPath);
            if (file.Exists)
                return File(file.FullName, "text/plain", file.Name + " ");
            else
                return HttpNotFound();
        }

        public string DownloadArrECRFileName(int id)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string FilePath = db.PFECRSummaryR.Where(e => e.Id == id).Select(e => e.ArrECRFileName).SingleOrDefault();
                    string filename = FilePath.Substring(FilePath.LastIndexOf("\\") + 1);
                    //string content = string.Empty;
                    //using (var stream = new StreamReader(Server.MapPath("~/PFECRRFile/" + filename)))
                    //{
                    //    content = stream.ReadToEnd();
                    //    //System.Diagnostics.Process.Start("notepad.exe", Server.MapPath("~/PFECRRFile/" + filename));
                    //}
                    //using (WebClient wc = new WebClient())
                    //{
                    //   // wc.DownloadProgressChanged += wc_DownloadProgressChanged;
                    //    wc.DownloadFileAsync(new System.Uri(filename),
                    //    "");
                    //}
                    //return View("~/Views/Payroll/MainViews/PFECRSummaryR/Index.cshtml");

                    return FilePath;
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
                    //return View("~/Views/Payroll/MainViews/PFECRSummaryR/Index.cshtml");
                    return null;
                    //return RedirectToAction("Index", "PFECRSummaryR");
                }
            }
        }

        public class Form5Class
        {
            public String UAN { get; set; }
            public String Previous_Member_Id { get; set; }
            public String Member_Name { get; set; }
            public String Date_of_Birth { get; set; }
            public String Date_of_Joining { get; set; }
            public String Gender { get; set; }
            public String Father_Husband_Name { get; set; }
            public String Relationship { get; set; }
            public String MobileNumber { get; set; }
            public String EmailId { get; set; }
            public String Nationality { get; set; }
            public String Wages_as_on_Joining { get; set; }
            public String Qualification { get; set; }
            public String Marital_Status { get; set; }
            public String Is_International_Worker { get; set; }
            public String Country_Of_Origin { get; set; }
            public String Passport_Number { get; set; }
            public String Passport_Valid_FromDate { get; set; }
            public String Passport_Valid_ToDate { get; set; }
            public String Is_Physical_Handicap { get; set; }
            public String Locomotive { get; set; }
            public String Hearing { get; set; }
            public String Visual { get; set; }
            public String Bank_Account_Number { get; set; }
            public String IFSC { get; set; }
            public String Name_as_per_BankDetails { get; set; }
            public String PAN { get; set; }
            public String Name_as_on_PAN { get; set; }
            public String AADHAAR_Number { get; set; }
            public String Name_as_on_AADHAAR { get; set; }

        }

        public ActionResult CreateECRFileForm5()
        {
            //string path = @"F:\ECR_PF_" + Convert.ToDateTime("01/" + mPayMonth).ToString("MMyyyy") + ".txt";
            string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\PFECRRFile";
            bool exists = System.IO.Directory.Exists(requiredPath);
            string localPath;
            if (!exists)
            {
                localPath = new Uri(requiredPath).LocalPath;
                System.IO.Directory.CreateDirectory(localPath);
            }
            string path = requiredPath + @"\ECR_FORM5.txt";
            localPath = new Uri(path).LocalPath;
            List<Form5Class> OForm5class = new List<Form5Class>();
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                var OEmpInfoData_Q = db.EmployeePayroll
                       .Include(e => e.Employee)
                       .Include(e => e.Employee.EmpOffInfo)
                       .Include(e => e.Employee.FatherName)
                       .Include(e => e.Employee.HusbandName)
                       .Include(e => e.Employee.EmpOffInfo.NationalityID)
                       .Include(e => e.Employee.EmpOffInfo.Branch)
                       .Include(e => e.Employee.PerContact)
                       .Include(e => e.Employee.PerContact.ContactNumbers)
                       .Include(e => e.Employee.PerAddr)
                       .Include(e => e.Employee.PerAddr.Country)
                       .Include(e => e.Employee.PassportDetails)
                       .Include(e => e.Employee.EmpName)
                       .Include(e => e.Employee.Gender)
                       .Include(e => e.Employee.MaritalStatus)
                       .Include(e => e.Employee.EmpOffInfo)
                       .Include(e => e.Employee.ServiceBookDates)
                    // .Where(q => q.Employee.EmpOffInfo.NationalityID.UANNo == "0" || q.Employee.EmpOffInfo.NationalityID.UANNo == null)
                       .AsNoTracking().AsParallel().ToList();
                var OEmpInfoData_ = OEmpInfoData_Q.Where(q => (q.Employee.EmpOffInfo.NationalityID.UANNo == "0" || q.Employee.EmpOffInfo.NationalityID.UANNo == null) && q.Employee.ServiceBookDates.ServiceLastDate == null).ToList();
                if (OEmpInfoData_.Count == 0)
                {
                    //return this.Json(new { success = false, responseText = "There is no employee to update." }, JsonRequestBehavior.AllowGet);
                    Msg.Add("There is no employee to update");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
               // OEmpInfoData_.Where(q=>q.Employee.ServiceBookDates.ServiceLastDate==null)
                foreach (var OEmpInfoData_t in OEmpInfoData_)
                {
                    //var check =OEmpInfoData_t.Employee.ServiceBookDates.ServiceLastDate;
                    //if (check!=null)
                    //{
                    //    //Msg.Add(OEmpInfoData_t.Employee.EmpCode +" is a retire");
                    //    continue;
                    //}
                    EmpOff EmpOffInfo = OEmpInfoData_t.Employee.EmpOffInfo;

                    //Form5Class frm5 = new Form5Class
                    //{
                    //    UAN ="0",
                    //    Previous_Member_Id="0",
                    //    Member_Name=OEmpInfoData_t.Employee.EmpName.FName,
                    //    Date_of_Birth = OEmpInfoData_t.Employee.ServiceBookDates!=null?OEmpInfoData_t.Employee.ServiceBookDates.BirthDate.Value.ToShortDateString():"",
                    //    Date_of_Joining = OEmpInfoData_t.Employee.ServiceBookDates!=null?OEmpInfoData_t.Employee.ServiceBookDates.JoiningDate.Value.ToShortDateString():"",
                    //    Gender=OEmpInfoData_t.Employee.Gender!=null?OEmpInfoData_t.Employee.Gender.LookupVal.Take(1).ToString():"",
                    //    Father_Husband_Name = OEmpInfoData_t.Employee.HusbandName != null ? OEmpInfoData_t.Employee.HusbandName.FName : OEmpInfoData_t.Employee.FatherName != null ? OEmpInfoData_t.Employee.FatherName.FName : "",
                    //    Relationship = OEmpInfoData_t.Employee.HusbandName != null ? "H" : OEmpInfoData_t.Employee.FatherName != null ? "F" : "",
                    //    MobileNumber = OEmpInfoData_t.Employee.PerContact!=null &&OEmpInfoData_t.Employee.PerContact.ContactNumbers!=null? OEmpInfoData_t.Employee.PerContact.ContactNumbers.FirstOrDefault().MobileNo:"",
                    //    EmailId = OEmpInfoData_t.Employee.PerContact != null? OEmpInfoData_t.Employee.PerContact .EmailId:"",
                    //    Nationality = OEmpInfoData_t.Employee.PerAddr != null &&  OEmpInfoData_t.Employee.PerAddr.Country!=null? OEmpInfoData_t.Employee.PerAddr.Country.CurrencyDesc:"",
                    //    Wages_as_on_Joining="",
                    //    Qualification="",
                    //    Marital_Status = OEmpInfoData_t.Employee.MaritalStatus != null ? OEmpInfoData_t.Employee.MaritalStatus.LookupVal.Take(1).ToString() : "",
                    //    Is_International_Worker="",
                    //    Country_Of_Origin="",
                    //    Passport_Number = OEmpInfoData_t.Employee.PassportDetails != null ? OEmpInfoData_t.Employee.PassportDetails.FirstOrDefault().PassportNo : "",
                    //    Passport_Valid_FromDate = OEmpInfoData_t.Employee.PassportDetails != null ? OEmpInfoData_t.Employee.PassportDetails.FirstOrDefault().IssueDate.Value.ToShortDateString() : "",
                    //    Passport_Valid_ToDate = OEmpInfoData_t.Employee.PassportDetails != null ? OEmpInfoData_t.Employee.PassportDetails.FirstOrDefault().ExpiryDate.Value.ToShortDateString() : "",
                    //    Is_Physical_Handicap = EmpOffInfo!=null?EmpOffInfo.SelfHandicap.ToString():"",
                    //    Locomotive="",
                    //    Hearing="",
                    //    Visual="",
                    //    Bank_Account_Number = EmpOffInfo!=null?EmpOffInfo.AccountNo:"",
                    //    IFSC = EmpOffInfo != null && EmpOffInfo.Branch != null ? EmpOffInfo.Branch.IFSCCode : "",
                    //    //Name_as_per_BankDetails = OEmpInfoData_t.EmpOffInfo!=null?EmpOffInfo.,
                    //    PAN = EmpOffInfo != null && EmpOffInfo.NationalityID != null ? EmpOffInfo.NationalityID.PANNo : "",
                    //    //Name_as_on_PAN=
                    //    AADHAAR_Number = EmpOffInfo != null && EmpOffInfo.NationalityID != null ? EmpOffInfo.NationalityID.AdharNo : "",
                    //    //Name_as_on_AADHAAR=OEmpInfoData_t.Employee.EmpOffInfo.
                    //};
                    string data = "";
                    string UAN = "0";
                    string Previous_Member_Id = "0";
                    string Member_Name = OEmpInfoData_t.Employee.EmpName.FullNameFML.Replace(" ", "");
                    string Date_of_Birth = OEmpInfoData_t.Employee.ServiceBookDates != null ? OEmpInfoData_t.Employee.ServiceBookDates.BirthDate.Value.ToShortDateString() : "";
                    if (Date_of_Birth == "")
                    {
                        data = "Date of Birth## ";
                    }
                    string Date_of_Joining = OEmpInfoData_t.Employee.ServiceBookDates != null ? OEmpInfoData_t.Employee.ServiceBookDates.JoiningDate.Value.ToShortDateString() : "";
                    if (Date_of_Joining == "")
                    {
                        data = data + "Date of Joining## ";
                    }
                    string Gender = OEmpInfoData_t.Employee.Gender != null ? OEmpInfoData_t.Employee.Gender.LookupVal[0].ToString() : "";
                    if (Gender == "")
                    {
                        data = data + "Gender## ";
                    }
                    string Father_Husband_Name = OEmpInfoData_t.Employee.HusbandName != null ? OEmpInfoData_t.Employee.HusbandName.FullNameFML.Replace(" ", "") 
                        : OEmpInfoData_t.Employee.FatherName != null ? OEmpInfoData_t.Employee.FatherName.FullNameFML.Replace(" ", "") : "";
                    if (Father_Husband_Name == "")
                    {
                        data = data + "Father/Husband Name## ";
                    }
                    string Relationship = OEmpInfoData_t.Employee.HusbandName != null ? "H" : OEmpInfoData_t.Employee.FatherName != null ? "F" : "";
                    string MobileNumber = OEmpInfoData_t.Employee.PerContact != null && OEmpInfoData_t.Employee.PerContact.ContactNumbers != null ? OEmpInfoData_t.Employee.PerContact.ContactNumbers.FirstOrDefault().MobileNo : "";
                    string EmailId = OEmpInfoData_t.Employee.PerContact != null ? OEmpInfoData_t.Employee.PerContact.EmailId : "";
                    string Nationality = OEmpInfoData_t.Employee.PerAddr != null && OEmpInfoData_t.Employee.PerAddr.Country != null ? OEmpInfoData_t.Employee.PerAddr.Country.Name : "";
                    if (Nationality == "")
                    {
                        data = data + "Nationality## ";
                    }
                    else if (Nationality == "INDIA")
                    {
                        Nationality = "INDIAN";
                    }
                    string Wages_as_on_Joining = "";
                    string Qualification = "";
                    string Marital_Status = OEmpInfoData_t.Employee.MaritalStatus != null ? OEmpInfoData_t.Employee.MaritalStatus.LookupVal.Substring(0,1) : "";
                    if (Marital_Status == "")
                    {
                        data = data + "Marital Status## ";
                    }
                    string Is_International_Worker = "N";
                    string Country_Of_Origin = "";
                    //if (Is_International_Worker == "")
                    //{
                    //    data = "Date of Joining## ";
                    //}
                    string Passport_Number = OEmpInfoData_t.Employee.PassportDetails.Count>0 ? OEmpInfoData_t.Employee.PassportDetails.FirstOrDefault().PassportNo : "";
                    //if (Date_of_Joining == "")
                    //{
                    //    data = "Date of Joining## ";
                    //}
                    string Passport_Valid_FromDate = OEmpInfoData_t.Employee.PassportDetails.Count > 0 ? OEmpInfoData_t.Employee.PassportDetails.FirstOrDefault().IssueDate.Value.ToShortDateString() : "";
                    //if (Date_of_Joining == "")
                    //{
                    //    data = "Date of Joining## ";
                    //}
                    string Passport_Valid_ToDate = OEmpInfoData_t.Employee.PassportDetails.Count > 0 ? OEmpInfoData_t.Employee.PassportDetails.FirstOrDefault().ExpiryDate.Value.ToShortDateString() : "";
                    //if (Date_of_Joining == "")
                    //{
                    //    data = "Date of Joining## ";
                    //}
                    string OIs_Physical_Handicap = EmpOffInfo != null ? EmpOffInfo.SelfHandicap.ToString() : "";
                    string Is_Physical_Handicap = "";
                    if (OIs_Physical_Handicap == "False")
                    {
                        Is_Physical_Handicap = "N";
                    }
                    else 
                    {
                        Is_Physical_Handicap = "Y";
                    }
                    string Locomotive = "";
                    string Hearing = "";
                    string Visual = "";
                    string Bank_Account_Number = EmpOffInfo != null ? EmpOffInfo.AccountNo : "";
                    string IFSC = EmpOffInfo != null && EmpOffInfo.Branch != null ? EmpOffInfo.Branch.IFSCCode : "";
                    //Name_as_per_BankDetails = OEmpInfoData_t.EmpOffInfo!=null?EmpOffInfo.;
                    string PAN = EmpOffInfo != null && EmpOffInfo.NationalityID != null ? EmpOffInfo.NationalityID.PANNo : "";
                    //Name_as_on_PAN=
                    string AADHAAR_Number = EmpOffInfo != null && EmpOffInfo.NationalityID != null ? EmpOffInfo.NationalityID.AdharNo : "";
                    //Name_as_on_AADHAAR=OEmpInfoData_t.Employee.EmpOffInfo.
                    if (data != "")
                    {
                        data = data.Replace("##", ",");
                        Msg.Add("Provide " + data + " for Employee Code" +OEmpInfoData_t.Employee.EmpCode );
                    }
                    else
                    {
                        Form5Class frm5 = new Form5Class
                        {
                            UAN = "0",
                            Previous_Member_Id = "0",
                            Member_Name = Member_Name,
                            Date_of_Birth = Date_of_Birth,
                            Date_of_Joining = Date_of_Joining,
                            Gender = Gender,
                            Father_Husband_Name = Father_Husband_Name,
                            Relationship = Relationship,
                            MobileNumber = MobileNumber,
                            EmailId = EmailId,
                            Nationality = Nationality,
                            Wages_as_on_Joining = Wages_as_on_Joining,
                            Qualification = Qualification,
                            Marital_Status = Marital_Status,
                            Is_International_Worker = "N",
                            Country_Of_Origin = "",
                            Passport_Number = Passport_Number,
                            Passport_Valid_FromDate = Passport_Valid_FromDate,
                            Passport_Valid_ToDate = Passport_Valid_ToDate,
                            Is_Physical_Handicap = Is_Physical_Handicap,
                            Locomotive = "",
                            Hearing = "",
                            Visual = "",
                            Bank_Account_Number = Bank_Account_Number,
                            IFSC = IFSC,
                            //Name_as_per_BankDetails = OEmpInfoData_t.EmpOffInfo!=null?EmpOffInfo.,
                            PAN = PAN,
                            //Name_as_on_PAN=
                            AADHAAR_Number = AADHAAR_Number,
                            //Name_as_on_AADHAAR=OEmpInfoData_t.Employee.EmpOffInfo.
                        };
                        OForm5class.Add(frm5);

                    }
                }
            }
            if (Msg.Count > 0 )
            {
                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
            }
            if (!System.IO.File.Exists(localPath))
            {
                FileStream fs = new FileStream(localPath, FileMode.OpenOrCreate);
                StreamWriter str = new StreamWriter(fs);
                str.BaseStream.Seek(0, SeekOrigin.Begin);
                foreach (var ca in OForm5class)
                {
                    str.WriteLine(ca.UAN + "#~#" + ca.Previous_Member_Id + "#~#" + ca.Member_Name + "#~#"
                        + ca.Date_of_Birth + "#~#" + ca.Date_of_Joining + "#~#" + ca.Gender + "#~#"
                        + ca.Father_Husband_Name + "#~#" + ca.Relationship + "#~#" + ca.MobileNumber + "#~#" + ca.EmailId + "#~#" + ca.Nationality
                        + "#~#" + ca.Wages_as_on_Joining + "#~#" + ca.Qualification + "#~#" + ca.Marital_Status + "#~#" + ca.Is_International_Worker
                        + "#~#" + ca.Country_Of_Origin + "#~#" + ca.Passport_Number + "#~#" + ca.Passport_Valid_FromDate + "#~#" + ca.Passport_Valid_ToDate
                        + "#~#" + ca.Is_Physical_Handicap + "#~#" + ca.Locomotive + "#~#" + ca.Hearing + "#~#" + ca.Visual
                        + "#~#" + ca.Bank_Account_Number + "#~#" + ca.IFSC + "#~#" + ca.Name_as_per_BankDetails + "#~#" + ca.PAN
                        + "#~#" + ca.Name_as_on_PAN + "#~#" + ca.AADHAAR_Number + "#~#" + ca.Name_as_on_AADHAAR
                       );
                }
                str.Flush();
                str.Close();
                fs.Close();
                List<string> Msgs = new List<string>();
                Msg.Add("File generated successfully");
                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                // return path;

            }
            else if (System.IO.File.Exists(localPath))
            {
                System.IO.File.Delete(localPath);
                FileStream fs = new FileStream(localPath, FileMode.OpenOrCreate);
                StreamWriter str = new StreamWriter(fs);
                str.BaseStream.Seek(0, SeekOrigin.Begin);
                foreach (var ca in OForm5class)
                {
                    str.WriteLine(ca.UAN + "#~#" + ca.Previous_Member_Id + "#~#" + ca.Member_Name + "#~#"
                       + ca.Date_of_Birth + "#~#" + ca.Date_of_Joining + "#~#" + ca.Gender + "#~#"
                       + ca.Father_Husband_Name + "#~#" + ca.Relationship + "#~#" + ca.MobileNumber + "#~#" + ca.EmailId + "#~#" + ca.Nationality
                       + "#~#" + ca.Wages_as_on_Joining + "#~#" + ca.Qualification + "#~#" + ca.Marital_Status + "#~#" + ca.Is_International_Worker
                       + "#~#" + ca.Country_Of_Origin + "#~#" + ca.Passport_Number + "#~#" + ca.Passport_Valid_FromDate + "#~#" + ca.Passport_Valid_ToDate
                       + "#~#" + ca.Is_Physical_Handicap + "#~#" + ca.Locomotive + "#~#" + ca.Hearing + "#~#" + ca.Visual
                       + "#~#" + ca.Bank_Account_Number + "#~#" + ca.IFSC + "#~#" + ca.Name_as_per_BankDetails + "#~#" + ca.PAN
                       + "#~#" + ca.Name_as_on_PAN + "#~#" + ca.AADHAAR_Number + "#~#" + ca.Name_as_on_AADHAAR
                      );

                }

                str.Flush();
                str.Close();
                fs.Close();
                List<string> Msgs = new List<string>();
                Msg.Add("Data Saved successfully");
                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                //  return localPath;
            }
            return null;
        }

        public ActionResult DownloadFileArrECRFileName(string fileName)
        {
            string localPath = new Uri(fileName).LocalPath;
            System.IO.FileInfo file = new System.IO.FileInfo(localPath);
            if (file.Exists)
                return File(file.FullName, "text/plain", file.Name + " ");
            else
                return Content("File Not Found");

        }
    }
}