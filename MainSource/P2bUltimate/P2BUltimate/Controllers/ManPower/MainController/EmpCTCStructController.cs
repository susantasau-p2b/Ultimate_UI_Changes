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
using P2BUltimate.Security;
using Recruitment;
using Payroll;
using P2BUltimate.Process;
using System.Diagnostics;

namespace P2BUltimate.Controllers.ManPower.MainController
{
    public class EmpCTCStructController : Controller
    {
        //
        // GET: /EmpCTCStruct/
        public ActionResult Index()
        {
            return View("~/Views/ManPower/MainViews/EmpCTCStruct/Index.cshtml");
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
        {  List<string> Msg = new List<string>();
					try{
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
                            SalaryHeadGenProcess.EmployeeSalaryStructCreation(OEmployeePayroll, OEmployee, OPayScalAgreement, Convert.ToDateTime(Effective_date));
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
              //  return Json(new { success = true, responseText = "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
                Msg.Add("  Data Saved successfully  ");
                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);


                return View();
                //string PayDays = form["PayDays"] == "0" ? "" : form["PayDays"];
                //string EffectiveDate = form["EffectiveDate"] == "0" ? "" : form["EffectiveDate"];
                //string EmpSalStructDetails = form["EmpSalStructDetailsList"] == "0" ? "" : form["EmpSalStructDetailsList"];
                //string EndDate = form["EndDate"] == "0" ? "" : form["EndDate"];

                //if (PayDays != null)
                //{
                //    if (PayDays != "")
                //    {
                //        var val = double.Parse(PayDays);
                //        EmpSalStruct.PayDays = val;
                //    }
                //}

                //if (EffectiveDate != null)
                //{
                //    if (EffectiveDate != "")
                //    {
                //        var val = DateTime.Parse(EffectiveDate);

                //        EmpSalStruct.EffectiveDate = val;
                //    }
                //}

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
                //if (EndDate != null)
                //{
                //    if (EndDate != "")
                //    {
                //        var val = DateTime.Parse(EndDate);

                //        EmpSalStruct.EndDate = val;
                //    }
                //}

                //if (ModelState.IsValid)
                //{
                //    using (TransactionScope ts = new TransactionScope())
                //    {


                //        EmpSalStruct.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                //        EmpSalStruct E = new EmpSalStruct()
                //        {
                //            PayDays = EmpSalStruct.PayDays,
                //            EffectiveDate = EmpSalStruct.EffectiveDate,
                //            EmpSalStructDetails = EmpSalStruct.EmpSalStructDetails,
                //            EndDate = EmpSalStruct.EndDate,
                //            DBTrack = EmpSalStruct.DBTrack
                //        };
                //        try
                //        {
                //            db.EmpSalStruct.Add(E);
                //            db.SaveChanges();


                //            ts.Complete();
                //            return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                //        }
                //        catch (DbUpdateConcurrencyException)
                //        {
                //            return RedirectToAction("Create", new { concurrencyError = true, id = EmpSalStruct.Id });
                //        }
                //        catch (DataException /* dex */)
                //        {
                //            return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
                //        }
                //    }
                //}
                //else
                //{
                //    StringBuilder sb = new StringBuilder("");
                //    foreach (ModelState modelState in ModelState.Values)
                //    {
                //        foreach (ModelError error in modelState.Errors)
                //        {
                //            sb.Append(error.ErrorMessage);
                //            sb.Append("." + "\n");
                //        }
                //    }
                //    var errorMsg = sb.ToString();
                //    return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
                //    //return this.Json(new { msg = errorMsg });
                //}

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
        { List<string> Msg = new List<string>();
					try{
            using (DataBaseContext db = new DataBaseContext())
            {
                var OPayScalAgreement = db.PayScaleAgreement.Where(e => e.Id == PayScaleAgreementId).SingleOrDefault();


                var EmpList = db.EmployeePayroll.Include(e => e.Employee.GeoStruct)
                            .Include(e => e.Employee.PayStruct)
                            .Include(e => e.Employee.FuncStruct).Include(e => e.EmpSalStruct)
                            .Include(e => e.Employee.EmpOffInfo)
                            .Include(e => e.Employee.EmpOffInfo.PayScale)
                    //.Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails))
                    //.Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.PayScaleAssignment)))
                    //.Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.PayScaleAssignment.SalHeadFormula)))//added by prashant 14042017
                    //.Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.PayScaleAssignment.PayScaleAgreemnt)))
                    //.Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead)))
                    //.Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.SalHeadOperationType)))
                    //.Include(e => e.CPIEntryT)
                            .ToList();




                foreach (var a in EmpList)
                {
                    var EmpSalStruct = db.EmployeePayroll.Include(e => e.EmpSalStruct)
                                   .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails))
                                    .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.PayScaleAssignment)))
                            .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.PayScaleAssignment.SalHeadFormula)))//added by prashant 14042017
                            .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.PayScaleAssignment.PayScaleAgreement)))
                            .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead)))
                            .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.SalHeadOperationType)))
                            .Include(e => e.CPIEntryT).Where(e => e.Id == a.Id).SingleOrDefault();

                    var SalStruct = EmpSalStruct.EmpSalStruct.Where(r => r.EndDate == null).ToList();
                    foreach (var b in SalStruct)
                    {
                        var OEmpSalStructDet = b.EmpSalStructDetails.Select(r => r.PayScaleAssignment.PayScaleAgreement.Id == PayScaleAgreementId).ToList();
                        if (OEmpSalStructDet.Count > 0)
                        {
                            Employee OEmployee = db.Employee
                                               .Include(e => e.GeoStruct)
                                               .Include(e => e.FuncStruct)
                                               .Include(e => e.PayStruct)
                                                .Where(e => e.Id == a.Employee.Id)
                                                .SingleOrDefault();

                            using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                          new System.TimeSpan(0, 30, 0)))
                            {
                                try
                                {
                                    SalaryHeadGenProcess.EmployeeSalaryStructCreationWithUpdation(b.Id, OEmployee, OPayScalAgreement, Convert.ToDateTime(b.EffectiveDate), a);
                                    //db.RefreshAllEntites(RefreshMode.StoreWins);//added by prashant 14042017
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
                                       // LineNo = Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
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
              List<string> Msg = new List<string>();
					try{
            using (DataBaseContext db = new DataBaseContext())
            {
                var OPayScalAgreement = db.PayScaleAgreement.Where(e => e.Id == PayScaleAgreementId).SingleOrDefault();


                var EmpList = db.EmployeePayroll.Include(e => e.Employee.GeoStruct)
                            .Include(e => e.Employee.PayStruct)
                            .Include(e => e.Employee.FuncStruct).Include(e => e.EmpSalStruct)
                            .Include(e => e.Employee.EmpOffInfo)
                            .Include(e => e.Employee.EmpOffInfo.PayScale)
                            .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails))
                            .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.PayScaleAssignment)))
                            .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.PayScaleAssignment.SalHeadFormula)))//added by prashant 14042017
                            .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.PayScaleAssignment.PayScaleAgreement)))
                            .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead)))
                            .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.SalHeadOperationType)))
                            .Include(e => e.CPIEntryT)
                            .ToList();

                foreach (var a in EmpList)
                {
                    var SalStruct = a.EmpSalStruct.Where(r => r.EndDate == null).ToList();
                    foreach (var b in SalStruct)
                    {
                        var OEmpSalStructDet = b.EmpSalStructDetails.Select(r => r.PayScaleAssignment.PayScaleAgreement.Id == PayScaleAgreementId).ToList();
                        if (OEmpSalStructDet.Count > 0)
                        {
                            Employee OEmployee = db.Employee
                                               .Include(e => e.GeoStruct)
                                               .Include(e => e.FuncStruct)
                                               .Include(e => e.PayStruct)
                                                .Where(e => e.Id == a.Employee.Id)
                                                .SingleOrDefault();

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

              //  return Json(new { success = true, responseText = "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
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
        public ActionResult EditSave(EmpSalStruct EmpSalStruct, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
					try{
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
                        // db.RefreshAllEntites(RefreshMode.StoreWins);
                        ts.Complete();
                      //  return Json(new { success = true, responseText = "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
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
                        return Json(new { success = false, responseText = ex.InnerException }, JsonRequestBehavior.AllowGet);
                    }
                }
                return View();
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
        }

        public class DeserializeClass
        {
            public String Id { get; set; }
            public String Amount { get; set; }

        }

        //public class P2BGridData
        //{
        //    public int Id { get; set; }

        //    public int struct_Id { get; set; }
        //    public Employee Employee { get; set; }
        //    public string EffectiveDate { get; set; }
        //    public DateTime? EndDate { get; set; }
        //    public int PayScaleAgreement_Id { get; set; }
        //}
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
                var a = db.PayScaleAgreement.Find(int.Parse(data));
                return Json(a, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult process(string forwarddata, FormCollection form, String selected)
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
                                      new System.TimeSpan(0, 30, 0)))
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
                        .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.PayScaleAssignment.SalHeadFormula)))//added by prashant 14042017
                        .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.SalHeadOperationType)))
                        .Where(e => e.Employee.Id == EmpId).SingleOrDefault();

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


                var OEmployeeSalStruct = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails))
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
                                        .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead)))
                                        .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.SalHeadOperationType)))
                                        .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.Frequency)))
                                        .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.Type)))
                    //                    .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.PayScaleAssignment.SalHeadFormula)))
                    ////.Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.OrderBy(t => t.SalaryHead.SeqNo)))
                                        .Where(e => e.Employee.Id == EmpId)
                                       .SingleOrDefault();
                var id = Convert.ToInt32(gp.filter);
                var OEmpSalStruct = OEmployeeSalStruct.EmpSalStruct.Where(e => e.Id == id);

                foreach (var x in OEmpSalStruct)
                {

                    var OEmpSalStructDet = x.EmpSalStructDetails;
                    foreach (var SalForAppl in OEmpSalStructDet)
                    {
                        var m = db.EmpSalStructDetails.Include(e => e.SalaryHead).Include(e => e.SalHeadFormula).Where(e => e.Id == SalForAppl.Id).SingleOrDefault();


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


                        if (SalHeadForm != null)
                        {
                            if (SalForAppl.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() == "BASIC")
                                EditAppl = true;
                            else
                                EditAppl = false;
                        }
                        else
                        {
                            if (SalForAppl.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() == "EPF" || SalForAppl.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() == "PT" ||
                                SalForAppl.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() == "ITAX" || SalForAppl.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() == "LWF" ||
                                SalForAppl.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() == "ESIC" || SalForAppl.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() == "CPF" ||
                                SalForAppl.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() == "PENSION")
                                EditAppl = false;
                            else
                                EditAppl = true;
                        }
                        if (SalForAppl.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() != "LOAN")
                        {
                            view = new EditData()
                            {
                                Id = SalForAppl.Id,
                                SalaryHead = SalForAppl.SalaryHead,
                                Amount = SalForAppl.Amount,
                                Editable = EditAppl
                            };

                            model.Add(view);
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

                        jsonData = IE.Where(e => (e.SalaryHead.Name.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                               || (e.Amount.ToString().Contains(gp.searchString.ToUpper()))
                               || (e.SalaryHead.Frequency.LookupVal.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                               || (e.SalaryHead.Type.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                               || (e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                               || (e.Editable.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                               || (e.Id.ToString().Contains(gp.searchString))
                               ).Select(a => new Object[] { a.SalaryHead.Name, a.Amount, a.SalaryHead.Frequency.LookupVal, a.SalaryHead.Type.LookupVal, a.SalaryHead.SalHeadOperationType.LookupVal, a.Editable, a.Id }).ToList();

                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.SalaryHead.Name, a.Amount, a.SalaryHead.Frequency.LookupVal, a.SalaryHead.Type.LookupVal, a.SalaryHead.SalHeadOperationType.LookupVal, a.Editable, a.Id }).ToList();
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
                                         gp.sidx == "Editable" ? c.Editable.ToString() : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.SalaryHead.Name, a.Amount, a.SalaryHead.Frequency != null ? Convert.ToString(a.SalaryHead.Frequency.LookupVal) : "", a.SalaryHead.Type != null ? Convert.ToString(a.SalaryHead.Type.LookupVal) : "", a.SalaryHead.SalHeadOperationType != null ? Convert.ToString(a.SalaryHead.SalHeadOperationType.LookupVal) : "", a.Editable, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.SalaryHead.Name, a.Amount, a.SalaryHead.Frequency != null ? Convert.ToString(a.SalaryHead.Frequency.LookupVal) : "", a.SalaryHead.Type != null ? Convert.ToString(a.SalaryHead.Type.LookupVal) : "", a.SalaryHead.SalHeadOperationType != null ? Convert.ToString(a.SalaryHead.SalHeadOperationType.LookupVal) : "", a.Editable, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] {  a.SalaryHead.Name, a.Amount, a.SalaryHead.Frequency != null ? Convert.ToString(a.SalaryHead.Frequency.LookupVal) : "", a.SalaryHead.Type != null ? Convert.ToString(a.SalaryHead.Type.LookupVal) : "", a.SalaryHead.SalHeadOperationType != null ? Convert.ToString(a.SalaryHead.SalHeadOperationType.LookupVal) : "", a.Editable, a.Id }).ToList();
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
        //public ActionResult P2BInlineGrid(P2BGrid_Parameters gp)
        //{
        //	try
        //	{
        //		DataBaseContext db = new DataBaseContext();
        //		int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
        //		int pageSize = gp.rows;
        //		int totalPages = 0;
        //		int totalRecords = 0;
        //		var jsonData = (Object)null;
        //		IEnumerable<EditData> EmpSalStruct = null;

        //		List<EditData> model = new List<EditData>();

        //		var OEmployeePayroll = db.EmployeePayroll
        //			.SelectMany(o => o.EmpSalStruct.SelectMany(g => g.EmpSalStructDetails)).ToList();

        //	   var view = new EditData();

        //		foreach (var z in OEmployeePayroll)
        //		{
        //			var m = db.EmpSalStructDetails.Include(e => e.SalaryHead).Where(e => e.Id == z.Id).SingleOrDefault();
        //			bool EditAppl = true ;
        //			var SalForAppl = db.PayScaleAssignment.Include(e => e.SalaryHead).Include(e => e.SalHeadFormula)
        //				.Include(e => e.SalaryHead.SalHeadOperationType)
        //				.Include(e => e.SalaryHead.ProcessType)
        //				.Include(e => e.SalaryHead.Type)
        //				.Include(e => e.SalaryHead.Frequency)
        //				.Where(e => e.SalaryHead.Id == m.SalaryHead.Id).SingleOrDefault();
        //			if (SalForAppl.SalHeadFormula.Count > 0)
        //			{
        //				if (SalForAppl.SalaryHead.SalHeadOperationType.LookupVal == "BASIC")
        //					EditAppl = false;
        //				else
        //					EditAppl = true;
        //			}
        //			else
        //			{
        //				if (SalForAppl.SalaryHead.SalHeadOperationType.LookupVal == "PF" || SalForAppl.SalaryHead.SalHeadOperationType.LookupVal == "PT" ||
        //					SalForAppl.SalaryHead.SalHeadOperationType.LookupVal == "ITax" || SalForAppl.SalaryHead.SalHeadOperationType.LookupVal == "LWF" ||
        //					SalForAppl.SalaryHead.SalHeadOperationType.LookupVal == "ESIC" || SalForAppl.SalaryHead.SalHeadOperationType.LookupVal == "CPF" ||
        //					SalForAppl.SalaryHead.SalHeadOperationType.LookupVal == "Pension" || SalForAppl.SalaryHead.SalHeadOperationType.LookupVal == "Basic")
        //					EditAppl = false;
        //				else
        //					EditAppl = false;
        //			}
        //			view = new EditData()
        //			{
        //			   Id = z.Id,
        //			   SalaryHead = z.SalaryHead,
        //			   Amount = z.Amount,
        //			   Editable = EditAppl
        //			};

        //			model.Add(view);
        //		}

        //		EmpSalStruct = model;

        //		IEnumerable<EditData> IE;
        //		if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
        //		{
        //			IE = EmpSalStruct;
        //			if (gp.searchOper.Equals("eq"))
        //			{
        //				//if (gp.searchField == "Id")
        //				//    jsonData = IE.Select(a => new Object[] { a.Id, a.SalaryHead.Name, a.Amount, a.SalaryHead.Frequency.LookupVal, a.SalaryHead.Type.LookupVal, a.SalaryHead.SalHeadOperationType, a.Editable }).Where((e => (e.Contains(gp.searchString)))).ToList();
        //				//else if (gp.searchField == "SalaryHead")
        //				//    jsonData = IE.Select(a => new Object[] { a.Id, a.SalaryHead.Name, a.Amount, a.SalaryHead.Frequency, a.SalaryHead.Type, a.SalaryHead.SalHeadOperationType, a.Editable }).Where((e => (e.Contains(gp.searchString)))).ToList();
        //				//else if (gp.searchField == "Amount")
        //				//    jsonData = IE.Select(a => new Object[] { a.Id, a.SalaryHead.Name, a.Amount, a.SalaryHead.Frequency, a.SalaryHead.Type, a.SalaryHead.SalHeadOperationType, a.Editable }).Where((e => (e.Contains(gp.searchString)))).ToList();
        //				//else if (gp.searchField == "Frequency")
        //				//    jsonData = IE.Select(a => new Object[] { a.Id, a.SalaryHead.Name, a.Amount, a.SalaryHead.Frequency, a.SalaryHead.Type, a.SalaryHead.SalHeadOperationType, a.Editable }).Where((e => (e.Contains(gp.searchString)))).ToList();
        //				//else if (gp.searchField == "Type")
        //				//    jsonData = IE.Select(a => new Object[] { a.Id, a.SalaryHead.Name, a.Amount, a.SalaryHead.Frequency, a.SalaryHead.Type, a.SalaryHead.SalHeadOperationType, a.Editable }).Where((e => (e.Contains(gp.searchString)))).ToList();
        //				//else if (gp.searchField == "SalHeadOperationType")
        //				//    jsonData = IE.Select(a => new Object[] { a.Id, a.SalaryHead.Name, a.Amount, a.SalaryHead.Frequency, a.SalaryHead.Type, a.SalaryHead.SalHeadOperationType, a.Editable }).Where((e => (e.Contains(gp.searchString)))).ToList();

        //				//jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();

        //				jsonData = IE.Select(a => new Object[] { a.Id, a.SalaryHead.Name, a.Amount, a.SalaryHead.Frequency.LookupVal, a.SalaryHead.Type.LookupVal, a.SalaryHead.SalHeadOperationType.LookupVal, a.Editable }).Where((e => (e.Contains(gp.searchString)))).ToList();
        //			}
        //			if (pageIndex > 1)
        //			{
        //				int h = pageIndex * pageSize;
        //				jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.SalaryHead.Name, a.Amount, a.SalaryHead.Frequency.LookupVal, a.SalaryHead.Type.LookupVal, a.SalaryHead.SalHeadOperationType.LookupVal, a.Editable }).ToList();
        //			}
        //			totalRecords = IE.Count();
        //		}
        //		else
        //		{
        //			IE = EmpSalStruct;
        //			Func<EditData, dynamic> orderfuc;
        //			if (gp.sidx == "Id")
        //			{
        //				orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
        //			}
        //			else
        //			{
        //				orderfuc = (c => gp.sidx == "SalaryHead" ? c.SalaryHead.Name.ToString() :
        //								 gp.sidx == "Amount" ? c.Amount.ToString() :
        //								 gp.sidx == "Frequency" ? c.SalaryHead.Frequency.LookupVal.ToString() :
        //								 gp.sidx == "Type" ? c.SalaryHead.Type.LookupVal.ToString() :
        //								 gp.sidx == "SalHeadOperationType" ? c.SalaryHead.SalHeadOperationType.LookupVal.ToString() :
        //								 gp.sidx == "Editable" ? c.Editable.ToString() : "");
        //			}
        //			if (gp.sord == "asc")
        //			{
        //				IE = IE.OrderBy(orderfuc);
        //				jsonData = IE.Select(a => new Object[] { a.Id, a.SalaryHead.Name, a.Amount, a.SalaryHead.Frequency != null ? Convert.ToString(a.SalaryHead.Frequency.LookupVal) : "", a.SalaryHead.Type != null ? Convert.ToString(a.SalaryHead.Type.LookupVal) : "", a.SalaryHead.SalHeadOperationType != null ? Convert.ToString(a.SalaryHead.SalHeadOperationType.LookupVal) : "", a.Editable }).ToList();
        //			}
        //			else if (gp.sord == "desc")
        //			{
        //				IE = IE.OrderByDescending(orderfuc);
        //				jsonData = IE.Select(a => new Object[] { a.Id, a.SalaryHead.Name, a.Amount, a.SalaryHead.Frequency != null ? Convert.ToString(a.SalaryHead.Frequency.LookupVal) : "", a.SalaryHead.Type != null ? Convert.ToString(a.SalaryHead.Type.LookupVal) : "", a.SalaryHead.SalHeadOperationType != null ? Convert.ToString(a.SalaryHead.SalHeadOperationType.LookupVal) : "", a.Editable }).ToList();
        //			}
        //			if (pageIndex > 1)
        //			{
        //				int h = pageIndex * pageSize;
        //				jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.SalaryHead.Name, a.Amount }).ToList();
        //			}
        //			totalRecords = EmpSalStruct.Count();
        //		}
        //		if (totalRecords > 0)
        //		{
        //			totalPages = (int)Math.Ceiling((float)totalRecords / (float)gp.rows);
        //		}
        //		if (gp.page > totalPages)
        //		{
        //			gp.page = totalPages;
        //		}
        //		var JsonData = new
        //		{
        //			page = gp.page,
        //			rows = jsonData,
        //			records = totalRecords,
        //			total = totalPages
        //		};
        //		return Json(JsonData, JsonRequestBehavior.AllowGet);
        //	}
        //	catch (Exception ex)
        //	{
        //		throw ex;
        //	}
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

        //        db.Database.CommandTimeout = 300;

        //        var OEmployee = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.Employee.EmpName).ToList();

        //        foreach (var z in OEmployee)
        //        {
        //            var OEmpSalStruct = db.EmployeePayroll.Where(e => e.Id == z.Id).Select(e => e.EmpSalStruct.Where(r => r.EndDate == null))
        //                                .SingleOrDefault();


        //            DateTime? Eff_Date = null;
        //            int EmpStruct_Id = 0;

        //            PayScaleAgreement PayScaleAgr = null;
        //            foreach (var a in OEmpSalStruct)
        //            {
        //                Eff_Date = a.EffectiveDate;
        //                EmpStruct_Id = a.Id;
        //                var aa = db.EmpSalStruct.Where(e => e.Id == a.Id).Include(e => e.EmpSalStructDetails.Select(r => r.PayScaleAssignment.PayScaleAgreement)).SingleOrDefault();
        //                var EmpSalStructDet = aa.EmpSalStructDetails;
        //                foreach (var q in EmpSalStructDet)
        //                {
        //                    PayScaleAgr = q.PayScaleAssignment.PayScaleAgreement;
        //                    break;
        //                }
        //                break;
        //            }
        //            if (Eff_Date != null)
        //            {
        //                view = new P2BGridData()
        //                {
        //                    Id = z.Employee.Id,
        //                    struct_Id = EmpStruct_Id,
        //                    Employee = z.Employee,
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
        //                    jsonData = IE.Select(a => new { a.Id, a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.EffectiveDate, a.EndDate, a.PayScaleAgreement_Id, a.struct_Id }).Where((e => (e.Id.ToString().Contains(gp.searchString)))).ToList();
        //                if (gp.searchField == "EmpCode")
        //                    jsonData = IE.Select(a => new { a.Id, a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.EffectiveDate, a.EndDate, a.PayScaleAgreement_Id, a.struct_Id }).Where((e => (e.EmpCode.ToString().Contains(gp.searchString)))).ToList();
        //                if (gp.searchField == "EmpName")
        //                    jsonData = IE.Select(a => new { a.Id, a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.EffectiveDate, a.EndDate, a.PayScaleAgreement_Id, a.struct_Id }).Where((e => (e.FullNameFML.ToString().Contains(gp.searchString)))).ToList();
        //                else if (gp.searchField == "EffectiveDate")
        //                    jsonData = IE.Select(a => new { a.Id, a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.EffectiveDate, a.EndDate, a.PayScaleAgreement_Id, a.struct_Id }).Where((e => (e.EffectiveDate.ToString().Contains(gp.searchString)))).ToList();
        //                else if (gp.searchField == "EndDate")
        //                    jsonData = IE.Select(a => new { a.Id, a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.EffectiveDate, a.EndDate, a.PayScaleAgreement_Id, a.struct_Id }).Where((e => (e.EndDate.ToString().Contains(gp.searchString)))).ToList();

        //                //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.EffectiveDate, a.EndDate, a.PayScaleAgreement_Id, a.struct_Id
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
        //                orderfuc = (c => gp.sidx == "EmpCode" ? c.Employee.EmpCode.ToString() :
        //                                 gp.sidx == "EmpName" ? c.Employee.EmpName.FullNameFML.ToString() :
        //                                 gp.sidx == "EffectiveDate" ? c.EffectiveDate.ToString() :
        //                                 gp.sidx == "EndDate" ? c.EndDate.ToString() :
        //                                 gp.sidx == "struct_Id" ? c.struct_Id.ToString() : "");
        //            }
        //            if (gp.sord == "asc")
        //            {
        //                IE = IE.OrderBy(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.EffectiveDate, a.EndDate, a.PayScaleAgreement_Id, a.struct_Id }).ToList();
        //            }
        //            else if (gp.sord == "desc")
        //            {
        //                IE = IE.OrderByDescending(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.EffectiveDate, a.EndDate, a.PayScaleAgreement_Id, a.struct_Id }).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.EffectiveDate, a.EndDate, a.PayScaleAgreement_Id, a.struct_Id }).ToList();
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
                    List<Employees> OEmployee = new List<Employees>();
                    using (DataBaseContext db1 = new DataBaseContext())
                    {
                        db1.Configuration.AutoDetectChangesEnabled = false;
                        OEmployee = db1.EmployeePayroll.Include(e => e.Employee).Include(e => e.Employee.EmpName)
                            .Select(e => new Employees
                            {
                                Id = e.Id,
                                Employee_Id = e.Employee.Id,
                                EmpCode = e.Employee.EmpCode,
                                EmpName_FullNameFML = e.Employee.EmpName.FullNameFML,
                            }).AsNoTracking().AsParallel().ToList();


                    }
                    foreach (Employees z in OEmployee)
                    {
                        List<int> OEmpSalStruct = new List<int>();
                        using (DataBaseContext db1 = new DataBaseContext())
                        {
                            db1.Configuration.AutoDetectChangesEnabled = false;
                            OEmpSalStruct.AddRange(db1.EmployeePayroll.Where(e => e.Id == z.Id).SelectMany(a => a.EmpSalStruct)
                                .Where(a => a.EndDate == null).Select(a => a.Id).AsParallel().ToList());

                            //         .SelectMany(a =>a.EmpSalStruct.
                            //         //a.EmpSalStruct.
                            //         e.EmpSalStruct.Where(r => r.EndDate == null)
                            //.Select(a => a.Id)).AsNoTracking().AsParallel()
                            //                .SingleOrDefault();

                            //     OEmpSalStruct.Add(db.EmployeePayroll.Where(e => e.Id == z.Id).Select(e => e.EmpSalStruct.Where(r => r.EndDate == null)
                            //.Select(a => a.Id)).AsNoTracking().AsParallel()
                            //                .SingleOrDefault());

                        }

                        //DateTime? Eff_Date = null;
                        //int EmpStruct_Id = 0;

                        //PayScaleAgreement PayScaleAgr = null;
                        //foreach (var a in OEmpSalStruct)
                        //{
                        //    var aa = db.EmpSalStruct.Where(e => e.Id == a).Include(e => e.EmpSalStructDetails.Select(r => r.PayScaleAssignment.PayScaleAgreement)).SingleOrDefault();
                        //    Eff_Date = aa.EffectiveDate;
                        //    EmpStruct_Id = aa.Id;
                        //    var EmpSalStructDet = aa.EmpSalStructDetails;
                        //    if (aa.EmpSalStructDetails.Count == 0)
                        //    {
                        //        Eff_Date = null;
                        //    }
                        //    foreach (var q in EmpSalStructDet)
                        //    {
                        //        PayScaleAgr = q.PayScaleAssignment.PayScaleAgreement;
                        //        break;
                        //    }
                        //    break;
                        //}
                        string Eff_Date = "";
                        string EmpStruct_Id = "";

                        //PayScaleAgreement PayScaleAgr = null;



                        //List<EmpSalStructDetails> EmpSalStructDet = new List<EmpSalStructDetails>();
                        Parallel.ForEach(OEmpSalStruct, (x) =>
                        {
                            using (DataBaseContext db1 = new DataBaseContext())
                            {
                                db1.Configuration.AutoDetectChangesEnabled = false;

                                tempClass aa = db1.EmpSalStruct.Where(e => e.Id == x)
                                    //.Include(e => e.EmpSalStructDetails
                                    // .Select(r => r.PayScaleAssignment.PayScaleAgreement)
                                    //)
                                    .Select(e => new tempClass
                                    {

                                        EffectiveDate = e.EffectiveDate,
                                        Id = e.Id.ToString(),
                                        EmpSalStructDetails = e.EmpSalStructDetails.Select(a => a.Id).ToList()
                                    })
                                    .AsParallel().SingleOrDefault();
                                Eff_Date = aa.EffectiveDate.Value.ToString("dd/MM/yyyy");
                                EmpStruct_Id = aa.Id;
                                // EmpSalStructDet = aa.EmpSalStructDetails.ToList();
                                if (aa.EmpSalStructDetails.Count() == 0)
                                {
                                    Eff_Date = "";
                                }

                                //Parallel.ForEach(aa.EmpSalStructDetails, (y) =>
                                //{
                                //    PayScaleAgr = y.PayScaleAssignment.PayScaleAgreement;

                                //});
                            }

                        });
                        if (Eff_Date != "")
                        {
                            view = new P2BGridData()
                            {
                                Id = z.Employee_Id.ToString(),
                                struct_Id = EmpStruct_Id,
                                EmpCode = z.EmpCode,
                                FullNameFML = z.EmpName_FullNameFML,
                                //Employee = z.Employee,
                                EffectiveDate = Eff_Date,
                                EndDate = null,
                                //     PayScaleAgreement_Id = PayScaleAgr.Id
                            };

                            model.Add(view);
                        }


                    }

                    EmpSalStruct = model;

                    IEnumerable<P2BGridData> IE;
                    if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                    {
                        IE = EmpSalStruct;
                        if (gp.searchOper.Equals("eq"))
                        {
                            
                            jsonData = IE.Where(e => (e.EmpCode.ToString().Contains(gp.searchString))
                               || (e.FullNameFML.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                               || (e.EffectiveDate.ToString().Contains(gp.searchString))
                               || (e.EndDate != null ? e.EndDate.ToUpper().ToString().Contains(gp.searchString.ToUpper()) : false)
                               || (e.struct_Id.ToString().Contains(gp.searchString))
                               || (e.Id.ToString().Contains(gp.searchString))
                               ).Select(a => new Object[] { a.EmpCode, a.FullNameFML, a.EffectiveDate, a.EndDate != null ? a.EndDate : "", a.struct_Id, a.Id }).ToList();                     
                    }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.EmpCode, a.FullNameFML, a.EffectiveDate, a.EndDate, a.struct_Id, a.Id }).ToList();
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
        //        IEnumerable<EmpSalStruct> EmpSalStruct = null;
        //        if (gp.IsAutho == true)
        //        {
        //            EmpSalStruct = db.EmpSalStruct.Include(e => e.EmpSalStructDetails).Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
        //        }
        //        else
        //        {
        //            EmpSalStruct = db.EmpSalStruct.Include(e => e.EmpSalStructDetails).AsNoTracking().ToList();
        //        }

        //        IEnumerable<EmpSalStruct> IE;
        //        if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
        //        {
        //            IE = EmpSalStruct;
        //            if (gp.searchOper.Equals("eq"))
        //            {
        //                if (gp.searchField == "Id")
        //                    jsonData = IE.Select(a => new { a.Id, a.EffectiveDate, a.EndDate }).Where((e => (e.Id.ToString().Contains(gp.searchString)))).ToList();
        //                else if (gp.searchField == "EffectiveDate")
        //                    jsonData = IE.Select(a => new { a.Id, a.EffectiveDate, a.EndDate }).Where((e => (e.EffectiveDate.ToString().Contains(gp.searchString)))).ToList();
        //                else if (gp.searchField == "EndDate")
        //                    jsonData = IE.Select(a => new { a.Id, a.EffectiveDate, a.EndDate }).Where((e => (e.EndDate.ToString().Contains(gp.searchString)))).ToList();

        //                //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.EffectiveDate, a.EndDate != null ? Convert.ToString(a.EndDate) : "" }).ToList();
        //            }
        //            totalRecords = IE.Count();
        //        }
        //        else
        //        {
        //            IE = EmpSalStruct;
        //            Func<EmpSalStruct, dynamic> orderfuc;
        //            if (gp.sidx == "Id")
        //            {
        //                orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
        //            }
        //            else
        //            {
        //                orderfuc = (c => gp.sidx == "EffectiveDate" ? c.EffectiveDate.ToString() :
        //                                 gp.sidx == "EndDate" ? c.EndDate.ToString() : "");
        //            }
        //            if (gp.sord == "asc")
        //            {
        //                IE = IE.OrderBy(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.EffectiveDate), a.EndDate != null ? Convert.ToString(a.EndDate) : "" }).ToList();
        //            }
        //            else if (gp.sord == "desc")
        //            {
        //                IE = IE.OrderByDescending(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.EffectiveDate), a.EndDate != null ? Convert.ToString(a.EndDate) : "" }).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.EffectiveDate, a.EndDate != null ? Convert.ToString(a.EndDate) : "" }).ToList();
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
                        jsonData = IE.Where(e =>
                                     (e.Code.ToString().Contains(gp.searchString.ToString()))
                               || (e.Name.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                             || (e.Id.ToString().Contains(gp.searchString.ToString())))
                           .Select(a => new Object[] { a.Code, a.Name, a.Id }).ToList();
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
                        orderfuc = (c => gp.sidx == "EmpCode" ? c.Code.ToString() :
                                         gp.sidx == "EmpName" ? c.Name.ToString() :
                                          "");
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
            using (DataBaseContext db = new DataBaseContext())
            {
                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    try
                    {
                        //EmpSalStruct empsalstruct = db.EmpSalStruct
                        //                       .Include(e => e.EmpSalStructDetails)
                        //                     .Where(e => e.Id == data).SingleOrDefault();

                        var Emp = db.EmployeePayroll.Where(e => e.Employee.Id == data).Select(e => e.EmpSalStruct).SingleOrDefault();
                        EmpSalStruct empsalstruct = Emp.Where(e => e.EndDate == null).SingleOrDefault();
                        db.Entry(empsalstruct).State = System.Data.Entity.EntityState.Deleted;
                        await db.SaveChangesAsync();
                        ts.Complete();
                        return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });

                    }
                    catch (RetryLimitExceededException /* dex */)
                    {
                        return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
                    }
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
                    db.Database.CommandTimeout = 300;

                    var uids = db.EmployeePayroll
                        .Include(e => e.EmpSalStruct)
                        .Include(e => e.Employee)
                        //.Select(e => e.Employee.Id)
                        .ToList();
                    var muids1 = uids.Where(e => e.EmpSalStruct.Count() == 0).ToList();
                    if (muids1 != null && muids1.Count() > 0)
                    {
                        var mUids = muids1.Select(e => e.Employee.Id).ToList();
                        Employee = db.Employee.Include(e => e.EmpName).Where(t => mUids.Contains(t.Id));
                    }
                    else
                    {
                        Employee = null;
                        return this.Json(new Object[] { "", "", " ", JsonRequestBehavior.AllowGet });
                    }
                    //Employee = db.Employee.Include(e => e.EmpName).ToList();
                }

                IEnumerable<Employee> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = Employee;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.EmpCode.ToString().Contains(gp.searchString.ToString()))
                                || (e.EmpName.FullNameFML.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                                || (e.Id.ToString().Contains(gp.searchString.ToString()))
                            ).Select(a => new { a.EmpCode,  a.EmpName.FullNameFML, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.EmpCode), a.EmpName.FullNameFML, a.Id }).ToList();
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
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.EmpCode), a.EmpName.FullNameFML, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.EmpCode), a.EmpName.FullNameFML, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.EmpCode), a.EmpName.FullNameFML, a.Id }).ToList();
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