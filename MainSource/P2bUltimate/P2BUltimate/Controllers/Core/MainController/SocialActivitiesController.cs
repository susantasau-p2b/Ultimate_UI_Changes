///
/// Created by Sarika
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
    public class SocialActivitiesController : Controller
    {
        //
        // GET: /SocialActivities/

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult partial()
        {
            return View("~/Views/Shared/Core/_SocialActivities.cshtml");
        }

        //private DataBaseContext db = new DataBaseContext();






        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    SocialActivities SocialActivities = db.SocialActivities.Where(e => e.Id == data).SingleOrDefault();
                    if (SocialActivities.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, SocialActivities.DBTrack, SocialActivities, null, "Corporate");
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = SocialActivities.DBTrack.CreatedBy != null ? SocialActivities.DBTrack.CreatedBy : null,
                                CreatedOn = SocialActivities.DBTrack.CreatedOn != null ? SocialActivities.DBTrack.CreatedOn : null,
                                IsModified = SocialActivities.DBTrack.IsModified == true ? true : false
                            };
                            SocialActivities.DBTrack = dbT;
                            db.Entry(SocialActivities).State = System.Data.Entity.EntityState.Modified;
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", SocialActivities, null, "Corporate", SocialActivities.DBTrack);
                            await db.SaveChangesAsync();
                            using (var context = new DataBaseContext())
                            {
                                /// DBTrackFile.DBTrackSave("Core/P2b.Global", "D", SocialActivities, null, "Corporate", SocialActivities.DBTrack);
                            }
                            ts.Complete();
                            //   return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }
                    }
                    else
                    {
                        //var selectedRegions = SocialActivities.Regions;

                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //if (selectedRegions != null)
                            //{
                            //    var corpRegion = new HashSet<int>(SocialActivities.Regions.Select(e => e.Id));
                            //    if (corpRegion.Count > 0)
                            //    {
                            //        return Json(new Object[] { "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                            //        // return Json(new Object[] { "", "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                            //    }
                            //}

                            try
                            {
                                DBTrack dbT = new DBTrack
                                {
                                    Action = "D",
                                    ModifiedBy = SessionManager.UserName,
                                    ModifiedOn = DateTime.Now,
                                    CreatedBy = SocialActivities.DBTrack.CreatedBy != null ? SocialActivities.DBTrack.CreatedBy : null,
                                    CreatedOn = SocialActivities.DBTrack.CreatedOn != null ? SocialActivities.DBTrack.CreatedOn : null,
                                    IsModified = SocialActivities.DBTrack.IsModified == true ? false : false,
                                    AuthorizedBy = SessionManager.UserName,
                                    AuthorizedOn = DateTime.Now
                                };

                                // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };

                                db.Entry(SocialActivities).State = System.Data.Entity.EntityState.Deleted;
                                await db.SaveChangesAsync();


                                using (var context = new DataBaseContext())
                                {
                                    //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", SocialActivities, null, "Corporate", dbT);
                                }
                                ts.Complete();
                                // return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                                Msg.Add("  Data removed successfully.  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }
                            catch (RetryLimitExceededException /* dex */)
                            {
                                //  return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
                                Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);


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


        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.SocialActivities
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        InstituteName = e.InstituteName,
                        PostHeld = e.PostHeld,
                        FromPeriod = e.FromPeriod,
                        ToPeriod = e.ToPeriod,
                        Action = e.DBTrack.Action
                    }).ToList();
                var Old_Data = db.DT_PassportDetails
                     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id,
                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();
                var EOBJ = db.SocialActivities.Find(data);
                TempData["RowVersion"] = EOBJ.RowVersion;
                var Auth = EOBJ.DBTrack.IsModified;
                return Json(new Object[] { Q, "", Old_Data, Auth, JsonRequestBehavior.AllowGet });
            }
        }


        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult Create(SocialActivities OBJ, FormCollection form) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {

                            if (OBJ.FromPeriod > OBJ.ToPeriod)
                            {

                                Msg.Add("  From Period should be less than To Period.  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }

                            if (db.SocialActivities.Any(o => o.InstituteName == OBJ.InstituteName))
                            {
                                Msg.Add("SocialActivities Already Exists. ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                            OBJ.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName };
                            SocialActivities COBJ = new SocialActivities()
                            {
                                Id = OBJ.Id,
                                FromPeriod = OBJ.FromPeriod,
                                PostHeld = OBJ.PostHeld,
                                InstituteName = OBJ.InstituteName,
                                ToPeriod = OBJ.ToPeriod,
                                DBTrack = OBJ.DBTrack
                            };
                            try
                            {
                                db.SocialActivities.Add(COBJ);
                                db.SaveChanges();
                                //DBTrackFile.DBTrackSave("Core/P2b.Global", "C", COBJ, null, "SocialActivities", null);
                                ts.Complete();
                                // return this.Json(new Object[] { COBJ.Id, COBJ.InstituteName, "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = COBJ.Id, Val = COBJ.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = OBJ.Id });
                            }
                            catch (DataException /* dex */)
                            {
                                //  return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
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

        public int EditS(int data, SocialActivities EOBJ, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var CurOBJ = db.SocialActivities.Find(data);
                TempData["CurrRowVersion"] = CurOBJ.RowVersion;
                db.Entry(CurOBJ).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    EOBJ.DBTrack = dbT;
                    SocialActivities OBJ = new SocialActivities()
                    {
                        Id = data,
                        FromPeriod = CurOBJ.FromPeriod,
                        PostHeld = EOBJ.PostHeld,
                        InstituteName = EOBJ.InstituteName,
                        ToPeriod = CurOBJ.ToPeriod,
                        DBTrack = EOBJ.DBTrack
                    };

                    db.SocialActivities.Attach(OBJ);
                    db.Entry(OBJ).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(OBJ).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    return 1;
                }
                return 0;
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
                IEnumerable<SocialActivities> SocialActivities = null;
                if (gp.IsAutho == true)
                {
                    SocialActivities = db.SocialActivities.Where(e => e.DBTrack.IsModified == true).ToList();
                }
                else
                {
                    SocialActivities = db.SocialActivities.ToList();
                }

                IEnumerable<SocialActivities> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = SocialActivities;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Select(a => new { a.Id, a.InstituteName, a.PostHeld, a.FromPeriod, a.ToPeriod }).Where((e => (e.Id.ToString() == gp.searchString) || (e.InstituteName.ToLower() == gp.searchString.ToLower()) || (e.PostHeld.ToLower() == gp.searchString.ToLower()) || (e.FromPeriod.ToString() == gp.searchString.ToLower()) || (e.ToPeriod.ToString() == gp.searchString.ToLower()))).ToList();
                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.InstituteName), Convert.ToString(a.PostHeld), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.InstituteName, a.PostHeld, a.FromPeriod, a.ToPeriod }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = SocialActivities;
                    Func<SocialActivities, int> orderfuc = (c =>
                                                               gp.sidx == "Id" ? c.Id : 0);
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.InstituteName), Convert.ToString(a.PostHeld), Convert.ToString(a.FromPeriod), Convert.ToString(a.ToPeriod) }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.InstituteName), Convert.ToString(a.PostHeld), Convert.ToString(a.FromPeriod), Convert.ToString(a.ToPeriod) }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.InstituteName, a.PostHeld, a.FromPeriod, a.ToPeriod }).ToList();
                    }
                    totalRecords = SocialActivities.Count();
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
        public ActionResult GetLookupDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.SocialActivities.ToList();
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.SocialActivities.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                    }
                }

                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }


        //[HttpPost]
        //public async Task<ActionResult> EditSave(SocialActivities ESOBJ, int data, FormCollection form) // Edit submit
        //{  List<string> Msg = new List<string>();
        //            try{

        //    bool Auth = form["Autho_Allow"] == "true" ? true : false;
        //    if (Auth == false)
        //    {
        //        if (ModelState.IsValid)
        //        {
        //            try
        //            {

        //                //DbContextTransaction transaction = db.Database.BeginTransaction();

        //                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                {
        //                    SocialActivities blog = null; // to retrieve old data
        //                    // DbPropertyValues originalBlogValues = null;

        //                    using (var context = new DataBaseContext())
        //                    {
        //                        blog = context.SocialActivities.Where(e => e.Id == data).AsNoTracking().SingleOrDefault();
        //                        // originalBlogValues = context.Entry(blog).OriginalValues;
        //                    }

        //                    ESOBJ.DBTrack = new DBTrack
        //                    {
        //                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                        Action = "M",
        //                        ModifiedBy = SessionManager.UserName,
        //                        ModifiedOn = DateTime.Now
        //                    };

        //                    int a = EditS(data, ESOBJ, ESOBJ.DBTrack);

        //                    await db.SaveChangesAsync();

        //                    using (var context = new DataBaseContext())
        //                    {
        //                        //To save data in history table 
        //                        //var Obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, ESOBJ, "SocialActivities", ESOBJ.DBTrack);
        //                        //DT_JobPosition DT_OBJ = (DT_JobPosition)Obj;
        //                        //db.DT_JobPosition.Add(DT_OBJ);
        //                        db.SaveChanges();
        //                    }
        //                    ts.Complete();
        //                  //  return Json(new Object[] { ESOBJ.Id, ESOBJ.InstituteName, "Record Updated", JsonRequestBehavior.AllowGet });
        //                     Msg.Add("  Record Updated");
        //            return Json(new Utility.JsonReturnClass { Id = ESOBJ .Id   , Val = ESOBJ.InstituteName , success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);


        //                }
        //            }
        //            catch (DbUpdateConcurrencyException ex)
        //            {
        //                var entry = ex.Entries.Single();
        //                var clientValues = (SocialActivities)entry.Entity;
        //                var databaseEntry = entry.GetDatabaseValues();
        //                if (databaseEntry == null)
        //                {
        //                  //  return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
        //               Msg.Add(" Unable to save changes. The record was deleted by another user.");				
        //            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                }
        //                else
        //                {
        //                    var databaseValues = (SocialActivities)databaseEntry.ToObject();
        //                    ESOBJ.RowVersion = databaseValues.RowVersion;

        //                }
        //            }
        //            Msg.Add("Record modified by another user.So refresh it and try to save again.");
        //            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);


        //           // return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
        //        }
        //    }
        //    else
        //    {
        //        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //        {
        //            SocialActivities Old_OBJ = db.SocialActivities.Where(e => e.Id == data).SingleOrDefault();

        //            SocialActivities Curr_OBJ = ESOBJ;
        //            ESOBJ.DBTrack = new DBTrack
        //            {
        //                CreatedBy = Old_OBJ.DBTrack.CreatedBy == null ? null : Old_OBJ.DBTrack.CreatedBy,
        //                CreatedOn = Old_OBJ.DBTrack.CreatedOn == null ? null : Old_OBJ.DBTrack.CreatedOn,
        //                Action = "M",
        //                IsModified = Old_OBJ.DBTrack.IsModified == true ? true : false,
        //                //ModifiedBy = SessionManager.UserName,
        //                //ModifiedOn = DateTime.Now
        //            };
        //            Old_OBJ.DBTrack = ESOBJ.DBTrack;

        //            db.Entry(Old_OBJ).State = System.Data.Entity.EntityState.Modified;
        //            db.SaveChanges();
        //            using (var context = new DataBaseContext())
        //            {
        //                //DBTrackFile.DBTrackSave("Core/P2b.Global", "M", Old_OBJ, Curr_OBJ, "SocialActivities", ESOBJ.DBTrack);
        //            }

        //            ts.Complete();
        //            //return Json(new Object[] { Old_OBJ.Id, ESOBJ.InstituteName, "Record Updated", JsonRequestBehavior.AllowGet });
        //       Msg.Add("  Record Updated");
        //            return Json(new Utility.JsonReturnClass { Id =  Old_OBJ.Id   , Val = ESOBJ.InstituteName , success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //        }

        //    }
        //    return View();
        //            }
        //            catch (Exception ex)
        //            {
        //                LogFile Logfile = new LogFile();
        //                ErrorLog Err = new ErrorLog()
        //                {
        //                    ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
        //                    ExceptionMessage = ex.Message,
        //                    ExceptionStackTrace = ex.StackTrace,
        //                    LineNo = Convert.ToString(new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
        //                    LogTime = DateTime.Now
        //                };
        //                Logfile.CreateLogFile(Err);
        //                Msg.Add(ex.Message);
        //                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //            } 
        //}

        //[HttpPost]
        //public async Task<ActionResult> EditSave(SocialActivities c, int data, FormCollection form) // Edit submit
        //{
        //    List<string> Msg = new List<string>();
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        try
        //        {
        //            var db_data = db.SocialActivities.Where(e => e.Id == data).SingleOrDefault();

        //            bool Auth = form["Autho_Allow"] == "true" ? true : false;




        //            //string Category = form["SkillTypelist"] == "0" ? "" : form["SkillTypelist"];

        //            //if (Category != null)
        //            //{
        //            //    if (Category != "")
        //            //    {
        //            //        var val = db.LookupValue.Find(int.Parse(Category));
        //            //        c.SkillType = val;
        //            //    }
        //            //}



        //            if (Auth == false)
        //            {


        //                if (ModelState.IsValid)
        //                {
        //                    try
        //                    {

        //                        //DbContextTransaction transaction = db.Database.BeginTransaction();

        //                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                        {
        //                            db.SocialActivities.Attach(db_data);
        //                            db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
        //                            db.SaveChanges();
        //                            TempData["RowVersion"] = db_data.RowVersion;
        //                            db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;

        //                            var Curr_Lookup = db.SocialActivities.Find(data);
        //                            TempData["CurrRowVersion"] = Curr_Lookup.RowVersion;
        //                            db.Entry(Curr_Lookup).State = System.Data.Entity.EntityState.Detached;

        //                            if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
        //                            {

        //                                SocialActivities blog = null; // to retrieve old data
        //                                DbPropertyValues originalBlogValues = null;

        //                                using (var context = new DataBaseContext())
        //                                {
        //                                    blog = context.SocialActivities.Where(e => e.Id == data)
        //                                                      .SingleOrDefault();
        //                                    originalBlogValues = context.Entry(blog).OriginalValues;
        //                                }

        //                                c.DBTrack = new DBTrack
        //                                {
        //                                    CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                                    CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                                    Action = "M",
        //                                    ModifiedBy = SessionManager.UserName,
        //                                    ModifiedOn = DateTime.Now
        //                                };
        //                                SocialActivities lk = new SocialActivities
        //                                {

        //                                    Id = data,
        //                                    FromPeriod = db_data.FromPeriod,
        //                                    PostHeld = c.PostHeld,
        //                                    InstituteName = c.InstituteName,
        //                                    ToPeriod = db_data.ToPeriod,
        //                                    DBTrack = c.DBTrack


        //                                };


        //                                db.SocialActivities.Attach(lk);
        //                                db.Entry(lk).State = System.Data.Entity.EntityState.Modified;

        //                                db.Entry(lk).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //                                // int a = EditS(Corp, Addrs, ContactDetails, data, c, c.DBTrack);

        //                                // int a = EditS(Values, Valueshobby, Valuesls, Valuesqd, Valuesskl, Valuesscr, data, c, c.DBTrack);


        //                                using (var context = new DataBaseContext())
        //                                {

        //                                    //var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
        //                                    //DT_SocialActivities DT_Corp = (DT_SocialActivities)obj;

        //                                    //db.Create(DT_Corp);
        //                                    db.SaveChanges();
        //                                }
        //                                await db.SaveChangesAsync();
        //                                ts.Complete();

        //                                Msg.Add("  Record Updated");
        //                                return Json(new Utility.JsonReturnClass { Id = lk.Id, Val = lk.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                                //return Json(new Object[] { lk.Id, "", "Record Updated", JsonRequestBehavior.AllowGet });
        //                            }
        //                        }
        //                    }
        //                    catch (DbUpdateConcurrencyException ex)
        //                    {
        //                        var entry = ex.Entries.Single();
        //                        var clientValues = (SocialActivities)entry.Entity;
        //                        var databaseEntry = entry.GetDatabaseValues();
        //                        if (databaseEntry == null)
        //                        {
        //                            Msg.Add(" Unable to save changes. The record was deleted by another user.");
        //                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                            //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
        //                        }
        //                        else
        //                        {
        //                            var databaseValues = (SocialActivities)databaseEntry.ToObject();
        //                            c.RowVersion = databaseValues.RowVersion;

        //                        }
        //                    }
        //                    Msg.Add("Record modified by another user.So refresh it and try to save again.");
        //                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                    // return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
        //                }
        //            }
        //            else
        //            {
        //                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                {

        //                    SocialActivities blog = null; // to retrieve old data
        //                    DbPropertyValues originalBlogValues = null;
        //                    SocialActivities Old_Corp = null;

        //                    using (var context = new DataBaseContext())
        //                    {
        //                        blog = context.SocialActivities.Where(e => e.Id == data).SingleOrDefault();
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
        //                    SocialActivities qualificationDetails = new SocialActivities()
        //                    {


        //                        Id = data,
        //                        FromPeriod = db_data.FromPeriod,
        //                        PostHeld = c.PostHeld,
        //                        InstituteName = c.InstituteName,
        //                        ToPeriod = db_data.ToPeriod,
        //                        DBTrack = c.DBTrack
        //                    };

        //                    using (var context = new DataBaseContext())
        //                    {
        //                        var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, qualificationDetails, "SocialActivities", c.DBTrack);
        //                        // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

        //                        Old_Corp = context.SocialActivities.Where(e => e.Id == data)
        //                        .SingleOrDefault();
        //                        DT_SocialActivities DT_Corp = (DT_SocialActivities)obj;
        //                        //DT_Corp.Address_Id = DBTrackFile.ValCompare(Old_Corp.Address, c.Address);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
        //                        //DT_Corp.BusinessType_Id = DBTrackFile.ValCompare(Old_Corp.BusinessType, c.BusinessType); //Old_Corp.BusinessType == c.BusinessType ? 0 : Old_Corp.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_Corp.BusinessType.Id;
        //                        //DT_Corp.ContactDetails_Id = DBTrackFile.ValCompare(Old_Corp.ContactDetails, c.ContactDetails); //Old_Corp.ContactDetails == c.ContactDetails ? 0 : Old_Corp.ContactDetails == null && c.ContactDetails != null ? c.ContactDetails.Id : Old_Corp.ContactDetails.Id;
        //                        db.Create(DT_Corp);
        //                        //db.SaveChanges();
        //                    }
        //                    blog.DBTrack = c.DBTrack;
        //                    db.SocialActivities.Attach(blog);
        //                    db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
        //                    db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //                    db.SaveChanges();
        //                    ts.Complete();

        //                    Msg.Add("  Record Updated");
        //                    return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = c.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                    //return Json(new Object[] { blog.Id, c.Institute, "Record Updated", JsonRequestBehavior.AllowGet });
        //                }
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
        //            Msg.Add(ex.Message);
        //            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //        }

        //        return View();
        //    }
        //}

        public ActionResult EditSave(SocialActivities c, int data, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                string InstituteName = form["InstituteName"] == "0" ? "" : form["InstituteName"];
                string PostHeld = form["PostHeld"] == "0" ? "" : form["PostHeld"];
                string FromPeriod = form["FromPeriod"] == "0" ? "" : form["FromPeriod"];
                string ToPeriod = form["ToPeriod"] == "0" ? "" : form["ToPeriod"];
                
                try
                {
                    var db_data = db.SocialActivities.Where(e => e.Id == data).SingleOrDefault();

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            db.SocialActivities.Attach(db_data);
                            db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            TempData["RowVersion"] = db_data.RowVersion;
                            db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;

                            SocialActivities blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            using (var context = new DataBaseContext())
                            {
                                blog = context.SocialActivities.Where(e => e.Id == data).SingleOrDefault();
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

                            SocialActivities corp = new SocialActivities()
                            {
                                InstituteName = c.InstituteName,
                                PostHeld = c.PostHeld,
                                FromPeriod = c.FromPeriod,
                                ToPeriod = c.ToPeriod,
                                Id = data,
                                DBTrack = c.DBTrack
                            };
                            try
                            {
                                db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                                db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];

                                db.SaveChanges();
                                ts.Complete();
                                //return this.Json(new { msg = "Data saved successfully." });
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = corp.Id, Val = corp.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = corp.Id });
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
                        return this.Json(new { msg = errorMsg, JsonRequestBehavior.AllowGet });
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

    }
}
