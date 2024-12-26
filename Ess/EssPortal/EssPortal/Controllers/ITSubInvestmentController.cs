using P2b.Global;
using Payroll;
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
using EssPortal.App_Start;
using EssPortal.Models;
using EssPortal.Security;
using System.Threading.Tasks;

namespace P2BUltimate.Controllers.Payroll.MainController
{
    [AuthoriseManger]
    public class ITSubInvestmentController : Controller
    {
       // private DataBaseContext db = new DataBaseContext();
        //
        // GET: /ITSubInvestment/
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult itsubinvestment_partial()
        {
            return View("~/Views/Shared/Payroll/_itsubinvestment.cshtml");
        }

        

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult Create(ITSubInvestment OBJ, FormCollection form)
        { 
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var itsubinvest_id = form[""];
                    if (ModelState.IsValid)
                    {
                        OBJ.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                        ITSubInvestment ITSubInvestment = new ITSubInvestment()
                        {
                            SubInvestmentName = OBJ.SubInvestmentName == null ? "" : OBJ.SubInvestmentName.Trim(),
                            DBTrack = OBJ.DBTrack
                        };
                        try
                        {
                            using (TransactionScope ts = new TransactionScope())
                            {
                                if (db.ITSubInvestment.Any(o => o.SubInvestmentName.ToLower() == OBJ.SubInvestmentName.ToLower().Trim()))
                                {
                                    //  return Json(new Object[]{null,null,"Investment already exists."});
                                    Msg.Add("Investment Already Exists.  ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }
                                db.ITSubInvestment.Add(ITSubInvestment);
                                db.SaveChanges();
                                ts.Complete();
                            }
                            //  return Json(new Object[]{ITSubInvestment.Id,ITSubInvestment.FullDetails, "Data saved successfully." });
                            //Msg.Add("  Data Saved successfully  ");
                            return Json(new { status = true, responseText = "Data Saved successfully." }, JsonRequestBehavior.AllowGet);
                           
                           // return Json(new Utility.JsonReturnClass { Id = ITSubInvestment.Id, Val = ITSubInvestment.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            return RedirectToAction("Create", new { concurrencyError = true, id = OBJ.Id });
                        }
                        catch (DataException /* dex */)
                        {
                            //  return this.Json(new { msg = "Unable to edit. Try again, and if the problem persists contact your system administrator." });
                            Msg.Add(" Unable to edit.Try again, and if the problem persists, see your system administrator");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

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
                        return this.Json(new { msg = errorMsg });
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

        public ActionResult GetLookupDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.ITSubInvestment.ToList();
                IEnumerable<ITSubInvestment> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.ITSubInvestment.ToList().Where(d => d.SubInvestmentName.Contains(data));

                }
                else
                {
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.SubInvestmentName }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.SubInvestmentName }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var r = db.ITSubInvestment.Where(e => e.Id == data).Select(e => new { ITSubInvestment_Id = e.Id, ITSubInvestment_Name = e.SubInvestmentName }).ToList();
                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        public async Task<ActionResult> EditSave(ITSubInvestment ITSub, int data, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
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
                                    ITSubInvestment blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.ITSubInvestment.Where(e => e.Id == data).SingleOrDefault();
                                        originalBlogValues = context.Entry(blog).OriginalValues;
                                    }

                                    ITSub.DBTrack = new DBTrack
                                    {
                                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };

                                    //int a = EditS(data, ITSub, ITSub.DBTrack);

                                    var CurInvest = db.ITSubInvestment.Find(data);
                                    TempData["CurrRowVersion"] = CurInvest.RowVersion;
                                    db.Entry(CurInvest).State = System.Data.Entity.EntityState.Detached;
                                    //if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    //{
                                    //ITSub.DBTrack = ITSub.DBTrack;
                                    ITSubInvestment ITSubInvest = new ITSubInvestment()
                                    {
                                        SubInvestmentName = ITSub.SubInvestmentName,
                                        Id = data,
                                        DBTrack = ITSub.DBTrack
                                    };


                                    db.ITSubInvestment.Attach(ITSubInvest);
                                    db.Entry(ITSubInvest).State = System.Data.Entity.EntityState.Modified;
                                    db.Entry(ITSubInvest).OriginalValues["RowVersion"] = TempData["RowVersion"];

                                    using (var context = new DataBaseContext())
                                    {

                                        var obj = DBTrackFile.DBTrackSave("Payroll/Payroll", originalBlogValues, db.ChangeTracker, ITSub.DBTrack);
                                        //DT_ITSubInvestment DT_ITSubInvestment = (DT_ITSubInvestment)obj;
                                        //db.Create(DT_ITSubInvestment);
                                        db.SaveChanges();
                                    }
                                    await db.SaveChangesAsync();
                                    var dat = db.ITSubInvestment.Where(q => q.Id == data).SingleOrDefault();
                                    ts.Complete();

                                    //   return Json(new Object[] { data, ITSub.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });
                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = data, Val = dat.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (ITSubInvestment)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    // return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                    Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }
                                else
                                {
                                    var databaseValues = (ITSubInvestment)databaseEntry.ToObject();
                                    ITSub.RowVersion = databaseValues.RowVersion;

                                }
                            }


                            //  return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                            Msg.Add("Record modified by another user.So refresh it and try to save again.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);


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
                            return Json(new Utility.JsonReturnClass { Id = ITSub.Id, Val = ITSub.SubInvestmentName, success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            // return this.Json(new Object[] { ITSub.Id, ITSub.SubInvestmentName, errorMsg, JsonRequestBehavior.AllowGet });
                            //return this.Json(new { msg = errorMsg });
                        }
                    }
                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            ITSubInvestment blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            ITSubInvestment Old_ITSub = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.ITSubInvestment.Where(e => e.Id == data).SingleOrDefault();
                                originalBlogValues = context.Entry(blog).OriginalValues;
                            }
                            ITSub.DBTrack = new DBTrack
                            {
                                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                Action = "M",
                                IsModified = blog.DBTrack.IsModified == true ? true : false,
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };

                            if (TempData["RowVersion"] == null)
                            {
                                TempData["RowVersion"] = blog.RowVersion;
                            }

                            ITSubInvestment ITSubInvest = new ITSubInvestment()
                            {
                                SubInvestmentName = ITSub.SubInvestmentName,
                                Id = data,
                                DBTrack = ITSub.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };


                            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Payroll/Payroll", "M", blog, ITSubInvest, "ITSubInvestment", ITSub.DBTrack);
                                // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                Old_ITSub = context.ITSubInvestment.Where(e => e.Id == data).SingleOrDefault();
                                DT_ITSubInvestment DT_ITSubInvestment = (DT_ITSubInvestment)obj;

                                db.Create(DT_ITSubInvestment);
                                //db.SaveChanges();
                            }
                            blog.DBTrack = ITSub.DBTrack;
                            db.ITSubInvestment.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            //  return Json(new Object[] { blog.Id, ITSub.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = ITSub.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }

                    }
                    //  return View();
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

        public int EditS(int data, ITSubInvestment L, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var CurInvest = db.ITSubInvestment.Find(data);
                TempData["CurrRowVersion"] = CurInvest.RowVersion;
                db.Entry(CurInvest).State = System.Data.Entity.EntityState.Detached;
                //if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                //{
                L.DBTrack = dbT;
                ITSubInvestment ITSubInvest = new ITSubInvestment()
                {
                    SubInvestmentName = L.SubInvestmentName,
                    Id = data,
                    DBTrack = L.DBTrack
                };


                db.ITSubInvestment.Attach(ITSubInvest);
                db.Entry(ITSubInvest).State = System.Data.Entity.EntityState.Modified;
                db.Entry(ITSubInvest).OriginalValues["RowVersion"] = TempData["RowVersion"];
                //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                return 1;
                // }
                return 0;
            }
        }

        [HttpPost]
        public ActionResult Delete(int? data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                ITSubInvestment ITSubInvestment = db.ITSubInvestment.Find(data);
                try
                {
                    if (data != null)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            db.ITSubInvestment.Attach(ITSubInvestment);
                            db.Entry(ITSubInvestment).State = System.Data.Entity.EntityState.Deleted;
                            db.SaveChanges();
                            db.Entry(ITSubInvestment).State = System.Data.Entity.EntityState.Detached;
                            ts.Complete();
                        }

                        // return this.Json(new { msg = "Data deleted." });
                        Msg.Add("  Data removed successfully.  ");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }
                    else
                    {
                        Msg.Add(" Data not deleted  ");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        // return this.Json(new { msg = "Data not deleted." });
                    }

                }

                catch (DataException /* dex */)
                {
                    //return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists contact your system administrator." });
                    Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                }
            }
        }
             
    }
}



