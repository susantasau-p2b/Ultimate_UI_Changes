using P2b.Global;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using P2BUltimate.Security;
using Payroll;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Transactions;
using System.IO;
using System.Net.NetworkInformation;

namespace P2BUltimate.Controllers.Payroll.MainController
{
    public class BmsPaymentRequestController : Controller
    {
        //
        // GET: /BmsPaymentRequest/
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/BmsPaymentRequest/Index.cshtml");
        }

        public class OffServBookGridData //releasegrid
        {
            public int Id { get; set; }
            public int EmpId { get; set; }
            public Employee Employee { get; set; }
            public Employee EmployeeOff { get; set; }
            public string FromDate { get; set; }
            public string ToDate { get; set; }
            public string PayMonth { get; set; }

        }

        public class OffChildDataClass //childgrid
        {
            public int Id { get; set; }
            public bool Release { get; set; }
            public string ReleaseDate { get; set; }
            public string Activity { get; set; }
            public string EmployeeOff { get; set; }
            public string FromDate { get; set; }
            public string ToDate { get; set; }
            public string PayMonth { get; set; }
            public string OnOfficiatingGrade { get; set; }
            public string Status { get; set; }
            public string InputMethod { get; set; }
        }

        public class returndatagridclass //Parentgrid
        {
            public string Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
            public string JoiningDate { get; set; }
            public string Job { get; set; }
            public string Grade { get; set; }
            public string Location { get; set; }
        }

        public ActionResult GetApplicableEmpPayStructAppl()
        {

            using (DataBaseContext db = new DataBaseContext())
            {
                var s = db.OfficiatingParameter.FirstOrDefault().OfficiatingEmpPayStructAppl;
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Update(BMSPaymentReq BMSPaymentReq, FormCollection form, String forwarddata)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {

                var Emp = form["Employee-Table"] == "0" ? "" : form["Employee-Table"];
                var empid = Convert.ToInt32(Emp);

                var emppayroll = db.EmployeePayroll.Where(e => e.Employee_Id == empid).FirstOrDefault();

                var bmspayment = db.BMSPaymentReq.Where(e => e.FromPeriod == BMSPaymentReq.FromPeriod && e.ToPeriod == BMSPaymentReq.ToPeriod && e.PayMonth == BMSPaymentReq.PayMonth && e.IsCancel == false && e.InputMethod == 0 && e.EmployeePayroll_Id == emppayroll.Id).FirstOrDefault();

                if (bmspayment == null)
                {
                    Msg.Add(" Please Enter the proper processed FromPeriod , ToPeriod or PayMonth ");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }

                var bmspaymentrequest = db.BMSPaymentReq.Where(e => e.Id == bmspayment.Id).FirstOrDefault();

                bmspaymentrequest.ReleaseFlag = true;
                bmspaymentrequest.TrClosed = true;
                bmspaymentrequest.ReleaseDate = DateTime.Now.Date;

                bmspaymentrequest.DBTrack = new DBTrack
                {
                    CreatedBy = bmspaymentrequest.DBTrack.CreatedBy == null ? null : bmspaymentrequest.DBTrack.CreatedBy,
                    CreatedOn = bmspaymentrequest.DBTrack.CreatedOn == null ? null : bmspaymentrequest.DBTrack.CreatedOn,
                    Action = "M",
                    ModifiedBy = SessionManager.UserName,
                    ModifiedOn = DateTime.Now
                };

                db.Entry(bmspaymentrequest).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
            }

            Msg.Add(" Data Saved Successfully. ");
            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CreateSaveAndProcess(BMSPaymentReq BMSPaymentReq, FormCollection form, String forwarddata) //Create submit
        {
            List<string> Msg = new List<string>();
            try
            {
                using (DataBaseContext db = new DataBaseContext())
                {
                    var Emp = form["Employee-Table"] == "0" ? "" : form["Employee-Table"];
                    List<int> ids = null;
                    if (Emp != null && Emp != "0" && Emp != "false")
                    {
                        ids = Utility.StringIdsToListIds(Emp);
                    }
                    else
                    {
                        Msg.Add(" Kindly select employee  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    var EmpOff = form["Incharge_Id"] == "0" ? "" : form["Incharge_Id"];
                    string OffActivity = form["OfficiatingParameterlist"] == "0" ? "" : form["OfficiatingParameterlist"];
                    string NewPayStruct = form["NewPayT-table"] == "0" ? "" : form["NewPayT-table"];
                    int CompId = 0;
                    if (SessionManager.UserName != null)
                    {
                        CompId = Convert.ToInt32(Session["CompId"]);
                    }

                    int EmpOffId;

                    if (!string.IsNullOrEmpty(EmpOff))
                    {
                        EmpOffId = Convert.ToInt32(EmpOff);
                    }
                    else
                    {
                        EmpOffId = 0;
                    }

                    Employee OEmployeeOff = null;
                   // BMSPaymentReq OEmployeePayrollOff = null;

                    // int EmpOffId = Convert.ToInt32(EmpOff);
                    int EmpIdCheck = Convert.ToInt32(Emp);
                    if (EmpIdCheck != null && EmpIdCheck != 0)
                    {

                        List<string> MsgCheck = new List<string>();

                        OEmployeeOff = db.Employee.Include(q => q.EmpName).Where(r => r.Id == EmpIdCheck).AsNoTracking().AsParallel().SingleOrDefault();

                        // OEmployeePayrollOff = db.EmployeePayroll.Include(e => e.OfficiatingServiceBook).Where(e => e.Employee_Id == OEmployeeOff.Id).AsNoTracking().AsParallel().SingleOrDefault();
                        // OEmployeePayrollOff = db.BMSPaymentReq.Include(e => e.EmployeePayroll).Where(e => e.EmployeePayroll.Employee_Id == OEmployeeOff.Id).AsNoTracking().AsParallel().FirstOrDefault();
                       


                        foreach (var id in ids)
                        {
                            DateTime mFromPeriod = Convert.ToDateTime(BMSPaymentReq.FromPeriod);
                            DateTime mEndDate = Convert.ToDateTime(BMSPaymentReq.ToPeriod);
                            var emp = db.Employee.Where(e => e.Id == id).FirstOrDefault();
                            for (DateTime mTempDate = mFromPeriod; mTempDate <= mEndDate; mTempDate = mTempDate.AddDays(1))
                            {

                                // int Officiatingdates = db.OfficiatingServiceBook.Where(e => e.EmployeePayroll.Employee.Id == id && e.FromDate.Value <= mTempDate.Date && e.ToDate.Value >= mTempDate.Date).Select(e => e.Id).SingleOrDefault();
                                int Officiatingdates = db.BMSPaymentReq.Where(e => e.EmployeePayroll.Employee.Id == id && (e.FromPeriod.Value <= mTempDate.Date && e.ToPeriod.Value >= mTempDate.Date) && e.IsCancel == false && e.TrReject == false).Select(e => e.Id).FirstOrDefault();

                                if (Officiatingdates > 0)
                                {
                                    Msg.Add("You have already process " + mTempDate.Date + " for this employee " + emp.EmpCode);
                                    return Json(new { status = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }

                            }
                        }

                    }

                    if (OffActivity == null || OffActivity == "")
                    {
                        Msg.Add(" Kindly select Officiating Parameter  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    Employee OEmployeec = null;
                    EmployeePayroll OEmployeePayrollc = null;
                    string EmpRel = null;
                    string EmpPro = null;
                    string EmpCPI = null;
                    string PayMonth = BMSPaymentReq.PayMonth;

                    if (EmpOffId > 0)
                    {

                        OEmployeec = db.Employee.Where(r => r.Id == EmpOffId).SingleOrDefault();

                        OEmployeePayrollc = db.EmployeePayroll.Where(e => e.Employee.Id == EmpOffId).SingleOrDefault();


                        var OEmpSalProcess = db.EmployeePayroll.Where(e => e.Id == OEmployeePayrollc.Id).Include(e => e.SalaryT).SingleOrDefault();
                        var EmpSalRelTPro = OEmpSalProcess.SalaryT != null ? OEmpSalProcess.SalaryT.Where(e => e.PayMonth == PayMonth && e.ReleaseDate == null) : null;

                        if (EmpSalRelTPro != null && EmpSalRelTPro.Count() > 0)
                        {
                            if (EmpPro == null || EmpPro == "")
                            {
                                EmpPro = OEmployeec.EmpCode;
                            }
                            else
                            {
                                EmpPro = EmpPro + ", " + OEmployeec.EmpCode;
                            }
                        }

                        var OEmpCPIProcess = db.EmployeePayroll.Where(e => e.Id == OEmployeePayrollc.Id).Include(e => e.CPIEntryT).SingleOrDefault();
                        var EmpCPIPro = db.CPIEntryT.Where(e => e.EmployeePayroll_Id == OEmployeePayrollc.Id && e.PayMonth == PayMonth).Select(e => e.Id).FirstOrDefault();


                        if (EmpCPIPro == 0)
                        {
                            if (EmpCPI == null || EmpCPI == "")
                            {
                                EmpCPI = OEmployeec.EmpCode;
                            }
                            else
                            {
                                EmpCPI = EmpCPI + ", " + OEmployeec.EmpCode;
                            }
                        }



                        var OEmpSalRelT = db.EmployeePayroll.Where(e => e.Id == OEmployeePayrollc.Id).Include(e => e.SalaryT).SingleOrDefault();
                        var EmpSalRelT = OEmpSalRelT.SalaryT != null ? OEmpSalRelT.SalaryT.Where(e => e.PayMonth == PayMonth && e.ReleaseDate != null) : null;

                        if (EmpSalRelT != null && EmpSalRelT.Count() > 0)
                        {
                            if (EmpRel == null || EmpRel == "")
                            {
                                EmpRel = OEmployeec.EmpCode;
                            }
                            else
                            {
                                EmpRel = EmpRel + ", " + OEmployeec.EmpCode;
                            }
                        }
                        if (EmpPro != null)
                        {
                            Msg.Add("Salary Processed for employee " + EmpPro + ". You can't Enter Officating now.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                        if (EmpRel != null)
                        {
                            Msg.Add("Salary released for employee " + EmpRel + ". You can't change Officating now.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                        if (EmpCPI != null)
                        {
                            Msg.Add("Please Run CPI for employee " + EmpCPI + ". And Try again.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }

                    }
                    //int TransActivityId = 0;

                    //if (OffActivity != null && OffActivity != "")
                    //{
                    //    TransActivityId = int.Parse(OffActivity);

                    //    BMSPaymentReq.OfficiatingParameter = db.OfficiatingParameter.Where(e => e.Id == TransActivityId).SingleOrDefault();
                    //}
                    //// NKGSB Start
                    //var cid = Convert.ToInt32(SessionManager.CompanyId);
                    //var offpara = db.OfficiatingParameter.Where(e => e.Id == TransActivityId).SingleOrDefault();
                    //if (offpara.NewPayStructOnScreenAppl == false)
                    //{
                    //    int EmpIdw = Convert.ToInt32(Emp);
                    //    Employee employeew = null;
                    //    OEmployeeOff = db.Employee.Include(q => q.EmpName).Where(r => r.Id == EmpOffId).SingleOrDefault();
                    //    employeew = db.Employee.Include(q => q.EmpName).Where(r => r.Id == EmpIdw).SingleOrDefault();
                    //    if (offpara.GradeShiftCount != 0)
                    //    {
                    //        if (employeew.PayStruct_Id > OEmployeeOff.PayStruct_Id)
                    //        {

                    //            var pay_dataoff = db.PayStruct.Where(e => e.Company.Id == cid && e.JobStatus_Id != null && e.Id == OEmployeeOff.PayStruct_Id)
                    //                   .Select(e => new
                    //                   {
                    //                       code = e.Id.ToString(),
                    //                       GCode = e.Grade.Code,
                    //                       GName = e.Grade.Name.ToString(),
                    //                       Levelname = e.Level.Name.ToString(),
                    //                       EmpActingStatus = e.JobStatus.EmpActingStatus.LookupVal,
                    //                       EmpStatus = e.JobStatus.EmpStatus.LookupVal


                    //                   }).SingleOrDefault();

                    //            var pay_datalist = db.PayStruct.Include(e => e.Level).Include(e => e.JobStatus).Include(e => e.JobStatus.EmpActingStatus).Include(e => e.JobStatus.EmpStatus).Where(e => e.Company.Id == cid && e.JobStatus_Id != null && e.Id < employeew.PayStruct_Id).OrderByDescending(e => e.Id).ToList();
                    //            if (pay_dataoff.Levelname == "")
                    //            {
                    //                var pay_data = pay_datalist.Where(e => e.JobStatus.EmpActingStatus.LookupVal == pay_dataoff.EmpActingStatus && e.JobStatus.EmpStatus.LookupVal == pay_dataoff.EmpStatus).OrderByDescending(e => e.Id).Skip(offpara.GradeShiftCount - 1).Take(1).SingleOrDefault();
                    //                if (pay_data == null)
                    //                {
                    //                    Msg.Add("Auto shift Grade not avaialble in paystructure");
                    //                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //                }
                    //                BMSPaymentReq.OfficiatingPayStruct = pay_data.Id;
                    //            }
                    //            else
                    //            {
                    //                var pay_data = pay_datalist.Where(e => e.Level.Name == pay_dataoff.Levelname && e.JobStatus.EmpActingStatus.LookupVal == pay_dataoff.EmpActingStatus && e.JobStatus.EmpStatus.LookupVal == pay_dataoff.EmpStatus).OrderByDescending(e => e.Id).Skip(offpara.GradeShiftCount - 1).Take(1).SingleOrDefault();
                    //                if (pay_data == null)
                    //                {
                    //                    Msg.Add("Auto shift Grade not avaialble in paystructure");
                    //                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //                }
                    //                BMSPaymentReq.OfficiatingPayStruct = pay_data.Id;
                    //            }



                    //        }
                    //        else
                    //        {
                    //            var pay_dataoff = db.PayStruct.Where(e => e.Company.Id == cid && e.JobStatus_Id != null && e.Id == OEmployeeOff.PayStruct_Id)
                    //                                                  .Select(e => new
                    //                                                  {
                    //                                                      code = e.Id.ToString(),
                    //                                                      GCode = e.Grade.Code,
                    //                                                      GName = e.Grade.Name.ToString(),
                    //                                                      Levelname = e.Level.Name.ToString(),
                    //                                                      EmpActingStatus = e.JobStatus.EmpActingStatus.LookupVal,
                    //                                                      EmpStatus = e.JobStatus.EmpStatus.LookupVal


                    //                                                  }).SingleOrDefault();


                    //            var pay_datalist = db.PayStruct.Include(e => e.Level).Include(e => e.JobStatus).Include(e => e.JobStatus.EmpActingStatus).Include(e => e.JobStatus.EmpStatus).Where(e => e.Company.Id == cid && e.JobStatus_Id != null && e.Id < employeew.PayStruct_Id).OrderBy(e => e.Id).ToList();
                    //            if (pay_dataoff.Levelname == "")
                    //            {
                    //                var pay_data = pay_datalist.Where(e => e.JobStatus.EmpActingStatus.LookupVal == pay_dataoff.EmpActingStatus && e.JobStatus.EmpStatus.LookupVal == pay_dataoff.EmpStatus).OrderBy(e => e.Id).Skip(offpara.GradeShiftCount - 1).Take(1).SingleOrDefault();
                    //                if (pay_data == null)
                    //                {
                    //                    Msg.Add("Auto shift Grade not avaialble in paystructure");
                    //                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //                }
                    //                BMSPaymentReq.OfficiatingPayStruct = pay_data.Id;
                    //            }
                    //            else
                    //            {
                    //                var pay_data = pay_datalist.Where(e => e.Level.Name == pay_dataoff.Levelname && e.JobStatus.EmpActingStatus.LookupVal == pay_dataoff.EmpActingStatus && e.JobStatus.EmpStatus.LookupVal == pay_dataoff.EmpStatus).OrderBy(e => e.Id).Skip(offpara.GradeShiftCount - 1).Take(1).SingleOrDefault();
                    //                if (pay_data == null)
                    //                {
                    //                    Msg.Add("Auto shift Grade not avaialble in paystructure");
                    //                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //                }
                    //                BMSPaymentReq.OfficiatingPayStruct = pay_data.Id;
                    //            }


                    //        }
                    //    }


                    //}
                    //if (NewPayStruct != "" && NewPayStruct != null)
                    //{
                    //    var payid = db.PayStruct.Find(int.Parse(NewPayStruct));
                    //    BMSPaymentReq.OfficiatingPayStruct = payid.Id;
                    //}

                    // NKGSB End


                    FunctAllWFDetails oAttWFDetails = new FunctAllWFDetails
                    {
                        WFStatus = 5,
                        Comments = BMSPaymentReq.Narration.ToString(),
                        DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                    };

                    List<FunctAllWFDetails> oAttWFDetails_List = new List<FunctAllWFDetails>();
                    oAttWFDetails_List.Add(oAttWFDetails);

                    using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                            new System.TimeSpan(0, 30, 0)))
                    {
                        Employee OEmployee = null;
                        int EmpId = Convert.ToInt32(Emp);
                        BMSPaymentReq.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                        OEmployee = db.Employee.Include(q => q.EmpName).Where(r => r.Id == EmpId).AsNoTracking().AsParallel().SingleOrDefault();
                        BMSPaymentReq.FuncStruct_Id = Convert.ToInt32(OEmployee.FuncStruct_Id);
                        BMSPaymentReq.GeoStruct_Id = Convert.ToInt32(OEmployee.GeoStruct_Id);
                        BMSPaymentReq.EmployeePayroll_Id = Convert.ToInt32(OEmployee.Id);
                        BMSPaymentReq.PayStruct_Id = Convert.ToInt32(OEmployee.PayStruct_Id);
                        // BMSPaymentReq.OfficiatingEmployeeId = Convert.ToInt32(OEmployeeOff.Id);
                        BMSPaymentReq.OfficiatingPayStruct = Convert.ToInt32(NewPayStruct);

                        if (EmpOffId > 0)
                        {
                            BMSPaymentReq.OfficiatingEmployeeId = Convert.ToInt32(EmpOffId);
                        }
                        else
                        {
                            BMSPaymentReq.OfficiatingEmployeeId = 0;
                        }

                        var ProMonth = DateTime.Now.ToString("MM/yyyy");
                        var calendarid = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "FinancialYear" && e.Default == true).SingleOrDefault().Id;
                        var FiCalendarid = Convert.ToInt32(calendarid);

                        var salheadid = db.SalaryHead.Include(e => e.SalHeadOperationType).Where(e => e.SalHeadOperationType.LookupVal.ToUpper() == "OFFICIATING").FirstOrDefault().Id;

                        var OEmployeePayrollOff = db.EmployeePayroll.Where(e => e.Employee_Id == EmpIdCheck).FirstOrDefault();

                        BMSPaymentReq OOffServiceBook = new BMSPaymentReq()
                        {
                            FuncStruct_Id = BMSPaymentReq.FuncStruct_Id,
                            GeoStruct_Id = BMSPaymentReq.GeoStruct_Id,
                            EmployeePayroll_Id = OEmployeePayrollOff.Id,
                            SalaryHead_Id = salheadid,
                            PayStruct_Id = BMSPaymentReq.PayStruct_Id,
                            FromPeriod = BMSPaymentReq.FromPeriod,
                            OfficiatingEmployeeId = BMSPaymentReq.OfficiatingEmployeeId,
                            Narration = BMSPaymentReq.Narration,
                            ToPeriod = BMSPaymentReq.ToPeriod,
                            DBTrack = BMSPaymentReq.DBTrack,
                            ReleaseDate = null,
                            PayMonth = BMSPaymentReq.PayMonth,
                            ProcessMonth = ProMonth,
                            FinancialYear_Id = FiCalendarid,
                            OfficiatingParameter = BMSPaymentReq.OfficiatingParameter,
                            OfficiatingParameter_Id = BMSPaymentReq.OfficiatingParameter_Id,
                            OfficiatingPayStruct = BMSPaymentReq.OfficiatingPayStruct,
                            InputMethod = 0,
                            WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "1").FirstOrDefault(),//db.LookupValue.Where(e => e.LookupVal == "1").SingleOrDefault(),
                            OffWFDetails = oAttWFDetails_List
                        };

                        db.BMSPaymentReq.Add(OOffServiceBook);
                        db.SaveChanges();
                        TempData["offid"] = OOffServiceBook.Id;
                        ts.Complete();
                    }

                    double CalAmount = 0.0;
                    var offid = Convert.ToInt32(TempData["offid"]);
                    TempData["officiate_id"] = offid;
                    var EmpPayroll = db.EmployeePayroll.Where(e => e.Employee_Id == EmpIdCheck).FirstOrDefault();
                    TempData["EmployeePayrollId"] = EmpPayroll.Id;
                    var OEmployeePayrollOfficiate = db.BMSPaymentReq.Include(e => e.EmployeePayroll).Where(e => e.EmployeePayroll_Id == EmpPayroll.Id).FirstOrDefault();

                    var OffProcess = officiateprocess(OEmployeePayrollOfficiate, PayMonth, CalAmount, OffActivity, offid);



                    Msg.Add(" Process Completed please click on submit button to completed the transaction. ");
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
                    LineNo = Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                    LogTime = DateTime.Now
                };
                Logfile.CreateLogFile(Err);

                Msg.Add(ex.Message);

                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult P2BGridRelease(P2BGrid_Parameters gp, string param)
        {
            try
            {
                DataBaseContext db = new DataBaseContext();
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                var jsonData = (Object)null;

                IEnumerable<OffServBookGridData> OffServBook = null;
                List<OffServBookGridData> model = new List<OffServBookGridData>();
                OffServBookGridData view = null;

                //var OEmployee = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.Employee.EmpName)
                //    .AsNoTracking().Where(q => q.OfficiatingServiceBook.Count > 0).AsParallel().ToList();
                var OEmployee = db.BMSPaymentReq.Include(e => e.EmployeePayroll).Include(e => e.EmployeePayroll.Employee).Include(e => e.EmployeePayroll.Employee.EmpName)
                                                .AsNoTracking().AsParallel().ToList();
                string PayMonth = "";
                string Month = "";
                if (gp.filter != null)
                    PayMonth = gp.filter;
                else
                {
                    if (DateTime.Now.Date.Month < 10)
                        Month = "0" + DateTime.Now.Date.Month;
                    else
                        Month = DateTime.Now.Date.Month.ToString();
                    PayMonth = Month + "/" + DateTime.Now.Date.Year;
                }
                foreach (var z in OEmployee)
                {
                    //var ObjOffServBook = db.EmployeePayroll.Where(e => e.Id == z.Id)
                    //                    .Select(e => e.OfficiatingServiceBook.Where(r => r.Release == false && r.InputMethod == 0))
                    //                    .SingleOrDefault();
                    var ObjOffServBook = db.BMSPaymentReq.Where(e => e.Id == z.Id && e.ReleaseFlag == false && e.InputMethod == 0).ToList();

                    //DateTime? Eff_Date = null;
                    //PayScaleAgreement PayScaleAgr = null;
                    foreach (var a in ObjOffServBook)
                    {
                        //Eff_Date = Convert.ToDateTime(a.ProcessMonth);
                        BMSPaymentReq aa = new BMSPaymentReq();
                        aa = db.BMSPaymentReq
                              .Where(e => e.Id == a.Id).SingleOrDefault();

                        if (aa != null)
                        {
                            //if (aa.ProcessIncrDate.Value.ToString("MM/yyyy") == PayMonth)
                            //{
                            view = new OffServBookGridData()
                            {
                                Id = a.Id,
                                EmpId = z.EmployeePayroll.Employee.Id,
                                Employee = z.EmployeePayroll.Employee,
                                FromDate = aa.FromPeriod != null ? aa.FromPeriod.Value.ToString("dd/MM/yyyy") : null,
                                ToDate = aa.ToPeriod != null ? aa.ToPeriod.Value.ToString("dd/MM/yyyy") : null,
                                PayMonth = aa.PayMonth
                            };

                            model.Add(view);
                            db.Entry(aa).State = System.Data.Entity.EntityState.Detached;
                            //}
                        }
                    }

                }

                OffServBook = model;

                IEnumerable<OffServBookGridData> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = OffServBook;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.EmpId.ToString().Contains(gp.searchString))
                                || (e.Employee.EmpCode.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                                || (e.Employee.EmpName.FullNameFML.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                                || (e.FromDate.ToString().Contains(gp.searchString))
                                || (e.ToDate.ToString().Contains(gp.searchString))
                                || (e.PayMonth.ToString().Contains(gp.searchString))
                                || (e.Id.ToString().Contains(gp.searchString))
                            ).Select(a => new Object[] { a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.FromDate, a.ToDate, a.Id, a.EmpId }).ToList();
                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.FromDate, a.ToDate, a.PayMonth, a.Id, a.EmpId }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = OffServBook;
                    Func<OffServBookGridData, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "EmpId" ? c.EmpId.ToString() :
                                         gp.sidx == "EmpCode" ? c.Employee.EmpCode.ToString() :
                                         gp.sidx == "EmpName" ? c.Employee.EmpName.FullNameFML.ToString() :
                                         gp.sidx == "FromDate" ? c.FromDate.ToString() :
                                         gp.sidx == "ToDate" ? c.ToDate.ToString() :
                                         gp.sidx == "PayMonth" ? c.PayMonth.ToString() : ""
                                          );
                    }
                    if (gp.sord == "asc")  //Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : ""
                    {
                        IE = IE.OrderBy(orderfuc);  // a.Id,a.Employeee.EmpCode,a.Employeee.EmpName, a.SalaryHead.Name, a.FromPeriod, a.ToPeriod, a.AmountPaid, a.OtherDeduction, a.ProcessMonth, a.TDSAmount, a.Narration
                        jsonData = IE.Select(a => new Object[] { a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.FromDate, a.ToDate, a.PayMonth, a.Id, a.EmpId }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.FromDate, a.ToDate, a.PayMonth, a.Id, a.EmpId }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.FromDate, a.ToDate, a.PayMonth, a.Id, a.EmpId }).ToList();
                    }
                    totalRecords = OffServBook.Count();
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

        public ActionResult Get_OffServBook(int data)
        {
            try
            {
                using (DataBaseContext db = new DataBaseContext())
                {
                    //var db_data = db.EmployeePayroll
                    //                .Include(e => e.OfficiatingServiceBook)
                    //                .Include(e => e.OfficiatingServiceBook.Select(a => a.OffWFDetails))
                    //                .Include(e => e.OfficiatingServiceBook.Select(a => a.WFStatus))
                    //                .Where(e => e.Id == data).SingleOrDefault();

                    //var db_data = db.BMSPaymentReq.Include(e => e.OffWFDetails).Include(e => e.WFStatus)
                    //              .Where(e => e.EmployeePayroll_Id == data).ToList();

                    var db_data = db.BMSPaymentReq.Include(e => e.OffWFDetails).Include(e => e.SalaryHead).Include(e => e.SalaryHead.SalHeadOperationType).Include(e => e.WFStatus)
                                 .Where(e => e.EmployeePayroll_Id == data && e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "OFFICIATING").ToList();

                    if (db_data != null)
                    {
                        List<OffChildDataClass> returndata = new List<OffChildDataClass>();
                        foreach (var item in db_data)
                        {
                            var Status = "--";
                            if (item.OffWFDetails.Count > 0)
                            {
                                Status = Utility.GetStatusName().Where(e => e.Key == item.OffWFDetails.LastOrDefault().WFStatus.ToString()).Select(e => e.Key).SingleOrDefault();
                            }
                            if (Status == "5")
                            {
                                Status = "Approved By HRM (M)";
                            }
                            if (Status == "0")
                            {
                                Status = "Applied";
                            }
                            else if (Status == "1")
                            {
                                Status = "Sanctioned";
                            }
                            else if (Status == "2")
                            {
                                Status = "Sanction Rejected";
                            }
                            else if (Status == "3")
                            {
                                Status = "Approved";
                            }
                            else if (Status == "4")
                            {
                                Status = "Approved Rejected";
                            }
                            else if (Status == "6")
                            {
                                Status = "Cancel";
                            }
                            string EmpName = "";
                            int EmppayrollId=Convert.ToInt32(data);
                            var Emp_Payroll = db.EmployeePayroll.Where(e => e.Id == EmppayrollId).SingleOrDefault();
                            if (Emp_Payroll!=null)
                            {
                                int EmployeeId = Convert.ToInt32(Emp_Payroll.Employee_Id);
                                var Emp = db.Employee.Include(e => e.EmpName).Where(e => e.Id == EmployeeId).SingleOrDefault();
                                if (Emp!=null)
                                {
                                    EmpName = Emp.EmpName.FullNameFML;
                                }
                            }
                            returndata.Add(new OffChildDataClass
                            {
                                Id = item.Id,
                                EmployeeOff = EmpName,
                                Release = item.ReleaseFlag,
                                ReleaseDate = item.ReleaseDate != null ? item.ReleaseDate.Value.ToString("dd/MM/yyyy") : null,
                                FromDate = item.FromPeriod != null ? item.FromPeriod.Value.ToString("dd/MM/yyyy") : null,
                                ToDate = item.ToPeriod != null ? item.ToPeriod.Value.ToString("dd/MM/yyyy") : null,
                                PayMonth = item.PayMonth,
                                OnOfficiatingGrade = db.PayStruct.Include(e => e.Grade).Where(e => e.Id == item.OfficiatingPayStruct).FirstOrDefault() == null ? "" : db.PayStruct.Include(e => e.Grade).Where(e => e.Id == item.OfficiatingPayStruct).FirstOrDefault().Grade.FullDetails.ToString() == null ? "" : db.PayStruct.Include(e => e.Grade).Where(e => e.Id == item.OfficiatingPayStruct).FirstOrDefault().Grade.FullDetails.ToString(),
                                Status = Status,
                                InputMethod = item.InputMethod != null ? item.InputMethod.ToString() : null,
                            });
                        }
                        var result = returndata.OrderByDescending(e => e.Id).ToList();

                        return Json(result, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public ActionResult Emp_Grid(ParamModel param, string y)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var all = db.EmployeePayroll.Include(e => e.Employee)
                        .Include(e => e.Employee.EmpName)
                        .Include(e => e.Employee.ServiceBookDates)
                        .Where(e => e.Employee.ServiceBookDates.ServiceLastDate == null).ToList();
                    // for searchs
                    IEnumerable<EmployeePayroll> fall;
                    if (param.sSearch == null)
                    {
                        fall = all;
                    }
                    else
                    {
                        //fall = all.Where(e => (e.Employee.EmpCode == param.sSearch) || (e.Employee.EmpName.FullNameFML.ToUpper() == param.sSearch.ToUpper())).ToList();
                        fall = all.Where(e => (e.Id.ToString().Contains(param.sSearch))
                               || (e.Employee.EmpCode.Contains(param.sSearch))
                               || (e.Employee.EmpName.FullNameFML.ToUpper().Contains(param.sSearch.ToUpper()))
                               ).ToList();
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
                                Name = item.Employee.EmpName != null ? item.Employee.EmpName.FullNameFML : null,
                                //JoiningDate = item.Employee.ServiceBookDates != null && item.Employee.ServiceBookDates.JoiningDate != null ? item.Employee.ServiceBookDates.JoiningDate.Value.ToShortDateString() : null,
                                //Job = item.Employee.FuncStruct != null && item.Employee.FuncStruct.Job != null ? item.Employee.FuncStruct.Job.Name : null,
                                //Grade = item.Employee.PayStruct != null && item.Employee.PayStruct.Grade != null ? item.Employee.PayStruct.Grade.Name : null,
                                //Location = item.Employee.GeoStruct != null && item.Employee.GeoStruct.Location != null && item.Employee.GeoStruct.Location.LocationObj != null ? item.Employee.GeoStruct.Location.LocationObj.LocDesc : null
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
                    throw e;
                }
            }
        }

        public ActionResult Release(OfficiatingServiceBook OfficiatingServiceBook, FormCollection form, String forwarddata, string param) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    var Id = form["releas_Id"] == "0" ? "" : form["releas_Id"];

                    var Emp = form["emp_Id"] == "0" ? "" : form["emp_Id"];
                    var ReleaseFlag = form["ReleaseFlag"] == "0" ? "" : form["ReleaseFlag"];
                    var chkrelease = Convert.ToBoolean(ReleaseFlag);

                    int CompId = 0;
                    if (!String.IsNullOrEmpty(Session["CompId"].ToString()))
                    {
                        CompId = Convert.ToInt32(Session["CompId"]);
                    }

                    List<int> ids = null;
                    if (Emp != null && Emp != "0" && Emp != "false")
                    {
                        ids = Utility.StringIdsToListIds(Emp);
                    }
                    else
                    {
                        Msg.Add(" Kindly select employee  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }


                    //  Employee OEmployee = null;
                    EmployeePayroll OEmployeePayroll = null;



                    foreach (var Empid in ids)
                    {

                        OEmployeePayroll = db.EmployeePayroll.Where(e => e.Employee.Id == Empid).AsNoTracking().SingleOrDefault();

                        using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, new System.TimeSpan(0, 30, 0)))
                        {
                            int OffId = Convert.ToInt32(Id.ToString().Replace(",", " "));
                            OfficiatingServiceBook OfficiatingServiceBook1 = db.OfficiatingServiceBook.Include(e => e.OfficiatingParameter).Where(e => e.Id == OffId).SingleOrDefault();
                            //  officiateprocess(OfficiatingServiceBook1);
                            OfficiatingServiceBook1.ReleaseDate = OfficiatingServiceBook.ReleaseDate;
                            OfficiatingServiceBook1.Release = Convert.ToBoolean(ReleaseFlag);
                            db.OfficiatingServiceBook.Attach(OfficiatingServiceBook1);
                            db.Entry(OfficiatingServiceBook1).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            ts.Complete();
                        }
                    }
                    Msg.Add("  Data Updated successfully  ");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                }
                catch (Exception ex)
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
                    //  List<string> Msg = new List<string>();
                    Msg.Add(ex.Message);
                    return Json(new { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public JsonResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                var Q = db.OfficiatingServiceBook
                    .Where(e => e.Id == data).SingleOrDefault();

                var returnObj = new
                {
                    Remark = Q.Remark == null ? "" : Q.Remark,
                    Releasedate = Q.ReleaseDate == null ? "" : Q.ReleaseDate.Value.ToShortDateString()
                };

                var OffServBook = db.OfficiatingServiceBook.Find(data);
                Session["RowVersion"] = OffServBook.RowVersion;
                var Auth = OffServBook.DBTrack.IsModified;
                return Json(new Object[] { returnObj, Auth }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetNewPayStructDetails(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var id = Convert.ToInt32(SessionManager.CompanyId);


                var pay_data = db.PayStruct.Where(e => e.Company.Id == id && e.JobStatus_Id != null && e.JobStatus.EmpActingStatus.LookupVal.ToUpper() == "ACTIVE")
                .Select(e => new
                {
                    code = e.Id.ToString(),
                    value = e.Grade.Code + " - " + e.Grade.Name.ToString() + " " + e.Level.Name.ToString() + e.JobStatus.EmpActingStatus.LookupVal + " " + e.JobStatus.EmpStatus.LookupVal
                }).ToList();

                return Json(pay_data, JsonRequestBehavior.AllowGet);

            }
        }

        public ActionResult GetApplicableData(string data, string data2)
        {
            //data-selected
            //data2-empid
            using (DataBaseContext db = new DataBaseContext())
            {
                if ((data2 != null && data2 != "0") && (data != null && data != "0"))
                {
                    var Empid = Convert.ToInt32(data);
                    var Policyid = Convert.ToInt32(data2);
                    var query = db.OfficiatingParameter.Where(e => e.Id == Policyid).FirstOrDefault();

                    DateTime? ExpDate = null;
                    DateTime? ProcActDate = null;
                    string Period = null;
                    string OfficiatingEmpPayStructAppl = null;
                    if (query != null)
                    {
                        if (query.NewPayStructOnScreenAppl == true)
                        {
                            OfficiatingEmpPayStructAppl = "Manual Select Grade";
                            return Json(OfficiatingEmpPayStructAppl, JsonRequestBehavior.AllowGet);
                        }
                    }

                    return Json(null, JsonRequestBehavior.AllowGet);


                }
                else
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }

            }
        }

        public ActionResult PopulateDropDownActivityList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                int OEmp_Id = Convert.ToInt32(data2);
                var query = db.EmployeePolicyStruct.AsNoTracking().Where(e => e.EmployeePayroll.Employee.Id == OEmp_Id && e.EndDate == null).Include(e => e.EmployeePayroll).Include(e => e.EmployeePayroll.Employee).Include(e => e.EmployeePolicyStructDetails)
                    .Include(e => e.EmployeePolicyStructDetails.Select(r => r.PolicyFormula)).Include(e => e.EmployeePolicyStructDetails.
                    Select(r => r.PolicyFormula.OfficiatingParameter)).FirstOrDefault();
                var TransActList = query.EmployeePolicyStructDetails.Select(r => r.PolicyFormula.OfficiatingParameter);

                var selected = (Object)null;

                List<OfficiatingParameter> TransAct = new List<OfficiatingParameter>();
                if (TransActList.Count() > 0)
                {
                    foreach (var item in TransActList)
                    {
                        if (item.FirstOrDefault() != null)
                        {
                            foreach (var item1 in item)
                            {
                                TransAct.Add(item1);
                            }
                        }

                    }
                }
                SelectList s = new SelectList(TransAct, "Id", "Name", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost]
        public ActionResult GridDelete(int data)
        {
            List<string> Msg = new List<string>();

            try
            {
                using (DataBaseContext db = new DataBaseContext())
                {
                    BMSPaymentReq OffServBook = db.BMSPaymentReq.Include(e => e.OffWFDetails).Include(e => e.OfficiatingPaymentT).Where(e => e.Id == data).SingleOrDefault();

                    var SalCheck = db.SalaryT.Include(e => e.EmployeePayroll).Where(e => e.EmployeePayroll.Id == OffServBook.EmployeePayroll_Id && e.PayMonth == OffServBook.PayMonth).FirstOrDefault();

                    if (SalCheck != null)
                    {
                        return Json(new Utility.JsonReturnClass { success = false, responseText = "Salary is Processed" }, JsonRequestBehavior.AllowGet);
                    }

                    //to delete the record
                    if (OffServBook.InputMethod == 0 && OffServBook.ReleaseFlag == false)
                    {

                        if (SalCheck == null)
                        {

                            var functwfdetailsObj = OffServBook.OffWFDetails.ToList();

                            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {
                                try
                                {
                                    foreach (var functitem in functwfdetailsObj)
                                    {
                                        FunctAllWFDetails funcTdata = functitem;
                                        db.Entry(funcTdata).State = System.Data.Entity.EntityState.Deleted;
                                        //functwfdetails.FunctAllWFDetails.Remove(funcTdata);
                                    }

                                    db.BMSPaymentReq.Attach(OffServBook);
                                    db.Entry(OffServBook).State = System.Data.Entity.EntityState.Deleted;
                                    db.SaveChanges();
                                    db.Entry(OffServBook).State = System.Data.Entity.EntityState.Detached;

                                    var OfficiatingPayments = db.OfficiatingPaymentT.Where(e => !db.BMSPaymentReq.Any(a => a.OfficiatingPaymentT.Contains(e)))
                                                             .ToList();

                                    if (OfficiatingPayments.Any())
                                    {

                                        db.OfficiatingPaymentT.RemoveRange(OfficiatingPayments);
                                        db.SaveChanges();
                                    }


                                    ts.Complete();
                                    return this.Json(new { status = true, responseText = "Data removed successfully.", JsonRequestBehavior.AllowGet });
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
                    }


                    //cancel
                    if (OffServBook.InputMethod == 0 && OffServBook.ReleaseFlag == true)
                    {

                        if (SalCheck == null)
                        {

                            var functwfdetailsObj = OffServBook.OffWFDetails.ToList();

                            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {
                                try
                                {
                                    foreach (var functitem in functwfdetailsObj)
                                    {
                                        functitem.WFStatus = 6;
                                        functitem.Comments = "Cancelled by Admin";
                                        functitem.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                    }

                                    OffServBook.IsCancel = true;
                                    OffServBook.TrClosed = true;

                                    db.BMSPaymentReq.Attach(OffServBook);
                                    db.Entry(OffServBook).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    ts.Complete();
                                    return this.Json(new { status = true, responseText = "Data removed successfully.", JsonRequestBehavior.AllowGet });
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
                    }

                    //cancel
                    if (OffServBook.InputMethod == 1 && OffServBook.ReleaseFlag == true)
                    {

                        if (SalCheck == null)
                        {

                            var functwfdetailsObj = OffServBook.OffWFDetails.ToList();

                            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {
                                try
                                {
                                    foreach (var functitem in functwfdetailsObj)
                                    {
                                        functitem.WFStatus = 6;
                                        functitem.Comments = "Cancelled by Admin";
                                        functitem.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                    }

                                    OffServBook.IsCancel = true;
                                    OffServBook.TrClosed = true;

                                    db.BMSPaymentReq.Attach(OffServBook);
                                    db.Entry(OffServBook).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    ts.Complete();
                                    return this.Json(new { status = true, responseText = "Data removed successfully.", JsonRequestBehavior.AllowGet });
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
                    }

                    return null;
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

        //public ActionResult Officiatingeprocess(string data, string data2, string data3, string data4, string data5, int data6)
        //{
        //    List<string> Msg = new List<string>();

        //    using (DataBaseContext db = new DataBaseContext())
        //    {

        //        //var Emp_Id = Convert.ToInt32(TempData["EMPId"]);
        //        var Emp_Id = Convert.ToInt32(data4);
        //        EmployeePayroll OEmpPayroll = db.EmployeePayroll.Include(e => e.Employee).Where(e => e.Employee.Id == Emp_Id).FirstOrDefault();
        //        string PayMonth = data;
        //        if (PayMonth == null || PayMonth == "")
        //        {
        //            Msg.Add("Please Select the PayMonth");
        //            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //        }
        //        double CalAmount = 0.0;

        //        //var offid = Convert.ToInt32(TempData["officitingId"]);
        //        var offid = Convert.ToInt32(data6);


        //        var salpro = db.SalaryT.Include(e => e.EmployeePayroll).Include(e => e.EmployeePayroll.Employee).Where(e => e.EmployeePayroll.Employee_Id == OEmpPayroll.Employee.Id && e.PayMonth == PayMonth).FirstOrDefault();

        //        if (salpro != null)
        //        {

        //            Msg.Add("Please delete the salary and contact to the Administration ");
        //            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //        }
        //        var SalaryHeadId = Convert.ToInt32(data5);

        //        var salheadoperation = db.SalaryHead.Include(e => e.SalHeadOperationType).Where(e => e.Id == SalaryHeadId).FirstOrDefault();
        //        string SalHeadoperationtype = salheadoperation.SalHeadOperationType.LookupVal.ToUpper();

        //        var OfficiatingParameterlist = data3;

        //        if (SalHeadoperationtype == "OFFICIATING")
        //        {
        //            if (OfficiatingParameterlist == null || OfficiatingParameterlist == "0")
        //            {
        //                Msg.Add("Please Select the Officiating Parameter");
        //                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //            }

        //            var OffProcess = officiateprocess(OEmpPayroll, PayMonth, CalAmount, SalHeadoperationtype, offid, OfficiatingParameterlist, SalaryHeadId);

        //            double offPro = Convert.ToDouble(OffProcess);

        //            return Json(offPro, JsonRequestBehavior.AllowGet);
        //        }

        //    }
        //    return View();
        //}

        public static double officiateprocess(BMSPaymentReq OEmpPayroll, string PayMonth, double CalAmount, string OfficiatingParameterlist, int offid)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {

                    using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                       new System.TimeSpan(0, 30, 0)))
                    {


                        // officiate code start
                        List<SalEarnDedT> OSalEarnDedToff_Final = new List<SalEarnDedT>();
                        List<SalEarnDedT> OSalEarnDedToffcur_Final = new List<SalEarnDedT>();
                        PFECRR OPFTransTcur = new PFECRR();
                        OPFTransTcur = null;
                        PFECRR OPFTransToff = new PFECRR();
                        OPFTransToff = null;
                        int OCompanyPayrollId = Convert.ToInt32(SessionManager.CompPayrollId);
                        var OCompanyPayroll = db.CompanyPayroll
                                              .Include(e => e.Company)
                                              .Include(e => e.PFMaster)
                                              .Include(e => e.PTaxMaster)
                                              .Include(e => e.LWFMaster)
                                              .Include(e => e.LWFMaster.Select(t => t.LWFStates))
                                              .Include(e => e.LWFMaster.Select(t => t.LWFStatutoryEffectiveMonths))
                                              .Include(e => e.ESICMaster)
                                              .Include(e => e.PTaxMaster.Select(a => a.States))
                                              .Where(d => d.Id == OCompanyPayrollId).FirstOrDefault();

                        var OEmpOff = db.EmployeePayroll.Where(r => r.Id == OEmpPayroll.EmployeePayroll.Id)
                                               .Select(e => new
                                               {
                                                   PFAppl = e.Employee.EmpOffInfo.PFAppl,
                                                   LWFAppl = e.Employee.EmpOffInfo.LWFAppl,
                                                   ESICAppl = e.Employee.EmpOffInfo.ESICAppl,
                                                   PTAppl = e.Employee.EmpOffInfo.PTAppl,
                                                   Branch = e.Employee.EmpOffInfo.Branch,
                                                   AccountType = e.Employee.EmpOffInfo.AccountType,
                                                   AccountNo = e.Employee.EmpOffInfo.AccountNo,
                                                   PayMode = e.Employee.EmpOffInfo.PayMode,
                                                   UANNo = e.Employee.EmpOffInfo.NationalityID.UANNo,
                                                   PFTrust_EstablishmentId = e.Employee.EmpOffInfo.PFTrust_EstablishmentId

                                               }).FirstOrDefault();

                        double officiateamtTotal = 0;

                        //delete the null records from table 






                        //delete if process

                        var OEmpSalT = db.BMSPaymentReq.Include(e => e.EmployeePayroll).Include(e => e.OfficiatingPaymentT).Include(e => e.EmployeePayroll.Employee).Where(e => e.Id == offid && e.EmployeePayroll_Id == OEmpPayroll.EmployeePayroll.Id).ToList();
                        var EmpSalTp = OEmpSalT.Where(e => e.OfficiatingPaymentT.Count > 0).SingleOrDefault();
                        if (EmpSalTp != null)
                        {

                            BMSPaymentReq SAT = db.BMSPaymentReq.Include(q => q.OfficiatingPaymentT).Where(q => q.Id == offid).SingleOrDefault();

                            SAT.OfficiatingPaymentT = null;
                            SAT.OfficiatingPFT = null;
                            db.BMSPaymentReq.Attach(SAT);
                            db.Entry(SAT).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            db.Entry(SAT).State = System.Data.Entity.EntityState.Detached;
                        }


                        var OfficiatingPayments = db.OfficiatingPaymentT
                          .Where(e => !db.BMSPaymentReq.Any(a => a.OfficiatingPaymentT.Contains(e)))
                          .ToList();

                        if (OfficiatingPayments.Any())
                        {

                            db.OfficiatingPaymentT.RemoveRange(OfficiatingPayments);
                            db.SaveChanges();
                        }


                        //if (OfficiatingPaymentT != null)
                        //{
                        //    var Objjob = new List<BMSPaymentReq>();
                        //    Objjob.Add(BMSPaymentReq);
                        //    Company.Location = Objjob;
                        //    var aa = ValidateObj(Company);
                        //    // var aa = ValidateObj(Company);
                        //    // var aa = ValidateObj(Company);
                        //    if (aa.Count > 0)
                        //    {
                        //        foreach (var item in aa)
                        //        {

                        //            Msg.Add("Company" + item);
                        //        }
                        //        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //    }
                        //    db.Company.Attach(Company);
                        //    db.Entry(Company).State = System.Data.Entity.EntityState.Modified;
                        //    db.SaveChanges();
                        //    db.Entry(Company).State = System.Data.Entity.EntityState.Detached;

                        //}



                        //var offservicebook = db.OfficiatingServiceBook.Where(e => e.EmployeeId == OEmpPayroll.Employee.Id && e.Release == true && e.PayMonth == PayMonth).ToList();
                        var offservicebook = db.BMSPaymentReq.Include(e => e.EmployeePayroll).Include(e => e.EmployeePayroll.Employee).Where(e => e.Id == offid && e.EmployeePayroll_Id == OEmpPayroll.EmployeePayroll.Id).ToList();

                        foreach (var offserid in offservicebook)
                        {
                            Boolean supannapplicable = false;
                            //OfficiatingServiceBook OfficiatingServiceBook1 = db.OfficiatingServiceBook.Include(e => e.OfficiatingParameter).Where(e => e.Id == offserid.Id).SingleOrDefault();
                            BMSPaymentReq OfficiatingServiceBook1 = db.BMSPaymentReq.Include(e => e.EmployeePayroll).Include(e => e.EmployeePayroll.Employee).Include(e => e.OfficiatingParameter).Where(e => e.Id == offserid.Id).SingleOrDefault();

                            var Empofficate = OfficiatingServiceBook1;
                            if (Empofficate != null)
                            {
                                List<SalEarnDedTMultiStructure> OSalEarnDedTMultiStructureoff = new List<SalEarnDedTMultiStructure>();


                                int onoffemployee = Empofficate.OfficiatingEmployeeId != 0 ? Empofficate.OfficiatingEmployeeId : Empofficate.EmployeePayroll.Employee.Id;


                                //int onoffemployeecur = Empofficate.EmployeeId;

                                EmployeePayroll oEmployeePayrolloffobj = db.EmployeePayroll
                                         .Include(e => e.Employee.EmpName)
                                         .Include(e => e.Employee.ServiceBookDates)
                                         .Where(e => e.Employee.Id == onoffemployee)
                                         .FirstOrDefault();
                                int oEmployeePayrolloff = oEmployeePayrolloffobj.Id;
                                //int oEmployeePayrolloff = oEmployeePayrolloffobj == null ? 0 : oEmployeePayrolloffobj.Id;

                                int onoffemployeecur = Convert.ToInt32(Empofficate.EmployeePayroll_Id);

                                EmployeePayroll oEmployeePayrolloffobjcur = db.EmployeePayroll
                                       .Include(e => e.Employee.EmpName)
                                       .Include(e => e.Employee.ServiceBookDates)
                                       .Where(e => e.Id == onoffemployeecur)
                                       .FirstOrDefault();
                                int oEmployeePayrolloffcur = oEmployeePayrolloffobjcur.Id;

                                //DateTime mofffrom = Convert.ToDateTime(Empofficate.FromDate.ToString());
                                //DateTime moffto = Convert.ToDateTime(Empofficate.ToDate.ToString());
                                DateTime mofffrom = Convert.ToDateTime(Empofficate.FromPeriod.ToString());
                                DateTime moffto = Convert.ToDateTime(Empofficate.ToPeriod.ToString());

                                int monthsApart = 12 * (moffto.Year - mofffrom.Year) + moffto.Month - mofffrom.Month;
                                double offemptotal = 0;
                                double Actualemptotal = 0;
                                for (int iq = 0; iq <= monthsApart; iq++)
                                {
                                    DateTime Dofffrom = DateTime.Now;
                                    DateTime DoffTo = DateTime.Now;
                                    if (iq == 0)
                                    {
                                        Dofffrom = mofffrom;
                                    }
                                    else
                                    {
                                        var DArrfrom1 = mofffrom.AddMonths(iq);
                                        var Fd = new DateTime(DArrfrom1.Year, DArrfrom1.Month, 1);
                                        Dofffrom = Fd;
                                    }

                                    if (moffto < new DateTime(Dofffrom.Year, Dofffrom.Month, 1).AddMonths(1).AddDays(-1))
                                    {
                                        DoffTo = moffto;
                                    }
                                    else
                                    {
                                        DoffTo = new DateTime(Dofffrom.Year, Dofffrom.Month, 1).AddMonths(1).AddDays(-1);

                                    }


                                    var comparedateoff = (Convert.ToDateTime("01/" + Dofffrom.ToString("MM/yyyy")).Date);
                                    var comparedateoffend = new DateTime(Dofffrom.Year, Dofffrom.Month, 1).AddMonths(1).AddDays(-1);
                                    double SalAttendanceT_monthDaysoff = comparedateoffend.Day;

                                    // offemptotal salary Start
                                    List<EmpSalStruct> EmpSalStructTotaloff = new List<EmpSalStruct>();
                                    List<EmpSalStructDetails> EmpSalStructDetailsoff = new List<EmpSalStructDetails>();
                                    List<PayScaleAssignment> PayScaleAssignmentoff = new List<PayScaleAssignment>();
                                    var PayScaleAssignmentObjoff = new PayScaleAssignment();
                                    List<SalaryHead> SalaryHeadoff = new List<SalaryHead>();
                                    var SalaryHeadObjoff = new SalaryHead();
                                    List<SalHeadFormula> SalHeadFormulaoff = new List<SalHeadFormula>();
                                    var SalaryHeadFormulaObjoff = new SalHeadFormula();
                                    int offparaid = Convert.ToInt32(OfficiatingParameterlist);
                                    var offciatepay = db.OfficiatingParameter.Where(e => e.Id == offparaid).FirstOrDefault();
                                    if (offciatepay.OfficiatingEmpPayStructAppl == true)
                                    {


                                        EmpSalStructTotaloff = db.EmpSalStruct.Include(e => e.EmpSalStructDetails).Include(e => e.EmpSalStructDetails.Select(a => a.SalHeadFormula)).Where(e => e.EmployeePayroll.Id == oEmployeePayrolloff && e.EffectiveDate >= comparedateoff && e.EffectiveDate <= comparedateoffend).ToList();
                                        foreach (var i in EmpSalStructTotaloff)
                                        {
                                            EmpSalStructDetailsoff = db.EmpSalStructDetails.Include(e => e.SalaryHead.SalHeadOperationType)
                                                .Where(e => e.EmpSalStruct.Id == i.Id).ToList();
                                            foreach (var j in EmpSalStructDetailsoff)
                                            {

                                                var SalHeadTmp = new SalaryHead();
                                                j.PayScaleAssignment = db.PayScaleAssignment.Where(e => e.Id == j.PayScaleAssignment_Id).FirstOrDefault();
                                                j.SalHeadFormula = db.SalHeadFormula.Where(e => e.Id == j.SalHeadFormula_Id).FirstOrDefault();
                                                j.SalaryHead = db.SalaryHead.Where(e => e.Id == j.SalaryHead_Id).FirstOrDefault();
                                                var id = db.EmpSalStructDetails.Include(e => e.SalaryHead).Where(r => r.Id == j.Id).Select(r => r.SalaryHead.Id).FirstOrDefault();
                                                var SalHeadData = db.SalaryHead.Where(e => e.Id == j.SalaryHead_Id)
                                                    .Select(r => new
                                                    {
                                                        Id = r.Id,
                                                        SalHeadOperationType = r.SalHeadOperationType,
                                                        Frequency = r.Frequency,
                                                        Type = r.Type,
                                                        RoundingMethod = r.RoundingMethod,
                                                        ProcessType = r.ProcessType,
                                                        LvHead = r.LvHead

                                                    }).FirstOrDefault();
                                                //For NKGSB Code Comment
                                                //if (SalHeadData.SalHeadOperationType.LookupVal.ToUpper() == "OFFICIATING")
                                                //{
                                                //    j.SalHeadFormula = db.SalHeadFormula.Where(e => e.PayStruct_Id == i.PayStruct_Id && e.Name.ToUpper() == "OFFICIATING").FirstOrDefault();
                                                //}


                                                if (SalHeadData.SalHeadOperationType.LookupVal.ToUpper() == "BASIC")
                                                {
                                                    if (offciatepay.ScaleFirstBasic == true)
                                                    {
                                                        var OSalHeadFormulaResult = j.SalHeadFormula;
                                                        List<BasicScaleDetails> OBasicScale = null;
                                                        if (OSalHeadFormulaResult != null)
                                                        {
                                                            var OSalHeadFormula = db.SalHeadFormula
                                                                                .Include(e => e.BASICDependRule)
                                                                                .Include(e => e.BASICDependRule.BasicScale)
                                                                                .Include(e => e.BASICDependRule.BasicScale.BasicScaleDetails)
                                                                                .Where(e => e.Id == OSalHeadFormulaResult.Id).SingleOrDefault();

                                                            OBasicScale = OSalHeadFormula.BASICDependRule.BasicScale.BasicScaleDetails
                                                                                        .Select(t => new BasicScaleDetails
                                                                                        {
                                                                                            StartingSlab = t.StartingSlab,
                                                                                            EndingSlab = t.EndingSlab,
                                                                                            IncrementAmount = t.IncrementAmount,
                                                                                            IncrementCount = t.IncrementCount,
                                                                                            EBMark = t.EBMark,
                                                                                        }).ToList();



                                                            OBasicScale = OBasicScale.OrderBy(e => e.StartingSlab).ToList();
                                                            double mNewBasic = 0;

                                                            foreach (var OBasicScaleRange in OBasicScale)
                                                            {
                                                                mNewBasic = OBasicScaleRange.StartingSlab;
                                                                break;
                                                            }
                                                            j.Amount = mNewBasic;// scale first basic


                                                        }

                                                    }
                                                }
                                                j.SalaryHead.SalHeadOperationType = SalHeadData.SalHeadOperationType;
                                                j.SalaryHead.Frequency = SalHeadData.Frequency;
                                                j.SalaryHead.Type = SalHeadData.Type;
                                                j.SalaryHead.RoundingMethod = SalHeadData.RoundingMethod;
                                                j.SalaryHead.ProcessType = SalHeadData.ProcessType;
                                                SalHeadTmp.LeaveDependPolicy = db.SalaryHead.Where(e => e.Id == id).Select(e => e.LeaveDependPolicy).FirstOrDefault();
                                                foreach (var item in SalHeadTmp.LeaveDependPolicy)
                                                {
                                                    item.LvHead = db.LeaveDependPolicy.Where(e => e.Id == item.Id).Select(t => t.LvHead).FirstOrDefault();
                                                }


                                            }

                                            i.EmpSalStructDetails = EmpSalStructDetailsoff;

                                            Process.SalaryHeadGenProcess.EmployeeSalaryStructUpdateNew(i, oEmployeePayrolloffobj, i.EffectiveDate);
                                        }

                                    }
                                    else // ofiiciate on selected grade/shift grade
                                    {
                                        // new grade sturcture create and formula update code start


                                        int structid = 0;
                                        List<EmpSalStruct> EmpSalStructTotaloffsel = new List<EmpSalStruct>();
                                        EmpSalStructTotaloffsel = db.EmpSalStruct.Where(e => e.EmployeePayroll.Id == oEmployeePayrolloffcur && e.EffectiveDate >= comparedateoff && e.EffectiveDate <= comparedateoffend).ToList();

                                        foreach (var shiftgrade in EmpSalStructTotaloffsel)
                                        {
                                            var OEmpSalStructDetailsNew = new List<EmpSalStructDetails>();
                                            EmpSalStruct OEmpSalStructNew = new EmpSalStruct();
                                            DateTime? mEffectiveDate = shiftgrade.EffectiveDate;
                                            structid = structid + 1;
                                            List<EmpSalStructDetails> OEmpSalStructDetailsCurrent1 = db.EmpSalStructDetails
                                                .Include(e => e.SalaryHead)
                                                .Include(e => e.SalaryHead.SalHeadOperationType)
                                                  .Include(e => e.SalaryHead.RoundingMethod)
                                                .Include(e => e.SalHeadFormula)
                                                .Include(e => e.PayScaleAssignment)
                                                .Include(e => e.PayScaleAssignment.SalHeadFormula)
                                                .Include(e => e.PayScaleAssignment.SalHeadFormula.Select(t => t.GeoStruct))
                                                 .Include(e => e.PayScaleAssignment.SalHeadFormula.Select(t => t.PayStruct))
                                                  .Include(e => e.PayScaleAssignment.SalHeadFormula.Select(t => t.FuncStruct))
                                                    .Include(e => e.PayScaleAssignment.SalHeadFormula.Select(t => t.FormulaType))
                                                .Where(e => e.EmpSalStruct_Id == shiftgrade.Id).ToList();
                                            OEmpSalStructNew.Id = structid;
                                            OEmpSalStructNew.EffectiveDate = shiftgrade.EffectiveDate;
                                            OEmpSalStructNew.EndDate = shiftgrade.EndDate;
                                            OEmpSalStructNew.GeoStruct = db.GeoStruct.Find(shiftgrade.GeoStruct_Id);
                                            OEmpSalStructNew.FuncStruct = db.FuncStruct.Find(shiftgrade.FuncStruct_Id);
                                            //OEmpSalStructNew.PayStruct = db.PayStruct.Where(e => e.Id == Empofficate.StructureRefId).SingleOrDefault();
                                            OEmpSalStructNew.PayStruct = db.PayStruct.Where(e => e.Id == Empofficate.OfficiatingPayStruct).SingleOrDefault();
                                            foreach (var ca in OEmpSalStructDetailsCurrent1)
                                            {

                                                var OEmpSalStructDetail = new EmpSalStructDetails();
                                                OEmpSalStructDetail.Amount = ca.Amount;
                                                OEmpSalStructDetail.DBTrack = ca.DBTrack;
                                                OEmpSalStructDetail.PayScaleAssignment = ca.PayScaleAssignment;
                                                OEmpSalStructDetail.SalaryHead = ca.SalaryHead;
                                                OEmpSalStructDetail.SalHeadFormula = Process.SalaryHeadGenProcess.SalFormulaFinderNew(OEmpSalStructNew, ca.PayScaleAssignment, ca.SalaryHead.Id);// ca.SalHeadFormula;
                                                OEmpSalStructDetailsNew.Add(OEmpSalStructDetail);
                                            }
                                            OEmpSalStructNew.EmpSalStructDetails = OEmpSalStructDetailsNew;
                                            Process.SalaryHeadGenProcess.EmployeeSalaryStructUpdateNew(OEmpSalStructNew, oEmployeePayrolloffobjcur, mEffectiveDate);
                                            EmpSalStructTotaloff.Add(OEmpSalStructNew);

                                        }
                                        // new grade sturcture create and formula update code end

                                        // new grade sturcture basic update(increment) dependent head value update start
                                        foreach (var i in EmpSalStructTotaloff)
                                        {

                                            foreach (var j in i.EmpSalStructDetails)
                                            {

                                                var SalHeadTmp = new SalaryHead();
                                                j.PayScaleAssignment = db.PayScaleAssignment.Where(e => e.Id == j.PayScaleAssignment.Id).FirstOrDefault();
                                                if (j.SalHeadFormula != null)
                                                {
                                                    j.SalHeadFormula = db.SalHeadFormula.Where(e => e.Id == j.SalHeadFormula.Id).FirstOrDefault();
                                                }
                                                else
                                                {
                                                    j.SalHeadFormula = null;
                                                }
                                                j.SalaryHead = db.SalaryHead.Where(e => e.Id == j.SalaryHead.Id).FirstOrDefault();
                                                //  var id = db.EmpSalStructDetails.Include(e => e.SalaryHead).Where(r => r.Id == j.Id).Select(r => r.SalaryHead.Id).FirstOrDefault();
                                                var SalHeadData = db.SalaryHead.Where(e => e.Id == j.SalaryHead.Id)
                                                    .Select(r => new
                                                    {
                                                        Id = r.Id,
                                                        SalHeadOperationType = r.SalHeadOperationType,
                                                        Frequency = r.Frequency,
                                                        Type = r.Type,
                                                        RoundingMethod = r.RoundingMethod,
                                                        ProcessType = r.ProcessType,
                                                        LvHead = r.LvHead

                                                    }).FirstOrDefault();


                                                if (SalHeadData.SalHeadOperationType.LookupVal.ToUpper() == "BASIC")
                                                {
                                                    if (offciatepay.NewGradeBasicAppl == true)
                                                    {
                                                        var OSalHeadFormulaResult = j.SalHeadFormula;
                                                        List<BasicScaleDetails> OBasicScale = null;
                                                        if (OSalHeadFormulaResult != null)
                                                        {


                                                            var OSalHeadFormula = db.SalHeadFormula
                                                                                .Include(e => e.BASICDependRule)
                                                                                .Include(e => e.BASICDependRule.BasicScale)
                                                                                .Include(e => e.BASICDependRule.BasicScale.BasicScaleDetails)
                                                                                .Where(e => e.Id == OSalHeadFormulaResult.Id).SingleOrDefault();

                                                            OBasicScale = OSalHeadFormula.BASICDependRule.BasicScale.BasicScaleDetails
                                                                                        .Select(t => new BasicScaleDetails
                                                                                        {
                                                                                            StartingSlab = t.StartingSlab,
                                                                                            EndingSlab = t.EndingSlab,
                                                                                            IncrementAmount = t.IncrementAmount,
                                                                                            IncrementCount = t.IncrementCount,
                                                                                            EBMark = t.EBMark,
                                                                                        }).ToList();



                                                            OBasicScale = OBasicScale.OrderBy(e => e.StartingSlab).ToList();
                                                            double mNewBasic = 0;
                                                            double mOldBasic = j.Amount;
                                                            List<BasicScaleDetails> OBasicScaleNew = OBasicScale;

                                                            double mFittmentAmount = Process.ServiceBook.BasicFittmentSelector(mOldBasic, OBasicScaleNew);

                                                            mOldBasic = mOldBasic + mFittmentAmount;

                                                            for (int k = 0; k < offciatepay.IncrementCount; k++)
                                                            {
                                                                foreach (var OBasicScaleRange in OBasicScale)
                                                                {
                                                                    mNewBasic = mOldBasic + OBasicScaleRange.IncrementAmount;
                                                                    if (mNewBasic > OBasicScaleRange.StartingSlab && mNewBasic <= OBasicScaleRange.EndingSlab)
                                                                    {
                                                                        mOldBasic = mNewBasic;
                                                                        break;
                                                                    }
                                                                    else if (mNewBasic <= OBasicScaleRange.StartingSlab)
                                                                    {
                                                                        mNewBasic = OBasicScaleRange.StartingSlab;
                                                                        break;
                                                                    }
                                                                    else if (mNewBasic > OBasicScaleRange.EndingSlab)
                                                                    {
                                                                        mNewBasic = mOldBasic;

                                                                    }
                                                                }
                                                            }

                                                            j.Amount = mNewBasic;// scale first basic


                                                        }

                                                    }
                                                }
                                                j.SalaryHead.SalHeadOperationType = SalHeadData.SalHeadOperationType;
                                                j.SalaryHead.Frequency = SalHeadData.Frequency;
                                                j.SalaryHead.Type = SalHeadData.Type;
                                                j.SalaryHead.RoundingMethod = SalHeadData.RoundingMethod;
                                                j.SalaryHead.ProcessType = SalHeadData.ProcessType;
                                                SalHeadTmp.LeaveDependPolicy = db.SalaryHead.Where(e => e.Id == j.SalaryHead.Id).Select(e => e.LeaveDependPolicy).FirstOrDefault();
                                                foreach (var item in SalHeadTmp.LeaveDependPolicy)
                                                {
                                                    item.LvHead = db.LeaveDependPolicy.Where(e => e.Id == item.Id).Select(t => t.LvHead).FirstOrDefault();
                                                }

                                            }

                                            i.EmpSalStructDetails = i.EmpSalStructDetails;

                                            Process.SalaryHeadGenProcess.EmployeeSalaryStructUpdateNew(i, oEmployeePayrolloffobj, i.EffectiveDate);
                                        }
                                        // new grade sturcture basic update(increment) dependent head value update  end


                                    }


                                    List<EmpSalStruct> mEmpSalStructTotaloff = EmpSalStructTotaloff.Where(e => e.EffectiveDate.Value.Date >= comparedateoff && e.EffectiveDate.Value.Date <= comparedateoffend)
                                                         .OrderBy(r => r.EffectiveDate)
                                                         .ToList();
                                    List<SalEarnDedT> OSalEarnDedToff = new List<SalEarnDedT>();
                                    if (mEmpSalStructTotaloff.Count() == 1)
                                    {


                                        foreach (EmpSalStruct OMultiEmpStruct in mEmpSalStructTotaloff)
                                        {
                                            int mPayDaysRunningoff = 0;
                                            mPayDaysRunningoff = (DoffTo.Date - Dofffrom.Date).Days + 1;
                                            // find salaryhead for officiating start
                                            List<int> OSalheadaddinwg = new List<int>();
                                            double headwisepercentage = 0;
                                            var OEmpsalheadoff = OMultiEmpStruct.EmpSalStructDetails.Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "OFFICIATING").FirstOrDefault();
                                            if (OEmpsalheadoff != null)
                                            {
                                                if (OEmpsalheadoff.SalHeadFormula.Id != null)
                                                {
                                                    var OEmpsalheadd = OMultiEmpStruct.EmpSalStructDetails.OrderBy(e => e.SalaryHead.SeqNo).ToList();
                                                    var OSalHeadFormualQuery = Process.SalaryHeadGenProcess._returnSalHeadFormula(OEmpsalheadoff.SalHeadFormula.Id);
                                                    var OSalHeadFormualQueryr = db.SalHeadFormula.Where(e => e.Id == OSalHeadFormualQuery.Id)
                                                        .Include(e => e.SalWages)
                                                         .Include(e => e.SalWages.RateMaster)
                                                         .Include(e => e.SalWages.RateMaster.Select(r => r.SalHead)).FirstOrDefault();

                                                    var assignhead = OSalHeadFormualQueryr.SalWages.RateMaster
                                                        .Join(OEmpsalheadd, u => u.SalHead.Id, uir => uir.SalaryHead.Id,
                                                          (u, uir) => new { u, uir })
                                                             .ToList();

                                                    foreach (var item in assignhead)
                                                    {
                                                        OSalheadaddinwg.Add(item.uir.SalaryHead.Id);
                                                    }
                                                    headwisepercentage = OSalHeadFormualQueryr.SalWages.Percentage;


                                                }
                                            }
                                            // find salaryhead for officiating end
                                            var OEmpsalhead = OMultiEmpStruct.EmpSalStructDetails.Where(e => OSalheadaddinwg.Contains(e.SalaryHead.Id)).OrderBy(e => e.SalaryHead.SeqNo).ToList();


                                            var OPayProcGrp = db.PayProcessGroup.Include(e => e.PayMonthConcept).Include(e => e.PayrollPeriod).Include(e => e.PayFrequency).SingleOrDefault();
                                            foreach (var OSalStructDetails in OEmpsalhead)
                                            {
                                                //if (OSalStructDetails.SalHeadFormula != null && OSalStructDetails.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == SalHeadoperationtype)
                                                //{
                                                double SalAmount = 0;
                                                bool appearhead = false;
                                                appearhead = OSalStructDetails.SalaryHead.OnAttend;
                                                // SalAmount = SalHeadAmountCalc(OSalStructDetails.SalHeadFormula.Id, null, OEmpsalhead, oEmployeePayrolloffobj, OMultiEmpStruct.EffectiveDate.Value.ToString("MM/yyyy"), false);
                                                SalAmount = OSalStructDetails.Amount * (headwisepercentage / 100);
                                                SalAmount = Math.Round(SalAmount + 0.001, 0);
                                                //SalAmount = OSalStructDetails.Amount;
                                                //offemptotal
                                                // offemptotal = offemptotal + offAmountCalc(SalAmount, appearhead, mPayDaysRunningoff, SalAttendanceT_monthDaysoff, Dofffrom.Date);
                                                offemptotal = offAmountCalc(SalAmount, appearhead, mPayDaysRunningoff, SalAttendanceT_monthDaysoff, Dofffrom.Date);

                                                DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };

                                                SalEarnDedT OSalEarnDedTObj = new SalEarnDedT()
                                                {
                                                    Amount = offemptotal,
                                                    StdAmount = offemptotal,
                                                    SalaryHead = OSalStructDetails.SalaryHead,
                                                    DBTrack = dbt

                                                };
                                                OSalEarnDedToff.Add(OSalEarnDedTObj);//add ref



                                                // }
                                            }

                                        }

                                    }
                                    else
                                    {


                                        foreach (EmpSalStruct OMultiEmpStruct in mEmpSalStructTotaloff)
                                        {
                                            int mPayDaysRunningoff = 0;

                                            if (OMultiEmpStruct.EffectiveDate.Value.Date <= Dofffrom.Date && OMultiEmpStruct.EndDate != null)
                                            {
                                                mPayDaysRunningoff = (OMultiEmpStruct.EndDate.Value.Date - Dofffrom.Date).Days + 1;

                                            }
                                            else
                                            {
                                                if (OMultiEmpStruct.EndDate != null && OMultiEmpStruct.EffectiveDate.Value.Date <= DoffTo.Date)
                                                {
                                                    mPayDaysRunningoff = (DoffTo.Date - OMultiEmpStruct.EffectiveDate.Value.Date).Days + 1;
                                                }
                                                else
                                                {
                                                    mPayDaysRunningoff = (DoffTo.Date - OMultiEmpStruct.EffectiveDate.Value.Date).Days + 1;
                                                }
                                            }
                                            Dofffrom = Dofffrom.Date.AddDays(mPayDaysRunningoff);

                                            // find salaryhead for officiating start
                                            List<int> OSalheadaddinwg = new List<int>();
                                            double headwisepercentage = 0;
                                            var OEmpsalheadoff = OMultiEmpStruct.EmpSalStructDetails.Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "OFFICIATING").FirstOrDefault();
                                            if (OEmpsalheadoff != null)
                                            {
                                                if (OEmpsalheadoff.SalHeadFormula.Id != null)
                                                {
                                                    var OEmpsalheadd = OMultiEmpStruct.EmpSalStructDetails.OrderBy(e => e.SalaryHead.SeqNo).ToList();
                                                    var OSalHeadFormualQuery = Process.SalaryHeadGenProcess._returnSalHeadFormula(OEmpsalheadoff.SalHeadFormula.Id);
                                                    var OSalHeadFormualQueryr = db.SalHeadFormula.Where(e => e.Id == OSalHeadFormualQuery.Id)
                                                        .Include(e => e.SalWages)
                                                         .Include(e => e.SalWages.RateMaster)
                                                         .Include(e => e.SalWages.RateMaster.Select(r => r.SalHead)).FirstOrDefault();

                                                    var assignhead = OSalHeadFormualQueryr.SalWages.RateMaster
                                                        .Join(OEmpsalheadd, u => u.SalHead.Id, uir => uir.SalaryHead.Id,
                                                          (u, uir) => new { u, uir })
                                                             .ToList();

                                                    foreach (var item in assignhead)
                                                    {
                                                        OSalheadaddinwg.Add(item.uir.SalaryHead.Id);
                                                    }
                                                    headwisepercentage = OSalHeadFormualQueryr.SalWages.Percentage;

                                                }
                                            }
                                            // find salaryhead for officiating end

                                            var OEmpsalhead = OMultiEmpStruct.EmpSalStructDetails.Where(e => OSalheadaddinwg.Contains(e.SalaryHead.Id)).OrderBy(e => e.SalaryHead.SeqNo).ToList();

                                            foreach (var OSalStructDetails in OEmpsalhead)
                                            {
                                                //if (OSalStructDetails.SalHeadFormula != null && OSalStructDetails.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == SalHeadoperationtype)
                                                //{
                                                double SalAmount = 0;
                                                bool appearhead = false;
                                                appearhead = OSalStructDetails.SalaryHead.OnAttend;
                                                //   SalAmount = SalHeadAmountCalc(OSalStructDetails.SalHeadFormula.Id, null, OEmpsalhead, oEmployeePayrolloffobj, OMultiEmpStruct.EffectiveDate.Value.ToString("MM/yyyy"), false);

                                                SalAmount = OSalStructDetails.Amount * (headwisepercentage / 100);
                                                SalAmount = Math.Round(SalAmount + 0.001, 0);
                                                // SalAmount = OSalStructDetails.Amount;
                                                //offemptotal
                                                //  offemptotal = offemptotal + offAmountCalc(SalAmount, appearhead, mPayDaysRunningoff, SalAttendanceT_monthDaysoff, Dofffrom.Date);
                                                offemptotal = offAmountCalc(SalAmount, appearhead, mPayDaysRunningoff, SalAttendanceT_monthDaysoff, Dofffrom.Date);

                                                DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };

                                                SalEarnDedT OSalEarnDedTObj = new SalEarnDedT()
                                                {
                                                    Amount = offemptotal,
                                                    StdAmount = offemptotal,
                                                    SalaryHead = OSalStructDetails.SalaryHead,
                                                    DBTrack = dbt

                                                };
                                                OSalEarnDedToff.Add(OSalEarnDedTObj);//add ref



                                                // }
                                            }
                                        }

                                    }

                                    // offemptotal salary end


                                    var OSalEarnDedTMultioff = OSalEarnDedToff.GroupBy(x => new { x.SalaryHead })
                                                       .Select(g => new
                                                       {
                                                           SalaryHead = g.Key.SalaryHead,
                                                           StdAmount = (g.Sum(x => x.StdAmount)) / g.Count(),
                                                           TotalAmount = g.Sum(x => x.Amount),
                                                           Count = g.Count()
                                                       }).ToList();
                                    foreach (var ca in OSalEarnDedTMultioff)
                                    {
                                        OSalEarnDedToff_Final = SaveSalayDetails(OSalEarnDedToff_Final, ca.TotalAmount, ca.StdAmount, ca.SalaryHead);
                                    }
                                    // Currentemptotal salary end

                                    List<EmpSalStruct> EmpSalStructTotaloffcur = new List<EmpSalStruct>();
                                    List<EmpSalStructDetails> EmpSalStructDetailsoffcur = new List<EmpSalStructDetails>();
                                    List<PayScaleAssignment> PayScaleAssignmentoffcur = new List<PayScaleAssignment>();
                                    var PayScaleAssignmentObjoffcur = new PayScaleAssignment();
                                    List<SalaryHead> SalaryHeadoffcur = new List<SalaryHead>();
                                    var SalaryHeadObjoffcur = new SalaryHead();
                                    List<SalHeadFormula> SalHeadFormulaoffcur = new List<SalHeadFormula>();
                                    var SalaryHeadFormulaObjoffcur = new SalHeadFormula();

                                    EmpSalStructTotaloffcur = db.EmpSalStruct.Where(e => e.EmployeePayroll.Id == oEmployeePayrolloffcur && e.EffectiveDate >= comparedateoff && e.EffectiveDate <= comparedateoffend).ToList();
                                    foreach (var i in EmpSalStructTotaloffcur)
                                    {
                                        EmpSalStructDetailsoffcur = db.EmpSalStructDetails.Include(e => e.SalaryHead.SalHeadOperationType)
                                            .Where(e => e.EmpSalStruct.Id == i.Id).ToList();
                                        foreach (var j in EmpSalStructDetailsoffcur)
                                        {

                                            var SalHeadTmp = new SalaryHead();
                                            j.PayScaleAssignment = db.PayScaleAssignment.Where(e => e.Id == j.PayScaleAssignment_Id).FirstOrDefault();
                                            j.SalHeadFormula = db.SalHeadFormula.Where(e => e.Id == j.SalHeadFormula_Id).FirstOrDefault();
                                            j.SalaryHead = db.SalaryHead.Where(e => e.Id == j.SalaryHead_Id).FirstOrDefault();
                                            var id = db.EmpSalStructDetails.Include(e => e.SalaryHead).Where(r => r.Id == j.Id).Select(r => r.SalaryHead.Id).FirstOrDefault();
                                            var SalHeadData = db.SalaryHead.Where(e => e.Id == j.SalaryHead_Id)
                                                .Select(r => new
                                                {
                                                    Id = r.Id,
                                                    SalHeadOperationType = r.SalHeadOperationType,
                                                    Frequency = r.Frequency,
                                                    Type = r.Type,
                                                    RoundingMethod = r.RoundingMethod,
                                                    ProcessType = r.ProcessType,
                                                    LvHead = r.LvHead

                                                }).FirstOrDefault();
                                            //For NKGSB Code Comment
                                            //if (SalHeadData.SalHeadOperationType.LookupVal.ToUpper() == "OFFICIATING")
                                            //{
                                            //    j.SalHeadFormula = db.SalHeadFormula.Where(e => e.PayStruct_Id == i.PayStruct_Id && e.Name.ToUpper() == "OFFICIATING").FirstOrDefault();
                                            //}
                                            j.SalaryHead.SalHeadOperationType = SalHeadData.SalHeadOperationType;
                                            j.SalaryHead.Frequency = SalHeadData.Frequency;
                                            j.SalaryHead.Type = SalHeadData.Type;
                                            j.SalaryHead.RoundingMethod = SalHeadData.RoundingMethod;
                                            j.SalaryHead.ProcessType = SalHeadData.ProcessType;
                                            SalHeadTmp.LeaveDependPolicy = db.SalaryHead.Where(e => e.Id == id).Select(e => e.LeaveDependPolicy).FirstOrDefault();
                                            foreach (var item in SalHeadTmp.LeaveDependPolicy)
                                            {
                                                item.LvHead = db.LeaveDependPolicy.Where(e => e.Id == item.Id).Select(t => t.LvHead).FirstOrDefault();
                                            }


                                        }

                                        i.EmpSalStructDetails = EmpSalStructDetailsoffcur;


                                    }


                                    List<EmpSalStruct> mEmpSalStructTotaloffcur = EmpSalStructTotaloffcur.Where(e => e.EffectiveDate.Value.Date >= comparedateoff && e.EffectiveDate.Value.Date <= comparedateoffend)
                                                         .OrderBy(r => r.EffectiveDate)
                                                         .ToList();
                                    List<SalEarnDedT> OSalEarnDedToffcur = new List<SalEarnDedT>();
                                    if (mEmpSalStructTotaloffcur.Count() == 1)
                                    {
                                        foreach (EmpSalStruct OMultiEmpStruct in mEmpSalStructTotaloffcur)
                                        {
                                            int mPayDaysRunningoff = 0;
                                            mPayDaysRunningoff = (DoffTo.Date - Dofffrom.Date).Days + 1;
                                            // find salaryhead for officiating start
                                            List<int> OSalheadaddinwg = new List<int>();
                                            double headwisepercentage = 0;
                                            var OEmpsalheadoff = OMultiEmpStruct.EmpSalStructDetails.Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "OFFICIATING").FirstOrDefault();
                                            if (OEmpsalheadoff != null)
                                            {
                                                if (OEmpsalheadoff.SalHeadFormula.Id != null)
                                                {
                                                    var OEmpsalheadd = OMultiEmpStruct.EmpSalStructDetails.OrderBy(e => e.SalaryHead.SeqNo).ToList();
                                                    var OSalHeadFormualQuery = Process.SalaryHeadGenProcess._returnSalHeadFormula(OEmpsalheadoff.SalHeadFormula.Id);
                                                    var OSalHeadFormualQueryr = db.SalHeadFormula.Where(e => e.Id == OSalHeadFormualQuery.Id)
                                                        .Include(e => e.SalWages)
                                                         .Include(e => e.SalWages.RateMaster)
                                                         .Include(e => e.SalWages.RateMaster.Select(r => r.SalHead)).FirstOrDefault();

                                                    var assignhead = OSalHeadFormualQueryr.SalWages.RateMaster
                                                        .Join(OEmpsalheadd, u => u.SalHead.Id, uir => uir.SalaryHead.Id,
                                                          (u, uir) => new { u, uir })
                                                             .ToList();

                                                    foreach (var item in assignhead)
                                                    {
                                                        OSalheadaddinwg.Add(item.uir.SalaryHead.Id);
                                                    }
                                                    headwisepercentage = OSalHeadFormualQueryr.SalWages.Percentage;

                                                }
                                            }
                                            // find salaryhead for officiating end

                                            var OEmpsalhead = OMultiEmpStruct.EmpSalStructDetails.Where(e => OSalheadaddinwg.Contains(e.SalaryHead.Id)).OrderBy(e => e.SalaryHead.SeqNo).ToList();
                                            var OPayProcGrp = db.PayProcessGroup.Include(e => e.PayMonthConcept).Include(e => e.PayrollPeriod).Include(e => e.PayFrequency).SingleOrDefault();
                                            foreach (var OSalStructDetails in OEmpsalhead)
                                            {
                                                //if (OSalStructDetails.SalHeadFormula != null && OSalStructDetails.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == SalHeadoperationtype)
                                                //{
                                                double SalAmount = 0;
                                                bool appearhead = false;
                                                appearhead = OSalStructDetails.SalaryHead.OnAttend;
                                                // SalAmount = SalHeadAmountCalc(OSalStructDetails.SalHeadFormula.Id, null, OEmpsalhead, oEmployeePayrolloffobjcur, OMultiEmpStruct.EffectiveDate.Value.ToString("MM/yyyy"), false);
                                                SalAmount = OSalStructDetails.Amount * (headwisepercentage / 100);
                                                SalAmount = Math.Round(SalAmount + 0.001, 0);
                                                // SalAmount = OSalStructDetails.Amount;
                                                //Actualemptotal
                                                // Actualemptotal = Actualemptotal + offAmountCalc(SalAmount, appearhead, mPayDaysRunningoff, SalAttendanceT_monthDaysoff, Dofffrom.Date);

                                                Actualemptotal = offAmountCalc(SalAmount, appearhead, mPayDaysRunningoff, SalAttendanceT_monthDaysoff, Dofffrom.Date);

                                                DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };

                                                SalEarnDedT OSalEarnDedTObj = new SalEarnDedT()
                                                {
                                                    Amount = Actualemptotal,
                                                    StdAmount = Actualemptotal,
                                                    SalaryHead = OSalStructDetails.SalaryHead,
                                                    DBTrack = dbt

                                                };
                                                OSalEarnDedToffcur.Add(OSalEarnDedTObj);//add ref


                                                //}
                                            }
                                        }

                                    }
                                    else
                                    {


                                        foreach (EmpSalStruct OMultiEmpStruct in mEmpSalStructTotaloffcur)
                                        {
                                            if (iq == 0)
                                            {
                                                Dofffrom = mofffrom;
                                            }
                                            else
                                            {
                                                var DArrfrom1 = mofffrom.AddMonths(iq);
                                                var Fd = new DateTime(DArrfrom1.Year, DArrfrom1.Month, 1);
                                                Dofffrom = Fd;
                                            }
                                            if (moffto < new DateTime(Dofffrom.Year, Dofffrom.Month, 1).AddMonths(1).AddDays(-1))
                                            {
                                                DoffTo = moffto;
                                            }
                                            else
                                            {
                                                DoffTo = new DateTime(Dofffrom.Year, Dofffrom.Month, 1).AddMonths(1).AddDays(-1);

                                            }
                                            int mPayDaysRunningoff = 0;

                                            if (OMultiEmpStruct.EffectiveDate.Value.Date <= Dofffrom.Date && OMultiEmpStruct.EndDate != null)
                                            {
                                                mPayDaysRunningoff = (OMultiEmpStruct.EndDate.Value.Date - Dofffrom.Date).Days + 1;

                                            }
                                            else
                                            {
                                                if (OMultiEmpStruct.EndDate != null && OMultiEmpStruct.EffectiveDate.Value.Date <= DoffTo.Date)
                                                {
                                                    mPayDaysRunningoff = (DoffTo.Date - OMultiEmpStruct.EffectiveDate.Value.Date).Days + 1;
                                                }
                                                else
                                                {
                                                    mPayDaysRunningoff = (DoffTo.Date - OMultiEmpStruct.EffectiveDate.Value.Date).Days + 1;
                                                }
                                            }
                                            Dofffrom = Dofffrom.Date.AddDays(mPayDaysRunningoff);

                                            // find salaryhead for officiating start
                                            List<int> OSalheadaddinwg = new List<int>();
                                            double headwisepercentage = 0;
                                            var OEmpsalheadoff = OMultiEmpStruct.EmpSalStructDetails.Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "OFFICIATING").FirstOrDefault();
                                            if (OEmpsalheadoff != null)
                                            {
                                                if (OEmpsalheadoff.SalHeadFormula.Id != null)
                                                {
                                                    var OEmpsalheadd = OMultiEmpStruct.EmpSalStructDetails.OrderBy(e => e.SalaryHead.SeqNo).ToList();
                                                    var OSalHeadFormualQuery = Process.SalaryHeadGenProcess._returnSalHeadFormula(OEmpsalheadoff.SalHeadFormula.Id);
                                                    var OSalHeadFormualQueryr = db.SalHeadFormula.Where(e => e.Id == OSalHeadFormualQuery.Id)
                                                        .Include(e => e.SalWages)
                                                         .Include(e => e.SalWages.RateMaster)
                                                         .Include(e => e.SalWages.RateMaster.Select(r => r.SalHead)).FirstOrDefault();

                                                    var assignhead = OSalHeadFormualQueryr.SalWages.RateMaster
                                                        .Join(OEmpsalheadd, u => u.SalHead.Id, uir => uir.SalaryHead.Id,
                                                          (u, uir) => new { u, uir })
                                                             .ToList();

                                                    foreach (var item in assignhead)
                                                    {
                                                        OSalheadaddinwg.Add(item.uir.SalaryHead.Id);
                                                    }
                                                    headwisepercentage = OSalHeadFormualQueryr.SalWages.Percentage;

                                                }
                                            }
                                            // find salaryhead for officiating end

                                            var OEmpsalhead = OMultiEmpStruct.EmpSalStructDetails.Where(e => OSalheadaddinwg.Contains(e.SalaryHead.Id)).OrderBy(e => e.SalaryHead.SeqNo).ToList();

                                            foreach (var OSalStructDetails in OEmpsalhead)
                                            {
                                                //if (OSalStructDetails.SalHeadFormula != null && OSalStructDetails.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == SalHeadoperationtype)
                                                //{
                                                double SalAmount = 0;
                                                bool appearhead = false;
                                                appearhead = OSalStructDetails.SalaryHead.OnAttend;
                                                // SalAmount = SalHeadAmountCalc(OSalStructDetails.SalHeadFormula.Id, null, OEmpsalhead, oEmployeePayrolloffobjcur, OMultiEmpStruct.EffectiveDate.Value.ToString("MM/yyyy"), false);
                                                SalAmount = OSalStructDetails.Amount * (headwisepercentage / 100);
                                                SalAmount = Math.Round(SalAmount + 0.001, 0);
                                                // SalAmount = OSalStructDetails.Amount;
                                                //Actualemptotal
                                                // Actualemptotal = Actualemptotal + offAmountCalc(SalAmount, appearhead, mPayDaysRunningoff, SalAttendanceT_monthDaysoff, Dofffrom.Date);
                                                Actualemptotal = offAmountCalc(SalAmount, appearhead, mPayDaysRunningoff, SalAttendanceT_monthDaysoff, Dofffrom.Date);

                                                DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };

                                                SalEarnDedT OSalEarnDedTObj = new SalEarnDedT()
                                                {
                                                    Amount = Actualemptotal,
                                                    StdAmount = Actualemptotal,
                                                    SalaryHead = OSalStructDetails.SalaryHead,
                                                    DBTrack = dbt

                                                };
                                                OSalEarnDedToffcur.Add(OSalEarnDedTObj);//add ref


                                                //}
                                            }
                                        }

                                    }





                                    var OSalEarnDedTMultioffcur = OSalEarnDedToffcur.GroupBy(x => new { x.SalaryHead })
                                                       .Select(g => new
                                                       {
                                                           SalaryHead = g.Key.SalaryHead,
                                                           StdAmount = (g.Sum(x => x.StdAmount)) / g.Count(),
                                                           TotalAmount = g.Sum(x => x.Amount),
                                                           Count = g.Count()
                                                       }).ToList();
                                    foreach (var ca in OSalEarnDedTMultioffcur)
                                    {
                                        OSalEarnDedToffcur_Final = SaveSalayDetails(OSalEarnDedToffcur_Final, ca.TotalAmount, ca.StdAmount, ca.SalaryHead);
                                    }

                                    //Deduction head depend on earn start
                                    //oficiate struct

                                    //supannu applicable

                                    List<EmpSalStruct> mEmpSalStructTotaloffcursupann = EmpSalStructTotaloffcur.Where(e => e.EffectiveDate.Value.Date >= comparedateoff && e.EffectiveDate.Value.Date <= comparedateoffend)
                                                       .OrderBy(r => r.EffectiveDate)
                                                       .ToList();
                                    if (mEmpSalStructTotaloffcursupann.Count() > 0)
                                    {
                                        foreach (EmpSalStruct OMultiEmpStruct in mEmpSalStructTotaloffcursupann)
                                        {
                                            var OEmpsalhead = OMultiEmpStruct.EmpSalStructDetails.Where(e => e.SalaryHead.Code == "SUPANNU" && e.Amount != 0).FirstOrDefault();
                                            if (OEmpsalhead != null)
                                            {
                                                supannapplicable = true;
                                            }
                                            else
                                            {
                                                supannapplicable = false;
                                            }
                                        }
                                    }
                                    var SUPANNOFFICIATINGval = db.SalaryHead.Include(e => e.SalHeadOperationType).Where(e => e.SalHeadOperationType.LookupVal.ToUpper() == "SUPANNOFFICIATING").SingleOrDefault();
                                    if (SUPANNOFFICIATINGval != null && supannapplicable == false)
                                    {

                                        if (OSalEarnDedToff_Final.Count() > 0)
                                        {

                                            if (OSalEarnDedToff_Final.Any(e => e.SalaryHead.Id == SUPANNOFFICIATINGval.Id))
                                            {
                                                OSalEarnDedToff_Final.Where(e => e.SalaryHead.Id == SUPANNOFFICIATINGval.Id).FirstOrDefault().Amount = 0;
                                            }
                                        }
                                        if (OSalEarnDedToffcur_Final.Count() > 0)
                                        {

                                            if (OSalEarnDedToffcur_Final.Any(e => e.SalaryHead.Id == SUPANNOFFICIATINGval.Id))
                                            {
                                                OSalEarnDedToffcur_Final.Where(e => e.SalaryHead.Id == SUPANNOFFICIATINGval.Id).FirstOrDefault().Amount = 0;
                                            }
                                        }
                                    }

                                    var mEmpSalStructTotaloffded = EmpSalStructTotaloff.Where(e => e.EffectiveDate.Value.Date >= comparedateoff && e.EffectiveDate.Value.Date <= comparedateoffend)
                                                              .OrderBy(r => r.EffectiveDate)
                                                              .LastOrDefault();

                                    List<EmpSalStructDetails> EmpsalDedheadoff = mEmpSalStructTotaloffded.EmpSalStructDetails
                  .Where(s => s.SalaryHead.Frequency.LookupVal.ToUpper() == "MONTHLY"
                      && s.SalaryHead.Type.LookupVal.ToUpper() == "DEDUCTION"
                      && s.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "REGULAR" && s.SalaryHead.ProcessType.LookupVal.ToUpper() == "EARNED" && s.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() != "INSURANCE").OrderBy(e => e.SalaryHead.SeqNo).ToList(); //|| s.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "LOAN").ToList();
                                    if (EmpsalDedheadoff != null && EmpsalDedheadoff.Count() > 0)
                                    {
                                        foreach (EmpSalStructDetails ca in EmpsalDedheadoff)
                                        {
                                            double CalAmountearn = 0;
                                            if (ca.SalaryHead.ProcessType.LookupVal.ToUpper() == "EARNED" && ca.SalHeadFormula != null)
                                            {
                                                EmployeePayroll OEmpPayrollEarn = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.Employee.ServiceBookDates).Where(e => e.Id == oEmployeePayrolloffcur).FirstOrDefault();
                                                CalAmountearn = Process.SalaryHeadGenProcess.SalHeadAmountCalc(ca.SalHeadFormula.Id, OSalEarnDedToff_Final, null, OEmpPayrollEarn, PayMonth);
                                            }
                                            CalAmountearn = Process.SalaryHeadGenProcess.RoundingFunction(ca.SalaryHead, CalAmountearn);

                                            if (OSalEarnDedToff_Final.Count() > 0)
                                            {

                                                if (OSalEarnDedToff_Final.Any(e => e.SalaryHead.Id == ca.SalaryHead.Id))
                                                {
                                                    OSalEarnDedToff_Final.Where(e => e.SalaryHead.Id == ca.SalaryHead.Id).FirstOrDefault().Amount = CalAmountearn;
                                                }
                                            }

                                        }
                                    }




                                    // employee cur stru
                                    var EmpSalStructTotaloffcurded = EmpSalStructTotaloffcur.Where(e => e.EffectiveDate.Value.Date >= comparedateoff && e.EffectiveDate.Value.Date <= comparedateoffend)
                                                              .OrderBy(r => r.EffectiveDate)
                                                              .LastOrDefault();

                                    List<EmpSalStructDetails> EmpsalDedhead = EmpSalStructTotaloffcurded.EmpSalStructDetails
                    .Where(s => s.SalaryHead.Frequency.LookupVal.ToUpper() == "MONTHLY"
                        && s.SalaryHead.Type.LookupVal.ToUpper() == "DEDUCTION"
                        && s.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "REGULAR" && s.SalaryHead.ProcessType.LookupVal.ToUpper() == "EARNED" && s.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() != "INSURANCE").OrderBy(e => e.SalaryHead.SeqNo).ToList(); //|| s.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "LOAN").ToList();
                                    if (EmpsalDedhead != null && EmpsalDedhead.Count() > 0)
                                    {
                                        foreach (EmpSalStructDetails ca in EmpsalDedhead)
                                        {
                                            double CalAmountearn = 0;
                                            if (ca.SalaryHead.ProcessType.LookupVal.ToUpper() == "EARNED" && ca.SalHeadFormula != null)
                                            {
                                                EmployeePayroll OEmpPayrollEarn = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.Employee.ServiceBookDates).Where(e => e.Id == oEmployeePayrolloffcur).FirstOrDefault();
                                                CalAmountearn = Process.SalaryHeadGenProcess.SalHeadAmountCalc(ca.SalHeadFormula.Id, OSalEarnDedToffcur_Final, null, OEmpPayrollEarn, PayMonth);
                                            }
                                            CalAmountearn = Process.SalaryHeadGenProcess.RoundingFunction(ca.SalaryHead, CalAmountearn);

                                            if (OSalEarnDedToffcur_Final.Count() > 0)
                                            {

                                                if (OSalEarnDedToffcur_Final.Any(e => e.SalaryHead.Id == ca.SalaryHead.Id))
                                                {
                                                    OSalEarnDedToffcur_Final.Where(e => e.SalaryHead.Id == ca.SalaryHead.Id).FirstOrDefault().Amount = CalAmountearn;
                                                }
                                            }

                                        }
                                    }
                                    //Deduction head depend on earn end
                                    // Currentemptotal salary end
                                    // pf calculation 
                                    var OEmpSalStruct = EmpSalStructTotaloffcur.Where(e => e.EffectiveDate.Value.Date >= comparedateoff && e.EffectiveDate.Value.Date <= comparedateoffend).OrderBy(r => r.Id).FirstOrDefault();

                                    if (OEmpOff != null)
                                    {
                                        if (OEmpOff.PFAppl == true)
                                        {
                                            var pfhead = db.SalaryHead.Include(e => e.SalHeadOperationType).Where(e => e.SalHeadOperationType.LookupVal.ToUpper() == "EPF").FirstOrDefault();

                                            var EmpPFHead = pfhead != null ? OSalEarnDedTMultioffcur.Where(e => e.SalaryHead.Id == pfhead.Id).FirstOrDefault() : null;

                                            if (EmpPFHead == null)
                                            {
                                                //no pf head defined

                                            }
                                            else
                                            {
                                                //PFMaster OCompPFMaster = OCompanyPayroll.PFMaster.Where(e => e.EndDate != null || e.EndDate.Value.Date > Convert.ToDateTime("01/" + PayMonth).Date).SingleOrDefault();
                                                //PFMaster OCompPFMaster = OCompanyPayroll.PFMaster.Where(e =>e.EstablishmentID==OEmpOff.PFTrust_EstablishmentId && e.EndDate == null || e.EndDate.Value.Date > Convert.ToDateTime("01/" + PayMonth).Date).FirstOrDefault();
                                                PFMaster OCompPFMaster = null;
                                                foreach (var itemPFmas in OCompanyPayroll.PFMaster.ToList())
                                                {
                                                    var Fulldate = PayMonth;
                                                    var PayMonthdate = Convert.ToDateTime(Fulldate);
                                                    var payMonthDate = PayMonthdate.ToString("MM/yyyy");

                                                    if (itemPFmas.EstablishmentID == OEmpOff.PFTrust_EstablishmentId && (itemPFmas.EndDate == null || itemPFmas.EndDate.Value.Date > Convert.ToDateTime("01/" + payMonthDate).Date))
                                                    {
                                                        OCompPFMaster = itemPFmas;
                                                    }
                                                }
                                                if (OCompPFMaster == null)
                                                {
                                                    //no pf master
                                                }
                                                else
                                                {
                                                    var Fulldate = PayMonth;
                                                    var PayMonthdate = Convert.ToDateTime(Fulldate);
                                                    var payMonthDate = PayMonthdate.ToString("MM/yyyy");
                                                    //PFTransT OPFTransT = new PFTransT();
                                                    OPFTransTcur = PFcalcArr(OCompPFMaster, OEmpSalStruct, OEmpPayroll.EmployeePayroll.Id, null, payMonthDate, OSalEarnDedToffcur_Final, OEmpOff.UANNo);
                                                    if (OPFTransTcur != null)
                                                    {
                                                        double CalAmountpf = 0;
                                                        CalAmountpf = Convert.ToDouble(OPFTransTcur.EE_Share);
                                                        CalAmountpf = Process.SalaryHeadGenProcess.RoundingFunction(EmpPFHead.SalaryHead, CalAmountpf);
                                                        foreach (var pfh in OSalEarnDedToffcur_Final)
                                                        {
                                                            if (pfh.SalaryHead.Code == pfhead.Code)
                                                            {
                                                                pfh.Amount = CalAmountpf;
                                                            }
                                                        }


                                                    }


                                                    OPFTransToff = PFcalcArr(OCompPFMaster, OEmpSalStruct, OEmpPayroll.EmployeePayroll.Id, null, payMonthDate, OSalEarnDedToff_Final, OEmpOff.UANNo);
                                                    if (OPFTransToff != null)
                                                    {
                                                        double CalAmountpf = 0;
                                                        CalAmountpf = Convert.ToDouble(OPFTransToff.EE_Share);
                                                        CalAmountpf = Process.SalaryHeadGenProcess.RoundingFunction(EmpPFHead.SalaryHead, CalAmountpf);
                                                        foreach (var pfh in OSalEarnDedToff_Final)
                                                        {
                                                            if (pfh.SalaryHead.Code == pfhead.Code)
                                                            {
                                                                pfh.Amount = CalAmountpf;
                                                            }
                                                        }


                                                    }

                                                }
                                            }

                                        }
                                    }

                                    // pf calculation off



                                }

                                // update off amount in starture

                                // officiateamt = offemptotal - Actualemptotal;
                                double officiateamt = 0;
                                //var offservicebookcheck = db.OfficiatingServiceBook.Include(e => e.OfficiatingParameter).Where(e => e.EmployeeId == OEmpPayroll.Employee.Id && e.Release == true && e.PayMonth == PayMonth).FirstOrDefault();
                                //var offservicebook = db.BMSPaymentReq.Include(e => e.EmployeePayroll).Include(e => e.EmployeePayroll.Employee).Where(e => e.Id == offid && e.EmployeePayroll_Id == OEmpPayroll.Employee.Id).ToList();
                                var offservicebookcheck = db.BMSPaymentReq.Include(e => e.EmployeePayroll).Include(e => e.EmployeePayroll.Employee).Include(e => e.OfficiatingParameter).Where(e => e.EmployeePayroll_Id == OEmpPayroll.EmployeePayroll.Id).FirstOrDefault();
                                int offparaid1 = Convert.ToInt32(OfficiatingParameterlist);
                                var offpara = db.OfficiatingParameter.Where(e => e.Id == offparaid1).FirstOrDefault();
                                List<OfficiatingPaymentT> OFinalOfficiatingPayementT = new List<OfficiatingPaymentT>();
                                SalaryArrearPFT OFinalOOfficiatingPFT = new SalaryArrearPFT();
                                if (offpara.PayAmountuppergradediffAppl == true)
                                {
                                    //Old Components not in new salary
                                    List<int> OSalheadadd = new List<int>();
                                    foreach (var item in OSalEarnDedToff_Final)
                                    {
                                        OSalheadadd.Add(item.SalaryHead.Id);
                                    }

                                    DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };
                                    foreach (var ca in OSalEarnDedToffcur_Final.Where(e => e.SalaryHead.SalHeadOperationType != null && e.SalaryHead.Frequency.LookupVal.ToUpper() != "DAILY" && e.SalaryHead.Frequency.LookupVal.ToUpper() != "HOURLY"))
                                    {
                                        if (OSalheadadd.Contains(ca.SalaryHead.Id) == false)
                                        {
                                            //new data
                                            OfficiatingPaymentT OSalOfficiatingPaymentT = new OfficiatingPaymentT();
                                            if (ca.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "EPF")
                                            {
                                                SalaryArrearPFT OSalaryArrearPFT = new SalaryArrearPFT()
                                                {
                                                    SalaryWages = OPFTransTcur.Gross_Wages,
                                                    EPFWages = OPFTransTcur.EPF_Wages,
                                                    EPSWages = OPFTransTcur.EPS_Wages,
                                                    EDLIWages = OPFTransTcur.EDLI_Wages,

                                                    CompPF = OPFTransTcur.ER_Share,
                                                    EmpPF = OPFTransTcur.EE_Share,
                                                    EmpVPF = OPFTransTcur.EE_VPF_Share,
                                                    EmpEPS = OPFTransTcur.EPS_Share,

                                                    DBTrack = dbt,
                                                };
                                                db.SalaryArrearPFT.Add(OSalaryArrearPFT);
                                                //OSalArrearPaymentT.SalaryArrearPFT = OSalaryArrearPFT;
                                                OFinalOOfficiatingPFT = OSalaryArrearPFT;
                                            }

                                            var Fulldate = PayMonth;
                                            var PayMonthdate = Convert.ToDateTime(Fulldate);
                                            var payMonthDate = PayMonthdate.ToString("MM/yyyy");


                                            OSalOfficiatingPaymentT.SalaryHead = db.SalaryHead.Find(ca.SalaryHead.Id);
                                            OSalOfficiatingPaymentT.SalHeadAmount = ca.Amount;

                                            OSalOfficiatingPaymentT.PayMonth = payMonthDate;

                                            OSalOfficiatingPaymentT.DBTrack = dbt;
                                            OFinalOfficiatingPayementT.Add(OSalOfficiatingPaymentT);

                                        }
                                    }

                                    //new salary componenets not in old salary
                                    List<int> OSalheadminus = new List<int>();
                                    foreach (var item in OSalEarnDedToffcur_Final.Where(e => e.SalaryHead.SalHeadOperationType != null && e.SalaryHead.Frequency.LookupVal.ToUpper() != "DAILY" && e.SalaryHead.Frequency.LookupVal.ToUpper() != "HOURLY"))
                                    {
                                        OSalheadminus.Add(item.SalaryHead.Id);
                                    }
                                    foreach (var ca in OSalEarnDedToff_Final)
                                    {
                                        if (OSalheadminus.Contains(ca.SalaryHead.Id) == false)
                                        {
                                            //old data data
                                            OfficiatingPaymentT OSalOfficiatingPaymentT = new OfficiatingPaymentT();
                                            if (ca.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "EPF")
                                            {
                                                SalaryArrearPFT OSalaryArrearPFT = new SalaryArrearPFT()
                                                {
                                                    SalaryWages = OPFTransToff.Gross_Wages,
                                                    EPFWages = OPFTransToff.EPF_Wages,
                                                    EPSWages = OPFTransToff.EPF_Wages,
                                                    EDLIWages = OPFTransToff.EDLI_Wages,

                                                    CompPF = -OPFTransToff.ER_Share,
                                                    EmpPF = -OPFTransToff.EE_Share,
                                                    EmpVPF = -OPFTransToff.EE_VPF_Share,
                                                    EmpEPS = -OPFTransToff.EPS_Share,

                                                    DBTrack = dbt,
                                                };
                                                db.SalaryArrearPFT.Add(OSalaryArrearPFT);
                                                //OSalArrearPaymentT.SalaryArrearPFT = OSalaryArrearPFT;
                                                OFinalOOfficiatingPFT = OSalaryArrearPFT;
                                            }

                                            var Fulldate = PayMonth;
                                            var PayMonthdate = Convert.ToDateTime(Fulldate);
                                            var payMonthDate = PayMonthdate.ToString("MM/yyyy");

                                            OSalOfficiatingPaymentT.SalaryHead = db.SalaryHead.Find(ca.SalaryHead.Id);
                                            OSalOfficiatingPaymentT.SalHeadAmount = -ca.Amount;

                                            OSalOfficiatingPaymentT.PayMonth = payMonthDate;

                                            OSalOfficiatingPaymentT.DBTrack = dbt;
                                            OFinalOfficiatingPayementT.Add(OSalOfficiatingPaymentT);

                                        }
                                    }
                                    //check for existing salary componenets
                                    foreach (var ca in OSalEarnDedToff_Final)
                                    {
                                        foreach (var ca1 in OSalEarnDedToffcur_Final)
                                        {
                                            if (ca.SalaryHead.Id == ca1.SalaryHead.Id)
                                            {
                                                OfficiatingPaymentT OSalOfficiatingPaymentT = new OfficiatingPaymentT();
                                                //  DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };
                                                if (ca.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "EPF")
                                                {
                                                    //  if lwp process then arrear paid not require as discuss with sir
                                                    SalaryArrearPFT OSalaryArrearPFT = new SalaryArrearPFT()
                                                    {
                                                        SalaryWages = (OPFTransToff.Gross_Wages - OPFTransTcur.Gross_Wages),
                                                        EPFWages = (OPFTransToff.EPF_Wages - OPFTransTcur.EPF_Wages),
                                                        EPSWages = (OPFTransToff.EPS_Wages - OPFTransTcur.EPS_Wages),
                                                        EDLIWages = (OPFTransToff.EDLI_Wages - OPFTransTcur.EDLI_Wages),

                                                        CompPF = (OPFTransToff.ER_Share - OPFTransTcur.ER_Share),
                                                        EmpPF = (OPFTransToff.EE_Share - OPFTransTcur.EE_Share),
                                                        EmpVPF = (OPFTransToff.EE_VPF_Share - OPFTransTcur.EE_VPF_Share),
                                                        EmpEPS = (OPFTransToff.EPS_Share - OPFTransTcur.EPS_Share),

                                                        DBTrack = dbt,

                                                    };
                                                    db.SalaryArrearPFT.Add(OSalaryArrearPFT);
                                                    OFinalOOfficiatingPFT = OSalaryArrearPFT;


                                                }
                                                var Fulldate = PayMonth;
                                                var PayMonthdate = Convert.ToDateTime(Fulldate);
                                                var payMonthDate = PayMonthdate.ToString("MM/yyyy");

                                                OSalOfficiatingPaymentT.PayMonth = payMonthDate;
                                                OSalOfficiatingPaymentT.SalaryHead = db.SalaryHead.Find(ca1.SalaryHead.Id);

                                                double Arrpaid = 0;

                                                if (ca.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "SUPANNOFFICIATING" && supannapplicable == false)
                                                {
                                                    OSalOfficiatingPaymentT.SalHeadAmount = 0;
                                                }
                                                else
                                                {
                                                    OSalOfficiatingPaymentT.SalHeadAmount = Process.SalaryHeadGenProcess.RoundingFunction(ca.SalaryHead, (ca.Amount - ca1.Amount));

                                                }


                                                OSalOfficiatingPaymentT.DBTrack = dbt;
                                                OFinalOfficiatingPayementT.Add(OSalOfficiatingPaymentT);

                                                break;
                                            }

                                        }

                                    }

                                    var oBMSPaymentReq = db.BMSPaymentReq.Include(e => e.EmployeePayroll).Include(e => e.EmployeePayroll.Employee).Where(e => e.Id == offid && e.EmployeePayroll_Id == OEmpPayroll.EmployeePayroll.Id).FirstOrDefault();

                                    OFinalOfficiatingPayementT = OFinalOfficiatingPayementT.Where(e => e.SalaryHead.InPayslip == true).ToList();

                                    if (OFinalOfficiatingPayementT.Count() > 0)
                                    {


                                        oBMSPaymentReq.DBTrack = new DBTrack
                                        {
                                            CreatedBy = oBMSPaymentReq.DBTrack.CreatedBy == null ? null : oBMSPaymentReq.DBTrack.CreatedBy,
                                            CreatedOn = oBMSPaymentReq.DBTrack.CreatedOn == null ? null : oBMSPaymentReq.DBTrack.CreatedOn,
                                            Action = "M",
                                            ModifiedBy = SessionManager.UserName,
                                            ModifiedOn = DateTime.Now
                                        };

                                        db.OfficiatingPaymentT.AddRange(OFinalOfficiatingPayementT);
                                        db.SaveChanges();
                                        if (OFinalOOfficiatingPFT.CompPF != 0.0)
                                        {
                                            oBMSPaymentReq.OfficiatingPFT = OFinalOOfficiatingPFT;
                                        }
                                        var Fulldate = PayMonth;
                                        var PayMonthdate = Convert.ToDateTime(Fulldate);
                                        var payMonthDate = PayMonthdate.ToString("MM/yyyy");

                                        oBMSPaymentReq.OfficiatingPaymentT = OFinalOfficiatingPayementT;
                                        oBMSPaymentReq.OtherDeduction = OFinalOfficiatingPayementT
                                                 .Where(e => e.SalaryHead.InPayslip == true && e.SalaryHead.Type.LookupVal.ToUpper() == "DEDUCTION")
                                                 .Sum(e => e.SalHeadAmount);
                                        oBMSPaymentReq.AmountPaid = OFinalOfficiatingPayementT
                                                 .Where(e => e.SalaryHead.InPayslip == true && e.SalaryHead.Type.LookupVal.ToUpper() == "EARNING")
                                                 .Sum(e => e.SalHeadAmount);
                                        oBMSPaymentReq.PayMonth = payMonthDate;
                                        //   oBMSPaymentReq.DBTrack = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };
                                        db.BMSPaymentReq.Attach(oBMSPaymentReq);
                                        db.Entry(oBMSPaymentReq).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                        db.Entry(oBMSPaymentReq).State = System.Data.Entity.EntityState.Detached;





                                    }


                                }
                                else
                                {
                                    // not diffrence in officiating
                                    DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };
                                    foreach (var ca in OSalEarnDedToff_Final)
                                    {

                                        //old data data
                                        OfficiatingPaymentT OSalOfficiatingPaymentT = new OfficiatingPaymentT();
                                        if (ca.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "EPF")
                                        {
                                            SalaryArrearPFT OSalaryArrearPFT = new SalaryArrearPFT()
                                            {
                                                SalaryWages = OPFTransToff.Gross_Wages,
                                                EPFWages = OPFTransToff.EPF_Wages,
                                                EPSWages = OPFTransToff.EPF_Wages,
                                                EDLIWages = OPFTransToff.EDLI_Wages,

                                                CompPF = OPFTransToff.ER_Share,
                                                EmpPF = OPFTransToff.EE_Share,
                                                EmpVPF = OPFTransToff.EE_VPF_Share,
                                                EmpEPS = OPFTransToff.EPS_Share,

                                                DBTrack = dbt,
                                            };
                                            db.SalaryArrearPFT.Add(OSalaryArrearPFT);
                                            //OSalArrearPaymentT.SalaryArrearPFT = OSalaryArrearPFT;
                                            OFinalOOfficiatingPFT = OSalaryArrearPFT;
                                        }

                                        var Fulldate = PayMonth;
                                        var PayMonthdate = Convert.ToDateTime(Fulldate);
                                        var payMonthDate = PayMonthdate.ToString("MM/yyyy");

                                        OSalOfficiatingPaymentT.SalaryHead = db.SalaryHead.Find(ca.SalaryHead.Id);
                                        OSalOfficiatingPaymentT.SalHeadAmount = ca.Amount;

                                        OSalOfficiatingPaymentT.PayMonth = payMonthDate;

                                        OSalOfficiatingPaymentT.DBTrack = dbt;
                                        OFinalOfficiatingPayementT.Add(OSalOfficiatingPaymentT);


                                    }

                                    var oBMSPaymentReq = db.BMSPaymentReq.Include(e => e.EmployeePayroll).Include(e => e.EmployeePayroll.Employee).Where(e => e.Id == offid && e.EmployeePayroll_Id == OEmpPayroll.EmployeePayroll.Id).FirstOrDefault();


                                    if (OFinalOfficiatingPayementT.Count() > 0)
                                    {
                                        var Fulldate = PayMonth;
                                        var PayMonthdate = Convert.ToDateTime(Fulldate);
                                        var payMonthDate = PayMonthdate.ToString("MM/yyyy");

                                        db.OfficiatingPaymentT.AddRange(OFinalOfficiatingPayementT);
                                        db.SaveChanges();
                                        if (OFinalOOfficiatingPFT.DBTrack == null)
                                        {
                                            OFinalOOfficiatingPFT.DBTrack = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };
                                        }
                                        oBMSPaymentReq.OfficiatingPFT = OFinalOOfficiatingPFT;
                                        oBMSPaymentReq.OfficiatingPaymentT = OFinalOfficiatingPayementT;
                                        oBMSPaymentReq.OtherDeduction = OFinalOfficiatingPayementT
                                  .Where(e => e.SalaryHead.InPayslip == true && e.SalaryHead.Type.LookupVal.ToUpper() == "DEDUCTION")
                                  .Sum(e => e.SalHeadAmount);
                                        oBMSPaymentReq.AmountPaid = OFinalOfficiatingPayementT
                                    .Where(e => e.SalaryHead.InPayslip == true && e.SalaryHead.Type.LookupVal.ToUpper() == "EARNING")
                                    .Sum(e => e.SalHeadAmount);
                                        oBMSPaymentReq.PayMonth = payMonthDate;
                                        oBMSPaymentReq.DBTrack = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };
                                        db.BMSPaymentReq.Attach(oBMSPaymentReq);
                                        db.Entry(oBMSPaymentReq).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                        db.Entry(oBMSPaymentReq).State = System.Data.Entity.EntityState.Detached;


                                    }


                                }

                            }
                        }
                        //CalAmount = officiateamtTotal;
                        // officiate code End
                        //return CalAmount;
                        //officiating close
                        ts.Complete();

                        //var employeeId = OEmpPayroll.EmployeePayroll.Employee_Id;
                        //var officitingId = offid;

                        //HttpContext.Current.Session["ModuleType"] = employeeId;

                    }//ts close
                }
                catch (Exception ex)
                {
                    throw;
                    LogFile Logfile = new LogFile();
                    ErrorLog Err = new ErrorLog()
                    {
                        // ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                        ExceptionMessage = ex.Message,
                        ExceptionStackTrace = ex.StackTrace,
                        LineNo = Convert.ToString(new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                        LogTime = DateTime.Now
                    };
                    Logfile.CreateLogFile(Err);

                    Msg.Add(ex.Message);
                    //return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }

            }
            return CalAmount;
        }


        public static double offAmountCalc(double SalAmount, bool appearhead, int mPayDaysRunningoff, double SalAttendanceT_monthDaysoff, DateTime Dofffromdate)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                int mon = Dofffromdate.Month;
                double mSalHeadAmount = 0;
                var OPayProcGrp = db.PayProcessGroup.Include(e => e.PayMonthConcept).Include(e => e.PayrollPeriod).Include(e => e.PayFrequency).SingleOrDefault();

                if (OPayProcGrp != null)
                {

                    if (OPayProcGrp.PayMonthConcept.LookupVal.ToString().ToUpper() == "FIXED30DAYS")
                    {

                        if (appearhead == true)
                        {

                            if (mPayDaysRunningoff > 30)
                            {
                                mPayDaysRunningoff = 30;
                            }
                            if (mon == 2)//feb month
                            {
                                if (mPayDaysRunningoff == 28 || mPayDaysRunningoff == 29)
                                {
                                    mPayDaysRunningoff = 30;
                                }
                            }
                            mSalHeadAmount = ((SalAmount / 30) * mPayDaysRunningoff);
                        }
                        else
                        {
                            mSalHeadAmount = SalAmount;
                        }

                    }
                    if (OPayProcGrp.PayMonthConcept.LookupVal.ToString().ToUpper() == "30DAYS")
                    {

                        if (appearhead == true)
                        {

                            mSalHeadAmount = ((SalAmount) - ((SalAttendanceT_monthDaysoff - mPayDaysRunningoff) / 30) * SalAmount);
                        }
                        else
                        {
                            mSalHeadAmount = SalAmount;
                        }

                    }
                    if (OPayProcGrp.PayMonthConcept.LookupVal.ToString().ToUpper() == "CALENDAR")
                    {

                        if (appearhead == true)
                        {

                            mSalHeadAmount = (SalAmount * ((mPayDaysRunningoff) / (SalAttendanceT_monthDaysoff)));
                        }
                        else
                        {
                            mSalHeadAmount = SalAmount;
                        }
                    }
                    mSalHeadAmount = Math.Round(mSalHeadAmount, 0);
                }
                // NkGSB Code Start

                string requiredPathLoanNet = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\App_Data\\Menu_Json";
                bool existscdNet = System.IO.Directory.Exists(requiredPathLoanNet);
                string localPathLoanNet;
                if (!existscdNet)
                {
                    localPathLoanNet = new Uri(requiredPathLoanNet).LocalPath;
                    System.IO.Directory.CreateDirectory(localPathLoanNet);
                }
                string pathLoanNet = requiredPathLoanNet + @"\OFFICIATINGPAYMONTHCONCEPT" + ".ini";
                localPathLoanNet = new Uri(pathLoanNet).LocalPath;
                if (!System.IO.File.Exists(localPathLoanNet))
                {

                    using (var fs = new FileStream(localPathLoanNet, FileMode.OpenOrCreate))
                    {
                        StreamWriter str = new StreamWriter(fs);
                        str.BaseStream.Seek(0, SeekOrigin.Begin);

                        str.Flush();
                        str.Close();
                        fs.Close();
                    }


                }
                string PayMonthConcept = "";
                string JvHeadparameterName = "";

                string requiredPathchkNet = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\App_Data\\Menu_Json";
                bool existschkNet = System.IO.Directory.Exists(requiredPathchkNet);
                string localPathchkNet;
                if (!existschkNet)
                {
                    localPathchkNet = new Uri(requiredPathchkNet).LocalPath;
                    System.IO.Directory.CreateDirectory(localPathchkNet);
                }
                string pathchkNet = requiredPathchkNet + @"\OFFICIATINGPAYMONTHCONCEPT" + ".ini";
                localPathchkNet = new Uri(pathchkNet).LocalPath;
                using (var streamReader = new StreamReader(localPathchkNet))
                {
                    string line;

                    while ((line = streamReader.ReadLine()) != null)
                    {

                        var PaymonCon = line;
                        PayMonthConcept = PaymonCon;


                    }
                }
                if (PayMonthConcept != "")
                {
                    if (PayMonthConcept.ToUpper() == "FIXED30DAYS")
                    {

                        if (appearhead == true)
                        {

                            if (mPayDaysRunningoff > 30)
                            {
                                mPayDaysRunningoff = 30;
                            }
                            if (mon == 2)//feb month
                            {
                                if (mPayDaysRunningoff == 28 || mPayDaysRunningoff == 29)
                                {
                                    mPayDaysRunningoff = 30;
                                }
                            }
                            mSalHeadAmount = ((SalAmount / 30) * mPayDaysRunningoff);
                        }
                        else
                        {
                            mSalHeadAmount = SalAmount;
                        }

                    }
                    if (PayMonthConcept.ToUpper() == "30DAYS")
                    {

                        if (appearhead == true)
                        {

                            mSalHeadAmount = ((SalAmount) - ((SalAttendanceT_monthDaysoff - mPayDaysRunningoff) / 30) * SalAmount);
                        }
                        else
                        {
                            mSalHeadAmount = SalAmount;
                        }

                    }
                    if (PayMonthConcept.ToUpper() == "CALENDAR")
                    {

                        if (appearhead == true)
                        {

                            mSalHeadAmount = (SalAmount * ((mPayDaysRunningoff) / (SalAttendanceT_monthDaysoff)));
                        }
                        else
                        {
                            mSalHeadAmount = SalAmount;
                        }
                    }
                    mSalHeadAmount = Math.Round(mSalHeadAmount, 0);


                }

                // NKGSB Code End
                return mSalHeadAmount;
            }
        }

        public static double SalHeadAmountCalc(Int32 OSalHeadFormula, List<SalEarnDedT> OSalaryDetails, List<EmpSalStructDetails> OEmpSalStructDetails,
                                          EmployeePayroll OEmployeePayroll, string mCPIMonth, bool VPFApplicable = false)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                var OSalHeadFormualQuery = Process.SalaryHeadGenProcess._returnSalHeadFormula(OSalHeadFormula);
                double mSalWages = 0;
                double mSalHeadAmount = 0;
                mSalWages = Process.SalaryHeadGenProcess.Wagecalc(OSalHeadFormualQuery, OSalaryDetails, OEmpSalStructDetails);
                mSalHeadAmount = Process.SalaryHeadGenProcess.CellingCkeck(OSalHeadFormualQuery, mSalWages);
                mSalHeadAmount = Math.Round(mSalHeadAmount + 0.01, 0);
                return mSalHeadAmount;
            }
        }

        public static List<SalEarnDedT> SaveSalayDetails(List<SalEarnDedT> OSalEarnDedT, double mAmount, double mStdAmount, SalaryHead mSalaryHead)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //  var mSalaryHeadDetails = db.SalaryHead.Include(e => e.Frequency).Include(e => e.Type).Where(e => e.Id == mSalaryHead).SingleOrDefault();

                DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };

                SalEarnDedT OSalEarnDedTObj = new SalEarnDedT()
                {
                    Amount = mAmount,
                    StdAmount = mStdAmount,
                    SalaryHead = mSalaryHead,
                    DBTrack = dbt

                };
                OSalEarnDedT.Add(OSalEarnDedTObj);//add ref
                return OSalEarnDedT;
            }
        }

        public static PFECRR PFcalcArr(PFMaster OCompanyPFMaster, EmpSalStruct OEmpSalstruct, int? OEmployeePayrollId, Calendar cal, string PayMonth, List<SalEarnDedT> OSalaryDetails, string UANNo)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var mPFMaster1 = OCompanyPFMaster;

                var OPFMaster = db.PFMaster
                                        .Include(e => e.EPSWages)
                                        .Include(e => e.EPSWages.RateMaster)
                                        .Include(e => e.PFAdminWages)
                                        .Include(e => e.PFEDLIWages)
                                        .Include(e => e.PFInspWages)
                                        .Include(e => e.EPFWages)
                                        .Include(e => e.PFAdminWages.RateMaster)
                                        .Include(e => e.PFEDLIWages.RateMaster)
                                        .Include(e => e.PFInspWages.RateMaster)
                                        .Include(e => e.EPFWages.RateMaster)
                                        .Include(e => e.EPFWages.RateMaster.Select(r => r.SalHead))
                                        .Include(e => e.PFTrustType)
                                        .Where(e => e.Id == mPFMaster1.Id)

                                        .SingleOrDefault();
                //get PF Master from companypayroll by passing companyID from session



                var OSalaryHead = OEmpSalstruct.EmpSalStructDetails.Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "EPF")
                   .SingleOrDefault();


                var OLWPAttend = db.SalAttendanceT.Where(e => e.PayMonth == PayMonth && e.EmployeePayroll.Id == OEmployeePayrollId).FirstOrDefault();

                if (OPFMaster == null)
                {
                    //pf master not exist

                }
                else
                {
                    //pf arrear data call newly added on 25032017
                    double ArrCompPF = 0;
                    double ArrEmpPF = 0;
                    double ArrEmpEPS = 0;
                    double ArrEPFWages = 0;
                    double ArrEPSWages = 0;
                    double ArrEDLIWages = 0;
                    double ArrSalaryWages = 0;
                    double ArrEmpVPF = 0;

                    EmployeePayroll OEmployeePayroll = db.EmployeePayroll
                                           .Include(e => e.Employee)
                                            .Include(e => e.Employee.EmpOffInfo)
                                             .Include(e => e.Employee.EmpOffInfo.NationalityID)
                                           .Include(e => e.Employee.EmpName)
                                           .Include(e => e.Employee.Gender)
                                           .Include(e => e.Employee.MaritalStatus)
                                           .Include(e => e.Employee.ServiceBookDates)
                                           .Where(d => d.Id == OEmployeePayrollId).FirstOrDefault();

                    double mAge = 0;
                    var mDateofBirth = OEmployeePayroll.Employee.ServiceBookDates.BirthDate;// db.Employee.Where(e => e.Id == mEmployeeID).Select(e => e.ServiceBookDates.BirthDate);
                    DateTime start = mDateofBirth.Value;
                    DateTime end = Convert.ToDateTime("01/" + PayMonth).AddMonths(1).AddDays(-1).Date;// DateTime.Now.Date;
                    int compMonth = (end.Month + end.Year * 12) - (start.Month + start.Year * 12);
                    //  double daysInEndMonth = (end - end.AddMonths(1)).Days;
                    double daysInEndMonth = (end.AddMonths(1) - end).Days;
                    double months = compMonth + (start.Day - end.Day) / daysInEndMonth;
                    mAge = months / 12;
                    //mAge = (DateTime.Now.Date - mDateofBirth.Value.Date).Days;
                    //mAge = mAge / 365;

                    double mCompPF = 0;
                    double mEmpPension = 0;
                    double mEmpPF = 0;
                    double mEmpVPF = 0;
                    double mTotalCompPF = 0;
                    double highpension = 0;
                    double highpensionper = 0;

                    mEmpVPF = OSalaryDetails.Where(e => e.SalaryHead.Code.ToUpper() == "VPF").SingleOrDefault() != null ?
                        OSalaryDetails.Where(e => e.SalaryHead.Code.ToUpper() == "VPF").SingleOrDefault().Amount : 0;


                    double mGrossWages = OSalaryDetails.Where(r => r.SalaryHead.Type.LookupVal.ToUpper() == "EARNING" && r.SalaryHead.InPayslip == true)
                                          .Sum(e => e.Amount);
                    mGrossWages = Math.Round(mGrossWages, 0);
                    //double mAdminWages = Process.SalaryHeadGenProcess.WagecalcDirect(OPFMaster.PFAdminWages, OSalaryDetails, null);
                    //SalHeadFormula PFFormula = Process.SalaryHeadGenProcess.SalFormulaFinder(OEmpSalstruct, OSalaryHead.SalaryHead.Id);
                    //double mPFWages = Process.SalaryHeadGenProcess.Wagecalc(PFFormula, OSalaryDetails, null);
                    double mPFWages = Process.SalaryHeadGenProcess.WagecalcDirect(OPFMaster.EPFWages, OSalaryDetails, null);

                    var EmpPFHead = OEmpSalstruct.EmpSalStructDetails
                                    .Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "EPF").SingleOrDefault();
                    if (EmpPFHead != null && EmpPFHead.SalHeadFormula_Id != null)
                    {
                        Int32 Pfformulaid = EmpPFHead.SalHeadFormula.Id;
                        var OSalHeadFormualQuery = Process.SalaryHeadGenProcess._returnSalHeadFormula(Pfformulaid);
                        mPFWages = Process.SalaryHeadGenProcess.Wagecalc(OSalHeadFormualQuery, OSalaryDetails, null);
                        mPFWages = Process.SalaryHeadGenProcess.CellingCkeck(OSalHeadFormualQuery, mPFWages);
                    }

                    mPFWages = Math.Round(mPFWages, 0);
                    double mPensionWages = Process.SalaryHeadGenProcess.WagecalcDirect(OPFMaster.EPSWages, OSalaryDetails, null);

                    var EmpPENSIONHead = OEmpSalstruct.EmpSalStructDetails
                                .Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "PENSION").SingleOrDefault();
                    if (EmpPENSIONHead != null && EmpPENSIONHead.SalHeadFormula_Id != null)
                    {
                        Int32 Pensionformulaid = EmpPENSIONHead.SalHeadFormula.Id;
                        var OSalHeadFormualQuery = Process.SalaryHeadGenProcess._returnSalHeadFormula(Pensionformulaid);
                        mPensionWages = Process.SalaryHeadGenProcess.Wagecalc(OSalHeadFormualQuery, OSalaryDetails, null);
                        mPensionWages = Process.SalaryHeadGenProcess.CellingCkeck(OSalHeadFormualQuery, mPensionWages);
                    }
                    mPensionWages = Math.Round(mPensionWages, 0);
                    double mEDLIWages = Process.SalaryHeadGenProcess.WagecalcDirect(OPFMaster.PFEDLIWages, OSalaryDetails, null);
                    mEDLIWages = Math.Round(mEDLIWages, 0);
                    //check database
                    //mEmpPF = Process.SalaryHeadGenProcess.SalHeadAmountCalc(PFFormula, OSalaryDetails, null, OEmployeePayroll);
                    mEmpPF = Math.Round(mPFWages * OPFMaster.EmpPFPerc / 100, 0);
                    //mTotalCompPF= mPFWages * (mPFMaster.CPFPerc + mPFMaster.EPSPerc); //applay rounding
                    mEmpPF = Math.Round(mEmpPF, 0);
                    if (mAge <= OPFMaster.PensionAge)
                    {
                        if (OEmployeePayroll.Employee.EmpOffInfo.NationalityID.HigherPension == true)
                        {

                            if ((mPFWages - mPensionWages) > 0)
                            {
                                mEmpPension = Math.Round(mPFWages * OPFMaster.EPSPerc / 100, 0);
                                string highpensionperval = OEmployeePayroll.Employee.EmpOffInfo.NationalityID.HigherPensionPer == null ? "0" : OEmployeePayroll.Employee.EmpOffInfo.NationalityID.HigherPensionPer.ToString();
                                highpensionper = Convert.ToDouble(highpensionperval);
                                highpension = Math.Round((mPFWages - mPensionWages) * highpensionper / 100, 0);
                                mEmpPension = mEmpPension + highpension;
                                mPensionWages = mPFWages;
                            }
                            else
                            {
                                mEmpPension = Math.Round(mPensionWages * OPFMaster.EPSPerc / 100, 0);
                            }

                        }
                        else
                        {
                            mEmpPension = Math.Round(mPensionWages * OPFMaster.EPSPerc / 100, 0);
                        }

                    }
                    else
                    {
                        mEmpPension = 0;
                        mPensionWages = 0;
                    }

                    if (OEmployeePayroll.Employee.EmpOffInfo.PensionAppl == false)
                    {
                        mEmpPension = 0;
                        mPensionWages = 0;
                    }

                    mCompPF = mEmpPF - mEmpPension;

                    if (mCompPF > OPFMaster.CompPFCeiling)
                    {
                        mCompPF = OPFMaster.CompPFCeiling;
                    }

                    // Surat Dcc comppf amount celling

                    var OSalaryHeadcomppf = OEmpSalstruct.EmpSalStructDetails.Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "CPF")
                .SingleOrDefault();
                    if (OSalaryHeadcomppf != null)
                    {
                        if (OSalaryHeadcomppf.SalHeadFormula != null)
                        {
                            if ((mCompPF + mEmpPension) > OSalaryHeadcomppf.Amount)
                            {
                                mCompPF = OSalaryHeadcomppf.Amount - mEmpPension;
                            }

                        }
                    }
                    DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };

                    int Comp_Id = Convert.ToInt32(SessionManager.CompPayrollId); ;
                    PFECRR OPFECRR = new PFECRR()
                    {
                        //PFCalendar = db.Calendar.Find(cal.Id),
                        UAN = UANNo,// OEmpOff.NationalityID==null?null: OEmpOff.NationalityID.UANNo,
                        UAN_Name = OEmployeePayroll.Employee.EmpName.FullNameFML,
                        Establishment_ID = OPFMaster.EstablishmentID,
                        Establishment_Name = db.Company.Where(e => e.Id == Comp_Id).Select(r => r.Name).SingleOrDefault(),

                        Gross_Wages = mGrossWages,
                        EPF_Wages = mPFWages,
                        EPS_Wages = mPensionWages,
                        EDLI_Wages = mEDLIWages,
                        EE_Share = (mEmpPF),
                        EPS_Share = (mEmpPension),
                        ER_Share = (mCompPF),
                        EE_VPF_Share = mEmpVPF,
                        NCP_Days = OLWPAttend.LWPDays,
                        Refund_of_Advances = 0,
                        //part of arrears file newly added on 25032017
                        Arrear_EPF_Wages = ArrEPFWages,
                        Arrear_EPS_Wages = ArrEPSWages,
                        Arrear_EDLI_Wages = ArrEDLIWages,
                        Arrear_EE_Share = ArrEmpPF + ArrEmpVPF,
                        Arrear_EPS_Share = ArrEmpEPS,
                        Arrear_ER_Share = ArrCompPF,


                        //required at the time of uan
                        Father_Husband_Name = OEmployeePayroll.Employee.FatherName == null ? "" : OEmployeePayroll.Employee.FatherName.FullNameFML,//.Employee.MaritalStatus.LookupVal.ToUpper() == "MARRIED" ? OEmployeePayroll.Employee.HusbandName.ToString() : OEmployeePayroll.Employee.FatherName.FullNameFML,
                        Relationship_with_the_Member = "F",
                        Date_of_Joining_EPF = OEmployeePayroll.Employee.ServiceBookDates.PFJoingDate == null ? null : OEmployeePayroll.Employee.ServiceBookDates.PFJoingDate,
                        Date_of_Joining_EPS = OEmployeePayroll.Employee.ServiceBookDates.PensionJoingDate == null ? null : OEmployeePayroll.Employee.ServiceBookDates.PensionJoingDate,
                        Date_of_Exit_from_EPF = OEmployeePayroll.Employee.ServiceBookDates.PFExitDate == null ? null : OEmployeePayroll.Employee.ServiceBookDates.PFExitDate,
                        Date_of_Exit_from_EPS = OEmployeePayroll.Employee.ServiceBookDates.PensionExitDate == null ? null : OEmployeePayroll.Employee.ServiceBookDates.PensionExitDate,
                        Reason_for_leaving = "",
                        //calculate at the time of ECR generation
                        //Administrative_Charges_AC2= Math.Round( mAdminWages * OPFMaster.AdminCharges/100,0),
                        //Inspection_Charges_AC2 = Math.Round(mAdminWages * OPFMaster.InspCharges / 100, 0),
                        //EDLI_Contribution_AC21 = Math.Round(mEDLIWages * OPFMaster.EDLICharges / 100, 0),
                        //Administrative_Charges_AC22 = Math.Round( mEDLIWages * OPFMaster.EDLICharges/100,0),
                        //Inspection_Charges_AC22 = Math.Round( mEDLIWages * OPFMaster.PaymentEDLIS/100,0),
                        //Exemption_Status = OPFMaster.PFTrustType.LookupVal,

                        Date_of_Birth = OEmployeePayroll.Employee.ServiceBookDates.BirthDate.Value,
                        Gender = OEmployeePayroll.Employee.Gender == null ? "" : OEmployeePayroll.Employee.Gender.LookupVal.ToUpper(),
                        Return_Month = "",
                        Wage_Month = PayMonth,

                        DBTrack = dbt
                    };

                    return OPFECRR;

                }


                return null;
            }
        }

        public class ProcessData1
        {

            public string totalEarning { get; set; }
            public string totalDeduction { get; set; }
            public string total { get; set; }
            public string salhead { get; set; }
            public string salheadamount { get; set; }
            public string type { get; set; }
        }

        public ActionResult offProData(string data1, string data2)
        {
            List<string> Msg = new List<string>();
            List<ProcessData1> return_Data = new List<ProcessData1>();

            using (DataBaseContext db = new DataBaseContext())
            {

                var OEmp_Id = Convert.ToInt32(TempData["EmployeePayrollId"]);
                var recordId = Convert.ToInt32(TempData["officiate_id"]);
                //var OEmp_Id = Convert.ToInt32(data1);
                //var recordId = Convert.ToInt32(data2);
                var offdata = db.BMSPaymentReq.Include(e => e.OfficiatingPaymentT)
                    .Include(e => e.OfficiatingPaymentT.Select(r => r.SalaryHead))
                    .Include(e => e.OfficiatingPaymentT.Select(r => r.SalaryHead.Type))
                    .Where(e => e.EmployeePayroll_Id == OEmp_Id && e.Id == recordId).FirstOrDefault();

                var totalEarning = offdata != null ? offdata.AmountPaid : 0;
                var totalDeduction = offdata != null ? offdata.OtherDeduction : 0;

                var total = totalEarning - totalDeduction;

                if (offdata != null)
                {
                    foreach (var item in offdata.OfficiatingPaymentT)
                    {
                        return_Data.Add(new ProcessData1
                        {
                            salhead = item.SalaryHead.Name,
                            salheadamount = item.SalHeadAmount.ToString(),
                            type = item.SalaryHead.Type.LookupVal,

                        });
                    }
                }


                return Json(new Object[] { return_Data, totalEarning, totalDeduction, total, JsonRequestBehavior.AllowGet });
            }

        }



























    }
}