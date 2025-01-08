///
/// Created by Rekha
///

using P2b.Global;
using P2BUltimate.App_Start;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Text;
using P2BUltimate.Models;
using System.Threading.Tasks;
using P2BUltimate.Security;
namespace P2BUltimate.Controllers.Core.MainController
{

    [AuthoriseManger]
    public class AddressController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();

        public ActionResult Index()
        {
            return View("~/Views/Core/MainViews/Address/Index.cshtml");
        }
        public ActionResult Partial()
        {
            //var id = new Test();
            return View("~/Views/Shared/Core/_Address.cshtml");
        }
        public ActionResult GetLookupDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.Address
                    .Include(e => e.Area)
                    .Include(e => e.City)
                    .Include(e => e.Country)
                    .Include(e => e.District)
                    .Include(e => e.District)
                    .Include(e => e.State)
                    .Include(e => e.StateRegion)
                    .Include(e => e.Taluka).ToList();
                if (!string.IsNullOrEmpty(data))
                {
                    var all = fall.Where(d => d.FullAddress.ToString().Contains(data)).ToList();
                    var result = (from c in all
                                  select new { c.Id, c.FullAddress }).Distinct();
                    return Json(result, JsonRequestBehavior.AllowGet);

                }
                else
                {
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullAddress }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);

                }

            }
        }

        [HttpPost]
        public ActionResult Create(Address Addrs, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    //  var Msg = new List<String>();
                    string count = form["CountryList_DDL"] == "0" ? "" : form["CountryList_DDL"];
                    try
                    {
                        if (count != null)
                        {
                            if (count != "")
                            {
                                Country Country = db.Country.Find(Convert.ToInt32(count));
                                Addrs.Country = Country;
                            }
                        }

                        count = form["StateList_DDL"] == "0" ? "" : form["StateList_DDL"];
                        if (count != null)
                        {
                            if (count != "")
                            {
                                State State = db.State.Find(Convert.ToInt32(count));
                                Addrs.State = State;
                            }
                        }

                        count = form["StateRegionList_DDL"] == "0" ? "" : form["StateRegionList_DDL"];
                        if (count != null)
                        {
                            if (count != "")
                            {
                                StateRegion Region = db.StateRegion.Find(Convert.ToInt32(count));
                                Addrs.StateRegion = Region;
                            }
                        }


                        count = form["DistrictList_DDL"] == "0" ? "" : form["DistrictList_DDL"];
                        if (count != null)
                        {
                            if (count != "")
                            {
                                District District = db.District.Find(Convert.ToInt32(count));
                                Addrs.District = District;
                            }
                        }


                        count = form["TalukaList_DDL"] == "0" ? "" : form["TalukaList_DDL"];
                        if (count != null)
                        {
                            if (count != "")
                            {
                                Taluka Taluka = db.Taluka.Find(Convert.ToInt32(count));
                                Addrs.Taluka = Taluka;
                            }
                        }


                        count = form["CityList_DDL"] == "0" ? "" : form["CityList_DDL"];
                        if (count != null)
                        {
                            if (count != "")
                            {
                                City City = db.City.Find(Convert.ToInt32(count));
                                Addrs.City = City;
                            }
                        }


                        count = form["AreaList_DDL"] == "0" ? "" : form["AreaList_DDL"];
                        if (count != null)
                        {
                            if (count != "")
                            {
                                Area Area = db.Area.Find(Convert.ToInt32(count));
                                Addrs.Area = Area;
                            }
                        }

                        if (Addrs.Area != null)
                        {
                            if (Addrs.City == null)
                            {
                                // return this.Json(new Object[] { null, null, "City cannot be null.", JsonRequestBehavior.AllowGet });
                                Msg.Add("City cannot be null.");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }
                        }

                        if (Addrs.City != null)
                        {
                            if (Addrs.Taluka == null)
                            {
                                //  return this.Json(new Object[] { null, null, "Taluka cannot be null.", JsonRequestBehavior.AllowGet });
                                Msg.Add("Taluka cannot be null.");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }
                        }

                        if (Addrs.Taluka != null)
                        {
                            if (Addrs.District == null)
                            {
                                // return this.Json(new Object[] { null, null, "District cannot be null.", JsonRequestBehavior.AllowGet });
                                Msg.Add("District cannot be null.");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }
                        }

                        if (Addrs.District != null)
                        {
                            if (Addrs.StateRegion == null)
                            {
                                //  return this.Json(new Object[] { null, null, "StateRegion cannot be null.", JsonRequestBehavior.AllowGet });
                                Msg.Add("StateRegion cannot be null.");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                        }

                        if (Addrs.StateRegion != null)
                        {
                            if (Addrs.State == null)
                            {
                                // return this.Json(new Object[] { null, null, "State cannot be null.", JsonRequestBehavior.AllowGet });
                                Msg.Add("State cannot be null.");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                        }

                        if (Addrs.State != null)
                        {
                            if (Addrs.Country == null)
                            {
                                //  return this.Json(new Object[] { null, null, "Country cannot be null.", JsonRequestBehavior.AllowGet });
                                Msg.Add("Country cannot be null.");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }
                        }

                        if (ModelState.IsValid)
                        {
                            Addrs.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName };

                            Address add = new Address()
                            {
                                Address1 = Addrs.Address1,
                                Address2 = Addrs.Address2,
                                Address3 = Addrs.Address3,
                                Area = Addrs.Area,
                                City = Addrs.City,
                                Country = Addrs.Country,
                                District = Addrs.District,
                                Landmark = Addrs.Landmark,
                                State = Addrs.State,
                                StateRegion = Addrs.StateRegion,
                                Taluka = Addrs.Taluka,
                                DBTrack = Addrs.DBTrack
                            };


                            using (TransactionScope ts = new TransactionScope())
                            {
                                db.Address.Add(add);
                                var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, add.DBTrack);
                                DT_Address DT_Addrs = (DT_Address)rtn_Obj;
                                DT_Addrs.Country_Id = Addrs.Country == null ? 0 : Addrs.Country.Id;
                                DT_Addrs.State_Id = Addrs.State == null ? 0 : Addrs.State.Id;
                                DT_Addrs.StateRegion_Id = Addrs.StateRegion == null ? 0 : Addrs.StateRegion.Id;
                                DT_Addrs.District_Id = Addrs.District == null ? 0 : Addrs.District.Id;
                                DT_Addrs.Taluka_Id = Addrs.Taluka == null ? 0 : Addrs.Taluka.Id;
                                DT_Addrs.City_Id = Addrs.City == null ? 0 : Addrs.City.Id;
                                DT_Addrs.Area_Id = Addrs.Area == null ? 0 : Addrs.Area.Id;
                                db.Create(DT_Addrs);
                                ////  DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, Addrs.DBTrack);
                                db.SaveChanges();
                                ts.Complete();
                                Msg.Add("Data Saved successfully");
                                return Json(new Utility.JsonReturnClass { Id = add.Id, Val = add.FullAddress, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Object[] { add.Id, add.FullAddress,  }, JsonRequestBehavior.AllowGet);
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
                            // return Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
                        }
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        return RedirectToAction("Create", new { concurrencyError = true, id = Addrs.Id });
                    }
                    catch (Exception e)
                    {
                        Msg.Add(e.Message);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
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
                IEnumerable<Address> Address = null;
                if (gp.IsAutho == true)
                {
                    Address = db.Address.Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    Address = db.Address.AsNoTracking().ToList();
                }

                IEnumerable<Address> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = Address;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Select(a => new { a.Id, a.Address1, a.Address2, a.Address3, a.Landmark }).Where((e => (e.Id.ToString() == gp.searchString) || (e.Address1.SafeToLower() == gp.searchString.SafeToLower()) || (e.Address2.SafeToLower() == gp.searchString.SafeToLower()) || (e.Address3.SafeToLower() == gp.searchString.SafeToLower()) || (e.Landmark.SafeToLower() == gp.searchString.SafeToLower()))).ToList();
                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Address1, a.Address2, a.Address3, a.Landmark }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = Address;
                    //Func<Address, dynamic> orderfuc = (c =>
                    //                                          gp.sidx == "Id" ? Convert.ToString(c.Id) :
                    //                                          gp.sidx == "Address1" ? c.Address1 :
                    //                                          gp.sidx == "Address2" ? c.Address2 :
                    //                                          gp.sidx == "Address3" ? c.Address3 :
                    //                                          gp.sidx == "Landmark" ? c.Landmark : "");

                    Func<Address, dynamic> orderfuc;

                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "Address1" ? c.Address1 :
                                         gp.sidx == "Address2" ? c.Address2 :
                                         gp.sidx == "Address3" ? c.Address3 :
                                         gp.sidx == "Landmark" ? c.Landmark : "");
                    }

                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.Address1, a.Address2, a.Address3, a.Landmark }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.Address1, a.Address2, a.Address3, a.Landmark }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Address1, a.Address2, a.Address3, a.Landmark }).ToList();
                    }
                    totalRecords = Address.Count();
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


        //[HttpPost]
        //public async Task<ActionResult> EditSave(Address Addrs, int data, FormCollection form)
        //{
        //        var  db_data = db.Address
        //                .Include(e => e.City)
        //                .Include(e => e.Country)
        //                .Include(e => e.District)
        //                .Include(e => e.State)
        //                .Include(e => e.Area)
        //                .Include(e => e.StateRegion)
        //                .Include(e => e.Taluka)
        //                .Where(e => e.Id == data).AsNoTracking().SingleOrDefault();
        //    String count = "";
        //     count = form["CountryList_DDL"];

        //    if (count != null && count != "" && count!="0")
        //    {
        //        Country Country = db.Country.Find(Convert.ToInt32(count));
        //        Addrs.Country = Country;
        //    }

        //    count = form["StateList_DDL"];
        //    if (count != null && count != "" && count != "0")
        //    {
        //        State State = db.State.Find(Convert.ToInt32(count));
        //        Addrs.State = State;
        //    }

        //    count = form["StateRegionList_DDL"];
        //    if (count != null && count != "" && count != "0")
        //    {
        //        StateRegion StateRegion = db.StateRegion.Find(Convert.ToInt32(count));
        //        Addrs.StateRegion = StateRegion;
        //    }


        //    count = form["DistrictList_DDL"];
        //    if (count != null && count != "" && count != "0")
        //    {
        //        District District = db.District.Find(Convert.ToInt32(count));
        //        Addrs.District = District;
        //    }


        //    count = form["TalukaList_DDL"];
        //    if (count != null && count != "" && count != "0")
        //    {
        //        Taluka Taluka = db.Taluka.Find(Convert.ToInt32(count));
        //        Addrs.Taluka = Taluka;
        //    }


        //    count = form["CityList_DDL"];
        //    if (count != null && count != "" && count != "0")
        //    {
        //        City City = db.City.Find(Convert.ToInt32(count));
        //        Addrs.City = City;
        //    }


        //    count = form["AreaList_DDL"];
        //    if (count != null && count != "" && count != "0")
        //    {
        //        Area Area = db.Area.Find(Convert.ToInt32(count));
        //        Addrs.Area = Area;
        //    }


        //    if (ModelState.IsValid)
        //    {

        //        try
        //        {
        //            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //            {
        //                if (Addrs.Country != null)
        //                {
        //                    db_data.Country = Addrs.Country;
        //                }
        //                else
        //                {
        //                    db_data.Country = null;
        //                }
        //                if (Addrs.State != null)
        //                {
        //                    db_data.State = Addrs.State;
        //                }
        //                else
        //                {
        //                    db_data.State = null;
        //                }
        //                if (Addrs.StateRegion != null)
        //                {
        //                    db_data.StateRegion = Addrs.StateRegion;
        //                }
        //                else
        //                {
        //                    db_data.StateRegion = null;
        //                }
        //                if (Addrs.District != null)
        //                {
        //                    db_data.District = Addrs.District;
        //                }
        //                else
        //                {
        //                     db_data.District =null;
        //                }
        //                if (Addrs.Taluka != null)
        //                {
        //                    db_data.Taluka = Addrs.Taluka;
        //                }
        //                else
        //                {
        //                    db_data.Taluka = null;
        //                }
        //                if (Addrs.City != null)
        //                {
        //                    db_data.City = Addrs.City;
        //                }
        //                else
        //                {
        //                    db_data.City = null;
        //                }
        //                if (Addrs.Area != null)
        //                {
        //                    db_data.Area = Addrs.Area;
        //                }
        //                else
        //                {
        //                    db_data.Area = null;
        //                }
        //                if (Addrs.Address1 != null)
        //                {
        //                    db_data.Address1 = Addrs.Address1.Trim();
        //                }
        //                if (Addrs.Address2 != null)
        //                {
        //                    db_data.Address2 = Addrs.Address2.Trim();
        //                }
        //                if (Addrs.Address3 != null)
        //                {
        //                    db_data.Address3 = Addrs.Address3.Trim();
        //                }
        //                if (Addrs.Landmark != null)
        //                {
        //                    db_data.Landmark = Addrs.Landmark.Trim();

        //                }

        //                var Curr_Corp = db.Address.Find(data);
        //                TempData["CurrRowVersion"] = Curr_Corp.RowVersion;
        //                db.Entry(Curr_Corp).State = System.Data.Entity.EntityState.Detached;
        //                Address add = null;
        //                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
        //                {
        //                    Address blog = blog = null;
        //                    DbPropertyValues originalBlogValues = null;

        //                    using (var context = new DataBaseContext())
        //                    {
        //                        blog = context.Address.Where(e => e.Id == data).SingleOrDefault();
        //                        originalBlogValues = context.Entry(blog).OriginalValues;
        //                    }

        //                    Addrs.DBTrack = new DBTrack
        //                    {
        //                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                        Action = "M",
        //                        ModifiedBy = SessionManager.UserName,
        //                        ModifiedOn = DateTime.Now
        //                    };
        //                     add = new Address()
        //                    {
        //                        Address1 = Addrs.Address1,
        //                        Address2 = Addrs.Address2,
        //                        Address3 = Addrs.Address3,
        //                        Area = Addrs.Area,
        //                        City = Addrs.City,
        //                        Country = Addrs.Country,
        //                        District = Addrs.District,
        //                        Landmark = Addrs.Landmark,
        //                        State = Addrs.State,
        //                        StateRegion = Addrs.StateRegion,
        //                        Taluka = Addrs.Taluka,
        //                        DBTrack = Addrs.DBTrack,
        //                        Id = data
        //                    };

        //                    db.Address.Attach(add);
        //                    db.Entry(add).State = System.Data.Entity.EntityState.Modified;

        //                    db.Entry(add).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //                  ////  DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, Addrs.DBTrack);
        //                    await db.SaveChangesAsync();
        //                    //DisplayTrackedEntities(db.ChangeTracker);
        //                    db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;

        //                }
        //                ts.Complete();
        //                return Json(new Object[] { add.Id, add.FullAddress, "Data Updated SucessFully", JsonRequestBehavior.AllowGet });
        //                //var full_add = db.Address.Find(db_data.Id);
        //                //var full_ad = full_add.FullAddress;
        //                //ts.Complete();
        //                //return this.Json(new Object[] { db_data.Id, full_ad, "Data Updated SucessFully" }, JsonRequestBehavior.AllowGet);
        //            }
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            return RedirectToAction("Create", new { concurrencyError = true, id = Addrs.Id });
        //        }
        //        catch (DataException /* dex */)
        //        {
        //            return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
        //        }
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
        //        return Json(new Object[]{"","",errorMsg,JsonRequestBehavior.AllowGet});
        //    }
        //}



        [HttpPost]
        public async Task<ActionResult> EditSave(Address add, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string Country = form["CountryList_DDL"] == "0" ? "" : form["CountryList_DDL"];
                    string State = form["StateList_DDL"] == "0" ? "" : form["StateList_DDL"];
                    string StateRegion = form["StateRegionList_DDL"] == "0" ? "" : form["StateRegionList_DDL"];
                    string District = form["DistrictList_DDL"] == "0" ? "" : form["DistrictList_DDL"];
                    string Taluka = form["TalukaList_DDL"] == "0" ? "" : form["TalukaList_DDL"];
                    string City = form["CityList_DDL"] == "0" ? "" : form["CityList_DDL"];
                    string Area = form["AreaList_DDL"] == "0" ? "" : form["AreaList_DDL"];

                    add.Country_Id = Country != null && Country != "" ? int.Parse(Country) : 0;
                    add.State_Id = State != null && State != "" ? int.Parse(State) : 0;
                    add.StateRegion_Id = StateRegion != null && StateRegion != "" ? int.Parse(StateRegion) : 0;
                    add.Taluka_Id = Taluka != null && Taluka != "" ? int.Parse(Taluka) : 0;
                    add.City_Id = City != null && City != "" ? int.Parse(City) : 0;
                    add.Area_Id = Area != null && Area != "" ? int.Parse(Area) : 0;
                    add.District_Id = District != null && District != "" ? int.Parse(District) : 0;

                    // var Msg = new List<String>();
                    //  bool Auth = form["Autho_Action"] == "" ? false : true;
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;
                    try
                    {
                        if (Country != null && Country != "" && Country != "0")
                        {
                            var val = db.Country.Find(int.Parse(Country));
                            add.Country = val;
                        }

                        if (State != null && State != "" && State != "0")
                        {
                            var val = db.State.Find(int.Parse(State));
                            add.State = val;
                        }

                        if (StateRegion != null && StateRegion != "" && StateRegion != "0")
                        {
                            var val = db.StateRegion.Find(int.Parse(StateRegion));
                            add.StateRegion = val;
                        }

                        if (District != null && District != "" && District != "0")
                        {
                            var val = db.District.Find(int.Parse(District));
                            add.District = val;
                        }

                        if (Taluka != null && Taluka != "" && Taluka != "0")
                        {
                            var val = db.Taluka.Find(int.Parse(Taluka));
                            add.Taluka = val;
                        }

                        if (City != null && City != "" && City != "0")
                        {
                            var val = db.City.Find(int.Parse(City));
                            add.City = val;
                        }

                        if (Area != null && Area != "" && Area != "0")
                        {
                            var val = db.Area.Find(int.Parse(Area));
                            add.Area = val;
                        }

                        if (add.Area != null)
                        {
                            if (add.City == null)
                            {
                                Msg.Add("  City cannot be null.  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // return this.Json(new Object[] { null, null, "City cannot be null.", JsonRequestBehavior.AllowGet });
                            }
                        }

                        if (add.City != null)
                        {
                            if (add.Taluka == null)
                            {
                                //  return this.Json(new Object[] { null, null, "Taluka cannot be null.", JsonRequestBehavior.AllowGet });
                                Msg.Add("Taluka cannot be null.");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }
                        }

                        if (add.Taluka != null)
                        {
                            if (add.District == null)
                            {
                                //  return this.Json(new Object[] { null, null, "District cannot be null.", JsonRequestBehavior.AllowGet });
                                Msg.Add("District cannot be null.");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }
                        }

                        if (add.District != null)
                        {
                            if (add.StateRegion == null)
                            {
                                //     return this.Json(new Object[] { null, null, "StateRegion cannot be null.", JsonRequestBehavior.AllowGet });
                                Msg.Add("StateRegion cannot be null.");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }
                        }

                        if (add.StateRegion != null)
                        {
                            if (add.State == null)
                            {
                                // return this.Json(new Object[] { null, null, "State cannot be null.", JsonRequestBehavior.AllowGet });
                                Msg.Add("State cannot be null.");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }
                        }

                        if (add.State != null)
                        {
                            if (add.Country == null)
                            {
                                //  return this.Json(new Object[] { null, null, "Country cannot be null.", JsonRequestBehavior.AllowGet });
                                Msg.Add("Country cannot be null.");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }
                        }

                        if (Auth == false)
                        {


                            if (ModelState.IsValid)
                            {


                                //DbContextTransaction transaction = db.Database.BeginTransaction();

                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {


                                    Address Address = db.Address.Find(data);
                                    TempData["CurrRowVersion"] = Address.RowVersion;
                                    // db.Entry(CurAdrs).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {

                                        Address blog = null; // to retrieve old data
                                        DbPropertyValues originalBlogValues = null;
                                        add.DBTrack = new DBTrack
                                          {
                                              CreatedBy = Address.DBTrack.CreatedBy == null ? null : Address.DBTrack.CreatedBy,
                                              CreatedOn = Address.DBTrack.CreatedOn == null ? null : Address.DBTrack.CreatedOn,
                                              Action = "M",
                                              ModifiedBy = SessionManager.UserName,
                                              ModifiedOn = DateTime.Now
                                          };
                                        

                                        if (add.Country_Id != 0)
                                            Address.Country_Id = add.Country_Id != null ? add.Country_Id : 0;
                                        if (add.State_Id != 0)
                                            Address.State_Id = add.State_Id != null ? add.State_Id : 0;
                                        if (add.StateRegion_Id != 0)
                                            Address.StateRegion_Id = add.StateRegion_Id != null ? add.StateRegion_Id : 0;
                                        if (add.Taluka_Id != 0)
                                            Address.Taluka_Id = add.Taluka_Id != null ? add.Taluka_Id : 0;
                                        if (add.City_Id != 0)
                                            Address.City_Id = add.City_Id != null ? add.City_Id : 0;
                                        if (add.Area_Id != 0)
                                            Address.Area_Id = add.Area_Id != null ? add.Area_Id : 0;
                                        if (add.District_Id != 0)
                                            Address.District_Id = add.District_Id != null ? add.District_Id : 0;
                                        Address.Address1 = add.Address1;
                                        Address.Address2 = add.Address2;
                                        Address.Address3 = add.Address3;
                                        Address.Landmark = add.Landmark;
                                        Address.DBTrack = add.DBTrack;
                                        Address.Id = data;


                                        //db.Address.Attach(add1);
                                        //db.Entry(add1).State = System.Data.Entity.EntityState.Modified;
                                        //db.Entry(add1).OriginalValues["RowVersion"] = TempData["RowVersion"];


                                        blog = db.Address.Where(e => e.Id == data).Include(e => e.Country).Include(e => e.Area).Include(e => e.City)
                                                           .Include(e => e.State).Include(e => e.StateRegion).Include(e => e.Taluka)
                                                           .SingleOrDefault();
                                        originalBlogValues = db.Entry(blog).OriginalValues;

                                        db.ChangeTracker.DetectChanges();
                                        var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, add.DBTrack);
                                        DT_Address DT_Addrs = (DT_Address)obj;
                                        DT_Addrs.Area_Id = blog.Area == null ? 0 : blog.Area.Id;
                                        DT_Addrs.City_Id = blog.City == null ? 0 : blog.City.Id;
                                        DT_Addrs.Country_Id = blog.Country == null ? 0 : blog.Country.Id;
                                        DT_Addrs.District_Id = blog.District == null ? 0 : blog.District.Id;
                                        DT_Addrs.State_Id = blog.State == null ? 0 : blog.State.Id;
                                        DT_Addrs.StateRegion_Id = blog.StateRegion == null ? 0 : blog.StateRegion.Id;
                                        DT_Addrs.Taluka_Id = blog.Taluka == null ? 0 : blog.Taluka.Id;
                                        db.Create(DT_Addrs);
                                        db.SaveChanges();

                                        await db.SaveChangesAsync();
                                        var query = db.Address.Where(e => e.Id == data).Include(e => e.Country).Include(e => e.Area).Include(e => e.City)
                                                                    .Include(e => e.State).Include(e => e.StateRegion).Include(e => e.Taluka)
                                                                    .SingleOrDefault();
                                    }
                                    ts.Complete();
                                    // string FullAddress = Session["FullAddress"].ToString();
                                    Msg.Add("Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = Address.Id, Val = Address.FullAddress, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    //    return Json(new { blog.Id, query.FullAddress, , JsonRequestBehavior.AllowGet });
                                }


                                //  return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                            }
                        }
                        else
                        {
                            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {

                                Address blog = null; // to retrieve old data
                                DbPropertyValues originalBlogValues = null;
                                Address Old_Addrs = null;

                                using (var context = new DataBaseContext())
                                {
                                    blog = context.Address.Where(e => e.Id == data).SingleOrDefault();
                                    originalBlogValues = context.Entry(blog).OriginalValues;
                                }
                                add.DBTrack = new DBTrack
                                {
                                    CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                    CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                    Action = "M",
                                    IsModified = blog.DBTrack.IsModified == true ? true : false,
                                    ModifiedBy = SessionManager.UserName,
                                    ModifiedOn = DateTime.Now
                                };
                                Address addrs = new Address()
                                {
                                    Address1 = add.Address1,
                                    Address2 = add.Address2,
                                    Address3 = add.Address3,
                                    Landmark = add.Landmark,
                                    DBTrack = add.DBTrack,
                                    Id = data
                                };


                                //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                                using (var context = new DataBaseContext())
                                {
                                    var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, add, "Address", add.DBTrack);
                                    // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                    Old_Addrs = context.Address.Where(e => e.Id == data).Include(e => e.Area).Include(e => e.City).Include(e => e.Country)
                                        .Include(e => e.District).Include(e => e.State).Include(e => e.StateRegion).Include(e => e.Taluka).SingleOrDefault();
                                    DT_Address DT_Addrs = (DT_Address)obj;
                                    DT_Addrs.Country_Id = DBTrackFile.ValCompare(Old_Addrs.Country, add.Country);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
                                    DT_Addrs.State_Id = DBTrackFile.ValCompare(Old_Addrs.State, add.State); //Old_Corp.BusinessType == c.BusinessType ? 0 : Old_Corp.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_Corp.BusinessType.Id;
                                    DT_Addrs.StateRegion_Id = DBTrackFile.ValCompare(Old_Addrs.StateRegion, add.StateRegion); //Old_Corp.ContactDetails == c.ContactDetails ? 0 : Old_Corp.ContactDetails == null && c.ContactDetails != null ? c.ContactDetails.Id : Old_Corp.ContactDetails.Id;
                                    DT_Addrs.District_Id = DBTrackFile.ValCompare(Old_Addrs.District, add.District);
                                    DT_Addrs.Taluka_Id = DBTrackFile.ValCompare(Old_Addrs.Taluka, add.Taluka);
                                    DT_Addrs.City_Id = DBTrackFile.ValCompare(Old_Addrs.City, add.City);
                                    DT_Addrs.Area_Id = DBTrackFile.ValCompare(Old_Addrs.Area, add.Area);
                                    db.Create(DT_Addrs);
                                    //db.SaveChanges();
                                }
                                blog.DBTrack = add.DBTrack;
                                db.Address.Attach(blog);
                                db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                                db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                db.SaveChanges();
                                ts.Complete();
                                //  return Json(new Object[] { blog.Id, add.FullAddress, "Record Updated", JsonRequestBehavior.AllowGet });
                                Msg.Add("Record Updated");
                                return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = add.FullAddress, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }

                        }
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        var entry = ex.Entries.Single();
                        var clientValues = (Address)entry.Entity;
                        var databaseEntry = entry.GetDatabaseValues();
                        if (databaseEntry == null)
                        {
                            Msg.Add(ex.Message);
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            var databaseValues = (Address)databaseEntry.ToObject();
                            add.RowVersion = databaseValues.RowVersion;

                        }
                    }
                    catch (Exception e)
                    {
                        Msg.Add(e.Message);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
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


        public int EditS(string Country, string State, string StateRegion, string District, string Taluka, string City, string Area, int data, Address addrs, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                IList<Address> typedetails = null;
                if (Country != null && Country != "")
                {
                    var val = db.Country.Find(int.Parse(Country));
                    addrs.Country = val;

                    var type = db.Address.Include(e => e.Country).Where(e => e.Id == data).SingleOrDefault();
                    if (type.Country != null)
                    {
                        typedetails = db.Address.Where(x => x.Country.Id == type.Country.Id && x.Id == data).ToList();
                    }
                    else
                    {
                        typedetails = db.Address.Where(x => x.Id == data).ToList();
                    }

                    foreach (var s in typedetails)
                    {
                        s.Country = addrs.Country;
                        db.Address.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }
                else
                {
                    var BusiTypeDetails = db.Address.Include(e => e.Country).Where(x => x.Id == data).ToList();
                    foreach (var s in BusiTypeDetails)
                    {
                        s.Country = null;
                        db.Address.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }


                if (State != null && State != "")
                {
                    var val = db.State.Find(int.Parse(State));
                    addrs.State = val;

                    var type = db.Address.Include(e => e.Country).Where(e => e.Id == data).SingleOrDefault();
                    if (type.State != null)
                    {
                        typedetails = db.Address.Where(x => x.State.Id == type.State.Id && x.Id == data).ToList();
                    }
                    else
                    {
                        typedetails = db.Address.Where(x => x.Id == data).ToList();
                    }

                    foreach (var s in typedetails)
                    {
                        s.State = addrs.State;
                        db.Address.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }
                else
                {
                    var BusiTypeDetails = db.Address.Include(e => e.State).Where(x => x.Id == data).ToList();
                    foreach (var s in BusiTypeDetails)
                    {
                        s.State = null;
                        db.Address.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }

                if (StateRegion != null && StateRegion != "")
                {
                    var val = db.StateRegion.Find(int.Parse(StateRegion));
                    addrs.StateRegion = val;

                    var type = db.Address.Include(e => e.StateRegion).Where(e => e.Id == data).SingleOrDefault();
                    if (type.StateRegion != null)
                    {
                        typedetails = db.Address.Where(x => x.StateRegion.Id == type.StateRegion.Id && x.Id == data).ToList();
                    }
                    else
                    {
                        typedetails = db.Address.Where(x => x.Id == data).ToList();
                    }

                    foreach (var s in typedetails)
                    {
                        s.StateRegion = addrs.StateRegion;
                        db.Address.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }
                else
                {
                    var BusiTypeDetails = db.Address.Include(e => e.StateRegion).Where(x => x.Id == data).ToList();
                    foreach (var s in BusiTypeDetails)
                    {
                        s.StateRegion = null;
                        db.Address.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }

                if (District != null && District != "")
                {
                    var val = db.District.Find(int.Parse(StateRegion));
                    addrs.District = val;

                    var type = db.Address.Include(e => e.District).Where(e => e.Id == data).SingleOrDefault();
                    if (type.District != null)
                    {
                        typedetails = db.Address.Where(x => x.District.Id == type.District.Id && x.Id == data).ToList();
                    }
                    else
                    {
                        typedetails = db.Address.Where(x => x.Id == data).ToList();
                    }

                    foreach (var s in typedetails)
                    {
                        s.District = addrs.District;
                        db.Address.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }
                else
                {
                    var BusiTypeDetails = db.Address.Include(e => e.District).Where(x => x.Id == data).ToList();
                    foreach (var s in BusiTypeDetails)
                    {
                        s.District = null;
                        db.Address.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }

                if (Taluka != null && Taluka != "")
                {
                    var val = db.Taluka.Find(int.Parse(Taluka));
                    addrs.Taluka = val;

                    var type = db.Address.Include(e => e.Taluka).Where(e => e.Id == data).SingleOrDefault();
                    if (type.Taluka != null)
                    {
                        typedetails = db.Address.Where(x => x.Taluka.Id == type.Taluka.Id && x.Id == data).ToList();
                    }
                    else
                    {
                        typedetails = db.Address.Where(x => x.Id == data).ToList();
                    }

                    foreach (var s in typedetails)
                    {
                        s.Taluka = addrs.Taluka;
                        db.Address.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }
                else
                {
                    var BusiTypeDetails = db.Address.Include(e => e.Taluka).Where(x => x.Id == data).ToList();
                    foreach (var s in BusiTypeDetails)
                    {
                        s.Taluka = null;
                        db.Address.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }




                if (City != null && City != "")
                {
                    var val = db.City.Find(int.Parse(City));
                    addrs.City = val;

                    var type = db.Address.Include(e => e.City).Where(e => e.Id == data).SingleOrDefault();
                    if (type.City != null)
                    {
                        typedetails = db.Address.Where(x => x.City.Id == type.City.Id && x.Id == data).ToList();
                    }
                    else
                    {
                        typedetails = db.Address.Where(x => x.Id == data).ToList();
                    }

                    foreach (var s in typedetails)
                    {
                        s.City = addrs.City;
                        db.Address.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }
                else
                {
                    var BusiTypeDetails = db.Address.Include(e => e.City).Where(x => x.Id == data).ToList();
                    foreach (var s in BusiTypeDetails)
                    {
                        s.City = null;
                        db.Address.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }



                if (Area != null && Area != "")
                {
                    var val = db.Area.Find(int.Parse(Area));
                    addrs.Area = val;

                    var type = db.Address.Include(e => e.Area).Where(e => e.Id == data).SingleOrDefault();
                    if (type.Area != null)
                    {
                        typedetails = db.Address.Where(x => x.Area.Id == type.Area.Id && x.Id == data).ToList();
                    }
                    else
                    {
                        typedetails = db.Address.Where(x => x.Id == data).ToList();
                    }

                    foreach (var s in typedetails)
                    {
                        s.Area = addrs.Area;
                        db.Address.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }
                else
                {
                    var BusiTypeDetails = db.Address.Include(e => e.Area).Where(x => x.Id == data).ToList();
                    foreach (var s in BusiTypeDetails)
                    {
                        s.Area = null;
                        db.Address.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }


                var CurAdrs = db.Address.Find(data);
                TempData["CurrRowVersion"] = CurAdrs.RowVersion;
                db.Entry(CurAdrs).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    addrs.DBTrack = dbT;
                    Address add = new Address()
                    {
                        Address1 = addrs.Address1,
                        Address2 = addrs.Address2,
                        Address3 = addrs.Address3,
                        Landmark = addrs.Landmark,
                        DBTrack = addrs.DBTrack,
                        Id = data
                    };

                    // Session["FullAddress"] = add.FullAddress;
                    db.Address.Attach(add);
                    db.Entry(add).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(add).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                    return 1;
                }
                return 0;
            }
        }


        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.Address
                      .Include(e => e.Country)
                      .Include(e => e.State)
                      .Include(e => e.StateRegion)
                      .Include(e => e.District)
                      .Include(e => e.Taluka)
                      .Include(e => e.City)
                      .Include(e => e.Area)
                      .Where(e => e.Id == data).Select
                      (e => new
                      {
                          Address1 = e.Address1,
                          Address2 = e.Address2,
                          Address3 = e.Address3,
                          Landmark = e.Landmark,
                          Country_Id = e.Country.Id == null ? 0 : e.Country.Id,
                          State_Id = e.State.Id == null ? 0 : e.State.Id,
                          StateRegion_Id = e.StateRegion.Id == null ? 0 : e.StateRegion.Id,
                          District_Id = e.District.Id == null ? 0 : e.District.Id,
                          Taluka_Id = e.Taluka.Id == null ? 0 : e.Taluka.Id,
                          City_Id = e.City.Id == null ? 0 : e.City.Id,
                          Area_Id = e.Area.Id == null ? 0 : e.Area.Id,
                          Action = e.DBTrack.Action
                      }).ToList();

                var W = db.DT_Address
                     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id,
                         Address1 = e.Address1 == null ? "" : e.Address1,
                         Address2 = e.Address2 == null ? "" : e.Address2,
                         Address3 = e.Address3 == null ? "" : e.Address3,
                         Landmark = e.Landmark == null ? "" : e.Landmark,
                         Country_Val = e.Country_Id == 0 ? "" : db.Country
                                    .Where(x => x.Id == e.Country_Id)
                                    .Select(x => x.Name).FirstOrDefault(),

                         State_Val = e.State_Id == 0 ? "" : db.State.Where(x => x.Id == e.State_Id).Select(x => x.Name).FirstOrDefault(),
                         StateRegion_Val = e.StateRegion_Id == 0 ? "" : db.StateRegion.Where(x => x.Id == e.StateRegion_Id).Select(x => x.Name).FirstOrDefault(),
                         District_Val = e.District_Id == 0 ? "" : db.District.Where(x => x.Id == e.District_Id).Select(x => x.Name).FirstOrDefault(),
                         Taluka_Val = e.Taluka_Id == 0 ? "" : db.Taluka.Where(x => x.Id == e.Taluka_Id).Select(x => x.Name).FirstOrDefault(),
                         City_Val = e.City_Id == 0 ? "" : db.City.Where(x => x.Id == e.City_Id).Select(x => x.Name).FirstOrDefault(),
                         Area_Val = e.Area_Id == 0 ? "" : db.Area.Where(x => x.Id == e.Area_Id).Select(x => x.Name).FirstOrDefault()
                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var Addrs = db.Address.Find(data);
                TempData["RowVersion"] = Addrs.RowVersion;
                var Auth = Addrs.DBTrack.IsModified;
                return Json(new Object[] { Q, "", W, Auth, JsonRequestBehavior.AllowGet });
            }
        }

        //[HttpPost]
        ////[ValidateAntiForgeryToken]
        //public ActionResult  Delete(int data)
        //    {
        //    Address  add = db.Address
        //        .Include(e => e.Area)
        //        .Include(e => e.City)
        //        .Include(e => e.Country)
        //        .Include(e => e.District)
        //        .Include(e => e.State)
        //        .Include(e => e.StateRegion)
        //        .Include(e => e.Taluka)
        //        .Where(e => e.Id == data).SingleOrDefault();
        //    try
        //    {
        //        using (TransactionScope ts = new TransactionScope())
        //        {
        //            DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };
        //            db.Entry(add).State = System.Data.Entity.EntityState.Deleted;
        //           //// DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dbT);
        //            db.SaveChanges();

        //            return this.Json(new { msg = "Data removed.", JsonRequestBehavior.AllowGet });
        //        }
        //    }

        //    catch (DataException e)

        //    {
        //        var exp = e.InnerException.InnerException.Message.ToString();
        //        return Json(new Object[]{"",exp,JsonRequestBehavior.AllowGet});

        //    }
        //    catch (DBConcurrencyException e)
        //    {
        //        var exp = e.InnerException.InnerException.Message.ToString();
        //        return Json(new Object[]{"",exp, JsonRequestBehavior.AllowGet });

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

                    Address Addrs = db.Address.Include(e => e.Area).Include(e => e.City)
                        .Include(e => e.Country)
                        .Include(e => e.District)
                        .Include(e => e.State)
                        .Include(e => e.StateRegion)
                        .Include(e => e.Taluka)
                        .Where(e => e.Id == data).SingleOrDefault();

                    Country count = Addrs.Country;
                    State state = Addrs.State;
                    StateRegion stateReg = Addrs.StateRegion;
                    District dist = Addrs.District;
                    Taluka tal = Addrs.Taluka;
                    City city = Addrs.City;
                    Area area = Addrs.Area;


                    if (Addrs.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack, corporates, null, "Corporate");
                            try
                            {
                                DBTrack dbT = new DBTrack
                                {
                                    Action = "D",
                                    CreatedBy = Addrs.DBTrack.CreatedBy != null ? Addrs.DBTrack.CreatedBy : null,
                                    CreatedOn = Addrs.DBTrack.CreatedOn != null ? Addrs.DBTrack.CreatedOn : null,
                                    IsModified = Addrs.DBTrack.IsModified == true ? true : false
                                };
                                Addrs.DBTrack = dbT;
                                db.Entry(Addrs).State = System.Data.Entity.EntityState.Modified;
                                var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, Addrs.DBTrack);
                                DT_Address DT_Addrs = (DT_Address)rtn_Obj;

                                DT_Addrs.Country_Id = Addrs.Country == null ? 0 : Addrs.Country.Id;
                                DT_Addrs.State_Id = Addrs.State == null ? 0 : Addrs.State.Id;
                                DT_Addrs.StateRegion_Id = Addrs.StateRegion == null ? 0 : Addrs.StateRegion.Id;
                                DT_Addrs.District_Id = Addrs.District == null ? 0 : Addrs.District.Id;
                                DT_Addrs.Taluka_Id = Addrs.Taluka == null ? 0 : Addrs.Taluka.Id;
                                DT_Addrs.City_Id = Addrs.City == null ? 0 : Addrs.City.Id;
                                DT_Addrs.Area_Id = Addrs.Area == null ? 0 : Addrs.Area.Id;
                                db.Create(DT_Addrs);
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
                            catch (Exception ex)
                            {
                                throw ex;
                            }
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
                                    CreatedBy = Addrs.DBTrack.CreatedBy != null ? Addrs.DBTrack.CreatedBy : null,
                                    CreatedOn = Addrs.DBTrack.CreatedOn != null ? Addrs.DBTrack.CreatedOn : null,
                                    IsModified = Addrs.DBTrack.IsModified == true ? false : false//,
                                    //AuthorizedBy = SessionManager.UserName,
                                    //AuthorizedOn = DateTime.Now
                                };

                                // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };

                                db.Entry(Addrs).State = System.Data.Entity.EntityState.Deleted;
                                var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dbT);
                                DT_Address DT_Addrs = (DT_Address)rtn_Obj;
                                DT_Addrs.Country_Id = Addrs.Country == null ? 0 : Addrs.Country.Id;
                                DT_Addrs.State_Id = Addrs.State == null ? 0 : Addrs.State.Id;
                                DT_Addrs.StateRegion_Id = Addrs.StateRegion == null ? 0 : Addrs.StateRegion.Id;
                                DT_Addrs.District_Id = Addrs.District == null ? 0 : Addrs.District.Id;
                                DT_Addrs.Taluka_Id = Addrs.Taluka == null ? 0 : Addrs.Taluka.Id;
                                DT_Addrs.City_Id = Addrs.City == null ? 0 : Addrs.City.Id;
                                DT_Addrs.Area_Id = Addrs.Area == null ? 0 : Addrs.Area.Id;
                                db.Create(DT_Addrs);

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
                                Msg.Add("  Data removed successfully.  ");
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

                            Address Addrs = db.Address.Include(e => e.Area)
                                .Include(e => e.City).Include(e => e.Country).Include(e => e.District).Include(e => e.State)
                                .Include(e => e.StateRegion).Include(e => e.Taluka).FirstOrDefault(e => e.Id == auth_id);

                            Addrs.DBTrack = new DBTrack
                            {
                                Action = "C",
                                ModifiedBy = Addrs.DBTrack.ModifiedBy != null ? Addrs.DBTrack.ModifiedBy : null,
                                CreatedBy = Addrs.DBTrack.CreatedBy != null ? Addrs.DBTrack.CreatedBy : null,
                                CreatedOn = Addrs.DBTrack.CreatedOn != null ? Addrs.DBTrack.CreatedOn : null,
                                IsModified = Addrs.DBTrack.IsModified == true ? false : false,
                                AuthorizedBy = SessionManager.UserName,
                                AuthorizedOn = DateTime.Now
                            };

                            db.Address.Attach(Addrs);
                            db.Entry(Addrs).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(Addrs).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            //db.SaveChanges();
                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, Addrs.DBTrack);
                            DT_Address DT_Addrs = (DT_Address)rtn_Obj;
                            DT_Addrs.Country_Id = Addrs.Country == null ? 0 : Addrs.Country.Id;
                            DT_Addrs.State_Id = Addrs.State == null ? 0 : Addrs.State.Id;
                            DT_Addrs.StateRegion_Id = Addrs.StateRegion == null ? 0 : Addrs.StateRegion.Id;
                            DT_Addrs.District_Id = Addrs.District == null ? 0 : Addrs.District.Id;
                            DT_Addrs.Taluka_Id = Addrs.Taluka == null ? 0 : Addrs.Taluka.Id;
                            DT_Addrs.City_Id = Addrs.City == null ? 0 : Addrs.City.Id;
                            DT_Addrs.Area_Id = Addrs.Area == null ? 0 : Addrs.Area.Id;
                            db.Create(Addrs);
                            await db.SaveChangesAsync();

                            ts.Complete();
                            Msg.Add("  Record Authorised");
                            return Json(new Utility.JsonReturnClass { Id = Addrs.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { Addrs.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else if (auth_action == "M")
                    {

                        Address Old_Addrs = db.Address.Include(e => e.Area).Include(e => e.City).Include(e => e.Country)
                                                          .Include(e => e.District).Include(e => e.State).Include(e => e.StateRegion)
                                                          .Include(e => e.Taluka).Where(e => e.Id == auth_id).SingleOrDefault();



                        DT_Address Curr_Addrs = db.DT_Address
                                                    .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                                    .OrderByDescending(e => e.Id)
                                                    .FirstOrDefault();

                        if (Curr_Addrs != null)
                        {
                            Address add = new Address();

                            string Country = Curr_Addrs.Country_Id == null ? null : Curr_Addrs.Country_Id.ToString();
                            string State = Curr_Addrs.State_Id == null ? null : Curr_Addrs.State_Id.ToString();
                            string StateRegion = Curr_Addrs.StateRegion_Id == null ? null : Curr_Addrs.StateRegion_Id.ToString();
                            string District = Curr_Addrs.District_Id == null ? null : Curr_Addrs.District_Id.ToString();
                            string Taluka = Curr_Addrs.Taluka_Id == null ? null : Curr_Addrs.Taluka_Id.ToString();
                            string City = Curr_Addrs.City_Id == null ? null : Curr_Addrs.City_Id.ToString();
                            string Area = Curr_Addrs.Area_Id == null ? null : Curr_Addrs.Area_Id.ToString();
                            add.Address1 = Curr_Addrs.Address1 == null ? Old_Addrs.Address1 : Curr_Addrs.Address1;
                            add.Address2 = Curr_Addrs.Address2 == null ? Old_Addrs.Address2 : Curr_Addrs.Address2;
                            add.Address3 = Curr_Addrs.Address3 == null ? Old_Addrs.Address3 : Curr_Addrs.Address3;
                            add.Landmark = Curr_Addrs.Landmark == null ? Old_Addrs.Landmark : Curr_Addrs.Landmark;

                            //      corp.Id = auth_id;

                            if (ModelState.IsValid)
                            {
                                try
                                {

                                    //DbContextTransaction transaction = db.Database.BeginTransaction();

                                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                    {
                                        // db.Configuration.AutoDetectChangesEnabled = false;
                                        add.DBTrack = new DBTrack
                                        {
                                            CreatedBy = Old_Addrs.DBTrack.CreatedBy == null ? null : Old_Addrs.DBTrack.CreatedBy,
                                            CreatedOn = Old_Addrs.DBTrack.CreatedOn == null ? null : Old_Addrs.DBTrack.CreatedOn,
                                            Action = "M",
                                            ModifiedBy = Old_Addrs.DBTrack.ModifiedBy == null ? null : Old_Addrs.DBTrack.ModifiedBy,
                                            ModifiedOn = Old_Addrs.DBTrack.ModifiedOn == null ? null : Old_Addrs.DBTrack.ModifiedOn,
                                            AuthorizedBy = SessionManager.UserName,
                                            AuthorizedOn = DateTime.Now,
                                            IsModified = false
                                        };

                                        int a = EditS(Country, State, StateRegion, District, Taluka, City, Area, auth_id, add, add.DBTrack);
                                        //int a = EditS(add, Addrs, ContactDetails, auth_id, corp, corp.DBTrack);

                                        await db.SaveChangesAsync();

                                        ts.Complete();
                                        Msg.Add("  Record Authorised");
                                        return Json(new Utility.JsonReturnClass { Id = add.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        // return Json(new Object[] { add.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                                    }
                                }
                                catch (DbUpdateConcurrencyException ex)
                                {
                                    var entry = ex.Entries.Single();
                                    var clientValues = (Address)entry.Entity;
                                    var databaseEntry = entry.GetDatabaseValues();
                                    if (databaseEntry == null)
                                    {
                                        Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        // return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                    }
                                    else
                                    {
                                        var databaseValues = (Address)databaseEntry.ToObject();
                                        add.RowVersion = databaseValues.RowVersion;
                                    }
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
                            //Corporate corp = db.Corporate.Find(auth_id);
                            Address Addrs = db.Address.Include(e => e.Area).Include(e => e.City).Include(e => e.Country)
                                                          .Include(e => e.District).Include(e => e.State).Include(e => e.StateRegion)
                                                          .Include(e => e.Taluka).FirstOrDefault(e => e.Id == auth_id);

                            Country count = Addrs.Country;
                            State state = Addrs.State;
                            StateRegion stateReg = Addrs.StateRegion;
                            District dist = Addrs.District;
                            Taluka tal = Addrs.Taluka;
                            City city = Addrs.City;
                            Area area = Addrs.Area;
                            try
                            {

                                Addrs.DBTrack = new DBTrack
                                {
                                    Action = "D",
                                    ModifiedBy = Addrs.DBTrack.ModifiedBy != null ? Addrs.DBTrack.ModifiedBy : null,
                                    CreatedBy = Addrs.DBTrack.CreatedBy != null ? Addrs.DBTrack.CreatedBy : null,
                                    CreatedOn = Addrs.DBTrack.CreatedOn != null ? Addrs.DBTrack.CreatedOn : null,
                                    IsModified = false,
                                    AuthorizedBy = SessionManager.UserName,
                                    AuthorizedOn = DateTime.Now
                                };

                                db.Address.Attach(Addrs);
                                db.Entry(Addrs).State = System.Data.Entity.EntityState.Deleted;


                                var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, Addrs.DBTrack);
                                DT_Address DT_Addrs = (DT_Address)rtn_Obj;
                                DT_Addrs.Country_Id = Addrs.Country == null ? 0 : Addrs.Country.Id;
                                DT_Addrs.State_Id = Addrs.State == null ? 0 : Addrs.State.Id;
                                DT_Addrs.StateRegion_Id = Addrs.StateRegion == null ? 0 : Addrs.StateRegion.Id;
                                DT_Addrs.District_Id = Addrs.District == null ? 0 : Addrs.District.Id;
                                DT_Addrs.Taluka_Id = Addrs.Taluka == null ? 0 : Addrs.Taluka.Id;
                                DT_Addrs.City_Id = Addrs.City == null ? 0 : Addrs.City.Id;
                                DT_Addrs.Area_Id = Addrs.Area == null ? 0 : Addrs.Area.Id;
                                db.Create(DT_Addrs);
                                await db.SaveChangesAsync();
                                db.Entry(Addrs).State = System.Data.Entity.EntityState.Detached;
                                ts.Complete();
                                Msg.Add(" Record Authorised ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // return Json(new Object[] { "", "Record Authorised", JsonRequestBehavior.AllowGet });
                            }
                            catch (DataException e)
                            {
                                var exp = e.InnerException.InnerException.Message.ToString();
                                return Json(new Object[] { "", exp, JsonRequestBehavior.AllowGet });

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
                return View();
            }
        }
    }

    public static class StringHelper
    {
        public static string SafeToLower(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }
            return value.ToLower();
        }
    }
}

