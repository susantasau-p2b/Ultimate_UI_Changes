using EssPortal.App_Start;
using EssPortal.Models;
using EssPortal.Security;
using P2b.Global;
using Payroll;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Transactions;
using System.Web.Mvc;
using System.Data.Entity;
using Leave;
using System.IO;
using System.Web;
using System.Data.Entity.Validation;

namespace EssPortal.Controllers
{
    public class BmsPaymentRequestController : Controller
    {

        // GET: /BmsPaymentRequest/
        public ActionResult Index()
        {
            return View("~/Views/BmsPaymentRequest/Index.cshtml");
        }
        public ActionResult partial()
        {
            return View("~/Views/Shared/_BmsPaymentRequestView.cshtml");

        }
        public ActionResult partial1()
        {
            return View("~/Views/Shared/_OfficiatingAndLtaView.cshtml");

        }

        public ActionResult GetApplicableEmpPayStructAppl()
        {

            using (DataBaseContext db = new DataBaseContext())
            {
                var s = db.OfficiatingParameter.FirstOrDefault().OfficiatingEmpPayStructAppl;
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetApplicableEmpId(string EmpId1)
        {

            using (DataBaseContext db = new DataBaseContext())
            {
                TempData["Empid1"] = EmpId1;
                return Json(EmpId1, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetLeaveId(string leaveid)
        {

            using (DataBaseContext db = new DataBaseContext())
            {
                TempData["leaveid"] = leaveid;

                return Json(leaveid, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetApplicableData(string data, string data2)
        {
            //var Id = Convert.ToInt32(TempData["EMPId"]);

            if (TempData["Empid1"] != null)
            {

                using (DataBaseContext db = new DataBaseContext())
                {

                    var Id = Convert.ToInt32(TempData["Empid1"]);

                    var EmployeePayroll = db.EmployeePayroll.Include(e => e.Employee).Where(e => e.Employee_Id == Id).FirstOrDefault();
                    var query = db.EmployeePolicyStruct.AsNoTracking().Where(e => e.EmployeePayroll.Employee.Id == EmployeePayroll.Id && e.EndDate == null).Include(e => e.EmployeePayroll).Include(e => e.EmployeePayroll.Employee).Include(e => e.EmployeePolicyStructDetails)
                                      .Include(e => e.EmployeePolicyStructDetails.Select(r => r.PolicyFormula)).Include(e => e.EmployeePolicyStructDetails.
                                      Select(r => r.PolicyFormula.OfficiatingParameter)).FirstOrDefault();
                    var TransActList = query.EmployeePolicyStructDetails.Select(r => r.PolicyFormula.OfficiatingParameter);

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

                    var selected = (Object)null;

                    if (data2 != "" && data != "0" && data2 != "0")
                    {
                        selected = Convert.ToInt32(data2);
                    }

                    SelectList s = new SelectList(TransAct, "Id", "Name", selected);
                    return Json(s, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult PopulateDropDownActivityList(string data1, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var OEmp_Id = Convert.ToInt32(data1);
                var oemp_id = db.EmployeePayroll.Where(e => e.Id == OEmp_Id).FirstOrDefault().Employee_Id;
                var query = db.EmployeePolicyStruct.AsNoTracking().Where(e => e.EmployeePayroll.Employee.Id == oemp_id && e.EndDate == null).Include(e => e.EmployeePayroll).Include(e => e.EmployeePayroll.Employee).Include(e => e.EmployeePolicyStructDetails)
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


        public ActionResult GetLookupIncharge(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var empinchargeloc = db.Employee
                   .Include(e => e.GeoStruct)
                   .Include(e => e.GeoStruct.Location)
                   .Include(e => e.GeoStruct.Location.Incharge)
                   .Where(e => e.EmpCode == data).FirstOrDefault();
                int inchid = 0;
                if (empinchargeloc != null)
                {
                    if (empinchargeloc.GeoStruct.Location.Incharge != null)
                    {
                        inchid = empinchargeloc.GeoStruct.Location.Incharge.Id;// if dep inchagre on leave then that location incharge should be incharge of that dep.
                        //other loc,dep,division incharge not come in list. as suggest sir
                    }

                }

                var exceploc = db.Location.Where(e => e.Incharge.Id != null && e.Incharge.Id != inchid).Select(e => e.Incharge.Id).ToList();
                var excepDep = db.Department.Where(e => e.Incharge.Id != null && e.Incharge.Id != inchid).Select(e => e.Incharge.Id).ToList();
                var excepDivision = db.Division.Where(e => e.Incharge.Id != null && e.Incharge.Id != inchid).Select(e => e.Incharge.Id).ToList();
                var exceptot = exceploc.Union(excepDep).Union(excepDivision).ToList();

                var fall = db.Employee
                    .Include(e => e.EmpName)
                    .Include(e => e.ServiceBookDates)
                    .Where(e => e.ServiceBookDates.ServiceLastDate == null).ToList();

                IEnumerable<Employee> all;
                if (!string.IsNullOrEmpty(data))
                {
                    // all = db.Employee.ToList().Where(d => d.EmpCode.Contains(data));
                    all = fall;

                }
                else
                {
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { srno = c.Id, lookupvalue = c.FullDetails }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        //public ActionResult GetLookupIncharge()
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var fall = db.Employee
        //            .Include(e => e.EmpName)
        //            .Include(e => e.ServiceBookDates)
        //            .Where(e => e.ServiceBookDates.ServiceLastDate == null).ToList();

        //           IEnumerable<Employee> all;
        //           all = fall;

        //           var r = (from ca in all select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
        //           return Json(r, JsonRequestBehavior.AllowGet);
        //    }
        //}

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

        public ActionResult Create(BMSPaymentReq BMSPaymentReq, FormCollection form) //Create submit
        {
            List<string> Msg = new List<string>();
            try
            {
                using (DataBaseContext db = new DataBaseContext())
                {
                    var LeaveId = Convert.ToInt32(TempData["leaveid"]);
                    int? lvheadids = LeaveId;
                  
                    var Emp = Convert.ToInt32(SessionManager.EmpId);
                    var ProMonth = form["ProcessMonth"] == "0" ? null : form["ProcessMonth"];
                    //var type = TempData["saltype"];
                    var Salheadlist1 = form["Salheadlist"] == "0" ? null : form["Salheadlist"];
                    var Salheadlist = Convert.ToInt32(Salheadlist1);
                    var type = db.SalaryHead.Include(e => e.SalHeadOperationType).Where(e => e.Id == Salheadlist).FirstOrDefault();
                    var lvhead = db.LvHead.Where(e => e.LTAAppl == true).ToList();
                    if (type.SalHeadOperationType.LookupVal.ToUpper() == "LTA")
                    {
                        if (lvhead.Count()>0)
                        {
                            if (LeaveId == 0 || LeaveId == null)
                            {
                                Msg.Add("You Can't apply for LTA as Leave requisition is not available");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }
                        }
                        
                    }

                    var EmpOff = form["Incharge_id"] == "0" ? null : form["Incharge_id"];
                    string OffActivity = form["OfficiatingParameterlist"] == "0" ? "" : form["OfficiatingParameterlist"];
                    string NewPayStruct = form["NewPayT-table"] == "0" ? "" : form["NewPayT-table"];
                    int CompId = 0;
                    if (SessionManager.UserName != null)
                    {
                        CompId = Convert.ToInt32(Session["CompId"]);
                    }
                    Employee OEmployeeOff = null;
                  //  BMSPaymentReq OEmployeePayrollOff = null;

                    int SalaryheadId = Convert.ToInt32(Salheadlist);

                    int EmpOffId;

                    if (!string.IsNullOrEmpty(EmpOff))
                    {
                        EmpOffId = Convert.ToInt32(EmpOff);
                    }
                    else
                    {
                        EmpOffId = 0;
                    }

                    if (type.SalHeadOperationType.LookupVal.ToUpper() == "OFFICIATING")
                    {

                        var query = db.EmployeePolicyStruct.AsNoTracking().Where(e => e.EmployeePayroll.Employee.Id == Emp && e.EndDate == null).Include(e => e.EmployeePayroll).Include(e => e.EmployeePayroll.Employee).Include(e => e.EmployeePolicyStructDetails)
                                        .Include(e => e.EmployeePolicyStructDetails.Select(r => r.PolicyFormula)).Include(e => e.EmployeePolicyStructDetails.
                                        Select(r => r.PolicyFormula.OfficiatingParameter)).FirstOrDefault();
                        var TransActList = query.EmployeePolicyStructDetails.Select(r => r.PolicyFormula.OfficiatingParameter);

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
                        if (TransAct.Count() == 0)
                        {
                            Msg.Add("You Are not Applicable for Officiating");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    int EmpIdCheck = Convert.ToInt32(Emp);
                    if (EmpIdCheck != null && EmpIdCheck != 0)
                    {

                        List<string> MsgCheck = new List<string>();

                        OEmployeeOff = db.Employee.Include(q => q.EmpName).Where(r => r.Id == EmpIdCheck).AsNoTracking().AsParallel().SingleOrDefault();
                        //OEmployeePayrollOff = db.EmployeePayroll.Include(e => e.OfficiatingServiceBook).Where(e => e.Employee.Id == OEmployeeOff.Id).AsNoTracking().AsParallel().SingleOrDefault();
                       // OEmployeePayrollOff = db.BMSPaymentReq.Include(e => e.EmployeePayroll).Where(e => e.EmployeePayroll.Employee.Id == OEmployeeOff.Id).AsNoTracking().AsParallel().FirstOrDefault();

                        DateTime mFromPeriod = Convert.ToDateTime(BMSPaymentReq.FromPeriod);
                        DateTime mEndDate = Convert.ToDateTime(BMSPaymentReq.ToPeriod);
                        if (type.SalHeadOperationType.LookupVal.ToUpper() == "OFFICIATING")
                        {
                            if (mFromPeriod.Year != mEndDate.Year || mFromPeriod.Month != mEndDate.Month)
                            {
                                Msg.Add("Please select the FromDate and ToDate of same month ");
                                return Json(new { status = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                        }

                        var emp = db.Employee.Where(e => e.Id == Emp).FirstOrDefault();

                        for (DateTime mTempDate = mFromPeriod; mTempDate <= mEndDate; mTempDate = mTempDate.AddDays(1))
                        {

                            int Officiatingdates = db.BMSPaymentReq.Where(e => e.EmployeePayroll.Employee.Id == Emp && (e.FromPeriod.Value <= mTempDate.Date && e.ToPeriod.Value >= mTempDate.Date) && e.IsCancel == false && e.TrReject == false && e.SalaryHead_Id == SalaryheadId).Select(e => e.Id).FirstOrDefault();


                            if (Officiatingdates > 0)
                            {
                                Msg.Add("You have already enter " + mTempDate.Date + " for this employee " + emp.EmpCode);
                                return Json(new { status = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }

                        }

                    }
                    else
                    {
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    Employee OEmployeec = null;
                    EmployeePayroll OEmployeePayrollc = null;
                    string EmpRel = null;
                    string EmpPro = null;
                    string EmpCPI = null;
                    //string PayMonth = OfficiatingServiceBook.PayMonth;
                    if (EmpOffId > 0)
                    {
                        OEmployeec = db.Employee.Where(r => r.Id == EmpOffId).SingleOrDefault();

                        OEmployeePayrollc = db.EmployeePayroll.Where(e => e.Employee.Id == OEmployeec.Id).SingleOrDefault();

                        var OEmpSalProcess = db.EmployeePayroll.Where(e => e.Id == OEmployeePayrollc.Id).Include(e => e.SalaryT).SingleOrDefault();
                        var EmpSalRelTPro = OEmpSalProcess.SalaryT != null ? OEmpSalProcess.SalaryT.Where(e => e.ReleaseDate == null) : null;

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
                        var EmpCPIPro = db.CPIEntryT.Where(e => e.EmployeePayroll_Id == OEmployeePayrollc.Id).Select(e => e.Id).FirstOrDefault();


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
                        var EmpSalRelT = OEmpSalRelT.SalaryT != null ? OEmpSalRelT.SalaryT.Where(e => e.ReleaseDate != null) : null;

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



                    //if (type.ToString() == "OFFICIATING ALLOWANCE")
                    //   { 

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
                    //if (NewPayStruct == null && offpara.NewPayStructOnScreenAppl == true)
                    //{
                    //    Msg.Add("Please select PayStruct");
                    //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //}
                    //if (NewPayStruct != "" && NewPayStruct != null)
                    //{
                    //    var payid = db.PayStruct.Find(int.Parse(NewPayStruct));
                    //    BMSPaymentReq.OfficiatingPayStruct = payid.Id;
                    //}
                    // NKGSB End
                    //}
                    var calendarid = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "FinancialYear" && e.Default == true).SingleOrDefault().Id;
                    var FiCalendarid = Convert.ToInt32(calendarid);

                    var OEmployeePayrollOff = db.EmployeePayroll.Where(e => e.Employee_Id == EmpIdCheck).FirstOrDefault();

                    if (type.SalHeadOperationType.LookupVal.ToUpper() == "OFFICIATING")
                    {

                        FunctAllWFDetails oAttWFDetails = new FunctAllWFDetails
                        {
                            WFStatus = 0,
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
                            BMSPaymentReq.EmployeePayroll_Id = Convert.ToInt32(OEmployeePayrollOff.Id);
                            BMSPaymentReq.PayStruct_Id = Convert.ToInt32(OEmployee.PayStruct_Id);

                            if (EmpOffId > 0)
                            {
                                BMSPaymentReq.OfficiatingEmployeeId = Convert.ToInt32(EmpOffId);
                            }
                            else
                            {
                                BMSPaymentReq.OfficiatingEmployeeId = 0;
                            }
                            BMSPaymentReq.OfficiatingPayStruct = Convert.ToInt32(NewPayStruct);

                            BMSPaymentReq OOffServiceBook = new BMSPaymentReq()
                            {
                                FuncStruct_Id = BMSPaymentReq.FuncStruct_Id,
                                GeoStruct_Id = BMSPaymentReq.GeoStruct_Id,
                                EmployeePayroll_Id = OEmployeePayrollOff.Id,
                                PayStruct_Id = BMSPaymentReq.PayStruct_Id,
                                FromPeriod = BMSPaymentReq.FromPeriod,
                                OfficiatingEmployeeId = BMSPaymentReq.OfficiatingEmployeeId,
                                OfficiatingPayStruct = BMSPaymentReq.OfficiatingPayStruct,
                                Narration = BMSPaymentReq.Narration,
                                ToPeriod = BMSPaymentReq.ToPeriod,
                                DBTrack = BMSPaymentReq.DBTrack,
                                ReleaseDate = null,

                                SalaryHead_Id = SalaryheadId,
                                ProcessMonth = ProMonth,
                                FinancialYear_Id = FiCalendarid,
                                //PayMonth = OfficiatingServiceBook.PayMonth,
                                //OfficiatingParameter = BMSPaymentReq.OfficiatingParameter,
                                OfficiatingParameter = null,
                                //off = OfficiatingServiceBook.StructureRefId,
                                InputMethod = 1,
                                WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "0").FirstOrDefault(),//db.LookupValue.Where(e => e.LookupVal == "1").SingleOrDefault(),
                                OffWFDetails = oAttWFDetails_List
                            };

                            db.BMSPaymentReq.Add(OOffServiceBook);
                            db.SaveChanges();

                            ts.Complete();

                            return Json(new { status = true, responseText = "Data Created Successfully." }, JsonRequestBehavior.AllowGet);

                        }
                    }
                    else
                    {
                        FunctAllWFDetails oAttWFDetails = new FunctAllWFDetails
                        {
                            WFStatus = 0,
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
                            BMSPaymentReq.EmployeePayroll_Id = Convert.ToInt32(OEmployeePayrollOff.Id);
                            BMSPaymentReq.PayStruct_Id = Convert.ToInt32(OEmployee.PayStruct_Id);
                            //BMSPaymentReq.OfficiatingPayStruct = Convert.ToInt32(OEmployee.PayStruct_Id);


                            if (EmpOffId > 0)
                            {
                                BMSPaymentReq.OfficiatingEmployeeId = Convert.ToInt32(EmpOffId);
                            }
                            else
                            {
                                BMSPaymentReq.OfficiatingEmployeeId = 0;
                            }
                            //BMSPaymentReq.OfficiatingPayStruct = Convert.ToInt32(OEmployeeOff.PayStruct_Id);


                            BMSPaymentReq OOffServiceBook = new BMSPaymentReq()
                            {
                                FuncStruct_Id = BMSPaymentReq.FuncStruct_Id,
                                GeoStruct_Id = BMSPaymentReq.GeoStruct_Id,
                                EmployeePayroll_Id = OEmployeePayrollOff.Id,
                                PayStruct_Id = BMSPaymentReq.PayStruct_Id,
                                FromPeriod = BMSPaymentReq.FromPeriod,
                                OfficiatingEmployeeId = BMSPaymentReq.OfficiatingEmployeeId,
                                OfficiatingPayStruct = 0,
                                Narration = BMSPaymentReq.Narration,
                                ToPeriod = BMSPaymentReq.ToPeriod,
                                DBTrack = BMSPaymentReq.DBTrack,
                                ReleaseDate = null,
                                LvNewReq_Id = lvheadids == 0 ? null : lvheadids,
                                SalaryHead_Id = SalaryheadId,
                                ProcessMonth = ProMonth,
                                FinancialYear_Id = FiCalendarid,
                                //PayMonth = OfficiatingServiceBook.PayMonth,
                                OfficiatingParameter = null,
                                //off = OfficiatingServiceBook.StructureRefId,
                                InputMethod = 1,
                                WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "0").FirstOrDefault(),//db.LookupValue.Where(e => e.LookupVal == "1").SingleOrDefault(),
                                OffWFDetails = oAttWFDetails_List
                            };

                            db.BMSPaymentReq.Add(OOffServiceBook);
                            db.SaveChanges();

                            ts.Complete();

                            return Json(new { status = true, responseText = "Data Created Successfully." }, JsonRequestBehavior.AllowGet);

                        }
                    }

                }


            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                    }
                }
                throw;
            }

        }


        public ActionResult CreateCancel(BMSPaymentReq c, FormCollection form, String data) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {

                    string LvCancellist = form["IsCancel"] == "0" ? "" : form["IsCancel"];

                    var ids = Utility.StringIdsToListString(data);
                    var status = ids.Count > 0 ? ids[2] : null;
                    var LvId = Convert.ToInt32(ids[0]);
                    //var LvId = Convert.ToInt32(ids[1]);

                    var LvCancelchk = false;
                    var LeaveId = 0;
                    if (LvId != null)
                    {
                        LeaveId = Convert.ToInt32(LvId);
                    }
                    else
                    {
                        return Json(new { status = true, responseText = "Try Again" }, JsonRequestBehavior.AllowGet);
                    }

                    if (LvCancellist != null)
                    {
                        LvCancelchk = Convert.ToBoolean(LvCancellist);
                        if (LvCancelchk == false)
                        {
                            return Json(new { status = true, responseText = "Set cancel True" }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        return Json(new { status = true, responseText = "Apply leave cancel" }, JsonRequestBehavior.AllowGet);
                    }

                    int EmpId = Convert.ToInt32(SessionManager.EmpId);


                    int lvnewreqid = Convert.ToInt32(LvId);

                    var query = db.BMSPaymentReq
                    .Include(e => e.OffWFDetails)
                    .Where(e => e.Id == lvnewreqid).FirstOrDefault();

                    FunctAllWFDetails oFunctAllWFDetails = new FunctAllWFDetails
                    {
                        WFStatus = 6,
                        Comments = "Cancelled by Myself",
                        DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                    };
                    List<FunctAllWFDetails> oFuncAllWFDetails_List = new List<FunctAllWFDetails>();
                    if (query.OffWFDetails.Count() > 0)
                    {
                        oFuncAllWFDetails_List.AddRange(query.OffWFDetails);
                    }
                    oFuncAllWFDetails_List.Add(oFunctAllWFDetails);

                    query.IsCancel = true;
                    query.TrClosed = true;
                    query.OffWFDetails = oFuncAllWFDetails_List;

                    db.BMSPaymentReq.Attach(query);
                    db.Entry(query).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();

                    return Json(new { status = true, responseText = "Record updated successfully.." }, JsonRequestBehavior.AllowGet);
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
                    return Json(new { status = false, responseText = ex.InnerException }, JsonRequestBehavior.AllowGet);
                }

            }
        }


        public ActionResult GetOfficiating()
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                LookupValue FuncModule = new LookupValue();
                if (!string.IsNullOrEmpty(Convert.ToString(Session["user-module"])))
                {
                    var id = Convert.ToString(Session["user-module"]);
                    var lookupdata = db.Lookup
                               .Include(e => e.LookupValues)
                               .Where(e => e.Code == "601").SingleOrDefault();
                    FuncModule = lookupdata.LookupValues.Where(e => e.LookupVal.ToUpper() == id.ToUpper()).SingleOrDefault();
                }
                if (FuncModule == null)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                string AccessRight = "";
                if (!string.IsNullOrEmpty(Convert.ToString(Session["user-module"])))
                {
                    AccessRight = Convert.ToString(Session["auho"]);
                }
                var EmpIds = UserManager.GeEmployeeList(FuncModule.Id, 0);
                List<EssPortal.Process.PortalRights.AccessRightsData> EmpidsWithfunsub = UserManager.GeEmployeeListWithfunsub(FuncModule.Id, 0, AccessRight);
                if (EmpidsWithfunsub.Any(e => e.SubModuleName != null))
                {
                    EmpidsWithfunsub = EmpidsWithfunsub.Where(e => e.SubModuleName == "OFFICIATING" || e.SubModuleName == "LTA").ToList();

                }
               
                List<int> EmpPayrollid = new List<int>();

                foreach (var item in EmpidsWithfunsub)
                {
                    EmpPayrollid.AddRange(item.ReportingEmployee);                 
                }

                var EmployeePay = db.EmployeePayroll.Where(e => EmpPayrollid.Contains(e.Employee.Id)).ToList().Select(e => e.Id);

                if (EmpIds == null)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }


                //var LvList = db.BMSPaymentReq
                //    .Include(e => e.EmployeePayroll)
                //    .Include(e => e.EmployeePayroll.Employee)
                //    .Include(e => e.EmployeePayroll.Employee.ReportingStructRights)
                //    .Include(e => e.EmployeePayroll.Employee.ReportingStructRights.Select(a => a.AccessRights))
                //    .Include(e => e.EmployeePayroll.Employee.ReportingStructRights.Select(a => a.AccessRights.ActionName))
                //    .Include(e => e.OffWFDetails)
                //    .Include(e => e.WFStatus)
                //    .Where(e => EmployeePay.Contains(e.EmployeePayroll.Id)).ToList();
                   //.Where(e => EmpIds.Contains(e.EmployeePayroll.Employee.Id)).ToList();

                List<ItinvestmentpaymentClass1> ListreturnLvnewClass = new List<ItinvestmentpaymentClass1>();
                ListreturnLvnewClass.Add(new ItinvestmentpaymentClass1
                {
                    Emp = "Employee",
                    FromDate = "From Date",
                    ToDate = "To Date",
                    PayMonth = "Pay Month",
                    SalaryHead = "Salary Head",
                    PayStruct = "On Officiating Grade",

                });

                List<BMSPaymentReq> LvList = new List<BMSPaymentReq>();

                foreach (var item1 in EmpidsWithfunsub)
                {
                    if (item1.ReportingEmployee.Count() > 0)
                    {
                        var temp = db.BMSPaymentReq
                       .Include(e => e.EmployeePayroll)
                       .Include(e => e.EmployeePayroll.Employee)
                       .Include(e => e.EmployeePayroll.Employee.ReportingStructRights)
                       .Include(e => e.EmployeePayroll.Employee.ReportingStructRights.Select(a => a.AccessRights))
                       .Include(e => e.EmployeePayroll.Employee.ReportingStructRights.Select(a => a.AccessRights.ActionName))
                       .Include(e => e.OffWFDetails)
                       .Include(e => e.WFStatus)
                       .Where(e => EmployeePay.Contains(e.EmployeePayroll.Id)).ToList();
                        // .Include(e => e.FunctAttendanceT.Select(a => a.WFStatus));
                        //.Where(e => EmpIds.Contains(e.Employee.Id)).ToList();

                       // var LvList = temp.Where(e => item1.ReportingEmployee.Contains(e.Employee.Id)).ToList();
                         LvList = temp.Where(e => item1.ReportingEmployee.Contains(e.EmployeePayroll.Employee.Id)).ToList();

                    }
                }
                var LvIds = UserManager.FilterOfficiating(LvList.ToList(), Convert.ToString(Session["auho"]), Convert.ToInt32(SessionManager.CompanyId));

                var session = Session["auho"].ToString().ToUpper();
                foreach (var item in LvIds)
                {

                    var query = db.BMSPaymentReq.Include(e => e.EmployeePayroll)
                               .Include(e => e.EmployeePayroll.Employee)
                               .Include(e => e.EmployeePayroll.Employee.EmpName)
                               .Where(e => e.Id == item).SingleOrDefault();

                    //TempData["EmployeeId"] = query.EmployeePayroll.Employee_Id; 

                    string authority = Convert.ToString(Session["auho"]);


                    int WfStatusNew = query.OffWFDetails.ToList().OrderByDescending(e => e.Id).FirstOrDefault().WFStatus;
                    if (authority.ToUpper() == "SANCTION" && WfStatusNew == 0)
                    {
                        ListreturnLvnewClass.Add(new ItinvestmentpaymentClass1
                        {
                            RowData = new ChildGetLvNewReqClass2
                            {
                                LvNewReq = query.Id.ToString(),
                                EmpLVid = query.Id.ToString(),
                                IsClose = query.EmployeePayroll.Employee.ReportingStructRights
                                .Where(a => a.AccessRights != null && a.AccessRights.ActionName.LookupVal.ToUpper() == session)
                                .Select(a => a.AccessRights.IsClose).FirstOrDefault().ToString(),
                                LvHead_Id = "",
                            },
                         
                            Emp = query.EmployeePayroll.Employee.EmpCode + " " + query.EmployeePayroll.Employee.EmpName.FullNameFML,
                            FromDate = query.FromPeriod.Value.ToShortDateString(),
                            ToDate = query.ToPeriod.Value.ToShortDateString(),
                            PayMonth = query.PayMonth == null ? "" : query.PayMonth,
                            //SalaryHead = query.SalaryHead_Id.ToString(),
                            SalaryHead = db.SalaryHead.Where(e => e.Id == query.SalaryHead_Id).FirstOrDefault().Name,
                            PayStruct = db.PayStruct.Include(e => e.Grade).Where(e => e.Id == query.OfficiatingPayStruct).FirstOrDefault() == null ? "" : db.PayStruct.Include(e => e.Grade).Where(e => e.Id == query.OfficiatingPayStruct).FirstOrDefault().Grade.FullDetails.ToString()
                          });

                        }
                     }

                  
               
               var result = ListreturnLvnewClass.ToList();

                if (result != null && result.Count > 0)
                {
                    return Json(new { status = true, data = result, responseText = "Okk" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = false, data = result, responseText = "Error" }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        public class ItinvestmentpaymentClass1
        {
          
            public string Emp { get; set; }
            public string FromDate { get; set; }
            public string ToDate { get; set; }
            public string PayMonth { get; set; }
            public string PayStruct { get; set; }
            public string SalaryHead { get; set; }
            public ChildGetLvNewReqClass2 RowData { get; set; }
        }
        public class ChildGetLvNewReqClass2
        {
            public string LvNewReq { get; set; }
            public string EmpLVid { get; set; }
            public string IsClose { get; set; }
            public string LvHead_Id { get; set; }

        }

        public ActionResult SalHeadType(string saltype)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                TempData["saltype"] = saltype;
                int Salheadid = db.SalaryHead.Where(e => e.Name == saltype).FirstOrDefault().Id;
                var SalHead = db.SalaryHead.Include(e => e.SalHeadOperationType).Include(e => e.ProcessType).Where(e => e.Id == Salheadid).FirstOrDefault();

                var qurey = SalHead.SalHeadOperationType.LookupVal.ToUpper();

                return Json(qurey, JsonRequestBehavior.AllowGet);
            }
           

        }

        public ActionResult lvHeadType(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var LeaveData = db.LvHead.Where(e => e.LTAAppl == true).ToList();

                var EmpId = Convert.ToInt32(SessionManager.EmpId);

                //var leavecalendarid = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "LEAVECALENDAR" && e.Default == true).SingleOrDefault();

                var LeaveheadId = LeaveData.Select(e => e.Id).ToList();

                //var Leaves = db.LvNewReq.Include(e => e.LeaveHead).Where(e => LeaveheadId.Contains(e.LeaveHead.Id) && e.EmployeeLeave_Id == EmpId && e.LeaveCalendar.Id == leavecalendarid.Id).ToList();

                EmployeeLeave oEmployeeLeave = db.EmployeeLeave.Include(e => e.Employee).Include(e => e.LvNewReq)
                                                               .Include(e => e.LvNewReq.Select(a => a.LeaveHead))
                                                               .Include(e => e.LvNewReq.Select(a => a.LeaveCalendar))
                                                               .Include(e => e.LvNewReq.Select(a => a.WFStatus))
                                                               .Where(e => e.Employee.Id == EmpId).OrderBy(e => e.Id).SingleOrDefault();
                //if (oEmployeeLeave != null)
                //{

                var LvCalendarFilter = oEmployeeLeave.LvNewReq.Where(e => LeaveheadId.Contains(e.LeaveHead.Id)).OrderBy(e => e.Id).ToList();

                var LvOrignal_id = LvCalendarFilter.Where(e => e.LvOrignal != null).OrderBy(e => e.Id).Select(e => e.LvOrignal.Id).ToList();
                var AntCancel = LvCalendarFilter.Where(e => e.IsCancel == false && e.TrReject == false && e.TrClosed == true).OrderBy(e => e.Id).ToList();
                var listLvs = AntCancel.Where(e => !LvOrignal_id.Contains(e.Id) && e.WFStatus.LookupVal != "0" && e.WFStatus.LookupVal != "2" && e.WFStatus.LookupVal != "3").OrderBy(e => e.Id).ToList();
                //&& e.WFStatus.LookupVal != "0" 0 for enter opening balance

                // }
                var selected = (Object)null;

                if (!string.IsNullOrEmpty(data2))
                {
                    selected = data2;
                }

                SelectList s = new SelectList(listLvs, "Id", "FullDetails", selected);
                return Json(s, JsonRequestBehavior.AllowGet);

            }

        }

        public ActionResult GetMyEmpOfficiating()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (string.IsNullOrEmpty(Convert.ToString(SessionManager.EmpLvId)))
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                var Emp = Convert.ToInt32(SessionManager.EmpId);

                var EmployeePayroll = db.EmployeePayroll.Where(e => e.Employee_Id == Emp).FirstOrDefault();

                var db_data = db.BMSPaymentReq
                  .Include(e => e.EmployeePayroll)
                  .Include(e => e.OffWFDetails)
                  .Include(e => e.WFStatus)
                  .Where(e => e.EmployeePayroll.Id == EmployeePayroll.Id).ToList();


                List<GetOfficiatingClass> returndata = new List<GetOfficiatingClass>();
                returndata.Add(new GetOfficiatingClass
                {
                    //Emp = "Employee",
                    FromDate = "From Date",
                    ToDate = "To Date",
                    PayMonth = "Pay Month" + " ",
                    SalaryHeadId = "SalaryHeadId",
                    PayStruct = "On Officiating Grade" + " ",
                    Status = "Status",
                    OfficiatingId = "Id"

                });

                if (db_data != null)
                {
                    foreach (var item in db_data.OrderByDescending(a => a.FromPeriod))
                    {
                        var InvestmentDate = item.FromPeriod != null ? item.FromPeriod.Value.ToShortDateString() : null;
                        var Status = "--";
                        if (item.OffWFDetails.Count > 0)
                        {
                            Status = Utility.GetStatusName().Where(e => e.Key == item.OffWFDetails.LastOrDefault().WFStatus.ToString()).Select(e => e.Key).SingleOrDefault();
                        }
                        if (Status == "5")
                        {
                            Status = "Approved By HRM (M)";
                        }
                        else if (Status == "0")
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

                        returndata.Add(new GetOfficiatingClass
                        {
                            RowData = new ChildGetLvNewReqClass
                            {
                                LvNewReq = item.Id.ToString(),
                                EmpLVid = db_data.ToString(),
                                IsClose = "",
                                Status = Status,
                                LvHead_Id = "",
                            },

                            //Emp = item.EmployeeId == null ? "" : item.EmployeeId.ToString(),
                            FromDate = item.FromPeriod == null ? "0" : item.FromPeriod.Value.ToShortDateString(),
                            ToDate = item.ToPeriod == null ? "0" : item.ToPeriod.Value.ToShortDateString(),
                            PayMonth = item.PayMonth == null ? "" : item.PayMonth,
                            //SalaryHeadId = item.SalaryHead_Id == null ? "0" : item.SalaryHead_Id.ToString(),
                            SalaryHeadId = item.SalaryHead_Id == null ? "" : db.SalaryHead.Where(e => e.Id == item.SalaryHead_Id).FirstOrDefault().Name,
                            //PayStruct = db.PayStruct.Include(e => e.Grade).Where(e => e.Id == item.OfficiatingPayStruct).FirstOrDefault().Grade.FullDetails.ToString() == null ? "" :  db.PayStruct.Include(e => e.Grade).Where(e => e.Id == item.OfficiatingPayStruct).FirstOrDefault().Grade.FullDetails.ToString(),
                            PayStruct = db.PayStruct.Include(e => e.Grade).Where(e => e.Id == item.OfficiatingPayStruct).FirstOrDefault() == null ? "" : db.PayStruct.Include(e => e.Grade).Where(e => e.Id == item.OfficiatingPayStruct).FirstOrDefault().Grade.ToString() == null ? "" : db.PayStruct.Include(e => e.Grade).Where(e => e.Id == item.OfficiatingPayStruct).FirstOrDefault().Grade.FullDetails.ToString(),
                            Status = Status,
                            OfficiatingId = item.Id.ToString()

                        });
                    }


                    return Json(new { status = true, data = returndata }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public class GetOfficiatingClass
        {
            //public string Emp { get; set; }
            public string FromDate { get; set; }
            public string ToDate { get; set; }
            public string PayMonth { get; set; }
            public string SalaryHeadId { get; set; }
            public string PayStruct { get; set; }
            public string Status { get; set; }
            public string OfficiatingId { get; set; }

            public ChildGetLvNewReqClass RowData { get; set; }
        }

        public class ChildGetLvNewReqClass
        {
            public string LvNewReq { get; set; }
            public string EmpLVid { get; set; }
            public string Status { get; set; }
            public string LvHead_Id { get; set; }
            public string IsClose { get; set; }

        }

        public class getdata
        {
            public string FromDate { get; set; }
            public string ToDate { get; set; }
            public string PayMonth { get; set; }
            public string OfficiatingParameterId { get; set; }
            public string SalaryHeadId { get; set; }
            public string SalOperationType { get; set; }
            public string OfficiatingParameterName { get; set; }
            public string PayStructdetails { get; set; }
            public string PayStructId { get; set; }
            //public string EmpPaystructid { get; set; }
            public string OfficiatingEmployeeId { get; set; }
            public string InchargeName { get; set; }
            public string Remark { get; set; }
            public string OfficiatingId { get; set; }
            public string SanctionComment { get; set; }
            public string ApporavalComment { get; set; }
            public string Wf { get; set; }
            public string lvdetailsid { get; set; }
            public string lvfulldetails { get; set; }

        }


        public ActionResult GetMyOfficiatingData(string data)
        {
            List<getdata> return_data = new List<getdata>();
            using (DataBaseContext db = new DataBaseContext())
            {
                BMSPaymentReq officiating = null;
                string localpath = "";
                string authority = Convert.ToString(Session["auho"]);
                var WfStatus = new List<Int32>();
                var SanctionStatus = new List<Int32>();
                var ApporvalStatus = new List<Int32>();
                if (authority.ToUpper() == "SANCTION")
                {
                    WfStatus.Add(1);
                    WfStatus.Add(2);
                }
                else if (authority.ToUpper() == "APPROVAL")
                {
                    WfStatus.Add(1);
                    WfStatus.Add(2);
                }
                else if (authority.ToUpper() == "MYSELF")
                {
                    SanctionStatus.Add(1);
                    SanctionStatus.Add(2);

                    ApporvalStatus.Add(3);
                    ApporvalStatus.Add(4);
                }
                var Emp = Convert.ToInt32(SessionManager.EmpId);
                var ids = Utility.StringIdsToListString(data);
                var id = Convert.ToInt32(ids[0]);

                var listOfObject = db.BMSPaymentReq
                    .Include(e => e.OfficiatingParameter)
                    .Include(e => e.EmployeePayroll)
                    .Include(e => e.EmployeePayroll.Employee)
                    .Include(e => e.EmployeePayroll.Employee.EmpName)
                    .Include(e => e.OffWFDetails)
                    .Include(e => e.SalaryHead)
                     .Include(e => e.SalaryHead.SalHeadOperationType)
                    .Include(e => e.WFStatus)
                    .Include(e => e.LvNewReq)
                    .Include(e => e.LvNewReq.LeaveHead)
                    .Where(e => e.Id == id).AsEnumerable().Select
                (e => new
                {
                    FromDate = e.FromPeriod != null ? e.FromPeriod.Value.ToShortDateString() : null,
                    ToDate = e.ToPeriod != null ? e.ToPeriod.Value.ToShortDateString() : null,
                    PayMonth = e.PayMonth == null ? "" : e.PayMonth,
                    OfficiatingParameterId = e.OfficiatingParameter_Id == null ? 0 : e.OfficiatingParameter_Id,
                    SalaryHeadId = e.SalaryHead_Id,
                    SalOperationType = e.SalaryHead.Name,
                    OfficiatingParameterName = e.OfficiatingParameter == null ? "" : e.OfficiatingParameter.Name == null ? "" : e.OfficiatingParameter.Name,
                    PayStructId = e.OfficiatingPayStruct == null ? 0 : e.OfficiatingPayStruct,
                    geostruct = e.GeoStruct_Id,
                    OfficiatingEmployeeId = e.OfficiatingEmployeeId == null ? 0 : e.OfficiatingEmployeeId,
                    Remark = e.Narration,
                    OfficiatingId = e.Id == null ? 0 : e.Id,
                    LvNewReqid = e.LvNewReq_Id == null ? 0 : e.LvNewReq_Id,
                    lvfulldetails = e.LvNewReq == null ? null : e.LvNewReq.FullDetails,
                    SanctionComment = e.OffWFDetails != null && e.OffWFDetails.Count > 0 ? e.OffWFDetails.Where(z => SanctionStatus.Contains(z.WFStatus)).OrderByDescending(a => a.Id).Select(s => s.Comments).LastOrDefault() : null,
                    ApporavalComment = e.OffWFDetails != null && e.OffWFDetails.Count > 0 ? e.OffWFDetails.Where(z => ApporvalStatus.Contains(z.WFStatus)).OrderByDescending(a => a.Id).Select(s => s.Comments).LastOrDefault() : null,
                    Wf = e.OffWFDetails != null && e.OffWFDetails.Count > 0 ? e.OffWFDetails.Where(z => WfStatus.Contains(z.WFStatus)).OrderByDescending(s => s.Id).Select(a => a.Comments).LastOrDefault() : null
                }).ToList();

                foreach (var item in listOfObject)
                {

                    var empname = db.Employee.Include(e => e.EmpName).Where(e => e.Id == item.OfficiatingEmployeeId).FirstOrDefault();
                    var paystructcode = db.PayStruct.Include(e => e.Grade).Where(e => e.Id == item.PayStructId).FirstOrDefault();
                    //var EmpPaystructId = db.PayStruct.Include(e => e.Grade).Where(e => e.Id == item.EmpPaystructId).FirstOrDefault();
                    var geostruct = db.GeoStruct.Where(e => e.Id == item.geostruct).FirstOrDefault();


                    return_data.Add(new getdata
                    {
                        FromDate = item.FromDate,
                        ToDate = item.ToDate,
                        PayMonth = item.PayMonth == null ? " " : item.PayMonth,
                        OfficiatingParameterId = item.OfficiatingParameterId.ToString(),
                        SalaryHeadId = item.SalaryHeadId.ToString(),
                        SalOperationType = item.SalOperationType.ToString(),
                        OfficiatingParameterName = item.OfficiatingParameterName,
                        PayStructdetails = paystructcode == null ? "" : paystructcode.Grade == null ? "" : paystructcode.Grade.FullDetails,
                        PayStructId = paystructcode == null ? "" : paystructcode.Id.ToString(),
                        OfficiatingEmployeeId = item.OfficiatingEmployeeId != null ? item.OfficiatingEmployeeId.ToString() : null,
                        InchargeName = empname == null ? "" : empname.EmpName == null ? "" : empname.EmpName.FullNameFML,
                        Remark = item.Remark,
                        OfficiatingId = item.OfficiatingId.ToString(),
                        lvdetailsid = item.LvNewReqid == null ? "0" : item.LvNewReqid.ToString(),
                        lvfulldetails = item.lvfulldetails == null ? "" : item.lvfulldetails,
                        SanctionComment = item.SanctionComment,
                        ApporavalComment = item.ApporavalComment,
                        Wf = item.Wf,

                    });
                }

                return Json(new { data = return_data, status = false }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetOfficiatingData(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string authority = Convert.ToString(Session["auho"]);
                var WfStatus = new List<Int32>();
                var SanctionStatus = new List<Int32>();
                var ApporvalStatus = new List<Int32>();
                if (authority.ToUpper() == "SANCTION")
                {
                    WfStatus.Add(1);
                    WfStatus.Add(2);
                }
                else if (authority.ToUpper() == "APPROVAL")
                {
                    WfStatus.Add(1);
                    WfStatus.Add(2);
                }
                else if (authority.ToUpper() == "MYSELF")
                {
                    SanctionStatus.Add(1);
                    SanctionStatus.Add(2);

                    ApporvalStatus.Add(3);
                    ApporvalStatus.Add(4);
                }
                var ids = Utility.StringIdsToListString(data);
                var status = ids.Count > 0 ? ids[2] : null;
                var id = Convert.ToInt32(ids[0]);
                var officitingId = Convert.ToInt32(ids[1]);

                TempData["officitingId"] = officitingId;
                //TempData["officitingId1"] = officitingId;

                var EmpId = db.BMSPaymentReq.Where(e => e.Id == officitingId).SingleOrDefault().EmployeePayroll_Id;


                var paystructdetails = db.BMSPaymentReq.Where(e => e.Id == officitingId).FirstOrDefault().OfficiatingPayStruct;
                var emppaystructdetails = db.BMSPaymentReq.Where(e => e.Id == officitingId).FirstOrDefault().PayStruct_Id;
                var empfuncstruct = db.BMSPaymentReq.Where(e => e.Id == officitingId).FirstOrDefault().FuncStruct_Id;
                var emplocation = db.BMSPaymentReq.Where(e => e.Id == officitingId).FirstOrDefault().GeoStruct_Id;
                var paystructcode = db.PayStruct.Include(e => e.Grade).Where(e => e.Id == paystructdetails).FirstOrDefault();
                var emppaystructcode = db.PayStruct.Include(e => e.Grade).Where(e => e.Id == emppaystructdetails).FirstOrDefault();
                var empfuncstructcode = db.FuncStruct.Include(e => e.Job).Where(e => e.Id == empfuncstruct).FirstOrDefault();
                var emplocationcode = db.GeoStruct.Include(e => e.Location).Include(e => e.Location.BusinessCategory).Where(e => e.Id == emplocation).FirstOrDefault();

                var listOfObject = db.BMSPaymentReq.Include(e => e.LvNewReq).Include(e => e.LvNewReq.LeaveHead).Include(e => e.SalaryHead).Include(e => e.SalaryHead.ProcessType).Include(e => e.OfficiatingParameter).Include(e => e.OffWFDetails).Include(e => e.WFStatus).Where(e => e.Id == officitingId).AsEnumerable().Select
                (e => new
                {

                    FromDate = e.FromPeriod != null ? e.FromPeriod.Value.ToShortDateString() : null,
                    ToDate = e.ToPeriod != null ? e.ToPeriod.Value.ToShortDateString() : null,
                    PayMonth = e.PayMonth == null ? "" : e.PayMonth,
                    ProcessMonth = e.ProcessMonth == null ? "" : e.ProcessMonth,
                    OfficiatingParameterName = e.OfficiatingParameter == null ? "" : e.OfficiatingParameter.Name,
                    PayStructDetails = paystructcode == null ? "" : paystructcode.Grade == null ? "" : paystructcode.Grade.FullDetails,
                    PayStructId = e.OfficiatingPayStruct == null ? 0 : e.OfficiatingPayStruct,
                    LvDetails = e.LvNewReq == null ? "" : e.LvNewReq.FullDetails,
                    SalaryHeadId = e.SalaryHead_Id,
                    SalOperationType = e.SalaryHead.Name,
                    ProcessType = e.SalaryHead == null ? "" : e.SalaryHead.ProcessType.LookupVal,
                    Grade = emppaystructcode == null ? "" : emppaystructcode.Grade == null ? "" : emppaystructcode.Grade.FullDetails,
                    Designation = empfuncstructcode == null ? "" : empfuncstructcode.Job == null ? "" : empfuncstructcode.Job.Name,
                    Location = emplocationcode.Location == null ? "" : emplocationcode.Location.BusinessCategory == null ? "" : emplocationcode.Location.BusinessCategory.LookupVal,
                    IsClose = status,
                    IsCancel = e.IsCancel,
                    Remark = e.Narration,
                    OfficiatingId = e.Id,
                    SanctionComment = e.OffWFDetails != null && e.OffWFDetails.Count > 0 ? e.OffWFDetails.Where(z => SanctionStatus.Contains(z.WFStatus)).OrderByDescending(a => a.Id).Select(s => s.Comments).LastOrDefault() : null,
                    ApporavalComment = e.OffWFDetails != null && e.OffWFDetails.Count > 0 ? e.OffWFDetails.Where(z => ApporvalStatus.Contains(z.WFStatus)).OrderByDescending(a => a.Id).Select(s => s.Comments).LastOrDefault() : null,
                    Wf = e.OffWFDetails != null && e.OffWFDetails.Count > 0 ? e.OffWFDetails.Where(z => WfStatus.Contains(z.WFStatus)).OrderByDescending(s => s.Id).Select(a => a.Comments).LastOrDefault() : null
                }).FirstOrDefault();
                TempData["IsClose"] = status;
                TempData["EMPId"] = EmpId;
                TempData["SalOperationType"] = listOfObject.SalOperationType;
                //TempData["EMPId1"] = EmpId;
                //return Json(new { data = listOfObject, status = true }, JsonRequestBehavior.AllowGet);
                return Json(new Object[] { listOfObject, status, EmpId, officitingId, JsonRequestBehavior.AllowGet });
            }
        }

        public ActionResult UpdateStatus(BMSPaymentReq LvReq, FormCollection form, String data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var isClose = TempData["IsClose"];
                    string authority = form["authority"] == null ? null : form["authority"];
                    if (authority == null && isClose == null)
                    {
                        return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                    }

                    var ProcessMonth = form["ProcessMonth"] == null ? null : form["ProcessMonth"];
                    var Fulldate = form["PayMonth"] == null ? null : form["PayMonth"];
                    var PayMonthdate = Convert.ToDateTime(Fulldate);
                    var PayDate = PayMonthdate.ToString("dd/MM/yyyy");
                    var PayMondate = Convert.ToDateTime(Fulldate);
                    var PayMonth = PayMondate.ToString("MM/yyyy");

                    //var ids1 = Utility.StringIdsToListString(data);
                    //var officitingId1 = Convert.ToInt32(ids1[1]);
                    //var payMonth1 = db.BMSPaymentReq.Where(e => e.Id == officitingId1).SingleOrDefault().PayMonth;

                    string NewPayStruct = form["NewPayT-table"] == "0" ? "" : form["NewPayT-table"];
                    string MySelf = form["MySelf"] == null ? "false" : form["MySelf"];
                    string Sanction = form["Sanction"];
                    string Approval = form["Approval"];
                    string HR = form["HR"] == null ? null : form["HR"];
                    string ReasonMySelf = form["ReasonMySelf"] == null ? null : form["ReasonMySelf"];
                    string ReasonSanction = form["ReasonSanction"];
                    string ReasonApproval = form["ReasonApproval"];
                    string ReasonHr = form["ReasonHr"] == null ? null : form["ReasonHr"];
                    bool SanctionRejected = false;
                    bool HrRejected = false;
                    var ids = Utility.StringIdsToListString(data);
                    var status = ids.Count > 0 ? ids[2] : null;
                    var id = Convert.ToInt32(ids[0]);
                    var officitingId = Convert.ToInt32(ids[1]);
                    //using (DataBaseContext db = new DataBaseContext())
                    //{
                    var qurey = db.BMSPaymentReq.Include(e => e.OffWFDetails)
                        .Include(e => e.WFStatus).Where(e => e.Id == officitingId).SingleOrDefault();


                    if (ProcessMonth.Replace(",", "")!= qurey.ProcessMonth)
                    {
                        return Json(new Utility.JsonClass { status = false, responseText = "Please Select Correct Process Month" }, JsonRequestBehavior.AllowGet);
                    }

                    if (Sanction.ToString() == "true")
                    {
                        if (PayMonth.Replace(",", "") != qurey.PayMonth)
                        {
                            return Json(new Utility.JsonClass { status = false, responseText = "Please Select Correct Payment Month" }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    if (authority.ToUpper() == "MYSELF")
                    {
                        qurey.Narration = ReasonMySelf;
                        qurey.IsCancel = true;
                        qurey.TrClosed = true;
                        qurey.WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "6").FirstOrDefault(); //db.LookupValue.Where(e => e.LookupVal == "6").SingleOrDefault();
                    }
                    else if (authority.ToUpper() == "SANCTION")
                    {
                        if (Sanction == null)
                        {
                            return Json(new Utility.JsonClass { status = false, responseText = "Please Select The Sanction Status....." }, JsonRequestBehavior.AllowGet);

                        }
                        if (ReasonSanction == "")
                        {
                            return Json(new Utility.JsonClass { status = false, responseText = "Please Enter The Reason...." }, JsonRequestBehavior.AllowGet);

                        }

                        if (Convert.ToBoolean(Sanction) == true)
                        {
                            //sanction yes -1
                            var LvWFDetails = new FunctAllWFDetails
                            {
                                WFStatus = 1,
                                Comments = ReasonSanction,
                                DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName }
                            };
                            qurey.OffWFDetails.Add(LvWFDetails);
                            qurey.WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "1").FirstOrDefault(); // db.LookupValue.Where(e => e.LookupVal == "1").SingleOrDefault();
                            qurey.TrClosed = Convert.ToBoolean(isClose) == true ? true : false;
                            if (qurey.TrClosed == true)
                            {
                                qurey.ReleaseDate = DateTime.Now;
                                qurey.ReleaseFlag = true;
                            }

                        }
                        else if (Convert.ToBoolean(Sanction) == false)
                        {
                            //sanction no -2
                            var LvWFDetails = new FunctAllWFDetails
                            {
                                WFStatus = 2,
                                Comments = ReasonSanction,
                                DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName }
                            };
                            qurey.OffWFDetails.Add(LvWFDetails);
                            qurey.WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "2").FirstOrDefault(); // db.LookupValue.Where(e => e.LookupVal == "2").SingleOrDefault();
                            qurey.TrClosed = true;
                            SanctionRejected = true;
                        }
                    }
                    else if (authority.ToUpper() == "APPROVAL")//Hr
                    {
                        if (Approval == null)
                        {
                            return Json(new Utility.JsonClass { status = false, responseText = "Please Select The Approval Status....." }, JsonRequestBehavior.AllowGet);

                        }
                        if (ReasonApproval == "")
                        {
                            return Json(new Utility.JsonClass { status = false, responseText = "Please Enter The Reason...." }, JsonRequestBehavior.AllowGet);

                        }

                        if (Convert.ToBoolean(Approval) == true)
                        {
                            //approval yes-3
                            var LvWFDetails = new FunctAllWFDetails
                            {
                                WFStatus = 3,
                                Comments = ReasonApproval,
                                DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName }
                            };
                            qurey.TrClosed = Convert.ToBoolean(isClose) == true ? true : false;
                            if (qurey.TrClosed == true)
                            {
                                qurey.ReleaseDate = DateTime.Now;
                                qurey.ReleaseFlag = true;
                            }
                            qurey.OffWFDetails.Add(LvWFDetails);
                            qurey.WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "3").FirstOrDefault(); // db.LookupValue.Where(e => e.LookupVal == "3").SingleOrDefault();

                        }
                        else if (Convert.ToBoolean(Approval) == false)
                        {
                            //approval no-4
                            var LvWFDetails = new FunctAllWFDetails
                            {
                                WFStatus = 4,
                                Comments = ReasonApproval,
                                DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName }
                            };
                            qurey.OffWFDetails.Add(LvWFDetails);
                            qurey.WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "4").FirstOrDefault(); // db.LookupValue.Where(e => e.LookupVal == "4").SingleOrDefault();
                            qurey.TrClosed = true;
                            HrRejected = true;
                        }
                    }
                    else if (authority.ToUpper() == "RECOMMAND")
                    {

                    }

                    using (TransactionScope ts = new TransactionScope())
                    {
                        //if someone reject lv
                        if (SanctionRejected == true || HrRejected == true)
                        {
                            qurey.TrReject = true;
                        }

                        var ProcessMonth1 = ProcessMonth.Replace(",", "");

                        var SalOperationType = TempData["SalOperationType"];



                        if (SalOperationType.ToString() == "OFFICIATING ALLOWANCE")
                        {
                            var iPaymonth = PayMonth.Replace(",", "");
                            qurey.PayMonth = iPaymonth;
                            qurey.ProcessMonth = ProcessMonth1;
                            qurey.OfficiatingPayStruct = qurey.OfficiatingPayStruct;

                            db.BMSPaymentReq.Attach(qurey);
                            db.Entry(qurey).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            db.Entry(qurey).State = System.Data.Entity.EntityState.Detached;
                            ts.Complete();
                            return Json(new Utility.JsonClass { status = true, responseText = "Record Updated..!" }, JsonRequestBehavior.AllowGet);

                        }
                        else
                        {
                            if (Convert.ToBoolean(Sanction) == false || Convert.ToBoolean(Approval) == false)
                            {
                                db.BMSPaymentReq.Attach(qurey);
                                db.Entry(qurey).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(qurey).State = System.Data.Entity.EntityState.Detached;
                                ts.Complete();
                                return Json(new Utility.JsonClass { status = true, responseText = "Record Updated..!" }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                var paymentt = db.BMSPaymentReq.Where(e => e.Id == officitingId).SingleOrDefault();
                                var iPaymonth = PayMonth.Replace(",", "");
                                qurey.PayMonth = iPaymonth;

                                var calendarid = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "FinancialYear" && e.Default == true).SingleOrDefault().Id;
                                var FiCalendarid = Convert.ToInt32(calendarid);

                                YearlyPaymentT objofyearly = new YearlyPaymentT()
                                {
                                    EmployeePayroll_Id = paymentt.EmployeePayroll_Id,
                                    GeoStruct_Id = paymentt.GeoStruct_Id,
                                    FuncStruct_Id = paymentt.FuncStruct_Id,
                                    PayStruct_Id = paymentt.PayStruct_Id,
                                    SalaryHead_Id = paymentt.SalaryHead_Id,
                                    FromPeriod = paymentt.FromPeriod,
                                    ToPeriod = paymentt.ToPeriod,
                                    ProcessMonth = ProcessMonth1,
                                    PayMonth = qurey.PayMonth,
                                    TDSAmount = paymentt.TDSAmount,
                                    OtherDeduction = paymentt.OtherDeduction,
                                    AmountPaid = paymentt.AmountPaid,
                                    // FinancialYear_Id = paymentt.FinancialYear_Id,
                                    FinancialYear_Id = FiCalendarid,
                                    Narration = paymentt.Narration,
                                    ReleaseDate = qurey.ReleaseDate,
                                    ReleaseFlag = qurey.ReleaseFlag,
                                    DBTrack = paymentt.DBTrack,
                                };

                                db.YearlyPaymentT.Add(objofyearly);
                                db.SaveChanges();

                                qurey.YearlyPaymentT = objofyearly;
                                //OFAT.AddRange(OEmployeePayroll.YearlyPaymentT);
                                //aa.YearlyPaymentT = OFAT;
                                db.BMSPaymentReq.Attach(qurey);
                                db.Entry(qurey).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(qurey).State = System.Data.Entity.EntityState.Detached;
                                ts.Complete();
                                return Json(new Utility.JsonClass { status = true, responseText = "Record Updated..!" }, JsonRequestBehavior.AllowGet);
                            }
                           
                        }

                    }
                    // }
                }

                catch (DbEntityValidationException e)
                {
                    foreach (var eve in e.EntityValidationErrors)
                    {
                        Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                            eve.Entry.Entity.GetType().Name, eve.Entry.State);
                        foreach (var ve in eve.ValidationErrors)
                        {
                            Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                                ve.PropertyName, ve.ErrorMessage);
                        }
                    }
                    throw;
                }
            }
        }


        public ActionResult EditSave(BMSPaymentReq BMSPaymentReq, FormCollection form, string forwardadtad) //Create submit
        {
            List<string> Msg = new List<string>();

            using (DataBaseContext db = new DataBaseContext())
            {

                var Emp = Convert.ToInt32(SessionManager.EmpId);
                string[] values = (forwardadtad.Split(new string[] { "," }, StringSplitOptions.None));
                int data = Convert.ToInt32(values[0]);
                var ProMonth = form["ProcessMonth"] == "0" ? null : form["ProcessMonth"];
                //var type = TempData["saltype"];
                var Salheadlist1 = form["Salheadlist"] == "0" ? null : form["Salheadlist"];
                var Salheadlist = Convert.ToInt32(Salheadlist1);
                var type = db.SalaryHead.Where(e => e.Id == Salheadlist).FirstOrDefault().Name;

                var EmpOff = form["Incharge_id"] == "0" ? null : form["Incharge_id"];
                string OffActivity = form["OfficiatingParameterlist"] == "0" ? "" : form["OfficiatingParameterlist"];
                string NewPayStruct = form["NewPayT-table"] == "0" ? "" : form["NewPayT-table"];
                int CompId = 0;
                if (SessionManager.UserName != null)
                {
                    CompId = Convert.ToInt32(Session["CompId"]);
                }
                Employee OEmployeeOff = null;
                BMSPaymentReq OEmployeePayrollOff = null;

                int SalaryheadId = Convert.ToInt32(Salheadlist);
                //int ProMonth = Convert.ToInt32(ProcessMonth);
                //int EmpOffId = Convert.ToInt32(EmpOff);
                int EmpOffId;

                if (!string.IsNullOrEmpty(EmpOff))
                {
                    EmpOffId = Convert.ToInt32(EmpOff);
                }
                else
                {
                    //var query1 = db.Employee.Include(e => e.PayStruct).Where(e=>e.PayStruct_Id==GradeId).FirstOrDefault();
                    //EmpOffId = query1.Id;
                    EmpOffId = 0;
                }

                if (type.ToString() == "OFFICIATING ALLOWANCE")
                {
                    var query = db.EmployeePolicyStruct.AsNoTracking().Where(e => e.EmployeePayroll.Employee.Id == Emp && e.EndDate == null).Include(e => e.EmployeePayroll).Include(e => e.EmployeePayroll.Employee).Include(e => e.EmployeePolicyStructDetails)
                                    .Include(e => e.EmployeePolicyStructDetails.Select(r => r.PolicyFormula)).Include(e => e.EmployeePolicyStructDetails.
                                    Select(r => r.PolicyFormula.OfficiatingParameter)).FirstOrDefault();
                    var TransActList = query.EmployeePolicyStructDetails.Select(r => r.PolicyFormula.OfficiatingParameter);

                    List<OfficiatingParameter> TransAct = new List<OfficiatingParameter>();
                    if (TransActList.Count() == 0)
                    {
                        Msg.Add("You Are not Applicable for Officiating");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }
                }
                int EmpIdCheck = Convert.ToInt32(Emp);
                if (EmpIdCheck != null && EmpIdCheck != 0)
                {

                    List<string> MsgCheck = new List<string>();

                    OEmployeeOff = db.Employee.Include(q => q.EmpName).Where(r => r.Id == EmpIdCheck).AsNoTracking().AsParallel().SingleOrDefault();
                    //OEmployeePayrollOff = db.EmployeePayroll.Include(e => e.OfficiatingServiceBook).Where(e => e.Employee.Id == OEmployeeOff.Id).AsNoTracking().AsParallel().SingleOrDefault();
                    OEmployeePayrollOff = db.BMSPaymentReq.Include(e => e.EmployeePayroll).Where(e => e.EmployeePayroll.Employee.Id == OEmployeeOff.Id).AsNoTracking().AsParallel().FirstOrDefault();

                    DateTime mFromPeriod = Convert.ToDateTime(BMSPaymentReq.FromPeriod);
                    DateTime mEndDate = Convert.ToDateTime(BMSPaymentReq.ToPeriod);
                    if (type.ToString() == "OFFICIATING ALLOWANCE")
                    {
                        if (mFromPeriod.Year != mEndDate.Year || mFromPeriod.Month != mEndDate.Month)
                        {
                            Msg.Add("Please select the FromDate and ToDate of same month ");
                            return Json(new { status = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }

                    var emp = db.Employee.Where(e => e.Id == Emp).FirstOrDefault();

                    for (DateTime mTempDate = mFromPeriod; mTempDate <= mEndDate; mTempDate = mTempDate.AddDays(1))
                    {

                        int Officiatingdates = db.BMSPaymentReq.Where(e => e.EmployeePayroll.Employee.Id == Emp && (e.FromPeriod.Value <= mTempDate.Date && e.ToPeriod.Value >= mTempDate.Date) && e.IsCancel == false && e.TrReject == false && e.SalaryHead_Id == SalaryheadId).Select(e => e.Id).FirstOrDefault();


                        if (Officiatingdates > 0)
                        {
                            Msg.Add("You have already enter " + mTempDate.Date + " for this employee " + emp.EmpCode);
                            return Json(new { status = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }

                    }

                }
                else
                {
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }

                Employee OEmployeec = null;
                EmployeePayroll OEmployeePayrollc = null;
                string EmpRel = null;
                string EmpPro = null;
                string EmpCPI = null;
                //string PayMonth = OfficiatingServiceBook.PayMonth;
                if (EmpOffId > 0)
                {
                    OEmployeec = db.Employee.Where(r => r.Id == EmpOffId).SingleOrDefault();


                    OEmployeePayrollc = db.EmployeePayroll.Where(e => e.Employee.Id == OEmployeec.Id).SingleOrDefault();

                    var OEmpSalProcess = db.EmployeePayroll.Where(e => e.Id == OEmployeePayrollc.Id).Include(e => e.SalaryT).SingleOrDefault();
                    var EmpSalRelTPro = OEmpSalProcess.SalaryT != null ? OEmpSalProcess.SalaryT.Where(e => e.ReleaseDate == null) : null;

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
                    var EmpCPIPro = db.CPIEntryT.Where(e => e.EmployeePayroll_Id == OEmployeePayrollc.Id).Select(e => e.Id).FirstOrDefault();


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
                    var EmpSalRelT = OEmpSalRelT.SalaryT != null ? OEmpSalRelT.SalaryT.Where(e => e.ReleaseDate != null) : null;

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



                //if (type.ToString() == "OFFICIATING ALLOWANCE")
                //{
                //    //if (OffActivity == null || OffActivity == "")
                //    //{
                //    //    Msg.Add(" Kindly select Officiating Parameter  ");
                //    //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                //    //}

                //    int TransActivityId = 0;

                //    if (OffActivity != null && OffActivity != "")
                //    {
                //        TransActivityId = int.Parse(OffActivity);

                //        BMSPaymentReq.OfficiatingParameter = db.OfficiatingParameter.Where(e => e.Id == TransActivityId).SingleOrDefault();
                //    }
                //    // NKGSB Start
                //    var cid = Convert.ToInt32(SessionManager.CompanyId);
                //    var offpara = db.OfficiatingParameter.Where(e => e.Id == TransActivityId).SingleOrDefault();
                //    if (offpara.NewPayStructOnScreenAppl == false)
                //    {
                //        int EmpIdw = Convert.ToInt32(Emp);
                //        Employee employeew = null;
                //        OEmployeeOff = db.Employee.Include(q => q.EmpName).Where(r => r.Id == EmpOffId).SingleOrDefault();
                //        employeew = db.Employee.Include(q => q.EmpName).Where(r => r.Id == EmpIdw).SingleOrDefault();
                //        if (offpara.GradeShiftCount != 0)
                //        {
                //            if (employeew.PayStruct_Id > OEmployeeOff.PayStruct_Id)
                //            {

                //                var pay_dataoff = db.PayStruct.Where(e => e.Company.Id == cid && e.JobStatus_Id != null && e.Id == OEmployeeOff.PayStruct_Id)
                //                       .Select(e => new
                //                       {
                //                           code = e.Id.ToString(),
                //                           GCode = e.Grade.Code,
                //                           GName = e.Grade.Name.ToString(),
                //                           Levelname = e.Level.Name.ToString(),
                //                           EmpActingStatus = e.JobStatus.EmpActingStatus.LookupVal,
                //                           EmpStatus = e.JobStatus.EmpStatus.LookupVal


                //                       }).SingleOrDefault();

                //                var pay_datalist = db.PayStruct.Include(e => e.Level).Include(e => e.JobStatus).Include(e => e.JobStatus.EmpActingStatus).Include(e => e.JobStatus.EmpStatus).Where(e => e.Company.Id == cid && e.JobStatus_Id != null && e.Id < employeew.PayStruct_Id).OrderByDescending(e => e.Id).ToList();
                //                if (pay_dataoff.Levelname == "")
                //                {
                //                    var pay_data = pay_datalist.Where(e => e.JobStatus.EmpActingStatus.LookupVal == pay_dataoff.EmpActingStatus && e.JobStatus.EmpStatus.LookupVal == pay_dataoff.EmpStatus).OrderByDescending(e => e.Id).Skip(offpara.GradeShiftCount - 1).Take(1).SingleOrDefault();
                //                    if (pay_data == null)
                //                    {
                //                        Msg.Add("Auto shift Grade not avaialble in paystructure");
                //                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                //                    }
                //                    BMSPaymentReq.OfficiatingPayStruct = pay_data.Id;
                //                }
                //                else
                //                {
                //                    var pay_data = pay_datalist.Where(e => e.Level.Name == pay_dataoff.Levelname && e.JobStatus.EmpActingStatus.LookupVal == pay_dataoff.EmpActingStatus && e.JobStatus.EmpStatus.LookupVal == pay_dataoff.EmpStatus).OrderByDescending(e => e.Id).Skip(offpara.GradeShiftCount - 1).Take(1).SingleOrDefault();
                //                    if (pay_data == null)
                //                    {
                //                        Msg.Add("Auto shift Grade not avaialble in paystructure");
                //                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                //                    }
                //                    BMSPaymentReq.OfficiatingPayStruct = pay_data.Id;
                //                }



                //            }
                //            else
                //            {
                //                var pay_dataoff = db.PayStruct.Where(e => e.Company.Id == cid && e.JobStatus_Id != null && e.Id == OEmployeeOff.PayStruct_Id)
                //                                                      .Select(e => new
                //                                                      {
                //                                                          code = e.Id.ToString(),
                //                                                          GCode = e.Grade.Code,
                //                                                          GName = e.Grade.Name.ToString(),
                //                                                          Levelname = e.Level.Name.ToString(),
                //                                                          EmpActingStatus = e.JobStatus.EmpActingStatus.LookupVal,
                //                                                          EmpStatus = e.JobStatus.EmpStatus.LookupVal


                //                                                      }).SingleOrDefault();


                //                var pay_datalist = db.PayStruct.Include(e => e.Level).Include(e => e.JobStatus).Include(e => e.JobStatus.EmpActingStatus).Include(e => e.JobStatus.EmpStatus).Where(e => e.Company.Id == cid && e.JobStatus_Id != null && e.Id < employeew.PayStruct_Id).OrderBy(e => e.Id).ToList();
                //                if (pay_dataoff.Levelname == "")
                //                {
                //                    var pay_data = pay_datalist.Where(e => e.JobStatus.EmpActingStatus.LookupVal == pay_dataoff.EmpActingStatus && e.JobStatus.EmpStatus.LookupVal == pay_dataoff.EmpStatus).OrderBy(e => e.Id).Skip(offpara.GradeShiftCount - 1).Take(1).SingleOrDefault();
                //                    if (pay_data == null)
                //                    {
                //                        Msg.Add("Auto shift Grade not avaialble in paystructure");
                //                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                //                    }
                //                    BMSPaymentReq.OfficiatingPayStruct = pay_data.Id;
                //                }
                //                else
                //                {
                //                    var pay_data = pay_datalist.Where(e => e.Level.Name == pay_dataoff.Levelname && e.JobStatus.EmpActingStatus.LookupVal == pay_dataoff.EmpActingStatus && e.JobStatus.EmpStatus.LookupVal == pay_dataoff.EmpStatus).OrderBy(e => e.Id).Skip(offpara.GradeShiftCount - 1).Take(1).SingleOrDefault();
                //                    if (pay_data == null)
                //                    {
                //                        Msg.Add("Auto shift Grade not avaialble in paystructure");
                //                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                //                    }
                //                    BMSPaymentReq.OfficiatingPayStruct = pay_data.Id;
                //                }


                //            }
                //        }


                //    }
                //    if (NewPayStruct == null && offpara.NewPayStructOnScreenAppl == true)
                //    {
                //        Msg.Add("Please select PayStruct");
                //        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                //    }
                //    if (NewPayStruct != "" && NewPayStruct != null)
                //    {
                //        var payid = db.PayStruct.Find(int.Parse(NewPayStruct));
                //        BMSPaymentReq.OfficiatingPayStruct = payid.Id;
                //    }
                //    // NKGSB End
                //}


                // NKGSB End

                try
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        var db_data = db.BMSPaymentReq
                            .Where(e => e.Id == data).SingleOrDefault();

                        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                        {

                            db_data.Id = data;
                            db_data.FromPeriod = BMSPaymentReq.FromPeriod;
                            db_data.ToPeriod = BMSPaymentReq.ToPeriod;
                            db_data.PayMonth = BMSPaymentReq.PayMonth == null ? "" : BMSPaymentReq.PayMonth;
                            db_data.OfficiatingPayStruct = Convert.ToInt32(NewPayStruct) == null ? 0 : Convert.ToInt32(NewPayStruct);
                            db_data.Narration = BMSPaymentReq.Narration;
                            db_data.DBTrack = new DBTrack
                            {
                                CreatedBy = db_data.DBTrack.CreatedBy == null ? null : db_data.DBTrack.CreatedBy,
                                CreatedOn = db_data.DBTrack.CreatedOn == null ? null : db_data.DBTrack.CreatedOn,
                                Action = "M",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };

                            db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                        }
                        ts.Complete();
                    }
                    Msg.Add("Record Updated");
                    return Json(new Utility.JsonClass { status = true, responseText = "Record Updated..!" }, JsonRequestBehavior.AllowGet);
                    //return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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

            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        }

        public class salheadlist
        {
            public string Id { get; set; }
            public string Name { get; set; }
        };

        public ActionResult PopulateDropDownList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //var qurey = db.SalaryHead.Where(e => e.Frequency.LookupVal.ToUpper() == "YEARLY" && e.SalHeadOperationType.LookupVal.ToUpper() != "PERK").ToList();
                var qurey = db.SalaryHead.Include(e => e.SalHeadOperationType).ToList();
                List<string> salaryheadcode = new List<string>() { "OFFICIATING", "LTA", "MEDALLOW" };
               // List<string> salaryheadcode = new List<string>() { "LTA", "MEDALLOW" };

                List<salheadlist> returndata = new List<salheadlist>();

                foreach (var item in qurey)
                {
                    foreach (var item1 in salaryheadcode)
                    {
                        if (item1.ToUpper() == item.SalHeadOperationType.LookupVal.ToUpper())
                        {
                            returndata.Add(new salheadlist
                             {
                                 Id = item.Id.ToString(),
                                 Name = item.Name,

                             });
                        }
                    }

                }

                var selected = (Object)null;
                if (data2 != "" && data != "0" && data2 != "0")
                {
                    selected = Convert.ToInt32(data2);
                }

                SelectList s = new SelectList(returndata, "Id", "Name", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult Officiatingeprocess(string data, string data2, string data3, string data4, string data5, int data6,string idata)
        {
            List<string> Msg = new List<string>();

            using (DataBaseContext db = new DataBaseContext())
            {
                string iProcessMonth = idata;
                //var Emp_Id = Convert.ToInt32(TempData["EMPId"]);
                var Emp_Id = Convert.ToInt32(data4);
                EmployeePayroll OEmpPayroll = db.EmployeePayroll.Where(e => e.Id == Emp_Id).FirstOrDefault();
                string PayMonth = data;           
                //if (PayMonth == null || PayMonth == "")
                //{
                //    Msg.Add("Please Select the PayMonth");
                //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                //}16/112024
                double CalAmount = 0.0;

                //var offid = Convert.ToInt32(TempData["officitingId"]);
                var offid = Convert.ToInt32(data6);


                var salpro = db.SalaryT.Include(e => e.EmployeePayroll).Include(e => e.EmployeePayroll.Employee).Where(e => e.EmployeePayroll.Id == OEmpPayroll.Id && e.PayMonth == PayMonth).FirstOrDefault();//16/11/2024

                if (salpro != null)
                {
                    Msg.Add("Please delete the salary and contact to the Administration ");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
                var SalaryHeadId = Convert.ToInt32(data5);

                var salheadoperation = db.SalaryHead.Include(e => e.SalHeadOperationType).Where(e => e.Id == SalaryHeadId).FirstOrDefault();
                string SalHeadoperationtype = salheadoperation.SalHeadOperationType.LookupVal.ToUpper();

                var OfficiatingParameterlist = data3;

                if (SalHeadoperationtype == "OFFICIATING")
                {
                    if (OfficiatingParameterlist == null || OfficiatingParameterlist == "0")
                    {
                        Msg.Add("Please Select the Officiating Parameter");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    var OffProcess = officiateprocess(OEmpPayroll, PayMonth, CalAmount, SalHeadoperationtype, offid, OfficiatingParameterlist, SalaryHeadId);

                    double offPro = Convert.ToDouble(OffProcess);

                    return Json(offPro, JsonRequestBehavior.AllowGet);
                }

                else if (SalHeadoperationtype != "OFFICIATING")
                {
                    var LtaProcess = Ltaprocess(OEmpPayroll, PayMonth, CalAmount, SalHeadoperationtype, offid, OfficiatingParameterlist, SalaryHeadId, iProcessMonth);

                    double LtaPro = Convert.ToDouble(LtaProcess);

                    return Json(LtaPro, JsonRequestBehavior.AllowGet);
                }

            }
            return View();
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

                //var OEmp_Id = Convert.ToInt32(TempData["EMPId1"]);
                //var recordId = Convert.ToInt32(TempData["officitingId1"]);
                var OEmp_Id = Convert.ToInt32(data1);
                var recordId = Convert.ToInt32(data2);
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


        public ActionResult ltaProData(string data1, string data2)
        {
            List<string> Msg = new List<string>();
            List<ProcessData1> return_Data = new List<ProcessData1>();

            using (DataBaseContext db = new DataBaseContext())
            {

                //var OEmp_Id = Convert.ToInt32(TempData["EMPId1"]);
                //var recordId = Convert.ToInt32(TempData["officitingId1"]);
                var OEmp_Id = Convert.ToInt32(data1);
                var recordId = Convert.ToInt32(data2);
                var offdata = db.BMSPaymentReq
                    .Include(e => e.SalaryHead)
                    .Include(e => e.SalaryHead.Type)
                    .Where(e => e.EmployeePayroll_Id == OEmp_Id && e.Id == recordId).FirstOrDefault();

                var totalEarning = offdata.AmountPaid;
                var totalDeduction = offdata.OtherDeduction;

                var total = totalEarning - totalDeduction;

                return_Data.Add(new ProcessData1
                        {
                            salhead = offdata.SalaryHead.Name,
                            salheadamount = offdata.AmountPaid.ToString(),
                            type = offdata.SalaryHead.Type.LookupVal,
                        });

                return Json(new Object[] { return_Data, totalEarning, totalDeduction, total, JsonRequestBehavior.AllowGet });
            }

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

                string requiredPathLoanNet = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\EssPortal\\App_Data\\Menu_Json";
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

                string requiredPathchkNet = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\EssPortal\\App_Data\\Menu_Json";
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

        public static PFECRR PFcalcArr(PFMaster OCompanyPFMaster, EmpSalStruct OEmpSalstruct, int OEmployeePayrollId, Calendar cal, string PayMonth, List<SalEarnDedT> OSalaryDetails, string UANNo)
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


        public static double officiateprocess(EmployeePayroll OEmpPayroll, string PayMonth, double CalAmount, string SalHeadoperationtype, int offid, string OfficiatingParameterlist, int SalaryHeadId)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {

                    using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                       new System.TimeSpan(0, 30, 0)))
                    {

                        if (SalHeadoperationtype == "OFFICIATING")
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

                            // var OEmpOff = db.EmployeePayroll.Where(r => r.Id == OEmpPayroll.Id)
                            var OEmpOff = db.EmployeePayroll.Where(r => r.Id == OEmpPayroll.Id)
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

                            var OEmpSalT = db.BMSPaymentReq.Include(e => e.EmployeePayroll).Include(e => e.OfficiatingPaymentT).Include(e => e.EmployeePayroll.Employee).Where(e => e.Id == offid && e.EmployeePayroll_Id == OEmpPayroll.Id).ToList();
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
                            var offservicebook = db.BMSPaymentReq.Include(e => e.EmployeePayroll).Include(e => e.EmployeePayroll.Employee).Where(e => e.Id == offid && e.EmployeePayroll_Id == OEmpPayroll.Id).ToList();

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


                                            EmpSalStructTotaloff = db.EmpSalStruct.Where(e => e.EmployeePayroll.Id == oEmployeePayrolloff && e.EffectiveDate >= comparedateoff && e.EffectiveDate <= comparedateoffend).ToList();
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
                                                var OEmpsalheadoff = OMultiEmpStruct.EmpSalStructDetails.Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == SalHeadoperationtype).FirstOrDefault();
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
                                                   // SalAmount = OSalStructDetails.Amount;
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
                                                var OEmpsalheadoff = OMultiEmpStruct.EmpSalStructDetails.Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == SalHeadoperationtype).FirstOrDefault();
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
                                                    //SalAmount = OSalStructDetails.Amount;
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
                                                var OEmpsalheadoff = OMultiEmpStruct.EmpSalStructDetails.Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == SalHeadoperationtype).FirstOrDefault();
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
                                                var OEmpsalheadoff = OMultiEmpStruct.EmpSalStructDetails.Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == SalHeadoperationtype).FirstOrDefault();
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
                                                    //SalAmount = OSalStructDetails.Amount;
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
                                                        OPFTransTcur = PFcalcArr(OCompPFMaster, OEmpSalStruct, OEmpPayroll.Id, null, payMonthDate, OSalEarnDedToffcur_Final, OEmpOff.UANNo);
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


                                                        OPFTransToff = PFcalcArr(OCompPFMaster, OEmpSalStruct, OEmpPayroll.Id, null, payMonthDate, OSalEarnDedToff_Final, OEmpOff.UANNo);
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
                                    var offservicebookcheck = db.BMSPaymentReq.Include(e => e.EmployeePayroll).Include(e => e.EmployeePayroll.Employee).Include(e => e.OfficiatingParameter).Where(e => e.EmployeePayroll_Id == OEmpPayroll.Id).FirstOrDefault();
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

                                        var oBMSPaymentReq = db.BMSPaymentReq.Include(e => e.EmployeePayroll).Include(e => e.EmployeePayroll.Employee).Where(e => e.Id == offid && e.EmployeePayroll_Id == OEmpPayroll.Id).FirstOrDefault();

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

                                        var oBMSPaymentReq = db.BMSPaymentReq.Include(e => e.EmployeePayroll).Include(e => e.EmployeePayroll.Employee).Where(e => e.Id == offid && e.EmployeePayroll_Id == OEmpPayroll.Id).FirstOrDefault();


                                        if (OFinalOfficiatingPayementT.Count() > 0)
                                        {
                                            var Fulldate = PayMonth;
                                            var PayMonthdate = Convert.ToDateTime(Fulldate);
                                            var payMonthDate = PayMonthdate.ToString("MM/yyyy");

                                            db.OfficiatingPaymentT.AddRange(OFinalOfficiatingPayementT);
                                            db.SaveChanges();
                                            if (OFinalOOfficiatingPFT.DBTrack==null)
                                            {
                                                OFinalOOfficiatingPFT.DBTrack= new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };

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
                        }//officiating close
                        ts.Complete();
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

        public static double Ltaprocess(EmployeePayroll OEmpPayroll, string PayMonth, double CalAmount, string SalHeadoperationtype, int offid, string OfficiatingParameterlist, int SalaryHeadId, string iiProcessMonth)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {

                    using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, new System.TimeSpan(0, 30, 0)))
                    {
                        Double[] BonusExGratiaAmt = new Double[5];
                        double BonusAmt = 0;
                        double ExGratiaAmt = 0;
                        int Salheadid = Convert.ToInt32(SalaryHeadId);
                        var SalHead = db.SalaryHead.Include(e => e.SalHeadOperationType).Include(e => e.ProcessType).Where(e => e.Id == Salheadid).FirstOrDefault();

                        List<BMSPaymentReq> OFAT = new List<BMSPaymentReq>();

                        //  BMSPaymentReq objofBmspaymentreq = new BMSPaymentReq();

                        BMSPaymentReq ltadata1 = db.BMSPaymentReq.Include(e => e.EmployeePayroll).Include(e => e.EmployeePayroll.Employee).Where(e => e.Id == offid && e.EmployeePayroll_Id == OEmpPayroll.Id).SingleOrDefault();


                        //DateTime processMonthDate = DateTime.ParseExact(ltadata1.ProcessMonth, "MM/yyyy", null);16/11/2024
                        DateTime processMonthDate = Convert.ToDateTime(iiProcessMonth);
                        // DateTime payMonthDate = DateTime.ParseExact(PayMonth, "MM/yyyy", null);
                        //var Fulldate = PayMonth;16/11/2024
                        //var PayMonthdate = Convert.ToDateTime(Fulldate);16/11/2024
                        //var PayDate = PayMonthdate.ToString("dd/MM/yyyy");
                        //var PayMondate = Convert.ToDateTime(Fulldate);
                        //var payMonthDate = PayMonthdate.ToString("MM/yyyy");16/11/2024
                        DateTime PayMondate = Convert.ToDateTime(PayMonth);
                        var payMonthDate = PayMondate.ToString("MM/yyyy");
                        if (processMonthDate > PayMondate)
                        {
                            iiProcessMonth = PayMondate.ToString("MM/yyyy");
                        }

                        // ltadata1 = db.BMSPaymentReq.Include(e => e.EmployeePayroll).Include(e => e.EmployeePayroll.Employee).Where(e => e.Id == OEmpPayroll.Id).SingleOrDefault();
                        ltadata1.DBTrack = new DBTrack
                        {
                            CreatedBy = ltadata1.DBTrack.CreatedBy == null ? null : ltadata1.DBTrack.CreatedBy,
                            CreatedOn = ltadata1.DBTrack.CreatedOn == null ? null : ltadata1.DBTrack.CreatedOn,
                            Action = "M",
                            ModifiedBy = SessionManager.UserName,
                            ModifiedOn = DateTime.Now
                        };

                        if (SalHead.SalHeadOperationType.LookupVal.ToUpper() != "BONUS" && SalHead.ProcessType.LookupVal.ToUpper() == "STANDARD")
                        {

                            Double Fixamt = 0;

                            var OEmpSalstructH = db.EmpSalStruct
                                                   .Include(e => e.EmpSalStructDetails)
                                                   .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead))
                                                   .Where(e => e.EmployeePayroll.Id == OEmpPayroll.Id)
                                                   .ToList();

                            var OEmpSalStruct = OEmpSalstructH.OrderByDescending(e => e.EffectiveDate).FirstOrDefault();


                            if (OEmpSalStruct != null)
                            {
                                var OEmpSalDetails = OEmpSalStruct.EmpSalStructDetails.Where(e => e.SalaryHead.Id == SalHead.Id).SingleOrDefault();

                                if (OEmpSalDetails != null)
                                {
                                    Fixamt = OEmpSalDetails.Amount;
                                }
                            }
                            // Attendance check start
                            if (Fixamt != 0)
                            {
                                if (SalHead.OnAttend == true)
                                {

                                    String mPeriodRange = "";
                                    DateTime fromdt = Convert.ToDateTime(ltadata1.FromPeriod);
                                    DateTime EndDt = Convert.ToDateTime(ltadata1.ToPeriod);

                                    List<string> mPeriod = new List<string>();
                                    for (DateTime mTempDate = fromdt; mTempDate <= EndDt; mTempDate = mTempDate.AddMonths(1))
                                    {
                                        if (mPeriodRange == "")
                                        {
                                            mPeriodRange = Convert.ToDateTime(mTempDate).ToString("MM/yyyy");
                                            mPeriod.Add(Convert.ToDateTime(mTempDate).ToString("MM/yyyy"));
                                        }
                                        else
                                        {
                                            mPeriodRange = mPeriodRange + "," + Convert.ToDateTime(mTempDate).ToString("MM/yyyy");
                                            mPeriod.Add(Convert.ToDateTime(mTempDate).ToString("MM/yyyy"));
                                        }
                                    }
                                    double presentdays = 0;
                                    double monthdays = 0;
                                    foreach (var item1 in mPeriod)
                                    {
                                        var prdays = db.SalAttendanceT.Where(e => e.PayMonth == item1 && e.EmployeePayroll.Id == OEmpPayroll.Id).FirstOrDefault();
                                        if (prdays != null)
                                        {
                                            presentdays = presentdays + Convert.ToDouble(prdays.PaybleDays) + Convert.ToDouble(prdays.ArrearDays);
                                        }

                                        var mdays = db.SalAttendanceT.Where(e => e.PayMonth == item1 && e.EmployeePayroll.Id == OEmpPayroll.Id).FirstOrDefault();
                                        if (mdays != null)
                                        {
                                            monthdays = monthdays + Convert.ToDouble(mdays.MonthDays);
                                        }

                                    }

                                    Fixamt = Math.Round(((presentdays) * Fixamt / monthdays), 0);

                                    // Attendance Check end                                

                                }
                            }
                            ltadata1.AmountPaid = Fixamt;
                            ltadata1.PayMonth = payMonthDate;
                            //ltadata1.ProcessMonth = ltadata1.ProcessMonth;16/11/2024               
                            ltadata1.ProcessMonth = iiProcessMonth;
                            db.BMSPaymentReq.Attach(ltadata1);
                            db.Entry(ltadata1).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            db.Entry(ltadata1).State = System.Data.Entity.EntityState.Detached;


                        }
                        else if (SalHead.SalHeadOperationType.LookupVal.ToUpper() != "BONUS" && SalHead.ProcessType.LookupVal.ToUpper() == "FIXEDMONTH")
                        {

                            Double Fixamt1 = 0;
                            //DateTime? mEffectiveDate = Convert.ToDateTime(ltadata1.ProcessMonth);16/11/2024
                            DateTime? mEffectiveDate = Convert.ToDateTime(iiProcessMonth);
                            DateTime enddate = Convert.ToDateTime(mEffectiveDate).AddMonths(1).Date.AddDays(-1).Date;

                            var OEmpSalstructH1 = db.EmpSalStruct
                                                   .Include(e => e.EmpSalStructDetails)
                                                   .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead))
                                                   .Where(e => e.EmployeePayroll.Id == OEmpPayroll.Id && e.EffectiveDate >= mEffectiveDate && e.EffectiveDate <= enddate);


                            var OEmpSalStruct1 = OEmpSalstructH1.OrderByDescending(e => e.EffectiveDate).FirstOrDefault();
                            if (OEmpSalStruct1 != null)
                            {
                                var OEmpSalDetails = OEmpSalStruct1.EmpSalStructDetails.Where(e => e.SalaryHead.Id == SalHead.Id).SingleOrDefault();

                                if (OEmpSalDetails != null)
                                {
                                    Fixamt1 = OEmpSalDetails.Amount;
                                }
                            }
                            // Attendance check start
                            if (Fixamt1 != 0)
                            {
                                if (SalHead.OnAttend == true)
                                {
                                    String mPeriodRange = "";
                                    DateTime fromdt = Convert.ToDateTime(ltadata1.FromPeriod);
                                    DateTime EndDt = Convert.ToDateTime(ltadata1.ToPeriod);

                                    List<string> mPeriod = new List<string>();
                                    for (DateTime mTempDate = fromdt; mTempDate <= EndDt; mTempDate = mTempDate.AddMonths(1))
                                    {
                                        if (mPeriodRange == "")
                                        {
                                            mPeriodRange = Convert.ToDateTime(mTempDate).ToString("MM/yyyy");
                                            mPeriod.Add(Convert.ToDateTime(mTempDate).ToString("MM/yyyy"));
                                        }
                                        else
                                        {
                                            mPeriodRange = mPeriodRange + "," + Convert.ToDateTime(mTempDate).ToString("MM/yyyy");
                                            mPeriod.Add(Convert.ToDateTime(mTempDate).ToString("MM/yyyy"));
                                        }
                                    }
                                    double presentdays = 0;
                                    double monthdays = 0;
                                    foreach (var item in mPeriod)
                                    {
                                        var prdays = db.SalAttendanceT.Where(e => e.PayMonth == item && e.EmployeePayroll.Id == OEmpPayroll.Id).FirstOrDefault();
                                        if (prdays != null)
                                        {
                                            presentdays = presentdays + Convert.ToDouble(prdays.PaybleDays) + Convert.ToDouble(prdays.ArrearDays);
                                        }

                                        var mdays = db.SalAttendanceT.Where(e => e.PayMonth == item && e.EmployeePayroll.Id == OEmpPayroll.Id).FirstOrDefault();
                                        if (mdays != null)
                                        {
                                            monthdays = monthdays + Convert.ToDouble(mdays.MonthDays);
                                        }

                                    }
                                    Fixamt1 = Math.Round(((presentdays) * Fixamt1 / monthdays), 0);
                                }


                            }

                            // Attendance Check end
                            ltadata1.AmountPaid = Fixamt1;
                            //ltadata1.ProcessMonth = ltadata1.ProcessMonth;16/11/2024
                            ltadata1.PayMonth = payMonthDate;                   
                            ltadata1.ProcessMonth = iiProcessMonth;                 
                            db.BMSPaymentReq.Attach(ltadata1);
                            db.Entry(ltadata1).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            db.Entry(ltadata1).State = System.Data.Entity.EntityState.Detached;


                        }

                        else if (SalHead.SalHeadOperationType.LookupVal.ToUpper() != "BONUS" && SalHead.ProcessType.LookupVal.ToUpper() == "EARNED")
                        {
                            String mPeriodRange = "";
                            DateTime fromdt = Convert.ToDateTime(ltadata1.FromPeriod);
                            DateTime EndDt = Convert.ToDateTime(ltadata1.ToPeriod);
                            double totmonths = 0;
                            double CalAmount1 = 0;
                            double TotCalAmount = 0;
                            List<string> mPeriod = new List<string>();
                            BMSPaymentReq OEmpPayroll1 = db.BMSPaymentReq.Include(e => e.EmployeePayroll).Include(e => e.EmployeePayroll.Employee).Include(e => e.EmployeePayroll.Employee.ServiceBookDates).Where(e => e.Id == OEmpPayroll.Id).FirstOrDefault();

                            for (DateTime mTempDate = fromdt; mTempDate <= EndDt; mTempDate = mTempDate.AddMonths(1))
                            {
                                if (mPeriodRange == "")
                                {
                                    mPeriodRange = Convert.ToDateTime(mTempDate).ToString("MM/yyyy");
                                    mPeriod.Add(Convert.ToDateTime(mTempDate).ToString("MM/yyyy"));
                                }
                                else
                                {
                                    mPeriodRange = mPeriodRange + "," + Convert.ToDateTime(mTempDate).ToString("MM/yyyy");
                                    mPeriod.Add(Convert.ToDateTime(mTempDate).ToString("MM/yyyy"));
                                }
                            }

                            foreach (var item in mPeriod)
                            {

                                var salearndedt = db.SalaryT.Include(e => e.SalEarnDedT).Include(e => e.SalEarnDedT.Select(r => r.SalaryHead)).Where(e => e.PayMonth == item && e.EmployeePayroll.Id == OEmpPayroll1.Id).AsNoTracking().FirstOrDefault();
                                if (salearndedt != null)
                                {
                                    List<SalEarnDedT> OSalEarnDedT = new List<SalEarnDedT>();
                                    OSalEarnDedT = salearndedt.SalEarnDedT.ToList();



                                    DateTime? mEffectiveDate = Convert.ToDateTime("01/" + item);
                                    DateTime enddate = Convert.ToDateTime(mEffectiveDate).AddMonths(1).Date.AddDays(-1).Date;

                                    var OEmpSalstructH1 = db.EmpSalStruct
                                    .Include(e => e.EmpSalStructDetails)
                                    .Include(e => e.EmpSalStructDetails.Select(x => x.SalHeadFormula))
                                   .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead))
                                    .Where(e => e.EmployeePayroll.Id == OEmpPayroll1.Id && e.EffectiveDate >= mEffectiveDate && e.EffectiveDate <= enddate);


                                    var OEmpSalStruct1 = OEmpSalstructH1.OrderByDescending(e => e.EffectiveDate).FirstOrDefault();
                                    if (OEmpSalStruct1 != null)
                                    {
                                        var OEmpSalDetails = OEmpSalStruct1.EmpSalStructDetails.Where(e => e.SalaryHead.Id == SalHead.Id).SingleOrDefault();

                                        if (OEmpSalDetails != null && OEmpSalDetails.SalHeadFormula_Id != null)
                                        {
                                            totmonths = totmonths + 1;
                                            CalAmount = Process.SalaryHeadGenProcess.SalHeadAmountCalc(OEmpSalDetails.SalHeadFormula.Id, OSalEarnDedT, null, OEmpPayroll, item);
                                            CalAmount = Process.SalaryHeadGenProcess.RoundingFunction(OEmpSalDetails.SalaryHead, CalAmount); //rounding function
                                            TotCalAmount = TotCalAmount + CalAmount;

                                        }
                                    }


                                }

                            }
                            TotCalAmount = Math.Round((TotCalAmount / totmonths), 0);

                            ltadata1.AmountPaid = TotCalAmount;
                            //ltadata1.ProcessMonth = ltadata1.ProcessMonth;16/11/2024
                            ltadata1.PayMonth = payMonthDate;                    
                            ltadata1.ProcessMonth = iiProcessMonth;                        
                            db.BMSPaymentReq.Attach(ltadata1);
                            db.Entry(ltadata1).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            db.Entry(ltadata1).State = System.Data.Entity.EntityState.Detached;


                        }

                        else//ProRata Base
                        {
                            DateTime fromdty = Convert.ToDateTime(ltadata1.FromPeriod);
                            DateTime EndDty = Convert.ToDateTime(ltadata1.ToPeriod);
                            int YearDaysdiff = (EndDty - fromdty).Days + 1;
                            var Id = Convert.ToInt32(SessionManager.CompanyId);
                            string CompCode = db.Company.Where(e => e.Id == Id).SingleOrDefault().Code.ToUpper();
                            BMSPaymentReq OEmpPayroll1 = db.BMSPaymentReq.Include(e => e.EmployeePayroll).Include(e => e.EmployeePayroll.Employee).Include(e => e.EmployeePayroll.Employee.ServiceBookDates).Where(e => e.Id == OEmpPayroll.Id).FirstOrDefault();

                            if (OEmpPayroll1.EmployeePayroll.Employee.ServiceBookDates.RetirementDate < ltadata1.ToPeriod)
                            {
                                if (CompCode != "ASBL")
                                {
                                    ltadata1.ToPeriod = Convert.ToDateTime(OEmpPayroll1.EmployeePayroll.Employee.ServiceBookDates.RetirementDate);
                                }

                            }
                            DateTime fromdt = Convert.ToDateTime(ltadata1.FromPeriod);
                            DateTime EndDt = Convert.ToDateTime(ltadata1.ToPeriod);

                            var OEmpSalstructH = db.EmpSalStruct
                              .Include(e => e.EmpSalStructDetails)
                            .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead))
                            .Include(e => e.EmpSalStructDetails.Select(q => q.SalHeadFormula))
                            .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.Frequency))
                            .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.Type))
                            .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.RoundingMethod))
                            .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.SalHeadOperationType))
                              .Where(e => e.EmployeePayroll.Id == OEmpPayroll1.Id);


                            var OEmpSalStruct = OEmpSalstructH.Where(e => e.EffectiveDate >= fromdt && e.EffectiveDate <= EndDt).OrderByDescending(e => e.EffectiveDate).ToList();


                            double Amount = 0;
                            DateTime mChkDate = EndDt;
                            Boolean checkcell = false;
                            Double cellamt = 0;

                            if (OEmpSalStruct != null && OEmpSalStruct.Count() > 0)
                            {

                                int runday = 0;
                                int totaldays = 0;
                                double prevamount = 0;

                                foreach (var item in OEmpSalStruct)
                                {
                                    string PayMonth1 = mChkDate.Month + "/" + mChkDate.Year;

                                    var OEmpSalDetails = item.EmpSalStructDetails.Where(e => e.SalaryHead.Id == SalHead.Id).SingleOrDefault();

                                    if (OEmpSalDetails != null)
                                    {

                                        if (checkcell == false)
                                        {
                                            cellamt = OEmpSalDetails.Amount;
                                        }
                                        if (OEmpSalDetails.EmpSalStruct.EffectiveDate <= fromdt)
                                        {
                                            if ((fromdt.Date).Day == 1 && (mChkDate.Date).Day == DateTime.DaysInMonth(Convert.ToInt32(payMonthDate.Split('/')[1]), Convert.ToInt32(payMonthDate.Split('/')[0])))
                                            {
                                                // int monthsdiff = (12 * (mChkDate.Year - fromdt.Year) + mChkDate.Month - fromdt.Month) + 1;
                                                int Daysdiff = (mChkDate.Date - fromdt.Date).Days + 1;
                                                if (prevamount != OEmpSalDetails.Amount)
                                                {
                                                    Amount = Amount + Math.Round(((totaldays) * prevamount / YearDaysdiff), 2);
                                                    totaldays = 0;
                                                }
                                                // Amount = Amount + Math.Round(((monthsdiff) * OEmpSalDetails.Amount / 12), 2);

                                                prevamount = OEmpSalDetails.Amount;
                                                totaldays = totaldays + Daysdiff;

                                            }
                                            else
                                            {
                                                int Daysdiff = Math.Abs(fromdt.Day - mChkDate.Day) + 1;
                                                if (prevamount != OEmpSalDetails.Amount)
                                                {
                                                    Amount = Amount + Math.Round(((totaldays) * prevamount / YearDaysdiff), 2);
                                                    totaldays = 0;
                                                }
                                                //Amount = Amount + Math.Round(((Daysdiff) * OEmpSalDetails.Amount / YearDaysdiff), 2);

                                                prevamount = OEmpSalDetails.Amount;
                                                totaldays = totaldays + Daysdiff;
                                            }
                                        }
                                        else
                                        {
                                            if ((OEmpSalDetails.EmpSalStruct.EffectiveDate).Value.Day == 1 && (mChkDate.Date).Day == DateTime.DaysInMonth(Convert.ToInt32(payMonthDate.Split('/')[1]), Convert.ToInt32(payMonthDate.Split('/')[0])))
                                            {
                                                // int monthsdiff = (12 * (mChkDate.Year - OEmpSalDetails.EmpSalStruct.EffectiveDate.Value.Year) + mChkDate.Month - OEmpSalDetails.EmpSalStruct.EffectiveDate.Value.Month) + 1;
                                                int Daysdiff = (mChkDate.Date - OEmpSalDetails.EmpSalStruct.EffectiveDate.Value.Date).Days + 1;
                                                // Amount = Amount + Math.Round(((monthsdiff) * OEmpSalDetails.Amount / 12), 2);
                                                if (prevamount != OEmpSalDetails.Amount)
                                                {
                                                    Amount = Amount + Math.Round(((totaldays) * prevamount / YearDaysdiff), 2);
                                                    totaldays = 0;
                                                }


                                                prevamount = OEmpSalDetails.Amount;
                                                totaldays = totaldays + Daysdiff;
                                                mChkDate = (OEmpSalDetails.EmpSalStruct.EffectiveDate.Value.Date).AddDays(-1);


                                            }
                                            else
                                            {
                                                int Daysdiff = Math.Abs(OEmpSalDetails.EmpSalStruct.EffectiveDate.Value.Day - mChkDate.Day) + 1;
                                                if (prevamount != OEmpSalDetails.Amount)
                                                {
                                                    Amount = Amount + Math.Round(((totaldays) * prevamount / YearDaysdiff), 2);
                                                    totaldays = 0;
                                                }

                                                // Amount = Amount + Math.Round(((Daysdiff) * OEmpSalDetails.Amount / YearDaysdiff), 2);

                                                prevamount = OEmpSalDetails.Amount;
                                                totaldays = totaldays + Daysdiff; ;

                                                mChkDate = (OEmpSalDetails.EmpSalStruct.EffectiveDate.Value.Date).AddDays(-1);


                                            }

                                        }

                                    }
                                    checkcell = true;
                                }

                                Amount = Amount + Math.Round(((totaldays) * prevamount / YearDaysdiff), 2);
                                totaldays = 0;
                                Amount = Math.Round(Amount + 0.001, 0);

                            }
                            if (Amount != 0)
                            {
                                if (SalHead.OnAttend == true)
                                {
                                    String mPeriodRange = "";
                                    List<string> mPeriod = new List<string>();
                                    for (DateTime mTempDate = fromdt; mTempDate <= EndDt; mTempDate = mTempDate.AddMonths(1))
                                    {
                                        if (mPeriodRange == "")
                                        {
                                            mPeriodRange = Convert.ToDateTime(mTempDate).ToString("MM/yyyy");
                                            mPeriod.Add(Convert.ToDateTime(mTempDate).ToString("MM/yyyy"));
                                        }
                                        else
                                        {
                                            mPeriodRange = mPeriodRange + "," + Convert.ToDateTime(mTempDate).ToString("MM/yyyy");
                                            mPeriod.Add(Convert.ToDateTime(mTempDate).ToString("MM/yyyy"));
                                        }
                                    }
                                    double presentdays = 0;
                                    double monthdays = 0;
                                    foreach (var item in mPeriod)
                                    {
                                        var prdays = db.SalAttendanceT.Where(e => e.PayMonth == item && e.EmployeePayroll.Id == OEmpPayroll1.Id).FirstOrDefault();
                                        if (prdays != null)
                                        {
                                            presentdays = presentdays + Convert.ToDouble(prdays.PaybleDays) + Convert.ToDouble(prdays.ArrearDays);
                                        }

                                        var mdays = db.SalAttendanceT.Where(e => e.PayMonth == item && e.EmployeePayroll.Id == OEmpPayroll1.Id).FirstOrDefault();
                                        if (mdays != null)
                                        {
                                            monthdays = monthdays + Convert.ToDouble(mdays.MonthDays);
                                        }

                                    }
                                    Amount = Math.Round(((presentdays) * Amount / monthdays), 0);
                                }
                                if (Amount > cellamt)
                                {
                                    Amount = cellamt;
                                }

                            }

                            ltadata1.AmountPaid = Amount;
                            //ltadata1.ProcessMonth = ltadata1.ProcessMonth;
                            ltadata1.PayMonth = payMonthDate;                         
                            ltadata1.ProcessMonth = iiProcessMonth;
                            db.BMSPaymentReq.Attach(ltadata1);
                            db.Entry(ltadata1).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            db.Entry(ltadata1).State = System.Data.Entity.EntityState.Detached;

                        }

                        ts.Complete();
                    }
                }
                catch (DbEntityValidationException e)
                {
                    foreach (var eve in e.EntityValidationErrors)
                    {
                        Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                            eve.Entry.Entity.GetType().Name, eve.Entry.State);
                        foreach (var ve in eve.ValidationErrors)
                        {
                            Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                                ve.PropertyName, ve.ErrorMessage);
                        }
                    }
                    throw;
                }
            }
            return CalAmount;
        }


    }
}