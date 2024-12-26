using P2b.Global;
using P2BUltimate.Process;
using Payroll;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using P2BUltimate.App_Start;
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
using Attendance;

namespace P2BUltimate.Controllers.Attendance.MainController
{
    [AuthoriseManger]
    public class AttendanceuploadinPayrollController : Controller
    {
        public ActionResult Index()
        {
            return View("~/Views/Attendance/MainViews/AttendanceuploadinPayroll/Index.cshtml");
        }


        public ActionResult ChkProcess(string typeofbtn, string month)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                bool selected = true;
                //List<SalAttendanceT> query = db.EmployeePayroll.SelectMany(r => r.SalAttendance).ToList();
                //var a = query.Where(t => t.PayMonth == month).ToList();
                //if (a.Count > 0)
                //{
                //    selected = true;
                //}
                var data = new
                {
                    status = selected,
                };
                return Json(data, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult Getpaymonconcept(string EmpId) //Pass leavehead id here
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var query1 = db.PayProcessGroup.Include(e => e.PayrollPeriod)
                  .SingleOrDefault().PayrollPeriod.FirstOrDefault();

                    int startday = query1.StartDate;
                    int endday = query1.EndDate;
                   
                var Emppayconcept = db.PayProcessGroup.Include(a => a.PayMonthConcept)
                    .OrderByDescending(e => e.Id).FirstOrDefault();
                string paymonConcept = Emppayconcept.PayMonthConcept.LookupVal.ToUpper();

                return Json(new { Locname = paymonConcept, Jobname = startday + "-" + endday }, JsonRequestBehavior.AllowGet);

            }
        }

        public ActionResult ReleaseProcess(string forwardata, string PayMonth)
        {

            List<string> Msg = new List<string>();
            List<string> Msg1 = new List<string>();
            int Processgrpid;
            List<int> ids = null; try
            {
                if (forwardata != null && forwardata != "0" && forwardata != "false")
                {
                    ids = Utility.StringIdsToListIds(forwardata);
                }
                else
                {
                    Msg.Add("  Kindly Select Employee.  ");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                }
                //Muster clear check code start
                using (DataBaseContext db1 = new DataBaseContext())
                {
                    var query1 = db1.PayProcessGroup.Include(e => e.PayrollPeriod)
                     .SingleOrDefault().PayrollPeriod.FirstOrDefault();

                    int startday = query1.StartDate;
                    int endday = query1.EndDate;
                    DateTime _PayMonth = Convert.ToDateTime("01/" + PayMonth);

                    DateTime end = Convert.ToDateTime("01/" + PayMonth).AddMonths(1).AddDays(-1).Date;
                    //int daysInEndMonth = (end - end.AddMonths(1)).Days
                    int daysInEndMonth = end.Day;
                    int daysInstartMonth = 1;
                    DateTime FromPeriod;
                    DateTime EndPeriod;
                    DateTime Currentmonthstart;
                    DateTime CurrentmonthEnd;
                    DateTime Prevmonthstart;
                    DateTime PrevmonthEnd;
                    int ProDays = 0;
                    int RetProDays = 0;
                    int daym = (Convert.ToDateTime("01/" + PayMonth).AddMonths(1).AddDays(-1).Date).Day;
                    Currentmonthstart = Convert.ToDateTime("01/" + PayMonth);


                    if (endday > daym)
                    {
                        endday = daym;
                    }
                    ProDays = daym - endday;

                    if (startday == daysInstartMonth && endday == daysInEndMonth)
                    {
                        FromPeriod = _PayMonth;
                        EndPeriod = Convert.ToDateTime(DateTime.DaysInMonth(_PayMonth.Year, _PayMonth.Month) + "/" + _PayMonth.Month + "/" + _PayMonth.Year);
                    }
                    else
                    {
                        DateTime prvmonth = Convert.ToDateTime("01/" + PayMonth).AddMonths(-1).Date;
                        startday = endday + 1;
                        string pmonth = prvmonth.ToString("MM/yyyy");
                        FromPeriod = Convert.ToDateTime(startday + "/" + pmonth);


                        EndPeriod = Convert.ToDateTime(endday + "/" + PayMonth);

                    }
                    CurrentmonthEnd = EndPeriod;
                    Prevmonthstart = FromPeriod;
                    if (startday == daysInstartMonth && endday == daysInEndMonth)
                    {
                        PrevmonthEnd = FromPeriod.AddDays(ProDays);
                    }
                    else
                    {
                        PrevmonthEnd = Convert.ToDateTime("01/" + FromPeriod.ToString("MM/yyyy")).AddMonths(1).AddDays(-1).Date;
                    }
                    var chkk = db1.ProcessedData.Include(x => x.OldMusterRemarks).Where(a => DbFunctions.TruncateTime(a.SwipeDate) >= Prevmonthstart
                               && DbFunctions.TruncateTime(a.SwipeDate) <= CurrentmonthEnd && a.OldMusterRemarks == null && a.PresentStatus.LookupVal == "-" && (a.MusterRemarks.LookupVal.ToUpper() != "UA" && a.MusterRemarks.LookupVal.ToUpper() != "AA")).ToList();                                       
                    if (chkk!=null && chkk.Count()>0)
                    {
                        Msg.Add("Please Clear Muster Correction ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    //var OEmpSalRelT = db1.EmployeePayroll.Include(e => e.SalAttendance).AsNoTracking().ToList();
                   // var EmpSalRelT = OEmpSalRelT.SalaryT != null ? OEmpSalRelT.SalaryT.Where(e => e.PayMonth == PayMonth && e.ReleaseDate != null) : null;
                    var EmpSalRelT =  db1.SalaryT.Where(e => e.PayMonth == PayMonth && e.ReleaseDate != null).ToList();

                    if (EmpSalRelT != null && EmpSalRelT.Count() > 0)
                    {
                        Msg.Add("This month salary has lock you can not upload ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet); 
                    }
                    var EmpSalnotRelT = db1.SalaryT.Where(e => e.PayMonth == PayMonth && e.ReleaseDate == null).ToList();
                    if (EmpSalnotRelT != null && EmpSalnotRelT.Count() > 0)
                    {
                        Msg.Add("Please delete salary for this month and try again ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }



                }
                //Muster clear check code End
              

                Employee OEmployee = null;
                EmployeePayroll OEmployeePayroll = null;

                List<SalaryT> salaryt = new List<SalaryT>();
                foreach (var i in ids)
                {
                    using (DataBaseContext db = new DataBaseContext())
                    {
                        // OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct).Where(r => r.Id == i).SingleOrDefault();
                        OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.GeoStruct.Company).Include(e => e.FuncStruct).Include(e => e.PayStruct)
                                        .Where(r => r.Id == i).SingleOrDefault();

                        OEmployeePayroll = db.EmployeePayroll.Include(e => e.SalAttendance).Where(e => e.Employee.Id == i).SingleOrDefault();
                        SalAttendanceT Salatt = OEmployeePayroll.SalAttendance.Where(e => e.PayMonth == PayMonth).SingleOrDefault();
                        if (Salatt != null)
                        {
                            Msg.Add("Attendance For Employee Code=" + OEmployeePayroll.Employee.EmpCode + ", allready available Unable To Upload" + "\n");
                            continue;

                        }
                        // upload code start 
                        var BindEmpList = db.EmployeeAttendance.Include(e => e.Employee.EmpName)
                       .Include(e => e.Employee.EmpOffInfo)
                       .Include(e => e.Employee.ServiceBookDates)
                       .Include(e => e.Employee.EmpOffInfo.PayProcessGroup)
                       .Include(e => e.Employee.ServiceBookDates)
                       .Include(e => e.ProcessedData)
                       .Include(e => e.ProcessedData.Select(x => x.PresentStatus))
                       .Include(e => e.ProcessedData.Select(x => x.MusterRemarks))
                       .Include(e => e.ProcessedData.Select(x => x.OldMusterRemarks))
                       .Where(e => e.Employee.Id == i).AsNoTracking().AsParallel().FirstOrDefault();

                        Processgrpid = BindEmpList.Employee.EmpOffInfo.PayProcessGroup.Id;

                        var query1 = db.PayProcessGroup.Include(e => e.PayrollPeriod).Where(e => e.Id == Processgrpid)
                   .SingleOrDefault().PayrollPeriod.FirstOrDefault();

                        int startday = query1.StartDate;
                        int endday = query1.EndDate;
                        DateTime _PayMonth = Convert.ToDateTime("01/" + PayMonth);

                        DateTime end = Convert.ToDateTime("01/" + PayMonth).AddMonths(1).AddDays(-1).Date;
                        //int daysInEndMonth = (end - end.AddMonths(1)).Days
                        int daysInEndMonth = end.Day;
                        int daysInstartMonth = 1;
                        DateTime FromPeriod;
                        DateTime EndPeriod;
                        DateTime Currentmonthstart;
                        DateTime CurrentmonthEnd;
                        DateTime Prevmonthstart;
                        DateTime PrevmonthEnd;
                        int ProDays = 0;
                        int RetProDays = 0;
                        int daym = (Convert.ToDateTime("01/" + PayMonth).AddMonths(1).AddDays(-1).Date).Day;
                        Currentmonthstart = Convert.ToDateTime("01/" + PayMonth);


                        if (endday > daym)
                        {
                            endday = daym;
                        }
                        ProDays = daym - endday;
                        RetProDays = ProDays;
                        if (startday == daysInstartMonth && endday == daysInEndMonth)
                        {
                            FromPeriod = _PayMonth;
                            EndPeriod = Convert.ToDateTime(DateTime.DaysInMonth(_PayMonth.Year, _PayMonth.Month) + "/" + _PayMonth.Month + "/" + _PayMonth.Year);
                        }
                        else
                        {
                            DateTime prvmonth = Convert.ToDateTime("01/" + PayMonth).AddMonths(-1).Date;
                            startday = endday + 1;
                            string pmonth = prvmonth.ToString("MM/yyyy");
                            FromPeriod = Convert.ToDateTime(startday + "/" + pmonth);


                            EndPeriod = Convert.ToDateTime(endday + "/" + PayMonth);

                        }
                        CurrentmonthEnd = EndPeriod;
                        Prevmonthstart = FromPeriod;
                        if (startday == daysInstartMonth && endday == daysInEndMonth)
                        {
                            PrevmonthEnd = FromPeriod.AddDays(ProDays);
                        }
                        else
                        {
                            PrevmonthEnd = Convert.ToDateTime("01/" + FromPeriod.ToString("MM/yyyy")).AddMonths(1).AddDays(-1).Date;
                        }


                        // attendance upload period End
                        double Fday = 0;
                        double Hday = 0;
                        double AAday = 0;
                        double UAday = 0;
                        double Recday = 0;
                        double Paybledays = 0;
                        double Arrdays = 0;
                        double Musterclear = 0;
                        double HOWO = 0;
                        Boolean LockStatus = false;

                        if ((BindEmpList.Employee != null && BindEmpList.Employee.EmpOffInfo != null && BindEmpList.Employee.EmpOffInfo.PayProcessGroup != null
                            && BindEmpList.Employee.EmpOffInfo.PayProcessGroup.Id != Processgrpid) ||
                            (BindEmpList.Employee != null && BindEmpList.Employee.ServiceBookDates != null && BindEmpList.Employee.ServiceBookDates.ServiceLastDate != null))
                        {
                            continue;
                        }

                        //var chklockstatus = BindEmpList.ProcessedData.Where(a => a.SwipeDate.Value.Date >= Prevmonthstart &&
                        //                                                  a.SwipeDate.Value.Date <= CurrentmonthEnd).Select(y => y.IsLocked).FirstOrDefault();

                        //if (chklockstatus != null)
                        //{
                        //    LockStatus = chklockstatus;
                        //}
                        if (BindEmpList.Employee.ServiceBookDates.RetirementDate != null)
                        {

                            if (BindEmpList.Employee.ServiceBookDates.RetirementDate.Value.Date <= CurrentmonthEnd.Date)
                            {
                                CurrentmonthEnd = BindEmpList.Employee.ServiceBookDates.RetirementDate.Value.Date;
                                RetProDays = 0;

                            }
                            else if (BindEmpList.Employee.ServiceBookDates.RetirementDate.Value.Date >= CurrentmonthEnd.Date && BindEmpList.Employee.ServiceBookDates.RetirementDate.Value.Date <= end.Date)
                            {
                                RetProDays = (BindEmpList.Employee.ServiceBookDates.RetirementDate.Value.Date - CurrentmonthEnd).Days;

                            }
                        }
                        var chkk = BindEmpList.ProcessedData.Where(a => a.SwipeDate.Value.Date >= Currentmonthstart &&
                                                                        a.SwipeDate.Value.Date <= CurrentmonthEnd).ToList();
                        foreach (var item in chkk)
                        {
                            if (item.MusterRemarks.LookupVal.ToUpper() == "HO" || item.MusterRemarks.LookupVal.ToUpper() == "WO")
                            {
                                HOWO = HOWO + 1;
                            }
                            if (item.PresentStatus.LookupVal.ToUpper() == "F")
                            {
                                Fday = Fday + 1;
                            }
                            else if (item.PresentStatus.LookupVal.ToUpper() == "H")
                            {
                                Hday = Hday + 0.5;
                                AAday = AAday + 0.5;
                            }
                            else if (item.PresentStatus.LookupVal.ToUpper() == "-" && (item.MusterRemarks.LookupVal.ToUpper() != "UA" && item.MusterRemarks.LookupVal.ToUpper() != "AA"))
                            {
                                if (item.OldMusterRemarks == null)
                                {
                                    Musterclear = Musterclear + 1;
                                }
                                else if (item.MusterRemarks.LookupVal.ToUpper() == item.OldMusterRemarks.LookupVal.ToUpper() && item.MusterRemarks.LookupVal.ToUpper() == "LF")// apply action for leave but leave balance 0 so old and new remark LF this treat as absent
                                {
                                    AAday = AAday + 1;
                                }
                                // Musterclear = Musterclear + 1;
                            }
                            else if (item.PresentStatus.LookupVal.ToUpper() == "-" && item.MusterRemarks.LookupVal.ToUpper() == "AA")
                            {
                                AAday = AAday + 1;
                            }
                            else
                            {
                                UAday = UAday + 1;
                            }
                        }


                        var chkkrec = BindEmpList.ProcessedData.Where(a => a.SwipeDate.Value.Date >= Prevmonthstart &&
                                                                      a.SwipeDate.Value.Date <= PrevmonthEnd).ToList();
                        if (ProDays != 0)//if 1 to 30 or 31 month then 01 date two time pick above and here
                        {

                            foreach (var rec in chkkrec)
                            {
                                if (rec.MusterRemarks.LookupVal.ToUpper() == "HO" || rec.MusterRemarks.LookupVal.ToUpper() == "WO")
                                {
                                    HOWO = HOWO + 1;
                                }
                                if (rec.PresentStatus.LookupVal.ToUpper() == "F")
                                {

                                }
                                else if (rec.PresentStatus.LookupVal.ToUpper() == "H")
                                {
                                    AAday = AAday + 0.5;
                                }
                                else if (rec.PresentStatus.LookupVal.ToUpper() == "-" && (rec.MusterRemarks.LookupVal.ToUpper() != "UA" && rec.MusterRemarks.LookupVal.ToUpper() != "AA"))
                                {
                                    if (rec.OldMusterRemarks == null)
                                    {
                                        Musterclear = Musterclear + 1;
                                    }
                                    else if (rec.MusterRemarks.LookupVal.ToUpper() == rec.OldMusterRemarks.LookupVal.ToUpper() && rec.MusterRemarks.LookupVal.ToUpper() == "LF")// apply action for leave but leave balance 0 so old and new remark LF this treat as absent
                                    {
                                        AAday = AAday + 1;
                                    }

                                    //Musterclear = Musterclear + 1;
                                }
                                else if (rec.PresentStatus.LookupVal.ToUpper() == "-" && rec.MusterRemarks.LookupVal.ToUpper() == "AA")
                                {
                                    AAday = AAday + 1;
                                }
                                else
                                {
                                    // Recday = Recday + 1;
                                    UAday = UAday + 1;
                                }
                            }
                        }
                        if (Prevmonthstart.Date == PrevmonthEnd.Date)
                        {
                            Recday = 0;
                        }

                        if (chkk.Count() != 0 || chkkrec.Count() != 0)
                        {
                            var uploadpolicy = db.AttendancePayrollPolicy.Where(e => e.PayProcessGroup.Id == Processgrpid).SingleOrDefault();
                            if (uploadpolicy != null)
                            {
                                //if (uploadpolicy.LWPAdjustCurSal == true)
                                //{
                                //    Paybledays = Fday + Hday + ProDays - Recday;
                                //    if (Paybledays < 0)
                                //    {
                                //        Paybledays = 0;
                                //    }

                                //}
                                //else
                                //{
                                //    Paybledays = Fday + Hday + ProDays;
                                //    Arrdays = Recday;
                                //}
                                if (RetProDays == ProDays)
                                {
                                    if (BindEmpList.Employee.ServiceBookDates.RetirementDate.Value.Date <= CurrentmonthEnd.Date)
                                    {
                                        Paybledays = CurrentmonthEnd.Day - (AAday + UAday + Musterclear);
                                    }
                                    else
                                    {
                                        Paybledays = daym - (AAday + UAday + Musterclear);
                                    }
                                    
                                }
                                else
                                {
                                    Paybledays = daym - (ProDays - RetProDays) - (AAday + UAday + Musterclear);
                                }
                                if (Paybledays < 0)
                                {
                                    Paybledays = 0;
                                }
                                if ((HOWO + AAday + UAday) >= daym)
                                {
                                    Paybledays = 0;
                                }


                            }
                            // upload code end

                            string Emppayconceptf30 = db.PayProcessGroup.Include(a => a.PayMonthConcept).AsNoTracking().OrderByDescending(e => e.Id).FirstOrDefault().PayMonthConcept.LookupVal;
                            if (Emppayconceptf30 == "FIXED30DAYS")
                            {
                                if (Paybledays > 30)
                                {
                                    Paybledays = 30;
                                }
                            }
                            if (Emppayconceptf30 == "30DAYS")
                            {
                                if (Paybledays > 30)
                                {
                                    Paybledays = 30;
                                }
                            }

                            if (Emppayconceptf30 == "CALENDAR")
                            {
                                if (Paybledays > daym)
                                {
                                    Paybledays = daym;
                                }
                            }

                            SalAttendanceT ObjFAT = new SalAttendanceT();
                            {
                                ObjFAT.PayMonth = PayMonth;
                                ObjFAT.PaybleDays = Paybledays;
                                ObjFAT.MonthDays = daym;

                            }
                            //LWP leave upload given so this Comment (Goa Urban)

                            //if (Emppayconceptf30 == "FIXED30DAYS")
                            //{
                            //    ObjFAT.LWPDays = 30 - Paybledays;
                            //}

                            //if (Emppayconceptf30 == "CALENDAR")
                            //{
                            //    ObjFAT.LWPDays = daym - Paybledays;
                            //}

                            //if (Emppayconceptf30 == "30DAYS")
                            //{
                            //    ObjFAT.LWPDays = 30 - Paybledays; // as Suggesess by sir  in upload
                            //    ObjFAT.MonthDays = 30; // as Suggesess by sir in upload
                            //}

                            List<string> PayMnth = new List<string>();
                            DateTime Paydate = Convert.ToDateTime("01/" + PayMonth);

                            var EmployeeDetails = db.Employee.Include(e => e.SalaryHoldDetails).Where(e => e.Id == i).AsNoTracking().FirstOrDefault().SalaryHoldDetails.Where(e => (e.FromDate >= Paydate) && (Paydate <= e.ToDate));

                            if (EmployeeDetails.Count() == 0)
                            {

                            }
                            else
                            {
                                ObjFAT.PaybleDays = 0;
                            }

                            //  var test = db.EmployeePayroll
                            // .Join(db.Employee, p => p.Employee.Id, pc => pc.Id, (p, pc) => new { p, pc })
                            //.Where(p => p.p.Employee.Id == i).AsNoTracking()
                            //.Select(m => new
                            //{
                            //    GeoStruct_id = m.pc.GeoStruct.Id,
                            //    PayStruct_id = m.pc.PayStruct.Id,
                            //    FuncStruct_id = m.pc.FuncStruct.Id,
                            //    Id = m.p.Id
                            //}).FirstOrDefault();
                            //ObjFAT.GeoStruct = db.GeoStruct.OrderBy(e => e.Id).Where(e => e.Id == test.GeoStruct_id).FirstOrDefault();

                            //ObjFAT.FuncStruct = db.FuncStruct.OrderBy(e => e.Id).Where(e => e.Id == test.FuncStruct_id).FirstOrDefault();

                            //ObjFAT.PayStruct = db.PayStruct.OrderBy(e => e.Id).Where(e => e.Id == test.PayStruct_id).FirstOrDefault();
                            ObjFAT.GeoStruct = db.GeoStruct.Find(OEmployee.GeoStruct.Id);
                            ObjFAT.FuncStruct = db.FuncStruct.Find(OEmployee.FuncStruct.Id);
                            ObjFAT.PayStruct = db.PayStruct.Find(OEmployee.PayStruct.Id);

                            ObjFAT.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false, CreatedOn = DateTime.Now };


                            using (TransactionScope ts = new TransactionScope())
                            {
                                try
                                {
                                    db.SalAttendanceT.Add(ObjFAT);
                                    db.SaveChanges();
                                    List<SalAttendanceT> OFAT = new List<SalAttendanceT>();
                                    OFAT.Add(db.SalAttendanceT.Find(ObjFAT.Id));

                                    if (OEmployeePayroll == null)
                                    {
                                        EmployeePayroll OTEP = new EmployeePayroll()
                                        {
                                            Employee = db.Employee.Find(i),
                                            SalAttendance = OFAT,
                                            DBTrack = ObjFAT.DBTrack

                                        };


                                        db.EmployeePayroll.Add(OTEP);
                                        db.SaveChanges();
                                    }
                                    else
                                    {
                                        var aa = db.EmployeePayroll.Find(OEmployeePayroll.Id);
                                        //aa.SalAttendance = null;
                                        if (aa.SalAttendance != null)
                                        {
                                            OFAT.AddRange(aa.SalAttendance);
                                        }

                                        aa.SalAttendance = OFAT;
                                        //OEmployeePayroll.DBTrack = dbt;
                                        db.EmployeePayroll.Attach(aa);
                                        db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                        db.Entry(aa).State = System.Data.Entity.EntityState.Detached;

                                    }


                                    // Arrear Code Start
                                    if (Arrdays != 0)
                                    {
                                        DateTime DArrfrom = DateTime.Now;
                                        DateTime DArrTo = DateTime.Now;
                                        DArrfrom = Prevmonthstart.Date;
                                        DArrTo = Convert.ToDateTime(Prevmonthstart.Date).AddDays(Arrdays);
                                        int lwpid = db.LookupValue.Where(e => e.LookupVal.ToUpper() == "LWP").FirstOrDefault().Id;
                                        SalaryArrearT OSalaryArrearT = new SalaryArrearT();
                                        List<SalaryArrearT> OFATArrear = new List<SalaryArrearT>();
                                        OSalaryArrearT = new SalaryArrearT();
                                        {
                                            OSalaryArrearT.ArrearType = db.Lookup.Where(e => e.Code == "417").FirstOrDefault().LookupValues.Where(e => e.LookupVal.ToUpper() == "LWP").FirstOrDefault();//db.LookupValue.Where(e => e.LookupVal.ToUpper() == "LWP").FirstOrDefault();
                                            OSalaryArrearT.FromDate = DArrfrom;
                                            OSalaryArrearT.TotalDays = Arrdays;
                                            OSalaryArrearT.ToDate = DArrTo;
                                            OSalaryArrearT.IsAuto = false;
                                            OSalaryArrearT.IsPaySlip = true;
                                            OSalaryArrearT.IsRecovery = true;
                                            OSalaryArrearT.PayMonth = PayMonth;
                                            OSalaryArrearT.GeoStruct = db.GeoStruct.Find(OEmployee.GeoStruct.Id);
                                            OSalaryArrearT.FuncStruct = db.FuncStruct.Find(OEmployee.FuncStruct.Id);
                                            OSalaryArrearT.PayStruct = db.PayStruct.Find(OEmployee.PayStruct.Id);
                                            OSalaryArrearT.DBTrack = ObjFAT.DBTrack;
                                        }

                                        db.SalaryArrearT.Add(OSalaryArrearT);
                                        db.SaveChanges();

                                        OFATArrear.Add(db.SalaryArrearT.Find(OSalaryArrearT.Id));
                                        if (OEmployeePayroll == null)
                                        {
                                            EmployeePayroll OTEPArr = new EmployeePayroll()
                                            {
                                                Employee = db.Employee.Find(OEmployee.Id),
                                                SalaryArrearT = OFATArrear,
                                                DBTrack = ObjFAT.DBTrack
                                            };
                                            db.EmployeePayroll.Add(OTEPArr);
                                            db.SaveChanges();
                                        }
                                        else
                                        {
                                            var aa1 = db.EmployeePayroll.Find(OEmployeePayroll.Id);
                                            aa1.SalaryArrearT = OFATArrear;
                                            db.EmployeePayroll.Attach(aa1);
                                            db.Entry(aa1).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();
                                            // db.Entry(aa).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }

                                    // Arrear Code End
                                    ts.Complete();
                                    Msg.Add("Data Saved successfully for" + OEmployee.EmpCode);



                                }
                                catch (Exception ex)
                                {

                                    // List<string> Msg = new List<string>();
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
                                    // return Json(new { success = false, Msg}, JsonRequestBehavior.AllowGet);
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }
                            }



                        }
                    }
                }
                Msg.Add("Data Upload successfully");
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
                Msg.Add(ex.Message);
                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

            }
            return Json(new { success = true, responseText = "Salary released for employee." }, JsonRequestBehavior.AllowGet);

        }
        public ActionResult LockAttendanceprocess(string forwardata, string PayMonth)
        {

            List<string> Msg = new List<string>();
            List<string> Msg1 = new List<string>();
            int Processgrpid;
            List<int> ids = null; try
            {
                if (forwardata != null && forwardata != "0" && forwardata != "false")
                {
                    ids = Utility.StringIdsToListIds(forwardata);
                }
                else
                {
                    Msg.Add("  Kindly Select Employee.  ");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                }

                Employee OEmployee = null;
                EmployeePayroll OEmployeePayroll = null;

                List<SalaryT> salaryt = new List<SalaryT>();
                foreach (var i in ids)
                {
                    using (DataBaseContext db = new DataBaseContext())
                    {
                        OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.GeoStruct.Company).Include(e => e.FuncStruct).Include(e => e.PayStruct)
                                      .Where(r => r.Id == i).SingleOrDefault();
                        //OEmployeePayroll = db.EmployeePayroll.Include(e => e.SalAttendance).Where(e => e.Employee.Id == i).SingleOrDefault();
                        //SalAttendanceT Salatt = OEmployeePayroll.SalAttendance.Where(e => e.PayMonth == PayMonth).SingleOrDefault();
                        //if (Salatt != null)
                        //{
                        //    Msg.Add("Attendance For Employee Code=" + OEmployeePayroll.Employee.EmpCode + ", allready available Unable To Upload" + "\n");
                        //    continue;

                        //}
                        // upload code start 
                        var BindEmpList = db.EmployeeAttendance.Include(e => e.Employee.EmpName)
                       .Include(e => e.Employee.EmpOffInfo)
                       .Include(e => e.Employee.EmpOffInfo.PayProcessGroup)
                       .Include(e => e.Employee.ServiceBookDates)
                       .Include(e => e.ProcessedData)
                       .Include(e => e.ProcessedData.Select(x => x.PresentStatus))
                       .Where(e => e.Employee.Id == i).AsNoTracking().AsParallel().FirstOrDefault();

                        Processgrpid = BindEmpList.Employee.EmpOffInfo.PayProcessGroup.Id;

                        var query1 = db.PayProcessGroup.Include(e => e.PayrollPeriod).Where(e => e.Id == Processgrpid)
                   .SingleOrDefault().PayrollPeriod.FirstOrDefault();

                        int startday = query1.StartDate;
                        int endday = query1.EndDate;
                        DateTime _PayMonth = Convert.ToDateTime("01/" + PayMonth);

                        DateTime end = Convert.ToDateTime("01/" + PayMonth).AddMonths(1).AddDays(-1).Date;
                        //int daysInEndMonth = (end - end.AddMonths(1)).Days
                        int daysInEndMonth = end.Day;
                        int daysInstartMonth = 1;
                        DateTime FromPeriod;
                        DateTime EndPeriod;
                        DateTime Currentmonthstart;
                        DateTime CurrentmonthEnd;
                        DateTime Prevmonthstart;
                        DateTime PrevmonthEnd;
                        int ProDays = 0;
                        int daym = (Convert.ToDateTime("01/" + PayMonth).AddMonths(1).AddDays(-1).Date).Day;
                        Currentmonthstart = Convert.ToDateTime("01/" + PayMonth);


                        if (endday > daym)
                        {
                            endday = daym;
                        }
                        ProDays = daym - endday;

                        if (startday == daysInstartMonth && endday == daysInEndMonth)
                        {
                            FromPeriod = _PayMonth;
                            EndPeriod = Convert.ToDateTime(DateTime.DaysInMonth(_PayMonth.Year, _PayMonth.Month) + "/" + _PayMonth.Month + "/" + _PayMonth.Year);
                        }
                        else
                        {
                            DateTime prvmonth = Convert.ToDateTime("01/" + PayMonth).AddMonths(-1).Date;
                            startday = endday + 1;
                            string pmonth = prvmonth.ToString("MM/yyyy");
                            FromPeriod = Convert.ToDateTime(startday + "/" + pmonth);


                            EndPeriod = Convert.ToDateTime(endday + "/" + PayMonth);

                        }
                        CurrentmonthEnd = EndPeriod;
                        Prevmonthstart = FromPeriod;
                        if (startday == daysInstartMonth && endday == daysInEndMonth)
                        {
                            PrevmonthEnd = FromPeriod.AddDays(ProDays);
                        }
                        else
                        {
                            PrevmonthEnd = Convert.ToDateTime("01/" + FromPeriod.ToString("MM/yyyy")).AddMonths(1).AddDays(-1).Date;
                        }


                        // attendance upload period End

                        if ((BindEmpList.Employee != null && BindEmpList.Employee.EmpOffInfo != null && BindEmpList.Employee.EmpOffInfo.PayProcessGroup != null
                            && BindEmpList.Employee.EmpOffInfo.PayProcessGroup.Id != Processgrpid) ||
                            (BindEmpList.Employee != null && BindEmpList.Employee.ServiceBookDates != null && BindEmpList.Employee.ServiceBookDates.ServiceLastDate != null))
                        {
                            continue;
                        }


                        var chkk = BindEmpList.ProcessedData.Where(a => a.SwipeDate.Value.Date >= Prevmonthstart &&
                                                                        a.SwipeDate.Value.Date <= CurrentmonthEnd).ToList();


                        if (chkk.Count() != 0)
                        {

                            // upload code end

                            using (TransactionScope ts = new TransactionScope())
                            {
                                try
                                {

                                    foreach (var item in chkk)
                                    {
                                        ProcessedData empprocessr = db.ProcessedData.Where(e => e.Id == item.Id).FirstOrDefault();

                                        empprocessr.IsLocked = true;
                                        empprocessr.LockDate = DateTime.Now;
                                        //OEmployeePayroll.DBTrack = dbt;
                                        db.ProcessedData.Attach(empprocessr);
                                        db.Entry(empprocessr).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                        db.Entry(empprocessr).State = System.Data.Entity.EntityState.Detached;

                                    }

                                    Msg.Add("Attendance Process Lock successfully for" + OEmployee.EmpCode);

                                    ts.Complete();

                                }
                                catch (Exception ex)
                                {

                                    // List<string> Msg = new List<string>();
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
                                    // return Json(new { success = false, Msg}, JsonRequestBehavior.AllowGet);
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }
                            }



                        }
                    }
                }
                Msg.Add("Attendance Process Lock successfully");
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
                Msg.Add(ex.Message);
                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

            }
            return Json(new { success = true, responseText = "Salary released for employee." }, JsonRequestBehavior.AllowGet);

        }


        public class P2BGridData
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
            public double Monthdays { get; set; }
            public double MusterCorrectionDays { get; set; }
            public double UnauthuorisedAbentdays { get; set; }
            public double AuthorisedAbsentdays { get; set; }
            public double TotalPaybledays { get; set; }
            public Boolean LockStatus { get; set; }
          
        }
        public ActionResult P2BGrid(P2BGrid_Parameters gp)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                    int pageSize = gp.rows;
                    int totalPages = 0;
                    int totalRecords = 0;
                    var jsonData = (Object)null;
                    int Processgrpid;
                    IEnumerable<P2BGridData> SalaryList = null;
                    List<P2BGridData> model = new List<P2BGridData>();
                    P2BGridData view = null;
                    string PayMonth = "";
                    string Month = "";
                    if (gp.filter != null)
                        // PayMonth = gp.filter.ToString().Split(',')[0];
                        PayMonth = gp.filter;
                    else
                    {
                        if (DateTime.Now.Date.Month < 10)
                            Month = "0" + DateTime.Now.Date.Month;
                        else
                            Month = DateTime.Now.Date.Month.ToString();
                        PayMonth = Month + "/" + DateTime.Now.Date.Year;
                    }
                    //if (gp.filter != null)
                    //{
                    //    Processgrpid = Convert.ToInt32(gp.filter.ToString().Split(',')[1]);
                    //}
                    //else
                    //{
                    //    Processgrpid = db.PayProcessGroup.FirstOrDefault().Id;
                    //}
                    // Payroll Period start
                    DateTime prvemon = Convert.ToDateTime("01/" + PayMonth).AddMonths(-1).Date;
                    DateTime curmon=Convert.ToDateTime("01/" + PayMonth).AddMonths(1).AddDays(-1).Date;
                    var tt = db.ProcessedData.Where(e => e.SwipeDate >= prvemon && e.SwipeDate <= curmon).Select(r => r.EmployeeAttendance_Id).Distinct().ToList();
                    var BindEmpList = db.EmployeeAttendance.Where(e => tt.Contains(e.Id)).Select(e =>
                        new
                        {
                            Employee = e.Employee,
                            ProcessedData = e.ProcessedData.Where(x => x.SwipeDate >= prvemon && x.SwipeDate <= curmon).Select(y => new { PresentStatus = y.PresentStatus, MusterRemarks = y.MusterRemarks, SwipeDate = y.SwipeDate, IsLocked = y.IsLocked, OldMusterRemarks = y.OldMusterRemarks }).ToList(),
                            EmpName = e.Employee.EmpName,
                            EmpOffInfo = e.Employee.EmpOffInfo,
                            ServiceBookDates = e.Employee.ServiceBookDates,
                            PayProcessGroup=e.Employee.EmpOffInfo.PayProcessGroup

                        }).ToList();

                    //var BindEmpList = db.EmployeeAttendance.Include(e => e.Employee.EmpName)
                    //    .Include(e => e.Employee.EmpOffInfo)
                    //     .Include(e => e.Employee.ServiceBookDates)
                    //    .Include(e => e.Employee.EmpOffInfo.PayProcessGroup)
                    //    .Include(e => e.Employee.ServiceBookDates)
                    //    .Include(e => e.ProcessedData)
                    //    .Include(e => e.ProcessedData.Select(x => x.PresentStatus))
                    //     .Include(e => e.ProcessedData.Select(x => x.MusterRemarks))
                    //    .AsNoTracking().AsParallel().ToList();

                    foreach (var z in BindEmpList)
                    {
                        // attendance upload period start
                        if ((z.Employee != null && z.Employee.EmpOffInfo != null && z.Employee.EmpOffInfo.PayProcessGroup != null))
                        {
                            Processgrpid = z.Employee.EmpOffInfo.PayProcessGroup.Id;


                        }
                        else
                        {
                            continue;
                        }

                        var query1 = db.PayProcessGroup.Include(e => e.PayrollPeriod).Where(e => e.Id == Processgrpid)
                   .SingleOrDefault().PayrollPeriod.FirstOrDefault();

                        int startday = query1.StartDate;
                        int endday = query1.EndDate;
                        DateTime _PayMonth = Convert.ToDateTime("01/" + PayMonth);

                        DateTime end = Convert.ToDateTime("01/" + PayMonth).AddMonths(1).AddDays(-1).Date;
                        //int daysInEndMonth = (end - end.AddMonths(1)).Days
                        int daysInEndMonth = end.Day;
                        int daysInstartMonth = 1;
                        DateTime FromPeriod;
                        DateTime EndPeriod;
                        DateTime Currentmonthstart;
                        DateTime CurrentmonthEnd;
                        DateTime Prevmonthstart;
                        DateTime PrevmonthEnd;
                        int ProDays = 0;
                        int RetProDays = 0;
                        int daym = (Convert.ToDateTime("01/" + PayMonth).AddMonths(1).AddDays(-1).Date).Day;
                        Currentmonthstart = Convert.ToDateTime("01/" + PayMonth);


                        if (endday > daym)
                        {
                            endday = daym;
                        }
                        ProDays = daym - endday;
                        RetProDays = ProDays;
                        if (startday == daysInstartMonth && endday == daysInEndMonth)
                        {
                            FromPeriod = _PayMonth;
                            EndPeriod = Convert.ToDateTime(DateTime.DaysInMonth(_PayMonth.Year, _PayMonth.Month) + "/" + _PayMonth.Month + "/" + _PayMonth.Year);
                        }
                        else
                        {
                            DateTime prvmonth = Convert.ToDateTime("01/" + PayMonth).AddMonths(-1).Date;
                            startday = endday + 1;
                            string pmonth = prvmonth.ToString("MM/yyyy");
                            FromPeriod = Convert.ToDateTime(startday + "/" + pmonth);


                            EndPeriod = Convert.ToDateTime(endday + "/" + PayMonth);

                        }
                        CurrentmonthEnd = EndPeriod;
                        Prevmonthstart = FromPeriod;
                        if (startday == daysInstartMonth && endday == daysInEndMonth)
                        {
                            PrevmonthEnd = FromPeriod.AddDays(ProDays);
                        }
                        else
                        {
                            PrevmonthEnd = Convert.ToDateTime("01/" + FromPeriod.ToString("MM/yyyy")).AddMonths(1).AddDays(-1).Date;
                        }


                        // attendance upload period End
                        double Fday = 0;
                        double Hday = 0;
                        double AAday = 0;
                        double UAday = 0;
                        double Recday = 0;
                        double Paybledays = 0;
                        double Musterclear = 0;
                        double HOWO = 0;
                        Boolean LockStatus = false;

                        if ((z.Employee != null && z.Employee.EmpOffInfo != null && z.Employee.EmpOffInfo.PayProcessGroup != null
                            && z.Employee.EmpOffInfo.PayProcessGroup.Id != Processgrpid) ||
                            (z.Employee != null && z.Employee.ServiceBookDates != null && z.Employee.ServiceBookDates.ServiceLastDate != null))
                        {
                            continue;
                        }

                        var chklockstatus = z.ProcessedData.Where(a => a.SwipeDate.Value.Date >= Prevmonthstart &&
                                                                          a.SwipeDate.Value.Date <= CurrentmonthEnd).Select(y => y.IsLocked).FirstOrDefault();

                        if (chklockstatus != null)
                        {
                            LockStatus = chklockstatus;
                        }
                        if (z.Employee.ServiceBookDates.RetirementDate != null)
                        {

                            if (z.Employee.ServiceBookDates.RetirementDate.Value.Date <= CurrentmonthEnd.Date)
                            {
                                CurrentmonthEnd = z.Employee.ServiceBookDates.RetirementDate.Value.Date;
                                RetProDays = 0;

                            }
                            else if (z.Employee.ServiceBookDates.RetirementDate.Value.Date >= CurrentmonthEnd.Date && z.Employee.ServiceBookDates.RetirementDate.Value.Date <= end.Date)
                            {
                                RetProDays = (z.Employee.ServiceBookDates.RetirementDate.Value.Date - CurrentmonthEnd).Days;

                            }
                        }
                        var chkk = z.ProcessedData.Where(a => a.SwipeDate.Value.Date >= Currentmonthstart &&
                                                                        a.SwipeDate.Value.Date <= CurrentmonthEnd).ToList();
                        foreach (var item in chkk)
                        {
                            if (item.MusterRemarks.LookupVal.ToUpper() == "HO" || item.MusterRemarks.LookupVal.ToUpper() == "WO")
                            {
                                HOWO = HOWO + 1;
                            }
                            if (item.PresentStatus.LookupVal.ToUpper() == "F")
                            {
                                Fday = Fday + 1;
                            }
                            else if (item.PresentStatus.LookupVal.ToUpper() == "H")
                            {
                                Hday = Hday + 0.5;
                                AAday = AAday + 0.5;
                            }
                            else if (item.PresentStatus.LookupVal.ToUpper() == "-" && (item.MusterRemarks.LookupVal.ToUpper() != "UA" && item.MusterRemarks.LookupVal.ToUpper() != "AA"))
                            {
                                if (item.OldMusterRemarks == null)
                                {
                                    Musterclear = Musterclear + 1;
                                }
                                else if (item.MusterRemarks.LookupVal.ToUpper() == item.OldMusterRemarks.LookupVal.ToUpper() && item.MusterRemarks.LookupVal.ToUpper() == "LF")// apply action for leave but leave balance 0 so old and new remark LF this treat as absent
                                {
                                    AAday = AAday + 1;
                                }
                            }
                            else if (item.PresentStatus.LookupVal.ToUpper() == "-" && item.MusterRemarks.LookupVal.ToUpper() == "AA")
                            {
                                AAday = AAday + 1;
                            }
                            else
                            {
                                UAday = UAday + 1;
                            }
                        }


                        var chkkrec = z.ProcessedData.Where(a => a.SwipeDate.Value.Date >= Prevmonthstart &&
                                                                      a.SwipeDate.Value.Date <= PrevmonthEnd).ToList();
                        if (ProDays != 0)//if 1 to 30 or 31 month then 01 date two time pick above and here
                        {
                            foreach (var rec in chkkrec)
                            {
                                if (rec.MusterRemarks.LookupVal.ToUpper() == "HO" || rec.MusterRemarks.LookupVal.ToUpper() == "WO")
                                {
                                    HOWO = HOWO + 1;
                                }
                                if (rec.PresentStatus.LookupVal.ToUpper() == "F")
                                {

                                }
                                else if (rec.PresentStatus.LookupVal.ToUpper() == "H")
                                {
                                    AAday = AAday + 0.5;
                                }
                                else if (rec.PresentStatus.LookupVal.ToUpper() == "-" && (rec.MusterRemarks.LookupVal.ToUpper() != "UA" && rec.MusterRemarks.LookupVal.ToUpper() != "AA"))
                                {
                                    if (rec.OldMusterRemarks == null)
                                    {
                                        Musterclear = Musterclear + 1;
                                    }
                                    else if (rec.MusterRemarks.LookupVal.ToUpper() == rec.OldMusterRemarks.LookupVal.ToUpper() && rec.MusterRemarks.LookupVal.ToUpper() == "LF")// apply action for leave but leave balance 0 so old and new remark LF this treat as absent
                                    {
                                        AAday = AAday + 1;
                                    }
                                    //Musterclear = Musterclear + 1;
                                }
                                else if (rec.PresentStatus.LookupVal.ToUpper() == "-" && rec.MusterRemarks.LookupVal.ToUpper() == "AA")
                                {
                                    AAday = AAday + 1;
                                }
                                else
                                {
                                    //Recday = Recday + 1;
                                    UAday = UAday + 1;
                                }
                            }
                        }
                        if (Prevmonthstart.Date == PrevmonthEnd.Date)
                        {
                            Recday = 0;
                        }
                        if (chkk.Count() != 0 || chkkrec.Count() != 0)
                        {
                            var uploadpolicy = db.AttendancePayrollPolicy.Where(e => e.PayProcessGroup.Id == Processgrpid).SingleOrDefault();
                            if (uploadpolicy != null)
                            {
                                //if (uploadpolicy.LWPAdjustCurSal == true)
                                //{
                                //    Paybledays = Fday + Hday + ProDays - Recday;
                                //    if (Paybledays < 0)
                                //    {
                                //        Paybledays = 0;
                                //    }
                                //}
                                //else
                                //{
                                //    Paybledays = Fday + Hday + ProDays;
                                //}
                                if (RetProDays==ProDays)
                                {
                                    if (z.Employee.ServiceBookDates.RetirementDate.Value.Date <= CurrentmonthEnd.Date)
                                    {
                                        Paybledays = CurrentmonthEnd.Day - (AAday + UAday + Musterclear);
                                    }
                                    else
                                    {
                                        Paybledays = daym - (AAday + UAday + Musterclear);
                                    }
                                   
                                }
                                else
                                {
                                    Paybledays = daym - (ProDays - RetProDays) - (AAday + UAday + Musterclear);
                                }
                                if (Paybledays<0)
                                {
                                    Paybledays = 0; 
                                }
                                if ((HOWO + AAday + UAday) >= daym)
                                {
                                    Paybledays = 0;
                                }

                            }

                            //foreach (var Sal in z.SalaryT)
                            //{
                            //    if (Sal.PayMonth == PayMonth)
                            //    {
                            view = new P2BGridData()
                            {
                                Id = z.Employee.Id,
                                Code = z.Employee.EmpCode,
                                Name = z.Employee.EmpName.FullNameFML,
                                Monthdays = daym,
                                MusterCorrectionDays = Musterclear,
                                UnauthuorisedAbentdays = UAday,
                                AuthorisedAbsentdays = AAday,
                                TotalPaybledays = Paybledays,
                                LockStatus = LockStatus
                            };
                            model.Add(view);
                            //    }
                            //}
                        }

                    }

                    SalaryList = model;

                    IEnumerable<P2BGridData> IE;
                    if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                    {
                        IE = SalaryList;
                        if (gp.searchOper.Equals("eq"))
                        {
                            jsonData = IE.Where(e => (e.Code.ToString().Contains(gp.searchString))
                                  || (e.Name.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                                  || (e.Monthdays.ToString().Contains(gp.searchString.ToUpper()))
                                  || (e.MusterCorrectionDays.ToString().Contains(gp.searchString))
                                  || (e.UnauthuorisedAbentdays.ToString().Contains(gp.searchString))
                                  || (e.AuthorisedAbsentdays.ToString().Contains(gp.searchString))
                                  || (e.TotalPaybledays.ToString().Contains(gp.searchString))
                                  || (e.LockStatus.ToString().Contains(gp.searchString))
                                  || (e.Id.ToString().Contains(gp.searchString))
                                  )
                              .Select(a => new Object[] { a.Code, a.Name, a.Monthdays, a.MusterCorrectionDays, a.UnauthuorisedAbentdays, a.AuthorisedAbsentdays, a.TotalPaybledays, a.LockStatus, a.Id }).ToList();



                            //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Code, a.Name, a.Monthdays, a.MusterCorrectionDays, a.UnauthuorisedAbentdays, a.AuthorisedAbsentdays, a.TotalPaybledays, a.LockStatus, a.Id }).ToList();
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
                            orderfuc = (c => gp.sidx == "EmpCode" ? c.Code.ToString() :
                                             gp.sidx == "Name" ? c.Name.ToString() :
                                              gp.sidx == "Monthdays" ? c.Monthdays.ToString() :
                                             gp.sidx == "MusterCorrectionDays" ? c.MusterCorrectionDays.ToString() :
                                             gp.sidx == "UnauthuorisedAbentdays" ? c.UnauthuorisedAbentdays.ToString() :
                                             gp.sidx == "AuthorisedAbsentdays" ? c.AuthorisedAbsentdays.ToString() :
                                             gp.sidx == "TotalPaybledays" ? c.TotalPaybledays.ToString() :
                                             gp.sidx == "LockStatus" ? c.LockStatus.ToString() : "");
                        }
                        if (gp.sord == "asc")
                        {
                            IE = IE.OrderBy(orderfuc);
                            jsonData = IE.Select(a => new Object[] { a.Code, a.Name, a.Monthdays, a.MusterCorrectionDays, a.UnauthuorisedAbentdays, a.AuthorisedAbsentdays, a.TotalPaybledays, a.LockStatus, a.Id }).ToList();
                        }
                        else if (gp.sord == "desc")
                        {
                            IE = IE.OrderByDescending(orderfuc);
                            jsonData = IE.Select(a => new Object[] { a.Code, a.Name, a.Monthdays, a.MusterCorrectionDays, a.UnauthuorisedAbentdays, a.AuthorisedAbsentdays, a.TotalPaybledays, a.LockStatus, a.Id }).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Code, a.Name, a.Monthdays, a.MusterCorrectionDays, a.UnauthuorisedAbentdays, a.AuthorisedAbsentdays, a.TotalPaybledays, a.LockStatus, a.Id }).ToList();
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
        }






    }
}