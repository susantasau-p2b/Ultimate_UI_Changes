
///
/// Created by Kapil
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
using Attendance;
using P2BUltimate.Security;

namespace P2BUltimate.Controllers.Attendance.MainController
{
    public class TimingPolicyBatchAssignmentController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();
        List<string> Msg = new List<string>();

        public ActionResult Index()
        {
            return View("~/Views/Attendance/MainViews/TimingPolicyBatchAssignment/Index.cshtml");
        }

        public int EditS(string Corp, string Addrs, string ContactDetails, int data, Corporate c, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (Corp != null)
                {
                    if (Corp != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(Corp));
                        c.BusinessType = val;

                        var type = db.Corporate.Include(e => e.BusinessType).Where(e => e.Id == data).SingleOrDefault();
                        IList<Corporate> typedetails = null;
                        if (type.BusinessType != null)
                        {
                            typedetails = db.Corporate.Where(x => x.BusinessType.Id == type.BusinessType.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            typedetails = db.Corporate.Where(x => x.Id == data).ToList();
                        }
                        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                        foreach (var s in typedetails)
                        {
                            s.BusinessType = c.BusinessType;
                            db.Corporate.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                    else
                    {
                        var BusiTypeDetails = db.Corporate.Include(e => e.BusinessType).Where(x => x.Id == data).ToList();
                        foreach (var s in BusiTypeDetails)
                        {
                            s.BusinessType = null;
                            db.Corporate.Attach(s);
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
                    var BusiTypeDetails = db.Corporate.Include(e => e.BusinessType).Where(x => x.Id == data).ToList();
                    foreach (var s in BusiTypeDetails)
                    {
                        s.BusinessType = null;
                        db.Corporate.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }

                if (Addrs != null)
                {
                    if (Addrs != "")
                    {
                        var val = db.Address.Find(int.Parse(Addrs));
                        c.Address = val;

                        var add = db.Corporate.Include(e => e.Address).Where(e => e.Id == data).SingleOrDefault();
                        IList<Corporate> addressdetails = null;
                        if (add.Address != null)
                        {
                            addressdetails = db.Corporate.Where(x => x.Address.Id == add.Address.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            addressdetails = db.Corporate.Where(x => x.Id == data).ToList();
                        }
                        if (addressdetails != null)
                        {
                            foreach (var s in addressdetails)
                            {
                                s.Address = c.Address;
                                db.Corporate.Attach(s);
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
                    var addressdetails = db.Corporate.Include(e => e.Address).Where(x => x.Id == data).ToList();
                    foreach (var s in addressdetails)
                    {
                        s.Address = null;
                        db.Corporate.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }

                if (ContactDetails != null)
                {
                    if (ContactDetails != "")
                    {
                        var val = db.ContactDetails.Find(int.Parse(ContactDetails));
                        c.ContactDetails = val;

                        var add = db.Corporate.Include(e => e.ContactDetails).Where(e => e.Id == data).SingleOrDefault();
                        IList<Corporate> contactsdetails = null;
                        if (add.ContactDetails != null)
                        {
                            contactsdetails = db.Corporate.Where(x => x.ContactDetails.Id == add.ContactDetails.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            contactsdetails = db.Corporate.Where(x => x.Id == data).ToList();
                        }
                        foreach (var s in contactsdetails)
                        {
                            s.ContactDetails = c.ContactDetails;
                            db.Corporate.Attach(s);
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
                    var contactsdetails = db.Corporate.Include(e => e.ContactDetails).Where(x => x.Id == data).ToList();
                    foreach (var s in contactsdetails)
                    {
                        s.ContactDetails = null;
                        db.Corporate.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }

                var CurCorp = db.Corporate.Find(data);
                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    c.DBTrack = dbT;
                    Corporate corp = new Corporate()
                    {
                        Code = c.Code,
                        Name = c.Name,
                        Id = data,
                        DBTrack = c.DBTrack
                    };


                    db.Corporate.Attach(corp);
                    db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                    return 1;
                }
                return 0;
            }
        }

        [HttpPost]
        public ActionResult Create(TimingPolicyBatchAssignment c, FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string TimingweeklySchedulelist = form["TimingweeklySchedulelist"] == "0" ? "" : form["TimingweeklySchedulelist"];
                string TimingGrouplist = form["TimingGrouplist"] == "0" ? "" : form["TimingGrouplist"];
                string TimingGrouplistR = form["TimingGrouplistR"] == "0" ? "" : form["TimingGrouplistR"];

                var IsWeeklyTimingSchedule = form["IsWeeklyTimingSchedule"] == "0" ? "" : form["IsWeeklyTimingSchedule"];
                var IsTimingGroup = form["IsTimingGroup"] == "0" ? "" : form["IsTimingGroup"];
                var IsRoaster = form["IsRoaster"] == "0" ? "" : form["IsRoaster"];

                c.IsWeeklyTimingSchedule = Convert.ToBoolean(IsWeeklyTimingSchedule);
                c.IsTimingGroup = Convert.ToBoolean(IsTimingGroup);
                c.IsRoaster = Convert.ToBoolean(IsRoaster);
                if (c.IsWeeklyTimingSchedule)
                {
                    IsTimingGroup = null;
                }
                if (c.IsTimingGroup)
                {
                    IsWeeklyTimingSchedule = null;
                }
                if (TimingweeklySchedulelist != null)
                {
                    if (TimingweeklySchedulelist != "")
                    {
                        var ids = Utility.StringIdsToListIds(TimingweeklySchedulelist);
                        List<TimingWeeklySchedule> _TimingWeeklySchedule = new List<TimingWeeklySchedule>();
                        foreach (var item in ids)
                        {
                            var val = db.TimingWeeklySchedule.Where(e => e.Id == item).SingleOrDefault();
                            if (val != null)
                            {
                                _TimingWeeklySchedule.Add(val);
                            }
                        }
                        c.TimingweeklySchedule = _TimingWeeklySchedule;
                    }
                }
                if (TimingGrouplist != null)
                {
                    if (TimingGrouplist != "")
                    {
                        int ContId = Convert.ToInt32(TimingGrouplist);
                        var val = db.TimingGroup.Where(e => e.Id == ContId).SingleOrDefault();
                        c.TimingGroup = val;
                    }
                }
                if (TimingGrouplistR != null)
                {
                    if (TimingGrouplistR != "")
                    {
                        int ContId = Convert.ToInt32(TimingGrouplistR);
                        var val = db.TimingGroup.Where(e => e.Id == ContId).SingleOrDefault();
                        c.TimingGroup = val;
                    }
                }
                if (ModelState.IsValid)
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        try
                        {
                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            TimingPolicyBatchAssignment tro = new TimingPolicyBatchAssignment

                            {
                                PolicyBatchName = c.PolicyBatchName,
                                TimingGroup = c.TimingGroup,
                                IsRoaster = c.IsRoaster,
                                IsTimingGroup = c.IsTimingGroup,// added 17/10/20.IsRoaster,
                                IsWeeklyTimingSchedule = c.IsWeeklyTimingSchedule,
                                TimingweeklySchedule = c.TimingweeklySchedule,
                                DBTrack = c.DBTrack
                            };

                            db.TimingPolicyBatchAssignment.Add(tro);
                            // var rtn_Obj = DBTrackFile.DBTrackSave("Attendance/Attendance", null, db.ChangeTracker, c.DBTrack);
                            //  DT_TimingPolicyBatchAssignment DT_Corp = (DT_TimingPolicyBatchAssignment)rtn_Obj;
                            //  db.Create(DT_Corp);
                            db.SaveChanges();
                            ts.Complete();
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
        }

        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.TimingPolicyBatchAssignment
                    .Include(e => e.TimingGroup)
                    .Include(e => e.TimingweeklySchedule)
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        IsRoaster = e.IsRoaster,
                        PolicyBatchName = e.PolicyBatchName,
                        IsTimingGroup = e.IsTimingGroup,
                        IsWeeklyTimingSchedule = e.IsWeeklyTimingSchedule,
                        TimingGroup_Id = e.TimingGroup.Id == null ? 0 : e.TimingGroup.Id,
                        Action = e.DBTrack.Action
                    }).ToList();

                var add_data = db.TimingPolicyBatchAssignment
                    .Include(e => e.TimingGroup)
                    .Include(e => e.TimingweeklySchedule)
                    .Where(e => e.Id == data).ToList();

                var r = (from ca in add_data
                         select new
                         {
                             TimingGroup_Id = ca.TimingGroup == null ? 0 : ca.TimingGroup.Id,
                             TimingGroup_val = ca.TimingGroup == null ? null : ca.TimingGroup.FullDetails,
                             TimingweeklySchedule_id = ca.TimingweeklySchedule.Select(x => x.Id.ToString()).ToArray(),
                             TimingweeklySchedule_val = ca.TimingweeklySchedule.Select(x => x.FullDetails).ToArray()
                         }).ToList();


                //var tw=add_data;

                //    .Select(e => new
                //    {
                //        TimingGroup_Id = e.TimingGroup.Id == null ? 0 : e.TimingGroup.Id,
                //        TimingGroup_val = e.TimingGroup.FullDetails == null ? null : e.TimingGroup.FullDetails,
                //        TimingweeklySchedule_id = e.TimingweeklySchedule.Select(x => e.Id.ToString()).ToArray(),
                //        TimingweeklySchedule_val = e.TimingweeklySchedule.Select(x => e.FullDetails).ToArray()

                //    }).ToList();


                var Corp = db.TimingPolicyBatchAssignment.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, r, "", Auth, JsonRequestBehavior.AllowGet });
            }
        }
        [HttpPost]
        public async Task<ActionResult> EditSave(TimingPolicyBatchAssignment L, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
                try
                {
                    string TimingweeklySchedulelist = form["TimingweeklySchedulelist"] == "0" ? "" : form["TimingweeklySchedulelist"];
                    //string timesche = form["TimingweeklySchedulelist"] == "0" ? "" : form["TimingweeklySchedulelist"];
                    string timegroup = form["TimingGrouplist"] == "0" ? "" : form["TimingGrouplist"];

                    var IsWeeklyTimingSchedule = form["IsWeeklyTimingSchedule"] == "0" ? "" : form["IsWeeklyTimingSchedule"];
                    var IsTimingGroup = form["IsTimingGroup"] == "0" ? "" : form["IsTimingGroup"];
                    var IsRoaster = form["IsRoaster"] == "0" ? "" : form["IsRoaster"];

                    L.IsWeeklyTimingSchedule = Convert.ToBoolean(IsWeeklyTimingSchedule);
                    L.IsTimingGroup = Convert.ToBoolean(IsTimingGroup);
                    L.IsRoaster = Convert.ToBoolean(IsRoaster);

                    if (timegroup!=null && timegroup!="")
                    {
                        L.TimingGroup_Id = int.Parse(timegroup);
                    }
                    else
                    {
                        L.TimingGroup_Id = null;
                    }
                  //  L.TimingGroup_Id = timegroup != null && timegroup != "" ? int.Parse(timegroup) : 0;

                    if (TimingweeklySchedulelist != null)
                    {
                        if (TimingweeklySchedulelist != "")
                        {
                            var ids = Utility.StringIdsToListIds(TimingweeklySchedulelist);
                            List<TimingWeeklySchedule> _TimingWeeklySchedule = new List<TimingWeeklySchedule>();
                            foreach (var item in ids)
                            {
                                var val = db.TimingWeeklySchedule.Where(e => e.Id == item).SingleOrDefault();
                                if (val != null)
                                {
                                    _TimingWeeklySchedule.Add(val);
                                }
                            }
                            L.TimingweeklySchedule = _TimingWeeklySchedule;
                        }
                    }
                  
                    if (ModelState.IsValid)
                    { 
                        try
                        {
                            {
                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                     
                                    //var CurCorp = db.TimingPolicyBatchAssignment.Find(data);
                                    var CurCorp = db.TimingPolicyBatchAssignment.Include(e => e.TimingGroup).Include(e => e.TimingweeklySchedule).Where(e => e.Id == data).SingleOrDefault();
                                    //TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    //db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                    //if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    //{
                                        TimingPolicyBatchAssignment blog = blog = null;
                                        DbPropertyValues originalBlogValues = null;


                                        blog = db.TimingPolicyBatchAssignment.Where(e => e.Id == data).SingleOrDefault();
                                        originalBlogValues = db.Entry(blog).OriginalValues;

                                        L.DBTrack = new DBTrack
                                        {
                                            CreatedBy = CurCorp.DBTrack.CreatedBy == null ? null : CurCorp.DBTrack.CreatedBy,
                                            CreatedOn = CurCorp.DBTrack.CreatedOn == null ? null : CurCorp.DBTrack.CreatedOn,
                                            Action = "M",
                                            ModifiedBy = SessionManager.UserName,
                                            ModifiedOn = DateTime.Now
                                        };
                                        
                                            CurCorp.IsRoaster = L.IsRoaster;
                                            CurCorp.IsTimingGroup = L.IsTimingGroup;
                                            CurCorp.IsWeeklyTimingSchedule = L.IsWeeklyTimingSchedule;
                                            CurCorp.TimingGroup_Id = L.TimingGroup_Id;
                                            CurCorp.Id = data;
                                            CurCorp.PolicyBatchName = L.PolicyBatchName;
                                            CurCorp.DBTrack = L.DBTrack;
                                            CurCorp.TimingweeklySchedule = L.TimingweeklySchedule;

                                            db.Entry(CurCorp).State = System.Data.Entity.EntityState.Modified;
                                            var obj = DBTrackFile.DBTrackSave("Attendance/Attendance", originalBlogValues, db.ChangeTracker, L.DBTrack);
                                            DT_TimingPolicyBatchAssignment DT_OBJ = (DT_TimingPolicyBatchAssignment)obj;
                                            DT_OBJ.TimingGroup_Id = blog.TimingGroup == null ? 0 : blog.TimingGroup.Id;
                                            db.Create(DT_OBJ);
                                       
                                        db.SaveChanges();

                              
                                        ts.Complete();
                                        Msg.Add("  Record Updated");
                                        return Json(new Utility.JsonReturnClass { Id = CurCorp.Id, Val = CurCorp.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        // return Json(new Object[] { L.Id, L.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });

                                   // }
                                }
                            }
                        }
                        catch (DbUpdateConcurrencyException ex)
                        {
                            var entry = ex.Entries.Single();
                            var clientValues = (TimingPolicyBatchAssignment)entry.Entity;
                            var databaseEntry = entry.GetDatabaseValues();
                            if (databaseEntry == null)
                            {
                                Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                            }
                            else
                            {
                                var databaseValues = (TimingPolicyBatchAssignment)databaseEntry.ToObject();
                                L.RowVersion = databaseValues.RowVersion;

                            }
                        }
                        Msg.Add("Record modified by another user.So refresh it and try to save again.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        // return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });

                    }
                    return Json(new Object[] { "", "", "", JsonRequestBehavior.AllowGet });
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
        //TimingPolicyBatchAssignment
        public ActionResult GetTimingPolicyBatchAssignment(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //var fall = db.TimingPolicyBatchAssignment.ToList();
                //IEnumerable<TimingPolicyBatchAssignment> all;
                //if (!string.IsNullOrEmpty(data))
                //{
                //    all = db.TimingPolicyBatchAssignment.ToList().Where(d => d.FullDetails.Contains(data));

                //}
                //else
                //{
                var list1 = db.TimingPolicyBatchAssignment.ToList();
                //var list2 = fall.Except(list1);

                //  var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.LookupVal }).Distinct();
                var r = (from ca in list1
                         select new
                         {
                             srno = ca.Id,
                             lookupvalue =
                             "PolicyBatchName :" + ca.PolicyBatchName +
                             "IsRoaster :" + ca.IsRoaster +
                             "IsTimingGroup :" + ca.IsTimingGroup +
                             "IsWeeklyTimingSchedule :" + ca.IsWeeklyTimingSchedule

                         }).Distinct();
                //var result_1 = (from c in fall
                //                select new { c.Id, c.CorporateCode, c.CorporateName });
                return Json(r, JsonRequestBehavior.AllowGet);
                // }
                //var result = (from c in all
                //              select new { c.Id, c.FullDetails }).Distinct();
                //  return Json(result, JsonRequestBehavior.AllowGet);
            }
            // return View();
        }
        public ActionResult GetTimingGrpDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.TimingGroup.Include(e => e.TimingPolicy).ToList();
                IEnumerable<TimingGroup> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.TimingGroup.ToList().Where(d => d.GroupName.Contains(data));

                }
                else
                {
                    var list1 = db.TimingMonthlyRoaster.ToList().Select(e => e.TimingGroup);
                    var list2 = fall.Except(list1);

                    //  var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.LookupVal }).Distinct();
                    var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.GroupName }).Distinct();
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
        public ActionResult GetWeeklyschedulelk(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.TimingWeeklySchedule.ToList();
                IEnumerable<TimingWeeklySchedule> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.TimingWeeklySchedule.ToList().Where(d => d.Description.Contains(data));

                }
                else
                {
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.Description }).Distinct();

                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.Description }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            // return View();
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
                IEnumerable<TimingPolicyBatchAssignment> TimingPolicyBatchAssignment = null;
                if (gp.IsAutho == true)
                {
                    TimingPolicyBatchAssignment = db.TimingPolicyBatchAssignment.Include(e => e.TimingGroup).Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    TimingPolicyBatchAssignment = db.TimingPolicyBatchAssignment.Include(e => e.TimingGroup).AsNoTracking().ToList();
                }

                IEnumerable<TimingPolicyBatchAssignment> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = TimingPolicyBatchAssignment;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.PolicyBatchName.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                            || (e.IsWeeklyTimingSchedule .ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                            || (e.IsTimingGroup.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                            || (e.IsRoaster.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                             || (e.Id.ToString().Contains(gp.searchString))
                           ).Select(a => new { a.PolicyBatchName, a.IsWeeklyTimingSchedule, a.IsTimingGroup, a.IsRoaster, a.Id }).ToList();

                        //if (gp.searchField == "Id")
                        //    jsonData = IE.Select(a => new { a.Id,a.PolicyBatchName }).Where((e => (e.Id.ToString().Contains(gp.searchString)))).ToList();
                        //else if (gp.searchField == "PolicyBatchName")
                        //    jsonData = IE.Select(a => new { a.Id, a.PolicyBatchName }).Where((e => (e.PolicyBatchName.ToString().Contains(gp.searchString)))).ToList();
                        //else if (gp.searchField == "Name")
                        //    jsonData = IE.Select(a => new { a.Id}).Where((e => (e.Name.ToString().Contains(gp.searchString)))).ToList();

                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.PolicyBatchName, a.IsWeeklyTimingSchedule, a.IsTimingGroup, a.IsRoaster, a.Id }.ToList());
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = TimingPolicyBatchAssignment;
                    Func<TimingPolicyBatchAssignment, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c =>  gp.sidx == "PolicyBatchName" ? c.PolicyBatchName :
                                        gp.sidx == "IsWeeklyTimingSchedule" ? c.PolicyBatchName.ToString() :
                                        gp.sidx == "IsTimingGroup" ? c.PolicyBatchName.ToString() :
                                        gp.sidx == "IsRoaster" ? c.PolicyBatchName.ToString() :
                     "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.PolicyBatchName, Convert.ToString(a.IsWeeklyTimingSchedule == true ? "Yes" : "No"), Convert.ToString(a.IsTimingGroup == true ? "Yes" : "No"), Convert.ToString(a.IsRoaster == true ? "Yes" : "No"), a.Id }.ToList());
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] {  a.PolicyBatchName, Convert.ToString(a.IsWeeklyTimingSchedule == true ? "Yes" : "No"), Convert.ToString(a.IsTimingGroup == true ? "Yes" : "No"), Convert.ToString(a.IsRoaster == true ? "Yes" : "No"), a.Id }.ToList());
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.PolicyBatchName, Convert.ToString(a.IsWeeklyTimingSchedule == true ? "Yes" : "No"), Convert.ToString(a.IsTimingGroup == true ? "Yes" : "No"), Convert.ToString(a.IsRoaster == true ? "Yes" : "No"), a.Id }.ToList());
                    }
                    totalRecords = TimingPolicyBatchAssignment.Count();
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
        public async Task<ActionResult> Delete(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                TimingPolicyBatchAssignment c = db.TimingPolicyBatchAssignment
                                           .Include(e => e.TimingGroup)
                                           .Where(e => e.Id == data).SingleOrDefault();

                TimingGroup tg = c.TimingGroup;
                // TimingWeeklySchedule timing = c.TimingweeklySchedule;
                //TimingweeklySchedule timing = c.TimingweeklySchedule;
                //  LookupValue val = corporates.BusinessType;
                //Corporate corporates = db.Corporate.Where(e => e.Id == data).SingleOrDefault();
                if (c.DBTrack.IsModified == true)
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack, corporates, null, "Corporate");
                        DBTrack dbT = new DBTrack
                        {
                            Action = "D",
                            CreatedBy = c.DBTrack.CreatedBy != null ? c.DBTrack.CreatedBy : null,
                            CreatedOn = c.DBTrack.CreatedOn != null ? c.DBTrack.CreatedOn : null,
                            IsModified = c.DBTrack.IsModified == true ? true : false
                        };
                        c.DBTrack = dbT;
                        db.Entry(c).State = System.Data.Entity.EntityState.Modified;
                        var rtn_Obj = DBTrackFile.DBTrackSave("Attendance/Attendance", null, db.ChangeTracker, c.DBTrack);
                        //  DT_TimingPolicyBatchAssignment DT_Corp = (DT_TimingPolicyBatchAssignment)rtn_Obj;
                        //  DT_Corp.TimingGroup_Id = c.TimingGroup == null ? 0 : c.TimingGroup.Id;
                        // DT_Corp.TimingweeklySchedule_Id = c.TimingweeklySchedule == null ? 0 : c.TimingweeklySchedule.Id;
                        //   DT_Corp.ContactDetails_Id = corporates.ContactDetails == null ? 0 : corporates.ContactDetails.Id;
                        // db.Create(DT_Corp);
                        // db.SaveChanges();
                        //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack);
                        await db.SaveChangesAsync();
                        //using (var context = new DataBaseContext())
                        //{
                        //   // DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack );
                        //}
                        ts.Complete();
                        return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                    }
                }
                else
                {
                    //var selectedRegions = c.Regions;

                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        //{
                        //    if (selectedRegions != null)
                        //    {
                        //        var corpRegion = new HashSet<int>(c.Regions.Select(e => e.Id));
                        //        if (corpRegion.Count > 0)
                        //        {
                        //            return Json(new Object[] { "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                        //            // return Json(new Object[] { "", "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                        //        }
                        //    }

                        try
                        {
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                // ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now,
                                CreatedBy = c.DBTrack.CreatedBy != null ? c.DBTrack.CreatedBy : null,
                                CreatedOn = c.DBTrack.CreatedOn != null ? c.DBTrack.CreatedOn : null,
                                IsModified = c.DBTrack.IsModified == true ? false : false//,
                                //AuthorizedBy = SessionManager.UserName,
                                //AuthorizedOn = DateTime.Now
                            };

                            // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };

                            db.Entry(c).State = System.Data.Entity.EntityState.Deleted;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Attendance/Attendance", null, db.ChangeTracker, dbT);
                            //   DT_TimingPolicyBatchAssignment DT_Corp = (DT_TimingPolicyBatchAssignment)rtn_Obj;
                            //  DT_Corp.TimingGroup_Id = c.TimingGroup == null ? 0 : c.TimingGroup.Id;
                            //  DT_Corp.TimingweeklySchedule_Id = c.TimingweeklySchedule == null ? 0 : c.TimingweeklySchedule.Id;
                            //   db.Create(DT_Corp);

                            await db.SaveChangesAsync();


                            //using (var context = new DataBaseContext())
                            //{
                            //    corporates.Address = add;
                            //    corporates.ContactDetails = conDet;
                            //    corporates.BusinessType = val;
                            //    DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dbT);
                            //    //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", dbT);
                            //}
                            ts.Complete();
                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                        catch (RetryLimitExceededException /* dex */)
                        {
                            Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                }
            }
        }
    }
}


