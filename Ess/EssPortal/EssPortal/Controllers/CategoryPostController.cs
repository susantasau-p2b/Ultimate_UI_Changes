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
    public class CategoryPostController : Controller
    {
        //
        // GET: /Qualification/

        //private DataBaseContext db = new DataBaseContext();

        public ActionResult partial()
        {
            return View("~/Views/Shared/_Category.cshtml");
        }
        public ActionResult Create(CategoryPost cp, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string Category = form["CategoryPostListP"] == "0" ? "" : form["CategoryPostListP"];

                    if (Category != null)
                    {
                        if (Category != "")
                        {
                            var val = db.LookupValue.Find(int.Parse(Category));
                            cp.Category = val;
                        }
                    }

                    cp.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };


                    CategoryPost cat = new CategoryPost
                    {
                        NoOfVacancies = cp.NoOfVacancies,
                        Category = cp.Category,
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
                                db.CategoryPost.Add(cat);
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
                    var qurey = db.CategoryPost.Find(data);
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

            //var adddata = db.CategoryPost.Include(e => e.Category).Where(e => e.Id == data).Select(e => new
            //{
            //    NoOfVacancies = e.NoOfVacancies,
            //    RelaxationAge = e.RelaxationAge,
            //    CategoryType_Id = e.Category.Id == null ? 0 : e.Category.Id,
            //    Category = e.Category
            //});

            using (DataBaseContext db = new DataBaseContext())
            {
                var adddata = db.CategoryPost
                    .Include(e => e.Category)
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        NoOfVacancies = e.NoOfVacancies,
                        RelaxationAge = e.RelaxationAge,
                        CategoryType_Id = e.Category.Id == null ? 0 : e.Category.Id,
                        Action = e.DBTrack.Action
                    }).ToList();


                var Corp = db.CategoryPost.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { adddata, Auth, JsonRequestBehavior.AllowGet });

                //  TempData["RowVersion"] = db.CategoryPost.Find(data).RowVersion;
                //  return Json(new object[] { adddata, "" }, JsonRequestBehavior.AllowGet);
                // return Json(, JsonRequestBehavior.AllowGet);
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
        public async Task<ActionResult> EditSave1(CategoryPost c, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string Category = form["CategoryPostListP"] == "0" ? "" : form["CategoryPostListP"];
                    if (Category != null && Category != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(Category));
                        c.Category = val;
                    }

                    var db_data = db.CategoryPost.Include(e => e.Category)

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
                                    db.CategoryPost.Attach(db_data);
                                    db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    TempData["RowVersion"] = db_data.RowVersion;
                                    db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;

                                    var Curr_Lookup = db.CategoryPost.Find(data);
                                    TempData["CurrRowVersion"] = Curr_Lookup.RowVersion;
                                    db.Entry(Curr_Lookup).State = System.Data.Entity.EntityState.Detached;

                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {

                                        CategoryPost blog = null; // to retrieve old data
                                        DbPropertyValues originalBlogValues = null;

                                        using (var context = new DataBaseContext())
                                        {
                                            blog = context.CategoryPost.Where(e => e.Id == data).Include(e => e.Category)

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

                                        int a = EditS(Category, data, c, c.DBTrack);

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
                                            //  var Obj = DBTrackFile.ModifiedDataHistory("Recruitement.CategoryPost", "M", blog, c, "Category", c.DBTrack);
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
                                var clientValues = (CategoryPost)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (CategoryPost)databaseEntry.ToObject();
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

                            CategoryPost blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            //CategoryPost Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.CategoryPost.Where(e => e.Id == data).SingleOrDefault();
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
                            CategoryPost Info = new CategoryPost()
                            {

                                Id = data,
                                NoOfVacancies = c.NoOfVacancies,
                                RelaxationAge = c.RelaxationAge,
                                Category = c.Category,
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
        public async Task<ActionResult> EditSave(CategoryPost c, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {


                    bool Auth = form["autho_allow"] == "true" ? true : false;

                    string Category = form["CategoryPostListP"] == "0" ? "" : form["CategoryPostListP"];
                    //if (Category != null && Category != "")
                    //{
                    //    var val = db.LookupValue.Find(int.Parse(Category));
                    //    c.Category = val;
                    //}
                    var db_data = db.CategoryPost.Include(e => e.Category)
                                                     .Where(e => e.Id == data).SingleOrDefault();



                    if (Category != null)
                    {
                        if (Category != "")
                        {
                            var val = db.LookupValue.Find(int.Parse(Category)); ;
                            db_data.Category = val;

                            var type = db.CategoryPost.Include(e => e.Category).Where(e => e.Id == data).SingleOrDefault();
                            IList<CategoryPost> typedetails = null;
                            if (type.Category != null)
                            {
                                typedetails = db.CategoryPost.Where(x => x.Category.Id == type.Category.Id && x.Id == data).ToList();
                            }
                            else
                            {
                                typedetails = db.CategoryPost.Where(x => x.Id == data).ToList();
                            }
                            //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                            foreach (var s in typedetails)
                            {
                                s.Category = db_data.Category;
                                db.CategoryPost.Attach(s);
                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                //await db.SaveChangesAsync();
                                db.SaveChanges();
                                TempData["RowVersion"] = s.RowVersion;
                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                            }
                        }
                        else
                        {
                            var WFTypeDetails = db.CategoryPost.Include(e =>e.Category).Where(x => x.Id == data).ToList();
                            foreach (var s in WFTypeDetails)
                            {
                                s.Category = null;
                                db.CategoryPost.Attach(s);
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


                        if (ModelState.IsValid)
                        {
                            try
                            {

                                //DbContextTransaction transaction = db.Database.BeginTransaction();

                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    db.CategoryPost.Attach(db_data);
                                    db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    TempData["RowVersion"] = db_data.RowVersion;
                                    db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;

                                    var Curr_Lookup = db.CategoryPost.Find(data);
                                    TempData["CurrRowVersion"] = Curr_Lookup.RowVersion;
                                    db.Entry(Curr_Lookup).State = System.Data.Entity.EntityState.Detached;

                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {

                                        CategoryPost blog = null; // to retrieve old data
                                        DbPropertyValues originalBlogValues = null;

                                        using (var context = new DataBaseContext())
                                        {
                                            blog = context.CategoryPost.Where(e => e.Id == data).Include(e => e.Category)
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
                                        CategoryPost lk = new CategoryPost
                                        {
                                            Id = data,
                                            Category = c.Category,
                                            NoOfVacancies = c.NoOfVacancies,
                                            RelaxationAge = c.RelaxationAge,
                                            DBTrack = c.DBTrack
                                        };


                                        db.CategoryPost.Attach(lk);
                                        db.Entry(lk).State = System.Data.Entity.EntityState.Modified;

                                        db.Entry(lk).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                        // int a = EditS(Corp, Addrs, ContactDetails, data, c, c.DBTrack);

                                        // int a = EditS(Values, Valueshobby, Valuesls, Valuesqd, Valuesskl, Valuesscr, data, c, c.DBTrack);


                                        using (var context = new DataBaseContext())
                                        {

                                            //    var obj = DBTrackFile.DBTrackSave("Recruitement/Recruitement", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                            //   DT_CategoryPost DT_Corp = (DT_CategoryPost)obj;

                                            //   db.Create(DT_Corp);
                                            //   db.SaveChanges();
                                        }
                                        await db.SaveChangesAsync();
                                        var categorypostdata = db.CategoryPost.Include(e => e.Category).Where(e => e.Id == data).SingleOrDefault();
                                        ts.Complete();
                                        
                                        Msg.Add("  Record Updated");
                                        return Json(new Utility.JsonReturnClass { Id = categorypostdata.Id, Val = categorypostdata.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        //return Json(new Object[] { lk.Id, "", "Record Updated", JsonRequestBehavior.AllowGet });
                                    }
                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (CategoryPost)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (CategoryPost)databaseEntry.ToObject();
                                    c.RowVersion = databaseValues.RowVersion;

                                }
                            }
                            Msg.Add("Record modified by another user.So refresh it and try to save again.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            // return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            CategoryPost blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            CategoryPost Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.CategoryPost.Include(e => e.Category).Where(e => e.Id == data).SingleOrDefault();
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
                            CategoryPost qualificationDetails = new CategoryPost()
                            {

                                Id = data,
                                Category = c.Category,
                                NoOfVacancies = c.NoOfVacancies,
                                RelaxationAge = c.RelaxationAge,
                                DBTrack = c.DBTrack
                            };

                            using (var context = new DataBaseContext())
                            {
                                //var obj = DBTrackFile.ModifiedDataHistory("Recruitement/Recruitement", "M", blog, qualificationDetails, "CategoryPost", c.DBTrack);
                                // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                //   Old_Corp = context.CategoryPost.Where(e => e.Id == data)
                                //        .Include(e => e.Category).SingleOrDefault();
                                //DT_LanguageSkill DT_Corp = (DT_LanguageSkill)obj;
                                //DT_Corp.Address_Id = DBTrackFile.ValCompare(Old_Corp.Address, c.Address);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
                                //DT_Corp.BusinessType_Id = DBTrackFile.ValCompare(Old_Corp.BusinessType, c.BusinessType); //Old_Corp.BusinessType == c.BusinessType ? 0 : Old_Corp.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_Corp.BusinessType.Id;
                                //DT_Corp.ContactDetails_Id = DBTrackFile.ValCompare(Old_Corp.ContactDetails, c.ContactDetails); //Old_Corp.ContactDetails == c.ContactDetails ? 0 : Old_Corp.ContactDetails == null && c.ContactDetails != null ? c.ContactDetails.Id : Old_Corp.ContactDetails.Id;
                                //   db.Create(DT_Corp);
                                //db.SaveChanges();
                            }
                            blog.DBTrack = c.DBTrack;
                            db.CategoryPost.Attach(blog);
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



        public int EditS(string Category, int data, CategoryPost c, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                CategoryPost CatPost = null;
                if (Category != null & Category != "")
                {
                    var val = db.LookupValue.Find(int.Parse(Category));
                    c.Category = val;

                    var type = db.CategoryPost.Include(e => e.Category).Where(e => e.Id == data).SingleOrDefault();

                    if (type.Category != null)
                    {
                        CatPost = db.CategoryPost.Where(x => x.Category.Id == type.Category.Id && x.Id == data).SingleOrDefault();
                    }
                    else
                    {
                        CatPost = db.CategoryPost.Where(x => x.Id == data).SingleOrDefault();
                    }
                    CatPost.Category = c.Category;
                }
                else
                {
                    CatPost = db.CategoryPost.Include(e => e.Category).Where(x => x.Id == data).SingleOrDefault();
                    CatPost.Category = null;
                }

                db.CategoryPost.Attach(CatPost);
                db.Entry(CatPost).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                TempData["RowVersion"] = CatPost.RowVersion;
                db.Entry(CatPost).State = System.Data.Entity.EntityState.Detached;

                var CurCorp = db.CategoryPost.Find(data);
                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;


                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    c.DBTrack = dbT;
                    CategoryPost corp = new CategoryPost()
                    {
                        Id = data,
                        NoOfVacancies = c.NoOfVacancies,
                        RelaxationAge = c.RelaxationAge,
                        DBTrack = c.DBTrack
                    };

                    db.CategoryPost.Attach(corp);
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