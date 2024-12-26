///
/// Created by Tanushri
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
using Payroll;
using P2BUltimate.Security;




namespace P2BUltimate.Controllers.Core.MainController
{
     [AuthoriseManger]
    public class MedicineController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();
        //
        // GET: /Medicine/
        public ActionResult Index()
        {

            return View();
        }



        public ActionResult Partial()
        {
            return View("~/Views/Shared/Core/_Medicine.cshtml");
        }


        [HttpPost]
        public async Task<ActionResult> EditSave(Medicine NOBJ, int data, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        try
                        {
                            //DbContextTransaction transaction = db.Database.BeginTransaction();

                            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {
                                // db.Configuration.AutoDetectChangesEnabled = false;

                                var MedicineD = db.Medicine.Find(data);
                                TempData["CurrRowVersion"] = MedicineD.RowVersion;
                                db.Entry(MedicineD).State = System.Data.Entity.EntityState.Detached;
                                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                {
                                    Medicine blog = blog = null;
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.Medicine.Where(e => e.Id == data).SingleOrDefault();
                                        originalBlogValues = context.Entry(blog).OriginalValues;
                                    }

                                    NOBJ.DBTrack = new DBTrack
                                    {
                                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };
                                    Medicine MedicineDtl = new Medicine()
                                    {
                                        Name = NOBJ.Name,
                                        MedPower = NOBJ.MedPower,
                                        Manufacturer = NOBJ.Manufacturer,
                                        Id = data,
                                        DBTrack = NOBJ.DBTrack
                                    };
                                    db.Medicine.Attach(MedicineDtl);
                                    db.Entry(MedicineDtl).State = System.Data.Entity.EntityState.Modified;
                                    // db.Entry(c.DBTrack).State = System.Data.Entity.EntityState.Modified;
                                    //db.SaveChanges();
                                    db.Entry(MedicineDtl).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                    //DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, NOBJ.DBTrack);
                                    await db.SaveChangesAsync();
                                    //DisplayTrackedEntities(db.ChangeTracker);
                                    db.Entry(MedicineDtl).State = System.Data.Entity.EntityState.Detached;
                                    ts.Complete();
                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = MedicineDtl.Id, Val = MedicineDtl.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    //return Json(new Object[] { MedicineDtl.Id, MedicineDtl.MedPower, "Record Updated", JsonRequestBehavior.AllowGet });
                                }
                            }
                        }
                        catch (DbUpdateConcurrencyException ex)
                        {
                            var entry = ex.Entries.Single();
                            var clientValues = (Medicine)entry.Entity;
                            var databaseEntry = entry.GetDatabaseValues();
                            if (databaseEntry == null)
                            {
                                Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                // return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                            }
                            else
                            {
                                var databaseValues = (Medicine)databaseEntry.ToObject();
                                NOBJ.RowVersion = databaseValues.RowVersion;
                            }
                        }
                        Msg.Add("Record modified by another user.So refresh it and try to save again.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        // return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
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


        public ActionResult Create(Medicine NOBJ, FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            if (db.Medicine.Any(o => o.Name == NOBJ.Name))
                            {
                                Msg.Add("  Name Already Exists.  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                // return Json(new Object[] { "", "", "Name Already Exists.", JsonRequestBehavior.AllowGet });
                            }
                            if (db.Medicine.Any(o => o.Name == NOBJ.Name) && db.Medicine.Any(o => o.MedPower == NOBJ.MedPower) && db.Medicine.Any(o => o.Manufacturer == NOBJ.Manufacturer))
                            {
                                Msg.Add("  Record Already Exists.  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                // return Json(new Object[] { "", "", "REcord Already Exists.", JsonRequestBehavior.AllowGet });
                            }
                            NOBJ.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName };
                            Medicine MedicineD = new Medicine()
                            {
                                Name = NOBJ.Name,
                                MedPower = NOBJ.MedPower,
                                Manufacturer = NOBJ.Manufacturer,
                                DBTrack = NOBJ.DBTrack
                            };
                            try
                            {
                                db.Medicine.Add(MedicineD);
                                // DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, td.DBTrack);
                                db.SaveChanges();
                                ts.Complete();
                                Msg.Add(" Data Saved Successfully.");
                                return Json(new Utility.JsonReturnClass { Id = MedicineD.Id, Val = MedicineD.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Object[] { MedicineD.Id, MedicineD.FullDetails, "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                                //return Json(new Object[] { LookupValue.Id, LookupValue.LookupVal, "Record Created", JsonRequestBehavior.AllowGet });
                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = NOBJ.Id });
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
                var Q = db.Medicine
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        //   Name = e.Name,
                        MedicalName = e.Name,
                        Manufacturer = e.Manufacturer,
                        MedPower = e.MedPower,
                    }).ToList();

                //var add_data = db.Medicine
                //    .ToList();
                TempData["RowVersion"] = db.Medicine.Find(data).RowVersion;
                return Json(new Object[] { Q, JsonRequestBehavior.AllowGet });
            }
        }




        //[HttpPost]
        //// GET: /Medicine/Edit/5
        //public ActionResult Edit(Medicine OBJ, int data)
        //{
        //    //Medicine c = db.Medicine.Find(data);

        //    if (ModelState.IsValid)
        //    {
        //        Medicine Medicine = new Medicine()
        //        {
        //            MedPower = OBJ.MedPower,
        //            Manufacturer = OBJ.Manufacturer,
        //            IncrementCount = OBJ.IncrementCount,
        //            EndingSlab = OBJ.EndingSlab,
        //            EBMark = OBJ.EBMark,
        //            Id = data
        //        };

        //        db.Entry(OBJ).State = System.Data.Entity.EntityState.Detached;
        //        try
        //        {
        //            using (TransactionScope ts = new TransactionScope())
        //            {
        //                db.Entry(Medicine).State = System.Data.Entity.EntityState.Modified;
        //                db.SaveChanges();
        //                ts.Complete();
        //            }

        //            return Json(new Object[]{null,null ,"Data saved successfully." });
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            return RedirectToAction("Edit", new { concurrencyError = true, id = OBJ.Id });
        //        }
        //        catch (DataException /* dex */)
        //        {
        //            return this.Json(new { msg = "Unable to edit. Try again, and if the problem persists contact your system administrator." });
        //            //Log the error (uncomment dex variable name after DataException and add a line here to write a log.
        //            //ModelState.AddModelError(string.Empty, "Unable to edit. Try again, and if the problem persists contact your system administrator.");
        //            //return View(OBJ);
        //        }
        //    }
        //    return View(OBJ);
        //}





        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Delete(int? data)
        //{
        //    Medicine Medicine = db.Medicine.Find(data);
        //    try
        //    {
        //        if (data != null)
        //        {
        //            using (TransactionScope ts = new TransactionScope())
        //            {
        //                db.Entry(Medicine).State = System.Data.Entity.EntityState.Deleted;
        //                db.SaveChanges();
        //                ts.Complete();
        //            }

        //            return this.Json(new { msg = "Data deleted." });
        //        }
        //            else
        //        {
        //            return this.Json(new { msg = "Data not deleted." });
        //        }

        //    }

        //    catch (DataException /* dex */)
        //    {
        //        //Log the error (uncomment dex variable name after DataException and add a line here to write a log.
        //        return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists contact your system administrator." });
        //        //ModelState.AddModelError(string.Empty, "Unable to delete. Try again, and if the problem persists contact your system administrator.");
        //        //return RedirectToAction("Index");
        //    }
        //}


        //[HttpPost]
        //public ActionResult Delete(int data)
        //{

        //    Medicine MedicineD = db.Medicine.Find(data);
        //    try
        //    {
        //        DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };
        //        db.Entry(MedicineD).State = System.Data.Entity.EntityState.Deleted;
        //         DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dbT);
        //        db.SaveChanges();
        //        return this.Json(new { msg = "Data removed.", JsonRequestBehavior.AllowGet });               
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        return RedirectToAction("Delete", new { concurrencyError = true, id = data });
        //    }
        //    catch (RetryLimitExceededException /* dex */)
        //    {
        //        return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
        //    }
        //}

        public ActionResult delete(int? data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    var qurey = db.Medicine.Find(data);
                    using (TransactionScope ts = new TransactionScope())
                    {
                        //db.LookupValue.Attach(qurey);
                        //db.Entry(qurey).State = System.Data.Entity.EntityState.Deleted;
                        //db.SaveChanges();
                        //db.Entry(qurey).State = System.Data.Entity.EntityState.Detached;
                        //ts.Complete();

                        DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };
                        db.Entry(qurey).State = System.Data.Entity.EntityState.Deleted;
                        DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dbT);
                        db.SaveChanges();
                        ts.Complete();
                        Msg.Add("  Data removed successfully.  ");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        //return this.Json(new { msg = "Data removed.", JsonRequestBehavior.AllowGet });
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
}


