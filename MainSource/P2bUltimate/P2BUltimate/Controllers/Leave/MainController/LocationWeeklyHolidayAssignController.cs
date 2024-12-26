using System;
using System.Collections.Generic;
using System.Web;
using System.Linq;
using System.Web.Mvc;
using System.Net;
using System.Data.Entity;
using P2BUltimate.Models;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Transactions;
using P2b.Global;
using System.Text;
using P2BUltimate.App_Start;
using System.Threading.Tasks;
using P2BUltimate.Security;
using Payroll;
using Leave;


namespace P2BUltimate.Controllers.Leave.MainController
{
    public class LocationWeeklyHolidayAssignController : Controller
    {
        //
        // GET: /LocationWeeklyHolidayAssign/
        public ActionResult Index()
        {
            return View("~/Views/Leave/MainViews/LocationWeeklyHolidayAssign/Index.cshtml");
        }

        public class holicalendarlistdetails
        {
            public int HO_Id { get; set; }
            public string HO_FullDetails { get; set; }
        }

        public class WOcalendarlistdetails
        {
            public int WO_Id { get; set; }
            public string WO_val { get; set; }
        }

        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var db_data = db.Location
                        .Include(e => e.HolidayCalendar.Select(a => a.HoliCalendar))
                        .Include(e => e.HolidayCalendar.Select(a => a.HoliCalendar.Name))
                        .Include(e => e.WeeklyOffCalendar.Select(a => a.WOCalendar))
                        .Include(e => e.WeeklyOffCalendar.Select(a => a.WOCalendar.Name))
                         .Where(e => e.Id == data).SingleOrDefault();
                List<holicalendarlistdetails> returndata = new List<holicalendarlistdetails>();
                List<WOcalendarlistdetails> returndata1 = new List<WOcalendarlistdetails>();

                if (db_data != null)
                {

                   
                    foreach (var item in db_data.HolidayCalendar)
                    {
                        returndata.Add(new holicalendarlistdetails
                        {
                            HO_Id = item.Id,
                            HO_FullDetails = item.FullDetails
                        });
                    }
                    foreach (var item in db_data.WeeklyOffCalendar)
                    {

                        returndata1.Add(new WOcalendarlistdetails
                        {
                            WO_Id = item.Id,
                            WO_val = item.FullDetails
                        }); 
                    }
                }
                var Corp = db.Location.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { "", returndata, returndata1, Auth, JsonRequestBehavior.AllowGet });

            }
        }

        [HttpPost]
        public ActionResult EditSaveEdit(Location c,int data, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    
                   
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;
                    int ids = 0;
                    if (data != null && data != 0 )
                    {
                        ids = Convert.ToInt32(data);
                    }
                     

                    if (Auth == false)
                    {


                        if (ModelState.IsValid)
                        {
                            try
                            {
                                //DbContextTransaction transaction = db.Database.BeginTransaction();
                             
                                    var db_data = db.Location
                                        .Include(e => e.HolidayCalendar)
                                        .Include(e => e.LocationObj)
                                        .Where(e => e.Id == ids).SingleOrDefault();
                                    if (db_data.LocationObj != null)
                                    {
                                        c.LocationObj = db_data.LocationObj;
                                    }
                                    if (db_data.OpeningDate != null)
                                    {
                                        c.OpeningDate = db_data.OpeningDate;
                                    }
                                    List<HolidayCalendar> lookupLang = new List<HolidayCalendar>();
                                    string HolidayCalendar_DDL = form["HOCalendarList"] == "" ? null : form["HOCalendarList"];
                                    if (HolidayCalendar_DDL != null)
                                    {
                                        var ids1 = Utility.StringIdsToListIds(HolidayCalendar_DDL);
                                        foreach (var ca in ids1)
                                        {
                                            var Lookup_val = db.HolidayCalendar.Find(ca);

                                            lookupLang.Add(Lookup_val);
                                            db_data.HolidayCalendar = lookupLang;
                                        }
                                    }
                                    else
                                    {
                                        db_data.HolidayCalendar = null;
                                    }
                                    var dbdata = db.Location.Include(e => e.WeeklyOffCalendar).Where(e => e.Id == ids).SingleOrDefault();
                                    List<WeeklyOffCalendar> lookupLang1 = new List<WeeklyOffCalendar>();
                                    string WeeklyOffCalendar_DDL = form["WOCalendarList"] == "" ? null : form["WOCalendarList"];
                                    if (WeeklyOffCalendar_DDL != null)
                                    {
                                        var ids2 = Utility.StringIdsToListIds(WeeklyOffCalendar_DDL);
                                        foreach (var ca in ids2)
                                        {
                                            var Lookup_val = db.WeeklyOffCalendar.Find(ca);

                                            lookupLang1.Add(Lookup_val);
                                            dbdata.WeeklyOffCalendar = lookupLang1;
                                        }
                                    }
                                    else
                                    {
                                        dbdata.WeeklyOffCalendar = null;
                                    }
                                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                    {
                                        Location blog = null; // to retrieve old data
                                        DbPropertyValues originalBlogValues = null;

                                        using (var context = new DataBaseContext())
                                        {
                                            blog = context.Location.Where(e => e.Id == ids)
                                                                    .Include(e => e.Address).Include(e => e.LocationObj)
                                                                    .Include(e => e.ContactDetails).SingleOrDefault();
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
                                        //	db.Entry(c.LocationObj).State = System.Data.Entity.EntityState.Modified;
                                        // db.SaveChanges();

                                        //   int a = EditS(LocationObj, Incharge_DDL, HolidayCalendar_DDL, WeeklyOffCalendar_DDL, Addrs, ContactDetails, data, c, c.DBTrack);
                                        //  EditS(LocationObj,Incharge,  HolidayCalendar,  Weeklyoffcalendar, Addrs,  ContactDetails,);
                                        var m1 = db.Location.Include(e => e.HolidayCalendar).Where(e => e.Id == ids).ToList();
                                        foreach (var s in m1)
                                        {
                                            // s.AppraisalPeriodCalendar = null;
                                            db.Location.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                        var CurCorp = db.Location.Find(ids);
                                        TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                        db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                        {
                                            // c.DBTrack = dbT;
                                            Location corp = new Location()
                                            {
                                                // ContactDetails = c.ContactDetails,
                                                GeoFencingParameter_Id = db_data.GeoFencingParameter_Id,
                                                LocationObj_Id = db_data.LocationObj_Id,
                                                LocationObj = c.LocationObj,
                                                HolidayCalendar = db_data.HolidayCalendar,
                                                WeeklyOffCalendar = dbdata.WeeklyOffCalendar,
                                                OpeningDate = c.OpeningDate,
                                                Id = ids,
                                                DBTrack = c.DBTrack
                                            };


                                            db.Location.Attach(corp);
                                            db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                                            db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];

                                            // return 1;
                                            using (var context = new DataBaseContext())
                                            {
                                                

                                                var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                                DT_Location DT_Corp = (DT_Location)obj;
                                                DT_Corp.Address_Id = blog.Address == null ? 0 : blog.Address.Id;
                                                DT_Corp.ContactDetails_Id = blog.ContactDetails == null ? 0 : blog.ContactDetails.Id;
                                                db.Create(DT_Corp);
                                                db.SaveChanges();
                                            }
                                            // await db.SaveChangesAsync();
                                            ts.Complete();
                                        }

                                    }
                                 
                                Msg.Add("  Record Updated  ");
                                //  return Json(new Utility.JsonReturnClass { Id = corp.Id, Val = corp.LocationObj.LocDesc, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Object[] { c.Id, c.LocationObj.LocDesc, "Record Updated", JsonRequestBehavior.AllowGet });
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (Location)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (Location)databaseEntry.ToObject();
                                    c.RowVersion = databaseValues.RowVersion;

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
                            Msg.Add("Record modified by another user.So refresh it and try to save again.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            // return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
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
        public ActionResult EditSave(Location c, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    // string Addrs = form["AddressList"] == "0" ? "" : form["AddressList"];
                    // string ContactDetails = form["ContactDetailsList"] == "0" ? "" : form["ContactDetailsList"];
                    //  string OpeningDate = form["OpeningDate"] == "0" ? "" : form["OpeningDate"];
                    // string LocationObj = form["Location_Obj_DDL"] == "0" ? "" : form["Location_Obj_DDL"];

                    //string Incharge_DDL = form["Incharge_DDL"] == "0" ? "" : form["Incharge_DDL"];
                    // string Incharge_DDL = form["Incharge_Id"] == "0" ? "" : form["Incharge_Id"];
                    string Emp = form["Employee-Table"] == null ? null : form["Employee-Table"];
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;
                    List<int> ids = null;
                    if (Emp != null && Emp != "0" && Emp != "false")
                    {
                        ids = Utility.StringIdsToListIds(Emp);
                    }
                    else
                    {
                        Msg.Add("  Kindly Select Location ");
                        //  return Json(new Utility.JsonReturnClass { Id = corp.Id, Val = corp.LocationObj.LocDesc, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    if (Auth == false)
                    {


                        if (ModelState.IsValid)
                        {
                            try
                            {
                                //DbContextTransaction transaction = db.Database.BeginTransaction();
                                foreach (var item in ids)
                                {

                                    var db_data = db.Location.Include(e=>e.Address)
                                        .Include(e => e.HolidayCalendar)
                                        .Include(e => e.LocationObj)
                                        .Include(e => e.GeoFencingParameter)
                                        .Where(e => e.Id == item).SingleOrDefault();
                                    if (db_data.LocationObj != null)
                                    {
                                        c.LocationObj_Id = db_data.LocationObj_Id;
                                    }
                                    if (db_data.OpeningDate != null)
                                    {
                                        c.OpeningDate = db_data.OpeningDate;
                                    }
                                    if (db_data.Address != null)
                                    {
                                        c.Address_Id = db_data.Address_Id;
                                    }

                                    List<HolidayCalendar> lookupLang = new List<HolidayCalendar>();
                                    string HolidayCalendar_DDL = form["HOCalendarList"] == "" ? null : form["HOCalendarList"];
                                    if (HolidayCalendar_DDL != null)
                                    {
                                        var ids1 = Utility.StringIdsToListIds(HolidayCalendar_DDL);
                                        foreach (var ca in ids1)
                                        {
                                            var Lookup_val = db.HolidayCalendar.Find(ca);

                                            lookupLang.Add(Lookup_val);
                                            db_data.HolidayCalendar = lookupLang;
                                        }
                                    }
                                    else
                                    {
                                        db_data.HolidayCalendar = null;
                                    }
                                    var dbdata = db.Location.Include(e => e.WeeklyOffCalendar).Where(e => e.Id == item).SingleOrDefault();
                                    List<WeeklyOffCalendar> lookupLang1 = new List<WeeklyOffCalendar>();
                                    string WeeklyOffCalendar_DDL = form["WOCalendarList"] == "" ? null : form["WOCalendarList"];
                                    if (WeeklyOffCalendar_DDL != null)
                                    {
                                        var ids2 = Utility.StringIdsToListIds(WeeklyOffCalendar_DDL);
                                        foreach (var ca in ids2)
                                        {
                                            var Lookup_val = db.WeeklyOffCalendar.Find(ca);

                                            lookupLang1.Add(Lookup_val);
                                            dbdata.WeeklyOffCalendar = lookupLang1;
                                        }
                                    }
                                    else
                                    {
                                        dbdata.WeeklyOffCalendar = null;
                                    }
                                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                    {
                                        Location blog = null; // to retrieve old data
                                        DbPropertyValues originalBlogValues = null;

                                        using (var context = new DataBaseContext())
                                        {
                                            blog = context.Location.Where(e => e.Id == item)
                                                                    .Include(e => e.Address).Include(e => e.LocationObj)
                                                                    .Include(e => e.ContactDetails).SingleOrDefault();
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
                                        //	db.Entry(c.LocationObj).State = System.Data.Entity.EntityState.Modified;
                                        // db.SaveChanges();

                                        //   int a = EditS(LocationObj, Incharge_DDL, HolidayCalendar_DDL, WeeklyOffCalendar_DDL, Addrs, ContactDetails, data, c, c.DBTrack);
                                        //  EditS(LocationObj,Incharge,  HolidayCalendar,  Weeklyoffcalendar, Addrs,  ContactDetails,);
                                        var m1 = db.Location.Include(e => e.HolidayCalendar).Where(e => e.Id == item).ToList();
                                        foreach (var s in m1)
                                        {
                                            // s.AppraisalPeriodCalendar = null;
                                            db.Location.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                        var CurCorp = db.Location.Find(item);
                                        TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                        db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                        {
                                            // c.DBTrack = dbT;
                                            Location corp = new Location()
                                            {
                                                // ContactDetails = c.ContactDetails,
                                                GeoFencingParameter_Id = db_data.GeoFencingParameter_Id,
                                                LocationObj_Id = c.LocationObj_Id,
                                                Address_Id = db_data.Address_Id,
                                                HolidayCalendar = db_data.HolidayCalendar,
                                                WeeklyOffCalendar = dbdata.WeeklyOffCalendar,
                                                OpeningDate = c.OpeningDate,
                                                Id = item,
                                                DBTrack = c.DBTrack
                                            };


                                            db.Location.Attach(corp);
                                            db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                                            db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];

                                            // return 1;
                                            using (var context = new DataBaseContext())
                                            {
                                                //c.Id = data;

                                                /// DBTrackFile.DBTrackSave("Core/P2b.Global", "M", blog, c, "Location", c.DBTrack);
                                                //PropertyInfo[] fi = null;
                                                //Dictionary<string, object> rt = new Dictionary<string, object>();
                                                //fi = c.GetType().GetProperties();
                                                ////foreach (var Prop in fi)
                                                ////{
                                                ////    if (Prop.LocationObj.LocDesc != "Id" && Prop.LocationObj.LocDesc != "DBTrack" && Prop.LocationObj.LocDesc != "RowVersion")
                                                ////    {
                                                ////        rt.Add(Prop.LocationObj.LocDesc, Prop.GetValue(c));
                                                ////    }
                                                ////}
                                                //rt = blog.DetailedCompare(c);
                                                //rt.Add("Orig_Id", c.Id);
                                                //rt.Add("Action", "M");
                                                //rt.Add("DBTrack", c.DBTrack);
                                                //rt.Add("RowVersion", c.RowVersion);
                                                //var aa = DBTrackFile.GetInstance("Core/P2b.Global", "DT_" + "Location", rt);
                                                //DT_Location d = (DT_Location)aa;
                                                //db.DT_Location.Add(d);
                                                //db.SaveChanges();

                                                //To save data in history table 
                                                //var Obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, c, "Location", c.DBTrack);
                                                //DT_Location DT_Corp = (DT_Location)Obj;
                                                //db.DT_Location.Add(DT_Corp);
                                                //db.SaveChanges();\


                                                var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                                DT_Location DT_Corp = (DT_Location)obj;
                                                DT_Corp.Address_Id = blog.Address == null ? 0 : blog.Address.Id;
                                                DT_Corp.ContactDetails_Id = blog.ContactDetails == null ? 0 : blog.ContactDetails.Id;
                                                db.Create(DT_Corp);
                                                db.SaveChanges();
                                            }
                                            // await db.SaveChangesAsync();
                                            ts.Complete();
                                        }

                                    }
                                }
                                Msg.Add("  Record Updated  ");
                                //  return Json(new Utility.JsonReturnClass { Id = corp.Id, Val = corp.LocationObj.LocDesc, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Object[] { c.Id, c.LocationObj.LocDesc, "Record Updated", JsonRequestBehavior.AllowGet });
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (Location)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (Location)databaseEntry.ToObject();
                                    c.RowVersion = databaseValues.RowVersion;

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
                            Msg.Add("Record modified by another user.So refresh it and try to save again.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            // return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
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
        public class returndatagridclass
        {
            public string Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
            public string JoiningDate { get; set; }
            public string Job { get; set; }
            public string Grade { get; set; }
            public string Location { get; set; }
        }
        public ActionResult Emp_Grid(ParamModel param, string y)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {

                    var all = db.Location
                        .Include(e => e.LocationObj)
                        .ToList();
                    // for searchs
                    IEnumerable<Location> fall;

                    if (param.sSearch == null)
                    {
                        fall = all;

                    }
                    else
                    {
                        fall = all.ToList();
                    }

                    //for column sorting
                    var sortindex = Convert.ToInt32(Request["iSortCol_0"]);

                    Func<Location, string> orderfunc = (c =>
                                                                Convert.ToUInt32(sortindex) == 0 ? c.Id.ToString() :
                                                                sortindex == 1 ? c.LocationObj.LocCode : "");
                    var sortcolumn = Request["sSortDir_0"];
                    if (sortcolumn == "asc")
                    {
                        fall = fall.OrderBy(orderfunc);
                    }
                    else
                    {
                        fall = fall.OrderByDescending(orderfunc);
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
                                Code = item.LocationObj != null ? item.LocationObj.LocCode : "",
                                Name = item.FullDetails != null ? item.FullDetails : null,
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

                                     select new[] { null, Convert.ToString(c.Id), c.FullDetails, };
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
                    throw e;
                }
            }
        }
        public class LvNewReqChildDataClass
        {
            public int Id { get; set; }
            public string FullDetails { get; set; }

        }
        public ActionResult Get_HolidayWeeklyOffDetails(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var db_data = db.Location
                        .Include(e => e.HolidayCalendar.Select(a => a.HoliCalendar))
                        .Include(e => e.HolidayCalendar.Select(a => a.HoliCalendar.Name))
                        .Include(e => e.WeeklyOffCalendar.Select(a => a.WOCalendar))
                        .Include(e => e.WeeklyOffCalendar.Select(a => a.WOCalendar.Name))
                         .Where(e => e.Id == data).SingleOrDefault();


                    if (db_data != null)
                    {

                        List<LvNewReqChildDataClass> returndata = new List<LvNewReqChildDataClass>();
                        foreach (var item in db_data.WeeklyOffCalendar)
                        {

                            returndata.Add(new LvNewReqChildDataClass
                            {
                                Id = item.Id,
                                FullDetails = item.FullDetails
                            });
                            // }
                        }
                        foreach (var item in db_data.HolidayCalendar)
                        {

                            returndata.Add(new LvNewReqChildDataClass
                            {
                                Id = item.Id,
                                FullDetails = item.FullDetails
                            });
                            // }
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
            return null;
        }
        public ActionResult GridDelete(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var m1 = db.Location.Include(e => e.HolidayCalendar).Include(e => e.WeeklyOffCalendar).Where(e => e.Id == data).FirstOrDefault();

                db.Location.Attach(m1);
                db.Entry(m1).State = System.Data.Entity.EntityState.Modified; 
                    db.SaveChanges();
                    TempData["RowVersion"] = m1.RowVersion;
                    db.Entry(m1).State = System.Data.Entity.EntityState.Detached;
                
                List<string> Msgr = new List<string>();
                Msgr.Add("Record Deleted Successfully  ");
                return Json(new Utility.JsonReturnClass { success = true, responseText = Msgr }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}