///
/// Created by Tanushri
///


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
using P2BUltimate.Security;
using Payroll;
namespace P2BUltimate.Controllers.Core.MainController
{
    [AuthoriseManger]
    public class PayScaleAgreementController : Controller
    {

        //private DataBaseContext db = new DataBaseContext();

        public ActionResult Index()
        {
            return View("~/Views/Core/MainViews/PayScaleAgreement/Index.cshtml");
        }
        public ActionResult Partial()
        {
            return View("~/Views/Shared/Core/_PayScaleAgreement.cshtml");
        }
        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult Create(PayScaleAgreement c, FormCollection form) //Create submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string PayScaleDetails = form["PayScalelist"] == "" ? null : form["PayScalelist"];
                    int company_Id = 0;
                    company_Id = Convert.ToInt32(Session["CompId"]);
                    var Company = new Company();
                    if (company_Id != null)
                    {
                        Company = db.Company.Where(e => e.Id == company_Id).SingleOrDefault();

                    }

                    if (PayScaleDetails != null)
                    {
                        int ContId = Convert.ToInt32(PayScaleDetails);
                        var val = db.PayScale.Include(e => e.PayScaleType).Include(e => e.PayScaleArea.Select(r => r.LocationObj))
                                            .Where(e => e.Id == ContId).SingleOrDefault();
                        c.PayScale = val;
                    }
                    else
                    {
                        Msg.Add("Select Payscale Agreement..!");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            if (db.PayScaleAgreement.Any(o => o.EffDate == c.EffDate))
                            {
                                Msg.Add("  Effective Date Already Exists.  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // return Json(new Object[] { "", "", "Effective Date Already Exists.", JsonRequestBehavior.AllowGet });
                            }

                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            PayScaleAgreement PayScaleAgreement = new PayScaleAgreement()
                            {

                                EffDate = c.EffDate,
                                EndDate = c.EndDate,
                                PayScale = c.PayScale,
                                DBTrack = c.DBTrack
                            };
                            try
                            {

                                var OldPayScaleAgr = db.PayScaleAgreement.Where(e => e.EndDate == null).SingleOrDefault();
                                if (OldPayScaleAgr != null)
                                {
                                    OldPayScaleAgr.EndDate = PayScaleAgreement.EffDate.Value.AddDays(-1).Date;
                                    db.PayScaleAgreement.Attach(OldPayScaleAgr);
                                    db.Entry(OldPayScaleAgr).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    db.Entry(OldPayScaleAgr).State = System.Data.Entity.EntityState.Detached;
                                }

                                db.PayScaleAgreement.Add(PayScaleAgreement);
                                var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, c.DBTrack);
                                DT_PayScaleAgreement DT_OBJ = (DT_PayScaleAgreement)rtn_Obj;
                                DT_OBJ.PayScale_Id = c.PayScale == null ? 0 : c.PayScale.Id;
                                db.Create(DT_OBJ);
                                db.SaveChanges();
                                //DBTrackFile.DBTrackSave("Core/P2b.Global", "C", PayScaleAgreement, null, "PayScaleAgreement", null);
                                if (Company != null)
                                {
                                    var payscaleagrremnt_list = new List<PayScaleAgreement>();
                                    payscaleagrremnt_list.Add(PayScaleAgreement);
                                    Company.PayScaleAgreement = payscaleagrremnt_list;
                                    db.Company.Attach(Company);
                                    db.Entry(Company).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    db.Entry(Company).State = System.Data.Entity.EntityState.Detached;

                                }


                                ts.Complete();
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = PayScaleAgreement.Id, Val = PayScaleAgreement.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // return this.Json(new Object[] { PayScaleAgreement.Id, PayScaleAgreement.FullDetails, "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = c.Id });
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
                        //  return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
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

        //public ActionResult Editcontactdetails_partial(int data)
        //{
        //    var r = (from ca in db.PayScale
        //             .Where(e => e.Id == data)
        //             select new
        //             {
        //                 Id = ca.Id,
        //                 EmailId = ca.EmailId,
        //                 FaxNo = ca.FaxNo,
        //                 Website = ca.Website
        //             }).ToList();

        //    var a = db.PayScale.Include(e => e.ContactNumbers).Where(e => e.Id == data).SingleOrDefault();
        //    var b = a.ContactNumbers;

        //    var r1 = (from s in b
        //              select new
        //              {
        //                  Id = s.Id,
        //                  FullContactNumbers = s.FullContactNumbers
        //              }).ToList();

        //    TempData["RowVersion"] = db.PayScale.Find(data).RowVersion;
        //    return Json(new object[] { r, r1 }, JsonRequestBehavior.AllowGet);
        //}

        public ActionResult CreatePayScale_partial()
        {
            return View("~/Views/Shared/Core/_PayScaleP.cshtml");
        }



        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.PayScaleAgreement
                    .Include(e => e.PayScale)
                    .Where(e => e.Id == data).Select
                    (e => new
                    {

                        EndDate = e.EndDate,
                        EffDate = e.EffDate,
                        PayScale = e.PayScale,
                        Action = e.DBTrack.Action
                    }).ToList();

                var add_data = db.PayScaleAgreement
                  .Include(e => e.PayScale)
                    //.Include(e => e.PayScale.PayScaleArea.Select(r => r.LocationObj))
                    .Where(e => e.Id == data)
                    .Select(e => new
                    {
                        //Address_FullAddress = e.Address.FullAddress == null ? "" : e.Address.FullAddress,
                        //Add_Id = e.Address.Id == null ? "" : e.Address.Id.ToString(),
                        PayScale_Id = e.PayScale.Id == null ? "" : e.PayScale.Id.ToString(),
                        PayScale_FullDetails = e.PayScale.FullDetails == null ? "" : e.PayScale.FullDetails
                    }).ToList();


                var W = db.DT_PayScaleAgreement
                     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id,
                         EffDate = e.EffDate,
                         EndDate = e.EndDate,
                         PayScale_Val = e.PayScale_Id == 0 ? "" : db.PayScale
                         .Where(x => x.Id == e.PayScale_Id).Select(x => x.FullDetails).FirstOrDefault()
                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var Corp = db.PayScaleAgreement.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, add_data, W, Auth, JsonRequestBehavior.AllowGet });
            }
        }

        [HttpPost]
        public async Task<ActionResult> EditSave(PayScaleAgreement c, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string PayScaleDetails = form["PayScalelist"] == "0" ? "" : form["PayScalelist"];
                    //  bool Auth = form["Autho_Action"] == "" ? false : true;
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;

                    c.PayScale_Id = PayScaleDetails != null && PayScaleDetails != "" ? int.Parse(PayScaleDetails) : 0;
 
                   
                    if (Auth == false)
                    {


                        if (ModelState.IsValid)
                        {
                            try
                            { 
                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    PayScaleAgreement blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                      blog = db.PayScaleAgreement.Where(e => e.Id == data)
                                            //.Include(e => e.Address)
                                                                .Include(e => e.PayScale).SingleOrDefault();
                                        originalBlogValues = db.Entry(blog).OriginalValues;
                                   
                                    c.DBTrack = new DBTrack
                                    {
                                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };

                            
                                    var CurCorp = db.PayScaleAgreement.Find(data);
                                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {
                                        CurCorp.EffDate = c.EffDate;
                                        CurCorp.EndDate = c.EndDate;
                                        CurCorp.Id = data;
                                        CurCorp.DBTrack = c.DBTrack;
                                        CurCorp.PayScale_Id = c.PayScale_Id != 0 ? c.PayScale_Id : null;

                                        db.Entry(CurCorp).State = System.Data.Entity.EntityState.Modified;
                                        var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                        DT_PayScaleAgreement DT_OBJ = (DT_PayScaleAgreement)obj;
                                        //DT_OBJ.Address_Id = blog.Address == null ? 0 : blog.Address.Id;                               
                                        DT_OBJ.PayScale_Id = blog.PayScale == null ? 0 : blog.PayScale.Id;
                                        db.Create(DT_OBJ);
                                        db.SaveChanges();
                                    }
 
                                    ts.Complete(); 
                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = c.Id, Val = c.Id.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet); 

                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (PayScaleAgreement)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (PayScaleAgreement)databaseEntry.ToObject();
                                    c.RowVersion = databaseValues.RowVersion;

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

                            PayScaleAgreement blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            PayScaleAgreement Old_OBJ = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.PayScaleAgreement.Where(e => e.Id == data).SingleOrDefault();
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
                            PayScaleAgreement corp = new PayScaleAgreement()
                            {
                                EffDate = c.EffDate,
                                EndDate = c.EndDate,
                                PayScale = c.PayScale,
                                Id = data,
                                DBTrack = c.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };


                            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, corp, "PayScaleAgreement", c.DBTrack);
                                // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                Old_OBJ = context.PayScaleAgreement.Where(e => e.Id == data)
                                    .Include(e => e.PayScale).SingleOrDefault();
                                DT_PayScaleAgreement DT_OBJ = (DT_PayScaleAgreement)obj;
                                // DT_OBJ.Address_Id = DBTrackFile.ValCompare(Old_OBJ.Address, c.Address);//Old_OBJ.Address == c.Address ? 0 : Old_OBJ.Address == null && c.Address != null ? c.Address.Id : Old_OBJ.Address.Id;

                                DT_OBJ.PayScale_Id = DBTrackFile.ValCompare(Old_OBJ.PayScale, c.PayScale); //Old_OBJ.ContactDetails == c.ContactDetails ? 0 : Old_OBJ.ContactDetails == null && c.ContactDetails != null ? c.ContactDetails.Id : Old_OBJ.ContactDetails.Id;
                                db.Create(DT_OBJ);
                                //db.SaveChanges();
                            }
                            blog.DBTrack = c.DBTrack;
                            db.PayScaleAgreement.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            return Json(new Object[] { blog.Id, c.Id, "Record Updated", JsonRequestBehavior.AllowGet });
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

        //[HttpPost]
        //public async Task<ActionResult> AuthSave(int auth_id, Boolean isauth, String auth_action) //to save Authorized data
        //{
        //    if (auth_action == "C")
        //    {
        //        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //        {
        //            //PayScaleAgreement OBJ = db.PayScaleAgreement.Find(auth_id);
        //            //PayScaleAgreement OBJ = db.PayScaleAgreement.AsNoTracking().FirstOrDefault(e => e.Id == auth_id);

        //            PayScaleAgreement OBJ = db.PayScaleAgreement
        //                //.Include(e => e.Address)
        //                .Include(e => e.PayScale)
        //               .FirstOrDefault(e => e.Id == auth_id);

        //            OBJ.DBTrack = new DBTrack
        //            {
        //                Action = "C",
        //                ModifiedBy = OBJ.DBTrack.ModifiedBy != null ? OBJ.DBTrack.ModifiedBy : null,
        //                CreatedBy = OBJ.DBTrack.CreatedBy != null ? OBJ.DBTrack.CreatedBy : null,
        //                CreatedOn = OBJ.DBTrack.CreatedOn != null ? OBJ.DBTrack.CreatedOn : null,
        //                IsModified = OBJ.DBTrack.IsModified == true ? false : false,
        //                AuthorizedBy = SessionManager.UserName,
        //                AuthorizedOn = DateTime.Now
        //            };

        //            db.PayScaleAgreement.Attach(OBJ);
        //            db.Entry(OBJ).State = System.Data.Entity.EntityState.Modified;
        //            db.Entry(OBJ).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //            //db.SaveChanges();
        //            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, OBJ.DBTrack);


        //            DT_OBJ.PayScale_Id = OBJ.PayScale == null ? 0 : OBJ.PayScale.Id;
        //            db.Create(DT_OBJ);
        //            await db.SaveChangesAsync();

        //            ts.Complete();
        //            return Json(new Object[] { OBJ.Id, "Record Authorized", JsonRequestBehavior.AllowGet });
        //        }
        //    }
        //    else if (auth_action == "M")
        //    {

        //        PayScaleAgreement Old_OBJ = db.PayScaleAgreement.Include(e => e.VenuType)
        //            //.Include(e => e.Address)
        //                                          .Include(e => e.PayScale).Where(e => e.Id == auth_id).SingleOrDefault();



        //        DT_Venue Curr_OBJ = db.DT_Venue
        //                                    .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
        //                                    .OrderByDescending(e => e.Id)
        //                                    .FirstOrDefault();

        //        if (Curr_OBJ != null)
        //        {
        //            PayScaleAgreement OBJ = new PayScaleAgreement();

        //            string LKVAL = Curr_OBJ.VenuType_Id == null ? null : Curr_OBJ.VenuType_Id.ToString();
        //            //string Addrs = Curr_OBJ.Address_Id == null ? null : Curr_OBJ.Address_Id.ToString();
        //            string Addrs = "";
        //            string ContactDetails = Curr_OBJ.ContactDetails_Id == null ? null : Curr_OBJ.ContactDetails_Id.ToString();
        //            OBJ.Name = Curr_OBJ.Name == null ? Old_OBJ.Name : Curr_OBJ.Name;
        //            OBJ.Fees = Curr_OBJ.Fees == null ? Old_OBJ.Fees : Curr_OBJ.Fees;
        //            //      OBJ.Id = auth_id;

        //            if (ModelState.IsValid)
        //            {
        //                try
        //                {

        //                    //DbContextTransaction transaction = db.Database.BeginTransaction();

        //                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                    {
        //                        // db.Configuration.AutoDetectChangesEnabled = false;
        //                        OBJ.DBTrack = new DBTrack
        //                        {
        //                            CreatedBy = Old_OBJ.DBTrack.CreatedBy == null ? null : Old_OBJ.DBTrack.CreatedBy,
        //                            CreatedOn = Old_OBJ.DBTrack.CreatedOn == null ? null : Old_OBJ.DBTrack.CreatedOn,
        //                            Action = "M",
        //                            ModifiedBy = Old_OBJ.DBTrack.ModifiedBy == null ? null : Old_OBJ.DBTrack.ModifiedBy,
        //                            ModifiedOn = Old_OBJ.DBTrack.ModifiedOn == null ? null : Old_OBJ.DBTrack.ModifiedOn,
        //                            AuthorizedBy = SessionManager.UserName,
        //                            AuthorizedOn = DateTime.Now,
        //                            IsModified = false
        //                        };

        //                        int a = EditS(LKVAL, Addrs, ContactDetails, auth_id, OBJ, OBJ.DBTrack);


        //                        await db.SaveChangesAsync();

        //                        ts.Complete();
        //                        return Json(new Object[] { OBJ.Id, "Record Authorized", JsonRequestBehavior.AllowGet });
        //                    }
        //                }
        //                catch (DbUpdateConcurrencyException ex)
        //                {
        //                    var entry = ex.Entries.Single();
        //                    var clientValues = (PayScaleAgreement)entry.Entity;
        //                    var databaseEntry = entry.GetDatabaseValues();
        //                    if (databaseEntry == null)
        //                    {
        //                        return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
        //                    }
        //                    else
        //                    {
        //                        var databaseValues = (PayScaleAgreement)databaseEntry.ToObject();
        //                        OBJ.RowVersion = databaseValues.RowVersion;
        //                    }
        //                }

        //                return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
        //            }
        //        }
        //        else
        //            return Json(new Object[] { "", "Data removed from history", JsonRequestBehavior.AllowGet });
        //    }
        //    else if (auth_action == "D")
        //    {
        //        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //        {
        //            //PayScaleAgreement OBJ = db.PayScaleAgreement.Find(auth_id);
        //            PayScaleAgreement OBJ = db.PayScaleAgreement.AsNoTracking().Include(e => e.VenuType)
        //                                                        .Include(e => e.PayScale).FirstOrDefault(e => e.Id == auth_id);


        //            ContactDetails conDet = OBJ.ContactDetails;
        //            LookupValue val = OBJ.VenuType;

        //            OBJ.DBTrack = new DBTrack
        //            {
        //                Action = "D",
        //                ModifiedBy = OBJ.DBTrack.ModifiedBy != null ? OBJ.DBTrack.ModifiedBy : null,
        //                CreatedBy = OBJ.DBTrack.CreatedBy != null ? OBJ.DBTrack.CreatedBy : null,
        //                CreatedOn = OBJ.DBTrack.CreatedOn != null ? OBJ.DBTrack.CreatedOn : null,
        //                IsModified = false,
        //                AuthorizedBy = SessionManager.UserName,
        //                AuthorizedOn = DateTime.Now
        //            };

        //            db.PayScaleAgreement.Attach(OBJ);
        //            db.Entry(OBJ).State = System.Data.Entity.EntityState.Deleted;


        //            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, OBJ.DBTrack);
        //            DT_Venue DT_OBJ = (DT_Venue)rtn_Obj;
        //            DT_OBJ.VenuType_Id = OBJ.VenuType == null ? 0 : OBJ.VenuType.Id;
        //            DT_OBJ.ContactDetails_Id = OBJ.ContactDetails == null ? 0 : OBJ.ContactDetails.Id;
        //            db.Create(DT_OBJ);
        //            await db.SaveChangesAsync();
        //            db.Entry(OBJ).State = System.Data.Entity.EntityState.Detached;
        //            ts.Complete();
        //            return Json(new Object[] { "", "Record Authorized", JsonRequestBehavior.AllowGet });
        //        }

        //    }
        //    return View();

        //}

        public ActionResult AddCarryForwad(int PayScaleAgreementId)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                var PrevAgree = db.PayScaleAgreement.Where(x => x.EndDate != null).OrderByDescending(e => e.Id).FirstOrDefault();
                var CurrAgree = db.PayScaleAgreement.Where(x => x.EndDate == null).OrderByDescending(e => e.Id).FirstOrDefault();

                var newagree = db.LookupValue.Where(e => e.Id == PayScaleAgreementId).SingleOrDefault();
                string newagreeval = "";
                if (newagree != null)
                {
                    newagreeval = newagree.LookupVal.ToString();
                }
                //assignment
                if (newagreeval.ToUpper() == "PAYSCALEASSIGNMENT")
                {
                    var ChkAssignment = db.PayScaleAssignment.Where(x => x.PayScaleAgreement.Id == CurrAgree.Id).ToList();
                    var PrvAssignment = db.PayScaleAssignment.Include(x => x.SalaryHead).Include(x => x.SalHeadFormula).Where(x => x.PayScaleAgreement.Id == PrevAgree.Id).ToList();


                    if (ChkAssignment.Count == 0)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, new System.TimeSpan(1, 30, 0)))
                        {
                            foreach (var item in PrvAssignment)
                            {
                                int company_Id = 0;
                                company_Id = Convert.ToInt32(Session["CompId"]);
                                var companypayroll = new CompanyPayroll();
                                companypayroll = db.CompanyPayroll.Include(e => e.PayScaleAssignment).Where(e => e.Company.Id == company_Id).SingleOrDefault();
                                DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };

                                PayScaleAssignment PayScaleAssignment = new PayScaleAssignment()
                                {

                                    PayScaleAgreement = CurrAgree,
                                    SalaryHead = item.SalaryHead,
                                    SalHeadFormula = item.SalHeadFormula,
                                    DBTrack = dbt
                                };
                                try
                                {
                                    db.PayScaleAssignment.Add(PayScaleAssignment);
                                    db.SaveChanges();

                                    if (companypayroll != null)
                                    {
                                        var payscaleassignment_list = new List<PayScaleAssignment>();
                                        payscaleassignment_list.Add(PayScaleAssignment);
                                        if (companypayroll.PayScaleAssignment != null)
                                        {
                                            payscaleassignment_list.AddRange(companypayroll.PayScaleAssignment);
                                        }
                                        companypayroll.PayScaleAssignment = payscaleassignment_list;
                                        db.CompanyPayroll.Attach(companypayroll);
                                        db.Entry(companypayroll).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                        db.Entry(companypayroll).State = System.Data.Entity.EntityState.Detached;


                                    }
                                }
                                catch (DbUpdateConcurrencyException)
                                {
                                    return RedirectToAction("Create", new { concurrencyError = true, id = PayScaleAssignment.Id });
                                }
                                catch (DataException /* dex */)
                                {
                                    Msg.Add(" Unable toData Carry Forward.Try again, and if the problem persists, see your system administrator");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }

                            }
                            ts.Complete();
                        }

                        Msg.Add("  Data Carry Forward successfully.  ");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);


                    }
                    else
                    {
                        return this.Json(new { success = true, responseText = "Record available for =" + CurrAgree.EffDate + " Assignment date", JsonRequestBehavior.AllowGet });

                    }
                }

                ////increment
                else if (newagreeval.ToUpper() == "INCREMENTACTIVITY")
                {
                    var ChkAssignment = db.PayScaleAgreement.Include(e => e.IncrActivity).Where(x => x.Id == CurrAgree.Id).FirstOrDefault();
                    var PrvAssignment = db.PayScaleAgreement.Include(x => x.IncrActivity)
                        .Include(x => x.IncrActivity.Select(y => y.IncrPolicy))
                         .Include(x => x.IncrActivity.Select(y => y.IncrList))
                          .Include(x => x.IncrActivity.Select(y => y.StagIncrPolicy))
                        .Where(x => x.Id == PrevAgree.Id).FirstOrDefault();


                    if (ChkAssignment.IncrActivity.Count == 0)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, new System.TimeSpan(1, 30, 0)))
                        {
                            List<IncrActivity> IncrAct = PrvAssignment.IncrActivity.ToList();
                            foreach (var item in IncrAct)
                            {
                                int company_Id = 0;
                                company_Id = Convert.ToInt32(Session["CompId"]);
                                var companypayroll = new CompanyPayroll();
                                companypayroll = db.CompanyPayroll.Include(e => e.PayScaleAssignment).Where(e => e.Company.Id == company_Id).SingleOrDefault();
                                DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };

                                IncrActivity IncrActivity = new IncrActivity()
                                {
                                    Name = item.Name == null ? "" : item.Name.Trim(),
                                    IncrList = item.IncrList,
                                    IncrPolicy = item.IncrPolicy,
                                    StagIncrPolicy = item.StagIncrPolicy,
                                    //FullDetails=c.FullDetails,
                                    DBTrack = dbt

                                };
                                try
                                {
                                    db.IncrActivity.Add(IncrActivity);
                                    db.SaveChanges();

                                    List<IncrActivity> IncrActivitylist = new List<IncrActivity>();
                                    IncrActivitylist.Add(IncrActivity);
                                    var ChkAssignment1 = db.PayScaleAgreement.Find(CurrAgree.Id);
                                    if (ChkAssignment1.IncrActivity.Count() > 0)
                                    {
                                        IncrActivitylist.AddRange(ChkAssignment1.IncrActivity);
                                    }

                                    ChkAssignment1.IncrActivity = IncrActivitylist;
                                    db.PayScaleAgreement.Attach(ChkAssignment1);
                                    db.Entry(ChkAssignment1).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    db.Entry(ChkAssignment1).State = System.Data.Entity.EntityState.Detached;

                                }
                                catch (DbUpdateConcurrencyException)
                                {
                                    return RedirectToAction("Create", new { concurrencyError = true, id = IncrActivity.Id });
                                }
                                catch (DataException /* dex */)
                                {
                                    Msg.Add(" Unable toData Carry Forward.Try again, and if the problem persists, see your system administrator");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }

                            }
                            ts.Complete();
                        }

                        Msg.Add("  Data Carry Forward successfully.  ");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);


                    }
                    else
                    {
                        return this.Json(new { success = true, responseText = "Record available for =" + CurrAgree.EffDate + " Assignment date", JsonRequestBehavior.AllowGet });

                    }


                }
                //// Promotion

                else if (newagreeval.ToUpper() == "PROMOTIONACTIVITY")
                {
                    var ChkAssignment = db.PayScaleAgreement.Include(e => e.PromoActivity).Where(x => x.Id == CurrAgree.Id).FirstOrDefault();
                    var PrvAssignment = db.PayScaleAgreement.Include(x => x.PromoActivity)
                        .Include(x => x.PromoActivity.Select(y => y.PromoPolicy))
                         .Include(x => x.PromoActivity.Select(y => y.PromoList))
                        // .Include(x => x.PromoActivity.Select(y => y.StagIncrPolicy))
                        .Where(x => x.Id == PrevAgree.Id).FirstOrDefault();


                    if (ChkAssignment.PromoActivity.Count == 0)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, new System.TimeSpan(1, 30, 0)))
                        {
                            List<PromoActivity> IncrAct = PrvAssignment.PromoActivity.ToList();
                            foreach (var item in IncrAct)
                            {
                                int company_Id = 0;
                                company_Id = Convert.ToInt32(Session["CompId"]);
                                var companypayroll = new CompanyPayroll();
                                companypayroll = db.CompanyPayroll.Include(e => e.PayScaleAssignment).Where(e => e.Company.Id == company_Id).SingleOrDefault();
                                DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };

                                PromoActivity PromoActivity = new PromoActivity()
                                {
                                    Name = item.Name == null ? "" : item.Name.Trim(),
                                    PromoList = item.PromoList,
                                    PromoPolicy = item.PromoPolicy,

                                    //FullDetails=c.FullDetails,
                                    DBTrack = dbt

                                };
                                try
                                {
                                    db.PromoActivity.Add(PromoActivity);
                                    db.SaveChanges();

                                    List<PromoActivity> PromoActivitylist = new List<PromoActivity>();
                                    PromoActivitylist.Add(PromoActivity);
                                    var ChkAssignment1 = db.PayScaleAgreement.Find(CurrAgree.Id);
                                    if (ChkAssignment1.PromoActivity.Count() > 0)
                                    {
                                        PromoActivitylist.AddRange(ChkAssignment1.PromoActivity);
                                    }

                                    ChkAssignment1.PromoActivity = PromoActivitylist;
                                    db.PayScaleAgreement.Attach(ChkAssignment1);
                                    db.Entry(ChkAssignment1).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    db.Entry(ChkAssignment1).State = System.Data.Entity.EntityState.Detached;

                                }
                                catch (DbUpdateConcurrencyException)
                                {
                                    return RedirectToAction("Create", new { concurrencyError = true, id = PromoActivity.Id });
                                }
                                catch (DataException /* dex */)
                                {
                                    Msg.Add(" Unable toData Carry Forward.Try again, and if the problem persists, see your system administrator");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }

                            }
                            ts.Complete();
                        }

                        Msg.Add("  Data Carry Forward successfully.  ");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);


                    }
                    else
                    {
                        return this.Json(new { success = true, responseText = "Record available for =" + CurrAgree.EffDate + " Assignment date", JsonRequestBehavior.AllowGet });

                    }


                    //return this.Json(new { success = true, responseText = "Record available for =" + CurrAgree.EffDate + " Assignment date", JsonRequestBehavior.AllowGet });
                }
                ////Transfer
                else if (newagreeval.ToUpper() == "TRANSFERACTIVITY")
                {
                    var ChkAssignment = db.PayScaleAgreement.Include(e => e.TransActivity).Where(x => x.Id == CurrAgree.Id).FirstOrDefault();
                    var PrvAssignment = db.PayScaleAgreement.Include(x => x.TransActivity)
                        .Include(x => x.TransActivity.Select(y => y.TranPolicy))
                         .Include(x => x.TransActivity.Select(y => y.TransList))
                        // .Include(x => x.PromoActivity.Select(y => y.StagIncrPolicy))
                        .Where(x => x.Id == PrevAgree.Id).FirstOrDefault();


                    if (ChkAssignment.TransActivity.Count == 0)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, new System.TimeSpan(1, 30, 0)))
                        {
                            List<TransActivity> IncrAct = PrvAssignment.TransActivity.ToList();
                            foreach (var item in IncrAct)
                            {
                                int company_Id = 0;
                                company_Id = Convert.ToInt32(Session["CompId"]);
                                var companypayroll = new CompanyPayroll();
                                companypayroll = db.CompanyPayroll.Include(e => e.PayScaleAssignment).Where(e => e.Company.Id == company_Id).SingleOrDefault();
                                DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };

                                TransActivity TransActivity = new TransActivity()
                                {
                                    Name = item.Name == null ? "" : item.Name.Trim(),
                                    TransList = item.TransList,
                                    TranPolicy = item.TranPolicy,

                                    //FullDetails=c.FullDetails,
                                    DBTrack = dbt

                                };
                                try
                                {
                                    db.TransActivity.Add(TransActivity);
                                    db.SaveChanges();

                                    List<TransActivity> TransActivitylist = new List<TransActivity>();
                                    TransActivitylist.Add(TransActivity);
                                    var ChkAssignment1 = db.PayScaleAgreement.Find(CurrAgree.Id);
                                    if (ChkAssignment1.TransActivity.Count() > 0)
                                    {
                                        TransActivitylist.AddRange(ChkAssignment1.TransActivity);
                                    }

                                    ChkAssignment1.TransActivity = TransActivitylist;
                                    db.PayScaleAgreement.Attach(ChkAssignment1);
                                    db.Entry(ChkAssignment1).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    db.Entry(ChkAssignment1).State = System.Data.Entity.EntityState.Detached;

                                }
                                catch (DbUpdateConcurrencyException)
                                {
                                    return RedirectToAction("Create", new { concurrencyError = true, id = TransActivity.Id });
                                }
                                catch (DataException /* dex */)
                                {
                                    Msg.Add(" Unable toData Carry Forward.Try again, and if the problem persists, see your system administrator");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }

                            }
                            ts.Complete();
                        }

                        Msg.Add("  Data Carry Forward successfully.  ");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);


                    }
                    else
                    {
                        return this.Json(new { success = true, responseText = "Record available for =" + CurrAgree.EffDate + " Assignment date", JsonRequestBehavior.AllowGet });

                    }
                    // return this.Json(new { success = true, responseText = "Record available for =" + CurrAgree.EffDate + " Assignment date", JsonRequestBehavior.AllowGet });
                }
                ////other Activity
                else if (newagreeval.ToUpper() == "OTHERACTIVITY")
                {
                    var ChkAssignment = db.PayScaleAgreement.Include(e => e.OthServiceBookActivity).Where(x => x.Id == CurrAgree.Id).FirstOrDefault();
                    var PrvAssignment = db.PayScaleAgreement.Include(x => x.OthServiceBookActivity)
                        .Include(x => x.OthServiceBookActivity.Select(y => y.OthServiceBookPolicy))
                         .Include(x => x.OthServiceBookActivity.Select(y => y.OtherSerBookActList))
                        // .Include(x => x.PromoActivity.Select(y => y.StagIncrPolicy))
                        .Where(x => x.Id == PrevAgree.Id).FirstOrDefault();


                    if (ChkAssignment.OthServiceBookActivity.Count == 0)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, new System.TimeSpan(1, 30, 0)))
                        {
                            List<OthServiceBookActivity> IncrAct = PrvAssignment.OthServiceBookActivity.ToList();
                            foreach (var item in IncrAct)
                            {
                                int company_Id = 0;
                                company_Id = Convert.ToInt32(Session["CompId"]);
                                var companypayroll = new CompanyPayroll();
                                companypayroll = db.CompanyPayroll.Include(e => e.PayScaleAssignment).Where(e => e.Company.Id == company_Id).SingleOrDefault();
                                DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };

                                OthServiceBookActivity OthServiceBookActivity = new OthServiceBookActivity()
                                {
                                    Name = item.Name == null ? "" : item.Name.Trim(),
                                    OtherSerBookActList = item.OtherSerBookActList,
                                    OthServiceBookPolicy = item.OthServiceBookPolicy,

                                    //FullDetails=c.FullDetails,
                                    DBTrack = dbt

                                };
                                try
                                {
                                    db.OthServiceBookActivity.Add(OthServiceBookActivity);
                                    db.SaveChanges();

                                    List<OthServiceBookActivity> OthServiceBookActivitylist = new List<OthServiceBookActivity>();
                                    OthServiceBookActivitylist.Add(OthServiceBookActivity);
                                    var ChkAssignment1 = db.PayScaleAgreement.Find(CurrAgree.Id);
                                    if (ChkAssignment1.OthServiceBookActivity.Count() > 0)
                                    {
                                        OthServiceBookActivitylist.AddRange(ChkAssignment1.OthServiceBookActivity);
                                    }

                                    ChkAssignment1.OthServiceBookActivity = OthServiceBookActivitylist;
                                    db.PayScaleAgreement.Attach(ChkAssignment1);
                                    db.Entry(ChkAssignment1).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    db.Entry(ChkAssignment1).State = System.Data.Entity.EntityState.Detached;

                                }
                                catch (DbUpdateConcurrencyException)
                                {
                                    return RedirectToAction("Create", new { concurrencyError = true, id = OthServiceBookActivity.Id });
                                }
                                catch (DataException /* dex */)
                                {
                                    Msg.Add(" Unable toData Carry Forward.Try again, and if the problem persists, see your system administrator");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }

                            }
                            ts.Complete();
                        }

                        Msg.Add("  Data Carry Forward successfully.  ");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);


                    }
                    else
                    {
                        return this.Json(new { success = true, responseText = "Record available for =" + CurrAgree.EffDate + " Assignment date", JsonRequestBehavior.AllowGet });

                    }
                    //return this.Json(new { success = true, responseText = "Record available for =" + CurrAgree.EffDate + " Assignment date", JsonRequestBehavior.AllowGet });
                }
                else
                {
                    return this.Json(new { success = true, responseText = "Record available for =" + CurrAgree.EffDate + " Assignment date", JsonRequestBehavior.AllowGet });

                }

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
                IEnumerable<PayScaleAgreement> PayScaleAgreement = null;
                if (gp.IsAutho == true)
                {
                    PayScaleAgreement = db.PayScaleAgreement.Include(e => e.PayScale).Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    PayScaleAgreement = db.PayScaleAgreement.Include(e => e.PayScale).AsNoTracking().ToList();
                }

                IEnumerable<PayScaleAgreement> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = PayScaleAgreement;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.EffDate.Value.ToShortDateString().ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                               || (e.EndDate != null ? e.EndDate.Value.ToShortDateString().ToUpper().ToString().Contains(gp.searchString.ToUpper()) : false)
                               || (e.Id.ToString().Contains(gp.searchString))
                               ).Select(a => new Object[] { a.EffDate.Value.ToShortDateString(), a.EndDate != null ? a.EndDate.Value.ToShortDateString() : "", a.Id }).ToList();


                        //jsonData = IE.Select(a => new { a.EffDate, a.EndDate, a.Id }).Where((e => (e.Id.ToString() == gp.searchString) || (e.EffDate.Value.ToString() == gp.searchString.ToLower()))).ToList();
                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Fees), Convert.ToString(a.Name), Convert.ToString(a.VenuType) != null ? Convert.ToString(a.VenuType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.EffDate != null ? a.EffDate.Value.ToString("dd/MM/yyyy") : "", a.EndDate != null ? a.EndDate.Value.ToString("dd/MM/yyyy") : "", a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = PayScaleAgreement;
                    Func<PayScaleAgreement, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "EffDate" ? c.EffDate.Value.ToString() :
                                            gp.sidx == "EndDate" ? c.EndDate.Value.ToString() : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.EffDate != null ? a.EffDate.Value.ToString("dd/MM/yyyy") : "", a.EndDate != null ? a.EndDate.Value.ToString("dd/MM/yyyy") : "", a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.EffDate != null ? a.EffDate.Value.ToString("dd/MM/yyyy") : "", a.EndDate != null ? a.EndDate.Value.ToString("dd/MM/yyyy") : "", a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.EffDate != null ? a.EffDate.Value.ToString("dd/MM/yyyy") : "", a.EndDate != null ? a.EndDate.Value.ToString("dd/MM/yyyy") : "", a.Id }).ToList();
                    }
                    totalRecords = PayScaleAgreement.Count();
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

        //    PayScaleAgreement PayScaleAgreement = db.PayScaleAgreement
        //        //.Include(e => e.Address)
        //                                       .Include(e => e.PayScale)
        //                                       .Where(e => e.Id == data).SingleOrDefault();

        //    // Address add = PayScaleAgreement.Address;
        //    PayScale PayScaleDet = PayScaleAgreement.PayScale;            
        //    //PayScaleAgreement PayScaleAgreement = db.PayScaleAgreement.Where(e => e.Id == data).SingleOrDefault();
        //    if (PayScaleAgreement.DBTrack.IsModified == true)
        //    {
        //        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //        {
        //            //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, PayScaleAgreement.DBTrack, PayScaleAgreement, null, "PayScaleAgreement");
        //            DBTrack dbT = new DBTrack
        //            {
        //                Action = "D",
        //                CreatedBy = PayScaleAgreement.DBTrack.CreatedBy != null ? PayScaleAgreement.DBTrack.CreatedBy : null,
        //                CreatedOn = PayScaleAgreement.DBTrack.CreatedOn != null ? PayScaleAgreement.DBTrack.CreatedOn : null,
        //                IsModified = PayScaleAgreement.DBTrack.IsModified == true ? true : false
        //            };
        //            PayScaleAgreement.DBTrack = dbT;
        //            db.Entry(PayScaleAgreement).State = System.Data.Entity.EntityState.Modified;
        //            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, PayScaleAgreement.DBTrack);
        //            DT_PayScaleAgreement DT_OBJ = (DT_PayScaleAgreement)rtn_Obj;                  
        //            DT_OBJ.PayScale_Id = PayScaleAgreement.PayScale == null ? 0 : PayScaleAgreement.PayScale.Id;
        //            db.Create(DT_OBJ);
        //            // db.SaveChanges();
        //            //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", PayScaleAgreement, null, "PayScaleAgreement", PayScaleAgreement.DBTrack);
        //            await db.SaveChangesAsync();
        //            //using (var context = new DataBaseContext())
        //            //{
        //            //   // DBTrackFile.DBTrackSave("Core/P2b.Global", "D", PayScaleAgreement, null, "PayScaleAgreement", PayScaleAgreement.DBTrack );
        //            //}
        //            ts.Complete();
        //            Msg.Add("  Data removed successfully.  ");
        //      return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //        }
        //    }
        //    else
        //    {
        //        //var selectedRegions = PayScaleAgreement.Regions;

        //        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //        {
        //            //if (selectedRegions != null)
        //            //{
        //            //    var corpRegion = new HashSet<int>(PayScaleAgreement.Regions.Select(e => e.Id));
        //            //    if (corpRegion.Count > 0)
        //            //    {
        //            //        return Json(new Object[] { "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
        //            //        // return Json(new Object[] { "", "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
        //            //    }
        //            //}

        //            try
        //            {
        //                DBTrack dbT = new DBTrack
        //                {
        //                    Action = "D",
        //                    ModifiedBy = SessionManager.UserName,
        //                    ModifiedOn = DateTime.Now,
        //                    CreatedBy = PayScaleAgreement.DBTrack.CreatedBy != null ? PayScaleAgreement.DBTrack.CreatedBy : null,
        //                    CreatedOn = PayScaleAgreement.DBTrack.CreatedOn != null ? PayScaleAgreement.DBTrack.CreatedOn : null,
        //                    IsModified = PayScaleAgreement.DBTrack.IsModified == true ? false : false//,
        //                    //AuthorizedBy = SessionManager.UserName,
        //                    //AuthorizedOn = DateTime.Now
        //                };

        //                // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };

        //                db.Entry(PayScaleAgreement).State = System.Data.Entity.EntityState.Deleted;
        //                var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dbT);
        //                DT_PayScaleAgreement DT_OBJ = (DT_PayScaleAgreement)rtn_Obj;
        //                // DT_OBJ.Address_Id = add == null ? 0 : add.Id;
        //                DT_OBJ.PayScale_Id = PayScaleDet == null ? 0 : PayScaleDet.Id;
        //                db.Create(DT_OBJ);

        //                await db.SaveChangesAsync();


        //                //using (var context = new DataBaseContext())
        //                //{
        //                //    PayScaleAgreement.Address = add;
        //                //    PayScaleAgreement.ContactDetails = conDet;
        //                //    PayScaleAgreement.VenuType = val;
        //                //    DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dbT);
        //                //    //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", PayScaleAgreement, null, "PayScaleAgreement", dbT);
        //                //}
        //                ts.Complete();
        //                Msg.Add("  Data removed successfully.  ");
        //      return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);


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
                    PayScaleAgreement PayScaleAgreement = db.PayScaleAgreement.Include(e => e.PayScale).Where(e => e.Id == data).SingleOrDefault();


                    PayScale PayScale = PayScaleAgreement.PayScale;
                    var id = int.Parse(Session["CompId"].ToString());
                    var company = db.Company.Where(e => e.Id == id).SingleOrDefault();
                    company.PayScaleAgreement.Where(e => e.Id == PayScaleAgreement.Id);
                    company.PayScaleAgreement = null;
                    db.Entry(company).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    Msg.Add("  Data removed successfully.  ");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    // Msg.Add("  Data removed successfully.  ");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);


                    //Corporate corporates = db.Corporate.Where(e => e.Id == data).SingleOrDefault();
                    if (PayScaleAgreement.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack, corporates, null, "Corporate");
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = PayScaleAgreement.DBTrack.CreatedBy != null ? PayScaleAgreement.DBTrack.CreatedBy : null,
                                CreatedOn = PayScaleAgreement.DBTrack.CreatedOn != null ? PayScaleAgreement.DBTrack.CreatedOn : null,
                                IsModified = PayScaleAgreement.DBTrack.IsModified == true ? true : false
                            };
                            PayScaleAgreement.DBTrack = dbT;
                            db.Entry(PayScaleAgreement).State = System.Data.Entity.EntityState.Modified;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, PayScaleAgreement.DBTrack);
                            DT_PayScaleAgreement DT_OBJ = (DT_PayScaleAgreement)rtn_Obj;
                            DT_OBJ.PayScale_Id = PayScaleAgreement.PayScale == null ? 0 : PayScaleAgreement.PayScale.Id;
                            db.Create(DT_OBJ);
                            await db.SaveChangesAsync();
                            ts.Complete();
                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);


                        }
                    }
                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            var payscaleagreementlist = db.Company.Include(e => e.PayScaleAgreement).ToList();
                            foreach (var s in payscaleagreementlist)
                            {
                                s.PayScaleAgreement = null;
                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                            }
                            try
                            {
                                DBTrack dbT = new DBTrack
                                {
                                    Action = "D",
                                    ModifiedBy = SessionManager.UserName,
                                    ModifiedOn = DateTime.Now,
                                    CreatedBy = PayScaleAgreement.DBTrack.CreatedBy != null ? PayScaleAgreement.DBTrack.CreatedBy : null,
                                    CreatedOn = PayScaleAgreement.DBTrack.CreatedOn != null ? PayScaleAgreement.DBTrack.CreatedOn : null,
                                    IsModified = PayScaleAgreement.DBTrack.IsModified == true ? false : false//,

                                };



                                //db.Entry(PayScaleAgreement).State = System.Data.Entity.EntityState.Deleted;
                                var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dbT);
                                DT_PayScaleAgreement DT_OBJ = (DT_PayScaleAgreement)rtn_Obj;
                                DT_OBJ.PayScale_Id = PayScale == null ? 0 : PayScale.Id;
                                db.Create(DT_OBJ);
                                await db.SaveChangesAsync();
                                ts.Complete();
                                Msg.Add("  Data removed successfully.  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);



                            }
                            catch (RetryLimitExceededException /* dex */)
                            {
                                Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
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

        [HttpPost]
        public ActionResult GetPayScaleLKDetails(List<int> SkipIds)
        {

            using (DataBaseContext db = new DataBaseContext())
            {
                //List<string> Ids = SkipIds.ToString();
                var fall = db.PayScale.Include(e => e.PayScaleType).Include(e => e.PayScaleArea.Select(r => r.LocationObj)).ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.PayScale.Include(e => e.PayScaleArea).Include(e => e.PayScaleArea.Select(r => r.LocationObj)).Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                    }
                }
                var P = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                return Json(P, JsonRequestBehavior.AllowGet);
            }
        }



        public int EditS(string PayScaleDetails, int data, PayScaleAgreement c, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (PayScaleDetails != null)
                {
                    if (PayScaleDetails != "")
                    {
                        var val = db.PayScale.Find(int.Parse(PayScaleDetails));
                        c.PayScale = val;

                        var add = db.PayScaleAgreement.Include(e => e.PayScale).Where(e => e.Id == data).SingleOrDefault();
                        IList<PayScaleAgreement> PSDetails = null;
                        if (add.PayScale != null)
                        {
                            PSDetails = db.PayScaleAgreement.Where(x => x.PayScale.Id == add.PayScale.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            PSDetails = db.PayScaleAgreement.Where(x => x.Id == data).ToList();
                        }
                        foreach (var s in PSDetails)
                        {
                            s.PayScale = c.PayScale;
                            db.PayScaleAgreement.Attach(s);
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
                    var PSdetails = db.PayScaleAgreement.Include(e => e.PayScale).Where(x => x.Id == data).ToList();
                    foreach (var s in PSdetails)
                    {
                        s.PayScale = null;
                        db.PayScaleAgreement.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }

                var CurCorp = db.PayScaleAgreement.Find(data);
                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    c.DBTrack = dbT;
                    PayScaleAgreement OBJ = new PayScaleAgreement()
                    {
                        EffDate = c.EffDate,
                        EndDate = c.EndDate,
                        Id = data,
                        DBTrack = c.DBTrack
                    };


                    db.PayScaleAgreement.Attach(OBJ);
                    db.Entry(OBJ).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(OBJ).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                    return 1;
                }
                return 0;
            }
        }
    }
}
