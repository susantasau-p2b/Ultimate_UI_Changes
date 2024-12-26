///
/// Created by Sarika
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

namespace P2BUltimate.Controllers.Core.MainController
{
     [AuthoriseManger]
    public class PromoActivityController : Controller
    {
    
        public ActionResult Index()
        {
            return View("~/Views/Core/MainViews/PromoActivity/Index.cshtml");
        }
        public ActionResult partial()
        {
            return View("~/Views/Shared/Core/_PromoPolicy.cshtml");
        }
      
        //private DataBaseContext db = new DataBaseContext();

        public ActionResult PopulateDropDownList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var qurey = db.PayScaleAgreement.Include(e => e.PromoActivity).ToList();
                var selected = (Object)null;
                if (data2 != "" && data != "0" && data2 != "0")
                {
                    selected = Convert.ToInt32(data2);
                }

                SelectList s = new SelectList(qurey, "Id", "FullDetails", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        public ActionResult Create(PromoActivity c, FormCollection form) //Create submit
        {
             List<string> Msg = new List<string>();
             using (DataBaseContext db = new DataBaseContext())
             {
                 try
                 {
                     string Category = form["Category"] == "0" ? "" : form["Category"];
                     string PromoPolicy = form["PromoPolicy_list"] == "0" ? "" : form["PromoPolicy_list"];
                     var PayscaleagreementdetailsCreatelist = form["PayscaleagreementdetailsCreatelist"] == "0" ? "" : form["PayscaleagreementdetailsCreatelist"];
                     if (Category != null)
                     {
                         if (Category != "")
                         {
                             var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "313").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(Category)).FirstOrDefault();  //db.LookupValue.Find(int.Parse(Category));
                             c.PromoList = val;
                         }
                     }
                     var PsAgreement = new PayScaleAgreement();
                     if (PayscaleagreementdetailsCreatelist != null && PayscaleagreementdetailsCreatelist != "")
                     {
                         PsAgreement = db.PayScaleAgreement.Find(int.Parse(PayscaleagreementdetailsCreatelist));
                     }

                     if (PromoPolicy != null)
                     {
                         if (PromoPolicy != "")
                         {

                             var val = db.PromoPolicy.Find(int.Parse(PromoPolicy));

                             c.PromoPolicy = val;
                         }
                     }

                     if (ModelState.IsValid)
                     {
                         using (TransactionScope ts = new TransactionScope())
                         {
                             if (db.PromoActivity.Any(o => o.Name == c.Name))
                             {
                                 Msg.Add("  Name Already Exists.  ");
                                 return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                 //return Json(new Object[] { "", "", "Name Already Exists.", JsonRequestBehavior.AllowGet });
                             }

                             c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                             PromoActivity PromoActivity = new PromoActivity()
                             {
                                 Name = c.Name == null ? "" : c.Name.Trim(),
                                 PromoList = c.PromoList,
                                 PromoPolicy = c.PromoPolicy,

                                 DBTrack = c.DBTrack
                             };
                             try
                             {
                                 db.PromoActivity.Add(PromoActivity);
                                 var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, c.DBTrack);
                                 DT_PromoActivity DT_Corp = (DT_PromoActivity)rtn_Obj;
                                 DT_Corp.PromoList_Id = c.PromoList == null ? 0 : c.PromoList.Id;
                                 DT_Corp.PromoPolicy_Id = c.PromoPolicy == null ? 0 : c.PromoPolicy.Id;
                                 db.Create(DT_Corp);
                                 db.SaveChanges();
                                 List<PromoActivity> promopolicylist = new List<PromoActivity>();
                                 promopolicylist.Add(PromoActivity);
                                 if (PsAgreement != null)
                                 {
                                     PsAgreement.PromoActivity = promopolicylist;
                                     db.PayScaleAgreement.Attach(PsAgreement);
                                     db.Entry(PsAgreement).State = System.Data.Entity.EntityState.Modified;
                                     db.SaveChanges();
                                     db.Entry(PsAgreement).State = System.Data.Entity.EntityState.Detached;

                                 }

                                 ts.Complete();
                                 Msg.Add("  Data Saved successfully  ");
                                 return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                 //return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                             }
                             catch (DbUpdateConcurrencyException)
                             {
                                 return RedirectToAction("Create", new { concurrencyError = true, id = c.Id });
                             }
                             catch (DataException /* dex */)
                             {
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
                         Msg.Add(errorMsg);
                         return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                         //return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
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

        public class Payscalegreement_details 
        {

            public int Payscaleagg_Id { get; set; }
            public string PayscaleagreementDetails { get; set; }
        
        }




        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.PromoActivity
                    .Include(e => e.PromoList)
                    .Include(e => e.PromoPolicy)
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        Name = e.Name,
                        PromoList_Id = e.PromoList.Id == null ? 0 : e.PromoList.Id,
                        PromoPolicy_Details = e.PromoPolicy.FullDetails == null ? "" : e.PromoPolicy.FullDetails,
                        PromoPolicy_Id = e.PromoPolicy.Id == null ? "" : e.PromoPolicy.Id.ToString(),
                        Action = e.DBTrack.Action
                    }).ToList();

                var PSA = db.PayScaleAgreement.Include(e => e.PromoActivity).Where(e => e.PromoActivity.Any(t => t.Id == data)).ToList();
                List<Payscalegreement_details> ObjPayscaleAgg = new List<Payscalegreement_details>();
                if (PSA != null)
                {
                    foreach (var ca in PSA)
                    {
                        ObjPayscaleAgg.Add(new Payscalegreement_details
                        {
                            Payscaleagg_Id = ca.Id,
                            PayscaleagreementDetails = ca.FullDetails
                        });

                    }


                }

                var add_data = db.PromoActivity
                  .Include(e => e.PromoList)
                    .Include(e => e.PromoPolicy)
                    .Where(e => e.Id == data)
                    .Select(e => new
                    {
                        PromoPolicy_Details = e.PromoPolicy.FullDetails == null ? "" : e.PromoPolicy.FullDetails,
                        PromoPolicy_Id = e.PromoPolicy.Id == null ? "" : e.PromoPolicy.Id.ToString(),
                    }).ToList();

                var W = db.DT_PromoActivity
                     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id,
                         Name = e.Name == null ? "" : e.Name,
                         PromoList_Val = e.PromoList_Id == 0 ? "" : db.LookupValue
                                    .Where(x => x.Id == e.PromoList_Id)
                                    .Select(x => x.LookupVal).FirstOrDefault(),
                         Address_Val = e.PromoPolicy_Id == 0 ? "" : db.PromoPolicy.Where(x => x.Id == e.PromoPolicy_Id).Select(x => x.FullDetails).FirstOrDefault(),
                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var Corp = db.PromoActivity.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, ObjPayscaleAgg, W, Auth, JsonRequestBehavior.AllowGet });
            }
         }

        [HttpPost]
        public async Task<ActionResult> EditSave(PromoActivity c, int data, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string Category = form["Category"] == "0" ? "" : form["Category"];
                    string PromoPolicy = form["PromoPolicy_list"] == "0" ? "" : form["PromoPolicy_list"];
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;

                    if (Category != null)
                    {
                        if (Category != "")
                        {
                            var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "313").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(Category)).FirstOrDefault();  //db.LookupValue.Find(int.Parse(Category));
                            c.PromoList = val;
                            c.PromoList_Id = int.Parse(Category);
                        }
                    }
                    if (PromoPolicy != null)
                    {
                        if (PromoPolicy != "")
                        {
                            c.PromoPolicy_Id = Convert.ToInt32(PromoPolicy);
                        }
                    }
                    if (Category != "")
                    {
                        var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "313").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(Category)).FirstOrDefault();// db.LookupValue.Find(int.Parse(Category));
                        c.PromoList = val;

                        var type = db.PromoActivity.Include(e => e.PromoList).Where(e => e.Id == data).SingleOrDefault();
                        IList<PromoActivity> typedetails = null;
                        if (type.PromoList != null)
                        {
                            typedetails = db.PromoActivity.Where(x => x.PromoList.Id == type.PromoList.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            typedetails = db.PromoActivity.Where(x => x.Id == data).ToList();
                        }
                        foreach (var s in typedetails)
                        {
                            s.PromoList = c.PromoList;
                            db.PromoActivity.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }

                    if (PromoPolicy != null)
                    {
                        if (PromoPolicy != "")
                        {
                            int ContId = Convert.ToInt32(PromoPolicy);
                            var val = db.PromoPolicy.Where(e => e.Id == ContId).SingleOrDefault();
                            c.PromoPolicy = val;
                            
                            var type = db.PromoActivity.Include(e => e.PromoPolicy).Where(e => e.Id == data).SingleOrDefault();
                            IList<PromoActivity> typedetails = null;
                            if (type.PromoPolicy != null)
                            {
                                typedetails = db.PromoActivity.Where(x => x.PromoPolicy.Id == type.PromoPolicy.Id && x.Id == data).ToList();
                            }
                            else
                            {
                                typedetails = db.PromoActivity.Where(x => x.Id == data).ToList();
                            }
                            foreach (var s in typedetails)
                            {
                                s.PromoPolicy = c.PromoPolicy;
                                db.PromoActivity.Attach(s);
                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                TempData["RowVersion"] = s.RowVersion;
                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                            }
                        }
                    }
                    else
                    {
                        var BusiTypeDetails = db.PromoActivity.Include(e => e.PromoPolicy).Where(x => x.Id == data).ToList();
                        foreach (var s in BusiTypeDetails)
                        {
                            s.PromoPolicy = null;
                            db.PromoActivity.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
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
                                    PromoActivity blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.PromoActivity.Where(e => e.Id == data).Include(e => e.PromoList)
                                                                .Include(e => e.PromoPolicy)
                                                                .SingleOrDefault();
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
                                    var m1 = db.PromoActivity.Where(e => e.Id == data).ToList();
                                    foreach (var s in m1)
                                    {
                                        // s.AppraisalPeriodCalendar = null;
                                        db.PromoActivity.Attach(s);
                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                        //await db.SaveChangesAsync();
                                        db.SaveChanges();
                                        TempData["RowVersion"] = s.RowVersion;
                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    }
                                    //int a = EditS(Category, PromoPolicy, data, c, c.DBTrack);
                                    var CurCorp = db.PromoActivity.Find(data);
                                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {
                                        //c.DBTrack = dbT;
                                        PromoActivity corp = new PromoActivity()
                                        {
                                            Name = c.Name,
                                            Id = data,
                                            PromoPolicy_Id=c.PromoPolicy_Id,
                                            PromoList_Id=c.PromoList_Id,
                                            DBTrack = c.DBTrack
                                        };


                                        db.PromoActivity.Attach(corp);
                                        db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                                        db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                       // return 1;
                                    }

                                    using (var context = new DataBaseContext())
                                    {
                                        var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                        DT_PromoActivity DT_Corp = (DT_PromoActivity)obj;
                                        DT_Corp.PromoPolicy_Id = blog.PromoPolicy == null ? 0 : blog.PromoPolicy.Id;
                                        DT_Corp.PromoList_Id = blog.PromoList == null ? 0 : blog.PromoList.Id;
                                        db.Create(DT_Corp);
                                        db.SaveChanges();
                                    }
                                    await db.SaveChangesAsync();
                                    ts.Complete();
                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = c.Id, Val = c.Name, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    // return Json(new Object[] { c.Id, c.Name, "Record Updated", JsonRequestBehavior.AllowGet });
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

                            PromoActivity blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            PromoActivity Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.PromoActivity.Where(e => e.Id == data).SingleOrDefault();
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
                            PromoActivity corp = new PromoActivity()
                            {
                                Name = c.Name,
                                Id = data,
                                DBTrack = c.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, corp, "PromoActivity", c.DBTrack);

                                Old_Corp = context.PromoActivity.Where(e => e.Id == data).Include(e => e.PromoPolicy)
                                    .Include(e => e.PromoList).SingleOrDefault();
                                DT_PromoActivity DT_Corp = (DT_PromoActivity)obj;
                                DT_Corp.PromoList_Id = DBTrackFile.ValCompare(Old_Corp.PromoList, c.PromoList);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
                                DT_Corp.PromoPolicy_Id = DBTrackFile.ValCompare(Old_Corp.PromoPolicy, c.PromoPolicy); //Old_Corp.PromoList == c.PromoList ? 0 : Old_Corp.PromoList == null && c.PromoList != null ? c.PromoList.Id : Old_Corp.PromoList.Id;
                                db.Create(DT_Corp);
                            }
                            blog.DBTrack = c.DBTrack;
                            db.PromoActivity.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = c.Name, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            // return Json(new Object[] { blog.Id, c.Name, "Record Updated", JsonRequestBehavior.AllowGet });
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
                            //PromoActivity corp = db.PromoActivity.Find(auth_id);
                            //PromoActivity corp = db.PromoActivity.AsNoTracking().FirstOrDefault(e => e.Id == auth_id);

                            PromoActivity corp = db.PromoActivity.Include(e => e.PromoList)
                                .Include(e => e.PromoPolicy)
                                .FirstOrDefault(e => e.Id == auth_id);

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

                            db.PromoActivity.Attach(corp);
                            db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            //db.SaveChanges();
                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corp.DBTrack);
                            DT_PromoActivity DT_Corp = (DT_PromoActivity)rtn_Obj;
                            DT_Corp.PromoPolicy_Id = corp.PromoPolicy == null ? 0 : corp.PromoPolicy.Id;
                            DT_Corp.PromoList_Id = corp.PromoList == null ? 0 : corp.PromoList.Id;
                            db.Create(DT_Corp);
                            await db.SaveChangesAsync();

                            ts.Complete();
                            Msg.Add("  Record Authorised");
                            return Json(new Utility.JsonReturnClass { Id = corp.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            // return Json(new Object[] { corp.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else if (auth_action == "M")
                    {

                        PromoActivity Old_Corp = db.PromoActivity.Include(e => e.PromoList)
                                                          .Include(e => e.PromoPolicy)
                                                          .Where(e => e.Id == auth_id).SingleOrDefault();



                        DT_PromoActivity Curr_Corp = db.DT_PromoActivity
                                                    .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                                    .OrderByDescending(e => e.Id)
                                                    .FirstOrDefault();

                        if (Curr_Corp != null)
                        {
                            PromoActivity corp = new PromoActivity();

                            string PromoPolicy = Curr_Corp.PromoPolicy_Id == null ? null : Curr_Corp.PromoPolicy_Id.ToString();
                            string PromoList = Curr_Corp.PromoList_Id == null ? null : Curr_Corp.PromoList_Id.ToString();
                            corp.Name = Curr_Corp.Name == null ? Old_Corp.Name : Curr_Corp.Name;
                            //      corp.Id = auth_id;

                            if (ModelState.IsValid)
                            {
                                try
                                {

                                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                    {
                                        corp.DBTrack = new DBTrack
                                        {
                                            CreatedBy = Old_Corp.DBTrack.CreatedBy == null ? null : Old_Corp.DBTrack.CreatedBy,
                                            CreatedOn = Old_Corp.DBTrack.CreatedOn == null ? null : Old_Corp.DBTrack.CreatedOn,
                                            Action = "M",
                                            ModifiedBy = Old_Corp.DBTrack.ModifiedBy == null ? null : Old_Corp.DBTrack.ModifiedBy,
                                            ModifiedOn = Old_Corp.DBTrack.ModifiedOn == null ? null : Old_Corp.DBTrack.ModifiedOn,
                                            AuthorizedBy = SessionManager.UserName,
                                            AuthorizedOn = DateTime.Now,
                                            IsModified = false
                                        };

                                        int a = EditS(PromoPolicy, PromoList, auth_id, corp, corp.DBTrack);
                                        await db.SaveChangesAsync();
                                        ts.Complete();
                                        Msg.Add("  Record Authorised");
                                        return Json(new Utility.JsonReturnClass { Id = corp.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        //return Json(new Object[] { corp.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
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
                                        //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                    }
                                    else
                                    {
                                        var databaseValues = (PromoActivity)databaseEntry.ToObject();
                                        corp.RowVersion = databaseValues.RowVersion;
                                    }
                                }
                                Msg.Add("Record modified by another user.So refresh it and try to save again.");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
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
                            PromoActivity corp = db.PromoActivity.AsNoTracking().Include(e => e.PromoList)
                                                                        .Include(e => e.PromoPolicy)
                                                                      .FirstOrDefault(e => e.Id == auth_id);


                            corp.DBTrack = new DBTrack
                            {
                                Action = "D",
                                ModifiedBy = corp.DBTrack.ModifiedBy != null ? corp.DBTrack.ModifiedBy : null,
                                CreatedBy = corp.DBTrack.CreatedBy != null ? corp.DBTrack.CreatedBy : null,
                                CreatedOn = corp.DBTrack.CreatedOn != null ? corp.DBTrack.CreatedOn : null,
                                IsModified = false,
                                AuthorizedBy = SessionManager.UserName,
                                AuthorizedOn = DateTime.Now
                            };

                            db.PromoActivity.Attach(corp);
                            db.Entry(corp).State = System.Data.Entity.EntityState.Deleted;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corp.DBTrack);
                            DT_PromoActivity DT_Corp = (DT_PromoActivity)rtn_Obj;
                            DT_Corp.PromoPolicy_Id = corp.PromoPolicy == null ? 0 : corp.PromoPolicy.Id;
                            DT_Corp.PromoList_Id = corp.PromoList == null ? 0 : corp.PromoList.Id;
                            db.Create(DT_Corp);
                            await db.SaveChangesAsync();
                            db.Entry(corp).State = System.Data.Entity.EntityState.Detached;
                            ts.Complete();
                            Msg.Add(" Record Authorised ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { "", "Record Authorised", JsonRequestBehavior.AllowGet });
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

        public class P2BGridData
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string FullDetails { get; set; }
          
        }

         

        public ActionResult P2BGrid(P2BGrid_Parameters gp, FormCollection form)
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

                    IEnumerable<P2BGridData> PromoactivityList = null;
                    List<P2BGridData> model = new List<P2BGridData>();
                    P2BGridData view = null;
                    var Payscaleagreement = (String)null;
                    // Payscaleagreement = form["Payscaleagreementdetailslist"] == "0" ? "" : form["Payscaleagreementdetailslist"];
                    if (gp.filter != null)
                    {
                        Payscaleagreement = gp.filter;
                    }
                    int Id = Convert.ToInt32(Payscaleagreement);

                    var BindPayscaleagreementList = db.PayScaleAgreement.Include(e => e.PromoActivity).Where(e => e.Id == Id).ToList();

                    foreach (var z in BindPayscaleagreementList)
                    {
                        if (z.PromoActivity != null)
                        {

                            foreach (var Promo in z.PromoActivity)
                            {
                                //var aa = db.Calendar.Where(e => e.Id == Sal.Id).SingleOrDefault();
                                view = new P2BGridData()
                                {
                                    Id = Promo.Id,

                                    Name = Promo.Name,
                                    FullDetails = Promo.FullDetails,
                                };
                                model.Add(view);

                            }
                        }

                    }

                    PromoactivityList = model;

                    IEnumerable<P2BGridData> IE;
                    if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                    {
                        IE = PromoactivityList;
                        if (gp.searchOper.Equals("eq"))
                        {

                            jsonData = IE.Where(e => (e.Name.ToUpper().Contains(gp.searchString.ToUpper()))
                            || (e.Id.ToString().Contains(gp.searchString))
                            ).Select(a => new Object[] { a.Name, a.Id }).ToList();
                            //if (gp.searchField == "Id")
                            //    jsonData = IE.Select(a => new { a.Name, a.Id }).Where((e => (e.Id.ToString().Contains(gp.searchString)))).ToList();
                            //if (gp.searchField == "Name")
                            //    jsonData = IE.Select(a => new {  a.Name, a.Id }).Where((e => (e.Name.ToString().Contains(gp.searchString)))).ToList();

                            //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.Name), Convert.ToString(a.FullDetails), a.Id }).ToList();
                        }
                        totalRecords = IE.Count();
                    }
                    else
                    {
                        IE = PromoactivityList;
                        Func<P2BGridData, dynamic> orderfuc;
                        if (gp.sidx == "Id")
                        {
                            orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                        }
                        else
                        {
                            orderfuc = (c => gp.sidx == "Name" ? c.Name.ToString() : "");
                        }
                        if (gp.sord == "asc")
                        {
                            IE = IE.OrderBy(orderfuc);
                            jsonData = IE.Select(a => new Object[] { Convert.ToString(a.Name), a.Id }).ToList();
                        }
                        else if (gp.sord == "desc")
                        {
                            IE = IE.OrderByDescending(orderfuc);
                            jsonData = IE.Select(a => new Object[] { Convert.ToString(a.Name), a.Id }).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] {  Convert.ToString(a.Name), a.Id}).ToList();
                        }
                        totalRecords = PromoactivityList.Count();
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
        }

        //public ActionResult P2BGrid(P2BGrid_Parameters gp)
        //{
        //    try
        //    {
        //        DataBaseContext db = new DataBaseContext();
        //        int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
        //        int pageSize = gp.rows;
        //        int totalPages = 0;
        //        int totalRecords = 0;
        //        var jsonData = (Object)null;
        //        IEnumerable<PromoActivity> PromoActivity = null;
        //        if (gp.IsAutho == true)
        //        {
        //            PromoActivity = db.PromoActivity.Include(e => e.PromoList).Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
        //        }
        //        else
        //        {
        //            PromoActivity = db.PromoActivity.Include(e => e.PromoList).AsNoTracking().ToList();
        //        }

        //        IEnumerable<PromoActivity> IE;
        //        if (!string.IsNullOrEmpty(gp.searchField))
        //        {
        //            IE = PromoActivity;
        //            if (gp.searchOper.Equals("eq"))
        //            {
        //                jsonData = IE.Select(a => new { a.Id,a.Name }).Where((e => (e.Id.ToString() == gp.searchString) ||(e.Name.ToLower() == gp.searchString.ToLower()))).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id,a.Name, a.PromoList != null ? Convert.ToString(a.PromoList.LookupVal) : "" }).ToList();
        //            }
        //            totalRecords = IE.Count();
        //        }
        //        else
        //        {
        //            IE = PromoActivity;
        //            Func<PromoActivity, dynamic> orderfuc;
        //            if (gp.sidx == "Id")
        //            {
        //                orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
        //            }
        //            else
        //            {
        //                orderfuc = (c =>
        //                                 gp.sidx == "Name" ? c.Name :
        //                                 gp.sidx == "PromoList" ? c.PromoList.LookupVal : "");
        //            }
        //            if (gp.sord == "asc")
        //            {
        //                IE = IE.OrderBy(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Name), a.PromoList != null ? Convert.ToString(a.PromoList.LookupVal) : "" }).ToList();
        //            }
        //            else if (gp.sord == "desc")
        //            {
        //                IE = IE.OrderByDescending(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Name), a.PromoList != null ? Convert.ToString(a.PromoList.LookupVal) : "" }).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Name, a.PromoList != null ? Convert.ToString(a.PromoList.LookupVal) : "" }).ToList();
        //            }
        //            totalRecords = PromoActivity.Count();
        //        }
        //        if (totalRecords > 0)
        //        {
        //            totalPages = (int)Math.Ceiling((float)totalRecords / (float)gp.rows);
        //        }
        //        if (gp.page > totalPages)
        //        {
        //            gp.page = totalPages;
        //        }
        //        var JsonData = new
        //        {
        //            page = gp.page,
        //            rows = jsonData,
        //            records = totalRecords,
        //            total = totalPages
        //        };
        //        return Json(JsonData, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    PromoActivity PromoActivitys = db.PromoActivity.Include(e => e.PromoList)
                                                       .Include(e => e.PromoPolicy)
                                                       .Where(e => e.Id == data).SingleOrDefault();

                    PromoPolicy add = PromoActivitys.PromoPolicy;
                    LookupValue val = PromoActivitys.PromoList;
                    //var id = int.Parse(Session["CompId"].ToString());
                    //var companypayroll = db.PayScaleAgreement.Include(e => e.PromoActivity).Where(e=>e.PromoActivity.Any(t=>t.Id == data))
                    //                    .ToList();
                    //companypayroll = null;
                    //db.Entry(companypayroll).State = System.Data.Entity.EntityState.Modified;
                    //db.SaveChanges();
                    //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });


                    //if (PromoActivitys.DBTrack.IsModified == true)
                    //{
                    //    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    //    {
                    //        DBTrack dbT = new DBTrack
                    //        {
                    //            Action = "D",
                    //            CreatedBy = PromoActivitys.DBTrack.CreatedBy != null ? PromoActivitys.DBTrack.CreatedBy : null,
                    //            CreatedOn = PromoActivitys.DBTrack.CreatedOn != null ? PromoActivitys.DBTrack.CreatedOn : null,
                    //            IsModified = PromoActivitys.DBTrack.IsModified == true ? true : false
                    //        };
                    //        PromoActivitys.DBTrack = dbT;
                    //        db.Entry(PromoActivitys).State = System.Data.Entity.EntityState.Modified;
                    //        var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, PromoActivitys.DBTrack);
                    //        DT_PromoActivity DT_Corp = (DT_PromoActivity)rtn_Obj;
                    //        DT_Corp.PromoPolicy_Id = PromoActivitys.PromoPolicy == null ? 0 : PromoActivitys.PromoPolicy.Id;
                    //        DT_Corp.PromoList_Id = PromoActivitys.PromoList == null ? 0 : PromoActivitys.PromoList.Id;
                    //        db.Create(DT_Corp);
                    //        await db.SaveChangesAsync();
                    //        ts.Complete();
                    //        return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                    //    }
                    //}

                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {

                        try
                        {
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now,
                                CreatedBy = PromoActivitys.DBTrack.CreatedBy != null ? PromoActivitys.DBTrack.CreatedBy : null,
                                CreatedOn = PromoActivitys.DBTrack.CreatedOn != null ? PromoActivitys.DBTrack.CreatedOn : null,
                                IsModified = PromoActivitys.DBTrack.IsModified == true ? false : false//,
                            };

                            // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };

                            db.Entry(PromoActivitys).State = System.Data.Entity.EntityState.Deleted;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dbT);
                            DT_PromoActivity DT_Corp = (DT_PromoActivity)rtn_Obj;
                            DT_Corp.PromoPolicy_Id = add == null ? 0 : add.Id;
                            DT_Corp.PromoList_Id = val == null ? 0 : val.Id;
                            db.Create(DT_Corp);
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

        public int EditS(string Category, string Addrs, int data, PromoActivity c, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (Category != null)
                {
                    if (Category != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(Category));
                        c.PromoList = val;

                        var type = db.PromoActivity.Include(e => e.PromoList).Where(e => e.Id == data).SingleOrDefault();
                        IList<PromoActivity> typedetails = null;
                        if (type.PromoList != null)
                        {
                            typedetails = db.PromoActivity.Where(x => x.PromoList.Id == type.PromoList.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            typedetails = db.PromoActivity.Where(x => x.Id == data).ToList();
                        }
                        foreach (var s in typedetails)
                        {
                            s.PromoList = c.PromoList;
                            db.PromoActivity.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                    else
                    {
                        var BusiTypeDetails = db.PromoActivity.Include(e => e.PromoList).Where(x => x.Id == data).ToList();
                        foreach (var s in BusiTypeDetails)
                        {
                            s.PromoList = null;
                            db.PromoActivity.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                }
                else
                {
                    var BusiTypeDetails = db.PromoActivity.Include(e => e.PromoList).Where(x => x.Id == data).ToList();
                    foreach (var s in BusiTypeDetails)
                    {
                        s.PromoList = null;
                        db.PromoActivity.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }

                if (Addrs != null)
                {
                    if (Addrs != "")
                    {
                        var val = db.PromoPolicy.Find(int.Parse(Addrs));
                        c.PromoPolicy = val;

                        var add = db.PromoActivity.Include(e => e.PromoPolicy).Where(e => e.Id == data).SingleOrDefault();
                        IList<PromoActivity> addressdetails = null;
                        if (add.PromoPolicy != null)
                        {
                            addressdetails = db.PromoActivity.Where(x => x.PromoPolicy.Id == add.PromoPolicy.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            addressdetails = db.PromoActivity.Where(x => x.Id == data).ToList();
                        }
                        if (addressdetails != null)
                        {
                            foreach (var s in addressdetails)
                            {
                                s.PromoPolicy = c.PromoPolicy;
                                db.PromoActivity.Attach(s);
                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                TempData["RowVersion"] = s.RowVersion;
                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                            }
                        }
                    }
                }
                else
                {
                    var addressdetails = db.PromoActivity.Include(e => e.PromoPolicy).Where(x => x.Id == data).ToList();
                    foreach (var s in addressdetails)
                    {
                        s.PromoPolicy = null;
                        db.PromoActivity.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }

                var CurCorp = db.PromoActivity.Find(data);
                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    c.DBTrack = dbT;
                    PromoActivity corp = new PromoActivity()
                    {
                        Name = c.Name,
                        Id = data,
                        DBTrack = c.DBTrack
                    };


                    db.PromoActivity.Attach(corp);
                    db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    return 1;
                }
                return 0;
            }
        }

        public void RollBack()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var changedEntries = db.ChangeTracker.Entries()
                    .Where(x => x.State != System.Data.Entity.EntityState.Unchanged).ToList();

                foreach (var entry in changedEntries.Where(x => x.State == System.Data.Entity.EntityState.Modified))
                {
                    entry.CurrentValues.SetValues(entry.OriginalValues);
                    entry.State = System.Data.Entity.EntityState.Unchanged;
                }

                foreach (var entry in changedEntries.Where(x => x.State == System.Data.Entity.EntityState.Added))
                {
                    entry.State = System.Data.Entity.EntityState.Detached;
                }

                foreach (var entry in changedEntries.Where(x => x.State == System.Data.Entity.EntityState.Deleted))
                {
                    entry.State = System.Data.Entity.EntityState.Unchanged;
                }

            }

        }

    }
}