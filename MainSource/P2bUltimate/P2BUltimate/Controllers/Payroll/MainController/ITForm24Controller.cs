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
    public class ITForm24Controller : Controller
    {
        //private DataBaseContext db = new DataBaseContext();
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/ITForm24/Index.cshtml");
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

                var query = db.ITProjection.Include(e => e.FinancialYear).ToList();
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
                    int Calendar_Id = Convert.ToInt32(FinancialYear);
                    var FromDate = db.Calendar.Where(e => e.Id == Calendar_Id)
                                  .Select(e => e.FromDate)
                                 .SingleOrDefault();
                    var ToDate = db.Calendar.Where(e => e.Id == Calendar_Id)
                                  .Select(e => e.ToDate)
                                 .SingleOrDefault();
                    var OFinancialYear = db.Calendar.Find(int.Parse(FinancialYear));

                    //DateTime FromPeriod = Convert.ToDateTime(FromDate);
                    //IT.FromPeriod = FromPeriod;
                    //DateTime ToPeriod = Convert.ToDateTime(ToDate);
                    //IT.ToPeriod = ToPeriod;
                    //if (FinancialYear != null && FinancialYear != "")
                    //{
                    //    var value = db.Calendar.Find(int.Parse(FinancialYear));
                    //    IT.FinancialYear = value;

                    //}

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
                Msg.Add("Kindly process IT Challan before Form24.");
                return Json(new { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

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
                string PayMonth = "";

                if (gp.filter != null)
                    PayMonth = gp.filter;
                if (PayMonth != null && PayMonth != "")
                {

                    var financialyear = db.Calendar.Find(int.Parse(PayMonth));

                    var BindEmpList = db.EmployeePayroll.Include(e => e.Employee.EmpName).Include(e => e.ITProjection)
                        .ToList();

                    foreach (var z in BindEmpList)
                    {
                        if (z.ITProjection != null && z.ITProjection.Count() > 0)
                        {
                            var all = z.ITProjection.GroupBy(e => e.ProjectionDate).Select(e => e.FirstOrDefault()).SingleOrDefault();
                            if (all.FinancialYear.Id == financialyear.Id)
                            {
                                view = new P2BGridData()
                                {
                                    Id = all.Id,
                                    Code = z.Employee.EmpCode,
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
                        jsonData = IE.Where(e => (e.Id.ToString().Contains(gp.searchString))
                                || (e.Code.ToString().Contains(gp.searchString))
                                || (e.Name.ToString().Contains(gp.searchString))
                                ).Select(a => new Object[] { a.Id, a.Code, a.Name }).ToList();
                        //jsonData = IE.Select(a => new  { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, Convert.ToString(a.Code), a.Name, a.ProjectionDate, Convert.ToString(a.Islock) }).ToList();
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
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), a.Name, a.ProjectionDate, Convert.ToString(a.Islock) }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), a.Name, a.ProjectionDate, Convert.ToString(a.Islock) }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, Convert.ToString(a.Code), a.Name, a.ProjectionDate, Convert.ToString(a.Islock) }).ToList();
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
                    Employee OEmployee = null;
                    EmployeePayroll OEmployeePayroll = null;

                    foreach (var i in ids)
                    {
                        OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct)
                                    .Where(r => r.Id == i).SingleOrDefault();

                        OEmployeePayroll = db.EmployeePayroll.Include(e => e.ITProjection).Where(e => e.Employee.Id == i).SingleOrDefault();


                        using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                           new System.TimeSpan(0, 30, 0)))
                        {

                            

                                var ITProject = OEmployeePayroll.ITProjection.Where(e => e.FinancialYear == Ofinanialyear).SingleOrDefault();

                                if (ITProject != null)
                                {
                                    ITProject.IsLocked = true;
                                    db.ITProjection.Attach(ITProject);
                                    db.Entry(ITProject).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    TempData["RowVersion"] = ITProject.RowVersion;
                                    db.Entry(ITProject).State = System.Data.Entity.EntityState.Detached;
                                }
                            
                            ts.Complete();

                        }
                        return Json(new { success = true, responseText = "ITProjection Is Locked." }, JsonRequestBehavior.AllowGet);
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
                                    Code = z.Employee.EmpCode,
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
                            jsonData = IE.Where(e => (e.Id.ToString().Contains(gp.searchString))
                                    || (e.Code.ToString().Contains(gp.searchString))
                                    || (e.Name.ToString().Contains(gp.searchString))
                                    ).Select(a => new Object[] { a.Id, a.Code, a.Name }).ToList();
                            //jsonData = IE.Select(a => new { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Code, a.title, a.Section, a.ChapterName, a.ProjectedAmount, a.ActualAmount, a.ActualQualifyingAmount, a.ProjectedQualifyingAmount, a.Narration }).ToList();
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
                            jsonData = IE.Select(a => new Object[] { a.Id, a.Code, a.title, a.Section, a.ChapterName, a.ProjectedAmount, a.ActualAmount, a.ActualQualifyingAmount, a.ProjectedQualifyingAmount, a.Narration }).ToList();
                        }
                        else if (gp.sord == "desc")
                        {
                            IE = IE.OrderByDescending(orderfuc);
                            jsonData = IE.Select(a => new Object[] { a.Id, a.Code, a.title, a.Section, a.ChapterName, a.ProjectedAmount, a.ActualAmount, a.ActualQualifyingAmount, a.ProjectedQualifyingAmount, a.Narration }).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Code, a.title, a.Section, a.ChapterName, a.ProjectedAmount, a.ActualAmount, a.ActualQualifyingAmount, a.ProjectedQualifyingAmount, a.Narration }).ToList();
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
    }
}