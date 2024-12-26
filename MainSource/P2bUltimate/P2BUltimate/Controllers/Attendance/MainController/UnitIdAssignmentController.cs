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

namespace P2BUltimate.Controllers.Attendance.MainController
{
    public class UnitIdAssignmentController : Controller
    {
        //
        // GET: /UnitIdAssignment/
        public ActionResult Index()
        {
            return View("~/Views/Attendance/MainViews/UnitIdAssignment/Index.cshtml");
        }

        public ActionResult GetNewGeoStructDetails(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<int> GeoIds = new List<int>();
               

               
                    var list1 = db.GeoStruct.Include(e => e.Company).Include(e => e.Location)
                        .Where(e => e.Location != null).Include(e => e.Location.LocationObj)
                        .Include(e => e.Department).Include(e => e.Department.DepartmentObj).ToList();
                    var list2 = db.UnitIdAssignment.Where(e => e.GeoStruct.Count() > 0).ToList().SelectMany(e => e.GeoStruct);
                    var list3 = list1.Except(list2);

                   
                    List<GeoData> geo_dataList = new List<GeoData>();
                    foreach (var item in list3)
                    {
                        GeoData geo_data = new GeoData();
                        var t = db.GeoStruct.Where(e => e.Location_Id == item.Location_Id && e.Department_Id != null).ToList();
                        if (t.Count() > 0)
                        {
                            if (item.Department != null)
                            {
                                geo_data = new GeoData()
                                {
                                    code = item.Id.ToString(),
                                    value = item.Company.FullDetails + " " + "Location : " + item.Location.FullDetails + " Department : " + item.Department.FullDetails
                                };
                            }
                        }
                        else
                        {
                            if (item.Department != null)
                            {
                                geo_data = new GeoData()
                                {
                                    code = item.Id.ToString(),
                                    value = item.Company.FullDetails + " " + "Location : " + item.Location.FullDetails + " Department : " + item.Department.FullDetails
                                };
                            }
                            else
                            {
                                geo_data = new GeoData()
                                {
                                    code = item.Id.ToString(),
                                    value = item.Company.FullDetails + " " + "Location : " + item.Location.FullDetails
                                };
                            }
                        }
                        if (geo_data.code != null)
                        {
                            geo_dataList.Add(geo_data);
                        }
                       
                    }

                    //var geo_data = list3.Select(e => new
                    //{
                    //    code = e.Id.ToString(),

                    //    value = e.Company.FullDetails + " " + "Location : " + e.Location.FullDetails + e.Department != null ? " Department : " + e.Department.FullDetails : ""

                    //}).ToList();
                    return Json(geo_dataList, JsonRequestBehavior.AllowGet);
                
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

        public class GeoData
        {
            public string code { get; set; }
            public string value { get; set; }
         

        }

        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                
                var Q = db.UnitIdAssignment
                   .Where(e => e.Id == data).Select
                   (e => new
                   {
                       UnitId = e.UnitId, 
                       Action = e.DBTrack.Action, 
                   }).ToList();

                TempData["RowVersion"] = db.UnitIdAssignment.Find(data).RowVersion;
                return Json(new object[] { Q, "", "", "", "" }, JsonRequestBehavior.AllowGet);
            }
            
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult EditSave(UnitIdAssignment UnitIdAssignment, int data, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string NewGeoStruct = form["NewGeoT-table"] == "0" ? "" : form["NewGeoT-table"];


                    List<GeoStruct> OGeoStructList = new List<GeoStruct>();


                    var db_data = db.UnitIdAssignment.Include(e => e.GeoStruct).Where(e => e.Id == data).SingleOrDefault();
                    OGeoStructList.AddRange(db_data.GeoStruct);

                    if (OGeoStructList != null)
                    {
                        var ids = Utility.StringIdsToListIds(NewGeoStruct);
                        foreach (var ca in ids)
                        {
                            var GeoStruct_val = db.GeoStruct.Find(ca);
                            OGeoStructList.Add(GeoStruct_val); 
                        }
                    }
                    else
                    {
                        db_data.GeoStruct = null;
                    }
                    db_data.GeoStruct = OGeoStructList;
                
                    if (ModelState.IsValid)
                    {
                         
                        UnitIdAssignment blog = null; // to retrieve old data
                        using (var context = new DataBaseContext())
                        {
                            blog = context.UnitIdAssignment.Where(e => e.Id == data).SingleOrDefault();
                        }

                        UnitIdAssignment.DBTrack = new DBTrack
                        {
                            CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                            CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                            Action = "M",
                            ModifiedBy = SessionManager.UserName,
                            ModifiedOn = DateTime.Now
                        };
                        try
                        {
                            db.UnitIdAssignment.Attach(db_data);
                            db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            TempData["RowVersion"] = db_data.RowVersion;
                            db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;
 
                       
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = UnitIdAssignment.Id, Val = UnitIdAssignment.UnitId, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { SalHead.Id, SalHead.Name, "Record Updated", JsonRequestBehavior.AllowGet });
                        }
                        catch (DataException /* dex */)
                        {
                            Msg.Add(" Unable to Edit.Try again, and if the problem persists, see your system administrator");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //Log the error (uncomment dex variable name after DataException and add a line here to write a log.
                            // ModelState.AddModelError(string.Empty, "Unable to Edit. Try again, and if the problem persists contact your system administrator.");
                            // return RedirectToAction("Edit");
                        }
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
                        Msg.Add(errorMsg);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        // return this.Json(new { msg = errorMsg });
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
            //  return View();
        }

        [HttpPost]
        public ActionResult GridDelete(int data,int data2)
        {
            List<string> Msg = new List<string>();
            try
            {
                using (DataBaseContext db = new DataBaseContext())
                {
                    UnitIdAssignment OUnitIdAssign = db.UnitIdAssignment.Include(e => e.GeoStruct).Where(e => e.Id == data2).FirstOrDefault();

                  
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        try
                        {
                            OUnitIdAssign.GeoStruct.Remove(db.GeoStruct.Find(data));
                            //db.UnitIdAssignment.Attach(OUnitIdAssign);
                            //db.Entry(PromoServBook).State = System.Data.Entity.EntityState.Deleted;
                            db.SaveChanges();
                            //db.Entry(PromoServBook).State = System.Data.Entity.EntityState.Detached;
                            ts.Complete();
                            return this.Json(new { status = true, responseText = "Data removed successfully.", JsonRequestBehavior.AllowGet });



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
                            // List<string> Msg = new List<string>();
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

        public ActionResult Get_Details(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                { 

                    var db_data = db.UnitIdAssignment.Include(e => e.GeoStruct)
                  
                        .Include(e => e.GeoStruct.Select(r => r.Location))
                         .Include(e => e.GeoStruct.Select(r => r.Location.LocationObj))
                        .Include(e => e.GeoStruct.Select(r => r.Department))
                        .Include(e => e.GeoStruct.Select(r => r.Department.DepartmentObj)).AsNoTracking()
                        .Where(e => e.Id == data).SingleOrDefault();
                    if (db_data != null)
                    {
                        List<UnitChildDataClass> returndata = new List<UnitChildDataClass>();
                        foreach (var item in db_data.GeoStruct)
                        {
                            if (item.Department != null)
                            {
                                returndata.Add(new UnitChildDataClass
                                {
                                    Id = item.Id,
                                    Geostruct = "Location : " + item.Location.FullDetails + " Department : " + item.Department.FullDetails
                                });
                            }
                            else
                            {
                                returndata.Add(new UnitChildDataClass
                                {
                                    Id = item.Id,
                                    Geostruct = "Location : " + item.Location.FullDetails  
                                });
                            }
                            
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

                    List<string> Msg = new List<string>();
                    LogFile Logfile = new LogFile();
                    ErrorLog Err = new ErrorLog()
                    {
                        ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                        ExceptionMessage = e.Message,
                        ExceptionStackTrace = e.StackTrace,
                        LineNo = Convert.ToString(new System.Diagnostics.StackTrace(e, true).GetFrame(0).GetFileLineNumber()),
                        LogTime = DateTime.Now
                    };
                    Logfile.CreateLogFile(Err);
                    Msg.Add(e.Message);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
            }
        }

 
        public ActionResult Create(UnitIdAssignment UnitIdAssignment, FormCollection form) //Create submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string NewGeoStruct = form["NewGeoT-table"] == "0" ? "" : form["NewGeoT-table"];
                    using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                    new System.TimeSpan(0, 30, 0)))
                    {
                        List<GeoStruct> OGeoStructList = new List<GeoStruct>();
                        if (NewGeoStruct != null && NewGeoStruct != "")
                        {
                            var ids = Utility.StringIdsToListIds(NewGeoStruct);
                            foreach (var ca in ids)
                            {
                                var geostruct_val = db.GeoStruct.Find(ca);

                                OGeoStructList.Add(geostruct_val);
                                UnitIdAssignment.GeoStruct = OGeoStructList;
                            }
                        }
                        if (db.UnitIdAssignment.Any(o => o.UnitId.ToLower() == UnitIdAssignment.UnitId.ToLower()))
                        {
                            var code = db.UnitIdAssignment.Where(o => o.UnitId.ToLower() == UnitIdAssignment.UnitId.ToLower()).SingleOrDefault();
                            Msg.Add("UnitId already exists.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            //	return this.Json(new Object[] { "", "", "Code already exists for City - " + code.Name + ".", JsonRequestBehavior.AllowGet });
                        }
                        UnitIdAssignment.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                        UnitIdAssignment OUnit = new UnitIdAssignment() 
                        {
                            GeoStruct = UnitIdAssignment.GeoStruct,
                            UnitId = UnitIdAssignment.UnitId,
                            DBTrack = UnitIdAssignment.DBTrack
                        };
                        db.UnitIdAssignment.Add(OUnit);
                        db.SaveChanges();
                        ts.Complete();
                        Msg.Add("Data Saved successfully");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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

                    
                    Msg.Add(ex.Message);

                    // return Json(new Object[] { "", "", Msg }, JsonRequestBehavior.AllowGet);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public ActionResult Emp_Grid(ParamModel param, string y)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var all = db.UnitIdAssignment.ToList();
                    // for searchs
                    IEnumerable<UnitIdAssignment> fall;
                    if (param.sSearch == null)
                    {
                        fall = all;
                    }
                    else
                    {
                        fall = all.Where(e => e.UnitId  == param.sSearch).ToList();
                    }

                    //for column sorting
                    var sortindex = Convert.ToInt32(Request["iSortCol_0"]);

                    Func<UnitIdAssignment, string> orderfunc = (c =>
                                                                Convert.ToUInt32(sortindex) == 0 ? c.Id.ToString() :
                                                                sortindex == 1 ? c.UnitId : "");
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
                                UnitId = item.UnitId
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

                                     select new[] { null, Convert.ToString(c.Id), c.UnitId, };
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

        [HttpPost]
        public ActionResult Delete(string data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var id = int.Parse(data.ToString()); 
                    var UnitIdAssign = db.UnitIdAssignment.Include(e => e.GeoStruct).Where(e => e.Id == id).FirstOrDefault();
                    if (UnitIdAssign != null)
                    {
                        db.UnitIdAssignment.Remove(UnitIdAssign);
                    }
                    db.SaveChanges();
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