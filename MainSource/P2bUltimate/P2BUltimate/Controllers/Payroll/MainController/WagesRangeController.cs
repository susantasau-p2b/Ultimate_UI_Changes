using Payroll;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using P2BUltimate.App_Start;
using P2b.Global;
using P2BUltimate.Models;
using P2BUltimate.Security;
using System.Threading.Tasks;

namespace P2BUltimate.Controllers
{
    [AuthoriseManger]
    public class WagesRangeController : Controller
    {
        // private DataBaseContext db = new DataBaseContext();
        //
        // GET: /WagesRange/

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult WagesRangepartial()
        {
            return View("~/Views/Shared/Payroll/_wagesrange.cshtml");
        }

        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult CreateSave(WagesRange wageRange, FormCollection form)
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
                            wageRange.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            WagesRange Range = new WagesRange()
                            {

                                CeilingMax = wageRange.CeilingMax,
                                CeilingMin = wageRange.CeilingMin,
                                Percentage = wageRange.Percentage,
                                Amount = wageRange.Amount,
                                WageFrom = wageRange.WageFrom,
                                WageTo = wageRange.WageTo,
                                DBTrack = wageRange.DBTrack
                            };
                            try
                            {
                                db.WagesRange.Add(Range);
                                var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, wageRange.DBTrack);
                                DT_WagesRange DT_WagesRange = (DT_WagesRange)rtn_Obj;
                                db.Create(DT_WagesRange);
                                db.SaveChanges();
                                ts.Complete();
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = Range.Id, Val = Range.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Object[] { Range.Id, Range.FullDetails, "Data saved successfully" }, JsonRequestBehavior.AllowGet);
                            }

                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = Range.Id });
                            }
                            catch (DataException /* dex */)
                            {
                                //Log the error (uncomment dex variable name after DataException and add a line here to write a log.
                                //ModelState.AddModelError(string.Empty, "Unable to create. Try again, and if the problem persists contact your system administrator.");
                                //return View(level);
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
                        // return this.Json(new { msg = errorMsg });
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

        public ActionResult EditWagesRange_partial(int data)
        {
            //var add = db.Wages.Include(e => e.RateMaster)
            //.Where(e => e.Id == data).SingleOrDefault();
            using (DataBaseContext db = new DataBaseContext())
            {
                var r = (from ca in db.WagesRange.Where(e => e.Id == data)
                         select new
                         {
                             Id = ca.Id,
                             CeilingMax = ca.CeilingMax,
                             CeilingMin = ca.CeilingMin,
                             Percentage = ca.Percentage,
                             Amount = ca.Amount,
                             WageFrom = ca.WageFrom,
                             WageTo = ca.WageTo
                         }).SingleOrDefault();

                var Q = db.WagesRange
                   .Where(e => e.Id == data).Select
                   (ca => new
                   {
                       Id = ca.Id,
                       CeilingMax = ca.CeilingMax,
                       CeilingMin = ca.CeilingMin,
                       Percentage = ca.Percentage,
                       Amount = ca.Amount,
                       WageFrom = ca.WageFrom,
                       WageTo = ca.WageTo
                   }).SingleOrDefault();



                return Json(new object[] { Q }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public async Task<ActionResult> EditSave(WagesRange P, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;

                    if (Auth == false)
                    {
                        if (ModelState.IsValid)
                        {
                            try
                            {
                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    WagesRange blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.WagesRange.Where(e => e.Id == data).SingleOrDefault();
                                        originalBlogValues = context.Entry(blog).OriginalValues;
                                    }

                                    P.DBTrack = new DBTrack
                                    {
                                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };
                                    var m1 = db.WagesRange.Where(e => e.Id == data).ToList();
                                    foreach (var s in m1)
                                    {
                                        // s.AppraisalPeriodCalendar = null;
                                        db.WagesRange.Attach(s);
                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                        //await db.SaveChangesAsync();
                                        db.SaveChanges();
                                        TempData["RowVersion"] = s.RowVersion;
                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    }
                                    //int a = EditS(Category, PromoPolicy, data, c, c.DBTrack);
                                    var CurCorp = db.WagesRange.Find(data);
                                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {

                                        WagesRange corp = new WagesRange()
                                        {
                                            CeilingMax = P.CeilingMax,
                                            CeilingMin = P.CeilingMin,
                                            Percentage = P.Percentage,
                                            Amount = P.Amount,
                                            WageFrom = P.WageFrom,
                                            WageTo = P.WageTo,
                                            Id = data,
                                            DBTrack = P.DBTrack
                                        };

                                        db.WagesRange.Attach(corp);
                                        db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                                        db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                        // return 1;

                                        using (var context = new DataBaseContext())
                                        {
                                            var obj = DBTrackFile.DBTrackSave("Payroll/Payroll", originalBlogValues, db.ChangeTracker, P.DBTrack);
                                            DT_WagesRange DT_Corp = (DT_WagesRange)obj;
                                            // DT_Corp.WagesRangeArea_Id = blog.WagesRangeType == null ? 0 : blog.WagesRangeType.Id;

                                            db.Create(DT_Corp);
                                            db.SaveChanges();
                                        }
                                        await db.SaveChangesAsync();
                                        ts.Complete();
                                        Msg.Add("  Record Updated");
                                        return Json(new Utility.JsonReturnClass { Id = corp.Id, Val = corp.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
                                    P.RowVersion = databaseValues.RowVersion;

                                }
                            }
                            Msg.Add("Record modified by another user.So refresh it and try to save again.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
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

        public ActionResult GetWagesRangeLKDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.WagesRange.ToList();
                IEnumerable<WagesRange> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.WagesRange.ToList().Where(d => d.FullDetails.ToString().Contains(data));

                }
                else
                {
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                    //var result_1 = (from c in fall
                    //                select new { c.Id, c.CorporateCode, c.CorporateName });
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.FullDetails }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            // return View();
        }


        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult Delete(int? data, int? forwarddata)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    WagesRange wages = db.WagesRange.Find(data);
                    SalHeadFormula sal = db.SalHeadFormula.Find(forwarddata);

                    try
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {

                            db.SaveChanges();
                            ts.Complete();
                        }
                        Msg.Add("  Data removed successfully.  ");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        // return Json(new Object[] { data, "Data removed." }, JsonRequestBehavior.AllowGet);
                        // return this.Json(new { msg = "Data deleted." });
                    }

                    catch (DataException /* dex */)
                    {
                        //Log the error (uncomment dex variable name after DataException and add a line here to write a log.
                        Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        // return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists contact your system administrator." });
                        //ModelState.AddModelError(string.Empty, "Unable to delete. Try again, and if the problem persists contact your system administrator.");
                        //return RedirectToAction("Index");
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
