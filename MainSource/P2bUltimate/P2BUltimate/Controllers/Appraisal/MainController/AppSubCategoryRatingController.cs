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
    public class AppSubCategoryRatingController : Controller
    {
        //  private DataBaseContext db = new DataBaseContext();
        //
        // GET: /AppSubCategoryRating/
        public ActionResult Index()
        {
            return View("~/Views/Appraisal/MainViews/AppSubCategoryRating/Index.cshtml");
        }

        public ActionResult GetAppSubCatDetailLKDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.AppSubCategory.ToList();
                IEnumerable<AppSubCategory> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.AppSubCategory.ToList().Where(d => d.Code.Contains(data));
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
        public ActionResult Create(AppSubCategoryRating p, FormCollection form) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string AppSubCat = form["AppSubCategorylist"] == "0" ? "" : form["AppSubCategorylist"];
                var AppRatingObj = form["AppRatingObjectivelist"] == "0" ? "" : form["AppRatingObjectivelist"];

                List<String> Msg = new List<String>();
                try
                {
                    if (AppSubCat != null && AppSubCat != "")
                    {
                        int AppSubCatId = Convert.ToInt32(AppSubCat);
                        var val = db.AppSubCategory.Where(e => e.Id == AppSubCatId).SingleOrDefault();
                        p.AppSubCategory = val;
                    }
                    else
                    {
                        Msg.Add("Kindly Enter Appraisal SubCategory.");
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


                            if (db.AppSubCategoryRating.Any(o => o.AppSubCategory.Id == p.AppSubCategory.Id))
                            {
                                Msg.Add("Record Already Exists.");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                            int AppCatPoints = 0;
                            int AppSubCatPoints = p.MaxPoints;
                            int Count = 0;
                            int AppCatId = 0;
                            var AppCatlist = db.AppCategory.Include(e => e.AppSubCategory).ToList();
                            foreach (var a in AppCatlist)
                            {
                                foreach (var b in a.AppSubCategory)
                                {
                                    if (b.Id == p.AppSubCategory.Id)
                                    {
                                        if (!db.AppCategoryRating.Any(o => o.AppCategory.Id == a.Id))
                                        {
                                            Msg.Add("Kindly enter Appraisal Category Rating.");
                                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        }
                                        AppCatId = a.Id;
                                        Count = 1;
                                        break;
                                    }
                                }
                                if (Count > 0)
                                    break;
                            }

                            if (Count > 0)
                            {
                                AppCatPoints = db.AppCategoryRating.Where(e => e.AppCategory.Id == AppCatId).Select(e => e.MaxRatingPoints).SingleOrDefault();
                                var Catlist = db.AppCategory.Where(e => e.Id == AppCatId).Include(e => e.AppSubCategory).Select(e => e.AppSubCategory).SingleOrDefault();
                                foreach (var b in Catlist)
                                {
                                    AppSubCatPoints = AppSubCatPoints + db.AppSubCategoryRating.Include(e => e.AppSubCategory).Where(e => e.AppSubCategory.Id == b.Id).Select(e => e.MaxPoints).SingleOrDefault();
                                }

                            }

                            if (AppCatPoints < AppSubCatPoints)
                            {
                                Msg.Add("SubCategory points can't be more than category points.");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }

                            p.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            AppSubCategoryRating AppSubCategoryRat = new AppSubCategoryRating()
                            {
                                AppRatingObjective = p.AppRatingObjective,
                                AppSubCategory = p.AppSubCategory,
                                MaxPoints = p.MaxPoints,
                                DBTrack = p.DBTrack
                            };

                            db.AppSubCategoryRating.Add(AppSubCategoryRat);
                            var rtn_Obj = DBTrackFile.DBTrackSave("Appraisal/Appraisal", null, db.ChangeTracker, p.DBTrack);
                            DT_AppSubCategoryRating DT_Corp = (DT_AppSubCategoryRating)rtn_Obj;
                            DT_Corp.AppSubCategory_Id = p.AppSubCategory == null ? 0 : p.AppSubCategory.Id;
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
            public string AppSubCategory_Id { get; set; }
            public string AppSubCategory_FullDetails { get; set; }
        }

        [HttpPost]
        public ActionResult Edit(int data)
        {
            List<IsRatingObjectiveDetailsC> return_data = new List<IsRatingObjectiveDetailsC>();
            using (DataBaseContext db = new DataBaseContext())
            {
                var r = db.AppSubCategoryRating
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        MaxPoints = e.MaxPoints,
                        Action = e.DBTrack.Action
                    }).ToList();

                var a = db.AppSubCategoryRating.Include(e => e.AppSubCategory)
                    .Include(e => e.AppRatingObjective.Select(b => b.ObjectiveWordings))
                 .Where(e => e.Id == data).ToList();
                foreach (var ca in a)
                {
                    return_data.Add(new IsRatingObjectiveDetailsC
                    {
                        AppRatingObjective_Id = ca.AppRatingObjective.Select(e => e.Id.ToString()).ToArray(),
                        AppRatingObjective_FullDetails = ca.AppRatingObjective.Select(e => e.FullDetails).ToArray(),
                        AppSubCategory_Id = ca.AppSubCategory.Id.ToString(),
                        AppSubCategory_FullDetails = ca.AppSubCategory.FullDetails,
                    });
                }

                var LKup = db.AppSubCategoryRating.Find(data);
                TempData["RowVersion"] = LKup.RowVersion;
                var Auth = LKup.DBTrack.IsModified;

                return this.Json(new Object[] { r, return_data, "", Auth, JsonRequestBehavior.AllowGet });
            }
        }


       
        //public async Task<ActionResult> EditSave(AppSubCategoryRating add, int data, FormCollection form) // Edit submit
        //{
        //    List<string> Msg = new List<string>();
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        try
        //        {
        //            string AppSubCategory = form["AppSubCategorylist"] == "0" ? "" : form["AppSubCategorylist"];
        //            string AppRatingObjective = form["AppRatingObjectivelist"] == "0" ? "" : form["AppRatingObjectivelist"];
        //            var db_data = db.AppSubCategoryRating.Include(e => e.AppRatingObjective).Where(e => e.Id == data).SingleOrDefault();
        //            bool Auth = form["Autho_Allow"] == "true" ? true : false;
        //            try
        //            {

        //                if (AppSubCategory != null && AppSubCategory != "" && AppSubCategory != "0")
        //                {
        //                    var val = db.AppSubCategory.Find(int.Parse(AppSubCategory));
        //                    add.AppSubCategory = val;
        //                }

        //                List<AppRatingObjective> ObjITsection = new List<AppRatingObjective>();
        //                AppRatingObjective pd = null;
        //                pd = db.AppRatingObjective.Include(e => e.ObjectiveWordings).Where(e => e.Id == data).SingleOrDefault();
        //                if (AppRatingObjective != null && AppRatingObjective != "")
        //                {
        //                    var ids = Utility.StringIdsToListIds(AppRatingObjective);
        //                    foreach (var ca in ids)
        //                    {
        //                        var value = db.AppRatingObjective.Find(ca);
        //                        ObjITsection.Add(value);
        //                        db_data.AppRatingObjective = ObjITsection;

        //                    }
        //                }
        //                else
        //                {
        //                    db_data.AppRatingObjective = null;
        //                }

        //                int AppCatPoints = 0;
        //                int AppSubCatPoints = 0;
        //                int Count = 0;
        //                int AppCatId = 0;
        //                var AppCatlist = db.AppCategory.Include(e => e.AppSubCategory).ToList();
        //                foreach (var a in AppCatlist)
        //                {
        //                    foreach (var b in a.AppSubCategory)
        //                    {
        //                        if (b.Id == add.AppSubCategory.Id)
        //                        {
        //                            AppCatId = a.Id;
        //                            Count = 1;
        //                            break;
        //                        }
        //                    }
        //                    if (Count > 0)
        //                        break;
        //                }

        //                if (Count > 0)
        //                {
        //                    AppCatPoints = db.AppCategoryRating.Where(e => e.AppCategory.Id == AppCatId).Select(e => e.MaxRatingPoints).SingleOrDefault();
        //                    var Catlist = db.AppCategory.Where(e => e.Id == AppCatId).Include(e => e.AppSubCategory).Select(e => e.AppSubCategory).SingleOrDefault();
        //                    foreach (var b in Catlist)
        //                    {
        //                        AppSubCatPoints = AppSubCatPoints + db.AppSubCategoryRating.Include(e => e.AppSubCategory).Where(e => e.AppSubCategory.Id == b.Id).Select(e => e.MaxPoints).SingleOrDefault();
        //                    }

        //                }

        //                if (AppCatPoints < AppSubCatPoints)
        //                {
        //                    Msg.Add("SubCategory points can't be more than category points.");
        //                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                }

        //                db.AppSubCategoryRating.Attach(db_data);
        //                db.Entry(pd).State = System.Data.Entity.EntityState.Modified;
        //                db.SaveChanges();
        //                TempData["RowVersion"] = pd.RowVersion;
        //                db.Entry(pd).State = System.Data.Entity.EntityState.Detached;

        //                if (Auth == false)
        //                {
        //                    if (ModelState.IsValid)
        //                    {
        //                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                        {
        //                            AppSubCategoryRating blog = null; // to retrieve old data
        //                            DbPropertyValues originalBlogValues = null;

        //                            using (var context = new DataBaseContext())
        //                            {
        //                                blog = context.AppSubCategoryRating.Where(e => e.Id == data)
        //                            .Include(e => e.AppSubCategory).Include(e => e.AppRatingObjective).SingleOrDefault();
        //                                originalBlogValues = context.Entry(blog).OriginalValues;
        //                            }

        //                            add.DBTrack = new DBTrack
        //                            {
        //                                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                                Action = "M",
        //                                ModifiedBy = SessionManager.UserName,
        //                                ModifiedOn = DateTime.Now
        //                            };

        //                            int a = EditS(AppSubCategory, AppRatingObjective, data, add, add.DBTrack);



        //                            using (var context = new DataBaseContext())
        //                            {
        //                                var obj = DBTrackFile.DBTrackSave("Appraisal/Appraisal", originalBlogValues, db.ChangeTracker, add.DBTrack);
        //                                //DT_AppSubCategoryRating DT_Addrs = (DT_AppSubCategoryRating)obj;
        //                               // DT_Addrs.AppSubCategory_Id = blog.AppSubCategory == null ? 0 : blog.AppSubCategory.Id;
        //                               // db.Create(DT_Addrs);

        //                                db.SaveChanges();
        //                            }
        //                            await db.SaveChangesAsync();
        //                            ts.Complete();
        //                            Msg.Add("Record Updated");
        //                            return Json(new Utility.JsonReturnClass { Id = data, Val = blog.Id.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                            //    return Json(new { blog.Id, query.FullAddress, , JsonRequestBehavior.AllowGet });
        //                        }


        //                        //  return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
        //                    }
        //                }
        //                else
        //                {
        //                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                    {

        //                        AppSubCategoryRating blog = null; // to retrieve old data
        //                        DbPropertyValues originalBlogValues = null;
        //                        AppSubCategoryRating Old_Addrs = null;

        //                        using (var context = new DataBaseContext())
        //                        {
        //                            blog = context.AppSubCategoryRating.Where(e => e.Id == data).SingleOrDefault();
        //                            originalBlogValues = context.Entry(blog).OriginalValues;
        //                        }
        //                        add.DBTrack = new DBTrack
        //                        {
        //                            CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                            CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                            Action = "M",
        //                            IsModified = blog.DBTrack.IsModified == true ? true : false,
        //                            ModifiedBy = SessionManager.UserName,
        //                            ModifiedOn = DateTime.Now
        //                        };
        //                        AppSubCategoryRating L = new AppSubCategoryRating()
        //                        {
        //                            MaxPoints = add.MaxPoints == null ? 0 : add.MaxPoints,
        //                            AppSubCategory = add.AppSubCategory,
        //                            AppRatingObjective = add.AppRatingObjective,
        //                            DBTrack = add.DBTrack,
        //                            Id = data
        //                        };


        //                        //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

        //                        using (var context = new DataBaseContext())
        //                        {
        //                            var obj = DBTrackFile.ModifiedDataHistory("Appraisal/Appraisal", "M", blog, add, "AppSubCategoryRating", add.DBTrack);
        //                            // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

        //                            Old_Addrs = context.AppSubCategoryRating.Where(e => e.Id == data)
        //                            .Include(e => e.AppSubCategory).Include(e => e.AppRatingObjective).SingleOrDefault();
        //                           // DT_AppSubCategoryRating DT_Addrs = (DT_AppSubCategoryRating)obj;

        //                           // DT_Addrs.AppSubCategory_Id = DBTrackFile.ValCompare(Old_Addrs.AppSubCategory, add.AppSubCategory);
        //                           // db.Create(DT_Addrs);
        //                            //db.SaveChanges();
        //                        }
        //                        blog.DBTrack = add.DBTrack;
        //                        db.AppSubCategoryRating.Attach(blog);
        //                        db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
        //                        db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //                        db.SaveChanges();
        //                        ts.Complete();
        //                        //  return Json(new Object[] { blog.Id, add.FullAddress, "Record Updated", JsonRequestBehavior.AllowGet });
        //                        Msg.Add("Record Updated");
        //                        return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = blog.Id.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                    }

        //                }
        //            }
        //            catch (DbUpdateConcurrencyException ex)
        //            {
        //                var entry = ex.Entries.Single();
        //                var clientValues = (AppManualAssignment)entry.Entity;
        //                var databaseEntry = entry.GetDatabaseValues();
        //                if (databaseEntry == null)
        //                {
        //                    Msg.Add(ex.Message);
        //                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                }
        //                else
        //                {
        //                    var databaseValues = (AppManualAssignment)databaseEntry.ToObject();
        //                    add.RowVersion = databaseValues.RowVersion;

        //                }
        //            }
        //            catch (Exception e)
        //            {
        //                Msg.Add(e.Message);
        //                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                //return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
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

        [HttpPost]
        public async Task<ActionResult> EditSave(AppSubCategoryRating add, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string AppSubCategory = form["AppSubCategorylist"] == "0" ? "" : form["AppSubCategorylist"];
                    string AppRatingObjective = form["AppRatingObjectivelist"] == "0" ? "" : form["AppRatingObjectivelist"];

                    bool Auth = form["Autho_Allow"] == "true" ? true : false;
                    try
                    {

                        var db_data = db.AppSubCategoryRating.Include(e => e.AppRatingObjective).Where(e => e.Id == data).SingleOrDefault();

                        if (AppSubCategory != null && AppSubCategory != "" && AppSubCategory != "0")
                        {
                            var val = db.AppSubCategory.Find(int.Parse(AppSubCategory));
                            db_data.AppSubCategory = val;
                        }

                        List<AppRatingObjective> ObjITsection = new List<AppRatingObjective>();
                        AppRatingObjective pd = null;
                        pd = db.AppRatingObjective.Include(e => e.ObjectiveWordings).Where(e => e.Id == data).SingleOrDefault();
                        if (AppRatingObjective != null && AppRatingObjective != "")
                        {
                            var ids = Utility.StringIdsToListIds(AppRatingObjective);
                            foreach (var ca in ids)
                            {
                                var value = db.AppRatingObjective.Find(ca);
                                ObjITsection.Add(value);
                                db_data.AppRatingObjective = ObjITsection;

                            }
                        }
                        else
                        {
                            db_data.AppRatingObjective = null;
                        }


                        db.AppSubCategoryRating.Attach(db_data);
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
                                    AppSubCategoryRating blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.AppSubCategoryRating.Where(e => e.Id == data)
                                     .Include(e => e.AppSubCategory).Include(e => e.AppRatingObjective).SingleOrDefault();
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

                                    var m1 = db.AppSubCategoryRating.Where(e => e.Id == data).ToList();
                                    foreach (var s in m1)
                                    {
                                        // s.AppraisalPeriodCalendar = null;
                                        db.AppSubCategoryRating.Attach(s);
                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                        //await db.SaveChangesAsync();
                                        db.SaveChanges();
                                        TempData["RowVersion"] = s.RowVersion;
                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    }

                                    //int a = EditS(AppCategory, AppRatingObjective, data, add, add.DBTrack);
                                    var CurCorp = db.AppSubCategoryRating.Find(data);
                                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;

                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {

                                        AppSubCategoryRating corp = new AppSubCategoryRating()
                                        {
                                            MaxPoints = add.MaxPoints == null ? 0 : add.MaxPoints,
                                            AppSubCategory = add.AppSubCategory,
                                            Id = data,
                                            DBTrack = add.DBTrack
                                        };

                                        db.AppSubCategoryRating.Attach(corp);
                                        db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                                        db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                        // return 1;
                                    }


                                    using (var context = new DataBaseContext())
                                    {
                                        var obj = DBTrackFile.DBTrackSave("Appraisal/Appraisal", originalBlogValues, db.ChangeTracker, add.DBTrack);
                                        DT_AppSubCategoryRating DT_Addrs = (DT_AppSubCategoryRating)obj;
                                        DT_Addrs.AppSubCategory_Id = blog.AppSubCategory == null ? 0 : blog.AppSubCategory.Id;
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
                                AppSubCategoryRating Old_Addrs = null;

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
                                AppSubCategoryRating L = new AppSubCategoryRating()
                                {
                                    MaxPoints = add.MaxPoints == null ? 0 : add.MaxPoints,
                                    AppSubCategory = add.AppSubCategory,
                                    AppRatingObjective = add.AppRatingObjective,
                                    DBTrack = add.DBTrack,
                                    Id = data
                                };


                                //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                                using (var context = new DataBaseContext())
                                {
                                    var obj = DBTrackFile.ModifiedDataHistory("Appraisal/Appraisal", "M", blog, add, "AppSubCategoryRating", add.DBTrack);
                                    // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                    Old_Addrs = context.AppSubCategoryRating.Where(e => e.Id == data)
                                    .Include(e => e.AppSubCategory).Include(e => e.AppRatingObjective).SingleOrDefault();
                                    DT_AppSubCategoryRating DT_Addrs = (DT_AppSubCategoryRating)obj;

                                    DT_Addrs.AppSubCategory_Id = DBTrackFile.ValCompare(Old_Addrs.AppSubCategory, add.AppSubCategory);
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

        //public int EditS(string AppSubCategory, string AppRatingObjective, int data, AppSubCategoryRating c, DBTrack dbT)
        //{
        //    IList<AppSubCategoryRating> typedetails = null;
        //    using (DataBaseContext db = new DataBaseContext())
        //    {

        //        if (AppSubCategory != null && AppSubCategory != "")
        //        {
        //            var val = db.AppSubCategory.Find(int.Parse(AppSubCategory));
        //            c.AppSubCategory = val;

        //            var type = db.AppSubCategoryRating.Include(e => e.AppSubCategory).Where(e => e.Id == data).SingleOrDefault();
        //            if (type.AppSubCategory != null)
        //            {
        //                typedetails = db.AppSubCategoryRating.Where(x => x.AppSubCategory.Id == type.AppSubCategory.Id && x.Id == data).ToList();
        //            }
        //            else
        //            {
        //                typedetails = db.AppSubCategoryRating.Where(x => x.Id == data).ToList();
        //            }

        //            foreach (var s in typedetails)
        //            {
        //                s.AppSubCategory = c.AppSubCategory;
        //                db.AppSubCategoryRating.Attach(s);
        //                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                //await db.SaveChangesAsync();
        //                db.SaveChanges();
        //                TempData["RowVersion"] = s.RowVersion;
        //                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //            }
        //        }
        //        else
        //        {
        //            var BusiTypeDetails = db.AppSubCategoryRating.Include(e => e.AppSubCategory).Where(x => x.Id == data).ToList();
        //            foreach (var s in BusiTypeDetails)
        //            {
        //                s.AppSubCategory = null;
        //                db.AppSubCategoryRating.Attach(s);
        //                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                //await db.SaveChangesAsync();
        //                db.SaveChanges();
        //                TempData["RowVersion"] = s.RowVersion;
        //                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //            }
        //        }


        //        var CurCorp = db.AppSubCategoryRating.Find(data);
        //        TempData["CurrRowVersion"] = CurCorp.RowVersion;
        //        db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
        //        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
        //        {
        //            c.DBTrack = dbT;
        //            AppSubCategoryRating corp = new AppSubCategoryRating()
        //            {
        //                MaxPoints = c.MaxPoints == null ? 0 : c.MaxPoints,
        //                AppSubCategory = c.AppSubCategory,
        //                Id = data,
        //                DBTrack = c.DBTrack
        //            };


        //            db.AppSubCategoryRating.Attach(corp);
        //            db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
        //            db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //            //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
        //            return 1;
        //        }
        //        return 0;
        //    }
        //}


        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int? data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    AppSubCategoryRating AppSubCategoryRat = db.AppSubCategoryRating.Include(e => e.AppRatingObjective)
                        .Include(e => e.AppSubCategory).Where(e => e.Id == data).SingleOrDefault();




                    if (AppSubCategoryRat.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack, corporates, null, "Corporate");
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = AppSubCategoryRat.DBTrack.CreatedBy != null ? AppSubCategoryRat.DBTrack.CreatedBy : null,
                                CreatedOn = AppSubCategoryRat.DBTrack.CreatedOn != null ? AppSubCategoryRat.DBTrack.CreatedOn : null,
                                IsModified = AppSubCategoryRat.DBTrack.IsModified == true ? true : false
                            };
                            AppSubCategoryRat.DBTrack = dbT;
                            db.Entry(AppSubCategoryRat).State = System.Data.Entity.EntityState.Modified;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Appraisal/Appraisal", null, db.ChangeTracker, AppSubCategoryRat.DBTrack);
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
                            if (AppSubCategoryRat.AppRatingObjective != null)
                            {
                                var selectedValues = AppSubCategoryRat.AppRatingObjective;
                                var lkValue = new HashSet<int>(AppSubCategoryRat.AppRatingObjective.Select(e => e.Id));
                                if (lkValue.Count > 0)
                                {
                                    Msg.Add(" Child record exists.Cannot remove it..  ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            db.Entry(AppSubCategoryRat).State = System.Data.Entity.EntityState.Deleted;
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
                IEnumerable<AppSubCategoryRating> AppSubCatRating = null;
                if (gp.IsAutho == true)
                {
                    AppSubCatRating = db.AppSubCategoryRating.Include(e => e.AppRatingObjective)
                        .Include(e => e.AppSubCategory).Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    AppSubCatRating = db.AppSubCategoryRating.Include(e => e.AppRatingObjective)
                        .Include(e => e.AppSubCategory).AsNoTracking().ToList();
                }

                IEnumerable<AppSubCategoryRating> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = AppSubCatRating;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.AppSubCategory.Name.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                               || (e.MaxPoints.ToString().Contains(gp.searchString))
                               || (e.Id.ToString().Contains(gp.searchString))
                               ).Select(a => new Object[] { a.AppSubCategory.Name, a.MaxPoints, a.Id }).ToList();

                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.AppSubCategory.Name, a.MaxPoints, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = AppSubCatRating;
                    Func<AppSubCategoryRating, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "SubCategory" ? c.AppSubCategory.Name.ToString() :
                                         gp.sidx == "MaxPoints" ? c.MaxPoints.ToString() : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.AppSubCategory.Name), Convert.ToString(a.MaxPoints), a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.AppSubCategory.Name), Convert.ToString(a.MaxPoints), a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.AppSubCategory.Name, a.MaxPoints, a.Id }).ToList();
                    }
                    totalRecords = AppSubCatRating.Count();
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