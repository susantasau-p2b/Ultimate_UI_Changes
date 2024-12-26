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

namespace P2BUltimate.Controllers.Core.MainController
{
    public class Android_ApplicationController : Controller
    {
        //
        // GET: /Android_Application/
        public ActionResult Index()
        {
            return View("~/Views/Core/MainViews/Android_Application/Index.cshtml");

        }

        public ActionResult GridPartial()
        {
            return View("~/Views/Shared/Core/_AndroidApplicationEdit.cshtml");
        }

        public ActionResult GetEmpCode(int EmpId)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                string EmpCode = db.Employee.Where(e => e.Id == EmpId).FirstOrDefault().EmpCode;

                return Json(EmpCode, JsonRequestBehavior.AllowGet);

            }
        }

        public ActionResult GetData()
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                var oNewAppList = db.Android_Application.Where(e => e.Emp_Code == null).ToList();

                return Json(oNewAppList, JsonRequestBehavior.AllowGet);

            }
        }

        public ActionResult Partial()
        {
            return View("~/Views/Shared/Core/_AndroidApplication.cshtml");

        }

        public ActionResult GridEditData(int data)
        {
            var returnlist = new List<AndroidAppChildDataClass>();
            using (DataBaseContext db = new DataBaseContext())
            {
                if (data != null && data != 0)
                {
                    var retrundataList = db.Android_Application.Where(e => e.Id == data).ToList();
                    foreach (var a in retrundataList)
                    {
                        returnlist.Add(new AndroidAppChildDataClass()
                        {
                            ActiveUser = a.Active_User,
                            IsGeoFencingApp = a.IsGeoFenceAttendApplicable  
                        });
                    }
                    return Json(new { returndata = returnlist }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return View();
                }
            }
        }

        public ActionResult GridEditSave(Android_Application AndroidApp, FormCollection form, string data)
        {
            List<string> Msgs = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                int Id = int.Parse(data);
               Android_Application OAndroidApp = db.Android_Application.Where(e => e.Id == Id).SingleOrDefault();

               OAndroidApp.Active_User = Convert.ToBoolean(form["IsActive"]);
               OAndroidApp.IsGeoFenceAttendApplicable = Convert.ToBoolean(form["IsGeoFenceApp"]);
             

                using (TransactionScope ts = new TransactionScope())
                {

                    OAndroidApp.DBTrack = new DBTrack
                    {
                        CreatedBy = OAndroidApp.DBTrack.CreatedBy == null ? null : OAndroidApp.DBTrack.CreatedBy,
                        CreatedOn = OAndroidApp.DBTrack.CreatedOn == null ? null : OAndroidApp.DBTrack.CreatedOn,
                        Action = "M",
                        ModifiedBy = SessionManager.UserName,
                        ModifiedOn = DateTime.Now
                    };
 

                    try
                    {

                        
                            db.Android_Application.Attach(OAndroidApp);
                            db.Entry(OAndroidApp).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            db.Entry(OAndroidApp).State = System.Data.Entity.EntityState.Detached;
                            ts.Complete();
                            return this.Json(new { status = true, responseText = "Data Updated Successfully.", JsonRequestBehavior.AllowGet });
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        return RedirectToAction("Create", new { concurrencyError = true, id = OAndroidApp.Id });
                    }
                    catch (DataException /* dex */)
                    {
                        return this.Json(new { status = false, responseText = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
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
        public class AndroidAppChildDataClass
        {
            public int Id { get; set; } 
            public string PhoneNo { get; set; } 
            public string IMEINo { get; set; } 
            public bool ActiveUser { get; set; }
            public bool IsGeoFencingApp { get; set; }


        }
        public ActionResult Get_AppData(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var db_data = db.Android_Application
                        .Include(e => e.Employee) 
                        .Where(e => e.Id == data).ToList();
                    if (db_data.Count > 0)
                    {
                        List<AndroidAppChildDataClass> returndata = new List<AndroidAppChildDataClass>();
                        foreach (var item in db_data)
                        {
                            returndata.Add(new AndroidAppChildDataClass
                            {
                                Id = item.Id,
                                ActiveUser = item.Active_User,
                                IMEINo = item.IMEI_No,
                                PhoneNo = item.Phone_No,
                                IsGeoFencingApp = item.IsGeoFenceAttendApplicable
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

        public class new_Application
        {
            public int Id { get; set; }
            public string EmpCode { get; set; }
            public string PhoneNo { get; set; }

            public string IMEINo { get; set; }

            public bool ActiveUser { get; set; }
        }


        public class returndatagridclass //Parentgrid
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
        }
        public ActionResult Emp_Grid(ParamModel param, string y)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    List<returndatagridclass> model = new List<returndatagridclass>();
                    returndatagridclass view = null;

                    var all = db.Android_Application.Include(e => e.Employee).Include(e => e.Employee.EmpName).Where(e => e.Emp_Code != null && e.Employee != null)
                        .ToList();

                    IEnumerable<Android_Application> fall;
                    if (param.sSearch == null)
                    {
                        fall = all;
                    }
                    else
                    {
                        fall = all.Where(e => (e.Id.ToString().Contains(param.sSearch))
                                   || (e.Employee.EmpCode.ToString().ToUpper().Contains(param.sSearch.ToUpper()))
                                   || (e.Employee.EmpName.FullNameFML.ToString().ToUpper().Contains(param.sSearch.ToUpper()))
                            //|| (e.LoanAdvRequest.Select(t => t.LoanAdvRepaymentT.Select(q => q.InstallmentAmount.ToString().Contains(param.sSearch)))
                                   ).ToList();
                    }
                    foreach (var z in fall)
                    {
                        view = new returndatagridclass()
                        {
                            Id = z.Id,
                            Code = z.Employee.EmpCode,
                            Name = z.Employee.EmpName != null ? z.Employee.EmpName.FullNameFML : null,
                        };

                        model.Add(view);
                    }



                    //for column sorting
                    var sortindex = Convert.ToInt32(Request["iSortCol_0"]);

                    Func<returndatagridclass, string> orderfunc = (c =>
                                                                Convert.ToUInt32(sortindex) == 0 ? c.Id.ToString() :
                                                                sortindex == 1 ? c.Name : "");
                    var sortcolumn = Request["sSortDir_0"];

                    var dcompanies = model
                            .Skip(param.iDisplayStart).Take(param.iDisplayLength).ToList();
                    if (dcompanies.Count == 0)
                    {
                        List<returndatagridclass> result = new List<returndatagridclass>();
                        result = model;
                        return Json(new
                        {
                            sEcho = param.sEcho,
                            iTotalRecords = model.Count(),
                            iTotalDisplayRecords = fall.Count(),
                            data = result,
                        }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        var result = from c in dcompanies

                                     select new[] { null, Convert.ToString(c.Id), c.Name };
                        return Json(new
                        {
                            sEcho = param.sEcho,
                            iTotalRecords = model.Count(),
                            iTotalDisplayRecords = fall.Count(),
                            data = result,
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

        [HttpPost]
        public ActionResult Save(List<new_Application> data)
        {
            List<string> Msgs = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            { 
                try
                {
                    foreach (new_Application item in data)
                    {
                        Android_Application AndroidApp = db.Android_Application.Where(e => e.Id == item.Id).FirstOrDefault();
                        Employee OEmp = null;
                        if (item.EmpCode != "null" && item.EmpCode != "")
                        {
                            OEmp = db.Employee.Where(a => a.EmpCode == item.EmpCode).FirstOrDefault();
                            AndroidApp.Employee = OEmp;
                        }
                        var chk = db.Android_Application.Include(e => e.Employee).Where(e => e.Employee.Id == OEmp.Id && e.Emp_Code == OEmp.EmpCode).ToList();
                        if (chk.Count() > 0)
                        {
                            Msgs.Add("\n This Id -" + item.PhoneNo + " is already assigned to Employee - " + OEmp.EmpCode + ". Please delete existing record and re-assigned.");
                            continue;
                        }
                        int CompId = Convert.ToInt32(SessionManager.CompanyId);
                        Company OComp = db.Company.Where(a => a.Id == CompId).FirstOrDefault();
                        AndroidApp.Company = OComp;
                        using (TransactionScope ts = new TransactionScope())
                        {
                            if (item.EmpCode != "null" && item.EmpCode != "")
                            {
                                try
                                {
                                    AndroidApp.DBTrack = new DBTrack
                                    {
                                        CreatedBy = AndroidApp.DBTrack.CreatedBy == null ? null : AndroidApp.DBTrack.CreatedBy,
                                        CreatedOn = AndroidApp.DBTrack.CreatedOn == null ? null : AndroidApp.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };

                                    //AndroidApp = new Android_Application()
                                    //{
                                    AndroidApp.Company = OComp;
                                    AndroidApp.Active_User = item.ActiveUser;
                                    AndroidApp.Comp_Code = OComp.Code;
                                    AndroidApp.Emp_Code = OEmp.EmpCode;
                                    AndroidApp.Employee = OEmp;
                                    //AndroidApp.IMEI_No = item.IMEINo,
                                    //AndroidApp.Phone_No = item.PhoneNo,
                                    //AndroidApp.Id = item.Id,
                                    AndroidApp.DBTrack = AndroidApp.DBTrack;
                                    //};


                                    db.Android_Application.Attach(AndroidApp);
                                    db.Entry(AndroidApp).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    TempData["RowVersion"] = AndroidApp.RowVersion;
                                    db.Entry(AndroidApp).State = System.Data.Entity.EntityState.Detached;

                                    ts.Complete();
                                    Msgs.Add("\n" + item.PhoneNo + " - Data Saved successfully");
                                }
                                catch (Exception ex)
                                {
                                    Msgs.Add("\n" + item.PhoneNo + " - " + ex.Message);
                                }
                               
                            }
                            // return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                           
                            
                            
                        }
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    return RedirectToAction("Create", new { concurrencyError = true, id = 0 });
                }
                catch (DataException /* dex */)
                {
                    return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
                } 
            }
            return Json(new Utility.JsonReturnClass { success = true, responseText = Msgs }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GeoAppSave(List<int> data)
        {
            List<string> Msgs = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    
                    foreach (int item in data)
                    {
                        Android_Application AndroidApp = db.Android_Application.Include(e => e.Employee).Where(e => e.Employee.Id == item).FirstOrDefault();
                        if (AndroidApp != null)
                        {

                            if (AndroidApp.Active_User == true)
                            {
                                using (TransactionScope ts = new TransactionScope())
                                {

                                    AndroidApp.DBTrack = new DBTrack
                                    {
                                        CreatedBy = AndroidApp.DBTrack.CreatedBy == null ? null : AndroidApp.DBTrack.CreatedBy,
                                        CreatedOn = AndroidApp.DBTrack.CreatedOn == null ? null : AndroidApp.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };

                                    AndroidApp.IsGeoFenceAttendApplicable = true;
                                    AndroidApp.DBTrack = AndroidApp.DBTrack;

                                    db.Android_Application.Attach(AndroidApp);
                                    db.Entry(AndroidApp).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    TempData["RowVersion"] = AndroidApp.RowVersion;
                                    db.Entry(AndroidApp).State = System.Data.Entity.EntityState.Detached;

                                    ts.Complete();

                                    // return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });

                                    Msgs.Add(AndroidApp.Phone_No + "-" + "Data Saved successfully");

                                }
                            }
                            else
                            {
                                Msgs.Add(AndroidApp.Phone_No + "-" + "Employee not assigned to this IMEI No.");
                            }
                        }
                        else
                        {
                            Msgs.Add(item + "-" + "Record not found.");
                        }
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    return RedirectToAction("Create", new { concurrencyError = true, id = 0 });
                }
                catch (DataException /* dex */)
                {
                    return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
                }
            }
            return Json(new Utility.JsonReturnClass { success = true, responseText = Msgs }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GridDelete(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                Android_Application AndroidApp = db.Android_Application.Find(data);
              
                if (AndroidApp != null)
                {
                    if (AndroidApp.Active_User == false && AndroidApp.IsGeoFenceAttendApplicable == false)
                    {
                        db.Android_Application.Remove(AndroidApp); 
                        db.SaveChanges();
                    }
                    else
                    {
                        Msg.Add("  You can't delete this record.  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                }

                Msg.Add("Record Deleted Successfully  ");
                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
            }
        }

      
    }          
      
}