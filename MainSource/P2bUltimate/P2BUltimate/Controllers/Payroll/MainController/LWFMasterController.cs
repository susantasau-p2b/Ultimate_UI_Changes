//Create By Nikhil.

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
using Payroll;
using P2BUltimate.Security;

namespace P2BUltimate.Controllers
{
    [AuthoriseManger]
    public class LWFMasterController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();
        #region PageLinks
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/LWFMaster/Index.cshtml");
        }


        public ActionResult StatutoryEffectiveMonthspartial()
        {
            return View("~/Views/Shared/Payroll/_StatutoryEffectiveMonths.cshtml");
        }
        public ActionResult GetState(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.State.ToList();
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.State.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }
                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult WagesRangepartial()
        {
            return View("~/Views/Shared/Payroll/_wagesrange.cshtml");
        }
        #endregion

        #region CRUD OPERATION

        #region Create
        [HttpPost]
        public ActionResult Create(LWFMaster LWF, FormCollection form) //Create submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string EffectiveDate = form["EffectiveDate"] == "0" ? "" : form["EffectiveDate"];
                    //string EndDate = form["EndDate"] == "0" ? "" : form["EndDate"];
                    //string LWFWagesRangelist = form["LWFWagesRangelist"] == "0" ? "" : form["LWFWagesRangelist"];
                    string StateList = form["StateList"] == "0" ? "" : form["StateList"];
                    string StatutoryEffectiveMonthsList = form["StatutoryEffectiveMonthsList"] == "0" ? "" : form["StatutoryEffectiveMonthsList"];
                    string WagesMasterList = form["WagesMasterList"] == "0" ? "" : form["WagesMasterList"];
                    var id = int.Parse(Session["CompId"].ToString());
                    var companypayroll = db.CompanyPayroll.Where(e => e.Company.Id == id).SingleOrDefault();


                    if (EffectiveDate != null)
                    {
                        if (EffectiveDate != "")
                        {

                            var val = DateTime.Parse(EffectiveDate);
                            LWF.EffectiveDate = val;
                        }
                    }
                    //if (EndDate != null)
                    //{
                    //    if (EndDate != "")
                    //    {

                    //        var val = DateTime.Parse(EndDate);
                    //        LWF.EndDate = val;
                    //    }
                    //}              

                    if (StateList != null)
                    {
                        if (StateList != "")
                        {

                            var val = db.State.Find(int.Parse(StateList));
                            LWF.LWFStates = val;
                        }
                    }

                    List<StatutoryEffectiveMonths> StatutoryEffectiveMonths = new List<StatutoryEffectiveMonths>();
                    if (StatutoryEffectiveMonthsList != null && StatutoryEffectiveMonthsList != "")
                    {
                        var ids = Utility.StringIdsToListIds(StatutoryEffectiveMonthsList);
                        foreach (var ca in ids)
                        {
                            var StatutoryEffectiveMonthsList_val = db.StatutoryEffectiveMonths.Find(ca);
                            StatutoryEffectiveMonths.Add(StatutoryEffectiveMonthsList_val);
                            LWF.LWFStatutoryEffectiveMonths = StatutoryEffectiveMonths;
                        }
                    }

                    if (WagesMasterList != null)
                    {
                        if (WagesMasterList != "")
                        {
                            int WagesId = Convert.ToInt32(WagesMasterList);
                            var val = db.Wages.Where(e => e.Id == WagesId).SingleOrDefault();
                            LWF.WagesMaster = val;
                        }
                    }

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {


                            LWF.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            if (LWF.LWFStates == null)
                            {
                                Msg.Add("LWFStates is required.  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // return this.Json(new Object[] { "", "", "LWFStates is required.", JsonRequestBehavior.AllowGet });


                            }
                            if (LWF.LWFStatutoryEffectiveMonths == null)
                            {
                                //return this.Json(new Object[] { "", "", "LWFStatutoryEffectiveMonths is required.", JsonRequestBehavior.AllowGet });
                                Msg.Add("LWFStatutoryEffectiveMonths is required.  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                            if (LWF.WagesMaster == null)
                            {
                                Msg.Add("WagesMaster is required.  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return this.Json(new Object[] { "", "", "WagesMaster is required.", JsonRequestBehavior.AllowGet });

                            }

                            LWFMaster lwfmaster = new LWFMaster()
                            {

                                //LWFWageRange = LWF.LWFWageRange,
                                EffectiveDate = LWF.EffectiveDate,
                                //EndDate   = LWF.EndDate,
                                LWFStates = LWF.LWFStates,
                                LWFStatutoryEffectiveMonths = LWF.LWFStatutoryEffectiveMonths,
                                WagesMaster = LWF.WagesMaster,
                                DBTrack = LWF.DBTrack
                            };
                            try
                            {
                                db.LWFMaster.Add(lwfmaster);
                                var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, LWF.DBTrack);
                                //DT_LWFMaster DT_Corp = (DT_LWFMaster)rtn_Obj;
                                //DT_Corp.WagesMaster_Id = LWF.WagesMaster == null ? 0 : LWF.WagesMaster.Id;
                                ////DT_Corp.BusinessType_Id = c.BusinessType == null ? 0 : c.BusinessType.Id;
                                ////DT_Corp.ContactDetails_Id = c.ContactDetails == null ? 0 : c.ContactDetails.Id;
                                //db.Create(DT_Corp);
                                db.SaveChanges();
                                //DBTrackFile.DBTrackSave("Core/P2b.Global", "C", corporate, null, "Corporate", null);
                                if (companypayroll != null)
                                {
                                    List<LWFMaster> pfmasterlist = new List<LWFMaster>();
                                    pfmasterlist.Add(lwfmaster);
                                    companypayroll.LWFMaster = pfmasterlist;
                                    db.Entry(companypayroll).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    db.Entry(companypayroll).State = System.Data.Entity.EntityState.Detached;
                                }

                                ts.Complete();
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = lwfmaster.Id, Val = lwfmaster.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // return this.Json(new Object[] { lwfmaster.Id, lwfmaster.FullDetails, "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = LWF.Id });
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
                        //var errorMsg = sb.ToString();
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
        }
        #endregion

        #region EDIT & EDITSAVE
        public class LWFWagesRangeList
        {
            public Array LWFWagesRange_Id { get; set; }
            public Array LWFWagesRange_Fulldetails { get; set; }

        };

        public class StatutoryEffectiveMonthsList
        {
            public Array StatutoryEffectiveMonths_Id { get; set; }
            public Array StatutoryEffectiveMonths_Details { get; set; }

        };

        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.LWFMaster
                   .Include(e => e.LWFStates)
                   .Include(e => e.WagesMaster)
                    .Include(e => e.LWFStatutoryEffectiveMonths)
                   .Where(e => e.Id == data).Select
                   (e => new
                   {
                       EffectiveDate = e.EffectiveDate,
                       //EndDate = e.EndDate,
                       WagesMaster_Id = e.WagesMaster.Id == null ? 0 : e.WagesMaster.Id,
                       WagesMaster_FullDetails = e.WagesMaster.FullDetails == null ? "" : e.WagesMaster.FullDetails,
                       State_Id = e.LWFStates.Id == null ? 0 : e.LWFStates.Id,
                       State_Details = e.LWFStates.FullDetails == null ? "" : e.LWFStates.FullDetails,
                       Action = e.DBTrack.Action
                   }).ToList();


                //var add_data = db.LWFMaster
                //     .Include(e => e.LWFStates)
                //     .Include(e => e.WagesMaster)
                //     .Include(e => e.LWFStatutoryEffectiveMonths)
                //     .Where(e => e.Id == data)
                //    .Select(e => new
                //    {
                //        WagesMaster_Id = e.WagesMaster.Id == null ? 0 : e.WagesMaster.Id,
                //        WagesMaster_FullDetails = e.WagesMaster.FullDetails == null ? "" : e.WagesMaster.FullDetails,
                //        State_Id = e.LWFStates.Id == null ? 0 : e.LWFStates.Id,
                //        State_Details = e.LWFStates.FullDetails == null ? "" : e.LWFStates.FullDetails, 
                //    }).ToList();

                List<StatutoryEffectiveMonthsList> StatutoryEffectiveMonths = new List<StatutoryEffectiveMonthsList>();
                var KSEF = db.LWFMaster.Include(e => e.LWFStatutoryEffectiveMonths).Include(z => z.LWFStatutoryEffectiveMonths.Select(e => e.EffectiveMonth)).Where(e => e.Id == data).SingleOrDefault();
                if (KSEF != null)
                {
                    StatutoryEffectiveMonths.Add(new StatutoryEffectiveMonthsList
                    {
                        StatutoryEffectiveMonths_Id = KSEF.LWFStatutoryEffectiveMonths.Select(e => e.Id.ToString()).ToArray(),
                        StatutoryEffectiveMonths_Details = KSEF.LWFStatutoryEffectiveMonths.Select(e => e.EffectiveMonth.LookupVal).ToArray()
                    });
                }

                var Corp = db.LWFMaster.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, StatutoryEffectiveMonths, Auth, JsonRequestBehavior.AllowGet });
            }
        }

        [HttpPost]
        public async Task<ActionResult> EditSave(LWFMaster LWF, int data, FormCollection form) // Edit submit
        {
             List<string> Msg = new List<string>();
             using (DataBaseContext db = new DataBaseContext())
             {
                 try
                 {
                     string EffectiveDate = form["EffectiveDate"] == "0" ? "" : form["EffectiveDate"];
                     //string EndDate = form["EndDate"] == "0" ? "" : form["EndDate"];
                     string LWFWagesRangelist = form["LWFWagesRangelist"] == "0" ? "" : form["LWFWagesRangelist"];
                     string StateList = form["StateList"] == "0" ? "" : form["StateList"];
                     string StatutoryEffectiveMonthsList = form["StatutoryEffectiveMonthsList"] == "0" ? "" : form["StatutoryEffectiveMonthsList"];
                     string WagesMasterList = form["WagesMasterList"] == "0" ? "" : form["WagesMasterList"];
                     //  bool Auth = form["Autho_Action"] == "" ? false : true;
                     bool Auth = form["Autho_Allow"] == "true" ? true : false;
                     if (StateList == null)
                     {
                         Msg.Add("LWFStates is required.");
                         return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                         // return this.Json(new Object[] { "", "", "LWFStates is required.", JsonRequestBehavior.AllowGet });
                     }
                     if (WagesMasterList == null)
                     {
                         Msg.Add("WagesMaster is required.");
                         return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                         //return this.Json(new Object[] { "", "", "WagesMaster is required.", JsonRequestBehavior.AllowGet });
                     }
                     if (StatutoryEffectiveMonthsList == null)
                     {
                         //return this.Json(new Object[] { "", "", "LWFStatutoryEffectiveMonths is required.", JsonRequestBehavior.AllowGet });
                         Msg.Add("LWFStatutoryEffectiveMonths is required.");
                         return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                     }

                     LWF.LWFStates_Id = StateList != "0" && StateList != "" && StateList != null ? int.Parse(StateList) : 0;
                     LWF.WagesMaster_Id = WagesMasterList != null && WagesMasterList != "" ? int.Parse(WagesMasterList) : 0;
                      
                    
                     if (Auth == false)
                     {
                         if (ModelState.IsValid)
                         {
                             try
                             {
                                 using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                 {
                                     LWFMaster typedetails = null;

                                     List<StatutoryEffectiveMonths> StatutoryEffectiveMonths = new List<StatutoryEffectiveMonths>();
                                     typedetails = db.LWFMaster.Include(e => e.LWFStatutoryEffectiveMonths).Where(e => e.Id == data).SingleOrDefault();
                                     if (StatutoryEffectiveMonthsList != null && StatutoryEffectiveMonthsList != "")
                                     {
                                         var ids = Utility.StringIdsToListIds(StatutoryEffectiveMonthsList);
                                         foreach (var ca in ids)
                                         {
                                             var StatutoryEffectiveMonthsList_val = db.StatutoryEffectiveMonths.Find(ca);
                                             StatutoryEffectiveMonths.Add(StatutoryEffectiveMonthsList_val);
                                             typedetails.LWFStatutoryEffectiveMonths = StatutoryEffectiveMonths;
                                         }
                                     }
                                     else
                                     {
                                         typedetails.LWFStatutoryEffectiveMonths = null;
                                     }

                                      
                                   
                                     db.LWFMaster.Attach(typedetails);
                                     db.Entry(typedetails).State = System.Data.Entity.EntityState.Modified;
                                     db.SaveChanges();
                                     TempData["RowVersion"] = typedetails.RowVersion; 


                                     var Curr_OBJ = db.LWFMaster.Find(data);
                                     TempData["CurrRowVersion"] = Curr_OBJ.RowVersion;

                                     if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                     {
                                         LWFMaster blog = blog = null;
                                         DbPropertyValues originalBlogValues = null;

                                         blog = db.LWFMaster.Where(e => e.Id == data).SingleOrDefault();
                                         originalBlogValues = db.Entry(blog).OriginalValues;

                                         LWF.DBTrack = new DBTrack
                                         {
                                             CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                             CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                             Action = "M",
                                             ModifiedBy = SessionManager.UserName,
                                             ModifiedOn = DateTime.Now
                                         };

                                         Curr_OBJ.WagesMaster_Id = LWF.WagesMaster_Id != 0 ? LWF.WagesMaster_Id : null;
                                         Curr_OBJ.LWFStates_Id = LWF.LWFStates_Id != 0 ? LWF.LWFStates_Id : null;
                                         Curr_OBJ.Id = data;
                                         Curr_OBJ.DBTrack = LWF.DBTrack;
                                         Curr_OBJ.EffectiveDate = DateTime.Parse(EffectiveDate);

                                         db.Entry(Curr_OBJ).State = System.Data.Entity.EntityState.Modified;
                                         db.SaveChanges();
                                         ts.Complete();
                                         Msg.Add("  Record Updated");
                                         return Json(new Utility.JsonReturnClass { Id = Curr_OBJ.Id, Val = Curr_OBJ.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                     }
                                 }

                             }
                             catch (DbUpdateConcurrencyException ex)
                             {
                                 var entry = ex.Entries.Single();
                                 var clientValues = (Unit)entry.Entity;
                                 var databaseEntry = entry.GetDatabaseValues();
                                 if (databaseEntry == null)
                                 {
                                     Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                     return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                     // return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                 }
                                 else
                                 {
                                     var databaseValues = (LWFMaster)databaseEntry.ToObject();
                                     LWF.RowVersion = databaseValues.RowVersion;

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

                             LWFMaster blog = null; // to retrieve old data
                             DbPropertyValues originalBlogValues = null;
                             LWFMaster Old_Corp = null;

                             using (var context = new DataBaseContext())
                             {
                                 blog = context.LWFMaster.Where(e => e.Id == data).SingleOrDefault();
                                 originalBlogValues = context.Entry(blog).OriginalValues;
                             }
                             LWF.DBTrack = new DBTrack
                             {
                                 CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                 CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                 Action = "M",
                                 IsModified = blog.DBTrack.IsModified == true ? true : false,
                                 ModifiedBy = SessionManager.UserName,
                                 ModifiedOn = DateTime.Now
                             };
                             LWFMaster lwfmaster = new LWFMaster()
                             {
                                 //LWFWageRange = LWF.LWFWageRange,
                                 LWFStates = LWF.LWFStates,
                                 LWFStatutoryEffectiveMonths = LWF.LWFStatutoryEffectiveMonths,
                                 Id = data,
                                 DBTrack = LWF.DBTrack,
                                 RowVersion = (Byte[])TempData["RowVersion"]
                             };

                             using (var context = new DataBaseContext())
                             {
                                 var obj = DBTrackFile.ModifiedDataHistory("Payroll/Payroll", "M", blog, lwfmaster, "CPIRule", LWF.DBTrack);
                                 Old_Corp = context.LWFMaster.Where(e => e.Id == data)
                                     .Include(e => e.LWFStates).Include(e => e.LWFStatutoryEffectiveMonths).Include(e => e.WagesMaster).SingleOrDefault();
                                 DT_LWFMaster DT_Corp = (DT_LWFMaster)obj;
                                 DT_Corp.WagesMaster_Id = DBTrackFile.ValCompare(Old_Corp.WagesMaster, LWF.WagesMaster);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
                                 //DT_Corp.PayScale_Id = DBTrackFile.ValCompare(Old_Corp.PayScale, c.PayScale); //Old_Corp.PayScale == c.PayScale ? 0 : Old_Corp.PayScale == null && c.PayScale != null ? c.PayScale.Id : Old_Corp.PayScale.Id;

                                 db.Create(DT_Corp);

                             }
                             blog.DBTrack = LWF.DBTrack;
                             db.LWFMaster.Attach(blog);
                             db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                             db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                             db.SaveChanges();
                             ts.Complete();
                             Msg.Add("  Record Updated");
                             return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = LWF.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                             // return Json(new Object[] { blog.Id, LWF.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });
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

                 return View();
             }
        }


        #endregion

        #region Delete

        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        { 
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {

                    LWFMaster lwfmaster = db.LWFMaster.Include(e => e.WagesMaster)
                                                       .Include(e => e.LWFStatutoryEffectiveMonths)
                                                       .Include(e => e.LWFStates)
                                                       .Where(e => e.Id == data).SingleOrDefault();

                    //Corporate corporates = db.Corporate.Where(e => e.Id == data).SingleOrDefault();
                    var id = int.Parse(Session["CompId"].ToString());
                    var companypayroll = db.CompanyPayroll.Where(e => e.Company.Id == id).SingleOrDefault();
                    companypayroll.LWFMaster.Where(e => e.Id == lwfmaster.Id);
                    companypayroll.LWFMaster = null;
                    db.Entry(companypayroll).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();

                    Msg.Add("  Data removed successfully.  ");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                    if (lwfmaster.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack, corporates, null, "Corporate");
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = lwfmaster.DBTrack.CreatedBy != null ? lwfmaster.DBTrack.CreatedBy : null,
                                CreatedOn = lwfmaster.DBTrack.CreatedOn != null ? lwfmaster.DBTrack.CreatedOn : null,
                                IsModified = lwfmaster.DBTrack.IsModified == true ? true : false
                            };
                            lwfmaster.DBTrack = dbT;
                            db.Entry(lwfmaster).State = System.Data.Entity.EntityState.Modified;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, lwfmaster.DBTrack);
                            DT_LWFMaster DT_Corp = (DT_LWFMaster)rtn_Obj;
                            //DT_Corp.Address_Id = corporates.Address == null ? 0 : corporates.Address.Id;
                            //DT_Corp.BusinessType_Id = corporates.BusinessType == null ? 0 : corporates.BusinessType.Id;
                            //DT_Corp.ContactDetails_Id = corporates.ContactDetails == null ? 0 : corporates.ContactDetails.Id;
                            //db.Create(DT_Corp);
                            // db.SaveChanges();
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack);
                            await db.SaveChangesAsync();
                            //using (var context = new DataBaseContext())
                            //{
                            //   // DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack );
                            //}
                            ts.Complete();
                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
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
                                    CreatedBy = lwfmaster.DBTrack.CreatedBy != null ? lwfmaster.DBTrack.CreatedBy : null,
                                    CreatedOn = lwfmaster.DBTrack.CreatedOn != null ? lwfmaster.DBTrack.CreatedOn : null,
                                    IsModified = lwfmaster.DBTrack.IsModified == true ? false : false//,
                                    //AuthorizedBy = SessionManager.UserName,
                                    //AuthorizedOn = DateTime.Now
                                };

                                // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };

                                db.Entry(lwfmaster).State = System.Data.Entity.EntityState.Deleted;
                                var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, dbT);
                                //DT_LWFMaster DT_Corp = (DT_LWFMaster)rtn_Obj;
                                //DT_Corp.Address_Id = add == null ? 0 : add.Id;
                                //DT_Corp.BusinessType_Id = val == null ? 0 : val.Id;
                                //DT_Corp.ContactDetails_Id = conDet == null ? 0 : conDet.Id;
                                //db.Create(DT_Corp);

                                await db.SaveChangesAsync();


                                ts.Complete();
                                //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                                Msg.Add("  Data removed successfully.  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                            catch (RetryLimitExceededException /* dex */)
                            {
                                Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                //return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
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

        #endregion



        #endregion

        #region P2BGride
        public class P2BGridData
        {
            public int Id { get; set; }

            public string WagesMaster { get; set; }
            public string State { get; set; }
        }


        public ActionResult P2BGrid(P2BGrid_Parameters gp)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                    int pageSize = gp.rows;
                    int totalPages = 0;
                    int totalRecords = 0;
                    var jsonData = (Object)null;

                    IEnumerable<P2BGridData> ESICMASTERList = null;
                    List<P2BGridData> model = new List<P2BGridData>();
                    P2BGridData view = null;


                    //var BindEmpList = db.EmployeePayroll.Include(e => e.Employee.EmpName).Include(e => e.SalaryT)
                    //    .Where(e => e.SalaryT.Any(u => u.PayMonth != "01/2017")).ToList();
                    int company_Id = 0;
                    company_Id = Convert.ToInt32(Session["CompId"]);

                    var BindCompList = db.CompanyPayroll.Include(e => e.LWFMaster).Include(e => e.LWFMaster.Select(t => t.LWFStates))
                                      .Include(e => e.LWFMaster.Select(t => t.WagesMaster))
                                      .Where(e => e.Company.Id == company_Id).ToList();

                    foreach (var z in BindCompList)
                    {
                        if (z.LWFMaster != null)
                        {

                            foreach (var E in z.LWFMaster)
                            {
                                //var aa = db.Calendar.Where(e => e.Id == Sal.Id).SingleOrDefault();
                                view = new P2BGridData()
                                {
                                    Id = E.Id,
                                    WagesMaster = E.WagesMaster != null ? Convert.ToString(E.WagesMaster.FullDetails) : "",
                                    State = E.LWFStates != null ? Convert.ToString(E.LWFStates.FullDetails) : ""

                                };
                                model.Add(view);

                            }
                        }

                    }

                    ESICMASTERList = model;

                    IEnumerable<P2BGridData> IE;
                    if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                    {
                        IE = ESICMASTERList;
                        if (gp.searchOper.Equals("eq"))
                        {
                            //if (gp.searchField == "Id")
                                jsonData = IE.Where(e => (e.Id.ToString().Contains(gp.searchString))
                               || (e.WagesMaster.ToUpper().Contains(gp.searchString.ToUpper())
                               || (e.State.ToUpper().Contains(gp.searchString.ToUpper())))
                                    ).Select(a => new Object[] { a.WagesMaster, a.State, a.Id }).ToList();
                            //jsonData = IE.Select(a => new  { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] {  a.WagesMaster, a.State ,a.Id }).ToList();
                        }
                        totalRecords = IE.Count();
                    }
                    else
                    {
                        IE = ESICMASTERList;
                        Func<P2BGridData, dynamic> orderfuc;
                        if (gp.sidx == "Id")
                        {
                            orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                        }
                        else
                        {
                            orderfuc = (c => gp.sidx == "WagesMaster" ? c.WagesMaster.ToString() :
                                             gp.sidx == "State" ? c.State.ToString() : "");
                        }
                        if (gp.sord == "asc")
                        {
                            IE = IE.OrderBy(orderfuc);
                            jsonData = IE.Select(a => new Object[] {  a.WagesMaster, a.State, a.Id }).ToList();
                        }
                        else if (gp.sord == "desc")
                        {
                            IE = IE.OrderByDescending(orderfuc);
                            jsonData = IE.Select(a => new Object[] {  a.WagesMaster, a.State, a.Id }).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] {  a.WagesMaster, a.State, a.Id}).ToList();
                        }
                        totalRecords = ESICMASTERList.Count();
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
        #endregion
        }
    } 
}