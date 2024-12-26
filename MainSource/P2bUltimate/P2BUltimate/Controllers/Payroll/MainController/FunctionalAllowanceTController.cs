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
using P2BUltimate.Process;
using System.Diagnostics;
using P2BUltimate.Security;
namespace P2BUltimate.Controllers.Payroll.MainController
{
    [AuthoriseManger]
    public class FunctionalAllowanceTController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();
        public ActionResult Index()
        {
            return View();
        }


        public List<int> one_ids(string form)
        {
            string itsec_id = form;
            List<int> ids = itsec_id.Split(',').Select(e => int.Parse(e)).ToList();
            return (ids);
        }


        public ActionResult Create(FunctAttendanceT F, FormCollection form, String forwarddata) //Create submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string Emp = forwarddata == "0" ? "" : forwarddata;
                    string PayProcessgropp = form["DDLPayProcessGroup"] == "0" ? "" : form["DDLPayProcessGroup"];
                    string ProcessMonth = form["ProcessMonth"] == "0" ? "" : form["ProcessMonth"];

                    if (PayProcessgropp != null && PayProcessgropp != "")
                    {
                        var val = db.PayProcessGroup.Find(int.Parse(PayProcessgropp));
                        F.PayProcessGroup = val;

                    }


                    var EmpVariable = db.Employee.Include(e => e.GeoStruct).Include(e => e.GeoStruct.Location)
                                    .Where(r => r.Id == 1).SingleOrDefault();

                    List<int> ids = null;
                    if (Emp != null && Emp != "0" && Emp != "false")
                    {
                        ids = one_ids(Emp);
                    }
                    else
                    {
                        Msg.Add(" Kindly select employee");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        // return Json(new { sucess = false, responseText = "Kindly select employee" }, JsonRequestBehavior.AllowGet);
                    }

                    Employee OEmployee = null;

                    EmployeePayroll OEmployeePayroll = null;

                    F.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                    FunctAttendanceT ObjFAT = new FunctAttendanceT();
                    {
                        ObjFAT.ProcessMonth = F.ProcessMonth;
                        ObjFAT.PayMonth = F.PayMonth;
                        ObjFAT.PayProcessGroup = F.PayProcessGroup;
                        if (ObjFAT.GeoStruct != null)
                        {
                            ObjFAT.GeoStruct = db.GeoStruct.Find(OEmployee.GeoStruct.Id);
                        };
                        //OEmpSalStruct.GeoStruct = db.GeoStruct.Find( OEmployeePayroll.GeoStruct.Id);
                        if (ObjFAT.FuncStruct != null)
                        {
                            ObjFAT.FuncStruct = db.FuncStruct.Find(OEmployee.FuncStruct.Id);
                        };
                        if (ObjFAT.PayStruct != null)
                        {

                            ObjFAT.PayStruct = db.PayStruct.Find(OEmployee.PayStruct.Id == null ? 0 : OEmployee.PayStruct.Id);
                        };
                        //OEmpSalStruct.PayStruct = db.PayStruct.Find( OEmployeePayroll.PayStruct.Id);
                        ObjFAT.DBTrack = F.DBTrack;

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
                                        db.FunctAttendanceT.Add(ObjFAT);
                                        db.SaveChanges();
                                        List<FunctAttendanceT> OFAT = new List<FunctAttendanceT>();
                                        OFAT.Add(db.FunctAttendanceT.Find(ObjFAT.Id));

                                        if (OEmployeePayroll == null)
                                        {
                                            EmployeePayroll OTEP = new EmployeePayroll()
                                            {
                                                Employee = db.Employee.Find(OEmployee.Id),
                                                FunctAttendanceT = OFAT,
                                                DBTrack = F.DBTrack

                                            };


                                            db.EmployeePayroll.Add(OTEP);
                                            db.SaveChanges();
                                        }
                                        else
                                        {
                                            var aa = db.EmployeePayroll.Find(OEmployeePayroll.Id);
                                            aa.FunctAttendanceT = OFAT;
                                            //OEmployeePayroll.DBTrack = dbt;
                                            db.EmployeePayroll.Attach(aa);
                                            db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();
                                            db.Entry(aa).State = System.Data.Entity.EntityState.Detached;

                                        }
                                        ts.Complete();
                                        // return Json(new { sucess = true, responseText = "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
                                        Msg.Add("  Data Saved successfully  ");
                                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    }
                                    catch (DataException ex)
                                    {
                                        //LogFile Logfile = new LogFile();
                                        //ErrorLog Err = new ErrorLog()
                                        //{
                                        //    ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                                        //    ExceptionMessage = ex.Message,
                                        //    ExceptionStackTrace = ex.StackTrace,
                                        //    LineNo = Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                                        //    LogTime = DateTime.Now
                                        //};
                                        //Logfile.CreateLogFile(Err);
                                        // return Json(new { sucess = false, responseText = ex.InnerException }, JsonRequestBehavior.AllowGet);
                                    }


                                }
                            }
                        }
                        Msg.Add("  Unable to create...  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        //   return this.Json(new Object[] { "", "", "Unable to create...", JsonRequestBehavior.AllowGet });
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


        public JsonResult Edit(int data)
        {
            int ID = Convert.ToInt32(data);
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.FunctAttendanceT
                    .Where(e => e.Id == ID).Select
                    (e => new
                    {
                        HourDays = e.HourDays,
                        PayMonth = e.PayMonth,
                        ProcessMonth = e.ProcessMonth,
                        Reason = e.Reason,
                        Payprocessgroup_Id = e.PayProcessGroup.Id == null ? 0 : e.PayProcessGroup.Id,
                        EditPayprocessgroup = e.PayProcessGroup.FullDetails,
                        Action = e.DBTrack.Action
                    }).ToList();


                var FunctAtT = db.FunctAttendanceT.Find(ID);
                TempData["RowVersion"] = FunctAtT.RowVersion;
                var Auth = FunctAtT.DBTrack.IsModified;
                return Json(new Object[] { Q, "", Auth }, JsonRequestBehavior.AllowGet);
            }
        }

        public async Task<ActionResult> EditSave(FunctAttendanceT F, String forwarddata, FormCollection form, String selected) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    int data = Convert.ToInt32(selected);
                    string PayProcessGroup = form["DDLPayProcessGroupEdit"] == "0" ? "" : form["DDLPayProcessGroupEdit"];
                    string PayMonthEdit = form["Effective_Date"] == "0" ? "" : form["Effective_Date"];
                    string ProcessMonth = form["Effective_Date"] == "0" ? "" : form["Effective_Date"];
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;

                    if (PayProcessGroup != null)
                    {
                        if (PayProcessGroup != "")
                        {
                            var val = db.PayProcessGroup.Find(int.Parse(PayProcessGroup));
                            F.PayProcessGroup = val;
                        }
                    }
                    if (PayMonthEdit != null)
                    {
                        if (PayMonthEdit != "")
                        {
                            var val = PayMonthEdit;
                            F.PayMonth = val;
                        }
                    }
                    if (ProcessMonth != null)
                    {
                        if (ProcessMonth != "")
                        {
                            var val = ProcessMonth;
                            F.ProcessMonth = val;
                        }
                    }

                    if (Auth == false)
                    {
                        if (ModelState.IsValid)
                        {

                            try
                            {

                                //DbContextTransaction transaction = db.Database.BeginTransaction();

                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    SalAttendanceT blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.SalAttendanceT.Where(e => e.Id == data)
                                                                .SingleOrDefault();
                                        originalBlogValues = context.Entry(blog).OriginalValues;
                                    }

                                    F.DBTrack = new DBTrack
                                    {
                                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };

                                    if (PayProcessGroup != null)
                                    {
                                        if (PayProcessGroup != "")
                                        {
                                            var val = db.PayProcessGroup.Find(int.Parse(PayProcessGroup));
                                            F.PayProcessGroup = val;

                                            var type = db.FunctAttendanceT.Include(e => e.PayProcessGroup).Where(e => e.Id == data).SingleOrDefault();
                                            IList<FunctAttendanceT> typedetails = null;
                                            if (type.PayProcessGroup != null)
                                            {
                                                typedetails = db.FunctAttendanceT.Where(x => x.PayProcessGroup.Id == type.PayProcessGroup.Id && x.Id == data).ToList();
                                            }
                                            else
                                            {
                                                typedetails = db.FunctAttendanceT.Where(x => x.Id == data).ToList();
                                            }
                                            //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                                            foreach (var s in typedetails)
                                            {
                                                s.PayProcessGroup = F.PayProcessGroup;
                                                db.FunctAttendanceT.Attach(s);
                                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                                //await db.SaveChangesAsync();
                                                db.SaveChanges();
                                                TempData["RowVersion"] = s.RowVersion;
                                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                            }
                                        }
                                        else
                                        {
                                            var ProcessTypeDetails = db.FunctAttendanceT.Include(e => e.PayProcessGroup).Where(x => x.Id == data).ToList();
                                            foreach (var s in ProcessTypeDetails)
                                            {
                                                s.PayProcessGroup = null;
                                                db.FunctAttendanceT.Attach(s);
                                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                                //await db.SaveChangesAsync();
                                                db.SaveChanges();
                                                TempData["RowVersion"] = s.RowVersion;
                                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var ProcessTypeDetails = db.FunctAttendanceT.Include(e => e.PayProcessGroup).Where(x => x.Id == data).ToList();
                                        foreach (var s in ProcessTypeDetails)
                                        {
                                            s.PayProcessGroup = null;
                                            db.FunctAttendanceT.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }

                                    var CurCorp = db.FunctAttendanceT.Find(data);
                                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {

                                        FunctAttendanceT Fattendance = new FunctAttendanceT()
                                        {
                                            HourDays = F.HourDays,
                                            Reason = F.Reason,
                                            PayProcessGroup = F.PayProcessGroup,
                                            Id = data,
                                            DBTrack = F.DBTrack
                                        };
                                        db.FunctAttendanceT.Attach(Fattendance);
                                        db.Entry(Fattendance).State = System.Data.Entity.EntityState.Modified;
                                        db.Entry(Fattendance).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                    }
                                    using (var context = new DataBaseContext())
                                    {
                                        var obj = DBTrackFile.DBTrackSave("Payroll/Payroll", originalBlogValues, db.ChangeTracker, F.DBTrack);
                                        db.SaveChanges();
                                    }
                                    await db.SaveChangesAsync();
                                    ts.Complete();
                                    // return Json(new { sucess = true, responseText = "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
                                    Msg.Add("  Data Saved successfully  ");
                                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (SalAttendanceT)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    // return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                    Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }
                                else
                                {
                                    var databaseValues = (SalAttendanceT)databaseEntry.ToObject();
                                    F.RowVersion = databaseValues.RowVersion;

                                }
                            }

                            //    return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                            Msg.Add("Record modified by another user.So refresh it and try to save again.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }
                    }
                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            FunctAttendanceT blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            FunctAttendanceT Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.FunctAttendanceT.Where(e => e.Id == data).SingleOrDefault();
                                originalBlogValues = context.Entry(blog).OriginalValues;
                            }
                            F.DBTrack = new DBTrack
                            {
                                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                Action = "M",
                                IsModified = blog.DBTrack.IsModified == true ? true : false,
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };

                            if (TempData["RowVersion"] == null)
                            {
                                TempData["RowVersion"] = blog.RowVersion;
                            }

                            FunctAttendanceT FattendanceT = new FunctAttendanceT()
                            {
                                PayProcessGroup = F.PayProcessGroup,
                                HourDays = F.HourDays,
                                Reason = F.Reason,
                                Id = data,
                                DBTrack = F.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };
                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Payroll/Payroll", "M", blog, FattendanceT, "FunctAttendanceT", F.DBTrack);
                                // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                Old_Corp = context.FunctAttendanceT.Where(e => e.Id == data)
                                    .SingleOrDefault();
                            }
                            blog.DBTrack = F.DBTrack;
                            db.FunctAttendanceT.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            return Json(new Object[] { blog.Id, F.PayProcessGroup, "Record Updated", JsonRequestBehavior.AllowGet });
                        }

                    }
                    return View();
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

        public ActionResult GetLookupEmpDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.EmpSalStructDetails.ToList();

                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.SalaryHead, ca.Amount }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);


            }

        }

        public ActionResult GetLookupEmp(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.Employee.ToList();

                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.EmpCode }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);

            }


        }


        public ActionResult Polulate_payscale_agreement(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var query = db.PayScaleAgreement.ToList();
                var selected = (Object)null;
                if (data2 != "" && data != "0" && data2 != "0")
                {
                    selected = Convert.ToInt32(data2);
                }

                SelectList s = new SelectList(query, "Id", "FullDetails", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }



        public ActionResult PopulateGradeDropDownList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var query = db.Grade.ToList();
                var selected = (Object)null;
                if (data2 != "" && data != "0" && data2 != "0")
                {
                    selected = Convert.ToInt32(data2);
                }

                SelectList s = new SelectList(query, "Id", "Name", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }

        public class EditData
        {
            public int Id { get; set; }
            public string ProcessMonth { get; set; }
            public string Reason { get; set; }
            public bool Editable { get; set; }
            public Double HourDays { get; set; }
        }

        public class DeserializeClass
        {
            public String Id { get; set; }
            public String Amount { get; set; }

        }

        public class P2BGridData
        {
            public int Id { get; set; }
            public Employee Employee { get; set; }
            public string EffectiveDate { get; set; }
            public DateTime? EndDate { get; set; }
            public int PayScaleAgreement_Id { get; set; }
        }

        public JsonResult GetPayscaleagreement(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var a = db.PayScaleAgreement.Find(int.Parse(data));
                return Json(a, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult process(string forwarddata, FormCollection form, String selected)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var serialize = new JavaScriptSerializer();
                    var obj = serialize.Deserialize<List<DeserializeClass>>(forwarddata);

                    if (obj.Count < 0)
                    {
                        return Json(new { sucess = false, responseText = "You have to change amount to update salary structure." }, JsonRequestBehavior.AllowGet);
                    }
                    List<int> b = obj.Select(e => int.Parse(e.Id)).ToList();

                    string PayScaleAgr = form["payscaleagreement_id"] == "0" ? "" : form["payscaleagreement_id"];
                    string Effective_date = form["Effective_Date"] == "0" ? "" : form["Effective_Date"];

                    using (TransactionScope ts = new TransactionScope())
                    {
                        foreach (int ca in b)
                        {
                            EmpSalStructDetails EmpSalStructDet = db.EmpSalStructDetails.Find(ca);
                            EmpSalStructDet.Amount = Convert.ToDouble(obj.Where(e => e.Id == ca.ToString()).Select(e => e.Amount).Single());
                            db.EmpSalStructDetails.Attach(EmpSalStructDet);
                            db.Entry(EmpSalStructDet).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            db.Entry(EmpSalStructDet).State = System.Data.Entity.EntityState.Detached;
                        }
                        int EmpId = int.Parse(selected);

                        Employee OEmployee = db.Employee
                             .Include(e => e.GeoStruct)
                             .Include(e => e.FuncStruct)
                             .Include(e => e.PayStruct)
                              .Where(e => e.Id == EmpId)
                              .SingleOrDefault();

                        var OEmployeePayroll = db.EmployeePayroll.Include(e => e.EmpSalStruct)
                            .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.RoundingMethod)))
                            .Where(e => e.Employee.Id == EmpId).SingleOrDefault();

                        if (PayScaleAgr != null && PayScaleAgr != "")
                        {
                            int PayScaleAgrId = int.Parse(PayScaleAgr);
                            var OPayScalAgreement = db.PayScaleAgreement.Where(e => e.Id == PayScaleAgrId).SingleOrDefault();
                        }



                        try
                        {
                            // SalaryHeadGenProcess.EmployeeSalaryStructUpdate(OEmployeePayroll, Convert.ToDateTime(Effective_date));
                            ts.Complete();
                            //    return Json(new { sucess = true, responseText = "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
                            Msg.Add("  Data Saved successfully  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);


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
                            return Json(new { sucess = false, responseText = ex.InnerException }, JsonRequestBehavior.AllowGet);
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


        public ActionResult P2BInlineGrid(P2BGrid_Parameters gp)
        {
            try
            {
                DataBaseContext db = new DataBaseContext();
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                var jsonData = (Object)null;
                IEnumerable<EditData> FAttendanceT = null;

                List<EditData> model = new List<EditData>();

                var OEmployeePayroll = db.EmployeePayroll
                    .SelectMany(o => o.FunctAttendanceT).ToList();

                var view = new EditData();

                foreach (var z in OEmployeePayroll)
                {
                    var m = db.FunctAttendanceT.Where(e => e.Id == z.Id).SingleOrDefault();
                    bool EditAppl = true;
                    view = new EditData()
                    {
                        Id = z.Id,
                        ProcessMonth = z.ProcessMonth,
                        HourDays = z.HourDays,
                        Editable = EditAppl
                    };

                    model.Add(view);
                }

                FAttendanceT = model;

                IEnumerable<EditData> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = FAttendanceT;
                    if (gp.searchOper.Equals("eq"))
                    {

                        jsonData = IE.Select(a => new Object[] { a.Id, a.ProcessMonth, a.HourDays, a.Reason, a.Editable }).Where((e => (e.Contains(gp.searchString)))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.ProcessMonth, a.HourDays, a.Reason, a.Editable }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = FAttendanceT;
                    Func<EditData, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "ProcessMonth" ? c.ProcessMonth.ToString() :
                                         gp.sidx == "HourDays" ? c.HourDays.ToString() :
                                         gp.sidx == "Reason" ? c.Reason.ToString() :
                                         gp.sidx == "Editable" ? c.Editable.ToString() : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.ProcessMonth, a.HourDays, a.Reason, a.Editable }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.ProcessMonth, a.HourDays, a.Reason, a.Editable }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.ProcessMonth, a.HourDays, a.Reason, a.Editable }).ToList();
                    }
                    totalRecords = FAttendanceT.Count();
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

                IEnumerable<P2BGridData> FunctionalAttendanceT = null;
                List<P2BGridData> model = new List<P2BGridData>();
                P2BGridData view = null;

                var OEmployee = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.Employee.EmpName).ToList();

                foreach (var z in OEmployee)
                {
                    var OFunctionalAttendanceT = db.EmployeePayroll.Where(e => e.Id == z.Id).Select(e => e.FunctAttendanceT)
                                        .SingleOrDefault();


                    DateTime? Eff_Date = null;
                    //PayScaleAgreement PayScaleAgr = null;
                    foreach (var a in OFunctionalAttendanceT)
                    {
                        Eff_Date = Convert.ToDateTime(a.ProcessMonth);
                        var aa = db.FunctAttendanceT.Where(e => e.Id == a.Id).SingleOrDefault();
                    }
                    view = new P2BGridData()
                    {
                        Id = z.Employee.Id,
                        Employee = z.Employee,
                        EffectiveDate = Eff_Date.Value.ToString("dd/MM/yyyy"),
                        EndDate = null,
                        //PayScaleAgreement_Id = PayScaleAgr.Id
                    };

                    model.Add(view);
                }

                FunctionalAttendanceT = model;

                IEnumerable<P2BGridData> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = FunctionalAttendanceT;
                    if (gp.searchOper.Equals("eq"))
                    {
                        if (gp.searchField == "Id")
                        {
                            jsonData = IE.Where(e => e.Id.ToString().Contains(gp.searchString)
                                ).Select(a => new Object[] { a.Id, a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.EffectiveDate, a.EndDate}).ToList();
                        }
                        else if (gp.searchField == "EmpCode")
                        {
                            jsonData = IE.Where(e => e.Employee.EmpCode.ToUpper().Contains(gp.searchString.ToUpper())
                                ).Select(a => new Object[] { a.Id, a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.EffectiveDate, a.EndDate}).ToList();
                        }
                        else if (gp.searchField == "EmpName")
                        {
                            jsonData = IE.Where(e => e.Employee.EmpName.FullNameFML.ToUpper().Contains(gp.searchString.ToUpper())
                                ).Select(a => new Object[] { a.Id, a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.EffectiveDate, a.EndDate}).ToList();
                        }
                        else if (gp.searchField == "EffectiveDate")
                        {
                            jsonData = IE.Where(e => e.EffectiveDate.ToString().Contains(gp.searchString)
                                ).Select(a => new Object[] { a.Id, a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.EffectiveDate, a.EndDate}).ToList();
                        }
                        else if (gp.searchField == "EndDate")
                        {
                            jsonData = IE.Where(e => e.EndDate.ToString().Contains(gp.searchString)
                                ).Select(a => new Object[] { a.Id, a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.EffectiveDate, a.EndDate}).ToList();
                        }
                        //jsonData = IE.Select(a => new { a.Id, a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.EffectiveDate, a.EndDate }).Where(e => (e.Id.ToString().Contains(gp.searchString)) 
                        //    ||  (e.EmpCode.ToString().Contains(gp.searchString))
                        //    || (e.FullNameFML.ToString().Contains(gp.searchString))
                        //    ||  (e.EffectiveDate.ToString().Contains(gp.searchString))
                        //    ||  (e.EndDate.ToString().Contains(gp.searchString))
                        //    ).ToList();
                       

                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.EffectiveDate, a.EndDate != null ? Convert.ToString(a.EndDate) : "" }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = FunctionalAttendanceT;
                    Func<P2BGridData, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "EmpCode" ? c.Employee.EmpCode.ToString() :
                                         gp.sidx == "EmpName" ? c.Employee.EmpName.FullNameFML.ToString() :
                                         gp.sidx == "EffectiveDate" ? c.EffectiveDate.ToString() :
                                         gp.sidx == "EndDate" ? c.EndDate.ToString() : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, Convert.ToString(a.EffectiveDate), a.EndDate != null ? Convert.ToString(a.EndDate) : "" }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, Convert.ToString(a.EffectiveDate), a.EndDate != null ? Convert.ToString(a.EndDate) : "" }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.EffectiveDate, a.EndDate != null ? Convert.ToString(a.EndDate) : "" }).ToList();
                    }
                    totalRecords = FunctionalAttendanceT.Count();
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



        public ActionResult PopulatePayprocesssgrouplist(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var query = db.PayProcessGroup.ToList();

                var selected = (Object)null;
                if (data2 != "" && data != "0" && data2 != "0")
                {
                    selected = Convert.ToInt32(data2);
                }

                SelectList s = new SelectList(query, "Id", "Name", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult P2BGridDisplay(P2BGrid_Parameters gp)
        {
            try
            {
                DataBaseContext db = new DataBaseContext();
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                var jsonData = (Object)null;
                IEnumerable<SalaryHead> SalaryHead = null;
                if (gp.IsAutho == true)
                {
                    SalaryHead = db.SalaryHead.Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    SalaryHead = db.SalaryHead.AsNoTracking().ToList();
                }

                IEnumerable<SalaryHead> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = SalaryHead;
                    if (gp.searchOper.Equals("eq"))
                    {    
                            jsonData = IE.Select(a => new { a.Id, a.Code }).Where(e => (e.Id.ToString().Contains(gp.searchString)) ||(e.Code.ToString().Contains(gp.searchString)) ).ToList();
                     

                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Code }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = SalaryHead;
                    Func<SalaryHead, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "Code" ? c.Code.ToString() : "");


                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code) }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code) }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Code }).ToList();
                    }
                    totalRecords = SalaryHead.Count();
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
                            var Emp = db.EmployeePayroll.Where(e => e.Employee.Id == data).Select(e => e.FunctAttendanceT).SingleOrDefault();
                            FunctAttendanceT FunctAttendanceT = Emp.Where(e => e.Reason == null).SingleOrDefault();
                            db.Entry(FunctAttendanceT).State = System.Data.Entity.EntityState.Deleted;
                            await db.SaveChangesAsync();
                            ts.Complete();
                            //  return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }
                        catch (RetryLimitExceededException /* dex */)
                        {
                            // return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
                            Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
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
        }

        public ActionResult LoadEmp(P2BGrid_Parameters gp)
        {
            try
            {
                DataBaseContext db = new DataBaseContext();
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                var jsonData = (Object)null;
                IEnumerable<Employee> Employee = null;
                if (gp.IsAutho == true)
                {
                    Employee = db.Employee.Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {

                    var uids = db.EmployeePayroll.Include(e => e.FunctAttendanceT).Select(e => e.Employee.Id).ToList();
                    Employee = db.Employee.Include(e => e.EmpName);
                }

                IEnumerable<Employee> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = Employee;
                    if (gp.searchOper.Equals("eq"))
                    {
                        if (gp.searchField == "Id")
                            jsonData = IE.Select(a => new { a.Id, a.CardCode }).Where((e => (e.Id.ToString().Contains(gp.searchString)))).ToList();
                        else if (gp.searchField == "EmpCode")
                            jsonData = IE.Select(a => new { a.Id, a.EmpCode }).Where((e => (e.EmpCode.ToString().Contains(gp.searchString)))).ToList();
                        else if (gp.searchField == "EmpName")
                            jsonData = IE.Select(a => new { a.Id, a.EmpName.FullNameFML }).Where((e => (e.FullNameFML.ToString().Contains(gp.searchString)))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.CardCode }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = Employee;
                    Func<Employee, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id.ToString() :
                                         gp.sidx == "EmpCode" ? c.EmpCode.ToString() :
                                         gp.sidx == "EmpName" ? c.EmpName.FullNameFML.ToString() : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.EmpCode), a.EmpName.FullNameFML }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.EmpCode), a.EmpName.FullNameFML }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, Convert.ToString(a.EmpCode), a.EmpName.FullNameFML }).ToList();
                    }
                    totalRecords = Employee.Count();
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
                throw (ex);
            }
        }


    }
}