using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using System.Transactions;
using P2b.Global;
using P2BUltimate.Security;
using System.Text;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Threading.Tasks;
using Recruitment;
namespace P2BUltimate.Controllers

    .Recruitment.MainController
{
    public class RecruitJoinParaProcessResultController : Controller
    {
        List<String> Msg = new List<String>();

        //  private DataBaseContext db = new DataBaseContext();
        // 
        // GET: /RecruitEvaluationProcess/
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Partial()
        {
            return View("~/Views/Shared/Recruitement/_RecruitJoinParaProcessResult.cshtml");
        }

        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult Create(RecruitJoinParaProcessResult c, FormCollection form) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var ActivityAccepted = form["ActivityAcceptedAppl"] == null ? "" : form["ActivityAcceptedAppl"];
                var ActivityLetterIssue = form["ActivityLetterIssueAppl"] == null ? "" : form["ActivityLetterIssueAppl"];

                c.ActivityAccepted = Convert.ToBoolean(ActivityAccepted);
                c.ActivityLetterIssue = Convert.ToBoolean(ActivityLetterIssue);

                string RecruitJoiningPara = form["RecruitJoiningParalist"] == null ? "" : form["RecruitJoiningParalist"];

                if (RecruitJoiningPara != "" && RecruitJoiningPara != null)
                {
                    var val = db.RecruitJoiningPara.Find(int.Parse(RecruitJoiningPara));
                    c.RecruitJoiningPara = val;
                }
                try
                {

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {


                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            RecruitJoinParaProcessResult corporate = new RecruitJoinParaProcessResult()
                            {
                                ActivityAccepted = c.ActivityAccepted,
                                ActivityAcceptedDate = c.ActivityAcceptedDate,
                                ActivityDate = c.ActivityDate,
                                ActivityLetterIssue = c.ActivityLetterIssue,
                                RecruitJoiningPara = c.RecruitJoiningPara,
                                DBTrack = c.DBTrack
                            };

                            db.RecruitJoinParaProcessResult.Add(corporate);

                            //var rtn_Obj = DBTrackFile.DBTrackSave("Recruitement/Recruitement", null, db.ChangeTracker, c.DBTrack);
                            //DT_RecruitEvaluationProcessResult DT_Corp = (DT_RecruitEvaluationProcessResult)rtn_Obj;
                            //DT_Corp.J = c.Address == null ? 0 : c.Address.Id;
                            //DT_Corp.BusinessType_Id = c.BusinessType == null ? 0 : c.BusinessType.Id;
                            //DT_Corp.ContactDetails_Id = c.ContactDetails == null ? 0 : c.ContactDetails.Id;
                            //  db.Create(DT_Corp);
                            db.SaveChanges();
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", "C", corporate, null, "Corporate", null);


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
        }

        public class RecruitJoiningPara
        {
            public Array RJ_id { get; set; }
            public string RJ_val { get; set; }
        }
        //public class RecruitJoinPara
        //{
        //    public Array RJ_id { get; set; }
        //    public Array RJ_val { get; set; }
        //}
        [HttpPost]
        public ActionResult Edit(int data)
        {
            //string tableName = "Corporate";

            //    // Fetch the table records dynamically
            //    var tableData = db.GetType()
            //    .GetProperty(tableName)
            //    .GetValue(db, null);
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.RecruitJoinParaProcessResult
                     .Include(e => e.RecruitJoiningPara)


            .Where(e => e.Id == data).Select
            (e => new
            {
                ActivityAccepted = e.ActivityAccepted,
                ActivityAcceptedDate = e.ActivityAcceptedDate,
                ActivityDate = e.ActivityDate,
                ActivityLetterIssue = e.ActivityLetterIssue,
                Action = e.DBTrack.Action
            }).ToList();


                List<RecruitJoiningPara> return_data = new List<RecruitJoiningPara>();


                var a = db.RecruitJoinParaProcessResult.Include(e => e.RecruitJoiningPara).Where(e => e.Id == data).Select(e => e.RecruitJoiningPara).ToList();

                foreach (var ca in a)
                {
                    return_data.Add(
                new RecruitJoiningPara
                {
                    RJ_id = ca.Id.ToString().ToArray(),
                    RJ_val = ca.FullDetails
                });
                }



                //List<RecruitJoinPara> return_data2 = new List<RecruitJoinPara>();


                //var a1 = db.RecruitEvaluationProcess.Include(e => e.RecruitJoiningPara).Where(e => e.Id == data).Select(e => e.RecruitJoiningPara).ToList();

                //foreach (var ca in a1)
                //{
                //    return_data2.Add(
                //new RecruitJoinPara
                //{
                //    RJ_id = ca.Select(e => e.Id.ToString()).ToArray(),
                //    RJ_val = ca.Select(e => e.FullDetails).ToArray()
                //});
                //}



                var Corp = db.RecruitJoinParaProcessResult.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, return_data, Auth, JsonRequestBehavior.AllowGet });
            }
        }


        //public async Task<ActionResult> EditSave(RecruitJoinParaProcessResult c, int data, FormCollection form) // Edit submit
        //{

        //    // string ContactDetails = form["FacultySpecializationlist"] == "0" ? "" : form["FacultySpecializationlist"];
        //    //  bool Auth = form["Autho_Action"] == "" ? false : true;
        //    bool Auth = form["Autho_Allow"] == "true" ? true : false;
        //    string RecruitJoiningPara = form["RecruitJoiningParalist"] == null ? "" : form["RecruitJoiningParalist"];

        //    var db_Data = db.RecruitJoinParaProcessResult
        //        .Include(e => e.RecruitJoiningPara)
        //        .Where(e => e.Id == data).SingleOrDefault();

        //    //db_Data.RecruitEvaluationPara = null;
        //   // db_Data.RecruitJoiningPara = null;


        //    if (RecruitJoiningPara != null)
        //    {
        //        if (RecruitJoiningPara != "")
        //        {
        //            int ConId = Convert.ToInt32(RecruitJoiningPara);
        //            var val = db.RecruitJoiningPara
        //                .Include(e => e.RecruitJoinPara)
        //                .Where(e => e.Id == ConId).SingleOrDefault();
        //            c.RecruitJoiningPara = val;

        //        }

        //    }



        //    //List<RecruitJoiningPara> jobinside = new List<RecruitJoiningPara>();
        //    //string j_inside = form["JobInsideOrglist"];

        //    //if (j_inside != null)
        //    //{
        //    //    var ids = Utility.StringIdsToListIds(j_inside);
        //    //    foreach (var ca in ids)
        //    //    {
        //    //        var Lookup_val = db.RecruitJoiningPara.Find(ca);

        //    //        jobinside.Add(Lookup_val);
        //    //        db_Data.RecruitJoiningPara = jobinside;
        //    //    }
        //    //}
        //    //else
        //    //{
        //    //    db_Data.RecruitJoiningPara = null;
        //    //}

        //    if (Auth == false)
        //    {
        //        if (ModelState.IsValid)
        //        {
        //            try
        //            {

        //                //DbContextTransaction transaction = db.Database.BeginTransaction();

        //                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                {
        //                    using (var context = new DataBaseContext())
        //                    {
        //                        RecruitJoinParaProcessResult corporate = new RecruitJoinParaProcessResult()
        //                        {
        //                            ActivityAccepted = c.ActivityAccepted,
        //                            ActivityAcceptedDate = c.ActivityAcceptedDate,
        //                            ActivityDate = c.ActivityDate,
        //                            ActivityLetterIssue = c.ActivityLetterIssue,
        //                            RecruitJoiningPara = c.RecruitJoiningPara,
        //                            DBTrack = c.DBTrack
        //                        };



        //                        db.RecruitJoinParaProcessResult.Attach(corporate);
        //                        db.Entry(corporate).State = System.Data.Entity.EntityState.Modified;
        //                        db.SaveChanges();
        //                        TempData["RowVersion"] = db_Data.RowVersion;
        //                        db.Entry(db_Data).State = System.Data.Entity.EntityState.Detached;

        //                        var Curr_OBJ = db.RecruitJoinParaProcessResult.Find(data);
        //                        TempData["CurrRowVersion"] = Curr_OBJ.RowVersion;
        //                        db.Entry(Curr_OBJ).State = System.Data.Entity.EntityState.Detached;

        //                        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
        //                        {
        //                            RecruitJoinParaProcessResult blog = null; // to retrieve old data
        //                            DbPropertyValues originalBlogValues = null;


        //                            blog = context.RecruitJoinParaProcessResult.Where(e => e.Id == data).Include(e => e.RecruitJoiningPara)
        //                                                   .SingleOrDefault();
        //                            originalBlogValues = context.Entry(blog).OriginalValues;


        //                            c.DBTrack = new DBTrack
        //                            {
        //                                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                                Action = "M",
        //                                ModifiedBy = SessionManager.UserName,
        //                                ModifiedOn = DateTime.Now
        //                            };

        //                            //RecruitEvaluationProcess lk = new RecruitEvaluationProcess
        //                            //{
        //                            //    Id = data,

        //                            //    RecruitEvaluationPara = db_Data.RecruitEvaluationPara

        //                            //    DBTrack = c.DBTrack
        //                            //};


        //                            //db.RecruitEvaluationProcessResult.Attach(lk);
        //                            //db.Entry(lk).State = System.Data.Entity.EntityState.Modified;

        //                            //// db.Entry(c.DBTrack).State = System.Data.Entity.EntityState.Modified;
        //                            ////db.SaveChanges();
        //                            //db.Entry(lk).OriginalValues["RowVersion"] = TempData["RowVersion"];

        //                            var obj = DBTrackFile.DBTrackSave("Recruitement/Recruitement", originalBlogValues, db.ChangeTracker, c.DBTrack);
        //                            //DT_JobSource DT_LK = (DT_JobSource)obj;
        //                            //  DT_LK.Allergy = lk.Allergy.Select(e => e.Id);
        //                            //db.Create(DT_LK);
        //                            db.SaveChanges();
        //                            await db.SaveChangesAsync();
        //                            //db.Entry(lk).State = System.Data.Entity.EntityState.Detached;

        //                            var aaq = db.RecruitJoinParaProcessResult.Include(e => e.RecruitJoiningPara).Where(e => e.Id == data).SingleOrDefault();
        //                            ts.Complete();
        //                            Msg.Add("Record Updated Successfully.");
        //                            return Json(new Utility.JsonReturnClass { Id = aaq.Id, Val = aaq.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                        }
        //                    }
        //                }
        //            }
        //            catch (DbUpdateConcurrencyException ex)
        //            {
        //                var entry = ex.Entries.Single();
        //                var clientValues = (JobSource)entry.Entity;
        //                var databaseEntry = entry.GetDatabaseValues();
        //                if (databaseEntry == null)
        //                {
        //                    return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
        //                }
        //                else
        //                {
        //                    var databaseValues = (JobSource)databaseEntry.ToObject();
        //                    c.RowVersion = databaseValues.RowVersion;

        //                }
        //            }

        //            return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
        //        }
        //    }
        //    else
        //    {
        //        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //        {

        //            RecruitJoinParaProcessResult blog = null; // to retrieve old data
        //            DbPropertyValues originalBlogValues = null;
        //            RecruitJoinParaProcessResult Old_Corp = null;

        //            using (var context = new DataBaseContext())
        //            {
        //                blog = context.RecruitJoinParaProcessResult.Where(e => e.Id == data).SingleOrDefault();
        //                originalBlogValues = context.Entry(blog).OriginalValues;
        //            }
        //            c.DBTrack = new DBTrack
        //            {
        //                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                Action = "M",
        //                IsModified = blog.DBTrack.IsModified == true ? true : false,
        //                ModifiedBy = SessionManager.UserName,
        //                ModifiedOn = DateTime.Now
        //            };

        //            if (TempData["RowVersion"] == null)
        //            {
        //                TempData["RowVersion"] = blog.RowVersion;
        //            }

        //            JobSource corp = new JobSource()
        //            {

        //                Id = data,
        //                DBTrack = c.DBTrack,
        //                RowVersion = (Byte[])TempData["RowVersion"]
        //            };


        //            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

        //            using (var context = new DataBaseContext())
        //            {
        //                var obj = DBTrackFile.ModifiedDataHistory("Recruitement/Recruitement", "M", blog, corp, "RecruitEvaluationProcess", c.DBTrack);
        //                // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

        //                Old_Corp = context.RecruitJoinParaProcessResult.Where(e => e.Id == data).Include(e => e.RecruitJoiningPara)
        //                  .SingleOrDefault();
        //                //DT_JobSource DT_Corp = (DT_JobSource)obj;
        //                //  DT_Corp.TrainingInstitue_Id = DBTrackFile.ValCompare(Old_Corp.TrainingInstitue, c.TrainingInstitue);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
        //                // DT_Corp.FacultyType_Id = DBTrackFile.ValCompare(Old_Corp.FacultyType, c.FacultyType); //Old_Corp.BusinessType == c.BusinessType ? 0 : Old_Corp.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_Corp.BusinessType.Id;
        //                //DT_Corp.ContactDetails_Id = DBTrackFile.ValCompare(Old_Corp.ContactDetails, c.ContactDetails); //Old_Corp.ContactDetails == c.ContactDetails ? 0 : Old_Corp.ContactDetails == null && c.ContactDetails != null ? c.ContactDetails.Id : Old_Corp.ContactDetails.Id;
        //                //db.Create(DT_Corp);
        //                db.SaveChanges();
        //            }
        //            blog.DBTrack = c.DBTrack;
        //            db.RecruitJoinParaProcessResult.Attach(blog);
        //            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
        //            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //            db.SaveChanges();
        //            var aaq = db.RecruitJoinParaProcessResult.Include(e => e.RecruitJoiningPara).Where(e => e.Id == data).SingleOrDefault();
        //            ts.Complete();
        //            Msg.Add("Record Updated Successfully.");
        //            return Json(new Utility.JsonReturnClass { Id = aaq.Id, Val = aaq.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //        }

        //    }
        //    return View();

        //}


        //[HttpPost]
        //public async Task<ActionResult> EditSave(RecruitJoinParaProcessResult c, int data, FormCollection form) // Edit submit
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        try
        //        { // string ContactDetails = form["FacultySpecializationlist"] == "0" ? "" : form["FacultySpecializationlist"];
        //            //  bool Auth = form["Autho_Action"] == "" ? false : true;
        //            bool Auth = form["Autho_Allow"] == "true" ? true : false;

        //            var ActivityAccepted = form["ActivityAcceptedAppl"] == null ? "" : form["ActivityAcceptedAppl"];
        //            var ActivityLetterIssue = form["ActivityLetterIssueAppl"] == null ? "" : form["ActivityLetterIssueAppl"];

        //            c.ActivityAccepted = Convert.ToBoolean(ActivityAccepted);
        //            c.ActivityLetterIssue = Convert.ToBoolean(ActivityLetterIssue);

        //            string RecruitJoiningPara = form["RecruitJoiningParalist"] == null ? "" : form["RecruitJoiningParalist"];

        //            if (RecruitJoiningPara != "" && RecruitJoiningPara != null)
        //            {
        //                var val = db.RecruitJoiningPara.Find(int.Parse(RecruitJoiningPara));
        //                c.RecruitJoiningPara = val;
        //            }

        //            var db_Data = db.RecruitJoinParaProcessResult
        //                .Include(e => e.RecruitJoiningPara)
        //                 .Where(e => e.Id == data).SingleOrDefault();

        //            //List<RecruitJoiningPara> jobinside = new List<RecruitJoiningPara>();
        //            //string j_inside = form["JobInsideOrglist"];

        //            //if (j_inside != null)
        //            //{
        //            //    var ids = Utility.StringIdsToListIds(j_inside);
        //            //    foreach (var ca in ids)
        //            //    {
        //            //        var Lookup_val = db.RecruitJoiningPara.Find(ca);

        //            //        jobinside.Add(Lookup_val);
        //            //        db_Data.RecruitJoiningPara = jobinside;
        //            //    }
        //            //}
        //            //else
        //            //{
        //            //    db_Data.RecruitJoiningPara = null;
        //            //}

        //            if (Auth == false)
        //            {


        //                if (ModelState.IsValid)
        //                {



        //                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                    {
        //                        db.RecruitJoinParaProcessResult.Attach(db_Data);
        //                        db.Entry(db_Data).State = System.Data.Entity.EntityState.Modified;
        //                        db.SaveChanges();
        //                        TempData["RowVersion"] = db_Data.RowVersion;
        //                        db.Entry(db_Data).State = System.Data.Entity.EntityState.Detached;

        //                        var Curr_Lookup = db.RecruitJoinParaProcessResult.Find(data);
        //                        TempData["CurrRowVersion"] = Curr_Lookup.RowVersion;
        //                        db.Entry(Curr_Lookup).State = System.Data.Entity.EntityState.Detached;

        //                        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
        //                        {

        //                            RecruitJoinParaProcessResult blog = null; // to retrieve old data
        //                            DbPropertyValues originalBlogValues = null;

        //                            using (var context = new DataBaseContext())
        //                            {
        //                                blog = context.RecruitJoinParaProcessResult.Where(e => e.Id == data)
        //                              .Include(e => e.RecruitJoiningPara).SingleOrDefault();
        //                                originalBlogValues = context.Entry(blog).OriginalValues;
        //                            }

        //                            c.DBTrack = new DBTrack
        //                            {
        //                                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                                Action = "M",
        //                                ModifiedBy = SessionManager.UserName,
        //                                ModifiedOn = DateTime.Now
        //                            };
        //                            RecruitJoinParaProcessResult lk = new RecruitJoinParaProcessResult
        //                            {
        //                                ActivityAccepted = c.ActivityAccepted,
        //                                ActivityAcceptedDate = c.ActivityAcceptedDate,
        //                                ActivityDate = c.ActivityDate,
        //                                ActivityLetterIssue = c.ActivityLetterIssue,
        //                                RecruitJoiningPara = c.RecruitJoiningPara,
        //                                DBTrack = c.DBTrack

        //                            };


        //                            db.RecruitJoinParaProcessResult.Attach(lk);
        //                            db.Entry(lk).State = System.Data.Entity.EntityState.Modified;
        //                            db.Entry(lk).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //                            // int a = EditS(Corp, Addrs, ContactDetails, data, c, c.DBTrack);

        //                            // int a = EditS(Values, Valueshobby, Valuesls, Valuesqd, Valuesskl, Valuesscr, data, c, c.DBTrack);


        //                            using (var context = new DataBaseContext())
        //                            {

        //                                var obj = DBTrackFile.DBTrackSave("Recruitement/Recruitement", originalBlogValues, db.ChangeTracker, c.DBTrack);
        //                                DT_RecruitJoinParaProcessResult DT_Corp = (DT_RecruitJoinParaProcessResult)obj;
        //                                DT_Corp.RecruitJoiningPara_Id = c.RecruitJoiningPara == null ? 0 : c.RecruitJoiningPara.Id;
        //                                db.Create(DT_Corp);
        //                                db.SaveChanges();
        //                            }
        //                            await db.SaveChangesAsync();
        //                            ts.Complete();

        //                            Msg.Add("  Record Updated");
        //                            return Json(new Utility.JsonReturnClass { Id = lk.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                        }
        //                    }



        //                    List<string> MsgB = new List<string>();
        //                    MsgB.Add("Record modified by another user.So refresh it and try to save again.");
        //                    return Json(new Utility.JsonReturnClass { success = false, responseText = MsgB }, JsonRequestBehavior.AllowGet);

        //                    //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
        //                }
        //            }
        //            else
        //            {
        //                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                {

        //                    RecruitJoinParaProcessResult blog = null; // to retrieve old data
        //                    DbPropertyValues originalBlogValues = null;
        //                    RecruitJoinParaProcessResult Old_Corp = null;

        //                    using (var context = new DataBaseContext())
        //                    {
        //                        blog = context.RecruitJoinParaProcessResult.Where(e => e.Id == data).SingleOrDefault();
        //                        originalBlogValues = context.Entry(blog).OriginalValues;
        //                    }
        //                    c.DBTrack = new DBTrack
        //                    {
        //                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                        Action = "M",
        //                        IsModified = blog.DBTrack.IsModified == true ? true : false,
        //                        ModifiedBy = SessionManager.UserName,
        //                        ModifiedOn = DateTime.Now
        //                    };

        //                    if (TempData["RowVersion"] == null)
        //                    {
        //                        TempData["RowVersion"] = blog.RowVersion;
        //                    }

        //                    RecruitJoinParaProcessResult corp = new RecruitJoinParaProcessResult()
        //                    {
        //                        ActivityAccepted = c.ActivityAccepted,
        //                        ActivityAcceptedDate = c.ActivityAcceptedDate,
        //                        ActivityDate = c.ActivityDate,
        //                        ActivityLetterIssue = c.ActivityLetterIssue,
        //                        RecruitJoiningPara = c.RecruitJoiningPara,
        //                        DBTrack = c.DBTrack,
        //                        RowVersion = (Byte[])TempData["RowVersion"]
        //                    };


        //                    //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

        //                    using (var context = new DataBaseContext())
        //                    {
        //                        var obj = DBTrackFile.ModifiedDataHistory("Recruitment/Recruitment", "M", blog, corp, "RecruitJoinParaProcessResult", c.DBTrack);
        //                        // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

        //                        Old_Corp = context.RecruitJoinParaProcessResult.Where(e => e.Id == data)
        //                            .Include(e => e.RecruitJoiningPara)
        //                            .SingleOrDefault();
        //                        DT_RecruitJoinParaProcessResult DT_Corp = (DT_RecruitJoinParaProcessResult)obj;
        //                        DT_Corp.RecruitJoiningPara_Id = c.RecruitJoiningPara == null ? 0 : c.RecruitJoiningPara.Id;

        //                        db.Create(DT_Corp);
        //                        db.SaveChanges();
        //                    }
        //                    blog.DBTrack = c.DBTrack;
        //                    db.RecruitJoinParaProcessResult.Attach(blog);
        //                    db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
        //                    db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //                    db.SaveChanges();
        //                    ts.Complete();
        //                    Msg.Add("Record Updated Successfully.");
        //                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                }

        //            }
        //        }
        //        catch (DbUpdateConcurrencyException ex)
        //        {
        //            var entry = ex.Entries.Single();
        //            var clientValues = (RecruitJoinParaProcessResult)entry.Entity;
        //            var databaseEntry = entry.GetDatabaseValues();
        //            if (databaseEntry == null)
        //            {
        //                Msg.Add(ex.Message);
        //                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                //  return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
        //            }
        //            else
        //            {
        //                var databaseValues = (RecruitJoinParaProcessResult)databaseEntry.ToObject();
        //                c.RowVersion = databaseValues.RowVersion;

        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            Msg.Add(e.Message);
        //            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //        }
        //        return View();
        //    }
        //}

        [HttpPost]
        public async Task<ActionResult> EditSave(RecruitJoinParaProcessResult c, int data, FormCollection form) // Edit submit
        {
            List<String> Msg = new List<String>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var ActivityAccepted = form["ActivityAcceptedAppl"] == null ? "" : form["ActivityAcceptedAppl"];
                    var ActivityLetterIssue = form["ActivityLetterIssueAppl"] == null ? "" : form["ActivityLetterIssueAppl"];
                    string RecruitJoiningPara = form["RecruitJoiningParalist"] == null ? "" : form["RecruitJoiningParalist"];
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;

                    c.ActivityAccepted = Convert.ToBoolean(ActivityAccepted);
                    c.ActivityLetterIssue = Convert.ToBoolean(ActivityLetterIssue);

                    if (RecruitJoiningPara != "" && RecruitJoiningPara != null)
                    {
                        var val = db.RecruitJoiningPara.Find(int.Parse(RecruitJoiningPara));
                        c.RecruitJoiningPara = val;
                    }
                    //    var db_Data = db.RecruitJoinParaProcessResult
                    //.Include(e => e.RecruitJoiningPara)
                    // .Where(e => e.Id == data).SingleOrDefault();
                    if (Auth == false)
                    {
                        if (ModelState.IsValid)
                        {

                            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {
                                RecruitJoinParaProcessResult blog = null; // to retrieve old data
                                DbPropertyValues originalBlogValues = null;

                                using (var context = new DataBaseContext())
                                {
                                    blog = context.RecruitJoinParaProcessResult.Where(e => e.Id == data)
                                      .Include(e => e.RecruitJoiningPara).SingleOrDefault();
                                    originalBlogValues = context.Entry(blog).OriginalValues;
                                }

                                c.DBTrack = new DBTrack
                                {
                                    CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                    CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                    Action = "M",
                                    ModifiedBy = SessionManager.UserName,
                                    ModifiedOn = DateTime.Now
                                };

                                //  int a = EditS(Corp, Addrs, ContactDetails, data, c, c.DBTrack);


                                if (RecruitJoiningPara != null)
                                {
                                    if (RecruitJoiningPara != "")
                                    {
                                        var val = db.RecruitJoiningPara.Find(int.Parse(RecruitJoiningPara));
                                        c.RecruitJoiningPara = val;

                                        var add = db.RecruitJoinParaProcessResult.Include(e => e.RecruitJoiningPara).Where(e => e.Id == data).SingleOrDefault();
                                        IList<RecruitJoinParaProcessResult> RecruitJoiningParadetails = null;
                                        if (add.RecruitJoiningPara != null)
                                        {
                                            RecruitJoiningParadetails = db.RecruitJoinParaProcessResult.Where(x => x.RecruitJoiningPara.Id == add.RecruitJoiningPara.Id && x.Id == data).ToList();
                                        }
                                        else
                                        {
                                            RecruitJoiningParadetails = db.RecruitJoinParaProcessResult.Where(x => x.Id == data).ToList();
                                        }
                                        if (RecruitJoiningParadetails != null)
                                        {
                                            foreach (var s in RecruitJoiningParadetails)
                                            {
                                                s.RecruitJoiningPara = c.RecruitJoiningPara;
                                                db.RecruitJoinParaProcessResult.Attach(s);
                                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                                // await db.SaveChangesAsync(false);
                                                db.SaveChanges();
                                                TempData["RowVersion"] = s.RowVersion;
                                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    var RecruitJoiningParadetails = db.RecruitJoinParaProcessResult.Include(e => e.RecruitJoiningPara).Where(x => x.Id == data).ToList();
                                    foreach (var s in RecruitJoiningParadetails)
                                    {
                                        s.RecruitJoiningPara = null;
                                        db.RecruitJoinParaProcessResult.Attach(s);
                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                        //await db.SaveChangesAsync();
                                        db.SaveChanges();
                                        TempData["RowVersion"] = s.RowVersion;
                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    }
                                }



                                var CurCorp = db.RecruitJoinParaProcessResult.Find(data);
                                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                {
                                    RecruitJoinParaProcessResult corp = new RecruitJoinParaProcessResult()
                                    {
                                        ActivityAccepted = c.ActivityAccepted,
                                        ActivityAcceptedDate = c.ActivityAcceptedDate,
                                        ActivityDate = c.ActivityDate,
                                        ActivityLetterIssue = c.ActivityLetterIssue,
                                        Id = data,
                                        DBTrack = c.DBTrack
                                    };


                                    db.RecruitJoinParaProcessResult.Attach(corp);
                                    db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                                    db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                    //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                }

                                using (var context = new DataBaseContext())
                                {

                                    var obj = DBTrackFile.DBTrackSave("Recruitement/Recruitement", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                    DT_RecruitJoinParaProcessResult DT_Corp = (DT_RecruitJoinParaProcessResult)obj;
                                    DT_Corp.RecruitJoiningPara_Id = blog.RecruitJoiningPara.Id == null ? 0 : blog.RecruitJoiningPara.Id;
                                    db.Create(DT_Corp);
                                    db.SaveChanges();
                                }
                                await db.SaveChangesAsync();
                                ts.Complete();

                                Msg.Add("Record Updated Successfully.");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                            List<string> MsgB = new List<string>();
                            MsgB.Add("Record modified by another user.So refresh it and try to save again.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = MsgB }, JsonRequestBehavior.AllowGet);

                            //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            RecruitJoinParaProcessResult blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            RecruitJoinParaProcessResult Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.RecruitJoinParaProcessResult.Where(e => e.Id == data).SingleOrDefault();
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

                            if (TempData["RowVersion"] == null)
                            {
                                TempData["RowVersion"] = blog.RowVersion;
                            }

                            RecruitJoinParaProcessResult corp = new RecruitJoinParaProcessResult()
                            {
                                ActivityAccepted = c.ActivityAccepted,
                                ActivityAcceptedDate = c.ActivityAcceptedDate,
                                ActivityDate = c.ActivityDate,
                                ActivityLetterIssue = c.ActivityLetterIssue,
                                Id = data,
                                DBTrack = c.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };


                            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Recruitement/Recruitement", "M", blog, corp, "RecruitJoinParaProcessResult", c.DBTrack);
                                // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                Old_Corp = context.RecruitJoinParaProcessResult.Where(e => e.Id == data)
                                    .Include(e => e.RecruitJoiningPara).SingleOrDefault();
                                DT_RecruitJoinParaProcessResult DT_Corp = (DT_RecruitJoinParaProcessResult)obj;
                                DT_Corp.RecruitJoiningPara_Id = DBTrackFile.ValCompare(Old_Corp.RecruitJoiningPara, c.RecruitJoiningPara);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
                                db.Create(DT_Corp);
                                //db.SaveChanges();
                            }
                            blog.DBTrack = c.DBTrack;
                            db.RecruitJoinParaProcessResult.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("Record Updated Successfully.");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }

                    }
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    var entry = ex.Entries.Single();
                    var clientValues = (RecruitJoinParaProcessResult)entry.Entity;
                    var databaseEntry = entry.GetDatabaseValues();
                    if (databaseEntry == null)
                    {
                        Msg.Add(ex.Message);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //  return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                    }
                    else
                    {
                        var databaseValues = (RecruitJoinParaProcessResult)databaseEntry.ToObject();
                        c.RowVersion = databaseValues.RowVersion;

                    }
                }
                catch (Exception e)
                {
                    Msg.Add(e.Message);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
                return View();
            }

        }



        [HttpPost]
        public ActionResult GetLookupDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.RecruitJoiningPara
                    .Include(e => e.RecruitJoinPara)


                    .ToList();
                if (!string.IsNullOrEmpty(data))
                {
                    var all = fall.Where(d => d.FullDetails.ToString().Contains(data)).ToList();
                    var result = (from c in all
                                  select new { c.Id, c.FullDetails }).Distinct();
                    return Json(result, JsonRequestBehavior.AllowGet);

                }
                else
                {
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);

                }

            }
        }


        [HttpPost]
        public ActionResult GetLookupDetailsJoiningPara(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.RecruitJoiningPara
                    .Include(e => e.RecruitJoinPara)



                    .ToList();
                if (!string.IsNullOrEmpty(data))
                {
                    var all = fall.Where(d => d.FullDetails.ToString().Contains(data)).ToList();
                    var result = (from c in all
                                  select new { c.Id, c.FullDetails }).Distinct();
                    return Json(result, JsonRequestBehavior.AllowGet);

                }
                else
                {
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);

                }

            }
        }


        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int? data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    RecruitJoinParaProcessResult recruitEva = db.RecruitJoinParaProcessResult.Include(e => e.RecruitJoiningPara)
                                                        .Where(e => e.Id == data).SingleOrDefault();



                    if (recruitEva.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack, corporates, null, "Corporate");
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = recruitEva.DBTrack.CreatedBy != null ? recruitEva.DBTrack.CreatedBy : null,
                                CreatedOn = recruitEva.DBTrack.CreatedOn != null ? recruitEva.DBTrack.CreatedOn : null,
                                IsModified = recruitEva.DBTrack.IsModified == true ? true : false
                            };
                            recruitEva.DBTrack = dbT;
                            db.Entry(recruitEva).State = System.Data.Entity.EntityState.Modified;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Recruitement/Recruitement", null, db.ChangeTracker, recruitEva.DBTrack);
                            //DT_JobSource DT_OBJ = (DT_JobSource)rtn_Obj;
                            //db.Create(DT_OBJ);

                            await db.SaveChangesAsync();
                            ts.Complete();

                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            // return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else
                    {

                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {


                            //var lkValue1 = new HashSet<int>(recruitEva.RecruitEvaluationPara.Id.ToString());
                            //if (lkValue1.Count > 0)
                            //{
                            //    Msg.Add(" Child record exists.Cannot remove it..  ");
                            //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            //    //return this.Json(new Object[] { "", "Child record exists.Cannot delete.", JsonRequestBehavior.AllowGet });
                            //}

                            //var lkValue2 = new HashSet<int>(recruitEva.RecruitJoiningPara.Select(e => e.Id));
                            //if (lkValue2.Count > 0)
                            //{
                            //    Msg.Add(" Child record exists.Cannot remove it..  ");
                            //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            //    // return this.Json(new Object[] { "", "Child record exists.Cannot delete.", JsonRequestBehavior.AllowGet });
                            //}



                            db.Entry(recruitEva).State = System.Data.Entity.EntityState.Deleted;
                            db.SaveChanges();
                            ts.Complete();

                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                        }
                    }
                    db.Entry(recruitEva).State = System.Data.Entity.EntityState.Deleted;
                    db.SaveChanges();
                    // ts.Complete();

                    Msg.Add("  Data removed successfully.  ");
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
                //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
            }
        }
    }
}