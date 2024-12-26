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
using P2BUltimate.Security;
using System.Threading.Tasks;
using System.Text;

namespace P2BUltimate.Controllers.Payroll.MainController
{
    public class TravelEligibilityPolicyController : Controller
    {
        //
        // GET: /TravelEligibilityPolicy/
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/TravelEligibilityPolicy/Index.cshtml");
        }
        [HttpPost]
        public ActionResult GetWagesRange(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.WagesRange.ToList();
                IEnumerable<WagesRange> all;
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.WagesRange.Where(e => e.Id != a).ToList();
                        else
                            fall = fall.Where(e => e.Id != a).ToList();

                    }
                }

                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);


            }
        }

        public ActionResult GetTravelModeEligibilityPolicy(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.TravelModeEligibilityPolicy.Select(e => new
                {
                    Id = e.Id,
                    FullDetails = "TA_TM_Elligibilty_Code:" + e.TA_TM_Elligibilty_Code + "," + "ClassOfTravel:" + e.ClassOfTravel.LookupVal
                                   + "," + "TravelMode:" + e.TravelMode.LookupVal

                }).ToList();
                IEnumerable<TravelModeEligibilityPolicy> all;
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.TravelModeEligibilityPolicy.Where(e => e.Id != a).Select(e => new
                            {
                                Id = e.Id,
                                FullDetails = "TA_TM_Elligibilty_Code:" + e.TA_TM_Elligibilty_Code + "," + "ClassOfTravel:" + e.ClassOfTravel.LookupVal
                                               + "," + "TravelMode:" + e.TravelMode.LookupVal
                            }).ToList();
                        else
                            fall = fall.Where(e => e.Id != a).ToList();

                    }
                }

                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);


            }
        }
        public ActionResult partial()
        {
            return View("~/Views/Shared/Payroll/_TravelModeEligibilityPolicy.cshtml");
        }
        public ActionResult Create(TravelEligibilityPolicy c, FormCollection form) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string TA_Eligibility_Wages = form["PTWagesMasterlist"] == "0" ? "" : form["PTWagesMasterlist"];
                string WagesRange = form["WagesRangeList"] == "0" ? "" : form["WagesRangeList"];
                string travelModeEligibilityPolicyList = form["TravelModeEligibilityPolicyList"] == "0" ? "" : form["TravelModeEligibilityPolicyList"];
                List<String> Msg = new List<String>();
                try
                {
                    if (TA_Eligibility_Wages != null)
                    {
                        if (TA_Eligibility_Wages != "")
                        {
                            int wagesid = Convert.ToInt32(TA_Eligibility_Wages);
                            var val = db.Wages.Where(e => e.Id == wagesid).SingleOrDefault();
                            c.TA_Eligibility_Wages = val;
                        }
                    }

                    if (WagesRange != null)
                    {
                        var ids = Utility.StringIdsToListIds(WagesRange);
                        var WagesRangelist = new List<WagesRange>();
                        foreach (var item in ids)
                        {

                            int wagerangelistids = Convert.ToInt32(item);
                            var val = db.WagesRange.Find(wagerangelistids);
                            if (val != null)
                            {
                                WagesRangelist.Add(val);
                            }
                        }
                        c.WagesRange = WagesRangelist; 
                    }

                    if (travelModeEligibilityPolicyList != null)
                    {
                        var ids = Utility.StringIdsToListIds(travelModeEligibilityPolicyList);
                        var travelmodelist = new List<TravelModeEligibilityPolicy>();
                        foreach (var item in ids)
                        {

                            int travelmodelistids = Convert.ToInt32(item);
                            var val = db.TravelModeEligibilityPolicy.Include(e => e.ClassOfTravel).Include(e => e.TravelMode).Where(e => e.Id == travelmodelistids).SingleOrDefault();
                            if (val != null)
                            {
                                travelmodelist.Add(val);
                            }
                        }
                        c.TravelModeEligibilityPolicy = travelmodelist;
                    }
                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            // if (db.Corporate.Any(o => o.Code == c.Code))
                            //{
                            //    Msg.Add("Code Already Exists.");
                            //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //}

                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            TravelEligibilityPolicy traveleligibilitypolicy = new TravelEligibilityPolicy()
                            {
                                TA_TC_Eligibilty_Code = c.TA_TC_Eligibilty_Code,
                                TA_Eligibility_Wages = c.TA_Eligibility_Wages,
                                WagesRange = c.WagesRange,
                                TravelModeEligibilityPolicy = c.TravelModeEligibilityPolicy,
                                DBTrack = c.DBTrack
                            };

                            db.TravelEligibilityPolicy.Add(traveleligibilitypolicy);
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("Data Saved Successfully.");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                        //catch (DbUpdateConcurrencyException)
                        //{
                        //    return RedirectToAction("Create", new { concurrencyError = true, id = c.Id });
                        //}


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
                        Msg.Add("Code Already Exists.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
                        //return this.Json(new { msg = errorMsg });
                    }
                }
                catch (Exception e)
                {
                    Msg.Add(e.Message);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
            }

        }
        public class returnEditClass
        {
            public Array WagesRange_Id { get; set; }
            public Array WagesRangeFullDetails { get; set; }
            public Array wages_Id { get; set; }
            public Array WagesFullDetails { get; set; }
            public Array TravelModeEligibility_Id { get; set; }
            public Array TravelModeEligibilityFullDetails { get; set; }
        }
        [HttpPost]
        public ActionResult Edit(int data)
        {
            //string tableName = "Corporate";

            //    // Fetch the table records dynamically
            //    var tableData = db.GetType()
            //    .GetProperty(tableName)
            //    .GetValue(db, null);
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.TravelEligibilityPolicy
                    .Include(e => e.WagesRange)
                    .Include(e => e.TA_Eligibility_Wages)
                    .Include(e => e.TravelModeEligibilityPolicy)
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        Code = e.TA_TC_Eligibilty_Code,
                        Wages_id = e.TA_Eligibility_Wages == null ? 0 : e.TA_Eligibility_Wages.Id,
                        Wages_Fulldetails = e.TA_Eligibility_Wages == null ? "" : e.TA_Eligibility_Wages.FullDetails,
                        //WagesRange_Id = e.WagesRange == null ? 0 : e.WagesRange.Id,
                        //WagesRange_Fulldetails = e.WagesRange == null ? "" : e.WagesRange.FullDetails,
                    }).ToList();

                List<returnEditClass> oreturnEditClass = new List<returnEditClass>();

                var add_data = db.TravelEligibilityPolicy
                    .Include(e => e.TravelModeEligibilityPolicy)
                    .Include(e => e.TravelModeEligibilityPolicy.Select(t => t.ClassOfTravel))
                    .Include(e => e.TravelModeEligibilityPolicy.Select(t => t.TravelMode))
                    .Include(e => e.WagesRange) 
                    .Where(e => e.Id == data).SingleOrDefault();

                var fall = add_data.TravelModeEligibilityPolicy.Select(e => new
                {
                    Id = e.Id,
                    FullDetails = "TA_TM_Elligibilty_Code:" + e.TA_TM_Elligibilty_Code + "," + "ClassOfTravel:" + e.ClassOfTravel.LookupVal
                                   + "," + "TravelMode:" + e.TravelMode.LookupVal

                }).ToList();

                var fall1 = add_data.WagesRange.Select(e => new
                {
                    Id = e.Id,
                    FullDetails = e.FullDetails,

                }).ToList();

                oreturnEditClass.Add(new returnEditClass
                {
                    TravelModeEligibility_Id = fall.Select(a => a.Id.ToString()).ToArray(),
                    TravelModeEligibilityFullDetails = fall.Select(a => a.FullDetails).ToArray(),
                    WagesRange_Id = fall1.Select(a => a.Id).ToArray(),
                    WagesRangeFullDetails = fall1.Select(a => a.FullDetails).ToArray()
                });





                var Corp = db.TravelEligibilityPolicy.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, oreturnEditClass, Auth, JsonRequestBehavior.AllowGet });
            }
        }
        public async Task<ActionResult> EditSave(TravelEligibilityPolicy c, int data, FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();

                //Calendar c = db.Calendar.Find(data);
                string TaEligibilitywages = form["PTWagesMasterlist"] == "0" ? "" : form["PTWagesMasterlist"];
                string wagesrangelist = form["WagesRangeList"] == "0" ? "" : form["WagesRangeList"];
                string travelModeEligibilityPolicyList = form["TravelModeEligibilityPolicyList"] == "0" ? "" : form["TravelModeEligibilityPolicyList"];
                bool Auth = form["Autho_Allow"] == "true" ? true : false;

                //if (TaEligibilitywages == null)
                //{
                //    Msg.Add(" TA_Eligibility_Wages is required ");
                //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);   
                //}
                //if (wagesrangelist == null)
                //{
                //    Msg.Add(" WageRange is required ");
                //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);   
                //}
                //if (travelModeEligibilityPolicyList == null)
                //{
                //    Msg.Add(" TravelModeEligibilityPolicyList is required ");
                //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                //}
                if (TaEligibilitywages != null && TaEligibilitywages != "")
                {
                    c.TA_Eligibility_Wages_Id = Convert.ToInt32(TaEligibilitywages);
                }
                else
                {
                    c.TA_Eligibility_Wages_Id = null;
                }
                //if (wagesrangelist != null && wagesrangelist != "")
                //{
                //    c.WagesRange_Id = Convert.ToInt32(wagesrangelist);
                //}
                //else
                //{
                //    c.WagesRange_Id = null;
                //}
                if (TaEligibilitywages != null)
                {
                    if (TaEligibilitywages != "")
                    {
                        var val = db.Wages.Find(int.Parse(TaEligibilitywages));
                        c.TA_Eligibility_Wages = val;

                        var add = db.TravelEligibilityPolicy.Include(e => e.TA_Eligibility_Wages).Where(e => e.Id == data).SingleOrDefault();
                        IList<TravelEligibilityPolicy> addressdetails = null;
                        if (add.TA_Eligibility_Wages != null)
                        {
                            addressdetails = db.TravelEligibilityPolicy.Where(x => x.TA_Eligibility_Wages.Id == add.TA_Eligibility_Wages.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            addressdetails = db.TravelEligibilityPolicy.Where(x => x.Id == data).ToList();
                        }
                        if (addressdetails != null)
                        {
                            foreach (var s in addressdetails)
                            {
                                s.TA_Eligibility_Wages = c.TA_Eligibility_Wages;
                                db.TravelEligibilityPolicy.Attach(s);
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
                    var addressdetails = db.TravelEligibilityPolicy.Include(e => e.TA_Eligibility_Wages).Where(x => x.Id == data).ToList();
                    foreach (var s in addressdetails)
                    {
                        s.TA_Eligibility_Wages = null;
                        db.TravelEligibilityPolicy.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }
                //if (wagesrangelist != null)
                //{
                //    if (wagesrangelist != "")
                //    {
                //        var val = db.WagesRange.Find(int.Parse(wagesrangelist));
                //        c.WagesRange = val;

                //        var type = db.TravelEligibilityPolicy.Include(e => e.WagesRange).Where(e => e.Id == data).SingleOrDefault();
                //        IList<TravelEligibilityPolicy> typedetails = null;
                //        if (type.WagesRange != null)
                //        {
                //            typedetails = db.TravelEligibilityPolicy.Where(x => x.WagesRange.Id == type.WagesRange.Id && x.Id == data).ToList();
                //        }
                //        else
                //        {
                //            typedetails = db.TravelEligibilityPolicy.Where(x => x.Id == data).ToList();
                //        }
                //        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                //        foreach (var s in typedetails)
                //        {
                //            s.WagesRange = c.WagesRange;
                //            db.TravelEligibilityPolicy.Attach(s);
                //            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                //            //await db.SaveChangesAsync();
                //            db.SaveChanges();
                //            TempData["RowVersion"] = s.RowVersion;
                //            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                //        }
                //    }
                //    else
                //    {
                //        var BusiTypeDetails = db.TravelEligibilityPolicy.Include(e => e.WagesRange).Where(x => x.Id == data).ToList();
                //        foreach (var s in BusiTypeDetails)
                //        {
                //            s.WagesRange = null;
                //            db.TravelEligibilityPolicy.Attach(s);
                //            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                //            //await db.SaveChangesAsync();
                //            db.SaveChanges();
                //            TempData["RowVersion"] = s.RowVersion;
                //            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                //        }
                //    }
                //}
                //var alrdy = db.Calendar.Include(a => a.Name).Where(e => e.Name.LookupVal.ToString().ToUpper() == db_data.Name.LookupVal.ToString().ToUpper() && e.Default == true && data1.Default == true).Count();

                //if (alrdy > 0)
                //{
                //    Msg.Add("   Default  Year already exist. ");
                //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                //    // return this.Json(new Object[] { "", "", "From Date already exists.", JsonRequestBehavior.AllowGet });
                //}
                var db_data = db.TravelEligibilityPolicy.Include(e => e.TravelModeEligibilityPolicy).Include(e => e.WagesRange).Where(e => e.Id == data).SingleOrDefault();
                List<TravelModeEligibilityPolicy> lookupLang = new List<TravelModeEligibilityPolicy>();
                // string HolidayCalendar_DDL = form["HOCalendarList"] == "" ? null : form["HOCalendarList"];
                if (travelModeEligibilityPolicyList != null)
                {
                    var ids = Utility.StringIdsToListIds(travelModeEligibilityPolicyList);
                    foreach (var ca in ids)
                    {
                        var Lookup_val = db.TravelModeEligibilityPolicy.Find(ca);

                        lookupLang.Add(Lookup_val);
                        db_data.TravelModeEligibilityPolicy = lookupLang;
                    }
                }
                else
                {
                    db_data.TravelModeEligibilityPolicy = null;
                }

                 
                List<WagesRange> WagesRangeList= new List<WagesRange>();
                // string HolidayCalendar_DDL = form["HOCalendarList"] == "" ? null : form["HOCalendarList"];
                if (wagesrangelist != null)
                {
                    var ids = Utility.StringIdsToListIds(wagesrangelist);
                    foreach (var ca in ids)
                    {
                        var Lookup_val = db.WagesRange.Find(ca);

                        WagesRangeList.Add(Lookup_val);
                        db_data.WagesRange = WagesRangeList;
                    }
                }
                else
                {
                    db_data.WagesRange = null;
                }

                if (Auth == false)
                {


                    if (ModelState.IsValid)
                    {
                        try
                        {
                            //DbContextTransaction transaction = db.Database.BeginTransaction();

                            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {
                                TravelEligibilityPolicy blog = null; // to retrieve old data
                                DbPropertyValues originalBlogValues = null;

                                using (var context = new DataBaseContext())
                                {
                                    blog = context.TravelEligibilityPolicy.Where(e => e.Id == data)
                                                            .Include(e => e.TA_Eligibility_Wages).Include(e => e.WagesRange)
                                                            .Include(e => e.TravelModeEligibilityPolicy).SingleOrDefault();
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
                                var m1 = db.TravelEligibilityPolicy.Include(e => e.TravelModeEligibilityPolicy).Where(e => e.Id == data).ToList();
                                foreach (var s in m1)
                                {
                                    // s.AppraisalPeriodCalendar = null;
                                    db.TravelEligibilityPolicy.Attach(s);
                                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                    //await db.SaveChangesAsync();
                                    db.SaveChanges();
                                    TempData["RowVersion"] = s.RowVersion;
                                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                }
                                var CurCorp = db.TravelEligibilityPolicy.Find(data);
                                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                {
                                    // c.DBTrack = dbT;
                                    TravelEligibilityPolicy corp = new TravelEligibilityPolicy()
                                    {
                                        //LocationObj.LocCode = c.LocationObj.LocCode,
                                        //LocationObj.LocDesc = c.LocationObj.LocDesc,
                                        TA_TC_Eligibilty_Code = c.TA_TC_Eligibilty_Code,
                                        TA_Eligibility_Wages = c.TA_Eligibility_Wages,
                                        TA_Eligibility_Wages_Id = c.TA_Eligibility_Wages_Id,
                                        WagesRange = c.WagesRange,
                                        //WagesRange_Id=c.WagesRange_Id,
                                        TravelModeEligibilityPolicy = db_data.TravelModeEligibilityPolicy,
                                        Id = data,
                                        DBTrack = c.DBTrack
                                    };


                                    db.TravelEligibilityPolicy.Attach(corp);
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
                                    return Json(new Utility.JsonReturnClass { Id = corp.Id, Val = corp.TA_TC_Eligibilty_Code, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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
        public ActionResult Delete(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();

                TravelEligibilityPolicy traveleligibilitypolicy = db.TravelEligibilityPolicy.Find(data);
                try
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        db.Entry(traveleligibilitypolicy).State = System.Data.Entity.EntityState.Deleted;
                        db.SaveChanges();
                        ts.Complete();
                    }
                    Msg.Add("  Data removed successfully.  ");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    // return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });

                }

                catch (DataException /* dex */)
                {
                    //Log the error (uncomment dex variable name after DataException and add a line here to write a log.
                    Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //  return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists contact your system administrator." });
                    //ModelState.AddModelError(string.Empty, "Unable to delete. Try again, and if the problem persists contact your system administrator.");
                    //return RedirectToAction("Index");
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
                var ModuleName = System.Web.HttpContext.Current.Session["ModuleType"];
                var TravelEligibilityPolicy = new List<TravelEligibilityPolicy>();
                TravelEligibilityPolicy = db.TravelEligibilityPolicy
                                      .Include(e => e.WagesRange)
                                      .Include(e => e.TA_Eligibility_Wages)
                                      .ToList();
                IEnumerable<TravelEligibilityPolicy> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = TravelEligibilityPolicy;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE
                            .Where((e => (e.TA_TC_Eligibilty_Code.ToString().Contains(gp.searchString))
                                || (e.TA_Eligibility_Wages == null ? false : e.TA_Eligibility_Wages.FullDetails.ToString().Contains(gp.searchString))
                                || (e.Id.ToString().Contains(gp.searchString))
                               )).ToList()
                               .Select(a => new Object[] { a.TA_TC_Eligibilty_Code, a.TA_Eligibility_Wages == null ? "" : a.TA_Eligibility_Wages.FullDetails.ToString(), a.Id });
                        // jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Name != null ? a.Name.LookupVal : null), Convert.ToString(a.FromDate), Convert.ToString(a.ToDate), Convert.ToString(a.Default) }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.TA_TC_Eligibilty_Code, a.TA_Eligibility_Wages == null ? "" : a.TA_Eligibility_Wages.FullDetails.ToString(), a.Id }).ToList();
                        //jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Name != null ? a.Name.LookupVal : null, a.FromDate, a.ToDate, a.Default }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = TravelEligibilityPolicy;
                    Func<TravelEligibilityPolicy, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "TA_TC_Elligibilty_Code" ? c.TA_TC_Eligibilty_Code :
                                        // gp.sidx == "WagesRange" ? c.WagesRange == null ? "" : c.WagesRange.FullDetails.ToString() :
                                         gp.sidx == "TA_Elligibility_Wages" ? c.TA_Eligibility_Wages == null ? "" : c.TA_Eligibility_Wages.FullDetails.ToString() : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.TA_TC_Eligibilty_Code, a.TA_Eligibility_Wages == null ? "" : a.TA_Eligibility_Wages.FullDetails.ToString(), a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.TA_TC_Eligibilty_Code, a.TA_Eligibility_Wages == null ? "" : a.TA_Eligibility_Wages.FullDetails.ToString(), a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.TA_TC_Eligibilty_Code, a.TA_Eligibility_Wages == null ? "" : a.TA_Eligibility_Wages.FullDetails.ToString(), a.Id }).ToList();
                    }
                    totalRecords = TravelEligibilityPolicy.Count();
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