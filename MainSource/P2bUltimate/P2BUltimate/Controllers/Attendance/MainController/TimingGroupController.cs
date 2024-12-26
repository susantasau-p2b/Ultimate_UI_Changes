///
/// Created by Tanushri
///

using P2b.Global;
using Payroll;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Threading.Tasks;
using Attendance;
using P2BUltimate.Security;

namespace P2BUltimate.Controllers.Attendance.MainController
{
    public class TimingGroupController : Controller
    {

        //private DataBaseContext db = new DataBaseContext();
        public ActionResult Index()
        {
            return View("~/Views/Attendance/MainViews/TimingGroup/Index.cshtml");
        }

        public ActionResult partial()
        {
            return View("~/Views/Shared/Attendance/_TimingGroup.cshtml");
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
                int ParentId = 2;
                var jsonData = (Object)null;
                var LKVal = db.TimingGroup.ToList();

                if (gp.IsAutho == true)
                {
                    LKVal = db.TimingGroup.AsNoTracking().Where(e => e.DBTrack.IsModified == true).ToList();
                }
                else
                {
                    LKVal = db.TimingGroup.AsNoTracking().ToList();
                }


                IEnumerable<TimingGroup> IE;
                if (!string.IsNullOrEmpty(gp.searchString))
                {
                    IE = LKVal;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Select(a => new { a.Id, a.GroupCode, a.GroupName }).Where((e => (e.Id.ToString() == gp.searchString) || (e.GroupCode.ToString() == gp.searchString.ToLower()) || (e.GroupName.ToString() == gp.searchString.ToLower())));
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.GroupCode, a.GroupName }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = LKVal;
                    Func<TimingGroup, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "GroupCode" ? c.GroupCode.ToString() :
                                         gp.sidx == "GroupName " ? c.GroupName.ToString() :
                                         "");
                    }

                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.GroupCode, a.GroupName }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.GroupCode, a.GroupName }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.GroupCode, a.GroupName }).ToList();
                    }
                    totalRecords = LKVal.Count();
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
                    total = totalPages,
                    p2bparam = ParentId
                };
                return Json(JsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public ActionResult EditContactDetails_partial(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var val = db.TimingPolicy.Where(e => e.Id == data).SingleOrDefault();

                var r = (from ca in db.TimingPolicy
                         select new
                         {
                             Id = ca.Id,
                             FullDetails = ca.FullDetails
                         }).Where(e => e.Id == data).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetTimingPolicyDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.TimingPolicy.ToList();
                IEnumerable<TimingGroup> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.TimingGroup.ToList().Where(d => d.GroupName.Contains(data));

                }
                else
                {
                    //var list1 = db.TimingGroup.ToList().SelectMany(e => e.TimingPolicy);
                    //var list2 = fall.Except(list1);
                    //  var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.LookupVal }).Distinct();
                    //var list1 = db.TimingGroup.Include(e => e.TimingPolicy).SelectMany(e => e.TimingPolicy);
                    //var list2 = fall.Except(list1);
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                    //var result_1 = (from c in fall
                    //                select new { c.Id, c.CorporateCode, c.CorporateName });
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.GroupName }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            // return View();
        }

        //public ActionResult GetLookupDetails(string data)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        IEnumerable<TimingPolicy>all;
        //        var fall = db.TimingPolicy.ToList();
        //        if (!string.IsNullOrEmpty(data))
        //        {
        //            all = db.TimingPolicy.ToList().Where(d => d.StartingSlab.ToString().Contains(data));
        //            var result = (from c in all
        //                          select new { c.Id, c.StartingSlab }).Distinct();
        //            return Json(result, JsonRequestBehavior.AllowGet);

        //       }
        //        else
        //        {
        //            var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.StartingSlab }).Distinct();
        //            //var result_1 = (from c in fall
        //            //                select new { c.Id, c.CorporateCode, c.CorporateName });
        //            return Json(r, JsonRequestBehavior.AllowGet);

        //        }

        //    }
        //    // return View();
        //}


        private MultiSelectList GetContactDetailsValues(List<int> selectedValues)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<TimingPolicy> lkval = new List<TimingPolicy>();
                lkval = db.TimingPolicy.ToList();
                return new MultiSelectList(lkval, "Id", "FullDetails", selectedValues);
            }
        }


        public ActionResult ValidateForm(TimingGroup L, FormCollection form)
        {

            using (DataBaseContext db = new DataBaseContext())
            {
                L.TimingPolicy = null;
                List<TimingPolicy> OBJ = new List<TimingPolicy>();
                string Values = form["TimingPolicylist"];

                if (Values != null)
                {
                    var ids = Utility.StringIdsToListIds(Values);
                    foreach (var ca in ids)
                    {
                        var OBJ_val = db.TimingPolicy.Find(ca);
                        OBJ.Add(OBJ_val);
                        L.TimingPolicy = OBJ;
                    }
                }

                if (db.TimingGroup.Any(e => e.GroupCode == L.GroupCode))
                {
                    var Msg = new List<string>();
                    Msg.Add("GroupCode Already Exist");
                    return Json(new { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);

            }
        }

        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult Create(TimingGroup NOBJ, FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {


                    NOBJ.TimingPolicy = null;
                    List<TimingPolicy> OBJ = new List<TimingPolicy>();
                    string Values = form["TimingPolicylist"];

                    if (Values != null)
                    {
                        var ids = Utility.StringIdsToListIds(Values);
                        foreach (var ca in ids)
                        {
                            var OBJ_val = db.TimingPolicy.Find(ca);
                            OBJ.Add(OBJ_val);
                            NOBJ.TimingPolicy = OBJ;
                        }
                    }


                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {

                            if (db.TimingGroup.Any(o => o.GroupCode.Replace(" ", String.Empty) == NOBJ.GroupCode.Replace(" ", String.Empty)))
                            {
                                Msg.Add("  TimingGroup GroupName already exists..  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }

                            NOBJ.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            TimingGroup TimingGroup = new TimingGroup()
                            {
                                GroupName = NOBJ.GroupName == null ? "" : NOBJ.GroupName.Trim(),
                                GroupCode = NOBJ.GroupCode == null ? "" : NOBJ.GroupCode.Trim(),
                                IsAutoShift = NOBJ.IsAutoShift,
                                IsManualRotateShift = NOBJ.IsManualRotateShift,
                                TimingPolicy = NOBJ.TimingPolicy,
                                DBTrack = NOBJ.DBTrack
                            };
                            try
                            {



                                db.TimingGroup.Add(TimingGroup);
                                var rtn_Obj = DBTrackFile.DBTrackSave("Attendance/Attendance", null, db.ChangeTracker, NOBJ.DBTrack);
                                DT_TimingGroup DT_OBJ = (DT_TimingGroup)rtn_Obj;
                                db.Create(DT_OBJ);
                                db.SaveChanges();
                                var dat1 = db.TimingGroup.Include(q => q.TimingPolicy).Where(e => e.Id == TimingGroup.Id).SingleOrDefault();
                                ts.Complete();
                                //return this.Json(new Object[] { TimingGroup.Id, TimingGroup.FullDetails, "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = dat1.Id, Val = dat1.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }

                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = NOBJ.Id });
                            }
                            catch (DataException /* dex */)
                            {
                                // return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
                                Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
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
                        return this.Json(new { msg = errorMsg });
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

        public int EditS(string Lkval, string OBJ, int data, TimingGroup ESOBJ, DBTrack dbT)
        {
            //if (Lkval != null)
            //{
            //    if (Lkval != "")
            //    {
            //        var val = db.LookupValue.Find(int.Parse(Lkval));
            //        ESOBJ.IsManualRotateShift = val;

            //        var type = db.TimingGroup.Include(e => e.IsManualRotateShift).Where(e => e.Id == data).SingleOrDefault();
            //        IList<TimingGroup> typedetails = null;
            //        if (type.IsManualRotateShift != null)
            //        {
            //            typedetails = db.TimingGroup.Where(x => x.IsManualRotateShift.Id == type.IsManualRotateShift.Id && x.Id == data).ToList();
            //        }
            //        else
            //        {
            //            typedetails = db.TimingGroup.Where(x => x.Id == data).ToList();
            //        }
            //        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
            //        foreach (var s in typedetails)
            //        {
            //            s.IsManualRotateShift = ESOBJ.IsManualRotateShift;
            //            db.TimingGroup.Attach(s);
            //            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
            //            //await db.SaveChangesAsync();
            //            db.SaveChanges();
            //            TempData["RowVersion"] = s.RowVersion;
            //            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
            //        }
            //    }
            //    else
            //    {
            //        var InstituteTypeDetails = db.TimingGroup.Include(e => e.IsManualRotateShift).Where(x => x.Id == data).ToList();
            //        foreach (var s in InstituteTypeDetails)
            //        {
            //            s.IsManualRotateShift = null;
            //            db.TimingGroup.Attach(s);
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
            //    var InstituteTypeDetails = db.TimingGroup.Include(e => e.IsManualRotateShift).Where(x => x.Id == data).ToList();
            //    foreach (var s in InstituteTypeDetails)
            //    {
            //        s.IsManualRotateShift = null;
            //        db.TimingGroup.Attach(s);
            //        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
            //        //await db.SaveChangesAsync();
            //        db.SaveChanges();
            //        TempData["RowVersion"] = s.RowVersion;
            //        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
            //    }
            //}

            //var db_data = db.TimingGroup.Include(e => e.TimingPolicy).Where(e => e.Id == data).SingleOrDefault();
            //List<TimingPolicy> lookupval = new List<TimingPolicy>();
            //string Values = OBJ;

            //if (Values != null)
            //{
            //    var ids = Utility.StringIdsToListIds(Values);
            //    foreach (var ca in ids)
            //    {
            //        var Lookup_val = db.TimingPolicy.Find(ca);
            //        lookupval.Add(Lookup_val);
            //        db_data.TimingPolicy = lookupval;
            //    }
            //}
            //else
            //{
            //    db_data.TimingPolicy = null;
            //}
            using (DataBaseContext db = new DataBaseContext())
            {
                if (OBJ != null)
                {
                    if (OBJ != "")
                    {

                        List<int> IDs = OBJ.Split(',').Select(e => int.Parse(e)).ToList();
                        foreach (var k in IDs)
                        {
                            var value = db.TimingPolicy.Find(k);
                            ESOBJ.TimingPolicy = new List<TimingPolicy>();
                            ESOBJ.TimingPolicy.Add(value);
                        }
                    }
                }
                else
                {
                    var Details = db.TimingGroup.Include(e => e.TimingPolicy).Where(x => x.Id == data).ToList();
                    foreach (var s in Details)
                    {
                        s.TimingPolicy = null;
                        db.TimingGroup.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }

                //db.TimingGroup.Attach(db_data);
                //db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                //db.SaveChanges();
                //TempData["RowVersion"] = db_data.RowVersion;
                //db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;



                var CurOBJ = db.TimingGroup.Find(data);
                TempData["CurrRowVersion"] = CurOBJ.RowVersion;
                db.Entry(CurOBJ).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    ESOBJ.DBTrack = dbT;
                    TimingGroup BOBJ = new TimingGroup()
                    {
                        GroupName = ESOBJ.GroupName,
                        GroupCode = ESOBJ.GroupCode,
                        IsAutoShift = ESOBJ.IsAutoShift,
                        Id = data,
                        DBTrack = ESOBJ.DBTrack
                    };


                    db.TimingGroup.Attach(BOBJ);
                    db.Entry(BOBJ).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(BOBJ).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    ////  DBTrackFile.DBTrackSave("Attendance/Attendance", originalBlogValues, db.ChangeTracker, c.DBTrack);
                    return 1;
                }
                return 0;

            }
        }

        [HttpPost]
        public async Task<ActionResult> AuthSave(int auth_id, Boolean isauth, String auth_action) //to save authorised data
        {

            using (DataBaseContext db = new DataBaseContext())
            {
                if (auth_action == "C")
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        TimingGroup TimingGroup = db.TimingGroup.Include(e => e.TimingPolicy)
                          .FirstOrDefault(e => e.Id == auth_id);
                        TimingGroup.DBTrack = new DBTrack
                        {
                            Action = "C",
                            ModifiedBy = TimingGroup.DBTrack.ModifiedBy != null ? TimingGroup.DBTrack.ModifiedBy : null,
                            CreatedBy = TimingGroup.DBTrack.CreatedBy != null ? TimingGroup.DBTrack.CreatedBy : null,
                            CreatedOn = TimingGroup.DBTrack.CreatedOn != null ? TimingGroup.DBTrack.CreatedOn : null,
                            IsModified = TimingGroup.DBTrack.IsModified == true ? false : false,
                            AuthorizedBy = SessionManager.UserName,
                            AuthorizedOn = DateTime.Now
                        };
                        db.TimingGroup.Attach(TimingGroup);
                        db.Entry(TimingGroup).State = System.Data.Entity.EntityState.Modified;
                        db.Entry(TimingGroup).OriginalValues["RowVersion"] = TempData["RowVersion"];

                        var rtn_Obj = DBTrackFile.DBTrackSave("Attendance/Attendance", null, db.ChangeTracker, TimingGroup.DBTrack);
                        DT_TimingGroup DT_OBJ = (DT_TimingGroup)rtn_Obj;

                        db.Create(DT_OBJ);
                        await db.SaveChangesAsync();


                        ts.Complete();
                        return Json(new Object[] { TimingGroup.Id, TimingGroup.GroupName, "Record Authorised", JsonRequestBehavior.AllowGet });
                    }
                }
                else if (auth_action == "M")
                {
                    TimingGroup Old_OBJ = db.TimingGroup
                                                      .Include(e => e.TimingPolicy).Where(e => e.Id == auth_id).SingleOrDefault();


                    DT_TimingGroup Curr_OBJ = db.DT_TimingGroup
                                                .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                                .OrderByDescending(e => e.Id)
                                                .FirstOrDefault();

                    if (Curr_OBJ != null)
                    {
                        TimingGroup TimingGroup = new TimingGroup();
                        string LKVAL = "";
                        //string LKVAL = Curr_OBJ.InstituteType_Id == null ? null : Curr_OBJ.InstituteType_Id.ToString();
                        string CONVAL = Curr_OBJ.TimingPolicy_Id == null ? null : Curr_OBJ.TimingPolicy_Id.ToString();
                        string TimingPolicy = Curr_OBJ.TimingPolicy_Id == null ? null : Curr_OBJ.TimingPolicy_Id.ToString();
                        TimingGroup.GroupName = Curr_OBJ.GroupName == null ? Old_OBJ.GroupName : Curr_OBJ.GroupName;
                        TimingGroup.GroupCode = Curr_OBJ.GroupCode == null ? Old_OBJ.GroupCode : Curr_OBJ.GroupCode;
                        TimingGroup.IsAutoShift = Curr_OBJ.IsAutoShift == null ? Old_OBJ.IsAutoShift : Curr_OBJ.IsAutoShift;
                        //      corp.Id = auth_id;

                        if (ModelState.IsValid)
                        {
                            try
                            {

                                //DbContextTransaction transaction = db.Database.BeginTransaction();

                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    // db.Configuration.AutoDetectChangesEnabled = false;
                                    TimingGroup.DBTrack = new DBTrack
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

                                    //int a = EditS(LKVAL, CONVAL, auth_id, TimingGroup, TimingGroup.DBTrack);
                                    //db.Entry(Old_OBJ).State = System.Data.Entity.EntityState.Detached;
                                    //db.TimingGroup.Attach(TimingGroup);
                                    //db.Entry(TimingGroup).State = System.Data.Entity.EntityState.Modified;
                                    //db.Entry(TimingGroup).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                    //db.SaveChanges();
                                    //db.Entry(TimingGroup).State = System.Data.Entity.EntityState.Detached;

                                    await db.SaveChangesAsync();

                                    ts.Complete();
                                    return Json(new Object[] { TimingGroup.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (TimingGroup)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (TimingGroup)databaseEntry.ToObject();
                                    TimingGroup.RowVersion = databaseValues.RowVersion;
                                }
                            }

                            return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else
                        return Json(new Object[] { "", "Data removed from history", JsonRequestBehavior.AllowGet });


                }
                else if (auth_action == "D")
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        //TimingGroup Lkval = db.TimingGroup.Find(auth_id);
                        TimingGroup TimingGroup = db.TimingGroup.AsNoTracking().Include(e => e.TimingPolicy)
                                                                    .FirstOrDefault(e => e.Id == auth_id);
                        var selectedValues = TimingGroup.TimingPolicy;
                        TimingGroup.DBTrack = new DBTrack
                        {
                            Action = "D",
                            ModifiedBy = TimingGroup.DBTrack.ModifiedBy != null ? TimingGroup.DBTrack.ModifiedBy : null,
                            CreatedBy = TimingGroup.DBTrack.CreatedBy != null ? TimingGroup.DBTrack.CreatedBy : null,
                            CreatedOn = TimingGroup.DBTrack.CreatedOn != null ? TimingGroup.DBTrack.CreatedOn : null,
                            IsModified = TimingGroup.DBTrack.IsModified == true ? false : false,
                            AuthorizedBy = SessionManager.UserName,
                            AuthorizedOn = DateTime.Now
                        };
                        db.TimingGroup.Attach(TimingGroup);
                        db.Entry(TimingGroup).State = System.Data.Entity.EntityState.Deleted;
                        await db.SaveChangesAsync();
                        using (var context = new DataBaseContext())
                        {
                            TimingGroup.TimingPolicy = selectedValues;
                            // DBTrackFile.DBTrackSave("Attendance/Attendance", "D", TimingGroup, null, "TimingGroup", TimingGroup.DBTrack);
                        }
                        db.Entry(TimingGroup).State = System.Data.Entity.EntityState.Detached;
                        ts.Complete();
                        return Json(new Object[] { "", "", "Record Authorised", JsonRequestBehavior.AllowGet });
                    }

                }
                return View();

            }
        }

        [HttpPost]
        public async Task<ActionResult> EditSave(TimingGroup ESOBJ, int data, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;
                    var db_data = db.TimingGroup.Include(e => e.TimingPolicy).Where(e => e.Id == data).SingleOrDefault();
                    List<TimingPolicy> TimingPolicy = new List<TimingPolicy>();
                    string Values = form["TimingPolicylist"];

                    if (Values != null)
                    {
                        var ids = Utility.StringIdsToListIds(Values);
                        foreach (var ca in ids)
                        {
                            var ContactDetails_val = db.TimingPolicy.Find(ca);
                            TimingPolicy.Add(ContactDetails_val);
                            db_data.TimingPolicy = TimingPolicy;
                        }
                    }
                    else
                    {
                        db_data.TimingPolicy = null;
                    }

                    string INTYPE = "";
                    //string INTYPE = form["InstituteTypelist"] == "0" ? "" : form["InstituteTypelist"];
                    //if (INTYPE != null)
                    //{
                    //    if (INTYPE != "")
                    //    {
                    //        var val = db.LookupValue.Find(int.Parse(INTYPE));
                    //        ESOBJ.IsManualRotateShift = val;
                    //    }
                    //}


                    //db.TimingGroup.Attach(db_data);
                    //db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                    //db.SaveChanges();
                    //TempData["RowVersion"] = db_data.RowVersion;
                    //db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;



                    if (Auth == false)
                    {
                        if (ModelState.IsValid)
                        {
                            try
                            {
                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    db.TimingGroup.Attach(db_data);
                                    db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    TempData["RowVersion"] = db_data.RowVersion;
                                    db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;

                                    var Curr_OBJ = db.TimingGroup.Find(data);
                                    TempData["CurrRowVersion"] = Curr_OBJ.RowVersion;
                                    db.Entry(Curr_OBJ).State = System.Data.Entity.EntityState.Detached;

                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {
                                        TimingGroup blog = blog = null;
                                        DbPropertyValues originalBlogValues = null;

                                        using (var context = new DataBaseContext())
                                        {
                                            blog = context.TimingGroup.Where(e => e.Id == data).SingleOrDefault();
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
                                        TimingGroup OBJ = new TimingGroup
                                        {
                                            Id = data,
                                            TimingPolicy = db_data.TimingPolicy,
                                            GroupCode = ESOBJ.GroupCode,
                                            GroupName = ESOBJ.GroupName,
                                            IsAutoShift = ESOBJ.IsAutoShift,
                                            IsManualRotateShift = ESOBJ.IsManualRotateShift,
                                            DBTrack = ESOBJ.DBTrack
                                        };
                                        db.TimingGroup.Attach(OBJ);
                                        db.Entry(OBJ).State = System.Data.Entity.EntityState.Modified;
                                        db.Entry(OBJ).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                        var obj = DBTrackFile.DBTrackSave("Attendance/Attendance", originalBlogValues, db.ChangeTracker, ESOBJ.DBTrack);
                                        DT_TimingGroup DT_OBJ = (DT_TimingGroup)obj;
                                        db.Create(DT_OBJ);
                                        db.SaveChanges();
                                        await db.SaveChangesAsync();
                                        db.Entry(OBJ).State = System.Data.Entity.EntityState.Detached;
                                        ts.Complete();
                                        //  return Json(new Object[] { OBJ.Id, OBJ.FullDetails  , "Record Updated", JsonRequestBehavior.AllowGet });
                                        Msg.Add("  Record Updated");
                                        return Json(new Utility.JsonReturnClass { Id = OBJ.Id, Val = OBJ.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    }
                                }
                            }

                            catch (DbUpdateException e) { throw e; }
                            catch (DataException e) { throw e; }
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
                            return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
                        }
                    }
                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            TimingGroup blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            //TimingGroup Old_LKup = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.TimingGroup.Where(e => e.Id == data).SingleOrDefault();
                                TempData["RowVersion"] = blog.RowVersion;
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
                            TimingGroup TimingGroup = new TimingGroup()
                            {

                                TimingPolicy = db_data.TimingPolicy,
                                GroupCode = blog.GroupCode,
                                GroupName = blog.GroupName,
                                IsAutoShift = blog.IsAutoShift,
                                IsManualRotateShift = blog.IsManualRotateShift,
                                Id = data,
                                DBTrack = ESOBJ.DBTrack,

                            };
                            db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;
                            db.TimingGroup.Attach(TimingGroup);
                            db.Entry(TimingGroup).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(TimingGroup).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            TempData["RowVersion"] = db_data.RowVersion;
                            db.Entry(TimingGroup).State = System.Data.Entity.EntityState.Detached;


                            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Attendance/Attendance", "M", blog, ESOBJ, "TimingGroup", ESOBJ.DBTrack);
                                // var obj = DBTrackFile.DBTrackSave("Attendance/Attendance", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                //Old_LKup = context.TimingGroup.Where(e => e.Id == data).Include(e => e.BasicScaleDetails).SingleOrDefault();
                                DT_TimingGroup DT_LKup = (DT_TimingGroup)obj;

                                db.Create(DT_LKup);
                                //db.SaveChanges();
                            }

                            ts.Complete();
                            //   return Json(new Object[] { blog.Id, ESOBJ.GroupName, "Record Updated", JsonRequestBehavior.AllowGet });
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = ESOBJ.GroupName, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> EditSave1(TimingGroup NOBJ, int data, FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var db_data = db.TimingGroup.Include(e => e.TimingPolicy).Where(e => e.Id == data).SingleOrDefault();
                List<TimingPolicy> TimingPolicy = new List<TimingPolicy>();
                string Values = form["TimingPolicylist"];

                if (Values != null)
                {
                    var ids = Utility.StringIdsToListIds(Values);
                    foreach (var ca in ids)
                    {
                        var Medicine_val = db.TimingPolicy.Find(ca);
                        TimingPolicy.Add(Medicine_val);
                        db_data.TimingPolicy = TimingPolicy;
                    }
                }
                else
                {
                    db_data.TimingPolicy = null;
                }


                db.TimingGroup.Attach(db_data);
                db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                TempData["RowVersion"] = db_data.RowVersion;
                db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;



                try
                {


                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            var Curr_Medicine = db.TimingGroup.Find(data);
                            TempData["CurrRowVersion"] = Curr_Medicine.RowVersion;
                            db.Entry(Curr_Medicine).State = System.Data.Entity.EntityState.Detached;

                            if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                            {
                                TimingGroup blog = blog = null;
                                DbPropertyValues originalBlogValues = null;

                                using (var context = new DataBaseContext())
                                {
                                    blog = context.TimingGroup.Where(e => e.Id == data).SingleOrDefault();
                                    originalBlogValues = context.Entry(blog).OriginalValues;
                                }

                                NOBJ.DBTrack = new DBTrack
                                {
                                    CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                    CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                    Action = "M",
                                    ModifiedBy = SessionManager.UserName,
                                    ModifiedOn = DateTime.Now
                                };
                                TimingGroup TimingGroup = new TimingGroup
                                {
                                    Id = data,
                                    TimingPolicy = NOBJ.TimingPolicy,
                                    GroupName = NOBJ.GroupName == null ? "" : NOBJ.GroupName.Trim(),
                                    DBTrack = NOBJ.DBTrack
                                };


                                db.TimingGroup.Attach(TimingGroup);
                                db.Entry(TimingGroup).State = System.Data.Entity.EntityState.Modified;

                                // db.Entry(c.DBTrack).State = System.Data.Entity.EntityState.Modified;
                                //db.SaveChanges();
                                db.Entry(TimingGroup).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                DBTrackFile.DBTrackSave("Attendance/Attendance", originalBlogValues, db.ChangeTracker, NOBJ.DBTrack);
                                await db.SaveChangesAsync();
                                //DisplayTrackedEntities(db.ChangeTracker);
                                db.Entry(TimingGroup).State = System.Data.Entity.EntityState.Detached;
                                ts.Complete();
                                return Json(new Object[] { TimingGroup.Id, TimingGroup.GroupName, "Record Updated", JsonRequestBehavior.AllowGet });
                            }
                        }
                    }
                    return View();
                }
                catch (DbUpdateException e) { throw e; }
                catch (DataException e) { throw e; }

            }
        }

        public ActionResult Createcontactdetails_partial()
        {
            return View("~/Views/Shared/Core/_ContactDetails.cshtml");
        }

        public class TimingPolicy_CD
        {
            public Array TimingPolicy_Id { get; set; }
            public Array TimingPolicy_FullDetails { get; set; }
        }

        [HttpPost]
        public ActionResult Edit(int data)
        {
            List<TimingPolicy_CD> return_data = new List<TimingPolicy_CD>();
            using (DataBaseContext db = new DataBaseContext())
            {
                var r = db.TimingGroup.Include(e => e.TimingPolicy).Where(e => e.Id == data)
                       .Select(e => new
                       {


                           GroupName = e.GroupName,
                           IsAutoShift = e.IsAutoShift,
                           GroupCode = e.GroupCode,
                           IsManualRotateShift = e.IsManualRotateShift,
                           Action = e.DBTrack.Action



                       }).ToList();

                // var a = db.TimingGroup.Include(e => e.TimingPolicy).Where(e => e.Id == data).Select(e => e.TimingPolicy).ToList();
                var k = db.TimingGroup.Include(e => e.TimingPolicy).Where(e => e.Id == data).SingleOrDefault();


                return_data.Add(
            new TimingPolicy_CD
            {
                TimingPolicy_Id = k.TimingPolicy.Select(a => a.Id.ToString()).ToArray(),
                TimingPolicy_FullDetails = k.TimingPolicy.Select(e => e.FullDetails).ToArray()

                //TimingPolicy_Id = ca.Select(e => e.Id.ToString()).ToArray(),
                // TimingPolicy_FullDetails = ca.Select(e => e.FullDetails).ToArray()
            });
                //var a = db.TimingGroup.Include(e => e.TimingPolicy).Where(e => e.Id == data).Select(e => e.TimingPolicy).SingleOrDefault();
                //var BCDETAILS = (from ca in a
                //                 select new
                //                 {
                //                     Id = ca.Id,
                //                     TimingPolicy_Id = ca.Id,
                //                     TimingPolicy_FullDetails = ca.FullDetails
                //                 }).Distinct();

                TempData["RowVersion"] = db.TimingGroup.Find(data).RowVersion;

                // var Old_Data = db.DT_TimingGroup
                // .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M")
                // .Select
                // (e => new
                // {
                //     DT_Id = e.Id,
                //     GroupName = e.GroupName == null ? "" : e.GroupName,
                //     GroupCode = e.GroupCode == null ? "" : e.GroupCode,
                //     IsAutoShift = e.IsAutoShift,
                //     IsManualRotateShift = e.IsManualRotateShift,
                //     Contact_Val = e.TimingPolicy_Id == 0 ? "" : db.TimingPolicy.Where(x => x.Id == e.TimingPolicy_Id).Select(x => x.FullDetails).FirstOrDefault()
                // }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var Lkval = db.TimingGroup.Find(data);
                TempData["RowVersion"] = Lkval.RowVersion;
                var Auth = Lkval.DBTrack.IsModified;

                return Json(new Object[] { r, return_data, Auth, JsonRequestBehavior.AllowGet });
                //return this.Json(new Object[] { r, a, JsonRequestBehavior.AllowGet });
            }
        }

        [HttpPost]
        public async Task<ActionResult> Delete1(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                TimingGroup TimingGroup = db.TimingGroup.Include(e => e.TimingPolicy)
                                                   .Where(e => e.Id == data).SingleOrDefault();

                var add = TimingGroup.TimingPolicy;

                //TimingGroup TimingGroup = db.TimingGroup.Where(e => e.Id == data).SingleOrDefault();
                if (TimingGroup.DBTrack.IsModified == true)
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        //DBTrackFile.DBTrackSave("Attendance/Attendance", null, db.ChangeTracker, TimingGroup.DBTrack, TimingGroup, null, "TimingGroup");
                        DBTrack dbT = new DBTrack
                        {
                            Action = "D",
                            CreatedBy = TimingGroup.DBTrack.CreatedBy != null ? TimingGroup.DBTrack.CreatedBy : null,
                            CreatedOn = TimingGroup.DBTrack.CreatedOn != null ? TimingGroup.DBTrack.CreatedOn : null,
                            IsModified = TimingGroup.DBTrack.IsModified == true ? true : false
                        };
                        TimingGroup.DBTrack = dbT;
                        db.Entry(TimingGroup).State = System.Data.Entity.EntityState.Modified;
                        //DBTrackFile.DBTrackSave("Attendance/Attendance", "D", TimingGroup, null, "TimingGroup", TimingGroup.DBTrack);
                        await db.SaveChangesAsync();
                        using (var context = new DataBaseContext())
                        {
                            //DBTrackFile.DBTrackSave("Attendance/Attendance", "D", TimingGroup, null, "TimingGroup", TimingGroup.DBTrack);
                        }
                        ts.Complete();
                        return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                    }
                }
                else
                {
                    //var selectedRegions = TimingGroup.Regions;

                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        //if (selectedRegions != null)
                        //{
                        //    var corpRegion = new HashSet<int>(TimingGroup.Regions.Select(e => e.Id));
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
                                CreatedBy = TimingGroup.DBTrack.CreatedBy != null ? TimingGroup.DBTrack.CreatedBy : null,
                                CreatedOn = TimingGroup.DBTrack.CreatedOn != null ? TimingGroup.DBTrack.CreatedOn : null,
                                IsModified = TimingGroup.DBTrack.IsModified == true ? false : false,
                                AuthorizedBy = SessionManager.UserName,
                                AuthorizedOn = DateTime.Now
                            };

                            // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };

                            db.Entry(TimingGroup).State = System.Data.Entity.EntityState.Deleted;
                            await db.SaveChangesAsync();


                            using (var context = new DataBaseContext())
                            {
                                TimingGroup.TimingPolicy = add;
                                //DBTrackFile.DBTrackSave("Attendance/Attendance", "D", TimingGroup, null, "TimingGroup", dbT);
                            }
                            ts.Complete();
                            return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });

                        }
                        catch (RetryLimitExceededException /* dex */)
                        {
                            //Log the error (uncomment dex variable GroupName and add a line here to write a log.)
                            //ModelState.AddModelError("", "Unable to delete. Try again, and if the problem persists, see your system administrator.");
                            //return RedirectToAction("Delete");
                            return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });

                        }
                    }
                }
                return new EmptyResult();
            }

        }

        public async Task<ActionResult> Delete(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    TimingGroup TIOBJ = db.TimingGroup
                                                       .Include(e => e.TimingPolicy)
                                                       .Where(e => e.Id == data).SingleOrDefault();


                    //LookupValue val = TIOBJ.IsManualRotateShift;
                    // TimingPolicy tp = TIOBJ.TimingPolicy;

                    if (TIOBJ.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //DBTrackFile.DBTrackSave("Attendance/Attendance", null, db.ChangeTracker, TIOBJ.DBTrack, TIOBJ, null, "TimingGroup");
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = TIOBJ.DBTrack.CreatedBy != null ? TIOBJ.DBTrack.CreatedBy : null,
                                CreatedOn = TIOBJ.DBTrack.CreatedOn != null ? TIOBJ.DBTrack.CreatedOn : null,
                                IsModified = TIOBJ.DBTrack.IsModified == true ? true : false
                            };
                            TIOBJ.DBTrack = dbT;
                            db.Entry(TIOBJ).State = System.Data.Entity.EntityState.Modified;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Attendance/Attendance", null, db.ChangeTracker, TIOBJ.DBTrack);
                            DT_TimingGroup DT_Corp = (DT_TimingGroup)rtn_Obj;

                            //DT_Corp.InstituteType_Id = TIOBJ.IsManualRotateShift == null ? 0 : TIOBJ.IsManualRotateShift.Id;

                            db.Create(DT_Corp);
                            // db.SaveChanges();
                            //DBTrackFile.DBTrackSave("Attendance/Attendance", "D", TIOBJ, null, "TimingGroup", TIOBJ.DBTrack);
                            await db.SaveChangesAsync();
                            //using (var context = new DataBaseContext())
                            //{
                            //   // DBTrackFile.DBTrackSave("Attendance/Attendance", "D", TIOBJ, null, "TimingGroup", TIOBJ.DBTrack );
                            //}
                            ts.Complete();
                            // return Json(new Object[] { "", "Data will be removed after authorization.", JsonRequestBehavior.AllowGet });
                            Msg.Add(" Data will be removed after authorization. ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);


                        }
                    }
                    else
                    {
                        var TimiggroupPolicy = TIOBJ.TimingPolicy;

                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            if (TimiggroupPolicy != null)
                            {
                                var TPolicy = new HashSet<int>(TIOBJ.TimingPolicy.Select(e => e.Id));
                                if (TPolicy.Count > 0)
                                {
                                    //   return Json(new Object[] { "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                                    // return Json(new Object[] { "", "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                                    Msg.Add(" Child record exists.Cannot remove it..  ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }
                            }
                            var v = db.TimingMonthlyRoaster.Where(a => a.TimingGroup.Id == TIOBJ.Id).ToList();
                            if (v != null)
                            {

                                if (v.Count > 0)
                                {
                                    Msg.Add(" Child record exists.Cannot remove it..  ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }
                            }


                            var v1 = db.TimingWeeklySchedule.Where(a => a.TimingGroup.Id == TIOBJ.Id).ToList();
                            if (v1 != null)
                            {

                                if (v1.Count > 0)
                                {
                                    Msg.Add(" Child record exists.Cannot remove it..  ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }
                            }


                            try
                            {
                                DBTrack dbT = new DBTrack
                                {
                                    Action = "D",
                                    ModifiedBy = SessionManager.UserName,
                                    ModifiedOn = DateTime.Now,
                                    CreatedBy = TIOBJ.DBTrack.CreatedBy != null ? TIOBJ.DBTrack.CreatedBy : null,
                                    CreatedOn = TIOBJ.DBTrack.CreatedOn != null ? TIOBJ.DBTrack.CreatedOn : null,
                                    IsModified = TIOBJ.DBTrack.IsModified == true ? false : false//,
                                    //AuthorizedBy = SessionManager.UserName,
                                    //AuthorizedOn = DateTime.Now
                                };

                                // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };

                                db.Entry(TIOBJ).State = System.Data.Entity.EntityState.Deleted;
                                var rtn_Obj = DBTrackFile.DBTrackSave("Attendance/Attendance", null, db.ChangeTracker, dbT);
                                DT_TimingGroup DT_OBJ = (DT_TimingGroup)rtn_Obj;
                                //DT_Corp.Address_Id = add == null ? 0 : add.Id;
                                //DT_OBJ.InstituteType_Id = val == null ? 0 : val.Id;
                                //  DT_OBJ.TimingPolicy_Id = conDet == null ? 0 : conDet.Id;
                                db.Create(DT_OBJ);

                                await db.SaveChangesAsync();


                                //using (var context = new DataBaseContext())
                                //{
                                //    TIOBJ.Address = add;
                                //    TIOBJ.TimingPolicy = conDet;
                                //    TIOBJ.BusinessType = val;
                                //    DBTrackFile.DBTrackSave("Attendance/Attendance", null, db.ChangeTracker, dbT);
                                //    //DBTrackFile.DBTrackSave("Attendance/Attendance", "D", TIOBJ, null, "TimingGroup", dbT);
                                //}
                                ts.Complete();
                                //  return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                                Msg.Add("  Data removed successfully.  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }
                            catch (RetryLimitExceededException /* dex */)
                            {
                                //Log the error (uncomment dex variable GroupName and add a line here to write a log.)
                                //ModelState.AddModelError("", "Unable to delete. Try again, and if the problem persists, see your system administrator.");
                                //return RedirectToAction("Delete");
                                //  return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
                                Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

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

        public class returndatagridclass //childgrid
        {
            public string Id { get; set; }
            public string GroupCode { get; set; }
            public string GroupName { get; set; }

        }

        public ActionResult Emp_Grid(ParamModel param, string y)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var all = db.TimingGroup.Include(e => e.TimingPolicy)
                        .ToList();
                    // for searchs
                    IEnumerable<TimingGroup> fall;
                    if (param.sSearch == null)
                    {
                        fall = all;
                    }
                    else
                    {

                        fall = all.Where(e => (e.Id.ToString().Contains(param.sSearch))
                                  || (e.GroupCode.ToString().ToLower().Contains(param.sSearch.ToLower()))
                                  || (e.GroupName.ToString().ToUpper().Contains(param.sSearch.ToUpper()))

                                  ).ToList();
                    }

                    //for column sorting
                    var sortindex = Convert.ToInt32(Request["iSortCol_0"]);

                    Func<TimingGroup, string> orderfunc = (c =>
                                                                Convert.ToUInt32(sortindex) == 0 ? c.Id.ToString() :
                                                                sortindex == 1 ? c.GroupCode.ToString() : "");
                    var sortcolumn = Request["sSortDir_0"];
                    if (sortcolumn == "asc")
                    {
                        fall = fall.OrderBy(orderfunc);
                    }
                    else
                    {
                        fall = fall.OrderBy(orderfunc);
                    }
                    // Paging 
                    var dcompanies = fall
                            .Skip(param.iDisplayStart).Take(param.iDisplayLength).ToList();
                    if (dcompanies.Count == 0)
                    {
                        List<returndatagridclass> result = new List<returndatagridclass>();
                        foreach (var item in fall)
                        {

                            result.Add(new returndatagridclass
                            {
                                Id = item.Id.ToString(),
                                GroupCode = item.GroupCode != null ? item.GroupCode : null,
                                GroupName = item.GroupName != null ? item.GroupName : null,

                            });

                        }
                        return Json(new
                        {
                            sEcho = param.sEcho,
                            iTotalRecords = all.Count(),
                            iTotalDisplayRecords = fall.Count(),
                            data = result
                        }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        var result = from c in dcompanies

                                     select new[] { null, Convert.ToString(c.Id), c.GroupName, c.GroupName };
                        return Json(new
                        {
                            sEcho = param.sEcho,
                            iTotalRecords = all.Count(),
                            iTotalDisplayRecords = fall.Count(),
                            data = result
                        }, JsonRequestBehavior.AllowGet);
                    }//for data reterivation
                }
                catch (Exception e)
                {
                    List<string> Msg = new List<string>();
                    LogFile Logfile = new LogFile();
                    ErrorLog Err = new ErrorLog()
                    {
                        ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                        ExceptionMessage = e.Message,
                        ExceptionStackTrace = e.StackTrace,
                        LineNo = Convert.ToString(new System.Diagnostics.StackTrace(e, true).GetFrame(0).GetFileLineNumber()),
                        LogTime = DateTime.Now
                    };
                    Logfile.CreateLogFile(Err);
                    Msg.Add(e.Message);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public class LoanAdvReqChildDataClass //childgrid
        {
            public int Id { get; set; }
            
            public string FullDetails { get; set; }

        }

        public ActionResult Get_TimingPolicy(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var db_data = db.TimingGroup
                        .Include(e => e.TimingPolicy)
                        .Where(e => e.Id == data)
                        .SingleOrDefault();

                    if (db_data.TimingPolicy != null)
                    {
                        List<LoanAdvReqChildDataClass> returndata = new List<LoanAdvReqChildDataClass>();

                        foreach (var item in db_data.TimingPolicy.OrderBy(e => e.Id))
                        {
                            returndata.Add(new LoanAdvReqChildDataClass
                            {
                                Id = item.Id,
                                FullDetails = item.FullDetails != null ? item.FullDetails : "",

                            });
                        }
                        return Json(returndata, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return null;
                    }

                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }
    }
}