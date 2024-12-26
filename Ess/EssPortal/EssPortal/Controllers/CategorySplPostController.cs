//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using System.Web.Mvc;

//namespace P2BUltimate.Controllers.Recruitement.MainController
//{
//    public class CategorySplController : Controller
//    {
//        //
//        // GET: /CategorySpltroller/
//        public ActionResult Index()
//        {
//            return View();
//        }
//    }
//}



using P2b.Global;
using Payroll;
using EssPortal.App_Start;
using EssPortal.Models;
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
using Recruitment;
using EssPortal.Security;

namespace P2BUltimate.Controllers.Recruitement.MainController
{
    public class CategorySplPostController : Controller
    {
        //
        // GET: /Qualification/

      //  private DataBaseContext db = new DataBaseContext();

        public ActionResult partial()
        {

            return View("~/Views/Shared/_Categorysplpost.cshtml");
        }
        public ActionResult Create(CategorySplPost cp, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string CategorySpl = form["CategorySplPostlistP"] == "0" ? "" : form["CategorySplPostlistP"];


                    if (CategorySpl != null)
                    {
                        if (CategorySpl != "")
                        {
                            var val = db.LookupValue.Find(int.Parse(CategorySpl));
                            cp.SpecialCategory = val;
                        }
                    }

                    cp.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };


                    CategorySplPost cat = new CategorySplPost
                    {
                        NoOfVacancies = cp.NoOfVacancies,
                        SpecialCategory = cp.SpecialCategory,
                        RelaxationAge = cp.RelaxationAge,
                        FullDetails = cp.FullDetails,
                        DBTrack = cp.DBTrack
                    };
                    try
                    {
                        if (ModelState.IsValid)
                        {
                            using (TransactionScope ts = new TransactionScope())
                            {
                                db.CategorySplPost.Add(cat);
                                //DBTrackFile.DBTrackSave("Core/P2b.Global", "C", qualificn, null, "Qualification", null);
                                db.SaveChanges();
                                ts.Complete();
                            }
                        }
                        //return Json(new Object[] { cat.Id, cat.RelaxationAge, "Record Created", JsonRequestBehavior.AllowGet });
                        Msg.Add("  Data Created successfully  ");
                        return Json(new Utility.JsonReturnClass { Id = cat.Id, Val = cat.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }
                    catch (DataException e) { throw e; }
                    catch (DBConcurrencyException e) { throw e; }
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

        public ActionResult delete(int? data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var qurey = db.CategorySplPost.Find(data);
                    using (TransactionScope ts = new TransactionScope())
                    {
                        //db.LookupValue.Attach(qurey);
                        //db.Entry(qurey).State = System.Data.Entity.EntityState.Deleted;
                        //db.SaveChanges();
                        //db.Entry(qurey).State = System.Data.Entity.EntityState.Detached;
                        //ts.Complete();

                        //DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.EmpId, ModifiedOn = DateTime.Now };
                        db.Entry(qurey).State = System.Data.Entity.EntityState.Deleted;
                        //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dbT);
                        db.SaveChanges();
                        ts.Complete();
                        Msg.Add("  Data removed successfully.  ");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        // return this.Json(new { msg = "Data removed.", JsonRequestBehavior.AllowGet });
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

        public ActionResult edit(int? data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var adddata = db.CategorySplPost.Include(e => e.SpecialCategory).Where(e => e.Id == data).Select(e => new
                {
                    NoOfVacancies = e.NoOfVacancies,
                    RelaxationAge = e.RelaxationAge,
                    CategorySplType_Id = e.SpecialCategory.Id == null ? 0 : e.SpecialCategory.Id,
                    SpecialCategory = e.SpecialCategory,
                }).ToList();
                var Corp = db.CategorySplPost.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { adddata, Auth, JsonRequestBehavior.AllowGet });
            }
         }
        //[HttpPost]
        //public ActionResult edit(int data)
        //{
        //    //string tableName = "Corporate";

        //    //    // Fetch the table records dynamically
        //    //    var tableData = db.GetType()
        //    //    .GetProperty(tableName)
        //    //    .GetValue(db, null);

        //    var Q = db.Qualification
        //        .Include(e => e.QualificationType)

        //        .Where(e => e.Id == data).Select
        //        (e => new
        //        {
        //            QualificationShortName = e.QualificationShortName,
        //            QualificationDesc = e.QualificationDesc,
        //            BusinessType_Id = e.QualificationType.Id == null ? 0 : e.QualificationType.Id,
        //            Action = e.DBTrack.Action
        //        }).ToList();

        //    var adddata = db.Qualification.Include(e => e.QualificationType).Where(e => e.Id == data).ToList();

        //    var Corp = db.Qualification.Find(data);
        //    TempData["RowVersion"] = Corp.RowVersion;
        //    var Auth = Corp.DBTrack.IsModified;
        //    return Json(new Object[] { Q, adddata, Auth, JsonRequestBehavior.AllowGet });
        //}
        [HttpPost]
        public async Task<ActionResult> EditSave1(CategorySplPost c, int data, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    string CategorySpl = form["CategorySplPostListP"] == "0" ? "" : form["CategorySplPostListP"];
                    if (CategorySpl != null && CategorySpl != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(CategorySpl));
                        c.SpecialCategory = val;
                    }

                    var db_data = db.CategorySplPost.Include(e => e.SpecialCategory)

                                .Where(e => e.Id == data).SingleOrDefault();

                    bool Auth = form["autho_allow"] == "true" ? true : false;

                    if (Auth == false)
                    {
                        if (ModelState.IsValid)
                        {
                            try
                            {
                                //DbContextTransaction transaction = db.Database.BeginTransaction();   
                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    db.CategorySplPost.Attach(db_data);
                                    db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    TempData["RowVersion"] = db_data.RowVersion;
                                    db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;

                                    var Curr_Lookup = db.CategorySplPost.Find(data);
                                    TempData["CurrRowVersion"] = Curr_Lookup.RowVersion;
                                    db.Entry(Curr_Lookup).State = System.Data.Entity.EntityState.Detached;

                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {

                                        CategorySplPost blog = null; // to retrieve old data
                                        DbPropertyValues originalBlogValues = null;

                                        using (var context = new DataBaseContext())
                                        {
                                            blog = context.CategorySplPost.Where(e => e.Id == data).Include(e => e.SpecialCategory)

                                  .SingleOrDefault();
                                            originalBlogValues = context.Entry(blog).OriginalValues;
                                        }

                                        c.DBTrack = new DBTrack
                                        {
                                            CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                            CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                            Action = "M",
                                            ModifiedBy = SessionManager.EmpId,
                                            ModifiedOn = DateTime.Now
                                        };

                                        int a = EditS(CategorySpl, data, c, c.DBTrack);

                                        await db.SaveChangesAsync();


                                        //Qualification lk = new Qualification
                                        //{
                                        //    Id = data,
                                        //    QualificationShortName = db_data.QualificationShortName,
                                        //    QualificationDesc = db_data.QualificationDesc,
                                        //    QualificationType = db_data.QualificationType,
                                        //    DBTrack = c.DBTrack
                                        //};


                                        //db.Qualification.Attach(lk);
                                        //db.Entry(lk).State = System.Data.Entity.EntityState.Modified;
                                        //db.Entry(lk).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                        // int a = EditS(Corp, Addrs, ContactDetails, data, c, c.DBTrack);

                                        // int a = EditS(Values, Valueshobby, Valuesls, Valuesqd, Valuesskl, Valuesscr, data, c, c.DBTrack);


                                        using (var context = new DataBaseContext())
                                        {

                                            //  var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                            // var Obj = DBTrackFile.ModifiedDataHistory("Recruitement.CategorySplPost", "M", blog, c, "SpecialCategory", c.DBTrack);
                                            //.DT_Corp = (DT_Qualification)Obj;
                                            //db.DT_Qualification.Add(DT_Corp);

                                            // db.Create(DT_Corp);
                                            db.SaveChanges();
                                        }
                                        await db.SaveChangesAsync();
                                        ts.Complete();

                                        Msg.Add("  Record Updated");
                                        return Json(new Utility.JsonReturnClass { Id = c.Id, Val = c.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        // return Json(new Object[] { c.Id, "", "Record Updated", JsonRequestBehavior.AllowGet });
                                    }
                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (CategorySplPost)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (CategorySplPost)databaseEntry.ToObject();
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

                            CategorySplPost blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            CategorySplPost Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.CategorySplPost.Where(e => e.Id == data).SingleOrDefault();
                                originalBlogValues = context.Entry(blog).OriginalValues;
                            }
                            c.DBTrack = new DBTrack
                            {
                                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                Action = "M",
                                IsModified = blog.DBTrack.IsModified == true ? true : false,
                                ModifiedBy = SessionManager.EmpId,
                                ModifiedOn = DateTime.Now
                            };
                            CategorySplPost Info = new CategorySplPost()
                            {

                                Id = data,
                                NoOfVacancies = c.NoOfVacancies,
                                RelaxationAge = c.RelaxationAge,
                                SpecialCategory = c.SpecialCategory,
                                DBTrack = c.DBTrack
                            };

                            db.Entry(blog).State = System.Data.Entity.EntityState.Detached;
                            Info.DBTrack = c.DBTrack;
                            // db.EmpAcademicInfo.Attach(empAcademicInfo);                   
                            //db.Entry(empAcademicInfo).State = System.Data.Entity.EntityState.Modified;
                            //db.Entry(empAcademicInfo).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = blog.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { blog.Id, "", "Record Updated", JsonRequestBehavior.AllowGet });
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

        [HttpPost]
        public async Task<ActionResult> EditSave(CategorySplPost c, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {

                    string CategorySpl = form["CategorySplPostListP"] == "0" ? "" : form["CategorySplPostListP"];
                    if (CategorySpl != null && CategorySpl != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(CategorySpl));
                        c.SpecialCategory = val;
                    }

                    var db_data = db.CategorySplPost.Include(e => e.SpecialCategory)

                                .Where(e => e.Id == data).SingleOrDefault();
                    bool Auth = form["autho_allow"] == "true" ? true : false;


                    CategorySplPost blog = null;
                    if (CategorySpl != null & CategorySpl != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(CategorySpl));
                        c.SpecialCategory = val;

                        var type = db.CategorySplPost.Include(e => e.SpecialCategory).Where(e => e.Id == data).SingleOrDefault();

                        if (type.SpecialCategory != null)
                        {
                            blog = db.CategorySplPost.Where(x => x.SpecialCategory.Id == type.SpecialCategory.Id && x.Id == data).SingleOrDefault();
                        }
                        else
                        {
                            blog = db.CategorySplPost.Where(x => x.Id == data).SingleOrDefault();
                        }
                        blog.SpecialCategory = c.SpecialCategory;
                    }
                    else
                    {
                        blog = db.CategorySplPost.Include(e => e.SpecialCategory).Where(x => x.Id == data).SingleOrDefault();
                        blog.SpecialCategory = null;
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
                        //            db.CategorySplPost.Attach(db_data);
                        //            db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                        //            db.SaveChanges();
                        //            TempData["RowVersion"] = db_data.RowVersion;
                        //            db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;

                        //            var Curr_Lookup = db.CategorySplPost.Find(data);
                        //            TempData["CurrRowVersion"] = Curr_Lookup.RowVersion;
                        //            db.Entry(Curr_Lookup).State = System.Data.Entity.EntityState.Detached;

                        //            if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                        //            {

                        //                CategorySplPost blog = null; // to retrieve old data
                        //                DbPropertyValues originalBlogValues = null;

                        //                using (var context = new DataBaseContext())
                        //                {
                        //                    blog = context.CategorySplPost.Where(e => e.Id == data).Include(e => e.SpecialCategory)
                        //                                      .SingleOrDefault();
                        //                    originalBlogValues = context.Entry(blog).OriginalValues;
                        //                }

                        //                c.DBTrack = new DBTrack
                        //                {
                        //                    CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                        //                    CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                        //                    Action = "M",
                        //                    ModifiedBy = SessionManager.UserName,
                        //                    ModifiedOn = DateTime.Now
                        //                };
                        //                CategorySplPost lk = new CategorySplPost
                        //                {
                        //                    Id = data,
                        //                    SpecialCategory = c.SpecialCategory,
                        //                    NoOfVacancies = c.NoOfVacancies,
                        //                    RelaxationAge = c.RelaxationAge,
                        //                    DBTrack = c.DBTrack
                        //                };


                        //                db.CategorySplPost.Attach(lk);
                        //                db.Entry(lk).State = System.Data.Entity.EntityState.Modified;

                        //                db.Entry(lk).OriginalValues["RowVersion"] = TempData["RowVersion"];
                        //                // int a = EditS(Corp, Addrs, ContactDetails, data, c, c.DBTrack);

                        //                // int a = EditS(Values, Valueshobby, Valuesls, Valuesqd, Valuesskl, Valuesscr, data, c, c.DBTrack);


                        //                using (var context = new DataBaseContext())
                        //                {

                        //                    //    var obj = DBTrackFile.DBTrackSave("Recruitement/Recruitement", originalBlogValues, db.ChangeTracker, c.DBTrack);
                        //                    //   DT_CategoryPost DT_Corp = (DT_CategoryPost)obj;

                        //                    //   db.Create(DT_Corp);
                        //                    //   db.SaveChanges();
                        //                }
                        //                await db.SaveChangesAsync();
                        //                ts.Complete();

                        //                Msg.Add("  Record Updated");
                        //                return Json(new Utility.JsonReturnClass { Id = lk.Id, Val = lk.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //                //return Json(new Object[] { lk.Id, "", "Record Updated", JsonRequestBehavior.AllowGet });
                        //            }
                        //        }
                        //    }
                        //    catch (DbUpdateConcurrencyException ex)
                        //    {
                        //        var entry = ex.Entries.Single();
                        //        var clientValues = (CategorySplPost)entry.Entity;
                        //        var databaseEntry = entry.GetDatabaseValues();
                        //        if (databaseEntry == null)
                        //        {
                        //            Msg.Add(" Unable to save changes. The record was deleted by another user.");
                        //            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //            //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                        //        }
                        //        else
                        //        {
                        //            var databaseValues = (CategorySplPost)databaseEntry.ToObject();
                        //            c.RowVersion = databaseValues.RowVersion;

                        //        }
                        //    }
                        //    Msg.Add("Record modified by another user.So refresh it and try to save again.");
                        //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //    // return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                        //}

                        if (ModelState.IsValid)
                        {
                            try
                            {
                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    //CategorySplPost blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.CategorySplPost.Where(e => e.Id == data).Include(e => e.SpecialCategory)
                                                              .SingleOrDefault();
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
                                    var m1 = db.CategorySplPost.Where(e => e.Id == data).ToList();
                                    foreach (var s in m1)
                                    {
                                        // s.AppraisalPeriodCalendar = null;
                                        db.CategorySplPost.Attach(s);
                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                        //await db.SaveChangesAsync();
                                        db.SaveChanges();
                                        TempData["RowVersion"] = s.RowVersion;
                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    }
                                    //int a = EditS(Category, PromoPolicy, data, c, c.DBTrack);
                                    var CurCorp = db.CategorySplPost.Find(data);
                                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {

                                        CategorySplPost corp = new CategorySplPost()
                                        {
                                            Id = data,
                                            SpecialCategory = c.SpecialCategory,
                                            NoOfVacancies = c.NoOfVacancies,
                                            RelaxationAge = c.RelaxationAge,
                                            DBTrack = c.DBTrack
                                        };

                                        db.CategorySplPost.Attach(corp);
                                        db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                                        db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                        // return 1;

                                        //using (var context = new DataBaseContext())
                                        //{
                                        //    var obj = DBTrackFile.DBTrackSave("Recruitement/Recruitement", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                        //    DT_CategoryPost DT_Corp = (DT_CategoryPost)obj;
                                        //    DT_Corp.PayScaleArea_Id = blog.PayScaleType == null ? 0 : blog.PayScaleType.Id;

                                        //    db.Create(DT_Corp);

                                        //}
                                        db.SaveChanges();
                                        await db.SaveChangesAsync();
                                        var categoryspecial = db.CategorySplPost.Where(e => e.Id == data).Include(e => e.SpecialCategory)
                                                             .SingleOrDefault();
                                        ts.Complete();
                                       
                                        Msg.Add("  Record Updated");
                                        return Json(new Utility.JsonReturnClass { Id = categoryspecial.Id, Val = categoryspecial.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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

                            //CategorySplPost blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            CategorySplPost Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.CategorySplPost.Include(e => e.SpecialCategory).Where(e => e.Id == data).SingleOrDefault();
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
                            CategorySplPost qualificationDetails = new CategorySplPost()
                            {

                                Id = data,
                                SpecialCategory = c.SpecialCategory,
                                NoOfVacancies = c.NoOfVacancies,
                                RelaxationAge = c.RelaxationAge,
                                DBTrack = c.DBTrack
                            };

                            using (var context = new DataBaseContext())
                            {
                                //var obj = DBTrackFile.ModifiedDataHistory("Recruitement/Recruitement", "M", blog, qualificationDetails, "CategorySplPost", c.DBTrack);
                                // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                //   Old_Corp = context.CategorySplPost.Where(e => e.Id == data)
                                //        .Include(e => e.SpecialCategory).SingleOrDefault();
                                //DT_LanguageSkill DT_Corp = (DT_LanguageSkill)obj;
                                //DT_Corp.Address_Id = DBTrackFile.ValCompare(Old_Corp.Address, c.Address);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
                                //DT_Corp.BusinessType_Id = DBTrackFile.ValCompare(Old_Corp.BusinessType, c.BusinessType); //Old_Corp.BusinessType == c.BusinessType ? 0 : Old_Corp.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_Corp.BusinessType.Id;
                                //DT_Corp.ContactDetails_Id = DBTrackFile.ValCompare(Old_Corp.ContactDetails, c.ContactDetails); //Old_Corp.ContactDetails == c.ContactDetails ? 0 : Old_Corp.ContactDetails == null && c.ContactDetails != null ? c.ContactDetails.Id : Old_Corp.ContactDetails.Id;
                                //   db.Create(DT_Corp);
                                //db.SaveChanges();
                            }
                            blog.DBTrack = c.DBTrack;
                            db.CategorySplPost.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();

                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = c.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { blog.Id, c.Institute, "Record Updated", JsonRequestBehavior.AllowGet });
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





        public int EditS(string CategorySpl, int data, CategorySplPost c, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                CategorySplPost CatPost = null;
                if (CategorySpl != null & CategorySpl != "")
                {
                    var val = db.LookupValue.Find(int.Parse(CategorySpl));
                    c.SpecialCategory = val;

                    var type = db.CategorySplPost.Include(e => e.SpecialCategory).Where(e => e.Id == data).SingleOrDefault();

                    if (type.SpecialCategory != null)
                    {
                        CatPost = db.CategorySplPost.Where(x => x.SpecialCategory.Id == type.SpecialCategory.Id && x.Id == data).SingleOrDefault();
                    }
                    else
                    {
                        CatPost = db.CategorySplPost.Where(x => x.Id == data).SingleOrDefault();
                    }
                    CatPost.SpecialCategory = c.SpecialCategory;
                }
                else
                {
                    CatPost = db.CategorySplPost.Include(e => e.SpecialCategory).Where(x => x.Id == data).SingleOrDefault();
                    CatPost.SpecialCategory = null;
                }
                db.CategorySplPost.Attach(CatPost);
                db.Entry(CatPost).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                TempData["RowVersion"] = CatPost.RowVersion;
                db.Entry(CatPost).State = System.Data.Entity.EntityState.Detached;

                var CurCorp = db.CategorySplPost.Find(data);
                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    c.DBTrack = dbT;
                    CategorySplPost corp = new CategorySplPost()
                    {
                        Id = data,
                        NoOfVacancies = c.NoOfVacancies,
                        RelaxationAge = c.RelaxationAge,
                        SpecialCategory = c.SpecialCategory,
                        DBTrack = c.DBTrack
                    };

                    db.CategorySplPost.Attach(corp);
                    db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                    return 1;
                }
                return 0;
            }
        }
    }
}