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
    public class IncrPolicyController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();
        public ActionResult Index()
        {
            return View("~/Views/Core/MainViews/IncrPolicy/Index.cshtml");
        }
        public ActionResult Partial()
        {
            return View("~/Views/Shared/Core/_IncrPolicy.cshtml");
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
                            //Corporate corp = db.Corporate.Find(auth_id);
                            //Corporate corp = db.Corporate.AsNoTracking().FirstOrDefault(e => e.Id == auth_id);

                            IncrPolicy corp = db.IncrPolicy
                                .Include(e => e.RegIncrPolicy)
                                .Include(e => e.NonRegIncrPolicy).FirstOrDefault(e => e.Id == auth_id);

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

                            db.IncrPolicy.Attach(corp);
                            db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            //db.SaveChanges();
                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corp.DBTrack);
                            DT_IncrPolicy DT_Corp = (DT_IncrPolicy)rtn_Obj;
                            DT_Corp.RegIncrPolicy_Id = corp.RegIncrPolicy == null ? 0 : corp.RegIncrPolicy.Id;
                            DT_Corp.NonRegIncrPolicy_Id = corp.NonRegIncrPolicy == null ? 0 : corp.NonRegIncrPolicy.Id;

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

                        IncrPolicy Old_Corp = db.IncrPolicy
                                                          .Include(e => e.RegIncrPolicy)
                                                          .Include(e => e.NonRegIncrPolicy).Where(e => e.Id == auth_id).SingleOrDefault();


                        DT_IncrPolicy Curr_Corp = db.DT_IncrPolicy
                                                    .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                                    .OrderByDescending(e => e.Id)
                                                    .FirstOrDefault();

                        if (Curr_Corp != null)
                        {
                            IncrPolicy corp = new IncrPolicy();

                            string Corp = Curr_Corp.RegIncrPolicy_Id == null ? null : Curr_Corp.RegIncrPolicy_Id.ToString();
                            string Addrs = Curr_Corp.NonRegIncrPolicy_Id == null ? null : Curr_Corp.NonRegIncrPolicy_Id.ToString();

                            corp.Name = Curr_Corp.Name == null ? Old_Corp.Name : Curr_Corp.Name;
                            corp.IsRegularIncr = Old_Corp.IsRegularIncr;

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
                                        Msg.Add("  Record Authorised");
                                        return Json(new Utility.JsonReturnClass { Id = corp.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        //return Json(new Object[] { corp.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                                    }
                                }
                                catch (DbUpdateConcurrencyException ex)
                                {
                                    var entry = ex.Entries.Single();
                                    var clientValues = (IncrPolicy)entry.Entity;
                                    var databaseEntry = entry.GetDatabaseValues();
                                    if (databaseEntry == null)
                                    {
                                        Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        // return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                    }
                                    else
                                    {
                                        var databaseValues = (IncrPolicy)databaseEntry.ToObject();
                                        corp.RowVersion = databaseValues.RowVersion;
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
                        // return Json(new Object[] { "", "Data removed from history", JsonRequestBehavior.AllowGet });
                    }
                    else if (auth_action == "D")
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //Corporate corp = db.Corporate.Find(auth_id);
                            IncrPolicy corp = db.IncrPolicy.AsNoTracking()
                                                                        .Include(e => e.RegIncrPolicy)
                                                                        .Include(e => e.NonRegIncrPolicy).FirstOrDefault(e => e.Id == auth_id);

                            RegIncrPolicy add = corp.RegIncrPolicy;

                            NonRegIncrPolicy val = corp.NonRegIncrPolicy;

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

                            db.IncrPolicy.Attach(corp);
                            db.Entry(corp).State = System.Data.Entity.EntityState.Deleted;


                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corp.DBTrack);
                            DT_IncrPolicy DT_Corp = (DT_IncrPolicy)rtn_Obj;
                            DT_Corp.RegIncrPolicy_Id = corp.RegIncrPolicy == null ? 0 : corp.RegIncrPolicy.Id;
                            DT_Corp.NonRegIncrPolicy_Id = corp.NonRegIncrPolicy == null ? 0 : corp.NonRegIncrPolicy.Id;

                            db.Create(DT_Corp);
                            await db.SaveChangesAsync();
                            db.Entry(corp).State = System.Data.Entity.EntityState.Detached;
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


        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult Create(IncrPolicy c, FormCollection form) //Create submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string IncrPromoPolicyDetails = form["IncrPolicyDetailslist"] == "0" ? "" : form["IncrPolicyDetailslist"];
                    string RegIncrPol = form["RegIncrPolicylist"] == "0" ? "" : form["RegIncrPolicylist"];
                    string NonRegIncrPol = form["NonRegIncrPolicylist"] == "0" ? "" : form["NonRegIncrPolicylist"];

                    if (RegIncrPol != null)
                    {
                        if (RegIncrPol != "")
                        {
                            int AddId = Convert.ToInt32(RegIncrPol);
                            var val = db.RegIncrPolicy.Where(e => e.Id == AddId).SingleOrDefault();
                            c.RegIncrPolicy = val;
                        }
                    }

                    if (NonRegIncrPol != null)
                    {
                        if (NonRegIncrPol != "")
                        {
                            int ContId = Convert.ToInt32(NonRegIncrPol);
                            var val = db.NonRegIncrPolicy.Where(e => e.Id == ContId).SingleOrDefault();
                            c.NonRegIncrPolicy = val;
                        }
                    }
                    if (IncrPromoPolicyDetails != null)
                    {
                        if (IncrPromoPolicyDetails != "")
                        {

                            var val = db.IncrPolicyDetails.Find(int.Parse(IncrPromoPolicyDetails));
                            c.IncrPolicyDetails = val;
                        }
                    }

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            if (db.IncrPolicy.Any(o => o.Name == c.Name))
                            {
                                Msg.Add("  Code Already Exists.  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Object[] { "", "", "Code Already Exists.", JsonRequestBehavior.AllowGet });
                            }

                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            IncrPolicy incrPolicy = new IncrPolicy()
                            {
                                Name = c.Name == null ? "" : c.Name.Trim(),
                                IsRegularIncr = c.IsRegularIncr,
                                RegIncrPolicy = c.RegIncrPolicy,
                                NonRegIncrPolicy = c.NonRegIncrPolicy,
                                FullDetails = c.FullDetails,
                                IncrPolicyDetails = c.IncrPolicyDetails,
                                DBTrack = c.DBTrack
                            };
                            try
                            {

                                db.IncrPolicy.Add(incrPolicy);
                                var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, c.DBTrack);
                                DT_IncrPolicy DT_Corp = (DT_IncrPolicy)rtn_Obj;
                                DT_Corp.RegIncrPolicy_Id = c.RegIncrPolicy == null ? 0 : c.RegIncrPolicy.Id;
                                DT_Corp.NonRegIncrPolicy_Id = c.NonRegIncrPolicy == null ? 0 : c.NonRegIncrPolicy.Id;
                                db.Create(DT_Corp);
                                db.SaveChanges();
                                //DBTrackFile.DBTrackSave("Core/P2b.Global", "C", incrPolicy, null, "IncrPolicy", null);


                                ts.Complete();
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = incrPolicy.Id, Val = incrPolicy.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // return this.Json(new Object[] { incrPolicy.Id, incrPolicy.FullDetails, "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
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
                IEnumerable<IncrPolicy> IncrPolicy = null;
                if (gp.IsAutho == true)
                {
                    IncrPolicy = db.IncrPolicy.Where(e => e.DBTrack.IsModified == true).ToList();
                }
                else
                {
                    IncrPolicy = db.IncrPolicy.ToList();
                }

                IEnumerable<IncrPolicy> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = IncrPolicy;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Select(a => new { a.Id, a.Name, a.IsRegularIncr }).Where((e => (e.Id.ToString() == gp.searchString) || (e.Name.ToLower() == gp.searchString.ToLower()) || (e.IsRegularIncr.ToString() == gp.searchString.ToLower()))).ToList();
                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Name, a.IsRegularIncr }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = IncrPolicy;
                    Func<IncrPolicy, int> orderfuc = (c =>
                                                               gp.sidx == "Id" ? c.Id : 0);
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Name), a.IsRegularIncr }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Name), a.IsRegularIncr }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Name, a.IsRegularIncr }).ToList();
                    }
                    totalRecords = IncrPolicy.Count();
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


        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.IncrPolicy
                    .Include(e => e.RegIncrPolicy)
                    .Include(e => e.NonRegIncrPolicy)
                    .Include(e => e.IncrPolicyDetails)
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        Name = e.Name,
                        IsRegularIncr = e.IsRegularIncr,
                        RegIncrFulldetails = e.RegIncrPolicy.FullDetails == null ? "" : e.RegIncrPolicy.FullDetails,
                        RegIncrPolicyId = e.RegIncrPolicy.Id == null ? 0 : e.RegIncrPolicy.Id,
                        NonRegIncrPolicyId = e.NonRegIncrPolicy.Id == null ? 0 : e.NonRegIncrPolicy.Id,
                        NonRegIncrFulldetails = e.NonRegIncrPolicy.FullDetails == null ? "" : e.NonRegIncrPolicy.FullDetails,
                        IncrPromoPolicyDetails_Id = e.IncrPolicyDetails.Id == null ? 0 : e.IncrPolicyDetails.Id,
                        IncrPromoPolicy_FullDetails = e.IncrPolicyDetails.FullDetails == null ? "" : e.IncrPolicyDetails.FullDetails,
                        Action = e.DBTrack.Action
                    }).ToList();

                //var add_data = db.IncrPolicy
                //   .Include(e => e.RegIncrPolicy)
                //    .Include(e => e.NonRegIncrPolicy)
                //    .Where(e => e.Id == data)
                //    .Select(e => new
                //    {
                //        RegIncrFulldetails = e.RegIncrPolicy.FullDetails == null ? "" : e.RegIncrPolicy.FullDetails,
                //        RegIncrPolicyId = e.RegIncrPolicy.Id == null ? "" : e.RegIncrPolicy.Id.ToString(),
                //        NonRegIncrPolicyId = e.NonRegIncrPolicy.Id == null ? "" : e.NonRegIncrPolicy.Id.ToString(),
                //        NonRegIncrFulldetails = e.NonRegIncrPolicy.FullDetails == null ? "" : e.NonRegIncrPolicy.FullDetails,
                //        IncrPromoPolicyDetails_Id = e.IncrPolicyDetails.Id == null ? "" : e.IncrPolicyDetails.Id.ToString(),
                //        IncrPromoPolicy_FullDetails = e.IncrPolicyDetails.FullDetails == null ? "" : e.IncrPolicyDetails.FullDetails,

                //    }).ToList();

                //TempData["RowVersion"] = db.IncrPolicy.Find(data).RowVersion;

                //return Json(new Object[] { Q, add_data, JsonRequestBehavior.AllowGet });


                var W = db.DT_IncrPolicy
                     .Include(e => e.RegIncrPolicy_Id)
                    .Include(e => e.NonRegIncrPolicy_Id)
                     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id,
                         Name = e.Name == null ? "" : e.Name,
                         //IncrAmount = e. == null ? 0 : e.IncrAmount,
                         //IncrPercent = e.IncrPercent,
                         //IncrSteps = e.IncrSteps,
                         //IsIncrPercent = e.IsIncrPercent,
                         //IsRegularIncr = e.IsRegularIncr,
                         //IsIncrSteps = e.IsIncrSteps,
                         //IsIncrAmount = e.IsIncrAmount,
                         RegIncrPolicy_Val = e.RegIncrPolicy_Id == null ? "" : e.RegIncrPolicy_Id.ToString(),
                         NonRegIncrPolicy_Val = e.NonRegIncrPolicy_Id == null ? "" : e.NonRegIncrPolicy_Id.ToString(),
                         IncrPolicyDetails_val = e.IncrPolicyDetails_Id == null ? "" : e.IncrPolicyDetails_Id.ToString(),
                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var Corp = db.IncrPolicy.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, W, Auth, JsonRequestBehavior.AllowGet });
            }
        }
        public int EditS(string Corp, string Addrs, int data, IncrPolicy c, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (Corp != null)
                {
                    if (Corp != "")
                    {
                        var val = db.RegIncrPolicy.Find(int.Parse(Corp));
                        c.RegIncrPolicy = val;

                        var type = db.IncrPolicy.Include(e => e.RegIncrPolicy).Where(e => e.Id == data).SingleOrDefault();
                        IList<IncrPolicy> typedetails = null;
                        if (type.RegIncrPolicy != null)
                        {
                            typedetails = db.IncrPolicy.Where(x => x.RegIncrPolicy.Id == type.RegIncrPolicy.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            typedetails = db.IncrPolicy.Where(x => x.Id == data).ToList();
                        }
                        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                        foreach (var s in typedetails)
                        {
                            s.RegIncrPolicy = c.RegIncrPolicy;
                            db.IncrPolicy.Attach(s);
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
                        var val = db.NonRegIncrPolicy.Find(int.Parse(Addrs));
                        c.NonRegIncrPolicy = val;

                        var add = db.IncrPolicy.Include(e => e.NonRegIncrPolicy).Where(e => e.Id == data).SingleOrDefault();
                        IList<IncrPolicy> addressdetails = null;
                        if (add.NonRegIncrPolicy != null)
                        {
                            addressdetails = db.IncrPolicy.Where(x => x.NonRegIncrPolicy.Id == add.NonRegIncrPolicy.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            addressdetails = db.IncrPolicy.Where(x => x.Id == data).ToList();
                        }
                        if (addressdetails != null)
                        {
                            foreach (var s in addressdetails)
                            {
                                s.NonRegIncrPolicy = c.NonRegIncrPolicy;
                                db.IncrPolicy.Attach(s);
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
                    var addressdetails = db.IncrPolicy.Include(e => e.NonRegIncrPolicy).Where(x => x.Id == data).ToList();
                    foreach (var s in addressdetails)
                    {
                        s.NonRegIncrPolicy = null;
                        db.IncrPolicy.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }


                var CurCorp = db.IncrPolicy.Find(data);
                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    c.DBTrack = dbT;
                    IncrPolicy corp = new IncrPolicy()
                    {
                        Name = c.Name,
                        IsRegularIncr = c.IsRegularIncr,
                        Id = data,
                        DBTrack = c.DBTrack
                    };


                    db.IncrPolicy.Attach(corp);
                    db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                    return 1;
                }
                return 0;
            }
        }

        [HttpPost]
        public async Task<ActionResult> EditSave(IncrPolicy c, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string Corp = form["RegIncrPolicylist"] == "0" ? "" : form["RegIncrPolicylist"];
                    string IncrPromoPolicyDetails = form["IncrPolicyDetailslist"] == "0" ? "" : form["IncrPolicyDetailslist"];
                    string Addrs = form["NonRegIncrPolicylist"] == "0" ? "" : form["NonRegIncrPolicylist"];


                    //  bool Auth = form["Autho_Action"] == "" ? false : true;
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;

                    c.RegIncrPolicy_Id = Corp != null && Corp != "" ? int.Parse(Corp) : 0;
                    c.IncrPolicyDetails_Id = IncrPromoPolicyDetails != null && IncrPromoPolicyDetails != "" ? int.Parse(IncrPromoPolicyDetails) : 0;
                    c.NonRegIncrPolicy_Id = Addrs != null && Addrs != "" ? int.Parse(Addrs) : 0;


                    //if (Corp != null)
                    //{
                    //    if (Corp != "")
                    //    {
                    //        var val = db.RegIncrPolicy.Find(int.Parse(Corp));
                    //        c.RegIncrPolicy = val;

                    //        var type = db.IncrPolicy.Include(e => e.RegIncrPolicy).Where(e => e.Id == data).SingleOrDefault();
                    //        IList<IncrPolicy> typedetails = null;
                    //        if (type.RegIncrPolicy != null)
                    //        {
                    //            typedetails = db.IncrPolicy.Where(x => x.RegIncrPolicy.Id == type.RegIncrPolicy.Id && x.Id == data).ToList();
                    //        }
                    //        else
                    //        {
                    //            typedetails = db.IncrPolicy.Where(x => x.Id == data).ToList();
                    //        }
                    //        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                    //        foreach (var s in typedetails)
                    //        {
                    //            s.RegIncrPolicy = c.RegIncrPolicy;
                    //            db.IncrPolicy.Attach(s);
                    //            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                    //            //await db.SaveChangesAsync();
                    //            db.SaveChanges();
                    //            TempData["RowVersion"] = s.RowVersion;
                    //            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    //        }
                    //    }
                    //}
                    //else
                    //{
                    //    var addressdetails = db.IncrPolicy.Include(e => e.RegIncrPolicy).Where(x => x.Id == data).ToList();
                    //    foreach (var s in addressdetails)
                    //    {
                    //       // s.NonRegIncrPolicy = null;
                    //          s.RegIncrPolicy = null;
                    //        //s.IncrPolicyDetails = null;
                    //        db.IncrPolicy.Attach(s);
                    //        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                    //        //await db.SaveChangesAsync();
                    //        db.SaveChanges();
                    //        TempData["RowVersion"] = s.RowVersion;
                    //        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    //    }
                    //}
                    //if (Addrs != null)
                    //{
                    //    if (Addrs != "")
                    //    {
                    //        var val = db.NonRegIncrPolicy.Find(int.Parse(Addrs));
                    //        c.NonRegIncrPolicy = val;

                    //        var add = db.IncrPolicy.Include(e => e.NonRegIncrPolicy).Where(e => e.Id == data).SingleOrDefault();
                    //        IList<IncrPolicy> addressdetails = null;
                    //        if (add.NonRegIncrPolicy != null)
                    //        {
                    //            addressdetails = db.IncrPolicy.Where(x => x.NonRegIncrPolicy.Id == add.NonRegIncrPolicy.Id && x.Id == data).ToList();
                    //        }
                    //        else
                    //        {
                    //            addressdetails = db.IncrPolicy.Where(x => x.Id == data).ToList();
                    //        }
                    //        if (addressdetails != null)
                    //        {
                    //            foreach (var s in addressdetails)
                    //            {
                    //                s.NonRegIncrPolicy = c.NonRegIncrPolicy;
                    //                db.IncrPolicy.Attach(s);
                    //                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                    //                // await db.SaveChangesAsync(false);
                    //                db.SaveChanges();
                    //                TempData["RowVersion"] = s.RowVersion;
                    //                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    //            }
                    //        }
                    //    }
                    //}
                    //else
                    //{
                    //    var addressdetails = db.IncrPolicy.Include(e => e.NonRegIncrPolicy).Where(x => x.Id == data).ToList();
                    //    foreach (var s in addressdetails)
                    //    {
                    //        s.NonRegIncrPolicy = null;
                    //      //  s.RegIncrPolicy = null;
                    //        //s.IncrPolicyDetails = null;
                    //        db.IncrPolicy.Attach(s);
                    //        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                    //        //await db.SaveChangesAsync();
                    //        db.SaveChanges();
                    //        TempData["RowVersion"] = s.RowVersion;
                    //        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    //    }
                    //}
                   


                    //if (IncrPromoPolicyDetails != null)
                    //{
                    //    if (IncrPromoPolicyDetails != "")
                    //    {

                    //        var val = db.IncrPolicyDetails.Find(int.Parse(IncrPromoPolicyDetails));
                    //        c.IncrPolicyDetails = val;
                    //        var add = db.IncrPolicy.Include(e => e.IncrPolicyDetails).Where(e => e.Id == data).SingleOrDefault();
                    //        IList<IncrPolicy> addressdetails = null;
                    //        if (add.IncrPolicyDetails != null)
                    //        {
                    //            addressdetails = db.IncrPolicy.Where(x => x.IncrPolicyDetails.Id == add.IncrPolicyDetails.Id && x.Id == data).ToList();
                    //        }
                    //        else
                    //        {
                    //            addressdetails = db.IncrPolicy.Where(x => x.Id == data).ToList();
                    //        }
                    //        if (addressdetails != null)
                    //        {
                    //            foreach (var s in addressdetails)
                    //            {
                    //                s.IncrPolicyDetails = c.IncrPolicyDetails;
                    //                db.IncrPolicy.Attach(s);
                    //                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                    //                // await db.SaveChangesAsync(false);
                    //                db.SaveChanges();
                    //                TempData["RowVersion"] = s.RowVersion;
                    //                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    //            }
                    //        }
                    //    }
                    //}
                    //else
                    //{
                    //    var addressdetails = db.IncrPolicy.Include(e => e.IncrPolicyDetails).Where(x => x.Id == data).ToList();
                    //    foreach (var s in addressdetails)
                    //    {
                    //      //  s.NonRegIncrPolicy = null;
                    //        //  s.RegIncrPolicy = null;
                    //        s.IncrPolicyDetails = null;
                    //        db.IncrPolicy.Attach(s);
                    //        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                    //        //await db.SaveChangesAsync();
                    //        db.SaveChanges();
                    //        TempData["RowVersion"] = s.RowVersion;
                    //        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    //    }
                    //}

                    if (Auth == false)
                    {


                        if (ModelState.IsValid)
                        {
                            try
                            {
                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                  
                                    //using (var context = new DataBaseContext())
                                    //{
                                       
                                        // originalBlogValues = context.Entry(blog).OriginalValues;
                                    //}

                                  
                                    //var m1 = db.IncrPolicy.Where(e => e.Id == data).ToList();
                                    //foreach (var s in m1)
                                    //{
                                    //    // s.AppraisalPeriodCalendar = null;
                                    //    db.IncrPolicy.Attach(s);
                                    //    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                    //    //await db.SaveChangesAsync();
                                    //    db.SaveChanges();
                                    //    TempData["RowVersion"] = s.RowVersion;
                                    //    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    //}

                                        IncrPolicy IncrPolicy = db.IncrPolicy.Find(data);
                                        TempData["CurrRowVersion"] = IncrPolicy.RowVersion;
                                   // db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {
                                        IncrPolicy blog = null; // to retrieve old data
                                        DbPropertyValues originalBlogValues = null;

                                        c.DBTrack = new DBTrack
                                        {
                                            CreatedBy = IncrPolicy.DBTrack.CreatedBy == null ? null : IncrPolicy.DBTrack.CreatedBy,
                                            CreatedOn = IncrPolicy.DBTrack.CreatedOn == null ? null : IncrPolicy.DBTrack.CreatedOn,
                                            Action = "M",
                                            ModifiedBy = SessionManager.UserName,
                                            ModifiedOn = DateTime.Now
                                        };

                                        //IncrPolicy corp = new IncrPolicy()
                                        //{
                                        if (c.IncrPolicyDetails_Id != 0)
                                            IncrPolicy.IncrPolicyDetails_Id = c.IncrPolicyDetails_Id != null ? c.IncrPolicyDetails_Id : 0;
                                        if (c.NonRegIncrPolicy_Id != 0)
                                            IncrPolicy.NonRegIncrPolicy_Id = c.NonRegIncrPolicy_Id != null ? c.NonRegIncrPolicy_Id : 0;
                                        if (c.RegIncrPolicy_Id != 0)
                                            IncrPolicy.RegIncrPolicy_Id = c.RegIncrPolicy_Id != null ? c.RegIncrPolicy_Id : 0;
                                            IncrPolicy.Name = c.Name;
                                            IncrPolicy.IsRegularIncr = c.IsRegularIncr;
                                            IncrPolicy.Id = data;
                                            IncrPolicy.DBTrack = c.DBTrack;
                                       // };

                                            db.IncrPolicy.Attach(IncrPolicy);
                                            db.Entry(IncrPolicy).State = System.Data.Entity.EntityState.Modified;
                                          //  db.Entry(IncrPolicy).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                        // return 1;

                                    //using (var context = new DataBaseContext())
                                    //{
                                            blog = db.IncrPolicy.Where(e => e.Id == data).Include(e => e.RegIncrPolicy)
                                                                   .Include(e => e.NonRegIncrPolicy)
                                                                   .SingleOrDefault();
                                            originalBlogValues = db.Entry(blog).OriginalValues;
                                            db.ChangeTracker.DetectChanges();
                                        var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                        DT_IncrPolicy DT_Corp = (DT_IncrPolicy)obj;
                                        DT_Corp.RegIncrPolicy_Id = blog.RegIncrPolicy == null ? 0 : blog.RegIncrPolicy.Id;
                                        DT_Corp.NonRegIncrPolicy_Id = blog.NonRegIncrPolicy == null ? 0 : blog.NonRegIncrPolicy.Id;
                                        DT_Corp.IncrPolicyDetails_Id = blog.IncrPolicyDetails == null ? 0 : blog.IncrPolicyDetails.Id;

                                        db.Create(DT_Corp);
                                        db.SaveChanges();
                                   // }
                                    await db.SaveChangesAsync();
                                    }
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
                            IncrPolicy blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            IncrPolicy Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.IncrPolicy.Where(e => e.Id == data).SingleOrDefault();
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

                            IncrPolicy corp = new IncrPolicy()
                            {
                                Name = c.Name == null ? "" : c.Name.Trim(),
                                IsRegularIncr = c.IsRegularIncr,
                                Id = data,
                                DBTrack = c.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, corp, "IncrPolicy", c.DBTrack);
                                // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                Old_Corp = context.IncrPolicy.Where(e => e.Id == data).Include(e => e.RegIncrPolicy)
                                    .Include(e => e.NonRegIncrPolicy).SingleOrDefault();
                                DT_IncrPolicy DT_Corp = (DT_IncrPolicy)obj;
                                DT_Corp.RegIncrPolicy_Id = DBTrackFile.ValCompare(Old_Corp.RegIncrPolicy, c.RegIncrPolicy);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
                                DT_Corp.NonRegIncrPolicy_Id = DBTrackFile.ValCompare(Old_Corp.NonRegIncrPolicy, c.NonRegIncrPolicy); //Old_Corp.BusinessType == c.BusinessType ? 0 : Old_Corp.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_Corp.BusinessType.Id;
                                
                                db.Create(DT_Corp);
                                //db.SaveChanges();
                            }
                            blog.DBTrack = c.DBTrack;
                            db.IncrPolicy.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            // return Json(new Object[] { blog.Id, c.Name, "Record Updated", JsonRequestBehavior.AllowGet });
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = c.Name, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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
        public async Task<ActionResult> Delete(int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    IncrPolicy incrPolicy = db.IncrPolicy.Include(e => e.RegIncrPolicy)
                                                       .Include(e => e.NonRegIncrPolicy)
                                                       .Where(e => e.Id == data).SingleOrDefault();

                    RegIncrPolicy add = incrPolicy.RegIncrPolicy;
                    NonRegIncrPolicy conDet = incrPolicy.NonRegIncrPolicy;

                    //Corporate corporates = db.Corporate.Where(e => e.Id == data).SingleOrDefault();
                    if (incrPolicy.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack, corporates, null, "Corporate");
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = incrPolicy.DBTrack.CreatedBy != null ? incrPolicy.DBTrack.CreatedBy : null,
                                CreatedOn = incrPolicy.DBTrack.CreatedOn != null ? incrPolicy.DBTrack.CreatedOn : null,
                                IsModified = incrPolicy.DBTrack.IsModified == true ? true : false
                            };
                            incrPolicy.DBTrack = dbT;
                            db.Entry(incrPolicy).State = System.Data.Entity.EntityState.Modified;
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack);
                            await db.SaveChangesAsync();
                            using (var context = new DataBaseContext())
                            {
                                ////DBTrackFile.DBTrackSave("Core/P2b.Global", "D", incrPolicy, null, "IncrPolicy", incrPolicy.DBTrack);
                            }
                            ts.Complete();
                            // return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }
                    }
                    else
                    {
                        // var selectedRegions = incrPolicy.Regions;

                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //if (selectedRegions != null)
                            //{
                            //    var corpRegion = new HashSet<int>(incrPolicy.Regions.Select(e => e.Id));
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
                                    CreatedBy = incrPolicy.DBTrack.CreatedBy != null ? incrPolicy.DBTrack.CreatedBy : null,
                                    CreatedOn = incrPolicy.DBTrack.CreatedOn != null ? incrPolicy.DBTrack.CreatedOn : null,
                                    IsModified = incrPolicy.DBTrack.IsModified == true ? false : false,
                                    AuthorizedBy = SessionManager.UserName,
                                    AuthorizedOn = DateTime.Now
                                };

                                // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };

                                db.Entry(incrPolicy).State = System.Data.Entity.EntityState.Deleted;
                                await db.SaveChangesAsync();


                                using (var context = new DataBaseContext())
                                {
                                    incrPolicy.RegIncrPolicy = add;
                                    incrPolicy.NonRegIncrPolicy = conDet;
                                    ////DBTrackFile.DBTrackSave("Core/P2b.Global", "D", incrPolicy, null, "IncrPolicy", dbT);
                                }
                                ts.Complete();
                                //  return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                                Msg.Add("  Data removed successfully.  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }
                            catch (RetryLimitExceededException /* dex */)
                            {
                                //Log the error (uncomment dex variable name and add a line here to write a log.)
                                //ModelState.AddModelError("", "Unable to delete. Try again, and if the problem persists, see your system administrator.");
                                //return RedirectToAction("Delete");
                                // return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });

                                Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }
                        }
                    }
                    // return new EmptyResult();


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


        public ActionResult GetLookup_IncrPolicy(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.IncrPolicy.ToList();
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.IncrPolicy.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
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
