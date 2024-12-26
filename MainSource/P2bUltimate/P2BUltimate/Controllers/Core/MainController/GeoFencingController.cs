
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
using Attendance;
using System.Diagnostics;

namespace P2BUltimate.Controllers.Core.MainController
{
    public class GeoFencingController : Controller
    {
        //
        // GET: /UnitIdAssignment/
        public ActionResult Index()
        {
            return View("~/Views/Core/MainViews/GeoFencing/Index.cshtml");
        }

        public ActionResult GetLocDetails()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<int> LocIds = new List<int>();
                var geo_data = db.Location.Include(e => e.LocationObj).Select(e => new
                {
                    code = e.Id.ToString(),

                    value = "Code :" + e.LocationObj.LocCode + ",Name :" + e.LocationObj.LocDesc

                }).ToList();

                return Json(geo_data, JsonRequestBehavior.AllowGet);
            }
        }

        public class returndatagridclass
        {
            public string Id { get; set; }
            public string UnitId { get; set; }
        }

        public class UnitChildDataClass
        {
            public int Id { get; set; }
            public string Geostruct { get; set; }

        }

        public ActionResult GetDeptDetails(List<int> data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                if (data.Count != 0 && data[0] != 0)
                {
                    foreach (var s in data)
                    {
                        int id = Convert.ToInt32(s);
                        var query = db.Location.Include(e => e.Department)
                            .Include(e => e.Department.Select(x => x.GeoFencingParameter))
                           .Include(e => e.Department.Select(x => x.Address))
                           .Include(e => e.Department.Select(x => x.DepartmentObj))
                            .Where(e => e.Id == id).SingleOrDefault();
                        foreach (var ca in query.Department.Where(e => e.GeoFencingParameter == null))
                        {
                            returndata.Add(new Utility.returndataclass
                            {
                                code = ca.Id.ToString(),
                                value = "Code :" + ca.DepartmentObj.DeptCode + ",Name :" + ca.DepartmentObj.DeptDesc
                            });
                        }
                    }
                }
                //else
                //{
                //    var reference_data = db.Location.Include(e => e.Department)
                //        .Include(e => e.Department.Select(x => x.Address))
                //        .SelectMany(e => e.Department).ToList();
                //    var db_data = db.Department.Include(e => e.DepartmentObj).Include(e => e.Address).ToList();
                //    var expact_list = db_data.Except(reference_data);
                //    foreach (var s in expact_list)
                //    {
                //        returndata.Add(new Utility.returndataclass
                //        {
                //            code = s.Id.ToString(),
                //            value = s.FullDetails.ToString()
                //        });
                //    }
                //}
                return Json(returndata, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetDeptDetailsEdit(List<int> data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                if (data.Count != 0 && data[0] != 0)
                {
                    foreach (var s in data)
                    {
                        int id = Convert.ToInt32(s);
                        var query = db.Location.Include(e => e.Department)
                            .Include(e => e.Department.Select(x => x.GeoFencingParameter))
                           .Include(e => e.Department.Select(x => x.Address))
                           .Include(e => e.Department.Select(x => x.DepartmentObj))
                            .Where(e => e.Id == id).SingleOrDefault();
                        foreach (var ca in query.Department)
                        {
                            returndata.Add(new Utility.returndataclass
                            {
                                code = ca.Id.ToString(),
                                value = "Code :" + ca.DepartmentObj.DeptCode + ",Name :" + ca.DepartmentObj.DeptDesc
                            });
                        }
                    }
                }

                return Json(returndata, JsonRequestBehavior.AllowGet);
            }
        }

        public class returnEditClass
        {
            public string code { get; set; }
            public string value { get; set; }

        }

        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<returnEditClass> oreturnEditClass = new List<returnEditClass>();
                var Q = db.Location.Include(e => e.LocationObj).Include(e => e.GeoFencingParameter)
                   .Where(e => e.GeoFencingParameter.Id == data).Select
                   (e => new
                   {
                       LongitudeOriginal = e.GeoFencingParameter.LongitudeOriginal,
                       LongitudeReference = e.GeoFencingParameter.LongitudeReference,
                       LatitudeOriginal = e.GeoFencingParameter.LatitudeOriginal,
                       LatitudeReference = e.GeoFencingParameter.LatitudeReference,
                       DistanceMeter = e.GeoFencingParameter.DistanceMeter,
                       Action = e.DBTrack.Action,
                       Location = "Code :" + e.LocationObj.LocCode + ",Name :" + e.LocationObj.LocDesc,
                       LocId = e.Id,
                       ESSAttAppl = e.GeoFencingParameter.ESSAttAppl
                   }).ToList();

                var k = db.Department.Include(e => e.GeoFencingParameter)
                    .Include(e => e.DepartmentObj)
                       .Where(e => e.GeoFencingParameter.Id == data).ToList();
                foreach (var e in k)
                {
                    oreturnEditClass.Add(new returnEditClass
                    {
                        code = e.Id.ToString(),
                        value = "Code :" + e.DepartmentObj.DeptCode + ",Name :" + e.DepartmentObj.DeptDesc
                    });
                }

                TempData["RowVersion"] = db.GeoFencing.Find(data).RowVersion;
                return Json(new object[] { Q, oreturnEditClass, "", "", "" }, JsonRequestBehavior.AllowGet);
            }

        }


        [HttpPost]
        public async Task<ActionResult> EditSave(GeoFencing c, int data, FormCollection form) // Edit submit
        {
            List<String> Msg = new List<String>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;
                    string NewDept = form["department-table"] == "0" ? "" : form["department-table"];

                    if (Auth == false)
                    {
                        if (ModelState.IsValid)
                        {

                            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {
                                GeoFencing blog = null; // to retrieve old data
                                DbPropertyValues originalBlogValues = null;

                                using (var context = new DataBaseContext())
                                {
                                    blog = context.GeoFencing.Where(e => e.Id == data)
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




                                GeoFencing OGeoFence = new GeoFencing()
                                {
                                    DistanceMeter = c.DistanceMeter,
                                    LatitudeOriginal = c.LatitudeOriginal,
                                    LatitudeReference = c.LatitudeReference,
                                    LongitudeOriginal = c.LongitudeOriginal,
                                    LongitudeReference = c.LongitudeReference,
                                    DBTrack = c.DBTrack,
                                    Id = data,
                                    ESSAttAppl = c.ESSAttAppl
                                };


                                db.GeoFencing.Attach(OGeoFence);
                                db.Entry(OGeoFence).State = System.Data.Entity.EntityState.Modified;
                                db.Entry(OGeoFence).OriginalValues["RowVersion"] = TempData["RowVersion"];

                                db.SaveChanges();

                                List<Department> ODeptList = db.Department.Include(e => e.GeoFencingParameter).Where(e => e.GeoFencingParameter.Id == data).ToList();
                                if (ODeptList != null)
                                {
                                    foreach (var item in ODeptList)
                                    {
                                        item.GeoFencingParameter = null;
                                        db.Department.Attach(item);
                                        db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                    }

                                }

                                if (NewDept != null && NewDept != "")
                                {
                                    var id = Utility.StringIdsToListIds(NewDept);
                                    foreach (var ca in id)
                                    {
                                        Department dept_val = db.Department.Find(ca);
                                        if (dept_val != null)
                                        {
                                            dept_val.GeoFencingParameter = OGeoFence;
                                            db.GeoFencing.Attach(OGeoFence);
                                            db.Entry(OGeoFence).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();
                                            db.Entry(OGeoFence).State = System.Data.Entity.EntityState.Detached;

                                        }
                                    }
                                }

                                ts.Complete();

                                Msg.Add("Record Updated Successfully.");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                            List<string> MsgB = new List<string>();
                            MsgB.Add("Record modified by another user.So refresh it and try to save again.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = MsgB }, JsonRequestBehavior.AllowGet);

                            //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            GeoFencing blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            //Email Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.GeoFencing.Where(e => e.Id == data).SingleOrDefault();
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


                            GeoFencing corp = new GeoFencing()
                            {
                                DistanceMeter = c.DistanceMeter,
                                LatitudeOriginal = c.LatitudeOriginal,
                                LatitudeReference = c.LatitudeReference,
                                LongitudeOriginal = c.LongitudeOriginal,
                                LongitudeReference = c.LongitudeReference,
                                DBTrack = c.DBTrack,
                                Id = data,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };


                            blog.DBTrack = c.DBTrack;
                            db.GeoFencing.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("Record Updated Successfully.");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }

                    }
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    var entry = ex.Entries.Single();
                    var clientValues = (Corporate)entry.Entity;
                    var databaseEntry = entry.GetDatabaseValues();
                    if (databaseEntry == null)
                    {
                        Msg.Add(ex.Message);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //  return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                    }
                    else
                    {
                        var databaseValues = (Corporate)databaseEntry.ToObject();
                        c.RowVersion = databaseValues.RowVersion;

                    }
                }
                catch (Exception e)
                {
                    Msg.Add(e.Message);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
                return View();
            }

        }

        public class GeoFencingParameter
        {
            public int Id { get; set; }
            public string LocationFullDetails { get; set; }
            public string DepartmentFullDetails { get; set; }
            public string LatitudeOriginal { get; set; }
            public string LatitudeReference { get; set; }
            public string LongitudeOriginal { get; set; }
            public string LongitudeReference { get; set; }
            public double DistanceMeter { get; set; }
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
                IEnumerable<Location> Location = null;
                IEnumerable<Department> Department = null;
                List<GeoFencingParameter> GeoFencingParameter = new List<GeoFencingParameter>();
                if (gp.IsAutho == true)
                {
                    Location = db.Location.Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                    Department = db.Department.Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    Location = db.Location.Include(e => e.GeoFencingParameter).Include(e => e.LocationObj).Where(e => e.GeoFencingParameter != null).AsNoTracking().ToList();
                    Department = db.Department.Include(e => e.GeoFencingParameter).Include(e => e.DepartmentObj).Where(e => e.GeoFencingParameter != null).AsNoTracking().ToList();
                }

                if (Location != null)
                {
                    foreach (var item1 in Location)
                    {
                        GeoFencingParameter.Add(new GeoFencingParameter
                        {
                            Id = item1.GeoFencingParameter.Id,
                            LocationFullDetails = item1.FullDetails,
                            DepartmentFullDetails = "",
                            LatitudeOriginal = item1.GeoFencingParameter.LatitudeOriginal,
                            LatitudeReference = item1.GeoFencingParameter.LatitudeReference,
                            LongitudeOriginal = item1.GeoFencingParameter.LongitudeOriginal,
                            LongitudeReference = item1.GeoFencingParameter.LongitudeReference,
                            DistanceMeter = item1.GeoFencingParameter.DistanceMeter

                        });
                    }
                }

                if (Department != null)
                {
                    foreach (var item2 in Department)
                    {
                        var LocDesc = db.Location.Include(e => e.LocationObj).Include(e => e.Department).ToList();
                        foreach (var item3 in LocDesc)
                        {
                            foreach (var item4 in item3.Department.Where(e => e.Id == item2.Id))
                            {
                                GeoFencingParameter.Add(new GeoFencingParameter
                                {
                                    Id = item2.GeoFencingParameter.Id,
                                    LocationFullDetails = item3.FullDetails,
                                    DepartmentFullDetails = item2.FullDetails,
                                    LatitudeOriginal = item2.GeoFencingParameter.LatitudeOriginal,
                                    LatitudeReference = item2.GeoFencingParameter.LatitudeReference,
                                    LongitudeOriginal = item2.GeoFencingParameter.LongitudeOriginal,
                                    LongitudeReference = item2.GeoFencingParameter.LongitudeReference,
                                    DistanceMeter = item2.GeoFencingParameter.DistanceMeter
                                });
                            }
                        }
                    }
                }

                //IEnumerable<Location> IE;
                IEnumerable<GeoFencingParameter> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = GeoFencingParameter;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where((e => (e.Id.ToString().Contains(gp.searchString))
                            || (e.LocationFullDetails.ToLower().Contains(gp.searchString.ToLower()))
                            || (e.DepartmentFullDetails.ToLower().Contains(gp.searchString.ToLower()))
                            || (e.LatitudeOriginal.ToString().ToLower().Contains(gp.searchString.ToLower()))
                            || (e.LatitudeReference.ToString().ToLower().Contains(gp.searchString.ToLower()))
                            || (e.LongitudeOriginal.ToString().ToLower().Contains(gp.searchString.ToLower()))
                            || (e.LongitudeReference.ToString().ToLower().Contains(gp.searchString.ToLower()))
                            || (e.DistanceMeter.ToString().ToLower().Contains(gp.searchString.ToLower())))
                            ).Select(a => new
                            {
                                a.LocationFullDetails,
                                a.DepartmentFullDetails,
                                a.LatitudeOriginal,
                                a.LatitudeReference,
                                a.LongitudeOriginal,
                                a.LongitudeReference,
                                a.DistanceMeter,
                                a.Id
                            }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] {a.LocationFullDetails,a.DepartmentFullDetails, a.LatitudeOriginal, a.LatitudeReference,
                                                a.LongitudeOriginal, a.LongitudeReference,  
                                                a.DistanceMeter, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = GeoFencingParameter;
                    Func<GeoFencingParameter, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "LocationFullDetails" ? c.LocationFullDetails :
                                         gp.sidx == "DepartmentFullDetails" ? c.DepartmentFullDetails :
                                         gp.sidx == "LatitudeOriginal" ? c.LatitudeOriginal.ToString() :
                                         gp.sidx == "LatitudeReference" ? c.LatitudeReference.ToString() :
                                         gp.sidx == "LongitudeOriginal" ? c.LongitudeOriginal.ToString() :
                                         gp.sidx == "LongitudeReference" ? c.LongitudeReference.ToString() :
                                         gp.sidx == "DistanceMeter" ? c.DistanceMeter.ToString() : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.LocationFullDetails, a.DepartmentFullDetails, Convert.ToString(a.LatitudeOriginal), Convert.ToString(a.LatitudeReference), Convert.ToString(a.LongitudeOriginal), Convert.ToString(a.LongitudeReference), Convert.ToString(a.DistanceMeter), a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.LocationFullDetails, a.DepartmentFullDetails, Convert.ToString(a.LatitudeOriginal), Convert.ToString(a.LatitudeReference), Convert.ToString(a.LongitudeOriginal), Convert.ToString(a.LongitudeReference), Convert.ToString(a.DistanceMeter), a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.LocationFullDetails, a.DepartmentFullDetails, Convert.ToString(a.LatitudeOriginal), Convert.ToString(a.LatitudeReference), Convert.ToString(a.LongitudeOriginal), Convert.ToString(a.LongitudeReference), Convert.ToString(a.DistanceMeter), a.Id }).ToList();
                    }
                    totalRecords = GeoFencingParameter.Count();
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

        public ActionResult Create(GeoFencing GeoFencing, FormCollection form) //Create submit
        {
            List<string> Msgs = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string NewLoc = form["location-table"] == "0" ? "" : form["location-table"];
                    string NewDept = form["department-table"] == "0" ? "" : form["department-table"];
                    using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, new System.TimeSpan(0, 30, 0)))
                    {

                        GeoFencing.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                        GeoFencing OGeoFence = new GeoFencing()
                        {
                            LatitudeOriginal = GeoFencing.LatitudeOriginal,
                            LatitudeReference = GeoFencing.LatitudeReference,
                            LongitudeOriginal = GeoFencing.LongitudeOriginal,
                            LongitudeReference = GeoFencing.LongitudeReference,
                            Narration = GeoFencing.Narration,
                            DistanceMeter = GeoFencing.DistanceMeter,
                            DBTrack = GeoFencing.DBTrack,
                            ESSAttAppl = GeoFencing.ESSAttAppl
                        };
                        db.GeoFencing.Add(OGeoFence);
                        db.SaveChanges();
                        if (NewLoc != null && NewLoc != "")
                        {
                            int LocId = Convert.ToInt32(NewLoc);
                            Location loc_val = db.Location.Find(LocId);
                            if (loc_val != null)
                            {
                                loc_val.GeoFencingParameter = OGeoFence;
                                db.GeoFencing.Attach(OGeoFence);
                                db.Entry(OGeoFence).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(OGeoFence).State = System.Data.Entity.EntityState.Detached;

                            }
                        }
                        if (NewDept != null && NewDept != "")
                        {
                            var id = Utility.StringIdsToListIds(NewDept);
                            foreach (var ca in id)
                            {
                                Department dept_val = db.Department.Find(ca);
                                if (dept_val != null)
                                {
                                    dept_val.GeoFencingParameter = OGeoFence;
                                    db.GeoFencing.Attach(OGeoFence);
                                    db.Entry(OGeoFence).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    db.Entry(OGeoFence).State = System.Data.Entity.EntityState.Detached;

                                }
                            }
                        }
                        ts.Complete();
                        Msgs.Add("Data Saved successfully");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msgs }, JsonRequestBehavior.AllowGet);
                    }

                }
                catch (Exception ex)
                {
                    // DbContextExtensions.RefreshEntites<EmployeePayroll>(db, RefreshMode.StoreWins);
                    LogFile Logfile = new LogFile();
                    ErrorLog Err = new ErrorLog()
                    {
                        ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                        ExceptionMessage = ex.Message,
                        ExceptionStackTrace = ex.StackTrace,
                        LineNo = Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                        LogTime = DateTime.Now
                    };
                    Logfile.CreateLogFile(Err);

                    List<string> Msg = new List<string>();
                    Msg.Add(ex.Message);

                    // return Json(new Object[] { "", "", Msg }, JsonRequestBehavior.AllowGet);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
            }
        }


        [HttpPost]
        public ActionResult Delete(string data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var id = int.Parse(data.ToString());
                    var GeoFence = db.Location.Include(e => e.GeoFencingParameter).Where(e => e.GeoFencingParameter.Id == id).FirstOrDefault();
                    if (GeoFence != null)
                    {
                        GeoFence.GeoFencingParameter = null;
                        db.Location.Attach(GeoFence);
                        db.Entry(GeoFence).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                    }

                    Msg.Add("  Data removed successfully  ");
                    return Json(new Utility.JsonReturnClass { data = data.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                }
                catch (DbUpdateConcurrencyException)
                {
                    return RedirectToAction("Delete", new { concurrencyError = true, id = data });
                }
                catch (RetryLimitExceededException /* dex */)
                {
                    //Log the error (uncomment dex variable name and add a line here to write a log.)
                    //ModelState.AddModelError("", "Unable to delete. Try again, and if the problem persists, see your system administrator.");
                    //return RedirectToAction("Delete");
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
}