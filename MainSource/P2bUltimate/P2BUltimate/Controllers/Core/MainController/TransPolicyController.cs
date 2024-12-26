///
/// Created by Rekha
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
    public class TransPolicyController : Controller
    {

        //private DataBaseContext db = new DataBaseContext();

        //
        // GET: /PromoPolicy/
        public ActionResult Index()
        {
            return View("~/Views/Core/MainViews/TransPolicy/Index.cshtml");
        }


        public ActionResult CreateTransPolicy_partial()
        {
            return View("~/Views/Shared/Core/_TransPolicy.cshtml");
        }

        [HttpPost]       
        public ActionResult Create(TransPolicy c, FormCollection form) //Create submit
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
                            if (db.TransPolicy.Any(o => o.Name == c.Name))
                            {
                                // return Json(new Object[] { "", "", "Name Already Exists.", JsonRequestBehavior.AllowGet });
                                Msg.Add("  Code Already Exists.  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }

                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            TransPolicy TransPolicy = new TransPolicy()
                            {
                                IsDepartmentChange = c.IsDepartmentChange,
                                IsDivsionChange = c.IsDivsionChange,
                                IsFuncStructChange = c.IsFuncStructChange,
                                IsGroupChange = c.IsGroupChange,
                                IsLocationChange = c.IsLocationChange,
                                IsUnitChange = c.IsUnitChange,
                                Name = c.Name,
                                DBTrack = c.DBTrack
                            };
                            try
                            {
                                db.TransPolicy.Add(TransPolicy);
                                var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, c.DBTrack);
                                DT_TransPolicy DT_Corp = (DT_TransPolicy)rtn_Obj;
                                db.Create(DT_Corp);
                                db.SaveChanges();
                                ts.Complete();
                                // return this.Json(new Object[] { TransPolicy.Id, TransPolicy.FullDetails, "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = TransPolicy.Id, Val = TransPolicy.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = c.Id });
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
                var Q = db.TransPolicy
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        IsDepartmentChange = e.IsDepartmentChange,
                        IsDivsionChange = e.IsDivsionChange,
                        IsFuncStructChange = e.IsFuncStructChange,
                        IsGroupChange = e.IsGroupChange,
                        IsLocationChange = e.IsLocationChange,
                        IsUnitChange = e.IsUnitChange,
                        Name = e.Name,
                        Action = e.DBTrack.Action
                    }).ToList();

                //var add_data = db.PromoPolicy.Include(e => e.IncrActivity)
                //               .Where(e => e.Id == data).Select
                //               (e => new 
                //               {
                //                    IncrActivity_FullDetails = e.IncrActivity.FullDetails == null ? " " : e.IncrActivity.FullDetails, 
                //                    IncrActivity_Id = e.IncrActivity.Id == null ? "" : e.IncrActivity.Id.ToString(),               

                //               }).ToList();                                                                                                                                   

                var W = db.DT_TransPolicy
                     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id,
                         IsDepartmentChange = e.IsDepartmentChange,
                         IsDivsionChange = e.IsDivsionChange,
                         IsFuncStructChange = e.IsFuncStructChange,
                         IsGroupChange = e.IsGroupChange,
                         IsLocationChange = e.IsLocationChange,
                         //IsPayJobStatusChange = e.IsPayJobStatusChange,
                         IsUnitChange = e.IsUnitChange,
                         Name = e.Name,
                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var Corp = db.TransPolicy.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, "", W, Auth, JsonRequestBehavior.AllowGet });
            }
        }

        [HttpPost]
        public async Task<ActionResult> EditSave(TransPolicy c, int data, FormCollection form) // Edit submit
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
                                    TransPolicy blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.TransPolicy.Where(e => e.Id == data).SingleOrDefault();
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
                                    var m1 = db.TransPolicy.Where(e => e.Id == data).ToList();
                                    foreach (var s in m1)
                                    {
                                        // s.AppraisalPeriodCalendar = null;
                                        db.TransPolicy.Attach(s);
                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                        //await db.SaveChangesAsync();
                                        db.SaveChanges();
                                        TempData["RowVersion"] = s.RowVersion;
                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    }
                                    var CurCorp = db.TransPolicy.Find(data);
                                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {
                                        //c.DBTrack = dbT;
                                        TransPolicy TransPolicy = new TransPolicy()
                                        {
                                            IsDepartmentChange = c.IsDepartmentChange,
                                            IsDivsionChange = c.IsDivsionChange,
                                            IsFuncStructChange = c.IsFuncStructChange,
                                            IsGroupChange = c.IsGroupChange,
                                            IsLocationChange = c.IsLocationChange,
                                            IsUnitChange = c.IsUnitChange,
                                            Name = c.Name,
                                            Id = data,
                                            DBTrack = c.DBTrack
                                        };

                                        db.TransPolicy.Attach(TransPolicy);
                                        db.Entry(TransPolicy).State = System.Data.Entity.EntityState.Modified;
                                        db.Entry(TransPolicy).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                        //return 1;
                                    }
                                    //int a = EditS(data, c, c.DBTrack);
                                    using (var context = new DataBaseContext())
                                    {
                                        var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                        DT_TransPolicy DT_TransPolicy = (DT_TransPolicy)obj;
                                        db.Create(DT_TransPolicy);
                                        db.SaveChanges();
                                    }
                                    await db.SaveChangesAsync();
                                    ts.Complete();
                                    // return Json(new Object[] { data, c.Name, "Record Updated", JsonRequestBehavior.AllowGet });
                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = data, Val = c.Name, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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

                                    //  return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (PromoPolicy)databaseEntry.ToObject();
                                    c.RowVersion = databaseValues.RowVersion;

                                }
                            }

                            //   return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                            Msg.Add("Record modified by another user.So refresh it and try to save again.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }
                    }
                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            TransPolicy blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            TransPolicy Old_TransPolicy = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.TransPolicy.Where(e => e.Id == data).SingleOrDefault();
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
                            TransPolicy TransPolicy = new TransPolicy()
                            {
                                IsDepartmentChange = c.IsDepartmentChange,
                                IsDivsionChange = c.IsDivsionChange,
                                IsFuncStructChange = c.IsFuncStructChange,
                                IsGroupChange = c.IsGroupChange,
                                IsLocationChange = c.IsLocationChange,
                                IsUnitChange = c.IsUnitChange,
                                Name = c.Name,
                                Id = data,
                                DBTrack = c.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };



                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, TransPolicy, "TransPolicy", c.DBTrack);
                                DT_TransPolicy DT_TransPolicy = (DT_TransPolicy)obj;
                                db.Create(DT_TransPolicy);
                            }
                            blog.DBTrack = c.DBTrack;
                            db.TransPolicy.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            // return Json(new Object[] { blog.Id, c.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = c.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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
                            TransPolicy TransPolicy = db.TransPolicy.FirstOrDefault(e => e.Id == auth_id);
                            TransPolicy.DBTrack = new DBTrack
                            {
                                Action = "C",
                                ModifiedBy = TransPolicy.DBTrack.ModifiedBy != null ? TransPolicy.DBTrack.ModifiedBy : null,
                                CreatedBy = TransPolicy.DBTrack.CreatedBy != null ? TransPolicy.DBTrack.CreatedBy : null,
                                CreatedOn = TransPolicy.DBTrack.CreatedOn != null ? TransPolicy.DBTrack.CreatedOn : null,
                                IsModified = TransPolicy.DBTrack.IsModified == true ? false : false,
                                AuthorizedBy = SessionManager.UserName,
                                AuthorizedOn = DateTime.Now
                            };

                            db.TransPolicy.Attach(TransPolicy);
                            db.Entry(TransPolicy).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(TransPolicy).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, TransPolicy.DBTrack);
                            DT_PromoPolicy DT_Corp = (DT_PromoPolicy)rtn_Obj;
                            db.Create(DT_Corp);
                            await db.SaveChangesAsync();

                            ts.Complete();
                            // return Json(new Object[] { TransPolicy.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                            Msg.Add("  Record Authorised");
                            return Json(new Utility.JsonReturnClass { Id = TransPolicy.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }
                    }
                    else if (auth_action == "M")
                    {

                        TransPolicy Old_TransPolicy = db.TransPolicy.Where(e => e.Id == auth_id).SingleOrDefault();


                        DT_TransPolicy Curr_TransPolicy = db.DT_TransPolicy
                                                    .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                                    .OrderByDescending(e => e.Id)
                                                    .FirstOrDefault();

                        if (Curr_TransPolicy != null)
                        {
                            TransPolicy transPolicy = new TransPolicy();
                            transPolicy.Name = Curr_TransPolicy.Name == null ? Old_TransPolicy.Name : Old_TransPolicy.Name;
                            transPolicy.IsFuncStructChange = Curr_TransPolicy.IsFuncStructChange;
                            transPolicy.IsDepartmentChange = Curr_TransPolicy.IsDepartmentChange;
                            transPolicy.IsDivsionChange = Curr_TransPolicy.IsDivsionChange;
                            transPolicy.IsGroupChange = Curr_TransPolicy.IsGroupChange;
                            transPolicy.IsLocationChange = Curr_TransPolicy.IsLocationChange;
                            transPolicy.IsUnitChange = Curr_TransPolicy.IsUnitChange;

                            if (ModelState.IsValid)
                            {
                                try
                                {
                                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                    {
                                        transPolicy.DBTrack = new DBTrack
                                        {
                                            CreatedBy = Old_TransPolicy.DBTrack.CreatedBy == null ? null : Old_TransPolicy.DBTrack.CreatedBy,
                                            CreatedOn = Old_TransPolicy.DBTrack.CreatedOn == null ? null : Old_TransPolicy.DBTrack.CreatedOn,
                                            Action = "M",
                                            ModifiedBy = Old_TransPolicy.DBTrack.ModifiedBy == null ? null : Old_TransPolicy.DBTrack.ModifiedBy,
                                            ModifiedOn = Old_TransPolicy.DBTrack.ModifiedOn == null ? null : Old_TransPolicy.DBTrack.ModifiedOn,
                                            AuthorizedBy = SessionManager.UserName,
                                            AuthorizedOn = DateTime.Now,
                                            IsModified = false
                                        };

                                        int a = EditS(auth_id, transPolicy, transPolicy.DBTrack);
                                        await db.SaveChangesAsync();
                                        ts.Complete();
                                        // return Json(new Object[] { transPolicy.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                                        Msg.Add("  Record Authorised");
                                        return Json(new Utility.JsonReturnClass { Id = transPolicy.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    }
                                }
                                catch (DbUpdateConcurrencyException ex)
                                {
                                    var entry = ex.Entries.Single();
                                    var clientValues = (PromoPolicy)entry.Entity;
                                    var databaseEntry = entry.GetDatabaseValues();
                                    if (databaseEntry == null)
                                    {
                                        //   return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                        Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    }
                                    else
                                    {
                                        var databaseValues = (PromoPolicy)databaseEntry.ToObject();
                                        transPolicy.RowVersion = databaseValues.RowVersion;
                                    }
                                }

                                // return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                                Msg.Add("Record modified by another user.So refresh it and try to save again.");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }
                        }
                        else

                            Msg.Add("  Data removed successfully.  ");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        //  return Json(new Object[] { "", "Data removed from history", JsonRequestBehavior.AllowGet });
                    }
                    else if (auth_action == "D")
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            TransPolicy corp = db.TransPolicy.AsNoTracking().FirstOrDefault(e => e.Id == auth_id);

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

                            db.TransPolicy.Attach(corp);
                            db.Entry(corp).State = System.Data.Entity.EntityState.Deleted;


                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corp.DBTrack);
                            DT_TransPolicy DT_Corp = (DT_TransPolicy)rtn_Obj;
                            db.Create(DT_Corp);
                            await db.SaveChangesAsync();
                            db.Entry(corp).State = System.Data.Entity.EntityState.Detached;
                            ts.Complete();
                            //return Json(new Object[] { "", "Record Authorised", JsonRequestBehavior.AllowGet });
                            Msg.Add(" Record Authorised ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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
                IEnumerable<TransPolicy> TransPolicy = null;
                if (gp.IsAutho == true)
                {
                    TransPolicy = db.TransPolicy.Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    TransPolicy = db.TransPolicy.AsNoTracking().ToList();
                }

                IEnumerable<TransPolicy> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = TransPolicy;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Select(a => new { a.Id, a.Name }).Where((e => (e.Id.ToString() == gp.searchString) ||(e.Name.ToLower() == gp.searchString.ToLower()))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Name}).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = TransPolicy;
                    Func<TransPolicy, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => 
                                         gp.sidx == "Name" ? c.Name : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.Name }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.Name}).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Name}).ToList();
                    }
                    totalRecords = TransPolicy.Count();
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

                    TransPolicy TransPolicy = db.TransPolicy.Where(e => e.Id == data).SingleOrDefault();
                    if (TransPolicy.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = TransPolicy.DBTrack.CreatedBy != null ? TransPolicy.DBTrack.CreatedBy : null,
                                CreatedOn = TransPolicy.DBTrack.CreatedOn != null ? TransPolicy.DBTrack.CreatedOn : null,
                                IsModified = TransPolicy.DBTrack.IsModified == true ? true : false
                            };
                            TransPolicy.DBTrack = dbT;
                            db.Entry(TransPolicy).State = System.Data.Entity.EntityState.Modified;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, TransPolicy.DBTrack);
                            DT_TransPolicy DT_Corp = (DT_TransPolicy)rtn_Obj;
                            db.Create(DT_Corp);
                            await db.SaveChangesAsync();
                            ts.Complete();
                            //  return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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
                                    CreatedBy = TransPolicy.DBTrack.CreatedBy != null ? TransPolicy.DBTrack.CreatedBy : null,
                                    CreatedOn = TransPolicy.DBTrack.CreatedOn != null ? TransPolicy.DBTrack.CreatedOn : null,
                                    IsModified = TransPolicy.DBTrack.IsModified == true ? false : false//,
                                };

                                db.Entry(TransPolicy).State = System.Data.Entity.EntityState.Deleted;
                                var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dbT);
                                DT_TransPolicy DT_Corp = (DT_TransPolicy)rtn_Obj;
                                db.Create(DT_Corp);
                                await db.SaveChangesAsync();
                                ts.Complete();
                                return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });

                            }
                            catch (RetryLimitExceededException /* dex */)
                            {
                                //return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
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
        public int EditS(int data, TransPolicy c, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var CurCorp = db.TransPolicy.Find(data);
                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    c.DBTrack = dbT;
                    TransPolicy TransPolicy = new TransPolicy()
                    {
                        IsDepartmentChange = c.IsDepartmentChange,
                        IsDivsionChange = c.IsDivsionChange,
                        IsFuncStructChange = c.IsFuncStructChange,
                        IsGroupChange = c.IsGroupChange,
                        IsLocationChange = c.IsLocationChange,
                        IsUnitChange = c.IsUnitChange,
                        Name = c.Name,
                        Id = data,
                        DBTrack = c.DBTrack
                    };

                    db.TransPolicy.Attach(TransPolicy);
                    db.Entry(TransPolicy).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(TransPolicy).OriginalValues["RowVersion"] = TempData["RowVersion"];
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

        public ActionResult GetLookup_TransPolicy(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.TransPolicy.ToList();
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.TransPolicy.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
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