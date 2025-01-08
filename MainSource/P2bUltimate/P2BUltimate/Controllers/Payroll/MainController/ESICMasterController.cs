
///
/// Created by Tanushri
///

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
    public class ESICMasterController : Controller
    {
        //  private DataBaseContext db = new DataBaseContext();
        #region PageLinks
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/ESICMaster/Index.cshtml");
        }

        public ActionResult Wages_partial()
        {
            return View("~/Views/Shared/_Wages.cshtml");
        }

        public ActionResult StatutoryEffectiveMonthspartial()
        {
            return View("~/Views/Shared/Payroll/_StatutoryEffectiveMonths.cshtml");
        }

        public ActionResult Range_partial()
        {
            return View("~/Views/Shared/_Range.cshtml");
        }
        #endregion


        #region LookUpValueFillUp
        public ActionResult GetWagesDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.Wages.ToList();
                IEnumerable<Wages> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.Wages.ToList().Where(d => d.FullDetails.Contains(data));
                }
                else
                {
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.FullDetails }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetRangeDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.Range.ToList();
                IEnumerable<Range> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.Range.ToList().Where(d => d.FullDetails.Contains(data));
                }
                else
                {
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.FullDetails }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetStatutoryEffectiveMonths(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.StatutoryEffectiveMonths.Include(e => e.EffectiveMonth).ToList();
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.StatutoryEffectiveMonths.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                    }
                }
                var list1 = db.ESICMaster.Include(e => e.ESICStatutoryEffectiveMonths).SelectMany(e => e.ESICStatutoryEffectiveMonths).ToList();
                var list2 = fall.Except(list1);
                var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.EffectiveMonth.LookupVal }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetLoactionObjDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.Location.Include(e => e.LocationObj).ToList();
                IEnumerable<Location> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.Location.Include(e => e.LocationObj).ToList().Where(d => d.LocationObj.LocDesc.Contains(data));
                }
                else
                {
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.LocationObj.LocDesc }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.LocationObj.LocDesc }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region CRUD OPERATION
        #region CREATE
        [HttpPost]
        public ActionResult Create(ESICMaster E, FormCollection form)
        {
            var Msg = new List<String>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string EffectiveDate = form["EffectiveDate"] == "0" ? "" : form["EffectiveDate"];
                    string StatutoryEffectiveMonthsList = form["StatutoryEffectiveMonthsList"] == "0" ? "" : form["StatutoryEffectiveMonthsList"];
                    string WageMasterPayList = form["WageMasterPayList"] == "0" ? "" : form["WageMasterPayList"];
                    string WageMasterQualifyList = form["WageMasterQualifyList"] == "0" ? "" : form["WageMasterQualifyList"];
                    string LocationList_DDL = form["Locationlist"] == "0" ? "" : form["Locationlist"];
                    var id = int.Parse(Session["CompId"].ToString());
                    var companypayroll = db.CompanyPayroll.Where(e => e.Company.Id == id).SingleOrDefault();

                    if (!string.IsNullOrEmpty(EffectiveDate))
                    {
                        var val = DateTime.Parse(EffectiveDate);
                        E.EffectiveDate = val;

                    }
                    List<Location> location = new List<Location>();
                    if (!string.IsNullOrEmpty(LocationList_DDL))
                    {
                        var ids = Utility.StringIdsToListIds(LocationList_DDL);
                        foreach (var ca in ids)
                        {
                            var val = db.Location.Find(ca);
                            location.Add(val);
                        }
                        E.Location = location;
                    }


                    List<StatutoryEffectiveMonths> StatutoryEffectiveMonths = new List<StatutoryEffectiveMonths>();
                    if (!string.IsNullOrEmpty(StatutoryEffectiveMonthsList))
                    {
                        var ids = Utility.StringIdsToListIds(StatutoryEffectiveMonthsList);
                        foreach (var ca in ids)
                        {
                            var StatutoryEffectiveMonthsList_val = db.StatutoryEffectiveMonths.Find(ca);
                            StatutoryEffectiveMonths.Add(StatutoryEffectiveMonthsList_val);
                        }
                        E.ESICStatutoryEffectiveMonths = StatutoryEffectiveMonths;
                    }

                    if (!string.IsNullOrEmpty(WageMasterPayList))
                    {
                        int WagesId = Convert.ToInt32(WageMasterPayList);
                        var val = db.Wages.Where(e => e.Id == WagesId).SingleOrDefault();
                        E.WageMasterPay = val;

                    }


                    if (!string.IsNullOrEmpty(WageMasterQualifyList))
                    {
                        int WagesId = Convert.ToInt32(WageMasterQualifyList);
                        var val = db.Wages.Where(e => e.Id == WagesId).SingleOrDefault();
                        E.WageMasterQualify = val;
                    }


                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {

                            E.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            ESICMaster ESICMaster = new ESICMaster()
                            {
                                EffectiveDate = E.EffectiveDate,
                                Location = E.Location,
                                ESICStatutoryEffectiveMonths = E.ESICStatutoryEffectiveMonths,
                                WageMasterPay = E.WageMasterPay,
                                WageMasterQualify = E.WageMasterQualify,
                                DBTrack = E.DBTrack
                            };
                            try
                            {
                                db.ESICMaster.Add(ESICMaster);
                                //var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, E.DBTrack);
                                //DT_ESICMaster DT_OBJ = (DT_ESICMaster)rtn_Obj;
                                //DT_OBJ.WageMasterPay_Id = E.WageMasterPay == null ? 0 : E.WageMasterPay.Id;
                                //DT_OBJ.WageMasterQualify_Id = E.WageMasterQualify == null ? 0 : E.WageMasterQualify.Id;                               
                                //db.Create(DT_OBJ);
                                db.SaveChanges();
                                if (companypayroll != null)
                                {
                                    List<ESICMaster> pfmasterlist = new List<ESICMaster>();
                                    pfmasterlist.Add(ESICMaster);
                                    companypayroll.ESICMaster = pfmasterlist;
                                    db.Entry(companypayroll).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    db.Entry(companypayroll).State = System.Data.Entity.EntityState.Detached;
                                }

                                ts.Complete();
                                Msg.Add("  Data Created successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = ESICMaster.Id, Val = ESICMaster.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // return this.Json(new Object[] { , , "Data Created Successfully.", JsonRequestBehavior.AllowGet });
                            }

                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = E.Id });
                            }
                            catch (DataException /* dex */)
                            {
                                Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Utility.JsonReturnClass { success = false, responseText = "Unable to create. Try again, and if the problem persists contact your system administrator.." }, JsonRequestBehavior.AllowGet);
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
        #endregion

        #region EDIT & EDITSAVE
        public class ESICMASTERICOLLECTION
        {
            public Array StatutoryEffectiveMonths_Id { get; set; }
            public Array StatutoryEffectiveMonths_Details { get; set; }
            public Array Location_Id { get; set; }
            public Array LoactionFulldetails { get; set; }
        };

        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.ESICMaster
                  .Include(e => e.WageMasterPay)
                  .Include(e => e.WageMasterQualify)
                  .Include(e => e.ESICStatutoryEffectiveMonths)
                  .Where(e => e.Id == data).Select
                  (e => new
                  {
                      EffectiveDate = e.EffectiveDate,                       
                      WageMasterPay_Id = e.WageMasterPay == null ? 0 : e.WageMasterPay.Id,
                      WageMasterPay_FullDetails = e.WageMasterPay.FullDetails == null ? "" : e.WageMasterPay.FullDetails,
                      WagesMasterQualify_Id = e.WageMasterQualify == null ? 0 : e.WageMasterQualify.Id,
                      WageMasterQualify_FullDetails = e.WageMasterQualify.FullDetails == null ? "" : e.WageMasterQualify.FullDetails,
                      Action = e.DBTrack.Action
                  }).ToList();


                List<ESICMASTERICOLLECTION> ESICMASTERICOLLECTION = new List<ESICMASTERICOLLECTION>();
                var KSEF = db.ESICMaster.Include(e => e.ESICStatutoryEffectiveMonths).Include(z => z.ESICStatutoryEffectiveMonths.Select(e => e.EffectiveMonth)).Where(e => e.Id == data).ToList();
                if (KSEF != null)
                {
                    foreach (var ca in KSEF)
                    {
                        ESICMASTERICOLLECTION.Add(new ESICMASTERICOLLECTION
                        {
                            StatutoryEffectiveMonths_Id = ca.ESICStatutoryEffectiveMonths.Select(e => e.Id.ToString()).ToArray(),
                            StatutoryEffectiveMonths_Details = ca.ESICStatutoryEffectiveMonths.Select(e => e.EffectiveMonth.LookupVal).ToArray()

                        });
                    }
                }

                var LOBJ = db.ESICMaster.Include(e => e.Location).Include(e=>e.Location.Select(r=>r.LocationObj)).Where(e => e.Id == data).ToList();

                if (LOBJ != null)
                {
                    foreach (var l in LOBJ)
                    {
                        ESICMASTERICOLLECTION.Add(new ESICMASTERICOLLECTION
                        {
                            Location_Id = l.Location.Select(r=>r.Id).ToArray(),
                            LoactionFulldetails = l.Location.Select(r=>r.LocationObj.LocDesc).ToArray()
                        });
                    }
                }

                var W = db.DT_ESICMaster
                     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id,
                         Wages_PayVal = e.WageMasterPay_Id == 0 ? "" : db.Wages.Where(x => x.Id == e.WageMasterPay_Id).Select(x => x.WagesName).FirstOrDefault(),
                         Wages_QualifyVal = e.WageMasterQualify_Id == 0 ? "" : db.Wages.Where(x => x.Id == e.WageMasterQualify_Id).Select(x => x.WagesName).FirstOrDefault(),
                         //Range_Val = e.Range_Id == 0 ? "" : db.Range.Where(x => x.Id == e.Range_Id).Select(x => x.FullDetails).FirstOrDefault(),
                         //Range_Val = e.Range_Id == null ? "" : db.Range.Where(x => x.Id == e.Range_Id).Select(x => x.Id).FirstOrDefault(),
                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();
                var LKup = db.ESICMaster.Find(data);
                TempData["RowVersion"] = LKup.RowVersion;
                var Auth = LKup.DBTrack.IsModified;

                return this.Json(new Object[] { Q, ESICMASTERICOLLECTION, W, Auth, JsonRequestBehavior.AllowGet });
            }
        }

        [HttpPost]
        public async Task<ActionResult> EditSave(ESICMaster E, FormCollection form, int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;
                    string EffectiveDate = form["EffectiveDate"] == "0" ? "" : form["EffectiveDate"];       
                    string StatutoryEffectiveMonthsList = form["StatutoryEffectiveMonthsList"] == "0" ? "" : form["StatutoryEffectiveMonthsList"];
                    string WageMasterPayList = form["WageMasterPayList"] == "0" ? "" : form["WageMasterPayList"];
                    string WageMasterQualifyList = form["WageMasterQualifyList"] == "0" ? "" : form["WageMasterQualifyList"];
                    string LocationList_DDL = form["Locationlist"] == "0" ? "" : form["Locationlist"];

                    if (!string.IsNullOrEmpty(EffectiveDate))
                    {
                        var val = DateTime.Parse(EffectiveDate);
                        E.EffectiveDate = val;

                    }

                    if (!string.IsNullOrEmpty(WageMasterPayList))
                    {
                        int WagesId = Convert.ToInt32(WageMasterPayList);
                        var val = db.Wages.Where(e => e.Id == WagesId).SingleOrDefault();
                        E.WageMasterPay_Id = val.Id;
                    }

                    if (!string.IsNullOrEmpty(WageMasterQualifyList))
                    {
                        int WagesId = Convert.ToInt32(WageMasterQualifyList);
                        var val = db.Wages.Where(e => e.Id == WagesId).SingleOrDefault();
                        E.WageMasterQualify_Id = val.Id;

                    }
                   
                   
                    List<StatutoryEffectiveMonths> dbStatutoryEffectiveMonthslist = new List<StatutoryEffectiveMonths>();
                   
                    if (!string.IsNullOrEmpty(StatutoryEffectiveMonthsList))
                    {
                        var ids = Utility.StringIdsToListIds(StatutoryEffectiveMonthsList);
                        foreach (var ca in ids)
                        {
                            var StatutoryEffectiveMonthsList_val = db.StatutoryEffectiveMonths.Find(ca);
                            dbStatutoryEffectiveMonthslist.Add(StatutoryEffectiveMonthsList_val);
                        }                        
                    }

                    List<Location> dbLocationlist = new List<Location>();
                   
                    if (!string.IsNullOrEmpty(LocationList_DDL))
                    {
                        var ids = Utility.StringIdsToListIds(LocationList_DDL);
                        foreach (var ca in ids)
                        {
                            var val = db.Location.Find(ca);
                            dbLocationlist.Add(val);                           
                        }
                    }

                    if (Auth == false)
                    {
                        if (ModelState.IsValid)
                        {
                            try
                            {
                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    ESICMaster blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.ESICMaster.Where(e => e.Id == data)
                                                                .Include(e => e.ESICStatutoryEffectiveMonths)
                                                                .Include(e => e.WageMasterPay)
                                                                .Include(e => e.WageMasterQualify)
                                                                .SingleOrDefault();
                                        originalBlogValues = context.Entry(blog).OriginalValues;
                                    }

                                    E.DBTrack = new DBTrack
                                    {
                                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };

                                    ESICMaster typedetails = db.ESICMaster.Include(e=>e.Location).Include(e => e.ESICStatutoryEffectiveMonths).Where(e => e.Id == data).SingleOrDefault();
                                    typedetails.Id = data;
                                    typedetails.EffectiveDate = E.EffectiveDate;
                                    typedetails.Location = dbLocationlist;
                                    typedetails.ESICStatutoryEffectiveMonths = dbStatutoryEffectiveMonthslist;
                                    typedetails.WageMasterPay_Id = E.WageMasterPay_Id;
                                    typedetails.WageMasterQualify_Id = E.WageMasterQualify_Id;

                                    db.ESICMaster.Attach(typedetails);
                                    db.Entry(typedetails).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    TempData["RowVersion"] = typedetails.RowVersion;
                                    db.Entry(typedetails).State = System.Data.Entity.EntityState.Detached;


                                    //var Curr_OBJ = db.ESICMaster.Find(data);
                                    //TempData["CurrRowVersion"] = Curr_OBJ.RowVersion;
                                    //db.Entry(Curr_OBJ).State = System.Data.Entity.EntityState.Detached;


                                    //using (var context = new DataBaseContext())
                                    //{
                                    //    var obj = DBTrackFile.DBTrackSave("Payroll/Payroll", originalBlogValues, db.ChangeTracker, E.DBTrack);
                                    //    //DT_ESICMaster DT_OBJ = (DT_ESICMaster)obj;
                                    //    //DT_OBJ.Wage_Id = blog.WageMasterPay == null ? 0 : blog.WageMasterPay.Id;
                                    //    //DT_OBJ.Wage_Id = blog.WageMasterQualify == null ? 0 : blog.WageMasterQualify.Id;
                                    //    //db.Create(DT_OBJ);
                                    //    db.SaveChanges();
                                    //}
                                    await db.SaveChangesAsync();
                                    ts.Complete();
                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = E.Id, Val = E.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    // return Json(new Object[] {,, "Record Updated", JsonRequestBehavior.AllowGet });

                                }
                            }

                            //catch (DbUpdateException e) { throw e; }
                            //catch (DataException e) { throw e; }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (ESICMaster)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (ESICMaster)databaseEntry.ToObject();
                                    E.RowVersion = databaseValues.RowVersion;

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
                            // return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
                        }
                    }


                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            ESICMaster blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            ESICMaster Old_Obj = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.ESICMaster.Where(e => e.Id == data).SingleOrDefault();
                                originalBlogValues = context.Entry(blog).OriginalValues;
                            }
                            E.DBTrack = new DBTrack
                            {
                                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                Action = "M",
                                IsModified = blog.DBTrack.IsModified == true ? true : false,
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };
                            ESICMaster corp = new ESICMaster()
                            {
                                Id = data,
                                EffectiveDate = E.EffectiveDate,
                                //EndDate  = E.EndDate,
                                ESICStatutoryEffectiveMonths = E.ESICStatutoryEffectiveMonths,
                                WageMasterPay = E.WageMasterPay,
                                WageMasterQualify = E.WageMasterQualify,
                                DBTrack = E.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };


                            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Payroll/Payroll", "M", blog, corp, "ESICMaster", E.DBTrack);
                                // var obj = DBTrackFile.DBTrackSave("CPayroll/Payroll", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                Old_Obj = context.ESICMaster.Where(e => e.Id == data).Include(e => e.WageMasterPay)
                                    .Include(e => e.WageMasterQualify).SingleOrDefault();
                                DT_ESICMaster DT_OBJ = (DT_ESICMaster)obj;
                                DT_OBJ.WageMasterPay_Id = DBTrackFile.ValCompare(Old_Obj.WageMasterPay, E.WageMasterPay);//Old_Obj.Address == c.Address ? 0 : Old_Obj.Address == null && c.Address != null ? c.Address.Id : Old_Obj.Address.Id;
                                DT_OBJ.WageMasterQualify_Id = DBTrackFile.ValCompare(Old_Obj.WageMasterQualify, E.WageMasterQualify);

                                db.Create(DT_OBJ);
                                //db.SaveChanges();
                            }
                            blog.DBTrack = E.DBTrack;
                            db.ESICMaster.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = E.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { ,, "Record Updated", JsonRequestBehavior.AllowGet });
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

        public int EditS(string FUNC, int data, ESICMaster ESOBJ, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (FUNC != null)
                {
                    if (FUNC != "")
                    {
                        var val = db.Wages.Find(int.Parse(FUNC));
                        ESOBJ.WageMasterPay = val;

                        var add = db.ESICMaster.Include(e => e.WageMasterPay).Where(e => e.Id == data).SingleOrDefault();
                        IList<ESICMaster> Wage = null;
                        if (add.WageMasterPay != null)
                        {
                            Wage = db.ESICMaster.Where(x => x.WageMasterPay.Id == add.WageMasterPay.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            Wage = db.ESICMaster.Where(x => x.Id == data).ToList();
                        }
                        foreach (var s in Wage)
                        {
                            s.WageMasterPay = ESOBJ.WageMasterPay;
                            db.ESICMaster.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                }
                else
                {
                    var Wage = db.ESICMaster.Include(e => e.WageMasterPay).Where(x => x.Id == data).ToList();
                    foreach (var s in Wage)
                    {
                        s.WageMasterPay = null;
                        db.ESICMaster.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }

                var CurOBJ = db.ESICMaster.Find(data);
                TempData["CurrRowVersion"] = CurOBJ.RowVersion;
                db.Entry(CurOBJ).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    ESOBJ.DBTrack = dbT;
                    ESICMaster ESIOBJ = new ESICMaster()
                    {
                        Id = data,
                        FullDetails = ESOBJ.FullDetails,
                        DBTrack = ESOBJ.DBTrack
                    };

                    db.ESICMaster.Attach(ESIOBJ);
                    db.Entry(ESIOBJ).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(ESIOBJ).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    //// DBTrackFile.DBTrackSave("Payroll", originalBlogValues, db.ChangeTracker, c.DBTrack);
                    return 1;
                }
                return 0;
            }
        }


        #endregion

        #region DELETE
        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    ESICMaster ESICMaster = db.ESICMaster.Include(e => e.ESICStatutoryEffectiveMonths)
                    .Include(e => e.WageMasterPay)
                    .Include(e => e.WageMasterQualify)
                    .Where(e => e.Id == data).SingleOrDefault();

                    var id = int.Parse(Session["CompId"].ToString());
                    var companypayroll = db.CompanyPayroll.Where(e => e.Company.Id == id).SingleOrDefault();
                    companypayroll.ESICMaster.Where(e => e.Id == ESICMaster.Id);
                    companypayroll.ESICMaster = null;
                    db.Entry(companypayroll).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    Msg.Add("  Data removed successfully.  ");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });

                    if (ESICMaster.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = ESICMaster.DBTrack.CreatedBy != null ? ESICMaster.DBTrack.CreatedBy : null,
                                CreatedOn = ESICMaster.DBTrack.CreatedOn != null ? ESICMaster.DBTrack.CreatedOn : null,
                                IsModified = ESICMaster.DBTrack.IsModified == true ? true : false
                            };
                            ESICMaster.DBTrack = dbT;
                            db.Entry(ESICMaster).State = System.Data.Entity.EntityState.Modified;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, ESICMaster.DBTrack);
                            DT_ESICMaster DT_ESICMaster = (DT_ESICMaster)rtn_Obj;
                            //DT_ESICMaster.EffectMonths_Id = ESICMaster.ESICStatutoryEffectiveMonths.Select(e=>e.EffectiveMonth.Id) == null ? 0 : ESICMaster.ESICStatutoryEffectiveMonths.Select(e => e.EffectiveMonth.Id);
                            DT_ESICMaster.WageMasterPay_Id = ESICMaster.WageMasterPay == null ? 0 : ESICMaster.WageMasterPay.Id;
                            db.Create(DT_ESICMaster);
                            await db.SaveChangesAsync();
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
                                    CreatedBy = ESICMaster.DBTrack.CreatedBy != null ? ESICMaster.DBTrack.CreatedBy : null,
                                    CreatedOn = ESICMaster.DBTrack.CreatedOn != null ? ESICMaster.DBTrack.CreatedOn : null,
                                    IsModified = ESICMaster.DBTrack.IsModified == true ? false : false//,
                                };

                                db.Entry(ESICMaster).State = System.Data.Entity.EntityState.Deleted;
                                var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, dbT);
                                DT_ESICMaster DT_ESICMaster = (DT_ESICMaster)rtn_Obj;
                                DT_ESICMaster.WageMasterPay_Id = ESICMaster.WageMasterPay == null ? 0 : ESICMaster.WageMasterPay.Id;
                                db.Create(DT_ESICMaster);
                                await db.SaveChangesAsync();
                                ts.Complete();
                                Msg.Add("  Data removed successfully.  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });

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

        #region AuthoritySave
        [HttpPost]
        public async Task<ActionResult> AuthSave(int auth_id, Boolean isauth, String auth_action) //to save authorised data
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    if (auth_action == "C")
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            ESICMaster ESI = db.ESICMaster
                                .Include(e => e.WageMasterPay)
                                .Include(e => e.WageMasterQualify)
                                .Include(e => e.ESICStatutoryEffectiveMonths)
                                .FirstOrDefault(e => e.Id == auth_id);

                            ESI.DBTrack = new DBTrack
                            {
                                Action = "C",
                                ModifiedBy = ESI.DBTrack.ModifiedBy != null ? ESI.DBTrack.ModifiedBy : null,
                                CreatedBy = ESI.DBTrack.CreatedBy != null ? ESI.DBTrack.CreatedBy : null,
                                CreatedOn = ESI.DBTrack.CreatedOn != null ? ESI.DBTrack.CreatedOn : null,
                                IsModified = ESI.DBTrack.IsModified == true ? false : false,
                                AuthorizedBy = SessionManager.UserName,
                                AuthorizedOn = DateTime.Now
                            };

                            db.ESICMaster.Attach(ESI);
                            db.Entry(ESI).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(ESI).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            //db.SaveChanges();
                            var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, ESI.DBTrack);
                            DT_ESICMaster DT_OBJ = (DT_ESICMaster)rtn_Obj;

                            db.Create(DT_OBJ);
                            await db.SaveChangesAsync();

                            ts.Complete();
                            Msg.Add("  Record Authorised");
                            return Json(new Utility.JsonReturnClass { Id = ESI.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { ESI.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else if (auth_action == "M")
                    {

                        ESICMaster Old_OBJ = db.ESICMaster.Include(e => e.WageMasterPay).Include(e => e.WageMasterQualify).Include(e => e.ESICStatutoryEffectiveMonths).Where(e => e.Id == auth_id).SingleOrDefault();


                        DT_ESICMaster Curr_OBJ = db.DT_ESICMaster
                                                    .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                                    .OrderByDescending(e => e.Id)
                                                    .FirstOrDefault();

                        if (Curr_OBJ != null)
                        {
                            ESICMaster ESICMaster = new ESICMaster();
                            string Wage = Curr_OBJ.WageMasterPay_Id == null ? null : Curr_OBJ.WageMasterPay_Id.ToString();


                            ESICMaster.FullDetails = Curr_OBJ.FullDetails == null ? Old_OBJ.FullDetails : Curr_OBJ.FullDetails;

                            //      corp.Id = auth_id;

                            if (ModelState.IsValid)
                            {
                                try
                                {

                                    //DbContextTransaction transaction = db.Database.BeginTransaction();

                                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                    {
                                        // db.Configuration.AutoDetectChangesEnabled = false;
                                        ESICMaster.DBTrack = new DBTrack
                                        {
                                            CreatedBy = Old_OBJ.DBTrack.CreatedBy == null ? null : Old_OBJ.DBTrack.CreatedBy,
                                            CreatedOn = Old_OBJ.DBTrack.CreatedOn == null ? null : Old_OBJ.DBTrack.CreatedOn,
                                            Action = "M",
                                            ModifiedBy = Old_OBJ.DBTrack.ModifiedBy == null ? null : Old_OBJ.DBTrack.ModifiedBy,
                                            ModifiedOn = Old_OBJ.DBTrack.ModifiedOn == null ? null : Old_OBJ.DBTrack.ModifiedOn,
                                            AuthorizedBy = SessionManager.UserName,
                                            AuthorizedOn = DateTime.Now,
                                            IsModified = false
                                        };

                                        int a = EditS(Wage, auth_id, ESICMaster, ESICMaster.DBTrack);

                                        await db.SaveChangesAsync();

                                        ts.Complete();
                                        Msg.Add("  Record Authorised");
                                        return Json(new Utility.JsonReturnClass { Id = ESICMaster.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        // return Json(new Object[] { ESICMaster.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                                    }
                                }
                                catch (DbUpdateConcurrencyException ex)
                                {
                                    var entry = ex.Entries.Single();
                                    var clientValues = (ESICMaster)entry.Entity;
                                    var databaseEntry = entry.GetDatabaseValues();
                                    if (databaseEntry == null)
                                    {
                                        Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        // return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                    }
                                    else
                                    {
                                        var databaseValues = (ESICMaster)databaseEntry.ToObject();
                                        ESICMaster.RowVersion = databaseValues.RowVersion;
                                    }
                                }
                                Msg.Add("Record modified by another user.So refresh it and try to save again.");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                            }
                        }
                        else
                        {
                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                        //return Json(new Object[] { "", "Data removed from history", JsonRequestBehavior.AllowGet });
                    }
                    else if (auth_action == "D")
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //ESICMaster corp = db.ESICMaster.Find(auth_id);
                            ESICMaster ESI = db.ESICMaster.AsNoTracking().FirstOrDefault(e => e.Id == auth_id);

                            //Address add = corp.Address;
                            //ContactDetails conDet = corp.ContactDetails;
                            //SocialActivities val = corp.BusinessType;

                            ESI.DBTrack = new DBTrack
                            {
                                Action = "D",
                                ModifiedBy = ESI.DBTrack.ModifiedBy != null ? ESI.DBTrack.ModifiedBy : null,
                                CreatedBy = ESI.DBTrack.CreatedBy != null ? ESI.DBTrack.CreatedBy : null,
                                CreatedOn = ESI.DBTrack.CreatedOn != null ? ESI.DBTrack.CreatedOn : null,
                                IsModified = false,
                                AuthorizedBy = SessionManager.UserName,
                                AuthorizedOn = DateTime.Now
                            };

                            db.ESICMaster.Attach(ESI);
                            db.Entry(ESI).State = System.Data.Entity.EntityState.Deleted;


                            var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, ESI.DBTrack);
                            DT_ESICMaster DT_OBJ = (DT_ESICMaster)rtn_Obj;
                            //DT_OBJ.Address_Id = corp.Address == null ? 0 : corp.Address.Id;
                            //DT_OBJ.BusinessType_Id = corp.BusinessType == null ? 0 : corp.BusinessType.Id;
                            //DT_OBJ.ContactDetails_Id = corp.ContactDetails == null ? 0 : corp.ContactDetails.Id;
                            db.Create(DT_OBJ);
                            await db.SaveChangesAsync();
                            db.Entry(ESI).State = System.Data.Entity.EntityState.Detached;
                            ts.Complete();
                            Msg.Add(" Record Authorised ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { "", "Record Authorised", JsonRequestBehavior.AllowGet });
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

        #endregion

        #region DROPDOWNLISTPOPULATION
        public ActionResult PopulateDropDownListBC(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var qurey = db.Country.ToList();
                var selected = (Object)null;
                if (data2 != "" && data != "0" && data2 != "0")
                {
                    selected = Convert.ToInt32(data2);
                }

                SelectList s = new SelectList(qurey, "Id", "MaxSalary", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult PopulateDropDownListCC(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var qurey = db.Country.ToList();
                var selected = (Object)null;
                if (data2 != "" && data != "0" && data2 != "0")
                {
                    selected = Convert.ToInt32(data2);
                }

                SelectList s = new SelectList(qurey, "Id", "MaxSalary", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion


        #region P2BGRIDVIEW

        public class P2BGridData
        {
            public int Id { get; set; }

            public string WagesPay { get; set; }
            public string WagesQualify { get; set; }
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

                    var BindCompList = db.CompanyPayroll.Include(e => e.ESICMaster).Include(e => e.ESICMaster.Select(t => t.WageMasterPay))
                                      .Include(e => e.ESICMaster.Select(t => t.WageMasterQualify))
                                      .Where(e => e.Company.Id == company_Id).ToList();

                    foreach (var z in BindCompList)
                    {
                        if (z.ESICMaster != null)
                        {

                            foreach (var E in z.ESICMaster)
                            {
                                //var aa = db.Calendar.Where(e => e.Id == Sal.Id).SingleOrDefault();
                                view = new P2BGridData()
                                {
                                    Id = E.Id,
                                    WagesQualify = E.WageMasterQualify != null ? Convert.ToString(E.WageMasterQualify.FullDetails) : "",
                                    WagesPay = E.WageMasterPay != null ? Convert.ToString(E.WageMasterPay.FullDetails) : ""

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
                            jsonData = IE.Where(e => (e.WagesPay.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                             || (e.WagesQualify.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                             || (e.Id.ToString().Contains(gp.searchString))
                             ).Select(a => new Object[] { a.WagesPay, a.WagesQualify, a.Id }).ToList();

                            //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.WagesPay, a.WagesQualify, a.Id }).ToList();
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
                            orderfuc = (c => gp.sidx == "WagesPay" ? c.WagesPay.ToString() :
                                                gp.sidx == "WagesQualify" ? c.WagesQualify.ToString() : ""


                                            );
                        }
                        if (gp.sord == "asc")
                        {
                            IE = IE.OrderBy(orderfuc);
                            jsonData = IE.Select(a => new Object[] { a.WagesPay, a.WagesQualify, a.Id }).ToList();
                        }
                        else if (gp.sord == "desc")
                        {
                            IE = IE.OrderByDescending(orderfuc);
                            jsonData = IE.Select(a => new Object[] { a.WagesPay, a.WagesQualify, a.Id }).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.WagesPay, a.WagesQualify, a.Id }).ToList();
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








































//using P2b.Global;
//using P2BUltimate.App_Start;
//using System;
//using System.Collections.Generic;
//using System.Data.Entity.Infrastructure;
//using System.Linq;
//using System.Text;
//using System.Transactions;
//using System.Web;
//using System.Web.Mvc;
//using System.Data;
//using System.Data.Entity;
//using P2BUltimate.Models;
//using System.Web.Script.Serialization;
//using System.Threading.Tasks;
//using System.Collections;
//using System.Data.Entity.Core.Objects;
//using Payroll;


//namespace P2BUltimate.Controllers
//{
//    public class ESICController : Controller
//    {





//        private DataBaseContext db = new DataBaseContext();

//        public ActionResult Index()
//        {
//            return View();
//        }

//        [HttpPost]
//        // [ValidateAntiForgeryToken]
//        public ActionResult Create(ESICMaster NOBJ, FormCollection form)
//        {

//            if (ModelState.IsValid)
//            {
//                using (TransactionScope ts = new TransactionScope())
//                {
//                    if (db.ESICMaster.Any(o => o.MaxSalary == NOBJ.MaxSalary))
//                    {
//                        return Json(new Object[] { "", "", "Neg Sal Actname Already Exists.", JsonRequestBehavior.AllowGet });
//                    }

//                    //NOBJ.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName };



//                    ESICMaster ESICMaster = new ESICMaster()
//                    {
//                        MaxSalary = NOBJ.MaxSalary == null ? "" : NOBJ.MaxSalary.Trim(),
//                        Range = NOBJ.Range,
//                        Wage = NOBJ.Wage,

//                       // DBTrack = NOBJ.DBTrack
//                    };
//                    try
//                    {
//                        db.ESICMaster.Add(ESICMaster);
//                        //DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, NOBJ.DBTrack);
//                        db.SaveChanges();
//                        ts.Complete();
//                        return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
//                    }
//                    catch (DbUpdateConcurrencyException)
//                    {
//                        return RedirectToAction("Create", new { concurrencyError = true, id = NOBJ.Id });
//                    }
//                    catch (DataException /* dex */)
//                    {
//                         return Json(new Utility.JsonReturnClass { success = false, responseText = "Unable to create. Try again, and if the problem persists contact your system administrator.." }, JsonRequestBehavior.AllowGet);
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

//        [HttpPost]
//        public ActionResult Edit(int data)
//        {
//            //string tableName = "ESICMaster";

//            //    // Fetch the table records dynamically
//            //    var tableData = db.GetType()
//            //    .GetProperty(tableName)
//            //    .GetValue(db, null);

//            var Q = db.ESICMaster
//                .Where(e => e.Id == data);
//            var add_data = db.ESICMaster
//                .Where(e => e.fu == data);
//            //TempData["RowVersion"] = db.ESICMaster.Find(data).RowVersion;
//            return Json(new Object[] { Q, add_data, JsonRequestBehavior.AllowGet });
//        }

//        public ActionResult P2BGrid(P2BGrid_Parameters gp)
//        {
//            try
//            {
//                DataBaseContext db = new DataBaseContext();
//                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
//                int pageSize = gp.rows;
//                int totalPages = 0;
//                int totalRecords = 0;
//                var jsonData = (Object)null;
//                var ESICMaster = db.ESICMaster.ToList();


//                IEnumerable<ESICMaster> IE;
//                if (!string.IsNullOrEmpty(gp.searchField))
//                {
//                    IE = ESICMaster;
//                    if (gp.searchOper.Equals("eq"))
//                    {
//                        jsonData = IE.Select(a => new { a.Id, a.MaxSalary, a.Range, a.Wage }).Where((e => (e.Id.ToString() == gp.searchString) || (e.MaxSalary.ToString()== gp.searchString.ToLower()) || (e.Range.ToString() == gp.searchString) || (e.Wage.ToString() == gp.searchString))).ToList();
//                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.MaxSalary), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
//                    }
//                    if (pageIndex > 1)
//                    {
//                        int h = pageIndex * pageSize;
//                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.MaxSalary, a.Range, a.Wage }).ToList();
//                    }
//                    totalRecords = IE.Count();
//                }
//                else
//                {
//                    IE = ESICMaster;
//                    Func<ESICMaster, string> orderfuc = (c => gp.sidx == "Id" ? c.Id.ToString() :
//                                                               gp.sidx == "MaxSalary" ? c.MaxSalary.ToString() :
//                                                               gp.sidx == "Range" ? c.Range.ToString() :
//                                                                gp.sidx == "Wage" ? c.Wage.ToString() : ""
//                                                                );
//                    if (gp.sord == "asc")
//                    {
//                        IE = IE.OrderBy(orderfuc);
//                        jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.MaxSalary), Convert.ToString(a.Range), Convert.ToString(a.Wage) }).ToList();
//                    }
//                    else if (gp.sord == "desc")
//                    {
//                        IE = IE.OrderByDescending(orderfuc);
//                        jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.MaxSalary), Convert.ToString(a.Range), Convert.ToString(a.Wage) }).ToList();
//                    }
//                    if (pageIndex > 1)
//                    {
//                        int h = pageIndex * pageSize;
//                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.MaxSalary, a.Range, a.Wage }).ToList();
//                    }
//                    totalRecords = ESICMaster.Count();
//                }
//                if (totalRecords > 0)
//                {
//                    totalPages = (int)Math.Ceiling((float)totalRecords / (float)gp.rows);
//                }
//                if (gp.page > totalPages)
//                {
//                    gp.page = totalPages;
//                }
//                var JsonData = new
//                {
//                    page = gp.page,
//                    rows = jsonData,
//                    records = totalRecords,
//                    total = totalPages
//                };
//                return Json(JsonData, JsonRequestBehavior.AllowGet);
//            }
//            catch (Exception ex)
//            {
//                throw ex;
//            }
//        }


//        [HttpPost]
//        public async Task<ActionResult> EditSave(ESICMaster NOBJ, int data, FormCollection form)
//        {


//            if (ModelState.IsValid)
//            {
//                try
//                {

//                    //DbContextTransaction transaction = db.Database.BeginTransaction();

//                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
//                    {

//                        var Curr_OBJ = db.ESICMaster.Find(data);
//                        //TempData["CurrRowVersion"] = Curr_OBJ.RowVersion;
//                        db.Entry(Curr_OBJ).State = System.Data.Entity.EntityState.Detached;
//                        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
//                        {
//                            ESICMaster blog = blog = null;
//                            DbPropertyValues originalBlogValues = null;

//                            using (var context = new DataBaseContext())
//                            {
//                                blog = context.ESICMaster.Where(e => e.Id == data).SingleOrDefault();
//                                originalBlogValues = context.Entry(blog).OriginalValues;
//                            }

//                            //NOBJ.DBTrack = new DBTrack
//                            //{
//                            //    CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
//                            //    CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
//                            //    Action = "M",
//                            //    ModifiedBy = SessionManager.UserName,
//                            //    ModifiedOn = DateTime.Now
//                            //};
//                            ESICMaster EOBJ = new ESICMaster()
//                            {
//                                MaxSalary = NOBJ.MaxSalary,
//                                Range = NOBJ.Range,
//                                Wage = NOBJ.Wage,
//                                Id = data,
//                               // DBTrack = NOBJ.DBTrack
//                            };


//                            db.ESICMaster.Attach(EOBJ);
//                            db.Entry(EOBJ).State = System.Data.Entity.EntityState.Modified;

//                            // db.Entry(c.DBTrack).State = System.Data.Entity.EntityState.Modified;
//                            //db.SaveChanges();
//                            db.Entry(EOBJ).OriginalValues["RowVersion"] = TempData["RowVersion"];
//                            //DBTrackFile.DBTrackSave("Payroll/Payroll", originalBlogValues, db.ChangeTracker, NOBJ.DBTrack);
//                            await db.SaveChangesAsync();
//                            //DisplayTrackedEntities(db.ChangeTracker);
//                            db.Entry(EOBJ).State = System.Data.Entity.EntityState.Detached;
//                            ts.Complete();
//                            return Json(new Object[] { EOBJ.Id, EOBJ.MaxSalary, "Record Updated", JsonRequestBehavior.AllowGet });
//                        }
//                    }
//                }
//                catch (DbUpdateConcurrencyException ex)
//                {
//                    var entry = ex.Entries.Single();
//                    var clientValues = (ESICMaster)entry.Entity;
//                    var databaseEntry = entry.GetDatabaseValues();
//                    if (databaseEntry == null)
//                    {
//                        return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
//                    }
//                    else
//                    {
//                        var databaseValues = (ESICMaster)databaseEntry.ToObject();
//                       // NOBJ.RowVersion = databaseValues.RowVersion;
//                    }
//                }

//                return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });

//                //db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
//                //await db.SaveChangesAsync();
//                //return Json(new Object[] { "", "", "Data saved successfully.", JsonRequestBehavior.AllowGet });

//            }
//            return View();
//        }

//        [HttpPost]
//        public ActionResult Delete(int data)
//        {

//            ESICMaster EOBJ = db.ESICMaster.Find(data);
//            var selectedRegions = "";

//            if (selectedRegions != "")
//            {

//            }


//            try
//            {
//                DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };
//                db.Entry(EOBJ).State = System.Data.Entity.EntityState.Deleted;
//                //DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, dbT);
//                db.SaveChanges();

//                //return this.Json(new { msg = "Data removed.", JsonRequestBehavior.AllowGet });
//                return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
//            }
//            catch (DbUpdateConcurrencyException)
//            {
//                return RedirectToAction("Delete", new { concurrencyError = true, id = data });
//            }
//            catch (RetryLimitExceededException /* dex */)
//            {
//                //Log the error (uncomment dex variable MaxSalary and add a line here to write a log.)
//                //ModelState.AddModelError("", "Unable to delete. Try again, and if the problem persists, see your system administrator.");
//                //return RedirectToAction("Delete");
//                return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
//            }
//        }



//    }
//}