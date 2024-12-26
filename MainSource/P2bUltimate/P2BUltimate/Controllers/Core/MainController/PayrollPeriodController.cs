using P2b.Global;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using P2BUltimate.Security;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using System.Web.Mvc;

namespace P2BUltimate.Controllers
{
     [AuthoriseManger]
    public class PayrollPeriodController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();
        //
        // GET: /PayrollPeriod/
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult PayrollPeriod_partial()
        {
            return View("~/Views/Shared/Core/_PayrollPeriod.cshtml");
        }

        [HttpPost]
        public ActionResult Create(PayrollPeriod PayPeriod, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    //if (form["StartDate_drop"] == "-Select-" || form["ToDate_drop"] == "-Select-")
                    //{
                    //    return this.Json(new Object[] { null, null, "Select Start &To Date", JsonRequestBehavior.AllowGet });
                    //}



                    PayPeriod.StartDate = Convert.ToInt32(form["StartDate_drop"] == "-Select-" ? "0" : form["StartDate_drop"]);
                    PayPeriod.EndDate = Convert.ToInt32(form["ToDate_drop"] == "-Select-" ? "0" : form["ToDate_drop"]);
                    PayPeriod.PayDate = Convert.ToInt32(form["PayDate_drop1"] == "-Select-" ? "0" : form["PayDate_drop1"]);

                    if (PayPeriod.StartDate > PayPeriod.EndDate)
                    {
                        Msg.Add("  Start Date should be leass than To Date.  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //return this.Json(new Object[] { null, null, "Start Date should be leass than To Date.", JsonRequestBehavior.AllowGet });
                    }

                    if (ModelState.IsValid)
                    {
                        PayPeriod.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                        PayrollPeriod PayrollPer = new PayrollPeriod()
                        {
                            StartDate = PayPeriod.StartDate,
                            EndDate = PayPeriod.EndDate,
                            PayDate = PayPeriod.PayDate,
                            DBTrack = PayPeriod.DBTrack
                        };

                        try
                        {
                            using (TransactionScope ts = new TransactionScope())
                            {
                                db.PayrollPeriod.Add(PayrollPer);
                                var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, PayrollPer.DBTrack);
                                DT_PayrollPeriod DT_PayrollPeriod = (DT_PayrollPeriod)rtn_Obj;
                                db.Create(DT_PayrollPeriod);
                                db.SaveChanges();
                                ts.Complete();
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = PayrollPer.Id, Val = PayrollPer.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Object[] { PayrollPer.Id, PayrollPer.FullDetails, "Data Saved successfully" }, JsonRequestBehavior.AllowGet);
                            }
                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            return RedirectToAction("Create", new { concurrencyError = true, id = PayrollPer.Id });
                        }
                        catch (DataException /* dex */)
                        {
                            Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            // return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
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
                        //return Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
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
                var Q = db.PayrollPeriod

                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        StartDate = e.StartDate,
                        EndDate = e.EndDate,
                        PayDate = e.PayDate,


                        Action = e.DBTrack.Action
                    }).ToList();

                //var add_data = db.VisaDetails
                //    .Include(e => e.Country)
                //    .Include(e => e.VisaType)
                //    .Where(e => e.Id == data)
                //    .Select(e => new
                //    {
                //        Country_FullDetails = e.Country.Name == null ? "" : e.Country.Name,
                //        Country_Id = e.Country.Id == null ? "" : e.Country.Id.ToString(),
                //    }).ToList();

                var Old_Data = db.DT_PayrollPeriod
                    // .Include(e => e.VisaType)
                    //  .Include(e => e.Country)                 
                     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id,
                         StartDate = e.StartDate,
                         EndDate = e.EndDate,
                         PayDate = e.PayDate,

                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();
                var EOBJ = db.PayrollPeriod.Find(data);
                TempData["RowVersion"] = EOBJ.RowVersion;
                var Auth = EOBJ.DBTrack.IsModified;

                //var Corp = db.PayProcessGroup.Find(data);
                //TempData["RowVersion"] = Corp.RowVersion;
                //var Auth = Corp.DBTrack.IsModified;

                return Json(new Object[] { Q, Old_Data, Auth, JsonRequestBehavior.AllowGet });
            }
        }


        public ActionResult PopulateDropDownList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var qurey = db.PayrollPeriod.ToList();
                var selected = (Object)null;
                if (data2 != "" && data != "0" && data2 != "0")
                {
                    selected = Convert.ToInt32(data2);
                }

                SelectList s = new SelectList(qurey, "Id", "StartDate", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public async Task<ActionResult> EditSave(PayrollPeriod L, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;
                    L.StartDate = Convert.ToInt32(form["StartDate_drop"] == "-Select-" ? "0" : form["StartDate_drop"]);
                    L.EndDate = Convert.ToInt32(form["ToDate_drop"] == "-Select-" ? "0" : form["ToDate_drop"]);
                    L.PayDate = Convert.ToInt32(form["PayDate_drop1"] == "-Select-" ? "0" : form["PayDate_drop1"]);

                    if (Auth == false)
                    {


                        if (ModelState.IsValid)
                        {
                            try
                            {

                                //DbContextTransaction transaction = db.Database.BeginTransaction();

                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    PayrollPeriod blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.PayrollPeriod.Where(e => e.Id == data).SingleOrDefault();
                                        originalBlogValues = context.Entry(blog).OriginalValues;
                                    }

                                    L.DBTrack = new DBTrack
                                    {
                                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };

                                  //  int a = EditS(data, L, L.DBTrack);

                                    var CurCorp = db.PayrollPeriod.Find(data);
                                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {
                                      //  L.DBTrack = dbT;
                                        PayrollPeriod corp = new PayrollPeriod()
                                        {
                                            StartDate = L.StartDate,
                                            EndDate = L.EndDate,
                                            PayDate = L.PayDate,

                                            Id = data,
                                            DBTrack = L.DBTrack
                                        };


                                        db.PayrollPeriod.Attach(corp);
                                        db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                                        db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                        //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                       /// return 1;

                                    using (var context = new DataBaseContext())
                                    {
                                        var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, L.DBTrack);
                                        DT_PayrollPeriod DT_Corp = (DT_PayrollPeriod)obj;
                                        db.Create(DT_Corp);
                                        db.SaveChanges();
                                    }
                                    await db.SaveChangesAsync();
                                    ts.Complete();

                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = corp.Id, Val = corp.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new Object[] { L.Id, L.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });

                                    }
                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (PayrollPeriod)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (PayrollPeriod)databaseEntry.ToObject();
                                    L.RowVersion = databaseValues.RowVersion;

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

                            PayrollPeriod blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            PayrollPeriod Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.PayrollPeriod.Where(e => e.Id == data).SingleOrDefault();
                                originalBlogValues = context.Entry(blog).OriginalValues;
                            }
                            L.DBTrack = new DBTrack
                            {
                                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                Action = "M",
                                IsModified = blog.DBTrack.IsModified == true ? true : false,
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };
                            PayrollPeriod corp = new PayrollPeriod()
                            {
                                StartDate = L.StartDate,
                                EndDate = L.EndDate,
                                PayDate = L.PayDate,

                                Id = data,
                                DBTrack = L.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };


                            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, corp, "PayrollPeriod", L.DBTrack);
                                // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                Old_Corp = context.PayrollPeriod.Where(e => e.Id == data).SingleOrDefault();
                                DT_PayrollPeriod DT_Corp = (DT_PayrollPeriod)obj;

                                db.Create(DT_Corp);
                                //db.SaveChanges();
                            }
                            blog.DBTrack = L.DBTrack;
                            db.PayrollPeriod.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = L.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            // return Json(new Object[] { blog.Id, L.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });
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
        public int EditS(int data, PayrollPeriod L, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var CurCorp = db.PayrollPeriod.Find(data);
                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    L.DBTrack = dbT;
                    PayrollPeriod corp = new PayrollPeriod()
                    {
                        StartDate = L.StartDate,
                        EndDate = L.EndDate,
                        PayDate = L.PayDate,

                        Id = data,
                        DBTrack = L.DBTrack
                    };


                    db.PayrollPeriod.Attach(corp);
                    db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                    return 1;
                }
                return 0;
            }
        }
	}
}