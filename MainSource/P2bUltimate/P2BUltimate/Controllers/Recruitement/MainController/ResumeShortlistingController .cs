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
using System.Reflection;
using P2BUltimate.Security;
using Recruitment;
namespace P2BUltimate.Controllers.Recruitement.MainController
{
    public class ResumeShortlistingController : Controller
    {
        //
        // GET: /ResumeShortlisting/
        public ActionResult Index()
        {
            return View("~/Views/Recruitement/MainView/ResumeShortlisting/Index.cshtml");
        }
        [HttpPost]
        public ActionResult GetRecruitExpensesLKDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.RecruitInitiator.Include(a => a.RecruitBatchInitiator).ToList();
                IEnumerable<RecruitInitiator> all;
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.RecruitInitiator.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }

                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = "Advertise Reference No:" + ca.AdvertiseReferenceNo }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);


            }
        }

        [HttpPost]
        public ActionResult GetShortListingLKDetails(List<int> SkipIds, string forwardata)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<int> forwardataids = null;
                RecruitInitiator recruitinitiator = null;
                List<RecruitBatchInitiator> fall = new List<RecruitBatchInitiator>();
                List<ShortlistingCriteria> fall1 = new List<ShortlistingCriteria>();
                // var fall1="";
                if (forwardata != null)
                {
                    forwardataids = Utility.StringIdsToListIds(forwardata);
                    foreach (var item in forwardataids)
                    {
                        recruitinitiator = db.RecruitInitiator
                           .Include(a => a.RecruitBatchInitiator)
                           .Where(a => a.Id == item).SingleOrDefault();
                        if (SkipIds != null)
                        {
                            foreach (var a in SkipIds)
                            {
                                if (fall == null)
                                    fall = db.RecruitBatchInitiator.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                                else
                                    fall = recruitinitiator.RecruitBatchInitiator.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                            }
                        }
                        else
                        {
                            fall = recruitinitiator.RecruitBatchInitiator.ToList();
                        }

                    }
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    fall1 = db.ShortlistingCriteria.ToList();

                    if (SkipIds != null)
                    {
                        foreach (var a in SkipIds)
                        {
                            if (fall1 == null)
                                fall1 = db.ShortlistingCriteria.Where(e => e.Id != a).ToList();
                            else
                                fall1 = fall1.Where(e => e.Id != a).ToList();
                        }
                    }
                    var r = (from ca in fall1 select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }

            }
            return null;
        }

        public ActionResult Getshorlistingparameter(List<int> SkipIds, string forwardata)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<int> forwardataids = null;
                RecruitBatchInitiator recruitinitiator = null;
                List<ShortlistingCriteria> fall = new List<ShortlistingCriteria>();
                List<ShortlistingCriteria> fall1 = new List<ShortlistingCriteria>();
                // var fall1="";
                if (forwardata != null)
                {
                    forwardataids = Utility.StringIdsToListIds(forwardata);
                    foreach (var item in forwardataids)
                    {
                        recruitinitiator = db.RecruitBatchInitiator
                           .Include(a => a.RecruitEvaluationProcess)
                           .Include(a => a.RecruitEvaluationProcess.Select(e => e.ShortlistingCriteria.Select(f => f.FuncStruct.Job.JobPosition)))
                           .Where(a => a.Id == item).SingleOrDefault();
                        foreach (var item1 in recruitinitiator.RecruitEvaluationProcess)
                        {
                            if (item1.ShortlistingCriteria != null)
                            {
                                if (SkipIds != null)
                                {
                                    foreach (var a in SkipIds)
                                    {
                                        if (fall == null)
                                            fall = item1.ShortlistingCriteria.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                                        else
                                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                                        fall1.AddRange(fall);
                                    }
                                }
                                else
                                {
                                    fall = item1.ShortlistingCriteria.ToList();
                                }

                                fall = db.ShortlistingCriteria
                                    .Include(t => t.FuncStruct.Job)
                                    .Include(t => t.FuncStruct.Job.JobPosition)
                                    .ToList();
                            }

                        }

                    }
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    fall = db.ShortlistingCriteria.ToList();
                    if (SkipIds != null)
                    {
                        foreach (var a in SkipIds)
                        {
                            if (fall == null)
                                fall = db.ShortlistingCriteria.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                            else
                                fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                        }
                    }
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);

                }

            }
            return null;
            return null;
        }

        [HttpPost]
        public ActionResult Create(ResumeCollection c, FormCollection form) //Create submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string shrtlistingid = form["TrainingScheduleList1"] == "0" ? "" : form["TrainingScheduleList1"];
                    string recruitbatchinitiatorid = form["RecruitBatch"] == "0" ? "" : form["RecruitBatch"];
                    string advertisereferenceno = form["AdPostListM"] == "0" ? "" : form["AdPostListM"];
                    var resumeshortlistingstatus = form["CategorylistEvalpara"] == "0" ? "" : form["CategorylistEvalpara"];
                    var Candidatelist = form["Employee-Table"] == null ? "" : form["Employee-Table"];

                    List<int> cadidateid = null;
                    List<int> recruitbatchid = null;
                    if (recruitbatchinitiatorid != "")
                    {
                        recruitbatchid = Utility.StringIdsToListIds(recruitbatchinitiatorid);
                    }
                    if (Candidatelist != "")
                    {
                        cadidateid = Utility.StringIdsToListIds(Candidatelist);
                    }

                    if (resumeshortlistingstatus != null && resumeshortlistingstatus != "" && resumeshortlistingstatus != "-Select-")
                    {
                        var value = db.LookupValue.Find(int.Parse(resumeshortlistingstatus));
                        c.ResumeSortlistingStatus = value;
                    }

                    RecruitInitiator recruitinitiator = null;
                    List<int> funids = null;
                    List<int> shortlistingidint = null;

                    List<ManPowerBudget> ManPowerBudgetf = new List<ManPowerBudget>();
                    if (shrtlistingid != null)
                    {
                        shortlistingidint = Utility.StringIdsToListIds(shrtlistingid);
                    }

                    IEnumerable<manprov> ManPower = null;
                    ShortlistingCriteria shortlistingdata = db.ShortlistingCriteria
                                           .Include(t => t.Category)
                                           .Include(t => t.MaritalStatus)
                                           .Include(t => t.MaritalStatus)
                                           .Include(t => t.Qualification)
                                           .Include(t => t.Skill)
                                           .Include(t => t.Gender)
                                           .Where(t => shortlistingidint.Contains(t.Id))
                                           .SingleOrDefault();

                    var candidatedata = db.Candidate
                                       .Include(t => t.CanSocialInfo.Category)
                                       .Include(t => t.Gender)
                                       .Include(t => t.CanName)
                                       .Include(t => t.MaritalStatus)
                                       .Include(t => t.ServiceBookDates)
                                       .Include(t => t.CanAcademicInfo.QualificationDetails)
                                       .Include(t => t.CanAcademicInfo.Skill)
                                       .ToList();
                    List<ResumeCollection> resumecollectionlist = new List<ResumeCollection>();
                    using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                              new System.TimeSpan(0, 30, 0)))
                    {

                        foreach (var item1 in cadidateid)
                        {
                            if (resumeshortlistingstatus != null && resumeshortlistingstatus != "")
                            {
                                var val = db.LookupValue.Find(int.Parse(resumeshortlistingstatus));
                                c.ResumeSortlistingStatus = val;

                                var add = db.ResumeCollection.Include(e => e.ResumeSortlistingStatus).Include(e => e.Candidate).Where(e => e.Candidate.Id == item1).SingleOrDefault();
                                IList<ResumeCollection> contactsdetails = null;
                                if (add.ShortlistingCriteria != null)
                                {
                                    contactsdetails = db.ResumeCollection.Include(t => t.ResumeSortlistingStatus).Where(x => x.Candidate.Id == item1).ToList();
                                }
                                else
                                {
                                    contactsdetails = db.ResumeCollection.Where(x => x.Candidate.Id == item1).ToList();
                                }
                                foreach (var s in contactsdetails)
                                {
                                    s.ResumeSortlistingStatus = c.ResumeSortlistingStatus;
                                    db.ResumeCollection.Attach(s);
                                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                    //await db.SaveChangesAsync();
                                    db.SaveChanges();
                                    TempData["RowVersion"] = s.RowVersion;
                                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                }
                            }
                            else
                            {
                                var contactsdetails = db.ResumeCollection.Include(e => e.Candidate).Include(e => e.ShortlistingCriteria).Where(x => x.ShortlistingCriteria.Id == item1).ToList();
                                foreach (var s in contactsdetails)
                                {
                                    s.ResumeSortlistingStatus = null;
                                    db.ResumeCollection.Attach(s);
                                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                    //await db.SaveChangesAsync();
                                    db.SaveChanges();
                                    TempData["RowVersion"] = s.RowVersion;
                                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                }
                            }

                            if (shrtlistingid != null)
                            {
                                if (shrtlistingid != "")
                                {
                                    var val = db.ShortlistingCriteria.Find(int.Parse(shrtlistingid));
                                    c.ShortlistingCriteria = val;

                                    var add = db.ResumeCollection.Include(e => e.ShortlistingCriteria).Include(e => e.Candidate).Where(e => e.Candidate.Id == item1).SingleOrDefault();
                                    IList<ResumeCollection> contactsdetails = null;
                                    if (add.ShortlistingCriteria != null)
                                    {
                                        contactsdetails = db.ResumeCollection.Where(x => x.ShortlistingCriteria.Id == add.ShortlistingCriteria.Id && x.Candidate.Id == item1).ToList();
                                    }
                                    else
                                    {
                                        contactsdetails = db.ResumeCollection.Where(x => x.Candidate.Id == item1).ToList();
                                    }
                                    foreach (var s in contactsdetails)
                                    {
                                        s.ShortlistingCriteria = c.ShortlistingCriteria;
                                        db.ResumeCollection.Attach(s);
                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                        //await db.SaveChangesAsync();
                                        db.SaveChanges();
                                        TempData["RowVersion"] = s.RowVersion;
                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    }
                                }
                            }
                            else
                            {
                                var contactsdetails = db.ResumeCollection.Include(e => e.Candidate).Include(e => e.ShortlistingCriteria).Where(x => x.ShortlistingCriteria.Id == item1).ToList();
                                foreach (var s in contactsdetails)
                                {
                                    s.ShortlistingCriteria = null;
                                    db.ResumeCollection.Attach(s);
                                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                    //await db.SaveChangesAsync();
                                    db.SaveChanges();
                                    TempData["RowVersion"] = s.RowVersion;
                                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                }
                            }
                            ResumeCollection blog = null;
                            DbPropertyValues originalBlogValues = null;
                            using (var context = new DataBaseContext())
                            {
                                blog = context.ResumeCollection.Include(e => e.Candidate)
                                    .Where(e => e.Candidate.Id == item1).SingleOrDefault();
                                originalBlogValues = context.Entry(blog).OriginalValues;
                            }
                            c.DBTrack = new DBTrack
                            {
                                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                Action = "M",
                                IsModified = blog.DBTrack.IsModified == true ? true : false,
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };

                            var CurOBJ = db.ResumeCollection.Where(e => e.Candidate.Id == item1).SingleOrDefault();
                            TempData["CurrRowVersion"] = CurOBJ.RowVersion;
                            db.Entry(CurOBJ).State = System.Data.Entity.EntityState.Detached;
                            ResumeCollection rc = new ResumeCollection()
                            {
                                Id = CurOBJ.Id,
                                DBTrack = c.DBTrack,
                            };

                            db.ResumeCollection.Attach(rc);
                            db.Entry(rc).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(rc).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            resumecollectionlist.Add(db.ResumeCollection.Where(e => e.Candidate.Id == item1).SingleOrDefault());
                            RecruitBatchInitiator recruitbatchdata = db.RecruitBatchInitiator.Include(e => e.ResumeCollection).Where(e => recruitbatchid.Contains(e.Id)).SingleOrDefault();

                            //ResumeCollection
                            if (recruitbatchdata.ResumeCollection != null)
                            {
                                resumecollectionlist.AddRange(recruitbatchdata.ResumeCollection);
                            }
                            recruitbatchdata.ResumeCollection = resumecollectionlist;
                            //OEmployeePayroll.DBTrack = dbt;
                            db.RecruitBatchInitiator.Attach(recruitbatchdata);
                            db.Entry(recruitbatchdata).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            db.Entry(recruitbatchdata).State = System.Data.Entity.EntityState.Detached;

                        }
                        ts.Complete();
                        Msg.Add(" Record Updated Successfully ");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                }


                catch (DbUpdateConcurrencyException)
                {
                    return RedirectToAction("Create", new { concurrencyError = true, id = c.Id });
                }
                catch (DataException /* dex */)
                {
                    //  return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
                    Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
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
                return null;
            }
        }

        public class P2BGridData
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string ShortListingStatus { get; set; }
            public string Fulldetails { get; set; }
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
                    string PayMonth = "";

                    if (gp.filter != null)
                        PayMonth = gp.filter;
                    if (PayMonth != null && PayMonth != "")
                    {
                        int shrotlistingid = Convert.ToInt32(PayMonth);
                        var financialyear = db.Calendar.Find(int.Parse(PayMonth));

                        var BindEmpList = db.ResumeCollection
                            .Include(e => e.Candidate)
                            .Include(e => e.ResumeSortlistingStatus)
                            .Include(e => e.ShortlistingCriteria)
                            .Include(e => e.Candidate.CanName)
                            .Where(e => e.ShortlistingCriteria.Id == shrotlistingid)
                            .ToList();

                        foreach (var z in BindEmpList)
                        {
                            if (z.Candidate != null)
                            {
                                // var all = z.ITForm16Data.Where(e => e.FinancialYear.Id == financialyear.Id).SingleOrDefault();
                                // if (all != null)
                                // {
                                view = new P2BGridData()
                                {
                                    Id = z.Id,
                                    Name = z.Candidate.CanName.FullNameFML,
                                    Fulldetails = z.FullDetails,
                                    ShortListingStatus = z.ResumeSortlistingStatus.LookupVal,
                                    // FromPeriod = all.PeriodFrom,
                                    /// Toperiod = all.PeriodTo,
                                    // Islock = all.IsLocked,
                                    // ReportDate = all.ReportDate.Value.ToString("dd/MM/yyyy")
                                };

                                model.Add(view);
                                //}
                            }

                        }

                    }
                    else
                    {
                        List<string> Msgu = new List<string>();
                        Msgu.Add("Please Select Shortlisting Criteria");
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
                            jsonData = IE.Where(e => (e.Name.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                                || (e.Fulldetails.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                                || (e.ShortListingStatus.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                                || (e.Id.ToString().Contains(gp.searchString))
                                ).Select(a => new Object[] { a.Name, a.Fulldetails, a.ShortListingStatus, a.Id }).ToList();

                            //jsonData = IE.Select(a => new  { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.Name), a.Fulldetails, a.ShortListingStatus, a.Id }).ToList();
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
                            orderfuc = (c => gp.sidx == "Name" ? c.Name.ToString() :
                                             gp.sidx == "Fulldetails" ? c.Fulldetails.ToString() :
                                             gp.sidx == "ShortlistingStatus" ? c.ShortListingStatus.ToString() :
                                             "");
                        }
                        if (gp.sord == "asc")
                        {
                            IE = IE.OrderBy(orderfuc);
                            jsonData = IE.Select(a => new Object[] { Convert.ToString(a.Name), a.Fulldetails, a.ShortListingStatus, a.Id }).ToList();
                        }
                        else if (gp.sord == "desc")
                        {
                            IE = IE.OrderByDescending(orderfuc);
                            jsonData = IE.Select(a => new Object[] { Convert.ToString(a.Name), a.Fulldetails, a.ShortListingStatus, a.Id }).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.Name), a.Fulldetails, a.ShortListingStatus, a.Id }).ToList();
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
        public class manprov
        {
            public double BudgetAmount { get; set; }
            public double SanctionedPosts { get; set; }
            public string jobname { get; set; }
            public string Name { get; set; }
            public string Fulldetails { get; set; }
            public double filledpost { get; set; }
            public double vacantpost { get; set; }
            public double ExcessPost { get; set; }
            public double CurrentCTC { get; set; }
            public double ExcessCTC { get; set; }
            public double TotalCTC { get; set; }
            public int Id { get; set; }
        }
        public ActionResult Process(P2BGrid_Parameters gp, string extraeditdata, FormCollection form)
        {
            try
            {
                DataBaseContext db = new DataBaseContext();
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                var jsonData = (Object)null;
                List<int> Manpowerbudgetid = null;
                if (extraeditdata != "")
                {
                    Manpowerbudgetid = Utility.StringIdsToListIds(extraeditdata);
                }

                IEnumerable<manprov> ManPower = null;
                var shortlistingdata = db.ShortlistingCriteria
                                      .Include(t => t.Category)
                                      .Include(t => t.MaritalStatus)
                                      .Include(t => t.MaritalStatus)
                                      .Include(t => t.Qualification)
                                      .Include(t => t.Skill)
                                      .Include(t => t.Gender)
                                      .Where(t => Manpowerbudgetid.Contains(t.Id))
                                      .SingleOrDefault();


                var candidatedata = db.Candidate
                                   .Include(t => t.CanSocialInfo.Category)
                                   .Include(t => t.Gender)
                                   .Include(t => t.CanName)
                                   .Include(t => t.MaritalStatus)
                                   .Include(t => t.ServiceBookDates)
                                   .Include(t => t.CanAcademicInfo.QualificationDetails)
                                   .Include(t => t.CanAcademicInfo.Skill)
                                   .ToList();
                List<manprov> b = new List<manprov>();
                var view = new manprov();

                //foreach (var item in shortlistingdata)
                //{
                // var oempl = "";
                //foreach (var item1 in candidatedata)
                //{
                var shortlistingempdata = candidatedata.Where(t => t.Gender.LookupVal.ToString().ToUpper() == shortlistingdata.Gender.LookupVal.ToString().ToUpper()
                   && t.MaritalStatus.LookupVal.ToString().ToUpper() == shortlistingdata.MaritalStatus.LookupVal.ToString().ToUpper()
                   && t.CanSocialInfo.Category.LookupVal.ToString().ToUpper() == shortlistingdata.Category.LookupVal.ToString()
                   ).ToList();

                if (shortlistingempdata.Count() > 0)
                {
                    foreach (var item1 in shortlistingempdata)
                    {
                        var dob = item1.ServiceBookDates != null && item1.ServiceBookDates.BirthDate != null ? item1.ServiceBookDates.BirthDate : null;

                        int age = 0;
                        age = DateTime.Now.Year - dob.Value.Year;
                        if (DateTime.Now.DayOfYear < dob.Value.DayOfYear)
                            age = age - 1;

                        if (age > shortlistingdata.AgeFrom && age <= shortlistingdata.AgeTo)
                        {
                            if (item1 != null)
                            {

                                //if ()
                                //{
                                b.Add(new manprov
                                {
                                    Id = item1.Id,
                                    Name = item1.CanName.FullNameFML,
                                    Fulldetails = item1.FullDetails

                                });

                                //}

                            }
                        }
                    }

                }



                //}
                // }
                //b.Add(oempl);
                ManPower = b;

                IEnumerable<manprov> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = ManPower;
                    if (gp.searchOper.Equals("eq"))
                    {
                        if (gp.searchField == "Id")

                            jsonData = IE.Select(a => new { a.Id }).Where((e => (e.Id.ToString().Contains(gp.searchString)))).ToList();
                        else if (gp.searchField == "Name")
                            jsonData = IE.Select(a => new { a.Id, a.Name }).Where((e => (e.Name.ToString().Contains(gp.searchString)))).ToList();
                        else if (gp.searchField == "Fulldetais")
                            jsonData = IE.Select(a => new { a.Id, a.Fulldetails }).Where((e => (e.Fulldetails.ToString().Contains(gp.searchString)))).ToList();

                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Name, a.Fulldetails }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = ManPower;
                    Func<manprov, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "Name" ? c.Name.ToString() :
                                         gp.sidx == "Fulldetails" ? c.Fulldetails.ToString() :
                                           "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.Name, a.Fulldetails }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.Name, a.Fulldetails }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Id, a.Name, a.Fulldetails }).ToList();
                    }
                    totalRecords = shortlistingempdata.Count();
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

        public ActionResult Get_Employelist(string databatch, string session, string geo_id)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                DateTime? dt = null;
                string monthyr = "";
                DateTime? dtChk = null;
                int lastDayOfMonth;


                List<Employee> data = new List<Employee>();
                List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                var compid = Convert.ToInt32(Session["CompId"].ToString());
                db.Database.CommandTimeout = 3600;
                List<int> shortlistingid = null;
                if (session != null)
                {
                    shortlistingid = Utility.StringIdsToListIds(session);
                }

                // int id = db.LookupValue.Where(t => t.LookupVal.ToString().ToUpper() == "SELECTED").Select(t => t.Id).SingleOrDefault();
                var shortlistingdata = db.ShortlistingCriteria
                                      .Include(t => t.Category)
                                      .Include(t => t.MaritalStatus)
                                      .Include(t => t.MaritalStatus)
                                      .Include(t => t.Qualification)
                                      .Include(t => t.Skill)
                                      .Include(t => t.Gender)
                                      .Where(t => shortlistingid.Contains(t.Id))
                                      .SingleOrDefault();


                var candidatedata = db.Candidate
                                   .Include(t => t.CanSocialInfo.Category)
                                   .Include(t => t.Gender)
                                   .Include(t => t.CanName)
                                   .Include(t => t.MaritalStatus)
                                   .Include(t => t.ServiceBookDates)
                                   .Include(t => t.CanAcademicInfo.QualificationDetails)
                                   .Include(t => t.CanAcademicInfo.Skill)
                                   .ToList();

                var shortlistingcandidate = db.ResumeCollection
                                          .Include(t => t.Candidate)
                                          .Include(t => t.ResumeSortlistingStatus)
                                          .Include(t => t.ResumeSortlistingStatus)
                                          .Include(t => t.Candidate.CanName)
                                          .Where(t => t.ResumeSortlistingStatus.LookupVal.ToString().ToUpper() == "SELECTED").Select(t => t.Candidate.Id).ToList();






                //if (shortlistingempdata.Count() > 0)
                //{
                //foreach (var item in shortlistingcandidate)
                //{
                var shortlistingempdata = candidatedata.Where(t => t.Gender.LookupVal.ToString().ToUpper() == shortlistingdata.Gender.LookupVal.ToString().ToUpper()
            && t.MaritalStatus.LookupVal.ToString().ToUpper() == shortlistingdata.MaritalStatus.LookupVal.ToString().ToUpper()
            && t.CanSocialInfo.Category.LookupVal.ToString().ToUpper() == shortlistingdata.Category.LookupVal.ToString()
            && !shortlistingcandidate.Contains(t.Id)
            ).ToList();
                foreach (var item1 in shortlistingempdata)
                {
                    var dob = item1.ServiceBookDates != null && item1.ServiceBookDates.BirthDate != null ? item1.ServiceBookDates.BirthDate : null;

                    int age = 0;
                    age = DateTime.Now.Year - dob.Value.Year;
                    if (DateTime.Now.DayOfYear < dob.Value.DayOfYear)
                        age = age - 1;

                    if (age > shortlistingdata.AgeFrom && age <= shortlistingdata.AgeTo)
                    {
                        if (item1 != null)
                        {

                            //if ()
                            //{
                            returndata.Add(new Utility.returndataclass
                            {
                                code = item1.Id.ToString(),
                                value = item1.FullDetails,
                            });

                            //}

                        }
                    }

                }
                // }


                var returnjson = new
                {
                    data = returndata,
                    tablename = "Employee-Table"
                };
                return Json(new List<Object> { returnjson }, JsonRequestBehavior.AllowGet);

                //// }
                //else
                //{
                //    return Json(new Utility.JsonReturnClass { success = false, responseText = "No Record Found!For This Shortlist Parameter" }, JsonRequestBehavior.AllowGet);
                //    //return Json("No Record Found", JsonRequestBehavior.AllowGet);
                //}
            }
        }

    }
}