using P2b.Global;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using P2BUltimate.Process;
using Payroll;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using P2BUltimate.Security;
using System.Data.Entity.Core.Objects;
using System.IO;

namespace P2BUltimate.Controllers.Payroll.MainController
{
    public class ArrearJVProcessDataController : Controller
    {
      //  private DataBaseContext db = new DataBaseContext();
        //
        // GET: /JVProcessData/
        public ActionResult Index()
        {
            // db.RefreshAllEntites(RefreshMode.StoreWins);
            return View("~/Views/Payroll/MainViews/ArrearJVProcessData/Index.cshtml");
        }



        public ActionResult GetJVNameLKDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var id = Convert.ToInt32(SessionManager.CompanyId);
                var Comp_jv_data = db.CompanyPayroll.Include(e => e.ArrJVParameter).Where(e => e.Company.Id == id).SingleOrDefault();
                var jv_data = Comp_jv_data.ArrJVParameter.Select(e => new
                {
                    code = e.Id.ToString(),
                    value = "JVCode: " + e.ArrJVProductCode.ToString() + ",JVName: " + e.ArrJVName.ToString(),

                }).ToList();
                return Json(jv_data, JsonRequestBehavior.AllowGet);
            }

        }
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.ArrJVProcessDataSummary
                    .Where(e => e.Id == data).Select
                    (e => new
                    {

                        BatchName = e.ArrBatchName,
                        CreditAmount = e.ArrCreditAmount,
                        DebitAmount = e.ArrDebitAmount,
                        JVFileName = e.ArrJVFileName,
                        ProcessDate = e.ArrProcessDate,
                        ProcessMonth = e.ArrProcessMonth,
                        ReleaseDate = e.ReleaseDate,
                        Action = e.DBTrack.Action
                    }).ToList();
                //var Corp = db.JVProcessData.Find(data);
                //TempData["RowVersion"] = Corp.RowVersion;
                //Session["Id"] = Corp.Id;
                //var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, "", "", "", JsonRequestBehavior.AllowGet });
            }
        }
        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult Create(FormCollection form) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string JVParams = form["ArrJVParameter-table"] == "0" ? "" : form["ArrJVParameter-table"];
                string mPayMonth = form["Create-month"] == "0" ? "" : form["Create-month"];
                string mBatchName = form["Batch"] == "0" ? "" : form["Batch"];
                string ArrJVParameterslist = form["jvnamelist"] == "0" ? "" : form["jvnamelist"];

                try
                {
                    int CompId = 0;

                    if (!String.IsNullOrEmpty(Session["CompId"].ToString()))
                    {
                        CompId = int.Parse(Session["CompId"].ToString());
                    }

                    List<int> ids = new List<int>();
                    if (JVParams != null && JVParams != "0" && JVParams != "false")
                    {
                        String itsec_id = JVParams;
                        ids = itsec_id.Split(',').Select(e => int.Parse(e)).ToList();
                        //  ids.Add(2517);

                    }
                    //   List<int> _JVCodeList = new List<int>();

                    //var JVList = db.ArrJVParameter.Include(e => e.SalaryHead).Where(e => ids.Contains(e.Id)).ToList();
                    //foreach (var item in JVList)
                    //{
                    //    string Code = db.ArrJVParameter.Where(e => e.Id == item.Id).SingleOrDefault().JVProductCode;
                    //    _JVCodeList.Add(Code);
                    //}
                    //foreach (var item in JVList)
                    //{
                    //    if (item.Irregular)
                    //    {
                    //        _JVCodeList = new List<string>();
                    //        var aa = item.SalaryHead.Select(a => a.Id.ToString()).ToList();
                    //        foreach (var item2 in aa)
                    //        {
                    //            _JVCodeList.Add(item2);
                    //        }
                    //    }
                    //}
                    var EmpList = db.EmployeePayroll.Include(e => e.SalaryArrearT).AsNoTracking().AsParallel().ToList();

                    List<int> OEmployeePayrollIds = new List<int>();
                  
                        foreach (var i in EmpList)
                        {
                            var SalT = i.SalaryArrearT.Where(e => e.IsPaySlip == false && e.PayMonth== mPayMonth).FirstOrDefault();
                            if (SalT != null)
                            {
                                OEmployeePayrollIds.Add(i.Id);
                            }
                        }
                    int OCompanyPayrollId = Convert.ToInt32(SessionManager.CompPayrollId);
                    //db.CompanyPayroll.Where(e => e.Company.Id == CompId).SingleOrDefault().Id;
                    //using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, new System.TimeSpan(0, 60, 0)))
                    //{
                    ArrJvProcess.GenerateArrJV(mPayMonth, OCompanyPayrollId, OEmployeePayrollIds, mBatchName, ids);
                    //  ts.Complete();
                    // }
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
                    List<string> Msg = new List<string>();
                    Msg.Add(ex.Message);
                    // return Json(new Object[] { "", "", Msg }, JsonRequestBehavior.AllowGet);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
                List<string> Msgs = new List<string>();
                Msgs.Add("Data Saved successfully");
                return Json(new Utility.JsonReturnClass { success = true, responseText = Msgs }, JsonRequestBehavior.AllowGet);
            }
        }

        public string Download(int id)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string FilePath = db.ArrJVProcessDataSummary.Where(e => e.Id == id).Select(e => e.ArrJVFileName).SingleOrDefault();
                    string filename = FilePath.Substring(FilePath.LastIndexOf("\\") + 1);
                    string content = string.Empty;
                    //using (var stream = new StreamReader(Server.MapPath("~/JVFile/" + filename)))
                    //{
                    //    content = stream.ReadToEnd();
                    //    //System.Diagnostics.Process.Start("notepad.exe", Server.MapPath("~/JVFile/" + filename));
                    //}

                    // return View("~/Views/Payroll/MainViews/PFECRSummaryR/Index.cshtml");
                    return FilePath;

                }
                catch (Exception ex)
                {

                    string content = string.Empty;
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
                    content = "LogFile Created";
                    //return View("~/Views/Payroll/MainViews/PFECRSummaryR/Index.cshtml");
                    return content;
                }
            }
        }

        public ActionResult DownloadFile(string fileName)
        {
            string localPath = new Uri(fileName).LocalPath;
            System.IO.FileInfo file = new System.IO.FileInfo(localPath);
            if (file.Exists)
                return File(file.FullName, "text/plain", file.Name);
            else
                return HttpNotFound();
        }

        public string ViewJVFile(int id)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string FilePath = db.ArrJVProcessDataSummary.Where(e => e.Id == id).Select(e => e.ArrJVFileName).SingleOrDefault();
                    string filename = FilePath.Substring(FilePath.LastIndexOf("\\") + 1);
                    string content = string.Empty;
                    using (var stream = new StreamReader(Server.MapPath("~/JVFile/" + filename)))
                    {
                        content = stream.ReadToEnd();
                        //System.Diagnostics.Process.Start("notepad.exe", Server.MapPath("~/JVFile/" + filename));
                    }

                    // return View("~/Views/Payroll/MainViews/PFECRSummaryR/Index.cshtml");
                    return content;

                }
                catch (Exception ex)
                {
                    string content = string.Empty;
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
                    content = "LogFile Created";
                    //return View("~/Views/Payroll/MainViews/PFECRSummaryR/Index.cshtml");
                    return content;
                }
            }
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
                IEnumerable<ArrJVProcessDataSummary> JVProcessDataSummary = null;

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

                if (gp.IsAutho == true)
                {
                    JVProcessDataSummary = db.ArrJVProcessDataSummary.Where(e => e.DBTrack.IsModified == true && e.ArrProcessMonth == PayMonth).AsNoTracking().ToList();
                }
                else
                {
                    JVProcessDataSummary = db.ArrJVProcessDataSummary.Where(e => e.ArrProcessMonth == PayMonth).AsNoTracking().ToList();
                }

                IEnumerable<ArrJVProcessDataSummary> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = JVProcessDataSummary;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e =>  (e.ArrBatchName.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                        || (e.ArrCreditAmount.ToString().Contains(gp.searchString))
                        || (e.ArrDebitAmount.ToString().Contains(gp.searchString))
                        || (e.ArrProcessMonth.ToString().Contains(gp.searchString))
                        || (e.ArrProcessDate.ToString().Contains(gp.searchString))
                        || (e.ReleaseDate.ToString().Contains(gp.searchString))
                        || (e.ArrJVFileName.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                        || (e.Id.ToString().Contains(gp.searchString))
                        ).Select(a => new Object[] { a.ArrBatchName, a.ArrCreditAmount, a.ArrDebitAmount, a.ArrProcessMonth, a.ArrProcessDate.Value.ToShortDateString(), a.ReleaseDate, a.ArrJVFileName, a.Id }).ToList();

                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.ArrBatchName, a.ArrCreditAmount, a.ArrDebitAmount, a.ArrProcessMonth, a.ArrProcessDate, a.ReleaseDate, a.ArrJVFileName, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = JVProcessDataSummary;
                    Func<ArrJVProcessDataSummary, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "BatchName" ? c.ArrBatchName.ToString() :
                                         gp.sidx == "CreditAmount" ? c.ArrCreditAmount.ToString() :
                                         gp.sidx == "DebitAmount" ? c.ArrDebitAmount.ToString() :
                                         gp.sidx == "ProcessMonth" ? c.ArrProcessMonth.ToString() :
                                         gp.sidx == "ProcessDate" ? c.ArrProcessDate.ToString() :
                                         gp.sidx == "ReleaseDate" ? c.ReleaseDate.ToString() :
                                         gp.sidx == "JVFileName" ? c.ArrJVFileName.ToString() : ""
                                         );
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.ArrBatchName, a.ArrCreditAmount, a.ArrDebitAmount, a.ArrProcessMonth, a.ArrProcessDate.Value.ToString("dd/MM/yyyy"), a.ReleaseDate != null ? a.ReleaseDate.Value.ToString("dd/MM/yyyy") : null, a.ArrJVFileName, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.ArrBatchName, a.ArrCreditAmount, a.ArrDebitAmount, a.ArrProcessMonth, a.ArrProcessDate.Value.ToString("dd/MM/yyyy"), a.ReleaseDate != null ? a.ReleaseDate.Value.ToString("dd/MM/yyyy") : null, a.ArrJVFileName, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.ArrBatchName, a.ArrCreditAmount, a.ArrDebitAmount, a.ArrProcessMonth, a.ArrProcessDate.Value.ToString("dd/MM/yyyy"), a.ReleaseDate != null ? a.ReleaseDate.Value.ToString("dd/MM/yyyy") : null, a.ArrJVFileName, a.Id }).ToList();
                    }
                    totalRecords = JVProcessDataSummary.Count();
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
}