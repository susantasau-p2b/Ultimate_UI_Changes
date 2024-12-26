using EssPortal.App_Start;
using EssPortal.Models;
using EssPortal.Security;
using P2b.Global;
using Payroll;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web;
using System.Web.Mvc;

namespace EssPortal.Controllers
{
    public class JourneyDetailsController : Controller
    {
        //
        // GET: /JourneyDetails/
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult partial_JourneyObjectDetails()
        {
            return View("~/Views/Shared/_JourneyObjectDetails.cshtml");
        }

        public ActionResult partial_MisExpenseObjectDetails()
        {
            return View("~/Views/Shared/_MisExpenseObjectDetails.cshtml");
        }

        public ActionResult getdays(String FromDate, string ToDate)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (FromDate != "" && ToDate != "")
                {
                    DateTime StartDate = DateTime.Parse(FromDate);
                    DateTime EndDate = DateTime.Parse(ToDate);

                    var data = (EndDate.Date - StartDate.Date).TotalDays + 1;

                    return Json(data);

                }
                return null;
            }
        }

       
       
        [HttpPost]
        public ActionResult EditSaveJourneyObj(JourneyObject data1, int data, FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();

                try
                {
                    var db_data = db.JourneyObject.Include(e=>e.TravelModeEligibilityPolicy)
                         .Where(a => a.Id == data).SingleOrDefault();

                    string TravelModeEligibilityPolicylistobjlist = form["TravelModeEligibilityPolicylistobj"] == "0" ? "" : form["TravelModeEligibilityPolicylistobj"];

                    if (TravelModeEligibilityPolicylistobjlist != null)
                    {
                        if (TravelModeEligibilityPolicylistobjlist != "")
                        {
                            int id = Convert.ToInt32(TravelModeEligibilityPolicylistobjlist);
                            var val = db.TravelModeEligibilityPolicy.Where(e => e.Id == id).SingleOrDefault();
                            db_data.TravelModeEligibilityPolicy =val;
                        }
                    }
                    else
                    {
                        db_data.TravelModeEligibilityPolicy = null;
                    }

                    data1.DBTrack = new DBTrack
                    {
                        CreatedBy = db_data.DBTrack.CreatedBy == null ? null : db_data.DBTrack.CreatedBy,
                        CreatedOn = db_data.DBTrack.CreatedOn == null ? null : db_data.DBTrack.CreatedOn,
                        Action = "M",
                        ModifiedBy = SessionManager.UserName,
                        ModifiedOn = DateTime.Now
                    };

                    db_data.JourneyDist = data1.JourneyDist;
                    db_data.FromDate = data1.FromDate;
                    db_data.ToDate = data1.ToDate;
                    db_data.PlaceFrom = data1.PlaceFrom;
                    db_data.PlaceTo = data1.PlaceTo;
                    db_data.TAClaimAmt = data1.TAClaimAmt;
                    db_data.Remark = data1.Remark;
                    db_data.TASettleAmt = data1.TASettleAmt;
                    db_data.TAElligibleAmt = data1.TAElligibleAmt;
                    db_data.DBTrack = data1.DBTrack;
                    db_data.TravelModeEligibilityPolicy = db_data.TravelModeEligibilityPolicy;

                    using (TransactionScope ts = new TransactionScope())
                    {
                        db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;
                        ts.Complete();
                    }

                    Msg.Add("  Data Updated successfully  ");
                    return Json(new Utility.JsonReturnClass { Id = db_data.Id, Val = db_data.PlaceFrom.ToString() + " " + db_data.PlaceTo.ToString() + " " + db_data.TAElligibleAmt.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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
        public ActionResult GetTravel_Hotel_Booking()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.TravelHotelBooking.Include(e => e.HotelEligibilityPolicy).ToList();              
                var list1 = db.LTCSettlementClaim.Where(e => e.Travel_Hotel_Booking.Count() > 0).ToList().SelectMany(e => e.Travel_Hotel_Booking);              
                var list2 = db.JourneyDetails.Where(e => e.Travel_Hotel_Booking.Count() > 0).ToList().SelectMany(e => e.Travel_Hotel_Booking);
                var list3 = fall.Except(list1).Except(list2);
                var r = (from ca in list3 select new { srno = ca.Id, lookupvalue = ca.HotelName + ", StartDate :" + ca.StartDate.Value.ToString("dd/MM/yyyy") }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult CreateMisExpenseObj(MisExpenseObject c, FormCollection form, string journeystart, string journeyend) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                // int TravelModeEligibilityPolicylist = form["TravelModeEligibilityPolicylistobj"] == "0" ? 0 : Convert.ToInt32(form["TravelModeEligibilityPolicylistobj"]);

                DateTime jStart = Convert.ToDateTime(journeystart);
                DateTime jEnd = Convert.ToDateTime(journeyend);
                List<String> Msg = new List<String>();
                try
                {

                    if (c.MisExpenseDate > jEnd.Date)
                    {
                        //Msg.Add("Date between journey date");
                        //return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        return this.Json(new Object[] { "Date between journey date" }, JsonRequestBehavior.AllowGet);
                    }
                    if (c.MisExpenseDate < jStart.Date)
                    {
                        //Msg.Add("Date between journey date");
                        //return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        return this.Json(new Object[] { "Date between journey date" }, JsonRequestBehavior.AllowGet);
                    }
                    if (c.MisExpenseSettleAmt > c.MisExpenseClaimAmt)
                    {
                        //Msg.Add("Settlement amount should not be greater than claim amount");
                        //return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        return this.Json(new Object[] { "Settlement amount should not be greater than claim amount" }, JsonRequestBehavior.AllowGet);
                    }
                    //if (TravelModeEligibilityPolicylist != null)
                    //{
                    //    if (TravelModeEligibilityPolicylist != "")
                    //    {
                    //        int id = Convert.ToInt32(TravelModeEligibilityPolicylist);
                    //        var val = db.TravelModeEligibilityPolicy.Where(e => e.Id == id).SingleOrDefault();
                    //        c.TravelModeEligibilityPolicy = val;
                    //    }
                    //}




                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {


                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            MisExpenseObject MisExpenseObjectdetails = new MisExpenseObject()
                            {

                                MisExpenseDate = c.MisExpenseDate,

                                Remark = c.Remark,
                                MisExpenseClaimAmt = c.MisExpenseClaimAmt,
                                MisExpenseElligibleAmt = c.MisExpenseElligibleAmt,
                                MisExpenseSettleAmt = c.MisExpenseSettleAmt,

                                DBTrack = c.DBTrack
                            };

                            db.MisExpenseObject.Add(MisExpenseObjectdetails);
                            db.SaveChanges();
                            //var journyobjdata = db.JourneyObject.Where(e => e.Id == JourneyObjectdetails.Id).Select(e => new
                            //{
                            //    Id = e.Id,
                            //    fulldetails = "JourneyDistance:" + e.JourneyDist + ", " + "PlaceFrom:" + e.PlaceFrom + ", " + "PlaceTo:" + e.PlaceTo + ", " + "FromDate:" + e.FromDate.Value.ToString("MM/dd/yyyy hh:mm") + ", " + "ToDate:" + e.ToDate.Value.ToString("MM/dd/yyyy hh:mm")
                            //}).SingleOrDefault();
                            ts.Complete();
                            //Msg.Add("Data Saved Successfully.");29082023
                            //return Json(new Utility.JsonReturnClass { Id = MisExpenseObjectdetails.Id, Val = MisExpenseObjectdetails.MisExpenseDate.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            return this.Json(new Object[] { MisExpenseObjectdetails.Id, MisExpenseObjectdetails.MisExpenseDate.ToString(), "Data Saved SucessFully" }, JsonRequestBehavior.AllowGet);
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
                        //Msg.Add("Code Already Exists.");29082023
                        return this.Json(new Object[] { "Code Already Exists." }, JsonRequestBehavior.AllowGet);
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
        public ActionResult Getduplicatejourney(string Id, string data2, string data3, string Type)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var empid = Convert.ToInt32(Id);
                    DateTime Jstart = Convert.ToDateTime(data2);
                    DateTime Jend = Convert.ToDateTime(data3);
                    Company OComany = db.Company.FirstOrDefault();
                    string ratekm = "";

                    if (data2 != "" && data2 != null)
                    {

                        if (Type.ToUpper() == "DIARY")
                        {
                            var EmpLTCStruct_Details = db.EmployeePayroll
                            .Include(e => e.DIARYSettlementClaim)
                             .Include(e => e.DIARYSettlementClaim.Select(x => x.JourneyDetails))
                           //.Include(e => e.JourneyDetails)
                           .Where(e => e.Employee.Id == empid).FirstOrDefault();

                            var travelmodlist = EmpLTCStruct_Details.DIARYSettlementClaim.Select(r => r.JourneyDetails)
                                .ToList();


                            if (travelmodlist.Count() > 0)
                            {
                                if (travelmodlist.Where(e => e.JourneyStart.Value.Date >= Jstart.Date && e.JourneyStart.Value.Date <= Jend.Date).Count() != 0 ||
                                    travelmodlist.Where(e => e.JourneyEnd.Value.Date >= Jstart.Date && e.JourneyEnd.Value.Date <= Jend.Date).Count() != 0)
                                {
                                    ratekm = "DuplicateEntry";

                                }
                                if (travelmodlist.Where(e => e.JourneyStart.Value.Date <= Jstart.Date && e.JourneyEnd.Value.Date >= Jend.Date).Count() != 0)
                                {
                                    ratekm = "DuplicateEntry";
                                }

                            }
                        }
                        else if (Type.ToUpper() == "TADA")
                        {
                            var EmpLTCStruct_Details = db.EmployeePayroll
                            .Include(e => e.TADASettlementClaim)
                             .Include(e => e.TADASettlementClaim.Select(x => x.JourneyDetails))
                           //.Include(e => e.JourneyDetails)
                           .Where(e => e.Employee.Id == empid).FirstOrDefault();

                            var travelmodlist = EmpLTCStruct_Details.TADASettlementClaim.Select(r => r.JourneyDetails)
                                .ToList();


                            if (travelmodlist.Count() > 0)
                            {
                                if (travelmodlist.Where(e => e.JourneyStart.Value.Date >= Jstart.Date && e.JourneyStart.Value.Date <= Jend.Date).Count() != 0 ||
                                    travelmodlist.Where(e => e.JourneyEnd.Value.Date >= Jstart.Date && e.JourneyEnd.Value.Date <= Jend.Date).Count() != 0)
                                {
                                    ratekm = "DuplicateEntry";

                                }
                                if (travelmodlist.Where(e => e.JourneyStart.Value.Date <= Jstart.Date && e.JourneyEnd.Value.Date >= Jend.Date).Count() != 0)
                                {
                                    ratekm = "DuplicateEntry";
                                }

                            }
                        }



                    }

                    return Json(new Object[] { ratekm, JsonRequestBehavior.AllowGet });

                }
                catch (Exception ex)
                {

                    string content = string.Empty;
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
                    content = "LogFile Created";
                    //return View("~/Views/Payroll/MainViews/PFECRSummaryR/Index.cshtml");
                    return Json(new Object[] { content, "", JsonRequestBehavior.AllowGet });
                }
            }
        }
        public ActionResult GetMisExpenseObjDetails()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.MisExpenseObject.ToList();                                             
                var list1 = db.JourneyDetails.Where(e => e.MisExpenseObject.Count() > 0).ToList().SelectMany(e => e.MisExpenseObject);
                var list2 = fall.Except(list1);
                var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = "Misexpclaim:" + ca.MisExpenseClaimAmt + ", " + "Misexpelg:" + ca.MisExpenseElligibleAmt + ", " + "MisexpSett:" + ca.MisExpenseSettleAmt + ", " + "MisexpDate:" + ca.MisExpenseDate.Value.ToString("MM/dd/yyyy hh:mm") }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }
        public class returnEditClassm
        {
            public String Id { get; set; }
            public String MisExpenseDate { get; set; }
            public String Remark { get; set; }
            public String MisExpenseClaimAmt { get; set; }
            public String MisExpenseElligibleAmt { get; set; }
            public String MisExpenseSettleAmt { get; set; }

            public String PlaceFromId { get; set; }
            public String PlaceToId { get; set; }

        }

        public ActionResult getMisExpenseobjdata(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var add = db.MisExpenseObject
                                      .Where(e => e.Id == data).SingleOrDefault();


                List<returnEditClassm> oreturnEditClassm = new List<returnEditClassm>();

                oreturnEditClassm.Add(new returnEditClassm
                {
                    Id = add.Id.ToString(),
                    MisExpenseDate = add.MisExpenseDate.ToString(),
                    Remark = add.Remark.ToString(),
                    MisExpenseClaimAmt = add.MisExpenseClaimAmt.ToString(),
                    MisExpenseElligibleAmt = add.MisExpenseElligibleAmt.ToString(),
                    MisExpenseSettleAmt = add.MisExpenseSettleAmt.ToString(),


                });

                return Json(oreturnEditClassm, JsonRequestBehavior.AllowGet);


            }
        }

        [HttpPost]
        public ActionResult EditSaveMisExpenseObj(MisExpenseObject E, int data, FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {


                    var MisExpenseObjects = db.MisExpenseObject
                         .Where(a => a.Id == data).SingleOrDefault();

                    E.DBTrack = new DBTrack
                    {
                        CreatedBy = MisExpenseObjects.DBTrack.CreatedBy == null ? null : MisExpenseObjects.DBTrack.CreatedBy,
                        CreatedOn = MisExpenseObjects.DBTrack.CreatedOn == null ? null : MisExpenseObjects.DBTrack.CreatedOn,
                        Action = "M",
                        ModifiedBy = SessionManager.UserName,
                        ModifiedOn = DateTime.Now
                    };
                    MisExpenseObjects.Id = data;
                    MisExpenseObjects.MisExpenseDate = E.MisExpenseDate;
                    MisExpenseObjects.MisExpenseClaimAmt = E.MisExpenseClaimAmt;
                    MisExpenseObjects.MisExpenseElligibleAmt = E.MisExpenseElligibleAmt;
                    MisExpenseObjects.MisExpenseSettleAmt = E.MisExpenseSettleAmt;
                    MisExpenseObjects.Remark = E.Remark;
                    MisExpenseObjects.DBTrack = E.DBTrack;

                    using (TransactionScope ts = new TransactionScope())
                    {
                        db.Entry(MisExpenseObjects).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        db.Entry(MisExpenseObjects).State = System.Data.Entity.EntityState.Detached;
                        ts.Complete();
                    }
                    //Msg.Add("  Data Saved successfully  ");
                    //return Json(new Utility.JsonReturnClass { Id = db_data.Id, Val = db_data.MisExpenseDate.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    Msg.Add("  Data Updated successfully  ");
                    return Json(new Utility.JsonReturnClass { Id = MisExpenseObjects.Id, Val = MisExpenseObjects.MisExpenseDate.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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

        public ActionResult GetCode(string Id, string data2, string data3, string data4)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var empid = Convert.ToInt32(Id);
                    var Travelmodeid = Convert.ToInt32(data2);
                    var DiaryTypeid = Convert.ToInt32(data4);
                    Company OComany = db.Company.FirstOrDefault();
                    string ratekm = "";
                    bool DistAppl = false;
                    bool ClaimAppl = false;
                    bool Readonly = true;
                    if (data3 == "")
                    {
                        data3 = "0";
                    }
                    if (data2 != "" && data2 != null)
                    {
                        double dist = Convert.ToDouble(data3);
                        int Travelmodid = Convert.ToInt32(data2);

                        var EmpLTCStruct_Details = db.EmployeeLTCStruct
                            .Include(e => e.EmployeeLTCStructDetails)
                            .Include(e => e.EmployeeLTCStructDetails.Select(r => r.LTCFormula))
                            .Include(e => e.EmployeeLTCStructDetails.Select(r => r.LTCFormula.TravelModeRateCeilingPolicy))
                            .Include(e => e.EmployeeLTCStructDetails.Select(r => r.LTCFormula.TravelModeRateCeilingPolicy.Select(z => z.DistanceRange)))
                            .Include(e => e.EmployeeLTCStructDetails.Select(r => r.LTCFormula.TravelModeRateCeilingPolicy.Select(x => x.TravelModeEligibilityPolicy)))
                             .Include(e => e.EmployeeLTCStructDetails.Select(r => r.LTCFormula.TravelModeRateCeilingPolicy.Select(x => x.TravelModeEligibilityPolicy.TravelMode)))
                            .Where(e => e.EmployeePayroll.Employee_Id == empid && e.EndDate == null).FirstOrDefault();

                        var DiaryTypePolicyAss = db.BMSModuleTypePolicyAssignment.Include(e => e.HotelEligibilityPolicy).Include(e => e.TravelEligibilityPolicy).Include(e => e.PolicyType)
                            .Include(e => e.TravelModeEligibilityPolicy).Include(e => e.TravelModeRateCeilingPolicy)
                            .Where(e => e.PolicyType_Id == DiaryTypeid).OrderByDescending(e => e.Id).FirstOrDefault();
                        //  .Select(t => t.TravelModeRateCeilingPolicy.Where(r => r.TravelModeEligibilityPolicy_Id == Travelmodeid).FirstOrDefault());

                        var travelmodlist = EmpLTCStruct_Details.EmployeeLTCStructDetails.Select(r => r.LTCFormula)
                            .ToList();

                        if (travelmodlist.Count() > 0 && DiaryTypePolicyAss != null)
                        {
                            foreach (var item in travelmodlist.Where(e => e.TravelModeRateCeilingPolicy.Count() > 0).Select(x => x.TravelModeRateCeilingPolicy))
                            {

                                var dd = item.Where(e => e.TravelModeEligibilityPolicy_Id == Travelmodid).FirstOrDefault();
                                var diarytypepolicy = DiaryTypePolicyAss.TravelModeRateCeilingPolicy.Where(e => e.TravelModeEligibilityPolicy_Id == Travelmodid).FirstOrDefault();
                                if (dd != null && diarytypepolicy != null)
                                {
                                    if (DiaryTypePolicyAss.PolicyType.LookupVal.ToUpper() == "CASH")
                                    {
                                        DistAppl = false;
                                        ClaimAppl = true;
                                    }
                                    else if (DiaryTypePolicyAss.PolicyType.LookupVal.ToUpper().Contains("OTHER"))
                                    {
                                        if (dd.TravelModeEligibilityPolicy.TravelMode.LookupVal.ToUpper() == "OWN TWO WHEELER")
                                        {
                                            DistAppl = true;
                                        }
                                        if (dd.TravelModeEligibilityPolicy.TravelMode.LookupVal.ToUpper() == "OWN FOUR WHEELER")
                                        {
                                            DistAppl = true;
                                        }
                                        var ss = dd.DistanceRange.Where(e => e.WagesFrom <= dist && e.WagesTo >= dist).FirstOrDefault();
                                        if (ss != null)
                                        {
                                            ratekm = ss.Amount.ToString();
                                        }

                                    }
                                    else
                                    { ClaimAppl = false; }


                                }

                            }
                        }


                    }

                    if (DistAppl == true)
                    {
                        Readonly = false;
                    }
                    return Json(new object[] { Readonly, ratekm, ClaimAppl, JsonRequestBehavior.AllowGet });

                }
                catch (Exception ex)
                {

                    string content = string.Empty;
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
                    content = "LogFile Created";
                    //return View("~/Views/Payroll/MainViews/PFECRSummaryR/Index.cshtml");
                    return Json(new Object[] { content, "", JsonRequestBehavior.AllowGet });
                }
            }
        }



        public class GetTadasettlamt //childgrid
        {

            public double DAElligibleAmt { get; set; }
            public double DAClaimAmt { get; set; }
            public double DASettleAmt { get; set; }
            public double TAElligibleAmt { get; set; }
            public double TAClaimAmt { get; set; }
            public double TASettleAmt { get; set; }
            public double HotelElligibleAmt { get; set; }
            public double HotelClaimAmt { get; set; }
            public double HotelSettleAmt { get; set; }
            public double MisExpenseElligibleAmt { get; set; }
            public double MisExpenseClaimAmt { get; set; }
            public double MisExpenseSettleAmt { get; set; }
            public double TotalClaimAmt { get; set; }
        }
        [HttpPost]
        public ActionResult GetTADAAmount(FormCollection form, int data, string data1, string data2)
        {

            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string journewdetailslist = form["JourneyObjectlist-edit"] == "0" ? "" : form["JourneyObjectlist-edit"];
                    string tadaadvanceclaimlist = form["TADAAdvanceClaimList"] == "0" ? "" : form["TADAAdvanceClaimList"];
                    string travelhotelbookinglist = form["Travel_Hotel_BookingList"] == "0" ? "" : form["Travel_Hotel_BookingList"];
                    string TADATypelist = data2;

                    string JourneyObjectlistedit = form["JourneyObjectlist-edit"] == "0" ? "" : form["JourneyObjectlist-edit"];
                    string MisExpenseObjectlistedit = form["MisExpenseObjectlist-edit"] == "0" ? "" : form["MisExpenseObjectlist-edit"];


                    //string salcode = "";
                    //if (TADATypelist != null && TADATypelist != "")
                    //{
                    //    var val = db.LookupValue.Find(int.Parse(TADATypelist));
                    //    if (val.LookupValData != null && val.LookupValData != "")
                    //    {
                    //        salcode = val.LookupValData.ToString();
                    //    }

                    //} 

                    List<BMSModuleProcess.ReturnData_GetSettleAmt> returndata = new List<BMSModuleProcess.ReturnData_GetSettleAmt>();
                    returndata = BMSModuleProcess.Calculate_TADASettlementAmount(data, JourneyObjectlistedit, travelhotelbookinglist, MisExpenseObjectlistedit, data1, int.Parse(TADATypelist));

                    return Json(returndata, JsonRequestBehavior.AllowGet);

                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }


        public ActionResult getStartDays(String PayMonth)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                DateTime firstDay = Convert.ToDateTime("01/" + PayMonth);
                DateTime lastDay = firstDay.AddMonths(1).AddDays(-1);
                return Json(new { firstDay, lastDay });

            }
        }

        public ActionResult getTotalHours(String StartDate, string EndDate)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (StartDate != "" && EndDate != "")
                {
                    DateTime StartDateN = DateTime.Parse(StartDate);
                    DateTime EndDateN = DateTime.Parse(EndDate);


                    TimeSpan diffT = EndDateN.TimeOfDay - StartDateN.TimeOfDay;
                    // TimeSpan  tothrs = Convert.ToDateTime(diffT.ToString());

                    return Json(diffT.ToString().Remove(5, 3));

                }
                return null;
            }
        }


 

        public ActionResult getdaysValidation(string FromDate, string ToDate, string MonthStart, string MonthEnd)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (MonthStart != "" && MonthEnd != "")
                {
                    DateTime StartDate = DateTime.Parse(FromDate);
                    DateTime EndDate = DateTime.Now;
                    DateTime MStartDate = DateTime.Parse(MonthStart);
                    DateTime MEndDate = DateTime.Parse(MonthEnd);

                    string msg = "";
                    if (ToDate != "")
                    {
                        EndDate = DateTime.Parse(ToDate);
                        if (EndDate >= MStartDate && EndDate <= MEndDate) { }
                        else
                        { msg = "You have to add requisition for the selected month only."; return Json(msg); }
                    }
                    else
                    {
                        if (StartDate >= MStartDate && StartDate <= MEndDate) { }
                        else
                        { msg = "You have to add requisition for the selected month only."; return Json(msg); }

                    }

                }
                return null;
            }
        }

        public ActionResult Create(JourneyDetails c, FormCollection form, string EmpId, string data1) //Create submit
        {       
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    int Emp = Convert.ToInt32(EmpId);

                    //int Emp = form["EmpLvnereq_Id"] == "0" ? 0 : Convert.ToInt32(form["EmpLvnereq_Id"]);
                    //  string Emp = form["employee-table"] == "0" ? "" : form["employee-table"];
                    //string Country = form["CountryList"] == "0" ? "" : form["CountryList"];
                    //string State = form["StateList"] == "0" ? "" : form["StateList"];
                    //string City = form["CityList"] == "0" ? "" : form["CityList"];
                    // string Addrs = form["Addresslist"] == "0" ? "" : form["Addresslist"];
                    // string ContactDetails = form["ContactDetailslist"] == "0" ? "" : form["ContactDetailslist"];
                    //     string benifit = form["BenefitTypelist"] == "0" ? "" : form["BenefitTypelist"];
                    string JourneyObjectlist = form["JourneyObjectlist-edit"] == "0" ? "" : form["JourneyObjectlist-edit"];
                    string FamilyDetailslist = form["FamilyDetailslist-edit"] == "0" ? "" : form["FamilyDetailslist-edit"];
                    string EmpDocumentslist = form["EmpDocumentslist-edit"] == "0" ? "" : form["EmpDocumentslist-edit"];
                    string trhotelbookinglist = form["Travel_Hotel_BookingList"] == "0" ? "" : form["Travel_Hotel_BookingList"];
                    string MisExpenseObjeclist = form["MisExpenseObjectlist-edit"] == "0" ? "" : form["MisExpenseObjectlist-edit"];
                    int TravelModeEligibilityPolicylist = form["TravelModeEligibilityPolicylist"] == "0" ? 0 : Convert.ToInt32(form["TravelModeEligibilityPolicylist"]);

                    string IsFamIncl = form["IsFamIncl"] == "0" ? "" : form["IsFamIncl"];
                    if (IsFamIncl != "")
                    {
                        c.IsFamilyIncl = Convert.ToBoolean(IsFamIncl);
                    }

                    //if (nominee != null)
                    //{
                    //    if (nominee != "")
                    //    {
                    //        var nomineeId = Utility.StringIdsToListIds(nominee);
                    //        var FamilyDetailslist = new List<FamilyDetails>();
                    //        foreach (var item in nomineeId)
                    //        {
                    //            int FamilyListid = Convert.ToInt32(item);
                    //            var vals = db.FamilyDetails.Where(e => e.MemberName.Id == FamilyListid).SingleOrDefault();
                    //            if (vals != null)
                    //            {
                    //                FamilyDetailslist.Add(vals);
                    //            }
                    //        }
                    //        c.FamilyDetails = FamilyDetailslist;
                    //    }
                    //}

                    if (c.JourneyEnd < c.JourneyStart)
                    {
                        Msg.Add("JourneyEnd date Should Not be Less than JourneyStart Date");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }

                   int Emp_Id = Convert.ToInt32(Emp);
                   List<DAObject> newDA = new List<DAObject>();
                   var EmpLTCStruct_Details = db.EmployeeLTCStruct
                  .Include(e => e.EmployeeLTCStructDetails)
                  .Include(e => e.EmployeeLTCStructDetails.Select(x => x.PolicyName))
                  .Include(e => e.EmployeeLTCStructDetails.Select(r => r.LTCFormula))
                  .Include(e => e.EmployeeLTCStructDetails.Select(r => r.LTCFormula.DAEligibilityPolicy))
                  .Include(e => e.EmployeeLTCStructDetails.Select(r => r.LTCFormula.DAEligibilityPolicy.Select(z => z.SlabDependRule)))
                   .Include(e => e.EmployeeLTCStructDetails.Select(r => r.LTCFormula.DAEligibilityPolicy.Select(z => z.SlabDependRule.WageRange)))
                  .Where(e => e.EmployeePayroll.Employee_Id == Emp_Id && e.EndDate == null).FirstOrDefault();
                   LTCFormula travelmodlist = EmpLTCStruct_Details.EmployeeLTCStructDetails.Where(e => e.PolicyName.LookupVal.ToUpper().ToString() == data1.ToUpper().ToString()).Select(e => e.LTCFormula).FirstOrDefault();

                   if (JourneyObjectlist != null)
                   {
                       var ids = Utility.StringIdsToListIds(JourneyObjectlist);
                       var JourneyObjectlisttada = new List<JourneyObject>();


                       var totdatelist = db.JourneyObject.Where(e => ids.Contains(e.Id)).Select(x => x.FromDate).Distinct().ToList();
                       var totdatelistd = totdatelist.Select(e => e.Value.ToShortDateString()).Distinct().ToList();
                       foreach (var item in totdatelistd)
                       {
                           DateTime mdate = Convert.ToDateTime(item);
                           JourneyObjectlisttada = db.JourneyObject.Where(e => ids.Contains(e.Id) && DbFunctions.TruncateTime(e.FromDate) == mdate).ToList();
                           double hrs = 0;
                           double min = 0;
                           foreach (var t in JourneyObjectlisttada)
                           {
                               DateTime a = Convert.ToDateTime(t.FromDate);
                               DateTime b = Convert.ToDateTime(t.ToDate);
                               TimeSpan diffT = b.TimeOfDay - a.TimeOfDay;
                               var tm = diffT;
                               string tmin = Convert.ToString(tm);
                               //hrs = Convert.ToInt32(tmin.Split(':')[0]);
                               //min = Convert.ToInt32(tmin.Split(':')[1]);

                               hrs = hrs + Convert.ToInt32(tmin.Split(':')[0]);
                               min = min + Convert.ToInt32(tmin.Split(':')[1]);

                           }
                           if (min != 0)
                           {
                               hrs = hrs + (min / 60);
                           }
                           // Da calc start
                           double DAElgAmt = 0;
                           double DAClaimAmt = 0;
                           double DASetAmt = 0;
                           if (travelmodlist != null)
                           {
                               var dd = travelmodlist.DAEligibilityPolicy.FirstOrDefault();
                               if (dd != null)
                               {
                                   foreach (var ca in dd.SlabDependRule.WageRange)
                                   {
                                       if (hrs > ca.WageFrom && hrs <= ca.WageTo)
                                       {

                                           DAElgAmt = DAElgAmt + (hrs * ca.Percentage / 100) + ca.Amount;
                                           DAClaimAmt = DAClaimAmt + (hrs * ca.Percentage / 100) + ca.Amount;
                                           DASetAmt = DASetAmt + (hrs * ca.Percentage / 100) + ca.Amount;
                                       }
                                   }
                               }


                           }
                           // Da oblect data save
                           c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                           DAObject HNS = new DAObject()
                           {
                               DADate = mdate,
                               DAElligibleAmt = DAElgAmt,
                               DAClaimAmt = DAClaimAmt,
                               DASettleAmt = DASetAmt,
                               DBTrack = c.DBTrack
                           };
                           newDA.Add(HNS);

                           // Da oblect data save
                           // Da calc end
                       }

                   }

                    if (JourneyObjectlist != null)
                    {
                        if (JourneyObjectlist != "")
                        {
                            var JourneyObjId = Utility.StringIdsToListIds(JourneyObjectlist);
                            var Journeyobjdatalist = new List<JourneyObject>();
                            foreach (var item in JourneyObjId)
                            {
                                int Journeyobjid = Convert.ToInt32(item);
                                var vals = db.JourneyObject.Where(e => e.Id == Journeyobjid).SingleOrDefault();
                                if (vals != null)
                                {
                                    Journeyobjdatalist.Add(vals);
                                }
                            }
                            c.JourneyObject = Journeyobjdatalist;
                        }
                    }

                    if (MisExpenseObjeclist != null)
                    {
                        if (MisExpenseObjeclist != "")
                        {
                            var MisExpenseObjId = Utility.StringIdsToListIds(MisExpenseObjeclist);
                            var MisExpenseobjdatalist = new List<MisExpenseObject>();
                            foreach (var item in MisExpenseObjId)
                            {
                                int JMisExpenseobjid = Convert.ToInt32(item);
                                var vals = db.MisExpenseObject.Where(e => e.Id == JMisExpenseobjid).SingleOrDefault();
                                if (vals != null)
                                {
                                    MisExpenseobjdatalist.Add(vals);
                                }
                            }
                            c.MisExpenseObject = MisExpenseobjdatalist;
                        }
                    }

                    if (trhotelbookinglist != null)
                    {
                        var ids = Utility.StringIdsToListIds(trhotelbookinglist);
                        var travelhotelbookinglist = new List<TravelHotelBooking>();
                        foreach (var item in ids)
                        {

                            int hotellistid = Convert.ToInt32(item);
                            var val = db.TravelHotelBooking.Include(e => e.HotelEligibilityPolicy).Where(e => e.Id == hotellistid).SingleOrDefault();
                            if (val != null)
                            {
                                travelhotelbookinglist.Add(val);
                            }
                        }
                        c.Travel_Hotel_Booking = travelhotelbookinglist;
                    }

                    if (FamilyDetailslist != null)
                    {
                        if (FamilyDetailslist != "")
                        {
                            var nomineeId = Utility.StringIdsToListIds(FamilyDetailslist);
                            var FamilyDetailslistData = new List<FamilyDetails>();
                            foreach (var item in nomineeId)
                            {
                                int FamilyListid = Convert.ToInt32(item);
                                var vals = db.FamilyDetails.Where(e => e.MemberName.Id == FamilyListid).SingleOrDefault();
                                if (vals != null)
                                {
                                    FamilyDetailslistData.Add(vals);
                                }
                            }
                            c.FamilyDetails = FamilyDetailslistData;
                        }
                    }


                    if (EmpDocumentslist != null)
                    {
                        if (EmpDocumentslist != "")
                        {
                            var empdoclistId = Utility.StringIdsToListIds(EmpDocumentslist);
                            var empdoculist = new List<EmployeeDocuments>();
                            foreach (var item in empdoclistId)
                            {
                                int EmpdocListid = Convert.ToInt32(item);
                                var vals = db.EmployeeDocuments.Where(e => e.Id == EmpdocListid).SingleOrDefault();
                                if (vals != null)
                                {
                                    empdoculist.Add(vals);
                                }
                            }
                            c.EmpDocuments = empdoculist;
                        }
                    }
                    //if (State != null)
                    //{
                    //    if (State != "")
                    //    {
                    //        State val = db.State.Find(Convert.ToInt32(State));
                    //        c.State = val;
                    //    }
                    //}
                    //if (City != null)
                    //{
                    //    if (City != "")
                    //    {
                    //        City val = db.City.Find(int.Parse(City));
                    //        c.City = val;
                    //    }
                    //}

                    //if (Addrs != null)
                    //{
                    //    if (Addrs != "")
                    //    {
                    //        int AddId = Convert.ToInt32(Addrs);
                    //        var val = db.HotelEligibilityPolicy
                    //                            .Where(e => e.Id == AddId).SingleOrDefault();
                    //        c.HotelEligibilityPolicy = val;
                    //    }
                    //}

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

                    //EmployeePayroll EmpData;
                    //if (Emp != 0)
                    //{
                    //    int em = Convert.ToInt32(Emp);
                    //    // EmpData = db.Employee.Include(q => q.FamilyDetails).Where(q => q.Id == em).SingleOrDefault();
                    //    EmpData = db.EmployeePayroll.Include(q => q.HotelBookingRequest).Where(e => e.Employee.Id == em).SingleOrDefault();

                    //}
                    //else
                    //{
                    //    Msg.Add("  Kindly select employee  ");
                    //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //}
                    //Employee OEmployee = null;

                    //OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct)
                    //                       .Where(r => r.Id == Emp).SingleOrDefault();

                    //if (ModelState.IsValid)
                    //{
                        using (TransactionScope ts = new TransactionScope())
                        {


                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            List<JourneyDetails> HotelBookingRequest = new List<JourneyDetails>();
                            JourneyDetails bns = new JourneyDetails()
                            {
                                //  HotelEligibilityPolicy = c.HotelEligibilityPolicy,
                                JourneyEnd = c.JourneyEnd,
                                JourneyStart = c.JourneyStart,
                                NoOfDays = c.NoOfDays,
                                TotFamilyMembers = c.TotFamilyMembers,
                                JourneyObject = c.JourneyObject,
                                FamilyDetails = c.FamilyDetails,
                                EmpDocuments = c.EmpDocuments,
                                IsFamilyIncl = c.IsFamilyIncl,
                                Remark = c.Remark,
                                DBTrack = c.DBTrack,
                                Travel_Hotel_Booking = c.Travel_Hotel_Booking,
                                MisExpenseObject = c.MisExpenseObject,
                                DAElligibleAmt = c.DAElligibleAmt,
                                DAClaimAmt = c.DAClaimAmt,
                                DASettleAmt = c.DASettleAmt,
                                TAElligibleAmt = c.TAElligibleAmt,
                                TAClaimAmt = c.TAClaimAmt,
                                TASettleAmt = c.TASettleAmt,
                                HotelElligibleAmt = c.HotelElligibleAmt,
                                HotelClaimAmt = c.HotelClaimAmt,
                                HotelSettleAmt = c.HotelSettleAmt,
                                MisExpenseClaimAmt = c.MisExpenseClaimAmt,
                                MisExpenseElligibleAmt = c.MisExpenseElligibleAmt,
                                MisExpenseSettleAmt = c.MisExpenseSettleAmt,
                                TravelModeEligibilityPolicy = db.TravelModeEligibilityPolicy.Find(TravelModeEligibilityPolicylist),
                            };

                            try
                            {
                                db.JourneyDetails.Add(bns);
                                db.SaveChanges();

                                bns.DAObject = newDA;
                                db.JourneyDetails.Attach(bns);
                                db.Entry(bns).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(bns).State = System.Data.Entity.EntityState.Detached;

                                //List<BenefitNominees>newben=new List<BenefitNominees>();
                                //newben.Add(bns);

                                //DBTrackFile.DBTrackSave("Core/P2b.Global", "C", BenefitNominees, null, "BenefitNominees", null);

                                ts.Complete();
                                //Msg.Add("Data Saved Successfully.");
                                //return Json(new Utility.JsonReturnClass { Id = bns.Id, Val = bns.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                return this.Json(new Object[] { bns.Id, bns.FullDetails, "Data Saved SucessFully" }, JsonRequestBehavior.AllowGet);
                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = c.Id });
                            }
                            catch (DataException /* dex */)
                            {
                                //   return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
                                Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
                                return this.Json(new Object[] { 0, "", Msg }, JsonRequestBehavior.AllowGet);

                            }
                        }
                    //}
                    //else
                    //{
                    //    StringBuilder sb = new StringBuilder("");
                    //    foreach (ModelState modelState in ModelState.Values)
                    //    {
                    //        foreach (ModelError error in modelState.Errors)
                    //        {
                    //            sb.Append(error.ErrorMessage);
                    //            sb.Append("." + "\n");
                    //        }
                    //    }
                    //    var errorMsg = sb.ToString();
                    //    Msg.Add(errorMsg);
                    //    return this.Json(new Object[] { 0, "", Msg }, JsonRequestBehavior.AllowGet);
                    //}
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
                    return this.Json(new Object[] { 0, "", Msg }, JsonRequestBehavior.AllowGet);
                }
            }
        }


        public static TimeSpan _returnTimeSpan(DateTime? oDateTime)
        {
            return new TimeSpan(oDateTime.Value.Hour, oDateTime.Value.Minute, 0);
        }

        public class returnEditClass
        {
            public int Id { get; set; }
            public int TravelModeEligibilityPolicy_Id { get; set; }
            public String JourneyDist { get; set; }
            public String PlaceFrom { get; set; }
            public String PlaceTo { get; set; }
            public String Remark { get; set; }
            public String TAClaimAmt { get; set; }
            public String TAElligibleAmt { get; set; }
            public String TASettleAmt { get; set; }
            public String FromDate { get; set; }
            public String ToDate { get; set; }
            public String PlaceFromId { get; set; }
            public String PlaceToId { get; set; }
        }

        public ActionResult GetTravelMode(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                int TID = Convert.ToInt32(data2);
                var qurey = db.TravelModeEligibilityPolicy.Where(e => e.Id == TID).ToList();
                var selected = (Object)null;
                if (data2 != "" && data != "0" && data2 != "0")
                {
                    selected = Convert.ToInt32(data2);
                }

                SelectList s = new SelectList(qurey, "Id", "TA_TM_Elligibilty_Code", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }           
        }
        public ActionResult getJourneyobjdata(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {


                var k = db.JourneyObject.Include(e => e.TravelModeEligibilityPolicy)
                                   .Where(e => e.Id == data).SingleOrDefault();

                string PlaceFromId = db.Location.Include(e => e.LocationObj).Where(e => e.LocationObj.LocCode == k.PlaceFrom.Substring(7, 3)).FirstOrDefault().Id.ToString();
                string PlaceToId = db.Location.Include(e => e.LocationObj).Where(e => e.LocationObj.LocCode == k.PlaceTo.Substring(7, 3)).FirstOrDefault().Id.ToString();

                List<returnEditClass> oreturnEditClass = new List<returnEditClass>();
                oreturnEditClass.Add(new returnEditClass
                {
                    Id = k.Id,
                    JourneyDist = k.JourneyDist.ToString(),
                    PlaceFrom = k.PlaceFrom.ToString(),
                    PlaceTo = k.PlaceTo.ToString(),
                    Remark = k.Remark,
                    TAClaimAmt = k.TAClaimAmt.ToString(),
                    TAElligibleAmt = k.TAElligibleAmt.ToString(),
                    TASettleAmt = k.TASettleAmt.ToString(),
                    FromDate = k.FromDate.ToString(),
                    ToDate = k.ToDate.ToString(),
                    TravelModeEligibilityPolicy_Id = k.TravelModeEligibilityPolicy!=null ? k.TravelModeEligibilityPolicy.Id:0,
                    PlaceFromId = PlaceFromId,
                    PlaceToId = PlaceToId
                });

                return Json(oreturnEditClass, JsonRequestBehavior.AllowGet);


            }
        }
        public ActionResult CreateJourneyObj(JourneyObject c, FormCollection form, string journeystart, string journeyend) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<String> Msg = new List<String>();

                int TravelModeEligibilityPolicylist = form["TravelModeEligibilityPolicylistobj"] == "0" ? 0 : Convert.ToInt32(form["TravelModeEligibilityPolicylistobj"]);
                string wages = form["Wageslist"] == "0" ? "" : form["Wageslist"];
                string Servicerangelist = form["ServiceRangeList"] == "0" ? "" : form["ServiceRangeList"];
                string Onwayfrom = form["Onwayfrom"];
                string OnwayTo = form["OnwayTo"];
                

                if (Onwayfrom != "" && Onwayfrom != null)
                {

                    TimeSpan TF = _returnTimeSpan(c.FromDate);
                    var t = TF;
                    string t1 = Convert.ToString(t);
                    if (t1 != "00:00:00")
                    {
                        Msg.Add("Please Select start Time 00:00");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //return this.Json(new Object[] { "Please Select start Time 00:00" }, JsonRequestBehavior.AllowGet);
                    }
                }

                DateTime jStart = Convert.ToDateTime(journeystart);
                DateTime jEnd = Convert.ToDateTime(journeyend);
                var ctodate = c.FromDate.Value.ToShortDateString() + " " + c.ToDate.Value.ToShortTimeString();
                c.ToDate = Convert.ToDateTime(ctodate);

                if (OnwayTo != "" && OnwayTo != null)
                {
                    TimeSpan TF = _returnTimeSpan(c.ToDate);
                    var t = TF;
                    string t1 = Convert.ToString(t);

                    if (t1 != "23:59:00")
                    {
                        Msg.Add("Please Select end Time 23:59");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //return this.Json(new Object[] { "Please Select end Time 23:59" }, JsonRequestBehavior.AllowGet);
                    }
                }

               
                try
                {
                    if (c.ToDate < c.FromDate)
                    {
                        Msg.Add("JourneyEnd date Should Not be Less than JourneyStart Date");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //return this.Json(new Object[] { "JourneyEnd date Should Not be Less than JourneyStart Date" }, JsonRequestBehavior.AllowGet);

                    }
                    if (c.FromDate.Value.Date > jEnd.Date)
                    {
                        Msg.Add("Date between journey date");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        
                    }

                    if (c.FromDate.Value.Date < jStart.Date)
                    {
                        Msg.Add("Date between journey date");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //return this.Json(new Object[] { "Date between journey date" }, JsonRequestBehavior.AllowGet);
                    }
                    if (c.TASettleAmt > c.TAClaimAmt)
                    {
                        Msg.Add("Settlement amount should not be greater than claim amount");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //return this.Json(new Object[] { "Settlement amount should not be greater than claim amount" }, JsonRequestBehavior.AllowGet);
                    }

                    //if (TravelModeEligibilityPolicylist != null)
                    //{
                    //    if (TravelModeEligibilityPolicylist != "")
                    //    {
                    //        int id = Convert.ToInt32(TravelModeEligibilityPolicylist);
                    //        var val = db.TravelModeEligibilityPolicy.Where(e => e.Id == id).SingleOrDefault();
                    //        c.TravelModeEligibilityPolicy = val;
                    //    }
                    //}

                    //if (wages != null)
                    //{
                    //    if (wages != "")
                    //    {
                    //        int id = Convert.ToInt32(wages);
                    //        var val = db.Wages.Where(e => e.Id == id).SingleOrDefault();
                    //        c.TA_TMRC_Eligibility_Wages = val;
                    //    }
                    //}

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

                            JourneyObject JourneyObjectdetails = new JourneyObject()
                            {
                                JourneyDist = c.JourneyDist,
                                ToDate = c.ToDate,
                                FromDate = c.FromDate,
                                PlaceFrom = c.PlaceFrom,
                                PlaceTo = c.PlaceTo,
                                Remark = c.Remark,
                                TAClaimAmt = c.TAClaimAmt,
                                TAElligibleAmt = c.TAElligibleAmt,
                                TASettleAmt = c.TASettleAmt,
                                TravelModeEligibilityPolicy = db.TravelModeEligibilityPolicy.Find(TravelModeEligibilityPolicylist),
                                DBTrack = c.DBTrack,
                                TotalHrs = c.TotalHrs,
                                TotalDistance = c.JourneyDist
                            };

                            db.JourneyObject.Add(JourneyObjectdetails);
                            db.SaveChanges();

                            //var journyobjdata = db.JourneyObject.Where(e => e.Id == JourneyObjectdetails.Id).Select(e => new
                            //{
                            //    Id = e.Id,
                            //    fulldetails = "JourneyDistance:" + e.JourneyDist + ", " + "PlaceFrom:" + e.PlaceFrom + ", " + "PlaceTo:" + e.PlaceTo + ", " + "FromDate:" + e.FromDate.Value.ToString("MM/dd/yyyy hh:mm") + ", " + "ToDate:" + e.ToDate.Value.ToString("MM/dd/yyyy hh:mm")
                            //}).SingleOrDefault();

                            ts.Complete();
                            //Msg.Add("Data Saved Successfully.");
                            //return Json(new Utility.JsonReturnClass { Id = JourneyObjectdetails.Id, Val = JourneyObjectdetails.PlaceFrom, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            return this.Json(new Object[] { JourneyObjectdetails.Id, JourneyObjectdetails.PlaceFrom, "Data Saved SucessFully" }, JsonRequestBehavior.AllowGet);

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


        public ActionResult GetJourneyObjDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //var fall = db.JourneyObject.Select(e => new
                //{
                //    Id = e.Id,
                //    fulldetails = "JourneyDistance:" + e.JourneyDist + ", " + "PlaceFrom:" + e.PlaceFrom + ", " + "PlaceTo:" + e.PlaceTo
                //}).ToList();
                var fall = db.JourneyObject.ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.JourneyObject.Where(e => e.Id != a).ToList();
                        else
                            fall = fall.Where(e => e.Id != a).ToList();
                    }
                }
                var list1 = db.JourneyDetails.Where(e => e.JourneyObject.Count() > 0).ToList().SelectMany(e => e.JourneyObject);
                var list2 = fall.Except(list1);
                var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = "JourneyDistance:" + ca.JourneyDist + ", " + "PlaceFrom:" + ca.PlaceFrom + ", " + "PlaceTo:" + ca.PlaceTo + ", " + "FromDate:" + ca.FromDate.Value.ToString("MM/dd/yyyy hh:mm") + ", " + "ToDate:" + ca.ToDate.Value.ToString("MM/dd/yyyy hh:mm") }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }

        

        public ActionResult GetLocation(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var selected = (String)null;
                var qurey = db.Location.Include(e => e.LocationObj).ToList();
                if (data2 != null && data2 != "")
                {
                    selected = data2;
                }
                if (data != null && data != "")
                {
                    var id = Convert.ToInt32(data);
                    qurey = db.Location.Include(e => e.LocationObj).Where(e => e.Id == id).ToList();
                }
                SelectList s = new SelectList(qurey, "Id", "FullDetails", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }

    }
}