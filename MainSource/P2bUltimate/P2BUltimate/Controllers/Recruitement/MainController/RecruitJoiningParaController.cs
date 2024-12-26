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
using System.Data;


namespace P2BUltimate.Controllers.Recruitment.MainController
{
    public class RecruitJoiningParaController : Controller
    {
        List<string> Msg = new List<string>();

        //private DataBaseContext db = new DataBaseContext();
        //
        // GET: /RecruitJoiningPara/
        public ActionResult Index()
        {
            return View();
        }


        public ActionResult Partial()
        {
            return View("~/Views/Shared/Recruitement/_RecruitJoiningPara.cshtml");
        }
        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult Create(RecruitJoiningPara COBJ, FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string Category = form["RecruitJoinParalist"] == "0" ? "" : form["RecruitJoinParalist"];


                    if (Category != null)
                    {
                        if (Category != "")
                        {
                            var val = db.LookupValue.Find(int.Parse(Category));
                            COBJ.RecruitJoinPara = val;
                        }
                    }

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            //if (db.EmpSocialInfo.Any(o => o.Category == COBJ.Category))
                            //{                       
                            //    return this.Json(new Object[] { null, null, "Category already exists.", JsonRequestBehavior.AllowGet });
                            //}

                            COBJ.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            //look.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName };

                            RecruitJoiningPara EmpSocialInfo = new RecruitJoiningPara()
                            {
                                RecruitJoinPara = COBJ.RecruitJoinPara,
                                Stage = COBJ.Stage,

                                DBTrack = COBJ.DBTrack
                            };
                            try
                            {
                                db.RecruitJoiningPara.Add(EmpSocialInfo);
                                var rtn_Obj = DBTrackFile.DBTrackSave("Recruitment/Recruitment", null, db.ChangeTracker, COBJ.DBTrack);
                                //DT_RecruitJoiningPara DT_OBJ = (DT_RecruitJoiningPara)rtn_Obj;
                                //DT_OBJ.RecruitJoinPara_Id = COBJ.RecruitJoinPara == null ? 0 : COBJ.RecruitJoinPara.Id;
                                //db.Create(DT_OBJ);
                                db.SaveChanges();

                                ts.Complete();

                                Msg.Add("  Data Created successfully.  ");
                                return Json(new Utility.JsonReturnClass { Id = EmpSocialInfo.Id, Val = EmpSocialInfo.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }

                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = COBJ.Id });
                            }
                            catch (DataException /* dex */)
                            {
                                Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
                var Q = db.RecruitJoiningPara
                     .Include(e => e.RecruitJoinPara)

             .Where(e => e.Id == data).Select
             (e => new
             {

                 RecruitJoinPara_Id = e.RecruitJoinPara.Id == null ? 0 : e.RecruitJoinPara.Id,
                 Stage = e.Stage,
                 Action = e.DBTrack.Action
             }).ToList();


                var Corp = db.RecruitJoiningPara.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, Auth, JsonRequestBehavior.AllowGet });
            }
        }

        [HttpPost]
        public ActionResult GetLookupDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                    var fall = db.RecruitJoiningPara
                        .Include(e=>e.RecruitJoinPara)

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
        public async Task<ActionResult> EditSave(RecruitJoiningPara c, int data, FormCollection form) // Edit submit
        {
            // string ContactDetails = form["FacultySpecializationlist"] == "0" ? "" : form["FacultySpecializationlist"];
            //  bool Auth = form["Autho_Action"] == "" ? false : true;
            using (DataBaseContext db = new DataBaseContext())
            {
                bool Auth = form["Autho_Allow"] == "true" ? true : false;
                string Corp = form["RecruitJoinParalist"] == "0" ? "" : form["RecruitJoinParalist"];

                var db_Data = db.RecruitJoiningPara.Include(e => e.RecruitJoinPara)
                     .Where(e => e.Id == data).SingleOrDefault();
                db_Data.RecruitJoinPara = null;
                db_Data.Stage = c.Stage;

                if (Corp != null)
                {
                    if (Corp != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(Corp));
                        db_Data.RecruitJoinPara = val;
                    }
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
                                    db.RecruitJoiningPara.Attach(db_Data);
                                    db.Entry(db_Data).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    TempData["RowVersion"] = db_Data.RowVersion;
                                    db.Entry(db_Data).State = System.Data.Entity.EntityState.Detached;

                                    var Curr_OBJ = db.RecruitJoiningPara.Find(data);
                                    TempData["CurrRowVersion"] = Curr_OBJ.RowVersion;
                                    db.Entry(Curr_OBJ).State = System.Data.Entity.EntityState.Detached;

                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {
                                        RecruitJoiningPara blog = null; // to retrieve old data
                                        DbPropertyValues originalBlogValues = null;


                                        blog = context.RecruitJoiningPara.Where(e => e.Id == data).Include(e => e.RecruitJoinPara).SingleOrDefault();
                                        originalBlogValues = context.Entry(blog).OriginalValues;


                                        c.DBTrack = new DBTrack
                                        {
                                            CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                            CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                            Action = "M",
                                            ModifiedBy = SessionManager.UserName,
                                            ModifiedOn = DateTime.Now
                                        };

                                        RecruitJoiningPara lk = new RecruitJoiningPara
                                        {
                                            Id = data,

                                            RecruitJoinPara = db_Data.RecruitJoinPara,
                                            Stage = db_Data.Stage,

                                            DBTrack = c.DBTrack
                                        };


                                        db.RecruitJoiningPara.Attach(lk);
                                        db.Entry(lk).State = System.Data.Entity.EntityState.Modified;

                                        // db.Entry(c.DBTrack).State = System.Data.Entity.EntityState.Modified;
                                        //db.SaveChanges();
                                        db.Entry(lk).OriginalValues["RowVersion"] = TempData["RowVersion"];

                                        var obj = DBTrackFile.DBTrackSave("Recruitment/Recruitment", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                        // DT_RecruitJoiningPara DT_LK = (DT_RecruitJoiningPara)obj;
                                        // DT_LK. = lk.Allergy.Select(e => e.Id);
                                        //db.Create(DT_LK);
                                        db.SaveChanges();
                                        await db.SaveChangesAsync();
                                        db.Entry(lk).State = System.Data.Entity.EntityState.Detached;
                                        var aaq = db.RecruitJoiningPara.Include(e => e.RecruitJoinPara).Where(e => e.Id == data).SingleOrDefault();
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
                            var clientValues = (RecruitEvaluationPara)entry.Entity;
                            var databaseEntry = entry.GetDatabaseValues();
                            if (databaseEntry == null)
                            {
                                return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                            }
                            else
                            {
                                var databaseValues = (RecruitEvaluationPara)databaseEntry.ToObject();
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

                        RecruitJoiningPara blog = null; // to retrieve old data
                        DbPropertyValues originalBlogValues = null;
                        RecruitJoiningPara Old_Corp = null;

                        using (var context = new DataBaseContext())
                        {
                            blog = context.RecruitJoiningPara.Where(e => e.Id == data).SingleOrDefault();
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

                        RecruitJoiningPara corp = new RecruitJoiningPara()
                        {

                            Id = data,
                            RecruitJoinPara = blog.RecruitJoinPara,
                            Stage = blog.Stage,
                            DBTrack = c.DBTrack,
                            RowVersion = (Byte[])TempData["RowVersion"]
                        };


                        //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                        using (var context = new DataBaseContext())
                        {
                            var obj = DBTrackFile.ModifiedDataHistory("Recruitement/Recruitement", "M", blog, corp, "JobSource", c.DBTrack);
                            // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                            //Old_Corp = context.RecruitJoiningPara.Where(e => e.Id == data).Include(e => e.RecruitJoinPara)
                            //    .Include(e => e.Stage).SingleOrDefault();
                            //DT_RecruitJoiningPara DT_Corp = (DT_RecruitJoiningPara)obj;
                            //DT_Corp.RecruitJoinPara_Id = DBTrackFile.ValCompare(Old_Corp.RecruitJoinPara, c.RecruitJoinPara);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
                            //db.Create(DT_Corp);
                            db.SaveChanges();
                        }
                        blog.DBTrack = c.DBTrack;
                        db.RecruitJoiningPara.Attach(blog);
                        db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                        db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                        db.SaveChanges();
                        var aaq = db.RecruitJoiningPara.Include(e => e.RecruitJoinPara).Where(e => e.Id == data).SingleOrDefault();
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
                    RecruitJoiningPara recruitEva = db.RecruitJoiningPara.Include(e => e.RecruitJoinPara).Where(e => e.Id == data).SingleOrDefault();

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
                            DT_RecruitJoiningPara DT_OBJ = (DT_RecruitJoiningPara)rtn_Obj;
                            db.Create(DT_OBJ);

                            await db.SaveChangesAsync();
                            ts.Complete();

                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
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