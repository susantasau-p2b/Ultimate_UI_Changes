using System;
using EssPortal.App_Start;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using Attendance;
using Leave;
using P2b.Global;
using EssPortal.Security;
using System.Data.OleDb;
using EssPortal.Models;
using System.Data.SqlClient;
using System.Data;
using System.Transactions;
using System.IO;
using System.Web;
using System.Xml.Linq;
using System.Globalization;
using Payroll;

public static class BMSModuleProcess
{
    public class ReturnData_GetSettleAmt
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
        public string TotalHrs { get; set; }
        public double TotalDistance { get; set; }
    }

    public class GetDIARYsettlamt //childgrid
    {

        public double AdvanceAmt { get; set; }
        public double DIARYClaimamt { get; set; }
        public double DIARYElgmamt { get; set; }
        public double SanctionAmt { get; set; }
        public double SettlementAmt { get; set; }
        public double TotalClaimAmt { get; set; }
        public string TotalHrs { get; set; }
        public double TotalDistance { get; set; }
        public double NoOfDays { get; set; }
    }

    public static List<ReturnData_GetSettleAmt> Calculate_TADASettlementAmount(int data, string JourneyObjectlistedit, string travelhotelbookinglist, string MisExpenseObjectlistedit, string data1, int DiaryTypeId)
    {
        using (DataBaseContext db = new DataBaseContext())
        {
            double DAElgAmt = 0;
            double DAClaimAmt = 0;
            double DASetAmt = 0;
            double TAElgAmt = 0;
            double TAClaimAmt = 0;
            double TASetAmt = 0;
            double Hotelelgamt = 0;
            double HotelClaimamt = 0;
            double HotelSetamt = 0;
            double MisElgAmt = 0;
            double MisClaimAmt = 0;
            double MisSetAmt = 0;
            double TotalClaimAmt = 0;
            string TotalHrs = "";
            double TotalDistane = 0;

            List<ReturnData_GetSettleAmt> Return_Final_Amt = new List<ReturnData_GetSettleAmt>();
            var EmpLTCStruct_Details = db.EmployeeLTCStruct
                     .Include(e => e.EmployeeLTCStructDetails)
                     .Include(e => e.EmployeeLTCStructDetails.Select(x => x.PolicyName))
                     .Include(e => e.EmployeeLTCStructDetails.Select(r => r.LTCFormula))
                     .Include(e => e.EmployeeLTCStructDetails.Select(r => r.LTCFormula.DAEligibilityPolicy))
                     .Include(e => e.EmployeeLTCStructDetails.Select(r => r.LTCFormula.DAEligibilityPolicy.Select(z => z.SlabDependRule)))
                      .Include(e => e.EmployeeLTCStructDetails.Select(r => r.LTCFormula.DAEligibilityPolicy.Select(z => z.SlabDependRule.WageRange)))
                     .Where(e => e.EmployeePayroll.Employee_Id == data && e.EndDate == null).FirstOrDefault();

            var travelmodlist = EmpLTCStruct_Details.EmployeeLTCStructDetails.Where(e => e.PolicyName.LookupVal.ToUpper().ToString() == data1.ToUpper().ToString()).Select(e => e.LTCFormula);


            if (JourneyObjectlistedit != null)
            {
                var ids = Utility.StringIdsToListIds(JourneyObjectlistedit);
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
                        // hrs = Convert.ToInt32(tmin.Split(':')[0]);
                        // min = Convert.ToInt32(tmin.Split(':')[1]);

                        hrs = hrs + Convert.ToInt32(tmin.Split(':')[0]);
                        min = min + Convert.ToInt32(tmin.Split(':')[1]);
                        TAElgAmt = TAElgAmt + t.TAElligibleAmt;
                        TAClaimAmt = TAClaimAmt + t.TAClaimAmt;
                        TASetAmt = TASetAmt + t.TASettleAmt;
                    }
                    if (min != 0)
                    {
                        hrs = hrs + (min / 60);
                    }
                    // Da calc start
                    if (travelmodlist != null)
                    {
                        var BMSPolicy = db.BMSModuleTypePolicyAssignment.Include(e => e.DAEligibilityPolicy).Where(e => e.PolicyType_Id == DiaryTypeId).FirstOrDefault();
                        DAEligibilityPolicy dd = null;
                        foreach (var itemfor in travelmodlist)
                        {
                            if (itemfor.DAEligibilityPolicy.Count() > 0)
                            {
                                if (itemfor.DAEligibilityPolicy.Any(e => e.Id == BMSPolicy.DAEligibilityPolicy.FirstOrDefault().Id))
                                {
                                    dd = BMSPolicy.DAEligibilityPolicy.FirstOrDefault();

                                }
                            }
                        }

                        if (BMSPolicy != null && dd != null && BMSPolicy.DAEligibilityPolicy != null)
                        {

                            foreach (var ca in dd.SlabDependRule.WageRange)
                            {
                                if (hrs > ca.WageFrom && hrs <= ca.WageTo)
                                {

                                    DAElgAmt = DAElgAmt + (hrs * ca.Percentage / 100) + ca.Amount;
                                    DAClaimAmt = DAClaimAmt + (hrs * ca.Percentage / 100) + ca.Amount;
                                    DASetAmt = DASetAmt + (hrs * ca.Percentage / 100) + ca.Amount;
                                    TotalHrs = TotalHrs + hrs;
                                }
                            }

                        }
                    }

                }

            }
            if (travelhotelbookinglist != null)
            {
                var ids = Utility.StringIdsToListIds(travelhotelbookinglist);
                var travelhotelbookinglisttada = new List<TravelHotelBooking>();
                var travelhotelbookinlist = db.TravelHotelBooking.Include(e => e.HotelObject).Where(e => ids.Contains(e.Id)).ToList();
                if (travelhotelbookinlist.Count() > 0)
                {
                    foreach (var item in travelhotelbookinlist)
                    {
                        var hoteleligibilitypolicy = item.HotelObject.Where(e => e.HotelDate != null).ToList();
                        Hotelelgamt = Hotelelgamt + hoteleligibilitypolicy.Sum(t => t.HotelElligibleAmt);
                        HotelClaimamt = HotelClaimamt + hoteleligibilitypolicy.Sum(t => t.HotelClaimAmt);
                        HotelSetamt = HotelSetamt + hoteleligibilitypolicy.Sum(t => t.HotelSettleAmt);
                    }

                }

            }

            if (MisExpenseObjectlistedit != null)
            {
                var ids = Utility.StringIdsToListIds(MisExpenseObjectlistedit);
                var MisExpenseObjectlisteditlist = new List<MisExpenseObject>();
                var travelhotelbookinlist = db.MisExpenseObject.Where(e => ids.Contains(e.Id)).ToList();
                if (travelhotelbookinlist.Count() > 0)
                {
                    foreach (var item in travelhotelbookinlist)
                    {

                        MisElgAmt = MisElgAmt + item.MisExpenseElligibleAmt;
                        MisClaimAmt = MisClaimAmt + item.MisExpenseClaimAmt;
                        MisSetAmt = MisSetAmt + item.MisExpenseSettleAmt;
                    }

                }

            }

            // TADASettleamt = Hoteltotalamt + TAclaimamt;
            List<ReturnData_GetSettleAmt> returndata = new List<ReturnData_GetSettleAmt>();

            returndata.Add(new ReturnData_GetSettleAmt
            {
                DAElligibleAmt = DAElgAmt,
                DAClaimAmt = DAClaimAmt,
                DASettleAmt = DASetAmt,
                TAElligibleAmt = TAElgAmt,
                TAClaimAmt = TAClaimAmt,
                TASettleAmt = TASetAmt,

                HotelElligibleAmt = Hotelelgamt,
                HotelClaimAmt = HotelClaimamt,
                HotelSettleAmt = HotelSetamt,
                MisExpenseElligibleAmt = MisElgAmt,
                MisExpenseClaimAmt = MisClaimAmt,
                MisExpenseSettleAmt = MisSetAmt,
                TotalClaimAmt = DAClaimAmt + TAClaimAmt + HotelClaimamt + MisClaimAmt,
                TotalHrs = TotalHrs
            });
            return returndata;
        }
    }

    public static List<GetDIARYsettlamt> Calculate_GetDiaryAmount(int data, string DIARYadvanceclaimlist, string travelhotelbookinglist, string journewdetailslist, string MisExpenseObjectlistedit)
    {

        double TAclaimamt = 0;
        double TAElgamt = 0;
        double TASettamt = 0;
        double advanceclaimamt = 0;
        double hotelamount = 0;
        double hotelelgamount = 0; double MisElgAmt = 0; double MisClaimAmt = 0; double MisSetAmt = 0;
        double DAEligAmt = 0; double DAClaimAmt = 0; double NoOfDays = 0; string TotalHrs = null; double TotalDist = 0;

        using (DataBaseContext db = new DataBaseContext())
        {



            if (DIARYadvanceclaimlist != null && DIARYadvanceclaimlist != "")
            {
                int DIARYadvanceid = Convert.ToInt32(DIARYadvanceclaimlist);
                var advanceclaimdata = db.DIARYAdvanceClaim.Where(e => e.Id == DIARYadvanceid).SingleOrDefault();
                if (advanceclaimdata != null)
                {
                    advanceclaimamt = advanceclaimdata.AdvAmt;
                }
                // c.LTCAdvanceClaim = value;

            }
            if (travelhotelbookinglist != null)
            {
                var ids = Utility.StringIdsToListIds(travelhotelbookinglist);
                var travelhotelbookinglistDIARY = new List<TravelHotelBooking>();
                var travelhotelbookinlist = db.TravelHotelBooking.Include(e => e.HotelEligibilityPolicy).Where(e => ids.Contains(e.Id)).ToList();
                if (travelhotelbookinlist.Count() > 0)
                {
                    var hoteleligibilitypolicy = travelhotelbookinlist.Where(e => e.HotelEligibilityPolicy != null).Select(e => e.HotelEligibilityPolicy);
                    hotelamount = travelhotelbookinlist.Sum(t => t.BillAmount);
                    hotelelgamount = travelhotelbookinlist.Sum(t => t.Elligible_BillAmount);
                }
            }

            if (MisExpenseObjectlistedit != null)
            {
                var ids = Utility.StringIdsToListIds(MisExpenseObjectlistedit);
                var MisExpenseObjectlisteditlist = new List<MisExpenseObject>();
                var travelhotelbookinlist = db.MisExpenseObject.Where(e => ids.Contains(e.Id)).ToList();
                if (travelhotelbookinlist.Count() > 0)
                {
                    foreach (var item in travelhotelbookinlist)
                    {

                        MisElgAmt = MisElgAmt + item.MisExpenseElligibleAmt;
                        MisClaimAmt = MisClaimAmt + item.MisExpenseClaimAmt;
                        MisSetAmt = MisSetAmt + item.MisExpenseSettleAmt;
                    }

                }

            }



            if (journewdetailslist != null && journewdetailslist != "")
            {
                int ids = Convert.ToInt32(journewdetailslist);
                double totalminutes = 0;
                double totalminutesTOT = 0;
                string finaltotalhours = "";
                string finaltotalhoursTOT = "";
                double employeetotalwh = 0, totalemphours = 0, totalhours = 0;
                double employeetotalwhTOT = 0, totalemphoursTOT = 0, totalhoursTOT = 0;
                List<double> totalemphours1 = new List<double>();
                List<double> totalempminutes = new List<double>();
                List<double> totalemphours1TOT = new List<double>();
                List<double> totalempminutesTOT = new List<double>();
                var JourneyObjdata = db.JourneyDetails.Include(e => e.JourneyObject).Include(e => e.MisExpenseObject).Where(e => e.Id == ids).SingleOrDefault();
                foreach (var item in JourneyObjdata.JourneyObject)
                {
                    var JourneyTimediff = Convert.ToDateTime(item.ToDate) - Convert.ToDateTime(item.FromDate);
                    var compare = JourneyTimediff.ToString("hh\\:mm");
                    var sd = compare.Split(':');
                    double minutes = Convert.ToDouble(sd[1]);
                    int hours = Convert.ToInt32(sd[0]);
                    totalminutes += minutes;
                    if (totalminutes != null)
                    {
                        totalhours += hours;
                        totalemphours1.Add(totalhours);
                        totalempminutes.Add(totalminutes);
                    }
                    if (JourneyObjdata.JourneyObject.LastOrDefault().Id == item.Id)
                    {
                        TimeSpan SWorkhours = TimeSpan.FromMinutes(totalempminutes.LastOrDefault());
                        double hours1 = SWorkhours.Hours + totalemphours1.LastOrDefault();
                        finaltotalhours = hours1.ToString() + ":" + SWorkhours.Minutes.ToString();
                    }
                }

                var idmis = JourneyObjdata.MisExpenseObject.Select(e => e.Id);
                var MisExpenseObjectlisteditlist = new List<MisExpenseObject>();
                var misobjectinlist = db.MisExpenseObject.Where(e => idmis.Contains(e.Id)).ToList();
                if (misobjectinlist.Count() > 0)
                {
                    foreach (var item in misobjectinlist)
                    {

                        MisElgAmt = MisElgAmt + item.MisExpenseElligibleAmt;
                        MisClaimAmt = MisClaimAmt + item.MisExpenseClaimAmt;
                        MisSetAmt = MisSetAmt + item.MisExpenseSettleAmt;
                    }

                }
                TAclaimamt = JourneyObjdata.JourneyObject.Sum(e => e.TAClaimAmt);
                TAElgamt = JourneyObjdata.JourneyObject.Sum(e => e.TAElligibleAmt);
                TASettamt = JourneyObjdata.JourneyObject.Sum(e => e.TASettleAmt);
                DAClaimAmt = JourneyObjdata.DAClaimAmt;
                DAEligAmt = JourneyObjdata.DAElligibleAmt;
                NoOfDays = JourneyObjdata.NoOfDays;
                TotalHrs = finaltotalhours.ToString();
                TotalDist = JourneyObjdata.JourneyObject.Sum(e => e.TotalDistance);
            }

            // DIARYSettleamt = Hoteltotalamt + TAclaimamt;
            List<GetDIARYsettlamt> returndata = new List<GetDIARYsettlamt>();

            returndata.Add(new GetDIARYsettlamt
            {
                AdvanceAmt = advanceclaimamt,
                DIARYClaimamt = hotelamount + TAclaimamt + DAClaimAmt + MisClaimAmt,//travel hotel bill amount+journey TAclaim amt
                DIARYElgmamt = hotelamount + TAclaimamt + DAClaimAmt + MisElgAmt,// travel hotel elg amount+ journey elg amount
                SanctionAmt = hotelamount + TAclaimamt + DAClaimAmt + MisSetAmt,
                SettlementAmt = (hotelamount + TAclaimamt + DAClaimAmt + MisSetAmt) - advanceclaimamt,//sanction amount-advance amount
                NoOfDays = NoOfDays,
                TotalHrs = TotalHrs.ToString(),
                TotalDistance = TotalDist
            });

            return returndata;
        }
    }
}