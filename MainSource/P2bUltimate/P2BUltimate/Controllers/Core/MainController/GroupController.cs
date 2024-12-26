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
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.Entity;
using P2BUltimate.Models;
using System.Web.Script.Serialization;
using P2BUltimate.App_Start;
using System.Threading.Tasks;
using P2BUltimate.Security;

namespace P2BUltimate.Controllers.Core.MainController
{
    [AuthoriseManger]
    public class GroupController : Controller
    {
        //
        // GET: /Group/
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
            return View("~/Views/Core/MainViews/Group/Index.cshtml");
        }
        public ActionResult CreateIncharge_partial()
        {
            return View("~/Views/Shared/Core/_Namedetails.cshtml");
        }
        public ActionResult CreateUnit_partial()
        {
            return View("~/Views/Shared/Core/_Unit.cshtml");
        }



        //private DataBaseContext db = new DataBaseContext();

        public ActionResult getemploye(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.Employee.Include(e => e.EmpName).ToList();
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

                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.FullContactDetails }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }

        }

        /* -------------------------- Incharge ----------------------*/
        public ActionResult GetLookupIncharge(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.Employee.ToList();
                IEnumerable<Employee> all;
                if (!string.IsNullOrEmpty(data))
                {
                    //   all = db.Employee.ToList().Where(d => d.NameDetails.FullNameFML.Contains(data));
                }
                else
                {

                    // var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.NameDetails.FullNameFML }).Distinct();
                    // return Json(r, JsonRequestBehavior.AllowGet);
                }
                //  var result = (from c in all
                //               select new { c.Id, c.NameDetails.FullNameFML }).Distinct();
                //return Json(result, JsonRequestBehavior.AllowGet);
                return Json(null, JsonRequestBehavior.AllowGet);
            }

        }


        ///* ----------------------------- Incharge ---------------------------*/

        //public ActionResult GetLookupIncharge(string data)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var fall = db.Group.ToList();
        //        IEnumerable<Group> all;
        //        if (!string.IsNullOrEmpty(data))
        //        {
        //            all = db.Group.ToList();
        //        }
        //        else
        //        {
        //            var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.Name}).Distinct();
        //            return Json(r, JsonRequestBehavior.AllowGet);
        //        }
        //        var result = (from c in all
        //                      select new { c.Id, c.Name }).Distinct();
        //        return Json(result, JsonRequestBehavior.AllowGet);
        //    }
        //}


        /* ----------------------------- Unit GET Details ---------------------------*/

        public ActionResult GetLookupUnit(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.Unit.ToList();
                IEnumerable<Unit> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.Unit.ToList();
                }
                else
                {
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.Name }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.Name }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        /* ----------------- Grid ------------------- */



        public class CorporateRegion
        {
            public int GId { get; set; }
            public string GName { get; set; }
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
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
        //        IEnumerable<CorporateRegion> Groups = null;

        //        List<CorporateRegion> model = new List<CorporateRegion>();

        //        var r = db.Department.Include(e => e.Groups).ToList();

        //        var view = new CorporateRegion();
        //        foreach (var i in r)
        //        {
        //            foreach (var z in i.Groups)
        //            {
        //                view = new CorporateRegion()
        //                {
        //                    GId = i.Id,
        //                    GName = i.Name,
        //                    Id = z.Id,
        //                    Code = z.Code,
        //                    Name = z.Name
        //                };

        //                model.Add(view);
        //            }
        //        }

        //        var fall = db.Group.ToList();
        //        var list1 = db.Department.ToList().SelectMany(e => e.Groups);
        //        var list2 = fall.Except(list1);

        //        foreach (var z in list2)
        //        {
        //            view = new CorporateRegion()
        //            {
        //                GId = 0,
        //                GName = "",
        //                Id = z.Id,
        //                Code = z.Code,
        //                Name = z.Name
        //            };

        //            model.Add(view);
        //        }

        //        Groups = model;

        //        IEnumerable<CorporateRegion> IE;
        //        if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
        //        {
        //            IE = Groups;
        //            if (gp.searchOper.Equals("eq"))
        //            {
        //                if (gp.searchField == "GName")
        //                    jsonData = IE.Select(a => new { a.Id, a.Code, a.Name, a.GName }).Where((e => (e.GName.ToString().Contains(gp.searchString)))).ToList();
        //                else if (gp.searchField == "Id")
        //                    jsonData = IE.Select(a => new { a.Id, a.Code, a.Name, a.GName }).Where((e => (e.Id.ToString().Contains(gp.searchString)))).ToList();
        //                else if (gp.searchField == "Code")
        //                    jsonData = IE.Select(a => new { a.Id, a.Code, a.Name, a.GName }).Where((e => (e.Code.ToString().Contains(gp.searchString)))).ToList();
        //                else if (gp.searchField == "Name")
        //                    jsonData = IE.Select(a => new { a.Id, a.Code, a.Name, a.GName }).Where((e => (e.Name.ToString().Contains(gp.searchString)))).ToList();
        //                ////jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Code, a.Name, a.GName }).ToList();
        //            }
        //            totalRecords = IE.Count();
        //        }
        //        else
        //        {
        //            IE = Groups;
        //            Func<CorporateRegion, dynamic> orderfuc;
        //            if (gp.sidx == "Id")
        //            {
        //                orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
        //            }
        //            else
        //            {
        //                orderfuc = (c => gp.sidx == "GName" ? c.GName.ToString() :
        //                                 gp.sidx == "Code" ? c.Code :
        //                                 gp.sidx == "Name" ? c.Name : "");
        //            }
        //            if (gp.sord == "asc")
        //            {
        //                IE = IE.OrderBy(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.GName), Convert.ToString(a.Code), Convert.ToString(a.Name) }).ToList();
        //            }
        //            else if (gp.sord == "desc")
        //            {
        //                IE = IE.OrderByDescending(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.GName), Convert.ToString(a.Code), Convert.ToString(a.Name) }).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.GName, a.Code, a.Name }).ToList();
        //            }
        //            totalRecords = Groups.Count();
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
                IEnumerable<Group> Location = null;
                IEnumerable<Group> IE;
                if (gp.IsAutho == true)
                {
                    Location = db.Company.Include(e => e.Group).SelectMany(e => e.Group).AsNoTracking().ToList();
                }
                else
                {

                    FilterSession.Session a = new FilterSession.Session();
                    var b = a.Check_flow();
                    if (b != null)
                    {
                        if (b.type == "master")
                        {
                            Location = db.Company.Include(e => e.Group).Where(e => e.Id == b.comp_code).SelectMany(e => e.Group).ToList();
                        }
                        else
                        {
                            Location = db.Group.ToList();
                        }
                    }
                    else
                    {
                        Location = db.Group.ToList();
                    }
                    //if (Session["object"] != null && Session["object"] != "")
                    //{
                    //    if (Session["object"].ToString() == "object")
                    //    {
                    //        Location = db.Group.ToList();
                    //    }
                    //    else if (Session["object"].ToString() == "master")
                    //    {
                    //        var id = Convert.ToInt32(Session["CompCode"]);

                    //        if (id != null && id != 0)
                    //        {
                    //            Location = db.Company.Include(e => e.Group).Where(e => e.Id == id).SelectMany(e => e.Group).ToList();
                    //        }
                    //    }
                    //}
                    //else
                    //{
                    //    Location = db.Group.ToList();
                    //}
                }

                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = Location;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.Code.ToString().Contains(gp.searchString))
                              || (e.Name.ToUpper().Contains(gp.searchString.ToUpper()))
                              || (e.Id.ToString().Contains(gp.searchString.ToUpper()))
                              ).Select(a => new Object[] { a.Code, a.Name, a.Id }).ToList();
                        //jsonData = IE.Select(a => new Object[] { Convert.ToString(a.Code), Convert.ToString(a.Name), a.Id, Convert.ToString(a.OpeningDate) }).ToList();

                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.Code), Convert.ToString(a.Name), a.Id, Convert.ToString(a.OpeningDate) }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = Location;
                    Func<Group, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "Code" ? c.Code :
                                                               gp.sidx == "Name" ? c.Name :

                                         "");
                    }
                    //Func<Group, string> orderfuc = (c =>
                    //                                           gp.sidx == "ID" ? c.Id.ToString() :
                    //                                           gp.sidx == "Code" ? c.Code :
                    //                                           gp.sidx == "Name" ? c.Name :

                    //                                            "");
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.Code), Convert.ToString(a.Name), a.Id, Convert.ToString(a.OpeningDate) }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.Code), Convert.ToString(a.Name), a.Id, Convert.ToString(a.OpeningDate) }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.Code), Convert.ToString(a.Name), a.Id, Convert.ToString(a.OpeningDate) }).ToList();
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

        /*------------------------------- Create---------------------------------------*/
        [HttpPost]
        public ActionResult Create(Group u, FormCollection form)
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
                    string disc = form["Contact_List"] == "0" ? "" : form["Contact_List"];
                    string Category = form["Typelist"] == "0" ? "" : form["Typelist"];
                    string inch = form["Incharge_List"] == "0" ? "" : form["Incharge_List"];
                    string unitlist = form["Units_List"] == "0" ? "" : form["Units_List"];

                    if (inch != null)
                    {
                        if (inch != "")
                        {
                            int InchId = Convert.ToInt32(inch);
                            var vals = db.Employee.Where(e => e.Id == InchId).SingleOrDefault();
                            u.Incharge = vals;
                        }
                    }

                    if (unitlist != null)
                    {
                        if (unitlist != "")
                        {
                            List<int> Ids = unitlist.Split(',').Select(e => int.Parse(e)).ToList();
                            foreach (var k in Ids)
                            {
                                var value = db.Unit.Find(k);
                                u.Units = new List<Unit>();
                                u.Units.Add(value);
                            }
                        }
                    }

                    if (Category != null)
                    {
                        if (Category != "")
                        {
                            var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "403").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(Category)).FirstOrDefault();  //db.LookupValue.Find(int.Parse(Category));
                            u.Type = val;
                        }
                    }

                    if (disc != null)
                    {
                        if (disc != "")
                        {
                            int DiscId = Convert.ToInt32(disc);
                            var vals = db.ContactDetails.Where(e => e.Id == DiscId).SingleOrDefault();
                            u.ContactDetails = vals;
                        }
                    }


                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            if (db.Group.Any(o => o.Code == u.Code))
                            {
                                Msg.Add("  Code Already Exists.  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // return Json(new Object[] { "", "", "Code Already Exists.", JsonRequestBehavior.AllowGet });
                            }

                            u.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName };

                            Group un = new Group()
                            {
                                Type = u.Type,
                                OpeningDate = u.OpeningDate,
                                Name = u.Name,
                                Code = u.Code,
                                ContactDetails = u.ContactDetails,
                                Incharge = u.Incharge,
                                Units = u.Units,
                                DBTrack = u.DBTrack
                            };

                            try
                            {
                                db.Group.Add(un);
                                var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, u.DBTrack);
                                DT_Group DT_Corp = (DT_Group)rtn_Obj;
                                DT_Corp.Incharge_Id = u.Incharge == null ? 0 : u.Incharge.Id;
                                DT_Corp.Type_Id = u.Type == null ? 0 : u.Type.Id;
                                DT_Corp.ContactDetails_Id = u.ContactDetails == null ? 0 : u.ContactDetails.Id;
                                db.Create(DT_Corp);
                                db.SaveChanges();

                                if (Company != null)
                                {
                                    var Objjob = new List<Group>();
                                    Objjob.Add(un);
                                    Company.Group = Objjob;
                                    db.Company.Attach(Company);
                                    db.Entry(Company).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    db.Entry(Company).State = System.Data.Entity.EntityState.Detached;

                                }
                                ts.Complete();
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                // return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });

                                //db.Group.Add(un);
                                //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, u.DBTrack);
                                //db.SaveChanges();
                                //ts.Complete();
                                //return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = u.Id });
                            }
                            catch (DataException)
                            {
                                Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //ModelState.AddModelError(string.Empty, "Unable to create. Try again, and if the problem persists contact your system administrator.");
                                //return View(u);
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

        /******************************* Edit ******************************/
        public class GroupUnits
        {
            public string Cont_id { get; set; }
            public string Contact_Name { get; set; }
            public string Incharge_Id { get; set; }
            public string Incharge_Name { get; set; }
            public Array unitid { get; set; }
            public Array unitname { get; set; }
        }


        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.Group
                    .Include(e => e.ContactDetails)
                    .Include(e => e.Incharge)
                    .Include(e => e.Type)
                    .Where(e => e.Id == data).Select(e => new
                    {
                        Name = e.Name,
                        Code = e.Code,
                        Type_ID = e.Type.Id == null ? 0 : e.Type.Id,
                        OpeningDate = e.OpeningDate == null ? null : e.OpeningDate,
                        Action = e.DBTrack.Action
                    }).ToList();


                //var list_data = db.Group.Include(e => e.Incharge).Include(e => e.ContactDetails).ToList();
                //List<GroupUnits> _GroupUnits = new List<GroupUnits>();
                //foreach (var ca in list_data)
                //{
                //    _GroupUnits.Add(new GroupUnits
                //    {

                //        unitid = ca.Units.Select(e => e.Id.ToString()).ToArray(),
                //        unitname = ca.Units.Select(e => e.Name).ToArray()
                //    });
                //}
                var add_data = db.Group
              .Include(e => e.ContactDetails)
              .Include(e => e.Incharge)
                .Include(e => e.Type)
                .Where(e => e.Id == data)
                .Select(e => new
                {
                    Cont_Id = e.ContactDetails.Id == null ? "" : e.ContactDetails.Id.ToString(),
                    FullContactDetails = e.ContactDetails.FullContactDetails == null ? "" : e.ContactDetails.FullContactDetails,
                    Incharge_Id = e.Incharge.Id == null ? "" : e.Incharge.Id.ToString(),
                    //  InchargetDetails = e.Incharge.EmpCode
                    InchargetDetails = e.Incharge.FullDetails == null ? "" : e.Incharge.FullDetails,
                }).ToList();
                //var a = (from ca in list_data
                //         select new
                //         {
                //             Cont_id = ca.ContactDetails == null ? null : ca.ContactDetails.Id.ToString(),
                //             Contact_Name = ca.ContactDetails == null ? null : ca.ContactDetails.FullContactDetails,
                //             Incharge_Id = ca.Incharge == null ? null : ca.Incharge.Id.ToString(),
                //             Incharge_Name = ca.Incharge == null ? null : ca.Incharge.CardCode,
                //         }).FirstOrDefault();
                //_GroupUnits.Add(new GroupUnits
                //{
                //    Cont_id = a.Cont_id,
                //    Contact_Name = a.Contact_Name,
                //    Incharge_Id = a.Incharge_Id,
                //    Incharge_Name = a.Incharge_Name,
                //});
                var W = db.DT_Group
                     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id,
                         Code = e.Code == null ? "" : e.Code,
                         Name = e.Name == null ? "" : e.Name,
                         Type_Val = e.Type_Id == 0 ? "" : db.LookupValue.Where(x => x.Id == e.Type_Id).Select(x => x.LookupVal).FirstOrDefault(),
                         Contact_Val = e.ContactDetails_Id == 0 ? "" : db.ContactDetails.Where(x => x.Id == e.ContactDetails_Id).Select(x => x.FullContactDetails).FirstOrDefault(),
                         Incharge_val = e.Incharge_Id == 0 ? "" : db.Employee.Where(x => x.Id == e.Incharge_Id).Select(x => x.CardCode).FirstOrDefault(),
                         Unit_val = e.Units_Id == 0 ? "" : db.Unit.Where(x => x.Id == e.Units_Id).Select(x => x.FullDetails).FirstOrDefault()
                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var Corp = db.Group.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, add_data, W, Auth, JsonRequestBehavior.AllowGet });
            }
        }


        /*-------------------------------------- Edit save --------------------------------------------*/

        //[HttpPost]
        //public async Task<ActionResult> EditSave(Group g, int data, FormCollection form)
        //{

        //    string ContactDetails = form["Contact_List"] == "0" ? "" : form["Contact_List"];
        //    string Category = form["Category"] == "0" ? "" : form["Category"];
        //    string inch = form["Incharge_List"] == "0" ? "" : form["Incharge_List"];
        //    string unit = form["Units_List"] == "0" ? "" : form["Units_List"];
        //    bool Auth = form["Autho_Allow"] == "true" ? true : false;


        //    if (Category != null)
        //    {
        //        if (Category != "")
        //        {
        //            var val = db.LookupValue.Find(int.Parse(Category));
        //            g.Type = val;
        //        }
        //    }

        //    if (inch != null)
        //    {
        //        if (inch != "")
        //        {
        //            int AddId = Convert.ToInt32(inch);
        //            var val = db.Employee.Include(e => e.EmpCode)
        //                                .Where(e => e.Id == AddId).SingleOrDefault();
        //            g.Incharge = val;
        //        }
        //    }

        //    if (ContactDetails != null)
        //    {
        //        if (ContactDetails != "")
        //        {
        //            int ContId = Convert.ToInt32(ContactDetails);
        //            var val = db.ContactDetails.Include(e => e.ContactNumbers)
        //                                .Where(e => e.Id == ContId).SingleOrDefault();
        //            g.ContactDetails = val;
        //        }
        //    }


        //    if (unit != null)
        //    {
        //        if (unit != "")
        //        {
        //            List<int> Ids = unit.Split(',').Select(e => int.Parse(e)).ToList();
        //            foreach (var k in Ids)
        //            {
        //                var value = db.Unit.Find(k);
        //                g.Units = new List<Unit>();
        //                g.Units.Add(value);
        //            }
        //        }
        //    }

        //    if (Auth == false)
        //    {
        //        if (ModelState.IsValid)
        //        {
        //            try
        //            {
        //                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                {
        //                    Group blog = null; // to retrieve old data
        //                    DbPropertyValues originalBlogValues = null;

        //                    using (var context = new DataBaseContext())
        //                    {
        //                        blog = context.Group.Where(e => e.Id == data).Include(e => e.Type)
        //                                                .Include(e => e.Incharge)
        //                                                .Include(e => e.ContactDetails).SingleOrDefault();
        //                        originalBlogValues = context.Entry(blog).OriginalValues;
        //                    }

        //                    g.DBTrack = new DBTrack
        //                    {
        //                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                        Action = "M",
        //                        ModifiedBy = SessionManager.UserName,
        //                        ModifiedOn = DateTime.Now
        //                    };

        //                    int a = EditS(Category, inch, ContactDetails, unit, data, g, g.DBTrack);

        //                    using (var context = new DataBaseContext())
        //                    {
        //                        var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, g.DBTrack);
        //                        DT_Group DT_Corp = (DT_Group)obj;
        //                        DT_Corp.Incharge_Id = blog.Incharge == null ? 0 : blog.Incharge.Id;
        //                        DT_Corp.Type_Id = blog.Type == null ? 0 : blog.Type.Id;
        //                        DT_Corp.ContactDetails_Id = blog.ContactDetails == null ? 0 : blog.ContactDetails.Id;
        //                        //                      DT_Corp.Units_Id = blog.Units == null ? 0 : blog.Units.Id;
        //                        db.Create(DT_Corp);
        //                        db.SaveChanges();
        //                    }
        //                    await db.SaveChangesAsync();
        //                    ts.Complete();
        //                    return Json(new Object[] { g.Id, g.Name, "Record Updated", JsonRequestBehavior.AllowGet });
        //                }
        //            }
        //            catch (DbUpdateConcurrencyException ex)
        //            {
        //                var entry = ex.Entries.Single();
        //                var clientValues = (Group)entry.Entity;
        //                var databaseEntry = entry.GetDatabaseValues();
        //                if (databaseEntry == null)
        //                {
        //                    return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
        //                }
        //                else
        //                {
        //                    var databaseValues = (Group)databaseEntry.ToObject();
        //                    g.RowVersion = databaseValues.RowVersion;

        //                }
        //            }

        //            return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
        //        }
        //    }
        //    else
        //    {
        //        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //        {

        //            Group blog = null; // to retrieve old data
        //            DbPropertyValues originalBlogValues = null;
        //            Group Old_Corp = null;

        //            using (var context = new DataBaseContext())
        //            {
        //                blog = context.Group.Where(e => e.Id == data).SingleOrDefault();
        //                originalBlogValues = context.Entry(blog).OriginalValues;
        //            }
        //            g.DBTrack = new DBTrack
        //            {
        //                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                Action = "M",
        //                IsModified = blog.DBTrack.IsModified == true ? true : false,
        //                ModifiedBy = SessionManager.UserName,
        //                ModifiedOn = DateTime.Now
        //            };
        //            Group corp = new Group()
        //            {
        //                Code = g.Code,
        //                Name = g.Name,
        //                Id = data,
        //                DBTrack = g.DBTrack,
        //                RowVersion = (Byte[])TempData["RowVersion"]
        //            };

        //            using (var context = new DataBaseContext())
        //            {
        //                var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, corp, "Group", g.DBTrack);

        //                Old_Corp = context.Group.Where(e => e.Id == data).Include(e => e.Type)
        //                    .Include(e => e.Incharge).Include(e => e.ContactDetails).Include(e => e.Units).SingleOrDefault();
        //                DT_Group DT_Corp = (DT_Group)obj;
        //                DT_Corp.Incharge_Id = DBTrackFile.ValCompare(Old_Corp.Incharge, g.Incharge);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
        //                DT_Corp.Type_Id = DBTrackFile.ValCompare(Old_Corp.Type, g.Type); //Old_Corp.Type == c.Type ? 0 : Old_Corp.Type == null && c.Type != null ? c.Type.Id : Old_Corp.Type.Id;
        //                DT_Corp.ContactDetails_Id = DBTrackFile.ValCompare(Old_Corp.ContactDetails, g.ContactDetails); //Old_Corp.ContactDetails == c.ContactDetails ? 0 : Old_Corp.ContactDetails == null && c.ContactDetails != null ? c.ContactDetails.Id : Old_Corp.ContactDetails.Id;
        //                DT_Corp.Units_Id = DBTrackFile.ValCompare(Old_Corp.Units, g.Units);
        //                db.Create(DT_Corp);
        //            }
        //            blog.DBTrack = g.DBTrack;
        //            db.Group.Attach(blog);
        //            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
        //            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //            db.SaveChanges();
        //            ts.Complete();
        //            return Json(new Object[] { blog.Id, g.Name, "Record Updated", JsonRequestBehavior.AllowGet });
        //        }

        //    }
        //    return View();

        //}


        [HttpPost]
        public async Task<ActionResult> EditSave(Group c, int data, FormCollection form) // Edit submit
        {

            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string Typelist = form["Typelist"] == "0" ? "" : form["Typelist"];
                    string inch = form["Incharge_List"] == "0" ? "" : form["Incharge_List"];
                    string ContactDetails = form["Contact_List"] == "0" ? "" : form["Contact_List"];
                    string Code = form["Code"] == "0" ? "" : form["Code"];
                    string Name = form["Name"] == "0" ? "" : form["Name"];
                    string OpeningDate = form["OpeningDate"] == "0" ? "" : form["OpeningDate"];
                    //  bool Auth = form["Autho_Action"] == "" ? false : true;
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;

                    if (Typelist != null)
                    {
                        if (Typelist != "")
                        {
                            var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "403").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(Typelist)).FirstOrDefault(); //db.LookupValue.Find(int.Parse(Typelist));
                            c.Type = val;
                            c.Type_Id = int.Parse(Typelist);
                        }
                    }

                    if (inch != null)
                    {
                        if (inch != "")
                        {
                            int InchId = Convert.ToInt32(inch);
                            var vals = db.Employee.Where(e => e.Id == InchId).SingleOrDefault();
                            c.Incharge = vals;
                            c.Incharge_Id = Convert.ToInt32(inch);
                        }
                    }
                    if (Code != null && Code != "")
                    {

                        var code = Code;
                        c.Code = code;

                    }
                    if (Name != null && Name != "")
                    {

                        var val = Name;
                        c.Name = val;

                    }
                    if (OpeningDate != null && OpeningDate != "")
                    {

                        var val = DateTime.Parse(OpeningDate);
                        c.OpeningDate = val;

                    }
                    if (ContactDetails != null)
                    {
                        if (ContactDetails != "")
                        {
                            int ContId = Convert.ToInt32(ContactDetails);
                            var val = db.ContactDetails.Include(e => e.ContactNumbers)
                                                .Where(e => e.Id == ContId).SingleOrDefault();
                            c.ContactDetails = val;
                            c.ContactDetails_Id = Convert.ToInt32(ContactDetails); ;
                        }
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
                                    Group blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {

                                        blog = context.Group.Include(e => e.Type)
                                                                .Include(e => e.ContactDetails).Where(e => e.Id == data).SingleOrDefault();
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

                                    //  int a = EditS(Typelist, inch, ContactDetails, data, c, c.DBTrack);

                                    if (Typelist != null)
                                    {
                                        if (Typelist != "")
                                        {
                                            var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "403").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(Typelist)).FirstOrDefault();// db.LookupValue.Find(int.Parse(Typelist));
                                            c.Type = val;
                                            var type = db.Group.Include(e => e.Type).Where(e => e.Id == data).SingleOrDefault();
                                            IList<Group> typedetails = null;
                                            if (type.Type != null)
                                            {
                                                typedetails = db.Group.Where(x => x.Type.Id == type.Type.Id && x.Id == data).ToList();
                                            }
                                            else
                                            {
                                                typedetails = db.Group.Where(x => x.Id == data).ToList();
                                            }
                                            foreach (var s in typedetails)
                                            {
                                                s.Type = c.Type;
                                                db.Group.Attach(s);
                                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                                db.SaveChanges();
                                                TempData["RowVersion"] = s.RowVersion;
                                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                            }
                                        }
                                        else
                                        {
                                            var BusiTypeDetails = db.Group.Include(e => e.Type).Where(x => x.Id == data).ToList();
                                            foreach (var s in BusiTypeDetails)
                                            {
                                                s.Type = null;
                                                db.Group.Attach(s);
                                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                                db.SaveChanges();
                                                TempData["RowVersion"] = s.RowVersion;
                                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var TypeDetails = db.Group.Include(e => e.Type).Where(x => x.Id == data).ToList();
                                        foreach (var s in TypeDetails)
                                        {
                                            s.Type = null;
                                            db.Group.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }

                                    if (inch != null)
                                    {
                                        if (inch != "")
                                        {

                                            var add = db.Group.Include(e => e.Incharge).Where(e => e.Id == data).SingleOrDefault();
                                            IList<Group> addressdetails = null;
                                            if (add.Incharge != null)
                                            {
                                                addressdetails = db.Group.Where(x => x.Incharge.Id == add.Incharge.Id && x.Id == data).ToList();
                                            }
                                            else
                                            {
                                                addressdetails = db.Group.Where(x => x.Id == data).ToList();
                                            }
                                            if (addressdetails != null)
                                            {
                                                foreach (var s in addressdetails)
                                                {
                                                    s.Incharge = c.Incharge;
                                                    db.Group.Attach(s);
                                                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                                    db.SaveChanges();
                                                    TempData["RowVersion"] = s.RowVersion;
                                                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var addressdetails = db.Group.Include(e => e.Incharge).Where(x => x.Id == data).ToList();
                                        // var addressdetails = db.Group.Where(x => x.Id == data).ToList();
                                        foreach (var s in addressdetails)
                                        {
                                            s.Incharge = null;
                                            db.Group.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
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

                                            var add = db.Group.Include(e => e.ContactDetails).Where(e => e.Id == data).SingleOrDefault();
                                            IList<Group> contactsdetails = null;
                                            if (add.ContactDetails != null)
                                            {
                                                contactsdetails = db.Group.Where(x => x.ContactDetails.Id == add.ContactDetails.Id && x.Id == data).ToList();
                                            }
                                            else
                                            {
                                                contactsdetails = db.Group.Where(x => x.Id == data).ToList();
                                            }
                                            foreach (var s in contactsdetails)
                                            {
                                                s.ContactDetails = c.ContactDetails;
                                                db.Group.Attach(s);
                                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                                db.SaveChanges();
                                                TempData["RowVersion"] = s.RowVersion;
                                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var contactsdetails = db.Group.Include(e => e.ContactDetails).Where(x => x.Id == data).ToList();
                                        foreach (var s in contactsdetails)
                                        {
                                            s.ContactDetails = null;
                                            db.Group.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }

                                    var CurCorp = db.Group.Find(data);
                                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {
                                        c.DBTrack = c.DBTrack;
                                        Group corp = new Group()
                                        {
                                            Type_Id = c.Type_Id,
                                            Code = c.Code,
                                            Name = c.Name,
                                            OpeningDate = c.OpeningDate,
                                            Incharge_Id=c.Incharge_Id,
                                            ContactDetails_Id=c.ContactDetails_Id,
                                            Id = data,
                                            DBTrack = c.DBTrack
                                        };

                                        db.Group.Attach(corp);
                                        db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                                        db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];

                                    }

                                    using (var context = new DataBaseContext())
                                    {



                                        var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                        DT_Group DT_Corp = (DT_Group)obj;

                                        DT_Corp.Type_Id = blog.Type == null ? 0 : blog.Type.Id;
                                        DT_Corp.ContactDetails_Id = blog.ContactDetails == null ? 0 : blog.ContactDetails.Id;
                                        DT_Corp.Incharge_Id = blog.Incharge == null ? 0 : blog.Incharge.Id;
                                        db.Create(DT_Corp);
                                        db.SaveChanges();
                                    }
                                    await db.SaveChangesAsync();
                                    ts.Complete();

                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = c.Id, Val = c.Name, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new Object[] { c.Id, c.Name, "Record Updated", JsonRequestBehavior.AllowGet });

                                }

                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (Group)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (Group)databaseEntry.ToObject();
                                    c.RowVersion = databaseValues.RowVersion;

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

                            Group blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            Group Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.Group.Where(e => e.Id == data).SingleOrDefault();
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

                            Group corp = new Group()
                            {
                                Code = c.Code,
                                Name = c.Name,
                                OpeningDate = c.OpeningDate,
                                Id = data,
                                DBTrack = c.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };


                            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, corp, "Group", c.DBTrack);
                                // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                Old_Corp = context.Group.Where(e => e.Id == data).Include(e => e.Type)
                                    .Include(e => e.Incharge).Include(e => e.ContactDetails).SingleOrDefault();
                                DT_Group DT_Corp = (DT_Group)obj;

                                DT_Corp.Type_Id = DBTrackFile.ValCompare(Old_Corp.Type, c.Type); //Old_Corp.BusinessType == c.BusinessType ? 0 : Old_Corp.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_Corp.BusinessType.Id;
                                DT_Corp.ContactDetails_Id = DBTrackFile.ValCompare(Old_Corp.ContactDetails, c.ContactDetails); //Old_Corp.ContactDetails == c.ContactDetails ? 0 : Old_Corp.ContactDetails == null && c.ContactDetails != null ? c.ContactDetails.Id : Old_Corp.ContactDetails.Id;
                                DT_Corp.Incharge_Id = DBTrackFile.ValCompare(Old_Corp.Incharge, c.Incharge); //Old_Corp.ContactDetails == c.ContactDetails ? 0 : Old_Corp.ContactDetails == null && c.ContactDetails != null ? c.ContactDetails.Id : Old_Corp.ContactDetails.Id;
                                db.Create(DT_Corp);
                                //db.SaveChanges();
                            }
                            blog.DBTrack = c.DBTrack;
                            db.Group.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = c.Name, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { blog.Id, c.Name, "Record Updated", JsonRequestBehavior.AllowGet });
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



        /*-------------------- edit Units------------------------*/

        public ActionResult EditUnitsdetails_partial(int data)
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



        /*---------------------------------------- Delete ------------------------------------- */
        //[HttpPost]
        ////[ValidateAntiForgeryToken]
        //public ActionResult Delete(int? data)
        //{
        //    Group group = db.Group.Find(data);
        //    try
        //    {
        //        using (TransactionScope ts = new TransactionScope())
        //        {
        //            db.Entry(group).State = System.Data.Entity.EntityState.Deleted;
        //            db.SaveChanges();
        //            ts.Complete();
        //        }

        //        return this.Json(new { msg = "Data deleted." });
        //    }

        //    catch (DataException /* dex */)
        //    {
        //        //Log the error (uncomment dex variable name after DataException and add a line here to write a log.
        //        return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists contact your system administrator." });
        //        //ModelState.AddModelError(string.Empty, "Unable to delete. Try again, and if the problem persists contact your system administrator.");
        //        //return RedirectToAction("Index");
        //    }
        //}


        [HttpPost]
        public ActionResult Delete(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                Group group = db.Group.Find(data);

                try
                {
                    DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };
                    db.Entry(group).State = System.Data.Entity.EntityState.Deleted;
                    DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dbT);
                    db.SaveChanges();
                    Msg.Add("  Data removed successfully.  ");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                }
                catch (DbUpdateConcurrencyException)
                {
                    return RedirectToAction("Delete", new { concurrencyError = true, id = data });
                }
                catch (RetryLimitExceededException /* dex */)
                {
                    Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    // return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
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


        public int EditS(string Corp, string inch, string ContactDetails, int data, Group c, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (Corp != null)
                {
                    if (Corp != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(Corp));
                        c.Type = val;
                        var type = db.Group.Include(e => e.Type).Where(e => e.Id == data).SingleOrDefault();
                        IList<Group> typedetails = null;
                        if (type.Type != null)
                        {
                            typedetails = db.Group.Where(x => x.Type.Id == type.Type.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            typedetails = db.Group.Where(x => x.Id == data).ToList();
                        }
                        foreach (var s in typedetails)
                        {
                            s.Type = c.Type;
                            db.Group.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                    else
                    {
                        var BusiTypeDetails = db.Group.Include(e => e.Type).Where(x => x.Id == data).ToList();
                        foreach (var s in BusiTypeDetails)
                        {
                            s.Type = null;
                            db.Group.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                }
                else
                {
                    var TypeDetails = db.Group.Include(e => e.Type).Where(x => x.Id == data).ToList();
                    foreach (var s in TypeDetails)
                    {
                        s.Type = null;
                        db.Group.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }

                if (inch != null)
                {
                    if (inch != "")
                    {

                        var add = db.Group.Include(e => e.Incharge).Where(e => e.Id == data).SingleOrDefault();
                        IList<Group> addressdetails = null;
                        if (add.Incharge != null)
                        {
                            addressdetails = db.Group.Where(x => x.Incharge.Id == add.Incharge.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            addressdetails = db.Group.Where(x => x.Id == data).ToList();
                        }
                        if (addressdetails != null)
                        {
                            foreach (var s in addressdetails)
                            {
                                s.Incharge = c.Incharge;
                                db.Group.Attach(s);
                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                TempData["RowVersion"] = s.RowVersion;
                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                            }
                        }
                    }
                }
                else
                {
                    var addressdetails = db.Group.Where(x => x.Id == data).ToList();
                    foreach (var s in addressdetails)
                    {
                        s.Incharge = null;
                        db.Group.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }

                //if (unit != null)
                //{
                //    if (unit != "")
                //    {
                //        //var val = db.Address.Find(int.Parse(inch));
                //        //c.in = val;

                //        var add = db.Group.Include(e => e.Units).Where(e => e.Id == data).SingleOrDefault();
                //        IList<Group> addressdetails = null;
                //        if (add.Units != null)
                //        {
                //            addressdetails = db.Group.Where(x => x.Units.Id == add.Units.Id && x.Id == data).ToList();
                //        }
                //        else
                //        {
                //            addressdetails = db.Group.Where(x => x.Id == data).ToList();
                //        }
                //        if (addressdetails != null)
                //        {
                //            foreach (var s in addressdetails)
                //            {
                //                s.Incharge = c.Incharge;
                //                db.Group.Attach(s);
                //                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                //                db.SaveChanges();
                //                TempData["RowVersion"] = s.RowVersion;
                //                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                //            }
                //        }
                //    }
                //}
                //else
                //{
                //    var addressdetails = db.Group.Include(e => e.Incharge).Where(x => x.Id == data).ToList();
                //    foreach (var s in addressdetails)
                //    {
                //        s.Incharge = null;
                //        db.Group.Attach(s);
                //        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                //        db.SaveChanges();
                //        TempData["RowVersion"] = s.RowVersion;
                //        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                //    }
                //}



                if (ContactDetails != null)
                {
                    if (ContactDetails != "")
                    {
                        var val = db.ContactDetails.Find(int.Parse(ContactDetails));
                        c.ContactDetails = val;

                        var add = db.Group.Include(e => e.ContactDetails).Where(e => e.Id == data).SingleOrDefault();
                        IList<Group> contactsdetails = null;
                        if (add.ContactDetails != null)
                        {
                            contactsdetails = db.Group.Where(x => x.ContactDetails.Id == add.ContactDetails.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            contactsdetails = db.Group.Where(x => x.Id == data).ToList();
                        }
                        foreach (var s in contactsdetails)
                        {
                            s.ContactDetails = c.ContactDetails;
                            db.Group.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                }
                else
                {
                    var contactsdetails = db.Group.Include(e => e.ContactDetails).Where(x => x.Id == data).ToList();
                    foreach (var s in contactsdetails)
                    {
                        s.ContactDetails = null;
                        db.Group.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }

                var CurCorp = db.Group.Find(data);
                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    c.DBTrack = dbT;
                    Group corp = new Group()
                    {
                        Code = c.Code,
                        Name = c.Name,
                        OpeningDate = c.OpeningDate,
                        Id = data,
                        DBTrack = c.DBTrack
                    };

                    db.Group.Attach(corp);
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
                            Group corp = db.Group.Include(e => e.Incharge)
                                .Include(e => e.ContactDetails).Include(e => e.Units)
                                .Include(e => e.Type).FirstOrDefault(e => e.Id == auth_id);

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

                            db.Group.Attach(corp);
                            db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            //db.SaveChanges();
                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corp.DBTrack);
                            DT_Group DT_Corp = (DT_Group)rtn_Obj;
                            DT_Corp.Incharge_Id = corp.Incharge == null ? 0 : corp.Incharge.Id;
                            //DT_Corp.Units_Id = corp.Incharge == null ? 0 : corp.Units;
                            DT_Corp.Type_Id = corp.Type == null ? 0 : corp.Type.Id;
                            DT_Corp.ContactDetails_Id = corp.ContactDetails == null ? 0 : corp.ContactDetails.Id;
                            db.Create(DT_Corp);
                            await db.SaveChangesAsync();
                            ts.Complete();
                            Msg.Add("  Record Authorised");
                            return Json(new Utility.JsonReturnClass { Id = corp.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            // return Json(new Object[] { corp.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else if (auth_action == "M")
                    {

                        Group Old_Corp = db.Group.
                                                         Include(e => e.Incharge)
                                .Include(e => e.ContactDetails).Include(e => e.Units)
                                .Include(e => e.Type).Where(e => e.Id == auth_id).SingleOrDefault();

                        //var W = db.DT_Group
                        //.Include(e => e.ContactDetails)
                        //.Include(e => e.Address)
                        //.Include(e => e.Type)
                        //.Include(e => e.ContactDetails)
                        //.Where(e => e.Orig_Id == auth_id && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                        //(e => new
                        //{
                        //    DT_Id = e.Id,
                        //    Code = e.Code == null ? "" : e.Code,
                        //    Name = e.Name == null ? "" : e.Name,
                        //    Type_Val = e.Type.Id == null ? "" : e.Type.LookupVal,
                        //    Address_Val = e.Address.Id == null ? "" : e.Address.FullAddress,
                        //    Contact_Val = e.ContactDetails.Id == null ? "" : e.ContactDetails.FullContactDetails,
                        //}).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                        DT_Group Curr_Corp = db.DT_Group
                                                    .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                                    .OrderByDescending(e => e.Id)
                                                    .FirstOrDefault();

                        if (Curr_Corp != null)
                        {
                            Group corp = new Group();

                            string Corp = Curr_Corp.Type_Id == null ? null : Curr_Corp.Type_Id.ToString();
                            string inch = Curr_Corp.Incharge_Id == null ? null : Curr_Corp.Incharge_Id.ToString();
                            string ContactDetails = Curr_Corp.ContactDetails_Id == null ? null : Curr_Corp.ContactDetails_Id.ToString();
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

                                        int a = EditS(Corp, inch, ContactDetails, auth_id, corp, corp.DBTrack);
                                        await db.SaveChangesAsync();
                                        ts.Complete();
                                        Msg.Add("  Record Authorised");
                                        return Json(new Utility.JsonReturnClass { Id = corp.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    }
                                }
                                catch (DbUpdateConcurrencyException ex)
                                {
                                    var entry = ex.Entries.Single();
                                    var clientValues = (Group)entry.Entity;
                                    var databaseEntry = entry.GetDatabaseValues();
                                    if (databaseEntry == null)
                                    {
                                        Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                    }
                                    else
                                    {
                                        var databaseValues = (Group)databaseEntry.ToObject();
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
                        //  return Json(new Object[] { "", "Data removed from history", JsonRequestBehavior.AllowGet });
                    }
                    else if (auth_action == "D")
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //Group corp = db.Group.Find(auth_id);
                            Group corp = db.Group.AsNoTracking().Include(e => e.Incharge)
                                .Include(e => e.ContactDetails).Include(e => e.Units)
                                .Include(e => e.Type).FirstOrDefault(e => e.Id == auth_id);

                            Employee add = corp.Incharge;
                            ContactDetails conDet = corp.ContactDetails;
                            LookupValue val = corp.Type;

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

                            db.Group.Attach(corp);
                            db.Entry(corp).State = System.Data.Entity.EntityState.Deleted;


                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corp.DBTrack);
                            DT_Group DT_Corp = (DT_Group)rtn_Obj;
                            DT_Corp.Incharge_Id = corp.Incharge == null ? 0 : corp.Incharge.Id;
                            DT_Corp.Type_Id = corp.Type == null ? 0 : corp.Type.Id;
                            DT_Corp.ContactDetails_Id = corp.ContactDetails == null ? 0 : corp.ContactDetails.Id;
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
    }
}