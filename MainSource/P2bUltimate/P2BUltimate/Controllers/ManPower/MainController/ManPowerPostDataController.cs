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
using Recruitment;
using Payroll;
namespace P2BUltimate.Controllers.ManPower.MainController
{
    public class ManPowerPostDataController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();
        //
        // GET: /ManPowerPostData/
        public ActionResult Index()
        {
            return View("~/Views/ManPower/MainViews/ManPowerPostData/Index.cshtml");
        }
        [HttpPost]

        public ActionResult Create(ManPowerPostData c, FormCollection form) //Create submit
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

                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            ManPowerPostData ctc = new ManPowerPostData()
                            {
                               
                                BudgetCTC = c.BudgetCTC,
                                CurrentCTC = c.CurrentCTC,
                                ExcessCTC = c.ExcessCTC,
                                ExcessPosts = c.ExcessPosts,
                                FilledPosts = c.FilledPosts,
                                SanctionedPosts = c.SanctionedPosts,
                                TotalCTC = c.TotalCTC,
                                VacantPosts = c.VacantPosts,
                                DBTrack = c.DBTrack
                            };
                            try
                            {
                                db.ManPowerPostData.Add(ctc);
                                //   var rtn_Obj = DBTrackFile.DBTrackSave("ManPower", null, db.ChangeTracker,"");
                                //  DT_CtcDefinition DT_Corp = (DT_Corporate)rtn_Obj;

                                //   db.Create(DT_Corp);
                                db.SaveChanges();
                                //DBTrackFile.DBTrackSave("Core/P2b.Global", "C", corporate, null, "Corporate", null);


                                ts.Complete();
                                // return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);


                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = c.Id });
                            }
                            catch (DataException /* dex */)
                            {
                                // return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });

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

        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                var Q = db.ManPowerPostData

                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        BudgetCTC = e.BudgetCTC,
                        CurrentCTC = e.CurrentCTC,
                        ExcessCTC = e.ExcessCTC,
                        ExcessPosts = e.ExcessPosts,
                        FilledPosts = e.FilledPosts,
                        SanctionedPosts = e.SanctionedPosts,
                        TotalCTC = e.TotalCTC,
                        VacantPosts = e.VacantPosts,
                        //  Action = e.DBTrack.Action
                    }).ToList();

                var Corp = db.ManPowerPostData.Find(data);
                //TempData["RowVersion"] = Corp.RowVersion;
                // var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, JsonRequestBehavior.AllowGet });
            }
        }


      //  [HttpPost]
        //public async Task<ActionResult> EditSave(ManPowerPostData c, int data, FormCollection form) // Edit submit
        //{
        //    List<string> Msg = new List<string>();
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        try
        //        {
        //            bool Auth = form["autho_allow"] == "true" ? true : false;



        //            if (Auth == false)
        //            {


        //                if (ModelState.IsValid)
        //                {
        //                    try
        //                    {

        //                        //DbContextTransaction transaction = db.Database.BeginTransaction();

        //                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                        {
        //                            ManPowerPostData blog = null; // to retrieve old data
        //                            DbPropertyValues originalBlogValues = null;

        //                            using (var context = new DataBaseContext())
        //                            {
        //                                blog = context.ManPowerPostData.Where(e => e.Id == data).SingleOrDefault();
        //                                originalBlogValues = context.Entry(blog).OriginalValues;
        //                            }


        //                            //int a = EditS(salhd, data, c);
        //                            var CurCorp = db.ManPowerPostData.Find(data);
        //                            //  TempData["CurrRowVersion"] = CurCorp.RowVersion;
        //                            db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
        //                            //   if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
        //                            //  {
        //                            // c.DBTrack = dbT;
        //                            ManPowerPostData corp = new ManPowerPostData()
        //                            {
        //                                BudgetCTC = c.BudgetCTC,
        //                                CurrentCTC = c.CurrentCTC,
        //                                ExcessCTC = c.ExcessCTC,
        //                                ExcessPosts = c.ExcessPosts,
        //                                FilledPosts = c.FilledPosts,
        //                                SanctionedPosts = c.SanctionedPosts,
        //                                TotalCTC = c.TotalCTC,
        //                                VacantPosts = c.VacantPosts,
        //                                Id = data,
        //                                //    DBTrack = c.DBTrack
        //                            };


        //                            db.ManPowerPostData.Attach(corp);
        //                            db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
        //                            //db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //                            ////// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);



        //                            using (var context = new DataBaseContext())
        //                            {
        //                                //                              var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
        //                                //DT_Corporate DT_Corp = (DT_Corporate)obj;
        //                                //DT_Corp.Address_Id = blog.Address == null ? 0 : blog.Address.Id;
        //                                //DT_Corp.BusinessType_Id = blog.BusinessType == null ? 0 : blog.BusinessType.Id;
        //                                //DT_Corp.ContactDetails_Id = blog.ContactDetails == null ? 0 : blog.ContactDetails.Id;
        //                                //db.Create(DT_Corp);
        //                                db.SaveChanges();
        //                            }
        //                            db.SaveChanges();
        //                            ts.Complete();
        //                            var query = db.ManPowerPostData.Where(e => e.Id == data).SingleOrDefault();

        //                            //   return Json(new Object[] { c.Id, c.BudgetCTC, "Record Updated", JsonRequestBehavior.AllowGet });
        //                            Msg.Add("  Record Updated");
        //                            //return Json(new Utility.JsonReturnClass { Id = c.Id, Val = c.BudgetCTC.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                            return Json(new Utility.JsonReturnClass { Id = query.Id, Val = query.BudgetCTC.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                        }
        //                    }
        //                    catch (DbUpdateConcurrencyException ex)
        //                    {
        //                        var entry = ex.Entries.Single();
        //                        var clientValues = (ManPowerPostData)entry.Entity;
        //                        var databaseEntry = entry.GetDatabaseValues();
        //                        if (databaseEntry == null)
        //                        {
        //                            //   return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
        //                            Msg.Add(" Unable to save changes. The record was deleted by another user.");
        //                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                        }
        //                        else
        //                        {
        //                            var databaseValues = (ManPowerPostData)databaseEntry.ToObject();
        //                            //   c.RowVersion = databaseValues.RowVersion;

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

        //                    ManPowerPostData blog = null; // to retrieve old data
        //                    DbPropertyValues originalBlogValues = null;
        //                    ManPowerPostData Old_Corp = null;

        //                    using (var context = new DataBaseContext())
        //                    {
        //                        blog = context.ManPowerPostData.Where(e => e.Id == data).SingleOrDefault();
        //                        originalBlogValues = context.Entry(blog).OriginalValues;
        //                    }
        //                    //c.DBTrack = new DBTrack
        //                    //{
        //                    //    CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                    //    CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                    //    Action = "M",
        //                    //    IsModified = blog.DBTrack.IsModified == true ? true : false,
        //                    //    ModifiedBy = SessionManager.UserName,
        //                    //    ModifiedOn = DateTime.Now
        //                    //};

        //                    //if (TempData["RowVersion"] == null)
        //                    //{
        //                    //    TempData["RowVersion"] = blog.RowVersion;
        //                    //}

        //                    ManPowerPostData corp = new ManPowerPostData()
        //                    {
        //                        BudgetCTC = c.BudgetCTC,
        //                        CurrentCTC = c.CurrentCTC,
        //                        ExcessCTC = c.ExcessCTC,
        //                        ExcessPosts = c.ExcessPosts,
        //                        FilledPosts = c.FilledPosts,
        //                        SanctionedPosts = c.SanctionedPosts,
        //                        TotalCTC = c.TotalCTC,
        //                        VacantPosts = c.VacantPosts,
        //                        Id = data,
        //                        //   DBTrack = c.DBTrack,
        //                        //    RowVersion = (Byte[])TempData["RowVersion"]
        //                    };


        //                    //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

        //                    using (var context = new DataBaseContext())
        //                    {
        //                        //var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, corp, "Corporate", c.DBTrack);
        //                        //// var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

        //                        //Old_Corp = context.Corporate.Where(e => e.Id == data).Include(e => e.BusinessType)
        //                        //    .Include(e => e.Address).Include(e => e.ContactDetails).SingleOrDefault();
        //                        //DT_Corporate DT_Corp = (DT_Corporate)obj;
        //                        //DT_Corp.Address_Id = DBTrackFile.ValCompare(Old_Corp.Address, c.Address);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
        //                        //DT_Corp.BusinessType_Id = DBTrackFile.ValCompare(Old_Corp.BusinessType, c.BusinessType); //Old_Corp.BusinessType == c.BusinessType ? 0 : Old_Corp.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_Corp.BusinessType.Id;
        //                        //DT_Corp.ContactDetails_Id = DBTrackFile.ValCompare(Old_Corp.ContactDetails, c.ContactDetails); //Old_Corp.ContactDetails == c.ContactDetails ? 0 : Old_Corp.ContactDetails == null && c.ContactDetails != null ? c.ContactDetails.Id : Old_Corp.ContactDetails.Id;
        //                        //db.Create(DT_Corp);
        //                        //db.SaveChanges();
        //                    }
        //                    //blog.DBTrack = c.DBTrack;
        //                    //db.Corporate.Attach(blog);
        //                    db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
        //                    //   db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //                    db.SaveChanges();
        //                    ts.Complete();
        //                    // return Json(new Object[] { blog.Id, c.BudgetCTC, "Record Updated", JsonRequestBehavior.AllowGet });
        //                    Msg.Add("  Record Updated");
        //                    return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = c.BudgetCTC.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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
        //        public async Task<ActionResult> EditSave(ManPowerPostData c, int data, FormCollection form) // Edit submit
        //{
        //    List<string> Msg = new List<string>();
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        try
        //        {
        //            bool Auth = form["autho_allow"] == "true" ? true : false;



        //            if (Auth == false)
        //            {


        //                if (ModelState.IsValid)
        //                {
        //                    try
        //                    {

        //                        //DbContextTransaction transaction = db.Database.BeginTransaction();

        //                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                        {
        //                            ManPowerPostData blog = null; // to retrieve old data
        //                            DbPropertyValues originalBlogValues = null;

        //                            using (var context = new DataBaseContext())
        //                            {
        //                                blog = context.ManPowerPostData.Where(e => e.Id == data).SingleOrDefault();
        //                                originalBlogValues = context.Entry(blog).OriginalValues;
        //                            }


        //                            //int a = EditS(salhd, data, c);
        //                            var CurCorp = db.ManPowerPostData.Find(data);
        //                            //  TempData["CurrRowVersion"] = CurCorp.RowVersion;
        //                            db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
        //                            //   if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
        //                            //  {
        //                            // c.DBTrack = dbT;
        //                            ManPowerPostData corp = new ManPowerPostData()
        //                            {
        //                                BudgetCTC = c.BudgetCTC,
        //                                CurrentCTC = c.CurrentCTC,
        //                                ExcessCTC = c.ExcessCTC,
        //                                ExcessPosts = c.ExcessPosts,
        //                                FilledPosts = c.FilledPosts,
        //                                SanctionedPosts = c.SanctionedPosts,
        //                                TotalCTC = c.TotalCTC,
        //                                VacantPosts = c.VacantPosts,
        //                                Id = data,
        //                                //    DBTrack = c.DBTrack
        //                            };


        //                            db.ManPowerPostData.Attach(corp);
        //                            db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
        //                            //db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //                            ////// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);



        //                            using (var context = new DataBaseContext())
        //                            {
        //                                //                              var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
        //                                //DT_Corporate DT_Corp = (DT_Corporate)obj;
        //                                //DT_Corp.Address_Id = blog.Address == null ? 0 : blog.Address.Id;
        //                                //DT_Corp.BusinessType_Id = blog.BusinessType == null ? 0 : blog.BusinessType.Id;
        //                                //DT_Corp.ContactDetails_Id = blog.ContactDetails == null ? 0 : blog.ContactDetails.Id;
        //                                //db.Create(DT_Corp);
        //                                db.SaveChanges();
        //                            }
        //                            db.SaveChanges();
        //                            ts.Complete();
        //                            var query = db.ManPowerPostData.Where(e => e.Id == data).SingleOrDefault();

        //                            //   return Json(new Object[] { c.Id, c.BudgetCTC, "Record Updated", JsonRequestBehavior.AllowGet });
        //                            Msg.Add("  Record Updated");
        //                            //return Json(new Utility.JsonReturnClass { Id = c.Id, Val = c.BudgetCTC.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                            return Json(new Utility.JsonReturnClass { Id = query.Id, Val = query.BudgetCTC.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                        }
        //                    }
        //                    catch (DbUpdateConcurrencyException ex)
        //                    {
        //                        var entry = ex.Entries.Single();
        //                        var clientValues = (ManPowerPostData)entry.Entity;
        //                        var databaseEntry = entry.GetDatabaseValues();
        //                        if (databaseEntry == null)
        //                        {
        //                            //   return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
        //                            Msg.Add(" Unable to save changes. The record was deleted by another user.");
        //                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                        }
        //                        else
        //                        {
        //                            var databaseValues = (ManPowerPostData)databaseEntry.ToObject();
        //                            //   c.RowVersion = databaseValues.RowVersion;

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

        //                    ManPowerPostData blog = null; // to retrieve old data
        //                    DbPropertyValues originalBlogValues = null;
        //                    ManPowerPostData Old_Corp = null;

        //                    using (var context = new DataBaseContext())
        //                    {
        //                        blog = context.ManPowerPostData.Where(e => e.Id == data).SingleOrDefault();
        //                        originalBlogValues = context.Entry(blog).OriginalValues;
        //                    }
        //                    //c.DBTrack = new DBTrack
        //                    //{
        //                    //    CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                    //    CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                    //    Action = "M",
        //                    //    IsModified = blog.DBTrack.IsModified == true ? true : false,
        //                    //    ModifiedBy = SessionManager.UserName,
        //                    //    ModifiedOn = DateTime.Now
        //                    //};

        //                    //if (TempData["RowVersion"] == null)
        //                    //{
        //                    //    TempData["RowVersion"] = blog.RowVersion;
        //                    //}

        //                    ManPowerPostData corp = new ManPowerPostData()
        //                    {
        //                        BudgetCTC = c.BudgetCTC,
        //                        CurrentCTC = c.CurrentCTC,
        //                        ExcessCTC = c.ExcessCTC,
        //                        ExcessPosts = c.ExcessPosts,
        //                        FilledPosts = c.FilledPosts,
        //                        SanctionedPosts = c.SanctionedPosts,
        //                        TotalCTC = c.TotalCTC,
        //                        VacantPosts = c.VacantPosts,
        //                        Id = data,
        //                        //   DBTrack = c.DBTrack,
        //                        //    RowVersion = (Byte[])TempData["RowVersion"]
        //                    };


        //                    //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

        //                    using (var context = new DataBaseContext())
        //                    {
        //                        //var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, corp, "Corporate", c.DBTrack);
        //                        //// var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

        //                        //Old_Corp = context.Corporate.Where(e => e.Id == data).Include(e => e.BusinessType)
        //                        //    .Include(e => e.Address).Include(e => e.ContactDetails).SingleOrDefault();
        //                        //DT_Corporate DT_Corp = (DT_Corporate)obj;
        //                        //DT_Corp.Address_Id = DBTrackFile.ValCompare(Old_Corp.Address, c.Address);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
        //                        //DT_Corp.BusinessType_Id = DBTrackFile.ValCompare(Old_Corp.BusinessType, c.BusinessType); //Old_Corp.BusinessType == c.BusinessType ? 0 : Old_Corp.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_Corp.BusinessType.Id;
        //                        //DT_Corp.ContactDetails_Id = DBTrackFile.ValCompare(Old_Corp.ContactDetails, c.ContactDetails); //Old_Corp.ContactDetails == c.ContactDetails ? 0 : Old_Corp.ContactDetails == null && c.ContactDetails != null ? c.ContactDetails.Id : Old_Corp.ContactDetails.Id;
        //                        //db.Create(DT_Corp);
        //                        //db.SaveChanges();
        //                    }
        //                    //blog.DBTrack = c.DBTrack;
        //                    //db.Corporate.Attach(blog);
        //                    db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
        //                    //   db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //                    db.SaveChanges();
        //                    ts.Complete();
        //                    // return Json(new Object[] { blog.Id, c.BudgetCTC, "Record Updated", JsonRequestBehavior.AllowGet });
        //                    Msg.Add("  Record Updated");
        //                    return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = c.BudgetCTC.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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
        public async Task<ActionResult> EditSave(ManPowerPostData c, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    bool Auth = form["autho_allow"] == "true" ? true : false;



                    if (Auth == true)
                    {


                        if (ModelState.IsValid)
                        {
                            try
                            {

                                //DbContextTransaction transaction = db.Database.BeginTransaction();

                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    ManPowerPostData blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.ManPowerPostData.Where(e => e.Id == data).SingleOrDefault();
                                        originalBlogValues = context.Entry(blog).OriginalValues;
                                    }


                                    //int a = EditS(salhd, data, c);
                                    var CurCorp = db.ManPowerPostData.Find(data);
                                    //  TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                    //   if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    //  {
                                    // c.DBTrack = dbT;
                                    ManPowerPostData corp = new ManPowerPostData()
                                    {
                                        BudgetCTC = c.BudgetCTC,
                                        CurrentCTC = c.CurrentCTC,
                                        ExcessCTC = c.ExcessCTC,
                                        ExcessPosts = c.ExcessPosts,
                                        FilledPosts = c.FilledPosts,
                                        SanctionedPosts = c.SanctionedPosts,
                                        TotalCTC = c.TotalCTC,
                                        VacantPosts = c.VacantPosts,
                                        Id = data,
                                        //    DBTrack = c.DBTrack
                                    };


                                    db.ManPowerPostData.Attach(corp);
                                    db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                                    //db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                    ////// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);



                                    using (var context = new DataBaseContext())
                                    {
                                        var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                        DT_Corporate DT_Corp = (DT_Corporate)obj;
                                        //DT_Corp.Address_Id = blog.Address == null ? 0 : blog.Address.Id;
                                        //DT_Corp.BusinessType_Id = blog.BusinessType == null ? 0 : blog.BusinessType.Id;
                                        //DT_Corp.ContactDetails_Id = blog.ContactDetails == null ? 0 : blog.ContactDetails.Id;
                                        //db.Create(DT_Corp);
                                        db.SaveChanges();
                                    }
                                    db.SaveChanges();
                                    ts.Complete();
                                    var query = db.ManPowerPostData.Where(e => e.Id == data).SingleOrDefault();

                                    //   return Json(new Object[] { c.Id, c.BudgetCTC, "Record Updated", JsonRequestBehavior.AllowGet });
                                    Msg.Add("  Record Updated");
                                    //return Json(new Utility.JsonReturnClass { Id = c.Id, Val = c.BudgetCTC.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    return Json(new Utility.JsonReturnClass { Id = query.Id, Val = query.BudgetCTC.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (ManPowerPostData)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    //   return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                    Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }
                                else
                                {
                                    var databaseValues = (ManPowerPostData)databaseEntry.ToObject();
                                    //   c.RowVersion = databaseValues.RowVersion;

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

                            ManPowerPostData blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            ManPowerPostData Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.ManPowerPostData.Where(e => e.Id == data).SingleOrDefault();
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

                            //if (TempData["RowVersion"] == null)
                            //{
                            //    TempData["RowVersion"] = blog.RowVersion;
                            //}
                            var m1 = db.ManPowerPostData.Where(e => e.Id == data).ToList();
                            foreach (var s in m1)
                            {
                                // s.AppraisalPeriodCalendar = null;
                                db.ManPowerPostData.Attach(s);
                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                //await db.SaveChangesAsync();
                                db.SaveChanges();
                                TempData["RowVersion"] = s.RowVersion;
                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                            }
                            //TempData["RowVersion"] = c.RowVersion;
                            var CurCorp = db.ManPowerPostData.Find(data);
                            TempData["CurrRowVersion"] = CurCorp.RowVersion;
                            db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                            if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                            {
                                ManPowerPostData corp = new ManPowerPostData()
                                {
                                    BudgetCTC = c.BudgetCTC,
                                    CurrentCTC = c.CurrentCTC,
                                    ExcessCTC = c.ExcessCTC,
                                    ExcessPosts = c.ExcessPosts,
                                    FilledPosts = c.FilledPosts,
                                    SanctionedPosts = c.SanctionedPosts,
                                    TotalCTC = c.TotalCTC,
                                    VacantPosts = c.VacantPosts,
                                    Id = data,
                                    DBTrack = c.DBTrack,
                                    //RowVersion = (Byte[])TempData["RowVersion"]
                                };



                                db.ManPowerPostData.Attach(corp);
                                db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                                db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            }
                            using (var context = new DataBaseContext())
                            {
                                //var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, corp, "Corporate", c.DBTrack);
                                //// var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                //Old_Corp = context.Corporate.Where(e => e.Id == data).Include(e => e.BusinessType)
                                //    .Include(e => e.Address).Include(e => e.ContactDetails).SingleOrDefault();
                                //DT_Corporate DT_Corp = (DT_Corporate)obj;
                                //DT_Corp.Address_Id = DBTrackFile.ValCompare(Old_Corp.Address, c.Address);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
                                //DT_Corp.BusinessType_Id = DBTrackFile.ValCompare(Old_Corp.BusinessType, c.BusinessType); //Old_Corp.BusinessType == c.BusinessType ? 0 : Old_Corp.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_Corp.BusinessType.Id;
                                //DT_Corp.ContactDetails_Id = DBTrackFile.ValCompare(Old_Corp.ContactDetails, c.ContactDetails); //Old_Corp.ContactDetails == c.ContactDetails ? 0 : Old_Corp.ContactDetails == null && c.ContactDetails != null ? c.ContactDetails.Id : Old_Corp.ContactDetails.Id;
                                //db.Create(DT_Corp);
                                db.SaveChanges();
                            }
                            //blog.DBTrack = c.DBTrack;
                            //db.Corporate.Attach(blog);
                           // db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            //   db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                           // await db.SaveChangesAsync();
                            var query = db.ManPowerPostData.Where(e => e.Id == data).SingleOrDefault();
                            ts.Complete();
                            // return Json(new Object[] { blog.Id, c.BudgetCTC, "Record Updated", JsonRequestBehavior.AllowGet });
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = query.Id, Val = query.BudgetCTC.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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
        //public async Task<ActionResult> EditSave(ManPowerPostData c, int data, FormCollection form) // Edit submit
        //{
        //    List<string> Msg = new List<string>();
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        try
        //        {
        //            bool Auth = form["autho_allow"] == "true" ? true : false;



        //            if (Auth == false)
        //            {


        //                if (ModelState.IsValid)
        //                {
        //                    try
        //                    {

        //                        //DbContextTransaction transaction = db.Database.BeginTransaction();

        //                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                        {
        //                            ManPowerPostData blog = null; // to retrieve old data
        //                            DbPropertyValues originalBlogValues = null;

        //                            using (var context = new DataBaseContext())
        //                            {
        //                                blog = context.ManPowerPostData.Where(e => e.Id == data).SingleOrDefault();
        //                                originalBlogValues = context.Entry(blog).OriginalValues;
        //                            }


        //                            //int a = EditS(salhd, data, c);
        //                            var CurCorp = db.ManPowerPostData.Find(data);
        //                            //  TempData["CurrRowVersion"] = CurCorp.RowVersion;
        //                            db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
        //                            //   if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
        //                            //  {
        //                            // c.DBTrack = dbT;
        //                            ManPowerPostData corp = new ManPowerPostData()
        //                            {
        //                                BudgetCTC = c.BudgetCTC,
        //                                CurrentCTC = c.CurrentCTC,
        //                                ExcessCTC = c.ExcessCTC,
        //                                ExcessPosts = c.ExcessPosts,
        //                                FilledPosts = c.FilledPosts,
        //                                SanctionedPosts = c.SanctionedPosts,
        //                                TotalCTC = c.TotalCTC,
        //                                VacantPosts = c.VacantPosts,
        //                                Id = data,
        //                                //    DBTrack = c.DBTrack
        //                            };


        //                            db.ManPowerPostData.Attach(corp);
        //                            db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
        //                            //db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //                            ////// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);



        //                            using (var context = new DataBaseContext())
        //                            {
        //                                //                              var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
        //                                //DT_Corporate DT_Corp = (DT_Corporate)obj;
        //                                //DT_Corp.Address_Id = blog.Address == null ? 0 : blog.Address.Id;
        //                                //DT_Corp.BusinessType_Id = blog.BusinessType == null ? 0 : blog.BusinessType.Id;
        //                                //DT_Corp.ContactDetails_Id = blog.ContactDetails == null ? 0 : blog.ContactDetails.Id;
        //                                //db.Create(DT_Corp);
        //                                db.SaveChanges();
        //                            }
        //                            db.SaveChanges();
        //                            ts.Complete();
        //                            var query = db.ManPowerPostData.Where(e => e.Id == data).SingleOrDefault();

        //                            //   return Json(new Object[] { c.Id, c.BudgetCTC, "Record Updated", JsonRequestBehavior.AllowGet });
        //                            Msg.Add("  Record Updated");
        //                            //return Json(new Utility.JsonReturnClass { Id = c.Id, Val = c.BudgetCTC.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                            return Json(new Utility.JsonReturnClass { Id = query.Id, Val = query.BudgetCTC.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                        }
        //                    }
        //                    catch (DbUpdateConcurrencyException ex)
        //                    {
        //                        var entry = ex.Entries.Single();
        //                        var clientValues = (ManPowerPostData)entry.Entity;
        //                        var databaseEntry = entry.GetDatabaseValues();
        //                        if (databaseEntry == null)
        //                        {
        //                            //   return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
        //                            Msg.Add(" Unable to save changes. The record was deleted by another user.");
        //                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                        }
        //                        else
        //                        {
        //                            var databaseValues = (ManPowerPostData)databaseEntry.ToObject();
        //                            //   c.RowVersion = databaseValues.RowVersion;

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

        //                    ManPowerPostData blog = null; // to retrieve old data
        //                    DbPropertyValues originalBlogValues = null;
        //                    ManPowerPostData Old_Corp = null;

        //                    using (var context = new DataBaseContext())
        //                    {
        //                        blog = context.ManPowerPostData.Where(e => e.Id == data).SingleOrDefault();
        //                        originalBlogValues = context.Entry(blog).OriginalValues;
        //                    }
        //                    //c.DBTrack = new DBTrack
        //                    //{
        //                    //    CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                    //    CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                    //    Action = "M",
        //                    //    IsModified = blog.DBTrack.IsModified == true ? true : false,
        //                    //    ModifiedBy = SessionManager.UserName,
        //                    //    ModifiedOn = DateTime.Now
        //                    //};

        //                    //if (TempData["RowVersion"] == null)
        //                    //{
        //                    //    TempData["RowVersion"] = blog.RowVersion;
        //                    //}

        //                    ManPowerPostData corp = new ManPowerPostData()
        //                    {
        //                        BudgetCTC = c.BudgetCTC,
        //                        CurrentCTC = c.CurrentCTC,
        //                        ExcessCTC = c.ExcessCTC,
        //                        ExcessPosts = c.ExcessPosts,
        //                        FilledPosts = c.FilledPosts,
        //                        SanctionedPosts = c.SanctionedPosts,
        //                        TotalCTC = c.TotalCTC,
        //                        VacantPosts = c.VacantPosts,
        //                        Id = data,
        //                        //   DBTrack = c.DBTrack,
        //                        //    RowVersion = (Byte[])TempData["RowVersion"]
        //                    };


        //                    //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

        //                    using (var context = new DataBaseContext())
        //                    {
        //                        //var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, corp, "Corporate", c.DBTrack);
        //                        //// var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

        //                        //Old_Corp = context.Corporate.Where(e => e.Id == data).Include(e => e.BusinessType)
        //                        //    .Include(e => e.Address).Include(e => e.ContactDetails).SingleOrDefault();
        //                        //DT_Corporate DT_Corp = (DT_Corporate)obj;
        //                        //DT_Corp.Address_Id = DBTrackFile.ValCompare(Old_Corp.Address, c.Address);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
        //                        //DT_Corp.BusinessType_Id = DBTrackFile.ValCompare(Old_Corp.BusinessType, c.BusinessType); //Old_Corp.BusinessType == c.BusinessType ? 0 : Old_Corp.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_Corp.BusinessType.Id;
        //                        //DT_Corp.ContactDetails_Id = DBTrackFile.ValCompare(Old_Corp.ContactDetails, c.ContactDetails); //Old_Corp.ContactDetails == c.ContactDetails ? 0 : Old_Corp.ContactDetails == null && c.ContactDetails != null ? c.ContactDetails.Id : Old_Corp.ContactDetails.Id;
        //                        //db.Create(DT_Corp);
        //                        //db.SaveChanges();
        //                    }
        //                    //blog.DBTrack = c.DBTrack;
        //                    //db.Corporate.Attach(blog);
        //                    db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
        //                    //   db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //                    db.SaveChanges();
        //                    ts.Complete();
        //                    // return Json(new Object[] { blog.Id, c.BudgetCTC, "Record Updated", JsonRequestBehavior.AllowGet });
        //                    Msg.Add("  Record Updated");
        //                    return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = c.BudgetCTC.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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
                IEnumerable<ManPowerPostData> ManPowerPostData = null;
                if (gp.IsAutho == true)
                {
                    ManPowerPostData = db.ManPowerPostData.ToList();
                }
                else
                {
                    ManPowerPostData = db.ManPowerPostData.AsNoTracking().ToList();
                }

                IEnumerable<ManPowerPostData> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = ManPowerPostData;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.Id.ToString().Contains(gp.searchString))
                                  || (e.BudgetCTC.ToString().Contains(gp.searchString))
                                  || (e.CurrentCTC.ToString().Contains(gp.searchString))
                                  || (e.ExcessCTC.ToString().Contains(gp.searchString))
                                  || (e.ExcessPosts.ToString().Contains(gp.searchString))
                         || (e.FilledPosts.ToString().Contains(gp.searchString))
                         || (e.SanctionedPosts.ToString().Contains(gp.searchString))
                         || (e.VacantPosts.ToString().Contains(gp.searchString))
                         || (e.TotalCTC.ToString().Contains(gp.searchString))
                     ).Select(a => new Object[] { Convert.ToString(a.Id), Convert.ToString(a.BudgetCTC), Convert.ToString(a.CurrentCTC), Convert.ToString(a.ExcessCTC), Convert.ToString(a.ExcessPosts), Convert.ToString(a.FilledPosts), Convert.ToString(a.SanctionedPosts), Convert.ToString(a.VacantPosts), Convert.ToString(a.TotalCTC) }).Where(a => a.Contains((gp.searchString))).ToList();
                    }


                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.BudgetCTC, a.CurrentCTC, a.ExcessCTC, a.ExcessPosts, a.FilledPosts, a.SanctionedPosts, a.VacantPosts, a.TotalCTC }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = ManPowerPostData;
                    Func<ManPowerPostData, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "BudgetCTC" ? c.BudgetCTC.ToString() :
                                         gp.sidx == "CurrentCTC" ? c.CurrentCTC.ToString() :
                                            gp.sidx == "ExcessCTC" ? c.CurrentCTC.ToString() :
                                               gp.sidx == "ExcessPosts" ? c.CurrentCTC.ToString() :
                                                  gp.sidx == "FilledPosts" ? c.CurrentCTC.ToString() :
                                                     gp.sidx == "SanctionedPosts" ? c.CurrentCTC.ToString() :
                                                        gp.sidx == "VacantPosts" ? c.CurrentCTC.ToString() :
                                                           gp.sidx == "TotalCTC" ? c.CurrentCTC.ToString() :
                                         "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.BudgetCTC), Convert.ToString(a.CurrentCTC), Convert.ToString(a.ExcessCTC), Convert.ToString(a.ExcessPosts), Convert.ToString(a.FilledPosts), Convert.ToString(a.SanctionedPosts), Convert.ToString(a.VacantPosts), Convert.ToString(a.TotalCTC) }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.BudgetCTC), Convert.ToString(a.CurrentCTC), Convert.ToString(a.ExcessCTC), Convert.ToString(a.ExcessPosts), Convert.ToString(a.FilledPosts), Convert.ToString(a.SanctionedPosts), Convert.ToString(a.VacantPosts), Convert.ToString(a.TotalCTC) }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, Convert.ToString(a.BudgetCTC), Convert.ToString(a.CurrentCTC), Convert.ToString(a.ExcessCTC), Convert.ToString(a.ExcessPosts), Convert.ToString(a.FilledPosts), Convert.ToString(a.SanctionedPosts), Convert.ToString(a.VacantPosts), Convert.ToString(a.TotalCTC) }).ToList();
                    }
                    totalRecords = ManPowerPostData.Count();
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

        //   [HttpPost]
        //public ActionResult Edit(int data)
        //{
        //    //string tableName = "Corporate";

        //    //    // Fetch the table records dynamically
        //    //    var tableData = db.GetType()
        //    //    .GetProperty(tableName)
        //    //    .GetValue(db, null);
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var Q = db.AppraisalPublish
        //            .Include(e => e.AppraisalPeriodCalendar)
        //            .Where(e => e.Id == data).Select
        //            (p => new
        //            {
        //                AppraisalPeriodCalendar_Id = p.AppraisalPeriodCalendar != null ? p.AppraisalPeriodCalendar.Id : 0,
        //                PublishDate = p.PublishDate,
        //                SpanPeriod = p.SpanPeriod == null ? 0 : p.SpanPeriod,
        //                Extension = p.Extension == null ? 0 : p.Extension,
        //                IsTrClose = p.IsTrClose,
        //                Id = p.Id,
        //                Action = p.DBTrack.Action
        //            }).ToList();

        //        var Corp = db.AppraisalPublish.Find(data);
        //        TempData["RowVersion"] = Corp.RowVersion;
        //        var Auth = Corp.DBTrack.IsModified;
        //        return Json(new Object[] { Q, "", "", Auth, JsonRequestBehavior.AllowGet });
        //    }
        //}

        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    ManPowerPostData corporates = db.ManPowerPostData.Where(e => e.Id == data).SingleOrDefault();


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
                            var rtn_Obj = DBTrackFile.DBTrackSave("Appraisal/Appraisal", null, db.ChangeTracker, corporates.DBTrack);
                            //   DT_AppraisalPublish DT_Corp = (DT_AppraisalPublish)rtn_Obj;
                            //  DT_Corp.AppraisalPeriodCalendar_Id = corporates.AppraisalPeriodCalendar == null ? 0 : corporates.AppraisalPeriodCalendar.Id;
                            //  db.Create(DT_Corp);
                            await db.SaveChangesAsync();
                            ts.Complete();
                            //Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            Msg.Add("  Data removed.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {

                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
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
                            // var rtn_Obj = DBTrackFile.DBTrackSave("Appraisal/Appraisal", null, db.ChangeTracker, dbT);
                            // DT_Corp = (DT_AppraisalPublish)rtn_Obj;
                            //   DT_Corp.AppraisalPeriodCalendar_Id = corporates.AppraisalPeriodCalendar == null ? 0 : corporates.AppraisalPeriodCalendar.Id;
                            // db.Create(DT_Corp);

                            await db.SaveChangesAsync();
                            ts.Complete();
                            Msg.Add("  Data removed.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
                catch (RetryLimitExceededException /* dex */)
                {
                    Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
}