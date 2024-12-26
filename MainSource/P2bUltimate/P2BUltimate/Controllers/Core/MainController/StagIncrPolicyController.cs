///
/// Created by Sarika
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

namespace P2BUltimate.Controllers.Core.MainController
{
    public class StagIncrPolicyController : Controller
    {
//private DataBaseContext db = new DataBaseContext();
        //
        // GET: /StagIncrPolicy/
        public ActionResult partial()
        {
            return View("~/Views/Shared/Core/_StagIncrPolicy.cshtml");
        }
        public ActionResult Create(StagIncrPolicy lkval, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    lkval.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName };
                    StagIncrPolicy StagIncrPolicy = new StagIncrPolicy
                    {
                        Name = lkval.Name,
                        SpanYears = lkval.SpanYears,
                        MaxStagIncr = lkval.MaxStagIncr,
                        IsLastIncr = lkval.IsLastIncr,
                        IsFixAmount = lkval.IsFixAmount,
                        IncrAmount = lkval.IncrAmount,
                        DBTrack = lkval.DBTrack
                    };
                    try
                    {
                        if (ModelState.IsValid)
                        {

                            if (db.StagIncrPolicy.Any(o => o.Name == lkval.Name))
                            {
                                Msg.Add(" Name  Already Exists.  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // return Json(new Object[] { "", "", "Name  Already Exists.", JsonRequestBehavior.AllowGet });
                            }
                            using (TransactionScope ts = new TransactionScope())
                            {
                                db.StagIncrPolicy.Add(StagIncrPolicy);
                                //DBTrackFile.DBTrackSave("Core/P2b.Global", "C", lkval, null, "StagIncrPolicy", null);
                                db.SaveChanges();
                                ts.Complete();
                            }
                        }
                        else
                        {
                            //return Json(new Object[] { "", "Cannot Create Details.", JsonRequestBehavior.AllowGet });
                            Msg.Add("  Cannot Create Details...  ");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }
                        // return Json(new Object[] { StagIncrPolicy.Id, StagIncrPolicy.FullDetails, "Record Created", JsonRequestBehavior.AllowGet });
                        Msg.Add("  Data Created successfully  ");
                        return Json(new Utility.JsonReturnClass { Id = StagIncrPolicy.Id, Val = StagIncrPolicy.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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
                var qurey = db.StagIncrPolicy.Find(data);
                using (TransactionScope ts = new TransactionScope())
                {
                    //db.LookupValue.Attach(qurey);
                    //db.Entry(qurey).State = System.Data.Entity.EntityState.Deleted;
                    //db.SaveChanges();
                    //db.Entry(qurey).State = System.Data.Entity.EntityState.Detached;
                    //ts.Complete();

                    //DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };
                    db.Entry(qurey).State = System.Data.Entity.EntityState.Deleted;
                    //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dbT);
                    db.SaveChanges();
                    ts.Complete();
                    //   return this.Json(new { msg = "Data removed.", JsonRequestBehavior.AllowGet });
                    Msg.Add("  Data removed successfully.  ");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);


                }
            }
        }

        public ActionResult Edit(int? data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.StagIncrPolicy
                .Where(e => e.Id == data).Select
                (e => new
                {
                    Name = e.Name,
                    SpanYears = e.SpanYears,
                    MaxStagIncr = e.MaxStagIncr,
                    IsLastIncr = e.IsLastIncr,
                    IsFixAmount = e.IsFixAmount,
                    IncrAmount = e.IncrAmount,
                    Action = e.DBTrack.Action
                }).ToList();

                var add_data = db.IncrActivity
                  .Include(e => e.IncrPolicy)
                   .Where(e => e.Id == data).ToList();

                var W = db.DT_StagIncrPolicy
                    //.Include(e => e.IncrPolicy)
                    //.Include(e => e.IncrList)
                     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id,
                         Name = e.Name == null ? "" : e.Name,
                         SpanYears = e.SpanYears,
                         MaxStagIncr = e.MaxStagIncr,
                         IsLastIncr = e.IsLastIncr,
                         IsFixAmount = e.IsFixAmount,
                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var Corp = db.StagIncrPolicy.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, add_data, W, Auth, JsonRequestBehavior.AllowGet });
            }
        }

        public async Task<ActionResult> EditSave(StagIncrPolicy val, int data)
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
                                  var Curr_LKValue = db.StagIncrPolicy.Find(data);
                                  TempData["CurrRowVersion"] = Curr_LKValue.RowVersion;
                                  db.Entry(Curr_LKValue).State = System.Data.Entity.EntityState.Detached;
                                  if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                  {
                                      StagIncrPolicy blog = blog = null;
                                      DbPropertyValues originalBlogValues = null;

                                      using (var context = new DataBaseContext())
                                      {
                                          blog = context.StagIncrPolicy.Where(e => e.Id == data).SingleOrDefault();
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
                                      StagIncrPolicy lkval = new StagIncrPolicy
                                      {
                                          Id = data,
                                          Name = val.Name,
                                          SpanYears = val.SpanYears,
                                          MaxStagIncr = val.MaxStagIncr,
                                          IsLastIncr = val.IsLastIncr,
                                          IsFixAmount = val.IsFixAmount,
                                          IncrAmount = val.IncrAmount,
                                          DBTrack = val.DBTrack
                                      };


                                      db.StagIncrPolicy.Attach(lkval);
                                      db.Entry(lkval).State = System.Data.Entity.EntityState.Modified;

                                      // db.Entry(c.DBTrack).State = System.Data.Entity.EntityState.Modified;
                                      //db.SaveChanges();
                                      db.Entry(lkval).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                      // DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, val.DBTrack);
                                      await db.SaveChangesAsync();
                                      //DisplayTrackedEntities(db.ChangeTracker);
                                      db.Entry(lkval).State = System.Data.Entity.EntityState.Detached;
                                      ts.Complete();
                                      //return Json(new Object[] { lkval.Id, lkval.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });
                                      Msg.Add("  Record Updated");
                                      return Json(new Utility.JsonReturnClass { Id = lkval.Id, Val = lkval.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                  }
                              }
                          }
                          return View();
                      }
                      catch (DbUpdateConcurrencyException e) { throw e; }
                      catch (DbUpdateException e) { throw e; }
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

        public ActionResult GetLookup_StagIncrPolicy(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.StagIncrPolicy.ToList();
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.StagIncrPolicy.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
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