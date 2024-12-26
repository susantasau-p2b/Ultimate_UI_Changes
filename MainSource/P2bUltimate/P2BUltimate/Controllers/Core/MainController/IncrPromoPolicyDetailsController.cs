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
    [AuthoriseManger]
    public class IncrPromoPolicyDetailsController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();

        public ActionResult Index()
        {
            return View("~/Views/Core/MainViews/IncrPromoPolicyDetails/Index.cshtml");
        }

        public ActionResult partial()
        {
            return View("~/Views/Shared/Core/_RegIncrPolicy.cshtml");
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
                IEnumerable<IncrPromoPolicyDetails> IncrPromoPolicyDetails = null;
                if (gp.IsAutho == true)
                {
                    IncrPromoPolicyDetails = db.IncrPromoPolicyDetails.Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    IncrPromoPolicyDetails = db.IncrPromoPolicyDetails.AsNoTracking().ToList();
                }

                IEnumerable<IncrPromoPolicyDetails> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = IncrPromoPolicyDetails;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Select(a => new { a.Id, a.IncrAmount, a.IncrPercent, a.IncrSteps }).Where((e => (e.Id.ToString() == gp.searchString) || (e.IncrAmount.ToString() == gp.searchString.ToString()) || (e.IncrPercent.ToString() == gp.searchString.ToString()) || (e.IncrSteps.ToString() == gp.searchString.ToString()))).ToList();
                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.IncrAmount, a.IncrPercent, a.IncrSteps }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = IncrPromoPolicyDetails;
                    Func<IncrPromoPolicyDetails, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "IncrAmount" ? c.IncrAmount.ToString() : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.IncrAmount), Convert.ToString(a.IncrPercent), a.IncrSteps }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.IncrAmount), Convert.ToString(a.IncrPercent), a.IncrSteps }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.IncrAmount, a.IncrPercent, a.IncrSteps }).ToList();
                    }
                    totalRecords = IncrPromoPolicyDetails.Count();
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
        public ActionResult Create(IncrPromoPolicyDetails c)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                if (c.IsIncrAmount == true && c.IsIncrPercent == true && c.IsIncrSteps == true)
                {
                    Msg.Add("please select Only one amount");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
                else if (c.IsIncrAmount == false && c.IsIncrPercent == false && c.IsIncrSteps == false)
                {
                    Msg.Add("please select Only one amount");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
                else if (c.IsIncrAmount == true && c.IsIncrPercent == true && c.IsIncrSteps == false)
                {
                    Msg.Add("please select Only one amount");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
                else if (c.IsIncrAmount == false && c.IsIncrPercent == true && c.IsIncrSteps == true)
                {
                    Msg.Add("please select Only one amount");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
                else if (c.IsIncrAmount == true && c.IsIncrPercent == false && c.IsIncrSteps == true)
                {
                    Msg.Add("please select Only one amount");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    try
                    {
                        if (ModelState.IsValid)
                        {
                            using (TransactionScope ts = new TransactionScope())
                            {
                                if (db.IncrPromoPolicyDetails.Any(o => o.IncrAmount == c.IncrAmount))
                                {
                                    Msg.Add("  IncrAmount Already Exists.  ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new Object[] { "", "", "IncrAmount Already Exists.", JsonRequestBehavior.AllowGet });
                                }

                                c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                                IncrPromoPolicyDetails IncrPromoPolicyDetails = new IncrPromoPolicyDetails()
                                {
                                    IncrPercent = c.IncrPercent,
                                    IncrAmount = c.IncrAmount,
                                    IncrSteps = c.IncrSteps,
                                    IsIncrAmount = c.IsIncrAmount,
                                    IsIncrPercent = c.IsIncrPercent,
                                    IsIncrSteps = c.IsIncrSteps,
                                    DBTrack = c.DBTrack
                                };
                                try
                                {
                                    db.IncrPromoPolicyDetails.Add(IncrPromoPolicyDetails);
                                    var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, c.DBTrack);
                                    //DT_RegIncrPolicy DT_Corp = (DT_RegIncrPolicy)rtn_Obj;

                                    //db.Create(DT_Corp);
                                    db.SaveChanges();
                                    //DBTrackFile.DBTrackSave("Core/P2b.Global", "C", corporate, null, "Corporate", null);


                                    ts.Complete();
                                    Msg.Add("  Data Saved successfully  ");
                                    return Json(new Utility.JsonReturnClass { Id = IncrPromoPolicyDetails.Id, Val = IncrPromoPolicyDetails.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return this.Json(new Object[] { IncrPromoPolicyDetails.Id, IncrPromoPolicyDetails.FullDetails, "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
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
        }

        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    IncrPromoPolicyDetails corporates = db.IncrPromoPolicyDetails.Where(e => e.Id == data).SingleOrDefault();

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
                            DT_RegIncrPolicy DT_Corp = (DT_RegIncrPolicy)rtn_Obj;
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
                             //   DT_RegIncrPolicy DT_Corp = (DT_RegIncrPolicy)rtn_Obj;

                               // db.Create(DT_Corp);

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


        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                var Q = db.IncrPromoPolicyDetails
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        IncrPercent = e.IncrPercent,
                        IncrAmount = e.IncrAmount,
                        IncrSteps = e.IncrSteps,
                        IsIncrAmount = e.IsIncrAmount,
                        IsIncrPercent = e.IsIncrPercent,
                        IsIncrSteps = e.IsIncrSteps,
                        DBTrack = e.DBTrack,
                        Action = e.DBTrack.Action
                    }).ToList();

                //var add_data = db.IncrPromoPolicyDetails
                //    .Where(e => e.Id == data)
                //    .ToList();


                //var w = db.dt_incrpromopolicydetails
                //	 .where(e => e.orig_id == data && e.dbtrack.ismodified == true && e.dbtrack.action == "m").select
                //	 (e => new
                //	 {
                //		 dt_id=e.orig_id,
                //		incrpercent = e.incrpercent,
                //			incramount = e.incramount,
                //			incrsteps = e.incrsteps,
                //			isincramount = e.isincramount,
                //			isincrpercent = e.isincrpercent,
                //			isincrsteps = e.isincrsteps,
                //			dbtrack = e.dbtrack
                //	 }).orderbydescending(e => e.dt_id).firstordefault();

                var Corp = db.IncrPromoPolicyDetails.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, Auth, JsonRequestBehavior.AllowGet });
            }
        }

        [HttpPost]
        public async Task<ActionResult> EditSave(IncrPromoPolicyDetails R, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                if (R.IsIncrAmount == true && R.IsIncrPercent == true && R.IsIncrSteps == true)
                {
                    Msg.Add("please select Only one amount");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
                else if (R.IsIncrAmount == false && R.IsIncrPercent == false && R.IsIncrSteps == false)
                {
                    Msg.Add("please select Only one amount");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
                else if (R.IsIncrAmount == true && R.IsIncrPercent == true && R.IsIncrSteps == false)
                {
                    Msg.Add("please select Only one amount");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
                else if (R.IsIncrAmount == false && R.IsIncrPercent == true && R.IsIncrSteps == true)
                {
                    Msg.Add("please select Only one amount");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
                else if (R.IsIncrAmount == true && R.IsIncrPercent == false && R.IsIncrSteps == true)
                {
                    Msg.Add("please select Only one amount");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
                else
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
                                        IncrPromoPolicyDetails blog = null; // to retrieve old data
                                        DbPropertyValues originalBlogValues = null;

                                        using (var context = new DataBaseContext())
                                        {
                                            blog = context.IncrPromoPolicyDetails.Where(e => e.Id == data).SingleOrDefault();


                                            originalBlogValues = context.Entry(blog).OriginalValues;
                                        }

                                        R.DBTrack = new DBTrack
                                        {
                                            CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                            CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                            Action = "M",
                                            ModifiedBy = SessionManager.UserName,
                                            ModifiedOn = DateTime.Now
                                        };
                                        var m1 = db.IncrPromoPolicyDetails.Where(e => e.Id == data).ToList();
                                        foreach (var s in m1)
                                        {
                                            // s.AppraisalPeriodCalendar = null;
                                            db.IncrPromoPolicyDetails.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                        var CurCorp = db.IncrPromoPolicyDetails.Find(data);
                                        TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                        db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                        {

                                            IncrPromoPolicyDetails IncrPromoPolicyDetails = new IncrPromoPolicyDetails()
                                            {
                                                Id = data,
                                                IncrPercent = R.IncrPercent,
                                                IncrAmount = R.IncrAmount,
                                                IncrSteps = R.IncrSteps,
                                                IsIncrAmount = R.IsIncrAmount,
                                                IsIncrPercent = R.IsIncrPercent,
                                                IsIncrSteps = R.IsIncrSteps,
                                                DBTrack = R.DBTrack,
                                            };


                                            db.IncrPromoPolicyDetails.Attach(IncrPromoPolicyDetails);
                                            db.Entry(IncrPromoPolicyDetails).State = System.Data.Entity.EntityState.Modified;
                                            db.Entry(IncrPromoPolicyDetails).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                            //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                        }

                                        //int a = EditS(Corp, Addrs, ContactDetails, data, c, c.DBTrack);



                                        using (var context = new DataBaseContext())
                                        {

                                            //var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, R.DBTrack);
                                            //DT_RegIncrPolicy DT_obj = (DT_RegIncrPolicy)obj;
                                            //db.Create(DT_obj);
                                            db.SaveChanges();
                                        }
                                        // db.SaveChanges();
                                        await db.SaveChangesAsync();
                                        ts.Complete();

                                        Msg.Add("  Record Updated");
                                        return Json(new Utility.JsonReturnClass { Id = R.Id, Val = R.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        //return Json(new Object[] { R.Id, R.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });

                                    }
                                }
                                catch (DbUpdateConcurrencyException ex)
                                {
                                    var entry = ex.Entries.Single();
                                    var clientValues = (IncrPromoPolicyDetails)entry.Entity;
                                    var databaseEntry = entry.GetDatabaseValues();
                                    if (databaseEntry == null)
                                    {
                                        Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                    }
                                    else
                                    {
                                        var databaseValues = (IncrPromoPolicyDetails)databaseEntry.ToObject();
                                        R.RowVersion = databaseValues.RowVersion;

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

                                IncrPromoPolicyDetails blog = null; // to retrieve old data
                                DbPropertyValues originalBlogValues = null;
                                IncrPromoPolicyDetails Old_Corp = null;

                                using (var context = new DataBaseContext())
                                {
                                    blog = context.IncrPromoPolicyDetails.Where(e => e.Id == data).SingleOrDefault();
                                    originalBlogValues = context.Entry(blog).OriginalValues;
                                }
                                R.DBTrack = new DBTrack
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
                                IncrPromoPolicyDetails IncrPromoPolicyDetails = new IncrPromoPolicyDetails()
                                {
                                    IncrPercent = R.IncrPercent,
                                    IncrAmount = R.IncrAmount,
                                    IncrSteps = R.IncrSteps,
                                    IsIncrAmount = R.IsIncrAmount,
                                    IsIncrPercent = R.IsIncrPercent,
                                    IsIncrSteps = R.IsIncrSteps,
                                    DBTrack = R.DBTrack,
                                    RowVersion = (Byte[])TempData["RowVersion"]
                                };



                                //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                                using (var context = new DataBaseContext())
                                {
                                    var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, IncrPromoPolicyDetails, "IncrPromoPolicyDetails", R.DBTrack);
                                    // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                    Old_Corp = context.IncrPromoPolicyDetails.Where(e => e.Id == data).SingleOrDefault();
                                    DT_RegIncrPolicy DT_Corp = (DT_RegIncrPolicy)obj;
                                    db.Create(DT_Corp);
                                    //db.SaveChanges();
                                }
                                blog.DBTrack = R.DBTrack;
                                db.IncrPromoPolicyDetails.Attach(blog);
                                db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                                db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                db.SaveChanges();
                                ts.Complete();
                                Msg.Add("  Record Updated");
                                return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = R.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Object[] { blog.Id, R.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });
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
        }

        [HttpPost]
        public async Task<ActionResult> AuthSave(int auth_id, Boolean isauth, String auth_action) //to save authorised data
        {
            //if (auth_action == "C")
            //{
            //	using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            //	{
            //		//Corporate corp = db.Corporate.Find(auth_id);
            //		//Corporate corp = db.Corporate.AsNoTracking().FirstOrDefault(e => e.Id == auth_id);

            //		IncrPromoPolicyDetails corp = db.IncrPromoPolicyDetails.FirstOrDefault(e => e.Id == auth_id);

            //		corp.DBTrack = new DBTrack
            //		{
            //			Action = "C",
            //			ModifiedBy = corp.DBTrack.ModifiedBy != null ? corp.DBTrack.ModifiedBy : null,
            //			CreatedBy = corp.DBTrack.CreatedBy != null ? corp.DBTrack.CreatedBy : null,
            //			CreatedOn = corp.DBTrack.CreatedOn != null ? corp.DBTrack.CreatedOn : null,
            //			IsModified = corp.DBTrack.IsModified == true ? false : false,
            //			AuthorizedBy = SessionManager.UserName,
            //			AuthorizedOn = DateTime.Now
            //		};

            //		db.IncrPromoPolicyDetails.Attach(corp);
            //		db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
            //		db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
            //		//db.SaveChanges();
            //		var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corp.DBTrack);
            //		DT_RegIncrPolicy DT_Corp = (DT_RegIncrPolicy)rtn_Obj;

            //		db.Create(DT_Corp);
            //		await db.SaveChangesAsync();

            //		ts.Complete();
            //		return Json(new Object[] { corp.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
            //	}
            //}
            //else if (auth_action == "M")
            //{

            //	IncrPromoPolicyDetails Old_Corp = db.IncrPromoPolicyDetails.Where(e => e.Id == auth_id).SingleOrDefault();


            //	DT_IncrPromoPolicyDetails Curr_Corp = db.DT_IncrPromoPolicyDetails
            //								.Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
            //								.OrderByDescending(e => e.Orig_Id)
            //								.FirstOrDefault();

            //	if (Curr_Corp != null)
            //	{
            //		IncrPromoPolicyDetails corp = new IncrPromoPolicyDetails();

            //		corp.IncrAmount = Curr_Corp.IncrAmount == null ? Old_Corp.IncrPercent : Curr_Corp.IncrAmount;
            //		corp.IncrPercent = Curr_Corp.IncrPercent == null ? Old_Corp.IncrAmount : Curr_Corp.IncrPercent;
            //		corp.IncrSteps = Curr_Corp.IncrSteps == null ? Old_Corp.IncrSteps :Curr_Corp.IncrSteps;
            //		corp.IsIncrAmount = Curr_Corp.IsIncrAmount == null ? Old_Corp.IsIncrAmount : Curr_Corp.IsIncrAmount;
            //		corp.IsIncrPercent = Curr_Corp.IsIncrPercent == null ? Old_Corp.IsIncrPercent : Curr_Corp.IsIncrPercent;
            //		corp.IncrSteps = Curr_Corp.IsIncrSteps == null ? Old_Corp.IncrSteps : Curr_Corp.IncrSteps;
            //	//	corp.IsNewScaleAction = Curr_Corp.IsNewScaleAction == null ? Old_Corp.IsNewScaleAction : Curr_Corp.IsNewScaleAction;

            //	//	corp.IsOldScaleAction = Curr_Corp.IsOldScaleAction == null ? Old_Corp.IsOldScaleAction : Curr_Corp.IsOldScaleAction;
            //	//	corp.IsLWPIncl = Curr_Corp.IsLWPIncl == null ? Old_Corp.IsLWPIncl : Curr_Corp.IsLWPIncl;
            //	///	corp.IsMidMonthEffect = Curr_Corp.IsMidMonthEffect == null ? Old_Corp.IsMidMonthEffect : Curr_Corp.IsMidMonthEffect;
            //		// corp.IsMidQuarterEffect = Curr_Corp.IsMidQuarterEffect == null ? Old_Corp.IsMidQuarterEffect : Curr_Corp.IsMidQuarterEffect;
            //		//corp.MidMonthLockDay = Curr_Corp.MidMonthLockDay == null ? Old_Corp.IsJoiningDate : Curr_Corp.IsJoiningDate;
            //		//  corp.NextMonStartDay = Curr_Corp.NextMonStartDay == null ? Old_Corp.NextMonStartDay : Curr_Corp.NextMonStartDay;


            //		//      corp.Id = auth_id;

            //		if (ModelState.IsValid)
            //		{
            //			try
            //			{

            //				//DbContextTransaction transaction = db.Database.BeginTransaction();

            //				using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            //				{
            //					// db.Configuration.AutoDetectChangesEnabled = false;
            //					corp.DBTrack = new DBTrack
            //					{
            //						CreatedBy = Old_Corp.DBTrack.CreatedBy == null ? null : Old_Corp.DBTrack.CreatedBy,
            //						CreatedOn = Old_Corp.DBTrack.CreatedOn == null ? null : Old_Corp.DBTrack.CreatedOn,
            //						Action = "M",
            //						ModifiedBy = Old_Corp.DBTrack.ModifiedBy == null ? null : Old_Corp.DBTrack.ModifiedBy,
            //						ModifiedOn = Old_Corp.DBTrack.ModifiedOn == null ? null : Old_Corp.DBTrack.ModifiedOn,
            //						AuthorizedBy = SessionManager.UserName,
            //						AuthorizedOn = DateTime.Now,
            //						IsModified = false
            //					};

            //					int a = EditS(auth_id, corp, corp.DBTrack);

            //					await db.SaveChangesAsync();

            //					ts.Complete();
            //					return Json(new Object[] { corp.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
            //				}
            //			}
            //			catch (DbUpdateConcurrencyException ex)
            //			{
            //				var entry = ex.Entries.Single();
            //				var clientValues = (IncrPromoPolicyDetails)entry.Entity;
            //				var databaseEntry = entry.GetDatabaseValues();
            //				if (databaseEntry == null)
            //				{
            //					return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
            //				}
            //				else
            //				{
            //					var databaseValues = (IncrPromoPolicyDetails)databaseEntry.ToObject();
            //					corp.RowVersion = databaseValues.RowVersion;
            //				}
            //			}

            //			return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
            //		}
            //	}
            //	else
            //		return Json(new Object[] { "", "Data removed from history", JsonRequestBehavior.AllowGet });
            //}
            //else if (auth_action == "D")
            //{
            //	using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            //	{
            //		//Corporate corp = db.Corporate.Find(auth_id);
            //		IncrPromoPolicyDetails corp = db.IncrPromoPolicyDetails.AsNoTracking().FirstOrDefault(e => e.Id == auth_id);
            //		corp.DBTrack = new DBTrack
            //		{
            //			Action = "D",
            //			ModifiedBy = corp.DBTrack.ModifiedBy != null ? corp.DBTrack.ModifiedBy : null,
            //			CreatedBy = corp.DBTrack.CreatedBy != null ? corp.DBTrack.CreatedBy : null,
            //			CreatedOn = corp.DBTrack.CreatedOn != null ? corp.DBTrack.CreatedOn : null,
            //			IsModified = false,
            //			AuthorizedBy = SessionManager.UserName,
            //			AuthorizedOn = DateTime.Now
            //		};

            //		db.IncrPromoPolicyDetails.Attach(corp);
            //		db.Entry(corp).State = System.Data.Entity.EntityState.Deleted;


            //		var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corp.DBTrack);
            //		DT_RegIncrPolicy DT_Corp = (DT_RegIncrPolicy)rtn_Obj;
            //		db.Create(DT_Corp);
            //		await db.SaveChangesAsync();
            //		db.Entry(corp).State = System.Data.Entity.EntityState.Detached;
            //		ts.Complete();
            //		return Json(new Object[] { "", "Record Authorised", JsonRequestBehavior.AllowGet });
            return View();
        }


        public int EditS(int data, IncrPromoPolicyDetails c, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var CurCorp = db.IncrPromoPolicyDetails.Find(data);
                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    c.DBTrack = dbT;
                    IncrPromoPolicyDetails corp = new IncrPromoPolicyDetails()
                    {
                        IncrPercent = c.IncrPercent,
                        IncrAmount = c.IncrAmount,
                        IncrSteps = c.IncrSteps,
                        IsIncrAmount = c.IsIncrAmount,
                        IsIncrPercent = c.IsIncrPercent,
                        IsIncrSteps = c.IsIncrSteps,
                        DBTrack = c.DBTrack,
                        Id = data,
                    };


                    db.IncrPromoPolicyDetails.Attach(corp);
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
            using (DataBaseContext db = new DataBaseContext())
            {
                //  var context = DataContextFactory.GetDataContext();
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
        public ActionResult GetLookup_IncrPromoPolicyDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.IncrPromoPolicyDetails.ToList();
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.IncrPromoPolicyDetails.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
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