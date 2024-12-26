using System;
using System.Collections.Generic;
using System.Web;
using System.Linq;
using System.Web.Mvc;
using P2BUltimate.App_Start;
using System.Net;
using System.Data.Entity;
using P2BUltimate.Models;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Transactions;
using P2b.Global;
using System.Text;
using System.Threading.Tasks;
using P2BUltimate.Security;
using Payroll;
using Leave;


namespace P2BUltimate.Controllers.Payroll.MainController
{
    [AuthoriseManger]
    public class GratuityActController : Controller
    {
        //  private DataBaseContext db = new DataBaseContext();

        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/GratuityAct/Index.cshtml");
        }

        public ActionResult Partial()
        {
            return View("~/Views/Shared/Core/_Location.cshtml");

        }


        public ActionResult GetLookupDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                {
                    var fall = db.LvHead.ToList();
                    IEnumerable<LvHead> all;
                    if (!string.IsNullOrEmpty(data))
                    {
                        all = db.LvHead.ToList().Where(d => d.FullDetails.ToString().Contains(data));

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
        }


        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult Create(GratuityAct _GratuityAct, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string GratuityWageslist = form["GratuityWageslist"] == "0" ? "" : form["GratuityWageslist"];
                    string LvHeadlist = form["LvHeadlist"] == "0" ? "" : form["LvHeadlist"];

                    //company_Id = 0;
                    int company_Id = Convert.ToInt32(Session["CompId"]);
                    var Companypayroll = db.CompanyPayroll.Where(e => e.Company.Id == company_Id).SingleOrDefault();


                    if (GratuityWageslist != null && GratuityWageslist != "")
                    {
                        var val = db.Wages.Find(int.Parse(GratuityWageslist));
                        _GratuityAct.GratuityWages = val;

                    }
                    if (LvHeadlist != null)
                    {
                        if (LvHeadlist != "")
                        {
                            var ids = Utility.StringIdsToListIds(LvHeadlist);
                            List<LvHead> LvHead_list = new List<LvHead>();
                            foreach (var ca in ids)
                            {
                                var id = Convert.ToInt32(ca);
                                var data = db.LvHead.Find(id);
                                LvHead_list.Add(data);
                            }
                            _GratuityAct.LvHead = LvHead_list;
                        }
                    }
                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            _GratuityAct.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            GratuityAct GratuityAct = new GratuityAct()
                            {
                                EffectiveDate = _GratuityAct.EffectiveDate,
                                //EndDate = _GratuityAct.EndDate,
                                GratuityActName = _GratuityAct.GratuityActName,
                                GratuityWages = _GratuityAct.GratuityWages,
                                IsDateOfConfirm = _GratuityAct.IsDateOfConfirm,
                                IsDateOfJoin = _GratuityAct.IsDateOfJoin,
                                IsLVInclude = _GratuityAct.IsLVInclude,
                                IsLWPInclude = _GratuityAct.IsLWPInclude,
                                ITExemptionAmount = _GratuityAct.ITExemptionAmount,
                                LvHead = _GratuityAct.LvHead,
                                MaxGratuityAmount = _GratuityAct.MaxGratuityAmount,
                                PayableDays = _GratuityAct.PayableDays,
                                ServiceFrom = _GratuityAct.ServiceFrom,
                                ServiceTo = _GratuityAct.ServiceTo,
                                DBTrack = _GratuityAct.DBTrack
                            };
                            try
                            {

                                db.GratuityAct.Add(GratuityAct);
                                //var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, _GratuityAct.DBTrack);
                                //DT_Gratuity DT_OBJ = (DT_Gratuity)rtn_Obj;
                                //DT_OBJ.GratuityWages = _GratuityAct.GratuityWages == null ? null : _GratuityAct.GratuityWages;
                                //db.Create(DT_OBJ);
                                db.SaveChanges();
                                if (Companypayroll != null)
                                {
                                    var payscale_list = new List<GratuityAct>();
                                    payscale_list.Add(GratuityAct);
                                    Companypayroll.GratuityAct = payscale_list;
                                    db.CompanyPayroll.Attach(Companypayroll);
                                    db.Entry(Companypayroll).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    db.Entry(Companypayroll).State = System.Data.Entity.EntityState.Detached;

                                }

                                ts.Complete();
                                Msg.Add("  Data Created successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = GratuityAct.Id, Val = GratuityAct.GratuityActName, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                // return this.Json(new Object[] { GratuityAct.Id, GratuityAct.GratuityActName, "Data Created Successfully.", JsonRequestBehavior.AllowGet });
                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = _GratuityAct.Id });
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

        public ActionResult GetContactDetLKDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.ContactDetails.ToList();
                IEnumerable<ContactDetails> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.ContactDetails.ToList().Where(d => d.FullContactDetails.Contains(data));

                }
                else
                {
                    var list1 = db.Corporate.ToList().Select(e => e.ContactDetails);
                    //var list2 = fall.Except(list1);
                    //  var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.LookupVal }).Distinct();
                    var r = (from ca in list1 select new { srno = ca.Id, lookupvalue = ca.FullContactDetails }).Distinct();
                    //var result_1 = (from c in fall
                    //                select new { c.Id, c.CorporateCode, c.CorporateName });
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.FullContactDetails }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            // return View();
        }


        public ActionResult GetLocationDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.Location.ToList();
                IEnumerable<Location> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.Location.Include(e => e.LocationObj).ToList().Where(d => d.FullDetails.Contains(data));

                }
                else
                {
                    //var list1 = db.GratuityAct.ToList().Select(e => e.PayScaleArea);
                    //var list1 = db.GratuityAct.ToList();
                    //var list2 = fall.Except(list1);
                    //  var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.LookupVal }).Distinct();
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                    //var result_1 = (from c in fall
                    //                select new { c.Id, c.CorporateCode, c.CorporateName });
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.FullDetails }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);


            }
            // return View();
        }





        //public async Task<ActionResult> AuthSave(int auth_id, Boolean isauth, String auth_action) //to save authorised data
        //{
        //    //if (auth_action == "C")
        //    //{
        //    //    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //    //    {
        //    //        GratuityAct GratuityAct = db.GratuityAct.Include(e => e.PayScaleArea.Select(r => r.LocationObj)).Include(e => e.PayScaleType)
        //    //          .FirstOrDefault(e => e.Id == auth_id);
        //    //        GratuityAct.DBTrack = new DBTrack
        //    //        {
        //    //            Action = "C",
        //    //            ModifiedBy = GratuityAct.DBTrack.ModifiedBy != null ? GratuityAct.DBTrack.ModifiedBy : null,
        //    //            CreatedBy = GratuityAct.DBTrack.CreatedBy != null ? GratuityAct.DBTrack.CreatedBy : null,
        //    //            CreatedOn = GratuityAct.DBTrack.CreatedOn != null ? GratuityAct.DBTrack.CreatedOn : null,
        //    //            IsModified = GratuityAct.DBTrack.IsModified == true ? false : false,
        //    //            AuthorizedBy = SessionManager.UserName,
        //    //            AuthorizedOn = DateTime.Now
        //    //        };
        //    //        db.GratuityAct.Attach(GratuityAct);
        //    //        db.Entry(GratuityAct).State = System.Data.Entity.EntityState.Modified;
        //    //        db.Entry(GratuityAct).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //    //        var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, GratuityAct.DBTrack);
        //    //        DT_PayScale DT_OBJ = (DT_PayScale)rtn_Obj;
        //    //        DT_OBJ.PayScaleType_Id = GratuityAct.PayScaleType == null ? 0 : GratuityAct.PayScaleType.Id;

        //    //        db.Create(DT_OBJ);
        //    //        await db.SaveChangesAsync();

        //    //        ts.Complete();
        //    //        return Json(new Object[] { GratuityAct.Id, GratuityAct.FullDetails, "Record Authorised", JsonRequestBehavior.AllowGet });
        //    //    }
        //    //}
        //    //else if (auth_action == "M")
        //    //{

        //    //    GratuityAct Old_OBJ = db.GratuityAct.Include(e => e.PayScaleType)
        //    //                                      .Include(e => e.PayScaleArea.Select(r => r.LocationObj))
        //    //                                      .Where(e => e.Id == auth_id).SingleOrDefault();

        //    //    // DT_PayScale Curr_OBJ = db.DT_PayScale.Include(e => e.PayScaleArea)  
        //    //    DT_PayScale Curr_OBJ = db.DT_PayScale
        //    //                          .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
        //    //                          .OrderByDescending(e => e.Id)
        //    //                          .FirstOrDefault();

        //    //    GratuityAct GratuityAct = new GratuityAct();

        //    //    string Arealst = Curr_OBJ.PayScaleArea_Id == null ? null : Curr_OBJ.PayScaleArea_Id.ToString();
        //    //    string Typelst = Curr_OBJ.PayScaleType_Id == null ? null : Curr_OBJ.PayScaleType_Id.ToString();


        //    //    if (ModelState.IsValid)
        //    //    {
        //    //        try
        //    //        {

        //    //            //DbContextTransaction transaction = db.Database.BeginTransaction();

        //    //            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //    //            {
        //    //                // db.Configuration.AutoDetectChangesEnabled = false;
        //    //                GratuityAct.DBTrack = new DBTrack
        //    //                {
        //    //                    CreatedBy = Old_OBJ.DBTrack.CreatedBy == null ? null : Old_OBJ.DBTrack.CreatedBy,
        //    //                    CreatedOn = Old_OBJ.DBTrack.CreatedOn == null ? null : Old_OBJ.DBTrack.CreatedOn,
        //    //                    Action = "M",
        //    //                    ModifiedBy = Old_OBJ.DBTrack.ModifiedBy == null ? null : Old_OBJ.DBTrack.ModifiedBy,
        //    //                    ModifiedOn = Old_OBJ.DBTrack.ModifiedOn == null ? null : Old_OBJ.DBTrack.ModifiedOn,
        //    //                    AuthorizedBy = SessionManager.UserName,
        //    //                    AuthorizedOn = DateTime.Now,
        //    //                    IsModified = false
        //    //                };

        //    //                int a = EditS(Arealst, Typelst, "", auth_id, GratuityAct, GratuityAct.DBTrack);

        //    //                await db.SaveChangesAsync();

        //    //                ts.Complete();
        //    //                return Json(new Object[] { GratuityAct.Id, GratuityAct.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });
        //    //            }
        //    //        }
        //    //        catch (DbUpdateConcurrencyException ex)
        //    //        {
        //    //            var entry = ex.Entries.Single();
        //    //            var clientValues = (GratuityAct)entry.Entity;
        //    //            var databaseEntry = entry.GetDatabaseValues();
        //    //            if (databaseEntry == null)
        //    //            {
        //    //                return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
        //    //            }
        //    //            else
        //    //            {
        //    //                var databaseValues = (GratuityAct)databaseEntry.ToObject();
        //    //                GratuityAct.RowVersion = databaseValues.RowVersion;
        //    //            }
        //    //        }

        //    //        return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
        //    //    }
        //    //}
        //    //else if (auth_action == "D")
        //    //{
        //    //    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //    //    {
        //    //        //GratuityAct corp = db.GratuityAct.Find(auth_id);
        //    //        GratuityAct GratuityAct = db.GratuityAct.AsNoTracking().Include(e => e.PayScaleArea.Select(r => r.LocationObj))
        //    //                                                    .FirstOrDefault(e => e.Id == auth_id);
        //    //        var selectedValues = GratuityAct.PayScaleArea;
        //    //        GratuityAct.DBTrack = new DBTrack
        //    //        {
        //    //            Action = "D",
        //    //            ModifiedBy = GratuityAct.DBTrack.ModifiedBy != null ? GratuityAct.DBTrack.ModifiedBy : null,
        //    //            CreatedBy = GratuityAct.DBTrack.CreatedBy != null ? GratuityAct.DBTrack.CreatedBy : null,
        //    //            CreatedOn = GratuityAct.DBTrack.CreatedOn != null ? GratuityAct.DBTrack.CreatedOn : null,
        //    //            IsModified = GratuityAct.DBTrack.IsModified == true ? false : false,
        //    //            AuthorizedBy = SessionManager.UserName,
        //    //            AuthorizedOn = DateTime.Now
        //    //        };
        //    //        db.GratuityAct.Attach(GratuityAct);
        //    //        db.Entry(GratuityAct).State = System.Data.Entity.EntityState.Deleted;
        //    //        var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, GratuityAct.DBTrack);
        //    //        DT_PayScale DT_OBJ = (DT_PayScale)rtn_Obj;
        //    //        DT_OBJ.PayScaleType_Id = GratuityAct.PayScaleType == null ? 0 : GratuityAct.PayScaleType.Id;

        //    //        db.Create(DT_OBJ);
        //    //        await db.SaveChangesAsync();

        //    //        db.Entry(GratuityAct).State = System.Data.Entity.EntityState.Detached;
        //    //        ts.Complete();
        //    //        return Json(new Object[] { "", "", "Record Authorised", JsonRequestBehavior.AllowGet });
        //    //    }

        //    }
        ////    return View();

        //}



        public ActionResult PopulateDropDownList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var qurey = db.Location.ToList();
                var selected = (Object)null;
                if (data2 != "" && data != "0" && data2 != "0")
                {
                    selected = Convert.ToInt32(data2);
                }

                SelectList s = new SelectList(qurey, "Id", "Name", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }



        [HttpPost]
        public async Task<ActionResult> EditSave(GratuityAct _GratuityAct, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;
                    string GratuityWageslist = form["GratuityWageslist"] == "0" ? "" : form["GratuityWageslist"];
                    string LvHeadlist = form["LvHeadlist"] == "0" ? "" : form["LvHeadlist"];

                    //company_Id = 0;
                    int company_Id = Convert.ToInt32(Session["CompId"]);
                    var Companypayroll = db.CompanyPayroll.Where(e => e.Company.Id == company_Id).SingleOrDefault();

                    _GratuityAct.GratuityWages_Id = GratuityWageslist != null && GratuityWageslist != "" ? int.Parse(GratuityWageslist) : 0;


                    if (Auth == false)
                    {


                        if (ModelState.IsValid)
                        {
                            try
                            {

                                //DbContextTransaction transaction = db.Database.BeginTransaction();

                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    GratuityAct blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    //using (var context = new DataBaseContext())
                                    //{
                                        blog = db.GratuityAct.Where(e => e.Id == data).Include(e => e.GratuityWages)
                                                                .Include(e => e.LvHead).SingleOrDefault();
                                        originalBlogValues = db.Entry(blog).OriginalValues;
                                    //}

                                    _GratuityAct.DBTrack = new DBTrack
                                    {
                                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    }; 

                                    List<LvHead> LvHead = new List<LvHead>();

                                    var lvheaddetails = db.GratuityAct.Include(e => e.LvHead).Where(e => e.Id == data).SingleOrDefault();
                                    if (LvHeadlist != null && LvHeadlist != "")
                                    {
                                        var ids = Utility.StringIdsToListIds(LvHeadlist);
                                        foreach (var ca in ids)
                                        {
                                            var value = db.LvHead.Find(ca);
                                            LvHead.Add(value);
                                            lvheaddetails.LvHead = LvHead;
                                        }
                                    }
                                    else
                                    {
                                        lvheaddetails.LvHead = null;
                                    }


                                    db.GratuityAct.Attach(lvheaddetails);
                                    db.Entry(lvheaddetails).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    TempData["RowVersion"] = lvheaddetails.RowVersion; 

                                    var gradutityact = db.GratuityAct.Find(data);
                                    TempData["CurrRowVersion"] = gradutityact.RowVersion;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {
                                        gradutityact.EffectiveDate = _GratuityAct.EffectiveDate;
                                        gradutityact.GratuityActName = _GratuityAct.GratuityActName;
                                        gradutityact.IsDateOfConfirm = _GratuityAct.IsDateOfConfirm;
                                        gradutityact.IsDateOfJoin = _GratuityAct.IsDateOfJoin;
                                        gradutityact.IsLVInclude = _GratuityAct.IsLVInclude;
                                        gradutityact.IsLWPInclude = _GratuityAct.IsLWPInclude;
                                        gradutityact.ITExemptionAmount = _GratuityAct.ITExemptionAmount;
                                        //LvHead = _GratuityAct.LvHead,
                                        gradutityact.MaxGratuityAmount = _GratuityAct.MaxGratuityAmount;
                                        gradutityact.MonthDays = _GratuityAct.MonthDays;
                                        gradutityact.PayableDays = _GratuityAct.PayableDays;
                                        gradutityact.ServiceFrom = _GratuityAct.ServiceFrom;
                                        gradutityact.ServiceTo = _GratuityAct.ServiceTo;
                                        gradutityact.Id = data;
                                        gradutityact.DBTrack = _GratuityAct.DBTrack;
                                        gradutityact.GratuityWages_Id = _GratuityAct.GratuityWages_Id;

                                        db.Entry(gradutityact).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                    }
                                     
                                    ts.Complete();

                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = _GratuityAct.Id, Val = _GratuityAct.GratuityActName, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
 

                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (GratuityAct)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    // return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (GratuityAct)databaseEntry.ToObject();
                                    _GratuityAct.RowVersion = databaseValues.RowVersion;

                                }
                            }

                            Msg.Add("Record modified by another user.So refresh it and try to save again.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            // return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            GratuityAct blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            GratuityAct Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.GratuityAct.Where(e => e.Id == data).SingleOrDefault();
                                originalBlogValues = context.Entry(blog).OriginalValues;
                            }
                            _GratuityAct.DBTrack = new DBTrack
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

                            GratuityAct GratuityAct = new GratuityAct()
                            {
                                EffectiveDate = _GratuityAct.EffectiveDate,
                                EndDate = _GratuityAct.EndDate,
                                GratuityActName = _GratuityAct.GratuityActName,
                                GratuityWages = _GratuityAct.GratuityWages,
                                IsDateOfConfirm = _GratuityAct.IsDateOfConfirm,
                                IsDateOfJoin = _GratuityAct.IsDateOfJoin,
                                IsLVInclude = _GratuityAct.IsLVInclude,
                                IsLWPInclude = _GratuityAct.IsLWPInclude,
                                ITExemptionAmount = _GratuityAct.ITExemptionAmount,
                                LvHead = _GratuityAct.LvHead,
                                MaxGratuityAmount = _GratuityAct.MaxGratuityAmount,
                                PayableDays = _GratuityAct.PayableDays,
                                ServiceFrom = _GratuityAct.ServiceFrom,
                                ServiceTo = _GratuityAct.ServiceTo,
                                DBTrack = _GratuityAct.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };
                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, GratuityAct, "GratuityAct", _GratuityAct.DBTrack);
                                // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                Old_Corp = context.GratuityAct.Where(e => e.Id == data).Include(e => e.GratuityWages).SingleOrDefault();
                                DT_Gratuity DT_Corp = (DT_Gratuity)obj;

                                //  DT_Corp.GratuityWages = DBTrackFile.ValCompare(Old_Corp.GratuityWages==_GratuityAct.GratuityWages?0:Old_Corp.GratuityWages==null&&); //Old_Corp.BusinessType == c.BusinessType ? 0 : Old_Corp.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_Corp.BusinessType.Id;

                                db.Create(DT_Corp);
                                //db.SaveChanges();
                            }
                            blog.DBTrack = _GratuityAct.DBTrack;
                            db.GratuityAct.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = _GratuityAct.GratuityActName, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            //return Json(new Object[] { blog.Id, _GratuityAct.GratuityActName, "Record Updated", JsonRequestBehavior.AllowGet });
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


        public class returndataclass
        {
            public string GratuityWages_id { get; set; }
            public string GratuityWages_val { get; set; }
            public Array LvHead_id { get; set; }
            public Array LvHead_val { get; set; }
        }

        [HttpPost]
        public ActionResult Edit(int data)
        {
            List<returndataclass> return_data = new List<returndataclass>();
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.GratuityAct
                  .Include(e => e.GratuityWages)
                  .Include(e => e.LvHead)
                  .Where(e => e.Id == data).AsEnumerable()
                  .Select(e => new
                  {
                      GratuityActName = e.GratuityActName,
                      IsDateOfConfirm = e.IsDateOfConfirm,
                      IsDateOfJoin = e.IsDateOfJoin,
                      IsLVInclude = e.IsLVInclude,
                      IsLWPInclude = e.IsLWPInclude,
                      ITExemptionAmount = e.ITExemptionAmount,
                      //LvHead = e.LvHead,
                      MaxGratuityAmount = e.MaxGratuityAmount,
                      PayableDays = e.PayableDays,
                      MonthDays = e.MonthDays,
                      ServiceFrom = e.ServiceFrom,
                      ServiceTo = e.ServiceTo,
                      EffectiveDate = e.EffectiveDate != null ? e.EffectiveDate.Value.ToShortDateString() : null,
                      Action = e.DBTrack.Action
                  }).ToList();
                TempData["RowVersion"] = db.GratuityAct.Find(data).RowVersion;

                var a = db.GratuityAct.Include(e => e.LvHead).Include(e => e.GratuityWages).Where(e => e.Id == data).ToList();

                foreach (var ca in a)
                {

                    return_data.Add(
                new returndataclass
                {
                    GratuityWages_id = ca.GratuityWages != null ? ca.GratuityWages.Id.ToString() : null,
                    GratuityWages_val = ca.GratuityWages != null ? ca.GratuityWages.FullDetails.ToString() : null,
                    LvHead_id = ca.LvHead.Select(e => e.Id.ToString()).ToArray(),
                    LvHead_val = ca.LvHead.Select(e => e.FullDetails).ToArray(),

                });
                }
                //var Old_Data = db.DT_PayScale
                //    //.Include(e => e.PayScaleArea)
                //    // .Include(e => e.PayScaleType) 
                // .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M")
                // .Select
                // (e => new
                // {
                //     DT_Id = e.Id,
                //     BasicScaleAppl = e.BasicScaleAppl,
                //     PayScaleADetails_Val = e.PayScaleArea_Id == 0 ? "" : db.Location.Where(x => x.Id == e.PayScaleArea_Id).Select(x => x.FullDetails).FirstOrDefault(),
                //     ////BasicScaleDetails_Val = e.BasicScaleDetails.Id == null ? "" : e.BasicScaleDetails.FullDetails,  
                //     PayScaleType_Val = e.PayScaleType_Id == 0 ? "" : db.LookupValue
                //                   .Where(x => x.Id == e.PayScaleType_Id)
                //                   .Select(x => x.LookupVal).FirstOrDefault(),
                // }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var Corp = db.GratuityAct.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;


                return Json(new Object[] { Q, return_data, "", Auth, JsonRequestBehavior.AllowGet });
            }
            //return this.Json(new Object[] { Q, BCDETAILS, JsonRequestBehavior.AllowGet });
        }






        [HttpPost]
        public ActionResult Delete(int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    GratuityAct GratuityAct = db.GratuityAct.Include(e => e.GratuityWages).Include(e => e.LvHead)
                                                       .Where(e => e.Id == data).SingleOrDefault();
                    var id = int.Parse(Session["CompId"].ToString());
                    var companypayroll = db.CompanyPayroll.Where(e => e.Company.Id == id).SingleOrDefault();
                    companypayroll.GratuityAct.Where(e => e.Id == GratuityAct.Id);
                    companypayroll.GratuityAct = null;
                    db.Entry(companypayroll).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    Msg.Add("  Data removed successfully.  ");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //  return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
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
            public string GratuityActName { get; set; }
            public double ITExemptionAmount { get; set; }
            public double MaxGratuityAmount { get; set; }


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


                    IEnumerable<P2BGridData> GratutiyList = null;
                    List<P2BGridData> model = new List<P2BGridData>();
                    P2BGridData view = null;


                    //var BindEmpList = db.EmployeePayroll.Include(e => e.Employee.EmpName).Include(e => e.SalaryT)
                    //    .Where(e => e.SalaryT.Any(u => u.PayMonth != "01/2017")).ToList();

                    var BindCompList = db.CompanyPayroll.Include(e => e.GratuityAct).ToList();

                    foreach (var z in BindCompList)
                    {
                        if (z.GratuityAct != null)
                        {

                            foreach (var G in z.GratuityAct)
                            {
                                //var aa = db.Calendar.Where(e => e.Id == Sal.Id).SingleOrDefault();
                                view = new P2BGridData()
                                {
                                    Id = G.Id,
                                    GratuityActName = G.GratuityActName,
                                    ITExemptionAmount = G.ITExemptionAmount,
                                    MaxGratuityAmount = G.MaxGratuityAmount
                                };
                                model.Add(view);

                            }
                        }

                    }

                    GratutiyList = model;

                    IEnumerable<P2BGridData> IE;
                    if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                    {
                        IE = GratutiyList;
                        if (gp.searchOper.Equals("eq"))
                        {
                            jsonData = IE.Where(e => (e.GratuityActName.ToUpper().Contains(gp.searchString.ToUpper()))
                                || (e.ITExemptionAmount.ToString().Contains(gp.searchString))
                                || (e.MaxGratuityAmount.ToString().Contains(gp.searchString))
                                || (e.Id.ToString().Contains(gp.searchString))
                                ).Select(a => new Object[] { a.GratuityActName, a.ITExemptionAmount, a.MaxGratuityAmount, a.Id, }).ToList();

                            //if (gp.searchField == "Id")
                            //    jsonData = IE.Select(a => new {  a.GratuityActName, a.ITExemptionAmount, a.MaxGratuityAmount ,a.Id,}).Where((e => (e.Id.ToString().Contains(gp.searchString)))).ToList();
                            //if (gp.searchField == "GratuityActName")
                            //    jsonData = IE.Select(a => new { a.GratuityActName, a.ITExemptionAmount, a.MaxGratuityAmount, a.Id,}).Where((e => (e.GratuityActName.ToString().Contains(gp.searchString)))).ToList();
                            //if (gp.searchField == "ITExemptionAmount")
                            //    jsonData = IE.Select(a => new {  a.GratuityActName, a.ITExemptionAmount, a.MaxGratuityAmount ,a.Id, }).Where((e => (e.ITExemptionAmount.ToString().Contains(gp.searchString)))).ToList();
                            //if (gp.searchField == "MaxGratuityAmount")
                            //    jsonData = IE.Select(a => new {  a.GratuityActName, a.ITExemptionAmount, a.MaxGratuityAmount ,a.Id, }).Where((e => (e.MaxGratuityAmount.ToString().Contains(gp.searchString)))).ToList();




                            //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] {  Convert.ToString(a.GratuityActName), Convert.ToString(a.ITExemptionAmount), Convert.ToString(a.MaxGratuityAmount), a.Id,}).ToList();
                        }
                        totalRecords = IE.Count();
                    }
                    else
                    {
                        IE = GratutiyList;
                        Func<P2BGridData, dynamic> orderfuc;
                        if (gp.sidx == "Id")
                        {
                            orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                        }
                        else
                        {
                            orderfuc = (c => gp.sidx == "GratuityActName" ? c.GratuityActName.ToString() :
                                             gp.sidx == "ITExemptionAmount" ? c.ITExemptionAmount.ToString() :
                                             gp.sidx == "MaxGratuityAmount" ? c.MaxGratuityAmount.ToString() : ""

                                            );
                        }
                        if (gp.sord == "asc")
                        {
                            IE = IE.OrderBy(orderfuc);
                            jsonData = IE.Select(a => new Object[] { Convert.ToString(a.GratuityActName), Convert.ToString(a.ITExemptionAmount), Convert.ToString(a.MaxGratuityAmount) ,a.Id, }).ToList();
                        }
                        else if (gp.sord == "desc")
                        {
                            IE = IE.OrderByDescending(orderfuc);
                            jsonData = IE.Select(a => new Object[] {  Convert.ToString(a.GratuityActName), Convert.ToString(a.ITExemptionAmount), Convert.ToString(a.MaxGratuityAmount), a.Id, }).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.GratuityActName), Convert.ToString(a.ITExemptionAmount), Convert.ToString(a.MaxGratuityAmount), a.Id,}).ToList();
                        }
                        totalRecords = GratutiyList.Count();
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
    }
}
