///
/// Created by Sarika
///

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
using P2BUltimate.Security;

namespace P2BUltimate.Controllers.Core.MainController
{
    [AuthoriseManger]
    public class PromoPolicyController : Controller
    {

        //private DataBaseContext db = new DataBaseContext();

        //
        // GET: /PromoPolicy/
        public ActionResult Index()
        {
            return View("~/Views/Core/MainViews/PromoPolicy/Index.cshtml");
        }


        public ActionResult CreatePromoActivity_partial()
        {
            return View("~/Views/Shared/Core/_PromoPolicy.cshtml");
        }

        [HttpPost]
        public ActionResult Create(PromoPolicy c, FormCollection form) //Create submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string IncrActivity = form["IncrActivitylist"] == "0" ? "" : form["IncrActivitylist"];
                    string IsNewScaleAction = form["IsNewScaleAction"] == "0" ? "" : form["IsNewScaleAction"];
                    string IsOldScaleAction = form["IsOldScaleAction"] == "0" ? "" : form["IsOldScaleAction"];

                    c.IsNewScaleIncrAction = Convert.ToBoolean(IsNewScaleAction);
                    c.IsOldScaleIncrAction = Convert.ToBoolean(IsOldScaleAction);
                    if (IncrActivity != null)
                    {
                        if (IncrActivity != "")
                        {
                            var val = db.IncrActivity.Find(int.Parse(IncrActivity));
                            c.IncrActivity = val;
                        }
                    }
                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            if (db.PromoPolicy.Any(o => o.Name == c.Name))
                            {
                                Msg.Add("  Code Already Exists.  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Object[] { "", "", "Name Already Exists.", JsonRequestBehavior.AllowGet });
                            }

                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            PromoPolicy PromoPolicy = new PromoPolicy()
                            {
                                IncrActivity = c.IncrActivity,
                                IsActionDateAsIncrDate = c.IsActionDateAsIncrDate,
                                IsFuncStructChange = c.IsFuncStructChange,
                                IsNewScaleIncrAction = c.IsNewScaleIncrAction,
                                IsOldScaleIncrAction = c.IsOldScaleIncrAction,
                                IsPayJobStatusChange = c.IsPayJobStatusChange,
                                IsPayStructChange = c.IsPayStructChange,
                                Name = c.Name,
                                DBTrack = c.DBTrack
                            };
                            try
                            {
                                db.PromoPolicy.Add(PromoPolicy);
                                var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, c.DBTrack);
                                DT_PromoPolicy DT_Corp = (DT_PromoPolicy)rtn_Obj;
                                db.Create(DT_Corp);
                                db.SaveChanges();
                                ts.Complete();
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = PromoPolicy.Id, Val = PromoPolicy.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return this.Json(new Object[] { PromoPolicy.Id, PromoPolicy.FullDetails, "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = c.Id });
                            }
                            catch (DataException /* dex */)
                            {
                                Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
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
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    var Q = db.PromoPolicy
                        .Where(e => e.Id == data).Select
                        (e => new
                        {

                            IsActionDateAsIncrDate = e.IsActionDateAsIncrDate,
                            IsFuncStructChange = e.IsFuncStructChange,
                            IsNewScaleIncrAction = e.IsNewScaleIncrAction,
                            IsOldScaleIncrAction = e.IsOldScaleIncrAction,
                            IsPayJobStatusChange = e.IsPayJobStatusChange,
                            IsPayStructChange = e.IsPayStructChange,
                            IncrActivity = e.IncrActivity,

                            Name = e.Name,
                            Action = e.DBTrack.Action
                        }).ToList();

                    var add_data = db.PromoPolicy.Include(e => e.IncrActivity)
                                   .Where(e => e.Id == data).Select
                                   (e => new
                                   {
                                       IncrActivity_FullDetails = e.IncrActivity.FullDetails == null ? " " : e.IncrActivity.FullDetails,
                                       IncrActivity_Id = e.IncrActivity.Id == null ? "" : e.IncrActivity.Id.ToString(),

                                   }).ToList();

                    var W = db.DT_PromoPolicy
                         .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                         (e => new
                         {
                             DT_Id = e.Id,
                             IsActionDateAsIncrDate = e.IsActionDateAsIncrDate,
                             IsFuncStructChange = e.IsFuncStructChange,
                             IsNewScaleIncrAction = e.IsNewScaleIncrAction,
                             IsOldScaleIncrAction = e.IsOldScaleIncrAction,
                             IsPayJobStatusChange = e.IsPayJobStatusChange,
                             IsPayStructChange = e.IsPayStructChange,
                             Name = e.Name,
                         }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                    var Corp = db.PromoPolicy.Find(data);
                    TempData["RowVersion"] = Corp.RowVersion;
                    var Auth = Corp.DBTrack.IsModified;
                    return Json(new Object[] { Q, add_data, W, Auth, JsonRequestBehavior.AllowGet });
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
        public async Task<ActionResult> EditSave(PromoPolicy c, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;
                    string IncrActivity = form["IncrActivitylist"] == "0" ? "" : form["IncrActivitylist"];

                    string IsNewScaleAction = form["IsNewScaleAction"] == "0" ? "" : form["IsNewScaleAction"];
                    string IsOldScaleAction = form["IsOldScaleAction"] == "0" ? "" : form["IsOldScaleAction"];

                    c.IsNewScaleIncrAction = Convert.ToBoolean(IsNewScaleAction);
                    c.IsOldScaleIncrAction = Convert.ToBoolean(IsOldScaleAction);

                    //  c.IncrActivity_Id = IncrActivity != null && IncrActivity != "" ? int.Parse(IncrActivity) : 0;
                    if (IncrActivity != null && IncrActivity != "")
                    {
                        c.IncrActivity_Id = int.Parse(IncrActivity);
                    }
                    else
                    {
                        c.IncrActivity_Id = null;
                    }
                    if (IncrActivity != null)
                    {
                        if (IncrActivity != "")
                        {
                            var val = db.IncrActivity.Find(int.Parse(IncrActivity));
                            c.IncrActivity = val;

                            var type = db.PromoPolicy.Include(e => e.IncrActivity).Where(e => e.Id == data).SingleOrDefault();
                            IList<PromoPolicy> typedetails = null;
                            if (type.IncrActivity != null)
                            {
                                typedetails = db.PromoPolicy.Where(x => x.IncrActivity.Id == type.IncrActivity.Id && x.Id == data).ToList();
                            }
                            else
                            {
                                typedetails = db.PromoPolicy.Where(x => x.Id == data).ToList();
                            }
                            //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                            foreach (var s in typedetails)
                            {
                                s.IncrActivity = c.IncrActivity;
                                db.PromoPolicy.Attach(s);
                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                await db.SaveChangesAsync();
                                db.SaveChanges();
                                TempData["RowVersion"] = s.RowVersion;
                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                            }
                        }
                        else
                        {
                            var IncrDetails = db.PromoPolicy.Include(e => e.IncrActivity).Where(x => x.Id == data).ToList();
                            foreach (var s in IncrDetails)
                            {
                                s.IncrActivity = null;
                                db.PromoPolicy.Attach(s);
                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                await db.SaveChangesAsync();
                                db.SaveChanges();
                                TempData["RowVersion"] = s.RowVersion;
                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                            }
                        }
                    }
                    else
                    {
                        var IncrDetails = db.PromoPolicy.Include(e => e.IncrActivity).Where(x => x.Id == data).ToList();
                        foreach (var s in IncrDetails)
                        {
                            s.IncrActivity = null;
                            db.PromoPolicy.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                    if (Auth == false)
                    {


                        //if (ModelState.IsValid)
                        //{
                        //    try
                        //    {
                        //        //DbContextTransaction transaction = db.Database.BeginTransaction();

                        //        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        //        {
                        //            PromoPolicy blog = null; // to retrieve old data
                        //            DbPropertyValues originalBlogValues = null;

                        //            using (var context = new DataBaseContext())
                        //            {
                        //                blog = context.PromoPolicy.Where(e => e.Id == data).SingleOrDefault();
                        //                originalBlogValues = context.Entry(blog).OriginalValues;
                        //            }


                        //            c.DBTrack = new DBTrack
                        //            {
                        //                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                        //                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                        //                Action = "M",
                        //                ModifiedBy = SessionManager.UserName,
                        //                ModifiedOn = DateTime.Now
                        //            };
                        //            var m1 = db.PromoPolicy.Where(e => e.Id == data).ToList();
                        //            foreach (var s in m1)
                        //            {
                        //                // s.AppraisalPeriodCalendar = null;
                        //                db.PromoPolicy.Attach(s);
                        //                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //                //await db.SaveChangesAsync();
                        //                db.SaveChanges();
                        //                TempData["RowVersion"] = s.RowVersion;
                        //                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        //            }
                        //            var CurCorp = db.PromoPolicy.Find(data);
                        //            TempData["CurrRowVersion"] = CurCorp.RowVersion;
                        //            db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                        //            if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                        //            {
                        //               // c.DBTrack = dbT;
                        //                PromoPolicy corp = new PromoPolicy()
                        //                {
                        //                    IncrActivity = c.IncrActivity,
                        //                    IsActionDateAsIncrDate = c.IsActionDateAsIncrDate,
                        //                    IsFuncStructChange = c.IsFuncStructChange,
                        //                    IsNewScaleIncrAction = c.IsNewScaleIncrAction,
                        //                    IsOldScaleIncrAction = c.IsOldScaleIncrAction,
                        //                    IsPayJobStatusChange = c.IsPayJobStatusChange,
                        //                    IsPayStructChange = c.IsPayStructChange,
                        //                    Name = c.Name,
                        //                    Id = data,
                        //                    DBTrack = c.DBTrack
                        //                };

                        //                db.PromoPolicy.Attach(corp);
                        //                db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                        //                db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                        //                //return 1;
                        //            }
                        //            if (IncrActivity != null)
                        //            {
                        //                if (IncrActivity != "")
                        //                {
                        //                    var val = db.IncrActivity.Find(int.Parse(IncrActivity));
                        //                    c.IncrActivity = val;

                        //                    var type = db.PromoPolicy.Include(e => e.IncrActivity).Where(e => e.Id == data).SingleOrDefault();
                        //                    IList<PromoPolicy> typedetails = null;
                        //                    if (type.IncrActivity != null)
                        //                    {
                        //                        typedetails = db.PromoPolicy.Where(x => x.IncrActivity.Id == type.IncrActivity.Id && x.Id == data).ToList();
                        //                    }
                        //                    else
                        //                    {
                        //                        typedetails = db.PromoPolicy.Where(x => x.Id == data).ToList();
                        //                    }
                        //                    //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                        //                    foreach (var s in typedetails)
                        //                    {
                        //                        s.IncrActivity = c.IncrActivity;
                        //                        db.PromoPolicy.Attach(s);
                        //                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //                        await db.SaveChangesAsync();
                        //                        db.SaveChanges();
                        //                        TempData["RowVersion"] = s.RowVersion;
                        //                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        //                    }
                        //                }
                        //                else
                        //                {
                        //                    var IncrDetails = db.PromoPolicy.Include(e => e.IncrActivity).Where(x => x.Id == data).ToList();
                        //                    foreach (var s in IncrDetails)
                        //                    {
                        //                        s.IncrActivity = null;
                        //                        db.PromoPolicy.Attach(s);
                        //                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //                        await db.SaveChangesAsync();
                        //                        db.SaveChanges();
                        //                        TempData["RowVersion"] = s.RowVersion;
                        //                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        //                    }
                        //                }
                        //            }
                        //            else
                        //            {
                        //                var IncrDetails = db.PromoPolicy.Include(e => e.IncrActivity).Where(x => x.Id == data).ToList();
                        //                foreach (var s in IncrDetails)
                        //                {
                        //                    s.IncrActivity = null;
                        //                    db.PromoPolicy.Attach(s);
                        //                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //                    await db.SaveChangesAsync();
                        //                    db.SaveChanges();
                        //                    TempData["RowVersion"] = s.RowVersion;
                        //                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        //                }
                        //            }

                        //            //int a = EditS(data, c, c.DBTrack);
                        //            using (var context = new DataBaseContext())
                        //            {
                        //                var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                        //                DT_PromoPolicy DT_Corp = (DT_PromoPolicy)obj;
                        //                db.Create(DT_Corp);
                        //                db.SaveChanges();
                        //            }
                        //            await db.SaveChangesAsync();
                        //            ts.Complete();
                        //            Msg.Add("  Record Updated");
                        //            return Json(new Utility.JsonReturnClass { Id = data, Val = c.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //            //return Json(new Object[] { data, c.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });
                        //        }
                        //    }
                        //    catch (DbUpdateConcurrencyException ex)
                        //    {
                        //        var entry = ex.Entries.Single();
                        //        var clientValues = (PromoPolicy)entry.Entity;
                        //        var databaseEntry = entry.GetDatabaseValues();
                        //        if (databaseEntry == null)
                        //        {
                        //            Msg.Add(" Unable to save changes. The record was deleted by another user.");
                        //            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //            //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                        //        }
                        //        else
                        //        {
                        //            var databaseValues = (PromoPolicy)databaseEntry.ToObject();
                        //            c.RowVersion = databaseValues.RowVersion;

                        //        }
                        //    }
                        //    Msg.Add("Record modified by another user.So refresh it and try to save again.");
                        //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //    //eturn Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                        //}

                        //new code
                        //if (ModelState.IsValid)
                        //{
                        //    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        //    {
                        //        var Curr_LKValue = db.PromoPolicy.Find(data);
                        //        TempData["CurrRowVersion"] = Curr_LKValue.RowVersion;
                        //        db.Entry(Curr_LKValue).State = System.Data.Entity.EntityState.Detached;
                        //        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                        //        {
                        //            PromoPolicy blog = blog = null;
                        //            DbPropertyValues originalBlogValues = null;

                        //            using (var context = new DataBaseContext())
                        //            {
                        //                blog = context.PromoPolicy.Where(e => e.Id == data).SingleOrDefault();
                        //                originalBlogValues = context.Entry(blog).OriginalValues;
                        //            }

                        //            c.DBTrack = new DBTrack
                        //            {
                        //                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                        //                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                        //                Action = "M",
                        //                ModifiedBy = SessionManager.UserName,
                        //                ModifiedOn = DateTime.Now
                        //            };
                        //            var m1 = db.PromoPolicy.Where(e => e.Id == data).ToList();
                        //            foreach (var s in m1)
                        //            {
                        //                // s.AppraisalPeriodCalendar = null;
                        //                db.PromoPolicy.Attach(s);
                        //                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //                //await db.SaveChangesAsync();
                        //                db.SaveChanges();
                        //                TempData["RowVersion"] = s.RowVersion;
                        //                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        //            }
                        //            var CurCorp = db.PromoPolicy.Find(data);
                        //            TempData["CurrRowVersion"] = CurCorp.RowVersion;
                        //            db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                        //            if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                        //            {
                        //                //c.DBTrack = dbT;
                        //                PromoPolicy corp = new PromoPolicy()
                        //                {
                        //                    IncrActivity = c.IncrActivity,
                        //                    IsActionDateAsIncrDate = c.IsActionDateAsIncrDate,
                        //                    IsFuncStructChange = c.IsFuncStructChange,
                        //                    IsNewScaleIncrAction = c.IsNewScaleIncrAction,
                        //                    IsOldScaleIncrAction = c.IsOldScaleIncrAction,
                        //                    IsPayJobStatusChange = c.IsPayJobStatusChange,
                        //                    IsPayStructChange = c.IsPayStructChange,
                        //                    Name = c.Name,
                        //                    Id = data,
                        //                    DBTrack = c.DBTrack
                        //                };

                        //                db.PromoPolicy.Attach(corp);
                        //                db.Entry(corp).State = System.Data.Entity.EntityState.Modified;

                        //                // db.Entry(c.DBTrack).State = System.Data.Entity.EntityState.Modified;
                        //                //db.SaveChanges();
                        //                db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                        //                // DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, val.DBTrack);
                        //                await db.SaveChangesAsync();
                        //                //DisplayTrackedEntities(db.ChangeTracker);
                        //                db.Entry(corp).State = System.Data.Entity.EntityState.Detached;
                        //                ts.Complete();
                        //                //return Json(new Object[] { lkval.Id, lkval.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });
                        //                Msg.Add("  Record Updated");
                        //                return Json(new Utility.JsonReturnClass { Id = corp.Id, Val = corp.Name, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //            }





                        //        }
                        //    }
                        //}

                        ////
                        if (ModelState.IsValid)
                        {
                            try
                            {
                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    PromoPolicy blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.PromoPolicy.Where(e => e.Id == data).SingleOrDefault();
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
                                    var m1 = db.PromoPolicy.Where(e => e.Id == data).ToList();
                                    foreach (var s in m1)
                                    {
                                        // s.AppraisalPeriodCalendar = null;
                                        db.PromoPolicy.Attach(s);
                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                        //await db.SaveChangesAsync();
                                        db.SaveChanges();
                                        TempData["RowVersion"] = s.RowVersion;
                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    }
                                    //int a = EditS(Category, PromoPolicy, data, c, c.DBTrack);
                                    var CurCorp = db.PromoPolicy.Find(data);
                                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {

                                        PromoPolicy corp = new PromoPolicy()
                                        {

                                            IncrActivity_Id = c.IncrActivity_Id,
                                            IncrActivity = c.IncrActivity,
                                            IsActionDateAsIncrDate = c.IsActionDateAsIncrDate,
                                            IsFuncStructChange = c.IsFuncStructChange,
                                            IsNewScaleIncrAction = c.IsNewScaleIncrAction,
                                            IsOldScaleIncrAction = c.IsOldScaleIncrAction,
                                            IsPayJobStatusChange = c.IsPayJobStatusChange,
                                            IsPayStructChange = c.IsPayStructChange,
                                            Name = c.Name,
                                            Id = data,
                                            DBTrack = c.DBTrack
                                        };

                                        db.PromoPolicy.Attach(corp);
                                        db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                                        db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                        // return 1;

                                        using (var context = new DataBaseContext())
                                        {
                                            var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                            DT_PromoPolicy DT_Corp = (DT_PromoPolicy)obj;
                                            DT_Corp.IncrActivity_Id = blog.IncrActivity == null ? 0 : blog.IncrActivity.Id;

                                            db.Create(DT_Corp);
                                            db.SaveChanges();
                                        }
                                        db.SaveChanges();
                                        await db.SaveChangesAsync();
                                        ts.Complete();
                                        Msg.Add("  Record Updated");
                                        return Json(new Utility.JsonReturnClass { Id = corp.Id, Val = corp.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        // return Json(new Object[] { c.Id, c.Name, "Record Updated", JsonRequestBehavior.AllowGet });
                                    }
                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (PromoActivity)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    // return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (PromoActivity)databaseEntry.ToObject();
                                    c.RowVersion = databaseValues.RowVersion;

                                }
                            }
                            Msg.Add("Record modified by another user.So refresh it and try to save again.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                        }



                    }
                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            PromoPolicy blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            PromoPolicy Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.PromoPolicy.Where(e => e.Id == data).SingleOrDefault();
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
                            PromoPolicy corp = new PromoPolicy()
                            {
                                IncrActivity = c.IncrActivity,
                                IsActionDateAsIncrDate = c.IsActionDateAsIncrDate,
                                IsFuncStructChange = c.IsFuncStructChange,
                                IsNewScaleIncrAction = c.IsNewScaleIncrAction,
                                IsOldScaleIncrAction = c.IsOldScaleIncrAction,
                                IsPayJobStatusChange = c.IsPayJobStatusChange,
                                IsPayStructChange = c.IsPayStructChange,
                                Name = c.Name,
                                Id = data,
                                DBTrack = c.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };



                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, corp, "PromoPolicy", c.DBTrack);
                                DT_PromoPolicy DT_Corp = (DT_PromoPolicy)obj;
                                db.Create(DT_Corp);
                            }
                            blog.DBTrack = c.DBTrack;
                            db.PromoPolicy.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = c.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            // return Json(new Object[] { blog.Id, c.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });
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

        [HttpPost]
        public async Task<ActionResult> AuthSave(int auth_id, Boolean isauth, String auth_action) //to save authorised data
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    if (auth_action == "C")
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            PromoPolicy corp = db.PromoPolicy.FirstOrDefault(e => e.Id == auth_id);
                            corp.DBTrack = new DBTrack
                            {
                                Action = "C",
                                ModifiedBy = corp.DBTrack.ModifiedBy != null ? corp.DBTrack.ModifiedBy : null,
                                CreatedBy = corp.DBTrack.CreatedBy != null ? corp.DBTrack.CreatedBy : null,
                                CreatedOn = corp.DBTrack.CreatedOn != null ? corp.DBTrack.CreatedOn : null,
                                IsModified = corp.DBTrack.IsModified == true ? false : false,
                                AuthorizedBy = SessionManager.UserName,
                                AuthorizedOn = DateTime.Now
                            };

                            db.PromoPolicy.Attach(corp);
                            db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corp.DBTrack);
                            DT_PromoPolicy DT_Corp = (DT_PromoPolicy)rtn_Obj;
                            db.Create(DT_Corp);
                            await db.SaveChangesAsync();

                            ts.Complete();
                            Msg.Add("  Record Authorised");
                            return Json(new Utility.JsonReturnClass { Id = corp.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { corp.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else if (auth_action == "M")
                    {

                        PromoPolicy Old_Corp = db.PromoPolicy.Where(e => e.Id == auth_id).SingleOrDefault();


                        DT_PromoPolicy Curr_Corp = db.DT_PromoPolicy
                                                    .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                                    .OrderByDescending(e => e.Id)
                                                    .FirstOrDefault();

                        if (Curr_Corp != null)
                        {
                            PromoPolicy corp = new PromoPolicy();
                            corp.Name = Curr_Corp.Name == null ? Old_Corp.Name : Curr_Corp.Name;
                            corp.IsFuncStructChange = Curr_Corp.IsFuncStructChange;
                            corp.IsActionDateAsIncrDate = Curr_Corp.IsActionDateAsIncrDate;
                            corp.IsNewScaleIncrAction = Curr_Corp.IsNewScaleIncrAction;
                            corp.IsOldScaleIncrAction = Curr_Corp.IsOldScaleIncrAction;
                            corp.IsPayJobStatusChange = Curr_Corp.IsPayJobStatusChange;
                            corp.IsPayStructChange = Curr_Corp.IsPayStructChange;

                            if (ModelState.IsValid)
                            {
                                try
                                {
                                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                    {
                                        corp.DBTrack = new DBTrack
                                        {
                                            CreatedBy = Old_Corp.DBTrack.CreatedBy == null ? null : Old_Corp.DBTrack.CreatedBy,
                                            CreatedOn = Old_Corp.DBTrack.CreatedOn == null ? null : Old_Corp.DBTrack.CreatedOn,
                                            Action = "M",
                                            ModifiedBy = Old_Corp.DBTrack.ModifiedBy == null ? null : Old_Corp.DBTrack.ModifiedBy,
                                            ModifiedOn = Old_Corp.DBTrack.ModifiedOn == null ? null : Old_Corp.DBTrack.ModifiedOn,
                                            AuthorizedBy = SessionManager.UserName,
                                            AuthorizedOn = DateTime.Now,
                                            IsModified = false
                                        };

                                        int a = EditS(auth_id, corp, corp.DBTrack);
                                        await db.SaveChangesAsync();
                                        ts.Complete();
                                        Msg.Add("  Record Authorised");
                                        return Json(new Utility.JsonReturnClass { Id = corp.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                        // return Json(new Object[] { corp.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                                    }
                                }
                                catch (DbUpdateConcurrencyException ex)
                                {
                                    var entry = ex.Entries.Single();
                                    var clientValues = (PromoPolicy)entry.Entity;
                                    var databaseEntry = entry.GetDatabaseValues();
                                    if (databaseEntry == null)
                                    {
                                        Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        // return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                    }
                                    else
                                    {
                                        var databaseValues = (PromoPolicy)databaseEntry.ToObject();
                                        corp.RowVersion = databaseValues.RowVersion;
                                    }
                                }
                                Msg.Add("Record modified by another user.So refresh it and try to save again.");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                            }
                        }
                        else
                        {
                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                        // return Json(new Object[] { "", "Data removed from history", JsonRequestBehavior.AllowGet });
                    }
                    else if (auth_action == "D")
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            PromoPolicy corp = db.PromoPolicy.AsNoTracking().FirstOrDefault(e => e.Id == auth_id);

                            corp.DBTrack = new DBTrack
                            {
                                Action = "D",
                                ModifiedBy = corp.DBTrack.ModifiedBy != null ? corp.DBTrack.ModifiedBy : null,
                                CreatedBy = corp.DBTrack.CreatedBy != null ? corp.DBTrack.CreatedBy : null,
                                CreatedOn = corp.DBTrack.CreatedOn != null ? corp.DBTrack.CreatedOn : null,
                                IsModified = false,
                                AuthorizedBy = SessionManager.UserName,
                                AuthorizedOn = DateTime.Now
                            };

                            db.PromoPolicy.Attach(corp);
                            db.Entry(corp).State = System.Data.Entity.EntityState.Deleted;


                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corp.DBTrack);
                            DT_PromoPolicy DT_Corp = (DT_PromoPolicy)rtn_Obj;
                            db.Create(DT_Corp);
                            await db.SaveChangesAsync();
                            db.Entry(corp).State = System.Data.Entity.EntityState.Detached;
                            ts.Complete();
                            Msg.Add(" Record Authorised ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            // return Json(new Object[] { "", "Record Authorised", JsonRequestBehavior.AllowGet });
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

        public ActionResult P2BGrid(P2BGrid_Parameters gp)
        {
            try
            {
                DataBaseContext db = new DataBaseContext();
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                var jsonData = (Object)null;
                IEnumerable<PromoPolicy> PromoPolicy = null;
                if (gp.IsAutho == true)
                {
                    PromoPolicy = db.PromoPolicy.Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    PromoPolicy = db.PromoPolicy.Include(e => e.IncrActivity).AsNoTracking().ToList();
                }

                IEnumerable<PromoPolicy> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = PromoPolicy;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Select(a => new { a.Id, a.Name }).Where((e => (e.Id.ToString() == gp.searchString) || (e.Name.ToLower() == gp.searchString.ToLower()))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Name, a.IncrActivity != null ? Convert.ToString(a.IncrActivity.Name) : "" }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = PromoPolicy;
                    Func<PromoPolicy, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c =>
                                         gp.sidx == "Name" ? c.Name :
                                         gp.sidx == "IncrActivity" ? c.IncrActivity.ToString() : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.Name, a.IncrActivity != null ? Convert.ToString(a.IncrActivity.Name) : "" }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.Name, a.IncrActivity != null ? Convert.ToString(a.IncrActivity.Name) : "" }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Name, a.IncrActivity != null ? Convert.ToString(a.IncrActivity.Name) : "" }).ToList();
                    }
                    totalRecords = PromoPolicy.Count();
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
        public async Task<ActionResult> Delete(int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    PromoPolicy PromoPolicys = db.PromoPolicy.Where(e => e.Id == data).SingleOrDefault();
                    var promoactivity = db.PromoActivity.Include(e => e.PromoPolicy).Where(e => e.PromoPolicy.Id == PromoPolicys.Id).SingleOrDefault();
                    if (promoactivity != null)
                    {
                        Msg.Add(" Child record exists.Cannot remove it..  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        // return Json(new Object[] { "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                        // return Json(new Object[] { "", "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });


                    }
                    if (PromoPolicys.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = PromoPolicys.DBTrack.CreatedBy != null ? PromoPolicys.DBTrack.CreatedBy : null,
                                CreatedOn = PromoPolicys.DBTrack.CreatedOn != null ? PromoPolicys.DBTrack.CreatedOn : null,
                                IsModified = PromoPolicys.DBTrack.IsModified == true ? true : false
                            };
                            PromoPolicys.DBTrack = dbT;
                            db.Entry(PromoPolicys).State = System.Data.Entity.EntityState.Modified;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, PromoPolicys.DBTrack);
                            DT_PromoPolicy DT_Corp = (DT_PromoPolicy)rtn_Obj;
                            db.Create(DT_Corp);
                            await db.SaveChangesAsync();
                            ts.Complete();
                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            try
                            {
                                DBTrack dbT = new DBTrack
                                {
                                    Action = "D",
                                    ModifiedBy = SessionManager.UserName,
                                    ModifiedOn = DateTime.Now,
                                    CreatedBy = PromoPolicys.DBTrack.CreatedBy != null ? PromoPolicys.DBTrack.CreatedBy : null,
                                    CreatedOn = PromoPolicys.DBTrack.CreatedOn != null ? PromoPolicys.DBTrack.CreatedOn : null,
                                    IsModified = PromoPolicys.DBTrack.IsModified == true ? false : false//,
                                };

                                db.Entry(PromoPolicys).State = System.Data.Entity.EntityState.Deleted;
                                //var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dbT);
                                //DT_PromoPolicy DT_Corp = (DT_PromoPolicy)rtn_Obj;
                                //db.Create(DT_Corp);
                                await db.SaveChangesAsync();
                                ts.Complete();
                                Msg.Add("  Data removed successfully.  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });

                            }
                            catch (RetryLimitExceededException /* dex */)
                            {
                                Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
                            }
                        }
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
        public int EditS(int data, PromoPolicy c, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var CurCorp = db.PromoPolicy.Find(data);
                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    c.DBTrack = dbT;
                    PromoPolicy corp = new PromoPolicy()
                    {
                        IncrActivity = c.IncrActivity,
                        IsActionDateAsIncrDate = c.IsActionDateAsIncrDate,
                        IsFuncStructChange = c.IsFuncStructChange,
                        IsNewScaleIncrAction = c.IsNewScaleIncrAction,
                        IsOldScaleIncrAction = c.IsOldScaleIncrAction,
                        IsPayJobStatusChange = c.IsPayJobStatusChange,
                        IsPayStructChange = c.IsPayStructChange,
                        Name = c.Name,
                        Id = data,
                        DBTrack = c.DBTrack
                    };

                    db.PromoPolicy.Attach(corp);
                    db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    return 1;
                }
                return 0;
            }
        }

        public void RollBack()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var changedEntries = db.ChangeTracker.Entries()
                    .Where(x => x.State != System.Data.Entity.EntityState.Unchanged).ToList();

                foreach (var entry in changedEntries.Where(x => x.State == System.Data.Entity.EntityState.Modified))
                {
                    entry.CurrentValues.SetValues(entry.OriginalValues);
                    entry.State = System.Data.Entity.EntityState.Unchanged;
                }

                foreach (var entry in changedEntries.Where(x => x.State == System.Data.Entity.EntityState.Added))
                {
                    entry.State = System.Data.Entity.EntityState.Detached;
                }

                foreach (var entry in changedEntries.Where(x => x.State == System.Data.Entity.EntityState.Deleted))
                {
                    entry.State = System.Data.Entity.EntityState.Unchanged;
                }
            }
        }

        //public ActionResult GetLookupPolicy(string data)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var fall = db.PromoPolicy.ToList();
        //        IEnumerable<PromoPolicy> all;
        //        if (!string.IsNullOrEmpty(data))
        //        {
        //            all = db.PromoPolicy.ToList().Where(d => d.FullDetails.Contains(data));

        //        }
        //        else
        //        {
        //            var list1 = db.PromoActivity.ToList().Select(e => e.PromoPolicy);
        //            var list2 = fall.Except(list1);
        //            //  var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.LookupVal }).Distinct();
        //            var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
        //            //var result_1 = (from c in fall
        //            //                select new { c.Id, c.PromoPolicyCode, c.PromoPolicyName });
        //            return Json(r, JsonRequestBehavior.AllowGet);
        //        }
        //        var result = (from c in all
        //                      select new { c.Id, c.FullDetails }).Distinct();
        //        return Json(result, JsonRequestBehavior.AllowGet);
        //    }
        //    // return View();
        //}

        public ActionResult GetLookupPolicy(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.PromoPolicy.Include(e => e.IncrActivity).ToList();
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.PromoPolicy.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                    }
                    //var Y = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                    //return Json(Y, JsonRequestBehavior.AllowGet);
                }
                var list1 = db.PromoActivity.Include(e => e.PromoPolicy).Select(e => e.PromoPolicy).ToList();
                var list2 = fall.Except(list1);
                var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
            }

        }



    }

}