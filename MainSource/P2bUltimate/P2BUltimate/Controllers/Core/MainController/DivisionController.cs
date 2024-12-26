///
/// Created by Sarika
///

using P2b.Global;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Transactions;
using System.Web.Mvc;
using System.Data.Entity;
using System.Text;
using P2BUltimate.Models;
using P2BUltimate.App_Start;
using System.Threading.Tasks;
using P2BUltimate.Security;
using System.Configuration;
using System.Data.SqlClient;
namespace P2BUltimate.Controllers.Core.MainController
{
     [AuthoriseManger]
    public class DivisionController : Controller
    {
        //
        // GET: /Division/

        public ActionResult Index()
        {
            return View("~/Views/Core/MainViews/Division/Index.cshtml");
        }

        public ActionResult CreateIncharge_partial()
        {
            return View("~/Views/Shared/Core/_Namedetails.cshtml");
        }

        public ActionResult CreateContact_partial()
        {

            return View("~/Views/Shared/Core/_Contactdetails.cshtml");
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

        // GET: /Division/

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
        // StateRegion region = db.Region.Include(e => e.Districts).Where(e => e.Id == id).SingleOrDefault();
        // ICollection<District> dist = region.Districts;
        // return new SelectList(dist, "Id", "Code", selectedValues);
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
            //ViewBag.country = GetCountry(0);
            //ViewBag.state = new MultiSelectList("", "", "");
            //ViewBag.region = new MultiSelectList("", "", "");
            //ViewBag.district = new MultiSelectList("", "", "");
            //ViewBag.taluka = new MultiSelectList("", "", "");
            //ViewBag.city = new MultiSelectList("", "", "");
            //ViewBag.area = new MultiSelectList("", "", "");
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

                TempData["RowVersion"] = db.Address.Find(data).RowVersion;
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

                TempData["RowVersion"] = db.Address.Find(data).RowVersion;
                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        public ActionResult Create(Division c, FormCollection form) //Create submit
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
                    string inch = form["InchargeList"] == "0" ? "" : form["InchargeList"];
                    string Addrs = form["Address_List"] == "0" ? "" : form["Address_List"];
                    string ContactDetails = form["Contact_List"] == "0" ? "" : form["Contact_List"];

                    //if (inch != null)
                    //{
                    // if (inch != "")
                    // {
                    // int InchId = Convert.ToInt32(inch);
                    // var vals = db.Employee.Where(e => e.Id == InchId).SingleOrDefault();
                    // c.Incharge = vals;
                    // }


                    if (inch != null)
                    {
                        if (inch != "")
                        {
                            int ContId = Convert.ToInt32(inch);
                            var val = db.Employee.Include(e => e.EmpName)
                            .Where(e => e.Id == ContId).SingleOrDefault();
                            c.Incharge = val;
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

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            if (db.Division.Any(o => o.Code == c.Code))
                            {
                                Msg.Add("  Code Already Exists.  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // return Json(new Object[] { "", "", "Code Already Exists.", JsonRequestBehavior.AllowGet });
                            }

                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            Division Division = new Division()
                            {
                                Code = c.Code == null ? "" : c.Code.Trim(),
                                Name = c.Name == null ? "" : c.Name.Trim(),
                                Incharge = c.Incharge,
                                Address = c.Address,
                                OpeningDate = c.OpeningDate,
                                ContactDetails = c.ContactDetails,
                                DBTrack = c.DBTrack
                            };
                            try
                            {
                                db.Division.Add(Division);
                                var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, c.DBTrack);
                                DT_Division DT_Corp = (DT_Division)rtn_Obj;
                                DT_Corp.Address_Id = c.Address == null ? 0 : c.Address.Id;
                                DT_Corp.Incharge_Id = c.Incharge == null ? 0 : c.Incharge.Id;
                                DT_Corp.ContactDetails_Id = c.ContactDetails == null ? 0 : c.ContactDetails.Id;
                                db.Create(DT_Corp);
                                db.SaveChanges();
                                if (Company != null)
                                {
                                    var Objjob = new List<Division>();
                                    Objjob.Add(Division);
                                    Company.Divisions = Objjob;
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
                                // return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = c.Id });
                            }
                            catch (DataException /* dex */)
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
                        //return Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
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

        /*----------------------Grid ----------------------------*/
        public class CompanyDivision
        {
            public int CompId { get; set; }
            public string CompName { get; set; }
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
            public DBTrack DBTrack { get; set; }
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
                IEnumerable<Division> Company = null;
                IEnumerable<Division> IE;

                if (gp.IsAutho == true)
                {
                    Company = db.Division.Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    FilterSession.Session a = new FilterSession.Session();
                    var b = a.Check_flow();
                    if (b != null)
                    {
                        if (b.type == "master")
                        {
                            Company = db.Company.Include(e => e.Divisions).Where(e => e.Id == b.comp_code).SelectMany(e => e.Divisions).ToList();
                        }
                        else
                        {
                            Company = db.Division.ToList();
                        }
                    }
                    else
                    {
                        Company = db.Division.ToList();
                    }
                    if (Session["object"] != null && Session["object"] != "")
                    {
                        if (Session["object"].ToString() == "object")
                        {
                            Company = db.Division.ToList();
                        }
                        else if (Session["object"].ToString() == "master")
                        {
                            var id = Convert.ToInt32(Session["CompCode"]);

                            if (id != null && id != 0)
                            {
                                //rr = db.Company.Where(e => e.Id == id).ToList();
                                Company = db.Company.Include(e => e.Divisions).Where(e => e.Id == id).SelectMany(e => e.Divisions).ToList();
                            }
                        }
                    }
                    else
                    {
                        Company = db.Division.ToList();
                    }

                }
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = Company;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.Code.ToString().Contains(gp.searchString))
                              || (e.Name.ToUpper().Contains(gp.searchString.ToUpper()))
                              || (e.Id.ToString().Contains(gp.searchString.ToUpper()))
                              ).Select(a => new Object[] { a.Code, a.Name, a.Id }).ToList();
                        //jsonData = IE.Select(a => new { a.Code, a.Name, a.Id }).Where((e => (e.Id.ToString() == gp.searchString) || (e.Code.ToLower() == gp.searchString.ToLower()) || (e.Name.ToLower() == gp.searchString.ToLower()))).ToList();
                        //jsonData = IE.Select(a => new Object[] { Convert.ToString(a.Code), Convert.ToString(a.Name), a.Id }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Code, a.Name, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = Company;
                    Func<Division, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "Code" ? c.Code :
                                         
                                         gp.sidx == "Name" ? c.Name : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.Code), Convert.ToString(a.Name), a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.Code), Convert.ToString(a.Name), a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.Code), Convert.ToString(a.Name), a.Id }).ToList();
                    }
                    totalRecords = Company.Count();
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
        //        IEnumerable<CompanyDivision> Region = null;

        //        List<CompanyDivision> model = new List<CompanyDivision>();
        //        var id = Convert.ToInt32(Session["CompanyId"]);
        //        List<Company> r = new List<Company>();
        //        if (id != null || id != 0)
        //        {
        //            r = db.Company.Where(e => e.Id == id).Include(e => e.Divisions).ToList();
        //        }

        //        var view = new CompanyDivision();
        //        foreach (var i in r)
        //        {
        //            foreach (var z in i.Divisions)
        //            {
        //                view = new CompanyDivision()
        //                {
        //                    CompId = i.Id,
        //                    CompName = i.Name,
        //                    Id = z.Id,
        //                    Code = z.Code,
        //                    Name = z.Name,
        //                    DBTrack = z.DBTrack
        //                };

        //                model.Add(view);
        //            }
        //        }

        //        //var fall = db.Division.ToList();
        //        //var list1 = db.Company.Where(e=>e.Id==id).ToList().SelectMany(e => e.Divisions);
        //        //var list2 = fall.Except(list1);

        //        //foreach (var z in list2)
        //        //{
        //        //    view = new CompanyDivision()
        //        //    {
        //        //        CompId = 0,
        //        //        CompName = "",
        //        //        Id = z.Id,
        //        //        Code = z.Code,
        //        //        Name = z.Name,
        //        //        DBTrack = z.DBTrack
        //        //    };

        //        //    model.Add(view);
        //        //}

        //        Region = model;
        //        IEnumerable<CompanyDivision> IE;
        //        if (gp.IsAutho == true)
        //        {
        //            IE = Region.Where(e => e.DBTrack.IsModified == true).ToList();
        //        }
        //        else
        //        {
        //            IE = Region.ToList();
        //        }
        //        if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
        //        {
        //            //IE = Region;
        //            if (gp.searchOper.Equals("eq"))
        //            {
        //                if (gp.searchField == "CompName")
        //                    jsonData = IE.Select(a => new { a.Id, a.CompName, a.Code, a.Name }).Where((e => (e.CompName.ToString().Contains(gp.searchString)))).ToList();
        //                else if (gp.searchField == "Id")
        //                    jsonData = IE.Select(a => new { a.Id, a.CompName, a.Code, a.Name }).Where((e => (e.Id.ToString().Contains(gp.searchString)))).ToList();
        //                else if (gp.searchField == "Code")
        //                    jsonData = IE.Select(a => new { a.Id, a.CompName, a.Code, a.Name }).Where((e => (e.Code.ToString().Contains(gp.searchString)))).ToList();
        //                else if (gp.searchField == "Name")
        //                    jsonData = IE.Select(a => new { a.Id, a.CompName, a.Code, a.Name }).Where((e => (e.Name.ToString().Contains(gp.searchString)))).ToList();
        //                ////jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.CompName, a.Id, a.Code, a.Name }).ToList();
        //            }
        //            totalRecords = IE.Count();
        //        }
        //        else
        //        {
        //            //IE = Region;
        //            Func<CompanyDivision, dynamic> orderfuc;
        //            if (gp.sidx == "Id")
        //            {
        //                orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
        //            }
        //            else
        //            {
        //                orderfuc = (c => gp.sidx == "CompName" ? c.CompName.ToString() :
        //                gp.sidx == "Code" ? c.Code :
        //                gp.sidx == "Name" ? c.Name : "");
        //            }
        //            if (gp.sord == "asc")
        //            {
        //                IE = IE.OrderBy(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.CompName), Convert.ToString(a.Code), Convert.ToString(a.Name) }).ToList();
        //            }
        //            else if (gp.sord == "desc")
        //            {
        //                IE = IE.OrderByDescending(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.CompName), Convert.ToString(a.Code), Convert.ToString(a.Name) }).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.CompName, a.Code, a.Name }).ToList();
        //            }
        //            totalRecords = Region.Count();
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


        /*-------------------------------------- Get Lookup Details Address ---------------------------------- */

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
            // return View();
        }

        /* -------------------------------- GetLookupDetailsIncharge ----------------------------- */

        [HttpPost]
        public ActionResult GetLookupDetailsIncharge(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.Division.Include(e => e.Incharge).ToList();
                IEnumerable<Division> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.Division.ToList();
                }
                else
                {
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.Incharge.EmpName }).Distinct();
                    //var result_1 = (from c in fall
                    // select new { c.Id, c.DivisionCode, c.DivisionName });
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.Incharge.EmpName}).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            // return View();
        }


        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    Division Divisions = db.Division.Include(e => e.Address)
                    .Include(e => e.ContactDetails)
                    .Where(e => e.Id == data).SingleOrDefault();

                    Address add = Divisions.Address;
                    ContactDetails conDet = Divisions.ContactDetails;
                    if (Divisions.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = Divisions.DBTrack.CreatedBy != null ? Divisions.DBTrack.CreatedBy : null,
                                CreatedOn = Divisions.DBTrack.CreatedOn != null ? Divisions.DBTrack.CreatedOn : null,
                                IsModified = Divisions.DBTrack.IsModified == true ? true : false
                            };
                            Divisions.DBTrack = dbT;
                            db.Entry(Divisions).State = System.Data.Entity.EntityState.Modified;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, Divisions.DBTrack);
                            DT_Division DT_Corp = (DT_Division)rtn_Obj;
                            DT_Corp.Address_Id = Divisions.Address == null ? 0 : Divisions.Address.Id;
                            DT_Corp.ContactDetails_Id = Divisions.ContactDetails == null ? 0 : Divisions.ContactDetails.Id;
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
                                    CreatedBy = Divisions.DBTrack.CreatedBy != null ? Divisions.DBTrack.CreatedBy : null,
                                    CreatedOn = Divisions.DBTrack.CreatedOn != null ? Divisions.DBTrack.CreatedOn : null,
                                    IsModified = Divisions.DBTrack.IsModified == true ? false : false//,
                                };


                                db.Entry(Divisions).State = System.Data.Entity.EntityState.Deleted;
                                var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dbT);
                                DT_Division DT_Corp = (DT_Division)rtn_Obj;
                                DT_Corp.Address_Id = add == null ? 0 : add.Id;
                                DT_Corp.ContactDetails_Id = conDet == null ? 0 : conDet.Id;
                                db.Create(DT_Corp);
                                await db.SaveChangesAsync();
                                ts.Complete();
                                Msg.Add("  Data removed successfully.  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });

                            }
                            catch (RetryLimitExceededException /* dex */)
                            {
                                Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
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

        //[HttpPost]
        //public ActionResult Delete(int? data)
        //{
        // Division d = db.Division.Find(data);
        // try
        // {
        // using (TransactionScope ts = new TransactionScope())
        // {

        // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };
        // db.Entry(d).State = System.Data.Entity.EntityState.Deleted;
        // //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dbT);
        // db.SaveChanges();
        // ts.Complete();
        // //return this.Json(new { msg = "Data removed.", JsonRequestBehavior.AllowGet });
        // return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });

        // // db.Entry(d).State = System.Data.Entity.EntityState.Deleted;
        // // db.SaveChanges();
        // // ts.Complete();
        // // return this.Json(new { msg = "Data removed.", JsonRequestBehavior.AllowGet });

        // }

        // // return this.Json(new { msg = "Data removed.", JsonRequestBehavior.AllowGet });
        // // return Json(new Object[] { "","", "Data removed.", JsonRequestBehavior.AllowGet });

        // }

        // catch (DataException)
        // {
        // return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists contact your system administrator." });
        // }
        //}

        /*------------------- Lookup Details Contact ---------------------- */

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
                    // select new { c.Id, c.DivisionCode, c.DivisionName });
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.FullContactDetails }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            // return View();
        }

        /* --------------------------------- Incharge --------------------------------------*/

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
                    //var result_1 = (from c in fall
                    // select new { c.Id, c.DivisionCode, c.DivisionName });
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.FullNameFML }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            // return View();
        }

		public ActionResult GetEmployee(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
             var fall = db.Employee.Include(e=>e.EmpName).ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                        {
                            fall = db.Employee.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                        }
                        else
                        {
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        }
                    }
                }
                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }
        ///*------------====== ----------------*/

        //public ActionResult GetLookupIncharge(string data)
        //{
        // using (DataBaseContext db = new DataBaseContext())
        // {
        // var fall = db.Employee.Include(e => e.EmpNameDetail).ToList();
        // IEnumerable<Employee> all;
        // if (!string.IsNullOrEmpty(data))
        // {
        // all = db.Employee.ToList().Where(d => d.EmpNameDetail.FullNameFML.Contains(data));
        // }
        // else
        // {
        // var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.EmpNameDetail.FullNameFML }).Distinct();
        // return Json(r, JsonRequestBehavior.AllowGet);
        // }
        // var result = (from c in all
        // select new { c.Id, c.EmpNameDetail.FullNameFML }).Distinct();
        // return Json(result, JsonRequestBehavior.AllowGet);
        // return Json(null, JsonRequestBehavior.AllowGet);
        // }

        // // var fall = db.Division.Include(e=>e.Incharge).ToList();
        // // IEnumerable<Division> all;
        // // if (!string.IsNullOrEmpty(data))
        // // {
        // // all = db.Division.ToList().Where(d => d.Incharge.EmpNameDetail.FullNameFML.Contains(data));
        // // }
        // // else
        // // {
        // // var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.Incharge.EmpNameDetail.FullNameFML }).Distinct();
        // // return Json(r, JsonRequestBehavior.AllowGet);
        // // }
        // // var result = (from c in all
        // // select new { c.Id, c.Incharge.EmpNameDetail.FullNameFML }).Distinct();
        // // return Json(result, JsonRequestBehavior.AllowGet);
        // }


        // }

        //* --------------------------------- Incharge --------------------------------------*/
        // public ActionResult GetLookupIncharge(string data)
        // {
        // using (DataBaseContext db = new DataBaseContext())
        // {
        // var fall = db.NameDetails.ToList();
        // IEnumerable<NameDetails> all;
        // if (!string.IsNullOrEmpty(data))
        // {
        // all = db.NameDetails.ToList().Where(d => d.FullNameFML.Contains(data));
        // }
        // else
        // {
        // var r = (from ca in fall select new { Id = ca.Id,ca.FullNameFML }).Distinct();

        // return Json(r, JsonRequestBehavior.AllowGet);
        // }
        // var result = (from c in all
        // select new { c.Id, c.FullNameFML }).Distinct();
        // return Json(result, JsonRequestBehavior.AllowGet);
        // }
        // }


        /*---------------------- Edit incharge -----------------------------*/

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

                return Json(new object[] { r }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult DelIncharge(int? data, int? forwarddata)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                NameDetails addrs = db.NameDetails.Find(data);
                Division div = db.Division.Find(forwarddata);
                try
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        db.Entry(div).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        ts.Complete();
                    }
                    Msg.Add("  Data removed successfully.  ");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //return this.Json(new { msg = "Data deleted." });
                }

                catch (DataException)
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

        [HttpPost]

        public ActionResult DelContact(int? data, int? forwarddata)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                NameDetails addrs = db.NameDetails.Find(data);
                Division div = db.Division.Find(forwarddata);
                try
                {
                    using (TransactionScope ts = new TransactionScope())
                    {

                        db.Entry(div).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        ts.Complete();
                    }
                    Msg.Add("  Data removed successfully.  ");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    // return Json(new Object[] { "", "", "Data removed.", JsonRequestBehavior.AllowGet });
                }

                catch (DataException)
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ContactDelete(int? data, int? forwarddata)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                ContactDetails contDet = db.ContactDetails.Find(data);
                Division divi = db.Division.Find(forwarddata);
                try
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        divi.ContactDetails = null;
                        db.Entry(divi).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        ts.Complete();
                    }
                    Msg.Add("  Data removed successfully.  ");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    // return this.Json(new { msg = "Data deleted." });
                }

                catch (DataException)
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

        /*---------------------------------------------------------------------------*/

        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.Division.Include(e => e.ContactDetails)
                .Include(e => e.Address)
                .Include(e => e.Incharge)
                .Where(e => e.Id == data)
                .Select(e => new
                {
                    Name = e.Name,
                    Code = e.Code,
                    Action = e.DBTrack.Action,
                    OpeningDate = e.OpeningDate,
                }).ToList();

                var add_data = db.Division
                .Include(e => e.ContactDetails)
                .Include(e => e.Address)
                .Include(e => e.Incharge)
                .Where(e => e.Id == data)
                .Select(e => new
                {
                    Address_FullAddress = e.Address.FullAddress == null ? "" : e.Address.FullAddress,
                    Add_Id = e.Address.Id == null ? "" : e.Address.Id.ToString(),
                    Cont_Id = e.ContactDetails.Id == null ? "" : e.ContactDetails.Id.ToString(),
                    FullContactDetails = e.ContactDetails.FullContactDetails == null ? "" : e.ContactDetails.FullContactDetails,
                    Incharge_Id = e.Incharge.Id == null ? "" : e.Incharge.Id.ToString(),
                    Incharge_Details = e.Incharge.Id == null ? "" : e.Incharge.EmpName.FullDetails
                }).ToList();

                var W = db.DT_Division
                .Include(e => e.ContactDetails_Id)
                .Include(e => e.Address_Id)
                .Include(e => e.Incharge_Id)
                .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                (e => new
                {
                    DT_Id = e.Id,
                    Code = e.Code == null ? "" : e.Code,
                    Name = e.Name == null ? "" : e.Name,
                    Address_Val = e.Address_Id == 0 ? "" : db.Address.Where(x => x.Id == e.Address_Id).Select(x => x.FullAddress).FirstOrDefault(),
                    Contact_Val = e.ContactDetails_Id == 0 ? "" : db.ContactDetails.Where(x => x.Id == e.ContactDetails_Id).Select(x => x.FullContactDetails).FirstOrDefault(),
                }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var divi = db.Division.Find(data);
                TempData["RowVersion"] = divi.RowVersion;
                var Auth = divi.DBTrack.IsModified;
                return Json(new Object[] { Q, add_data, W, Auth, JsonRequestBehavior.AllowGet });
            }
        }

        /*-------------------------------Edit save --------------------------------------------------*/


        [HttpPost]
        public async Task<ActionResult> EditSave(Division c, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string Addrs = form["Address_List"] == "0" ? "" : form["Address_List"];
                    string ContactDetails = form["Contact_List"] == "0" ? "" : form["Contact_List"];
                    string inch = form["Incharge_List"] == "0" ? "" : form["Incharge_List"];
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;

                    c.Address_Id = Addrs != null && Addrs != "" ? int.Parse(Addrs) : 0;
                    c.ContactDetails_Id = ContactDetails != null && ContactDetails != "" ? int.Parse(ContactDetails) : 0;
                    c.Incharge_Id = inch != null && inch != "" ? int.Parse(inch) : 0;

                    //if (Addrs != null)
                    //{
                    //    if (Addrs != "")
                    //    {
                    //        int AddId = Convert.ToInt32(Addrs);
                    //        var val = db.Address.Include(e => e.Area)
                    //        .Include(e => e.City)
                    //        .Include(e => e.Country)
                    //        .Include(e => e.District)
                    //        .Include(e => e.State)
                    //        .Include(e => e.StateRegion)
                    //        .Include(e => e.Taluka)
                    //        .Where(e => e.Id == AddId).SingleOrDefault();
                    //        c.Address = val;
                    //    }
                    //}

                    //if (ContactDetails != null)
                    //{
                    //    if (ContactDetails != "")
                    //    {
                    //        int ContId = Convert.ToInt32(ContactDetails);
                    //        var val = db.ContactDetails.Include(e => e.ContactNumbers)
                    //        .Where(e => e.Id == ContId).SingleOrDefault();
                    //        c.ContactDetails = val;
                    //    }
                    //}

                    if (Auth == false)
                    {
                        if (ModelState.IsValid)
                        {
                            try
                            {
                                //using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))

                                using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, new System.TimeSpan(0, 30, 0)))
                                {
                                    

                                    //using (var context = new DataBaseContext())
                                    //{
                                       
                                  //  }

                                   

                                   // int a = EditS(Addrs, ContactDetails, inch, data, c, c.DBTrack);

                                    //if (inch != null)
                                    //{
                                    //    if (inch != "")
                                    //    {
                                    //        var val = db.Employee.Find(int.Parse(inch));
                                    //        c.Incharge = val;

                                    //        var type = db.Division.Include(e => e.Incharge).Where(e => e.Id == data).SingleOrDefault();
                                    //        IList<Division> typedetails = null;
                                    //        if (type.Incharge != null)
                                    //        {
                                    //            typedetails = db.Division.Where(x => x.Incharge.Id == type.Incharge.Id && x.Id == data).ToList();
                                    //        }
                                    //        else
                                    //        {
                                    //            typedetails = db.Division.Where(x => x.Id == data).ToList();
                                    //        }
                                    //        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                                    //        foreach (var s in typedetails)
                                    //        {
                                    //            s.Incharge = c.Incharge;
                                    //            db.Division.Attach(s);
                                    //            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                    //            //await db.SaveChangesAsync();
                                    //            db.SaveChanges();
                                    //            TempData["RowVersion"] = s.RowVersion;
                                    //            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    //        }
                                    //    }
                                    //}

                                    //if (Addrs != null)
                                    //{
                                    //    if (Addrs != "")
                                    //    {
                                    //        var val = db.Address.Find(int.Parse(Addrs));
                                    //        c.Address = val;

                                    //        var add = db.Division.Include(e => e.Address).Where(e => e.Id == data).SingleOrDefault();
                                    //        IList<Division> addressdetails = null;
                                    //        if (add.Address != null)
                                    //        {
                                    //            addressdetails = db.Division.Where(x => x.Address.Id == add.Address.Id && x.Id == data).ToList();
                                    //        }
                                    //        else
                                    //        {
                                    //            addressdetails = db.Division.Where(x => x.Id == data).ToList();
                                    //        }
                                    //        if (addressdetails != null)
                                    //        {
                                    //            foreach (var s in addressdetails)
                                    //            {
                                    //                s.Address = c.Address;
                                    //                db.Division.Attach(s);
                                    //                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                    //                // await db.SaveChangesAsync(false);
                                    //                db.SaveChanges();
                                    //                TempData["RowVersion"] = s.RowVersion;
                                    //                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    //            }
                                    //        }
                                    //    }
                                    //}
                                    //else
                                    //{
                                    //    var addressdetails = db.Division.Include(e => e.Address).Where(x => x.Id == data).ToList();
                                    //    foreach (var s in addressdetails)
                                    //    {
                                    //        s.Address = null;
                                    //        db.Division.Attach(s);
                                    //        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                    //        //await db.SaveChangesAsync();
                                    //        db.SaveChanges();
                                    //        TempData["RowVersion"] = s.RowVersion;
                                    //        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    //    }
                                    //}

                                    //if (ContactDetails != null)
                                    //{
                                    //    if (ContactDetails != "")
                                    //    {
                                    //        var val = db.ContactDetails.Find(int.Parse(ContactDetails));
                                    //        c.ContactDetails = val;

                                    //        var add = db.Division.Include(e => e.ContactDetails).Where(e => e.Id == data).SingleOrDefault();
                                    //        IList<Division> contactsdetails = null;
                                    //        if (add.ContactDetails != null)
                                    //        {
                                    //            contactsdetails = db.Division.Where(x => x.ContactDetails.Id == add.ContactDetails.Id && x.Id == data).ToList();
                                    //        }
                                    //        else
                                    //        {
                                    //            contactsdetails = db.Division.Where(x => x.Id == data).ToList();
                                    //        }
                                    //        foreach (var s in contactsdetails)
                                    //        {
                                    //            s.ContactDetails = c.ContactDetails;
                                    //            db.Division.Attach(s);
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
                                    //    var contactsdetails = db.Division.Include(e => e.ContactDetails).Where(x => x.Id == data).ToList();
                                    //    foreach (var s in contactsdetails)
                                    //    {
                                    //        s.ContactDetails = null;
                                    //        db.Division.Attach(s);
                                    //        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                    //        //await db.SaveChangesAsync();
                                    //        db.SaveChanges();
                                    //        TempData["RowVersion"] = s.RowVersion;
                                    //        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    //    }
                                    //}




                                    Division Division = db.Division.Find(data);
                                    TempData["CurrRowVersion"] = Division.RowVersion;
                                    //db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {
                                        Division blog = null; // to retrieve old data
                                        DbPropertyValues originalBlogValues = null;
                                        c.DBTrack = new DBTrack
                                        {
                                            CreatedBy = Division.DBTrack.CreatedBy == null ? null : Division.DBTrack.CreatedBy,
                                            CreatedOn = Division.DBTrack.CreatedOn == null ? null : Division.DBTrack.CreatedOn,
                                            Action = "M",
                                            ModifiedBy = SessionManager.UserName,
                                            ModifiedOn = DateTime.Now
                                        };

                                        if (c.Incharge_Id != 0)
                                            Division.Incharge_Id = c.Incharge_Id != null ? c.Incharge_Id : 0;
                                        if (c.Address_Id != 0)
                                            Division.Address_Id = c.Address_Id != null ? c.Address_Id : 0;
                                        if (c.ContactDetails_Id != 0)
                                            Division.ContactDetails_Id = c.ContactDetails_Id != null ? c.ContactDetails_Id : 0;
                                            Division.Code = c.Code;
                                            Division.Name = c.Name;
                                            Division.Id = data;
                                            Division.OpeningDate = c.OpeningDate;
                                            Division.DBTrack = c.DBTrack;
                                       
                                      //  db.Division.Attach(corp);
                                        db.Entry(Division).State = System.Data.Entity.EntityState.Modified;
                                       // db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                        //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                   
                                    //using (var context = new DataBaseContext())
                                    //{
                                    blog = db.Division.Where(e => e.Id == data).Include(e => e.Incharge)
                                       .Include(e => e.Address)
                                       .Include(e => e.ContactDetails).SingleOrDefault();
                                    

                                    originalBlogValues = db.Entry(blog).OriginalValues;
                                    db.ChangeTracker.DetectChanges();
                                        var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                        DT_Division DT_Corp = (DT_Division)obj;
                                        DT_Corp.Address_Id = blog.Address == null ? 0 : blog.Address.Id;
                                        DT_Corp.Incharge_Id = blog.Incharge == null ? 0 : blog.Incharge.Id;
                                        DT_Corp.ContactDetails_Id = blog.ContactDetails == null ? 0 : blog.ContactDetails.Id;
                                        db.Create(DT_Corp);
                                        db.SaveChanges();
                                  //  }
                                    await db.SaveChangesAsync();
                                    }

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
                                    return Json(new Utility.JsonReturnClass { Id = c.Id, Val = c.Name, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new Object[] { , , "Record Updated", JsonRequestBehavior.AllowGet });

                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (Division)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (Division)databaseEntry.ToObject();
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
                            //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                        }

                        else
                        {
                            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {

                                Division blog = null; // to retrieve old data
                                DbPropertyValues originalBlogValues = null;
                                Division Old_Corp = null;

                                using (var context = new DataBaseContext())
                                {
                                    blog = context.Division.Where(e => e.Id == data).SingleOrDefault();
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
                                Division corp = new Division()
                                {
                                    Code = c.Code,
                                    Name = c.Name,
                                    Id = data,
                                    DBTrack = c.DBTrack,
                                    RowVersion = (Byte[])TempData["RowVersion"]
                                };

                                using (var context = new DataBaseContext())
                                {
                                    var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, corp, "Division", c.DBTrack);
                                    Old_Corp = context.Division.Where(e => e.Id == data).Include(e => e.Incharge)
                                    .Include(e => e.Address).Include(e => e.ContactDetails).SingleOrDefault();
                                    DT_Division DT_Corp = (DT_Division)obj;
                                    DT_Corp.Address_Id = DBTrackFile.ValCompare(Old_Corp.Address, c.Address);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
                                    DT_Corp.Incharge_Id = DBTrackFile.ValCompare(Old_Corp.Incharge, c.Incharge); //Old_Corp.Incharge == c.Incharge ? 0 : Old_Corp.Incharge == null && c.Incharge != null ? c.Incharge.Id : Old_Corp.Incharge.Id;
                                    DT_Corp.ContactDetails_Id = DBTrackFile.ValCompare(Old_Corp.ContactDetails, c.ContactDetails); //Old_Corp.ContactDetails == c.ContactDetails ? 0 : Old_Corp.ContactDetails == null && c.ContactDetails != null ? c.ContactDetails.Id : Old_Corp.ContactDetails.Id;
                                    db.Create(DT_Corp);

                                }
                                blog.DBTrack = c.DBTrack;
                                db.Division.Attach(blog);
                                db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                                db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                db.SaveChanges();
                                ts.Complete();
                                Msg.Add("Record Updated");
                                return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = c.Name, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                //return Json(new Object[] { blog.Id, c.Name, "Record Updated", JsonRequestBehavior.AllowGet });
                            }
                        }
                    }
                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            Division blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            Division Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.Division.Where(e => e.Id == data).SingleOrDefault();
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

                            if (TempData["RowVersion"] == null)
                            {
                                TempData["RowVersion"] = blog.RowVersion;
                            }

                            Division corp = new Division()
                            {
                                Code = c.Code,
                                Name = c.Name,
                                Id = data,
                                OpeningDate = c.OpeningDate,
                                DBTrack = c.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };


                            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, corp, "Division", c.DBTrack);
                                // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                Old_Corp = context.Division.Where(e => e.Id == data).Include(e => e.Address).Include(e => e.ContactDetails).SingleOrDefault();
                                DT_Division DT_Corp = (DT_Division)obj;
                                DT_Corp.Address_Id = DBTrackFile.ValCompare(Old_Corp.Address, c.Address);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
                                //DT_Corp.BusinessType_Id = DBTrackFile.ValCompare(Old_Corp., c.BusinessType); //Old_Corp.BusinessType == c.BusinessType ? 0 : Old_Corp.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_Corp.BusinessType.Id;
                                DT_Corp.ContactDetails_Id = DBTrackFile.ValCompare(Old_Corp.ContactDetails, c.ContactDetails); //Old_Corp.ContactDetails == c.ContactDetails ? 0 : Old_Corp.ContactDetails == null && c.ContactDetails != null ? c.ContactDetails.Id : Old_Corp.ContactDetails.Id;
                                db.Create(DT_Corp);
                                //db.SaveChanges();
                            }
                            blog.DBTrack = c.DBTrack;
                            db.Division.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = c.Name, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            //return Json(new Object[] { blog.Id, c.Name, "Record Updated", JsonRequestBehavior.AllowGet });
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

        [HttpPost]
        public ActionResult DeleteAddress(int? data, int? forwarddata)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                Address addrs = db.Address.Find(data);
                Division div = db.Division.Find(forwarddata);
                try
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        div.Address = null;
                        db.Entry(div).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        ts.Complete();
                    }
                    Msg.Add("  Data removed successfully.  ");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //return this.Json(new { msg = "Data deleted." });
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


        public int EditS(string Addrs, string ContactDetails, string Corp, int data, Division c, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
             
                if (Corp != null)
                {
                    if (Corp != "")
                    {
                        var val = db.Employee.Find(int.Parse(Corp));
                        c.Incharge = val;

                        var type = db.Division.Include(e => e.Incharge).Where(e => e.Id == data).SingleOrDefault();
                        IList<Division> typedetails = null;
                        if (type.Incharge != null)
                        {
                            typedetails = db.Division.Where(x => x.Incharge.Id == type.Incharge.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            typedetails = db.Division.Where(x => x.Id == data).ToList();
                        }
                        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                        foreach (var s in typedetails)
                        {
                            s.Incharge = c.Incharge;
                            db.Division.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                }

                if (Addrs != null)
                {
                    if (Addrs != "")
                    {
                        var val = db.Address.Find(int.Parse(Addrs));
                        c.Address = val;

                        var add = db.Division.Include(e => e.Address).Where(e => e.Id == data).SingleOrDefault();
                        IList<Division> addressdetails = null;
                        if (add.Address != null)
                        {
                            addressdetails = db.Division.Where(x => x.Address.Id == add.Address.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            addressdetails = db.Division.Where(x => x.Id == data).ToList();
                        }
                        if (addressdetails != null)
                        {
                            foreach (var s in addressdetails)
                            {
                                s.Address = c.Address;
                                db.Division.Attach(s);
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
                    var addressdetails = db.Division.Include(e => e.Address).Where(x => x.Id == data).ToList();
                    foreach (var s in addressdetails)
                    {
                        s.Address = null;
                        db.Division.Attach(s);
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

                        var add = db.Division.Include(e => e.ContactDetails).Where(e => e.Id == data).SingleOrDefault();
                        IList<Division> contactsdetails = null;
                        if (add.ContactDetails != null)
                        {
                            contactsdetails = db.Division.Where(x => x.ContactDetails.Id == add.ContactDetails.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            contactsdetails = db.Division.Where(x => x.Id == data).ToList();
                        }
                        foreach (var s in contactsdetails)
                        {
                            s.ContactDetails = c.ContactDetails;
                            db.Division.Attach(s);
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
                    var contactsdetails = db.Division.Include(e => e.ContactDetails).Where(x => x.Id == data).ToList();
                    foreach (var s in contactsdetails)
                    {
                        s.ContactDetails = null;
                        db.Division.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }

                var CurCorp = db.Division.Find(data);
                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    c.DBTrack = dbT;
                    Division corp = new Division()
                    {
                        Code = c.Code,
                        Name = c.Name,
                        Id = data,
                        OpeningDate = c.OpeningDate,
                        DBTrack = c.DBTrack
                    };
                    db.Division.Attach(corp);
                    db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                    return 1;
                }
                return 0;
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
                            Division corp = db.Division.Include(e => e.Address)
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

                            db.Division.Attach(corp);
                            db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            //db.SaveChanges();
                            await db.SaveChangesAsync();
                            using (var context = new DataBaseContext())
                            {
                                // corp = db.Division.Include(e => e.Address)
                                //.Include(e => e.ContactDetails)
                                //.Include(e => e.Incharge).FirstOrDefault(e => e.Id == auth_id);
                                //DBTrackFile.DBTrackSave("Core/P2b.Global", "M", corp, null, "Division", corp.DBTrack);
                            }

                            //using (TransactionScope ts1 = new TransactionScope())
                            //{
                            // corp = db.Division.Include(e => e.Address)
                            // .Include(e => e.ContactDetails)
                            // .Include(e => e.Incharge).FirstOrDefault(e => e.Id == auth_id);
                            // DBTrackFile.DBTrackSave("Core/P2b.Global", "M", corp, null, "Division", corp.DBTrack );
                            // db.SaveChanges();
                            // ts1.Complete();
                            //}
                            // db.Entry(corp).State = System.Data.Entity.EntityState.Detached;
                            ts.Complete();
                            Msg.Add("  Record Authorised  ");
                            return Json(new Utility.JsonReturnClass { Id = corp.Id, Val = corp.Name, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { corp.Id, corp.Name, "Record Authorised", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else if (auth_action == "M")
                    {

                        Division Old_Corp = db.Division.Include(e => e.Address).Include(e => e.ContactDetails).Where(e => e.Id == auth_id).SingleOrDefault();



                        DT_Division Curr_Corp = db.DT_Division
                        .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                        .OrderByDescending(e => e.Id)
                        .FirstOrDefault();

                        Division corp = new Division();
                        string Addrs = Curr_Corp.Address_Id == null ? null : Curr_Corp.Address_Id.ToString();
                        string ContactDetails = Curr_Corp.ContactDetails_Id == null ? null : Curr_Corp.ContactDetails_Id.ToString();
                        string inch = Curr_Corp.Incharge_Id == null ? null : Curr_Corp.Incharge_Id.ToString();

                        corp.Name = Curr_Corp.Name == null ? Old_Corp.Name : Curr_Corp.Name;
                        corp.Code = Curr_Corp.Code == null ? Old_Corp.Code : Curr_Corp.Code;

                        if (ModelState.IsValid)
                        {
                            try
                            {
                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
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

                                    int a = EditS(Addrs, ContactDetails, inch, auth_id, corp, corp.DBTrack);
                                    await db.SaveChangesAsync();
                                    ts.Complete();
                                    Msg.Add("  Record Updated  ");
                                    return Json(new Utility.JsonReturnClass { Id = corp.Id, Val = corp.Name, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new Object[] { corp.Id, corp.Name, "Record Updated", JsonRequestBehavior.AllowGet });
                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (Division)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (Division)databaseEntry.ToObject();
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
                    else if (auth_action == "D")
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //Division corp = db.Division.Find(auth_id);
                            Division corp = db.Division.AsNoTracking().Include(e => e.Address)
                                // .Include(e => e.Incharge)
                            .Include(e => e.ContactDetails).FirstOrDefault(e => e.Id == auth_id);

                            Address add = corp.Address;
                            ContactDetails conDet = corp.ContactDetails;
                            // LookupValue val = corp.Incharge;

                            corp.DBTrack = new DBTrack
                            {
                                Action = "D",
                                ModifiedBy = corp.DBTrack.ModifiedBy != null ? corp.DBTrack.ModifiedBy : null,
                                CreatedBy = corp.DBTrack.CreatedBy != null ? corp.DBTrack.CreatedBy : null,
                                CreatedOn = corp.DBTrack.CreatedOn != null ? corp.DBTrack.CreatedOn : null,
                                IsModified = corp.DBTrack.IsModified == true ? false : false,
                                AuthorizedBy = SessionManager.UserName,
                                AuthorizedOn = DateTime.Now
                            };

                            db.Division.Attach(corp);
                            db.Entry(corp).State = System.Data.Entity.EntityState.Deleted;
                            await db.SaveChangesAsync();
                            using (var context = new DataBaseContext())
                            {
                                corp.Address = add;
                                corp.ContactDetails = conDet;
                                // corp.Incharge = val;
                                // DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corp, null, "Division", corp.DBTrack);
                            }

                            db.Entry(corp).State = System.Data.Entity.EntityState.Detached;
                            ts.Complete();
                            Msg.Add(" Record Authorised ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { "", "", "Record Authorised", JsonRequestBehavior.AllowGet });
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
    }
}
