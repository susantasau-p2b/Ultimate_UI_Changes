///
/// Created by Kapil
///

using P2b.Global;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web.Mvc;
using System.Data;
using System.Data.Entity;
using P2BUltimate.Models;
using P2BUltimate.App_Start;
using System.Threading.Tasks;
using P2BUltimate.Security;
//using System.Web.Script.Serialization;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data.SqlClient;

namespace P2BUltimate.Controllers.Core.MainController
{
    [AuthoriseManger]
    public class LocationController : Controller
    {
        public ActionResult CreateContact_partial()
        {

            return View("~/Views/Shared/Core/_Contactdetails.cshtml");
        }

        public ActionResult Createcontactdetails_partial()
        {
            return View("~/Views/Shared/Core/_Contactdetails.cshtml");
        }


        public ActionResult Index()
        {
            return View("~/Views/Core/MainViews/Location/Index.cshtml");
        }
        public ActionResult CreateIncharge_partial()
        {
            return View("~/Views/Shared/_Namedetails.cshtml");
        }

        public ActionResult PopulateLookupDropDownList(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                int id = data == "" ? 0 : Convert.ToInt32(data);
                var lookupQuery = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "015").SingleOrDefault();

                List<SelectListItem> values = new List<SelectListItem>();

                if (lookupQuery != null)
                {
                    foreach (var item in lookupQuery.LookupValues)
                    {
                        if (item.IsActive == true)
                        {
                            values.Add(new SelectListItem
                            {
                                Text = item.LookupVal,
                                Value = item.Id.ToString(),
                                Selected = (item.Id == id ? true : false)
                            });
                        }
                    }
                }

                return Json(values, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult PopulateDropDownListWeeklyOffCalendar(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var qurey = db.WeeklyOffCalendar.Include(e => e.WeeklyOffList).Include(e => e.WOCalendar).ToList();
                //   var qurey = db.Calendar.Include(e => e.Name).ToList();
                var selected = "";
                if (!string.IsNullOrEmpty(data2))
                {
                    selected = data2;
                }
                var returndata = new SelectList(qurey, "Id", "FullDetails", selected);
                return Json(returndata, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult PopulateDropDownListHolidayCalendar(string data, string data2)
        {

            using (DataBaseContext db = new DataBaseContext())
            {
                var qurey = db.HolidayCalendar.Include(e => e.HolidayList).Include(e => e.HoliCalendar).ToList();
                //   var qurey = db.Calendar.Include(e => e.Name).ToList();
                var selected = "";
                if (!string.IsNullOrEmpty(data2))
                {
                    selected = data2;
                }
                var returndata = new SelectList(qurey, "Id", "FullDetails", selected);
                return Json(returndata, JsonRequestBehavior.AllowGet);
            }
        }
        //public ActionResult PopulateDropDownListIncharge(string data, string data2)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var qurey = db.Employee.Include(e => e.EmpOffInfo).Include(e => e.EmpName).OrderBy(e => e.EmpCode).ToList();
        //        //   var qurey = db.Calendar.Include(e => e.Name).ToList();
        //        var selected = "";
        //        if (!string.IsNullOrEmpty(data2))
        //        {
        //            selected = data2;
        //        }
        //        var returndata = new SelectList(qurey, "Id", "FullDetails", selected);
        //        return Json(returndata, JsonRequestBehavior.AllowGet);
        //    }
        //}
        public ActionResult LookupDepartmentIncharge(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.Employee.Include(e => e.EmpOffInfo).Include(e => e.EmpName).OrderBy(e => e.EmpCode).ToList();
                IEnumerable<Employee> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.Employee.ToList().Where(d => d.EmpCode.Contains(data));
                }
                else
                {
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.EmpCode }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);                
            }
        }
        public ActionResult GetLookupDetailsWOCalendar(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.WeeklyOffCalendar
                    .Include(e => e.WeeklyOffList)
                    .Include(e => e.WOCalendar).Include(e => e.WOCalendar.Name).ToList();
                // IEnumerable<WeeklyOffCalendar> all;
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.WeeklyOffCalendar.Where(e => e.Id != a).ToList();
                        else
                            fall = fall.Where(e => e.Id != a).ToList();
                    }
                }

                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                //var result_1 = (from c in fall
                //                select new { c.Id, c.DepartmentCode, c.DepartmentName });
                return Json(r, JsonRequestBehavior.AllowGet);

            }
            // return View();
        }

        public class returnClass
        {
            public string srno { get; set; }
            public string lookupvalue { get; set; }
        }
        public ActionResult GetLookupDetailsHolliday(List<int> SkipIds)
        { 
                using (DataBaseContext db = new DataBaseContext())
                {
                    var fall = db.HolidayCalendar
                        .Include(e => e.HoliCalendar)
                        .Include(e => e.HoliCalendar.Name).ToList();
                    // IEnumerable<WeeklyOffCalendar> all;
                    if (SkipIds != null)
                    {
                        foreach (var a in SkipIds)
                        {
                            if (fall == null)
                                fall = db.HolidayCalendar.Where(e => e.Id != a).ToList();
                            else
                                fall = fall.Where(e => e.Id != a).ToList();
                        }
                    }

                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                    //var result_1 = (from c in fall
                    //                select new { c.Id, c.DepartmentCode, c.DepartmentName });
                    return Json(r, JsonRequestBehavior.AllowGet);

                }  
        }

        //private DataBaseContext db = new DataBaseContext();


        private MultiSelectList GetCountry(int selectedValues)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<Country> country = new List<Country>();
                country = db.Country.ToList();
                return new SelectList(country, "Id", "LocationObj.LocDesc", selectedValues);
            }
        }

        private MultiSelectList GetState(int id, int selectedValues)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                Country country = db.Country.Include(e => e.States).Where(e => e.Id == id).SingleOrDefault();
                ICollection<State> st = country.States;
                return new SelectList(st, "Id", "LocationObj.LocDesc", selectedValues);
            }
        }

        private MultiSelectList GetRegion(int id, int selectedValues)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                State st = db.State.Include(e => e.StateRegions).Where(e => e.Id == id).SingleOrDefault();
                ICollection<StateRegion> region = st.StateRegions;
                return new SelectList(region, "Id", "LocationObj.LocDesc", selectedValues);
            }
        }

        //private MultiSelectList GetDist(int id, int selectedValues)
        //{
        //    StateRegion region = db.Region.Include(e => e.Districts).Where(e => e.Id == id).SingleOrDefault();
        //    ICollection<District> dist = region.Districts;
        //    return new SelectList(dist, "Id", "LocationObj.LocCode", selectedValues);
        //}

        private MultiSelectList GetTaluka(int id, int selectedValues)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                District dist = db.District.Include(e => e.Talukas).Where(e => e.Id == id).SingleOrDefault();
                ICollection<Taluka> taluka = dist.Talukas;
                return new SelectList(taluka, "Id", "LocationObj.LocDesc", selectedValues);
            }
        }

        private MultiSelectList GetCity(int id, int selectedValues)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                Taluka taluka = db.Taluka.Include(e => e.Cities).Where(e => e.Id == id).SingleOrDefault();
                ICollection<City> city = taluka.Cities;
                return new SelectList(city, "Id", "LocationObj.LocDesc", selectedValues);
            }
        }

        private MultiSelectList GetArea(int id, int selectedValues)
        {

            using (DataBaseContext db = new DataBaseContext())
            {
                City city = db.City.Include(e => e.Areas).Where(e => e.Id == id).SingleOrDefault();
                ICollection<Area> area = city.Areas;
                return new SelectList(area, "Id", "LocationObj.LocDesc", selectedValues);
            }
        }

        public MultiSelectList getState()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var state = db.State.ToList();
                return new MultiSelectList(state, "Id", "LocationObj.LocDesc", "");
            }
        }

        public ActionResult Createaddress_partial()
        {

            return View("~/Views/Shared/Core/_Address.cshtml");
        }

        public ActionResult Editaddress_partial(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var add = db.Address.Include(e => e.Area)
                                      .Include(e => e.City)
                                      .Include(e => e.Country)
                                      .Include(e => e.District)
                                      .Include(e => e.State)
                                      .Include(e => e.StateRegion)
                                      .Include(e => e.Taluka)
                                      .Where(e => e.Id == data).SingleOrDefault();

                var r = (from ca in db.Address
                         select new
                         {
                             Id = ca.Id,
                             Address1 = ca.Address1,
                             Address2 = ca.Address2,
                             Address3 = ca.Address3,
                             Landmark = ca.Landmark,
                             CountryName = ca.Country.Name,
                             StateName = ca.State.Name,
                             RegionName = ca.StateRegion.Name,
                             DistrictName = ca.District.Name,
                             TalukaName = ca.Taluka.Name,
                             CityName = ca.City.Name,
                             AreaName = ca.Area.Name,
                             Pincode = ca.Area.PinCode
                         }).Where(e => e.Id == data).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        public ActionResult Editcontact_partial(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var add = db.Address.Include(e => e.Area)
                                      .Include(e => e.City)
                                      .Include(e => e.Country)
                                      .Include(e => e.District)
                                      .Include(e => e.State)
                                      .Include(e => e.StateRegion)
                                      .Include(e => e.Taluka)
                                      .Where(e => e.Id == data).SingleOrDefault();

                var r = (from ca in db.Address
                         select new
                         {
                             Id = ca.Id,
                             Address1 = ca.Address1,
                             Address2 = ca.Address2,
                             Address3 = ca.Address3,
                             Landmark = ca.Landmark,
                             CountryName = ca.Country.Name,
                             StateName = ca.State.Name,
                             RegionName = ca.StateRegion.Name,
                             DistrictName = ca.District.Name,
                             TalukaName = ca.Taluka.Name,
                             CityName = ca.City.Name,
                             AreaName = ca.Area.Name,
                             Pincode = ca.Area.PinCode
                         }).Where(e => e.Id == data).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }


        //  [HttpPost]
        //public ActionResult Create(Location c, FormCollection form)
        //{
        //    List<string> Msg = new List<string>();
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        try
        //        {
        //            int comp_Id = 0;
        //            comp_Id = Convert.ToInt32(Session["CompId"]);
        //            var Company = new Company();
        //            Company = db.Company.Where(e => e.Id == comp_Id).SingleOrDefault();
        //            string Addrs = form["AddressList"] == "0" ? "" : form["AddressList"];
        //            string disc = form["ContactDetailsList"] == "0" ? "" : form["ContactDetailsList"];
        //            string OpeningDate = form["OpeningDate"] == "0" ? "" : form["OpeningDate"];
        //            string Location_Obj_DDL = form["Location_Obj_DDL"] == "0" ? "" : form["Location_Obj_DDL"];

        //            string Incharge_DDL = form["Incharge_DDL"] == "" ? null : form["Incharge_DDL"];
        //            if (Incharge_DDL != null && Incharge_DDL != "-Select-")
        //            {
        //                var value = db.Employee.Find(int.Parse(Incharge_DDL));
        //                c.Incharge = value;

        //            }
        //            //List<HolidayCalendar> lookupLang = new List<HolidayCalendar>();

        //            //string HolidayCalendar_DDL = form["HOCalendarList"] == "" ? null : form["HOCalendarList"];
        //            //if (HolidayCalendar_DDL != null)
        //            //{
        //            //    var ids = Utility.StringIdsToListIds(HolidayCalendar_DDL);
        //            //    foreach (var ca in ids)
        //            //    {
        //            //        var Lookup_val = db.HolidayCalendar.Find(ca);

        //            //        lookupLang.Add(Lookup_val);
        //            //        c.HolidayCalendar = lookupLang;
        //            //    }
        //            //}
        //            //else
        //            //{
        //            //    c.HolidayCalendar = null;
        //            //}

        //            //List<WeeklyOffCalendar> lookupLang1 = new List<WeeklyOffCalendar>();
        //            //string WeeklyOffCalendar_DDL = form["WOCalendarList"] == "" ? null : form["WOCalendarList"];

        //            //if (WeeklyOffCalendar_DDL != null)
        //            //{
        //            //    var ids = Utility.StringIdsToListIds(WeeklyOffCalendar_DDL);
        //            //    foreach (var ca in ids)
        //            //    {
        //            //        var Lookup_val = db.WeeklyOffCalendar.Find(ca);

        //            //        lookupLang1.Add(Lookup_val);
        //            //        c.WeeklyOffCalendar = lookupLang1;
        //            //    }
        //            //}
        //            //else
        //            //{
        //            //    c.WeeklyOffCalendar = null;
        //            //}


        //            string HolidayCalendar_DDL = form["HOCalendarList"] == "" ? null : form["HOCalendarList"];

        //            if (HolidayCalendar_DDL != null)
        //            {
        //                var ids = Utility.StringIdsToListIds(HolidayCalendar_DDL);
        //                var Holidaycallist = new List<HolidayCalendar>();
        //                foreach (var item in ids)
        //                {

        //                    int HolidayListid = Convert.ToInt32(item);
        //                    var val = db.HolidayCalendar.Include(e => e.HolidayList).Include(e => e.HoliCalendar).Where(e => e.Id == HolidayListid).SingleOrDefault();
        //                    if (val != null)
        //                    {
        //                        Holidaycallist.Add(val);
        //                    }
        //                }
        //                c.HolidayCalendar = Holidaycallist;
        //            }


        //            string WeeklyOffCalendar_DDL = form["WOCalendarList"] == "" ? null : form["WOCalendarList"];

        //            if (WeeklyOffCalendar_DDL != null)
        //            {
        //                var ids = Utility.StringIdsToListIds(WeeklyOffCalendar_DDL);
        //                var Weeklycallist = new List<WeeklyOffCalendar>();
        //                foreach (var item in ids)
        //                {

        //                    int weeklyListid = Convert.ToInt32(item);
        //                    var val = db.WeeklyOffCalendar.Include(e => e.WeeklyOffList).Include(e => e.WOCalendar).Where(e => e.Id == weeklyListid).SingleOrDefault();
        //                    if (val != null)
        //                    {
        //                        Weeklycallist.Add(val);
        //                    }
        //                }
        //                c.WeeklyOffCalendar = Weeklycallist;
        //            }



        //            if (disc != null && disc != "")
        //            {
        //                int ContId = Convert.ToInt32(disc);
        //                var val = db.ContactDetails.Include(e => e.ContactNumbers)
        //                                    .Where(e => e.Id == ContId).SingleOrDefault();
        //                c.ContactDetails = val;
        //            }
        //            if (OpeningDate != null && OpeningDate != "")
        //            {
        //                var val = DateTime.Parse(OpeningDate);
        //                c.OpeningDate = val;
        //            }
        //            if (Location_Obj_DDL != null && Location_Obj_DDL != "")
        //            {
        //                var id = Convert.ToInt32(Location_Obj_DDL);
        //                var val = db.LocationObj.Find(id);
        //                c.LocationObj = val;
        //            }

        //            if (Addrs != null && Addrs != "")
        //            {
        //                int AddId = Convert.ToInt32(Addrs);
        //                var val = db.Address.Include(e => e.Area)
        //                                    .Include(e => e.City)
        //                                    .Include(e => e.Country)
        //                                    .Include(e => e.District)
        //                                    .Include(e => e.State)
        //                                    .Include(e => e.StateRegion)
        //                                    .Include(e => e.Taluka)
        //                                    .Where(e => e.Id == AddId).SingleOrDefault();
        //                c.Address = val;
        //            }

        //            if (ModelState.IsValid)
        //            {


        //                using (TransactionScope ts = new TransactionScope())
        //                {
        //                    //if (db.Location.Any(o => o.LocationObj.LocCode == c.LocationObj.LocCode))
        //                    //{
        //                    //	return Json(new Object[] { "", "", "LocationObj.LocCode Already Exists.", JsonRequestBehavior.AllowGet });
        //                    //}

        //                    c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };


        //                    Location Location = new Location()
        //                    {
        //                        LocationObj = c.LocationObj,
        //                        OpeningDate = c.OpeningDate,
        //                        Address = c.Address,

        //                        ContactDetails = c.ContactDetails,
        //                        Incharge = c.Incharge,
        //                        HolidayCalendar = c.HolidayCalendar,
        //                        WeeklyOffCalendar = c.WeeklyOffCalendar,
        //                        DBTrack = c.DBTrack
        //                    };
        //                    try
        //                    {

        //                        db.Location.Add(Location);
        //                        var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, c.DBTrack);
        //                        DT_Location DT_Corp = (DT_Location)rtn_Obj;
        //                        DT_Corp.Address_Id = c.Address == null ? 0 : c.Address.Id;
        //                        DT_Corp.ContactDetails_Id = c.ContactDetails == null ? 0 : c.ContactDetails.Id;
        //                        db.Create(DT_Corp);
        //                        db.SaveChanges();
        //                        db.SaveChanges();
        //                        if (Company != null)
        //                        {
        //                            var Objjob = new List<Location>();
        //                            Objjob.Add(Location);
        //                            Company.Location = Objjob;
        //                            db.Company.Attach(Company);
        //                            db.Entry(Company).State = System.Data.Entity.EntityState.Modified;
        //                            db.SaveChanges();
        //                            db.Entry(Company).State = System.Data.Entity.EntityState.Detached;

        //                        }
        //                        ts.Complete();
        //                        Msg.Add("  Data Saved successfully  ");
        //                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                        //return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
        //                        //db..Add(Location);
        //                        //db.SaveChanges();
        //                        //ts.Complete();
        //                        //return Json(new Object[] { null, null, "Data saved successfully." }, JsonRequestBehavior.AllowGet);
        //                    }

        //                    catch (DbUpdateConcurrencyException)
        //                    {
        //                        return RedirectToAction("Create", new { concurrencyError = true, id = c.Id });
        //                    }
        //                    catch (DataException)
        //                    {
        //                        Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
        //                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                        // return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
        //                    }

        //                }
        //            }
        //            else
        //            {
        //                StringBuilder sb = new StringBuilder("");
        //                foreach (ModelState modelState in ModelState.Values)
        //                {
        //                    foreach (ModelError error in modelState.Errors)
        //                    {
        //                        sb.Append(error.ErrorMessage);
        //                        sb.Append("." + "\n");
        //                    }
        //                }
        //                var errorMsg = sb.ToString();
        //                Msg.Add(errorMsg);
        //                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                //var errorMsg = sb.ToString();
        //                //return this.Json(new { msg = errorMsg });
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            LogFile Logfile = new LogFile();
        //            ErrorLog Err = new ErrorLog()
        //            {
        //                ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
        //                ExceptionMessage = ex.Message,
        //                ExceptionStackTrace = ex.StackTrace,
        //                LineNo = Convert.ToString(new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
        //                LogTime = DateTime.Now
        //            };
        //            Logfile.CreateLogFile(Err);
        //            Msg.Add(ex.Message);
        //            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //}
        public List<string> ValidateObj(Object obj)
        {
            var errorList = new List<String>();
            var context = new ValidationContext(obj, serviceProvider: null, items: null);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(obj, context, results, true);
            if (!isValid)
            {
                foreach (var validationResult in results)
                {
                    errorList.Add(validationResult.ErrorMessage);
                }
                return errorList;
            }
            else
            {
                return errorList;
            }
        }

        //    [HttpPost]
        //public ActionResult Create(Location c, FormCollection form)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        List<string> Msg = new List<string>();
        //        try
        //        {
        //            int comp_Id = 0;
        //            comp_Id = Convert.ToInt32(Session["CompId"]);
        //            var Company = new Company();
        //            Company = db.Company.Where(e => e.Id == comp_Id).SingleOrDefault();
        //            string Addrs = form["AddressList"] == "0" ? "" : form["AddressList"];
        //            string disc = form["ContactDetailsList"] == "0" ? "" : form["ContactDetailsList"];
        //            string OpeningDate = form["OpeningDate"] == "0" ? "" : form["OpeningDate"];
        //            string Location_Obj_DDL = form["Location_Obj_DDL"] == "0" ? "" : form["Location_Obj_DDL"];

        //            string Incharge_DDL = form["Incharge_DDL"] == "" ? null : form["Incharge_DDL"];
        //            if (Incharge_DDL != null && Incharge_DDL != "-Select-")
        //            {
        //                var value = db.Employee.Find(int.Parse(Incharge_DDL));
        //                c.Incharge = value;

        //            }
        //            //List<HolidayCalendar> lookupLang = new List<HolidayCalendar>();

        //            //string HolidayCalendar_DDL = form["HOCalendarList"] == "" ? null : form["HOCalendarList"];
        //            //if (HolidayCalendar_DDL != null)
        //            //{
        //            //    var ids = Utility.StringIdsToListIds(HolidayCalendar_DDL);
        //            //    foreach (var ca in ids)
        //            //    {
        //            //        var Lookup_val = db.HolidayCalendar.Find(ca);

        //            //        lookupLang.Add(Lookup_val);
        //            //        c.HolidayCalendar = lookupLang;
        //            //    }
        //            //}
        //            //else
        //            //{
        //            //    c.HolidayCalendar = null;
        //            //}

        //            //List<WeeklyOffCalendar> lookupLang1 = new List<WeeklyOffCalendar>();
        //            //string WeeklyOffCalendar_DDL = form["WOCalendarList"] == "" ? null : form["WOCalendarList"];

        //            //if (WeeklyOffCalendar_DDL != null)
        //            //{
        //            //    var ids = Utility.StringIdsToListIds(WeeklyOffCalendar_DDL);
        //            //    foreach (var ca in ids)
        //            //    {
        //            //        var Lookup_val = db.WeeklyOffCalendar.Find(ca);

        //            //        lookupLang1.Add(Lookup_val);
        //            //        c.WeeklyOffCalendar = lookupLang1;
        //            //    }
        //            //}
        //            //else
        //            //{
        //            //    c.WeeklyOffCalendar = null;
        //            //}


        //            string HolidayCalendar_DDL = form["HOCalendarList"] == "" ? null : form["HOCalendarList"];

        //            if (HolidayCalendar_DDL != null)
        //            {
        //                var ids = Utility.StringIdsToListIds(HolidayCalendar_DDL);
        //                var Holidaycallist = new List<HolidayCalendar>();
        //                foreach (var item in ids)
        //                {

        //                    int HolidayListid = Convert.ToInt32(item);
        //                    var val = db.HolidayCalendar.Include(e => e.HolidayList).Include(e => e.HoliCalendar).Include(e => e.HoliCalendar.Name).Where(e => e.Id == HolidayListid).SingleOrDefault();
        //                    if (val != null)
        //                    {
        //                        Holidaycallist.Add(val);
        //                    }
        //                }
        //                c.HolidayCalendar = Holidaycallist;
        //            }


        //            string WeeklyOffCalendar_DDL = form["WOCalendarList"] == "" ? null : form["WOCalendarList"];

        //            if (WeeklyOffCalendar_DDL != null)
        //            {
        //                var ids = Utility.StringIdsToListIds(WeeklyOffCalendar_DDL);
        //                var Weeklycallist = new List<WeeklyOffCalendar>();
        //                foreach (var item in ids)
        //                {

        //                    int weeklyListid = Convert.ToInt32(item);
        //                    var val = db.WeeklyOffCalendar.Include(e => e.WeeklyOffList).Include(e => e.WOCalendar).Where(e => e.Id == weeklyListid).SingleOrDefault();

        //                    if (val != null)
        //                    {
        //                        Weeklycallist.Add(val);
        //                    }
        //                }
        //                c.WeeklyOffCalendar = Weeklycallist;
        //            }



        //            if (disc != null && disc != "")
        //            {
        //                int ContId = Convert.ToInt32(disc);
        //                var val = db.ContactDetails.Include(e => e.ContactNumbers)
        //                                    .Where(e => e.Id == ContId).SingleOrDefault();
        //                c.ContactDetails = val;
        //            }
        //            if (OpeningDate != null && OpeningDate != "")
        //            {
        //                var val = DateTime.Parse(OpeningDate);
        //                c.OpeningDate = val;
        //            }
        //            if (Location_Obj_DDL != null && Location_Obj_DDL != "")
        //            {
        //                var id = Convert.ToInt32(Location_Obj_DDL);
        //                var val = db.LocationObj.Find(id);
        //                c.LocationObj = val;
        //            }

        //            if (Addrs != null && Addrs != "")
        //            {
        //                int AddId = Convert.ToInt32(Addrs);
        //                var val = db.Address.Include(e => e.Area)
        //                                    .Include(e => e.City)
        //                                    .Include(e => e.Country)
        //                                    .Include(e => e.District)
        //                                    .Include(e => e.State)
        //                                    .Include(e => e.StateRegion)
        //                                    .Include(e => e.Taluka)
        //                                    .Where(e => e.Id == AddId).SingleOrDefault();
        //                c.Address = val;
        //            }

        //            if (ModelState.IsValid)
        //            {


        //                using (TransactionScope ts = new TransactionScope())
        //                {
        //                    c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
        //                    Location Location = new Location()
        //                    {
        //                        LocationObj = c.LocationObj,
        //                        OpeningDate = c.OpeningDate,
        //                        Address = c.Address,

        //                        ContactDetails = c.ContactDetails,
        //                        Incharge = c.Incharge,
        //                        HolidayCalendar = c.HolidayCalendar,
        //                        WeeklyOffCalendar = c.WeeklyOffCalendar,
        //                        DBTrack = c.DBTrack
        //                    };
        //                    var LocationValidation = ValidateObj(Location);
        //                    if (LocationValidation.Count > 0)
        //                    {
        //                        foreach (var item in LocationValidation)
        //                        {

        //                            Msg.Add("Location" + item);
        //                        }
        //                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                    }
        //                    db.Location.Add(Location);
        //                    var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, c.DBTrack);
        //                    DT_Location DT_Corp = (DT_Location)rtn_Obj;
        //                    DT_Corp.Address_Id = c.Address == null ? 0 : c.Address.Id;
        //                    DT_Corp.ContactDetails_Id = c.ContactDetails == null ? 0 : c.ContactDetails.Id;
        //                    db.Create(DT_Corp);
        //                    db.SaveChanges();
        //                    if (Company != null)
        //                    {
        //                        var Objjob = new List<Location>();
        //                        Objjob.Add(Location);
        //                        Company.Location = Objjob;
        //                        var aa = ValidateObj(Company);
        //                        // var aa = ValidateObj(Company);
        //                        // var aa = ValidateObj(Company);
        //                        if (aa.Count > 0)
        //                        {
        //                            foreach (var item in aa)
        //                            {

        //                                Msg.Add("Company" + item);
        //                            }
        //                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                        }
        //                        db.Company.Attach(Company);
        //                        db.Entry(Company).State = System.Data.Entity.EntityState.Modified;
        //                        db.SaveChanges();
        //                        db.Entry(Company).State = System.Data.Entity.EntityState.Detached;

        //                    }
        //                    ts.Complete();
        //                    Msg.Add("  Data Saved successfully  ");
        //                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                }
        //            }
        //            else
        //            {
        //                StringBuilder sb = new StringBuilder("");
        //                foreach (ModelState modelState in ModelState.Values)
        //                {
        //                    foreach (ModelError error in modelState.Errors)
        //                    {
        //                        sb.Append(error.ErrorMessage);
        //                        sb.Append("." + "\n");
        //                    }
        //                }
        //                var errorMsg = sb.ToString();
        //                Msg.Add(errorMsg);
        //                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                //var errorMsg = sb.ToString();
        //                //return this.Json(new { msg = errorMsg });
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            LogFile Logfile = new LogFile();
        //            ErrorLog Err = new ErrorLog()
        //            {
        //                ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
        //                ExceptionMessage = ex.Message,
        //                ExceptionStackTrace = ex.StackTrace,
        //                LineNo = Convert.ToString(new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
        //                LogTime = DateTime.Now
        //            };
        //            Logfile.CreateLogFile(Err);
        //            Msg.Add(ex.Message);
        //            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //        }
        //    }

        //}

        [HttpPost]
        public ActionResult Create(Location c, FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    int comp_Id = 0;
                    comp_Id = Convert.ToInt32(Session["CompId"]);
                    var Company = new Company();
                    Company = db.Company.Where(e => e.Id == comp_Id).SingleOrDefault();
                    string Addrs = form["AddressList"] == "0" ? "" : form["AddressList"];
                    string disc = form["ContactDetailsList"] == "0" ? "" : form["ContactDetailsList"];
                    string OpeningDate = form["OpeningDate"] == "0" ? "" : form["OpeningDate"];
                    string Location_Obj_DDL = form["Location_Obj_DDL"] == "0" ? "" : form["Location_Obj_DDL"];
                    string UnitId = form["UnitId"] == "0" ? "" : form["UnitId"];
                    //string Incharge_DDL = form["Incharge_DDL"] == "" ? null : form["Incharge_DDL"];
                    string Incharge_DDL = form["Incharge_Id"] == "" ? null : form["Incharge_Id"];

                    if (Incharge_DDL != null && Incharge_DDL != "-Select-")
                    {
                        var value = db.Employee.Find(int.Parse(Incharge_DDL));
                        c.Incharge = value;

                    }
                    string BusinessCategory = form["BusinessCategory_DDL"] == "0" ? "" : form["BusinessCategory_DDL"];
                    if (BusinessCategory != null && BusinessCategory != "")
                    {
                        var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "408").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(BusinessCategory)).FirstOrDefault();  //db.LookupValue.Find(int.Parse(BusinessCategory));
                        c.BusinessCategory = val;
                    }
                    if (Location_Obj_DDL == null || Location_Obj_DDL == "")
                    {
                        Msg.Add("Please Select Location object.  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    //if (UnitId != null && UnitId != "")
                    //{
                    //    // var val = db.Location.Find(int.Parse(biometric));
                    //    c.UnitId = UnitId;
                    //}

                    //List<HolidayCalendar> lookupLang = new List<HolidayCalendar>();

                    //string HolidayCalendar_DDL = form["HOCalendarList"] == "" ? null : form["HOCalendarList"];
                    //if (HolidayCalendar_DDL != null)
                    //{
                    //    var ids = Utility.StringIdsToListIds(HolidayCalendar_DDL);
                    //    foreach (var ca in ids)
                    //    {
                    //        var Lookup_val = db.HolidayCalendar.Find(ca);

                    //        lookupLang.Add(Lookup_val);
                    //        c.HolidayCalendar = lookupLang;
                    //    }
                    //}
                    //else
                    //{
                    //    c.HolidayCalendar = null;
                    //}

                    //List<WeeklyOffCalendar> lookupLang1 = new List<WeeklyOffCalendar>();
                    //string WeeklyOffCalendar_DDL = form["WOCalendarList"] == "" ? null : form["WOCalendarList"];

                    //if (WeeklyOffCalendar_DDL != null)
                    //{
                    //    var ids = Utility.StringIdsToListIds(WeeklyOffCalendar_DDL);
                    //    foreach (var ca in ids)
                    //    {
                    //        var Lookup_val = db.WeeklyOffCalendar.Find(ca);

                    //        lookupLang1.Add(Lookup_val);
                    //        c.WeeklyOffCalendar = lookupLang1;
                    //    }
                    //}
                    //else
                    //{
                    //    c.WeeklyOffCalendar = null;
                    //}


                    string HolidayCalendar_DDL = form["HOCalendarList"] == "" ? null : form["HOCalendarList"];

                    if (HolidayCalendar_DDL != null)
                    {
                        var ids = Utility.StringIdsToListIds(HolidayCalendar_DDL);
                        var Holidaycallist = new List<HolidayCalendar>();
                        foreach (var item in ids)
                        {

                            int HolidayListid = Convert.ToInt32(item);
                            var val = db.HolidayCalendar.Include(e => e.HolidayList).Include(e => e.HoliCalendar).Include(e => e.HoliCalendar.Name).Where(e => e.Id == HolidayListid).SingleOrDefault();
                            if (val != null)
                            {
                                Holidaycallist.Add(val);
                            }
                        }
                        c.HolidayCalendar = Holidaycallist;
                    }


                    string WeeklyOffCalendar_DDL = form["WOCalendarList"] == "" ? null : form["WOCalendarList"];

                    if (WeeklyOffCalendar_DDL != null)
                    {
                        var ids = Utility.StringIdsToListIds(WeeklyOffCalendar_DDL);
                        var Weeklycallist = new List<WeeklyOffCalendar>();
                        foreach (var item in ids)
                        {

                            int weeklyListid = Convert.ToInt32(item);
                            var val = db.WeeklyOffCalendar.Include(e => e.WOCalendar).Include(e => e.WOCalendar.Name).Where(e => e.Id == weeklyListid).SingleOrDefault();

                            if (val != null)
                            {
                                Weeklycallist.Add(val);
                            }
                        }
                        c.WeeklyOffCalendar = Weeklycallist;
                    }



                    if (disc != null && disc != "")
                    {
                        int ContId = Convert.ToInt32(disc);
                        var val = db.ContactDetails.Include(e => e.ContactNumbers)
                                            .Where(e => e.Id == ContId).SingleOrDefault();
                        c.ContactDetails = val;
                    }
                    if (OpeningDate != null && OpeningDate != "")
                    {
                        var val = DateTime.Parse(OpeningDate);
                        c.OpeningDate = val;
                    }
                    if (Location_Obj_DDL != null && Location_Obj_DDL != "")
                    {
                        var id = Convert.ToInt32(Location_Obj_DDL);
                        var val = db.LocationObj.Find(id);
                        c.LocationObj = val;
                    }

                    if (Addrs != null && Addrs != "")
                    {
                        int AddId = Convert.ToInt32(Addrs);
                        var val = db.Address.Include(e => e.Area)
                                            .Include(e => e.City)
                                            .Include(e => e.Country)
                                            .Include(e => e.District)
                                            .Include(e => e.State)
                                            .Include(e => e.StateRegion)
                                            .Include(e => e.Taluka)
                                            .Where(e => e.Id == AddId).SingleOrDefault();
                        c.Address = val;
                    }

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            Location Location = new Location()
                            {
                                LocationObj = c.LocationObj,
                                OpeningDate = c.OpeningDate,
                                Address = c.Address,

                                ContactDetails = c.ContactDetails,
                                Incharge = c.Incharge,
                                HolidayCalendar = c.HolidayCalendar,
                                WeeklyOffCalendar = c.WeeklyOffCalendar,
                                //UnitId = c.UnitId, commented due to entity changes
                                DBTrack = c.DBTrack
                            };
                            var LocationValidation = ValidateObj(Location);
                            if (LocationValidation.Count > 0)
                            {
                                foreach (var item in LocationValidation)
                                {

                                    Msg.Add("Location" + item);
                                }
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                            db.Location.Add(Location);
                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, c.DBTrack);
                            DT_Location DT_Corp = (DT_Location)rtn_Obj;
                            DT_Corp.Address_Id = c.Address == null ? 0 : c.Address.Id;
                            DT_Corp.ContactDetails_Id = c.ContactDetails == null ? 0 : c.ContactDetails.Id;
                            db.Create(DT_Corp);
                            db.SaveChanges();
                            if (Company != null)
                            {
                                var Objjob = new List<Location>();
                                Objjob.Add(Location);
                                Company.Location = Objjob;
                                var aa = ValidateObj(Company);
                                // var aa = ValidateObj(Company);
                                // var aa = ValidateObj(Company);
                                if (aa.Count > 0)
                                {
                                    foreach (var item in aa)
                                    {

                                        Msg.Add("Company" + item);
                                    }
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }
                                db.Company.Attach(Company);
                                db.Entry(Company).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(Company).State = System.Data.Entity.EntityState.Detached;

                            }


                            int Compid = int.Parse(Convert.ToString(Session["CompId"]));
                            string connString = ConfigurationManager.ConnectionStrings["DataBaseContext"].ConnectionString;
                            using (SqlConnection con = new SqlConnection(connString))
                            {
                                using (SqlCommand cmd = new SqlCommand("Insert_EmpReporting_All", con))
                                {
                                    cmd.CommandType = CommandType.StoredProcedure;

                                    cmd.Parameters.Add("@CompCode", SqlDbType.VarChar).Value = db.Company.Where(e => e.Id == Compid).SingleOrDefault().Code;
                                    cmd.Parameters.Add("@EmpCode", SqlDbType.VarChar).Value = "";

                                    con.Open();
                                    cmd.ExecuteNonQuery();
                                }
                            }


                            ts.Complete();
                            Msg.Add("  Data Saved successfully  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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

                        //var errorMsg = sb.ToString();
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
        /*---------------------------------------- Edit ------------------------------------ */
        public class returnEditClass
        {
            public Array week_Id { get; set; }
            public Array WeeklyFullDetails { get; set; }
            public Array Holly_Id { get; set; }
            public Array HollyFullDetails { get; set; }
        }
        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.Location
                    .Include(e => e.ContactDetails)
                    .Include(e => e.Address)
                    .Include(e => e.Type)
                    .Include(e => e.ContactDetails)
                    .Include(e => e.LocationObj)
                      .Include(e => e.HolidayCalendar)
                        .Include(e => e.WeeklyOffCalendar)
                        .Include(e => e.Incharge)
                        .Include(e => e.Incharge.EmpName)
                         .Include(e => e.BusinessCategory)
                         .Where(e => e.Id == data).Select
                    (e => new
                    {
                        Code = e.LocationObj.LocCode,
                        Desc = e.LocationObj.LocDesc,
                        LocationObj = e.LocationObj == null ? 0 : e.LocationObj.Id,
                        Incharge = e.Incharge == null ? "" : e.Incharge.FullDetails,
                        Incharge_Id = e.Incharge != null ? e.Incharge.Id : 0,
                        BusinessCategory_Id = e.BusinessCategory != null ? e.BusinessCategory.Id : 0,
                        //  HolidayCalendar_Id = e.HolidayCalendar != null ? e.HolidayCalendar.Id : 0,
                        //WeeklyOffCalendar_Id = e.WeeklyOffCalendar != null ? e.WeeklyOffCalendar.Id : 0,
                        Address_FullAddress = e.Address.FullAddress == null ? "" : e.Address.FullAddress,
                        Add_Id = e.Address.Id == null ? "" : e.Address.Id.ToString(),
                        Cont_Id = e.ContactDetails.Id == null ? "" : e.ContactDetails.Id.ToString(),
                        FullContactDetails = e.ContactDetails.FullContactDetails == null ? "" : e.ContactDetails.FullContactDetails,
                        OpeningDate = e.OpeningDate,
                        //Unit = e.UnitId == null ? "" : e.UnitId,
                        Type_Id = e.Type.Id == null ? 0 : e.Type.Id,
                        Action = e.DBTrack.Action
                    }).ToList();


                List<returnEditClass> oreturnEditClass = new List<returnEditClass>();

                //var k = db.Location.Include(e => e.HolidayCalendar)
                //    .Include(e => e.HolidayCalendar.Select(a => a.HoliCalendar.Name))
                //    .Include(e => e.HolidayCalendar.Select(a => a.HolidayList))
                //   .Where(e => e.Id == data && e.HolidayCalendar.Count > 0).ToList();

                var k = db.Location.Include(e => e.HolidayCalendar)
                    .Include(e => e.HolidayCalendar.Select(a => a.HoliCalendar))
                     .Include(e => e.HolidayCalendar.Select(a => a.HoliCalendar.Name))
                    .Include(e => e.HolidayCalendar.Select(a => a.HolidayList))
                   .Where(e => e.Id == data && e.HolidayCalendar.Count > 0).ToList();
                foreach (var e in k)
                {
                    oreturnEditClass.Add(new returnEditClass
                    {
                        Holly_Id = e.HolidayCalendar.Select(a => a.Id.ToString()).ToArray(),
                        HollyFullDetails = e.HolidayCalendar.Select(a => a.FullDetails).ToArray()
                    });
                }

                var m = db.Location.Include(e => e.WeeklyOffCalendar)
                    .Include(e => e.WeeklyOffCalendar.Select(a => a.WOCalendar))
                     .Include(e => e.WeeklyOffCalendar.Select(a => a.WOCalendar.Name))
                    .Include(e => e.WeeklyOffCalendar.Select(a => a.WeeklyOffList))
                   .Where(e => e.Id == data && e.WeeklyOffCalendar.Count > 0).ToList();

                //var m = db.Location.Include(e => e.WeeklyOffCalendar)
                //    //.Include(e => e.WeeklyOffCalendar.Select(a => a.WeeklyOffList))
                //      .Include(e => e.WeeklyOffCalendar.Select(a => a.WOCalendar))
                //      .Include(e=>e.WeeklyOffCalendar.Select(a=>a.WOCalendar.Name))
                //    .Include(e => e.WeeklyOffCalendar.Select(a => a.WeeklyOffList.Select(z => z.WeeklyOffStatus)))
                //    .Include(e => e.WeeklyOffCalendar.Select(a => a.WeeklyOffList.Select(z => z.WeekDays)))
                //    .Where(e => e.Id == data).ToList();
                foreach (var e in m)
                {
                    oreturnEditClass.Add(new returnEditClass
                    {
                        week_Id = e.WeeklyOffCalendar.Select(a => a.Id.ToString()).ToArray(),
                        WeeklyFullDetails = e.WeeklyOffCalendar.Select(a => a.FullDetails).ToArray()
                    });
                }

                var W = db.DT_Location
                     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id,
                         Code = e.LocationObj_Id == 0 ? "" : db.LocationObj
                                   .Where(x => x.Id == e.LocationObj_Id)
                                   .Select(x => x.LocCode).FirstOrDefault(),
                         Name = e.LocationObj_Id == 0 ? "" : db.LocationObj
                                    .Where(x => x.Id == e.LocationObj_Id)
                                    .Select(x => x.LocDesc).FirstOrDefault(),
                         OpeningDate = e.OpeningDate,
                         Type_Val = e.Type_Id == 0 ? "" : db.LookupValue
                                    .Where(x => x.Id == e.Type_Id)
                                    .Select(x => x.LookupVal).FirstOrDefault(),
                         Address_Val = e.Address_Id == 0 ? "" : db.Address.Where(x => x.Id == e.Address_Id).Select(x => x.FullAddress).FirstOrDefault(),
                         Contact_Val = e.ContactDetails_Id == 0 ? "" : db.ContactDetails.Where(x => x.Id == e.ContactDetails_Id).Select(x => x.FullContactDetails).FirstOrDefault()
                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var Corp = db.Location.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, oreturnEditClass, W, Auth, JsonRequestBehavior.AllowGet });
            }
        }


        //[HttpPost]
        //public ActionResult Edit(int data)
        //{
        //    var corpo = db.Location
        //                           .Include(e => e.ContactDetails).Include(e => e.ContactDetails.ContactNumbers)
        //                           .Include(e => e.Address).Include(e => e.Address.Area)
        //                           .Include(e => e.Address.City)
        //                           .Include(e => e.Address.Country)
        //                           .Include(e => e.Address.District)
        //                           .Include(e => e.Address.State)
        //                           .Include(e => e.Address.StateRegion)
        //                           .Include(e => e.Address.Taluka)
        //                           .Where(e => e.Id == data).ToList();

        //    var add = (from ca in corpo
        //               where ca.Address != null
        //               select new
        //               {
        //                   Id = ca.Address.Id,
        //                   address = ca.Address.FullAddress
        //               }).Distinct();

        //    var cont = (from ca in corpo
        //                where ca.ContactDetails != null
        //                select new
        //                {
        //                    Id = ca.ContactDetails.Id,
        //                    FullContactDetails = ca.ContactDetails.FullContactDetails
        //                }).Distinct();

        //    TempData["RowVersion"] = db.Location.Find(data).RowVersion;
        //    return Json(new object[] { add, cont }, JsonRequestBehavior.AllowGet);
        //}



        /* ------------------------------------------ Grid -------------------------------------------------- */



        //public class DivisionLocation
        //{
        //	public int DiviId { get; set; }
        //	public string DiviName { get; set; }
        //	public int Id { get; set; }
        //	public string LocationObj.LocCode { get; set; }
        //	public string LocationObj.LocDesc { get; set; }
        //	public DBTrack DBTrack { get; set; }
        //}

        //public ActionResult P2BGrid(P2BGrid_Parameters gp)
        //{
        //    try
        //    {
        //        DataBaseContext db = new DataBaseContext();
        //        int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
        //        int pageSize = gp.rows;
        //        int totalPages = 0;
        //        int totalRecords = 0;
        //        var jsonData = (Object)null;
        //        IEnumerable<DivisionLocation> Location = null;

        //        List<DivisionLocation> model = new List<DivisionLocation>();

        //        var r = db.Division.Include(e => e.Locations).ToList();

        //        var view = new DivisionLocation();
        //        foreach (var i in r)
        //        {
        //            foreach (var z in i.Locations)
        //            {
        //                view = new DivisionLocation()
        //                {
        //                    DiviId = i.Id,
        //                    DiviName = i.LocationObj.LocDesc,
        //                    Id = z.Id,
        //                    LocationObj.LocCode = z.LocationObj.LocCode,
        //                    LocationObj.LocDesc = z.LocationObj.LocDesc,
        //                    DBTrack = z.DBTrack 
        //                };

        //                model.Add(view);
        //            }
        //        }

        //        var fall = db.Location.ToList();
        //        var list1 = db.Division.ToList().SelectMany(e => e.Locations);
        //        var list2 = fall.Except(list1);

        //        foreach (var z in list2)
        //        {
        //            view = new DivisionLocation()
        //            {
        //                DiviId = 0,
        //                DiviName = "",
        //                Id = z.Id,
        //                LocationObj.LocCode = z.LocationObj.LocCode,
        //                LocationObj.LocDesc = z.LocationObj.LocDesc,
        //                DBTrack = z.DBTrack 
        //            };

        //            model.Add(view);
        //        }

        //        Location = model;

        //        IEnumerable<DivisionLocation> IE; 
        //        if (gp.IsAutho == true)
        //        {
        //            IE = Location.Where(e => e.DBTrack.IsModified == true).ToList();
        //        }
        //        else
        //        {
        //            IE = Location.ToList();
        //        }
        //        if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
        //        {
        //            //IE = Location;
        //            if (gp.searchOper.Equals("eq"))
        //            {
        //                if (gp.searchField == "DiviName")
        //                    jsonData = IE.Select(a => new { a.Id, a.DiviName, a.LocationObj.LocCode, a.LocationObj.LocDesc }).Where((e => (e.DiviName.ToString().Contains(gp.searchString)))).ToList();
        //                else if (gp.searchField == "Id")
        //                    jsonData = IE.Select(a => new { a.Id, a.DiviName, a.LocationObj.LocCode, a.LocationObj.LocDesc }).Where((e => (e.Id.ToString().Contains(gp.searchString)))).ToList();
        //                else if (gp.searchField == "LocationObj.LocCode")
        //                    jsonData = IE.Select(a => new { a.Id, a.DiviName, a.LocationObj.LocCode, a.LocationObj.LocDesc }).Where((e => (e.LocationObj.LocCode.ToString().Contains(gp.searchString)))).ToList();
        //                else if (gp.searchField == "LocationObj.LocDesc")
        //                    jsonData = IE.Select(a => new { a.Id, a.DiviName, a.LocationObj.LocCode, a.LocationObj.LocDesc }).Where((e => (e.LocationObj.LocDesc.ToString().Contains(gp.searchString)))).ToList();
        //                ////jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.LocationObj.LocCode), Convert.ToString(a.LocationObj.LocDesc), Convert.ToString(a.LocationObj) != null ? Convert.ToString(a.LocationObj.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.DiviName, a.Id, a.LocationObj.LocCode, a.LocationObj.LocDesc }).ToList();
        //            }
        //            totalRecords = IE.Count();
        //        }
        //        else
        //        {
        //            //IE = Region;
        //            Func<DivisionLocation, dynamic> orderfuc;
        //            if (gp.sidx == "Id")
        //            {
        //                orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
        //            }
        //            else
        //            {
        //                orderfuc = (c => gp.sidx == "DiviName" ? c.DiviName.ToString() :
        //                                 gp.sidx == "LocationObj.LocCode" ? c.LocationObj.LocCode :
        //                                 gp.sidx == "LocationObj.LocDesc" ? c.LocationObj.LocDesc : "");
        //            }
        //            if (gp.sord == "asc")
        //            {
        //                IE = IE.OrderBy(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.DiviName), Convert.ToString(a.LocationObj.LocCode), Convert.ToString(a.LocationObj.LocDesc) }).ToList();
        //            }
        //            else if (gp.sord == "desc")
        //            {
        //                IE = IE.OrderByDescending(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.DiviName), Convert.ToString(a.LocationObj.LocCode), Convert.ToString(a.LocationObj.LocDesc) }).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.DiviName, a.LocationObj.LocCode, a.LocationObj.LocDesc }).ToList();
        //            }
        //            totalRecords = Location.Count();
        //        }
        //        if (totalRecords > 0)
        //        {
        //            totalPages = (int)Math.Ceiling((float)totalRecords / (float)gp.rows);
        //        }
        //        if (gp.page > totalPages)
        //        {
        //            gp.page = totalPages;
        //        }
        //        var JsonData = new
        //        {
        //            page = gp.page,
        //            rows = jsonData,
        //            records = totalRecords,
        //            total = totalPages
        //        };
        //        return Json(JsonData, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}
        //public ActionResult P2BGrid(P2BGrid_Parameters gp)
        //{
        //    try
        //    {
        //        DataBaseContext db = new DataBaseContext();
        //        int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
        //        int pageSize = gp.rows;
        //        int totalPages = 0;
        //        int totalRecords = 0;
        //        var jsonData = (Object)null;
        //        IEnumerable<Location> Location = null;
        //        if (gp.IsAutho == true)
        //        {
        //            Location = db.Location.Include(e => e.Type).Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
        //        }
        //        else
        //        {
        //            Location = db.Location.Include(e => e.Type).AsNoTracking().ToList();
        //        }

        //        IEnumerable<Location> IE;
        //        if (!string.IsNullOrEmpty(gp.searchField))
        //        {
        //            IE = Location;
        //            if (gp.searchOper.Equals("eq"))
        //            {
        //                jsonData = IE.Select(a => new { a.Id, a.LocationObj.LocCode, a.LocationObj.LocDesc }).Where((e => (e.Id.ToString() == gp.searchString) || (e.LocationObj.LocCode.ToLower() == gp.searchString.ToLower()) || (e.LocationObj.LocDesc.ToLower() == gp.searchString.ToLower()))).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.LocationObj.LocCode, a.LocationObj.LocDesc, a.Type != null ? Convert.ToString(a.Type.LookupVal) : "" }).ToList();
        //            }
        //            totalRecords = IE.Count();
        //        }
        //        else
        //        {
        //            IE = Location;
        //            Func<Location, dynamic> orderfuc;
        //            if (gp.sidx == "Id")
        //            {
        //                orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
        //            }
        //            else
        //            {
        //                orderfuc = (c => gp.sidx == "LocationObj.LocCode" ? c.LocationObj.LocCode :
        //                                 gp.sidx == "LocationObj.LocDesc" ? c.LocationObj.LocDesc :
        //                                 gp.sidx == "Type" ? c.Type.LookupVal : "");
        //            }
        //            if (gp.sord == "asc")
        //            {
        //                IE = IE.OrderBy(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.LocationObj.LocCode), Convert.ToString(a.LocationObj.LocDesc), a.Type != null ? Convert.ToString(a.Type.LookupVal) : "" }).ToList();
        //            }
        //            else if (gp.sord == "desc")
        //            {
        //                IE = IE.OrderByDescending(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.LocationObj.LocCode), Convert.ToString(a.LocationObj.LocDesc), a.Type != null ? Convert.ToString(a.Type.LookupVal) : "" }).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.LocationObj.LocCode, a.LocationObj.LocDesc, a.Type != null ? Convert.ToString(a.Type.LookupVal) : "" }).ToList();
        //            }
        //            totalRecords = Location.Count();
        //        }
        //        if (totalRecords > 0)
        //        {
        //            totalPages = (int)Math.Ceiling((float)totalRecords / (float)gp.rows);
        //        }
        //        if (gp.page > totalPages)
        //        {
        //            gp.page = totalPages;
        //        }
        //        var JsonData = new
        //        {
        //            page = gp.page,
        //            rows = jsonData,
        //            records = totalRecords,
        //            total = totalPages
        //        };
        //        return Json(JsonData, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}


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
                IEnumerable<Location> Location = null;
                if (gp.IsAutho == true)
                {
                    Location = db.Location.Include(e => e.Type).Where(e => e.DBTrack.IsModified == false).AsNoTracking().ToList();
                }
                else
                {
                    FilterSession.Session a = new FilterSession.Session();
                    var b = a.Check_flow();
                    if (b != null)
                    {
                        if (b.type == "master")
                        {
                            Location = db.Company.Include(e => e.Location).Where(e => e.Id == b.comp_code).SelectMany(e => e.Location).ToList();
                        }
                        else
                        {
                            Location = db.Location.Include(e => e.LocationObj).ToList();
                        }
                    }
                    else
                    {
                        Location = db.Location.Include(e => e.LocationObj).ToList();
                    }
                    //if (Session["object"] != null && Session["object"]!="")
                    //{
                    //    if (Session["object"].ToString() == "object")
                    //    {
                    //        Location =db.Location.ToList();
                    //    }
                    //    else if (Session["object"].ToString() == "master")
                    //    {
                    //        var id = Convert.ToInt32(Session["CompCode"]);

                    //        if (id != null && id != 0)
                    //        {
                    //            Location = db.Company.Include(e => e.Location).Where(e => e.Id == id).SelectMany(e => e.Location).ToList();
                    //        }
                    //    }
                    //}
                    //else
                    //{
                    //    Location = db.Location.ToList();
                    //}
                }

                IEnumerable<Location> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = Location;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.LocationObj.LocCode.ToString().Contains(gp.searchString))
                               || (e.LocationObj.LocDesc.ToUpper().Contains(gp.searchString.ToUpper()))
                               || (e.Id.ToString().Contains(gp.searchString.ToUpper()))
                               ).Select(a => new Object[] { a.LocationObj.LocCode, a.LocationObj.LocDesc, a.Id }).ToList();
                        // jsonData = IE.Select(a => new { a.LocationObj.LocCode, a.LocationObj.LocDesc, a.Id }).Where((e => (e.Id.ToString() == gp.searchString) || (e.LocCode.ToLower() == gp.searchString.ToLower()) || (e.LocDesc.ToLower() == gp.searchString.ToLower()))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.LocationObj.LocCode, a.LocationObj.LocDesc, a.Id, a.Type != null ? Convert.ToString(a.Type.LookupVal) : "" }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = Location;
                    Func<Location, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "LocCode" ? c.LocationObj.LocCode :
                                         gp.sidx == "LocDesc" ? c.LocationObj.LocDesc :
                                         gp.sidx == "Type" ? c.Type.LookupVal : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.LocationObj != null ? a.LocationObj.LocCode : "", a.LocationObj != null ? a.LocationObj.LocDesc : "", a.Id, a.Type != null ? Convert.ToString(a.Type.LookupVal) : "" }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.LocationObj.LocCode), Convert.ToString(a.LocationObj.LocDesc), a.Id, a.Type != null ? Convert.ToString(a.Type.LookupVal) : "" }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.LocationObj.LocCode, a.LocationObj.LocDesc, a.Id, a.Type != null ? Convert.ToString(a.Type.LookupVal) : "" }).ToList();
                    }
                    totalRecords = Location.Count();
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
                List<string> Msg = new List<string>();
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


        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    Location Locations = db.Location.Include(e => e.Address)
                                                       .Include(e => e.ContactDetails)
                                                       .Include(e => e.Type).Where(e => e.Id == data).SingleOrDefault();

                    Address add = Locations.Address;
                    ContactDetails conDet = Locations.ContactDetails;
                    LookupValue val = Locations.Type;

                    if (Locations.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = Locations.DBTrack.CreatedBy != null ? Locations.DBTrack.CreatedBy : null,
                                CreatedOn = Locations.DBTrack.CreatedOn != null ? Locations.DBTrack.CreatedOn : null,
                                IsModified = Locations.DBTrack.IsModified == true ? true : false
                            };
                            Locations.DBTrack = dbT;
                            db.Entry(Locations).State = System.Data.Entity.EntityState.Modified;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, Locations.DBTrack);
                            DT_Location DT_Corp = (DT_Location)rtn_Obj;
                            DT_Corp.Address_Id = Locations.Address == null ? 0 : Locations.Address.Id;
                            DT_Corp.Type_Id = Locations.Type == null ? 0 : Locations.Type.Id;
                            DT_Corp.ContactDetails_Id = Locations.ContactDetails == null ? 0 : Locations.ContactDetails.Id;
                            db.Create(DT_Corp);
                            await db.SaveChangesAsync();
                            ts.Complete();
                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            try
                            {
                                DBTrack dbT = new DBTrack
                                {
                                    Action = "D",
                                    ModifiedBy = SessionManager.UserName,
                                    ModifiedOn = DateTime.Now,
                                    CreatedBy = Locations.DBTrack.CreatedBy != null ? Locations.DBTrack.CreatedBy : null,
                                    CreatedOn = Locations.DBTrack.CreatedOn != null ? Locations.DBTrack.CreatedOn : null,
                                    IsModified = Locations.DBTrack.IsModified == true ? false : false//,
                                };

                                db.Entry(Locations).State = System.Data.Entity.EntityState.Deleted;
                                var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dbT);
                                DT_Location DT_Corp = (DT_Location)rtn_Obj;
                                DT_Corp.Address_Id = add == null ? 0 : add.Id;
                                DT_Corp.Type_Id = val == null ? 0 : val.Id;
                                DT_Corp.ContactDetails_Id = conDet == null ? 0 : conDet.Id;
                                db.Create(DT_Corp);
                                await db.SaveChangesAsync();
                                ts.Complete();
                                Msg.Add("  Data removed successfully.  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                            }
                            catch (RetryLimitExceededException /* dex */)
                            {
                                Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //eturn this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
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


        [HttpPost]
        public ActionResult GetLookupDetailsAddress(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.Address.Include(e => e.Country).Include(e => e.State).Include(e => e.StateRegion)
                    .Include(e => e.District).Include(e => e.Taluka).Include(e => e.City).Include(e => e.Area).ToList();
                IEnumerable<Address> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.Address.ToList().Where(d => d.FullAddress.Contains(data));

                }
                else
                {
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullAddress }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.Address3 }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }

        }




        //public ActionResult GetLookupDetails(string data)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var fall = db.Address.Include(e => e.Country).Include(e => e.State).Include(e => e.StateRegion)
        //            .Include(e => e.District).Include(e => e.Taluka).Include(e => e.City).Include(e => e.Area).ToList();
        //        IEnumerable<Address> all;
        //        if (!string.IsNullOrEmpty(data))
        //        {
        //            all = db.Address.ToList().Where(d => d.FullAddress.Contains(data));

        //        }
        //        else
        //        {
        //            var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullAddress }).Distinct();
        //            var result_1 = (from c in fall
        //                            select new { c.Id, c.LocationCode, c.LocationName });
        //            return Json(r, JsonRequestBehavior.AllowGet);
        //        }
        //        var result = (from c in all
        //                      select new { c.Id, c.Address3 }).Distinct();
        //        return Json(result, JsonRequestBehavior.AllowGet);
        //    }
        //    return View();
        //}


        /* -------------------------- Incharge ----------------------*/
        //public ActionResult GetLookupIncharge(string data)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var fall = db.NameDetails.ToList();
        //        IEnumerable<NameDetails> all;
        //        if (!string.IsNullOrEmpty(data))
        //        {
        //            all = db.NameDetails.ToList().Where(d => d.FullNameFML.Contains(data));
        //        }
        //        else
        //        {

        //            var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullNameFML }).Distinct();
        //            return Json(r, JsonRequestBehavior.AllowGet);
        //        }
        //        var result = (from c in all
        //                      select new { c.Id, c.FullNameFML }).Distinct();
        //        return Json(result, JsonRequestBehavior.AllowGet);
        //    }

        //}

        public ActionResult GetLookupIncharge(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.Employee
                    .Include(e => e.EmpName)
                    .Include(e => e.ServiceBookDates)
                    .Where(e => e.ServiceBookDates.ServiceLastDate == null).ToList();
                IEnumerable<Employee> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.Employee.ToList().Where(d => d.EmpCode.Contains(data));
                }
                else
                {
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.EmpCode }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }


        /* ---------------------------- Details Contact -----------------------*/

        public ActionResult GetLookupDetailsContact(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.ContactDetails.Include(e => e.ContactNumbers).ToList();
                IEnumerable<ContactDetails> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.ContactDetails.ToList().Where(d => d.FullContactDetails.Contains(data));

                }
                else
                {
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullContactDetails }).Distinct();
                    //var result_1 = (from c in fall
                    //                select new { c.Id, c.LocationCode, c.LocationName });
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.FullContactDetails }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            // return View();
        }


        /*----------------- edit details--------------------*/

        public ActionResult Editcontactdetails_partial(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var r = (from ca in db.ContactDetails
                         .Where(e => e.Id == data)
                         select new
                         {
                             Id = ca.Id,
                             EmailId = ca.EmailId,
                             FaxNo = ca.FaxNo,
                             Website = ca.Website
                         }).ToList();

                var a = db.ContactDetails.Include(e => e.ContactNumbers).Where(e => e.Id == data).SingleOrDefault();
                var b = a.ContactNumbers;

                var r1 = (from s in b
                          select new
                          {
                              Id = s.Id,
                              FullContactNumbers = s.FullContactNumbers
                          }).ToList();
                TempData["RowVersion"] = db.ContactDetails.Find(data).RowVersion;
                return Json(new object[] { r, r1 }, JsonRequestBehavior.AllowGet);
            }
        }

        /*-----------------------  */


        public ActionResult EditIncharge_partial(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var r = (from ca in db.NameDetails
                         .Where(e => e.Id == data)
                         select new
                         {
                             EmpFName = ca.EmpFName,
                             EmpMName = ca.EmpMName,
                             EmpLName = ca.EmpLName,
                             FatherFName = ca.FatherFName,
                             FatherLName = ca.FatherLName,
                             FatherMName = ca.FatherMName,
                             HusbandFName = ca.HusbandFName,
                             HusbandLName = ca.HusbandLName,
                             HusbandMName = ca.HusbandMName,
                             MotherFName = ca.MotherFName,
                             MotherLName = ca.MotherLName,
                             MotherMName = ca.MotherMName,
                             PreviousFName = ca.PreviousFName,
                             PreviousLName = ca.PreviousLName,
                             PreviousMName = ca.PreviousMName,
                             Id = data
                         }).ToList();
                TempData["RowVersion"] = db.Location.Find(data).RowVersion;
                return Json(new object[] { r }, JsonRequestBehavior.AllowGet);
            }
        }




        /*------------Edit save --------------------------*/

        //  [HttpPost]
        //public async Task<ActionResult> EditSave(Location c, int data, FormCollection form) // Edit submit
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        List<string> Msg = new List<string>();
        //        try
        //        {
        //            string Addrs = form["AddressList"] == "0" ? "" : form["AddressList"];
        //            string ContactDetails = form["ContactDetailsList"] == "0" ? "" : form["ContactDetailsList"];
        //            string OpeningDate = form["OpeningDate"] == "0" ? "" : form["OpeningDate"];
        //            string LocationObj = form["Location_Obj_DDL"] == "0" ? "" : form["Location_Obj_DDL"];

        //            string Incharge_DDL = form["Incharge_DDL"] == "0" ? "" : form["Incharge_DDL"];
        //            bool Auth = form["Autho_Allow"] == "true" ? true : false;

        //            if (Addrs != null)
        //            {
        //                if (Addrs != "")
        //                {
        //                    int AddId = Convert.ToInt32(Addrs);
        //                    var val = db.Address.Include(e => e.Area)
        //                                        .Include(e => e.City)
        //                                        .Include(e => e.Country)
        //                                        .Include(e => e.District)
        //                                        .Include(e => e.State)
        //                                        .Include(e => e.StateRegion)
        //                                        .Include(e => e.Taluka)
        //                                        .Where(e => e.Id == AddId).SingleOrDefault();
        //                    c.Address = val;
        //                }
        //            }

        //            if (ContactDetails != null)
        //            {
        //                if (ContactDetails != "")
        //                {
        //                    int ContId = Convert.ToInt32(ContactDetails);
        //                    var val = db.ContactDetails.Include(e => e.ContactNumbers)
        //                                        .Where(e => e.Id == ContId).SingleOrDefault();
        //                    c.ContactDetails = val;
        //                }
        //            }
        //            if (OpeningDate != null)
        //            {
        //                if (OpeningDate != "")
        //                {

        //                    var val = DateTime.Parse(OpeningDate);
        //                    c.OpeningDate = val;
        //                }
        //            }

        //            if (c.LocationObj != null)
        //            {
        //                var LocObj = db.Location.Select(e => e.LocationObj).Where(e => e.Id == data).SingleOrDefault();
        //                c.LocationObj = LocObj;
        //            }

        //            List<HolidayCalendar> lookupLang = new List<HolidayCalendar>();

        //            string HolidayCalendar_DDL = form["HOCalendarList"] == "" ? null : form["HOCalendarList"];
        //            if (HolidayCalendar_DDL != null)
        //            {
        //                var ids = Utility.StringIdsToListIds(HolidayCalendar_DDL);
        //                foreach (var ca in ids)
        //                {
        //                    var Lookup_val = db.HolidayCalendar.Find(ca);

        //                    lookupLang.Add(Lookup_val);
        //                    c.HolidayCalendar = lookupLang;
        //                }
        //            }
        //            else
        //            {
        //                c.HolidayCalendar = null;
        //            }

        //            List<WeeklyOffCalendar> lookupLang1 = new List<WeeklyOffCalendar>();
        //            string WeeklyOffCalendar_DDL = form["WOCalendarList"] == "" ? null : form["WOCalendarList"];

        //            if (WeeklyOffCalendar_DDL != null)
        //            {
        //                var ids = Utility.StringIdsToListIds(WeeklyOffCalendar_DDL);
        //                foreach (var ca in ids)
        //                {
        //                    var Lookup_val = db.WeeklyOffCalendar.Find(ca);

        //                    lookupLang1.Add(Lookup_val);
        //                    c.WeeklyOffCalendar = lookupLang1;
        //                }
        //            }
        //            else
        //            {
        //                c.WeeklyOffCalendar = null;
        //            }



        //            if (Auth == false)
        //            {


        //                if (ModelState.IsValid)
        //                {
        //                    try
        //                    {
        //                        //DbContextTransaction transaction = db.Database.BeginTransaction();

        //                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                        {
        //                            Location blog = null; // to retrieve old data
        //                            DbPropertyValues originalBlogValues = null;

        //                            using (var context = new DataBaseContext())
        //                            {
        //                                blog = context.Location.Where(e => e.Id == data)
        //                                                        .Include(e => e.Address).Include(e => e.LocationObj)
        //                                                        .Include(e => e.ContactDetails).SingleOrDefault();
        //                                originalBlogValues = context.Entry(blog).OriginalValues;
        //                            }

        //                            c.DBTrack = new DBTrack
        //                            {
        //                                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                                Action = "M",
        //                                ModifiedBy = SessionManager.UserName,
        //                                ModifiedOn = DateTime.Now
        //                            };
        //                            //	db.Entry(c.LocationObj).State = System.Data.Entity.EntityState.Modified;
        //                            db.SaveChanges();

        //                            int a = EditS(LocationObj, Incharge_DDL, HolidayCalendar_DDL, WeeklyOffCalendar_DDL, Addrs, ContactDetails, data, c, c.DBTrack);



        //                            using (var context = new DataBaseContext())
        //                            {
        //                                //c.Id = data;

        //                                /// DBTrackFile.DBTrackSave("Core/P2b.Global", "M", blog, c, "Location", c.DBTrack);
        //                                //PropertyInfo[] fi = null;
        //                                //Dictionary<string, object> rt = new Dictionary<string, object>();
        //                                //fi = c.GetType().GetProperties();
        //                                ////foreach (var Prop in fi)
        //                                ////{
        //                                ////    if (Prop.LocationObj.LocDesc != "Id" && Prop.LocationObj.LocDesc != "DBTrack" && Prop.LocationObj.LocDesc != "RowVersion")
        //                                ////    {
        //                                ////        rt.Add(Prop.LocationObj.LocDesc, Prop.GetValue(c));
        //                                ////    }
        //                                ////}
        //                                //rt = blog.DetailedCompare(c);
        //                                //rt.Add("Orig_Id", c.Id);
        //                                //rt.Add("Action", "M");
        //                                //rt.Add("DBTrack", c.DBTrack);
        //                                //rt.Add("RowVersion", c.RowVersion);
        //                                //var aa = DBTrackFile.GetInstance("Core/P2b.Global", "DT_" + "Location", rt);
        //                                //DT_Location d = (DT_Location)aa;
        //                                //db.DT_Location.Add(d);
        //                                //db.SaveChanges();

        //                                //To save data in history table 
        //                                //var Obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, c, "Location", c.DBTrack);
        //                                //DT_Location DT_Corp = (DT_Location)Obj;
        //                                //db.DT_Location.Add(DT_Corp);
        //                                //db.SaveChanges();\


        //                                var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
        //                                DT_Location DT_Corp = (DT_Location)obj;
        //                                DT_Corp.Address_Id = blog.Address == null ? 0 : blog.Address.Id;
        //                                DT_Corp.ContactDetails_Id = blog.ContactDetails == null ? 0 : blog.ContactDetails.Id;
        //                                db.Create(DT_Corp);
        //                                db.SaveChanges();
        //                            }
        //                            await db.SaveChangesAsync();
        //                            ts.Complete();
        //                            Msg.Add("  Record Updated  ");
        //                            return Json(new Utility.JsonReturnClass { Id = c.Id, Val = c.LocationObj.LocDesc, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                            //return Json(new Object[] { c.Id, c.LocationObj.LocDesc, "Record Updated", JsonRequestBehavior.AllowGet });

        //                        }
        //                    }
        //                    catch (DbUpdateConcurrencyException ex)
        //                    {
        //                        var entry = ex.Entries.Single();
        //                        var clientValues = (Location)entry.Entity;
        //                        var databaseEntry = entry.GetDatabaseValues();
        //                        if (databaseEntry == null)
        //                        {
        //                            Msg.Add(" Unable to save changes. The record was deleted by another user. ");
        //                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                            //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
        //                        }
        //                        else
        //                        {
        //                            var databaseValues = (Location)databaseEntry.ToObject();
        //                            c.RowVersion = databaseValues.RowVersion;

        //                        }
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        LogFile Logfile = new LogFile();
        //                        ErrorLog Err = new ErrorLog()
        //                        {
        //                            ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
        //                            ExceptionMessage = ex.Message,
        //                            ExceptionStackTrace = ex.StackTrace,
        //                            LineNo = Convert.ToString(new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
        //                            LogTime = DateTime.Now
        //                        };
        //                        Logfile.CreateLogFile(Err);
        //                        Msg.Add(ex.Message);
        //                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                    }
        //                    Msg.Add("Record modified by another user.So refresh it and try to save again.");
        //                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                    // return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
        //                }
        //            }
        //            else
        //            {
        //                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                {

        //                    Location blog = null; // to retrieve old data
        //                    DbPropertyValues originalBlogValues = null;
        //                    Location Old_Corp = null;

        //                    using (var context = new DataBaseContext())
        //                    {
        //                        blog = context.Location.Where(e => e.Id == data).SingleOrDefault();
        //                        originalBlogValues = context.Entry(blog).OriginalValues;
        //                    }
        //                    c.DBTrack = new DBTrack
        //                    {
        //                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                        Action = "M",
        //                        IsModified = blog.DBTrack.IsModified == true ? true : false,
        //                        ModifiedBy = SessionManager.UserName,
        //                        ModifiedOn = DateTime.Now
        //                    };
        //                    Location corp = new Location()
        //                    {
        //                        //LocationObj.LocCode = c.LocationObj.LocCode,
        //                        //LocationObj.LocDesc = c.LocationObj.LocDesc,
        //                        OpeningDate = c.OpeningDate,
        //                        Id = data,
        //                        DBTrack = c.DBTrack,
        //                        RowVersion = (Byte[])TempData["RowVersion"]
        //                    };


        //                    //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

        //                    using (var context = new DataBaseContext())
        //                    {
        //                        var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, corp, "Location", c.DBTrack);

        //                        Old_Corp = context.Location.Where(e => e.Id == data)
        //                            .Include(e => e.Address).Include(e => e.ContactDetails).SingleOrDefault();
        //                        DT_Location DT_Corp = (DT_Location)obj;
        //                        DT_Corp.Address_Id = DBTrackFile.ValCompare(Old_Corp.Address, c.Address);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
        //                        DT_Corp.ContactDetails_Id = DBTrackFile.ValCompare(Old_Corp.ContactDetails, c.ContactDetails); //Old_Corp.ContactDetails == c.ContactDetails ? 0 : Old_Corp.ContactDetails == null && c.ContactDetails != null ? c.ContactDetails.Id : Old_Corp.ContactDetails.Id;
        //                        db.Create(DT_Corp);
        //                        //db.SaveChanges();
        //                    }
        //                    blog.DBTrack = c.DBTrack;
        //                    db.Location.Attach(blog);
        //                    db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
        //                    db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //                    db.SaveChanges();
        //                    ts.Complete();
        //                    Msg.Add("  Record Updated  ");
        //                    return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = c.LocationObj.LocDesc, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                    //return Json(new Object[] { blog.Id, c.LocationObj.LocDesc, "Record Updated", JsonRequestBehavior.AllowGet });
        //                }

        //            }
        //            return View();
        //        }
        //        catch (Exception ex)
        //        {
        //            LogFile Logfile = new LogFile();
        //            ErrorLog Err = new ErrorLog()
        //            {
        //                ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
        //                ExceptionMessage = ex.Message,
        //                ExceptionStackTrace = ex.StackTrace,
        //                LineNo = Convert.ToString(new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
        //                LogTime = DateTime.Now
        //            };
        //            Logfile.CreateLogFile(Err);
        //            Msg.Add(ex.Message);
        //            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //}
        [HttpPost]
        public async Task<ActionResult> EditSave(Location c, int data, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    string BusinessCategory = form["BusinessCategory_DDL"] == "0" ? "" : form["BusinessCategory_DDL"];
                    string Addrs = form["AddressList"] == "0" ? "" : form["AddressList"];
                    string ContactDetails = form["ContactDetailsList"] == "0" ? "" : form["ContactDetailsList"];
                    string OpeningDate = form["OpeningDate"] == "0" ? "" : form["OpeningDate"];
                    string LocationObj = form["Location_Obj_DDL"] == "0" ? "" : form["Location_Obj_DDL"];
                    string UnitId = form["UnitId"] == "0" ? "" : form["UnitId"];
                    //string Incharge_DDL = form["Incharge_DDL"] == "0" ? "" : form["Incharge_DDL"];
                    string Incharge_DDL = form["Incharge_Id"] == "0" ? "" : form["Incharge_Id"];
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;

                    if (LocationObj == null || LocationObj == "")
                    {
                        Msg.Add("Please Select Location object.  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    //if (Addrs != null)
                    //{
                    //    if (Addrs != "")
                    //    {
                    //        int AddId = Convert.ToInt32(Addrs);
                    //        var val = db.Address.Include(e => e.Area)
                    //                            .Include(e => e.City)
                    //                            .Include(e => e.Country)
                    //                            .Include(e => e.District)
                    //                            .Include(e => e.State)
                    //                            .Include(e => e.StateRegion)
                    //                            .Include(e => e.Taluka)
                    //                            .Where(e => e.Id == AddId).SingleOrDefault();
                    //        c.Address = val;
                    //    }
                    //}
                    if (Addrs != null)
                    {
                        if (Addrs != "")
                        {
                            var val = db.Address.Find(int.Parse(Addrs));
                            c.Address = val;

                            var add = db.Location.Include(e => e.Address).Where(e => e.Id == data).SingleOrDefault();
                            IList<Location> addressdetails = null;
                            if (add.Address != null)
                            {
                                addressdetails = db.Location.Where(x => x.Address.Id == add.Address.Id && x.Id == data).ToList();
                            }
                            else
                            {
                                addressdetails = db.Location.Where(x => x.Id == data).ToList();
                            }
                            if (addressdetails != null)
                            {
                                foreach (var s in addressdetails)
                                {
                                    s.Address = c.Address;
                                    db.Location.Attach(s);
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
                        var addressdetails = db.Location.Include(e => e.Address).Where(x => x.Id == data).ToList();
                        foreach (var s in addressdetails)
                        {
                            s.Address = null;
                            db.Location.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                    if (Incharge_DDL != null)
                    {
                        if (Incharge_DDL != "")
                        {
                            var val = db.Employee.Find(int.Parse(Incharge_DDL));
                            c.Incharge = val;

                            var type = db.Location.Include(e => e.Incharge).Where(e => e.Id == data).SingleOrDefault();
                            IList<Location> typedetails = null;
                            if (type.Incharge != null)
                            {
                                typedetails = db.Location.Where(x => x.Incharge.Id == type.Incharge.Id && x.Id == data).ToList();
                            }
                            else
                            {
                                typedetails = db.Location.Where(x => x.Id == data).ToList();
                            }
                            //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                            foreach (var s in typedetails)
                            {
                                s.Incharge = c.Incharge;
                                db.Location.Attach(s);
                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                //await db.SaveChangesAsync();
                                db.SaveChanges();
                                TempData["RowVersion"] = s.RowVersion;
                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                            }
                        }
                        else
                        {
                            var BusiTypeDetails = db.Location.Include(e => e.Incharge).Where(x => x.Id == data).ToList();
                            foreach (var s in BusiTypeDetails)
                            {
                                s.Incharge = null;
                                db.Location.Attach(s);
                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                //await db.SaveChangesAsync();
                                db.SaveChanges();
                                TempData["RowVersion"] = s.RowVersion;
                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                            }
                        }
                    }
                    //bucat
                    if (BusinessCategory != null)
                    {
                        if (BusinessCategory != "")
                        {
                            var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "408").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(BusinessCategory)).FirstOrDefault();  //db.LookupValue.Find(int.Parse(BusinessCategory));
                            c.BusinessCategory = val;

                            var type = db.Location.Include(e => e.BusinessCategory).Where(e => e.Id == data).SingleOrDefault();
                            IList<Location> typedetails = null;
                            if (type.BusinessCategory != null)
                            {
                                typedetails = db.Location.Where(x => x.BusinessCategory.Id == type.BusinessCategory.Id && x.Id == data).ToList();
                            }
                            else
                            {
                                typedetails = db.Location.Where(x => x.Id == data).ToList();
                            }
                            //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                            foreach (var s in typedetails)
                            {
                                s.BusinessCategory = c.BusinessCategory;
                                db.Location.Attach(s);
                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                //await db.SaveChangesAsync();
                                db.SaveChanges();
                                TempData["RowVersion"] = s.RowVersion;
                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                            }
                        }
                        else
                        {
                            var BusiTypeDetails = db.Location.Include(e => e.BusinessCategory).Where(x => x.Id == data).ToList();
                            foreach (var s in BusiTypeDetails)
                            {
                                s.BusinessCategory = null;
                                db.Location.Attach(s);
                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                //await db.SaveChangesAsync();
                                db.SaveChanges();
                                TempData["RowVersion"] = s.RowVersion;
                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                            }
                        }
                    }
                    //if (ContactDetails != null)
                    //{
                    //    if (ContactDetails != "")
                    //    {
                    //        int ContId = Convert.ToInt32(ContactDetails);
                    //        var val = db.ContactDetails.Include(e => e.ContactNumbers)
                    //                            .Where(e => e.Id == ContId).SingleOrDefault();
                    //        c.ContactDetails = val;
                    //    }
                    //}
                    if (ContactDetails != null)
                    {
                        if (ContactDetails != "")
                        {
                            var val = db.ContactDetails.Find(int.Parse(ContactDetails));
                            c.ContactDetails = val;

                            var add = db.Location.Include(e => e.ContactDetails).Where(e => e.Id == data).SingleOrDefault();
                            IList<Location> contactsdetails = null;
                            if (add.ContactDetails != null)
                            {
                                contactsdetails = db.Location.Where(x => x.ContactDetails.Id == add.ContactDetails.Id && x.Id == data).ToList();
                            }
                            else
                            {
                                contactsdetails = db.Location.Where(x => x.Id == data).ToList();
                            }
                            foreach (var s in contactsdetails)
                            {
                                s.ContactDetails = c.ContactDetails;
                                db.Location.Attach(s);
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
                        var contactsdetails = db.Location.Include(e => e.ContactDetails).Where(x => x.Id == data).ToList();
                        foreach (var s in contactsdetails)
                        {
                            s.ContactDetails = null;
                            db.Location.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }

                    if (OpeningDate != null)
                    {
                        if (OpeningDate != "")
                        {

                            var val = DateTime.Parse(OpeningDate);
                            c.OpeningDate = val;
                        }
                    }

                    //if (UnitId != null)
                    //{
                    //    if (UnitId != "")
                    //    {
                    //        c.UnitId = UnitId;
                    //    }
                    //}

                    if (c.LocationObj != null)
                    {
                        var LocObj = db.Location.Select(e => e.LocationObj).Where(e => e.Id == data).SingleOrDefault();
                        c.LocationObj = LocObj;
                    }

                    if (LocationObj != null && LocationObj != "")
                    {

                        var val = db.LocationObj.Find(int.Parse(LocationObj));
                        c.LocationObj = val;

                        var type = db.Location.Include(e => e.LocationObj).Where(e => e.Id == data).SingleOrDefault();
                        IList<Location> typedetails = null;
                        if (type.LocationObj != null)
                        {
                            typedetails = db.Location.Where(x => x.LocationObj.Id == type.LocationObj.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            typedetails = db.Location.Where(x => x.Id == data).ToList();
                        }
                        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                        foreach (var s in typedetails)
                        {
                            s.LocationObj = c.LocationObj;
                            db.Location.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                    else
                    {
                        var BusiTypeDetails = db.Location.Include(e => e.LocationObj).Where(x => x.Id == data).ToList();
                        foreach (var s in BusiTypeDetails)
                        {
                            s.LocationObj = null;
                            db.Location.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }

                    var db_data = db.Location.Include(e => e.HolidayCalendar).Where(e => e.Id == data).SingleOrDefault();
                    List<HolidayCalendar> lookupLang = new List<HolidayCalendar>();
                    string HolidayCalendar_DDL = form["HOCalendarList"] == "" ? null : form["HOCalendarList"];
                    if (HolidayCalendar_DDL != null)
                    {
                        var ids = Utility.StringIdsToListIds(HolidayCalendar_DDL);
                        foreach (var ca in ids)
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

                    //string HolidayCalendar_DDL = form["HOCalendarList"] == "" ? null : form["HOCalendarList"];

                    //                  if (HolidayCalendar_DDL != null)
                    //                  {
                    //                      var ids = Utility.StringIdsToListIds(HolidayCalendar_DDL);
                    //                      var Holidaycallist = new List<HolidayCalendar>();
                    //                      foreach (var item in ids)
                    //                      {

                    //                          int HolidayListid = Convert.ToInt32(item);
                    //                          var val = db.HolidayCalendar.Include(e => e.HolidayList).Include(e => e.HoliCalendar).Include(e => e.HoliCalendar.Name).Where(e => e.Id == HolidayListid).SingleOrDefault();
                    //                          if (val != null)
                    //                          {
                    //                              Holidaycallist.Add(val);
                    //                          }
                    //                      }
                    //                      c.HolidayCalendar = Holidaycallist;
                    //                  }



                    var dbdata = db.Location.Include(e => e.WeeklyOffCalendar).Where(e => e.Id == data).SingleOrDefault();
                    List<WeeklyOffCalendar> lookupLang1 = new List<WeeklyOffCalendar>();
                    string WeeklyOffCalendar_DDL = form["WOCalendarList"] == "" ? null : form["WOCalendarList"];
                    if (WeeklyOffCalendar_DDL != null)
                    {
                        var ids = Utility.StringIdsToListIds(WeeklyOffCalendar_DDL);
                        foreach (var ca in ids)
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


                    if (Auth == false)
                    {
                        if (ModelState.IsValid)
                        {
                            try
                            {
                                //DbContextTransaction transaction = db.Database.BeginTransaction();

                                // using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))

                                using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, new System.TimeSpan(0, 30, 0)))
                                {
                                    Location blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    //using (var context = new DataBaseContext())
                                    //{
                                        blog = db.Location.Where(e => e.Id == data)
                                                                .Include(e => e.Address).Include(e => e.LocationObj)
                                                                .Include(e => e.ContactDetails).SingleOrDefault();
                                        originalBlogValues = db.Entry(blog).OriginalValues;
                                   // }

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
                                    var s = db.Location.Include(e => e.HolidayCalendar).Include(x=>x.LocationObj).Where(e => e.Id == data).FirstOrDefault();
                                    //foreach (var s in m1)
                                    //{
                                        // s.AppraisalPeriodCalendar = null;
                                      s.ContactDetails = c.ContactDetails;
                                            s.Address = c.Address;
                                            s.Incharge = c.Incharge;
                                            s.BusinessCategory = c.BusinessCategory;
                                            s.LocationObj = c.LocationObj;
                                            s.HolidayCalendar = db_data.HolidayCalendar;
                                            s.WeeklyOffCalendar = dbdata.WeeklyOffCalendar;
                                            s.OpeningDate = c.OpeningDate;
                                           // UnitId=c.UnitId,
                                            s.Id = data;
                                            s.DBTrack = c.DBTrack;

                                        db.Location.Attach(s);
                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                        //await db.SaveChangesAsync();
                                        db.ChangeTracker.DetectChanges();
                                        TempData["RowVersion"] = s.RowVersion;
                                       // db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                  //  }
                                        var CurCorp = db.Location.Find(data);
                                        TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    ///db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {
                                        // c.DBTrack = dbT;
                                        //Location corp = new Location()
                                        //{
                                        //    //LocationObj.LocCode = c.LocationObj.LocCode,
                                        //    //LocationObj.LocDesc = c.LocationObj.LocDesc,
                                        //    ContactDetails = c.ContactDetails,
                                        //    Address = c.Address,
                                        //    Incharge = c.Incharge,
                                        //    BusinessCategory = c.BusinessCategory,
                                        //    LocationObj = c.LocationObj,
                                        //    HolidayCalendar = db_data.HolidayCalendar,
                                        //    WeeklyOffCalendar = dbdata.WeeklyOffCalendar,
                                        //    OpeningDate = c.OpeningDate,
                                        //   // UnitId=c.UnitId,
                                        //    Id = data,
                                        //    DBTrack = c.DBTrack
                                        //};


                                        //db.Location.Attach(corp);
                                        //db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                                        //db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];

                                        // return 1;
                                        //using (var context = new DataBaseContext())
                                        //{


                                            var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                            DT_Location DT_Corp = (DT_Location)obj;
                                            DT_Corp.Address_Id = blog.Address == null ? 0 : blog.Address.Id;
                                            DT_Corp.ContactDetails_Id = blog.ContactDetails == null ? 0 : blog.ContactDetails.Id;
                                            db.Create(DT_Corp);
                                            db.SaveChanges();
                                       // }

                                            bool GeoFenceAppl = db.GeoFencing.FirstOrDefault() != null ? true : false;
                                            if (GeoFenceAppl == true)
                                            {
                                                int Compid = int.Parse(Convert.ToString(Session["CompId"]));
                                                string connString = ConfigurationManager.ConnectionStrings["DataBaseContext"].ConnectionString;
                                                using (SqlConnection con = new SqlConnection(connString))
                                                {
                                                    using (SqlCommand cmd = new SqlCommand("Insert_EmpReporting_All", con))
                                                    {
                                                        cmd.CommandType = CommandType.StoredProcedure;

                                                        cmd.Parameters.Add("@CompCode", SqlDbType.VarChar).Value = db.Company.Where(e => e.Id == Compid).SingleOrDefault().Code;
                                                        cmd.Parameters.Add("@EmpCode", SqlDbType.VarChar).Value = "";

                                                        con.Open();
                                                        cmd.CommandTimeout = 3600;
                                                        cmd.ExecuteNonQuery();
                                                    }
                                                }
                                            }


                                        ts.Complete();
                                        Msg.Add("  Record Updated  ");
                                        //  return Json(new Utility.JsonReturnClass { Id = corp.Id, Val = corp.LocationObj.LocDesc, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        return Json(new Utility.JsonReturnClass { Id = s.Id, Val = s.LocationObj.LocDesc, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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
                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            Location blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            Location Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.Location.Where(e => e.Id == data).SingleOrDefault();
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
                            Location corp = new Location()
                            {
                                //LocationObj.LocCode = c.LocationObj.LocCode,
                                //LocationObj.LocDesc = c.LocationObj.LocDesc,
                                OpeningDate = c.OpeningDate,
                                Id = data,
                                DBTrack = c.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };


                            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, corp, "Location", c.DBTrack);

                                Old_Corp = context.Location.Where(e => e.Id == data)
                                    .Include(e => e.Address).Include(e => e.ContactDetails).SingleOrDefault();
                                DT_Location DT_Corp = (DT_Location)obj;
                                DT_Corp.Address_Id = DBTrackFile.ValCompare(Old_Corp.Address, c.Address);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
                                DT_Corp.ContactDetails_Id = DBTrackFile.ValCompare(Old_Corp.ContactDetails, c.ContactDetails); //Old_Corp.ContactDetails == c.ContactDetails ? 0 : Old_Corp.ContactDetails == null && c.ContactDetails != null ? c.ContactDetails.Id : Old_Corp.ContactDetails.Id;
                                db.Create(DT_Corp);
                                //db.SaveChanges();
                            }
                            blog.DBTrack = c.DBTrack;
                            db.Location.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("  Record Updated  ");
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = c.LocationObj.LocDesc, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { blog.Id, c.LocationObj.LocDesc, "Record Updated", JsonRequestBehavior.AllowGet });
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
                            //Location corp = db.Location.Find(auth_id);
                            //Location corp = db.Location.AsNoTracking().FirstOrDefault(e => e.Id == auth_id);

                            Location corp = db.Location.Include(e => e.Address)
                                .Include(e => e.ContactDetails)
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

                            db.Location.Attach(corp);
                            db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            //db.SaveChanges();
                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corp.DBTrack);
                            DT_Location DT_Corp = (DT_Location)rtn_Obj;
                            DT_Corp.Address_Id = corp.Address == null ? 0 : corp.Address.Id;
                            DT_Corp.ContactDetails_Id = corp.ContactDetails == null ? 0 : corp.ContactDetails.Id;
                            db.Create(DT_Corp);
                            await db.SaveChangesAsync();

                            ts.Complete();
                            Msg.Add("  Record Authorised");
                            return Json(new Utility.JsonReturnClass { Id = corp.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { corp.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else if (auth_action == "M")
                    {

                        Location Old_Corp = db.Location
                                                          .Include(e => e.Address)
                                                          .Include(e => e.ContactDetails).Where(e => e.Id == auth_id).SingleOrDefault();

                        //var W = db.DT_Location
                        //.Include(e => e.ContactDetails)
                        //.Include(e => e.Address)
                        //.Include(e => e.LocationObj)
                        //.Include(e => e.ContactDetails)
                        //.Where(e => e.Orig_Id == auth_id && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                        //(e => new
                        //{
                        //    DT_Id = e.Id,
                        //    LocationObj.LocCode = e.LocationObj.LocCode == null ? "" : e.LocationObj.LocCode,
                        //    LocationObj.LocDesc = e.LocationObj.LocDesc == null ? "" : e.LocationObj.LocDesc,
                        //    BusinessType_Val = e.LocationObj.Id == null ? "" : e.LocationObj.LookupVal,
                        //    Address_Val = e.Address.Id == null ? "" : e.Address.FullAddress,
                        //    Contact_Val = e.ContactDetails.Id == null ? "" : e.ContactDetails.FullContactDetails,
                        //}).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                        DT_Location Curr_Corp = db.DT_Location
                                                    .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                                    .OrderByDescending(e => e.Id)
                                                    .FirstOrDefault();

                        if (Curr_Corp != null)
                        {
                            Location corp = new Location();

                            string Addrs = Curr_Corp.Address_Id == null ? null : Curr_Corp.Address_Id.ToString();
                            string ContactDetails = Curr_Corp.ContactDetails_Id == null ? null : Curr_Corp.ContactDetails_Id.ToString();
                            //corp.LocationObj.LocDesc = Curr_Corp.LocationObj.LocDesc == null ? Old_Corp.LocationObj.LocDesc : Curr_Corp.LocationObj.LocDesc;
                            //corp.LocationObj.LocCode = Curr_Corp.LocationObj.LocCode == null ? Old_Corp.LocationObj.LocCode : Curr_Corp.LocationObj.LocCode;
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

                                        //	int a = EditS(Addrs, ContactDetails, auth_id, corp, corp.DBTrack);
                                        //var CurCorp = db.Location.Find(auth_id);
                                        //TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                        //db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                        //if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                        //{
                                        //    c.DBTrack = new DBTrack
                                        //    {
                                        //        CreatedBy = Old_Corp.DBTrack.CreatedBy == null ? null : Old_Corp.DBTrack.CreatedBy,
                                        //        CreatedOn = Old_Corp.DBTrack.CreatedOn == null ? null : Old_Corp.DBTrack.CreatedOn,
                                        //        Action = "M",
                                        //        ModifiedBy = Old_Corp.DBTrack.ModifiedBy == null ? null : Old_Corp.DBTrack.ModifiedBy,
                                        //        ModifiedOn = Old_Corp.DBTrack.ModifiedOn == null ? null : Old_Corp.DBTrack.ModifiedOn,
                                        //        AuthorizedBy = SessionManager.UserName,
                                        //        AuthorizedOn = DateTime.Now,
                                        //        IsModified = false
                                        //    };
                                        //    Location corp = new Location()
                                        //    {
                                        //        LocationObj.LocCode = c.LocationObj.LocCode,
                                        //        LocationObj.LocDesc = c.LocationObj.LocDesc,
                                        //        Id = Convert.ToInt32(auth_id),
                                        //        DBTrack = c.DBTrack
                                        //    };


                                        //    db.Location.Attach(corp);
                                        //    db.Entry(corp).State = System.Data.Entity.EntityState.Modified;

                                        //    // db.Entry(c.DBTrack).State = System.Data.Entity.EntityState.Modified;
                                        //    //db.SaveChanges();
                                        //    db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                        //    //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                        //    await db.SaveChangesAsync();
                                        //    //DisplayTrackedEntities(db.ChangeTracker);
                                        //    db.Entry(corp).State = System.Data.Entity.EntityState.Detached;
                                        //    ts.Complete();
                                        //    return Json(new Object[] { corp.Id, corp.LocationObj.LocDesc, "Record Updated", JsonRequestBehavior.AllowGet });
                                        //}

                                        await db.SaveChangesAsync();

                                        ts.Complete();
                                        Msg.Add("  Record Authorised");
                                        return Json(new Utility.JsonReturnClass { Id = corp.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        // return Json(new Object[] { corp.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
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
                                        corp.RowVersion = databaseValues.RowVersion;
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
                            //Location corp = db.Location.Find(auth_id);
                            Location corp = db.Location.AsNoTracking().Include(e => e.Address)
                                                                          .Include(e => e.ContactDetails).FirstOrDefault(e => e.Id == auth_id);

                            Address add = corp.Address;
                            ContactDetails conDet = corp.ContactDetails;

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

                            db.Location.Attach(corp);
                            db.Entry(corp).State = System.Data.Entity.EntityState.Deleted;


                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corp.DBTrack);
                            DT_Location DT_Corp = (DT_Location)rtn_Obj;
                            DT_Corp.Address_Id = corp.Address == null ? 0 : corp.Address.Id;
                            DT_Corp.ContactDetails_Id = corp.ContactDetails == null ? 0 : corp.ContactDetails.Id;
                            db.Create(DT_Corp);
                            await db.SaveChangesAsync();
                            db.Entry(corp).State = System.Data.Entity.EntityState.Detached;
                            ts.Complete();
                            Msg.Add(" Record Authorised ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            // return Json(new Object[] { "", "Record Authorised", JsonRequestBehavior.AllowGet });
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





        /*----- Contact Details Delete ********/


        [HttpPost]
        public ActionResult DeleteContactDetails(int? data, int? forwarddata)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                ContactDetails contDet = db.ContactDetails.Find(data);
                Location loc = db.Location.Find(forwarddata);
                try
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        loc.ContactDetails = null;
                        db.Entry(loc).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        ts.Complete();
                    }
                    Msg.Add("  Data removed successfully.  ");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    // return this.Json(new { msg = "Data deleted." });
                }

                catch (DataException /* dex */)
                {
                    Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists contact your system administrator." });
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



        /*------------------------------- Address Delete -------------------- */
        [HttpPost]
        public ActionResult AddressRemove(int? data, int? forwarddata)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                Address addrs = db.Address.Find(data);
                Location loc = db.Location.Find(forwarddata); try
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        loc.Address = null;
                        db.Entry(loc).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        ts.Complete();
                    }
                    Msg.Add("  Data removed successfully.  ");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //return Json(new Object[] { null, "Data Remove successfully." }, JsonRequestBehavior.AllowGet);
                }

                catch (DataException /* dex */)
                {
                    Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists contact your system administrator." });
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


        /*------------------------------- ContactDetails Delete -------------------- */
        [HttpPost]
        public ActionResult ContactDetailsRemove(int? data, int? forwarddata)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                ContactDetails contDet = db.ContactDetails.Find(data);
                Location loc = db.Location.Find(forwarddata);
                try
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        loc.ContactDetails = null;
                        db.Entry(loc).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        ts.Complete();
                    }
                    Msg.Add("  Data removed successfully.  ");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    // return Json(new Object[] { null, "Data Remove successfully." }, JsonRequestBehavior.AllowGet);

                }

                catch (DataException /* dex */)
                {
                    Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists contact your system administrator." });

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

       

    }
}