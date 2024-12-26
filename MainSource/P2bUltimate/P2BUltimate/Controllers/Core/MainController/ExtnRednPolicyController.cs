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
    public class ExtnRednPolicyController : Controller
    {

        //private DataBaseContext db = new DataBaseContext();

        //
        // GET: /PromoPolicy/
        public ActionResult Index()
        {
            return View("~/Views/Core/MainViews/ExtnRednPolicy/Index.cshtml");
        }


        public ActionResult CreateExtnRednPolicy_partial()
        {
            return View("~/Views/Shared/Core/_ExtnRednPolicy.cshtml");
        }

        [HttpPost]
        public ActionResult Create(ExtnRednPolicy c, FormCollection form) //Create submit
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
                            string ExtnRednCauseType_drop = form["ExtnRednCauseType_drop"] == "0" ? "" : form["ExtnRednCauseType_drop"];
                            string ExtnRednPeriodUnit_drop = form["ExtnRednPeriodUnit_drop"] == "0" ? "" : form["ExtnRednPeriodUnit_drop"];
                            if (db.ExtnRednPolicy.Any(o => o.Name == c.Name))
                            {
                                // return Json(new Object[] { "", "", "Name Already Exists.", JsonRequestBehavior.AllowGet });
                                Msg.Add("  Code Already Exists.  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }
                            if (ExtnRednCauseType_drop != null && ExtnRednCauseType_drop != "")
                            {
                                var val = db.LookupValue.Find(int.Parse(ExtnRednCauseType_drop));
                                c.ExtnRednCauseType = val;
                            }
                            if (ExtnRednPeriodUnit_drop != null && ExtnRednPeriodUnit_drop != "")
                            {
                                var val = db.LookupValue.Find(int.Parse(ExtnRednPeriodUnit_drop));
                                c.ExtnRednPeriodUnit = val;
                            }


                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            ExtnRednPolicy ExtnRednPolicy = new ExtnRednPolicy()
                            {
                                IsExtn = c.IsExtn,
                                IsRedn = c.IsRedn,
                                ExtnRednPeriodUnit = c.ExtnRednPeriodUnit,
                                Name = c.Name,
                                ExtnRednPeriod = c.ExtnRednPeriod,
                                MaxCount = c.MaxCount,
                                ExtnRednCauseType=c.ExtnRednCauseType,
                                DBTrack = c.DBTrack
                            };
                            try
                            {
                                db.ExtnRednPolicy.Add(ExtnRednPolicy);
                                var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, c.DBTrack);
                                DT_ExtnRednPolicy DT_Corp = (DT_ExtnRednPolicy)rtn_Obj;
                                db.Create(DT_Corp);
                                db.SaveChanges();
                                ts.Complete();
                                // return this.Json(new Object[] { ExtnRednPolicy.Id, ExtnRednPolicy.FullDetails, "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = ExtnRednPolicy.Id, Val = ExtnRednPolicy.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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
                var Q = db.ExtnRednPolicy
                     .Include(e => e.ExtnRednCauseType)
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        IsExtn = e.IsExtn,
                        IsRedn = e.IsRedn,
                        ExtnRednPeriodUnit = e.ExtnRednPeriodUnit.Id == null ? 0 : e.ExtnRednPeriodUnit.Id,
                        Name = e.Name,
                        ExtnRednPeriod = e.ExtnRednPeriod,
                        MaxCount = e.MaxCount,
                        ExtnRednCauseType = e.ExtnRednCauseType.Id == null ? 0 : e.ExtnRednCauseType.Id,
                        Action = e.DBTrack.Action
                    }).ToList();

                //var add_data = db.PromoPolicy.Include(e => e.IncrActivity)
                //               .Where(e => e.Id == data).Select
                //               (e => new 
                //               {
                //                    IncrActivity_FullDetails = e.IncrActivity.FullDetails == null ? " " : e.IncrActivity.FullDetails, 
                //                    IncrActivity_Id = e.IncrActivity.Id == null ? "" : e.IncrActivity.Id.ToString(),               

                //               }).ToList();                                                                                                                                   

                var W = db.DT_ExtnRednPolicy
                     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id,
                         IsExtn = e.IsExtn,
                         IsRedn = e.IsRedn,
                         //IsDaysPeriod = e.IsDaysPeriod,
                         //IsMonthsPeriod = e.IsMonthsPeriod,
                         //IsYearsPeriod = e.IsYearsPeriod,
                         Name = e.Name,
                         ExtnRednPeriod = e.ExtnRednPeriod,
                       
                         MaxCount = e.MaxCount,
                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var Corp = db.ExtnRednPolicy.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, "", W, Auth, JsonRequestBehavior.AllowGet });
            }
        }


        [HttpPost]
        public async Task<ActionResult> EditSave(ExtnRednPolicy c, int data, FormCollection form) // Edit submit 
        {
            List<string> Msg = new List<string>();
            string ExtnRednCauseType_drop = form["ExtnRednCauseType_drop"] == "0" ? "" : form["ExtnRednCauseType_drop"];
            string ExtnRednPeriodUnit_drop = form["ExtnRednPeriodUnit_drop"] == "0" ? "" : form["ExtnRednPeriodUnit_drop"];
            bool Auth = form["Autho_Allow"] == "true" ? true : false;

            c.ExtnRednCauseType_Id = ExtnRednCauseType_drop != null && ExtnRednCauseType_drop != "" ? int.Parse(ExtnRednCauseType_drop) : 0;
            c.ExtnRednPeriodUnit_Id = ExtnRednPeriodUnit_drop != null && ExtnRednPeriodUnit_drop != "" ? int.Parse(ExtnRednPeriodUnit_drop) : 0;
           



            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
 
                        ExtnRednPolicy ExtnRednPolicy = db.ExtnRednPolicy.Find(data);
                        TempData["CurrRowVersion"] = ExtnRednPolicy.RowVersion;
                        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                        {
                            c.DBTrack = new DBTrack
                            {
                                CreatedBy = ExtnRednPolicy.DBTrack.CreatedBy == null ? null : ExtnRednPolicy.DBTrack.CreatedBy,
                                CreatedOn = ExtnRednPolicy.DBTrack.CreatedOn == null ? null : ExtnRednPolicy.DBTrack.CreatedOn,
                                Action = "M",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };
                            ExtnRednPolicy.Name = c.Name;
                            ExtnRednPolicy.ExtnRednCauseType_Id = c.ExtnRednCauseType_Id;
                            ExtnRednPolicy.ExtnRednPeriodUnit_Id = c.ExtnRednPeriodUnit_Id;
                            ExtnRednPolicy.IsExtn = c.IsExtn;
                            ExtnRednPolicy.IsRedn = c.IsRedn;
                            ExtnRednPolicy.ExtnRednPeriod = c.ExtnRednPeriod;
                            ExtnRednPolicy.MaxCount = c.MaxCount; 
                            ExtnRednPolicy.Id = data;
                            ExtnRednPolicy.DBTrack = c.DBTrack; 
                            db.Entry(ExtnRednPolicy).State = System.Data.Entity.EntityState.Modified;

                            ExtnRednPolicy blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;

                            blog = db.ExtnRednPolicy.Where(e => e.Id == data).Include(e => e.ExtnRednCauseType)
                                                    .Include(e => e.ExtnRednPeriodUnit).SingleOrDefault();
                            originalBlogValues = db.Entry(blog).OriginalValues;
                            db.ChangeTracker.DetectChanges();
                            var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                            DT_ExtnRednPolicy DT_Corp = (DT_ExtnRednPolicy)obj;
                            DT_Corp.ExtnRednCauseType_Id = blog.ExtnRednCauseType == null ? 0 : blog.ExtnRednCauseType.Id;
                            DT_Corp.ExtnRednPeriodUnit_Id = blog.ExtnRednPeriodUnit == null ? 0 : blog.ExtnRednPeriodUnit.Id;
                             
                            db.Create(DT_Corp);
                            db.SaveChanges();
                        }
                        //}
                        ts.Complete();
                    }
                    Msg.Add("  Record Updated");
                    return Json(new Utility.JsonReturnClass { Id = c.Id, Val = c.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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

            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        }

        //[HttpPost]
        //public async Task<ActionResult> EditSave(ExtnRednPolicy c, int data, FormCollection form) // Edit submit
        //{ 
        //    List<string> Msg = new List<string>();
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        try
        //        {
        //            bool Auth = form["Autho_Allow"] == "true" ? true : false;
        //            string ExtnRednCauseType_drop = form["ExtnRednCauseType_drop"] == "0" ? "" : form["ExtnRednCauseType_drop"];
        //            string ExtnRednPeriodUnit_drop = form["ExtnRednPeriodUnit_drop"] == "0" ? "" : form["ExtnRednPeriodUnit_drop"];

        //            if (Auth == false)
        //            {


        //                if (ModelState.IsValid)
        //                {
        //                    try
        //                    {
        //                        //DbContextTransaction transaction = db.Database.BeginTransaction();

                               

        //                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                        {
        //                            ExtnRednPolicy blog = null; // to retrieve old data
        //                            DbPropertyValues originalBlogValues = null;

        //                            using (var context = new DataBaseContext())
        //                            {
        //                                blog = context.ExtnRednPolicy.Where(e => e.Id == data).SingleOrDefault();
        //                                originalBlogValues = context.Entry(blog).OriginalValues;
        //                            }


        //                            c.DBTrack = new DBTrack
        //                            {
        //                                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                                Action = "M",
        //                                ModifiedBy = SessionManager.UserName,
        //                                ModifiedOn = DateTime.Now
        //                            };
        //                            if (ExtnRednCauseType_drop != null && ExtnRednCauseType_drop != "")
        //                            {
        //                                var val = db.LookupValue.Find(int.Parse(ExtnRednCauseType_drop));
        //                                c.ExtnRednCauseType = val;

        //                                var type = db.ExtnRednPolicy.Include(e => e.ExtnRednCauseType).Where(e => e.Id == data).FirstOrDefault();
        //                                ExtnRednPolicy typedetails = null;
        //                                if (type.ExtnRednCauseType != null)
        //                                {
        //                                    typedetails = db.ExtnRednPolicy.Where(x => x.ExtnRednCauseType.Id == type.ExtnRednCauseType.Id && x.Id == data).FirstOrDefault();
        //                                }
        //                                else
        //                                {
        //                                    typedetails = db.ExtnRednPolicy.Where(x => x.Id == data).FirstOrDefault();
        //                                }

        //                                typedetails.ExtnRednCauseType = c.ExtnRednCauseType;
        //                                db.ExtnRednPolicy.Attach(typedetails);
        //                                db.Entry(typedetails).State = System.Data.Entity.EntityState.Modified;
        //                                //await db.SaveChangesAsync();
        //                                db.SaveChanges();
        //                                TempData["RowVersion"] = typedetails.RowVersion;
        //                                db.Entry(typedetails).State = System.Data.Entity.EntityState.Detached;

        //                            }
        //                            if (ExtnRednPeriodUnit_drop != null && ExtnRednPeriodUnit_drop != "")
        //                            {
        //                                var val = db.LookupValue.Find(int.Parse(ExtnRednPeriodUnit_drop));
        //                                c.ExtnRednPeriodUnit = val;

        //                                var type = db.ExtnRednPolicy.Include(e => e.ExtnRednPeriodUnit).Where(e => e.Id == data).FirstOrDefault();
        //                                ExtnRednPolicy typedetails = null;
        //                                if (type.ExtnRednPeriodUnit != null)
        //                                {
        //                                    typedetails = db.ExtnRednPolicy.Where(x => x.ExtnRednPeriodUnit.Id == type.ExtnRednPeriodUnit.Id && x.Id == data).FirstOrDefault();
        //                                }
        //                                else
        //                                {
        //                                    typedetails = db.ExtnRednPolicy.Where(x => x.Id == data).FirstOrDefault();
        //                                }

        //                                typedetails.ExtnRednPeriodUnit = c.ExtnRednPeriodUnit;
        //                                db.ExtnRednPolicy.Attach(typedetails);
        //                                db.Entry(typedetails).State = System.Data.Entity.EntityState.Modified;
        //                                //await db.SaveChangesAsync();
        //                                db.SaveChanges();
        //                                TempData["RowVersion"] = typedetails.RowVersion;
        //                                db.Entry(typedetails).State = System.Data.Entity.EntityState.Detached;

        //                            }

        //                            var CurCorp = db.ExtnRednPolicy.Find(data);
        //                            TempData["CurrRowVersion"] = CurCorp.RowVersion;
        //                            db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
        //                            if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
        //                            {
        //                                //c.DBTrack = dbT;
        //                                ExtnRednPolicy ExtnRednPolicy = new ExtnRednPolicy()
        //                                {
        //                                    IsExtn = c.IsExtn,
        //                                    IsRedn = c.IsRedn,
        //                                    //IsDaysPeriod = c.IsDaysPeriod,
        //                                    //IsMonthsPeriod = c.IsMonthsPeriod,
        //                                    //IsYearsPeriod = c.IsYearsPeriod,
        //                                    Name = c.Name,
        //                                    ExtnRednPeriod = c.ExtnRednPeriod,
        //                                    MaxCount = c.MaxCount,
        //                                    //ExtnRednCauseType=c.ExtnRednCauseType,
        //                                    //ExtnRednPeriodUnit = c.ExtnRednPeriodUnit,
        //                                    Id = data,
        //                                    DBTrack = c.DBTrack
        //                                };

        //                                db.ExtnRednPolicy.Attach(ExtnRednPolicy);
        //                                db.Entry(ExtnRednPolicy).State = System.Data.Entity.EntityState.Modified;
        //                                db.Entry(ExtnRednPolicy).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //                                //return 1;
        //                            }
        //                            //int a = EditS(data, c, c.DBTrack);
        //                            using (var context = new DataBaseContext())
        //                            {
        //                                var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
        //                                DT_ExtnRednPolicy DT_ExtnRednPolicy = (DT_ExtnRednPolicy)obj;
        //                                db.Create(DT_ExtnRednPolicy);
        //                                db.SaveChanges();
        //                            }
        //                            await db.SaveChangesAsync();
        //                            ts.Complete();
        //                            // return Json(new Object[] { data, c.Name, "Record Updated", JsonRequestBehavior.AllowGet });
        //                            Msg.Add("  Record Updated");
        //                            return Json(new Utility.JsonReturnClass { Id = data, Val = c.Name, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                        }
        //                    }
        //                    catch (DbUpdateConcurrencyException ex)
        //                    {
        //                        var entry = ex.Entries.Single();
        //                        var clientValues = (PromoPolicy)entry.Entity;
        //                        var databaseEntry = entry.GetDatabaseValues();
        //                        if (databaseEntry == null)
        //                        {
        //                            Msg.Add(" Unable to save changes. The record was deleted by another user.");
        //                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                            //  return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
        //                        }
        //                        else
        //                        {
        //                            var databaseValues = (PromoPolicy)databaseEntry.ToObject();
        //                            c.RowVersion = databaseValues.RowVersion;

        //                        }
        //                    }

        //                    //   return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
        //                    Msg.Add("Record modified by another user.So refresh it and try to save again.");
        //                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                }
        //            }
        //            else
        //            {
        //                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                {

        //                    ExtnRednPolicy blog = null; // to retrieve old data
        //                    DbPropertyValues originalBlogValues = null;
        //                    ExtnRednPolicy Old_ExtnRednPolicy = null;

        //                    using (var context = new DataBaseContext())
        //                    {
        //                        blog = context.ExtnRednPolicy.Where(e => e.Id == data).SingleOrDefault();
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
        //                    ExtnRednPolicy ExtnRednPolicy = new ExtnRednPolicy()
        //                    {
        //                        IsExtn = c.IsExtn,
        //                        IsRedn = c.IsRedn,
        //                        //IsDaysPeriod = c.IsDaysPeriod,
        //                        //IsMonthsPeriod = c.IsMonthsPeriod,
        //                        //IsYearsPeriod = c.IsYearsPeriod,
        //                        Name = c.Name,
        //                        ExtnRednPeriod = c.ExtnRednPeriod,
        //                        ExtnRednCauseType = c.ExtnRednCauseType,
        //                        ExtnRednPeriodUnit = c.ExtnRednPeriodUnit,
        //                        MaxCount = c.MaxCount,
        //                        Id = data,
        //                        DBTrack = c.DBTrack,
        //                        RowVersion = (Byte[])TempData["RowVersion"]
        //                    };



        //                    using (var context = new DataBaseContext())
        //                    {
        //                        var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, ExtnRednPolicy, "ExtnRednPolicy", c.DBTrack);
        //                        DT_ExtnRednPolicy DT_ExtnRednPolicy = (DT_ExtnRednPolicy)obj;
        //                        db.Create(DT_ExtnRednPolicy);
        //                    }
        //                    blog.DBTrack = c.DBTrack;
        //                    db.ExtnRednPolicy.Attach(blog);
        //                    db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
        //                    db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //                    db.SaveChanges();
        //                    ts.Complete();
        //                    // return Json(new Object[] { blog.Id, c.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });
        //                    Msg.Add("  Record Updated");
        //                    return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = c.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                }

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
                            ExtnRednPolicy ExtnRednPolicy = db.ExtnRednPolicy.FirstOrDefault(e => e.Id == auth_id);
                            ExtnRednPolicy.DBTrack = new DBTrack
                            {
                                Action = "C",
                                ModifiedBy = ExtnRednPolicy.DBTrack.ModifiedBy != null ? ExtnRednPolicy.DBTrack.ModifiedBy : null,
                                CreatedBy = ExtnRednPolicy.DBTrack.CreatedBy != null ? ExtnRednPolicy.DBTrack.CreatedBy : null,
                                CreatedOn = ExtnRednPolicy.DBTrack.CreatedOn != null ? ExtnRednPolicy.DBTrack.CreatedOn : null,
                                IsModified = ExtnRednPolicy.DBTrack.IsModified == true ? false : false,
                                AuthorizedBy = SessionManager.UserName,
                                AuthorizedOn = DateTime.Now
                            };

                            db.ExtnRednPolicy.Attach(ExtnRednPolicy);
                            db.Entry(ExtnRednPolicy).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(ExtnRednPolicy).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, ExtnRednPolicy.DBTrack);
                            DT_PromoPolicy DT_Corp = (DT_PromoPolicy)rtn_Obj;
                            db.Create(DT_Corp);
                            await db.SaveChangesAsync();

                            ts.Complete();
                            // return Json(new Object[] { ExtnRednPolicy.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                            Msg.Add("  Record Authorised");
                            return Json(new Utility.JsonReturnClass { Id = ExtnRednPolicy.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }
                    }
                    else if (auth_action == "M")
                    {

                        ExtnRednPolicy Old_ExtnRednPolicy = db.ExtnRednPolicy.Where(e => e.Id == auth_id).SingleOrDefault();


                        DT_ExtnRednPolicy Curr_ExtnRednPolicy = db.DT_ExtnRednPolicy
                                                    .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                                    .OrderByDescending(e => e.Id)
                                                    .FirstOrDefault();

                        if (Curr_ExtnRednPolicy != null)
                        {
                            ExtnRednPolicy ExtnRednPolicy = new ExtnRednPolicy();
                            ExtnRednPolicy.Name = Curr_ExtnRednPolicy.Name == null ? Old_ExtnRednPolicy.Name : Old_ExtnRednPolicy.Name;
                            ExtnRednPolicy.ExtnRednPeriod = Curr_ExtnRednPolicy.ExtnRednPeriod == null ? Old_ExtnRednPolicy.ExtnRednPeriod : Old_ExtnRednPolicy.ExtnRednPeriod;
                            ExtnRednPolicy.MaxCount = Curr_ExtnRednPolicy.MaxCount == null ? Old_ExtnRednPolicy.MaxCount : Old_ExtnRednPolicy.MaxCount;
                            ExtnRednPolicy.IsExtn = Curr_ExtnRednPolicy.IsExtn;
                            ExtnRednPolicy.IsRedn = Curr_ExtnRednPolicy.IsRedn;
                            //ExtnRednPolicy.IsDaysPeriod = Curr_ExtnRednPolicy.IsDaysPeriod;
                            //ExtnRednPolicy.IsMonthsPeriod = Curr_ExtnRednPolicy.IsMonthsPeriod;
                            //ExtnRednPolicy.IsYearsPeriod = Curr_ExtnRednPolicy.IsYearsPeriod;
                           

                            if (ModelState.IsValid)
                            {
                                try
                                {
                                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                    {
                                        ExtnRednPolicy.DBTrack = new DBTrack
                                        {
                                            CreatedBy = Old_ExtnRednPolicy.DBTrack.CreatedBy == null ? null : Old_ExtnRednPolicy.DBTrack.CreatedBy,
                                            CreatedOn = Old_ExtnRednPolicy.DBTrack.CreatedOn == null ? null : Old_ExtnRednPolicy.DBTrack.CreatedOn,
                                            Action = "M",
                                            ModifiedBy = Old_ExtnRednPolicy.DBTrack.ModifiedBy == null ? null : Old_ExtnRednPolicy.DBTrack.ModifiedBy,
                                            ModifiedOn = Old_ExtnRednPolicy.DBTrack.ModifiedOn == null ? null : Old_ExtnRednPolicy.DBTrack.ModifiedOn,
                                            AuthorizedBy = SessionManager.UserName,
                                            AuthorizedOn = DateTime.Now,
                                            IsModified = false
                                        };

                                        int a = EditS(auth_id, ExtnRednPolicy, ExtnRednPolicy.DBTrack);
                                        await db.SaveChangesAsync();
                                        ts.Complete();
                                        // return Json(new Object[] { ExtnRednPolicy.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                                        Msg.Add("  Record Authorised");
                                        return Json(new Utility.JsonReturnClass { Id = ExtnRednPolicy.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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
                                        ExtnRednPolicy.RowVersion = databaseValues.RowVersion;
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
                            ExtnRednPolicy corp = db.ExtnRednPolicy.AsNoTracking().FirstOrDefault(e => e.Id == auth_id);

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

                            db.ExtnRednPolicy.Attach(corp);
                            db.Entry(corp).State = System.Data.Entity.EntityState.Deleted;


                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corp.DBTrack);
                            DT_ExtnRednPolicy DT_Corp = (DT_ExtnRednPolicy)rtn_Obj;
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
                IEnumerable<ExtnRednPolicy> ExtnRednPolicy = null;
                if (gp.IsAutho == true)
                {
                    ExtnRednPolicy = db.ExtnRednPolicy.Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    ExtnRednPolicy = db.ExtnRednPolicy.AsNoTracking().ToList();
                }

                IEnumerable<ExtnRednPolicy> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = ExtnRednPolicy;
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
                    IE = ExtnRednPolicy;
                    Func<ExtnRednPolicy, dynamic> orderfuc;
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
                    totalRecords = ExtnRednPolicy.Count();
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

                    ExtnRednPolicy ExtnRednPolicy = db.ExtnRednPolicy.Where(e => e.Id == data).SingleOrDefault();
                    if (ExtnRednPolicy.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = ExtnRednPolicy.DBTrack.CreatedBy != null ? ExtnRednPolicy.DBTrack.CreatedBy : null,
                                CreatedOn = ExtnRednPolicy.DBTrack.CreatedOn != null ? ExtnRednPolicy.DBTrack.CreatedOn : null,
                                IsModified = ExtnRednPolicy.DBTrack.IsModified == true ? true : false
                            };
                            ExtnRednPolicy.DBTrack = dbT;
                            db.Entry(ExtnRednPolicy).State = System.Data.Entity.EntityState.Modified;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, ExtnRednPolicy.DBTrack);
                            DT_ExtnRednPolicy DT_Corp = (DT_ExtnRednPolicy)rtn_Obj;
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
                                    CreatedBy = ExtnRednPolicy.DBTrack.CreatedBy != null ? ExtnRednPolicy.DBTrack.CreatedBy : null,
                                    CreatedOn = ExtnRednPolicy.DBTrack.CreatedOn != null ? ExtnRednPolicy.DBTrack.CreatedOn : null,
                                    IsModified = ExtnRednPolicy.DBTrack.IsModified == true ? false : false//,
                                };

                                db.Entry(ExtnRednPolicy).State = System.Data.Entity.EntityState.Deleted;
                                var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dbT);
                                DT_ExtnRednPolicy DT_Corp = (DT_ExtnRednPolicy)rtn_Obj;
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
        public int EditS(int data, ExtnRednPolicy c, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var CurCorp = db.ExtnRednPolicy.Find(data);
                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    c.DBTrack = dbT;
                    ExtnRednPolicy ExtnRednPolicy = new ExtnRednPolicy()
                    {
                        IsExtn = c.IsExtn,
                        IsRedn = c.IsRedn,
                        //IsDaysPeriod = c.IsDaysPeriod,
                        //IsMonthsPeriod = c.IsMonthsPeriod,
                        //IsYearsPeriod = c.IsYearsPeriod,
                        Name = c.Name,
                        ExtnRednPeriod = c.ExtnRednPeriod,
                        MaxCount = c.MaxCount,
                        Id = data,
                        DBTrack = c.DBTrack
                    };

                    db.ExtnRednPolicy.Attach(ExtnRednPolicy);
                    db.Entry(ExtnRednPolicy).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(ExtnRednPolicy).OriginalValues["RowVersion"] = TempData["RowVersion"];
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

        public ActionResult GetLookup_ExtnRednPolicy(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.ExtnRednPolicy.ToList();
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.ExtnRednPolicy.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
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