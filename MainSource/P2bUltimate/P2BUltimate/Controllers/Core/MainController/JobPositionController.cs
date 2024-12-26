///
/// Created by Tanushri
///

using System;
using System.Collections.Generic;
using System.Web;
using System.Linq;
using System.Web.Mvc;
using System.Net;
using System.Data.Entity;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Transactions;
using P2b.Global;
using System.Text;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using System.Threading.Tasks;
using P2BUltimate.Security;
namespace P2BUltimate.Controllers.Core.MainController
{
     [AuthoriseManger]
    public class JobPositionController : Controller
    {
        //
        // GET: /JobPosition/

        public ActionResult Index()
        {
            return View();
        }

        //private DataBaseContext db = new DataBaseContext();


        /*---------------------------------------------------------- Create ---------------------------------------------- */
        [HttpPost]
        public ActionResult CreateSave1(JobPosition jp)
          {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            if (db.JobPosition.Any(o => o.JobPositionCode == jp.JobPositionCode))
                            {
                                Msg.Add("  Position Already Exists.  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                //return Json(new Object[] { "", "", "Position Already Exists.", JsonRequestBehavior.AllowGet });
                            }

                            jp.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName };

                            JobPosition jps = new JobPosition()
                             {
                                 JobPositionCode = jp.JobPositionCode,
                                 DBTrack = jp.DBTrack
                             };

                            try
                            {
                                db.JobPosition.Add(jps);
                                //  DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, jp.DBTrack);
                                db.SaveChanges();
                                ts.Complete();
                                //return this.Json(new Object[]{"","", "Data Saved Successfully.",JsonRequestBehavior.AllowGet });    
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = jps.Id, Val = jps.JobPositionCode, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                // return Json(new Object[] { jps.Id, jps.JobPositionCode, "Data saved successfully." }, JsonRequestBehavior.AllowGet);
                                // using (TransactionScope ts = new TransactionScope())
                                // {
                                //     db.JobPosition.Add(jps);
                                //     db.SaveChanges();
                                //     ts.Complete();
                                // }
                                //// return Json(new {jps.Id,jp.Position, "Data saved successfully." }, JsonRequestBehavior.AllowGet);
                                // return Json(new Object[] { jps.Id, jps.Position, "Data saved successfully." }, JsonRequestBehavior.AllowGet);

                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = jp.Id });
                            }
                            catch (DataException /* dex */)
                            {
                                Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                // return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
                            }
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

                        // return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
                        //return this.Json(new { msg = errorMsg });
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

        /* ------------------------------------- Edit Save ------------------------------------------------- */

        public ActionResult EditPostion_partial(int? data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                var Q = db.JobPosition
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        JobPositionCode = e.JobPositionCode,
                        JobPositionDesc = e.JobPositionDesc,
                        Action = e.DBTrack.Action
                    }).ToList();
                var Corp = db.JobPosition.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, Auth, JsonRequestBehavior.AllowGet });
            }
        }



        public ActionResult EditSave1(JobPosition jp, int data)
        {
            	List<string> Msg = new List<string>();
                using (DataBaseContext db = new DataBaseContext())
                {
                    try
                    {
                        if (ModelState.IsValid)
                        {
                            using (TransactionScope ts = new TransactionScope())
                            {

                                JobPosition jbp = new JobPosition()
                                {
                                    JobPositionCode = jp.JobPositionCode,
                                    Id = data
                                };
                                try
                                {
                                    db.Entry(jbp).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    ts.Complete();

                                    // return this.Json(new Object[] { jbp.Id, jbp.Position, "Data Saved SucessFully" }, JsonRequestBehavior.AllowGet);
                                    Msg.Add("  Data Saved successfully  ");
                                    return Json(new Utility.JsonReturnClass { Id = jbp.Id, Val = jbp.JobPositionCode, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    //return this.Json(new Object[] { jbp.Id, jbp.JobPositionCode, "Data Saved SucessFully" }, JsonRequestBehavior.AllowGet);
                                }
                                catch (DbUpdateConcurrencyException)
                                {
                                    return RedirectToAction("Create", new { concurrencyError = true, id = jbp.Id });
                                }
                                catch (DataException /* dex */)
                                {

                                    Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    // return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator.", JsonRequestBehavior.AllowGet });
                                }
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

                            // return this.Json(new { msg = errorMsg, JsonRequestBehavior.AllowGet });
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


        /*---------------------------------------------- Delete ------------------------------------------------- */

        public ActionResult DeleteJobPostion(int? data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var qurey = db.JobPosition.Find(data);
                    using (TransactionScope ts = new TransactionScope())
                    {
                        db.JobPosition.Attach(qurey);
                        db.Entry(qurey).State = System.Data.Entity.EntityState.Deleted;
                        db.SaveChanges();
                        db.Entry(qurey).State = System.Data.Entity.EntityState.Detached;
                        ts.Complete();
                    }
                    Msg.Add("  Record Deleted");
                    return Json(new Utility.JsonReturnClass { Id = qurey.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    //return Json(new Object[] { qurey.Id, "Record Deleted", JsonRequestBehavior.AllowGet });
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
        public ActionResult edit(int? data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var JobPosition = db.JobPosition
                                    .Where(e => e.Id == data).ToList();
                var r = (from ca in JobPosition
                         select new
                         {
                             Id = ca.Id,
                             JobPositionCode = ca.JobPositionCode,
                             JobPositionDesc = ca.JobPositionDesc,
                             Action = ca.DBTrack.Action
                         }).Distinct();

                var a = "";

                var W = db.DT_JobPosition
                     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id,
                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();
                var LKup = db.JobPosition.Find(data);
                TempData["RowVersion"] = LKup.RowVersion;
                var Auth = LKup.DBTrack.IsModified;

                return this.Json(new Object[] { r, a, W, Auth, JsonRequestBehavior.AllowGet });
            }
        }


        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult Create(JobPosition OBJ, FormCollection form) //Create submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    int comp_Id = 0;
                    comp_Id = Convert.ToInt32(Session["CompId"]);
                    var Company = new Company();
                    Company = db.Company.Where(e => e.Id == comp_Id).SingleOrDefault();

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            if (db.JobPosition.Any(o => o.Id == OBJ.Id))
                            {
                                Msg.Add("  Code Already Exists.  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                // return Json(new Object[] { "", "", "Code Already Exists.", JsonRequestBehavior.AllowGet });
                            }
                            OBJ.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName };
                            JobPosition COBJ = new JobPosition()
                            {
                                Id = OBJ.Id,
                                JobPositionCode = OBJ.JobPositionCode,
                                JobPositionDesc = OBJ.JobPositionDesc,
                                DBTrack = OBJ.DBTrack
                            };
                            try
                            {
                                db.JobPosition.Add(COBJ);
                                db.SaveChanges();
                                //DBTrackFile.DBTrackSave("Core/P2b.Global", "C", COBJ, null, "JobPosition", null);
                                if (Company != null)
                                {
                                    var Objjob = new List<JobPosition>();
                                    Objjob.Add(COBJ);
                                    Company.JobPosition = Objjob;
                                    db.Company.Attach(Company);
                                    db.Entry(Company).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    db.Entry(Company).State = System.Data.Entity.EntityState.Detached;

                                }

                                ts.Complete();
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = COBJ.Id, Val = COBJ.JobPositionDesc, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                // return this.Json(new Object[] { COBJ.Id, COBJ.JobPositionDesc, "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = OBJ.Id });
                            }
                            catch (DataException /* dex */)
                            {
                                Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                // return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
                            }
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

                        //return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
                        //return this.Json(new { msg = errorMsg });
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

        public int EditS(int data, JobPosition EOBJ, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                var CurOBJ = db.JobPosition.Find(data);
                TempData["CurrRowVersion"] = CurOBJ.RowVersion;
                db.Entry(CurOBJ).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    EOBJ.DBTrack = dbT;
                    JobPosition OBJ = new JobPosition()
                    {
                        Id = data,
                        JobPositionCode = EOBJ.JobPositionCode,
                        JobPositionDesc = EOBJ.JobPositionDesc,
                        DBTrack = EOBJ.DBTrack
                    };

                    db.JobPosition.Attach(OBJ);
                    db.Entry(OBJ).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(OBJ).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    return 1;
                }
                return 0;
            }
        }


        [HttpPost]
        public async Task<ActionResult> EditSave(JobPosition ESOBJ, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
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
                                    JobPosition blog = null; // to retrieve old data
                                    // DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.JobPosition.Where(e => e.Id == data).AsNoTracking().SingleOrDefault();
                                        // originalBlogValues = context.Entry(blog).OriginalValues;
                                    }

                                    ESOBJ.DBTrack = new DBTrack
                                    {
                                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };

                                    int a = EditS(data, ESOBJ, ESOBJ.DBTrack);

                                    await db.SaveChangesAsync();

                                    using (var context = new DataBaseContext())
                                    {
                                        //To save data in history table 
                                        var Obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, ESOBJ, "JobPosition", ESOBJ.DBTrack);
                                        DT_JobPosition DT_OBJ = (DT_JobPosition)Obj;
                                        db.DT_JobPosition.Add(DT_OBJ);
                                        db.SaveChanges();
                                    }
                                    ts.Complete();
                                    // return Json(new Object[] { data, ESOBJ.JobPositionDesc, "Record Updated" }, JsonRequestBehavior.AllowGet);
                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = data, Val = ESOBJ.JobPositionDesc, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (JobPosition)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    // return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (JobPosition)databaseEntry.ToObject();
                                    ESOBJ.RowVersion = databaseValues.RowVersion;

                                }
                            }
                            Msg.Add("Record modified by another user.So refresh it and try to save again.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            // return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            JobPosition Old_OBJ = db.JobPosition.Where(e => e.Id == data).SingleOrDefault();

                            JobPosition Curr_OBJ = ESOBJ;
                            ESOBJ.DBTrack = new DBTrack
                            {
                                CreatedBy = Old_OBJ.DBTrack.CreatedBy == null ? null : Old_OBJ.DBTrack.CreatedBy,
                                CreatedOn = Old_OBJ.DBTrack.CreatedOn == null ? null : Old_OBJ.DBTrack.CreatedOn,
                                Action = "M",
                                IsModified = Old_OBJ.DBTrack.IsModified == true ? true : false,
                                //ModifiedBy = SessionManager.UserName,
                                //ModifiedOn = DateTime.Now
                            };
                            Old_OBJ.DBTrack = ESOBJ.DBTrack;

                            db.Entry(Old_OBJ).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            using (var context = new DataBaseContext())
                            {
                                //DBTrackFile.DBTrackSave("Core/P2b.Global", "M", Old_OBJ, Curr_OBJ, "JobPosition", ESOBJ.DBTrack);
                            }

                            ts.Complete();
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = Old_OBJ.Id, Val = ESOBJ.JobPositionCode, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            // return Json(new Object[] { Old_OBJ.Id, ESOBJ.JobPositionCode, "Record Updated", JsonRequestBehavior.AllowGet });
                        }

                    }
                    return View();
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
