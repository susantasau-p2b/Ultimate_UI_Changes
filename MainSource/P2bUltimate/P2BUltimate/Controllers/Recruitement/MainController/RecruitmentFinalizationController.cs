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
using System.Data.Entity.Core.Objects;
using System.Reflection;
using Recruitment;
using P2BUltimate.Security;
using Training;


namespace P2BUltimate.Controllers.recruitment.MainController
{
    public class RecruitmentFinalizationController : Controller
    {
        // private DataBaseContext db = new DataBaseContext();
        // GET: /BudgetParameters/
        public ActionResult Index()
        {
            return View("~/Views/Recruitement/MainView/RecruitmentFinalization/Index.cshtml");
        }
        //public ActionResult Index()
        //{
        //    return View("~/Views/Shared/Training/_BudgetParameters.cshtml");
        //}


        public ActionResult GetLookup(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.BudgetParameters.ToList();


                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.BudgetParameters.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }
                //var list1 = db.FacultySpecialization.ToList();
                //var list2 = fall.Except(list1);
                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult partial()
        {
            return View("~/Views/Shared/Recruitement/_CandidateFinalizationPartial.cshtml");
        }
        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult Create(ResumeCollection L, FormCollection form)
        {
            List<String> Msg = new List<String>();
            using (DataBaseContext db = new DataBaseContext())
            {
                var IsHREvaluationConfirmation = form["IsHREvaluationConfirmation"] == "0" ? "" : form["IsHREvaluationConfirmation"];
                var IsHRJoiningConfirmation = form["IsHRJoiningConfirmation"] == "0" ? "" : form["IsHRJoiningConfirmation"];
                var IsInductionTraining = form["IsInductionTraining"] == "0" ? "" : form["IsInductionTraining"];
                var IsJoined = form["IsJoined"] == "0" ? "" : form["IsJoined"];
                var IsNotificationToHeads = form["IsNotificationToHeads"] == "0" ? "" : form["IsNotificationToHeads"];
                var IsServiceBookUpdate = form["IsServiceBookUpdate"] == "0" ? "" : form["IsServiceBookUpdate"];
                var Candidatelist = form["Employee-Table"] == null ? "" : form["Employee-Table"];

                L.IsHREvaluationConfirmation = Convert.ToBoolean(IsHREvaluationConfirmation);
                L.IsHRJoiningConfirmation = Convert.ToBoolean(IsHRJoiningConfirmation);
                L.IsInductionTraining = Convert.ToBoolean(IsInductionTraining);
                L.IsJoined = Convert.ToBoolean(IsJoined);
                L.IsNotificationToHeads = Convert.ToBoolean(IsNotificationToHeads);
                L.IsServiceBookUpdate = Convert.ToBoolean(IsServiceBookUpdate);

                List<int> cadidateid = null;
                if (Candidatelist != "")
                {
                    cadidateid = Utility.StringIdsToListIds(Candidatelist);
                }
                bool Auth = form["Autho_Allow"] == "true" ? true : false;

                if (Auth == false)
                {


                    if (ModelState.IsValid)
                    {



                        try
                        {

                            //DbContextTransaction transaction = db.Database.BeginTransaction();

                            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {
                                foreach (var item in cadidateid)
                                {
                                    var db_data = db.ResumeCollection.Include(e => e.Candidate).Where(e => e.Candidate.Id == item)
                                                                .SingleOrDefault();
                                    ResumeCollection blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.ResumeCollection.Include(e => e.Candidate).Where(e => e.Candidate.Id == item)
                                                                .SingleOrDefault();
                                        originalBlogValues = context.Entry(blog).OriginalValues;
                                    }

                                    L.DBTrack = new DBTrack
                                    {
                                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };

                                    db_data.IsHREvaluationConfirmation = L.IsHREvaluationConfirmation;
                                    db_data.IsJoined = L.IsJoined;
                                    db_data.IsHRJoiningConfirmation = L.IsHRJoiningConfirmation;
                                    db_data.IsInductionTraining = L.IsInductionTraining;
                                    db_data.IsServiceBookUpdate = L.IsServiceBookUpdate;
                                    db_data.IsNotificationToHeads = L.IsNotificationToHeads;
                                    db_data.ReasonToFailureEvaluat = L.ReasonToFailureEvaluat;
                                    db_data.ReasonToFailureJoining = L.ReasonToFailureJoining;
                                    db_data.DBTrack = L.DBTrack;

                                    db.ResumeCollection.Attach(db_data);
                                    db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;
                                    //  db.Entry(db_data).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                    //db.SaveChanges();
                                    // var CurCorp = db.ResumeCollection.Find(db_data.Id);
                                    //TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    //db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                    //if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    //{
                                    //    ResumeCollection resumecollection = new ResumeCollection()
                                    //    {
                                    //        IsHREvaluationConfirmation = L.IsHREvaluationConfirmation,
                                    //        IsHRJoiningConfirmation = L.IsHRJoiningConfirmation,
                                    //        IsInductionTraining = L.IsInductionTraining,
                                    //        IsJoined = L.IsJoined,
                                    //        IsNotificationToHeads = L.IsNotificationToHeads,
                                    //        IsServiceBookUpdate = L.IsServiceBookUpdate,
                                    //        ReasonToFailureEvaluat = L.ReasonToFailureEvaluat,
                                    //        ReasonToFailureJoining = L.ReasonToFailureJoining,

                                    //        Id = db_data.Id,
                                    //        DBTrack = L.DBTrack
                                    //    };

                                    //}
                                    // int a = EditS(CreditDatelist, ExcludeLeaveHeadslist, ConvertLeaveHeadBallist, ConvertLeaveHeadlist, data, L, L.DBTrack);
                                    //using (var context = new DataBaseContext())
                                    //{
                                    //    // var obj = DBTrackFile.DBTrackSave("Training/Training", originalBlogValues, db.ChangeTracker, L.DBTrack);
                                    //    // var obj = DBTrackFile.DBTrackSave("Training/Training", originalBlogValues, db.ChangeTracker, L.DBTrack);
                                    //    // DT_BudgetParameters DT_Corp = (DT_BudgetParameters)obj;
                                    //    //DT_Corp.CreditDate_Id = blog.CreditDate == null ? 0 : blog.CreditDate.Id;
                                    //    // db.Create(DT_Corp);
                                    //    db.SaveChanges();
                                    //}
                                }
                                // await db.SaveChangesAsync();
                                ts.Complete();
                                Msg.Add("Record Updated.");
                                return Json(new Utility.JsonReturnClass { Id = L.Id, Val = L.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                            //return Json(new Object[] { L.Id, L.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });


                        }
                        catch (DbUpdateConcurrencyException ex)
                        {
                            var entry = ex.Entries.Single();
                            var clientValues = (BudgetParameters)entry.Entity;
                            var databaseEntry = entry.GetDatabaseValues();
                            if (databaseEntry == null)
                            {
                                return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                            }
                            //else
                            //{
                            //    var databaseValues = (LvEncashReq)databaseEntry.ToObject();
                            //    L.RowVersion = databaseValues.RowVersion;

                            //}
                        }

                        return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                    }
                }
                else
                {
                    //using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    //{

                    //    ResumeCollection blog = null; // to retrieve old data
                    //    DbPropertyValues originalBlogValues = null;
                    //    ResumeCollection Old_Corp = null;

                    //    using (var context = new DataBaseContext())
                    //    {
                    //        blog = context.ResumeCollection.Include(e => e.Candidate).Where(e => e.Id == data).SingleOrDefault();
                    //        originalBlogValues = context.Entry(blog).OriginalValues;
                    //    }
                    //    L.DBTrack = new DBTrack
                    //    {
                    //        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                    //        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                    //        Action = "M",
                    //        IsModified = blog.DBTrack.IsModified == true ? true : false,
                    //        ModifiedBy = SessionManager.UserName,
                    //        ModifiedOn = DateTime.Now
                    //    };

                    //    if (TempData["RowVersion"] == null)
                    //    {
                    //        TempData["RowVersion"] = blog.RowVersion;
                    //    }

                    //    ResumeCollection corp = new ResumeCollection()
                    //    {
                    //        IsHREvaluationConfirmation = L.IsHREvaluationConfirmation,
                    //        IsHRJoiningConfirmation = L.IsHRJoiningConfirmation,
                    //        IsInductionTraining = L.IsInductionTraining,
                    //        IsJoined = L.IsJoined,
                    //        IsNotificationToHeads = L.IsNotificationToHeads,
                    //        IsServiceBookUpdate = L.IsServiceBookUpdate,
                    //        ReasonToFailureEvaluat = L.ReasonToFailureEvaluat,
                    //        ReasonToFailureJoining = L.ReasonToFailureJoining,

                    //        // Id = data,
                    //        DBTrack = L.DBTrack,
                    //        RowVersion = (Byte[])TempData["RowVersion"]
                    //    };


                    //    //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];


                    //    //using (var context = new DataBaseContext())
                    //    //{
                    //    //var obj = DBTrackFile.ModifiedDataHistory("Training/Training", "M", blog, corp, "BudgetParameters", L.DBTrack);
                    //    // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                    //    // Old_Corp = context.ResumeCollection.Where(e => e.Id == data).Include(e => e.Candidate)
                    //    // .SingleOrDefault();
                    //    //DT_BudgetParameters DT_Corp = (DT_BudgetParameters)obj;
                    //    //DT_Corp.CreditDate_Id = blog.CreditDate == null ? 0 : blog.CreditDate.Id;

                    //    //db.Create(DT_Corp);
                    //    //db.SaveChanges();
                    //    //}
                    //    blog.DBTrack = L.DBTrack;
                    //    db.ResumeCollection.Attach(blog);
                    //    db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                    //    db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    //    db.SaveChanges();
                    //    ts.Complete();
                    //    Msg.Add("Record Updated.");
                    //    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //    //return Json(new Object[] { blog.Id, L.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });
                    //}

                }
                return View();
            }
        }

        //public ActionResult Create(ResumeCollection NOBJ, FormCollection form)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        List<String> Msg = new List<String>();

        //        var IsHREvaluationConfirmation = form["IsHREvaluationConfirmation"] == "0" ? "" : form["IsHREvaluationConfirmation"];
        //        var IsHRJoiningConfirmation = form["IsHRJoiningConfirmation"] == "0" ? "" : form["IsHRJoiningConfirmation"];
        //        var IsInductionTraining = form["IsInductionTraining"] == "0" ? "" : form["IsInductionTraining"];
        //        var IsJoined = form["IsJoined"] == "0" ? "" : form["IsJoined"];
        //        var IsNotificationToHeads = form["IsNotificationToHeads"] == "0" ? "" : form["IsNotificationToHeads"];
        //        var IsServiceBookUpdate = form["IsServiceBookUpdate"] == "0" ? "" : form["IsServiceBookUpdate"];

        //        var Candidatelist = form["Employee-Table"] == null ? "" : form["Employee-Table"];

        //        NOBJ.IsHREvaluationConfirmation = Convert.ToBoolean(IsHREvaluationConfirmation);
        //        NOBJ.IsHRJoiningConfirmation = Convert.ToBoolean(IsHRJoiningConfirmation);
        //        NOBJ.IsInductionTraining = Convert.ToBoolean(IsInductionTraining);
        //        NOBJ.IsJoined = Convert.ToBoolean(IsJoined);
        //        NOBJ.IsNotificationToHeads = Convert.ToBoolean(IsNotificationToHeads);
        //        NOBJ.IsServiceBookUpdate = Convert.ToBoolean(IsServiceBookUpdate);

        //        int company_Id = 0;
        //        company_Id = Convert.ToInt32(Session["CompId"]);
        //        var companytraining = new CompanyRecruitment();
        //        companytraining = db.CompanyRecruitment.Where(e => e.Company.Id == company_Id).SingleOrDefault();


        //        int cadidateid = Convert.ToInt32(Candidatelist);


        //        if (ModelState.IsValid)
        //        {
        //            using (TransactionScope ts = new TransactionScope())
        //            {

        //                NOBJ.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName };


        //                var can_id = db.ResumeCollection.Include(e => e.Candidate).Where(e => e.Candidate.Id == cadidateid).SingleOrDefault().Id;
        //                var CurCorp = db.ResumeCollection.Find(can_id);

        //                TempData["CurrRowVersion"] = CurCorp.RowVersion;
        //                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;

        //                ResumeCollection budgetparameters = new ResumeCollection()
        //                {
        //                    IsHREvaluationConfirmation = NOBJ.IsHREvaluationConfirmation,
        //                    IsHRJoiningConfirmation = NOBJ.IsHRJoiningConfirmation,
        //                    IsInductionTraining = NOBJ.IsInductionTraining,
        //                    IsJoined = NOBJ.IsJoined,
        //                    IsNotificationToHeads = NOBJ.IsNotificationToHeads,
        //                    IsServiceBookUpdate = NOBJ.IsServiceBookUpdate,
        //                    ReasonToFailureEvaluat = NOBJ.ReasonToFailureEvaluat,
        //                    ReasonToFailureJoining = NOBJ.ReasonToFailureJoining,
        //                    Id = can_id,
        //                    DBTrack = NOBJ.DBTrack
        //                };

        //                try
        //                {

        //                    db.ResumeCollection.Add(budgetparameters);

        //                    db.SaveChanges();


        //                    ts.Complete();
        //                    // return this.Json(new Object[] { budgetparameters.Id, budgetparameters.FullDetails, "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
        //                    Msg.Add("Data Saved Successfully.");
        //                    return Json(new Utility.JsonReturnClass { Id = budgetparameters.Id, Val = budgetparameters.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                }

        //                catch (DbUpdateConcurrencyException)
        //                {
        //                    return RedirectToAction("Create", new { concurrencyError = true, id = NOBJ.Id });
        //                }
        //                catch (DataException /* dex */)
        //                {
        //                    Msg.Add("Unable to create. Try again, and if the problem persists contact your system administrator.");
        //                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                    // return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
        //                }
        //            }
        //        }
        //        else
        //        {
        //            StringBuilder sb = new StringBuilder("");
        //            foreach (ModelState modelState in ModelState.Values)
        //            {
        //                foreach (ModelError error in modelState.Errors)
        //                {
        //                    sb.Append(error.ErrorMessage);
        //                    sb.Append("." + "\n");
        //                }
        //            }
        //            var errorMsg = sb.ToString();
        //            Msg.Add(errorMsg);
        //            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //            //return this.Json(new { msg = errorMsg });
        //        }
        //    }
        //}

        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.ResumeCollection
                        .Where(e => e.Id == data).Select
                    (e => new
                    {
                        IsHREvaluationConfirmation = e.IsHREvaluationConfirmation,
                        IsHRJoiningConfirmation = e.IsHRJoiningConfirmation,
                        IsInductionTraining = e.IsInductionTraining,
                        IsJoined = e.IsJoined,
                        IsNotificationToHeads = e.IsNotificationToHeads,
                        IsServiceBookUpdate = e.IsServiceBookUpdate,
                        ReasonToFailureEvaluat = e.ReasonToFailureEvaluat,
                        ReasonToFailureJoining = e.ReasonToFailureJoining,

                        Action = e.DBTrack.Action
                    }).ToList();
                var Corp = db.ResumeCollection.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, Auth, JsonRequestBehavior.AllowGet });
            }
        }

        [HttpPost]
        public async Task<ActionResult> EditSave(ResumeCollection L, int data, FormCollection form) // Edit submit
        {
            List<String> Msg = new List<String>();
            using (DataBaseContext db = new DataBaseContext())
            {
                var IsHREvaluationConfirmation = form["IsHREvaluationConfirmation"] == "0" ? "" : form["IsHREvaluationConfirmation"];
                var IsHRJoiningConfirmation = form["IsHRJoiningConfirmation"] == "0" ? "" : form["IsHRJoiningConfirmation"];
                var IsInductionTraining = form["IsInductionTraining"] == "0" ? "" : form["IsInductionTraining"];
                var IsJoined = form["IsJoined"] == "0" ? "" : form["IsJoined"];
                var IsNotificationToHeads = form["IsNotificationToHeads"] == "0" ? "" : form["IsNotificationToHeads"];
                var IsServiceBookUpdate = form["IsServiceBookUpdate"] == "0" ? "" : form["IsServiceBookUpdate"];


                L.IsHREvaluationConfirmation = Convert.ToBoolean(IsHREvaluationConfirmation);
                L.IsHRJoiningConfirmation = Convert.ToBoolean(IsHRJoiningConfirmation);
                L.IsInductionTraining = Convert.ToBoolean(IsInductionTraining);
                L.IsJoined = Convert.ToBoolean(IsJoined);
                L.IsNotificationToHeads = Convert.ToBoolean(IsNotificationToHeads);
                L.IsServiceBookUpdate = Convert.ToBoolean(IsServiceBookUpdate);


                bool Auth = form["Autho_Allow"] == "true" ? true : false;

                if (Auth == false)
                {


                    if (ModelState.IsValid)
                    {
                        try
                        {

                            //DbContextTransaction transaction = db.Database.BeginTransaction();

                            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {
                                ResumeCollection blog = null; // to retrieve old data
                                DbPropertyValues originalBlogValues = null;

                                using (var context = new DataBaseContext())
                                {
                                    blog = context.ResumeCollection.Include(e => e.Candidate).Where(e => e.Id == data)
                                                            .SingleOrDefault();
                                    originalBlogValues = context.Entry(blog).OriginalValues;
                                }

                                L.DBTrack = new DBTrack
                                {
                                    CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                    CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                    Action = "M",
                                    ModifiedBy = SessionManager.UserName,
                                    ModifiedOn = DateTime.Now
                                };


                                var CurCorp = db.ResumeCollection.Find(data);
                                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                {

                                    ResumeCollection resumecollection = new ResumeCollection()
                                    {
                                        IsHREvaluationConfirmation = L.IsHREvaluationConfirmation,
                                        IsHRJoiningConfirmation = L.IsHRJoiningConfirmation,
                                        IsInductionTraining = L.IsInductionTraining,
                                        IsJoined = L.IsJoined,
                                        IsNotificationToHeads = L.IsNotificationToHeads,
                                        IsServiceBookUpdate = L.IsServiceBookUpdate,
                                        ReasonToFailureEvaluat = L.ReasonToFailureEvaluat,
                                        ReasonToFailureJoining = L.ReasonToFailureJoining,

                                        Id = data,
                                        DBTrack = L.DBTrack
                                    };
                                    db.ResumeCollection.Attach(resumecollection);
                                    db.Entry(resumecollection).State = System.Data.Entity.EntityState.Modified;
                                    db.Entry(resumecollection).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                }
                                // int a = EditS(CreditDatelist, ExcludeLeaveHeadslist, ConvertLeaveHeadBallist, ConvertLeaveHeadlist, data, L, L.DBTrack);
                                using (var context = new DataBaseContext())
                                {
                                    // var obj = DBTrackFile.DBTrackSave("Training/Training", originalBlogValues, db.ChangeTracker, L.DBTrack);
                                    // var obj = DBTrackFile.DBTrackSave("Training/Training", originalBlogValues, db.ChangeTracker, L.DBTrack);
                                    // DT_BudgetParameters DT_Corp = (DT_BudgetParameters)obj;
                                    //DT_Corp.CreditDate_Id = blog.CreditDate == null ? 0 : blog.CreditDate.Id;
                                    // db.Create(DT_Corp);
                                    db.SaveChanges();
                                }
                                await db.SaveChangesAsync();
                                ts.Complete();
                                Msg.Add("Record Updated.");
                                return Json(new Utility.JsonReturnClass { Id = L.Id, Val = L.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Object[] { L.Id, L.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });

                            }

                        }
                        catch (DbUpdateConcurrencyException ex)
                        {
                            var entry = ex.Entries.Single();
                            var clientValues = (BudgetParameters)entry.Entity;
                            var databaseEntry = entry.GetDatabaseValues();
                            if (databaseEntry == null)
                            {
                                return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                            }
                            //else
                            //{
                            //    var databaseValues = (LvEncashReq)databaseEntry.ToObject();
                            //    L.RowVersion = databaseValues.RowVersion;

                            //}
                        }

                        return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                    }
                }
                else
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {

                        ResumeCollection blog = null; // to retrieve old data
                        DbPropertyValues originalBlogValues = null;
                        ResumeCollection Old_Corp = null;

                        using (var context = new DataBaseContext())
                        {
                            blog = context.ResumeCollection.Include(e => e.Candidate).Where(e => e.Id == data).SingleOrDefault();
                            originalBlogValues = context.Entry(blog).OriginalValues;
                        }
                        L.DBTrack = new DBTrack
                        {
                            CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                            CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                            Action = "M",
                            IsModified = blog.DBTrack.IsModified == true ? true : false,
                            ModifiedBy = SessionManager.UserName,
                            ModifiedOn = DateTime.Now
                        };

                        if (TempData["RowVersion"] == null)
                        {
                            TempData["RowVersion"] = blog.RowVersion;
                        }

                        ResumeCollection corp = new ResumeCollection()
                        {
                            IsHREvaluationConfirmation = L.IsHREvaluationConfirmation,
                            IsHRJoiningConfirmation = L.IsHRJoiningConfirmation,
                            IsInductionTraining = L.IsInductionTraining,
                            IsJoined = L.IsJoined,
                            IsNotificationToHeads = L.IsNotificationToHeads,
                            IsServiceBookUpdate = L.IsServiceBookUpdate,
                            ReasonToFailureEvaluat = L.ReasonToFailureEvaluat,
                            ReasonToFailureJoining = L.ReasonToFailureJoining,

                            Id = data,
                            DBTrack = L.DBTrack,
                            RowVersion = (Byte[])TempData["RowVersion"]
                        };


                        //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];


                        //using (var context = new DataBaseContext())
                        //{
                        //var obj = DBTrackFile.ModifiedDataHistory("Training/Training", "M", blog, corp, "BudgetParameters", L.DBTrack);
                        // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                        // Old_Corp = context.ResumeCollection.Where(e => e.Id == data).Include(e => e.Candidate)
                        // .SingleOrDefault();
                        //DT_BudgetParameters DT_Corp = (DT_BudgetParameters)obj;
                        //DT_Corp.CreditDate_Id = blog.CreditDate == null ? 0 : blog.CreditDate.Id;

                        //db.Create(DT_Corp);
                        //db.SaveChanges();
                        //}
                        blog.DBTrack = L.DBTrack;
                        db.ResumeCollection.Attach(blog);
                        db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                        db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                        db.SaveChanges();
                        ts.Complete();
                        Msg.Add("Record Updated.");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //return Json(new Object[] { blog.Id, L.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });
                    }

                }
                return View();
            }
        }

        //public class P2BGridData
        //{
        //    public int Id { get; set; }
        //    public bool IsBudgetAppl { get; set; }
        //    public bool IsCategory { get; set; }
        //    public bool IsFuncStruct { get; set; }
        //    public bool IsGeoStruct { get; set; }
        //    public bool IsPayStruct { get; set; }
        //    public bool IsProgram { get; set; }


        //}

        //public ActionResult P2BGrid(P2BGrid_Parameters gp)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        try
        //        {
        //            int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
        //            int pageSize = gp.rows;
        //            int totalPages = 0;
        //            int totalRecords = 0;
        //            var jsonData = (Object)null;

        //            IEnumerable<P2BGridData> salheadList = null;
        //            List<P2BGridData> model = new List<P2BGridData>();
        //            P2BGridData view = null;


        //            //var BindEmpList = db.EmployeePayroll.Include(e => e.Employee.EmpName).Include(e => e.SalaryT)
        //            //    .Where(e => e.SalaryT.Any(u => u.PayMonth != "01/2017")).ToList();
        //            int company_Id = 0;
        //            company_Id = Convert.ToInt32(Session["CompId"]);

        //            var BindCompList = db.CompanyTraining.Include(e => e.Budgetparameters).Where(e => e.Company.Id == company_Id).ToList();

        //            foreach (var z in BindCompList)
        //            {
        //                if (z.Budgetparameters != null)
        //                {

        //                    foreach (var s in z.Budgetparameters)
        //                    {
        //                        //var aa = db.Calendar.Where(e => e.Id == Sal.Id).SingleOrDefault();
        //                        view = new P2BGridData()
        //                        {
        //                            Id = s.Id,
        //                            IsBudgetAppl = s.IsBudgetAppl,
        //                            IsCategory = s.IsCategory,
        //                            IsFuncStruct = s.IsFuncStruct,
        //                            IsGeoStruct = s.IsGeoStruct,
        //                            IsPayStruct = s.IsPayStruct,
        //                            IsProgram = s.IsProgram


        //                        };
        //                        model.Add(view);

        //                    }
        //                }

        //            }

        //            salheadList = model;

        //            IEnumerable<P2BGridData> IE;
        //            if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
        //            {
        //                IE = salheadList;
        //                if (gp.searchOper.Equals("eq"))
        //                {
        //                    jsonData = IE.Where(e => (e.Id.ToString().Contains(gp.searchString))
        //                        || (e.IsBudgetAppl.ToString().Contains(gp.searchString.ToUpper()))
        //                        || (e.IsCategory.ToString().Contains(gp.searchString.ToUpper()))
        //                        || (e.IsFuncStruct.ToString().Contains(gp.searchString))
        //                        || (e.IsGeoStruct.ToString().Contains(gp.searchString))
        //                        || (e.IsPayStruct.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
        //                        || (e.IsProgram.ToString().ToUpper().Contains(gp.searchString.ToUpper()))

        //                        ).Select(a => new Object[] { a.Id, a.IsBudgetAppl, a.IsCategory, a.IsFuncStruct, a.IsGeoStruct, a.IsPayStruct, a.IsProgram }).ToList();
        //                    //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
        //                }
        //                if (pageIndex > 1)
        //                {
        //                    int h = pageIndex * pageSize;
        //                    jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, Convert.ToString(a.IsBudgetAppl), Convert.ToString(a.IsCategory), Convert.ToString(a.IsFuncStruct), Convert.ToString(a.IsGeoStruct), Convert.ToString(a.IsPayStruct), Convert.ToString(a.IsProgram) }).ToList();
        //                }
        //                totalRecords = IE.Count();
        //            }
        //            else
        //            {
        //                IE = salheadList;
        //                Func<P2BGridData, dynamic> orderfuc;
        //                if (gp.sidx == "Id")
        //                {
        //                    orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
        //                }
        //                else
        //                {
        //                    orderfuc = (c => gp.sidx == "IsBudgetAppl" ? c.IsBudgetAppl.ToString() :
        //                                     gp.sidx == "IsCategory" ? c.IsCategory.ToString() : "");
        //                }
        //                if (gp.sord == "asc")
        //                {
        //                    IE = IE.OrderBy(orderfuc);
        //                    jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.IsBudgetAppl), Convert.ToString(a.IsCategory), Convert.ToString(a.IsFuncStruct), Convert.ToString(a.IsGeoStruct), Convert.ToString(a.IsPayStruct), Convert.ToString(a.IsProgram) }).ToList();
        //                }
        //                else if (gp.sord == "desc")
        //                {
        //                    IE = IE.OrderByDescending(orderfuc);
        //                    jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.IsBudgetAppl), Convert.ToString(a.IsCategory), Convert.ToString(a.IsFuncStruct), Convert.ToString(a.IsGeoStruct), Convert.ToString(a.IsPayStruct), Convert.ToString(a.IsProgram) }).ToList();
        //                }
        //                if (pageIndex > 1)
        //                {
        //                    int h = pageIndex * pageSize;
        //                    jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, Convert.ToString(a.IsBudgetAppl), Convert.ToString(a.IsCategory), Convert.ToString(a.IsFuncStruct), Convert.ToString(a.IsGeoStruct), Convert.ToString(a.IsPayStruct), Convert.ToString(a.IsProgram) }).ToList();
        //                }
        //                totalRecords = salheadList.Count();
        //            }
        //            if (totalRecords > 0)
        //            {
        //                totalPages = (int)Math.Ceiling((float)totalRecords / (float)gp.rows);
        //            }
        //            if (gp.page > totalPages)
        //            {
        //                gp.page = totalPages;
        //            }
        //            var JsonData = new
        //            {
        //                page = gp.page,
        //                rows = jsonData,
        //                records = totalRecords,
        //                total = totalPages
        //            };
        //            return Json(JsonData, JsonRequestBehavior.AllowGet);
        //        }
        //        catch (Exception ex)
        //        {
        //            throw ex;

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
                if (session != null)
                {
                    Batchids = Utility.StringIdsToListIds(session);
                }
                if (databatch != null)
                {
                    evalparaidlist = Utility.StringIdsToListIds(databatch);
                }
                Batchwiseempdata = db.RecruitBatchInitiator
                                 .Include(e => e.ResumeCollection)
                                 .Include(e => e.ResumeCollection.Select(t => t.Candidate))
                                 .Include(e => e.ResumeCollection.Select(t => t.Candidate.CanName))
                                 .Include(e => e.ResumeCollection.Select(t => t.ResumeSortlistingStatus))
                                 .Include(e => e.ResumeCollection.Select(t => t.ShortlistingCriteria))
                                 .Include(e => e.ResumeCollection.Select(t => t.HREvaluationStatus))
                                 .Include(e => e.ResumeCollection.Select(t => t.HRJoiningStatus))
                                 .Include(e => e.ResumeCollection.Select(t => t.RecruitEvaluationProcessResult.Select(m => m.RecruitEvaluationPara)))
                                 .Include(e => e.ResumeCollection.Select(t => t.RecruitEvaluationProcessResult.Select(m => m.ActivityResult)))
                                 .Where(e => Batchids.Contains(e.Id)).SingleOrDefault();
                int stages;
                //if (session != null)
                //{
                //    evalparaidlist = Utility.StringIdsToListIds(session);
                //}

                //var recruitevalparaid = db.RecruitEvaluationPara.Include(t => t.RecruitEvalPara).Where(e => evalparaidlist.Contains(e.Id)).SingleOrDefault();
                //stages = recruitevalparaid.Stage - 1;

                //if (stages == 0)
                //{
                empdata = Batchwiseempdata.ResumeCollection
                    .Where(e => e.HREvaluationStatus != null && e.HRJoiningStatus != null && e.HREvaluationStatus.LookupVal.ToString().ToUpper() == "SELECTED" && e.HRJoiningStatus.LookupVal.ToString().ToUpper() == "SELECTED")
                    .ToList();

                //if (databatch != null)
                //{
                //    foreach (var item in empdata)
                //    {
                //        Emp.Add(item.Candidate);
                //    }
                //}

                //if (recruitevalparaid != null)
                //{

                foreach (var item in empdata)
                {
                    // var stagequlify = item.RecruitEvaluationProcessResult.Where(t => t.RecruitEvaluationPara != null && t.ActivityResult.LookupVal.ToString().ToUpper() == "PASS").ToList();

                    if (item.Candidate != null)
                    {
                        Emp.Add(item.Candidate);
                    }

                }
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
                    return Json(new Utility.JsonReturnClass { success = false, responseText = "No Candidate Found !" }, JsonRequestBehavior.AllowGet);
                    //return Json("No Record Found", JsonRequestBehavior.AllowGet);
                }
                //}
                //else
                //{
                //    empdata = db.ResumeCollection
                //        .Include(e => e.Candidate)
                //        .Include(e => e.RecruitEvaluationProcessResult)
                //        .Include(e => e.RecruitEvaluationProcessResult.Select(t => t.RecruitEvaluationPara))
                //        .Include(e => e.RecruitEvaluationProcessResult.Select(t => t.ActivityResult))
                //        .Include(e => e.ResumeSortlistingStatus)
                //        .Include(e => e.Candidate.CanName)
                //        .Include(e => e.ShortlistingCriteria)
                //        .Where(e => e.ResumeSortlistingStatus.LookupVal.ToString().ToUpper() == "SELECTED")
                //        .ToList();
                //    if (recruitevalparaid != null)
                //    {

                //        foreach (var item in empdata)
                //        {
                //            var stagequlify = item.RecruitEvaluationProcessResult.Where(t => t.RecruitEvaluationPara != null && t.RecruitEvaluationPara.Stage == stages && t.ActivityResult.LookupVal.ToString().ToUpper() == "PASS").ToList();


                //            if (stagequlify.Count() > 0)
                //            {
                //                var currentstagepass = item.RecruitEvaluationProcessResult.Where(t => t.RecruitEvaluationPara.Stage == recruitevalparaid.Stage && t.ActivityResult.LookupVal.ToString().ToUpper() == "PASS").ToList();
                //                if (currentstagepass.Count() != 1)
                //                {
                //                    Emp.Add(item.Candidate);

                //                }
                //            }

                //        }
                //    }

                //    //if (databatch != null)
                //    //{
                //    //    foreach (var item in empdata)
                //    //    {
                //    //        Emp.Add(item.Candidate);
                //    //    }
                //    //}


                //    List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                //    if (Emp != null && Emp.Count != 0)
                //    {
                //        foreach (var item in Emp)
                //        {
                //            returndata.Add(new Utility.returndataclass
                //            {
                //                code = item.Id.ToString(),
                //                value = item.FullDetails,
                //            });
                //        }

                //        var returnjson = new
                //        {
                //            data = returndata,
                //            tablename = "Employee-Table"
                //        };
                //        return Json(new List<Object> { returnjson }, JsonRequestBehavior.AllowGet);
                //    }
                //    else
                //    {
                //        return Json(new Utility.JsonReturnClass { success = false, responseText = "No Candidate Found !For Stage:" + recruitevalparaid.Stage + ":" + recruitevalparaid.RecruitEvalPara.LookupVal }, JsonRequestBehavior.AllowGet);
                //        //return Json("No Record Found", JsonRequestBehavior.AllowGet);
                //    }
                //}

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
                            .Include(e => e.ResumeCollection.Select(t => t.HREvaluationStatus))
                            .Include(e => e.ResumeCollection.Select(t => t.ResumeSortlistingStatus))
                            .Where(e => e.Id == batchid).SingleOrDefault();

                        Candidatedata = batchwiseemp.ResumeCollection.Where(e => e.HREvaluationStatus != null && e.HRJoiningStatus != null && e.HREvaluationStatus.LookupVal.ToString().ToUpper() == "SELECTED" && e.HRJoiningStatus.LookupVal.ToString().ToUpper() == "SELECTED").ToList();
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


                            result.Add(new returndatagridclass
                            {
                                Id = item.Id.ToString(),
                                CandidateName = item.Candidate == null ? "" : item.Candidate.CanName == null ? "" : item.Candidate.CanName.FullNameFML,
                                Fulldetails = item.FullDetails,
                                HrEvaluationStatus = item.HREvaluationStatus == null ? "" : item.HREvaluationStatus.LookupVal,
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
        public class FormChildDataClass //"ReasonToFailureEvaluat", "ReasonToFailureJoining", "IsHREvaluationConfirmation", "IsHRJoiningConfirmation", "IsInductionTraining", "IsJoined"],
        {
            public int Id { get; set; }
            public string ReasonToFailureEvaluat { get; set; }
            public string ReasonToFailureJoining { get; set; }
            public bool IsHREvaluationConfirmation { get; set; }
            public bool IsHRJoiningConfirmation { get; set; }
            public bool IsInductionTraining { get; set; }
            public bool IsJoined { get; set; }
        }
        public ActionResult Get_FormulaStructDetails(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    int id = Convert.ToInt32(Session["CompId"]);


                    var db_data = db.ResumeCollection
                        .Where(e => e.Id == data).SingleOrDefault();

                    if (db_data != null)
                    {
                        List<FormChildDataClass> returndata = new List<FormChildDataClass>();


                        returndata.Add(new FormChildDataClass
                        {
                            Id = db_data.Id,
                            ReasonToFailureEvaluat = db_data.ReasonToFailureEvaluat,
                            ReasonToFailureJoining = db_data.ReasonToFailureJoining,
                            IsHREvaluationConfirmation = db_data.IsHREvaluationConfirmation,
                            IsHRJoiningConfirmation = db_data.IsHRJoiningConfirmation,
                            IsInductionTraining = db_data.IsInductionTraining,
                            IsJoined = db_data.IsJoined,
                        });
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
                var Q = db.ResumeCollection
                 .Where(e => e.Id == data).AsEnumerable().Select
                 (c => new
                 {
                     ReasonToFailureEvaluat = c.ReasonToFailureEvaluat,
                     ReasonToFailureJoining = c.ReasonToFailureJoining,
                     IsHREvaluationConfirmation = c.IsHREvaluationConfirmation,
                     IsHRJoiningConfirmation = c.IsHRJoiningConfirmation,
                     IsInductionTraining = c.IsInductionTraining,
                     IsJoined = c.IsJoined,
                     IsServiceBookUpdate = c.IsServiceBookUpdate,
                     IsNotificationToHeads = c.IsNotificationToHeads
                 }).SingleOrDefault();
                return Json(new { data = Q }, JsonRequestBehavior.AllowGet);
            }
        }
        List<String> Msg = new List<String>();
        public ActionResult GridEditSave(ResumeCollection L, FormCollection form, string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                // string ContactDetails = form["FacultySpecializationlist"] == "0" ? "" : form["FacultySpecializationlist"];
                //  bool Auth = form["Autho_Action"] == "" ? false : true;
                bool Auth = form["Autho_Allow"] == "true" ? true : false;
                int id = Convert.ToInt32(data);
                var db_data = db.ResumeCollection
                        .Where(e => e.Id == id).SingleOrDefault();

                var IsHREvaluationConfirmation = form["IsHREvaluationConfirmation"] == "0" ? "" : form["IsHREvaluationConfirmation"];
                var IsHRJoiningConfirmation = form["IsHRJoiningConfirmation"] == "0" ? "" : form["IsHRJoiningConfirmation"];
                var IsInductionTraining = form["IsInductionTraining"] == "0" ? "" : form["IsInductionTraining"];
                var IsJoined = form["IsJoined"] == "0" ? "" : form["IsJoined"];
                var IsNotificationToHeads = form["IsNotificationToHeads"] == "0" ? "" : form["IsNotificationToHeads"];
                var IsServiceBookUpdate = form["IsServiceBookUpdate"] == "0" ? "" : form["IsServiceBookUpdate"];
                var Candidatelist = form["Employee-Table"] == null ? "" : form["Employee-Table"];

                L.IsHREvaluationConfirmation = Convert.ToBoolean(IsHREvaluationConfirmation);
                L.IsHRJoiningConfirmation = Convert.ToBoolean(IsHRJoiningConfirmation);
                L.IsInductionTraining = Convert.ToBoolean(IsInductionTraining);
                L.IsJoined = Convert.ToBoolean(IsJoined);
                L.IsNotificationToHeads = Convert.ToBoolean(IsNotificationToHeads);
                L.IsServiceBookUpdate = Convert.ToBoolean(IsServiceBookUpdate);


                db_data.IsHREvaluationConfirmation = L.IsHREvaluationConfirmation;
                db_data.IsJoined = L.IsJoined;
                db_data.IsHRJoiningConfirmation = L.IsHRJoiningConfirmation;
                db_data.IsInductionTraining = L.IsInductionTraining;
                db_data.IsServiceBookUpdate = L.IsServiceBookUpdate;
                db_data.IsNotificationToHeads = L.IsNotificationToHeads;
                db_data.ReasonToFailureEvaluat = L.ReasonToFailureEvaluat;
                db_data.ReasonToFailureJoining = L.ReasonToFailureJoining;

                try
                {
                    db.ResumeCollection.Attach(db_data);
                    db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;
                    return Json(new { status = true, data = db_data, responseText = "Record Updated..!" }, JsonRequestBehavior.AllowGet);



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
        //public ActionResult Edit(int data)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        List<City_Info> return_data = new List<City_Info>();
        //        var Q = db.ResumeCollection
        //            .Include(e => e.Candidate)
        //            .Include(e => e.HREvaluationStatus)
        //            .Include(e => e.Candidate.CanName)
        //         .Where(e => e.Id == data).AsEnumerable().Select
        //         (c => new
        //         {
        //             CityTypeper = c.Candidate.FullDetails,
        //             hrevaluationid = c.HREvaluationStatus == null ? "" : c.HREvaluationStatus.Id.ToString()
        //         }).ToList();

        //        //var Citydetails = db.HRAExemptionMaster.Include(e => e.City).Where(e => e.Id == data).Select(e => e.City).ToList();
        //        //if (Citydetails != null && Citydetails.Count > 0)
        //        //{
        //        //    foreach (var ca in Citydetails)
        //        //    {
        //        //        return_data.Add(new City_Info
        //        //        {
        //        //            Cityid = ca.Select(e => e.Id).ToArray(),
        //        //            CityFulldetails = ca.Select(e => e.FullDetails).ToArray()

        //        //        });

        //        //    }

        //        //}
        //        return Json(new Object[] { Q, return_data, JsonRequestBehavior.AllowGet });
        //    }
        //}
        //public ActionResult EditSave(ResumeCollection c, int data, FormCollection form)
        //{
        //    List<string> Msg = new List<string>();
        //    var Hrevaluationstatus = form["CategorylistEvalpara"] == "0" ? "" : form["CategorylistEvalpara"];


        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        if (Hrevaluationstatus != null && Hrevaluationstatus != "")
        //        {
        //            var val = db.LookupValue.Find(int.Parse(Hrevaluationstatus));
        //            c.HREvaluationStatus = val;

        //            var add = db.ResumeCollection.Include(e => e.HREvaluationStatus).Where(e => e.Id == data).SingleOrDefault();
        //            IList<ResumeCollection> contactsdetails = null;
        //            if (add.ShortlistingCriteria != null)
        //            {
        //                contactsdetails = db.ResumeCollection.Include(t => t.HREvaluationStatus).Where(x => x.Id == data).ToList();
        //            }
        //            else
        //            {
        //                contactsdetails = db.ResumeCollection.Where(x => x.Id == data).ToList();
        //            }
        //            foreach (var s in contactsdetails)
        //            {
        //                s.HREvaluationStatus = c.HREvaluationStatus;
        //                db.ResumeCollection.Attach(s);
        //                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                //await db.SaveChangesAsync();
        //                db.SaveChanges();
        //                TempData["RowVersion"] = s.RowVersion;
        //                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //            }
        //        }
        //        else
        //        {
        //            var contactsdetails = db.ResumeCollection.Include(e => e.HREvaluationStatus).Where(x => x.Id == data).ToList();
        //            foreach (var s in contactsdetails)
        //            {
        //                s.HREvaluationStatus = null;
        //                db.ResumeCollection.Attach(s);
        //                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                //await db.SaveChangesAsync();
        //                db.SaveChanges();
        //                TempData["RowVersion"] = s.RowVersion;
        //                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //            }
        //        }
        //        try
        //        {
        //            using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
        //                      new System.TimeSpan(0, 30, 0)))
        //            {
        //                ResumeCollection blog = null;
        //                DbPropertyValues originalBlogValues = null;
        //                using (var context = new DataBaseContext())
        //                {
        //                    blog = context.ResumeCollection.Include(e => e.Candidate)
        //                        .Where(e => e.Id == data).SingleOrDefault();
        //                    originalBlogValues = context.Entry(blog).OriginalValues;
        //                }
        //                c.DBTrack = new DBTrack
        //                {
        //                    CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                    CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                    Action = "M",
        //                    IsModified = blog.DBTrack.IsModified == true ? true : false,
        //                    ModifiedBy = SessionManager.UserName,
        //                    ModifiedOn = DateTime.Now
        //                };

        //                var CurOBJ = db.ResumeCollection.Where(e => e.Id == data).SingleOrDefault();
        //                TempData["CurrRowVersion"] = CurOBJ.RowVersion;
        //                db.Entry(CurOBJ).State = System.Data.Entity.EntityState.Detached;
        //                ResumeCollection rc = new ResumeCollection()
        //                {
        //                    Id = CurOBJ.Id,
        //                    DBTrack = c.DBTrack,
        //                };

        //                db.ResumeCollection.Attach(rc);
        //                db.Entry(rc).State = System.Data.Entity.EntityState.Modified;
        //                db.Entry(rc).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //                db.SaveChanges();
        //                ts.Complete();
        //                Msg.Add(" Record Updated Successfully ");
        //                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //            }

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
        //        return View();
        //    }
        //}
    }
}