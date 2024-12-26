
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
using Appraisal;

namespace P2BUltimate.Controllers.Appraisal.MainController
{
    public class AppSubCategoryController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();

        public ActionResult partial()
        {
            return View("~/Views/Shared/Appraisal/_AppSubCategory.cshtml");
        }
        public ActionResult Create(AppSubCategory a, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    a.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };


                    AppSubCategory AppSubcategory = new AppSubCategory
                    {
                        Code = a.Code,
                        Name = a.Name,
                        FullDetails = a.FullDetails,
                        DBTrack = a.DBTrack
                    };
                    try
                    {
                        if (ModelState.IsValid)
                        {
                            using (TransactionScope ts = new TransactionScope())
                            {
                                db.AppSubCategory.Add(AppSubcategory);
                                db.SaveChanges();
                                ts.Complete();
                            }
                        }
                        Msg.Add("  Record Created");
                        return Json(new Utility.JsonReturnClass { Id = AppSubcategory.Id, Val = AppSubcategory.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    var qurey = db.AppSubCategory.Find(data);
                    using (TransactionScope ts = new TransactionScope())
                    {
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

        //public ActionResult edit(int? data)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var Q = db.AppSubCategory
        //            .Where(e => e.Id == data).Select
        //            (e => new
        //            {
        //                Code = e.Code,
        //                Name = e.Name,

        //                Action = e.DBTrack.Action
        //            }).ToList();
        //        var adddata = db.AppSubCategory.Where(e => e.Id == data).Select(e => new
        //        {
        //            Code = e.Code,
        //            Name = e.Name,
        //        });

        //        var W = db.DT_AppSubCategory
        //            .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
        //             (e => new
        //        {
        //            DT_Id = e.Id,
        //            Code = e.Code == null ? "" : e.Code,
        //            Name = e.Name == null ? "" : e.Name,
        //        }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

        //        var Corp = db.AppSubCategory.Find(data);
        //        TempData["RowVersion"] = Corp.RowVersion;
        //        var Auth = Corp.DBTrack.IsModified;
        //        return Json(new Object[] { Q, adddata, W, Auth, JsonRequestBehavior.AllowGet });
        //    }
        //}

        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.AppSubCategory
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        Code = e.Code,
                        Name = e.Name,
                        Action = e.DBTrack.Action
                    }).ToList();


                var Corp = db.AppSubCategory.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, "", "", Auth, JsonRequestBehavior.AllowGet });
            }
        }

        //public ActionResult Edit(int data)
        //{
        //    var Q = db.AppSubCategory.Where(e => e.Id == data).Select
        //     (e => new
        //     {
        //         SubCode = e.Code,
        //         SubName = e.Name,
        //         Action = e.DBTrack.Action
        //     }).ToList();

        //    var add_data = db.Language.Where(e => e.Id == data)
        //       .ToList();

        //    //TempData["RowVersion"] = db.IncrActivity.Find(data).RowVersion;

        //    //return Json(new Object[] { Q, add_data, JsonRequestBehavior.AllowGet });

        //    var W = db.DT_AppSubCategory
        //         .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
        //         (e => new
        //         {
        //             DT_Id = e.Id,
        //             Code = e.Code,
        //             Name = e.Name == null ? "" : e.Name,
        //         }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

        //    var Corp = db.Language.Find(data);
        //    TempData["RowVersion"] = Corp.RowVersion;
        //    var Auth = Corp.DBTrack.IsModified;
        //    return Json(new Object[] { Q, add_data, W, Auth, JsonRequestBehavior.AllowGet });



        //}
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
        public async Task<ActionResult> EditSave(int data, AppSubCategory c, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {

                    var db_data = db.AppSubCategory.Where(e => e.Id == data).SingleOrDefault();

                    bool Auth = form["Autho_Allow"] == "true" ? true : false;


                    if (Auth == false)
                    {

                        //if (ModelState.IsValid)
                        //{
                        //    using (TransactionScope ts = new TransactionScope())
                        //    {

                        //        c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                        //        //look.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.EmpId };

                        //        AppSubCategory corp = new AppSubCategory()
                        //        {
                        //            Id = data,
                        //            Code = c.Code,
                        //            Name = c.Name,
                        //            FullDetails = c.FullDetails,
                        //            DBTrack = c.DBTrack,
                        //            // RowVersion=c.RowVersion
                        //        };
                        //        try
                        //        {
                        //            db.AppSubCategory.Add(corp);
                        //            var rtn_Obj = DBTrackFile.DBTrackSave("Appraisal/Appraisal", null, db.ChangeTracker, c.DBTrack);
                        //            DT_AppSubCategory DT_OBJ = (DT_AppSubCategory)rtn_Obj;
                        //            db.Create(DT_OBJ);
                        //            db.SaveChanges();
                        //            ts.Complete();
                        //            Msg.Add("  Data Created successfully  ");
                        //            return Json(new Utility.JsonReturnClass { Id = corp.Id, Val = corp.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //            //return this.Json(new Object[] { OBJLVBP.Id, OBJLVBP.FullDetails, "Data Created Successfully.", JsonRequestBehavior.AllowGet });
                        //        }

                        //        catch (DbUpdateConcurrencyException)
                        //        {
                        //            return RedirectToAction("Create", new { concurrencyError = true, id = c.Id });
                        //        }
                        //        catch (DataException /* dex */)
                        //        {
                        //            Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
                        //            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //            //return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
                        //        }
                        //    }
                        //}


                        if (ModelState.IsValid)
                        {
                            try
                            {
                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    AppSubCategory blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.AppSubCategory.Where(e => e.Id == data).SingleOrDefault();
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
                                    var m1 = db.AppSubCategory.Where(e => e.Id == data).ToList();
                                    foreach (var s in m1)
                                    {
                                        // s.AppraisalPeriodCalendar = null;
                                        db.AppSubCategory.Attach(s);
                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                        //await db.SaveChangesAsync();
                                        db.SaveChanges();
                                        TempData["RowVersion"] = s.RowVersion;
                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    }
                                    //int a = EditS(Category, PromoPolicy, data, c, c.DBTrack);
                                    var CurCorp = db.AppSubCategory.Find(data);
                                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {

                                        AppSubCategory corp = new AppSubCategory()
                                        {
                                            Id = data,
                                            Code = c.Code,
                                            Name = c.Name,
                                            FullDetails = c.FullDetails,
                                            DBTrack = c.DBTrack,
                                            // RowVersion=c.RowVersion
                                        };

                                        db.AppSubCategory.Attach(corp);
                                        db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                                        db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                        // return 1;
                                    }

                                    using (var context = new DataBaseContext())
                                    {
                                        var obj = DBTrackFile.DBTrackSave("Appraisal/Appraisal", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                        DT_AppSubCategory DT_Corp = (DT_AppSubCategory)obj;
                                        //DT_Corp.Id = blog.Id == null ? 0 : blog.Id.Id;

                                        db.Create(DT_Corp);
                                        db.SaveChanges();
                                    }
                                    await db.SaveChangesAsync();
                                    ts.Complete();
                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = c.Id, Val = c.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    // return Json(new Object[] { c.Id, c.Name, "Record Updated", JsonRequestBehavior.AllowGet });
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

                            AppSubCategory blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            AppSubCategory Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.AppSubCategory.Where(e => e.Id == data).SingleOrDefault();
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
                            AppSubCategory corp = new AppSubCategory()
                            {
                                Id = data,
                                Code = c.Code,
                                Name = c.Name,
                                FullDetails = c.FullDetails,
                                DBTrack = c.DBTrack,
                                // RowVersion=c.RowVersion
                            };
                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Appraisal/Appraisal", "M", blog, c, "AppSubCategory", c.DBTrack);
                                // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                Old_Corp = context.AppSubCategory.Where(e => e.Id == data).SingleOrDefault();
                                DT_AppSubCategory DT_Corp = (DT_AppSubCategory)obj;
                                //DT_Corp.Code = DBTrackFile.ValCompare(Old_Corp.Code, c.Co);   need to work  on it
                                db.Create(DT_Corp);
                                //db.SaveChanges();
                            }
                            blog.DBTrack = c.DBTrack;
                            db.AppSubCategory.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Detached;
                            //DT_AppSubCategory.DBTrack = c.DBTrack;
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

        //public int EditS(int data, AppSubCategory c, DBTrack dbT)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var CurCorp = db.AppSubCategory.Find(data);
        //        TempData["CurrRowVersion"] = CurCorp.RowVersion;
        //        db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
        //        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
        //        {
        //            c.DBTrack = dbT;
        //            AppSubCategory corp = new AppSubCategory()
        //            {
        //                Id = data,
        //                Code = c.Code,
        //                Name = c.Name,
        //                FullDetails = c.FullDetails,
        //                DBTrack = c.DBTrack,
        //                // RowVersion=c.RowVersion
        //            };

        //            db.AppSubCategory.Attach(corp);
        //            db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
        //            db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //            //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
        //            return 1;
        //        }
        //        return 0;
        //    }
        //}

        public ActionResult PopulateDropDownList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var selected = (Object)null;
                SelectList s = (SelectList)null;

                if (data != "" && data != null && data != "0")
                {
                    var filter = Convert.ToInt32(data);
                    var qurey = db.AppCategory.Include(e => e.AppSubCategory).Where(e => e.Id == filter).SingleOrDefault();
                    if (data2 != "" && data2 != null && data2 != "0")
                    {
                        selected = Convert.ToInt32(data2);
                    }
                    s = new SelectList(qurey.AppSubCategory, "Id", "FullDetails", selected);
                    return Json(s, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var query = db.AppSubCategory.ToList(); 
                    if (data2 != "" && data2 != null && data2 != "0")
                    {
                        selected = Convert.ToInt32(data2);
                    }
                    s = new SelectList(query, "Id", "FullDetails", selected);
                    return Json(s, JsonRequestBehavior.AllowGet);
                }

                //<--------------- THE ELSE PART COMMENTED IS OLD ONE! NEW CREATED BY VINAYAK BCZ THERE WAS NO DATA COMING IN EDIT
                //else
                //{
                //    if (data2 != "")
                //    {
                //        selected = Convert.ToInt32(data2);
                //    }

                //    s = new SelectList(db.State, "Id", "FullDetails", selected);
                //    return Json(s, JsonRequestBehavior.AllowGet);
                //}
            }
        }
    }
}