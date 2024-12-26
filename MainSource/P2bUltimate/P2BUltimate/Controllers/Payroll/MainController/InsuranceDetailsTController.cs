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
using Leave;
using P2BUltimate.Process;
using System.Diagnostics;
using P2BUltimate.Security;

namespace P2BUltimate.Controllers.Payroll.MainController
{
    [AuthoriseManger]
    public class InsuranceDetailsTController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/InsuranceDetailsT/Index.cshtml");
        }

        public ActionResult partial()
        {
            return View("~/Views/Shared/Payroll/_InsuranceDetailsT.cshtml");

        }

        public ActionResult ChkProcess(string typeofbtn, string month, string EmpCode)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                bool selected = false;
                var query1 = db.EmployeePayroll.Include(e => e.InsuranceDetailsT).ToList();
                foreach (var b in query1)
                {
                    foreach (var c in b.InsuranceDetailsT)
                    {
                        if (c.Id.ToString() == EmpCode)
                        {
                            var queryT = db.EmployeePayroll.Include(e => e.SalaryT).Where(e => e.Id == b.Id).SingleOrDefault();
                            if (queryT != null)
                            {
                                var query = queryT.SalaryT.Where(e => e.PayMonth == month).ToList();
                                if (query.Count > 0)
                                {
                                    selected = true;
                                }
                                var data = new
                                {
                                    status = selected,
                                };
                                return Json(data, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
                }
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetInsuranceDetailsTDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.InsuranceProduct.ToList();
                IEnumerable<InsuranceProduct> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.InsuranceProduct.ToList().Where(d => d.FullDetails.Contains(data));
                }
                else
                {
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.FullDetails }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public List<int> one_ids(string form)
        {
            string itsec_id = form;
            List<int> ids = itsec_id.Split(',').Select(e => int.Parse(e)).ToList();
            return (ids);
        }

        public ActionResult Create(InsuranceDetailsT ID, FormCollection form, String forwarddata) //Create submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string Emp = form["Employee-Table"] == "0" ? "" : form["Employee-Table"];
                    string Frequencylist = form["Frequencylist"] == "0" ? "" : form["Frequencylist"];
                    string OperationStatuslist = form["OperationStatuslist"] == "0" ? "" : form["OperationStatuslist"];
                    string InsuranceProductlist = form["InsuranceProductlist"] == "0" ? "" : form["InsuranceProductlist"];
                    string employee = form["Employee-Table"] == "0" ? "" : form["Employee-Table"];
                    List<int> ids = null;
                    if (Emp != null && Emp != "0" && Emp != "false")
                    {
                        ids = one_ids(Emp);
                    }
                    else
                    {
                        Msg.Add("  Kindly select employee  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    if (Frequencylist != null && Frequencylist != "")
                    {
                        var Val = db.LookupValue.Find(int.Parse(Frequencylist));
                        ID.Frequency = Val;
                    }
                    if (OperationStatuslist != null && OperationStatuslist != "")
                    {
                        var Val = db.LookupValue.Find(int.Parse(OperationStatuslist));
                        ID.OperationStatus = Val;
                    }
                    if (InsuranceProductlist != null && InsuranceProductlist != "")
                    {
                        var Val = db.InsuranceProduct.Find(int.Parse(InsuranceProductlist));
                        ID.InsuranceProduct = Val;
                    }
                    else
                    {
                        Msg.Add("Kindly select The Insurance Product  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    Employee OEmployee = null;
                    EmployeePayroll OEmployeePayroll = null;

                    ID.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                    InsuranceDetailsT ObjID = new InsuranceDetailsT();
                    {
                        ObjID.Frequency = ID.Frequency;
                        ObjID.FromDate = ID.FromDate;
                        ObjID.InsuranceProduct = ID.InsuranceProduct;
                        ObjID.ToDate = ID.ToDate;
                        ObjID.PolicyNo = ID.PolicyNo;
                        ObjID.Premium = ID.Premium;
                        ObjID.SumAssured = ID.SumAssured;
                        ObjID.OperationStatus = ID.OperationStatus;
                        ObjID.DBTrack = ID.DBTrack;

                    }
                    if (ModelState.IsValid)
                    {
                        if (ids != null)
                        {
                            foreach (var i in ids)
                            {
                                OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct)
                                            .Where(r => r.Id == i).SingleOrDefault();

                                OEmployeePayroll
                                = db.EmployeePayroll
                              .Where(e => e.Employee.Id == i).SingleOrDefault();

                                using (TransactionScope ts = new TransactionScope())
                                {
                                    try
                                    {
                                        db.InsuranceDetailsT.Add(ObjID);
                                        db.SaveChanges();
                                        List<InsuranceDetailsT> OFAT = new List<InsuranceDetailsT>();
                                        OFAT.Add(db.InsuranceDetailsT.Find(ObjID.Id));

                                        if (OEmployeePayroll == null)
                                        {
                                            EmployeePayroll OTEP = new EmployeePayroll()
                                            {
                                                Employee = db.Employee.Find(OEmployee.Id),
                                                InsuranceDetailsT = OFAT,
                                                DBTrack = ID.DBTrack

                                            };


                                            db.EmployeePayroll.Add(OTEP);
                                            db.SaveChanges();
                                        }
                                        else
                                        {
                                            var aa = db.EmployeePayroll.Find(OEmployeePayroll.Id);
                                            aa.InsuranceDetailsT = OFAT;
                                            //OEmployeePayroll.DBTrack = dbt;
                                            db.EmployeePayroll.Attach(aa);
                                            db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();
                                            db.Entry(aa).State = System.Data.Entity.EntityState.Detached;

                                        }
                                        ts.Complete();
                                        //return Json(new Object[] { "", "", "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
                                        List<string> Msgs = new List<string>();
                                        Msgs.Add("Data Saved successfully");
                                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msgs }, JsonRequestBehavior.AllowGet);

                                    }
                                    catch (Exception ex)
                                    {
                                        //List<string> Msg = new List<string>();
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
                        // List<string> Msgu = new List<string>();
                        Msg.Add("  Unable to create...  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        // return this.Json(new Object[] { "", "", "Unable to create...", JsonRequestBehavior.AllowGet });
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
                        //var errorMsg = sb.ToString();
                        //return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
                        ////return this.Json(new { msg = errorMsg });
                        List<string> MsgB = new List<string>();
                        var errorMsg = sb.ToString();
                        MsgB.Add(errorMsg);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = MsgB }, JsonRequestBehavior.AllowGet);
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

        public JsonResult EditGridDetails(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.InsuranceDetailsT
                     .Include(e => e.InsuranceProduct)
                     .Include(e => e.Frequency)
                     .Include(e => e.OperationStatus)
                     .Where(e => e.Id == data).Select
                     (e => new
                     {
                         Frequency = e.Frequency != null ? e.Frequency.LookupVal : null,
                         FromDate = e.FromDate,
                         InsuranceProduct = e.InsuranceProduct,
                         ToDate = e.ToDate,
                         PolicyNo = e.PolicyNo,
                         Premium = e.Premium,
                         SumAssured = e.SumAssured,
                         OperationStatus = e.OperationStatus != null ? e.OperationStatus.Id.ToString() : null,
                         Action = e.DBTrack.Action
                     }).ToList();
                var yearlypymentT = db.InsuranceDetailsT.Find(data);
                Session["RowVersion"] = yearlypymentT.RowVersion;
                var Auth = yearlypymentT.DBTrack.IsModified;
                //return Json(new Object[] { Q, add_data, "", Auth, JsonRequestBehavior.AllowGet });
                return Json(new { data = Q }, JsonRequestBehavior.AllowGet);
            }
        }


        public class InsuranceDetailsTChildDataClass
        {
            public string Id { get; set; }
            public string PolicyNo { get; set; }
            public string SumAssured { get; set; }
            public string Premium { get; set; }
            public string Status { get; set; }
            public string FromDate { get; set; }
            public string ToDate { get; set; }
        }
        public ActionResult Get_InsuranceDetails(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var db_data = db.EmployeePayroll
                        .Include(e => e.InsuranceDetailsT)
                        .Include(e => e.InsuranceDetailsT.Select(t => t.OperationStatus))
                        .Where(e => e.Id == data).ToList();
                    if (db_data.Count > 0)
                    {
                        List<InsuranceDetailsTChildDataClass> returndata = new List<InsuranceDetailsTChildDataClass>();
                        foreach (var item in db_data.SelectMany(e => e.InsuranceDetailsT))
                        {
                            returndata.Add(new InsuranceDetailsTChildDataClass
                            {
                                Id = item.Id.ToString(),
                                PolicyNo = item.PolicyNo.ToString(),
                                SumAssured = item.SumAssured.ToString(),
                                Premium = item.Premium.ToString(),
                                Status = item.OperationStatus.LookupVal.ToUpper(),
                                FromDate = item.FromDate.Value.ToShortDateString(),
                                ToDate = item.ToDate.Value.ToShortDateString()

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

        public ActionResult GridEditSave(InsuranceDetailsT ID, FormCollection form, string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                var OpStatus = form["OpStatus"] == "0" ? "" : form["OpStatus"];
                var CloserDate = form["CloserDate"] == "0" ? "" : form["CloserDate"];
                var Closedate = Convert.ToDateTime(CloserDate);
                DateTime cdate=Convert.ToDateTime(CloserDate);
                if (data != null)
                {
                    var ids = Convert.ToInt32(data);
                    var db_dataemp = db.InsuranceDetailsT.Where(e => e.Id == ids).SingleOrDefault();
                    if (Convert.ToDateTime(CloserDate)<db_dataemp.FromDate)
                    {
                        Msg.Add("  Closer date should not less than From date...!  ");
                        return this.Json(new { status = false, responseText = Msg, JsonRequestBehavior.AllowGet });
                    }
                    string paymon=cdate.ToString("MM/yyyy");
                    var salt = db.SalaryT.Where(e => e.EmployeePayroll_Id == db_dataemp.EmployeePayroll_Id && e.PayMonth == paymon).SingleOrDefault();
                    if (salt!=null)
                    {
                         Msg.Add("Please delete salary for this employee and try again...!  ");
                         return this.Json(new { status = false, responseText = Msg, JsonRequestBehavior.AllowGet });
                    }
                }
                if (data != null)
                {
                    var id = Convert.ToInt32(data);
                    var db_data = db.InsuranceDetailsT.Where(e => e.Id == id).SingleOrDefault();
                    db_data.SumAssured = ID.SumAssured;
                    db_data.PolicyNo = ID.PolicyNo;
                    db_data.Premium = ID.Premium;
                    db_data.ToDate = Closedate;
                    var opstatus_id = Convert.ToInt32(OpStatus);
                    db_data.OperationStatus = db.LookupValue.Where(e => e.Id == opstatus_id).SingleOrDefault();
                    try
                    {
                        db.InsuranceDetailsT.Attach(db_data);
                        db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;
                        return this.Json(new { status = true, responseText = "Data Updated Successfully.", JsonRequestBehavior.AllowGet });
                    }
                    catch (Exception e)
                    {

                        throw e;
                    }
                }
                else
                {
                    return this.Json(new { status = false, responseText = "Data Is Null" }, JsonRequestBehavior.AllowGet);
                }
            }
        }


        #region DELETE
        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        try
                        {
                            //var Emp = db.EmployeePayroll.Where(e => e.Employee.Id == data).Select(e => e.SalAttendance).SingleOrDefault();
                            //SalAttendanceT SalAttendanceT = Emp.Where(e => e.LWPDays == null).SingleOrDefault();
                            InsuranceDetailsT InsuranceDetailsT = db.InsuranceDetailsT.Find(data);
                            db.Entry(InsuranceDetailsT).State = System.Data.Entity.EntityState.Deleted;
                            await db.SaveChangesAsync();
                            ts.Complete();
                            return this.Json(new { status = true, responseText = "Data removed.", JsonRequestBehavior.AllowGet });

                        }
                        catch (RetryLimitExceededException /* dex */)
                        {
                            //  return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
                            Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

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
                            // List<string> Msg = new List<string>();
                            Msg.Add(ex.Message);
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

            }
        #endregion
        }
    }
}