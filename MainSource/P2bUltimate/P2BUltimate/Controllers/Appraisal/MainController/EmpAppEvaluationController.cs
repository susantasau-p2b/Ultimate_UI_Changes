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
using Appraisal;
using Training;

namespace P2BUltimate.Controllers.Appraisal.MainController
{

    public class EmpAppEvaluationController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();
        public ActionResult Index()
        {
            return View("~/Views/Appraisal/MainViews/EmpAppEvaluation/Index.cshtml");
        }

        public ActionResult GridPartial()
        {
            return View("~/Views/Shared/Appraisal/_EmpAppEvaluation.cshtml");
        }

        public ActionResult P2BGrid1(P2BGrid_Parameters gp)
        {
            List<string> Msg = new List<string>();
            try
            {
                DataBaseContext db = new DataBaseContext();
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                var jsonData = (Object)null;
                IEnumerable<EmpAppEvaluation> Corporate = null;
                if (gp.IsAutho == true)
                {
                    Corporate = db.EmpAppEvaluation.Include(e => e.EmpAppRatingConclusion).Include(e => e.YearlyProgramAssignment).Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    Corporate = db.EmpAppEvaluation.Include(e => e.EmpAppRatingConclusion).Include(e => e.YearlyProgramAssignment).AsNoTracking().ToList();
                }

                IEnumerable<EmpAppEvaluation> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = Corporate;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.Id.ToString().Contains(gp.searchString))
                           || (e.MaxPoints.ToString().Contains(gp.searchString))
                           || (e.SecurePoints.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                           ).Select(a => new { a.Id, a.MaxPoints, a.SecurePoints }).ToList();
                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.MaxPoints, a.SecurePoints }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = Corporate;
                    Func<EmpAppEvaluation, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "MaxPoints" ? c.MaxPoints.ToString() :
                                         gp.sidx == "SecurePoints" ? c.SecurePoints.ToString() : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.MaxPoints), Convert.ToString(a.SecurePoints), "" }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.MaxPoints), Convert.ToString(a.SecurePoints), "" }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.MaxPoints, a.SecurePoints, "" }).ToList();
                    }
                    totalRecords = Corporate.Count();
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
        public ActionResult P2BGrid(P2BGrid_Parameters gp)
        {
            List<string> Msg = new List<string>();


            try
            {
                DataBaseContext db = new DataBaseContext();
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                var jsonData = (Object)null;
                IEnumerable<EmpAppEvaluation> Corporate = null;
                if (gp.IsAutho == true)
                {
                    Corporate = db.EmpAppEvaluation.Include(e => e.EmpAppRatingConclusion).Include(e => e.YearlyProgramAssignment).Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    Corporate = db.EmpAppEvaluation.Include(e => e.EmpAppRatingConclusion).Include(e => e.YearlyProgramAssignment).AsNoTracking().ToList();
                }

                IEnumerable<EmpAppEvaluation> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = Corporate;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.Id.ToString().Contains(gp.searchString))
                           || (e.MaxPoints.ToString().Contains(gp.searchString))
                           || (e.SecurePoints.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                           ).Select(a => new { a.Id, a.MaxPoints, a.SecurePoints }).ToList();
                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.MaxPoints, a.SecurePoints }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = Corporate;
                    Func<EmpAppEvaluation, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "MaxPoints" ? c.MaxPoints.ToString() :
                                         gp.sidx == "SecurePoints" ? c.SecurePoints.ToString() : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.MaxPoints), Convert.ToString(a.SecurePoints), "" }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.MaxPoints), Convert.ToString(a.SecurePoints), "" }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.MaxPoints, a.SecurePoints, "" }).ToList();
                    }
                    totalRecords = Corporate.Count();
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
        public ActionResult Create(EmpAppEvaluation p, FormCollection form) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string Emp = form["Employee-Table"] == null ? null : form["Employee-Table"];
                string SchName = form["txtPayMonth1"] == null ? null : form["txtPayMonth1"];
                string EmpAppRatingConclusion = form["EmpAppRatingConclusionlist"] == "0" ? "" : form["EmpAppRatingConclusionlist"];
                var IsRecommendDiscussion = form["IsRecommendDiscussion"] == "0" ? "" : form["IsRecommendDiscussion"];
                var HrRecommendIncrement = form["HrRecommendIncrement"] == "0" ? "" : form["HrRecommendIncrement"];
                var IsRecommnedPromotion = form["IsRecommnedPromotion"] == "0" ? "" : form["IsRecommnedPromotion"];
                var IsRecommendTraining = form["IsRecommendTraining"] == "0" ? "" : form["IsRecommendTraining"];
                var IsTrClose = form["IsTrClose"] == "0" ? "" : form["IsTrClose"];
                string YearlyTrainingCalendarT = form["YearlyProgramAssignmentlist"] == "0" ? "" : form["YearlyProgramAssignmentlist"];

                string TotalMaxPNTs = form["MaxPoints"] == "0" ? "0" : form["MaxPoints"];
                string TotalRatePNTs = form["SecurePoints"] == "0" ? "0" : form["SecurePoints"];
                string ScorePERCENT = form["ScorePercentage"] == "0" ? "0" : form["ScorePercentage"];

                List<String> Msg = new List<String>();
                try
                {
                    List<int> ids1 = null;
                    if (Emp != null && Emp != "0" && Emp != "false")
                    {
                        ids1 = Utility.StringIdsToListIds(Emp);
                    }
                    else
                    {
                        List<string> Msgu = new List<string>();
                        Msgu.Add("  Kindly select employee  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msgu }, JsonRequestBehavior.AllowGet);
                    }

                    var iid = Convert.ToInt32(ids1[0]);
                    var q1 = db.Employee.Where(q => q.Id == (iid)).SingleOrDefault();

                    p.EmpAppRatingConclusion = null;
                    List<EmpAppRatingConclusion> cp = new List<EmpAppRatingConclusion>();

                    if (EmpAppRatingConclusion != null && EmpAppRatingConclusion != "")
                    {
                        var ids = Utility.StringIdsToListIds(EmpAppRatingConclusion);
                        foreach (var ca in ids)
                        {
                            var p_val = db.EmpAppRatingConclusion.Find(ca);
                            cp.Add(p_val);
                            p.EmpAppRatingConclusion = cp;
                        }
                    }

                    p.YearlyProgramAssignment = null;
                    List<YearlyProgramAssignment> cp1 = new List<YearlyProgramAssignment>();
                    if (YearlyTrainingCalendarT != null && YearlyTrainingCalendarT != "")
                    {
                        var ids = Utility.StringIdsToListIds(YearlyTrainingCalendarT);
                        foreach (var ca in ids)
                        {
                            var p_val = db.YearlyProgramAssignment.Find(ca);
                            cp1.Add(p_val);
                            p.YearlyProgramAssignment = cp1;

                        }
                    }

                    p.IsRecommendDiscussion = Convert.ToBoolean(IsRecommendDiscussion);
                    p.HrRecommendIncrement = Convert.ToBoolean(HrRecommendIncrement);
                    p.IsRecommnedPromotion = Convert.ToBoolean(IsRecommnedPromotion);
                    p.IsRecommendTraining = Convert.ToBoolean(IsRecommendTraining);
                    p.IsTrClose = Convert.ToBoolean(IsTrClose);

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            //if (db.PostDetails.Any(o => o.FuncStruct.JobPosition == p.FuncStruct.JobPosition))
                            //{
                            //    Msg.Add("Code Already Exists.");
                            //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //}

                            p.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };                     //???????

                            EmpAppEvaluation OEmpAppEvalNew = db.EmployeeAppraisal.Include(e => e.EmpAppEvaluation).Include(e => e.EmpAppEvaluation.Select(r => r.AppraisalSchedule))
                                .Where(e => e.Employee.Id == iid).SingleOrDefault().EmpAppEvaluation.Where(r => r.AppraisalSchedule.BatchName == SchName).SingleOrDefault();


                            if (OEmpAppEvalNew != null)
                            {
                                OEmpAppEvalNew.DiscussionSchedule = p.DiscussionSchedule;
                                OEmpAppEvalNew.HRComments = p.HRComments == null ? "" : p.HRComments;
                                OEmpAppEvalNew.IncPercent = p.IncPercent;
                                OEmpAppEvalNew.MaxPoints = Convert.ToInt32(TotalMaxPNTs);
                                OEmpAppEvalNew.SecurePoints = Convert.ToInt32(TotalRatePNTs);
                                OEmpAppEvalNew.ScorePercentage = Convert.ToDouble(ScorePERCENT);
                                OEmpAppEvalNew.DiscussionNode = p.DiscussionNode == null ? "" : p.DiscussionNode;

                                OEmpAppEvalNew.EmpAppRatingConclusion = p.EmpAppRatingConclusion;
                                OEmpAppEvalNew.YearlyProgramAssignment = p.YearlyProgramAssignment;

                                OEmpAppEvalNew.IsTrClose = p.IsTrClose;
                                OEmpAppEvalNew.IsRecommendDiscussion = p.IsRecommendDiscussion;
                                OEmpAppEvalNew.HrRecommendIncrement = p.HrRecommendIncrement;
                                OEmpAppEvalNew.IsRecommnedPromotion = p.IsRecommnedPromotion;
                                OEmpAppEvalNew.IsRecommendTraining = p.IsRecommendTraining;
                                OEmpAppEvalNew.DBTrack = p.DBTrack;

                                db.EmpAppEvaluation.Attach(OEmpAppEvalNew);
                                db.Entry(OEmpAppEvalNew).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                            }
                            ts.Complete();
                            Msg.Add("Data updated successfully.");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
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
                        Msg.Add(errorMsg);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception e)
                {
                    LogFile Logfile = new LogFile();
                    ErrorLog Err = new ErrorLog()
                    {
                        ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                        ExceptionMessage = e.Message,
                        ExceptionStackTrace = e.StackTrace,
                        LineNo = Convert.ToString(new System.Diagnostics.StackTrace(e, true).GetFrame(0).GetFileLineNumber()),
                        LogTime = DateTime.Now
                    };
                    Logfile.CreateLogFile(Err);
                    Msg.Add(e.Message);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public class EmpAppEvaluationD
        {
            public DateTime? DiscussionScheduleP { get; set; }
            public string HRCommentsP { get; set; }
            public double IncPercentP { get; set; }
            public int MaxPointsP { get; set; }
            public int SecurePointsP { get; set; }
            public double ScorePercentageP { get; set; }
            public string DiscussionNodeP { get; set; }

            //public List<EmpAppRatingConclusion> EmpAppRatingConclusionP{ get; set; }
            //public List<YearlyProgramAssignment> YearlyTrainingCalendarP { get; set; }

            public Array EmpAppRatingConclusionP { get; set; }
            public Array YearlyTrainingCalendarP { get; set; }

            public Boolean IsTrCloseP { get; set; }
            public Boolean IsRecommendDiscussionP { get; set; }
            public Boolean HrRecommendIncrementP { get; set; }
            public Boolean IsRecommnedPromotionP { get; set; }
            public string EmpNameP { get; set; }
            public Boolean IsRecommendTrainingP { get; set; }

            public Array YearlyTrainingCalendarDetails_Id { get; set; }
            public Array YearlyTrainingCalendarDetails_val { get; set; }
            public Array EmpAppRatingConclusionDetails_Id { get; set; }
            public Array EmpAppRatingConclusionDetails_val { get; set; }
        }

        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var EmpAppevaluation = new EmpAppEvaluation();
                var emp = EmpAppevaluation.Id;

                var EmployeeAppraisal = db.EmployeeAppraisal.Include(e => e.EmpAppEvaluation)
                    .Include(e => e.Employee)
                    .Include(e => e.Employee.EmpName)
                    .Where(e => e.EmpAppEvaluation.Any(af => af.Id == data))
                    .Select(e => new { Employee = e.Employee, EmpAppEvaluation = e.EmpAppEvaluation.ToList() }).SingleOrDefault();
                var empcode = EmployeeAppraisal.Employee.CardCode.ToString();
                var nm = db.Employee.Where(a => a.CardCode.ToString() == empcode.ToString()).Select(a => a.EmpName).FirstOrDefault();
                int na;
                var aa = 0;
                if (EmployeeAppraisal != null)
                {
                    aa = EmployeeAppraisal.Employee.Id;
                    na = aa;
                    var name = new NameSingle();
                    var full = db.NameSingle.Where(ad => ad.Id == na).Select(q => new { FullNameFML = name.FullNameFML }).SingleOrDefault();
                }

                var Q = db.EmpAppEvaluation
                    .Include(e => e.EmpAppRatingConclusion)
                    .Include(e => e.YearlyProgramAssignment).Where(e => e.Id == data).Select
                    (p => new
                    {
                        DiscussionSchedule = p.DiscussionSchedule,
                        HRComments = p.HRComments == null ? "" : p.HRComments,
                        IncPercent = p.IncPercent == null ? 0 : p.IncPercent,
                        MaxPoints = p.MaxPoints == null ? 0 : p.MaxPoints,
                        SecurePoints = p.SecurePoints == null ? 0 : p.SecurePoints,
                        ScorePercentage = p.ScorePercentage == null ? 0 : p.ScorePercentage,
                        DiscussionNode = p.DiscussionNode == null ? "" : p.DiscussionNode,

                        EmpAppRatingConclusion = p.EmpAppRatingConclusion,
                        YearlyProgramAssignment = p.YearlyProgramAssignment,

                        IsTrCloseP = p.IsTrClose,
                        IsRecommendDiscussion = p.IsRecommendDiscussion,
                        HrRecommendIncrement = p.HrRecommendIncrement,
                        IsRecommnedPromotion = p.IsRecommnedPromotion,
                        //  EmpName = EmployeeAppraisal != null && EmployeeAppraisal.Employee != null && EmployeeAppraisal.Employee.EmpName != null ? EmployeeAppraisal.Employee.EmpName.FullNameFML : null,
                        IsRecommendTraining = p.IsRecommendTraining,
                        Action = p.DBTrack.Action
                    }).SingleOrDefault();


                List<EmpAppEvaluationD> pst = new List<EmpAppEvaluationD>();
                var b = db.EmpAppEvaluation
                    .Include(e => e.EmpAppRatingConclusion)
                    .Include(e => e.YearlyProgramAssignment).Where(e => e.Id == data).ToList();
                foreach (var ca in b)
                {
                    pst.Add(new EmpAppEvaluationD
                    {
                        DiscussionScheduleP = ca.DiscussionSchedule,
                        HRCommentsP = ca.HRComments == null ? "" : ca.HRComments,
                        IncPercentP = ca.IncPercent == null ? 0 : ca.IncPercent,
                        MaxPointsP = ca.MaxPoints == null ? 0 : ca.MaxPoints,
                        SecurePointsP = ca.SecurePoints == null ? 0 : ca.SecurePoints,
                        ScorePercentageP = ca.ScorePercentage == null ? 0 : ca.ScorePercentage,
                        DiscussionNodeP = ca.DiscussionNode == null ? "" : ca.DiscussionNode,

                        //EmpAppRatingConclusionP = ca.EmpAppRatingConclusion,
                        //YearlyTrainingCalendarP = ca.YearlyProgramAssignment,

                        IsTrCloseP = ca.IsTrClose,
                        IsRecommendDiscussionP = ca.IsRecommendDiscussion,
                        HrRecommendIncrementP = ca.HrRecommendIncrement,
                        IsRecommnedPromotionP = ca.IsRecommnedPromotion,
                        // EmpNameP = EmployeeAppraisal != null && EmployeeAppraisal.Employee != null && EmployeeAppraisal.Employee.EmpName != null ? EmployeeAppraisal.Employee.EmpName.FullNameFML : null,
                        EmpNameP = nm.FullNameFML,
                        IsRecommendTrainingP = ca.IsRecommendTraining,

                        YearlyTrainingCalendarDetails_Id = ca.YearlyProgramAssignment.Select(e => e.Id.ToString()).ToArray(),
                        YearlyTrainingCalendarDetails_val = ca.YearlyProgramAssignment.Select(e => e.FullDetails).ToArray(),
                        EmpAppRatingConclusionDetails_Id = ca.EmpAppRatingConclusion.Select(e => e.Id.ToString()).ToArray(),
                        EmpAppRatingConclusionDetails_val = ca.EmpAppRatingConclusion.Select(e => e.FullDetails).ToArray(),
                    });
                }
                var Corp = db.EmpAppEvaluation.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, aa, pst, "", Auth, JsonRequestBehavior.AllowGet });
            }
        }

        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    EmpAppEvaluation Postdetails = db.EmpAppEvaluation
                                                   .Include(e => e.EmpAppRatingConclusion)
                                                   .Include(e => e.YearlyProgramAssignment).Where(e => e.Id == data).SingleOrDefault();

                    if (Postdetails.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = Postdetails.DBTrack.CreatedBy != null ? Postdetails.DBTrack.CreatedBy : null,
                                CreatedOn = Postdetails.DBTrack.CreatedOn != null ? Postdetails.DBTrack.CreatedOn : null,
                                IsModified = Postdetails.DBTrack.IsModified == true ? true : false
                            };
                            Postdetails.DBTrack = dbT;
                            db.Entry(Postdetails).State = System.Data.Entity.EntityState.Modified;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Appraisal/Appraisal", null, db.ChangeTracker, Postdetails.DBTrack);
                            DT_EmpAppEvaluation DT_Post = (DT_EmpAppEvaluation)rtn_Obj;
                            db.Create(DT_Post);

                            await db.SaveChangesAsync();
                            ts.Complete();
                            Msg.Add("  Data removed.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now,
                                CreatedBy = Postdetails.DBTrack.CreatedBy != null ? Postdetails.DBTrack.CreatedBy : null,
                                CreatedOn = Postdetails.DBTrack.CreatedOn != null ? Postdetails.DBTrack.CreatedOn : null,
                                IsModified = Postdetails.DBTrack.IsModified == true ? false : false//,
                            };
                            if (Postdetails.EmpAppRatingConclusion != null)
                            {
                                Postdetails.EmpAppRatingConclusion = null;
                            }
                            db.Entry(Postdetails).State = System.Data.Entity.EntityState.Deleted;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Appraisal/Appraisal", null, db.ChangeTracker, Postdetails.DBTrack);
                            // var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dbT);
                            DT_EmpAppEvaluation DT_Post = (DT_EmpAppEvaluation)rtn_Obj;

                            db.Create(DT_Post);

                            await db.SaveChangesAsync();

                            ts.Complete();
                            Msg.Add("  Data removed.  ");                                                                                             // the original place 
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
                catch (RetryLimitExceededException /* dex */)
                {
                    Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
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

        [HttpPost]
        public async Task<ActionResult> EditSave(EmpAppEvaluation L, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
                try
                {
                    // string Emp = form["Employee-Table"] == null ? null : form["Employee-Table"];
                    string EmpAppRatingConclusion = form["EmpAppRatingConclusionlist"] == "0" ? "" : form["EmpAppRatingConclusionlist"];
                    var IsRecommendDiscussion = form["IsRecommendDiscussion"] == "0" ? "" : form["IsRecommendDiscussion"];
                    var HrRecommendIncrement = form["HrRecommendIncrement"] == "0" ? "" : form["HrRecommendIncrement"];
                    var IsRecommnedPromotion = form["IsRecommnedPromotion"] == "0" ? "" : form["IsRecommnedPromotion"];
                    var IsRecommendTraining = form["IsRecommendTraining"] == "0" ? "" : form["IsRecommendTraining"];
                    var IsTrClose = form["IsTrClose"] == "0" ? "" : form["IsTrClose"];
                    string YearlyTrainingCalendarT = form["YearlyProgramAssignmentlist"] == "0" ? "" : form["YearlyProgramAssignmentlist"];
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;
                    var blog1 = db.EmpAppEvaluation.Where(e => e.Id == data).SingleOrDefault();

                    //List<int> ids = null;
                    //if (Emp != null && Emp != "0" && Emp != "false")
                    //{
                    //    ids = Utility.StringIdsToListIds(Emp);
                    //}
                    //else
                    //{
                    //    List<string> Msgu = new List<string>();
                    //    Msgu.Add("  Kindly select employee  ");
                    //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msgu }, JsonRequestBehavior.AllowGet);
                    //} 

                    blog1.DiscussionSchedule = L.DiscussionSchedule;
                    blog1.HRComments = L.HRComments == null ? "" : L.HRComments;
                    blog1.IncPercent = L.IncPercent;
                    blog1.MaxPoints = L.MaxPoints;
                    blog1.SecurePoints = L.SecurePoints;
                    blog1.ScorePercentage = L.ScorePercentage;
                    blog1.DiscussionNode = L.DiscussionNode == null ? "" : L.DiscussionNode;

                    blog1.EmpAppRatingConclusion = L.EmpAppRatingConclusion;
                    blog1.YearlyProgramAssignment = L.YearlyProgramAssignment;

                    blog1.IsTrClose = L.IsTrClose;
                    blog1.IsRecommendDiscussion = L.IsRecommendDiscussion;
                    blog1.HrRecommendIncrement = L.HrRecommendIncrement;
                    blog1.IsRecommnedPromotion = L.IsRecommnedPromotion;
                    blog1.IsRecommendTraining = L.IsRecommendTraining;



                    List<EmpAppRatingConclusion> SOBJ = new List<EmpAppRatingConclusion>();
                    blog1.EmpAppRatingConclusion = null;
                    if (EmpAppRatingConclusion != null)
                    {
                        var ids = Utility.StringIdsToListIds(EmpAppRatingConclusion);
                        foreach (var ca in ids)
                        {
                            var Lookup_val = db.EmpAppRatingConclusion.Find(ca);
                            SOBJ.Add(Lookup_val);
                            blog1.EmpAppRatingConclusion = SOBJ;
                        }
                    }


                    List<YearlyProgramAssignment> SOBJ1 = new List<YearlyProgramAssignment>();
                    blog1.YearlyProgramAssignment = null;
                    if (YearlyTrainingCalendarT != null)
                    {
                        var ids = Utility.StringIdsToListIds(YearlyTrainingCalendarT);
                        foreach (var ca in ids)
                        {
                            var Lookup_val = db.YearlyProgramAssignment.Find(ca);
                            SOBJ1.Add(Lookup_val);
                            blog1.YearlyProgramAssignment = SOBJ1;
                        }
                    }





                    EmpAppEvaluation pd = null;
                    pd = db.EmpAppEvaluation.Where(e => e.Id == data).SingleOrDefault();
                    List<EmpAppEvaluation> ObjITsection = new List<EmpAppEvaluation>();

                    if (ModelState.IsValid)
                    {
                        try
                        {
                            {
                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    using (var context = new DataBaseContext())
                                        //{
                                        //    blog = context.PostDetails.Where(e => e.Id == data).Include(e => e.FuncStruct)
                                        //                  .Include(e => e.FuncStruct.JobPosition)
                                        //                  .Include(e => e.FuncStruct.Job)
                                        //                  .Include(e => e.ExpFilter)
                                        //                  .Include(e => e.RangeFilter)
                                        //                  .Include(e => e.Qualification)
                                        //                  .Include(e => e.Skill)
                                        //                  .Include(e => e.Gender)
                                        //                  .Include(e => e.MaritalStatus)
                                        //                  .Include(e => e.CategoryPost)
                                        //                  .Include(e => e.CategoryPost.Select(q => q.Category))
                                        //                  .Include(e => e.CategorySplPost)
                                        //                  .Include(e => e.CategorySplPost.Select(q => q.SpecialCategory))
                                        //                            .SingleOrDefault();
                                        //    originalBlogValues = context.Entry(blog).OriginalValues;
                                        //   }

                                        blog1.DBTrack = new DBTrack
                                        {
                                            CreatedBy = blog1.DBTrack.CreatedBy == null ? null : blog1.DBTrack.CreatedBy,
                                            CreatedOn = blog1.DBTrack.CreatedOn == null ? null : blog1.DBTrack.CreatedOn,
                                            Action = "M",
                                            ModifiedBy = SessionManager.UserName,
                                            ModifiedOn = DateTime.Now
                                        };


                                    var CurCorp = db.EmpAppEvaluation.Find(data);
                                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {

                                        EmpAppEvaluation p = new EmpAppEvaluation()
                                        {
                                            DiscussionSchedule = blog1.DiscussionSchedule,
                                            HRComments = blog1.HRComments == null ? "" : blog1.HRComments,
                                            IncPercent = blog1.IncPercent,
                                            MaxPoints = blog1.MaxPoints,
                                            SecurePoints = blog1.SecurePoints,
                                            ScorePercentage = blog1.ScorePercentage,
                                            DiscussionNode = blog1.DiscussionNode == null ? "" : blog1.DiscussionNode,

                                            EmpAppRatingConclusion = blog1.EmpAppRatingConclusion,
                                            YearlyProgramAssignment = blog1.YearlyProgramAssignment,

                                            IsTrClose = blog1.IsTrClose,
                                            IsRecommendDiscussion = blog1.IsRecommendDiscussion,
                                            HrRecommendIncrement = blog1.HrRecommendIncrement,
                                            IsRecommnedPromotion = blog1.IsRecommnedPromotion,
                                            IsRecommendTraining = blog1.IsRecommendTraining,
                                            Id = data,
                                            DBTrack = blog1.DBTrack
                                        };
                                        db.EmpAppEvaluation.Attach(p);
                                        db.Entry(p).State = System.Data.Entity.EntityState.Modified;

                                        db.Entry(p).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                        db.SaveChanges();

                                        await db.SaveChangesAsync();
                                        db.Entry(p).State = System.Data.Entity.EntityState.Detached;

                                        var a = db.EmpAppEvaluation.Include(e => e.EmpAppRatingConclusion).Include(e => e.YearlyProgramAssignment).Where(e => e.Id == data).SingleOrDefault();

                                        ts.Complete();
                                        Msg.Add("  Record Updated");
                                        return Json(new Utility.JsonReturnClass { Id = a.Id, Val = a.SecurePoints.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        // return Json(new Object[] { L.Id, L.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });

                                    }
                                }
                            }
                        }
                        catch (DbUpdateConcurrencyException ex)
                        {
                            var entry = ex.Entries.Single();
                            var clientValues = (AppEvalMethod)entry.Entity;
                            var databaseEntry = entry.GetDatabaseValues();
                            if (databaseEntry == null)
                            {
                                Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                            }
                            else
                            {
                                var databaseValues = (AppEvalMethod)databaseEntry.ToObject();
                                blog1.RowVersion = databaseValues.RowVersion;

                            }
                        }
                        Msg.Add("Record modified by another user.So refresh it and try to save again.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        // return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });

                    }
                    return Json(new Object[] { "", "", "", JsonRequestBehavior.AllowGet });
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

        //public ActionResult GetAppRatingObjectiveLKDetails(string data)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var fall = db.EmpAppRating.Include(a => a.ObjectiveWordings).Where(a => a.ObjectiveWordings != null).ToList();
        //        //   var fall = db.EmpAppRating.Where(b=>b.ObjectiveWordings!=null).Select(b=>b.ObjectiveWordings).ToList();
        //        IEnumerable<EmpAppRating> all;
        //        IEnumerable<EmpAppRating> all1;
        //        if (!string.IsNullOrEmpty(data))
        //        {
        //            all = db.EmpAppRating.Include(a => a.ObjectiveWordings).ToList().Where(d => d.Id.ToString().Contains(data));
        //            all1 = all.Where(a => a.ObjectiveWordings != null).ToList();
        //        }
        //        else
        //        {
        //            var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.ObjectiveWordings.LookupVal.ToString() + "," + ca.RatingPoints }).Distinct();
        //            return Json(r, JsonRequestBehavior.AllowGet);
        //        }
        //        var result = (from c in all1
        //                      select new { c.Id, c.ObjectiveWordings.LookupVal, c.RatingPoints }).Distinct();
        //        return Json(result, JsonRequestBehavior.AllowGet);
        //    }
        //}
        public class appridata
        {
            public string AprId { get; set; }
            public string Aprdatadet { get; set; }
            public string Apprentryby { get; set; }
            
        }
        public ActionResult CheckData(int data, string SchName)
        {
            double TotalMaxPoints = 0;
            double TotalRatingPNTS = 0;

            double getCount = 0;
            List<appridata> return_data = new List<appridata>();
            using (DataBaseContext db = new DataBaseContext())
            {
                var emp = db.EmployeeAppraisal.Include(a => a.EmpAppEvaluation.Select(b => b.EmpAppRatingConclusion.Select(c => c.AppraisalAssistance)))
                    //.Include(a => a.EmpAppEvaluation.Select(b => b.EmpAppRatingConclusion.Select(c => c.AppraisalAssistance.LookupVal)))
                    .Include(e => e.EmpAppEvaluation.Select(b => b.AppraisalSchedule))
                    .Include(e => e.EmpAppEvaluation.Select(b => b.EmpAppRatingConclusion.Select(d => d.EmpAppRating.Select(r => r.AppAssignment))))
                    .Include(a => a.EmpAppEvaluation.Select(b => b.EmpAppRatingConclusion.Select(c => c.EmpAppRating.Select(d => d.ObjectiveWordings))))
                    .Include(e => e.EmpAppEvaluation.Select(b => b.EmpAppRatingConclusion.Select(d => d.EmpAppRating.Select(r => r.AppAssignment.AppSubCategory))))
                    .Include(a => a.Employee).Where(a => a.Employee.Id == data).AsNoTracking().SingleOrDefault();
                var empdt = emp.EmpAppEvaluation.Where(e => e.AppraisalSchedule.BatchName == SchName).Select(a => a.EmpAppRatingConclusion).ToList();
                //foreach (var ca in empdt)
                //{
                //    return_data.Add(new appridata
                //    {
                //        AprId = ca.Select(e => e.Id.ToString()).ToArray(),
                //        Aprdatadet = ca.Select(e => e.FullDetails).ToArray()
                        
                //    });
                //}

                var TEmp = empdt.Select(a => new
                {
                    GetEmpAppConclusion = a.Select(b => new
                    {
                       // entryby =a.Select(z=>z.AppraisalAssistance.LookupVal),
                        entryBy = b.AppraisalAssistance.LookupVal,
                        GetEmpAppRATE = b.EmpAppRating.Select(c => new
                        {
                            GetRatingPNT = c.RatingPoints,
                            AppASSIGNMENT = c.AppAssignment,
                            RatingObjectId = c.ObjectiveWordings.Id.ToString(),
                            RatingObject = c.ObjectiveWordings.LookupVal.ToString() + ", Point: " + c.RatingPoints.ToString(),
                            
                        }).ToList(),

                    }).ToList(),

                }).ToList();




                if (TEmp.Count() > 0)
                {
                    foreach (var item in TEmp)
                    {
                        getCount = item.GetEmpAppConclusion.Select(z => z.GetEmpAppRATE).Count();
                        foreach (var itemEAPC in item.GetEmpAppConclusion)
                        {
                            
                            foreach (var itemEAR in itemEAPC.GetEmpAppRATE)
                            {
                                TotalRatingPNTS += itemEAR.GetRatingPNT;
                                TotalMaxPoints += itemEAR.AppASSIGNMENT.MaxRatingPoints;

                                return_data.Add(new appridata
                                {
                                    AprId = itemEAR.RatingObjectId.ToString(),
                                    Aprdatadet = itemEAR.RatingObject.ToString(),
                                   // Apprentryby = itemEAPC.entryby.ToString(),
                                    Apprentryby = itemEAPC.entryBy.ToString(),
                                });
                            }
                        }
                    }
                }

                double Percentage = 0;
                if (TotalRatingPNTS != 0 && TotalMaxPoints != 0)
                {
                    TotalRatingPNTS = TotalRatingPNTS / getCount;
                    TotalMaxPoints = TotalMaxPoints / getCount;

                    TotalRatingPNTS = Math.Round(TotalRatingPNTS + 0.01, 0);
                    TotalMaxPoints = Math.Round(TotalMaxPoints + 0.01, 0);

                    Percentage = (TotalRatingPNTS / TotalMaxPoints) * 100;
                    Percentage = Math.Round(Percentage, 2);
                }


                if (TotalRatingPNTS > TotalMaxPoints)
                {
                    return Json(new Utility.JsonReturnClass { success = true, responseText = "RatingPoints should not be greater than MaxPoints...!" }, JsonRequestBehavior.AllowGet);
                }


                return this.Json(new Object[] { "", return_data, TotalMaxPoints, TotalRatingPNTS, Percentage, JsonRequestBehavior.AllowGet });
            }
        }
        public ActionResult GridEditData(int data)
        {
            var returnlist = new List<returnDataClass>();
            using (DataBaseContext db = new DataBaseContext())
            {

                if (data != 0)
                {
                    var retrundataList = db.EmpAppEvaluation.Where(e => e.Id == data).SingleOrDefault();
                    //  var returnl = retrundataList.Select(a => a.EmpAppRating).ToList();
                    // var rp = retrundataList.EmpAppRating.Select(b => new { b.RatingPoints, b.Comments }).SingleOrDefault();
                    returnlist.Add(new returnDataClass()
                    {
                        // Appriasalassistance = a.AppraisalAssistance.LookupVal.ToString(),
                        SecurePoints = retrundataList.SecurePoints,
                        MaxPoints = retrundataList.MaxPoints,
                        ScorePercentage = retrundataList.ScorePercentage,
                        DiscussionNote = retrundataList.DiscussionNode,
                        DateDrive = retrundataList.DiscussionSchedule.Value.ToShortDateString(),
                        Percentile = retrundataList.IncPercent,
                        HrRecommendation = retrundataList.IsRecommendDiscussion,
                        HrRecommendedforincrement = retrundataList.HrRecommendIncrement,
                        HrRecommendedforPromotion = retrundataList.IsRecommnedPromotion,
                        HrRecommendedforTraining = retrundataList.IsRecommendTraining,
                        AppraisalCompleted = retrundataList.IsTrClose,
                        Comments = retrundataList.HRComments
                    });

                    return Json(new { returndata = returnlist }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return View();
                }
            }
        }
        public ActionResult GridEditSave(EmpAppEvaluation Empappdata, FormCollection form, string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                int empidMain = 0;
                if (data != null)
                {
                    empidMain = Convert.ToInt32(data);
                }
                var rat = form["RatingPoints"] == null ? "" : form["RatingPoints"];
                var com = form["Comments"] == null ? "" : form["Comments"];
                var IsRecommendDiscussion = form["IsRecommendDiscussion1"] == "0" ? "" : form["IsRecommendDiscussion1"];
                var HrRecommendIncrement = form["HrRecommendIncrement2"] == "0" ? "" : form["HrRecommendIncrement2"];
                var IsRecommnedPromotion = form["IsRecommnedPromotion1"] == "0" ? "" : form["IsRecommnedPromotion1"];
                var IsRecommendTraining = form["IsRecommendTraining1"] == "0" ? "" : form["IsRecommendTraining1"];
                var IsTrClose = form["IsTrClose1"] == "0" ? "" : form["IsTrClose1"];

                // var Appriasalassi = form["Appriasalassistance"] == null ? "" : form["Appriasalassistance"];
                //    var dat = form["data"] == null ? "" : form["data"];
                //  var blog = db.EmpAppRating.Include(a => a.AppAssignment).Where(a => a.Id == empidintrnal).SingleOrDefault();
                Empappdata.IsRecommendDiscussion = Convert.ToBoolean(IsRecommendDiscussion);
                Empappdata.HrRecommendIncrement = Convert.ToBoolean(HrRecommendIncrement);
                Empappdata.IsRecommnedPromotion = Convert.ToBoolean(IsRecommnedPromotion);
                Empappdata.IsRecommendTraining = Convert.ToBoolean(IsRecommendTraining);
                Empappdata.IsTrClose = Convert.ToBoolean(IsTrClose);

                var retrundataList = db.EmpAppEvaluation
                    .Where(a => a.Id == empidMain).SingleOrDefault();
                // var OEmpAppRating = blog.EmpAppRating.(();
                retrundataList.SecurePoints = Empappdata.SecurePoints;
                retrundataList.MaxPoints = Empappdata.MaxPoints;
                retrundataList.ScorePercentage = Empappdata.ScorePercentage;
                retrundataList.DiscussionNode = Empappdata.DiscussionNode;
                retrundataList.DiscussionSchedule = Empappdata.DiscussionSchedule;
                retrundataList.IncPercent = Empappdata.IncPercent;
                retrundataList.IsRecommendDiscussion = Empappdata.IsRecommendDiscussion;
                retrundataList.HrRecommendIncrement = Empappdata.HrRecommendIncrement;
                retrundataList.IsRecommnedPromotion = Empappdata.IsRecommnedPromotion;
                retrundataList.IsRecommendTraining = Empappdata.IsRecommendTraining;
                retrundataList.IsTrClose = Empappdata.IsTrClose;
                retrundataList.HRComments = Empappdata.HRComments;
                try
                {
                    db.EmpAppEvaluation.Attach(retrundataList);
                    db.Entry(retrundataList).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    db.Entry(retrundataList).State = System.Data.Entity.EntityState.Detached;
                    return Json(new { status = true, data = retrundataList, responseText = "Record Updated..!" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {

                    throw e;
                }


            }
        }

        public ActionResult GetyearlytrainingcalendarLKDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.YearlyProgramAssignment.ToList();
                IEnumerable<YearlyProgramAssignment> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.YearlyProgramAssignment.ToList().Where(d => d.Id.ToString().Contains(data));
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

        public class DeserializeClass
        {
            public string Id { get; set; }
            public string EmpEvaluationId { get; set; }
            public string MaxPoints { get; set; }
            public string SecurePoints { get; set; }
            public string Percentile { get; set; }
            public string Comments { get; set; }
            public string ScheduleBatchName { get; set; }
        }
        public class DeserializedClass
        {
            public string Autho_Action { get; set; }
            public string Autho_Allow { get; set; }
            public string employee_table { get; set; }
            public string AppraisalAssistancelist { get; set; }
            public string AssistanceOverallComments { get; set; }


        }
        public class returndatagridclass //Parentgrid
        {
            public string Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
            public string JoiningDate { get; set; }
            public string Job { get; set; }
            public string Grade { get; set; }
            public string Location { get; set; }
        }
        public class returnDataClass
        {

            public double MaxPoints { get; set; }
            public double SecurePoints { get; set; }
            public double Percentile { get; set; }
            public double ScorePercentage { get; set; }
            public string Comments { get; set; }
            public bool HrRecommendation { get; set; }
            public string DateDrive { get; set; }
            public string DiscussionNote { get; set; }
            public bool HrRecommendedforincrement { get; set; }
            public bool HrRecommendedforPromotion { get; set; }
            public bool HrRecommendedforTraining { get; set; }
            public bool AppraisalCompleted { get; set; }

        }


        public ActionResult Get_Employelist(string AppCal, string SchName)
        {
            using (DataBaseContext db = new DataBaseContext())
            {


                List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                List<Employee> data = new List<Employee>();
                var compid = Convert.ToInt32(Session["CompId"].ToString());
                db.Database.CommandTimeout = 3600;

                var empdata = db.EmployeeAppraisal.Include(e => e.Employee).Include(e => e.Employee.EmpName)
                    .Include(e => e.EmpAppEvaluation).Include(e => e.EmpAppEvaluation.Select(r => r.AppraisalSchedule))
                   .AsNoTracking().AsParallel().ToList();

                List<Employee> Emp = new List<Employee>();
                if (SchName != null)
                {


                    foreach (var item in empdata)
                    {
                        EmpAppEvaluation OAppEval = item.EmpAppEvaluation.Where(e => e.AppraisalSchedule.BatchName == SchName).FirstOrDefault();
                        if (OAppEval != null)
                        {
                            Emp.Add(item.Employee);
                        }

                    }
                }



                if (Emp != null && Emp.Count != 0)
                {
                    foreach (var item in Emp)
                    {
                        if (SchName != "")
                        {
                            returndata.Add(new Utility.returndataclass
                            {
                                code = item.Id.ToString(),
                                value = item.FullDetails,
                            });
                        }

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
                    return Json(new Utility.JsonReturnClass { success = false, responseText = "No Record Found OR Data is already assigned for all employees in this batch!" }, JsonRequestBehavior.AllowGet);
                    //return Json("No Record Found", JsonRequestBehavior.AllowGet);
                }
            }
        }

        public ActionResult PopulateDropDownListCalendar(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var qurey = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "APPRAISALCALENDAR" && e.Default == true).ToList();
                //  var qurey = db.Calendar.Include(e=>e.Name).ToList();
                var selected = "";
                if (!string.IsNullOrEmpty(data2))
                {
                    selected = data2;
                }
                var returndata = new SelectList(qurey, "Id", "FullDetails", selected);
                return Json(returndata, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetAppraisalScheduleDetails(List<int> SkipIds)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {


                var fall = db.AppraisalSchedule.GroupBy(e => e.BatchName).Select(e => e.FirstOrDefault()).ToList();

                //var fall = db.AppraisalSchedule.ToList();
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.AppraisalSchedule
                                .Include(e => e.AppraisalPublish)
                                .Include(e => e.AppraisalPeriodCalendar)
                                .Include(e => e.GeoStruct)
                                .Include(e => e.FuncStruct)
                                .Include(e => e.PayStruct)
                                .Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }


                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.BatchName }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);


            }
            // return View();
        }

        public ActionResult Emp_Grid(ParamModel param, string y)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {

                    var all = db.EmployeeAppraisal
                        .Include(e => e.Employee)
                        .Include(e => e.Employee.EmpName)
                        .Include(e => e.Employee.ServiceBookDates)
                        .Include(e => e.Employee.GeoStruct)
                        .Include(e => e.Employee.GeoStruct.Location.LocationObj)
                        .Include(e => e.Employee.FuncStruct)
                        .Include(e => e.Employee.FuncStruct.Job)
                        .Include(e => e.Employee.PayStruct)
                        .Include(e => e.Employee.PayStruct.Grade)
                        .ToList();



                    // for searchs
                    IEnumerable<EmployeeAppraisal> fall;
                    if (param.sSearch == null)
                    {
                        fall = all;
                    }
                    else
                    {
                        fall = all.Where(e => e.Employee.EmpCode.ToUpper() == param.sSearch.ToUpper()).ToList();
                        fall = all.Where(e => (e.Id.ToString().Contains(param.sSearch))
                                  || (e.Employee.EmpCode.ToString().Contains(param.sSearch))
                                  || (e.Employee.EmpName.FullNameFML.ToString().ToUpper().Contains(param.sSearch.ToUpper()))
                                  || (e.Employee.ServiceBookDates.JoiningDate.Value.ToString("dd/MM/yyyy").Contains(param.sSearch))
                                  || (e.Employee.FuncStruct.Job.Name.ToString().ToUpper().Contains(param.sSearch.ToUpper()))
                                  || (e.Employee.PayStruct.Grade.Name.ToString().ToUpper().Contains(param.sSearch.ToUpper()))
                                  || (e.Employee.GeoStruct.Location.LocationObj.ToString().ToUpper().Contains(param.sSearch.ToUpper()))
                                  ).ToList();
                    }

                    //for column sorting
                    var sortindex = Convert.ToInt32(Request["iSortCol_0"]);

                    Func<EmployeeAppraisal, string> orderfunc = (c =>
                                                                Convert.ToUInt32(sortindex) == 0 ? c.Id.ToString() :
                                                                sortindex == 1 ? c.Employee.EmpCode : "");
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
                                Code = item.Employee != null ? item.Employee.EmpCode : null,
                                Name = item.Employee != null && item.Employee.EmpName != null ? item.Employee.EmpName.FullNameFML : null,
                                JoiningDate = item.Employee.ServiceBookDates.JoiningDate != null ? item.Employee.ServiceBookDates.JoiningDate.Value.ToString("dd/MM/yyyy") : null,
                                Job = item.Employee.FuncStruct != null && item.Employee.FuncStruct.Job != null ? item.Employee.FuncStruct.Job.Name : null,
                                Grade = item.Employee.PayStruct != null && item.Employee.PayStruct.Grade != null ? item.Employee.PayStruct.Grade.Name : null,
                                Location = item.Employee.GeoStruct != null && item.Employee.GeoStruct.Location != null && item.Employee.GeoStruct.Location.LocationObj != null ? item.Employee.GeoStruct.Location.LocationObj.LocDesc : null,
                            });
                        }
                        return Json(new
                        {
                            sEcho = param.sEcho,
                            iTotalRecords = all.Count(),
                            iTotalDisplayRecords = fall.Count(),
                            data = result
                        }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        var result = from c in dcompanies

                                     select new[] { null, Convert.ToString(c.Id), c.Employee.EmpCode, };
                        return Json(new
                        {
                            sEcho = param.sEcho,
                            iTotalRecords = all.Count(),
                            iTotalDisplayRecords = fall.Count(),
                            data = result
                        }, JsonRequestBehavior.AllowGet);
                    }//for data reterivation
                }
                catch (Exception e)
                {
                    List<string> Msg = new List<string>();
                    LogFile Logfile = new LogFile();
                    ErrorLog Err = new ErrorLog()
                    {
                        ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                        ExceptionMessage = e.Message,
                        ExceptionStackTrace = e.StackTrace,
                        LineNo = Convert.ToString(new System.Diagnostics.StackTrace(e, true).GetFrame(0).GetFileLineNumber()),
                        LogTime = DateTime.Now
                    };
                    Logfile.CreateLogFile(Err);
                    Msg.Add(e.Message);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public ActionResult Get_AppAssignData(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var db_data = db.EmployeeAppraisal
                        .Include(e => e.EmpAppEvaluation)
                        .Include(e => e.EmpAppEvaluation.Select(t => t.AppraisalSchedule))
                        .Where(e => e.Id == data).AsNoTracking().SingleOrDefault();
                    if (db_data.EmpAppEvaluation != null)
                    {
                        List<DeserializeClass> returndata = new List<DeserializeClass>();

                        foreach (var item in db_data.EmpAppEvaluation)
                        {
                            returndata.Add(new DeserializeClass
                            {
                                Id = item.Id.ToString(),
                                ScheduleBatchName = item.AppraisalSchedule.BatchName,
                                Comments = item.HRComments == null ? "" : item.HRComments.ToString(),
                                MaxPoints = item.MaxPoints == null ? "" : item.MaxPoints.ToString(),
                                SecurePoints = item.SecurePoints == null ? "" : item.SecurePoints.ToString(),
                                Percentile = item.ScorePercentage == null ? "" : item.ScorePercentage.ToString()
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
    }
}