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

namespace P2BUltimate.Controllers.Recruitment.MainController
{
    public class JobSourceController : Controller
    {

        //private DataBaseContext db = new DataBaseContext();
        List<string> Msg = new List<string>();
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Partial()
        {
            return View("~/Views/Shared/Recruitement/_JobSource.cshtml");
        }


        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult Create(JobSource c, FormCollection form) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<String> Msg = new List<String>();
                try
                {

                    c.JobAgency = null;
                    List<JobAgency> OBJ = new List<JobAgency>();
                    string Values = form["JobAgencylist"];

                    if (Values != null)
                    {
                        var ids = Utility.StringIdsToListIds(Values);
                        foreach (var ca in ids)
                        {
                            var OBJ_val = db.JobAgency.Find(ca);
                            OBJ.Add(OBJ_val);
                            c.JobAgency = OBJ;
                        }
                    }

                    c.JobInsideOrg = null;
                    List<JobInsideOrg> OBJ1 = new List<JobInsideOrg>();
                    string Values1 = form["JobInsideOrglist"];

                    if (Values1 != null)
                    {
                        var ids = Utility.StringIdsToListIds(Values1);
                        foreach (var ca in ids)
                        {
                            var OBJ_val1 = db.JobInsideOrg.Find(ca);
                            OBJ1.Add(OBJ_val1);
                            c.JobInsideOrg = OBJ1;
                        }
                    }

                    c.JobNewsPaper = null;
                    List<JobNewsPaper> OBJ2 = new List<JobNewsPaper>();
                    string Values2 = form["JobNewsPaperlist"];

                    if (Values2 != null)
                    {
                        var ids = Utility.StringIdsToListIds(Values2);
                        foreach (var ca in ids)
                        {
                            var OBJ_val2 = db.JobNewsPaper.Find(ca);
                            OBJ2.Add(OBJ_val2);
                            c.JobNewsPaper = OBJ2;
                        }
                    }

                    c.JobPortal = null;
                    List<JobPortal> OBJ3 = new List<JobPortal>();
                    string Values3 = form["JobPortallist"];

                    if (Values3 != null)
                    {
                        var ids = Utility.StringIdsToListIds(Values3);
                        foreach (var ca in ids)
                        {
                            var OBJ_val3 = db.JobPortal.Find(ca);
                            OBJ3.Add(OBJ_val3);
                            c.JobPortal = OBJ3;
                        }
                    }


                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {


                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            JobSource corporate = new JobSource()
                            {

                                JobAgency = c.JobAgency,
                                JobInsideOrg = c.JobInsideOrg,
                                JobNewsPaper = c.JobNewsPaper,
                                JobPortal = c.JobPortal,
                                DBTrack = c.DBTrack,
                                Id = c.Id
                            };

                            db.JobSource.Add(corporate);
                            var rtn_Obj = DBTrackFile.DBTrackSave("Recruitement/Recruitement", null, db.ChangeTracker, c.DBTrack);
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


        public class RecruitJobAgency
        {
            public Array JA_id { get; set; }
            public Array JA_val { get; set; }
        }
        public class RecruitJobInsideOrg
        {
            public Array JI_id { get; set; }
            public Array JI_val { get; set; }
        }

        public class RecruitJobNewsPaper
        {
            public Array JN_id { get; set; }
            public Array JN_val { get; set; }
        }
        public class RecruitJobPortal
        {
            public Array JP_id { get; set; }
            public Array JP_val { get; set; }
        }
        [HttpPost]
        public ActionResult Edit(int data)
        {
            //string tableName = "Corporate";

            //    // Fetch the table records dynamically
            //    var tableData = db.GetType()
            //    .GetProperty(tableName)
            //    .GetValue(db, null);
            List<RecruitJobAgency> return_data = new List<RecruitJobAgency>();

            using (DataBaseContext db = new DataBaseContext())
            {
                var a = db.JobSource.Include(e => e.JobAgency).Where(e => e.Id == data).Select(e => e.JobAgency).ToList();

                foreach (var ca in a)
                {
                    return_data.Add(
                new RecruitJobAgency
                {
                    JA_id = ca.Select(e => e.Id.ToString()).ToArray(),
                    JA_val = ca.Select(e => e.FullDetails).ToArray()
                });
                }



                List<RecruitJobInsideOrg> return_data2 = new List<RecruitJobInsideOrg>();


                var a1 = db.JobSource.Include(e => e.JobInsideOrg).Where(e => e.Id == data).Select(e => e.JobInsideOrg).ToList();

                foreach (var ca in a1)
                {
                    return_data2.Add(
                new RecruitJobInsideOrg
                {
                    JI_id = ca.Select(e => e.Id.ToString()).ToArray(),
                    JI_val = ca.Select(e => e.FullDetails).ToArray()
                });
                }


                List<RecruitJobNewsPaper> return_data3 = new List<RecruitJobNewsPaper>();


                var a2 = db.JobSource.Include(e => e.JobNewsPaper).Where(e => e.Id == data).Select(e => e.JobNewsPaper).ToList();

                foreach (var ca in a2)
                {
                    return_data3.Add(
                new RecruitJobNewsPaper
                {
                    JN_id = ca.Select(e => e.Id.ToString()).ToArray(),
                    JN_val = ca.Select(e => e.FullDetails).ToArray()
                });
                }



                List<RecruitJobPortal> return_data4 = new List<RecruitJobPortal>();


                var a3 = db.JobSource.Include(e => e.JobPortal).Where(e => e.Id == data).Select(e => e.JobPortal).ToList();

                foreach (var ca in a3)
                {
                    return_data4.Add(
                new RecruitJobPortal
                {
                    JP_id = ca.Select(e => e.Id.ToString()).ToArray(),
                    JP_val = ca.Select(e => e.FullDetails).ToArray()
                });
                }


                //var Q = db.JobSource

                //    .Include(e => e.PostDetails)
                //    .Include(e => e.Geostruct)

                //    .Where(e => e.Id == data).Select
                //    (e => new
                //    {

                //        Name = e.Name,
                //        ReferenceNo = e.ReferenceNo,
                //        Narration = e.Narration,
                //        initiatedDate = e.initiatedDate,
                //        PublishDate = e.PublishDate,
                //        BatchCloseDate = e.BatchCloseDate,
                //        Action = e.DBTrack.Action
                //    }).ToList();

                //var add_data = db.RecruitBatchInitiator
                //  .Include(e => e.PostDetails)
                //    .Include(e => e.Geostruct)

                //    .Where(e => e.Id == data)
                //    .Select(e => new
                //    {
                //        PostDetails_Full = e.PostDetails.FullDetails == null ? "" : e.PostDetails.FullDetails,
                //        Add_Id = e.PostDetails.Id == null ? "" : e.PostDetails.Id.ToString(),
                //        Cont_Id = e.Geostruct.Id == null ? "" : e.Geostruct.Id.ToString(),
                //        FullGeostructDetails = e.Geostruct.FullDetails == null ? "" : e.Geostruct.FullDetails
                //    }).ToList();


                //var W = db.DT_RecruitBatchInitiator
                //     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                //     (e => new
                //     {
                //         DT_Id = e.Id,

                //         Name = e.Name == null ? "" : e.Name,
                //         ReferenceNo = e.RefferenceNo == null ? "" : e.RefferenceNo,
                //         //BusinessType_Val = e.BusinessType_Id == 0 ? "" : db.LookupValue
                //         //           .Where(x => x.Id == e.BusinessType_Id)
                //         //           .Select(x => x.LookupVal).FirstOrDefault(),

                //         Address_Val = e.PostDetails_Id == 0 ? "" : db.PostDetails.Where(x => x.Id == e.PostDetails_Id).Select(x => x.FullDetails).FirstOrDefault(),
                //         Contact_Val = e.Geostruct_Id == 0 ? "" : db.GeoStruct.Where(x => x.Id == e.Geostruct_Id).Select(x => x.FullDetails).FirstOrDefault()
                //     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var Corp = db.JobSource.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { return_data, return_data2, return_data3, return_data4, Auth, JsonRequestBehavior.AllowGet });
            }
        }
	
    
        [HttpPost]
        public async Task<ActionResult> EditSave(JobSource c, int data, FormCollection form) // Edit submit
        {
  
           // string ContactDetails = form["FacultySpecializationlist"] == "0" ? "" : form["FacultySpecializationlist"];
            //  bool Auth = form["Autho_Action"] == "" ? false : true;
            bool Auth = form["Autho_Allow"] == "true" ? true : false;
            using (DataBaseContext db = new DataBaseContext())
            {
                var db_Data = db.JobSource.Include(e => e.JobAgency).Include(e => e.JobInsideOrg).Include(e => e.JobNewsPaper).Include(e => e.JobPortal)
                     .Where(e => e.Id == data).SingleOrDefault();
                db_Data.JobAgency = null;
                db_Data.JobInsideOrg = null;
                db_Data.JobNewsPaper = null;
                db_Data.JobPortal = null;



                List<JobAgency> job_agency = new List<JobAgency>();
                string j_agency = form["JobAgencylist"];

                if (j_agency != null)
                {
                    var ids = Utility.StringIdsToListIds(j_agency);
                    foreach (var ca in ids)
                    {
                        var Lookup_val = db.JobAgency.Find(ca);

                        job_agency.Add(Lookup_val);
                        db_Data.JobAgency = job_agency;
                    }
                }
                else
                {
                    db_Data.JobAgency = null;
                }

                List<JobInsideOrg> jobinside = new List<JobInsideOrg>();
                string j_inside = form["JobInsideOrglist"];

                if (j_inside != null)
                {
                    var ids = Utility.StringIdsToListIds(j_inside);
                    foreach (var ca in ids)
                    {
                        var Lookup_val = db.JobInsideOrg.Find(ca);

                        jobinside.Add(Lookup_val);
                        db_Data.JobInsideOrg = jobinside;
                    }
                }
                else
                {
                    db_Data.JobInsideOrg = null;
                }

                List<JobNewsPaper> jobnews = new List<JobNewsPaper>();
                string j_news = form["JobNewsPaperlist"];

                if (j_news != null)
                {
                    var ids = Utility.StringIdsToListIds(j_news);
                    foreach (var ca in ids)
                    {
                        var Lookup_val = db.JobNewsPaper.Find(ca);

                        jobnews.Add(Lookup_val);
                        db_Data.JobNewsPaper = jobnews;
                    }
                }
                else
                {
                    db_Data.JobNewsPaper = null;
                }



                List<JobPortal> jobportal = new List<JobPortal>();
                string j_portal = form["JobPortallist"];

                if (j_portal != null)
                {
                    var ids = Utility.StringIdsToListIds(j_portal);
                    foreach (var ca in ids)
                    {
                        var Lookup_val = db.JobPortal.Find(ca);

                        jobportal.Add(Lookup_val);
                        db_Data.JobPortal = jobportal;
                    }
                }
                else
                {
                    db_Data.JobPortal = null;
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
                                    db.JobSource.Attach(db_Data);
                                    db.Entry(db_Data).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    TempData["RowVersion"] = db_Data.RowVersion;
                                    db.Entry(db_Data).State = System.Data.Entity.EntityState.Detached;

                                    var Curr_OBJ = db.JobSource.Find(data);
                                    TempData["CurrRowVersion"] = Curr_OBJ.RowVersion;
                                    db.Entry(Curr_OBJ).State = System.Data.Entity.EntityState.Detached;

                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {
                                        JobSource blog = null; // to retrieve old data
                                        DbPropertyValues originalBlogValues = null;


                                        blog = context.JobSource.Where(e => e.Id == data).Include(e => e.JobAgency)
                                                                .Include(e => e.JobInsideOrg)
                                                                .Include(e => e.JobNewsPaper)
                                                                .Include(e => e.JobPortal).SingleOrDefault();
                                        originalBlogValues = context.Entry(blog).OriginalValues;


                                        c.DBTrack = new DBTrack
                                        {
                                            CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                            CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                            Action = "M",
                                            ModifiedBy = SessionManager.UserName,
                                            ModifiedOn = DateTime.Now
                                        };

                                        JobSource lk = new JobSource
                                        {
                                            Id = data,

                                            JobAgency = db_Data.JobAgency,
                                            JobInsideOrg = db_Data.JobInsideOrg,
                                            JobNewsPaper = db_Data.JobNewsPaper,
                                            JobPortal = db_Data.JobPortal,
                                            DBTrack = c.DBTrack
                                        };


                                        db.JobSource.Attach(lk);
                                        db.Entry(lk).State = System.Data.Entity.EntityState.Modified;

                                        // db.Entry(c.DBTrack).State = System.Data.Entity.EntityState.Modified;
                                        //db.SaveChanges();
                                        db.Entry(lk).OriginalValues["RowVersion"] = TempData["RowVersion"];

                                        var obj = DBTrackFile.DBTrackSave("Recruitement/Recruitement", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                        //DT_JobSource DT_LK = (DT_JobSource)obj;
                                        //  DT_LK.Allergy = lk.Allergy.Select(e => e.Id);
                                        //db.Create(DT_LK);
                                        db.SaveChanges();
                                        await db.SaveChangesAsync();
                                        db.Entry(lk).State = System.Data.Entity.EntityState.Detached;
                                        var aaq = db.JobSource.Include(e => e.JobAgency).Include(e => e.JobInsideOrg).Include(e => e.JobNewsPaper).Include(e => e.JobPortal).Where(e => e.Id == data).SingleOrDefault();
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
                                Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                var databaseValues = (JobSource)databaseEntry.ToObject();
                                c.RowVersion = databaseValues.RowVersion;

                            }
                        }

                        Msg.Add("Record modified by another user.So refresh it and try to save again.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {

                        JobSource blog = null; // to retrieve old data
                        DbPropertyValues originalBlogValues = null;
                        JobSource Old_Corp = null;

                        using (var context = new DataBaseContext())
                        {
                            blog = context.JobSource.Where(e => e.Id == data).SingleOrDefault();
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
                            var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, corp, "JobSource", c.DBTrack);
                            // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                            Old_Corp = context.JobSource.Where(e => e.Id == data).Include(e => e.JobAgency)
                                .Include(e => e.JobInsideOrg).Include(e => e.JobNewsPaper).Include(e => e.JobPortal).SingleOrDefault();
                            db.SaveChanges();
                        }
                        blog.DBTrack = c.DBTrack;
                        db.JobSource.Attach(blog);
                        db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                        db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                        db.SaveChanges();
                        var aaq = db.JobSource.Include(e => e.JobAgency).Include(e => e.JobInsideOrg).Include(e => e.JobNewsPaper).Include(e => e.JobPortal).Where(e => e.Id == data).SingleOrDefault();
                        ts.Complete();
                        Msg.Add("Record Updated Successfully.");
                        return Json(new Utility.JsonReturnClass { Id = aaq.Id, Val = aaq.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }

                }
                return View();
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
                       JobSource jobsource = db.JobSource.Include(e => e.JobAgency)
                           .Include(e => e.JobInsideOrg)
                           .Include(e => e.JobNewsPaper)
                           .Include(e => e.JobPortal).Where(e => e.Id == data).SingleOrDefault();



                       if (jobsource.DBTrack.IsModified == true)
                       {
                           using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                           {
                               //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack, corporates, null, "Corporate");
                               DBTrack dbT = new DBTrack
                               {
                                   Action = "D",
                                   CreatedBy = jobsource.DBTrack.CreatedBy != null ? jobsource.DBTrack.CreatedBy : null,
                                   CreatedOn = jobsource.DBTrack.CreatedOn != null ? jobsource.DBTrack.CreatedOn : null,
                                   IsModified = jobsource.DBTrack.IsModified == true ? true : false
                               };
                               jobsource.DBTrack = dbT;
                               db.Entry(jobsource).State = System.Data.Entity.EntityState.Modified;
                               var rtn_Obj = DBTrackFile.DBTrackSave("Recruitement/Recruitement", null, db.ChangeTracker, jobsource.DBTrack);
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


                               var lkValue1 = new HashSet<int>(jobsource.JobAgency.Select(e => e.Id));
                               if (lkValue1.Count > 0)
                               {
                                   Msg.Add(" Child record exists.Cannot remove it..  ");
                                   return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                               }

                               var lkValue2 = new HashSet<int>(jobsource.JobInsideOrg.Select(e => e.Id));
                               if (lkValue2.Count > 0)
                               {
                                   Msg.Add(" Child record exists.Cannot remove it..  ");
                                   return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                               }

                               var lkValue3 = new HashSet<int>(jobsource.JobNewsPaper.Select(e => e.Id));
                               if (lkValue3.Count > 0)
                               {
                                   Msg.Add(" Child record exists.Cannot remove it..  ");
                                   return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                   //return this.Json(new Object[] { "", "Child record exists.Cannot delete.", JsonRequestBehavior.AllowGet });
                               }
                               var lkValue4 = new HashSet<int>(jobsource.JobPortal.Select(e => e.Id));
                               if (lkValue4.Count > 0)
                               {
                                   Msg.Add(" Child record exists.Cannot remove it..  ");
                                   return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                   //return this.Json(new Object[] { "", "Child record exists.Cannot delete.", JsonRequestBehavior.AllowGet });
                               }

                               db.Entry(jobsource).State = System.Data.Entity.EntityState.Deleted;
                               db.SaveChanges();
                               ts.Complete();

                               Msg.Add("  Data removed successfully.  ");
                               return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                               //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                           }
                       }
                       db.Entry(jobsource).State = System.Data.Entity.EntityState.Deleted;
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

          [HttpPost]
           public ActionResult GetLookupDetails(string data)
           {
               using (DataBaseContext db = new DataBaseContext())
               {
                   var fall = db.JobAgency
                       .Include(e => e.AgencyAddress)
                       .Include(e => e.ContactDetails)
                       .Include(e => e.ContactPerson)
                       
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
          public ActionResult GetLookupJobInsideDetails(string data)
          {
              using (DataBaseContext db = new DataBaseContext())
              {
                  var fall = db.JobInsideOrg
                      .Include(e => e.ContactDetails)
                      .Include(e => e.ContactPerson)
                      .Include(e => e.PortalAddress)

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
          public ActionResult GetLookupJobNewsPaperDetails(string data)
          {
              using (DataBaseContext db = new DataBaseContext())
              {
                  var fall = db.JobNewsPaper
                      .Include(e => e.ContactDetails)
                      .Include(e => e.ContactPerson)
                      .Include(e => e.NewPaperAddress)

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
          public ActionResult GetLookupJobPortalDetails(string data)
          {
              using (DataBaseContext db = new DataBaseContext())
              {
                  var fall = db.JobPortal
                      .Include(e => e.ContactDetails)
                      .Include(e => e.ContactPerson)
                      .Include(e => e.PortalAddress)

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
    }
}