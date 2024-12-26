using Payroll;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using P2b.Global;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using P2BUltimate.Security;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Text;
using System.Transactions;
using System.Data.Entity;
using System.Threading.Tasks;
using Attendance;

namespace P2BUltimate.Controllers.Payroll.MainController
{
    public class BMSModuleTypePolicyAssignmentController : Controller
    {
        // GET: BMSModuleTypePolicyAssignment
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/BMSModuleTypePolicyAssignment/Index.cshtml");
        }

        public ActionResult partial_TravelEligibilityPolicy()
        {
            return View("~/Views/Shared/Payroll/_TravelEligibilityPolicy.cshtml");
        }

        public ActionResult partial_TravelModeEligibilityPolicy()
        {
            return View("~/Views/Shared/Payroll/_TravelModeEligibilityPolicy.cshtml");
        }


        public ActionResult partial_HotelEligibilityPolicy()
        {
            return View("~/Views/Shared/Payroll/_HotelEligibilityPolicy.cshtml");
        }

        public ActionResult partial_TravelModeRateCeilingPolicy()
        {
            return View("~/Views/Shared/Payroll/_TravelModeRateCeilingPolicy.cshtml");
        }


        public ActionResult GetLookupTravelEligibilityPolicy(string data)
        {

            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.TravelEligibilityPolicy.ToList();


                //var employeedata = db.Employee
                //          .Include(e => e.FamilyDetails)
                //          .Include(e => e.FamilyDetails.Select(t => t.MemberName))
                //              .Where(e => e.Id == empids).ToList();
                //var fall = employeedata.SelectMany(e => e.DIARYAdvanceClaim).ToList();

                IEnumerable<TravelEligibilityPolicy> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.TravelEligibilityPolicy.ToList().Where(d => d.FullDetails.Contains(data));
                }
                else
                {
                    //var list1 = db.FamilyDetails.ToList();

                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.FullDetails }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetLookupHotelEligibilityPolicy(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.HotelEligibilityPolicy.ToList();


                //var employeedata = db.Employee
                //          .Include(e => e.FamilyDetails)
                //          .Include(e => e.FamilyDetails.Select(t => t.MemberName))
                //              .Where(e => e.Id == empids).ToList();
                //var fall = employeedata.SelectMany(e => e.DIARYAdvanceClaim).ToList();

                IEnumerable<HotelEligibilityPolicy> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.HotelEligibilityPolicy.ToList().Where(d => d.FullDetails.Contains(data));
                }
                else
                {
                    //var list1 = db.FamilyDetails.ToList();

                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.FullDetails }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetlookupTravelModeEligibilityPolicy(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.TravelModeEligibilityPolicy.ToList();


                IEnumerable<TravelModeEligibilityPolicy> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.TravelModeEligibilityPolicy.ToList().Where(d => d.FullDetails.Contains(data));
                }
                else
                {
                    //var list1 = db.FamilyDetails.ToList();

                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.FullDetails }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetlookupTravelModeRateCeilingPolicy(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.TravelModeRateCeilingPolicy.ToList();
                IEnumerable<TravelModeRateCeilingPolicy> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.TravelModeRateCeilingPolicy.ToList().Where(d => d.FullDetails.Contains(data));
                }
                else
                {
                    //var list1 = db.FamilyDetails.ToList();

                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.FullDetails }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetLookupDAEligibilityPolicy(string data)
        {

            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.DAEligibilityPolicy.ToList();

                IEnumerable<DAEligibilityPolicy> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.DAEligibilityPolicy.ToList().Where(d => d.FullDetails.Contains(data));
                }
                else
                {
                    //var list1 = db.FamilyDetails.ToList();

                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.FullDetails }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Create(BMSModuleTypePolicyAssignment c, FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    string DIARYtypelist = form["DIARYTypelist"] == "0" ? "" : form["DIARYTypelist"];
                    string TravelEligibilityPolicy = form["TravelEligibilityPolicyList"] == "0" ? "" : form["TravelEligibilityPolicyList"];
                    string HotelEligibilityPolicy = form["HotelEligibilityPolicyList"] == "0" ? "" : form["HotelEligibilityPolicyList"];
                    string TravelModeEligibilityPolicy = form["TravelModeEligibilityPolicyList"] == "0" ? "" : form["TravelModeEligibilityPolicyList"];
                    string TravelModeRateCeilingPolicy = form["TravelModeRateCeilingPolicyList"] == "0" ? "" : form["TravelModeRateCeilingPolicyList"];
                    string DAEligibilityPolicy = form["DAEligibilityPolicyList"] == "0" ? "" : form["DAEligibilityPolicyList"];
                    //policyname
                    if (DIARYtypelist != null && DIARYtypelist != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(DIARYtypelist));
                        c.PolicyType = val;
                    }
                    //traval
                    if (TravelEligibilityPolicy != null)
                    {
                        var ids = Utility.StringIdsToListIds(TravelEligibilityPolicy);
                        var travelbookinglist = new List<TravelEligibilityPolicy>();
                        foreach (var item in ids)
                        {

                            int travallistid = Convert.ToInt32(item);
                            var val = db.TravelEligibilityPolicy.Where(e => e.Id == travallistid).SingleOrDefault();
                            if (val != null)
                            {
                                travelbookinglist.Add(val);
                            }
                        }
                        c.TravelEligibilityPolicy = travelbookinglist;
                    }
                    //hotel
                    if (HotelEligibilityPolicy != null)
                    {
                        var ids = Utility.StringIdsToListIds(HotelEligibilityPolicy);
                        var Hoteleligibilitylist = new List<HotelEligibilityPolicy>();
                        foreach (var item in ids)
                        {

                            int hotellistid = Convert.ToInt32(item);
                            var val = db.HotelEligibilityPolicy.Where(e => e.Id == hotellistid).SingleOrDefault();
                            if (val != null)
                            {
                                Hoteleligibilitylist.Add(val);
                            }
                        }
                        c.HotelEligibilityPolicy = Hoteleligibilitylist;
                    }
                    //TravalMode
                    if (TravelModeEligibilityPolicy != null)
                    {
                        var ids = Utility.StringIdsToListIds(TravelModeEligibilityPolicy);
                        var TravalModelist = new List<TravelModeEligibilityPolicy>();
                        foreach (var item in ids)
                        {

                            int travalModelistid = Convert.ToInt32(item);
                            var val = db.TravelModeEligibilityPolicy.Where(e => e.Id == travalModelistid).SingleOrDefault();
                            if (val != null)
                            {
                                TravalModelist.Add(val);
                            }
                        }
                        c.TravelModeEligibilityPolicy = TravalModelist;
                    }
                    //TravalModeRate
                    if (TravelModeRateCeilingPolicy != null)
                    {
                        var ids = Utility.StringIdsToListIds(TravelModeRateCeilingPolicy);
                        var TravalModeRatelist = new List<TravelModeRateCeilingPolicy>();
                        foreach (var item in ids)
                        {

                            int travalModelRateistid = Convert.ToInt32(item);
                            var val = db.TravelModeRateCeilingPolicy.Where(e => e.Id == travalModelRateistid).SingleOrDefault();
                            if (val != null)
                            {
                                TravalModeRatelist.Add(val);
                            }
                        }
                        c.TravelModeRateCeilingPolicy = TravalModeRatelist;
                    }
                    //DA
                    if (DAEligibilityPolicy != null)
                    {
                        var ids = Utility.StringIdsToListIds(DAEligibilityPolicy);
                        var DAEligibilityPolicylist = new List<DAEligibilityPolicy>();
                        foreach (var item in ids)
                        {

                            int DAEligibilityPolicyid = Convert.ToInt32(item);
                            var val = db.DAEligibilityPolicy.Where(e => e.Id == DAEligibilityPolicyid).SingleOrDefault();
                            if (val != null)
                            {
                                DAEligibilityPolicylist.Add(val);
                            }
                        }
                        c.DAEligibilityPolicy = DAEligibilityPolicylist;
                    }

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            BMSModuleTypePolicyAssignment bMSModuleTypePolicyAssignment = new BMSModuleTypePolicyAssignment()
                            {
                                PolicyType = c.PolicyType,
                                TravelEligibilityPolicy = c.TravelEligibilityPolicy,
                                HotelEligibilityPolicy = c.HotelEligibilityPolicy,
                                TravelModeEligibilityPolicy = c.TravelModeEligibilityPolicy,
                                TravelModeRateCeilingPolicy = c.TravelModeRateCeilingPolicy,
                                DAEligibilityPolicy = c.DAEligibilityPolicy,
                                DBTrack = c.DBTrack

                            };
                            db.BMSModuleTypePolicyAssignment.Add(bMSModuleTypePolicyAssignment);
                            db.SaveChanges();

                            List<BMSModuleTypePolicyAssignment> bMSModuleTypePolicyAssignmentsList = new List<BMSModuleTypePolicyAssignment>();
                            var aa = db.BMSModuleTypePolicyAssignment.ToList();

                            //db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                            //db.SaveChanges();
                            // db.Entry(aa).State = System.Data.Entity.EntityState.Detached;
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

        public ActionResult GridEditSave(BMSModuleTypePolicyAssignment c, FormCollection form, string data)
        {
            List<string> Msgs = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                int Id = int.Parse(data);
                string DIARYtypelist = form["DIARYTypelist"] == "0" ? "" : form["DIARYTypelist"];
                string TravelEligibilityPolicy = form["Travel-Eligibility-Policy"] == "0" ? "" : form["Travel-Eligibility-Policy"];
                string HotelEligibilityPolicy = form["Hotel-Eligibility-Policy"] == "0" ? "" : form["Hotel-Eligibility-Policy"];
                string TravelModeEligibilityPolicy = form["Travel-Mode-Eligibility-Policy"] == "0" ? "" : form["Travel-Mode-Eligibility-Policy"];
                string TravelModeRateCeilingPolicy = form["Travel-Mode-Rate-Ceiling-Policy"] == "0" ? "" : form["Travel-Mode-Rate-Ceiling-Policy"];

                var BMSModulePolicyList = db.BMSModuleTypePolicyAssignment
                     .Include(e => e.PolicyType)
                     .Include(e => e.TravelEligibilityPolicy)
                     .Include(e => e.HotelEligibilityPolicy)
                     .Include(e => e.TravelModeEligibilityPolicy)
                     .Include(e => e.TravelModeRateCeilingPolicy)
                    .Where(e => e.Id == Id).SingleOrDefault();


                if (DIARYtypelist != null && DIARYtypelist != "")
                {
                    var val = db.LookupValue.Find(int.Parse(DIARYtypelist));
                    BMSModulePolicyList.PolicyType = val;
                }
                else
                {
                    BMSModulePolicyList.PolicyType = null;
                }
                //traval
                if (TravelEligibilityPolicy != null)
                {
                    var ids = Utility.StringIdsToListIds(TravelEligibilityPolicy);
                    var TravalModelist = new List<TravelEligibilityPolicy>();
                    foreach (var item in ids)
                    {

                        int travllistid = Convert.ToInt32(item);
                        var val = db.TravelEligibilityPolicy.Where(e => e.Id == travllistid).SingleOrDefault();
                        if (val != null)
                        {
                            TravalModelist.Add(val);
                        }
                    }
                    BMSModulePolicyList.TravelEligibilityPolicy = TravalModelist;
                }
                else
                {
                    BMSModulePolicyList.TravelEligibilityPolicy = null;
                }
                //hotel
                if (HotelEligibilityPolicy != null)
                {
                    var ids = Utility.StringIdsToListIds(TravelEligibilityPolicy);
                    var hotellistid = new List<HotelEligibilityPolicy>();
                    foreach (var item in ids)
                    {

                        int HotelEligibilitylistid = Convert.ToInt32(item);
                        var val = db.HotelEligibilityPolicy.Where(e => e.Id == HotelEligibilitylistid).SingleOrDefault();
                        if (val != null)
                        {
                            hotellistid.Add(val);
                        }
                    }
                    BMSModulePolicyList.HotelEligibilityPolicy = hotellistid;
                }
                else
                {
                    BMSModulePolicyList.HotelEligibilityPolicy = null;
                }
                //TravalMode
                if (TravelModeEligibilityPolicy != null)
                {
                    var ids = Utility.StringIdsToListIds(TravelEligibilityPolicy);
                    var TravalModelist = new List<TravelModeEligibilityPolicy>();
                    foreach (var item in ids)
                    {

                        int TravelModeEligibilitylistid = Convert.ToInt32(item);
                        var val = db.TravelModeEligibilityPolicy.Where(e => e.Id == TravelModeEligibilitylistid).SingleOrDefault();
                        if (val != null)
                        {
                            TravalModelist.Add(val);
                        }
                    }
                    BMSModulePolicyList.TravelModeEligibilityPolicy = TravalModelist;
                }
                else
                {
                    BMSModulePolicyList.TravelModeEligibilityPolicy = null;
                }

                //travalModeRate
                if (TravelModeRateCeilingPolicy != null)
                {
                    var ids = Utility.StringIdsToListIds(TravelEligibilityPolicy);
                    var TravelModeRateList = new List<TravelModeRateCeilingPolicy>();
                    foreach (var item in ids)
                    {

                        int TravelModeRatelistid = Convert.ToInt32(item);
                        var val = db.TravelModeRateCeilingPolicy.Where(e => e.Id == TravelModeRatelistid).SingleOrDefault();
                        if (val != null)
                        {
                            TravelModeRateList.Add(val);
                        }
                    }
                    BMSModulePolicyList.TravelModeRateCeilingPolicy = TravelModeRateList;
                }
                else
                {
                    BMSModulePolicyList.TravelModeRateCeilingPolicy = null;
                }

                BMSModulePolicyList.PolicyType = BMSModulePolicyList.PolicyType;
                BMSModulePolicyList.TravelEligibilityPolicy = BMSModulePolicyList.TravelEligibilityPolicy;
                BMSModulePolicyList.HotelEligibilityPolicy = BMSModulePolicyList.HotelEligibilityPolicy;
                BMSModulePolicyList.TravelModeEligibilityPolicy = BMSModulePolicyList.TravelModeEligibilityPolicy;
                BMSModulePolicyList.TravelModeRateCeilingPolicy = BMSModulePolicyList.TravelModeRateCeilingPolicy;

                using (TransactionScope ts = new TransactionScope())
                {

                    BMSModulePolicyList.DBTrack = new DBTrack
                    {
                        CreatedBy = BMSModulePolicyList.DBTrack.CreatedBy == null ? null : BMSModulePolicyList.DBTrack.CreatedBy,
                        CreatedOn = BMSModulePolicyList.DBTrack.CreatedOn == null ? null : BMSModulePolicyList.DBTrack.CreatedOn,
                        Action = "M",
                        ModifiedBy = SessionManager.UserName,
                        ModifiedOn = DateTime.Now
                    };


                    try
                    {
                        db.BMSModuleTypePolicyAssignment.Attach(BMSModulePolicyList);
                        db.Entry(BMSModulePolicyList).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        db.Entry(BMSModulePolicyList).State = System.Data.Entity.EntityState.Detached;

                        ts.Complete();
                        return this.Json(new { status = true, responseText = "Data Updated Successfully.", JsonRequestBehavior.AllowGet });
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        return RedirectToAction("Create", new { concurrencyError = true, id = BMSModulePolicyList.Id });
                    }
                    catch (DataException /* dex */)
                    {
                        return this.Json(new { status = false, responseText = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
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
                        List<string> Msg = new List<string>();
                        Msg.Add(ex.Message);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
        }

        public class returndatagridclass //Parentgrid
        {
            public string PolicyType_Id { get; set; }
            public string Id { get; set; }
            public string PolicyName { get; set; }
        }

        public ActionResult Emp_Grid(ParamModel param, string y)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var all = db.BMSModuleTypePolicyAssignment.Include(e => e.PolicyType)
                  .ToList();
                    // for searchs
                    IEnumerable<BMSModuleTypePolicyAssignment> fall;
                    if (param.sSearch == null)
                    {
                        fall = all;
                    }
                    else
                    {
                        fall = all.Where(e => (e.Id.ToString().Contains(param.sSearch))

                                  || (e.PolicyType.ToString().ToUpper().Contains(param.sSearch.ToUpper()))
                                  ).ToList();
                    }

                    //for column sorting
                    var sortindex = Convert.ToInt32(Request["iSortCol_0"]);

                    Func<BMSModuleTypePolicyAssignment, string> orderfunc = (c =>
                                                                Convert.ToUInt32(sortindex) == 0 ? c.Id.ToString() :
                                                                sortindex == 1 ? c.PolicyType.LookupVal : "");
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
                                //PolicyType_Id = item.Id.ToString(),
                                Id = item.Id.ToString(),
                                PolicyName = item.PolicyType == null ? "" : item.PolicyType.LookupVal == null ? "" : item.PolicyType.LookupVal
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

                                     select new[] { null, Convert.ToString(c.Id), c.PolicyType.LookupVal, };
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

        //public class OthChildDataClass
        //{
        //    public int Id { get; set; }
        //    public bool TravelEligibilityPolicy { get; set; }
        //    public string HotelEligibilityPolicy { get; set; }
        //    public string TravelModeEligibilityPolicy { get; set; }
        //    public string TravelModeRateCeilingPolicy { get; set; }

        //}

        //public ActionResult Get_Moduledata(int data)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        try
        //        {
        //            var db_data = db.BMSModuleTypePolicyAssignment

        //                .Include(e => e.TravelEligibilityPolicy.Select(r =>r.TA_TC_Eligibilty_Code))
        //                .Include(e => e.HotelEligibilityPolicy.Select(r =>r.HotelEligibilityCode))
        //                 .Include(e => e.TravelModeEligibilityPolicy.Select(r =>r.TA_TM_Elligibilty_Code))
        //                  .Include(e => e.TravelModeRateCeilingPolicy.Select(r =>r.TA_TMRC_Elligibilty_Code))

        //                .Where(e => e.Id == data).SingleOrDefault();
        //            if (db_data != null)
        //            {
        //                List<OthChildDataClass> returndata = new List<OthChildDataClass>();
        //                foreach (var item in db_data.TravelEligibilityPolicy)
        //                {
        //                    returndata.Add(new OthChildDataClass
        //                    {
        //                        TravelEligibilityPolicy = item.FullDetails == null ? "": item.FullDetails == null ? "" :
        //                    });
        //                }
        //                   foreach (var item in db_data.HotelEligibilityPolicy)
        //                {
        //                    returndata.Add(new OthChildDataClass
        //                    {
        //                        HotelEligibilityPolicy = item.FullDetails == null ? "": item.FullDetails == null ? "" :
        //                    });
        //                }
        //                       foreach (var item in db_data.TravelModeEligibilityPolicy)
        //                {
        //                    returndata.Add(new OthChildDataClass
        //                    {
        //                        TravelModeEligibilityPolicy = item.FullDetails == null ? "": item.FullDetails == null ? "" :
        //                    });
        //                }
        //                       foreach (var item in db_data.TravelModeRateCeilingPolicy)
        //                {
        //                    returndata.Add(new OthChildDataClass
        //                    {
        //                        TravelModeRateCeilingPolicy = item.FullDetails == null ? "": item.FullDetails == null ? "" :
        //                    });
        //                }
        //                return Json(returndata, JsonRequestBehavior.AllowGet);
        //            }
        //            else
        //            {
        //                return null;
        //            }

        //        }
        //        catch (Exception ex)
        //        {
        //            List<string> Msg = new List<string>();
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

        public class OthChildDataClass
        {
            public string Id { get; set; }
            public string TravelEligibilityPolicy { get; set; }
            public string HotelEligibilityPolicy { get; set; }
            public string TravelModeEligibilityPolicy { get; set; }
            public string TravelModeRateCeilingPolicy { get; set; }
            public string DAEligibilityPolicy { get; set; }
        }

        public ActionResult Get_Moduledata(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var db_data = db.BMSModuleTypePolicyAssignment
                         .Include(e => e.TravelEligibilityPolicy)
                        .Include(e => e.HotelEligibilityPolicy)
                        .Include(e => e.TravelModeEligibilityPolicy)
                        .Include(e => e.TravelModeRateCeilingPolicy)
                        .Include(e => e.DAEligibilityPolicy)
                        .Where(e => e.Id == data)
                        .SingleOrDefault();

                    if (db_data != null)
                    {
                        List<OthChildDataClass> returndata = new List<OthChildDataClass>();

                        foreach (var item in db_data.TravelEligibilityPolicy)
                        {
                            returndata.Add(new OthChildDataClass
                            {
                                Id = item.Id.ToString(),
                                TravelEligibilityPolicy = item.FullDetails ?? "",
                            });
                        }

                        foreach (var item in db_data.HotelEligibilityPolicy)
                        {
                            returndata.Add(new OthChildDataClass
                            {
                                Id = item.Id.ToString(),
                                HotelEligibilityPolicy = item.FullDetails ?? "",
                            });
                        }

                        foreach (var item in db_data.TravelModeEligibilityPolicy)
                        {
                            returndata.Add(new OthChildDataClass
                            {
                                Id = item.Id.ToString(),
                                TravelModeEligibilityPolicy = item.FullDetails ?? "",
                            });
                        }

                        foreach (var item in db_data.TravelModeRateCeilingPolicy)
                        {
                            returndata.Add(new OthChildDataClass
                            {
                                Id = item.Id.ToString(),
                                TravelModeRateCeilingPolicy = item.FullDetails ?? "",
                            });
                        }

                        foreach (var item in db_data.DAEligibilityPolicy)
                        {
                            returndata.Add(new OthChildDataClass
                            {
                                Id = item.Id.ToString(),
                                DAEligibilityPolicy = item.FullDetails ?? "",
                            });
                        }
                        return Json(returndata, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(null, JsonRequestBehavior.AllowGet);
                    }
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
        }

        public class sessionDetails
        {
            public Array HotelEligibility_Id { get; set; }
            public Array HotelEligibility_val { get; set; }
            public Array DAEligibility_val { get; set; }
            public Array DAEligibility_Id { get; set; }
            public Array TravelEligibility_Id { get; set; }
            public Array TravelEligibility_val { get; set; }
            public Array TravelModeEligibility_Id { get; set; }
            public Array TravelModeEligibility_val { get; set; }
            public Array TravelModeRateCeiling_Id { get; set; }
            public Array TravelModeRateCeiling_val { get; set; }
            public string PolicyType_val { get; set; }

        }

        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                var BMS = db.BMSModuleTypePolicyAssignment
                    .Include(e => e.PolicyType)
                    .Include(e => e.TravelEligibilityPolicy)
                    .Include(e => e.HotelEligibilityPolicy)
                    .Include(e => e.TravelModeEligibilityPolicy)
                    .Include(e => e.TravelModeRateCeilingPolicy)
                    .Include(e => e.DAEligibilityPolicy)
                    .Where(e => e.Id == data).Select(e => new
                    {
                        PolicyType = e.PolicyType,
                        TravelEligibilityPolicy = e.TravelEligibilityPolicy,
                        HotelEligibilityPolicy = e.HotelEligibilityPolicy,
                        TravelModeEligibilityPolicy = e.TravelModeEligibilityPolicy,
                        TravelModeRateCeilingPolicy = e.TravelModeRateCeilingPolicy,
                        DAEligibilityPolicy = e.DAEligibilityPolicy

                    }).ToList();
                List<sessionDetails> contno = new List<sessionDetails>();

                var add_data = db.BMSModuleTypePolicyAssignment
                    .Include(e => e.TravelEligibilityPolicy)
                    .Include(e => e.TravelModeEligibilityPolicy)
                     .Include(e => e.TravelModeRateCeilingPolicy)
                      .Include(e => e.DAEligibilityPolicy)
                  .Where(e => e.Id == data)
                  .Select(e => new
                  {
                      e.Id,
                      e.PolicyType,
                      e.TravelEligibilityPolicy,
                      e.HotelEligibilityPolicy,
                      e.TravelModeEligibilityPolicy,
                      e.TravelModeRateCeilingPolicy,
                      e.DAEligibilityPolicy
                  })
                  .ToList();

                foreach (var ca in add_data)
                {
                    contno.Add(
                new sessionDetails
                {
                    TravelEligibility_Id = ca.TravelEligibilityPolicy.Select(e => e.Id.ToString()).ToArray(),
                    TravelEligibility_val = ca.TravelEligibilityPolicy.Select(e => e.FullDetails).ToArray(),

                    HotelEligibility_Id = ca.HotelEligibilityPolicy.Select(e => e.Id.ToString()).ToArray(),
                    HotelEligibility_val = ca.HotelEligibilityPolicy.Select(e => e.FullDetails).ToArray(),

                    TravelModeEligibility_Id = ca.TravelModeEligibilityPolicy.Select(e => e.Id.ToString()).ToArray(),
                    TravelModeEligibility_val = ca.TravelModeEligibilityPolicy.Select(e => e.FullDetails).ToArray(),

                    TravelModeRateCeiling_Id = ca.TravelModeRateCeilingPolicy.Select(e => e.Id.ToString()).ToArray(),
                    TravelModeRateCeiling_val = ca.TravelModeRateCeilingPolicy.Select(e => e.FullDetails).ToArray(),

                    DAEligibility_Id = ca.DAEligibilityPolicy.Select(e => e.Id.ToString()).ToArray(),
                    DAEligibility_val = ca.DAEligibilityPolicy.Select(e => e.FullDetails).ToArray(),

                    PolicyType_val = ca.PolicyType == null ? "" : ca.PolicyType.Id.ToString(),


                });
                }

                var Corp = db.BMSModuleTypePolicyAssignment.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { BMS, add_data, "", Auth, "", contno, JsonRequestBehavior.AllowGet });
            }
        }

        [HttpPost]
        public async Task<ActionResult> EditSave(BMSModuleTypePolicyAssignment c, int data, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {

                    var db_data = db.BMSModuleTypePolicyAssignment
                                    .Include(e => e.TravelEligibilityPolicy)
                                    .Include(e => e.HotelEligibilityPolicy)
                                    .Include(e => e.TravelModeEligibilityPolicy)
                                      .Include(e => e.TravelModeRateCeilingPolicy)
                                        .Include(e => e.DAEligibilityPolicy)
                                    .Where(e => e.Id == data).SingleOrDefault();
                    List<TravelEligibilityPolicy> lookupval = new List<TravelEligibilityPolicy>();
                    string Val1 = form["TravelEligibilityPolicyList"];

                    if (Val1 == null)
                    {
                        Msg.Add("Please select Travel Eligibility Policy!! ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    if (Val1 != null)
                    {
                        var ids = Utility.StringIdsToListIds(Val1);
                        foreach (var ca in ids)
                        {
                            var Lookup_val = db.TravelEligibilityPolicy.Find(ca);
                            lookupval.Add(Lookup_val);
                            db_data.TravelEligibilityPolicy = lookupval;
                        }
                    }
                    else
                    {
                        db_data.TravelEligibilityPolicy = null;
                    }

                    List<HotelEligibilityPolicy> lookupval1 = new List<HotelEligibilityPolicy>();
                    string Val2 = form["HotelEligibilityPolicyList"];

                    //if (Val2 == null)
                    //{
                    //    Msg.Add("Please select Hotel Eligibility Policy!! ");
                    //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //}

                    if (Val2 != null)
                    {
                        var ids = Utility.StringIdsToListIds(Val2);
                        foreach (var ca in ids)
                        {
                            var Lookup_val = db.HotelEligibilityPolicy.Find(ca);
                            lookupval1.Add(Lookup_val);

                        }
                        db_data.HotelEligibilityPolicy = lookupval1;
                    }
                    else
                    {
                        db_data.HotelEligibilityPolicy = null;
                    }
                    //travalmode
                    List<TravelModeEligibilityPolicy> lookupval3 = new List<TravelModeEligibilityPolicy>();
                    string Val3 = form["TravelModeEligibilityPolicyList"];

                    //if (Val3 == null)
                    //{
                    //    Msg.Add("Please select Travel Eligibility Policy!! ");
                    //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //}

                    if (Val3 != null)
                    {
                        var ids = Utility.StringIdsToListIds(Val3);
                        foreach (var ca in ids)
                        {
                            var Lookup_val = db.TravelModeEligibilityPolicy.Find(ca);
                            lookupval3.Add(Lookup_val);

                        }
                        db_data.TravelModeEligibilityPolicy = lookupval3;
                    }
                    else
                    {
                        db_data.TravelModeEligibilityPolicy = null;
                    }
                    //travalmode rate celling
                    List<TravelModeRateCeilingPolicy> lookupval4 = new List<TravelModeRateCeilingPolicy>();
                    string Val4 = form["TravelModeRateCeilingPolicyList"];

                    //if (Val4 == null)
                    //{
                    //    Msg.Add("Please select Travel Mode Rate Ceiling Policy!! ");
                    //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //}

                    if (Val4 != null)
                    {
                        var ids = Utility.StringIdsToListIds(Val4);
                        foreach (var ca in ids)
                        {
                            var Lookup_val = db.TravelModeRateCeilingPolicy.Find(ca);
                            lookupval4.Add(Lookup_val);
                        }
                        db_data.TravelModeRateCeilingPolicy = lookupval4;
                    }
                    else
                    {
                        db_data.TravelModeRateCeilingPolicy = null;
                    }
                    //DA
                    List<DAEligibilityPolicy> lookupval5 = new List<DAEligibilityPolicy>();
                    string Val5 = form["DAEligibilityPolicyList"];

                    //if (Val5 == null)
                    //{
                    //    Msg.Add("Please select DA Eligibility Policy!! ");
                    //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //}

                    if (Val5 != null)
                    {
                        var ids = Utility.StringIdsToListIds(Val5);
                        foreach (var ca in ids)
                        {
                            var Lookup_val = db.DAEligibilityPolicy.Find(ca);
                            lookupval5.Add(Lookup_val);

                        }
                        db_data.DAEligibilityPolicy = lookupval5;
                    }
                    else
                    {
                        db_data.DAEligibilityPolicy = null;
                    }
                    //var bmp = form["DIARYTypelist"] == "0" ? "" : form["DIARYTypelist"];
                    //var bmp1 = Convert.ToInt32(bmp);
                    //c.PolicyType = null; // Default to null

                    //if (!string.IsNullOrEmpty(bmp))
                    //{
                    //    var val = db.LookupValue.Find(int.Parse(bmp));
                    //    if (val != null)
                    //    {
                    //        c.PolicyType = val;
                    //    }
                    //}
                    var bmp = form["DIARYTypelist"] == "0" ? "" : form["DIARYTypelist"];
                    var bmp1 = Convert.ToInt32(bmp);
                    if (bmp != "" || bmp == null)
                    {
                        var val = db.LookupValue.Find(int.Parse(bmp));
                        c.PolicyType = val;
                    }
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;

                     
                    
                    if (Auth == false)
                    {
                        if (ModelState.IsValid)
                        {
                            try
                            {
                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    db.BMSModuleTypePolicyAssignment.Attach(db_data);
                                    db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    TempData["RowVersion"] = db_data.RowVersion;
                                    db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;

                                    BMSModuleTypePolicyAssignment blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.BMSModuleTypePolicyAssignment.Where(e => e.Id == data)
                                            .Include(e => e.PolicyType)
                          .Include(e => e.TravelEligibilityPolicy)
                        .Include(e => e.HotelEligibilityPolicy)
                        .Include(e => e.TravelModeEligibilityPolicy)
                        .Include(e => e.TravelModeRateCeilingPolicy)
                        .Include(e => e.DAEligibilityPolicy)
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
                                    
                                    var CurCorp = db.BMSModuleTypePolicyAssignment.Find(data);
                                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {

                                        BMSModuleTypePolicyAssignment corp = new BMSModuleTypePolicyAssignment()
                                        {

                                            Id = data,
                                            DBTrack = c.DBTrack,
                                            PolicyType_Id = bmp1,
                                            DAEligibilityPolicy = db_data.DAEligibilityPolicy,
                                            HotelEligibilityPolicy = db_data.HotelEligibilityPolicy,
                                            TravelEligibilityPolicy = db_data.TravelEligibilityPolicy,
                                            TravelModeEligibilityPolicy = db_data.TravelModeEligibilityPolicy,
                                            TravelModeRateCeilingPolicy = db_data.TravelModeRateCeilingPolicy
                                        };


                                        db.BMSModuleTypePolicyAssignment.Attach(corp);
                                        db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                                        db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];

                                    }


                                    using (var context = new DataBaseContext())
                                    {
                                        db.SaveChanges();
                                    }
                                    await db.SaveChangesAsync();
                                    ts.Complete();



                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = c.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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