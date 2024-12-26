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
    public class TransActivityController : Controller
    {
        //
        // GET: /TransActivity/
        public ActionResult Index()
        {
            return View("~/Views/Core/MainViews/TransActivity/Index.cshtml");
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
        public ActionResult Create(TransActivity c, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {


                    string TransList_drop = form["TransList_drop"] == "0" ? "" : form["TransList_drop"];
                    string TransPolicyList = form["TransPolicyList"] == "0" ? "" : form["TransPolicyList"];
                    var PayscaleagreementdetailsCreatelist = form["PayscaleagreementdetailsCreatelist"] == "0" ? "" : form["PayscaleagreementdetailsCreatelist"];
                    var PsAgreement = new PayScaleAgreement();
                    if (PayscaleagreementdetailsCreatelist != null && PayscaleagreementdetailsCreatelist != "")
                    {
                        PsAgreement = db.PayScaleAgreement.Find(int.Parse(PayscaleagreementdetailsCreatelist));
                    }
                    if (TransList_drop != null && TransList_drop != "")
                    {
                        var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "446").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(TransList_drop)).FirstOrDefault(); //db.LookupValue.Find(int.Parse(TransList_drop));
                        c.TransList = val;
                    }


                    if (TransPolicyList != null && TransPolicyList != "")
                    {
                        var val = db.TransPolicy.Find(int.Parse(TransPolicyList));
                        c.TranPolicy = val;
                    }

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            if (db.TransActivity.Any(o => o.Name == c.Name))
                            {
                                Msg.Add("  Name Already Exists.  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //     return Json(new Object[] { "", "", "Name  Already Exists.", JsonRequestBehavior.AllowGet });
                            }

                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            TransActivity _transActivity = new TransActivity()
                            {
                                Name = c.Name == null ? "" : c.Name.Trim(),
                                TransList = c.TransList,
                                TranPolicy = c.TranPolicy,
                                DBTrack = c.DBTrack
                            };
                            try
                            {
                                db.TransActivity.Add(_transActivity);
                                //var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, c.DBTrack);
                                //DT_TransActivity DT_TransActivity = (DT_TransActivity)rtn_Obj;
                                //db.Create(DT_TransActivity);
                                db.SaveChanges();


                                List<TransActivity> TransActivitylist = new List<TransActivity>();
                                TransActivitylist.Add(_transActivity);
                                if (PsAgreement != null)
                                {
                                    PsAgreement.TransActivity = TransActivitylist;
                                    db.PayScaleAgreement.Attach(PsAgreement);
                                    db.Entry(PsAgreement).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    db.Entry(PsAgreement).State = System.Data.Entity.EntityState.Detached;

                                }
                                ts.Complete();
                                // return this.Json(new Object[] { _transActivity.Id, _transActivity.FullDetails, "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = _transActivity.Id, Val = _transActivity.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = c.Id });
                            }
                            catch (DataException /* dex */)
                            {
                                //   return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
                                Msg.Add("  Unable to create...  ");
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
                var Q = db.TransActivity
                  .Include(e => e.TranPolicy)
                  .Include(e => e.TransList)
                  .Where(e => e.Id == data).Select
                  (e => new
                  {
                      Name = e.Name,
                      TransList_id = e.TransList.Id == null ? 0 : e.TransList.Id,
                      TranPolicy_FullDetails = e.TranPolicy.FullDetails == null ? "" : e.TranPolicy.FullDetails,
                      TranPolicy_Id = e.TranPolicy.Id == null ? "" : e.TranPolicy.Id.ToString(),
                      Action = e.DBTrack.Action
                  }).ToList();

                var add_data = db.TransActivity
                  .Include(e => e.TranPolicy)
                   .Where(e => e.Id == data)
                    .Select(e => new
                    {
                        TranPolicy_FullDetails = e.TranPolicy.FullDetails == null ? "" : e.TranPolicy.FullDetails,
                        TranPolicy_Id = e.TranPolicy.Id == null ? "" : e.TranPolicy.Id.ToString(),
                    }).ToList();

                var PSA = db.PayScaleAgreement.Include(e => e.TransActivity).Where(e => e.TransActivity.Any(t => t.Id == data)).ToList();
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

                //var W = db.DT_TransActivity
                //     //.Include(e => e.IncrPolicy)
                //     //.Include(e => e.IncrList)
                //     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                //     (e => new
                //     {
                //         DT_Id = e.Id,
                //         Name = e.Name == null ? "" : e.Name,
                //         TransList_val = e.TransList_Id == null ? "" : e.TransList_Id.ToString(),
                //         TransPolicy_val = e.TranPolicy_Id == null ? "" : e.TranPolicy_Id.ToString(),
                //     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var Corp = db.TransActivity.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, ObjPayscaleAgg, "", Auth, JsonRequestBehavior.AllowGet });


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
        public async Task<ActionResult> EditSave(TransActivity c, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string TransActivity = form["TransList_drop"] == "0" ? "" : form["TransList_drop"];
                    string TransPolicyList = form["TransPolicyList"] == "0" ? "" : form["TransPolicyList"];
                    if (TransPolicyList != null)
                    {
                        if (TransPolicyList != "")
                        {
                            int ContId = Convert.ToInt32(TransPolicyList);
                            var val = db.TransPolicy.Where(e => e.Id == ContId).SingleOrDefault();
                            c.TranPolicy = val;
                            c.TranPolicy_Id = Convert.ToInt32(TransPolicyList);
                            var type = db.TransActivity.Include(e => e.TranPolicy).Where(e => e.Id == data).SingleOrDefault();
                            IList<TransActivity> typedetails = null;
                            if (type.TranPolicy != null)
                            {
                                typedetails = db.TransActivity.Where(x => x.TranPolicy.Id == type.TranPolicy.Id && x.Id == data).ToList();
                            }
                            else
                            {
                                typedetails = db.TransActivity.Where(x => x.Id == data).ToList();
                            }
                            foreach (var s in typedetails)
                            {
                                s.TranPolicy = c.TranPolicy;
                                db.TransActivity.Attach(s);
                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                TempData["RowVersion"] = s.RowVersion;
                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                            }
                        }
                    }
                    else
                    {
                        var BusiTypeDetails = db.TransActivity.Include(e => e.TranPolicy).Where(x => x.Id == data).ToList();
                        foreach (var s in BusiTypeDetails)
                        {
                            s.TranPolicy = null;
                            db.TransActivity.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }

                    bool Auth = form["Autho_Allow"] == "true" ? true : false;
                    if (TransActivity != null && TransActivity != "")
                    {
                        var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "446").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(TransActivity)).FirstOrDefault();  //db.LookupValue.Find(int.Parse(TransActivity));
                        c.TransList = val;
                        c.TransList_Id = int.Parse(TransActivity);
                        var type = db.TransActivity.Include(e => e.TransList).Where(e => e.Id == data).SingleOrDefault();
                        IList<TransActivity> typedetails = null;
                        if (type.TransList != null)
                        {
                            typedetails = db.TransActivity.Where(x => x.TransList.Id == type.TransList.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            typedetails = db.TransActivity.Where(x => x.Id == data).ToList();
                        }
                        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                        foreach (var s in typedetails)
                        {
                            s.TransList = c.TransList;
                            db.TransActivity.Attach(s);
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
                                    TransActivity blog = null; // to retrieve old data
                                    // DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.TransActivity.Where(e => e.Id == data).Include(e => e.TransList)
                                                                .Include(e => e.TranPolicy)
                                                                .AsNoTracking().SingleOrDefault();
                                        // originalBlogValues = context.Entry(blog).OriginalValues;
                                    }

                                    c.DBTrack = new DBTrack
                                    {
                                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };
                                    var m1 = db.TransActivity.Where(e => e.Id == data).ToList();
                                    foreach (var s in m1)
                                    {
                                        // s.AppraisalPeriodCalendar = null;
                                        db.TransActivity.Attach(s);
                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                        //await db.SaveChangesAsync();
                                        db.SaveChanges();
                                        TempData["RowVersion"] = s.RowVersion;
                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    }
                                    //int a = EditS(TransActivity, TransPolicyList, data, c, c.DBTrack);
                                    var CurCorp = db.TransActivity.Find(data);
                                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {
                                        //c.DBTrack = dbT;
                                        TransActivity corp = new TransActivity()
                                        {
                                            Name = c.Name,
                                            Id = data,
                                            TransList_Id = c.TransList_Id,
                                            TranPolicy_Id=c.TranPolicy_Id,
                                            DBTrack = c.DBTrack
                                        };


                                        db.TransActivity.Attach(corp);
                                        db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                                        db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                        //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                        // return 1;
                                    }
                                    await db.SaveChangesAsync();

                                    //using (var context = new DataBaseContext())
                                    //{

                                    //    //var Obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, c, "TransActivity", c.DBTrack);
                                    //    //DT_TransActivity DT_Corp = (DT_TransActivity)Obj;
                                    //    //db.DT_TransActivity.Add(DT_Corp);

                                    //}
                                    db.SaveChanges();
                                    ts.Complete();


                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = data, Val = c.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    // return Json(new Object[] { data, c.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });

                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (TransActivity)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    //  return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (TransActivity)databaseEntry.ToObject();
                                    c.RowVersion = databaseValues.RowVersion;

                                }
                            }
                            Msg.Add("Record modified by another user.So refresh it and try to save again.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            //  return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            TransActivity Old_Corp = db.TransActivity.Include(e => e.TransList)
                                                                .Include(e => e.TranPolicy)
                                                                .Where(e => e.Id == data).SingleOrDefault();

                            TransActivity Curr_Corp = c;
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
                            // return Json(new Object[] { Old_Corp.Id, c.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = Old_Corp.Id, Val = c.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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

                    IEnumerable<P2BGridData> TransActivity = null;
                    List<P2BGridData> model = new List<P2BGridData>();
                    P2BGridData view = null;
                    var Payscaleagreement = (String)null;
                    // Payscaleagreement = form["Payscaleagreementdetailslist"] == "0" ? "" : form["Payscaleagreementdetailslist"];
                    if (gp.filter != null)
                    {
                        Payscaleagreement = gp.filter;
                    }
                    int Id = Convert.ToInt32(Payscaleagreement);

                    var BindPayscaleagreementList = db.PayScaleAgreement.Include(e => e.TransActivity).Where(e => e.Id == Id).ToList();

                    foreach (var z in BindPayscaleagreementList)
                    {
                        if (z.TransActivity != null)
                        {

                            foreach (var trans in z.TransActivity)
                            {
                                //var aa = db.Calendar.Where(e => e.Id == Sal.Id).SingleOrDefault();
                                view = new P2BGridData()
                                {
                                    Id = trans.Id,

                                    Name = trans.Name,
                                    FullDetails = trans.FullDetails,
                                };
                                model.Add(view);

                            }
                        }

                    }

                    TransActivity = model;

                    IEnumerable<P2BGridData> IE;
                    if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                    {
                        IE = TransActivity;
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
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.Name), a.Id  }).ToList();
                        }
                        totalRecords = IE.Count();
                    }
                    else
                    {
                        IE = TransActivity;
                        Func<P2BGridData, dynamic> orderfuc;
                        if (gp.sidx == "Id")
                        {
                            orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                        }
                        else
                        {
                            orderfuc = (c => gp.sidx == "Name" ? c.Name.ToString() : "" );
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
                        totalRecords = TransActivity.Count();
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

        //        IEnumerable<TransActivity> TransActivity = null;
        //        if (gp.IsAutho == true)
        //        {
        //            TransActivity = db.TransActivity.Include(e => e.TransList).Where(e => e.DBTrack.IsModified == true).ToList();
        //        }
        //        else
        //        {
        //            TransActivity = db.TransActivity.Include(e => e.TransList).ToList();
        //        }
        //        IEnumerable<TransActivity> IE;

        //        if (!string.IsNullOrEmpty(gp.searchField))
        //        {
        //            IE = TransActivity;
        //            if (gp.searchOper.Equals("eq"))
        //            {
        //                jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Name), Convert.ToString(a.TransList.LookupVal) }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Name, a.TransList != null ? Convert.ToString(a.TransList.LookupVal) : "" }).ToList();
        //            }
        //            totalRecords = IE.Count();
        //        }
        //        else
        //        {
        //            IE = TransActivity;
        //            Func<TransActivity, string> orderfuc = (c =>
        //                                                       gp.sidx == "ID" ? c.Id.ToString() :
        //                                                       gp.sidx == "Name" ? c.Name :
        //                                                       gp.sidx == "TransList" ? c.TransList.LookupVal :
        //                                                         "");
        //            if (gp.sord == "asc")
        //            {
        //                IE = IE.OrderBy(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Name), a.TransList != null ? Convert.ToString(a.TransList.LookupVal) : "" }).ToList();
        //            }
        //            else if (gp.sord == "desc")
        //            {
        //                IE = IE.OrderByDescending(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Name), a.TransList != null ? Convert.ToString(a.TransList.LookupVal) : ""}).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Name, a.TransList != null ? Convert.ToString(a.TransList.LookupVal) : "" }).ToList();
        //            }
        //            totalRecords = TransActivity.Count();
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
        public async Task<ActionResult> AuthSave(int auth_id, Boolean isauth, String auth_action) //to save authorised data
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    if (auth_action == "C")
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            TransActivity corp = db.TransActivity.Include(e => e.TransList)
                                       .Include(e => e.TranPolicy).FirstOrDefault(e => e.Id == auth_id);

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

                            db.TransActivity.Attach(corp);
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
                            return Json(new Utility.JsonReturnClass { Id = corp.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            //return Json(new Object[] { corp.Id, corp.Name, "Record Authorised", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else if (auth_action == "M")
                    {

                        TransActivity Old_Corp = db.TransActivity.Include(e => e.TransList)
                                                          .Include(e => e.TranPolicy)
                                                          .Where(e => e.Id == auth_id).SingleOrDefault();


                        //DT_TransActivity Curr_Corp = db.DT_TransActivity.Include(e => e.TransList_Id)
                        //                            .Include(e => e.TranPolicy_Id)
                        //                            .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                        //                            .OrderByDescending(e => e.Id)
                        //                            .FirstOrDefault();

                        TransActivity corp = new TransActivity();

                        //string Corp = Curr_Corp.TransList_Id == null ? null : Curr_Corp.TransList_Id.ToString();
                        //string Addrs = Curr_Corp.TranPolicy_Id == null ? null : Curr_Corp.TranPolicy_Id.ToString();

                        //corp.Name = Curr_Corp.Name == null ? Old_Corp.Name : Curr_Corp.Name;
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
                                    var Corp = "";
                                    var Addrs = "";
                                    int a = EditS(Corp, Addrs, auth_id, corp, corp.DBTrack);


                                    await db.SaveChangesAsync();

                                    ts.Complete();
                                    // return Json(new Object[] { corp.Id, corp.Name, "Record Updated", JsonRequestBehavior.AllowGet });


                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = corp.Id, Val = corp.Name, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (TransActivity)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    // return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                    Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }
                                else
                                {
                                    var databaseValues = (TransActivity)databaseEntry.ToObject();
                                    corp.RowVersion = databaseValues.RowVersion;
                                }
                            }
                            Msg.Add("Record modified by another user.So refresh it and try to save again.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            //  return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else if (auth_action == "D")
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            TransActivity corp = db.TransActivity.AsNoTracking().Include(e => e.TransList)
                                                                        .Include(e => e.TranPolicy)
                                                                       .FirstOrDefault(e => e.Id == auth_id);


                            LookupValue val = corp.TransList;

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

                            db.TransActivity.Attach(corp);
                            db.Entry(corp).State = System.Data.Entity.EntityState.Deleted;
                            await db.SaveChangesAsync();
                            using (var context = new DataBaseContext())
                            {

                                corp.TransList = val;
                                ////DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corp, null, "IncrActivity", corp.DBTrack);
                            }


                            db.Entry(corp).State = System.Data.Entity.EntityState.Detached;
                            ts.Complete();
                            //  return Json(new Object[] { "", "", "Record Authorised", JsonRequestBehavior.AllowGet });
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
        }

        public int EditS(string TransList, string TransPolicyList, int data, TransActivity c, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (TransList != null && TransList != "")
                {
                    var val = db.LookupValue.Find(int.Parse(TransList));
                    c.TransList = val;

                    var type = db.TransActivity.Include(e => e.TransList).Where(e => e.Id == data).SingleOrDefault();
                    IList<TransActivity> typedetails = null;
                    if (type.TransList != null)
                    {
                        typedetails = db.TransActivity.Where(x => x.TransList.Id == type.TransList.Id && x.Id == data).ToList();
                    }
                    else
                    {
                        typedetails = db.TransActivity.Where(x => x.Id == data).ToList();
                    }
                    //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                    foreach (var s in typedetails)
                    {
                        s.TransList = c.TransList;
                        db.TransActivity.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }


                if (TransPolicyList != null && TransPolicyList != "")
                {
                    var val = db.TransPolicy.Find(int.Parse(TransPolicyList));
                    c.TranPolicy = val;

                    var add = db.TransActivity.Include(e => e.TransList).Where(e => e.Id == data).SingleOrDefault();
                    IList<TransActivity> TransPolicydetails = null;
                    if (add.TranPolicy != null)
                    {
                        TransPolicydetails = db.TransActivity.Where(x => x.TransList.Id == add.TransList.Id && x.Id == data).ToList();
                    }
                    else
                    {
                        TransPolicydetails = db.TransActivity.Where(x => x.Id == data).ToList();
                    }
                    if (TransPolicydetails != null)
                    {
                        foreach (var s in TransPolicydetails)
                        {
                            s.TranPolicy = c.TranPolicy;
                            db.TransActivity.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            // await db.SaveChangesAsync(false);
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                }

                else
                {
                    var TransPolicydetails = db.TransActivity.Include(e => e.TranPolicy).Where(x => x.Id == data).ToList();
                    foreach (var s in TransPolicydetails)
                    {
                        s.TranPolicy = null;
                        db.TransActivity.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }



                var CurCorp = db.TransActivity.Find(data);
                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    c.DBTrack = dbT;
                    TransActivity corp = new TransActivity()
                    {
                        Name = c.Name,
                        Id = data,
                        DBTrack = c.DBTrack
                    };


                    db.TransActivity.Attach(corp);
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
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    TransActivity TransActivity = db.TransActivity.Include(e => e.TranPolicy)
                                                      .Include(e => e.TransList).Where(e => e.Id == data).SingleOrDefault();


                    LookupValue val = TransActivity.TransList;
                    //Corporate corporates = db.Corporate.Where(e => e.Id == data).SingleOrDefault();
                    if (TransActivity.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack, corporates, null, "Corporate");
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = TransActivity.DBTrack.CreatedBy != null ? TransActivity.DBTrack.CreatedBy : null,
                                CreatedOn = TransActivity.DBTrack.CreatedOn != null ? TransActivity.DBTrack.CreatedOn : null,
                                IsModified = TransActivity.DBTrack.IsModified == true ? true : false
                            };
                            TransActivity.DBTrack = dbT;
                            db.Entry(TransActivity).State = System.Data.Entity.EntityState.Modified;
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack);
                            await db.SaveChangesAsync();
                            using (var context = new DataBaseContext())
                            {
                                ////DBTrackFile.DBTrackSave("Core/P2b.Global", "D", incrActivity, null, "IncrActivity", incrActivity.DBTrack);
                            }
                            ts.Complete();
                            //   return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }
                    }
                    else
                    {
                        var selectedRegions = TransActivity.TransList;

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
                                    CreatedBy = TransActivity.DBTrack.CreatedBy != null ? TransActivity.DBTrack.CreatedBy : null,
                                    CreatedOn = TransActivity.DBTrack.CreatedOn != null ? TransActivity.DBTrack.CreatedOn : null,
                                    IsModified = TransActivity.DBTrack.IsModified == true ? false : false,
                                    AuthorizedBy = SessionManager.UserName,
                                    AuthorizedOn = DateTime.Now
                                };

                                // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };

                                db.Entry(TransActivity).State = System.Data.Entity.EntityState.Deleted;
                                await db.SaveChangesAsync();


                                using (var context = new DataBaseContext())
                                {

                                    TransActivity.TransList = val;
                                    ////DBTrackFile.DBTrackSave("Core/P2b.Global", "D", incrActivity, null, "IncrActivity", dbT);
                                }
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
                                //   return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });

                                Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

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
