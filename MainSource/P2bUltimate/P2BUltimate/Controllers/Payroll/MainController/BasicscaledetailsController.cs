///
/// Created by Tanushri
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
namespace P2BUltimate.Controllers.Payroll.MainController
{
    [AuthoriseManger]
    public class BasicScaleDetailsController : Controller
    {
        // private DataBaseContext db = new DataBaseContext();

        public ActionResult partial()
        {
            return View("~/Views/Shared/Payroll/_BasicScaleDetails.cshtml");
        }

        public ActionResult Create(BasicScaleDetails OBJ)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    if (OBJ.StartingSlab >= OBJ.EndingSlab)
                    {
                        Msg.Add(" Ending Slab Must be greater than Starting Slab. ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        // return this.Json(new Object[] { null, null, "Ending Slab Must be greater than Starting Slab.", JsonRequestBehavior.AllowGet });
                    }

                    // if (db.BasicScaleDetails.Any(o => o.StartingSlab == OBJ.StartingSlab) && db.BasicScaleDetails.Any(o => o.EndingSlab == OBJ.EndingSlab))
                    if (db.BasicScaleDetails.Where(o => o.StartingSlab == OBJ.StartingSlab && o.IncrementAmount == OBJ.IncrementAmount && o.EndingSlab == OBJ.EndingSlab).Count() > 0)
                    {
                        //return this.Json(new { msg = "Code already exists." });
                        Msg.Add("  Slab Already Exists.  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        // return this.Json(new Object[] { null, null, "Slab already exists.", JsonRequestBehavior.AllowGet });

                    }
                    OBJ.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                    BasicScaleDetails BasicScaleDetails = new BasicScaleDetails
                    {
                        StartingSlab = OBJ.StartingSlab,
                        EndingSlab = OBJ.EndingSlab,
                        IncrementCount = OBJ.IncrementCount,
                        IncrementAmount = OBJ.IncrementAmount,
                        EBMark = OBJ.EBMark,
                        DBTrack = OBJ.DBTrack
                    };
                    try
                    {
                        if (ModelState.IsValid)
                        {
                            using (TransactionScope ts = new TransactionScope())
                            {
                                db.BasicScaleDetails.Add(BasicScaleDetails);
                                var a = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, OBJ.DBTrack);
                                DT_BasicScaleDetails DT_LKVal = (DT_BasicScaleDetails)a;
                                db.Create(DT_LKVal);

                                ////   DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, OBJ.DBTrack);
                                db.SaveChanges();
                                //  DBTrackFile.DBTrackSave("Payroll/Payroll", "C", BasicScaleDetails, null, "BasicScaleDetails", null);
                                ts.Complete();
                                //   return this.Json(new Object[] { BasicScaleDetails.Id, BasicScaleDetails.FullDetails, "Record Created", JsonRequestBehavior.AllowGet });
                                Msg.Add("  Data Created successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = BasicScaleDetails.Id, Val = BasicScaleDetails.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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

                            // return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
                        }

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
                    var qurey = db.BasicScaleDetails.Find(data);
                    using (TransactionScope ts = new TransactionScope())
                    {
                        //db.BasicScaleDetails.Attach(qurey);
                        //db.Entry(qurey).State = System.Data.Entity.EntityState.Deleted;
                        //db.SaveChanges();
                        //db.Entry(qurey).State = System.Data.Entity.EntityState.Detached;
                        //ts.Complete();

                        DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };
                        db.Entry(qurey).State = System.Data.Entity.EntityState.Deleted;
                        /////  DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, dbT);
                        db.SaveChanges();
                        ts.Complete();
                        //   return this.Json(new { msg = "Data removed.", JsonRequestBehavior.AllowGet });
                        Msg.Add(" Data removed  ");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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
        //    var qurey = db.BasicScaleDetails
        //        .Where(e => e.Id == data).Select
        //        (e => new
        //        {

        //            StartingSlab = e.StartingSlab,
        //            IncrementAmount = e.IncrementAmount,
        //            IncrementCount = e.IncrementCount,
        //            EndingSlab = e.EndingSlab,
        //            EBMark = e.EBMark,
        //            Action = e.DBTrack.Action
        //        }).ToList();
        //    TempData["RowVersion"] = db.BasicScaleDetails.Find(data).RowVersion;
        //    return Json(qurey, JsonRequestBehavior.AllowGet);
        //}

        [HttpPost]
        public ActionResult Edit(int data)
        {
            //string tableName = "Corporate";

            //    // Fetch the table records dynamically
            //    var tableData = db.GetType()
            //    .GetProperty(tableName)
            //    .GetValue(db, null);
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.BasicScaleDetails
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        StartingSlab = e.StartingSlab,
                        IncrementAmount = e.IncrementAmount,
                        IncrementCount = e.IncrementCount,
                        EndingSlab = e.EndingSlab,
                        EBMark = e.EBMark,
                        Action = e.DBTrack.Action
                    }).ToList();

                var add_data = db.BasicScaleDetails

                    .Where(e => e.Id == data)
                    .Select(e => new
                    {
                        id = e.Id.ToString(),
                        FullDetails = e.FullDetails
                    }).ToList();


                var W = db.DT_BasicScaleDetails
                     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id,
                         StartingSlab = e.StartingSlab,
                         IncrementAmount = e.IncrementAmount,
                         IncrementCount = e.IncrementCount,
                         EndingSlab = e.EndingSlab,
                         EBMark = e.EBMark,
                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var Corp = db.BasicScaleDetails.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, add_data, W, Auth, JsonRequestBehavior.AllowGet });
            }
        }

        public async Task<ActionResult> EditSave(BasicScaleDetails ESOBJ, int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {

                    try
                    {
                        if (ModelState.IsValid)
                        {
                            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {
                                //db.BasicScaleDetails.Attach(OBJ);
                                //db.Entry(OBJ).State = System.Data.Entity.EntityState.Modified;
                                //db.SaveChanges();
                                //db.Entry(OBJ).State = System.Data.Entity.EntityState.Detached;
                                //ts.Complete();
                                var Curr_OBJ = db.BasicScaleDetails.Find(data);
                                TempData["CurrRowVersion"] = Curr_OBJ.RowVersion;
                                db.Entry(Curr_OBJ).State = System.Data.Entity.EntityState.Detached;
                                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                {
                                    BasicScaleDetails blog = blog = null;
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.BasicScaleDetails.Where(e => e.Id == data).SingleOrDefault();
                                        originalBlogValues = context.Entry(blog).OriginalValues;
                                    }

                                    ESOBJ.DBTrack = new DBTrack
                                    {
                                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };
                                    BasicScaleDetails OBJ = new BasicScaleDetails
                                    {
                                        Id = data,
                                        StartingSlab = ESOBJ.StartingSlab,
                                        EndingSlab = ESOBJ.EndingSlab,
                                        IncrementCount = ESOBJ.IncrementCount,
                                        IncrementAmount = ESOBJ.IncrementAmount,
                                        EBMark = ESOBJ.EBMark,
                                        DBTrack = ESOBJ.DBTrack
                                    };


                                    db.BasicScaleDetails.Attach(OBJ);
                                    db.Entry(OBJ).State = System.Data.Entity.EntityState.Modified;

                                    // db.Entry(c.DBTrack).State = System.Data.Entity.EntityState.Modified;
                                    //db.SaveChanges();
                                    db.Entry(OBJ).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                    ////   DBTrackFile.DBTrackSave("Payroll/Payroll", originalBlogValues, db.ChangeTracker, ESOBJ.DBTrack);
                                    await db.SaveChangesAsync();
                                    //DisplayTrackedEntities(db.ChangeTracker);
                                    db.Entry(OBJ).State = System.Data.Entity.EntityState.Detached;
                                    ts.Complete();
                                    //  return Json(new Object[] { OBJ.Id, OBJ.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });
                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = OBJ.Id, Val = OBJ.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);


                                }
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
                catch (DbUpdateConcurrencyException e) { throw e; }
                catch (DbUpdateException e) { throw e; }
            }

        }


        //public ActionResult Partial()
        //   {
        //       return View("~/Views/Shared/_BasicScaleDetails.cshtml");
        //   }
    }
}




//        [HttpPost]
//        public async Task<ActionResult> EditSave(BasicScaleDetails NOBJ, int data, FormCollection form)
//        {

//            if (ModelState.IsValid)
//            {
//                try
//                {

//                    //DbContextTransaction transaction = db.Database.BeginTransaction();

//                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
//                    {
//                        // db.Configuration.AutoDetectChangesEnabled = false;

//                        var BasicScaleD = db.BasicScaleDetails.Find(data);
//                        TempData["CurrRowVersion"] = BasicScaleD.RowVersion;
//                        db.Entry(BasicScaleD).State = System.Data.Entity.EntityState.Detached;
//                        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
//                        {
//                            BasicScaleDetails blog = blog = null;
//                            DbPropertyValues originalBlogValues = null;

//                            using (var context = new DataBaseContext())
//                            {
//                                blog = context.BasicScaleDetails.Where(e => e.Id == data).SingleOrDefault();
//                                originalBlogValues = context.Entry(blog).OriginalValues;
//                            }

//                            NOBJ.DBTrack = new DBTrack
//                            {
//                                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
//                                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
//                                Action = "M",
//                                ModifiedBy = SessionManager.UserName,
//                                ModifiedOn = DateTime.Now
//                            };
//                            BasicScaleDetails BasicScaleDtl = new BasicScaleDetails()
//                            {
//                                EndingSlab = NOBJ.EndingSlab,
//                                StartingSlab = NOBJ.StartingSlab,
//                                IncrementAmount = NOBJ.IncrementAmount,
//                                IncrementCount = NOBJ.IncrementCount,
//                                EBMark = NOBJ.EBMark,
//                                Id = data,
//                                DBTrack = NOBJ.DBTrack
//                            };
//                            db.BasicScaleDetails.Attach(BasicScaleDtl);
//                            db.Entry(BasicScaleDtl).State = System.Data.Entity.EntityState.Modified;
//                            // db.Entry(c.DBTrack).State = System.Data.Entity.EntityState.Modified;
//                            //db.SaveChanges();
//                            db.Entry(BasicScaleDtl).OriginalValues["RowVersion"] = TempData["RowVersion"];
//                            // DBTrackFile.DBTrackSave("Payroll/Payroll", originalBlogValues, db.ChangeTracker, NOBJ.DBTrack);
//                            await db.SaveChangesAsync();
//                            //DisplayTrackedEntities(db.ChangeTracker);
//                            db.Entry(BasicScaleDtl).State = System.Data.Entity.EntityState.Detached;
//                            ts.Complete();
//                            return Json(new Object[] { BasicScaleDtl.Id, BasicScaleDtl.StartingSlab, "Record Updated", JsonRequestBehavior.AllowGet });
//                        }
//                    }
//                }
//                catch (DbUpdateConcurrencyException ex)
//                {
//                    var entry = ex.Entries.Single();
//                    var clientValues = (BasicScaleDetails)entry.Entity;
//                    var databaseEntry = entry.GetDatabaseValues();
//                    if (databaseEntry == null)
//                    {
//                        return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
//                    }
//                    else
//                    {
//                        var databaseValues = (BasicScaleDetails)databaseEntry.ToObject();
//                        NOBJ.RowVersion = databaseValues.RowVersion;
//                    }
//                }
//                return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
//            }
//            return View();
//        }


//        public ActionResult Create(BasicScaleDetails NOBJ, FormCollection form)
//        {
//            if (ModelState.IsValid)
//            {
//                using (TransactionScope ts = new TransactionScope())
//                {
//                    if (db.BasicScaleDetails.Any(o => o.Id == NOBJ.Id))
//                    {
//                        return Json(new Object[] { "", "", "Code Already Exists.", JsonRequestBehavior.AllowGet });
//                    }

//                    NOBJ.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName };
//                    BasicScaleDetails BASICSCALED = new BasicScaleDetails()
//                    {
//                        EndingSlab = NOBJ.EndingSlab,
//                        StartingSlab = NOBJ.StartingSlab,
//                        IncrementAmount = NOBJ.IncrementAmount,
//                        IncrementCount = NOBJ.IncrementCount,
//                        EBMark = NOBJ.EBMark,
//                        DBTrack = NOBJ.DBTrack
//                    };
//                    try
//                    {
//                        db.BasicScaleDetails.Add(BASICSCALED);
//                       //  DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, td.DBTrack);
//                        db.SaveChanges();
//                        ts.Complete();
//                        //return Json(new Object[] { BasicScaleDetails.Id, BasicScaleDetails.LookupVal, "Record Created", JsonRequestBehavior.AllowGet });
//                        return Json(new Object[] { BASICSCALED.Id, BASICSCALED.StartingSlab, "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
//                    }
//                    catch (DbUpdateConcurrencyException)
//                    {
//                        return RedirectToAction("Create", new { concurrencyError = true, id = NOBJ.Id });
//                    }
//                    catch (DataException /* dex */)
//                    {
//                        return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
//                    }
//                }
//            }
//            else
//            {
//                StringBuilder sb = new StringBuilder("");
//                foreach (ModelState modelState in ModelState.Values)
//                {
//                    foreach (ModelError error in modelState.Errors)
//                    {
//                        sb.Append(error.ErrorMessage);
//                        sb.Append("." + "\n");
//                    }
//                }
//                var errorMsg = sb.ToString();
//                return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
//                //return this.Json(new { msg = errorMsg });
//            }
//        }

//        public ActionResult edit(int? data)
//        {

//            var qurey = db.BasicScaleDetails.Where(e => e.Id == data).ToList();
//            TempData["RowVersion"] = db.BasicScaleDetails.Find(data).RowVersion;
//            return Json(qurey, JsonRequestBehavior.AllowGet);
//        }
//        //[HttpPost]
//        //public ActionResult Edit(int data) 
//        //{
//        //    var Q = db.BasicScaleDetails              
//        //        .Where(e => e.Id == data).Select
//        //        (e => new
//        //        {
//        //            EBMark = e.EBMark,
//        //            EndingSlab = e.EndingSlab,
//        //            IncrementAmount = e.IncrementAmount,
//        //            IncrementCount = e.IncrementCount,
//        //            StartingSlab = e.StartingSlab, 
//        //        }).ToList();

//        //    var add_data = db.BasicScaleDetails             
//        //        .ToList();
//        //    TempData["RowVersion"] = db.BasicScaleDetails.Find(data).RowVersion;
//        //    return Json(new Object[] { Q, add_data, JsonRequestBehavior.AllowGet });
//        //}




//        //[HttpPost]
//        //// GET: /BasicScaleDetails/Edit/5
//        //public ActionResult Edit(BasicScaleDetails OBJ, int data)
//        //{
//        //    //BasicScaleDetails c = db.BasicScaleDetails.Find(data);

//        //    if (ModelState.IsValid)
//        //    {
//        //        BasicScaleDetails BasicScaleDetails = new BasicScaleDetails()
//        //        {
//        //            StartingSlab = OBJ.StartingSlab,
//        //            IncrementAmount = OBJ.IncrementAmount,
//        //            IncrementCount = OBJ.IncrementCount,
//        //            EndingSlab = OBJ.EndingSlab,
//        //            EBMark = OBJ.EBMark,
//        //            Id = data
//        //        };

//        //        db.Entry(OBJ).State = System.Data.Entity.EntityState.Detached;
//        //        try
//        //        {
//        //            using (TransactionScope ts = new TransactionScope())
//        //            {
//        //                db.Entry(BasicScaleDetails).State = System.Data.Entity.EntityState.Modified;
//        //                db.SaveChanges();
//        //                ts.Complete();
//        //            }

//        //            return Json(new Object[]{null,null ,"Data saved successfully." });
//        //        }
//        //        catch (DbUpdateConcurrencyException)
//        //        {
//        //            return RedirectToAction("Edit", new { concurrencyError = true, id = OBJ.Id });
//        //        }
//        //        catch (DataException /* dex */)
//        //        {
//        //            return this.Json(new { msg = "Unable to edit. Try again, and if the problem persists contact your system administrator." });
//        //            //Log the error (uncomment dex variable name after DataException and add a line here to write a log.
//        //            //ModelState.AddModelError(string.Empty, "Unable to edit. Try again, and if the problem persists contact your system administrator.");
//        //            //return View(OBJ);
//        //        }
//        //    }
//        //    return View(OBJ);
//        //}





//        //[HttpPost]
//        //[ValidateAntiForgeryToken]
//        //public ActionResult Delete(int? data)
//        //{
//        //    BasicScaleDetails BasicScaleDetails = db.BasicScaleDetails.Find(data);
//        //    try
//        //    {
//        //        if (data != null)
//        //        {
//        //            using (TransactionScope ts = new TransactionScope())
//        //            {
//        //                db.Entry(BasicScaleDetails).State = System.Data.Entity.EntityState.Deleted;
//        //                db.SaveChanges();
//        //                ts.Complete();
//        //            }

//        //            return this.Json(new { msg = "Data deleted." });
//        //        }
//        //            else
//        //        {
//        //            return this.Json(new { msg = "Data not deleted." });
//        //        }

//        //    }

//        //    catch (DataException /* dex */)
//        //    {
//        //        //Log the error (uncomment dex variable name after DataException and add a line here to write a log.
//        //        return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists contact your system administrator." });
//        //        //ModelState.AddModelError(string.Empty, "Unable to delete. Try again, and if the problem persists contact your system administrator.");
//        //        //return RedirectToAction("Index");
//        //    }
//        //}


//        //[HttpPost]
//        //public ActionResult Delete(int data)
//        //{

//        //    BasicScaleDetails BasicScaleD = db.BasicScaleDetails.Find(data);
//        //    try
//        //    {
//        //        DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };
//        //        db.Entry(BasicScaleD).State = System.Data.Entity.EntityState.Deleted;
//        //         DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, dbT);
//        //        db.SaveChanges();
//        //        return this.Json(new { msg = "Data removed.", JsonRequestBehavior.AllowGet });               
//        //    }
//        //    catch (DbUpdateConcurrencyException)
//        //    {
//        //        return RedirectToAction("Delete", new { concurrencyError = true, id = data });
//        //    }
//        //    catch (RetryLimitExceededException /* dex */)
//        //    {
//        //        return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
//        //    }
//        //}

//        public ActionResult delete(int? data)
//        {
//            var qurey = db.BasicScaleDetails.Find(data);
//            using (TransactionScope ts = new TransactionScope())
//            {
//                //db.BasicScaleDetails.Attach(qurey);
//                //db.Entry(qurey).State = System.Data.Entity.EntityState.Deleted;
//                //db.SaveChanges();
//                //db.Entry(qurey).State = System.Data.Entity.EntityState.Detached;
//                //ts.Complete();

//                DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };
//                db.Entry(qurey).State = System.Data.Entity.EntityState.Deleted;
//                 DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, dbT);
//                db.SaveChanges();
//                ts.Complete();
//                return this.Json(new { msg = "Data removed.", JsonRequestBehavior.AllowGet });
//            }
//        }


//    }
//}


