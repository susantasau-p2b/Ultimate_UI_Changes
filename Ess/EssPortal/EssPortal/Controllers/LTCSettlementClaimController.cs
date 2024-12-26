///
/// Created By Anandrao 
/// 

using EssPortal.App_Start;
using Payroll;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.Entity;
using EssPortal.Models;
using P2b.Global;
using EssPortal.Security;
using System.Transactions;
using System.Text;

namespace EssPortal.Controllers
{
    public class LTCSettlementClaimController : Controller
    {
        //
        // GET: /LTCSettlementClaim/
        public ActionResult Index()
        {
            return View("~/Views/LTCSettlementClaim/Index.cshtml");
        }


        // GET: /_JourneyDetails/
        public ActionResult Partial_JourneyDetails()
        {
            return View("~/Views/Shared/_JourneyDetails.cshtml");
        }

        // GET: /_TravelHotelBooking/
        public ActionResult Partial_TravelHotelBooking()
        {
            return View("~/Views/Shared/_TravelHotelBooking.cshtml");
        }

        public ActionResult LtcSettlementClaimPartialSanction()
        {
            return View("~/Views/Shared/_LtcSettlementClaimOnSanction.cshtml");
        }
        public class returnEditClass
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
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                //var empltcblockdata = db.EmpLTCBlockT.Select(e => e.LTCSettlementClaim.Where(t => t.Id == data)).ToList();

                var q1 = db.JourneyDetails.Include(e => e.JourneyObject).Include(e => e.TravelModeEligibilityPolicy)
                    .Include(e => e.TravelModeEligibilityPolicy)
                     .Include(e => e.TravelModeEligibilityPolicy.TravelMode)
                    .Where(e => e.Id == data).Select(x => new
                    {
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

                List<returnEditClass> oreturnEditClass = new List<returnEditClass>();



                var k = db.JourneyDetails.Include(e => e.JourneyObject)
                   .Where(e => e.Id == data && e.JourneyObject.Count > 0).ToList();
                foreach (var e in k)
                {
                    oreturnEditClass.Add(new returnEditClass
                    {
                        JourneyObject_Id = e.JourneyObject.Select(a => a.Id.ToString()).ToArray(),
                        JourneyObjectFullDetails = e.JourneyObject.Select(a => a.PlaceFrom + " " + a.PlaceTo + " " + a.FromDate + "" + a.ToDate).ToArray()
                    });
                }

                var y = db.JourneyDetails.Include(e => e.FamilyDetails)
                      .Where(e => e.Id == data && e.FamilyDetails.Count > 0).ToList();
                foreach (var e in k)
                {
                    if (e.FamilyDetails != null)
                    {
                        oreturnEditClass.Add(new returnEditClass
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
                        oreturnEditClass.Add(new returnEditClass
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
                        oreturnEditClass.Add(new returnEditClass
                        {
                            TravelHotelBooking_Id = e.Travel_Hotel_Booking.Select(a => a.Id.ToString()).ToArray(),
                            TravelHotelBookingFullDetails = e.Travel_Hotel_Booking.Select(a => a.StartDate + " " + a.EndDate + " " + a.HotelName + " " + a.BillAmount).ToArray()
                        });
                    }

                }


                return Json(new Object[] { q1, oreturnEditClass, "", JsonRequestBehavior.AllowGet });

                //return Json(new Object[] { Q, return_data, return_dataDoc, JsonRequestBehavior.AllowGet });
                //return Json(new { data = Q }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult TravelModeEligibilityPolicylist(string data, string data2)
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

        public ActionResult TravelModeEligibilityPolicylistd(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //SelectList s = (SelectList)null;
                var selected = data2 != "" ? Convert.ToInt32(data2) : 0;
                List<TravelModeEligibilityPolicy> TransMode = new List<TravelModeEligibilityPolicy>();
                if (data != "" && data != null)
                {
                    int empid = Convert.ToInt32(data);

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
                                    TransMode.Add(item1);
                                }
                            }

                        }
                    }

                }
                SelectList s = new SelectList(TransMode, "Id", "TA_TM_Elligibilty_Code", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }



        public class GetTadasettlamt //childgrid
        {

            public double Elligible_BillAmount { get; set; }

        }



        [HttpPost]
        public ActionResult GetELGAmount(FormCollection form, int data)
        {

            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {


                    string startdate = form["StartDate"] == "0" ? "" : form["StartDate"];
                    string Enddate = form["EndDate"] == "0" ? "" : form["EndDate"];
                    int HotelEligibilityPolicyList = form["HotelEligibilityPolicyList"] == "0" ? 0 : Convert.ToInt32(form["HotelEligibilityPolicyList"]);

                    DateTime Bstartdate = Convert.ToDateTime(startdate);
                    DateTime BEnddate = Convert.ToDateTime(Enddate);
                    int bdays = (BEnddate.Date - Bstartdate.Date).Days + 1;
                    string NoOfRooms = form["NoOfRooms"] == "0" ? "" : form["NoOfRooms"];

                    var hotelelgpolicy = db.HotelEligibilityPolicy.Where(e => e.Id == HotelEligibilityPolicyList).FirstOrDefault();
                    //Elligible_BillAmount

                    double ElligibleBillAmount = 0;
                    ElligibleBillAmount = (hotelelgpolicy.Lodging_Eligible_Amt_PerDay + hotelelgpolicy.Food_Eligible_Amt_PerDay) * bdays;


                    List<GetTadasettlamt> returndata = new List<GetTadasettlamt>();

                    returndata.Add(new GetTadasettlamt
                    {
                        Elligible_BillAmount = ElligibleBillAmount,


                    });
                    return Json(returndata, JsonRequestBehavior.AllowGet);

                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }



        public JsonResult GetEmpLTCBlock(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //var a = db.PayScaleAgreement.Find(int.Parse(data));
                int Emp_Id = int.Parse(data);
                var a = db.EmployeePayroll.Include(e => e.EmpLTCBlock)
                    .Include(e => e.EmpLTCBlock.Select(y => y.EmpLTCBlockT))
                    .Where(e => e.Id == Emp_Id).AsNoTracking().SingleOrDefault().EmpLTCBlock.Where(e => e.IsBlockClose == false).FirstOrDefault();

                var b = a.EmpLTCBlockT.Where(e => e.IsBlockClose == false).OrderBy(e => e.BlockPeriodStart).FirstOrDefault();

                return Json(b, JsonRequestBehavior.AllowGet);
            }
        }



        public ActionResult GetAdavanceClaimLKDetails(List<int> SkipIds, string EmpId, string EmpLTCBlockId)
        {
            int empids = Convert.ToInt32(EmpId);
            int EmpLtcBlockid = Convert.ToInt32(EmpLTCBlockId);
            using (DataBaseContext db = new DataBaseContext())
            {
                var OEMPLTCBlock = db.EmployeePayroll.Include(e => e.Employee).Where(e => e.Employee.Id == empids).Include(e => e.EmpLTCBlock)
                             .Include(e => e.EmpLTCBlock.Select(r => r.EmpLTCBlockT))
                             .Include(e => e.EmpLTCBlock.Select(r => r.EmpLTCBlockT.Select(t => t.LTCAdvanceClaim))).AsNoTracking().FirstOrDefault().EmpLTCBlock;

                var OEMPLTCBlockT = OEMPLTCBlock.SelectMany(e => e.EmpLTCBlockT.Where(t => t.Id == EmpLtcBlockid)).ToList();

                var fall = OEMPLTCBlockT.Where(e => e.Id == EmpLtcBlockid).FirstOrDefault().LTCAdvanceClaim;

                //var fall = db.LTCAdvanceClaim.Include(e => e.RateMaster).ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.LTCAdvanceClaim.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }


                var ret = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                return Json(ret, JsonRequestBehavior.AllowGet);
            }
            // return View();
        }
        
        public ActionResult GetHotelBookingDetails(List<int> SkipIds)
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
                //var list2 = db.TADASettlementClaim.Where(e => e.Travel_Hotel_Booking.Count() > 0).ToList().SelectMany(e => e.Travel_Hotel_Booking);
                var list2 = db.TADASettlementClaim.Include(e => e.JourneyDetails)
                    .Include(e => e.JourneyDetails.Travel_Hotel_Booking)
                    .Where(e => e.JourneyDetails.Travel_Hotel_Booking.Count() > 0).ToList().SelectMany(e => e.JourneyDetails.Travel_Hotel_Booking);
                var list3 = fall.Except(list1).Except(list2);

                var r = (from ca in list3 select new { srno = ca.Id, lookupvalue = ca.HotelName + ", StartDate :" + ca.StartDate.Value.ToString("dd/MM/yyyy") }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult GetJourneyLKDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //List<string> Ids = SkipIds.ToString();
                var fall = db.JourneyDetails.Include(e => e.JourneyObject).ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.JourneyDetails.Include(e => e.JourneyObject).Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }
                var list1 = db.LTCSettlementClaim.Where(e => e.JourneyDetails != null).ToList().Select(e => e.JourneyDetails);
                var list2 = db.TADASettlementClaim.Where(e => e.JourneyDetails != null).ToList().Select(e => e.JourneyDetails);
                var list3 = fall.Except(list1).Except(list2);

                var r = (from ca in list3 select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
            // return View();
        }



        public class LTCSettlClaimChildDataClass //childgrid
        {
            public int Id { get; set; }
            public string BlockPeriod { get; set; }
            public string ReqDate { get; set; }
            public string LTCType { get; set; }
            public double EligibleAmt { get; set; }
            public double AdvanceAmt { get; set; }
            public string Remark { get; set; }
            public double LTCEncashAmount { get; set; }
            public double LTC_Sanction_Amt { get; set; }
            public double LTC_Settle_Amt { get; set; }
            public double LTCAdvAmt { get; set; }
        }

        public class Getltcsettlamt //childgrid
        {

            public double AdvanceAmt { get; set; }
            public double LTCClaimamt { get; set; }
            public double LTCElgmamt { get; set; }
            public double SanctionAmt { get; set; }
            public double SettlementAmt { get; set; }
        }


        public ActionResult Get_LTCSettlementClaim(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var db_data = db.EmployeePayroll
                        .Include(e => e.EmpLTCBlock)
                        .Include(e => e.EmpLTCBlock.Select(a => a.EmpLTCBlockT))
                        .Include(e => e.EmpLTCBlock.Select(a => a.EmpLTCBlockT.Select(b => b.LTCSettlementClaim)))
                        .Include(e => e.EmpLTCBlock.Select(a => a.EmpLTCBlockT.Select(b => b.LTCSettlementClaim.Select(c => c.LTC_TYPE))))
                        .Where(e => e.Id == data).FirstOrDefault();
                    if (db_data.EmpLTCBlock != null)
                    {
                        List<LTCSettlClaimChildDataClass> returndata = new List<LTCSettlClaimChildDataClass>();
                        var LTCSetteleList = db_data.EmpLTCBlock;
                        foreach (var item in db_data.EmpLTCBlock.OrderByDescending(e => e.Id))
                        {
                            foreach (var item1 in item.EmpLTCBlockT)
                            {
                                if (item1.LTCSettlementClaim.Count() > 0)
                                {
                                    foreach (var item2 in item1.LTCSettlementClaim)
                                    {
                                        returndata.Add(new LTCSettlClaimChildDataClass
                                        {
                                            Id = item2.Id,
                                            // BlockPeriod = item1.FullDetails,
                                            ReqDate = item2.DateOfAppl.Value != null ? item2.DateOfAppl.Value.ToShortDateString() : "",
                                            LTCType = item2.LTC_TYPE != null ? item2.LTC_TYPE.LookupVal.ToString() : null,
                                            EligibleAmt = item2.LTC_Eligible_Amt,
                                            AdvanceAmt = item2.LTCAdvAmt,
                                            Remark = item2.Remark,
                                            LTCEncashAmount = item2.EncashmentAmount == null ? 0 : item2.EncashmentAmount,
                                            LTC_Sanction_Amt = item2.LTC_Sanction_Amt == null ? 0 : item2.LTC_Sanction_Amt,
                                            LTC_Settle_Amt = item2.LTC_Settle_Amt == null ? 0 : item2.LTC_Settle_Amt,
                                            LTCAdvAmt = item2.LTCAdvAmt == null ? 0 : item2.LTCAdvAmt
                                        });
                                    }
                                }
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
                    throw e;
                }
            }
        }


        public class HotelElData //childgrid
        {
            public int Id { get; set; }
            public string FullDetails { get; set; }

        }


        public ActionResult GetHotelEligibilityPolicyDetails(List<int> SkipIds, string Empid)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //List<string> Ids = SkipIds.ToString();
                int em = Convert.ToInt32(Empid);
                var oemployee = db.EmployeePayroll
                     .Include(e => e.Employee.EmpOffInfo)
                     .Include(e => e.Employee.EmpName)
                     .Include(e => e.Employee.ServiceBookDates).Where(e => e.Employee.Id == em).FirstOrDefault();

                var LTCStruct = db.EmployeeLTCStruct
                                       .Include(e => e.EmployeeLTCStructDetails)
                                        .Include(e => e.EmployeeLTCStructDetails.Select(t => t.PolicyName))
                                         .Include(e => e.EmployeeLTCStructDetails.Select(t => t.LTCFormula))
                                          .Include(e => e.EmployeeLTCStructDetails.Select(t => t.LTCFormula.HotelEligibilityPolicy))
                                       .Include(e => e.EmployeeLTCStructDetails.Select(t => t.LTCPolicyAssignment))
                                       .Include(e => e.EmployeeLTCStructDetails.Select(t => t.LTCPolicyAssignment.PayScaleAgreement))
                                       .Where(e => e.EmployeePayroll.Id == oemployee.Id && e.EndDate == null).AsNoTracking().OrderBy(e => e.Id).FirstOrDefault();


                List<HotelElData> OList = new List<HotelElData>();
                if (LTCStruct != null)
                {

                    var OEmpSalStructDet = LTCStruct.EmployeeLTCStructDetails.Where(e => e.PolicyName.LookupVal.ToUpper() == "HOTELELIGIBILITY").Select(x => x.LTCFormula.HotelEligibilityPolicy).ToList();

                    foreach (var i in OEmpSalStructDet)
                    {
                        foreach (var j in i)
                        {
                            OList.Add(new HotelElData
                            {
                                Id = j.Id,
                                FullDetails = j.FullDetails
                            });
                        }

                    }

                }
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (OList != null)
                            OList = OList.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }


                var r = (from ca in OList select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
            // return View();
        }


        public class LvNewReqData
        {
            public int Id { get; set; }
            public string FullDetails { get; set; }

        }

        [HttpPost]
        public ActionResult GetLVReqLKDetails(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                int Id = int.Parse(data);
                int block_Id = int.Parse(data2);
                int Count = 0;
                List<LvNewReqData> model = new List<LvNewReqData>();
                LvNewReqData view = null;

                var OEmployeeLeave = db.EmployeeLeave.Include(e => e.LvNewReq)
                                        .Include(e => e.LvCancelReq)
                                 .Include(e => e.LvCancelReq.Select(t => t.WFStatus))
                               .Include(e => e.LvCancelReq.Select(t => t.LvNewReq))
                    .Include(e => e.LvNewReq.Select(r => r.LeaveHead))
                    .Include(e => e.LvNewReq.Select(r => r.WFStatus))
                    .Include(e => e.LvNewReq.Select(r => r.LeaveCalendar))
                  .Where(e => e.Employee.Id == Id).SingleOrDefault();

                var lvcancelreqidslist = OEmployeeLeave.LvCancelReq.Where(e => e.WFStatus.LookupVal == "0" || e.WFStatus.LookupVal == "1").Select(e => e.LvNewReq.Id).ToList();
                // var DefCal = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "LEAVECALENDAR" && e.Default == true).SingleOrDefault();
                //Cl max date
                List<int> lvcode = OEmployeeLeave.LvNewReq.Where(e => e.LvCreditDate != null).Select(e => e.LeaveHead.Id).Distinct().ToList();
                foreach (var item in lvcode)
                {
                    DateTime? lvcrdate = OEmployeeLeave.LvNewReq.Where(e => e.LeaveHead.Id == item && e.LvCreditDate != null).Max(e => e.LvCreditDate.Value);
                    if (lvcrdate != null)
                    {
                        EmpLTCBlockT OEmpLTCBlockT = db.EmpLTCBlockT.Find(block_Id);

                        DateTime? BlockStartchk = OEmpLTCBlockT.BlockPeriodStart;
                        DateTime? BlockEndchk = OEmpLTCBlockT.BlockPeriodEnd;
                        var OEmpLTCBlock = db.EmployeePayroll.Include(e => e.EmpLTCBlock).Where(e => e.Employee.Id == Id).SingleOrDefault();
                        var OEmpLTCBlockper = OEmpLTCBlock.EmpLTCBlock.Where(e => e.BlockStart >= BlockStartchk.Value && BlockEndchk.Value <= e.BlockEnd).FirstOrDefault();

                        if (OEmpLTCBlockper != null)
                        {
                            DateTime? BlockStart = OEmpLTCBlockper.BlockStart;
                            DateTime? BlockEnd = OEmpLTCBlockper.BlockEnd;

                            //DateTime? BlockStart = OEmpLTCBlockT.BlockPeriodStart;
                            //DateTime? BlockEnd = OEmpLTCBlockT.BlockPeriodEnd;
                            //DateTime? Lvyearfrom = lvcrdate;
                            //DateTime? LvyearTo = Lvyearfrom.Value.AddDays(-1);
                            //LvyearTo = LvyearTo.Value.AddYears(1);

                            //var OEmpLeave = OEmployeeLeave.LvNewReq.Where(e => e.LeaveCalendar.Id == DefCal.Id && e.IsCancel == false && e.TrClosed == true && e.LvOrignal == null && e.WFStatus.LookupVal != "2").ToList();

                            //var query = OEmployeeLeave.LvNewReq.Where(e => e.LeaveCalendar.Id == DefCal.Id && e.IsCancel == true && e.TrClosed == true && e.LvOrignal != null).ToList();
                            var OEmpLeave = OEmployeeLeave.LvNewReq.Where(e => !lvcancelreqidslist.Contains(e.Id) && e.LeaveHead.Id == item && e.IsCancel == false && e.TrClosed == true && e.LvOrignal == null && e.WFStatus.LookupVal != "2" && e.ReqDate >= BlockStart.Value && e.ReqDate <= BlockEnd.Value).ToList();

                            var query = OEmployeeLeave.LvNewReq.Where(e => e.LeaveHead.Id == item && e.IsCancel == true && e.TrClosed == true && e.LvOrignal != null && e.ReqDate >= BlockStart.Value && e.ReqDate <= BlockEnd.Value).ToList();


                            foreach (var a in OEmpLeave)
                            {
                                //if (query.Count > 0)
                                //{
                                Count = 0;
                                foreach (var b in query)
                                {
                                    if (b.LvOrignal.Id == a.Id)
                                    {
                                        Count = 1;
                                    }
                                }
                                if (Count == 0)
                                {

                                    view = new LvNewReqData()
                                    {
                                        Id = a.Id,
                                        FullDetails = a.FullDetails
                                    };
                                    model.Add(view);
                                }
                                //}
                                //else
                                //{
                                //    view = new LvNewReqData()
                                //    {
                                //        Id = a.Id,
                                //        FullDetails = a.FullDetails
                                //    };
                                //    model.Add(view);
                                //}
                            }
                        }
                    }
                }

                var selected = "";
                if (data2 != null)
                {
                    selected = data2;
                }
                if (model != null && model.Count() > 0)
                {
                    SelectList s = new SelectList(model, "Id", "FullDetails", selected);
                    return Json(s, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
            }
        }


        [HttpPost]
        public ActionResult Create(LTCSettlementClaim c, FormCollection form)
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
                    int Emp = form["EmpLvnereq_Id"] == "0" ? 0 : Convert.ToInt32(form["EmpLvnereq_Id"]);
                    string ltcadvanceclaimlist = form["LTCAdvanceClaimList"] == "0" ? "" : form["LTCAdvanceClaimList"];
                    string ltctypelist = form["LTCTypelist"] == "0" ? "" : form["LTCTypelist"];
                    string lvreqlist = form["LvReqList"] == "0" ? "" : form["LvReqList"];
                    string journewdetailslist = form["JourneyDetailsList"] == "0" ? "" : form["JourneyDetailsList"];
                    string hotelbookinglist = form["HotelBookingDetailsList"] == "0" ? "" : form["HotelBookingDetailsList"];
                    string LtcBlock = form["BlockId"] == "0" ? "" : form["BlockId"];


                    //string ddlIncharge = form["ddlIncharge"] == "" ? null : form["ddlIncharge"];
                    string ddlIncharge = form["Incharge_id"] == "" ? null : form["Incharge_id"];

                    if (c.LTC_Settle_Amt > c.LTC_Eligible_Amt)
                    {
                        Msg.Add("Settlement amount Should not be greater than Eligible amount.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    if (c.LTC_Sanction_Amt > c.LTC_Eligible_Amt)
                    {
                        Msg.Add("Sanction amount Should not be greater than Eligible amount.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    if (c.LTC_Settle_Amt > c.LTC_Sanction_Amt)
                    {
                        Msg.Add("Settlement amount Should not be greater than sanction amount.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    if (c.LTC_Settle_Amt == 0)
                    {
                        Msg.Add("Settlement amount Should not be 0.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    if (c.LTC_Sanction_Amt == 0)
                    {
                        Msg.Add("sanction amount Should not be 0.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }


                    if (ltcadvanceclaimlist != null && ltcadvanceclaimlist != "")
                    {
                        int ltcadvanceid = Convert.ToInt32(ltcadvanceclaimlist);
                        var value = db.LTCAdvanceClaim.Where(e => e.Id == ltcadvanceid).SingleOrDefault();
                        c.LTCAdvanceClaim = value;

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

                    if (lvreqlist != null)
                    {
                        if (lvreqlist != "")
                        {
                            int ids = Convert.ToInt32(lvreqlist);
                            var val = db.LvNewReq.Where(e => e.Id == ids).SingleOrDefault();
                            c.LvNewReq = val;
                        }
                    }

                    if (ltctypelist != null && ltctypelist != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(ltctypelist));
                        c.LTC_TYPE = val;
                    }
                    var employeedata = db.Employee.Include(e => e.GeoStruct).Include(e => e.PayStruct).Include(e => e.FuncStruct).Where(e => e.Id == Emp).SingleOrDefault();
                    c.PayStruct = employeedata.PayStruct;
                    c.GeoStruct = employeedata.GeoStruct;
                    c.FuncStruct = employeedata.FuncStruct;
                    int Blockid = int.Parse(LtcBlock);
                    EmpLTCBlockT OEmpLTCBlockT = db.EmpLTCBlockT.Find(Blockid);

                    var a = db.EmployeePayroll.Include(e => e.EmpLTCBlock).Include(e => e.Employee)
                             .Include(e => e.EmpLTCBlock.Select(y => y.EmpLTCBlockT))
                             .Where(e => e.Employee.Id == Emp).AsNoTracking().SingleOrDefault().EmpLTCBlock.Where(e => e.IsBlockClose == false).FirstOrDefault();

                    var test = a.EmpLTCBlockT.Where(e => e.BlockPeriodStart >= OEmpLTCBlockT.BlockPeriodStart
                       && e.BlockPeriodEnd <= OEmpLTCBlockT.BlockPeriodEnd).FirstOrDefault();

                    var LtcStruct = db.EmployeeLTCStruct
                                        .Include(e => e.EmployeeLTCStructDetails)
                                         .Include(e => e.EmployeeLTCStructDetails.Select(t => t.PolicyName))
                                         .Include(e => e.EmployeeLTCStructDetails.Select(t => t.LTCFormula))
                                          .Include(e => e.EmployeeLTCStructDetails.Select(t => t.LTCFormula.GlobalLTCBlock))
                                        .Include(e => e.EmployeeLTCStructDetails.Select(t => t.LTCPolicyAssignment))
                                        .Include(e => e.EmployeeLTCStructDetails.Select(t => t.LTCPolicyAssignment.PayScaleAgreement))
                                        .Where(e => e.EmployeePayroll.Employee.Id == Emp && e.EndDate == null).AsNoTracking().OrderBy(e => e.Id).FirstOrDefault();
                    var OEmpLtcStructDet = LtcStruct.EmployeeLTCStructDetails.Where(e => e.PolicyName.LookupVal.ToUpper() == "LTCBLOCK").Select(r => r.LTCFormula).FirstOrDefault();
                    var GLTC = OEmpLtcStructDet.GlobalLTCBlock.FirstOrDefault();
                    int Blockid1 = GLTC.Id;

                    int NoOFTimes = db.GlobalLTCBlock.Where(e => e.Id == Blockid1).FirstOrDefault().NoOfTimes;

                    // yearlypayment

                    var emppi = db.EmployeePayroll.Where(e => e.Employee.Id == Emp).SingleOrDefault();
                    int salhdid = 0;
                    var fyyr = db.Calendar.Include(x => x.Name).Where(x => x.Name.LookupVal.ToUpper() == "FINANCIALYEAR" && x.Default == true).SingleOrDefault();

                    var QEmpSalStruct1 = db.EmpSalStruct//.Where(e => e.EmployeePayroll_Id == OEmployeePayrollId && e.EffectiveDate >= comparedate)
                                          .Include(e => e.EmpSalStructDetails)
                                          .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead))
                                          .Include(e => e.EmpSalStructDetails.Select(q => q.SalHeadFormula))
                                          .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.Frequency))
                                          .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.Type))
                                          .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.RoundingMethod))
                                          .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.SalHeadOperationType))
                                          .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.LvHead));//.ToList();
                    var EmpSalStructTotal = QEmpSalStruct1.Where(e => e.EmployeePayroll.Id == emppi.Id && e.EndDate == null)
                                                  .ToList();
                    var OEmpSalStruct = EmpSalStructTotal.Where(e => e.EndDate == null).SingleOrDefault();
                    if (OEmpSalStruct != null)
                    {

                        salhdid = OEmpSalStruct.EmpSalStructDetails
                           .Where(s => s.SalaryHead.Frequency.LookupVal.ToUpper() == "YEARLY"
                        && s.SalaryHead.Type.LookupVal.ToUpper() == "EARNING"
                        && (s.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "LTC"
                        )).FirstOrDefault() != null ? OEmpSalStruct.EmpSalStructDetails
                           .Where(s => s.SalaryHead.Frequency.LookupVal.ToUpper() == "YEARLY"
                        && s.SalaryHead.Type.LookupVal.ToUpper() == "EARNING"
                        && (s.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "LTC"
                        )).FirstOrDefault().SalaryHead.Id : 0;
                    }


                    if (salhdid == 0)
                    {
                        Msg.Add("Please Define LTC in salary Structure.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }



                    if (test.LTCAdvanceClaim != null)
                    {
                        Msg.Add("Record Already Exists.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    FunctAllWFDetails oAttWFDetails = new FunctAllWFDetails
                    {
                        WFStatus = 0,
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
                            LTCSettlementClaim ltcsettlement = new LTCSettlementClaim()
                            {
                                DateOfAppl = c.DateOfAppl,
                                EncashmentAmount = c.EncashmentAmount,
                                JourneyDetails = c.JourneyDetails,
                                Travel_Hotel_Booking = c.Travel_Hotel_Booking,
                                NoOfDays = c.NoOfDays,
                                LvNewReq = c.LvNewReq,
                                LTC_TYPE = c.LTC_TYPE,
                                LTCAdvanceClaim = c.LTCAdvanceClaim,
                                LTC_Claim_Amt = c.LTC_Claim_Amt,
                                LTC_Eligible_Amt = c.LTC_Eligible_Amt,
                                LTCAdvAmt = c.LTCAdvAmt,
                                LTC_Settle_Amt = c.LTC_Settle_Amt,
                                LTC_Sanction_Amt = c.LTC_Sanction_Amt,
                                Remark = c.Remark,
                                FuncStruct = c.FuncStruct,
                                GeoStruct = c.GeoStruct,
                                PayStruct = c.PayStruct,
                                WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "1").FirstOrDefault(),//db.LookupValue.Where(e => e.LookupVal == "1").SingleOrDefault(),
                                LTCWFDetails = oAttWFDetails_List,
                                DBTrack = c.DBTrack
                            };

                            db.LTCSettlementClaim.Add(ltcsettlement);
                            db.SaveChanges();

                            // yearly payment strat
                            YearlyPaymentT ObjYPT = new YearlyPaymentT();
                            {
                                ObjYPT.SalaryHead = db.SalaryHead.Find(salhdid);
                                ObjYPT.AmountPaid = c.LTC_Settle_Amt;
                                ObjYPT.FromPeriod = a.BlockStart;
                                ObjYPT.ToPeriod = a.BlockEnd;
                                ObjYPT.ProcessMonth = Convert.ToDateTime(c.DateOfAppl).ToString("MM/yyyy");
                                ObjYPT.PayMonth = Convert.ToDateTime(c.DateOfAppl).ToString("MM/yyyy");
                                ObjYPT.ReleaseDate = c.DateOfAppl;
                                ObjYPT.ReleaseFlag = true;
                                ObjYPT.TDSAmount = 0;
                                ObjYPT.OtherDeduction = 0;
                                ObjYPT.FinancialYear = fyyr;
                                ObjYPT.DBTrack = c.DBTrack;
                                ObjYPT.FuncStruct = c.FuncStruct;
                                ObjYPT.GeoStruct = c.GeoStruct;
                                ObjYPT.PayStruct = c.PayStruct;

                            }
                            List<YearlyPaymentT> OYrlyPaylist = new List<YearlyPaymentT>();
                            db.YearlyPaymentT.Add(ObjYPT);
                            db.SaveChanges();
                            OYrlyPaylist.Add(ObjYPT);

                            //yearly payment end
                            List<LTCSettlementClaim> LtcSettlementlist = new List<LTCSettlementClaim>();
                            var aa = db.EmpLTCBlockT.Include(e => e.LTCSettlementClaim).Where(e => e.Id == test.Id).SingleOrDefault();
                            if (aa.LTCSettlementClaim.Count() > 0)
                            {
                                LtcSettlementlist.AddRange(aa.LTCSettlementClaim);
                            }
                            else
                            {
                                LtcSettlementlist.Add(ltcsettlement);
                            }
                            aa.LTCSettlementClaim = LtcSettlementlist;
                            aa.Occurances = 1;
                            aa.IsBlockClose = true;
                            int TotOcc = a.EmpLTCBlockT.Sum(e => e.Occurances) + aa.Occurances;

                            db.EmpLTCBlockT.Attach(aa);
                            db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            db.Entry(aa).State = System.Data.Entity.EntityState.Detached;

                            if (NoOFTimes == TotOcc)
                            {
                                a.IsBlockClose = true;
                            }
                            a.Occurances = a.Occurances + 1;

                            db.EmpLTCBlock.Attach(a);
                            db.Entry(a).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            db.Entry(a).State = System.Data.Entity.EntityState.Detached;

                            if (NoOFTimes == TotOcc)
                            {
                                GlobalLTCBlock OGlobalLTCBlockT = db.GlobalLTCBlock.Where(e => e.Id == Blockid1).FirstOrDefault();
                                EmpLTCBlock OEmpLTCBlockNew = new EmpLTCBlock();
                                OEmpLTCBlockNew.GlobalLTCBlock = OGlobalLTCBlockT;
                                OEmpLTCBlockNew.BlockStart = a.BlockEnd.Value.AddDays(1);
                                OEmpLTCBlockNew.BlockEnd = OEmpLTCBlockNew.BlockStart.Value.AddYears(OGlobalLTCBlockT.BlockYear).AddDays(-1);

                                DateTime? firstDate = OEmpLTCBlockNew.BlockStart;
                                DateTime? secondDate = OEmpLTCBlockNew.BlockEnd;
                                List<DateTime?> dates = new List<DateTime?>();
                                List<DateTime?> datenew = new List<DateTime?>();
                                int totalYears = secondDate.Value.Year - firstDate.Value.Year;
                                int block = 0;

                                block = (totalYears / OGlobalLTCBlockT.NoOfTimes);

                                OEmpLTCBlockNew.EmpLTCBlockT = null;
                                List<EmpLTCBlockT> OBJ = new List<EmpLTCBlockT>();
                                OEmpLTCBlockNew.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                                for (int j = 1; j <= OGlobalLTCBlockT.NoOfTimes; j++)
                                {
                                    if (OGlobalLTCBlockT.NoOfTimes == 1)
                                    {
                                        dates.Add(firstDate);
                                        datenew.Add(firstDate);
                                        dates.Add(secondDate);
                                        datenew.Add(secondDate);
                                    }

                                    if (j == 1 && OGlobalLTCBlockT.NoOfTimes > 1)
                                    {
                                        dates.Add(firstDate);
                                        datenew.Add(firstDate);
                                        dates.Add(firstDate.Value.Date.AddYears(OGlobalLTCBlockT.NoOfTimes).AddDays(-1));
                                        datenew.Add(firstDate.Value.Date.AddYears(OGlobalLTCBlockT.NoOfTimes).AddDays(-1));
                                    }
                                    if (j != 1 && OGlobalLTCBlockT.NoOfTimes > 1)
                                    {
                                        dates.Add(dates.LastOrDefault().Value.AddDays(1));
                                        datenew.Add(dates.LastOrDefault().Value);
                                        dates.Add(dates.LastOrDefault().Value.AddYears(OGlobalLTCBlockT.NoOfTimes).AddDays(-1));
                                        datenew.Add(dates.LastOrDefault().Value);
                                    }
                                    EmpLTCBlockT OEmpLTCBlockTNew = new EmpLTCBlockT()
                                    {
                                        BlockPeriodStart = datenew[0],
                                        BlockPeriodEnd = datenew[1],
                                        Occurances = 0,
                                        DBTrack = OEmpLTCBlockNew.DBTrack
                                    };
                                    OBJ.Add(OEmpLTCBlockTNew);
                                    datenew.Clear();
                                };


                                //EmpLTCBlock OEmpLTCBlock = new EmpLTCBlock()
                                //{
                                //    BlockEnd = OEmpLTCBlockNew.BlockEnd,
                                //    BlockStart = OEmpLTCBlockNew.BlockStart,
                                //    GlobalLTCBlock = OEmpLTCBlockNew.GlobalLTCBlock,
                                //    Occurances = 0,
                                //    DBTrack = OEmpLTCBlockNew.DBTrack,
                                //    EmpLTCBlockT = OBJ
                                //};
                                OEmpLTCBlockNew.EmpLTCBlockT = OBJ;
                                db.EmpLTCBlock.Add(OEmpLTCBlockNew);
                                db.SaveChanges();




                                //OEmpLTCBlockT.LTCAdvanceClaim = LTCAdvanceClaim;
                                List<EmpLTCBlock> EmpLTCBlocklist = new List<EmpLTCBlock>();
                                var OEmpPayroll = db.EmployeePayroll.Include(e => e.EmpLTCBlock).Include(e => e.YearlyPaymentT).Where(e => e.Employee.Id == Emp).SingleOrDefault();
                                if (OEmpPayroll.EmpLTCBlock.Count() > 0)
                                {
                                    EmpLTCBlocklist.AddRange(OEmpPayroll.EmpLTCBlock);
                                }
                                if (OEmpPayroll.YearlyPaymentT.Count() > 0)
                                {
                                    OYrlyPaylist.AddRange(OEmpPayroll.YearlyPaymentT);
                                }
                                EmpLTCBlocklist.Add(OEmpLTCBlockNew);

                                OEmpPayroll.EmpLTCBlock = EmpLTCBlocklist;
                                //OEmployeePayroll.DBTrack = dbt;
                                OEmpPayroll.YearlyPaymentT = OYrlyPaylist;
                                db.EmployeePayroll.Attach(OEmpPayroll);
                                db.Entry(OEmpPayroll).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(OEmpPayroll).State = System.Data.Entity.EntityState.Detached;
                            }



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
        public ActionResult GetLtcAmount(FormCollection form, int data)
        {

            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string journewdetailslist = form["JourneyDetailsList"] == "0" ? "" : form["JourneyDetailsList"];
                    string ltcadvanceclaimlist = form["LTCAdvanceClaimList"] == "0" ? "" : form["LTCAdvanceClaimList"];
                    string hotelbookinglist = form["HotelBookingDetailsList"] == "0" ? "" : form["HotelBookingDetailsList"];

                    double TAclaimamt = 0;
                    double TAElgamt = 0;
                    double TASettamt = 0;
                    double Hoteltotalamt = 0;
                    double advanceclaimamt = 0;
                    double hotelamount = 0;
                    double hotelelgamount = 0;
                    double foodeligibleamt = 0;
                    double LTCSettleamt = 0;
                    double Lodging_Eligible_Amt_PerDay = 0;
                    double LtcEligibleamt = 0;

                    var QEmpSalStruct1 = db.EmpSalStruct//.Where(e => e.EmployeePayroll_Id == OEmployeePayrollId && e.EffectiveDate >= comparedate)
                                           .Include(e => e.EmpSalStructDetails)
                                           .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead))
                                           .Include(e => e.EmpSalStructDetails.Select(q => q.SalHeadFormula))
                                           .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.Frequency))
                                           .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.Type))
                                           .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.RoundingMethod))
                                           .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.SalHeadOperationType))
                                           .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.LvHead));//.ToList();
                    var EmpSalStructTotal = QEmpSalStruct1.Where(e => e.EmployeePayroll.Id == data && e.EndDate == null)
                                                  .ToList();
                    var OEmpSalStruct = EmpSalStructTotal.Where(e => e.EndDate == null).SingleOrDefault();
                    if (OEmpSalStruct != null)
                    {

                        LtcEligibleamt = OEmpSalStruct.EmpSalStructDetails
                           .Where(s => s.SalaryHead.Frequency.LookupVal.ToUpper() == "YEARLY"
                        && s.SalaryHead.Type.LookupVal.ToUpper() == "EARNING"
                        && (s.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "LTC"

                        )).FirstOrDefault().Amount;
                    }
                    if (ltcadvanceclaimlist != null && ltcadvanceclaimlist != "")
                    {
                        int ltcadvanceid = Convert.ToInt32(ltcadvanceclaimlist);
                        var advanceclaimdata = db.LTCAdvanceClaim.Where(e => e.Id == ltcadvanceid).SingleOrDefault();
                        if (advanceclaimdata != null)
                        {
                            advanceclaimamt = advanceclaimdata.LTCAdvAmt;
                        }
                        // c.LTCAdvanceClaim = value;

                    }
                    if (hotelbookinglist != null)
                    {
                        var ids = Utility.StringIdsToListIds(hotelbookinglist);
                        var travelhotelbookinglist = new List<TravelHotelBooking>();
                        var travelhotelbookinlist = db.TravelHotelBooking.Include(e => e.HotelEligibilityPolicy).Where(e => ids.Contains(e.Id)).ToList();
                        if (travelhotelbookinlist.Count() > 0)
                        {
                            var hoteleligibilitypolicy = travelhotelbookinlist.Where(e => e.HotelEligibilityPolicy != null).Select(e => e.HotelEligibilityPolicy);
                            hotelamount = travelhotelbookinlist.Sum(t => t.BillAmount);
                            hotelelgamount = travelhotelbookinlist.Sum(t => t.Elligible_BillAmount);
                            //if (hoteleligibilitypolicy != null)
                            //{
                            //   // foodeligibleamt = hoteleligibilitypolicy.Sum(e => e.Food_Eligible_Amt_PerDay); Food transaction entry not given so add in bill amount food value

                            //}
                        }
                        //  Hoteltotalamt = hotelamount + foodeligibleamt + Lodging_Eligible_Amt_PerDay;

                    }



                    if (journewdetailslist != null)
                    {
                        if (journewdetailslist != "")
                        {
                            int ids = Convert.ToInt32(journewdetailslist);
                            var JourneyObjdata = db.JourneyDetails.Include(e => e.JourneyObject).Where(e => e.Id == ids).SingleOrDefault();
                            TAclaimamt = JourneyObjdata.JourneyObject.Sum(e => e.TAClaimAmt);
                            TAElgamt = JourneyObjdata.JourneyObject.Sum(e => e.TAElligibleAmt);
                            TASettamt = JourneyObjdata.JourneyObject.Sum(e => e.TASettleAmt);
                            // c.JourneyDetails = val;
                        }
                    }

                    // LTCSettleamt = Hoteltotalamt + TAclaimamt;
                    List<Getltcsettlamt> returndata = new List<Getltcsettlamt>();
                    double FinalLtcEligibleamt = 0;
                    //FinalLtcEligibleamt = (hotelelgamount + TAElgamt) <= (LtcEligibleamt) ? (hotelelgamount + TAElgamt) : (LtcEligibleamt);
                    FinalLtcEligibleamt = LtcEligibleamt;
                    returndata.Add(new Getltcsettlamt
                    {
                        AdvanceAmt = advanceclaimamt,
                        // LTCClaimamt = LTCSettleamt,
                        LTCClaimamt = hotelamount + TAclaimamt,//travel hotel bill amount+journey TAclaim amt
                        // LTCElgmamt = LtcEligibleamt
                        LTCElgmamt = FinalLtcEligibleamt,// travel hotel elg amount+ journey elg amount
                        SanctionAmt = FinalLtcEligibleamt <= (hotelamount + TAclaimamt) ? FinalLtcEligibleamt : (hotelamount + TAclaimamt),// elg amount and claim amount which ever is less
                        SettlementAmt = (FinalLtcEligibleamt <= (hotelamount + TAclaimamt) ? FinalLtcEligibleamt : (hotelamount + TAclaimamt)) - advanceclaimamt,//sanction amount-advance amount


                    });
                    return Json(returndata, JsonRequestBehavior.AllowGet);

                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }




        public class ChildGetLTCsettlementClaimReqClass
        {
            public string LvNewReq { get; set; }
            public string EmpLVid { get; set; }
            public string Status { get; set; }
            public string LvHead_Id { get; set; }
            public string IsClose { get; set; }

        }



        public class GetLTCsettlementClaimReqClass
        {
            public string Emp { get; set; }
            public string ReqDate { get; set; }
            public string LTCType { get; set; }
            public string AdvanceAmount { get; set; }
            public string LTCEncashmentAmount { get; set; }
            public string LTCSettlementAmount { get; set; }
            public string Status { get; set; }


            public ChildGetLTCsettlementClaimReqClass RowData { get; set; }
        }



        public ActionResult GetMyLTCsettlementClaimReq()   /// Get Created Data on Grid
        {
            try
            {
                using (DataBaseContext db = new DataBaseContext())
                {
                    string authority = Convert.ToString(Session["auho"]);

                    List<GetLTCsettlementClaimReqClass> OLTCsettleClaimlist = new List<GetLTCsettlementClaimReqClass>();
                    if (string.IsNullOrEmpty(Convert.ToString(SessionManager.EmpLvId)))
                    {
                        return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                    }
                    var Id = Convert.ToInt32(SessionManager.EmpLvId);

                    var db_data = db.EmployeePayroll
                            .Include(e => e.EmpLTCBlock)
                            .Include(e => e.EmpLTCBlock.Select(a => a.EmpLTCBlockT))
                            .Include(e => e.EmpLTCBlock.Select(a => a.EmpLTCBlockT.Select(b => b.LTCSettlementClaim)))
                            .Include(e => e.EmpLTCBlock.Select(a => a.EmpLTCBlockT.Select(b => b.LTCSettlementClaim.Select(c => c.LTC_TYPE))))
                            .Include(e => e.EmpLTCBlock.Select(a => a.EmpLTCBlockT.Select(b => b.LTCSettlementClaim.Select(c => c.LTCWFDetails))))
                            .Where(e => e.Id == Id).FirstOrDefault();


                    if (db_data.EmpLTCBlock != null)
                    {
                        List<GetLTCsettlementClaimReqClass> returndata = new List<GetLTCsettlementClaimReqClass>();
                        returndata.Add(new GetLTCsettlementClaimReqClass
                        {

                            ReqDate = "Requisition Date",
                            LTCType = "LTCType",
                            AdvanceAmount = "AdvanceAmount",
                            LTCEncashmentAmount = "LTCEncashmentAmount",
                            LTCSettlementAmount = "LTCSettlementAmount",
                            Status = "Status"
                        });


                        foreach (var item in db_data.EmpLTCBlock)
                        {
                            var LTCSettlelist = item.EmpLTCBlockT.ToList();

                            if (LTCSettlelist.Count() > 0)
                            {
                                var LTCsettlementlaimlist = LTCSettlelist.Select(d => d.LTCSettlementClaim).ToList();
                                foreach (var LTCsettleItems in LTCsettlementlaimlist)
                                {
                                    if (LTCsettleItems.Count() > 0)
                                    {
                                        foreach (var LTCWitems in LTCsettleItems)
                                        {
                                            //var LTCWFDetailitems = LTCWitems.LTCWFDetails.ToList();
                                            //foreach (var items in LTCWFDetailitems)
                                            //{
                                            int WfStatusNew = LTCWitems.LTCWFDetails.Select(e => e.WFStatus).LastOrDefault();
                                            string Comments = LTCWitems.LTCWFDetails.Select(e => e.Comments).LastOrDefault();


                                            string StatusNarration = "";
                                            if (WfStatusNew == 0)
                                                StatusNarration = "Applied";
                                            else if (WfStatusNew == 1)
                                                StatusNarration = "Sanctioned";
                                            else if (WfStatusNew == 2)
                                                StatusNarration = "Rejected by Sanction";
                                            else if (WfStatusNew == 3)
                                                StatusNarration = "Approved";
                                            else if (WfStatusNew == 4)
                                                StatusNarration = "Rejected by Approval";
                                            else if (WfStatusNew == 5)
                                                StatusNarration = "Approved By HRM (M)";



                                            if (authority.ToUpper() == "SANCTION" && WfStatusNew == 0)
                                            {
                                                GetLTCsettlementClaimReqClass ObjLTCsettleClaimRequest = new GetLTCsettlementClaimReqClass()
                                                {
                                                    RowData = new ChildGetLTCsettlementClaimReqClass
                                                    {
                                                        LvNewReq = LTCWitems.LvNewReq_Id.ToString(),
                                                        EmpLVid = db_data.Id.ToString()

                                                    },
                                                    ReqDate = LTCWitems.DateOfAppl.Value.ToShortDateString(),
                                                    LTCType = LTCWitems.LTC_TYPE.LookupVal.ToString(),
                                                    LTCEncashmentAmount = LTCWitems.EncashmentAmount.ToString() != "0" ? LTCWitems.EncashmentAmount.ToString() : "0",
                                                    LTCSettlementAmount = LTCWitems.LTC_Settle_Amt.ToString() != "0" ? LTCWitems.LTC_Settle_Amt.ToString() : "0",
                                                    AdvanceAmount = LTCWitems.LTCAdvAmt.ToString() != "0" ? LTCWitems.LTCAdvAmt.ToString() : "0",
                                                    Status = StatusNarration
                                                };
                                                returndata.Add(ObjLTCsettleClaimRequest);
                                            }

                                            else if (authority.ToUpper() == "APPROVAL" && WfStatusNew == 1)
                                            {
                                                GetLTCsettlementClaimReqClass ObjLTCsettleClaimRequest = new GetLTCsettlementClaimReqClass()
                                                {
                                                    RowData = new ChildGetLTCsettlementClaimReqClass
                                                    {
                                                        LvNewReq = LTCWitems.LvNewReq_Id.ToString(),
                                                        EmpLVid = db_data.Id.ToString()

                                                    },
                                                    ReqDate = LTCWitems.DateOfAppl.Value.ToShortDateString(),
                                                    LTCType = LTCWitems.LTC_TYPE.LookupVal.ToString(),
                                                    LTCEncashmentAmount = LTCWitems.EncashmentAmount.ToString() != "0" ? LTCWitems.EncashmentAmount.ToString() : "0",
                                                    LTCSettlementAmount = LTCWitems.LTC_Settle_Amt.ToString() != "0" ? LTCWitems.LTC_Settle_Amt.ToString() : "0",
                                                    AdvanceAmount = LTCWitems.LTCAdvAmt.ToString() != "0" ? LTCWitems.LTCAdvAmt.ToString() : "0",
                                                    Status = StatusNarration
                                                };
                                                returndata.Add(ObjLTCsettleClaimRequest);
                                            }
                                            else if (authority.ToUpper() == "MYSELF")
                                            {
                                                GetLTCsettlementClaimReqClass ObjLTCsettleClaimRequest = new GetLTCsettlementClaimReqClass()
                                                {
                                                    RowData = new ChildGetLTCsettlementClaimReqClass
                                                    {
                                                        LvNewReq = LTCWitems.LvNewReq_Id.ToString(),
                                                        EmpLVid = db_data.Id.ToString()

                                                    },
                                                    ReqDate = LTCWitems.DateOfAppl.Value.ToShortDateString(),
                                                    LTCType = LTCWitems.LTC_TYPE.LookupVal.ToString(),
                                                    LTCEncashmentAmount = LTCWitems.EncashmentAmount.ToString() != "0" ? LTCWitems.EncashmentAmount.ToString() : "0",
                                                    LTCSettlementAmount = LTCWitems.LTC_Settle_Amt.ToString() != "0" ? LTCWitems.LTC_Settle_Amt.ToString() : "0",
                                                    AdvanceAmount = LTCWitems.LTCAdvAmt.ToString() != "0" ? LTCWitems.LTCAdvAmt.ToString() : "0",
                                                    Status = StatusNarration
                                                };
                                                returndata.Add(ObjLTCsettleClaimRequest);
                                            }

                                            //  }
                                        }
                                    }
                                }

                            }
                        }

                        return Json(new { status = true, data = returndata }, JsonRequestBehavior.AllowGet);

                    }
                    else
                    {
                        return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception Ex)
            {
                throw Ex;
            }

        }



        #region Get Applied Data on Sanction Grid
        public class ChildGetLTCSettlementClaimClass
        {
            public string LvNewReq { get; set; }
            public string EmpLVid { get; set; }
            public string IsClose { get; set; }
            public string LvHead_Id { get; set; }
            public string OdReqId { get; set; }

        }


        public class GetLTCSettleClaimClass1
        {
            public string ReqDate { get; set; }
            public string LTCType { get; set; }
            public string AdvanceAmount { get; set; }
            public string LTCEncashmentAmount { get; set; }
            public string LTCSettlementAmount { get; set; }

            public ChildGetLTCSettlementClaimClass RowData { get; set; }
        }



        public ActionResult GetLTCSettlementClaimonSanction(FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                LookupValue FuncModule = new LookupValue();
                string AccessRight = "";
                if (!string.IsNullOrEmpty(Convert.ToString(Session["user-module"])))
                {
                    var id = Convert.ToString(Session["user-module"]);
                    FuncModule = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "601").FirstOrDefault()
                        .LookupValues.Where(e => e.LookupVal.ToUpper() == id.ToUpper()).FirstOrDefault();
                }
                if (FuncModule == null)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                if (!string.IsNullOrEmpty(Convert.ToString(Session["user-module"])))
                {
                    AccessRight = Convert.ToString(Session["auho"]);
                }
                // var EmpIds = UserManager.GeEmployeeList(FuncModule.Id, 0);
                List<EssPortal.Process.PortalRights.AccessRightsData> EmpidsWithfunsub = UserManager.GeEmployeeListWithfunsub(FuncModule.Id, 0, AccessRight);
                if (EmpidsWithfunsub == null && EmpidsWithfunsub.Count == 0)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                // var returnDataClass = new List<returnDataClass>();

                List<GetLTCSettleClaimClass1> returndata = new List<GetLTCSettleClaimClass1>();
                returndata.Add(new GetLTCSettleClaimClass1
                {
                    ReqDate = "Requisition Date",
                    LTCType = "LTC Type",
                    AdvanceAmount = "Advance Amount",
                    LTCEncashmentAmount = "LTCEncashment Amount",
                    LTCSettlementAmount = "LTCSettlement Amount",

                    RowData = new ChildGetLTCSettlementClaimClass
                    {
                        LvNewReq = "0",
                        EmpLVid = "0",
                        IsClose = "0",
                        LvHead_Id = "",
                    }
                });
                foreach (var item1 in EmpidsWithfunsub)
                {


                    var Emps = db.EmployeePayroll
                        .Where(e => (item1.ReportingEmployee.Contains(e.Employee.Id)))
                        .Include(e => e.Employee)
                        .Include(e => e.Employee.ReportingStructRights)
                        .Include(e => e.Employee.ReportingStructRights.Select(b => b.AccessRights.ActionName))
                        .Include(e => e.Employee.ReportingStructRights.Select(b => b.FuncModules))
                        .Include(e => e.Employee.EmpName)
                        .Include(e => e.EmpLTCBlock)
                        .Include(e => e.EmpLTCBlock.Select(b => b.EmpLTCBlockT.Select(a => a.LTCSettlementClaim)))
                        .Include(e => e.EmpLTCBlock.Select(w => w.EmpLTCBlockT.Select(a => a.LTCSettlementClaim.Select(c => c.WFStatus))))
                        .Include(e => e.EmpLTCBlock.Select(w => w.EmpLTCBlockT.Select(a => a.LTCSettlementClaim.Select(c => c.JourneyDetails))))
                        .Include(e => e.EmpLTCBlock.Select(w => w.EmpLTCBlockT.Select(a => a.LTCSettlementClaim.Select(c => c.LTC_TYPE))))
                        .Include(e => e.EmpLTCBlock.Select(w => w.EmpLTCBlockT.Select(a => a.LTCSettlementClaim.Select(c => c.LvNewReq))))
                        .Include(e => e.EmpLTCBlock.Select(w => w.EmpLTCBlockT.Select(a => a.LTCSettlementClaim.Select(c => c.LTCWFDetails))))
                        .ToList();





                    foreach (var item in Emps)
                    {

                        if (item.EmpLTCBlock != null && item.EmpLTCBlock.Count() > 0)
                        {
                            foreach (var empltcbockitem in item.EmpLTCBlock)
                            {
                                foreach (var empLTCblockTitem in empltcbockitem.EmpLTCBlockT)
                                {
                                    if (empLTCblockTitem.LTCSettlementClaim.Count() > 0)
                                    {
                                        var LvIds = UserManager.FilterLTCSettlementClaim(empLTCblockTitem.LTCSettlementClaim.OrderByDescending(e => e.DateOfAppl.Value.ToShortDateString()).ToList(),
                                        Convert.ToString(Session["auho"]), Convert.ToInt32(SessionManager.CompanyId));


                                        if (LvIds.Count() > 0)
                                        {
                                            var LTCSettlementclaimreqdata = empLTCblockTitem.LTCSettlementClaim.Where(e => LvIds.Contains(e.Id)).ToList();
                                            foreach (var singleLTCsettleDetails in LTCSettlementclaimreqdata)
                                            {
                                                if (singleLTCsettleDetails.LTCWFDetails != null)
                                                {

                                                    var session = Session["auho"].ToString().ToUpper();
                                                    var EmpR = item.Employee.ReportingStructRights.Where(e => e.AccessRights != null && e.AccessRights.ActionName.LookupVal.ToUpper() == session && e.FuncModules.LookupVal.ToUpper() == item1.ModuleName.ToUpper())
                                                    .Select(e => e.AccessRights.IsClose).FirstOrDefault();


                                                    returndata.Add(new GetLTCSettleClaimClass1
                                                    {
                                                        RowData = new ChildGetLTCSettlementClaimClass
                                                        {
                                                            LvNewReq = singleLTCsettleDetails.Id.ToString(),
                                                            EmpLVid = item.Id.ToString(),
                                                            IsClose = EmpR.ToString(),
                                                            LvHead_Id = "",
                                                        },

                                                        ReqDate = singleLTCsettleDetails.DateOfAppl.Value.ToShortDateString(),
                                                        LTCType = singleLTCsettleDetails.LTC_TYPE.LookupVal,
                                                        AdvanceAmount = singleLTCsettleDetails.LTCAdvAmt != 0 ? singleLTCsettleDetails.LTCAdvAmt.ToString() : "0",
                                                        LTCEncashmentAmount = singleLTCsettleDetails.EncashmentAmount != 0 ? singleLTCsettleDetails.EncashmentAmount.ToString() : "0",
                                                        LTCSettlementAmount = singleLTCsettleDetails.LTC_Settle_Amt != 0 ? singleLTCsettleDetails.LTC_Settle_Amt.ToString() : "0"



                                                    });


                                                }
                                            }
                                        }
                                    }
                                }


                            }
                        }
                    }
                }

                if (returndata != null && returndata.Count > 0)
                {

                    return Json(new { status = true, data = returndata, responseText = "Okk" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = false, data = returndata, responseText = "Error" }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        #endregion



        #region Get Employee Hotel Book request On Sanction Bind DATA
        public class EmpLTCsettlementClaimdataClass
        {

            public string Status { get; set; }
            public string Emplvhead { get; set; }
            public string Request_Date { get; set; }
            public int? LeaveRequisitionId { get; set; }
            public int LTCTypeId { get; set; }
            public int EmpLTCBlockId { get; set; }

            public int LTCAdvanceClaimId { get; set; }
            public string LTCAdvanceClaimFulldetails { get; set; }
            public int JourneyDetailsID { get; set; }
            public string JourneyDetailsFulldetails { get; set; }
            public int Travel_Hotel_Bookingid { get; set; }
            public string Travel_Hotel_Bookingdetails { get; set; }

            public string NoOfRooms { get; set; }
            public string Narration { get; set; }
            public string SanctionCode { get; set; }
            public string SanctionEmpname { get; set; }
            public string RecomendationCode { get; set; }
            public string RecomendationEmpname { get; set; }
            public double LTCclaimAmount { get; set; }
            public double LTCEligibleAmount { get; set; }
            public double EncashmentAmount { get; set; }
            public double LTCAdvAmt { get; set; }
            public double NoOfDays { get; set; }
            public double LTC_Sanction_Amt { get; set; }
            public double LTC_Settle_Amt { get; set; }
            public int TotalSrCitizen { get; set; }
            public int TotFamilyMembers { get; set; }
            public bool TrClosed { get; set; }
            public bool TrReject { get; set; }
            public string SanctionComment { get; set; }
            public string ApporavalComment { get; set; }
            public string Wf { get; set; }
            public Int32 Id { get; set; }
            public Int32 Lvnewreq { get; set; }
            public string txtLTCBlock { get; set; }
            public string EmployeeName { get; set; }
            public string Remark { get; set; }
            public string Empcode { get; set; }
            public string Isclose { get; set; }
            public int EmployeeId { get; set; }
            public string Incharge { get; set; }
        }


        public ActionResult GetLTCSettlementClaimData(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string authority = Convert.ToString(Session["auho"]);
                var WfStatus = new List<Int32>();
                var SanctionStatus = new List<Int32>();
                var ApporvalStatus = new List<Int32>();
                var RecomendationStatus = new List<Int32>();
                if (authority.ToUpper() == "SANCTION")
                {
                    WfStatus.Add(1);
                    WfStatus.Add(2);
                }
                else if (authority.ToUpper() == "APPROVAL")
                {
                    WfStatus.Add(1);
                    WfStatus.Add(2);
                    RecomendationStatus.Add(7);
                    RecomendationStatus.Add(8);
                }
                else if (authority.ToUpper() == "RECOMMENDATION")
                {
                    WfStatus.Add(1);
                    WfStatus.Add(2);
                }
                else if (authority.ToUpper() == "MYSELF")
                {
                    SanctionStatus.Add(1);
                    SanctionStatus.Add(2);

                    ApporvalStatus.Add(3);
                    ApporvalStatus.Add(4);
                }
                var ids = Utility.StringIdsToListString(data);
                var id = Convert.ToInt32(ids[0]);
                var status = ids.Count > 0 ? ids[2] : null;
                var emplvId = ids.Count > 0 ? ids[1] : null;
                //var LvHeadId = ids.Count > 0 ? ids[3] : null;

                //var lvheadidint = Convert.ToInt32(LvHeadId);
                var EmpLvIdint = Convert.ToInt32(emplvId);

                var W = db.EmployeePayroll
                        .Include(e => e.Employee.EmpName)
                        .Include(e => e.EmpLTCBlock)
                        .Include(e => e.EmpLTCBlock.Select(b => b.EmpLTCBlockT.Select(a => a.LTCSettlementClaim)))
                        .Include(e => e.EmpLTCBlock.Select(w => w.EmpLTCBlockT.Select(a => a.LTCSettlementClaim.Select(c => c.WFStatus))))
                        .Include(e => e.EmpLTCBlock.Select(w => w.EmpLTCBlockT.Select(a => a.LTCSettlementClaim.Select(c => c.JourneyDetails))))
                        .Include(e => e.EmpLTCBlock.Select(w => w.EmpLTCBlockT.Select(a => a.LTCSettlementClaim.Select(c => c.LTC_TYPE))))
                        .Include(e => e.EmpLTCBlock.Select(w => w.EmpLTCBlockT.Select(a => a.LTCSettlementClaim.Select(c => c.LvNewReq))))
                        .Include(e => e.EmpLTCBlock.Select(w => w.EmpLTCBlockT.Select(a => a.LTCSettlementClaim.Select(c => c.LTCAdvanceClaim))))
                        .Include(e => e.EmpLTCBlock.Select(w => w.EmpLTCBlockT.Select(a => a.LTCSettlementClaim.Select(c => c.Travel_Hotel_Booking))))
                        .Include(e => e.EmpLTCBlock.Select(w => w.EmpLTCBlockT.Select(a => a.LTCSettlementClaim.Select(c => c.LTCWFDetails))))

                        .Where(e => e.Employee.Id == EmpLvIdint).ToList();

                foreach (var empPayrollitem in W)
                {
                    foreach (var empLtcblockitem in empPayrollitem.EmpLTCBlock)
                    {
                        foreach (var LtcblockTitem in empLtcblockitem.EmpLTCBlockT)
                        {
                            if (LtcblockTitem != null && LtcblockTitem.LTCSettlementClaim.Count() > 0)
                            {

                                var v = LtcblockTitem.LTCSettlementClaim.Where(e => e.Id == id).Select(s => new EmpLTCsettlementClaimdataClass
                                {
                                    EmployeeId = empPayrollitem.Employee.Id,
                                    EmployeeName = empPayrollitem.Employee.EmpCode + " " + empPayrollitem.Employee.EmpName.FullNameFML,
                                    Lvnewreq = s.Id,
                                    Empcode = empPayrollitem.Employee.EmpCode,
                                    EmpLTCBlockId = LtcblockTitem.Id,


                                    Request_Date = s.DateOfAppl != null ? s.DateOfAppl.Value.ToShortDateString() : null,
                                    LeaveRequisitionId = s.LvNewReq_Id != 0 ? s.LvNewReq_Id : 0,
                                    LTCTypeId = s.LTC_TYPE != null ? s.LTC_TYPE.Id : 0,
                                    //City = s.City.Id,
                                    //State = s.State.Id,
                                    //IsFamilyIncl = s.IsFamilyIncl,
                                    LTCAdvanceClaimId = s.LTCAdvanceClaim != null ? s.LTCAdvanceClaim.Id : 0,
                                    LTCAdvanceClaimFulldetails = s.LTCAdvanceClaim != null ? s.LTCAdvanceClaim.FullDetails.ToString() : "",
                                    JourneyDetailsID = s.JourneyDetails != null ? s.JourneyDetails.Id : 0,
                                    JourneyDetailsFulldetails = s.JourneyDetails != null ? s.JourneyDetails.FullDetails.ToString() : "",
                                    Travel_Hotel_Bookingid = s.Travel_Hotel_Booking != null && s.Travel_Hotel_Booking.Count() > 0 ? s.Travel_Hotel_Booking.Select(i => i.Id).FirstOrDefault() : 0,
                                    Travel_Hotel_Bookingdetails = s.Travel_Hotel_Booking != null && s.Travel_Hotel_Booking.Count() > 0 ? s.Travel_Hotel_Booking.Select(h => h.HotelName + ", StartDate :" + h.StartDate.Value.ToShortDateString()).FirstOrDefault() : null,


                                    LTCclaimAmount = s.LTC_Claim_Amt,
                                    EncashmentAmount = s.EncashmentAmount,
                                    LTCEligibleAmount = s.LTC_Eligible_Amt,
                                    LTC_Sanction_Amt = s.LTC_Sanction_Amt,
                                    LTCAdvAmt = s.LTCAdvAmt,
                                    LTC_Settle_Amt = s.LTC_Settle_Amt,

                                    NoOfDays = s.NoOfDays,
                                    Remark = s.Remark,

                                    Isclose = status.ToString(),
                                    //Id = s.Id,
                                    TrClosed = s.TrClosed,
                                    SanctionCode = s.LTCWFDetails != null && s.LTCWFDetails.Count > 0 ? s.LTCWFDetails.Where(z => WfStatus.Contains(z.WFStatus)).OrderByDescending(e => e.Id).Select(e => e.DBTrack.CreatedBy).FirstOrDefault() : null,
                                    SanctionComment = s.LTCWFDetails != null && s.LTCWFDetails.Count > 0 ? s.LTCWFDetails.Where(z => SanctionStatus.Contains(z.WFStatus)).OrderByDescending(e => e.Id).Select(e => e.Comments).FirstOrDefault() : null,
                                    ApporavalComment = s.LTCWFDetails != null && s.LTCWFDetails.Count > 0 ? s.LTCWFDetails.Where(z => ApporvalStatus.Contains(z.WFStatus)).OrderByDescending(e => e.Id).Select(e => e.Comments).FirstOrDefault() : null,
                                    Wf = s.LTCWFDetails != null && s.LTCWFDetails.Count > 0 ? s.LTCWFDetails.Where(z => WfStatus.Contains(z.WFStatus)).OrderByDescending(e => e.Id).Select(e => e.Comments).LastOrDefault() : null,
                                    RecomendationCode = s.LTCWFDetails != null && s.LTCWFDetails.Count > 0 ? s.LTCWFDetails.Where(z => RecomendationStatus.Contains(z.WFStatus)).OrderByDescending(e => e.Id).Select(e => e.DBTrack.CreatedBy).FirstOrDefault() : null,
                                    RecomendationEmpname = s.LTCWFDetails != null && s.LTCWFDetails.Count > 0 ? s.LTCWFDetails.Where(z => RecomendationStatus.Contains(z.WFStatus)).OrderByDescending(e => e.Id).Select(e => e.Comments).FirstOrDefault() : null,
                                    // Incharge = s.Incharge != null ? s.Incharge.EmpCode + ' ' + s.Incharge.EmpName.FullDetails.ToString() : null
                                }).ToList();

                                TempData["LTCsettleClaim"] = v;

                            }

                        }
                    }
                }
                var LTCsettleClaimreturn = TempData["LTCsettleClaim"];
                return Json(LTCsettleClaimreturn, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion


        public ActionResult GetLookupIncharge(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var empinchargeloc = db.Employee
                    .Include(e => e.GeoStruct)
                    .Include(e => e.GeoStruct.Location)
                    .Include(e => e.GeoStruct.Location.Incharge)
                    .Where(e => e.EmpCode == data).FirstOrDefault();
                int inchid = 0;
                if (empinchargeloc != null)
                {
                    if (empinchargeloc.GeoStruct.Location.Incharge != null)
                    {
                        inchid = empinchargeloc.GeoStruct.Location.Incharge.Id;// if dep inchagre on leave then that location incharge should be incharge of that dep.
                        //other loc,dep,division incharge not come in list. as suggest sir
                    }

                }
                var exceploc = db.Location.Where(e => e.Incharge.Id != null && e.Incharge.Id != inchid).Select(e => e.Incharge.Id).ToList();
                var excepDep = db.Department.Where(e => e.Incharge.Id != null && e.Incharge.Id != inchid).Select(e => e.Incharge.Id).ToList();
                var excepDivision = db.Division.Where(e => e.Incharge.Id != null && e.Incharge.Id != inchid).Select(e => e.Incharge.Id).ToList();
                var exceptot = exceploc.Union(excepDep).Union(excepDivision).ToList();

                var fall = db.Employee
                    .Include(e => e.EmpName)
                    .Include(e => e.ServiceBookDates)
                    .Where(e => e.ServiceBookDates.ServiceLastDate == null && !exceptot.Contains(e.Id)).ToList();
                IEnumerable<Employee> all;
                if (!string.IsNullOrEmpty(data))
                {
                    // all = db.Employee.ToList().Where(d => d.EmpCode.Contains(data));
                    all = fall;

                }
                else
                {
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { srno = c.Id, lookupvalue = c.FullDetails }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }





        public ActionResult Update_LTCsettlementCLAIM(LTCSettlementClaim HbReq, FormCollection form, String data)
        {

            string authority = form["authority"] == null ? null : form["authority"];
            var isClose = form["isClose"] == null ? null : form["isClose"];
            if (authority == null && isClose == null)
            {
                return Json(new { status = false }, JsonRequestBehavior.AllowGet);
            }
            var ids = Utility.StringIdsToListString(data);
            var LTCsettlenewreqid = Convert.ToInt32(ids[0]);
            var EmpPayrollId = Convert.ToInt32(ids[1]);
            string Sanction = form["Sanction"];
            string ReasonSanction = form["ReasonSanction"];
            string HR = form["HR"] == null ? null : form["HR"];
            string MySelf = form["MySelf"] == null ? "false" : form["MySelf"];
            string ReasonMySelf = form["ReasonMySelf"] == null ? null : form["ReasonMySelf"];
            string Approval = form["Approval"];
            string ReasonApproval = form["ReasonApproval"];
            string ReasonHr = form["ReasonHr"] == null ? null : form["ReasonHr"];
            string Recomendation = form["Recomendation"];
            string ReasonRecomendation = form["ReasonRecomendation"];
            bool SanctionRejected = false;
            bool HrRejected = false;
            string SanInchargeid = form["SanIncharge_id"];
            string RecInchargeid = form["RecIncharge_id"];
            string AppInchargeid = form["AppIncharge_id"];



            //bool self = false;
            using (DataBaseContext db = new DataBaseContext())
            {
                //  access right no of levaefrom days and to days check start
                LookupValue FuncModule = new LookupValue();
                string AccessRight = "";

                if (!string.IsNullOrEmpty(Convert.ToString(Session["user-module"])))
                {
                    var id = Convert.ToString(Session["user-module"]);
                    var lookupdata = db.Lookup
                               .Include(e => e.LookupValues)
                               .Where(e => e.Code == "601").AsNoTracking().FirstOrDefault();
                    FuncModule = lookupdata.LookupValues.Where(e => e.LookupVal.ToUpper() == id.ToUpper()).FirstOrDefault();


                }

                var query = db.EmployeePayroll.Where(e => e.Id == EmpPayrollId)
                        .Include(e => e.EmpLTCBlock)
                        .Include(e => e.Employee.EmpName)
                        .Include(e => e.EmpLTCBlock.Select(b => b.EmpLTCBlockT.Select(a => a.LTCSettlementClaim)))
                        .Include(e => e.EmpLTCBlock.Select(w => w.EmpLTCBlockT.Select(a => a.LTCSettlementClaim.Select(c => c.WFStatus))))
                        .Include(e => e.EmpLTCBlock.Select(w => w.EmpLTCBlockT.Select(a => a.LTCSettlementClaim.Select(c => c.JourneyDetails))))
                        .Include(e => e.EmpLTCBlock.Select(w => w.EmpLTCBlockT.Select(a => a.LTCSettlementClaim.Select(c => c.LTC_TYPE))))
                        .Include(e => e.EmpLTCBlock.Select(w => w.EmpLTCBlockT.Select(a => a.LTCSettlementClaim.Select(c => c.LvNewReq))))
                        .Include(e => e.EmpLTCBlock.Select(w => w.EmpLTCBlockT.Select(a => a.LTCSettlementClaim.Select(c => c.LTCAdvanceClaim))))
                        .Include(e => e.EmpLTCBlock.Select(w => w.EmpLTCBlockT.Select(a => a.LTCSettlementClaim.Select(c => c.Travel_Hotel_Booking))))
                        .Include(e => e.EmpLTCBlock.Select(w => w.EmpLTCBlockT.Select(a => a.LTCSettlementClaim.Select(c => c.LTCWFDetails))))
                    .ToList();



                if (!string.IsNullOrEmpty(Convert.ToString(Session["user-module"])))
                {
                    AccessRight = Convert.ToString(Session["auho"]);
                }

                if (FuncModule == null)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }

                List<EssPortal.Process.PortalRights.AccessRightsData> EmpidsWithfunsub = UserManager.GeEmployeeListWithfunsub(FuncModule.Id, 0, AccessRight);


                bool TrClosed = false;

                try
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        foreach (var empPayrollitem in query)
                        {
                            foreach (var empLtcblockitem in empPayrollitem.EmpLTCBlock)
                            {
                                foreach (var LtcblockTitem in empLtcblockitem.EmpLTCBlockT)
                                {
                                    if (LtcblockTitem != null && LtcblockTitem.LTCSettlementClaim.Count() > 0)
                                    {
                                        var LTCSettlementList = LtcblockTitem.LTCSettlementClaim.Where(e => e.Id == LTCsettlenewreqid).ToList();
                                        //if someone reject lv
                                        foreach (var Ltcsettleitems in LTCSettlementList)
                                        {
                                            List<FunctAllWFDetails> oFunctWFDetails_List = new List<FunctAllWFDetails>();
                                            FunctAllWFDetails objLTCWFDetails = new FunctAllWFDetails();

                                            int salhdid = 0;
                                            var fyyr = db.Calendar.Include(x => x.Name).Where(x => x.Name.LookupVal.ToUpper() == "FINANCIALYEAR" && x.Default == true).SingleOrDefault();

                                            var QEmpSalStruct1 = db.EmpSalStruct//.Where(e => e.EmployeePayroll_Id == OEmployeePayrollId && e.EffectiveDate >= comparedate)
                                                              .Include(e => e.EmpSalStructDetails)
                                                              .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead))
                                                              .Include(e => e.EmpSalStructDetails.Select(q => q.SalHeadFormula))
                                                              .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.Frequency))
                                                              .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.Type))
                                                              .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.RoundingMethod))
                                                              .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.SalHeadOperationType))
                                                              .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.LvHead));//.ToList();
                                            var EmpSalStructTotal = QEmpSalStruct1.Where(e => e.EmployeePayroll.Id == empPayrollitem.Id && e.EndDate == null)
                                                                          .ToList();
                                            var OEmpSalStruct = EmpSalStructTotal.Where(e => e.EndDate == null).SingleOrDefault();
                                            if (OEmpSalStruct != null)
                                            {

                                                salhdid = OEmpSalStruct.EmpSalStructDetails
                                                   .Where(s => s.SalaryHead.Frequency.LookupVal.ToUpper() == "YEARLY"
                                                && s.SalaryHead.Type.LookupVal.ToUpper() == "EARNING"
                                                && (s.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "LTC"
                                                )).FirstOrDefault() != null ? OEmpSalStruct.EmpSalStructDetails
                                                   .Where(s => s.SalaryHead.Frequency.LookupVal.ToUpper() == "YEARLY"
                                                && s.SalaryHead.Type.LookupVal.ToUpper() == "EARNING"
                                                && (s.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "LTC"
                                                )).FirstOrDefault().SalaryHead.Id : 0;
                                            }


                                            if (authority.ToUpper() == "MYSELF")
                                            {
                                                TrClosed = false;
                                            }
                                            if (authority.ToUpper() == "SANCTION")
                                            {

                                                if (Sanction == null)
                                                {
                                                    return Json(new Utility.JsonClass { status = false, responseText = "Please Select the Sanction Status" }, JsonRequestBehavior.AllowGet);
                                                }
                                                if (ReasonSanction == "")
                                                {
                                                    return Json(new Utility.JsonClass { status = false, responseText = "Please Enter the Reason" }, JsonRequestBehavior.AllowGet);
                                                }
                                                if (Convert.ToBoolean(Sanction) == true)
                                                {
                                                    //sanction yes -1

                                                    objLTCWFDetails = new FunctAllWFDetails
                                                    {
                                                        WFStatus = 1,
                                                        Comments = ReasonSanction,
                                                        DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName }
                                                    };

                                                    oFunctWFDetails_List.Add(objLTCWFDetails);

                                                    Ltcsettleitems.TrClosed = Convert.ToBoolean(isClose) == true ? true : false;
                                                    Ltcsettleitems.WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "1").FirstOrDefault();


                                                    if (Convert.ToBoolean(isClose) == true)
                                                    {
                                                        if (salhdid != null)
                                                        {
                                                            // yearly payment Start
                                                            YearlyPaymentT ObjYPT = new YearlyPaymentT();
                                                            {
                                                                ObjYPT.SalaryHead = db.SalaryHead.Find(salhdid);
                                                                ObjYPT.AmountPaid = Ltcsettleitems.LTC_Settle_Amt;
                                                                ObjYPT.FromPeriod = empLtcblockitem.BlockStart;
                                                                ObjYPT.ToPeriod = empLtcblockitem.BlockEnd;
                                                                ObjYPT.ProcessMonth = Convert.ToDateTime(Ltcsettleitems.DateOfAppl).ToString("MM/yyyy");
                                                                ObjYPT.PayMonth = Convert.ToDateTime(Ltcsettleitems.DateOfAppl).ToString("MM/yyyy");
                                                                ObjYPT.ReleaseDate = Ltcsettleitems.DateOfAppl;
                                                                ObjYPT.ReleaseFlag = true;
                                                                ObjYPT.TDSAmount = 0;
                                                                ObjYPT.OtherDeduction = 0;
                                                                ObjYPT.FinancialYear = fyyr;
                                                                ObjYPT.DBTrack = Ltcsettleitems.DBTrack;
                                                                ObjYPT.FuncStruct = Ltcsettleitems.FuncStruct;
                                                                ObjYPT.GeoStruct = Ltcsettleitems.GeoStruct;
                                                                ObjYPT.PayStruct = Ltcsettleitems.PayStruct;

                                                            }
                                                            List<YearlyPaymentT> OYrlyPaylist = new List<YearlyPaymentT>();
                                                            db.YearlyPaymentT.Add(ObjYPT);
                                                            db.SaveChanges();
                                                            OYrlyPaylist.Add(ObjYPT);

                                                            //yearly payment end
                                                        }
                                                    }

                                                }
                                                else if (Convert.ToBoolean(Sanction) == false)
                                                {

                                                    objLTCWFDetails = new FunctAllWFDetails
                                                    {
                                                        WFStatus = 2,
                                                        Comments = ReasonSanction,
                                                        DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                                                    };
                                                    oFunctWFDetails_List.Add(objLTCWFDetails);


                                                    SanctionRejected = true;
                                                }
                                            }
                                            else if (authority.ToUpper() == "APPROVAL")//Hr
                                            {
                                                if (Approval == null)
                                                {
                                                    return Json(new Utility.JsonClass { status = false, responseText = "Please Select the Approval Status" }, JsonRequestBehavior.AllowGet);
                                                }
                                                if (ReasonApproval == "")
                                                {
                                                    return Json(new Utility.JsonClass { status = false, responseText = "Please Enter the Reason" }, JsonRequestBehavior.AllowGet);
                                                }
                                                if (Convert.ToBoolean(Approval) == true)
                                                {
                                                    //approval yes-3
                                                    //var CheckAllreadySanction = BAAppTarget.Where(e => e.BA_WorkFlow.Any(r => r.WFStatus == 3)).ToList();
                                                    //if (CheckAllreadySanction.Count() > 0)
                                                    //{
                                                    //    return Json(new Utility.JsonClass { status = false, responseText = "This Leave Allready Approved....Refresh The Page!" }, JsonRequestBehavior.AllowGet);
                                                    //}
                                                    objLTCWFDetails = new FunctAllWFDetails
                                                    {
                                                        WFStatus = 3,
                                                        Comments = ReasonApproval,
                                                        DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName }
                                                    };
                                                    oFunctWFDetails_List.Add(objLTCWFDetails);

                                                    Ltcsettleitems.TrClosed = Convert.ToBoolean(isClose) == true ? true : false;
                                                    Ltcsettleitems.WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "3").FirstOrDefault();


                                                    if (Convert.ToBoolean(isClose) == true)
                                                    {
                                                        if (salhdid != null)
                                                        {
                                                            // yearly payment Start
                                                            YearlyPaymentT ObjYPT = new YearlyPaymentT();
                                                            {
                                                                ObjYPT.SalaryHead = db.SalaryHead.Find(salhdid);
                                                                ObjYPT.AmountPaid = Ltcsettleitems.LTC_Settle_Amt;
                                                                ObjYPT.FromPeriod = empLtcblockitem.BlockStart;
                                                                ObjYPT.ToPeriod = empLtcblockitem.BlockEnd;
                                                                ObjYPT.ProcessMonth = Convert.ToDateTime(Ltcsettleitems.DateOfAppl).ToString("MM/yyyy");
                                                                ObjYPT.PayMonth = Convert.ToDateTime(Ltcsettleitems.DateOfAppl).ToString("MM/yyyy");
                                                                ObjYPT.ReleaseDate = Ltcsettleitems.DateOfAppl;
                                                                ObjYPT.ReleaseFlag = true;
                                                                ObjYPT.TDSAmount = 0;
                                                                ObjYPT.OtherDeduction = 0;
                                                                ObjYPT.FinancialYear = fyyr;
                                                                ObjYPT.DBTrack = Ltcsettleitems.DBTrack;
                                                                ObjYPT.FuncStruct = Ltcsettleitems.FuncStruct;
                                                                ObjYPT.GeoStruct = Ltcsettleitems.GeoStruct;
                                                                ObjYPT.PayStruct = Ltcsettleitems.PayStruct;

                                                            }
                                                            List<YearlyPaymentT> OYrlyPaylist = new List<YearlyPaymentT>();
                                                            db.YearlyPaymentT.Add(ObjYPT);
                                                            db.SaveChanges();
                                                            OYrlyPaylist.Add(ObjYPT);

                                                            //yearly payment end
                                                        }
                                                    }

                                                }
                                                else if (Convert.ToBoolean(Approval) == false)
                                                {
                                                    //approval no-4
                                                    //var LvWFDetails = new LvWFDetails
                                                    //{
                                                    //    WFStatus = 4,
                                                    //    Comments = ReasonApproval,
                                                    //    DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName }
                                                    //};
                                                    //qurey.LvWFDetails.Add(LvWFDetails);

                                                    //qurey.LvWFDetails.Add(LvWFDetails);
                                                    //var CheckAllreadySanction = BAAppTarget.Where(e => e.BA_WorkFlow.Any(r => r.WFStatus == 4)).ToList();
                                                    //if (CheckAllreadySanction.Count() > 0)
                                                    //{
                                                    //    return Json(new Utility.JsonClass { status = false, responseText = "This Leave Allready Rejected....Refresh The Page!" }, JsonRequestBehavior.AllowGet);
                                                    //}
                                                    objLTCWFDetails = new FunctAllWFDetails
                                                    {
                                                        WFStatus = 4,
                                                        Comments = ReasonApproval,
                                                        DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                                                    };
                                                    oFunctWFDetails_List.Add(objLTCWFDetails);
                                                    //  qurey.WFStatus = db.LookupValue.Where(e => e.LookupVal == "2").SingleOrDefault();
                                                    TrClosed = true;
                                                    HrRejected = true;
                                                }
                                            }
                                            else if (authority.ToUpper() == "RECOMMENDATION")
                                            {

                                                if (Recomendation == null)
                                                {
                                                    return Json(new Utility.JsonClass { status = false, responseText = "Please Select The Recomendation Status" }, JsonRequestBehavior.AllowGet);
                                                }
                                                if (ReasonRecomendation == "")
                                                {
                                                    return Json(new Utility.JsonClass { status = false, responseText = "Please Enter The Reason" }, JsonRequestBehavior.AllowGet);
                                                }
                                                if (Convert.ToBoolean(Recomendation) == true)
                                                {
                                                    //Recomendation yes -7
                                                    var CheckAllreadyRecomendation = LTCSettlementList.Where(e => e.LTCWFDetails.Any(r => r.WFStatus == 7)).ToList();
                                                    if (CheckAllreadyRecomendation.Count() > 0)
                                                    {
                                                        return Json(new Utility.JsonClass { status = false, responseText = "This Leave Allready Recomendation....Refresh The Page!" }, JsonRequestBehavior.AllowGet);
                                                    }
                                                    objLTCWFDetails = new FunctAllWFDetails
                                                    {
                                                        WFStatus = 7,
                                                        Comments = ReasonRecomendation,
                                                        DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName }
                                                    };
                                                    //qurey.BA_WorkFlow.Add(AppWFDetails);

                                                }
                                                else if (Convert.ToBoolean(Recomendation) == false)
                                                {
                                                    //Recommendation no -8

                                                    var CheckAllreadyRecomendation = LTCSettlementList.Where(e => e.LTCWFDetails.Any(r => r.WFStatus == 8)).ToList();
                                                    if (CheckAllreadyRecomendation.Count() > 0)
                                                    {
                                                        return Json(new Utility.JsonClass { status = false, responseText = "This Leave Allready Rejected....Refresh The Page!" }, JsonRequestBehavior.AllowGet);
                                                    }
                                                    objLTCWFDetails = new FunctAllWFDetails
                                                    {
                                                        WFStatus = 8,
                                                        Comments = ReasonRecomendation,
                                                        DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                                                    };
                                                    oFunctWFDetails_List.Add(objLTCWFDetails);

                                                    //   qurey.WFStatus = db.LookupValue.Where(e => e.LookupVal == "2").SingleOrDefault();
                                                    TrClosed = true;
                                                    SanctionRejected = true;
                                                }
                                            }
                                            if (Ltcsettleitems.LTCWFDetails != null)
                                            {
                                                oFunctWFDetails_List.AddRange(Ltcsettleitems.LTCWFDetails);
                                            }

                                            Ltcsettleitems.LTCWFDetails = oFunctWFDetails_List;
                                            db.LTCSettlementClaim.Attach(Ltcsettleitems);
                                            db.Entry(Ltcsettleitems).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();
                                            //db.Entry(x).State = System.Data.Entity.EntityState.Detached;   
                                        }
                                    }
                                }
                            }
                        }

                        ts.Complete();
                        return Json(new Utility.JsonClass { status = true, responseText = "Record Updated..!" }, JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception e)
                {
                    return Json(new Utility.JsonClass { status = false, responseText = e.InnerException.Message.ToString() }, JsonRequestBehavior.AllowGet);
                }
            }
        }




    }
}