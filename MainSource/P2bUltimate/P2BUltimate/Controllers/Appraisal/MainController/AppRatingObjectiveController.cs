
using P2b.Global;
using Payroll;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Threading.Tasks;
using P2BUltimate.Security;
using Appraisal;

namespace P2BUltimate.Controllers.Appraisal.MainController
{
    public class AppRatingObjectiveController : Controller
    {

        //private DataBaseContext db = new DataBaseContext();

        public ActionResult partial()
        {
            return View("~/Views/Shared/Appraisal/_AppRatingObjective.cshtml");
        }

        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult Create(AppRatingObjective p, FormCollection form) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string Type = form["ObjectiveWordingslist"] == "0" ? "" : form["ObjectiveWordingslist"];

                List<String> Msg = new List<String>();
                try
                {

                    if (Type != null && Type != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(Type));
                        p.ObjectiveWordings = val;
                    }

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            p.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };                     //???????
                            //if (db.AppRatingObjective.Include(a=>a.ObjectiveWordings).Any(a=>a.ObjectiveWordings.Id==p.ObjectiveWordings.Id))
                            //{
                            //    Msg.Add("Objective Wordings already defined.");
                            // return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet); 
                            //}
                            AppRatingObjective AppRatingobjective = new AppRatingObjective()
                            {
                                RatingObjectName = p.RatingObjectName,
                                RatingPoints = p.RatingPoints,
                                RatingPointsFrom = p.RatingPointsFrom,
                                RatingPointsTo = p.RatingPointsTo,
                                ObjectiveWordings = p.ObjectiveWordings,
                                DBTrack = p.DBTrack
                            };

                            db.AppRatingObjective.Add(AppRatingobjective);
                            var rtn_Obj = DBTrackFile.DBTrackSave("Appraisal/Appraisal", null, db.ChangeTracker, p.DBTrack);

                            //DT_AppCategory DT_AppC = (DT_AppCategory)rtn_Obj;
                            //DT_AppC.AppMode_Id = p.AppMode == null ? 0 : p.AppMode.Id;
                            //db.Create(DT_AppC);
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("Data Saved Successfully.");
                            return Json(new Utility.JsonReturnClass { Id = AppRatingobjective.Id, Val = AppRatingobjective.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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

        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {

            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    AppRatingObjective corporates = db.AppRatingObjective.Include(e => e.ObjectiveWordings).Where(e => e.Id == data).SingleOrDefault();

                    LookupValue val = corporates.ObjectiveWordings;
                    // FuncStruct fun = corporates.FuncStruct;
                    if (corporates.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack, corporates, null, "Corporate");
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = corporates.DBTrack.CreatedBy != null ? corporates.DBTrack.CreatedBy : null,
                                CreatedOn = corporates.DBTrack.CreatedOn != null ? corporates.DBTrack.CreatedOn : null,
                                IsModified = corporates.DBTrack.IsModified == true ? true : false
                            };
                            corporates.DBTrack = dbT;
                            db.Entry(corporates).State = System.Data.Entity.EntityState.Modified;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Appraisal/Appraisal", null, db.ChangeTracker, corporates.DBTrack);
                            DT_AppRatingObjective DT_Corp = (DT_AppRatingObjective)rtn_Obj;
                            DT_Corp.ObjectiveWordings_Id = corporates.ObjectiveWordings == null ? 0 : corporates.ObjectiveWordings.Id;
                            db.Create(DT_Corp);

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
                                CreatedBy = corporates.DBTrack.CreatedBy != null ? corporates.DBTrack.CreatedBy : null,
                                CreatedOn = corporates.DBTrack.CreatedOn != null ? corporates.DBTrack.CreatedOn : null,
                                IsModified = corporates.DBTrack.IsModified == true ? false : false//,
                                //AuthorizedBy = SessionManager.UserName,
                                //AuthorizedOn = DateTime.Now
                            };

                            // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };

                            db.Entry(corporates).State = System.Data.Entity.EntityState.Deleted;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Appraisal/Appraisal", null, db.ChangeTracker, dbT);
                            DT_AppRatingObjective DT_Corp = (DT_AppRatingObjective)rtn_Obj;
                            DT_Corp.ObjectiveWordings_Id = val == null ? 0 : val.Id;
                            db.Create(DT_Corp);

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
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.AppRatingObjective
                    .Include(e => e.ObjectiveWordings)
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        RatingObjectName = e.RatingObjectName,
                        RatingPoints = e.RatingPoints,
                        RatingPointsFrom = e.RatingPointsFrom,
                        RatingPointsTo = e.RatingPointsTo,
                        ObjectiveWordings_Id = e.ObjectiveWordings.Id == null ? 0 : e.ObjectiveWordings.Id,
                        Action = e.DBTrack.Action
                    }).ToList();



                var W = db.DT_AppRatingObjective
                     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id,
                         RatingPoints = e.RatingPoints,
                         ObjectiveWordings_Val = e.ObjectiveWordings_Id == 0 ? "" : db.LookupValue
                                    .Where(x => x.Id == e.ObjectiveWordings_Id)
                                    .Select(x => x.LookupVal).FirstOrDefault(),
                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var Corp = db.AppRatingObjective.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, "", W, Auth, JsonRequestBehavior.AllowGet });
            }
        }

        [HttpPost]
        public async Task<ActionResult> EditSave(AppRatingObjective L, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
                try
                {
                    string ObjectiveWordings_Id = form["ObjectiveWordingslist"] == "0" ? "" : form["ObjectiveWordingslist"];
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;

                    if (ModelState.IsValid)
                    {
                        try
                        {
                            {
                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    AppRatingObjective blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.AppRatingObjective.Where(e => e.Id == data).Include(e => e.ObjectiveWordings).SingleOrDefault();
                                        originalBlogValues = context.Entry(blog).OriginalValues;
                                    }
                                    //var blog1 = db.AppRatingObjective.Include(q => q.ObjectiveWordings).Where(e => e.Id == data).SingleOrDefault();

                                    L.DBTrack = new DBTrack
                                    {
                                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };

                                    AppRatingObjective typedetails = null;
                                    if (ObjectiveWordings_Id != null && ObjectiveWordings_Id != "")
                                    {
                                        var val = db.LookupValue.Find(int.Parse(ObjectiveWordings_Id));
                                        L.ObjectiveWordings = val;

                                        var type = db.AppRatingObjective.Include(e => e.ObjectiveWordings).Where(e => e.Id == data).SingleOrDefault();

                                        if (type.ObjectiveWordings != null)
                                        {
                                            typedetails = db.AppRatingObjective.Where(x => x.ObjectiveWordings.Id == type.ObjectiveWordings.Id && x.Id == data).SingleOrDefault();
                                        }
                                        else
                                        {
                                            typedetails = db.AppRatingObjective.Where(x => x.Id == data).SingleOrDefault();
                                        }
                                        typedetails.ObjectiveWordings = L.ObjectiveWordings;
                                    }
                                    else
                                    {
                                        typedetails = db.AppRatingObjective.Include(e => e.ObjectiveWordings).Where(x => x.Id == data).SingleOrDefault();
                                        typedetails.ObjectiveWordings = null;
                                    }
                                    db.AppRatingObjective.Attach(typedetails);
                                    db.Entry(typedetails).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    TempData["RowVersion"] = typedetails.RowVersion;
                                    db.Entry(typedetails).State = System.Data.Entity.EntityState.Detached;

                                    var CurCorp = db.AppRatingObjective.Find(data);
                                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {
                                        L.DBTrack = blog.DBTrack;
                                        AppRatingObjective post = new AppRatingObjective()
                                        {
                                            ObjectiveWordings = L.ObjectiveWordings,
                                            RatingObjectName = L.RatingObjectName,
                                            RatingPoints = L.RatingPoints,
                                            RatingPointsFrom = L.RatingPointsFrom,
                                            RatingPointsTo = L.RatingPointsTo,
                                            Id = data,
                                            DBTrack = L.DBTrack
                                        };
                                        db.AppRatingObjective.Attach(post);
                                        db.Entry(post).State = System.Data.Entity.EntityState.Modified;

                                        db.Entry(post).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                        db.SaveChanges();
                                        var blog1 = db.AppRatingObjective.Include(e => e.ObjectiveWordings).Where(e => e.Id == data).SingleOrDefault();
                                        // db.Entry(post).State = System.Data.Entity.EntityState.Detached;
                                        ts.Complete();



                                        Msg.Add("  Record Updated");
                                        return Json(new Utility.JsonReturnClass { Id = data, Val = blog1.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        // return Json(new Object[] { L.Id, L.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });

                                    }
                                }
                            }
                        }
                        catch (DbUpdateConcurrencyException ex)
                        {
                            var entry = ex.Entries.Single();
                            var clientValues = (AppRatingObjective)entry.Entity;
                            var databaseEntry = entry.GetDatabaseValues();
                            if (databaseEntry == null)
                            {
                                Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                            }
                            else
                            {
                                var databaseValues = (AppRatingObjective)databaseEntry.ToObject();
                                L.RowVersion = databaseValues.RowVersion;

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

    }
}