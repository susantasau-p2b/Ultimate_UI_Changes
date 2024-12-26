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
using Payroll;
using Leave;
using Training;
using Recruitment;
using Attendance;
using Appraisal;
using EMS;

namespace P2BUltimate.Controllers
{
    [AuthoriseManger]
    public class InitialCompanyCreateController : Controller
    {
        //
        // GET: /InitialCompanyCreate/
        //private DataBaseContext db = new DataBaseContext();
        public ActionResult Index()
        {
            return View("~/Views/Shared/InitialCompanyCreate.cshtml");
        }
        public ActionResult CreateCalender(Calendar c, FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    if (ModelState.IsValid)
                    {
                        c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                        var Name = form["Name_drop"] == "0" ? 0 : Convert.ToInt32(form["Name_drop"]);
                        if (Name != 0)
                        {
                            c.Name = db.LookupValue.Find(Name);
                        }
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
                                    //ModelState.AddModelError(string.Empty, "To Date should be greater than From Date.");
                                    Msg.Add("  To Date should be greater than From Date ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    //return this.Json(new Object[] { "", "", "To Date should be greater than From Date.", JsonRequestBehavior.AllowGet });
                                    //return View(c);
                                }

                                if (db.Calendar.Any(o => o.FromDate == c.FromDate))
                                {
                                    Msg.Add("  From Date already exists. ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    //return this.Json(new Object[] { "", "", "From Date already exists.", JsonRequestBehavior.AllowGet });
                                }

                                if (db.Calendar.Any(o => o.ToDate == c.ToDate))
                                {
                                    //ModelState.AddModelError(string.Empty, "To Date already exists.");
                                    Msg.Add("  To Date Already Exists.  ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    // return this.Json(new Object[] { "", "", "To Date already exists.", JsonRequestBehavior.AllowGet });
                                    // return View(c);
                                }

                                db.Calendar.Add(calendar);
                                db.SaveChanges();
                                ts.Complete();
                            } Msg.Add("  Data Saved successfully  ");
                            return Json(new Utility.JsonReturnClass { Id = calendar.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            // return Json(new Object[] { calendar.Id, "", "Data saved successfully." }, JsonRequestBehavior.AllowGet);
                            // return this.Json(new { Error = calendar.Id });
                            //return RedirectToAction("Index");
                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            return RedirectToAction("Create", new { concurrencyError = true, id = c.Id });
                        }
                        catch (DataException /* dex */)
                        {
                            //Log the error (uncomment dex variable name after DataException and add a line here to write a log.
                            ModelState.AddModelError(string.Empty, "Unable to create. Try again, and if the problem persists contact your system administrator.");
                            return View(c);
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
        public ActionResult partial()
        {
            return View("~/Views/Shared/Core/_Calender.cshtml");

        }
        public ActionResult PopulateDropDownListCalendar(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var qurey = db.Calendar.ToList();
                var selected = (Object)null;
                if (data2 != "" && data != "0" && data2 != "0")
                {
                    selected = Convert.ToInt32(data2);
                }

                SelectList s = new SelectList(qurey, "Id", "FullDetails", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }
        public List<int> one_ids(string form)
        {
            string itsec_id = form;
            List<int> ids = itsec_id.Split(',').Select(e => int.Parse(e)).ToList();
            return (ids);
        }

        public ActionResult Create(Company comp, FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Financial_Year = form["Financial_Yearlist"] == "0" ? 0 : Convert.ToInt32(form["Financial_Yearlist"]);

                List<Calendar> ObjCalendar = new List<Calendar>();


                if (Financial_Year != 0)
                {
                    //var ids = one_ids(Financial_Year);
                    //foreach (var ca in ids)
                    //{
                    var value = db.Calendar.Find(Financial_Year);
                    ObjCalendar.Add(value);
                    comp.Calendar = ObjCalendar;
                    //}

                }

                if (ModelState.IsValid)
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        comp.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                        Company OBJComp = new Company()
                        {
                            Calendar = comp.Calendar,
                            Code = comp.Code,
                            Name = comp.Name,
                            DBTrack = comp.DBTrack
                        };
                        try
                        {
                            db.Company.Add(OBJComp);
                            db.SaveChanges();
                            if (!db.CompanyPayroll.Any(e => e.Company.Id == OBJComp.Id))
                            {
                                var oCompanyPayroll = new CompanyPayroll();
                                oCompanyPayroll.Company = OBJComp;
                                oCompanyPayroll.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                db.CompanyPayroll.Add(oCompanyPayroll);
                                db.SaveChanges();
                            }
                            if (!db.CompanyLeave.Any(e => e.Company.Id == OBJComp.Id))
                            {
                                var oCompanyLeave = new CompanyLeave();
                                oCompanyLeave.Company = OBJComp;
                                oCompanyLeave.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                db.CompanyLeave.Add(oCompanyLeave);
                                db.SaveChanges();
                            }
                            if (!db.CompanyTraining.Any(e => e.Company.Id == OBJComp.Id))
                            {
                                var oCompanyTraining = new CompanyTraining();
                                oCompanyTraining.Company = OBJComp;
                                oCompanyTraining.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                db.CompanyTraining.Add(oCompanyTraining);
                                db.SaveChanges();
                            }
                            if (!db.CompanyRecruitment.Any(e => e.Company.Id == OBJComp.Id))
                            {
                                var oCompanyRecruitment = new CompanyRecruitment();
                                oCompanyRecruitment.Company = OBJComp;
                                oCompanyRecruitment.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                db.CompanyRecruitment.Add(oCompanyRecruitment);
                                db.SaveChanges();
                            }
                            if (!db.CompanyAttendance.Any(e => e.Company.Id == OBJComp.Id))
                            {
                                var oCompanyAttendance = new CompanyAttendance();
                                oCompanyAttendance.Company = OBJComp;
                                oCompanyAttendance.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                db.CompanyAttendance.Add(oCompanyAttendance);
                                db.SaveChanges();
                            }
                            if (!db.CompanyAppraisal.Any(e => e.Company.Id == OBJComp.Id))
                            {
                                var oCompanyAppraisal = new CompanyAppraisal();
                                oCompanyAppraisal.Company = OBJComp;
                                oCompanyAppraisal.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                db.CompanyAppraisal.Add(oCompanyAppraisal);
                                db.SaveChanges();
                            }
                            if (!db.CompanyExit.Any(e => e.Company.Id == OBJComp.Id))
                            {
                                var oCompanyExit = new CompanyExit();
                                oCompanyExit.Company = OBJComp;
                                oCompanyExit.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                db.CompanyExit.Add(oCompanyExit);
                                db.SaveChanges();
                            }
                            ts.Complete();
                            return Json(new { success = true, data = Url.Action("logout", "login"), responseText = "Data Created Successfully." }, JsonRequestBehavior.AllowGet);
                        }

                        catch (DbUpdateConcurrencyException)
                        {
                            return RedirectToAction("Create", new { concurrencyError = true, id = comp.Id });
                        }
                        catch (DataException /* dex */)
                        {
                            return this.Json(new { success = false, data = OBJComp.Id, responseText = "Unable to create. Try again, and if the problem persists contact your system administrator.." }, JsonRequestBehavior.AllowGet);
                        }
                    }
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
                    return this.Json(new { success = false, responseText = errorMsg, JsonRequestBehavior.AllowGet });
                    // return this.Json(new { msg = errorMsg });
                }
            }
        }
    }
}