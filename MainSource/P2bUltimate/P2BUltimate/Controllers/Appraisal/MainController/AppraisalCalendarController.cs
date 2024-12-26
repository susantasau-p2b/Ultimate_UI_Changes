using System;
using System.Collections.Generic;
using System.Web;
using System.Linq;
using System.Web.Mvc;
using System.Net;
using System.Data.Entity;
using P2BUltimate.Models;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Transactions;
using P2b.Global;
using System.Text;
using P2BUltimate.App_Start;
using System.Threading.Tasks;
using P2BUltimate.Security;
using Appraisal;
namespace P2BUltimate.Controllers.Core.MainController
{
    [AuthoriseManger]
    public class AppraisalCalendarController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();

        public ActionResult Index()
        {
            return View("~/Views/Appraisal/MainViews/AppraisalCalendar/Index.cshtml");
        }

        public ActionResult Partial()
        {
            return View("~/Views/Shared/Core/_Calender.cshtml");
        }
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var returndata = db.Calendar.Include(e => e.Name).Where(e => e.Id == data).AsEnumerable()
                    .Select(e => new
                    {
                        Name = e.Name != null ? e.Name.Id : 0,
                        ToDate = e.ToDate,
                        FromDate = e.FromDate,
                        Default = e.Default,
                    }).ToList();
                return Json(new Object[] { returndata, "", "", "", JsonRequestBehavior.AllowGet });
            }
        }
        [HttpPost]
        public ActionResult Create(Calendar c, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var Name = form["Name_drop"] == "0" ? 0 : Convert.ToInt32(form["Name_drop"]);
                    if (Name != 0)
                    {
                        c.Name = db.LookupValue.Find(Name);
                    }
                    c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName };
                    if (ModelState.IsValid)
                    {
                        Calendar calendar = new Calendar()
                        {
                            Name = c.Name,
                            FromDate = c.FromDate,
                            ToDate = c.ToDate,
                            Default = c.Default,
                            DBTrack = c.DBTrack
                        };
                        try
                        {
                            using (TransactionScope ts = new TransactionScope())
                            {
                                if (c.FromDate > c.ToDate)
                                {
                                    Msg.Add(" To Date should be greater than From Date. ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }
                                var alrdy = db.Calendar.Where(e => e.Name.LookupVal.ToUpper() == "APPRAISALCALENDAR" && (e.Default == true && calendar.Default == true)).Count();

                                if (alrdy > 0)
                                {
                                    Msg.Add("   Default  Appraisal Calendar already exist.  ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }

                                var alrDq = db.Calendar.Where(e => e.Name.LookupVal.ToUpper() == calendar.Name.LookupVal.ToUpper())
                                   .Any(q => ((q.FromDate <= c.FromDate && q.ToDate <= c.ToDate) || (q.FromDate <= c.FromDate && q.ToDate >= c.ToDate)) && (q.ToDate >= c.FromDate));
                                if (alrDq == true)
                                {
                                    Msg.Add("  Appraisal Calendar With this Period already exist.  ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }

                                if (db.Calendar.Where(e => e.Name.LookupVal.ToUpper() == calendar.Name.LookupVal.ToUpper()).Any(o => o.FromDate == c.FromDate))
                                {
                                    Msg.Add("  From Date already exists.  ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }

                                if (db.Calendar.Where(e => e.Name.LookupVal.ToUpper() == calendar.Name.LookupVal.ToUpper()).Any(o => o.ToDate == c.ToDate))
                                {
                                    Msg.Add("  To Date Already Exists.  ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }

                                db.Calendar.Add(calendar);
                                db.SaveChanges();

                                var compid = Convert.ToInt32(SessionManager.CompanyId);
                                var oCompany = db.Company.Find(compid);

                                List<Calendar> OFAT = new List<Calendar>();
                                OFAT.Add(db.Calendar.Find(calendar.Id));

                                CompanyAppraisal OEmployeePayroll = null;
                                OEmployeePayroll = db.CompanyAppraisal.Include(e => e.AppCalendar)
                                    .Where(e => e.Company.Id == oCompany.Id).SingleOrDefault();

                                if (OEmployeePayroll == null)
                                {
                                    CompanyAppraisal OTEP = new CompanyAppraisal()
                                    {
                                        Company = db.Company.Find(oCompany.Id),
                                        AppCalendar = OFAT,
                                        DBTrack = c.DBTrack

                                    };
                                    db.CompanyAppraisal.Add(OTEP);
                                    db.SaveChanges();
                                }
                                else
                                {
                                    var aa = db.CompanyAppraisal.Find(OEmployeePayroll.Id);
                                    OFAT.AddRange(aa.AppCalendar);
                                    aa.AppCalendar = OFAT;
                                    //OEmployeePayroll.DBTrack = dbt;

                                    db.CompanyAppraisal.Attach(aa);
                                    db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    db.Entry(aa).State = System.Data.Entity.EntityState.Detached;

                                }

                                //var CalendarList = new List<Calendar>();
                                //CalendarList.Add(calendar);
                                //oCompany.Calendar = CalendarList;
                                //db.Entry(oCompany).State = System.Data.Entity.EntityState.Modified;
                                //db.SaveChanges();

                                ts.Complete();
                            }
                            Msg.Add("  Data Saved successfully  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            return RedirectToAction("Create", new { concurrencyError = true, id = c.Id });
                        }
                        catch (DataException /* dex */)
                        {
                            Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
                return View(c);
            }
        }

        public ActionResult PopulateDropDownList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var id = Convert.ToInt32(data);
                var qurey = db.Calendar
                    .Where(e => e.Id == id)
                    .Select(e =>
                        new
                        {
                            FromDate_Month = e.FromDate.Value,
                            FromDate_Year = e.FromDate.Value,
                            ToDate_Month = e.ToDate.Value,
                            ToDate_Year = e.ToDate.Value
                        }).ToList();
                return Json(qurey, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult EditSave(Calendar data1, int data, FormCollection form)
        {

            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    //Calendar c = db.Calendar.Find(data);
                    var Name = form["Name_drop"] == "0" ? 0 : Convert.ToInt32(form["Name_drop"]);

                    if (Name != 0)
                    {
                        data1.Name = db.LookupValue.Find(Name);
                    }

                    var db_data = db.Calendar.Include(q => q.Name).Where(a => a.Id == data).SingleOrDefault();
                    var alrdy = db.Calendar.Include(a => a.Name).Where(e => e.Name.LookupVal.ToString().ToUpper() == db_data.Name.LookupVal.ToString().ToUpper() && e.Default == true && data1.Default == true).Count();

                    if (alrdy > 0)
                    {
                        Msg.Add("   Default Appraisal Year already exist.  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        // return this.Json(new Object[] { "", "", "From Date already exists.", JsonRequestBehavior.AllowGet });
                    }

                    db_data.ToDate = db_data.ToDate;
                    db_data.FromDate = db_data.FromDate;
                    db_data.Default = data1.Default;
                    db_data.Name = db_data.Name;

                    using (TransactionScope ts = new TransactionScope())
                    {
                        db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;
                        ts.Complete();
                    }
                    Msg.Add("  Data Saved successfully  ");
                    return Json(new Utility.JsonReturnClass { Id = db_data.Id, Val = db_data.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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


        public ActionResult GetLookupValue(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                SelectList s = (SelectList)null;
                var selected = "";
                if (data != "" && data != null)
                {
                    // var qurey = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == data).SingleOrDefault();
                    var qurey = db.Lookup.Where(e => e.Code == data).Select(e => e.LookupValues.Where(r => (r.IsActive == true) && (r.LookupVal.ToUpper() == "APPRAISALCALENDAR"))).SingleOrDefault(); // added by rekha 26-12-16
                    if (data2 != "" && data2 != "0")
                    {
                        selected = data2;
                    }
                    if (qurey != null)
                    {
                        s = new SelectList(qurey, "Id", "LookupVal", selected);
                    }
                }
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult Delete(int? data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                Calendar calendar = db.Calendar.Find(data);
                try
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        if (calendar.Default.ToString() == "True")
                        {
                            Msg.Add("  Data Cannot be removed due to Default Year. ");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }

                        db.Entry(calendar).State = System.Data.Entity.EntityState.Deleted;
                        db.SaveChanges();
                        ts.Complete();
                    }
                    Msg.Add("  Data removed successfully.  ");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    // return Json(new Object[] { "", "Data deleted.", JsonRequestBehavior.AllowGet });
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
        public ActionResult ValidateForm(Calendar c, FormCollection form)
        {
            // for success
            //return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            //for error
            List<string> Msg = new List<string>();
            Msg.Add("Ok");
            Msg.Add("Okk");
            return Json(new { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
                var Calendar = db.Calendar.Include(e => e.Name).Where(a => a.Name.LookupVal.ToUpper() == "APPRAISALCALENDAR").ToList();
                IEnumerable<Calendar> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = Calendar;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.Name != null ? e.Name.LookupVal.ToUpper().ToString().Contains(gp.searchString.ToUpper()) : false)
                               || (e.FromDate.Value.ToShortDateString().ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                               || (e.ToDate.Value.ToShortDateString().ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                               || (e.Default.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                               || (e.Id.ToString().Contains(gp.searchString))
                               ).Select(a => new Object[] { a.Name != null? a.Name.LookupVal :"", a.FromDate.Value.ToShortDateString(), a.ToDate.Value.ToShortDateString(), a.Default.ToString(), a.Id }).ToList();
                        
                        // jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Name != null ? a.Name.LookupVal : null), Convert.ToString(a.FromDate), Convert.ToString(a.ToDate), Convert.ToString(a.Default) }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.Name != null ? a.Name.LookupVal : "a"), a.FromDate.Value.ToString("dd/MM/yyyy"), a.ToDate.Value.ToString("dd/MM/yyyy"), Convert.ToString(a.Default), a.Id }).ToList();
                        //jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Name != null ? a.Name.LookupVal : null, a.FromDate, a.ToDate, a.Default }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = Calendar;
                    Func<Calendar, string> orderfuc = (c =>
                                                               gp.sidx == "ID" ? c.Id.ToString() :
                                                               gp.sidx == "Name" ? c.Name != null ? c.Name.LookupVal : null :
                                                               gp.sidx == "FromDate" ? c.FromDate.ToString() :
                                                               gp.sidx == "ToDate" ? c.ToDate.ToString() :
                                                               gp.sidx == "Default" ? c.Default.ToString() : "");
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.Name != null ? a.Name.LookupVal : null), a.FromDate.Value.ToString("dd/MM/yyyy"), a.ToDate.Value.ToString("dd/MM/yyyy"), Convert.ToString(a.Default), a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.Name != null ? a.Name.LookupVal : null), a.FromDate.Value.ToString("dd/MM/yyyy"), a.ToDate.Value.ToString("dd/MM/yyyy"), Convert.ToString(a.Default), a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Name != null ? a.Name.LookupVal : null, a.FromDate, a.ToDate, a.Default, a.Id }).ToList();
                    }
                    totalRecords = Calendar.Count();
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
