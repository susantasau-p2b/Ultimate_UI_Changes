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
using System.Reflection;
using P2BUltimate.Security;
using Appraisal;

namespace P2BUltimate.Controllers.Appraisal.MainController
{
    public class AppCategoryRatingController : Controller
    {
      // private DataBaseContext db = new DataBaseContext();
        //
        // GET: /AppSubCategoryRating/
        public ActionResult Index()
        {
            return View("~/Views/Appraisal/MainViews/AppCategoryRating/Index.cshtml");
        }

        public ActionResult GetAppCatDetailLKDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.AppCategory.ToList();
                IEnumerable<AppCategory> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.AppCategory.ToList().Where(d => d.Code.Contains(data));
                }
                else
                {
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.FullDetails }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetAppRatingObjectiveLKDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.AppRatingObjective.Include(e => e.ObjectiveWordings).ToList();
                IEnumerable<AppRatingObjective> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.AppRatingObjective.ToList().Where(d => d.Id.ToString().Contains(data));
                }
                else
                {
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.FullDetails }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult Create(AppCategoryRating p, FormCollection form) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string AppCat = form["AppCategorylist"] == "0" ? "" : form["AppCategorylist"];
                var AppRatingObj = form["AppRatingObjectivelist"] == "0" ? "" : form["AppRatingObjectivelist"];

                List<String> Msg = new List<String>();
                try
                {
                    if (AppCat != null && AppCat != "")
                    {
                        int AppCatId = Convert.ToInt32(AppCat);
                        var val = db.AppCategory.Where(e => e.Id == AppCatId).SingleOrDefault();
                        p.AppCategory = val;
                    }
                    else
                    {
                        Msg.Add("Kindly Enter Appraisal Category.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    p.AppRatingObjective = null;
                    List<AppRatingObjective> cp = new List<AppRatingObjective>();

                    if (AppRatingObj != null && AppRatingObj != "")
                    {
                        var ids = Utility.StringIdsToListIds(AppRatingObj);
                        foreach (var ca in ids)
                        {
                            var p_val = db.AppRatingObjective.Find(ca);
                            cp.Add(p_val);
                            p.AppRatingObjective = cp;
                        }
                    }


                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            if (db.AppCategoryRating.Any(o => o.AppCategory.Id == p.AppCategory.Id))
                            {
                                Msg.Add("Record Already Exists.");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }


                            p.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            AppCategoryRating AppCategoryRat = new AppCategoryRating()
                            {
                                AppRatingObjective = p.AppRatingObjective,
                                AppCategory = p.AppCategory,
                                MaxRatingPoints = p.MaxRatingPoints,
                                DBTrack = p.DBTrack
                            };

                            db.AppCategoryRating.Add(AppCategoryRat);
                            var rtn_Obj = DBTrackFile.DBTrackSave("Appraisal/Appraisal", null, db.ChangeTracker, p.DBTrack);
                            DT_AppCategoryRating DT_Corp = (DT_AppCategoryRating)rtn_Obj;
                            DT_Corp.AppCategory_Id = p.AppCategory == null ? 0 : p.AppCategory.Id;
                            db.Create(DT_Corp);
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("Data Saved Successfully.");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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

        public class IsRatingObjectiveDetailsC
        {
            public Array AppRatingObjective_Id { get; set; }
            public Array AppRatingObjective_FullDetails { get; set; }
            public string AppCategory_Id { get; set; }
            public string AppCategory_FullDetails { get; set; }
        }

        [HttpPost]
        public ActionResult Edit(int data)
        {
            List<IsRatingObjectiveDetailsC> return_data = new List<IsRatingObjectiveDetailsC>();
            using (DataBaseContext db = new DataBaseContext())
            {
                var r = db.AppCategoryRating
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        MaxPoints = e.MaxRatingPoints,
                        Action = e.DBTrack.Action
                    }).ToList();

                var a = db.AppCategoryRating.Include(e => e.AppCategory)
                    .Include(e => e.AppRatingObjective.Select(b => b.ObjectiveWordings))
                 .Where(e => e.Id == data).ToList();
                foreach (var ca in a)
                {
                    return_data.Add(new IsRatingObjectiveDetailsC
                    {
                        AppRatingObjective_Id = ca.AppRatingObjective.Select(e => e.Id.ToString()).ToArray(),
                        AppRatingObjective_FullDetails = ca.AppRatingObjective.Select(e => e.FullDetails).ToArray(),
                        AppCategory_Id = ca.AppCategory.Id.ToString(),
                        AppCategory_FullDetails = ca.AppCategory.FullDetails,
                    });
                }

                var LKup = db.AppCategoryRating.Find(data);
                TempData["RowVersion"] = LKup.RowVersion;
                var Auth = LKup.DBTrack.IsModified;

                return this.Json(new Object[] { r, return_data, "", Auth, JsonRequestBehavior.AllowGet });
            }
        }


        [HttpPost]
        public async Task<ActionResult> EditSave(AppCategoryRating add, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string AppCategory = form["AppCategorylist"] == "0" ? "" : form["AppCategorylist"];
                    string AppRatingObjective = form["AppRatingObjectivelist"] == "0" ? "" : form["AppRatingObjectivelist"];

                    bool Auth = form["Autho_Allow"] == "true" ? true : false;
                    try
                    {

                        var db_data = db.AppCategoryRating.Include(e => e.AppRatingObjective)
                            .Include(e => e.AppRatingObjective.Select(s => s.ObjectiveWordings))
                            .Where(e => e.Id == data).SingleOrDefault();

                        if (AppCategory != null && AppCategory != "" && AppCategory != "0")
                        {
                            var val = db.AppCategory.Find(int.Parse(AppCategory));
                            add.AppCategory = val;
                        }

                        List<AppRatingObjective> ObjITsection = new List<AppRatingObjective>();
                        AppRatingObjective pd = null;
                        pd = db.AppRatingObjective.Include(e => e.ObjectiveWordings).Where(e => e.Id == data).SingleOrDefault();
                        if (AppRatingObjective != null && AppRatingObjective != "")
                        {
                            var ids = Utility.StringIdsToListIds(AppRatingObjective);
                            foreach (var ca in ids)
                            {
                                //var value = db.AppRatingObjective.Find(ca);
                                var value = db.AppRatingObjective.Include(e => e.ObjectiveWordings).Where(e => e.Id == ca).SingleOrDefault();
                                ObjITsection.Add(value);
                                db_data.AppRatingObjective = ObjITsection;
                                db_data.Id = db_data.Id;

                            }
                        }
                        else
                        {
                            db_data.AppRatingObjective = null;
                        }


                        db.AppCategoryRating.Attach(db_data);
                        db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        TempData["RowVersion"] = db_data.RowVersion;
                        db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;


                        if (Auth == false)
                        {
                            if (ModelState.IsValid)
                            {
                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    AppCategoryRating blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.AppCategoryRating.Where(e => e.Id == data)
                                    .Include(e => e.AppCategory).Include(e => e.AppRatingObjective).SingleOrDefault();
                                        originalBlogValues = context.Entry(blog).OriginalValues;
                                    }

                                    add.DBTrack = new DBTrack
                                    {
                                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };

                                    var m1 = db.AppCategoryRating.Where(e => e.Id == data).ToList();
                                    foreach (var s in m1)
                                    {
                                        // s.AppraisalPeriodCalendar = null;
                                        db.AppCategoryRating.Attach(s);
                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                        //await db.SaveChangesAsync();
                                        db.SaveChanges();
                                        TempData["RowVersion"] = s.RowVersion;
                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    }

                                    //int a = EditS(AppCategory, AppRatingObjective, data, add, add.DBTrack);
                                    var CurCorp = db.AppCategoryRating.Find(data);
                                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                    
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {

                                        AppCategoryRating corp = new AppCategoryRating()
                                        {
                                            MaxRatingPoints = add.MaxRatingPoints == null ? 0 : add.MaxRatingPoints,
                                            AppCategory = add.AppCategory,
                                            Id = data,
                                            DBTrack = add.DBTrack
                                        };

                                        db.AppCategoryRating.Attach(corp);
                                        db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                                        db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                        // return 1;
                                    }


                                    using (var context = new DataBaseContext())
                                    {
                                        var obj = DBTrackFile.DBTrackSave("Appraisal/Appraisal", originalBlogValues, db.ChangeTracker, add.DBTrack);
                                        DT_AppCategoryRating DT_Addrs = (DT_AppCategoryRating)obj;
                                        DT_Addrs.AppCategory_Id = blog.AppCategory == null ? 0 : blog.AppCategory.Id;
                                        db.Create(DT_Addrs);

                                        db.SaveChanges();
                                    }
                                    await db.SaveChangesAsync();
                                    ts.Complete();
                                    Msg.Add("Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = blog.Id.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    //    return Json(new { blog.Id, query.FullAddress, , JsonRequestBehavior.AllowGet });
                                }


                                //  return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                            }
                        }
                        else
                        {
                            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {

                                AppSubCategoryRating blog = null; // to retrieve old data
                                DbPropertyValues originalBlogValues = null;
                                AppCategoryRating Old_Addrs = null;

                                using (var context = new DataBaseContext())
                                {
                                    blog = context.AppSubCategoryRating.Where(e => e.Id == data).SingleOrDefault();
                                    originalBlogValues = context.Entry(blog).OriginalValues;
                                }
                                add.DBTrack = new DBTrack
                                {
                                    CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                    CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                    Action = "M",
                                    IsModified = blog.DBTrack.IsModified == true ? true : false,
                                    ModifiedBy = SessionManager.UserName,
                                    ModifiedOn = DateTime.Now
                                };
                                AppCategoryRating L = new AppCategoryRating()
                                {
                                    MaxRatingPoints = add.MaxRatingPoints == null ? 0 : add.MaxRatingPoints,
                                    AppCategory = add.AppCategory,
                                    AppRatingObjective = add.AppRatingObjective,
                                    DBTrack = add.DBTrack,
                                    Id = data
                                };


                                //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                                using (var context = new DataBaseContext())
                                {
                                    var obj = DBTrackFile.ModifiedDataHistory("Appraisal/Appraisal", "M", blog, add, "AppCategoryRating", add.DBTrack);
                                    // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                    Old_Addrs = context.AppCategoryRating.Where(e => e.Id == data)
                                    .Include(e => e.AppCategory).Include(e => e.AppRatingObjective).SingleOrDefault();
                                    DT_AppCategoryRating DT_Addrs = (DT_AppCategoryRating)obj;

                                    DT_Addrs.AppCategory_Id = DBTrackFile.ValCompare(Old_Addrs.AppCategory, add.AppCategory);
                                    db.Create(DT_Addrs);
                                    //db.SaveChanges();
                                }
                                blog.DBTrack = add.DBTrack;
                                db.AppSubCategoryRating.Attach(blog);
                                db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                                db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                db.SaveChanges();
                                ts.Complete();
                                //  return Json(new Object[] { blog.Id, add.FullAddress, "Record Updated", JsonRequestBehavior.AllowGet });
                                Msg.Add("Record Updated");
                                return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = blog.Id.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }

                        }
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        var entry = ex.Entries.Single();
                        var clientValues = (AppManualAssignment)entry.Entity;
                        var databaseEntry = entry.GetDatabaseValues();
                        if (databaseEntry == null)
                        {
                            Msg.Add(ex.Message);
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            var databaseValues = (AppManualAssignment)databaseEntry.ToObject();
                            add.RowVersion = databaseValues.RowVersion;

                        }
                    }

                    catch (Exception e)
                    {
                        Msg.Add(e.Message);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
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
        public int EditS(string AppCategory, string AppRatingObjective, int data, AppCategoryRating c, DBTrack dbT)
        {
            IList<AppCategoryRating> typedetails = null;
            using (DataBaseContext db = new DataBaseContext())
            {

                if (AppCategory != null && AppCategory != "")
                {
                    var val = db.AppCategory.Find(int.Parse(AppCategory));
                    c.AppCategory = val;

                    var type = db.AppCategoryRating.Include(e => e.AppCategory).Where(e => e.Id == data).SingleOrDefault();
                    if (type.AppCategory != null)
                    {
                        typedetails = db.AppCategoryRating.Where(x => x.AppCategory.Id == type.AppCategory.Id && x.Id == data).ToList();
                    }
                    else
                    {
                        typedetails = db.AppCategoryRating.Where(x => x.Id == data).ToList();
                    }

                    foreach (var s in typedetails)
                    {
                        s.AppCategory = c.AppCategory;
                        db.AppCategoryRating.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }
                else
                {
                    var BusiTypeDetails = db.AppCategoryRating.Include(e => e.AppCategory).Where(x => x.Id == data).ToList();
                    foreach (var s in BusiTypeDetails)
                    {
                        s.AppCategory = null;
                        db.AppCategoryRating.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }


                var CurCorp = db.AppCategoryRating.Find(data);
                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    c.DBTrack = dbT;
                    AppCategoryRating corp = new AppCategoryRating()
                    {
                        MaxRatingPoints = c.MaxRatingPoints == null ? 0 : c.MaxRatingPoints,
                        AppCategory = c.AppCategory,
                        Id = data,
                        DBTrack = c.DBTrack
                    };


                    db.AppCategoryRating.Attach(corp);
                    db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                    return 1;
                }
                return 0;
            }
        }


        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int? data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    AppCategoryRating AppCategoryRat = db.AppCategoryRating.Include(e => e.AppRatingObjective)
                        .Include(e => e.AppCategory).Where(e => e.Id == data).SingleOrDefault();




                    if (AppCategoryRat.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack, corporates, null, "Corporate");
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = AppCategoryRat.DBTrack.CreatedBy != null ? AppCategoryRat.DBTrack.CreatedBy : null,
                                CreatedOn = AppCategoryRat.DBTrack.CreatedOn != null ? AppCategoryRat.DBTrack.CreatedOn : null,
                                IsModified = AppCategoryRat.DBTrack.IsModified == true ? true : false
                            };
                            AppCategoryRat.DBTrack = dbT;
                            db.Entry(AppCategoryRat).State = System.Data.Entity.EntityState.Modified;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Appraisal/Appraisal", null, db.ChangeTracker, AppCategoryRat.DBTrack);
                            DT_AppSubCategoryRating DT_OBJ = (DT_AppSubCategoryRating)rtn_Obj;
                            db.Create(DT_OBJ);

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
                            if (AppCategoryRat.AppRatingObjective != null)
                            {
                                var selectedValues = AppCategoryRat.AppRatingObjective;
                                var lkValue = new HashSet<int>(AppCategoryRat.AppRatingObjective.Select(e => e.Id));
                                if (lkValue.Count > 0)
                                {
                                    Msg.Add(" Child record exists.Cannot remove it..  ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            db.Entry(AppCategoryRat).State = System.Data.Entity.EntityState.Deleted;
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
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

        public ActionResult P2BGrid(P2BGrid_Parameters gp)
        {
            List<string> Msg = new List<string>();
            
                try
                {
                    DataBaseContext db = new DataBaseContext();
                    int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                    int pageSize = gp.rows;
                    int totalPages = 0;
                    int totalRecords = 0;
                    var jsonData = (Object)null;
                    IEnumerable<AppCategoryRating> AppCatRating = null;
                    if (gp.IsAutho == true)
                    {
                        AppCatRating = db.AppCategoryRating.Include(e => e.AppRatingObjective)
                            .Include(e => e.AppCategory).Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                    }
                    else
                    {
                        AppCatRating = db.AppCategoryRating.Include(e => e.AppRatingObjective).Include(e => e.AppCategory).AsNoTracking().ToList();
                    }

                    IEnumerable<AppCategoryRating> IE;
                    if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                    {
                        IE = AppCatRating;
                        if (gp.searchOper.Equals("eq"))
                        {
                            jsonData = IE.Where(e => (e.AppCategory.Name.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                               || (e.MaxRatingPoints.ToString().Contains(gp.searchString))
                               || (e.Id.ToString().Contains(gp.searchString))
                               ).Select(a => new Object[] { a.AppCategory.Name, a.MaxRatingPoints, a.Id }).ToList();
                            //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.AppCategory.Name, a.MaxRatingPoints, a.Id }).ToList();
                        }
                        totalRecords = IE.Count();
                    }
                    else
                    {
                        IE = AppCatRating;
                        Func<AppCategoryRating, dynamic> orderfuc;
                        if (gp.sidx == "Id")
                        {
                            orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                        }
                        else
                        {
                            orderfuc = (c => gp.sidx == "Category" ? c.AppCategory.Name.ToString() :
                                             gp.sidx == "MaxPoints" ? c.MaxRatingPoints.ToString() : "");
                        }
                        if (gp.sord == "asc")
                        {
                            IE = IE.OrderBy(orderfuc);
                            jsonData = IE.Select(a => new Object[] { Convert.ToString(a.AppCategory.Name), Convert.ToString(a.MaxRatingPoints), a.Id }).ToList();
                        }
                        else if (gp.sord == "desc")
                        {
                            IE = IE.OrderByDescending(orderfuc);
                            jsonData = IE.Select(a => new Object[] { Convert.ToString(a.AppCategory.Name), Convert.ToString(a.MaxRatingPoints), a.Id }).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.AppCategory.Name, a.MaxRatingPoints, a.Id }).ToList();
                        }
                        totalRecords = AppCatRating.Count();
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
        
	}
}