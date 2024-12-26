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
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;

namespace P2BUltimate.Controllers.Payroll.MainController
{
    [AuthoriseManger]
    public class FFSSettlementReleaseDetailTController : Controller
    {

        readonly IP2BINI p2BINI;
        readonly LeaveEncashExemptionSettings LeaveEncashExemptionSettings;
        private readonly Type Type;

        public FFSSettlementReleaseDetailTController()
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
            Session.Remove("LeaveData");
            return View("~/Views/Payroll/MainViews/FFSSettlementReleaseDetailT/Index.cshtml");
        }
        public class SalheadData
        {
            public int Id { get; set; }
            public string SalCode { get; set; }
        }

        public class P2BGridData
        {
            public int Id { get; set; }
            public string EmpCode { get; set; }
            public string EmpName { get; set; }
            public string Salcode { get; set; }
            public string SalType { get; set; }
            public double OpenAmount { get; set; }
            public double RelAmount { get; set; }
            public double BalAmount { get; set; }
            public string RelMonth { get; set; }
            // public string ProcessMonth { get; set; }
            public string RelDate { get; set; }


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
                    .Include(e => e.SeperationProcessT.FFSSettlementDetailT.Select(x => x.ProcessType))
                      .Include(e => e.SeperationProcessT.FFSSettlementDetailT.Select(y => y.FFSSettlementReleaseDetailT))
                    .AsNoTracking().ToList();

                foreach (var z in empresig)
                {
                    if (z.SeperationProcessT != null && z.SeperationProcessT.FFSSettlementDetailT != null)
                    {
                        foreach (var item in z.SeperationProcessT.FFSSettlementDetailT)
                        {
                            foreach (var item1 in item.FFSSettlementReleaseDetailT)
                            {

                                view = new P2BGridData()
                                {
                                    Id = item.Id,
                                    EmpCode = z.Employee.EmpCode,
                                    EmpName = z.Employee.EmpName.FullNameFML,
                                    Salcode = item.SalaryHead,
                                    SalType = item.SalType,
                                    OpenAmount = item1.OpenAmount,
                                    BalAmount = item1.balAmount,
                                    RelAmount = item1.RelAmount,
                                    RelMonth = item1.RelMonth.Value.Date.ToShortDateString(),
                                    //   ProcessMonth = item.ProcessMonth != null ? item.ProcessMonth.Value.Date.ToShortDateString() : "",
                                    RelDate = item1.RelDate.Value.Date.ToShortDateString(),

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
                              || (e.Salcode != null ? e.Salcode.ToString().Contains(gp.searchString) : false)
                             || (e.SalType != null ? e.SalType.ToString().Contains(gp.searchString) : false)
                              || (e.OpenAmount != null ? e.OpenAmount.ToString().Contains(gp.searchString) : false)
                               || (e.BalAmount != null ? e.BalAmount.ToString().Contains(gp.searchString) : false)
                                || (e.RelAmount != null ? e.RelAmount.ToString().Contains(gp.searchString) : false)
                               || (e.RelMonth != null ? e.RelMonth.ToString().Contains(gp.searchString) : false)
                            //|| (e.ProcessMonth != null ? e.ProcessMonth.ToString().Contains(gp.searchString) : false)
                               || (e.RelDate != null ? e.RelDate.ToString().Contains(gp.searchString) : false)

                             || (e.Id.ToString().Contains(gp.searchString))
                             ).Select(a => new Object[] { a.EmpCode, a.EmpName, a.Salcode, a.SalType, a.OpenAmount, a.RelAmount, a.BalAmount, a.RelMonth, a.RelDate != null ? Convert.ToString(a.RelDate) : "", a.Id }).ToList();



                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.EmpCode, a.EmpName, a.Salcode, a.SalType, a.OpenAmount, a.RelAmount, a.BalAmount, a.RelMonth, a.RelDate != null ? Convert.ToString(a.RelDate) : "", a.Id }).ToList();
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
                                          gp.sidx == "Salcode" ? c.Salcode.ToString() :
                                           gp.sidx == "SalType" ? c.SalType.ToString() :
                                            gp.sidx == "OpenAmount" ? c.OpenAmount.ToString() :
                                             gp.sidx == "BalAmount" ? c.BalAmount.ToString() :
                                              gp.sidx == "RelAmount" ? c.RelAmount.ToString() :
                                             gp.sidx == "RelMonth" ? c.RelMonth.ToString() :
                                         gp.sidx == "RelDate" ? c.RelDate.ToString() :


                                    "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.EmpCode, a.EmpName, a.Salcode, a.SalType, a.OpenAmount, a.RelAmount, a.BalAmount, a.RelMonth, a.RelDate != null ? Convert.ToString(a.RelDate) : "", a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.EmpCode, a.EmpName, a.Salcode, a.SalType, a.OpenAmount, a.RelAmount, a.BalAmount, a.RelMonth, a.RelDate != null ? Convert.ToString(a.RelDate) : "", a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.EmpCode, a.EmpName, a.Salcode, a.SalType, a.OpenAmount, a.RelAmount, a.BalAmount, a.RelMonth, a.RelDate != null ? Convert.ToString(a.RelDate) : "", a.Id }).ToList();
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
        public static string[] Lvencashpayment(int OEmployeeId, string PayMonth, string ProcessMonth, string Paymentdate, string lvencashidst, LvEncashPayment L)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string[] Msg = new string[2];
                try
                {

                    string lvencashid = lvencashidst == "0" ? "" : lvencashidst;

                    var salhead = db.SalaryHead.Include(e => e.SalHeadOperationType).Where(e => e.SalHeadOperationType.LookupVal.ToUpper() == "LVENCASH").FirstOrDefault();


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
                            ObjID.AmountPaid = PayrollReportGen.LeaveEncashCalc(OEmployeePayroll.Id, ObjID);
                            Msg[1] = Convert.ToString(ObjID.AmountPaid);
                            // As Told by Sir for Settlement Process LvEncashPayment and yearlypayment data save not req.direct save in FFsetlmentdetaist table

                            using (TransactionScope ts = new TransactionScope())
                            {

                                int LvencashId = ObjID.LvEncashReq.Id;
                                LvEncashReq lvupdate = db.LvEncashReq.Include(e => e.WFStatus).Where(e => e.Id == LvencashId).SingleOrDefault();
                                lvupdate.TrClosed = true;
                                lvupdate.WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "3").FirstOrDefault();// db.LookupValue.Where(e => e.LookupVal == "3").SingleOrDefault();
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
        public static string Lvencashreqs(int OEmployeeId, string PayMonth, string Processmonth, string Paymentdate, string lvheadid, LvEncashReq L)
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
                            var encid = db.SalaryHead.Include(e => e.Frequency).Include(e => e.SalHeadOperationType).Where(e => e.SalHeadOperationType.LookupVal.ToUpper() == "LVENCASH")
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
                                var dat1 = CheckEncashSpan.LeaveEncashReq.Where(t => t.LvHead.Id == chk && t.TrReject != true && t.LeaveCalendar.Id == Cal.Id).ToList();
                                if (dat1.Count() == encashpolicy.EncashSpanYear)
                                {

                                    Msg = ("Encashment Span Year yet to complete. " + encashpolicy.EncashSpanYear);
                                    return Msg;
                                    // return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }
                            }
                            else
                            {
                                //var dat1 = CheckEncashSpan.LeaveEncashReq.Where(t => t.LvHead.Id == chk && t.TrReject != true && t.LeaveCalendar.Id==Cal.Id).ToList();
                                DateTime? lvcrdate = _Prv_EmpLvData.LvNewReq.Where(e => e.LeaveHead.Id == chk && e.LvCreditDate != null).Max(e => e.LvCreditDate.Value);
                                if (lvcrdate != null)
                                {
                                    Lvyearfrom = lvcrdate;
                                    LvyearTo = Lvyearfrom.Value.AddYears(1);
                                    LvyearTo = LvyearTo.Value.AddDays(-1);

                                    var dat1 = CheckEncashSpan.LeaveEncashReq.Where(t => t.LvHead.Id == chk && t.TrReject != true && t.LeaveCalendar.Id == Cal.Id && t.FromPeriod >= Lvyearfrom && t.ToPeriod <= LvyearTo).ToList();

                                    if (dat1.Count() == encashpolicy.EncashSpanYear)
                                    {

                                        Msg = ("Encashment Span Year yet to complete. " + encashpolicy.EncashSpanYear);
                                        return Msg;
                                        //return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        //}
                                    }
                                }

                            }
                        }
                        else
                        {

                            var encid = db.SalaryHead.Include(e => e.Frequency).Include(e => e.SalHeadOperationType).Where(e => e.SalHeadOperationType.LookupVal.ToUpper() == "LVENCASH")
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
                    var lvcalendar = db.Calendar.Where(e => e.Name.LookupVal.ToUpper() == "LEAVECALENDAR" && e.Default == true).FirstOrDefault();
                    OBJLVWFDnew.FromPeriod = lvcalendar.FromDate.Value.Date;
                    OBJLVWFDnew.ToPeriod = lvcalendar.ToDate.Value.Date;
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
                    if (encashpolicy.IsLvMultiple == true)
                    {
                        if (OBJLVWFDnew.EncashDays % encashpolicy.LvMultiplier != 0)
                        {
                            Msg = ("You can apply leave encashment multiplier of " + encashpolicy.LvMultiplier);
                            return Msg;
                            // return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }

                    if (encashpolicy.IsOnBalLv == true)
                    {
                        if (OBJLVWFDnew.EncashDays != Math.Round((oLvClosingData * encashpolicy.LvBalPercent / 100) + 0.001, 0))
                        {
                            Msg = ("you can apply Encash days balance leave of " + encashpolicy.LvBalPercent + " percent.");
                            return Msg;
                            //return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }

                    if (OBJLVWFDnew.EncashDays < encashpolicy.MinEncashment)
                    {
                        Msg = (" Encash days should be more than  " + encashpolicy.MinEncashment);
                        return Msg;
                        // return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    if (OBJLVWFDnew.EncashDays > encashpolicy.MaxEncashment)
                    {
                        Msg = (" Encash days cannot more than  " + encashpolicy.MaxEncashment);
                        return Msg;
                        //return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }



                    if (UtilizedLv < encashpolicy.MinUtilized)
                    {
                        Msg = ("For Encashment minimum utilization less than  " + encashpolicy.MinUtilized);
                        return Msg;
                        //return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    if (CompCode == "KDCC")
                    {
                        if ((oLvClosingData - OBJLVWFDnew.EncashDays) < encashpolicy.MinBalance)
                        {
                            Msg = ("For Encashment minimum balance require  " + encashpolicy.MinBalance);
                            return Msg;
                            //return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        if (oLvClosingData < encashpolicy.MinBalance)
                        {
                            Msg = ("For Encashment minimum balance require  " + encashpolicy.MinBalance);
                            return Msg;
                            // return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }



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
                        WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "0").FirstOrDefault(),//db.LookupValue.Where(e => e.LookupVal == "0").SingleOrDefault(),

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
                                            WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "2").FirstOrDefault(),//db.LookupValue.Where(e => e.LookupVal == "2").SingleOrDefault(),

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
        //  public ActionResult Create(FFSSettlementReleaseDetailT S, FormCollection form, string forwarddata1, string empid1)

        public class EmpLeaveDataClass
        {
          
            public string LeaveHeadId { get; set; }
            public string LeaveCode { get; set; }
            public string OpenBal { get; set; }
            public string RelAmount { get; set; } 
        }

        public object Save(string form, string LvCode) //Create submit
        {
            List<EmpLeaveDataClass> OEmpLeaveDataClass = new List<EmpLeaveDataClass>();
            if (Session["LeaveData"] != null)
            {
                OEmpLeaveDataClass = (List<EmpLeaveDataClass>)Session["LeaveData"];
            }
            EmpLeaveDataClass OEmpLeaveDataClassNew = new EmpLeaveDataClass();
            string[] values = (form.Split(new string[] { "&" }, StringSplitOptions.None));
            if (values != null)
            {
                
                OEmpLeaveDataClassNew = new EmpLeaveDataClass()
                {
                    LeaveHeadId = values[4].Split('=')[1],
                    LeaveCode = LvCode,
                    OpenBal = values[5].Split('=')[1],
                    RelAmount = values[7].Split('=')[1]
                };
                OEmpLeaveDataClass.Add(OEmpLeaveDataClassNew);
            }
            Session["LeaveData"] = OEmpLeaveDataClass;
            return Json(OEmpLeaveDataClass, JsonRequestBehavior.AllowGet);
        }
 
        public ActionResult Create(FFSSettlementReleaseDetailT S, FormCollection form, String forwarddata) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    string Emp = forwarddata == "0" ? "" : forwarddata;
                    string Lvhead = form["LvHead"] == "" ? "0" : Convert.ToString(form["LvHead"]);
                    List<int> LvHead_ids = null;
                    if (Lvhead != null && Lvhead != "0")
                    {
                        LvHead_ids = Utility.StringIdsToListIds(Lvhead);
                    }
                    List<int> ids = null;
                    if (Emp != "null" && Emp != "0" && Emp != "false")
                    {
                        ids = one_ids(Emp);
                    }
                    else
                    {
                        Msg.Add("  Kindly select employee  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    string ProcessMonth = form["Create_Processmonth"] == "0 " ? "" : form["Create_Processmonth"];
                    string PayMonth = form["Create_Paymonth"] == "0" ? "" : form["Create_Paymonth"];
                    if (string.IsNullOrEmpty(PayMonth))
                    {
                        Msg.Add(" Kindly Select Pay Month ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    string Balamt = form["BalAmount"] == "0" ? "" : form["BalAmount"];
                    if (string.IsNullOrEmpty(Balamt))
                    {
                        Msg.Add("Your have Release all Amount ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    //if (!string.IsNullOrEmpty(ViewBag["LvHeadCount"]))
                    //{
                    //    int count = ViewBag["LvHeadCount"];
                    //}

                    string Relamt = form["RelAmount"] == "0" ? "" : form["RelAmount"];
                    string Openbal = form["OpenAmount"] == "0" ? "" : form["OpenAmount"];
                    S.OpenAmount = Convert.ToDouble(Openbal);
                    S.RelAmount = Convert.ToDouble(Relamt);
                    string Paymentdate = form["Create_Paymentdate"] == "0" ? "" : form["Create_Paymentdate"];
                    if (string.IsNullOrEmpty(Paymentdate))
                    {
                        Msg.Add(" Kindly Select Payment date ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    int SalaryHead = form["SalaryHeadlist"] == "0" ? 0 : Convert.ToInt32(form["SalaryHeadlist"]);
                    string fromdate = form["fromdate"] == "0 " ? "" : form["fromdate"];
                    string Todate = form["Todate"] == "0 " ? "" : form["Todate"];

                    var salaryheadret = db.SalaryHead.Include(e => e.SalHeadOperationType).Where(e => e.Id == SalaryHead).FirstOrDefault();
                    Employee OEmployee = null;
                    EmployeePayroll OEmployeePayroll = null;

                   

                    if (!string.IsNullOrEmpty(PayMonth))
                    {
                        S.RelMonth = Convert.ToDateTime(PayMonth);
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
                            OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.GeoStruct.Company).Include(e => e.FuncStruct).Include(e => e.PayStruct)
                                 .Include(e => e.ServiceBookDates)
                                   .Where(r => r.Id == i).SingleOrDefault();

                            OEmployeePayroll = db.EmployeePayroll
                          .Where(e => e.Employee.Id == i).SingleOrDefault();
                     
                           
                            List<EmpLeaveDataClass> OEmpLeaveDataClass = new List<EmpLeaveDataClass>();
                            if (salaryheadret.SalHeadOperationType.LookupVal.ToUpper() == "LVENCASHEXEMPTED")
                            {
                                if (Session["LeaveData"] != null)
                                {
                                    OEmpLeaveDataClass = (List<EmpLeaveDataClass>)Session["LeaveData"];
                                    S.RelAmount = OEmpLeaveDataClass.Sum(e => Convert.ToDouble(e.RelAmount));
                                    S.OpenAmount = OEmpLeaveDataClass.Sum(e => Convert.ToDouble(e.OpenBal));
                                }
                                else
                                {
                                    Msg.Add(" Kindly Save Data For LVENCASHEXEMPTED ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }
                            }
                           
                               

                            FFSSettlementReleaseDetailT ObjFAT = new FFSSettlementReleaseDetailT();
                            {
                                ObjFAT.OpenAmount = S.OpenAmount;
                                ObjFAT.RelMonth = S.RelMonth;
                                ObjFAT.RelAmount = S.RelAmount;
                                ObjFAT.balAmount = S.OpenAmount - S.RelAmount;
                                ObjFAT.RelDate = Convert.ToDateTime(Paymentdate);
                                ObjFAT.DBTrack = S.DBTrack;

                            }
                            using (TransactionScope ts = new TransactionScope())
                            {
                                try
                                {

                                    List<FFSSettlementReleaseDetailT> OFFSList = new List<FFSSettlementReleaseDetailT>();
                                    db.FFSSettlementReleaseDetailT.Add(ObjFAT);
                                    db.SaveChanges();
                                    OFFSList.Add(ObjFAT);
                                    var aa = db.EmployeeExit.Include(x => x.SeperationProcessT)
                                        .Include(e => e.SeperationProcessT.FFSSettlementDetailT)
                                         .Include(e => e.SeperationProcessT.FFSSettlementDetailT.Select(y => y.FFSSettlementReleaseDetailT))
                                        .Where(e => e.Employee.Id == i).FirstOrDefault();

                                    var salheadprocess = aa.SeperationProcessT.FFSSettlementDetailT.Where(e => e.SalaryHead == salaryheadret.Code).FirstOrDefault();
                                    FFSSettlementDetailT ffsdet = db.FFSSettlementDetailT.Find(salheadprocess.Id);

                                    if (ffsdet == null)
                                    {
                                        FFSSettlementDetailT OTEP = new FFSSettlementDetailT()
                                        {
                                            FFSSettlementReleaseDetailT = OFFSList,
                                            DBTrack = S.DBTrack
                                        };
                                        db.FFSSettlementDetailT.Add(OTEP);
                                        db.SaveChanges();

                                        ffsdet.FFSSettlementReleaseDetailT = OFFSList;
                                        db.FFSSettlementDetailT.Attach(ffsdet);
                                        db.Entry(ffsdet).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                        db.Entry(ffsdet).State = System.Data.Entity.EntityState.Detached;

                                    }
                                    else
                                    {
                                        OFFSList.AddRange(ffsdet.FFSSettlementReleaseDetailT);
                                        ffsdet.FFSSettlementReleaseDetailT = OFFSList;
                                        db.FFSSettlementDetailT.Attach(ffsdet);
                                        db.Entry(ffsdet).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                        db.Entry(ffsdet).State = System.Data.Entity.EntityState.Detached;
                                    }

                                    // yearly payment start
                                    List<YearlyPaymentT> OFAT = new List<YearlyPaymentT>();
                                    List<ITSection10Payment> OFAT1 = new List<ITSection10Payment>();
                                    YearlyPaymentT Y = new YearlyPaymentT();

                                    var fyyr = db.Calendar.Include(a => a.Name).Where(a => a.Name.LookupVal.ToUpper() == "FINANCIALYEAR" && a.Default == true).SingleOrDefault();
                                    Y.FinancialYear = fyyr;

                                    var Val = db.SalaryHead.Find((SalaryHead));
                                    Y.SalaryHead = Val;
                                    Y.FromPeriod = fyyr.FromDate;
                                    Y.ToPeriod = fyyr.ToDate;
                                    Y.ProcessMonth = PayMonth;
                                    Y.ReleaseDate = Convert.ToDateTime(Paymentdate);
                                    Y.PayMonth = PayMonth;
                                    Y.AmountPaid = 0;
                                    Y.TDSAmount = 0;
                                    Y.OtherDeduction = 0;
                                    Y.LvEncashReq = null;
                                    Y.Narration = "Settlement Process";
                                    Y.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                                    YearlyPaymentT ObjYPT = new YearlyPaymentT();
                                    {
                                        ObjYPT.SalaryHead = Y.SalaryHead;
                                        ObjYPT.FromPeriod = Y.FromPeriod;
                                        ObjYPT.ToPeriod = Y.ToPeriod;
                                        ObjYPT.ProcessMonth = Y.ProcessMonth;
                                        ObjYPT.PayMonth = Y.PayMonth;
                                        ObjYPT.ReleaseDate = Y.ReleaseDate;
                                        ObjYPT.ReleaseFlag = true;
                                        ObjYPT.TDSAmount = Y.TDSAmount;
                                        ObjYPT.LvEncashReq = Y.LvEncashReq;
                                        ObjYPT.OtherDeduction = Y.OtherDeduction;
                                        ObjYPT.Narration = Y.Narration;
                                        ObjYPT.FinancialYear = Y.FinancialYear;
                                        ObjYPT.DBTrack = Y.DBTrack;

                                    }
                                    ObjYPT.GeoStruct_Id = OEmployee.GeoStruct_Id;  //db.GeoStruct.Find(OEmployee.GeoStruct.Id);

                                    ObjYPT.FuncStruct_Id = OEmployee.FuncStruct_Id; //db.FuncStruct.Find(OEmployee.FuncStruct.Id);

                                    ObjYPT.PayStruct_Id = OEmployee.PayStruct_Id;//db.PayStruct.Find(OEmployee.PayStruct == null ? 0 : OEmployee.PayStruct.Id);

                                    ObjYPT.AmountPaid = S.RelAmount;
                                    db.YearlyPaymentT.Add(ObjYPT);
                                    db.SaveChanges();

                                    if (salaryheadret.SalHeadOperationType.LookupVal.ToUpper() == "LVENCASHEXEMPTED")
                                    {
                                        if ( Session["LeaveData"] !=null)
                                        {
                                            double LvUtilisedInService = 0, LvEntitledInService = 0, RelAmount = 0;
                                            List<int> Lvexemptlist = db.LvHead.Where(e => e.ExemptEncashAmtRetirement == true).Select(e => e.Id).ToList();
                                            foreach (int LvHeadId in Lvexemptlist)
                                            {

                                                foreach (var item in OEmpLeaveDataClass)
                                                {
                                                    if (LvHeadId == Convert.ToInt32(item.LeaveHeadId))
                                                    {
                                                        var query1 = db.EmployeeExit.Where(e => e.Id == aa.Id).Select(t => t.SeperationProcessT.FFSSettlementDetailT.Select(y => y.LvEncashReq)).FirstOrDefault();
                                                        int? LvEnchashReq_Id = query1.Where(t => t.LvHead_Id == LvHeadId).FirstOrDefault().Id;

                                                        LvEncashExemptEligibleDays getdays = db.LvEncashExemptEligibleDays.Where(e => e.LvEncashReq_Id == LvEnchashReq_Id).FirstOrDefault();
                                                        LvUtilisedInService += getdays.LvUtilisedInService;
                                                        LvEntitledInService += getdays.LvEntitledInService;
                                                        RelAmount += Convert.ToDouble(item.RelAmount);
                                                    }

                                                }
                                            }
                                            ITSection oITSection = db.ITSection.Include(e => e.ITSectionListType).Where(e => e.ITSectionListType.LookupVal.ToUpper() == "SECTION10B").FirstOrDefault();
                                            List<ITSection10> oITSection10 = db.ITSection10.Include(e => e.Itsection10salhead).Include(e => e.Itsection10salhead.Select(t=> t.SalHead))
                                                .Include(e => e.Itsection10salhead.Select(t => t.SalHead.SalHeadOperationType)).ToList();
                                            int ITSection10_Id = 0;
                                            foreach (var item10 in oITSection10)
                                            {
                                                if (item10.Itsection10salhead.Where(e => e.SalHead.SalHeadOperationType.LookupVal.ToUpper() == "LVENCASHEXEMPTED").FirstOrDefault() != null)
                                                {
                                                    ITSection10_Id = item10.Id;
                                                    break;
                                                }
                                            }


                                            var query = db.EmployeeExit.Include(e => e.SeperationProcessT.FFSSettlementDetailT)
                                                    .Include(e => e.SeperationProcessT.FFSSettlementDetailT.Select(t => t.LeaveEncashExemptionT))
                                                    .Include(e => e.SeperationProcessT.FFSSettlementDetailT.Select(t => t.LeaveEncashExemptionT.LvEncashExemptDetails))
                                                    .Include(e => e.SeperationProcessT.FFSSettlementDetailT.Select(t => t.LeaveEncashExemptionT.LvEncashExemptEligibleDays))
                                                     .Include(e => e.SeperationProcessT.FFSSettlementDetailT.Select(t => t.FFSSettlementReleaseDetailT))
                                                     
                                                .Where(e => e.Employee.Id == i).FirstOrDefault();

                                            if (db.ITSection10Payment.Any(e => e.EmployeePayroll_Id == OEmployeePayroll.Id && e.ITSection10_Id == ITSection10_Id))
                                            {
                                                

                                                if (query != null)
                                                {
                                                    foreach (var item in query.SeperationProcessT.FFSSettlementDetailT)
                                                    {
                                                        RelAmount += item.FFSSettlementReleaseDetailT.Sum(e => e.RelAmount);
                                                        LvUtilisedInService += Math.Round(item.LeaveEncashExemptionT.LvEncashExemptEligibleDays.Where(e => e.LvEncashReq_Id == item.LvEncashReq.Id).Sum(e => e.LvUtilisedInService),2);
                                                        LvEntitledInService += Math.Round(item.LeaveEncashExemptionT.LvEncashExemptEligibleDays.Where(e => e.LvEncashReq_Id == item.LvEncashReq.Id).Sum(e => e.LvEntitledInService), 2);
                                                    }
                                                }

                                                P2BUltimate.Process.PayrollReportGen.LvEncashExemptReturnClass LvEncashExemptReturnClass = PayrollReportGen.LeaveEncashExempt(i, (int)ObjYPT.LvEncashReq.LvHead_Id, RelAmount, int.Parse(ObjYPT.SalaryHead_Id.Value.ToString()), int.Parse(ObjYPT.FinancialYear_Id.Value.ToString()), LvUtilisedInService, LvEntitledInService, LeaveEncashExemptionSettings);
                                                ITSection10Payment OITSection10Pay = db.ITSection10Payment.Where(e => e.EmployeePayroll_Id == OEmployeePayroll.Id && e.FinancialYear_Id == Y.FinancialYear_Id && e.ITSection10_Id == ITSection10_Id).FirstOrDefault();
                                                DBTrack dbt = new DBTrack
                                                {
                                                    CreatedBy = OITSection10Pay.DBTrack.CreatedBy == null ? null : OITSection10Pay.DBTrack.CreatedBy,
                                                    CreatedOn = OITSection10Pay.DBTrack.CreatedOn == null ? null : OITSection10Pay.DBTrack.CreatedOn,
                                                    Action = "M",
                                                    ModifiedBy = SessionManager.UserName,
                                                    ModifiedOn = DateTime.Now
                                                };
                                                if (OITSection10Pay != null)
                                                {
                                                    OITSection10Pay.ActualInvestment = LvEncashExemptReturnClass.mLvEncashAmount;
                                                    OITSection10Pay.DeclaredInvestment = LvEncashExemptReturnClass.mLvEncashAmount;
                                                    OITSection10Pay.DBTrack = dbt;
                                                    db.ITSection10Payment.Attach(OITSection10Pay);
                                                    db.Entry(OITSection10Pay).State = System.Data.Entity.EntityState.Modified;
                                                    db.SaveChanges();
                                                }
                                                if (query != null)
                                                {
                                                    foreach (var item in query.SeperationProcessT.FFSSettlementDetailT)
                                                    {
                                                        if (item.LeaveEncashExemptionT != null)
                                                        {
                                                            LeaveEncashExemptionT oLeaveEncashExemptionT = db.LeaveEncashExemptionT.Include(e => e.LvEncashExemptDetails)
                                                                .Where(e => e.Id == item.LeaveEncashExemptionT.Id).FirstOrDefault();
 
                                                            LvEncashExemptDetails oLvEncashExemptDetails = oLeaveEncashExemptionT.LvEncashExemptDetails.Where(e => e.PickupId == 5).FirstOrDefault();
                                                            if (oLvEncashExemptDetails != null)
                                                            {
                                                                oLvEncashExemptDetails.ActualAmount = LvEncashExemptReturnClass.LvEncashExemptDetails.Where(e => e.PickupId == 5).FirstOrDefault().ActualAmount;
                                                                db.LvEncashExemptDetails.Attach(oLvEncashExemptDetails);
                                                                db.Entry(oLvEncashExemptDetails).State = System.Data.Entity.EntityState.Modified;
                                                                db.SaveChanges();

                                                            }

                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                P2BUltimate.Process.PayrollReportGen.LvEncashExemptReturnClass LvEncashExemptReturnClass = PayrollReportGen.LeaveEncashExempt(i, Lvexemptlist.FirstOrDefault(), RelAmount, int.Parse(ObjYPT.SalaryHead_Id.Value.ToString()), int.Parse(ObjYPT.FinancialYear_Id.Value.ToString()), LvUtilisedInService, LvEntitledInService, LeaveEncashExemptionSettings);
                                                ITSection10Payment ITSection10Pay = new ITSection10Payment()
                                                {
                                                    FinancialYear = Y.FinancialYear,
                                                    InvestmentDate = DateTime.Now,
                                                    ITSection_Id = oITSection.Id,
                                                    ITSection10_Id = ITSection10_Id,
                                                    Narration = "Leave Encash Exemption",
                                                    ActualInvestment = LvEncashExemptReturnClass.mLvEncashAmount,
                                                    DeclaredInvestment = LvEncashExemptReturnClass.mLvEncashAmount,
                                                    DBTrack = S.DBTrack,
                                                    EmployeePayroll_Id = OEmployeePayroll.Id
                                                };
                                                db.ITSection10Payment.Add(ITSection10Pay);
                                                db.SaveChanges();
                                                if (query != null)
                                                {
                                                    foreach (var item in query.SeperationProcessT.FFSSettlementDetailT)
                                                    {
                                                        if (item.LeaveEncashExemptionT != null)
                                                        {
                                                            LeaveEncashExemptionT oLeaveEncashExemptionT = db.LeaveEncashExemptionT.Include(e => e.LvEncashExemptDetails)
                                                                .Where(e => e.Id == item.LeaveEncashExemptionT.Id).FirstOrDefault();

                                                            //if (oLeaveEncashExemptionT != null)
                                                            //{
                                                            //    List<LvEncashExemptDetails> OlvExemptList = new List<LvEncashExemptDetails>();
                                                            //    OlvExemptList.AddRange(oLeaveEncashExemptionT.LvEncashExemptDetails);
                                                            //    OlvExemptList.AddRange(LvEncashExemptReturnClass.LvEncashExemptDetails);
                                                            //    oLeaveEncashExemptionT.LvEncashExemptDetails = OlvExemptList; 
                                                            //    db.LeaveEncashExemptionT.Attach(oLeaveEncashExemptionT);
                                                            //    db.Entry(oLeaveEncashExemptionT).State = System.Data.Entity.EntityState.Modified;
                                                            //    db.SaveChanges();

                                                            //}
                                                            //break;
                                                            LvEncashExemptDetails oLvEncashExemptDetails = oLeaveEncashExemptionT.LvEncashExemptDetails.Where(e => e.PickupId == 5).FirstOrDefault();
                                                            if (oLvEncashExemptDetails != null)
                                                            {
                                                                oLvEncashExemptDetails.ActualAmount = LvEncashExemptReturnClass.LvEncashExemptDetails.Where(e => e.PickupId == 5).FirstOrDefault().ActualAmount;
                                                                db.LvEncashExemptDetails.Attach(oLvEncashExemptDetails);
                                                                db.Entry(oLvEncashExemptDetails).State = System.Data.Entity.EntityState.Modified;
                                                                db.SaveChanges();

                                                            }
                                                             
                                                        }
                                                    }
                                                }
                                                
                                            }

                                        }
                                    }
                      
                                    





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
                                        var aay = db.EmployeePayroll.Find(OEmployeePayroll.Id);
                                        aay.YearlyPaymentT = OFAT; 
                                        db.EmployeePayroll.Attach(aay);
                                        db.Entry(aay).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                        db.Entry(aay).State = System.Data.Entity.EntityState.Detached;

                                    }
                                    ts.Complete();
                                }
                                catch (Exception ex)
                                {
                                    //   List<string> Msg = new List<string>();
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
                                    Session.Remove("LeaveData");
                                    //return Json(new { sucess = false, Msg }, JsonRequestBehavior.AllowGet);
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }


                            }

                        }
                        // List<string> Msgs = new List<string>();
                        Session.Remove("LeaveData");
                        Msg.Add("Data Saved successfully");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //return this.Json(new { success = true, responseText = "Data Saved Successfully...", JsonRequestBehavior.AllowGet });
                    }
                    // List<string> Msg = new List<string>();
                    Session.Remove("LeaveData");
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

        public JsonResult ChkExemption(int data1)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                
                var qurey = db.EmployeeExit.Include(e => e.SeperationProcessT.FFSSettlementDetailT).Include(e => e.SeperationProcessT.FFSSettlementDetailT.Select(t => t.LeaveEncashExemptionT))
                    .Where(e => e.Employee.Id == data1).FirstOrDefault();
                if (qurey != null)
                {
                    if (qurey.SeperationProcessT.FFSSettlementDetailT.Any(e => e.LeaveEncashExemptionT != null))
                    {
                        return Json(true, JsonRequestBehavior.AllowGet);
                    }
                    else
                    { return Json(false, JsonRequestBehavior.AllowGet); }

                   
                }
                else
                {
                    return Json(false, JsonRequestBehavior.AllowGet);
                }

            }
        }

        

        [HttpPost]
        public JsonResult lvheadlist(string data, string data2, string Empid)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                int empid = Convert.ToInt32(Empid);
                int id = Convert.ToInt32(data);
                SelectList s = null;
                var query1 = db.EmployeeExit.Where(e => e.Employee.Id == empid)
                    .Include(e => e.Employee.EmpName).Include(e => e.SeperationProcessT)
                    .Include(e => e.SeperationProcessT.FFSSettlementDetailT)
                    .Include(e => e.SeperationProcessT.FFSSettlementDetailT)
                    .Include(e => e.SeperationProcessT.FFSSettlementDetailT.Select(a => a.LvEncashReq))
                    .Include(e => e.SeperationProcessT.FFSSettlementDetailT.Select(a => a.LvEncashReq.LvHead))
               .SingleOrDefault();

                var CheckingLeavheadFromlvnereq = query1.SeperationProcessT.FFSSettlementDetailT.Where(a => a.SalaryHead == db.SalaryHead.Where(e => e.Id == id).SingleOrDefault().Code)
                    .ToList().Select(r => r.LvEncashReq.LvHead);
                //ViewBag["LvHeadCount"] = CheckingLeavheadFromlvnereq.Count();
                if (CheckingLeavheadFromlvnereq != null)
                {
                    s = new SelectList(CheckingLeavheadFromlvnereq, "Id", "LvCode");
                    return Json(s, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
            }
        }


        public JsonResult LeaveheadopencloseBalance(string leave, string EmployeeId)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var leaveids = leave == "" ? "0" : leave;
                int empid = Convert.ToInt32(EmployeeId);
                int lvnewreqid = Convert.ToInt32(leaveids);
                var query1 = db.EmployeeExit.Where(e => e.Employee.Id == empid)
                    .Include(e => e.Employee.EmpName).Include(e => e.SeperationProcessT)
                    .Include(e => e.SeperationProcessT.FFSSettlementDetailT)
                    .Include(e => e.SeperationProcessT.FFSSettlementDetailT)
                     .Include(e => e.SeperationProcessT.FFSSettlementDetailT.Select(x => x.ProcessType))
                    .Include(e => e.SeperationProcessT.FFSSettlementDetailT.Select(a => a.LvEncashReq))
                    .Include(e => e.SeperationProcessT.FFSSettlementDetailT.Select(a => a.LvEncashReq.LvHead))
                     .Include(e => e.SeperationProcessT.FFSSettlementDetailT.Select(y => y.FFSSettlementReleaseDetailT))
               .SingleOrDefault();
                var empleaveid = db.EmployeeLeave.Where(r => r.Employee_Id == empid).SingleOrDefault();
                var lvheadname = db.LvNewReq.Where(e => e.EmployeeLeave_Id == empleaveid.Id && e.LeaveHead_Id == lvnewreqid && e.Narration == "Leave Encash Payment").SingleOrDefault();
                if (query1 != null && query1.SeperationProcessT != null && query1.SeperationProcessT.FFSSettlementDetailT != null)
                {
                    var salheadprocess = query1.SeperationProcessT.FFSSettlementDetailT.FirstOrDefault();
                    if (salheadprocess != null)
                    {
                        var salheadrelease = salheadprocess.FFSSettlementReleaseDetailT.OrderByDescending(e => e.Id).FirstOrDefault();
                        if (salheadrelease != null)
                        {
                            var salhead = salheadrelease;

                            return Json(salhead, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            var salhead = query1.SeperationProcessT.FFSSettlementDetailT.Where(e => e.LvEncashReq.LvHead.Id == lvnewreqid)
                                                      .Select(e => new
                                                      {

                                                          OpenAmount = e.PayAmount,
                                                          balAmount = e.PayAmount
                                                      })
                                                     .FirstOrDefault();
                            return Json(salhead, JsonRequestBehavior.AllowGet);
                        }

                    }

                }
                else
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }

                return Json(null, JsonRequestBehavior.AllowGet);
                //var salhead = query1.SeperationProcessT.FFSSettlementDetailT.Where(e => e.LvEncashReq.LvHead.Id == lvnewreqid).Select(e => new
                // {
                //     OpenAmount = e.PayAmount,
                //     balAmount = e.PayAmount,
                //     //RelAmount = e.PayAmount
                // }).FirstOrDefault();
                //return Json(salhead, JsonRequestBehavior.AllowGet);
            }
        }


        public JsonResult salheadreleaseamountcalc(string data, string data1)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                int Id = int.Parse(data);
                int EId = int.Parse(data1);
                var qurey = db.SalaryHead.Where(e => e.Id == Id).SingleOrDefault();
                if (qurey != null)
                {
                    var empresig = db.EmployeeExit.Include(e => e.Employee.EmpName).Include(e => e.SeperationProcessT)
                   .Include(e => e.SeperationProcessT.FFSSettlementDetailT)
                   .Include(e => e.SeperationProcessT.FFSSettlementDetailT.Select(x => x.ProcessType))
                    .Include(e => e.SeperationProcessT.FFSSettlementDetailT.Select(y => y.FFSSettlementReleaseDetailT))
                     .Where(e => e.Employee.Id == EId)
                  .FirstOrDefault();

                    if (empresig != null && empresig.SeperationProcessT != null && empresig.SeperationProcessT.FFSSettlementDetailT != null)
                    {
                        var salheadprocess = empresig.SeperationProcessT.FFSSettlementDetailT.Where(e => e.SalaryHead == qurey.Code).FirstOrDefault();
                        if (salheadprocess != null)
                        {
                            var salheadrelease = salheadprocess.FFSSettlementReleaseDetailT.OrderByDescending(e => e.Id).FirstOrDefault();
                            if (salheadrelease != null)
                            {
                                var salhead = salheadrelease;

                                return Json(salhead, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                var salhead = empresig.SeperationProcessT.FFSSettlementDetailT.Where(e => e.SalaryHead == qurey.Code)
                                                          .Select(e => new
                                                          {

                                                              OpenAmount = e.PayAmount,
                                                              balAmount = e.PayAmount
                                                          })
                                                         .FirstOrDefault();
                                return Json(salhead, JsonRequestBehavior.AllowGet);
                            }

                        }

                    }
                    else
                    {
                        return Json(null, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }

            }
            return Json(null, JsonRequestBehavior.AllowGet);
        }

        //public JsonResult GetSalHead_old(string data,string data1)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        int Id = int.Parse(data);
        //        int EId = int.Parse(data1);
        //        var qurey = db.SalaryHead.Where(e => e.Id == Id).SingleOrDefault();
        //        if (qurey != null)
        //        {
        //            var empresig = db.EmployeeExit.Include(e => e.Employee.EmpName).Include(e => e.SeperationProcessT)
        //           .Include(e => e.SeperationProcessT.FFSSettlementDetailT)
        //           .Include(e => e.SeperationProcessT.FFSSettlementDetailT.Select(x => x.ProcessType))
        //            .Include(e => e.SeperationProcessT.FFSSettlementDetailT.Select(y => y.FFSSettlementReleaseDetailT))
        //             .Where(e => e.Employee.Id == EId)
        //          .FirstOrDefault();

        //            if (empresig != null && empresig.SeperationProcessT != null && empresig.SeperationProcessT.FFSSettlementDetailT != null)
        //            {
        //                var salheadprocess = empresig.SeperationProcessT.FFSSettlementDetailT.Where(e => e.SalaryHead == qurey.Code).FirstOrDefault();
        //                if (salheadprocess!=null)
        //                {
        //                    var salheadrelease = salheadprocess.FFSSettlementReleaseDetailT.OrderByDescending(e => e.Id).FirstOrDefault();
        //                    if (salheadrelease!=null)
        //                    {
        //                        var salhead = salheadrelease;

        //                        return Json(salhead, JsonRequestBehavior.AllowGet);
        //                    }
        //                    else
        //                    {
        //                        var salhead = empresig.SeperationProcessT.FFSSettlementDetailT.Where(e => e.SalaryHead == qurey.Code)
        //                                                  .Select(e => new
        //                                                  {

        //                                                      OpenAmount = e.PayAmount,
        //                                                      balAmount = e.PayAmount
        //                                                  })
        //                                                 .FirstOrDefault();
        //                        return Json(salhead, JsonRequestBehavior.AllowGet);
        //                    }

        //                }



        //            }
        //            else
        //            {
        //                return Json(null, JsonRequestBehavior.AllowGet);
        //            }

        //          //  var salheadoper = qurey.SalHeadOperationType.LookupVal.ToUpper();

        //           // return Json(salheadoper, JsonRequestBehavior.AllowGet);
        //        }
        //        else
        //        {
        //            return Json(null, JsonRequestBehavior.AllowGet);
        //        }

        //    }
        //    return Json(null, JsonRequestBehavior.AllowGet);
        //}


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

        public ActionResult PopulateDropDownList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var selected = (Object)null;
                IEnumerable<SalheadData> SalaryHeadList = null;
                List<SalheadData> model = new List<SalheadData>();
                SalheadData Salview = null;
                int Id = Convert.ToInt32(data);
                var empresig = db.EmployeeExit.Include(e => e.Employee.EmpName).Include(e => e.SeperationProcessT)
                   .Include(e => e.SeperationProcessT.FFSSettlementDetailT)
                   .Include(e => e.SeperationProcessT.FFSSettlementDetailT.Select(x => x.ProcessType))
                     .Where(e => e.Employee.Id == Id)
                  .FirstOrDefault();
                if (empresig != null && empresig.SeperationProcessT != null && empresig.SeperationProcessT.FFSSettlementDetailT != null)
                {
                    var salhead = empresig.SeperationProcessT.FFSSettlementDetailT.Select(e => e.SalaryHead).Distinct();
                    foreach (var item in salhead)
                    {
                        var salcode = db.SalaryHead.Where(e => e.Code == item).FirstOrDefault();
                        if (salcode != null)
                        {
                            Salview = new SalheadData()
                            {
                                Id = salcode.Id,
                                SalCode = salcode.Code
                            };
                            model.Add(Salview);
                        }
                    }
                    SalaryHeadList = model;

                    IEnumerable<SalheadData> IE;
                    IE = SalaryHeadList;
                    if (IE.Count() > 0)
                    {
                        SelectList s = new SelectList(IE, "Id", "SalCode", selected);

                        return Json(s, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(null, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }


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
                    .Include(e => e.EmployeeExit.Select(a => a.Employee.ServiceBookDates))
                    .Include(e => e.EmployeeExit.Select(a => a.SeperationProcessT))
                    .Include(e => e.EmployeeExit.Select(a => a.SeperationProcessT.FFSSettlementDetailT))
                    .AsNoTracking()
                   .SingleOrDefault();

                var emp = empdata.EmployeeExit.ToList();

                foreach (var z in emp)
                {
                    if (z.SeperationProcessT != null && z.SeperationProcessT.FFSSettlementDetailT != null)
                    {



                        view = new P2BCrGridData()
                        {
                            Id = z.Employee.Id,
                            Code = z.Employee.EmpCode,
                            Name = z.Employee.EmpName.FullNameFML
                        };
                        model.Add(view);


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
        public ActionResult GetLVHEAD1(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                int empid = Convert.ToInt32(data2);
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
                    s = new SelectList(CheckingLeavheadFromlvnereq, "Id", "FullDetails", selected);
                    return Json(s, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }



            }
        }

        [HttpPost]
        public ActionResult GetLVHEAD2(string data, string data2, string Empids)
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

    }
 

}