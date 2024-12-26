using P2BUltimate.App_Start;
using System.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Payroll;
using System.Transactions;
using P2b.Global;
using P2BUltimate.Security;
using P2BUltimate.Models;


namespace P2BUltimate.Controllers.Payroll.MainController
{
    public class FunctionalAllowancePolicyController : Controller
    {
        //
        // GET: /FunctionalAllowancePolicy/
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/FunctionalAllowancePolicy/Index.cshtml");
        }
        public ActionResult SalaryheadCode(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                var query = db.SalaryHead.Include(e => e.Frequency)
                    .Where(e => e.Frequency.LookupVal.ToUpper() == "DAILY"
                        || e.Frequency.LookupVal.ToUpper() == "HOURLY").ToList();
                var selected = (Object)null;
                if (!string.IsNullOrEmpty(data2))
                {
                    selected = Convert.ToInt32(data2);
                }
                SelectList s = new SelectList(query, "Id", "Code", selected);
                return Json(s, JsonRequestBehavior.AllowGet);

            }
        }

        public ActionResult PaymonthConcept(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                var Lookup = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "550").SingleOrDefault();
                var qurey = Lookup.LookupValues.ToList();
                var selected = (Object)null;
                if (data2 != "" && data != "0" && data2 != "0")
                {
                    selected = Convert.ToInt32(data2);
                }

                SelectList s = new SelectList(qurey, "Id", "LookupVal", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public ActionResult Create(FunctionalAllowancePolicy c, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string SalaryHeadlist = form["SalaryHeadlist"] == "0" ? "" : form["SalaryHeadlist"];
                    string PaymonthConceptlist = form["PaymonthConceptlist"] == "0" ? "" : form["PaymonthConceptlist"];
                    int company_Id = 0;
                    company_Id = Convert.ToInt32(Session["CompId"]);
                    var companypayroll = new CompanyPayroll();
                    companypayroll = db.CompanyPayroll.Where(e => e.Company.Id == company_Id).SingleOrDefault();

                    if (SalaryHeadlist != null && SalaryHeadlist != "")
                    {
                        var val = db.SalaryHead.Find(int.Parse(SalaryHeadlist));
                        c.SalaryHead = val;
                    }

                    var AutoSanctionAppl = form["IsAutoSanctionAppl"] == "0" ? "" : form["IsAutoSanctionAppl"];
                    c.IsAutoSanctionAppl = Convert.ToBoolean(AutoSanctionAppl);
                    var AutoRecommendAppl = form["IsAutoRecommendAppl"] == "0" ? "" : form["IsAutoRecommendAppl"];
                    c.IsAutoRecommendAppl = Convert.ToBoolean(AutoRecommendAppl);
                    var AutoHRApprovalAppl = form["IsAutoHRApprovalAppl"] == "0" ? "" : form["IsAutoHRApprovalAppl"];
                    c.IsAutoHRApprovalAppl = Convert.ToBoolean(AutoHRApprovalAppl);

                    var AttendanceMusterAppl = form["IsAttendanceMusterAppl"] == "0" ? "" : form["IsAttendanceMusterAppl"];
                    c.IsAttendanceMusterAppl = Convert.ToBoolean(AttendanceMusterAppl);
                    var ContinousAppl = form["IsContinousAppl"] == "0" ? "" : form["IsContinousAppl"];
                    c.IsContinousAppl = Convert.ToBoolean(ContinousAppl);
                    var RequisitionPeriodAppl = form["IsRequisitionPeriodAppl"] == "0" ? "" : form["IsRequisitionPeriodAppl"];
                    c.IsRequisitionPeriodAppl = Convert.ToBoolean(RequisitionPeriodAppl);

                    var DubliSalaryHead = db.FunctionalAllowancePolicy
                        .Include(e => e.SalaryHead)
                        .Where(e => e.SalaryHead.Id == c.SalaryHead.Id).SingleOrDefault();

                    if (DubliSalaryHead != null)
                    {
                        Msg.Add(" Record Already Exist..!");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    if (PaymonthConceptlist != null && PaymonthConceptlist != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(PaymonthConceptlist));
                        c.PaymonthConcept = val;
                    }

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {

                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            FunctionalAllowancePolicy FunctionalAllowancePolicy = new FunctionalAllowancePolicy()
                            {
                                MinDays = c.MinDays,
                                MaxDays = c.MaxDays,
                                SalaryHead = c.SalaryHead,
                                PaymonthConcept = c.PaymonthConcept,
                                IsAutoSanctionAppl = c.IsAutoSanctionAppl,
                                IsAutoRecommendAppl = c.IsAutoRecommendAppl,
                                IsAutoHRApprovalAppl = c.IsAutoHRApprovalAppl,
                                AutoSanctionGracePeriod = c.AutoSanctionGracePeriod,
                                AutoRecommendGracePeriod = c.AutoRecommendGracePeriod,
                                AutoHRApprovalGracePeriod = c.AutoHRApprovalGracePeriod,

                                IsRequisitionPeriodAppl = c.IsRequisitionPeriodAppl,
                                IsContinousAppl = c.IsContinousAppl,
                                IsAttendanceMusterAppl = c.IsAttendanceMusterAppl,
                                MonthFromDay = c.MonthFromDay,
                                MonthEndDay = c.MonthEndDay,

                                DBTrack = c.DBTrack

                            };
                            db.FunctionalAllowancePolicy.Add(FunctionalAllowancePolicy);
                            db.SaveChanges();
                            if (companypayroll != null)
                            {
                                var FunctionalAllowancePolicy_list = new List<FunctionalAllowancePolicy>();
                                FunctionalAllowancePolicy_list.Add(FunctionalAllowancePolicy);
                                companypayroll.FunctionalAllowancePolicy = FunctionalAllowancePolicy_list;
                                db.CompanyPayroll.Attach(companypayroll);
                                db.Entry(companypayroll).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(companypayroll).State = System.Data.Entity.EntityState.Detached;
                            }
                            ts.Complete();
                            Msg.Add(" Data Saved successfully ");
                        }
                    }

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
                        LineNo = Convert.ToString(new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                        LogTime = DateTime.Now
                    };
                    Logfile.CreateLogFile(Err);
                    Msg.Add(ex.Message);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        public class P2BGridData
        {
            public int Id { get; set; }
            public string MinDays { get; set; }
            public string MaxDays { get; set; }
            public string SalaryHead { get; set; }
            public string PaymonthConcept { get; set; }

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

                    IEnumerable<P2BGridData> FunctionalAllowancePolicyList = null;
                    List<P2BGridData> model = new List<P2BGridData>();
                    P2BGridData view = null;

                    var FunctionalAllowancePolicy = db.FunctionalAllowancePolicy
                        .Include(e => e.SalaryHead)
                        .Include(e => e.PaymonthConcept)
                        .ToList();

                    foreach (var s in FunctionalAllowancePolicy)
                    {
                        view = new P2BGridData()
                        {
                            Id = s.Id,
                            MinDays = s.MinDays.ToString(),
                            MaxDays = s.MaxDays.ToString(),
                            SalaryHead = s.SalaryHead.Code,
                            PaymonthConcept = s.PaymonthConcept.LookupVal.ToString()
                        };
                        model.Add(view);

                    }
                    FunctionalAllowancePolicyList = model;

                    IEnumerable<P2BGridData> IE;
                    if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                    {
                        IE = FunctionalAllowancePolicyList;
                        if (gp.searchOper.Equals("eq"))
                        {
                            jsonData = IE.Where(e => (e.Id.ToString().Contains(gp.searchString))
                                || (e.MinDays.ToUpper().Contains(gp.searchString.ToUpper()))
                                || (e.MaxDays.ToUpper().Contains(gp.searchString.ToUpper()))
                                || (e.SalaryHead.ToString().Contains(gp.searchString))
                                || (e.PaymonthConcept.ToString().Contains(gp.searchString))
                                ).Select(a => new Object[] { a.MinDays, a.MaxDays, a.SalaryHead, a.PaymonthConcept, a.Id }).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.MinDays, a.MaxDays, a.SalaryHead, a.PaymonthConcept, a.Id }).ToList();
                        }
                        totalRecords = IE.Count();
                    }
                    else
                    {
                        IE = FunctionalAllowancePolicyList;
                        Func<P2BGridData, dynamic> orderfuc;
                        if (gp.sidx == "Id")
                        {
                            orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                        }
                        else
                        {
                            orderfuc = (c => gp.sidx == "MinDays" ? c.MinDays.ToString() :
                                             gp.sidx == "MaxDays" ? c.MaxDays.ToString() :
                                             gp.sidx == "SalaryHead" ? c.SalaryHead.ToString() :
                                             gp.sidx == "PaymonthConcept" ? c.PaymonthConcept.ToString() : "");
                        }
                        if (gp.sord == "asc")
                        {
                            IE = IE.OrderBy(orderfuc);
                            jsonData = IE.Select(a => new Object[] { a.MinDays, a.MaxDays, a.SalaryHead, a.PaymonthConcept, a.Id }).ToList();
                        }
                        else if (gp.sord == "desc")
                        {
                            IE = IE.OrderByDescending(orderfuc);
                            jsonData = IE.Select(a => new Object[] { a.MinDays, a.MaxDays, a.SalaryHead, a.PaymonthConcept, a.Id }).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.MinDays, a.MaxDays, a.SalaryHead, a.PaymonthConcept, a.Id }).ToList();
                        }
                        totalRecords = FunctionalAllowancePolicyList.Count();
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

        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.FunctionalAllowancePolicy
                    .Include(e => e.SalaryHead)
                    .Include(e => e.PaymonthConcept)
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        MinDays = e.MinDays,
                        MaxDays = e.MaxDays,
                        SalaryHead_Id = e.SalaryHead.Id == null ? 0 : e.SalaryHead.Id,
                        PaymonthConcept_Id = e.PaymonthConcept.Id == null ? 0 : e.PaymonthConcept.Id,
                        IsAutoSanctionAppl = e.IsAutoSanctionAppl,
                        IsAutoRecommendAppl = e.IsAutoRecommendAppl,
                        IsAutoHRApprovalAppl = e.IsAutoHRApprovalAppl,
                        AutoSanctionGracePeriod = e.AutoSanctionGracePeriod,
                        AutoRecommendGracePeriod = e.AutoRecommendGracePeriod,
                        AutoHRApprovalGracePeriod = e.AutoHRApprovalGracePeriod,
                        IsAttendanceMusterAppl = e.IsAttendanceMusterAppl,
                        IsContinousAppl = e.IsContinousAppl,
                        IsRequisitionPeriodAppl = e.IsRequisitionPeriodAppl,
                        MonthFromDay = e.MonthFromDay,
                        MonthEndDay = e.MonthEndDay,

                    }).ToList();

                return Json(new Object[] { Q, "", "", "", JsonRequestBehavior.AllowGet });
            }
        }

        [HttpPost]
        public ActionResult EditSave(FunctionalAllowancePolicy c, int data, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                string SalaryHeadlist = form["SalaryHeadlist"] == "0" ? "" : form["SalaryHeadlist"];
                string PaymonthConceptlist = form["PaymonthConceptlist"] == "0" ? "" : form["PaymonthConceptlist"];
               
                if (SalaryHeadlist != null && SalaryHeadlist != "")
                {
                    var val = db.SalaryHead.Find(int.Parse(SalaryHeadlist));
                    c.SalaryHead = val;
                }

                var AutoSanctionAppl = form["IsAutoSanctionAppl"] == "0" ? "" : form["IsAutoSanctionAppl"];
                c.IsAutoSanctionAppl = Convert.ToBoolean(AutoSanctionAppl);
                var AutoRecommendAppl = form["IsAutoRecommendAppl"] == "0" ? "" : form["IsAutoRecommendAppl"];
                c.IsAutoRecommendAppl = Convert.ToBoolean(AutoRecommendAppl);
                var AutoHRApprovalAppl = form["IsAutoHRApprovalAppl"] == "0" ? "" : form["IsAutoHRApprovalAppl"];
                c.IsAutoHRApprovalAppl = Convert.ToBoolean(AutoHRApprovalAppl);

                var AttendanceMusterAppl = form["IsAttendanceMusterAppl"] == "0" ? "" : form["IsAttendanceMusterAppl"];
                c.IsAttendanceMusterAppl = Convert.ToBoolean(AttendanceMusterAppl);
                var ContinousAppl = form["IsContinousAppl"] == "0" ? "" : form["IsContinousAppl"];
                c.IsContinousAppl = Convert.ToBoolean(ContinousAppl);
                var RequisitionPeriodAppl = form["IsRequisitionPeriodAppl"] == "0" ? "" : form["IsRequisitionPeriodAppl"];
                c.IsRequisitionPeriodAppl = Convert.ToBoolean(RequisitionPeriodAppl);

                var DubliSalaryHead = db.FunctionalAllowancePolicy
                    .Include(e => e.SalaryHead)
                    .Where(e => e.SalaryHead.Id == c.SalaryHead.Id).SingleOrDefault();

              
                if (PaymonthConceptlist != null && PaymonthConceptlist != "")
                {
                    var val = db.LookupValue.Find(int.Parse(PaymonthConceptlist));
                    c.PaymonthConcept = val;
                }

                try
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        var db_data = db.FunctionalAllowancePolicy.Include(e => e.SalaryHead).Include(e => e.PaymonthConcept).Where(e => e.Id == data).FirstOrDefault();

                        c.DBTrack = new DBTrack
                        {
                            CreatedBy = db_data.DBTrack.CreatedBy == null ? null : db_data.DBTrack.CreatedBy,
                            CreatedOn = db_data.DBTrack.CreatedOn == null ? null : db_data.DBTrack.CreatedOn,
                            Action = "M",
                            ModifiedBy = SessionManager.UserName,
                            ModifiedOn = DateTime.Now
                        };
                        db_data.Id = data;
                        db_data.MinDays = c.MinDays;
                        db_data.MaxDays = c.MaxDays;
                        db_data.PaymonthConcept = c.PaymonthConcept;
                        db_data.SalaryHead = c.SalaryHead;
                        db_data.IsAutoSanctionAppl = c.IsAutoSanctionAppl;
                        db_data.IsAutoRecommendAppl = c.IsAutoRecommendAppl;
                        db_data.IsAutoHRApprovalAppl = c.IsAutoHRApprovalAppl;
                        db_data.AutoSanctionGracePeriod = c.AutoSanctionGracePeriod;
                        db_data.AutoRecommendGracePeriod = c.AutoRecommendGracePeriod;
                        db_data.AutoHRApprovalGracePeriod = c.AutoHRApprovalGracePeriod;

                        db_data.IsRequisitionPeriodAppl = c.IsRequisitionPeriodAppl;
                        db_data.IsContinousAppl = c.IsContinousAppl;
                        db_data.IsAttendanceMusterAppl = c.IsAttendanceMusterAppl;
                        db_data.MonthFromDay = c.MonthFromDay;
                        db_data.MonthEndDay = c.MonthEndDay;
                        db_data.DBTrack = c.DBTrack;
                        db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        ts.Complete();
                    }
                    Msg.Add("  Record Updated");
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

        [HttpPost]
        public ActionResult Delete(int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    FunctionalAllowancePolicy FunctionalAllowancePolicy = db.FunctionalAllowancePolicy
                                                      .Include(e => e.SalaryHead)
                                                      .Include(e => e.PaymonthConcept)
                                                      .Where(e => e.Id == data).SingleOrDefault();
                     
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        DBTrack dbT = new DBTrack
                        {
                            Action = "D",
                            ModifiedBy = SessionManager.UserName,
                            ModifiedOn = DateTime.Now,
                            CreatedBy = FunctionalAllowancePolicy.DBTrack.CreatedBy != null ? FunctionalAllowancePolicy.DBTrack.CreatedBy : null,
                            CreatedOn = FunctionalAllowancePolicy.DBTrack.CreatedOn != null ? FunctionalAllowancePolicy.DBTrack.CreatedOn : null,
                            IsModified = FunctionalAllowancePolicy.DBTrack.IsModified == true ? false : false

                        };
                        db.Entry(FunctionalAllowancePolicy).State = System.Data.Entity.EntityState.Deleted;
                        db.SaveChanges();
                        ts.Complete();
                        Msg.Add("  Data removed.  ");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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


    }
}