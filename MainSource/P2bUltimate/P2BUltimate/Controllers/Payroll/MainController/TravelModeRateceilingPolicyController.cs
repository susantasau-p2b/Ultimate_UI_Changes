using P2b.Global;
using P2BUltimate.App_Start;
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
using System.Threading.Tasks;
using System.Collections;
using System.Data.Entity.Core.Objects;
using System.Reflection;
using Payroll;
using P2BUltimate.Security;

namespace P2BUltimate.Controllers.Payroll.MainController
{
    public class TravelModeRateceilingPolicyController : Controller
    {
        //
        // GET: /TravelMode_Rateceiling_Policy/
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/TravelModeRateceilingPolicy/Index.cshtml");
        }


        public ActionResult Partial_TravelModeEligibilityPolicy()
        {
            return View("~/Views/Shared/Payroll/_TravelModeEligibilityPolicy.cshtml");
        }

        public ActionResult Partial_Wages()
        {
            return View("~/Views/Shared/Payroll/_Wages.cshtml");

        }

        public ActionResult Partial_ServiceRange()
        {
            return View("~/Views/Shared/Payroll/_ServiceRange.cshtml");
        }


        public ActionResult EditTravelModeEligibilityPolicy_partial(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var add = db.TravelModeEligibilityPolicy.Include(e => e.TravelMode)
                                      .Include(e => e.ClassOfTravel)

                                      .Where(e => e.Id == data).SingleOrDefault();

                var r = (from ca in db.TravelModeEligibilityPolicy
                         select new
                         {
                             Id = ca.Id,
                             TA_TM_Elligibilty_Code = ca.TA_TM_Elligibilty_Code,
                             TravelMode_Id = ca.TravelMode.Id,
                             ClassTravel_id = ca.ClassOfTravel.Id

                         }).Where(e => e.Id == data).SingleOrDefault();

                return Json(r, JsonRequestBehavior.AllowGet);


            }
        }

        public ActionResult EditTA_TMRC_Eligibility_Wages_partial(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var add = db.Wages.Include(e => e.WagesName)
                                      .Include(e => e.Percentage)

                                      .Where(e => e.Id == data).SingleOrDefault();

                var r = (from ca in db.Wages
                         select new
                         {
                             Id = ca.Id,
                             WagesName = ca.WagesName,
                             WagesName_Id = ca.WagesName

                         }).Where(e => e.Id == data).SingleOrDefault();

                return Json(r, JsonRequestBehavior.AllowGet);


            }
        }
        public ActionResult Delete(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();

                TravelModeRateCeilingPolicy travelmoderateceilingpolicy = db.TravelModeRateCeilingPolicy.Find(data);
                try
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        db.Entry(travelmoderateceilingpolicy).State = System.Data.Entity.EntityState.Deleted;
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
        public ActionResult GetTravelModeEligibilitydata(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.TravelModeEligibilityPolicy
                    .Include(e => e.TravelMode)
                    .Include(e => e.ClassOfTravel)
                    .ToList();
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.TravelModeEligibilityPolicy.Where(e => e.Id != a).ToList();
                        else
                            fall = fall.Where(e => e.Id != a).ToList();
                    }
                }

                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = "TravelMode:" + (ca.TravelMode != null ? ca.TravelMode.LookupVal : "") + ", ClassOfTravel :" + (ca.ClassOfTravel != null ? ca.ClassOfTravel.LookupVal : "") }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
            }
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
        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult Create(TravelModeRateCeilingPolicy c, FormCollection form) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string TravelModeEligibilityPolicylist = form["Travel_Mode_Eligibility_PolicyList"] == "0" ? "" : form["Travel_Mode_Eligibility_PolicyList"];
                string wages = form["Wageslist"] == "0" ? "" : form["Wageslist"];
                string Servicerangelist = form["ServiceRangeList"] == "0" ? "" : form["ServiceRangeList"];
                List<String> Msg = new List<String>();
                try
                {
                    if (TravelModeEligibilityPolicylist != null)
                    {
                        if (TravelModeEligibilityPolicylist != "")
                        {
                            int id = Convert.ToInt32(TravelModeEligibilityPolicylist);
                            var val = db.TravelModeEligibilityPolicy.Where(e => e.Id == id).SingleOrDefault();
                            c.TravelModeEligibilityPolicy = val;
                        }
                    }

                    if (wages != null)
                    {
                        if (wages != "")
                        {
                            int id = Convert.ToInt32(wages);
                            var val = db.Wages.Where(e => e.Id == id).SingleOrDefault();
                            c.TA_TMRC_Eligibility_Wages = val;
                        }
                    }

                    if (Servicerangelist != null && Servicerangelist != "")
                    {
                        var ids = Utility.StringIdsToListIds(Servicerangelist);
                        var ServiceRangelist = new List<ServiceRange>();
                        foreach (var item in ids)
                        {

                            int servicerangelistids = Convert.ToInt32(item);
                            var val = db.ServiceRange.Find(servicerangelistids);
                            if (val != null)
                            {
                                ServiceRangelist.Add(val);
                            }
                        }
                        c.DistanceRange = ServiceRangelist;
                    }

                    //if (Servicerangelist != null)
                    //{
                    //    if (Servicerangelist != "")
                    //    {
                    //        int id = Convert.ToInt32(Servicerangelist);
                    //        var val = db.ServiceRange.Where(e => e.Id == id).SingleOrDefault();
                    //        c.DistanceRange = val;
                    //    }
                    //}


                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {


                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            TravelModeRateCeilingPolicy travelmodeeRateceiling = new TravelModeRateCeilingPolicy()
                            {
                                TravelModeEligibilityPolicy = c.TravelModeEligibilityPolicy,
                                DistanceRange = c.DistanceRange,
                                TA_TMRC_Eligibility_Wages = c.TA_TMRC_Eligibility_Wages,
                                TA_TMRC_Elligibilty_Code = c.TA_TMRC_Elligibilty_Code,
                                DBTrack = c.DBTrack
                            };

                            db.TravelModeRateCeilingPolicy.Add(travelmodeeRateceiling);
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
                var TravelModeRateCeilingPolicy = new List<TravelModeRateCeilingPolicy>();
                TravelModeRateCeilingPolicy = db.TravelModeRateCeilingPolicy
                                      .Include(e => e.TA_TMRC_Eligibility_Wages)
                                      .Include(e => e.DistanceRange)
                                      .ToList();
                IEnumerable<TravelModeRateCeilingPolicy> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = TravelModeRateCeilingPolicy;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE
                            .Where((e => (e.Id.ToString().Contains(gp.searchString))
                                || (e.TA_TMRC_Elligibilty_Code.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                                || (e.TA_TMRC_Eligibility_Wages == null ? false : e.TA_TMRC_Eligibility_Wages.FullDetails.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                                //|| (e.DistanceRange == null ? false : e.DistanceRange.FullDetails.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                               )).ToList()
                        .Select(a => new Object[] { a.TA_TMRC_Elligibilty_Code, a.TA_TMRC_Eligibility_Wages == null ? "" : a.TA_TMRC_Eligibility_Wages.FullDetails.ToString(), a.Id });
                        // jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Name != null ? a.Name.LookupVal : null), Convert.ToString(a.FromDate), Convert.ToString(a.ToDate), Convert.ToString(a.Default) }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] {  a.TA_TMRC_Elligibilty_Code, a.TA_TMRC_Eligibility_Wages == null ? "" : a.TA_TMRC_Eligibility_Wages.FullDetails.ToString(), a.Id }).ToList();
                        //jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Name != null ? a.Name.LookupVal : null, a.FromDate, a.ToDate, a.Default }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = TravelModeRateCeilingPolicy;
                    Func<TravelModeRateCeilingPolicy, string> orderfuc = (c =>
                                                               gp.sidx == "Name" ? c.TA_TMRC_Elligibilty_Code :
                                                               gp.sidx == "From Date" ? c.TA_TMRC_Eligibility_Wages == null ? "" : c.TA_TMRC_Eligibility_Wages.FullDetails.ToString() :
                                                               //gp.sidx == "To Date" ? c.DistanceRange == null ? "" : c.DistanceRange.FullDetails.ToString() :
                                                               gp.sidx == "Id" ? c.Id.ToString() : "");
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.TA_TMRC_Elligibilty_Code, a.TA_TMRC_Eligibility_Wages == null ? "" : a.TA_TMRC_Eligibility_Wages.FullDetails.ToString(), a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.TA_TMRC_Elligibilty_Code, a.TA_TMRC_Eligibility_Wages == null ? "" : a.TA_TMRC_Eligibility_Wages.FullDetails.ToString(), a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.TA_TMRC_Elligibilty_Code, a.TA_TMRC_Eligibility_Wages == null ? "" : a.TA_TMRC_Eligibility_Wages.FullDetails.ToString(), a.Id }).ToList();
                    }
                    totalRecords = TravelModeRateCeilingPolicy.Count();
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

        public class returnEditClass
        {
            public Array ServiceRange_Id { get; set; }
            public Array ServiceRangeFullDetails { get; set; }
            
        }
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var returndata = db.TravelModeRateCeilingPolicy
                                 .Include(e => e.DistanceRange)
                                 .Include(e => e.TA_TMRC_Eligibility_Wages)
                                 .Include(e => e.TravelModeEligibilityPolicy)
                                  .Where(e => e.Id == data).AsEnumerable()
                    .Select(e => new
                    {
                        Name = e.TA_TMRC_Elligibilty_Code,
                        Wages_id = e.TA_TMRC_Eligibility_Wages == null ? 0 : e.TA_TMRC_Eligibility_Wages.Id,
                        Wages_Fulldetails = e.TA_TMRC_Eligibility_Wages == null ? "" : e.TA_TMRC_Eligibility_Wages.FullDetails,
                        //ServiceRange_id = e.DistanceRange == null ? 0 : e.DistanceRange.Id,
                        //ServiceRange_Fulldetails = e.DistanceRange == null ? "" : e.DistanceRange.FullDetails,
                        Travemodeeligibility_id = e.TravelModeEligibilityPolicy == null ? 0 : e.TravelModeEligibilityPolicy.Id,
                        Travelmodeeligibilitypolicycode = e.TravelModeEligibilityPolicy == null ? "" : e.TravelModeEligibilityPolicy.TA_TM_Elligibilty_Code
                    }).ToList();

                List<returnEditClass> oreturnEditClass = new List<returnEditClass>();

                var add_data = db.TravelModeRateCeilingPolicy
                  .Include(e => e.DistanceRange) 
                  .Where(e => e.Id == data).SingleOrDefault();

                var fall = add_data.DistanceRange.Select(e => new
                {
                    Id = e.Id,
                    FullDetails = e.FullDetails,

                }).ToList();

                oreturnEditClass.Add(new returnEditClass
                {
                    ServiceRange_Id  = fall.Select(a => a.Id.ToString()).ToArray(),
                    ServiceRangeFullDetails = fall.Select(a => a.FullDetails).ToArray() 
                });

                return Json(new Object[] { returndata, oreturnEditClass, "", "", JsonRequestBehavior.AllowGet });
            }
        }
        
        [HttpPost]
        public ActionResult EditSave(TravelModeRateCeilingPolicy data1, int data, FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    //Calendar c = db.Calendar.Find(data);
                    string TravelModeEligibilityPolicylist = form["Travel_Mode_Eligibility_PolicyList"] == "0" ? "" : form["Travel_Mode_Eligibility_PolicyList"];
                    string wages = form["Wageslist"] == "0" ? "" : form["Wageslist"];
                    string Servicerangelist = form["ServiceRangeList"] == "0" ? "" : form["ServiceRangeList"];
                    
                    
                          var db_data = db.TravelModeRateCeilingPolicy

                         .Include(q => q.TravelModeEligibilityPolicy)
                         .Include(q => q.DistanceRange)
                         .Include(q => q.TA_TMRC_Eligibility_Wages)
                         .Where(a => a.Id == data).SingleOrDefault();
                    if (TravelModeEligibilityPolicylist != null)
                    {
                        if (TravelModeEligibilityPolicylist != "")
                        {
                            int id = Convert.ToInt32(TravelModeEligibilityPolicylist);
                            var val = db.TravelModeEligibilityPolicy.Where(e => e.Id == id).SingleOrDefault();
                            db_data.TravelModeEligibilityPolicy = val;
                        }
                    }
                    else
                    {
                        db_data.TravelModeEligibilityPolicy = null;
                    }

                    if (wages != null)
                    {
                        if (wages != "")
                        {
                            int id = Convert.ToInt32(wages);
                            var val = db.Wages.Where(e => e.Id == id).SingleOrDefault();
                            db_data.TA_TMRC_Eligibility_Wages = val;
                        }
                    }
                    else
                    {
                        db_data.TA_TMRC_Eligibility_Wages = null;
                    }
 
                    List<ServiceRange> servicerangelistnew = new List<ServiceRange>(); 
                    if (Servicerangelist != null)
                    {
                        var ids = Utility.StringIdsToListIds(Servicerangelist);
                        foreach (var ca in ids)
                        {
                            var Lookup_val = db.ServiceRange.Find(ca);

                            servicerangelistnew.Add(Lookup_val);
                            db_data.DistanceRange = servicerangelistnew;
                        }
                    }
                    else
                    {
                        db_data.DistanceRange = null;
                    }
                    db.TravelModeRateCeilingPolicy.Attach(db_data);
                    db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    TempData["RowVersion"] = db_data.RowVersion;


                    //if (Servicerangelist != null)
                    //{
                    //    if (Servicerangelist != "")
                    //    {
                    //        int id = Convert.ToInt32(Servicerangelist);
                    //        var val = db.ServiceRange.Where(e => e.Id == id).SingleOrDefault();
                    //        db_data.DistanceRange = val;
                    //    }
                    //}
                    //else
                    //{
                    //    db_data.DistanceRange = null;
                    //}
                    //var alrdy = db.Calendar.Include(a => a.Name).Where(e => e.Name.LookupVal.ToString().ToUpper() == db_data.Name.LookupVal.ToString().ToUpper() && e.Default == true && data1.Default == true).Count();

                    //if (alrdy > 0)
                    //{
                    //    Msg.Add("   Default  Year already exist. ");
                    //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //    // return this.Json(new Object[] { "", "", "From Date already exists.", JsonRequestBehavior.AllowGet });
                    //}
                    using (TransactionScope ts = new TransactionScope())
                    {
                        data1.DBTrack = new DBTrack
                        {
                            CreatedBy = db_data.DBTrack.CreatedBy == null ? null : db_data.DBTrack.CreatedBy,
                            CreatedOn = db_data.DBTrack.CreatedOn == null ? null : db_data.DBTrack.CreatedOn,
                            Action = "M",
                            ModifiedBy = SessionManager.UserName,
                            ModifiedOn = DateTime.Now
                        };

                        db_data.TA_TMRC_Elligibilty_Code = data1.TA_TMRC_Elligibilty_Code;
                        db_data.TA_TMRC_Eligibility_Wages = db_data.TA_TMRC_Eligibility_Wages;
                        db_data.TravelModeEligibilityPolicy = db_data.TravelModeEligibilityPolicy;
                      //  db_data.DistanceRange = db_data.DistanceRange;
                        db_data.DBTrack = db_data.DBTrack;


                        db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;
                        ts.Complete();
                    }
                    Msg.Add("  Data Saved successfully  ");
                    return Json(new Utility.JsonReturnClass { Id = db_data.Id, Val = db_data.TA_TMRC_Elligibilty_Code, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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
