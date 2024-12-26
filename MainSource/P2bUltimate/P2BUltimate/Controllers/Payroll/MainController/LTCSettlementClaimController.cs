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
using Leave;
using P2BUltimate.Process;
using System.Diagnostics;
using P2BUltimate.Security;

namespace P2BUltimate.Controllers.Payroll.MainController
{
    public class LTCSettlementClaimController : Controller
    {
        List<String> Msg = new List<string>();
        //
        // GET: /LTCSettlementClaim/
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/LTCSettlementClaim/Index.cshtml");
        }

        public ActionResult GridPartial()
        {
            return View("~/Views/Shared/Payroll/_LTCSettlClaimGridPartial.cshtml");
        }

        public ActionResult partial_JourneyDetails()
        {
            return View("~/Views/Shared/Payroll/_JourneyDetails.cshtml");
        }

        public ActionResult partial_TravelHotelBooking()
        {
            return View("~/Views/Shared/Payroll/_TravelHotelBooking.cshtml");
        }

        public ActionResult partial_HotelEligibilityPolicy()
        {
            return View("~/Views/Shared/Payroll/_HotelEligibilityPolicy.cshtml");
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

        public class LvNewReqData
        {
            public int Id { get; set; }
            public string FullDetails { get; set; }

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
                   // ElligibleBillAmount = (hotelelgpolicy.Lodging_Eligible_Amt_PerDay + hotelelgpolicy.Food_Eligible_Amt_PerDay) * bdays;
                    ElligibleBillAmount = (hotelelgpolicy.Lodging_Eligible_Amt_PerDay) * bdays;
                   
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
        public ActionResult TravelModeEligibilityPolicylist(string data, string data2)
        {
            //using (DataBaseContext db = new DataBaseContext())
            //{
            //    int TID = Convert.ToInt32(data2);
            //    var qurey = db.TravelModeEligibilityPolicy.Where(e => e.Id == TID).ToList();
            //    var selected = (Object)null;
            //    if (data2 != "" && data != "0" && data2 != "0")
            //    {
            //        selected = Convert.ToInt32(data2);
            //    }

            //    SelectList s = new SelectList(qurey, "Id", "TA_TM_Elligibilty_Code", selected);
            //    return Json(s, JsonRequestBehavior.AllowGet);
            //}

            using (DataBaseContext db = new DataBaseContext())
            {
                //SelectList s = (SelectList)null;
                var selected = (Object)null;
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
        //public ActionResult TravelModeEligibilityPolicylist(string data, string data2)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        //SelectList s = (SelectList)null;
        //        var selected = (Object)null;
        //       // List<TravelModeEligibilityPolicy> TransMode = new List<TravelModeEligibilityPolicy>();
        //        if (data != "" && data != null)
        //        {
        //            //int empid = Convert.ToInt32(data);


        //            int TID = Convert.ToInt32(data);
        //           // var tramode = db.TravelModeEligibilityPolicy.Where(e => e.TA_TM_Elligibilty_Code == data).ToList();
        //            var qurey = db.TravelModeEligibilityPolicy.Where(e => e.Id == TID).ToList();

                   
        //            if (data2 != "" && data != "0" && data2 != "0")
        //            {
        //                selected = Convert.ToInt32(data2);
        //            }

        //            SelectList s = new SelectList(qurey, "Id", "TA_TM_Elligibilty_Code", selected);
        //            return Json(s, JsonRequestBehavior.AllowGet);
        //        //    if (tramode.Count() > 0)
        //        //    {
        //        //        foreach (var item in tramode)
        //        //        {
        //        //            if (item.TA_TM_Elligibilty_Code != null)
        //        //            {

        //        //                TransMode.Add(item);
                              
        //        //            }

        //        //        }
        //        //    }

        //        }
               
               

        //        //SelectList s = new SelectList(TransMode, "Id", "TA_TM_Elligibilty_Code", selected);
        //        //return Json(s, JsonRequestBehavior.AllowGet);
        //    }
        //}

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
                        .Include(e => e.EmployeeLTCStructDetails.Select(r=>r.LTCFormula))
                        .Include(e => e.EmployeeLTCStructDetails.Select(r => r.LTCFormula.TravelModeEligibilityPolicy))
                        .Where(e => e.EmployeePayroll.Employee_Id == empid && e.EndDate==null).FirstOrDefault();

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
                var list2 = db.TADASettlementClaim.Include(e=>e.JourneyDetails)
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

        public class HotelElData //childgrid
        {
            public int Id { get; set; }
            public string FullDetails { get; set; }

        }

        public ActionResult GetHotelEligibilityPolicyDetails(List<int> SkipIds,string Empid)
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
                if (LTCStruct!=null)
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
                    int Emp = form["Employee-Table"] == "0" ? 0 : Convert.ToInt32(form["Employee-Table"]);
                    string ltcadvanceclaimlist = form["LTCAdvanceClaimList"] == "0" ? "" : form["LTCAdvanceClaimList"];
                    string ltctypelist = form["LTCTypelist"] == "0" ? "" : form["LTCTypelist"];
                    string lvreqlist = form["LvReqList"] == "0" ? "" : form["LvReqList"];
                    string journewdetailslist = form["JourneyDetailsList"] == "0" ? "" : form["JourneyDetailsList"];
                    string hotelbookinglist = form["HotelBookingDetailsList"] == "0" ? "" : form["HotelBookingDetailsList"];
                    string LtcBlock = form["BlockId"] == "0" ? "" : form["BlockId"];


                    //string Incharge_DDL = form["Incharge_DDL"] == "" ? null : form["Incharge_DDL"];
                    string Incharge_DDL = form["Incharge_Id"] == "" ? null : form["Incharge_Id"];

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
                            JourneyObjectFullDetails = e.JourneyObject.Select(a => "JourneyDistance:" + a.JourneyDist + ", " + "PlaceFrom:" + a.PlaceFrom + ", " + "PlaceTo:" + a.PlaceTo + ", " + "FromDate:" + a.FromDate.Value.ToString("MM/dd/yyyy hh:mm") + ", " + "ToDate:" + a.ToDate.Value.ToString("MM/dd/yyyy hh:mm")).ToArray() 
                        });
                    }

                    var y = db.JourneyDetails.Include(e => e.FamilyDetails)
                          .Where(e => e.Id == data && e.FamilyDetails.Count > 0).ToList();
                    foreach (var e in k)
                    {
                        if (e.FamilyDetails!=null)
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
        public ActionResult GridEditData(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    //var empltcblockdata = db.EmpLTCBlockT.Select(e => e.LTCSettlementClaim.Where(t => t.Id == data)).ToList();


                    var Q = db.LTCSettlementClaim
                        .Include(e => e.JourneyDetails)
                        .Include(e => e.Travel_Hotel_Booking)
                        .Include(e => e.LTCAdvanceClaim)
                        //.Include(e=>e.FamilyDetails)
                        //.Include(e=>e.EmployeeDocuments)
                     .Where(e => e.Id == data).Select
                     (c => new
                     {
                         Id = c.Id,
                         // BlockPeriod = c.FullDetails,
                         ReqDate = c.DateOfAppl,
                         Noofdays = c.NoOfDays,
                         LTCType = c.LTC_TYPE != null ? c.LTC_TYPE.Id : 0,
                         lvreqid = c.LvNewReq == null ? 0 : c.LvNewReq.Id,
                         EligibleAmt = c.LTC_Eligible_Amt,
                         AdvanceAmt = c.LTCAdvAmt,
                         Remark = c.Remark,
                         ltcclaimamt = c.LTC_Claim_Amt,
                         LTCEncashAmount = c.EncashmentAmount == null ? 0 : c.EncashmentAmount,
                         LTC_Sanction_Amt = c.LTC_Sanction_Amt == null ? 0 : c.LTC_Sanction_Amt,
                         LTC_Settle_Amt = c.LTC_Settle_Amt == null ? 0 : c.LTC_Settle_Amt,
                         LTCAdvAmt = c.LTCAdvAmt == null ? 0 : c.LTCAdvAmt,
                         JourneydetailsId = c.JourneyDetails == null ? 0 : c.JourneyDetails.Id,
                         JourneydetailsFulldetails = c.JourneyDetails == null ? "" : c.JourneyDetails.FullDetails,
                         LTCAdvanceClaimid = c.LTCAdvanceClaim == null ? 0 : c.LTCAdvanceClaim.Id,
                         LTCAdvanceClaimfulldetails = c.LTCAdvanceClaim == null ? "" : c.LTCAdvanceClaim.FullDetails,
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
        public ActionResult GridEditSave(LTCSettlementClaim c, FormCollection form, string data)
        {
            List<string> Msgs = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                int Id = int.Parse(data);
                string ltcadvanceclaimlist = form["LTCAdvanceClaimListPartial"] == "0" ? "" : form["LTCAdvanceClaimListPartial"];
                string ltctypelist = form["LTCTypelistPartial"] == "0" ? "" : form["LTCTypelistPartial"];
                string lvreqlist = form["LvReqListPartial"] == "0" ? "" : form["LvReqListPartial"];
                string journewdetailslist = form["JourneyDetailsListPartial"] == "0" ? "" : form["JourneyDetailsListPartial"];
                string hotelbookinglist = form["HotelBookingDetailsListPartial"] == "0" ? "" : form["HotelBookingDetailsListPartial"];
                string LtcBlock = form["BlockIdPartial"] == "0" ? "" : form["BlockIdPartial"];

                var LTCSettleClaimData = db.LTCSettlementClaim
                     .Include(e => e.JourneyDetails)
                     .Include(e => e.Travel_Hotel_Booking)
                     .Include(e => e.LTCAdvanceClaim)
                    .Where(e => e.Id == Id).SingleOrDefault();

                if (ltcadvanceclaimlist != null && ltcadvanceclaimlist != "")
                {
                    int ltcadvanceid = Convert.ToInt32(ltcadvanceclaimlist);
                    var value = db.LTCAdvanceClaim.Where(e => e.Id == ltcadvanceid).SingleOrDefault();
                    LTCSettleClaimData.LTCAdvanceClaim = value;

                }
                else
                {
                    LTCSettleClaimData.LTCAdvanceClaim = null;
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
                    LTCSettleClaimData.Travel_Hotel_Booking = travelhotelbookinglist;
                }
                else
                {
                    LTCSettleClaimData.Travel_Hotel_Booking = null;
                }


                if (journewdetailslist != null)
                {
                    if (journewdetailslist != "")
                    {
                        int ids = Convert.ToInt32(journewdetailslist);
                        var val = db.JourneyDetails.Where(e => e.Id == ids).SingleOrDefault();
                        LTCSettleClaimData.JourneyDetails = val;
                    }
                }
                else
                {
                    LTCSettleClaimData.JourneyDetails = null;
                }
                if (lvreqlist != null)
                {
                    if (lvreqlist != "")
                    {
                        int ids = Convert.ToInt32(lvreqlist);
                        var val = db.LvNewReq.Where(e => e.Id == ids).SingleOrDefault();
                        LTCSettleClaimData.LvNewReq = val;
                    }
                }
                else
                {
                    LTCSettleClaimData.Travel_Hotel_Booking = null;
                }
                if (ltctypelist != null && ltctypelist != "")
                {
                    var val = db.LookupValue.Find(int.Parse(ltctypelist));
                    LTCSettleClaimData.LTC_TYPE = val;
                }
                else
                {
                    LTCSettleClaimData.LTC_TYPE = null;
                }

                LTCSettleClaimData.JourneyDetails = LTCSettleClaimData.JourneyDetails;
                LTCSettleClaimData.LTCAdvanceClaim = LTCSettleClaimData.LTCAdvanceClaim;
                LTCSettleClaimData.Travel_Hotel_Booking = LTCSettleClaimData.Travel_Hotel_Booking;
                LTCSettleClaimData.Remark = c.Remark;
                LTCSettleClaimData.LvNewReq = LTCSettleClaimData.LvNewReq;
                LTCSettleClaimData.NoOfDays = c.NoOfDays;
                LTCSettleClaimData.LTC_TYPE = LTCSettleClaimData.LTC_TYPE;
                LTCSettleClaimData.LTCAdvAmt = c.LTCAdvAmt;
                LTCSettleClaimData.LTC_Settle_Amt = c.LTC_Settle_Amt;
                LTCSettleClaimData.LTC_Sanction_Amt = c.LTC_Sanction_Amt;
                LTCSettleClaimData.EncashmentAmount = c.EncashmentAmount;
                LTCSettleClaimData.DateOfAppl = c.DateOfAppl;
                LTCSettleClaimData.LTC_Eligible_Amt = c.LTC_Eligible_Amt;
                LTCSettleClaimData.LTC_Claim_Amt = c.LTC_Claim_Amt;
                using (TransactionScope ts = new TransactionScope())
                {

                    LTCSettleClaimData.DBTrack = new DBTrack
                    {
                        CreatedBy = LTCSettleClaimData.DBTrack.CreatedBy == null ? null : LTCSettleClaimData.DBTrack.CreatedBy,
                        CreatedOn = LTCSettleClaimData.DBTrack.CreatedOn == null ? null : LTCSettleClaimData.DBTrack.CreatedOn,
                        Action = "M",
                        ModifiedBy = SessionManager.UserName,
                        ModifiedOn = DateTime.Now
                    };


                    try
                    {
                        db.LTCSettlementClaim.Attach(LTCSettleClaimData);
                        db.Entry(LTCSettleClaimData).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        db.Entry(LTCSettleClaimData).State = System.Data.Entity.EntityState.Detached;

                        ts.Complete();
                        return this.Json(new { status = true, responseText = "Data Updated Successfully.", JsonRequestBehavior.AllowGet });
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        return RedirectToAction("Create", new { concurrencyError = true, id = LTCSettleClaimData.Id });
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
                    FinalLtcEligibleamt =  LtcEligibleamt;
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


    }
}