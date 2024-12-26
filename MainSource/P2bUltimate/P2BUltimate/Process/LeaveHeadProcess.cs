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
        public class prefixsuffixapl
        {
            public double _TotalDays { get; set; }

            public double mTotPrefix { get; set; }

            public double mTotSuffix { get; set; }

            public bool _isPrifix { get; set; }

            public bool _isSufix { get; set; }

            public int error { get; set; }

            public int CombineLvSufix_Id { get; set; }

            public int CombineLvPrefix_Id { get; set; }
        }
        public class ReturnData_LeaveValidation
        {
            public double DebitDays { get; set; }
            public int ErrNo { get; set; }
            public int InfoNo { get; set; }
            public double LvnewReqprefix { get; set; }
            public double LvnewReqSuffix { get; set; }
            public bool PrefixSufix { get; set; }
            public bool IsLvDebitSharing { get; set; }
            public bool IsLvHolidayWeeklyoffExclude { get; set; }
            public double LvCountPrefixSuffix { get; set; }
            public double LvFinalPrefixSuffixCount { get; set; }

        }

        public class ReturnDatacalendarpara
        {
            public int ErrNo { get; set; }
            public DateTime? Leaveyearfrom { get; set; }
            public DateTime? LeaveyearTo { get; set; }
        }

        public static ReturnData_LeaveValidation LeaveValidation(LvNewReq OLvNewReq, int EmployeeLeave_Id, DateTime? Leaveyearfrom, DateTime? LeaveyearTo)
        {

            ReturnData_LeaveValidation RetData = new ReturnData_LeaveValidation();
            //return paramters initialise
            double mLvnewReqprefix = 0;
            double mLvnewReqSuffix = 0;
            bool IsDebitSharing = false;
            bool IsLvHolidayWeeklyoffExclude = false;

            using (DataBaseContext db = new DataBaseContext())
            {
                db.Configuration.AutoDetectChangesEnabled = false;
                if (OLvNewReq.LeaveHead_Id > 0)
                {
                    RetData.DebitDays = 0;
                    RetData.ErrNo = 1;
                    return RetData;
                }
                OLvNewReq.LeaveHead_Id = OLvNewReq.LeaveHead.Id;

                //LvDebitPolict and LVCreditPolicy
                //EmpLvStruct employee leave structure
                #region Leave policy of Leave selected head
                //get leave debit and credit policy of leave 
                var EmpLvStruct = db.EmployeeLvStruct.Where(e => e.EndDate == null && e.EmployeeLeave_Id == EmployeeLeave_Id).SingleOrDefault();//.ToList();
                if (EmpLvStruct == null)
                {
                    RetData.DebitDays = 0;
                    RetData.ErrNo = 2;
                    return RetData;
                }
                List<int> EmployeeLvStructDetails_FormulaId = db.EmployeeLvStructDetails.Where(e => e.EmployeeLvStruct_Id == EmpLvStruct.Id && e.LvHead_Id == OLvNewReq.LeaveHead_Id).Select(e => e.LvHeadFormula_Id.Value).ToList();


                LvHeadFormula LvHeadFormula_Debit = db.LvHeadFormula.Where(e => EmployeeLvStructDetails_FormulaId.Contains(e.Id) && e.LvDebitPolicy_Id > 0).SingleOrDefault();
                if (LvHeadFormula_Debit == null)
                {
                    RetData.DebitDays = 0;
                    RetData.ErrNo = 3;
                    return RetData;
                }
                LvHeadFormula LvHeadFormula_Credit = db.LvHeadFormula.Where(e => EmployeeLvStructDetails_FormulaId.Contains(e.Id) && e.LvCreditPolicy_Id > 0).SingleOrDefault();
                if (LvHeadFormula_Credit == null)
                {
                    RetData.DebitDays = 0;
                    RetData.ErrNo = 4;
                    return RetData;
                }

                //EmployeeLvStructDetails.LvHeadFormula = LvHeadFormula;
                LvDebitPolicy LvDebitPolicy = db.LvDebitPolicy.Include(r => r.PrefixSuffix_PrefixSuffixAction)
                    .Include(r => r.PostApplyPrefixSuffix_PrefixSuffixAction).Include(r => r.PreApplyPrefixSuffix_PrefixSuffixAction).Where(e => e.Id == LvHeadFormula_Debit.LvDebitPolicy_Id).SingleOrDefault();
                LvHeadFormula_Debit.LvDebitPolicy = LvDebitPolicy;
                LvHead LvHead = db.LvHead.Where(e => e.Id == LvDebitPolicy.LvHead_Id).FirstOrDefault();
                LvDebitPolicy.LvHead = LvHead;
                List<CombinedLvHead> CombinedLvHead = db.LvDebitPolicy.Where(e => e.Id == LvHeadFormula_Debit.LvDebitPolicy_Id).Select(e => e.CombinedLvHead.ToList()).SingleOrDefault();
                LvDebitPolicy.CombinedLvHead = CombinedLvHead;

                //LookupValue LvHeadOprationType = db.LookupValue.Where(e => e.Id == LvHead.LvHeadOprationType_Id).SingleOrDefault();
                //LvHead.LvHeadOprationType = LvHeadOprationType;
                LvCreditPolicy LvCreditPolicy = db.LvCreditPolicy.Where(e => e.Id == LvHeadFormula_Credit.LvCreditPolicy_Id).SingleOrDefault();
                LvHeadFormula_Credit.LvCreditPolicy = LvCreditPolicy;

                #endregion Leave policy of Leave selected head
                var LeaveHistory_Combined = new List<LvNewReq>();
                var LeaveHistory_Last_Transaction = new LvNewReq();//used for lv balance
                var LeaveHistory_Current = new List<LvNewReq>();
                var LeaveHistory_Validation = new List<LvNewReq>();

                #region Current Leave Year Leave requistions history with last year last leave transaction
                //last year last leave transaction : Used for combined leave check

                var LastLeaveCredit1 = db.LvNewReq.Where(e => e.EmployeeLeave_Id == EmployeeLeave_Id && e.LeaveHead_Id== OLvNewReq.LeaveHead_Id && e.LvCreditDate != null).ToList();

                var LastLeaveCredit = LastLeaveCredit1.OrderBy(e => e.Id).LastOrDefault(); //last leave credit record, start of leave year
                if (LastLeaveCredit != null)
                {
                    //check for only last leave requisitions of last year from last 15days check, exclude cancel leave request, include leave credit record
                    //Leave Cancel requisitions exclude
                    DateTime NewDt = LastLeaveCredit.LvCreditDate.Value.AddDays(-15);
                    var LvNewReqListOldCancel = db.LvNewReq.Where(e => e.EmployeeLeave_Id == EmployeeLeave_Id && e.Id < LastLeaveCredit.Id && e.ToDate > NewDt && e.LvOrignal_Id > 0).OrderByDescending(r => r.ToDate).ToList();
                    int LvNewReqListOldCancel_Id = LvNewReqListOldCancel != null ? LvNewReqListOldCancel.Select(e => e.Id).FirstOrDefault() : 0;
                    
                    //last leave for validation
                    NewDt = LastLeaveCredit.LvCreditDate.Value.AddDays(-10);//calculate last credit date and go back for 10 days to add leaves to get last year leave for checking
                    LvNewReq LvNewReqListOld = db.LvNewReq.Include(r => r.WFStatus).Where(e => e.EmployeeLeave_Id == EmployeeLeave_Id && e.Id < LastLeaveCredit.Id && e.ToDate > NewDt && e.Narration.ToUpper() == "Leave Requisition".ToUpper() && e.Id != LvNewReqListOldCancel_Id).OrderByDescending(e => e.ToDate).FirstOrDefault();

                    //current leave cancel leaves
                    List<int> LvNewReqListCuurentCancel_Id = db.LvNewReq.Where(e => e.EmployeeLeave_Id == EmployeeLeave_Id && e.Id >= LastLeaveCredit.Id && (e.Narration.ToString().ToUpper() == "Leave Requisition".ToUpper())
                     && e.LvOrignal_Id > 0).Select(e => e.Id).ToList();
                    //check for only leave requisitions, exclude cancel leave request, include leave credit record
                    var f = 8;
                    List<LvNewReq> LvNewReqListCuurent = db.LvNewReq.Include(r => r.WFStatus).Where(e => e.EmployeeLeave_Id == EmployeeLeave_Id && e.Id >= LastLeaveCredit.Id && (e.Narration.ToUpper() == "Leave Requisition".ToUpper() ||
                    e.Narration.ToUpper() == "Credit Process" || e.Narration.ToUpper() == "Leave Opening Balance".ToUpper()) && LvNewReqListCuurentCancel_Id.Contains(e.Id) == false).ToList();

                    //LeaveHistory_Combined = LvNewReqListCuurent;//LeaveHistory_Combined : Include Last year last record
                    //LeaveHistory_Combined.Add(LvNewReqListOld);


                    //LeaveHistory_Validation = LeaveHistory_Combined;
                    //LeaveHistory_Current : include Leave Credit Record
                    LeaveHistory_Current = db.LvNewReq.Where(e => e.EmployeeLeave_Id == EmployeeLeave_Id && e.LeaveHead_Id==OLvNewReq.LeaveHead_Id && e.Id >= LastLeaveCredit.Id).OrderBy(e => e.Id).ToList();
                    LeaveHistory_Last_Transaction = LeaveHistory_Current.LastOrDefault();
                    //Only include New Leave Requisitions exclude Leave Cancel/Encash/Credit/LeaveSharing
                    LeaveHistory_Validation = db.LvNewReq.Where(e => e.EmployeeLeave_Id == EmployeeLeave_Id && e.Id >= LastLeaveCredit.Id && e.Narration.ToUpper() == "Leave Requisition".ToUpper()).OrderBy(e => e.Id).ToList();
                    //LeaveHistory_Current.Where(e => e.Narration.ToUpper() == "Leave Requisition".ToUpper()).ToList();

                    LeaveHistory_Combined = LeaveHistory_Validation.ToList();//LeaveHistory_Combined : Include Last year last record
                    //modified 08122022
                    if (LvNewReqListOld != null)
                    {
                        LeaveHistory_Combined.Add(LvNewReqListOld);
                    }

                }
                else
                {

                    List<int> LvNewReqListCuurentCancel_Id = db.LvNewReq.Where(e => e.EmployeeLeave_Id == EmployeeLeave_Id && (e.Narration.ToUpper() == "Leave Requisition".ToUpper() ||
                    e.Narration.ToUpper() == "Credit Process".ToUpper()) && e.LvOrignal_Id != null).Select(e => e.Id).ToList();
                    //check for only leave requisitions, exclude cancel leave request, include leave credit record
                    List<LvNewReq> LvNewReqListCuurent = db.LvNewReq.Include(r => r.WFStatus).Where(e => e.EmployeeLeave_Id == EmployeeLeave_Id && (e.Narration.ToUpper() == "Leave Requisition".ToUpper() ||
                    e.Narration.ToUpper() == "Credit Process".ToUpper()) && LvNewReqListCuurentCancel_Id.Contains(e.Id) == false).ToList();
                    LeaveHistory_Combined = LvNewReqListCuurent;

                    //eaveHistory_Combined = LvNewReqListCuurent;

                    //LeaveHistory_Validation = LeaveHistory_Combined;

                    LeaveHistory_Current = db.LvNewReq.Where(e => e.EmployeeLeave_Id == EmployeeLeave_Id && e.Id >= LastLeaveCredit.Id).OrderBy(e => e.Id).ToList();
                    LeaveHistory_Last_Transaction = LeaveHistory_Current != null ? LeaveHistory_Current.LastOrDefault() : null;
                    LeaveHistory_Validation = LeaveHistory_Current.Where(e => e.Narration.ToUpper() == "Leave Requisition".ToUpper()).ToList();
                }
                //TotalLeaveHistory = TotalLeaveHistory.OrderBy(e => e.Id).ToList();
                //foreach (var TotalLeaveHistoryItem in TotalLeaveHistory)
                //{
                //    LookupValue WFStatus = db.LookupValue.Where(e => e.Id == TotalLeaveHistoryItem.WFStatus_Id).SingleOrDefault();
                //    TotalLeaveHistoryItem.WFStatus = WFStatus;
                //}
                //SelectLastTransaction = TotalLeaveHistory.Where(e => e.LeaveHead_Id == OLvNewReq.LeaveHead_Id && e.Narration.ToUpper()=="Leave Requisition".ToUpper()).LastOrDefault();//last leave transaction as per leave type
                #endregion Current Leave Year Leave requistions history with last year last leave transaction

                #region LeaveYear Parameters LeaveFrom and LeaveTo dynamic calculation
                //DateTime LvCreditDate = db.LvNewReq.Where(e => e.EmployeeLeave_Id == EmployeeLeave_Id && e.LeaveHead_Id == OLvNewReq.LeaveHead_Id && e.LvCreditDate != null).OrderByDescending(e => e.Id).FirstOrDefault().LvCreditDate.Value;
                //if (LvCreditDate == null)
                //{
                //    RetData.DebitDays = 0;
                //    RetData.ErrNo = 12;
                //    return RetData;//credit date no avilable
                //}
                ReturnDatacalendarpara RetDataparam = new ReturnDatacalendarpara();
                RetDataparam = LeaveCalendarpara(OLvNewReq.LeaveHead_Id.Value, EmployeeLeave_Id);
                if (RetDataparam.ErrNo > 0)
                {

                    RetData.DebitDays = 0;
                    RetData.ErrNo = RetDataparam.ErrNo;
                    return RetData;//creditpolicy date not avilable

                }
                var LeaveYearFrom = RetDataparam.Leaveyearfrom;
                var LeaveYearTo = RetDataparam.LeaveyearTo;
                DateTime LvCreditDate = LeaveYearFrom.Value;

                #endregion LeaveYear Parameters LeaveFrom and LeaveTo dynamic calculation

                var HoliDayListFinal_Current = new List<HolidayList>();//use for further condition checking
                var WeeklyOffListFinal_Current = new List<WeeklyOffList>();//use for further condition checking
                var WeeklyOffListFinal_Last = new List<WeeklyOffList>();//use for further condition checking

                LeaveHeadProcess.ReturnHWO_List HOWO_List = LeaveHeadProcess.Holiday_WeekOff_List(OLvNewReq.GeoStruct.Location_Id, OLvNewReq.GeoStruct.Department_Id, OLvNewReq.ToDate.Value);
                if (HOWO_List.Errno != 0)
                {
                    RetData.DebitDays = 0;
                    RetData.ErrNo = HOWO_List.Errno.Value;//Employee Holiday and Weekly off Calendar is not defined Err7- holiday err8 weeklyoff
                    return RetData;
                }
                HoliDayListFinal_Current = HOWO_List.HolidayList.ToList();
                WeeklyOffListFinal_Current= HOWO_List.WeeklyOffList_Current.ToList();
                WeeklyOffListFinal_Last= HOWO_List.WeeklyOffList_Last.ToList();
              
                #region check Closing balance by calling history existance
                var LeaveHistory_Select_CloseBal = LeaveHistory_Current.Where(e => e.LeaveHead_Id == OLvNewReq.LeaveHead_Id).LastOrDefault();
                if (LeaveHistory_Select_CloseBal == null)
                {
                    RetData.DebitDays = 0;
                    RetData.ErrNo = 10;
                    return RetData;//fill the opening balance
                }
                #endregion check Closing balance by calling history existance
                #region check for start date should not be more than end date
                if (OLvNewReq.FromDate.Value.Date > OLvNewReq.ToDate.Value.Date)
                {
                    RetData.DebitDays = 0;
                    RetData.ErrNo = 14;
                    return RetData;

                }
                #endregion check for start date should not be more than end date

                #region Check for Leave Existance
                if (LeaveHistory_Current != null)
                {
                   
                    var LvHistoryChk1 = LeaveHistory_Validation.Where(e => e.FromDate.Value.Date >= OLvNewReq.FromDate.Value.Date && e.FromDate.Value.Date <= OLvNewReq.ToDate.Value.Date).ToList();
                    
                    var LvHistoryChk2 = LeaveHistory_Validation.Where(e => e.ToDate.Value.Date >= OLvNewReq.FromDate.Value.Date && e.ToDate.Value.Date <= OLvNewReq.ToDate.Value.Date).ToList();
                    if (LvHistoryChk1.Count > 0 || LvHistoryChk2.Count > 0)
                    {
                        //already exits
                        RetData.DebitDays = 0;
                        RetData.ErrNo = 11;
                        return RetData;
                    }
                    var LvHistoryChk3 = LeaveHistory_Validation.Where(e => e.FromDate.Value.Date <= OLvNewReq.FromDate.Value.Date && e.ToDate.Value.Date >= OLvNewReq.ToDate.Value.Date).ToList();
                    if (LvHistoryChk3.Count > 0)
                    {
                        //already exits
                        RetData.DebitDays = 0;
                        RetData.ErrNo = 11;
                        return RetData;
                    }

                }
                #endregion Check for Leave Existance
                #region CheckLeave assignment for new head
                //  var CompanyLeave_Id = db.CompanyLeave.Where(e => e.Company_Id == CompanyId).Select(e => e.Id).SingleOrDefault();
                //List<Int32> LvHeadId = db.LvHead.Where(e => e.CompanyLeave_Id == CompanyLeave_Id).Select(e => e.Id).ToList();
                List<Int32> LvHeadId = db.LvHead.Select(e => e.Id).ToList();
                List<LvAssignment> OLvAssign = db.LvAssignment.Where(e => !LvHeadId.Contains(e.LvHead.Id)).AsNoTracking().OrderBy(e => e.Id).ToList();
                if (OLvAssign.Count() > 0)
                {
                    RetData.DebitDays = 0;
                    RetData.ErrNo = 12;
                    return RetData; //Leave assignment not done for new head
                }
                #endregion CheckLeave assignment for new head

                #region Check Employee Leave Structure for input leave
                //if (EmployeeLvStructDetails == null)
                //{
                //    RetData.DebitDays = 0;
                //    RetData.ErrNo = 27;
                //    return RetData; //No leave structure available
                //}
                #endregion Check Employee Leave Structure  for input leave

                #region check leave debit policy
                if (LvDebitPolicy == null)
                {
                    RetData.DebitDays = 0;
                    RetData.ErrNo = 3;
                    return RetData; //No leave debit policy defined
                }
                #endregion check leave debit policy



                var mTotalApplyLv = OLvNewReq.ToDate.Value.CompareTo(OLvNewReq.FromDate.Value);

                //use LvNewReqList object from credit date
                //get particular requset history for Closing balanc on lvledger exists
                #region check Closing balance by calling last record

                if (LeaveHistory_Last_Transaction == null)
                {
                    RetData.DebitDays = 0;
                    RetData.ErrNo = 10;
                    return RetData;//fill the opening balance
                }
                #endregion check Closing balance by calling last record
                #region check Closing balance by calling last record

                if (LeaveHistory_Last_Transaction != null && LeaveHistory_Last_Transaction.CloseBal == 0)
                {
                    if (LvDebitPolicy.IsDebitShare != true)
                    {
                        RetData.DebitDays = 0;
                        RetData.ErrNo = 13;//write new error 51

                        return RetData;//fill the opening balance
                    }
                    else
                    {
                        IsDebitSharing = true;
                    }
                }
                #endregion check Closing balance by calling last record


                //#region Check Leave input object existance
                //if (OLvNewReq == null)
                //{
                //    RetData.DebitDays = 0;
                //    RetData.ErrNo = 1;

                //    return RetData;//fill the requisition properly
                //}
                //#endregion Check Leave input object existance


                double mCloseBal = 0; double mPrefixCount = 0; double mOcc = 0; double mSufixCount = 0; double mLvCount = 0; double mLvCountPrefixSuffix = 0;
                mCloseBal = LeaveHistory_Last_Transaction.CloseBal;
                mOcc = LeaveHistory_Last_Transaction.LvOccurances;
                mPrefixCount = LeaveHistory_Last_Transaction.PrefixCount;
                mSufixCount = LeaveHistory_Last_Transaction.SufixCount;
                mLvCount = LeaveHistory_Last_Transaction.LVCount;
                mLvCountPrefixSuffix = LeaveHistory_Last_Transaction.LvCountPrefixSuffix;


                int monthval = 0;
                double result = 0;
                double days = 0;
                monthval = LvCreditPolicy.ProCreditFrequency;
                // monthval = Math.Abs(((Leaveyearfrom.Value.Year - LeaveyearTo.Value.Year) * 12) + Leaveyearfrom.Value.Month - LeaveyearTo.Value.Month) + 1;
                DateTime leaveyearfromChk;
                DateTime leaveyeartoChk;
                leaveyearfromChk = LeaveYearFrom.Value.AddMonths(monthval);
                leaveyeartoChk = LeaveyearTo.Value.AddMonths(monthval);

                //var lvcalendar = db.Calendar.Where(e => e.Name.LookupVal.ToUpper() == "LEAVECALENDAR" && e.Default == true).FirstOrDefault();

                //write leave year calculation function and set leay year from and end
                //DateTime leaveyearfromChk;
                //DateTime leaveyeartoChk;

                if (OLvNewReq.ReqDate.Value.Date >= leaveyearfromChk.Date && OLvNewReq.ReqDate.Value.Date <= leaveyeartoChk.Date && (OLvNewReq.FromDate.Value.Date > leaveyearfromChk.Date))
                {
                    RetData.DebitDays = 0;
                    RetData.ErrNo = 15;
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
                if (OLvNewReq.FromDate.Value.Date > LeaveyearTo.Value.Date)
                {
                    RetData.DebitDays = 0;
                    RetData.ErrNo = 16;
                    return RetData;//out off leave year
                }
                if (OLvNewReq.ToDate.Value.Date < Leaveyearfrom.Value.Date)
                {
                    RetData.DebitDays = 0;
                    RetData.ErrNo = 17;
                    return RetData;//out off leave year
                }

                #region Check debit policy for applied leave
                //leave debit policy check
                if (LvDebitPolicy != null)
                {
                    #region check end date should be more than start date
                    if (OLvNewReq.ToDate.Value < OLvNewReq.FromDate.Value)
                    {
                        RetData.DebitDays = 0;
                        RetData.ErrNo = 14;
                        return RetData;//end date should be more than start date
                    }
                    #endregion check end date should be more than start date
                    #region check lv balance is less that min apply leave
                    if (mCloseBal < LvDebitPolicy.MinUtilDays)
                    {
                        if (LvDebitPolicy.IsDebitShare != true)
                        {
                            RetData.DebitDays = 0;
                            RetData.ErrNo = 18;
                            return RetData;//lv balance is less that min apply leave
                        }
                        else
                        {
                            IsDebitSharing = true;
                        }
                    }
                    #endregion check lv balance is less that min apply leave

                    #region check leave occuances is more than max count
                    if (mOcc >= LvDebitPolicy.YearlyOccurances)
                    {
                        RetData.DebitDays = 0;
                        RetData.ErrNo = 43;
                        return RetData;//lv balance is less that min apply leave

                    }
                    #endregion check leave occuances is more than max count





                    #region check end date should be more than start date
                    if (OLvNewReq.ToDate.Value < OLvNewReq.FromDate.Value)
                    {
                        RetData.DebitDays = 0;
                        RetData.ErrNo = 14;
                        return RetData;//end date should be more than start date
                    }
                    #endregion check end date should be more than start date

                    #region check /from date should be more than Preapplied grace
                    if (LvDebitPolicy.PreApplied == true && LvDebitPolicy.PostApplied == false && OLvNewReq.FromDate.Value.Date < (DateTime.Now.AddDays(LvDebitPolicy.PreDays)).Date)
                    {
                        RetData.DebitDays = 0;
                        RetData.ErrNo = 19;
                        return RetData;//from date should be more than Preapplied grace
                    }
                    #endregion check /from date should be more than Preapplied grace
                    #region check from date should be less than Post Applied grace
                    if (LvDebitPolicy.PostApplied == true && LvDebitPolicy.PreApplied == false && OLvNewReq.FromDate.Value < (DateTime.Now.AddDays(-LvDebitPolicy.PostDays)))
                    {
                        RetData.DebitDays = 0;
                        RetData.ErrNo = 20;
                        return RetData;//from date should be less than Post Applied grace
                    }
                    #endregion check from date should be less than Post Applied grace
                    #region check from date should be less than pre Applied grace
                    if (LvDebitPolicy.PostApplied == false && LvDebitPolicy.PreApplied == true && OLvNewReq.FromDate.Value.Date < DateTime.Now.Date)
                    {
                        RetData.DebitDays = 0;
                        RetData.ErrNo = 21;
                        return RetData;
                    }
                    #endregion check from date should be less than pre Applied grace
                    #region check from date should be less than pre Applied grace
                    if (LvDebitPolicy.PreApplied == false && LvDebitPolicy.PostApplied == true && OLvNewReq.FromDate.Value.Date > DateTime.Now.Date)
                    {
                        RetData.DebitDays = 0;
                        RetData.ErrNo = 22;
                        return RetData;
                    }
                    #endregion check from date should be less than pre Applied grace
                    // Surendra Start
                    #region check from date should be more than Preapplied grace
                    if (LvDebitPolicy.PreApplied == true && LvDebitPolicy.PostApplied == true && OLvNewReq.FromDate.Value.Date < (DateTime.Now.AddDays(LvDebitPolicy.PreDays)).Date)
                    {
                        RetData.DebitDays = 0;
                        RetData.ErrNo = 23;
                        return RetData;//from date should be more than Preapplied grace
                    }
                    #endregion check from date should be less than pre Applied grace
                    #region check from date should be less than Post Applied grace
                    if (LvDebitPolicy.PostApplied == true && LvDebitPolicy.PreApplied == true && OLvNewReq.FromDate.Value.Date < (DateTime.Now.AddDays(-LvDebitPolicy.PostDays)).Date)
                    {
                        RetData.DebitDays = 0;
                        RetData.ErrNo = 24;
                        return RetData;//from date should be less than Post Applied grace
                    }
                    #endregion from date should be less than Post Applied grace

                    // Surendra end
                    #region check lv prefix/suffix leave apply limit over
                    //ML and MCL check pending
                    if (LvDebitPolicy.PrefixMaxCount != 0 && mPrefixCount == LvDebitPolicy.PrefixMaxCount)
                    {
                        RetData.DebitDays = 0;
                        RetData.ErrNo = 25;
                        return RetData;//lv prefix/suffix leave apply limit over
                    }
                    #endregion check lv prefix/suffix leave apply limit over

                    #region check Future leave can not be applied beyond OLvDebit.ApplyFutureGraceMonths months
                    if (LvDebitPolicy.ApplyFutureGraceMonths > 0)
                    {
                        //var FutureDateChk = leaveyeartoChk.AddMonths(LvDebitPolicy.ApplyFutureGraceMonths).Date;
                        if (OLvNewReq.ToDate.Value.Date > DateTime.Now.AddMonths(LvDebitPolicy.ApplyFutureGraceMonths).Date)
                        {
                            RetData.DebitDays = 0;
                            RetData.ErrNo = 26;
                            return RetData;//Future leave can not be applied beyond OLvDebit.ApplyFutureGraceMonths months
                        }
                    }
                    #endregion check past leave can not be applied beyond OLvDebit.ApplyPastGraceMonths months

                    #region check past leave can not be applied beyond OLvDebit.ApplyPastGraceMonths months
                    if (LvDebitPolicy.ApplyPastGraceMonths > 0)
                    {
                        if (OLvNewReq.FromDate.Value.Date < DateTime.Now.AddMonths(-LvDebitPolicy.ApplyPastGraceMonths).Date)
                        {
                            RetData.DebitDays = 0;
                            RetData.ErrNo = 27;
                            return RetData;//past leave can not be applied beyond OLvDebit.ApplyPastGraceMonths months
                        }
                    }
                    #endregion check past leave can not be applied beyond OLvDebit.ApplyPastGraceMonths months


                    double mDebitDays = (OLvNewReq.ToDate.Value.Date - OLvNewReq.FromDate.Value.Date).Days + 1;

                    #region check /Half day leave not allowed
                    //halfway allowed?
                    if (LvDebitPolicy.HalfDayAppl == false)
                    {
                        if (OLvNewReq.FromStat.LookupVal.ToUpper() != "FULLSESSION" || OLvNewReq.ToStat.LookupVal.ToUpper() != "FULLSESSION")
                        {
                            RetData.DebitDays = 0;
                            RetData.ErrNo = 28;
                            return RetData;//Half day leave not allowed
                        }

                    }
                    #endregion check /Half day leave not allowed
                    #region check Halfday/HalfPay debit days calculation
                    if ((OLvNewReq.FromStat.LookupVal.ToUpper() != "FULLSESSION") && (OLvNewReq.ToStat.LookupVal.ToUpper() == "FULLSESSION") && LvDebitPolicy.HalfDayAppl == true)
                    {
                        mDebitDays = mDebitDays - 0.5; //debit half day leave
                    }
                    if ((OLvNewReq.ToStat.LookupVal.ToUpper() != "FULLSESSION") && (OLvNewReq.FromStat.LookupVal.ToUpper() == "FULLSESSION") && LvDebitPolicy.HalfDayAppl == true)
                    {
                        mDebitDays = mDebitDays - 0.5; //debit half day leave
                    }

                    if ((OLvNewReq.ToStat.LookupVal.ToUpper() != "FULLSESSION") && (OLvNewReq.FromStat.LookupVal.ToUpper() != "FULLSESSION") && LvDebitPolicy.HalfDayAppl == true)
                    {
                        mDebitDays = mDebitDays - 1; // - 0.5; //debit half day leave
                    }


                    if (LvDebitPolicy.LvHead.HFPay == true)
                    {
                        mDebitDays = mDebitDays * 2;
                    }
                    else
                    {
                        mDebitDays = mDebitDays;
                    }
                    #endregion check Halfday/HalfPay debit days calculation


                    #region check lv debit is more than max apply leave
                    if (mDebitDays > LvDebitPolicy.MaxUtilDays)
                    {
                        RetData.DebitDays = 0;
                        RetData.ErrNo = 41;
                        return RetData;//lv balance is less that min apply leave

                    }
                    #endregion check lv debit is more than max apply leave

                    #region check FromDate & Todate is weekly off

                    var IsWeekOff = WeeklyOffListFinal_Current.Where(e => e.WeekDays.LookupVal.ToUpper() == OLvNewReq.FromDate.Value.DayOfWeek.ToString().ToUpper()
                    || e.WeekDays.LookupVal.ToUpper() == OLvNewReq.ToDate.Value.DayOfWeek.ToString().ToUpper()).SingleOrDefault();

                    if (IsWeekOff != null)
                    {
                        RetData.DebitDays = 0;
                        RetData.ErrNo = 29;
                        return RetData;
                    }

                    #endregion check FromDate & Todate is weekly off

                    #region check FromDate & Todate is weekly off/Holiday


                    //holiday check
                    //check to from date on holiday
                    #region check FromDate & Todate is holiday
                    var oDateIsHolidayDay = HoliDayListFinal_Current.Where(e => e.HolidayDate.Value.Date == OLvNewReq.FromDate.Value.Date || e.HolidayDate.Value.Date == OLvNewReq.ToDate.Value.Date).SingleOrDefault();
                    if (oDateIsHolidayDay != null)
                    {
                        RetData.DebitDays = 0;
                        RetData.ErrNo = 42;
                        return RetData;
                    }
                    #endregion check FromDate & Todate is holiday



                    #region check debit holidays
                    var DayCount = (OLvNewReq.ToDate.Value.Date - OLvNewReq.FromDate.Value.Date).TotalDays;//for leave include min. 3 day leave is required
                    if (LvDebitPolicy.HolidayInclusive == false && DayCount >= 2)
                    {
                        var mHolidayCnt = HoliDayListFinal_Current.Where(e => e.HolidayDate.Value.Date > OLvNewReq.FromDate.Value.Date && e.HolidayDate.Value.Date < OLvNewReq.ToDate.Value.Date).Count();
                        var mWeeklyOffCnt = 0;
                        for (var mdate = OLvNewReq.FromDate.Value.AddDays(1); mdate.Date < OLvNewReq.ToDate.Value.Date; mdate = mdate.AddDays(1))
                        {
                            if (HoliDayListFinal_Current.Where(e => e.HolidayDate.Value.Date == mdate.Date).SingleOrDefault() == null)
                            {
                                mWeeklyOffCnt = mWeeklyOffCnt + WeeklyOffListFinal_Current.Where(e => e.WeekDays.LookupVal.ToUpper() == mdate.DayOfWeek.ToString().ToUpper()).Count();
                            }
                        }

                        mDebitDays = mDebitDays - mHolidayCnt - mWeeklyOffCnt;//HolidayCount(OLvNewReq, EmployeeId); //debit holidays
                        IsLvHolidayWeeklyoffExclude = true;

                    }
                    #endregion check debit holidays


                    #endregion check FromDate & Todate is weekly off/Holiday
                    //use this object for prefix siffix and combined leave
                    //var LvPrefixSuffix_Result = PrefixSufixCount("", OLvNewReq, CompanyId, HoliDayListFinal_Current, WeeklyOffListFinal_Current, WeeklyOffListFinal_Last, CreditDate, LeaveHistory_Validation);
                    var LvPrefixSuffix_Result = new prefixsuffixapl();
                    if (LvDebitPolicy.Sandwich == true || LvDebitPolicy.PrefixSuffix == true || LvDebitPolicy.PreApplyPrefixSuffix == true || LvDebitPolicy.PostApplyPrefixSuffix == true || LvDebitPolicy.Combined == true)
                    {
                        LvPrefixSuffix_Result = PrefixSufixCount(OLvNewReq, HoliDayListFinal_Current, WeeklyOffListFinal_Current, WeeklyOffListFinal_Last, LvCreditDate, LeaveHistory_Validation);
                    }
                    #region check for combined leave policy
                    if (LvDebitPolicy.Combined == true)
                    {
                        if (LvDebitPolicy.CombinedLvHead == null || LvDebitPolicy.CombinedLvHead.Count == 0)
                        {
                            RetData.DebitDays = 0;
                            RetData.ErrNo = 30;
                            return RetData;
                        }
                        if (LvPrefixSuffix_Result != null)//if null, it is already checked for leave validation
                        {
                            if (LvPrefixSuffix_Result.CombineLvPrefix_Id != 0 && LvDebitPolicy.CombinedLvHead != null)
                            {
                                var Verify_Lv_Head_Id = db.LvNewReq.Where(e => e.Id == LvPrefixSuffix_Result.CombineLvPrefix_Id).Select(e => e.LeaveHead_Id).SingleOrDefault();
                                var Combined_Ok = LvDebitPolicy.CombinedLvHead.Where(e => e.LvHead_Id == Verify_Lv_Head_Id).SingleOrDefault();//last leave under combined leave policy
                                if (Combined_Ok == null)
                                {
                                    RetData.DebitDays = 0;
                                    RetData.ErrNo = 31;//prefix
                                    return RetData;
                                }

                            }
                            if (LvPrefixSuffix_Result.CombineLvSufix_Id != 0 && LvDebitPolicy.CombinedLvHead != null)
                            {
                                var Verify_Lv_Head_Id = db.LvNewReq.Where(e => e.Id == LvPrefixSuffix_Result.CombineLvSufix_Id).Select(e => e.LeaveHead_Id).SingleOrDefault();
                                var Combined_Ok = LvDebitPolicy.CombinedLvHead.Where(e => e.LvHead_Id == Verify_Lv_Head_Id).SingleOrDefault();
                                if (Combined_Ok == null)
                                {
                                    RetData.DebitDays = 0;
                                    RetData.ErrNo = 32;//sufix
                                    return RetData;
                                }

                            }
                        }
                    }
                    #endregion check for combined leave policy

                    //applyPrfixSufix
                    #region check for applyPrfixSufix leave policy
                    if (LvPrefixSuffix_Result._TotalDays > 0)
                    {
                        //new debit policy parameters for checking prefixsuffic action of leave debit days 30/11/2022
                        //check data for PreficSuffix policy

                        //from day second session and todate first session leave then halfday policy will be applicable
                        //discard prefixsuffix action
                        var FromStat_Session = db.LookupValue.Where(e => e.Id == OLvNewReq.FromStat_Id).Select(e => e.LookupVal).FirstOrDefault();
                        var ToStat_Session = db.LookupValue.Where(e => e.Id == OLvNewReq.ToStat_Id).Select(e => e.LookupVal).FirstOrDefault();
                        if (LvDebitPolicy.IsHalfDaySession_WaiveOffPrefixSuffix == true && FromStat_Session == "SECONDSESSION" && ToStat_Session == "FIRSTSESSION")
                        {
                            mLvnewReqprefix = 0;
                            mLvnewReqSuffix = 0;
                            RetData.DebitDays = mDebitDays;
                            RetData.ErrNo = 0;
                            RetData.LvCountPrefixSuffix = mLvCountPrefixSuffix;
                            RetData.LvnewReqprefix = mPrefixCount;
                            RetData.LvnewReqSuffix = mSufixCount;
                            RetData.IsLvDebitSharing = IsDebitSharing;
                            return RetData;
                        }


                        if (LvDebitPolicy.Sandwich == true)
                        {


                        }
                        if (mLvCountPrefixSuffix < LvDebitPolicy.PrefixGraceCount)
                        {
                            mLvCountPrefixSuffix = mLvCountPrefixSuffix + 1;
                            mLvnewReqprefix = LvPrefixSuffix_Result._isPrifix == true ? (mPrefixCount + 1) : mPrefixCount;
                            mLvnewReqprefix = LvPrefixSuffix_Result._isSufix == true ? (mSufixCount + 1) : mSufixCount;
                            mDebitDays = mDebitDays;
                            //message code
                        }
                        else if (mLvCountPrefixSuffix >= LvDebitPolicy.PrefixMaxCount)
                        {
                            mLvCountPrefixSuffix = mLvCountPrefixSuffix + 1;
                            mLvnewReqprefix = LvPrefixSuffix_Result._isPrifix == true ? (mPrefixCount + 1) : mPrefixCount;
                            mLvnewReqprefix = LvPrefixSuffix_Result._isSufix == true ? (mSufixCount + 1) : mSufixCount;
                            mDebitDays = mDebitDays;
                            //message code
                        }
                        else if (LvDebitPolicy.PreApplyPrefixSuffix == true && OLvNewReq.FromDate.Value.Date > DateTime.Now.Date)
                        {
                            mLvCountPrefixSuffix = mLvCountPrefixSuffix + 1;
                            mLvnewReqprefix = LvPrefixSuffix_Result._isPrifix == true ? (mPrefixCount + 1) : mPrefixCount;
                            mLvnewReqprefix = LvPrefixSuffix_Result._isSufix == true ? (mSufixCount + 1) : mSufixCount;
                            if (LvDebitPolicy.PreApplyPrefixSuffix_PrefixSuffixAction != null)
                            {
                                if (LvDebitPolicy.PreApplyPrefixSuffix_PrefixSuffixAction.IsFixedDayDebit_PrefixSuffixAction == true)//fixed day debit actionn
                                {
                                    mDebitDays = mDebitDays + LvDebitPolicy.PreApplyPrefixSuffix_PrefixSuffixAction.FixedDebitDay;

                                }
                                else if (LvDebitPolicy.PreApplyPrefixSuffix_PrefixSuffixAction.IsActualDayDebit_PrefixSuffixAction == true)//fixed day debit actionn
                                {
                                    mDebitDays = mDebitDays + LvPrefixSuffix_Result._TotalDays;
                                }
                                else if (LvDebitPolicy.PreApplyPrefixSuffix_PrefixSuffixAction.IsWaiveOffDayDebit_PrefixSuffixAction == true)//fixed day debit actionn
                                {
                                    mDebitDays = mDebitDays + LvPrefixSuffix_Result._TotalDays - LvDebitPolicy.PreApplyPrefixSuffix_PrefixSuffixAction.WaiveOffDebitDay;
                                }
                            }
                            else
                            {
                                RetData.DebitDays = 0;
                                RetData.ErrNo = 46;//sufix
                                return RetData;
                            }
                        }
                        else if (LvDebitPolicy.PostApplyPrefixSuffix == true && OLvNewReq.FromDate.Value.Date < DateTime.Now.Date)
                        {
                            mLvCountPrefixSuffix = mLvCountPrefixSuffix + 1;
                            mLvnewReqprefix = LvPrefixSuffix_Result._isPrifix == true ? (mPrefixCount + 1) : mPrefixCount;
                            mLvnewReqprefix = LvPrefixSuffix_Result._isSufix == true ? (mSufixCount + 1) : mSufixCount;
                            if (LvDebitPolicy.PostApplyPrefixSuffix_PrefixSuffixAction != null)
                            {

                                if (LvDebitPolicy.PostApplyPrefixSuffix_PrefixSuffixAction.IsFixedDayDebit_PrefixSuffixAction == true)//fixed day debit actionn
                                {
                                    mDebitDays = mDebitDays + LvDebitPolicy.PostApplyPrefixSuffix_PrefixSuffixAction.FixedDebitDay;
                                }
                                else if (LvDebitPolicy.PostApplyPrefixSuffix_PrefixSuffixAction.IsActualDayDebit_PrefixSuffixAction == true)//fixed day debit actionn
                                {
                                    mDebitDays = mDebitDays + LvPrefixSuffix_Result._TotalDays;
                                }
                                else if (LvDebitPolicy.PostApplyPrefixSuffix_PrefixSuffixAction.IsWaiveOffDayDebit_PrefixSuffixAction == true)//fixed day debit actionn
                                {
                                    mDebitDays = mDebitDays + LvPrefixSuffix_Result._TotalDays - LvDebitPolicy.PostApplyPrefixSuffix_PrefixSuffixAction.WaiveOffDebitDay;
                                }
                            }
                            else
                            {
                                RetData.DebitDays = 0;
                                RetData.ErrNo = 47;//sufix
                                return RetData;
                            }

                        }
                        else if (LvDebitPolicy.PrefixSuffix == true)
                        {
                            mLvCountPrefixSuffix = mLvCountPrefixSuffix + 1;
                            mLvnewReqprefix = LvPrefixSuffix_Result._isPrifix == true ? (mPrefixCount + 1) : mPrefixCount;
                            mLvnewReqprefix = LvPrefixSuffix_Result._isSufix == true ? (mSufixCount + 1) : mSufixCount;
                            if (LvDebitPolicy.PrefixSuffix_PrefixSuffixAction != null)
                            {
                                if (LvDebitPolicy.PrefixSuffix_PrefixSuffixAction.IsFixedDayDebit_PrefixSuffixAction == true)//fixed day debit actionn
                                {
                                    mDebitDays = mDebitDays + LvDebitPolicy.PrefixSuffix_PrefixSuffixAction.FixedDebitDay;
                                }
                                else if (LvDebitPolicy.PrefixSuffix_PrefixSuffixAction.IsActualDayDebit_PrefixSuffixAction == true)//fixed day debit actionn
                                {
                                    mDebitDays = mDebitDays + LvPrefixSuffix_Result._TotalDays;
                                }
                                else if (LvDebitPolicy.PrefixSuffix_PrefixSuffixAction.IsWaiveOffDayDebit_PrefixSuffixAction == true)//fixed day debit actionn
                                {
                                    mDebitDays = mDebitDays + LvPrefixSuffix_Result._TotalDays - LvDebitPolicy.PrefixSuffix_PrefixSuffixAction.WaiveOffDebitDay;
                                }
                            }
                            else
                            {
                                RetData.DebitDays = 0;
                                RetData.ErrNo = 48;//sufix
                                return RetData;
                            }

                        }

                    }

                    #endregion check for applyPrfixSufix leave policy
                    //var ComCode = 1;
                    if (mDebitDays > mCloseBal)
                    {
                        if (LvDebitPolicy.IsDebitShare != true)
                        {
                            RetData.DebitDays = 0;
                            RetData.ErrNo = 33;
                            return RetData; //Leave can not be applied beyond maximum limit
                        }
                        else
                        {
                            IsDebitSharing = true;
                        }
                    }
                    if (mDebitDays > LvDebitPolicy.MaxUtilDays)
                    {
                        RetData.DebitDays = 0;
                        RetData.ErrNo = 34;
                        return RetData; //Leave can not be applied beyond maximum limit
                    }

                    RetData.DebitDays = mDebitDays;
                    RetData.ErrNo = 0;
                    if (mLvnewReqprefix > 0 || mLvnewReqSuffix > 0)
                    {
                        RetData.PrefixSufix = true;
                    }
                    else
                    {
                        RetData.PrefixSufix = false;
                    }
                    RetData.DebitDays = mDebitDays;
                    RetData.LvnewReqprefix = mLvnewReqprefix;
                    RetData.LvnewReqSuffix = mLvnewReqSuffix;
                    RetData.IsLvDebitSharing = IsDebitSharing;
                    RetData.LvCountPrefixSuffix = mLvCountPrefixSuffix;
                    return RetData;
                }

                #endregion Check debit policy for applied leave

                return RetData;

            }
        }
        public static ReturnDatacalendarpara LeaveCalendarpara(int LvHeadId, int EmployeeLvId)
        {
            ReturnDatacalendarpara RetDataparam = new ReturnDatacalendarpara();
            using (DataBaseContext db = new DataBaseContext())
            {
                var m = 10;
                var EmployeeLvStruct = db.EmployeeLvStruct.Where(e => e.EmployeeLeave_Id == EmployeeLvId && e.EndDate == null).SingleOrDefault();
                if (EmployeeLvStruct == null)
                {
                    RetDataparam.ErrNo = 2;//Employee Leave structure not avaialable
                    return RetDataparam;
                }
                List<int> EmployeeLvStructDetails_FormulaId = db.EmployeeLvStructDetails.Where(e => e.EmployeeLvStruct_Id == EmployeeLvStruct.Id && e.LvHead_Id == LvHeadId && e.LvHeadFormula_Id != null).Select(e => e.LvHeadFormula.Id).ToList();
                m = 24; ;

                LvHeadFormula LvHeadFormula = db.LvHeadFormula.Where(e => EmployeeLvStructDetails_FormulaId.Contains(e.Id) && e.LvCreditPolicy_Id > 0).SingleOrDefault();
                m = 24; ;
                LvCreditPolicy LvCreditPolicy = db.LvCreditPolicy.Where(e => e.Id == LvHeadFormula.LvCreditPolicy_Id && e.CreditDate != null).FirstOrDefault();
                LvHeadFormula.LvCreditPolicy = LvCreditPolicy;
                LookupValue CreditDate = db.LookupValue.Where(e => e.Id == LvCreditPolicy.CreditDate_Id).FirstOrDefault();
                LvCreditPolicy.CreditDate = CreditDate;

                if (LvCreditPolicy == null)
                {

                    RetDataparam.ErrNo = 4;//no lv credit policy
                    return RetDataparam;


                }


                DateTime LvCreditDate = db.LvNewReq.Where(e => e.EmployeeLeave_Id == EmployeeLvId && e.LeaveHead_Id == LvHeadId && e.LvCreditDate != null).OrderByDescending(e => e.Id).FirstOrDefault().LvCreditDate.Value;
                if (LvCreditDate == null)
                {
                    RetDataparam.ErrNo = 5;
                    return RetDataparam;//credit date no avilable
                }
                DateTime? Leaveyearfrom = null;
                DateTime? LeaveyearTo = null;
                var Fulldate = "";
                int ProCreditFrequency = LvCreditPolicy.ProCreditFrequency;//Frequency in months to create leave year end

                Fulldate = LvCreditDate.ToShortDateString();
                Leaveyearfrom = Convert.ToDateTime(Fulldate);
                LeaveyearTo = Leaveyearfrom.Value.AddDays(-1);
                LeaveyearTo = LeaveyearTo.Value.AddMonths(ProCreditFrequency);//newly modified on 16/11/2022 by Prashant taking concept of leaveyear variation
                RetDataparam.Leaveyearfrom = Leaveyearfrom;
                RetDataparam.LeaveyearTo = LeaveyearTo;
                RetDataparam.ErrNo = 0;

                return RetDataparam;
            }
        }

      
        public static prefixsuffixapl PrefixSufixCount(LvNewReq OLvNewReq, List<HolidayList> HolidayList_Current, List<WeeklyOffList> WeeklyOffList_Current,
            List<WeeklyOffList> WeeklyOffList_Last, DateTime CreditDate, List<LvNewReq> LeaveHistory_Validation)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                db.Configuration.AutoDetectChangesEnabled = false;

                double mTotPrefix = 0;
                double mTotSuffix = 0;
                double mTotPrefixSuffix = 0;
                Boolean LvNewReqPrefix = false, LvNewReqSuffix = false;
                int CombineLvPrefix_Id = 0;
                int CombineLvSufix_Id = 0;
                //continue 17/11/2022 18:16*******************************************************************************************************************************
                #region Prefix days and Leave Combine calculation
                for (var mdate = OLvNewReq.FromDate.Value.AddDays(-1).Date; mdate.Date >= OLvNewReq.FromDate.Value.AddDays(-10).Date; mdate = mdate.AddDays(-1))
                {
                    var OHolidayChk = HolidayList_Current != null ? HolidayList_Current.Where(t => t.HolidayDate.Value.Date == mdate.Date).SingleOrDefault() : null;
                    var OnWeaklyOff = WeeklyOffList_Current != null ? WeeklyOffList_Current.Where(t => t.WeekDays.LookupVal.ToUpper() == mdate.DayOfWeek.ToString().ToUpper() && mdate.Date >= CreditDate.Date.Date).SingleOrDefault() : null;
                    var OnWeaklyOff_LY = WeeklyOffList_Last != null ? WeeklyOffList_Last.Where(t => t.WeekDays.LookupVal.ToUpper() == mdate.DayOfWeek.ToString().ToUpper() && mdate.Date < CreditDate.Date).SingleOrDefault() : null;
                    if (OHolidayChk != null || OnWeaklyOff != null || OnWeaklyOff_LY != null)
                    {
                        mTotPrefix = mTotPrefix + 1;
                        LvNewReqPrefix = true;
                    }
                    else
                    {
                        CombineLvPrefix_Id = LeaveHistory_Validation.Where(e => e.FromDate.Value.Date >= mdate.Date && e.ToDate.Value.Date <= mdate.Date).Select(e => e.Id).FirstOrDefault();
                        break;
                    }

                }
                #endregion Prefix days calculation
                #region Sufix daysand Leave Combine calculation
                for (DateTime mdate = OLvNewReq.ToDate.Value.AddDays(+1).Date; mdate.Date <= OLvNewReq.ToDate.Value.AddDays(+10).Date; mdate = mdate.AddDays(+1))
                {
                    var aa = 0;
                    var OHolidayChk = HolidayList_Current != null ? HolidayList_Current.Where(t => t.HolidayDate.Value.Date == mdate.Date).SingleOrDefault() : null;
                    var OnWeaklyOff = WeeklyOffList_Current != null ? WeeklyOffList_Current.Where(t => t.WeekDays.LookupVal.ToUpper() == mdate.DayOfWeek.ToString().ToUpper() && mdate.Date >= CreditDate.Date).SingleOrDefault() : null;
                    var OnWeaklyOff_LY = WeeklyOffList_Last != null ? WeeklyOffList_Last.Where(t => t.WeekDays.LookupVal.ToUpper() == mdate.DayOfWeek.ToString().ToUpper() && mdate.Date < CreditDate.Date).SingleOrDefault() : null;
                    if (OHolidayChk != null || OnWeaklyOff != null || OnWeaklyOff_LY != null)
                    {
                        mTotSuffix = mTotSuffix + 1;
                        LvNewReqSuffix = true;
                    }
                    else
                    {
                        CombineLvSufix_Id = LeaveHistory_Validation.Where(e => e.FromDate.Value.Date >= mdate.Date && e.ToDate.Value.Date <= mdate.Date).Select(e => e.Id).FirstOrDefault();
                        break;
                    }

                }
                #endregion Sufix days calculation

                mTotPrefixSuffix = mTotSuffix + mTotPrefix;

                //var ComId = 3;// Convert.ToInt32(SessionManager.CompanyId);
                //var ComCode = "aa";// db.Company.Where(e => e.Id == ComId).AsNoTracking().OrderBy(e => e.Id).SingleOrDefault().Code;
                //    if (ComCode.ToUpper() == "HCBL" && OLvNewReq.LeaveHead.LvCode == "CL")//hindustan bank
                //    {
                //        if (OLvNewReq.FromStat.LookupVal.ToUpper() != "FULLSESSION")
                //        {
                //            if (mTotPrefix != 0)
                //            {
                //                mTotPrefixSuffix = mTotPrefixSuffix - mTotPrefix;
                //            }
                //        }
                //        if (OLvNewReq.ToStat.LookupVal.ToUpper() == "FIRSTSESSION")
                //        {
                //            if (mTotSuffix != 0)
                //            {
                //                mTotPrefixSuffix = mTotPrefixSuffix - mTotSuffix;
                //            }
                //        }


                //    }
                //    else
                //    {

                //        if (mTotPrefixSuffix != 0)
                //        {
                //            mTotPrefixSuffix = mTotPrefixSuffix - 1;//standard sufix-Prefix policy
                //        }
                //    }


                return new prefixsuffixapl
                {

                    _TotalDays = mTotPrefixSuffix,
                    _isPrifix = LvNewReqPrefix,
                    _isSufix = LvNewReqSuffix,
                    mTotPrefix = mTotPrefix,
                    mTotSuffix = mTotSuffix,
                    CombineLvPrefix_Id = CombineLvPrefix_Id,
                    CombineLvSufix_Id = CombineLvSufix_Id
                };


            }
        }

        public class ReturnHWO_List
        {
            public int? Errno { get; set; }
            public ICollection<HolidayList> HolidayList { get; set; }
            public ICollection<WeeklyOffList> WeeklyOffList_Current { get; set; }
            public ICollection<WeeklyOffList> WeeklyOffList_Last { get; set; }

        }

        public static ReturnHWO_List Holiday_WeekOff_List(Int32? LocId, Int32? DeptId, DateTime ToDate)//Holiday weekly off list to check day status
        {
            ReturnHWO_List ReturnHWO_List = new ReturnHWO_List();
            try
            {
                using (DataBaseContext db = new DataBaseContext())
                {
                    //ReturnHWO_List ReturnHWO_List = new ReturnHWO_List();

                    int default_DateIsHolidayDay = db.Calendar.Where(e => e.Name.LookupVal.ToUpper() == "HOLIDAYCALENDAR" && e.Default == true && e.ToDate >= ToDate.Date)
                       .AsNoTracking().OrderBy(e => e.Id).Select(e => e.Id).FirstOrDefault();
                    if (default_DateIsHolidayDay == 0)
                    {
                        ReturnHWO_List.Errno = 612; //Holiday calendar not defined
                        return ReturnHWO_List;
                    }
                    int default_DateIsWeeklyoff = db.Calendar.Where(e => e.Name.LookupVal.ToUpper() == "WEEKLYOFFCALENDAR" && e.Default == true && e.ToDate >= ToDate.Date)
                        .AsNoTracking().OrderBy(e => e.Id).Select(e => e.Id).FirstOrDefault();
                    if (default_DateIsWeeklyoff == 0)
                    {
                        ReturnHWO_List.Errno = 613; //weekly calendar not defined
                        return ReturnHWO_List;
                    }
                    List<HolidayList> HolidayList = new List<HolidayList>();
                    List<WeeklyOffList> WeeklyOffList_Current = new List<WeeklyOffList>();
                    List<WeeklyOffList> WeeklyOffList_Last = new List<WeeklyOffList>();


                    int HolidayCalendar_Id_Loc = db.Location.Where(e => e.Id == LocId).Select(e => e.HolidayCalendar.Where(a => a.HoliCalendar.Id == default_DateIsHolidayDay).Select(z => z.Id).FirstOrDefault())
                                    .FirstOrDefault();

                    List<HolidayList> HolidayListLoc = db.HolidayCalendar.Where(e => e.Id == HolidayCalendar_Id_Loc).Select(e => e.HolidayList.ToList()).SingleOrDefault();

                    List<HolidayList> HolidayListLastLoc = db.HolidayCalendar.Where(e => e.Id == (HolidayCalendar_Id_Loc - 1)).Select(e => e.HolidayList.ToList()).SingleOrDefault();

                    int HolidayCalendar_Id_Dept = db.Department.Where(e => e.Id == DeptId).Select(e => e.HolidayCalendar.Where(a => a.HoliCalendar.Id == default_DateIsHolidayDay).Select(z => z.Id).FirstOrDefault())
                                    .FirstOrDefault();

                    List<HolidayList> HolidayListDept = db.HolidayCalendar.Where(e => e.Id == HolidayCalendar_Id_Dept).Select(e => e.HolidayList.ToList()).SingleOrDefault();

                    List<HolidayList> HolidayListLastDept = db.HolidayCalendar.Where(e => e.Id == (HolidayCalendar_Id_Dept - 1)).Select(e => e.HolidayList.ToList()).SingleOrDefault();

                    if (HolidayListDept != null)
                    {
                        HolidayList.AddRange(HolidayListDept);
                        if (HolidayListLastDept != null)
                        {
                            HolidayList.AddRange(HolidayListLastDept);
                        }
                        else if (HolidayListLastLoc != null)
                        {
                            HolidayList.AddRange(HolidayListLastLoc);
                        }
                        else { }
                    }
                    else if (HolidayListLoc != null)
                    {

                        HolidayList.AddRange(HolidayListLoc);
                        if (HolidayListLastLoc != null)
                        {
                            HolidayList.AddRange(HolidayListLastLoc);
                        }
                        else if (HolidayListLastDept != null)
                        {
                            HolidayList.AddRange(HolidayListLastDept);
                        }
                    }


                    int WOCalendar_Id_Loc = db.Location.Where(e => e.Id == LocId).Select(e => e.WeeklyOffCalendar.Where(a => a.WOCalendar.Id == default_DateIsWeeklyoff).Select(z => z.Id).FirstOrDefault())
                                    .FirstOrDefault();
                    List<WeeklyOffList> WeeklyOffList_Current_Loc = db.WeeklyOffCalendar.Where(e => e.Id == WOCalendar_Id_Loc).Select(e => e.WeeklyOffList.ToList()).SingleOrDefault();
                    int WOCalendar_Id_Dept = db.Department.Where(e => e.Id == DeptId).Select(e => e.WeeklyOffCalendar.Where(a => a.WOCalendar.Id == default_DateIsWeeklyoff).Select(z => z.Id).FirstOrDefault())
                                    .FirstOrDefault();
                    List<WeeklyOffList> WeeklyOffList_Current_Dept = db.WeeklyOffCalendar.Where(e => e.Id == WOCalendar_Id_Dept).Select(e => e.WeeklyOffList.ToList()).SingleOrDefault();
                    if (WeeklyOffList_Current_Dept != null)
                    {
                        WeeklyOffList_Current.AddRange(WeeklyOffList_Current_Dept);
                    }
                    else if (WeeklyOffList_Current_Loc != null)
                    {
                        WeeklyOffList_Current.AddRange(WeeklyOffList_Current_Loc);
                    }



                    foreach (var WeeklyOffList_Currentitem in WeeklyOffList_Current)
                    {
                        LookupValue WeekDays = db.LookupValue.Where(e => e.Id == WeeklyOffList_Currentitem.WeekDays_Id).SingleOrDefault();
                        WeeklyOffList_Currentitem.WeekDays = WeekDays;
                        LookupValue WeeklyOffStatus = db.LookupValue.Where(e => e.Id == WeeklyOffList_Currentitem.WeeklyOffStatus_Id).SingleOrDefault();
                        WeeklyOffList_Currentitem.WeeklyOffStatus = WeeklyOffStatus;
                    }

                    List<WeeklyOffList> WeeklyOffList_Last_Loc = db.WeeklyOffCalendar.Where(e => e.Id == (WOCalendar_Id_Loc - 1)).Select(e => e.WeeklyOffList.ToList()).SingleOrDefault();
                    List<WeeklyOffList> WeeklyOffList_Last_Dept = db.WeeklyOffCalendar.Where(e => e.Id < (WOCalendar_Id_Dept - 1)).Select(e => e.WeeklyOffList.ToList()).SingleOrDefault();
                    if (WeeklyOffList_Last_Dept != null)
                    {
                        WeeklyOffList_Last.AddRange(WeeklyOffList_Last_Dept);
                    }
                    else if (WeeklyOffList_Last_Loc != null)
                    {
                        WeeklyOffList_Last.AddRange(WeeklyOffList_Last_Loc);
                    }

                    foreach (var WeeklyOffList_Lastitem in WeeklyOffList_Last)
                    {
                        LookupValue WeekDays = db.LookupValue.Where(e => e.Id == WeeklyOffList_Lastitem.WeekDays_Id).SingleOrDefault();
                        WeeklyOffList_Lastitem.WeekDays = WeekDays;
                        LookupValue WeeklyOffStatus = db.LookupValue.Where(e => e.Id == WeeklyOffList_Lastitem.WeeklyOffStatus_Id).SingleOrDefault();
                        WeeklyOffList_Lastitem.WeeklyOffStatus = WeeklyOffStatus;
                    }
                    ReturnHWO_List.Errno = 0; //Holiday calendar not defined
                    ReturnHWO_List.HolidayList = HolidayList;
                    ReturnHWO_List.WeeklyOffList_Current = WeeklyOffList_Current;
                    ReturnHWO_List.WeeklyOffList_Last = WeeklyOffList_Last;
                    return ReturnHWO_List;
                }
            }
            catch (Exception Ex)
            {
                ReturnHWO_List.Errno = 606; //Holiday calendar not defined
                return ReturnHWO_List;
            }
        }

    }

}


