using P2b.Global;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using P2BUltimate.Process;
using Payroll;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using P2BUltimate.Security;

namespace P2BUltimate.Controllers.Payroll.MainController
{
    [AuthoriseManger]
    public class ITProjectionController : Controller
    {
        //  private DataBaseContext db = new DataBaseContext();
        public ActionResult Index()
        {
            //   db.RefreshAllEntites(RefreshMode.StoreWins);
            //    db.RefreshAllEntites(RefreshMode.StoreWins);
            // db.RefreshAllEntites(RefreshMode.StoreWins);
            return View("~/Views/Payroll/MainViews/ITProjection/Index.cshtml");
        }

        public ActionResult ChkProcess(string typeofbtn, string month)
        {
            using (DataBaseContext db = new DataBaseContext())
            {


                Calendar finanialyear = new Calendar();
                if (month != null)
                {
                    Int32 id = int.Parse(month);
                    finanialyear = db.Calendar.Where(e => e.Id == id).SingleOrDefault();
                }
                bool selected = false;
                //List<ITProjection> query = db.ITProjection.Include(e => e.FinancialYear).Where(f => f.FinancialYear.Id == finanialyear.Id).AsNoTracking().AsParallel().ToList();
                //if (query.Count() > 0)
                //{
                //    selected = true;
                //}
                var Emplist = db.ITProjection.Where(f => f.FinancialYear.Id == finanialyear.Id).AsNoTracking().Select(r => r.EmployeePayroll_Id).ToList();
                foreach (var emp in Emplist)
                {
                    var EmpExist = 0;
                    EmpExist = db.EmployeePayroll.Where(e => e.Id == emp).Select(e => e.Id).FirstOrDefault();
                    if (EmpExist != 0)
                    {
                        selected = true;
                        break;
                    }
                };

                var data = new
                {
                    status = selected,
                };
                return Json(data, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Polulate_PayProcessGroup(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (!String.IsNullOrEmpty(Session["CompId"].ToString()))
                {
                    int CompId = int.Parse(Session["CompId"].ToString());
                    var query = db.Company.Include(e => e.PayProcessGroup).Where(e => e.Id == CompId).SingleOrDefault();
                    var selected = (Object)null;
                    if (data2 != "" && data != "0" && data2 != "0")
                    {
                        selected = Convert.ToInt32(data2);
                    }

                    SelectList s = new SelectList(query.PayProcessGroup, "Id", "FullDetails", selected);
                    return Json(s, JsonRequestBehavior.AllowGet);
                }
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult GetPayscaleagreement(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var a = db.PayProcessGroup.Find(int.Parse(data));
                return Json(a, JsonRequestBehavior.AllowGet);
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


                    //if (data.ToString().Split('/')[0] == "03")
                    //{
                    //    a = new List<ProcType>()
                    //{
                    //    new ProcType() { Id = 0, Text = "Actual Investment & Actual Income" } 
                    //};
                    //}
                    //else
                    //{
                    a = new List<ProcType>()
                    {
                        new ProcType() { Id = 0, Text = "Actual Investment & Actual Income" }, 
                        new ProcType() { Id = 1, Text = "Declare Investment & Projected Income" },
                        new ProcType() { Id = 2, Text = "Actual Investment & Projected Income" }
                    };

                    //}

                    SelectList s = new SelectList(a, "Id", "Text");
                    return Json(s, JsonRequestBehavior.AllowGet);
                }
                return Json("", JsonRequestBehavior.AllowGet);
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
                    .Where(r => r.Id == Emp).AsParallel().SingleOrDefault();
            }
        }
        public EmployeePayroll _returnITInvestmentPayment(Int32 Emp, int OFinancia)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //return db.EmployeePayroll.Include(e => e.Employee)//.Include(e => e.EmpSalStruct)
                //        .Include(e => e.Employee.EmpOffInfo)
                //        .Include(e => e.Employee.EmpOffInfo.NationalityID)
                //        .Include(e => e.Employee.Gender)
                //          .Include(e => e.Employee.ServiceBookDates)
                //           .Include(e => e.Employee.EmpName)
                //           .Include(e => e.ITProjection)
                //           .Include(e => e.ITProjection.Select(r => r.FinancialYear))
                //        .Where(e => e.Employee.Id == Emp).AsParallel().SingleOrDefault();
               
                var OEmployeePayroll = new EmployeePayroll();
                OEmployeePayroll = db.EmployeePayroll.Where(e => e.Employee.Id == Emp).FirstOrDefault();
                var OEmp = db.EmployeePayroll.Where(e => e.Employee.Id == Emp).Select(r => r.Employee).FirstOrDefault();
                var OEmpOff = db.EmployeePayroll.Where(e => e.Employee.Id == Emp).Select(r => r.Employee.EmpOffInfo).FirstOrDefault();
                var NationalityID = db.Employee.Where(e => e.Id == OEmp.Id).Select(r => r.EmpOffInfo.NationalityID).FirstOrDefault();
                var Gender = db.Employee.Where(e => e.Id == OEmp.Id).Select(r => r.Gender).FirstOrDefault();
                var EmpName = db.Employee.Where(e => e.Id == OEmp.Id).Select(r => r.EmpName).FirstOrDefault();
                var ServiceBookDates = db.Employee.Where(e => e.Id == OEmp.Id).Select(r => r.ServiceBookDates).FirstOrDefault();
                var Regimi = db.EmployeePayroll.Include(e => e.RegimiScheme).Include(e => e.RegimiScheme.Select(y => y.Scheme)).Where(e => e.Id == OEmployeePayroll.Id).FirstOrDefault();
                List<RegimiScheme> RegimiScheme = Regimi.RegimiScheme.Where(e => e.FinancialYear_Id == OFinancia).ToList();

                List<ITProjection> ITProjection = db.EmployeePayroll.Where(e => e.Id == OEmployeePayroll.Id).Select(r => r.ITProjection.Where(e => e.EmployeePayroll_Id == OEmployeePayroll.Id && e.FinancialYear_Id == OFinancia).ToList()).FirstOrDefault();
                foreach (var i in ITProjection)
                {
                    i.FinancialYear = db.Calendar.Find(i.FinancialYear_Id);

                }
                OEmp.EmpName = EmpName;
                OEmp.ServiceBookDates = ServiceBookDates;
                OEmp.Gender = Gender;
                OEmpOff.NationalityID = NationalityID;
                OEmp.EmpOffInfo = OEmpOff;
                OEmployeePayroll.Employee = OEmp;
                OEmployeePayroll.ITProjection = ITProjection;
                OEmployeePayroll.RegimiScheme = RegimiScheme;
                return OEmployeePayroll;
               
                

            }
        }

        public EmployeePayroll _returnEmployeePayrollOne(Int32 empid)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                EmployeePayroll oEmployeePayroll = db.EmployeePayroll
                                   .Include(e => e.Employee.EmpName)
                                   .Include(e => e.Employee.ServiceBookDates)
                                   .Where(e => e.Id == empid)
                                   .FirstOrDefault();
                return oEmployeePayroll;
            }
        }

        public ActionResult Create(ITProjection IT, FormCollection form) //Create submit
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {

                    DateTime processdate = DateTime.Now;

                    string Emp = form["Employee-Table"] == "0" ? "" : form["Employee-Table"];
                    string PayProcessGroupList = form["PayProcessGroupList"] == "0" ? "" : form["PayProcessGroupList"];
                    string FinancialYear = form["txtfinancialyear_id"] == "0" ? "" : form["txtfinancialyear_id"];
                    string finance = form["FinancialYearList"] == "0" ? "" : form["FinancialYearList"];
                    string ProcType1 = form["ProcTypeList"] == "" ? "" : form["ProcTypeList"];

                    var Comp_id = int.Parse(finance);
                    var OFinancia = db.Calendar.Where(e => e.Id == Comp_id).SingleOrDefault();
                    int Calendar_Id = Convert.ToInt32(OFinancia.Id);
                    DateTime? FromDate = db.Calendar.Where(e => e.Id == Calendar_Id)
                                  .Select(e => e.FromDate)
                                 .SingleOrDefault();
                    DateTime? ToDate = db.Calendar.Where(e => e.Id == Calendar_Id)
                                  .Select(e => e.ToDate)
                                 .SingleOrDefault();

                    DateTime FromPeriod = Convert.ToDateTime(FromDate);
                    IT.FromPeriod = FromPeriod;
                    DateTime ToPeriod = Convert.ToDateTime(ToDate);
                    IT.ToPeriod = ToPeriod;
                    if (ProcType1 == "")
                    {
                        Msg.Add(" Kindly select Tax Calculation method");
                        return Json(new { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    if (finance != null && finance != "")
                    {
                        var id = int.Parse(finance);
                        var value = db.Calendar.Where(e => e.Id == id).SingleOrDefault();
                        IT.FinancialYear = value;

                    }
                    int CompId = 0;
                    if (!String.IsNullOrEmpty(Session["CompId"].ToString()))
                    {
                        CompId = int.Parse(Session["CompId"].ToString());
                    }

                    List<int> ids = null;
                    if (Emp != null && Emp != "0" && Emp != "false")
                    {
                        ids = Utility.StringIdsToListIds(Emp);
                    }
                    else
                    {
                        Msg.Add("Kindly select employee");
                        return Json(new { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    Employee OEmployee = null;
                    EmployeePayroll OEmployeePayroll = null;
                    CompanyPayroll OCompanyPayroll = null;

                    foreach (var i in ids)
                    {
                        int ProcType = Convert.ToInt32(ProcType1);
                        OEmployee = _returnEmployeePayroll(i);
                        OEmployeePayroll = _returnITInvestmentPayment(OEmployee.Id, IT.FinancialYear.Id);
                        OCompanyPayroll = db.CompanyPayroll.Where(e => e.Company.Id == OEmployee.GeoStruct.Company.Id).SingleOrDefault();

                        List<EmployeePayroll> emp_list = new List<EmployeePayroll>();
                        List<int> HRAt = new List<int>();
                        List<int> ITSection10Payment = new List<int>();

                        var HRAt1 = db.EmployeePayroll
                        .Where(t => t.HRATransT.Count() > 0 && ids.Contains(t.Employee.Id)).Select(e => new
                        {
                            HRATransT = e.HRATransT.Where(s => s.Financialyear.Id == Comp_id).ToList(),
                            Id = e.Id,
                        }).ToList();

                        HRAt = HRAt1.Select(e => e.Id).ToList();

                        var ITSection10Payment1 = db.EmployeePayroll
                           .Where(t => t.ITSection10Payment.Count() > 0 && ids.Contains(t.Employee.Id)).Select(e => new
                           {
                               ITSection10Payment = e.ITSection10Payment.Where(s => s.FinancialYear.Id == Comp_id).ToList(),
                               Id = e.Id,
                           }).ToList();


                        ITSection10Payment = ITSection10Payment1.Select(e => e.Id).ToList();

                        var list2 = HRAt.Except(ITSection10Payment);

                        if (list2.Count() > 0)
                        {
                            foreach (var EmpId in list2)
                            {
                                EmployeePayroll EmpPay = _returnEmployeePayrollOne(EmpId);

                                emp_list.Add(EmpPay);
                                Msg.Add(EmpPay.Employee.FullDetails + ", HRA rent is entered and ITSection10Payment of this employee is not entered ,Please enter ITSection10Payment and then process Itprojection ,");

                            }
                        }

                        if (Msg.Count() > 0)
                        {
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }

                        SalaryT OSal = null;
                        List<SalaryT> OSalmonth = db.SalaryT.Where(e => e.EmployeePayroll_Id == OEmployeePayroll.Id).ToList();
                        string OSalmontht = OSalmonth.Where(e => Convert.ToDateTime("01/" + e.PayMonth) >= IT.FinancialYear.FromDate.Value && Convert.ToDateTime("01/" + e.PayMonth) <= IT.FinancialYear.ToDate.Value).OrderByDescending(e => e.Id).Select(q => q.PayMonth).FirstOrDefault();
                        if (OSalmontht != "")
                        {
                            OSal = db.SalaryT.Where(e => e.PayMonth == OSalmontht && e.EmployeePayroll_Id == OEmployeePayroll.Id).SingleOrDefault();

                        }
                        else
                        {
                            OSal = null;
                        }
                        FromPeriod = Convert.ToDateTime(FromDate);
                        ToPeriod = Convert.ToDateTime(ToDate);
                        // double status = 1;
                        if (OEmployee.ServiceBookDates.JoiningDate >= IT.FinancialYear.FromDate)
                        {
                            FromPeriod = Convert.ToDateTime(OEmployee.ServiceBookDates.JoiningDate);
                            //IncomeTaxCalc.ITCalculation(OEmployeePayroll, CompId, IT.FinancialYear, FromPeriod, ToPeriod, processdate, db);
                            IncomeTaxCalc.ITCalculation(OEmployeePayroll, CompId, IT.FinancialYear.Id, FromPeriod, ToPeriod, processdate, OSal, null, 1, ProcType);
                        }
                        else if (OEmployee.ServiceBookDates.ServiceLastDate >= IT.FinancialYear.FromDate &&
                           OEmployee.ServiceBookDates.ServiceLastDate <= IT.FinancialYear.ToDate)
                        {
                            ToPeriod = Convert.ToDateTime(OEmployee.ServiceBookDates.ServiceLastDate);
                            //IncomeTaxCalc.ITCalculation(OEmployeePayroll, CompId, IT.FinancialYear, FromPeriod, ToPeriod, processdate, db);
                            IncomeTaxCalc.ITCalculation(OEmployeePayroll, CompId, IT.FinancialYear.Id, FromPeriod, ToPeriod, processdate, OSal, null, 1, ProcType);
                        }
                        else if (OEmployee.ServiceBookDates.RetirementDate >= IT.FinancialYear.FromDate &&
                           OEmployee.ServiceBookDates.RetirementDate <= IT.FinancialYear.ToDate)
                        {
                            ToPeriod = Convert.ToDateTime(OEmployee.ServiceBookDates.RetirementDate);
                            IncomeTaxCalc.ITCalculation(OEmployeePayroll, CompId, IT.FinancialYear.Id, FromPeriod, ToPeriod, processdate, OSal, null, 1, ProcType);
                        }
                        else
                        {

                            IncomeTaxCalc.ITCalculation(OEmployeePayroll, CompId, IT.FinancialYear.Id, FromPeriod, ToPeriod, processdate, OSal, null, 1, ProcType);
                        }

                        //  ts.Complete();

                        //}
                    }
                }

                catch (Exception ex)
                {
                    throw;
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
                //return Json(new Object[] { "", "", "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
                //  List<string> Msgs = new List<string>();
                Msg.Add("Data Saved successfully");
                watch.Stop();
                Utility.DumpProcessStatus(watch.Elapsed.Hours + ":" + watch.Elapsed.Minutes + ":" + watch.Elapsed.Seconds + ":" + watch.Elapsed.Milliseconds);
                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

            }
        }

        public ActionResult GetCalendarDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "FINANCIALYEAR").ToList();
                IEnumerable<Calendar> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.Calendar.ToList().Where(d => d.FullDetails.Contains(data));
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


        public class P2BGridData
        {
            public int Id { get; set; }
            public int EmpId { get; set; }
            public string EmpCode { get; set; }
            public string Name { get; set; }
            public double ActualAmount { get; set; }
            public double ActualQualifyingAmount { get; set; }
            public string ChapterName { get; set; }
            public string Form16Header { get; set; }
            public string Form24Header { get; set; }
            public string FinancialYear { get; set; }
            public int PickupId { get; set; }
            public double ProjectedAmount { get; set; }
            public double ProjectedQualifyingAmount { get; set; }
            public string ProjectionDate { get; set; }
            public double QualifiedAmount { get; set; }
            public int SalayHead { get; set; }
            public string Section { get; set; }
            public string SectionType { get; set; }
            public string SubChapter { get; set; }
            public double TDSComponents { get; set; }
            public DateTime? FromPeriod { get; set; }
            public DateTime? Toperiod { get; set; }
            public string title { get; set; }
            public bool Islock { get; set; }
            public string Narration { get; set; }
        }

        public ActionResult P2BGrid(P2BGrid_Parameters gp)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    int Id = Convert.ToInt32(gp.id);
                    int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                    int pageSize = gp.rows;
                    int totalPages = 0;
                    int totalRecords = 0;
                    var jsonData = (Object)null;

                    IEnumerable<P2BGridData> ITProjectionList = null;
                    List<P2BGridData> model = new List<P2BGridData>();
                    P2BGridData view = null;
                    Calendar fall = null;
                    string PayMonth = "";
                    if (gp.filter != null)
                    {
                        var fromdate = gp.filter.Substring(33, 10);
                        var todate = gp.filter.Substring(53, 11);

                        DateTime finfromdate = Convert.ToDateTime(fromdate);
                        DateTime finTodate = Convert.ToDateTime(todate);
                        fall = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "FINANCIALYEAR" && e.FromDate == finfromdate && e.ToDate == finTodate).SingleOrDefault();


                    }

                    if (gp.filter != null)
                        PayMonth = gp.filter;
                    if (PayMonth != null && PayMonth != "")
                    {

                        // var financialyear = db.Calendar.Find(int.Parse(PayMonth));

                        var BindEmpList = db.EmployeePayroll.Include(e => e.Employee.EmpName).Include(e => e.ITProjection)
                            .Include(e => e.ITProjection.Select(r => r.FinancialYear))
                            .AsNoTracking().AsParallel().ToList();

                        foreach (var z in BindEmpList)
                        {
                            if (z.ITProjection != null && z.ITProjection.Count() > 0)
                            {
                                var all = z.ITProjection.Where(x => x.FinancialYear.Id == fall.Id).GroupBy(e => e.ProjectionDate).Select(e => e.FirstOrDefault()).SingleOrDefault();
                                if (all != null)
                                {
                                    if (all.FinancialYear.Id == fall.Id)
                                    {
                                        view = new P2BGridData()
                                        {
                                            Id = all.Id,
                                            EmpId = z.Employee.Id,
                                            EmpCode = z.Employee.EmpCode,
                                            Name = z.Employee.EmpName.FullNameFML,

                                            FromPeriod = all.FromPeriod,
                                            Toperiod = all.ToPeriod,
                                            title = all.Tiltle,
                                            Islock = all.IsLocked,
                                            Narration = all.Narration != null ? all.Narration.ToString() : "",
                                            ProjectionDate = all.ProjectionDate.Value.ToString("dd/MM/yyyy")
                                        };

                                        model.Add(view);
                                    }
                                }
                            }

                        }

                    }
                    else
                    {
                        List<string> Msgu = new List<string>();
                        Msgu.Add("  Financial Year Not Selected ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msgu }, JsonRequestBehavior.AllowGet);
                        //return Json(new Object[] { "", "", "Financial Year Not Selected", JsonRequestBehavior.AllowGet });
                    }
                    ITProjectionList = model;

                    IEnumerable<P2BGridData> IE;
                    if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                    {
                        IE = ITProjectionList;
                        if (gp.searchOper.Equals("eq"))
                        {
                            jsonData = IE.Where(e => (e.EmpCode.ToString().Contains(gp.searchString))
                                    || (e.EmpId.ToString().Contains(gp.searchString))
                                    || (e.Name.ToString().Contains(gp.searchString))
                                    || (e.Islock.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                                    || (e.Id.ToString().Contains(gp.searchString))
                                    ).Select(a => new Object[] { Convert.ToString(a.EmpCode), a.EmpId, a.Name, a.ProjectionDate, Convert.ToString(a.Islock), a.Id }).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.EmpId, Convert.ToString(a.EmpCode), a.Name, a.ProjectionDate, Convert.ToString(a.Islock), a.Id }).ToList();
                        }
                        totalRecords = IE.Count();
                    }
                    else
                    {
                        IE = ITProjectionList;
                        Func<P2BGridData, dynamic> orderfuc;
                        if (gp.sidx == "Id")
                        {
                            orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                        }
                        else
                        {
                            orderfuc = (c => gp.sidx == "Code" ? c.EmpCode.ToString() :
                                             gp.sidx == "EmpId" ? c.EmpId.ToString() :
                                             gp.sidx == "Name" ? c.Name.ToString() :
                                             gp.sidx == "Islock" ? c.Islock.ToString() :
                                             "");
                        }
                        if (gp.sord == "asc")
                        {
                            IE = IE.OrderBy(orderfuc);
                            jsonData = IE.Select(a => new Object[] { Convert.ToString(a.EmpCode), a.EmpId, a.Name, a.ProjectionDate, Convert.ToString(a.Islock), a.Id }).ToList();
                        }
                        else if (gp.sord == "desc")
                        {
                            IE = IE.OrderByDescending(orderfuc);
                            jsonData = IE.Select(a => new Object[] { Convert.ToString(a.EmpCode), a.EmpId, a.Name, a.ProjectionDate, Convert.ToString(a.Islock), a.Id }).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.EmpCode), a.EmpId, a.Name, a.ProjectionDate, Convert.ToString(a.Islock), a.Id }).ToList();
                        }
                        totalRecords = ITProjectionList.Count();
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
                    List<string> Msg = new List<string>();
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

        public ActionResult ReleaseProcess(string forwardata, string PayMonth)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    var Ofinanialyear = db.Calendar.Find(int.Parse(PayMonth));
                    List<int> ids = null;
                    if (forwardata == "false" || forwardata == null || forwardata == "0")
                    {
                        Msg.Add(" Unable To Forward Data  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        //  return this.Json(new Object[] { "", "", "", JsonRequestBehavior.AllowGet });
                    }
                    if (forwardata != null && forwardata != "0" && forwardata != "false")
                    {
                        ids = Utility.StringIdsToListIds(forwardata);
                    }

                    List<ITProjection> itprojectiondata = new List<ITProjection>();
                    foreach (var i in ids)
                    {

                        //using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                        //                   new System.TimeSpan(0, 30, 0)))
                        //{

                        //using (DataBaseContext db2 = new DataBaseContext())
                        //{

                        var ITProject = db.EmployeePayroll
                                      .Include(e => e.Employee)
                                      .Include(e => e.ITProjection)
                                      .Include(e => e.ITProjection.Select(t => t.FinancialYear))
                                      .Where(e => e.Employee.Id == i).SingleOrDefault();

                        if (ITProject != null)
                        {
                            //foreach (var a in ITProject)
                            //{
                            List<ITProjection> itprojectiolist = ITProject.ITProjection.Where(t => t.FinancialYear.Id == Ofinanialyear.Id).ToList();
                            //if (itprojectiolist.Count() > 0)
                            //{
                            //    itprojectiondata.AddRange(itprojectiolist);
                            //}
                            //foreach (var a1 in itprojectiolist)
                            //{
                            //    a1.IsLocked = true;
                            //    db.ITProjection.Attach(a1);
                            //    db.Entry(a1).State = System.Data.Entity.EntityState.Modified;
                            //    db.SaveChanges();
                            //    TempData["RowVersion"] = a.RowVersion;

                            //}
                            if (itprojectiolist.Count > 0)
                            {
                                using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, new System.TimeSpan(0, 30, 0)))
                                {

                                    itprojectiolist.ForEach(q => q.IsLocked = true);
                                    // db.Entry;
                                    //  db.Entry(salaryt).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    ts.Complete();
                                    // TempData["RowVersion"] = SalT.RowVersion;
                                    //  db.Entry(salaryt).State = System.Data.Entity.EntityState.Detached;
                                }
                            }

                            //}
                            //db.Entry(ITProject).State = System.Data.Entity.EntityState.Detached;
                        }
                        // }
                        // ts.Complete();

                        //}
                    }
                    return Json(new { success = true, responseText = "ITProjection has been Locked." }, JsonRequestBehavior.AllowGet);
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
                return View();
            }
        }


        public ActionResult Edit(int data)
        {

            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.ITProjection.Include(e => e.FinancialYear)
                    .Where(e => e.Id == data).Select(e => new
                    {

                        ActualAmount = e.ActualAmount,
                        ActualQualifyingAmount = e.ActualQualifyingAmount,
                        ChapterName = e.ChapterName,
                        FinancialYear_Id = e.FinancialYear.Id,
                        FinancialYear_FullDetails = e.FinancialYear.FullDetails,
                        Form16Header = e.Form16Header,
                        Form24Header = e.Form24Header,
                        FromPeriod = e.FromPeriod,
                        IsLocked = e.IsLocked,
                        Narration = e.Narration,
                        PickupId = e.PickupId,
                        ProjectedAmount = e.ProjectedAmount,
                        ProjectedQualifyingAmount = e.ProjectedQualifyingAmount,
                        ProjectionDate = e.ProjectionDate,
                        QualifiedAmount = e.QualifiedAmount,
                        SalayHead = e.SalayHead,
                        Section = e.Section,
                        SectionType = e.SectionType,
                        SubChapter = e.SubChapter,
                        TDSComponents = e.TDSComponents,
                        Tiltle = e.Tiltle,
                        ToPeriod = e.ToPeriod


                    }).ToList();

                var Corp = db.ITProjection.Find(data);
                //TempData["RowVersion"] = Corp.RowVersion;
                // var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, "", "", "", JsonRequestBehavior.AllowGet });

            }
        }


        public ActionResult ViewProjection(P2BGrid_Parameters gp)//, string data
        {
            int Id = Convert.ToInt32(gp.id);
            try
            {
                using (DataBaseContext db = new DataBaseContext())
                {
                    int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                    int pageSize = gp.rows;
                    int totalPages = 0;
                    int totalRecords = 0;
                    var jsonData = (Object)null;

                    IEnumerable<P2BGridData> ITProjectionList = null;
                    List<P2BGridData> model = new List<P2BGridData>();
                    P2BGridData view = null;
                    string PayMonth = "";

                    //if (gp.filter != null)
                    //    PayMonth = gp.filter;




                    //var BindEmpList = db.EmployeePayroll.Include(e => e.Employee.EmpName).Include(e => e.SalaryT)
                    //    .Where(e => e.SalaryT.Any(u => u.PayMonth != "01/2017")).ToList();

                    var BindEmpList = db.EmployeePayroll.Include(e => e.Employee.EmpName).Include(e => e.ITProjection).Include(e => e.ITProjection.Select(t => t.FinancialYear))
                                      .Where(e => e.ITProjection.Any(t => t.Id == Id))
                                      .ToList();

                    foreach (var z in BindEmpList)
                    {
                        if (z.ITProjection != null)
                        {

                            foreach (var Sal in z.ITProjection)
                            {

                                view = new P2BGridData()
                                {
                                    Id = z.Employee.Id,
                                    EmpCode = z.Employee.EmpCode,
                                    Name = z.Employee.EmpName.FullNameFML,
                                    ActualAmount = Sal.ActualAmount,
                                    ActualQualifyingAmount = Sal.ActualQualifyingAmount,
                                    ChapterName = Sal.ChapterName,
                                    FinancialYear = Sal.FinancialYear.FullDetails != null ? Sal.FinancialYear.FullDetails.ToString() : "",
                                    Form16Header = Sal.Form16Header,
                                    Form24Header = Sal.Form24Header,
                                    PickupId = Sal.PickupId,
                                    ProjectedAmount = Sal.ProjectedAmount,
                                    ProjectedQualifyingAmount = Sal.ProjectedQualifyingAmount,
                                    ProjectionDate = Sal.ProjectionDate != null ? Sal.ProjectionDate.ToString() : "",
                                    QualifiedAmount = Sal.QualifiedAmount,
                                    SalayHead = Sal.SalayHead,
                                    Section = Sal.Section != null ? Sal.Section : "",
                                    SectionType = Sal.SectionType != null ? Sal.SectionType : "",
                                    SubChapter = Sal.SubChapter != null ? Sal.SubChapter : "",
                                    TDSComponents = Sal.TDSComponents,
                                    FromPeriod = Sal.FromPeriod,
                                    Toperiod = Sal.ToPeriod,
                                    title = Sal.Tiltle,
                                    Islock = Sal.IsLocked,
                                    Narration = Sal.Narration != null ? Sal.Narration.ToString() : ""
                                };
                                model.Add(view);

                            }
                        }

                    }



                    ITProjectionList = model;

                    IEnumerable<P2BGridData> IE;
                    if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                    {
                        IE = ITProjectionList;
                        if (gp.searchOper.Equals("eq"))
                        {
                            jsonData = IE.Where(e => (e.EmpCode.ToString().Contains(gp.searchString))
                               || (e.title.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                               || (e.Section.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                               || (e.ChapterName.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                               || (e.ProjectedAmount.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                               || (e.ActualAmount.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                               || (e.ActualQualifyingAmount.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                               || (e.ProjectedQualifyingAmount.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                               || (e.Narration.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                               || (e.Id.ToString().Contains(gp.searchString))
                               ).Select(a => new Object[] { a.EmpCode, a.title, a.Section, a.ChapterName, a.ProjectedAmount, a.ActualAmount, a.ActualQualifyingAmount, a.ProjectedQualifyingAmount, a.Narration, a.Id }).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.EmpCode, a.title, a.Section, a.ChapterName, a.ProjectedAmount, a.ActualAmount, a.ActualQualifyingAmount, a.ProjectedQualifyingAmount, a.Narration, a.Id }).ToList();
                        }
                        totalRecords = IE.Count();
                    }
                    else
                    {
                        IE = ITProjectionList;
                        Func<P2BGridData, dynamic> orderfuc;
                        if (gp.sidx == "Id")
                        {
                            orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                        }
                        else
                        {
                            orderfuc = (c => gp.sidx == "Code" ? c.EmpCode.ToString() :
                                             gp.sidx == "Title" ? c.title.ToString() :
                                             gp.sidx == "Section" ? c.Section.ToString() :
                                             gp.sidx == "ChapterName" ? c.ChapterName.ToString() :
                                             gp.sidx == "ProjectedAmount" ? c.ProjectedAmount.ToString() :
                                             gp.sidx == "ActualAmount" ? c.ActualAmount.ToString() :
                                             gp.sidx == "ActualQualifyingAmount" ? c.ActualQualifyingAmount.ToString() :
                                             gp.sidx == "ProjectedQualifyingAmount" ? c.ProjectedQualifyingAmount.ToString() :
                                             gp.sidx == "Narration" ? c.Narration.ToString() :
                                            
                                             "");
                        }
                        if (gp.sord == "asc")
                        {
                            IE = IE.OrderBy(orderfuc);
                            jsonData = IE.Select(a => new Object[] { a.EmpCode, a.title, a.Section, a.ChapterName, a.ProjectedAmount, a.ActualAmount, a.ActualQualifyingAmount, a.ProjectedQualifyingAmount, a.Narration, a.Id }).ToList();
                        }
                        else if (gp.sord == "desc")
                        {
                            IE = IE.OrderByDescending(orderfuc);
                            jsonData = IE.Select(a => new Object[] { a.EmpCode, a.title, a.Section, a.ChapterName, a.ProjectedAmount, a.ActualAmount, a.ActualQualifyingAmount, a.ProjectedQualifyingAmount, a.Narration, a.Id }).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.EmpCode, a.title, a.Section, a.ChapterName, a.ProjectedAmount, a.ActualAmount, a.ActualQualifyingAmount, a.ProjectedQualifyingAmount, a.Narration, a.Id }).ToList();
                        }
                        totalRecords = ITProjectionList.Count();
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
    }
}