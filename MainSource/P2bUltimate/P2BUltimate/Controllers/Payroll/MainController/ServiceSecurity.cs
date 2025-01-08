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
    public class ServiceSecurityController : Controller
    {
        //  private DataBaseContext db = new DataBaseContext();
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/ServiceSecurity/Index.cshtml");
        }

        public ActionResult GridPartial()
        {
            return View("~/Views/Shared/Payroll/_ServiceSecurityGridPartial.cshtml");
        }

        public ActionResult Create(ServiceSecurity c, FormCollection form) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    DateTime processdate = DateTime.Now;

                    string Location = form["LocationList_DDL"] == "0" ? "" : form["LocationList_DDL"];
                    string Emp = form["Employee-Table"] == "" ? "" : form["Employee-Table"];
                    EmployeePayroll EmpData;
                    if (Emp != null && Emp != "")
                    {
                        int em = Convert.ToInt32(Emp);
                        EmpData = db.EmployeePayroll.Include(q => q.Employee).Include(q => q.ServiceSecurity).Where(q => q.Employee.Id == em).SingleOrDefault();
                    }
                    else
                    {
                        Msg.Add("  Kindly select employee  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    if (Location != null && Location != "")
                    {
                        int ContId = Convert.ToInt32(Location);
                        var val = db.Location.Where(e => e.Id == ContId).SingleOrDefault();
                        c.Location = val;
                    }
                    using (TransactionScope ts = new TransactionScope())
                    {
                        c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                        List<ServiceSecurity> OServiceSecurity = new List<ServiceSecurity>();
                        ServiceSecurity ServiceSecurity = new ServiceSecurity()
                        {
                            Amount = c.Amount,
                            Closer = c.Closer,
                            Date = c.Date,
                            DateOfCloser = c.DateOfCloser,
                            DateOfMaturity = c.DateOfMaturity,
                            FDR_No = c.FDR_No,
                            Remark = c.Remark,
                            Location=c.Location,
                            DBTrack = c.DBTrack
                        };
                        try
                        {
                            db.ServiceSecurity.Add(ServiceSecurity);
                            db.SaveChanges();
                            OServiceSecurity.Add(ServiceSecurity);
                            if (EmpData != null)
                            {
                                OServiceSecurity.AddRange(EmpData.ServiceSecurity);
                                EmpData.ServiceSecurity = OServiceSecurity;
                                db.EmployeePayroll.Attach(EmpData);
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
                var Q = db.ServiceSecurity.Include(e => e.Location).Include(e => e.Location.LocationObj)
                 .Where(e => e.Id == data).AsEnumerable().Select
                 (c => new
                 {
                     Amount = c.Amount.ToString(),
                     Closer2 = c.Closer,
                     Date = c.Date.Value.ToShortDateString(),
                     DateOfCloser = c.DateOfCloser != null ? c.DateOfCloser.Value.ToShortDateString() : "",
                     DateOfMaturity = c.DateOfMaturity.Value.ToShortDateString(),
                     FDR_No = c.FDR_No.ToString(),
                     Remark = c.Remark,
                     //Location = c.Location.LocationObj.LocDesc,
                     Location = c.Location != null ? c.Location.Id.ToString() : null,
                 }).SingleOrDefault();
                return Json(new { data = Q }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult LocationBranchDrop(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                SelectList s = (SelectList)null;
                var selected = "";
                //var id = Convert.ToInt32(data);
                var qurey = db.Location.Include(e => e.LocationObj).ToList(); // added by rekha 26-12-16
                if (data2 != "" && data2 != "0")
                {
                    selected = data2;
                }
                if (qurey != null)
                {
                    s = new SelectList(qurey, "Id", "FullDetails", selected);
                }
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult PopulateLocationList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var selected = (String)null;
                var qurey = db.Location.Include(e => e.LocationObj).ToList();
                if (data2 != null && data2 != "")
                {
                    selected = data2;
                }
                if (data != null && data != "")
                {
                    var id = Convert.ToInt32(data);
                    qurey = db.Location.Include(e => e.LocationObj).Where(e => e.Id == id).ToList();
                }
                SelectList s = new SelectList(qurey, "Id", "FullDetails", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GridEditSave(ServiceSecurity c, FormCollection form, string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (data != null)
                {
                    var id = Convert.ToInt32(data);
                    bool Closer = Convert.ToBoolean(form["Closer2"]);
                    string Location = form["LocationList_DDL"] == "0" ? "" : form["LocationList_DDL"];
                    if (Location != null && Location != "")
                    {
                        int ContId = Convert.ToInt32(Location);
                        var val = db.Location.Where(e => e.Id == ContId).SingleOrDefault();
                        c.Location = val;
                    }
                    var db_data = db.ServiceSecurity.Where(e => e.Id == id).SingleOrDefault();
                    db_data.Amount = c.Amount;
                    db_data.Closer = Closer;
                    db_data.Date = c.Date;
                    db_data.DateOfCloser = c.DateOfCloser;
                    db_data.DateOfMaturity = c.DateOfMaturity;
                    db_data.FDR_No = c.FDR_No;
                    db_data.Remark = c.Remark;
                    db_data.Location = c.Location;
                    try
                    {
                        db.ServiceSecurity.Attach(db_data);
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
                var LvEP = db.ServiceSecurity.Find(data);
                db.ServiceSecurity.Remove(LvEP);
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
            public string Amount { get; set; }
            public string Closer { get; set; }
            public string Date { get; set; }
            public string DateOfCloser { get; set; }
            public string DateOfMaturity { get; set; }
            public string FDR_No { get; set; }
            public string Remark { get; set; }
        }

        public ActionResult Emp_Grid(ParamModel param, string y)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var all = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.Employee.EmpName).AsNoTracking().AsParallel().ToList();
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

        public ActionResult Get_LvCancelReq(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var db_data = db.EmployeePayroll
                        .Include(e => e.ServiceSecurity)
                        .Where(e => e.Id == data).AsNoTracking().AsParallel().SingleOrDefault();
                    if (db_data != null)
                    {
                        List<ChildDataClass> returndata = new List<ChildDataClass>();
                        foreach (var c in db_data.ServiceSecurity)
                        {
                            returndata.Add(new ChildDataClass
                            {
                                Id = c.Id,
                                Amount = c.Amount.ToString(),
                                Closer = c.Closer.ToString(),
                                Date = c.Date.Value.ToShortDateString(),
                                DateOfCloser = c.DateOfCloser != null ? c.DateOfCloser.Value.ToShortDateString() : "",
                                DateOfMaturity = c.DateOfMaturity.Value.ToShortDateString(),
                                FDR_No = c.FDR_No.ToString(),
                                Remark = c.Remark,
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