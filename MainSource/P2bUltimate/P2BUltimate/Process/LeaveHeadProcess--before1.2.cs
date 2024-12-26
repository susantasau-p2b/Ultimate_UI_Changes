using P2b.Global;
using P2BUltimate.App_Start;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using Leave;
using P2BUltimate.Security;
namespace P2BUltimate.Process
{
    public static class LeaveHeadProcess
    {
        public class ReturnData
        {
            public double DebitDays { get; set; }
            public int ErrNo { get; set; }
            public double LvnewReqprefix { get; set; }
            public double LvnewReqSuffix { get; set; }
            public bool PrefixSufix { get; set; }

        }

        public class ReturnDatacalendarpara
        {
            public int ErrNo { get; set; }
            public DateTime? Leaveyearfrom { get; set; }
            public DateTime? LeaveyearTo { get; set; }
        }

        public static EmployeeLeave _returnCompanyLeave(Int32 CompanyId, Int32 EmpLVId)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                EmployeeLeave OCompanyLeave = db.EmployeeLeave.Where(e => e.Id == EmpLVId)
                                 .Include(e => e.Employee)
                                 .Include(e => e.LvOpenBal.Select(t => t.LvHead))
                                 .Include(e => e.LvOpenBal.Select(t => t.LvCalendar))
                                 .Include(e => e.LvNewReq)
                                 .Include(e => e.LvNewReq.Select(t => t.LeaveCalendar))
                                 .Include(e => e.LvNewReq.Select(t => t.LeaveHead))
                                 .AsNoTracking().OrderBy(e => e.Id).FirstOrDefault();

                //CompanyLeave OCompanyLeave = db.CompanyLeave.Where(e => e.Company.Id == CompanyId && e.EmployeeLeave.Any(a => a.Id == EmpId))
                //                 .Include(e => e.Company)
                //                 .Include(e => e.EmployeeLeave)
                //                 .Include(e => e.EmployeeLeave.Select(r => r.Employee))
                //                 .Include(e => e.EmployeeLeave.Select(r => r.EmployeeLvStruct))
                //                 .Include(e => e.EmployeeLeave.Select(r => r.LvOpenBal.Select(t => t.LvHead)))
                //                 .Include(e => e.EmployeeLeave.Select(r => r.LvOpenBal.Select(t => t.LvCalendar)))
                //                 .Include(e => e.EmployeeLeave.Select(r => r.LvNewReq))
                //                 .Include(e => e.EmployeeLeave.Select(r => r.LvNewReq.Select(t => t.LeaveCalendar)))
                //                 .Include(e => e.EmployeeLeave.Select(r => r.LvNewReq.Select(t => t.LeaveHead)))
                //                 .Include(e => e.LvCreditPolicy)
                //                 .Include(e => e.LvDebitPolicy.Select(t => t.LvHead))
                //                 .Include(e => e.LvDebitPolicy.Select(t => t.CombineLeaveHeads))
                //                 .Include(e => e.LvEncashPolicy)
                //                 .AsNoTracking().OrderBy(e => e.Id).FirstOrDefault();

                return OCompanyLeave;
            }
        }
        public static ReturnData LeaveValidation(LvNewReq OLvNewReq, int CompanyId, int EmployeeId, Calendar OLeaveCalendar, DateTime? Leaveyearfrom, DateTime? LeaveyearTo)
        {
            DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };
            ReturnData RetData = new ReturnData();

            using (DataBaseContext db = new DataBaseContext())
            {
                db.Configuration.AutoDetectChangesEnabled = false;
                EmployeeLeave oEmployeeLeave = db.EmployeeLeave.Include(e => e.LvNewReq)
                    .Include(e => e.LvNewReq.Select(a => a.LeaveCalendar))
                    .Include(e => e.LvNewReq.Select(a => a.WFStatus))
                    //.Where(e => e.Id == EmployeeId && e.LvNewReq.Any(a => a.LeaveCalendar.Id == OLeaveCalendar.Id)).OrderBy(e => e.Id).SingleOrDefault();
                .Where(e => e.Id == EmployeeId).OrderBy(e => e.Id).SingleOrDefault();
                if (oEmployeeLeave != null)
                {
                    // var LvCalendarFilter = oEmployeeLeave.LvNewReq.Where(e => e.LeaveCalendar.Id == OLeaveCalendar.Id).OrderBy(e => e.Id).ToList();
                    var LvCalendarFilter = oEmployeeLeave.LvNewReq.OrderBy(e => e.Id).ToList();

                    var LvOrignal_id = LvCalendarFilter.Where(e => e.LvOrignal != null).OrderBy(e => e.Id).Select(e => e.LvOrignal.Id).ToList();
                    var AntCancel = LvCalendarFilter.Where(e => e.IsCancel == false && e.TrReject == false).OrderBy(e => e.Id).ToList();
                    var listLvs = AntCancel.Where(e => !LvOrignal_id.Contains(e.Id) && e.WFStatus.LookupVal != "2" && e.WFStatus.LookupVal != "3").OrderBy(e => e.Id).ToList();
                    //  if (listLvs.Where(e => e.FromDate <= OLvNewReq.FromDate && e.ToDate >= OLvNewReq.ToDate).Count() != 0)
                    if (listLvs.Where(e => e.FromDate >= OLvNewReq.FromDate && e.FromDate <= OLvNewReq.ToDate).Count() != 0 ||
                        listLvs.Where(e => e.ToDate >= OLvNewReq.FromDate && e.ToDate <= OLvNewReq.ToDate).Count() != 0)
                    //if (listLvs.Where(e => e.FromDate <= OLvNewReq.FromDate && e.ToDate >= OLvNewReq.ToDate).Count() != 0 || listLvs.Where(e => e.FromDate == OLvNewReq.FromDate || e.ToDate == OLvNewReq.ToDate).Count() != 0)
                    {
                        //already exits
                        RetData.DebitDays = 0;
                        RetData.ErrNo = 16;
                        return RetData;
                    }
                    if (listLvs.Where(e => e.FromDate <= OLvNewReq.FromDate && e.ToDate >= OLvNewReq.ToDate).Count() != 0)
                    {
                        //already exits
                        RetData.DebitDays = 0;
                        RetData.ErrNo = 16;
                        return RetData;
                    }

                }



                List<Int32> LvHeadId = db.LvHead.Select(e => e.Id).ToList();
                List<LvAssignment> OLvAssign = db.LvAssignment.Where(e => !LvHeadId.Contains(e.LvHead.Id)).AsNoTracking().OrderBy(e => e.Id).ToList();
                if (OLvAssign.Count() > 0)
                {
                    RetData.DebitDays = 0;
                    RetData.ErrNo = 26;
                    return RetData; //Leave assignment not done for new head
                }

                var OEmployeeLeave = _returnCompanyLeave(CompanyId, EmployeeId);

                var OEmpLvHistory = new LvNewReq();
                var OEmpLvOpenBalAll = new List<LvOpenBal>();
                if (OEmployeeLeave != null)
                {
                    OEmpLvHistory = OEmployeeLeave.LvNewReq
                                     .Where(e => e.LeaveHead.Id == OLvNewReq.LeaveHead.Id
                        // && e.LeaveCalendar.Id == OLeaveCalendar.Id)
                                      )
                                     .OrderByDescending(e => e.Id).FirstOrDefault();

                    if (OEmpLvHistory == null)
                    {
                        OEmpLvOpenBalAll = OEmployeeLeave.LvOpenBal.Where(e => e.LvCalendar.Id == OLeaveCalendar.Id).OrderBy(e => e.Id).ToList();
                    }
                }

                var OLvSalStruct = db.EmployeeLvStruct
                                         .Include(e => e.EmployeeLvStructDetails)
                                         .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHead))
                                         .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula))
                                         .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvDebitPolicy))
                                         .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHead.LvHeadOprationType))
                                             .Where(e => e.EndDate == null && e.EmployeeLeave_Id == OEmployeeLeave.Id).SingleOrDefault();//.ToList();

                if (OLvSalStruct == null)
                {
                    RetData.DebitDays = 0;
                    RetData.ErrNo = 27;
                    return RetData; //No leave structure available
                }

                var OEmpLvOpenBal = OEmpLvOpenBalAll.Count > 0 ? OEmpLvOpenBalAll.Where(e => e.LvHead.Id == OLvNewReq.LeaveHead.Id).OrderBy(e => e.Id).FirstOrDefault() : null;

                LvDebitPolicy OLvDebit = OLvSalStruct.EmployeeLvStructDetails.Where(e => e.LvHead.Id == OLvNewReq.LeaveHead.Id && e.LvHeadFormula != null && e.LvHeadFormula.LvDebitPolicy != null).Select(r => r.LvHeadFormula.LvDebitPolicy).FirstOrDefault();

                //var OLvDebit = OCompanyLeave.LvDebitPolicy.Where(e => e.LvHead.Id == OLvNewReq.LeaveHead.Id).OrderBy(e => e.Id).FirstOrDefault();
                //var OLvDebit = OEmpLVFormula.LvDebitPolicy;
                var mTotalApplyLv = OLvNewReq.ToDate.Value.CompareTo(OLvNewReq.FromDate.Value);

                if (OLvDebit == null)
                {
                    RetData.DebitDays = 0;
                    RetData.ErrNo = 28;
                    return RetData; //No leave debit policy defined
                }

                /*
                OpenBal Check
                */
                if (OEmpLvHistory == null && OEmpLvOpenBal == null && OEmpLvOpenBal.LvClosing == 0)
                {
                    RetData.DebitDays = 0;
                    RetData.ErrNo = 2;
                    return RetData;//fill the opening balance
                }
                //if (OEmpLvOpenBal.LvClosing == 0)
                //{
                //    RetData.DebitDays = 0; RetData.ErrNo = 2; return RetData;//fill the opening balance
                //}
                if (OEmpLvHistory != null && OEmpLvHistory.CloseBal == 0)
                {
                    RetData.DebitDays = 0; RetData.ErrNo = 2; return RetData;
                    //fill the opening balance
                }


                //leave check in between
                if (OLvNewReq == null)
                {
                    RetData.DebitDays = 0;
                    RetData.ErrNo = 1;

                    return RetData;//fill the requisition properly
                }
                if (OLvNewReq.FromDate > OLvNewReq.ToDate)
                {
                    RetData.DebitDays = 0;
                    RetData.ErrNo = 3;
                    return RetData;
                    //start date should not be more than end date
                }
                double mCloseBal = 0; double mPrefixCount = 0; double mOcc = 0; double mSufixCount = 0; double mLvCount = 0;

                if (OEmpLvHistory != null)
                {
                    mCloseBal = OEmpLvHistory.CloseBal;
                    mOcc = OEmpLvHistory.LvOccurances;
                    mPrefixCount = OEmpLvHistory.PrefixCount;
                    mSufixCount = OEmpLvHistory.SufixCount;
                    mLvCount = OEmpLvHistory.LVCount;
                }
                else
                {
                    mCloseBal = OEmpLvOpenBal.LvClosing;
                    mOcc = OEmpLvOpenBal.LvOccurances;
                    mPrefixCount = OEmpLvOpenBal.PrefixCount;
                    mSufixCount = OEmpLvOpenBal.SufixCount;
                    mLvCount = OEmpLvOpenBal.LVCount;
                }

                //req date between leave year
                //if (OLvNewReq.ReqDate.Value >= OLeaveCalendar.FromDate.Value && OLvNewReq.ReqDate.Value <= OLeaveCalendar.ToDate.Value)
                int monthval = 0;
                double result = 0;
                double days = 0;
                monthval = Math.Abs(((Leaveyearfrom.Value.Year - LeaveyearTo.Value.Year) * 12) + Leaveyearfrom.Value.Month - LeaveyearTo.Value.Month) + 1;
                DateTime leaveyearfromChk;
                DateTime leaveyeartoChk;
                leaveyearfromChk = (Leaveyearfrom.Value).AddMonths(monthval);
                leaveyeartoChk = (LeaveyearTo.Value).AddMonths(monthval);
                var lvcalendar = db.Calendar.Where(e => e.Name.LookupVal.ToUpper() == "LEAVECALENDAR" && e.Default == true && e.FromDate >= leaveyearfromChk && e.ToDate <= leaveyeartoChk).FirstOrDefault();

                if (OLvNewReq.ReqDate.Value >= leaveyearfromChk && OLvNewReq.ReqDate.Value <= leaveyeartoChk && (OLvNewReq.FromDate.Value > leaveyearfromChk) && lvcalendar == null)
                {
                    RetData.DebitDays = 0;
                    RetData.ErrNo = 5;
                    return RetData;//out off leave year  
                }

                //if (OLvNewReq.ReqDate.Value >= Leaveyearfrom.Value && OLvNewReq.ReqDate.Value <= LeaveyearTo.Value)
                //{

                //}
                //else
                //{

                //    RetData.DebitDays = 0;
                //    RetData.ErrNo = 5;
                //    return RetData;//out off leave year
                //}
                if (OLvNewReq.FromDate.Value > LeaveyearTo.Value)
                {
                    RetData.DebitDays = 0;
                    RetData.ErrNo = 5;
                    return RetData;//out off leave year
                }
                if (OLvNewReq.FromDate.Value < Leaveyearfrom.Value)
                {
                    RetData.DebitDays = 0;
                    RetData.ErrNo = 5;
                    return RetData;//out off leave year
                }

                //leave debit policy check
                if (OLvDebit != null)
                {
                    if (OLvNewReq.ToDate.Value < OLvNewReq.FromDate.Value)
                    {
                        RetData.DebitDays = 0;
                        RetData.ErrNo = 6;
                        return RetData;//end date should be more than start date
                    }

                    if (mCloseBal < OLvDebit.MinUtilDays)
                    {
                        RetData.DebitDays = 0;
                        RetData.ErrNo = 7;
                        return RetData;//lv balance is less that min apply leave
                    }

                    if (OLvDebit.PreApplied == true && OLvDebit.PostApplied == false && OLvNewReq.FromDate.Value < (DateTime.Now.AddDays(OLvDebit.PreDays)))
                    {
                        RetData.DebitDays = 0;
                        RetData.ErrNo = 8;
                        return RetData;//from date should be more than Preapplied grace
                    }

                    if (OLvDebit.PostApplied == true && OLvDebit.PreApplied == false && OLvNewReq.FromDate.Value < (DateTime.Now.AddDays(-OLvDebit.PostDays)))
                    {
                        RetData.DebitDays = 0;
                        RetData.ErrNo = 9;
                        return RetData;//from date should be less than Post Applied grace
                    }
                    if (OLvDebit.PostApplied == false && OLvDebit.PreApplied == true && OLvNewReq.FromDate.Value < DateTime.Now)
                    {
                        RetData.DebitDays = 0;
                        RetData.ErrNo = 17;
                        return RetData;
                    }
                    if (OLvDebit.PreApplied == false && OLvDebit.PostApplied == true && OLvNewReq.FromDate.Value > DateTime.Now)
                    {
                        RetData.DebitDays = 0;
                        RetData.ErrNo = 18;
                        return RetData;
                    }

                    // Surendra Start

                    if (OLvDebit.PreApplied == true && OLvDebit.PostApplied == true && OLvNewReq.FromDate.Value < (DateTime.Now.AddDays(OLvDebit.PreDays)))
                    {
                        RetData.DebitDays = 0;
                        RetData.ErrNo = 8;
                        return RetData;//from date should be more than Preapplied grace
                    }

                    if (OLvDebit.PostApplied == true && OLvDebit.PreApplied == true && OLvNewReq.FromDate.Value < (DateTime.Now.AddDays(-OLvDebit.PostDays)))
                    {
                        RetData.DebitDays = 0;
                        RetData.ErrNo = 9;
                        return RetData;//from date should be less than Post Applied grace
                    }


                    // Surendra end

                    //ML and MCL check pending
                    if (OLvDebit.PrefixMaxCount != 0 && mPrefixCount == OLvDebit.PrefixMaxCount)
                    {
                        RetData.DebitDays = 0;
                        RetData.ErrNo = 10;
                        return RetData;//lv prefix/suffix leave apply limit over
                    }
                    if (OLvDebit.ApplyFutureGraceMonths > 0)
                    {

                        if (OLvNewReq.FromDate > DateTime.Now.AddMonths(OLvDebit.ApplyFutureGraceMonths).Date)
                        {
                            RetData.DebitDays = 0;
                            RetData.ErrNo = 11;
                            return RetData;//Future leave can not be applied beyond OLvDebit.ApplyFutureGraceMonths months
                        }
                    }
                    if (OLvDebit.ApplyPastGraceMonths > 0)
                    {
                        if (OLvNewReq.FromDate < DateTime.Now.AddMonths(-OLvDebit.ApplyPastGraceMonths).Date)
                        {
                            RetData.DebitDays = 0;
                            RetData.ErrNo = 12;
                            return RetData;//past leave can not be applied beyond OLvDebit.ApplyPastGraceMonths months
                        }
                    }



                    double mDebitDays = (OLvNewReq.ToDate.Value.Date - OLvNewReq.FromDate.Value.Date).Days + 1;

                    //halfway allowed?
                    if (OLvDebit.HalfDayAppl == false)
                    {
                        if (OLvNewReq.FromStat.LookupVal.ToUpper() != "FULLSESSION" && OLvNewReq.ToStat.LookupVal.ToUpper() != "FULLSESSION")
                        {
                            RetData.DebitDays = 0;
                            RetData.ErrNo = 13;
                            return RetData;//Half day leave not allowed
                        }

                    }
                    if ((OLvNewReq.FromStat.LookupVal.ToUpper() != "FULLSESSION") && (OLvNewReq.ToStat.LookupVal.ToUpper() == "FULLSESSION"))
                    {
                        mDebitDays = mDebitDays - 0.5; //debit half day leave
                    }
                    if ((OLvNewReq.ToStat.LookupVal.ToUpper() != "FULLSESSION") && (OLvNewReq.FromStat.LookupVal.ToUpper() == "FULLSESSION"))
                    {
                        mDebitDays = mDebitDays - 0.5; //debit half day leave
                    }

                    if ((OLvNewReq.ToStat.LookupVal.ToUpper() != "FULLSESSION") && (OLvNewReq.FromStat.LookupVal.ToUpper() != "FULLSESSION"))
                    {
                        mDebitDays = mDebitDays / 2; // - 0.5; //debit half day leave
                    }


                    if (OLvDebit.LvHead.HFPay == true)
                    {
                        mDebitDays = mDebitDays * 2;
                    }
                    else
                    {
                        mDebitDays = mDebitDays;
                    }
                    var ComId = Convert.ToInt32(SessionManager.CompanyId);
                    var ComCode = db.Company.Where(e => e.Id == ComId).AsNoTracking().OrderBy(e => e.Id).SingleOrDefault().Code;
                    if (ComCode.ToUpper() != "KDCC")
                    {
                        var oDateIsWeaklyOff = DateIsWeaklyOff(OLvNewReq, EmployeeId);
                        if (oDateIsWeaklyOff == 0)
                        {
                            RetData.DebitDays = 0;
                            RetData.ErrNo = 20;
                            return RetData;
                        }
                    }


                    if (ComCode.ToUpper() != "BDCB" && ComCode.ToUpper() != "KDCC")
                    {
                        //holiday check
                        //check to from date on holiday
                        var oDateIsHolidayDay = DateIsHolidayDay(OLvNewReq, EmployeeId);
                        if (oDateIsHolidayDay == 0)
                        {
                            RetData.DebitDays = 0;
                            RetData.ErrNo = 19;
                            return RetData;
                        }
                        var Holidaycount = HolidayCount(OLvNewReq, EmployeeId);

                        if (Holidaycount == 100)
                        {
                            RetData.DebitDays = 0;
                            RetData.ErrNo = 100;
                            return RetData;//holiday calendar not defined : throw alert message
                        }
                        if (Holidaycount == 101)
                        {
                            RetData.DebitDays = 0;
                            RetData.ErrNo = 101; //return 101;//holiday calendar not defined for leave applied: throw alert message
                            return RetData;
                        }
                        if (Holidaycount == 102)
                        {
                            RetData.DebitDays = 0;
                            RetData.ErrNo = 102;
                            return RetData; //  return 102;//weekly off calendar not defined : throw alert message
                        }
                        if (Holidaycount == 103)
                        {

                            RetData.DebitDays = 0;
                            RetData.ErrNo = 103;
                            return RetData; //return 103;//Weekly off calendar not defined for leave applied: throw alert message
                        }
                        if (Holidaycount == 104)
                        {

                            RetData.DebitDays = 0;
                            RetData.ErrNo = 104;
                            return RetData; //return 103;//Weekly off calendar not defined for leave applied: throw alert message
                        }
                        if (OLvDebit.HolidayInclusive == false)
                        {
                            mDebitDays = mDebitDays - HolidayCount(OLvNewReq, EmployeeId); //debit holidays
                        }

                    }
                    //Combine check


                    //if (OLvDebit.Combined == false)
                    //{
                    //    var a = PrefixSufixCountLeavehead("BOTH", OLvNewReq, OCompanyLeave.Company.Id, OEmployeeLeave.Employee.Id, OLvNewReq.LeaveCalendar);
                    //    if (a.error == 0)
                    //    {

                    //        if (a._TotalDays == 0)
                    //        {
                    //            RetData.DebitDays = 0;
                    //            RetData.ErrNo = 21;
                    //            return RetData;
                    //        }

                    //    }
                    //    else
                    //    {
                    //        RetData.DebitDays = mDebitDays;
                    //        RetData.ErrNo = a.error;
                    //    }


                    //}
                    if (OLvDebit.Combined == true || OLvDebit.Combined == false)
                    {
                        var a = PrefixSufixCountLeavehead("BOTH", OLvNewReq, CompanyId, OEmployeeLeave.Employee.Id, OLvNewReq.LeaveCalendar);
                        DateTime mdate;
                        //if (OLvDebit.CombineLeaveHeads != null)
                        //{
                        if (a.mTotPrefix > 0)
                        {
                            mdate = OLvNewReq.FromDate.Value.AddDays(-(a.mTotPrefix + 1));
                        }
                        else
                        {
                            mdate = OLvNewReq.FromDate.Value.AddDays(-(1));
                        }
                        var listLvscnt = DayLeaveHeadValidation(mdate, CompanyId, EmployeeId, OLeaveCalendar, OLvNewReq.LeaveHead.Id, "");
                        // var oCombineChk = OLvDebit.CombineLeaveHeads.Where(e => e.LvCode == OLvNewReq.LeaveHead.LvCode).ToList();
                        if (listLvscnt.lvrecc == "NLR")
                        {
                            RetData.DebitDays = 0;
                            RetData.ErrNo = 22;
                            return RetData;
                        }
                        else
                        {
                            RetData.DebitDays = mDebitDays;
                            RetData.ErrNo = a.error;
                        }

                        if (a.mTotSuffix > 0)
                        {
                            mdate = OLvNewReq.ToDate.Value.AddDays(a.mTotSuffix + 1);
                        }
                        else
                        {
                            mdate = OLvNewReq.ToDate.Value.AddDays(1);
                        }

                        listLvscnt = DayLeaveHeadValidation(mdate, CompanyId, EmployeeId, OLeaveCalendar, OLvNewReq.LeaveHead.Id, "");
                        if (listLvscnt.lvrecc == "NLR")
                        {
                            RetData.DebitDays = 0;
                            RetData.ErrNo = 22;
                            return RetData;
                        }
                        else
                        {
                            RetData.DebitDays = mDebitDays;
                            RetData.ErrNo = a.error;
                        }

                    }

                    ////if (OLvDebit.Combined == true || OLvDebit.Combined == false)
                    ////{
                    ////    var a = PrefixSufixCountLeavehead("BOTH", OLvNewReq, CompanyId, OEmployeeLeave.Employee.Id, OLvNewReq.LeaveCalendar);
                    ////    DateTime mdate;
                    ////    //if (OLvDebit.CombineLeaveHeads != null)
                    ////    //{
                    ////    if (a.mTotPrefix > 0)
                    ////    {
                    ////        mdate = OLvNewReq.FromDate.Value.AddDays(-(a.mTotPrefix + 1));
                    ////    }
                    ////    else
                    ////    {
                    ////        mdate = OLvNewReq.FromDate.Value.AddDays(-(1));
                    ////    }
                    ////    var listLvscnt = DayLeaveHeadValidation(mdate, CompanyId, EmployeeId, OLeaveCalendar, OLvNewReq.LeaveHead.Id, "");
                    ////    // var oCombineChk = OLvDebit.CombineLeaveHeads.Where(e => e.LvCode == OLvNewReq.LeaveHead.LvCode).ToList();
                    ////    if (listLvscnt.lvrecc == "NLR")
                    ////    {
                    ////        RetData.DebitDays = 0;
                    ////        RetData.ErrNo = 22;
                    ////        return RetData;
                    ////    }
                    ////    else
                    ////    {
                    ////        RetData.DebitDays = mDebitDays;
                    ////        RetData.ErrNo = a.error;
                    ////    }

                    ////    if (a.mTotSuffix > 0)
                    ////    {
                    ////        mdate = OLvNewReq.ToDate.Value.AddDays(a.mTotSuffix + 1);
                    ////    }
                    ////    else
                    ////    {
                    ////        mdate = OLvNewReq.ToDate.Value.AddDays(1);
                    ////    }

                    ////    listLvscnt = DayLeaveHeadValidation(mdate, CompanyId, EmployeeId, OLeaveCalendar, OLvNewReq.LeaveHead.Id, "");
                    ////    if (listLvscnt.lvrecc == "NLR")
                    ////    {
                    ////        RetData.DebitDays = 0;
                    ////        RetData.ErrNo = 22;
                    ////        return RetData;
                    ////    }
                    ////    else
                    ////    {
                    ////        RetData.DebitDays = mDebitDays;
                    ////        RetData.ErrNo = a.error;
                    ////    }

                    ////  }

                    //if (OLvDebit.Combined == false)
                    //{
                    //    var a = PrefixSufixCountLeavehead("BOTH", OLvNewReq, CompanyId, OEmployeeLeave.Employee.Id, OLvNewReq.LeaveCalendar);

                    //    if (a.error == 0)
                    //    {

                    //        if (a._TotalDays != 0)
                    //        {
                    //            RetData.DebitDays = 0;
                    //            RetData.ErrNo = 21;
                    //            return RetData;
                    //        }

                    //    }
                    //    else
                    //    {
                    //        RetData.DebitDays = mDebitDays;
                    //        RetData.ErrNo = a.error;
                    //    }

                    //}
                    //if (OLvDebit.Combined == true)
                    //{
                    //    var a = PrefixSufixCountLeavehead("BOTH", OLvNewReq, CompanyId, OEmployeeLeave.Employee.Id, OLvNewReq.LeaveCalendar);
                    //    if (OLvDebit.CombineLeaveHeads != null)
                    //    {
                    //        var oCombineChk = OLvDebit.CombineLeaveHeads.Where(e => e.LvCode.Contains(OLvDebit.LvHead.LvCode)).ToList();
                    //        if (oCombineChk == null)
                    //        {
                    //            RetData.DebitDays = 0;
                    //            RetData.ErrNo = 22;
                    //            return RetData;
                    //        }
                    //        else
                    //        {
                    //            RetData.DebitDays = mDebitDays;
                    //            RetData.ErrNo = a.error;
                    //        }
                    //    }
                    //    else 
                    //    {
                    //        if (a.error == 0)
                    //        {
                    //            if (a._TotalDays != 0)
                    //            {
                    //                RetData.DebitDays = 0;
                    //                RetData.ErrNo = 22;
                    //                return RetData;
                    //            }

                    //        }
                    //        else
                    //        {
                    //            RetData.DebitDays = mDebitDays;
                    //            RetData.ErrNo = a.error;
                    //        }

                    //    }


                    //    //if (ComCode.ToString() == "KDCC")
                    //    //{
                    //    //    if (OLvNewReq.LeaveHead.LvCode == "PL")
                    //    //    {  
                    //    //           var oCombineChk = OLvDebit.CombineLeaveHeads.Where(e => e.LvCode == "SL").SingleOrDefault();
                    //    //           if (oCombineChk != null)
                    //    //           {
                    //    //               LvNewReq OEmpLvHistorySL = new LvNewReq();
                    //    //               OEmpLvHistorySL = OEmployeeLeave.LvNewReq
                    //    //             .Where(e => e.LeaveHead.Id == oCombineChk.Id
                    //    //             && e.LeaveCalendar.Id == OLeaveCalendar.Id && e.ToDate == OLvNewReq.FromDate.Value.AddDays(-1))
                    //    //             .OrderByDescending(e => e.Id).FirstOrDefault();
                    //    //                 if (OEmpLvHistorySL != null && OEmpLvHistorySL.CloseBal != 0)
                    //    //                 {
                    //    //                     RetData.DebitDays = 0;
                    //    //                     RetData.ErrNo = 23;
                    //    //                     return RetData; //You can apply PL combine with SL only if SL is 0. 
                    //    //                 }
                    //    //           }

                    //    //    }
                    //    //}
                    //}
                    // Surendra Start
                    //applyPrfixSufix
                    double mLvnewReqprefix = 0;
                    double mLvnewReqSuffix = 0;
                    if (OLvDebit.Sandwich == true)
                    {
                        var a = PrefixSufixCount("BOTH", OLvNewReq, CompanyId, OEmployeeLeave.Employee.Id, OLvNewReq.LeaveCalendar);
                        if (a.error == 0)
                        {
                            if (ComCode.ToUpper() == "KDCC")
                            {
                                var pre = PrefixSufixCount("PRE", OLvNewReq, CompanyId, OEmployeeLeave.Employee.Id, OLvNewReq.LeaveCalendar);
                                var pos = PrefixSufixCount("POST", OLvNewReq, CompanyId, OEmployeeLeave.Employee.Id, OLvNewReq.LeaveCalendar);

                                //mLvnewReqprefix=pre.mTotPrefix;
                                //mLvnewReqSuffix=pos.mTotSuffix;

                                if (pre.mTotPrefix > 0 && pos.mTotSuffix > 0)
                                {
                                    RetData.PrefixSufix = true;
                                    if (OLvNewReq.LeaveHead.LvCode == "CL")
                                    {
                                        var OEmpLvHistoryall = OEmployeeLeave.LvNewReq
                                     .Any(e => e.LeaveHead.Id == OLvNewReq.LeaveHead.Id
                                     && e.LeaveCalendar.Id == OLeaveCalendar.Id && e.PrefixSuffix == true);

                                        if (OEmpLvHistoryall != false)
                                        {
                                            mDebitDays = mDebitDays + 1;
                                        }
                                    }
                                    else if (OLvNewReq.LeaveHead.LvCode == "SL" || OLvNewReq.LeaveHead.LvCode == "PL")
                                    {
                                        mDebitDays = mDebitDays + 1;
                                    }

                                }
                            }
                            else
                            {
                                if (a._TotalDays != 0)
                                {
                                    mDebitDays = mDebitDays + a._TotalDays; //+ 1; //debit holidays
                                }
                            }
                        }
                        else
                        {
                            RetData.DebitDays = mDebitDays;
                            RetData.ErrNo = a.error;
                        }
                    }
                    if (OLvDebit.PrefixSuffix == true)
                    {
                        var a = PrefixSufixCount("BOTH", OLvNewReq, CompanyId, OEmployeeLeave.Employee.Id, OLvNewReq.LeaveCalendar);
                        if (a.error == 0)
                        {
                            mDebitDays = mDebitDays + a._TotalDays; //debit holidays
                            if (a._isPrifix == true)
                            {
                                mLvnewReqprefix = (mPrefixCount + 1);

                            }
                            else
                            {
                                mLvnewReqprefix = mPrefixCount;
                            }
                            if (a._isSufix == true)
                            {
                                mLvnewReqSuffix = mSufixCount + 1;
                            }
                            else
                            {
                                mLvnewReqSuffix = mSufixCount;
                            }

                        }
                        else
                        {
                            RetData.DebitDays = mDebitDays;
                            RetData.ErrNo = a.error;
                        }
                    }

                    //preapplyPrfix
                    if (OLvDebit.PreApplyPrefixSuffix == true && OLvNewReq.FromDate.Value > DateTime.Now)
                    {
                        var a = PrefixSufixCount("PRE", OLvNewReq, CompanyId, OEmployeeLeave.Employee.Id, OLvNewReq.LeaveCalendar);
                        if (a.error == 0)
                        {
                            // Prefix suffix Leave above max grace grace count
                            if ((mPrefixCount + mSufixCount) >= OLvDebit.PrefixMaxCount)
                            {
                                mDebitDays = mDebitDays + a._TotalDays + 1; //debit holidays
                                if (a._isPrifix == true)
                                {
                                    mLvnewReqprefix = (mPrefixCount + 1);

                                }
                                else
                                {
                                    mLvnewReqprefix = mPrefixCount;
                                }
                                if (a._isSufix == true)
                                {
                                    mLvnewReqSuffix = mSufixCount + 1;
                                }
                                else
                                {
                                    mLvnewReqSuffix = mSufixCount;
                                }
                            }
                            // Prefix suffix Leave above minimum grace count
                            else if ((mPrefixCount + mSufixCount) >= OLvDebit.PrefixGraceCount)
                            {
                                mDebitDays = mDebitDays + a._TotalDays; //debit holidays
                                if (a._isPrifix == true)
                                {
                                    mLvnewReqprefix = (mPrefixCount + 1);

                                }
                                else
                                {
                                    mLvnewReqprefix = mPrefixCount;
                                }
                                if (a._isSufix == true)
                                {
                                    mLvnewReqSuffix = mSufixCount + 1;
                                }
                                else
                                {
                                    mLvnewReqSuffix = mSufixCount;
                                }
                            }
                            // Prefix suffix Leave Below minimum grace  count
                            else
                            {
                                mDebitDays = mDebitDays;
                                if (a._isPrifix == true)
                                {
                                    mLvnewReqprefix = (mPrefixCount + 1);

                                }
                                else
                                {
                                    mLvnewReqprefix = mPrefixCount;
                                }
                                if (a._isSufix == true)
                                {
                                    mLvnewReqSuffix = mSufixCount + 1;
                                }
                                else
                                {
                                    mLvnewReqSuffix = mSufixCount;
                                }
                            }
                        }
                        else
                        {
                            RetData.DebitDays = mDebitDays;
                            RetData.ErrNo = a.error;
                        }

                        //mDebitDays = mDebitDays - PrefixSufixCount("PRE", OLvNewReq, OCompanyLeave.Company.Id); //debit holidays
                    }
                    //post apply prefix
                    //preapplyPrfix
                    if (OLvDebit.PostApplyPrefixSuffix == true && OLvNewReq.FromDate.Value < DateTime.Now)
                    {
                        var a = PrefixSufixCount("POST", OLvNewReq, CompanyId, OEmployeeLeave.Employee.Id, OLvNewReq.LeaveCalendar);
                        if (a.error == 0)
                        {
                            // Prefix suffix Leave above max grace grace count
                            if ((mPrefixCount + mSufixCount) >= OLvDebit.PrefixMaxCount)
                            {
                                mDebitDays = mDebitDays + a._TotalDays + 1; //debit holidays
                                if (a._isPrifix == true)
                                {
                                    mLvnewReqprefix = (mPrefixCount + 1);

                                }
                                else
                                {
                                    mLvnewReqprefix = mPrefixCount;
                                }
                                if (a._isSufix == true)
                                {
                                    mLvnewReqSuffix = mSufixCount + 1;
                                }
                                else
                                {
                                    mLvnewReqSuffix = mSufixCount;
                                }
                            }
                            // Prefix suffix Leave above minimum grace count
                            else if ((mPrefixCount + mSufixCount) >= OLvDebit.PrefixGraceCount)
                            {
                                mDebitDays = mDebitDays + a._TotalDays; //debit holidays
                                if (a._isPrifix == true)
                                {
                                    mLvnewReqprefix = (mPrefixCount + 1);

                                }
                                else
                                {
                                    mLvnewReqprefix = mPrefixCount;
                                }
                                if (a._isSufix == true)
                                {
                                    mLvnewReqSuffix = mSufixCount + 1;
                                }
                                else
                                {
                                    mLvnewReqSuffix = mSufixCount;
                                }
                            }
                            // Prefix suffix Leave Below minimum grace  count
                            else
                            {
                                mDebitDays = mDebitDays;
                                if (a._isPrifix == true)
                                {
                                    mLvnewReqprefix = (mPrefixCount + 1);

                                }
                                else
                                {
                                    mLvnewReqprefix = mPrefixCount;
                                }
                                if (a._isSufix == true)
                                {
                                    mLvnewReqSuffix = mSufixCount + 1;
                                }
                                else
                                {
                                    mLvnewReqSuffix = mSufixCount;
                                }
                            }
                            //mDebitDays = mDebitDays + a._TotalDays; //debit holidays
                            //if (a._isPrifix == true)
                            //{
                            //    mLvnewReqprefix = (mPrefixCount + 1);

                            //}
                            //else
                            //{
                            //    mLvnewReqprefix = mPrefixCount;
                            //}
                            //if (a._isSufix == true)
                            //{
                            //    mLvnewReqSuffix = mSufixCount + 1;
                            //}
                            //else
                            //{
                            //    mLvnewReqSuffix = mSufixCount;
                            //}
                        }
                        else
                        {
                            RetData.DebitDays = mDebitDays;
                            RetData.ErrNo = a.error;
                        }

                        // mDebitDays = mDebitDays - PrefixSufixCount("POST", OLvNewReq, OCompanyLeave.Company.Id); //debit holidays
                    }
                    // Surendra End

                    if (ComCode.ToString() == "KDCC")
                    {
                        if (OLvNewReq.LeaveHead.LvCode != "SL")
                        {
                            if (mDebitDays > mCloseBal)
                            {
                                RetData.DebitDays = 0;
                                RetData.ErrNo = 14;
                                return RetData; //Leave can not be applied beyond maximum limit
                            }
                            if (mDebitDays > OLvDebit.MaxUtilDays)
                            {
                                RetData.DebitDays = 0;
                                RetData.ErrNo = 14;
                                return RetData; //Leave can not be applied beyond maximum limit
                            }
                        }

                    }
                    else
                    {
                        if (mDebitDays > mCloseBal)
                        {
                            RetData.DebitDays = 0;
                            RetData.ErrNo = 14;
                            return RetData; //Leave can not be applied beyond maximum limit
                        }
                        if (mDebitDays > OLvDebit.MaxUtilDays)
                        {
                            RetData.DebitDays = 0;
                            RetData.ErrNo = 14;
                            return RetData; //Leave can not be applied beyond maximum limit
                        }
                    }


                    if (mDebitDays < OLvDebit.MinUtilDays)
                    {
                        RetData.DebitDays = 0;
                        RetData.ErrNo = 15;
                        return RetData; //Leave can not be applied beyond minimum limit
                    }


                    //in between close

                    RetData.DebitDays = mDebitDays;
                    RetData.ErrNo = 0;
                    RetData.LvnewReqprefix = mLvnewReqprefix;
                    RetData.LvnewReqSuffix = mLvnewReqSuffix;

                    return RetData;
                }
                //prepost leave combine check
                //if(PrePostCombineLeaveChk==true)
                //{
                //     return 6; //Leave can not be combined with other leave
                //}

                RetData.DebitDays = 0;
                RetData.ErrNo = 0;

                return RetData;

            }
        }
        public static ReturnDatacalendarpara LeaveCalendarpara(int LVheadid, int EmployeeLvId)
        {
            ReturnDatacalendarpara RetDataparam = new ReturnDatacalendarpara();
            using (DataBaseContext db = new DataBaseContext())
            {
                var CompLvId = Convert.ToInt32(SessionManager.CompLvId);
                //var CompCreditPolicyList = db.CompanyLeave
                //.Include(e => e.LvCreditPolicy)
                //.Include(e => e.LvCreditPolicy.Select(a => a.ConvertLeaveHead))
                //.Include(e => e.LvCreditPolicy.Select(a => a.LvHead))
                //.Include(e => e.LvCreditPolicy.Select(a => a.CreditDate))
                //.Include(e => e.LvCreditPolicy.Select(a => a.ConvertLeaveHeadBal))
                //.Include(e => e.LvCreditPolicy.Select(a => a.ExcludeLeaveHeads))
                //.Where(e => e.Id == CompLvId).FirstOrDefault();

                var OLvSalStruct = db.EmployeeLvStruct
                                        .Include(e => e.EmployeeLvStructDetails)
                                        .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvCreditPolicy))
                                        .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvCreditPolicy.ConvertLeaveHead))
                                        .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvCreditPolicy.LvHead))
                                        .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvCreditPolicy.CreditDate))
                                        .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvCreditPolicy.ConvertLeaveHeadBal))
                                        .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvCreditPolicy.ExcludeLeaveHeads))
                                            .Where(e => e.EndDate == null && e.EmployeeLeave_Id == EmployeeLvId).SingleOrDefault();

                LvCreditPolicy oLvCreditPolicy = OLvSalStruct.EmployeeLvStructDetails.Where(e => e.LvHead.Id == LVheadid && e.LvHeadFormula != null
                    && e.LvHeadFormula.LvCreditPolicy != null).Select(r => r.LvHeadFormula.LvCreditPolicy).FirstOrDefault();


                //LvCreditPolicy oLvCreditPolicy = CompCreditPolicyList.LvCreditPolicy.Where(e => e.CreditDate != null &&
                //            e.LvHead != null && e.LvHead.Id == LVheadid).SingleOrDefault();

                if (oLvCreditPolicy == null)
                {

                    RetDataparam.ErrNo = 24;
                    return RetDataparam;


                }
                LvNewReq aa = null;
                DateTime? Leaveyearfrom = null;
                DateTime? LeaveyearTo = null;
                var Fulldate = "";

                switch (oLvCreditPolicy.CreditDate.LookupVal.ToUpper())
                {
                    case "CALENDAR":
                        var LvNewReqDatac = db.EmployeeLeave.Include(q => q.Employee).Include(q => q.LvNewReq).Include(q => q.LvNewReq.Select(t => t.LeaveHead)).Where(q => q.Id == EmployeeLvId).FirstOrDefault();
                        if (LvNewReqDatac != null)
                        {
                            aa = LvNewReqDatac.LvNewReq.Where(q => q.LvCreditDate != null && q.LeaveHead.Id == LVheadid).OrderByDescending(q => q.Id).FirstOrDefault();
                        }

                        if (aa != null)
                        {
                            Fulldate = aa.LvCreditDate.Value.ToShortDateString();
                            Leaveyearfrom = Convert.ToDateTime(Fulldate);
                            LeaveyearTo = Leaveyearfrom.Value.AddDays(-1);
                            LeaveyearTo = LeaveyearTo.Value.AddYears(1);
                            RetDataparam.Leaveyearfrom = Leaveyearfrom;
                            RetDataparam.LeaveyearTo = LeaveyearTo;
                            RetDataparam.ErrNo = 0;
                        }
                        else
                        {
                            RetDataparam.ErrNo = 25;

                        }


                        //RetDataparam.Leaveyearfrom = Leaveyearfrom;
                        //RetDataparam.LeaveyearTo = LeaveyearTo;
                        //return RetDataparam;
                        break;
                    case "JOININGDATE":

                        var LvNewReqData = db.EmployeeLeave.Include(q => q.Employee).Include(q => q.LvNewReq).Include(q => q.LvNewReq.Select(t => t.LeaveHead)).Where(q => q.Id == EmployeeLvId).FirstOrDefault();
                        if (LvNewReqData != null)
                        {
                            aa = LvNewReqData.LvNewReq.Where(q => q.LvCreditDate != null && q.LeaveHead.Id == LVheadid).OrderByDescending(q => q.Id).FirstOrDefault();
                        }

                        if (aa != null)
                        {
                            Fulldate = aa.LvCreditDate.Value.ToShortDateString();
                            Leaveyearfrom = Convert.ToDateTime(Fulldate);
                            LeaveyearTo = Leaveyearfrom.Value.AddDays(-1);
                            LeaveyearTo = LeaveyearTo.Value.AddYears(1);
                            RetDataparam.Leaveyearfrom = Leaveyearfrom;
                            RetDataparam.LeaveyearTo = LeaveyearTo;
                            RetDataparam.ErrNo = 0;
                        }
                        else
                        {
                            RetDataparam.ErrNo = 25;

                        }


                        //RetDataparam.Leaveyearfrom = Leaveyearfrom;
                        //RetDataparam.LeaveyearTo = LeaveyearTo;
                        //return RetDataparam;

                        break;
                    case "CONFIRMATIONDATE":
                        var LvNewReqDatacf = db.EmployeeLeave.Include(q => q.Employee).Include(q => q.LvNewReq).Include(q => q.LvNewReq.Select(t => t.LeaveHead)).Where(q => q.Id == EmployeeLvId).FirstOrDefault();
                        if (LvNewReqDatacf != null)
                        {
                            aa = LvNewReqDatacf.LvNewReq.Where(q => q.LvCreditDate != null && q.LeaveHead.Id == LVheadid).OrderByDescending(q => q.Id).FirstOrDefault();
                        }

                        if (aa != null)
                        {
                            Fulldate = aa.LvCreditDate.Value.ToShortDateString();
                            Leaveyearfrom = Convert.ToDateTime(Fulldate);
                            LeaveyearTo = Leaveyearfrom.Value.AddDays(-1);
                            LeaveyearTo = LeaveyearTo.Value.AddYears(1);
                            RetDataparam.Leaveyearfrom = Leaveyearfrom;
                            RetDataparam.LeaveyearTo = LeaveyearTo;
                            RetDataparam.ErrNo = 0;
                        }
                        else
                        {
                            RetDataparam.ErrNo = 25;

                        }
                        //RetDataparam.Leaveyearfrom = Leaveyearfrom;
                        //RetDataparam.LeaveyearTo = LeaveyearTo;
                        //return RetDataparam;

                        break;
                    case "INCREMENTDATE":
                        var LvNewReqDatai = db.EmployeeLeave.Include(q => q.Employee).Include(q => q.LvNewReq).Include(q => q.LvNewReq.Select(t => t.LeaveHead)).Where(q => q.Id == EmployeeLvId).FirstOrDefault();
                        if (LvNewReqDatai != null)
                        {
                            aa = LvNewReqDatai.LvNewReq.Where(q => q.LvCreditDate != null && q.LeaveHead.Id == LVheadid).OrderByDescending(q => q.Id).FirstOrDefault();
                        }

                        if (aa != null)
                        {
                            Fulldate = aa.LvCreditDate.Value.ToShortDateString();
                            Leaveyearfrom = Convert.ToDateTime(Fulldate);
                            LeaveyearTo = Leaveyearfrom.Value.AddDays(-1);
                            LeaveyearTo = LeaveyearTo.Value.AddYears(1);
                            RetDataparam.Leaveyearfrom = Leaveyearfrom;
                            RetDataparam.LeaveyearTo = LeaveyearTo;
                            RetDataparam.ErrNo = 0;
                        }
                        else
                        {
                            RetDataparam.ErrNo = 25;

                        }

                        //RetDataparam.Leaveyearfrom = Leaveyearfrom;
                        //RetDataparam.LeaveyearTo = LeaveyearTo;
                        //return RetDataparam;
                        break;
                    case "PROMOTIONDATE":
                        var LvNewReqDatap = db.EmployeeLeave.Include(q => q.Employee).Include(q => q.LvNewReq).Include(q => q.LvNewReq.Select(t => t.LeaveHead)).Where(q => q.Id == EmployeeLvId).FirstOrDefault();
                        if (LvNewReqDatap != null)
                        {
                            aa = LvNewReqDatap.LvNewReq.Where(q => q.LvCreditDate != null && q.LeaveHead.Id == LVheadid).OrderByDescending(q => q.Id).FirstOrDefault();
                        }

                        if (aa != null)
                        {
                            Fulldate = aa.LvCreditDate.Value.ToShortDateString();
                            Leaveyearfrom = Convert.ToDateTime(Fulldate);
                            LeaveyearTo = Leaveyearfrom.Value.AddDays(-1);
                            LeaveyearTo = LeaveyearTo.Value.AddYears(1);
                            RetDataparam.Leaveyearfrom = Leaveyearfrom;
                            RetDataparam.LeaveyearTo = LeaveyearTo;
                            RetDataparam.ErrNo = 0;
                        }
                        else
                        {
                            RetDataparam.ErrNo = 25;

                        }

                        //RetDataparam.Leaveyearfrom = Leaveyearfrom;
                        //RetDataparam.LeaveyearTo = LeaveyearTo;
                        //return RetDataparam;
                        break;
                    case "FIXDAYS":
                        var LvNewReqDatafx = db.EmployeeLeave.Include(q => q.Employee).Include(q => q.LvNewReq).Include(q => q.LvNewReq.Select(t => t.LeaveHead)).Where(q => q.Id == EmployeeLvId).FirstOrDefault();
                        if (LvNewReqDatafx != null)
                        {
                            aa = LvNewReqDatafx.LvNewReq.Where(q => q.LvCreditDate != null && q.LeaveHead.Id == LVheadid).OrderByDescending(q => q.Id).FirstOrDefault();
                        }

                        if (aa != null)
                        {
                            Fulldate = aa.LvCreditDate.Value.ToShortDateString();
                            Leaveyearfrom = Convert.ToDateTime(Fulldate);
                            LeaveyearTo = Leaveyearfrom.Value.AddDays(-1);
                            LeaveyearTo = LeaveyearTo.Value.AddYears(1);
                            RetDataparam.Leaveyearfrom = Leaveyearfrom;
                            RetDataparam.LeaveyearTo = LeaveyearTo;
                            RetDataparam.ErrNo = 0;
                        }
                        else
                        {
                            RetDataparam.ErrNo = 25;

                        }

                        break;
                    default:
                        break;

                }

                return RetDataparam;
            }
        }
        public class returnDataClass
        {
            public bool success { get; set; }
            public double data { get; set; }
        }
        public static double DateIsHolidayDay(LvNewReq OLvNewReq, int EmployeeId)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                db.Configuration.AutoDetectChangesEnabled = false;
                //  Int32 default_DateIsHolidayDay = db.Calendar.Where(e => e.Name.LookupVal.ToUpper() == "HOLIDAYCALENDAR" && e.Default == true)

                List<int> default_DateIsHolidayDay = db.Calendar.Where(e => e.Name.LookupVal.ToUpper() == "HOLIDAYCALENDAR")
                    .AsNoTracking().OrderBy(e => e.Id).Select(e => e.Id).ToList();
                var dayHolidayfrom = OLvNewReq.FromDate;
                var dayHolidayto = OLvNewReq.ToDate;

                //List<HolidayList> OCompany = db.Company
                //      .Where(e => e.Id == CompanyId).SelectMany(e => e.HolidayCalendar.Where(a => default_DateIsHolidayDay.Contains(a.HoliCalendar.Id)))
                //                .SelectMany(e => e.HolidayList.Where(r => r.HolidayDate == dayHolidayfrom ||
                //                    r.HolidayDate == dayHolidayto)).AsNoTracking().OrderBy(e => e.Id).ToList();


                int LocId = db.Employee.Include(e => e.GeoStruct).Include(e => e.GeoStruct.Location)
                                .Where(e => e.Id == EmployeeId).FirstOrDefault().GeoStruct.Location.Id;
                List<HolidayList> OHOList = db.Location.Where(e => e.Id == LocId).SelectMany(e => e.HolidayCalendar.Where(a => default_DateIsHolidayDay.Contains(a.HoliCalendar.Id)))
                               .SelectMany(e => e.HolidayList.Where(r => r.HolidayDate == dayHolidayfrom ||
                                   r.HolidayDate == dayHolidayto)).AsNoTracking().OrderBy(e => e.Id).ToList();

                if (OHOList.Count() > 0)
                {
                    //error
                    return 0;

                }
                else
                {
                    //success
                    return 1;
                }

            }
        }
        public static double DateIsWeaklyOff(LvNewReq OLvNewReq, int EmployeeId)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                db.Configuration.AutoDetectChangesEnabled = false;
                //Int32 default_weaklyofcal = db.Calendar.Where(e => e.Name.LookupVal.ToUpper() == "WEEKLYOFFCALENDAR" && e.Default == true)
                List<int> default_weaklyofcal = db.Calendar.Where(e => e.Name.LookupVal.ToUpper() == "WEEKLYOFFCALENDAR")
                    .AsNoTracking().OrderBy(e => e.Id).Select(e => e.Id).ToList();
                DayOfWeek dayofweekfrom = OLvNewReq.FromDate.Value.DayOfWeek;
                DayOfWeek dayofweekto = OLvNewReq.ToDate.Value.DayOfWeek;

                //List<WeeklyOffList> OCompany = db.Company
                //              .Where(e => e.Id == CompanyId).SelectMany(e => e.WeeklyOffCalendar.Where(a => default_weaklyofcal.Contains(a.WOCalendar.Id)))
                //              .SelectMany(e => e.WeeklyOffList.Where(r => r.WeekDays.LookupVal.ToString() == dayofweekfrom.ToString() ||
                //                  r.WeekDays.LookupVal.ToString() == dayofweekto.ToString())).AsNoTracking().OrderBy(e => e.Id).ToList();

                int LocId = db.Employee.Include(e => e.GeoStruct).Include(e => e.GeoStruct.Location).Where(e => e.Id == EmployeeId).FirstOrDefault().GeoStruct.Location.Id;
                List<WeeklyOffList> OWOList = db.Location
                              .Where(e => e.Id == LocId).SelectMany(e => e.WeeklyOffCalendar.Where(a => default_weaklyofcal.Contains(a.WOCalendar.Id)))
                              .SelectMany(e => e.WeeklyOffList.Where(r => r.WeekDays.LookupVal.ToString() == dayofweekfrom.ToString() ||
                                  r.WeekDays.LookupVal.ToString() == dayofweekto.ToString())).AsNoTracking().OrderBy(e => e.Id).ToList();

                if (OWOList.Count() > 0)
                {
                    //error
                    return 0;

                }
                else
                {
                    //success
                    return 1;
                }

            }
        }
        public class Dayleave
        {
            public double dlvheadday { get; set; }
            public string lvrecc { get; set; }
        }
        public static Dayleave DayLeaveHeadValidation(DateTime Odate, int CompanyId, int EmployeeId, Calendar OLeaveCalendar, int OLvHeadID, string lvrec)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                db.Configuration.AutoDetectChangesEnabled = false;
                EmployeeLeave oEmployeeLeave = db.EmployeeLeave.Include(e => e.LvNewReq)
                    .Include(e => e.LvNewReq.Select(a => a.LeaveCalendar))
                   .Include(e => e.LvNewReq.Select(a => a.LeaveHead))
                    .Include(e => e.LvNewReq.Select(a => a.WFStatus))
                   .Include(e => e.LvNewReq.Select(a => a.LvOrignal))
                    .Where(e => e.Id == EmployeeId && e.LvNewReq.Any(a => a.LeaveCalendar.Id == OLeaveCalendar.Id)).AsNoTracking().OrderBy(e => e.Id).SingleOrDefault();
                if (oEmployeeLeave != null)
                {
                    List<LvNewReq> LvCalendarFilter = oEmployeeLeave.LvNewReq.Where(e => e.LeaveCalendar.Id == OLeaveCalendar.Id).OrderBy(e => e.Id).ToList();

                    List<Int32> LvOrignal_id = LvCalendarFilter.Where(e => e.LvOrignal != null).OrderBy(e => e.Id).Select(e => e.LvOrignal.Id).ToList();
                    List<LvNewReq> AntCancel = LvCalendarFilter.Where(e => e.IsCancel == false && e.TrReject == false).OrderBy(e => e.Id).ToList();
                    List<LvNewReq> listLvs = AntCancel.Where(e => !LvOrignal_id.Contains(e.Id) && e.WFStatus.LookupVal != "2" && e.WFStatus.LookupVal != "3").OrderBy(e => e.Id).ToList();

                    // var listLvscnt = listLvs.Where(e => e.LeaveHead.Id == OLvHeadID && e.FromDate <= Odate && e.ToDate >= Odate).Count();
                    List<LvNewReq> listLvscnt = listLvs.Where(e => e.FromDate <= Odate && e.ToDate >= Odate).OrderBy(e => e.Id).ToList();
                    int lvheds = listLvscnt.OrderBy(e => e.Id).Select(e => e.LeaveHead.Id).SingleOrDefault();
                    LvDebitPolicy lvdeb = db.LvDebitPolicy.Include(e => e.LvHead).Include(e => e.CombinedLvHead.Select(r => r.LvHead)).Where(q => q.LvHead.Id == lvheds).AsNoTracking().OrderBy(e => e.Id).SingleOrDefault();
                    int combleave = 0;
                    if (lvdeb != null)
                    {
                        if (lvdeb.Combined == true)
                        {
                            foreach (LvHead item in lvdeb.CombinedLvHead.Select(r => r.LvHead))
                            {
                                if (item.Id == OLvHeadID)
                                {
                                    combleave = 1;
                                    lvrec = "LR";
                                    break;
                                }
                                else
                                {
                                    combleave = 1;
                                    lvrec = "NLR";
                                }

                            }
                        }
                        else if (lvdeb.Combined == false)
                        {
                            if (listLvscnt.Count() > 0)
                            {
                                combleave = 0;
                                lvrec = "NLR";
                            }
                        }


                    }
                    return new Dayleave
                    {
                        dlvheadday = 0,
                        lvrecc = lvrec
                    };
                }
                else
                {
                    return new Dayleave
                    {
                        dlvheadday = 1,
                        lvrecc = ""
                    };
                }
            }

        }

        public static double DayLeaveValidation(DateTime Odate, int CompanyId, int EmployeeId, Calendar OLeaveCalendar)
        {


            using (DataBaseContext db = new DataBaseContext())
            {
                db.Configuration.AutoDetectChangesEnabled = false;
                EmployeeLeave oEmployeeLeave = db.EmployeeLeave.Include(e => e.LvNewReq)
                    .Include(e => e.LvNewReq.Select(a => a.LeaveCalendar))
                    .Include(e => e.LvNewReq.Select(a => a.LvOrignal))
                    //.Where(e => e.Id == EmployeeId && e.LvNewReq.Any(a => a.LeaveCalendar.Id == OLeaveCalendar.Id)).AsNoTracking().OrderBy(e => e.Id).SingleOrDefault();
                    .Where(e => e.Id == EmployeeId).AsNoTracking().OrderBy(e => e.Id).SingleOrDefault();

                if (oEmployeeLeave != null)
                {
                    //List<LvNewReq> LvCalendarFilter = oEmployeeLeave.LvNewReq.Where(e => e.LeaveCalendar.Id == OLeaveCalendar.Id).ToList();
                    List<LvNewReq> LvCalendarFilter = oEmployeeLeave.LvNewReq.ToList();

                    List<int> LvOrignal_id = LvCalendarFilter.Where(e => e.LvOrignal != null).Select(e => e.LvOrignal.Id).ToList();
                    List<LvNewReq> AntCancel = LvCalendarFilter.Where(e => e.IsCancel == false && e.TrReject == false).ToList();
                    List<LvNewReq> listLvs = AntCancel.Where(e => !LvOrignal_id.Contains(e.Id)).ToList();
                    int listLvscnt = listLvs.Where(e => e.FromDate <= Odate && e.ToDate >= Odate).Count();
                    if (listLvscnt != 0)
                    {
                        //already exits

                        return 0;
                    }
                    else
                    {
                        return 1;
                    }
                }
                else
                {
                    return 1;
                }
            }

        }

        public static double DayDateIsWeaklyOff(DateTime Odate, int EmployeeId)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                db.Configuration.AutoDetectChangesEnabled = false;
                ////var OCompany = db.Company.Where(e => e.Id == CompanyId)
                ////                .Include(e => e.WeeklyOffCalendar)
                ////               .Include(e => e.WeeklyOffCalendar.Select(r => r.WOCalendar))
                ////               .Include(e => e.WeeklyOffCalendar.Select(r => r.WeeklyOffList))
                ////               .Include(e => e.WeeklyOffCalendar.Select(r => r.WeeklyOffList.Select(t => t.WeekDays)))
                ////               .Include(e => e.WeeklyOffCalendar.Select(r => r.WeeklyOffList.Select(t => t.WeeklyOffStatus)))
                ////               .Include(e => e.Location)
                ////               .Include(e => e.Location.Select(r => r.WeeklyOffCalendar.Select(t => t.WOCalendar)))
                ////               .Include(e => e.Location.Select(r => r.WeeklyOffCalendar.Select(t => t.WeeklyOffList))).AsNoTracking().OrderBy(e => e.Id)
                ////               .FirstOrDefault();
                ////List<WeeklyOffCalendar> OWeklyOffChk = OCompany.WeeklyOffCalendar.OrderBy(e => e.Id).ToList();
                ////if (OWeklyOffChk.Count() == 0)
                ////{
                ////    return 100;//holiday calendar not defined : throw alert message
                ////}
                ////List<WeeklyOffList> OnWeaklyOff = OCompany.WeeklyOffCalendar.SelectMany(r => r.WeeklyOffList.Where(t => t.WeekDays.LookupVal.ToString() == Odate.Date.DayOfWeek.ToString())).OrderBy(e => e.Id).ToList();
                ////// .Where(e => e.WeeklyOffList.Any(r => r.WeekDays.LookupVal.ToString() == Odate.Date.DayOfWeek.ToString())).OrderBy(e => e.Id).ToList();
                ////if (OnWeaklyOff.Count > 0)
                ////{
                ////    //error
                ////    return 0;

                ////}
                ////else
                ////{
                ////    //success
                ////    return 1;
                ////}


                var OEmp = db.Employee.Where(e => e.Id == EmployeeId)
                                .Include(e => e.GeoStruct)
                                .Include(e => e.GeoStruct.Location)
                                .Include(e => e.GeoStruct.Location.WeeklyOffCalendar)
                               .Include(e => e.GeoStruct.Location.WeeklyOffCalendar.Select(t => t.WOCalendar))
                               .Include(e => e.GeoStruct.Location.WeeklyOffCalendar.Select(t => t.WeeklyOffList))
                               .Include(e => e.GeoStruct.Location.WeeklyOffCalendar.Select(t => t.WeeklyOffList.Select(y => y.WeekDays))).AsNoTracking().OrderBy(e => e.Id)
                               .FirstOrDefault();
                List<WeeklyOffCalendar> OWeklyOffChk = OEmp.GeoStruct.Location.WeeklyOffCalendar.OrderBy(e => e.Id).ToList();
                if (OWeklyOffChk.Count() == 0)
                {
                    return 100;//holiday calendar not defined : throw alert message
                }
                List<WeeklyOffList> OnWeaklyOff = OEmp.GeoStruct.Location.WeeklyOffCalendar.SelectMany(r => r.WeeklyOffList.Where(t => t.WeekDays.LookupVal.ToString() == Odate.Date.DayOfWeek.ToString())).OrderBy(e => e.Id).ToList();
                // .Where(e => e.WeeklyOffList.Any(r => r.WeekDays.LookupVal.ToString() == Odate.Date.DayOfWeek.ToString())).OrderBy(e => e.Id).ToList();
                if (OnWeaklyOff.Count > 0)
                {
                    //error
                    return 0;

                }
                else
                {
                    //success
                    return 1;
                }

            }
        }
        public static returnDataClass GetHolidayCount(LvNewReq OLvNewReq, int EmployeeId)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                db.Configuration.AutoDetectChangesEnabled = false;
                Location OCompany = db.Location
                                  .Where(e => e.Id == OLvNewReq.GeoStruct.Location.Id)
                                   .Include(e => e.HolidayCalendar)
                                  .Include(e => e.HolidayCalendar.Select(r => r.HoliCalendar))
                                  .Include(e => e.HolidayCalendar.Select(r => r.HolidayList.Select(t => t.Holiday)))
                                  .Include(e => e.HolidayCalendar.Select(r => r.HolidayList.Select(t => t.Holiday.HolidayName)))
                                  .Include(e => e.HolidayCalendar.Select(r => r.HolidayList.Select(t => t.Holiday.HolidayType)))
                                  .Include(e => e.HolidayCalendar.Select(t => t.HoliCalendar))
                                  .Include(e => e.HolidayCalendar.Select(t => t.HolidayList)).AsNoTracking().FirstOrDefault();
                //Company OCompany = db.Company
                //                  .Where(e => e.Id == CompanyId && e.Location.Any(a => a.Id == OLvNewReq.GeoStruct.Location.Id))
                //                   .Include(e => e.HolidayCalendar)
                //                  .Include(e => e.HolidayCalendar.Select(r => r.HoliCalendar))
                //                  .Include(e => e.HolidayCalendar.Select(r => r.HolidayList.Select(t => t.Holiday)))
                //                  .Include(e => e.HolidayCalendar.Select(r => r.HolidayList.Select(t => t.Holiday.HolidayName)))
                //                  .Include(e => e.HolidayCalendar.Select(r => r.HolidayList.Select(t => t.Holiday.HolidayType)))
                //                  .Include(e => e.Location).Include(e => e.Location.Select(r => r.HolidayCalendar.Select(t => t.HoliCalendar)))
                //                  .Include(e => e.Location.Select(r => r.HolidayCalendar.Select(t => t.HolidayList))).AsNoTracking().OrderBy(e => e.Id).FirstOrDefault();

                List<HolidayCalendar> OHolidayCalendarChk = null;
                // holiday calendar list
                List<Calendar> holicalcheck = OCompany.HolidayCalendar.Select(q => q.HoliCalendar).ToList();
                if (holicalcheck.Count() > 0)
                {
                    OHolidayCalendarChk = OCompany.HolidayCalendar.Where(e => e.HoliCalendar.Default == true).ToList();
                }
                else
                {
                    return new returnDataClass
                    {
                        data = 100,
                        success = false
                    };
                }
                //OHolidayCalendarChk
                if (OHolidayCalendarChk.Count() == 0)
                {
                    return new returnDataClass
                    {
                        success = false,
                        data = 100,
                    };
                    //  return 100;//holiday calendar not defined : throw alert message
                }
                // company holiday list
                var OHolidayList = OCompany.HolidayCalendar.OrderBy(e => e.Id)
                                .Select(e => e.HolidayList.Where(r => r.HolidayDate.Value >= OLvNewReq.FromDate.Value &&
                               r.HolidayDate.Value <= OLvNewReq.ToDate.Value)
                                ).ToList();

                //OHolidayList = null;

                if (OHolidayList.Count <= 0)
                {
                    return new returnDataClass
                    {
                        success = false,
                        data = 101,
                    };
                    // return 101;//holiday calendar not defined for leave applied: throw alert message
                }
                //var OHolidayClendarList = OHolidayCalendarChk.Where(e => e.HoliCalendar.Default == true).OrderBy(e => e.Id).Select(t => t.HolidayList).FirstOrDefault();
                // location holiday list
                // Location OLocList = OCompany.Location.Where(e => e.Id == OLvNewReq.GeoStruct.Location.Id && e.HolidayCalendar.Count() > 0).OrderBy(e => e.Id).SingleOrDefault();
                if (OCompany == null)
                {
                    return new returnDataClass
                    {
                        success = false,
                        data = 101,
                    };
                }
                // Surendra start
                //HolidayCalendar LocWiseHolidaycalendar = OLocList.HolidayCalendar.Where(h => h.HoliCalendar.Default == true).OrderBy(e => e.Id).SingleOrDefault();
                List<HolidayCalendar> LocWiseHolidaycalendar = OCompany.HolidayCalendar.OrderBy(e => e.Id).ToList();

                // List<HolidayList> LocWiseHolidaylist = LocWiseHolidaycalendar.HolidayList.Where(a => a.HolidayDate.Value >= OLvNewReq.FromDate.Value && a.HolidayDate.Value <= OLvNewReq.ToDate.Value).OrderBy(e => e.Id).ToList();
                List<HolidayList> LocWiseHolidaylist = LocWiseHolidaycalendar.SelectMany(w => w.HolidayList.Where(a => a.HolidayDate.Value >= OLvNewReq.FromDate.Value && a.HolidayDate.Value <= OLvNewReq.ToDate.Value).OrderBy(e => e.Id)).ToList();
                // Surendra end
                //.Where(e => e.HolidayList.Any(a => a.HolidayDate.Value >= OLvNewReq.FromDate.Value && a.HolidayDate.Value <= OLvNewReq.ToDate.Value)).Select(e => e.HolidayList).ToList();
                //var OLocHolidayNameList = OCompany.Location.Where(e => e.Id == OLvNewReq.GeoStruct.Location.Id)
                //                    .Select(r => r.HolidayCalendar.Select(a => a.HolidayList.Where(t => t.HolidayDate.Value >= OLvNewReq.FromDate.Value &&
                //               t.HolidayDate.Value <= OLvNewReq.ToDate.Value))).SingleOrDefault();
                var OLocHolidayNameList_HolidayDate = new List<DateTime?>();
                // Surendra start
                foreach (var item in LocWiseHolidaylist)
                {
                    OLocHolidayNameList_HolidayDate.Add(item.HolidayDate);
                    //itemSelect(e => e.Holidaydate).ToList();
                    // Surendra End
                }
                // var OHolidayFinal = OHolidayClendarList.Select(e => e.HolidayDate).Intersect(OLocHolidayNameList_HolidayDate).ToList();
                var mTotalHoliday = OLocHolidayNameList_HolidayDate.Distinct().Count();
                //return mTotalHoliday;
                return new returnDataClass
                {
                    data = mTotalHoliday,
                    success = true
                };
            }
        }
        public static returnDataClass GetWeaklyOffConut(LvNewReq OLvNewReq, int CompanyId)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                db.Configuration.AutoDetectChangesEnabled = false;
                //Company OCompany = db.Company
                //                 .Where(e => e.Id == CompanyId && e.Location.Any(a => a.Id == OLvNewReq.GeoStruct.Location.Id))
                //                 .Include(e => e.Location)
                //                 .Include(e => e.Location.Select(r => r.WeeklyOffCalendar.Select(t => t.WOCalendar)))
                //                 .Include(e => e.Location.Select(r => r.WeeklyOffCalendar.Select(t => t.WeeklyOffList)))
                //                 .Include(e => e.WeeklyOffCalendar)
                //                 .Include(e => e.WeeklyOffCalendar.Select(r => r.WOCalendar))
                //                 .Include(e => e.WeeklyOffCalendar.Select(r => r.WeeklyOffList))
                //                 .Include(e => e.WeeklyOffCalendar.Select(r => r.WeeklyOffList.Select(t => t.WeekDays)))
                //                 .Include(e => e.WeeklyOffCalendar.Select(r => r.WeeklyOffList.Select(t => t.WeeklyOffStatus))).OrderBy(e => e.Id).SingleOrDefault();
                Location OCompany = db.Location
                                 .Where(e => e.Id == OLvNewReq.GeoStruct.Location.Id)
                                .Include(e => e.WeeklyOffCalendar.Select(t => t.WOCalendar))
                                 .Include(e => e.WeeklyOffCalendar.Select(t => t.WeeklyOffList))
                                 .Include(e => e.WeeklyOffCalendar)
                                 .Include(e => e.WeeklyOffCalendar.Select(r => r.WOCalendar))
                                 .Include(e => e.WeeklyOffCalendar.Select(r => r.WeeklyOffList))
                                 .Include(e => e.WeeklyOffCalendar.Select(r => r.WeeklyOffList.Select(t => t.WeekDays)))
                                 .Include(e => e.WeeklyOffCalendar.Select(r => r.WeeklyOffList.Select(t => t.WeeklyOffStatus))).
                                 OrderBy(e => e.Id).FirstOrDefault();




                var OWeeklyoffCalendarChk = OCompany.WeeklyOffCalendar.ToList();
                if (OWeeklyoffCalendarChk == null && OWeeklyoffCalendarChk.Count <= 0)
                {
                    return new returnDataClass
                    {
                        data = 102,
                        success = false
                    };
                    // return 102;//weekly off calendar not defined : throw alert message
                }
                //var OWeeklyOffList = OWeeklyoffCalendarChk.Where(e => e.WOCalendar.Default == true)
                //                .Where(e => OLvNewReq.FromDate.Value >= e.WOCalendar.FromDate.Value &&
                //                OLvNewReq.ToDate.Value <= e.WOCalendar.ToDate.Value
                //                ).OrderBy(e => e.Id).SingleOrDefault();
                DateTime? Leavecalendarfromdate;
                DateTime? LeavecalendarToate;
                Leavecalendarfromdate = OWeeklyoffCalendarChk.OrderBy(e => e.WOCalendar.FromDate).FirstOrDefault().WOCalendar.FromDate;
                LeavecalendarToate = OWeeklyoffCalendarChk.OrderBy(e => e.WOCalendar.FromDate).LastOrDefault().WOCalendar.ToDate;
                if (OLvNewReq.FromDate.Value >= Leavecalendarfromdate.Value &&
                               OLvNewReq.ToDate.Value <= LeavecalendarToate.Value)
                {

                }
                else
                {
                    return new returnDataClass
                    {
                        data = 103,
                        success = false
                    };
                    // return 103;//holiday calendar not defined for leave applied: throw alert message
                }

                var OweeklyOffcalendarlvcheck = OWeeklyoffCalendarChk
                    .Where(e => OLvNewReq.FromDate.Value == e.WOCalendar.FromDate.Value && OLvNewReq.ToDate.Value == e.WOCalendar.ToDate.Value).OrderBy(e => e.Id)
                    .SingleOrDefault();

                //var OHolidaycalendarlvcheck = OHolidayCalendarChk
                //    .Where(e => OLvNewReq.FromDate.Value == e.HoliCalendar.FromDate.Value && OLvNewReq.ToDate.Value == e.HoliCalendar.ToDate.Value)
                //    .SingleOrDefault();
                //if (OWeeklyOffList == null)
                //{
                //    return new returnDataClass
                //    {
                //        data = 103,
                //        success = false
                //    };
                //    // return 103;//holiday calendar not defined for leave applied: throw alert message
                //}
                if (OweeklyOffcalendarlvcheck != null)
                {
                    return new returnDataClass
                    {
                        data = 104,
                        success = false
                    };
                    //return 104; // fromdate and todate should not be weekly off or holiday
                }
                //var OLocFDWeeklyOffNameList = OCompany.Location.Where(e => e.Id == OLvNewReq.GeoStruct.Location.Id)
                //                    .Select(r => r.WeeklyOffCalendar.Select(e => e.WeeklyOffList.Select(t => t.WeekDays))).SingleOrDefault();

                var OLocHDWeeklyOffNameList = OCompany.WeeklyOffCalendar.Select(r => r.WeeklyOffList);
                //.Select(e => new
                //{
                //    WeeklyOffList = e.WeeklyOffCalendar.Select(a => a.WeeklyOffList)
                //}).SingleOrDefault();
                if (OLocHDWeeklyOffNameList == null)
                {
                    return new returnDataClass
                    {
                        data = 103,
                        success = false
                    };
                }
                //   var OFDWeeklyOffChk = OWeeklyOffList.WeeklyOffList.OrderBy(e => e.Id).ToList();
                var WeeklyOffStatus_list = new List<String>();
                foreach (var item in OLocHDWeeklyOffNameList)
                {
                    foreach (var items in item.Select(e => e.WeekDays))
                    {
                        WeeklyOffStatus_list.Add(items.LookupVal);
                    }
                }
                double mTotWeeklyOff = 0;
                var WeeklyOffStatus_listdays = new List<String>();
                WeeklyOffStatus_listdays = WeeklyOffStatus_list.Distinct().ToList();

                for (var mDate = OLvNewReq.FromDate; mDate <= OLvNewReq.ToDate; mDate = mDate.Value.AddDays(1))
                {
                    foreach (var ca in WeeklyOffStatus_listdays)
                    {
                        if (ca.ToUpper() == mDate.Value.DayOfWeek.ToString().ToUpper())
                        {
                            mTotWeeklyOff = mTotWeeklyOff + 1;
                        }
                    }

                }
                //OWeeklyOffList.WeeklyOffList.ToList()
                WeeklyOffStatus_list = new List<String>();
                foreach (var item in OLocHDWeeklyOffNameList)
                {
                    foreach (var items in item.Select(e => e.WeeklyOffStatus))
                    {
                        WeeklyOffStatus_list.Add(items.LookupVal);
                    }
                }
                var WeeklyOffStatus_listdaysH = new List<String>();
                WeeklyOffStatus_listdaysH = WeeklyOffStatus_list.Distinct().ToList();

                for (var mDate = OLvNewReq.FromDate; mDate <= OLvNewReq.ToDate; mDate = mDate.Value.AddDays(1))
                {
                    foreach (var ca in WeeklyOffStatus_listdaysH)
                    {
                        if (ca.ToUpper() == "HALFDAY")
                        {
                            mTotWeeklyOff = mTotWeeklyOff + 0.5;
                        }
                    }

                }
                return new returnDataClass
                {
                    data = mTotWeeklyOff,
                    success = true
                };
                // return mTotWeeklyOff;
            }
        }
        public static double HolidayCount(LvNewReq OLvNewReq, int EmployeeId)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                db.Configuration.AutoDetectChangesEnabled = false;
                returnDataClass HolidayCount = GetHolidayCount(OLvNewReq, EmployeeId);
                if (HolidayCount.success == false)
                {
                    return HolidayCount.data;
                }

                returnDataClass WeaklyOffCount = GetWeaklyOffConut(OLvNewReq, EmployeeId);
                if (WeaklyOffCount.success == false)
                {
                    return WeaklyOffCount.data;
                }
                return HolidayCount.data + WeaklyOffCount.data;
                //return 00;
            }
        }
        public class prefixsuffixapl
        {
            public double _TotalDays { get; set; }
            public double mTotPrefix { get; set; }
            public double mTotSuffix { get; set; }
            public bool _isPrifix { get; set; }
            public bool _isSufix { get; set; }
            public int error { get; set; }
        }
        public static prefixsuffixapl PrefixSufixCount(string mPrefixSufixType, LvNewReq OLvNewReq, int CompanyId, int EmployeeId, Calendar OLeaveCalendar)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                db.Configuration.AutoDetectChangesEnabled = false;
                //var OCompany = db.Company.Where(e => e.Id == CompanyId && e.Location.Any(a => a.Id == OLvNewReq.GeoStruct.Location.Id))
                //            .Include(e => e.HolidayCalendar)
                //            .Include(e => e.HolidayCalendar.Select(r => r.HoliCalendar))
                //            .Include(e => e.HolidayCalendar.Select(r => r.HolidayList.Select(t => t.Holiday)))
                //            .Include(e => e.HolidayCalendar.Select(r => r.HolidayList.Select(t => t.Holiday.HolidayType)))
                //            .Include(e => e.Location)
                //            .Include(e => e.Location.Select(r => r.WeeklyOffCalendar))
                //            .Include(e => e.Location.Select(r => r.HolidayCalendar))
                //            .Include(e => e.Location.Select(r => r.HolidayCalendar))
                //            .Include(e => e.WeeklyOffCalendar)
                //            .Include(e => e.WeeklyOffCalendar.Select(r => r.WOCalendar))
                //            .Include(e => e.WeeklyOffCalendar.Select(r => r.WeeklyOffList))
                //            .Include(e => e.WeeklyOffCalendar.Select(r => r.WeeklyOffList.Select(t => t.WeekDays)))
                //            .Include(e => e.WeeklyOffCalendar.Select(r => r.WeeklyOffList.Select(t => t.WeeklyOffStatus)))
                //            .AsNoTracking().OrderBy(e => e.Id)
                //           .FirstOrDefault();
                var OCompany = db.Location.Where(e => e.Id == OLvNewReq.GeoStruct.Location.Id)
                           .Include(e => e.HolidayCalendar)
                           .Include(e => e.HolidayCalendar.Select(r => r.HoliCalendar))
                           .Include(e => e.HolidayCalendar.Select(r => r.HolidayList.Select(t => t.Holiday)))
                           .Include(e => e.HolidayCalendar.Select(r => r.HolidayList.Select(t => t.Holiday.HolidayType)))
                           .Include(e => e.WeeklyOffCalendar)
                           .Include(e => e.WeeklyOffCalendar.Select(r => r.WOCalendar))
                           .Include(e => e.WeeklyOffCalendar.Select(r => r.WeeklyOffList))
                           .Include(e => e.WeeklyOffCalendar.Select(r => r.WeeklyOffList.Select(t => t.WeekDays)))
                           .Include(e => e.WeeklyOffCalendar.Select(r => r.WeeklyOffList.Select(t => t.WeeklyOffStatus)))
                           .AsNoTracking().OrderBy(e => e.Id)
                          .FirstOrDefault();


                var OHolidayCalendarChk = OCompany.HolidayCalendar.OrderBy(e => e.Id).ToList();
                if (OHolidayCalendarChk == null)
                {
                    return new prefixsuffixapl
                    {
                        error = 0
                    };//holiday calendar not defined : throw alert message
                }
                //var OHolidayList = OHolidayCalendarChk
                //.Where(e => e.HoliCalendar.Default == true).OrderBy(e => e.Id).ToList();
                var OHolidayList = OHolidayCalendarChk
                .OrderBy(e => e.Id).ToList();

                double mTotPrefix = 0;
                double mTotSuffix = 0;
                double mTotPrefixSuffix = 0;

                Boolean LvnewReqprefix = false, LvnewReqSuffix = false;
                switch (mPrefixSufixType)
                {
                    case "PRE":
                        for (var mdate = OLvNewReq.FromDate.Value.AddDays(-1).Date; mdate >= OLvNewReq.FromDate.Value.AddDays(-10).Date; mdate = mdate.AddDays(-1))
                        {
                            var aa = 0;
                            //foreach (var ca in OHolidayList)
                            //{
                            //    var OHolidayChk = ca.HolidayList
                            //.Where(e => e.HolidayDate.Value == mdate
                            //        //&& OLocHolidayNameList.HoliCalendar.Default == true
                            //).OrderBy(e => e.Id).SingleOrDefault();

                            var OHolidayChk = OHolidayList.SelectMany(e => e.HolidayList.Where(t => t.HolidayDate.Value == mdate)
                                                ).OrderBy(e => e.Id).ToList();
                            double OnWeaklyOff = DayDateIsWeaklyOff(mdate, EmployeeId);
                            double listLvscnt = DayLeaveValidation(mdate, CompanyId, EmployeeId, OLeaveCalendar);

                            if ((OHolidayChk.Count() == 0) && (OnWeaklyOff != 0))
                            {
                                aa = 1;

                                if (listLvscnt == 0 && mTotPrefix == 0)
                                {
                                    mTotPrefix = 0;
                                    LvnewReqprefix = false;
                                }
                            }
                            else
                            {
                                mTotPrefix = mTotPrefix + 1;
                                LvnewReqprefix = true;
                            }

                            //  break;

                            //  }

                            if (aa == 1)
                            {
                                break;
                            }

                        }

                        // Surendra start
                        for (var mdatep = OLvNewReq.ToDate.Value.AddDays(+1).Date; mdatep <= OLvNewReq.ToDate.Value.AddDays(+10).Date; mdatep = mdatep.AddDays(+1))
                        {
                            var aa = 0;
                            //foreach (var ca in OHolidayList)
                            //{
                            //var OHolidayChk = ca.HolidayList.Where(e => e.HolidayDate.Value == mdatep
                            //    // && OLocHolidayNameList.HoliCalendar.Default == true
                            //    ).SingleOrDefault();
                            var OHolidayChk = OHolidayList.SelectMany(e => e.HolidayList.Where(t => t.HolidayDate.Value == mdatep)
                                                      ).OrderBy(e => e.Id).ToList();
                            double OnWeaklyOff = DayDateIsWeaklyOff(mdatep, EmployeeId);
                            double listLvscnt = DayLeaveValidation(mdatep, CompanyId, EmployeeId, OLeaveCalendar);

                            if ((OHolidayChk.Count() == 0) && (OnWeaklyOff != 0))
                            {
                                aa = 1;
                                if (listLvscnt == 0)
                                {
                                    mTotSuffix = 0;
                                    LvnewReqSuffix = false;
                                }
                            }
                            else
                            {
                                mTotSuffix = mTotSuffix + 1;
                                LvnewReqSuffix = true;
                            }

                            // break;


                            // }
                            if (aa == 1)
                            {
                                break;

                            }

                        }
                        // Surendra End

                        break;
                    case "POST":
                        for (var mdate = OLvNewReq.FromDate.Value.AddDays(-1).Date; mdate >= OLvNewReq.FromDate.Value.AddDays(-10).Date; mdate = mdate.AddDays(-1))
                        {
                            var aa = 0;
                            //foreach (var ca in OHolidayList)
                            //{
                            //    var OHolidayChk = ca.HolidayList.Where(e => e.HolidayDate.Value == mdate
                            //        //&& OLocHolidayNameList.HoliCalendar.Default == true
                            //        ).SingleOrDefault();
                            var OHolidayChk = OHolidayList.SelectMany(e => e.HolidayList.Where(t => t.HolidayDate.Value == mdate)
                                                   ).OrderBy(e => e.Id).ToList();

                            double OnWeaklyOff = DayDateIsWeaklyOff(mdate, EmployeeId);
                            double listLvscnt = DayLeaveValidation(mdate, CompanyId, EmployeeId, OLeaveCalendar);

                            if ((OHolidayChk.Count() == 0) && (OnWeaklyOff != 0))
                            {
                                aa = 1;
                                if (listLvscnt == 0)
                                {
                                    mTotPrefix = 0;
                                    LvnewReqprefix = false;
                                }

                            }
                            else
                            {
                                mTotPrefix = mTotPrefix + 1;
                                LvnewReqprefix = true;
                            }

                            //    break;

                            //}

                            if (aa == 1)
                            {
                                break;
                            }

                        }
                        // Surendra start
                        for (var mdatep = OLvNewReq.ToDate.Value.AddDays(+1).Date; mdatep <= OLvNewReq.ToDate.Value.AddDays(+10).Date; mdatep = mdatep.AddDays(+1))
                        {
                            var aa = 0;
                            //foreach (var ca in OHolidayList)
                            //{
                            //    var OHolidayChk = ca.HolidayList.Where(e => e.HolidayDate.Value == mdatep
                            //        // && OLocHolidayNameList.HoliCalendar.Default == true
                            //        ).OrderBy(e => e.Id).SingleOrDefault();
                            var OHolidayChk = OHolidayList.SelectMany(e => e.HolidayList.Where(t => t.HolidayDate.Value == mdatep)
                                                   ).OrderBy(e => e.Id).ToList();

                            double OnWeaklyOff = DayDateIsWeaklyOff(mdatep, EmployeeId);
                            double listLvscnt = DayLeaveValidation(mdatep, CompanyId, EmployeeId, OLeaveCalendar);
                            if ((OHolidayChk.Count() == 0) && (OnWeaklyOff != 0))
                            {
                                aa = 1;
                                if (listLvscnt == 0 && mTotSuffix == 0)
                                {
                                    mTotSuffix = 0;
                                    LvnewReqSuffix = false;
                                }
                            }
                            else
                            {
                                mTotSuffix = mTotSuffix + 1;
                                LvnewReqSuffix = true;
                            }

                            //    break;

                            //}
                            if (aa == 1)
                            {
                                break;
                            }

                        }
                        // Surendra End

                        break;
                    case "BOTH":
                        for (var mdate = OLvNewReq.FromDate.Value.AddDays(-1).Date; mdate >= OLvNewReq.FromDate.Value.AddDays(-10).Date; mdate = mdate.AddDays(-1))
                        {
                            var aa = 0;
                            //foreach (var ca in OHolidayList)
                            //{
                            //    var OHolidayChk = ca.HolidayList.Where(e => e.HolidayDate.Value == mdate
                            //        // && OLocHolidayNameList.HoliCalendar.Default == true
                            //        ).OrderBy(e => e.Id).SingleOrDefault();
                            var OHolidayChk = OHolidayList.SelectMany(e => e.HolidayList.Where(t => t.HolidayDate.Value == mdate)
                                                   ).OrderBy(e => e.Id).ToList();
                            double OnWeaklyOff = DayDateIsWeaklyOff(mdate, EmployeeId);
                            double listLvscnt = DayLeaveValidation(mdate, CompanyId, EmployeeId, OLeaveCalendar);
                            if ((OHolidayChk.Count() == 0) && (OnWeaklyOff != 0))
                            {
                                aa = 1;
                                if (listLvscnt == 0)
                                {
                                    mTotPrefix = 0;
                                    LvnewReqprefix = false;
                                }


                            }
                            else
                            {
                                mTotPrefix = mTotPrefix + 1;
                                LvnewReqprefix = true;
                            }

                            //    break;

                            //}

                            if (aa == 1)
                            {
                                break;
                            }

                        }

                        // Surendra start
                        for (var mdatep = OLvNewReq.ToDate.Value.AddDays(+1).Date; mdatep <= OLvNewReq.ToDate.Value.AddDays(+10).Date; mdatep = mdatep.AddDays(+1))
                        {
                            var aa = 0;
                            //foreach (var ca in OHolidayList)
                            //{
                            //var OHolidayChk = ca.HolidayList.Where(e => e.HolidayDate.Value == mdatep
                            //    // && OLocHolidayNameList.HoliCalendar.Default == true
                            //    ).OrderBy(e => e.Id).SingleOrDefault();
                            var OHolidayChk = OHolidayList.SelectMany(e => e.HolidayList.Where(t => t.HolidayDate.Value == mdatep)
                                               ).OrderBy(e => e.Id).ToList();
                            double OnWeaklyOff = DayDateIsWeaklyOff(mdatep, EmployeeId);
                            double listLvscnt = DayLeaveValidation(mdatep, CompanyId, EmployeeId, OLeaveCalendar);
                            if ((OHolidayChk.Count() == 0) && (OnWeaklyOff != 0))
                            {
                                aa = 1;
                                if (listLvscnt == 0)
                                {
                                    mTotSuffix = 0;
                                    LvnewReqSuffix = false;
                                }
                            }
                            else
                            {
                                mTotSuffix = mTotSuffix + 1;
                                LvnewReqSuffix = true;
                            }

                            //    break;

                            //}
                            if (aa == 1)
                            {
                                break;
                            }

                        }

                        // Surendra End


                        break;

                }

                mTotPrefixSuffix = mTotSuffix + mTotPrefix;

                var ComId = Convert.ToInt32(SessionManager.CompanyId);
                var ComCode = db.Company.Where(e => e.Id == ComId).AsNoTracking().OrderBy(e => e.Id).SingleOrDefault().Code;
                if (ComCode.ToUpper() == "HCBL" && OLvNewReq.LeaveHead.LvCode == "CL")//hindustan bank
                {
                    if (OLvNewReq.FromStat.LookupVal.ToUpper() != "FULLSESSION")
                    {
                        if (mTotPrefix != 0)
                        {
                            mTotPrefixSuffix = mTotPrefixSuffix - mTotPrefix;
                        }
                    }
                    if (OLvNewReq.ToStat.LookupVal.ToUpper() == "FIRSTSESSION")
                    {
                        if (mTotSuffix != 0)
                        {
                            mTotPrefixSuffix = mTotPrefixSuffix - mTotSuffix;
                        }
                    }

                    if (mTotPrefixSuffix != 0)
                    {
                        mTotPrefixSuffix = mTotPrefixSuffix;
                    }
                }
                else
                {

                    if (mTotPrefixSuffix != 0)
                    {
                        mTotPrefixSuffix = mTotPrefixSuffix - 1;
                    }
                }


                return new prefixsuffixapl
                {

                    _TotalDays = mTotPrefixSuffix,
                    _isPrifix = LvnewReqprefix,
                    _isSufix = LvnewReqSuffix,
                    mTotPrefix = mTotPrefix,
                    mTotSuffix = mTotSuffix
                };


            }
        }

        public static prefixsuffixapl PrefixSufixCountLeavehead(string mPrefixSufixType, LvNewReq OLvNewReq, int CompanyId, int EmployeeId, Calendar OLeaveCalendar)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                db.Configuration.AutoDetectChangesEnabled = false;
                //var OCompany = db.Company.Where(e => e.Id == CompanyId && e.Location.Any(a => a.Id == OLvNewReq.GeoStruct.Location.Id))
                //            .Include(e => e.HolidayCalendar)
                //            .Include(e => e.HolidayCalendar.Select(r => r.HoliCalendar))
                //            .Include(e => e.HolidayCalendar.Select(r => r.HolidayList.Select(t => t.Holiday)))
                //            .Include(e => e.HolidayCalendar.Select(r => r.HolidayList.Select(t => t.Holiday.HolidayType)))
                //            .Include(e => e.Location)
                //            .Include(e => e.Location.Select(r => r.WeeklyOffCalendar))
                //            .Include(e => e.Location.Select(r => r.HolidayCalendar))
                //            .Include(e => e.Location.Select(r => r.HolidayCalendar))
                //            .Include(e => e.WeeklyOffCalendar)
                //            .Include(e => e.WeeklyOffCalendar.Select(r => r.WOCalendar))
                //            .Include(e => e.WeeklyOffCalendar.Select(r => r.WeeklyOffList))
                //            .Include(e => e.WeeklyOffCalendar.Select(r => r.WeeklyOffList.Select(t => t.WeekDays)))
                //            .Include(e => e.WeeklyOffCalendar.Select(r => r.WeeklyOffList.Select(t => t.WeeklyOffStatus)))
                //            .AsNoTracking().OrderBy(e => e.Id).FirstOrDefault();
                var OCompany = db.Location.Where(e => e.Id == OLvNewReq.GeoStruct.Location.Id)
                           .Include(e => e.HolidayCalendar)
                           .Include(e => e.HolidayCalendar.Select(r => r.HoliCalendar))
                           .Include(e => e.HolidayCalendar.Select(r => r.HolidayList.Select(t => t.Holiday)))
                           .Include(e => e.HolidayCalendar.Select(r => r.HolidayList.Select(t => t.Holiday.HolidayType)))
                           .Include(e => e.WeeklyOffCalendar)
                           .Include(e => e.WeeklyOffCalendar.Select(r => r.WOCalendar))
                           .Include(e => e.WeeklyOffCalendar.Select(r => r.WeeklyOffList))
                           .Include(e => e.WeeklyOffCalendar.Select(r => r.WeeklyOffList.Select(t => t.WeekDays)))
                           .Include(e => e.WeeklyOffCalendar.Select(r => r.WeeklyOffList.Select(t => t.WeeklyOffStatus)))
                           .AsNoTracking().OrderBy(e => e.Id).FirstOrDefault();


                var OHolidayCalendarChk = OCompany.HolidayCalendar.OrderBy(e => e.Id).ToList();
                if (OHolidayCalendarChk == null)
                {
                    return new prefixsuffixapl
                    {
                        error = 0
                    };//holiday calendar not defined : throw alert message
                }
                var OHolidayList = OHolidayCalendarChk
                .OrderBy(e => e.Id).ToList();






                double mTotPrefix = 0;
                double mTotSuffix = 0;
                double mPrefix = 0;
                double mSuffix = 0;
                string lvrec = "";
                Boolean LvnewReqprefix = false, LvnewReqSuffix = false;
                switch (mPrefixSufixType)
                {

                    case "BOTH":
                        for (var mdate = OLvNewReq.FromDate.Value.AddDays(-1).Date; mdate >= OLvNewReq.FromDate.Value.AddDays(-10).Date; mdate = mdate.AddDays(-1))
                        {
                            //var aa = 0;
                            //foreach (var ca in OHolidayList)
                            //{
                            //    var OHolidayChk = ca.HolidayList.Where(e => e.HolidayDate.Value == mdate
                            //        // && OLocHolidayNameList.HoliCalendar.Default == true
                            //        ).OrderBy(e => e.Id).FirstOrDefault();
                            var OHolidayChk = OHolidayList.SelectMany(e => e.HolidayList.Where(t => t.HolidayDate.Value == mdate)
                                                   ).OrderBy(e => e.Id).ToList();

                            double OnWeaklyOff = DayDateIsWeaklyOff(mdate, EmployeeId);
                            //var listLvscnt = DayLeaveHeadValidation(mdate, CompanyId, EmployeeId, OLeaveCalendar, OLvNewReq.LeaveHead.Id, lvrec);
                            if ((OHolidayChk.Count() == 0) && (OnWeaklyOff != 0))
                            {
                                //aa = 1;
                                break;
                                //if (listLvscnt.dlvheadday == 0)
                                //{
                                //    if (listLvscnt.lvrecc == "LR")
                                //    {
                                //        mPrefix = 0;
                                //    }
                                //    else
                                //    {
                                //        mPrefix = mPrefix + 1;
                                //    }

                                //    LvnewReqprefix = false;
                                //}
                            }
                            else
                            {
                                if (OnWeaklyOff == 0 || OHolidayChk.Count() > 0)
                                {
                                    mPrefix = mPrefix + 1;
                                    LvnewReqprefix = true;
                                }
                            }


                            //    break;

                            //}

                            //if (aa == 1)
                            //{
                            //    break;
                            //}

                        }
                        if (mPrefix > 0)
                        {
                            mTotPrefix = mPrefix;
                        }

                        // Surendra start
                        for (var mdatep = OLvNewReq.ToDate.Value.AddDays(+1).Date; mdatep <= OLvNewReq.ToDate.Value.AddDays(+10).Date; mdatep = mdatep.AddDays(+1))
                        {
                            //var aa = 0;
                            //foreach (var ca in OHolidayList)
                            //{
                            //    var OHolidayChk = ca.HolidayList.Where(e => e.HolidayDate.Value == mdatep
                            //        // && OLocHolidayNameList.HoliCalendar.Default == true
                            //        ).OrderBy(e => e.Id).FirstOrDefault();
                            var OHolidayChk = OHolidayList.SelectMany(e => e.HolidayList.Where(t => t.HolidayDate.Value == mdatep)
                                                  ).OrderBy(e => e.Id).ToList();
                            double OnWeaklyOff = DayDateIsWeaklyOff(mdatep, EmployeeId);
                            //var listLvscnt = DayLeaveHeadValidation(mdatep, CompanyId, EmployeeId, OLeaveCalendar, OLvNewReq.LeaveHead.Id, lvrec);
                            if ((OHolidayChk.Count() == 0) && (OnWeaklyOff != 0))
                            {
                                break;
                                //aa = 1;
                                //if (listLvscnt.dlvheadday == 0)
                                //{
                                //    if (listLvscnt.lvrecc == "LR")
                                //    {
                                //        mSuffix = 0;
                                //    }
                                //    else
                                //    {
                                //        mSuffix = mSuffix + 1;
                                //    }
                                //    LvnewReqSuffix = false;
                                //}
                            }
                            else
                            {
                                if (OnWeaklyOff == 0 || OHolidayChk.Count() > 0)
                                {
                                    mSuffix = mSuffix + 1;
                                    LvnewReqprefix = true;
                                }
                            }
                            //    break;

                            //}
                            //if (aa == 1)
                            //{
                            //    break;
                            //}

                        }
                        if (mSuffix > 0)
                        {
                            mTotSuffix = mSuffix;
                        }
                        // Surendra End


                        break;

                }

                //mTotPrefix = mTotSuffix + mTotPrefix;


                return new prefixsuffixapl
                {

                    _TotalDays = mTotPrefix,
                    _isPrifix = LvnewReqprefix,
                    _isSufix = LvnewReqSuffix,
                    mTotPrefix = mTotPrefix,
                    mTotSuffix = mTotSuffix
                };


            }

        }
        public static LvHeadFormula LvFormulaFinder(EmployeeLvStruct OEmpLvstruct, int OLvHeadID)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                db.Configuration.AutoDetectChangesEnabled = false;
                string mFormulaName = "";
                var OEmployeeLvStruct = db.EmployeeLvStruct.Where(t => t.Id == OEmpLvstruct.Id)
                                       .Include(e => e.GeoStruct.Corporate)
                                        .Include(e => e.GeoStruct.Region)
                                        .Include(e => e.GeoStruct.Company)
                                        .Include(e => e.GeoStruct.Division)
                                        .Include(e => e.GeoStruct.Location)
                                        .Include(e => e.GeoStruct.Department)
                                        .Include(e => e.GeoStruct.Group)
                                        .Include(e => e.GeoStruct.Unit)

                                        .Include(e => e.PayStruct.Company)
                                        .Include(e => e.PayStruct.Grade)
                                        .Include(e => e.PayStruct.Level)
                                        .Include(e => e.PayStruct.JobStatus)

                                        .Include(e => e.FuncStruct.Company)
                                        .Include(e => e.FuncStruct.Job)
                                        .Include(e => e.FuncStruct.JobPosition)
                                          .Include(e => e.EmployeeLvStructDetails)
                                          .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHead))
                                          .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvAssignment))
                                          .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvAssignment.LvHeadFormula))

                                           .Include(e => e.EmployeeLvStructDetails.Select(d => d.LvAssignment.LvHeadFormula.Select(s => s.GeoStruct.Corporate)))
                                            .Include(e => e.EmployeeLvStructDetails.Select(d => d.LvAssignment.LvHeadFormula.Select(s => s.GeoStruct.Region)))
                                            .Include(e => e.EmployeeLvStructDetails.Select(d => d.LvAssignment.LvHeadFormula.Select(s => s.GeoStruct.Company)))
                                            .Include(e => e.EmployeeLvStructDetails.Select(d => d.LvAssignment.LvHeadFormula.Select(s => s.GeoStruct.Division)))

                                            .Include(e => e.EmployeeLvStructDetails.Select(d => d.LvAssignment.LvHeadFormula.Select(s => s.GeoStruct.Location)))
                                            .Include(e => e.EmployeeLvStructDetails.Select(d => d.LvAssignment.LvHeadFormula.Select(s => s.GeoStruct.Department)))


                                            .Include(e => e.EmployeeLvStructDetails.Select(d => d.LvAssignment.LvHeadFormula.Select(s => s.GeoStruct.Group)))
                                            .Include(e => e.EmployeeLvStructDetails.Select(d => d.LvAssignment.LvHeadFormula.Select(s => s.GeoStruct.Unit)))

                                            .Include(e => e.EmployeeLvStructDetails.Select(d => d.LvAssignment.LvHeadFormula.Select(s => s.FuncStruct.Job)))
                                            .Include(e => e.EmployeeLvStructDetails.Select(d => d.LvAssignment.LvHeadFormula.Select(s => s.FuncStruct.JobPosition)))


                                            .Include(e => e.EmployeeLvStructDetails.Select(d => d.LvAssignment.LvHeadFormula.Select(s => s.PayStruct.Grade)))
                                            .Include(e => e.EmployeeLvStructDetails.Select(d => d.LvAssignment.LvHeadFormula.Select(s => s.PayStruct.Level)))
                                            .Include(e => e.EmployeeLvStructDetails.Select(d => d.LvAssignment.LvHeadFormula.Select(s => s.PayStruct.JobStatus)))
                                            .SingleOrDefault();




                //var OLvHeadFormulaGeo = OEmployeeSalStruct.EmpSalStructDetails
                //    .Where(e => e.SalaryHead.Id == OSalaryHeadID)
                //    .Select(e => e.PayScaleAssignment.SalHeadFormula
                //        .Where(r => (r.GeoStruct != null ) || r.GeoStruct.Id == OEmployeeSalStruct.GeoStruct.Id)).SingleOrDefault();

                var OLvHeadFormulaGeo = OEmployeeLvStruct.EmployeeLvStructDetails
                    .Where(e => e.LvHead.Id == OLvHeadID)
                    .Select(e => e.LvAssignment.LvHeadFormula
                        .Where(r => r.GeoStruct != null)).SingleOrDefault();

                OLvHeadFormulaGeo = OLvHeadFormulaGeo.Where(e => e.GeoStruct.Id == OEmployeeLvStruct.GeoStruct.Id);

                var OLvHeadFormulaPay = OLvHeadFormulaGeo;
                OLvHeadFormulaPay = null;

                var OLvHeadFormula = OLvHeadFormulaGeo;
                if (OLvHeadFormulaGeo.SingleOrDefault() != null && OLvHeadFormulaGeo.Count() > 0)
                {
                    mFormulaName = OLvHeadFormulaGeo.Select(e => e.Name).FirstOrDefault();
                    OLvHeadFormulaPay = OEmployeeLvStruct.EmployeeLvStructDetails
                                  .Where(e => e.LvHead.Id == OLvHeadID)
                                  .Select(e => e.LvAssignment.LvHeadFormula
                                         .Where(r => r.PayStruct != null && r.Name == mFormulaName)).SingleOrDefault();
                    if (OLvHeadFormulaPay.Count() > 0)
                    {
                        //OLvHeadFormulaPay = OLvHeadFormulaPay.Where(r =>
                        //    (r.PayStruct != null && OEmployeeSalStruct.PayStruct != null) || r.PayStruct.Id == OEmployeeSalStruct.PayStruct.Id).ToList();

                        OLvHeadFormulaPay = OLvHeadFormulaPay.Where(e => e.PayStruct.Id == OEmployeeLvStruct.PayStruct.Id);

                        //OLvHeadFormula = OLvHeadFormulaPay;
                    }

                }
                else
                {
                    OLvHeadFormulaPay = OEmployeeLvStruct.EmployeeLvStructDetails
                                  .Where(e => e.LvHead.Id == OLvHeadID)
                                  .Select(e => e.LvAssignment.LvHeadFormula
                                         .Where(r => (r.PayStruct != null))).SingleOrDefault();

                    OLvHeadFormulaPay = OLvHeadFormulaPay.Where(e => e.PayStruct.Id == OEmployeeLvStruct.PayStruct.Id);

                }

                var OLvHeadFormulaFunc = OLvHeadFormulaGeo;
                OLvHeadFormulaFunc = null;

                if (OLvHeadFormula.SingleOrDefault() == null)
                {
                    OLvHeadFormulaFunc = OEmployeeLvStruct.EmployeeLvStructDetails
                                .Where(e => e.LvHead.Id == OLvHeadID)
                                   .Select(e => e.LvAssignment.LvHeadFormula
                  .Where(r => (r.FuncStruct != null && OEmployeeLvStruct.FuncStruct != null))
                    ).SingleOrDefault();

                    OLvHeadFormulaFunc = OLvHeadFormulaFunc.Where(e => e.FuncStruct.Id == OEmployeeLvStruct.FuncStruct.Id);
                }
                else
                {
                    mFormulaName = OLvHeadFormula.Select(e => e.Name).FirstOrDefault();
                    OLvHeadFormulaFunc = OEmployeeLvStruct.EmployeeLvStructDetails
                                 .Where(e => e.LvHead.Id == OLvHeadID)
                                 .Select(e => e.LvAssignment.LvHeadFormula
                                        .Where(r => r.FuncStruct != null && r.Name == mFormulaName)).SingleOrDefault();

                    if (OLvHeadFormulaFunc.SingleOrDefault() != null || OLvHeadFormulaFunc.Count() > 0)
                    {
                        //OLvHeadFormulaFunc = OLvHeadFormulaPay.Where(r =>
                        //    (r.FuncStruct != null));

                        OLvHeadFormulaFunc = OLvHeadFormulaFunc.Where(r => r.FuncStruct.Id == OEmployeeLvStruct.FuncStruct.Id);

                        //OLvHeadFormula = OLvHeadFormulaFunc;
                    }
                }

                if (OLvHeadFormulaPay.SingleOrDefault() != null && OLvHeadFormulaFunc.SingleOrDefault() != null)
                    if (OLvHeadFormulaPay.Count() > 0 && OLvHeadFormulaFunc.Count() > 0)
                        OLvHeadFormula = OLvHeadFormulaFunc;

                if (OLvHeadFormulaPay.SingleOrDefault() != null && OLvHeadFormulaFunc.SingleOrDefault() != null)
                    if (OLvHeadFormulaPay.Count() > 0 && OLvHeadFormulaFunc.Count() == 0)
                        OLvHeadFormula = OLvHeadFormulaPay;

                if (OLvHeadFormulaPay.SingleOrDefault() != null && OLvHeadFormulaFunc.SingleOrDefault() != null)
                    if (OLvHeadFormulaPay.Count() == 0 && OLvHeadFormulaFunc.Count() > 0)
                        OLvHeadFormula = OLvHeadFormulaFunc;


                //foreach (var ca in OLvHeadFormula)
                //{
                //    return ca;
                //}
                return OLvHeadFormula.SingleOrDefault();
            }//return salheadformula
        }


        public static ReturnData MSCLvValidate(LvNewReq OLvNewReq, int CompanyId, int EmployeeId, Calendar OLeaveCalendar, DateTime? Leaveyearfrom, DateTime? LeaveyearTo, double pld)
        {
            DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };
            ReturnData RetData = new ReturnData();

            using (DataBaseContext db = new DataBaseContext())
            {
                EmployeeLeave oEmployeeLeave = db.EmployeeLeave.Include(e => e.LvNewReq)
                   .Include(e => e.LvNewReq.Select(a => a.LeaveCalendar))
                   .Include(e => e.LvNewReq.Select(a => a.WFStatus))
               .Where(e => e.Id == EmployeeId).OrderBy(e => e.Id).SingleOrDefault();
                if (oEmployeeLeave != null)
                {
                    if (OLvNewReq.LeaveHead.LvCode == "SLHP")
                    {
                        var OEmployeeLeave = _returnCompanyLeave(CompanyId, EmployeeId);

                        var OEmpLvHistory = new LvNewReq();
                        var OEmpLvOpenBalAll = new List<LvOpenBal>();
                        if (OEmployeeLeave != null)
                        {
                            // service year check start
                            var OLvSalStruct = db.EmployeeLvStruct
                                        .Include(e => e.EmployeeLvStructDetails)
                                        .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHead))
                                        .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula))
                                        .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvDebitPolicy))
                                        .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHead.LvHeadOprationType))
                                            .Where(e => e.EndDate == null && e.EmployeeLeave_Id == OEmployeeLeave.Id).SingleOrDefault();//.ToList();

                            if (OLvSalStruct == null)
                            {
                                RetData.DebitDays = 0;
                                RetData.ErrNo = 27;
                                return RetData; //No leave structure available
                            }
                            LvDebitPolicy OLvDebit = OLvSalStruct.EmployeeLvStructDetails.Where(e => e.LvHead.Id == OLvNewReq.LeaveHead.Id && e.LvHeadFormula != null && e.LvHeadFormula.LvDebitPolicy != null).Select(r => r.LvHeadFormula.LvDebitPolicy).FirstOrDefault();
                            if (OLvDebit == null)
                            {
                                RetData.DebitDays = 0;
                                RetData.ErrNo = 28;
                                return RetData; //No leave debit policy defined
                            }
                            if (OLvDebit != null)
                            {
                                if (OLvDebit.HalfDayAppl == true)
                                {
                                    DateTime RetirementDt = (DateTime)db.EmployeeLeave.Include(x => x.Employee.ServiceBookDates).Where(a => a.Id == EmployeeId).SingleOrDefault().Employee.ServiceBookDates.RetirementDate;
                                    DateTime start = DateTime.Today;
                                    DateTime end = RetirementDt;
                                    int compMonth = (end.Month + end.Year * 12) - (start.Month + start.Year * 12);
                                    double daysInEndMonth = (end.AddMonths(1) - end).Days;
                                    double months = compMonth + Math.Abs((start.Day - end.Day) / daysInEndMonth);
                                    double mAge = Math.Abs(months / 12);
                                    if (mAge > 3)
                                    {

                                        if (OLvNewReq.FromStat.LookupVal.ToUpper() == "FULLSESSION" || OLvNewReq.ToStat.LookupVal.ToUpper() == "FULLSESSION")
                                        {
                                            RetData.DebitDays = 0;
                                            RetData.ErrNo = 32;
                                            return RetData;//Half day leave not allowed
                                        }
                                    }

                                }
                            }

                            // service year check end
                            OEmpLvHistory = OEmployeeLeave.LvNewReq
                                             .Where(e => e.LeaveHead.LvCode == "SL"
                                              )
                                             .OrderByDescending(e => e.Id).FirstOrDefault();


                            if (OEmpLvHistory.CloseBal > 0)
                            {
                                RetData.DebitDays = 0;
                                RetData.ErrNo = 31;
                                return RetData; //You can not apply for SLHP as SL balance is not 0.
                            }
                            else
                            {
                                RetData.DebitDays = 0;
                                RetData.ErrNo = 0;
                                return RetData;
                            }


                        }


                    }
                    else if (OLvNewReq.LeaveHead.LvCode == "PL")//IF CL Balance is zero then if  Employee Requested for PL , It exclude Holiday and Weekly off but it is allowed for only 5 working days
                    {
                        var OEmployeeLeave = _returnCompanyLeave(CompanyId, EmployeeId);
                        var OEmpLvHistory = new LvNewReq();
                        OEmpLvHistory = OEmployeeLeave.LvNewReq
                                             .Where(e => e.LeaveHead.LvCode == "CL"
                                              )
                                             .OrderByDescending(e => e.Id).FirstOrDefault();
                        var OLvSalStruct = db.EmployeeLvStruct
                                       .Include(e => e.EmployeeLvStructDetails)
                                       .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHead))
                                       .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula))
                                       .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvDebitPolicy))
                                       .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHead.LvHeadOprationType))
                                           .Where(e => e.EndDate == null && e.EmployeeLeave_Id == OEmployeeLeave.Id).SingleOrDefault();//.ToList();

                        if (OLvSalStruct == null)
                        {
                            RetData.DebitDays = 0;
                            RetData.ErrNo = 27;
                            return RetData; //No leave structure available
                        }
                        LvDebitPolicy OLvDebit = OLvSalStruct.EmployeeLvStructDetails.Where(e => e.LvHead.Id == OLvNewReq.LeaveHead.Id && e.LvHeadFormula != null && e.LvHeadFormula.LvDebitPolicy != null).Select(r => r.LvHeadFormula.LvDebitPolicy).FirstOrDefault();
                        if (OLvDebit == null)
                        {
                            RetData.DebitDays = 0;
                            RetData.ErrNo = 28;
                            return RetData; //No leave debit policy defined
                        }

                        if (OEmpLvHistory != null)
                        {
                            if (OEmpLvHistory.CloseBal == 0)
                            {
                                double actualpd = pld;
                                if (OLvDebit.HolidayInclusive == false) //IF CL Balance is zero then if  Employee Requested for PL , It exclude Holiday and Weekly off 
                                {
                                    pld = pld + HolidayCount(OLvNewReq, EmployeeId); //debit holidays
                                }
                                if (actualpd <= 5)
                                {
                                    RetData.DebitDays = actualpd;
                                }
                                else
                                {
                                    RetData.DebitDays = pld;
                                }

                                RetData.ErrNo = 0;
                                return RetData; //if CL balance 0 then applied PL holiday exclude.
                            }
                            else
                            {
                                RetData.DebitDays = pld;
                                RetData.ErrNo = 0;
                                return RetData;
                            }
                        }
                        else
                        {
                            RetData.DebitDays = pld;
                            RetData.ErrNo = 0;
                            return RetData;
                        }
                    }
                    else
                    {
                        RetData.DebitDays = 0;
                        RetData.ErrNo = 0;
                        return RetData;
                    }
                }
            }
            return null;
        }

    }
}