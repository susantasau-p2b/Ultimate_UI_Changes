using P2b.Global;
using P2BUltimate.Models;
using Payroll;
using Leave;
using EMS;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Web.Script.Serialization;
using System.Collections;
using System.Data.Entity.Core.Objects;
using System.Reflection;
using System.Diagnostics;
using System.Threading.Tasks;
using P2BUltimate.App_Start;
using P2BUltimate.Security;
using Newtonsoft.Json;
using P2BUltimate.Process;
using P2B.SERVICES.Factory;
using P2B.SERVICES.Interface;

namespace P2BUltimate.Controllers.Payroll.MainController
{
    [AuthoriseManger]
    public class FFSSettlementDetailTController : Controller
    {
        readonly IP2BINI p2BINI;
        readonly LeaveEncashExemptionSettings LeaveEncashExemptionSettings;
         private readonly Type Type;

         public FFSSettlementDetailTController()
        {
            p2BINI = P2BINI.RegisterSettings();
            LeaveEncashExemptionSettings = new LeaveEncashExemptionSettings(p2BINI.GetSectionValues("LeaveEncashExemptionSettings")); 
            Type = typeof(FFSSettlementReleaseDetailTController);
           
        }

        List<String> Msg = new List<string>();
        //
        // GET: /FFSSettlementDetailT/
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/FFSSettlementDetailT/Index.cshtml");
        }

        public ActionResult PARTIAL()
        {
            return View("~/Views/Shared/Payroll/_LvEncashExemptDetails.cshtml");
        }
        public class P2BGridData
        {
            public int Id { get; set; }
            public string EmpCode { get; set; }
            public string EmpName { get; set; }
            public string Salcode { get; set; }
            public string SalType { get; set; }
            public double PayAmount { get; set; }
            public string PayMonth { get; set; }
            public string LvHeadCode { get; set; }
            public string Paydate { get; set; }


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
                IEnumerable<P2BGridData> SalaryList = null;
                List<P2BGridData> model = new List<P2BGridData>();
                P2BGridData view = null;

                //   IEnumerable<NoticePeriodProcess> Corporate = null;

                var empresig = db.EmployeeExit.Include(e => e.Employee.EmpName).Include(e => e.SeperationProcessT)
                    .Include(e => e.SeperationProcessT.FFSSettlementDetailT)
                    .Include(e => e.SeperationProcessT.FFSSettlementDetailT.Select(r=>r.LvEncashReq))
                    .Include(e => e.SeperationProcessT.FFSSettlementDetailT.Select(r => r.LvEncashReq.LvHead))
                    .Include(e => e.SeperationProcessT.FFSSettlementDetailT.Select(x => x.ProcessType))
                    .AsNoTracking().ToList();

                foreach (var z in empresig)
                {
                    if (z.SeperationProcessT != null && z.SeperationProcessT.FFSSettlementDetailT != null)
                    {
                        foreach (var item in z.SeperationProcessT.FFSSettlementDetailT)
                        {
                            if (item.ProcessType.LookupVal.ToUpper() == "BENEFIT PROCESS")
                            {
                                view = new P2BGridData()
                                {
                                    Id = item.Id, 
                                    EmpCode = z.Employee.EmpCode,
                                    EmpName = z.Employee.EmpName.FullNameFML,
                                    Salcode = item.SalaryHead,
                                    SalType = item.SalType,
                                    PayAmount = item.PayAmount,
                                    PayMonth = item.PayMonth.Value.Date.ToShortDateString(),
                                    LvHeadCode = item.LvEncashReq==null ? "": item.LvEncashReq.LvHead == null ? "" : item.LvEncashReq.LvHead.LvCode,
                                    Paydate = item.PayDate.Value.Date.ToShortDateString(),

                                };

                                model.Add(view);
                            }
                        }
                    }
                }

                SalaryList = model;

                IEnumerable<P2BGridData> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = SalaryList;
                    if (gp.searchOper.Equals("eq"))
                    {

                        //jsonData = IE.Where(e => (e.Narration.ToUpper().ToString().Contains(gp.searchString.ToUpper()))                             
                        //      || (e.Id.ToString().Contains(gp.searchString))
                        //      ).Select(a => new Object[] { a.Narration, a.Id }).ToList();
                        jsonData = IE.Where(e => (e.EmpCode.ToString().Contains(gp.searchString))
                             || (e.EmpName.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                             || (e.LvHeadCode.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                              || (e.Salcode != null ? e.Salcode.ToString().Contains(gp.searchString) : false)
                             || (e.SalType != null ? e.SalType.ToString().Contains(gp.searchString) : false)
                              || (e.PayAmount != null ? e.PayAmount.ToString().Contains(gp.searchString) : false)
                               || (e.PayMonth != null ? e.PayMonth.ToString().Contains(gp.searchString) : false)
                            //|| (e.ProcessMonth != null ? e.ProcessMonth.ToString().Contains(gp.searchString) : false)
                               || (e.Paydate != null ? e.Paydate.ToString().Contains(gp.searchString) : false)

                             || (e.Id.ToString().Contains(gp.searchString))
                             ).Select(a => new Object[] { a.EmpCode, a.EmpName,a.LvHeadCode, a.Salcode, a.SalType, a.PayAmount, a.PayMonth, a.Paydate != null ? Convert.ToString(a.Paydate) : "", a.Id }).ToList();



                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.EmpCode, a.EmpName,a.LvHeadCode, a.Salcode, a.SalType, a.PayAmount, a.PayMonth, a.Paydate != null ? Convert.ToString(a.Paydate) : "", a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = SalaryList;
                    Func<P2BGridData, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "EmpCode" ? c.EmpCode.ToString() :
                                         gp.sidx == "EmpName" ? c.EmpName.ToString() :
                                         gp.sidx == "LvHeadCode" ? c.LvHeadCode.ToString() :
                                          gp.sidx == "Salcode" ? c.Salcode.ToString() :
                                           gp.sidx == "SalType" ? c.SalType.ToString() :
                                            gp.sidx == "PayAmount" ? c.PayAmount.ToString() :
                                             gp.sidx == "PayMonth" ? c.PayMonth.ToString() :

                                         gp.sidx == "Paydate" ? c.Paydate.ToString() :


                                    "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.EmpCode, a.EmpName, a.LvHeadCode,a.Salcode, a.SalType, a.PayAmount, a.PayMonth, a.Paydate != null ? Convert.ToString(a.Paydate) : "", a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.EmpCode, a.EmpName,a.LvHeadCode, a.Salcode, a.SalType, a.PayAmount, a.PayMonth, a.Paydate != null ? Convert.ToString(a.Paydate) : "", a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.EmpCode, a.EmpName, a.LvHeadCode,a.Salcode, a.SalType, a.PayAmount, a.PayMonth, a.Paydate != null ? Convert.ToString(a.Paydate) : "", a.Id }).ToList();
                    }
                    totalRecords = SalaryList.Count();
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
        public static string[] Annualpayment(int OEmployeeId, string PayMonth, string Paymentdate, string fromdate, string Todate, int Salheadid, YearlyPaymentT L)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string[] Msg = new string[2];
                try
                {
                    YearlyPaymentT Y = new YearlyPaymentT();

                    var fyyr = db.Calendar.Include(a => a.Name).Where(a => a.Name.LookupVal.ToUpper() == "FINANCIALYEAR" && a.Default == true).SingleOrDefault();
                    Y.FinancialYear = fyyr;

                    var Val = db.SalaryHead.Find((Salheadid));
                    Y.SalaryHead = Val;
                    Y.FromPeriod = DateTime.Parse(fromdate);
                    Y.ToPeriod = DateTime.Parse(Todate);
                    Y.ProcessMonth = PayMonth;
                    Y.AmountPaid = 0;
                    Y.TDSAmount = 0;
                    Y.OtherDeduction = 0;
                    Y.LvEncashReq = null;
                    Y.Narration = "Settlement Process";

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

                    if (OEmployeeId != null)
                    {

                        OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.GeoStruct.Company).Include(e => e.FuncStruct).Include(e => e.PayStruct)
                             .Include(e => e.ServiceBookDates)
                               .Where(r => r.Id == OEmployeeId).SingleOrDefault();

                        OEmployeePayroll = db.EmployeePayroll
                      .Where(e => e.Employee.Id == OEmployeeId).SingleOrDefault();

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
                                var SalHead = db.SalaryHead.Include(e => e.SalHeadOperationType).Where(e => e.Id == Salheadid).FirstOrDefault();
                                List<YearlyPaymentT> OFAT = new List<YearlyPaymentT>();


                                if (SalHead.Name.ToUpper() == "BONUS")
                                {

                                    CompanyPayroll OCompanyPayroll = null;

                                    var BonusCal = db.Calendar.Include(a => a.Name).Where(a => a.Name.LookupVal.ToUpper() == "BONUSYEAR" && a.Default == true).SingleOrDefault();

                                    Calendar Cal = db.Calendar.Where(e => e.Id == BonusCal.Id).SingleOrDefault();

                                    OCompanyPayroll = db.CompanyPayroll.Where(e => e.Company.Id == OEmployee.GeoStruct.Company.Id).SingleOrDefault();


                                    BonusExGratiaAmt = YearlyPaymentTController.BonusCalc(OCompanyPayroll.Id, OEmployeePayroll.Id, Cal);
                                    if (BonusExGratiaAmt != null)
                                    {
                                        BonusAmt = BonusExGratiaAmt[0];
                                        ExGratiaAmt = BonusExGratiaAmt[1];

                                    }
                                    if (BonusAmt > 0)
                                    {
                                        var BonusSalhead = db.SalaryHead.Where(e => e.Code.ToUpper() == "BONUS").FirstOrDefault();
                                        Msg[1] = Convert.ToString(BonusAmt);
                                        // As Told by Sir for Settlement Process LvEncashPayment and yearlypayment data save not req.direct save in FFsetlmentdetaist table

                                        //ObjYPT.AmountPaid = BonusAmt;
                                        //ObjYPT.SalaryHead = null;
                                        //ObjYPT.SalaryHead = BonusSalhead;
                                        //db.YearlyPaymentT.Add(ObjYPT);
                                        //db.SaveChanges();

                                        //OFAT.Add(db.YearlyPaymentT.Find(ObjYPT.Id));

                                        //if (OEmployeePayroll == null)
                                        //{
                                        //    EmployeePayroll OTEP = new EmployeePayroll()
                                        //    {
                                        //        Employee = db.Employee.Find(OEmployee.Id),
                                        //        YearlyPaymentT = OFAT,
                                        //        DBTrack = Y.DBTrack

                                        //    };


                                        //    db.EmployeePayroll.Add(OTEP);
                                        //    db.SaveChanges();
                                        //}
                                        //else
                                        //{
                                        //    var aa = db.EmployeePayroll.Find(OEmployeePayroll.Id);
                                        //    aa.YearlyPaymentT = OFAT;
                                        //    //OEmployeePayroll.DBTrack = dbt;
                                        //    db.EmployeePayroll.Attach(aa);
                                        //    db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                        //    db.SaveChanges();
                                        //    db.Entry(aa).State = System.Data.Entity.EntityState.Detached;

                                        //}

                                        // As Told by Sir for Settlement Process LvEncashPayment and yearlypayment data save not req.direct save in FFsetlmentdetaist table

                                    }
                                    if (ExGratiaAmt > 0)
                                    {
                                        var ExGratiaSalhead = db.SalaryHead.Where(e => e.Code.ToUpper() == "EXGRATIA").FirstOrDefault();
                                        Msg[1] = Convert.ToString(BonusAmt + ExGratiaAmt);

                                        // As Told by Sir for Settlement Process LvEncashPayment and yearlypayment data save not req.direct save in FFsetlmentdetaist table

                                        //ObjYPT.AmountPaid = ExGratiaAmt;
                                        //ObjYPT.SalaryHead = null;
                                        //ObjYPT.SalaryHead = ExGratiaSalhead;
                                        //db.YearlyPaymentT.Add(ObjYPT);
                                        //db.SaveChanges();

                                        //OFAT.Add(db.YearlyPaymentT.Find(ObjYPT.Id));

                                        //if (OEmployeePayroll == null)
                                        //{
                                        //    EmployeePayroll OTEP = new EmployeePayroll()
                                        //    {
                                        //        Employee = db.Employee.Find(OEmployee.Id),
                                        //        YearlyPaymentT = OFAT,
                                        //        DBTrack = Y.DBTrack

                                        //    };


                                        //    db.EmployeePayroll.Add(OTEP);
                                        //    db.SaveChanges();
                                        //}
                                        //else
                                        //{
                                        //    var aa = db.EmployeePayroll.Find(OEmployeePayroll.Id);
                                        //    aa.YearlyPaymentT = OFAT;
                                        //    //OEmployeePayroll.DBTrack = dbt;
                                        //    db.EmployeePayroll.Attach(aa);
                                        //    db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                        //    db.SaveChanges();
                                        //    db.Entry(aa).State = System.Data.Entity.EntityState.Detached;

                                        //}

                                        // As Told by Sir for Settlement Process LvEncashPayment and yearlypayment data save not req.direct save in FFsetlmentdetaist table

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
                                        //.Include(e => e.EmpSalStructDetails.Select(q => q.SalHeadFormula))
                                        //.Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.Frequency))
                                        //.Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.Type))
                                        //.Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.RoundingMethod))
                                        //.Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.SalHeadOperationType))
                                      .Where(e => e.EmployeePayroll.Id == OEmployeePayroll.Id);

                                    if (SalHead.SalHeadOperationType.LookupVal.ToUpper() == "GRATUITY")
                                    {
                                        fromdt = Convert.ToDateTime(fyyr.FromDate);
                                        EndDt = Convert.ToDateTime(fyyr.ToDate);
                                    }
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
                                            string PayMonthnew = mChkDate.Month + "/" + mChkDate.Year;

                                            var OEmpSalDetails = item.EmpSalStructDetails.Where(e => e.SalaryHead.Id == SalHead.Id).SingleOrDefault();

                                            if (OEmpSalDetails != null)
                                            {

                                                if (checkcell == false)
                                                {
                                                    cellamt = OEmpSalDetails.Amount;
                                                }
                                                if (OEmpSalDetails.EmpSalStruct.EffectiveDate <= fromdt)
                                                {
                                                    if ((fromdt.Date).Day == 1 && (mChkDate.Date).Day == DateTime.DaysInMonth(Convert.ToInt32(PayMonthnew.Split('/')[1]), Convert.ToInt32(PayMonthnew.Split('/')[0])))
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
                                                    if ((OEmpSalDetails.EmpSalStruct.EffectiveDate).Value.Day == 1 && (mChkDate.Date).Day == DateTime.DaysInMonth(Convert.ToInt32(PayMonthnew.Split('/')[1]), Convert.ToInt32(PayMonthnew.Split('/')[0])))
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

                                    // Gratuity calculation start
                                    if (SalHead.SalHeadOperationType.LookupVal.ToUpper() == "GRATUITY")
                                    {

                                        var db_data = db.EmployeePayroll.Include(e => e.GratuityT)
                                       .Where(e => e.Employee.Id == OEmployeeId).FirstOrDefault();
                                        double gratuityamount = 0;
                                        if (db_data != null)
                                        {
                                            var gratamt = db_data.GratuityT.OrderByDescending(e => e.Id).FirstOrDefault();
                                            if (gratamt != null)
                                            {
                                                gratuityamount = gratamt.Amount;

                                            }

                                        }


                                        ObjYPT.AmountPaid = gratuityamount;

                                    }
                                    // Gratuity calculation end
                                    Msg[1] = Convert.ToString(ObjYPT.AmountPaid);

                                    // As Told by Sir for Settlement Process LvEncashPayment and yearlypayment data save not req.direct save in FFsetlmentdetaist table

                                    //db.YearlyPaymentT.Add(ObjYPT);
                                    //db.SaveChanges();
                                    ////List<YearlyPaymentT> OFAT = new List<YearlyPaymentT>();
                                    //OFAT.Add(db.YearlyPaymentT.Find(ObjYPT.Id));

                                    //if (OEmployeePayroll == null)
                                    //{
                                    //    EmployeePayroll OTEP = new EmployeePayroll()
                                    //    {
                                    //        Employee = db.Employee.Find(OEmployee.Id),
                                    //        YearlyPaymentT = OFAT,
                                    //        DBTrack = Y.DBTrack

                                    //    };


                                    //    db.EmployeePayroll.Add(OTEP);
                                    //    db.SaveChanges();
                                    //}
                                    //else
                                    //{
                                    //    var aa = db.EmployeePayroll.Find(OEmployeePayroll.Id);
                                    //    aa.YearlyPaymentT = OFAT;
                                    //    //OEmployeePayroll.DBTrack = dbt;
                                    //    db.EmployeePayroll.Attach(aa);
                                    //    db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                    //    db.SaveChanges();
                                    //    db.Entry(aa).State = System.Data.Entity.EntityState.Detached;

                                    //}

                                    // As Told by Sir for Settlement Process LvEncashPayment and yearlypayment data save not req.direct save in FFsetlmentdetaist table

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
                                    ControllerName = "",//this.ControllerContext.RouteData.Values["controller"].ToString(),
                                    ExceptionMessage = ex.Message,
                                    ExceptionStackTrace = ex.StackTrace,
                                    LineNo = Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                                    LogTime = DateTime.Now
                                };
                                Msg[0] = (ex.Message);
                                return Msg;

                                //Logfile.CreateLogFile(Err);
                                //return Json(new { sucess = false, responseText = ex.InnerException }, JsonRequestBehavior.AllowGet);
                            }
                            catch (Exception ex)
                            {
                                LogFile Logfile = new LogFile();
                                ErrorLog Err = new ErrorLog()
                                {
                                    ControllerName = "", //this.ControllerContext.RouteData.Values["controller"].ToString(),
                                    ExceptionMessage = ex.Message,
                                    ExceptionStackTrace = ex.StackTrace,
                                    LineNo = Convert.ToString(new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                                    LogTime = DateTime.Now
                                };
                                Logfile.CreateLogFile(Err);

                                Msg[0] = (ex.Message);
                                return Msg;
                                // return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                        }

                    }
                    else
                    {

                        // List<string> Msgu = new List<string>();
                        Msg[0] = ("  Unable to create...  ");
                        return Msg;
                        //return Json(new Utility.JsonReturnClass { success = false, responseText = Msgu }, JsonRequestBehavior.AllowGet);
                        // return this.Json(new Object[] { "", "", "Unable to create...", JsonRequestBehavior.AllowGet });
                    }
                    // List<string> Msgs = new List<string>();
                    Msg[0] = ("  Data Saved successfully  ");
                    return Msg;
                    //return Json(new Utility.JsonReturnClass { success = true, responseText = Msgs }, JsonRequestBehavior.AllowGet);

                }
                catch (Exception ex)
                {
                    //  List<string> Msg = new List<string>();

                    LogFile Logfile = new LogFile();
                    ErrorLog Err = new ErrorLog()
                    {
                        ControllerName = "",// this.ControllerContext.RouteData.Values["controller"].ToString(),
                        ExceptionMessage = ex.Message,
                        ExceptionStackTrace = ex.StackTrace,
                        LineNo = Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                        LogTime = DateTime.Now
                    };
                    Logfile.CreateLogFile(Err);
                    Msg[0] = (ex.Message);
                    return Msg;
                    // return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                }
                return Msg;
                // return Json(new Utility.JsonReturnClass { success = false, responseText = "Unable to Create" }, JsonRequestBehavior.AllowGet);
            }
        }
        public static string[] Lvencashpayment(int OEmployeeId, string PayMonth, string ProcessMonth, string Paymentdate, string lvencashidst, LvEncashPayment L, int SalaryHead)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string[] Msg = new string[2];
                try
                {

                    string lvencashid = lvencashidst == "0" ? "" : lvencashidst;

                    // var salhead = db.SalaryHead.Include(e => e.SalHeadOperationType).Where(e => e.SalHeadOperationType.LookupVal.ToUpper() == "LVENCASH").FirstOrDefault();
                    var salhead = db.SalaryHead.Include(e => e.SalHeadOperationType).Where(e => e.Id == SalaryHead).FirstOrDefault();


                    List<int> Lvencashids = null;


                    Employee OEmployee = null;
                    EmployeePayroll OEmployeePayroll = null;
                    EmployeeLeave OEmployeeLeave = null;
                    LvEncashPayment LEP = new LvEncashPayment();

                    LEP.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                    int counter = 0;

                    if (OEmployeeId != null)
                    {

                        OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct)
                                    .Where(r => r.Id == OEmployeeId).SingleOrDefault();

                        OEmployeePayroll = db.EmployeePayroll
                      .Where(e => e.Employee.Id == OEmployeeId).SingleOrDefault();

                        OEmployeeLeave = db.EmployeeLeave
                                        .Include(e => e.Employee)
                                        .Include(e => e.LeaveEncashReq)
                                        .Include(e => e.LeaveEncashReq.Select(x => x.LvHead))
                                        .Where(e => e.Employee.Id == OEmployeeId).SingleOrDefault();

                        var LeaveEncashList = OEmployeeLeave.LeaveEncashReq.Where(e => e.IsCancel == false && e.TrClosed == false).ToList();

                        foreach (var item in LeaveEncashList)
                        {

                            LvEncashPayment ObjID = new LvEncashPayment();
                            {
                                ObjID.AmountPaid = LEP.AmountPaid;
                                ObjID.OtherDeduction = LEP.OtherDeduction;
                                ObjID.PaymentDate = Convert.ToDateTime(Paymentdate);
                                ObjID.PaymentMonth = PayMonth;
                                ObjID.TDSAmount = LEP.TDSAmount;
                                ObjID.ProcessMonth = ProcessMonth;
                                ObjID.LvEncashReq = item;
                                ObjID.DBTrack = LEP.DBTrack;

                            }
                            ObjID.AmountPaid = PayrollReportGen.LeaveEncashCalcSepration(OEmployeePayroll.Id, ObjID, salhead.Id);
                            Msg[1] = Convert.ToString(ObjID.AmountPaid);
                            // As Told by Sir for Settlement Process LvEncashPayment and yearlypayment data save not req.direct save in FFsetlmentdetaist table

                            using (TransactionScope ts = new TransactionScope())
                            {

                                int LvencashId = ObjID.LvEncashReq.Id;
                                LvEncashReq lvupdate = db.LvEncashReq.Include(e => e.WFStatus).Where(e => e.Id == LvencashId).SingleOrDefault();
                                lvupdate.TrClosed = true;
                                lvupdate.WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "3").FirstOrDefault();//db.LookupValue.Where(e => e.LookupVal == "3").SingleOrDefault();
                                db.LvEncashReq.Attach(lvupdate);
                                db.Entry(lvupdate).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(lvupdate).State = System.Data.Entity.EntityState.Detached;

                                //    ObjID.AmountPaid = PayrollReportGen.LeaveEncashCalc(OEmployeePayroll.Id, ObjID);
                                //    Msg[1] = Convert.ToString(ObjID.AmountPaid);
                                //    db.LvEncashPayment.Add(ObjID);
                                //    db.SaveChanges();


                                //    YearlyPaymentT objyearlyP = new YearlyPaymentT();
                                //    {
                                //        //objyearlyP.FromPeriod = LEP.LvEncashReq != null ? LEP.LvEncashReq.FromPeriod : null;
                                //        //objyearlyP.ToPeriod = LEP.LvEncashReq != null ? LEP.LvEncashReq.ToPeriod : null;
                                //        objyearlyP.FinancialYear = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "FINANCIALYEAR" && e.Default == true).FirstOrDefault();
                                //        objyearlyP.FromPeriod = ObjID.LvEncashReq.FromPeriod;
                                //        objyearlyP.ToPeriod = ObjID.LvEncashReq.ToPeriod;
                                //        objyearlyP.Narration = ObjID.LvEncashReq.Narration;
                                //        objyearlyP.AmountPaid = ObjID.AmountPaid;
                                //        objyearlyP.OtherDeduction = ObjID.OtherDeduction;
                                //        objyearlyP.PayMonth = ObjID.PaymentMonth;
                                //        objyearlyP.TDSAmount = ObjID.TDSAmount;
                                //        objyearlyP.ProcessMonth = ObjID.ProcessMonth;
                                //        objyearlyP.LvEncashReq = ObjID.LvEncashReq;
                                //        objyearlyP.SalaryHead = db.SalaryHead.Include(e => e.Frequency).Include(e => e.SalHeadOperationType).Where(e => e.SalHeadOperationType.LookupVal.ToUpper() == "LVENCASH")
                                //                                .FirstOrDefault();
                                //        objyearlyP.DBTrack = ObjID.DBTrack;

                                //    }

                                //    objyearlyP.GeoStruct = db.GeoStruct.Find(OEmployee.GeoStruct.Id);

                                //    objyearlyP.FuncStruct = db.FuncStruct.Find(OEmployee.FuncStruct.Id);

                                //    objyearlyP.PayStruct = db.PayStruct.Find(OEmployee.PayStruct.Id);

                                //    objyearlyP.AmountPaid = ObjID.AmountPaid;
                                //    db.YearlyPaymentT.Add(objyearlyP);
                                //    db.SaveChanges();





                                //    List<LvEncashPayment> OFAT = new List<LvEncashPayment>();
                                //    // OFAT.Add(db.LvEncashPayment.Find(ObjID.Id));
                                //    OFAT.Add(ObjID);
                                //    List<YearlyPaymentT> OYP = new List<YearlyPaymentT>();
                                //    //  OYP.Add(db.YearlyPaymentT.Find(objyearlyP.Id));
                                //    OYP.Add(objyearlyP);

                                //    if (OEmployeePayroll == null)
                                //    {
                                //        EmployeePayroll OTEP = new EmployeePayroll()
                                //        {
                                //            Employee = db.Employee.Find(OEmployee.Id),
                                //            LvEncashPayment = OFAT,
                                //            YearlyPaymentT = OYP,
                                //            DBTrack = LEP.DBTrack

                                //        };


                                //        db.EmployeePayroll.Add(OTEP);
                                //        db.SaveChanges();
                                //    }
                                //    else
                                //    {


                                //        EmployeePayroll LeavEncashEmployeepayroll = db.EmployeePayroll
                                //           .Include(e => e.LvEncashPayment).Where(e => e.Employee.Id == OEmployeeId).SingleOrDefault();

                                //        if (LeavEncashEmployeepayroll.LvEncashPayment != null)
                                //        {
                                //            OFAT.AddRange(LeavEncashEmployeepayroll.LvEncashPayment);
                                //        }
                                //        LeavEncashEmployeepayroll.LvEncashPayment = OFAT;

                                //        db.EmployeePayroll.Attach(LeavEncashEmployeepayroll);
                                //        db.Entry(LeavEncashEmployeepayroll).State = System.Data.Entity.EntityState.Modified;
                                //        db.SaveChanges();
                                //        db.Entry(LeavEncashEmployeepayroll).State = System.Data.Entity.EntityState.Detached;

                                //        EmployeePayroll YearlypaymentEmployeepayroll = db.EmployeePayroll
                                //          .Include(e => e.YearlyPaymentT).Where(e => e.Employee.Id == OEmployeeId).SingleOrDefault();

                                //        if (YearlypaymentEmployeepayroll.YearlyPaymentT != null)
                                //        {
                                //            OYP.AddRange(YearlypaymentEmployeepayroll.YearlyPaymentT);
                                //        }
                                //        YearlypaymentEmployeepayroll.YearlyPaymentT = OYP;

                                //        db.EmployeePayroll.Attach(YearlypaymentEmployeepayroll);
                                //        db.Entry(YearlypaymentEmployeepayroll).State = System.Data.Entity.EntityState.Modified;
                                //        db.SaveChanges();
                                //        db.Entry(YearlypaymentEmployeepayroll).State = System.Data.Entity.EntityState.Detached;



                                //    }

                                ts.Complete();

                                //    //}

                                //    //  Deduct Lv end
                            }

                            // As Told by Sir for Settlement Process LvEncashPayment and yearlypayment data save not req.direct save in FFsetlmentdetaist table

                        }


                        Msg[0] = ("Data Saved successfully");
                        return Msg;
                        // return Json(new Utility.JsonReturnClass { success = true, responseText = Msgs }, JsonRequestBehavior.AllowGet);

                    }


                }
                catch (Exception ex)
                {
                    //  List<string> Msg = new List<string>();

                    LogFile Logfile = new LogFile();
                    ErrorLog Err = new ErrorLog()
                    {
                        ControllerName = "",// this.ControllerContext.RouteData.Values["controller"].ToString(),
                        ExceptionMessage = ex.Message,
                        ExceptionStackTrace = ex.StackTrace,
                        LineNo = Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                        LogTime = DateTime.Now
                    };
                    Logfile.CreateLogFile(Err);
                    Msg[0] = (ex.Message);
                    return Msg;
                    // return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                }
                return Msg;
                // return Json(new Utility.JsonReturnClass { success = false, responseText = "Unable to Create" }, JsonRequestBehavior.AllowGet);
            }
        }

        public List<int> one_ids(string form)
        {
            string itsec_id = form;
            List<int> ids = itsec_id.Split(',').Select(e => int.Parse(e)).ToList();
            return (ids);
        }
        public static string Lvencashreqs(int OEmployeeId, string PayMonth, string Processmonth, string Paymentdate, string lvheadid, string fromdate, string Todate, LvEncashReq L, int SalaryHead)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string Msg = "";
                try
                {
                    LvEncashReq OBJLVWFDnew = new LvEncashReq();
                    int sv = 0;
                    var val = db.LvHead.Find(int.Parse(lvheadid));

                    OBJLVWFDnew.LvHead = val;
                    var Id = Convert.ToInt32(SessionManager.CompanyId);
                    string CompCode = db.Company.Where(e => e.Id == Id).SingleOrDefault().Code.ToUpper();
                    var Cal = db.Calendar.Where(e => e.Default == true && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();
                    double oLvClosingData = 0;
                    double UtilizedLv = 0;
                    double UtilizedLvcount = 0;
                    double oLvOccurances = 0;
                    Int32 GeoStruct = 0;
                    Int32 PayStruct = 0;
                    Int32 FuncStruct = 0;
                    int chk = Convert.ToInt16(lvheadid);
                    DateTime? Lvyearfrom = null;
                    DateTime? LvyearTo = null;
                    LvEncashPolicy encashpolicy = null;
                    var LvNewReqlist = "";
                    //if (ModelState.IsValid)
                    //{
                    if (OEmployeeId != null)
                    {
                        //foreach (var j in ids)
                        //{
                        EmployeeLeave _Prv_EmpLvData = db.EmployeeLeave.Where(a => a.Employee.Id == OEmployeeId)
                                           .Include(a => a.LvNewReq)
                                           .Include(a => a.LvNewReq.Select(e => e.LeaveHead))
                                           .Include(a => a.LvNewReq.Select(e => e.LeaveCalendar))
                                           .Include(a => a.LvNewReq.Select(e => e.WFStatus))
                                           .Include(a => a.LvNewReq.Select(e => e.GeoStruct))
                                           .Include(a => a.LvNewReq.Select(e => e.PayStruct))
                                           .Include(a => a.LvNewReq.Select(e => e.FuncStruct))
                                            .Where(e => e.LvNewReq.Any(a => a.LeaveHead != null && a.LeaveHead.Id == chk))
                                               .AsParallel()
                                               .SingleOrDefault();

                        EmployeeLvStruct OLvSalStruct = db.EmployeeLvStruct
                              .Include(e => e.EmployeeLvStructDetails)
                              .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHead))
                              .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula))
                              .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvEncashPolicy))
                              .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHead.LvHeadOprationType))
                                  .Where(e => e.EndDate == null && e.EmployeeLeave_Id == _Prv_EmpLvData.Id).SingleOrDefault();

                        encashpolicy = OLvSalStruct.EmployeeLvStructDetails.Where(e => e.LvHead.Id == chk && e.LvHeadFormula != null && e.LvHeadFormula.LvEncashPolicy != null).Select(r => r.LvHeadFormula.LvEncashPolicy).FirstOrDefault();
                        if (encashpolicy == null)
                        {
                            Msg = ("Policy not defined for this employee..!");
                            return Msg;
                            //  return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }

                        List<LvNewReq> Filter_oEmpLvData = new List<LvNewReq>();
                        if (_Prv_EmpLvData != null)
                        {
                            Filter_oEmpLvData = _Prv_EmpLvData.LvNewReq
                                .Where(a => a.LeaveHead != null && a.LeaveHead.Id == chk).ToList();
                            if (Filter_oEmpLvData.Count == 0)
                            {
                                Msg = ("Employee opening balance not inserted!");
                                return Msg;
                                // return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                        }

                        if (CompCode == "MSCB")
                        {
                            if (db.LvHead.Where(e => e.Id == chk).FirstOrDefault().LvCode == "SLHP")
                            {
                                DateTime RetirementDt = (DateTime)db.Employee.Include(x => x.ServiceBookDates).Where(a => a.Id == OEmployeeId).SingleOrDefault().ServiceBookDates.RetirementDate;
                                DateTime start = DateTime.Today;
                                DateTime end = RetirementDt;
                                int compMonth = (end.Month + end.Year * 12) - (start.Month + start.Year * 12);
                                double daysInEndMonth = (end.AddMonths(1) - end).Days;
                                double months = compMonth + Math.Abs((start.Day - end.Day) / daysInEndMonth);
                                double mAge = Math.Abs(months / 12);

                                if (mAge > 3)
                                {
                                    Msg = ("You can not encash SLHP.because Your Service More than 3 Years");
                                    return Msg;
                                    //  return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }
                            }

                        }


                        if (Filter_oEmpLvData.Count == 0)
                        {
                            //get Data from opening
                            var _Emp_EmpOpeningData = db.EmployeeLeave.Where(e => e.Employee.Id == OEmployeeId && e.LvOpenBal.Any(a => a.LvHead.Id == chk && a.LvCalendar.Id == Cal.Id))
                                .Include(e => e.LvOpenBal.Select(a => a.LvHead))
                                .Include(e => e.LvOpenBal.Select(a => a.LvCalendar))
                                .Include(e => e.Employee.GeoStruct)
                                .Include(e => e.Employee.PayStruct)
                                .Include(e => e.Employee.FuncStruct)
                                .SingleOrDefault();

                            var _EmpOpeningData = _Emp_EmpOpeningData.LvOpenBal.Where(e => e.LvHead.Id == chk && e.LvCalendar.Id == Cal.Id).Select(e => e.LvOpening).SingleOrDefault();
                            var _EmpOpeninglvoccranceData = _Emp_EmpOpeningData.LvOpenBal.Where(e => e.LvHead.Id == chk && e.LvCalendar.Id == Cal.Id).Select(e => e.LvOccurances).SingleOrDefault();

                            var UtilizedLeavefromLvopebal = _Emp_EmpOpeningData.LvOpenBal.Where(e => e.LvHead.Id == chk && e.LvCalendar.Id == Cal.Id).Select(e => e.LVCount).SingleOrDefault();

                            oLvOccurances = _EmpOpeninglvoccranceData;
                            oLvClosingData = _EmpOpeningData;
                            UtilizedLv = UtilizedLeavefromLvopebal;
                            GeoStruct = _Emp_EmpOpeningData.Employee.GeoStruct.Id;
                            PayStruct = _Emp_EmpOpeningData.Employee.PayStruct.Id;
                            FuncStruct = _Emp_EmpOpeningData.Employee.FuncStruct.Id;
                        }
                        else
                        {
                            var LastLvData = Filter_oEmpLvData.OrderByDescending(a => a.Id).FirstOrDefault();

                            oLvClosingData = LastLvData.CloseBal;
                            oLvOccurances = LastLvData.LvOccurances;
                            UtilizedLv = LastLvData.LVCount;
                            GeoStruct = LastLvData.GeoStruct.Id;
                            PayStruct = LastLvData.PayStruct.Id;
                            FuncStruct = LastLvData.FuncStruct.Id;
                        }

                        if (encashpolicy.EncashSpanYear != 0)
                        {
                            //var encid = db.SalaryHead.Include(e => e.Frequency).Include(e => e.SalHeadOperationType).Where(e => e.SalHeadOperationType.LookupVal.ToUpper() == "LVENCASH")
                            //                        .FirstOrDefault();
                            var encid = db.SalaryHead.Include(e => e.Frequency).Include(e => e.SalHeadOperationType).Where(e => e.Id == SalaryHead)
                                                    .FirstOrDefault();

                            var LVENP = db.EmployeePayroll.Include(q => q.YearlyPaymentT.Select(a => a.SalaryHead)).Where(e => e.Employee.Id == OEmployeeId).SingleOrDefault();

                            var dat = LVENP.YearlyPaymentT.Where(a => a.SalaryHead.Id == encid.Id).ToList();
                            foreach (var item in dat)
                            {
                                //if (item.ReleaseDate == null)
                                //{
                                //    Msg = ("Please Release Old enashment then apply for new. ");
                                //    return Msg;
                                //    //  return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //}





                            }

                            EmployeeLeave CheckEncashSpan = db.EmployeeLeave.Include(t => t.LeaveEncashReq).Include(t => t.LeaveEncashReq.Select(e => e.LeaveCalendar))
                                                        .Include(t => t.LeaveEncashReq.Select(e => e.LvHead)).Where(t => t.Employee.Id == OEmployeeId).SingleOrDefault();

                            if (CompCode == "KDCC")// Kolhapur DCC
                            {
                                var dat1 = CheckEncashSpan.LeaveEncashReq.Where(t => t.LvHead.Id == chk && t.TrReject != true && t.IsCancel != true && t.LeaveCalendar.Id == Cal.Id).ToList();
                                // settlement process encash policy not check
                                //if (dat1.Count() == encashpolicy.EncashSpanYear)
                                //{

                                //    Msg = ("Encashment Span Year yet to complete. " + encashpolicy.EncashSpanYear);
                                //    return Msg;
                                //    // return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                //}
                            }
                            else
                            {
                                //var dat1 = CheckEncashSpan.LeaveEncashReq.Where(t => t.LvHead.Id == chk && t.TrReject != true && t.LeaveCalendar.Id==Cal.Id).ToList();
                                DateTime? lvcrdate = _Prv_EmpLvData.LvNewReq.Where(e => e.LeaveHead.Id == chk && e.LvCreditDate != null).Max(e => e.LvCreditDate.Value);
                                if (lvcrdate != null)
                                {
                                    Lvyearfrom = lvcrdate;
                                    LvyearTo = Lvyearfrom.Value.AddYears(1);
                                    // LvyearTo = LvyearTo.Value.AddDays(-1);

                                    var dat1 = CheckEncashSpan.LeaveEncashReq.Where(t => t.LvHead.Id == chk && t.TrReject != true && t.LeaveCalendar.Id == Cal.Id && t.FromPeriod >= Lvyearfrom && t.ToPeriod <= LvyearTo).ToList();
                                    // settlement process encash policy not check
                                    //if (dat1.Count() == encashpolicy.EncashSpanYear)
                                    //{

                                    //    Msg = ("Encashment Span Year yet to complete. " + encashpolicy.EncashSpanYear);
                                    //    return Msg;
                                    //    //return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //    //}
                                    //}
                                }

                            }
                        }
                        else
                        {

                            //var encid = db.SalaryHead.Include(e => e.Frequency).Include(e => e.SalHeadOperationType).Where(e => e.SalHeadOperationType.LookupVal.ToUpper() == "LVENCASH")
                            //                        .FirstOrDefault();
                            var encid = db.SalaryHead.Include(e => e.Frequency).Include(e => e.SalHeadOperationType).Where(e => e.Id == SalaryHead)
                                                   .FirstOrDefault();

                            var LVENP = db.EmployeePayroll.Include(q => q.YearlyPaymentT.Select(a => a.SalaryHead)).Where(e => e.Employee.Id == OEmployeeId).SingleOrDefault();

                            var dat = LVENP.YearlyPaymentT.Where(a => a.SalaryHead.Id == encid.Id).ToList();
                            foreach (var item in dat)
                            {
                                //if (item.ReleaseDate == null)
                                //{
                                //    Msg = ("Please Release Old enashment then apply for new. ");
                                //    return Msg;
                                //    //  return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //}


                            }

                        }

                        //}
                    }
                    // }
                    DateTime Fromdateenc = Convert.ToDateTime(fromdate);
                    DateTime Todateenc = Convert.ToDateTime(Todate);

                    var lvcalendar = db.Calendar.Where(e => e.Name.LookupVal.ToUpper() == "LEAVECALENDAR" && e.Default == true).FirstOrDefault();
                    OBJLVWFDnew.FromPeriod = Fromdateenc.Date;//lvcalendar.FromDate.Value.Date;
                    OBJLVWFDnew.ToPeriod = Todateenc.Date; //lvcalendar.ToDate.Value.Date;
                    OBJLVWFDnew.LvNewReq = null;
                    OBJLVWFDnew.EncashDays = oLvClosingData;
                    OBJLVWFDnew.Narration = "Settlement Process";
                    OBJLVWFDnew.LeaveCalendar = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "LEAVECALENDAR" && e.Default == true).SingleOrDefault();

                    if (OBJLVWFDnew.ToPeriod.Value < OBJLVWFDnew.FromPeriod.Value)
                    {
                        Msg = ("To Period Should Be More than From Period ");
                        return Msg;
                        // return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    if (CompCode != "KDCC")
                    {
                        if (Lvyearfrom != null && LvyearTo != null)
                        {

                            if (OBJLVWFDnew.FromPeriod < Lvyearfrom)
                            {
                                Msg = ("Encashment Period Should be between Leave Calender year ");
                                return Msg;
                                //return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                            if (OBJLVWFDnew.ToPeriod > LvyearTo)
                            {
                                Msg = ("Encashment Period Should be between Leave Calender year ");
                                return Msg;
                                //return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
                    // settlement process encash policy not check
                    //if (encashpolicy.IsLvMultiple == true)
                    //{
                    //    if (OBJLVWFDnew.EncashDays % encashpolicy.LvMultiplier != 0)
                    //    {
                    //        Msg = ("You can apply leave encashment multiplier of " + encashpolicy.LvMultiplier);
                    //        return Msg;
                    //        // return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //    }
                    //}

                    //if (encashpolicy.IsOnBalLv == true)
                    //{
                    //    if (OBJLVWFDnew.EncashDays != Math.Round((oLvClosingData * encashpolicy.LvBalPercent / 100) + 0.001, 0))
                    //    {
                    //        Msg = ("you can apply Encash days balance leave of " + encashpolicy.LvBalPercent + " percent.");
                    //        return Msg;
                    //        //return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //    }
                    //}
                    // settlement process encash policy not check

                    // settlement process encash policy not check
                    //if (OBJLVWFDnew.EncashDays < encashpolicy.MinEncashment)
                    //{
                    //    Msg = (" Encash days should be more than  " + encashpolicy.MinEncashment);
                    //    return Msg;
                    //    // return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //}

                    //if (OBJLVWFDnew.EncashDays > encashpolicy.MaxEncashment)
                    //{
                    //    Msg = (" Encash days cannot more than  " + encashpolicy.MaxEncashment);
                    //    return Msg;
                    //    //return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //}



                    //if (UtilizedLv < encashpolicy.MinUtilized)
                    //{
                    //    Msg = ("For Encashment minimum utilization less than  " + encashpolicy.MinUtilized);
                    //    return Msg;
                    //    //return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //}
                    //if (CompCode == "KDCC")
                    //{
                    //    if ((oLvClosingData - OBJLVWFDnew.EncashDays) < encashpolicy.MinBalance)
                    //    {
                    //        Msg = ("For Encashment minimum balance require  " + encashpolicy.MinBalance);
                    //        return Msg;
                    //        //return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //    }
                    //}
                    //else
                    //{
                    //    if (oLvClosingData < encashpolicy.MinBalance)
                    //    {
                    //        Msg = ("For Encashment minimum balance require  " + encashpolicy.MinBalance);
                    //        return Msg;
                    //        // return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //    }
                    //}



                    if (oLvClosingData < OBJLVWFDnew.EncashDays)
                    {
                        Msg = ("You can not apply for encashment because your Leave Balance is  " + oLvClosingData);
                        return Msg;
                        // return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    // Start Checking for policy end

                    if (LvNewReqlist != null && LvNewReqlist != "-Select-" && LvNewReqlist != "")
                    {
                        var value = db.LvNewReq.Find(int.Parse(LvNewReqlist));
                        OBJLVWFDnew.LvNewReq = value;

                    }
                    var Comp_Id = 0;
                    Comp_Id = Convert.ToInt32(SessionManager.CompanyId);
                    var Z = db.CompanyPayroll.Where(e => e.Company.Id == Comp_Id).Include(e => e.EmployeePayroll.Select(f => f.Employee)).SingleOrDefault();
                    Employee OEmployee = null;
                    EmployeeLeave OEmployeePayroll = null;

                    OBJLVWFDnew.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.EmpId, IsModified = false };
                    OBJLVWFDnew.LeaveCalendar = db.Calendar.Where(e => e.Name.LookupVal.ToUpper() == "LEAVECALENDAR" && e.Default == true).SingleOrDefault();
                    LvEncashReq OBJLVWFD = new LvEncashReq()
                    {
                        EncashDays = OBJLVWFDnew.EncashDays,
                        FromPeriod = OBJLVWFDnew.FromPeriod,
                        ToPeriod = OBJLVWFDnew.ToPeriod,
                        LvNewReq = OBJLVWFDnew.LvNewReq,
                        Narration = OBJLVWFDnew.Narration,
                        LvHead = OBJLVWFDnew.LvHead,
                        DBTrack = OBJLVWFDnew.DBTrack,
                        LeaveCalendar = OBJLVWFDnew.LeaveCalendar,
                        WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "0").FirstOrDefault(), //db.LookupValue.Where(e => e.LookupVal == "0").SingleOrDefault(),

                    };
                    //if (ModelState.IsValid)
                    //{
                    if (OEmployeeId != null)
                    {
                        //foreach (var i in ids)
                        //{
                        OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct)
                                    .Where(r => r.Id == OEmployeeId).SingleOrDefault();

                        OEmployeePayroll
                        = db.EmployeeLeave.Include(e => e.LeaveEncashReq)
                      .Where(e => e.Employee.Id == OEmployeeId).SingleOrDefault();
                        OBJLVWFD.GeoStruct = db.GeoStruct.Find(OEmployee.GeoStruct.Id);
                        OBJLVWFD.FuncStruct = db.FuncStruct.Find(OEmployee.FuncStruct.Id);
                        OBJLVWFD.PayStruct = db.PayStruct.Find(OEmployee.PayStruct.Id);

                        using (TransactionScope ts = new TransactionScope())
                        {
                            try
                            {

                                db.LvEncashReq.Add(OBJLVWFD);
                                db.SaveChanges();
                                List<LvEncashReq> OFAT = new List<LvEncashReq>();
                                //  OFAT.Add(db.LvEncashReq.Find(OBJLVWFD.Id));
                                OFAT.Add(OBJLVWFD);
                                if (OEmployeePayroll == null)
                                {
                                    EmployeeLeave OTEP = new EmployeeLeave()
                                    {
                                        Employee = db.Employee.Find(OEmployee.Id),
                                        LeaveEncashReq = OFAT,
                                        DBTrack = OBJLVWFDnew.DBTrack

                                    };


                                    db.EmployeeLeave.Add(OTEP);
                                    db.SaveChanges();
                                }
                                else
                                {


                                    if (OEmployeePayroll.LeaveEncashReq != null)
                                    {
                                        OFAT.AddRange(OEmployeePayroll.LeaveEncashReq);
                                    }
                                    OEmployeePayroll.LeaveEncashReq = OFAT;
                                    db.EmployeeLeave.Attach(OEmployeePayroll);
                                    db.Entry(OEmployeePayroll).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    db.Entry(OEmployeePayroll).State = System.Data.Entity.EntityState.Detached;

                                }

                                // Surendra Start lv debit
                                if (OEmployeePayroll != null)
                                {
                                    var EmpID = Convert.ToInt32(OEmployeeId);
                                    var OEmployeeLv = db.EmployeeLeave
                                        .Include(e => e.LvNewReq)
                                        .Include(e => e.LvNewReq.Select(a => a.LeaveHead))
                                        .Include(e => e.LvOpenBal.Select(a => a.LvHead))
                                        .Include(e => e.LvNewReq.Select(a => a.LeaveCalendar))
                                        .Where(e => e.Employee.Id == EmpID)
                                        .FirstOrDefault();

                                    LvNewReq PrevReq = null;
                                    // if (L.LvNewReq != null)
                                    if (OEmployeeLv != null)
                                    {
                                        PrevReq = OEmployeeLv.LvNewReq
                                        .Where(e => e.LeaveHead.Id == chk)
                                        .OrderByDescending(e => e.Id).FirstOrDefault();
                                    }
                                    else
                                    {
                                        //PrevReq = OEmployeeLv.LvNewReq
                                        //              .Where(e => e.LeaveHead.Id == LEP.LvEncashReq.LvHead.Id && e.LeaveCalendar.Id == LEP.LvEncashReq.LeaveCalendar.Id)
                                        //    .OrderByDescending(e => e.Id).FirstOrDefault();
                                    }
                                    LvNewReq oLvNewReq = null;
                                    if (PrevReq != null)
                                    {
                                        int id = PrevReq.Id;
                                        var LvNewReq = db.LvNewReq.Where(e => e.Id == id)
                                            .Include(e => e.LeaveCalendar).Include(e => e.LeaveHead).Include(e => e.GeoStruct).Include(e => e.PayStruct).Include(e => e.FuncStruct)
                                            .FirstOrDefault();

                                        //  L.LvNewReq = value;
                                        oLvNewReq = new LvNewReq()
                                        {
                                            ReqDate = DateTime.Now,
                                            CreditDays = 0,
                                            FromDate = OBJLVWFDnew.FromPeriod,
                                            FromStat = LvNewReq.FromStat,
                                            LeaveHead = LvNewReq.LeaveHead,
                                            Reason = LvNewReq.Reason,
                                            ResumeDate = LvNewReq.ResumeDate,
                                            ToDate = OBJLVWFDnew.ToPeriod,
                                            ToStat = LvNewReq.ToStat,
                                            LeaveCalendar = LvNewReq.LeaveCalendar,
                                            DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false },
                                            OpenBal = LvNewReq.CloseBal,
                                            //  DebitDays = LEP.LvEncashReq.EncashDays,
                                            DebitDays = OBJLVWFDnew.EncashDays,
                                            // CloseBal = LvNewReq.CloseBal - LEP.LvEncashReq.EncashDays,
                                            CloseBal = LvNewReq.CloseBal - OBJLVWFDnew.EncashDays,
                                            LVCount = LvNewReq.LVCount + OBJLVWFDnew.EncashDays,
                                            LvOccurances = LvNewReq.LvOccurances,
                                            TrClosed = true,
                                            LvOrignal = null,
                                            GeoStruct = LvNewReq.GeoStruct,
                                            PayStruct = LvNewReq.PayStruct,
                                            FuncStruct = LvNewReq.FuncStruct,
                                            Narration = "Leave Encash Payment",
                                            WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "2").FirstOrDefault(),//db.LookupValue.Where(e => e.LookupVal == "2").SingleOrDefault(),
                                        };
                                    }
                                    else
                                    {
                                        // var LvEncashReq = db.LvEncashReq.Include(e => e.LvHead).Include(e => e.LeaveCalendar).Where(e => e.Id == LEP.LvEncashReq.Id).SingleOrDefault();
                                        var OpenBalData = db.EmployeeLeave.Include(e => e.LvOpenBal)
                                            .Include(e => e.LvOpenBal.Select(a => a.LvCalendar))
                                            .Include(e => e.LvOpenBal.Select(a => a.LvHead))
                                            .Include(e => e.Employee.GeoStruct)
                                            .Include(e => e.Employee.PayStruct)
                                            .Include(e => e.Employee.FuncStruct)
                                            .Where(e => e.Employee.Id == EmpID && e.LvOpenBal.Count() > 0 && e.LvOpenBal.Any(a => a.LvHead.Id == chk && a.LvCalendar.Id == L.LeaveCalendar.Id))
                                            .SingleOrDefault();
                                        var OpenBal = OpenBalData.LvOpenBal.Where(e => e.LvHead.Id == chk && e.LvCalendar.Id == L.LeaveCalendar.Id).SingleOrDefault();
                                        oLvNewReq = new LvNewReq()
                                        {
                                            ReqDate = DateTime.Now,
                                            CreditDays = 0,
                                            FromDate = OBJLVWFDnew.FromPeriod,
                                            FromStat = db.LookupValue.Where(e => e.LookupVal.ToUpper() == "FULLSESSION").SingleOrDefault(),
                                            LeaveHead = OBJLVWFDnew.LvHead,
                                            Reason = OBJLVWFDnew.Narration,
                                            ResumeDate = OBJLVWFDnew.ToPeriod.Value.AddDays(1),
                                            ToDate = OBJLVWFDnew.ToPeriod,
                                            ToStat = db.LookupValue.Where(e => e.LookupVal.ToUpper() == "FULLSESSION").SingleOrDefault(),
                                            LeaveCalendar = OBJLVWFDnew.LeaveCalendar,
                                            DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false },
                                            OpenBal = OpenBal.LvClosing,
                                            DebitDays = OBJLVWFDnew.EncashDays,
                                            CloseBal = OpenBal.LvClosing - OBJLVWFDnew.EncashDays,
                                            LVCount = OBJLVWFDnew.EncashDays,
                                            LvOccurances = 0,
                                            TrClosed = true,
                                            LvOrignal = null,
                                            WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "2").FirstOrDefault(), //db.LookupValue.Where(e => e.LookupVal == "2").SingleOrDefault(),

                                            GeoStruct = OpenBalData.Employee.GeoStruct,
                                            PayStruct = OpenBalData.Employee.PayStruct,
                                            FuncStruct = OpenBalData.Employee.FuncStruct,
                                            //IsCancel = true
                                            Narration = "Leave Encash Payment"
                                        };
                                    }
                                    db.LvNewReq.Add(oLvNewReq);
                                    db.SaveChanges();
                                    OEmployeeLv.LvNewReq.Add(oLvNewReq);
                                    db.Entry(OEmployeeLv).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    db.Entry(OEmployeeLv).State = System.Data.Entity.EntityState.Detached;

                                }

                                // Surendra end lv debit

                                ts.Complete();
                                //  Msg = ("  Data Saved successfully  ");
                                Msg = ("SAVED");
                                return Msg;
                                //return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                                //return Json(new { sucess = true, responseText = "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);

                            }
                            catch (DataException ex)
                            {
                                LogFile Logfile = new LogFile();
                                ErrorLog Err = new ErrorLog()
                                {
                                    ControllerName = "",//this.ControllerContext.RouteData.Values["controller"].ToString(),
                                    ExceptionMessage = ex.Message,
                                    ExceptionStackTrace = ex.StackTrace,
                                    LineNo = Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                                    LogTime = DateTime.Now
                                };
                                Logfile.CreateLogFile(Err);
                                Msg = (ex.Message);
                                return Msg;
                                //return Json(new { sucess = false, responseText = ex.InnerException }, JsonRequestBehavior.AllowGet);
                            }


                        }
                        //}
                    }
                    Msg = (" Unable to create.Try again, and if the problem persists, see your system administrator");
                    return Msg;
                    // return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //return this.Json(new Object[] { "", "", "Unable to create...", JsonRequestBehavior.AllowGet });
                    //}
                    //else
                    //{
                    //    StringBuilder sb = new StringBuilder("");
                    //    //foreach (ModelState modelState in ModelState.Values)
                    //    //{
                    //    //    foreach (ModelError error in modelState.Errors)
                    //    //    {
                    //    //        sb.Append(error.ErrorMessage);
                    //    //        sb.Append("." + "\n");
                    //    //    }
                    //    //}
                    //    var errorMsg = "";//sb.ToString();
                    //    Msg=(errorMsg);
                    //    return Msg;
                    //    //return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //    //return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
                    //    //return this.Json(new { msg = errorMsg });
                    //}

                }
                catch (Exception ex)
                {
                    LogFile Logfile = new LogFile();
                    ErrorLog Err = new ErrorLog()
                    {
                        ControllerName = "", //this.ControllerContext.RouteData.Values["controller"].ToString(),
                        ExceptionMessage = ex.Message,
                        ExceptionStackTrace = ex.StackTrace,
                        LineNo = Convert.ToString(new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                        LogTime = DateTime.Now
                    };
                    Logfile.CreateLogFile(Err);
                    Msg = (ex.Message);
                    return Msg;
                    // return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }

                return Msg;

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
                            var empresig = db.EmployeeExit.Include(e => e.Employee).Include(e => e.Employee.EmpName).Include(e => e.SeperationProcessT)
                    .Include(e => e.SeperationProcessT.FFSSettlementDetailT)
                    .Include(e => e.SeperationProcessT.FFSSettlementDetailT.Select(x => x.ProcessType))
                    .Include(e => e.SeperationProcessT.FFSSettlementDetailT.Select(y => y.FFSSettlementReleaseDetailT))
                   .ToList();

                            foreach (var z in empresig)
                            {
                                if (z.SeperationProcessT != null && z.SeperationProcessT.FFSSettlementDetailT != null)
                                {
                                    foreach (var item in z.SeperationProcessT.FFSSettlementDetailT)
                                    {

                                        if (item.Id == data)
                                        {
                                            if (item.FFSSettlementReleaseDetailT.Count() != 0)
                                            {
                                                Msg.Add("This Salary Head Amount is Released" + " So, Unable to Delete");
                                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                            }

                                            var salaryheadretoperation = db.SalaryHead.Include(e => e.SalHeadOperationType).Where(e => e.Code == item.SalaryHead).FirstOrDefault();

                                            if (salaryheadretoperation.SalHeadOperationType.LookupVal.ToUpper() == "LVENCASH" || salaryheadretoperation.SalHeadOperationType.LookupVal.ToUpper() == "LVENCASHEXEMPTED")
                                            {
                                                var lvcalendarid = db.Calendar.Include(e => e.Name)
                                                .Where(e => e.Default == true && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();

                                                var db_data = db.FFSSettlementDetailT.Include(e => e.LvEncashReq)
                                                   .Where(e => e.Id == data).SingleOrDefault();
                                                var db_data2 = db.LvEncashReq
                                               .Where(e => e.Id == db_data.LvEncashReq.Id).SingleOrDefault();

                                                if (db_data2 != null)
                                                {
                                                    var LvCalendar = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "LEAVECALENDAR" && e.Default == true).SingleOrDefault();
                                                    var OEmployeeLv = db.EmployeeLeave
                                                        .Include(e => e.Employee)
                                                        .Include(e => e.LvNewReq)
                                                        .Include(e => e.LvNewReq.Select(a => a.LeaveHead))
                                                        .Include(e => e.LvOpenBal.Select(a => a.LvHead))
                                                        .Include(e => e.LvNewReq.Select(a => a.GeoStruct))
                                                        .Include(e => e.LvNewReq.Select(a => a.PayStruct))
                                                        .Include(e => e.LvNewReq.Select(a => a.FuncStruct))
                                                        .Include(e => e.LvNewReq.Select(a => a.LeaveCalendar))
                                                        .Where(e => e.Employee.Id == z.Employee.Id)
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
                                                        FromStat = db.LookupValue.Where(a => a.LookupVal.ToUpper() == "FULLSESSION").Distinct().SingleOrDefault(),
                                                        LeaveHead = db_data2.LvHead,
                                                        //Reason = db_data1.Reason,
                                                        ResumeDate = DateTime.Now,
                                                        ToDate = db_data2.ToPeriod,
                                                        ToStat = db.LookupValue.Where(a => a.LookupVal.ToUpper() == "FULLSESSION").Distinct().SingleOrDefault(),
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
                                                        WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "2").FirstOrDefault(), //db.LookupValue.Where(e => e.LookupVal == "2").SingleOrDefault(),
                                                        Narration = "Leave Encashment Cancelled"
                                                    };

                                                    db.LvNewReq.Add(oLvNewReq);
                                                    // db.SaveChanges();

                                                    var aa = db.EmployeeLeave.Include(e => e.Employee).Where(e => e.Employee.Id == z.Employee.Id)
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



                                                    //  db.SaveChanges();
                                                    db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                                                    db.SaveChanges();
                                                    db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;

                                                    db_data2.WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "4").FirstOrDefault();//db.LookupValue.Where(e => e.LookupVal == "4").SingleOrDefault();
                                                    db_data2.IsCancel = true;
                                                    db_data2.TrClosed = true;
                                                    db.LvEncashReq.Attach(db_data2);
                                                    //  db.SaveChanges();
                                                    db.Entry(db_data2).State = System.Data.Entity.EntityState.Modified;
                                                    db.SaveChanges();
                                                    db.Entry(db_data2).State = System.Data.Entity.EntityState.Detached;


                                                    FFSSettlementDetailT FFSSettlementDetailT = db.FFSSettlementDetailT.Find(data);
                                                    db.Entry(FFSSettlementDetailT).State = System.Data.Entity.EntityState.Deleted;
                                                    await db.SaveChangesAsync();
                                                    ts.Complete();



                                                    Msg.Add("Data removed successfully. ");
                                                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                                }


                                            }
                                            else
                                            {
                                                FFSSettlementDetailT FFSSettlementDetailT = db.FFSSettlementDetailT.Find(data);
                                                db.Entry(FFSSettlementDetailT).State = System.Data.Entity.EntityState.Deleted;
                                                await db.SaveChangesAsync();
                                                ts.Complete();
                                                Msg.Add("  Data removed successfully.  ");
                                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                            }
                                        }
                                    }
                                }
                            }


                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            // return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });

                        }
                        catch (RetryLimitExceededException /* dex */)
                        {
                            // return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
                            //  List<string> Msgn = new List<string>();
                            Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator. ");
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
        }

        public ActionResult Create(FFSSettlementDetailT S, FormCollection form, String forwarddata) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    string Emp = forwarddata == "0" ? "" : forwarddata;
                    string Lvhead = form["lvhead_id"] == "" ? "0" : Convert.ToString(form["lvhead_id"]);
                    List<int> LvHead_ids = null;
                    if (Lvhead != null && Lvhead != "0")
                    {
                        LvHead_ids = Utility.StringIdsToListIds(Lvhead);
                    }
                    //  string Empstruct_drop = form["Empstruct_drop"] == "0" ? "" : form["Empstruct_drop"];

                    List<int> ids = null;
                    if (Emp != "null" && Emp != "0" && Emp != "false")
                    {
                        ids = one_ids(Emp);
                        //if (Empstruct_drop != null && Empstruct_drop != "" && Empstruct_drop != "-Select-")
                        //{
                        //    var value = db.EmpSalStruct.Find(int.Parse(Empstruct_drop));
                        //    S.EmpSalStruct = value;
                        //}
                        //else
                        //{
                        //    Msg.Add("  Please Select the Process Month And Load the Structure...!  ");
                        //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //}
                    }
                    else
                    {
                        Msg.Add("  Kindly select employee  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    string ProcessMonth = form["Create_Processmonth"] == "0 " ? "" : form["Create_Processmonth"];


                    string PayMonth = form["Create_Paymonth"] == "0" ? "" : form["Create_Paymonth"];
                    if (PayMonth == "")
                    {
                        Msg.Add(" Kindly Select Pay Month ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }


                    string Paymentdate = form["Create_Paymentdate"] == "0" ? "" : form["Create_Paymentdate"];
                    if (Paymentdate == "")
                    {
                        Msg.Add(" Kindly Select Payment date ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    //  string Reason = form["Create_Reason"] == "0" ? "" : form["Create_Reason"];

                    // List<int> SalHead = null;
                    int SalaryHead = form["SalaryHeadlist"] == "0" ? 0 : Convert.ToInt32(form["SalaryHeadlist"]);


                    string fromdate = form["fromdate"] == "0 " ? "" : form["fromdate"];
                    string Todate = form["Todate"] == "0 " ? "" : form["Todate"];

                    var salaryheadret = db.SalaryHead.Include(e => e.SalHeadOperationType).Where(e => e.Id == SalaryHead).FirstOrDefault();

                    if (salaryheadret.SalHeadOperationType.LookupVal.ToUpper() == "LVENCASH" || salaryheadret.SalHeadOperationType.LookupVal.ToUpper() == "LVENCASHEXEMPTED")
                    {
                        if (Lvhead == null)
                        {
                            Msg.Add("  Kindly Select Leave Code ");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    //DateTime prevmonth = DateTime.Parse(ProcessMonth);
                    //prevmonth = prevmonth.AddMonths(-1);
                    //string prev = prevmonth.ToString("MM/yyyy");

                    //List<String> paychk = PayMonth.Split('/').Select(e => e).ToList();
                    //int monch = Convert.ToInt32(paychk[0]);
                    //int yerch = Convert.ToInt32(paychk[1]);
                    //int days = DateTime.DaysInMonth(yerch, monch);


                    Employee OEmployee = null;
                    EmployeeExit OEmployeePayroll = null;
                    string EmpCode = null;
                    string EmpRel = null;

                    if (ids != null)
                    {
                        foreach (var i in ids)
                        {

                            var val = db.SalaryHead.Find(SalaryHead);

                            OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.GeoStruct.Company).Include(e => e.FuncStruct).Include(e => e.PayStruct)
                                .Include(e => e.PayStruct.JobStatus)
                                 .Include(e => e.PayStruct.JobStatus.EmpActingStatus)
                                       .Where(r => r.Id == i).AsNoTracking().SingleOrDefault();

                            OEmployeePayroll = db.EmployeeExit.Include(e => e.Employee).Where(e => e.Employee.Id == i).AsNoTracking().SingleOrDefault();
                            if (OEmployee.PayStruct.JobStatus.EmpActingStatus.LookupVal.ToUpper() != "RESIGNED" && salaryheadret.SalHeadOperationType.LookupVal.ToUpper() == "LVENCASH")
                            {
                                Msg.Add("  Kindly select Exempted leave encashment  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }
                            if (OEmployee.PayStruct.JobStatus.EmpActingStatus.LookupVal.ToUpper() == "RESIGNED" && salaryheadret.SalHeadOperationType.LookupVal.ToUpper() == "LVENCASHEXEMPTED")
                            {
                                Msg.Add("  Kindly select leave encashment ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }
                            //var v = OEmployeePayroll.SalaryT.Where(a => a.ReleaseDate == null && a.PayMonth == prev).FirstOrDefault();
                            //if (v != null)
                            //{
                            //    Msg.Add("Lock the Salary for month=" + prev);
                            //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //}
                            var OEmpSalT = db.EmployeeExit.Where(e => e.Id == OEmployeePayroll.Id)
                                .Include(e => e.SeperationProcessT)
                                .Include(e => e.SeperationProcessT.FFSSettlementDetailT)
                                .Include(e => e.SeperationProcessT.FFSSettlementDetailT.Select(x => x.LvEncashReq))
                                .Include(e => e.SeperationProcessT.FFSSettlementDetailT.Select(x => x.LvEncashReq.LvHead))
                                //.Include(e => e.SeperationProcessT.FFSSettlementDetailT.Select(p => p.SalaryHead))
                                .AsNoTracking()
                                //.Where(e => e.SeperationProcessT.FFSSettlementDetailT.SalaryHead.ToUpper() == val.Code.ToUpper()).AsNoTracking()
                                .SingleOrDefault();
                            if (salaryheadret.SalHeadOperationType.LookupVal.ToUpper() == "LVENCASH" || salaryheadret.SalHeadOperationType.LookupVal.ToUpper() == "LVENCASHEXEMPTED")
                            {
                                foreach (int LvHeadidss in LvHead_ids)
                                {
                                    if (OEmpSalT != null && OEmpSalT.SeperationProcessT != null && OEmpSalT.SeperationProcessT.FFSSettlementDetailT != null)
                                    {
                                        var lvenchead = OEmpSalT.SeperationProcessT.FFSSettlementDetailT.Where(x => x.SalaryHead.ToUpper() == salaryheadret.Code.ToUpper()).Select(e => e.LvEncashReq.LvHead).ToList();
                                        if (lvenchead.Count() > 0)
                                        {
                                            foreach (var item in lvenchead)
                                            {
                                                if (item.Id == LvHeadidss)
                                                {
                                                    var lvcode = db.LvHead.Where(e => e.Id == item.Id).SingleOrDefault();
                                                    return Json(new { success = true, responseText = "FF Settlement already exists for " + lvcode.LvCode.ToUpper() + "." }, JsonRequestBehavior.AllowGet);
                                                    break;
                                                }
                                            }
                                        }

                                    }
                                }
                            }
                            else
                            {


                                if (OEmpSalT != null && OEmpSalT.SeperationProcessT != null && OEmpSalT.SeperationProcessT.FFSSettlementDetailT.Any(t => t.SalaryHead.ToUpper() == salaryheadret.Code.ToUpper()))
                                    return Json(new { success = true, responseText = "FF Settlement already exists for " + salaryheadret.Code.ToUpper() + "." }, JsonRequestBehavior.AllowGet);
                            }

                        }
                    }

                    var GetProcessType = db.Lookup
                                        .Include(e => e.LookupValues)
                                         .Where(e => e.Code == "3030").FirstOrDefault();
                    if (GetProcessType == null)
                    {
                        Msg.Add("  Kindly Define in Lookup FFSETTLEMENT Process Type and code will 3030  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        Boolean prtype = false;
                        var processtype = GetProcessType.LookupValues.ToList();
                        foreach (var item in processtype)
                        {
                            if (item.LookupVal.ToUpper() == "BENEFIT PROCESS")
                            {
                                prtype = true;
                                S.ProcessType = db.LookupValue.Find(item.Id);
                            }

                        }

                        if (prtype == false)
                        {
                            Msg.Add(" Kindly Define in Lookup 'BENEFIT PROCESS' under FFSETTLEMENT ");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }


                    if (PayMonth != null && PayMonth != "")
                    {
                        S.PayMonth = Convert.ToDateTime(PayMonth);
                        int mon = int.Parse(PayMonth.Split('/')[0].StartsWith("0") == true ? PayMonth.Split('/')[0].Remove(0, 1) : PayMonth.Split('/')[0]);
                        int DaysInMonth = System.DateTime.DaysInMonth(int.Parse(PayMonth.Split('/')[1]), mon);
                        //S.MonthDays = DaysInMonth;
                    }




                    DateTime Todaydate = DateTime.Now;

                    S.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };



                    if (ids != null)
                    {
                        foreach (var i in ids)
                        {

                            double Amountpaid = 0;
                            var salaryheadretoperation = db.SalaryHead.Include(e => e.SalHeadOperationType).Where(e => e.Id == SalaryHead).FirstOrDefault();

                            if (salaryheadretoperation.SalHeadOperationType.LookupVal.ToUpper() == "GRATUITY")
                            {
                                CompanyPayroll OCompanyPayroll = null;
                                DateTime ProcDate = Convert.ToDateTime(Todate);
                                OCompanyPayroll = db.CompanyPayroll.Where(e => e.Company.Id == OEmployee.GeoStruct.Company.Id).SingleOrDefault();
                                EmployeePayroll OEmployeePayrollex = null;
                                OEmployeePayrollex = db.EmployeePayroll.Where(e => e.Employee.Id == i).AsNoTracking().SingleOrDefault();
                                Process.PayrollReportGen.GratuityCalc(OCompanyPayroll.Id, OEmployeePayrollex.Id, ProcDate);
                            }
                            FFSSettlementDetailT ObjFAT = new FFSSettlementDetailT();

                            if (salaryheadretoperation.SalHeadOperationType.LookupVal.ToUpper() == "LVENCASH" || salaryheadret.SalHeadOperationType.LookupVal.ToUpper() == "LVENCASHEXEMPTED")
                            {
                                if (ProcessMonth == "")
                                {
                                    Msg.Add(" Kindly Select Process Month");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }
                                S.ProcessMonth = Convert.ToDateTime(ProcessMonth);
                                // before process old encash check release or not 
                                var encid = db.SalaryHead.Include(e => e.Frequency).Include(e => e.SalHeadOperationType).Where(e => e.Id == SalaryHead)
                                                       .FirstOrDefault();

                                var LVENP = db.EmployeePayroll.Include(q => q.YearlyPaymentT.Select(a => a.SalaryHead)).Where(e => e.Employee.Id == i).SingleOrDefault();

                                var dat = LVENP.YearlyPaymentT.Where(a => a.SalaryHead.Id == encid.Id).ToList();
                                foreach (var item in dat)
                                {
                                    if (item.ReleaseDate == null)
                                    {
                                        Msg.Add("Please Release Old enashment then apply for new. ");
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    }

                                }

                                // leave encash req
                                //using (TransactionScope ts1 = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 30, 0)))
                                //{
                                //try
                                //{
                                foreach (int LvHeadids in LvHead_ids)
                                {
                                    using (TransactionScope ts1 = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 30, 0)))
                                    {
                                        try
                                        {
                                            string req = "";
                                            req = Lvencashreqs(i, PayMonth, ProcessMonth, Paymentdate, Convert.ToString(LvHeadids), fromdate, Todate, null, SalaryHead);
                                            if (req == "SAVED")
                                            {
                                                var Cal = db.Calendar.Where(e => e.Default == true && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();
                                                var empdata = db.EmployeeLeave
                                                       .Include(e => e.LeaveEncashReq)
                                                       .Include(e => e.LeaveEncashReq.Select(t => t.LvHead))
                                                       .Include(e => e.LeaveEncashReq.Select(t => t.LeaveCalendar))
                                                       .Include(e => e.Employee)
                                                       .Include(e => e.Employee.EmpName)
                                                       .Where(e => e.Employee.Id == i).FirstOrDefault();

                                                List<EmployeePayroll> EmpLvencashpayment = db.EmployeePayroll
                                                           .Include(e => e.LvEncashPayment)
                                                           .Include(e => e.LvEncashPayment.Select(t => t.LvEncashReq))
                                                           .Where(e => e.Employee.Id == i).ToList();

                                                var LeaveEncashId = EmpLvencashpayment.SelectMany(e => e.LvEncashPayment.Select(t => t.LvEncashReq.Id)).ToList();
                                                var EmployeeLeaveEncashList = empdata.LeaveEncashReq.Where(a => a.LeaveCalendar.Id == Cal.Id && a.TrClosed == false && !LeaveEncashId.Contains(a.Id)).ToList();
                                                if (EmployeeLeaveEncashList.Count() > 0)
                                                {

                                                    // string Setpr = "";
                                                    foreach (var item in EmployeeLeaveEncashList.Where(e => e.LvHead_Id == LvHeadids))
                                                    {
                                                        string[] Setpr = Lvencashpayment(i, PayMonth, ProcessMonth, Paymentdate, Convert.ToString(item.Id), null, SalaryHead);
                                                        if (Setpr[1] != "")
                                                        {
                                                            // Amountpaid = Amountpaid + Convert.ToDouble(Setpr[1]);
                                                            Amountpaid = Convert.ToDouble(Setpr[1]);
                                                            var val = db.SalaryHead
                                                                     .Include(e => e.Type)
                                                                     .Include(e => e.ProcessType)
                                                                     .Where(e => e.Id == SalaryHead).FirstOrDefault();
                                                            S.SalaryHead = val.Code;
                                                            S.SalaryHeadDesc = val.Name;
                                                            S.SalType = val.Type.LookupVal.ToUpper().ToString();
                                                            //    FFSSettlementDetailT ObjFAT = new FFSSettlementDetailT();
                                                            ObjFAT.PayAmount = Amountpaid;
                                                            ObjFAT.PayMonth = S.PayMonth;
                                                            ObjFAT.ProcessMonth = S.ProcessMonth;
                                                            ObjFAT.SalaryHead = S.SalaryHead;
                                                            ObjFAT.SalaryHeadDesc = S.SalaryHeadDesc;
                                                            ObjFAT.ProcessType = S.ProcessType;
                                                            ObjFAT.SalType = S.SalType;
                                                            ObjFAT.PayDate = Convert.ToDateTime(Paymentdate);
                                                            ObjFAT.LvEncashReq = db.LvEncashReq.Find(item.Id);
                                                            ObjFAT.IsPaid = false;
                                                            ObjFAT.DBTrack = S.DBTrack;

                                                        }
                                                    }

                                                }

                                                List<FFSSettlementDetailT> OFFSList = new List<FFSSettlementDetailT>();
                                                db.FFSSettlementDetailT.Add(ObjFAT);
                                                db.SaveChanges();
                                                OFFSList.Add(ObjFAT);
                                                var aa = db.EmployeeExit.Include(x => x.SeperationProcessT).Include(e => e.SeperationProcessT.FFSSettlementDetailT).Where(e => e.Employee.Id == i).FirstOrDefault();
                                                if (aa.SeperationProcessT == null)
                                                {
                                                    SeperationProcessT OTEP = new SeperationProcessT()
                                                    {
                                                        FFSSettlementDetailT = OFFSList,
                                                        DBTrack = S.DBTrack
                                                    };
                                                    db.SeperationProcessT.Add(OTEP);
                                                    db.SaveChanges();
                                                    aa.SeperationProcessT = OTEP;
                                                    db.EmployeeExit.Attach(aa);
                                                    db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                                    db.SaveChanges();
                                                    db.Entry(aa).State = System.Data.Entity.EntityState.Detached;

                                                }
                                                else
                                                {
                                                    OFFSList.AddRange(aa.SeperationProcessT.FFSSettlementDetailT);
                                                    aa.SeperationProcessT.FFSSettlementDetailT = OFFSList;
                                                    db.EmployeeExit.Attach(aa);
                                                    db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                                    db.SaveChanges();
                                                    db.Entry(aa).State = System.Data.Entity.EntityState.Detached;
                                                }
                                            }
                                            else
                                            {
                                                return Json(new Utility.JsonReturnClass { success = false, responseText = req }, JsonRequestBehavior.AllowGet);

                                            }
                                        }
                                        catch (Exception ex)
                                        {

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
                                        ts1.Complete();
                                    }
                                }


                                //}
                                //catch (Exception ex)
                                //{

                                //    Msg.Add(ex.Message);
                                //    LogFile Logfile = new LogFile();
                                //    ErrorLog Err = new ErrorLog()
                                //    {
                                //        ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                                //        ExceptionMessage = ex.Message,
                                //        ExceptionStackTrace = ex.StackTrace,
                                //        LineNo = Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                                //        LogTime = DateTime.Now
                                //    };
                                //    Logfile.CreateLogFile(Err);
                                //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //}


                                //ts1.Complete();
                                //}
                                // leave encash payment
                            }
                            // other yearly payment Bonus,Gratuity....
                            else
                            {
                                string[] Annpay = Annualpayment(i, PayMonth, Paymentdate, fromdate, Todate, SalaryHead, null);
                                if (Annpay[1] != "" && Annpay[1] != null)
                                {
                                    Amountpaid = Amountpaid + Convert.ToDouble(Annpay[1]);
                                }

                            }
                            if (salaryheadretoperation.SalHeadOperationType.LookupVal.ToUpper() != "LVENCASH" && salaryheadretoperation.SalHeadOperationType.LookupVal.ToUpper() != "LVENCASHEXEMPTED")
                            {
                                using (TransactionScope ts2 = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 30, 0)))
                                {
                                    try
                                    {
                                        var val = db.SalaryHead
                                        .Include(e => e.Type)
                                        .Include(e => e.ProcessType)
                                        .Where(e => e.Id == SalaryHead).FirstOrDefault();
                                        S.SalaryHead = val.Code;
                                        S.SalaryHeadDesc = val.Name;
                                        S.SalType = val.Type.LookupVal.ToUpper().ToString();
                                        // FFSSettlementDetailT ObjFAT = new FFSSettlementDetailT();

                                        ObjFAT.PayAmount = Amountpaid;
                                        ObjFAT.PayMonth = S.PayMonth;
                                        ObjFAT.ProcessMonth = S.ProcessMonth;
                                        ObjFAT.SalaryHead = S.SalaryHead;
                                        ObjFAT.SalaryHeadDesc = S.SalaryHeadDesc;
                                        ObjFAT.ProcessType = S.ProcessType;
                                        ObjFAT.SalType = S.SalType;
                                        ObjFAT.PayDate = Convert.ToDateTime(Paymentdate);
                                        ObjFAT.IsPaid = false;
                                        ObjFAT.DBTrack = S.DBTrack;

                                        List<FFSSettlementDetailT> OFFSList = new List<FFSSettlementDetailT>();
                                        db.FFSSettlementDetailT.Add(ObjFAT);
                                        db.SaveChanges();
                                        OFFSList.Add(ObjFAT);
                                        var aa = db.EmployeeExit.Include(x => x.SeperationProcessT).Include(e => e.SeperationProcessT.FFSSettlementDetailT).Where(e => e.Employee.Id == i).FirstOrDefault();
                                        if (aa.SeperationProcessT == null)
                                        {
                                            SeperationProcessT OTEP = new SeperationProcessT()
                                            {
                                                FFSSettlementDetailT = OFFSList,
                                                DBTrack = S.DBTrack
                                            };
                                            db.SeperationProcessT.Add(OTEP);
                                            db.SaveChanges();
                                            aa.SeperationProcessT = OTEP;
                                            db.EmployeeExit.Attach(aa);
                                            db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();
                                            db.Entry(aa).State = System.Data.Entity.EntityState.Detached;

                                        }
                                        else
                                        {
                                            OFFSList.AddRange(aa.SeperationProcessT.FFSSettlementDetailT);
                                            aa.SeperationProcessT.FFSSettlementDetailT = OFFSList;
                                            db.EmployeeExit.Attach(aa);
                                            db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();
                                            db.Entry(aa).State = System.Data.Entity.EntityState.Detached;
                                        }

                                    }
                                    catch (Exception ex)
                                    {

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

                                    ts2.Complete();
                                }

                            }

                            //using (TransactionScope ts = new TransactionScope())
                            //{
                            //    try
                            //    {
                            //        List<FFSSettlementDetailT> OFFSList = new List<FFSSettlementDetailT>();
                            //        db.FFSSettlementDetailT.Add(ObjFAT);
                            //        db.SaveChanges();
                            //        OFFSList.Add(ObjFAT);
                            //        var aa = db.EmployeeExit.Include(x => x.SeperationProcessT).Include(e => e.SeperationProcessT.FFSSettlementDetailT).Where(e => e.Employee.Id == i).FirstOrDefault();
                            //        if (aa.SeperationProcessT == null)
                            //        {
                            //            SeperationProcessT OTEP = new SeperationProcessT()
                            //            {
                            //                FFSSettlementDetailT = OFFSList,
                            //                DBTrack = S.DBTrack
                            //            };


                            //            db.SeperationProcessT.Add(OTEP);

                            //            db.SaveChanges();

                            //            aa.SeperationProcessT = OTEP;
                            //            db.EmployeeExit.Attach(aa);
                            //            db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                            //            db.SaveChanges();
                            //            db.Entry(aa).State = System.Data.Entity.EntityState.Detached;

                            //        }
                            //        else
                            //        {
                            //            OFFSList.AddRange(aa.SeperationProcessT.FFSSettlementDetailT);
                            //            aa.SeperationProcessT.FFSSettlementDetailT = OFFSList;
                            //            db.EmployeeExit.Attach(aa);
                            //            db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                            //            db.SaveChanges();
                            //            db.Entry(aa).State = System.Data.Entity.EntityState.Detached;
                            //        }
                            //        ts.Complete();

                            //    }
                            //    catch (Exception ex)
                            //    {
                            //        //   List<string> Msg = new List<string>();
                            //        Msg.Add(ex.Message);
                            //        LogFile Logfile = new LogFile();
                            //        ErrorLog Err = new ErrorLog()
                            //        {
                            //            ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                            //            ExceptionMessage = ex.Message,
                            //            ExceptionStackTrace = ex.StackTrace,
                            //            LineNo = Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                            //            LogTime = DateTime.Now
                            //        };
                            //        Logfile.CreateLogFile(Err);
                            //        //return Json(new { sucess = false, Msg }, JsonRequestBehavior.AllowGet);
                            //        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //    }
                            //}

                        }
                        // List<string> Msgs = new List<string>();
                        Msg.Add("Data Saved successfully");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //return this.Json(new { success = true, responseText = "Data Saved Successfully...", JsonRequestBehavior.AllowGet });
                    }
                    // List<string> Msg = new List<string>();
                    Msg.Add("  Unable to create...  ");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //return this.Json(new { success = false, responseText = "Unable to create...", JsonRequestBehavior.AllowGet });
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

        public JsonResult GetSalHead(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                int Id = int.Parse(data);
                var qurey = db.SalaryHead.Include(e => e.SalHeadOperationType).Where(e => e.Frequency.LookupVal.ToUpper() == "YEARLY" && e.SalHeadOperationType.LookupVal.ToUpper() != "PERK").Where(e => e.Id == Id).SingleOrDefault();
                if (qurey != null)
                {
                    var salheadoper = qurey.SalHeadOperationType.LookupVal.ToUpper();

                    return Json(salheadoper, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }

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
        public ActionResult getCalendarjoinretdt(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                int Id = int.Parse(data);
                var query = db.Employee.Where(e => e.Id == Id)
                  .Include(e => e.ServiceBookDates).AsEnumerable()
                  .Select(e => new
                  {
                      Fycalendardesc = "From Date :" + e.ServiceBookDates.JoiningDate.Value.ToShortDateString() + " To Date :" + e.ServiceBookDates.RetirementDate.Value.ToShortDateString(),
                      Fromperiod = e.ServiceBookDates.JoiningDate.Value.ToShortDateString(),
                      Toperiod = e.ServiceBookDates.RetirementDate.Value.ToShortDateString()
                  })
                  .SingleOrDefault();
                return Json(query, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult getLeaveCalendar()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var qurey = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "LEAVECALENDAR" && e.Default == true).AsEnumerable()
                    .Select(e => new
                    {
                        Fycalendardesc = "From Date :" + e.FromDate.Value.ToShortDateString() + " To Date :" + e.ToDate.Value.ToShortDateString(),
                        Fromperiod = e.FromDate.Value.ToShortDateString(),
                        Toperiod = e.ToDate.Value.ToShortDateString()
                    }).SingleOrDefault();
                return Json(qurey, JsonRequestBehavior.AllowGet);
            }
        }
        // if LVENCASHEXEMPTED then LvNewReq Date Show
        public ActionResult getLvNewReqCalendar(string EmployeeId)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                int empid = Convert.ToInt32(EmployeeId);

                var query1 = db.EmployeeLeave.Where(r => r.Employee_Id == empid).SelectMany(r => r.LvNewReq).Select(n => new
                {
                    LeaveHeadId = n.LeaveHead.Id,
                    Narration = n.Narration,
                    LvCreditDate = n.LvCreditDate,
                    LvCreditNextDate = n.LvCreditNextDate,
                    CloseBal = n.CloseBal,
                    CreditDays = n.CreditDays,
                    OpenBal = n.OpenBal,
                    LvLapsed = n.LvLapsed
                }).Where(x => x.Narration == "Settlement Process").FirstOrDefault();
                return Json(query1, JsonRequestBehavior.AllowGet);
            }

        }
        //for LeaveHead click event show the balance.
        public JsonResult LeaveHeadClosingBalance(string leave, string EmployeeId)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var leaveids = leave == "" ? "0" : leave;
                int empid = Convert.ToInt32(EmployeeId);
                int lvnewreqid = Convert.ToInt32(leaveids);
                var empleaveid = db.EmployeeLeave.Where(r => r.Employee_Id == empid).SingleOrDefault();
                var lvnewsettlementprocessid = db.LvNewReq.Where(e => e.EmployeeLeave_Id == empleaveid.Id && e.LeaveHead_Id == lvnewreqid && e.Narration == "Settlement Process").SingleOrDefault();
                var lvnewsettlementprocessclosebal = db.LvNewReq.Include(e => e.EmployeeLeave).Where(e => e.Id >= lvnewsettlementprocessid.Id && e.LeaveHead_Id == lvnewreqid && e.EmployeeLeave_Id == empleaveid.Id).Select(r => new
                {
                    LvCreditDate = r.LvCreditDate,
                    LvCreditNextDate = r.LvCreditNextDate,
                    CloseBal = r.CloseBal,
                    CreditDays = r.CreditDays,
                    OpenBal = r.OpenBal,
                    LvLapsed = r.LvLapsed,
                    Narration = r.Narration,
                    LvnewreqId = r.Id

                }).ToList().OrderByDescending(e => e.LvnewreqId).FirstOrDefault();
                return Json(lvnewsettlementprocessclosebal, JsonRequestBehavior.AllowGet);
            }
        }



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

        public class P2BCrGridData
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }


        }
        public ActionResult LoadEmp(P2BGrid_Parameters gp)
        {
            DataBaseContext db = new DataBaseContext();

            List<int> lvheadids = null;



            try
            {
                DateTime? dt = null;
                string monthyr = "";
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                var jsonData = (Object)null;

                IEnumerable<P2BCrGridData> EmpList = null;
                List<P2BCrGridData> model = new List<P2BCrGridData>();
                P2BCrGridData view = null;

                List<Employee> EmpList1 = new List<Employee>();

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


                var compid = Convert.ToInt32(Session["CompId"].ToString());
                var CompanyPayroll_Id = db.CompanyPayroll.Where(e => e.Company.Id == compid).SingleOrDefault();
                CompanyExit CompanyExit = new CompanyExit();
                CompanyExit = db.CompanyExit.Include(e => e.Company).Where(e => e.Company.Id == compid).FirstOrDefault();

                var empdata = db.CompanyExit.Where(e => e.Company.Id == compid)
                    .Include(e => e.EmployeeExit)
                    .Include(e => e.EmployeeExit.Select(a => a.Employee))
                    .Include(e => e.EmployeeExit.Select(a => a.Employee.EmpName))
                    .Include(e => e.EmployeeExit.Select(a => a.Employee.EmpName))
                    .Include(e => e.EmployeeExit.Select(a => a.Employee.ServiceBookDates))
                    .AsNoTracking().OrderBy(e => e.Id)
                   .SingleOrDefault();

                var emp = empdata.EmployeeExit.ToList();

                foreach (var z in emp)
                {
                    if (z.Employee.ServiceBookDates.FFSCompletionDate == null)
                    {

                        var EmployeeSeperationStruct = db.EmployeeSeperationStruct
                                       .Include(e => e.EmployeeSeperationStructDetails)
                                       .Include(e => e.EmployeeSeperationStructDetails.Select(t => t.SeperationMaster))
                                        .Include(e => e.EmployeeSeperationStructDetails.Select(t => t.SeperationPolicyFormula))
                                        .Include(e => e.EmployeeSeperationStructDetails.Select(t => t.SeperationPolicyFormula.ExitProcess_Config_Policy))
                                       .Include(e => e.EmployeeSeperationStructDetails.Select(t => t.SeperationPolicyAssignment))
                                       .Include(e => e.EmployeeSeperationStructDetails.Select(t => t.SeperationPolicyAssignment.PayScaleAgreement))
                                       .Where(e => e.EmployeeExit.Id == z.Id && e.EndDate == null).AsNoTracking().OrderBy(e => e.Id).FirstOrDefault();


                        view = new P2BCrGridData()
                        {
                            Id = z.Employee.Id,
                            Code = z.Employee.EmpCode,
                            Name = z.Employee.EmpName.FullNameFML
                        };

                        // Resign,EXPIRED,TERMINATION
                        var OOtherServiceBook = db.EmployeePayroll.Where(e => e.Employee.Id == z.Employee.Id)
                        .Include(e => e.OtherServiceBook)
                         .Include(e => e.OtherServiceBook.Select(x => x.OthServiceBookActivity))
                         .Include(e => e.OtherServiceBook.Select(x => x.OthServiceBookActivity.OtherSerBookActList))
                       .SingleOrDefault();
                        var OOOtherServiceBook = OOtherServiceBook.OtherServiceBook.Where(e => e.OthServiceBookActivity.OtherSerBookActList.LookupVal.ToUpper() == "RESIGNED" || e.OthServiceBookActivity.OtherSerBookActList.LookupVal.ToUpper() == "EXPIRED" || e.OthServiceBookActivity.OtherSerBookActList.LookupVal.ToUpper() == "TERMINATION").Select(x => x.OthServiceBookActivity.OtherSerBookActList)
                                                 .FirstOrDefault();
                        if (OOOtherServiceBook != null)
                        {
                            string check = OOOtherServiceBook.LookupVal.ToUpper().ToString();
                            if (check != null)
                            {
                                if (check == "RESIGNED" || check == "EXPIRED" || check == "TERMINATION")
                                {
                                    model.Add(view);
                                    continue;
                                }
                            }
                        }

                        // Retire employee
                        if (EmployeeSeperationStruct != null)
                        {
                            var OEmployeeSeperationStructDet = EmployeeSeperationStruct.EmployeeSeperationStructDetails.Where(r => r.SeperationPolicyFormula.ExitProcess_Config_Policy.Count() > 0).FirstOrDefault();
                            if (OEmployeeSeperationStructDet != null)
                            {
                                var exitpolicyday = OEmployeeSeperationStructDet.SeperationPolicyFormula.ExitProcess_Config_Policy.FirstOrDefault();

                                DateTime actRetdate = Convert.ToDateTime(z.Employee.ServiceBookDates.RetirementDate.Value.AddDays(exitpolicyday.FFSSettlementPeriod_FromLastWorkDay));
                                // FFSSettlementPeriod_FromLastWorkDay +5 days
                                if ((DateTime.Now.Date >= Convert.ToDateTime(z.Employee.ServiceBookDates.RetirementDate.Value.ToString("dd/MM/yyyy")) && DateTime.Now.Date <= actRetdate.Date))
                                {
                                    model.Add(view);
                                }
                                // FFSSettlementPeriod_FromLastWorkDay -5 days
                                // if ((DateTime.Now.Date >= actRetdate.Date && Convert.ToDateTime(z.Employee.ServiceBookDates.RetirementDate.Value.ToString("dd/MM/yyyy")) <= DateTime.Now.Date))
                                if ((DateTime.Now.Date >= actRetdate.Date && DateTime.Now.Date <= Convert.ToDateTime(z.Employee.ServiceBookDates.RetirementDate.Value.ToString("dd/MM/yyyy"))))
                                {
                                    model.Add(view);
                                }
                            }
                            continue;
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
                                || (e.Name.ToUpper().ToString().Contains(gp.searchString.ToUpper()))

                                || (e.Id.ToString().Contains(gp.searchString))
                                ).Select(a => new Object[] { a.Code, a.Name, a.Id }).ToList();
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
                    IE = EmpList;
                    Func<P2BCrGridData, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c =>
                                         gp.sidx == "EmpCode" ? c.Code.ToString() :
                                         gp.sidx == "EmpName" ? c.Name.ToString() : ""


                        );
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.Code), a.Name, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.Code), a.Name, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.Code), a.Name, a.Id }).ToList();
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
        [HttpPost]
        public ActionResult GetLVHEAD(string data, string data2, string Empids)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                int empid = Convert.ToInt32(Empids);
                SelectList s = (SelectList)null;
                var selected = "";

                var query1 = db.EmployeeLeave.Where(e => e.Employee.Id == empid)
                   .Include(e => e.LvNewReq)
                   .Include(e => e.LvNewReq.Select(r => r.LeaveHead))
                   .Include(e => e.LvNewReq.Select(r => r.LeaveCalendar))
                   .SingleOrDefault();

                var lvcalendar = db.Calendar.Where(e => e.Name.LookupVal.ToUpper() == "LEAVECALENDAR" && e.Default == true).FirstOrDefault();

                var CheckingLeavheadFromlvnereq = query1.LvNewReq.Where(x => x.Narration == "Settlement Process").Select(t => t.LeaveHead).Where(e => e.EncashRetirement == true).Distinct().ToList();

                if (CheckingLeavheadFromlvnereq.Count() > 0)
                {
                    //s = new SelectList(CheckingLeavheadFromlvnereq, "Id", "FullDetails", selected);
                    //return Json(s, JsonRequestBehavior.AllowGet);

                    var fun_data = CheckingLeavheadFromlvnereq.Select(e => new
                    {
                        code = e.Id.ToString(),
                        value = e.FullDetails
                    }).ToList();
                    return Json(fun_data, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public class EditData
        {
            public int Id { get; set; }
            public Employee Employee { get; set; }
            public string LvHeadDetails { get; set; }
            public double LvUtilisedInService { get; set; }
            public string LvEntitledInService { get; set; } 
        }

        public ActionResult Edit(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var empresig = db.EmployeeExit.Include(e => e.Employee.EmpName).Include(e => e.SeperationProcessT)
                 .Include(e => e.SeperationProcessT.FFSSettlementDetailT)
                 .Include(e => e.SeperationProcessT.FFSSettlementDetailT.Select(r => r.LvEncashReq))
                 .Include(e => e.SeperationProcessT.FFSSettlementDetailT.Select(r => r.LvEncashReq.LvHead))
                 .AsNoTracking().ToList();

                int Id = int.Parse(data.Split(',')[0]);
                string LvCode = data.Split(',')[1];
                double EntitledDays = 0, UtilisedDays =0;
                foreach (var item in empresig)
                {
                    int EmpLvId = db.EmployeeLeave.Where(e => e.Employee_Id == item.Employee.Id).FirstOrDefault().Id;
                    if (item.SeperationProcessT != null && item.SeperationProcessT.FFSSettlementDetailT != null)
                    {
                        foreach (var item1 in item.SeperationProcessT.FFSSettlementDetailT)
                        {
                            if (item1.Id == Id && item1.LvEncashReq.LvHead.LvCode == LvCode)
                            {
                                EmployeeLvStruct OLvSalStruct = db.EmployeeLvStruct
                          .Include(e => e.EmployeeLvStructDetails)
                          .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHead))
                          .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula))
                          .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvCreditPolicy)) .AsNoTracking()
                          .Where(e => e.EndDate == null && e.EmployeeLeave_Id == EmpLvId).FirstOrDefault();

                                LvCreditPolicy OLvCrPolicy = OLvSalStruct.EmployeeLvStructDetails.Where(e => e.LvHead.Id == item1.LvEncashReq.LvHead_Id && e.LvHeadFormula != null && e.LvHeadFormula.LvCreditPolicy != null).Select(r => r.LvHeadFormula.LvCreditPolicy).FirstOrDefault();
                                
                                if (OLvCrPolicy != null)
                                {
                                    EntitledDays = OLvCrPolicy.FixedCreditDays == true ? OLvCrPolicy.CreditDays : SalaryHeadGenProcess.GovtRound(365 / OLvCrPolicy.WorkingDays, 2); 
                                }

                                var LV_Exits_Original = db.LvNewReq.Include(e => e.LeaveHead).Include(e => e.LvOrignal)
                                                             .Where(e => e.EmployeeLeave_Id == EmpLvId
                                                                && e.LvOrignal != null).AsNoTracking().Select(e => e.LvOrignal.Id).ToList();

                                  UtilisedDays = db.LvNewReq.Where(e => e.EmployeeLeave_Id == EmpLvId &&
                                    e.Narration.ToUpper() == "Leave Requisition".ToUpper() && e.IsCancel == false && e.TrReject == false && LV_Exits_Original.Contains(e.Id) == false).OrderBy(e => e.Id).AsNoTracking().ToList().Sum(e => e.DebitDays);
                                break;
                            }

                        }
                    }
                }

                return Json(new { EntitledDays, UtilisedDays }, JsonRequestBehavior.AllowGet);
            }
             
        }

        public ActionResult EditSave(FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    int FFSSettleId = form["Settle_Id"] == "0" ? 0 : int.Parse(form["Settle_Id"].Split(',')[0]);
                    string LvCode = form["Settle_Id"] == "0" ? "" :  form["Settle_Id"].Split(',')[1];
                    double LvUtitlised = form["LvUtilisedInService"] == "0" ? 0 : Convert.ToDouble(form["LvUtilisedInService"]);
                    double LvEntitled = form["LvEntitledInService"] == "0" ? 0 : Convert.ToDouble(form["LvEntitledInService"]);

                     var empresig = db.EmployeeExit.Include(e => e.Employee.EmpName).Include(e => e.SeperationProcessT)
                 .Include(e => e.SeperationProcessT.FFSSettlementDetailT)
                 .Include(e => e.SeperationProcessT.FFSSettlementDetailT.Select(r => r.LvEncashReq))
                 .Include(e => e.SeperationProcessT.FFSSettlementDetailT.Select(r => r.LvEncashReq.LvHead))
                 .Include(e => e.SeperationProcessT.FFSSettlementDetailT.Select(t => t.LeaveEncashExemptionT))
                 .AsNoTracking().ToList();

        
                foreach (var item in empresig)
                {
                    int EmpLvId = db.EmployeeLeave.Where(e => e.Employee_Id == item.Employee.Id).FirstOrDefault().Id;
                    if (item.SeperationProcessT != null && item.SeperationProcessT.FFSSettlementDetailT != null)
                    {
                        foreach (var item1 in item.SeperationProcessT.FFSSettlementDetailT)
                        {
                            if (item1.Id == FFSSettleId && item1.LvEncashReq.LvHead.LvCode == LvCode)
                            {
                                int SalaryHeadId = db.SalaryHead.Where(e => e.Code.ToUpper() == item1.SalaryHead.ToUpper()).FirstOrDefault().Id;

                                P2BUltimate.Process.PayrollReportGen.LvEncashExemptReturnClass LvEncashExemptReturnClass = PayrollReportGen.LeaveEncashExempt(item.Employee.Id, item1.LvEncashReq.LvHead.Id, item1.PayAmount, SalaryHeadId, 0, LvUtitlised, LvEntitled, LeaveEncashExemptionSettings);
                                DBTrack dbt = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                                using (TransactionScope ts = new TransactionScope())
                                {
                                    List<LvEncashExemptEligibleDays> oLvEncashExemptEligibleDaysList = new List<LvEncashExemptEligibleDays>();
                                    if (db.LvEncashExemptEligibleDays.Any(e => e.LvEncashReq_Id == item1.LvEncashReq.Id))
                                    {
                                        LvEncashExemptEligibleDays OLvEncashExemptEligibleDays = db.LvEncashExemptEligibleDays.Where(e => e.LvEncashReq_Id == item1.LvEncashReq.Id).FirstOrDefault();
                                        OLvEncashExemptEligibleDays.LvEntitledInService = LvEntitled;
                                        OLvEncashExemptEligibleDays.LvUtilisedInService = LvUtitlised;
                                        db.LvEncashExemptEligibleDays.Attach(OLvEncashExemptEligibleDays);
                                        db.Entry(OLvEncashExemptEligibleDays).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                    }
                                    else
                                    {
                                        LvEncashExemptEligibleDays OLvEncashExemptEligibleDays = new LvEncashExemptEligibleDays()
                                        {
                                            LvEncashReq_Id = item1.LvEncashReq.Id,
                                            LvEntitledInService = LvEntitled,
                                            LvUtilisedInService = LvUtitlised,
                                            DBTrack = dbt
                                        }; 
                                        oLvEncashExemptEligibleDaysList.Add(OLvEncashExemptEligibleDays);
                                    }

                                    if (item1.LeaveEncashExemptionT != null)
                                    {
                                        LeaveEncashExemptionT oLeaveEncashExemptionT = db.LeaveEncashExemptionT.Include(e => e.LvEncashExemptDetails)
                                            .Where(e => e.Id == item1.LeaveEncashExemptionT.Id).FirstOrDefault();
                                        db.LvEncashExemptDetails.RemoveRange(oLeaveEncashExemptionT.LvEncashExemptDetails);
                                        oLeaveEncashExemptionT.LvEncashExemptDetails = LvEncashExemptReturnClass.LvEncashExemptDetails;
                                       // oLeaveEncashExemptionT.LvEncashExemptEligibleDays = oLvEncashExemptEligibleDaysList;
                                        item1.DBTrack = new DBTrack
                                        {
                                            CreatedBy = item1.LeaveEncashExemptionT.DBTrack.CreatedBy == null ? null : item1.LeaveEncashExemptionT.DBTrack.CreatedBy,
                                            CreatedOn = item1.LeaveEncashExemptionT.DBTrack.CreatedOn == null ? null : item1.LeaveEncashExemptionT.DBTrack.CreatedOn,
                                            Action = "M",
                                            ModifiedBy = SessionManager.UserName,
                                            ModifiedOn = DateTime.Now
                                        };
                                        db.LeaveEncashExemptionT.Attach(oLeaveEncashExemptionT);
                                        db.Entry(oLeaveEncashExemptionT).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                    }
                                    else
                                    {
                                        LeaveEncashExemptionT OLeaveEncashExemptionT = new LeaveEncashExemptionT()
                                        {
                                            LvEncashExemptDetails = LvEncashExemptReturnClass.LvEncashExemptDetails,
                                            LvEncashExemptEligibleDays = oLvEncashExemptEligibleDaysList,
                                            DBTrack = dbt
                                        };

                                        db.LeaveEncashExemptionT.Add(OLeaveEncashExemptionT);
                                        db.SaveChanges();

                                        FFSSettlementDetailT oFFSSettlementDetailT = db.FFSSettlementDetailT.Find(item1.Id);
                                        oFFSSettlementDetailT.LeaveEncashExemptionT = OLeaveEncashExemptionT;
                                        oFFSSettlementDetailT.LeaveEncashExemptionT.DBTrack = new DBTrack
                                        {
                                            CreatedBy = item1.DBTrack.CreatedBy == null ? null : item1.DBTrack.CreatedBy,
                                            CreatedOn = item1.DBTrack.CreatedOn == null ? null : item1.DBTrack.CreatedOn,
                                            Action = "M",
                                            ModifiedBy = SessionManager.UserName,
                                            ModifiedOn = DateTime.Now
                                        };
                                        db.FFSSettlementDetailT.Attach(oFFSSettlementDetailT);
                                        db.Entry(oFFSSettlementDetailT).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();

                                    }
                                    ts.Complete();
                                }
                              
                                return Json(new { EncashAmt = LvEncashExemptReturnClass.mLvEncashAmount }, JsonRequestBehavior.AllowGet);
                                 
                            }
                        }
                    }
                }

                
                }
                catch (Exception ex)
                {
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

            return null;
        }
    }


    public class LeaveEncashExemptionSettings
    {
        private string OrgType;
        private string ExemptionLimit;
        private string SeperationType;
        private string CertificateA;
        private string CertificateB;
        private string CertificateC;
        private string CertificateD;


        public LeaveEncashExemptionSettings(IDictionary<string, string> settinigs)
        {
            this.OrgType = settinigs.First(x => x.Key.Equals("OrgType")).Value;
            this.ExemptionLimit = settinigs.First(x => x.Key.Equals("ExemptionLimit")).Value;
            this.SeperationType = settinigs.First(x => x.Key.Equals("SeperationType")).Value;
            this.CertificateA = settinigs.First(x => x.Key.Equals("CertificateA")).Value;
            this.CertificateB = settinigs.First(x => x.Key.Equals("CertificateB")).Value;
            this.CertificateC = settinigs.First(x => x.Key.Equals("CertificateC")).Value;
            this.CertificateD = settinigs.First(x => x.Key.Equals("CertificateD")).Value;

        }

        public string DOrgType { get { return OrgType; } }
        public string DExemptionLimit { get { return ExemptionLimit; } }
        public string DSeperationType { get { return SeperationType; } }
        public string DCertificateA { get { return CertificateA; } }
        public string DCertificateB { get { return CertificateB; } }
        public string DCertificateC { get { return CertificateC; } }
        public string DCertificateD { get { return CertificateD; } }

    }

}