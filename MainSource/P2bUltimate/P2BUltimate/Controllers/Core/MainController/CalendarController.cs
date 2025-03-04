﻿using System;
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
using Recruitment;
using P2B.EExMS;
namespace P2BUltimate.Controllers.Core.MainController
{
    [AuthoriseManger]
    public class CalendarController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();

        public ActionResult Index()
        {
            return View("~/Views/Core/MainViews/Calendar/Index.cshtml");
        }
        public ActionResult RecruitmentCalendarIndex()
        {
            return View("~/Views/Recruitement/MainView/Calendar/Index.cshtml");
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
                        c.Name = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "500").FirstOrDefault().LookupValues.Where(e => e.Id == Name).FirstOrDefault(); //db.LookupValue.Find(Name);
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

                                var alrDq = db.Calendar.Where(e => e.Name.LookupVal.ToUpper() == calendar.Name.LookupVal.ToUpper())
                                 .Any(q => ((q.FromDate <= c.FromDate && q.ToDate <= c.ToDate) || (q.FromDate <= c.FromDate && q.ToDate >= c.ToDate)) && (q.ToDate >= c.FromDate));
                                if (alrDq == true)
                                {
                                    Msg.Add("Year With this Period already exist.  ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }
                                if (c.FromDate > c.ToDate)
                                {
                                    Msg.Add(" To Date should be greater than From Date.  ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }

                                var alrdy = db.Calendar.Where(e => (e.Name.LookupVal.ToUpper() == calendar.Name.LookupVal.ToUpper())
                                   && (e.Default == true && calendar.Default == true)).Count();

                                if (alrdy > 0)
                                {
                                    Msg.Add(" Default  Year already exist. ");
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
                                var CalendarList = new List<Calendar>();
                                CalendarList.Add(calendar);
                                oCompany.Calendar = CalendarList;
                                db.Entry(oCompany).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                ts.Complete();
                            }
                            Msg.Add("  Data Saved successfully  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            // return Json(new Object[] { "", "", "Data saved successfully." }, JsonRequestBehavior.AllowGet);
                            // return this.Json(new { Error = calendar.Id });
                            //return RedirectToAction("Index");
                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            return RedirectToAction("Create", new { concurrencyError = true, id = c.Id });
                        }
                        catch (DataException /* dex */)
                        {
                            Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //Log the error (uncomment dex variable name after DataException and add a line here to write a log.
                            //ModelState.AddModelError(string.Empty, "Unable to create. Try again, and if the problem persists contact your system administrator.");
                            //return View(c);
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

        public ActionResult RecruitmentCalendarCreate(Calendar c, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var Name = form["Name_drop"] == "0" ? 0 : Convert.ToInt32(form["Name_drop"]);
                    if (Name != 0)
                    {
                        c.Name = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "500").FirstOrDefault().LookupValues.Where(e => e.Id == Name).FirstOrDefault();//db.LookupValue.Find(Name);
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

                                var alrDq = db.Calendar.Where(e => e.Name.LookupVal.ToUpper() == calendar.Name.LookupVal.ToUpper())
                                 .Any(q => ((q.FromDate <= c.FromDate && q.ToDate <= c.ToDate) || (q.FromDate <= c.FromDate && q.ToDate >= c.ToDate)) && (q.ToDate >= c.FromDate));
                                if (alrDq == true)
                                {
                                    Msg.Add("Year With this Period already exist.  ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }
                                if (c.FromDate > c.ToDate)
                                {
                                    Msg.Add(" To Date should be greater than From Date.  ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }

                                var alrdy = db.Calendar.Where(e => (e.Name.LookupVal.ToUpper() == calendar.Name.LookupVal.ToUpper())
                                   && (e.Default == true && calendar.Default == true)).Count();

                                if (alrdy > 0)
                                {
                                    Msg.Add(" Default  Year already exist. ");
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

                                RecruitYearlyCalendar Recruitcalendar = new RecruitYearlyCalendar()
                                {
                                    RecruitmentCalendar = calendar,
                                    DBTrack = c.DBTrack

                                };

                                db.RecruitYearlyCalendar.Add(Recruitcalendar);
                                db.SaveChanges();



                                var compid = Convert.ToInt32(SessionManager.CompanyId);
                                var companyRecruitment = db.CompanyRecruitment.Where(e => e.Company.Id == compid).SingleOrDefault();

                                if (companyRecruitment != null)
                                {
                                    List<RecruitYearlyCalendar> RecruitYearlyCalendarlst = new List<RecruitYearlyCalendar>();
                                    RecruitYearlyCalendarlst.Add(Recruitcalendar);
                                    companyRecruitment.RecruitYearlyCalendar = RecruitYearlyCalendarlst;
                                    db.Entry(companyRecruitment).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    db.Entry(companyRecruitment).State = System.Data.Entity.EntityState.Detached;
                                }

                                var oCompany = db.Company.Find(compid);
                                var CalendarList = new List<Calendar>();
                                CalendarList.Add(calendar);
                                oCompany.Calendar = CalendarList;
                                db.Entry(oCompany).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                ts.Complete();
                            }
                            Msg.Add("  Data Saved successfully  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            // return Json(new Object[] { "", "", "Data saved successfully." }, JsonRequestBehavior.AllowGet);
                            // return this.Json(new { Error = calendar.Id });
                            //return RedirectToAction("Index");
                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            return RedirectToAction("Create", new { concurrencyError = true, id = c.Id });
                        }
                        catch (DataException /* dex */)
                        {
                            Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //Log the error (uncomment dex variable name after DataException and add a line here to write a log.
                            //ModelState.AddModelError(string.Empty, "Unable to create. Try again, and if the problem persists contact your system administrator.");
                            //return View(c);
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

        public ActionResult ExpenseCalendarCreate(Calendar c, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var Name = form["Name_drop"] == "0" ? 0 : Convert.ToInt32(form["Name_drop"]);
                    if (Name != 0)
                    {
                        c.Name = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "500").FirstOrDefault().LookupValues.Where(e => e.Id == Name).FirstOrDefault();//db.LookupValue.Find(Name);
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

                                var alrDq = db.Calendar.Where(e => e.Name.LookupVal.ToUpper() == calendar.Name.LookupVal.ToUpper())
                                 .Any(q => ((q.FromDate <= c.FromDate && q.ToDate <= c.ToDate) || (q.FromDate <= c.FromDate && q.ToDate >= c.ToDate)) && (q.ToDate >= c.FromDate));
                                if (alrDq == true)
                                {
                                    Msg.Add("Year With this Period already exist.  ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }
                                if (c.FromDate > c.ToDate)
                                {
                                    Msg.Add(" To Date should be greater than From Date.  ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }

                                var alrdy = db.Calendar.Where(e => (e.Name.LookupVal.ToUpper() == calendar.Name.LookupVal.ToUpper())
                                   && (e.Default == true && calendar.Default == true)).Count();

                                if (alrdy > 0)
                                {
                                    Msg.Add(" Default  Year already exist. ");
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

                                ExpenseCalendar expensecalendar = new ExpenseCalendar()
                                {
                                    Calendar = calendar,
                                    DBTrack = c.DBTrack

                                };

                                db.ExpenseCalendar.Add(expensecalendar);
                                db.SaveChanges();



                                var compid = Convert.ToInt32(SessionManager.CompanyId);
                                var companyRecruitment = db.CompanyRecruitment.Where(e => e.Company.Id == compid).SingleOrDefault();

                                //if (companyRecruitment != null)
                                //{
                                //    List<ExpenseCalendar> ExpenseCalendarlst = new List<ExpenseCalendar>();
                                //    ExpenseCalendarlst.Add(expensecalendar);
                                //    companyRecruitment.ex = RecruitYearlyCalendarlst;
                                //    db.Entry(companyRecruitment).State = System.Data.Entity.EntityState.Modified;
                                //    db.SaveChanges();
                                //    db.Entry(companyRecruitment).State = System.Data.Entity.EntityState.Detached;
                                //}

                                var oCompany = db.Company.Find(compid);
                                var CalendarList = new List<Calendar>();
                                CalendarList.Add(calendar);
                                oCompany.Calendar = CalendarList;
                                db.Entry(oCompany).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                ts.Complete();
                            }
                            Msg.Add("  Data Saved successfully  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            // return Json(new Object[] { "", "", "Data saved successfully." }, JsonRequestBehavior.AllowGet);
                            // return this.Json(new { Error = calendar.Id });
                            //return RedirectToAction("Index");
                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            return RedirectToAction("Create", new { concurrencyError = true, id = c.Id });
                        }
                        catch (DataException /* dex */)
                        {
                            Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //Log the error (uncomment dex variable name after DataException and add a line here to write a log.
                            //ModelState.AddModelError(string.Empty, "Unable to create. Try again, and if the problem persists contact your system administrator.");
                            //return View(c);
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

        public ActionResult GetLookupValue(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                SelectList s = (SelectList)null;
                var selected = "";
                if (data != "" && data != null)
                {
                    // var qurey = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == data).SingleOrDefault();
                    var qurey = db.Lookup.Where(e => e.Code == data).Select(e => e.LookupValues.Where(r => (r.IsActive == true) && (r.LookupVal.ToUpper() != "LEAVECALENDAR"))).SingleOrDefault(); // added by rekha 26-12-16
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
        //[HttpPost]
        //public ActionResult EditSave(Calendar data1, int data, FormCollection form)
        //{

        //    List<string> Msg = new List<string>();
        //    //Calendar c = db.Calendar.Find(data);
        //    var Name = form["Name"] == "0" ? 0 : Convert.ToInt32(form["Name"]);
        //    if (Name != 0)
        //    {
        //        data1.Name = db.LookupValue.Find(Name);
        //    }
        //    var db_data = db.Calendar.Find(data);
        //    db_data.ToDate = data1.ToDate;
        //    db_data.FromDate = data1.FromDate;
        //    db_data.Default = data1.Default;
        //    db_data.Name = data1.Name;
        //    //if (ModelState.IsValid)
        //    //{
        //    //    //Calendar blog = null; // to retrieve old data
        //    //DbPropertyValues originalBlogValues = null;
        //    //Calendar Old_Corp = null;

        //    //using (var context = new DataBaseContext())
        //    //{
        //    //    blog = context.Calendar.Where(e => e.Id == data).SingleOrDefault();
        //    //    originalBlogValues = context.Entry(blog).OriginalValues;
        //    //}
        //    //data1.DBTrack = new DBTrack
        //    //{
        //    //    CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //    //    CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //    //    Action = "M",
        //    //    ModifiedBy = SessionManager.UserName,
        //    //    ModifiedOn = DateTime.Now
        //    //};
        //    //Calendar calendar = new Calendar()
        //    //{
        //    //    Name = data1.Name,
        //    //    FromDate = data1.FromDate,
        //    //    ToDate = data1.ToDate,
        //    //    Default = data1.Default,
        //    //    DBTrack = data1.DBTrack,
        //    //    Id = data,
        //    //};
        //    //try
        //    // {
        //    using (TransactionScope ts = new TransactionScope())
        //    {
        //        db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
        //        db.SaveChanges();
        //        db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;
        //        ts.Complete();
        //    }
        //    Msg.Add("  Data Saved successfully  ");
        //    return Json(new Utility.JsonReturnClass { Id = db_data.Id, Val = db_data.FullDetails ,success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //    //return this.Json(new Object[] { "", "", "Data saved successfully." });
        //    //    }
        //    //    catch (DbUpdateConcurrencyException)
        //    //    {
        //    //        return RedirectToAction("Edit", new { concurrencyError = true, id = data1.Id });
        //    //    }
        //    //    catch (DataException /* dex */)
        //    //    {
        //    //        //Log the error (uncomment dex variable name after DataException and add a line here to write a log.
        //    //        ModelState.AddModelError(string.Empty, "Unable to edit. Try again, and if the problem persists contact your system administrator.");
        //    //        return View(data1);
        //    //    }
        //    //}
        //   // return View(data1);
        //}

        [HttpPost]
        public ActionResult EditSave(Calendar data1, int data, FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    //Calendar c = db.Calendar.Find(data);
                    var Name = form["Name_drop"] == "0" ? 0 : Convert.ToInt32(form["Name_drop"]);

                    if (Name != 0)
                    {
                        data1.Name = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "500").FirstOrDefault().LookupValues.Where(e => e.Id == Name).FirstOrDefault();//db.LookupValue.Find(Name);
                    }

                    var db_data = db.Calendar.Include(q => q.Name).Where(a => a.Id == data).SingleOrDefault();
                    var alrdy = db.Calendar.Include(a => a.Name).Where(e => e.Name.LookupVal.ToString().ToUpper() == db_data.Name.LookupVal.ToString().ToUpper() && e.Default == true && data1.Default == true).Count();

                    if (alrdy > 0)
                    {
                        Msg.Add("   Default  Year already exist. ");
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


        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult Delete(int? data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                Calendar calendar = db.Calendar.Find(data);
                try
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        if (calendar.Default.ToString() == "True")
                        {
                            Msg.Add("  Data Cannont Be removed Due to Default Year. ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                        var chk = db.LvOpenBal.Include(q => q.LvCalendar).Where(e => e.LvCalendar.Id == data).Count();
                        if (chk > 0)
                        {
                            Msg.Add("  Data Cannont Be removed Since its used in  Open Balance. ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                        var chk1 = db.RecruitYearlyCalendar.Include(q => q.RecruitmentCalendar).Where(e => e.RecruitmentCalendar.Id == data).Count();
                        var chk2 = db.ExpenseCalendar.Include(q => q.Calendar).Where(e => e.Calendar.Id == data).Count();
                        if (chk1 > 0)
                        {
                            Msg.Add("  Data Cannont Be removed Since its used in RecruitYearlyCalendar. ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                        else if(chk2 > 0)
                        {
                            Msg.Add("  Data Cannont Be removed Since its used in ExpenseCalendar. ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                        db.Entry(calendar).State = System.Data.Entity.EntityState.Deleted;
                        db.SaveChanges();
                        ts.Complete();
                    }
                    Msg.Add("  Data removed successfully.  ");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    // return Json(new Object[] { "", "Data deleted.", JsonRequestBehavior.AllowGet });
                }

                catch (DataException /* dex */)
                {
                    Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //Log the error (uncomment dex variable name after DataException and add a line here to write a log.
                    //return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists contact your system administrator." });
                    //ModelState.AddModelError(string.Empty, "Unable to delete. Try again, and if the problem persists contact your system administrator.");
                    //return RedirectToAction("Index");
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
        public ActionResult RecruitmentCalendarP2BGrid(P2BGrid_Parameters gp)
        {
            try
            {
                DataBaseContext db = new DataBaseContext();
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                var jsonData = (Object)null;
                var ModuleName = System.Web.HttpContext.Current.Session["ModuleType"];
                var Calendar = new List<Calendar>();
                Calendar = db.Calendar.Include(e => e.Name).Where(q => q.Name.LookupVal.ToUpper() == "RECRUITMENTCALENDAR").ToList();
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
                              ).Select(a => new Object[] { a.Name != null ? a.Name.LookupVal : "", a.FromDate.Value.ToShortDateString(), a.ToDate.Value.ToShortDateString(), a.Default.ToString(), a.Id }).ToList();
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
                    Func<Calendar, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "Name" ? c.Name != null ? c.Name.LookupVal : null :
                                                               gp.sidx == "FromDate" ? c.FromDate.Value.ToString() :
                                                               gp.sidx == "ToDate" ? c.ToDate.Value.ToString() :
                                                               gp.sidx == "Default" ? c.Default.ToString() : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.Name != null ? a.Name.LookupVal : null), a.FromDate.Value.ToString("dd/MM/yyyy"), a.ToDate.Value.ToString("dd/MM/yyyy"), Convert.ToString(a.Default), a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.Name != null ? a.Name.LookupVal : null), a.FromDate.Value.ToString("dd/MM/yyyy"), a.ToDate.Value.ToString("dd/MM/yyyy"), Convert.ToString(a.Default), a.Id  }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] {  a.Name != null ? a.Name.LookupVal : null, a.FromDate, a.ToDate, a.Default, a.Id }).ToList();
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


        public ActionResult ExpenseCalendarP2BGrid(P2BGrid_Parameters gp)
        {
            try
            {
                DataBaseContext db = new DataBaseContext();
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                var jsonData = (Object)null;
                var ModuleName = System.Web.HttpContext.Current.Session["ModuleType"];
                var Calendar = new List<Calendar>();
                Calendar = db.Calendar.Include(e => e.Name).Where(q => q.Name.LookupVal.ToUpper() == "EXPENSECALENDAR").ToList();
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
                              ).Select(a => new Object[] { a.Name != null ? a.Name.LookupVal : "", a.FromDate.Value.ToShortDateString(), a.ToDate.Value.ToShortDateString(), a.Default.ToString(), a.Id }).ToList();
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
                    Func<Calendar, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "Name" ? c.Name != null ? c.Name.LookupVal : null :
                                                               gp.sidx == "FromDate" ? c.FromDate.Value.ToString() :
                                                               gp.sidx == "ToDate" ? c.ToDate.Value.ToString() :
                                                               gp.sidx == "Default" ? c.Default.ToString() : "");
                    }
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
                var ModuleName = System.Web.HttpContext.Current.Session["ModuleType"];
                var Calendar = new List<Calendar>();
                if (ModuleName.ToString().Trim().ToUpper() == "EEIS")
                {
                    Calendar = db.Calendar.Include(e => e.Name).Where(q => q.Name.LookupVal.ToUpper() == "TRAININGCALENDAR").ToList();

                }
                
                else
                {
                    Calendar = db.Calendar.Include(e => e.Name).Where(q => q.Name.LookupVal.ToUpper() != "LEAVECALENDAR").ToList();
                }
                IEnumerable<Calendar> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = Calendar;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.Name != null ? e.Name.LookupVal.ToUpper().ToString().Contains(gp.searchString.ToUpper()): false)
                             || (e.FromDate.Value.ToShortDateString().ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                             || (e.ToDate.Value.ToShortDateString().ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                             || (e.Default.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                             || (e.Id.ToString().Contains(gp.searchString))
                             ).Select(a => new Object[] { a.Name != null ? a.Name.LookupVal : "", Convert.ToString(a.FromDate.Value.ToShortDateString()), Convert.ToString(a.ToDate.Value.ToShortDateString()), a.Default.ToString(), a.Id }).ToList();
                        
                        // jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Name != null ? a.Name.LookupVal : null), Convert.ToString(a.FromDate), Convert.ToString(a.ToDate), Convert.ToString(a.Default) }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.Name != null ? a.Name.LookupVal : "a"), Convert.ToString(a.FromDate.Value.ToString("dd/MM/yyyy")), Convert.ToString(a.ToDate.Value.ToString("dd/MM/yyyy")), Convert.ToString(a.Default), a.Id }).ToList();
                        //jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Name != null ? a.Name.LookupVal : null, a.FromDate, a.ToDate, a.Default }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = Calendar;
                    Func<Calendar, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c =>                       gp.sidx == "Name" ? c.Name != null ? c.Name.LookupVal : null :
                                                               gp.sidx == "FromDate" ? c.FromDate.Value.ToString() :
                                                               gp.sidx == "ToDate" ? c.ToDate.Value.ToString() :
                                                               gp.sidx == "Default" ? c.Default.ToString() : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.Name != null ? a.Name.LookupVal : null), Convert.ToString(a.FromDate.Value.ToString("dd/MM/yyyy")), Convert.ToString(a.ToDate.Value.ToString("dd/MM/yyyy")), Convert.ToString(a.Default), a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.Name != null ? a.Name.LookupVal : null), Convert.ToString(a.FromDate.Value.ToString("dd/MM/yyyy")), Convert.ToString(a.ToDate.Value.ToString("dd/MM/yyyy")), Convert.ToString(a.Default), a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.Name != null ? a.Name.LookupVal : null), Convert.ToString(a.FromDate.Value.ToString("dd/MM/yyyy")), Convert.ToString(a.ToDate.Value.ToString("dd/MM/yyyy")), Convert.ToString(a.Default), a.Id }).ToList();
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
