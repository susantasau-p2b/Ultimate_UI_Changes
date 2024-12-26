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

namespace P2BUltimate.Controllers
{
    [AuthoriseManger]
    public class ServiceRangeController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();
        //
        // GET: /ServiceRange/

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult CreateSave(ServiceRange servRange, FormCollection form)
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
                            //if (db.WagesRange.Any(o => o. == wage.WagesName))
                            //{
                            //    //ModelState.AddModelError(string.Empty, "Code already exists.");
                            //    return this.Json(new { msg = "Name already exists." });
                            //    //return View(c);
                            //}
                            servRange.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            ServiceRange Range = new ServiceRange()
                            {

                                CeilingMax = servRange.CeilingMax,
                                CeilingMin = servRange.CeilingMin,
                                Percentage = servRange.Percentage,
                                Amount = servRange.Amount,
                                WagesFrom = servRange.WagesFrom,
                                WagesTo = servRange.WagesTo,
                                ServiceFrom = servRange.ServiceFrom,
                                ServiceTo = servRange.ServiceTo,
                                DBTrack = servRange.DBTrack
                            };
                            try
                            {
                                db.ServiceRange.Add(Range);
                                var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, servRange.DBTrack);
                                DT_ServiceRange DT_ServiceRange = (DT_ServiceRange)rtn_Obj;
                                db.Create(DT_ServiceRange);
                                db.SaveChanges();
                                ts.Complete();
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = Range.Id, Val = Range.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
                                // return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
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
                        // return this.Json(new { msg = errorMsg });
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

        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                var r = (from ca in db.ServiceRange
                         select new
                         {
                             Id = ca.Id,
                             Servicefrom = ca.ServiceFrom,
                             ServiceTo = ca.ServiceTo,
                             RangeFrom = ca.WagesFrom,
                             RangeTo = ca.WagesTo,
                             Amount = ca.Amount,
                             CeilingMin = ca.CeilingMin,
                             CeilingMax = ca.CeilingMax,
                             Percentage = ca.Percentage
                         }).Where(e => e.Id == data).ToList();

                return Json(r,JsonRequestBehavior.AllowGet);


            }
        }



        public ActionResult EditServRange_partial(int data)
        {
            //var add = db.Wages.Include(e => e.RateMaster)
            //.Where(e => e.Id == data).SingleOrDefault();
            using (DataBaseContext db = new DataBaseContext())
            {
                var r = (from ca in db.ServiceRange
                         select new
                         {
                             Id = ca.Id,
                             CeilingMax = ca.CeilingMax,
                             CeilingMin = ca.CeilingMin,
                             Percentage = ca.Percentage,
                             Amount = ca.Amount,
                             WagesFrom = ca.WagesFrom,
                             WagesTo = ca.WagesTo,
                             ServiceFrom = ca.ServiceFrom,
                             ServiceTo = ca.ServiceTo
                         }).Where(e => e.Id == data).ToList();



                return Json(new object[] { r }, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        // [ValidateAntiForgeryToken]
        //public ActionResult EditSave(ServiceRange servRange, int data, FormCollection form)
        //{
        //    List<string> Msg = new List<string>();
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        try
        //        {
        //            if (ModelState.IsValid)
        //            {
        //                using (TransactionScope ts = new TransactionScope())
        //                {
        //                    ServiceRange blog = null; // to retrieve old data
        //                    DbPropertyValues originalBlogValues = null;

        //                    using (var context = new DataBaseContext())
        //                    {
        //                        blog = context.ServiceRange.Where(e => e.Id == data).SingleOrDefault();
        //                        originalBlogValues = context.Entry(blog).OriginalValues;
        //                    }

        //                    servRange.DBTrack = new DBTrack
        //                    {
        //                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                        Action = "M",
        //                        ModifiedBy = SessionManager.UserName,
        //                        ModifiedOn = DateTime.Now
        //                    };


        //                    ServiceRange Range = new ServiceRange()
        //                    {

        //                        CeilingMax = servRange.CeilingMax,
        //                        CeilingMin = servRange.CeilingMin,
        //                        Percentage = servRange.Percentage,
        //                        Amount = servRange.Amount,
        //                        WagesFrom = servRange.WagesFrom,
        //                        WagesTo = servRange.WagesTo,
        //                        ServiceFrom = servRange.ServiceFrom,
        //                        ServiceTo = servRange.ServiceTo,
        //                        Id = data,
        //                        DBTrack = servRange.DBTrack
        //                    };
        //                    try
        //                    {
        //                        db.Entry(Range).State = System.Data.Entity.EntityState.Modified;
        //                        var obj = DBTrackFile.DBTrackSave("Payroll/Payroll", originalBlogValues, db.ChangeTracker, Range.DBTrack);
        //                        DT_ServiceRange DT_ServiceRange = (DT_ServiceRange)obj;
        //                        db.Create(DT_ServiceRange);
        //                        db.SaveChanges();
        //                        ts.Complete();
        //                        Msg.Add("  Data Saved successfully  ");
        //                        return Json(new Utility.JsonReturnClass { Id = Range.Id, Val = Range.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                        // return Json(new Object[] { Range.Id, Range.FullDetails, "Data saved successfully" }, JsonRequestBehavior.AllowGet);
        //                    }

        //                    catch (DbUpdateConcurrencyException)
        //                    {
        //                        return RedirectToAction("Create", new { concurrencyError = true, id = Range.Id });
        //                    }
        //                    catch (DataException /* dex */)
        //                    {
        //                        //Log the error (uncomment dex variable name after DataException and add a line here to write a log.
        //                        //ModelState.AddModelError(string.Empty, "Unable to create. Try again, and if the problem persists contact your system administrator.");
        //                        //return View(level);
        //                        Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
        //                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                        // return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
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
        //                Msg.Add(errorMsg);
        //                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                // return this.Json(new { msg = errorMsg });
        //            }
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
        public ActionResult EditSave(ServiceRange c, int data, FormCollection form)
        {
            List<string> Msg = new List<string>();
            
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        var db_data = db.ServiceRange.Where(e => e.Id == data).SingleOrDefault();

                        db.ServiceRange.Attach(db_data);
                        db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        TempData["RowVersion"] = db_data.RowVersion;

                        ServiceRange servicerange = db.ServiceRange.Find(data);
                        TempData["CurrRowVersion"] = servicerange.RowVersion;

                        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                        {
                           

                            c.DBTrack = new DBTrack
                            {
                                CreatedBy = servicerange.DBTrack.CreatedBy == null ? null : servicerange.DBTrack.CreatedBy,
                                CreatedOn = servicerange.DBTrack.CreatedOn == null ? null : servicerange.DBTrack.CreatedOn,
                                Action = "M",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };

                                servicerange.CeilingMax = c.CeilingMax;
                                servicerange.CeilingMin = c.CeilingMin;
                                servicerange.Percentage = c.Percentage;
                                servicerange.Amount = c.Amount;
                                servicerange.WagesFrom = c.WagesFrom;
                                servicerange. WagesTo = c.WagesTo;
                                servicerange.ServiceFrom = c.ServiceFrom;
                                servicerange.ServiceTo = c.ServiceTo;
                                servicerange.Id = data;
                                c.Id = data;

                               servicerange.DBTrack = c.DBTrack;

                              db.Entry(servicerange).State = System.Data.Entity.EntityState.Modified;


                            //using (var context = new DataBaseContext())
                            //{

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

        public ActionResult GetServRangeLKDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.ServiceRange.ToList();
                IEnumerable<ServiceRange> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.ServiceRange.ToList().Where(d => d.FullDetails.ToString().Contains(data));

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
                    ServiceRange serv = db.ServiceRange.Find(data);
                    SalHeadFormula sal = db.SalHeadFormula.Find(forwarddata);

                    //var ServDependRule = db.ServiceDependRule.Where(e => e.Id == forwarddata).SingleOrDefault();
                    //if (ServDependRule != null)
                    //{
                    //    ServDependRule.ServiceRange = null;
                    //    db.Entry(ServDependRule).State = System.Data.Entity.EntityState.Modified;
                    //}

                    using (TransactionScope ts = new TransactionScope())
                    {

                        db.SaveChanges();
                        ts.Complete();
                    }
                    Msg.Add("  Data removed successfully  ");
                    return Json(new Utility.JsonReturnClass { data = data.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    // return Json(new Object[] { data, "Data removed." }, JsonRequestBehavior.AllowGet);
                    // return this.Json(new { msg = "Data deleted." });
                }

                catch (DataException /* dex */)
                {
                    Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //Log the error (uncomment dex variable name after DataException and add a line here to write a log.
                    // return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists contact your system administrator." });
                    //ModelState.AddModelError(string.Empty, "Unable to delete. Try again, and if the problem persists contact your system administrator.");
                    //return RedirectToAction("Index");
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
