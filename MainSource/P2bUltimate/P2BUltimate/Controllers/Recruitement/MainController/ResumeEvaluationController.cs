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
    public class ResumeEvaluationController : Controller
    {
        //
        // GET: /ResumeEvaluation/

        //
        // GET: /ResumeShortlisting/
        public ActionResult Index()
        {
            return View("~/Views/Recruitement/MainView/ResumeEvaluation/Index.cshtml");
        }
        public ActionResult partial()
        {
            return View("~/Views/Shared/Recruitement/_RecruitEvaluationSchedule.cshtml");
        }
        public ActionResult RecruitEvaluationResult()
        {
            return View("~/Views/Shared/Recruitement/_RecruitEvaluationProcessResult.cshtml");
        }
        public ActionResult ResumeEvaluationResult()
        {
            return View("~/Views/Recruitement/MainView/ResumeEvaluationResult/Index.cshtml");
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
        public ActionResult Create(RecruitEvaluationProcessResult c, FormCollection form) //Create submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string ActivityResult = form["ActivityresultList"] == "0" ? "" : form["ActivityresultList"];
                    string advertisereferenceno = form["AdPostListM"] == "0" ? "" : form["AdPostListM"];
                    string Activityletterissue = form["Activityletterissuedata"] == "0" ? "" : form["Activityletterissuedata"];
                    string Activityattedence = form["Activityattendancedata"] == "0" ? "" : form["Activityattendancedata"];
                    var Recruitevaluationpara = form["TrainingScheduleList1"] == "0" ? "" : form["TrainingScheduleList1"];
                    var Candidatelist = form["Employee-Table"] == null ? "" : form["Employee-Table"];

                    ResumeCollection resumedata = null;
                    if (Activityletterissue != "")
                    {
                        c.ActivityLetterIssue = Convert.ToBoolean(Activityletterissue);
                    }
                    if (Activityattedence != "" && Activityattedence != null)
                    {

                        c.ActivityAttendance = Convert.ToBoolean(Activityattedence);
                    }

                    List<int> cadidateid = null;
                    if (Candidatelist != "")
                    {
                        cadidateid = Utility.StringIdsToListIds(Candidatelist);
                    }

                    if (Recruitevaluationpara != null && Recruitevaluationpara != "" && Recruitevaluationpara != "-Select-")
                    {
                        var value = db.RecruitEvaluationPara.Find(int.Parse(Recruitevaluationpara));
                        c.RecruitEvaluationPara = value;
                    }

                    if (ActivityResult != "" && ActivityResult != null)
                    {
                        var val = db.LookupValue.Find(int.Parse(ActivityResult));
                        c.ActivityResult = val;
                    }
                    List<RecruitEvaluationProcessResult> OFAT = new List<RecruitEvaluationProcessResult>();
                    try
                    {

                        if (ModelState.IsValid)
                        {
                            using (TransactionScope ts = new TransactionScope())
                            {


                                c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                                RecruitEvaluationProcessResult corporate = new RecruitEvaluationProcessResult()
                                {

                                    AbsentRemark = c.AbsentRemark,
                                    ActivityAttendance = c.ActivityAttendance,
                                    ActivityDate = c.ActivityDate,
                                    ActivityLetterIssue = c.ActivityLetterIssue,
                                    ActivityResult = c.ActivityResult,
                                    ActivityScore = c.ActivityScore,
                                    PannelNarration = c.PannelNarration,
                                    RecruitEvaluationPara = c.RecruitEvaluationPara,

                                    DBTrack = c.DBTrack
                                };

                                db.RecruitEvaluationProcessResult.Add(corporate);
                                //var rtn_Obj = DBTrackFile.DBTrackSave("Recruitement/Recruitement", null, db.ChangeTracker, c.DBTrack);
                                //DT_RecruitEvaluationProcessResult DT_Corp = (DT_RecruitEvaluationProcessResult)rtn_Obj;
                                //DT_Corp.J = c.Address == null ? 0 : c.Address.Id;
                                //DT_Corp.BusinessType_Id = c.BusinessType == null ? 0 : c.BusinessType.Id;
                                //DT_Corp.ContactDetails_Id = c.ContactDetails == null ? 0 : c.ContactDetails.Id;
                                //  db.Create(DT_Corp);
                                db.SaveChanges();
                                //DBTrackFile.DBTrackSave("Core/P2b.Global", "C", corporate, null, "Corporate", null);
                                OFAT.Add(db.RecruitEvaluationProcessResult.Find(corporate.Id));
                                foreach (var item1 in cadidateid)
                                {
                                    resumedata = db.ResumeCollection.Include(e => e.Candidate).Include(e => e.RecruitEvaluationProcessResult).Where(e => e.Candidate.Id == item1).SingleOrDefault();

                                    if (resumedata.RecruitEvaluationProcessResult != null)
                                    {
                                        OFAT.AddRange(resumedata.RecruitEvaluationProcessResult);
                                    }
                                    resumedata.RecruitEvaluationProcessResult = OFAT;
                                    //OEmployeePayroll.DBTrack = dbt;
                                    db.ResumeCollection.Attach(resumedata);
                                    db.Entry(resumedata).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    db.Entry(resumedata).State = System.Data.Entity.EntityState.Detached;
                                }

                                ts.Complete();
                                Msg.Add("Data Saved Successfully.");
                                return Json(new Utility.JsonReturnClass { Id = corporate.Id, Val = corporate.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                            //catch (DbUpdateConcurrencyException)
                            //{
                            //    return RedirectToAction("Create", new { concurrencyError = true, id = c.Id });
                            //}


                        }
                        else
                        {
                            StringBuilder sb = new StringBuilder("");
                            foreach (ModelState modelState in ModelState.Values)
                            {
                                foreach (ModelError error in modelState.Errors)
                                {
                                    sb.Append(error.ErrorMessage);
                                    sb.Append("." + "\n");
                                }
                            }
                            var errorMsg = sb.ToString();
                            Msg.Add("Code Already Exists.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
                            //return this.Json(new { msg = errorMsg });
                        }
                    }
                    catch (Exception e)
                    {
                        Msg.Add(e.Message);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
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

        public ActionResult GetBatchDetails(List<int> SkipIds, string forwardata)
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

            }
            return null;
        }


        [HttpPost]
        public ActionResult GetShortListingLKDetails(List<int> SkipIds, string forwardata)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<int> forwardataids = null;
                RecruitBatchInitiator recruitbatchinitiator = null;
                List<RecruitEvaluationPara> fall = new List<RecruitEvaluationPara>();
                List<RecruitEvaluationPara> fall1 = new List<RecruitEvaluationPara>();
                // var fall1="";
                if (forwardata != null)
                {
                    forwardataids = Utility.StringIdsToListIds(forwardata);


                    foreach (var item in forwardataids)
                    {

                        recruitbatchinitiator = db.RecruitBatchInitiator
                           .Include(a => a.RecruitEvaluationProcess)
                           .Include(a => a.RecruitEvaluationProcess.Select(y => y.RecruitEvaluationPara))
                           .Include(a => a.RecruitEvaluationProcess.Select(y => y.RecruitEvaluationPara.Select(m => m.RecruitEvalPara)))
                           .Where(a => a.Id == item).SingleOrDefault();
                        foreach (var item1 in recruitbatchinitiator.RecruitEvaluationProcess)
                        {

                            //foreach (var item2 in item1.)
                            //{
                            if (item1.RecruitEvaluationPara != null)
                            {

                                if (SkipIds != null)
                                {
                                    foreach (var a in SkipIds)
                                    {
                                        if (fall == null)
                                            fall = item1.RecruitEvaluationPara.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                                        else
                                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                                        fall1.AddRange(fall);
                                    }
                                }
                                else
                                {
                                    fall = item1.RecruitEvaluationPara.ToList();
                                }

                                //fall = db.RecruitEvaluationPara
                                //    .Include(t => t.RecruitEvalPara)
                                //    .ToList();

                            }

                            //}
                        }

                    }
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    fall = db.RecruitEvaluationPara.ToList();
                    if (SkipIds != null)
                    {
                        foreach (var a in SkipIds)
                        {
                            if (fall == null)
                                fall = db.RecruitEvaluationPara.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                            else
                                fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                        }
                    }
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);

                }

            }
            return null;
        }

        //[HttpPost]
        //public ActionResult Create(ResumeCollection c, FormCollection form) //Create submit
        //{
        //    List<string> Msg = new List<string>();
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        try
        //        {
        //            string manpowerdet = form["ManPowerPostDatalist"] == "0" ? "" : form["ManPowerPostDatalist"];
        //            ManPowerBudget ManPowerBudget = null;
        //            string funid = form["fun_id"] == "0" ? null : form["fun_id"];
        //            if (funid == "")
        //            {
        //                Msg.Add("Apply the Function Struct Filter");
        //                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //            }
        //            List<int> funids = null;
        //            List<int> Manpowerbudgetid = null;

        //            if (funid != null)
        //            {
        //                funids = Utility.StringIdsToListIds(funid);
        //            }
        //            List<ManPowerBudget> ManPowerBudgetf = new List<ManPowerBudget>();
        //            if (manpowerdet != null)
        //            {
        //                Manpowerbudgetid = Utility.StringIdsToListIds(manpowerdet);
        //            }

        //            Calendar calendar = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToString().ToUpper() == "RECRUITMENTCALENDAR" && e.Default == true).SingleOrDefault();
        //            RecruitYearlyCalendar recrut = db.RecruitYearlyCalendar
        //                                        .Include(e => e.ManPowerDetailsBatch)
        //                                        .Where(e => e.RecruitmentCalendar.Id == calendar.Id).SingleOrDefault();

        //            ManPowerBudgetf = db.ManPowerBudget
        //                      .Include(e => e.FuncStruct)
        //                      .Include(e => e.FuncStruct.Job)
        //                      .Include(e => e.FuncStruct.Job.JobPosition)
        //                      .Where(e => Manpowerbudgetid.Contains(e.Id)).ToList();

        //            if (ModelState.IsValid)
        //            {
        //                using (TransactionScope ts = new TransactionScope())
        //                {
        //                    c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
        //                    //foreach (var item1 in funids)
        //                    //{
        //                    List<ManPowerPostData> b = new List<ManPowerPostData>();
        //                    foreach (var item in ManPowerBudgetf)
        //                    {
        //                        if (item != null)
        //                        {
        //                            double CurrentCTC = 0;
        //                            double totalCTC = 0;
        //                            double ExcessCTC = 0;
        //                            int vacantpost = 0;
        //                            int excesspost = 0;
        //                            double CurrentCTCMonthly = 0;
        //                            double CurrentCTCYearly = 0;

        //                            var FilledPost = db.Employee
        //                                .Include(a => a.FuncStruct)
        //                                .Include(a => a.ServiceBookDates)
        //                                .Where(e => e.FuncStruct.Id == item.FuncStruct.Id && e.ServiceBookDates.ServiceLastDate == null).ToList();

        //                            var test = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.Employee.FuncStruct).Where(q => q.Employee.FuncStruct.Id == item.FuncStruct.Id).ToList();
        //                            foreach (var aa in test)
        //                            {
        //                                var a = db.EmployeePayroll.Include(e => e.EmpSalStruct)
        //                                    .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails))
        //                                    .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.Frequency)))
        //                                    .Where(e => e.Id == aa.Id).SingleOrDefault();
        //                                CurrentCTCMonthly = CurrentCTCMonthly + a.EmpSalStruct.LastOrDefault().EmpSalStructDetails.Where(t => t.SalaryHead.Frequency.LookupVal.ToString().ToUpper() == "MONTHLY").Sum(e => e.Amount);
        //                                CurrentCTCYearly = CurrentCTCYearly + a.EmpSalStruct.LastOrDefault().EmpSalStructDetails.Where(t => t.SalaryHead.Frequency.LookupVal.ToString().ToUpper() == "YEARLY").Sum(e => e.Amount);

        //                                CurrentCTC = (CurrentCTCMonthly * 12) + CurrentCTCYearly;
        //                            }
        //                            if (item.BudgetAmount >= CurrentCTC)
        //                            {
        //                                ExcessCTC = item.BudgetAmount - CurrentCTC;
        //                            }
        //                            if (item.SanctionedPosts >= FilledPost.Count())
        //                            {
        //                                vacantpost = item.SanctionedPosts - FilledPost.Count();
        //                            }
        //                            else
        //                            {
        //                                excesspost = FilledPost.Count() - item.SanctionedPosts;
        //                            }
        //                            totalCTC = CurrentCTC + item.BudgetAmount;
        //                            if (item.SanctionedPosts >= FilledPost.Count())
        //                            {
        //                                ManPowerPostData manpowerdata = new ManPowerPostData
        //                                {
        //                                    FilledPosts = FilledPost.Count(),
        //                                    VacantPosts = vacantpost,
        //                                    BudgetCTC = item.BudgetAmount,
        //                                    SanctionedPosts = item.SanctionedPosts,
        //                                    ExcessPosts = excesspost,
        //                                    ExcessCTC = ExcessCTC,
        //                                    CurrentCTC = CurrentCTC,
        //                                    TotalCTC = totalCTC,
        //                                    ManPowerBudget = item,
        //                                    DBTrack = c.DBTrack

        //                                };
        //                                db.ManPowerPostData.Add(manpowerdata);
        //                                db.SaveChanges();

        //                                b.Add(db.ManPowerPostData.Find(manpowerdata.Id));
        //                            }

        //                        }
        //                    }



        //                    List<ManPowerDetailsBatch> mdb1 = new List<ManPowerDetailsBatch>();
        //                    //mdb1.Add(new ManPowerDetailsBatch
        //                    //{

        //                    //    BatchName = c.BatchName,
        //                    //    ProcessDate = c.ProcessDate,
        //                    //    ManPowerPostData = b,
        //                    //    DBTrack = c.DBTrack
        //                    //});

        //                    ManPowerDetailsBatch manpowerdetails = new ManPowerDetailsBatch
        //                    {
        //                        BatchName = c.BatchName,
        //                        ProcessDate = c.ProcessDate,
        //                        ManPowerPostData = b,
        //                        DBTrack = c.DBTrack
        //                    };
        //                    db.ManPowerDetailsBatch.Add(manpowerdetails);
        //                    db.SaveChanges();

        //                    mdb1.Add(db.ManPowerDetailsBatch.Find(manpowerdetails.Id));

        //                    if (recrut.ManPowerDetailsBatch != null)
        //                    {
        //                        mdb1.AddRange(recrut.ManPowerDetailsBatch);
        //                    }
        //                    recrut.ManPowerDetailsBatch = mdb1;
        //                    //OEmployeePayroll.DBTrack = dbt;
        //                    db.RecruitYearlyCalendar.Attach(recrut);
        //                    db.Entry(recrut).State = System.Data.Entity.EntityState.Modified;
        //                    db.SaveChanges();
        //                    db.Entry(recrut).State = System.Data.Entity.EntityState.Detached;
        //                    //ManPowerDetailsBatch mdb = new ManPowerDetailsBatch()
        //                    //{
        //                    //    BatchName = batchnm,
        //                    //    ProcessDate =DateTime.Parse(Procesddate),
        //                    //    ManPowerPostData=b,
        //                    //     DBTrack=c.DBTrack
        //                    //};
        //                    //  db.ManPowerDetailsBatch.Add(mdb);
        //                    //  db.SaveChanges();

        //                    //ManPowerPostData ctc = new ManPowerPostData()
        //                    //{
        //                    //    ActionDate = ActionDatee,
        //                    //    ActionMovement = c.ActionMovement,
        //                    //    ActionRecruitement = c.ActionRecruitement,
        //                    //    ManPowerDetailsBatch = mdb1,

        //                    //    DBTrack = c.DBTrack
        //                    //};

        //                    //db.ManPowerProvision.Add(ctc);
        //                    //   var rtn_Obj = DBTrackFile.DBTrackSave("ManPower", null, db.ChangeTracker,"");
        //                    //  DT_CtcDefinition DT_Corp = (DT_Corporate)rtn_Obj;

        //                    //   db.Create(DT_Corp);
        //                    db.SaveChanges();
        //                    //DBTrackFile.DBTrackSave("Core/P2b.Global", "C", corporate, null, "Corporate", null);

        //                    // }
        //                    ts.Complete();
        //                    //  return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
        //                    Msg.Add("  Data Saved successfully  ");
        //                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                }
        //            }
        //            else
        //            {
        //                StringBuilder sb = new StringBuilder("");
        //                foreach (ModelState modelState in ModelState.Values)
        //                {
        //                    foreach (ModelError error in modelState.Errors)
        //                    {
        //                        sb.Append(error.ErrorMessage);
        //                        sb.Append("." + "\n");
        //                    }
        //                }
        //                var errorMsg = sb.ToString();
        //                return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
        //                //return this.Json(new { msg = errorMsg });
        //            }
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            return RedirectToAction("Create", new { concurrencyError = true, id = c.Id });
        //        }
        //        catch (DataException /* dex */)
        //        {
        //            //  return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
        //            Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
        //            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //        }
        //        catch (Exception ex)
        //        {
        //            LogFile Logfile = new LogFile();
        //            ErrorLog Err = new ErrorLog()
        //            {
        //                ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
        //                ExceptionMessage = ex.Message,
        //                ExceptionStackTrace = ex.StackTrace,
        //                LineNo = Convert.ToString(new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
        //                LogTime = DateTime.Now
        //            };
        //            Logfile.CreateLogFile(Err);
        //            Msg.Add(ex.Message);
        //            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //}

        public class P2BGridData
        {
            public int Id { get; set; }
            public string Name { get; set; }
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
                            jsonData = IE.Where(e => (e.Id.ToString().Contains(gp.searchString))
                                    || (e.Name.ToString().Contains(gp.searchString))
                                    || (e.Fulldetails.ToString().Contains(gp.searchString))
                                    ).Select(a => new Object[] { a.Id, a.Name, a.Fulldetails, a.ReportDate, a.Islock }).ToList();
                            //jsonData = IE.Select(a => new  { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, Convert.ToString(a.Name), a.Fulldetails }).ToList();
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
                                             "");
                        }
                        if (gp.sord == "asc")
                        {
                            IE = IE.OrderBy(orderfuc);
                            jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Name), a.Fulldetails }).ToList();
                        }
                        else if (gp.sord == "desc")
                        {
                            IE = IE.OrderByDescending(orderfuc);
                            jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Name), a.Fulldetails }).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, Convert.ToString(a.Name), a.Fulldetails }).ToList();
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
                var compid = Convert.ToInt32(Session["CompId"].ToString());
                db.Database.CommandTimeout = 3600;
                List<int> evalparaidlist = null;
                List<int> Batchids = null;
                List<ResumeCollection> empdata = null;
                RecruitBatchInitiator Batchwiseempdata = null;
                List<Candidate> Emp = new List<Candidate>();
                int stages;
                if (session != null)
                {
                    evalparaidlist = Utility.StringIdsToListIds(session);
                }
                if (databatch != null)
                {
                    Batchids = Utility.StringIdsToListIds(databatch);
                }
                Batchwiseempdata = db.RecruitBatchInitiator
                                 .Include(e => e.ResumeCollection)
                                 .Include(e => e.ResumeCollection.Select(t => t.Candidate))
                                 .Include(e => e.ResumeCollection.Select(t => t.Candidate.CanName))
                                 .Include(e => e.ResumeCollection.Select(t => t.ResumeSortlistingStatus))
                                 .Include(e => e.ResumeCollection.Select(t => t.ShortlistingCriteria))
                                 .Include(e => e.ResumeCollection.Select(t => t.RecruitEvaluationProcessResult.Select(m => m.RecruitEvaluationPara)))
                                 .Include(e => e.ResumeCollection.Select(t => t.RecruitEvaluationProcessResult.Select(m => m.ActivityResult)))
                                 .Where(e => Batchids.Contains(e.Id)).SingleOrDefault();
                var recruitevalparaid = db.RecruitEvaluationPara.Include(t => t.RecruitEvalPara).Where(e => evalparaidlist.Contains(e.Id)).SingleOrDefault();
                stages = recruitevalparaid.Stage - 1;

                if (stages == 0)
                {
                    empdata = Batchwiseempdata.ResumeCollection.Where(e => e.ResumeSortlistingStatus.LookupVal.ToString().ToUpper() == "SELECTED").ToList();
                    //empdata = db.ResumeCollection
                    //    .Include(e => e.Candidate)
                    //    .Include(e => e.ResumeSortlistingStatus)
                    //    .Include(e => e.Candidate.CanName)
                    //    .Include(e => e.RecruitEvaluationProcessResult.Select(t => t.RecruitEvaluationPara))
                    //    .Include(e => e.RecruitEvaluationProcessResult.Select(t => t.ActivityResult))
                    //    .Include(e => e.ShortlistingCriteria)
                    //    .Where(e => e.ResumeSortlistingStatus.LookupVal.ToString().ToUpper() == "SELECTED")
                    //    .ToList();

                    //if (databatch != null)
                    //{
                    //    foreach (var item in empdata)
                    //    {
                    //        Emp.Add(item.Candidate);
                    //    }
                    //}

                    if (recruitevalparaid != null)
                    {

                        foreach (var item in empdata)
                        {
                            var stagequlify = item.RecruitEvaluationProcessResult.Where(t => t.RecruitEvaluationPara != null).ToList();

                            if (stagequlify.Count() == 0)
                            {
                                Emp.Add(item.Candidate);
                            }

                        }
                    }

                    List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                    if (Emp != null && Emp.Count != 0)
                    {
                        foreach (var item in Emp)
                        {
                            returndata.Add(new Utility.returndataclass
                            {
                                code = item.Id.ToString(),
                                value = item.FullDetails,
                            });
                        }

                        var returnjson = new
                        {
                            data = returndata,
                            tablename = "Employee-Table"
                        };
                        return Json(new List<Object> { returnjson }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new Utility.JsonReturnClass { success = false, responseText = "No Candidate Found !For Stage:" + recruitevalparaid.Stage + ":" + recruitevalparaid.RecruitEvalPara.LookupVal }, JsonRequestBehavior.AllowGet);
                        //return Json("No Record Found", JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    empdata = Batchwiseempdata.ResumeCollection.Where(e => e.ResumeSortlistingStatus.LookupVal.ToString().ToUpper() == "SELECTED").ToList();
                    //empdata = db.ResumeCollection
                    //    .Include(e => e.Candidate)
                    //    .Include(e => e.RecruitEvaluationProcessResult)
                    //    .Include(e => e.RecruitEvaluationProcessResult.Select(t => t.RecruitEvaluationPara))
                    //    .Include(e => e.RecruitEvaluationProcessResult.Select(t => t.ActivityResult))
                    //    .Include(e => e.ResumeSortlistingStatus)
                    //    .Include(e => e.Candidate.CanName)
                    //    .Include(e => e.ShortlistingCriteria)
                    //    .Where(e => e.ResumeSortlistingStatus.LookupVal.ToString().ToUpper() == "SELECTED")
                    //    .ToList();
                    if (recruitevalparaid != null)
                    {

                        foreach (var item in empdata)
                        {
                            var stagequlify = item.RecruitEvaluationProcessResult.Where(t => t.RecruitEvaluationPara != null && t.RecruitEvaluationPara.Stage == stages).ToList();


                            //if (stagequlify.Count() > 0)
                            //{
                            //    var currentstagepass = item.RecruitEvaluationProcessResult.Where(t => t.RecruitEvaluationPara != null && t.RecruitEvaluationPara.Stage == recruitevalparaid.Stage).ToList();
                            //    if (currentstagepass.Count() != 1)
                            //    {
                                    Emp.Add(item.Candidate);

                            //    }
                            //}

                        }
                    }

                    //if (databatch != null)
                    //{
                    //    foreach (var item in empdata)
                    //    {
                    //        Emp.Add(item.Candidate);
                    //    }
                    //}


                    List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                    if (Emp != null && Emp.Count != 0)
                    {
                        foreach (var item in Emp)
                        {
                            returndata.Add(new Utility.returndataclass
                            {
                                code = item.Id.ToString(),
                                value = item.FullDetails,
                            });
                        }

                        var returnjson = new
                        {
                            data = returndata,
                            tablename = "Employee-Table"
                        };
                        return Json(new List<Object> { returnjson }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new Utility.JsonReturnClass { success = false, responseText = "No Candidate Found !For Stage:" + recruitevalparaid.Stage + ":" + recruitevalparaid.RecruitEvalPara.LookupVal }, JsonRequestBehavior.AllowGet);
                        //return Json("No Record Found", JsonRequestBehavior.AllowGet);
                    }
                }

            }
        }
        public class returndatagridclass //Parentgrid
        {
            public string Id { get; set; }
            public string CandidateName { get; set; }
            public string Fulldetails { get; set; }
            public string HrEvaluationStatus { get; set; }
        }
        public ActionResult Formula_Grid(ParamModel param, string Filterdata)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                List<ResumeCollection> Candidatedata = new List<ResumeCollection>();
                RecruitBatchInitiator batchwiseemp = null;
                try
                {
                    var id = int.Parse(Session["CompId"].ToString());
                    if (Filterdata != "")
                    {
                        int batchid = Convert.ToInt32(Filterdata);
                        //var a = db.PayScaleAssignment.Include(e => e.SalHeadFormula).Include(e => e.PayScaleAgreemnt).Where(e => e.Id == data).ToList();
                        batchwiseemp = db.RecruitBatchInitiator
                            .Include(e => e.ResumeCollection)
                            .Include(e => e.ResumeCollection.Select(t => t.Candidate))
                            .Include(e => e.ResumeCollection.Select(t => t.Candidate.CanName))
                            .Include(e => e.ResumeCollection.Select(t => t.HRJoiningStatus))
                            .Include(e => e.ResumeCollection.Select(t => t.ResumeSortlistingStatus))
                            .Where(e => e.Id == batchid).SingleOrDefault();

                        Candidatedata = batchwiseemp.ResumeCollection.Where(e => e.ResumeSortlistingStatus != null && e.ResumeSortlistingStatus.LookupVal.ToString().ToUpper() == "SELECTED").ToList();
                    }
                    else
                    {
                        Candidatedata = db.ResumeCollection
                                      .Include(e => e.Candidate)
                                      .Include(e => e.Candidate.CanName)
                                       .Include(e => e.HRJoiningStatus)
                                       .Include(e => e.ResumeSortlistingStatus)
                                     . Where(e => e.ResumeSortlistingStatus != null && e.ResumeSortlistingStatus.LookupVal.ToString().ToUpper() == "SELECTED").ToList();
                    }
                    //var all = Sal.GroupBy(e => e.GeoStruct.Id).Select(e => e.FirstOrDefault()).ToList();
                    // for searchs
                    IEnumerable<ResumeCollection> fall;
                    string DependRule = "";
                    if (param.sSearch == null)
                    {
                        fall = Candidatedata;

                    }
                    else
                    {
                        fall = Candidatedata.Where(e => (e.Id.ToString().Contains(param.sSearch))
                            || (e.Candidate == null ? false : e.Candidate.CanName == null ? false : e.Candidate.CanName.FullNameFML.Contains(param.sSearch.ToUpper()))
                            || (e.FullDetails == null ? false : e.FullDetails.Contains(param.sSearch))
                            ).ToList();

                    }

                    //for column sorting
                    var sortindex = Convert.ToInt32(Request["iSortCol_0"]);

                    Func<ResumeCollection, string> orderfunc = (c =>
                                                                Convert.ToUInt32(sortindex) == 0 ? c.Id.ToString() :
                                                                sortindex == 1 ? c.Candidate == null ? "" : c.Candidate.CanName == null ? "" : c.Candidate.CanName.FullNameFML : "");
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
                    var dcompanies = fall
                            .Skip(param.iDisplayStart).Take(param.iDisplayLength).ToList();
                    if (dcompanies.Count == 0)
                    {
                        List<returndatagridclass> result = new List<returndatagridclass>();
                        foreach (var item in fall)
                        {
                            result.Add(new returndatagridclass
                            {
                                Id = item.Id.ToString(),
                                CandidateName = item.Candidate == null ? "" : item.Candidate.CanName == null ? "" : item.Candidate.CanName.FullNameFML,
                                Fulldetails = item.FullDetails,
                            });
                        }


                        return Json(new
                        {
                            sEcho = param.sEcho,
                            iTotalRecords = Candidatedata.Count(),
                            iTotalDisplayRecords = fall.Count(),
                            data = result
                        }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        var result = from c in dcompanies
                                     select new[] { null, Convert.ToString(c.Id), c.Candidate.CanName.FullNameFML, c.FullDetails };
                        return Json(new
                        {
                            sEcho = param.sEcho,
                            iTotalRecords = Candidatedata.Count(),
                            iTotalDisplayRecords = fall.Count(),
                            data = result
                        }, JsonRequestBehavior.AllowGet);
                    }//for data reterivation
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
        public ActionResult Formula_Grid2(ParamModel param, string Filterdata)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                List<ResumeCollection> Candidatedata = new List<ResumeCollection>();
                RecruitBatchInitiator batchwiseemp = null;
                try
                {
                    var id = int.Parse(Session["CompId"].ToString());
                    if (Filterdata != "")
                    {
                        int batchid = Convert.ToInt32(Filterdata);
                        //var a = db.PayScaleAssignment.Include(e => e.SalHeadFormula).Include(e => e.PayScaleAgreemnt).Where(e => e.Id == data).ToList();
                        batchwiseemp = db.RecruitBatchInitiator
                            .Include(e => e.ResumeCollection)
                            .Include(e => e.ResumeCollection.Select(t => t.RecruitEvaluationProcessResult))
                            .Include(e => e.ResumeCollection.Select(t => t.Candidate))
                            .Include(e => e.ResumeCollection.Select(t => t.Candidate.CanName))
                            .Include(e => e.ResumeCollection.Select(t => t.HRJoiningStatus))
                            .Include(e => e.ResumeCollection.Select(t => t.ResumeSortlistingStatus))
                            .Where(e => e.Id == batchid).SingleOrDefault();

                        Candidatedata = batchwiseemp.ResumeCollection.Where(e => e.ResumeSortlistingStatus != null && e.ResumeSortlistingStatus.LookupVal.ToString().ToUpper() == "SELECTED").ToList();
                    }
                    else
                    {
                        Candidatedata = db.ResumeCollection
                                      .Include(e => e.Candidate)
                                      .Include(e => e.Candidate.CanName)
                                       .Include(e => e.HRJoiningStatus)
                                       .Include(e => e.RecruitEvaluationProcessResult)
                                       .Include(e => e.ResumeSortlistingStatus)
                                     .Where(e => e.ResumeSortlistingStatus != null && e.ResumeSortlistingStatus.LookupVal.ToString().ToUpper() == "SELECTED").ToList();
                    }
                    //var all = Sal.GroupBy(e => e.GeoStruct.Id).Select(e => e.FirstOrDefault()).ToList();
                    // for searchs
                    IEnumerable<ResumeCollection> fall;
                    string DependRule = "";
                    if (param.sSearch == null)
                    {
                        fall = Candidatedata;

                    }
                    else
                    {
                        fall = Candidatedata.Where(e => (e.Id.ToString().Contains(param.sSearch))
                            || (e.Candidate == null ? false : e.Candidate.CanName == null ? false : e.Candidate.CanName.FullNameFML.Contains(param.sSearch.ToUpper()))
                            || (e.FullDetails == null ? false : e.FullDetails.Contains(param.sSearch))
                            || (e.HREvaluationStatus == null ? false : e.HREvaluationStatus.LookupVal.Contains(param.sSearch))
                            ).ToList();

                    }

                    //for column sorting
                    var sortindex = Convert.ToInt32(Request["iSortCol_0"]);

                    Func<ResumeCollection, string> orderfunc = (c =>
                                                                Convert.ToUInt32(sortindex) == 0 ? c.Id.ToString() :
                                                                sortindex == 1 ? c.Candidate == null ? "" : c.Candidate.CanName == null ? "" : c.Candidate.CanName.FullNameFML : "");
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
                    var dcompanies = fall
                            .Skip(param.iDisplayStart).Take(param.iDisplayLength).ToList();
                    if (dcompanies.Count == 0)
                    {
                        List<returndatagridclass> result = new List<returndatagridclass>();
                        foreach (var item in fall)
                        {
                            var activitydateconfirmemployee = item.RecruitEvaluationProcessResult.Where(t => t.ActivityDate != null).ToList();
                            if (activitydateconfirmemployee.Count() > 0)
                            {
                                result.Add(new returndatagridclass
                                {
                                    Id = item.Id.ToString(),
                                    CandidateName = item.Candidate == null ? "" : item.Candidate.CanName == null ? "" : item.Candidate.CanName.FullNameFML,
                                    Fulldetails = item.FullDetails,
                                    HrEvaluationStatus = item.HREvaluationStatus == null ? "" : item.HREvaluationStatus.LookupVal
                                });
                            }
                        }


                        return Json(new
                        {
                            sEcho = param.sEcho,
                            iTotalRecords = Candidatedata.Count(),
                            iTotalDisplayRecords = fall.Count(),
                            data = result
                        }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        var result = from c in dcompanies
                                     select new[] { null, Convert.ToString(c.Id), c.Candidate.CanName.FullNameFML, c.FullDetails, c.HREvaluationStatus == null ? "" : c.HREvaluationStatus.LookupVal };
                        return Json(new
                        {
                            sEcho = param.sEcho,
                            iTotalRecords = Candidatedata.Count(),
                            iTotalDisplayRecords = fall.Count(),
                            data = result
                        }, JsonRequestBehavior.AllowGet);
                    }//for data reterivation
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

        public class FormChildDataClass //childgrid"Id", "ActivityDate", "ActivityLetterIssue","ActivityAttendance","AbsentRemark","ActivityScore","ActivityResult"
        {
            public int Id { get; set; }
            public string ActivityDate { get; set; }
            public bool ActivityLetterIssue { get; set; }
            public bool ActivityAttendance { get; set; }
            public string AbsentRemark { get; set; }
            public double ActivityScore { get; set; }
            public string ActivityResult { get; set; }
            public string StageFulldetails { get; set; }
        }
        public ActionResult Get_FormulaStructDetails(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    int id = Convert.ToInt32(Session["CompId"]);


                    var db_data = db.ResumeCollection
                        .Include(e => e.RecruitEvaluationProcessResult)
                        .Include(e => e.RecruitEvaluationProcessResult.Select(a => a.ActivityResult))
                        .Include(e => e.RecruitEvaluationProcessResult.Select(t => t.RecruitEvaluationPara))
                        .Include(e => e.RecruitEvaluationProcessResult.Select(t => t.RecruitEvaluationPara.RecruitEvalPara))
                        .Where(e => e.Id == data).SingleOrDefault();

                    if (db_data != null)
                    {
                        List<FormChildDataClass> returndata = new List<FormChildDataClass>();

                        foreach (var a in db_data.RecruitEvaluationProcessResult)//"ActivityDate", "ActivityLetterIssue","ActivityAttendance","AbsentRemark","ActivityScore","ActivityResult"
                        {
                            returndata.Add(new FormChildDataClass
                            {
                                Id = a.Id,
                                ActivityDate = a.ActivityDate.Value.ToShortDateString(),
                                ActivityLetterIssue = a.ActivityLetterIssue == null ? false : a.ActivityLetterIssue,
                                ActivityAttendance = a.ActivityAttendance == null ? false : a.ActivityAttendance,
                                AbsentRemark = a.AbsentRemark == null ? "" : a.AbsentRemark,
                                ActivityScore = a.ActivityScore == null ? 0 : a.ActivityScore,
                                ActivityResult = a.ActivityResult == null ? "" : a.ActivityResult.LookupVal,
                                StageFulldetails = a.RecruitEvaluationPara == null ? "" : a.RecruitEvaluationPara.FullDetails

                            });
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
        public ActionResult EditGridDetails(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.RecruitEvaluationProcessResult
                    .Include(e => e.ActivityResult)
                 .Where(e => e.Id == data).AsEnumerable().Select
                 (c => new
                 {
                     ActivityDate = c.ActivityDate.Value.ToShortDateString(),
                     ActivityLetterIssue = c.ActivityLetterIssue == null ? false : c.ActivityLetterIssue,
                     ActivityAttendance = c.ActivityAttendance == null ? false : c.ActivityAttendance,
                     AbsentRemark = c.AbsentRemark == null ? "" : c.AbsentRemark,
                     ActivityScore = c.ActivityScore == null ? 0 : c.ActivityScore,
                     ActivityResult = c.ActivityResult == null ? 0 : c.ActivityResult.Id,
                 }).SingleOrDefault();
                return Json(new { data = Q }, JsonRequestBehavior.AllowGet);
            }
        }
        List<String> Msg = new List<String>();
        public ActionResult GridEditSave(RecruitEvaluationProcessResult c, FormCollection form, string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                // string ContactDetails = form["FacultySpecializationlist"] == "0" ? "" : form["FacultySpecializationlist"];
                //  bool Auth = form["Autho_Action"] == "" ? false : true;
                bool Auth = form["Autho_Allow"] == "true" ? true : false;
                int id = Convert.ToInt32(data);
                var db_Data = db.RecruitEvaluationProcessResult
                       .Include(e => e.ActivityResult)
                       .Include(e => e.RecruitEvaluationPara)
                        .Where(e => e.Id == id).SingleOrDefault();

                var ActivityAttendance = form["ActivityAttendanceAppl"] == null ? "" : form["ActivityAttendanceAppl"];
                var ActivityLetterIssue = form["ActivityLetterIssueAppl"] == null ? "" : form["ActivityLetterIssueAppl"];

                if (ActivityAttendance != "")
                {
                    db_Data.ActivityAttendance = Convert.ToBoolean(ActivityAttendance);
                }
                if (ActivityLetterIssue != "")
                {
                    db_Data.ActivityLetterIssue = Convert.ToBoolean(ActivityLetterIssue);
                }

                string ActivityResult = form["ActivityResultlist"] == null ? "" : form["ActivityResultlist"];
                string RecruitEvaluationPara = form["RecruitEvaluationParalist"] == null ? "" : form["RecruitEvaluationParalist"];
                if (ActivityResult != "" && ActivityResult != null)
                {
                    var val = db.LookupValue.Find(int.Parse(ActivityResult));
                    db_Data.ActivityResult = val;
                }

                db_Data.AbsentRemark = c.AbsentRemark;
                db_Data.ActivityScore = c.ActivityScore;
                db_Data.ActivityDate = c.ActivityDate;

                try
                {
                    db.RecruitEvaluationProcessResult.Attach(db_Data);
                    db.Entry(db_Data).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    db.Entry(db_Data).State = System.Data.Entity.EntityState.Detached;
                    return Json(new { status = true, data = db_Data, responseText = "Record Updated..!" }, JsonRequestBehavior.AllowGet);



                }
                catch (Exception e)
                {

                    throw e;
                }
            }
            return null;
        }
        public ActionResult GridDelete(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var LvEP = db.RecruitEvaluationProcessResult.Find(data);
                db.RecruitEvaluationProcessResult.Remove(LvEP);
                db.SaveChanges();
                List<string> Msgs = new List<string>();
                Msgs.Add("Record Deleted Successfully ");
                return Json(new Utility.JsonReturnClass { success = true, responseText = Msgs }, JsonRequestBehavior.AllowGet);

            }
        }
        public class City_Info
        {
            public Array Cityid { get; set; }
            public Array CityFulldetails { get; set; }

        }
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<City_Info> return_data = new List<City_Info>();
                var Q = db.ResumeCollection
                    .Include(e => e.Candidate)
                    .Include(e => e.HREvaluationStatus)
                    .Include(e => e.Candidate.CanName)
                 .Where(e => e.Id == data).AsEnumerable().Select
                 (c => new
                 {
                     CityTypeper = c.Candidate.FullDetails,
                     hrevaluationid = c.HREvaluationStatus == null ? "" : c.HREvaluationStatus.Id.ToString(),
                     interviewdate = c.InterviewDate == null ? "" : c.InterviewDate.Value.ToShortDateString()
                 }).ToList();

                //var Citydetails = db.HRAExemptionMaster.Include(e => e.City).Where(e => e.Id == data).Select(e => e.City).ToList();
                //if (Citydetails != null && Citydetails.Count > 0)
                //{
                //    foreach (var ca in Citydetails)
                //    {
                //        return_data.Add(new City_Info
                //        {
                //            Cityid = ca.Select(e => e.Id).ToArray(),
                //            CityFulldetails = ca.Select(e => e.FullDetails).ToArray()

                //        });

                //    }

                //}
                return Json(new Object[] { Q, return_data, JsonRequestBehavior.AllowGet });
            }
        }
        public ActionResult EditSave(ResumeCollection c, int data, FormCollection form)
        {
            List<string> Msg = new List<string>();
            var Hrevaluationstatus = form["CategorylistEvalpara"] == "0" ? "" : form["CategorylistEvalpara"];


            using (DataBaseContext db = new DataBaseContext())
            {
                if (Hrevaluationstatus != null && Hrevaluationstatus != "")
                {
                    var val = db.LookupValue.Find(int.Parse(Hrevaluationstatus));
                    c.HREvaluationStatus = val;

                    var add = db.ResumeCollection.Include(e => e.HREvaluationStatus).Where(e => e.Id == data).SingleOrDefault();
                    IList<ResumeCollection> contactsdetails = null;
                    if (add.ShortlistingCriteria != null)
                    {
                        contactsdetails = db.ResumeCollection.Include(t => t.HREvaluationStatus).Where(x => x.Id == data).ToList();
                    }
                    else
                    {
                        contactsdetails = db.ResumeCollection.Where(x => x.Id == data).ToList();
                    }
                    foreach (var s in contactsdetails)
                    {
                        s.HREvaluationStatus = c.HREvaluationStatus;
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
                    var contactsdetails = db.ResumeCollection.Include(e => e.HREvaluationStatus).Where(x => x.Id == data).ToList();
                    foreach (var s in contactsdetails)
                    {
                        s.HREvaluationStatus = null;
                        db.ResumeCollection.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }
                try
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                              new System.TimeSpan(0, 30, 0)))
                    {
                        ResumeCollection blog = null;
                        DbPropertyValues originalBlogValues = null;
                        using (var context = new DataBaseContext())
                        {
                            blog = context.ResumeCollection.Include(e => e.Candidate)
                                .Where(e => e.Id == data).SingleOrDefault();
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

                        var CurOBJ = db.ResumeCollection.Where(e => e.Id == data).SingleOrDefault();
                        TempData["CurrRowVersion"] = CurOBJ.RowVersion;
                        db.Entry(CurOBJ).State = System.Data.Entity.EntityState.Detached;
                        ResumeCollection rc = new ResumeCollection()
                        {
                            Id = CurOBJ.Id,
                            DBTrack = c.DBTrack,
                            InterviewDate = c.InterviewDate
                        };

                        db.ResumeCollection.Attach(rc);
                        db.Entry(rc).State = System.Data.Entity.EntityState.Modified;
                        db.Entry(rc).OriginalValues["RowVersion"] = TempData["RowVersion"];
                        db.SaveChanges();
                        ts.Complete();
                        Msg.Add(" Record Updated Successfully ");
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
                return View();
            }
        }



    }
}