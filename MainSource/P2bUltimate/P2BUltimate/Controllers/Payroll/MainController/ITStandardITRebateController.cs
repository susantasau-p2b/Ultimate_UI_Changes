using P2b.Global;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
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
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Payroll;
using P2BUltimate.Security;

namespace P2BUltimate.Controllers.Payroll.MainController
{
    [AuthoriseManger]
    public class ITStandardITRebateController : Controller
    {
        private DataBaseContext db = new DataBaseContext();

        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/ITStandardITRebate/Index.cshtml");
        }


        [HttpPost]
        public ActionResult Create(ITStandardITRebate c, FormCollection form) //Create submit
        {
           List<string> Msg = new List<string>();
					try{
            if (ModelState.IsValid)
            {
                using (TransactionScope ts = new TransactionScope())
                {
                    string Regimelist = form["Regimelist"] == "0" ? "" : form["Regimelist"];
                    if (Regimelist != null && Regimelist != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(Regimelist));
                        c.Regime = val;
                    }
                  
                        if (db.ITStandardITRebate.Any(o => o.StartAmount == c.StartAmount && o.EndAmount == c.EndAmount && o.Regime_Id==c.Regime.Id))
                        {
                            Msg.Add("  Code Already Exists.  ");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }
                   
                    if (c.StartAmount > c.EndAmount)
                    {
                        Msg.Add(" Start Amount should be greater than End Amount ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    var alrDq = db.ITStandardITRebate.Any(q => q.Regime_Id == c.Regime.Id && ((q.StartAmount <= c.EndAmount && q.EndAmount <= c.EndAmount) || (q.StartAmount <= c.StartAmount && q.EndAmount >= c.EndAmount)) && (q.EndAmount >= c.StartAmount));
                    if (alrDq == true)
                    {
                        Msg.Add(" DATA With this already exist.  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        // return this.Json(new Object[] { "", "", "From Date already exists.", JsonRequestBehavior.AllowGet });
                    }

                    if (c.DeductionAmount>0 && c.DeductionPerc>0)
                    {
                          Msg.Add(" You cannont enter Amount and Percentage both at a time kindly enter any 1 of them.  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                    ITStandardITRebate StdITRebate = new ITStandardITRebate()
                    { 
                        StartAmount = c.StartAmount,
                        EndAmount = c.EndAmount ,
                        DeductionAmount = c.DeductionAmount,
                        DeductionPerc = c.DeductionPerc,
                        Regime=c.Regime,
                        DBTrack = c.DBTrack
                    };
                    try
                    {
                        db.ITStandardITRebate.Add(StdITRebate);
                        var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, c.DBTrack);
                        DT_ITStandardITRebate DT_StdITRebate = (DT_ITStandardITRebate)rtn_Obj;
                        db.Create(DT_StdITRebate);
                        db.SaveChanges();
                        ts.Complete();
                    //    return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
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
                    Msg.Add(" Unable to create.Try again, and if the problem persists, contact your system administrator");				
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
                Msg.Add(errorMsg);
                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                //return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
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

        [HttpPost]
        public ActionResult Edit(int data)
        {
            var Q = db.ITStandardITRebate
                .Include(e=>e.Regime)
                .Where(e => e.Id == data).Select
                (e => new
                {
                    Regime_Id = e.Regime.Id == null ? 0 : e.Regime.Id,
                    StartAmount = e.StartAmount,
                    EndAmount = e.EndAmount,
                    DeductionAmount = e.DeductionAmount,
                    DeductionPerc = e.DeductionPerc,
                    Action = e.DBTrack.Action
                }).ToList();


            var W = db.DT_ITStandardITRebate
                 .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                 (e => new
                 {
                     DT_Id = e.Id,
                     StartAmount = e.StartAmount,
                     EndAmount = e.EndAmount,
                     DeductionAmount = e.DeductionAmount,
                     DeductionPerc = e.DeductionPerc
                 }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

            var ITRebate = db.ITStandardITRebate.Find(data);
            TempData["RowVersion"] = ITRebate.RowVersion;
            var Auth = ITRebate.DBTrack.IsModified;
            return Json(new Object[] { Q, "", W, Auth, JsonRequestBehavior.AllowGet });
        }

        [HttpPost]
        public async Task<ActionResult> EditSave(ITStandardITRebate c, int data, FormCollection form) // Edit submit
        {
            
   List<string> Msg = new List<string>();
					try{
            bool Auth = form["Autho_Allow"] == "true" ? true : false;
            string Regimelist = form["Regimelist"] == "0" ? "" : form["Regimelist"];
            c.Regime_Id = Regimelist != null && Regimelist != "" ? int.Parse(Regimelist) : 0; 
            if (c.DeductionAmount > 0 && c.DeductionPerc > 0)
            {
                Msg.Add(" You cannont enter Amount and Percentage both at a time kindly enter any 1 of them.  ");
                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
            }
            if (c.StartAmount > c.EndAmount)
            {
                Msg.Add(" Start Amount should be greater than End Amount ");
                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
            }
            if (Auth == false)
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            ITStandardITRebate blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.ITStandardITRebate.Where(e => e.Id == data).SingleOrDefault();
                                originalBlogValues = context.Entry(blog).OriginalValues;
                            }

                            c.DBTrack = new DBTrack
                            {
                                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                Action = "M",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };
                            if (TempData["RowVersion"] == null)
                            {
                                TempData["RowVersion"] = blog.RowVersion;
                            }

                            var CurStdITRebate = db.ITStandardITRebate.Find(data);
                            TempData["CurrRowVersion"] = CurStdITRebate.RowVersion;
                            db.Entry(CurStdITRebate).State = System.Data.Entity.EntityState.Detached;
                            if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                            {
                                ITStandardITRebate StdITRebate = new ITStandardITRebate()
                                {
                                    Regime_Id=c.Regime_Id,
                                    StartAmount = c.StartAmount,
                                    EndAmount = c.EndAmount,
                                    DeductionAmount = c.DeductionAmount,
                                    DeductionPerc = c.DeductionPerc,
                                    Id = data,
                                    DBTrack = c.DBTrack
                                };


                                db.ITStandardITRebate.Attach(StdITRebate);
                                db.Entry(StdITRebate).State = System.Data.Entity.EntityState.Modified;
                                db.Entry(StdITRebate).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            }

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.DBTrackSave("Payroll/Payroll", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                DT_ITStandardITRebate DT_StdITRebate = (DT_ITStandardITRebate)obj;
                                db.Create(DT_StdITRebate);
                                db.SaveChanges();
                            }
                            await db.SaveChangesAsync();
                            ts.Complete();


                          //  return Json(new Object[] { c.Id, c.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });
                                Msg.Add("  Record Updated");
                    return Json(new Utility.JsonReturnClass { Id =  c.Id   , Val = c .FullDetails , success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        var entry = ex.Entries.Single();
                        var clientValues = (Corporate)entry.Entity;
                        var databaseEntry = entry.GetDatabaseValues();
                        if (databaseEntry == null)
                        {
                            //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                       Msg.Add(" Unable to save changes. The record was deleted by another user.");				
 					return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }
                        else
                        {
                            var databaseValues = (Corporate)databaseEntry.ToObject();
                            c.RowVersion = databaseValues.RowVersion;

                        }
                    }

                   // return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
              Msg.Add("Record modified by another user.So refresh it and try to save again.");
					return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                }
            }
            else
            {
                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {

                    ITStandardITRebate blog = null; // to retrieve old data
                    DbPropertyValues originalBlogValues = null;
                    ITStandardITRebate Old_StdITRebate = null;

                    using (var context = new DataBaseContext())
                    {
                        blog = context.ITStandardITRebate.Where(e => e.Id == data).SingleOrDefault();
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

                    if (TempData["RowVersion"] == null)
                    {
                        TempData["RowVersion"] = blog.RowVersion;
                    }

                    ITStandardITRebate StdITRebate = new ITStandardITRebate()
                    {
                        StartAmount = c.StartAmount,
                        EndAmount = c.EndAmount,
                        DeductionAmount = c.DeductionAmount,
                        DeductionPerc = c.DeductionPerc,
                        Id = data,
                        DBTrack = c.DBTrack,
                        RowVersion = (Byte[])TempData["RowVersion"]
                    };


                    using (var context = new DataBaseContext())
                    {
                        var obj = DBTrackFile.ModifiedDataHistory("Payroll/Payroll", "M", blog, StdITRebate, "ITStandardITRebate", c.DBTrack);
                       
                        Old_StdITRebate = context.ITStandardITRebate.Where(e => e.Id == data).SingleOrDefault();
                        DT_Corporate DT_Corp = (DT_Corporate)obj;
                       
                        db.Create(DT_Corp);
                        //db.SaveChanges();
                    }
                    blog.DBTrack = c.DBTrack;
                    db.ITStandardITRebate.Attach(blog);
                    db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    db.SaveChanges();
                    ts.Complete();
                    //return Json(new Object[] { blog.Id, c.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });
               Msg.Add("  Record Updated");
                    return Json(new Utility.JsonReturnClass { Id = blog .Id   , Val = c .FullDetails , success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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

        [HttpPost]
        public async Task<ActionResult> AuthSave(int auth_id, Boolean isauth, String auth_action) //to save authorised data
        { List<string> Msg = new List<string>();
					try{
            if (auth_action == "C")
            {
                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    //Corporate corp = db.Corporate.Find(auth_id);
                    //Corporate corp = db.Corporate.AsNoTracking().FirstOrDefault(e => e.Id == auth_id);

                    Corporate corp = db.Corporate.Include(e => e.Address)
                        .Include(e => e.ContactDetails)
                        .Include(e => e.BusinessType).FirstOrDefault(e => e.Id == auth_id);

                    corp.DBTrack = new DBTrack
                    {
                        Action = "C",
                        ModifiedBy = corp.DBTrack.ModifiedBy != null ? corp.DBTrack.ModifiedBy : null,
                        CreatedBy = corp.DBTrack.CreatedBy != null ? corp.DBTrack.CreatedBy : null,
                        CreatedOn = corp.DBTrack.CreatedOn != null ? corp.DBTrack.CreatedOn : null,
                        IsModified = corp.DBTrack.IsModified == true ? false : false,
                        AuthorizedBy = SessionManager.UserName,
                        AuthorizedOn = DateTime.Now
                    };

                    db.Corporate.Attach(corp);
                    db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    //db.SaveChanges();
                    var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corp.DBTrack);
                    DT_Corporate DT_Corp = (DT_Corporate)rtn_Obj;
                    DT_Corp.Address_Id = corp.Address == null ? 0 : corp.Address.Id;
                    DT_Corp.BusinessType_Id = corp.BusinessType == null ? 0 : corp.BusinessType.Id;
                    DT_Corp.ContactDetails_Id = corp.ContactDetails == null ? 0 : corp.ContactDetails.Id;
                    db.Create(DT_Corp);
                    await db.SaveChangesAsync();

                    ts.Complete();
                    Msg.Add("  Record Authorised");
                    return Json(new Utility.JsonReturnClass { Id =  corp.Id ,success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                  //  return Json(new Object[] { corp.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                }
            }
            else if (auth_action == "M")
            {

                ITStandardITRebate Old_StdITRebate = db.ITStandardITRebate.Where(e => e.Id == auth_id).SingleOrDefault();

                DT_ITStandardITRebate Curr_StdITRebate = db.DT_ITStandardITRebate
                                            .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                            .OrderByDescending(e => e.Id)
                                            .FirstOrDefault();

                if (Curr_StdITRebate != null)
                {
                    ITStandardITRebate StdITRebate = new ITStandardITRebate();

                    StdITRebate.StartAmount = Curr_StdITRebate.StartAmount == null ? Old_StdITRebate.StartAmount : Curr_StdITRebate.StartAmount;
                    StdITRebate.EndAmount = Curr_StdITRebate.EndAmount == null ? Old_StdITRebate.EndAmount : Curr_StdITRebate.EndAmount;
                    StdITRebate.DeductionAmount = Curr_StdITRebate.DeductionAmount == null ? Old_StdITRebate.DeductionAmount : Curr_StdITRebate.DeductionAmount;
                    StdITRebate.DeductionPerc = Curr_StdITRebate.DeductionPerc == null ? Old_StdITRebate.DeductionPerc : Curr_StdITRebate.DeductionPerc;
                    

                    if (ModelState.IsValid)
                    {
                        try
                        {

                            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {
                                // db.Configuration.AutoDetectChangesEnabled = false;
                                StdITRebate.DBTrack = new DBTrack
                                {
                                    CreatedBy = Old_StdITRebate.DBTrack.CreatedBy == null ? null : Old_StdITRebate.DBTrack.CreatedBy,
                                    CreatedOn = Old_StdITRebate.DBTrack.CreatedOn == null ? null : Old_StdITRebate.DBTrack.CreatedOn,
                                    Action = "M",
                                    ModifiedBy = Old_StdITRebate.DBTrack.ModifiedBy == null ? null : Old_StdITRebate.DBTrack.ModifiedBy,
                                    ModifiedOn = Old_StdITRebate.DBTrack.ModifiedOn == null ? null : Old_StdITRebate.DBTrack.ModifiedOn,
                                    AuthorizedBy = SessionManager.UserName,
                                    AuthorizedOn = DateTime.Now,
                                    IsModified = false
                                };

                                var CurStdITRebate = db.ITStandardITRebate.Find(auth_id);
                                TempData["CurrRowVersion"] = CurStdITRebate.RowVersion;
                                db.Entry(CurStdITRebate).State = System.Data.Entity.EntityState.Detached;
                                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                {
                                    ITStandardITRebate StandardITRebate = new ITStandardITRebate()
                                    {
                                        StartAmount = StdITRebate.StartAmount,
                                        EndAmount = StdITRebate.EndAmount,
                                        DeductionAmount = StdITRebate.DeductionAmount,
                                        DeductionPerc = StdITRebate.DeductionPerc,
                                        Id = auth_id,
                                        DBTrack = StdITRebate.DBTrack
                                    };


                                    db.ITStandardITRebate.Attach(StandardITRebate);
                                    db.Entry(StandardITRebate).State = System.Data.Entity.EntityState.Modified;
                                    db.Entry(StandardITRebate).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                }

                               
                                await db.SaveChangesAsync();

                                ts.Complete();
                              //  return Json(new Object[] { StdITRebate.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                             Msg.Add("  Record Authorised");
                    return Json(new Utility.JsonReturnClass { Id =  StdITRebate.Id ,success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }
                        }
                        catch (DbUpdateConcurrencyException ex)
                        {
                            var entry = ex.Entries.Single();
                            var clientValues = (Corporate)entry.Entity;
                            var databaseEntry = entry.GetDatabaseValues();
                            if (databaseEntry == null)
                            {
                              //  return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                           Msg.Add(" Unable to save changes. The record was deleted by another user.");				
 					return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }
                            else
                            {
                                var databaseValues = (Corporate)databaseEntry.ToObject();
                                StdITRebate.RowVersion = databaseValues.RowVersion;
                            }
                        }

                       // return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                    Msg.Add("Record modified by another user.So refresh it and try to save again.");
					return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }
                }
                else
                    Msg.Add("  Data removed from history  ");
           			return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                   // return Json(new Object[] { "", "Data removed from history", JsonRequestBehavior.AllowGet });
            }
            else if (auth_action == "D")
            {
                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    
                    ITStandardITRebate StdITRebate = db.ITStandardITRebate.AsNoTracking().FirstOrDefault(e => e.Id == auth_id);


                    StdITRebate.DBTrack = new DBTrack
                    {
                        Action = "D",
                        ModifiedBy = StdITRebate.DBTrack.ModifiedBy != null ? StdITRebate.DBTrack.ModifiedBy : null,
                        CreatedBy = StdITRebate.DBTrack.CreatedBy != null ? StdITRebate.DBTrack.CreatedBy : null,
                        CreatedOn = StdITRebate.DBTrack.CreatedOn != null ? StdITRebate.DBTrack.CreatedOn : null,
                        IsModified = false,
                        AuthorizedBy = SessionManager.UserName,
                        AuthorizedOn = DateTime.Now
                    };

                    db.ITStandardITRebate.Attach(StdITRebate);
                    db.Entry(StdITRebate).State = System.Data.Entity.EntityState.Deleted;


                    var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, StdITRebate.DBTrack);
                    DT_ITStandardITRebate DT_StdITRebate = (DT_ITStandardITRebate)rtn_Obj;

                    db.Create(DT_StdITRebate);
                    await db.SaveChangesAsync();
                    db.Entry(StdITRebate).State = System.Data.Entity.EntityState.Detached;
                    ts.Complete();
                  //  return Json(new Object[] { "", "Record Authorised", JsonRequestBehavior.AllowGet });
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
                IEnumerable<ITStandardITRebate> ITStandardITRebate = null;
                if (gp.IsAutho == true)
                {
                    ITStandardITRebate = db.ITStandardITRebate.Include(e=>e.Regime).Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    ITStandardITRebate = db.ITStandardITRebate.Include(e => e.Regime).AsNoTracking().ToList();
                }

                IEnumerable<ITStandardITRebate> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = ITStandardITRebate;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.StartAmount.ToString().Contains(gp.searchString.ToString()))
                               || (e.EndAmount.ToString().Contains(gp.searchString))
                               || (e.DeductionAmount.ToString().Contains(gp.searchString))
                               || (e.DeductionPerc.ToString().Contains(gp.searchString))
                               || (e.Id.ToString().Contains(gp.searchString))
                               ).Select(a => new Object[] { a.StartAmount, a.EndAmount, a.DeductionAmount, a.DeductionPerc, a.Id }).ToList();


                        //jsonData = IE.Where(e => (e.Id.ToString().Contains(gp.searchString))
                        //        ||  (e.StartAmount.ToString().Contains(gp.searchString))
                        //        ||  (e.EndAmount.ToString().Contains(gp.searchString))
                        //        ||  (e.DeductionAmount.ToString().Contains(gp.searchString))
                        //        ||  (e.DeductionPerc.ToString().Contains(gp.searchString))
                        //        ).Select(a => new Object[] { a.StartAmount, a.EndAmount, a.DeductionAmount, a.DeductionPerc, a.Id }).ToList();
                          }
                    if (pageIndex >= 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.StartAmount, a.EndAmount, a.DeductionAmount, a.DeductionPerc, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = ITStandardITRebate;
                    Func<ITStandardITRebate, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "StartAmount" ? c.StartAmount.ToString() :
                                         gp.sidx == "EndAmount" ? c.EndAmount.ToString() :
                                         gp.sidx == "DeductionAmount" ? c.DeductionAmount.ToString() :
                                         gp.sidx == "DeductionPerc" ? c.DeductionPerc.ToString() : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.StartAmount, a.EndAmount, a.DeductionAmount, a.DeductionPerc, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.StartAmount, a.EndAmount, a.DeductionAmount, a.DeductionPerc, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.StartAmount, a.EndAmount, a.DeductionAmount, a.DeductionPerc, a.Id }).ToList();
                    }
                    totalRecords = ITStandardITRebate.Count();
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
            try{
            ITStandardITRebate ITStandardITRebate = db.ITStandardITRebate.Where(e => e.Id == data).SingleOrDefault();


            if (ITStandardITRebate.DBTrack.IsModified == true)
            {
                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                   
                    DBTrack dbT = new DBTrack
                    {
                        Action = "D",
                        CreatedBy = ITStandardITRebate.DBTrack.CreatedBy != null ? ITStandardITRebate.DBTrack.CreatedBy : null,
                        CreatedOn = ITStandardITRebate.DBTrack.CreatedOn != null ? ITStandardITRebate.DBTrack.CreatedOn : null,
                        IsModified = ITStandardITRebate.DBTrack.IsModified == true ? true : false
                    };
                    ITStandardITRebate.DBTrack = dbT;
                    db.Entry(ITStandardITRebate).State = System.Data.Entity.EntityState.Modified;
                    var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, ITStandardITRebate.DBTrack);
                    DT_ITStandardITRebate DT_ITRebate = (DT_ITStandardITRebate)rtn_Obj;
                   
                    db.Create(DT_ITRebate);
                    
                    await db.SaveChangesAsync();
                    
                    ts.Complete();
                   // return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
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
                            CreatedBy = ITStandardITRebate.DBTrack.CreatedBy != null ? ITStandardITRebate.DBTrack.CreatedBy : null,
                            CreatedOn = ITStandardITRebate.DBTrack.CreatedOn != null ? ITStandardITRebate.DBTrack.CreatedOn : null,
                            IsModified = ITStandardITRebate.DBTrack.IsModified == true ? false : false//,
                        };
                        db.Entry(ITStandardITRebate).State = System.Data.Entity.EntityState.Deleted;
                        var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, dbT);
                        DT_ITStandardITRebate DT_Corp = (DT_ITStandardITRebate)rtn_Obj;
                        db.Create(DT_Corp);

                        await db.SaveChangesAsync();
                        ts.Complete();
                       // return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                          Msg.Add("  Data removed successfully.  ");
           			return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }
                    catch (RetryLimitExceededException /* dex */)
                    {
                        //Log the error (uncomment dex variable name and add a line here to write a log.)
                        //ModelState.AddModelError("", "Unable to delete. Try again, and if the problem persists, see your system administrator.");
                        //return RedirectToAction("Delete");
                      //  return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
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
}
