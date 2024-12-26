using P2b.Global;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using P2BUltimate.Security;
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
using System.Threading.Tasks;

namespace P2BUltimate.Controllers.Payroll.MainController
{
    public class ITSection10Controller : Controller
    {
        //private DataBaseContext db = new DataBaseContext();
        //
        // GET: /ITSection10/
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/ITSection10/Index.cshtml");
        }

        public ActionResult ITSection10SalHead_partial()
        {
            return View("~/Views/Shared/Payroll/_ITSection10SalHead.cshtml");
        }


        [HttpPost]
        public ActionResult GetITSection10SalHeadLKDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.ITSection10SalHeads.Include(q=>q.SalHead).Include(q=>q.Frequency).ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.ITSection10SalHeads.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }
                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult Create(ITSection10 ITSec10, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string Itsection10salheadlist = form["Itsection10salheadlist"] == "0" ? "" : form["Itsection10salheadlist"];


                    List<ITSection10SalHeads> ObjITSection10SalHeads = new List<ITSection10SalHeads>();
                    if (Itsection10salheadlist != null && Itsection10salheadlist != "")
                    {
                        var ids = Utility.StringIdsToListIds(Itsection10salheadlist);
                        foreach (var ca in ids)
                        {
                            var value = db.ITSection10SalHeads.Find(ca);
                            ObjITSection10SalHeads.Add(value);
                            ITSec10.Itsection10salhead = ObjITSection10SalHeads;
                        }
                    }

                    if (db.ITSection10.Any(o => o.ExemptionCode == ITSec10.ExemptionCode))
                    {
                        //return this.Json(new { msg = "Code already exists." });
                        Msg.Add("  Record Already Exists.  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        // return this.Json(new Object[] { null, null, "ScaleName already exists.", JsonRequestBehavior.AllowGet });

                    }

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            ITSec10.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            ITSection10 ITSection10 = new ITSection10()
                            {
                                ExemptionCode = ITSec10.ExemptionCode,
                                Itsection10salhead = ITSec10.Itsection10salhead,
                                MaxAmount = ITSec10.MaxAmount,
                                DBTrack = ITSec10.DBTrack
                            };
                            try
                            {

                                db.ITSection10.Add(ITSection10);
                                var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, ITSec10.DBTrack);
                                DT_ITSection10 DT_ITSec10 = (DT_ITSection10)rtn_Obj;
                                db.Create(DT_ITSec10);
                                db.SaveChanges();
                                ts.Complete();
                                Msg.Add("  Data Created successfully  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return this.Json(new Object[] { "", "", "Data Created Successfully.", JsonRequestBehavior.AllowGet });

                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("create", new { concurrencyError = true, id = ITSec10.Id });
                            }
                            catch (DataException /* dex */)
                            {
                                Msg.Add(" Unable to edit. Try again, and if the problem persists, see your system administrator");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // return this.Json(new { msg = "Unable to edit. Try again, and if the problem persists contact your system administrator." });
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


        public class ITSection10_Val
        {
            public Array ITSection10SalHead_Id { get; set; }
            public Array ITSection10SalHead_FullDetails { get; set; }
        }

        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.ITSection10
                    .Include(e => e.Itsection10salhead)
                    .Where(e => e.Id == data).Select
                    (e => new
                    {

                        ExemptionCode = e.ExemptionCode,
                        MaxAmount = e.MaxAmount,
                        Action = e.DBTrack.Action
                    }).ToList();

                List<ITSection10_Val> returndata = new List<ITSection10_Val>();
                var ITSalhead = db.ITSection10
                    .Include(e => e.Itsection10salhead).Include(e => e.Itsection10salhead.Select(q => q.Frequency))
                    .Include(e => e.Itsection10salhead.Select(q => q.SalHead))
                      .Where(e => e.Id == data)
                     .ToList();
                foreach (var ca in ITSalhead)
                {
                    returndata.Add(new ITSection10_Val
                    {
                        ITSection10SalHead_Id = ca.Itsection10salhead.Select(e => e.Id).ToArray(),
                        ITSection10SalHead_FullDetails = ca.Itsection10salhead.Select(e => e.FullDetails).ToArray()
                    });

                }

                var W = db.DT_ITSection10
                     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id,
                         ExceptionCode = e.ExceptionCode == null ? "" : e.ExceptionCode,
                         MaxAmount = e.MaxAmount == null ? "" : e.MaxAmount.ToString(),
                        
                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var ITSection10 = db.DT_ITSection10.Find(data);
                TempData["RowVersion"] = ITSection10.RowVersion;
                var Auth = ITSection10.DBTrack.IsModified;
                return Json(new Object[] { Q, returndata, W, Auth, JsonRequestBehavior.AllowGet });
            }
        }

       

        public int EditS(string ITSection10SalHead, int data, ITSection10 NOBJ, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (ITSection10SalHead != null && ITSection10SalHead != "")
                {
                    List<int> IDs = ITSection10SalHead.Split(',').Select(e => int.Parse(e)).ToList();
                    foreach (var k in IDs)
                    {
                        var value = db.ITSection10SalHeads.Find(k);
                        NOBJ.Itsection10salhead = new List<ITSection10SalHeads>();
                        NOBJ.Itsection10salhead.Add(value);
                    }
                }
                else
                {
                    var details = db.ITSection10.Include(e => e.Itsection10salhead).Where(x => x.Id == data).ToList();
                    foreach (var s in details)
                    {
                        s.Itsection10salhead = null;
                        db.ITSection10.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }


                var CurOBJ = db.ITSection10.Find(data);
                TempData["CurrRowVersion"] = CurOBJ.RowVersion;
                db.Entry(CurOBJ).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    NOBJ.DBTrack = dbT;
                    ITSection10 TOBJ = new ITSection10()
                    {
                        ExemptionCode = NOBJ.ExemptionCode,
                        MaxAmount = NOBJ.MaxAmount,
                        Id = data,
                        DBTrack = NOBJ.DBTrack
                    };


                    db.ITSection10.Attach(TOBJ);
                    db.Entry(TOBJ).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(TOBJ).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                    return 1;
                }
                return 0;
            }
        }

        [HttpPost]
        public async Task<ActionResult> EditSave(ITSection10 ESOBJ, int data, FormCollection form) // Edit submit
        {
             List<string> Msg = new List<string>();
             using (DataBaseContext db = new DataBaseContext())
             {
                 try
                 {
                     bool Auth = form["Autho_Allow"] == "true" ? true : false;

                     var db_data = db.ITSection10.Include(e => e.Itsection10salhead).Where(e => e.Id == data).SingleOrDefault();
                     List<ITSection10SalHeads> ITSection10SalHeads = new List<ITSection10SalHeads>();
                     string Values = form["Itsection10salheadlist"];

                     if (Values != null)
                     {
                         var ids = Utility.StringIdsToListIds(Values);
                         foreach (var ca in ids)
                         {
                             var ITSection10SalHeads_val = db.ITSection10SalHeads.Find(ca);
                             ITSection10SalHeads.Add(ITSection10SalHeads_val);
                             db_data.Itsection10salhead = ITSection10SalHeads;
                         }
                     }
                     else
                     {
                         db_data.Itsection10salhead = null;
                     }

                     db.ITSection10.Attach(db_data);
                     db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                     db.SaveChanges();
                     TempData["RowVersion"] = db_data.RowVersion;
                     db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;


                     if (Auth == false)
                     {
                         if (ModelState.IsValid)
                         {
                             using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                             {
                                 try
                                 {
                                     //DbContextTransaction transaction = db.Database.BeginTransaction();

                                     ITSection10 blog = null; // to retrieve old data                           
                                     DbPropertyValues originalBlogValues = null;

                                     using (var context = new DataBaseContext())
                                     {
                                         blog = context.ITSection10.Where(e => e.Id == data)
                                                                 .Include(e => e.Itsection10salhead).SingleOrDefault();
                                         originalBlogValues = context.Entry(blog).OriginalValues;
                                     }
                                     ESOBJ.DBTrack = new DBTrack
                                     {
                                         CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                         CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                         Action = "M",
                                         ModifiedBy = SessionManager.UserName,
                                         ModifiedOn = DateTime.Now
                                     };
                                     int a = EditS(Values, data, ESOBJ, ESOBJ.DBTrack);
                                     await db.SaveChangesAsync();
                                     using (var context = new DataBaseContext())
                                     {

                                         //To save data in history table 
                                         var Obj = DBTrackFile.ModifiedDataHistory("Payroll/Payroll", "M", blog, ESOBJ, "ITSection10", ESOBJ.DBTrack);
                                         DT_ITSection10 DT_ITSection10 = (DT_ITSection10)Obj;
                                         db.DT_ITSection10.Add(DT_ITSection10);
                                         db.SaveChanges();
                                     }
                                     var dat1 = db.ITSection10.Include(e => e.Itsection10salhead).Where(e => e.Id == data).SingleOrDefault();
                                     ts.Complete();
                                     Msg.Add("  Record Updated");
                                     return Json(new Utility.JsonReturnClass { Id = dat1.Id, Val = dat1.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                     //return Json(new Object[] { ESOBJ.Id, ESOBJ.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });
                                 }

                                 catch (DbUpdateConcurrencyException ex)
                                 {
                                     var entry = ex.Entries.Single();
                                     var clientValues = (Grade)entry.Entity;
                                     var databaseEntry = entry.GetDatabaseValues();
                                     if (databaseEntry == null)
                                     {
                                         Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                         return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                         //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                     }
                                     else
                                     {
                                         var databaseValues = (Grade)databaseEntry.ToObject();
                                         ESOBJ.RowVersion = databaseValues.RowVersion;
                                     }
                                 }
                                 Msg.Add("Record modified by another user.So refresh it and try to save again.");
                                 return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                 //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                             }
                         }
                     }
                     else
                     {

                         using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                         {

                             ITSection10 blog = null; // to retrieve old data
                             DbPropertyValues originalBlogValues = null;
                             ITSection10 Old_OBJ = null;

                             using (var context = new DataBaseContext())
                             {
                                 blog = context.ITSection10.Where(e => e.Id == data).SingleOrDefault();
                                 originalBlogValues = context.Entry(blog).OriginalValues;
                             }
                             ESOBJ.DBTrack = new DBTrack
                             {
                                 CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                 CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                 Action = "M",
                                 IsModified = blog.DBTrack.IsModified == true ? true : false,
                                 ModifiedBy = SessionManager.UserName,
                                 ModifiedOn = DateTime.Now
                             };
                             ITSection10 OBJ = new ITSection10()
                             {

                                 Id = data,
                                 ExemptionCode = ESOBJ.ExemptionCode,
                                 MaxAmount = ESOBJ.MaxAmount,
                                 DBTrack = ESOBJ.DBTrack,
                                 RowVersion = (Byte[])TempData["RowVersion"]
                             };


                             //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                             using (var context = new DataBaseContext())
                             {
                                 var obj = DBTrackFile.ModifiedDataHistory("Payroll/Payroll", "M", blog, OBJ, "ITSection10", ESOBJ.DBTrack);

                                 Old_OBJ = context.ITSection10.Where(e => e.Id == data)
                                    .Include(e => e.Itsection10salhead).SingleOrDefault();
                                 DT_ITSection10 DT_OBJ = (DT_ITSection10)obj;

                                 DT_OBJ.Itsection10salhead_Id = DBTrackFile.ValCompare(Old_OBJ.Itsection10salhead, ESOBJ.Itsection10salhead);
                                 db.Create(DT_OBJ);
                                 //db.SaveChanges();
                             }
                             blog.DBTrack = ESOBJ.DBTrack;
                             db.ITSection10.Attach(blog);
                             db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                             db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                             db.SaveChanges();
                             ts.Complete();
                             Msg.Add("  Record Updated");
                             return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = OBJ.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                             //return Json(new Object[] { , , "Record Updated", JsonRequestBehavior.AllowGet });
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

                             ITSection10 AOBJ = db.ITSection10.Include(e => e.Itsection10salhead)
                                .FirstOrDefault(e => e.Id == auth_id);

                             AOBJ.DBTrack = new DBTrack
                             {
                                 Action = "C",
                                 ModifiedBy = AOBJ.DBTrack.ModifiedBy != null ? AOBJ.DBTrack.ModifiedBy : null,
                                 CreatedBy = AOBJ.DBTrack.CreatedBy != null ? AOBJ.DBTrack.CreatedBy : null,
                                 CreatedOn = AOBJ.DBTrack.CreatedOn != null ? AOBJ.DBTrack.CreatedOn : null,
                                 IsModified = AOBJ.DBTrack.IsModified == true ? false : false,
                                 AuthorizedBy = SessionManager.UserName,
                                 AuthorizedOn = DateTime.Now
                             };

                             db.ITSection10.Attach(AOBJ);
                             db.Entry(AOBJ).State = System.Data.Entity.EntityState.Modified;
                             db.Entry(AOBJ).OriginalValues["RowVersion"] = TempData["RowVersion"];

                             var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, AOBJ.DBTrack);
                             DT_Grade DT_OBJ = (DT_Grade)rtn_Obj;

                             await db.SaveChangesAsync();

                             db.Create(DT_OBJ);
                             ts.Complete();
                             Msg.Add("  Record Authorised");
                             return Json(new Utility.JsonReturnClass { Id = AOBJ.Id, Val = AOBJ.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                             // return Json(new Object[] { AOBJ.Id, AOBJ.FullDetails, "Record Authorised", JsonRequestBehavior.AllowGet });
                         }
                     }
                     else if (auth_action == "M")
                     {

                         ITSection10 Old_OBJ = db.ITSection10.Include(e => e.Itsection10salhead)
                                                          .Where(e => e.Id == auth_id).SingleOrDefault();

                         DT_ITSection10 Curr_OBJ = db.DT_ITSection10
                                                     .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                                     .OrderByDescending(e => e.Id)
                                                     .FirstOrDefault();

                         ITSection10 MOBJ = new ITSection10();

                         //string OBJ = Curr_OBJ.Itsection10salhead_Id == null ? null : Curr_OBJ.Itsection10salhead_Id.ToString();
                         //MOBJ.ExemptionCode = Curr_OBJ.ExemptionCode == null ? Old_OBJ.ExemptionCode : Curr_OBJ.ExemptionCode;
                         //MOBJ.MaxAmount = Curr_OBJ.MaxAmount == null ? Old_OBJ.MaxAmount : Curr_OBJ.MaxAmount;

                         if (ModelState.IsValid)
                         {
                             try
                             {

                                 //DbContextTransaction transaction = db.Database.BeginTransaction();

                                 using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                 {
                                     // db.Configuration.AutoDetectChangesEnabled = false;
                                     MOBJ.DBTrack = new DBTrack
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

                                     int a = EditS("", auth_id, MOBJ, MOBJ.DBTrack);

                                     await db.SaveChangesAsync();

                                     ts.Complete();
                                     Msg.Add("  Record Updated");
                                     return Json(new Utility.JsonReturnClass { Id = MOBJ.Id, Val = MOBJ.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                     //return Json(new Object[] { , , "Record Updated", JsonRequestBehavior.AllowGet });
                                 }
                             }
                             catch (DbUpdateConcurrencyException ex)
                             {
                                 var entry = ex.Entries.Single();
                                 var clientValues = (ITSection10)entry.Entity;
                                 var databaseEntry = entry.GetDatabaseValues();
                                 if (databaseEntry == null)
                                 {
                                     Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                     return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                     //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                 }
                                 else
                                 {
                                     var databaseValues = (ITSection10)databaseEntry.ToObject();
                                     MOBJ.RowVersion = databaseValues.RowVersion;
                                 }
                             }
                             Msg.Add("Record modified by another user.So refresh it and try to save again.");
                             return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                             //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                         }
                     }
                     else if (auth_action == "D")
                     {
                         using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                         {

                             ITSection10 DOBJ = db.ITSection10.AsNoTracking().Include(e => e.Itsection10salhead)
                                                                         .FirstOrDefault(e => e.Id == auth_id);
                             // Level Var1 = DOBJ.Levels.ToLookup();
                             ICollection<ITSection10SalHeads> Var1 = DOBJ.Itsection10salhead;
                             DOBJ.DBTrack = new DBTrack
                             {
                                 Action = "D",
                                 ModifiedBy = DOBJ.DBTrack.ModifiedBy != null ? DOBJ.DBTrack.ModifiedBy : null,
                                 CreatedBy = DOBJ.DBTrack.CreatedBy != null ? DOBJ.DBTrack.CreatedBy : null,
                                 CreatedOn = DOBJ.DBTrack.CreatedOn != null ? DOBJ.DBTrack.CreatedOn : null,
                                 IsModified = DOBJ.DBTrack.IsModified == true ? false : false,
                                 AuthorizedBy = SessionManager.UserName,
                                 AuthorizedOn = DateTime.Now
                             };

                             db.ITSection10.Attach(DOBJ);
                             db.Entry(DOBJ).State = System.Data.Entity.EntityState.Deleted;
                             await db.SaveChangesAsync();
                             using (var context = new DataBaseContext())
                             {
                                 DOBJ.Itsection10salhead = Var1;
                             }


                             db.Entry(DOBJ).State = System.Data.Entity.EntityState.Detached;
                             ts.Complete();
                             Msg.Add(" Record Authorised ");
                             return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                             //return Json(new Object[] { "", "", "Record Authorised", JsonRequestBehavior.AllowGet });
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
                IEnumerable<ITSection10> ITSection10 = null;
                if (gp.IsAutho == true)
                {
                    ITSection10 = db.ITSection10.Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    ITSection10 = db.ITSection10.AsNoTracking().ToList();
                }

                IEnumerable<ITSection10> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = ITSection10;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.ExemptionCode.ToString().Contains(gp.searchString))
                                ||  (e.MaxAmount.ToString().Contains(gp.searchString)) 
                                ||  (e.Id.ToString().Contains(gp.searchString))
                                ).Select(a => new Object[] { a.ExemptionCode, a.MaxAmount, a.Id }).ToList();
                        //jsonData = IE.Select(a => new  { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.ExemptionCode, a.MaxAmount, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = ITSection10;
                    Func<ITSection10, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "ExemptionCode" ? c.ExemptionCode :
                                         gp.sidx == "MaxAmount" ? c.MaxAmount.ToString() : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.ExemptionCode), Convert.ToString(a.MaxAmount), a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.ExemptionCode), Convert.ToString(a.MaxAmount), a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.ExemptionCode, a.MaxAmount, a.Id }).ToList();
                    }
                    totalRecords = ITSection10.Count();
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

        //[HttpPost]
        //public async Task<ActionResult> Delete(int data)
        //{

        //    ITSection10 DOBJ = db.ITSection10.Where(e => e.Id == data).SingleOrDefault();

        //    if (DOBJ.DBTrack.IsModified == true)
        //    {
        //        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //        {
        //            //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, DOBJ.DBTrack, DOBJ, null, "Grade");
        //            DBTrack dbT = new DBTrack
        //            {
        //                Action = "D",
        //                CreatedBy = DOBJ.DBTrack.CreatedBy != null ? DOBJ.DBTrack.CreatedBy : null,
        //                CreatedOn = DOBJ.DBTrack.CreatedOn != null ? DOBJ.DBTrack.CreatedOn : null,
        //                IsModified = DOBJ.DBTrack.IsModified == true ? true : false
        //            };
        //            DOBJ.DBTrack = dbT;
        //            db.Entry(DOBJ).State = System.Data.Entity.EntityState.Modified;
        //            var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, DOBJ.DBTrack);
        //            DT_ITSection10 DT_Corp = (DT_ITSection10)rtn_Obj;

        //            db.Create(DT_Corp);

        //            await db.SaveChangesAsync();

        //            ts.Complete();
        //            return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
        //        }
        //    }
        //    else
        //    {

        //        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //        {

        //            try
        //            {
        //                DBTrack dbT = new DBTrack
        //                {
        //                    Action = "D",
        //                    ModifiedBy = SessionManager.UserName,
        //                    ModifiedOn = DateTime.Now,
        //                    CreatedBy = DOBJ.DBTrack.CreatedBy != null ? DOBJ.DBTrack.CreatedBy : null,
        //                    CreatedOn = DOBJ.DBTrack.CreatedOn != null ? DOBJ.DBTrack.CreatedOn : null,
        //                    IsModified = DOBJ.DBTrack.IsModified == true ? false : false//,
        //                    //AuthorizedBy = SessionManager.UserName,
        //                    //AuthorizedOn = DateTime.Now
        //                };

        //                // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };

        //                db.Entry(DOBJ).State = System.Data.Entity.EntityState.Deleted;
        //                var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, dbT);
        //                DT_ITSection10 DT_Corp = (DT_ITSection10)rtn_Obj;
        //                db.Create(DT_Corp);

        //                await db.SaveChangesAsync();

        //                ts.Complete();
        //                return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });

        //            }
        //            catch (RetryLimitExceededException /* dex */)
        //            {
        //                //Log the error (uncomment dex variable name and add a line here to write a log.)
        //                //ModelState.AddModelError("", "Unable to delete. Try again, and if the problem persists, see your system administrator.");
        //                //return RedirectToAction("Delete");
        //                return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
        //            }
        //        }
        //    }
        //}


        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    ITSection10 objjvparam = db.ITSection10.Include(e => e.Itsection10salhead).Where(e => e.Id == data).SingleOrDefault();
                    var salheadlist = objjvparam.Itsection10salhead;


                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        if (salheadlist != null)
                        {
                            var objsalheadlist = new HashSet<int>(objjvparam.Itsection10salhead.Select(e => e.Id));
                            if (objsalheadlist.Count > 0)
                            {
                                Msg.Add(" Child record exists.Cannot remove it..  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Object[] { "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                                // return Json(new Object[] { "", "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                            }
                        }


                        try
                        {
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now,
                                CreatedBy = objjvparam.DBTrack.CreatedBy != null ? objjvparam.DBTrack.CreatedBy : null,
                                CreatedOn = objjvparam.DBTrack.CreatedOn != null ? objjvparam.DBTrack.CreatedOn : null,
                                IsModified = objjvparam.DBTrack.IsModified == true ? false : false//,
                                //AuthorizedBy = SessionManager.UserName,
                                //AuthorizedOn = DateTime.Now
                            };
                            db.Entry(objjvparam).State = System.Data.Entity.EntityState.Deleted;
                            //var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, objjvparam.DBTrack);
                            //DT_JVParameter DT_Corp = (DT_JVParameter)rtn_Obj;
                            //db.Create(DT_Corp);
                            await db.SaveChangesAsync();
                            ts.Complete();
                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });

                        }
                        catch (RetryLimitExceededException /* dex */)
                        {
                            //Log the error (uncomment dex variable name and add a line here to write a log.)
                            //ModelState.AddModelError("", "Unable to delete. Try again, and if the problem persists, see your system administrator.");
                            //return RedirectToAction("Delete");
                            Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            // return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
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
}