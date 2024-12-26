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

namespace P2BUltimate.Controllers
{
     [AuthoriseManger]
    public class LookupValueController : Controller
    {
     //   private DataBaseContext db = new DataBaseContext();

        public ActionResult partial()
        {
            return View("~/Views/Shared/Core/_Lookupvalues.cshtml");
        }

        public ActionResult Create(LookupValue lkval)
        {
             List<string> Msg = new List<string>();
             using (DataBaseContext db = new DataBaseContext())
             {
                 try
                 {
                     lkval.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName };
                     LookupValue LookupValue = new LookupValue
                     {
                         LookupVal = lkval.LookupVal,
                         IsActive = lkval.IsActive,
                         DeleteValue = lkval.DeleteValue,
                         DBTrack = lkval.DBTrack,
                         LookupValData = lkval.LookupValData
                     };
                     try
                     {
                         if (ModelState.IsValid)
                         {
                             using (TransactionScope ts = new TransactionScope())
                             {
                                 db.LookupValue.Add(LookupValue);
                                 var a = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, lkval.DBTrack);
                                 DT_LookupValue DT_LKVal = (DT_LookupValue)a;
                                 db.Create(DT_LKVal);
                                 ////  DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, lkval.DBTrack);
                                 db.SaveChanges();
                                 // DBTrackFile.DBTrackSave("Core/P2b.Global", "C", LookupValue, null, "LookupValue", null);
                                 ts.Complete();
                                 Msg.Add("  Record Updated");
                                 string LkData = "";
                                 if (LookupValue.LookupValData != "")
                                 {
                                     LkData = " - " + LookupValue.LookupValData;
                                 }
                                 return Json(new Utility.JsonReturnClass { Id = LookupValue.Id, Val = LookupValue.LookupVal + LkData, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                 //return Json(new Object[] { LookupValue.Id, LookupValue.LookupVal, "Record Created", JsonRequestBehavior.AllowGet });
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

                             //return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet }); 
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
                    var qurey = db.LookupValue.Find(data);
                    using (TransactionScope ts = new TransactionScope())
                    {
                        //db.LookupValue.Attach(qurey);
                        //db.Entry(qurey).State = System.Data.Entity.EntityState.Deleted;
                        //db.SaveChanges();
                        //db.Entry(qurey).State = System.Data.Entity.EntityState.Detached;
                        //ts.Complete();

                        DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };
                        db.Entry(qurey).State = System.Data.Entity.EntityState.Deleted;
                        ///// DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dbT);
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

        public ActionResult edit(int? data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var qurey = db.LookupValue.Where(e => e.Id == data).ToList();
                TempData["RowVersion"] = db.LookupValue.Find(data).RowVersion;
                return Json(qurey, JsonRequestBehavior.AllowGet);
            }
        }

        public async Task<ActionResult> EditSave(LookupValue val, int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //db.LookupValue.Attach(lkval);
                            //db.Entry(lkval).State = System.Data.Entity.EntityState.Modified;
                            //db.SaveChanges();
                            //db.Entry(lkval).State = System.Data.Entity.EntityState.Detached;
                            //ts.Complete();
                            var Curr_LKValue = db.LookupValue.Find(data);
                            TempData["CurrRowVersion"] = Curr_LKValue.RowVersion;
                            db.Entry(Curr_LKValue).State = System.Data.Entity.EntityState.Detached;
                            if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                            {
                                LookupValue blog = blog = null;
                                DbPropertyValues originalBlogValues = null;

                                using (var context = new DataBaseContext())
                                {
                                    blog = context.LookupValue.Where(e => e.Id == data).SingleOrDefault();
                                    originalBlogValues = context.Entry(blog).OriginalValues;
                                }

                                val.DBTrack = new DBTrack
                                {
                                    CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                    CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                    Action = "M",
                                    ModifiedBy = SessionManager.UserName,
                                    ModifiedOn = DateTime.Now
                                };
                                LookupValue lkval = new LookupValue
                                {
                                    Id = data,
                                    LookupVal = val.LookupVal,
                                    LookupValData = val.LookupValData,
                                    IsActive = val.IsActive,
                                    DeleteValue = val.DeleteValue,
                                    DBTrack = val.DBTrack
                                };


                                db.LookupValue.Attach(lkval);
                                db.Entry(lkval).State = System.Data.Entity.EntityState.Modified;

                                // db.Entry(c.DBTrack).State = System.Data.Entity.EntityState.Modified;
                                //db.SaveChanges();
                                db.Entry(lkval).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                ////  DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, val.DBTrack);
                                await db.SaveChangesAsync();
                                //DisplayTrackedEntities(db.ChangeTracker);
                                db.Entry(lkval).State = System.Data.Entity.EntityState.Detached;
                                ts.Complete();
                                Msg.Add("  Record Updated");
                                string LkData = "";
                                if (lkval.LookupValData != "")
                                {
                                    LkData = " - " + lkval.LookupValData;
                                }
                                return Json(new Utility.JsonReturnClass { Id = lkval.Id, Val = lkval.LookupVal + LkData, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                // return Json(new Object[] { lkval.Id, lkval.LookupVal, "Record Updated", JsonRequestBehavior.AllowGet });
                            }
                        }
                    }
                    return View();
                }
                catch (DbUpdateConcurrencyException e) { throw e; }
                catch (DbUpdateException e) { throw e; }
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