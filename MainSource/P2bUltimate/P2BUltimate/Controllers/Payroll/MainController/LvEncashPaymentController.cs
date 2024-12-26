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
using Leave;
namespace P2BUltimate.Controllers.Leave.MainController
{
    [AuthoriseManger]
    public class LvEncashPaymentController : Controller
    {
        // private DataBaseContext db = new DataBaseContext();
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/LvEncashPayment/Index.cshtml");
        }
        public ActionResult partial()
        {
            return View("~/Views/Shared/Payroll/_LvEncashPayment.cshtml");

        }


        public ActionResult GetlvencashreqLKDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.LvEncashReq.ToList();
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.LvEncashReq.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                    }
                }

                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult EditGridDetails(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.LvEncashPayment

                     .Where(e => e.Id == data).Select
                     (e => new
                     {
                         AmountPaid = e.AmountPaid,
                         OtherDeduction = e.OtherDeduction,
                         PaymentDate = e.PaymentDate,
                         PaymentMonth = e.PaymentMonth,
                         ProcessMonth = e.ProcessMonth,
                         TDSAmount = e.TDSAmount,
                         IsCancel = e.IsCancel,
                         TrClosed = e.TrClosed,
                         Action = e.DBTrack.Action
                     }).SingleOrDefault();

                var Lvencashpayment = db.LvEncashPayment.Find(data);
                Session["RowVersion"] = Lvencashpayment.RowVersion;
                var Auth = Lvencashpayment.DBTrack.IsModified;
                //return Json(new Object[] { Q, add_data, "", Auth, JsonRequestBehavior.AllowGet });
                return Json(Q, JsonRequestBehavior.AllowGet);

            }

        }

        public class LvEncashPaymentChildDataClass
        {
            public string Id { get; set; }
            public string AmountPaid { get; set; }
            public string OtherDeduction { get; set; }
            public string TDSAmount { get; set; }
            public string ProcessMonth { get; set; }
            public string PaymentDate { get; set; } public string PaymentMonth { get; set; }
            public Boolean IsCancel { get; set; }


        }

        public ActionResult Get_LvEncashPaymentDetails(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var db_data = db.EmployeePayroll.Include(e => e.LvEncashPayment)
                        .Where(e => e.Id == data).ToList();
                    if (db_data.Count > 0)
                    {
                        List<LvEncashPaymentChildDataClass> returndata = new List<LvEncashPaymentChildDataClass>();
                        foreach (var item in db_data.SelectMany(e => e.LvEncashPayment))
                        {
                            returndata.Add(new LvEncashPaymentChildDataClass
                            {
                                AmountPaid = item.AmountPaid.ToString(),
                                OtherDeduction = item.OtherDeduction.ToString(),
                                PaymentDate = item.PaymentDate != null ? item.PaymentDate.Value.ToShortDateString() : null,
                                PaymentMonth = item.PaymentMonth,
                                ProcessMonth = item.ProcessMonth,
                                TDSAmount = item.TDSAmount.ToString(),
                                IsCancel = item.IsCancel,
                                Id = item.Id.ToString(),

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

        public ActionResult GridEditSave(LvEncashPayment LEP, FormCollection form, string data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var AmountPaid = form["LvEncashPayment-AmountPaid"] == " 0" ? "" : form["LvEncashPayment-AmountPaid"];
                    var TDSAmount = form["LvEncashPayment-TDSAmount"] == " 0" ? "" : form["LvEncashPayment-TDSAmount"];
                    var OtherDeduction = form["LvEncashPayment-OtherDeduction"] == " 0" ? "" : form["LvEncashPayment-OtherDeduction"];
                    LEP.AmountPaid = Convert.ToDouble(AmountPaid);
                    LEP.TDSAmount = Convert.ToDouble(TDSAmount);
                    LEP.OtherDeduction = Convert.ToDouble(OtherDeduction);
                    if (data != null)
                    {
                        var id = Convert.ToInt32(data);
                        var db_data = db.LvEncashPayment.Include(e => e.LvEncashReq).Where(e => e.Id == id).SingleOrDefault();

                        YearlyPaymentT YearlypaymentTData = db.YearlyPaymentT.Include(t => t.LvEncashReq).Where(e => e.LvEncashReq.Id == db_data.LvEncashReq.Id).SingleOrDefault();

                        if (YearlypaymentTData.ReleaseDate == null)
                        {

                            db_data.AmountPaid = LEP.AmountPaid;
                            db_data.TDSAmount = LEP.TDSAmount;
                            db_data.OtherDeduction = LEP.OtherDeduction;

                            YearlypaymentTData.AmountPaid = LEP.AmountPaid;
                            YearlypaymentTData.TDSAmount = LEP.TDSAmount;
                            YearlypaymentTData.OtherDeduction = LEP.OtherDeduction;
                            try
                            {
                                db.LvEncashPayment.Attach(db_data);
                                db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;

                                db.YearlyPaymentT.Attach(YearlypaymentTData);
                                db.Entry(YearlypaymentTData).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(YearlypaymentTData).State = System.Data.Entity.EntityState.Detached;

                                Msg.Add("  Record Updated");
                                return Json(new { status = true, data = db_data.ToString(), responseText = Msg }, JsonRequestBehavior.AllowGet);

                                //    return Json(new Utility.JsonReturnClass { data = db_data.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // return Json(new { data = db_data, responseText = "Record Updated..!" }, JsonRequestBehavior.AllowGet);
                            }
                            catch (Exception e)
                            {

                                throw e;
                            }
                        }
                        else
                        {
                            Msg.Add(" LeaveEncashPayment Is Release You Can Not Change Amount ");
                            return Json(new { status = false, data = db_data.ToString(), responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        Msg.Add("  Data Is Null.  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        // return Json(new { responseText = "Data Is Null" }, JsonRequestBehavior.AllowGet);
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

        public List<int> one_ids(string form)
        {
            string itsec_id = form;
            List<int> ids = itsec_id.Split(',').Select(e => int.Parse(e)).ToList();
            return (ids);
        }

        //public ActionResult Create(LvEncashPayment LEP, FormCollection form, String forwarddata, String LeaveEncashId) //Create submit
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        try
        //        {
        //            //  string Emp = form["Employee-Table"] == "0" ? "" : form["Employee-Table"];

        //            string employee = form["Employee-Table"] == "0" ? "" : form["Employee-Table"];
        //            string Emp = forwarddata == "0" ? "" : forwarddata;
        //            string lvencashid = LeaveEncashId == "0" ? "" : LeaveEncashId;

        //            var salhead = db.SalaryHead.Include(e => e.SalHeadOperationType).Where(e => e.SalHeadOperationType.LookupVal.ToUpper() == "LVENCASH").FirstOrDefault();
        //            List<int> ids = new List<int>();
        //            List<int> idsE = new List<int>();
        //            if (Emp != "null" && Emp != "" && Emp != "false")
        //            {
        //                idsE = one_ids(Emp);

        //                ids = idsE.Distinct().ToList();
        //            }
        //            else
        //            {
        //                List<string> Msgu = new List<string>();
        //                Msgu.Add("  Kindly select employee  ");
        //                return Json(new Utility.JsonReturnClass { success = false, responseText = Msgu }, JsonRequestBehavior.AllowGet);
        //            }

        //            List<int> Lvencashids = null;
        //            List<int> Empids = new List<int>();
        //            if (lvencashid != null && lvencashid != "0" && lvencashid != "false")
        //            {
        //                Lvencashids = one_ids(lvencashid);
        //            }

        //            //string LvEncashReqList = form["LvEncashReqList"] == "0" ? "" : form["LvEncashReqList"];
        //            //if (LvEncashReqList != null && LvEncashReqList != "")
        //            //{
        //            //    var value = db.LvEncashReq.Find(int.Parse(LvEncashReqList));
        //            //    LEP.LvEncashReq = value;

        //            //}
        //            Employee OEmployee = null;
        //            EmployeePayroll OEmployeePayroll = null;
        //            EmployeeLeave OEmployeeLeave = null;

        //            LEP.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

        //            int counter = 0;
        //            if (ModelState.IsValid)
        //            {
        //                if (ids != null)
        //                {
        //                    foreach (var i in ids)
        //                    {
        //                        OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct)
        //                                    .Where(r => r.Id == i).SingleOrDefault();

        //                        OEmployeePayroll = db.EmployeePayroll
        //                      .Where(e => e.Employee.Id == i).SingleOrDefault();

        //                        OEmployeeLeave = db.EmployeeLeave
        //                                        .Include(e => e.Employee)
        //                                        .Include(e => e.LeaveEncashReq)
        //                                        .Include(e => e.LeaveEncashReq.Select(x=>x.LvHead))
        //                                        .Where(e => e.Employee.Id == i).SingleOrDefault();

        //                        var LeaveEncashList = OEmployeeLeave.LeaveEncashReq.Where(e => e.IsCancel == false && e.TrClosed == false).ToList();

        //                        foreach (var item in LeaveEncashList)
        //                        {

        //                            LvEncashPayment ObjID = new LvEncashPayment();
        //                            {
        //                                ObjID.AmountPaid = LEP.AmountPaid;
        //                                ObjID.OtherDeduction = LEP.OtherDeduction;
        //                                ObjID.PaymentDate = LEP.PaymentDate;
        //                                ObjID.PaymentMonth = LEP.PaymentMonth;
        //                                ObjID.TDSAmount = LEP.TDSAmount;
        //                                ObjID.ProcessMonth = LEP.ProcessMonth;
        //                                ObjID.LvEncashReq = item;
        //                                ObjID.DBTrack = LEP.DBTrack;

        //                            }
        //                            using (TransactionScope ts = new TransactionScope())
        //                            {

        //                                ObjID.AmountPaid = PayrollReportGen.LeaveEncashCalc(OEmployeePayroll.Id, ObjID);

        //                                db.LvEncashPayment.Add(ObjID);
        //                                db.SaveChanges();


        //                                YearlyPaymentT objyearlyP = new YearlyPaymentT();
        //                                {
        //                                    //objyearlyP.FromPeriod = LEP.LvEncashReq != null ? LEP.LvEncashReq.FromPeriod : null;
        //                                    //objyearlyP.ToPeriod = LEP.LvEncashReq != null ? LEP.LvEncashReq.ToPeriod : null;
        //                                    objyearlyP.FinancialYear = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "FINANCIALYEAR" && e.Default == true).FirstOrDefault();
        //                                    objyearlyP.FromPeriod = ObjID.LvEncashReq.FromPeriod;
        //                                    objyearlyP.ToPeriod = ObjID.LvEncashReq.ToPeriod;
        //                                    objyearlyP.Narration = ObjID.LvEncashReq.Narration;
        //                                    objyearlyP.AmountPaid = ObjID.AmountPaid;
        //                                    objyearlyP.OtherDeduction = ObjID.OtherDeduction;
        //                                    objyearlyP.PayMonth = ObjID.PaymentMonth;
        //                                    objyearlyP.TDSAmount = ObjID.TDSAmount;
        //                                    objyearlyP.ProcessMonth = ObjID.ProcessMonth;
        //                                    objyearlyP.LvEncashReq = ObjID.LvEncashReq;
        //                                    objyearlyP.SalaryHead = db.SalaryHead.Include(e => e.Frequency).Include(e => e.SalHeadOperationType).Where(e => e.SalHeadOperationType.LookupVal.ToUpper() == "LVENCASH")
        //                                                            .FirstOrDefault();
        //                                    objyearlyP.DBTrack = ObjID.DBTrack;

        //                                }

        //                                objyearlyP.GeoStruct = db.GeoStruct.Find(OEmployee.GeoStruct.Id);

        //                                objyearlyP.FuncStruct = db.FuncStruct.Find(OEmployee.FuncStruct.Id);

        //                                objyearlyP.PayStruct = db.PayStruct.Find(OEmployee.PayStruct.Id);

        //                                objyearlyP.AmountPaid = ObjID.AmountPaid;
        //                                db.YearlyPaymentT.Add(objyearlyP);
        //                                db.SaveChanges();





        //                                List<LvEncashPayment> OFAT = new List<LvEncashPayment>();
        //                                // OFAT.Add(db.LvEncashPayment.Find(ObjID.Id));
        //                                OFAT.Add(ObjID);
        //                                List<YearlyPaymentT> OYP = new List<YearlyPaymentT>();
        //                                //  OYP.Add(db.YearlyPaymentT.Find(objyearlyP.Id));
        //                                OYP.Add(objyearlyP);

        //                                if (OEmployeePayroll == null)
        //                                {
        //                                    EmployeePayroll OTEP = new EmployeePayroll()
        //                                    {
        //                                        Employee = db.Employee.Find(OEmployee.Id),
        //                                        LvEncashPayment = OFAT,
        //                                        YearlyPaymentT = OYP,
        //                                        DBTrack = LEP.DBTrack

        //                                    };


        //                                    db.EmployeePayroll.Add(OTEP);
        //                                    db.SaveChanges();
        //                                }
        //                                else
        //                                {
        //                                    //var aa = db.EmployeePayroll.Find(OEmployeePayroll.Id);
        //                                    //aa.LvEncashPayment = OFAT;
        //                                    //aa.YearlyPaymentT = OYP;
        //                                    ////OEmployeePayroll.DBTrack = dbt;
        //                                    //db.EmployeePayroll.Attach(aa);
        //                                    //db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
        //                                    //db.SaveChanges();
        //                                    //db.Entry(aa).State = System.Data.Entity.EntityState.Detached;

        //                                    EmployeePayroll LeavEncashEmployeepayroll = db.EmployeePayroll
        //                                       .Include(e => e.LvEncashPayment).Where(e => e.Employee.Id == i).SingleOrDefault();

        //                                    if (LeavEncashEmployeepayroll.LvEncashPayment != null)
        //                                    {
        //                                        OFAT.AddRange(LeavEncashEmployeepayroll.LvEncashPayment);
        //                                    }
        //                                    LeavEncashEmployeepayroll.LvEncashPayment = OFAT;

        //                                    db.EmployeePayroll.Attach(LeavEncashEmployeepayroll);
        //                                    db.Entry(LeavEncashEmployeepayroll).State = System.Data.Entity.EntityState.Modified;
        //                                    db.SaveChanges();
        //                                    db.Entry(LeavEncashEmployeepayroll).State = System.Data.Entity.EntityState.Detached;

        //                                    EmployeePayroll YearlypaymentEmployeepayroll = db.EmployeePayroll
        //                                      .Include(e => e.YearlyPaymentT).Where(e => e.Employee.Id == i).SingleOrDefault();

        //                                    if (YearlypaymentEmployeepayroll.YearlyPaymentT != null)
        //                                    {
        //                                        OYP.AddRange(YearlypaymentEmployeepayroll.YearlyPaymentT);
        //                                    }
        //                                    YearlypaymentEmployeepayroll.YearlyPaymentT = OYP;

        //                                    db.EmployeePayroll.Attach(YearlypaymentEmployeepayroll);
        //                                    db.Entry(YearlypaymentEmployeepayroll).State = System.Data.Entity.EntityState.Modified;
        //                                    db.SaveChanges();
        //                                    db.Entry(YearlypaymentEmployeepayroll).State = System.Data.Entity.EntityState.Detached;



        //                                }
        //                                //Deduct Lv start 
        //                                //if (LEP.LvEncashReq != null)
        //                                //{
        //                                //    var EmpID = Convert.ToInt32(Emp);
        //                                //    var OEmployeeLv = db.EmployeeLeave
        //                                //        .Include(e => e.LvNewReq)
        //                                //        .Include(e => e.LvNewReq.Select(a => a.LeaveHead))
        //                                //        .Include(e => e.LvOpenBal.Select(a => a.LvHead))
        //                                //        .Include(e => e.LvNewReq.Select(a => a.LeaveCalendar))
        //                                //        .Where(e => e.Employee.Id == EmpID)
        //                                //        .FirstOrDefault();

        //                                //    LvNewReq PrevReq = null;
        //                                //    if (LEP.LvEncashReq.LvNewReq != null)
        //                                //    {
        //                                //        PrevReq = OEmployeeLv.LvNewReq
        //                                //        .Where(e => e.LeaveHead.Id == LEP.LvEncashReq.LvNewReq.LeaveHead.Id && e.LeaveCalendar.Id == LEP.LvEncashReq.LvNewReq.LeaveCalendar.Id)
        //                                //        .OrderByDescending(e => e.Id).FirstOrDefault();
        //                                //    }
        //                                //    else
        //                                //    {
        //                                //        PrevReq = OEmployeeLv.LvNewReq
        //                                //                      .Where(e => e.LeaveHead.Id == LEP.LvEncashReq.LvHead.Id && e.LeaveCalendar.Id == LEP.LvEncashReq.LeaveCalendar.Id)
        //                                //            .OrderByDescending(e => e.Id).FirstOrDefault();
        //                                //    }
        //                                //    LvNewReq oLvNewReq = null;
        //                                //    if (PrevReq != null)
        //                                //    {
        //                                //        int id = PrevReq.Id;
        //                                //        var LvNewReq = db.LvNewReq.Where(e => e.Id == id)
        //                                //            .Include(e => e.LeaveCalendar).Include(e => e.LeaveHead).Include(e => e.GeoStruct).Include(e => e.PayStruct).Include(e => e.FuncStruct)
        //                                //            .FirstOrDefault();

        //                                //        //  L.LvNewReq = value;
        //                                //        oLvNewReq = new LvNewReq()
        //                                //       {
        //                                //           ReqDate = DateTime.Now,
        //                                //           CreditDays = 0,
        //                                //           FromDate = LvNewReq.FromDate,
        //                                //           FromStat = LvNewReq.FromStat,
        //                                //           LeaveHead = LvNewReq.LeaveHead,
        //                                //           Reason = LvNewReq.Reason,
        //                                //           ResumeDate = LvNewReq.ResumeDate,
        //                                //           ToDate = LvNewReq.ToDate,
        //                                //           ToStat = LvNewReq.ToStat,
        //                                //           LeaveCalendar = LvNewReq.LeaveCalendar,
        //                                //           DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false },
        //                                //           OpenBal = LvNewReq.CloseBal,
        //                                //           DebitDays = LEP.LvEncashReq.EncashDays,
        //                                //           CloseBal = LvNewReq.CloseBal - LEP.LvEncashReq.EncashDays,
        //                                //           LVCount = LvNewReq.DebitDays,
        //                                //           LvOccurances = LvNewReq.LvOccurances,
        //                                //           TrClosed = true,
        //                                //           LvOrignal = LvNewReq,
        //                                //           GeoStruct = LvNewReq.GeoStruct,
        //                                //           PayStruct = LvNewReq.PayStruct,
        //                                //           FuncStruct = LvNewReq.FuncStruct,
        //                                //           //Narration = "Leave Encash Payment",
        //                                //           WFStatus = db.LookupValue.Where(e => e.LookupVal == "7").SingleOrDefault(),
        //                                //       };
        //                                //    }
        //                                //    else
        //                                //    {
        //                                //        var LvEncashReq = db.LvEncashReq.Include(e => e.LvHead).Include(e => e.LeaveCalendar).Where(e => e.Id == LEP.LvEncashReq.Id).SingleOrDefault();
        //                                //        var OpenBalData = db.EmployeeLeave.Include(e => e.LvOpenBal)
        //                                //            .Include(e => e.LvOpenBal.Select(a => a.LvCalendar))
        //                                //            .Include(e => e.LvOpenBal.Select(a => a.LvHead))
        //                                //            .Include(e => e.Employee.GeoStruct)
        //                                //            .Include(e => e.Employee.PayStruct)
        //                                //            .Include(e => e.Employee.FuncStruct)
        //                                //            .Where(e => e.Employee.Id == EmpID && e.LvOpenBal.Count() > 0 && e.LvOpenBal.Any(a => a.LvHead.Id == LvEncashReq.LvHead.Id && a.LvCalendar.Id == LvEncashReq.LeaveCalendar.Id))
        //                                //            .SingleOrDefault();
        //                                //        var OpenBal = OpenBalData.LvOpenBal.Where(e => e.LvHead.Id == LvEncashReq.LvHead.Id && e.LvCalendar.Id == LvEncashReq.LeaveCalendar.Id).SingleOrDefault();
        //                                //        oLvNewReq = new LvNewReq()
        //                                //        {
        //                                //            ReqDate = DateTime.Now,
        //                                //            CreditDays = 0,
        //                                //            FromDate = LEP.LvEncashReq.FromPeriod,
        //                                //            FromStat = db.LookupValue.Where(e => e.LookupVal.ToUpper() == "FULLSESSION").SingleOrDefault(),
        //                                //            LeaveHead = LEP.LvEncashReq.LvHead,
        //                                //            Reason = LEP.LvEncashReq.Narration,
        //                                //            ResumeDate = LEP.LvEncashReq.ToPeriod.Value.AddDays(1),
        //                                //            ToDate = LEP.LvEncashReq.ToPeriod,
        //                                //            ToStat = db.LookupValue.Where(e => e.LookupVal.ToUpper() == "FULLSESSION").SingleOrDefault(),
        //                                //            LeaveCalendar = LEP.LvEncashReq.LeaveCalendar,
        //                                //            DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false },
        //                                //            OpenBal = OpenBal.LvClosing,
        //                                //            DebitDays = LEP.LvEncashReq.EncashDays,
        //                                //            CloseBal = OpenBal.LvClosing - LEP.LvEncashReq.EncashDays,
        //                                //            LVCount = LEP.LvEncashReq.EncashDays,
        //                                //            LvOccurances = 1,
        //                                //            TrClosed = true,
        //                                //            LvOrignal = null,
        //                                //            WFStatus = db.LookupValue.Where(e => e.LookupVal == "7").SingleOrDefault(),

        //                                //            GeoStruct = OpenBalData.Employee.GeoStruct,
        //                                //            PayStruct = OpenBalData.Employee.PayStruct,
        //                                //            FuncStruct = OpenBalData.Employee.FuncStruct,
        //                                //            //IsCancel = true
        //                                //            //Narration = "Leave Encash Payment"
        //                                //        };
        //                                //    }
        //                                //    db.LvNewReq.Add(oLvNewReq);
        //                                //    db.SaveChanges();
        //                                //    OEmployeeLv.LvNewReq.Add(oLvNewReq);
        //                                //    db.Entry(OEmployeeLv).State = System.Data.Entity.EntityState.Modified;
        //                                //      db.SaveChanges();
        //                                // db.Entry(OEmployeeLv).State = System.Data.Entity.EntityState.Detached;
        //                                // return Json(new Object[] { "", "", "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);

        //                                ts.Complete();

        //                                //}

        //                                //  Deduct Lv end
        //                            }
        //                        }
        //                    }
        //                    List<string> Msgs = new List<string>();
        //                    Msgs.Add("Data Saved successfully");
        //                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msgs }, JsonRequestBehavior.AllowGet);
        //                    ////eturn this.Json(new Object[] { "", "", "Unable to create...", JsonRequestBehavior.AllowGet });
        //                    //List<string> Msgu = new List<string>();
        //                    //Msgu.Add("Unable to create...");
        //                    //return Json(new Utility.JsonReturnClass { success = false, responseText = Msgu }, JsonRequestBehavior.AllowGet);
        //                }
        //            }
        //            else
        //            {
        //                StringBuilder sb = new StringBuilder("");
        //                foreach (ModelState modelState in ModelState.Values)
        //                {
        //                    foreach (ModelError error in modelState.Errors)
        //                    {
        //                        sb.Append(error.ErrorMessage);
        //                        sb.Append("." + "\n");
        //                    }
        //                }
        //                //var errorMsg = sb.ToString();
        //                //return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
        //                //return this.Json(new { msg = errorMsg });
        //                List<string> MsgB = new List<string>();
        //                var errorMsg = sb.ToString();
        //                MsgB.Add(errorMsg);
        //                return Json(new Utility.JsonReturnClass { success = false, responseText = MsgB }, JsonRequestBehavior.AllowGet);
        //            }

        //        }
        //        catch (Exception ex)
        //        {
        //            List<string> Msg = new List<string>();
        //            Msg.Add(ex.Message);
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
        //            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //            //return Json(new Object[] { "", "", Msg }, JsonRequestBehavior.AllowGet);
        //        }
        //        return Json(new Utility.JsonReturnClass { success = false, responseText = "Unable to Create" }, JsonRequestBehavior.AllowGet);
        //    }
        //}

        public ActionResult Create(LvEncashPayment LEP, FormCollection form, String forwarddata, String LeaveEncashId) //Create submit
        {

            try
            {
                //  string Emp = form["Employee-Table"] == "0" ? "" : form["Employee-Table"];
                List<string> Msgs = new List<string>();
                string employee = form["Employee-Table"] == "0" ? "" : form["Employee-Table"];
                string Emp = forwarddata == "0" ? "" : forwarddata;
                string lvencashid = LeaveEncashId == "0" ? "" : LeaveEncashId;


                List<int> ids = new List<int>();
                List<int> idsE = new List<int>();
                if (Emp != "null" && Emp != "" && Emp != "false")
                {
                    idsE = one_ids(Emp);

                    ids = idsE.Distinct().ToList();
                }
                else
                {
                    List<string> Msgu = new List<string>();
                    Msgu.Add("  Kindly select employee  ");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msgu }, JsonRequestBehavior.AllowGet);
                }

                List<int> Lvencashids = null;
                List<int> Empids = new List<int>();
                if (lvencashid != null && lvencashid != "0" && lvencashid != "false")
                {
                    Lvencashids = one_ids(lvencashid);
                }

                Employee OEmployee = null;
                EmployeePayroll OEmployeePayroll = null;
                EmployeeLeave OEmployeeLeave = null;

                LEP.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                int salhead = 0;
                int CalId = 0;
                using (DataBaseContext db = new DataBaseContext())
                {
                    salhead = db.SalaryHead.Include(e => e.SalHeadOperationType).Where(e => e.SalHeadOperationType.LookupVal.ToUpper() == "LVENCASH").FirstOrDefault().Id;
                    CalId = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "FINANCIALYEAR" && e.Default == true).FirstOrDefault().Id;
                }

                int counter = 0;
                if (ModelState.IsValid)
                {
                    if (ids != null)
                    {

                        foreach (var i in ids)
                        {
                            using (DataBaseContext db = new DataBaseContext())
                            {
                                OEmployee = db.Employee.Find(i);

                                OEmployeePayroll = db.EmployeePayroll.Where(e => e.Employee_Id == i).FirstOrDefault();

                                OEmployeeLeave = db.EmployeeLeave
                                                .Include(e => e.Employee)
                                                .Include(e => e.LeaveEncashReq)
                                                .Include(e => e.LeaveEncashReq.Select(x => x.LvHead))
                                                .Where(e => e.Employee_Id == i).FirstOrDefault();

                                var LeaveEncashList = OEmployeeLeave.LeaveEncashReq.Where(e => e.IsCancel == false && e.TrClosed == false).ToList();

                                foreach (var item in LeaveEncashList)
                                {

                                    LvEncashPayment ObjID = new LvEncashPayment();
                                    {
                                        ObjID.AmountPaid = LEP.AmountPaid;
                                        ObjID.OtherDeduction = LEP.OtherDeduction;
                                        ObjID.PaymentDate = LEP.PaymentDate;
                                        ObjID.PaymentMonth = LEP.PaymentMonth;
                                        ObjID.TDSAmount = LEP.TDSAmount;
                                        ObjID.ProcessMonth = LEP.ProcessMonth;
                                        ObjID.LvEncashReq = item;
                                        ObjID.DBTrack = LEP.DBTrack;

                                    }
                                    using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, new System.TimeSpan(0, 60, 0)))
                                    {

                                        ObjID.AmountPaid = PayrollReportGen.LeaveEncashCalc(OEmployeePayroll.Id, ObjID);

                                        db.LvEncashPayment.Add(ObjID);
                                        db.SaveChanges();


                                        YearlyPaymentT objyearlyP = new YearlyPaymentT();
                                        {
                                            //objyearlyP.FromPeriod = LEP.LvEncashReq != null ? LEP.LvEncashReq.FromPeriod : null;
                                            //objyearlyP.ToPeriod = LEP.LvEncashReq != null ? LEP.LvEncashReq.ToPeriod : null;
                                            objyearlyP.FinancialYear_Id = CalId;
                                            objyearlyP.FromPeriod = ObjID.LvEncashReq.FromPeriod;
                                            objyearlyP.ToPeriod = ObjID.LvEncashReq.ToPeriod;
                                            objyearlyP.Narration = ObjID.LvEncashReq.Narration;
                                            objyearlyP.AmountPaid = ObjID.AmountPaid;
                                            objyearlyP.OtherDeduction = ObjID.OtherDeduction;
                                            objyearlyP.PayMonth = ObjID.PaymentMonth;
                                            objyearlyP.TDSAmount = ObjID.TDSAmount;
                                            objyearlyP.ProcessMonth = ObjID.ProcessMonth;
                                            objyearlyP.LvEncashReq = ObjID.LvEncashReq;
                                            objyearlyP.SalaryHead_Id = salhead;
                                            objyearlyP.DBTrack = ObjID.DBTrack;

                                        }

                                        objyearlyP.GeoStruct_Id = OEmployee.GeoStruct_Id;

                                        objyearlyP.FuncStruct_Id = OEmployee.FuncStruct_Id;

                                        objyearlyP.PayStruct_Id = OEmployee.PayStruct_Id;

                                        objyearlyP.AmountPaid = ObjID.AmountPaid;
                                        db.YearlyPaymentT.Add(objyearlyP);
                                        db.SaveChanges();





                                        List<LvEncashPayment> OFAT = new List<LvEncashPayment>();
                                        // OFAT.Add(db.LvEncashPayment.Find(ObjID.Id));
                                        OFAT.Add(ObjID);
                                        List<YearlyPaymentT> OYP = new List<YearlyPaymentT>();
                                        //  OYP.Add(db.YearlyPaymentT.Find(objyearlyP.Id));
                                        OYP.Add(objyearlyP);

                                        if (OEmployeePayroll == null)
                                        {
                                            EmployeePayroll OTEP = new EmployeePayroll()
                                            {
                                                Employee = db.Employee.Find(OEmployee.Id),
                                                LvEncashPayment = OFAT,
                                                YearlyPaymentT = OYP,
                                                DBTrack = LEP.DBTrack

                                            };


                                            db.EmployeePayroll.Add(OTEP);
                                            db.SaveChanges();
                                        }
                                        else
                                        {
                                            //var aa = db.EmployeePayroll.Find(OEmployeePayroll.Id);
                                            //aa.LvEncashPayment = OFAT;
                                            //aa.YearlyPaymentT = OYP;
                                            ////OEmployeePayroll.DBTrack = dbt;
                                            //db.EmployeePayroll.Attach(aa);
                                            //db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                            //db.SaveChanges();
                                            //db.Entry(aa).State = System.Data.Entity.EntityState.Detached;

                                            EmployeePayroll LeavEncashEmployeepayroll = db.EmployeePayroll
                                               .Include(e => e.LvEncashPayment).Where(e => e.Employee.Id == i).FirstOrDefault();

                                            if (LeavEncashEmployeepayroll.LvEncashPayment != null)
                                            {
                                                OFAT.AddRange(LeavEncashEmployeepayroll.LvEncashPayment);
                                            }
                                            LeavEncashEmployeepayroll.LvEncashPayment = OFAT;

                                            db.EmployeePayroll.Attach(LeavEncashEmployeepayroll);
                                            db.Entry(LeavEncashEmployeepayroll).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();
                                            db.Entry(LeavEncashEmployeepayroll).State = System.Data.Entity.EntityState.Detached;

                                            EmployeePayroll YearlypaymentEmployeepayroll = db.EmployeePayroll
                                              .Include(e => e.YearlyPaymentT).Where(e => e.Employee.Id == i).FirstOrDefault();

                                            if (YearlypaymentEmployeepayroll.YearlyPaymentT != null)
                                            {
                                                OYP.AddRange(YearlypaymentEmployeepayroll.YearlyPaymentT);
                                            }
                                            YearlypaymentEmployeepayroll.YearlyPaymentT = OYP;

                                            db.EmployeePayroll.Attach(YearlypaymentEmployeepayroll);
                                            db.Entry(YearlypaymentEmployeepayroll).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();
                                            db.Entry(YearlypaymentEmployeepayroll).State = System.Data.Entity.EntityState.Detached;



                                        }


                                        ts.Complete();

                                        //}

                                        //  Deduct Lv end
                                    }
                                }
                            }

                            Msgs.Add("Data Saved successfully - " + OEmployee.EmpCode);
                            //return Json(new Utility.JsonReturnClass { success = true, responseText = Msgs }, JsonRequestBehavior.AllowGet);
                            ////eturn this.Json(new Object[] { "", "", "Unable to create...", JsonRequestBehavior.AllowGet });
                            //List<string> Msgu = new List<string>();
                            //Msgu.Add("Unable to create...");
                            //return Json(new Utility.JsonReturnClass { success = false, responseText = Msgu }, JsonRequestBehavior.AllowGet);
                        }
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msgs }, JsonRequestBehavior.AllowGet);
                    }

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
                    //return this.Json(new { msg = errorMsg });
                    List<string> MsgB = new List<string>();
                    var errorMsg = sb.ToString();
                    MsgB.Add(errorMsg);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = MsgB }, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                List<string> Msg = new List<string>();
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
                //return Json(new Object[] { "", "", Msg }, JsonRequestBehavior.AllowGet);
            }
            return Json(new Utility.JsonReturnClass { success = false, responseText = "Unable to Create" }, JsonRequestBehavior.AllowGet);
            // }
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

        public ActionResult Emp_Grid(ParamModel param, string y)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var all = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.Employee.EmpName).Include(e => e.Employee.ServiceBookDates)
                        .Include(e => e.Employee.GeoStruct).Include(e => e.Employee.GeoStruct.Location.LocationObj).Include(e => e.Employee.FuncStruct)
                        .Include(e => e.Employee.FuncStruct.Job).Include(e => e.Employee.PayStruct).Include(e => e.Employee.PayStruct.Grade)
                        .Include(e => e.LvEncashPayment).Where(e => e.LvEncashPayment.Count > 0).OrderByDescending(e => e.Employee.Id).ToList();

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
                                                                Convert.ToUInt32(sortindex) == 0 ? c.Employee.Id.ToString() :
                                                                sortindex == 1 ? c.Employee.EmpCode : "");

                    //  var sortcolumn = Request["sSortDir_0"];
                    var sortcolumn = Request["order[0][dir]"];

                    if (sortcolumn == "asc")
                    {
                        var da = Request["order[0][column]"];
                        if (da == "1")
                        {
                            fall = fall.OrderBy(orderfunc);
                            fall = fall.OrderBy(a => a.Id);
                        }
                        if (da == "2")
                        {
                            fall = fall.OrderBy(orderfunc);
                            fall = fall.OrderBy(a => a.Employee.EmpCode);
                        }

                    }
                    else
                    {

                        var da = Request["order[0][column]"];
                        if (da == "1")
                        {
                            fall = fall.OrderByDescending(orderfunc);
                            fall = fall.OrderByDescending(e => e.Id);
                        }
                        if (da == "2")
                        {
                            fall = fall.OrderByDescending(orderfunc);
                            fall = fall.OrderByDescending(a => a.Employee.EmpCode);
                        }




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
                                JoiningDate = item.Employee.ServiceBookDates != null && item.Employee.ServiceBookDates.JoiningDate != null ? item.Employee.ServiceBookDates.JoiningDate.Value.ToShortDateString() : null,
                                Job = item.Employee.FuncStruct != null && item.Employee.FuncStruct.Job != null ? item.Employee.FuncStruct.Job.Name : null,
                                Grade = item.Employee.PayStruct != null && item.Employee.PayStruct.Grade != null ? item.Employee.PayStruct.Grade.Name : null,
                                Location = item.Employee.GeoStruct != null && item.Employee.GeoStruct.Location != null && item.Employee.GeoStruct.Location.LocationObj != null ? item.Employee.GeoStruct.Location.LocationObj.LocDesc : null
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

        public ActionResult Process(LvEncashPayment LEP, FormCollection form)
        {

            using (DataBaseContext db = new DataBaseContext())
            {
                string Emp = form["Employee-Table"] == "0" ? "" : form["Employee-Table"];

                string employee = form["Employee-Table"] == "0" ? "" : form["Employee-Table"];
                string LvEncashReqList = form["LvEncashReqList"] == "0" ? "" : form["LvEncashReqList"];
                if (LvEncashReqList != null && LvEncashReqList != "")
                {
                    var value = db.LvEncashReq.Find(int.Parse(LvEncashReqList));
                    LEP.LvEncashReq = value;

                }
                List<int> ids = null;
                if (Emp != null && Emp != "0" && Emp != "false")
                {
                    ids = one_ids(Emp);
                }


                Employee OEmployee = null;
                EmployeePayroll OEmployeePayroll = null;

                LvEncashPayment ObjID = new LvEncashPayment();
                {
                    ObjID.AmountPaid = LEP.AmountPaid;
                    ObjID.OtherDeduction = LEP.OtherDeduction;
                    ObjID.PaymentDate = LEP.PaymentDate;
                    ObjID.PaymentMonth = LEP.PaymentMonth;
                    ObjID.TDSAmount = LEP.TDSAmount;
                    ObjID.ProcessMonth = LEP.ProcessMonth;
                    ObjID.LvEncashReq = LEP.LvEncashReq;
                }


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
                                ObjID.AmountPaid = PayrollReportGen.LeaveEncashCalc(OEmployeePayroll.Id, ObjID);
                            }
                            catch (Exception ex)
                            {
                                List<string> Msg = new List<string>();
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
                                //return Json(new { sucess = false, }, JsonRequestBehavior.AllowGet);
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
                }
                return Json(new { ObjID.AmountPaid });
            }
        }
        public ActionResult GridDelete(string data)
        {

            using (DataBaseContext db = new DataBaseContext())
            {
                //var LvEP = db.LvEncashReq.Find(data);
                var ids = Utility.StringIdsToListIds(data);

                var empidintrnal = Convert.ToInt32(ids[0]);
                var empidMain = Convert.ToInt32(ids[1]);

                var db_data = db.LvEncashPayment.Include(e => e.LvEncashReq)
                     .Where(e => e.Id == empidintrnal && e.IsCancel == false).SingleOrDefault();
                List<string> Msgr = new List<string>();

                YearlyPaymentT YearlypaymentTData = db.YearlyPaymentT.Include(t => t.LvEncashReq).Where(e => e.LvEncashReq.Id == db_data.LvEncashReq.Id).SingleOrDefault();

                if (YearlypaymentTData.ReleaseDate != null)
                {
                    Msgr.Add(" LeaveEncashPayment Is Release You Can Not Delete Record");
                    return Json(new { status = false, data = db_data.ToString(), responseText = Msgr }, JsonRequestBehavior.AllowGet);
                }
                if (db_data == null)
                {
                    Msgr.Add("Record already Canceled. ");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msgr }, JsonRequestBehavior.AllowGet);

                }

                var db_data1 = db.LvEncashPayment
                             .Where(e => e.Id == empidintrnal && e.IsCancel == false).SingleOrDefault();

                var db_data2 = db.LvEncashReq
                             .Where(e => e.Id == db_data.LvEncashReq.Id).SingleOrDefault();

                var lvcalendarid = db.Calendar.Include(e => e.Name)
                    .Where(e => e.Default == true && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();


                var LVENP = db.YearlyPaymentT.Include(e => e.LvEncashReq).Where(e => e.LvEncashReq.Id == db_data2.Id).SingleOrDefault();



                if (db_data.PaymentDate < DateTime.Now)
                {
                    Msgr.Add("You can not cancel this record because payment date is less than system date. ");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msgr }, JsonRequestBehavior.AllowGet);
                }

                if (db_data1 == null)
                {
                    Msgr.Add("Record  has alreday canceled.");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msgr }, JsonRequestBehavior.AllowGet);

                }
                else
                {
                    // start leave req cancel
                    var LvCalendar = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "LEAVECALENDAR" && e.Default == true).SingleOrDefault();
                    var OEmployeeLv = db.EmployeeLeave
                        .Include(e => e.LvNewReq)
                        .Include(e => e.LvNewReq.Select(a => a.LeaveHead))
                        .Include(e => e.LvOpenBal.Select(a => a.LvHead))
                        .Include(e => e.LvNewReq.Select(a => a.GeoStruct))
                        .Include(e => e.LvNewReq.Select(a => a.PayStruct))
                        .Include(e => e.LvNewReq.Select(a => a.FuncStruct))
                        .Include(e => e.LvNewReq.Select(a => a.LeaveCalendar))
                        .Where(e => e.Employee.Id == empidMain)
                        .SingleOrDefault();
                    var PrevReq = OEmployeeLv.LvNewReq
                        .Where(e => e.LeaveHead.Id == db_data2.LvHead.Id && e.LeaveCalendar.Id == LvCalendar.Id

                            )
                        .OrderByDescending(e => e.Id).FirstOrDefault();


                    LvNewReq oLvNewReq = new LvNewReq()
                    {
                        ReqDate = DateTime.Now,

                        DebitDays = 0,
                        CreditDays = db_data2.EncashDays,
                        FromDate = db_data2.FromPeriod,
                        FromStat = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "478").FirstOrDefault().LookupValues.Where(e => e.LookupVal.ToUpper() == "FULLSESSION").FirstOrDefault(), //db.LookupValue.Where(a => a.LookupVal.ToUpper() == "FULLSESSION").Distinct().SingleOrDefault(),
                        LeaveHead = db_data2.LvHead,
                        //Reason = db_data1.Reason,
                        ResumeDate = DateTime.Now,
                        ToDate = db_data2.ToPeriod,
                        ToStat = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "478").FirstOrDefault().LookupValues.Where(e => e.LookupVal.ToUpper() == "FULLSESSION").FirstOrDefault(),
                        LeaveCalendar = lvcalendarid,
                        DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false },
                        OpenBal = PrevReq.CloseBal,
                        CloseBal = PrevReq.CloseBal + db_data2.EncashDays,
                        LVCount = PrevReq.LVCount - db_data2.EncashDays,
                        LvOccurances = PrevReq.LvOccurances,
                        TrClosed = true,
                        LvOrignal = PrevReq.LvOrignal,
                        GeoStruct = PrevReq.GeoStruct,
                        PayStruct = PrevReq.PayStruct,
                        FuncStruct = PrevReq.FuncStruct,
                        IsCancel = true,
                        WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal.ToUpper() == "2").FirstOrDefault(),
                        Narration = "Leave Encashment Cancelled"
                    };

                    db.LvNewReq.Add(oLvNewReq);
                    // db.SaveChanges();

                    var aa = db.EmployeeLeave.Where(e => e.Employee.Id == empidMain)
                        .SingleOrDefault();
                    //   oLvNewReq.Add(aa.LvNewReq);
                    // aa.LvNewReq = oLvNewReq;
                    //OEmployeePayroll.DBTrack = dbt;
                    aa.LvNewReq.Add(oLvNewReq);
                    db.EmployeeLeave.Attach(aa);
                    db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    db.Entry(aa).State = System.Data.Entity.EntityState.Detached;


                    // End leave req cancel


                    db_data.IsCancel = true;
                    db_data.TrClosed = true;
                    db.LvEncashPayment.Attach(db_data);
                    //  db.SaveChanges();
                    db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;

                    db_data2.WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal.ToUpper() == "4").FirstOrDefault();//db.LookupValue.Where(e => e.LookupVal == "4").SingleOrDefault();
                    db_data2.IsCancel = true;
                    db_data2.TrClosed = true;
                    db.LvEncashReq.Attach(db_data2);
                    //  db.SaveChanges();
                    db.Entry(db_data2).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    db.Entry(db_data2).State = System.Data.Entity.EntityState.Detached;


                    if (LVENP != null)
                    {
                        db.YearlyPaymentT.Attach(LVENP);
                        db.Entry(LVENP).State = System.Data.Entity.EntityState.Deleted;
                        db.SaveChanges();
                        db.Entry(LVENP).State = System.Data.Entity.EntityState.Detached;

                    }



                    Msgr.Add("Record Cancel Successfully  ");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msgr }, JsonRequestBehavior.AllowGet);

                }
            }
        }

        public ActionResult PopulateDropDownStructureList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                int id = Convert.ToInt32(data2);
                List<LvEncashReq> newemp = new List<LvEncashReq>();
                var chk = db.EmployeeLeave.Include(e => e.LeaveEncashReq).Where(e => e.Employee.Id == id).ToList();
                foreach (var item in chk)
                {
                    foreach (var lm in item.LeaveEncashReq)
                    {
                        if (lm.IsCancel == false)
                        {
                            newemp.Add(lm);
                        }
                    }
                }
                if (newemp.Count > 0)
                {
                    var lk =
               new SelectList((from s in newemp
                               // from p in s.LeaveEncashReq
                               select new
                               {
                                   Id = s.Id,
                                   NewReqDetails = "FROMPERIOD:" + " " + s.FromPeriod.Value.ToShortDateString() + "" + "TOPERIOD:" + " " + s.ToPeriod.Value.ToShortDateString() + "  " + "  " + "ENCASHDAYS:" + " " + s.EncashDays
                               }).Distinct(), //
                     "Id",
                     "NewReqDetails",
                       null);
                    return Json(lk, JsonRequestBehavior.AllowGet);
                }
                return null;
            }
        }
        public class P2BCrGridData
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
            public string Encashdays { get; set; }
            public string LeaveEncashDetails { get; set; }
            public int LvEncashId { get; set; }
        }

        public ActionResult LoadEmp(P2BGrid_Parameters gp)
        {
            try
            {
                DateTime? dt = null;
                string monthyr = "";
                DataBaseContext db = new DataBaseContext();
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                var jsonData = (Object)null;
                IEnumerable<P2BCrGridData> EmpList = null;
                List<P2BCrGridData> model = new List<P2BCrGridData>();
                P2BCrGridData view = null;
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

                //var BindEmpList = db.EmployeePayroll.Include(e => e.Employee.EmpName).Include(e => e.SalaryT)
                //    .Where(e => e.SalaryT.Any(u => u.PayMonth != "01/2017")).ToList();
                var Cal = db.Calendar.Where(e => e.Default == true && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();

                var compid = Convert.ToInt32(Session["CompId"].ToString());
                var empdata = db.EmployeeLeave
                      .Include(e => e.LeaveEncashReq)
                      .Include(e => e.LeaveEncashReq.Select(t => t.LvHead))
                      .Include(e => e.LeaveEncashReq.Select(t => t.LeaveCalendar))
                      .Include(e => e.Employee)
                      .Include(e => e.Employee.EmpName)
                      .ToList();


                List<EmployeePayroll> EmpLvencashpayment = db.EmployeePayroll
                     .Include(e => e.LvEncashPayment)
                     .Include(e => e.LvEncashPayment.Select(t => t.LvEncashReq))
                     .ToList();
                // var emp = empdata..Select(e => e.Employee).ToList();

                var LeaveEncashId = EmpLvencashpayment.SelectMany(e => e.LvEncashPayment.Select(t => t.LvEncashReq.Id)).ToList();

                foreach (var emp in empdata)
                {

                    var EmployeeLeaveEncashList = emp.LeaveEncashReq.Where(a => a.LeaveCalendar.Id == Cal.Id && a.TrClosed == false && !LeaveEncashId.Contains(a.Id)).ToList();
                    if (EmployeeLeaveEncashList.Count() > 0)
                    {
                        foreach (var item in EmployeeLeaveEncashList)
                        {
                            view = new P2BCrGridData()
                            {
                                Id = emp.Employee.Id,
                                Code = emp.Employee.EmpCode,
                                Name = emp.Employee.EmpName.FullNameFML,
                                Encashdays = item.EncashDays.ToString(),
                                LeaveEncashDetails = "LvHead Name:" + item.LvHead.LvName + ", Encash Days:" + item.EncashDays + ", Period: " + item.FromPeriod.Value.ToShortDateString() + "," + item.ToPeriod.Value.ToShortDateString(),
                                LvEncashId = item.Id
                            };

                            model.Add(view);

                        }
                    }

                }
                EmpList = model;

                IEnumerable<P2BCrGridData> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = EmpList;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.Code.ToString().Contains(gp.searchString))
                               || (e.Name.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                               || (e.Encashdays.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                               || (e.LeaveEncashDetails.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                               || (e.LvEncashId.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                               || (e.Id.ToString().Contains(gp.searchString))
                               ).Select(a => new Object[] { a.Code, a.Name, a.Encashdays, a.LeaveEncashDetails, a.LvEncashId, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Code, a.Name, a.Encashdays, a.LeaveEncashDetails, a.LvEncashId, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = EmpList;
                    Func<P2BCrGridData, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c =>
                                         gp.sidx == "Employee Code" ? c.Code.ToString() :
                                         gp.sidx == "Employee Name" ? c.Name.ToString() :
                                           gp.sidx == "Encash Days" ? c.Encashdays.ToString() :
                                         gp.sidx == "LeaveEncashMentDetails" ? c.LeaveEncashDetails.ToString() :
                                         gp.sidx == "LvEncashId" ? c.LvEncashId.ToString() :
                                         "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.Code), a.Name, a.Encashdays, a.LeaveEncashDetails, a.LvEncashId, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.Code), a.Name, a.Encashdays, a.LeaveEncashDetails, a.LvEncashId, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.Code), a.Name, a.Encashdays, a.LeaveEncashDetails, a.LvEncashId, a.Id }).ToList();
                    }
                    totalRecords = EmpList.Count();
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

        public ActionResult LvEncashCancel(string LeaveEncashId, string EmployeeId)
        {

            string lvencashid = LeaveEncashId == "0" ? "" : LeaveEncashId;
            string Emp = EmployeeId == "0" ? "" : EmployeeId;

            try
            {
                List<int> ids = new List<int>();
                List<int> idsE = new List<int>();
                if (Emp != null && Emp != "" && Emp != "false")
                {
                    idsE = one_ids(Emp);

                    ids = idsE.Distinct().ToList();
                }
                else
                {
                    // return Json(new Object[] { "", "", "Kindly select employee" }, JsonRequestBehavior.AllowGet);
                    List<string> Msgu = new List<string>();
                    Msgu.Add("  Kindly select employee  ");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msgu }, JsonRequestBehavior.AllowGet);
                }

                List<int> Lvencashids = null;
                if (lvencashid != null && lvencashid != "" && lvencashid != "false")
                {
                    Lvencashids = one_ids(lvencashid);
                }


                using (DataBaseContext db = new DataBaseContext())
                {

                    var LvCalendar = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "LEAVECALENDAR" && e.Default == true).SingleOrDefault();
                    if (Lvencashids.Count() > 0 && ids.Count() > 0)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            foreach (var item1 in ids)
                            {
                                var EmployeeLeave = db.EmployeeLeave.Include(e => e.Employee)
                                                            .Include(e => e.LeaveEncashReq)
                                                            .Include(e => e.LeaveEncashReq.Select(t => t.LvHead))
                                                            .Include(e => e.LeaveEncashReq.Select(t => t.LeaveCalendar))
                                                            .Include(e => e.LvNewReq.Select(t => t.LeaveHead))
                                                            .Include(e => e.LvNewReq.Select(t => t.WFStatus))
                                                            .Include(e => e.LvNewReq.Select(t => t.LeaveCalendar))
                                                            .Include(e => e.LvNewReq.Select(t => t.PayStruct))
                                                            .Include(e => e.LvNewReq.Select(t => t.GeoStruct))
                                                            .Include(e => e.LvNewReq.Select(t => t.FuncStruct))
                                                            .Where(e => e.Employee.Id == item1).SingleOrDefault();

                                var Lvencashreq = EmployeeLeave.LeaveEncashReq.Where(e => Lvencashids.Contains(e.Id)).ToList();

                                foreach (var item in Lvencashreq)
                                {
                                    try
                                    {
                                        DateTime? fromperiod = item.FromPeriod;
                                        DateTime? toperiod = item.ToPeriod;

                                        LvNewReq Lvnewreq = EmployeeLeave.LvNewReq
                                             .Where(e => e.FromDate == fromperiod &&
                                              e.ToDate == toperiod &&
                                              e.LeaveHead.Id == item.LvHead.Id &&
                                                 // e.LeaveCalendar.Id == LvCalendar.Id &&
                                              e.WFStatus.LookupVal == "2").SingleOrDefault();

                                        if (Lvnewreq != null)
                                        {
                                            var PrevReq = EmployeeLeave.LvNewReq.Where(e => e.LeaveHead.Id == Lvnewreq.LeaveHead.Id).OrderByDescending(e => e.Id)
                                            .FirstOrDefault();

                                            LvNewReq oLvNewReq = new LvNewReq()
                                            {
                                                ReqDate = DateTime.Now,
                                                ContactNo = Lvnewreq.ContactNo,
                                                DebitDays = 0,
                                                InputMethod = 0,
                                                TrClosed = true,
                                                IsCancel = true,
                                                LvOrignal = Lvnewreq,
                                                CreditDays = Lvnewreq.DebitDays,
                                                FromDate = Lvnewreq.FromDate,
                                                FromStat = Lvnewreq.FromStat,
                                                LeaveHead = Lvnewreq.LeaveHead,
                                                Reason = Lvnewreq.Reason,
                                                ResumeDate = Lvnewreq.ResumeDate,
                                                ToDate = Lvnewreq.ToDate,
                                                ToStat = Lvnewreq.ToStat,
                                                LeaveCalendar = Lvnewreq.LeaveCalendar,
                                                DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false },
                                                OpenBal = PrevReq.CloseBal,
                                                CloseBal = PrevReq.CloseBal + Lvnewreq.DebitDays,
                                                LVCount = PrevReq.LVCount - Lvnewreq.DebitDays,
                                                LvOccurances = PrevReq.LvOccurances,
                                                GeoStruct = Lvnewreq.GeoStruct,
                                                PayStruct = Lvnewreq.PayStruct,
                                                FuncStruct = Lvnewreq.FuncStruct,
                                                Narration = "Leave Encashment Cancelled",
                                                LvCountPrefixSuffix = Lvnewreq.LvCountPrefixSuffix,
                                                WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal.ToUpper() == "2").FirstOrDefault(),//db.LookupValue.Where(e => e.LookupVal == "2").SingleOrDefault()
                                            };

                                            db.LvNewReq.Add(oLvNewReq);
                                            db.SaveChanges();
                                            EmployeeLeave.LvNewReq.Add(oLvNewReq);

                                            db.Entry(EmployeeLeave).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();
                                            // db.Entry(EmployeeLeave).State = System.Data.Entity.EntityState.Detached;

                                            //update Trclosed in lvencashreq
                                            LvEncashReq lvupdate = db.LvEncashReq.Include(e => e.WFStatus).Where(e => e.Id == item.Id).SingleOrDefault();
                                            lvupdate.TrClosed = true;
                                            lvupdate.IsCancel = true;
                                            lvupdate.TrReject = true;
                                            lvupdate.WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal.ToUpper() == "4").FirstOrDefault();//db.LookupValue.Where(e => e.LookupVal == "4").SingleOrDefault();

                                            db.LvEncashReq.Attach(lvupdate);
                                            db.Entry(lvupdate).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();
                                            db.Entry(lvupdate).State = System.Data.Entity.EntityState.Detached;

                                        }
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
                                        return Json(new { success = false, responseText = ex.Message }, JsonRequestBehavior.AllowGet);
                                    }

                                }
                            }
                            ts.Complete();
                            List<string> Msg = new List<string>();
                            Msg.Add("  Data Saved successfully  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
                    LineNo = Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                    LogTime = DateTime.Now
                };
                Logfile.CreateLogFile(Err);
                return Json(new { success = false, responseText = ex.Message }, JsonRequestBehavior.AllowGet);
            }
            return null;
        }

    }
}