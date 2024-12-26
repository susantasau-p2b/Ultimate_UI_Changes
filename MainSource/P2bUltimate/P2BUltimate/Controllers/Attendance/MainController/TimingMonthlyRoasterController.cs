///
/// Created by Sarika
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
using System.Web.Script.Serialization;
using System.Collections;
using System.Data.Entity.Core.Objects;
using System.Reflection;
using P2BUltimate.Security;

namespace P2BUltimate.Controllers.Attendance.MainController
{
    public class TimingMonthlyRoasterController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();
        //
        // GET: /TimingMonthlyRoaster/
        public ActionResult Index()
        {
            return View("~/Views/Attendance/MainViews/TimingMonthlyRoaster/Index.cshtml");
        }

        public ActionResult partial()
        {
            return View("~/Views/Shared/Attendance/_TimingMonthlyRoaster.cshtml");
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
                IEnumerable<TimingMonthlyRoaster> timingMonthlyRoaster = null;
                if (gp.IsAutho == true)
                {
                    timingMonthlyRoaster = db.TimingMonthlyRoaster.Include(e => e.TimingGroup).Where(e => e.DBTrack.IsModified == true && e.RoasterName != null).AsNoTracking().ToList();
                }
                else
                {
                    timingMonthlyRoaster = db.TimingMonthlyRoaster.Include(e => e.TimingGroup).Where(e=>e.RoasterName!=null).AsNoTracking().ToList();
                }

                IEnumerable<TimingMonthlyRoaster> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = timingMonthlyRoaster;
                    if (gp.searchOper.Equals("eq"))
                    {

                        jsonData = IE.Where(e => (e.RoasterName.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                             || (e.Id.ToString().Contains(gp.searchString.ToString()))
                           ).Select(a => new { a.RoasterName, a.Id }).ToList();

                        //jsonData = IE.Select(a => new { a.Id, a.RoasterName }).Where((e => (e.Id.ToString() == gp.searchString) || (e.RoasterName.ToLower() == gp.searchString.ToLower()))).ToList();

                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.RoasterName, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = timingMonthlyRoaster;
                    Func<TimingMonthlyRoaster, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "RoasterName" ? c.RoasterName : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] {  Convert.ToString(a.RoasterName), a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] {  Convert.ToString(a.RoasterName), a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] {  a.RoasterName, a.Id }).ToList();
                    }
                    totalRecords = timingMonthlyRoaster.Count();
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
                    var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
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

        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult Create(TimingMonthlyRoaster c, FormCollection form) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {

                    string tgrp = form["TimingGrouplist"] == "0" ? "" : form["TimingGrouplist"];

                    if (tgrp != null)
                    {
                        if (tgrp != "")
                        {
                            int AddId = Convert.ToInt32(tgrp);
                            var val = db.TimingGroup.Include(e => e.TimingPolicy)
                                                .Where(e => e.Id == AddId).SingleOrDefault();
                            c.TimingGroup = val;
                        }
                    }

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            if (db.TimingMonthlyRoaster.Any(o => o.RoasterName.Replace(" ", String.Empty) == c.RoasterName.Replace(" ", String.Empty)))
                            {
                                Msg.Add("  Code Already Exists.  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }

                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            TimingMonthlyRoaster timingMonthlyRoaster = new TimingMonthlyRoaster()
                            {

                                RoasterName = c.RoasterName == null ? "" : c.RoasterName.Trim(),
                                TimingGroup = c.TimingGroup,
                                DBTrack = c.DBTrack
                            };
                            try
                            {
                                db.TimingMonthlyRoaster.Add(timingMonthlyRoaster);
                                var rtn_Obj = DBTrackFile.DBTrackSave("Attendance/Attendance", null, db.ChangeTracker, c.DBTrack);
                                DT_TimingMonthlyRoaster DT_Corp = (DT_TimingMonthlyRoaster)rtn_Obj;
                                DT_Corp.TimingGroup_Id = c.TimingGroup == null ? 0 : c.TimingGroup.Id;
                                db.Create(DT_Corp);
                                db.SaveChanges();
                                //DBTrackFile.DBTrackSave("Core/P2b.Global", "C", corporate, null, "Corporate", null);


                                ts.Complete();
                                // return this.Json(new Object[] { timingMonthlyRoaster.Id, timingMonthlyRoaster.RoasterName, "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = timingMonthlyRoaster.Id, Val = timingMonthlyRoaster.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = c.Id });
                            }
                            catch (DataException /* dex */)
                            {
                                //   return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
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
                        return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
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

        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    TimingMonthlyRoaster corporates = db.TimingMonthlyRoaster.Include(e => e.TimingGroup)
                                                      .Where(e => e.Id == data).SingleOrDefault();

                    TimingGroup add = corporates.TimingGroup;

                    //Corporate corporates = db.Corporate.Where(e => e.Id == data).SingleOrDefault();
                    if (corporates.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            var v = db.EmpTimingMonthlyRoaster.Where(a => a.TimingMonthlyRoaster.Id == corporates.Id).ToList();
                            if (v != null)
                            {

                                if (v.Count > 0)
                                {
                                    Msg.Add(" Child record exists.Cannot remove it..  ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }
                            }

                            //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack, corporates, null, "Corporate");
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = corporates.DBTrack.CreatedBy != null ? corporates.DBTrack.CreatedBy : null,
                                CreatedOn = corporates.DBTrack.CreatedOn != null ? corporates.DBTrack.CreatedOn : null,
                                IsModified = corporates.DBTrack.IsModified == true ? true : false
                            };
                            corporates.DBTrack = dbT;
                            db.Entry(corporates).State = System.Data.Entity.EntityState.Modified;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Attendance/Attendance", null, db.ChangeTracker, corporates.DBTrack);
                            DT_TimingMonthlyRoaster DT_Corp = (DT_TimingMonthlyRoaster)rtn_Obj;
                            DT_Corp.TimingGroup_Id = corporates.TimingGroup == null ? 0 : corporates.TimingGroup.Id;

                            db.Create(DT_Corp);
                            // db.SaveChanges();
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack);
                            await db.SaveChangesAsync();
                            //using (var context = new DataBaseContext())
                            //{
                            //   // DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack );
                            //}
                            ts.Complete();
                            //  return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }
                    }
                    else
                    {
                        //var selectedRegions = corporates.Regions;

                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            //{
                            //    //if (selectedRegions != null)
                            //    //{
                            //    //    var corpRegion = new HashSet<int>(corporates.Regions.Select(e => e.Id));
                            //    //    if (corpRegion.Count > 0)
                            //    //    {
                            //    //        return Json(new Object[] { "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                            //    //        // return Json(new Object[] { "", "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                            //    //    }
                            //    }

                            try
                            {

                                var v = db.EmpTimingMonthlyRoaster.Where(a => a.TimingMonthlyRoaster.Id == corporates.Id).ToList();
                                if (v != null)
                                {

                                    if (v.Count > 0)
                                    {
                                        Msg.Add(" Child record exists.Cannot remove it..  ");
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    }
                                }



                                DBTrack dbT = new DBTrack
                                {
                                    Action = "D",
                                    ModifiedBy = SessionManager.UserName,
                                    ModifiedOn = DateTime.Now,
                                    CreatedBy = corporates.DBTrack.CreatedBy != null ? corporates.DBTrack.CreatedBy : null,
                                    CreatedOn = corporates.DBTrack.CreatedOn != null ? corporates.DBTrack.CreatedOn : null,
                                    IsModified = corporates.DBTrack.IsModified == true ? false : false//,
                                    //AuthorizedBy = SessionManager.UserName,
                                    //AuthorizedOn = DateTime.Now
                                };

                                // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };

                                db.Entry(corporates).State = System.Data.Entity.EntityState.Deleted;
                                var rtn_Obj = DBTrackFile.DBTrackSave("Attendance/Attendance", null, db.ChangeTracker, dbT);
                                DT_TimingMonthlyRoaster DT_Corp = (DT_TimingMonthlyRoaster)rtn_Obj;
                                DT_Corp.TimingGroup_Id = add == null ? 0 : add.Id;

                                db.Create(DT_Corp);

                                await db.SaveChangesAsync();



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
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.TimingMonthlyRoaster
                    .Include(e => e.TimingGroup)
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        RoasterName = e.RoasterName,
                        Action = e.DBTrack.Action
                    }).ToList();

                var add_data = db.TimingMonthlyRoaster
                  .Include(e => e.TimingGroup)
                      .Where(e => e.Id == data)
                    .Select(e => new
                    {
                        TimingGroup_FullAddress = e.TimingGroup.Id == null ? "" : e.TimingGroup.FullDetails,
                        TimingGroup_Id = e.TimingGroup.Id == null ? "" : e.TimingGroup.Id.ToString(),
                    }).ToList();


                var W = db.DT_TimingMonthlyRoaster
                     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id,
                         RoasterName = e.RoasterName == null ? "" : e.RoasterName,
                         TimingGroup_Val = e.TimingGroup_Id == 0 ? "" : db.TimingGroup.Where(x => x.Id == e.TimingGroup_Id).Select(x => x.FullDetails).FirstOrDefault()
                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var Corp = db.TimingMonthlyRoaster.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, add_data, W, Auth, JsonRequestBehavior.AllowGet });
            }
        }

        [HttpPost]
        public async Task<ActionResult> EditSave(TimingMonthlyRoaster ESOBJ, int data, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    string TimingGrouplist = form["TimingGrouplist"] == "0" ? "" : form["TimingGrouplist"];
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;


                    ESOBJ.TimingGroup_Id = TimingGrouplist != "" && TimingGrouplist != null ? int.Parse(TimingGrouplist) : 0;
                  
                    if (Auth == false)
                    {
                        if (ModelState.IsValid)
                        {
                            try
                            {
                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                { 
                                    var Curr_OBJ = db.TimingMonthlyRoaster.Find(data);
                                    TempData["CurrRowVersion"] = Curr_OBJ.RowVersion;
                                   
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {
                                        TimingMonthlyRoaster blog = blog = null;
                                        DbPropertyValues originalBlogValues = null;


                                        blog = db.TimingMonthlyRoaster.Where(e => e.Id == data).SingleOrDefault();
                                            originalBlogValues = db.Entry(blog).OriginalValues;
                                        

                                        ESOBJ.DBTrack = new DBTrack
                                        {
                                            CreatedBy = Curr_OBJ.DBTrack.CreatedBy == null ? null : Curr_OBJ.DBTrack.CreatedBy,
                                            CreatedOn = Curr_OBJ.DBTrack.CreatedOn == null ? null : Curr_OBJ.DBTrack.CreatedOn,
                                            Action = "M",
                                            ModifiedBy = SessionManager.UserName,
                                            ModifiedOn = DateTime.Now
                                        };

                                        Curr_OBJ.Id = data;
                                        Curr_OBJ.TimingGroup_Id = ESOBJ.TimingGroup_Id;
                                        Curr_OBJ.RoasterName = ESOBJ.RoasterName;
                                        Curr_OBJ.DBTrack = ESOBJ.DBTrack;


                                        db.Entry(Curr_OBJ).State = System.Data.Entity.EntityState.Modified;
                                    
                                        var obj = DBTrackFile.DBTrackSave("Attendance/Attendance", originalBlogValues, db.ChangeTracker, ESOBJ.DBTrack);
                                        DT_TimingMonthlyRoaster DT_OBJ = (DT_TimingMonthlyRoaster)obj;
                                        db.Create(DT_OBJ);
                                        db.SaveChanges();
                                     
                                        ts.Complete();
                                       
                                        Msg.Add("  Record Updated");
                                        return Json(new Utility.JsonReturnClass { Id = Curr_OBJ.Id, Val = Curr_OBJ.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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
                            var db_data = db.TimingMonthlyRoaster.Include(e => e.TimingGroup).Where(e => e.Id == data).SingleOrDefault();
                            List<TimingMonthlyRoaster> TimingPolicy = new List<TimingMonthlyRoaster>();
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
                            TimingMonthlyRoaster TimingGroup = new TimingMonthlyRoaster
                                        {
                                            Id = data,
                                            TimingGroup = ESOBJ.TimingGroup,
                                            RoasterName = ESOBJ.RoasterName,
                                            DBTrack = ESOBJ.DBTrack
                                        };
                            db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;
                            db.TimingMonthlyRoaster.Attach(TimingGroup);
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
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = ESOBJ.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    if (auth_action == "C")
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //Corporate corp = db.Corporate.Find(auth_id);
                            //Corporate corp = db.Corporate.AsNoTracking().FirstOrDefault(e => e.Id == auth_id);

                            TimingMonthlyRoaster corp = db.TimingMonthlyRoaster.Include(e => e.TimingGroup)
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

                            db.TimingMonthlyRoaster.Attach(corp);
                            db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            //db.SaveChanges();
                            var rtn_Obj = DBTrackFile.DBTrackSave("Attendance/Attendance", null, db.ChangeTracker, corp.DBTrack);
                            DT_TimingMonthlyRoaster DT_Corp = (DT_TimingMonthlyRoaster)rtn_Obj;
                            DT_Corp.TimingGroup_Id = corp.TimingGroup == null ? 0 : corp.TimingGroup.Id;

                            db.Create(DT_Corp);
                            await db.SaveChangesAsync();

                            ts.Complete();
                            // return Json(new Object[] { corp.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                            Msg.Add(" Record Authorised ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }
                    }
                    else if (auth_action == "M")
                    {

                        TimingMonthlyRoaster Old_Corp = db.TimingMonthlyRoaster
                                                          .Include(e => e.TimingGroup)
                                                          .Where(e => e.Id == auth_id).SingleOrDefault();



                        DT_TimingMonthlyRoaster Curr_Corp = db.DT_TimingMonthlyRoaster
                                                    .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                                    .OrderByDescending(e => e.Id)
                                                    .FirstOrDefault();

                        if (Curr_Corp != null)
                        {
                            TimingMonthlyRoaster corp = new TimingMonthlyRoaster();

                            string Addrs = Curr_Corp.TimingGroup_Id.ToString() == null ? null : Curr_Corp.TimingGroup_Id.ToString();
                            corp.RoasterName = Curr_Corp.RoasterName == null ? Old_Corp.RoasterName : Curr_Corp.RoasterName;

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

                                        int a = EditS(Addrs, auth_id, corp, corp.DBTrack);

                                        await db.SaveChangesAsync();

                                        ts.Complete();
                                        //  return Json(new Object[] { corp.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                                        Msg.Add(" Record Authorised ");
                                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    }
                                }
                                catch (DbUpdateConcurrencyException ex)
                                {
                                    var entry = ex.Entries.Single();
                                    var clientValues = (TimingMonthlyRoaster)entry.Entity;
                                    var databaseEntry = entry.GetDatabaseValues();
                                    if (databaseEntry == null)
                                    {
                                        //  return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                        Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);


                                    }
                                    else
                                    {
                                        var databaseValues = (TimingMonthlyRoaster)databaseEntry.ToObject();
                                        corp.RowVersion = databaseValues.RowVersion;
                                    }
                                }

                                // return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                                Msg.Add("Record modified by another user.So refresh it and try to save again.");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }
                        }
                        else
                        {
                            Msg.Add("  Data removed from history.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            // return Json(new Object[] { "", "Data removed from history", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else if (auth_action == "D")
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //Corporate corp = db.Corporate.Find(auth_id);
                            TimingMonthlyRoaster corp = db.TimingMonthlyRoaster.AsNoTracking().Include(e => e.TimingGroup)
                                                                        .FirstOrDefault(e => e.Id == auth_id);

                            TimingGroup add = corp.TimingGroup;


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

                            db.TimingMonthlyRoaster.Attach(corp);
                            db.Entry(corp).State = System.Data.Entity.EntityState.Deleted;


                            var rtn_Obj = DBTrackFile.DBTrackSave("Attendance/Attendance", null, db.ChangeTracker, corp.DBTrack);
                            DT_TimingMonthlyRoaster DT_Corp = (DT_TimingMonthlyRoaster)rtn_Obj;
                            DT_Corp.TimingGroup_Id = corp.TimingGroup == null ? 0 : corp.TimingGroup.Id;

                            db.Create(DT_Corp);
                            await db.SaveChangesAsync();
                            db.Entry(corp).State = System.Data.Entity.EntityState.Detached;
                            ts.Complete();
                            //  return Json(new Object[] { "", "Record Authorised", JsonRequestBehavior.AllowGet });
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

        [HttpPost]
        public int EditS(string Addrs, int data, TimingMonthlyRoaster c, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                if (Addrs != null)
                {
                    if (Addrs != "")
                    {
                        var val = db.TimingGroup.Find(int.Parse(Addrs));
                        c.TimingGroup = val;

                        var add = db.TimingMonthlyRoaster.Include(e => e.TimingGroup).Where(e => e.Id == data).SingleOrDefault();
                        IList<TimingMonthlyRoaster> addressdetails = null;
                        if (add.TimingGroup != null)
                        {
                            addressdetails = db.TimingMonthlyRoaster.Where(x => x.TimingGroup.Id == add.TimingGroup.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            addressdetails = db.TimingMonthlyRoaster.Where(x => x.Id == data).ToList();
                        }
                        if (addressdetails != null)
                        {
                            foreach (var s in addressdetails)
                            {
                                s.TimingGroup = c.TimingGroup;
                                db.TimingMonthlyRoaster.Attach(s);
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
                    var addressdetails = db.TimingMonthlyRoaster.Include(e => e.TimingGroup).Where(x => x.Id == data).ToList();
                    foreach (var s in addressdetails)
                    {
                        s.TimingGroup = null;
                        db.TimingMonthlyRoaster.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }


                var CurCorp = db.TimingMonthlyRoaster.Find(data);
                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    c.DBTrack = dbT;
                    TimingMonthlyRoaster corp = new TimingMonthlyRoaster()
                    {

                        RoasterName = c.RoasterName,
                        Id = data,
                        DBTrack = c.DBTrack
                    };


                    db.TimingMonthlyRoaster.Attach(corp);
                    db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                    return 1;
                }
                return 0;
            }
        }

        public void RollBack()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //  var context = DataContextFactory.GetDataContext();
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