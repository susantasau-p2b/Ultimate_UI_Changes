
using P2b.Global;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using P2BUltimate.Security;


namespace P2BUltimate.Controllers.Core.MainController
{
    public class WeeklyOffListController : Controller
    {
        // private DataBaseContext db = new DataBaseContext();
        public ActionResult Index()
        {
            return View("~/Views/Core/MainViews/WeeklyOffList/Index.cshtml");
        }
        public ActionResult partial()
        {
            return View("~/Views/Shared/Core/_WeeklyOffList.cshtml");
        }
        public ActionResult GetLookup(List<int> SkipIds, string Param)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                int WOCalendardropID = Convert.ToInt32(Param);
                var weeklycalendarid = db.WeeklyOffCalendar
                                     .Include(e => e.WOCalendar)
                                     .Include(e => e.WeeklyOffList)
                                     .Include(e => e.WeeklyOffList.Select(t => t.WeekDays))
                                     .Include(e => e.WeeklyOffList.Select(t => t.WeeklyOffStatus))
                                     .Where(e => e.WOCalendar.Id == WOCalendardropID).SingleOrDefault();

                //   var fall = db.WeeklyOffList.Include(e => e.WeekDays).Include(e => e.WeeklyOffStatus).ToList();
                if (weeklycalendarid != null)
                {

                    var fall = weeklycalendarid.WeeklyOffList.ToList();
                    if (SkipIds != null)
                    {
                        foreach (var a in SkipIds)
                        {
                            //if (fall == null)
                            //    fall = db.WeeklyOffList.Include(e => e.WeekDays).Include(e => e.WeeklyOffStatus).Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                            //else
                                fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                        }
                    }

                    //     var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.WeekDays.LookupVal + "," + ca.WeeklyOffStatus.LookupVal }).Distinct();
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                else
                {
                  var weeklyoffdata = db.WeeklyOffCalendar.SelectMany(t=>t.WeeklyOffList).ToList();
                 List<int> weeklyofflistid = weeklyoffdata.Select(e=>e.Id).ToList();
                 var fall = db.WeeklyOffList.Include(e => e.WeekDays).Include(e => e.WeeklyOffStatus).Where(e => !weeklyofflistid.Contains(e.Id)).ToList();
                 var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                 return Json(r, JsonRequestBehavior.AllowGet);
                }
                return null;
            }
        }

        public ActionResult ValidateForm(WeeklyOffList L, FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string WeekDays = form["WeekDayslist"] == "0" ? "" : form["WeekDayslist"];
                string WeeklyOffStatus = form["WeeklyOffStatuslist"] == "0" ? "" : form["WeeklyOffStatuslist"];
                if (WeekDays != null && WeekDays != "-Select-")
                {
                    var value = db.LookupValue.Find(int.Parse(WeekDays));
                    L.WeekDays = value;
                }
                if (WeeklyOffStatus != null && WeeklyOffStatus != "-Select-")
                {
                    var value = db.LookupValue.Find(int.Parse(WeeklyOffStatus));
                    L.WeeklyOffStatus = value;
                }
                //if (db.WeeklyOffList.Any(e => e.WeekDays.Id == L.WeekDays.Id && e.WeeklyOffStatus.Id == L.WeeklyOffStatus.Id))
                //{
                //    var Msg = new List<string>();
                //    Msg.Add("WeeklyOffList Already Exist");
                //    return Json(new { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                //}
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult Create(WeeklyOffList COBJ, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string WeekDays = form["WeekDayslist"] == "0" ? "" : form["WeekDayslist"];
                    string WeeklyOffStatus = form["WeeklyOffStatuslist"] == "0" ? "" : form["WeeklyOffStatuslist"];


                    if (WeekDays != null)
                    {
                        if (WeekDays != "")
                        {
                            var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "200").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(WeekDays)).FirstOrDefault();  //db.LookupValue.Find(int.Parse(WeekDays));
                            COBJ.WeekDays = val;
                        }
                    }

                    if (WeeklyOffStatus != null)
                    {
                        if (WeeklyOffStatus != "")
                        {
                            var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "201").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(WeeklyOffStatus)).FirstOrDefault(); //db.LookupValue.Find(int.Parse(WeeklyOffStatus));
                            COBJ.WeeklyOffStatus = val;
                        }
                    }


                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            //if (db.WeeklyOffList.Any(o => o.WeekDays.LookupVal.ToUpper() == COBJ.WeekDays.LookupVal.ToUpper() && o.WeeklyOffStatus.LookupVal.ToUpper() == COBJ.WeeklyOffStatus.LookupVal.ToUpper()))
                            //{
                            //    //return this.Json(new Object[] { "", "", "Already exist.", JsonRequestBehavior.AllowGet });
                            //    Msg.Add("  Code Already Exists.  ");
                            //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            //}

                            COBJ.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            //look.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName };

                            WeeklyOffList WeeklyOffList = new WeeklyOffList()
                            {
                                WeekDays = COBJ.WeekDays,
                                WeeklyOffStatus = COBJ.WeeklyOffStatus,
                                DBTrack = COBJ.DBTrack,

                            };
                            try
                            {
                                db.WeeklyOffList.Add(WeeklyOffList);
                                //var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, COBJ.DBTrack);
                                //DT_WeeklyOffList DT_OBJ = (DT_WeeklyOffList)rtn_Obj;
                                //DT_OBJ.WeeklyOffStatus_Id = COBJ.WeeklyOffStatus == null ? 0 : COBJ.WeeklyOffStatus.Id;
                                //DT_OBJ.WeekDays_Id = COBJ.WeekDays == null ? 0 : COBJ.WeekDays.Id;
                                //db.Create(DT_OBJ);
                                db.SaveChanges();
                                ts.Complete();
                                Msg.Add("  Data Created successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = WeeklyOffList.Id, Val = WeeklyOffList.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                // return this.Json(new Object[] { WeeklyOffList.Id, WeeklyOffList.FullDetails, "Data Created Successfully.", JsonRequestBehavior.AllowGet });
                            }

                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = COBJ.Id });
                            }
                            catch (DataException /* dex */)
                            {
                                Msg.Add("  Unable to create...  ");
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
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.WeeklyOffList.Include(e => e.WeekDays)
                       .Include(e => e.WeeklyOffStatus)
                        .Where(e => e.Id == data)
                        .Select(e => new
                        {
                            weekdays_Id = e.WeekDays != null ? e.WeekDays.Id : 0,
                            weekoffstatus = e.WeeklyOffStatus != null ? e.WeeklyOffStatus.Id : 0


                        }).ToList();
                var LKup = db.WeeklyOffList.Find(data);
                TempData["RowVersion"] = LKup.RowVersion;
                var Auth = LKup.DBTrack.IsModified;

                return this.Json(new Object[] { Q, "", "", Auth, JsonRequestBehavior.AllowGet });
            }
        }

        //public async Task<ActionResult> EditSave(WeeklyOffList frmWeeklyOffList, FormCollection form, int data)
        //{

        //    string WeekDaysdrop = form["WeekDaysdrop"] == "0" ? null : form["WeekDaysdrop"];

        //    if (WeekDaysdrop != null)
        //    {
        //        var val = db.LookupValue.Find(int.Parse(WeekDaysdrop));
        //        frmWeeklyOffList.WeekDays = val;
        //    }
        //    string WeeklyOffStatusdrop = form["WeeklyOffStatusdrop"] == "0" ? null : form["WeeklyOffStatusdrop"];

        //    if (WeeklyOffStatusdrop != null)
        //    {
        //        var val = db.LookupValue.Find(int.Parse(WeeklyOffStatusdrop));
        //        frmWeeklyOffList.WeeklyOffStatus = val;
        //    }
        //    if (ModelState.IsValid)
        //    {
        //        var db_data = db.WeeklyOffList
        //            .Include(e => e.WeekDays)
        //            .Include(e => e.WeeklyOffStatus)
        //            .Where(e => e.Id == data).SingleOrDefault();
        //        db_data.WeekDays = frmWeeklyOffList.WeekDays;
        //        db_data.WeeklyOffStatus = frmWeeklyOffList.WeeklyOffStatus;
        //        using (TransactionScope ts = new TransactionScope())
        //        {
        //            db.WeeklyOffList.Attach(db_data);
        //            db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
        //            db.SaveChanges();
        //            db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;
        //        }
        //        return Json(new Object[] { db_data.Id,db_data.Id, "Record Updated", JsonRequestBehavior.AllowGet });
        //    }
        //    else
        //    {
        //        StringBuilder sb = new StringBuilder("");
        //        foreach (ModelState modelState in ModelState.Values)
        //        {
        //            foreach (ModelError error in modelState.Errors)
        //            {
        //                sb.Append(error.ErrorMessage);
        //                sb.Append("." + "\n");
        //            }
        //        }
        //        var errorMsg = sb.ToString();
        //        Msg.Add(e.InnerException.Message.ToString());                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //    }

        //}


        [HttpPost]
        public async Task<ActionResult> EditSave(WeeklyOffList L, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string WeekDaysdrop = form["WeekDayslist"] == "0" ? null : form["WeekDayslist"];
                    string WeeklyOffStatusdrop = form["WeeklyOffStatuslist"] == "0" ? null : form["WeeklyOffStatuslist"];
                    if (WeekDaysdrop != null && WeekDaysdrop != "-Select" && WeekDaysdrop != "")
                    {
                        var value = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "200").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(WeekDaysdrop)).FirstOrDefault();  //db.LookupValue.Find(int.Parse(WeekDaysdrop));
                        L.WeekDays = value;

                    }

                    if (WeeklyOffStatusdrop != null && WeeklyOffStatusdrop != "" && WeeklyOffStatusdrop != "-Select")
                    {

                        var value = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "201").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(WeeklyOffStatusdrop)).FirstOrDefault();  //db.LookupValue.Find(int.Parse(WeeklyOffStatusdrop));
                        L.WeeklyOffStatus = value;

                    }

                    bool Auth = form["Autho_Allow"] == "true" ? true : false;
                    if (Auth == false)
                    {
                        if (ModelState.IsValid)
                        {
                            try
                            {

                                //DbContextTransaction transaction = db.Database.BeginTransaction();

                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    WeeklyOffList blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.WeeklyOffList.Where(e => e.Id == data).Include(e => e.WeeklyOffStatus)
                                                                .Include(e => e.WeekDays)
                                                                .SingleOrDefault();
                                        originalBlogValues = context.Entry(blog).OriginalValues;
                                    }

                                    L.DBTrack = new DBTrack
                                    {
                                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };

                                    if (WeekDaysdrop != null)
                                    {
                                        if (WeekDaysdrop != "")
                                        {
                                            var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "200").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(WeekDaysdrop)).FirstOrDefault();  //db.LookupValue.Find(int.Parse(WeekDaysdrop));
                                            L.WeekDays = val;

                                            var type = db.WeeklyOffList.Include(e => e.WeekDays).Where(e => e.Id == data).SingleOrDefault();
                                            IList<WeeklyOffList> typedetails = null;
                                            if (type.WeekDays != null)
                                            {
                                                typedetails = db.WeeklyOffList.Where(x => x.WeekDays.Id == type.WeekDays.Id && x.Id == data).ToList();
                                            }
                                            else
                                            {
                                                typedetails = db.WeeklyOffList.Where(x => x.Id == data).ToList();
                                            }
                                            //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                                            foreach (var s in typedetails)
                                            {
                                                s.WeekDays = L.WeekDays;
                                                db.WeeklyOffList.Attach(s);
                                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                                //await db.SaveChangesAsync();
                                                db.SaveChanges();
                                                TempData["RowVersion"] = s.RowVersion;
                                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                            }
                                        }
                                        else
                                        {
                                            var WFTypeDetails = db.WeeklyOffList.Include(e => e.WeekDays).Where(x => x.Id == data).ToList();
                                            foreach (var s in WFTypeDetails)
                                            {
                                                s.WeekDays = null;
                                                db.WeeklyOffList.Attach(s);
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
                                        var WeeklyOfflistDetails = db.WeeklyOffList.Include(e => e.WeekDays).Where(x => x.Id == data).ToList();
                                        foreach (var s in WeeklyOfflistDetails)
                                        {
                                            s.WeekDays = null;
                                            db.WeeklyOffList.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }
                                    if (WeeklyOffStatusdrop != null)
                                    {
                                        if (WeeklyOffStatusdrop != "")
                                        {
                                            var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "201").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(WeeklyOffStatusdrop)).FirstOrDefault();  //db.LookupValue.Find(int.Parse(WeeklyOffStatusdrop));
                                            L.WeeklyOffStatus = val;

                                            var type = db.WeeklyOffList.Include(e => e.WeeklyOffStatus).Where(e => e.Id == data).SingleOrDefault();
                                            IList<WeeklyOffList> typedetails = null;
                                            if (type.WeeklyOffStatus != null)
                                            {
                                                typedetails = db.WeeklyOffList.Where(x => x.WeeklyOffStatus.Id == type.WeeklyOffStatus.Id && x.Id == data).ToList();
                                            }
                                            else
                                            {
                                                typedetails = db.WeeklyOffList.Where(x => x.Id == data).ToList();
                                            }
                                            //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                                            foreach (var s in typedetails)
                                            {
                                                s.WeeklyOffStatus = L.WeeklyOffStatus;
                                                db.WeeklyOffList.Attach(s);
                                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                                //await db.SaveChangesAsync();
                                                db.SaveChanges();
                                                TempData["RowVersion"] = s.RowVersion;
                                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                            }
                                        }
                                        else
                                        {
                                            var WFTypeDetails = db.WeeklyOffList.Include(e => e.WeeklyOffStatus).Where(x => x.Id == data).ToList();
                                            foreach (var s in WFTypeDetails)
                                            {
                                                s.WeeklyOffStatus = null;
                                                db.WeeklyOffList.Attach(s);
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
                                        var WeeklyOfflistDetails = db.WeeklyOffList.Include(e => e.WeeklyOffStatus).Where(x => x.Id == data).ToList();
                                        foreach (var s in WeeklyOfflistDetails)
                                        {
                                            s.WeeklyOffStatus = null;
                                            db.WeeklyOffList.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }
                                    var CurCorp = db.WeeklyOffList.Find(data);
                                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {

                                        WeeklyOffList WeeklyOffList = new WeeklyOffList()
                                        {
                                            Id = data,
                                            DBTrack = L.DBTrack,
                                            WeekDays_Id=L.WeekDays.Id,
                                            WeeklyOffStatus_Id=L.WeeklyOffStatus.Id

                                        };
                                        db.WeeklyOffList.Attach(WeeklyOffList);
                                        db.Entry(WeeklyOffList).State = System.Data.Entity.EntityState.Modified;
                                        db.Entry(WeeklyOffList).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                    }
                                    // int a = EditS(CreditDatelist, ExcludeLeaveHeadslist, ConvertLeaveHeadBallist, ConvertLeaveHeadlist, data, L, L.DBTrack);
                                    using (var context = new DataBaseContext())
                                    {
                                        //var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, L.DBTrack);
                                        //dt_holiday DT_Corp = (DT_LvCreditPolicy)obj;
                                        //DT_Corp.CreditDate_Id = blog.CreditDate == null ? 0 : blog.CreditDate.Id;
                                        //db.Create(DT_Corp);
                                        db.SaveChanges();
                                    }
                                    await db.SaveChangesAsync();
                                    var dat = db.WeeklyOffList.Include(q => q.WeeklyOffStatus).Include(q => q.WeekDays).Where(e => e.Id == data).SingleOrDefault();
                                    ts.Complete();

                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = dat.Id, Val = dat.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    // return Json(new Object[] { L.Id, L.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });

                                }

                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (WeeklyOffList)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (WeeklyOffList)databaseEntry.ToObject();
                                    L.RowVersion = databaseValues.RowVersion;

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

                            WeeklyOffList blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            WeeklyOffList Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.WeeklyOffList.Where(e => e.Id == data).SingleOrDefault();
                                originalBlogValues = context.Entry(blog).OriginalValues;
                            }
                            L.DBTrack = new DBTrack
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

                            WeeklyOffList corp = new WeeklyOffList()
                            {
                                WeekDays = L.WeekDays,
                                WeeklyOffStatus = L.WeeklyOffStatus,
                                Id = data,
                                DBTrack = L.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };


                            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            using (var context = new DataBaseContext())
                            {
                                //var obj = DBTrackFile.ModifiedDataHistory("Leave/Leave", "M", blog, corp, "LvEncashReq", L.DBTrack);
                                //// var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                //Old_Corp = context.LvCreditPolicy.Where(e => e.Id == data).Include(e => e.ConvertLeaveHead)
                                //    .Include(e => e.ConvertLeaveHeadBal).Include(e => e.ExcludeLeaveHeads).Include(e => e.CreditDate).SingleOrDefault();
                                //DT_LvCreditPolicy DT_Corp = (DT_LvCreditPolicy)obj;
                                //DT_Corp.CreditDate_Id = blog.CreditDate == null ? 0 : blog.CreditDate.Id;

                                //db.Create(DT_Corp);
                                //db.SaveChanges();
                            }
                            blog.DBTrack = L.DBTrack;
                            db.WeeklyOffList.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = L.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            // return Json(new Object[] { blog.Id, L.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });
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
        public ActionResult Delete(int? data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                WeeklyOffList WeeklyOffList = db.WeeklyOffList.Where(e => e.Id == data).SingleOrDefault();
                try
                {
                    //var selectedValues = WeeklyOffList.SocialActivities;
                    //var lkValue = new HashSet<int>(WeeklyOffList.SocialActivities.Select(e => e.Id));
                    //if (lkValue.Count > 0)
                    //{
                    //    return this.Json(new Object[] { "", "Child record exists.Cannot delete.", JsonRequestBehavior.AllowGet });
                    //}

                    using (TransactionScope ts = new TransactionScope())
                    {
                        db.Entry(WeeklyOffList).State = System.Data.Entity.EntityState.Deleted;
                        db.SaveChanges();
                        ts.Complete();
                    }
                    Msg.Add("  Data removed successfully.  ");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    // return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                }

                catch (DataException /* dex */)
                {
                    //return this.Json(new Object[] { "", "Unable to delete. Try again, and if the problem persists contact your system administrator.", JsonRequestBehavior.AllowGet });
                    Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
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
                int ParentId = 2;
                var jsonData = (Object)null;
                var LKVal = db.WeeklyOffList.ToList();

                if (gp.IsAutho == true)
                {
                    LKVal = db.WeeklyOffList.Include(e => e.WeekDays).Include(e => e.WeeklyOffStatus).AsNoTracking().ToList();
                }
                else
                {
                    LKVal = db.WeeklyOffList.Include(e => e.WeekDays).Include(e => e.WeeklyOffStatus).AsNoTracking().ToList();
                }


                IEnumerable<WeeklyOffList> IE;
                if (!string.IsNullOrEmpty(gp.searchString))
                {
                    IE = LKVal;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Select(a => new { a.Id, a.WeekDays, a.WeeklyOffStatus }).Where((e => (e.Id.ToString() == gp.searchString) || (e.WeekDays.LookupVal.ToLower() == gp.searchString) || (e.WeeklyOffStatus.LookupVal.ToLower() == gp.searchString.ToLower())));
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.WeekDays.LookupVal, a.WeeklyOffStatus.LookupVal }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = LKVal;
                    Func<WeeklyOffList, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c =>
                                         gp.sidx == "WeekDays" ? c.WeekDays.LookupVal.ToString() :
                                         gp.sidx == "WeeklyOffStatus" ? c.WeeklyOffStatus.LookupVal.ToString() :

                                         "");
                    }

                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.WeekDays.LookupVal, a.WeeklyOffStatus.LookupVal }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.WeekDays.LookupVal, a.WeeklyOffStatus.LookupVal }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.WeekDays.LookupVal, a.WeeklyOffStatus.LookupVal }).ToList();
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
    }
}