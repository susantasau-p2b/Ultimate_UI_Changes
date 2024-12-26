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
    public class ExtnRednActivityController : Controller
    {
        //
        // GET: /TransActivity/
        public ActionResult Index()
        {
            return View("~/Views/Core/MainViews/ExtnRednActivity/Index.cshtml");
        }

        //private DataBaseContext db = new DataBaseContext();




        public ActionResult PopulateDropDownList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var qurey = db.PayScaleAgreement.Include(e => e.ExtnRednActivity).ToList();
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
        public ActionResult Create(ExtnRednActivity c, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {


                    string ExtnRednList_drop = form["ExtnRednList_drop"] == "0" ? "" : form["ExtnRednList_drop"];
                    string ExtnRednPolicyList = form["ExtnRednPolicyList"] == "0" ? "" : form["ExtnRednPolicyList"];
                    var PayscaleagreementdetailsCreatelist = form["PayscaleagreementdetailsCreatelist"] == "0" ? "" : form["PayscaleagreementdetailsCreatelist"];
                    var PsAgreement = new PayScaleAgreement();
                    if (PayscaleagreementdetailsCreatelist != null && PayscaleagreementdetailsCreatelist != "")
                    {
                        PsAgreement = db.PayScaleAgreement.Find(int.Parse(PayscaleagreementdetailsCreatelist));
                    }
                    if (ExtnRednList_drop != null && ExtnRednList_drop != "")
                    {
                        var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "101").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(ExtnRednList_drop)).FirstOrDefault();  //db.LookupValue.Find(int.Parse(ExtnRednList_drop));
                        c.ExtnRednList = val;
                    }


                    if (ExtnRednPolicyList != null && ExtnRednPolicyList != "")
                    {
                        var val = db.ExtnRednPolicy.Find(int.Parse(ExtnRednPolicyList));
                        c.ExtnRednPolicy = val;
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

                            ExtnRednActivity _ExtnRednActivity = new ExtnRednActivity()
                            {
                                Name = c.Name == null ? "" : c.Name.Trim(),
                                ExtnRednList = c.ExtnRednList,
                                ExtnRednPolicy = c.ExtnRednPolicy,
                                DBTrack = c.DBTrack
                            };
                            try
                            {
                                db.ExtnRednActivity.Add(_ExtnRednActivity);
                                //var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, c.DBTrack);
                                //DT_TransActivity DT_TransActivity = (DT_TransActivity)rtn_Obj;
                                //db.Create(DT_TransActivity);
                                db.SaveChanges();


                                List<ExtnRednActivity> ExtnRednActivitylist = new List<ExtnRednActivity>();
                                ExtnRednActivitylist.Add(_ExtnRednActivity);
                                if (PsAgreement != null)
                                {
                                    PsAgreement.ExtnRednActivity = ExtnRednActivitylist;
                                    db.PayScaleAgreement.Attach(PsAgreement);
                                    db.Entry(PsAgreement).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    db.Entry(PsAgreement).State = System.Data.Entity.EntityState.Detached;

                                }
                                ts.Complete();
                                // return this.Json(new Object[] { _transActivity.Id, _transActivity.FullDetails, "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = _ExtnRednActivity.Id, Val = _ExtnRednActivity.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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
                var Q = db.ExtnRednActivity
                  .Include(e => e.ExtnRednPolicy)
                  .Include(e => e.ExtnRednList)
                  .Where(e => e.Id == data).Select
                  (e => new
                  {
                      Name = e.Name,
                      ExtnRednList_id = e.ExtnRednList.Id == null ? 0 : e.ExtnRednList.Id,
                      ExtnRednPolicy_FullDetails = e.ExtnRednPolicy.FullDetails == null ? "" : e.ExtnRednPolicy.FullDetails,
                      ExtnRednPolicy_Id = e.ExtnRednPolicy.Id == null ? "" : e.ExtnRednPolicy.Id.ToString(),
                      Action = e.DBTrack.Action
                  }).ToList();

                var add_data = db.ExtnRednActivity
                  .Include(e => e.ExtnRednPolicy)
                   .Where(e => e.Id == data)
                    .Select(e => new
                    {
                        ExtnRednPolicy_FullDetails = e.ExtnRednPolicy.FullDetails == null ? "" : e.ExtnRednPolicy.FullDetails,
                        ExtnRednPolicy_Id = e.ExtnRednPolicy.Id == null ? "" : e.ExtnRednPolicy.Id.ToString(),
                    }).ToList();

                var PSA = db.PayScaleAgreement.Include(e => e.ExtnRednActivity).Where(e => e.ExtnRednActivity.Any(t => t.Id == data)).ToList();
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

                var Corp = db.ExtnRednActivity.Find(data);
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
        public async Task<ActionResult> EditSave(ExtnRednActivity c, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string ExtnRednActivity = form["ExtnRednList_drop"] == "0" ? "" : form["ExtnRednList_drop"];
                    string ExtnRednPolicyList = form["ExtnRednPolicyList"] == "0" ? "" : form["ExtnRednPolicyList"];
                    if (ExtnRednPolicyList != null)
                    {
                        if (ExtnRednPolicyList != "")
                        {
                            int ContId = Convert.ToInt32(ExtnRednPolicyList);
                            var val = db.ExtnRednPolicy.Where(e => e.Id == ContId).SingleOrDefault();
                            c.ExtnRednPolicy = val;
                            c.ExtnRednPolicy_Id = Convert.ToInt32(ExtnRednPolicyList); 
                            var type = db.ExtnRednActivity.Include(e => e.ExtnRednPolicy).Where(e => e.Id == data).SingleOrDefault();
                            IList<ExtnRednActivity> typedetails = null;
                            if (type.ExtnRednPolicy != null)
                            {
                                typedetails = db.ExtnRednActivity.Where(x => x.ExtnRednPolicy.Id == type.ExtnRednPolicy.Id && x.Id == data).ToList();
                            }
                            else
                            {
                                typedetails = db.ExtnRednActivity.Where(x => x.Id == data).ToList();
                            }
                            foreach (var s in typedetails)
                            {
                                s.ExtnRednPolicy = c.ExtnRednPolicy;
                                db.ExtnRednActivity.Attach(s);
                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                TempData["RowVersion"] = s.RowVersion;
                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                            }
                        }
                    }
                    else
                    {
                        var BusiTypeDetails = db.ExtnRednActivity.Include(e => e.ExtnRednPolicy).Where(x => x.Id == data).ToList();
                        foreach (var s in BusiTypeDetails)
                        {
                            s.ExtnRednPolicy = null;
                            db.ExtnRednActivity.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }

                    bool Auth = form["Autho_Allow"] == "true" ? true : false;
                    if (ExtnRednActivity != null && ExtnRednActivity != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(ExtnRednActivity));
                        c.ExtnRednList = val;
                        c.ExtnRednList_Id = int.Parse(ExtnRednActivity);
                        var type = db.ExtnRednActivity.Include(e => e.ExtnRednList).Where(e => e.Id == data).SingleOrDefault();
                        IList<ExtnRednActivity> typedetails = null;
                        if (type.ExtnRednList != null)
                        {
                            typedetails = db.ExtnRednActivity.Where(x => x.ExtnRednList.Id == type.ExtnRednList.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            typedetails = db.ExtnRednActivity.Where(x => x.Id == data).ToList();
                        }
                        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                        foreach (var s in typedetails)
                        {
                            s.ExtnRednList = c.ExtnRednList;
                            db.ExtnRednActivity.Attach(s);
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
                                    ExtnRednActivity blog = null; // to retrieve old data
                                    // DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.ExtnRednActivity.Where(e => e.Id == data).Include(e => e.ExtnRednList)
                                                                .Include(e => e.ExtnRednPolicy)
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
                                    var m1 = db.ExtnRednActivity.Where(e => e.Id == data).ToList();
                                    foreach (var s in m1)
                                    {
                                        // s.AppraisalPeriodCalendar = null;
                                        db.ExtnRednActivity.Attach(s);
                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                        //await db.SaveChangesAsync();
                                        db.SaveChanges();
                                        TempData["RowVersion"] = s.RowVersion;
                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    }
                                    //int a = EditS(TransActivity, TransPolicyList, data, c, c.DBTrack);
                                    var CurCorp = db.ExtnRednActivity.Find(data);
                                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {
                                        //c.DBTrack = dbT;
                                        ExtnRednActivity corp = new ExtnRednActivity()
                                        {
                                            Name = c.Name,
                                            Id = data,
                                            ExtnRednList_Id = c.ExtnRednList_Id,
                                            ExtnRednPolicy_Id=c.ExtnRednPolicy_Id,
                                            DBTrack = c.DBTrack
                                        };


                                        db.ExtnRednActivity.Attach(corp);
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
                            ExtnRednActivity Old_Corp = db.ExtnRednActivity.Include(e => e.ExtnRednList)
                                                                .Include(e => e.ExtnRednPolicy)
                                                                .Where(e => e.Id == data).SingleOrDefault();

                            ExtnRednActivity Curr_Corp = c;
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

                    IEnumerable<P2BGridData> ExtnRednActivity = null;
                    List<P2BGridData> model = new List<P2BGridData>();
                    P2BGridData view = null;
                    var Payscaleagreement = (String)null;
                    // Payscaleagreement = form["Payscaleagreementdetailslist"] == "0" ? "" : form["Payscaleagreementdetailslist"];
                    if (gp.filter != null)
                    {
                        Payscaleagreement = gp.filter;
                    }
                    int Id = Convert.ToInt32(Payscaleagreement);

                    var BindPayscaleagreementList = db.PayScaleAgreement.Include(e => e.ExtnRednActivity).Where(e => e.Id == Id).ToList();

                    foreach (var z in BindPayscaleagreementList)
                    {
                        if (z.ExtnRednActivity != null)
                        {

                            foreach (var trans in z.ExtnRednActivity)
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

                    ExtnRednActivity = model;

                    IEnumerable<P2BGridData> IE;
                    if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                    {
                        IE = ExtnRednActivity;
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
                        IE = ExtnRednActivity;
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
                        totalRecords = ExtnRednActivity.Count();
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

                            ExtnRednActivity corp = db.ExtnRednActivity.Include(e => e.ExtnRednList)
                                       .Include(e => e.ExtnRednPolicy).FirstOrDefault(e => e.Id == auth_id);

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

                            db.ExtnRednActivity.Attach(corp);
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

                        ExtnRednActivity Old_Corp = db.ExtnRednActivity.Include(e => e.ExtnRednList)
                                                          .Include(e => e.ExtnRednPolicy)
                                                          .Where(e => e.Id == auth_id).SingleOrDefault();


                        //DT_TransActivity Curr_Corp = db.DT_TransActivity.Include(e => e.TransList_Id)
                        //                            .Include(e => e.TranPolicy_Id)
                        //                            .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                        //                            .OrderByDescending(e => e.Id)
                        //                            .FirstOrDefault();

                        ExtnRednActivity corp = new ExtnRednActivity();

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

        public int EditS(string TransList, string TransPolicyList, int data, ExtnRednActivity c, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (TransList != null && TransList != "")
                {
                    var val = db.LookupValue.Find(int.Parse(TransList));
                    c.ExtnRednList = val;

                    var type = db.ExtnRednActivity.Include(e => e.ExtnRednList).Where(e => e.Id == data).SingleOrDefault();
                    IList<ExtnRednActivity> typedetails = null;
                    if (type.ExtnRednList != null)
                    {
                        typedetails = db.ExtnRednActivity.Where(x => x.ExtnRednList.Id == type.ExtnRednList.Id && x.Id == data).ToList();
                    }
                    else
                    {
                        typedetails = db.ExtnRednActivity.Where(x => x.Id == data).ToList();
                    }
                    //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                    foreach (var s in typedetails)
                    {
                        s.ExtnRednList = c.ExtnRednList;
                        db.ExtnRednActivity.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }


                if (TransPolicyList != null && TransPolicyList != "")
                {
                    var val = db.ExtnRednPolicy.Find(int.Parse(TransPolicyList));
                    c.ExtnRednPolicy = val;

                    var add = db.ExtnRednActivity.Include(e => e.ExtnRednList).Where(e => e.Id == data).SingleOrDefault();
                    IList<ExtnRednActivity> TransPolicydetails = null;
                    if (add.ExtnRednPolicy != null)
                    {
                        TransPolicydetails = db.ExtnRednActivity.Where(x => x.ExtnRednList.Id == add.ExtnRednList.Id && x.Id == data).ToList();
                    }
                    else
                    {
                        TransPolicydetails = db.ExtnRednActivity.Where(x => x.Id == data).ToList();
                    }
                    if (TransPolicydetails != null)
                    {
                        foreach (var s in TransPolicydetails)
                        {
                            s.ExtnRednPolicy = c.ExtnRednPolicy;
                            db.ExtnRednActivity.Attach(s);
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
                    var TransPolicydetails = db.ExtnRednActivity.Include(e => e.ExtnRednPolicy).Where(x => x.Id == data).ToList();
                    foreach (var s in TransPolicydetails)
                    {
                        s.ExtnRednPolicy = null;
                        db.ExtnRednActivity.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }



                var CurCorp = db.ExtnRednActivity.Find(data);
                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    c.DBTrack = dbT;
                    ExtnRednActivity corp = new ExtnRednActivity()
                    {
                        Name = c.Name,
                        Id = data,
                        DBTrack = c.DBTrack
                    };


                    db.ExtnRednActivity.Attach(corp);
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
                    ExtnRednActivity ExtnRednActivity = db.ExtnRednActivity.Include(e => e.ExtnRednPolicy)
                                                      .Include(e => e.ExtnRednList).Where(e => e.Id == data).SingleOrDefault();


                    LookupValue val = ExtnRednActivity.ExtnRednList;
                    //Corporate corporates = db.Corporate.Where(e => e.Id == data).SingleOrDefault();
                    if (ExtnRednActivity.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack, corporates, null, "Corporate");
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = ExtnRednActivity.DBTrack.CreatedBy != null ? ExtnRednActivity.DBTrack.CreatedBy : null,
                                CreatedOn = ExtnRednActivity.DBTrack.CreatedOn != null ? ExtnRednActivity.DBTrack.CreatedOn : null,
                                IsModified = ExtnRednActivity.DBTrack.IsModified == true ? true : false
                            };
                            ExtnRednActivity.DBTrack = dbT;
                            db.Entry(ExtnRednActivity).State = System.Data.Entity.EntityState.Modified;
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
                        var selectedRegions = ExtnRednActivity.ExtnRednList;

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
                                    CreatedBy = ExtnRednActivity.DBTrack.CreatedBy != null ? ExtnRednActivity.DBTrack.CreatedBy : null,
                                    CreatedOn = ExtnRednActivity.DBTrack.CreatedOn != null ? ExtnRednActivity.DBTrack.CreatedOn : null,
                                    IsModified = ExtnRednActivity.DBTrack.IsModified == true ? false : false,
                                    AuthorizedBy = SessionManager.UserName,
                                    AuthorizedOn = DateTime.Now
                                };

                                // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };

                                db.Entry(ExtnRednActivity).State = System.Data.Entity.EntityState.Deleted;
                                await db.SaveChangesAsync();


                                using (var context = new DataBaseContext())
                                {

                                    ExtnRednActivity.ExtnRednList = val;
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
