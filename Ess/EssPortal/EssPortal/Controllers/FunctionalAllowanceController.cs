using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web.Mvc;
using System.Data;
using System.Data.Entity;
using EssPortal.Models;
using EssPortal.Security;
using Payroll;
using EssPortal.App_Start;
using P2b.Global;
namespace EssPortal.Controllers
{
    [AuthoriseManger]
    public class FunctionalAllowanceController : Controller
    {
        //
        // GET: /FunctionalAllowance/
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Create(FunctAttendanceT S, FormCollection form, String forwarddata) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string Emp = forwarddata == "0" ? "" : forwarddata;
                string PayProcessgropp = form["payscaleagreement_drop"] == "0" ? "" : form["payscaleagreement_drop"];
                string ProcessMonth = form["Create_Processmonth"] == "0 " ? "" : form["Create_Processmonth"];
                string PayMonth = form["Create_Paymonth"] == "0" ? "" : form["Create_Paymonth"];
                string HourDays = form["Create_HourDays"] == "0" ? "" : form["Create_HourDays"];
                string Reason = form["Create_Reason"] == "0" ? "" : form["Create_Reason"];
                string SalaryHead = form["SalaryHead_drop"] == "0" ? "" : form["SalaryHead_drop"];
                string Empstruct_drop = form["Empstruct_drop"] == "0" ? "" : form["Empstruct_drop"];
                string fromdate = form["fromdate"] == "0 " ? "" : form["fromdate"];
                string Todate = form["Todate"] == "0 " ? "" : form["Todate"];

                var EmpVariable = db.Employee.Include(e => e.GeoStruct).Include(e => e.GeoStruct.Location)
                                .Where(r => r.Id == 1).SingleOrDefault();

                if (Empstruct_drop != null && Empstruct_drop != "" && Empstruct_drop != "-Select-")
                {
                    var value = db.EmpSalStruct.Find(int.Parse(Empstruct_drop));
                    S.EmpSalStruct = value;

                }
                if (SalaryHead != null && SalaryHead != "")
                {
                    var val = db.SalaryHead.Find(int.Parse(SalaryHead));
                    S.SalaryHead = val;
                }
                List<int> ids = null;
                if (Emp != null && Emp != "0" && Emp != "false")
                {
                    ids = Utility.StringIdsToListIds(Emp);
                }
                Employee OEmployee = null;
                EmployeePayroll OEmployeePayroll = null;
                string EmpCode = null;
                string EmpRel = null;

                if (ids != null)
                {
                    foreach (var i in ids)
                    {
                        OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct)
                                   .Where(r => r.Id == i).SingleOrDefault();

                        OEmployeePayroll = db.EmployeePayroll.Where(e => e.Employee.Id == i).SingleOrDefault();

                        var OEmpSalT = db.EmployeePayroll.Where(e => e.Id == OEmployeePayroll.Id)
                            .Include(e => e.FunctAttendanceT)
                            .Include(e => e.FunctAttendanceT.Select(a => a.SalaryHead))
                            .SingleOrDefault();
                        var EmpSalT = OEmpSalT.FunctAttendanceT != null ? OEmpSalT.FunctAttendanceT.Where(e => e.PayMonth == PayMonth && e.SalaryHead.Id == S.SalaryHead.Id && e.isCancel == false) : null;
                        if (EmpSalT != null && EmpSalT.Count() > 0)
                        {
                            if (EmpCode == null || EmpCode == "")
                            {
                                EmpCode = OEmployee.EmpCode;
                            }
                            else
                            {
                                EmpCode = EmpCode + ", " + OEmployee.EmpCode;
                            }
                        }

                        var OEmpSalRelT = db.EmployeePayroll.Where(e => e.Id == OEmployeePayroll.Id).Include(e => e.FunctAttendanceT).SingleOrDefault();
                        var EmpSalRelT = OEmpSalRelT.SalaryT != null ? OEmpSalRelT.SalaryT.Where(e => e.PayMonth == PayMonth && e.ReleaseDate != null) : null;

                        if (EmpSalRelT != null && EmpSalRelT.Count() > 0)
                        {
                            if (EmpRel == null || EmpRel == "")
                            {
                                EmpRel = OEmployee.EmpCode;
                            }
                            else
                            {
                                EmpRel = EmpRel + ", " + OEmployee.EmpCode;
                            }
                        }
                    }
                }
                if (EmpCode != null)
                    return Json(new { success = true, responseText = "FunctAttendance already exists for employee " + EmpCode + "." }, JsonRequestBehavior.AllowGet);
                // return Json(new Object[] { "", "", "Attendance already exists for employee " + EmpCode + "." }, JsonRequestBehavior.AllowGet);

                if (EmpRel != null)
                    return Json(new { success = true, responseText = "Salary released for employee " + EmpRel + ". You can't change attendance now." }, JsonRequestBehavior.AllowGet);
                //return Json(new Object[] { "", "", "Salary released for employee " + EmpRel + ". You can't change attendance now." }, JsonRequestBehavior.AllowGet);
                if (PayMonth != null && PayMonth != "")
                {
                    S.PayMonth = PayMonth;
                    int mon = int.Parse(PayMonth.Split('/')[0].StartsWith("0") == true ? PayMonth.Split('/')[0].Remove(0, 1) : PayMonth.Split('/')[0]);
                    int DaysInMonth = System.DateTime.DaysInMonth(int.Parse(PayMonth.Split('/')[1]), mon);
                    //S.MonthDays = DaysInMonth;
                }

                if (ProcessMonth != null && ProcessMonth != "")
                {
                    S.ProcessMonth = ProcessMonth;
                }
                if (fromdate != null && fromdate != "")
                {
                    var value = Convert.ToDateTime(fromdate);
                    S.FromDate = value;
                }
                if (Todate != null && Todate != "")
                {
                    var value = Convert.ToDateTime(Todate);
                    S.ToDate = value;
                }
                if (HourDays != null && HourDays != "")
                {
                    var val = int.Parse(HourDays);
                    S.HourDays = val;
                }
                if (Reason != null && Reason != "")
                {
                    var val = Reason;
                    S.Reason = val;
                }


                S.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                FunctAttendanceT ObjFAT = new FunctAttendanceT();
                {
                    ObjFAT.PayMonth = S.PayMonth;
                    ObjFAT.HourDays = S.HourDays;
                    ObjFAT.ProcessMonth = S.ProcessMonth;
                    ObjFAT.SalaryHead = S.SalaryHead;
                    ObjFAT.Reason = S.Reason;

                    ObjFAT.FromDate = S.FromDate;
                    ObjFAT.ToDate = S.ToDate;
                    //OEmpSalStruct.PayStruct = db.PayStruct.Find( OEmployeePayroll.PayStruct.Id);
                    ObjFAT.DBTrack = S.DBTrack;

                }
                if (ids != null)
                {
                    foreach (var i in ids)
                    {
                        int Id = int.Parse(Empstruct_drop);

                        OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct)
                                    .Where(r => r.Id == i).SingleOrDefault();

                        OEmployeePayroll
                        = db.EmployeePayroll.Include(e => e.EmpSalStruct)
                      .Where(e => e.Employee.Id == i).SingleOrDefault();

                        ObjFAT.GeoStruct = db.GeoStruct.Find(OEmployee.GeoStruct.Id);

                        //OEmpSalStruct.GeoStruct = db.GeoStruct.Find( OEmployeePayroll.GeoStruct.Id);

                        ObjFAT.FuncStruct = db.FuncStruct.Find(OEmployee.FuncStruct.Id);


                        ObjFAT.PayStruct = db.PayStruct.Find(OEmployee.PayStruct.Id == null ? 0 : OEmployee.PayStruct.Id);

                        var k = db.EmpSalStruct.Where(e => e.Id == Id).Select(e => new
                        {
                            EffectiveDate = e.EffectiveDate,
                            EndDate = e.EndDate,
                            EffectiveDate_EndDate = e.EffectiveDate != null ? e.EffectiveDate.ToString() : "" + e.EndDate != null ? e.EndDate.ToString() : ""
                        }).SingleOrDefault();
                        var Q = db.EmployeePayroll.Where(e => e.Employee.Id == i).Select(e => e.EmpSalStruct.Where(r => r.EffectiveDate == k.EffectiveDate && r.EndDate == k.EndDate)).SingleOrDefault();
                        var empsal_Id = Q.Select(r => r.Id).SingleOrDefault();
                        ObjFAT.EmpSalStruct = db.EmpSalStruct.Find(empsal_Id);

                        //var Z =
                        //Q.Where(e => e.Id == Id).Select(e => new
                        //{
                        //    Id = e.Id,
                        //    EffectiveDate = e.EffectiveDate,
                        //    EndDate = e.EndDate,
                        //    EffectiveDate_EndDate = e.EffectiveDate +" "+ e.EndDate

                        //}).ToList();
                        //var k = db.EmpSalStruct.Where(e => e.Id == Id).Select(e => new
                        //{
                        //    EffectiveDate = e.EffectiveDate,
                        //    EndDate = e.EndDate,
                        //    EffectiveDate_EndDate = e.EffectiveDate != null ? e.EffectiveDate.ToString():""  + e.EndDate != null ? e.EndDate.ToString():""
                        //}).SingleOrDefault();
                        //foreach (var ca in Z) 
                        //{
                        //    if (ca.EffectiveDate_EndDate == k.EffectiveDate_EndDate) 
                        //    {
                        //        ObjFAT.EmpSalStruct = S.EmpSalStruct;

                        //    }
                        //}

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
                                        DBTrack = S.DBTrack

                                    };
                                    db.EmployeePayroll.Add(OTEP);
                                    db.SaveChanges();
                                }
                                else
                                {
                                    var aa = db.EmployeePayroll.Find(OEmployeePayroll.Id);
                                    OFAT.AddRange(aa.FunctAttendanceT);
                                    aa.FunctAttendanceT = OFAT;
                                    //OEmployeePayroll.DBTrack = dbt;

                                    db.EmployeePayroll.Attach(aa);
                                    db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    db.Entry(aa).State = System.Data.Entity.EntityState.Detached;

                                }
                                ts.Complete();

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
                                //return Json(new { sucess = false, responseText = ex.InnerException }, JsonRequestBehavior.AllowGet);
                            }


                        }
                    }
                    return this.Json(new { success = true, responseText = "Data Saved Successfully...", JsonRequestBehavior.AllowGet });
                }

                return this.Json(new { success = false, responseText = "Unable to create...", JsonRequestBehavior.AllowGet });
            }
        }
    }
}