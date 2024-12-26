using P2BUltimate.App_Start;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.IO;
using P2BUltimate.Models;
using Payroll;
using P2b.Global;
using System.Transactions;
using P2BUltimate.Security;
using System.Data;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Data.Entity.Infrastructure;

namespace P2BUltimate.Controllers.Payroll.MainController
{
    public class ITsection24PaymentLayOutController : Controller
    {
        //
        // GET: /ITsection24PaymentLayOut/
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/ITsection24PaymentLayOut/Index.cshtml");
        }

        public ActionResult partial()
        {
            return View("~/Views/Shared/Payroll/_ITSection24Payment.cshtml");
        }
        public ActionResult GetITSectionByDefault()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.ITSection.Include(e => e.ITInvestments)
                    .Include(e => e.ITSectionList)
                    .Include(e => e.ITSectionListType)
                    .Where(e => e.ITSectionListType.LookupVal.ToUpper() == "SECTION24" && e.ITSectionListType.LookupVal.ToUpper() == "PROPERTY").ToList();
                var returnpara = new
                {
                    Id = fall.Select(a => a.Id.ToString()).ToArray(),
                    FullDetails = fall.Select(a => a.FullDetails).ToArray()
                };

                return Json(returnpara, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetITSectionLKDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.ITSection.Include(e => e.ITSectionList).Include(e => e.ITSectionListType)
                       .Where(e => e.ITSectionList.LookupVal.ToUpper() == "SECTION24" && e.ITSectionListType.LookupVal.ToUpper() == "PROPERTY")
                       .ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.ITSection.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }
                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GridDelete(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var LvEP = db.ITSection24Payment.Find(data);
                string deletefilepath = LvEP.Path;
                if (deletefilepath != null)
                {
                    FileInfo File = new FileInfo(deletefilepath);
                    bool exists = File.Exists;
                    if (exists)
                    {
                        System.IO.File.Delete(deletefilepath);
                    }
                }
                db.ITSection24Payment.Remove(LvEP);
                db.SaveChanges();
                //return Json(new Object[] { "", "", " Record Deleted Successfully " }, JsonRequestBehavior.AllowGet);
                List<string> Msgr = new List<string>();
                Msgr.Add("Record Deleted Successfully  ");
                return Json(new Utility.JsonReturnClass { success = true, responseText = Msgr }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult GetLoanAdvHeadLKDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.LoanAdvanceHead.ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.LoanAdvanceHead.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }
                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Create(ITSection24Payment ITSection24Payment, FormCollection form, String forwarddata) //Create submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var Emp = form["Employee-Table"] == "0" ? 0 : Convert.ToInt32(form["Employee-Table"]);
                    string ITSection = form["ITSectionlist"] == "0" ? "" : form["ITSectionlist"];
                    string LoanAdvHead = form["LoanAdvHeadlist"] == "0" ? "" : form["LoanAdvHeadlist"];
                    string FinancialYearList = form["FinancialYearList"] == "0" ? "" : form["FinancialYearList"];
                    if (FinancialYearList != null && FinancialYearList != "")
                    {
                        var value = db.Calendar.Find(int.Parse(FinancialYearList));
                        ITSection24Payment.FinancialYear = value;

                    }
                    int CompId = 0;
                    if (Session["CompId"] != null)
                        CompId = int.Parse(Session["CompId"].ToString());

                    int id = 0;
                    if (Emp != null && Emp != 0)
                        id = Emp;
                    else
                    {
                        Msg.Add("  Kindly Select Employee.  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    // return Json(new Object[] { "", "", "Kindly Select Employee." }, JsonRequestBehavior.AllowGet);

                    if (LoanAdvHead == null)
                    {
                        Msg.Add("  Kindly Select Loan Advance Head. ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    Employee OEmployee = null;
                    EmployeePayroll OEmployeePayroll = null;

                    OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct).Include(e => e.PayStruct.JobStatus)
                                    .Include(e => e.EmpOffInfo)
                                    .Where(r => r.Id == Emp).SingleOrDefault();

                    OEmployeePayroll = db.EmployeePayroll.Include(e => e.ITSection24Payment)
                        .Include(e => e.ITSection24Payment.Select(r => r.FinancialYear)).Where(e => e.Employee.Id == Emp).SingleOrDefault();

                    var calf = OEmployeePayroll.ITSection24Payment.Any(e => e.FinancialYear.Id == ITSection24Payment.FinancialYear.Id && e.InvestmentDate == ITSection24Payment.InvestmentDate);
                    if (calf == true)
                    {
                        Msg.Add("  Data Already Exist For THis Employee.  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    if (ITSection != null && ITSection != "")
                    {
                        int ITSectionId = int.Parse(ITSection);
                        ITSection24Payment.ITSection = db.ITSection.Where(e => e.Id == ITSectionId).SingleOrDefault();
                    }
                    else
                    {
                        Msg.Add("  IT Section not defined.  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    if (LoanAdvHead != null && LoanAdvHead != "")
                    {
                        int LoanAdvHeadId = int.Parse(LoanAdvHead);
                        ITSection24Payment.LoanAdvanceHead = db.LoanAdvanceHead.Where(e => e.Id == LoanAdvHeadId).SingleOrDefault();
                    }



                    using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                      new System.TimeSpan(0, 30, 0)))
                    {

                        ITSection24Payment.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName.ToString(), IsModified = false };
                        try
                        {
                            ITSection24Payment ITSection24Pay = new ITSection24Payment()
                            {
                                ActualInterest = ITSection24Payment.ActualInterest,
                                DeclaredInterest = ITSection24Payment.DeclaredInterest,
                                FinancialYear = ITSection24Payment.FinancialYear,
                                InvestmentDate = ITSection24Payment.InvestmentDate,
                                ITSection = ITSection24Payment.ITSection,
                                LoanAdvanceHead = ITSection24Payment.LoanAdvanceHead,
                                Narration = ITSection24Payment.Narration,
                                PaymentName = ITSection24Payment.PaymentName,
                                SalaryApp = ITSection24Payment.SalaryApp,
                                DBTrack = ITSection24Payment.DBTrack
                            };


                            db.ITSection24Payment.Add(ITSection24Pay);
                            db.SaveChanges();


                            List<ITSection24Payment> ITSection24PayList = new List<ITSection24Payment>();
                            ITSection24PayList.AddRange(OEmployeePayroll.ITSection24Payment);
                            ITSection24PayList.Add(ITSection24Pay);
                            OEmployeePayroll.ITSection24Payment = ITSection24PayList;
                            db.EmployeePayroll.Attach(OEmployeePayroll);
                            db.Entry(OEmployeePayroll).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            db.Entry(OEmployeePayroll).State = System.Data.Entity.EntityState.Detached;


                            ts.Complete();
                            Msg.Add("  Data Saved successfully  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            // return Json(new Object[] { "", "", "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
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
                            return Json(new { success = false, responseText = ex.InnerException }, JsonRequestBehavior.AllowGet);
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
                    Msg.Add(ex.Message);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public class returndatagridclass //Parentgrid
        {
            public string Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
            public string JoiningDate { get; set; }
            public string Job { get; set; }
            public string Grade { get; set; }
            public string Location { get; set; }
        }

        public class ITSection24PayChildDataClass //childgrid
        {
            public int Id { get; set; }
            public double ActualInterest { get; set; }
            public double DeclaredInterest { get; set; }
            public string InvestmentDate { get; set; }
            public string Narration { get; set; }
            public string PaymentName { get; set; }
            public string SalaryApp { get; set; }
        }


        public ActionResult Emp_Grid(ParamModel param, string y)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {

                    var all = db.EmployeePayroll.Include(e => e.ITSection24Payment)
                        .Include(e => e.ITSection24Payment.Select(t => t.ITSection))
                        .Include(e => e.ITSection24Payment.Select(t => t.ITSection.ITSectionList))
                        .Include(e => e.ITSection24Payment.Select(t => t.ITSection.ITSectionListType)).Include(e => e.Employee).Include(e => e.Employee.EmpName).Include(e => e.Employee.ServiceBookDates)
                        .Include(e => e.Employee.GeoStruct).Include(e => e.Employee.GeoStruct.Location.LocationObj).Include(e => e.Employee.FuncStruct)
                        .Include(e => e.Employee.FuncStruct.Job).Include(e => e.Employee.PayStruct).Include(e => e.Employee.PayStruct.Grade)
                        .Where(e => e.Employee.ServiceBookDates.ServiceLastDate == null)
                        .ToList();
                    // for searchs
                    IEnumerable<EmployeePayroll> fall;
                    if (param.sSearch == null)
                    {
                        fall = all;
                    }
                    else
                    {
                        fall = all.Where(e => e.Employee.EmpCode.ToUpper() == param.sSearch.ToUpper()).ToList();
                    }

                    //for column sorting
                    var sortindex = Convert.ToInt32(Request["iSortCol_0"]);

                    Func<EmployeePayroll, string> orderfunc = (c =>
                                                                Convert.ToUInt32(sortindex) == 0 ? c.Id.ToString() :
                                                                sortindex == 1 ? c.Employee.EmpCode : "");
                    var sortcolumn = Request["sSortDir_0"];
                    if (sortcolumn == "asc")
                    {
                        fall = fall.OrderBy(orderfunc);
                    }
                    else
                    {
                        fall = fall.OrderByDescending(orderfunc);
                    }
                    // Paging 
                    var dcompanies = fall
                            .Skip(param.iDisplayStart).Take(param.iDisplayLength).ToList();
                    if (dcompanies.Count == 0)
                    {
                        List<returndatagridclass> res = new List<returndatagridclass>();
                        foreach (var item in fall)
                        {
                            foreach (var item1 in item.ITSection24Payment)
                            {
                                if (item1.ITSection.ITSectionList.LookupVal.ToUpper() == "SECTION24" && item1.ITSection.ITSectionListType.LookupVal.ToUpper() == "PROPERTY")
                                {
                                    res.Add(new returndatagridclass
                                    {
                                        Id = item.Id.ToString(),
                                        Code = item.Employee.EmpCode,
                                        Name = item.Employee.EmpName.FullNameFML,
                                        JoiningDate = item.Employee.ServiceBookDates.JoiningDate.Value.ToString("dd/MM/yyyy"),
                                        //Job = item.Employee.FuncStruct.Job.Name,
                                        Grade = item.Employee.PayStruct != null && item.Employee.PayStruct.Grade != null ? item.Employee.PayStruct.Grade.Name : "",
                                        Location = item.Employee.GeoStruct != null && item.Employee.GeoStruct.Location != null && item.Employee.GeoStruct.Location.LocationObj != null ? item.Employee.GeoStruct.Location.LocationObj.LocDesc : ""
                                    });
                                }
                            }
                        }
                        var result = res.GroupBy(x => x.Code).Select(o => o.First()).ToList();
                        return Json(new
                        {
                            sEcho = param.sEcho,
                            iTotalRecords = all.Count(),
                            iTotalDisplayRecords = fall.Count(),
                            data = result
                        }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        var result = from c in dcompanies

                                     select new[] { null, Convert.ToString(c.Id), c.Employee.EmpCode, };
                        return Json(new
                        {
                            sEcho = param.sEcho,
                            iTotalRecords = all.Count(),
                            iTotalDisplayRecords = fall.Count(),
                            data = result
                        }, JsonRequestBehavior.AllowGet);
                    }//for data reterivation
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
                    Msg.Add(ex.Message);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public ActionResult Get_ITSection24Payment(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var db_data = db.EmployeePayroll
                        .Include(e => e.ITSection24Payment)
                        .Include(e => e.ITSection24Payment.Select(t => t.ITSection))
                        .Include(e => e.ITSection24Payment.Select(t => t.ITSection.ITSectionList))
                        .Include(e => e.ITSection24Payment.Select(t => t.ITSection.ITSectionListType))
                        .Where(e => e.Id == data).ToList();

                    if (db_data.Count > 0)
                    {
                        List<ITSection24PayChildDataClass> returndata = new List<ITSection24PayChildDataClass>();

                        foreach (var item in db_data.SelectMany(e => e.ITSection24Payment))
                        {
                            if (item.ITSection.ITSectionList.LookupVal.ToUpper() == "SECTION24" && item.ITSection.ITSectionListType.LookupVal.ToUpper() == "PROPERTY")
                            {
                                returndata.Add(new ITSection24PayChildDataClass
                                {
                                    Id = item.Id,
                                    InvestmentDate = item.InvestmentDate != null ? item.InvestmentDate.Value.ToString("dd/MM/yyyy") : "",
                                    ActualInterest = item.ActualInterest,
                                    DeclaredInterest = item.DeclaredInterest,
                                    PaymentName = item.PaymentName,
                                    SalaryApp = item.SalaryApp == true ? "YES" : "NO",
                                    Narration = item.Narration
                                });
                            }
                        }
                        return Json(returndata, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return null;
                    }

                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }

        public JsonResult EditGridDetails(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.ITSection24Payment
                     .Include(e => e.ITSection)
                    .Include(e => e.LoanAdvanceHead)
                    .Include(e => e.FinancialYear)
                     .Where(e => e.Id == data).Select
                     (e => new
                     {
                         PaymentName = e.PaymentName,
                         InvestmentDate = e.InvestmentDate,
                         ActualInterest = e.ActualInterest,
                         DeclaredInterest = e.DeclaredInterest,
                         Narration = e.Narration,
                         Action = e.DBTrack.Action
                     }).ToList();
                var ITSection24Payment = db.ITSection24Payment.Find(data);
                Session["RowVersion"] = ITSection24Payment.RowVersion;
                var Auth = ITSection24Payment.DBTrack.IsModified;
                //return Json(new Object[] { Q, add_data, "", Auth, JsonRequestBehavior.AllowGet });
                return Json(new { data = Q }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GridEditSave(ITSection24Payment ITP, FormCollection form, string data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var DeclaredInterest = form["ITSection24-DeclaredInterest"] == " 0" ? "" : form["ITSection24-DeclaredInterest"];
                    var ActualInterest = form["ITSection24-ActualInterest"] == " 0" ? "" : form["ITSection24-ActualInterest"];
                    var Narration = form["ITSection24-Narration"] == "0" ? "" : form["ITSection24-Narration"];
                    ITP.ActualInterest = Convert.ToDouble(ActualInterest);
                    ITP.DeclaredInterest = Convert.ToDouble(DeclaredInterest);
                    ITP.Narration = Narration;
                    if (data != null)
                    {
                        var id = Convert.ToInt32(data);
                        var db_data = db.ITSection24Payment.Where(e => e.Id == id).SingleOrDefault();
                        db_data.ActualInterest = ITP.ActualInterest;
                        db_data.DeclaredInterest = ITP.DeclaredInterest;
                        db_data.Narration = ITP.Narration;
                        try
                        {
                            db.ITSection24Payment.Attach(db_data);
                            db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;
                            Msg.Add("  Record Updated");
                            // return Json(new Utility.JsonReturnClass {  data = db_data.ToString() , success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            return Json(new { status = true, data = db_data, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new { data = db_data, responseText = "Record Updated..!" }, JsonRequestBehavior.AllowGet);
                        }
                        catch (Exception e)
                        {

                            throw e;
                        }
                    }
                    else
                    {
                        Msg.Add(" Data Is Null ");
                        return Json(new { status = false, data = "", responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        // return Json(new { responseText = "Data Is Null" }, JsonRequestBehavior.AllowGet);
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
                    Msg.Add(ex.Message);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                ITSection24Payment ITSec24Payment = db.ITSection24Payment.Where(e => e.Id == data).SingleOrDefault();

                //if (ITSec10Payment. != null)
                //{
                //    return this.Json(new { status = true, responseText = "You cannot delete as salary is generated.", JsonRequestBehavior.AllowGet });
                //}



                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    try
                    {
                        db.ITSection24Payment.Remove(ITSec24Payment);
                        await db.SaveChangesAsync();


                        ts.Complete();
                        return this.Json(new { status = true, responseText = "Data removed.", JsonRequestBehavior.AllowGet });

                    }
                    catch (RetryLimitExceededException /* dex */)
                    {
                        //Log the error (uncomment dex variable name and add a line here to write a log.)
                        //ModelState.AddModelError("", "Unable to delete. Try again, and if the problem persists, see your system administrator.");
                        //return RedirectToAction("Delete");
                        return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
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
                        List<string> Msg = new List<string>();
                        Msg.Add(ex.Message);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
        }
        public string InvestmentUploadFile(string FolderName)
        {
            string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(
                System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\Images\\" + FolderName + "\\";
            String localPath = "";
            bool exists = System.IO.Directory.Exists(requiredPath);
            if (!exists)
            {
                localPath = new Uri(requiredPath).LocalPath;
                System.IO.Directory.CreateDirectory(localPath);
            }
            return localPath;
        }

        [HttpPost]
        public ActionResult InvestmentUpload(HttpPostedFileBase[] files, FormCollection form, string data, string Id, string SubId)
        {
            if (ModelState.IsValid)
            {

                string extension, newfilename, deletefilepath = "";
                Int32 Count = 0;
                string loanadvancehead = "0", subinvestmentid = "0", fromdate = "", todate = "", OFinYr = "";
                ITSection24Payment itsection24data = null;
                // ITSubInvestmentPayment itsubinvestment = null;
                using (DataBaseContext db = new DataBaseContext())
                {
                    if (Id != null)
                    {
                        int itid = Convert.ToInt32(Id);
                        itsection24data = db.ITSection24Payment.Include(e => e.LoanAdvanceHead).Include(e => e.FinancialYear).Where(e => e.Id == itid).SingleOrDefault();
                        if (itsection24data.LoanAdvanceHead != null)
                        {
                            loanadvancehead = itsection24data.LoanAdvanceHead.Id.ToString();
                        }

                        OFinYr = itsection24data.FinancialYear.Id.ToString();
                        fromdate = itsection24data.FinancialYear.FromDate.Value.Year.ToString();
                        todate = itsection24data.FinancialYear.ToDate.Value.Year.ToString();
                        deletefilepath = itsection24data.Path;
                    }
                    //if (SubId != null && SubId != "")
                    //{
                    //    int subid = Convert.ToInt32(SubId);
                    //    itsubinvestment = db.ITSubInvestmentPayment.Where(e => e.Id == subid).SingleOrDefault();
                    //    subinvestmentid = SubId;
                    //    deletefilepath = itsubinvestment.Path;
                    //}
                    var allowedExtensions = new[] { ".Jpg", ".png", ".jpg", ".jpeg", ".pdf" };
                    foreach (HttpPostedFileBase file in files)
                    {
                        if (file == null)
                        {
                            return Json(new { success = false, responseText = "Please Select The File..!" }, JsonRequestBehavior.AllowGet);
                        }
                        extension = Path.GetExtension(file.FileName);
                        if (!allowedExtensions.Contains(extension))
                        {
                            return Json(new { success = false, responseText = "File Type Is Not Supported..!" }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    foreach (HttpPostedFileBase file in files)
                    {

                        if (file != null)
                        {
                            extension = Path.GetExtension(file.FileName);
                            newfilename = OFinYr + "_" + loanadvancehead + "_" + subinvestmentid + "_" + Id + extension;
                            String FolderName = "FinancialYear" + fromdate + "-" + todate + "\\Investment\\";

                            var InputFileName = Path.GetFileName(file.FileName);
                            string ServerSavePath = InvestmentUploadFile(FolderName);
                            string ServerMappath = ServerSavePath + newfilename;
                            if (deletefilepath != null)
                            {
                                FileInfo File = new FileInfo(deletefilepath);
                                bool exists = File.Exists;
                                if (exists)
                                {
                                    System.IO.File.Delete(deletefilepath);
                                }
                            }
                            file.SaveAs(Path.Combine(ServerSavePath, newfilename));
                            if (Id != null)
                            {
                                db.ITSection24Payment.Attach(itsection24data);
                                db.Entry(itsection24data).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                TempData["RowVersion"] = itsection24data.RowVersion;
                                db.Entry(itsection24data).State = System.Data.Entity.EntityState.Detached;
                                itsection24data.DBTrack = new DBTrack
                                {
                                    CreatedBy = itsection24data.DBTrack.CreatedBy == null ? null : itsection24data.DBTrack.CreatedBy,
                                    CreatedOn = itsection24data.DBTrack.CreatedOn == null ? null : itsection24data.DBTrack.CreatedOn,
                                    Action = "M",
                                    ModifiedBy = SessionManager.UserName,
                                    ModifiedOn = DateTime.Now
                                };
                                ITSection24Payment ContactDet = itsection24data;
                                ContactDet.Path = ServerMappath;
                                ContactDet.DBTrack = itsection24data.DBTrack;

                                db.Entry(ContactDet).State = System.Data.Entity.EntityState.Modified;
                                db.Entry(ContactDet).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                db.SaveChanges();
                            }
                            //else
                            //{
                            //    db.ITSubInvestmentPayment.Attach(itsubinvestment);
                            //    db.Entry(itsubinvestment).State = System.Data.Entity.EntityState.Modified;
                            //    db.SaveChanges();
                            //    TempData["RowVersion"] = itsubinvestment.RowVersion;
                            //    db.Entry(itsubinvestment).State = System.Data.Entity.EntityState.Detached;
                            //    itsubinvestment.DBTrack = new DBTrack
                            //    {
                            //        CreatedBy = itsubinvestment.DBTrack.CreatedBy == null ? null : itsubinvestment.DBTrack.CreatedBy,
                            //        CreatedOn = itsubinvestment.DBTrack.CreatedOn == null ? null : itsubinvestment.DBTrack.CreatedOn,
                            //        Action = "M",
                            //        ModifiedBy = SessionManager.UserName,
                            //        ModifiedOn = DateTime.Now
                            //    };
                            //    ITSubInvestmentPayment ContactDet = itsubinvestment;
                            //    ContactDet.Path = ServerMappath;
                            //    ContactDet.DBTrack = itsubinvestment.DBTrack;

                            //    db.Entry(ContactDet).State = System.Data.Entity.EntityState.Modified;
                            //    db.Entry(ContactDet).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            //    db.SaveChanges();
                            //}
                            Count++;
                        }
                        else
                        {

                        }
                    }
                    if (Count > 0)
                    {
                        return Json(new { success = true, responseText = "File Uploaded..!" }, JsonRequestBehavior.AllowGet);
                    }
                    //else
                    //{
                    //    return Json(new { success = false, responseText = "Something is Wrong..!" }, JsonRequestBehavior.AllowGet);

                    //}
                }
                return View();

            }
            return View();
        }
        public ActionResult CheckUploadFile(string id, string SubId)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string localpath = "";
                ITSection24Payment itsection24data = null;
                //  ITSubInvestmentPayment itsubinvestment = null;
                if (id != null && id != "")
                {
                    int itid = Convert.ToInt32(id);
                    itsection24data = db.ITSection24Payment.Where(e => e.Id == itid).SingleOrDefault();
                }

                //newfilename = OFinYr + "_" + investmentid + "_" + subinvestmentid + "_" + id + ".jpg";
                //String FolderName = "FinancialYear" + fromdate + "-" + todate + "\\Investment\\" + newfilename;
                //string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(
                //System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\Images\\" + FolderName;
                //if (SubId == "")
                //{

                if (itsection24data.Path != null)
                {
                    localpath = itsection24data.Path;
                }
                else
                {
                    return Json(new { success = false, responseText = "File Uploaded..!" }, JsonRequestBehavior.AllowGet);
                }
                // }
                //else
                //{
                //    int subid = Convert.ToInt32(SubId);
                //    itsubinvestment = db.ITSubInvestmentPayment.Where(e => e.Id == subid).SingleOrDefault();
                //    if (itsubinvestment.Path != null)
                //    {
                //        localpath = itsubinvestment.Path;
                //    }
                //    else
                //    {
                //        return Json(new { success = false, responseText = "File Uploaded..!" }, JsonRequestBehavior.AllowGet);
                //    }

                //}
                FileInfo file = new FileInfo(localpath);
                bool exists = file.Exists;
                string extension = Path.GetExtension(file.Name);
                if (exists)
                {
                    return Json(new { success = true, fileextension = extension }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, responseText = "File Uploaded..!" }, JsonRequestBehavior.AllowGet);

                }
            }
            return null;
        }
        public ActionResult GetCompImage(string id, string SubId)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string localpath = "";
                ITSection24Payment itsection24data = null;
                // ITSubInvestmentPayment itsubinvestment = null;
                if (id != null && id != "")
                {
                    int itid = Convert.ToInt32(id);
                    itsection24data = db.ITSection24Payment.Where(e => e.Id == itid).SingleOrDefault();
                }
                //if (SubId == "")
                //{

                if (itsection24data.Path != null)
                {
                    localpath = itsection24data.Path;
                }
                else
                {
                    return View("File Not Found");
                    // return Json(new { success = false, responseText = "File Not Found..!" }, JsonRequestBehavior.AllowGet);
                }
                //  }
                //else
                //{
                //    int subid = Convert.ToInt32(SubId);
                //    itsubinvestment = db.ITSubInvestmentPayment.Where(e => e.Id == subid).SingleOrDefault();
                //    if (itsubinvestment.Path != null)
                //    {
                //        localpath = itsubinvestment.Path;
                //    }
                //    else
                //    {
                //        return View("File Not Found");
                //        //return Content("File Not Found");
                //        //return Json(new { success = false, responseText = "File Not Found..!" }, JsonRequestBehavior.AllowGet);
                //    }
                //}
                FileInfo file = new FileInfo(localpath);
                bool exists = file.Exists;
                string extension = Path.GetExtension(file.Name);

                if (exists)
                {
                    if (extension == ".pdf")
                    {
                        return File(file.FullName, "application/pdf", file.Name + " ");
                        //string pdf="pdf";
                        //byte[] imageArray = System.IO.File.ReadAllBytes("" + file + "");

                        //string base64ImageRepresentation = Convert.ToBase64String(imageArray);
                        //return Json(new { data = base64ImageRepresentation, status = pdf }, JsonRequestBehavior.AllowGet);
                    }
                    if (extension == ".jpg")
                    {
                        // return File(file.FullName, "image/png", file.Name + " ");
                        byte[] imageArray = System.IO.File.ReadAllBytes("" + file + "");
                        string base64ImageRepresentation = Convert.ToBase64String(imageArray);
                        return Json(new { data = base64ImageRepresentation, status = extension }, JsonRequestBehavior.AllowGet);
                    }
                    if (extension == ".png")
                    {
                        //return File(file.FullName, "image/png", file.Name + " ");
                        string pdf = "png";
                        byte[] imageArray = System.IO.File.ReadAllBytes("" + file + "");
                        string base64ImageRepresentation = Convert.ToBase64String(imageArray);
                        return Json(new { data = base64ImageRepresentation, status = extension }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Content("File Not Found");
                    //return Json(new { success = false, responseText = "File Uploaded..!" }, JsonRequestBehavior.AllowGet);
                }
                return null;
            }


        }
	}
}