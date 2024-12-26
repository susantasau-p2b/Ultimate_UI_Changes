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

namespace P2BUltimate.Controllers.Payroll.MainController
{
    [AuthoriseManger]
    public class ITForm16PartBController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();
        public ActionResult Index()
        { 
            return View("~/Views/Payroll/MainViews/ITForm16PartB/Index.cshtml");
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

        //public ActionResult LoadEmpByDefault(string data, string data2)
        //{

        //    if (!String.IsNullOrEmpty(Session["CompId"].ToString()))
        //    {
        //        int CompId = int.Parse(Session["CompId"].ToString());
        //        var query = db.CompanyPayroll.Where(e => e.Company.Id == CompId).Include(e => e.EmployeePayroll).Select(e => e.EmployeePayroll).SingleOrDefault();
        //        List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
        //        foreach (var ca in query.Select(e => e.Employee))
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

        public ActionResult Create(ITProjection IT, FormCollection form) //Create submit
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
                    int Calendar_Id = Convert.ToInt32(FinancialYear);
                    var FromDate = db.Calendar.Where(e => e.Id == Calendar_Id)
                                  .Select(e => e.FromDate)
                                 .SingleOrDefault();
                    var ToDate = db.Calendar.Where(e => e.Id == Calendar_Id)
                                  .Select(e => e.ToDate)
                                 .SingleOrDefault();
                    var OFinancialYear = db.Calendar.Find(int.Parse(FinancialYear));

                    DateTime FromPeriod = Convert.ToDateTime(FromDate);
                    IT.FromPeriod = FromPeriod;
                    DateTime ToPeriod = Convert.ToDateTime(ToDate);
                    IT.ToPeriod = ToPeriod;
                    if (FinancialYear != null && FinancialYear != "")
                    {
                        var value = db.Calendar.Find(int.Parse(FinancialYear));
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

                    if (db.ITForm16Data.Any(e => e.FinancialYear.Id == OFinancialYear.Id && e.IsLocked == true))
                    {
                        Msg.Add("You can't reprocess because record has been locked.");
                        return Json(new { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    Employee OEmployee = null;
                    EmployeePayroll OEmployeePayroll = null;
                    CompanyPayroll OCompanyPayroll = null;




                    //int PayScaleAgrId = int.Parse(PayScaleAgr);
                    //var OPayScalAgreement = db.PayScaleAgreement.Where(e => e.Id == PayScaleAgrId).SingleOrDefault();

                    foreach (var i in ids)
                    {
                        OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.GeoStruct.Company).Include(e => e.FuncStruct).Include(e => e.PayStruct)
                                    .Where(r => r.Id == i).SingleOrDefault();

                        OEmployeePayroll = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.EmpSalStruct).Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails))
                            .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead)))
                            .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.Type)))
                            .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.SalHeadOperationType)))
                            .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.ProcessType)))
                              .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.Frequency)))
                              .Include(e => e.Employee.EmpOffInfo)
                               .Include(e => e.Employee.EmpOffInfo.NationalityID)
                               .Include(e => e.Employee.Gender)
                              .Include(e => e.Employee.ServiceBookDates)
                               .Include(e => e.Employee.EmpName)
                               .Include(e => e.ITProjection)
                               .Include(e => e.ITProjection.Select(r => r.FinancialYear))
                               .Include(e => e.ITInvestmentPayment)
                               .Include(e => e.ITInvestmentPayment.Select(r => r.FinancialYear))
                               .Include(e => e.ITInvestmentPayment.Select(r => r.ITSection))
                               .Include(e => e.ITInvestmentPayment.Select(r => r.ITSection.ITSectionList))
                               .Include(e => e.ITInvestmentPayment.Select(r => r.ITSection.ITSectionListType))
                               .Include(e => e.ITInvestmentPayment.Select(r => r.ITSection.ITSectionList))//added 17042017 by prashant
                               .Include(e => e.ITInvestmentPayment.Select(r => r.ITSubInvestmentPayment))
                               .Include(e => e.ITInvestmentPayment.Select(r => r.LoanAdvanceHead))
                               .Include(e => e.ITInvestmentPayment.Select(r => r.LoanAdvanceHead.ITLoan))
                               .Include(e => e.ITInvestmentPayment.Select(r => r.LoanAdvanceHead.ITSection))
                               .Include(e => e.ITInvestmentPayment.Select(r => r.LoanAdvanceHead.LoanAdvancePolicy))
                                .Include(e => e.ITInvestmentPayment.Select(r => r.ITInvestment))
                            .Where(e => e.Employee.Id == i).SingleOrDefault();

                        OCompanyPayroll = db.CompanyPayroll.Where(e => e.Company.Id == OEmployee.GeoStruct.Company.Id).SingleOrDefault();

                        using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                           new System.TimeSpan(0, 30, 0)))
                        {

                            // IncomeTaxCalc.ITForm16PartB(OEmployeePayroll, CompId, IT.FinancialYear, FromPeriod, ToPeriod, processdate,1);
                            ts.Complete();

                        }
                    }
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
                //return Json(new Object[] { "", "", "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
                //  List<string> Msgs = new List<string>();
                Msg.Add("Data Saved successfully");
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
            public string Code { get; set; }
            public string Name { get; set; }
            public string EmpPAN { get; set; }
            public string HeaderCol { get; set; }
            public string ActualAmountCol2 { get; set; }
            public string QualifyAmountCol3 { get; set; }
            public string DeductibleAmountCol4 { get; set; }
            public string FinalAmountCol5 { get; set; }
            public string FinancialYear { get; set; } 
            public DateTime? PeriodFrom { get; set; }
            public DateTime? PeriodTo { get; set; } 
            public string title { get; set; }
            public bool Islock { get; set; }
            public string ReportDate { get; set; }
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

                    IEnumerable<P2BGridData> ITPartBList = null;
                    List<P2BGridData> model = new List<P2BGridData>();
                    P2BGridData view = null;
                    string PayMonth = "";

                    if (gp.filter != null)
                        PayMonth = gp.filter;
                    if (PayMonth != null && PayMonth != "")
                    {

                        var financialyear = db.Calendar.Find(int.Parse(PayMonth));

                        var BindEmpList = db.EmployeePayroll.Include(e => e.Employee.EmpName).Include(e => e.ITForm16Data)
                            .Include(e => e.ITForm16Data.Select(r => r.FinancialYear)).Include(e => e.ITForm16Data.Select(r => r.ITForm16DataDetails))
                            .ToList();

                        foreach (var z in BindEmpList)
                        {
                            if (z.ITForm16Data != null && z.ITForm16Data.Count() > 0)
                            {
                                var all = z.ITForm16Data.GroupBy(e => e.ReportDate).Select(e => e.FirstOrDefault()).SingleOrDefault();
                                if (all.FinancialYear.Id == financialyear.Id)
                                {
                                    view = new P2BGridData()
                                    {
                                        Id = all.Id,
                                        Code = z.Employee.EmpCode,
                                        Name = z.Employee.EmpName.FullNameFML,
                                        PeriodFrom = all.PeriodFrom,
                                        PeriodTo = all.PeriodTo,
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
                    ITPartBList = model;

                    IEnumerable<P2BGridData> IE;
                    if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                    {
                        IE = ITPartBList;
                        if (gp.searchOper.Equals("eq"))
                        {
                            jsonData = IE.Where(e => (e.Id.ToString().Contains(gp.searchString))
                                    || (e.Code.ToString().Contains(gp.searchString))
                                    || (e.Name.ToString().Contains(gp.searchString))
                                    ).Select(a => new Object[] { a.Id, a.Code, a.Name }).ToList();
                            //jsonData = IE.Select(a => new  { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, Convert.ToString(a.Code), a.Name, a.ReportDate, Convert.ToString(a.Islock) }).ToList();
                        }
                        totalRecords = IE.Count();
                    }
                    else
                    {
                        IE = ITPartBList;
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
                        }
                        if (gp.sord == "asc")
                        {
                            IE = IE.OrderBy(orderfuc);
                            jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), a.Name, a.ReportDate, Convert.ToString(a.Islock) }).ToList();
                        }
                        else if (gp.sord == "desc")
                        {
                            IE = IE.OrderByDescending(orderfuc);
                            jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), a.Name, a.ReportDate, Convert.ToString(a.Islock) }).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, Convert.ToString(a.Code), a.Name, a.ReportDate, Convert.ToString(a.Islock) }).ToList();
                        }
                        totalRecords = ITPartBList.Count();
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
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
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


                    foreach (var i in ids)
                    {
                        //OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct)
                        //            .Where(r => r.Id == i).SingleOrDefault();

                        //OEmployeePayroll = db.EmployeePayroll.Include(e => e.ITForm16Data).Where(e => e.Employee.Id == i).SingleOrDefault();


                        using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                           new System.TimeSpan(0, 30, 0)))
                        {

                            using (DataBaseContext db2 = new DataBaseContext())
                            {

                                var ITProject = db.ITForm16Data.Where(e => e.Id == i).SingleOrDefault();

                                if (ITProject != null)
                                {
                                    ITProject.IsLocked = true;
                                    db.ITForm16Data.Attach(ITProject);
                                    db.Entry(ITProject).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    TempData["RowVersion"] = ITProject.RowVersion;
                                    db.Entry(ITProject).State = System.Data.Entity.EntityState.Detached;
                                }
                            }
                            ts.Complete();

                        }
                        return Json(new { success = true, responseText = "ITForm16Data Is Locked." }, JsonRequestBehavior.AllowGet);
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
                return View();
            }
        }


        //public ActionResult Edit(int data)
        //{

        //    var Q = db.ITForm16Data.Include(e => e.FinancialYear)
        //           .Where(e => e.Id == data).Select(e => new
        //           {

        //               ActualAmount = e.ActualAmount,
        //               ActualQualifyingAmount = e.ActualQualifyingAmount,
        //               ChapterName = e.ChapterName,
        //               FinancialYear_Id = e.FinancialYear.Id,
        //               FinancialYear_FullDetails = e.FinancialYear.FullDetails,
        //               Form16Header = e.Form16Header,
        //               Form24Header = e.Form24Header,
        //               FromPeriod = e.FromPeriod,
        //               IsLocked = e.IsLocked,
        //               Narration = e.Narration,
        //               PickupId = e.PickupId,
        //               ProjectedAmount = e.ProjectedAmount,
        //               ProjectedQualifyingAmount = e.ProjectedQualifyingAmount,
        //               ProjectionDate = e.ProjectionDate,
        //               QualifiedAmount = e.QualifiedAmount,
        //               SalayHead = e.SalayHead,
        //               Section = e.Section,
        //               SectionType = e.SectionType,
        //               SubChapter = e.SubChapter,
        //               TDSComponents = e.TDSComponents,
        //               Tiltle = e.Tiltle,
        //               ToPeriod = e.ToPeriod


        //           }).ToList();

        //    var Corp = db.ITForm16Data.Find(data);
        //    //TempData["RowVersion"] = Corp.RowVersion;
        //    // var Auth = Corp.DBTrack.IsModified;
        //    return Json(new Object[] { Q, "", "", "", JsonRequestBehavior.AllowGet });


        //}


        public ActionResult ViewPartBData(P2BGrid_Parameters gp)//, string data
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

                    IEnumerable<P2BGridData> ITForm16DataList = null;
                    List<P2BGridData> model = new List<P2BGridData>();
                    P2BGridData view = null;


                    var BindEmpList = db.EmployeePayroll.Include(e => e.Employee.EmpName).Include(e => e.ITForm16Data)
                                        .Include(e => e.ITForm16Data.Select(t => t.FinancialYear))
                                       .Include(e => e.ITForm16Data.Select(r => r.ITForm16DataDetails))
                                      .Where(e => e.ITForm16Data.Any(t => t.Id == Id))
                                      .ToList();

                    foreach (var z in BindEmpList)
                    {
                        if (z.ITForm16Data != null)
                        {

                            foreach (var Sal in z.ITForm16Data)
                            {

                                var Det = Sal.ITForm16DataDetails;
                                foreach (var d in Det)
                                {
                                    view = new P2BGridData()
                                    {
                                        Id = Sal.Id,
                                        Code = z.Employee.EmpCode,
                                        Name = z.Employee.EmpName.FullNameFML,
                                        HeaderCol = d.HeaderCol1,
                                        PeriodFrom = Sal.PeriodFrom,
                                        PeriodTo = Sal.PeriodTo,
                                        ActualAmountCol2 = d.ActualAmountCol2,
                                        QualifyAmountCol3 = d.QualifyAmountCol3,
                                        DeductibleAmountCol4 = d.DeductibleAmountCol4,
                                        FinalAmountCol5 = d.FinalAmountCol5,
                                        Islock = Sal.IsLocked,
                                        ReportDate = Sal.ReportDate.Value.ToString("dd/MM/yyyy")
                                    };
                                    model.Add(view);
                                }

                            }
                        }

                    }



                    ITForm16DataList = model;

                    IEnumerable<P2BGridData> IE;
                    if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                    {
                        IE = ITForm16DataList;
                        if (gp.searchOper.Equals("eq"))
                        {
                            jsonData = IE.Where(e => (e.Id.ToString().Contains(gp.searchString))
                                    || (e.Code.ToString().Contains(gp.searchString))
                                    || (e.Name.ToString().Contains(gp.searchString))
                                    ).Select(a => new Object[] { a.Id, a.Code, a.Name }).ToList();
                            //jsonData = IE.Select(a => new { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Code, a.HeaderCol, a.ActualAmountCol2, a.QualifyAmountCol3, a.DeductibleAmountCol4, a.FinalAmountCol5 }).ToList();
                        }
                        totalRecords = IE.Count();
                    }
                    else
                    {
                        IE = ITForm16DataList;
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
                            jsonData = IE.Select(a => new Object[] { a.Id, a.Code, a.HeaderCol, a.ActualAmountCol2, a.QualifyAmountCol3, a.DeductibleAmountCol4, a.FinalAmountCol5 }).ToList();
                        }
                        else if (gp.sord == "desc")
                        {
                            IE = IE.OrderByDescending(orderfuc);
                            jsonData = IE.Select(a => new Object[] { a.Id, a.Code, a.HeaderCol, a.ActualAmountCol2, a.QualifyAmountCol3, a.DeductibleAmountCol4, a.FinalAmountCol5 }).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Code, a.HeaderCol, a.ActualAmountCol2, a.QualifyAmountCol3, a.DeductibleAmountCol4, a.FinalAmountCol5 }).ToList();
                        }
                        totalRecords = ITForm16DataList.Count();
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