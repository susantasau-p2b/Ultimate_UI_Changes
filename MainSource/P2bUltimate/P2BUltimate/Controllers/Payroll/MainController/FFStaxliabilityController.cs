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
    public class FFStaxliabilityController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();

        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/FFStaxliability/Index.cshtml");
        }

        public List<int> one_ids(string form)
        {
            string itsec_id = form;
            List<int> ids = itsec_id.Split(',').Select(e => int.Parse(e)).ToList();
            return (ids);
        }
        public ActionResult partial()
        {
            return View("~/Views/Shared/Payroll/_YearlyPaymentTGridPartial.cshtml");

        }

        public ActionResult EmpLoad(ParamModel pm)
        {
            return View();
        }
        #region Create

        public ActionResult Create(YearlyPaymentT Y, FormCollection form, String forwarddata) //Create submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string Emp = form["Employee-Table"] == "0" ? "" : form["Employee-Table"];
                    string SalaryHeadlist = form["SalaryHeadlist"] == "0" ? "" : form["SalaryHeadlist"];
                    string FromPeriod = form["FromPeriod"] == "0" ? "" : form["FromPeriod"];
                    string ToPeriod = form["ToPeriod"] == "0" ? "" : form["ToPeriod"];
                    string ProcessMonth = form["ProcessMonth"] == "0" ? "" : form["ProcessMonth"];
                    string AmountPaid = form["AmountPaid"] == "0" ? "" : form["AmountPaid"];
                    string TDSAmount = form["TDSAmount"] == "0" ? "" : form["TDSAmount"];
                    string OtherDeduction = form["OtherDeduction"] == "0" ? "" : form["OtherDeduction"];
                    string LvEncashReq = form["LvEncashReq"] == "0" ? "" : form["LvEncashReq"];
                    string Narration = form["Narration"] == "0" ? "" : form["Narration"];
                    string employee = form["Employee-Table"] == "0" ? "" : form["Employee-Table"];

                    var fyyr = db.Calendar.Include(a => a.Name).Where(a => a.Name.LookupVal.ToUpper() == "FINANCIALYEAR" && a.Default == true).SingleOrDefault();
                    Y.FinancialYear = fyyr;
                    List<int> ids = null;
                    if (Emp != null && Emp != "0" && Emp != "false")
                    {
                        ids = one_ids(Emp);
                    }
                    else
                    {
                        List<string> Msgu = new List<string>();
                        Msgu.Add("  Kindly select employee  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msgu }, JsonRequestBehavior.AllowGet);
                    }
                    if (SalaryHeadlist != null && SalaryHeadlist != "")
                    {
                        var Val = db.SalaryHead.Find(int.Parse(SalaryHeadlist));
                        Y.SalaryHead = Val;
                    }
                    if (FromPeriod != null && FromPeriod != "")
                    {
                        var Val = DateTime.Parse(FromPeriod);
                        Y.FromPeriod = Val;
                    }
                    if (ToPeriod != null && ToPeriod != "")
                    {
                        var Val = DateTime.Parse(ToPeriod);
                        Y.ToPeriod = Val;
                    }
                    if (ProcessMonth != null && ProcessMonth != "")
                    {
                        var Val = ProcessMonth;
                        Y.ProcessMonth = Val;
                    }
                    if (AmountPaid != null && AmountPaid != "")
                    {
                        var Val = double.Parse(AmountPaid);
                        Y.AmountPaid = Val;
                    }
                    if (TDSAmount != null && TDSAmount != "")
                    {
                        var Val = double.Parse(TDSAmount);
                        Y.TDSAmount = Val;
                    }
                    if (OtherDeduction != null && OtherDeduction != "")
                    {
                        var Val = double.Parse(OtherDeduction);
                        Y.OtherDeduction = Val;
                    }
                    if (LvEncashReq != null && LvEncashReq != "")
                    {
                        var Val = db.LvEncashReq.Find(int.Parse(LvEncashReq));
                        Y.LvEncashReq = Val;
                    }
                    if (Narration != null && Narration != "")
                    {

                        var val = Narration;
                        Y.Narration = Narration;
                    }

                    //else
                    //{
                    //    return Json(new { sucess = false, responseText = "Kindly select employee" }, JsonRequestBehavior.AllowGet);
                    //}

                    Employee OEmployee = null;
                    EmployeePayroll OEmployeePayroll = null;

                    Y.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                    YearlyPaymentT ObjYPT = new YearlyPaymentT();
                    {
                        ObjYPT.SalaryHead = Y.SalaryHead;
                        ObjYPT.AmountPaid = Y.AmountPaid;
                        ObjYPT.FromPeriod = Y.FromPeriod;
                        ObjYPT.ToPeriod = Y.ToPeriod;
                        ObjYPT.ProcessMonth = Y.ProcessMonth;
                        ObjYPT.TDSAmount = Y.TDSAmount;
                        ObjYPT.LvEncashReq = Y.LvEncashReq;
                        ObjYPT.OtherDeduction = Y.OtherDeduction;
                        ObjYPT.Narration = Y.Narration;
                        ObjYPT.FinancialYear = Y.FinancialYear;
                        ObjYPT.DBTrack = Y.DBTrack;

                    }
                    if (ModelState.IsValid)
                    {
                        if (ids != null)
                        {
                            foreach (var i in ids)
                            {
                                OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.GeoStruct.Company).Include(e => e.FuncStruct).Include(e => e.PayStruct)
                                    .Include(e => e.ServiceBookDates)
                                    .Where(r => r.Id == i).SingleOrDefault();

                                OEmployeePayroll
                                = db.EmployeePayroll
                              .Where(e => e.Employee.Id == i).SingleOrDefault();


                                ObjYPT.GeoStruct = db.GeoStruct.Find(OEmployee.GeoStruct.Id);


                                ObjYPT.FuncStruct = db.FuncStruct.Find(OEmployee.FuncStruct.Id);


                                ObjYPT.PayStruct = db.PayStruct.Find(OEmployee.PayStruct.Id == null ? 0 : OEmployee.PayStruct.Id);

                                using (TransactionScope ts = new TransactionScope())
                                {
                                    try
                                    {
                                        /////new  
                                        Double[] BonusExGratiaAmt = new Double[5];
                                        double BonusAmt = 0;
                                        double ExGratiaAmt = 0;
                                        var SalHead = db.SalaryHead.Find(int.Parse(SalaryHeadlist));
                                        List<YearlyPaymentT> OFAT = new List<YearlyPaymentT>();


                                        if (SalHead.Name.ToUpper() == "BONUS")
                                        {

                                            CompanyPayroll OCompanyPayroll = null;

                                            var BonusCal = db.Calendar.Include(a => a.Name).Where(a => a.Name.LookupVal.ToUpper() == "BONUSYEAR" && a.Default == true).SingleOrDefault();

                                            Calendar Cal = db.Calendar.Where(e => e.Id == BonusCal.Id).SingleOrDefault();

                                            OCompanyPayroll = db.CompanyPayroll.Where(e => e.Company.Id == OEmployee.GeoStruct.Company.Id).SingleOrDefault();


                                            BonusExGratiaAmt = BonusCalc(OCompanyPayroll.Id, OEmployeePayroll.Id, Cal);
                                            if (BonusExGratiaAmt != null)
                                            {
                                                BonusAmt = BonusExGratiaAmt[0];
                                                ExGratiaAmt = BonusExGratiaAmt[1];

                                            }
                                            if (BonusAmt > 0)
                                            {
                                                var BonusSalhead = db.SalaryHead.Where(e => e.Code.ToUpper() == "BONUS").FirstOrDefault();

                                                ObjYPT.AmountPaid = BonusAmt;
                                                ObjYPT.SalaryHead = null;
                                                ObjYPT.SalaryHead = BonusSalhead;
                                                db.YearlyPaymentT.Add(ObjYPT);
                                                db.SaveChanges();

                                                OFAT.Add(db.YearlyPaymentT.Find(ObjYPT.Id));

                                                if (OEmployeePayroll == null)
                                                {
                                                    EmployeePayroll OTEP = new EmployeePayroll()
                                                    {
                                                        Employee = db.Employee.Find(OEmployee.Id),
                                                        YearlyPaymentT = OFAT,
                                                        DBTrack = Y.DBTrack

                                                    };


                                                    db.EmployeePayroll.Add(OTEP);
                                                    db.SaveChanges();
                                                }
                                                else
                                                {
                                                    var aa = db.EmployeePayroll.Find(OEmployeePayroll.Id);
                                                    aa.YearlyPaymentT = OFAT;
                                                    //OEmployeePayroll.DBTrack = dbt;
                                                    db.EmployeePayroll.Attach(aa);
                                                    db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                                    db.SaveChanges();
                                                    db.Entry(aa).State = System.Data.Entity.EntityState.Detached;

                                                }
                                            }
                                            if (ExGratiaAmt > 0)
                                            {
                                                var ExGratiaSalhead = db.SalaryHead.Where(e => e.Code.ToUpper() == "EXGRATIA").FirstOrDefault();

                                                ObjYPT.AmountPaid = ExGratiaAmt;
                                                ObjYPT.SalaryHead = null;
                                                ObjYPT.SalaryHead = ExGratiaSalhead;
                                                db.YearlyPaymentT.Add(ObjYPT);
                                                db.SaveChanges();

                                                OFAT.Add(db.YearlyPaymentT.Find(ObjYPT.Id));

                                                if (OEmployeePayroll == null)
                                                {
                                                    EmployeePayroll OTEP = new EmployeePayroll()
                                                    {
                                                        Employee = db.Employee.Find(OEmployee.Id),
                                                        YearlyPaymentT = OFAT,
                                                        DBTrack = Y.DBTrack

                                                    };


                                                    db.EmployeePayroll.Add(OTEP);
                                                    db.SaveChanges();
                                                }
                                                else
                                                {
                                                    var aa = db.EmployeePayroll.Find(OEmployeePayroll.Id);
                                                    aa.YearlyPaymentT = OFAT;
                                                    //OEmployeePayroll.DBTrack = dbt;
                                                    db.EmployeePayroll.Attach(aa);
                                                    db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                                    db.SaveChanges();
                                                    db.Entry(aa).State = System.Data.Entity.EntityState.Detached;

                                                }
                                            }

                                        }
                                        /////
                                        else
                                        {
                                            DateTime fromdty = Convert.ToDateTime(ObjYPT.FromPeriod);
                                            DateTime EndDty = Convert.ToDateTime(ObjYPT.ToPeriod);
                                            int YearDaysdiff = (EndDty - fromdty).Days + 1;
                                            var Id = Convert.ToInt32(SessionManager.CompanyId);
                                            string CompCode = db.Company.Where(e => e.Id == Id).SingleOrDefault().Code.ToUpper();

                                            if (OEmployee.ServiceBookDates.RetirementDate < ObjYPT.ToPeriod)
                                            {
                                                if (CompCode != "ASBL")
                                                {
                                                    ObjYPT.ToPeriod = Convert.ToDateTime(OEmployee.ServiceBookDates.RetirementDate);
                                                }

                                            }
                                            DateTime fromdt = Convert.ToDateTime(ObjYPT.FromPeriod);
                                            DateTime EndDt = Convert.ToDateTime(ObjYPT.ToPeriod);

                                            var OEmpSalstructH = db.EmpSalStruct
                                              .Include(e => e.EmpSalStructDetails)
                                            .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead))
                                            .Include(e => e.EmpSalStructDetails.Select(q => q.SalHeadFormula))
                                            .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.Frequency))
                                            .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.Type))
                                            .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.RoundingMethod))
                                            .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.SalHeadOperationType))
                                              .Where(e => e.EmployeePayroll.Id == OEmployeePayroll.Id);


                                            var OEmpSalStruct = OEmpSalstructH
                                                               .Where(e => e.EffectiveDate >= fromdt && e.EffectiveDate <= EndDt).OrderByDescending(e => e.EffectiveDate).ToList();


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
                                                    string PayMonth = mChkDate.Month + "/" + mChkDate.Year;

                                                    var OEmpSalDetails = item.EmpSalStructDetails.Where(e => e.SalaryHead.Id == SalHead.Id).SingleOrDefault();

                                                    if (OEmpSalDetails != null)
                                                    {

                                                        if (checkcell == false)
                                                        {
                                                            cellamt = OEmpSalDetails.Amount;
                                                        }
                                                        if (OEmpSalDetails.EmpSalStruct.EffectiveDate <= fromdt)
                                                        {
                                                            if ((fromdt.Date).Day == 1 && (mChkDate.Date).Day == DateTime.DaysInMonth(Convert.ToInt32(PayMonth.Split('/')[1]), Convert.ToInt32(PayMonth.Split('/')[0])))
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
                                                            if ((OEmpSalDetails.EmpSalStruct.EffectiveDate).Value.Day == 1 && (mChkDate.Date).Day == DateTime.DaysInMonth(Convert.ToInt32(PayMonth.Split('/')[1]), Convert.ToInt32(PayMonth.Split('/')[0])))
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
                                                        var prdays = db.SalAttendanceT.Where(e => e.PayMonth == item && e.EmployeePayroll.Id == OEmployeePayroll.Id).FirstOrDefault();
                                                        if (prdays != null)
                                                        {
                                                            presentdays = presentdays + Convert.ToDouble(prdays.PaybleDays) + Convert.ToDouble(prdays.ArrearDays);
                                                        }

                                                        var mdays = db.SalAttendanceT.Where(e => e.PayMonth == item && e.EmployeePayroll.Id == OEmployeePayroll.Id).FirstOrDefault();
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
                                                ObjYPT.AmountPaid = Amount;
                                            }


                                            db.YearlyPaymentT.Add(ObjYPT);
                                            db.SaveChanges();
                                            //List<YearlyPaymentT> OFAT = new List<YearlyPaymentT>();
                                            OFAT.Add(db.YearlyPaymentT.Find(ObjYPT.Id));

                                            if (OEmployeePayroll == null)
                                            {
                                                EmployeePayroll OTEP = new EmployeePayroll()
                                                {
                                                    Employee = db.Employee.Find(OEmployee.Id),
                                                    YearlyPaymentT = OFAT,
                                                    DBTrack = Y.DBTrack

                                                };


                                                db.EmployeePayroll.Add(OTEP);
                                                db.SaveChanges();
                                            }
                                            else
                                            {
                                                var aa = db.EmployeePayroll.Find(OEmployeePayroll.Id);
                                                aa.YearlyPaymentT = OFAT;
                                                //OEmployeePayroll.DBTrack = dbt;
                                                db.EmployeePayroll.Attach(aa);
                                                db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                                db.SaveChanges();
                                                db.Entry(aa).State = System.Data.Entity.EntityState.Detached;

                                            }
                                        }




                                        ts.Complete();
                                        //return Json(new { sucess = true, responseText = "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
                                        //List<string> Msgs = new List<string>();
                                        //Msgs.Add("  Data Saved successfully  ");
                                        //return Json(new Utility.JsonReturnClass { success = true, responseText = Msgs }, JsonRequestBehavior.AllowGet);
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
                        }
                        else
                        {

                            List<string> Msgu = new List<string>();
                            Msgu.Add("  Unable to create...  ");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msgu }, JsonRequestBehavior.AllowGet);
                            // return this.Json(new Object[] { "", "", "Unable to create...", JsonRequestBehavior.AllowGet });
                        }
                        List<string> Msgs = new List<string>();
                        Msgs.Add("  Data Saved successfully  ");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msgs }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        StringBuilder sb = new StringBuilder("");
                        foreach (ModelState modelState in ModelState.Values)
                        {
                            foreach (ModelError error in modelState.Errors)
                            {
                                sb.Append(error.ErrorMessage);
                                sb.Append("." + "/n");
                            }
                        }
                        List<string> MsgB = new List<string>();
                        var errorMsg = sb.ToString();
                        MsgB.Add(errorMsg);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = MsgB }, JsonRequestBehavior.AllowGet);

                        //var errorMsg = sb.ToString();
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

        #endregion

        #region Release Data

        public JsonResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.YearlyPaymentT
                    .Include(e => e.LvEncashReq)
                    .Include(e => e.SalaryHead)
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        FromPeriod = e.FromPeriod,
                        ToPeriod = e.ToPeriod,
                        ProcessMonth = e.ProcessMonth,
                        PayMonth = e.PayMonth,
                        AmountPaid = e.AmountPaid,
                        TDSAmount = e.TDSAmount,
                        OtherDeduction = e.OtherDeduction,
                        ReleaseFlag = e.ReleaseFlag,
                        ReleaseDate = e.ReleaseDate,
                        Narration = e.Narration,
                        Salaryhead_Id = e.SalaryHead.Id == null ? 0 : e.SalaryHead.Id,
                        Action = e.DBTrack.Action
                    }).ToList();

                var add_data = db.YearlyPaymentT
                  .Include(e => e.LvEncashReq)
                    .Where(e => e.Id == data)
                    .Select(e => new
                    {
                        Lvenchreq_FullDetails = e.LvEncashReq.EncashDays == null ? 0 : e.LvEncashReq.EncashDays,
                        Lvenchreq_Id = e.LvEncashReq.Id == null ? "" : e.LvEncashReq.Id.ToString(),

                    }).ToList();


                var yearlypymentT = db.YearlyPaymentT.Find(data);
                Session["RowVersion"] = yearlypymentT.RowVersion;
                var Auth = yearlypymentT.DBTrack.IsModified;
                //return Json(new Object[] { Q, add_data, "", Auth, JsonRequestBehavior.AllowGet });
                return Json(new Object[] { Q, add_data, Auth }, JsonRequestBehavior.AllowGet);
            }
        }

        public async Task<ActionResult> EditSave(string forwarddata, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            List<string> Msgs = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {

                try
                {
                    string PayMonth = form["PayMonth"] == "0" ? "" : form["PayMonth"];
                    string ReleaseDate = form["ReleaseDate"] == "0" ? "" : form["ReleaseDate"];
                    string ProcessFlag = form["ProcessFlag"] == "0" ? "" : form["ProcessFlag"];
                    string ProcessType = form["ProcTypeList"] == "" ? "" : form["ProcTypeList"];
                    string AmountProcessFlag = form["AmountProcessFlag"] == "0" ? "" : form["AmountProcessFlag"];


                    if (ReleaseDate == null || ReleaseDate == "")
                    {
                        Msg.Add("Enter Realease Date ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }
                    if (PayMonth == null || PayMonth == "")
                    {
                        Msg.Add("Enter Pay Month ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }



                    int CompId = 0;
                    if (!String.IsNullOrEmpty(Session["CompId"].ToString()))
                    {
                        CompId = int.Parse(Session["CompId"].ToString());
                    }
                    List<int> ids = null;
                    if (forwarddata != null && forwarddata != "0" && forwarddata != "false")
                    {
                        ids = one_ids(forwarddata);
                    }
                    try
                    {


                        //using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        //{
                        if (AmountProcessFlag == "true")
                        {
                            //if (ProcessType == "")
                            //{
                            //    Msgs.Add("Please select tax calculation on..!!  ");
                            //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msgs }, JsonRequestBehavior.AllowGet);
                            //}
                            int ProcType = Convert.ToInt32(ProcessType);


                            foreach (var ca in ids)
                            {
                                double FinalTDSAmount = 0;
                                var FinYear = db.Calendar.Where(e => e.Default == true && e.Name.LookupVal.ToString().ToUpper() == "FINANCIALYEAR").SingleOrDefault(); // financial year id
                                DateTime FromPeriod = Convert.ToDateTime(FinYear.FromDate);
                                DateTime ToPeriod = Convert.ToDateTime(FinYear.ToDate);
                                DateTime processdate = DateTime.Now;

                                //using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                              
                                // tax calculation start
                                YearlyPaymentT YearlyPayTTax = db.YearlyPaymentT.Include(q => q.EmployeePayroll).Include(q => q.LvEncashReq).Where(a => a.Id == ca).SingleOrDefault();
                                int empay = YearlyPayTTax.EmployeePayroll.Id;
                                Employee OEmployee = _returnEmployeePayroll(empay);
                                EmployeePayroll OEmployeePayroll = _returnITInvestmentPayment(OEmployee.Id);
                               
                                int OCompanyPayroll = db.CompanyPayroll.Where(e => e.Company.Id == OEmployee.GeoStruct.Company.Id).SingleOrDefault().Id;
                                OEmployeePayroll.RegimiScheme = OEmployeePayroll.RegimiScheme.Where(e => e.FinancialYear_Id == FinYear.Id).ToList();
                                // double status = 1;
                                if (OEmployee.ServiceBookDates.JoiningDate >= FinYear.FromDate)
                                {
                                    FromPeriod = Convert.ToDateTime(OEmployee.ServiceBookDates.JoiningDate);
                                    //IncomeTaxCalc.ITCalculation(OEmployeePayroll, CompId, FinYear, FromPeriod, ToPeriod, processdate, db);
                                    IncomeTaxCalc.ITCalculation(OEmployeePayroll, CompId, FinYear.Id, FromPeriod, ToPeriod, processdate, null, null, 1, ProcType);
                                }
                                else if (OEmployee.ServiceBookDates.ServiceLastDate >= FinYear.FromDate &&
                                   OEmployee.ServiceBookDates.ServiceLastDate <= FinYear.ToDate)
                                {
                                    ToPeriod = Convert.ToDateTime(OEmployee.ServiceBookDates.ServiceLastDate);
                                    //IncomeTaxCalc.ITCalculation(OEmployeePayroll, CompId, FinYear, FromPeriod, ToPeriod, processdate, db);
                                    IncomeTaxCalc.ITCalculation(OEmployeePayroll, CompId, FinYear.Id, FromPeriod, ToPeriod, processdate, null, null, 1, ProcType);
                                }
                                else if (OEmployee.ServiceBookDates.RetirementDate >= FinYear.FromDate &&
                                   OEmployee.ServiceBookDates.RetirementDate <= FinYear.ToDate)
                                {
                                    ToPeriod = Convert.ToDateTime(OEmployee.ServiceBookDates.RetirementDate);
                                    IncomeTaxCalc.ITCalculation(OEmployeePayroll, CompId, FinYear.Id, FromPeriod, ToPeriod, processdate, null, null, 1, ProcType);
                                }
                                else
                                {
                                    IncomeTaxCalc.ITCalculation(OEmployeePayroll, CompId, FinYear.Id, FromPeriod, ToPeriod, processdate, null, null, 1, ProcType);
                                }

                                double EmpIncome = db.ITProjection.Where(e => e.EmployeePayroll_Id == OEmployeePayroll.Id && e.FinancialYear.Id == FinYear.Id && e.PickupId == 121).SingleOrDefault().ProjectedAmount;
                                CompanyPayroll OIncomeTax = _returnCompanyPayroll_IncomeTax1(OCompanyPayroll);
                                IncomeTax OITMaster = OIncomeTax.IncomeTax.Where(e => e.FyCalendar.Id == FinYear.Id).SingleOrDefault();
                                Double[] ITPerc = new Double[5];
                                ITPerc = TDSCalc(OEmployeePayroll, OITMaster, EmpIncome, ToPeriod);

                                var ITaxPerc = ITPerc[0];
                                var EduCessPerc = ITPerc[1];
                                double TDSAmount1 = 0;
                                double TDSAmount = 0;
                                double EduCess = 0;


                                double finaltax = db.ITProjection.Where(e => e.EmployeePayroll_Id == OEmployeePayroll.Id && e.FinancialYear.Id == FinYear.Id && e.PickupId == 133).SingleOrDefault().ProjectedAmount;
                                if (finaltax == 0)
                                {
                                    ITaxPerc = 0;
                                }
                                double AmtPaid = db.YearlyPaymentT.Where(e => e.Id == ca).SingleOrDefault().AmountPaid;

                                TDSAmount1 = Math.Round((AmtPaid * ITaxPerc) / 100);
                                EduCess = Math.Round((TDSAmount1 * EduCessPerc) / 100);

                                TDSAmount = EduCess + TDSAmount1;

                                double BalTax = db.ITProjection
                                                .Where(e => e.EmployeePayroll_Id == OEmployeePayroll.Id && e.FinancialYear.Id == FinYear.Id && e.PickupId == 141)
                                                .FirstOrDefault().ProjectedAmount;

                                if (BalTax > 0)
                                {
                                    if (TDSAmount > BalTax)
                                    {

                                        FinalTDSAmount = BalTax;
                                    }
                                    else
                                    {
                                        FinalTDSAmount = TDSAmount;
                                    }
                                }
                                else
                                {
                                    FinalTDSAmount = 0;
                                }
                                YearlyPayTTax.TDSAmount = FinalTDSAmount;
                                db.YearlyPaymentT.Attach(YearlyPayTTax);
                                db.Entry(YearlyPayTTax).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(YearlyPayTTax).State = System.Data.Entity.EntityState.Detached;

                                //YearlyPaymentT YearlyPayTLvencashpay = db.YearlyPaymentT.Include(q => q.EmployeePayroll).Include(q => q.LvEncashReq).Where(a => a.Id == ca).SingleOrDefault();
                                //if (YearlyPayTLvencashpay.LvEncashReq != null)
                                //{
                                //    LvEncashPayment Lvencashpaymentdata = db.LvEncashPayment.Include(e => e.LvEncashReq).Where(e => e.LvEncashReq.Id == YearlyPayTLvencashpay.LvEncashReq.Id).SingleOrDefault();
                                //    Lvencashpaymentdata.TDSAmount = FinalTDSAmount;
                                //    db.LvEncashPayment.Attach(Lvencashpaymentdata);
                                //    db.Entry(Lvencashpaymentdata).State = System.Data.Entity.EntityState.Modified;
                                //    db.SaveChanges();
                                //    db.Entry(Lvencashpaymentdata).State = System.Data.Entity.EntityState.Detached;
                                //}

                                // tax calculation end

                            }
                        }
               
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        var entry = ex.Entries.Single();
                        var clientValues = (YearlyPaymentT)entry.Entity;
                        var databaseEntry = entry.GetDatabaseValues();
                        if (databaseEntry == null)
                        {
                            Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                        }
                        else
                        {
                            var databaseValues = (YearlyPaymentT)databaseEntry.ToObject();
                            // Y.RowVersion = databaseValues.RowVersion;

                        }
                        List<string> Msgn = new List<string>();
                        Msgn.Add("Record modified by another user.So refresh it and try to save again.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msgn }, JsonRequestBehavior.AllowGet);
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
            Msgs.Add(" Record Updated  ");
            return Json(new Utility.JsonReturnClass { success = true, responseText = Msgs }, JsonRequestBehavior.AllowGet);
        }

        #endregion


        public ActionResult PopulateDropDownList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var qurey = db.SalaryHead.Where(e => e.Frequency.LookupVal.ToUpper() == "YEARLY" && e.SalHeadOperationType.LookupVal.ToUpper() != "PERK").ToList();

                // var qurey = db.SalaryHead.Where(e => e.SalHeadOperationType.LookupVal.ToUpper() == "LVENCASH").ToList();

                var selected = (Object)null;
                if (data2 != "" && data != "0" && data2 != "0")
                {
                    selected = Convert.ToInt32(data2);
                }

                SelectList s = new SelectList(qurey, "Id", "Name", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult getCalendar()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var qurey = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "FINANCIALYEAR" && e.Default == true).AsEnumerable()
                    .Select(e => new
                    {
                        Fycalendardesc = "From Date :" + e.FromDate.Value.ToShortDateString() + " To Date :" + e.ToDate.Value.ToShortDateString(),
                        Fromperiod = e.FromDate.Value.ToShortDateString(),
                        Toperiod = e.ToDate.Value.ToShortDateString()
                    }).SingleOrDefault();
                return Json(qurey, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetlvencashreqDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.LvEncashReq.ToList();
                IEnumerable<LvEncashReq> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.LvEncashReq.ToList().Where(d => d.FullDetails.Contains(data));
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
        //public JsonResult GetSalHead(string data)
        //{
        //    int Id = int.Parse(data);
        //    var qurey = db.SalaryHead.Where(e => e.Frequency.LookupVal.ToUpper() == "YEARLY").Where(e=>e.Id == Id).ToList();
        //    var a = db.SalaryHead.Include(e => e.SalHeadOperationType).Where(e => e.Id == Id).SingleOrDefault();
        //    bool selected = false;
        //    if (a.SalHeadOperationType.LookupVal.ToUpper() == "LVENCASH")
        //    {
        //        selected = true;
        //    }
        //    return Json(selected, JsonRequestBehavior.AllowGet);
        //}
        public JsonResult GetSalHead(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                int Id = int.Parse(data);
                var qurey = db.SalaryHead.Where(e => e.Frequency.LookupVal.ToUpper() == "YEARLY" && e.SalHeadOperationType.LookupVal.ToUpper() != "PERK").Where(e => e.Id == Id).SingleOrDefault();
                var a = db.SalaryHead.Include(e => e.SalHeadOperationType).Where(e => e.Id == Id).SingleOrDefault();
                bool selected = false;
                if (qurey.Name == "LEAVE ENCASH")
                {
                    selected = true;
                }
                return Json(selected, JsonRequestBehavior.AllowGet);
            }
        }

        public class P2bYearlypaymentGridData
        {
            public int Id { get; set; }
            public Employee Employee { get; set; }
            public SalaryHead SalaryHead { get; set; }
            public DateTime? FromPeriod { get; set; }
            public DateTime? ToPeriod { get; set; }
            public string ProcessMonth { get; set; }
            public double AmountPaid { get; set; }
            public double TDSAmount { get; set; }
            public string Narration { get; set; }
            public double OtherDeduction { get; set; }
            public DateTime? ReleaseDate { get; set; }

        }
        public class returndatagridclass
        {
            public string Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
        }
        public ActionResult P2BGridRelease(P2BGrid_Parameters gp)
        {
            try
            {
                DataBaseContext db = new DataBaseContext();
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                var jsonData = (Object)null;

                IEnumerable<P2bYearlypaymentGridData> YearlyPaymentT = null;
                List<P2bYearlypaymentGridData> model = new List<P2bYearlypaymentGridData>();
                P2bYearlypaymentGridData view = null;

                var OEmployee = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.Employee.EmpName).ToList();

                foreach (var z in OEmployee)
                {
                    //var ObjYearlyPaymentT = db.EmployeePayroll.Where(e => e.Id == z.Id).Select(e => e.YearlyPaymentT)
                    //                    .SingleOrDefault();
                    var ObjYearlyPaymentT = db.EmployeePayroll.Include(q => q.YearlyPaymentT).Include(q => q.YearlyPaymentT.Select(s => s.SalaryHead)).Where(e => e.Id == z.Id).SingleOrDefault();

                    //DateTime? Eff_Date = null;
                    //PayScaleAgreement PayScaleAgr = null;
                    foreach (var a in ObjYearlyPaymentT.YearlyPaymentT.Where(e => e.Narration == "Settlement Process"))
                    {
                        //Eff_Date = Convert.ToDateTime(a.ProcessMonth);
                        //  var aa = db.YearlyPaymentT.Where(e => e.Id == a.Id).SingleOrDefault();
                        view = new P2bYearlypaymentGridData()
                        {
                            Id = a.Id,
                            Employee = z.Employee,
                            AmountPaid = a.AmountPaid,
                            FromPeriod = a.FromPeriod,
                            ToPeriod = a.ToPeriod,
                            ProcessMonth = a.ProcessMonth,
                            OtherDeduction = a.OtherDeduction,
                            TDSAmount = a.TDSAmount,
                            Narration = a.Narration,
                            SalaryHead = a.SalaryHead,
                            ReleaseDate = a.ReleaseDate,
                            // EffectiveDate = Eff_Date.Value.ToString("dd/MM/yyyy"),

                            //PayScaleAgreement_Id = PayScaleAgr.Id
                        };

                        model.Add(view);
                    }

                }

                YearlyPaymentT = model;

                IEnumerable<P2bYearlypaymentGridData> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = YearlyPaymentT;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.Employee.EmpCode.ToString().Contains(gp.searchString))
                            || (e.Employee.EmpName.FullNameFML.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                            || (e.SalaryHead.Name.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                            || (e.FromPeriod.ToString().Contains(gp.searchString))
                            || (e.ToPeriod.ToString().Contains(gp.searchString))
                            || (e.AmountPaid.ToString().Contains(gp.searchString))
                            || (e.OtherDeduction.ToString().Contains(gp.searchString))
                            || (e.ProcessMonth.ToString().Contains(gp.searchString))
                            || (e.TDSAmount.ToString().Contains(gp.searchString))
                            || (e.Narration.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                            || (e.Id.ToString().Contains(gp.searchString))
                            ).Select(a => new Object[] { a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.SalaryHead.Name, a.FromPeriod, a.ToPeriod, a.AmountPaid, a.OtherDeduction, a.ProcessMonth, a.TDSAmount, a.Narration, a.Id }).ToList();
                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.SalaryHead.Name, a.FromPeriod, a.ToPeriod, a.AmountPaid, a.OtherDeduction, a.ProcessMonth, a.TDSAmount, a.Narration, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = YearlyPaymentT;
                    Func<P2bYearlypaymentGridData, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "EmpCode" ? c.Employee.EmpCode.ToString() :
                                         gp.sidx == "EmpName" ? c.Employee.EmpName.FullNameFML.ToString() :
                                         gp.sidx == "SalaryHead" ? c.SalaryHead.Name :
                                         gp.sidx == "FromPeriod" ? c.FromPeriod.ToString() :
                                         gp.sidx == "ToPeriod" ? c.ToPeriod.ToString() :
                                         gp.sidx == "AmountPaid" ? c.AmountPaid.ToString() :
                                         gp.sidx == "OtherDeduction" ? c.OtherDeduction.ToString() :
                                         gp.sidx == "ProcessMonth" ? c.ProcessMonth :
                                         gp.sidx == "TDSAmount" ? c.TDSAmount.ToString() :
                                         gp.sidx == "Narration" ? c.Narration.ToString() : ""
                                          );
                    }
                    if (gp.sord == "asc")  //Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : ""
                    {
                        IE = IE.OrderBy(orderfuc);  // a.Id,a.Employeee.EmpCode,a.Employeee.EmpName, a.SalaryHead.Name, a.FromPeriod, a.ToPeriod, a.AmountPaid, a.OtherDeduction, a.ProcessMonth, a.TDSAmount, a.Narration
                        jsonData = IE.Select(a => new Object[] { a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, Convert.ToString(a.SalaryHead) != null ? Convert.ToString(a.SalaryHead.Name) : "", a.FromPeriod == null ? "" : a.FromPeriod.Value.ToShortDateString(), a.ToPeriod == null ? "" : a.ToPeriod.Value.ToShortDateString(), Convert.ToString(a.AmountPaid) != null ? Convert.ToString(a.AmountPaid) : "", Convert.ToString(a.OtherDeduction) != null ? Convert.ToString(a.OtherDeduction) : "", Convert.ToString(a.ProcessMonth) != null ? Convert.ToString(a.ProcessMonth) : "", Convert.ToString(a.TDSAmount) != null ? Convert.ToString(a.TDSAmount) : "", Convert.ToString(a.Narration) != null ? Convert.ToString(a.Narration) : "", a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.SalaryHead.Name, a.FromPeriod, a.ToPeriod, a.AmountPaid, a.OtherDeduction, a.ProcessMonth, a.TDSAmount, a.Narration, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.SalaryHead.Name, a.FromPeriod, a.ToPeriod, a.AmountPaid, a.OtherDeduction, a.ProcessMonth, a.TDSAmount, a.Narration, a.Id }).ToList();
                    }
                    totalRecords = YearlyPaymentT.Count();
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

        //public ActionResult P2BGridRelease(P2BGrid_Parameters gp)
        //{
        //    try
        //    {
        //        DataBaseContext db = new DataBaseContext();
        //        int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
        //        int pageSize = gp.rows;
        //        int totalPages = 0;
        //        int totalRecords = 0;
        //        var jsonData = (Object)null;
        //        if (gp.filter != null)
        //        {
        //            var serializer = new JavaScriptSerializer();
        //            var a = serializer.Deserialize<Utility.GridParaStructIdClass>(gp.filter);
        //        }
        //        IEnumerable<P2bYearlypaymentGridData> yearlypaymentt = null;
        //        List<P2bYearlypaymentGridData> model = new List<P2bYearlypaymentGridData>();
        //        P2bYearlypaymentGridData view = null;

        //        var OEmployee = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.Employee.EmpName).ToList();

        //        foreach (var z in OEmployee)
        //        {
        //            var Oyearlypaymentt = db.EmployeePayroll.Where(e => e.Id == z.Id).Select(e => e.YearlyPaymentT)
        //                                .SingleOrDefault();


        //            //DateTime? Eff_Date = null;
        //            //PayScaleAgreement PayScaleAgr = null;
        //            foreach (var a in Oyearlypaymentt)
        //            {
        //                //Eff_Date = Convert.ToDateTime(a.ProcessMonth);
        //                var aa = db.YearlyPaymentT.Where(e => e.Id == a.Id).SingleOrDefault();
        //                view = new P2bYearlypaymentGridData()
        //                {
        //                    Id = z.Employee.Id,
        //                    Employeee = z.Employee,

        //                    // EndDate = null,

        //                };

        //                model.Add(view);
        //            }

        //        }

        //        yearlypaymentt = model;
        //        IEnumerable<P2bYearlypaymentGridData> IE;
        //        if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
        //        {
        //          IE = yearlypaymentt;
        //            if (gp.searchOper.Equals("eq"))
        //            {
        //                if (gp.searchField == "Id")
        //                    jsonData = IE.Select(a => new { a.Id,a.Employeee.EmpCode,a.Employeee.EmpName, a.SalaryHead.Name, a.FromPeriod, a.ToPeriod, a.AmountPaid, a.OtherDeduction, a.ProcessMonth, a.TDSAmount, a.Narration }).Where((e => (e.Id.ToString().Contains(gp.searchString)))).ToList();
        //                else if (gp.searchField == "EmpCode")
        //                    jsonData = IE.Select(a => new { a.Id, a.Employeee.EmpCode, a.Employeee.EmpName, a.SalaryHead.Name, a.FromPeriod, a.ToPeriod, a.AmountPaid, a.OtherDeduction, a.ProcessMonth, a.TDSAmount, a.Narration }).Where((e => (e.EmpCode.ToString().Contains(gp.searchString)))).ToList();
        //                else if (gp.searchField == "EmpName")
        //                    jsonData = IE.Select(a => new { a.Id, a.Employeee.EmpCode, a.Employeee.EmpName, a.SalaryHead.Name, a.FromPeriod, a.ToPeriod, a.AmountPaid, a.OtherDeduction, a.ProcessMonth, a.TDSAmount, a.Narration }).Where((e => (e.EmpName.ToString().Contains(gp.searchString)))).ToList();
        //                else if (gp.searchField == "SalaryHead")
        //                    jsonData = IE.Select(a => new { a.Id, a.Employeee.EmpCode, a.Employeee.EmpName, a.SalaryHead.Name, a.FromPeriod, a.ToPeriod, a.AmountPaid, a.OtherDeduction, a.ProcessMonth, a.TDSAmount, a.Narration }).Where((e => (e.Name.ToString().Contains(gp.searchString)))).ToList();
        //                else if (gp.searchField == "FromPeriod")
        //                    jsonData = IE.Select(a => new { a.Id, a.Employeee.EmpCode, a.Employeee.EmpName, a.SalaryHead.Name, a.FromPeriod, a.ToPeriod, a.AmountPaid, a.OtherDeduction, a.ProcessMonth, a.TDSAmount, a.Narration }).Where((e => (e.FromPeriod.ToString().Contains(gp.searchString)))).ToList();
        //                else if (gp.searchField == "ToPeriod")
        //                    jsonData = IE.Select(a => new { a.Id, a.Employeee.EmpCode, a.Employeee.EmpName, a.SalaryHead.Name, a.FromPeriod, a.ToPeriod, a.AmountPaid, a.OtherDeduction, a.ProcessMonth, a.TDSAmount, a.Narration }).Where((e => (e.ToPeriod.ToString().Contains(gp.searchString)))).ToList();
        //                else if (gp.searchField == "AmountPaid")
        //                    jsonData = IE.Select(a => new { a.Id, a.Employeee.EmpCode, a.Employeee.EmpName, a.SalaryHead.Name, a.FromPeriod, a.ToPeriod, a.AmountPaid, a.OtherDeduction, a.ProcessMonth, a.TDSAmount, a.Narration }).Where((e => (e.AmountPaid.ToString().Contains(gp.searchString)))).ToList();
        //                else if (gp.searchField == "OtherDeduction")
        //                    jsonData = IE.Select(a => new { a.Id, a.Employeee.EmpCode, a.Employeee.EmpName, a.SalaryHead.Name, a.FromPeriod, a.ToPeriod, a.AmountPaid, a.OtherDeduction, a.ProcessMonth, a.TDSAmount, a.Narration }).Where((e => (e.OtherDeduction.ToString().Contains(gp.searchString)))).ToList();
        //                else if (gp.searchField == "ProcessMonth")
        //                    jsonData = IE.Select(a => new { a.Id, a.Employeee.EmpCode, a.Employeee.EmpName, a.SalaryHead.Name, a.FromPeriod, a.ToPeriod, a.AmountPaid, a.OtherDeduction, a.ProcessMonth, a.TDSAmount, a.Narration }).Where((e => (e.ProcessMonth.ToString().Contains(gp.searchString)))).ToList();
        //                else if (gp.searchField == "TDSAmount")
        //                    jsonData = IE.Select(a => new { a.Id, a.Employeee.EmpCode, a.Employeee.EmpName, a.SalaryHead.Name, a.FromPeriod, a.ToPeriod, a.AmountPaid, a.OtherDeduction, a.ProcessMonth, a.TDSAmount, a.Narration }).Where((e => (e.TDSAmount.ToString().Contains(gp.searchString)))).ToList();
        //                else if (gp.searchField == "Narration")
        //                    jsonData = IE.Select(a => new { a.Id, a.Employeee.EmpCode, a.Employeee.EmpName, a.SalaryHead.Name, a.FromPeriod, a.ToPeriod, a.AmountPaid, a.OtherDeduction, a.ProcessMonth, a.TDSAmount, a.Narration }).Where((e => (e.Narration.ToString().Contains(gp.searchString)))).ToList();

        //                //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.SalaryHead.Name, a.FromPeriod, a.ToPeriod, a.AmountPaid, a.OtherDeduction, a.ProcessMonth, a.TDSAmount, a.Narration }).ToList();
        //            }
        //            totalRecords = IE.Count();
        //        }
        //        else
        //        {
        //            IE = yearlypaymentt;
        //            Func<P2bYearlypaymentGridData, dynamic> orderfuc;
        //            if (gp.sidx == "Id")
        //            {
        //                orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
        //            }
        //            else
        //            {
        //                orderfuc = (c => gp.sidx == "EmpCode" ? c.Employeee.EmpCode.ToString() :
        //                                 gp.sidx == "EmpName" ? c.Employeee.EmpName.ToString() :
        //                                 gp.sidx == "SalaryHead" ? c.SalaryHead.Name :
        //                                 gp.sidx == "FromPeriod" ? c.FromPeriod.ToString() :
        //                                 gp.sidx == "ToPeriod" ? c.ToPeriod.ToString() :
        //                                 gp.sidx == "AmountPaid" ? c.AmountPaid.ToString() :
        //                                 gp.sidx == "OtherDeduction" ? c.OtherDeduction.ToString() :
        //                                 gp.sidx == "ProcessMonth" ? c.ProcessMonth :
        //                                 gp.sidx == "TDSAmount" ? c.TDSAmount.ToString() :
        //                                 gp.sidx == "Narration" ? c.Narration.ToString() :""
        //                                 );
        //            }
        //            if (gp.sord == "asc")
        //            {
        //                IE = IE.OrderBy(orderfuc);                     //a.EndDate != null ? Convert.ToString(a.EndDate) : "" 
        //                jsonData = IE.Select(a => new Object[] { a.Id, a.SalaryHead.Name != null, a.FromPeriod!=null? Convert.ToString(a.FromPeriod):"", Convert.ToString(a.ToPeriod), Convert.ToString(a.AmountPaid), Convert.ToString(a.OtherDeduction), Convert.ToString(a.ProcessMonth), Convert.ToString(a.TDSAmount), Convert.ToString(a.Narration) }).ToList();
        //            }
        //            else if (gp.sord == "desc")
        //            {
        //                IE = IE.OrderByDescending(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, a.SalaryHead.Name, Convert.ToString(a.FromPeriod), Convert.ToString(a.ToPeriod), Convert.ToString(a.AmountPaid), Convert.ToString(a.OtherDeduction), Convert.ToString(a.ProcessMonth), Convert.ToString(a.TDSAmount), Convert.ToString(a.Narration) }).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.SalaryHead.Name, Convert.ToString(a.FromPeriod), Convert.ToString(a.ToPeriod), Convert.ToString(a.AmountPaid), Convert.ToString(a.OtherDeduction), Convert.ToString(a.ProcessMonth), Convert.ToString(a.TDSAmount), Convert.ToString(a.Narration) }).ToList();
        //            }
        //            totalRecords = yearlypaymentt.Count();
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

        //try
        //            {
        //                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
        //                int pageSize = gp.rows;
        //                int totalPages = 0;
        //                int totalRecords = 0;
        //                var jsonData = (Object)null;

        //                IEnumerable<P2BGridData> IncrDueList = null;
        //                List<P2BGridData> model = new List<P2BGridData>();
        //                P2BGridData view = null;

        //                var BindEmpList = db.EmployeePayroll.Include(e => e.Employee.EmpName).Include(e => e.IncrDataCalc).ToList();

        //                foreach (var z in BindEmpList)
        //                {
        //                    if (z.IncrDataCalc != null)
        //                    {
        //                        view = new P2BGridData()
        //                        {
        //                            Id = z.Employee.Id,
        //                            Code = z.Employee.EmpCode,
        //                            Name = z.Employee.EmpName.FullNameFML,
        //                            IncrementOriginalDate = z.IncrDataCalc.OrignalIncrDate.Value.ToString("dd/MM/yyyy"),
        //                            IncrementProcessDate = z.IncrDataCalc.ProcessIncrDate.Value.ToString("dd/MM/yyyy"),
        //                            NewBasic = z.IncrDataCalc.NewBasic,
        //                            OldBasic = z.IncrDataCalc.OldBasic,
        //                            StagnantAppl = z.IncrDataCalc.StagnancyAppl,
        //                            StagnantCount = z.IncrDataCalc.StagnancyCount
        //                        };
        //                        model.Add(view);
        //                    }

        //                }

        //                IncrDueList = model;

        //                IEnumerable<P2BGridData> IE;
        //                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
        //                {
        //                    IE = IncrDueList;
        //                    if (gp.searchOper.Equals("eq"))
        //                    {
        //                        if (gp.searchField == "Id")
        //                            jsonData = IE.Select(a => new { a.Id, a.Code, a.Name, a.IncrementOriginalDate, a.IncrementProcessDate, a.OldBasic, a.NewBasic, a.StagnantAppl, a.StagnantCount }).Where((e => (e.Id.ToString().Contains(gp.searchString)))).ToList();
        //                        if (gp.searchField == "EmpCode")
        //                            jsonData = IE.Select(a => new { a.Id, a.Code, a.Name, a.IncrementOriginalDate, a.IncrementProcessDate, a.OldBasic, a.NewBasic, a.StagnantAppl, a.StagnantCount }).Where((e => (e.Code.ToString().Contains(gp.searchString)))).ToList();
        //                        if (gp.searchField == "EmpName")
        //                            jsonData = IE.Select(a => new { a.Id, a.Code, a.Name, a.IncrementOriginalDate, a.IncrementProcessDate, a.OldBasic, a.NewBasic, a.StagnantAppl, a.StagnantCount }).Where((e => (e.Name.ToString().Contains(gp.searchString)))).ToList();
        //                        else if (gp.searchField == "OldBasic")
        //                            jsonData = IE.Select(a => new { a.Id, a.Code, a.Name, a.IncrementOriginalDate, a.IncrementProcessDate, a.OldBasic, a.NewBasic, a.StagnantAppl, a.StagnantCount }).Where((e => (e.OldBasic.ToString().Contains(gp.searchString)))).ToList();
        //                        else if (gp.searchField == "NewBasic")
        //                            jsonData = IE.Select(a => new { a.Id, a.Code, a.Name, a.IncrementOriginalDate, a.IncrementProcessDate, a.OldBasic, a.NewBasic, a.StagnantAppl, a.StagnantCount }).Where((e => (e.NewBasic.ToString().Contains(gp.searchString)))).ToList();
        //                        else if (gp.searchField == "IncrementOriginalDate")
        //                            jsonData = IE.Select(a => new { a.Id, a.Code, a.Name, a.IncrementOriginalDate, a.IncrementProcessDate, a.OldBasic, a.NewBasic, a.StagnantAppl, a.StagnantCount }).Where((e => (e.IncrementOriginalDate.Contains(gp.searchString)))).ToList();
        //                        else if (gp.searchField == "IncrementProcessDate")
        //                            jsonData = IE.Select(a => new { a.Id, a.Code, a.Name, a.IncrementOriginalDate, a.IncrementProcessDate, a.OldBasic, a.NewBasic, a.StagnantAppl, a.StagnantCount }).Where((e => (e.IncrementProcessDate.Contains(gp.searchString)))).ToList();
        //                        else if (gp.searchField == "StagnantAppl")
        //                            jsonData = IE.Select(a => new { a.Id, a.Code, a.Name, a.IncrementOriginalDate, a.IncrementProcessDate, a.OldBasic, a.NewBasic, a.StagnantAppl, a.StagnantCount }).Where((e => (e.StagnantAppl.ToString().Contains(gp.searchString)))).ToList();
        //                        else if (gp.searchField == "StagnantCount")
        //                            jsonData = IE.Select(a => new { a.Id, a.Code, a.Name, a.IncrementOriginalDate, a.IncrementProcessDate, a.OldBasic, a.NewBasic, a.StagnantAppl, a.StagnantCount }).Where((e => (e.StagnantCount.ToString().Contains(gp.searchString)))).ToList();

        //                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
        //                    }
        //                    if (pageIndex > 1)
        //                    {
        //                        int h = pageIndex * pageSize;
        //                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Code, a.Name, a.IncrementOriginalDate, a.IncrementProcessDate, a.OldBasic, a.NewBasic, a.StagnantAppl, a.StagnantCount }).ToList();
        //                    }
        //                    totalRecords = IE.Count();
        //                }
        //                else
        //                {
        //                    IE = IncrDueList;
        //                    Func<P2BGridData, dynamic> orderfuc;
        //                    if (gp.sidx == "Id")
        //                    {
        //                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
        //                    }
        //                    else
        //                    {
        //                        orderfuc = (c => gp.sidx == "Code" ? c.Code.ToString() :
        //                                         gp.sidx == "Name" ? c.Name.ToString() :
        //                                         gp.sidx == "OldBasic" ? c.OldBasic.ToString() :
        //                                         gp.sidx == "NewBasic" ? c.NewBasic.ToString() :
        //                                         gp.sidx == "IncrementOriginalDate" ? c.IncrementOriginalDate.ToString() :
        //                                         gp.sidx == "IncrementProcessDate" ? c.IncrementProcessDate.ToString() :
        //                                         gp.sidx == "StagnantAppl" ? c.StagnantAppl.ToString() :
        //                                         gp.sidx == "StagnantCount" ? c.StagnantCount.ToString() : "");
        //                    }
        //                    if (gp.sord == "asc")
        //                    {
        //                        IE = IE.OrderBy(orderfuc);
        //                        jsonData = IE.Select(a => new Object[] { a.Id, a.Code, a.Name, a.IncrementOriginalDate != null ? Convert.ToString(a.IncrementOriginalDate) : "", a.IncrementProcessDate != null ? Convert.ToString(a.IncrementProcessDate) : "", a.OldBasic, a.NewBasic, a.StagnantAppl, a.StagnantCount }).ToList();
        //                    }
        //                    else if (gp.sord == "desc")
        //                    {
        //                        IE = IE.OrderByDescending(orderfuc);
        //                        jsonData = IE.Select(a => new Object[] { a.Id, a.Code, a.Name, a.IncrementOriginalDate != null ? Convert.ToString(a.IncrementOriginalDate) : "", a.IncrementProcessDate != null ? Convert.ToString(a.IncrementProcessDate) : "", a.OldBasic, a.NewBasic, a.StagnantAppl, a.StagnantCount }).ToList();
        //                    }
        //                    if (pageIndex > 1)
        //                    {
        //                        int h = pageIndex * pageSize;
        //                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Code, a.Name,  a.IncrementOriginalDate != null ? Convert.ToString(a.IncrementOriginalDate) : "", a.IncrementProcessDate != null ? Convert.ToString(a.IncrementProcessDate) : "", a.OldBasic, a.NewBasic, a.StagnantAppl, a.StagnantCount }).ToList();
        //                    }
        //                    totalRecords = IncrDueList.Count();
        //                }
        //                if (totalRecords > 0)
        //                {
        //                    totalPages = (int)Math.Ceiling((float)totalRecords / (float)gp.rows);
        //                }
        //                if (gp.page > totalPages)
        //                {
        //                    gp.page = totalPages;
        //                }
        //                var JsonData = new
        //                {
        //                    page = gp.page,
        //                    rows = jsonData,
        //                    records = totalRecords,
        //                    total = totalPages
        //                };
        //                return Json(JsonData, JsonRequestBehavior.AllowGet);
        //            }
        //            catch (Exception ex)
        //            {
        //                throw ex;
        //            }
        //        }
        //public ActionResult LoadEmpByDefault(string data, string data2)
        //{
        //    Session["CompId"] = "1";
        //    if (Session["CompId"] != null)
        //    {
        //        int CompId = int.Parse(Session["CompId"].ToString());
        //        var query = db.Company.Where(e => e.Id == CompId).Include(e => e.Employee.Select(r => r.EmpName)).SingleOrDefault();
        //        List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
        //        foreach (var ca in query.Employee)
        //        {
        //            returndata.Add(new Utility.returndataclass
        //            {
        //                code = ca.Id.ToString(),
        //                value = ca.FullDetails,
        //            });
        //        }
        //        var jsondata = new
        //        {
        //            tablename = "Employee-Table",
        //            data = returndata,
        //        };
        //        return Json(jsondata, JsonRequestBehavior.AllowGet);
        //    }
        //    return Json("", JsonRequestBehavior.AllowGet);
        //}

        public ActionResult GridDelete(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                var db_data = db.YearlyPaymentT.Where(e => e.Id == data).SingleOrDefault();
                if (db_data != null)
                {
                    db.YearlyPaymentT.Attach(db_data);
                    db.Entry(db_data).State = System.Data.Entity.EntityState.Deleted;
                    db.SaveChanges();
                    db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;
                    Msg.Add("  Data removed successfully.  ");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //return Json(new { data = "", responseText = "Record Deleted..!" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return null;
                }
            }
        }

        public class empgriddetails
        {
            public int Id { get; set; }
            public string EmployeeCode { get; set; }
            public string EmployeeName { get; set; }



        }
        public ActionResult Emp_Grid(ParamModel param, string y)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {

                    var all = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.YearlyPaymentT)
                        .Include(e => e.Employee.ServiceBookDates).AsNoTracking()
                        .Where(e => e.Employee.ServiceBookDates.ServiceLastDate == null).ToList();
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
                    var dcompaines = fall
                            .Skip(param.iDisplayStart).Take(param.iDisplayLength).ToList();
                    if (dcompaines.Count == 0)
                    {
                        List<returndatagridclass> result = new List<returndatagridclass>();
                        foreach (var item in fall)
                        {
                            result.Add(new returndatagridclass
                            {
                                Id = item.Id.ToString(),
                                Code = item.Employee.CardCode,

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
                        var result = from c in dcompaines

                                     select new[] { null, Convert.ToString(c.Id), c.Employee.EmpCode };
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

        public class YearlyPaymentChildDataClass
        {
            public String Id { get; set; }
            public String TDSAmount { get; set; }
            public String AmountPaid { get; set; }
            public String OtherDeduction { get; set; }
            public String Narration { get; set; }
            public string SalaryHead { get; set; }
            public String FromPeriod { get; set; }
            public String ToPeriod { get; set; }
            public String PayMonth { get; set; }
        }
        public ActionResult Get_YearlyPayment(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var db_data = db.EmployeePayroll
                        .Include(e => e.YearlyPaymentT)
                        .Include(e => e.YearlyPaymentT.Select(x => x.SalaryHead))
                        .Where(e => e.Id == data).ToList();
                    if (db_data.Count > 0)
                    {
                        List<YearlyPaymentChildDataClass> returndata = new List<YearlyPaymentChildDataClass>();
                        foreach (var item in db_data.SelectMany(e => e.YearlyPaymentT))
                        {
                            if (item.Narration == "Settlement Process")
                            {
                                returndata.Add(new YearlyPaymentChildDataClass
                                {
                                    Id = item.Id.ToString(),
                                    AmountPaid = item.AmountPaid.ToString(),
                                    OtherDeduction = item.OtherDeduction.ToString(),
                                    Narration = item.Narration,
                                    TDSAmount = item.TDSAmount.ToString(),
                                    SalaryHead = item.SalaryHead != null ? item.SalaryHead.Name : null,
                                    FromPeriod = item.FromPeriod.Value.Date.ToShortDateString(),
                                    ToPeriod = item.ToPeriod.Value.ToShortDateString(),
                                    PayMonth = item.PayMonth != null ? item.PayMonth.ToString() : ""

                                });
                            }
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

        public JsonResult EditGridDetails(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.YearlyPaymentT
                     .Include(e => e.LvEncashReq)
                     .Include(e => e.SalaryHead)
                     .Where(e => e.Id == data).Select
                     (e => new
                     {
                         FromPeriodP = e.FromPeriod,
                         ToPeriodP = e.ToPeriod,
                         ProcessMonthP = e.ProcessMonth,
                         AmountPaidP = e.AmountPaid,
                         TDSAmountP = e.TDSAmount,
                         OtherDeductionP = e.OtherDeduction,
                         ReleaseFlagP = e.ReleaseFlag,
                         ReleaseDateP = e.ReleaseDate,
                         NarrationP = e.Narration,
                         Salaryhead_IdP = e.SalaryHead.Id == null ? 0 : e.SalaryHead.Id,
                         Action = e.DBTrack.Action,
                         LvencashId = e.LvEncashReq == null ? "" : e.LvEncashReq.Id.ToString(),
                     }).SingleOrDefault();
                var yearlypymentT = db.YearlyPaymentT.Find(data);
                Session["RowVersion"] = yearlypymentT.RowVersion;
                var Auth = yearlypymentT.DBTrack.IsModified;
                //return Json(new Object[] { Q, add_data, "", Auth, JsonRequestBehavior.AllowGet });
                return Json(Q, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GridEditSave(YearlyPaymentT ypay, FormCollection form, string data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var AmountPaid = form["AmountPaid"] == " 0" ? "" : form["AmountPaid"];
                    ypay.AmountPaid = Convert.ToDouble(AmountPaid);
                    var TDSAmount = form["TDSAmount"] == " 0" ? "" : form["TDSAmount"];
                    ypay.TDSAmount = Convert.ToDouble(TDSAmount);
                    var OtherDeduction = form["OtherDeduction"] == " 0" ? "" : form["OtherDeduction"];
                    ypay.OtherDeduction = Convert.ToDouble(OtherDeduction);
                    if (data != null)
                    {
                        var id = Convert.ToInt32(data);
                        var db_data = db.YearlyPaymentT.Where(e => e.Id == id).SingleOrDefault();
                        db_data.AmountPaid = ypay.AmountPaid;
                        db_data.TDSAmount = ypay.TDSAmount;
                        db_data.OtherDeduction = ypay.OtherDeduction;
                        try
                        {
                            db.YearlyPaymentT.Attach(db_data);
                            db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;
                            return this.Json(new { status = true, responseText = "Record Updated Successfully.", JsonRequestBehavior.AllowGet });
                            //Msg.Add("  Record Updated");
                            //return Json(new Utility.JsonReturnClass { data = db_data.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            // return Json(new { data = db_data, responseText = "Record Updated..!" }, JsonRequestBehavior.AllowGet);
                        }
                        catch (Exception e)
                        {

                            throw e;
                        }
                    }
                    else
                    {
                        return this.Json(new { status = false, responseText = "  Data Is Null", JsonRequestBehavior.AllowGet });
                        //     Msg.Add("  Data Is Null  ");
                        //return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //return Json(new { responseText = "Data Is Null" }, JsonRequestBehavior.AllowGet);
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


        public double YearlyCal(SalaryHead SalHd, Employee Emp, string PayMonth, DateTime? FromPeriod, DateTime? ToPeriod)
        {
            double TotAmt = 0;
            using (DataBaseContext db = new DataBaseContext())
            {
                SalaryHead SalHead = db.SalaryHead.Include(e => e.ProcessType).Where(e => e.Id == SalHd.Id).SingleOrDefault();

                var OEmployeePayroll = db.EmployeePayroll
                    .Where(e => e.Employee.Id == Emp.Id).Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails))
                    .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead)))
                      .SingleOrDefault();


                var OEmpSalStruct = OEmployeePayroll.EmpSalStruct.OrderByDescending(e => e.EffectiveDate).Select(e => e.EmpSalStructDetails.Where(r => r.SalaryHead.Id == SalHd.Id)).FirstOrDefault();

                var LWPDaysQuery = db.EmployeePayroll
                .Where(e => e.Employee.Id == Emp.Id).Select(e => e.SalAttendance.Where(r => r.PayMonth == PayMonth)).SingleOrDefault();

                double LWPDays = LWPDaysQuery.Select(e => e.LWPDays).SingleOrDefault();

                if (SalHead.ProcessType.LookupVal.ToString().ToUpper() == "REGULAR")
                {
                    if (SalHead.OnAttend == true)
                    {
                        foreach (var s in OEmpSalStruct)
                        {
                            TotAmt = s.Amount;
                        }
                    }
                    else
                    {
                        foreach (var s in OEmpSalStruct)
                        {
                            TotAmt = s.Amount;
                        }
                    }
                }
                else if (SalHead.ProcessType.LookupVal.ToString().ToUpper() == "EARNED")
                {
                    if (SalHead.OnAttend == true)
                    {

                    }
                    else
                    {
                        foreach (var s in OEmpSalStruct)
                        {
                            TotAmt = s.Amount;
                        }
                    }
                }
                else if (SalHead.ProcessType.LookupVal.ToString().ToUpper() == "FIXEDMONTH")
                {
                    if (SalHead.OnAttend == true)
                    {
                        int TotDays = Convert.ToDateTime("01/" + PayMonth).DayOfYear;
                        foreach (var s in OEmpSalStruct)
                        {
                            TotAmt = s.Amount * LWPDays / TotDays;
                        }
                    }
                    else
                    {
                        foreach (var s in OEmpSalStruct)
                        {
                            TotAmt = s.Amount;
                        }
                    }
                }
                else if (SalHead.ProcessType.LookupVal.ToString().ToUpper() == "PRORATA")
                {
                    if (SalHead.OnAttend == true)
                    {
                        while (FromPeriod <= ToPeriod)
                        {
                            PayMonth = FromPeriod.Value.Month + "/" + FromPeriod.Value.Year;
                            LWPDaysQuery = db.EmployeePayroll
                            .Where(e => e.Employee.Id == Emp.Id).Select(e => e.SalAttendance.Where(r => r.PayMonth == PayMonth)).SingleOrDefault();

                            LWPDays = LWPDaysQuery.Select(e => e.LWPDays).SingleOrDefault();

                            int TotDays = DateTime.DaysInMonth(Convert.ToInt32(PayMonth.Split('/')[1]), Convert.ToInt32(PayMonth.Split('/')[0]));
                            OEmpSalStruct = OEmployeePayroll.EmpSalStruct.Select(e => e.EmpSalStructDetails.Where(r => r.SalaryHead.Id == SalHd.Id)).FirstOrDefault();
                            foreach (var s in OEmpSalStruct)
                            {
                                TotAmt = TotAmt + s.Amount * LWPDays / TotDays;
                            }
                            FromPeriod = FromPeriod.Value.AddMonths(1);
                        }

                    }
                    else
                    {
                        while (FromPeriod <= ToPeriod)
                        {
                            PayMonth = FromPeriod.Value.Month + "/" + FromPeriod.Value.Year;

                            OEmpSalStruct = OEmployeePayroll.EmpSalStruct.Select(e => e.EmpSalStructDetails.Where(r => r.SalaryHead.Id == SalHd.Id)).FirstOrDefault();
                            foreach (var s in OEmpSalStruct)
                            {
                                TotAmt = TotAmt + s.Amount;
                            }
                            FromPeriod = FromPeriod.Value.AddMonths(1);
                        }
                    }
                }
                return TotAmt;
            }
        }

        public Employee _returnEmployeePayroll(Int32 Emp)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                return db.Employee.Include(e => e.GeoStruct)
                    .Include(e => e.GeoStruct.Company)
                    .Include(e => e.FuncStruct)
                    .Include(e => e.PayStruct)
                    .Include(e => e.ServiceBookDates)
                    .Where(r => r.Id == Emp).SingleOrDefault();
            }
        }

        public EmployeePayroll _returnITInvestmentPayment(Int32 Emp)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                return db.EmployeePayroll.Include(e => e.Employee)
                        .Include(e => e.Employee.EmpOffInfo)
                        .Include(e => e.Employee.EmpOffInfo.NationalityID)
                        .Include(e => e.Employee.Gender)
                          .Include(e => e.Employee.ServiceBookDates)
                           .Include(e => e.Employee.EmpName)
                           .Include(e => e.ITProjection)
                           .Include(e => e.ITProjection.Select(r => r.FinancialYear))
                           .Include(e => e.RegimiScheme).Include(e => e.RegimiScheme.Select(y => y.Scheme))
                        .Where(e => e.Employee.Id == Emp).SingleOrDefault();
            }
        }

        public static Double[] TDSCalc(EmployeePayroll OEmployeePayroll, IncomeTax OITMaster, double mTotalITIncome, DateTime mToPeriod)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string OEmpSex = OEmployeePayroll.Employee.Gender.LookupVal.ToUpper();
                //******* gender look up checking *************//
                //  OEmpSex = "MALE";
                DateTime start = OEmployeePayroll.Employee.ServiceBookDates.BirthDate.Value;
                DateTime end = mToPeriod;
                int compMonth = (end.Month + end.Year * 12) - (start.Month + start.Year * 12);
                // double daysInEndMonth = (end - end.AddMonths(1)).Days;
                double daysInEndMonth = (end.AddMonths(1) - end).Days;
                double months = compMonth + (start.Day - end.Day) / daysInEndMonth;
                double mAge = Math.Abs(months / 12);
                string mCategory = "OTHERS";
                if (OEmpSex != null)
                {
                    if (OEmpSex == "MALE")
                    {
                        if (mAge >= 60)
                        {
                            mCategory = "SENIOR CITIZEN";
                        }
                        else
                        {
                            mCategory = "OTHERS";
                        }
                    }
                    else
                    {
                        if (mAge >= 60)
                        {
                            mCategory = "SENIOR CITIZEN";
                        }
                        else
                        {
                            mCategory = "WOMEN";
                        }
                    }
                }
                // for new tax slab start 28122020
                var FinYear = db.Calendar.Where(e => e.Default == true && e.Name.LookupVal.ToString().ToUpper() == "FINANCIALYEAR").SingleOrDefault();
                Calendar temp_OFinancialYear = db.Calendar.Where(e => e.Id == FinYear.Id).SingleOrDefault();
                DateTime finfrm = Convert.ToDateTime("01/04/2020");
                 var Regimi = db.EmployeePayroll.Include(e => e.RegimiScheme).Include(e => e.RegimiScheme.Select(x => x.FinancialYear)).Include(e => e.RegimiScheme.Select(y => y.Scheme)).Where(e => e.Id == OEmployeePayroll.Id).FirstOrDefault();
                var Regimischemecurryear = Regimi.RegimiScheme.Where(e => e.FinancialYear.Id == temp_OFinancialYear.Id).FirstOrDefault();
                if (Regimischemecurryear != null)
                {
                    if (Regimischemecurryear.Scheme.LookupVal.ToUpper() == "NEWTAX")
                    {
                        mCategory = "2021";
                    }
                }
                else
                {
                    if (temp_OFinancialYear.FromDate >= finfrm && OEmployeePayroll.Employee.EmpOffInfo.NationalityID.No2 == "Yes")
                    {
                        mCategory = "2021";
                    }
                }
                // for new tax slab end 28122020
                List<ITTDS> OITTDS = OITMaster.ITTDS.Where(e => e.Category.LookupVal.ToUpper() == mCategory).OrderBy(r => r.IncomeRangeFrom).ToList();
                List<EmpSalStruct> OEmpSalStruct = db.EmpSalStruct.Include(e => e.EmpSalStructDetails)
                    .Include(e => e.EmpSalStructDetails.Select(r => r.SalaryHead))
                    .Include(e => e.EmpSalStructDetails.Select(r => r.SalaryHead.SalHeadOperationType))
                    .Where(e => e.EmployeePayroll.Id == OEmployeePayroll.Id).ToList();
                ////EmpSalStruct OEmpSal = OEmployeePayroll.EmpSalStruct.Where(e => e.EndDate == null).SingleOrDefault();
                ////if (OEmpSal == null)
                ////{
                ////    OEmpSal = OEmployeePayroll.EmpSalStruct.LastOrDefault();
                ////}

                EmpSalStruct OEmpSal = OEmpSalStruct.Where(e => e.EndDate == null).SingleOrDefault();
                if (OEmpSal == null)
                {
                    OEmpSal = OEmpSalStruct.LastOrDefault();
                }
                EmpSalStructDetails OSalaryHead = OEmpSal.EmpSalStructDetails.Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "ITAX").SingleOrDefault();
                Double[] OTDSDetails = new Double[5];
                if (OSalaryHead == null || mTotalITIncome == 0)
                {
                    //    //define income tax head
                    OTDSDetails[1] = 0;
                    OTDSDetails[2] = 0;
                    OTDSDetails[3] = 0;
                    OTDSDetails[4] = 0;

                    return OTDSDetails;
                }
                double mITTax = 0;

                double[] mPerc = new Double[5];
                mPerc[0] = 0;
                double mITCess = 0;
                double mITSurcharge = 0;
                foreach (ITTDS OITTDSData in OITTDS)
                {
                    OTDSDetails[1] = OITTDSData.EduCessPercent;
                    OTDSDetails[2] = OITTDSData.SurchargePercent;
                    OTDSDetails[3] = OITTDSData.EduCessAmount;
                    OTDSDetails[4] = OITTDSData.SurchargeAmount;

                    if (mTotalITIncome > OITTDSData.IncomeRangeFrom && mTotalITIncome <= OITTDSData.IncomeRangeTo)
                    {
                        double OTDSTempFinal = ((mTotalITIncome - OITTDSData.IncomeRangeFrom) * OITTDSData.Percentage / 100) + OITTDSData.Amount;
                        mITTax = mITTax + OTDSTempFinal;
                        mITTax = Math.Round(mITTax);
                        mPerc[0] = OITTDSData.Percentage;
                        mPerc[1] = OITTDSData.EduCessPercent;

                        break;
                    }
                    else
                    {

                        double OTDSTemp = ((OITTDSData.IncomeRangeTo - OITTDSData.IncomeRangeFrom) * OITTDSData.Percentage / 100) + OITTDSData.Amount;
                        mITTax = mITTax + OTDSTemp;
                        mPerc[0] = OITTDSData.Percentage;
                        mPerc[1] = OITTDSData.EduCessPercent;
                    }
                }
                // OTDSDetails[0] = Process.SalaryHeadGenProcess.RoundingFunction(OSalaryHead.SalaryHead, mITTax);

                // return OTDSDetails;
                return mPerc;
            }
        }

        public static CompanyPayroll _returnCompanyPayroll_IncomeTax1(Int32 mCompanyId)
        {
            //Utility.DumpProcessStatus("_returnCompanyPayroll_IncomeTax");
            using (DataBaseContext db = new DataBaseContext())
            {
                return db.CompanyPayroll
               .Include(e => e.IncomeTax)
               .Include(e => e.IncomeTax.Select(r => r.FyCalendar))
                    //.Include(e => e.IncomeTax.Select(r => r.ITSection))
                    //.Include(e => e.IncomeTax.Select(r => r.ITSection.Select(s => s.ITInvestments)))
                    //.Include(e => e.IncomeTax.Select(r => r.ITSection.Select(s => s.ITInvestments.Select(m => m.ITSubInvestment))))
                    //.Include(e => e.IncomeTax.Select(r => r.ITSection.Select(s => s.ITInvestments.Select(m => m.SalaryHead))))
                    //.Include(e => e.IncomeTax.Select(r => r.ITSection.Select(s => s.LoanAdvanceHead)))
                    //.Include(e => e.IncomeTax.Select(r => r.ITSection.Select(s => s.LoanAdvanceHead.Select(d => d.SalaryHead))))
                    //.Include(e => e.IncomeTax.Select(r => r.ITSection.Select(s => s.LoanAdvanceHead.Select(d => d.ITLoan))))
                    //.Include(e => e.IncomeTax.Select(r => r.ITSection.Select(d => d.ITStandardITRebate)))
                    //.Include(e => e.IncomeTax.Select(r => r.ITSection.Select(d => d.ITSection10)))
                    //.Include(e => e.IncomeTax.Select(r => r.ITSection.Select(d => d.ITSection10.Select(s => s.Itsection10salhead))))
                    //.Include(e => e.IncomeTax.Select(r => r.ITSection.Select(d => d.ITSection10.Select(s => s.Itsection10salhead.Select(c => c.Frequency)))))
                    //.Include(e => e.IncomeTax.Select(r => r.ITSection.Select(d => d.ITSection10.Select(s => s.Itsection10salhead.Select(t => t.SalHead)))))
                    //.Include(e => e.IncomeTax.Select(r => r.ITSection.Select(d => d.ITSectionList)))
                    //.Include(e => e.IncomeTax.Select(r => r.ITSection.Select(d => d.ITSectionListType)))
               .Include(e => e.IncomeTax.Select(r => r.ITTDS))
               .Include(e => e.IncomeTax.Select(r => r.ITTDS.Select(d => d.Category))).Where(e => e.Company.Id == mCompanyId).AsParallel()
               .SingleOrDefault(); ;

                //return a.Where(e => e.Company.Id == mCompanyId).AsParallel()
                //   .SingleOrDefault();
            }
        }

        public class ProcType
        {
            public int Id { get; set; }
            public string Text { get; set; }
        };

        public ActionResult Polulate_ProcTypeChk(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (!String.IsNullOrEmpty(Session["CompId"].ToString()))
                {
                    data = DateTime.Now.ToString("MM/yyyy");
                    List<ProcType> a = new List<ProcType>();

                    a = new List<ProcType>()
                    {
                        new ProcType() { Id = 0, Text = "Actual Investment & Actual Income" }, 
                        new ProcType() { Id = 1, Text = "Declare Investment & Projected Income" },
                        new ProcType() { Id = 2, Text = "Actual Investment & Projected Income" }
                    };


                    SelectList s = new SelectList(a, "Id", "Text");
                    return Json(s, JsonRequestBehavior.AllowGet);
                }
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }

        public static double WagecalcDirectOnSalMonthData(Wages OWagesMaster, List<IncomeTaxCalc.ITSalaryHeadDataTemp> OSalaryEarnDedT)
        {

            double OWages = 0;
            if (OSalaryEarnDedT != null)
            {

                OWages = OWagesMaster.RateMaster
                    .Join(OSalaryEarnDedT, u => u.SalHead.Id, uir => uir.SalaryHead,
                            (u, uir) => new { u, uir })
                    .Select(e => e.u.Percentage / 100 * e.uir.ProjectedAmount).Sum();

                OWages = OWages + OWagesMaster.RateMaster
                    .Join(OSalaryEarnDedT, u => u.SalHead.Id, uir => uir.SalaryHead,
                            (u, uir) => new { u, uir })
                    .Select(e => e.u.Amount).Sum();


                if (OWagesMaster.CeilingMin != null)
                {
                    if (OWages < OWagesMaster.CeilingMin)
                    {
                        OWages = OWagesMaster.CeilingMin;
                    }
                }
                if (OWagesMaster.CeilingMax != null)
                {
                    if (OWages > OWagesMaster.CeilingMax)
                    {
                        OWages = OWagesMaster.CeilingMax;
                    }
                }
                return OWages;
            }

            return 0;
        }

        public static Double[] BonusCalc(int mCompanyPayroll_Id, int mEmployeePayroll_Id, Calendar OBonusCalendar)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                double[] BonusAmt = new Double[5];
                DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };
                var OCompanyPayroll = db.CompanyPayroll
                                    .Include(e => e.BonusAct)
                                    .Include(e => e.BonusAct.Select(r => r.BonusCalendar))
                                    .Include(e => e.BonusAct.Select(r => r.BonusWages))
                                    .Include(e => e.BonusAct.Select(r => r.BonusWages.RateMaster))
                                    .Where(e => e.Id == mCompanyPayroll_Id).SingleOrDefault();

                var OBonusAct = OCompanyPayroll.BonusAct
                                .Where(e => e.BonusCalendar.Id == OBonusCalendar.Id).SingleOrDefault();

                int strid = db.EmployeePayroll
                    .Include(e => e.EmpSalStruct)
                    .Where(e => e.Id == mEmployeePayroll_Id).SingleOrDefault().EmpSalStruct.Select(r => r.Id).LastOrDefault();

                var EmpSalStructlist = db.EmpSalStruct
                    .Include(e => e.EmpSalStructDetails.Select(t => t.SalaryHead.SalHeadOperationType))
                    .Include(e => e.EmpSalStructDetails.Select(t => t.SalaryHead.Type))
                    .Include(e => e.EmpSalStructDetails.Select(t => t.SalaryHead.ProcessType))
                    .Include(e => e.EmpSalStructDetails.Select(t => t.SalaryHead.Frequency))
                    .Where(r => r.Id == strid).SingleOrDefault();


                var OEmployeePayroll = db.EmployeePayroll
                                    .Include(e => e.BonusChkT)
                    //.Include(e => e.ITSalaryHeadData)
                    //.Include(e => e.ITSalaryHeadData.Select(r => r.SalaryHead))
                    //.Include(e => e.EmpSalStruct)
                                    .Include(e => e.SalAttendance)
                    //.Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.SalHeadOperationType)))
                    //.Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.Type)))
                    //.Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.SalHeadOperationType)))
                    //.Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.ProcessType)))
                    //.Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.Frequency)))
                                    .Include(e => e.Employee.EmpOffInfo)
                                    .Include(e => e.Employee.EmpOffInfo.NationalityID)
                                    .Include(e => e.Employee.ServiceBookDates)
                                    .Include(e => e.Employee.EmpName)
                                    .Where(e => e.Id == mEmployeePayroll_Id).SingleOrDefault();


                //var OEmpSalStructChk = OEmployeePayroll.ITSalaryHeadData
                //                .Where(e => Convert.ToDateTime("01/" + e.PayMonth).Date >= OBonusCalendar.FromDate.Value && Convert.ToDateTime("01/" + e.PayMonth).Date <= OBonusCalendar.ToDate.Value).ToList();

                //var OEmpSalStruct = OEmployeePayroll.EmpSalStruct
                //                   //.Where(e => e.EndDate == null).SingleOrDefault();
                //                   .LastOrDefault();
                var OEmpSalDetails = EmpSalStructlist.EmpSalStructDetails.Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "BONUS").SingleOrDefault();




                if (OEmpSalDetails == null)//bonus head present
                {
                    return null;
                }
                if (OBonusAct == null)//bonus act present
                {
                    return null;
                }
                //employee date of joing and birthdate should not be null
                DateTime? mEffectiveDate;

                //existing record deletion
                if (OEmployeePayroll.BonusChkT != null && OEmployeePayroll.BonusChkT.Count() > 0)
                {
                    var OEmpBonusDel = OEmployeePayroll.BonusChkT.SingleOrDefault();

                    db.BonusChkT.Attach(OEmpBonusDel);
                    db.Entry(OEmpBonusDel).State = System.Data.Entity.EntityState.Deleted;
                    db.SaveChanges();

                }
                //both conditions are ok.
                double mGratuity = 0;
                double mTotActService = 0;
                Int32 mTotEffectiveService = 0;
                // List<ITSalaryHeadData> OSalaryHeadData = new List<ITSalaryHeadData>();
                // OSalaryHeadData = IncomeTaxCalc.SalaryHeadMonthData(OEmployeePayroll, DateTime.Now, OBonusCalendar.FromDate.Value, OBonusCalendar.ToDate.Value, null);

                // for yearly payment month period start
                Calendar FinYr = db.Calendar.Where(e => e.Name.LookupVal.ToUpper().ToString() == "FINANCIALYEAR" && e.Default == true).SingleOrDefault();

                DateTime? FromDateyear = db.Calendar.Where(e => e.Id == FinYr.Id)
                                 .Select(e => e.FromDate)
                                .SingleOrDefault();
                DateTime? ToDateyear = db.Calendar.Where(e => e.Id == FinYr.Id)
                              .Select(e => e.ToDate)
                             .SingleOrDefault();
                DateTime FromPeriodyear = Convert.ToDateTime(FromDateyear);
                DateTime ToPeriodyear = Convert.ToDateTime(ToDateyear);

                string mPeriodRangeYear = "";
                List<string> mPeriodYear = new List<string>();
                DateTime mEndDateYear = Convert.ToDateTime("01/" + Convert.ToDateTime(ToPeriodyear).ToString("MM/yyyy")).AddMonths(1).Date;
                mEndDateYear = mEndDateYear.AddDays(-1).Date;
                for (DateTime mTempDate = FromPeriodyear; mTempDate <= mEndDateYear; mTempDate = mTempDate.AddMonths(1))
                {
                    if (mPeriodRangeYear == "")
                    {
                        mPeriodRangeYear = Convert.ToDateTime(mTempDate).ToString("MM/yyyy");
                        mPeriodYear.Add(Convert.ToDateTime(mTempDate).ToString("MM/yyyy"));
                    }
                    else
                    {
                        mPeriodRangeYear = mPeriodRangeYear + "," + Convert.ToDateTime(mTempDate).ToString("MM/yyyy");
                        mPeriodYear.Add(Convert.ToDateTime(mTempDate).ToString("MM/yyyy"));
                    }
                }


                // for yearly payment month period end

                List<IncomeTaxCalc.ITSalaryHeadDataTemp> OSalaryHeadData = new List<IncomeTaxCalc.ITSalaryHeadDataTemp>();
                OSalaryHeadData = IncomeTaxCalc.SalaryHeadMonthData(OEmployeePayroll, DateTime.Now, OBonusCalendar.FromDate.Value, OBonusCalendar.ToDate.Value, null, mPeriodYear, FinYr.Id);

                if (OSalaryHeadData == null || OSalaryHeadData.Count() == 0)
                {
                    return null;
                }
                else
                {
                    BonusChkT OBonusCheckList = new BonusChkT();

                    OBonusCheckList.BonusCalendar = db.Calendar.Find(OBonusCalendar.Id);
                    for (var mdate = OBonusCalendar.FromDate.Value; mdate <= OBonusCalendar.ToDate.Value; mdate = mdate.AddMonths(1))
                    {
                        var OSalForCal = OSalaryHeadData.Where(e => e.PayMonth == mdate.ToString("MM/yyyy")).ToList();
                        if (OSalForCal != null && OSalForCal.Count() > 0)
                        {
                            double mWorkingDays = 0;
                            var OWorkDay = OEmployeePayroll.SalAttendance.Where(e => e.PayMonth == mdate.ToString("MM/yyyy")).SingleOrDefault();
                            mWorkingDays = OWorkDay == null ? 0 : (OWorkDay.PaybleDays + OWorkDay.ArrearDays);

                            switch (mdate.ToString("MM"))
                            {
                                case "01":
                                    OBonusCheckList.WorkingDays_01 = mWorkingDays;
                                    OBonusCheckList.BonusWages_01 = WagecalcDirectOnSalMonthData(OBonusAct.BonusWages, OSalForCal);
                                    //bonus act application
                                    if (OBonusAct.ApplicationForBonus == false)
                                    {
                                        OBonusCheckList.Bonus_01 = Math.Round(OBonusCheckList.BonusWages_01 * OBonusAct.MaxPercentage / 100, 2);
                                        OBonusCheckList.ExGracia_01 = 0;
                                        OBonusCheckList.TotalAmount_01 = OBonusCheckList.Bonus_01;
                                    }
                                    else if (OBonusCheckList.BonusWages_01 <= OBonusAct.QualiAmount)
                                    {
                                        OBonusCheckList.Bonus_01 = Math.Round(OBonusCheckList.BonusWages_01 * OBonusAct.MinPercentage / 100, 2);
                                        OBonusCheckList.ExGracia_01 = Math.Round(OBonusCheckList.BonusWages_01 * OBonusAct.MaxPercentage / 100, 2) - OBonusCheckList.Bonus_01;
                                        OBonusCheckList.TotalAmount_01 = OBonusCheckList.Bonus_01 + OBonusCheckList.ExGracia_01;
                                    }
                                    else if (OBonusAct.WantToGiveExgratia == true)
                                    {
                                        OBonusCheckList.Bonus_01 = 0;
                                        OBonusCheckList.ExGracia_01 = Math.Round(OBonusCheckList.BonusWages_01 * OBonusAct.MaxPercentage / 100, 2) - OBonusCheckList.Bonus_01;
                                        OBonusCheckList.TotalAmount_01 = OBonusCheckList.Bonus_01 + OBonusCheckList.ExGracia_01;
                                    }
                                    else
                                    {
                                        OBonusCheckList.Bonus_01 = 0;
                                        OBonusCheckList.ExGracia_01 = 0;
                                        OBonusCheckList.TotalAmount_01 = 0;
                                    }
                                    break;
                                case "02":
                                    OBonusCheckList.WorkingDays_02 = mWorkingDays;
                                    OBonusCheckList.BonusWages_02 = WagecalcDirectOnSalMonthData(OBonusAct.BonusWages, OSalForCal);
                                    //bonus act application
                                    if (OBonusAct.ApplicationForBonus == false)
                                    {
                                        OBonusCheckList.Bonus_02 = Math.Round(OBonusCheckList.BonusWages_02 * OBonusAct.MaxPercentage / 100, 2);
                                        OBonusCheckList.ExGracia_02 = 0;
                                        OBonusCheckList.TotalAmount_02 = OBonusCheckList.Bonus_02;
                                    }
                                    else if (OBonusCheckList.BonusWages_02 <= OBonusAct.QualiAmount)
                                    {
                                        OBonusCheckList.Bonus_02 = Math.Round(OBonusCheckList.BonusWages_02 * OBonusAct.MinPercentage / 100, 2);
                                        OBonusCheckList.ExGracia_02 = Math.Round(OBonusCheckList.BonusWages_02 * OBonusAct.MaxPercentage / 100, 2) - OBonusCheckList.Bonus_02;
                                        OBonusCheckList.TotalAmount_02 = OBonusCheckList.Bonus_02 + OBonusCheckList.ExGracia_02;
                                    }
                                    else if (OBonusAct.WantToGiveExgratia == true)
                                    {
                                        OBonusCheckList.Bonus_02 = 0;
                                        OBonusCheckList.ExGracia_02 = Math.Round(OBonusCheckList.BonusWages_02 * OBonusAct.MaxPercentage / 100, 2) - OBonusCheckList.Bonus_02;
                                        OBonusCheckList.TotalAmount_02 = OBonusCheckList.Bonus_02 + OBonusCheckList.ExGracia_02;
                                    }
                                    else
                                    {
                                        OBonusCheckList.Bonus_02 = 0;
                                        OBonusCheckList.ExGracia_02 = 0;
                                        OBonusCheckList.TotalAmount_02 = 0;
                                    }
                                    break;
                                case "03":
                                    OBonusCheckList.WorkingDays_03 = mWorkingDays;
                                    OBonusCheckList.BonusWages_03 = WagecalcDirectOnSalMonthData(OBonusAct.BonusWages, OSalForCal);
                                    //bonus act application
                                    if (OBonusAct.ApplicationForBonus == false)
                                    {
                                        OBonusCheckList.Bonus_03 = Math.Round(OBonusCheckList.BonusWages_03 * OBonusAct.MaxPercentage / 100, 2);
                                        OBonusCheckList.ExGracia_03 = 0;
                                        OBonusCheckList.TotalAmount_03 = OBonusCheckList.Bonus_03;
                                    }
                                    else if (OBonusCheckList.BonusWages_03 <= OBonusAct.QualiAmount)
                                    {
                                        OBonusCheckList.Bonus_03 = Math.Round(OBonusCheckList.BonusWages_03 * OBonusAct.MinPercentage / 100, 2);
                                        OBonusCheckList.ExGracia_03 = Math.Round(OBonusCheckList.BonusWages_03 * OBonusAct.MaxPercentage / 100, 2) - OBonusCheckList.Bonus_03;
                                        OBonusCheckList.TotalAmount_03 = OBonusCheckList.Bonus_03 + OBonusCheckList.ExGracia_03;
                                    }
                                    else if (OBonusAct.WantToGiveExgratia == true)
                                    {
                                        OBonusCheckList.Bonus_03 = 0;
                                        OBonusCheckList.ExGracia_03 = Math.Round(OBonusCheckList.BonusWages_03 * OBonusAct.MaxPercentage / 100, 2) - OBonusCheckList.Bonus_03;
                                        OBonusCheckList.TotalAmount_03 = OBonusCheckList.Bonus_03 + OBonusCheckList.ExGracia_03;
                                    }
                                    else
                                    {
                                        OBonusCheckList.Bonus_03 = 0;
                                        OBonusCheckList.ExGracia_03 = 0;
                                        OBonusCheckList.TotalAmount_03 = 0;
                                    }
                                    break;
                                case "04":
                                    OBonusCheckList.WorkingDays_04 = mWorkingDays;
                                    OBonusCheckList.BonusWages_04 = WagecalcDirectOnSalMonthData(OBonusAct.BonusWages, OSalForCal);
                                    //bonus act application
                                    if (OBonusAct.ApplicationForBonus == false)
                                    {
                                        OBonusCheckList.Bonus_04 = Math.Round(OBonusCheckList.BonusWages_04 * OBonusAct.MaxPercentage / 100, 2);
                                        OBonusCheckList.ExGracia_04 = 0;
                                        OBonusCheckList.TotalAmount_04 = OBonusCheckList.Bonus_04;
                                    }
                                    else if (OBonusCheckList.BonusWages_04 <= OBonusAct.QualiAmount)
                                    {
                                        OBonusCheckList.Bonus_04 = Math.Round(OBonusCheckList.BonusWages_04 * OBonusAct.MinPercentage / 100, 2);
                                        OBonusCheckList.ExGracia_04 = Math.Round(OBonusCheckList.BonusWages_04 * OBonusAct.MaxPercentage / 100, 2) - OBonusCheckList.Bonus_04;
                                        OBonusCheckList.TotalAmount_04 = OBonusCheckList.Bonus_04 + OBonusCheckList.ExGracia_04;
                                    }
                                    else if (OBonusAct.WantToGiveExgratia == true)
                                    {
                                        OBonusCheckList.Bonus_04 = 0;
                                        OBonusCheckList.ExGracia_04 = Math.Round(OBonusCheckList.BonusWages_04 * OBonusAct.MaxPercentage / 100, 2) - OBonusCheckList.Bonus_04;
                                        OBonusCheckList.TotalAmount_04 = OBonusCheckList.Bonus_04 + OBonusCheckList.ExGracia_04;
                                    }
                                    else
                                    {
                                        OBonusCheckList.Bonus_04 = 0;
                                        OBonusCheckList.ExGracia_04 = 0;
                                        OBonusCheckList.TotalAmount_04 = 0;
                                    }
                                    break;
                                case "05":
                                    OBonusCheckList.WorkingDays_05 = mWorkingDays;
                                    OBonusCheckList.BonusWages_05 = WagecalcDirectOnSalMonthData(OBonusAct.BonusWages, OSalForCal);
                                    //bonus act application
                                    if (OBonusAct.ApplicationForBonus == false)
                                    {
                                        OBonusCheckList.Bonus_05 = Math.Round(OBonusCheckList.BonusWages_05 * OBonusAct.MaxPercentage / 100, 2);
                                        OBonusCheckList.ExGracia_05 = 0;
                                        OBonusCheckList.TotalAmount_05 = OBonusCheckList.Bonus_05;
                                    }
                                    else if (OBonusCheckList.BonusWages_05 <= OBonusAct.QualiAmount)
                                    {
                                        OBonusCheckList.Bonus_05 = Math.Round(OBonusCheckList.BonusWages_05 * OBonusAct.MinPercentage / 100, 2);
                                        OBonusCheckList.ExGracia_05 = Math.Round(OBonusCheckList.BonusWages_05 * OBonusAct.MaxPercentage / 100, 2) - OBonusCheckList.Bonus_05;
                                        OBonusCheckList.TotalAmount_05 = OBonusCheckList.Bonus_05 + OBonusCheckList.ExGracia_05;
                                    }
                                    else if (OBonusAct.WantToGiveExgratia == true)
                                    {
                                        OBonusCheckList.Bonus_05 = 0;
                                        OBonusCheckList.ExGracia_05 = Math.Round(OBonusCheckList.BonusWages_05 * OBonusAct.MaxPercentage / 100, 2) - OBonusCheckList.Bonus_05;
                                        OBonusCheckList.TotalAmount_05 = OBonusCheckList.Bonus_05 + OBonusCheckList.ExGracia_05;
                                    }
                                    else
                                    {
                                        OBonusCheckList.Bonus_05 = 0;
                                        OBonusCheckList.ExGracia_05 = 0;
                                        OBonusCheckList.TotalAmount_05 = 0;
                                    }
                                    break;
                                case "06":
                                    OBonusCheckList.WorkingDays_06 = mWorkingDays;
                                    OBonusCheckList.BonusWages_06 = WagecalcDirectOnSalMonthData(OBonusAct.BonusWages, OSalForCal);
                                    //bonus act application
                                    if (OBonusAct.ApplicationForBonus == false)
                                    {
                                        OBonusCheckList.Bonus_06 = Math.Round(OBonusCheckList.BonusWages_06 * OBonusAct.MaxPercentage / 100, 2);
                                        OBonusCheckList.ExGracia_06 = 0;
                                        OBonusCheckList.TotalAmount_06 = OBonusCheckList.Bonus_06;
                                    }
                                    else if (OBonusCheckList.BonusWages_06 <= OBonusAct.QualiAmount)
                                    {
                                        OBonusCheckList.Bonus_06 = Math.Round(OBonusCheckList.BonusWages_06 * OBonusAct.MinPercentage / 100, 2);
                                        OBonusCheckList.ExGracia_06 = Math.Round(OBonusCheckList.BonusWages_06 * OBonusAct.MaxPercentage / 100, 2) - OBonusCheckList.Bonus_06;
                                        OBonusCheckList.TotalAmount_06 = OBonusCheckList.Bonus_06 + OBonusCheckList.ExGracia_06;
                                    }
                                    else if (OBonusAct.WantToGiveExgratia == true)
                                    {
                                        OBonusCheckList.Bonus_06 = 0;
                                        OBonusCheckList.ExGracia_06 = Math.Round(OBonusCheckList.BonusWages_06 * OBonusAct.MaxPercentage / 100, 2) - OBonusCheckList.Bonus_06;
                                        OBonusCheckList.TotalAmount_06 = OBonusCheckList.Bonus_06 + OBonusCheckList.ExGracia_06;
                                    }
                                    else
                                    {
                                        OBonusCheckList.Bonus_06 = 0;
                                        OBonusCheckList.ExGracia_06 = 0;
                                        OBonusCheckList.TotalAmount_06 = 0;
                                    }
                                    break;
                                case "07":
                                    OBonusCheckList.WorkingDays_07 = mWorkingDays;
                                    OBonusCheckList.BonusWages_07 = WagecalcDirectOnSalMonthData(OBonusAct.BonusWages, OSalForCal);
                                    //bonus act application
                                    if (OBonusAct.ApplicationForBonus == false)
                                    {
                                        OBonusCheckList.Bonus_07 = Math.Round(OBonusCheckList.BonusWages_07 * OBonusAct.MaxPercentage / 100, 2);
                                        OBonusCheckList.ExGracia_07 = 0;
                                        OBonusCheckList.TotalAmount_07 = OBonusCheckList.Bonus_07;
                                    }
                                    else if (OBonusCheckList.BonusWages_07 <= OBonusAct.QualiAmount)
                                    {
                                        OBonusCheckList.Bonus_07 = Math.Round(OBonusCheckList.BonusWages_07 * OBonusAct.MinPercentage / 100, 2);
                                        OBonusCheckList.ExGracia_07 = Math.Round(OBonusCheckList.BonusWages_07 * OBonusAct.MaxPercentage / 100, 2) - OBonusCheckList.Bonus_07;
                                        OBonusCheckList.TotalAmount_07 = OBonusCheckList.Bonus_07 + OBonusCheckList.ExGracia_07;
                                    }
                                    else if (OBonusAct.WantToGiveExgratia == true)
                                    {
                                        OBonusCheckList.Bonus_07 = 0;
                                        OBonusCheckList.ExGracia_07 = Math.Round(OBonusCheckList.BonusWages_07 * OBonusAct.MaxPercentage / 100, 2) - OBonusCheckList.Bonus_07;
                                        OBonusCheckList.TotalAmount_07 = OBonusCheckList.Bonus_07 + OBonusCheckList.ExGracia_07;
                                    }
                                    else
                                    {
                                        OBonusCheckList.Bonus_07 = 0;
                                        OBonusCheckList.ExGracia_07 = 0;
                                        OBonusCheckList.TotalAmount_07 = 0;
                                    }
                                    break;
                                case "08":
                                    OBonusCheckList.WorkingDays_08 = mWorkingDays;
                                    OBonusCheckList.BonusWages_08 = WagecalcDirectOnSalMonthData(OBonusAct.BonusWages, OSalForCal);
                                    //bonus act application
                                    if (OBonusAct.ApplicationForBonus == false)
                                    {
                                        OBonusCheckList.Bonus_08 = Math.Round(OBonusCheckList.BonusWages_08 * OBonusAct.MaxPercentage / 100, 2);
                                        OBonusCheckList.ExGracia_08 = 0;
                                        OBonusCheckList.TotalAmount_08 = OBonusCheckList.Bonus_08;
                                    }
                                    else if (OBonusCheckList.BonusWages_08 <= OBonusAct.QualiAmount)
                                    {
                                        OBonusCheckList.Bonus_08 = Math.Round(OBonusCheckList.BonusWages_08 * OBonusAct.MinPercentage / 100, 2);
                                        OBonusCheckList.ExGracia_08 = Math.Round(OBonusCheckList.BonusWages_08 * OBonusAct.MaxPercentage / 100, 2) - OBonusCheckList.Bonus_08;
                                        OBonusCheckList.TotalAmount_08 = OBonusCheckList.Bonus_08 + OBonusCheckList.ExGracia_08;
                                    }
                                    else if (OBonusAct.WantToGiveExgratia == true)
                                    {
                                        OBonusCheckList.Bonus_08 = 0;
                                        OBonusCheckList.ExGracia_08 = Math.Round(OBonusCheckList.BonusWages_08 * OBonusAct.MaxPercentage / 100, 2) - OBonusCheckList.Bonus_08;
                                        OBonusCheckList.TotalAmount_08 = OBonusCheckList.Bonus_08 + OBonusCheckList.ExGracia_08;
                                    }
                                    else
                                    {
                                        OBonusCheckList.Bonus_08 = 0;
                                        OBonusCheckList.ExGracia_08 = 0;
                                        OBonusCheckList.TotalAmount_08 = 0;
                                    }
                                    break;
                                case "09":
                                    OBonusCheckList.WorkingDays_09 = mWorkingDays;
                                    OBonusCheckList.BonusWages_09 = WagecalcDirectOnSalMonthData(OBonusAct.BonusWages, OSalForCal);
                                    //bonus act application
                                    if (OBonusAct.ApplicationForBonus == false)
                                    {
                                        OBonusCheckList.Bonus_09 = Math.Round(OBonusCheckList.BonusWages_09 * OBonusAct.MaxPercentage / 100, 2);
                                        OBonusCheckList.ExGracia_09 = 0;
                                        OBonusCheckList.TotalAmount_09 = OBonusCheckList.Bonus_09;
                                    }
                                    else if (OBonusCheckList.BonusWages_09 <= OBonusAct.QualiAmount)
                                    {
                                        OBonusCheckList.Bonus_09 = Math.Round(OBonusCheckList.BonusWages_09 * OBonusAct.MinPercentage / 100, 2);
                                        OBonusCheckList.ExGracia_09 = Math.Round(OBonusCheckList.BonusWages_09 * OBonusAct.MaxPercentage / 100, 2) - OBonusCheckList.Bonus_09;
                                        OBonusCheckList.TotalAmount_09 = OBonusCheckList.Bonus_09 + OBonusCheckList.ExGracia_09;
                                    }
                                    else if (OBonusAct.WantToGiveExgratia == true)
                                    {
                                        OBonusCheckList.Bonus_09 = 0;
                                        OBonusCheckList.ExGracia_09 = Math.Round(OBonusCheckList.BonusWages_09 * OBonusAct.MaxPercentage / 100, 2) - OBonusCheckList.Bonus_09;
                                        OBonusCheckList.TotalAmount_09 = OBonusCheckList.Bonus_09 + OBonusCheckList.ExGracia_09;
                                    }
                                    else
                                    {
                                        OBonusCheckList.Bonus_09 = 0;
                                        OBonusCheckList.ExGracia_09 = 0;
                                        OBonusCheckList.TotalAmount_09 = 0;
                                    }
                                    break;
                                case "10":
                                    OBonusCheckList.WorkingDays_10 = mWorkingDays;
                                    OBonusCheckList.BonusWages_10 = WagecalcDirectOnSalMonthData(OBonusAct.BonusWages, OSalForCal);
                                    //bonus act application
                                    if (OBonusAct.ApplicationForBonus == false)
                                    {
                                        OBonusCheckList.Bonus_10 = Math.Round(OBonusCheckList.BonusWages_10 * OBonusAct.MaxPercentage / 100, 2);
                                        OBonusCheckList.ExGracia_10 = 0;
                                        OBonusCheckList.TotalAmount_10 = OBonusCheckList.Bonus_10;
                                    }
                                    else if (OBonusCheckList.BonusWages_10 <= OBonusAct.QualiAmount)
                                    {
                                        OBonusCheckList.Bonus_10 = Math.Round(OBonusCheckList.BonusWages_10 * OBonusAct.MinPercentage / 100, 2);
                                        OBonusCheckList.ExGracia_10 = Math.Round(OBonusCheckList.BonusWages_10 * OBonusAct.MaxPercentage / 100, 2) - OBonusCheckList.Bonus_10;
                                        OBonusCheckList.TotalAmount_10 = OBonusCheckList.Bonus_10 + OBonusCheckList.ExGracia_10;
                                    }
                                    else if (OBonusAct.WantToGiveExgratia == true)
                                    {
                                        OBonusCheckList.Bonus_10 = 0;
                                        OBonusCheckList.ExGracia_10 = Math.Round(OBonusCheckList.BonusWages_10 * OBonusAct.MaxPercentage / 100, 2) - OBonusCheckList.Bonus_10;
                                        OBonusCheckList.TotalAmount_10 = OBonusCheckList.Bonus_10 + OBonusCheckList.ExGracia_10;
                                    }
                                    else
                                    {
                                        OBonusCheckList.Bonus_10 = 0;
                                        OBonusCheckList.ExGracia_10 = 0;
                                        OBonusCheckList.TotalAmount_10 = 0;
                                    }
                                    break;
                                case "11":
                                    OBonusCheckList.WorkingDays_11 = mWorkingDays;
                                    OBonusCheckList.BonusWages_11 = WagecalcDirectOnSalMonthData(OBonusAct.BonusWages, OSalForCal);
                                    //bonus act application
                                    if (OBonusAct.ApplicationForBonus == false)
                                    {
                                        OBonusCheckList.Bonus_11 = Math.Round(OBonusCheckList.BonusWages_11 * OBonusAct.MaxPercentage / 100, 2);
                                        OBonusCheckList.ExGracia_11 = 0;
                                        OBonusCheckList.TotalAmount_11 = OBonusCheckList.Bonus_11;
                                    }
                                    else if (OBonusCheckList.BonusWages_11 <= OBonusAct.QualiAmount)
                                    {
                                        OBonusCheckList.Bonus_11 = Math.Round(OBonusCheckList.BonusWages_11 * OBonusAct.MinPercentage / 100, 2);
                                        OBonusCheckList.ExGracia_11 = Math.Round(OBonusCheckList.BonusWages_11 * OBonusAct.MaxPercentage / 100, 2) - OBonusCheckList.Bonus_11;
                                        OBonusCheckList.TotalAmount_11 = OBonusCheckList.Bonus_11 + OBonusCheckList.ExGracia_11;
                                    }
                                    else if (OBonusAct.WantToGiveExgratia == true)
                                    {
                                        OBonusCheckList.Bonus_11 = 0;
                                        OBonusCheckList.ExGracia_11 = Math.Round(OBonusCheckList.BonusWages_11 * OBonusAct.MaxPercentage / 100, 2) - OBonusCheckList.Bonus_11;
                                        OBonusCheckList.TotalAmount_11 = OBonusCheckList.Bonus_11 + OBonusCheckList.ExGracia_11;
                                    }
                                    else
                                    {
                                        OBonusCheckList.Bonus_11 = 0;
                                        OBonusCheckList.ExGracia_11 = 0;
                                        OBonusCheckList.TotalAmount_11 = 0;
                                    }
                                    break;
                                case "12":
                                    OBonusCheckList.WorkingDays_12 = mWorkingDays;
                                    OBonusCheckList.BonusWages_12 = WagecalcDirectOnSalMonthData(OBonusAct.BonusWages, OSalForCal);
                                    //bonus act application
                                    if (OBonusAct.ApplicationForBonus == false)
                                    {
                                        OBonusCheckList.Bonus_12 = Math.Round(OBonusCheckList.BonusWages_12 * OBonusAct.MaxPercentage / 100, 2);
                                        OBonusCheckList.ExGracia_12 = 0;
                                        OBonusCheckList.TotalAmount_12 = OBonusCheckList.Bonus_12;
                                    }
                                    else if (OBonusCheckList.BonusWages_12 <= OBonusAct.QualiAmount)
                                    {
                                        OBonusCheckList.Bonus_12 = Math.Round(OBonusCheckList.BonusWages_12 * OBonusAct.MinPercentage / 100, 2);
                                        OBonusCheckList.ExGracia_12 = Math.Round(OBonusCheckList.BonusWages_12 * OBonusAct.MaxPercentage / 100, 2) - OBonusCheckList.Bonus_12;
                                        OBonusCheckList.TotalAmount_12 = OBonusCheckList.Bonus_12 + OBonusCheckList.ExGracia_12;
                                    }
                                    else if (OBonusAct.WantToGiveExgratia == true)
                                    {
                                        OBonusCheckList.Bonus_12 = 0;
                                        OBonusCheckList.ExGracia_12 = Math.Round(OBonusCheckList.BonusWages_12 * OBonusAct.MaxPercentage / 100, 2) - OBonusCheckList.Bonus_12;
                                        OBonusCheckList.TotalAmount_12 = OBonusCheckList.Bonus_12 + OBonusCheckList.ExGracia_12;
                                    }
                                    else
                                    {
                                        OBonusCheckList.Bonus_12 = 0;
                                        OBonusCheckList.ExGracia_12 = 0;
                                        OBonusCheckList.TotalAmount_12 = 0;
                                    }
                                    break;
                            }

                        }
                        //else
                        //{
                        //    OBonusCheckList.Bonus_01 = 0;
                        //    OBonusCheckList.ExGracia_01 = 0;
                        //    OBonusCheckList.TotalAmount_01 = 0;
                        //}
                    }//for loop
                    OBonusCheckList.TotalAmount = Math.Round(OBonusCheckList.TotalAmount_01 + OBonusCheckList.TotalAmount_02 + OBonusCheckList.TotalAmount_03
                                            + OBonusCheckList.TotalAmount_04 + OBonusCheckList.TotalAmount_05 + OBonusCheckList.TotalAmount_06
                                            + OBonusCheckList.TotalAmount_07 + OBonusCheckList.TotalAmount_08 + OBonusCheckList.TotalAmount_09
                                            + OBonusCheckList.TotalAmount_10 + OBonusCheckList.TotalAmount_11 + OBonusCheckList.TotalAmount_12, 0);

                    OBonusCheckList.TotalBonus = Math.Round(OBonusCheckList.Bonus_01 + OBonusCheckList.Bonus_02 + OBonusCheckList.Bonus_03
                                            + OBonusCheckList.Bonus_04 + OBonusCheckList.Bonus_05 + OBonusCheckList.Bonus_06
                                            + OBonusCheckList.Bonus_07 + OBonusCheckList.Bonus_08 + OBonusCheckList.Bonus_09
                                            + OBonusCheckList.Bonus_10 + OBonusCheckList.Bonus_11 + OBonusCheckList.Bonus_12, 0);

                    OBonusCheckList.TotalBonusWages = Math.Round(OBonusCheckList.BonusWages_01 + OBonusCheckList.BonusWages_02 + OBonusCheckList.BonusWages_03
                                            + OBonusCheckList.BonusWages_04 + OBonusCheckList.BonusWages_05 + OBonusCheckList.BonusWages_06
                                            + OBonusCheckList.BonusWages_07 + OBonusCheckList.BonusWages_08 + OBonusCheckList.BonusWages_09
                                            + OBonusCheckList.BonusWages_10 + OBonusCheckList.BonusWages_11 + OBonusCheckList.BonusWages_12, 0);

                    OBonusCheckList.TotalExGracia = Math.Round(OBonusCheckList.ExGracia_01 + OBonusCheckList.ExGracia_02 + OBonusCheckList.ExGracia_03
                                            + OBonusCheckList.ExGracia_04 + OBonusCheckList.ExGracia_05 + OBonusCheckList.ExGracia_06
                                            + OBonusCheckList.ExGracia_07 + OBonusCheckList.ExGracia_08 + OBonusCheckList.ExGracia_09
                                            + OBonusCheckList.ExGracia_10 + OBonusCheckList.ExGracia_11 + OBonusCheckList.ExGracia_12, 0);

                    OBonusCheckList.TotalWorkingDays = Math.Round(OBonusCheckList.WorkingDays_01 + OBonusCheckList.WorkingDays_02 + OBonusCheckList.WorkingDays_03
                                            + OBonusCheckList.WorkingDays_04 + OBonusCheckList.WorkingDays_05 + OBonusCheckList.WorkingDays_06
                                            + OBonusCheckList.WorkingDays_07 + OBonusCheckList.WorkingDays_08 + OBonusCheckList.WorkingDays_09
                                            + OBonusCheckList.WorkingDays_10 + OBonusCheckList.WorkingDays_11 + OBonusCheckList.WorkingDays_12, 0);
                    //minimum working days
                    if (OBonusCheckList.TotalWorkingDays < OBonusAct.MinimumWorkingDays)
                    {
                        OBonusCheckList.TotalAmount = 0;
                        OBonusCheckList.TotalBonus = 0;
                        OBonusCheckList.TotalExGracia = 0;

                    }
                    //minimum bonus amount
                    if (OBonusCheckList.TotalBonus < OBonusAct.MinimumBonusAmount)
                    {
                        OBonusCheckList.TotalBonus = OBonusAct.MinimumBonusAmount;

                    }
                    BonusAmt[0] = OBonusCheckList.TotalBonus;
                    BonusAmt[1] = OBonusCheckList.TotalExGracia;
                }

                return BonusAmt;
            }
        }

    }
}