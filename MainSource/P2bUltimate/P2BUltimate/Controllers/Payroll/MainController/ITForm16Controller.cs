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
using System.Data.Entity.Core.Objects;
using System.IO;
using System.Net;

namespace P2BUltimate.Controllers.Payroll.MainController
{
    [AuthoriseManger]
    public class ITForm16Controller : Controller
    {
        //private DataBaseContext db = new DataBaseContext();
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/ITForm16/Index.cshtml");
        }

        public ActionResult ChkProcess(string typeofbtn, string month)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var finanialyear = new Calendar();
                if (month != null)
                {
                    finanialyear = db.Calendar.Find(int.Parse(month));
                }
                bool selected = false;

                var query = db.ITForm16Data.Include(e => e.FinancialYear).ToList();
                //var financialyear = query.Select(e => e.FinancialYear == finanialyear).SingleOrDefault();
                var financialyearR = query.Where(f => f.FinancialYear == finanialyear);
                if (financialyearR.Count() > 0) //chgd by rekha09122017
                {
                    selected = true;
                }
                var data = new
                {
                    status = selected,
                };
                return Json(data, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult GetSignPersonList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var qurey = db.ITForm16SigningPerson.ToList();
                data2 = db.ITForm16SigningPerson.Where(e => e.IsDefault == true).SingleOrDefault() != null ? db.ITForm16SigningPerson.Where(e => e.IsDefault == true).SingleOrDefault().Id.ToString() : "0";
                var selected = (Object)null;
                if (data2 != "" && data != "0" && data2 != "0")
                {
                    selected = Convert.ToInt32(data2);
                }

                SelectList s = new SelectList(qurey, "Id", "SigningPersonFullName", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult Create(ITForm16Data IT, FormCollection form) //Create submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    DateTime processdate = DateTime.Now;
                    string Emp = form["Employee-Table"] == "0" ? "" : form["Employee-Table"];
                    string PayProcessGroupList = form["PayProcessGroupList"] == "0" ? "" : form["PayProcessGroupList"];
                    string FinancialYear = form["txtfinancialyear_id"] == "0" ? "" : form["txtfinancialyear_id"];
                    string finance = form["FinancialYearList"] == "0" ? "" : form["FinancialYearList"];
                    int SigningPerson = form["SignPersonlist"] == "0" ? 0 : Convert.ToInt32(form["SignPersonlist"]);
                    var OFinancia = db.Calendar.Find(int.Parse(finance));
                    int Calendar_Id = Convert.ToInt32(OFinancia.Id);
                    var FromDate = db.Calendar.Where(e => e.Id == Calendar_Id)
                                  .Select(e => e.FromDate)
                                 .SingleOrDefault();
                    var ToDate = db.Calendar.Where(e => e.Id == Calendar_Id)
                                  .Select(e => e.ToDate)
                                 .SingleOrDefault();

                    DateTime FromPeriod = Convert.ToDateTime(FromDate);
                    DateTime ToPeriod = Convert.ToDateTime(ToDate);


                    if (finance != null && finance != "")
                    {
                        var value = db.Calendar.Find(int.Parse(finance));
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

                    if (SigningPerson == 0)
                    {
                        Msg.Add("Kindly Select Signing Authority.");
                        return Json(new { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    if (db.ITForm16Quarter.Where(e => e.QuarterFromDate >= FromPeriod && e.QuarterToDate <= ToPeriod).Any() == false)
                    {
                        Msg.Add("Kindly process IT Challan & Quarterly Returns before Form16.");
                        return Json(new { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    EmployeePayroll OEmployeePayroll = null;

                    Employee OEmployeeIT = null;
                    EmployeePayroll OEmployeePayrollIT = null;

                    foreach (var i in ids)
                    {
                        OEmployeePayroll = db.EmployeePayroll.Where(e => e.Employee.Id == i).SingleOrDefault();

                        OEmployeeIT = _returnEmployeePayroll(i);
                        OEmployeePayrollIT = _returnITInvestmentPayment(OEmployeeIT.Id);
                        OEmployeePayrollIT.RegimiScheme = OEmployeePayrollIT.RegimiScheme.Where(e => e.FinancialYear_Id == IT.FinancialYear.Id).ToList();

                        using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                           new System.TimeSpan(0, 30, 0)))
                        {

                            IncomeTaxCalc.ITCalculation(OEmployeePayrollIT, CompId, IT.FinancialYear.Id, FromPeriod, ToPeriod, processdate, null, null, 1, 0);

                            IncomeTaxCalc.ITForm16PartB(OEmployeePayroll.Id, CompId, IT.FinancialYear.Id, FromPeriod, ToPeriod, processdate, SigningPerson);
                            ts.Complete();

                        }
                    }

                    Msg.Add("Data Saved successfully");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }

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

        public EmployeePayroll _returnITInvestmentPayment(Int32 Emp)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                return db.EmployeePayroll.Include(e => e.Employee)//.Include(e => e.EmpSalStruct)
                    //         .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails))
                    //         .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead)))
                    //         .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.Type)))
                    //         .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.SalHeadOperationType)))
                    //         .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.ProcessType)))
                    //         .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.Frequency)))
                         .Include(e => e.Employee.EmpOffInfo)
                         .Include(e => e.Employee.EmpOffInfo.NationalityID)
                         .Include(e => e.Employee.Gender)
                           .Include(e => e.Employee.ServiceBookDates)
                            .Include(e => e.Employee.EmpName)
                            .Include(e => e.ITProjection)
                            .Include(e => e.ITProjection.Select(r => r.FinancialYear))
                             .Include(e => e.RegimiScheme).Include(e => e.RegimiScheme.Select(y => y.Scheme))
                    //.Include(e => e.ITInvestmentPayment)
                    //.Include(e => e.ITInvestmentPayment.Select(r => r.FinancialYear))
                    //.Include(e => e.ITInvestmentPayment.Select(r => r.ITSection))
                    //.Include(e => e.ITInvestmentPayment.Select(r => r.ITSection.ITSectionList))
                    //.Include(e => e.ITInvestmentPayment.Select(r => r.ITSection.ITSectionListType))
                    //.Include(e => e.ITInvestmentPayment.Select(r => r.ITSection.ITSectionList))//added 17042017 by prashant
                    //.Include(e => e.ITInvestmentPayment.Select(r => r.ITSubInvestmentPayment))
                    //.Include(e => e.ITInvestmentPayment.Select(r => r.LoanAdvanceHead))
                    //.Include(e => e.ITInvestmentPayment.Select(r => r.LoanAdvanceHead.ITLoan))
                    //.Include(e => e.ITInvestmentPayment.Select(r => r.LoanAdvanceHead.ITSection))
                    //.Include(e => e.ITInvestmentPayment.Select(r => r.LoanAdvanceHead.LoanAdvancePolicy))
                    //.Include(e => e.ITInvestmentPayment.Select(r => r.ITInvestment))
                    //.Include(e => e.ITInvestmentPayment.Select(r => r.ITInvestment.SalaryHead))
                    //.Include(e => e.ITReliefPayment)
                    //.Include(e => e.LoanAdvRequest)
                    //.Include(e => e.LoanAdvRequest.Select(r => r.LoanAdvanceHead))
                    //.Include(e => e.LoanAdvRequest.Select(r => r.LoanAdvRepaymentT))
                    //.Include(e => e.ITReliefPayment.Select(r => r.ITSection))
                         .Where(e => e.Employee.Id == Emp).AsParallel().SingleOrDefault();

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
            public string Code { get; set; }
            public string Name { get; set; }
            public string HeaderCol { get; set; }
            public string ActualAmount { get; set; }
            public string QualifyAmount { get; set; }
            public string DeductibleAmount { get; set; }
            public string FinalAmount { get; set; }
            public string Form16Header { get; set; }
            public string Form24Header { get; set; }
            public string FinancialYear { get; set; }
            public int PickupId { get; set; }
            public double ProjectedAmount { get; set; }
            public double ProjectedQualifyingAmount { get; set; }
            public string ReportDate { get; set; }
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

                        //var financialyear = db.Calendar.Find(int.Parse(PayMonth));

                        var BindEmpList = db.EmployeePayroll.Include(e => e.Employee.EmpName).Include(e => e.ITForm16Data).Include(e => e.ITForm16Data.Select(q => q.FinancialYear))
                            .ToList();

                        foreach (var z in BindEmpList)
                        {
                            if (z.ITForm16Data != null && z.ITForm16Data.Count() > 0)
                            {
                                var all = z.ITForm16Data.Where(e => e.FinancialYear.Id == fall.Id).SingleOrDefault();
                                if (all != null)
                                {
                                    view = new P2BGridData()
                                    {
                                        Id = all.Id,
                                        Code = z.Employee.EmpCode,
                                        Name = z.Employee.EmpName.FullNameFML,

                                        FromPeriod = all.PeriodFrom,
                                        Toperiod = all.PeriodTo,
                                        Islock = all.IsLocked,
                                        ReportDate = all.ReportDate.Value.ToString("dd/MM/yyyy")
                                    };

                                    model.Add(view);
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
                            jsonData = IE.Where(e => (e.Code.ToString().Contains(gp.searchString))
                                    || (e.Name.ToString().Contains(gp.searchString))
                                    || (e.ReportDate.ToString().Contains(gp.searchString))
                                    || (e.Id.ToString().Contains(gp.searchString))
                                    ).Select(a => new Object[] { a.Code, a.Name, a.ReportDate, a.Islock, a.Id }).ToList();

                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.Code), a.Name, a.ReportDate, Convert.ToString(a.Islock), a.Id }).ToList();
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
                            orderfuc = (c => gp.sidx == "Code" ? c.Code.ToString() :
                                             gp.sidx == "Name" ? c.Name.ToString() :
                                             gp.sidx == "ReportDate" ? c.ReportDate.ToString() :
                                             "");
                        }
                        if (gp.sord == "asc")
                        {
                            IE = IE.OrderBy(orderfuc);
                            jsonData = IE.Select(a => new Object[] { Convert.ToString(a.Code), a.Name, a.ReportDate, Convert.ToString(a.Islock), a.Id }).ToList();
                        }
                        else if (gp.sord == "desc")
                        {
                            IE = IE.OrderByDescending(orderfuc);
                            jsonData = IE.Select(a => new Object[] { Convert.ToString(a.Code), a.Name, a.ReportDate, Convert.ToString(a.Islock), a.Id }).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.Code), a.Name, a.ReportDate, Convert.ToString(a.Islock), a.Id }).ToList();
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


        ////public ActionResult ReleaseProcess(string forwardata, string PayMonth)
        ////{
        ////    List<string> Msg = new List<string>();
        ////    using (DataBaseContext db = new DataBaseContext())
        ////    {
        ////        try
        ////        {
        ////            var Ofinanialyear = db.Calendar.Find(int.Parse(PayMonth));
        ////            List<int> ids = null;
        ////            if (forwardata == "false" || forwardata == null || forwardata == "0")
        ////            {
        ////                Msg.Add(" Unable To Forward Data  ");
        ////                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

        ////                //  return this.Json(new Object[] { "", "", "", JsonRequestBehavior.AllowGet });
        ////            }
        ////            if (forwardata != null && forwardata != "0" && forwardata != "false")
        ////            {
        ////                ids = Utility.StringIdsToListIds(forwardata);
        ////            }


        ////            foreach (var i in ids)
        ////            {

        ////                using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
        ////                                   new System.TimeSpan(0, 30, 0)))
        ////                {

        ////                    using (DataBaseContext db2 = new DataBaseContext())
        ////                    {

        ////                        var ITProject = db.ITForm16Data.Where(e => e.FinancialYear.Id == Ofinanialyear.Id).ToList();

        ////                        if (ITProject != null)
        ////                        {
        ////                            foreach (var a in ITProject)
        ////                            {
        ////                                a.IsLocked = true;
        ////                                db.ITForm16Data.Attach(a);
        ////                                db.Entry(a).State = System.Data.Entity.EntityState.Modified;
        ////                                db.SaveChanges();
        ////                                TempData["RowVersion"] = a.RowVersion;

        ////                            }
        ////                            //db.Entry(ITProject).State = System.Data.Entity.EntityState.Detached;
        ////                        }
        ////                    }
        ////                    ts.Complete();

        ////                }
        ////                return Json(new { success = true, responseText = "ITForm16 has been Locked." }, JsonRequestBehavior.AllowGet);
        ////            }
        ////        }
        ////        catch (Exception ex)
        ////        {
        ////            Msg.Add(ex.Message);
        ////            LogFile Logfile = new LogFile();
        ////            ErrorLog Err = new ErrorLog()
        ////            {
        ////                ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
        ////                ExceptionMessage = ex.Message,
        ////                ExceptionStackTrace = ex.StackTrace,
        ////                LineNo = Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
        ////                LogTime = DateTime.Now
        ////            };
        ////            Logfile.CreateLogFile(Err);
        ////            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        ////        }
        ////        return View();
        ////    }
        ////}


        public ActionResult ReleaseProcess(string forwardata, string PayMonth)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                List<int> ids = null; try
                {
                    var Ofinanialyear = db.Calendar.Find(int.Parse(PayMonth));
                    if (forwardata == "false" || forwardata == null || forwardata == "0")
                    {
                        Msg.Add(" Unable To Forward Data  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    if (forwardata != null && forwardata != "0" && forwardata != "false")
                    {
                        ids = Utility.StringIdsToListIds(forwardata);
                    }
                    

                    List<ITForm16Data> itform16datalist = new List<ITForm16Data>();
                    foreach (var i in ids)
                    {
                        // OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct).Where(r => r.Id == i).SingleOrDefault();

                        // OEmployeePayroll = db.EmployeePayroll.Include(e => e.ITForm16Data).Include(e => e.ITForm16Data.Select(t => t.FinancialYear)).Where(e => e.Employee.Id == i).SingleOrDefault();
                        ITForm16Data itform16data = db.ITForm16Data.Where(e => e.FinancialYear.Id == Ofinanialyear.Id && e.Id == i).SingleOrDefault();
                        if (itform16data != null)
                        {
                            itform16datalist.Add(itform16data);
                            //SalT.ReleaseDate = DateTime.Now.Date;
                            //db.SalaryT.Attach(SalT);
                            //db.Entry(SalT).State = System.Data.Entity.EntityState.Modified;
                            //db.SaveChanges();
                            //TempData["RowVersion"] = SalT.RowVersion;
                            //db.Entry(SalT).State = System.Data.Entity.EntityState.Detached;
                        }
                        //ts.Complete();
                    }
                    if (itform16datalist.Count > 0)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, new System.TimeSpan(0, 30, 0)))
                        {

                            itform16datalist.ForEach(q => q.IsLocked = true);
                            // db.Entry;
                            //  db.Entry(salaryt).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            ts.Complete();
                            // TempData["RowVersion"] = SalT.RowVersion;
                            //  db.Entry(salaryt).State = System.Data.Entity.EntityState.Detached;
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
                    Msg.Add(ex.Message);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                }
                return Json(new { success = true, responseText = "ITForm16 has been Locked." }, JsonRequestBehavior.AllowGet);
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
            using (DataBaseContext db = new DataBaseContext())
            {
                try
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


                    var BindEmpList = db.EmployeePayroll.Include(e => e.Employee.EmpName).Include(e => e.ITForm16Data)
                        .Include(e => e.ITForm16Data.Select(t => t.FinancialYear)).Include(e => e.ITForm16Data.Select(r => r.ITForm16DataDetails))

                                      .Where(e => e.ITForm16Data.Any(t => t.Id == Id))
                                      .ToList();

                    foreach (var z in BindEmpList)
                    {
                        if (z.ITForm16Data != null)
                        {
                            var O12BADet = z.ITForm16Data.Select(r => r.ITForm16DataDetails).ToList();
                            foreach (var Sal in O12BADet)
                            {
                                foreach (var det in Sal)
                                {
                                    view = new P2BGridData()
                                    {
                                        Id = z.Employee.Id,
                                        Code = z.Employee.EmpCode,
                                        Name = z.Employee.EmpName.FullNameFML,
                                        HeaderCol = det.HeaderCol1,
                                        ActualAmount = det.ActualAmountCol2,
                                        QualifyAmount = det.QualifyAmountCol3,
                                        DeductibleAmount = det.DeductibleAmountCol4,
                                        FinalAmount = det.FinalAmountCol5
                                    };
                                    model.Add(view);
                                }



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
                            jsonData = IE.Where(e =>  (e.Code.ToString().Contains(gp.searchString))
                                    || (e.Name.ToString().Contains(gp.searchString))
                                    || (e.Id.ToString().Contains(gp.searchString))
                                    ).Select(a => new Object[] { a.Code, a.Name, a.Id }).ToList();
                            //jsonData = IE.Select(a => new { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Code, a.HeaderCol, a.ActualAmount, a.QualifyAmount, a.DeductibleAmount, a.FinalAmount, a.Id }).ToList();
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
                            orderfuc = (c => gp.sidx == "Code" ? c.Code.ToString() :
                                             gp.sidx == "Name" ? c.Name.ToString() :
                                             "");
                            // jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, Convert.ToString(a.Code), a.Name, a.FromPeriod != null ? Convert.ToString(a.FromPeriod) : "", a.Toperiod != null ? Convert.ToString(a.Toperiod) : "", a.title != null ? Convert.ToString(a.title) : "", Convert.ToString(a.Islock) }).ToList();
                        }
                        if (gp.sord == "asc")
                        {
                            IE = IE.OrderBy(orderfuc);
                            jsonData = IE.Select(a => new Object[] { a.Code, a.HeaderCol, a.ActualAmount, a.QualifyAmount, a.DeductibleAmount, a.FinalAmount, a.Id }).ToList();
                        }
                        else if (gp.sord == "desc")
                        {
                            IE = IE.OrderByDescending(orderfuc);
                            jsonData = IE.Select(a => new Object[] { a.Code, a.HeaderCol, a.ActualAmount, a.QualifyAmount, a.DeductibleAmount, a.FinalAmount, a.Id }).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Code, a.HeaderCol, a.ActualAmount, a.QualifyAmount, a.DeductibleAmount, a.FinalAmount, a.Id }).ToList();
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
                    throw ex;
                }
            }
        }

        
        public ActionResult GetEmpPdf(string forwardata)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                var EmpCode = forwardata;
                //var EmpCode = db.Employee.Where(e => e.Id == Empid).FirstOrDefault().EmpCode;

                string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(
                                  System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\App_Data\\Form16\\"; ///empcode.pdf
                string localPath = new Uri(requiredPath).LocalPath;

                String[] allfiles = System.IO.Directory.GetFiles(localPath, "*" + EmpCode + ".pdf*", System.IO.SearchOption.AllDirectories);
                if (allfiles.Length == 0 )
                {
                     return JavaScript("alert('No data Found..!!')");
                }
                string path = allfiles[0];

                FileInfo file = new FileInfo(path);
                bool exists = file.Exists;
                string extension = Path.GetExtension(file.Name);

                if (exists)
                {
                    if (extension == ".pdf")
                    {
                        return File(file.FullName, "application/pdf", file.Name + " ");
                    }
                }
                return null;
            }
        }

    }
}