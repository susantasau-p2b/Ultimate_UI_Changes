using P2b.Global;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using P2BUltimate.Security;
using Payroll;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Threading.Tasks;
using P2BUltimate.Process;
using P2B.SERVICES.Interface;
using P2B.SERVICES.Factory;

namespace P2BUltimate.Controllers.Payroll.MainController
{
    public class DIARYSettlementClaimController : Controller
    {
        readonly IP2BINI p2BINI;
        readonly CASHSettings CASHSettings;
        readonly WOSettings WOSettings;
        readonly WOWORKSettings WOWORKSettings;
        readonly OFFICEWORKSettings OFFICEWORKSettings;
        readonly OTHERSettings OTHERSettings;
        private readonly Type Type;

        public DIARYSettlementClaimController()
        {
            p2BINI = P2BINI.RegisterSettings();
            CASHSettings = new CASHSettings(p2BINI.GetSectionValues("CASHSettings"));
            WOSettings = new WOSettings(p2BINI.GetSectionValues("WOSettings"));
            WOWORKSettings = new WOWORKSettings(p2BINI.GetSectionValues("WOWORKSettings"));
            OFFICEWORKSettings = new OFFICEWORKSettings(p2BINI.GetSectionValues("OFFICEWORKSettings"));
            OTHERSettings = new OTHERSettings(p2BINI.GetSectionValues("OTHERSettings"));
            Type = typeof(DIARYSettlementClaimController);
           
        }

        
        

        public ActionResult hidebutton(string DiaryType)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var comp = db.Company.FirstOrDefault();
                if (DiaryType.ToUpper() == "CASH")
                {
                    if (CASHSettings.CCode.ToUpper() == comp.Code.ToUpper() && CASHSettings.DType.ToUpper() == DiaryType.ToUpper())
                    {
                        string s = CASHSettings.DInVisibleControlName;
                        string[] values = s.Split(','); 

                        string en = CASHSettings.DDisbleControlName;
                        string[] enValues = en.Split(',');

                        string vis = CASHSettings.DVisibleControlName;
                        string[] visValues = vis.Split(',');

                        return Json(new { values, enValues, visValues }, JsonRequestBehavior.AllowGet);
                    }  
                }
                else if (DiaryType.ToUpper() == "WEEKLYOFF")
                {
                    if (WOSettings.CCode.ToUpper() == comp.Code.ToUpper() && WOSettings.DType.ToUpper() == DiaryType.ToUpper())
                    {
                        string s = WOSettings.DInVisibleControlName;
                        string[] values = s.Split(',');

                        string en = WOSettings.DDisbleControlName;
                        string[] enValues = en.Split(',');

                        string vis = WOSettings.DVisibleControlName;
                        string[] visValues = vis.Split(',');

                        return Json(new { values, enValues, visValues }, JsonRequestBehavior.AllowGet);
                    }
                }
                else if (DiaryType.ToUpper() == "OFFICEWORK")
                {
                    if (OFFICEWORKSettings.CCode.ToUpper() == comp.Code.ToUpper() && OFFICEWORKSettings.DType.ToUpper() == DiaryType.ToUpper())
                    {
                        string s = OFFICEWORKSettings.DInVisibleControlName;
                        string[] values = s.Split(',');

                        string en = OFFICEWORKSettings.DDisbleControlName;
                        string[] enValues = en.Split(',');

                        string vis = OFFICEWORKSettings.DVisibleControlName;
                        string[] visValues = vis.Split(',');

                        return Json(new { values, enValues, visValues }, JsonRequestBehavior.AllowGet);
                    }
                }
                else if (DiaryType.ToUpper() == "WEEKLYOFFWITHWORK")
                {
                    if (WOWORKSettings.CCode.ToUpper() == comp.Code.ToUpper() && WOWORKSettings.DType.ToUpper() == DiaryType.ToUpper())
                    {
                        string s = WOWORKSettings.DInVisibleControlName;
                        string[] values = s.Split(',');

                        string en = WOWORKSettings.DDisbleControlName;
                        string[] enValues = en.Split(',');

                        string vis = WOWORKSettings.DVisibleControlName;
                        string[] visValues = vis.Split(',');

                        return Json(new { values, enValues, visValues }, JsonRequestBehavior.AllowGet);
                    }
                }
                else if (DiaryType.ToUpper().Contains("OTHER"))
                {
                    if (OTHERSettings.CCode.ToUpper() == comp.Code.ToUpper() && OTHERSettings.DType.ToUpper().Contains("OTHER") == DiaryType.ToUpper().Contains("OTHER"))
                    {
                        string s = OTHERSettings.DInVisibleControlName;
                        string[] values = s.Split(',');

                        string en = OTHERSettings.DDisbleControlName;
                        string[] enValues = en.Split(',');

                        string vis = OTHERSettings.DVisibleControlName;
                        string[] visValues = vis.Split(',');

                        return Json(new { values, enValues, visValues }, JsonRequestBehavior.AllowGet);
                    }
                }

                
            }
             
            return null;

        }
        //
        // GET: /DIARYSettlmentClaim/
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/DiarySettlementClaim/Index.cshtml");
        }
        public ActionResult partial_DIARYAdvanceclaim()
        {
            return View("~/Views/Shared/Payroll/_DiaryAdvanceClaim.cshtml");
        }


        public ActionResult GridPartial()
        {
            return View("~/Views/Shared/Payroll/_DiarySettlClaimGridPartial.cshtml");
        }
        public class GetDIARYsettlamt //childgrid
        {

            public double AdvanceAmt { get; set; }
            public double DIARYClaimamt { get; set; }
            public double DIARYElgmamt { get; set; }
            public double SanctionAmt { get; set; }
            public double SettlementAmt { get; set; }

            public double NoOfDays { get; set; }
        }
        public ActionResult GridEditData(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    //var empltcblockdata = db.EmpLTCBlockT.Select(e => e.LTCSettlementClaim.Where(t => t.Id == data)).ToList();


                    var Q = db.DIARYSettlementClaim
                        .Include(e => e.JourneyDetails)
                        .Include(e => e.Travel_Hotel_Booking)
                        .Include(e => e.DIARYAdvanceClaim)
                        //.Include(e=>e.FamilyDetails)
                        //.Include(e=>e.EmployeeDocuments)
                     .Where(e => e.Id == data).Select
                     (c => new
                     {
                         Id = c.Id,
                         // BlockPeriod = c.FullDetails,
                         ReqDate = c.DateOfAppl,
                         Noofdays = c.NoOfDays,
                         DIARYType = c.DIARYType != null ? c.DIARYType.Id : 0,
                         // lvreqid = c.LvNewReq == null ? 0 : c.LvNewReq.Id,
                         Eligible_Amt = c.Eligible_Amt,
                         AdvAmt = c.AdvAmt,
                         Remark = c.Remark,
                         Claim_Amt = c.Claim_Amt,
                         EncashmentAmount = c.EncashmentAmount == null ? 0 : c.EncashmentAmount,
                         SanctionAmt = c.SanctionAmt == null ? 0 : c.SanctionAmt,
                         SettlementAmt = c.SettlementAmt == null ? 0 : c.SettlementAmt,
                         LTCAdvAmt = c.AdvAmt == null ? 0 : c.AdvAmt,
                         JourneydetailsId = c.JourneyDetails == null ? 0 : c.JourneyDetails.Id,
                         JourneydetailsFulldetails = c.JourneyDetails == null ? "" : c.JourneyDetails.FullDetails,
                         DIARYAdvanceClaimid = c.DIARYAdvanceClaim == null ? 0 : c.DIARYAdvanceClaim.Id,
                         LTCAdvanceClaimfulldetails = c.DIARYAdvanceClaim == null ? "" : c.DIARYAdvanceClaim.FullDetails,
                         travelhoteldetails = c.Travel_Hotel_Booking.Select(n => new
                         {
                             id = n.Id,
                             fulldetails = "HotelName:" + n.HotelName + ",HotelDesc" + n.HotelDesc

                         })

                     }).SingleOrDefault();


                    return Json(new { data = Q }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {

                    throw;
                }
                //return Json(new Object[] { Q, return_data, return_dataDoc, JsonRequestBehavior.AllowGet });
                //return Json(new { data = Q }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GridEditSave(DIARYSettlementClaim c, FormCollection form, string data)
        {
            List<string> Msgs = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                int Id = int.Parse(data);
                string DIARYadvanceclaimlist = form["DIARYAdvanceClaimListPartial"] == "0" ? "" : form["DIARYAdvanceClaimListPartial"];
                string DIARYtypelist = form["DIARYTypelistPartial"] == "0" ? "" : form["DIARYTypelistPartial"];
                //string lvreqlist = form["LvReqListPartial"] == "0" ? "" : form["LvReqListPartial"];
                string journewdetailslist = form["JourneyDetailsListPartial"] == "0" ? "" : form["JourneyDetailsListPartial"];
                string hotelbookinglist = form["HotelBookingDetailsListPartial"] == "0" ? "" : form["HotelBookingDetailsListPartial"];
                //string LtcBlock = form["BlockIdPartial"] == "0" ? "" : form["BlockIdPartial"];

                var DIARYSettleClaimData = db.DIARYSettlementClaim
                     .Include(e => e.JourneyDetails)
                     .Include(e => e.Travel_Hotel_Booking)
                     .Include(e => e.DIARYAdvanceClaim)
                    .Where(e => e.Id == Id).SingleOrDefault();

                if (DIARYadvanceclaimlist != null && DIARYadvanceclaimlist != "")
                {
                    int DIARYadvanceid = Convert.ToInt32(DIARYadvanceclaimlist);
                    var value = db.DIARYAdvanceClaim.Where(e => e.Id == DIARYadvanceid).SingleOrDefault();
                    DIARYSettleClaimData.DIARYAdvanceClaim = value;

                }
                else
                {
                    DIARYSettleClaimData.DIARYAdvanceClaim = null;
                }
                if (hotelbookinglist != null)
                {
                    var ids = Utility.StringIdsToListIds(hotelbookinglist);
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
                    DIARYSettleClaimData.Travel_Hotel_Booking = travelhotelbookinglist;
                }
                else
                {
                    DIARYSettleClaimData.Travel_Hotel_Booking = null;
                }


                if (journewdetailslist != null)
                {
                    if (journewdetailslist != "")
                    {
                        int ids = Convert.ToInt32(journewdetailslist);
                        var val = db.JourneyDetails.Where(e => e.Id == ids).SingleOrDefault();
                        DIARYSettleClaimData.JourneyDetails = val;
                    }
                }
                else
                {
                    DIARYSettleClaimData.JourneyDetails = null;
                }
                //if (lvreqlist != null)
                //{
                //    if (lvreqlist != "")
                //    {
                //        int ids = Convert.ToInt32(lvreqlist);
                //        var val = db.LvNewReq.Where(e => e.Id == ids).SingleOrDefault();
                //        LTCSettleClaimData.LvNewReq = val;
                //    }
                //}
                //else
                //{
                //    LTCSettleClaimData.Travel_Hotel_Booking = null;
                //}
                if (DIARYtypelist != null && DIARYtypelist != "")
                {
                    var val = db.LookupValue.Find(int.Parse(DIARYtypelist));
                    DIARYSettleClaimData.DIARYType = val;
                }
                else
                {
                    DIARYSettleClaimData.DIARYType = null;
                }

                DIARYSettleClaimData.JourneyDetails = DIARYSettleClaimData.JourneyDetails;
                DIARYSettleClaimData.DIARYAdvanceClaim = DIARYSettleClaimData.DIARYAdvanceClaim;
                DIARYSettleClaimData.Travel_Hotel_Booking = DIARYSettleClaimData.Travel_Hotel_Booking;
                DIARYSettleClaimData.Remark = c.Remark;
                //  DIARYSettleClaimData.LvNewReq = DIARYSettleClaimData.LvNewReq;
                DIARYSettleClaimData.NoOfDays = c.NoOfDays;
                DIARYSettleClaimData.DIARYType = DIARYSettleClaimData.DIARYType;
                DIARYSettleClaimData.AdvAmt = c.AdvAmt;
                DIARYSettleClaimData.SettlementAmt = c.SettlementAmt;
                DIARYSettleClaimData.SanctionAmt = c.SanctionAmt;
                DIARYSettleClaimData.EncashmentAmount = c.EncashmentAmount;
                DIARYSettleClaimData.DateOfAppl = c.DateOfAppl;
                DIARYSettleClaimData.Eligible_Amt = c.Eligible_Amt;
                DIARYSettleClaimData.Claim_Amt = c.Claim_Amt;
                using (TransactionScope ts = new TransactionScope())
                {

                    DIARYSettleClaimData.DBTrack = new DBTrack
                    {
                        CreatedBy = DIARYSettleClaimData.DBTrack.CreatedBy == null ? null : DIARYSettleClaimData.DBTrack.CreatedBy,
                        CreatedOn = DIARYSettleClaimData.DBTrack.CreatedOn == null ? null : DIARYSettleClaimData.DBTrack.CreatedOn,
                        Action = "M",
                        ModifiedBy = SessionManager.UserName,
                        ModifiedOn = DateTime.Now
                    };


                    try
                    {
                        db.DIARYSettlementClaim.Attach(DIARYSettleClaimData);
                        db.Entry(DIARYSettleClaimData).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        db.Entry(DIARYSettleClaimData).State = System.Data.Entity.EntityState.Detached;

                        ts.Complete();
                        return this.Json(new { status = true, responseText = "Data Updated Successfully.", JsonRequestBehavior.AllowGet });
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        return RedirectToAction("Create", new { concurrencyError = true, id = DIARYSettleClaimData.Id });
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

        public class DIARYSettlClaimChildDataClass //childgrid
        {
            public int Id { get; set; }
            public string ReqDate { get; set; }
            public string DIARYType { get; set; }
            public double EligibleAmt { get; set; }
            public double AdvanceAmt { get; set; }
            public string Remark { get; set; }

        }

        public class returnEditClass1
        {
            public Array JourneyObject_Id { get; set; }
            public Array JourneyObjectFullDetails { get; set; }
            public Array Family_Id { get; set; }
            public Array FamilyFullDetails { get; set; }
            public Array MisExpenseObject_Id { get; set; }
            public Array MisExpenseObjectFullDetails { get; set; }
            public Array TravelHotelBooking_Id { get; set; }
            public Array TravelHotelBookingFullDetails { get; set; }
        }

        public static DateTime LastDayOfMonth(string dt)
        {
            DateTime dtnw = Convert.ToDateTime(dt);
            DateTime ss = new DateTime(dtnw.Year, dtnw.Month, 1); // Creating a new DateTime object representing the first day of the input month and year
            return ss.AddMonths(1).AddDays(-1); // Returning the last day of the input month by adding one month to the first day and then subtracting one day
        }

        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                //var empltcblockdata = db.EmpLTCBlockT.Select(e => e.LTCSettlementClaim.Where(t => t.Id == data)).ToList();

                var q1 = db.JourneyDetails.Include(e => e.JourneyObject).Include(e => e.TravelModeEligibilityPolicy)
                    .Include(e => e.TravelModeEligibilityPolicy)
                    .Where(e => e.Id == data).Select(x => new
                    {
                        PayMonth = x.JourneyStart.Value.Day + "/" + x.JourneyStart.Value.Year,
                        //MonthStart = "01/" + x.JourneyStart.Value.Day + "/" + x.JourneyStart.Value.Year,
                        //MonthEnd = LastDayOfMonth("01/" + x.JourneyStart.Value.Day + "/" + x.JourneyStart.Value.Year),
                        JourneyStart = x.JourneyStart,
                        JourneyEnd = x.JourneyEnd,
                        NoOfDays = x.NoOfDays,
                        // TravelModeEligibilityPolicy=x.TravelModeEligibilityPolicy.TA_TM_Elligibilty_Code,
                        TravelModeEligibilityPolicy = x.TravelModeEligibilityPolicy == null ? "0" : x.TravelModeEligibilityPolicy.Id.ToString(),
                        DAElligibleAmt = x.DAElligibleAmt,
                        DAClaimAmt = x.DAClaimAmt,
                        DASettleAmt = x.DASettleAmt,
                        TAElligibleAmt = x.TAElligibleAmt,
                        TAClaimAmt = x.TAClaimAmt,
                        TASettleAmt = x.TASettleAmt,
                        MisExpenseElligibleAmt = x.MisExpenseElligibleAmt,
                        MisExpenseClaimAmt = x.MisExpenseClaimAmt,
                        MisExpenseSettleAmt = x.MisExpenseSettleAmt,
                        HotelElligibleAmt = x.HotelElligibleAmt,
                        HotelClaimAmt = x.HotelClaimAmt,
                        HotelSettleAmt = x.HotelSettleAmt,
                        TotFamilyMembers = x.TotFamilyMembers,

                    }).SingleOrDefault();

                List<returnEditClass1> oreturnEditClass = new List<returnEditClass1>();

                string MonthStart = ("01/" + (q1.JourneyStart.Value.Month.ToString().Length != 1 ? q1.JourneyStart.Value.Month.ToString() : "0" + q1.JourneyStart.Value.Month.ToString()) + "/" + q1.JourneyStart.Value.Year).ToString();
                string MonthEnd = LastDayOfMonth(MonthStart).ToShortDateString();

                var k = db.JourneyDetails.Include(e => e.JourneyObject)
                   .Where(e => e.Id == data && e.JourneyObject.Count > 0).ToList();
                foreach (var e in k)
                {
                    oreturnEditClass.Add(new returnEditClass1
                    {
                        JourneyObject_Id = e.JourneyObject.Select(a => a.Id.ToString()).ToArray(),
                        JourneyObjectFullDetails = e.JourneyObject.Select(a => "JourneyDistance:" + a.JourneyDist + ", " + "PlaceFrom:" + a.PlaceFrom + ", " + "PlaceTo:" + a.PlaceTo + ", " + "FromDate:" + a.FromDate.Value.ToString("MM/dd/yyyy hh:mm") + ", " + "ToDate:" + a.ToDate.Value.ToString("MM/dd/yyyy hh:mm")).ToArray()
                    });
                }

                var y = db.JourneyDetails.Include(e => e.FamilyDetails)
                      .Where(e => e.Id == data && e.FamilyDetails.Count > 0).ToList();
                foreach (var e in k)
                {
                    if (e.FamilyDetails != null)
                    {
                        oreturnEditClass.Add(new returnEditClass1
                        {
                            Family_Id = e.FamilyDetails.Select(a => a.Id.ToString()).ToArray(),
                            FamilyFullDetails = e.FamilyDetails.Select(a => a.FullDetails).ToArray()
                        });
                    }

                }

                var m = db.JourneyDetails.Include(e => e.MisExpenseObject)
                         .Where(e => e.Id == data && e.MisExpenseObject.Count > 0).ToList();
                foreach (var e in k)
                {
                    if (e.MisExpenseObject != null)
                    {
                        oreturnEditClass.Add(new returnEditClass1
                        {
                            MisExpenseObject_Id = e.MisExpenseObject.Select(a => a.Id.ToString()).ToArray(),
                            MisExpenseObjectFullDetails = e.MisExpenseObject.Select(a => a.MisExpenseDate + " " + a.MisExpenseElligibleAmt + " " + a.MisExpenseClaimAmt + " " + a.MisExpenseSettleAmt).ToArray()
                        });
                    }

                }

                var t = db.JourneyDetails.Include(e => e.Travel_Hotel_Booking)
                             .Where(e => e.Id == data && e.Travel_Hotel_Booking.Count > 0).ToList();
                foreach (var e in k)
                {
                    if (e.Travel_Hotel_Booking != null)
                    {
                        oreturnEditClass.Add(new returnEditClass1
                        {
                            TravelHotelBooking_Id = e.Travel_Hotel_Booking.Select(a => a.Id.ToString()).ToArray(),
                            TravelHotelBookingFullDetails = e.Travel_Hotel_Booking.Select(a => a.StartDate + " " + a.EndDate + " " + a.HotelName + " " + a.BillAmount).ToArray()
                        });
                    }

                }


                return Json(new Object[] { q1, oreturnEditClass, "", MonthStart, MonthEnd, JsonRequestBehavior.AllowGet });

                //return Json(new Object[] { Q, return_data, return_dataDoc, JsonRequestBehavior.AllowGet });
                //return Json(new { data = Q }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Get_DIARYSettlementClaim(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    //var db_data = db.EmployeePayroll
                    //    .Include(e => e.EmpLTCBlock)
                    //    .Include(e => e.EmpLTCBlock.Select(a => a.EmpLTCBlockT))
                    //    .Include(e => e.EmpLTCBlock.Select(a => a.EmpLTCBlockT.Select(b => b.LTCAdvanceClaim)))
                    //    .Include(e => e.EmpLTCBlock.Select(a => a.EmpLTCBlockT.Select(b => b.LTCAdvanceClaim.Select(c => c.LTC_TYPE))))
                    //    .Where(e => e.Id == data).FirstOrDefault();
                    var db_data = db.EmployeePayroll
                       .Include(e => e.DIARYSettlementClaim)
                        .Include(e => e.DIARYSettlementClaim.Select(c => c.DIARYType))
                       .Where(e => e.Id == data).FirstOrDefault();

                    if (db_data.DIARYSettlementClaim != null)
                    {
                        List<DIARYSettlClaimChildDataClass> returndata = new List<DIARYSettlClaimChildDataClass>();
                        var LTCAdvClaimlist = db_data.DIARYSettlementClaim;
                        foreach (var item in db_data.DIARYSettlementClaim.OrderByDescending(e => e.Id))
                        {

                            returndata.Add(new DIARYSettlClaimChildDataClass
                            {
                                Id = item.Id,
                                ReqDate = item.DateOfAppl.Value != null ? item.DateOfAppl.Value.ToShortDateString() : "",
                                DIARYType = item.DIARYType != null ? item.DIARYType.LookupVal.ToString() : null,
                                EligibleAmt = item.Eligible_Amt,
                                AdvanceAmt = item.AdvAmt,
                                Remark = item.Remark,

                            });



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
                    throw e;
                }
            }
        }
        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                DIARYSettlementClaim DIARYsettleclaimReq = db.DIARYSettlementClaim.Include(e => e.Travel_Hotel_Booking).Where(e => e.Id == data).SingleOrDefault();



                var selectedTravelhotelbook = DIARYsettleclaimReq.Travel_Hotel_Booking;

                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {


                    try
                    {
                        db.TravelHotelBooking.RemoveRange(selectedTravelhotelbook);
                        db.DIARYSettlementClaim.Remove(DIARYsettleclaimReq);

                        //   db.Entry(LoanAdvReq).State = System.Data.Entity.EntityState.Deleted;


                        await db.SaveChangesAsync();


                        ts.Complete();
                        return this.Json(new { status = true, responseText = "Data removed.", JsonRequestBehavior.AllowGet });

                    }
                    catch (RetryLimitExceededException /* dex */)
                    {
                        //Log the error (uncomment dex variable name and add a line here to write a log.)
                        //ModelState.AddModelError("", "Unable to delete. Try again, and if the problem persists, see your system administrator.");
                        //return RedirectToAction("Delete");
                        return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
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
        public class returnEditClass
        {
            public int Id { get; set; }
            public string LookupVal { get; set; }
        }

        public ActionResult GetLookupPolicyname(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<returnEditClass> query1 = new List<returnEditClass>();

                query1.Add(new returnEditClass
                {
                    Id = 1,
                    LookupVal = "DIARY"
                });
                SelectList s = (SelectList)null;
                var selected = "";
                if (data != "" && data != null)
                {                
                    if (data2 != "" && data2 != "0")
                    {
                        selected = data2;
                    }
                    if (query1 != null)
                    {
                        s = new SelectList(query1, "Id", "LookupVal", selected);
                    }
                }
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult Create(DIARYSettlementClaim c, FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    int comp_Id = 0;
                    comp_Id = Convert.ToInt32(Session["CompId"]);
                    var Company = new Company();
                    Company = db.Company.Where(e => e.Id == comp_Id).SingleOrDefault();
                    int Emp = form["Employee-Table"] == "0" ? 0 : Convert.ToInt32(form["Employee-Table"]);
                    string DIARYadvanceclaimlist = form["DIARYAdvanceClaimList"] == "0" ? "" : form["DIARYAdvanceClaimList"];
                    string DIARYtypelist = form["DIARYTypelist"] == "0" ? "" : form["DIARYTypelist"];
                    // string lvreqlist = form["LvReqList"] == "0" ? "" : form["LvReqList"];
                    string journewdetailslist = form["JourneyDetailsList"] == "0" ? "" : form["JourneyDetailsList"];
                    string trhotelbookinglist = form["Travel_Hotel_BookingList"] == "0" ? "" : form["Travel_Hotel_BookingList"];
                    //  string LtcBlock = form["BlockId"] == "0" ? "" : form["BlockId"];

                    string tadatypelist = form["TADATypelist"] == "0" ? "" : form["TADATypelist"];
                    //string Incharge_DDL = form["Incharge_DDL"] == "" ? null : form["Incharge_DDL"];
                    string Incharge_DDL = form["Incharge_Id"] == "" ? null : form["Incharge_Id"];

                    if (DIARYadvanceclaimlist != null && DIARYadvanceclaimlist != "")
                    {
                        int DIARYadvanceid = Convert.ToInt32(DIARYadvanceclaimlist);
                        var value = db.DIARYAdvanceClaim.Where(e => e.Id == DIARYadvanceid).SingleOrDefault();
                        c.DIARYAdvanceClaim = value;

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



                    if (journewdetailslist != null)
                    {
                        if (journewdetailslist != "")
                        {
                            int ids = Convert.ToInt32(journewdetailslist);
                            var val = db.JourneyDetails.Where(e => e.Id == ids).SingleOrDefault();
                            c.JourneyDetails = val;
                        }
                    }

                    //if (lvreqlist != null)
                    //{
                    //    if (lvreqlist != "")
                    //    {
                    //        int ids = Convert.ToInt32(lvreqlist);
                    //        var val = db.LvNewReq.Where(e => e.Id == ids).SingleOrDefault();
                    //        c.LvNewReq = val;
                    //    }
                    //}

                    if (DIARYtypelist != null && DIARYtypelist != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(DIARYtypelist));
                        c.DIARYType = val;
                    }
                    var employeedata = db.Employee.Include(e => e.GeoStruct).Include(e => e.PayStruct).Include(e => e.FuncStruct).Where(e => e.Id == Emp).SingleOrDefault();
                    c.PayStruct = employeedata.PayStruct;
                    c.GeoStruct = employeedata.GeoStruct;
                    c.FuncStruct = employeedata.FuncStruct;
                    // int Blockid = int.Parse(LtcBlock);
                    // EmpLTCBlockT OEmpLTCBlockT = db.EmpLTCBlockT.Find(Blockid);

                    var a = db.EmployeePayroll.Include(e => e.EmpLTCBlock).Include(e => e.Employee)
                             .Include(e => e.EmpLTCBlock.Select(y => y.EmpLTCBlockT))
                             .Where(e => e.Employee.Id == Emp).AsNoTracking().SingleOrDefault().EmpLTCBlock.Where(e => e.IsBlockClose == false).FirstOrDefault();

                    ////var test = a.EmpLTCBlockT.Where(e => e.BlockPeriodStart >= OEmpLTCBlockT.BlockPeriodStart
                    ////   && e.BlockPeriodEnd <= OEmpLTCBlockT.BlockPeriodEnd).FirstOrDefault();
                    //if (c.SettlementAmt > c.Eligible_Amt)
                    //{
                    //    Msg.Add("Settlement amount Should not be greater than Eligible amount.");
                    //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //}
                    //if (c.SanctionAmt > c.Eligible_Amt)
                    //{
                    //    Msg.Add("Sanction amount Should not be greater than Eligible amount.");
                    //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //}

                    //if (c.SettlementAmt > c.SanctionAmt)
                    //{
                    //    Msg.Add("Settlement amount Should not be greater than sanction amount.");
                    //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //}

                    // yearlypayment

                    var emppi = db.EmployeePayroll.Where(e => e.Employee.Id == Emp).SingleOrDefault();
                    int salhdid = 0;
                    var fyyr = db.Calendar.Include(x => x.Name).Where(x => x.Name.LookupVal.ToUpper() == "FINANCIALYEAR" && x.Default == true).SingleOrDefault();

                    var Exist = db.EmployeePayroll.Include(e => e.DIARYSettlementClaim).Include(e => e.DIARYSettlementClaim.Select(t => t.JourneyDetails)).Where(e => e.Id == emppi.Id).FirstOrDefault();
                    if (Exist.DIARYSettlementClaim.Any(e => e.JourneyDetails.JourneyStart.Value == c.JourneyDetails.JourneyStart.Value))
                    {
                         Msg.Add("Record already exists.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    //var QEmpSalStruct1 = db.EmpSalStruct//.Where(e => e.EmployeePayroll_Id == OEmployeePayrollId && e.EffectiveDate >= comparedate)
                    //                      .Include(e => e.EmpSalStructDetails)
                    //                      .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead))
                    //                      .Include(e => e.EmpSalStructDetails.Select(q => q.SalHeadFormula))
                    //                      .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.Frequency))
                    //                      .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.Type))
                    //                      .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.RoundingMethod))
                    //                      .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.SalHeadOperationType))
                    //                      .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.LvHead));//.ToList();
                    //var EmpSalStructTotal = QEmpSalStruct1.Where(e => e.EmployeePayroll.Id == emppi.Id && e.EndDate == null)
                    //                              .ToList();
                    //var OEmpSalStruct = EmpSalStructTotal.Where(e => e.EndDate == null).SingleOrDefault();
                    //if (OEmpSalStruct != null)
                    //{

                    //    salhdid = OEmpSalStruct.EmpSalStructDetails
                    //       .Where(s => s.SalaryHead.Frequency.LookupVal.ToUpper() == "YEARLY"
                    //    && s.SalaryHead.Type.LookupVal.ToUpper() == "EARNING"
                    //    && (s.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "DIARY"
                    //    )).FirstOrDefault() != null ? OEmpSalStruct.EmpSalStructDetails
                    //       .Where(s => s.SalaryHead.Frequency.LookupVal.ToUpper() == "YEARLY"
                    //    && s.SalaryHead.Type.LookupVal.ToUpper() == "EARNING"
                    //    && (s.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "DIARY"
                    //    )).FirstOrDefault().SalaryHead.Id : 0;
                    //}


                    //if (salhdid == 0)
                    //{
                    //    Msg.Add("Please Define DIARY in salary Structure.");
                    //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //}
                    FunctAllWFDetails oAttWFDetails = new FunctAllWFDetails
                    {
                        WFStatus = 5,
                        Comments = c.Remark.ToString(),
                        DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                    };

                    List<FunctAllWFDetails> oAttWFDetails_List = new List<FunctAllWFDetails>();
                    oAttWFDetails_List.Add(oAttWFDetails);

                    if (ModelState.IsValid)
                    {


                        using (TransactionScope ts = new TransactionScope())
                        {
                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            DIARYSettlementClaim DIARYsettlement = new DIARYSettlementClaim()
                            {
                                DateOfAppl = c.DateOfAppl,
                                //  EncashmentAmount = c.EncashmentAmount,
                                JourneyDetails = c.JourneyDetails,
                                Travel_Hotel_Booking = c.Travel_Hotel_Booking,
                                DIARYAdvanceClaim = c.DIARYAdvanceClaim,
                                NoOfDays = c.NoOfDays,
                                DIARYType = c.DIARYType,
                                //LvNewReq = c.LvNewReq,
                                Claim_Amt = c.Claim_Amt,
                                Eligible_Amt = c.Eligible_Amt,
                                AdvAmt = c.AdvAmt,
                                SanctionAmt = c.SanctionAmt,
                                SettlementAmt = c.SettlementAmt,
                                Remark = c.Remark,
                                FuncStruct = c.FuncStruct,
                                GeoStruct = c.GeoStruct,
                                PayStruct = c.PayStruct,
                                WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "1").FirstOrDefault(),//db.LookupValue.Where(e => e.LookupVal == "1").SingleOrDefault(),
                                LTCWFDetails = oAttWFDetails_List,
                                DBTrack = c.DBTrack,
                                TotalDistance = c.TotalDistance,
                                TotalHrs = c.TotalHrs
                            };

                            db.DIARYSettlementClaim.Add(DIARYsettlement);
                            db.SaveChanges();

                            // yearly payment strat
                            //YearlyPaymentT ObjYPT = new YearlyPaymentT();
                            //{
                            //    ObjYPT.SalaryHead = db.SalaryHead.Find(salhdid);
                            //    ObjYPT.AmountPaid = c.SettlementAmt;
                            //    ObjYPT.FromPeriod = fyyr.FromDate;
                            //    ObjYPT.ToPeriod = fyyr.ToDate;
                            //    ObjYPT.ProcessMonth = Convert.ToDateTime(c.DateOfAppl).ToString("MM/yyyy");
                            //    ObjYPT.PayMonth = Convert.ToDateTime(c.DateOfAppl).ToString("MM/yyyy");
                            //    ObjYPT.ReleaseDate = c.DateOfAppl;
                            //    ObjYPT.ReleaseFlag = true;
                            //    ObjYPT.TDSAmount = 0;
                            //    ObjYPT.OtherDeduction = 0;
                            //    ObjYPT.FinancialYear = fyyr;
                            //    ObjYPT.DBTrack = c.DBTrack;
                            //    ObjYPT.FuncStruct = c.FuncStruct;
                            //    ObjYPT.GeoStruct = c.GeoStruct;
                            //    ObjYPT.PayStruct = c.PayStruct;

                            //}
                            //List<YearlyPaymentT> OYrlyPaylist = new List<YearlyPaymentT>();
                            //db.YearlyPaymentT.Add(ObjYPT);
                            //db.SaveChanges();
                            //OYrlyPaylist.Add(ObjYPT);

                            //yearly payment end

                            List<DIARYSettlementClaim> DIARYSettlementlist = new List<DIARYSettlementClaim>();
                            var aa = db.EmployeePayroll.Include(e => e.DIARYSettlementClaim).Include(e => e.YearlyPaymentT).Include(e => e.Employee).Where(e => e.Employee.Id == Emp).SingleOrDefault();
                            if (aa.DIARYSettlementClaim.Count() > 0)
                            {
                                DIARYSettlementlist.AddRange(aa.DIARYSettlementClaim);
                            }
                            else
                            {
                                DIARYSettlementlist.Add(DIARYsettlement);
                            }
                            //if (aa.YearlyPaymentT.Count() > 0)
                            //{
                            //    OYrlyPaylist.AddRange(aa.YearlyPaymentT);
                            //}

                            aa.DIARYSettlementClaim = DIARYSettlementlist;
                          //  aa.YearlyPaymentT = OYrlyPaylist;
                            //db.EmpLTCBlockT.Attach(aa);
                            db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            db.Entry(aa).State = System.Data.Entity.EntityState.Detached;
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

        [HttpPost]
        public ActionResult GetDIARYAmount(FormCollection form, int data)
        {

            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string tadatypelist = form["TADATypelist"] == "0" ? "" : form["TADATypelist"];
                    string journewdetailslist = form["JourneyDetailsList"] == "0" ? "" : form["JourneyDetailsList"];
                    string DIARYadvanceclaimlist = form["DIARYAdvanceClaimList"] == "0" ? "" : form["DIARYAdvanceClaimList"];
                    string travelhotelbookinglist = form["Travel_Hotel_BookingList"] == "0" ? "" : form["Travel_Hotel_BookingList"];
                    //string MisExpenseObjectlistedit = form["Travel_Hotel_BookingList"] == "0" ? "" : form["Travel_Hotel_BookingList"];

                    List<BMSModuleProcess.GetDIARYsettlamt> returndata = new List<BMSModuleProcess.GetDIARYsettlamt>();
                    returndata = BMSModuleProcess.Calculate_GetDiaryAmount(data, DIARYadvanceclaimlist, travelhotelbookinglist, journewdetailslist, null);

                    return Json(returndata, JsonRequestBehavior.AllowGet);

                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }

        public ActionResult GetLookupJourneyDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.JourneyDetails.ToList();
                IEnumerable<TravelHotelBooking> all;
                if (SkipIds != null)
                {
                    foreach (int a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.JourneyDetails.ToList();
                        else
                            fall = fall.Where(e => e.Id != a).ToList();

                    }
                }

                var list1 = db.LTCSettlementClaim.Where(e => e.JourneyDetails != null).ToList().Select(e => e.JourneyDetails);
                var list2 = db.DIARYSettlementClaim.Where(e => e.JourneyDetails != null).ToList().Select(e => e.JourneyDetails);
                var list3 = fall.Except(list1).Except(list2);

                var r = (from ca in list3 select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetLookupDIARYAdv(string data, string Empid)
        {
            int empids = Convert.ToInt32(Empid);


            using (DataBaseContext db = new DataBaseContext())
            {
                var employeedata = db.EmployeePayroll.Include(e => e.Employee)
                    .Include(e => e.DIARYAdvanceClaim)
                    .Include(e => e.DIARYAdvanceClaim.Select(c => c.DIARYType))
                    .Where(e => e.Employee.Id == empids).ToList();

                //var employeedata = db.Employee
                //          .Include(e => e.FamilyDetails)
                //          .Include(e => e.FamilyDetails.Select(t => t.MemberName))
                //              .Where(e => e.Id == empids).ToList();
                var fall = employeedata.SelectMany(e => e.DIARYAdvanceClaim).ToList();

                IEnumerable<DIARYAdvanceClaim> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.DIARYAdvanceClaim.ToList().Where(d => d.FullDetails.Contains(data));
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

        public class returndatagridclass //Parentgrid
        {
            public string Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
            public string JoiningDate { get; set; }
            public string Job { get; set; }
            public string Grade { get; set; }
            public string Location { get; set; }
        }

        public ActionResult Emp_Grid(ParamModel param, string y)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var all = db.EmployeePayroll.Include(e => e.Employee)
                        .Include(e => e.Employee.EmpName)
                        .Include(e => e.Employee.ServiceBookDates)
                        .Include(e => e.Employee.GeoStruct).Include(e => e.Employee.GeoStruct.Location.LocationObj)
                        //.Include(e => e.Employee.FuncStruct)
                        //   .Include(e => e.Employee.FuncStruct.Job).Include(e => e.Employee.PayStruct)
                        //  .Include(e => e.Employee.PayStruct.Grade)
                        .Where(e => e.Employee.ServiceBookDates.ServiceLastDate == null)
                        .ToList();
                    // for searchs
                    IEnumerable<EmployeePayroll> fall;
                    if (param.sSearch == null)
                    {
                        fall = all;
                    }
                    else
                    {
                        // fall = all.Where(e => e.Employee.EmpCode.ToUpper() == param.sSearch.ToUpper()).ToList();
                        fall = all.Where(e => (e.Id.ToString().Contains(param.sSearch))
                                  || (e.Employee.EmpCode.ToString().ToLower().Contains(param.sSearch.ToLower()))
                                  || (e.Employee.EmpName.FullNameFML.ToString().ToUpper().Contains(param.sSearch.ToUpper()))
                            //   || (e.Employee.ServiceBookDates.JoiningDate.Value.ToString("dd/MM/yyyy").Contains(param.sSearch))
                            //   || (e.Employee.FuncStruct.Job.Name.ToString().ToUpper().Contains(param.sSearch.ToUpper()))
                            // || (e.Employee.PayStruct.Grade.Name.ToString().ToUpper().Contains(param.sSearch.ToUpper()))
                            //  || (e.Employee.GeoStruct.Location.LocationObj.ToString().ToUpper().Contains(param.sSearch.ToUpper()))
                                  ).ToList();
                    }

                    //for column sorting
                    var sortindex = Convert.ToInt32(Request["iSortCol_0"]);

                    Func<EmployeePayroll, string> orderfunc = (c =>
                                                                Convert.ToUInt32(sortindex) == 0 ? c.Id.ToString() :
                                                                sortindex == 1 ? c.Employee.EmpCode : "");
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
                                Code = item.Employee != null ? item.Employee.EmpCode : null,
                                Name = item.Employee != null && item.Employee.EmpName != null ? item.Employee.EmpName.FullNameFML : null,
                                //    JoiningDate = item.Employee.ServiceBookDates.JoiningDate != null ? item.Employee.ServiceBookDates.JoiningDate.Value.ToString("dd/MM/yyyy") : null,
                                //    Job = item.Employee.FuncStruct != null && item.Employee.FuncStruct.Job != null ? item.Employee.FuncStruct.Job.Name : null,
                                //   Grade = item.Employee.PayStruct != null && item.Employee.PayStruct.Grade != null ? item.Employee.PayStruct.Grade.Name : null,
                                Location = item.Employee.GeoStruct != null && item.Employee.GeoStruct.Location != null && item.Employee.GeoStruct.Location.LocationObj != null ? item.Employee.GeoStruct.Location.LocationObj.LocDesc : null,
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

                                     select new[] { null, Convert.ToString(c.Id), c.Employee.EmpCode, };
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



        public ActionResult GetLookupHotelEligibilityPolicy(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.HotelEligibilityPolicy.ToList();
                IEnumerable<HotelEligibilityPolicy> all;
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.HotelEligibilityPolicy.ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }

                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetTravel_Hotel_Booking(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.TravelHotelBooking.Include(e => e.HotelEligibilityPolicy).ToList();
                // IEnumerable<TravelHotelBooking> all;
                if (SkipIds != null)
                {
                    foreach (int a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.TravelHotelBooking.ToList();
                        else
                            fall = fall.Where(e => e.Id != a).ToList();

                    }
                }

                var list1 = db.LTCSettlementClaim.Where(e => e.Travel_Hotel_Booking.Count() > 0).ToList().SelectMany(e => e.Travel_Hotel_Booking);
                var list2 = db.DIARYSettlementClaim.Where(e => e.Travel_Hotel_Booking.Count() > 0).ToList().SelectMany(e => e.Travel_Hotel_Booking);
                var list3 = fall.Except(list1).Except(list2);

                var r = (from ca in list3 select new { srno = ca.Id, lookupvalue = ca.HotelName + ", StartDate :" + ca.StartDate.Value.ToString("dd/MM/yyyy") }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult TravelModeEligibilityPolicylistd(string data, string data2, string DiaryType)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //SelectList s = (SelectList)null;
                var selected = data2 != "" ? Convert.ToInt32(data2) : 0;
                List<TravelModeEligibilityPolicy> TransMode = new List<TravelModeEligibilityPolicy>();
                if (data != "" && data != null)
                {
                    int empid = Convert.ToInt32(data);
                    int DiaryTypeId = Convert.ToInt32(DiaryType);
                    var EmpLTCStruct_Details = db.EmployeeLTCStruct
                        .Include(e => e.EmployeeLTCStructDetails)
                        .Include(e => e.EmployeeLTCStructDetails.Select(r => r.LTCFormula))
                        .Include(e => e.EmployeeLTCStructDetails.Select(r => r.LTCFormula.TravelModeEligibilityPolicy))
                        .Where(e => e.EmployeePayroll.Employee_Id == empid && e.EndDate == null).FirstOrDefault();

                    var travelmodlist = EmpLTCStruct_Details.EmployeeLTCStructDetails.Select(r => r.LTCFormula.TravelModeEligibilityPolicy).ToList();


                    if (travelmodlist.Count() > 0)
                    {
                        foreach (var item in travelmodlist)
                        {
                            if (item.FirstOrDefault() != null)
                            {
                                foreach (var item1 in item)
                                {
                                    var BMSModuleAssign = db.BMSModuleTypePolicyAssignment.Include(e => e.TravelModeEligibilityPolicy).Where(e => e.PolicyType_Id == DiaryTypeId).FirstOrDefault();
                                    if (BMSModuleAssign != null && BMSModuleAssign.TravelModeEligibilityPolicy.Count() > 0)
                                    {
                                        foreach (var item2 in BMSModuleAssign.TravelModeEligibilityPolicy)
                                        {
                                            if (item2.Id == item1.Id)
                                            {
                                                TransMode.Add(item1);
                                            }
                                            
                                        }
                                    }
                                    
                                }
                            }

                        }
                    }

                }
                SelectList s = new SelectList(TransMode, "Id", "TA_TM_Elligibilty_Code", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }

        
    }

    public class CASHSettings
    {
        private string CompCode;
        private string Type;
        private string InVisibleControlName;
        private string DisbleControlName;
        private string VisibleControlName;

        public CASHSettings(IDictionary<string, string> settinigs)
        {
            this.Type = settinigs.First(x => x.Key.Equals("Type")).Value;
            this.CompCode = settinigs.First(x => x.Key.Equals("CompCode")).Value;
            this.InVisibleControlName = settinigs.First(x => x.Key.Equals("InVisibleControlName")).Value;
            this.DisbleControlName = settinigs.First(x => x.Key.Equals("DisbleControlName")).Value;
            this.VisibleControlName = settinigs.First(x => x.Key.Equals("VisibleControlName")).Value;
        }

        public string CCode { get { return CompCode; } }
        public string DType { get { return Type; } }
        public string DInVisibleControlName { get { return InVisibleControlName; } }
        public string DDisbleControlName { get { return DisbleControlName; } }
        public string DVisibleControlName { get { return VisibleControlName; } }

    }

    public class WOSettings
    {
        private string CompCode;
        private string Type;
        private string InVisibleControlName;
        private string DisbleControlName;
        private string VisibleControlName;

        public WOSettings(IDictionary<string, string> settinigs)
        {
            this.Type = settinigs.First(x => x.Key.Equals("Type")).Value;
            this.CompCode = settinigs.First(x => x.Key.Equals("CompCode")).Value;
            this.InVisibleControlName = settinigs.First(x => x.Key.Equals("InVisibleControlName")).Value;
            this.DisbleControlName = settinigs.First(x => x.Key.Equals("DisbleControlName")).Value;
            this.VisibleControlName = settinigs.First(x => x.Key.Equals("VisibleControlName")).Value;
        }

        public string CCode { get { return CompCode; } }
        public string DType { get { return Type; } }
        public string DInVisibleControlName { get { return InVisibleControlName; } }
        public string DDisbleControlName { get { return DisbleControlName; } }
        public string DVisibleControlName { get { return VisibleControlName; } }

    }

    public class WOWORKSettings
    {
        private string CompCode;
        private string Type;
        private string InVisibleControlName;
        private string DisbleControlName;
        private string VisibleControlName;

        public WOWORKSettings(IDictionary<string, string> settinigs)
        {
            this.Type = settinigs.First(x => x.Key.Equals("Type")).Value;
            this.CompCode = settinigs.First(x => x.Key.Equals("CompCode")).Value;
            this.InVisibleControlName = settinigs.First(x => x.Key.Equals("InVisibleControlName")).Value;
            this.DisbleControlName = settinigs.First(x => x.Key.Equals("DisbleControlName")).Value;
            this.VisibleControlName = settinigs.First(x => x.Key.Equals("VisibleControlName")).Value;
        }

        public string CCode { get { return CompCode; } }
        public string DType { get { return Type; } }
        public string DInVisibleControlName { get { return InVisibleControlName; } }
        public string DDisbleControlName { get { return DisbleControlName; } }
        public string DVisibleControlName { get { return VisibleControlName; } }

    }

    public class OFFICEWORKSettings
    {
        private string CompCode;
        private string Type;
        private string InVisibleControlName;
        private string DisbleControlName;
        private string VisibleControlName;

        public OFFICEWORKSettings(IDictionary<string, string> settinigs)
        {
            this.Type = settinigs.First(x => x.Key.Equals("Type")).Value;
            this.CompCode = settinigs.First(x => x.Key.Equals("CompCode")).Value;
            this.InVisibleControlName = settinigs.First(x => x.Key.Equals("InVisibleControlName")).Value;
            this.DisbleControlName = settinigs.First(x => x.Key.Equals("DisbleControlName")).Value;
            this.VisibleControlName = settinigs.First(x => x.Key.Equals("VisibleControlName")).Value;
        }

        public string CCode { get { return CompCode; } }
        public string DType { get { return Type; } }
        public string DInVisibleControlName { get { return InVisibleControlName; } }
        public string DDisbleControlName { get { return DisbleControlName; } }
        public string DVisibleControlName { get { return VisibleControlName; } }

    }

    public class OTHERSettings
    {
        private string CompCode;
        private string Type;
        private string InVisibleControlName;
        private string DisbleControlName;
        private string VisibleControlName;

        public OTHERSettings(IDictionary<string, string> settinigs)
        {
            this.Type = settinigs.First(x => x.Key.Equals("Type")).Value;
            this.CompCode = settinigs.First(x => x.Key.Equals("CompCode")).Value;
            this.InVisibleControlName = settinigs.First(x => x.Key.Equals("InVisibleControlName")).Value;
            this.DisbleControlName = settinigs.First(x => x.Key.Equals("DisbleControlName")).Value;
            this.VisibleControlName = settinigs.First(x => x.Key.Equals("VisibleControlName")).Value;
        }

        public string CCode { get { return CompCode; } }
        public string DType { get { return Type; } }
        public string DInVisibleControlName { get { return InVisibleControlName; } }
        public string DDisbleControlName { get { return DisbleControlName; } }
        public string DVisibleControlName { get { return VisibleControlName; } }

    }
}