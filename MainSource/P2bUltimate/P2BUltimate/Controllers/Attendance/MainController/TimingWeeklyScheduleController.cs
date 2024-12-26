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
    public class TimingWeeklyScheduleController : Controller
    {
        public ActionResult Index()
        {
            return View("~/Views/Attendance/MainViews/TimingWeeklySchedule/Index.cshtml");
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
                IEnumerable<TimingWeeklySchedule> TimingWeeklySchedule = null;
                if (gp.IsAutho == true)
                {
                    TimingWeeklySchedule = db.TimingWeeklySchedule.Include(e => e.WeekDays).Include(e => e.WeeklyOffType).Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    TimingWeeklySchedule = db.TimingWeeklySchedule.Include(e => e.WeekDays).Include(e => e.WeeklyOffType).AsNoTracking().ToList();
                }

                IEnumerable<TimingWeeklySchedule> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = TimingWeeklySchedule;
                    if (gp.searchOper.Equals("eq"))
                        jsonData = IE.Where(e => (e.Description.ToUpper().Contains(gp.searchString.ToUpper()))
                          || (e.WeekDays != null ? e.WeekDays.LookupVal.ToUpper().Contains(gp.searchString.ToUpper()) : false)
                          || (e.WeeklyOffType != null ? e.WeeklyOffType.LookupVal.ToUpper().ToString().Contains(gp.searchString.ToUpper()) : false)
                          || (e.IsFixedWeeklyOff.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                          || (e.Id.ToString().Contains(gp.searchString))
                          ).Select(a => new {a.Description, a.WeekDays.LookupVal, a.WeeklyOffType, a.IsFixedWeeklyOff, a.Id }).ToList();
                    {
                        //jsonData = IE.Select(a => new { a.Id, a.Description }).Where((e => (e.Id.ToString() == gp.searchString) || (e.Description.ToLower() == gp.searchString.ToLower()))).ToList();
                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Description, a.WeekDays != null ? Convert.ToString(a.WeekDays.LookupVal) : "", a.WeeklyOffType != null ? Convert.ToString(a.WeeklyOffType.LookupVal) : "", a.IsFixedWeeklyOff, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = TimingWeeklySchedule;
                    Func<TimingWeeklySchedule, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                         orderfuc = (c =>  gp.sidx == "Description" ? c.Description :
                                        gp.sidx == "WeekDays" ? c.WeekDays.LookupVal.ToString() :
                                        gp.sidx == "WeeklyOffType" ? c.WeeklyOffType.LookupVal.ToString() :
                                        gp.sidx == "IsFixedWeeklyOff" ? c.IsFixedWeeklyOff.ToString() :
                     "");

                         

                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.Description), a.WeekDays != null ? Convert.ToString(a.WeekDays.LookupVal) : "", a.WeeklyOffType != null ? Convert.ToString(a.WeeklyOffType.LookupVal) : "", Convert.ToString(a.IsFixedWeeklyOff == true ? "Yes" : "No"), a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] {  Convert.ToString(a.Description), a.WeekDays != null ? Convert.ToString(a.WeekDays.LookupVal) : "", a.WeeklyOffType != null ? Convert.ToString(a.WeeklyOffType.LookupVal) : "", Convert.ToString(a.IsFixedWeeklyOff == true ? "Yes" : "No"), a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Description, a.WeekDays != null ? Convert.ToString(a.WeekDays.LookupVal) : "", a.WeeklyOffType != null ? Convert.ToString(a.WeeklyOffType.LookupVal) : "", Convert.ToString(a.IsFixedWeeklyOff == true ? "Yes" : "No"), a.Id }).ToList();
                    }
                    totalRecords = TimingWeeklySchedule.Count();
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
        public ActionResult Create(TimingWeeklySchedule c, FormCollection form) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    var WeekDays = form["WeekDayslist"] == "0" ? "" : form["WeekDayslist"];
                    var WeeklyOffType = form["WeeklyOffTypelist"] == "0" ? "" : form["WeeklyOffTypelist"];
                    var Addrs = form["TimingGroupList"] == "0" ? "" : form["TimingGroupList"];

                    if (db.TimingWeeklySchedule.Any(q => q.Description.Replace(" ", String.Empty) == c.Description.Replace(" ", String.Empty)))
                    {
                        Msg.Add("  Code Already Exists.  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    if (WeekDays != null && WeekDays != "" )
                    {

                        var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "200").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(WeekDays)).FirstOrDefault(); //db.LookupValue.Find(int.Parse(WeekDays));
                            c.WeekDays = val;
                         
                    }

                    if (WeeklyOffType != null && WeeklyOffType != "")
                    {

                        var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "612").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(WeeklyOffType)).FirstOrDefault(); //db.LookupValue.Find(int.Parse(WeeklyOffType));
                            c.WeeklyOffType = val; 
                    }

                    if (Addrs != null)
                    {
                        if (Addrs != "")
                        {
                            int AddId = Convert.ToInt32(Addrs);
                            var val = db.TimingGroup.Where(e => e.Id == AddId).SingleOrDefault();
                            c.TimingGroup = val;
                        }
                    }

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            //if (db.TimingWeeklySchedule.Any(o => o.Description == c.Description))
                            //{
                            //   // return Json(new Object[] { "", "", "Code Already Exists.", JsonRequestBehavior.AllowGet });
                            //Msg.Add("  Code Already Exists.  ");
                            // return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //}

                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            TimingWeeklySchedule WeeklySchedule = new TimingWeeklySchedule()
                            {
                                Description = c.Description,
                                WeekDays = c.WeekDays, //sarika
                                TimingGroup = c.TimingGroup,
                                Is7x24WeeklyOff = c.Is7x24WeeklyOff,
                                IsFixedWeeklyOff = c.IsFixedWeeklyOff,
                                WeeklyOffType = c.WeeklyOffType,
                                DBTrack = c.DBTrack
                            };
                            try
                            {
                                db.TimingWeeklySchedule.Add(WeeklySchedule);
                                //var rtn_Obj = DBTrackFile.DBTrackSave("Attendance/Attendance", null, db.ChangeTracker, c.DBTrack);
                                //DT_TimingWeeklySchedule DT_Corp = (DT_TimingWeeklySchedule)rtn_Obj;
                                //DT_Corp.TimingGroup_Id = c.TimingGroup == null ? 0 : c.TimingGroup.Id;
                                //DT_Corp.WeekDays_Id = c.WeekDays == null ? 0 : c.WeekDays.Id;
                                //DT_Corp.WeeklyOffType_Id = c.WeeklyOffType == null ? 0 : c.WeeklyOffType.Id;
                                //db.Create(DT_Corp);
                                db.SaveChanges();
                                //DBTrackFile.DBTrackSave("Core/P2b.Global", "C", corporate, null, "Corporate", null);


                                ts.Complete();
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                // return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = c.Id });
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
                        Msg.Add(errorMsg);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

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
                var Q = db.TimingWeeklySchedule
                    .Include(e => e.TimingGroup)
                    .Include(e => e.WeekDays)
                    .Include(e => e.WeeklyOffType)
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        Description = e.Description,
                        Is7x24WeeklyOff = e.Is7x24WeeklyOff,
                        IsFixedWeeklyOff = e.IsFixedWeeklyOff,
                        WeeklyOffType_Id = e.WeeklyOffType.Id == null ? 0 : e.WeeklyOffType.Id,
                        WeekDays_Id = e.WeekDays.Id == null ? 0 : e.WeekDays.Id,
                        Action = e.DBTrack.Action
                    }).ToList();

                var add_data = db.TimingWeeklySchedule
                  .Include(e => e.TimingGroup)
                    .Include(e => e.WeekDays)
                    .Include(e => e.WeeklyOffType)
                    .Where(e => e.Id == data)
                    .Select(e => new
                    {
                        TimingGroup_FullDetails = e.TimingGroup.FullDetails ,
                        TimingGroup_Id = e.TimingGroup.Id == null ? "" : e.TimingGroup.Id.ToString()

                    }).ToList();


                //var W = db.DT_TimingWeeklySchedule
                //     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                //     (e => new
                //     {
                //         DT_Id = e.Id,
                //         Description = e.Description == null ? "" : e.Description,
                //         Is7x24WeeklyOff = e.Is7x24WeeklyOff,
                //         IsFixedWeeklyOff = e.IsFixedWeeklyOff,
                //         WeeklyOffType = e.WeeklyOffType,
                //         WeekDays_Val = e.WeekDays_Id == 0 ? "" : db.LookupValue
                //                    .Where(x => x.Id == e.WeekDays_Id)
                //                    .Select(x => x.LookupVal).FirstOrDefault(),

                //         TimingGroup_Val = e.TimingGroup_Id == 0 ? "" : db.TimingGroup.Where(x => x.Id == e.TimingGroup_Id).Select(x => x.FullDetails).FirstOrDefault()

                //     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var Corp = db.TimingWeeklySchedule.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, add_data, "", Auth, JsonRequestBehavior.AllowGet });
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
        public async Task<ActionResult> EditSave(TimingWeeklySchedule c, int data, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    //  bool Auth = form["Autho_Action"] == "" ? false : true;
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;
                    string WeeklyOffType = form["WeeklyOffTypelist"] == "0" ? "" : form["WeeklyOffTypelist"];
                    string WeekDays = form["WeekDayslist"] == "0" ? "" : form["WeekDayslist"];
                    string TimingGroup = form["TimingGroupList"] == "0" ? "" : form["TimingGroupList"];
                   
                    if (WeeklyOffType!=null && WeeklyOffType!="")
                    {
                        c.WeeklyOffType_Id = int.Parse(WeeklyOffType);
                    }
                    else
                    {
                        c.WeeklyOffType_Id = null;
                    }
                    if (WeekDays != null && WeekDays != "")
                    {
                        c.WeekDays_Id = int.Parse(WeekDays);
                    }
                    else
                    {
                        c.WeekDays_Id = null;
                    }
                    if (TimingGroup != null && TimingGroup != "")
                    {
                        c.TimingGroup_Id = int.Parse(TimingGroup);
                    }
                    else
                    {
                        c.TimingGroup_Id = null;
                    }

                   // c.WeeklyOffType_Id = WeeklyOffType != null && WeeklyOffType != "" ? int.Parse(WeeklyOffType) : 0;
                   // c.WeekDays_Id =  WeekDays != null && WeekDays != "" ? int.Parse(WeekDays) :0;
                   // c.TimingGroup_Id = TimingGroup != null && TimingGroup != "" ? int.Parse(TimingGroup) : 0;

                  
                    if (Auth == false)
                    {
                        if (ModelState.IsValid)
                        {
                            try
                            {
                                //DbContextTransaction transaction = db.Database.BeginTransaction();
                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                   

                                 

                                    var CurCorp = db.TimingWeeklySchedule.Find(data);
                                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                   // db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {
                                        TimingWeeklySchedule blog = null; // to retrieve old data
                                        DbPropertyValues originalBlogValues = null;

                                        
                                            blog = db.TimingWeeklySchedule.Where(e => e.Id == data).Include(e => e.WeekDays).Include(e => e.WeeklyOffType)
                                                                    .Include(e => e.TimingGroup)
                                                                    .SingleOrDefault();
                                            originalBlogValues = db.Entry(blog).OriginalValues;
                                       

                                        c.DBTrack = new DBTrack
                                        {
                                            CreatedBy = CurCorp.DBTrack.CreatedBy == null ? null : CurCorp.DBTrack.CreatedBy,
                                            CreatedOn = CurCorp.DBTrack.CreatedOn == null ? null : CurCorp.DBTrack.CreatedOn,
                                            Action = "M",
                                            ModifiedBy = SessionManager.UserName,
                                            ModifiedOn = DateTime.Now
                                        };


                                        CurCorp.Description = c.Description == null ? "" : c.Description.Trim();
                                        CurCorp.WeekDays_Id = c.WeekDays_Id;
                                        CurCorp.TimingGroup_Id = c.TimingGroup_Id;
                                        CurCorp.Is7x24WeeklyOff = c.Is7x24WeeklyOff;
                                        CurCorp.IsFixedWeeklyOff = c.IsFixedWeeklyOff;
                                        CurCorp.WeeklyOffType_Id = c.WeeklyOffType_Id;
                                        CurCorp.Id = data;
                                        CurCorp.DBTrack = c.DBTrack;

                                        db.Entry(CurCorp).State = System.Data.Entity.EntityState.Modified;
                                      
                                    
 
                                        var obj = DBTrackFile.DBTrackSave("Attendance/Attendance", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                        DT_TimingWeeklySchedule DT_Corp = (DT_TimingWeeklySchedule)obj;
                                        DT_Corp.TimingGroup_Id = blog.TimingGroup == null ? 0 : blog.TimingGroup.Id;
                                        DT_Corp.WeekDays_Id = blog.WeekDays == null ? 0 : blog.WeekDays.Id;
                                        DT_Corp.WeeklyOffType_Id = blog.WeeklyOffType == null ? 0 : blog.WeeklyOffType.Id;
                                        db.Create(DT_Corp);
                                        db.SaveChanges();
                                    }
                                    ts.Complete();


                                    // return Json(new Object[] { c.Id, c.Description, "Record Updated", JsonRequestBehavior.AllowGet });
                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = c.Id, Val = c.Description, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (TimingWeeklySchedule)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    // return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                    Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);


                                }
                                else
                                {
                                    var databaseValues = (TimingWeeklySchedule)databaseEntry.ToObject();
                                    c.RowVersion = databaseValues.RowVersion;

                                }
                            }

                            //  return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                            Msg.Add("Record modified by another user.So refresh it and try to save again.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);


                        }
                    }
                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            TimingWeeklySchedule blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            TimingWeeklySchedule Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.TimingWeeklySchedule.Where(e => e.Id == data).SingleOrDefault();
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
                            TimingWeeklySchedule corp = new TimingWeeklySchedule()
                            {
                                Description = c.Description == null ? "" : c.Description.Trim(),
                                Is7x24WeeklyOff = c.Is7x24WeeklyOff,
                                IsFixedWeeklyOff = c.IsFixedWeeklyOff,
                                WeeklyOffType = c.WeeklyOffType,
                                Id = data,
                                DBTrack = c.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };


                            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Attendance/Attendance", "M", blog, corp, "TimingWeeklySchedule", c.DBTrack);
                                // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                Old_Corp = context.TimingWeeklySchedule.Where(e => e.Id == data).Include(e => e.WeekDays).Include(e => e.WeeklyOffType)
                                    .Include(e => e.TimingGroup).SingleOrDefault();
                                DT_TimingWeeklySchedule DT_Corp = (DT_TimingWeeklySchedule)obj;
                                DT_Corp.TimingGroup_Id = DBTrackFile.ValCompare(Old_Corp.TimingGroup, c.TimingGroup);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
                                DT_Corp.WeekDays_Id = DBTrackFile.ValCompare(Old_Corp.WeekDays, c.WeekDays); //Old_Corp.BusinessType == c.BusinessType ? 0 : Old_Corp.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_Corp.BusinessType.Id;
                                DT_Corp.WeeklyOffType_Id = DBTrackFile.ValCompare(Old_Corp.WeeklyOffType, c.WeeklyOffType);
                                db.Create(DT_Corp);
                                //db.SaveChanges();
                            }
                            blog.DBTrack = c.DBTrack;
                            db.TimingWeeklySchedule.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            //return Json(new Object[] { blog.Id, c.Description, "Record Updated", JsonRequestBehavior.AllowGet });
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = c.Description, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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

                            TimingWeeklySchedule corp = db.TimingWeeklySchedule.Include(e => e.TimingGroup)
                                                        .Include(e => e.WeekDays).FirstOrDefault(e => e.Id == auth_id);

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

                            db.TimingWeeklySchedule.Attach(corp);
                            db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            //db.SaveChanges();
                            var rtn_Obj = DBTrackFile.DBTrackSave("Attendance/Attendance", null, db.ChangeTracker, corp.DBTrack);
                            DT_TimingWeeklySchedule DT_Corp = (DT_TimingWeeklySchedule)rtn_Obj;
                            DT_Corp.TimingGroup_Id = corp.TimingGroup == null ? 0 : corp.TimingGroup.Id;
                            DT_Corp.WeekDays_Id = corp.WeekDays == null ? 0 : corp.WeekDays.Id;

                            db.Create(DT_Corp);
                            await db.SaveChangesAsync();

                            ts.Complete();
                            //  return Json(new Object[] { corp.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                            Msg.Add(" Record Authorised ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }
                    }
                    else if (auth_action == "M")
                    {

                        TimingWeeklySchedule Old_Corp = db.TimingWeeklySchedule.Include(e => e.WeekDays)
                                                          .Include(e => e.TimingGroup)
                                                          .Where(e => e.Id == auth_id).SingleOrDefault();


                        DT_TimingWeeklySchedule Curr_Corp = db.DT_TimingWeeklySchedule
                                                    .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                                    .OrderByDescending(e => e.Id)
                                                    .FirstOrDefault();

                        if (Curr_Corp != null)
                        {
                            TimingWeeklySchedule corp = new TimingWeeklySchedule();

                            string Corp = Curr_Corp.WeekDays_Id == null ? null : Curr_Corp.WeekDays_Id.ToString();
                            string Addrs = Curr_Corp.TimingGroup_Id == null ? null : Curr_Corp.TimingGroup_Id.ToString();

                            corp.Description = Curr_Corp.Description == null ? Old_Corp.Description : Curr_Corp.Description;
                            corp.Is7x24WeeklyOff = Curr_Corp.Is7x24WeeklyOff;
                            corp.IsFixedWeeklyOff = Curr_Corp.IsFixedWeeklyOff;
                            // corp.WeeklyOffType = Curr_Corp.WeeklyOffType;
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

                                        //  int a = EditS(Corp, Addrs, auth_id, corp, corp.DBTrack);

                                        await db.SaveChangesAsync();

                                        ts.Complete();
                                        // return Json(new Object[] { corp.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                                        Msg.Add(" Record Authorised ");
                                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    }
                                }
                                catch (DbUpdateConcurrencyException ex)
                                {
                                    var entry = ex.Entries.Single();
                                    var clientValues = (TimingWeeklySchedule)entry.Entity;
                                    var databaseEntry = entry.GetDatabaseValues();
                                    if (databaseEntry == null)
                                    {
                                        //  return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                        Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);


                                    }
                                    else
                                    {
                                        var databaseValues = (TimingWeeklySchedule)databaseEntry.ToObject();
                                        corp.RowVersion = databaseValues.RowVersion;
                                    }
                                }
                                Msg.Add("Record modified by another user.So refresh it and try to save again.");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                //   return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                            }
                        }
                        else
                            //   return Json(new Object[] { "", "Data removed from history", JsonRequestBehavior.AllowGet });
                            Msg.Add("  Data removed  from history.  ");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }
                    else if (auth_action == "D")
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //Corporate corp = db.Corporate.Find(auth_id);
                            TimingWeeklySchedule corp = db.TimingWeeklySchedule.AsNoTracking().Include(e => e.TimingGroup)
                                                                        .Include(e => e.WeekDays)
                                                                       .FirstOrDefault(e => e.Id == auth_id);

                            TimingGroup add = corp.TimingGroup;

                            LookupValue val = corp.WeekDays;

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

                            db.TimingWeeklySchedule.Attach(corp);
                            db.Entry(corp).State = System.Data.Entity.EntityState.Deleted;


                            var rtn_Obj = DBTrackFile.DBTrackSave("Attendance/Attendance", null, db.ChangeTracker, corp.DBTrack);
                            DT_TimingWeeklySchedule DT_Corp = (DT_TimingWeeklySchedule)rtn_Obj;
                            DT_Corp.TimingGroup_Id = corp.TimingGroup == null ? 0 : corp.TimingGroup.Id;
                            DT_Corp.WeekDays_Id = corp.WeekDays == null ? 0 : corp.WeekDays.Id;

                            db.Create(DT_Corp);
                            await db.SaveChangesAsync();
                            db.Entry(corp).State = System.Data.Entity.EntityState.Detached;
                            ts.Complete();
                            // return Json(new Object[] { "", "Record Authorised", JsonRequestBehavior.AllowGet });
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


        //public int EditS(string WeekDays, string WeeklyOffType, string TimingGroup, int data, TimingWeeklySchedule c, DBTrack dbT)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        if (WeekDays != null && WeekDays != "")
        //        {
        //            var val = db.LookupValue.Find(int.Parse(WeekDays));
        //            c.WeekDays = val;

        //            var type = db.TimingWeeklySchedule.Include(e => e.WeekDays).Where(e => e.Id == data).SingleOrDefault();
        //            IList<TimingWeeklySchedule> typedetails = null;
        //            if (type.WeekDays != null)
        //            {
        //                typedetails = db.TimingWeeklySchedule.Where(x => x.WeekDays.Id == type.WeekDays.Id && x.Id == data).ToList();
        //            }
        //            else
        //            {
        //                typedetails = db.TimingWeeklySchedule.Where(x => x.Id == data).ToList();
        //            }
        //            //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
        //            foreach (var s in typedetails)
        //            {
        //                s.WeekDays = c.WeekDays;
        //                db.TimingWeeklySchedule.Attach(s);
        //                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                //await db.SaveChangesAsync();
        //                db.SaveChanges();//sarika
        //                TempData["RowVersion"] = s.RowVersion;
        //                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //            }
        //        }
        //        else
        //        {
        //            var BusiTypeDetails = db.TimingWeeklySchedule.Include(e => e.WeekDays).Where(x => x.Id == data).ToList();
        //            foreach (var s in BusiTypeDetails)
        //            {
        //                s.WeekDays = null;
        //                db.TimingWeeklySchedule.Attach(s);
        //                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                //await db.SaveChangesAsync();
        //                db.SaveChanges();
        //                TempData["RowVersion"] = s.RowVersion;
        //                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //            }
        //        }

        //        if (WeeklyOffType != null && WeeklyOffType != "")
        //        {
        //            var val = db.LookupValue.Find(int.Parse(WeeklyOffType));
        //            c.WeeklyOffType = val;

        //            var type = db.TimingWeeklySchedule.Include(e => e.WeeklyOffType).Where(e => e.Id == data).SingleOrDefault();
        //            IList<TimingWeeklySchedule> typedetails = null;
        //            if (type.WeeklyOffType != null)
        //            {
        //                typedetails = db.TimingWeeklySchedule.Where(x => x.WeeklyOffType.Id == type.WeeklyOffType.Id && x.Id == data).ToList();
        //            }
        //            else
        //            {
        //                typedetails = db.TimingWeeklySchedule.Where(x => x.Id == data).ToList();
        //            }
        //            //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
        //            foreach (var s in typedetails)
        //            {
        //                s.WeeklyOffType = c.WeeklyOffType;
        //                db.TimingWeeklySchedule.Attach(s);
        //                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                //await db.SaveChangesAsync();
        //                db.SaveChanges();//sarika
        //                TempData["RowVersion"] = s.RowVersion;
        //                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //            }
        //        }
        //        else
        //        {
        //            var BusiTypeDetails = db.TimingWeeklySchedule.Include(e => e.WeeklyOffType).Where(x => x.Id == data).ToList();
        //            foreach (var s in BusiTypeDetails)
        //            {
        //                s.WeeklyOffType = null;
        //                db.TimingWeeklySchedule.Attach(s);
        //                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                //await db.SaveChangesAsync();
        //                db.SaveChanges();
        //                TempData["RowVersion"] = s.RowVersion;
        //                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //            }
        //        }

        //        if (TimingGroup != null)
        //        {
        //            if (TimingGroup != "")
        //            {
        //                var val = db.TimingGroup.Find(int.Parse(TimingGroup));
        //                c.TimingGroup = val;

        //                var add = db.TimingWeeklySchedule.Include(e => e.TimingGroup).Where(e => e.Id == data).SingleOrDefault();
        //                IList<TimingWeeklySchedule> addressdetails = null;
        //                if (add.TimingGroup != null)
        //                {
        //                    addressdetails = db.TimingWeeklySchedule.Where(x => x.TimingGroup.Id == add.TimingGroup.Id && x.Id == data).ToList();
        //                }
        //                else
        //                {
        //                    addressdetails = db.TimingWeeklySchedule.Where(x => x.Id == data).ToList();
        //                }
        //                if (addressdetails != null)
        //                {
        //                    foreach (var s in addressdetails)
        //                    {
        //                        s.TimingGroup = c.TimingGroup;
        //                        db.TimingWeeklySchedule.Attach(s);
        //                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                        // await db.SaveChangesAsync(false);
        //                        db.SaveChanges();
        //                        TempData["RowVersion"] = s.RowVersion;
        //                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                    }
        //                }
        //            }
        //        }
        //        else
        //        {
        //            var addressdetails = db.TimingWeeklySchedule.Include(e => e.TimingGroup).Where(x => x.Id == data).ToList();
        //            foreach (var s in addressdetails)
        //            {
        //                s.TimingGroup = null;
        //                db.TimingWeeklySchedule.Attach(s);
        //                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                //await db.SaveChangesAsync();
        //                db.SaveChanges();
        //                TempData["RowVersion"] = s.RowVersion;
        //                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //            }
        //        }



        //        var CurCorp = db.TimingWeeklySchedule.Find(data);
        //        TempData["CurrRowVersion"] = CurCorp.RowVersion;
        //        db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
        //        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
        //        {
        //            c.DBTrack = dbT;
        //            TimingWeeklySchedule corp = new TimingWeeklySchedule()
        //            {
        //                Description = c.Description == null ? "" : c.Description.Trim(),
        //                WeekDays = c.WeekDays,
        //                TimingGroup = c.TimingGroup,
        //                Is7x24WeeklyOff = c.Is7x24WeeklyOff,
        //                IsFixedWeeklyOff = c.IsFixedWeeklyOff,
        //                WeeklyOffType = c.WeeklyOffType,
        //                Id = data,
        //                DBTrack = c.DBTrack
        //            };


        //            db.TimingWeeklySchedule.Attach(corp);
        //            db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
        //            db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //            //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
        //            return 1;
        //        }
        //        return 0;
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

                    TimingWeeklySchedule corporates = db.TimingWeeklySchedule.Include(e => e.TimingGroup)
                                                       .Include(e => e.WeekDays).Where(e => e.Id == data).SingleOrDefault();

                    TimingGroup add = corporates.TimingGroup;
                    LookupValue val = corporates.WeekDays;
                    LookupValue val1 = corporates.WeeklyOffType;
                    //Corporate corporates = db.Corporate.Where(e => e.Id == data).SingleOrDefault();
                    if (corporates.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
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
                            DT_TimingWeeklySchedule DT_Corp = (DT_TimingWeeklySchedule)rtn_Obj;
                            DT_Corp.TimingGroup_Id = corporates.TimingGroup == null ? 0 : corporates.TimingGroup.Id;
                            DT_Corp.WeekDays_Id = corporates.WeekDays == null ? 0 : corporates.WeekDays.Id;
                            DT_Corp.WeeklyOffType_Id = corporates.WeeklyOffType == null ? 0 : corporates.WeeklyOffType.Id;
                            db.Create(DT_Corp);
                            // db.SaveChanges();
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack);
                            await db.SaveChangesAsync();
                            //using (var context = new DataBaseContext())
                            //{
                            //   // DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack );
                            //}
                            ts.Complete();
                            // return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                            Msg.Add("  Data removed .  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }
                    }
                    else
                    {
                        //var selectedRegions = corporates.TimingGroup;

                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //if (selectedRegions != null)
                            //{
                            //    var corpRegion = new HashSet<int>(corporates.TimingGroup.Select(e => e.Id));
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
                                    CreatedBy = corporates.DBTrack.CreatedBy != null ? corporates.DBTrack.CreatedBy : null,
                                    CreatedOn = corporates.DBTrack.CreatedOn != null ? corporates.DBTrack.CreatedOn : null,
                                    IsModified = corporates.DBTrack.IsModified == true ? false : false//,
                                    //AuthorizedBy = SessionManager.UserName,
                                    //AuthorizedOn = DateTime.Now
                                };

                                // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };

                                db.Entry(corporates).State = System.Data.Entity.EntityState.Deleted;
                                var rtn_Obj = DBTrackFile.DBTrackSave("Attendance/Attendance", null, db.ChangeTracker, dbT);
                                DT_TimingWeeklySchedule DT_Corp = (DT_TimingWeeklySchedule)rtn_Obj;
                                DT_Corp.TimingGroup_Id = add == null ? 0 : add.Id;
                                DT_Corp.WeekDays_Id = val == null ? 0 : val.Id;
                                DT_Corp.WeeklyOffType_Id = val1 == null ? 0 : val1.Id;

                                db.Create(DT_Corp);

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
                                //  return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                                Msg.Add("  Data removed .  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }
                            catch (RetryLimitExceededException /* dex */)
                            {
                                //Log the error (uncomment dex variable name and add a line here to write a log.)
                                //ModelState.AddModelError("", "Unable to delete. Try again, and if the problem persists, see your system administrator.");
                                //return RedirectToAction("Delete");
                                //  return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });

                                Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
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
