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
using Payroll;
using EMS;
using Training;
using Attendance;
using Leave;
using P2BUltimate.Security;

namespace P2BUltimate.Controllers.Payroll
{
    public class NoticePeriodObjectController : Controller
    {
        //
        // GET: /NoicePeriodObject/
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/NoticePeriodObject/Index.cshtml");
        }
        public ActionResult Partial_NoticePeriodObject()
        {
            return View("~/Views/Shared/Payroll/_NoticePeriodObject.cshtml");
        }

        public ActionResult Partial_Servicerange()
        {
            return View("~/Views/Shared/Payroll/_Servicerange.cshtml");
        }

        public ActionResult Partial_Wages()
        {
            return View("~/Views/Shared/Payroll/_Wages.cshtml");
        }


        public ActionResult Create(NoticePeriod_Object c, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string ServiceRangeList = form["ServiceRangeList"] == "0" ? "" : form["ServiceRangeList"];
                    string WagesList = form["WagesList"] == "0" ? "" : form["WagesList"];
                    if (ServiceRangeList == null || ServiceRangeList == "")
                    {
                        Msg.Add(" Kindly select ServiceRange ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    if (WagesList == null || WagesList == "")
                    {
                        Msg.Add(" Kindly select Wages");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    List<ServiceRange> ObjServiceRange = new List<ServiceRange>();
                    if (ServiceRangeList != null && ServiceRangeList != "")
                    {
                        int servicerangeid = Convert.ToInt32(ServiceRangeList);
                        var value = db.ServiceRange.Find(servicerangeid);
                        c.ServiceRange = value;

                    }

                    List<Wages> ObjWages = new List<Wages>();
                    if (WagesList != null && WagesList != "")
                    {
                        int Wagesrangeid = Convert.ToInt32(WagesList);
                        var value = db.Wages.Find(Wagesrangeid);
                        c.Wages = value;
                    }
                    if (db.NoticePeriod_Object.Any(o => o.PolicyName == c.PolicyName))
                    {
                        //return this.Json(new { msg = "Code already exists." });
                        Msg.Add("  Record Already Exists.  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        // return this.Json(new Object[] { null, null, "ScaleName already exists.", JsonRequestBehavior.AllowGet });

                    }

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            NoticePeriod_Object NoticePeriod_Object = new NoticePeriod_Object()
                            {
                                PolicyName = c.PolicyName,
                                ServiceRange = c.ServiceRange,
                                Wages = c.Wages,
                                NoticePeriod = c.NoticePeriod,
                                DBTrack = c.DBTrack
                            };
                            try
                            {

                                //db.NoticePeriod_Object.Add(NoticePeriod_Object);
                                //var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, c.DBTrack);
                                //DT_NoticePeriod_Object DT_c = (DT_NoticePeriod_Object)rtn_Obj;
                                //db.Create(DT_c);
                                db.NoticePeriod_Object.Add(NoticePeriod_Object);
                                db.SaveChanges();
                                ts.Complete();
                                Msg.Add("  Data Created successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = NoticePeriod_Object.Id, Val = NoticePeriod_Object.PolicyName, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                //return this.Json(new Object[] { "", "", "Data Created Successfully.", JsonRequestBehavior.AllowGet });

                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("create", new { concurrencyError = true, id = c.Id });
                            }
                            catch (DataException /* dex */)
                            {
                                Msg.Add(" Unable to edit. Try again, and if the problem persists, see your system administrator");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // return this.Json(new { msg = "Unable to edit. Try again, and if the problem persists contact your system administrator." });
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

        public ActionResult Edit(int data)
        {
            //string tableName = "Corporate";

            //    // Fetch the table records dynamically
            //    var tableData = db.GetType()
            //    .GetProperty(tableName)
            //    .GetValue(db, null);
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.NoticePeriod_Object
                    .Include(e => e.ServiceRange)
                    .Include(e => e.Wages)
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        PolicyName = e.PolicyName,
                        NoticePeriod = e.NoticePeriod,
                        ServiceranngeId = e.ServiceRange != null ? e.ServiceRange.Id : 0,
                        ServicerangeFulldetails = e.ServiceRange != null ? e.ServiceRange.FullDetails : "",
                        wagesid = e.Wages != null ? e.Wages.Id : 0,
                        wagesfulldetails = e.Wages != null ? e.Wages.FullDetails : ""
                    }).ToList();


                var NoticePeriod_Object = db.NoticePeriod_Object.Find(data);
                TempData["RowVersion"] = NoticePeriod_Object.RowVersion;
                var Auth = NoticePeriod_Object.DBTrack.IsModified;
                return Json(new Object[] { Q, Auth, JsonRequestBehavior.AllowGet });
            }
        }
        public ActionResult GetNoticeperiodObjDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //List<string> Ids = SkipIds.ToString();
                var fall = db.NoticePeriod_Object.Include(e => e.ServiceRange)
                    .Include(e => e.Wages).ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.NoticePeriod_Object.Include(e => e.ServiceRange).Include(e => e.Wages).Where(e => e.Id != a).ToList();
                        else
                            fall = fall.Where(e => e.Id != a).ToList();
                    }
                }


                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = "Policy Name:" + ca.PolicyName + ", Notice Period:" + ca.NoticePeriod }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
            // return View();
        }


        public async Task<ActionResult> EditSave(NoticePeriod_Object c, int data, FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();

                //Calendar c = db.Calendar.Find(data);
                string ServiceRangeList = form["ServiceRangeList"] == "0" ? "" : form["ServiceRangeList"];
                string WagesList = form["WagesList"] == "0" ? "" : form["WagesList"];
                bool Auth = form["Autho_Allow"] == "true" ? true : false;


                if (ServiceRangeList != null)
                {
                    if (ServiceRangeList != "")
                    {
                        var val = db.ServiceRange.Find(int.Parse(ServiceRangeList));
                        c.ServiceRange = val;

                        var add = db.NoticePeriod_Object.Include(e => e.ServiceRange).Where(e => e.Id == data).SingleOrDefault();
                        IList<NoticePeriod_Object> addressdetails = null;
                        if (add.ServiceRange != null)
                        {
                            addressdetails = db.NoticePeriod_Object.Where(x => x.ServiceRange.Id == add.ServiceRange.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            addressdetails = db.NoticePeriod_Object.Where(x => x.Id == data).ToList();
                        }
                        if (addressdetails != null)
                        {
                            foreach (var s in addressdetails)
                            {
                                s.ServiceRange = c.ServiceRange;
                                db.NoticePeriod_Object.Attach(s);
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
                    var addressdetails = db.NoticePeriod_Object.Include(e => e.ServiceRange).Where(x => x.Id == data).ToList();
                    foreach (var s in addressdetails)
                    {
                        s.ServiceRange = null;
                        db.NoticePeriod_Object.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }
                if (WagesList != null)
                {
                    if (WagesList != "")
                    {
                        var val = db.Wages.Find(int.Parse(WagesList));
                        c.Wages = val;

                        var type = db.NoticePeriod_Object.Include(e => e.Wages).Where(e => e.Id == data).SingleOrDefault();
                        IList<NoticePeriod_Object> typedetails = null;
                        if (type.Wages != null)
                        {
                            typedetails = db.NoticePeriod_Object.Where(x => x.Wages.Id == type.Wages.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            typedetails = db.NoticePeriod_Object.Where(x => x.Id == data).ToList();
                        }
                        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                        foreach (var s in typedetails)
                        {
                            s.Wages = c.Wages;
                            db.NoticePeriod_Object.Attach(s);
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
                    var BusiTypeDetails = db.NoticePeriod_Object.Include(e => e.Wages).Where(x => x.Id == data).ToList();
                    foreach (var s in BusiTypeDetails)
                    {
                        s.Wages = null;
                        db.NoticePeriod_Object.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }
                //var alrdy = db.Calendar.Include(a => a.Name).Where(e => e.Name.LookupVal.ToString().ToUpper() == db_data.Name.LookupVal.ToString().ToUpper() && e.Default == true && data1.Default == true).Count();

                //if (alrdy > 0)
                //{
                //    Msg.Add("   Default  Year already exist. ");
                //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                //    // return this.Json(new Object[] { "", "", "From Date already exists.", JsonRequestBehavior.AllowGet });
                //}

                if (Auth == false)
                {


                    if (ModelState.IsValid)
                    {
                        try
                        {
                            //DbContextTransaction transaction = db.Database.BeginTransaction();

                            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {
                                NoticePeriod_Object blog = null; // to retrieve old data
                                DbPropertyValues originalBlogValues = null;

                                using (var context = new DataBaseContext())
                                {
                                    blog = context.NoticePeriod_Object.Where(e => e.Id == data)
                                                            .Include(e => e.ServiceRange).Include(e => e.Wages)
                                                           .SingleOrDefault();
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
                                var m1 = db.NoticePeriod_Object.Include(e => e.ServiceRange).Where(e => e.Id == data).ToList();
                                foreach (var s in m1)
                                {
                                    // s.AppraisalPeriodCalendar = null;
                                    db.NoticePeriod_Object.Attach(s);
                                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                    //await db.SaveChangesAsync();
                                    db.SaveChanges();
                                    TempData["RowVersion"] = s.RowVersion;
                                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                }
                                var CurCorp = db.NoticePeriod_Object.Find(data);
                                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                {
                                    // c.DBTrack = dbT;
                                    NoticePeriod_Object corp = new NoticePeriod_Object()
                                    {
                                        //LocationObj.LocCode = c.LocationObj.LocCode,
                                        //LocationObj.LocDesc = c.LocationObj.LocDesc,
                                        PolicyName = c.PolicyName,
                                        NoticePeriod = c.NoticePeriod,
                                        ServiceRange = c.ServiceRange,
                                        Wages = c.Wages,
                                        Id = data,
                                        DBTrack = c.DBTrack
                                    };


                                    db.NoticePeriod_Object.Attach(corp);
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


                                        //var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                        //DT_Location DT_Corp = (DT_Location)obj;
                                        //DT_Corp.Address_Id = blog.Address == null ? 0 : blog.Address.Id;
                                        //DT_Corp.ContactDetails_Id = blog.ContactDetails == null ? 0 : blog.ContactDetails.Id;
                                        //db.Create(DT_Corp);
                                        db.SaveChanges();
                                    }
                                    await db.SaveChangesAsync();
                                    ts.Complete();
                                    Msg.Add("  Record Updated  ");
                                    //  return Json(new Utility.JsonReturnClass { Id = corp.Id, Val = corp.LocationObj.LocDesc, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    return Json(new Utility.JsonReturnClass { Id = corp.Id, Val = corp.PolicyName, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }
                                //return Json(new Object[] { c.Id, c.LocationObj.LocDesc, "Record Updated", JsonRequestBehavior.AllowGet });

                            }
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
            }
            return null;
        }
        public ActionResult GetTA_TMRC_Eligibility_Wages(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.Wages.ToList();
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.Wages.Where(e => e.Id != a).ToList();
                        else
                            fall = fall.Where(e => e.Id != a).ToList();
                    }
                }
                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);

            }
        }

        public ActionResult GetServiceRange(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.ServiceRange.ToList();
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.ServiceRange.Where(e => e.Id != a).ToList();
                        else
                            fall = fall.Where(e => e.Id != a).ToList();
                    }
                }

                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);

            }
        }

        public ActionResult delete(string data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var id = Convert.ToInt32(data);
                    var qurey = db.NoticePeriod_Object.Include(e => e.ServiceRange).Where(e => e.Id == id).SingleOrDefault();
                    db.NoticePeriod_Object.Attach(qurey);
                    db.Entry(qurey).State = System.Data.Entity.EntityState.Deleted;
                    db.SaveChanges();
                    db.Entry(qurey).State = System.Data.Entity.EntityState.Detached;
                    Msg.Add("  Record removed successfully.  ");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
                Msg.Add("  Data removed successfully.  ");
                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                // return Json(new { msg = "Record Deleted", JsonRequestBehavior.AllowGet });
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
                IEnumerable<NoticePeriod_Object> ITSection10 = null;
                if (gp.IsAutho == true)
                {
                    ITSection10 = db.NoticePeriod_Object.Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    ITSection10 = db.NoticePeriod_Object.AsNoTracking().ToList();
                }

                IEnumerable<NoticePeriod_Object> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = ITSection10;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.PolicyName.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                                || (e.NoticePeriod.ToString().Contains(gp.searchString))
                                || (e.Id.ToString().Contains(gp.searchString))
                                ).Select(a => new Object[] { a.PolicyName, a.NoticePeriod, a.Id }).ToList();
                        //jsonData = IE.Select(a => new  { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.PolicyName, a.NoticePeriod, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = ITSection10;
                    Func<NoticePeriod_Object, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "PolicyName" ? c.PolicyName :
                                         gp.sidx == "NoticePeriod" ? c.NoticePeriod.ToString() : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.PolicyName, a.NoticePeriod, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.PolicyName, a.NoticePeriod, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.PolicyName, a.NoticePeriod, a.Id }).ToList();
                    }
                    totalRecords = ITSection10.Count();
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