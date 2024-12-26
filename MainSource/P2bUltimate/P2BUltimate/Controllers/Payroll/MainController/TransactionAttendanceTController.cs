using P2b.Global;
using P2BUltimate.App_Start;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.Entity;
using P2BUltimate.Models;
using System.Web.Script.Serialization;
using System.Threading.Tasks;
using System.Collections;
using System.Data.Entity.Core.Objects;
using System.Reflection;
using Payroll;
using P2BUltimate.Security;
namespace P2BUltimate.Controllers.Payroll.MainController
{
    [AuthoriseManger]
    public class TransactionAttendanceTController : Controller
    {
        // private DataBaseContext db = new DataBaseContext();
        public ActionResult Index()
        {
            return View();
        }


        public ActionResult PopulateDropDownList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var qurey = db.PayProcessGroup.ToList();
                var selected = (Object)null;
                if (data2 != "" && data != "0" && data2 != "0")
                {
                    selected = Convert.ToInt32(data2);
                }

                SelectList s = new SelectList(qurey, "Id", "Name", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }

        public List<int> one_ids(string form)
        {
            string itsec_id = form;
            List<int> ids = itsec_id.Split(',').Select(e => int.Parse(e)).ToList();
            return (ids);
        }
        public ActionResult Create(AttendanceT AT, FormCollection form) //Create submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string calender = form["calender"] == "0" ? "" : form["calender"];
                    string PayProcessGroup = form["PayProcessGroup"] == "0" ? "" : form["PayProcessGroup"];
                    string MonthDays = form["MonthDays"] == "0" ? "" : form["MonthDays"];
                    string PaybleDays = form["PaybleDays"] == "0" ? "" : form["PaybleDays"];
                    string employee = form["Employee-Table"] == "0" ? "" : form["Employee-Table"];

                    //if (calender != null)
                    //{
                    //    if (calender != "")
                    //    {
                    //        var val = db.Calendar.Find(int.Parse(calender));
                    //        AT.calender = val;
                    //    }
                    //}

                    if (PayProcessGroup != null)
                    {
                        if (PayProcessGroup != "")
                        {
                            var val = db.PayProcessGroup.Find(int.Parse(PayProcessGroup));
                            AT.PayProcessGroup = val;
                        }
                    }

                    if (MonthDays != null)
                    {
                        if (MonthDays != "")
                        {
                            var val = int.Parse(MonthDays);
                            AT.MonthDays = val;

                        }
                    }

                    if (PaybleDays != null)
                    {
                        if (PaybleDays != "")
                        {
                            var val = int.Parse(PaybleDays);
                            AT.PaybleDays = val;
                        }
                    }
                    if (employee != null)
                    {
                        if (employee != "")
                        {
                            var val = db.Employee.Find(int.Parse(PayProcessGroup));
                            AT.Employee = val;
                        }
                    }
                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {


                            AT.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            AttendanceT attendance = new AttendanceT()
                            {
                                PayProcessGroup = AT.PayProcessGroup,
                                MonthDays = AT.MonthDays,
                                PaybleDays = AT.PaybleDays,
                                Employee = AT.Employee,
                                DBTrack = AT.DBTrack
                            };
                            try
                            {
                                db.AttendanceT.Add(attendance);
                                db.SaveChanges();
                                ts.Complete();
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                // return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = AT.Id });
                            }
                            catch (DataException /* dex */)
                            {
                                Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                //return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
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
                        Msg.Add(errorMsg);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        //return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
                        //return this.Json(new { msg = errorMsg });
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
    }
    
}