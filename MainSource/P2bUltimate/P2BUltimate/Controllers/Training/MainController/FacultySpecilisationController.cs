///
/// Created by Tanushri
///


using P2b.Global;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Payroll;
using Training;
using P2BUltimate.Security;

namespace P2BUltimate.Controllers.Training.MainController
{
    public class FacultySpecializationController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();
        //
        // GET: /FacultySpecialization/

        public ActionResult Index()
        {
            return View("~/Views/Training/MainViews/FacultySpecialization/Index.cshtml");
        }

        public ActionResult GetLookup(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.FacultySpecialization.ToList();


                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.FacultySpecialization.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
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

        public ActionResult Partial()
        {
            return View("~/Views/Shared/Training/_FacultySpecialization.cshtml");
        }

        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult Create(FacultySpecialization COBJ, FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {

                    if (COBJ.SpecType == null || COBJ.SpecArea == null)
                    {
                        Msg.Add("Enter Value for Specialization Type.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {

                            if (db.FacultySpecialization.Any(o => o.SpecArea == COBJ.SpecArea) && db.FacultySpecialization.Any(o => o.SpecType == COBJ.SpecType))
                            {

                                Msg.Add("  Record Already Exists.  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }

                            COBJ.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            //look.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName };

                            FacultySpecialization FacultySpecialization = new FacultySpecialization()
                            {

                                SpecType = COBJ.SpecType,
                                SpecArea = COBJ.SpecArea,
                                DBTrack = COBJ.DBTrack
                            };
                            try
                            {
                                db.FacultySpecialization.Add(FacultySpecialization);
                                var rtn_Obj = DBTrackFile.DBTrackSave("Training/Training", null, db.ChangeTracker, COBJ.DBTrack);
                                DT_FacultySpecialization DT_OBJ = (DT_FacultySpecialization)rtn_Obj;
                                db.Create(DT_OBJ);
                                db.SaveChanges();

                                ts.Complete();

                                Msg.Add("  Data Created successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = FacultySpecialization.Id, Val = FacultySpecialization.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);


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
                        return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
                        // return this.Json(new { msg = errorMsg });
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

                var FacultySpecialization = db.FacultySpecialization
                                    .Where(e => e.Id == data).ToList();
                var r = (from ca in FacultySpecialization
                         select new
                         {

                             Id = ca.Id,
                             SpecType = ca.SpecType,
                             SpecArea = ca.SpecArea,
                             Action = ca.DBTrack.Action
                         }).Distinct();

                var a = "";

                var W = db.DT_FacultySpecialization
                     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id,
                         SpecType = e.SpecType == null ? "" : e.SpecType,
                         SpecArea = e.SpecArea == null ? "" : e.SpecArea.ToString(),
                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();
                var LKup = db.FacultySpecialization.Find(data);
                TempData["RowVersion"] = LKup.RowVersion;
                var Auth = LKup.DBTrack.IsModified;

                return this.Json(new Object[] { r, a, W, Auth, JsonRequestBehavior.AllowGet });
            }
        }

        //[HttpPost]
        ////[ValidateAntiForgeryToken]
        //public async Task<ActionResult> EditSave(FacultySpecialization ESOBJ, FormCollection form, int data)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        List<string> Msg = new List<string>();
        //        try
        //        {
        //            bool Auth = form["Autho_Allow"] == "true" ? true : false;
        //            if (ESOBJ.SpecType == null || ESOBJ.SpecArea == null)
        //            {
        //                Msg.Add("Enter Value for Specialization Type.");
        //                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //            }

        //            if (Auth == false)
        //            {
        //                if (ModelState.IsValid)
        //                {
        //                    try
        //                    {
        //                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                        {
        //                            FacultySpecialization blog = null; // to retrieve old data
        //                            DbPropertyValues originalBlogValues = null;

        //                            using (var context = new DataBaseContext())
        //                            {
        //                                blog = context.FacultySpecialization.Where(e => e.Id == data)
        //                                                        .SingleOrDefault();
        //                                originalBlogValues = context.Entry(blog).OriginalValues;
        //                            }

        //                            ESOBJ.DBTrack = new DBTrack
        //                            {
        //                                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                                Action = "M",
        //                                ModifiedBy = SessionManager.UserName,
        //                                ModifiedOn = DateTime.Now
        //                            };

        //                            int a = EditS(data, ESOBJ, ESOBJ.DBTrack);

        //                            using (var context = new DataBaseContext())
        //                            {
        //                                var obj = DBTrackFile.DBTrackSave("Training/Training", originalBlogValues, db.ChangeTracker, ESOBJ.DBTrack);
        //                                //DT_FacultySpecialization DT_OBJ = (DT_FacultySpecialization)obj;
        //                                //  db.Create(DT_OBJ);
        //                                db.SaveChanges();
        //                            }
        //                            await db.SaveChangesAsync();
        //                            ts.Complete();

        //                            Msg.Add("  Record Updated");
        //                            return Json(new Utility.JsonReturnClass { Id = data, Val = ESOBJ.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                        }
        //                    }

        //                    //catch (DbUpdateException e) { throw e; }
        //                    //catch (DataException e) { throw e; }
        //                    catch (DbUpdateConcurrencyException ex)
        //                    {
        //                        var entry = ex.Entries.Single();
        //                        var clientValues = (FacultySpecialization)entry.Entity;
        //                        var databaseEntry = entry.GetDatabaseValues();
        //                        if (databaseEntry == null)
        //                        {

        //                            Msg.Add(" Unable to save changes. The record was deleted by another user.");
        //                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                        }
        //                        else
        //                        {
        //                            var databaseValues = (FacultySpecialization)databaseEntry.ToObject();
        //                            ESOBJ.RowVersion = databaseValues.RowVersion;

        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    StringBuilder sb = new StringBuilder("");
        //                    foreach (ModelState modelState in ModelState.Values)
        //                    {
        //                        foreach (ModelError error in modelState.Errors)
        //                        {
        //                            sb.Append(error.ErrorMessage);
        //                            sb.Append("." + "\n");
        //                        }
        //                    }
        //                    var errorMsg = sb.ToString();
        //                    return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
        //                }
        //            }


        //            else
        //            {
        //                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                {

        //                    FacultySpecialization blog = null; // to retrieve old data
        //                    DbPropertyValues originalBlogValues = null;
        //                    FacultySpecialization Old_Obj = null;

        //                    using (var context = new DataBaseContext())
        //                    {
        //                        blog = context.FacultySpecialization.Where(e => e.Id == data).SingleOrDefault();
        //                        originalBlogValues = context.Entry(blog).OriginalValues;
        //                    }
        //                    ESOBJ.DBTrack = new DBTrack
        //                    {
        //                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                        Action = "M",
        //                        IsModified = blog.DBTrack.IsModified == true ? true : false,
        //                        ModifiedBy = SessionManager.UserName,
        //                        ModifiedOn = DateTime.Now
        //                    };
        //                    FacultySpecialization corp = new FacultySpecialization()
        //                    {
        //                        SpecType = ESOBJ.SpecType,
        //                        SpecArea = ESOBJ.SpecArea,
        //                        Id = data,
        //                        DBTrack = ESOBJ.DBTrack,
        //                        RowVersion = (Byte[])TempData["RowVersion"]
        //                    };


        //                    //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

        //                    using (var context = new DataBaseContext())
        //                    {
        //                        var obj = DBTrackFile.ModifiedDataHistory("Training/Training", "M", blog, corp, "FacultySpecialization", ESOBJ.DBTrack);
        //                        // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

        //                        Old_Obj = context.FacultySpecialization.Where(e => e.Id == data)
        //                            .SingleOrDefault();
        //                        //DT_FacultySpecialization DT_Corp = (DT_FacultySpecialization)obj;
        //                        // db.Create(DT_Corp);
        //                        //db.SaveChanges();
        //                    }
        //                    blog.DBTrack = ESOBJ.DBTrack;
        //                    db.FacultySpecialization.Attach(blog);
        //                    db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
        //                    db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //                    db.SaveChanges();
        //                    ts.Complete();

        //                    Msg.Add("  Record Updated");
        //                    return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = ESOBJ.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                }

        //            }
        //            return View();
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
        //    }
        //}

        //[HttpPost]
        //public async Task<ActionResult> EditSave(FacultySpecialization c, int data, FormCollection form) // Edit submit
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        try
        //        {
        //            List<string> Msg = new List<string>();

        //            bool Auth = form["Autho_Allow"] == "true" ? true : false;

        //            if (ModelState.IsValid)
        //            {
        //                try
        //                {

        //                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                    {

        //                        //var Curr_Lookup = db.FacultySpecialization.Find(data);
        //                        //TempData["CurrRowVersion"] = Curr_Lookup.RowVersion;
        //                        //db.Entry(Curr_Lookup).State = System.Data.Entity.EntityState.Detached;

        //                        //if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
        //                        //{

        //                        FacultySpecialization blog = null; // to retrieve old data
        //                        DbPropertyValues originalBlogValues = null;

        //                        using (var context = new DataBaseContext())
        //                        {
        //                            blog = context.FacultySpecialization.Where(e => e.Id == data).SingleOrDefault();
        //                            originalBlogValues = context.Entry(blog).OriginalValues;
        //                        }

        //                        c.DBTrack = new DBTrack
        //                        {
        //                            CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                            CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                            Action = "M",
        //                            ModifiedBy = SessionManager.UserName,
        //                            ModifiedOn = DateTime.Now
        //                        };

        //                        FacultySpecialization lk = new FacultySpecialization()
        //                        {
        //                            Id = data,
        //                            SpecType = c.SpecType,
        //                            SpecArea = c.SpecArea,
        //                            DBTrack = c.DBTrack
        //                        };
        //                        db.FacultySpecialization.Attach(lk);
        //                        db.Entry(lk).State = System.Data.Entity.EntityState.Modified;
        //                        db.Entry(lk).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //                        // int a = EditS(Corp, Addrs, ContactDetails, data, c, c.DBTrack);

        //                        // int a = EditS(Values, Valueshobby, Valuesls, Valuesqd, Valuesskl, Valuesscr, data, c, c.DBTrack);


        //                        using (var context = new DataBaseContext())
        //                        {

        //                            var obj = DBTrackFile.DBTrackSave("Training/Training", originalBlogValues, db.ChangeTracker, c.DBTrack);
        //                            DT_FacultySpecialization DT_Corp = (DT_FacultySpecialization)obj;

        //                            db.Create(DT_Corp);
        //                            db.SaveChanges();
        //                        }
        //                        await db.SaveChangesAsync();
        //                        ts.Complete();

        //                        Msg.Add("  Record Updated");
        //                        return Json(new Utility.JsonReturnClass { Id = lk.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                        //  }
        //                    }
        //                }
        //                catch (DbUpdateConcurrencyException ex)
        //                {
        //                    var entry = ex.Entries.Single();
        //                    var clientValues = (FacultySpecialization)entry.Entity;
        //                    var databaseEntry = entry.GetDatabaseValues();
        //                    if (databaseEntry == null)
        //                    {
        //                        Msg.Add(" Unable to save changes. The record was deleted by another user.");
        //                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                    }
        //                    else
        //                    {
        //                        var databaseValues = (FacultySpecialization)databaseEntry.ToObject();
        //                        c.RowVersion = databaseValues.RowVersion;

        //                    }
        //                }
        //                Msg.Add("Record modified by another user.So refresh it and try to save again.");
        //                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
        //            List<string> Msg = new List<string>();
        //            Msg.Add(ex.Message);
        //            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //        }

        //        return View();
        //    }
        //}

        [HttpPost]
        public async Task<ActionResult> EditSave(FacultySpecialization c, int data, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<String> Msg = new List<String>();
                try
                {
                    string Addrs = form["SalaryComponentList"] == "0" ? "" : form["SalaryComponentList"];

                    bool Auth = form["Autho_Allow"] == "true" ? true : false;

                    List<SalaryHead> ObjITsection = new List<SalaryHead>();

                    FacultySpecialization pd = null;
                    pd = db.FacultySpecialization.Where(q => q.Id == data).SingleOrDefault();

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            FacultySpecialization blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.FacultySpecialization.Where(e => e.Id == data).SingleOrDefault();
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
                            var m1 = db.FacultySpecialization.Where(e => e.Id == data).ToList();
                            foreach (var s in m1)
                            {
                                // s.AppraisalPeriodCalendar = null;
                                db.FacultySpecialization.Attach(s);
                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                //await db.SaveChangesAsync();
                                db.SaveChanges();
                                TempData["RowVersion"] = s.RowVersion;
                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                            }

                            var CurCorp = db.FacultySpecialization.Find(data);
                            TempData["CurrRowVersion"] = CurCorp.RowVersion;
                            db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;

                            if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                            {
                                FacultySpecialization corp = new FacultySpecialization()
                                {
                                    Id = data,
                                    SpecType = c.SpecType,
                                    SpecArea = c.SpecArea,
                                    DBTrack = c.DBTrack
                                };

                                db.FacultySpecialization.Attach(corp);
                                db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                                db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            }
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            ts.Complete();

                            Msg.Add("Record Updated Successfully.");
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = blog.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }

                        Msg.Add("Record modified by another user.So refresh it and try to save again.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                    }

                }
                catch (DbUpdateConcurrencyException ex)
                {
                    var entry = ex.Entries.Single();
                    var clientValues = (FacultySpecialization)entry.Entity;
                    var databaseEntry = entry.GetDatabaseValues();
                    if (databaseEntry == null)
                    {
                        Msg.Add(ex.Message);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //  return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                    }
                    else
                    {
                        var databaseValues = (FacultySpecialization)databaseEntry.ToObject();
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

        public int EditS(int data, FacultySpecialization ESOBJ, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var CurOBJ = db.FacultySpecialization.Find(data);
                TempData["CurrRowVersion"] = CurOBJ.RowVersion;
                db.Entry(CurOBJ).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    ESOBJ.DBTrack = dbT;
                    FacultySpecialization ESIOBJ = new FacultySpecialization()
                    {
                        Id = data,
                        SpecType = ESOBJ.SpecType,
                        SpecArea = ESOBJ.SpecArea,
                        DBTrack = ESOBJ.DBTrack
                    };

                    db.FacultySpecialization.Attach(ESIOBJ);
                    db.Entry(ESIOBJ).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(ESIOBJ).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    //// DBTrackFile.DBTrackSave("Payroll", originalBlogValues, db.ChangeTracker, c.DBTrack);
                    return 1;
                }
                return 0;
            }
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult Delete(int? data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    FacultySpecialization FacultySpecialization = db.FacultySpecialization.Where(e => e.Id == data).SingleOrDefault();
                    try
                    {
                        //var selectedValues = FacultySpecialization.SocialActivities;
                        //var lkValue = new HashSet<int>(FacultySpecialization.SocialActivities.Select(e => e.Id));
                        //if (lkValue.Count > 0)
                        //{
                        //    return this.Json(new Object[] { "", "Child record exists.Cannot delete.", JsonRequestBehavior.AllowGet });
                        //}

                        using (TransactionScope ts = new TransactionScope())
                        {
                            db.Entry(FacultySpecialization).State = System.Data.Entity.EntityState.Deleted;
                            db.SaveChanges();
                            ts.Complete();
                        }
                        Msg.Add("  Data removed successfully.  ");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }

                    catch (DataException /* dex */)
                    {
                        Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
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

        public ActionResult P2BGrid(P2BGrid_Parameters gp)
        {
            try
            {
                DataBaseContext db = new DataBaseContext();
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                int ParentId = 2;
                var jsonData = (Object)null;
                var LKVal = db.FacultySpecialization.ToList();

                if (gp.IsAutho == true)
                {
                    LKVal = db.FacultySpecialization.AsNoTracking().Where(e => e.DBTrack.IsModified == true).ToList();
                }
                else
                {
                    LKVal = db.FacultySpecialization.AsNoTracking().ToList();
                }


                IEnumerable<FacultySpecialization> IE;
                if (!string.IsNullOrEmpty(gp.searchString))
                {
                    IE = LKVal;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Select(a => new { a.Id, a.SpecType, a.SpecArea }).Where((e => (e.Id.ToString() == gp.searchString) || (e.SpecType.ToLower() == gp.searchString.ToLower()) || (e.SpecArea.ToString() == gp.searchString.ToLower())));
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.SpecType, a.SpecArea }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = LKVal;
                    Func<FacultySpecialization, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "SpecType" ? c.SpecType.ToString() :
                                         gp.sidx == "SpecArea " ? c.SpecArea.ToString() :
                                         "");
                    }

                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.SpecType, a.SpecArea }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.SpecType, a.SpecArea }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.SpecType, a.SpecArea }).ToList();
                    }
                    totalRecords = LKVal.Count();
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
                    total = totalPages,
                    p2bparam = ParentId
                };
                return Json(JsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public async Task<ActionResult> AuthSave(int auth_id, Boolean isauth, String auth_action) //to save authorised data
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (auth_action == "C")
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        FacultySpecialization ESI = db.FacultySpecialization
                            .FirstOrDefault(e => e.Id == auth_id);

                        ESI.DBTrack = new DBTrack
                        {
                            Action = "C",
                            ModifiedBy = ESI.DBTrack.ModifiedBy != null ? ESI.DBTrack.ModifiedBy : null,
                            CreatedBy = ESI.DBTrack.CreatedBy != null ? ESI.DBTrack.CreatedBy : null,
                            CreatedOn = ESI.DBTrack.CreatedOn != null ? ESI.DBTrack.CreatedOn : null,
                            IsModified = ESI.DBTrack.IsModified == true ? false : false,
                            AuthorizedBy = SessionManager.UserName,
                            AuthorizedOn = DateTime.Now
                        };

                        db.FacultySpecialization.Attach(ESI);
                        db.Entry(ESI).State = System.Data.Entity.EntityState.Modified;
                        db.Entry(ESI).OriginalValues["RowVersion"] = TempData["RowVersion"];
                        //db.SaveChanges();
                        var rtn_Obj = DBTrackFile.DBTrackSave("Training/Training", null, db.ChangeTracker, ESI.DBTrack);
                        DT_FacultySpecialization DT_OBJ = (DT_FacultySpecialization)rtn_Obj;

                        db.Create(DT_OBJ);
                        await db.SaveChangesAsync();

                        ts.Complete();
                        return Json(new Object[] { ESI.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                    }
                }
                else if (auth_action == "M")
                {

                    FacultySpecialization Old_OBJ = db.FacultySpecialization
                                            .Where(e => e.Id == auth_id).SingleOrDefault();


                    DT_FacultySpecialization Curr_OBJ = db.DT_FacultySpecialization
                                                .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                                .OrderByDescending(e => e.Id)
                                                .FirstOrDefault();

                    if (Curr_OBJ != null)
                    {
                        FacultySpecialization FacultySpecialization = new FacultySpecialization();

                        FacultySpecialization.SpecType = Curr_OBJ.SpecType == null ? Old_OBJ.SpecType : Curr_OBJ.SpecType;
                        FacultySpecialization.SpecArea = Curr_OBJ.SpecArea == null ? Old_OBJ.SpecArea : Curr_OBJ.SpecArea;



                        //      corp.Id = auth_id;

                        if (ModelState.IsValid)
                        {
                            try
                            {

                                //DbContextTransaction transaction = db.Database.BeginTransaction();

                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    // db.Configuration.AutoDetectChangesEnabled = false;
                                    FacultySpecialization.DBTrack = new DBTrack
                                    {
                                        CreatedBy = Old_OBJ.DBTrack.CreatedBy == null ? null : Old_OBJ.DBTrack.CreatedBy,
                                        CreatedOn = Old_OBJ.DBTrack.CreatedOn == null ? null : Old_OBJ.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = Old_OBJ.DBTrack.ModifiedBy == null ? null : Old_OBJ.DBTrack.ModifiedBy,
                                        ModifiedOn = Old_OBJ.DBTrack.ModifiedOn == null ? null : Old_OBJ.DBTrack.ModifiedOn,
                                        AuthorizedBy = SessionManager.UserName,
                                        AuthorizedOn = DateTime.Now,
                                        IsModified = false
                                    };

                                    int a = EditS(auth_id, FacultySpecialization, FacultySpecialization.DBTrack);

                                    await db.SaveChangesAsync();

                                    ts.Complete();
                                    return Json(new Object[] { FacultySpecialization.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (FacultySpecialization)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (FacultySpecialization)databaseEntry.ToObject();
                                    FacultySpecialization.RowVersion = databaseValues.RowVersion;
                                }
                            }

                            return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else
                        return Json(new Object[] { "", "Data removed from history", JsonRequestBehavior.AllowGet });
                }
                else if (auth_action == "D")
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        //FacultySpecialization corp = db.FacultySpecialization.Find(auth_id);
                        FacultySpecialization ESI = db.FacultySpecialization.AsNoTracking().FirstOrDefault(e => e.Id == auth_id);

                        //Address add = corp.Address;
                        //ContactDetails conDet = corp.ContactDetails;
                        //SocialActivities val = corp.BusinessType;

                        ESI.DBTrack = new DBTrack
                        {
                            Action = "D",
                            ModifiedBy = ESI.DBTrack.ModifiedBy != null ? ESI.DBTrack.ModifiedBy : null,
                            CreatedBy = ESI.DBTrack.CreatedBy != null ? ESI.DBTrack.CreatedBy : null,
                            CreatedOn = ESI.DBTrack.CreatedOn != null ? ESI.DBTrack.CreatedOn : null,
                            IsModified = false,
                            AuthorizedBy = SessionManager.UserName,
                            AuthorizedOn = DateTime.Now
                        };

                        db.FacultySpecialization.Attach(ESI);
                        db.Entry(ESI).State = System.Data.Entity.EntityState.Deleted;


                        var rtn_Obj = DBTrackFile.DBTrackSave("Training/Training", null, db.ChangeTracker, ESI.DBTrack);
                        DT_FacultySpecialization DT_OBJ = (DT_FacultySpecialization)rtn_Obj;
                        //DT_OBJ.Address_Id = corp.Address == null ? 0 : corp.Address.Id;
                        //DT_OBJ.BusinessType_Id = corp.BusinessType == null ? 0 : corp.BusinessType.Id;
                        //DT_OBJ.ContactDetails_Id = corp.ContactDetails == null ? 0 : corp.ContactDetails.Id;
                        db.Create(DT_OBJ);
                        await db.SaveChangesAsync();
                        db.Entry(ESI).State = System.Data.Entity.EntityState.Detached;
                        ts.Complete();
                        return Json(new Object[] { "", "Record Authorised", JsonRequestBehavior.AllowGet });
                    }

                }
                return View();

            }
        }
    }
}
