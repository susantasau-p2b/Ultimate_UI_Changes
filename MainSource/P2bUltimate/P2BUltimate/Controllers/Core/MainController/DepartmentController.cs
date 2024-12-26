///
/// Created by Kapil
///

using System;
using System.Collections.Generic;
using System.Web;
using System.Linq;
using System.Web.Mvc;
using P2BUltimate.App_Start;
using System.Net;
using System.Data.Entity;
using P2BUltimate.Models;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Transactions;
using P2b.Global;
using System.Text;
using System.Threading.Tasks;
using P2BUltimate.Security;
using System.Configuration;
using System.Data.SqlClient;
namespace P2BUltimate.Controllers.Core.MainController
{
    [AuthoriseManger]
    public class DepartmentController : Controller
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
            return View("~/Views/Core/MainViews/Department/Index.cshtml");
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

        //private DataBaseContext db = new DataBaseContext();


        private MultiSelectList GetCountry(int selectedValues)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<Country> country = new List<Country>();
                country = db.Country.ToList();
                return new SelectList(country, "Id", "Name", selectedValues);
            }
        }

        private MultiSelectList GetState(int id, int selectedValues)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                Country country = db.Country.Include(e => e.States).Where(e => e.Id == id).SingleOrDefault();
                ICollection<State> st = country.States;
                return new SelectList(st, "Id", "Name", selectedValues);
            }
        }

        private MultiSelectList GetRegion(int id, int selectedValues)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                State st = db.State.Include(e => e.StateRegions).Where(e => e.Id == id).SingleOrDefault();
                ICollection<StateRegion> region = st.StateRegions;
                return new SelectList(region, "Id", "Name", selectedValues);
            }
        }

        //private MultiSelectList GetDist(int id, int selectedValues)
        //{
        //    StateRegion region = db.Region.Include(e => e.Districts).Where(e => e.Id == id).SingleOrDefault();
        //    ICollection<District> dist = region.Districts;
        //    return new SelectList(dist, "Id", "Code", selectedValues);
        //}

        private MultiSelectList GetTaluka(int id, int selectedValues)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                District dist = db.District.Include(e => e.Talukas).Where(e => e.Id == id).SingleOrDefault();
                ICollection<Taluka> taluka = dist.Talukas;
                return new SelectList(taluka, "Id", "Name", selectedValues);
            }
        }

        private MultiSelectList GetCity(int id, int selectedValues)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                Taluka taluka = db.Taluka.Include(e => e.Cities).Where(e => e.Id == id).SingleOrDefault();
                ICollection<City> city = taluka.Cities;
                return new SelectList(city, "Id", "Name", selectedValues);
            }
        }

        private MultiSelectList GetArea(int id, int selectedValues)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                City city = db.City.Include(e => e.Areas).Where(e => e.Id == id).SingleOrDefault();
                ICollection<Area> area = city.Areas;
                return new SelectList(area, "Id", "Name", selectedValues);
            }
        }

        public MultiSelectList getState()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var state = db.State.ToList();
                return new MultiSelectList(state, "Id", "Name", "");
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


        [HttpPost]
        public ActionResult Create(Department c, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    int comp_Id = 0;
                    comp_Id = Convert.ToInt32(Session["CompId"]);
                    var Company = new Company();
                    Company = db.Company.Where(e => e.Id == comp_Id).SingleOrDefault();

                    string Addrs = form["AddressList"] == "0" ? "" : form["AddressList"];
                    string disc = form["ContactDetailsList"] == "0" ? "" : form["ContactDetailsList"];
                    string Department_Obj_DDL = form["Department_Obj_DDL"] == "" ? null : form["Department_Obj_DDL"];
                    string UnitId = form["UnitId"] == "0" ? "" : form["UnitId"];
                    string Incharge_DDL = form["Incharge_Id"] == "" ? null : form["Incharge_Id"];

                    if (Incharge_DDL != null && Incharge_DDL != "-Select-")
                    {
                        var value = db.Employee.Find(int.Parse(Incharge_DDL));
                        c.Incharge = value;

                    }
                    //if (UnitId != null && UnitId != "")
                    //{
                    //    // var val = db.Location.Find(int.Parse(biometric));
                    //    c.UnitId = UnitId;
                    //}

                    //if (HolidayCalendar_DDL != null && HolidayCalendar_DDL != "-Select-")
                    //{
                    //    var value = db.HolidayCalendar.Find(int.Parse(HolidayCalendar_DDL));
                    //    c.HolidayCalendar = value;

                    //}
                    //if (WeeklyOffCalendar_DDL != null && WeeklyOffCalendar_DDL != "-Select-")
                    //{
                    //    var value = db.WeeklyOffCalendar.Find(int.Parse(WeeklyOffCalendar_DDL));
                    //    c.WeeklyOffCalendar = value;

                    //}
                    //List<HolidayCalendar> lookupLang = new List<HolidayCalendar>();

                    //string HolidayCalendar_DDL = form["HOCalendarsList"] == "" ? null : form["HOCalendarsList"];
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

                    string HolidayCalendar_DDL = form["HOCalendarsList"] == "" ? null : form["HOCalendarsList"];

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
                            var val = db.WeeklyOffCalendar.Include(e => e.WeeklyOffList).Include(e => e.WOCalendar).Include(e => e.WOCalendar.Name).Where(e => e.Id == weeklyListid).SingleOrDefault();
                            if (val != null)
                            {
                                Weeklycallist.Add(val);
                            }
                        }
                        c.WeeklyOffCalendar = Weeklycallist;
                    }


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

                    if (disc != null)
                    {
                        if (disc != "")
                        {
                            int ContId = Convert.ToInt32(disc);
                            var val = db.ContactDetails.Include(e => e.ContactNumbers)
                                                .Where(e => e.Id == ContId).SingleOrDefault();
                            c.ContactDetails = val;
                        }
                    }
                    if (Department_Obj_DDL != null)
                    {
                        if (Department_Obj_DDL != "")
                        {
                            int ContId = Convert.ToInt32(Department_Obj_DDL);
                            var val = db.DepartmentObj.Where(e => e.Id == ContId).SingleOrDefault();
                            c.DepartmentObj = val;
                        }
                    }

                    var deptcheck = db.Department.Include(e => e.DepartmentObj)
                        .Where(e => e.DepartmentObj.DeptCode == c.DepartmentObj.DeptCode).FirstOrDefault();
                    if (deptcheck != null)
                    {
                        if (Addrs == null)
                        {
                            Msg.Add(" Please select address..!!");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }

                    if (Addrs != null)
                    {
                        if (Addrs != "")
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
                    }

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            Department Department = new Department()
                            {
                                DepartmentObj = c.DepartmentObj,
                                OpeningDate = c.OpeningDate,
                                Address = c.Address,
                                ContactDetails = c.ContactDetails,
                                DBTrack = c.DBTrack,
                                Incharge = c.Incharge,
                                HolidayCalendar = c.HolidayCalendar,
                                WeeklyOffCalendar = c.WeeklyOffCalendar,
                                //UnitId = c.UnitId
                            };
                            try
                            {

                                db.Department.Add(Department);
                                var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, c.DBTrack);
                                DT_Department DT_Corp = (DT_Department)rtn_Obj;
                                DT_Corp.Address_Id = c.Address == null ? 0 : c.Address.Id;
                                DT_Corp.ContactDetails_Id = c.ContactDetails == null ? 0 : c.ContactDetails.Id;
                                DT_Corp.DepartmentObj_Id = c.DepartmentObj == null ? 0 : c.DepartmentObj.Id;

                                db.Create(DT_Corp);
                                db.SaveChanges();
                                if (Company != null)
                                {
                                    var Objjob = new List<Department>();
                                    Objjob.Add(Department);
                                    Company.Department = Objjob;
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
                                        cmd.CommandTimeout = 3000;
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
                                //eturn this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                            }

                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = c.Id });
                            }
                            catch (DataException)
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
                var Q = db.Department
                    .Include(e => e.ContactDetails)
                    .Include(e => e.Address)
                    .Include(e => e.Type)
                    .Include(e => e.Incharge)
                    .Include(e => e.Incharge.EmpName)
                    .Include(e => e.ContactDetails)
                      .Include(e => e.HolidayCalendar)
                        .Include(e => e.WeeklyOffCalendar)
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        Code = e.DepartmentObj.DeptCode,
                        Name = e.DepartmentObj.DeptDesc,
                        OpeningDate = e.OpeningDate,
                        //Incharge_Id = e.Incharge != null ? e.Incharge.Id : 0,
                        Incharge = e.Incharge == null ? "" : e.Incharge.FullDetails,
                        Incharge_Id = e.Incharge != null ? e.Incharge.Id : 0,
                        Address_FullAddress = e.Address.FullAddress == null ? "" : e.Address.FullAddress,
                        Add_Id = e.Address.Id == null ? "" : e.Address.Id.ToString(),
                        Cont_Id = e.ContactDetails.Id == null ? "" : e.ContactDetails.Id.ToString(),
                        FullContactDetails = e.ContactDetails.FullContactDetails == null ? "" : e.ContactDetails.FullContactDetails,
                        DepartmentObj_Id = e.DepartmentObj.Id == null ? 0 : e.DepartmentObj.Id,
                        //Unit = e.UnitId == null ? "" : e.UnitId,
                        Type_Id = e.Type.Id == null ? 0 : e.Type.Id,
                        Action = e.DBTrack.Action
                    }).ToList();
                List<returnEditClass> oreturnEditClass = new List<returnEditClass>();

                var k = db.Department.Include(e => e.HolidayCalendar)
                     .Include(e => e.HolidayCalendar.Select(a => a.HoliCalendar))
                    .Include(e => e.HolidayCalendar.Select(a => a.HolidayList))
                    .Include(e => e.HolidayCalendar.Select(a => a.HoliCalendar.Name))
                        .Where(e => e.Id == data && e.HolidayCalendar.Count() > 0).ToList();
                foreach (var e in k)
                {
                    oreturnEditClass.Add(new returnEditClass
                    {

                        Holly_Id = e.HolidayCalendar.Select(a => a.Id.ToString()).ToArray(),
                        HollyFullDetails = e.HolidayCalendar.Select(a => a.FullDetails).ToArray(),
                    });
                }

                var m = db.Department.Include(e => e.WeeklyOffCalendar)
                    .Include(e => e.WeeklyOffCalendar.Select(a => a.WeeklyOffList))
                      .Include(e => e.WeeklyOffCalendar.Select(a => a.WOCalendar))
                      .Include(e => e.WeeklyOffCalendar.Select(a => a.WOCalendar.Name))
                    .Include(e => e.WeeklyOffCalendar.Select(a => a.WeeklyOffList.Select(z => z.WeeklyOffStatus)))
                    .Include(e => e.WeeklyOffCalendar.Select(a => a.WeeklyOffList.Select(z => z.WeekDays)))
                    .Where(e => e.Id == data && e.WeeklyOffCalendar.Count() > 0).ToList();
                foreach (var e in m)
                {
                    oreturnEditClass.Add(new returnEditClass
                    {

                        week_Id = e.WeeklyOffCalendar.Select(a => a.Id.ToString()).ToArray(),
                        WeeklyFullDetails = e.WeeklyOffCalendar.Select(a => a.FullDetails).ToArray(),
                    });
                }

                var W = db.DT_Department
                     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id,
                         Code = e.DepartmentObj_Id == 0 ? "" : db.DepartmentObj
                                    .Where(x => x.Id == e.DepartmentObj_Id)
                                    .Select(x => x.DeptCode).FirstOrDefault(),
                         Name = e.DepartmentObj_Id == 0 ? "" : db.DepartmentObj
                                    .Where(x => x.Id == e.DepartmentObj_Id)
                                    .Select(x => x.DeptDesc).FirstOrDefault(),
                         OpeningDate = e.OpeningDate,
                         Type_Val = e.Type_Id == 0 ? "" : db.LookupValue
                                    .Where(x => x.Id == e.Type_Id)
                                    .Select(x => x.LookupVal).FirstOrDefault(),
                         Address_Val = e.Address_Id == 0 ? "" : db.Address.Where(x => x.Id == e.Address_Id).Select(x => x.FullAddress).FirstOrDefault(),
                         Contact_Val = e.ContactDetails_Id == 0 ? "" : db.ContactDetails.Where(x => x.Id == e.ContactDetails_Id).Select(x => x.FullContactDetails).FirstOrDefault()
                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var Corp = db.Department.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, oreturnEditClass, W, Auth, JsonRequestBehavior.AllowGet });
            }
        }


        //[HttpPost]
        //public ActionResult Edit(int data)
        //{
        //    var corpo = db.Department
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

        //    TempData["RowVersion"] = db.Department.Find(data).RowVersion;
        //    return Json(new object[] { add, cont }, JsonRequestBehavior.AllowGet);
        //}



        /* ------------------------------------------ Grid -------------------------------------------------- */



        public class CorporateRegion
        {
            public int DeptId { get; set; }
            public string DeptpName { get; set; }
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
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
                IEnumerable<Department> Location = null;
                if (gp.IsAutho == true)
                {
                    Location = db.Company.Include(e => e.Department).SelectMany(e => e.Department).AsNoTracking().ToList();
                }
                else
                {
                    FilterSession.Session a = new FilterSession.Session();
                    var b = a.Check_flow();
                    if (b != null)
                    {
                        if (b.type == "master")
                        {
                            Location = db.Company.Include(e => e.Department.Select(r => r.DepartmentObj)).Where(e => e.Id == b.comp_code).SelectMany(e => e.Department).ToList();
                        }
                        else
                        {
                            Location = db.Department.Include(e => e.DepartmentObj).ToList();

                        }
                    }
                    else
                    {
                        Location = db.Department.Include(e => e.DepartmentObj).ToList();
                    }
                    //if (Session["object"] != null && Session["object"] != "")
                    //{
                    //    if (Session["object"].ToString() == "object")
                    //    {
                    //        Location = db.Department.ToList();
                    //    }
                    //    else if (Session["object"].ToString() == "master")
                    //    {
                    //        var id = Convert.ToInt32(Session["CompCode"]);

                    //        if (id != null && id != 0)
                    //        {
                    //            Location = db.Company.Include(e => e.Department).Where(e => e.Id == id).SelectMany(e => e.Department).ToList();
                    //        }
                    //    }
                    //}else{
                    //    Location = db.Department.ToList();
                    //}

                }

                IEnumerable<Department> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = Location;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.DepartmentObj.DeptCode.ToString().Contains(gp.searchString))
                              || (e.DepartmentObj.DeptDesc.ToUpper().Contains(gp.searchString.ToUpper()))
                              || (e.Id.ToString().Contains(gp.searchString.ToUpper()))
                              ).Select(a => new Object[] { a.DepartmentObj.DeptCode, a.DepartmentObj.DeptDesc, a.Id }).ToList();
                        //jsonData = IE.Where(e => (e.Id.ToString() == gp.searchString) || (e.DepartmentObj.DeptCode.ToLower() == gp.searchString.ToLower()) || (e.DepartmentObj.DeptDesc.ToLower() == gp.searchString.ToLower())).Select(a => new Object[] { a.DepartmentObj.DeptCode, a.DepartmentObj.DeptDesc, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.DepartmentObj.DeptCode, a.DepartmentObj.DeptDesc, a.Id, a.Type != null ? Convert.ToString(a.Type.LookupVal) : "" }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = Location;
                    Func<Department, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "Code" ? c.DepartmentObj.DeptCode :
                                         gp.sidx == "Name" ? c.DepartmentObj.DeptDesc :
                                         gp.sidx == "Type" ? c.Type.LookupVal : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.DepartmentObj != null ? a.DepartmentObj.DeptCode : "", a.DepartmentObj != null ? a.DepartmentObj.DeptDesc : "", a.Id, a.Type != null ? Convert.ToString(a.Type.LookupVal) : "" }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.DepartmentObj != null ? a.DepartmentObj.DeptCode : "", a.DepartmentObj != null ? a.DepartmentObj.DeptDesc : "", a.Id, a.Type != null ? Convert.ToString(a.Type.LookupVal) : "" }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.DepartmentObj.DeptCode, a.DepartmentObj.DeptDesc, a.Id, a.Type != null ? Convert.ToString(a.Type.LookupVal) : "" }).ToList();
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
                throw ex;
            }
        }

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
        //        IEnumerable<CorporateRegion> Department = null;

        //        List<CorporateRegion> model = new List<CorporateRegion>();

        //        var r = db.Location.Include(e => e.Department).ToList();

        //        var view = new CorporateRegion();
        //        foreach (var i in r)
        //        {
        //            foreach (var z in i.Department)
        //            {
        //                view = new CorporateRegion()
        //                {
        //                    DeptId = i.Id,
        //                    DeptpName = i.Name,
        //                    Id = z.Id,
        //                    Code = z.Code,
        //                    Name = z.Name
        //                };

        //                model.Add(view);
        //            }
        //        }

        //        var fall = db.Department.ToList();
        //        var list1 = db.Location.ToList().SelectMany(e => e.Department);
        //        var list2 = fall.Except(list1);

        //        foreach (var z in list2)
        //        {
        //            view = new CorporateRegion()
        //            {
        //                DeptId = 0,
        //                DeptpName = "",
        //                Id = z.Id,
        //                Code = z.Code,
        //                Name = z.Name
        //            };

        //            model.Add(view);
        //        }

        //        Department = model;

        //        IEnumerable<CorporateRegion> IE;
        //        if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
        //        {
        //            IE = Department;
        //            if (gp.searchOper.Equals("eq"))
        //            {
        //                if (gp.searchField == "DeptpName")
        //                    jsonData = IE.Select(a => new { a.Id, a.DeptpName, a.Code, a.Name }).Where((e => (e.DeptpName.ToString().Contains(gp.searchString)))).ToList();
        //                else if (gp.searchField == "Id")
        //                    jsonData = IE.Select(a => new { a.Id, a.DeptpName, a.Code, a.Name }).Where((e => (e.Id.ToString().Contains(gp.searchString)))).ToList();
        //                else if (gp.searchField == "Code")
        //                    jsonData = IE.Select(a => new { a.Id, a.DeptpName, a.Code, a.Name }).Where((e => (e.Code.ToString().Contains(gp.searchString)))).ToList();
        //                else if (gp.searchField == "Name")
        //                    jsonData = IE.Select(a => new { a.Id, a.DeptpName, a.Code, a.Name }).Where((e => (e.Name.ToString().Contains(gp.searchString)))).ToList();
        //                ////jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.DeptpName, a.Id, a.Code, a.Name }).ToList();
        //            }
        //            totalRecords = IE.Count();
        //        }
        //        else
        //        {
        //            IE = Department;
        //            Func<CorporateRegion, dynamic> orderfuc;
        //            if (gp.sidx == "Id")
        //            {
        //                orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
        //            }
        //            else
        //            {
        //                orderfuc = (c => gp.sidx == "CorpName" ? c.DeptpName.ToString() :
        //                                 gp.sidx == "Code" ? c.Code :
        //                                 gp.sidx == "Name" ? c.Name : "");
        //            }
        //            if (gp.sord == "asc")
        //            {
        //                IE = IE.OrderBy(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.DeptpName), Convert.ToString(a.Code), Convert.ToString(a.Name) }).ToList();
        //            }
        //            else if (gp.sord == "desc")
        //            {
        //                IE = IE.OrderByDescending(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.DeptpName), Convert.ToString(a.Code), Convert.ToString(a.Name) }).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.DeptpName, a.Code, a.Name }).ToList();
        //            }
        //            totalRecords = Department.Count();
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







        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    Department Departments = db.Department.Include(e => e.Address)
                                                       .Include(e => e.ContactDetails).Include(e => e.HolidayCalendar).Include(e => e.WeeklyOffCalendar)
                                                       .Include(e => e.Type).Where(e => e.Id == data).SingleOrDefault();

                    Address add = Departments.Address;
                    ContactDetails conDet = Departments.ContactDetails;
                    LookupValue val = Departments.Type;
                    //   HolidayCalendar hol = Departments.HolidayCalendar;
                    //   WeeklyOffCalendar wof = Departments.WeeklyOffCalendar;
                    if (Departments.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = Departments.DBTrack.CreatedBy != null ? Departments.DBTrack.CreatedBy : null,
                                CreatedOn = Departments.DBTrack.CreatedOn != null ? Departments.DBTrack.CreatedOn : null,
                                IsModified = Departments.DBTrack.IsModified == true ? true : false
                            };
                            Departments.DBTrack = dbT;
                            db.Entry(Departments).State = System.Data.Entity.EntityState.Modified;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, Departments.DBTrack);
                            DT_Department DT_Corp = (DT_Department)rtn_Obj;
                            DT_Corp.Address_Id = Departments.Address == null ? 0 : Departments.Address.Id;
                            DT_Corp.Type_Id = Departments.Type == null ? 0 : Departments.Type.Id;
                            DT_Corp.ContactDetails_Id = Departments.ContactDetails == null ? 0 : Departments.ContactDetails.Id;
                            db.Create(DT_Corp);
                            await db.SaveChangesAsync();
                            ts.Complete();
                            //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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
                                    CreatedBy = Departments.DBTrack.CreatedBy != null ? Departments.DBTrack.CreatedBy : null,
                                    CreatedOn = Departments.DBTrack.CreatedOn != null ? Departments.DBTrack.CreatedOn : null,
                                    IsModified = Departments.DBTrack.IsModified == true ? false : false//,
                                };

                                db.Entry(Departments).State = System.Data.Entity.EntityState.Deleted;
                                var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dbT);
                                DT_Department DT_Corp = (DT_Department)rtn_Obj;
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
                                //return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
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
        //                            select new { c.Id, c.DepartmentCode, c.DepartmentName });
        //            return Json(r, JsonRequestBehavior.AllowGet);
        //        }
        //        var result = (from c in all
        //                      select new { c.Id, c.Address3 }).Distinct();
        //        return Json(result, JsonRequestBehavior.AllowGet);
        //    }
        //    return View();
        //}


        /* -------------------------- Incharge ----------------------*/
        public ActionResult GetLookupIncharge(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.NameDetails.ToList();
                IEnumerable<NameDetails> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.NameDetails.ToList().Where(d => d.FullNameFML.Contains(data));
                }
                else
                {

                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullNameFML }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.FullNameFML }).Distinct();
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
                    //                select new { c.Id, c.DepartmentCode, c.DepartmentName });
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.FullContactDetails }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            // return View();
        }

        public ActionResult GetLookupDetailsHolliday(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.HolidayCalendar.Include(e => e.HolidayList).Include(e => e.Department).Include(e => e.HolidayList.Select(q => q.Holiday))
                    .Include(e => e.HoliCalendar)
                    .Include(e => e.HoliCalendar.Name).ToList();
                IEnumerable<HolidayCalendar> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.HolidayCalendar.ToList().Where(d => d.FullDetails.Contains(data));

                }
                else
                {
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                    //var result_1 = (from c in fall
                    //                select new { c.Id, c.DepartmentCode, c.DepartmentName });
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.FullDetails }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            // return View();
        }

        //public ActionResult GetLookupDetailsHolliday(string data)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var fall = db.HolidayList.Include(e => e.Holiday).Include(e=>e.Holiday.HolidayType).ToList();
        //        IEnumerable<HolidayList> all;
        //        if (!string.IsNullOrEmpty(data))
        //        {
        //            all = db.HolidayList.ToList().Where(d => d.FullDetails.Contains(data));

        //        }
        //        else
        //        {
        //            var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
        //            //var result_1 = (from c in fall
        //            //                select new { c.Id, c.DepartmentCode, c.DepartmentName });
        //            return Json(r, JsonRequestBehavior.AllowGet);
        //        }
        //        var result = (from c in all
        //                      select new { c.Id, c.FullDetails }).Distinct();
        //        return Json(result, JsonRequestBehavior.AllowGet);

        //    }
        //    // return View();
        //}

        //public ActionResult GetLookupDetailsWOCalendar(string data)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var fall = db.WeeklyOffCalendar.Include(e => e.WeeklyOffList).ToList();
        //        IEnumerable<WeeklyOffCalendar> all;
        //        if (!string.IsNullOrEmpty(data))
        //        {
        //            all = db.WeeklyOffCalendar.ToList().Where(d => d.FullDetails.Contains(data));

        //        }
        //        else
        //        {
        //            var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
        //            //var result_1 = (from c in fall
        //            //                select new { c.Id, c.DepartmentCode, c.DepartmentName });
        //            return Json(r, JsonRequestBehavior.AllowGet);
        //        }
        //        var result = (from c in all
        //                      select new { c.Id, c.FullDetails }).Distinct();
        //        return Json(result, JsonRequestBehavior.AllowGet);

        //    }
        //    // return View();
        //}
        public ActionResult GetLookupDetailsWOCalendar(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.WeeklyOffCalendar
                    //.Include(e => e.WeeklyOffList)
                    .Include(e => e.WOCalendar.Name)
                    .Include(e => e.WOCalendar).ToList();
                IEnumerable<WeeklyOffCalendar> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.WeeklyOffCalendar.ToList().Where(d => d.FullDetails.Contains(data));

                }
                else
                {
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                    //var result_1 = (from c in fall
                    //                select new { c.Id, c.DepartmentCode, c.DepartmentName });
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.FullDetails }).Distinct();
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
                TempData["RowVersion"] = db.Department.Find(data).RowVersion;
                return Json(new object[] { r }, JsonRequestBehavior.AllowGet);
            }
        }




        /*------------Edit save --------------------------*/

        [HttpPost]
        public async Task<ActionResult> EditSave(Department c, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string Addrs = form["AddressList"] == "0" ? "" : form["AddressList"];
                    string ContactDetails = form["ContactDetailsList"] == "0" ? "" : form["ContactDetailsList"];
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;
                    string UnitId = form["UnitId"] == "0" ? "" : form["UnitId"];
                    string Incharge_DDL = form["Incharge_Id"] == "0" ? "" : form["Incharge_Id"];
                    string Department_Obj_DDL = form["Department_Obj_DDL"] == "0" ? "" : form["Department_Obj_DDL"];
                    
                    if (Addrs != null)
                    {
                        if (Addrs != "")
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
                    }
                    if (Department_Obj_DDL != null)
                    {
                        if (Department_Obj_DDL != "")
                        {
                            int ContId = Convert.ToInt32(Department_Obj_DDL);
                            var val = db.DepartmentObj.Where(e => e.Id == ContId).SingleOrDefault();
                            c.DepartmentObj = val;
                        }
                    }
                    if (ContactDetails != null)
                    {
                        if (ContactDetails != "")
                        {
                            int ContId = Convert.ToInt32(ContactDetails);
                            var val = db.ContactDetails.Include(e => e.ContactNumbers)
                                                .Where(e => e.Id == ContId).SingleOrDefault();
                            c.ContactDetails = val;
                        }
                    }

                    //if (UnitId != null)
                    //{
                    //    if (UnitId != "")
                    //    {
                    //        c.UnitId = UnitId;
                    //    }
                    //}
                    List<HolidayCalendar> lookupLang = new List<HolidayCalendar>();

                    string HolidayCalendar_DDL = form["HOCalendarsList"] == "" ? null : form["HOCalendarsList"];
                    if (HolidayCalendar_DDL != null)
                    {
                        var ids = Utility.StringIdsToListIds(HolidayCalendar_DDL);
                        foreach (var ca in ids)
                        {
                            var Lookup_val = db.HolidayCalendar.Find(ca);

                            lookupLang.Add(Lookup_val);
                            c.HolidayCalendar = lookupLang;
                        }
                    }
                    else
                    {
                        c.HolidayCalendar = null;
                    }

                    List<WeeklyOffCalendar> lookupLang1 = new List<WeeklyOffCalendar>();
                    string WeeklyOffCalendar_DDL = form["WOCalendarList"] == "" ? null : form["WOCalendarList"];

                    if (WeeklyOffCalendar_DDL != null)
                    {
                        var ids = Utility.StringIdsToListIds(WeeklyOffCalendar_DDL);
                        foreach (var ca in ids)
                        {
                            var Lookup_val = db.WeeklyOffCalendar.Find(ca);

                            lookupLang1.Add(Lookup_val);
                            c.WeeklyOffCalendar = lookupLang1;
                        }
                    }
                    else
                    {
                        c.WeeklyOffCalendar = null;
                    }


                    if (Auth == false)
                    {
                        if (ModelState.IsValid)
                        {
                            try
                            {
                                //DbContextTransaction transaction = db.Database.BeginTransaction();

                                //using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))

                                using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, new System.TimeSpan(0, 30, 0)))
                                {
                                    Department blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    //using (var context = new DataBaseContext())
                                    //{
                                        blog = db.Department.Where(e => e.Id == data)
                                                                .Include(e => e.Address).Include(e => e.DepartmentObj)
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

                                  //  db.SaveChanges();


                                    // int a = EditS(Department_Obj_DDL, Incharge_DDL, HolidayCalendar_DDL, WeeklyOffCalendar_DDL, Addrs, ContactDetails, data, c, c.DBTrack);

                                    if (Department_Obj_DDL != null)
                                    {
                                        if (Department_Obj_DDL != "")
                                        {
                                            var val = db.DepartmentObj.Find(int.Parse(Department_Obj_DDL));
                                            c.DepartmentObj = val;

                                            var type = db.Department.Include(e => e.DepartmentObj).Where(e => e.Id == data).SingleOrDefault();
                                            IList<Department> typedetails = null;
                                            if (type.DepartmentObj != null)
                                            {
                                                typedetails = db.Department.Where(x => x.DepartmentObj.Id == type.DepartmentObj.Id && x.Id == data).ToList();
                                            }
                                            else
                                            {
                                                typedetails = db.Department.Where(x => x.Id == data).ToList();
                                            }
                                            //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                                            foreach (var s in typedetails)
                                            {
                                                s.DepartmentObj = c.DepartmentObj;
                                                db.Department.Attach(s);
                                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                                //await db.SaveChangesAsync();
                                                db.SaveChanges();
                                                TempData["RowVersion"] = s.RowVersion;
                                               // db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                            }
                                        }
                                        else
                                        {
                                            var BusiTypeDetails = db.Department.Include(e => e.DepartmentObj).Where(x => x.Id == data).ToList();
                                            foreach (var s in BusiTypeDetails)
                                            {
                                                s.DepartmentObj = null;
                                                db.Department.Attach(s);
                                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                                //await db.SaveChangesAsync();
                                                db.SaveChanges();
                                                TempData["RowVersion"] = s.RowVersion;
                                               // db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var BusiTypeDetails = db.Department.Include(e => e.DepartmentObj).Where(x => x.Id == data).ToList();
                                        foreach (var s in BusiTypeDetails)
                                        {
                                            s.DepartmentObj = null;
                                            db.Department.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                          //  db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }
                                    if (Incharge_DDL != null)
                                    {
                                        if (Incharge_DDL != "")
                                        {
                                            // val = db.Employee.Find(int.Parse(Incharge_DDL));
                                            int empid = int.Parse(Incharge_DDL);
                                            var val = db.Employee.Where(q => q.Id == empid).SingleOrDefault();
                                            c.Incharge = val;
                                            //int locid=val.GeoStruct.Location.Id;
                                            var type = db.Department.Include(e => e.Incharge).Where(e => e.Id == data).SingleOrDefault();
                                            IList<Department> typedetails = null;
                                            if (type.Incharge != null)
                                            {
                                                typedetails = db.Department.Where(x => x.Incharge.Id == type.Incharge.Id && x.Id == data).ToList();
                                            }
                                            else
                                            {
                                                typedetails = db.Department.Where(x => x.Id == data).ToList();
                                            }
                                            //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                                            foreach (var s in typedetails)
                                            {
                                                s.Incharge = c.Incharge;
                                                db.Department.Attach(s);
                                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                                //await db.SaveChangesAsync();
                                                db.SaveChanges();
                                                TempData["RowVersion"] = s.RowVersion;
                                               // db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                            }
                                        }
                                        else
                                        {
                                            var BusiTypeDetails = db.Department.Include(e => e.Incharge).Where(x => x.Id == data).ToList();
                                            foreach (var s in BusiTypeDetails)
                                            {
                                                s.Incharge = null;
                                                db.Department.Attach(s);
                                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                                //await db.SaveChangesAsync();
                                                db.SaveChanges();
                                                TempData["RowVersion"] = s.RowVersion;
                                                //db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var BusiTypeDetails = db.Department.Include(e => e.Incharge).Where(x => x.Id == data).ToList();
                                        foreach (var s in BusiTypeDetails)
                                        {
                                            s.Incharge = null;
                                            db.Department.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            //db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }
                                   
                                

                                    if (HolidayCalendar_DDL != null)
                                    {
                                        if (HolidayCalendar_DDL != "")
                                        {
                                            var val = db.HolidayCalendar.Find(int.Parse(HolidayCalendar_DDL));
                                            //var val = db.Hobby.Where(e => e.Id == data).SingleOrDefault();

                                            var r = (from ca in db.HolidayCalendar
                                                     select new
                                                     {
                                                         Id = ca.Id,
                                                         LookupVal = ca.FullDetails,
                                                     }).Where(e => e.Id == data).Distinct();

                                            var add = db.Department.Include(e => e.HolidayCalendar).Where(e => e.Id == data).SingleOrDefault();
                                            IList<Department> contactdetails = null;
                                            if (add.HolidayCalendar != null)
                                            //{
                                            //    contactdetails = db.FamilyDetails.Where(x => x.ContactDetails == add.ContactDetails && x.Id == data).ToList();
                                            //}
                                            //else
                                            {
                                                contactdetails = db.Department.Where(x => x.Id == data).ToList();
                                            }
                                            if (contactdetails != null)
                                            {
                                                foreach (var s in contactdetails)
                                                {
                                                    s.HolidayCalendar = c.HolidayCalendar;
                                                    db.Department.Attach(s);
                                                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                                    // await db.SaveChangesAsync(false);
                                                    db.SaveChanges();
                                                    TempData["RowVersion"] = s.RowVersion;
                                                //    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var contactdetails = db.Department.Include(e => e.HolidayCalendar).Where(x => x.Id == data).ToList();
                                        foreach (var s in contactdetails)
                                        {
                                            s.HolidayCalendar = null;
                                            db.Department.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                           // db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }


                                    if (WeeklyOffCalendar_DDL != null)
                                    {
                                        if (WeeklyOffCalendar_DDL != "")
                                        {
                                            var val = db.WeeklyOffCalendar.Find(int.Parse(WeeklyOffCalendar_DDL));
                                            //var val = db.Hobby.Where(e => e.Id == data).SingleOrDefault();

                                            var r = (from ca in db.WeeklyOffCalendar
                                                     select new
                                                     {
                                                         Id = ca.Id,
                                                         LookupVal = ca.FullDetails,
                                                     }).Where(e => e.Id == data).Distinct();

                                            var add = db.Department.Include(e => e.WeeklyOffCalendar).Where(e => e.Id == data).SingleOrDefault();
                                            IList<Department> contactdetails = null;
                                            if (add.WeeklyOffCalendar != null)
                                            //{
                                            //    contactdetails = db.FamilyDetails.Where(x => x.ContactDetails == add.ContactDetails && x.Id == data).ToList();
                                            //}
                                            //else
                                            {
                                                contactdetails = db.Department.Where(x => x.Id == data).ToList();
                                            }
                                            if (contactdetails != null)
                                            {
                                                foreach (var s in contactdetails)
                                                {
                                                    s.WeeklyOffCalendar = c.WeeklyOffCalendar;
                                                    db.Department.Attach(s);
                                                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                                    // await db.SaveChangesAsync(false);
                                                    db.SaveChanges();
                                                    TempData["RowVersion"] = s.RowVersion;
                                                  //  db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var contactdetails = db.Department.Include(e => e.WeeklyOffCalendar).Where(x => x.Id == data).ToList();
                                        foreach (var s in contactdetails)
                                        {
                                            s.WeeklyOffCalendar = null;
                                            db.Department.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            //db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }


                                 
                                    if (Addrs != null)
                                    {
                                        if (Addrs != "")
                                        {
                                            var val = db.Address.Find(int.Parse(Addrs));
                                            c.Address = val;

                                            var add = db.Department.Include(e => e.Address).Where(e => e.Id == data).SingleOrDefault();
                                            IList<Department> addressdetails = null;
                                            if (add.Address != null)
                                            {
                                                addressdetails = db.Department.Where(x => x.Address.Id == add.Address.Id && x.Id == data).ToList();
                                            }
                                            else
                                            {
                                                addressdetails = db.Department.Where(x => x.Id == data).ToList();
                                            }
                                            if (addressdetails != null)
                                            {
                                                foreach (var s in addressdetails)
                                                {
                                                    s.Address = c.Address;
                                                    db.Department.Attach(s);
                                                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                                    // await db.SaveChangesAsync(false);
                                                    db.SaveChanges();
                                                    TempData["RowVersion"] = s.RowVersion;
                                                  //  db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var addressdetails = db.Department.Include(e => e.Address).Where(x => x.Id == data).ToList();
                                        foreach (var s in addressdetails)
                                        {
                                            s.Address = null;
                                            db.Department.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            //db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }

                                    if (ContactDetails != null)
                                    {
                                        if (ContactDetails != "")
                                        {
                                            var val = db.ContactDetails.Find(int.Parse(ContactDetails));
                                            c.ContactDetails = val;

                                            var add = db.Department.Include(e => e.ContactDetails).Where(e => e.Id == data).SingleOrDefault();
                                            IList<Department> contactsdetails = null;
                                            if (add.ContactDetails != null)
                                            {
                                                contactsdetails = db.Department.Where(x => x.ContactDetails.Id == add.ContactDetails.Id && x.Id == data).ToList();
                                            }
                                            else
                                            {
                                                contactsdetails = db.Department.Where(x => x.Id == data).ToList();
                                            }
                                            foreach (var s in contactsdetails)
                                            {
                                                s.ContactDetails = c.ContactDetails;
                                                db.Department.Attach(s);
                                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                                //await db.SaveChangesAsync();
                                                db.SaveChanges();
                                                TempData["RowVersion"] = s.RowVersion;
                                                //db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var contactsdetails = db.Department.Include(e => e.ContactDetails).Where(x => x.Id == data).ToList();
                                        foreach (var s in contactsdetails)
                                        {
                                            s.ContactDetails = null;
                                            db.Department.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            //db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }
                                    var dep = db.Department.Include(x => x.DepartmentObj).Where(e => e.Id == data).FirstOrDefault();
                                    dep.OpeningDate = c.OpeningDate;
                                    dep.Id = data;
                                    dep.DepartmentObj = c.DepartmentObj;
                                    dep.DBTrack = c.DBTrack;
                                    db.Department.Attach(dep);
                                    db.Entry(dep).State = System.Data.Entity.EntityState.Modified;
                                 //   db.SaveChanges();
                                    db.ChangeTracker.DetectChanges();
                                    TempData["RowVersion"] = dep.RowVersion;

                                    //var CurCorp = db.Department.Find(data);
                                    var CurCorp = db.Department.Include(x=>x.DepartmentObj).Where(e=>e.Id==data).FirstOrDefault();
                                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    //db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {
                                        //Department corp = new Department()
                                        //{
                                        //    //Code = c.Code,
                                        //    //Name = c.Name,
                                        //    OpeningDate = c.OpeningDate,
                                        //    Id = data,
                                        
                                        //   // UnitId = c.UnitId,
                                        //    DBTrack = c.DBTrack
                                        //};


                                        //db.Department.Attach(corp);
                                        //db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                                        //db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];

                                    }

                                    //using (var context = new DataBaseContext())
                                    //{

                                        var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                        DT_Department DT_Corp = (DT_Department)obj;
                                        DT_Corp.Address_Id = blog.Address == null ? 0 : blog.Address.Id;
                                        DT_Corp.ContactDetails_Id = blog.ContactDetails == null ? 0 : blog.ContactDetails.Id;
                                        DT_Corp.DepartmentObj_Id = blog.DepartmentObj == null ? 0 : blog.DepartmentObj.Id;
                                        db.Create(DT_Corp);
                                        db.SaveChanges();
                                    //}
                                    await db.SaveChangesAsync();

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
                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = c.Id, Val = c.DepartmentObj.DeptDesc, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    //eturn Json(new Object[] { c.Id, c.DepartmentObj.DeptDesc, "Record Updated", JsonRequestBehavior.AllowGet });
                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (Department)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (Department)databaseEntry.ToObject();
                                    c.RowVersion = databaseValues.RowVersion;

                                }
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

                            Department blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            Department Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.Department.Where(e => e.Id == data).SingleOrDefault();
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
                            Department corp = new Department()
                            {
                                //Code = c.Code,
                                //Name = c.Name,
                                DepartmentObj = c.DepartmentObj,
                                OpeningDate = c.OpeningDate,
                                Id = data,
                                DBTrack = c.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };


                            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, corp, "Department", c.DBTrack);

                                Old_Corp = context.Department.Where(e => e.Id == data)
                                    .Include(e => e.Address).Include(e => e.ContactDetails).SingleOrDefault();
                                DT_Department DT_Corp = (DT_Department)obj;
                                DT_Corp.Address_Id = DBTrackFile.ValCompare(Old_Corp.Address, c.Address);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
                                DT_Corp.ContactDetails_Id = DBTrackFile.ValCompare(Old_Corp.ContactDetails, c.ContactDetails); //Old_Corp.ContactDetails == c.ContactDetails ? 0 : Old_Corp.ContactDetails == null && c.ContactDetails != null ? c.ContactDetails.Id : Old_Corp.ContactDetails.Id;
                                DT_Corp.DepartmentObj_Id = DBTrackFile.ValCompare(Old_Corp.DepartmentObj, c.DepartmentObj);
                                db.Create(DT_Corp);
                                //db.SaveChanges();
                            }
                            blog.DBTrack = c.DBTrack;
                            db.Department.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = c.DepartmentObj.DeptDesc, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            //return Json(new Object[] { blog.Id, c.DepartmentObj.DeptDesc, "Record Updated", JsonRequestBehavior.AllowGet });
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
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    if (auth_action == "C")
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //Department corp = db.Department.Find(auth_id);
                            //Department corp = db.Department.AsNoTracking().FirstOrDefault(e => e.Id == auth_id);

                            Department corp = db.Department.Include(e => e.Address)
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

                            db.Department.Attach(corp);
                            db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            //db.SaveChanges();
                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corp.DBTrack);
                            DT_Department DT_Corp = (DT_Department)rtn_Obj;
                            DT_Corp.Address_Id = corp.Address == null ? 0 : corp.Address.Id;
                            DT_Corp.ContactDetails_Id = corp.ContactDetails == null ? 0 : corp.ContactDetails.Id;
                            DT_Corp.DepartmentObj_Id = corp.DepartmentObj == null ? 0 : corp.DepartmentObj.Id;
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

                        Department Old_Corp = db.Department
                                                          .Include(e => e.Address)
                                                          .Include(e => e.ContactDetails).Where(e => e.Id == auth_id).SingleOrDefault();

                        //var W = db.DT_Department
                        //.Include(e => e.ContactDetails)
                        //.Include(e => e.Address)
                        //.Include(e => e.BusinessType)
                        //.Include(e => e.ContactDetails)
                        //.Where(e => e.Orig_Id == auth_id && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                        //(e => new
                        //{
                        //    DT_Id = e.Id,
                        //    Code = e.Code == null ? "" : e.Code,
                        //    Name = e.Name == null ? "" : e.Name,
                        //    BusinessType_Val = e.BusinessType.Id == null ? "" : e.BusinessType.LookupVal,
                        //    Address_Val = e.Address.Id == null ? "" : e.Address.FullAddress,
                        //    Contact_Val = e.ContactDetails.Id == null ? "" : e.ContactDetails.FullContactDetails,
                        //}).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                        DT_Department Curr_Corp = db.DT_Department
                                                    .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                                    .OrderByDescending(e => e.Id)
                                                    .FirstOrDefault();

                        if (Curr_Corp != null)
                        {
                            Department corp = new Department();

                            string Addrs = Curr_Corp.Address_Id == null ? null : Curr_Corp.Address_Id.ToString();
                            string ContactDetails = Curr_Corp.ContactDetails_Id == null ? null : Curr_Corp.ContactDetails_Id.ToString();
                            string DeptObj = Curr_Corp.DepartmentObj_Id == null ? null : Curr_Corp.DepartmentObj_Id.ToString();
                            //corp.Name = Curr_Corp.Name == null ? Old_Corp.Name : Curr_Corp.Name;
                            //corp.Code = Curr_Corp.Code == null ? Old_Corp.Code : Curr_Corp.Code;
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

                                        //  int a = EditS(Addrs, ContactDetails, auth_id, corp, corp.DBTrack);
                                        //var CurCorp = db.Department.Find(auth_id);
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
                                        //    Department corp = new Department()
                                        //    {
                                        //        Code = c.Code,
                                        //        Name = c.Name,
                                        //        Id = Convert.ToInt32(auth_id),
                                        //        DBTrack = c.DBTrack
                                        //    };


                                        //    db.Department.Attach(corp);
                                        //    db.Entry(corp).State = System.Data.Entity.EntityState.Modified;

                                        //    // db.Entry(c.DBTrack).State = System.Data.Entity.EntityState.Modified;
                                        //    //db.SaveChanges();
                                        //    db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                        //    //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                        //    await db.SaveChangesAsync();
                                        //    //DisplayTrackedEntities(db.ChangeTracker);
                                        //    db.Entry(corp).State = System.Data.Entity.EntityState.Detached;
                                        //    ts.Complete();
                                        //    return Json(new Object[] { corp.Id, corp.Name, "Record Updated", JsonRequestBehavior.AllowGet });
                                        //}

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
                                    var clientValues = (Department)entry.Entity;
                                    var databaseEntry = entry.GetDatabaseValues();
                                    if (databaseEntry == null)
                                    {
                                        Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                        // return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                    }
                                    else
                                    {
                                        var databaseValues = (Department)databaseEntry.ToObject();
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
                            //Department corp = db.Department.Find(auth_id);
                            Department corp = db.Department.AsNoTracking().Include(e => e.Address)
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

                            db.Department.Attach(corp);
                            db.Entry(corp).State = System.Data.Entity.EntityState.Deleted;


                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corp.DBTrack);
                            DT_Department DT_Corp = (DT_Department)rtn_Obj;
                            DT_Corp.Address_Id = corp.Address == null ? 0 : corp.Address.Id;
                            DT_Corp.ContactDetails_Id = corp.ContactDetails == null ? 0 : corp.ContactDetails.Id;
                            DT_Corp.DepartmentObj_Id = corp.DepartmentObj == null ? 0 : corp.DepartmentObj.Id;
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

        /*----- Contact Details Delete ********/


        [HttpPost]
        public ActionResult DeleteContactDetails(int? data, int? forwarddata)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                ContactDetails contDet = db.ContactDetails.Find(data);
                Department loc = db.Department.Find(forwarddata);
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
                Department loc = db.Department.Find(forwarddata); try
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
                Department loc = db.Department.Find(forwarddata);
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
                    // return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists contact your system administrator." });

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

        public int EditS(string DepartmentObj, string Incharge, string HolidayCalendar, string Weeklyoffcalendar, string Addrs, string ContactDetails, int data, Department c, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (DepartmentObj != null)
                {
                    if (DepartmentObj != "")
                    {
                        var val = db.DepartmentObj.Find(int.Parse(DepartmentObj));
                        c.DepartmentObj = val;

                        var type = db.Department.Include(e => e.DepartmentObj).Where(e => e.Id == data).SingleOrDefault();
                        IList<Department> typedetails = null;
                        if (type.DepartmentObj != null)
                        {
                            typedetails = db.Department.Where(x => x.DepartmentObj.Id == type.DepartmentObj.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            typedetails = db.Department.Where(x => x.Id == data).ToList();
                        }
                        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                        foreach (var s in typedetails)
                        {
                            s.DepartmentObj = c.DepartmentObj;
                            db.Department.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                    else
                    {
                        var BusiTypeDetails = db.Department.Include(e => e.DepartmentObj).Where(x => x.Id == data).ToList();
                        foreach (var s in BusiTypeDetails)
                        {
                            s.DepartmentObj = null;
                            db.Department.Attach(s);
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
                    var BusiTypeDetails = db.Department.Include(e => e.DepartmentObj).Where(x => x.Id == data).ToList();
                    foreach (var s in BusiTypeDetails)
                    {
                        s.DepartmentObj = null;
                        db.Department.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }
                if (Incharge != null)
                {
                    if (Incharge != "")
                    {
                        var val = db.Employee.Find(int.Parse(Incharge));
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
                //if (HolidayCalendar != null)
                //{
                //    if (HolidayCalendar != "")
                //    {
                //        var val = db.HolidayCalendar.Find(int.Parse(HolidayCalendar));
                //        c.HolidayCalendar = val;

                //        var type = db.Location.Include(e => e.HolidayCalendar).Where(e => e.Id == data).SingleOrDefault();
                //        IList<Location> typedetails = null;
                //        if (type.HolidayCalendar != null)
                //        {
                //            typedetails = db.Location.Where(x => x.HolidayCalendar.Id == type.HolidayCalendar.Id && x.Id == data).ToList();
                //        }
                //        else
                //        {
                //            typedetails = db.Location.Where(x => x.Id == data).ToList();
                //        }
                //        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                //        foreach (var s in typedetails)
                //        {
                //            s.HolidayCalendar = c.HolidayCalendar;
                //            db.Location.Attach(s);
                //            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                //            //await db.SaveChangesAsync();
                //            db.SaveChanges();
                //            TempData["RowVersion"] = s.RowVersion;
                //            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                //        }
                //    }
                //    else
                //    {
                //        var BusiTypeDetails = db.Location.Include(e => e.HolidayCalendar).Where(x => x.Id == data).ToList();
                //        foreach (var s in BusiTypeDetails)
                //        {
                //            s.HolidayCalendar = null;
                //            db.Location.Attach(s);
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
                //    var BusiTypeDetails = db.Location.Include(e => e.HolidayCalendar).Where(x => x.Id == data).ToList();
                //    foreach (var s in BusiTypeDetails)
                //    {
                //        s.HolidayCalendar = null;
                //        db.Location.Attach(s);
                //        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                //        //await db.SaveChangesAsync();
                //        db.SaveChanges();
                //        TempData["RowVersion"] = s.RowVersion;
                //        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                //    }
                //}


                if (HolidayCalendar != null)
                {
                    if (HolidayCalendar != "")
                    {
                        var val = db.HolidayCalendar.Find(int.Parse(HolidayCalendar));
                        //var val = db.Hobby.Where(e => e.Id == data).SingleOrDefault();

                        var r = (from ca in db.HolidayCalendar
                                 select new
                                 {
                                     Id = ca.Id,
                                     LookupVal = ca.FullDetails,
                                 }).Where(e => e.Id == data).Distinct();

                        var add = db.Department.Include(e => e.HolidayCalendar).Where(e => e.Id == data).SingleOrDefault();
                        IList<Department> contactdetails = null;
                        if (add.HolidayCalendar != null)
                        //{
                        //    contactdetails = db.FamilyDetails.Where(x => x.ContactDetails == add.ContactDetails && x.Id == data).ToList();
                        //}
                        //else
                        {
                            contactdetails = db.Department.Where(x => x.Id == data).ToList();
                        }
                        if (contactdetails != null)
                        {
                            foreach (var s in contactdetails)
                            {
                                s.HolidayCalendar = c.HolidayCalendar;
                                db.Department.Attach(s);
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
                    var contactdetails = db.Department.Include(e => e.HolidayCalendar).Where(x => x.Id == data).ToList();
                    foreach (var s in contactdetails)
                    {
                        s.HolidayCalendar = null;
                        db.Department.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }


                if (Weeklyoffcalendar != null)
                {
                    if (Weeklyoffcalendar != "")
                    {
                        var val = db.WeeklyOffCalendar.Find(int.Parse(Weeklyoffcalendar));
                        //var val = db.Hobby.Where(e => e.Id == data).SingleOrDefault();

                        var r = (from ca in db.WeeklyOffCalendar
                                 select new
                                 {
                                     Id = ca.Id,
                                     LookupVal = ca.FullDetails,
                                 }).Where(e => e.Id == data).Distinct();

                        var add = db.Department.Include(e => e.WeeklyOffCalendar).Where(e => e.Id == data).SingleOrDefault();
                        IList<Department> contactdetails = null;
                        if (add.WeeklyOffCalendar != null)
                        //{
                        //    contactdetails = db.FamilyDetails.Where(x => x.ContactDetails == add.ContactDetails && x.Id == data).ToList();
                        //}
                        //else
                        {
                            contactdetails = db.Department.Where(x => x.Id == data).ToList();
                        }
                        if (contactdetails != null)
                        {
                            foreach (var s in contactdetails)
                            {
                                s.WeeklyOffCalendar = c.WeeklyOffCalendar;
                                db.Department.Attach(s);
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
                    var contactdetails = db.Department.Include(e => e.WeeklyOffCalendar).Where(x => x.Id == data).ToList();
                    foreach (var s in contactdetails)
                    {
                        s.WeeklyOffCalendar = null;
                        db.Department.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }


                //if (Weeklyoffcalendar != null)
                //{
                //    if (Weeklyoffcalendar != "")
                //    {
                //        var val = db.WeeklyOffCalendar.Find(int.Parse(Weeklyoffcalendar));
                //        c.WeeklyOffCalendar = val;

                //        var type = db.Location.Include(e => e.WeeklyOffCalendar).Where(e => e.Id == data).SingleOrDefault();
                //        IList<Location> typedetails = null;
                //        if (type.WeeklyOffCalendar != null)
                //        {
                //            typedetails = db.Location.Where(x => x.WeeklyOffCalendar.Id == type.WeeklyOffCalendar.Id && x.Id == data).ToList();
                //        }
                //        else
                //        {
                //            typedetails = db.Location.Where(x => x.Id == data).ToList();
                //        }
                //        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                //        foreach (var s in typedetails)
                //        {
                //            s.WeeklyOffCalendar = c.WeeklyOffCalendar;
                //            db.Location.Attach(s);
                //            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                //            //await db.SaveChangesAsync();
                //            db.SaveChanges();
                //            TempData["RowVersion"] = s.RowVersion;
                //            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                //        }
                //    }
                //    else
                //    {
                //        var BusiTypeDetails = db.Location.Include(e => e.WeeklyOffCalendar).Where(x => x.Id == data).ToList();
                //        foreach (var s in BusiTypeDetails)
                //        {
                //            s.WeeklyOffCalendar = null;
                //            db.Location.Attach(s);
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
                //    var BusiTypeDetails = db.Location.Include(e => e.WeeklyOffCalendar).Where(x => x.Id == data).ToList();
                //    foreach (var s in BusiTypeDetails)
                //    {
                //        s.WeeklyOffCalendar = null;
                //        db.Location.Attach(s);
                //        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                //        //await db.SaveChangesAsync();
                //        db.SaveChanges();
                //        TempData["RowVersion"] = s.RowVersion;
                //        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                //    }
                //}
                if (Addrs != null)
                {
                    if (Addrs != "")
                    {
                        var val = db.Address.Find(int.Parse(Addrs));
                        c.Address = val;

                        var add = db.Department.Include(e => e.Address).Where(e => e.Id == data).SingleOrDefault();
                        IList<Department> addressdetails = null;
                        if (add.Address != null)
                        {
                            addressdetails = db.Department.Where(x => x.Address.Id == add.Address.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            addressdetails = db.Department.Where(x => x.Id == data).ToList();
                        }
                        if (addressdetails != null)
                        {
                            foreach (var s in addressdetails)
                            {
                                s.Address = c.Address;
                                db.Department.Attach(s);
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
                    var addressdetails = db.Department.Include(e => e.Address).Where(x => x.Id == data).ToList();
                    foreach (var s in addressdetails)
                    {
                        s.Address = null;
                        db.Department.Attach(s);
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

                        var add = db.Department.Include(e => e.ContactDetails).Where(e => e.Id == data).SingleOrDefault();
                        IList<Department> contactsdetails = null;
                        if (add.ContactDetails != null)
                        {
                            contactsdetails = db.Department.Where(x => x.ContactDetails.Id == add.ContactDetails.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            contactsdetails = db.Department.Where(x => x.Id == data).ToList();
                        }
                        foreach (var s in contactsdetails)
                        {
                            s.ContactDetails = c.ContactDetails;
                            db.Department.Attach(s);
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
                    var contactsdetails = db.Department.Include(e => e.ContactDetails).Where(x => x.Id == data).ToList();
                    foreach (var s in contactsdetails)
                    {
                        s.ContactDetails = null;
                        db.Department.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }

                var CurCorp = db.Department.Find(data);
                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    c.DBTrack = dbT;
                    Department corp = new Department()
                    {
                        //Code = c.Code,
                        //Name = c.Name,
                        OpeningDate = c.OpeningDate,
                        Id = data,
                        DBTrack = c.DBTrack
                    };


                    db.Department.Attach(corp);
                    db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];

                    return 1;
                }
                return 0;
            }
        }


        public void RollBack()
        {
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
    }
}