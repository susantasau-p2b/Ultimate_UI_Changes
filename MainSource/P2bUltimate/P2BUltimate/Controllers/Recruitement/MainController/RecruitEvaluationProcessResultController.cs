using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Recruitment;
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
namespace P2BUltimate.Controllers

    .Recruitment.MainController
{
    public class RecruitEvaluationProcessResultController : Controller
    {
        List<String> Msg = new List<String>();

        //private DataBaseContext db = new DataBaseContext();
        // 
        // GET: /RecruitEvaluationProcess/
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Partial()
        {
            return View("~/Views/Shared/Recruitement/_RecruitEvaluationProcessResult.cshtml");
        }

        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult Create(RecruitEvaluationProcessResult c, FormCollection form) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var ActivityAttendance = form["ActivityAttendanceAppl"] == null ? "" : form["ActivityAttendanceAppl"];
                var ActivityLetterIssue = form["ActivityLetterIssueAppl"] == null ? "" : form["ActivityLetterIssueAppl"];

                c.ActivityAttendance = Convert.ToBoolean(ActivityAttendance);
                c.ActivityLetterIssue = Convert.ToBoolean(ActivityLetterIssue);

                string ActivityResult = form["ActivityResultlist"] == null ? "" : form["ActivityResultlist"];
                string RecruitEvaluationPara = form["RecruitEvaluationParalist"] == null ? "" : form["RecruitEvaluationParalist"];

                if (RecruitEvaluationPara != "" && RecruitEvaluationPara != null)
                {
                    var val = db.RecruitEvaluationPara.Find(int.Parse(RecruitEvaluationPara));
                    c.RecruitEvaluationPara = val;
                }
                if (ActivityResult != "" && ActivityResult != null)
                {
                    var val = db.LookupValue.Find(int.Parse(ActivityResult));
                    c.ActivityResult = val;
                }


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

        public class RecruitEvalProc
        {
            public Array RE_id { get; set; }
            public string RE_val { get; set; }
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
                var Q = db.RecruitEvaluationProcessResult
                     .Include(e => e.ActivityResult)
                      .Include(e => e.RecruitEvaluationPara)

            .Where(e => e.Id == data).Select
            (e => new
            {

                AbsentRemark = e.AbsentRemark,
                ActivityAttendance = e.ActivityAttendance,
                ActivityDate = e.ActivityDate,
                ActivityLetterIssue = e.ActivityLetterIssue,
                ActivityResult = e.ActivityResult.Id,
                ActivityScore = e.ActivityScore,
                PannelNarration = e.PannelNarration,
                Action = e.DBTrack.Action
            }).ToList();


                List<RecruitEvalProc> return_data = new List<RecruitEvalProc>();


                //var a = db.RecruitEvaluationProcessResult.Include(e => e.RecruitEvaluationPara.RecruitEvalPara).Include(e=>e.ActivityResult).Where(e => e.Id == data).Select(e => e.RecruitEvaluationPara).ToList();
                var a = db.RecruitEvaluationProcessResult.Include(e => e.RecruitEvaluationPara)
                    .Include(e => e.RecruitEvaluationPara.RecruitEvalPara).Where(e => e.Id == data).ToList();
                foreach (var ca in a)
                {
                    return_data.Add(
                new RecruitEvalProc
                {
                    RE_id = ca.Id.ToString().ToArray(),
                    RE_val = ca.RecruitEvaluationPara.FullDetails
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



                var Corp = db.RecruitEvaluationProcessResult.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, return_data, Auth, JsonRequestBehavior.AllowGet });
            }
        }


        //public async Task<ActionResult> EditSave(RecruitEvaluationProcessResult c, int data, FormCollection form) // Edit submit
        //{

        //    // string ContactDetails = form["FacultySpecializationlist"] == "0" ? "" : form["FacultySpecializationlist"];
        //    //  bool Auth = form["Autho_Action"] == "" ? false : true;
        //    bool Auth = form["Autho_Allow"] == "true" ? true : false;
        //    var ActivityResult = form["ActivityResultlist"] == null ? "" : form["ActivityResultlist"];
        //    var RecruitEvaluationPara = form["RecruitEvaluationParalist"] == null ? "" : form["RecruitEvaluationParalist"];

        //    var db_Data = db.RecruitEvaluationProcessResult
        //        .Include(e => e.ActivityResult)
        //        .Include(e => e.RecruitEvaluationPara)
        //        .Where(e => e.Id == data).SingleOrDefault();

        //    //db_Data.RecruitEvaluationPara = null;
        //   // db_Data.RecruitJoiningPara = null;


        //    if (RecruitEvaluationPara != null)
        //    {
        //        if (RecruitEvaluationPara != "")
        //        {
        //            int ConId = Convert.ToInt32(RecruitEvaluationPara);
        //            var val = db.RecruitEvaluationPara
        //                .Include(e => e.SelectionPanel)
        //                .Include(e => e.RecruitEvalPara)
        //                .Where(e => e.Id == ConId).SingleOrDefault();
        //            c.RecruitEvaluationPara = val;

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
        //                        c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

        //                        RecruitEvaluationProcessResult corporate = new RecruitEvaluationProcessResult()
        //                        {

        //                            AbsentRemark = c.AbsentRemark,
        //                            ActivityAttendance = c.ActivityAttendance,
        //                            ActivityDate = c.ActivityDate,
        //                            ActivityLetterIssue = c.ActivityLetterIssue,
        //                            ActivityResult = c.ActivityResult,
        //                            ActivityScore = c.ActivityScore,
        //                            PannelNarration = c.PannelNarration,
        //                            RecruitEvaluationPara = c.RecruitEvaluationPara,

        //                            DBTrack = c.DBTrack
        //                        };


        //                        db.RecruitEvaluationProcessResult.Attach(corporate);
        //                        db.Entry(corporate).State = System.Data.Entity.EntityState.Modified;
        //                        db.SaveChanges();
        //                        TempData["RowVersion"] = db_Data.RowVersion;
        //                        db.Entry(db_Data).State = System.Data.Entity.EntityState.Detached;

        //                        var Curr_OBJ = db.RecruitEvaluationProcessResult.Find(data);
        //                        TempData["CurrRowVersion"] = Curr_OBJ.RowVersion;
        //                        db.Entry(Curr_OBJ).State = System.Data.Entity.EntityState.Detached;

        //                        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
        //                        {
        //                            RecruitEvaluationProcessResult blog = null; // to retrieve old data
        //                            DbPropertyValues originalBlogValues = null;


        //                            blog = context.RecruitEvaluationProcessResult.Where(e => e.Id == data).Include(e => e.RecruitEvaluationPara)
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

        //                            var aaq = db.RecruitEvaluationProcessResult.Include(e => e.RecruitEvaluationPara).Where(e => e.Id == data).SingleOrDefault();
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

        //            RecruitEvaluationProcessResult blog = null; // to retrieve old data
        //            DbPropertyValues originalBlogValues = null;
        //            RecruitEvaluationProcessResult Old_Corp = null;

        //            using (var context = new DataBaseContext())
        //            {
        //                blog = context.RecruitEvaluationProcessResult.Where(e => e.Id == data).SingleOrDefault();
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

        //                Old_Corp = context.RecruitEvaluationProcessResult.Where(e => e.Id == data).Include(e => e.RecruitEvaluationPara)
        //                  .SingleOrDefault();
        //                //DT_JobSource DT_Corp = (DT_JobSource)obj;
        //                //  DT_Corp.TrainingInstitue_Id = DBTrackFile.ValCompare(Old_Corp.TrainingInstitue, c.TrainingInstitue);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
        //                // DT_Corp.FacultyType_Id = DBTrackFile.ValCompare(Old_Corp.FacultyType, c.FacultyType); //Old_Corp.BusinessType == c.BusinessType ? 0 : Old_Corp.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_Corp.BusinessType.Id;
        //                //DT_Corp.ContactDetails_Id = DBTrackFile.ValCompare(Old_Corp.ContactDetails, c.ContactDetails); //Old_Corp.ContactDetails == c.ContactDetails ? 0 : Old_Corp.ContactDetails == null && c.ContactDetails != null ? c.ContactDetails.Id : Old_Corp.ContactDetails.Id;
        //                //db.Create(DT_Corp);
        //                db.SaveChanges();
        //            }
        //            blog.DBTrack = c.DBTrack;
        //            db.RecruitEvaluationProcessResult.Attach(blog);
        //            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
        //            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //            db.SaveChanges();
        //            var aaq = db.RecruitEvaluationProcessResult.Include(e => e.RecruitEvaluationPara).Where(e => e.Id == data).SingleOrDefault();
        //            ts.Complete();
        //            Msg.Add("Record Updated Successfully.");
        //            return Json(new Utility.JsonReturnClass { Id = aaq.Id, Val = aaq.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //        }

        //    }
        //    return View();

        //}



        [HttpPost]
        public async Task<ActionResult> EditSave(RecruitEvaluationProcessResult c, int data, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                { // string ContactDetails = form["FacultySpecializationlist"] == "0" ? "" : form["FacultySpecializationlist"];
                    //  bool Auth = form["Autho_Action"] == "" ? false : true;
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;

                    var ActivityAttendance = form["ActivityAttendanceAppl"] == null ? "" : form["ActivityAttendanceAppl"];
                    var ActivityLetterIssue = form["ActivityLetterIssueAppl"] == null ? "" : form["ActivityLetterIssueAppl"];

                    c.ActivityAttendance = Convert.ToBoolean(ActivityAttendance);
                    c.ActivityLetterIssue = Convert.ToBoolean(ActivityLetterIssue);

                    string ActivityResult = form["ActivityResultlist"] == null ? "" : form["ActivityResultlist"];
                    string RecruitEvaluationPara = form["RecruitEvaluationParalist"] == null ? "" : form["RecruitEvaluationParalist"];

                    if (RecruitEvaluationPara != "" && RecruitEvaluationPara != null)
                    {
                        var val = db.RecruitEvaluationPara.Find(int.Parse(RecruitEvaluationPara));
                        c.RecruitEvaluationPara = val;
                    }
                    if (ActivityResult != "" && ActivityResult != null)
                    {
                        var val = db.LookupValue.Find(int.Parse(ActivityResult));
                        c.ActivityResult = val;
                    }


                    var db_Data = db.RecruitEvaluationProcessResult
                        .Include(e => e.ActivityResult)
                        .Include(e => e.RecruitEvaluationPara)
                         .Where(e => e.Id == data).SingleOrDefault();

                    //List<RecruitJoiningPara> jobinside = new List<RecruitJoiningPara>();
                    //string j_inside = form["JobInsideOrglist"];

                    //if (j_inside != null)
                    //{
                    //    var ids = Utility.StringIdsToListIds(j_inside);
                    //    foreach (var ca in ids)
                    //    {
                    //        var Lookup_val = db.RecruitJoiningPara.Find(ca);

                    //        jobinside.Add(Lookup_val);
                    //        db_Data.RecruitJoiningPara = jobinside;
                    //    }
                    //}
                    //else
                    //{
                    //    db_Data.RecruitJoiningPara = null;
                    //}

                    if (Auth == false)
                    {


                        if (ModelState.IsValid)
                        {



                            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {
                                db.RecruitEvaluationProcessResult.Attach(db_Data);
                                db.Entry(db_Data).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                TempData["RowVersion"] = db_Data.RowVersion;
                                db.Entry(db_Data).State = System.Data.Entity.EntityState.Detached;

                                var Curr_Lookup = db.RecruitEvaluationProcessResult.Find(data);
                                TempData["CurrRowVersion"] = Curr_Lookup.RowVersion;
                                db.Entry(Curr_Lookup).State = System.Data.Entity.EntityState.Detached;

                                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                {

                                    RecruitEvaluationProcessResult blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.RecruitEvaluationProcessResult.Where(e => e.Id == data)
                                      .Include(e => e.ActivityResult)
                        .Include(e => e.RecruitEvaluationPara).SingleOrDefault();
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
                                    RecruitEvaluationProcessResult lk = new RecruitEvaluationProcessResult
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


                                    db.RecruitEvaluationProcessResult.Attach(lk);
                                    db.Entry(lk).State = System.Data.Entity.EntityState.Modified;
                                    db.Entry(lk).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                    // int a = EditS(Corp, Addrs, ContactDetails, data, c, c.DBTrack);

                                    // int a = EditS(Values, Valueshobby, Valuesls, Valuesqd, Valuesskl, Valuesscr, data, c, c.DBTrack);


                                    using (var context = new DataBaseContext())
                                    {

                                        var obj = DBTrackFile.DBTrackSave("Recruitement/Recruitement", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                        DT_RecruitEvaluationProcessResult DT_Corp = (DT_RecruitEvaluationProcessResult)obj;

                                        db.Create(DT_Corp);
                                        db.SaveChanges();
                                    }
                                    await db.SaveChangesAsync();
                                    ts.Complete();

                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = lk.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }
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

                            RecruitEvaluationProcessResult blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            RecruitEvaluationProcessResult Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.RecruitEvaluationProcessResult.Where(e => e.Id == data).SingleOrDefault();
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

                            RecruitEvaluationProcessResult corp = new RecruitEvaluationProcessResult()
                            {
                                AbsentRemark = c.AbsentRemark,
                                ActivityAttendance = c.ActivityAttendance,
                                ActivityDate = c.ActivityDate,
                                ActivityLetterIssue = c.ActivityLetterIssue,
                                ActivityResult = c.ActivityResult,
                                ActivityScore = c.ActivityScore,
                                PannelNarration = c.PannelNarration,
                                RecruitEvaluationPara = c.RecruitEvaluationPara,

                                DBTrack = c.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };


                            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Recruitment/Recruitment", "M", blog, corp, "RecruitEvaluationProcessResult", c.DBTrack);
                                // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                Old_Corp = context.RecruitEvaluationProcessResult.Where(e => e.Id == data)
                                      .Include(e => e.ActivityResult)
                        .Include(e => e.RecruitEvaluationPara)
                                    .SingleOrDefault();
                                DT_RecruitEvaluationProcessResult DT_Corp = (DT_RecruitEvaluationProcessResult)obj;
                                DT_Corp.RecruitEvaluationPara_Id = c.RecruitEvaluationPara == null ? 0 : c.RecruitEvaluationPara.Id;
                                DT_Corp.ActivityResult_Id = c.ActivityResult == null ? 0 : c.ActivityResult.Id;
                                db.Create(DT_Corp);
                                //db.SaveChanges();
                            }
                            blog.DBTrack = c.DBTrack;
                            db.RecruitEvaluationProcessResult.Attach(blog);
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
                    var clientValues = (RecruitEvaluationProcessResult)entry.Entity;
                    var databaseEntry = entry.GetDatabaseValues();
                    if (databaseEntry == null)
                    {
                        Msg.Add(ex.Message);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //  return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                    }
                    else
                    {
                        var databaseValues = (RecruitEvaluationProcessResult)databaseEntry.ToObject();
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
                var fall = db.RecruitEvaluationPara
                    .Include(e => e.SelectionPanel)
                    .Include(e => e.RecruitEvalPara)


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
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    RecruitEvaluationProcessResult recruitEva = db.RecruitEvaluationProcessResult.Include(e => e.RecruitEvaluationPara)
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