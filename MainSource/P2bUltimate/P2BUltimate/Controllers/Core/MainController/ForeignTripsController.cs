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
    public class ForeignTripsController : Controller
    {
        public ActionResult Index()
        {
            return View("~/Views/Core/MainViews/ForeignTrips/Index.cshtml");
        }
        //private DataBaseContext db = new DataBaseContext();
        public ActionResult Create(ForeignTrips Addrs, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string Corp = form["Category"] == "0" ? "" : form["Category"];
                    int Emp = form["Employee-Table"] == "0" ? 0 : Convert.ToInt32(form["Employee-Table"]);
                    if (Corp != null)
                    {
                        if (Corp != "")
                        {
                            var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "305").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(Corp)).FirstOrDefault();  //db.LookupValue.Find(int.Parse(Corp));
                            Addrs.TripType = val;
                        }
                    }
                    string count = form["CountryList"] == "0" ? "" : form["CountryList"];

                    if (count != null)
                    {
                        if (count != "")
                        {
                            Country Country = db.Country.Find(Convert.ToInt32(count));
                            Addrs.Country = Country;
                        }
                    }

                    count = form["StateList"] == "0" ? "" : form["StateList"];
                    if (count != null)
                    {
                        if (count != "")
                        {
                            State State = db.State.Find(Convert.ToInt32(count));
                            Addrs.State = State;
                        }
                    }


                    var citycount = form["CityList"] == "0" ? "" : form["CityList"];
                    if (citycount != null)
                    {
                        if (citycount != "")
                        {
                            var City = db.City.Find(Convert.ToInt32(citycount));
                            Addrs.City = City;
                        }
                    }

                    Employee empdata;
                    if (Emp != null && Emp != 0)
                    {
                        empdata = db.Employee.Find(Emp);
                    }
                    else
                    {
                        Msg.Add("  Kindly select employee  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    if (ModelState.IsValid)
                    {
                        Addrs.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName };

                        var EmpFortrips = db.Employee.Include(e => e.ForeignTrips).Include(e => e.ForeignTrips.Select(q => q.City))
                                    .Include(e => e.ForeignTrips.Select(q => q.Country))
                                     .Include(e => e.ForeignTrips.Select(q => q.State))
                                    .Include(e => e.ForeignTrips.Select(q => q.TripType))
                                   .Where(e => e.Id != null).ToList();
                        foreach (var item in EmpFortrips)
                        {
                            if (item.ForeignTrips.Count != 0 && empdata.ForeignTrips.Count != 0)
                            {
                                int aid = item.ForeignTrips.Select(a => a.Id).SingleOrDefault();
                                int bid = empdata.ForeignTrips.Select(a => a.Id).SingleOrDefault();

                                if (aid == bid)
                                {
                                    var v = empdata.EmpCode;
                                    Msg.Add("Record Already Exist For Employee Code=" + v);
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }
                            }

                        }
                        if (Addrs.FromDate > Addrs.ToDate)
                        {
                            Msg.Add(" FromD Date Should Be Less Than To Date ");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }


                        ForeignTrips add = new ForeignTrips()
                        {
                            Purpose = Addrs.Purpose,
                            ToDate = Addrs.ToDate,
                            TripType = Addrs.TripType,
                            FromDate = Addrs.FromDate,
                            City = Addrs.City,
                            Country = Addrs.Country,
                            State = Addrs.State,
                            DBTrack = Addrs.DBTrack
                        };

                        try
                        {
                            using (TransactionScope ts = new TransactionScope())
                            {
                                db.ForeignTrips.Add(add);
                                var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, add.DBTrack);
                                DT_ForeignTrips DT_Addrs = (DT_ForeignTrips)rtn_Obj;
                                DT_Addrs.Country_Id = Addrs.Country == null ? 0 : Addrs.Country.Id;
                                //DT_Addrs.State_Id = Addrs.State == null ? 0 : Addrs.State.Id;
                                DT_Addrs.City_Id = Addrs.City == null ? 0 : Addrs.City.Id;
                                DT_Addrs.TripType_Id = Addrs.TripType == null ? 0 : Addrs.TripType.Id;

                                db.Create(DT_Addrs);
                                db.SaveChanges();


                                List<ForeignTrips> empForeignTrips = new List<ForeignTrips>();
                                empForeignTrips.Add(add);
                                empdata.ForeignTrips = empForeignTrips;
                                db.Employee.Attach(empdata);
                                db.Entry(empdata).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();

                                ts.Complete();
                                //  return Json(new Object[] { add.Id, add.FullDetails, "Data Saved successfully" }, JsonRequestBehavior.AllowGet);
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = add.Id, Val = add.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }
                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            return RedirectToAction("Create", new { concurrencyError = true, id = Addrs.Id });
                        }
                        catch (DataException /* dex */)
                        {
                            //   return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
                            Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);


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
        //        IEnumerable<ForeignTrips> ForeignTrips = null;
        //        if (gp.IsAutho == true)
        //        {
        //            ForeignTrips = db.ForeignTrips.Include(e => e.TripType).Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
        //        }
        //        else
        //        {
        //            ForeignTrips = db.ForeignTrips.Include(e => e.TripType).AsNoTracking().ToList();
        //        }

        //        IEnumerable<ForeignTrips> IE;
        //        if (!string.IsNullOrEmpty(gp.searchField))
        //        {
        //            IE = ForeignTrips;
        //            if (gp.searchOper.Equals("eq"))
        //            {
        //                jsonData = IE.Select(a => new { a.Id, a.ToDate, a.FromDate, a.TripType }).Where((e => (e.Id.ToString() == gp.searchString) || (e.ToDate.ToString() == gp.searchString.ToLower()) || (e.FromDate.ToString() == gp.searchString.ToLower()))).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, Convert.ToString(a.ToDate), Convert.ToString(a.FromDate), a.TripType != null ? Convert.ToString(a.TripType.LookupVal) : "" }).ToList();
        //            }
        //            totalRecords = IE.Count();
        //        }
        //        else
        //        {
        //            IE = ForeignTrips;

        //            Func<ForeignTrips, dynamic> orderfuc;

        //            if (gp.sidx == "Id")
        //            {
        //                orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
        //            }
        //            else
        //            {
        //                orderfuc = (c => gp.sidx == "ToDate" ? Convert.ToString(c.ToDate) :
        //                                 gp.sidx == "FromDate" ? Convert.ToString(c.FromDate) :
        //                                 gp.sidx == "TripType" ? Convert.ToString(c.TripType) : " ");
        //            }

        //            if (gp.sord == "asc")
        //            {
        //                IE = IE.OrderBy(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, a.ToDate.Value.ToShortDateString(), a.FromDate.Value.ToShortDateString(), a.TripType != null ? Convert.ToString(a.TripType.LookupVal) : "" }).ToList();
        //            }
        //            else if (gp.sord == "desc")
        //            {
        //                IE = IE.OrderByDescending(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, a.ToDate.Value.ToShortDateString(), a.FromDate.Value.ToShortDateString(), a.TripType != null ? Convert.ToString(a.TripType.LookupVal) : "" }).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.ToDate.Value.ToShortDateString(), a.FromDate.Value.ToShortDateString(), a.TripType != null ? Convert.ToString(a.TripType.LookupVal) : "" }).ToList();
        //            }
        //            totalRecords = ForeignTrips.Count();
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

                IEnumerable<Employee> Employee = null;
                Employee = db.Employee.Include(q => q.ForeignTrips).Include(q => q.EmpName).ToList();
                if (gp.IsAutho == true)
                {
                    Employee = db.Employee.Include(q => q.ForeignTrips).Include(q => q.EmpName).Where(e => e.DBTrack.IsModified == true).ToList();
                }
                else
                {
                    // Employee = db.Employee.Where(a=>a.Id==db.EmpAcademicInfo.Contains()).ToList();
                    Employee = db.Employee.Include(q => q.ForeignTrips).Include(q => q.EmpName).Where(q => q.ForeignTrips.Count > 0).ToList();
                }
                IEnumerable<Employee> IE;

                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = Employee;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => 
                               (e.EmpCode.ToString().Contains(gp.searchString.ToString())) 
                               || (e.EmpName.FullNameFML.ToUpper().Contains(gp.searchString.ToUpper()))
                               || (e.Id.ToString().Contains(gp.searchString.ToString())))
                           .Select(a => new Object[] { a.EmpCode, a.EmpName.FullNameFML, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.EmpCode, a.EmpName.FullNameFML, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = Employee;
                    Func<Employee, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "EmpCode" ? c.EmpCode.ToString() :
                                         gp.sidx == "EmpName" ? c.EmpName.FullNameFML.ToString() :
                                          "");
                    }



                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] {  a.EmpCode, a.EmpName.FullNameFML, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] {  a.EmpCode, a.EmpName.FullNameFML, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.EmpCode, a.EmpName.FullNameFML, a.Id }).ToList();
                    }
                    totalRecords = Employee.Count();
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

        //public ActionResult Edit(int data)
        //{
        //    var Q = db.ForeignTrips
        //          .Include(e => e.Country)
        //          .Include(e => e.State)

        //          .Include(e => e.City)
        //          .Include(e => e.TripType)
        //          .Where(e => e.Id == data).Select
        //          (e => new
        //          {
        //              FromDate = e.FromDate,
        //              ToDate = e.ToDate,
        //              Purpose = e.Purpose,

        //              Country_Id = e.Country.Id == null ? 0 : e.Country.Id,
        //              State_Id = e.State.Id == null ? 0 : e.State.Id,

        //              City_Id = e.City.Id == null ? 0 : e.City.Id,
        //              TripType_Id = e.TripType.Id == null ? 0 : e.TripType.Id,
        //              Action = e.DBTrack.Action
        //          }).ToList();

        //    var W = db.DT_ForeignTrips
        //         .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
        //         (e => new
        //         {
        //             DT_Id = e.Id,
        //             FromDate = e.FromDate,
        //             ToDate = e.ToDate,
        //             Purpose = e.Purpose == null ? "" : e.Purpose,

        //             Country_Val = e.Country_Id == 0 ? "" : db.Country
        //                        .Where(x => x.Id == e.Country_Id)
        //                        .Select(x => x.Name).FirstOrDefault(),


        //             City_Val = e.City_Id == 0 ? "" : db.City.Where(x => x.Id == e.City_Id).Select(x => x.Name).FirstOrDefault(),
        //             //  Triptype_Val = e.TripType_Id == 0 ? "" : db.ForeignTrips.Where(x => x.Id == e.TripType_Id).Select(x =>x.Id).FirstOrDefault()
        //         }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

        //    var Addrs = db.ForeignTrips.Find(data);
        //    TempData["RowVersion"] = Addrs.RowVersion;
        //    var Auth = Addrs.DBTrack.IsModified;
        //    return Json(new Object[] { Q, "", W, Auth, JsonRequestBehavior.AllowGet });
        //}

        //public ActionResult Edit(int data)
        //{
        //    var Q = db.Employee.Include(e=>e.ForeignTrips)
        //          .Include(e => e.ForeignTrips.Select(e=>e.Country))
        //          .Include(e => e.ForeignTrips.State)

        //          .Include(e => e.ForeignTrips.City)
        //          .Include(e => e.ForeignTrips.TripType)
        //          .Where(e => e.Id == data).Select
        //          (e => new
        //          {
        //              FromDate = e.FromDate,
        //              ToDate = e.ToDate,
        //              Purpose = e.Purpose,

        //              Country_Id = e.Country.Id == null ? 0 : e.Country.Id,
        //              State_Id = e.State.Id == null ? 0 : e.State.Id,

        //              City_Id = e.City.Id == null ? 0 : e.City.Id,
        //              TripType_Id = e.TripType.Id == null ? 0 : e.TripType.Id,
        //              Action = e.DBTrack.Action
        //          }).ToList();

        //    var W = db.DT_ForeignTrips
        //         .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
        //         (e => new
        //         {
        //             DT_Id = e.Id,
        //             FromDate = e.FromDate,
        //             ToDate = e.ToDate,
        //             Purpose = e.Purpose == null ? "" : e.Purpose,

        //             Country_Val = e.Country_Id == 0 ? "" : db.Country
        //                        .Where(x => x.Id == e.Country_Id)
        //                        .Select(x => x.Name).FirstOrDefault(),


        //             City_Val = e.City_Id == 0 ? "" : db.City.Where(x => x.Id == e.City_Id).Select(x => x.Name).FirstOrDefault(),
        //             //  Triptype_Val = e.TripType_Id == 0 ? "" : db.ForeignTrips.Where(x => x.Id == e.TripType_Id).Select(x =>x.Id).FirstOrDefault()
        //         }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

        //    var Addrs = db.ForeignTrips.Find(data);
        //    TempData["RowVersion"] = Addrs.RowVersion;
        //    var Auth = Addrs.DBTrack.IsModified;
        //    return Json(new Object[] { Q, "", W, Auth, JsonRequestBehavior.AllowGet });
        //}

        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.Employee.Include(e => e.ForeignTrips)
                      .Include(e => e.ForeignTrips.Select(a => a.Country))
                      .Include(e => e.ForeignTrips.Select(a => a.State))

                      .Include(e => e.ForeignTrips.Select(a => a.City))
                      .Include(e => e.ForeignTrips.Select(a => a.TripType))
                      .Where(e => e.Id == data).FirstOrDefault();

                int AID = Q.ForeignTrips.Select(a => a.Id).FirstOrDefault();
                var asd = Q.ForeignTrips.Select
                      (e => new
                      {
                          FromDate = e.FromDate,
                          ToDate = e.ToDate,
                          Purpose = e.Purpose,

                          Country_Id = e.Country == null ? 0 : e.Country.Id,
                          State_Id = e.State == null ? 0 : e.State.Id,

                          City_Id = e.City == null ? 0 : e.City.Id,
                          TripType_Id = e.TripType == null ? 0 : e.TripType.Id,
                          Action = e.DBTrack.Action
                      }).ToList();

                var W = db.DT_ForeignTrips
                     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id,
                         FromDate = e.FromDate,
                         ToDate = e.ToDate,
                         Purpose = e.Purpose == null ? "" : e.Purpose,

                         Country_Val = e.Country_Id == 0 ? "" : db.Country
                                    .Where(x => x.Id == e.Country_Id)
                                    .Select(x => x.Name).FirstOrDefault(),


                         City_Val = e.City_Id == 0 ? "" : db.City.Where(x => x.Id == e.City_Id).Select(x => x.Name).FirstOrDefault(),
                         //  Triptype_Val = e.TripType_Id == 0 ? "" : db.ForeignTrips.Where(x => x.Id == e.TripType_Id).Select(x =>x.Id).FirstOrDefault()
                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var Addrs = db.ForeignTrips.Find(AID);
                TempData["RowVersion"] = Addrs.RowVersion;
                var Auth = Addrs.DBTrack.IsModified;
                return Json(new Object[] { asd, "", W, Auth, JsonRequestBehavior.AllowGet });
            }
        }


        public async Task<ActionResult> EditSave(ForeignTrips c, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {

                    var Q = db.Employee.Include(e => e.ForeignTrips)
                      .Include(e => e.ForeignTrips.Select(a => a.Country))
                      .Include(e => e.ForeignTrips.Select(a => a.State))

                      .Include(e => e.ForeignTrips.Select(a => a.City))
                      .Include(e => e.ForeignTrips.Select(a => a.TripType))
                      .Where(e => e.Id == data).FirstOrDefault();

                    int AID = Q.ForeignTrips.Select(a => a.Id).FirstOrDefault();


                    string TripType = form["Category"] == "0" ? "" : form["Category"];
                    string Country = form["CountryList"] == "0" ? "" : form["CountryList"];
                    string State = form["StateList"] == "0" ? "" : form["StateList"];
                    string City = form["CityList"] == "0" ? "" : form["CityList"];
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;



                    if (TripType != null)
                    {
                        if (TripType != "")
                        {
                            var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "305").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(TripType)).FirstOrDefault();  //db.LookupValue.Find(int.Parse(TripType));
                            c.TripType = val;
                        }
                    }

                    if (Country != null && Country != "" && Country != "0")
                    {
                        var val = db.Country.Find(int.Parse(Country));
                        c.Country = val;
                    }

                    if (State != null && State != "" && State != "0")
                    {
                        var val = db.State.Find(int.Parse(State));
                        c.State = val;
                    }

                    if (City != null && City != "" && City != "0")
                    {
                        var val = db.City.Find(int.Parse(City));
                        c.City = val;
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
                                    ForeignTrips blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.ForeignTrips.Where(e => e.Id == AID).Include(e => e.TripType)
                                                                .Include(e => e.Country).Include(e => e.State)
                                                                .Include(e => e.City).SingleOrDefault();
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

                                  //  int a = EditS(Country, State, City, TripType, AID, c, c.DBTrack);


                                    IList<ForeignTrips> typedetails = null;
                                    if (Country != null && Country != "")
                                    {
                                        var val = db.Country.Find(int.Parse(Country));
                                        c.Country = val;

                                        var type = db.ForeignTrips.Include(e => e.Country).Where(e => e.Id == AID).SingleOrDefault();
                                        if (type.Country != null)
                                        {
                                            typedetails = db.ForeignTrips.Where(x => x.Country.Id == type.Country.Id && x.Id == AID).ToList();
                                        }
                                        else
                                        {
                                            typedetails = db.ForeignTrips.Where(x => x.Id == AID).ToList();
                                        }

                                        foreach (var s in typedetails)
                                        {
                                            s.Country = c.Country;
                                            db.ForeignTrips.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }
                                    else
                                    {
                                        var BusiTypeDetails = db.ForeignTrips.Include(e => e.Country).Where(x => x.Id == AID).ToList();
                                        foreach (var s in BusiTypeDetails)
                                        {
                                            s.Country = null;
                                            db.ForeignTrips.Attach(s);
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
                                        c.State = val;

                                        var type = db.ForeignTrips.Include(e => e.Country).Where(e => e.Id == AID).SingleOrDefault();
                                        if (type.State != null)
                                        {
                                            typedetails = db.ForeignTrips.Where(x => x.State.Id == type.State.Id && x.Id == AID).ToList();
                                        }
                                        else
                                        {
                                            typedetails = db.ForeignTrips.Where(x => x.Id == AID).ToList();
                                        }

                                        foreach (var s in typedetails)
                                        {
                                            s.State = c.State;
                                            db.ForeignTrips.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }
                                    else
                                    {
                                        var BusiTypeDetails = db.ForeignTrips.Include(e => e.State).Where(x => x.Id == AID).ToList();
                                        foreach (var s in BusiTypeDetails)
                                        {
                                            s.State = null;
                                            db.ForeignTrips.Attach(s);
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
                                        c.City = val;

                                        var type = db.ForeignTrips.Include(e => e.City).Where(e => e.Id == AID).SingleOrDefault();
                                        if (type.City != null)
                                        {
                                            typedetails = db.ForeignTrips.Where(x => x.City.Id == type.City.Id && x.Id == AID).ToList();
                                        }
                                        else
                                        {
                                            typedetails = db.ForeignTrips.Where(x => x.Id == AID).ToList();
                                        }

                                        foreach (var s in typedetails)
                                        {
                                            s.City = c.City;
                                            db.ForeignTrips.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }
                                    else
                                    {
                                        var BusiTypeDetails = db.ForeignTrips.Include(e => e.City).Where(x => x.Id == AID).ToList();
                                        foreach (var s in BusiTypeDetails)
                                        {
                                            s.City = null;
                                            db.ForeignTrips.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }

                                    if (TripType != null)
                                    {
                                        if (TripType != "")
                                        {
                                            var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "305").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(TripType)).FirstOrDefault(); //db.LookupValue.Find(int.Parse(TripType));
                                            c.TripType = val;

                                            var type = db.ForeignTrips.Include(e => e.TripType).Where(e => e.Id == AID).SingleOrDefault();
                                            // IList<ForeignTrips> typedetails = null;
                                            if (type.TripType != null)
                                            {
                                                typedetails = db.ForeignTrips.Where(x => x.TripType.Id == type.TripType.Id && x.Id == AID).ToList();
                                            }
                                            else
                                            {
                                                typedetails = db.ForeignTrips.Where(x => x.Id == AID).ToList();
                                            }
                                            //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                                            foreach (var s in typedetails)
                                            {
                                                s.TripType = c.TripType;
                                                db.ForeignTrips.Attach(s);
                                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                                //await db.SaveChangesAsync();
                                                db.SaveChanges();
                                                TempData["RowVersion"] = s.RowVersion;
                                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                            }
                                        }
                                        else
                                        {
                                            var BusiTypeDetails = db.ForeignTrips.Include(e => e.TripType).Where(x => x.Id == AID).ToList();
                                            foreach (var s in BusiTypeDetails)
                                            {
                                                s.TripType = null;
                                                db.ForeignTrips.Attach(s);
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
                                        var BusiTypeDetails = db.ForeignTrips.Include(e => e.TripType).Where(x => x.Id == AID).ToList();
                                        foreach (var s in BusiTypeDetails)
                                        {
                                            s.TripType = null;
                                            db.ForeignTrips.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }


                                    var CurAdrs = db.ForeignTrips.Find(AID);
                                    TempData["CurrRowVersion"] = CurAdrs.RowVersion;
                                    db.Entry(CurAdrs).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {
                                        c.DBTrack = c.DBTrack;
                                        ForeignTrips add = new ForeignTrips()
                                        {
                                            FromDate = c.FromDate,

                                            ToDate = c.ToDate,
                                            Purpose = c.Purpose,
                                            //  Landmark = addrs.Landmark,
                                            DBTrack = c.DBTrack,
                                            Id = AID
                                        };

                                        // Session["FullAddress"] = add.FullAddress;
                                        db.ForeignTrips.Attach(add);
                                        db.Entry(add).State = System.Data.Entity.EntityState.Modified;
                                        db.Entry(add).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                        //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                        
                                    }


                                    using (var context = new DataBaseContext())
                                    {
                                        //c.Id = data;

                                        /// DBTrackFile.DBTrackSave("Core/P2b.Global", "M", blog, c, "Corporate", c.DBTrack);
                                        //PropertyInfo[] fi = null;
                                        //Dictionary<string, object> rt = new Dictionary<string, object>();
                                        //fi = c.GetType().GetProperties();
                                        ////foreach (var Prop in fi)
                                        ////{
                                        ////    if (Prop.Name != "Id" && Prop.Name != "DBTrack" && Prop.Name != "RowVersion")
                                        ////    {
                                        ////        rt.Add(Prop.Name, Prop.GetValue(c));
                                        ////    }
                                        ////}
                                        //rt = blog.DetailedCompare(c);
                                        //rt.Add("Orig_Id", c.Id);
                                        //rt.Add("Action", "M");
                                        //rt.Add("DBTrack", c.DBTrack);
                                        //rt.Add("RowVersion", c.RowVersion);
                                        //var aa = DBTrackFile.GetInstance("Core/P2b.Global", "DT_" + "Corporate", rt);
                                        //DT_Corporate d = (DT_Corporate)aa;
                                        //db.DT_Corporate.Add(d);
                                        //db.SaveChanges();

                                        //To save data in history table 
                                        //var Obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, c, "Corporate", c.DBTrack);
                                        //DT_Corporate DT_Corp = (DT_Corporate)Obj;
                                        //db.DT_Corporate.Add(DT_Corp);
                                        //db.SaveChanges();\


                                        var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                        DT_ForeignTrips DT_Corp = (DT_ForeignTrips)obj;
                                        DT_Corp.TripType_Id = blog.TripType == null ? 0 : blog.TripType.Id;
                                        DT_Corp.Country_Id = blog.Country == null ? 0 : blog.Country.Id;
                                        DT_Corp.City_Id = blog.City == null ? 0 : blog.City.Id;
                                        //  DT_Corp.State_Id = blog.State == null ? 0 : blog.State.Id;
                                        //    DT_Corp. = blog.City == null ? 0 : blog.City.Id;
                                        db.Create(DT_Corp);
                                        db.SaveChanges();
                                    }
                                    await db.SaveChangesAsync();
                                    ts.Complete();

                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = c.Id, Val = c.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    //  return Json(new Object[] { c.Id, c.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });

                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (ForeignTrips)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    //   return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                    Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }
                                else
                                {
                                    var databaseValues = (ForeignTrips)databaseEntry.ToObject();
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

                            ForeignTrips blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            ForeignTrips Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.ForeignTrips.Where(e => e.Id == AID).SingleOrDefault();
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

                            ForeignTrips corp = new ForeignTrips()
                            {
                                //   Code = c.FromDate,
                                Purpose = c.Purpose,
                                Id = AID,
                                DBTrack = c.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };


                            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, corp, "ForeignTrips", c.DBTrack);
                                // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                Old_Corp = context.ForeignTrips.Where(e => e.Id == AID).Include(e => e.TripType)
                                    .Include(e => e.City).Include(e => e.Country).Include(e => e.State).SingleOrDefault();
                                DT_ForeignTrips DT_Corp = (DT_ForeignTrips)obj;
                                DT_Corp.TripType_Id = DBTrackFile.ValCompare(Old_Corp.TripType, c.TripType);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
                                DT_Corp.Country_Id = DBTrackFile.ValCompare(Old_Corp.Country, c.Country); //Old_Corp.BusinessType == c.BusinessType ? 0 : Old_Corp.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_Corp.BusinessType.Id;
                                DT_Corp.City_Id = DBTrackFile.ValCompare(Old_Corp.City, c.City); //Old_Corp.ContactDetails == c.ContactDetails ? 0 : Old_Corp.ContactDetails == null && c.ContactDetails != null ? c.ContactDetails.Id : Old_Corp.ContactDetails.Id;
                                db.Create(DT_Corp);
                                //db.SaveChanges();
                            }
                            blog.DBTrack = c.DBTrack;
                            db.ForeignTrips.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            //return Json(new Object[] { blog.Id, c.Purpose, "Record Updated", JsonRequestBehavior.AllowGet });
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = c.Purpose, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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
        public int EditS(string Country, string State, string City, string triptype, int AID, ForeignTrips addrs, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                IList<ForeignTrips> typedetails = null;
                if (Country != null && Country != "")
                {
                    var val = db.Country.Find(int.Parse(Country));
                    addrs.Country = val;

                    var type = db.ForeignTrips.Include(e => e.Country).Where(e => e.Id == AID).SingleOrDefault();
                    if (type.Country != null)
                    {
                        typedetails = db.ForeignTrips.Where(x => x.Country.Id == type.Country.Id && x.Id == AID).ToList();
                    }
                    else
                    {
                        typedetails = db.ForeignTrips.Where(x => x.Id == AID).ToList();
                    }

                    foreach (var s in typedetails)
                    {
                        s.Country = addrs.Country;
                        db.ForeignTrips.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }
                else
                {
                    var BusiTypeDetails = db.ForeignTrips.Include(e => e.Country).Where(x => x.Id == AID).ToList();
                    foreach (var s in BusiTypeDetails)
                    {
                        s.Country = null;
                        db.ForeignTrips.Attach(s);
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

                    var type = db.ForeignTrips.Include(e => e.Country).Where(e => e.Id == AID).SingleOrDefault();
                    if (type.State != null)
                    {
                        typedetails = db.ForeignTrips.Where(x => x.State.Id == type.State.Id && x.Id == AID).ToList();
                    }
                    else
                    {
                        typedetails = db.ForeignTrips.Where(x => x.Id == AID).ToList();
                    }

                    foreach (var s in typedetails)
                    {
                        s.State = addrs.State;
                        db.ForeignTrips.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }
                else
                {
                    var BusiTypeDetails = db.ForeignTrips.Include(e => e.State).Where(x => x.Id == AID).ToList();
                    foreach (var s in BusiTypeDetails)
                    {
                        s.State = null;
                        db.ForeignTrips.Attach(s);
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

                    var type = db.ForeignTrips.Include(e => e.City).Where(e => e.Id == AID).SingleOrDefault();
                    if (type.City != null)
                    {
                        typedetails = db.ForeignTrips.Where(x => x.City.Id == type.City.Id && x.Id == AID).ToList();
                    }
                    else
                    {
                        typedetails = db.ForeignTrips.Where(x => x.Id == AID).ToList();
                    }

                    foreach (var s in typedetails)
                    {
                        s.City = addrs.City;
                        db.ForeignTrips.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }
                else
                {
                    var BusiTypeDetails = db.ForeignTrips.Include(e => e.City).Where(x => x.Id == AID).ToList();
                    foreach (var s in BusiTypeDetails)
                    {
                        s.City = null;
                        db.ForeignTrips.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }

                if (triptype != null)
                {
                    if (triptype != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(triptype));
                        addrs.TripType = val;

                        var type = db.ForeignTrips.Include(e => e.TripType).Where(e => e.Id == AID).SingleOrDefault();
                        // IList<ForeignTrips> typedetails = null;
                        if (type.TripType != null)
                        {
                            typedetails = db.ForeignTrips.Where(x => x.TripType.Id == type.TripType.Id && x.Id == AID).ToList();
                        }
                        else
                        {
                            typedetails = db.ForeignTrips.Where(x => x.Id == AID).ToList();
                        }
                        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                        foreach (var s in typedetails)
                        {
                            s.TripType = addrs.TripType;
                            db.ForeignTrips.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                    else
                    {
                        var BusiTypeDetails = db.ForeignTrips.Include(e => e.TripType).Where(x => x.Id == AID).ToList();
                        foreach (var s in BusiTypeDetails)
                        {
                            s.TripType = null;
                            db.ForeignTrips.Attach(s);
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
                    var BusiTypeDetails = db.ForeignTrips.Include(e => e.TripType).Where(x => x.Id == AID).ToList();
                    foreach (var s in BusiTypeDetails)
                    {
                        s.TripType = null;
                        db.ForeignTrips.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }


                var CurAdrs = db.ForeignTrips.Find(AID);
                TempData["CurrRowVersion"] = CurAdrs.RowVersion;
                db.Entry(CurAdrs).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    addrs.DBTrack = dbT;
                    ForeignTrips add = new ForeignTrips()
                    {
                        FromDate = addrs.FromDate,

                        ToDate = addrs.ToDate,
                        Purpose = addrs.Purpose,
                        //  Landmark = addrs.Landmark,
                        DBTrack = addrs.DBTrack,
                        Id = AID
                    };

                    // Session["FullAddress"] = add.FullAddress;
                    db.ForeignTrips.Attach(add);
                    db.Entry(add).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(add).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                    return 1;
                }
                return 0;
            }
        }
        public async Task<ActionResult> Delete(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    var Q = db.Employee.Include(e => e.ForeignTrips)
                   .Include(e => e.ForeignTrips.Select(a => a.Country))
                   .Include(e => e.ForeignTrips.Select(a => a.State))
                   .Include(e => e.ForeignTrips.Select(a => a.City))
                   .Include(e => e.ForeignTrips.Select(a => a.TripType))
                   .Where(e => e.Id == data).FirstOrDefault();

                    int AID = Q.ForeignTrips.Select(a => a.Id).FirstOrDefault();
                    ForeignTrips corporates = db.ForeignTrips.Include(e => e.Country)
                                                       .Include(e => e.City).Include(e => e.State)
                                                       .Include(e => e.TripType).Where(e => e.Id == AID).SingleOrDefault();

                    Country add = corporates.Country;
                    City conDet = corporates.City;
                    State conDet1 = corporates.State;
                    LookupValue val = corporates.TripType;
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
                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack);
                            DT_ForeignTrips DT_Corp = (DT_ForeignTrips)rtn_Obj;
                            DT_Corp.Country_Id = corporates.Country == null ? 0 : corporates.Country.Id;
                            DT_Corp.City_Id = corporates.City == null ? 0 : corporates.City.Id;
                            DT_Corp.TripType_Id = corporates.TripType == null ? 0 : corporates.TripType.Id;
                            db.Create(DT_Corp);
                            // db.SaveChanges();
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack);
                            await db.SaveChangesAsync();
                            //using (var context = new DataBaseContext())
                            //{
                            //   // DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack );
                            //}
                            ts.Complete();
                            // Msg.Add("  Data removed successfully.  ");
                            //   return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }
                    }
                    else
                    {
                        //var selectedRegions = corporates.Regions;

                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //if (selectedRegions != null)
                            //{
                            //    var corpRegion = new HashSet<int>(corporates.Regions.Select(e => e.Id));
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
                                var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dbT);
                                DT_ForeignTrips DT_Corp = (DT_ForeignTrips)rtn_Obj;
                                DT_Corp.TripType_Id = val == null ? 0 : val.Id;
                                DT_Corp.Country_Id = add == null ? 0 : add.Id;
                                DT_Corp.City_Id = conDet == null ? 0 : conDet.Id;
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
                                // Msg.Add("  Data removed successfully.  ");
                                //   return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                Msg.Add("  Data removed successfully.  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }
                            catch (RetryLimitExceededException /* dex */)
                            {
                                //Log the error (uncomment dex variable name and add a line here to write a log.)
                                //ModelState.AddModelError("", "Unable to delete. Try again, and if the problem persists, see your system administrator.");
                                //return RedirectToAction("Delete");
                                // return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
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
        public async Task<ActionResult> AuthSave(int auth_id, Boolean isauth, String auth_action) //to save Authorized data
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

                            ForeignTrips corp = db.ForeignTrips.Include(e => e.City)
                                .Include(e => e.Country).Include(e => e.TripType)
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

                            db.ForeignTrips.Attach(corp);
                            db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corp.DBTrack);
                            DT_ForeignTrips DT_Corp = (DT_ForeignTrips)rtn_Obj;
                            DT_Corp.TripType_Id = corp.TripType == null ? 0 : corp.TripType.Id;
                            DT_Corp.Country_Id = corp.Country == null ? 0 : corp.Country.Id;
                            DT_Corp.City_Id = corp.City == null ? 0 : corp.City.Id;
                            db.Create(DT_Corp);
                            await db.SaveChangesAsync();
                            ts.Complete();
                            //  return Json(new Object[] { corp.Id, "Record Authorized", JsonRequestBehavior.AllowGet });
                            Msg.Add("  Record Authorized");
                            return Json(new Utility.JsonReturnClass { Id = corp.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }
                    }
                    else if (auth_action == "M")
                    {

                        ForeignTrips Old_Corp = db.ForeignTrips.Include(e => e.City)
                                .Include(e => e.Country).Include(e => e.TripType).Where(e => e.Id == auth_id).SingleOrDefault();


                        DT_ForeignTrips Curr_Corp = db.DT_ForeignTrips
                                                    .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                                    .OrderByDescending(e => e.Id)
                                                    .FirstOrDefault();

                        if (Curr_Corp != null)
                        {
                            ForeignTrips corp = new ForeignTrips();

                            string TripType = Curr_Corp.TripType_Id == null ? null : Curr_Corp.TripType_Id.ToString();
                            string city = Curr_Corp.City_Id == null ? null : Curr_Corp.City_Id.ToString();
                            string countery = Curr_Corp.Country_Id == null ? null : Curr_Corp.Country_Id.ToString();
                            corp.FromDate = Curr_Corp.FromDate == null ? Old_Corp.FromDate : Curr_Corp.FromDate;
                            corp.ToDate = Curr_Corp.ToDate == null ? Old_Corp.ToDate : Curr_Corp.ToDate;
                            //      corp.Id = auth_id;

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

                                        //                     int a = EditS(TripType, city, countery, auth_id, corp, corp.DBTrack);
                                        await db.SaveChangesAsync();
                                        ts.Complete();
                                        //   return Json(new Object[] { corp.Id, "Record Authorized", JsonRequestBehavior.AllowGet });
                                        Msg.Add("  Record Authorized");
                                        return Json(new Utility.JsonReturnClass { Id = corp.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    }
                                }
                                catch (DbUpdateConcurrencyException ex)
                                {
                                    var entry = ex.Entries.Single();
                                    var clientValues = (ForeignTrips)entry.Entity;
                                    var databaseEntry = entry.GetDatabaseValues();
                                    if (databaseEntry == null)
                                    {
                                        // return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                        Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    }
                                    else
                                    {
                                        var databaseValues = (ForeignTrips)databaseEntry.ToObject();
                                        corp.RowVersion = databaseValues.RowVersion;
                                    }
                                }
                                Msg.Add("Record modified by another user.So refresh it and try to save again.");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                //   return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                            }
                        }
                        else
                            Msg.Add("  Data removed successfully.  ");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        //   return Json(new Object[] { "", "Data removed from history", JsonRequestBehavior.AllowGet });
                    }
                    else if (auth_action == "D")
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //ForeignTrips corp = db.ForeignTrips.Find(auth_id);
                            ForeignTrips corp = db.ForeignTrips.AsNoTracking().Include(e => e.TripType)
                                                                        .Include(e => e.City)
                                                                        .Include(e => e.State)
                                                                        .Include(e => e.Country)
                                                                        .FirstOrDefault(e => e.Id == auth_id);

                            Country country = corp.Country;
                            City city = corp.City;
                            State state = corp.State;
                            LookupValue val = corp.TripType;

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

                            db.ForeignTrips.Attach(corp);
                            db.Entry(corp).State = System.Data.Entity.EntityState.Deleted;


                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corp.DBTrack);
                            DT_ForeignTrips DT_Corp = (DT_ForeignTrips)rtn_Obj;
                            DT_Corp.Country_Id = corp.Country == null ? 0 : corp.Country.Id;
                            DT_Corp.City_Id = corp.City == null ? 0 : corp.City.Id;
                            DT_Corp.TripType_Id = corp.TripType == null ? 0 : corp.TripType.Id;
                            db.Create(DT_Corp);
                            await db.SaveChangesAsync();
                            db.Entry(corp).State = System.Data.Entity.EntityState.Detached;
                            ts.Complete();
                            //  return Json(new Object[] { "", "Record Authorized", JsonRequestBehavior.AllowGet });
                            Msg.Add(" Record Authorized ");
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
    }
}