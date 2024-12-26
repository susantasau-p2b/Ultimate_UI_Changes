///
/// Created by Sarika
///

using P2b.Global;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using P2BUltimate.Security;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace P2BUltimate.Controllers.Core.MainController
{
    public class IncrActivityController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();

        public ActionResult Index()
        {
            return View("~/Views/Core/MainViews/IncrActivity/Index.cshtml"); //"~/Views/Core/MainViews/IncrementActivity/IncrementActivity.cshtml"

        }

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
        public ActionResult Create(IncrActivity c, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string Category = form["Category"] == "0" ? "" : form["Category"];
                    string StagIncrPolicyList = form["StagIncrPolicyList"] == "0" ? "" : form["StagIncrPolicyList"];
                    string IncrPolicyList = form["IncrPolicyList"] == "0" ? "" : form["IncrPolicyList"];
                    var PayscaleagreementdetailsCreatelist = form["PayscaleagreementdetailsCreatelist"] == "0" ? "" : form["PayscaleagreementdetailsCreatelist"];
                    var PsAgreement = new PayScaleAgreement();
                    if (PayscaleagreementdetailsCreatelist != null && PayscaleagreementdetailsCreatelist != "")
                    {
                        PsAgreement = db.PayScaleAgreement.Find(int.Parse(PayscaleagreementdetailsCreatelist));
                    }
                    if (Category != null)
                    {
                        if (Category != "")
                        {
                            var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "307").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(Category)).FirstOrDefault();  //db.LookupValue.Find(int.Parse(Category));
                            c.IncrList = val;
                        }
                    }

                    if (StagIncrPolicyList != null)
                    {
                        if (StagIncrPolicyList != "")
                        {
                            var val = db.StagIncrPolicy.Find(int.Parse(StagIncrPolicyList));
                            c.StagIncrPolicy = val;
                        }
                    }
                    if (IncrPolicyList != null)
                    {
                        if (IncrPolicyList != "")
                        {
                            var val = db.IncrPolicy.Find(int.Parse(IncrPolicyList));
                            c.IncrPolicy = val;
                        }
                    }

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            if (db.IncrActivity.Any(o => o.Name == c.Name))
                            {
                                Msg.Add(" Name  Already Exists.  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // return Json(new Object[] { "", "", "Name  Already Exists.", JsonRequestBehavior.AllowGet });
                            }

                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName };



                            IncrActivity _incrActivity = new IncrActivity()
                            {
                                Name = c.Name == null ? "" : c.Name.Trim(),
                                IncrList = c.IncrList,
                                IncrPolicy = c.IncrPolicy,
                                StagIncrPolicy = c.StagIncrPolicy,
                                //FullDetails=c.FullDetails,
                                DBTrack = c.DBTrack
                            };
                            try
                            {
                                db.IncrActivity.Add(_incrActivity);
                                //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, c.DBTrack);
                                db.SaveChanges();

                                List<IncrActivity> IncrActivitylist = new List<IncrActivity>();
                                IncrActivitylist.Add(_incrActivity);
                                if (PsAgreement != null && PayscaleagreementdetailsCreatelist != null && PayscaleagreementdetailsCreatelist != "")
                                {
                                    PsAgreement.IncrActivity = IncrActivitylist;
                                    db.PayScaleAgreement.Attach(PsAgreement);
                                    db.Entry(PsAgreement).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    db.Entry(PsAgreement).State = System.Data.Entity.EntityState.Detached;

                                }
                                ts.Complete();
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = _incrActivity.Id, Val = _incrActivity.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return this.Json(new Object[] { _incrActivity.Id, _incrActivity.FullDetails, "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
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
                var Q = db.IncrActivity
                 .Include(e => e.IncrPolicy)
                 .Include(e => e.IncrList)
                 .Where(e => e.Id == data).Select
                 (e => new
                 {
                     Name = e.Name,
                     IncrList_id = e.IncrList.Id == null ? 0 : e.IncrList.Id,
                     IncrPolicy_FullDetails = e.IncrPolicy.FullDetails == null ? "" : e.IncrPolicy.FullDetails,
                     IncrPolicy_Id = e.IncrPolicy.Id == null ? "" : e.IncrPolicy.Id.ToString(),
                     StagIncrPolicy_FullDetails = e.StagIncrPolicy.FullDetails == null ? "" : e.StagIncrPolicy.FullDetails,
                     StagIncrPolicy_Id = e.StagIncrPolicy.Id == null ? "" : e.StagIncrPolicy.Id.ToString(),
                     Action = e.DBTrack.Action
                 }).ToList();

                //var add_data = db.IncrActivity
                //  .Include(e => e.IncrPolicy)
                //   .Where(e => e.Id == data)
                //    .Select(e => new
                //    {
                //        IncrPolicy_FullDetails = e.IncrPolicy.FullDetails == null ? "" : e.IncrPolicy.FullDetails,
                //        IncrPolicy_Id = e.IncrPolicy.Id == null ? "" : e.IncrPolicy.Id.ToString(),
                //        StagIncrPolicy_FullDetails = e.StagIncrPolicy.FullDetails == null ? "" : e.StagIncrPolicy.FullDetails,
                //        StagIncrPolicy_Id = e.StagIncrPolicy.Id == null ? "" : e.StagIncrPolicy.Id.ToString(),
                //    }).ToList();
                var PSA = db.PayScaleAgreement.Include(e => e.IncrActivity).Where(e => e.IncrActivity.Any(t => t.Id == data)).ToList();
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

                //var z = db.PayScaleAgreement.Include(e => e.IncrActivity).ToList();
                //PayScaleAgreement PayScaleAgr = null;
                //foreach (var p in z)
                //{
                //    foreach (var pay in p.IncrActivity)
                //    {
                //        if (pay.Id == data)
                //        {
                //            PayScaleAgr = p;
                //            break;
                //        }
                //    }
                //}


                //return Json(new Object[] { Q, add_data, JsonRequestBehavior.AllowGet });

                var W = db.DT_IncrActivity
                    //.Include(e => e.IncrPolicy)
                    //.Include(e => e.IncrList)
                     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id,
                         Name = e.Name == null ? "" : e.Name,
                         IncrList_val = e.IncrList_Id == null ? "" : e.IncrList_Id.ToString(),
                         IncrPolicy_val = e.IncrPolicy_Id == null ? "" : e.IncrPolicy_Id.ToString(),
                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var Corp = db.IncrActivity.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, ObjPayscaleAgg, W, Auth, JsonRequestBehavior.AllowGet });

            }

        }

        ////[HttpPost]
        //////[ValidateAntiForgeryToken]
        ////public ActionResult EditSave(IncrActivity c, int data, FormCollection form)
        ////{
        ////    string Category = form["Category"] == null ? "0" : form["Category"];
        ////    string stag_id_list = form["lookuplist"];
        ////    if (stag_id_list != null)
        ////    {
        ////        var stag_value = db.StagIncrPolicy.Find(int.Parse(stag_id_list));
        ////        c.IncrPolicy = stag_value;
        ////    }
        ////    if (Category != null)
        ////    {
        ////        var val = db.LookupValue.Find(int.Parse(Category));
        ////        c.IncrList = val;
        ////    }

        ////    if (ModelState.IsValid)
        ////    {
        ////        using (TransactionScope ts = new TransactionScope())
        ////        {
        ////            try
        ////            {
        ////                IncrActivity IncrActivity = new IncrActivity()
        ////                {
        ////                    Name = c.Name == null ? "" : c.Name.Trim(),
        ////                    IncrList = c.IncrList,
        ////                    IncrPolicy = c.IncrPolicy,
        ////                };
        ////                db.Entry(IncrActivity).State = System.Data.Entity.EntityState.Modified;
        ////                db.SaveChanges();
        ////                ts.Complete();
        ////                return this.Json(new { msg = "Data saved successfully." });
        ////            }
        ////            catch (DbUpdateConcurrencyException)
        ////            {
        ////                return RedirectToAction("Edit", new { concurrencyError = true, id = c.Id });
        ////            }
        ////            catch (DataException /* dex */)
        ////            {
        ////                //Log the error (uncomment dex variable name after DataException and add a line here to write a log.
        ////                ModelState.AddModelError(string.Empty, "Unable to Edit. Try again, and if the problem persists contact your system administrator.");
        ////                return RedirectToAction("Edit");
        ////            }
        ////        }
        ////    }
        ////    else
        ////    {
        ////        StringBuilder sb = new StringBuilder("");
        ////        foreach (ModelState modelState in ModelState.Values)
        ////        {
        ////            foreach (ModelError error in modelState.Errors)
        ////            {
        ////                sb.Append(error.ErrorMessage);
        ////                sb.Append("." + "\n");
        ////            }
        ////        }
        ////        var errorMsg = sb.ToString();
        ////        return this.Json(new { msg = errorMsg });
        ////    }
        ////}

        //public ActionResult partial()
        //{
        //    //ViewBag.StagIncrPolicy = GetStagIncrPolicy(0);
        //    return View("~/Views/Shared/Core/_StagIncrPolicy.cshtml");
        //}

        public ActionResult IncrApartial()
        {
            //ViewBag.StagIncrPolicy = GetStagIncrPolicy(0);
            return View("~/Views/Shared/Core/_IncrActivityP.cshtml");
        }

        public ActionResult EditStagIncrPolicy_partial(int? data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var r = (from ca in db.StagIncrPolicy
                         select new
                         {
                             Id = ca.Id,
                             Name = ca.Name,
                             SpanYears = ca.SpanYears,
                             MaxStagIncr = ca.MaxStagIncr,
                             IsLastIncr = ca.IsLastIncr,
                             IsFixAmount = ca.IsFixAmount,
                             IncrAmount = ca.IncrAmount
                         }).Where(e => e.Id == data).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult PopulateLookupDropDownList(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var lookupQuery = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "101").SingleOrDefault();
                List<SelectListItem> values = new List<SelectListItem>();
                if (lookupQuery != null)
                {
                    foreach (var item in lookupQuery.LookupValues)
                    {
                        if (item.IsActive == true)
                        {
                            values.Add(new SelectListItem
                            {
                                Text = item.LookupVal,
                                Value = item.Id.ToString(),
                                Selected = (item.Id == Convert.ToInt32(data) ? true : false)
                            });
                        }
                    }
                }
                return Json(values, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        public async Task<ActionResult> EditSave(IncrActivity c, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string Category = form["Category"] == "0" ? "" : form["Category"];
                    string StagIncrPolicyList = form["StagIncrPolicyList"] == "0" ? "" : form["StagIncrPolicyList"];
                    string IncrPolicyList = form["IncrPolicyList"] == "0" ? "" : form["IncrPolicyList"];
                    //  bool Auth = form["Autho_Action"] == "" ? false : true;
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;


                    if (Category != null)
                    {
                        if (Category != "")
                        {
                          
                            c.IncrList_Id = int.Parse(Category);
                        }
                    }

                    if (StagIncrPolicyList != null)
                    {
                        if (StagIncrPolicyList != "")
                        {
                           
                            c.StagIncrPolicy_Id = int.Parse(StagIncrPolicyList);
                        }
                    }
                    if (IncrPolicyList != null)
                    {
                        if (IncrPolicyList != "")
                        {
                         
                            c.IncrPolicy_Id = int.Parse(IncrPolicyList);
                        }
                    }
                    if (Category != null)
                    {
                        if (Category != "")
                        {
                            var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "307").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(Category)).FirstOrDefault();  //db.LookupValue.Find(int.Parse(Category));
                            c.IncrList = val;

                            var type = db.IncrActivity.Include(e => e.IncrList).Where(e => e.Id == data).SingleOrDefault();
                            IList<IncrActivity> typedetails = null;
                            if (type.IncrList != null)
                            {
                                typedetails = db.IncrActivity.Where(x => x.IncrList.Id == type.IncrList.Id && x.Id == data).ToList();
                            }
                            else
                            {
                                typedetails = db.IncrActivity.Where(x => x.Id == data).ToList();
                            }
                            //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                            foreach (var s in typedetails)
                            {
                                s.IncrList = c.IncrList;
                                db.IncrActivity.Attach(s);
                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                //await db.SaveChangesAsync();
                                db.SaveChanges();
                                TempData["RowVersion"] = s.RowVersion;
                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                            }
                        }
                    }
                    if (StagIncrPolicyList != null)
                    {
                        if (StagIncrPolicyList != "")
                        {
                            var val = db.StagIncrPolicy.Find(int.Parse(StagIncrPolicyList));
                            c.StagIncrPolicy = val;

                            var add = db.IncrActivity.Include(e => e.StagIncrPolicy).Where(e => e.Id == data).SingleOrDefault();
                            IList<IncrActivity> StagIncrPolicydetails = null;
                            if (add.StagIncrPolicy != null)
                            {
                                StagIncrPolicydetails = db.IncrActivity.Where(x => x.StagIncrPolicy.Id == add.StagIncrPolicy.Id && x.Id == data).ToList();
                            }
                            else
                            {
                                StagIncrPolicydetails = db.IncrActivity.Where(x => x.Id == data).ToList();
                            }
                            if (StagIncrPolicydetails != null)
                            {
                                foreach (var s in StagIncrPolicydetails)
                                {
                                    s.StagIncrPolicy = c.StagIncrPolicy;
                                    db.IncrActivity.Attach(s);
                                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                    // await db.SaveChangesAsync(false);
                                    db.SaveChanges();
                                    TempData["RowVersion"] = s.RowVersion;
                                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                }
                            }
                        }
                    }
                    else
                    {
                        var StagIncrPolicydetails = db.IncrActivity.Include(e => e.StagIncrPolicy).Where(x => x.Id == data).ToList();
                        foreach (var s in StagIncrPolicydetails)
                        {
                            s.StagIncrPolicy = null;
                            db.IncrActivity.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }

                    if (IncrPolicyList != null)
                    {
                        if (IncrPolicyList != "")
                        {
                            var val = db.IncrPolicy.Find(int.Parse(IncrPolicyList));
                            c.IncrPolicy = val;

                            var add = db.IncrActivity.Include(e => e.IncrPolicy).Where(e => e.Id == data).SingleOrDefault();
                            IList<IncrActivity> IncrPolicydetails = null;
                            if (add.IncrPolicy != null)
                            {
                                IncrPolicydetails = db.IncrActivity.Where(x => x.IncrPolicy.Id == add.IncrPolicy.Id && x.Id == data).ToList();
                            }
                            else
                            {
                                IncrPolicydetails = db.IncrActivity.Where(x => x.Id == data).ToList();
                            }
                            if (IncrPolicydetails != null)
                            {
                                foreach (var s in IncrPolicydetails)
                                {
                                    s.IncrPolicy = c.IncrPolicy;
                                    db.IncrActivity.Attach(s);
                                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                    // await db.SaveChangesAsync(false);
                                    db.SaveChanges();
                                    TempData["RowVersion"] = s.RowVersion;
                                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                }
                            }
                        }
                    }
                    else
                    {
                        var IncrPolicydetails = db.IncrActivity.Include(e => e.IncrPolicy).Where(x => x.Id == data).ToList();
                        foreach (var s in IncrPolicydetails)
                        {
                            s.IncrPolicy = null;
                            db.IncrActivity.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
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
                                    IncrActivity blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.IncrActivity.Where(e => e.Id == data).Include(e => e.IncrList)
                                                                .Include(e => e.IncrPolicy)
                                                                .AsNoTracking().SingleOrDefault();
                                        //originalBlogValues = context.Entry(blog).OriginalValues;
                                    }

                                    c.DBTrack = new DBTrack
                                    {
                                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };
                                    var m1 = db.IncrActivity.Where(e => e.Id == data).ToList();
                                    foreach (var s in m1)
                                    {
                                        // s.AppraisalPeriodCalendar = null;
                                        db.IncrActivity.Attach(s);
                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                        //await db.SaveChangesAsync();
                                        db.SaveChanges();
                                        TempData["RowVersion"] = s.RowVersion;
                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    }
                                    //int a = EditS(Category, PromoPolicy, data, c, c.DBTrack);
                                    var CurCorp = db.IncrActivity.Find(data);
                                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {


                                        IncrActivity corp = new IncrActivity()
                                        {
                                            IncrPolicy_Id=c.IncrPolicy_Id,
                                            StagIncrPolicy_Id=c.StagIncrPolicy_Id,
                                            IncrList_Id =c.IncrList_Id,
                                            Name = c.Name,
                                            Id = data,
                                            DBTrack = c.DBTrack
                                        };

                                        db.IncrActivity.Attach(corp);
                                        db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                                        db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                       // return 1;
                                    }

                                    using (var context = new DataBaseContext())
                                    {
                                        var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                        DT_IncrActivity DT_Corp = (DT_IncrActivity)obj;
                                        DT_Corp.IncrList_Id = blog.IncrList == null ? 0 : blog.IncrList.Id;
                                        DT_Corp.IncrPolicy_Id = blog.IncrPolicy == null ? 0 : blog.IncrPolicy.Id;
                                       
                                        db.Create(DT_Corp);
                                        db.SaveChanges();
                                    }
                                    await db.SaveChangesAsync();
                                    ts.Complete();
                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = c.Id, Val = c.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
                            IncrActivity Old_Corp = db.IncrActivity.Include(e => e.IncrList)
                                                                .Include(e => e.IncrPolicy)
                                                                .Where(e => e.Id == data).SingleOrDefault();

                            IncrActivity Curr_Corp = c;
                            c.DBTrack = new DBTrack
                            {
                                CreatedBy = Old_Corp.DBTrack.CreatedBy == null ? null : Old_Corp.DBTrack.CreatedBy,
                                CreatedOn = Old_Corp.DBTrack.CreatedOn == null ? null : Old_Corp.DBTrack.CreatedOn,
                                Action = "M",
                                IsModified = Old_Corp.DBTrack.IsModified == true ? true : false,
                                //ModifiedBy = SessionManager.UserName,
                                //ModifiedOn = DateTime.Now
                            };
                            Old_Corp.DBTrack = c.DBTrack;

                            db.Entry(Old_Corp).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            using (var context = new DataBaseContext())
                            {
                                //DBTrackFile.DBTrackSave("Core/P2b.Global", "M", Old_Corp, Curr_Corp, "IncrActivity", c.DBTrack);
                            }

                            ts.Complete();
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = Old_Corp.Id, Val = c.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            // return Json(new Object[] { Old_Corp.Id, c.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });
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

                    IEnumerable<P2BGridData> IncractivityList = null;
                    List<P2BGridData> model = new List<P2BGridData>();
                    P2BGridData view = null;
                    var Payscaleagreement = (String)null;
                    // Payscaleagreement = form["Payscaleagreementdetailslist"] == "0" ? "" : form["Payscaleagreementdetailslist"];
                    if (gp.filter != null)
                    {
                        Payscaleagreement = gp.filter;
                    }
                    int Id = Convert.ToInt32(Payscaleagreement);

                    var BindPayscaleagreementList = db.PayScaleAgreement.Include(e => e.IncrActivity).Where(e => e.Id == Id).ToList();

                    foreach (var z in BindPayscaleagreementList)
                    {
                        if (z.IncrActivity != null)
                        {

                            foreach (var Incr in z.IncrActivity)
                            {
                                //var aa = db.Calendar.Where(e => e.Id == Sal.Id).SingleOrDefault();
                                view = new P2BGridData()
                                {
                                    Id = Incr.Id,

                                    Name = Incr.Name,
                                    FullDetails = Incr.FullDetails,
                                };
                                model.Add(view);

                            }
                        }

                    }

                    IncractivityList = model;

                    IEnumerable<P2BGridData> IE;
                    if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                    {
                        IE = IncractivityList;
                        if (gp.searchOper.Equals("eq"))
                        {
                            jsonData = IE.Where(e => (e.Name.ToUpper().Contains(gp.searchString.ToUpper()))
                            || (e.Id.ToString().Contains(gp.searchString))
                            ).Select(a => new Object[] { a.Name, a.Id }).ToList();
                            //if (gp.searchField == "Id")
                            //    jsonData = IE.Select(a => new { a.Name, a.Id }).Where((e => (e.Id.ToString().Contains(gp.searchString)))).ToList();
                            //if (gp.searchField == "Name")
                            //    jsonData = IE.Select(a => new { a.Name, a.Id }).Where((e => (e.Name.ToString().Contains(gp.searchString)))).ToList();
                            
                            //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.Name), a.Id }).ToList();
                        }
                        totalRecords = IE.Count();
                    }
                    else
                    {
                        IE = IncractivityList;
                        Func<P2BGridData, dynamic> orderfuc;
                        if (gp.sidx == "Id")
                        {
                            orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                        }
                        else
                        {
                            orderfuc = (c => gp.sidx == "Name" ? c.Name.ToString() : "" 

                                          );
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
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.Name), a.Id }).ToList();
                        }
                        totalRecords = IncractivityList.Count();
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

        //        IEnumerable<IncrActivity> IncrActivity = null;
        //        if (gp.IsAutho == true)
        //        {
        //            IncrActivity = db.IncrActivity.Include(e => e.IncrList).Where(e => e.DBTrack.IsModified == true).ToList();
        //        }
        //        else
        //        {
        //            IncrActivity = db.IncrActivity.Include(e=>e.IncrList).ToList();
        //        }
        //        IEnumerable<IncrActivity> IE;

        //        if (!string.IsNullOrEmpty(gp.searchField))
        //        {
        //            IE = IncrActivity;
        //            if (gp.searchOper.Equals("eq"))
        //            {
        //                jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Name), Convert.ToString(a.IncrList.LookupVal) }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Name, a.IncrList != null ? Convert.ToString(a.IncrList.LookupVal) : "" }).ToList();
        //            }
        //            totalRecords = IE.Count();
        //        }
        //        else
        //        {
        //            IE = IncrActivity;
        //            Func<IncrActivity, string> orderfuc = (c =>
        //                                                       gp.sidx == "ID" ? c.Id.ToString() :
        //                                                       gp.sidx == "Name" ? c.Name :
        //                                                       gp.sidx == "IncrList" ? c.IncrList.LookupVal :
        //                                                         "");
        //            if (gp.sord == "asc")
        //            {
        //                IE = IE.OrderBy(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Name), a.IncrList != null ? Convert.ToString(a.IncrList.LookupVal) : "" }).ToList();
        //            }
        //            else if (gp.sord == "desc")
        //            {
        //                IE = IE.OrderByDescending(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Name), a.IncrList != null ? Convert.ToString(a.IncrList.LookupVal) : ""}).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Name, a.IncrList != null ? Convert.ToString(a.IncrList.LookupVal) : "" }).ToList();
        //            }
        //            totalRecords = IncrActivity.Count();
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




        public ActionResult GetLookupDetails(string OBJ)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.StagIncrPolicy.ToList();
                IEnumerable<StagIncrPolicy> all;
                if (!string.IsNullOrEmpty(OBJ))
                {
                    all = db.StagIncrPolicy.ToList().Where(d => d.Name.Contains(OBJ));
                }
                else
                {
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.Name }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.Name }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            // return View();
        }

        //public ActionResult GetStagPolicyLKDetails(string data)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var fall = db.StagIncrPolicy.ToList();
        //        IEnumerable<StagIncrPolicy> all;
        //        if (!string.IsNullOrEmpty(data))
        //        {
        //            all = db.StagIncrPolicy.ToList().Where(d => d.Name.Contains(data));

        //        }
        //        else
        //        {
        //            var list1 = db.IncrActivity.ToList().Select(e => e.IncrPolicy);
        //            var list2 = fall.Except(list1);

        //            //  var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.LookupVal }).Distinct();
        //            var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.Name }).Distinct();
        //            //var result_1 = (from c in fall
        //            //                select new { c.Id, c.CorporateCode, c.CorporateName });
        //            return Json(r, JsonRequestBehavior.AllowGet);
        //        }
        //        var result = (from c in all
        //                      select new { c.Id, c.Name }).Distinct();
        //        return Json(result, JsonRequestBehavior.AllowGet);
        //    }
        //    // return View();
        //}

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

                            IncrActivity corp = db.IncrActivity.Include(e => e.IncrPolicy)
                                       .Include(e => e.IncrPolicy).FirstOrDefault(e => e.Id == auth_id);

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

                            db.IncrActivity.Attach(corp);
                            db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            //db.SaveChanges();

                            await db.SaveChangesAsync();


                            using (var context = new DataBaseContext())
                            {

                                ////DBTrackFile.DBTrackSave("Core/P2b.Global", "M", corp, null, "IncrActivity", corp.DBTrack);
                            }

                            ts.Complete();
                            Msg.Add("  Record Authorised");
                            return Json(new Utility.JsonReturnClass { Id = corp.Id, Val = corp.Name, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { corp.Id, corp.Name, "Record Authorised", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else if (auth_action == "M")
                    {

                        IncrActivity Old_Corp = db.IncrActivity.Include(e => e.IncrList).Include(e => e.StagIncrPolicy)
                                                          .Include(e => e.IncrPolicy)
                                                          .Where(e => e.Id == auth_id).SingleOrDefault();


                        DT_IncrActivity Curr_Corp = db.DT_IncrActivity.Include(e => e.IncrList_Id)
                                                    .Include(e => e.IncrPolicy_Id)
                                                    .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                                    .OrderByDescending(e => e.Id)
                                                    .FirstOrDefault();

                        IncrActivity corp = new IncrActivity();

                        string Corp = Curr_Corp.IncrList_Id == null ? null : Curr_Corp.IncrList_Id.ToString();
                        string Addrs = Curr_Corp.IncrPolicy_Id == null ? null : Curr_Corp.IncrPolicy_Id.ToString();
                        // string stagincrpolicy = Curr_Corp.s == null ? null : Curr_Corp.IncrPolicy_Id.ToString();
                        corp.Name = Curr_Corp.Name == null ? Old_Corp.Name : Curr_Corp.Name;
                        //      corp.Id = auth_id;

                        if (ModelState.IsValid)
                        {
                            try
                            {

                                //DbContextTransaction transaction = db.Database.BeginTransaction();

                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    // db.Configuration.AutoDetectChangesEnabled = false;
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

                                    int a = EditS(Corp, Addrs, "", auth_id, corp, corp.DBTrack);


                                    await db.SaveChangesAsync();

                                    ts.Complete();
                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = corp.Id, Val = corp.Name, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    // return Json(new Object[] { corp.Id, corp.Name, "Record Updated", JsonRequestBehavior.AllowGet });
                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (IncrActivity)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (IncrActivity)databaseEntry.ToObject();
                                    corp.RowVersion = databaseValues.RowVersion;
                                }
                            }
                            Msg.Add("Record modified by another user.So refresh it and try to save again.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            // return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else if (auth_action == "D")
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //Corporate corp = db.Corporate.Find(auth_id);
                            IncrActivity corp = db.IncrActivity.AsNoTracking().Include(e => e.IncrList)
                                                                        .Include(e => e.IncrPolicy)
                                                                       .FirstOrDefault(e => e.Id == auth_id);

                            StagIncrPolicy add = corp.StagIncrPolicy;
                            LookupValue val = corp.IncrList;

                            corp.DBTrack = new DBTrack
                            {
                                Action = "D",
                                ModifiedBy = corp.DBTrack.ModifiedBy != null ? corp.DBTrack.ModifiedBy : null,
                                CreatedBy = corp.DBTrack.CreatedBy != null ? corp.DBTrack.CreatedBy : null,
                                CreatedOn = corp.DBTrack.CreatedOn != null ? corp.DBTrack.CreatedOn : null,
                                IsModified = corp.DBTrack.IsModified == true ? false : false,
                                AuthorizedBy = SessionManager.UserName,
                                AuthorizedOn = DateTime.Now
                            };

                            db.IncrActivity.Attach(corp);
                            db.Entry(corp).State = System.Data.Entity.EntityState.Deleted;
                            await db.SaveChangesAsync();
                            using (var context = new DataBaseContext())
                            {
                                corp.StagIncrPolicy = add;
                                corp.IncrList = val;
                                ////DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corp, null, "IncrActivity", corp.DBTrack);
                            }


                            db.Entry(corp).State = System.Data.Entity.EntityState.Detached;
                            ts.Complete();
                            Msg.Add(" Record Authorised ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            // return Json(new Object[] { "", "", "Record Authorised", JsonRequestBehavior.AllowGet });
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

        public int EditS(string Category, string IncrPolicyList, string StagIncrPolicyList, int data, IncrActivity c, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (Category != null)
                {
                    if (Category != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(Category));
                        c.IncrList = val;

                        var type = db.IncrActivity.Include(e => e.IncrList).Where(e => e.Id == data).SingleOrDefault();
                        IList<IncrActivity> typedetails = null;
                        if (type.IncrList != null)
                        {
                            typedetails = db.IncrActivity.Where(x => x.IncrList.Id == type.IncrList.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            typedetails = db.IncrActivity.Where(x => x.Id == data).ToList();
                        }
                        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                        foreach (var s in typedetails)
                        {
                            s.IncrList = c.IncrList;
                            db.IncrActivity.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                }


                if (StagIncrPolicyList != null)
                {
                    if (StagIncrPolicyList != "")
                    {
                        var val = db.StagIncrPolicy.Find(int.Parse(StagIncrPolicyList));
                        c.StagIncrPolicy = val;

                        var add = db.IncrActivity.Include(e => e.StagIncrPolicy).Where(e => e.Id == data).SingleOrDefault();
                        IList<IncrActivity> StagIncrPolicydetails = null;
                        if (add.StagIncrPolicy != null)
                        {
                            StagIncrPolicydetails = db.IncrActivity.Where(x => x.StagIncrPolicy.Id == add.StagIncrPolicy.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            StagIncrPolicydetails = db.IncrActivity.Where(x => x.Id == data).ToList();
                        }
                        if (StagIncrPolicydetails != null)
                        {
                            foreach (var s in StagIncrPolicydetails)
                            {
                                s.StagIncrPolicy = c.StagIncrPolicy;
                                db.IncrActivity.Attach(s);
                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                // await db.SaveChangesAsync(false);
                                db.SaveChanges();
                                TempData["RowVersion"] = s.RowVersion;
                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                            }
                        }
                    }
                }
                else
                {
                    var StagIncrPolicydetails = db.IncrActivity.Include(e => e.StagIncrPolicy).Where(x => x.Id == data).ToList();
                    foreach (var s in StagIncrPolicydetails)
                    {
                        s.StagIncrPolicy = null;
                        db.IncrActivity.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }


                if (IncrPolicyList != null)
                {
                    if (IncrPolicyList != "")
                    {
                        var val = db.IncrPolicy.Find(int.Parse(IncrPolicyList));
                        c.IncrPolicy = val;

                        var add = db.IncrActivity.Include(e => e.IncrPolicy).Where(e => e.Id == data).SingleOrDefault();
                        IList<IncrActivity> IncrPolicydetails = null;
                        if (add.IncrPolicy != null)
                        {
                            IncrPolicydetails = db.IncrActivity.Where(x => x.IncrPolicy.Id == add.IncrPolicy.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            IncrPolicydetails = db.IncrActivity.Where(x => x.Id == data).ToList();
                        }
                        if (IncrPolicydetails != null)
                        {
                            foreach (var s in IncrPolicydetails)
                            {
                                s.IncrPolicy = c.IncrPolicy;
                                db.IncrActivity.Attach(s);
                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                // await db.SaveChangesAsync(false);
                                db.SaveChanges();
                                TempData["RowVersion"] = s.RowVersion;
                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                            }
                        }
                    }
                }
                else
                {
                    var IncrPolicydetails = db.IncrActivity.Include(e => e.IncrPolicy).Where(x => x.Id == data).ToList();
                    foreach (var s in IncrPolicydetails)
                    {
                        s.IncrPolicy = null;
                        db.IncrActivity.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }

                //if (IncrPolicyList != null)
                //{
                //	if (IncrPolicyList != "")
                //	{
                //		var val = db.IncrPolicy.Find(int.Parse(IncrPolicyList));
                //		c.IncrPolicy = val;

                //		var add = db.IncrActivity.Include(e => e.IncrPolicy).Where(e => e.Id == data).SingleOrDefault();
                //		IList<IncrActivity> IncrPolicydetails = null;
                //		if (add.IncrPolicy != null)
                //		{
                //			IncrPolicydetails = db.IncrActivity.Where(x => x.IncrPolicy.Id == add.IncrPolicy.Id && x.Id == data).ToList();
                //		}
                //		else
                //		{
                //			IncrPolicydetails = db.IncrActivity.Where(x => x.Id == data).ToList();
                //		}
                //		if (IncrPolicydetails != null)
                //		{
                //			foreach (var s in IncrPolicydetails)
                //			{
                //				s.IncrPolicy = c.IncrPolicy;
                //				db.IncrActivity.Attach(s);
                //				db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                //				// await db.SaveChangesAsync(false);
                //				db.SaveChanges();
                //				TempData["RowVersion"] = s.RowVersion;
                //				db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                //			}
                //		}
                //	}
                //}
                //else
                //{
                //	var IncrPolicydetails = db.IncrActivity.Include(e => e.IncrPolicy).Where(x => x.Id == data).ToList();
                //	foreach (var s in IncrPolicydetails)
                //	{
                //		s.IncrPolicy = null;
                //		db.IncrActivity.Attach(s);
                //		db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                //		//await db.SaveChangesAsync();
                //		db.SaveChanges();
                //		TempData["RowVersion"] = s.RowVersion;
                //		db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                //	}
                //}


                var CurCorp = db.IncrActivity.Find(data);
                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    c.DBTrack = dbT;
                    IncrActivity corp = new IncrActivity()
                    {
                        Name = c.Name,
                        Id = data,
                        DBTrack = c.DBTrack
                    };


                    db.IncrActivity.Attach(corp);
                    db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                    return 1;
                }
                return 0;
            }
        }
        // return View();
        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    IncrActivity incrActivity = db.IncrActivity.Include(e => e.IncrPolicy)
                                                      .Include(e => e.IncrList).Where(e => e.Id == data).SingleOrDefault();

                    StagIncrPolicy add = incrActivity.StagIncrPolicy;
                    LookupValue val = incrActivity.IncrList;
                    //Corporate corporates = db.Corporate.Where(e => e.Id == data).SingleOrDefault();
                    if (incrActivity.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack, corporates, null, "Corporate");
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = incrActivity.DBTrack.CreatedBy != null ? incrActivity.DBTrack.CreatedBy : null,
                                CreatedOn = incrActivity.DBTrack.CreatedOn != null ? incrActivity.DBTrack.CreatedOn : null,
                                IsModified = incrActivity.DBTrack.IsModified == true ? true : false
                            };
                            incrActivity.DBTrack = dbT;
                            db.Entry(incrActivity).State = System.Data.Entity.EntityState.Modified;
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack);
                            await db.SaveChangesAsync();
                            using (var context = new DataBaseContext())
                            {
                                ////DBTrackFile.DBTrackSave("Core/P2b.Global", "D", incrActivity, null, "IncrActivity", incrActivity.DBTrack);
                            }
                            ts.Complete();
                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else
                    {
                        var selectedRegions = incrActivity.IncrPolicy;

                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //if (selectedRegions != null)
                            //{
                            //    var corpRegion = new HashSet<int>(incrActivity.IncrPolicy.Select(e => e.Id));
                            //    if (corpRegion.Count > 0)
                            //    {
                            //        return Json(new Object[] { "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                            //        // return Json(new Object[] { "", "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                            //    }
                            //}

                            try
                            {
                                DBTrack dbT = new DBTrack
                                {
                                    Action = "D",
                                    ModifiedBy = SessionManager.UserName,
                                    ModifiedOn = DateTime.Now,
                                    CreatedBy = incrActivity.DBTrack.CreatedBy != null ? incrActivity.DBTrack.CreatedBy : null,
                                    CreatedOn = incrActivity.DBTrack.CreatedOn != null ? incrActivity.DBTrack.CreatedOn : null,
                                    IsModified = incrActivity.DBTrack.IsModified == true ? false : false,
                                    AuthorizedBy = SessionManager.UserName,
                                    AuthorizedOn = DateTime.Now
                                };

                                // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };

                                db.Entry(incrActivity).State = System.Data.Entity.EntityState.Deleted;
                                await db.SaveChangesAsync();


                                using (var context = new DataBaseContext())
                                {
                                    incrActivity.StagIncrPolicy = add;
                                    incrActivity.IncrList = val;
                                    ////DBTrackFile.DBTrackSave("Core/P2b.Global", "D", incrActivity, null, "IncrActivity", dbT);
                                }
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
                                //return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
                            }
                        }
                    }
                    //return new EmptyResult();
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
        public void RollBack()
        {

            //  var context = DataContextFactory.GetDataContext();
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

        public ActionResult GetIncrActivityDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.IncrActivity.Include(e => e.IncrPolicy).ToList();
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.IncrActivity.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
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
