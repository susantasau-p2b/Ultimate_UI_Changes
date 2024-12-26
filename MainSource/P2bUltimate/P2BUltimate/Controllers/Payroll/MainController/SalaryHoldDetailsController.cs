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

namespace P2BUltimate.Controllers.Payroll.MainController
{
    [AuthoriseManger]
    public class SalaryHoldDetailsController : Controller
    {
        //  private DataBaseContext db = new DataBaseContext();
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/SalaryHoldDetails/Index.cshtml");
        }

        public ActionResult GridPartial()
        {
            return View("~/Views/Shared/Payroll/_SalaryHoldDetailsGridPartial.cshtml");
        }

        public ActionResult Create(SalaryHoldDetails c, FormCollection form) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    DateTime processdate = DateTime.Now;

                    string Emp = form["Employee-Table"] == "" ? "" : form["Employee-Table"];
                    Employee EmpData;
                    if (Emp != null && Emp != "")
                    {
                        int em = Convert.ToInt32(Emp);
                        EmpData = db.Employee.Include(q => q.SalaryHoldDetails).Where(q => q.Id == em).SingleOrDefault();
                    }
                    else
                    {
                        Msg.Add("  Kindly select employee  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    if (c.ToDate <= c.FromDate)
                    {
                        Msg.Add("  ToDate should not be less than FromDate.  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    string AttendanceDate = db.SalAttendanceT.OrderByDescending(e => e.Id).FirstOrDefault().PayMonth;
                    
                    DateTime datecheck = Convert.ToDateTime(AttendanceDate);
                    if (c.FromDate <= datecheck || c.ToDate <= datecheck)
                    {
                        Msg.Add("  FromDate and ToDate should be greater than last attendance date. ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    using (TransactionScope ts = new TransactionScope())
                    {
                        c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                        List<SalaryHoldDetails> OSalaryHoldDetails = new List<SalaryHoldDetails>();
                        SalaryHoldDetails SalaryHoldDetails = new SalaryHoldDetails()
                        {
                            FromDate = c.FromDate,
                            ToDate = c.ToDate,

                            DBTrack = c.DBTrack
                        };
                        try
                        {
                            db.SalaryHoldDetails.Add(SalaryHoldDetails);
                            db.SaveChanges();
                            OSalaryHoldDetails.Add(SalaryHoldDetails);
                            if (EmpData != null)
                            {
                                OSalaryHoldDetails.AddRange(EmpData.SalaryHoldDetails);
                                EmpData.SalaryHoldDetails = OSalaryHoldDetails;
                                db.Employee.Attach(EmpData);
                                db.Entry(EmpData).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(EmpData).State = System.Data.Entity.EntityState.Detached;
                            }

                            ts.Complete();
                            Msg.Add("  Data Saved successfully  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            // return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                        }
                        catch (DataException /* dex */)
                        {
                            Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            // return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
                        }
                    }
                }

                catch (Exception ex)
                {
                    throw;
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
            return null;
        }

        public ActionResult GridEditData(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.SalaryHoldDetails
                 .Where(e => e.Id == data).AsEnumerable().Select
                 (c => new
                 {

                     FromDate = c.FromDate.Value.ToShortDateString(),
                     ToDate = c.ToDate.Value.ToShortDateString(),

                 }).SingleOrDefault();
                return Json(new { data = Q }, JsonRequestBehavior.AllowGet);
            }
        }





        public ActionResult GridEditSave(SalaryHoldDetails c, FormCollection form, string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();

                if (data != null)
                {
                    var id = Convert.ToInt32(data);

                    if (c.ToDate <= c.FromDate)
                    {
                        Msg.Add("  ToDate should not be less than FromDate.  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    string AttendanceDate = db.SalAttendanceT.OrderByDescending(e => e.Id).FirstOrDefault().PayMonth;
                    DateTime datecheck = Convert.ToDateTime(AttendanceDate);

                    if (c.FromDate <= datecheck || c.ToDate <= datecheck)
                    {
                        Msg.Add("  FromDate and ToDate should be greater than last attendance date. ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    var db_data = db.SalaryHoldDetails.Where(e => e.Id == id).SingleOrDefault();

                    db_data.FromDate = c.FromDate;
                    db_data.ToDate = c.ToDate;
                    
                    try
                    {
                        db.SalaryHoldDetails.Attach(db_data);
                        db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;
                        return Json(new { status = true, data = db_data, responseText = "Record Updated..!" }, JsonRequestBehavior.AllowGet);
                    }
                    catch (Exception e)
                    {

                        throw e;
                    }
                }
                else
                {
                    return Json(new { status = false, responseText = "Data Is Null" }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public ActionResult GridDelete(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var LvEP = db.SalaryHoldDetails.Find(data);
                db.SalaryHoldDetails.Remove(LvEP);
                db.SaveChanges();
                List<string> Msgs = new List<string>();
                Msgs.Add("Record Deleted Successfully ");
                return Json(new Utility.JsonReturnClass { success = true, responseText = Msgs }, JsonRequestBehavior.AllowGet);

            }
        }
        public class returndatagridclass
        {
            public string Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
        }

        public class ChildDataClass
        {
            public int Id { get; set; }
            public string FromDate { get; set; }
            public string ToDate { get; set; }

        }

        public ActionResult Emp_Grid(ParamModel param, string y)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var all = db.EmployeePayroll
                        .Include(e => e.Employee)
                        .Include(e => e.Employee.EmpName)
                        .Include(e => e.Employee.ServiceBookDates)
                        .Where(e => e.Employee.ServiceBookDates.ServiceLastDate == null)
                        .AsNoTracking().AsParallel().ToList();
                    // .Include(e => e.ServiceSecurity).ToList();
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
                        List<returndatagridclass> result = new List<returndatagridclass>();
                        foreach (var item in fall)
                        {
                            result.Add(new returndatagridclass
                            {
                                Id = item.Id.ToString(),
                                Code = item.Employee.EmpCode,
                                Name = item.Employee.EmpName.FullNameFML,
                            });
                        }
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
                catch (Exception e)
                {
                    List<string> Msg = new List<string>();
                    LogFile Logfile = new LogFile();
                    ErrorLog Err = new ErrorLog()
                    {
                        ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                        ExceptionMessage = e.Message,
                        ExceptionStackTrace = e.StackTrace,
                        LineNo = Convert.ToString(new System.Diagnostics.StackTrace(e, true).GetFrame(0).GetFileLineNumber()),
                        LogTime = DateTime.Now
                    };
                    Logfile.CreateLogFile(Err);
                    Msg.Add(e.Message);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public ActionResult Get_SalaryHoldDetails(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var db_data = db.Employee
                        .Include(e => e.SalaryHoldDetails)
                        .Where(e => e.Id == data).AsNoTracking().AsParallel().SingleOrDefault();
                    if (db_data != null)
                    {
                        List<ChildDataClass> returndata = new List<ChildDataClass>();
                        foreach (var c in db_data.SalaryHoldDetails)
                        {
                            returndata.Add(new ChildDataClass
                            {
                                Id = c.Id,
                                FromDate = c.FromDate.Value.ToShortDateString(),
                                ToDate = c.ToDate.Value.ToShortDateString(),

                            });
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

    }
}