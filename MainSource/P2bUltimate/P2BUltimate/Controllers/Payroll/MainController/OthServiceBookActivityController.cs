///
/// Created by Sarika
///

using P2b.Global;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
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
using P2BUltimate.Security;
using System.Web.Script.Serialization;


namespace P2BUltimate.Controllers.Core.MainController
{
    public class OthServiceBookActivityController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();
        // GET: /        OthServiceBookActivity/
        public ActionResult Index()
        {
            return View("~/Views/Core/MainViews/OthServiceBookActivity/Index.cshtml");
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

                    IEnumerable<P2BGridData> OthServiceBookActivityList = null;
                    List<P2BGridData> model = new List<P2BGridData>();
                    P2BGridData view = null;
                    var Payscaleagreement = (String)null;
                    // Payscaleagreement = form["Payscaleagreementdetailslist"] == "0" ? "" : form["Payscaleagreementdetailslist"];
                    if (gp.filter != null)
                    {
                        Payscaleagreement = gp.filter;
                    }
                    int Id = Convert.ToInt32(Payscaleagreement);

                    var BindPayscaleagreementList = db.PayScaleAgreement.Include(e => e.OthServiceBookActivity.Select(c=>c.OtherSerBookActList))
                        .Where(e => e.Id == Id).ToList();

                    foreach (var z in BindPayscaleagreementList)
                    {
                      
                        if (z.OthServiceBookActivity != null)
                        {

                            foreach (var Othr in z.OthServiceBookActivity)
                            {
                            //var aa = db.OthServiceBookActivity.Include(e => e.OtherSerBookActList).Include(e => e.OtherSerBookActList).ToList();
                                //var aa = db.Calendar.Where(e => e.Id == Sal.Id).SingleOrDefault();
                                view = new P2BGridData()
                                {
                                    Id = Othr.Id,

                                    Name = Othr.Name,
                                    FullDetails = Othr.OtherSerBookActList.LookupVal,
                                };
                                model.Add(view);

                            }
                        }

                    }

                    OthServiceBookActivityList = model;

                    IEnumerable<P2BGridData> IE;
                    if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                    {
                        IE = OthServiceBookActivityList;
                        if (gp.searchOper.Equals("eq"))
                        {
                            jsonData = IE.Where(e => (e.Name.ToUpper().Contains(gp.searchString.ToUpper()))
                                 || (e.FullDetails.ToUpper().Contains(gp.searchString.ToUpper()))
                                  || (e.Id.ToString().Contains(gp.searchString))
                                ).Select(a => new Object[] {  a.Name, a.FullDetails, a.Id }).ToList();
                            //jsonData = IE.Select(a => new { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
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
                        IE = OthServiceBookActivityList;
                        Func<P2BGridData, dynamic> orderfuc;
                        if (gp.sidx == "Id")
                        {
                            orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                        }
                        else
                        {
                            orderfuc = (c => gp.sidx == "Name" ? c.Name.ToString() :
                                             gp.sidx == "Default" ? c.FullDetails.ToString() : ""

                                          );
                        }
                        if (gp.sord == "asc")
                        {
                            IE = IE.OrderBy(orderfuc);
                            jsonData = IE.Select(a => new Object[] {  Convert.ToString(a.Name), Convert.ToString(a.FullDetails), a.Id }).ToList();
                        }
                        else if (gp.sord == "desc")
                        {
                            IE = IE.OrderByDescending(orderfuc);
                            jsonData = IE.Select(a => new Object[] {  Convert.ToString(a.Name), Convert.ToString(a.FullDetails), a.Id }).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] {  Convert.ToString(a.Name), Convert.ToString(a.FullDetails), a.Id }).ToList();
                        }
                        totalRecords = OthServiceBookActivityList.Count();
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

        //        IEnumerable<OthServiceBookActivity> OthServiceBookActivity = null;
        //        if (gp.IsAutho == true)
        //        {
        //            OthServiceBookActivity = db.OthServiceBookActivity.Include(e => e.OtherSerBookActList).Include(e => e.OthServiceBookPolicy).Where(e => e.DBTrack.IsModified == true).ToList();
        //        }
        //        else
        //        {
        //            OthServiceBookActivity = db.OthServiceBookActivity.Include(e => e.OtherSerBookActList).Include(e => e.OtherSerBookActList).ToList();
        //        }
        //        IEnumerable<OthServiceBookActivity> IE;

        //        if (!string.IsNullOrEmpty(gp.searchField))
        //        {
        //            IE = OthServiceBookActivity;
        //            if (gp.searchOper.Equals("eq"))
        //            {
        //                jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Name), Convert.ToString(a.OtherSerBookActList.Id) }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Name, a.OtherSerBookActList.Id }).ToList();
        //            }
        //            totalRecords = IE.Count();
        //        }
        //        else
        //        {
        //            IE = OthServiceBookActivity;
        //            Func<OthServiceBookActivity, string> orderfuc = (c =>
        //                                                       gp.sidx == "ID" ? c.Id.ToString() :
        //                                                       gp.sidx == "Name" ? c.Name :
        //                                                       //gp.sidx == "OtherSerBookActList" ? c.OtherSerBookActList.Id.ToString() :
        //                                                       gp.sidx == "OtherSerBookActList" ? c.OtherSerBookActList.LookupVal :
        //                                                         "");
        //            if (gp.sord == "asc")
        //            {
        //                IE = IE.OrderBy(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Name), Convert.ToString(a.OtherSerBookActList.LookupVal) }).ToList();
        //            }
        //            else if (gp.sord == "desc")
        //            {
        //                IE = IE.OrderByDescending(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Name), Convert.ToString(a.OtherSerBookActList.LookupVal) }).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Name, a.OtherSerBookActList.LookupVal }).ToList();
        //            }
        //            totalRecords = OthServiceBookActivity.Count();
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
                //var selected = "";
                //if (!string.IsNullOrEmpty(data2))
                //{
                //    selected = data2;
                //}
                SelectList s = new SelectList(qurey, "Id", "FullDetails", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult Create(OthServiceBookActivity c, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string Category = form["Category"] == "0" ? "" : form["Category"];
                    string Addrs = form["StagIncrPolicyList"] == "0" ? "" : form["StagIncrPolicyList"];
                    string OthServiceBookPolicyList = form["OthServiceBookPolicyList"] == "0" ? "" : form["OthServiceBookPolicyList"];
                    var PayscaleagreementdetailsCreatelist = form["PayscaleagreementdetailsCreatelist"] == "0" ? "" : form["PayscaleagreementdetailsCreatelist"];
                    var PsAgreement = new PayScaleAgreement();
                    if (PayscaleagreementdetailsCreatelist != null && PayscaleagreementdetailsCreatelist != "")
                    {
                        PsAgreement = db.PayScaleAgreement.Find(int.Parse(PayscaleagreementdetailsCreatelist));
                    }
                    var chk = db.OthServiceBookActivity.Any(e => e.Name ==c.Name);
                    if (chk == true)
                    {
                        Msg.Add("Name Already Exist.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    if (Category != null)
                    {
                        if (Category != "")
                        {
                            var val = db.LookupValue.Find(int.Parse(Category));
                            c.OtherSerBookActList = val;
                        }
                    }
                    if (OthServiceBookPolicyList != null && OthServiceBookPolicyList != "")
                    {
                        int OthServiceBookPolicyid=Convert.ToInt32(OthServiceBookPolicyList);
                        var value = db.OthServiceBookPolicy.Where(e=>e.Id==OthServiceBookPolicyid).SingleOrDefault();
                        c.OthServiceBookPolicy = value;

                    }
                    //if (OthServiceBookPolicyList != null && OthServiceBookPolicyList != "")
                    //{
                    //    var value = db.OthServiceBookPolicy.Find(int.Parse(OthServiceBookPolicyList));
                    //    c.OthServiceBookPolicy = value;

                    //}

                    //if (Addrs != null)
                    //{
                    //    if (Addrs != "")
                    //    {
                    //        int AddId = Convert.ToInt32(Addrs);
                    //        var val = db.OthServiceBookPolicy
                    //            //.Include(e => e.Name)
                    //            //.Include(e => e.SpanYears)
                    //            //.Include(e => e.MaxStagIncr)
                    //            //.Include(e => e.IsLastIncr)
                    //            //.Include(e => e.IsFixAmount)
                    //            //.Include(e => e.IncrAmount)
                    //            .Where(e => e.Id == AddId).SingleOrDefault();
                    //        c.OthServiceBookPolicy = val;
                    //    }
                    //}


                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            if (db.IncrActivity.Any(o => o.Name == c.Name))
                            {
                                //return Json(new Object[] { "", "", "Code Already Exists.", JsonRequestBehavior.AllowGet });

                                List<string> Msge = new List<string>();
                                Msge.Add("Code Already Exists.");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msge }, JsonRequestBehavior.AllowGet);
                            }

                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName };



                            OthServiceBookActivity othServiceBookAct = new OthServiceBookActivity()
                    {
                        Name = c.Name == null ? "" : c.Name.Trim(),
                        OtherSerBookActList = c.OtherSerBookActList,
                        OthServiceBookPolicy = c.OthServiceBookPolicy,
                        FullDetails = c.FullDetails,
                        DBTrack = c.DBTrack
                    };
                            try
                            {
                                db.OthServiceBookActivity.Add(othServiceBookAct);
                                //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, c.DBTrack);
                                db.SaveChanges();

                                List<OthServiceBookActivity> OthServiceBookActivitylist = new List<OthServiceBookActivity>();
                                OthServiceBookActivitylist.Add(othServiceBookAct);
                                if (PsAgreement != null)
                                {
                                    PsAgreement.OthServiceBookActivity = OthServiceBookActivitylist;
                                    db.PayScaleAgreement.Attach(PsAgreement);
                                    db.Entry(PsAgreement).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    db.Entry(PsAgreement).State = System.Data.Entity.EntityState.Detached;

                                }
                                ts.Complete();
                                //return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });

                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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

                        // return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
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

        public ActionResult Edit(int data,FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
              
                var Q = db.OthServiceBookActivity
                  .Include(e => e.OthServiceBookPolicy)
                  .Include(e => e.OtherSerBookActList)
                  .Where(e => e.Id == data).Select
                  (e => new
                  {
                      Name = e.Name,
                      OtherSerBookActList_id = e.OtherSerBookActList.Id == null ? 0 : e.OtherSerBookActList.Id,
                      Otherservicebookpolicy_Id = e.OthServiceBookPolicy == null ? 0 : e.OthServiceBookPolicy.Id,
                      Otherservicebookpolicy_fulldetails = e.OthServiceBookPolicy.FullDetails == null ? "" : e.OthServiceBookPolicy.FullDetails,
                      Action = e.DBTrack.Action
                  }).ToList();
                var PSA = db.PayScaleAgreement.Include(e => e.OthServiceBookActivity).Where(e=>e.OthServiceBookActivity.Any(t => t.Id == data)).ToList();
                //var PSA = db.PayScaleAgreement.Include(e => e.IncrActivity).Where(e => e.IncrActivity.Any(t => t.Id == data)).ToList();
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
                //var   BindPayscaleagreementList = db.PayScaleAgreement.Include(e => e.OthServiceBookActivity)
                //        .Where(e => e.Id == 1)
                //        .Select(e => new
                //        {
                //            id= e.Id,
                //        }
                //        ).ToList();
              
              //  var eff = db.PayScaleAgreement.Include(e => e.EffDate)
                var add_data = db.OthServiceBookActivity
                  .Include(e => e.OthServiceBookPolicy)
                   .Where(e => e.Id == data)
                    .Select(e => new
                    {
                        IncrPolicy_FullName = e.OthServiceBookPolicy.Name == null ? "" : e.OthServiceBookPolicy.Name,
                        IncrPolicy_Id = e.OthServiceBookPolicy.Id == null ? "" : e.OthServiceBookPolicy.Id.ToString(),
                    }).ToList();
                
                //TempData["RowVersion"] = db.IncrActivity.Find(data).RowVersion;

                //return Json(new Object[] { Q, add_data, JsonRequestBehavior.AllowGet });
               
                var W = db.DT_OthServiceBookActivity
                    .Include(e => e.OthServiceBookPolicy_Id)
                    .Include(e => e.OtherSerBookActList_Id)
                     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id,
                         Name = e.Name == null ? "" : e.Name,
                         OtherSerBookActList_val = e.OtherSerBookActList_Id == null ? "" : e.OtherSerBookActList_Id.ToString(),
                         OthServiceBookPolicy_val = e.OthServiceBookPolicy_Id == null ? "" : e.OthServiceBookPolicy_Id.ToString()
                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();
              
                var Corp = db.OthServiceBookActivity.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, ObjPayscaleAgg,add_data, W, Auth, JsonRequestBehavior.AllowGet });
             //   return Json(new Object[] {Q, Auth, JsonRequestBehavior.AllowGet });

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

                            OthServiceBookActivity corp = db.OthServiceBookActivity.Include(e => e.OtherSerBookActList)
                               .Include(e => e.OthServiceBookPolicy).FirstOrDefault(e => e.Id == auth_id);

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

                            db.OthServiceBookActivity.Attach(corp);
                            db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            //db.SaveChanges();

                            await db.SaveChangesAsync();


                            using (var context = new DataBaseContext())
                            {

                                //////DBTrackFile.DBTrackSave("Core/P2b.Global", "M", corp, null, "        OthServiceBookActivity", corp.DBTrack);
                            }

                            ts.Complete();
                            Msg.Add("  Record Authorised");
                            return Json(new Utility.JsonReturnClass { Id = corp.Id, Val = corp.Name, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            //return Json(new Object[] { corp.Id, corp.Name, "Record Authorised", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else if (auth_action == "M")
                    {

                        OthServiceBookActivity Old_Corp = db.OthServiceBookActivity.Include(e => e.OtherSerBookActList)
                                                  .Include(e => e.OthServiceBookPolicy)
                                                  .Where(e => e.Id == auth_id).SingleOrDefault();


                        DT_OthServiceBookActivity Curr_Corp = db.DT_OthServiceBookActivity.Include(e => e.OtherSerBookActList_Id.ToString())
                                                    .Include(e => e.OthServiceBookPolicy_Id)
                                                    .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                                    .OrderByDescending(e => e.Id)
                                                    .FirstOrDefault();

                        OthServiceBookActivity corp = new OthServiceBookActivity();

                        string Corp = Curr_Corp.OtherSerBookActList_Id == null ? null : Curr_Corp.OthServiceBookPolicy_Id.ToString();
                        string Addrs = Curr_Corp.OthServiceBookPolicy_Id == null ? null : Curr_Corp.OthServiceBookPolicy_Id.ToString();
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

                                    int a = EditS(Corp, Addrs, auth_id, corp, corp.DBTrack);


                                    await db.SaveChangesAsync();

                                    ts.Complete();
                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = corp.Id, Val = corp.Name, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new Object[] { corp.Id, corp.Name, "Record Updated", JsonRequestBehavior.AllowGet });
                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (OthServiceBookActivity)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (OthServiceBookActivity)databaseEntry.ToObject();
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
                            OthServiceBookActivity corp = db.OthServiceBookActivity.AsNoTracking().Include(e => e.OtherSerBookActList)
                                                                .Include(e => e.OthServiceBookPolicy)
                                                               .FirstOrDefault(e => e.Id == auth_id);

                            OthServiceBookPolicy add = corp.OthServiceBookPolicy;
                            LookupValue val = corp.OtherSerBookActList;

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

                            db.OthServiceBookActivity.Attach(corp);
                            db.Entry(corp).State = System.Data.Entity.EntityState.Deleted;
                            await db.SaveChangesAsync();
                            using (var context = new DataBaseContext())
                            {
                                corp.OthServiceBookPolicy = add;
                                corp.OtherSerBookActList = val;
                                //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corp, null, "        OthServiceBookActivity", corp.DBTrack);
                            }


                            db.Entry(corp).State = System.Data.Entity.EntityState.Detached;
                            ts.Complete();
                            Msg.Add(" Record Authorised ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            // return Json(new Object[] { "", "", "Record Authorised", JsonRequestBehavior.AllowGet });
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

        //   [HttpPost]
        //public async Task<ActionResult> EditSave(OthServiceBookActivity c, int data, FormCollection form) // Edit submit
        //{
        //    List<string> Msg = new List<string>();
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        try
        //        {
        //            var Category = form["Category"] == "0" ? "" : form["Category"];
        //            string Addrs = form["StagIncrPolicyList"] == "0" ? "" : form["StagIncrPolicyList"];
        //            string OthServiceBookPolicyList = form["OthServiceBookPolicyList"] == "0" ? "" : form["OthServiceBookPolicyList"];
        //            //  bool Auth = form["Autho_Action"] == "" ? false : true;
        //            bool Auth = form["Autho_Allow"] == "true" ? true : false;
        //            var cat1 = db.LookupValue.Find(int.Parse(Category));
        //            c.OtherSerBookActList = cat1;
        //            var other1 = db.OthServiceBookPolicy.Find(int.Parse(OthServiceBookPolicyList));
        //            c.OthServiceBookPolicy = other1;
        //            if (Auth == false)
        //            {


        //                if (ModelState.IsValid)
        //                {
        //                    try
        //                    {

        //                        //DbContextTransaction transaction = db.Database.BeginTransaction();

        //                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                        {
        //                            OthServiceBookActivity blog = null; // to retrieve old data
        //                            DbPropertyValues originalBlogValues = null;

        //                            using (var context = new DataBaseContext())
        //                            {
        //                                blog = context.OthServiceBookActivity.Where(e => e.Id == data).Include(e => e.OtherSerBookActList)
        //                                                        .Include(e => e.OthServiceBookPolicy)
        //                                                        .SingleOrDefault();
        //                                originalBlogValues = context.Entry(blog).OriginalValues;
        //                            }

        //                            c.DBTrack = new DBTrack
        //                            {
        //                                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                                Action = "M",
        //                                ModifiedBy = SessionManager.UserName,
        //                                ModifiedOn = DateTime.Now
        //                            };

        //                            // int a = EditS(Category, OthServiceBookPolicyList, data, c, c.DBTrack);

        //                            var m1 = db.OthServiceBookActivity.Where(e => e.Id == data).ToList();
        //                            foreach (var s in m1)
        //                            {
        //                                db.OthServiceBookActivity.Attach(s);
        //                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                                //await db.SaveChangesAsync();
        //                                db.SaveChanges();
        //                                TempData["RowVersion"] = s.RowVersion;
        //                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                            }
        //                            //TempData["RowVersion"] = c.RowVersion;
        //                            var CurCorp = db.OthServiceBookActivity.Find(data);
        //                            TempData["CurrRowVersion"] = CurCorp.RowVersion;
        //                            db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
        //                            if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
        //                            {
        //                                OthServiceBookActivity othServiceBookAct = new OthServiceBookActivity()
        //                        {
        //                            Name = c.Name == null ? "" : c.Name.Trim(),
        //                            OtherSerBookActList = c.OtherSerBookActList,
        //                            OthServiceBookPolicy = c.OthServiceBookPolicy,
        //                            FullDetails = c.FullDetails,
        //                            DBTrack = c.DBTrack
        //                        };
        //                                db.OthServiceBookActivity.Attach(othServiceBookAct);
        //                                db.Entry(othServiceBookAct).State = System.Data.Entity.EntityState.Modified;
        //                                db.Entry(othServiceBookAct).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //                            }
        //                            using (var context = new DataBaseContext())
        //                            {


        //                                var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
        //                                DT_OthServiceBookActivity DT_Corp = (DT_OthServiceBookActivity)obj;
        //                                // DT_Corp.OtherSerBookActList_Id = blog.OtherSerBookActList == null ? 0 : blog.OtherSerBookActList.Id;
        //                                // DT_Corp.OthServiceBookPolicy_Id = blog.OthServiceBookPolicy == null ? 0 : blog.OthServiceBookPolicy.Id;
        //                                db.Create(DT_Corp);
        //                                db.SaveChanges();
        //                            }
        //                            //   await db.SaveChangesAsync();
        //                            db.SaveChanges();
        //                            ts.Complete();
        //                            Msg.Add("  Record Updated");
        //                            return Json(new Utility.JsonReturnClass { Id = c.Id, Val = c.Name, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                            //return Json(new Object[] { c.Id, c.Name, "Record Updated", JsonRequestBehavior.AllowGet });

        //                        }
        //                    }
        //                    catch (DbUpdateConcurrencyException ex)
        //                    {
        //                        var entry = ex.Entries.Single();
        //                        var clientValues = (Corporate)entry.Entity;
        //                        var databaseEntry = entry.GetDatabaseValues();
        //                        if (databaseEntry == null)
        //                        {
        //                            Msg.Add(" Unable to save changes. The record was deleted by another user.");
        //                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                            // return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
        //                        }
        //                        else
        //                        {
        //                            var databaseValues = (Corporate)databaseEntry.ToObject();
        //                            c.RowVersion = databaseValues.RowVersion;

        //                        }
        //                    }
        //                    Msg.Add("Record modified by another user.So refresh it and try to save again.");
        //                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                    // return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
        //                }
        //            }

        //            return View();
        //        }
        //        catch (Exception ex)
        //        {
        //            LogFile Logfile = new LogFile();
        //            ErrorLog Err = new ErrorLog()
        //            {
        //                ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
        //                ExceptionMessage = ex.Message,
        //                ExceptionStackTrace = ex.StackTrace,
        //                LineNo = Convert.ToString(new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
        //                LogTime = DateTime.Now
        //            };
        //            Logfile.CreateLogFile(Err);
        //            Msg.Add(ex.Message);
        //            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //}

        [HttpPost]
        public async Task<ActionResult> EditSave(OthServiceBookActivity c, int data, FormCollection form) // Edit submit
        {
            List<String> Msg = new List<String>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var PayscaleagreementdetailsCreatelist = form["PayscaleagreementdetailsCreatelist"] == "0" ? "" : form["PayscaleagreementdetailsCreatelist"];
                    var Category = form["Category"] == "0" ? "" : form["Category"];
                    string OthServiceBookPolicyList = form["OthServiceBookPolicyList"] == "0" ? "" : form["OthServiceBookPolicyList"];
                   

                    bool Auth = form["Autho_Allow"] == "true" ? true : false;
                    var db_data = db.OthServiceBookActivity.Include(e => e.OthServiceBookPolicy).Include(e => e.OtherSerBookActList)
                        .Where(e => e.Id == data).SingleOrDefault();

                    List<ContactDetails> ObjBudsection = new List<ContactDetails>();

                 //   var other1 = db.OthServiceBookPolicy.Find(int.Parse(OthServiceBookPolicyList));
                  //  c.OthServiceBookPolicy = other1;
                    if (OthServiceBookPolicyList != null && OthServiceBookPolicyList != "")
                    {
                        c.OthServiceBookPolicy_Id = int.Parse(OthServiceBookPolicyList);

                    }
                    if (Category != null && Category != "")
                    {
                        c.OtherSerBookActList_Id = int.Parse(Category);
                    }

                    if (OthServiceBookPolicyList != null && OthServiceBookPolicyList != "")
                    {
                        var value = db.OthServiceBookPolicy.Find(int.Parse(OthServiceBookPolicyList));
                        db_data.OthServiceBookPolicy = value;

                    }
                    else
                    {
                        db_data.OthServiceBookPolicy = null;
                    }

                    //db.OthServiceBookActivity.Attach(db_data);
                    //db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                    //db.SaveChanges();
                    //TempData["RowVersion"] = db_data.RowVersion;
                    //db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;
                    //if (OthServiceBookPolicyList != null && OthServiceBookPolicyList != "")
                    //{
                    //    int OthServiceBookPolicyid = Convert.ToInt32(OthServiceBookPolicyList);
                    //    var value = db.OthServiceBookPolicy.Where(e => e.Id == OthServiceBookPolicyid).SingleOrDefault();
                    //    c.OthServiceBookPolicy = value;

                    //}
                    //if (OthServiceBookPolicyList != "")
                    //{
                    //    var ids = db_data.OthServiceBookPolicy(OthServiceBookPolicyList);
                    //    foreach (var ba in ids)
                    //    {

                    //          var value = db.ContactDetails.Find(ba);
                    //        ObjBudsection.Add(value);
                    //        db_data.ContactDetails = ObjBudsection;
                    //    }
                    //}
                    //else
                    //{
                    //    db_data.ContactDetails = null;
                    //}

                    //if (Category != "")
                    //{
                    //    var val = db.LookupValue.Find(int.Parse(Category));
                    //    db_data.OtherSerBookActList = val;
                    //}

                    //db.OthServiceBookActivity.Attach(db_data);
                    //db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                    //db.SaveChanges();
                    //TempData["RowVersion"] = db_data.RowVersion;
                    //db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;

                    if (Auth == false)
                    {
                        if (ModelState.IsValid)
                        {
                            //DbContextTransaction transaction = db.Database.BeginTransaction();

                            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {
                                OthServiceBookActivity blog = null; // to retrieve old data
                                DbPropertyValues originalBlogValues = null;

                                using (var context = new DataBaseContext())
                                {
                                    blog = context.OthServiceBookActivity.Where(e => e.Id == data).Include(e => e.OtherSerBookActList).Include(e => e.OthServiceBookPolicy).SingleOrDefault();
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
                                //  int a = EditS(data, c, c.DBTrack);
                                var m1 = db.OthServiceBookActivity.Where(e => e.Id == data).ToList();
                                foreach (var s in m1)
                                {
                                    // s.AppraisalPeriodCalendar = null;
                                    db.OthServiceBookActivity.Attach(s);
                                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                    //await db.SaveChangesAsync();
                                    db.SaveChanges();
                                    TempData["RowVersion"] = s.RowVersion;
                                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                }
                                var CurOBJ = db.OthServiceBookActivity.Find(data);
                                TempData["CurrRowVersion"] = CurOBJ.RowVersion;
                                db.Entry(CurOBJ).State = System.Data.Entity.EntityState.Detached;
                                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                {
                                    OthServiceBookActivity othServiceBookAct = new OthServiceBookActivity()
                                        {
                                            Name = c.Name == null ? "" : c.Name.Trim(),
                                            OtherSerBookActList_Id = c.OtherSerBookActList_Id,
                                            OthServiceBookPolicy_Id = c.OthServiceBookPolicy_Id,
                                            FullDetails = c.FullDetails,
                                            Id = data,
                                            DBTrack = c.DBTrack
                                        };
                                    db.OthServiceBookActivity.Attach(othServiceBookAct);
                                  //  db.SaveChanges();
                                    db.Entry(othServiceBookAct).State = System.Data.Entity.EntityState.Modified;
                                    db.Entry(othServiceBookAct).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                    //// DBTrackFile.DBTrackSave("Payroll", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                }


                                using (var context = new DataBaseContext())
                                //{
                                //    //To save data in history table 
                                //    var Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                //    //var Obj = DBTrackFile.ModifiedDataHistory("Training/Training", "M", blog, c, "TrainingInstitute", c.DBTrack);
                                //    DT_OthServiceBookActivity DT_Cat = (DT_OthServiceBookActivity)Obj;
                                //    db.DT_OthServiceBookActivity.Add(DT_Cat);
                                  db.SaveChanges();
                                //}
                               
                                ts.Complete();
                                Msg.Add("  Record Updated");
                                return Json(new Utility.JsonReturnClass { Id = c.Id, Val = c.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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

                            OthServiceBookActivity blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            OthServiceBookActivity Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.OthServiceBookActivity.Where(e => e.Id == data).SingleOrDefault();
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
                            OthServiceBookActivity othServiceBookAct = new OthServiceBookActivity()
                            {
                                Name = c.Name == null ? "" : c.Name.Trim(),
                                OtherSerBookActList = db_data.OtherSerBookActList,
                                OthServiceBookPolicy = c.OthServiceBookPolicy,
                                FullDetails = c.FullDetails,
                                DBTrack = c.DBTrack
                            };
                            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Training/Training", "M", blog, othServiceBookAct, "Corporate", c.DBTrack);
                                // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                Old_Corp = context.OthServiceBookActivity.Where(e => e.Id == data).Include(e => e.OtherSerBookActList)
                                    .Include(e => e.OthServiceBookPolicy).SingleOrDefault();
                                //DT_Corporate DT_Corp = (DT_Corporate)obj;
                                //DT_Corp.Address_Id = DBTrackFile.ValCompare(Old_Corp.Address, c.Address);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
                                //DT_Corp.BusinessType_Id = DBTrackFile.ValCompare(Old_Corp.BusinessType, c.BusinessType); //Old_Corp.BusinessType == c.BusinessType ? 0 : Old_Corp.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_Corp.BusinessType.Id;
                                //DT_Corp.ContactDetails_Id = DBTrackFile.ValCompare(Old_Corp.ContactDetails, c.ContactDetails); //Old_Corp.ContactDetails == c.ContactDetails ? 0 : Old_Corp.ContactDetails == null && c.ContactDetails != null ? c.ContactDetails.Id : Old_Corp.ContactDetails.Id;
                                //db.Create(DT_Corp);
                                //db.SaveChanges();
                            }
                            blog.DBTrack = c.DBTrack;
                            db.OthServiceBookActivity.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("Record Updated Successfully.");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }

                    }
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    var entry = ex.Entries.Single();
                    var clientValues = (OthServiceBookActivity)entry.Entity;
                    var databaseEntry = entry.GetDatabaseValues();
                    if (databaseEntry == null)
                    {
                        Msg.Add(ex.Message);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //  return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                    }
                    else
                    {
                        var databaseValues = (OthServiceBookActivity)databaseEntry.ToObject();
                        c.RowVersion = databaseValues.RowVersion;

                    }
                }
                catch (Exception e)
                {
                    Msg.Add(e.Message);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
                return View();
            }
        }



        public int EditS(string Corp, string Addrs, int data, OthServiceBookActivity c, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (Corp != null)
                {
                    if (Corp != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(Corp));
                        c.OtherSerBookActList = val;

                        var type = db.OthServiceBookActivity.Include(e => e.OtherSerBookActList).Where(e => e.Id == data).SingleOrDefault();
                        IList<OthServiceBookActivity> typedetails = null;
                        if (type.OtherSerBookActList != null)
                        {
                            typedetails = db.OthServiceBookActivity.Where(x => x.OtherSerBookActList.Id == type.OtherSerBookActList.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            typedetails = db.OthServiceBookActivity.Where(x => x.Id == data).ToList();
                        }
                        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                        foreach (var s in typedetails)
                        {
                            s.OtherSerBookActList = c.OtherSerBookActList;
                            db.OthServiceBookActivity.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                }

                if (Addrs != null)
                {
                    if (Addrs != "")
                    {
                        var val = db.OthServiceBookPolicy.Find(int.Parse(Addrs));
                        c.OthServiceBookPolicy = val;

                        var add = db.OthServiceBookActivity.Include(e => e.OthServiceBookPolicy).Where(e => e.Id == data).SingleOrDefault();
                        IList<OthServiceBookActivity> addressdetails = null;
                        if (add.OthServiceBookPolicy != null)
                        {
                            addressdetails = db.OthServiceBookActivity.Where(x => x.OthServiceBookPolicy.Id == add.OthServiceBookPolicy.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            addressdetails = db.OthServiceBookActivity.Where(x => x.Id == data).ToList();
                        }
                        if (addressdetails != null)
                        {
                            foreach (var s in addressdetails)
                            {
                                s.OthServiceBookPolicy = c.OthServiceBookPolicy;
                                db.OthServiceBookActivity.Attach(s);
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
                    var addressdetails = db.OthServiceBookActivity.Include(e => e.OthServiceBookPolicy).Where(x => x.Id == data).ToList();
                    foreach (var s in addressdetails)
                    {
                        s.OthServiceBookPolicy = null;
                        db.OthServiceBookActivity.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }



                var CurCorp = db.OthServiceBookActivity.Find(data);
                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    c.DBTrack = dbT;
                    OthServiceBookActivity corp = new OthServiceBookActivity()
            {
                Name = c.Name,
                Id = data,
                DBTrack = c.DBTrack
            };


                    db.OthServiceBookActivity.Attach(corp);
                    db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                    return 1;
                }
                return 0;
            }
        }
        // return View();
        public ActionResult GetOthServiceBookPolicyLKDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.OthServiceBookPolicy.ToList();
                IEnumerable<OthServiceBookPolicy> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.OthServiceBookPolicy.ToList().Where(d => d.Name.Contains(data));

                }
                else
                {
                    var list1 = db.OthServiceBookActivity.ToList().Select(e => e.OthServiceBookPolicy);
                    var list2 = fall.Except(list1);

                    //  var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.LookupVal }).Distinct();
                    var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.Name }).Distinct();
                    //var result_1 = (from c in fall
                    //                select new { c.Id, c.CorporateCode, c.CorporateName });
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.Name }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            // return View();
        }

        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    OthServiceBookActivity incrActivity = db.OthServiceBookActivity.Include(e => e.OthServiceBookPolicy)
                                              .Include(e => e.OtherSerBookActList).Where(e => e.Id == data).SingleOrDefault();

                    OthServiceBookPolicy add = incrActivity.OthServiceBookPolicy;
                    LookupValue val = incrActivity.OtherSerBookActList;
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
                        var selectedRegions = incrActivity.OthServiceBookPolicy;

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
                                    incrActivity.OthServiceBookPolicy = add;
                                    incrActivity.OtherSerBookActList = val;
                                    ////DBTrackFile.DBTrackSave("Core/P2b.Global", "D", incrActivity, null, "        OthServiceBookActivity", dbT);
                                }
                                ts.Complete();
                                Msg.Add("  Data removed successfully.  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                // return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });

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
    }
}