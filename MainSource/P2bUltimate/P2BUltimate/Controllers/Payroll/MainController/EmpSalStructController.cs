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
using P2BUltimate.Process;
using Leave;
using Attendance;
//using P2BUltimate.Process;

namespace P2BUltimate.Controllers.Core.MainController
{
    [AuthoriseManger]
    public class EmpSalStructController : Controller
    {
        //  private DataBaseContext db = new DataBaseContext();

        public ActionResult Index()
        {
            // db.RefreshAllEntites(RefreshMode.StoreWins);
            return View("~/Views/Payroll/MainViews/EmpSalStruct/Index.cshtml");
        }

        public ActionResult EmpSalStructPartial()
        {
            return View("~/Views/Shared/Payroll/_EmpSalStructDetails.cshtml");
        }
        public ActionResult Partial()
        {
            return View("~/Views/Shared/Payroll/_EmpSalStruct.cshtml");
        }

        public List<int> one_ids(string form)
        {
            string itsec_id = form;
            List<int> ids = itsec_id.Split(',').Select(e => int.Parse(e)).ToList();
            return (ids);
        }


        public ActionResult Create(EmpSalStruct EmpSalStruct, FormCollection form, String forwarddata) //Create submit
        {
            List<string> Msg = new List<string>();
            try
            {
                string Emp = forwarddata == "0" ? "" : forwarddata;
                string PayScaleAgr = form["payscaleagreement_drop"] == "0" ? "" : form["payscaleagreement_drop"];
                string Effective_date = form["Effective_Date"] == "0" ? "" : form["Effective_Date"];
                using (DataBaseContext db = new DataBaseContext())
                {


                    List<int> ids = null;
                    if (Emp != null && Emp != "0" && Emp != "false")
                    {
                        ids = one_ids(Emp);
                    }
                    else
                    {
                        return Json(new { success = false, responseText = "Kindly select employee" }, JsonRequestBehavior.AllowGet);
                    }

                    Employee OEmployee = null;
                    EmployeePayroll OEmployeePayroll = null;
                    PayScaleAgreement OPayScalAgreement = null;

                    if (PayScaleAgr != null && PayScaleAgr != "")
                    {
                        int PayScaleAgrId = int.Parse(PayScaleAgr);
                        OPayScalAgreement = db.PayScaleAgreement.Where(e => e.Id == PayScaleAgrId).SingleOrDefault();
                        if (OPayScalAgreement != null)
                        {
                            var PayScaleAssign = db.PayScaleAssignment.Where(e => e.PayScaleAgreement.Id == PayScaleAgrId).ToList();
                            if (PayScaleAssign.Count == 0)
                            {
                                return Json(new { success = false, responseText = "PayScalessignment not defined." }, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
                    else
                    {
                        return Json(new { success = false, responseText = "Kindly select PayScaleAgreement." }, JsonRequestBehavior.AllowGet);
                    }
                    int Comp_Id = int.Parse(Session["CompId"].ToString());
                    var ComPanyLeave_Id = db.CompanyLeave.Where(e => e.Company.Id == Comp_Id).SingleOrDefault();
                    foreach (var i in ids)
                    {
                        OEmployeePayroll = db.EmployeePayroll.Where(e => e.Employee.Id == i).SingleOrDefault();

                        using (TransactionScope ts = new TransactionScope())
                        {
                            try
                            {
                                EmployeeLeave OEmployeeLeave = db.EmployeeLeave.Where(e => e.Employee.Id == i).SingleOrDefault();
                                EmployeeAttendance OEmployeeAttendance = db.EmployeeAttendance.Where(e => e.Employee.Id == i).SingleOrDefault();

                                SalaryHeadGenProcess.EmployeeSalaryStructCreationTest(OEmployeePayroll, i, OPayScalAgreement.Id, Convert.ToDateTime(Effective_date), Comp_Id);
                                ServiceBook.EmployeeLTCStructCreationTest(OEmployeePayroll, i, OPayScalAgreement.Id, Convert.ToDateTime(Effective_date), Comp_Id);
                                ServiceBook.EmployeePolicyStructCreationTest(OEmployeePayroll, i, OPayScalAgreement.Id, Convert.ToDateTime(Effective_date), Comp_Id);
                                LeaveStructureProcess.EmployeeLeaveStructCreationTest(OEmployeeLeave, i, OPayScalAgreement.Id, Convert.ToDateTime(Effective_date), ComPanyLeave_Id.Id);
                                //////new added 12/08/2019
                                // attendance structure create start
                                ServiceBook.EmployeeAttStructCreation(OEmployeeAttendance, i, OPayScalAgreement.Id, Convert.ToDateTime(Effective_date));
                                // attendance structure create end
                                //employeeAttendance Action Policy Start
                                ServiceBook.EmpolyeeAttendacePolicyStructCreationTest(OEmployeeAttendance, i, OPayScalAgreement.Id, Convert.ToDateTime(Effective_date), ComPanyLeave_Id.Id);
                                //employeeAttendance Action Policy Start

                                int empid = 0;
                                // empid = int.Parse(Emp);
                                var Emp_id = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.Employee.PayStruct).Where(e => e.Employee.Id == i).SingleOrDefault();
                                string date = Convert.ToDateTime(Effective_date).ToString("MM/yyyy");



                                var pay = db.Employee.Include(e => e.PayStruct).Where(e => e.Id == i).SingleOrDefault();

                                var Emp_Check = db.EmpSalStruct.Include(e => e.EmployeePayroll).Include(e => e.PayStruct).Where(e => e.EmployeePayroll.Id == Emp_id.Id && e.PayStruct != null).SingleOrDefault();

                                var CPI_Check = db.CPIEntryT.Where(t => t.PayMonth == date).FirstOrDefault();

                                if (Emp_Check != null && CPI_Check != null)
                                {

                                    try
                                    {
                                        CPIEntryT c = db.CPIEntryT.Include(e => e.EmployeePayroll).Where(e => e.PayMonth == date && e.EmployeePayroll.Employee.PayStruct.Id == pay.PayStruct.Id)
                                            .ToList().FirstOrDefault();
                                        int empCpi = 0;
                                        if (c != null)
                                        {
                                            empCpi = c.EmployeePayroll.Id;
                                        }
                                        else
                                        {
                                            empCpi = Emp_id.Id;
                                        }
                                        EmployeePayroll EmployeePayroll = db.EmployeePayroll.Where(e => e.Id == empCpi).SingleOrDefault();
                                        if (c != null)
                                        {
                                            c.EmployeePayroll = EmployeePayroll;
                                        }
                                        Employee Employee = db.Employee.Where(e => e.Id == EmployeePayroll.Employee_Id).SingleOrDefault();
                                        EmployeePayroll.Employee = Employee;

                                        PayStruct PayStruct = db.PayStruct.Where(e => e.Id == Employee.PayStruct_Id).SingleOrDefault();
                                        Employee.PayStruct = PayStruct;
                                        //.Include(e => e.EmployeePayroll)
                                        //.Include(e => e.EmployeePayroll.Employee)
                                        //.Include(e => e.EmployeePayroll.Employee.PayStruct)

                                        int id = 0;
                                        if (PayScaleAgr != null && PayScaleAgr != "")
                                        {
                                            id = int.Parse(PayScaleAgr);
                                            var val = db.PayScaleAgreement.Include(e => e.PayScale).Where(e => e.Id == id).SingleOrDefault();
                                            if (c != null)
                                            {
                                                c.PayScale = val.PayScale;
                                            }
                                            else
                                            {
                                                c = new CPIEntryT();
                                                c.PayScale = val.PayScale;
                                                c.ActualIndexPoint = 0;
                                                c.CalIndexPoint = 0;
                                                c.PayMonth = date;
                                                c.EmployeePayroll = EmployeePayroll;
                                            }
                                        }
                                        if (ModelState.IsValid)
                                        {
                                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                            //int empid = 0;
                                            //empid = int.Parse(Emp);
                                            //var Emp_id = db.EmployeePayroll.Where(e => e.Id == empid).SingleOrDefault();

                                            CPIEntryT corporate = new CPIEntryT()
                                            {
                                                ActualIndexPoint = c.ActualIndexPoint,
                                                CalIndexPoint = c.CalIndexPoint,
                                                PayMonth = c.PayMonth,
                                                PayScale = c.PayScale,
                                                EmployeePayroll = Emp_id,
                                                DBTrack = c.DBTrack
                                            };

                                            db.CPIEntryT.Add(corporate);
                                            db.SaveChanges();

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
                                            Msg.Add("Code Already Exists.");
                                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        Msg.Add(e.Message);
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    }
                                }

                                ////////new

                                //db.RefreshAllEntites(RefreshMode.StoreWins);
                                ts.Complete();
                                // return Json(new { success = true, responseText = "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
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

                            //SalaryHeadGenProcess.EmployeeSalaryStructUpdate(OEmployeePayroll, DateTime.Now);
                        }

                    }
                    return Json(new { success = true, responseText = "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);

                    //return View();
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


        //public ActionResult Update_Struct(String forwarddata, string Effective_Date, int PayScaleAgreementId) //Create submit
        //{
        //    int EmpId = int.Parse(forwarddata);

        //    Employee OEmployee = db.Employee
        //         .Include(e => e.GeoStruct)
        //         .Include(e => e.FuncStruct)
        //         .Include(e => e.PayStruct)
        //          .Where(e => e.Id == EmpId)// && r.GeoStruct.Company.Id==1)
        //          .SingleOrDefault();

        //    var OEmployeePayroll
        //        = db.EmployeePayroll.Include(e => e.EmpSalStruct).Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails))
        //        .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead)))
        //    .Where(e => e.Employee.Id == EmpId).SingleOrDefault();

        //    int OEmpSalStructId = OEmployeePayroll.EmpSalStruct.Where(e => e.EffectiveDate == Convert.ToDateTime(Effective_Date)).Select(e => e.Id).SingleOrDefault();

        //    var OPayScalAgreement = db.PayScaleAgreement.Where(e => e.Id == PayScaleAgreementId).SingleOrDefault();


        //    using (TransactionScope ts = new TransactionScope())
        //    {
        //        try
        //        {
        //            OEmployeePayroll = null;
        //            SalaryHeadGenProcess.EmployeeSalaryStructCreationWithUpdation(OEmpSalStructId, OEmployee, OPayScalAgreement, Convert.ToDateTime(Effective_Date));
        //            ts.Complete();
        //            return Json(new { success = true, responseText = "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
        //        }
        //        catch (DataException ex)
        //        {
        //            LogFile Logfile = new LogFile();
        //            ErrorLog Err = new ErrorLog()
        //            {
        //                ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
        //                ExceptionMessage = ex.Message,
        //                ExceptionStackTrace = ex.StackTrace,
        //                LineNo = Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
        //                LogTime = DateTime.Now
        //            };
        //            Logfile.CreateLogFile(Err);
        //            return Json(new { sucess = false, responseText = ex.InnerException }, JsonRequestBehavior.AllowGet);
        //        }

        //        //SalaryHeadGenProcess.EmployeeSalaryStructUpdate(OEmployeePayroll, DateTime.Now);
        //    }



        //}

        public ActionResult Update_Struct(int PayScaleAgreementId) //Create submit
        {
            List<string> Msg = new List<string>();
            try
            {
                using (DataBaseContext db = new DataBaseContext())
                {
                    //var OPayScalAgreement = db.PayScaleAgreement.Where(e => e.Id == PayScaleAgreementId).AsNoTracking().SingleOrDefault();

                    var oemployee = db.EmployeePayroll.ToList();
                    foreach (var oemployeeitem in oemployee)
                    {
                        Employee Employee = db.Employee.Where(e => e.Id == oemployeeitem.Employee_Id).SingleOrDefault();
                        oemployeeitem.Employee = Employee;
                        EmpOff EmpOffInfo = db.EmpOff.Where(e => e.Id == Employee.EmpOffInfo_Id).SingleOrDefault();
                        Employee.EmpOffInfo = EmpOffInfo;
                        ServiceBookDates ServiceBookDates = db.ServiceBookDates.Where(e => e.Id == Employee.ServiceBookDates_Id).SingleOrDefault();
                        Employee.ServiceBookDates = ServiceBookDates;
                        NameSingle EmpName = db.NameSingle.Where(e => e.Id == Employee.EmpName_Id).SingleOrDefault();
                        Employee.EmpName = EmpName;

                    }
                    //.Include(e => e.Employee.EmpOffInfo)
                    //.Include(e => e.Employee.EmpName)
                    //.Include(e => e.Employee.ServiceBookDates)

                    var EmpList = oemployee.Where(e => e.Employee.ServiceBookDates.ServiceLastDate == null)
                               .ToList().OrderBy(d => d.Id);

                    //var EmpList = db.EmployeePayroll.AsNoTracking().OrderBy(e => e.Id)
                    //            .ToList();


                    foreach (var a in EmpList)
                    {
                        P2B.UTILS.P2BLogger logger = new P2B.UTILS.P2BLogger();
                        string EmpPayrollID = a.Id.ToString();
                        logger.Logging("EmpPayrollID::::  " + EmpPayrollID);
                        //Utility.DumpProcessStatus("Emp started structure update " + a.Id);

                        //var EmpSalStruct = db.EmployeePayroll.Include(e => e.EmpSalStruct)
                        //                .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails))
                        //                 .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead)))
                        //                .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.PayScaleAssignment)))
                        //                .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.PayScaleAssignment.PayScaleAgreement)))
                        //                .Where(e => e.Id == a.Id).AsNoTracking().OrderBy(e => e.Id).FirstOrDefault();
                        var SalStruct = db.EmpSalStruct.Where(e => e.EmployeePayroll.Id == a.Id && e.EndDate == null).OrderBy(e => e.Id).FirstOrDefault();
                        List<EmpSalStructDetails> EmpSalStructDetails = db.EmpSalStructDetails.Where(e => e.EmpSalStruct_Id == SalStruct.Id).ToList();

                        foreach (var EmpSalStructDetailsitem in EmpSalStructDetails)
                        {
                            SalaryHead SalaryHead = db.SalaryHead.Include(x => x.RoundingMethod).Where(e => e.Id == EmpSalStructDetailsitem.SalaryHead_Id).SingleOrDefault();
                            EmpSalStructDetailsitem.SalaryHead = SalaryHead;
                            PayScaleAssignment PayScaleAssignment = db.PayScaleAssignment.Where(e => e.Id == EmpSalStructDetailsitem.PayScaleAssignment_Id).SingleOrDefault();
                            EmpSalStructDetailsitem.PayScaleAssignment = PayScaleAssignment;
                            PayScaleAgreement PayScaleAgreement = db.PayScaleAgreement.Where(e => e.Id == PayScaleAssignment.PayScaleAgreement_Id).SingleOrDefault();
                            EmpSalStructDetailsitem.PayScaleAssignment.PayScaleAgreement = PayScaleAgreement;


                        }
                        SalStruct.EmpSalStructDetails = EmpSalStructDetails;
                        //.Include(e => e.EmpSalStructDetails)
                        // .Include(e => e.EmpSalStructDetails.Select(t => t.SalaryHead))
                        //.Include(e => e.EmpSalStructDetails.Select(t => t.PayScaleAssignment))
                        //.Include(e => e.EmpSalStructDetails.Select(t => t.PayScaleAssignment.PayScaleAgreement))

                        int CompId = Convert.ToInt32(SessionManager.CompanyId);
                        //var SalStruct = EmpSalStruct.EmpSalStruct.Where(r => r.EndDate == null).SingleOrDefault();
                        if (SalStruct != null)
                        {
                            var OEmpSalStructDet = SalStruct.EmpSalStructDetails.Select(r => r.PayScaleAssignment.PayScaleAgreement.Id == PayScaleAgreementId).ToList();
                            if (OEmpSalStructDet.Count > 0)
                            {
                                using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                              new System.TimeSpan(0, 30, 0)))
                                {
                                    try
                                    {
                                        List<int> q = new List<int>();
                                        if (!a.Employee.EmpOffInfo.VPFAppl)
                                        {
                                            q = db.PayScaleAssignment.Where(e => e.PayScaleAgreement.Id == PayScaleAgreementId && e.CompanyPayroll.Company.Id == CompId && e.SalaryHead.Code.ToUpper() != "VPF").Select(e => e.SalaryHead.Id).ToList();
                                        }
                                        else
                                        {
                                            q = db.PayScaleAssignment.Where(e => e.PayScaleAgreement.Id == PayScaleAgreementId && e.CompanyPayroll.Company.Id == CompId).Select(e => e.SalaryHead.Id).ToList();
                                        }
                                        // var q = db.PayScaleAssignment.Where(e => e.PayScaleAgreement.Id == PayScaleAgreementId ).OrderBy(e => e.SalaryHead.Id).Select(e => e.SalaryHead.Id).ToList();
                                        var t = SalStruct.EmpSalStructDetails.OrderBy(e => e.SalaryHead.Id).Select(r => r.SalaryHead.Id).ToList();
                                        var w = q.Except(t);

                                        if (w.Count() > 0)
                                        {
                                            using (TransactionScope ts1 = new TransactionScope(TransactionScopeOption.Required, new System.TimeSpan(1,0,0)) )
                                            {
                                                SalaryHeadGenProcess.EmployeeSalaryStructCreationWithUpdationTest(SalStruct.Id, 0, PayScaleAgreementId, Convert.ToDateTime(SalStruct.EffectiveDate), a.Id);

                                                ts1.Complete();
                                            }
                                            
                                        }


                                        SalaryHeadGenProcess.EmployeeSalaryStructCreationWithUpdateFormulaTest(SalStruct.Id, 0, PayScaleAgreementId, Convert.ToDateTime(SalStruct.EffectiveDate), a.Id);

                                        //db.RefreshAllEntites(RefreshMode.StoreWins);//added by prashant 14042017
                                        ts.Complete();
                                        //Utility.DumpProcessStatus("Emp ended structure update " + a.Id);
                                    }
                                    catch (DataException ex)
                                    {
                                        LogFile Logfile = new LogFile();
                                        ErrorLog Err = new ErrorLog()
                                        {
                                            ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                                            ExceptionMessage = ex.InnerException.Message,
                                            ExceptionStackTrace = ex.StackTrace,
                                            LineNo = Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                                            LogTime = DateTime.Now
                                        };
                                        Logfile.CreateLogFile(Err);
                                        return Json(new { sucess = false, responseText = ex.InnerException }, JsonRequestBehavior.AllowGet);
                                    }


                                }
                            }
                        }

                    }

                    // return Json(new { success = true, responseText = "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
                    Msg.Add("  Data Saved successfully  ");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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

        public ActionResult Begin_Month(string Month, int PayScaleAgreementId) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var OPayScalAgreement = db.PayScaleAgreement.Where(e => e.Id == PayScaleAgreementId).SingleOrDefault();


                var EmpList = db.EmployeePayroll.ToList();
                foreach (var EmpListitem in EmpList)
                {
                    Employee Employee = db.Employee.Where(e => e.Id == EmpListitem.Employee_Id).SingleOrDefault();
                    EmpListitem.Employee = Employee;
                    List<EmpSalStruct> EmpSalStructList = db.EmpSalStruct.Where(e => e.EmployeePayroll_Id == EmpListitem.Id).ToList();
                    EmpListitem.EmpSalStruct = EmpSalStructList;
                    List<CPIEntryT> CPIEntryTList = db.CPIEntryT.Where(e => e.EmployeePayroll_Id == EmpListitem.Id).ToList();
                    EmpListitem.CPIEntryT = CPIEntryTList;

                    GeoStruct GeoStruct = db.GeoStruct.Where(e => e.Id == Employee.GeoStruct_Id).SingleOrDefault();
                    Employee.GeoStruct = GeoStruct;
                    PayStruct PayStruct = db.PayStruct.Where(e => e.Id == Employee.PayStruct_Id).SingleOrDefault();
                    Employee.PayStruct = PayStruct;
                    FuncStruct FuncStruct = db.FuncStruct.Where(e => e.Id == Employee.FuncStruct_Id).SingleOrDefault();
                    Employee.FuncStruct = FuncStruct;
                    EmpOff EmpOffInfo = db.EmpOff.Where(e => e.Id == Employee.EmpOffInfo_Id).SingleOrDefault();
                    Employee.EmpOffInfo = EmpOffInfo;
                    PayScale PayScale = db.PayScale.Where(e => e.Id == EmpOffInfo.PayScale_Id).SingleOrDefault();
                    EmpOffInfo.PayScale = PayScale;
                    foreach (var EmpSalStructListitem in EmpSalStructList)
                    {
                        List<EmpSalStructDetails> EmpSalStructDetailsList = db.EmpSalStructDetails.Where(e => e.EmpSalStruct_Id == EmpSalStructListitem.Id).ToList();

                        foreach (var EmpSalStructDetailsListitem in EmpSalStructDetailsList)
                        {
                            SalaryHead SalaryHead = db.SalaryHead.Where(e => e.Id == EmpSalStructDetailsListitem.SalaryHead_Id).SingleOrDefault();
                            EmpSalStructDetailsListitem.SalaryHead = SalaryHead;
                            LookupValue SalHeadOperationType = db.LookupValue.Where(e => e.Id == SalaryHead.SalHeadOperationType_Id).SingleOrDefault();
                            EmpSalStructDetailsListitem.SalaryHead.SalHeadOperationType = SalHeadOperationType;
                            PayScaleAssignment PayScaleAssignment = db.PayScaleAssignment.Where(e => e.Id == EmpSalStructDetailsListitem.PayScaleAssignment_Id).SingleOrDefault();
                            EmpSalStructDetailsListitem.PayScaleAssignment = PayScaleAssignment;
                            PayScaleAgreement PayScaleAgreement = db.PayScaleAgreement.Where(e => e.Id == EmpSalStructDetailsListitem.PayScaleAssignment_Id).SingleOrDefault();
                            EmpSalStructDetailsListitem.PayScaleAssignment.PayScaleAgreement = PayScaleAgreement;
                            List<SalHeadFormula> SalHeadFormula = db.PayScaleAssignment.Where(e => e.Id == EmpSalStructDetailsListitem.PayScaleAssignment_Id).Select(e => e.SalHeadFormula.ToList()).SingleOrDefault();

                            EmpSalStructDetailsListitem.PayScaleAssignment.SalHeadFormula = SalHeadFormula;

                        }
                        EmpSalStructListitem.EmpSalStructDetails = EmpSalStructDetailsList;
                    }
                }







                foreach (var a in EmpList)
                {
                    var SalStruct = a.EmpSalStruct.Where(r => r.EndDate == null).ToList();
                    foreach (var b in SalStruct)
                    {
                        var OEmpSalStructDet = b.EmpSalStructDetails.Select(r => r.PayScaleAssignment.PayScaleAgreement.Id == PayScaleAgreementId).ToList();
                        if (OEmpSalStructDet.Count > 0)
                        {
                            Employee OEmployee = db.Employee.Where(e => e.Id == a.Employee.Id).SingleOrDefault();
                            GeoStruct GeoStruct = db.GeoStruct.Where(e => e.Id == OEmployee.GeoStruct_Id).SingleOrDefault();
                            OEmployee.GeoStruct = GeoStruct;
                            PayStruct PayStruct = db.PayStruct.Where(e => e.Id == OEmployee.PayStruct_Id).SingleOrDefault();
                            OEmployee.PayStruct = PayStruct;
                            FuncStruct FuncStruct = db.FuncStruct.Where(e => e.Id == OEmployee.FuncStruct_Id).SingleOrDefault();
                            OEmployee.FuncStruct = FuncStruct;

                            //.Include(e => e.GeoStruct)
                            //.Include(e => e.FuncStruct)
                            //.Include(e => e.PayStruct)

                            using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                          new System.TimeSpan(0, 30, 0)))
                            {
                                try
                                {
                                    SalaryHeadGenProcess.EmployeeSalaryStructCreationWithUpdation(b.Id, OEmployee, OPayScalAgreement, Convert.ToDateTime(b.EffectiveDate), a);
                                    // db.RefreshAllEntites(RefreshMode.StoreWins);//added by prashant 14042017
                                    ts.Complete();

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
                    }

                }

                return Json(new { success = true, responseText = "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);

            }


        }
        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.EmpSalStruct
                    .Include(e => e.EmpSalStructDetails)
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        PayDays = e.PayDays,
                        EffectiveDate = e.EffectiveDate,
                        EndDate = e.EndDate,
                        Action = e.DBTrack.Action
                    }).ToList();

                //var add_data = db.EmpSalStruct
                //  .Include(e => e.EmpSalStructDetails)
                //    .Where(e => e.Id == data)
                //    .Select(e => new
                //    {

                //        EmpSalStructDetails_ID = e.EmpSalStructDetails
                //    }).ToList();



                var EMPSal = db.EmpSalStruct.Find(data);
                TempData["RowVersion"] = EMPSal.RowVersion;
                var Auth = EMPSal.DBTrack.IsModified;
                return Json(new Object[] { Q, "", Auth, JsonRequestBehavior.AllowGet });
            }
        }

        [HttpPost]
        public ActionResult EditSave1(FormCollection form, string MainId, string data, string Empid, string Paymonth, DateTime? FromDt, DateTime ToDt) // Edit submit
        {
            var serialize = new JavaScriptSerializer();
            var obj = serialize.Deserialize<List<DeserializeClassManual>>(data);

            if (obj.Count < 0)
            {
                return Json(new { sucess = true, responseText = "You have to change days to update attendance." }, JsonRequestBehavior.AllowGet);
            }

            List<string> Msg = new List<string>();


            if (Empid != null && Paymonth != null && FromDt != null && MainId != "")
            {
                using (DataBaseContext db = new DataBaseContext())
                {
                    try
                    {
                        List<SalaryArrearPaymentT> SalaryArrearPaymentt = new List<SalaryArrearPaymentT>();
                        SalaryArrearPFT salarrpftdata = new SalaryArrearPFT();

                        double epfwages = 0.0;
                        double salarywages = 0.0;
                        double salarywagesdedution = 0.0;
                        double salarywagesnet = 0.0;

                        int salarrtid = Convert.ToInt32(MainId);
                        SalaryArrearT salarrt = db.SalaryArrearT.Include(q => q.SalaryArrearPaymentT).Where(q => q.Id == salarrtid).SingleOrDefault();
                        var EmpPayrollempid = db.EmployeePayroll.Where(e => e.Id == salarrt.EmployeePayroll_Id).SingleOrDefault().Employee_Id;
                        var chkPFApplicable = db.Employee.Include(e => e.EmpOffInfo).Where(e => e.Id == EmpPayrollempid).SingleOrDefault();

                        if (salarrt != null && salarrt.SalaryArrearPaymentT.Count > 0)
                        {
                            foreach (var item in salarrt.SalaryArrearPaymentT)
                            {
                                if (item.PayMonth == Paymonth)
                                {
                                    return Json(new { sucess = false, responseText = "Data is already created!" }, JsonRequestBehavior.AllowGet);
                                }
                            }
                        }
                        foreach (var salamt in obj)
                        {
                            double minamt = Convert.ToDouble(salamt.Amount);
                            if (salarrt.IsRecovery == false) //if recovery true then minus amount permission ADCC requirement sunil and shankar discuss
                            {
                                if (minamt < 0)
                                {
                                    return Json(new { sucess = false, responseText = "Amount Should not be less than zero" }, JsonRequestBehavior.AllowGet);
                                }
                            }
                        }
                        List<int> salidToremove = new List<int>();
                        using (TransactionScope ts = new TransactionScope())
                        {
                            foreach (var saldata in obj)
                            {
                                SalaryArrearPaymentT salarrear = new SalaryArrearPaymentT();

                                int salid = Convert.ToInt16(saldata.SalaryHeadId);
                                salidToremove.Add(salid);
                                SalaryHead salheadtype = db.SalaryHead.Where(q => q.Id == salid).SingleOrDefault();

                                salarrear.ProcessMonthYear = FromDt.Value.ToString("MM/yyyy");
                                salarrear.PayMonth = Paymonth;
                                salarrear.SalHeadAmount = Convert.ToDouble(saldata.Amount);
                                salarrear.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                salarrear.SalaryHead = salheadtype;

                                db.SalaryArrearPaymentT.Add(salarrear);
                                db.SaveChanges();

                                SalaryArrearPaymentt.Add(salarrear);

                                if (saldata.Type.ToUpper() == "Earning".ToUpper())
                                {
                                    salarywages += Convert.ToDouble(saldata.Amount);
                                }
                                if (saldata.Type.ToUpper() != "Earning".ToUpper())
                                {
                                    salarywagesdedution += Convert.ToDouble(saldata.Amount);
                                }
                                if (chkPFApplicable != null && chkPFApplicable.EmpOffInfo.PFAppl == true)
                                {
                                    var epfmaster = db.PFMaster.Include(q => q.EPFWages).Include(q => q.EPFWages.RateMaster)
                                        .Include(q => q.EPFWages.RateMaster.Select(a => a.SalHead))
                                        .Include(q => q.EPFWages.RateMaster.Select(a => a.SalHead.Type)).Where(e => e.EstablishmentID == chkPFApplicable.EmpOffInfo.PFTrust_EstablishmentId).SingleOrDefault();

                                    var ratemasterwages = epfmaster.EPFWages.RateMaster.Select(q => q.SalHead.Id).ToList();
                                    foreach (var wagesfromepf in ratemasterwages)
                                    {
                                        if (wagesfromepf == salheadtype.Id)
                                        {
                                            epfwages += Convert.ToDouble(saldata.Amount);
                                        }
                                    }
                                }


                                if (saldata.SalHeadOperationType == "EPF")
                                {
                                    salarrpftdata.EmpPF = Convert.ToDouble(saldata.Amount);
                                    salarrpftdata.CompPF = Convert.ToDouble(saldata.Amount);
                                }
                            }

                            List<SalaryHead> AllsalHead = db.SalaryHead
                                .Include(e => e.SalHeadOperationType)
                                .Include(e => e.Frequency)
                                .Where(q => !salidToremove.Contains(q.Id)).ToList();

                            foreach (var item in AllsalHead)
                            {
                                if (item.SalHeadOperationType.LookupVal.ToString().ToUpper() != "LOAN")
                                {
                                    if (item.SalHeadOperationType.LookupVal.ToString().ToUpper() != "PERK")
                                    {

                                        if (item.Frequency.LookupVal.ToString().ToUpper() == "MONTHLY")
                                        {

                                            SalaryArrearPaymentT salarrear = new SalaryArrearPaymentT();

                                            salarrear.ProcessMonthYear = FromDt.Value.ToString("MM/yyyy");
                                            salarrear.PayMonth = Paymonth;
                                            salarrear.SalHeadAmount = 0;
                                            salarrear.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                            salarrear.SalaryHead = item;

                                            db.SalaryArrearPaymentT.Add(salarrear);
                                            db.SaveChanges();

                                            SalaryArrearPaymentt.Add(salarrear);
                                        }
                                    }
                                }
                            }
                            salarrpftdata.EPFWages = epfwages;
                            salarrpftdata.SalaryWages = salarywages;

                            salarrpftdata.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            db.SalaryArrearPFT.Add(salarrpftdata);
                            db.SaveChanges();

                            //if (MainId != "")
                            //{
                            salarywagesnet = salarywages - salarywagesdedution;
                            salarrt.ArrTotalEarning = salarywages;
                            salarrt.ArrTotalDeduction = salarywagesdedution;
                            salarrt.ArrTotalNet = salarywagesnet;
                            salarrt.SalaryArrearPFT = salarrpftdata;
                            salarrt.SalaryArrearPaymentT = SalaryArrearPaymentt;
                            db.Entry(salarrt).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            db.Entry(salarrt).State = System.Data.Entity.EntityState.Detached;
                            // }

                            ts.Complete();
                            // Msg.Add("Data Saved Successfully");
                            return Json(new { sucess = true, responseText = "Data Saved Successfully" }, JsonRequestBehavior.AllowGet);

                            //return Json(new { sucess = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }
                        //  
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
                        return Json(new { sucess = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }
                }
            }
            return View();
        }


        [HttpPost]
        public ActionResult EditSave(EmpSalStruct EmpSalStruct, int data, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {   //string Emp = forwarddata == "0" ? "" : forwarddata;
                string PayScaleAgr = form["payscaleagreement_drop"] == "0" ? "" : form["payscaleagreement_drop"];
                string Effective_date = form["Effective_Date"] == "0" ? "" : form["Effective_Date"];


                if (Effective_date != null && Effective_date != "")
                {
                    var val = DateTime.Parse(Effective_date);
                    EmpSalStruct.EffectiveDate = val;
                }

                //if (EmpSalStructDetails != "" && EmpSalStructDetails != null)
                //{
                //    List<EmpSalStructDetails> EMP = new List<EmpSalStructDetails>();
                //    var ids = one_ids(EmpSalStructDetails);
                //    foreach (var ca in ids)
                //    {
                //        var val = db.EmpSalStructDetails.Find(ca);
                //        EMP.Add(val);
                //        EmpSalStruct.EmpSalStructDetails = EMP;
                //    }
                //}


                Employee OEmployee = db.Employee
                     .Include(e => e.GeoStruct)
                     .Include(e => e.FuncStruct)
                     .Include(e => e.PayStruct)
                      .Where(e => e.Id == 1)// && r.GeoStruct.Company.Id==1)
                      .SingleOrDefault();
                //var OEmployeePayroll = new EmployeePayroll();
                var OEmployeePayroll
                = db.EmployeePayroll
                .Where(e => e.Employee.Id == 1).SingleOrDefault();

                if (PayScaleAgr != null)
                {
                    int PayScaleAgrId = int.Parse(PayScaleAgr);
                    var OPayScalAgreement = db.PayScaleAgreement.Where(e => e.Id == PayScaleAgrId).SingleOrDefault();
                }


                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        SalaryHeadGenProcess.EmployeeSalaryStructUpdate(OEmployeePayroll, Convert.ToDateTime(Effective_date));

                        ts.Complete();
                        return Json(new { success = true, responseText = "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
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
                        return Json(new { success = false, responseText = ex.InnerException }, JsonRequestBehavior.AllowGet);
                    }
                }
                return View();
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
            public SalaryHead SalaryHead { get; set; }
            public bool Editable { get; set; }
            public double Amount { get; set; }
            public int Salheadformula_id { get; set; }
            public string FormulaEditable { get; set; }
            public string FormulaType { get; set; }
        }

        public class DeserializeClass
        {
            public String Id { get; set; }
            public String Amount { get; set; }
            public String FormulaEditable { get; set; }

        }

        public class DeserializeClassManual
        {
            public String Id { get; set; }
            public String SalaryHead { get; set; }
            public String Amount { get; set; }
            public String Frequency { get; set; }
            public String Type { get; set; }
            public String SalHeadOperationType { get; set; }
            public String SalaryHeadId { get; set; }
        }

        public class P2BGridData
        {
            public string Id { get; set; }

            public string struct_Id { get; set; }
            //public Employee Employee { get; set; }
            public string EmpCode { get; set; }//FullNameFML
            public string FullNameFML { get; set; }//FullNameFML
            public string EffectiveDate { get; set; }
            public string EndDate { get; set; }
            //  public int PayScaleAgreement_Id { get; set; }
        }

        public JsonResult GetPayscaleagreement(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //var a = db.PayScaleAgreement.Find(int.Parse(data));
                int Struct_Id = int.Parse(data);
                var a = db.EmpSalStruct.Include(e => e.EmpSalStructDetails).Include(e => e.EmpSalStructDetails.Select(r => r.PayScaleAssignment))
                    .Include(e => e.EmpSalStructDetails.Select(r => r.PayScaleAssignment.PayScaleAgreement))
                    .Where(e => e.Id == Struct_Id).AsNoTracking().SingleOrDefault()
                    .EmpSalStructDetails.FirstOrDefault().PayScaleAssignment.PayScaleAgreement;
                return Json(a, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult process(string forwarddata, FormCollection form, String selected)
        {
            List<string> Msg = new List<string>();
            try
            {
                var serialize = new JavaScriptSerializer();
                var obj = serialize.Deserialize<List<DeserializeClass>>(forwarddata);
                using (DataBaseContext db = new DataBaseContext())
                {

                    if (obj == null || obj.Count < 0)
                    {
                        return Json(new { success = false, responseText = "You have to change amount to update salary structure." }, JsonRequestBehavior.AllowGet);
                    }

                    List<int> b = obj.Select(e => int.Parse(e.Id)).ToList();

                    string PayScaleAgr = form["payscaleagreement_id"] == "0" ? "" : form["payscaleagreement_id"];
                    string Effective_date = form["Effective_Date"] == "0" ? "" : form["Effective_Date"];

                    using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                          new System.TimeSpan(0, 120, 0)))
                    {
                        foreach (int ca in b)
                        {
                            List<EmpSalStructDetails> OEmpSalStructDetails = new List<EmpSalStructDetails>();
                            EmpSalStructDetails EmpSalStructDet = db.EmpSalStructDetails.Where(e => e.Id == ca).FirstOrDefault();
                            EmpSalStruct EmpSalStruct = db.EmpSalStruct.Where(e => e.Id == EmpSalStructDet.EmpSalStruct_Id).SingleOrDefault();
                            EmpSalStructDet.EmpSalStruct = EmpSalStruct;
                            EmployeePayroll EmployeePayroll = db.EmployeePayroll.Where(e => e.Id == EmpSalStruct.EmployeePayroll_Id).SingleOrDefault();
                            EmpSalStruct.EmployeePayroll = EmployeePayroll;
                            SalaryHead SalaryHead = db.SalaryHead.Include(x => x.RoundingMethod).Where(e => e.Id == EmpSalStructDet.SalaryHead_Id).SingleOrDefault();
                            EmpSalStructDet.SalaryHead = SalaryHead;
                            LookupValue SalHeadOperationType = db.LookupValue.Where(e => e.Id == SalaryHead.SalHeadOperationType_Id).SingleOrDefault();
                            SalaryHead.SalHeadOperationType = SalHeadOperationType;
                            SalHeadFormula SalHeadFormula = db.SalHeadFormula.Include(e => e.FormulaType).Where(e => e.Id == EmpSalStructDet.SalHeadFormula_Id).SingleOrDefault();
                            EmpSalStructDet.SalHeadFormula = SalHeadFormula;
                            PayScaleAssignment PayScaleAssignment = db.PayScaleAssignment.Include(x => x.SalHeadFormula).Where(e => e.Id == EmpSalStructDet.PayScaleAssignment_Id).SingleOrDefault();
                            EmpSalStructDet.PayScaleAssignment = PayScaleAssignment;
                            List<SalHeadFormula> SalHeadFormulaList = db.PayScaleAssignment.Where(e => e.Id == EmpSalStructDet.PayScaleAssignment_Id).Select(e => e.SalHeadFormula.ToList()).SingleOrDefault();

                            foreach (var SalHeadFormulaListitem in SalHeadFormulaList)
                            {
                                LookupValue FormulaType = db.SalHeadFormula.Where(e => e.Id == SalHeadFormulaListitem.Id).Select(e => e.FormulaType).SingleOrDefault();
                                SalHeadFormulaListitem.FormulaType = FormulaType;
                                GeoStruct GeoStruct1 = db.GeoStruct.Where(e => e.Id == SalHeadFormulaListitem.GeoStruct_Id).SingleOrDefault();
                                SalHeadFormulaListitem.GeoStruct = GeoStruct1;
                                PayStruct PayStruct1 = db.PayStruct.Where(e => e.Id == SalHeadFormulaListitem.PayStruct_Id).SingleOrDefault();
                                SalHeadFormulaListitem.PayStruct = PayStruct1;
                                FuncStruct FuncStruct1 = db.FuncStruct.Where(e => e.Id == SalHeadFormulaListitem.FuncStruct_Id).SingleOrDefault();
                                SalHeadFormulaListitem.FuncStruct = FuncStruct1;

                            }
                            PayScaleAssignment.SalHeadFormula = SalHeadFormulaList;
                            //.Include(e => e.EmpSalStruct)
                            //.Include(e => e.EmpSalStruct.EmployeePayroll)
                            //.Include(e => e.SalaryHead)
                            //.Include(e => e.SalaryHead.SalHeadOperationType)
                            //.Include(e => e.PayScaleAssignment)
                            //.Include(e => e.PayScaleAssignment.SalHeadFormula)
                            //.Include(e => e.PayScaleAssignment.SalHeadFormula.Select(t => t.FormulaType))
                            //.Include(e => e.PayScaleAssignment.SalHeadFormula.Select(t => t.GeoStruct))
                            //.Include(e => e.PayScaleAssignment.SalHeadFormula.Select(t => t.PayStruct))
                            //.Include(e => e.PayScaleAssignment.SalHeadFormula.Select(t => t.FuncStruct))
                            //.Include(e => e.SalHeadFormula)
                            //.Include(e => e.SalHeadFormula.FormulaType)


                            EmpSalStructDet.DBTrack = new DBTrack
                            {
                                CreatedBy = EmpSalStructDet.DBTrack.CreatedBy == null ? null : EmpSalStructDet.DBTrack.CreatedBy,
                                CreatedOn = EmpSalStructDet.DBTrack.CreatedOn == null ? null : EmpSalStructDet.DBTrack.CreatedOn,
                                Action = "M",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };
                            double CheckAmountzeroOrnot = Convert.ToDouble(obj.Where(e => e.Id == ca.ToString()).Select(e => e.Amount).Single());
                            //if (CheckAmountzeroOrnot != 0.0)
                            if (CheckAmountzeroOrnot >= 0.0)
                            {
                                EmpSalStructDet.SalaryHead = db.SalaryHead.Where(e => e.Id == EmpSalStructDet.SalaryHead.Id).SingleOrDefault();
                                EmpSalStructDet.Amount = CheckAmountzeroOrnot;
                                //SalHeadFormula Salformula = SalaryHeadGenProcess.SalFormulaFinderNewNonStandard(EmpSalStructDet.EmpSalStruct, EmpSalStructDet.PayScaleAssignment, EmpSalStructDet.SalaryHead.Id);
                                //OEmpSalStructDetailsObj.SalHeadFormula = Salformula == null ? Salformula : db.SalHeadFormula.Where(e => e.Id == Salformula.Id).SingleOrDefault();
                            }
                            string CheckFormula = obj.Where(e => e.Id == ca.ToString()).Select(e => e.FormulaEditable).Single().ToUpper();

                            if (CheckFormula == "N" && (EmpSalStructDet.SalHeadFormula != null && EmpSalStructDet.SalHeadFormula.FormulaType.LookupVal.ToUpper() == "NONSTANDARDFORMULA"))
                            {
                                EmpSalStructDet.SalHeadFormula = null;
                            }
                            else if (CheckFormula == "Y" && (EmpSalStructDet.SalHeadFormula == null))
                            {
                                SalHeadFormula Salformula = SalaryHeadGenProcess.SalFormulaFinderNew(EmpSalStructDet.EmpSalStruct, EmpSalStructDet.PayScaleAssignment, EmpSalStructDet.SalaryHead.Id);
                                EmpSalStructDet.SalHeadFormula = Salformula == null ? Salformula : db.SalHeadFormula.Where(e => e.Id == Salformula.Id).SingleOrDefault();
                                if (EmpSalStructDet.SalHeadFormula != null)
                                {
                                    if (EmpSalStructDet.SalHeadFormula.FormulaType.LookupVal.ToString().ToUpper() == "NONSTANDARDFORMULA")
                                    {
                                        if (EmpSalStructDet.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() != "BASIC")
                                        {
                                            double SalAmount = SalaryHeadGenProcess.SalHeadAmountCalc(EmpSalStructDet.SalHeadFormula.Id, null, OEmpSalStructDetails, EmpSalStructDet.EmpSalStruct.EmployeePayroll, EmpSalStructDet.EmpSalStruct.EffectiveDate.Value.ToString("MM/yyyy"));
                                            SalAmount = SalaryHeadGenProcess.RoundingFunction(EmpSalStructDet.SalaryHead, SalAmount);
                                            EmpSalStructDet.Amount = SalAmount;
                                        }
                                    }
                                    else
                                    {
                                        EmpSalStructDet.Amount = Convert.ToDouble(obj.Where(e => e.Id == ca.ToString()).Select(e => e.Amount).Single());
                                    }
                                }
                                else
                                {
                                    EmpSalStructDet.Amount = CheckAmountzeroOrnot;
                                }
                            }

                            //db.ExecuteStoreCommand("UPDATE EmpSalStructDetails SET Amount = " + EmpSalStructDet.Amount + " WHERE Id = " + EmpSalStructDet.Id);
                            //db.SaveChanges();

                            //var EmpSalS = db.EmpSalStruct.Where(e => e.Id == EmpSalStructDet.EmpSalStruct_Id).SingleOrDefault();
                            //EmpSalS.EmpSalStructDetails.Add(EmpSalStructDet);
                            //EmpSalS.EmpSalStructDetails.Add(EmpSalStructDet);
                            //db.EmpSalStruct.Select(e=>e.EffectiveDate, )
                            //db.EmpSalStruct.Attach(EmpSalS);
                            //db.Entry(EmpSalS).State = System.Data.Entity.EntityState.Modified;
                            //db.SaveChanges();
                            //db.Entry(EmpSalStructDet).State = System.Data.Entity.EntityState.Detached;

                            db.EmpSalStructDetails.Attach(EmpSalStructDet);
                            db.Entry(EmpSalStructDet).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            db.Entry(EmpSalStructDet).State = System.Data.Entity.EntityState.Detached;
                        }
                        int EmpId = int.Parse(selected);
                        DateTime? mEffectiveDate = Convert.ToDateTime(Effective_date);
                        //Employee OEmployee = db.Employee
                        //     .Include(e => e.GeoStruct)
                        //     .Include(e => e.FuncStruct)
                        //     .Include(e => e.PayStruct)
                        //      .Where(e => e.Id == EmpId)
                        //      .SingleOrDefault();


                        var OEmployeePayroll = db.EmployeePayroll.Where(e => e.Employee.Id == EmpId).FirstOrDefault();
                        Employee OEmployee = db.Employee.Where(e => e.Id == OEmployeePayroll.Employee_Id).SingleOrDefault();
                        OEmployeePayroll.Employee = OEmployee;
                        EmpOff EmpOffInfo = db.EmpOff.Where(e => e.Id == OEmployee.EmpOffInfo_Id).SingleOrDefault();
                        OEmployee.EmpOffInfo = EmpOffInfo;
                        GeoStruct GeoStruct = db.GeoStruct.Where(e => e.Id == OEmployee.GeoStruct_Id).SingleOrDefault();
                        OEmployee.GeoStruct = GeoStruct;
                        PayStruct PayStruct = db.PayStruct.Where(e => e.Id == OEmployee.PayStruct_Id).SingleOrDefault();
                        OEmployee.PayStruct = PayStruct;
                        FuncStruct FuncStruct = db.FuncStruct.Where(e => e.Id == OEmployee.FuncStruct_Id).SingleOrDefault();
                        OEmployee.FuncStruct = FuncStruct;
                        ServiceBookDates ServiceBookDates = db.ServiceBookDates.Where(e => e.Id == OEmployee.ServiceBookDates_Id).SingleOrDefault();
                        OEmployee.ServiceBookDates = ServiceBookDates;

                        ////.Include(e => e.EmpSalStruct)
                        //.Include(e => e.Employee.EmpOffInfo)
                        // //.Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.RoundingMethod)))
                        // //.Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.PayScaleAssignment.SalHeadFormula)))//added by prashant 14042017
                        // //.Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.SalHeadOperationType)))
                        //.FirstOrDefault();

                        var OEmpsalstructdata = db.EmpSalStruct.Where(e => e.EmployeePayroll.Employee.Id == EmpId && e.EffectiveDate == mEffectiveDate).ToList();
                        foreach (var OEmpsalstructdataitem in OEmpsalstructdata)
                        {
                            List<EmpSalStructDetails> EmpSalStructDetails = db.EmpSalStructDetails.Where(e => e.EmpSalStruct_Id == OEmpsalstructdataitem.Id).ToList();

                            foreach (var EmpSalStructDetailsitem in EmpSalStructDetails)
                            {
                                SalaryHead SalaryHead = db.SalaryHead.Include(x => x.RoundingMethod).Where(e => e.Id == EmpSalStructDetailsitem.SalaryHead_Id).SingleOrDefault();
                                EmpSalStructDetailsitem.SalaryHead = SalaryHead;
                                LookupValue SalHeadOperationType = db.LookupValue.Where(e => e.Id == SalaryHead.SalHeadOperationType_Id).SingleOrDefault();
                                EmpSalStructDetailsitem.SalaryHead.SalHeadOperationType = SalHeadOperationType;
                                PayScaleAssignment PayScaleAssignment = db.PayScaleAssignment.Where(e => e.Id == EmpSalStructDetailsitem.PayScaleAssignment_Id).SingleOrDefault();
                                EmpSalStructDetailsitem.PayScaleAssignment = PayScaleAssignment;
                                List<SalHeadFormula> SalHeadFormula = db.PayScaleAssignment.Where(e => e.Id == EmpSalStructDetailsitem.PayScaleAssignment_Id).Select(e => e.SalHeadFormula.ToList()).SingleOrDefault();
                                EmpSalStructDetailsitem.PayScaleAssignment.SalHeadFormula = SalHeadFormula;


                            }
                            OEmpsalstructdataitem.EmpSalStructDetails = EmpSalStructDetails;
                        }
                        //.Include(e => e.EmpSalStructDetails.Select(t => t.SalaryHead.RoundingMethod))
                        //.Include(e => e.EmpSalStructDetails.Select(t => t.PayScaleAssignment.SalHeadFormula))//added by prashant 14042017
                        //.Include(e => e.EmpSalStructDetails.Select(t => t.SalaryHead.SalHeadOperationType))



                        if (PayScaleAgr != null && PayScaleAgr != "")
                        {
                            int PayScaleAgrId = int.Parse(PayScaleAgr);
                            var OPayScalAgreement = db.PayScaleAgreement.Where(e => e.Id == PayScaleAgrId).SingleOrDefault();
                            if (OPayScalAgreement != null)
                            {
                                var PayScaleAssign = db.PayScaleAssignment.Where(e => e.PayScaleAgreement.Id == PayScaleAgrId).ToList();
                                if (PayScaleAssign.Count == 0)
                                {
                                    return Json(new { success = false, responseText = "PayScalessignment not defined." }, JsonRequestBehavior.AllowGet);
                                }
                            }
                        }



                        try
                        {
                            //SalaryHeadGenProcess.EmployeeSalaryStructUpdate(OEmployeePayroll1, Convert.ToDateTime(Effective_date));
                            SalaryHeadGenProcess.EmployeeSalaryStructUpdateAmount(OEmployeePayroll, Convert.ToDateTime(Effective_date), OEmpsalstructdata);
                            ts.Complete();
                            return Json(new { success = true, responseText = "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
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
                            return Json(new { success = false, responseText = ex.InnerException }, JsonRequestBehavior.AllowGet);
                        }
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
                IEnumerable<EditData> EmpSalStruct = null;

                List<EditData> model = new List<EditData>();
                var view = new EditData();

                int EmpId = int.Parse(gp.id);
                bool EditAppl = true;
                string FormulaActive = "Y";
                string FormulaType = "";
                var Structid = Convert.ToInt32(gp.filter);

                var OEmployeeSalStruct = db.EmployeePayroll.Where(e => e.Employee.Id == EmpId).SingleOrDefault();
                Employee OEmployee = db.Employee.Where(e => e.Id == OEmployeeSalStruct.Employee_Id).SingleOrDefault();
                OEmployeeSalStruct.Employee = OEmployee;
                GeoStruct GeoStruct = db.GeoStruct.Where(e => e.Id == OEmployee.GeoStruct_Id).SingleOrDefault();
                OEmployee.GeoStruct = GeoStruct;
                PayStruct PayStruct = db.PayStruct.Where(e => e.Id == OEmployee.PayStruct_Id).SingleOrDefault();
                OEmployee.PayStruct = PayStruct;
                FuncStruct FuncStruct = db.FuncStruct.Where(e => e.Id == OEmployee.FuncStruct_Id).SingleOrDefault();
                OEmployee.FuncStruct = FuncStruct;
                List<EmpSalStruct> EmpSalStructList = db.EmpSalStruct.Where(e => e.EmployeePayroll_Id == OEmployeeSalStruct.Id && e.Id == Structid).ToList();
                OEmployeeSalStruct.EmpSalStruct = EmpSalStructList;
                foreach (var EmpSalStructListitem in EmpSalStructList)
                {
                    List<EmpSalStructDetails> EmpSalStructDetails = db.EmpSalStructDetails.Where(e => e.EmpSalStruct_Id == EmpSalStructListitem.Id).ToList();
                    EmpSalStructListitem.EmpSalStructDetails = EmpSalStructDetails;
                    foreach (var EmpSalStructDetailsitem in EmpSalStructDetails)
                    {
                        SalaryHead SalaryHead = db.SalaryHead.Include(e => e.Type).Where(e => e.Id == EmpSalStructDetailsitem.SalaryHead_Id).Single();
                        EmpSalStructDetailsitem.SalaryHead = SalaryHead;
                        LookupValue SalHeadOperationType = db.LookupValue.Where(e => e.Id == SalaryHead.SalHeadOperationType_Id).SingleOrDefault();
                        SalaryHead.SalHeadOperationType = SalHeadOperationType;
                        LookupValue Frequency = db.LookupValue.Where(e => e.Id == SalaryHead.Frequency_Id).SingleOrDefault();
                        SalaryHead.Frequency = Frequency;
                    }
                }

                // .Include(e => e.Employee).Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails))
                //.Include(e => e.EmpSalStruct.Select(r => r.GeoStruct))
                // .Include(e => e.EmpSalStruct.Select(r => r.GeoStruct.Company))
                // .Include(e => e.EmpSalStruct.Select(r => r.GeoStruct.Corporate))
                // .Include(e => e.EmpSalStruct.Select(r => r.GeoStruct.Department))
                // .Include(e => e.EmpSalStruct.Select(r => r.GeoStruct.Division))
                // .Include(e => e.EmpSalStruct.Select(r => r.GeoStruct.Group))
                // .Include(e => e.EmpSalStruct.Select(r => r.GeoStruct.Location))
                // .Include(e => e.EmpSalStruct.Select(r => r.GeoStruct.Region))
                // .Include(e => e.EmpSalStruct.Select(r => r.GeoStruct.Unit))
                //.Include(e => e.EmpSalStruct.Select(r => r.FuncStruct))
                //.Include(e => e.EmpSalStruct.Select(r => r.FuncStruct.Job))
                //.Include(e => e.EmpSalStruct.Select(r => r.FuncStruct.JobPosition))
                //.Include(e => e.EmpSalStruct.Select(r => r.PayStruct))
                //.Include(e => e.EmpSalStruct.Select(r => r.PayStruct.Grade))
                //.Include(e => e.EmpSalStruct.Select(r => r.PayStruct.Level))
                //.Include(e => e.EmpSalStruct.Select(r => r.PayStruct.JobStatus))
                //.Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.PayScaleAssignment)))
                //                    .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead)))
                //                    .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.SalHeadOperationType)))
                //                    .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.Frequency)))
                //                    .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.Type)))
                ////                    .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.PayScaleAssignment.SalHeadFormula)))
                ////.Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.OrderBy(t => t.SalaryHead.SeqNo)))


                var id = Convert.ToInt32(gp.filter);
                var OEmpSalStruct = OEmployeeSalStruct.EmpSalStruct.Where(e => e.Id == id);

                foreach (var x in OEmpSalStruct)
                {

                    var OEmpSalStructDet = x.EmpSalStructDetails;
                    foreach (var SalForAppl in OEmpSalStructDet)
                    {
                        var m = db.EmpSalStructDetails.Where(e => e.Id == SalForAppl.Id).SingleOrDefault();
                        SalaryHead SalaryHead = db.SalaryHead.Where(e => e.Id == m.SalaryHead_Id).SingleOrDefault();
                        m.SalaryHead = SalaryHead;
                        SalHeadFormula SalHeadFormula = db.SalHeadFormula.Include(e => e.FormulaType).Where(e => e.Id == m.SalHeadFormula_Id).SingleOrDefault();
                        m.SalHeadFormula = SalHeadFormula;

                        //var OSalHeadFormula = x.EmpSalStructDetails
                        //    .Where(e => e.SalaryHead.Id == m.SalaryHead.Id).Select(e => e.PayScaleAssignment.SalHeadFormula
                        //         .Where(r => (r.GeoStruct.Corporate == null || r.GeoStruct.Corporate == x.GeoStruct.Corporate)
                        //             && (r.GeoStruct.Region == null || r.GeoStruct.Region == x.GeoStruct.Region)
                        //             && (r.GeoStruct.Company == null || r.GeoStruct.Company == x.GeoStruct.Company)
                        //            && (r.GeoStruct.Division == null || r.GeoStruct.Division == x.GeoStruct.Division)
                        //            && (r.GeoStruct.Location == null || r.GeoStruct.Location == x.GeoStruct.Location)
                        //            && (r.GeoStruct.Department == null || r.GeoStruct.Department == x.GeoStruct.Department)
                        //            && (r.GeoStruct.Group == null || r.GeoStruct.Group == x.GeoStruct.Group)
                        //            && (r.GeoStruct.Unit == null || r.GeoStruct.Unit == x.GeoStruct.Unit)
                        //            && (r.FuncStruct.Job == null || r.FuncStruct.Job == x.FuncStruct.Job)
                        //            && (r.FuncStruct.JobPosition == null || r.FuncStruct.JobPosition == x.FuncStruct.JobPosition)
                        //            && (r.PayStruct.Grade == null || r.PayStruct.Grade == x.PayStruct.Grade)
                        //            && (r.PayStruct.Level == null || r.PayStruct.Level == x.PayStruct.Level)
                        //             )).ToList();


                        var SalHeadForm = m.SalHeadFormula; //SalaryHeadGenProcess.SalFormulaFinder(x, m.SalaryHead.Id);

                        if (SalForAppl.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() == "BASIC")
                        {
                            FormulaType = "STANDARDFORMULA";
                            EditAppl = true;
                            FormulaActive = "N";
                        }
                        if (SalHeadForm != null)
                        {
                            if (SalHeadForm.FormulaType.LookupVal.ToUpper() == "STANDARDFORMULA" && SalForAppl.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() != "BASIC")
                            {
                                FormulaActive = "N";
                                EditAppl = false;
                                FormulaType = "STANDARDFORMULA";
                            }
                            else if (SalHeadForm.FormulaType.LookupVal.ToUpper() == "NONSTANDARDFORMULA" && SalForAppl.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() != "BASIC")
                            {
                                FormulaActive = "Y";
                                EditAppl = true;
                                FormulaType = "NONSTANDARDFORMULA";
                            }
                        }
                        else
                        {
                            if (SalForAppl.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() == "EPF" || SalForAppl.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() == "PTAX" ||
                                SalForAppl.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() == "ITAX" || SalForAppl.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() == "LWF" ||
                                SalForAppl.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() == "ESIC" || SalForAppl.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() == "CPF" ||
                                SalForAppl.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() == "PENSION")
                                EditAppl = false;
                            else
                                EditAppl = true;


                            var test = db.PayScaleAssignment.AsNoTracking().Where(e => e.SalaryHead.Id == SalForAppl.SalaryHead.Id && e.SalaryHead.SalHeadOperationType.LookupVal.ToString() != "BASIC").Include(e => e.SalHeadFormula.Select(r => r.FormulaType)).FirstOrDefault();
                            if (test != null)
                            {
                                if (test.SalHeadFormula.Count() > 0 && test.SalHeadFormula.Any(r => r.FormulaType.LookupVal.ToUpper() == "STANDARDFORMULA"))
                                {
                                    if (SalHeadForm != null)//if one salaryhead (e.g. CCA) one grade formula but other grade not formula then other grade value can update in structure(NKGSB)
                                    {
                                        FormulaActive = "N";
                                        EditAppl = false;
                                        FormulaType = "STANDARDFORMULA";
                                    }
                                    else
                                    {
                                        FormulaActive = "N";
                                        EditAppl = true;
                                        FormulaType = "STANDARDFORMULA";
                                    }
                                   
                                }
                                else if (test.SalHeadFormula.Count() > 0 && test.SalHeadFormula.Any(r => r.FormulaType.LookupVal.ToUpper() == "NONSTANDARDFORMULA"))
                                {
                                    FormulaActive = "N";
                                    EditAppl = true;
                                    FormulaType = "NONSTANDARDFORMULA";
                                }
                                else
                                {
                                    FormulaActive = "N";
                                    EditAppl = true;
                                    FormulaType = "";
                                }
                            }
                        }

                        if (SalForAppl.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() != "LOAN" && SalForAppl.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() != "NONREGULAR" &&
                            SalForAppl.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() != "EPF" && SalForAppl.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() != "PTAX" &&
                                SalForAppl.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() != "ITAX" && SalForAppl.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() != "LWF" &&
                                SalForAppl.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() != "ESIC" && SalForAppl.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() != "CPF" &&
                                SalForAppl.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() != "PENSION" && SalForAppl.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() != "GROSS" &&
                             SalForAppl.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() != "NET" && SalForAppl.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() != "INSURANCE" &&
                            SalForAppl.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() != "COMPLWF" && SalForAppl.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() != "COMPESIC" &&
                            SalForAppl.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() != "PERK" && SalForAppl.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() != "GRATUITY" &&
                            SalForAppl.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() != "ARREARDED" && SalForAppl.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() != "ARREAREARN")
                        {
                            view = new EditData()
                            {
                                Id = SalForAppl.Id,
                                SalaryHead = SalForAppl.SalaryHead,
                                Amount = SalForAppl.Amount,
                                Editable = EditAppl,
                                Salheadformula_id = SalHeadForm != null ? SalHeadForm.Id : 0,
                                FormulaEditable = FormulaActive,
                                FormulaType = FormulaType
                            };

                            model.Add(view);
                        }
                    }
                }

                //  EmpSalStruct = model.OrderByDescending(e => e.SalaryHead.Type.LookupVal.ToString()).ThenBy(e => e.SalaryHead.SeqNo);
                EmpSalStruct = model.Where(e => e.Amount != 0).OrderByDescending(e => e.SalaryHead.Type.LookupVal.ToString()).ThenBy(e => e.SalaryHead.SeqNo).Union(model.Where(e => e.Amount == 0).OrderByDescending(e => e.SalaryHead.Type.LookupVal.ToString()).ThenBy(e => e.SalaryHead.SeqNo));

                IEnumerable<EditData> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = EmpSalStruct;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.SalaryHead.Name.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                               || (e.Amount.ToString().Contains(gp.searchString.ToUpper()))
                               || (e.SalaryHead.Frequency.LookupVal.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                               || (e.SalaryHead.Type.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                               || (e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                               || (e.Editable.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                               || (e.Id.ToString().Contains(gp.searchString))
                               ).Select(a => new Object[] { a.SalaryHead.Name, a.Amount, a.SalaryHead.Frequency.LookupVal, a.SalaryHead.Type.LookupVal, a.SalaryHead.SalHeadOperationType.LookupVal, a.FormulaType, a.Salheadformula_id, a.FormulaEditable, a.Editable, a.Id }).ToList();

                        //jsonData = IE.Select(a => new Object[] { a.SalaryHead.Name, a.Amount, a.SalaryHead.Frequency.LookupVal, a.SalaryHead.Type.LookupVal, a.SalaryHead.SalHeadOperationType.LookupVal, a.Editable, a.Id }).Where((e => (e.Contains(gp.searchString)))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.SalaryHead.Name, a.Amount, a.SalaryHead.Frequency.LookupVal, a.SalaryHead.Type.LookupVal, a.SalaryHead.SalHeadOperationType.LookupVal, a.FormulaType, a.Salheadformula_id, a.FormulaEditable, a.Editable, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = EmpSalStruct;
                    Func<EditData, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        //orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                        orderfuc = (c => gp.sidx == "Id" ? c.SalaryHead.Type.LookupVal.ToString() : "");

                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "SalaryHead" ? c.SalaryHead.Name.ToString() :
                                         gp.sidx == "Amount" ? c.Amount.ToString() :
                                         gp.sidx == "Frequency" ? c.SalaryHead.Frequency.LookupVal.ToString() :
                                         gp.sidx == "Type" ? c.SalaryHead.Type.LookupVal.ToString() :
                                         gp.sidx == "SalHeadOperationType" ? c.SalaryHead.SalHeadOperationType.LookupVal.ToString() :
                                         gp.sidx == "Editable" ? c.Editable.ToString() :
                                         gp.sidx == "FormulaType" ? c.FormulaType.ToString() :
                                         gp.sidx == "Salheadformula_id" ? c.Salheadformula_id.ToString() :
                                         gp.sidx == "FormulaEditable" ? c.FormulaEditable.ToString() : "");
                    }
                    gp.sord = "desc";
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.SalaryHead.Name, a.Amount, a.SalaryHead.Frequency != null ? Convert.ToString(a.SalaryHead.Frequency.LookupVal) : "", a.SalaryHead.Type != null ? Convert.ToString(a.SalaryHead.Type.LookupVal) : "", a.SalaryHead.SalHeadOperationType != null ? Convert.ToString(a.SalaryHead.SalHeadOperationType.LookupVal) : "", a.FormulaType, a.Salheadformula_id, a.FormulaEditable, a.Editable, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        // IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.SalaryHead.Name, a.Amount, a.SalaryHead.Frequency != null ? Convert.ToString(a.SalaryHead.Frequency.LookupVal) : "", a.SalaryHead.Type != null ? Convert.ToString(a.SalaryHead.Type.LookupVal) : "", a.SalaryHead.SalHeadOperationType != null ? Convert.ToString(a.SalaryHead.SalHeadOperationType.LookupVal) : "", a.FormulaType, a.Salheadformula_id, a.FormulaEditable, a.Editable, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.SalaryHead.Name, a.Amount, a.SalaryHead.Frequency != null ? Convert.ToString(a.SalaryHead.Frequency.LookupVal) : "", a.SalaryHead.Type != null ? Convert.ToString(a.SalaryHead.Type.LookupVal) : "", a.SalaryHead.SalHeadOperationType != null ? Convert.ToString(a.SalaryHead.SalHeadOperationType.LookupVal) : "", a.FormulaType, a.Salheadformula_id, a.FormulaEditable, a.Editable, a.Id }).ToList();
                    }
                    totalRecords = EmpSalStruct.Count();
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

        public ActionResult P2BInlineGridManunal(P2BGrid_Parameters gp, string extraeditdata)
        {
            try
            {
                DataBaseContext db = new DataBaseContext();
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                var jsonData = (Object)null;

                IEnumerable<EditData> EmpSalStruct = null;

                List<EditData> model = new List<EditData>();

                var view = new EditData();

                //int EmpId = 0;

                if (extraeditdata == null)
                {
                    extraeditdata = gp.filter;
                }
                //int EmpId = int.Parse(gp.id);
                bool EditAppl = true;





                //.Include(e => e.Employee).Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails))
                //                    .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead)))
                //                    .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.SalHeadOperationType)))
                //                    .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.Frequency)))
                //                    .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.Type)))

                //04032024 var OEmployeeSalStruct = db.EmployeePayroll.Where(e => e.Employee.EmpCode == extraeditdata).SingleOrDefault();
                //Employee Employee = db.Employee.Where(e => e.Id == OEmployeeSalStruct.Employee_Id).SingleOrDefault();
                //OEmployeeSalStruct.Employee = Employee;
                //List<EmpSalStruct> EmpSalStructList = db.EmpSalStruct.Where(e => e.EmployeePayroll_Id == OEmployeeSalStruct.Id).ToList();

                //foreach (var EmpSalStructListitem in EmpSalStructList)
                //{
                //    List<EmpSalStructDetails> EmpSalStructDetails = db.EmpSalStructDetails.Where(e => e.EmpSalStruct_Id == EmpSalStructListitem.Id).ToList();
                //    EmpSalStructListitem.EmpSalStructDetails = EmpSalStructDetails;
                //    foreach (var EmpSalStructDetailsitem in EmpSalStructDetails)
                //    {
                //        SalaryHead SalaryHead = db.SalaryHead.Include(e => e.Type).Where(e => e.Id == EmpSalStructDetailsitem.SalaryHead_Id).Single();
                //        EmpSalStructDetailsitem.SalaryHead = SalaryHead;
                //        LookupValue SalHeadOperationType = db.LookupValue.Where(e => e.Id == SalaryHead.SalHeadOperationType_Id).SingleOrDefault();
                //        EmpSalStructDetailsitem.SalaryHead.SalHeadOperationType = SalHeadOperationType;
                //        LookupValue Frequency = db.LookupValue.Where(e => e.Id == SalaryHead.Frequency_Id).SingleOrDefault();
                //        EmpSalStructDetailsitem.SalaryHead.Frequency = Frequency;
                //    }
                //}
                //OEmployeeSalStruct.EmpSalStruct = EmpSalStructList;//04032024

                var OEmployeeSalStruct = db.EmpSalStruct.Select(d => new
                {
                    OEmpCode = d.EmployeePayroll.Employee.EmpCode,
                    OEmpSalStructDetails = d.EmpSalStructDetails.Select(r => new
                    {
                        OId = r.Id,
                        OSalHeadFormula = r.SalHeadFormula,
                        OSalaryHeadId = r.SalaryHead.Id,
                        OSalaryHeadSeqNo = r.SalaryHead.SeqNo,
                        OSalaryHeadType = r.SalaryHead.Type.LookupVal,
                        OSalHeadOperationType = r.SalaryHead.SalHeadOperationType.LookupVal,
                        OFrequency = r.SalaryHead.Frequency.LookupVal
                    }).ToList()
                }).Where(e => e.OEmpCode == extraeditdata).ToList().Last();

                // var strid = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.EmpSalStruct).Where(e => e.Employee.Id == EmpId).Last();

                //  var id = Convert.ToInt32(gp.filter);
                //var OEmpSalStruct = OEmployeeSalStruct.EmpSalStruct.Last();//04032024

                //foreach (var x in OEmpSalStruct)
                //{
                //var OEmpSalStructDet = OEmpSalStruct.EmpSalStructDetails;//04032024

                var OEmpSalStructDet = OEmployeeSalStruct.OEmpSalStructDetails.OrderBy(e=>e.OSalaryHeadSeqNo).GroupBy(u => u.OSalaryHeadType);

                foreach (var item in OEmpSalStructDet)
                {
                    foreach (var SalForAppl in item)
                    {
                        //var m = db.EmpSalStructDetails.Include(e => e.SalaryHead).Include(e => e.SalHeadFormula).Where(e => e.Id == SalForAppl.Id).SingleOrDefault();

                        //var SalHeadForm = m.SalHeadFormula; 04032024 //SalaryHeadGenProcess.SalFormulaFinder(x, m.SalaryHead.Id);04032024

                        if (SalForAppl.OSalHeadFormula != null)
                        {
                            if (SalForAppl.OSalHeadOperationType.ToUpper() == "BASIC")
                                EditAppl = true;
                            else
                                EditAppl = true;
                        }
                        else
                        {
                            if (SalForAppl.OSalHeadOperationType.ToUpper() == "EPF" || SalForAppl.OSalHeadOperationType.ToUpper() == "PT" ||
                                SalForAppl.OSalHeadOperationType.ToUpper() == "ITAX" || SalForAppl.OSalHeadOperationType.ToUpper() == "LWF" ||
                                SalForAppl.OSalHeadOperationType.ToUpper() == "ESIC" || SalForAppl.OSalHeadOperationType.ToUpper() == "CPF" ||
                                SalForAppl.OSalHeadOperationType.ToUpper() == "PENSION")
                                EditAppl = true;

                            else
                                EditAppl = true;
                        }
                        if (SalForAppl.OSalHeadOperationType.ToUpper() != "LOAN")
                        {
                            if (SalForAppl.OSalHeadOperationType.ToUpper() != "PERK")
                            {

                                if (SalForAppl.OFrequency.ToUpper() == "MONTHLY")
                                {

                                    view = new EditData()
                                    {
                                        Id = SalForAppl.OId,
                                        SalaryHead = db.SalaryHead.Include(w => w.Frequency).Include(w => w.Type)
                                        .Include(w => w.SalHeadOperationType)
                                        .Where(e => e.Id == SalForAppl.OSalaryHeadId).SingleOrDefault(),
                                        // Amount = SalForAppl.Amount,
                                        Amount = 0,
                                        Editable = EditAppl
                                    };

                                    model.Add(view);
                                }
                            }
                        }
                    }
                }

                EmpSalStruct = model;

                IEnumerable<EditData> IE;

                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = EmpSalStruct;
                    if (gp.searchOper.Equals("eq"))
                    {

                        jsonData = IE.Where(e => (e.SalaryHead.Name.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                              || (e.Amount.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                              || (e.SalaryHead.Frequency.LookupVal.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                              || (e.SalaryHead.Type.LookupVal.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                              || (e.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                              || (e.Editable.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                              || (e.SalaryHead.Id.ToString().Contains(gp.searchString))
                              || (e.Id.ToString().Contains(gp.searchString))
                              ).Select(a => new Object[] { a.SalaryHead.Name, a.Amount, a.SalaryHead.Frequency.LookupVal, a.SalaryHead.Type.LookupVal, a.SalaryHead.SalHeadOperationType.LookupVal, a.Editable, a.SalaryHead.Id, a.Id }).ToList();

                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.SalaryHead.Name, a.Amount, a.SalaryHead.Frequency.LookupVal, a.SalaryHead.Type.LookupVal, a.SalaryHead.SalHeadOperationType.LookupVal, a.Editable, a.SalaryHead.Id, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = EmpSalStruct;
                    Func<EditData, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "SalaryHead" ? c.SalaryHead.Name.ToString() :
                                         gp.sidx == "Amount" ? c.Amount.ToString() :
                                         gp.sidx == "Frequency" ? c.SalaryHead.Frequency.LookupVal.ToString() :
                                         gp.sidx == "Type" ? c.SalaryHead.Type.LookupVal.ToString() :
                                         gp.sidx == "SalHeadOperationType" ? c.SalaryHead.SalHeadOperationType.LookupVal.ToString() :
                                         gp.sidx == "Editable" ? c.Editable.ToString() :
                                         gp.sidx == "SalaryHeadId" ? c.SalaryHead.Id.ToString() : "");
                    }
                    if (gp.sord == "asc")
                    {
                        //IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.SalaryHead.Name, a.Amount, a.SalaryHead.Frequency != null ? Convert.ToString(a.SalaryHead.Frequency.LookupVal) : "", a.SalaryHead.Type != null ? Convert.ToString(a.SalaryHead.Type.LookupVal) : "", a.SalaryHead.SalHeadOperationType != null ? Convert.ToString(a.SalaryHead.SalHeadOperationType.LookupVal) : "", a.Editable, a.SalaryHead.Id, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        //IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.SalaryHead.Name, a.Amount, a.SalaryHead.Frequency != null ? Convert.ToString(a.SalaryHead.Frequency.LookupVal) : "", a.SalaryHead.Type != null ? Convert.ToString(a.SalaryHead.Type.LookupVal) : "", a.SalaryHead.SalHeadOperationType != null ? Convert.ToString(a.SalaryHead.SalHeadOperationType.LookupVal) : "", a.Editable, a.SalaryHead.Id, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.SalaryHead.Name, a.Amount, a.SalaryHead.Frequency != null ? Convert.ToString(a.SalaryHead.Frequency.LookupVal) : "", a.SalaryHead.Type != null ? Convert.ToString(a.SalaryHead.Type.LookupVal) : "", a.SalaryHead.SalHeadOperationType != null ? Convert.ToString(a.SalaryHead.SalHeadOperationType.LookupVal) : "", a.Editable, a.SalaryHead.Id, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
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

        //public ActionResult P2BGrid(P2BGrid_Parameters gp)
        //{
        //    try
        //    {
        //        DataBaseContext db = new DataBaseContext();
        //        int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
        //        int pageSize = gp.rows;
        //        int totalPages = 0;
        //        int totalRecords = 0;
        //        var jsonData = (Object)null;

        //        IEnumerable<P2BGridData> EmpSalStruct = null;
        //        List<P2BGridData> model = new List<P2BGridData>();
        //        P2BGridData view = null;
        //        //////var ab = db.Database.SqlQuery<P2BGridData>("select distinct em.Id,e.Id struct_id,em.EmpCode, n.FullNameFML,  convert(nvarchar(10),e.EffectiveDate,103) EffectiveDate, '' EndDate,p.PayScaleAgreement_Id from EmpSalStruct e inner join EmpSalStructDetails e1 on  e.Id = e1.EmpSalStruct_Id inner join PayScaleAssignment p on e1.PayScaleAssignment_Id = p.Id inner join EmployeePayroll ep on e.EmployeePayroll_Id = ep.id inner join Employee em on ep.Employee_Id = em.Id inner join NameSingle n on n.Id = em.EmpName_Id and e.EndDate is null").ToList();


        //        //////foreach (var b in ab)
        //        //////{
        //        //////    view = new P2BGridData()
        //        //////    {
        //        //////        Id = b.Id,
        //        //////        struct_Id = b.struct_Id,
        //        //////        EmpCode = b.EmpCode,
        //        //////        FullNameFML = b.FullNameFML,
        //        //////        //Employee = z.Employee,
        //        //////        EffectiveDate = b.EffectiveDate != null ? b.EffectiveDate : null,
        //        //////        EndDate = null,
        //        //////        PayScaleAgreement_Id = b.PayScaleAgreement_Id != 0 ? b.PayScaleAgreement_Id : 1
        //        //////    };
        //        //////    model.Add(view);
        //        //////}

        //        //   db.Database.CommandTimeout = 300;

        //        var OEmployee = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.Employee.EmpName).Select(e => new
        //        {
        //            Id = e.Id,
        //            Employee_Id = e.Employee.Id,
        //            EmpCode = e.Employee.EmpCode,
        //            EmpName_FullNameFML = e.Employee.EmpName.FullNameFML,
        //        }).OrderBy(e => e.Id).AsNoTracking().ToList();

        //        foreach (var z in OEmployee)
        //        {
        //            var OEmpSalStruct = db.EmployeePayroll.Where(e => e.Id == z.Id).Select(e => e.EmpSalStruct.Where(r => r.EndDate == null).Select(a => a.Id)).AsNoTracking()
        //                                .SingleOrDefault();

        //            int aaa = OEmpSalStruct.SingleOrDefault();
        //            DateTime? Eff_Date = null;
        //            int EmpStruct_Id = 0;

        //            int PayScaleAgr = 0;
        //            //foreach (var a in OEmpSalStruct)
        //            //{


        //            EmpSalStruct aa = db.EmpSalStruct.Where(e => e.Id == aaa).OrderBy(e => e.Id).SingleOrDefault();
        //            //db.Entry(aa).Collection(e => e.EmpSalStructDetails).Query().Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "BASIC").Single();
        //            //db.Entry(aa.EmpSalStructDetails.First()).Reference(e => e.PayScaleAssignment).Load();
        //            //db.Entry(aa.EmpSalStructDetails.First().PayScaleAssignment).Reference(e => e.PayScaleAgreement).Load();

        //            //var ab = aa.EmpSalStructDetails.Select(r => r.PayScaleAssignment.PayScaleAgreement).FirstOrDefault();
        //            Eff_Date = aa.EffectiveDate;
        //            EmpStruct_Id = aa.Id;
        //           // PayScaleAgr = ab.Id;


        //            //PayScaleAgr = ab.Id; 
        //            if (Eff_Date != null)
        //            {
        //                view = new P2BGridData()
        //                {
        //                    Id = z.Employee_Id,
        //                    struct_Id = EmpStruct_Id,
        //                    EmpCode = z.EmpCode,
        //                    FullNameFML = z.EmpName_FullNameFML,
        //                    //Employee = z.Employee,
        //                    EffectiveDate = Eff_Date.Value != null ? Eff_Date.Value.ToString("dd/MM/yyyy") : null,
        //                    EndDate = null//,
        //                    //PayScaleAgreement_Id = PayScaleAgr != 0 ? PayScaleAgr : 1
        //                };

        //                model.Add(view);
        //            }
        //        }

        //        EmpSalStruct = model;

        //        IEnumerable<P2BGridData> IE;
        //        if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
        //        {
        //            IE = EmpSalStruct;
        //            if (gp.searchOper.Equals("eq"))
        //            {
        //                if (gp.searchField == "Id")
        //                {
        //                    jsonData = IE.Where(e => e.Id.ToString().Contains(gp.searchString)
        //                        ).Select(a => new Object[] { a.Id, a.EmpCode, a.FullNameFML, a.EffectiveDate, a.EndDate, a.PayScaleAgreement_Id, a.struct_Id }).ToList();
        //                }
        //                else if (gp.searchField == "EmpCode")
        //                {
        //                    jsonData = IE.Where(e => e.EmpCode.ToUpper().Contains(gp.searchString.ToUpper())
        //                        ).Select(a => new Object[] { a.Id, a.EmpCode, a.FullNameFML, a.EffectiveDate, a.EndDate, a.PayScaleAgreement_Id, a.struct_Id }).ToList();
        //                }
        //                else if (gp.searchField == "EmpName")
        //                {
        //                    jsonData = IE.Where(e => e.FullNameFML.ToUpper().Contains(gp.searchString.ToUpper())
        //                        ).Select(a => new Object[] { a.Id, a.EmpCode, a.FullNameFML, a.EffectiveDate, a.EndDate, a.PayScaleAgreement_Id, a.struct_Id }).ToList();
        //                }
        //                else if (gp.searchField == "EffectiveDate")
        //                {
        //                    jsonData = IE.Where(e => e.EffectiveDate.ToString().Contains(gp.searchString)
        //                        ).Select(a => new Object[] { a.Id, a.EmpCode, a.FullNameFML, a.EffectiveDate, a.EndDate, a.PayScaleAgreement_Id, a.struct_Id }).ToList();
        //                }
        //                else if (gp.searchField == "EndDate")
        //                {
        //                    jsonData = IE.Where(e => e.EndDate.ToString().Contains(gp.searchString)
        //                        ).Select(a => new Object[] { a.Id, a.EmpCode, a.FullNameFML, a.EffectiveDate, a.EndDate, a.PayScaleAgreement_Id, a.struct_Id }).ToList();
        //                }
        //                else if (gp.searchField == "PayScaleAgreement")
        //                {
        //                    jsonData = IE.Where(e => e.PayScaleAgreement_Id.ToString().Contains(gp.searchString)
        //                        ).Select(a => new Object[] { a.Id, a.EmpCode, a.FullNameFML, a.EffectiveDate, a.EndDate, a.PayScaleAgreement_Id, a.struct_Id }).ToList();
        //                }
        //                else if (gp.searchField == "struct_Id")
        //                {
        //                    jsonData = IE.Where(e => e.struct_Id.ToString().Contains(gp.searchString)
        //                        ).Select(a => new Object[] { a.Id, a.EmpCode, a.FullNameFML, a.EffectiveDate, a.EndDate, a.PayScaleAgreement_Id, a.struct_Id }).ToList();
        //                }


        //                //jsonData = IE.Where(e => (e.Id.ToString().Contains(gp.searchString))
        //                //    || (e.Employee.EmpCode.ToString().Contains(gp.searchString)) || (e.Employee.EmpName.FullNameFML.ToUpper().Contains(gp.searchString.ToUpper()))
        //                //    || (e.EffectiveDate.ToString().Contains(gp.searchString)) || (e.EndDate.ToString().Contains(gp.searchString))
        //                //    ||(e.struct_Id.ToString().Contains(gp.searchString)))
        //                //    .Select(a => new Object[] { a.Id, a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.EffectiveDate, a.EndDate, a.PayScaleAgreement_Id, a.struct_Id }).ToList();
        //                //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.EmpCode, a.FullNameFML, a.EffectiveDate, a.EndDate, a.PayScaleAgreement_Id, a.struct_Id
        //                }).ToList();
        //            }
        //            totalRecords = IE.Count();
        //        }
        //        else
        //        {
        //            IE = EmpSalStruct;
        //            Func<P2BGridData, dynamic> orderfuc;
        //            if (gp.sidx == "Id")
        //            {
        //                orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
        //            }
        //            else
        //            {
        //                orderfuc = (c => gp.sidx == "EmpCode" ? c.EmpCode.ToString() :
        //                                 gp.sidx == "EmpName" ? c.FullNameFML.ToString() :
        //                                 gp.sidx == "EffectiveDate" ? c.EffectiveDate.ToString() :
        //                                 gp.sidx == "EndDate" ? c.EndDate.ToString() :
        //                                 gp.sidx == "struct_Id" ? c.struct_Id.ToString() : "");
        //            }
        //            if (gp.sord == "asc")
        //            {
        //                IE = IE.OrderBy(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, a.EmpCode, a.FullNameFML, a.EffectiveDate, a.EndDate, a.PayScaleAgreement_Id, a.struct_Id }).ToList();
        //            }
        //            else if (gp.sord == "desc")
        //            {
        //                IE = IE.OrderByDescending(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, a.EmpCode, a.FullNameFML, a.EffectiveDate, a.EndDate, a.PayScaleAgreement_Id, a.struct_Id }).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.EmpCode, a.FullNameFML, a.EffectiveDate, a.EndDate, a.PayScaleAgreement_Id, a.struct_Id }).ToList();
        //            }
        //            totalRecords = EmpSalStruct.Count();
        //        }
        //        if (totalRecords > 0)
        //        {
        //            totalPages = (int)Math.Ceiling((float)totalRecords / (float)gp.rows);
        //        }
        //        if (gp.page > totalPages)
        //        {
        //            gp.page = totalPages;
        //        }
        //        var JsonData = new
        //        {
        //            page = gp.page,
        //            rows = jsonData,
        //            records = totalRecords,
        //            total = totalPages
        //        };
        //        return Json(JsonData, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}


        //public ActionResult P2BGrid(P2BGrid_Parameters gp)
        //{
        //    try
        //    {
        //        DataBaseContext db = new DataBaseContext();
        //        int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
        //        int pageSize = gp.rows;
        //        int totalPages = 0;
        //        int totalRecords = 0;
        //        var jsonData = (Object)null;

        //        IEnumerable<P2BGridData> EmpSalStruct = null;
        //        List<P2BGridData> model = new List<P2BGridData>();
        //        P2BGridData view = null;

        //        //   db.Database.CommandTimeout = 300;

        //        var OEmployee = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.Employee.EmpName).Select(e => new
        //        {
        //            Id = e.Id,
        //            Employee_Id = e.Employee.Id,
        //            EmpCode = e.Employee.EmpCode,
        //            EmpName_FullNameFML = e.Employee.EmpName.FullNameFML,
        //        }).ToList();

        //        foreach (var z in OEmployee)
        //        {
        //            var OEmpSalStruct = db.EmployeePayroll.Where(e => e.Id == z.Id).Select(e => e.EmpSalStruct.Where(r => r.EndDate == null).Select(a => a.Id))
        //                                .SingleOrDefault();


        //            //DateTime? Eff_Date = null;
        //            //int EmpStruct_Id = 0;

        //            //PayScaleAgreement PayScaleAgr = null;
        //            //foreach (var a in OEmpSalStruct)
        //            //{
        //            //    var aa = db.EmpSalStruct.Where(e => e.Id == a).Include(e => e.EmpSalStructDetails.Select(r => r.PayScaleAssignment.PayScaleAgreement)).SingleOrDefault();
        //            //    Eff_Date = aa.EffectiveDate;
        //            //    EmpStruct_Id = aa.Id;
        //            //    var EmpSalStructDet = aa.EmpSalStructDetails;
        //            //    if (aa.EmpSalStructDetails.Count == 0)
        //            //    {
        //            //        Eff_Date = null;
        //            //    }
        //            //    foreach (var q in EmpSalStructDet)
        //            //    {
        //            //        PayScaleAgr = q.PayScaleAssignment.PayScaleAgreement;
        //            //        break;
        //            //    }
        //            //    break;
        //            //}
        //            DateTime? Eff_Date = null;
        //            int EmpStruct_Id = 0;

        //            PayScaleAgreement PayScaleAgr = null;

        //            DataBaseContext db1 = new DataBaseContext();
        //            db1.Configuration.AutoDetectChangesEnabled = false;
        //            List<EmpSalStructDetails> EmpSalStructDet = new List<EmpSalStructDetails>();
        //            Parallel.ForEach(OEmpSalStruct, (x) =>
        //            {

        //                var aa = db1.EmpSalStruct.Where(e => e.Id == x).Include(e => e.EmpSalStructDetails.Select(r => r.PayScaleAssignment.PayScaleAgreement)).SingleOrDefault(s => s.Id == x);
        //                Eff_Date = aa.EffectiveDate;
        //                EmpStruct_Id = aa.Id;
        //                EmpSalStructDet = aa.EmpSalStructDetails.ToList();
        //                if (aa.EmpSalStructDetails.Count == 0)
        //                {
        //                    Eff_Date = null;
        //                }

        //                Parallel.ForEach(aa.EmpSalStructDetails, (y) =>
        //                {
        //                    PayScaleAgr = y.PayScaleAssignment.PayScaleAgreement;

        //                });

        //            });

        //            if (Eff_Date != null)
        //            {
        //                view = new P2BGridData()
        //                {
        //                    Id = z.Employee_Id,
        //                    struct_Id = EmpStruct_Id,
        //                    EmpCode = z.EmpCode,
        //                    FullNameFML = z.EmpName_FullNameFML,
        //                    //Employee = z.Employee,
        //                    EffectiveDate = Eff_Date.Value.ToString("dd/MM/yyyy"),
        //                    EndDate = null,
        //                    PayScaleAgreement_Id = PayScaleAgr.Id
        //                };

        //                model.Add(view);
        //            }


        //        }

        //        EmpSalStruct = model;

        //        IEnumerable<P2BGridData> IE;
        //        if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
        //        {
        //            IE = EmpSalStruct;
        //            if (gp.searchOper.Equals("eq"))
        //            {
        //                if (gp.searchField == "Id")
        //                {
        //                    jsonData = IE.Where(e => e.Id.ToString().Contains(gp.searchString)
        //                        ).Select(a => new Object[] { a.Id, a.EmpCode, a.FullNameFML, a.EffectiveDate, a.EndDate, a.PayScaleAgreement_Id, a.struct_Id }).ToList();
        //                }
        //                else if (gp.searchField == "EmpCode")
        //                {
        //                    jsonData = IE.Where(e => e.EmpCode.ToUpper().Contains(gp.searchString.ToUpper())
        //                        ).Select(a => new Object[] { a.Id, a.EmpCode, a.FullNameFML, a.EffectiveDate, a.EndDate, a.PayScaleAgreement_Id, a.struct_Id }).ToList();
        //                }
        //                else if (gp.searchField == "EmpName")
        //                {
        //                    jsonData = IE.Where(e => e.FullNameFML.ToUpper().Contains(gp.searchString.ToUpper())
        //                        ).Select(a => new Object[] { a.Id, a.EmpCode, a.FullNameFML, a.EffectiveDate, a.EndDate, a.PayScaleAgreement_Id, a.struct_Id }).ToList();
        //                }
        //                else if (gp.searchField == "EffectiveDate")
        //                {
        //                    jsonData = IE.Where(e => e.EffectiveDate.ToString().Contains(gp.searchString)
        //                        ).Select(a => new Object[] { a.Id, a.EmpCode, a.FullNameFML, a.EffectiveDate, a.EndDate, a.PayScaleAgreement_Id, a.struct_Id }).ToList();
        //                }
        //                else if (gp.searchField == "EndDate")
        //                {
        //                    jsonData = IE.Where(e => e.EndDate.ToString().Contains(gp.searchString)
        //                        ).Select(a => new Object[] { a.Id, a.EmpCode, a.FullNameFML, a.EffectiveDate, a.EndDate, a.PayScaleAgreement_Id, a.struct_Id }).ToList();
        //                }
        //                else if (gp.searchField == "PayScaleAgreement")
        //                {
        //                    jsonData = IE.Where(e => e.PayScaleAgreement_Id.ToString().Contains(gp.searchString)
        //                        ).Select(a => new Object[] { a.Id, a.EmpCode, a.FullNameFML, a.EffectiveDate, a.EndDate, a.PayScaleAgreement_Id, a.struct_Id }).ToList();
        //                }
        //                else if (gp.searchField == "struct_Id")
        //                {
        //                    jsonData = IE.Where(e => e.struct_Id.ToString().Contains(gp.searchString)
        //                        ).Select(a => new Object[] { a.Id, a.EmpCode, a.FullNameFML, a.EffectiveDate, a.EndDate, a.PayScaleAgreement_Id, a.struct_Id }).ToList();
        //                }


        //                //jsonData = IE.Where(e => (e.Id.ToString().Contains(gp.searchString))
        //                //    || (e.Employee.EmpCode.ToString().Contains(gp.searchString)) || (e.Employee.EmpName.FullNameFML.ToUpper().Contains(gp.searchString.ToUpper()))
        //                //    || (e.EffectiveDate.ToString().Contains(gp.searchString)) || (e.EndDate.ToString().Contains(gp.searchString))
        //                //    ||(e.struct_Id.ToString().Contains(gp.searchString)))
        //                //    .Select(a => new Object[] { a.Id, a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.EffectiveDate, a.EndDate, a.PayScaleAgreement_Id, a.struct_Id }).ToList();
        //                //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.EmpCode, a.FullNameFML, a.EffectiveDate, a.EndDate, a.PayScaleAgreement_Id, a.struct_Id
        //                }).ToList();
        //            }
        //            totalRecords = IE.Count();
        //        }
        //        else
        //        {
        //            IE = EmpSalStruct;
        //            Func<P2BGridData, dynamic> orderfuc;
        //            if (gp.sidx == "Id")
        //            {
        //                orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
        //            }
        //            else
        //            {
        //                orderfuc = (c => gp.sidx == "EmpCode" ? c.EmpCode.ToString() :
        //                                 gp.sidx == "EmpName" ? c.FullNameFML.ToString() :
        //                                 gp.sidx == "EffectiveDate" ? c.EffectiveDate.ToString() :
        //                                 gp.sidx == "EndDate" ? c.EndDate.ToString() :
        //                                 gp.sidx == "struct_Id" ? c.struct_Id.ToString() : "");
        //            }
        //            if (gp.sord == "asc")
        //            {
        //                IE = IE.OrderBy(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, a.EmpCode, a.FullNameFML, a.EffectiveDate, a.EndDate, a.PayScaleAgreement_Id, a.struct_Id }).ToList();
        //            }
        //            else if (gp.sord == "desc")
        //            {
        //                IE = IE.OrderByDescending(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, a.EmpCode, a.FullNameFML, a.EffectiveDate, a.EndDate, a.PayScaleAgreement_Id, a.struct_Id }).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.EmpCode, a.FullNameFML, a.EffectiveDate, a.EndDate, a.PayScaleAgreement_Id, a.struct_Id }).ToList();
        //            }
        //            totalRecords = EmpSalStruct.Count();
        //        }
        //        if (totalRecords > 0)
        //        {
        //            totalPages = (int)Math.Ceiling((float)totalRecords / (float)gp.rows);
        //        }
        //        if (gp.page > totalPages)
        //        {
        //            gp.page = totalPages;
        //        }
        //        var JsonData = new
        //        {
        //            page = gp.page,
        //            rows = jsonData,
        //            records = totalRecords,
        //            total = totalPages
        //        };
        //        return Json(JsonData, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        public class Employees
        {
            public int Id { get; set; }
            public int Employee_Id { get; set; }
            public string EmpCode { get; set; }
            public string EmpName_FullNameFML { get; set; }

        }
        public class tempClass
        {
            public DateTime? EffectiveDate { get; set; }
            public string Id { get; set; }
            public List<int> EmpSalStructDetails { get; set; }

            //public string EmpCode { get; set; }
            //public string EmpName_FullNameFML { get; set; }

        }

        public ActionResult P2BGrid(P2BGrid_Parameters gp)
        {
            try
            {
                // DataBaseContext db = new DataBaseContext();
                using (DataBaseContext db = new DataBaseContext())
                {


                    int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                    int pageSize = gp.rows;
                    int totalPages = 0;
                    int totalRecords = 0;
                    var jsonData = (Object)null;

                    IEnumerable<P2BGridData> EmpSalStruct = null;
                    List<P2BGridData> model = new List<P2BGridData>();
                    P2BGridData view = null;

                    //   db.Database.CommandTimeout = 300;
                    //List<Employees> OEmployee = new List<Employees>();
                    //using (DataBaseContext db1 = new DataBaseContext())
                    //{
                    //    db1.Configuration.AutoDetectChangesEnabled = false;
                    //    OEmployee = db1.EmployeePayroll
                    //        .Include(e => e.Employee)
                    //        .Include(e => e.Employee.EmpName)
                    //        .Where(e => e.Employee.ServiceBookDates != null && e.Employee.ServiceBookDates.ServiceLastDate == null)
                    //        .Select(e => new Employees
                    //        {
                    //            Id = e.Id,
                    //            Employee_Id = e.Employee.Id,
                    //            EmpCode = e.Employee.EmpCode,
                    //            EmpName_FullNameFML = e.Employee.EmpName.FullNameFML,
                    //        }).AsNoTracking().ToList();


                    //}

                    //var oemployeedata = db.EmpSalStruct.Where(e => e.EmployeePayroll.Employee.ServiceBookDates != null
                    //    && e.EmployeePayroll.Employee.ServiceBookDates.ServiceLastDate == null && e.EndDate == null)
                    //                     .Include(e => e.EmployeePayroll.Employee)
                    //                     .Include(e => e.EmployeePayroll.Employee.EmpName).OrderBy(e => e.EmployeePayroll.Employee.EmpCode).ToList();

                    var EmpSalStructdata = db.EmpSalStruct.Where(e => e.EndDate == null)
                        .Select(e => new
                        {
                            Id = e.EmployeePayroll.Employee_Id,
                            struct_Id = e.Id,
                            EmpCode = e.EmployeePayroll.Employee.EmpCode,
                            FullNameFML = e.EmployeePayroll.Employee.EmpName.FullNameFML,
                            EffectiveDate = e.EffectiveDate,
                            EndDate = ""
                        })
                        .OrderBy(e => e.Id).ToList();
                    //foreach (var i in EmpSalStructdata)
                    //{
                    //    var empsalobj = db.EmpSalStruct.Where(e => e.Id == i.Id).Select(e=>e.EmployeePayroll).FirstOrDefault();
                    //    i.EmployeePayroll.Employee = db.EmployeePayroll.Where(e => e.Id == empsalobj.Id).Select(r => r.Employee).FirstOrDefault();
                    //    i.EmployeePayroll.Employee.EmpName = db.EmployeePayroll.Where(e => e.Id == empsalobj.Id).Select(r => r.Employee.EmpName).FirstOrDefault();


                    //}
                    foreach (var item in EmpSalStructdata)
                    {
                        view = new P2BGridData()
                            {
                                Id = item.Id.ToString(),
                                struct_Id = item.struct_Id.ToString(),
                                EmpCode = item.EmpCode,
                                FullNameFML = item.FullNameFML,
                                //Employee = z.Employee,
                                EffectiveDate = item.EffectiveDate.Value.ToString("dd/MM/yyyy"),
                                EndDate = null,
                                //     PayScaleAgreement_Id = PayScaleAgr.Id
                            };

                        model.Add(view);
                    }


                    EmpSalStruct = model;

                    IEnumerable<P2BGridData> IE;
                    if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                    {
                        IE = EmpSalStruct;
                        if (gp.searchOper.Equals("eq"))
                        {
                            jsonData = IE.Where(e => (e.EmpCode.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                             || (e.FullNameFML.ToUpper().Contains(gp.searchString.ToUpper()))
                             || (e.EffectiveDate.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                             || (e.EndDate != null ? e.EndDate.ToUpper().ToString().Contains(gp.searchString.ToUpper()) : false)
                             || (e.struct_Id.ToString().Contains(gp.searchString))
                             || (e.Id.ToString().Contains(gp.searchString))
                             ).Select(a => new Object[] { a.EmpCode, a.FullNameFML, a.EffectiveDate, a.EndDate != null ? a.EndDate : "", a.struct_Id, a.Id }).ToList();



                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.EmpCode, a.FullNameFML, a.EffectiveDate, a.EndDate, 
                                a.struct_Id
                        }).ToList();
                        }
                        totalRecords = IE.Count();
                    }
                    else
                    {
                        IE = EmpSalStruct;
                        Func<P2BGridData, dynamic> orderfuc;
                        if (gp.sidx == "Id")
                        {
                            orderfuc = (c => gp.sidx == "Id" ? c.Id : "");
                        }
                        else
                        {
                            orderfuc = (c => gp.sidx == "EmpCode" ? c.EmpCode :
                                             gp.sidx == "EmpName" ? c.FullNameFML :
                                             gp.sidx == "EffectiveDate" ? c.EffectiveDate :
                                             gp.sidx == "EndDate" ? c.EndDate :
                                             gp.sidx == "struct_Id" ? c.struct_Id : "");
                        }
                        if (gp.sord == "asc")
                        {
                            IE = IE.OrderBy(orderfuc);
                            jsonData = IE.Select(a => new Object[] { a.EmpCode, a.FullNameFML, a.EffectiveDate, a.EndDate, a.struct_Id, a.Id }).ToList();
                        }
                        else if (gp.sord == "desc")
                        {
                            IE = IE.OrderByDescending(orderfuc);
                            jsonData = IE.Select(a => new Object[] { a.EmpCode, a.FullNameFML, a.EffectiveDate, a.EndDate, a.struct_Id, a.Id }).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.EmpCode, a.FullNameFML, a.EffectiveDate, a.EndDate, a.struct_Id, a.Id }).ToList();
                        }
                        totalRecords = EmpSalStruct.Count();
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
            }
            catch (Exception ex)
            {
                throw ex;
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
                        jsonData = IE.Where(e => (e.Code.ToString().Contains(gp.searchString))
                               || (e.Name.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                               || (e.Id.ToString().Contains(gp.searchString))
                               ).Select(a => new Object[] { a.Code, a.Name, a.Id }).ToList();


                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Code, a.Name, a.Id }).ToList();
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
                        orderfuc = (c => gp.sidx == "EmpCode" ? c.Code :
                                         gp.sidx == "EmpName" ? c.Name : "");


                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Code, a.Name, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Code, a.Name, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Code, a.Name, a.Id }).ToList();
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
            try
            {
                using (DataBaseContext db = new DataBaseContext())
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        try
                        {
                            var Emp = db.EmployeePayroll.Include(e => e.EmpSalStruct).Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails))
                                .Where(e => e.Employee.Id == data).SingleOrDefault();

                            EmpSalStruct empsalstruct = Emp.EmpSalStruct.Where(e => e.EndDate == null).SingleOrDefault();
                            if (empsalstruct != null)
                            {

                                SalaryT OSalT = db.SalaryT.Where(e => e.EmployeePayroll_Id == Emp.Id).FirstOrDefault();
                                if (OSalT == null)
                                {
                                    CPIEntryT OCPI = db.CPIEntryT.Where(e => e.EmployeePayroll_Id == Emp.Id).FirstOrDefault();
                                    if (OCPI != null)
                                    {
                                        db.Entry(OCPI).State = System.Data.Entity.EntityState.Deleted;
                                        db.EmpSalStructDetails.RemoveRange(empsalstruct.EmpSalStructDetails);
                                        db.EmpSalStruct.Remove(empsalstruct);
                                    }
                                    else
                                    {
                                        db.EmpSalStructDetails.RemoveRange(empsalstruct.EmpSalStructDetails);
                                        db.EmpSalStruct.Remove(empsalstruct);
                                    }

                                    await db.SaveChangesAsync();
                                }
                                else
                                {
                                    Msg.Add("You cannot deletethis record as salary for this employee is generated.");
                                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }
                            }

                            ts.Complete();
                            Msg.Add("  Data removed.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }
                        catch (RetryLimitExceededException /* dex */)
                        {
                            return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
                        }
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

        public ActionResult LoadEmp(P2BGrid_Parameters gp)
        {
            List<string> Msg = new List<string>();
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
                    db.Database.CommandTimeout = 300;

                    var uids = db.EmployeePayroll.ToList();
                    foreach (var uidsitem in uids)
                    {
                        Employee Employee1 = db.Employee.Where(e => e.Id == uidsitem.Employee_Id).SingleOrDefault();
                        uidsitem.Employee = Employee1;
                        ServiceBookDates ServiceBookDates = db.ServiceBookDates.Where(e => e.Id == Employee1.ServiceBookDates_Id).SingleOrDefault();
                        Employee1.ServiceBookDates = ServiceBookDates;
                        List<EmpSalStruct> EmpSalStruct = db.EmpSalStruct.Where(e => e.EmployeePayroll_Id == uidsitem.Id).ToList();
                        uidsitem.EmpSalStruct = EmpSalStruct;


                    }

                    //.Include(e => e.EmpSalStruct)
                    //.Include(e => e.Employee)
                    //.Include(e => e.Employee.ServiceBookDates)
                    //.AsNoTracking()
                    ////.Select(e => e.Employee.Id)

                    var muids1 = uids.Where(e => e.EmpSalStruct.Count() == 0).ToList();
                    if (muids1 != null && muids1.Count() > 0)
                    {
                        var mUids = muids1.Where(e => e.Employee.ServiceBookDates != null && e.Employee.ServiceBookDates.ServiceLastDate == null).Select(e => e.Employee.Id).ToList();
                        Employee = db.Employee.Include(e => e.EmpName).Where(t => mUids.Contains(t.Id));
                    }
                    else
                    {
                        Employee = null;
                        Msg.Add(" Employee Null  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        // return this.Json(new Object[] { "", "", " ", JsonRequestBehavior.AllowGet });
                    }
                    //Employee = db.Employee.Include(e => e.EmpName).ToList();
                }

                IEnumerable<Employee> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = Employee;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.EmpCode.ToString().Contains(gp.searchString))
                               || (e.EmpName.FullNameFML.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                               || (e.Id.ToString().Contains(gp.searchString))
                               ).Select(a => new Object[] { a.EmpCode, a.EmpName.FullNameFML, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.EmpCode, a.EmpName.FullNameFML, a.Id }).ToList();
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
                        orderfuc = (c =>
                                         gp.sidx == "EmpCode" ? c.EmpCode.ToString() :
                                         gp.sidx == "EmpName" ? c.EmpName.FullNameFML.ToString() : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.EmpCode), a.EmpName != null ? a.EmpName.FullNameFML : "", a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.EmpCode), a.EmpName != null ? a.EmpName.FullNameFML : "", a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.EmpCode), a.EmpName != null ? a.EmpName.FullNameFML : "", a.Id }).ToList();
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
                throw ex;
            }
        }

    }
}