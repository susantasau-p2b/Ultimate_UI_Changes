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
    public class RecruitEvaluationProcessController : Controller
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
            return View("~/Views/Shared/Recruitement/_RecruitEvaluationProcess.cshtml");
        }

        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult Create(RecruitEvaluationProcess c, FormCollection form) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {

                    c.RecruitEvaluationPara = null;
                    List<RecruitEvaluationPara> OBJ = new List<RecruitEvaluationPara>();
                    string Values = form["RecruitEvaluationParalist"];

                    if (Values != null)
                    {
                        var ids = Utility.StringIdsToListIds(Values);
                        foreach (var ca in ids)
                        {
                            var OBJ_val = db.RecruitEvaluationPara.Find(ca);
                            OBJ.Add(OBJ_val);
                            c.RecruitEvaluationPara = OBJ;
                        }
                    }
                    c.ShortlistingCriteria = null;
                    List<ShortlistingCriteria> shortlisting = new List<ShortlistingCriteria>();
                    string Values12 = form["RecruitShortlistingParalist"];

                    if (Values12 != null)
                    {
                        var ids = Utility.StringIdsToListIds(Values12);
                        foreach (var ca in ids)
                        {
                            var OBJ_val = db.ShortlistingCriteria.Find(ca);
                            shortlisting.Add(OBJ_val);
                            c.ShortlistingCriteria = shortlisting;
                        }
                    }

                    c.RecruitJoiningPara = null;
                    List<RecruitJoiningPara> OBJ1 = new List<RecruitJoiningPara>();
                    string Values1 = form["RecruitJoiningParalist"];

                    if (Values1 != null)
                    {
                        var ids = Utility.StringIdsToListIds(Values1);
                        foreach (var ca in ids)
                        {
                            var OBJ_val1 = db.RecruitJoiningPara.Find(ca);
                            OBJ1.Add(OBJ_val1);
                            c.RecruitJoiningPara = OBJ1;
                        }
                    }

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {


                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            RecruitEvaluationProcess corporate = new RecruitEvaluationProcess()
                            {

                                RecruitEvaluationPara = c.RecruitEvaluationPara,
                                RecruitJoiningPara = c.RecruitJoiningPara,
                                Name = c.Name,
                                ShortlistingCriteria = c.ShortlistingCriteria,
                                DBTrack = c.DBTrack
                            };

                            db.RecruitEvaluationProcess.Add(corporate);
                            var rtn_Obj = DBTrackFile.DBTrackSave("Recruitment/Recruitment", null, db.ChangeTracker, c.DBTrack);
                            //  DT_JobSource DT_Corp = (DT_JobSource)rtn_Obj;
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

        public class RecruitEvalPara
        {
            public int RE_id { get; set; }
            public string RE_val { get; set; }
        }
        public class RecruitJoinPara
        {
            public int RJ_id { get; set; }
            public string RJ_val { get; set; }
        }

        public class RecruitShortlistpara
        {
            public int Sh_id { get; set; }
            public string Sh_val { get; set; }
        }
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
                var Q = db.RecruitEvaluationProcess
                     .Include(e => e.RecruitEvaluationPara)
                      .Include(e => e.RecruitJoiningPara)
                      .Include(e => e.ShortlistingCriteria)

            .Where(e => e.Id == data).Select
            (e => new
            {

                Name = e.Name,
                Action = e.DBTrack.Action
            }).ToList();


                List<RecruitEvalPara> return_data = new List<RecruitEvalPara>();


                var a = db.RecruitEvaluationProcess.Include(e => e.RecruitEvaluationPara)
                    .Include(e => e.RecruitEvaluationPara)
                    .Include(e => e.RecruitEvaluationPara.Select(t=>t.RecruitEvalPara))
                    .Where(e => e.Id == data).SingleOrDefault();

                foreach (var ca in a.RecruitEvaluationPara)
                {
                    return_data.Add(
                new RecruitEvalPara
                {
                    RE_id = ca.Id,
                    RE_val = ca.FullDetails
                });
                }

                List<RecruitJoinPara> return_data2 = new List<RecruitJoinPara>();
                var a1 = db.RecruitEvaluationProcess.Include(e => e.RecruitJoiningPara)
                          .Include(e => e.RecruitJoiningPara)
                          .Include(e => e.RecruitJoiningPara.Select(t=>t.RecruitJoinPara))
                    .Where(e => e.Id == data).SingleOrDefault();
                foreach (var ca in a1.RecruitJoiningPara)
                {
                    return_data2.Add(
                new RecruitJoinPara
                {
                    RJ_id = ca.Id,
                    RJ_val = ca.FullDetails

                });
                }

                List<RecruitShortlistpara> return_data3 = new List<RecruitShortlistpara>();
                var a2 = db.RecruitEvaluationProcess.Include(e => e.ShortlistingCriteria).Where(e => e.Id == data).SingleOrDefault();

                foreach (var ca in a1.ShortlistingCriteria)
                {
                    return_data3.Add(
                new RecruitShortlistpara
                {
                    Sh_id = ca.Id,
                    Sh_val = ca.FullDetails

                });
                }



                var Corp = db.RecruitEvaluationProcess.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, return_data, return_data2, return_data3, Auth, JsonRequestBehavior.AllowGet });
            }
        }

        [HttpPost]
        public async Task<ActionResult> EditSave(RecruitEvaluationProcess c, int data, FormCollection form) // Edit submit
        {

            // string ContactDetails = form["FacultySpecializationlist"] == "0" ? "" : form["FacultySpecializationlist"];
            //  bool Auth = form["Autho_Action"] == "" ? false : true;
            bool Auth = form["Autho_Allow"] == "true" ? true : false;
            using (DataBaseContext db = new DataBaseContext())
            {
                var db_Data = db.RecruitEvaluationProcess.Include(e => e.RecruitEvaluationPara).Include(e => e.RecruitJoiningPara).Include(e => e.ShortlistingCriteria)
                     .Where(e => e.Id == data).SingleOrDefault();
                db_Data.RecruitEvaluationPara = null;
                db_Data.RecruitJoiningPara = null;

                List<RecruitEvaluationPara> job_agency = new List<RecruitEvaluationPara>();
                string j_agency = form["RecruitEvaluationParalist"];

                if (j_agency != null)
                {
                    var ids = Utility.StringIdsToListIds(j_agency);
                    foreach (var ca in ids)
                    {
                        var Lookup_val = db.RecruitEvaluationPara.Find(ca);

                        job_agency.Add(Lookup_val);
                        db_Data.RecruitEvaluationPara = job_agency;
                    }
                }
                else
                {
                    db_Data.RecruitEvaluationPara = null;
                }

                c.ShortlistingCriteria = null;
                List<ShortlistingCriteria> shortlisting = new List<ShortlistingCriteria>();
                string Values12 = form["RecruitShortlistingParalist"];

                if (Values12 != null)
                {
                    var ids = Utility.StringIdsToListIds(Values12);
                    foreach (var ca in ids)
                    {
                        var OBJ_val = db.ShortlistingCriteria.Find(ca);
                        shortlisting.Add(OBJ_val);
                        db_Data.ShortlistingCriteria = shortlisting;
                    }
                }
                else
                {
                    db_Data.ShortlistingCriteria = null;
                }

                List<RecruitJoiningPara> jobinside = new List<RecruitJoiningPara>();
                string j_inside = form["RecruitJoiningParalist"];

                if (j_inside != null)
                {
                    var ids = Utility.StringIdsToListIds(j_inside);
                    foreach (var ca in ids)
                    {
                        var Lookup_val = db.RecruitJoiningPara.Find(ca);

                        jobinside.Add(Lookup_val);
                        db_Data.RecruitJoiningPara = jobinside;
                    }
                }
                else
                {
                    db_Data.RecruitJoiningPara = null;
                }

                if (Auth == false)
                {
                    if (ModelState.IsValid)
                    {
                        try
                        {

                            //DbContextTransaction transaction = db.Database.BeginTransaction();

                            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {

                                using (var context = new DataBaseContext())
                                {
                                    db.RecruitEvaluationProcess.Attach(db_Data);
                                    db.Entry(db_Data).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    TempData["RowVersion"] = db_Data.RowVersion;
                                    db.Entry(db_Data).State = System.Data.Entity.EntityState.Detached;

                                    var Curr_OBJ = db.RecruitEvaluationProcess.Find(data);
                                    TempData["CurrRowVersion"] = Curr_OBJ.RowVersion;
                                    db.Entry(Curr_OBJ).State = System.Data.Entity.EntityState.Detached;

                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {
                                        RecruitEvaluationProcess blog = null; // to retrieve old data
                                        DbPropertyValues originalBlogValues = null;


                                        blog = context.RecruitEvaluationProcess.Where(e => e.Id == data).Include(e => e.RecruitEvaluationPara)
                                                                .Include(e => e.RecruitJoiningPara)
                                                               .SingleOrDefault();
                                        originalBlogValues = context.Entry(blog).OriginalValues;


                                        c.DBTrack = new DBTrack
                                        {
                                            CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                            CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                            Action = "M",
                                            ModifiedBy = SessionManager.UserName,
                                            ModifiedOn = DateTime.Now
                                        };

                                        RecruitEvaluationProcess lk = new RecruitEvaluationProcess
                                        {
                                            Id = data,

                                            RecruitEvaluationPara = db_Data.RecruitEvaluationPara,
                                            RecruitJoiningPara = db_Data.RecruitJoiningPara,
                                            ShortlistingCriteria = db_Data.ShortlistingCriteria,
                                            Name = c.Name,
                                            DBTrack = c.DBTrack
                                        };


                                        db.RecruitEvaluationProcess.Attach(lk);
                                        db.Entry(lk).State = System.Data.Entity.EntityState.Modified;

                                        // db.Entry(c.DBTrack).State = System.Data.Entity.EntityState.Modified;
                                        //db.SaveChanges();
                                        db.Entry(lk).OriginalValues["RowVersion"] = TempData["RowVersion"];

                                        var obj = DBTrackFile.DBTrackSave("Recruitment/Recruitment", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                        //DT_JobSource DT_LK = (DT_JobSource)obj;
                                        //  DT_LK.Allergy = lk.Allergy.Select(e => e.Id);
                                        //db.Create(DT_LK);
                                        db.SaveChanges();
                                        await db.SaveChangesAsync();
                                        db.Entry(lk).State = System.Data.Entity.EntityState.Detached;

                                        var aaq = db.RecruitEvaluationProcess.Include(e => e.RecruitEvaluationPara).Include(e => e.RecruitJoiningPara).Where(e => e.Id == data).SingleOrDefault();
                                        ts.Complete();
                                        Msg.Add("Record Updated Successfully.");
                                        return Json(new Utility.JsonReturnClass { Id = aaq.Id, Val = aaq.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    }
                                }
                            }
                        }
                        catch (DbUpdateConcurrencyException ex)
                        {
                            var entry = ex.Entries.Single();
                            var clientValues = (JobSource)entry.Entity;
                            var databaseEntry = entry.GetDatabaseValues();
                            if (databaseEntry == null)
                            {
                                return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                            }
                            else
                            {
                                var databaseValues = (JobSource)databaseEntry.ToObject();
                                c.RowVersion = databaseValues.RowVersion;

                            }
                        }

                        return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                    }
                }
                else
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {

                        RecruitEvaluationProcess blog = null; // to retrieve old data
                        DbPropertyValues originalBlogValues = null;
                        RecruitEvaluationProcess Old_Corp = null;

                        using (var context = new DataBaseContext())
                        {
                            blog = context.RecruitEvaluationProcess.Where(e => e.Id == data).SingleOrDefault();
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

                        JobSource corp = new JobSource()
                        {

                            Id = data,
                            DBTrack = c.DBTrack,
                            RowVersion = (Byte[])TempData["RowVersion"]
                        };


                        //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                        using (var context = new DataBaseContext())
                        {
                            var obj = DBTrackFile.ModifiedDataHistory("Recruitment/Recruitment", "M", blog, corp, "RecruitEvaluationProcess", c.DBTrack);
                            // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                            Old_Corp = context.RecruitEvaluationProcess.Where(e => e.Id == data).Include(e => e.RecruitEvaluationPara)
                                .Include(e => e.RecruitJoiningPara).SingleOrDefault();
                            //DT_JobSource DT_Corp = (DT_JobSource)obj;
                            //  DT_Corp.TrainingInstitue_Id = DBTrackFile.ValCompare(Old_Corp.TrainingInstitue, c.TrainingInstitue);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
                            // DT_Corp.FacultyType_Id = DBTrackFile.ValCompare(Old_Corp.FacultyType, c.FacultyType); //Old_Corp.BusinessType == c.BusinessType ? 0 : Old_Corp.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_Corp.BusinessType.Id;
                            //DT_Corp.ContactDetails_Id = DBTrackFile.ValCompare(Old_Corp.ContactDetails, c.ContactDetails); //Old_Corp.ContactDetails == c.ContactDetails ? 0 : Old_Corp.ContactDetails == null && c.ContactDetails != null ? c.ContactDetails.Id : Old_Corp.ContactDetails.Id;
                            //db.Create(DT_Corp);
                            db.SaveChanges();
                        }
                        blog.DBTrack = c.DBTrack;
                        db.RecruitEvaluationProcess.Attach(blog);
                        db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                        db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                        db.SaveChanges();
                        var aaq = db.RecruitEvaluationProcess.Include(e => e.RecruitEvaluationPara).Include(e => e.RecruitJoiningPara).Where(e => e.Id == data).SingleOrDefault();
                        ts.Complete();
                        Msg.Add("Record Updated Successfully.");
                        return Json(new Utility.JsonReturnClass { Id = aaq.Id, Val = aaq.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }

                }
                return View();
            }
        }


        [HttpPost]
        public ActionResult GetLookupDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.RecruitEvaluationPara
                    .Include(e => e.SelectionPanel)
                    .Include(e => e.RecruitEvalPara)


                    .ToList();

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
                //if (!string.IsNullOrEmpty(data))
                //{
                //    var all = fall.Where(d => d.FullDetails.ToString().Contains(data)).ToList();
                //    var result = (from c in all
                //                  select new { c.Id, c.FullDetails }).Distinct();
                //    return Json(result, JsonRequestBehavior.AllowGet);

                //}
                //else
                //{
                //    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                //    return Json(r, JsonRequestBehavior.AllowGet);

                //}
                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);

            }
        }


        [HttpPost]
        public ActionResult GetLookupDetailsJoiningPara(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.RecruitJoiningPara
                    .Include(e => e.RecruitJoinPara)
                    .ToList();
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.RecruitJoiningPara.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }
                //if (!string.IsNullOrEmpty(data))
                //{
                //    var all = fall.Where(d => d.FullDetails.ToString().Contains(data)).ToList();
                //    var result = (from c in all
                //                  select new { c.Id, c.FullDetails }).Distinct();
                //    return Json(result, JsonRequestBehavior.AllowGet);

                //}
                //else
                //{
                //    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                //    return Json(r, JsonRequestBehavior.AllowGet);

                //}
                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);


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
                    RecruitEvaluationProcess recruitEva = db.RecruitEvaluationProcess.Include(e => e.RecruitEvaluationPara)
                        .Include(e => e.RecruitJoiningPara)


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


                            var lkValue1 = new HashSet<int>(recruitEva.RecruitEvaluationPara.Select(e => e.Id));
                            if (lkValue1.Count > 0)
                            {
                                Msg.Add(" Child record exists.Cannot remove it..  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                //return this.Json(new Object[] { "", "Child record exists.Cannot delete.", JsonRequestBehavior.AllowGet });
                            }

                            var lkValue2 = new HashSet<int>(recruitEva.RecruitJoiningPara.Select(e => e.Id));
                            if (lkValue2.Count > 0)
                            {
                                Msg.Add(" Child record exists.Cannot remove it..  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                // return this.Json(new Object[] { "", "Child record exists.Cannot delete.", JsonRequestBehavior.AllowGet });
                            }



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