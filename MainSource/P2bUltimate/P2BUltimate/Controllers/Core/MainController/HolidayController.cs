
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
using P2BUltimate.Security;

namespace P2BUltimate.Controllers.Core.MainController
{
    //[AuthoriseManger]
    public class HolidayController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();
        public ActionResult Index()
        {
            return View("~/Views/Core/MainViews/Holiday/Index.cshtml");
        }
        public ActionResult partial()
        {
            return View("~/Views/Shared/Core/_Holiday.cshtml");
        }
        public ActionResult GetLookup(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.Holiday.Include(e => e.HolidayType).Include(e => e.HolidayName).ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.Holiday.Include(e => e.HolidayType).Include(e => e.HolidayName).Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }

                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult Create(Holiday COBJ, FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    string HolidayType = form["HolidayTypelist"] == "0" ? "" : form["HolidayTypelist"];
                    string HolidayName = form["HolidayNamelist"] == "0" ? "" : form["HolidayNamelist"];


                    if (HolidayType != null)
                    {
                        if (HolidayType != "")
                        {
                            var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "203").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(HolidayType)).FirstOrDefault(); //db.LookupValue.Find(int.Parse(HolidayType));
                            COBJ.HolidayType = val;
                        }
                    }

                    if (HolidayName != null)
                    {
                        if (HolidayName != "")
                        {
                            var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "202").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(HolidayName)).FirstOrDefault();  //db.LookupValue.Find(int.Parse(HolidayName));
                            COBJ.HolidayName = val;
                        }
                    }


                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {

                            COBJ.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            //look.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName };

                            Holiday Holiday = new Holiday()
                            {
                                HolidayType = COBJ.HolidayType,
                                HolidayName = COBJ.HolidayName,
                                DBTrack = COBJ.DBTrack,
                                FullDetails = COBJ.FullDetails
                            };
                            try
                            {
                                db.Holiday.Add(Holiday);
                                var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, COBJ.DBTrack);
                                DT_Holiday DT_OBJ = (DT_Holiday)rtn_Obj;
                                DT_OBJ.HolidayName_Id = COBJ.HolidayName == null ? 0 : COBJ.HolidayName.Id;
                                DT_OBJ.HolidayType_Id = COBJ.HolidayType == null ? 0 : COBJ.HolidayType.Id;
                                db.Create(DT_OBJ);
                                db.SaveChanges();
                                ts.Complete();
                                Msg.Add("  Data Created successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = Holiday.Id, Val = Holiday.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return this.Json(new Object[] { Holiday.Id, Holiday.FullDetails, "Data Created Successfully.", JsonRequestBehavior.AllowGet });
                            }

                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = COBJ.Id });
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
        public ActionResult Edit(int data)
        {
            //var Holiday = db.Holiday
            //                    .Include(e => e.HolidayType)
            //                    .Include(e => e.HolidayName)
            //                    .Where(e => e.Id == data).ToList();
            //var r = (from ca in Holiday
            //         select new
            //         {

            //             Id = ca.Id,
            //             HolidayName = ca.HolidayName.Id,
            //             HolidayType = ca.HolidayType.Id,
            //             Action = ca.DBTrack.Action
            //         }).Distinct();

            //var a = "";
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.Holiday.Include(e => e.HolidayName).Include(e => e.HolidayType)
                       .Where(e => e.Id == data)
                        .Select(e => new
                        {
                            HolidayName = e.HolidayName != null ? e.HolidayName.Id : 0,
                            HolidayType = e.HolidayType != null ? e.HolidayType.Id : 0,
                            Action = e.DBTrack.Action

                        }).ToList();
                var W = db.DT_Holiday
                     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id,
                         HolidayName = e.HolidayName_Id == 0 ? "" : db.LookupValue
                                    .Where(x => x.Id == e.HolidayName_Id)
                                    .Select(x => x.LookupVal).FirstOrDefault(),
                         HolidayType = e.HolidayType_Id == 0 ? "" : db.LookupValue
                                     .Where(x => x.Id == e.HolidayType_Id)
                                     .Select(x => x.LookupVal).FirstOrDefault(),

                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();
                var LKup = db.Holiday.Find(data);
                TempData["RowVersion"] = LKup.RowVersion;
                var Auth = LKup.DBTrack.IsModified;

                return this.Json(new Object[] { Q, "", W, Auth, JsonRequestBehavior.AllowGet });
            }
        }
        public async Task<ActionResult> EditSave(Holiday ESOBJ, FormCollection form, int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {

                    bool Auth = form["Autho_Allow"] == "true" ? true : false;
                    string HolidayType = form["HolidayTypelist"] == "0" ? "" : form["HolidayTypelist"];
                    if (HolidayType != null)
                    {
                        if (HolidayType != "")
                        {
                            var val = db.LookupValue.Find(int.Parse(HolidayType));
                            ESOBJ.HolidayType = val;
                        }
                    }
                    string HolidayName = form["HolidayNamelist"] == "0" ? "" : form["HolidayNamelist"];
                    if (HolidayName != null)
                    {
                        if (HolidayName != "")
                        {
                            var val = db.LookupValue.Find(int.Parse(HolidayName));
                            ESOBJ.HolidayName = val;
                        }
                    }
                    if (HolidayName != null)
                    {
                        if (HolidayName != "")
                        {
                            var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "202").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(HolidayName)).FirstOrDefault();// db.LookupValue.Find(int.Parse(HolidayName));
                            ESOBJ.HolidayName = val;

                            var type = db.Holiday.Include(e => e.HolidayName)
                                .Where(e => e.Id == data).SingleOrDefault();
                            IList<Holiday> typedetails = null;
                            if (type.HolidayName != null)
                            {
                                typedetails = db.Holiday.Where(x => x.HolidayName.Id == type.HolidayName.Id && x.Id == data).ToList();
                            }
                            else
                            {
                                typedetails = db.Holiday.Where(x => x.Id == data).ToList();
                            }
                            //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                            foreach (var s in typedetails)
                            {
                                s.HolidayName = ESOBJ.HolidayName;
                                db.Holiday.Attach(s);
                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                //await db.SaveChangesAsync();
                                db.SaveChanges();
                                TempData["RowVersion"] = s.RowVersion;
                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                            }
                        }
                        else
                        {
                            var Dtls = db.Holiday.Include(e => e.HolidayName).Where(x => x.Id == data).ToList();
                            foreach (var s in Dtls)
                            {
                                s.HolidayName = null;
                                db.Holiday.Attach(s);
                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                //await db.SaveChangesAsync();
                                db.SaveChanges();
                                TempData["RowVersion"] = s.RowVersion;
                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                            }
                        }
                    }
                    if (HolidayType != null)
                    {
                        if (HolidayType != "")
                        {
                            var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "203").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(HolidayType)).FirstOrDefault();  //db.LookupValue.Find(int.Parse(HolidayType));
                            ESOBJ.HolidayType = val;

                            var type = db.Holiday
                                .Include(e => e.HolidayType)
                                .Where(e => e.Id == data).SingleOrDefault();
                            IList<Holiday> typedetails = null;
                            if (type.HolidayType != null)
                            {
                                typedetails = db.Holiday.Where(x => x.HolidayType.Id == type.HolidayType.Id && x.Id == data).ToList();
                            }
                            else
                            {
                                typedetails = db.Holiday.Where(x => x.Id == data).ToList();
                            }
                            //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                            foreach (var s in typedetails)
                            {
                                s.HolidayType = ESOBJ.HolidayType;
                                db.Holiday.Attach(s);
                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                //await db.SaveChangesAsync();
                                db.SaveChanges();
                                TempData["RowVersion"] = s.RowVersion;
                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                            }
                        }
                        else
                        {
                            var Dtls = db.Holiday.Include(e => e.HolidayType).Where(x => x.Id == data).ToList();
                            foreach (var s in Dtls)
                            {
                                s.HolidayType = null;
                                db.Holiday.Attach(s);
                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                //await db.SaveChangesAsync();
                                db.SaveChanges();
                                TempData["RowVersion"] = s.RowVersion;
                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                            }
                        }
                    }
                    if (Auth == false)
                    {
                    //    if (ModelState.IsValid)
                    //    {
                    //        try
                    //        {
                    //            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    //            {
                    //                Holiday blog = null; // to retrieve old data
                    //                DbPropertyValues originalBlogValues = null;

                    //                using (var context = new DataBaseContext())
                    //                {
                    //                    blog = context.Holiday.Where(e => e.Id == data)
                    //                                            .Include(e => e.HolidayType)
                    //                                            .Include(e => e.HolidayName)
                    //                                            .SingleOrDefault();
                    //                    originalBlogValues = context.Entry(blog).OriginalValues;
                    //                }

                    //                ESOBJ.DBTrack = new DBTrack
                    //                {
                    //                    CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                    //                    CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                    //                    Action = "M",
                    //                    ModifiedBy = SessionManager.UserName,
                    //                    ModifiedOn = DateTime.Now
                    //                };

                    //                int a = EditS(HolidayName, HolidayType, data, ESOBJ, ESOBJ.DBTrack);



                    //                using (var context = new DataBaseContext())
                    //                {
                    //                    var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, ESOBJ.DBTrack);
                    //                    DT_Holiday DT_OBJ = (DT_Holiday)obj;
                    //                    DT_OBJ.HolidayType_Id = blog.HolidayType == null ? 0 : blog.HolidayType.Id;
                    //                    DT_OBJ.HolidayName_Id = blog.HolidayName == null ? 0 : blog.HolidayName.Id;
                    //                    db.Create(DT_OBJ);
                    //                    db.SaveChanges();
                    //                }
                    //                await db.SaveChangesAsync();
                    //                ts.Complete();

                    //                Msg.Add("  Record Updated");
                    //                return Json(new Utility.JsonReturnClass { Id = data, Val = ESOBJ.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //                // return Json(new Object[] { ESOBJ.Id, ESOBJ.HolidayName.LookupVal, "Record Updated", JsonRequestBehavior.AllowGet });

                    //            }
                    //        }

                    //        //catch (DbUpdateException e) { throw e; }
                    //        //catch (DataException e) { throw e; }
                    //        catch (DbUpdateConcurrencyException ex)
                    //        {
                    //            var entry = ex.Entries.Single();
                    //            var clientValues = (Holiday)entry.Entity;
                    //            var databaseEntry = entry.GetDatabaseValues();
                    //            if (databaseEntry == null)
                    //            {
                    //                Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                    //                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //                // return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                    //            }
                    //            else
                    //            {
                    //                var databaseValues = (Holiday)databaseEntry.ToObject();
                    //                ESOBJ.RowVersion = databaseValues.RowVersion;

                    //            }
                    //        }
                    //    }
                    //    else
                    //    {
                    //        StringBuilder sb = new StringBuilder("");
                    //        foreach (ModelState modelState in ModelState.Values)
                    //        {
                    //            foreach (ModelError error in modelState.Errors)
                    //            {
                    //                sb.Append(error.ErrorMessage);
                    //                sb.Append("." + "\n");
                    //            }
                    //        }
                    //        var errorMsg = sb.ToString();
                    //        Msg.Add(errorMsg);
                    //        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //        // return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
                    //    }
                    //}
                        //if (ModelState.IsValid)
                        //{
                        //    try
                        //    {
                        //        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        //        {
                        //            Holiday blog = null; // to retrieve old data
                        //            DbPropertyValues originalBlogValues = null;

                        //            using (var context = new DataBaseContext())
                        //            {
                        //                blog = context.Holiday.Where(e => e.Id == data)
                        //                                        .Include(e => e.HolidayType)
                        //                                        .Include(e => e.HolidayName)
                        //                                        .SingleOrDefault();
                        //                originalBlogValues = context.Entry(blog).OriginalValues;
                        //            }

                        //            ESOBJ.DBTrack = new DBTrack
                        //            {
                        //                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                        //                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                        //                Action = "M",
                        //                ModifiedBy = SessionManager.UserName,
                        //                ModifiedOn = DateTime.Now
                        //            };
                        //            var m1 = db.Holiday.Where(e => e.Id == data).ToList();
                        //            foreach (var s in m1)
                        //            {
                        //                // s.AppraisalPeriodCalendar = null;
                        //                db.Holiday.Attach(s);
                        //                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //                //await db.SaveChangesAsync();
                        //                db.SaveChanges();
                        //                TempData["RowVersion"] = s.RowVersion;
                        //                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        //            }
                        //            //int a = EditS(Category, PromoPolicy, data, c, c.DBTrack);
                        //            var CurCorp = db.Holiday.Find(data);
                        //            TempData["CurrRowVersion"] = CurCorp.RowVersion;
                        //            db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                        //            if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                        //            {

                        //                Holiday corp = new Holiday()
                        //                {
                        //                    HolidayName = ESOBJ.HolidayName,
                        //                    HolidayType = ESOBJ.HolidayType,
                                           
                        //                    Id = data,
                        //                    DBTrack = ESOBJ.DBTrack
                        //                };

                        //                db.Holiday.Attach(corp);
                        //                db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                        //                db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                        //                // return 1;
                        //            }

                        //            using (var context = new DataBaseContext())
                        //            {
                        //                var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, ESOBJ.DBTrack);
                        //                DT_Holiday DT_Corp = (DT_Holiday)obj;
                        //                DT_Corp.HolidayName_Id = blog.HolidayName == null ? 0 : blog.HolidayName.Id;
                        //                DT_Corp.HolidayType_Id = blog.HolidayType == null ? 0 : blog.HolidayType.Id;

                        //                db.Create(DT_Corp);
                        //                db.SaveChanges();
                        //            }
                        //            await db.SaveChangesAsync();
                        //            ts.Complete();
                        //            Msg.Add("  Record Updated");
                        //            return Json(new Utility.JsonReturnClass { Id = ESOBJ.Id, Val = ESOBJ.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //            // return Json(new Object[] { c.Id, c.Name, "Record Updated", JsonRequestBehavior.AllowGet });
                        //        }
                        //    }
                        //    catch (DbUpdateConcurrencyException ex)
                        //    {
                        //        var entry = ex.Entries.Single();
                        //        var clientValues = (PromoActivity)entry.Entity;
                        //        var databaseEntry = entry.GetDatabaseValues();
                        //        if (databaseEntry == null)
                        //        {
                        //            Msg.Add(" Unable to save changes. The record was deleted by another user.");
                        //            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //            // return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                        //        }
                        //        else
                        //        {
                        //            var databaseValues = (PromoActivity)databaseEntry.ToObject();
                        //            ESOBJ.RowVersion = databaseValues.RowVersion;

                        //        }
                        //    }
                        //    Msg.Add("Record modified by another user.So refresh it and try to save again.");
                        //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //    //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                        //}

                        if (ModelState.IsValid)
                        {
                            try
                            {
                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    Holiday blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.Holiday.Where(e => e.Id == data)
                                                                .Include(e => e.HolidayType)
                                                                .Include(e => e.HolidayName)
                                                                .SingleOrDefault();
                                        originalBlogValues = context.Entry(blog).OriginalValues;
                                    }

                                    ESOBJ.DBTrack = new DBTrack
                                    {
                                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };
                                    var m1 = db.Holiday.Where(e => e.Id == data).ToList();
                                    foreach (var s in m1)
                                    {
                                        // s.AppraisalPeriodCalendar = null;
                                        db.Holiday.Attach(s);
                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                        //await db.SaveChangesAsync();
                                        db.SaveChanges();
                                        TempData["RowVersion"] = s.RowVersion;
                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    }
                                    //int a = EditS(Category, PromoPolicy, data, c, c.DBTrack);
                                    var CurCorp = db.Holiday.Find(data);
                                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {

                                        Holiday corp = new Holiday()
                                        {
                                            HolidayName = ESOBJ.HolidayName,
                                            HolidayType = ESOBJ.HolidayType,
                                         
                                            Id = data,
                                            DBTrack = ESOBJ.DBTrack
                                        };

                                        db.Holiday.Attach(corp);
                                        db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                                        db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                        // return 1;

                                        using (var context = new DataBaseContext())
                                        {
                                            var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, ESOBJ.DBTrack);
                                            DT_Holiday DT_Corp = (DT_Holiday)obj;
                                            DT_Corp.HolidayName_Id = blog.HolidayName == null ? 0 : blog.HolidayName.Id;
                                            DT_Corp.HolidayType_Id = blog.HolidayType == null ? 0 : blog.HolidayType.Id;

                                            db.Create(DT_Corp);
                                            db.SaveChanges();
                                        }
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
                                    ESOBJ.RowVersion = databaseValues.RowVersion;

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

                            Holiday blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            Holiday Old_Obj = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.Holiday.Where(e => e.Id == data).SingleOrDefault();
                                originalBlogValues = context.Entry(blog).OriginalValues;
                            }
                            ESOBJ.DBTrack = new DBTrack
                            {
                                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                Action = "M",
                                IsModified = blog.DBTrack.IsModified == true ? true : false,
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };
                            Holiday corp = new Holiday()
                            {
                                Id = data,
                                DBTrack = ESOBJ.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };


                            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, corp, "Holiday", ESOBJ.DBTrack);
                                // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                Old_Obj = context.Holiday.Where(e => e.Id == data).Include(e => e.HolidayType)
                                    .Include(e => e.HolidayName).SingleOrDefault();
                                DT_Holiday DT_Corp = (DT_Holiday)obj;
                                DT_Corp.HolidayType_Id = DBTrackFile.ValCompare(Old_Obj.HolidayType, ESOBJ.HolidayType);//Old_Obj.Address == c.Address ? 0 : Old_Obj.Address == null && c.Address != null ? c.Address.Id : Old_Obj.Address.Id;
                                DT_Corp.HolidayName_Id = DBTrackFile.ValCompare(Old_Obj.HolidayName, ESOBJ.HolidayName); //Old_Obj.BusinessType == c.BusinessType ? 0 : Old_Obj.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_Obj.BusinessType.Id;                        
                                db.Create(DT_Corp);
                                //db.SaveChanges();
                            }
                            blog.DBTrack = ESOBJ.DBTrack;
                            db.Holiday.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = ESOBJ.HolidayName.LookupVal, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { blog.Id, ESOBJ.HolidayName.LookupVal, "Record Updated", JsonRequestBehavior.AllowGet });
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

                return View();
            }
        }
        public int EditS(string RMVal, string PerkHVal, int data, Holiday ESOBJ, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (RMVal != null)
                {
                    if (RMVal != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(RMVal));
                        ESOBJ.HolidayName = val;

                        var type = db.Holiday.Include(e => e.HolidayName)
                            .Where(e => e.Id == data).SingleOrDefault();
                        IList<Holiday> typedetails = null;
                        if (type.HolidayName != null)
                        {
                            typedetails = db.Holiday.Where(x => x.HolidayName.Id == type.HolidayName.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            typedetails = db.Holiday.Where(x => x.Id == data).ToList();
                        }
                        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                        foreach (var s in typedetails)
                        {
                            s.HolidayName = ESOBJ.HolidayName;
                            db.Holiday.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                    else
                    {
                        var Dtls = db.Holiday.Include(e => e.HolidayName).Where(x => x.Id == data).ToList();
                        foreach (var s in Dtls)
                        {
                            s.HolidayName = null;
                            db.Holiday.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                }
                else
                {
                    var Dtls = db.Holiday.Include(e => e.HolidayName).Where(x => x.Id == data).ToList();
                    foreach (var s in Dtls)
                    {
                        s.HolidayName = null;
                        db.Holiday.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }



                if (PerkHVal != null)
                {
                    if (PerkHVal != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(PerkHVal));
                        ESOBJ.HolidayType = val;

                        var type = db.Holiday
                            .Include(e => e.HolidayType)
                            .Where(e => e.Id == data).SingleOrDefault();
                        IList<Holiday> typedetails = null;
                        if (type.HolidayType != null)
                        {
                            typedetails = db.Holiday.Where(x => x.HolidayType.Id == type.HolidayType.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            typedetails = db.Holiday.Where(x => x.Id == data).ToList();
                        }
                        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                        foreach (var s in typedetails)
                        {
                            s.HolidayType = ESOBJ.HolidayType;
                            db.Holiday.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                    else
                    {
                        var Dtls = db.Holiday.Include(e => e.HolidayType).Where(x => x.Id == data).ToList();
                        foreach (var s in Dtls)
                        {
                            s.HolidayType = null;
                            db.Holiday.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                }
                else
                {
                    var Dtls = db.Holiday.Include(e => e.HolidayType).Where(x => x.Id == data).ToList();
                    foreach (var s in Dtls)
                    {
                        s.HolidayType = null;
                        db.Holiday.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }
                var CurOBJ = db.Holiday.Find(data);
                TempData["CurrRowVersion"] = CurOBJ.RowVersion;
                db.Entry(CurOBJ).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    ESOBJ.DBTrack = dbT;
                    Holiday ESIOBJ = new Holiday()
                    {
                        Id = data,
                        DBTrack = ESOBJ.DBTrack
                    };

                    db.Holiday.Attach(ESIOBJ);
                    db.Entry(ESIOBJ).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(ESIOBJ).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    //// DBTrackFile.DBTrackSave("Payroll", originalBlogValues, db.ChangeTracker, c.DBTrack);
                    return 1;
                }
                return 0;
            }
        }
        public ActionResult Delete(int? data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                Holiday Holiday = db.Holiday.Where(e => e.Id == data).SingleOrDefault();
                try
                {
                    //var selectedValues = Holiday.SocialActivities;
                    //var lkValue = new HashSet<int>(Holiday.SocialActivities.Select(e => e.Id));
                    //if (lkValue.Count > 0)
                    //{
                    //    return this.Json(new Object[] { "", "Child record exists.Cannot delete.", JsonRequestBehavior.AllowGet });
                    //}

                    using (TransactionScope ts = new TransactionScope())
                    {
                        db.Entry(Holiday).State = System.Data.Entity.EntityState.Deleted;
                        db.SaveChanges();
                        ts.Complete();
                    }
                    Msg.Add("  Data removed successfully.  ");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    // return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                }

                catch (DataException /* dex */)
                {
                    //return this.Json(new Object[] { "", "Unable to delete. Try again, and if the problem persists contact your system administrator.", JsonRequestBehavior.AllowGet });
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
                var LKVal = db.Holiday.ToList();

                if (gp.IsAutho == true)
                {
                    LKVal = db.Holiday.Include(e => e.HolidayType).Include(e => e.HolidayName).AsNoTracking().ToList();
                }
                else
                {
                    LKVal = db.Holiday.Include(e => e.HolidayType).Include(e => e.HolidayName).AsNoTracking().ToList();
                }


                IEnumerable<Holiday> IE;
                if (!string.IsNullOrEmpty(gp.searchString))
                {
                    IE = LKVal;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Select(a => new { a.Id, a.HolidayType, a.HolidayName }).Where((e => (e.Id.ToString() == gp.searchString) || (e.HolidayType.LookupVal.ToLower() == gp.searchString) || (e.HolidayName.LookupVal.ToLower() == gp.searchString.ToLower())));
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.HolidayType.LookupVal, a.HolidayName.LookupVal }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = LKVal;
                    Func<Holiday, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c =>
                                         gp.sidx == "HolidayType" ? c.HolidayType.LookupVal.ToString() :
                                         gp.sidx == "HolidayName" ? c.HolidayName.LookupVal.ToString() :

                                         "");
                    }

                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.HolidayType.LookupVal, a.HolidayName.LookupVal }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.HolidayType.LookupVal, a.HolidayName.LookupVal }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.HolidayType.LookupVal, a.HolidayName.LookupVal }).ToList();
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
                             Holiday ESI = db.Holiday
                                 .Include(e => e.HolidayName)
                                 .Include(e => e.HolidayType)
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

                             db.Holiday.Attach(ESI);
                             db.Entry(ESI).State = System.Data.Entity.EntityState.Modified;
                             db.Entry(ESI).OriginalValues["RowVersion"] = TempData["RowVersion"];
                             //db.SaveChanges();
                             var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, ESI.DBTrack);
                             DT_Holiday DT_OBJ = (DT_Holiday)rtn_Obj;

                             db.Create(DT_OBJ);
                             await db.SaveChangesAsync();

                             ts.Complete();
                             Msg.Add("  Record Authorised");
                             return Json(new Utility.JsonReturnClass { Id = ESI.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                             //return Json(new Object[] { ESI.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                         }
                     }
                     else if (auth_action == "M")
                     {

                         Holiday Old_OBJ = db.Holiday
                                                 .Include(e => e.HolidayName)
                                                 .Include(e => e.HolidayType)
                                                 .Where(e => e.Id == auth_id).SingleOrDefault();


                         DT_Holiday Curr_OBJ = db.DT_Holiday
                                                     .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                                     .OrderByDescending(e => e.Id)
                                                     .FirstOrDefault();

                         if (Curr_OBJ != null)
                         {
                             Holiday Holiday = new Holiday();
                             string HolidayName = Curr_OBJ.HolidayName_Id == null ? null : Curr_OBJ.HolidayName_Id.ToString();
                             string HolidayType = Curr_OBJ.HolidayType_Id == null ? null : Curr_OBJ.HolidayType_Id.ToString();


                             //      corp.Id = auth_id;

                             if (ModelState.IsValid)
                             {
                                 try
                                 {

                                     //DbContextTransaction transaction = db.Database.BeginTransaction();

                                     using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                     {
                                         // db.Configuration.AutoDetectChangesEnabled = false;
                                         Holiday.DBTrack = new DBTrack
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

                                         int a = EditS(HolidayName, HolidayType, auth_id, Holiday, Holiday.DBTrack);

                                         await db.SaveChangesAsync();

                                         ts.Complete();
                                         Msg.Add("  Record Authorised");
                                         return Json(new Utility.JsonReturnClass { Id = Holiday.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                         //return Json(new Object[] { Holiday.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                                     }
                                 }
                                 catch (DbUpdateConcurrencyException ex)
                                 {
                                     var entry = ex.Entries.Single();
                                     var clientValues = (Holiday)entry.Entity;
                                     var databaseEntry = entry.GetDatabaseValues();
                                     if (databaseEntry == null)
                                     {
                                         Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                         return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                         // return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                     }
                                     else
                                     {
                                         var databaseValues = (Holiday)databaseEntry.ToObject();
                                         Holiday.RowVersion = databaseValues.RowVersion;
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
                         //return Json(new Object[] { "", "Data removed from history", JsonRequestBehavior.AllowGet });
                     }
                     else if (auth_action == "D")
                     {
                         using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                         {
                             //Holiday corp = db.Holiday.Find(auth_id);
                             Holiday ESI = db.Holiday.AsNoTracking().FirstOrDefault(e => e.Id == auth_id);

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

                             db.Holiday.Attach(ESI);
                             db.Entry(ESI).State = System.Data.Entity.EntityState.Deleted;


                             var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, ESI.DBTrack);
                             DT_Holiday DT_OBJ = (DT_Holiday)rtn_Obj;
                             //DT_OBJ.Address_Id = corp.Address == null ? 0 : corp.Address.Id;
                             //DT_OBJ.BusinessType_Id = corp.BusinessType == null ? 0 : corp.BusinessType.Id;
                             //DT_OBJ.ContactDetails_Id = corp.ContactDetails == null ? 0 : corp.ContactDetails.Id;
                             db.Create(DT_OBJ);
                             await db.SaveChangesAsync();
                             db.Entry(ESI).State = System.Data.Entity.EntityState.Detached;
                             ts.Complete();
                             Msg.Add(" Record Authorised ");
                             return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                             // return Json(new Object[] { "", "Record Authorised", JsonRequestBehavior.AllowGet });
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
                 return View();
             }

        }

        public ActionResult PopulateDropDownListCalendar(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var qurey = db.Calendar.ToList();
                var selected = (Object)null;
                if (data2 != "" && data != "0" && data2 != "0")
                {
                    selected = Convert.ToInt32(data2);
                }

                SelectList s = new SelectList(qurey, "Id", "FullDetails", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult ValidateForm(Holiday frmholiday, FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string HolidayType = form["HolidayTypelist"] == "0" ? "" : form["HolidayTypelist"];
                string HolidayName = form["HolidayNamelist"] == "0" ? "" : form["HolidayNamelist"];
                if (HolidayName != null)
                {
                    if (HolidayName != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(HolidayName));
                        frmholiday.HolidayName = val;
                    }
                }
                if (db.Holiday.Any(e => e.HolidayName.Id == frmholiday.HolidayName.Id))
                {
                    var Msg = new List<string>();
                    Msg.Add("Holiday Already Exist");
                    return Json(new { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                }
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
