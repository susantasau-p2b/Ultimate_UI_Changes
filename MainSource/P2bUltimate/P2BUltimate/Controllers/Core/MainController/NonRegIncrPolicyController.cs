///
/// Created by Sarika
///

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

namespace P2BUltimate.Controllers.Core.MainController
{
    public class NonRegIncrPolicyController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();
        // GET: /NonRegIncrPolicy/
        public ActionResult Index()
        {
            return View("~/Views/Core/MainViews/NonRegIncrPolicy/Index.cshtml");
        }

        public ActionResult partial()
        {
            return View("~/Views/Shared/Core/_NonRegIncrPolicy.cshtml");
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
                IEnumerable<NonRegIncrPolicy> NonRegIncrPolicy = null;
                if (gp.IsAutho == true)
                {
                    NonRegIncrPolicy = db.NonRegIncrPolicy.Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    NonRegIncrPolicy = db.NonRegIncrPolicy.AsNoTracking().ToList();
                }

                IEnumerable<NonRegIncrPolicy> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = NonRegIncrPolicy;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Select(a => new { a.Id, a.MaxIncrInService, a.MaxService, a.MinService }).Where((e => (e.Id.ToString() == gp.searchString) || (e.MaxIncrInService.ToString() == gp.searchString.ToString()) || (e.MaxService.ToString() == gp.searchString.ToString()) || (e.MinService.ToString() == gp.searchString.ToString()))).ToList();
                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.MaxIncrInService, a.MaxService, a.MinService }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = NonRegIncrPolicy;
                    Func<NonRegIncrPolicy, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "MaxIncrInService" ? c.MaxIncrInService.ToString() : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.MaxIncrInService), Convert.ToString(a.MaxService), a.MinService }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.MaxIncrInService), Convert.ToString(a.MaxService), a.MinService }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.MaxIncrInService, a.MaxService, a.MinService }).ToList();
                    }
                    totalRecords = NonRegIncrPolicy.Count();
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
        public ActionResult Create(NonRegIncrPolicy c)
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
                            //if (db.NonRegIncrPolicy.Any(o => o. == c.MinService ))
                            //{
                            //    return Json(new Object[] { "", "", "Code Already Exists.", JsonRequestBehavior.AllowGet });
                            //}

                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            NonRegIncrPolicy NRegIncrPolicy = new NonRegIncrPolicy
                            {
                                MinService = c.MinService,
                                MinSerAppl = c.MinSerAppl,
                                MaxService = c.MaxService,
                                MaxSerLockAppl = c.MaxSerLockAppl,
                                MaxIncrInService = c.MaxIncrInService,
                                //FullDetails=c.FullDetails,
                                DBTrack = c.DBTrack
                            };
                            try
                            {
                                db.NonRegIncrPolicy.Add(NRegIncrPolicy);
                                var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, c.DBTrack);
                                DT_NonRegIncrPolicy DT_Corp = (DT_NonRegIncrPolicy)rtn_Obj;

                                db.Create(DT_Corp);
                                db.SaveChanges();
                                //DBTrackFile.DBTrackSave("Core/P2b.Global", "C", corporate, null, "Corporate", null);


                                ts.Complete();
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = NRegIncrPolicy.Id, Val = NRegIncrPolicy.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return this.Json(new Object[] { NRegIncrPolicy.Id, NRegIncrPolicy.FullDetails, "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
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

        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    NonRegIncrPolicy corporates = db.NonRegIncrPolicy.Where(e => e.Id == data).SingleOrDefault();

                    //Corporate corporates = db.Corporate.Where(e => e.Id == data).SingleOrDefault();
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
                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack);
                            DT_NonRegIncrPolicy DT_Corp = (DT_NonRegIncrPolicy)rtn_Obj;
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
                                    CreatedBy = corporates.DBTrack.CreatedBy != null ? corporates.DBTrack.CreatedBy : null,
                                    CreatedOn = corporates.DBTrack.CreatedOn != null ? corporates.DBTrack.CreatedOn : null,
                                    IsModified = corporates.DBTrack.IsModified == true ? false : false//,
                                    //AuthorizedBy = SessionManager.UserName,
                                    //AuthorizedOn = DateTime.Now
                                };

                                // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };

                                db.Entry(corporates).State = System.Data.Entity.EntityState.Deleted;
                                var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dbT);
                                DT_NonRegIncrPolicy DT_Corp = (DT_NonRegIncrPolicy)rtn_Obj;

                                db.Create(DT_Corp);

                                await db.SaveChangesAsync();

                                ts.Complete();
                                //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                                Msg.Add("  Data removed successfully.  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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


        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.NonRegIncrPolicy
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        MinService = e.MinService,
                        MinSerAppl = e.MinSerAppl,
                        MaxService = e.MaxService,
                        MaxSerLockAppl = e.MaxSerLockAppl,
                        MaxIncrInService = e.MaxIncrInService,
                        Action = e.DBTrack.Action
                    }).ToList();

                var add_data = db.NonRegIncrPolicy
                    .Where(e => e.Id == data)
                    .ToList();


                var W = db.DT_NonRegIncrPolicy
                     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id,
                         MinService = e.MinService,
                         MinSerAppl = e.MinSerAppl,
                         MaxService = e.MaxService,
                         MaxSerLockAppl = e.MaxSerLockAppl,
                         MaxIncrInService = e.MaxIncrInService
                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var Corp = db.NonRegIncrPolicy.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, add_data, W, Auth, JsonRequestBehavior.AllowGet });
            }
        }


        //public async Task<ActionResult> EditSave(NonRegIncrPolicy c, int data, FormCollection form) // Edit submit
        //{

        //    List<string> Msg = new List<string>();
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
        //                    NonRegIncrPolicy blog = null; // to retrieve old data
        //                    DbPropertyValues originalBlogValues = null;

        //                    using (var context = new DataBaseContext())
        //                    {
        //                        blog = context.NonRegIncrPolicy.Where(e => e.Id == data).SingleOrDefault();
        //                        originalBlogValues = context.Entry(blog).OriginalValues;
        //                    }

        //                    c.DBTrack = new DBTrack
        //                    {
        //                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                        Action = "M",
        //                        ModifiedBy = SessionManager.UserName,
        //                        ModifiedOn = DateTime.Now
        //                    };

        //                    int a = EditS(data, c, c.DBTrack);



        //                    using (var context = new DataBaseContext())
        //                    {
        //                        var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
        //                        DT_NonRegIncrPolicy DT_Corp = (DT_NonRegIncrPolicy)obj;
        //                        db.Create(DT_Corp);
        //                        db.SaveChanges();
        //                    }
        //                    await db.SaveChangesAsync();
        //                    ts.Complete();
        //                     Msg.Add("  Record Updated");
        //            return Json(new Utility.JsonReturnClass { Id = c .Id   , Val =  c.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);


        //                    //return Json(new Object[] { c.Id, c.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });

        //                }
        //            }
        //            catch (DbUpdateConcurrencyException ex)
        //            {
        //                var entry = ex.Entries.Single();
        //                var clientValues = (NonRegIncrPolicy)entry.Entity;
        //                var databaseEntry = entry.GetDatabaseValues();
        //                if (databaseEntry == null)
        //                {
        //                    Msg.Add(" Unable to save changes. The record was deleted by another user.");				
        //            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                    //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
        //                }
        //                else
        //                {
        //                    var databaseValues = (NonRegIncrPolicy)databaseEntry.ToObject();
        //                    c.RowVersion = databaseValues.RowVersion;

        //                }
        //            }
        //            Msg.Add("Record modified by another user.So refresh it and try to save again.");
        //            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //            //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
        //        }
        //    }
        //    else
        //    {
        //        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //        {

        //            NonRegIncrPolicy blog = null; // to retrieve old data
        //            DbPropertyValues originalBlogValues = null;
        //            NonRegIncrPolicy Old_Corp = null;

        //            using (var context = new DataBaseContext())
        //            {
        //                blog = context.NonRegIncrPolicy.Where(e => e.Id == data).SingleOrDefault();
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
        //            NonRegIncrPolicy corp = new NonRegIncrPolicy()
        //            {
        //                MinService = c.MinService,
        //                MinSerAppl = c.MinSerAppl,
        //                MaxService = c.MaxService,
        //                MaxSerLockAppl = c.MaxSerLockAppl,
        //                MaxIncrInService = c.MaxIncrInService,
        //                Id = data,
        //                DBTrack = c.DBTrack,
        //                RowVersion = (Byte[])TempData["RowVersion"]
        //            };


        //            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

        //            using (var context = new DataBaseContext())
        //            {
        //                var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, corp, "NonRegIncrPolicy", c.DBTrack);
        //                // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

        //                Old_Corp = context.NonRegIncrPolicy.Where(e => e.Id == data).SingleOrDefault();
        //                DT_NonRegIncrPolicy DT_Corp = (DT_NonRegIncrPolicy)obj;
        //                db.Create(DT_Corp);
        //                //db.SaveChanges();
        //            }
        //            blog.DBTrack = c.DBTrack;
        //            db.NonRegIncrPolicy.Attach(blog);
        //            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
        //            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //            db.SaveChanges();
        //            ts.Complete();
        //            Msg.Add("  Record Updated");
        //            return Json(new Utility.JsonReturnClass { Id = blog .Id   , Val = c.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //            //return Json(new Object[] { blog.Id, c.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });
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

        public async Task<ActionResult> EditSave(NonRegIncrPolicy val, int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    try
                    {
                        if (ModelState.IsValid)
                        {
                            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {
                                var Curr_LKValue = db.NonRegIncrPolicy.Find(data);
                                TempData["CurrRowVersion"] = Curr_LKValue.RowVersion;
                                db.Entry(Curr_LKValue).State = System.Data.Entity.EntityState.Detached;
                                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                {
                                    NonRegIncrPolicy blog = blog = null;
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.NonRegIncrPolicy.Where(e => e.Id == data).SingleOrDefault();
                                        originalBlogValues = context.Entry(blog).OriginalValues;
                                    }

                                    val.DBTrack = new DBTrack
                                    {
                                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };
                                    NonRegIncrPolicy lkval = new NonRegIncrPolicy
                                    {
                                        Id = data,
                                        MaxIncrInService = val.MaxIncrInService,
                                        MaxSerLockAppl = val.MaxSerLockAppl,
                                        MaxService = val.MaxService,
                                        MinSerAppl = val.MinSerAppl,
                                        MinService = val.MinService,

                                        DBTrack = val.DBTrack
                                    };


                                    db.NonRegIncrPolicy.Attach(lkval);
                                    db.Entry(lkval).State = System.Data.Entity.EntityState.Modified;

                                    // db.Entry(c.DBTrack).State = System.Data.Entity.EntityState.Modified;
                                    //db.SaveChanges();
                                    db.Entry(lkval).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                    // DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, val.DBTrack);
                                    await db.SaveChangesAsync();
                                    //DisplayTrackedEntities(db.ChangeTracker);
                                    db.Entry(lkval).State = System.Data.Entity.EntityState.Detached;
                                    ts.Complete();
                                    //return Json(new Object[] { lkval.Id, lkval.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });
                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = lkval.Id, Val = lkval.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }
                            }
                        }
                        return View();
                    }
                    catch (DbUpdateConcurrencyException e) { throw e; }
                    catch (DbUpdateException e) { throw e; }
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
                            //Corporate corp = db.Corporate.Find(auth_id);
                            //Corporate corp = db.Corporate.AsNoTracking().FirstOrDefault(e => e.Id == auth_id);

                            NonRegIncrPolicy corp = db.NonRegIncrPolicy.FirstOrDefault(e => e.Id == auth_id);

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

                            db.NonRegIncrPolicy.Attach(corp);
                            db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            //db.SaveChanges();
                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corp.DBTrack);
                            DT_NonRegIncrPolicy DT_Corp = (DT_NonRegIncrPolicy)rtn_Obj;

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

                        NonRegIncrPolicy Old_Corp = db.NonRegIncrPolicy.Where(e => e.Id == auth_id).SingleOrDefault();


                        DT_NonRegIncrPolicy Curr_Corp = db.DT_NonRegIncrPolicy
                                                    .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                                    .OrderByDescending(e => e.Id)
                                                    .FirstOrDefault();

                        if (Curr_Corp != null)
                        {
                            NonRegIncrPolicy corp = new NonRegIncrPolicy();

                            corp.MaxIncrInService = Curr_Corp.MaxIncrInService == null ? Old_Corp.MaxIncrInService : Curr_Corp.MaxIncrInService;
                            corp.MaxSerLockAppl = Curr_Corp.MaxSerLockAppl == null ? Old_Corp.MaxSerLockAppl : Curr_Corp.MaxSerLockAppl;
                            corp.MaxService = Curr_Corp.MaxService == null ? Old_Corp.MaxService : Curr_Corp.MaxService;
                            corp.MinSerAppl = Curr_Corp.MinSerAppl == null ? Old_Corp.MinSerAppl : Curr_Corp.MinSerAppl;
                            corp.MinService = Curr_Corp.MinService == null ? Old_Corp.MinService : Curr_Corp.MinService;

                            //      corp.Id = auth_id;

                            if (ModelState.IsValid)
                            {
                                try
                                {

                                    //DbContextTransaction transaction = db.Database.BeginTransaction();

                                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                    {
                                        // db.Configuration.AutoDetectChangesEnabled = false;
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

                                        //return Json(new Object[] { corp.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                                    }
                                }
                                catch (DbUpdateConcurrencyException ex)
                                {
                                    var entry = ex.Entries.Single();
                                    var clientValues = (NonRegIncrPolicy)entry.Entity;
                                    var databaseEntry = entry.GetDatabaseValues();
                                    if (databaseEntry == null)
                                    {
                                        Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                        //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                    }
                                    else
                                    {
                                        var databaseValues = (NonRegIncrPolicy)databaseEntry.ToObject();
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
                        //return Json(new Object[] { "", "Data removed from history", JsonRequestBehavior.AllowGet });
                    }
                    else if (auth_action == "D")
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //Corporate corp = db.Corporate.Find(auth_id);
                            NonRegIncrPolicy corp = db.NonRegIncrPolicy.AsNoTracking().FirstOrDefault(e => e.Id == auth_id);
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

                            db.NonRegIncrPolicy.Attach(corp);
                            db.Entry(corp).State = System.Data.Entity.EntityState.Deleted;


                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corp.DBTrack);
                            DT_NonRegIncrPolicy DT_Corp = (DT_NonRegIncrPolicy)rtn_Obj;
                            db.Create(DT_Corp);
                            await db.SaveChangesAsync();
                            db.Entry(corp).State = System.Data.Entity.EntityState.Detached;
                            ts.Complete();
                            Msg.Add(" Record Authorised ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            //return Json(new Object[] { "", "Record Authorised", JsonRequestBehavior.AllowGet });
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


        public int EditS(int data, NonRegIncrPolicy c, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var CurCorp = db.NonRegIncrPolicy.Find(data);
                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    c.DBTrack = dbT;
                    NonRegIncrPolicy corp = new NonRegIncrPolicy()
                    {
                        MinService = c.MinService,
                        MinSerAppl = c.MinSerAppl,
                        MaxService = c.MaxService,
                        MaxSerLockAppl = c.MaxSerLockAppl,
                        MaxIncrInService = c.MaxIncrInService,
                        Id = data,
                        DBTrack = c.DBTrack
                    };


                    db.NonRegIncrPolicy.Attach(corp);
                    db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                    return 1;
                }
                return 0;
            }
        }
        public void RollBack()
        {

            //  var context = DataContextFactory.GetDataContext();
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

        public ActionResult GetLookup_NonRegIncrPolicy(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.NonRegIncrPolicy.ToList();
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.NonRegIncrPolicy.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }
                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }
    }
}