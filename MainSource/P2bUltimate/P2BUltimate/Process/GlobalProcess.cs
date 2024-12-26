using P2b.Global;
using P2BUltimate.App_Start;
//using P2B.BUSINESSMODEL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using Payroll;
using P2B.PFTRUST;
using P2BUltimate.Security;
//using System.Linq.Dynamic.Core;
using System.Reflection.Emit;
using Microsoft.ReportingServices.DataProcessing;
using System.Globalization;
using System.Web.UI;
using IR;
using System.Web;
using System.Web.Mvc;
//using static P2B.BUSINESSLOGIC.PFTrust;

namespace P2BUltimate.Process
{
    public static class GlobalProcess
    {
        //public class ReturnHWO_List
        //{
        //    public int? Errno { get; set; }
        //    public string ErrMsg { get; set; }

        //    public ICollection<HolidayList> HolidayList { get; set; }
        //    public ICollection<WeeklyOffList> WeeklyOffList_Current { get; set; }
        //    public ICollection<WeeklyOffList> WeeklyOffList_Last { get; set; }

        //}
        public class ErrorLog
        {
            public int? Errno { get; set; }
            public string ErrMsg { get; set; }
        }
        public class ReturnData
        {
            public double ReturnValue { get; set; }
            public int? Errno { get; set; }
            public string ErrMsg { get; set; }
        }
        public class ReturnDataintp
        {
            public double ReturnValue { get; set; }
            public int? Errno { get; set; }
            public string ErrMsg { get; set; }
        }
        #region PFECR data upload from Payroll Module into PFTrust module
        //  public static ReturnData UploadECR( string MonthYear)
        //public static List<string> UploadECR(string MonthYear)
        //{
        //    //ReturnData ErrorLog = new ReturnData();
        //    List<string> Msg = new List<string>();
        //    try
        //    {
        //        var EmpCount = 0;
        //        using (DataBaseContext db = new DataBaseContext())
        //        {
        //            DBTrack DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
        //            var SalaryT = db.SalaryT.Where(e => e.PayMonth == MonthYear && e.ReleaseDate != null).Select(e => e.Id).ToList();
        //            //var SalaryT = db.SalaryT.Where(e => e.PayMonth == MonthYear && e.ReleaseDate != null && e.EmployeePayroll_Id == 840).Select(e => e.Id).ToList();
        //            DateTime Dateverifyfinancialyear = Convert.ToDateTime("01/" + MonthYear);

        //            var PassbookLoanIDValue = new List<string>();
        //            //PassbookLoanIDValue[0] = "Monthly PF Posting";
        //            //PassbookLoanIDValue[1] = "PF Balance";
        //            PassbookLoanIDValue.Add("Monthly PF Posting");
        //            PassbookLoanIDValue.Add("PF Balance");
        //            List<int> PassbookID = new List<int>();
        //            PassbookID = db.LookupValue.Where(e => PassbookLoanIDValue.Contains(e.LookupVal.ToUpper())).Select(e => e.Id).ToList();

        //            var Id = Convert.ToInt32(SessionManager.CompanyId);
        //            var CompCodeid = db.Company.Where(e => e.Id == Id).SingleOrDefault();
        //            var qurey = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "PFCALENDAR".ToUpper() && e.Default == true).AsEnumerable()
        //              .Select(e => new
        //              {
        //                  Id = e.Id,
        //                  Lvcalendardesc = "FromDate :" + e.FromDate.Value.ToShortDateString() + " ToDate :" + e.ToDate.Value.ToShortDateString(),
        //                  FromDate = e.FromDate.Value.ToShortDateString(),
        //                  ToDate = e.ToDate.Value.ToShortDateString()

        //              }).SingleOrDefault();



        //            ReturnData ReturnData = new ReturnData();
        //            var CompantPFTrust_Id = db.CompanyPFTrust.Where(e => e.Company_Id == CompCodeid.Id).Select(e => e.Id).SingleOrDefault();
        //            var IntPolicyId = db.PFTACCalendar.Include(e => e.PFTTDSMaster).Include(e => e.CompanyPFTrust).Include(e => e.InterestPolicies).Include(e => e.InterestPolicies.InterestRate).Where(e => e.CompanyPFTrust.Id == CompantPFTrust_Id && e.PFTCalendar.Id == qurey.Id).SingleOrDefault();

        //            var PassbookLoanIDValueint = new List<string>();
        //            PassbookLoanIDValueint.Add("INTEREST POSTING");
        //            PassbookLoanIDValueint.Add("INTEREST BALANCE");
        //            List<int> PassbookIDint = new List<int>();
        //            string InterestEffectivemonth = "";
        //            string uploadmonthstr = "";
        //            DateTime uploadmonth = Convert.ToDateTime("01/" + MonthYear);
        //            uploadmonthstr = uploadmonth.ToString("MMMM").ToUpper();
        //            PassbookIDint = db.LookupValue.Where(e => PassbookLoanIDValueint.Contains(e.LookupVal.ToUpper())).Select(e => e.Id).ToList();



        //            var InterestFrequency = db.InterestPolicies.Include(x => x.StatutoryEffectiveMonthsPFT)
        //                .Include(m => m.StatutoryEffectiveMonthsPFT.Select(r => r.EffectiveMonth)).Where(e => e.Id == IntPolicyId.InterestPolicies.Id).SingleOrDefault();

        //            var EffectiveMonths = InterestFrequency.StatutoryEffectiveMonthsPFT.Select(r => r.EffectiveMonth).ToList();
        //            foreach (var item in EffectiveMonths)
        //            {
        //                if (uploadmonthstr == item.LookupVal.ToString().ToUpper())
        //                {
        //                    InterestEffectivemonth = item.LookupVal.ToString().ToUpper();
        //                }

        //            }
        //            String mPeriodRange = "";
        //            List<string> mPeriod = new List<string>();
        //            var Financialyear = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "FINANCIALYEAR".ToUpper() && e.FromDate.Value <= Dateverifyfinancialyear && e.ToDate.Value >= Dateverifyfinancialyear).SingleOrDefault();

        //            foreach (var SalaryTitem in SalaryT)
        //            {
        //                var SalaryTPF = db.SalaryT.Include(e => e.Geostruct).Include(e => e.PayStruct).Include(e => e.FuncStruct).Include(e => e.PFECRR).Where(e => e.Id == SalaryTitem).SingleOrDefault();
        //                if (SalaryTPF.PFECRR != null)
        //                {
        //                    int EmployeePayroll_Id = SalaryTPF.EmployeePayroll_Id.Value;
        //                    var oemployee = db.EmployeePayroll.Include(e => e.Employee).Where(e => e.Id == EmployeePayroll_Id).SingleOrDefault();
        //                    int Employee_Id = db.EmployeePayroll.Where(e => e.Id == EmployeePayroll_Id).Select(e => e.Employee_Id.Value).SingleOrDefault();
        //                    #region Check record existance
        //                    var PFTEmployeeLedgerCurrent = db.EmployeePFTrust.Where(e => e.Employee_Id == Employee_Id)
        //                        .Select(e => e.PFTEmployeeLedger.Where(r => r.MonthYear == MonthYear
        //                        && PassbookID.Contains(r.PassbookActivity.Id)).ToList()).SingleOrDefault();
        //                    // Last Recor PF Balance
        //                    if (PFTEmployeeLedgerCurrent != null && PFTEmployeeLedgerCurrent.Count() > 0)
        //                    {
        //                        var PFTEmployeeLedgerCurrentLast = PFTEmployeeLedgerCurrent.OrderBy(e => e.Id).LastOrDefault();
        //                        var PFbalance = db.PFTEmployeeLedger.Include(e => e.PassbookActivity).Where(e => e.Id == PFTEmployeeLedgerCurrentLast.Id).SingleOrDefault();
        //                        if (PFbalance.PassbookActivity.LookupVal.ToUpper() == "PF BALANCE")
        //                        {
        //                            var empLedgerhistory = db.EmployeePFTrust.Include(e => e.PFTEmployeeLedger).Where(e => e.Employee_Id == Employee_Id).SingleOrDefault();
        //                            var emprecord = empLedgerhistory.PFTEmployeeLedger.Where(e => e.Id >= PFTEmployeeLedgerCurrentLast.Id).ToList();
        //                            if (emprecord.Count() == 1)
        //                            {
        //                                if (PFTEmployeeLedgerCurrent != null)
        //                                {
        //                                    db.PFTEmployeeLedger.RemoveRange(PFTEmployeeLedgerCurrent);
        //                                    db.SaveChanges();
        //                                }
        //                            }
        //                            else
        //                            {
        //                                Msg.Add("PF Upload is not allowed Employee PF Trust ledger " + oemployee.Employee.EmpCode + " Next Month activity has done");

        //                                continue;
        //                            }
        //                        }

        //                    }
        //                    if (InterestEffectivemonth == uploadmonthstr)
        //                    {
        //                        var PFTEmployeeLedgerCurrentint = db.EmployeePFTrust.Where(e => e.Employee_Id == Employee_Id)
        //                   .Select(e => e.PFTEmployeeLedger.Where(r => r.MonthYear == MonthYear
        //                   && PassbookIDint.Contains(r.PassbookActivity.Id)).ToList()).SingleOrDefault();
        //                        if (PFTEmployeeLedgerCurrentint.Count() == 0)
        //                        {
        //                            Msg.Add("First Post Interest For the month " + MonthYear + " of Employee code " + oemployee.Employee.EmpCode + " Then Upload PF");

        //                            continue;
        //                        }

        //                    }

        //                    #endregion Check record existance
        //                    //previous record to fetch closing balance
        //                    var PFTEmpLedgerOldList = db.EmployeePFTrust.Where(e => e.Employee_Id == Employee_Id).Select(r => r.PFTEmployeeLedger.ToList()).SingleOrDefault();
        //                    if (PFTEmpLedgerOldList != null)
        //                    {
        //                        var PFTEmpLedgerOld = PFTEmpLedgerOldList.LastOrDefault();


        //                        double NOntaxableaccpfmonthly = 0;
        //                        double Taxableaccpfmonthly = 0;
        //                        double taxableaccpFy = 0;
        //                        double NontaxInt = 0;
        //                        double TaxableaccInt = 0;

        //                        if (IntPolicyId != null && IntPolicyId.PFTTDSMaster != null)
        //                        {
        //                            if ((IntPolicyId.PFTTDSMaster.IsOwnTDSAppl == true) && (IntPolicyId.PFTTDSMaster.IsOwnerTDSAppl == true) && (IntPolicyId.PFTTDSMaster.IsVPFTDSAppl == true))
        //                            {
        //                                NOntaxableaccpfmonthly = SalaryTPF.PFECRR.EE_Share + SalaryTPF.PFECRR.Arrear_EE_Share + SalaryTPF.PFECRR.ER_Share + SalaryTPF.PFECRR.Arrear_ER_Share + SalaryTPF.PFECRR.EE_VPF_Share;

        //                            }
        //                            if ((IntPolicyId.PFTTDSMaster.IsOwnTDSAppl == true) && (IntPolicyId.PFTTDSMaster.IsOwnerTDSAppl == true) && (IntPolicyId.PFTTDSMaster.IsVPFTDSAppl == false))
        //                            {
        //                                NOntaxableaccpfmonthly = SalaryTPF.PFECRR.EE_Share + SalaryTPF.PFECRR.Arrear_EE_Share + SalaryTPF.PFECRR.ER_Share + SalaryTPF.PFECRR.Arrear_ER_Share;

        //                            }
        //                            if ((IntPolicyId.PFTTDSMaster.IsOwnTDSAppl == true) && (IntPolicyId.PFTTDSMaster.IsVPFTDSAppl == true) && (IntPolicyId.PFTTDSMaster.IsOwnerTDSAppl == false))
        //                            {
        //                                NOntaxableaccpfmonthly = SalaryTPF.PFECRR.EE_Share + SalaryTPF.PFECRR.Arrear_EE_Share + SalaryTPF.PFECRR.EE_VPF_Share;

        //                            }
        //                            if ((IntPolicyId.PFTTDSMaster.IsOwnTDSAppl == true) && (IntPolicyId.PFTTDSMaster.IsVPFTDSAppl == false) && (IntPolicyId.PFTTDSMaster.IsOwnerTDSAppl == false))
        //                            {
        //                                NOntaxableaccpfmonthly = SalaryTPF.PFECRR.EE_Share + SalaryTPF.PFECRR.Arrear_EE_Share;

        //                            }
        //                            if ((IntPolicyId.PFTTDSMaster.IsVPFTDSAppl == true) && (IntPolicyId.PFTTDSMaster.IsOwnerTDSAppl == false) && (IntPolicyId.PFTTDSMaster.IsOwnTDSAppl == false))
        //                            {
        //                                NOntaxableaccpfmonthly = SalaryTPF.PFECRR.EE_VPF_Share;

        //                            }
        //                            if ((IntPolicyId.PFTTDSMaster.IsOwnerTDSAppl == true) && (IntPolicyId.PFTTDSMaster.IsOwnTDSAppl == false) && (IntPolicyId.PFTTDSMaster.IsVPFTDSAppl == false))
        //                            {
        //                                NOntaxableaccpfmonthly = SalaryTPF.PFECRR.ER_Share + SalaryTPF.PFECRR.Arrear_ER_Share;

        //                            }
        //                            if (IntPolicyId.PFTTDSMaster.IsOwnTDSAppl == false && IntPolicyId.PFTTDSMaster.IsOwnerTDSAppl == false && IntPolicyId.PFTTDSMaster.IsVPFTDSAppl == false)
        //                            {
        //                                NOntaxableaccpfmonthly = 0;

        //                            }
        //                            var intrateeff = IntPolicyId.InterestPolicies.InterestRate.FirstOrDefault();

        //                            ReturnData = InterestRate(IntPolicyId.Id, intrateeff.EffectiveFrom, 0, 0);

        //                            if (PFTEmpLedgerOld != null)
        //                            {
        //                                if (Dateverifyfinancialyear == Financialyear.FromDate.Value.Date)
        //                                {
        //                                    // PFTEmpLedgerOld.AccuNonTaxableAccountFy_Closingbalance financial year start closeing balance 0
        //                                    if ((NOntaxableaccpfmonthly) > IntPolicyId.PFTTDSMaster.TaxableAccountCelling)
        //                                    {
        //                                        Taxableaccpfmonthly = NOntaxableaccpfmonthly - IntPolicyId.PFTTDSMaster.TaxableAccountCelling;
        //                                        NOntaxableaccpfmonthly = NOntaxableaccpfmonthly - Taxableaccpfmonthly;
        //                                    }

        //                                }

        //                                if (Dateverifyfinancialyear > Financialyear.FromDate.Value.Date && Dateverifyfinancialyear <= Financialyear.ToDate.Value.Date)
        //                                {
        //                                    if ((PFTEmpLedgerOld.AccuNonTaxableAccountFy_Closingbalance + NOntaxableaccpfmonthly) > IntPolicyId.PFTTDSMaster.TaxableAccountCelling)
        //                                    {
        //                                        Taxableaccpfmonthly = PFTEmpLedgerOld.AccuNonTaxableAccountFy_Closingbalance + NOntaxableaccpfmonthly - IntPolicyId.PFTTDSMaster.TaxableAccountCelling;
        //                                        NOntaxableaccpfmonthly = NOntaxableaccpfmonthly - Taxableaccpfmonthly;
        //                                    }
        //                                }

        //                            }
        //                            NontaxInt = (NOntaxableaccpfmonthly * (ReturnData.ReturnValue / (12 * 100)));
        //                            NontaxInt = Math.Round(NontaxInt + 0.001, 0);
        //                            TaxableaccInt = (Taxableaccpfmonthly * (ReturnData.ReturnValue / (12 * 100)));
        //                            TaxableaccInt = Math.Round(TaxableaccInt + 0.001, 0);

        //                            //Taxableaccpfmonthly=PFTEmpLedgerOld != null ? iif(PFTEmpLedgerOld.AccuNonTaxableAccountClosingbalance+NOntaxableaccpfmonthly>IntPolicyId.PFTTDSMaster.TaxableAccountCelling) : 0


        //                        }

        //                        //collection of PFECR data
        //                        var PFTEmpLedgerList = new List<PFTEmployeeLedger>();
        //                        //Create new ECR Data object to dump in PFLedger
        //                        var PFTEmpLedgerECRData = new PFTEmployeeLedger()
        //                        {
        //                            GeoStruct = SalaryTPF.Geostruct,
        //                            PayStruct = SalaryTPF.PayStruct,
        //                            FuncStruct = SalaryTPF.FuncStruct,
        //                            MonthYear = SalaryTPF.PayMonth,
        //                            PassbookActivity = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "512").FirstOrDefault().LookupValues.Where(e => e.LookupVal.ToUpper() == "MONTHLY PF POSTING").SingleOrDefault(),
        //                            PostingDate = DateTime.Now.Date,
        //                            CalcDate = Convert.ToDateTime(SalaryTPF.PayMonth).Date,//SalaryTPF.PaymentDate.Value, //SalaryTPF.PaymentDate null in table

        //                            #region Input Data for ledger
        //                            OwnPFMonthly = SalaryTPF.PFECRR.EE_Share + SalaryTPF.PFECRR.Arrear_EE_Share,
        //                            OwnerPFMonthly = SalaryTPF.PFECRR.ER_Share + SalaryTPF.PFECRR.Arrear_ER_Share,
        //                            VPFAmountMonthly = SalaryTPF.PFECRR.EE_VPF_Share,
        //                            PensionAmount = SalaryTPF.PFECRR.EPS_Share + SalaryTPF.PFECRR.Arrear_EPS_Share,
        //                            PFWages = SalaryTPF.PFECRR.EPF_Wages + SalaryTPF.PFECRR.Arrear_EPF_Wages,
        //                            #endregion Input Data for ledger

        //                            IsPassbookClose = false,
        //                            Narration = "PF Posting For Month" + SalaryTPF.PayMonth.ToString(),
        //                            InterestFrequency = null,
        //                            DBTrack = DBTrack,
        //                            AccuNonTaxableAccountFy_Closingbalance = NOntaxableaccpfmonthly,
        //                            NonTaxableAccountMonthly = NOntaxableaccpfmonthly,
        //                            NonTaxableAccountInterest = NontaxInt,
        //                            TaxableAccountFy_Closingbalance = Taxableaccpfmonthly,
        //                            TaxableAccountMonthly = Taxableaccpfmonthly,
        //                            TaxableAccountInterest = TaxableaccInt,
        //                        };
        //                        PFTEmpLedgerList.Add(PFTEmpLedgerECRData);//added monthly PF data
        //                        //Create new ECRData Balance pftlegder object

        //                        double AccuNonTaxableAccountFy_Closingbalancepost = 0;
        //                        double AccuNonTaxableAccountFy_Openingbalancepost = 0;
        //                        double TaxableAccountFy_Closingbalancepost = 0;
        //                        double TaxableAccountFy_Openingbalancepost = 0;
        //                        double NonTaxableAccountInterestpost = 0;
        //                        double TaxableAccountInterestpost = 0;
        //                        if (Dateverifyfinancialyear == Financialyear.FromDate.Value.Date)
        //                        {
        //                            AccuNonTaxableAccountFy_Closingbalancepost = NOntaxableaccpfmonthly;
        //                            AccuNonTaxableAccountFy_Openingbalancepost = 0;

        //                            TaxableAccountFy_Closingbalancepost = Taxableaccpfmonthly;
        //                            TaxableAccountFy_Openingbalancepost = 0;

        //                            NonTaxableAccountInterestpost = NontaxInt;
        //                            TaxableAccountInterestpost = TaxableaccInt;


        //                        }


        //                        if (Dateverifyfinancialyear > Financialyear.FromDate.Value.Date && Dateverifyfinancialyear <= Financialyear.ToDate.Value.Date)
        //                        {
        //                            AccuNonTaxableAccountFy_Closingbalancepost = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.AccuNonTaxableAccountFy_Closingbalance : 0) + NOntaxableaccpfmonthly;//check taxableaccpFy
        //                            AccuNonTaxableAccountFy_Openingbalancepost = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.AccuNonTaxableAccountFy_Closingbalance : 0);
        //                            if (IntPolicyId.PFTTDSMaster != null)
        //                            {
        //                                if (AccuNonTaxableAccountFy_Closingbalancepost > IntPolicyId.PFTTDSMaster.TaxableAccountCelling)
        //                                {
        //                                    AccuNonTaxableAccountFy_Closingbalancepost = IntPolicyId.PFTTDSMaster.TaxableAccountCelling;
        //                                }
        //                            }

        //                            TaxableAccountFy_Closingbalancepost = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.TaxableAccountFy_Closingbalance : 0) + Taxableaccpfmonthly;
        //                            TaxableAccountFy_Openingbalancepost = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.TaxableAccountFy_Closingbalance : 0);


        //                            NonTaxableAccountInterestpost = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.NonTaxableAccountInterest : 0) + NontaxInt;
        //                            TaxableAccountInterestpost = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.TaxableAccountInterest : 0) + TaxableaccInt;

        //                        }





        //                        var PFTEmpLedgerECRDataBalance = new PFTEmployeeLedger()
        //                        {
        //                            GeoStruct = SalaryTPF.Geostruct,
        //                            PayStruct = SalaryTPF.PayStruct,
        //                            FuncStruct = SalaryTPF.FuncStruct,
        //                            MonthYear = SalaryTPF.PayMonth,
        //                            PassbookActivity = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "512").FirstOrDefault().LookupValues.Where(e => e.LookupVal.ToUpper() == "PF BALANCE").SingleOrDefault(),
        //                            PostingDate = DateTime.Now.Date,
        //                            CalcDate = Convert.ToDateTime(SalaryTPF.PayMonth).Date,//SalaryTPF.PaymentDate.Value, //SalaryTPF.PaymentDate null in table

        //                            #region Input Data for ledger
        //                            //OwnPFMonthly = SalaryTPF.PFECRR.EE_Share + SalaryTPF.PFECRR.Arrear_EE_Share,
        //                            //OwnerPFMonthly = SalaryTPF.PFECRR.ER_Share + SalaryTPF.PFECRR.Arrear_ER_Share,
        //                            //VPFAmountMonthly = SalaryTPF.PFECRR.EE_VPF_Share,
        //                            //PensionAmount = SalaryTPF.PFECRR.EPS_Share + SalaryTPF.PFECRR.Arrear_EPS_Share,
        //                            //PFWages = SalaryTPF.PFECRR.EPF_Wages + SalaryTPF.PFECRR.Arrear_EPF_Wages,
        //                            LoanAmountCredit = 0,
        //                            LoanAmountDebit = 0,
        //                            OwnPFInt = 0,
        //                            OwnerPFInt = 0,
        //                            VPFInt = 0,
        //                            PFInt = 0,
        //                            #endregion Input Data for ledger

        //                            OwnOpenBal = PFTEmpLedgerOld != null ? PFTEmpLedgerOld.OwnCloseBal : 0,
        //                            OwnCloseBal = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.OwnCloseBal : 0) + SalaryTPF.PFECRR.EE_Share + SalaryTPF.PFECRR.Arrear_EE_Share,
        //                            OwnerOpenBal = PFTEmpLedgerOld != null ? PFTEmpLedgerOld.OwnerCloseBal : 0,
        //                            OwnerCloseBal = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.OwnerCloseBal : 0) + SalaryTPF.PFECRR.ER_Share + SalaryTPF.PFECRR.Arrear_ER_Share,
        //                            VPFOpenBal = PFTEmpLedgerOld != null ? PFTEmpLedgerOld.VPFCloseBal : 0,
        //                            VPFCloseBal = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.VPFCloseBal : 0) + SalaryTPF.PFECRR.EE_VPF_Share,
        //                            PFOpenBal = PFTEmpLedgerOld != null ? PFTEmpLedgerOld.PFCloseBal : 0,
        //                            PFCloseBal = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.PFCloseBal : 0) + SalaryTPF.PFECRR.EE_Share + SalaryTPF.PFECRR.ER_Share + SalaryTPF.PFECRR.Arrear_EE_Share + SalaryTPF.PFECRR.Arrear_ER_Share,

        //                            OwnIntOpenBal = PFTEmpLedgerOld != null ? PFTEmpLedgerOld.OwnIntCloseBal : 0,
        //                            OwnIntCloseBal = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.OwnIntCloseBal : 0),
        //                            OwnerIntOpenBal = PFTEmpLedgerOld != null ? PFTEmpLedgerOld.OwnerIntCloseBal : 0,
        //                            OwnerIntCloseBal = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.OwnerIntCloseBal : 0),
        //                            VPFIntOpenBal = PFTEmpLedgerOld != null ? PFTEmpLedgerOld.VPFIntCloseBal : 0,
        //                            VPFIntCloseBal = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.VPFIntCloseBal : 0),
        //                            PFIntOpenBal = PFTEmpLedgerOld != null ? PFTEmpLedgerOld.PFIntCloseBal : 0,
        //                            PFIntCloseBal = PFTEmpLedgerOld != null ? PFTEmpLedgerOld.PFIntCloseBal : 0,



        //                            OwnPFLoan = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.OwnPFLoan : 0),
        //                            OwnerPFLoan = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.OwnerPFLoan : 0),
        //                            VPFPFLoan = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.VPFPFLoan : 0),

        //                            IntOnInt = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.IntOnInt : 0),
        //                            IntonIntOpenBal = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.IntonIntCloseBal : 0),
        //                            IntonIntCloseBal = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.IntonIntCloseBal : 0),
        //                            IntOnIntPFLoan = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.IntOnIntPFLoan : 0),
        //                            OwnIntPFLoan = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.OwnIntPFLoan : 0),
        //                            OwnerIntPFLoan = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.OwnerIntPFLoan : 0),
        //                            VPFIntPFLoan = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.VPFIntPFLoan : 0),
        //                            TotalIntOpenBal = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.TotalIntCloseBal : 0),
        //                            TotalIntCloseBal = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.TotalIntCloseBal : 0),


        //                            IsPassbookClose = false,
        //                            Narration = "PF Balance For Month" + SalaryTPF.PayMonth.ToString(),
        //                            InterestFrequency = null,
        //                            DBTrack = DBTrack,
        //                            AccuNonTaxableAccountOpeningbalance = PFTEmpLedgerOld != null ? PFTEmpLedgerOld.AccuNonTaxableAccountClosingbalance : 0,
        //                            AccuNonTaxableAccountClosingbalance = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.AccuNonTaxableAccountClosingbalance : 0) + NOntaxableaccpfmonthly,
        //                            AccTaxableAccountOpeningbalance = PFTEmpLedgerOld != null ? PFTEmpLedgerOld.AccTaxableAccountClosingbalance : 0,
        //                            AccTaxableAccountClosingbalance = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.AccTaxableAccountClosingbalance : 0) + Taxableaccpfmonthly,

        //                            AccuNonTaxableAccountFy_Openingbalance = AccuNonTaxableAccountFy_Openingbalancepost,//PFTEmpLedgerOld != null ? PFTEmpLedgerOld.AccuNonTaxableAccountFy_Closingbalance : 0,
        //                            AccuNonTaxableAccountFy_Closingbalance = AccuNonTaxableAccountFy_Closingbalancepost,//check taxableaccpFy
        //                            TaxableAccountFy_Openingbalance = TaxableAccountFy_Openingbalancepost,//PFTEmpLedgerOld != null ? PFTEmpLedgerOld.TaxableAccountFy_Closingbalance : 0,
        //                            TaxableAccountFy_Closingbalance = TaxableAccountFy_Closingbalancepost,//(PFTEmpLedgerOld != null ? PFTEmpLedgerOld.TaxableAccountFy_Closingbalance : 0) + Taxableaccpfmonthly,

        //                            NonTaxableAccountInterest = NonTaxableAccountInterestpost,//(PFTEmpLedgerOld != null ? PFTEmpLedgerOld.NonTaxableAccountInterest : 0) + NontaxInt,
        //                            TaxableAccountInterest = TaxableAccountInterestpost,//(PFTEmpLedgerOld != null ? PFTEmpLedgerOld.TaxableAccountInterest : 0) + TaxableaccInt,


        //                        };
        //                        PFTEmpLedgerList.Add(PFTEmpLedgerECRDataBalance);//added monthly PF data balance
        //                        //var PFTEmployeeLedgerAdd = db.PFTEmployeeLedger.Where(e => == Employee_Id).SingleOrDefault();
        //                        var EmployeePFTrust = db.EmployeePFTrust.Include(e => e.PFTEmployeeLedger).Where(e => e.Employee_Id == Employee_Id).SingleOrDefault();
        //                        // EmployeePFTrust.PFTEmployeeLedger.ToList().AddRange(PFTEmpLedgerList);
        //                        PFTEmpLedgerList.AddRange(EmployeePFTrust.PFTEmployeeLedger);
        //                        EmployeePFTrust.PFTEmployeeLedger = PFTEmpLedgerList;
        //                        db.EmployeePFTrust.Attach(EmployeePFTrust);
        //                        db.Entry(EmployeePFTrust).State = System.Data.Entity.EntityState.Modified;
        //                        db.SaveChanges();
        //                        db.Entry(EmployeePFTrust).State = System.Data.Entity.EntityState.Detached;
        //                        EmpCount++;
        //                    }

        //                }

        //            }
        //            //ErrorLog.Errno = 0; //Holiday calendar not defined
        //            //ErrorLog.ErrMsg = "PF Sucessfully Uploaded to PF Trust for Employees " + EmpCount.ToString() + " On Month " + MonthYear;
        //            //return ErrorLog;
        //        }
        //        Msg.Add("PF Sucessfully Uploaded to PF Trust total Employees count " + EmpCount.ToString() + " On Month " + MonthYear);

        //    }
        //    catch (Exception ex)
        //    {
        //        Msg.Add("ProcessObjectName = " + ex.Source.ToString() + " Inner ExceptionMessage = " + ex.InnerException.Message + " ExceptionMessage = " + ex.Message + " ExceptionStackTrace = " + ex.StackTrace + "LineNo = " + Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()) + "LogTime = " + DateTime.Now);
        //        //ErrorLog.Errno = 1; //Holiday calendar not defined
        //        //ErrorLog.ErrMsg = "ProcessObjectName = " + ex.Source.ToString() + " Inner ExceptionMessage = " + ex.InnerException.Message + " ExceptionMessage = " + ex.Message + " ExceptionStackTrace = " + ex.StackTrace + "LineNo = " + Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()) + "LogTime = " + DateTime.Now;
        //        //return ErrorLog;
        //    };
        //    return Msg;
        //}24022024
        #endregion PFECR data upload from Payroll Module into PFTrust module

        #region PFECR Projection data upload from Payroll Module into PFTrust module
        public static List<string> UploadECRPro(DateTime? SettlementDate, int EmployeePFT_Id)
        {
            //ReturnData ErrorLog = new ReturnData();
            List<string> MsgP = new List<string>();
            string MonthYear = "";
            try
            {
                var EmpCount = 0;

                using (DataBaseContext db = new DataBaseContext())
                {
                    // projected EPF and CPF
                    var PassbookLoanIDValuemonthlypf = new List<string>();
                    PassbookLoanIDValuemonthlypf.Add("Monthly PF Posting");
                    List<int> PassbookIDmonthlypf = new List<int>();
                    PassbookIDmonthlypf = db.LookupValue.Where(e => PassbookLoanIDValuemonthlypf.Contains(e.LookupVal.ToUpper())).Select(e => e.Id).ToList();

                    double mEmpVPF = 0;
                    double mEmpPF = 0;
                    double mEmpPension = 0;
                    double highpension = 0;
                    double highpensionper = 0;
                    double mCompPF = 0;
                    double mPFWages = 0;
                    var PFTEmployeeLedgerCurrent = db.EmployeePFTrust.Where(e => e.Id == EmployeePFT_Id)
                            .Select(e => e.PFTEmployeeLedger.Where(r => PassbookIDmonthlypf.Contains(r.PassbookActivity.Id)).ToList()).SingleOrDefault();
                    var PFTEmployeeLedgerCurrentLast = PFTEmployeeLedgerCurrent.OrderByDescending(e => e.Id).FirstOrDefault();
                    DateTime? PFdatatilldate = DateTime.Now;



                    DateTime? PFdataStartdate;
                    var Oemployeeret = db.EmployeePFTrust.Include(e => e.Employee).Include(e => e.Employee.ServiceBookDates).Include(e => e.Employee.EmpOffInfo).Include(e => e.Employee.EmpOffInfo.NationalityID).Where(e => e.Id == EmployeePFT_Id).SingleOrDefault();
                    if (PFTEmployeeLedgerCurrentLast == null)
                    {
                        MsgP.Add("Monthly PF Posting not availble For " + Oemployeeret.Employee.EmpCode);
                        return MsgP;
                    }
                    if (Oemployeeret != null)
                    {
                        if (Oemployeeret.Employee.ServiceBookDates.RetirementDate.Value.Date < SettlementDate)
                        {
                            PFdatatilldate = Oemployeeret.Employee.ServiceBookDates.RetirementDate.Value.Date;
                        }
                        else
                        {
                            PFdatatilldate = SettlementDate;
                        }
                        string uploadedmonth = PFTEmployeeLedgerCurrentLast.CalcDate.ToString("MM/yyyy");
                        string tillmonth = PFdatatilldate.Value.ToString("MM/yyyy");


                        double mAge = 0;
                        var mDateofBirth = Oemployeeret.Employee.ServiceBookDates.BirthDate;// db.Employee.Where(e => e.Id == mEmployeeID).Select(e => e.ServiceBookDates.BirthDate);
                        DateTime start = mDateofBirth.Value;
                        DateTime end = Convert.ToDateTime("01/" + tillmonth).AddMonths(1).AddDays(-1).Date;// DateTime.Now.Date;
                        int compMonth = (end.Month + end.Year * 12) - (start.Month + start.Year * 12);
                        //   double daysInEndMonth = (end - end.AddMonths(1)).Days;
                        double daysInEndMonth = (end.AddMonths(1) - end).Days;
                        double months = compMonth + (start.Day - end.Day) / daysInEndMonth;
                        mAge = months / 12;



                        if (tillmonth != uploadedmonth)
                        {
                            var OEmployeePayroll = db.EmployeePayroll.Where(e => e.Employee_Id == Oemployeeret.Employee.Id).SingleOrDefault();
                            EmpSalStruct OSalCurrentStruct = db.EmpSalStruct.Where(e => e.EmployeePayroll_Id == OEmployeePayroll.Id && e.EndDate == null).SingleOrDefault();
                            if (OSalCurrentStruct != null)
                            {


                                List<EmpSalStructDetails> EmpSalStructDetails = db.EmpSalStructDetails.Where(e => e.EmpSalStruct_Id == OSalCurrentStruct.Id).ToList();

                                foreach (var EmpSalStructDetailsItem in EmpSalStructDetails)
                                {
                                    //var SalHeadTmp = new SalaryHead();
                                    PayScaleAssignment PayScaleAssignmentObj = db.PayScaleAssignment.Where(e => e.Id == EmpSalStructDetailsItem.PayScaleAssignment_Id).FirstOrDefault();
                                    EmpSalStructDetailsItem.PayScaleAssignment = PayScaleAssignmentObj;
                                    SalHeadFormula SalaryHeadFormulaObj = db.SalHeadFormula.Where(e => e.Id == EmpSalStructDetailsItem.SalHeadFormula_Id).FirstOrDefault();
                                    EmpSalStructDetailsItem.SalHeadFormula = SalaryHeadFormulaObj;


                                    SalaryHead SalaryHead = db.SalaryHead.Where(e => e.Id == EmpSalStructDetailsItem.SalaryHead_Id).SingleOrDefault();
                                    EmpSalStructDetailsItem.SalaryHead = SalaryHead;
                                    LookupValue SalHeadOperationType = db.LookupValue.Where(e => e.Id == SalaryHead.SalHeadOperationType_Id).SingleOrDefault();
                                    SalaryHead.SalHeadOperationType = SalHeadOperationType;
                                    LookupValue Type = db.LookupValue.Where(e => e.Id == SalaryHead.Type_Id).SingleOrDefault();
                                    SalaryHead.Type = Type;
                                    LookupValue Frequency = db.LookupValue.Where(e => e.Id == SalaryHead.Frequency_Id).SingleOrDefault();
                                    SalaryHead.Frequency = Frequency;
                                    LookupValue ProcessType = db.LookupValue.Where(e => e.Id == SalaryHead.ProcessType_Id).SingleOrDefault();
                                    SalaryHead.ProcessType = ProcessType;
                                    LookupValue RoundingMethod = db.LookupValue.Where(e => e.Id == SalaryHead.RoundingMethod_Id).SingleOrDefault();
                                    SalaryHead.RoundingMethod = RoundingMethod;




                                }
                            }

                            if (OSalCurrentStruct != null) //no sal structure in employee left condition 
                            {

                                List<EmpSalStructDetails> OSalaryHeadTotalProjected = OSalCurrentStruct.EmpSalStructDetails
                                .Where(v => v.SalaryHead.InITax == true
                                && v.SalaryHead.Frequency.LookupVal.ToUpper() == "MONTHLY")
                                .ToList();
                                foreach (EmpSalStructDetails ca in OSalaryHeadTotalProjected.Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "EPF"))
                                {
                                    if (ca.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() == "EPF")
                                    {
                                        var Comp_Id = Convert.ToInt32(SessionManager.CompanyId);

                                        CompanyPayroll OCompanyPayroll = db.CompanyPayroll.Include(e => e.PFMaster)
                                            .AsNoTracking()
                                            .Where(e => e.Company.Id == Comp_Id).SingleOrDefault();

                                        List<SalEarnDedT> OSalEarnDedT = null;
                                        PFMaster OCompPFMaster = OCompanyPayroll.PFMaster.Where(e => e.EndDate == null).SingleOrDefault();

                                        var OPFMaster = db.PFMaster.Where(e => e.Id == OCompPFMaster.Id)
                                          .Include(e => e.EPFWages.RateMaster.Select(r => r.SalHead)
                                          ).SingleOrDefault();

                                        var OPFMaster1 = db.PFMaster.Where(e => e.Id == OCompPFMaster.Id)
                                         .Include(e => e.EPSWages.RateMaster.Select(r => r.SalHead)
                                         ).SingleOrDefault();

                                        mPFWages = Process.SalaryHeadGenProcess.WagecalcDirect(OPFMaster.EPFWages, OSalEarnDedT, OSalaryHeadTotalProjected);
                                        mPFWages = Math.Round(mPFWages, 0);

                                        double mPensionWages = Process.SalaryHeadGenProcess.WagecalcDirect(OPFMaster1.EPSWages, OSalEarnDedT, OSalaryHeadTotalProjected);
                                        mPensionWages = Math.Round(mPensionWages, 0);

                                        mEmpPF = Math.Round(mPFWages * OPFMaster.EmpPFPerc / 100, 0);
                                        mEmpPF = Math.Round(mEmpPF, 0);

                                        if (compMonth == OPFMaster.PensionAge * 12 && start.Month == end.Month)
                                        {
                                            mAge = compMonth / 12;
                                        }

                                        if (mAge <= OPFMaster.PensionAge)
                                        {
                                            if (OEmployeePayroll.Employee.EmpOffInfo.NationalityID.HigherPension == true)
                                            {
                                                if ((mPFWages - mPensionWages) > 0)
                                                {
                                                    mEmpPension = Math.Round(mPFWages * OPFMaster.EPSPerc / 100, 0);
                                                    string highpensionperval = OEmployeePayroll.Employee.EmpOffInfo.NationalityID.HigherPensionPer == null ? "0" : OEmployeePayroll.Employee.EmpOffInfo.NationalityID.HigherPensionPer.ToString();
                                                    highpensionper = Convert.ToDouble(highpensionperval);
                                                    highpension = Math.Round((mPFWages - mPensionWages) * highpensionper / 100, 0);
                                                    mEmpPension = mEmpPension + highpension;
                                                    mPensionWages = mPFWages;
                                                }
                                                else
                                                {
                                                    mEmpPension = Math.Round(mPensionWages * OPFMaster.EPSPerc / 100, 0);
                                                }

                                            }
                                            else
                                            {
                                                mEmpPension = Math.Round(mPensionWages * OPFMaster.EPSPerc / 100, 0);
                                            }

                                        }
                                        else
                                        {
                                            mEmpPension = 0;
                                            mPensionWages = 0;
                                        }

                                        mCompPF = mEmpPF - mEmpPension;





                                    }
                                }

                                mEmpVPF = OSalCurrentStruct.EmpSalStructDetails.Where(e => e.SalaryHead.Code.ToUpper() == "VPF").SingleOrDefault() != null ?
                             PFTEmployeeLedgerCurrentLast.VPFAmountMonthly : 0;
                                // vpf amount calc start
                                var OSalaryHeadvppf = OSalCurrentStruct.EmpSalStructDetails.Where(e => e.SalaryHead.Code.ToUpper() == "VPF")
                              .SingleOrDefault();
                                if (OSalaryHeadvppf != null)
                                {
                                    if (OSalaryHeadvppf.SalHeadFormula != null)
                                    {
                                        mEmpVPF = SalaryHeadGenProcess.SalHeadAmountCalc(OSalaryHeadvppf.SalHeadFormula.Id, null, OSalaryHeadTotalProjected, OEmployeePayroll,
                                                tillmonth, OSalaryHeadvppf.SalaryHead.Code.ToUpper() == "VPF" ? true : false);
                                        mEmpVPF = SalaryHeadGenProcess.RoundingFunction(OSalaryHeadvppf.SalaryHead, mEmpVPF);


                                    }
                                }

                            }

                        }

                    }

                    // Projected EPF and CPF

                    DBTrack DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                    //var SalaryT = db.SalaryT.Where(e => e.PayMonth == MonthYear && e.ReleaseDate != null && e.EmployeePayroll_Id == 839).Select(e => e.Id).ToList();


                    var PassbookLoanIDValue = new List<string>();
                    //PassbookLoanIDValue[0] = "Monthly PF Posting";
                    //PassbookLoanIDValue[1] = "PF Balance";
                    PassbookLoanIDValue.Add("Monthly PF Posting");
                    PassbookLoanIDValue.Add("PF Balance");
                    List<int> PassbookID = new List<int>();
                    PassbookID = db.LookupValue.Where(e => PassbookLoanIDValue.Contains(e.LookupVal.ToUpper())).Select(e => e.Id).ToList();

                    var Id = Convert.ToInt32(SessionManager.CompanyId);
                    var CompCodeid = db.Company.Where(e => e.Id == Id).SingleOrDefault();
                    var qurey = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "PFCALENDAR".ToUpper() && e.Default == true).AsEnumerable()
                      .Select(e => new
                      {
                          Id = e.Id,
                          Lvcalendardesc = "FromDate :" + e.FromDate.Value.ToShortDateString() + " ToDate :" + e.ToDate.Value.ToShortDateString(),
                          FromDate = e.FromDate.Value.ToShortDateString(),
                          ToDate = e.ToDate.Value.ToShortDateString()

                      }).SingleOrDefault();



                    ReturnData ReturnData = new ReturnData();
                    var CompantPFTrust_Id = db.CompanyPFTrust.Where(e => e.Company_Id == CompCodeid.Id).Select(e => e.Id).SingleOrDefault();
                    var IntPolicyId = db.PFTACCalendar.Include(e => e.PFTTDSMaster).Include(e => e.CompanyPFTrust).Include(e => e.InterestPolicies).Include(e => e.InterestPolicies.InterestRate).Where(e => e.CompanyPFTrust.Id == CompantPFTrust_Id && e.PFTCalendar.Id == qurey.Id).SingleOrDefault();

                    var PassbookLoanIDValueint = new List<string>();
                    PassbookLoanIDValueint.Add("INTEREST POSTING");
                    PassbookLoanIDValueint.Add("INTEREST BALANCE");
                    List<int> PassbookIDint = new List<int>();

                    PassbookIDint = db.LookupValue.Where(e => PassbookLoanIDValueint.Contains(e.LookupVal.ToUpper())).Select(e => e.Id).ToList();




                    String mPeriodRange = "";
                    List<string> mPeriod = new List<string>();
                    DateTime Startuploaddt = PFTEmployeeLedgerCurrentLast.CalcDate.AddMonths(1);
                    // foreach (var SalaryTitem in SalaryT)
                    //DateTime penddate=PFdatatilldate!=null? PFdatatilldate.Value.Date : DateTime.Now.Date;

                    for (DateTime mTempDate = Startuploaddt; mTempDate <= PFdatatilldate; mTempDate = mTempDate.AddMonths(1))
                    {
                        string Mpaymonth = Convert.ToDateTime(mTempDate).ToString("MM/yyyy");
                        string InterestEffectivemonth = "";
                        string uploadmonthstr = "";
                        DateTime uploadmonth = Convert.ToDateTime("01/" + Mpaymonth);
                        uploadmonthstr = uploadmonth.ToString("MMMM").ToUpper();

                        var InterestFrequency = db.InterestPolicies.Include(x => x.StatutoryEffectiveMonthsPFT)
                      .Include(m => m.StatutoryEffectiveMonthsPFT.Select(r => r.EffectiveMonth)).Where(e => e.Id == IntPolicyId.InterestPolicies.Id).SingleOrDefault();

                        var EffectiveMonths = InterestFrequency.StatutoryEffectiveMonthsPFT.Select(r => r.EffectiveMonth).ToList();
                        foreach (var item in EffectiveMonths)
                        {
                            if (uploadmonthstr == item.LookupVal.ToString().ToUpper())
                            {
                                InterestEffectivemonth = item.LookupVal.ToString().ToUpper();
                            }

                        }


                        DateTime Dateverifyfinancialyear = Convert.ToDateTime("01/" + Mpaymonth);
                        var Financialyear = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "FINANCIALYEAR".ToUpper() && e.FromDate.Value <= Dateverifyfinancialyear && e.ToDate.Value >= Dateverifyfinancialyear).SingleOrDefault();


                        var emppft = db.EmployeePFTrust.Include(e => e.Employee).Where(e => e.Id == EmployeePFT_Id).SingleOrDefault();
                        var SalaryTPF = db.Employee.Include(e => e.GeoStruct).Include(e => e.PayStruct).Include(e => e.FuncStruct).Where(e => e.Id == emppft.Employee.Id).SingleOrDefault();

                        // var SalaryTPF = db.SalaryT.Include(e => e.Geostruct).Include(e => e.PayStruct).Include(e => e.FuncStruct).Include(e => e.PFECRR).Where(e => e.Id == SalaryTitem).SingleOrDefault();
                        if (SalaryTPF != null)
                        {
                            //int EmployeePayroll_Id = SalaryTPF.EmployeePayroll_Id.Value;
                            var oemployee = db.EmployeePayroll.Include(e => e.Employee).Where(e => e.Id == SalaryTPF.Id).SingleOrDefault();
                            int Employee_Id = db.EmployeePayroll.Where(e => e.Id == oemployee.Id).Select(e => e.Employee_Id.Value).SingleOrDefault();
                            #region Check record existance
                            //var PFTEmployeeLedgerCurrent = db.EmployeePFTrust.Where(e => e.Employee_Id == Employee_Id)
                            //    .Select(e => e.PFTEmployeeLedger.Where(r => r.MonthYear == MonthYear
                            //    && PassbookID.Contains(r.PassbookActivity.Id)).ToList()).SingleOrDefault();
                            // Last Recor PF Balance


                            #endregion Check record existance
                            //previous record to fetch closing balance
                            var PFTEmpLedgerOldList = db.EmployeePFTrust.Where(e => e.Employee_Id == Employee_Id).Select(r => r.EmpSettlementPFT.ToList()).SingleOrDefault();
                            if (PFTEmpLedgerOldList != null)
                            {
                                var PFTEmpLedgerOld = PFTEmpLedgerOldList.OrderBy(e => e.Id).LastOrDefault();


                                double NOntaxableaccpfmonthly = 0;
                                double Taxableaccpfmonthly = 0;
                                double taxableaccpFy = 0;
                                double NontaxInt = 0;
                                double TaxableaccInt = 0;

                                if (IntPolicyId != null && IntPolicyId.PFTTDSMaster != null)
                                {
                                    if ((IntPolicyId.PFTTDSMaster.IsOwnTDSAppl == true) && (IntPolicyId.PFTTDSMaster.IsOwnerTDSAppl == true) && (IntPolicyId.PFTTDSMaster.IsVPFTDSAppl == true))
                                    {
                                        // NOntaxableaccpfmonthly = SalaryTPF.PFECRR.EE_Share + SalaryTPF.PFECRR.Arrear_EE_Share + SalaryTPF.PFECRR.ER_Share + SalaryTPF.PFECRR.Arrear_ER_Share + SalaryTPF.PFECRR.EE_VPF_Share;
                                        NOntaxableaccpfmonthly = mEmpPF + mCompPF + mEmpVPF;


                                    }
                                    if ((IntPolicyId.PFTTDSMaster.IsOwnTDSAppl == true) && (IntPolicyId.PFTTDSMaster.IsOwnerTDSAppl == true) && (IntPolicyId.PFTTDSMaster.IsVPFTDSAppl == false))
                                    {
                                        // NOntaxableaccpfmonthly = SalaryTPF.PFECRR.EE_Share + SalaryTPF.PFECRR.Arrear_EE_Share + SalaryTPF.PFECRR.ER_Share + SalaryTPF.PFECRR.Arrear_ER_Share;
                                        NOntaxableaccpfmonthly = mEmpPF + mCompPF;

                                    }
                                    if ((IntPolicyId.PFTTDSMaster.IsOwnTDSAppl == true) && (IntPolicyId.PFTTDSMaster.IsVPFTDSAppl == true) && (IntPolicyId.PFTTDSMaster.IsOwnerTDSAppl == false))
                                    {
                                        //    NOntaxableaccpfmonthly = SalaryTPF.PFECRR.EE_Share + SalaryTPF.PFECRR.Arrear_EE_Share + SalaryTPF.PFECRR.EE_VPF_Share;
                                        NOntaxableaccpfmonthly = mEmpPF + mEmpVPF;

                                    }
                                    if ((IntPolicyId.PFTTDSMaster.IsOwnTDSAppl == true) && (IntPolicyId.PFTTDSMaster.IsVPFTDSAppl == false) && (IntPolicyId.PFTTDSMaster.IsOwnerTDSAppl == false))
                                    {
                                        // NOntaxableaccpfmonthly = SalaryTPF.PFECRR.EE_Share + SalaryTPF.PFECRR.Arrear_EE_Share;
                                        NOntaxableaccpfmonthly = mEmpPF;

                                    }
                                    if ((IntPolicyId.PFTTDSMaster.IsVPFTDSAppl == true) && (IntPolicyId.PFTTDSMaster.IsOwnerTDSAppl == false) && (IntPolicyId.PFTTDSMaster.IsOwnTDSAppl == false))
                                    {
                                        // NOntaxableaccpfmonthly = SalaryTPF.PFECRR.EE_VPF_Share;
                                        NOntaxableaccpfmonthly = mEmpVPF;

                                    }
                                    if ((IntPolicyId.PFTTDSMaster.IsOwnerTDSAppl == true) && (IntPolicyId.PFTTDSMaster.IsOwnTDSAppl == false) && (IntPolicyId.PFTTDSMaster.IsVPFTDSAppl == false))
                                    {
                                        //NOntaxableaccpfmonthly = SalaryTPF.PFECRR.ER_Share + SalaryTPF.PFECRR.Arrear_ER_Share;
                                        NOntaxableaccpfmonthly = mCompPF;

                                    }
                                    if (IntPolicyId.PFTTDSMaster.IsOwnTDSAppl == false && IntPolicyId.PFTTDSMaster.IsOwnerTDSAppl == false && IntPolicyId.PFTTDSMaster.IsVPFTDSAppl == false)
                                    {
                                        NOntaxableaccpfmonthly = 0;

                                    }
                                    var intrateeff = IntPolicyId.InterestPolicies.InterestRate.FirstOrDefault();

                                    ReturnData = InterestRate(IntPolicyId.Id, intrateeff.EffectiveFrom, 0, 0);

                                    if (PFTEmpLedgerOld != null)
                                    {
                                        if (Financialyear != null)
                                        {

                                            if (Dateverifyfinancialyear == Financialyear.FromDate.Value.Date)
                                            {
                                                // PFTEmpLedgerOld.AccuNonTaxableAccountFy_Closingbalance financial year start closeing balance 0
                                                if ((NOntaxableaccpfmonthly) > IntPolicyId.PFTTDSMaster.TaxableAccountCelling)
                                                {
                                                    Taxableaccpfmonthly = NOntaxableaccpfmonthly - IntPolicyId.PFTTDSMaster.TaxableAccountCelling;
                                                    NOntaxableaccpfmonthly = NOntaxableaccpfmonthly - Taxableaccpfmonthly;
                                                }

                                            }

                                            if (Dateverifyfinancialyear > Financialyear.FromDate.Value.Date && Dateverifyfinancialyear <= Financialyear.ToDate.Value.Date)
                                            {
                                                if ((PFTEmpLedgerOld.AccuNonTaxableAccountFy_Closingbalance + NOntaxableaccpfmonthly) > IntPolicyId.PFTTDSMaster.TaxableAccountCelling)
                                                {
                                                    Taxableaccpfmonthly = PFTEmpLedgerOld.AccuNonTaxableAccountFy_Closingbalance + NOntaxableaccpfmonthly - IntPolicyId.PFTTDSMaster.TaxableAccountCelling;
                                                    NOntaxableaccpfmonthly = NOntaxableaccpfmonthly - Taxableaccpfmonthly;
                                                }
                                            }
                                        }

                                    }
                                    NontaxInt = (NOntaxableaccpfmonthly * (ReturnData.ReturnValue / (12 * 100)));
                                    NontaxInt = Math.Round(NontaxInt + 0.001, 0);
                                    TaxableaccInt = (Taxableaccpfmonthly * (ReturnData.ReturnValue / (12 * 100)));
                                    TaxableaccInt = Math.Round(TaxableaccInt + 0.001, 0);

                                    //Taxableaccpfmonthly=PFTEmpLedgerOld != null ? iif(PFTEmpLedgerOld.AccuNonTaxableAccountClosingbalance+NOntaxableaccpfmonthly>IntPolicyId.PFTTDSMaster.TaxableAccountCelling) : 0


                                }

                                //collection of PFECR data
                                var EmpSettlementPFTList = new List<EmpSettlementPFT>();
                                //Create new ECR Data object to dump in PFLedger
                                var EmpSettlementPFTECRData = new EmpSettlementPFT()
                                {
                                    GeoStruct = SalaryTPF.GeoStruct,
                                    PayStruct = SalaryTPF.PayStruct,
                                    FuncStruct = SalaryTPF.FuncStruct,
                                    MonthYear = Mpaymonth,
                                    PassbookActivity = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "512").FirstOrDefault().LookupValues.Where(e => e.LookupVal.ToUpper() == "MONTHLY PF POSTING").SingleOrDefault(),
                                    PostingDate = DateTime.Now.Date,
                                    CalcDate = Convert.ToDateTime(Mpaymonth).Date,//SalaryTPF.PaymentDate.Value, //SalaryTPF.PaymentDate null in table
                                    SettlementDate = SettlementDate.Value.Date,
                                    SeperationDate = SettlementDate.Value.Date,
                                    PaymentDate = SettlementDate.Value.Date,
                                    ChequeIssueDate = SettlementDate.Value.Date,

                                    #region Input Data for ledger
                                    // OwnPFMonthly = SalaryTPF.PFECRR.EE_Share + SalaryTPF.PFECRR.Arrear_EE_Share,
                                    OwnPFMonthly = mEmpPF,
                                    // OwnerPFMonthly = SalaryTPF.PFECRR.ER_Share + SalaryTPF.PFECRR.Arrear_ER_Share,
                                    OwnerPFMonthly = mCompPF,
                                    //   VPFAmountMonthly = SalaryTPF.PFECRR.EE_VPF_Share,
                                    VPFAmountMonthly = mEmpVPF,
                                    // PensionAmount = SalaryTPF.PFECRR.EPS_Share + SalaryTPF.PFECRR.Arrear_EPS_Share,
                                    PensionAmount = mEmpPension,
                                    //  PFWages = SalaryTPF.PFECRR.EPF_Wages + SalaryTPF.PFECRR.Arrear_EPF_Wages,
                                    PFWages = mPFWages,
                                    #endregion Input Data for ledger

                                    //IsPassbookClose = false,
                                    Narration = "PF Posting For Month" + Mpaymonth.ToString(),
                                    InterestFrequency = null,
                                    DBTrack = DBTrack,
                                    AccuNonTaxableAccountFy_Closingbalance = NOntaxableaccpfmonthly,
                                    NonTaxableAccountMonthly = NOntaxableaccpfmonthly,
                                    NonTaxableAccountInterest = NontaxInt,
                                    TaxableAccountFy_Closingbalance = Taxableaccpfmonthly,
                                    TaxableAccountMonthly = Taxableaccpfmonthly,
                                    TaxableAccountInterest = TaxableaccInt,
                                };
                                EmpSettlementPFTList.Add(EmpSettlementPFTECRData);//added monthly PF data
                                //Create new ECRData Balance pftlegder object

                                double AccuNonTaxableAccountFy_Closingbalancepost = 0;
                                double AccuNonTaxableAccountFy_Openingbalancepost = 0;
                                double TaxableAccountFy_Closingbalancepost = 0;
                                double TaxableAccountFy_Openingbalancepost = 0;
                                double NonTaxableAccountInterestpost = 0;
                                double TaxableAccountInterestpost = 0;
                                if (Financialyear != null)
                                {

                                    if (Dateverifyfinancialyear == Financialyear.FromDate.Value.Date)
                                    {
                                        AccuNonTaxableAccountFy_Closingbalancepost = NOntaxableaccpfmonthly;
                                        AccuNonTaxableAccountFy_Openingbalancepost = 0;

                                        TaxableAccountFy_Closingbalancepost = Taxableaccpfmonthly;
                                        TaxableAccountFy_Openingbalancepost = 0;

                                        NonTaxableAccountInterestpost = NontaxInt;
                                        TaxableAccountInterestpost = TaxableaccInt;


                                    }


                                    if (Dateverifyfinancialyear > Financialyear.FromDate.Value.Date && Dateverifyfinancialyear <= Financialyear.ToDate.Value.Date)
                                    {
                                        AccuNonTaxableAccountFy_Closingbalancepost = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.AccuNonTaxableAccountFy_Closingbalance : 0) + NOntaxableaccpfmonthly;//check taxableaccpFy
                                        AccuNonTaxableAccountFy_Openingbalancepost = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.AccuNonTaxableAccountFy_Closingbalance : 0);
                                        if (IntPolicyId.PFTTDSMaster != null)
                                        {
                                            if (AccuNonTaxableAccountFy_Closingbalancepost > IntPolicyId.PFTTDSMaster.TaxableAccountCelling)
                                            {
                                                AccuNonTaxableAccountFy_Closingbalancepost = IntPolicyId.PFTTDSMaster.TaxableAccountCelling;
                                            }
                                        }

                                        TaxableAccountFy_Closingbalancepost = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.TaxableAccountFy_Closingbalance : 0) + Taxableaccpfmonthly;
                                        TaxableAccountFy_Openingbalancepost = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.TaxableAccountFy_Closingbalance : 0);


                                        NonTaxableAccountInterestpost = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.NonTaxableAccountInterest : 0) + NontaxInt;
                                        TaxableAccountInterestpost = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.TaxableAccountInterest : 0) + TaxableaccInt;

                                    }

                                }



                                var EmpSettlementPFTECRDataBalance = new EmpSettlementPFT()
                                {
                                    GeoStruct = SalaryTPF.GeoStruct,
                                    PayStruct = SalaryTPF.PayStruct,
                                    FuncStruct = SalaryTPF.FuncStruct,
                                    MonthYear = Mpaymonth,
                                    PassbookActivity = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "512").FirstOrDefault().LookupValues.Where(e => e.LookupVal.ToUpper() == "PF BALANCE").SingleOrDefault(),
                                    PostingDate = DateTime.Now.Date,
                                    CalcDate = Convert.ToDateTime(Mpaymonth).Date,//SalaryTPF.PaymentDate.Value, //SalaryTPF.PaymentDate null in table
                                    SettlementDate = SettlementDate.Value.Date,
                                    SeperationDate = SettlementDate.Value.Date,
                                    PaymentDate = SettlementDate.Value.Date,
                                    ChequeIssueDate = SettlementDate.Value.Date,

                                    #region Input Data for ledger
                                    //OwnPFMonthly = SalaryTPF.PFECRR.EE_Share + SalaryTPF.PFECRR.Arrear_EE_Share,
                                    //OwnerPFMonthly = SalaryTPF.PFECRR.ER_Share + SalaryTPF.PFECRR.Arrear_ER_Share,
                                    //VPFAmountMonthly = SalaryTPF.PFECRR.EE_VPF_Share,
                                    //PensionAmount = SalaryTPF.PFECRR.EPS_Share + SalaryTPF.PFECRR.Arrear_EPS_Share,
                                    //PFWages = SalaryTPF.PFECRR.EPF_Wages + SalaryTPF.PFECRR.Arrear_EPF_Wages,
                                    LoanAmountCredit = 0,
                                    LoanAmountDebit = 0,
                                    //OwnPFInt = 0,
                                    //OwnerPFInt = 0,
                                    //VPFInt = 0,
                                    //PFInt = 0,
                                    #endregion Input Data for ledger

                                    OwnOpenBal = PFTEmpLedgerOld != null ? PFTEmpLedgerOld.OwnCloseBal : 0,
                                    OwnCloseBal = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.OwnCloseBal : 0) + mEmpPF,
                                    OwnerOpenBal = PFTEmpLedgerOld != null ? PFTEmpLedgerOld.OwnerCloseBal : 0,
                                    OwnerCloseBal = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.OwnerCloseBal : 0) + mCompPF,
                                    VPFOpenBal = PFTEmpLedgerOld != null ? PFTEmpLedgerOld.VPFCloseBal : 0,
                                    VPFCloseBal = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.VPFCloseBal : 0) + mEmpVPF,
                                    PFOpenBal = PFTEmpLedgerOld != null ? PFTEmpLedgerOld.PFCloseBal : 0,
                                    PFCloseBal = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.PFCloseBal : 0) + mEmpPF + mCompPF,

                                    OwnIntOpenBal = PFTEmpLedgerOld != null ? PFTEmpLedgerOld.OwnIntCloseBal : 0,
                                    OwnIntCloseBal = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.OwnIntCloseBal : 0),
                                    OwnerIntOpenBal = PFTEmpLedgerOld != null ? PFTEmpLedgerOld.OwnerIntCloseBal : 0,
                                    OwnerIntCloseBal = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.OwnerIntCloseBal : 0),
                                    VPFIntOpenBal = PFTEmpLedgerOld != null ? PFTEmpLedgerOld.VPFIntCloseBal : 0,
                                    VPFIntCloseBal = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.VPFIntCloseBal : 0),
                                    PFIntOpenBal = PFTEmpLedgerOld != null ? PFTEmpLedgerOld.PFIntCloseBal : 0,
                                    PFIntCloseBal = PFTEmpLedgerOld != null ? PFTEmpLedgerOld.PFIntCloseBal : 0,



                                    OwnPFLoan = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.OwnPFLoan : 0),
                                    OwnerPFLoan = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.OwnerPFLoan : 0),
                                    VPFPFLoan = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.VPFPFLoan : 0),

                                    IntOnInt = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.IntOnInt : 0),
                                    IntonIntOpenBal = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.IntonIntCloseBal : 0),
                                    IntonIntCloseBal = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.IntonIntCloseBal : 0),
                                    IntOnIntPFLoan = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.IntOnIntPFLoan : 0),
                                    OwnIntPFLoan = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.OwnIntPFLoan : 0),
                                    OwnerIntPFLoan = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.OwnerIntPFLoan : 0),
                                    VPFIntPFLoan = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.VPFIntPFLoan : 0),
                                    TotalIntOpenBal = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.TotalIntCloseBal : 0),
                                    TotalIntCloseBal = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.TotalIntCloseBal : 0),


                                    // IsPassbookClose = false,
                                    Narration = "PF Balance For Month" + Mpaymonth.ToString(),
                                    InterestFrequency = null,
                                    DBTrack = DBTrack,
                                    AccuNonTaxableAccountOpeningbalance = PFTEmpLedgerOld != null ? PFTEmpLedgerOld.AccuNonTaxableAccountClosingbalance : 0,
                                    AccuNonTaxableAccountClosingbalance = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.AccuNonTaxableAccountClosingbalance : 0) + NOntaxableaccpfmonthly,
                                    AccTaxableAccountOpeningbalance = PFTEmpLedgerOld != null ? PFTEmpLedgerOld.AccTaxableAccountClosingbalance : 0,
                                    AccTaxableAccountClosingbalance = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.AccTaxableAccountClosingbalance : 0) + Taxableaccpfmonthly,

                                    AccuNonTaxableAccountFy_Openingbalance = AccuNonTaxableAccountFy_Openingbalancepost,//PFTEmpLedgerOld != null ? PFTEmpLedgerOld.AccuNonTaxableAccountFy_Closingbalance : 0,
                                    AccuNonTaxableAccountFy_Closingbalance = AccuNonTaxableAccountFy_Closingbalancepost,//check taxableaccpFy
                                    TaxableAccountFy_Openingbalance = TaxableAccountFy_Openingbalancepost,//PFTEmpLedgerOld != null ? PFTEmpLedgerOld.TaxableAccountFy_Closingbalance : 0,
                                    TaxableAccountFy_Closingbalance = TaxableAccountFy_Closingbalancepost,//(PFTEmpLedgerOld != null ? PFTEmpLedgerOld.TaxableAccountFy_Closingbalance : 0) + Taxableaccpfmonthly,

                                    NonTaxableAccountInterest = NonTaxableAccountInterestpost,//(PFTEmpLedgerOld != null ? PFTEmpLedgerOld.NonTaxableAccountInterest : 0) + NontaxInt,
                                    TaxableAccountInterest = TaxableAccountInterestpost,//(PFTEmpLedgerOld != null ? PFTEmpLedgerOld.TaxableAccountInterest : 0) + TaxableaccInt,


                                };
                                EmpSettlementPFTList.Add(EmpSettlementPFTECRDataBalance);//added monthly PF data balance
                                //var PFTEmployeeLedgerAdd = db.PFTEmployeeLedger.Where(e => == Employee_Id).SingleOrDefault();
                                var EmployeePFTrust = db.EmployeePFTrust.Include(e => e.EmpSettlementPFT).Where(e => e.Employee_Id == Employee_Id).SingleOrDefault();
                                // EmployeePFTrust.PFTEmployeeLedger.ToList().AddRange(PFTEmpLedgerList);
                                EmpSettlementPFTList.AddRange(EmployeePFTrust.EmpSettlementPFT);
                                EmployeePFTrust.EmpSettlementPFT = EmpSettlementPFTList;
                                db.EmployeePFTrust.Attach(EmployeePFTrust);
                                db.Entry(EmployeePFTrust).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(EmployeePFTrust).State = System.Data.Entity.EntityState.Detached;
                                // EmpCount++;
                            }

                        }

                    }
                    //ErrorLog.Errno = 0; //Holiday calendar not defined
                    //ErrorLog.ErrMsg = "PF Sucessfully Uploaded to PF Trust for Employees " + EmpCount.ToString() + " On Month " + MonthYear;
                    //return ErrorLog;
                }
                MsgP.Add("PF Sucessfully Uploaded to PF Trust total Employees count " + EmpCount.ToString() + " On Month " + MonthYear);

            }
            catch (Exception ex)
            {
                MsgP.Add("ProcessObjectName = " + ex.Source.ToString() + " Inner ExceptionMessage = " + ex.InnerException.Message + " ExceptionMessage = " + ex.Message + " ExceptionStackTrace = " + ex.StackTrace + "LineNo = " + Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()) + "LogTime = " + DateTime.Now);
                //ErrorLog.Errno = 1; //Holiday calendar not defined
                //ErrorLog.ErrMsg = "ProcessObjectName = " + ex.Source.ToString() + " Inner ExceptionMessage = " + ex.InnerException.Message + " ExceptionMessage = " + ex.Message + " ExceptionStackTrace = " + ex.StackTrace + "LineNo = " + Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()) + "LogTime = " + DateTime.Now;
                //return ErrorLog;
            };
            return MsgP;
        }
        #endregion PFECR Projection data upload from Payroll Module into PFTrust module

        #region Loan data posting from Payroll Module into PFTrust module
         public static List<ReturnData> LoanDebitPosting( string MonthYear)
        //public static List<string> LoanDebitPosting(string MonthYear)
        {
            ReturnData ErrorLog = new ReturnData();
            List<ReturnData> ReturnDataList = new List<ReturnData>();
            DBTrack DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
            List<string> Msg = new List<string>();
            try
            {
                using (DataBaseContext db = new DataBaseContext())
                {
                    //Non Refundable Loans
                    DateTime MonthYearDate = Convert.ToDateTime("01/" + MonthYear);
                    DateTime MonthYearDateTo = MonthYearDate.AddMonths(1).AddDays(-1).Date;

                    var LoanAdvRequestPFT = db.LoanAdvRequestPFT.Include(e=>e.LoanAdvanceHeadPFT).Include(e=>e.LoanWFDetails).Where(e => e.IsActive == true && e.CloserDate==null && e.SanctionedDate >= MonthYearDate && e.SanctionedDate <= MonthYearDateTo).ToList();
                    //Loan requests
                    if (LoanAdvRequestPFT != null && LoanAdvRequestPFT.Count() > 0)
                    {
                        var EmpCount = 0;


                        var PassbookLoanIDValue = new List<string>();
                        PassbookLoanIDValue.Add("LOAN DEBIT POSTING");
                        PassbookLoanIDValue.Add("LOAN DEBIT BALANCE");
                        //PassbookLoanIDValue[0] = "LOAN DEBIT";
                        //PassbookLoanIDValue[1] = "LOAN DEBIT BALANCE";
                        List<int> PassbookID = new List<int>();
                        PassbookID = db.LookupValue.Where(e => PassbookLoanIDValue.Contains(e.LookupVal.ToUpper())).Select(e => e.Id).ToList();

                        foreach (var LoanAdvRequestPFTitem in LoanAdvRequestPFT)
                        {
                            var wfstatusloan = LoanAdvRequestPFTitem.LoanWFDetails.ToList().OrderByDescending(e => e.Id).FirstOrDefault().WFStatus;
                            if (wfstatusloan!=1)// if not sanction then skip
                            {
                                  continue;
                            }
                            //ErrorLog = null;
                            var LoanHeadPolicy = db.LoanAdvanceHeadPFT.Include(e => e.LoanAdvancePolicyPFT).Include(e => e.LoanAdvancePolicyPFT.PFLoanType).Where(e => e.Id == LoanAdvRequestPFTitem.LoanAdvanceHeadPFT_Id).SingleOrDefault();
                            if (LoanHeadPolicy.LoanAdvancePolicyPFT.PFLoanType.LookupVal.ToUpper() != "NONREFUNDABLE")
                            {
                                continue;
                            }
                            //non refundable loan
                            //var LoanAdvRequestPFT_Id = db.LoanAdvRepaymentTPFT.Where(e => e.Id == LoanAdvRepaymentTPFTitem).Select(e=>e.LoanAdvRequestPFT_Id.Value).SingleOrDefault();
                            //var LoanAdvRepaymentTPFTData = db.LoanAdvRepaymentTPFT.Where(e => e.Id == LoanAdvRepaymentTPFTitem).SingleOrDefault();
                            //if (LoanAdvRequestPFT_Id >0)
                            //{
                            int EmployeePFT_Id = LoanAdvRequestPFTitem.EmployeePFTrust_Id.Value;// .EmployeePFTrust  db.LoanAdvRequestPFT.Where(e=>e.Id== LoanAdvRequestPFT_Id).Select(e=>e.EmployeePFTrust_Id.Value).SingleOrDefault();
                            //int Employee_Id = db.EmployeePayroll.Where(e => e.Id == EmployeePayroll_Id).Select(e => e.Employee_Id.Value).SingleOrDefault();
                            var employee = db.EmployeePFTrust.Include(e => e.Employee).Where(e => e.Id == EmployeePFT_Id).SingleOrDefault();

                            P2B.UTILS.P2BLogger logger = new P2B.UTILS.P2BLogger();
                            string EmpPayrollID = employee.Employee_Id.ToString();
                            logger.Logging("EmployeeID Loan Posting in Ledger::::  " + EmpPayrollID);
                            ErrorLog = new ReturnData();

                            #region Check record existance
                            var PFTEmployeeLedgerCurrent = db.EmployeePFTrust.Where(e => e.Id == EmployeePFT_Id)
                                .Select(e => e.PFTEmployeeLedger.Where(r => r.MonthYear == MonthYear
                                && PassbookID.Contains(r.PassbookActivity.Id)).ToList()).SingleOrDefault();

                            var PFTEmployeeLedgerLastRec = db.EmployeePFTrust.Include(e => e.PFTEmployeeLedger).Where(e => e.Id == EmployeePFT_Id)
                                .SingleOrDefault();

                            //var PFTEmployeeLedgerLastRecord = db.EmployeePFTrust.Where(e => e.Id == EmployeePFT_Id)
                            //    .Select(e => e.PFTEmployeeLedger.LastOrDefault()).SingleOrDefault();
                            var PFTEmployeeLedgerLastRecord = PFTEmployeeLedgerLastRec.PFTEmployeeLedger.OrderBy(e => e.Id).LastOrDefault();

                            var PFTEmployeeLedgerCurrent_Ids = PFTEmployeeLedgerCurrent.Select(e => e.Id).ToList();
                            if (PFTEmployeeLedgerCurrent_Ids.Contains(PFTEmployeeLedgerLastRecord.Id) == true)
                            {
                                ErrorLog.Errno = 1;
                                ErrorLog.ErrMsg = "loan posting is not allowed in between Employee PF Trust ledger" + employee.Employee.EmpCode;
                                ErrorLog.ReturnValue = EmployeePFT_Id;
                                ReturnDataList.Add(ErrorLog);
                                //Msg.Add("loan posting is not allowed in between Employee PF Trust ledger " + employee.Employee.EmpCode);
                                //loan posting is not allowed in between Employee PF Trust ledger
                                continue;
                            }
                            if (PFTEmployeeLedgerCurrent != null)
                            {
                                db.PFTEmployeeLedger.RemoveRange(PFTEmployeeLedgerCurrent);
                                db.SaveChanges();
                            }
                            #endregion Check record existance
                            //previous record to fetch closing balance
                            var PFTEmpLedgerOldList = db.EmployeePFTrust.Where(e => e.Id == EmployeePFT_Id).Select(r => r.PFTEmployeeLedger.ToList()).SingleOrDefault();
                            var PFTEmpLedgerOld = PFTEmpLedgerOldList.LastOrDefault();
                            //collection of PFECR data
                            var PFTEmpLedgerList = new List<PFTEmployeeLedger>();
                            //Create new ECR Data object to dump in PFLedger
                            var PFTEmpLedgerECRData = new PFTEmployeeLedger()
                            {
                                GeoStruct = PFTEmpLedgerOld.GeoStruct,
                                PayStruct = PFTEmpLedgerOld.PayStruct,
                                FuncStruct = PFTEmpLedgerOld.FuncStruct,
                                MonthYear = MonthYear,

                                PassbookActivity = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "512").FirstOrDefault().LookupValues.Where(e => e.LookupVal.ToUpper() == "LOAN DEBIT POSTING").SingleOrDefault(),
                                PostingDate = DateTime.Now.Date,
                                CalcDate = LoanAdvRequestPFTitem.SanctionedDate.Value,

                                #region Input Data for ledger
                                LoanAmountDebit = LoanAdvRequestPFTitem.LoanSanctionedAmount,
                                OwnPFLoan = LoanHeadPolicy.LoanAdvancePolicyPFT.IsOwnContibution == true ? LoanAdvRequestPFTitem.OwnPFAmount : 0,
                                OwnerPFLoan = LoanHeadPolicy.LoanAdvancePolicyPFT.IsOwnerContibution == true ? LoanAdvRequestPFTitem.OwnerPFAmount : 0,
                                VPFPFLoan = LoanHeadPolicy.LoanAdvancePolicyPFT.IsVPFContibution == true ? LoanAdvRequestPFTitem.VPFAmount : 0,
                                OwnIntPFLoan = LoanHeadPolicy.LoanAdvancePolicyPFT.IsOwnIntContibution == true ? LoanAdvRequestPFTitem.OwnPFIntAmount : 0,
                                OwnerIntPFLoan = LoanHeadPolicy.LoanAdvancePolicyPFT.IsOwnerIntContibution == true ? LoanAdvRequestPFTitem.OwnerPFIntAmount : 0,
                                VPFIntPFLoan = LoanHeadPolicy.LoanAdvancePolicyPFT.IsVPFIntContibution == true ? LoanAdvRequestPFTitem.VPFIntAmount : 0,
                                IntOnIntPFLoan = LoanHeadPolicy.LoanAdvancePolicyPFT.IsIntOnIntContibution == true ? LoanAdvRequestPFTitem.IntOnIntAmount : 0,

                                LoanAmountCredit = 0,
                                #endregion Input Data for ledger

                                IsPassbookClose = false,
                                Narration =LoanAdvRequestPFTitem.LoanAdvanceHeadPFT.Code+ " Loan Debit For Month" + MonthYear.ToString(),
                                InterestFrequency = null,
                                DBTrack = DBTrack

                            };
                            PFTEmpLedgerList.Add(PFTEmpLedgerECRData);//added monthly PF data
                            //Create new ECRData Balance pftlegder object
                            var PFTEmpLedgerECRDataBalance = new PFTEmployeeLedger()
                            {
                                GeoStruct = PFTEmpLedgerECRData.GeoStruct,
                                PayStruct = PFTEmpLedgerECRData.PayStruct,
                                FuncStruct = PFTEmpLedgerECRData.FuncStruct,
                                MonthYear = MonthYear,
                                PassbookActivity = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "512").FirstOrDefault().LookupValues.Where(e => e.LookupVal.ToUpper() == "LOAN DEBIT BALANCE").SingleOrDefault(),
                                PostingDate = DateTime.Now.Date,
                                CalcDate = PFTEmpLedgerECRData.CalcDate,

                                #region Input Data for ledger
                                //OwnPFMonthly = SalaryTPF.PFECRR.EE_Share + SalaryTPF.PFECRR.Arrear_EE_Share,
                                //OwnerPFMonthly = SalaryTPF.PFECRR.ER_Share + SalaryTPF.PFECRR.Arrear_ER_Share,
                                //VPFAmountMonthly = SalaryTPF.PFECRR.EE_VPF_Share,
                                //PensionAmount = SalaryTPF.PFECRR.EPS_Share + SalaryTPF.PFECRR.Arrear_EPS_Share,
                                //PFWages = SalaryTPF.PFECRR.EPF_Wages + SalaryTPF.PFECRR.Arrear_EPF_Wages,
                                LoanAmountCredit = 0,
                                LoanAmountDebit = 0,
                                OwnPFInt = 0,
                                OwnerPFInt = 0,
                                VPFInt = 0,
                                PFInt = 0,
                                #endregion Input Data for ledger

                                OwnOpenBal = PFTEmpLedgerOld != null ? PFTEmpLedgerOld.OwnCloseBal : 0,
                                OwnCloseBal = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.OwnCloseBal : 0) - PFTEmpLedgerECRData.OwnPFLoan,
                                OwnerOpenBal = PFTEmpLedgerOld != null ? PFTEmpLedgerOld.OwnerCloseBal : 0,
                                OwnerCloseBal = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.OwnerCloseBal : 0) - PFTEmpLedgerECRData.OwnerPFLoan,
                                VPFOpenBal = PFTEmpLedgerOld != null ? PFTEmpLedgerOld.VPFCloseBal : 0,
                                VPFCloseBal = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.VPFCloseBal : 0) - PFTEmpLedgerECRData.VPFPFLoan,
                                PFOpenBal = PFTEmpLedgerOld != null ? PFTEmpLedgerOld.PFCloseBal : 0,
                                PFCloseBal = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.PFCloseBal : 0) - PFTEmpLedgerECRData.OwnPFLoan - PFTEmpLedgerECRData.OwnerPFLoan,

                                OwnIntOpenBal = PFTEmpLedgerOld != null ? PFTEmpLedgerOld.OwnIntOpenBal : 0,
                                OwnIntCloseBal = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.OwnIntCloseBal : 0) - PFTEmpLedgerECRData.OwnIntPFLoan,
                                OwnerIntOpenBal = PFTEmpLedgerOld != null ? PFTEmpLedgerOld.OwnerIntCloseBal : 0,
                                OwnerIntCloseBal = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.OwnerIntCloseBal : 0) - PFTEmpLedgerECRData.OwnerIntPFLoan,
                                VPFIntOpenBal = PFTEmpLedgerOld != null ? PFTEmpLedgerOld.VPFIntCloseBal : 0,
                                VPFIntCloseBal = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.VPFIntCloseBal : 0) - PFTEmpLedgerECRData.VPFIntPFLoan,
                                PFIntOpenBal = PFTEmpLedgerOld != null ? PFTEmpLedgerOld.PFIntCloseBal : 0,
                                PFIntCloseBal = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.PFIntCloseBal : 0) - PFTEmpLedgerECRData.OwnIntPFLoan - PFTEmpLedgerECRData.OwnerIntPFLoan,



                                OwnPFLoan = 0,
                                OwnerPFLoan = 0,
                                VPFPFLoan = 0,

                                IntOnInt = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.IntOnInt : 0),
                                IntonIntOpenBal = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.IntonIntOpenBal : 0),
                                IntonIntCloseBal = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.IntonIntCloseBal : 0) - PFTEmpLedgerECRData.IntOnIntPFLoan,
                                IntOnIntPFLoan = 0,//(PFTEmpLedgerOld != null ? PFTEmpLedgerOld.IntOnIntPFLoan : 0),
                                OwnIntPFLoan = 0,//(PFTEmpLedgerOld != null ? PFTEmpLedgerOld.OwnIntPFLoan : 0),
                                OwnerIntPFLoan = 0,//(PFTEmpLedgerOld != null ? PFTEmpLedgerOld.OwnerIntPFLoan : 0),
                                VPFIntPFLoan = 0,//(PFTEmpLedgerOld != null ? PFTEmpLedgerOld.VPFIntPFLoan : 0),

                                TotalIntOpenBal = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.TotalIntOpenBal : 0),
                                TotalIntCloseBal = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.TotalIntCloseBal : 0)
                                                    - PFTEmpLedgerECRData.OwnIntPFLoan
                                                    - PFTEmpLedgerECRData.OwnerIntPFLoan
                                                    - PFTEmpLedgerECRData.VPFIntPFLoan,


                                IsPassbookClose = false,
                                Narration = LoanAdvRequestPFTitem.LoanAdvanceHeadPFT.Code + " Loan Debit Balance For Month" + MonthYear.ToString(),
                                InterestFrequency = null,
                                DBTrack = DBTrack

                            };
                            PFTEmpLedgerList.Add(PFTEmpLedgerECRDataBalance);//added monthly PF data balance
                            //var PFTEmployeeLedgerAdd = db.PFTEmployeeLedger.Where(e => == Employee_Id).SingleOrDefault();
                            var EmployeePFTrust = db.EmployeePFTrust.Include(e => e.PFTEmployeeLedger).Where(e => e.Id == EmployeePFT_Id).SingleOrDefault();
                            // EmployeePFTrust.PFTEmployeeLedger.ToList().AddRange(PFTEmpLedgerList);
                            PFTEmpLedgerList.AddRange(EmployeePFTrust.PFTEmployeeLedger);
                            EmployeePFTrust.PFTEmployeeLedger = PFTEmpLedgerList;
                            db.EmployeePFTrust.Attach(EmployeePFTrust);
                            db.Entry(EmployeePFTrust).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            db.Entry(EmployeePFTrust).State = System.Data.Entity.EntityState.Detached;

                            //closer date update
                            var db_data = db.LoanAdvRequestPFT.Include(e => e.LoanAdvanceHeadPFT)
                                                              .Include(e => e.LoanWFDetails)
                                                              .Where(e => e.Id == LoanAdvRequestPFTitem.Id).SingleOrDefault();
                            db_data.CloserDate = Convert.ToDateTime(MonthYear).Date;
                            db.LoanAdvRequestPFT.Attach(db_data);
                            db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();

                            EmpCount++;
                        }

                        ErrorLog = new ReturnData();
                        ErrorLog.Errno = 0; //Holiday calendar not defined
                        ErrorLog.ErrMsg = "Loan Posting done Sucessfully to PF Trust for Employees " + EmpCount.ToString() + " On Month " + MonthYear;
                        ReturnDataList.Add(ErrorLog);
                        return ReturnDataList;
                        //Msg.Add("Loan Posting done Sucessfully to PF Trust total Employees count " + EmpCount.ToString() + " On Month " + MonthYear);
                        //return Msg;
                    }
                    else
                    {
                        ErrorLog = new ReturnData();
                        ErrorLog.Errno = 0; //Holiday calendar not defined
                        ErrorLog.ErrMsg = "Loan data for Posting is not available" + " On Month " + MonthYear;
                        ReturnDataList.Add(ErrorLog);
                        return ReturnDataList;
                        //Msg.Add("Loan data for Posting is not available" + " On Month " + MonthYear);
                        //return Msg;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLog = new ReturnData();
                ErrorLog.Errno = 1; //Holiday calendar not defined
                ErrorLog.ErrMsg = "ProcessObjectName = " + ex.Source.ToString() + " Inner ExceptionMessage = " + ex.InnerException.Message + " ExceptionMessage = " + ex.Message + " ExceptionStackTrace = " + ex.StackTrace + "LineNo = " + Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()) + "LogTime = " + DateTime.Now;
                ReturnDataList.Add(ErrorLog);
                return ReturnDataList;
                //Msg.Add("ProcessObjectName = " + ex.Source.ToString() + " Inner ExceptionMessage = " + ex.InnerException.Message + " ExceptionMessage = " + ex.Message + " ExceptionStackTrace = " + ex.StackTrace + "LineNo = " + Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()) + "LogTime = " + DateTime.Now);
                //return Msg;

            };
        }
        #endregion Loan Posting is done  into PFTrust module


        #region PFInterestRate calculation
        //--InterestProduct definition parameters
        //--0 : OWN Employee
        //--1 : Owner Employer
        //--2 : VPF Employee VPF
        //--3 : PF
        //--4 : Total pf Contribution-include vpf
        //--5 : Total Int Contribution--interest on interest -Interest not added to employee's contribution

        //--TrustType as 0 = Comp or 1 = Govt

        //other parameter definition
        //{Interest policy of accounting year, Interest posting date, Base Product Type for Interest identification,PF trust type Comp -0/Govt-1}
        //return the interest rate as per base product in double, [0]=Interest rate [1]=Error
        public static ReturnData InterestRate(int IntPolicyId, DateTime IntPostingDate, int InterestProduct, int TrustType)
        {
            ErrorLog ErrorLog = new ErrorLog();
            using (DataBaseContext db = new DataBaseContext())
            {
                //List<double> InterestRate = new List<double>();
                var ReturnData = new ReturnData();
                Double IntRate = 0;
                if (TrustType == 0)//trust
                {
                    //var IntPostingDate = InterestPostingMonth;// Convert.ToDateTime("01/" + InterestPostingMonth).Date;
                    var InterestPolicies = db.InterestPolicies.Include(e => e.InterestRate).Where(e => e.Id == IntPolicyId).SingleOrDefault();
                    if (InterestPolicies != null)
                    {
                        switch (InterestProduct)
                        {
                            case 0://--0 : OWN Employee
                                IntRate = InterestPolicies.InterestRate.Where(e => IntPostingDate >= e.EffectiveFrom && IntPostingDate <= e.EffectiveTo)
                                                .Select(e => e.TrustEMPPFInt).SingleOrDefault();
                                break;
                            case 1://--1 : Owner Employer
                                IntRate = InterestPolicies.InterestRate.Where(e => IntPostingDate >= e.EffectiveFrom && IntPostingDate <= e.EffectiveTo)
                                                .Select(e => e.TrustEMPPFInt).SingleOrDefault();
                                break;
                            case 2://--2 : VPF Employee VPF
                                IntRate = InterestPolicies.InterestRate.Where(e => IntPostingDate >= e.EffectiveFrom && IntPostingDate <= e.EffectiveTo)
                                                .Select(e => e.TrustVPFInt).SingleOrDefault();
                                break;
                            case 3://--3 : PF
                                IntRate = InterestPolicies.InterestRate.Where(e => IntPostingDate >= e.EffectiveFrom && IntPostingDate <= e.EffectiveTo)
                                                .Select(e => e.TrustEMPPFInt).SingleOrDefault();
                                break;
                            case 4://--4 : Total pf Contribution
                                IntRate = InterestPolicies.InterestRate.Where(e => IntPostingDate >= e.EffectiveFrom && IntPostingDate <= e.EffectiveTo)
                                                .Select(e => e.TrustEMPPFInt).SingleOrDefault();
                                break;
                            case 5://--4 : Total int Contribution
                                IntRate = InterestPolicies.InterestRate.Where(e => IntPostingDate >= e.EffectiveFrom && IntPostingDate <= e.EffectiveTo)
                                                .Select(e => e.TrustEMPPFInt).SingleOrDefault();
                                break;
                            case 6://--4 : own interest on interest Contribution
                                IntRate = InterestPolicies.InterestRate.Where(e => IntPostingDate >= e.EffectiveFrom && IntPostingDate <= e.EffectiveTo)
                                                .Select(e => e.TrustEMPPFInt).SingleOrDefault();
                                break;
                            case 7://--4 : owner interest on interest Contribution
                                IntRate = InterestPolicies.InterestRate.Where(e => IntPostingDate >= e.EffectiveFrom && IntPostingDate <= e.EffectiveTo)
                                                .Select(e => e.TrustEMPPFInt).SingleOrDefault();
                                break;
                            case 8://--4 : vpf interest on interest Contribution
                                IntRate = InterestPolicies.InterestRate.Where(e => IntPostingDate >= e.EffectiveFrom && IntPostingDate <= e.EffectiveTo)
                                                .Select(e => e.TrustVPFInt).SingleOrDefault();
                                break;
                            case 9://--4 : Taxable account
                                IntRate = InterestPolicies.InterestRate.Where(e => IntPostingDate >= e.EffectiveFrom && IntPostingDate <= e.EffectiveTo)
                                                .Select(e => e.TrustEMPPFInt).SingleOrDefault();
                                break;
                            case 10://--4 : non Taxable account
                                IntRate = InterestPolicies.InterestRate.Where(e => IntPostingDate >= e.EffectiveFrom && IntPostingDate <= e.EffectiveTo)
                                                .Select(e => e.TrustEMPPFInt).SingleOrDefault();

                                break;
                            case 11://--0 : OWN interest on interest
                                IntRate = InterestPolicies.InterestRate.Where(e => IntPostingDate >= e.EffectiveFrom && IntPostingDate <= e.EffectiveTo)
                                                .Select(e => e.TrustEMPPFInt).SingleOrDefault();
                                break;
                            case 12://--1 :OWNER interest on interest
                                IntRate = InterestPolicies.InterestRate.Where(e => IntPostingDate >= e.EffectiveFrom && IntPostingDate <= e.EffectiveTo)
                                                .Select(e => e.TrustEMPPFInt).SingleOrDefault();
                                break;
                            case 13://--2 : VPF interest on interest
                                IntRate = InterestPolicies.InterestRate.Where(e => IntPostingDate >= e.EffectiveFrom && IntPostingDate <= e.EffectiveTo)
                                                .Select(e => e.TrustVPFInt).SingleOrDefault();
                                break;
                        }

                        if (IntRate != 0)
                        {
                            ReturnData.ReturnValue = IntRate;//value
                            ReturnData.Errno = 0; //error
                            return ReturnData;
                        }
                        else
                        {
                            ReturnData.ReturnValue = IntRate;//value
                            ReturnData.Errno = 1; //error
                            ReturnData.ErrMsg = "Interest Rate is not defined";
                            return ReturnData;
                        }
                    }
                    else
                    {
                        ReturnData.ReturnValue = IntRate;//value
                        ReturnData.Errno = 1; //error
                        ReturnData.ErrMsg = "Interest Policy is not defined";

                        return ReturnData;
                    }
                }
                else
                {

                    var InterestPolicies = db.InterestPolicies.Include(e => e.InterestRate).Where(e => e.Id == IntPolicyId).SingleOrDefault();
                    if (InterestPolicies != null)
                    {
                        switch (InterestProduct)
                        {
                            case 0://--0 : OWN Employee
                                IntRate = InterestPolicies.InterestRate.Where(e => IntPostingDate >= e.EffectiveFrom && IntPostingDate <= e.EffectiveTo)
                                                .Select(e => e.GovtEMPPFInt).SingleOrDefault();
                                break;
                            case 1://--1 : Owner Employer
                                IntRate = InterestPolicies.InterestRate.Where(e => IntPostingDate >= e.EffectiveFrom && IntPostingDate <= e.EffectiveTo)
                                                .Select(e => e.GovtEMPPFInt).SingleOrDefault();
                                break;
                            case 2://--2 : VPF Employee VPF
                                IntRate = InterestPolicies.InterestRate.Where(e => IntPostingDate >= e.EffectiveFrom && IntPostingDate <= e.EffectiveTo)
                                                .Select(e => e.GovtVPFInt).SingleOrDefault();
                                break;
                            case 3://--3 : PF
                                IntRate = InterestPolicies.InterestRate.Where(e => IntPostingDate >= e.EffectiveFrom && IntPostingDate <= e.EffectiveTo)
                                                .Select(e => e.GovtEMPPFInt).SingleOrDefault();
                                break;
                            case 4://--4 : Total pf Contribution
                                IntRate = InterestPolicies.InterestRate.Where(e => IntPostingDate >= e.EffectiveFrom && IntPostingDate <= e.EffectiveTo)
                                                .Select(e => e.GovtEMPPFInt).SingleOrDefault();
                                break;
                            case 5://--4 : Total int Contribution
                                IntRate = InterestPolicies.InterestRate.Where(e => IntPostingDate >= e.EffectiveFrom && IntPostingDate <= e.EffectiveTo)
                                                .Select(e => e.GovtEMPPFInt).SingleOrDefault();
                                break;
                        }
                        if (IntRate != 0)
                        {
                            ReturnData.ReturnValue = IntRate;//value
                            ReturnData.Errno = 0; //error
                            return ReturnData;
                        }
                        else
                        {
                            ReturnData.ReturnValue = IntRate;//value
                            ReturnData.Errno = 1; //error
                            ReturnData.ErrMsg = "Interest Rate is not defined";

                            return ReturnData;
                        }


                    }
                    else
                    {
                        ReturnData.ReturnValue = IntRate;//value
                        ReturnData.Errno = 1; //error
                        ReturnData.ErrMsg = "Interest Policy is not defined";

                        return ReturnData;
                    }
                }
            }
        }
        #endregion PFInterestRate calculation



        #region Interest Calculation
        //PArameters
        //{Interest policy of accounting year, Interest calculation start date, Interest calculation stop date,Core Employee ID, Base Product Type for Interest identification,PF trust type Comp -0/Govt-1}
        public static ReturnData InterestCalc(int IntPolicyId, DateTime FromPeriod, DateTime ToPeriod, int Employee_Id, int InterestProduct, int TrustType, DateTime IntPostingDate, DateTime? SettlementDate)
        {
            ReturnData ReturnData = new ReturnData();
            using (DataBaseContext db = new DataBaseContext())
            {
                List<double> InterestCalc = new List<double>();
                int @empPFID;
                //  List<string> PassbookLoanIDValue = new List<string>();
                var PassbookLoanIDValue = new List<string>();
                //PassbookLoanIDValue[0] = "LOAN DEBIT POSTING";
                //PassbookLoanIDValue[1] = "LOAN CREDIT POSTING";
                //PassbookLoanIDValue[2] = "INTEREST POSTING";
                PassbookLoanIDValue.Add("LOAN DEBIT POSTING");
                PassbookLoanIDValue.Add("LOAN CREDIT POSTING");
                PassbookLoanIDValue.Add("INTEREST POSTING");
                PassbookLoanIDValue.Add("MONTHLY PF POSTING");
                PassbookLoanIDValue.Add("LOAN DEBIT BALANCE");
                List<int> PassbookLoanID = new List<int>();
                // var PFExistDateObj = db.EmployeePFTrust.Include(e => e.Employee).Include(e => e.Employee.ServiceBookDates).Where(e => e.Id == Employee_Id && e.Employee.ServiceBookDates.PFExitDate == null).SingleOrDefault();//pfexit date checked for employee live in PF trust
                //  empPFID = db.EmployeePFTrust.Where(e => e.Id == Employee_Id && PFExistDateObj != null).Select(e => e.Id).SingleOrDefault();

                PassbookLoanID = db.LookupValue.Where(e => PassbookLoanIDValue.Contains(e.LookupVal.ToUpper())).Select(e => e.Id).ToList();
                if (SettlementDate != null)
                {
                    var InterestPolicies = db.InterestPolicies.Include(e => e.InterestRate).Where(e => e.Id == IntPolicyId).SingleOrDefault();
                    // ToPeriod = ToPeriod.AddMonths(-InterestPolicies.SettlementProductmonthAdj);
                    ToPeriod = Convert.ToDateTime("01/" + (SettlementDate.Value.AddMonths(-InterestPolicies.SettlementProductmonthAdj - 1).ToString("MM/yyyy")));
                }
                // var PFTEmployeeLedgerData;
                double CloseBal = 0;
                double CurrentData = 0;
                double CalcInt = 0;
                var IntDays = 0;
                var TotalDays = 0;

                //  Loan Balance Code Start MSC bank interest posting double loan amount deduct in product
                var PassbookLoanIDValue1 = new List<string>();
                PassbookLoanIDValue1.Add("LOAN DEBIT POSTING");
                List<int> PassbookLoanID1 = new List<int>();
                double OwnPFLoan = 0;
                double OwnerPFLoan = 0;
                double VPFPFLoan = 0;
                double OwnIntPFLoan = 0;
                double OwnerIntPFLoan = 0;
                double VPFIntPFLoan = 0;
                double IntOnIntPFLoan = 0;
                PassbookLoanID1 = db.LookupValue.Where(e => PassbookLoanIDValue1.Contains(e.LookupVal.ToUpper())).Select(e => e.Id).ToList();

                if (SettlementDate == null)
                {


                    var PFTEmployeeLedgerData_01 = db.EmployeePFTrust.Where(e => e.Id == Employee_Id)
                               .Select(e => e.PFTEmployeeLedger
                               .Where(r => r.CalcDate >= FromPeriod && r.CalcDate <= ToPeriod &&
                               PassbookLoanID1.Contains(r.PassbookActivity.Id) == true).OrderByDescending(x => x.Id).ToList()).SingleOrDefault();

                    if (PFTEmployeeLedgerData_01.Count() > 0)
                    {


                        foreach (var PFTEmployeeLedgerDataitem in PFTEmployeeLedgerData_01)
                        {
                            OwnPFLoan = OwnPFLoan + PFTEmployeeLedgerDataitem.OwnPFLoan;
                            OwnerPFLoan = OwnerPFLoan + PFTEmployeeLedgerDataitem.OwnerPFLoan;
                            VPFPFLoan = VPFPFLoan + PFTEmployeeLedgerDataitem.VPFPFLoan;
                            OwnIntPFLoan = OwnIntPFLoan + PFTEmployeeLedgerDataitem.OwnIntPFLoan;
                            OwnerIntPFLoan = OwnerIntPFLoan + PFTEmployeeLedgerDataitem.OwnerIntPFLoan;
                            VPFIntPFLoan = VPFIntPFLoan + PFTEmployeeLedgerDataitem.VPFIntPFLoan;
                            IntOnIntPFLoan = IntOnIntPFLoan + PFTEmployeeLedgerDataitem.IntOnIntPFLoan;


                        }
                        OwnPFLoan = OwnPFLoan * 2;
                        OwnerPFLoan = OwnerPFLoan * 2;
                        VPFPFLoan = VPFPFLoan * 2;
                        OwnIntPFLoan = OwnIntPFLoan * 2;
                        OwnerIntPFLoan = OwnerIntPFLoan * 2;
                        VPFIntPFLoan = VPFIntPFLoan * 2;
                        IntOnIntPFLoan = IntOnIntPFLoan * 2;
                    }
                }
                else
                {
                    var PFTEmployeeLedgerData_01 = db.EmployeePFTrust.Where(e => e.Id == Employee_Id)
                              .Select(e => e.EmpSettlementPFT
                              .Where(r => r.CalcDate >= FromPeriod && r.CalcDate <= ToPeriod &&
                              PassbookLoanID1.Contains(r.PassbookActivity.Id) == true).OrderByDescending(x => x.Id).ToList()).SingleOrDefault();

                    if (PFTEmployeeLedgerData_01.Count() > 0)
                    {


                        foreach (var PFTEmployeeLedgerDataitem in PFTEmployeeLedgerData_01)
                        {
                            OwnPFLoan = OwnPFLoan + PFTEmployeeLedgerDataitem.OwnPFLoan;
                            OwnerPFLoan = OwnerPFLoan + PFTEmployeeLedgerDataitem.OwnerPFLoan;
                            VPFPFLoan = VPFPFLoan + PFTEmployeeLedgerDataitem.VPFPFLoan;
                            OwnIntPFLoan = OwnIntPFLoan + PFTEmployeeLedgerDataitem.OwnIntPFLoan;
                            OwnerIntPFLoan = OwnerIntPFLoan + PFTEmployeeLedgerDataitem.OwnerIntPFLoan;
                            VPFIntPFLoan = VPFIntPFLoan + PFTEmployeeLedgerDataitem.VPFIntPFLoan;
                            IntOnIntPFLoan = IntOnIntPFLoan + PFTEmployeeLedgerDataitem.IntOnIntPFLoan;


                        }
                        OwnPFLoan = OwnPFLoan * 2;
                        OwnerPFLoan = OwnerPFLoan * 2;
                        VPFPFLoan = VPFPFLoan * 2;
                        OwnIntPFLoan = OwnIntPFLoan * 2;
                        OwnerIntPFLoan = OwnerIntPFLoan * 2;
                        VPFIntPFLoan = VPFIntPFLoan * 2;
                        IntOnIntPFLoan = IntOnIntPFLoan * 2;
                    }


                }
                //  Loan Balance Code end

                switch (InterestProduct)
                {
                    case 0: //--0 : OWN Employee
                        if (SettlementDate == null)
                        {
                            var PFTEmployeeLedgerData_0 = db.EmployeePFTrust.Where(e => e.Id == Employee_Id)
                                .Select(e => e.PFTEmployeeLedger
                                .Where(r => r.CalcDate >= FromPeriod && r.CalcDate <= ToPeriod &&
                                PassbookLoanID.Contains(r.PassbookActivity.Id) == false).OrderByDescending(x => x.Id).ToList()).SingleOrDefault();
                            foreach (var PFTEmployeeLedgerDataitem in PFTEmployeeLedgerData_0)
                            {
                                CloseBal = CloseBal + PFTEmployeeLedgerDataitem.OwnCloseBal;
                                //CurrentData = PFTEmployeeLedgerData.Sum(e => e.OwnPFMonthly + e.OwnPFInt);

                            }
                        }
                        else
                        {
                            var PFTEmployeeLedgerData_0 = db.EmployeePFTrust.Where(e => e.Id == Employee_Id)
                                                            .Select(e => e.EmpSettlementPFT
                                                            .Where(r => r.CalcDate >= FromPeriod && r.CalcDate <= ToPeriod &&
                                                            PassbookLoanID.Contains(r.PassbookActivity.Id) == false).OrderByDescending(x => x.Id).ToList()).SingleOrDefault();
                            foreach (var PFTEmployeeLedgerDataitem in PFTEmployeeLedgerData_0)
                            {
                                CloseBal = CloseBal + PFTEmployeeLedgerDataitem.OwnCloseBal;
                                //CurrentData = PFTEmployeeLedgerData.Sum(e => e.OwnPFMonthly + e.OwnPFInt);

                            }
                        }
                        CloseBal = CloseBal - OwnPFLoan;
                        ReturnData = InterestRate(IntPolicyId, IntPostingDate, InterestProduct, TrustType);
                        if (ReturnData.Errno == 0)
                        {
                            CalcInt = CalcInt + (CloseBal * (ReturnData.ReturnValue / (12 * 100)));
                            CalcInt = Math.Round(CalcInt + 0.001, 0);
                            ReturnData.ReturnValue = CalcInt;
                            ReturnData.Errno = 0;
                            return ReturnData;
                        }
                        else
                        {
                            return ReturnData;
                        }

                        break;
                    case 1://--1 : Owner Employer
                        if (SettlementDate == null)
                        {
                            var PFTEmployeeLedgerData_1 = db.EmployeePFTrust.Where(e => e.Id == Employee_Id)
                                .Select(e => e.PFTEmployeeLedger
                                .Where(r => r.CalcDate >= FromPeriod && r.CalcDate <= ToPeriod &&
                                PassbookLoanID.Contains(r.PassbookActivity.Id) == false).OrderByDescending(x => x.Id).ToList()).SingleOrDefault();
                            //CloseBal = PFTEmployeeLedgerData.OrderByDescending(e => e.Id).FirstOrDefault().OwnerCloseBal;
                            //CurrentData = PFTEmployeeLedgerData.Sum(e => e.OwnerPFMonthly + e.OwnerPFInt);
                            //ReturnData = InterestRate(IntPolicyId, IntPostingDate, InterestProduct, TrustType);
                            foreach (var PFTEmployeeLedgerDataitem in PFTEmployeeLedgerData_1)
                            {
                                CloseBal = CloseBal + PFTEmployeeLedgerDataitem.OwnerCloseBal;
                                //CurrentData = PFTEmployeeLedgerData.Sum(e => e.OwnPFMonthly + e.OwnPFInt);

                            }
                        }
                        else
                        {
                            var PFTEmployeeLedgerData_1 = db.EmployeePFTrust.Where(e => e.Id == Employee_Id)
                                                            .Select(e => e.EmpSettlementPFT
                                                            .Where(r => r.CalcDate >= FromPeriod && r.CalcDate <= ToPeriod &&
                                                            PassbookLoanID.Contains(r.PassbookActivity.Id) == false).OrderByDescending(x => x.Id).ToList()).SingleOrDefault();
                            //CloseBal = PFTEmployeeLedgerData.OrderByDescending(e => e.Id).FirstOrDefault().OwnerCloseBal;
                            //CurrentData = PFTEmployeeLedgerData.Sum(e => e.OwnerPFMonthly + e.OwnerPFInt);
                            //ReturnData = InterestRate(IntPolicyId, IntPostingDate, InterestProduct, TrustType);
                            foreach (var PFTEmployeeLedgerDataitem in PFTEmployeeLedgerData_1)
                            {
                                CloseBal = CloseBal + PFTEmployeeLedgerDataitem.OwnerCloseBal;
                                //CurrentData = PFTEmployeeLedgerData.Sum(e => e.OwnPFMonthly + e.OwnPFInt);

                            }
                        }
                        CloseBal = CloseBal - OwnerPFLoan;
                        ReturnData = InterestRate(IntPolicyId, IntPostingDate, InterestProduct, TrustType);
                        if (ReturnData.Errno == 0)
                        {
                            CalcInt = CalcInt + (CloseBal * (ReturnData.ReturnValue / (12 * 100)));
                            CalcInt = Math.Round(CalcInt + 0.001, 0);
                            ReturnData.ReturnValue = CalcInt;
                            ReturnData.Errno = 0;
                            return ReturnData;
                        }
                        else
                        {
                            return ReturnData;
                        }
                        break;
                    case 2://--2 : VPF Employee VPF
                        if (SettlementDate == null)
                        {
                            var PFTEmployeeLedgerData_2 = db.EmployeePFTrust.Where(e => e.Id == Employee_Id)
                               .Select(e => e.PFTEmployeeLedger
                               .Where(r => r.CalcDate >= FromPeriod && r.CalcDate <= ToPeriod &&
                               PassbookLoanID.Contains(r.PassbookActivity.Id) == false).OrderByDescending(x => x.Id).ToList()).SingleOrDefault();
                            //CloseBal = PFTEmployeeLedgerData.OrderByDescending(e => e.Id).FirstOrDefault().VPFCloseBal;
                            //CurrentData = PFTEmployeeLedgerData.Sum(e => e.VPFPFMonthly + e.VPFPFInt);
                            //ReturnData = InterestRate(IntPolicyId, IntPostingDate, InterestProduct, TrustType);
                            foreach (var PFTEmployeeLedgerDataitem in PFTEmployeeLedgerData_2)
                            {
                                CloseBal = CloseBal + PFTEmployeeLedgerDataitem.VPFCloseBal;
                                //CurrentData = PFTEmployeeLedgerData.Sum(e => e.OwnPFMonthly + e.OwnPFInt);

                            }
                        }
                        else
                        {
                            var PFTEmployeeLedgerData_2 = db.EmployeePFTrust.Where(e => e.Id == Employee_Id)
                              .Select(e => e.EmpSettlementPFT
                              .Where(r => r.CalcDate >= FromPeriod && r.CalcDate <= ToPeriod &&
                              PassbookLoanID.Contains(r.PassbookActivity.Id) == false).OrderByDescending(x => x.Id).ToList()).SingleOrDefault();
                            //CloseBal = PFTEmployeeLedgerData.OrderByDescending(e => e.Id).FirstOrDefault().VPFCloseBal;
                            //CurrentData = PFTEmployeeLedgerData.Sum(e => e.VPFPFMonthly + e.VPFPFInt);
                            //ReturnData = InterestRate(IntPolicyId, IntPostingDate, InterestProduct, TrustType);
                            foreach (var PFTEmployeeLedgerDataitem in PFTEmployeeLedgerData_2)
                            {
                                CloseBal = CloseBal + PFTEmployeeLedgerDataitem.VPFCloseBal;
                                //CurrentData = PFTEmployeeLedgerData.Sum(e => e.OwnPFMonthly + e.OwnPFInt);

                            }
                        }
                        CloseBal = CloseBal - VPFPFLoan;
                        ReturnData = InterestRate(IntPolicyId, IntPostingDate, InterestProduct, TrustType);
                        if (ReturnData.Errno == 0)
                        {
                            CalcInt = CalcInt + (CloseBal * (ReturnData.ReturnValue / (12 * 100)));
                            CalcInt = Math.Round(CalcInt + 0.001, 0);
                            ReturnData.ReturnValue = CalcInt;
                            ReturnData.Errno = 0;
                            return ReturnData;
                        }
                        else
                        {
                            return ReturnData;
                        }
                        break;
                    case 3://--3 : PF
                        if (SettlementDate == null)
                        {
                            var PFTEmployeeLedgerData_3 = db.EmployeePFTrust.Where(e => e.Id == Employee_Id)
                                  .Select(e => e.PFTEmployeeLedger
                                  .Where(r => r.CalcDate >= FromPeriod && r.CalcDate <= ToPeriod &&
                                  PassbookLoanID.Contains(r.PassbookActivity.Id) == false).OrderByDescending(x => x.Id).ToList()).SingleOrDefault();
                            //CloseBal = PFTEmployeeLedgerData.OrderByDescending(e => e.Id).FirstOrDefault().PFCloseBal;
                            //CurrentData = PFTEmployeeLedgerData.Sum(e => e.PFAmountMonthly + e.PFInt);
                            //ReturnData = InterestRate(IntPolicyId, IntPostingDate, InterestProduct, TrustType);
                            foreach (var PFTEmployeeLedgerDataitem in PFTEmployeeLedgerData_3)
                            {
                                CloseBal = CloseBal + PFTEmployeeLedgerDataitem.PFCloseBal;
                                //CurrentData = PFTEmployeeLedgerData.Sum(e => e.OwnPFMonthly + e.OwnPFInt);

                            }
                        }
                        else
                        {
                            var PFTEmployeeLedgerData_3 = db.EmployeePFTrust.Where(e => e.Id == Employee_Id)
                                                              .Select(e => e.EmpSettlementPFT
                                                              .Where(r => r.CalcDate >= FromPeriod && r.CalcDate <= ToPeriod &&
                                                              PassbookLoanID.Contains(r.PassbookActivity.Id) == false).OrderByDescending(x => x.Id).ToList()).SingleOrDefault();
                            //CloseBal = PFTEmployeeLedgerData.OrderByDescending(e => e.Id).FirstOrDefault().PFCloseBal;
                            //CurrentData = PFTEmployeeLedgerData.Sum(e => e.PFAmountMonthly + e.PFInt);
                            //ReturnData = InterestRate(IntPolicyId, IntPostingDate, InterestProduct, TrustType);
                            foreach (var PFTEmployeeLedgerDataitem in PFTEmployeeLedgerData_3)
                            {
                                CloseBal = CloseBal + PFTEmployeeLedgerDataitem.PFCloseBal;
                                //CurrentData = PFTEmployeeLedgerData.Sum(e => e.OwnPFMonthly + e.OwnPFInt);

                            }
                        }
                        CloseBal = CloseBal - OwnPFLoan - OwnerPFLoan;
                        ReturnData = InterestRate(IntPolicyId, IntPostingDate, InterestProduct, TrustType);
                        if (ReturnData.Errno == 0)
                        {
                            CalcInt = CalcInt + (CloseBal * (ReturnData.ReturnValue / (12 * 100)));
                            CalcInt = Math.Round(CalcInt + 0.001, 0);
                            ReturnData.ReturnValue = CalcInt;
                            ReturnData.Errno = 0;
                            return ReturnData;
                        }
                        else
                        {
                            return ReturnData;
                        }
                        break;
                    case 4://--4 : Total pf Contribution
                        if (SettlementDate == null)
                        {
                            var PFTEmployeeLedgerData_4 = db.EmployeePFTrust.Where(e => e.Id == Employee_Id)
                                 .Select(e => e.PFTEmployeeLedger
                                 .Where(r => r.CalcDate >= FromPeriod && r.CalcDate <= ToPeriod &&
                                 PassbookLoanID.Contains(r.PassbookActivity.Id) == false).OrderByDescending(x => x.Id).ToList()).SingleOrDefault();

                            //CloseBal = PFTEmployeeLedgerData.OrderByDescending(e => e.Id).FirstOrDefault().PFCloseBal;
                            //CurrentData = PFTEmployeeLedgerData.Sum(e => e.TotalPFAmountMonthly + e.TotalPFInt);
                            //ReturnData = InterestRate(IntPolicyId, IntPostingDate, InterestProduct, TrustType);
                            foreach (var PFTEmployeeLedgerDataitem in PFTEmployeeLedgerData_4)
                            {
                                CloseBal = CloseBal + PFTEmployeeLedgerDataitem.PFCloseBal + PFTEmployeeLedgerDataitem.VPFCloseBal;
                                //CurrentData = PFTEmployeeLedgerData.Sum(e => e.OwnPFMonthly + e.OwnPFInt);


                            }
                        }
                        else
                        {

                            var PFTEmployeeLedgerData_4 = db.EmployeePFTrust.Where(e => e.Id == Employee_Id)
                                 .Select(e => e.EmpSettlementPFT
                                 .Where(r => r.CalcDate >= FromPeriod && r.CalcDate <= ToPeriod &&
                                 PassbookLoanID.Contains(r.PassbookActivity.Id) == false).OrderByDescending(x => x.Id).ToList()).SingleOrDefault();

                            //CloseBal = PFTEmployeeLedgerData.OrderByDescending(e => e.Id).FirstOrDefault().PFCloseBal;
                            //CurrentData = PFTEmployeeLedgerData.Sum(e => e.TotalPFAmountMonthly + e.TotalPFInt);
                            //ReturnData = InterestRate(IntPolicyId, IntPostingDate, InterestProduct, TrustType);
                            foreach (var PFTEmployeeLedgerDataitem in PFTEmployeeLedgerData_4)
                            {
                                CloseBal = CloseBal + PFTEmployeeLedgerDataitem.PFCloseBal + PFTEmployeeLedgerDataitem.VPFCloseBal;
                                //CurrentData = PFTEmployeeLedgerData.Sum(e => e.OwnPFMonthly + e.OwnPFInt);


                            }
                        }
                        CloseBal = CloseBal - OwnPFLoan - OwnerPFLoan - VPFPFLoan;

                        ReturnData = InterestRate(IntPolicyId, IntPostingDate, InterestProduct, TrustType);
                        if (ReturnData.Errno == 0)
                        {
                            CalcInt = CalcInt + (CloseBal * (ReturnData.ReturnValue / (12 * 100)));
                            CalcInt = Math.Round(CalcInt + 0.001, 0);
                            ReturnData.ReturnValue = CalcInt;
                            ReturnData.Errno = 0;
                            return ReturnData;
                        }
                        else
                        {
                            return ReturnData;
                        }
                        break;
                    case 5://--4 : Total int Contribution
                        if (SettlementDate == null)
                        {
                            var PFTEmployeeLedgerData_5 = db.EmployeePFTrust.Where(e => e.Id == Employee_Id)
                                 .Select(e => e.PFTEmployeeLedger
                                 .Where(r => r.CalcDate >= FromPeriod && r.CalcDate <= ToPeriod &&
                                 PassbookLoanID.Contains(r.PassbookActivity.Id) == false).OrderByDescending(x => x.Id).ToList()).SingleOrDefault();

                            //CloseBal = PFTEmployeeLedgerData.OrderByDescending(e => e.Id).FirstOrDefault().PFCloseBal;
                            //CurrentData = PFTEmployeeLedgerData.Sum(e => e.TotalPFAmountMonthly + e.TotalPFInt);
                            //ReturnData = InterestRate(IntPolicyId, IntPostingDate, InterestProduct, TrustType);
                            foreach (var PFTEmployeeLedgerDataitem in PFTEmployeeLedgerData_5)
                            {
                                CloseBal = CloseBal + PFTEmployeeLedgerDataitem.TotalIntCloseBal;
                                //CurrentData = PFTEmployeeLedgerData.Sum(e => e.OwnPFMonthly + e.OwnPFInt);

                            }
                        }
                        else
                        {
                            var PFTEmployeeLedgerData_5 = db.EmployeePFTrust.Where(e => e.Id == Employee_Id)
                                 .Select(e => e.EmpSettlementPFT
                                 .Where(r => r.CalcDate >= FromPeriod && r.CalcDate <= ToPeriod &&
                                 PassbookLoanID.Contains(r.PassbookActivity.Id) == false).OrderByDescending(x => x.Id).ToList()).SingleOrDefault();

                            //CloseBal = PFTEmployeeLedgerData.OrderByDescending(e => e.Id).FirstOrDefault().PFCloseBal;
                            //CurrentData = PFTEmployeeLedgerData.Sum(e => e.TotalPFAmountMonthly + e.TotalPFInt);
                            //ReturnData = InterestRate(IntPolicyId, IntPostingDate, InterestProduct, TrustType);
                            foreach (var PFTEmployeeLedgerDataitem in PFTEmployeeLedgerData_5)
                            {
                                CloseBal = CloseBal + PFTEmployeeLedgerDataitem.TotalIntCloseBal;
                                //CurrentData = PFTEmployeeLedgerData.Sum(e => e.OwnPFMonthly + e.OwnPFInt);

                            }
                        }
                        CloseBal = CloseBal - OwnIntPFLoan - OwnerIntPFLoan - VPFIntPFLoan;
                        ReturnData = InterestRate(IntPolicyId, IntPostingDate, InterestProduct, TrustType);
                        if (ReturnData.Errno == 0)
                        {
                            CalcInt = CalcInt + (CloseBal * (ReturnData.ReturnValue / (12 * 100)));
                            CalcInt = Math.Round(CalcInt + 0.001, 0);
                            ReturnData.ReturnValue = CalcInt;
                            ReturnData.Errno = 0;
                            return ReturnData;
                        }
                        else
                        {
                            return ReturnData;
                        }
                        break;
                    case 6://--4 : own interest on interest
                        if (SettlementDate == null)
                        {
                            var PFTEmployeeLedgerData_6 = db.EmployeePFTrust.Where(e => e.Id == Employee_Id)
                                 .Select(e => e.PFTEmployeeLedger
                                 .Where(r => r.CalcDate >= FromPeriod && r.CalcDate <= ToPeriod &&
                                 PassbookLoanID.Contains(r.PassbookActivity.Id) == false).OrderByDescending(x => x.Id).ToList()).SingleOrDefault();

                            //CloseBal = PFTEmployeeLedgerData.OrderByDescending(e => e.Id).FirstOrDefault().PFCloseBal;
                            //CurrentData = PFTEmployeeLedgerData.Sum(e => e.TotalPFAmountMonthly + e.TotalPFInt);
                            //ReturnData = InterestRate(IntPolicyId, IntPostingDate, InterestProduct, TrustType);
                            foreach (var PFTEmployeeLedgerDataitem in PFTEmployeeLedgerData_6)
                            {
                                CloseBal = CloseBal + PFTEmployeeLedgerDataitem.OwnIntCloseBal;
                                //CurrentData = PFTEmployeeLedgerData.Sum(e => e.OwnPFMonthly + e.OwnPFInt);

                            }
                        }
                        else
                        {
                            var PFTEmployeeLedgerData_6 = db.EmployeePFTrust.Where(e => e.Id == Employee_Id)
                                                             .Select(e => e.EmpSettlementPFT
                                                             .Where(r => r.CalcDate >= FromPeriod && r.CalcDate <= ToPeriod &&
                                                             PassbookLoanID.Contains(r.PassbookActivity.Id) == false).OrderByDescending(x => x.Id).ToList()).SingleOrDefault();

                            //CloseBal = PFTEmployeeLedgerData.OrderByDescending(e => e.Id).FirstOrDefault().PFCloseBal;
                            //CurrentData = PFTEmployeeLedgerData.Sum(e => e.TotalPFAmountMonthly + e.TotalPFInt);
                            //ReturnData = InterestRate(IntPolicyId, IntPostingDate, InterestProduct, TrustType);
                            foreach (var PFTEmployeeLedgerDataitem in PFTEmployeeLedgerData_6)
                            {
                                CloseBal = CloseBal + PFTEmployeeLedgerDataitem.OwnIntCloseBal;
                                //CurrentData = PFTEmployeeLedgerData.Sum(e => e.OwnPFMonthly + e.OwnPFInt);

                            }
                        }
                        CloseBal = CloseBal - OwnIntPFLoan;
                        ReturnData = InterestRate(IntPolicyId, IntPostingDate, InterestProduct, TrustType);
                        if (ReturnData.Errno == 0)
                        {
                            CalcInt = CalcInt + (CloseBal * (ReturnData.ReturnValue / (12 * 100)));
                            CalcInt = Math.Round(CalcInt + 0.001, 0);
                            ReturnData.ReturnValue = CalcInt;
                            ReturnData.Errno = 0;
                            return ReturnData;
                        }
                        else
                        {
                            return ReturnData;
                        }
                        break;
                    case 7://--4 : owner interest on interest
                        if (SettlementDate == null)
                        {
                            var PFTEmployeeLedgerData_7 = db.EmployeePFTrust.Where(e => e.Id == Employee_Id)
                                 .Select(e => e.PFTEmployeeLedger
                                 .Where(r => r.CalcDate >= FromPeriod && r.CalcDate <= ToPeriod &&
                                 PassbookLoanID.Contains(r.PassbookActivity.Id) == false).OrderByDescending(x => x.Id).ToList()).SingleOrDefault();

                            //CloseBal = PFTEmployeeLedgerData.OrderByDescending(e => e.Id).FirstOrDefault().PFCloseBal;
                            //CurrentData = PFTEmployeeLedgerData.Sum(e => e.TotalPFAmountMonthly + e.TotalPFInt);
                            //ReturnData = InterestRate(IntPolicyId, IntPostingDate, InterestProduct, TrustType);
                            foreach (var PFTEmployeeLedgerDataitem in PFTEmployeeLedgerData_7)
                            {
                                CloseBal = CloseBal + PFTEmployeeLedgerDataitem.OwnerIntCloseBal;
                                //CurrentData = PFTEmployeeLedgerData.Sum(e => e.OwnPFMonthly + e.OwnPFInt);

                            }
                        }
                        else
                        {
                            var PFTEmployeeLedgerData_7 = db.EmployeePFTrust.Where(e => e.Id == Employee_Id)
                                                            .Select(e => e.EmpSettlementPFT
                                                            .Where(r => r.CalcDate >= FromPeriod && r.CalcDate <= ToPeriod &&
                                                            PassbookLoanID.Contains(r.PassbookActivity.Id) == false).OrderByDescending(x => x.Id).ToList()).SingleOrDefault();

                            //CloseBal = PFTEmployeeLedgerData.OrderByDescending(e => e.Id).FirstOrDefault().PFCloseBal;
                            //CurrentData = PFTEmployeeLedgerData.Sum(e => e.TotalPFAmountMonthly + e.TotalPFInt);
                            //ReturnData = InterestRate(IntPolicyId, IntPostingDate, InterestProduct, TrustType);
                            foreach (var PFTEmployeeLedgerDataitem in PFTEmployeeLedgerData_7)
                            {
                                CloseBal = CloseBal + PFTEmployeeLedgerDataitem.OwnerIntCloseBal;
                                //CurrentData = PFTEmployeeLedgerData.Sum(e => e.OwnPFMonthly + e.OwnPFInt);

                            }
                        }
                        CloseBal = CloseBal - OwnerIntPFLoan;
                        ReturnData = InterestRate(IntPolicyId, IntPostingDate, InterestProduct, TrustType);
                        if (ReturnData.Errno == 0)
                        {
                            CalcInt = CalcInt + (CloseBal * (ReturnData.ReturnValue / (12 * 100)));
                            CalcInt = Math.Round(CalcInt + 0.001, 0);
                            ReturnData.ReturnValue = CalcInt;
                            ReturnData.Errno = 0;
                            return ReturnData;
                        }
                        else
                        {
                            return ReturnData;
                        }
                        break;
                    case 8://--4 : Vpf interest on interest
                        if (SettlementDate == null)
                        {
                            var PFTEmployeeLedgerData_8 = db.EmployeePFTrust.Where(e => e.Id == Employee_Id)
                                 .Select(e => e.PFTEmployeeLedger
                                 .Where(r => r.CalcDate >= FromPeriod && r.CalcDate <= ToPeriod &&
                                 PassbookLoanID.Contains(r.PassbookActivity.Id) == false).OrderByDescending(x => x.Id).ToList()).SingleOrDefault();

                            //CloseBal = PFTEmployeeLedgerData.OrderByDescending(e => e.Id).FirstOrDefault().PFCloseBal;
                            //CurrentData = PFTEmployeeLedgerData.Sum(e => e.TotalPFAmountMonthly + e.TotalPFInt);
                            //ReturnData = InterestRate(IntPolicyId, IntPostingDate, InterestProduct, TrustType);
                            foreach (var PFTEmployeeLedgerDataitem in PFTEmployeeLedgerData_8)
                            {
                                CloseBal = CloseBal + PFTEmployeeLedgerDataitem.VPFIntCloseBal;
                                //CurrentData = PFTEmployeeLedgerData.Sum(e => e.OwnPFMonthly + e.OwnPFInt);

                            }
                        }
                        else
                        {
                            var PFTEmployeeLedgerData_8 = db.EmployeePFTrust.Where(e => e.Id == Employee_Id)
                               .Select(e => e.EmpSettlementPFT
                               .Where(r => r.CalcDate >= FromPeriod && r.CalcDate <= ToPeriod &&
                               PassbookLoanID.Contains(r.PassbookActivity.Id) == false).OrderByDescending(x => x.Id).ToList()).SingleOrDefault();

                            //CloseBal = PFTEmployeeLedgerData.OrderByDescending(e => e.Id).FirstOrDefault().PFCloseBal;
                            //CurrentData = PFTEmployeeLedgerData.Sum(e => e.TotalPFAmountMonthly + e.TotalPFInt);
                            //ReturnData = InterestRate(IntPolicyId, IntPostingDate, InterestProduct, TrustType);
                            foreach (var PFTEmployeeLedgerDataitem in PFTEmployeeLedgerData_8)
                            {
                                CloseBal = CloseBal + PFTEmployeeLedgerDataitem.VPFIntCloseBal;
                                //CurrentData = PFTEmployeeLedgerData.Sum(e => e.OwnPFMonthly + e.OwnPFInt);

                            }
                        }
                        CloseBal = CloseBal - VPFIntPFLoan;
                        ReturnData = InterestRate(IntPolicyId, IntPostingDate, InterestProduct, TrustType);
                        if (ReturnData.Errno == 0)
                        {
                            CalcInt = CalcInt + (CloseBal * (ReturnData.ReturnValue / (12 * 100)));
                            CalcInt = Math.Round(CalcInt + 0.001, 0);
                            ReturnData.ReturnValue = CalcInt;
                            ReturnData.Errno = 0;
                            return ReturnData;
                        }
                        else
                        {
                            return ReturnData;
                        }
                        break;
                    case 9://--4 : Taxableinterest
                        if (SettlementDate == null)
                        {
                            var PFTEmployeeLedgerData_9 = db.EmployeePFTrust.Where(e => e.Id == Employee_Id)
                                 .Select(e => e.PFTEmployeeLedger
                                 .Where(r => r.CalcDate >= FromPeriod && r.CalcDate <= ToPeriod &&
                                 PassbookLoanID.Contains(r.PassbookActivity.Id) == false).OrderByDescending(x => x.Id).ToList()).SingleOrDefault();

                            //CloseBal = PFTEmployeeLedgerData.OrderByDescending(e => e.Id).FirstOrDefault().PFCloseBal;
                            //CurrentData = PFTEmployeeLedgerData.Sum(e => e.TotalPFAmountMonthly + e.TotalPFInt);
                            //ReturnData = InterestRate(IntPolicyId, IntPostingDate, InterestProduct, TrustType);
                            foreach (var PFTEmployeeLedgerDataitem in PFTEmployeeLedgerData_9)
                            {
                                CloseBal = CloseBal + PFTEmployeeLedgerDataitem.TaxableAccountInterest;
                                break;

                            }
                        }
                        else
                        {
                            var PFTEmployeeLedgerData_9 = db.EmployeePFTrust.Where(e => e.Id == Employee_Id)
                                                             .Select(e => e.EmpSettlementPFT
                                                             .Where(r => r.CalcDate >= FromPeriod && r.CalcDate <= ToPeriod &&
                                                             PassbookLoanID.Contains(r.PassbookActivity.Id) == false).OrderByDescending(x => x.Id).ToList()).SingleOrDefault();

                            //CloseBal = PFTEmployeeLedgerData.OrderByDescending(e => e.Id).FirstOrDefault().PFCloseBal;
                            //CurrentData = PFTEmployeeLedgerData.Sum(e => e.TotalPFAmountMonthly + e.TotalPFInt);
                            //ReturnData = InterestRate(IntPolicyId, IntPostingDate, InterestProduct, TrustType);
                            foreach (var PFTEmployeeLedgerDataitem in PFTEmployeeLedgerData_9)
                            {
                                CloseBal = CloseBal + PFTEmployeeLedgerDataitem.TaxableAccountInterest;
                                break;

                            }
                        }
                        ReturnData = InterestRate(IntPolicyId, IntPostingDate, InterestProduct, TrustType);
                        if (ReturnData.Errno == 0)
                        {
                            CalcInt = CalcInt + CloseBal;
                            CalcInt = Math.Round(CalcInt + 0.001, 0);
                            ReturnData.ReturnValue = CalcInt;
                            ReturnData.Errno = 0;
                            return ReturnData;
                        }
                        else
                        {
                            return ReturnData;
                        }
                        break;
                    case 10://--4 : NonTaxableinterest
                        if (SettlementDate == null)
                        {
                            var PFTEmployeeLedgerData_10 = db.EmployeePFTrust.Where(e => e.Id == Employee_Id)
                                 .Select(e => e.PFTEmployeeLedger
                                 .Where(r => r.CalcDate >= FromPeriod && r.CalcDate <= ToPeriod &&
                                 PassbookLoanID.Contains(r.PassbookActivity.Id) == false).OrderByDescending(x => x.Id).ToList()).SingleOrDefault();

                            //CloseBal = PFTEmployeeLedgerData.OrderByDescending(e => e.Id).FirstOrDefault().PFCloseBal;
                            //CurrentData = PFTEmployeeLedgerData.Sum(e => e.TotalPFAmountMonthly + e.TotalPFInt);
                            //ReturnData = InterestRate(IntPolicyId, IntPostingDate, InterestProduct, TrustType);
                            foreach (var PFTEmployeeLedgerDataitem in PFTEmployeeLedgerData_10)
                            {
                                CloseBal = CloseBal + PFTEmployeeLedgerDataitem.NonTaxableAccountInterest;
                                break;

                            }
                        }
                        else
                        {
                            var PFTEmployeeLedgerData_10 = db.EmployeePFTrust.Where(e => e.Id == Employee_Id)
                                .Select(e => e.EmpSettlementPFT
                                .Where(r => r.CalcDate >= FromPeriod && r.CalcDate <= ToPeriod &&
                                PassbookLoanID.Contains(r.PassbookActivity.Id) == false).OrderByDescending(x => x.Id).ToList()).SingleOrDefault();

                            //CloseBal = PFTEmployeeLedgerData.OrderByDescending(e => e.Id).FirstOrDefault().PFCloseBal;
                            //CurrentData = PFTEmployeeLedgerData.Sum(e => e.TotalPFAmountMonthly + e.TotalPFInt);
                            //ReturnData = InterestRate(IntPolicyId, IntPostingDate, InterestProduct, TrustType);
                            foreach (var PFTEmployeeLedgerDataitem in PFTEmployeeLedgerData_10)
                            {
                                CloseBal = CloseBal + PFTEmployeeLedgerDataitem.NonTaxableAccountInterest;
                                break;

                            }
                        }
                        ReturnData = InterestRate(IntPolicyId, IntPostingDate, InterestProduct, TrustType);
                        if (ReturnData.Errno == 0)
                        {
                            CalcInt = CalcInt + CloseBal;
                            CalcInt = Math.Round(CalcInt + 0.001, 0);
                            ReturnData.ReturnValue = CalcInt;
                            ReturnData.Errno = 0;
                            return ReturnData;
                        }
                        else
                        {
                            return ReturnData;
                        }
                        break;
                    case 11://--4 : own interest on interest
                        if (SettlementDate == null)
                        {
                            var PFTEmployeeLedgerData_11 = db.EmployeePFTrust.Where(e => e.Id == Employee_Id)
                                 .Select(e => e.PFTEmployeeLedger
                                 .Where(r => r.CalcDate >= FromPeriod && r.CalcDate <= ToPeriod &&
                                 PassbookLoanID.Contains(r.PassbookActivity.Id) == false).OrderByDescending(x => x.Id).ToList()).SingleOrDefault();

                            //CloseBal = PFTEmployeeLedgerData.OrderByDescending(e => e.Id).FirstOrDefault().PFCloseBal;
                            //CurrentData = PFTEmployeeLedgerData.Sum(e => e.TotalPFAmountMonthly + e.TotalPFInt);
                            //ReturnData = InterestRate(IntPolicyId, IntPostingDate, InterestProduct, TrustType);
                            foreach (var PFTEmployeeLedgerDataitem in PFTEmployeeLedgerData_11)
                            {
                                CloseBal = CloseBal + PFTEmployeeLedgerDataitem.OwnIntCloseBal;


                            }
                        }
                        else
                        {
                            var PFTEmployeeLedgerData_11 = db.EmployeePFTrust.Where(e => e.Id == Employee_Id)
                                .Select(e => e.EmpSettlementPFT
                                .Where(r => r.CalcDate >= FromPeriod && r.CalcDate <= ToPeriod &&
                                PassbookLoanID.Contains(r.PassbookActivity.Id) == false).OrderByDescending(x => x.Id).ToList()).SingleOrDefault();

                            //CloseBal = PFTEmployeeLedgerData.OrderByDescending(e => e.Id).FirstOrDefault().PFCloseBal;
                            //CurrentData = PFTEmployeeLedgerData.Sum(e => e.TotalPFAmountMonthly + e.TotalPFInt);
                            //ReturnData = InterestRate(IntPolicyId, IntPostingDate, InterestProduct, TrustType);
                            foreach (var PFTEmployeeLedgerDataitem in PFTEmployeeLedgerData_11)
                            {
                                CloseBal = CloseBal + PFTEmployeeLedgerDataitem.OwnIntCloseBal;


                            }
                        }
                        CloseBal = CloseBal - OwnIntPFLoan;
                        ReturnData = InterestRate(IntPolicyId, IntPostingDate, InterestProduct, TrustType);
                        if (ReturnData.Errno == 0)
                        {
                            CalcInt = CalcInt + (CloseBal * (ReturnData.ReturnValue / (12 * 100)));
                            CalcInt = Math.Round(CalcInt + 0.001, 0);
                            ReturnData.ReturnValue = CalcInt;
                            ReturnData.Errno = 0;
                            return ReturnData;
                        }
                        else
                        {
                            return ReturnData;
                        }
                        break;
                    case 12://--4 : owner interest on interest
                        if (SettlementDate == null)
                        {
                            var PFTEmployeeLedgerData_12 = db.EmployeePFTrust.Where(e => e.Id == Employee_Id)
                                 .Select(e => e.PFTEmployeeLedger
                                 .Where(r => r.CalcDate >= FromPeriod && r.CalcDate <= ToPeriod &&
                                 PassbookLoanID.Contains(r.PassbookActivity.Id) == false).OrderByDescending(x => x.Id).ToList()).SingleOrDefault();

                            //CloseBal = PFTEmployeeLedgerData.OrderByDescending(e => e.Id).FirstOrDefault().PFCloseBal;
                            //CurrentData = PFTEmployeeLedgerData.Sum(e => e.TotalPFAmountMonthly + e.TotalPFInt);
                            //ReturnData = InterestRate(IntPolicyId, IntPostingDate, InterestProduct, TrustType);
                            foreach (var PFTEmployeeLedgerDataitem in PFTEmployeeLedgerData_12)
                            {
                                CloseBal = CloseBal + PFTEmployeeLedgerDataitem.OwnerIntCloseBal;


                            }
                        }
                        else
                        {
                            var PFTEmployeeLedgerData_12 = db.EmployeePFTrust.Where(e => e.Id == Employee_Id)
                               .Select(e => e.EmpSettlementPFT
                               .Where(r => r.CalcDate >= FromPeriod && r.CalcDate <= ToPeriod &&
                               PassbookLoanID.Contains(r.PassbookActivity.Id) == false).OrderByDescending(x => x.Id).ToList()).SingleOrDefault();

                            //CloseBal = PFTEmployeeLedgerData.OrderByDescending(e => e.Id).FirstOrDefault().PFCloseBal;
                            //CurrentData = PFTEmployeeLedgerData.Sum(e => e.TotalPFAmountMonthly + e.TotalPFInt);
                            //ReturnData = InterestRate(IntPolicyId, IntPostingDate, InterestProduct, TrustType);
                            foreach (var PFTEmployeeLedgerDataitem in PFTEmployeeLedgerData_12)
                            {
                                CloseBal = CloseBal + PFTEmployeeLedgerDataitem.OwnerIntCloseBal;


                            }
                        }
                        CloseBal = CloseBal - OwnerIntPFLoan;
                        ReturnData = InterestRate(IntPolicyId, IntPostingDate, InterestProduct, TrustType);
                        if (ReturnData.Errno == 0)
                        {
                            CalcInt = CalcInt + (CloseBal * (ReturnData.ReturnValue / (12 * 100)));
                            CalcInt = Math.Round(CalcInt + 0.001, 0);
                            ReturnData.ReturnValue = CalcInt;
                            ReturnData.Errno = 0;
                            return ReturnData;
                        }
                        else
                        {
                            return ReturnData;
                        }
                        break;
                    case 13://--4 : VPF interest on interest
                        if (SettlementDate == null)
                        {
                            var PFTEmployeeLedgerData_13 = db.EmployeePFTrust.Where(e => e.Id == Employee_Id)
                                 .Select(e => e.PFTEmployeeLedger
                                 .Where(r => r.CalcDate >= FromPeriod && r.CalcDate <= ToPeriod &&
                                 PassbookLoanID.Contains(r.PassbookActivity.Id) == false).OrderByDescending(x => x.Id).ToList()).SingleOrDefault();

                            //CloseBal = PFTEmployeeLedgerData.OrderByDescending(e => e.Id).FirstOrDefault().PFCloseBal;
                            //CurrentData = PFTEmployeeLedgerData.Sum(e => e.TotalPFAmountMonthly + e.TotalPFInt);
                            //ReturnData = InterestRate(IntPolicyId, IntPostingDate, InterestProduct, TrustType);
                            foreach (var PFTEmployeeLedgerDataitem in PFTEmployeeLedgerData_13)
                            {
                                CloseBal = CloseBal + PFTEmployeeLedgerDataitem.VPFIntCloseBal;


                            }
                        }
                        else
                        {
                            var PFTEmployeeLedgerData_13 = db.EmployeePFTrust.Where(e => e.Id == Employee_Id)
                                .Select(e => e.EmpSettlementPFT
                                .Where(r => r.CalcDate >= FromPeriod && r.CalcDate <= ToPeriod &&
                                PassbookLoanID.Contains(r.PassbookActivity.Id) == false).OrderByDescending(x => x.Id).ToList()).SingleOrDefault();

                            //CloseBal = PFTEmployeeLedgerData.OrderByDescending(e => e.Id).FirstOrDefault().PFCloseBal;
                            //CurrentData = PFTEmployeeLedgerData.Sum(e => e.TotalPFAmountMonthly + e.TotalPFInt);
                            //ReturnData = InterestRate(IntPolicyId, IntPostingDate, InterestProduct, TrustType);
                            foreach (var PFTEmployeeLedgerDataitem in PFTEmployeeLedgerData_13)
                            {
                                CloseBal = CloseBal + PFTEmployeeLedgerDataitem.VPFIntCloseBal;


                            }
                        }
                        CloseBal = CloseBal - VPFIntPFLoan;
                        ReturnData = InterestRate(IntPolicyId, IntPostingDate, InterestProduct, TrustType);
                        if (ReturnData.Errno == 0)
                        {
                            CalcInt = CalcInt + (CloseBal * (ReturnData.ReturnValue / (12 * 100)));
                            CalcInt = Math.Round(CalcInt + 0.001, 0);
                            ReturnData.ReturnValue = CalcInt;
                            ReturnData.Errno = 0;
                            return ReturnData;
                        }
                        else
                        {
                            return ReturnData;
                        }
                        break;
                }
                return ReturnData;
            }

        }
        #endregion Interest Calculation
        //PassbookLoanIDValue
        //{"Monthly Credit","Debit Load", "Credit Loan","Loan Posting Balance", "Interest Apply", "Interest Posting Balance"}


        //PassbookLoanIDValue[0] = "PFECR CREDIT";
        //PassbookLoanIDValue[1] = "PFECR BALANCE";
        //PassbookLoanIDValue[2] = "LOAN DEBIT";
        //PassbookLoanIDValue[3] = "LOAN CREDIT";
        //PassbookLoanIDValue[4] = "LOAN BALANCE";
        //PassbookLoanIDValue[5] = "INTEREST APPLY";
        //PassbookLoanIDValue[6] = "INTEREST BALANCE";
        //public static ReturnData InterestPostingEmp(int IntPolicyId, int EmployeePFT_Id,
        //    DateTime IntPostingDate, DateTime? SettlementDate, DBTrack DBTrack)
        public static List<ReturnDataintp> InterestPostingEmp(int IntPolicyId, int EmployeePFT_Id, DateTime IntPostingDate, DateTime? SettlementDate, DBTrack DBTrack)
      //  public static List<string> InterestPostingEmp(int IntPolicyId, int EmployeePFT_Id, DateTime IntPostingDate, DateTime? SettlementDate, DBTrack DBTrack)
        {
            ReturnData ReturnData = new ReturnData();
            ReturnDataintp ErrorLog = new ReturnDataintp();
            List<ReturnDataintp> ReturnDataList = new List<ReturnDataintp>();
            List<string> Msg = new List<string>();
            try
            {
                DateTime MonYearDate = IntPostingDate;// Convert.ToDateTime("01/" + MonthYear);
                string MonthYear = MonYearDate.ToString("MM/yyyy");
                string MonthYearFull = MonYearDate.ToString("MMMM");
                string InterestFrequencyValue = "";
                string InterestPostingMethod = "";
                bool IntCarryforward = false;
                bool IntMergePF = false;
                int InterestPostingMonth = 0;
                List<string> IntPostMonth = null;
                DateTime FromPeriod = DateTime.Now;
                DateTime ToPeriod = DateTime.Now;

                int EmployeePFTrust_Id = 0;
                int IntPostBal_Id = 0;
                int MonthlyCredit_Id = 0;
                int IntApply_Id = 0;
                int LoanApply_Id = 0;
                int LoanPostBal_Id = 0;
                double Curr_OwnInterest = 0;
                double Curr_OwnerInterest = 0;
                double Curr_VPFInterest = 0;
                double Curr_PFinterest = 0;
                double Curr_TotInterest = 0;
                double Curr_AllInterest = 0;
                double Curr_RLoanAmtInt = 0;
                double Curr_TaxableInterest = 0;
                double Curr_NonTaxableInterest = 0;
                double Tds_TaxableInterest = 0;
                double Curr_OwnInterestonInterest = 0;
                double Curr_OwnerInterestonInterest = 0;
                double Curr_VPFInterestonInterest = 0;

                double Curr_OwnInterestoninterest = 0;
                double Curr_OwnerInterestoninterest = 0;
                double Curr_VPFInterestoninterest = 0;

                using (DataBaseContext db = new DataBaseContext())
                {

                    //IntPostBal_Id = db.LookupValue.Where(e => e.IsActive != false && e.LookupVal.ToUpper() == "Interest Posting Balance".ToUpper()).Select(e => e.Id).SingleOrDefault();
                    //MonthlyCredit_Id = db.LookupValue.Where(e => e.IsActive != false && e.LookupVal.ToUpper() == "Monthly Credit".ToUpper()).Select(e => e.Id).SingleOrDefault();
                    //IntApply_Id = db.LookupValue.Where(e => e.IsActive != false && e.LookupVal.ToUpper() == "Interest Apply".ToUpper()).Select(e => e.Id).SingleOrDefault();
                    //LoanApply_Id = db.LookupValue.Where(e => e.IsActive != false && e.LookupVal.ToUpper() == "Loan Apply".ToUpper()).Select(e => e.Id).SingleOrDefault();
                    //LoanPostBal_Id = db.LookupValue.Where(e => e.IsActive != false && e.LookupVal.ToUpper() == "Loan Posting Balance".ToUpper()).Select(e => e.Id).SingleOrDefault();

                    //Find Last "Interest Posting Balance" employee Ledger
                    P2B.UTILS.P2BLogger logger = new P2B.UTILS.P2BLogger();
                    string EmpPayrollID = EmployeePFT_Id.ToString();
                    logger.Logging("Employeepftid interest posting in Ledger::::  " + EmpPayrollID);
                    ErrorLog = new ReturnDataintp();
                    var PassbookLoanIDValue = new List<string>();
                    PassbookLoanIDValue.Add("INTEREST POSTING");
                    PassbookLoanIDValue.Add("INTEREST BALANCE");


                    List<int> PassbookID = new List<int>();
                    PassbookID = db.LookupValue.Where(e => PassbookLoanIDValue.Contains(e.LookupVal.ToUpper())).Select(e => e.Id).ToList();


                    var PassbookLoanIDValueSettle = new List<string>();
                    PassbookLoanIDValueSettle.Add("SETTLEMENT POSTING");
                    PassbookLoanIDValueSettle.Add("SETTLEMENT BALANCE");


                    List<int> PassbookIDSettle = new List<int>();
                    PassbookIDSettle = db.LookupValue.Where(e => PassbookLoanIDValueSettle.Contains(e.LookupVal.ToUpper())).Select(e => e.Id).ToList();

                    //var EffectiveMonths = db.InterestPolicies
                    //    .Where(e => e.Id == IntPolicyId).Select(e => e.StatutoryEffectiveMonthsPFT.OrderBy(r => r.EffectiveMonth_Id.Value).ToList()).SingleOrDefault();
                    var EffectiveMonths1 = db.InterestPolicies
                        .Include(e => e.StatutoryEffectiveMonthsPFT)
                        .Include(e => e.StatutoryEffectiveMonthsPFT.Select(x => x.EffectiveMonth))
                       .Where(e => e.Id == IntPolicyId).SingleOrDefault();

                    var EffectiveMonths = EffectiveMonths1.StatutoryEffectiveMonthsPFT.Select(r => r.EffectiveMonth).ToList();
                    //var CurrentEmpLedger;
                    // var LastPFECRBalancePostLedger1 = db.EmployeePFTrust.Include(e=>e.Employee).Include(e => e.PFTEmployeeLedger.Where(r => r.PassbookActivity.LookupVal.ToUpper() == "MONTHLY PF BALANCE")).Where(e => e.Id == EmployeePFTrust_Id).SingleOrDefault();
                    var LastPFECRBalancePostLedger1 = db.EmployeePFTrust.Include(e => e.Employee).Include(e => e.PFTEmployeeLedger).Include(e => e.PFTEmployeeLedger.Select(x => x.PassbookActivity)).Where(e => e.Id == EmployeePFT_Id).AsNoTracking().SingleOrDefault();

                    var LastPFECRBalancePostLedger = LastPFECRBalancePostLedger1.PFTEmployeeLedger.Where(r => r.PassbookActivity.LookupVal.ToUpper() == "PF BALANCE").OrderBy(e => e.Id).LastOrDefault();
                    if (LastPFECRBalancePostLedger == null)
                    {
                        ErrorLog.ReturnValue = EmployeePFT_Id;
                        ErrorLog.Errno = 0;
                        ErrorLog.ErrMsg = "PF Credit balance is not available for " + LastPFECRBalancePostLedger1.Employee.EmpCode.ToString();
                        ReturnDataList.Add(ErrorLog);
                       // Msg.Add("PF Credit balance is not available for " + LastPFECRBalancePostLedger1.Employee.EmpCode.ToString());


                    }
                    else
                    {
                        FromPeriod = Convert.ToDateTime("01/" + LastPFECRBalancePostLedger.MonthYear).AddMonths(1);
                    }
                    //delete interest posting
                    //var IntPostLedgerRemovevar = db.EmployeePFTrust.Where(e => e.Id == EmployeePFTrust_Id)
                    //    .Select(r => r.PFTEmployeeLedger.Where(t => t.Id > LastPFECRBalancePostLedger.Id))
                    //    .SingleOrDefault();
                    //if (IntPostLedgerRemovevar.ToList() != null)
                    //{
                    //    var EmpL = db.EmployeePFTrust.Include(e => e.PFTEmployeeLedger).Where(e => e.Id == EmployeePFTrust_Id).SingleOrDefault();
                    //    var ss = db.PFTEmployeeLedger.RemoveRange(IntPostLedgerRemovevar);
                    //    EmpL.PFTEmployeeLedger = ss.ToList();
                    //    db.SaveChanges();

                    //}
                    if (SettlementDate == null)
                    {
                        var PFTEmployeeLedgerCurrent = db.EmployeePFTrust.Where(e => e.Id == EmployeePFT_Id)
                               .Select(e => e.PFTEmployeeLedger.Where(r => r.MonthYear == MonthYear
                               && PassbookID.Contains(r.PassbookActivity.Id)).ToList()).SingleOrDefault();
                        if (PFTEmployeeLedgerCurrent != null)
                        {
                            db.PFTEmployeeLedger.RemoveRange(PFTEmployeeLedgerCurrent);
                            db.SaveChanges();
                        }
                    }
                    if (SettlementDate != null)
                    {
                        var PFTEmployeeLedgerCurrent = db.EmployeePFTrust.Where(e => e.Id == EmployeePFT_Id)
                                                      .Select(e => e.EmpSettlementPFT
                                                       .ToList()).SingleOrDefault();
                        if (PFTEmployeeLedgerCurrent != null)
                        {
                            db.EmpSettlementPFT.RemoveRange(PFTEmployeeLedgerCurrent);
                            db.SaveChanges();
                        }
                    }

                    //PrevMonthYear = MonYearDate.AddMonths(-1);


                    IntPostMonth = EffectiveMonths.Select(x => x.LookupVal).ToList();// (select LookupVal from LookupValue where id = (select EffectiveMonth_Id from StatutoryEffectiveMonths where InterestPolicies_Id is not null))
                    var InterestFrequency = db.InterestPolicies.Include(x => x.InterestFrequency).Include(y => y.IntPostingMethod).Where(e => e.Id == IntPolicyId).SingleOrDefault();
                    InterestFrequencyValue = InterestFrequency.InterestFrequency.LookupVal.ToUpper();
                    InterestPostingMethod = InterestFrequency.IntPostingMethod.LookupVal.ToUpper();
                    IntCarryforward = InterestFrequency.IsIntCarryForward;
                    IntMergePF = InterestFrequency.IsIntMergePF;

                    ToPeriod = (MonYearDate.AddMonths(1)).AddDays(-1);
                    InterestPostingMonth = MonYearDate.Month;

                    if (InterestFrequencyValue.ToUpper() == "Monthly".ToUpper())
                    {
                        ToPeriod = (MonYearDate.AddMonths(1)).AddDays(-1);
                    }
                    else if (SettlementDate != null)
                    {
                        ToPeriod = Convert.ToDateTime("01/" + SettlementDate.Value.ToString("MM/yyyy"));
                        ToPeriod = (ToPeriod.AddMonths(1)).AddDays(-1);
                    }
                    else if (IntPostMonth.Contains(MonthYearFull) == true)
                    {
                        int Frequency = 0;
                        if (InterestFrequencyValue.ToUpper() == "YEARLY")
                        {
                            Frequency = 12;
                        }
                        else if (InterestFrequencyValue.ToUpper() == "MONTHLY")
                        {
                            Frequency = 1;
                        }
                        else if (InterestFrequencyValue.ToUpper() == "HALFYEARLY")
                        {
                            Frequency = 6;
                        }
                        else if (InterestFrequencyValue.ToUpper() == "QUARTERLY")
                        {
                            Frequency = 3;
                        }
                        //  var Frequency = Convert.ToInt16(InterestFrequencyValue);
                        FromPeriod = FromPeriod.Date.AddMonths(-Frequency).Date;
                        ToPeriod = FromPeriod.Date.AddMonths(Frequency).AddDays(-1).Date;
                    }
                    else
                    {
                        ErrorLog.ReturnValue = EmployeePFT_Id;
                        ErrorLog.Errno = 0;
                        ErrorLog.ErrMsg = "Interest Posting Month is not valid ";
                        ReturnDataList.Add(ErrorLog);
                        //Msg.Add("Interest Posting Month is not valid ");
                        return ReturnDataList;

                    }

                    //For settlement Last Record add from EmppftLedger start
                    if (SettlementDate != null)
                    {
                        var PassbookLoanIDValueIntbal = new List<string>();
                        PassbookLoanIDValueIntbal.Add("INTEREST BALANCE");
                        List<int> PassbookLoanIDIntbal = new List<int>();
                        PassbookLoanIDIntbal = db.LookupValue.Where(e => PassbookLoanIDValueIntbal.Contains(e.LookupVal.ToUpper())).Select(e => e.Id).ToList();


                        var PFTEmployeeLedgerDataIntbal = db.EmployeePFTrust.Where(e => e.Id == EmployeePFT_Id)
                               .Select(e => e.PFTEmployeeLedger
                               .Where(r => PassbookLoanIDIntbal.Contains(r.PassbookActivity.Id) == true).OrderByDescending(x => x.Id).ToList()).SingleOrDefault();
                        var PFTEmployeeLedgerDataIntbalid = PFTEmployeeLedgerDataIntbal.FirstOrDefault();


                        var PassbookLoanIDValueProd = new List<string>();

                        PassbookLoanIDValueProd.Add("LOAN DEBIT POSTING");
                        PassbookLoanIDValueProd.Add("LOAN CREDIT POSTING");
                        PassbookLoanIDValueProd.Add("INTEREST POSTING");
                        PassbookLoanIDValueProd.Add("MONTHLY PF POSTING");
                        PassbookLoanIDValueProd.Add("LOAN DEBIT BALANCE");
                        List<int> PassbookLoanIDProd = new List<int>();

                        PassbookLoanIDProd = db.LookupValue.Where(e => PassbookLoanIDValueProd.Contains(e.LookupVal.ToUpper())).Select(e => e.Id).ToList();

                        // if interest apply then after that produt in settlement
                        if (PFTEmployeeLedgerDataIntbalid != null)
                        {

                            //  var PFTEmpLedgerOldListset = db.EmployeePFTrust.Include(e => e.Employee)
                            //.Include(e => e.Employee.GeoStruct)
                            //.Include(e => e.Employee.PayStruct)
                            //.Include(e => e.Employee.FuncStruct)
                            //.Where(e => e.Id == EmployeePFT_Id).Select(r => r.PFTEmployeeLedger.Where(x => x.Id > PFTEmployeeLedgerDataIntbalid.Id && PassbookLoanIDProd.Contains(x.PassbookActivity.Id) == false).ToList()).SingleOrDefault();


                            var PFTEmpLedgerOldsett = db.PFTEmployeeLedger.Include(e => e.PayStruct).Include(e => e.FuncStruct).Include(e => e.GeoStruct).Include(e => e.PassbookActivity).Where(x => x.Id > PFTEmployeeLedgerDataIntbalid.Id && PassbookLoanIDProd.Contains(x.PassbookActivity.Id) == false && x.EmployeePFTrust_Id == EmployeePFT_Id).ToList();

                            // var PFTEmpLedgerOldsett = PFTEmpLedgerOldListset.ToList();
                            var PFTEmpLedgerListset = new List<EmpSettlementPFT>();
                            foreach (var PFTEmpLedgerOldset in PFTEmpLedgerOldsett)
                            {

                                var PFTEmpLedgerECRDataBalanceset = new EmpSettlementPFT()
                                {
                                    GeoStruct = PFTEmpLedgerOldset.GeoStruct,
                                    PayStruct = PFTEmpLedgerOldset.PayStruct,
                                    FuncStruct = PFTEmpLedgerOldset.FuncStruct,
                                    MonthYear = PFTEmpLedgerOldset.MonthYear,
                                    PassbookActivity = PFTEmpLedgerOldset.PassbookActivity,
                                    PostingDate = DateTime.Now.Date,
                                    CalcDate = PFTEmpLedgerOldset.CalcDate.Date,//SalaryTPF.PaymentDate.Value, //SalaryTPF.PaymentDate null in table
                                    SettlementDate = SettlementDate.Value.Date,
                                    SeperationDate = SettlementDate.Value.Date,
                                    PaymentDate = SettlementDate.Value.Date,
                                    ChequeIssueDate = SettlementDate.Value.Date,

                                    #region Input Data for ledger
                                    //OwnPFMonthly = SalaryTPF.PFECRR.EE_Share + SalaryTPF.PFECRR.Arrear_EE_Share,
                                    //OwnerPFMonthly = SalaryTPF.PFECRR.ER_Share + SalaryTPF.PFECRR.Arrear_ER_Share,
                                    //VPFAmountMonthly = SalaryTPF.PFECRR.EE_VPF_Share,
                                    //PensionAmount = SalaryTPF.PFECRR.EPS_Share + SalaryTPF.PFECRR.Arrear_EPS_Share,
                                    //PFWages = SalaryTPF.PFECRR.EPF_Wages + SalaryTPF.PFECRR.Arrear_EPF_Wages,
                                    LoanAmountCredit = PFTEmpLedgerOldset.LoanAmountCredit,
                                    LoanAmountDebit = PFTEmpLedgerOldset.LoanAmountDebit,
                                    //OwnPFInt = 0,
                                    //OwnerPFInt = 0,
                                    //VPFInt =0,
                                    //PFInt = 0,
                                    #endregion Input Data for ledger

                                    OwnOpenBal = PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.OwnOpenBal : 0,
                                    OwnCloseBal = (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.OwnCloseBal : 0),
                                    OwnerOpenBal = PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.OwnerOpenBal : 0,
                                    OwnerCloseBal = (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.OwnerCloseBal : 0),
                                    VPFOpenBal = PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.VPFOpenBal : 0,
                                    VPFCloseBal = (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.VPFCloseBal : 0),
                                    PFOpenBal = PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.PFOpenBal : 0,
                                    PFCloseBal = (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.PFCloseBal : 0),

                                    OwnIntOpenBal = PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.OwnIntOpenBal : 0,
                                    OwnIntCloseBal = (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.OwnIntCloseBal : 0),
                                    OwnerIntOpenBal = PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.OwnerIntOpenBal : 0,
                                    OwnerIntCloseBal = (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.OwnerIntCloseBal : 0),
                                    VPFIntOpenBal = PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.VPFIntOpenBal : 0,
                                    VPFIntCloseBal = (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.VPFIntCloseBal : 0),
                                    PFIntOpenBal = PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.PFIntOpenBal : 0,
                                    PFIntCloseBal = PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.PFIntCloseBal : 0,



                                    OwnPFLoan = (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.OwnPFLoan : 0),
                                    OwnerPFLoan = (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.OwnerPFLoan : 0),
                                    VPFPFLoan = (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.VPFPFLoan : 0),

                                    IntOnInt = (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.IntOnInt : 0),
                                    IntonIntOpenBal = (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.IntonIntOpenBal : 0),
                                    IntonIntCloseBal = (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.IntonIntCloseBal : 0),
                                    IntOnIntPFLoan = (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.IntOnIntPFLoan : 0),
                                    OwnIntPFLoan = (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.OwnIntPFLoan : 0),
                                    OwnerIntPFLoan = (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.OwnerIntPFLoan : 0),
                                    VPFIntPFLoan = (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.VPFIntPFLoan : 0),
                                    TotalIntOpenBal = (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.TotalIntOpenBal : 0),
                                    TotalIntCloseBal = (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.TotalIntCloseBal : 0),


                                    // IsPassbookClose = false,
                                    Narration = PFTEmpLedgerOldset.Narration,
                                    InterestFrequency = PFTEmpLedgerOldset.InterestFrequency,
                                    DBTrack = DBTrack,
                                    AccuNonTaxableAccountOpeningbalance = PFTEmpLedgerOldset.AccuNonTaxableAccountOpeningbalance,
                                    AccuNonTaxableAccountClosingbalance = PFTEmpLedgerOldset.AccuNonTaxableAccountClosingbalance,
                                    AccTaxableAccountOpeningbalance = PFTEmpLedgerOldset.AccTaxableAccountOpeningbalance,
                                    AccTaxableAccountClosingbalance = PFTEmpLedgerOldset.AccTaxableAccountClosingbalance,

                                    AccuNonTaxableAccountFy_Openingbalance = PFTEmpLedgerOldset.AccuNonTaxableAccountFy_Openingbalance,//PFTEmpLedgerOld != null ? PFTEmpLedgerOld.AccuNonTaxableAccountFy_Closingbalance : 0,
                                    AccuNonTaxableAccountFy_Closingbalance = PFTEmpLedgerOldset.AccuNonTaxableAccountFy_Closingbalance,//check taxableaccpFy
                                    TaxableAccountFy_Openingbalance = PFTEmpLedgerOldset.TaxableAccountFy_Openingbalance,//PFTEmpLedgerOld != null ? PFTEmpLedgerOld.TaxableAccountFy_Closingbalance : 0,
                                    TaxableAccountFy_Closingbalance = PFTEmpLedgerOldset.TaxableAccountFy_Closingbalance,//(PFTEmpLedgerOld != null ? PFTEmpLedgerOld.TaxableAccountFy_Closingbalance : 0) + Taxableaccpfmonthly,

                                    NonTaxableAccountInterest = PFTEmpLedgerOldset.NonTaxableAccountInterest,//(PFTEmpLedgerOld != null ? PFTEmpLedgerOld.NonTaxableAccountInterest : 0) + NontaxInt,
                                    TaxableAccountInterest = PFTEmpLedgerOldset.TaxableAccountInterest,//(PFTEmpLedgerOld != null ? PFTEmpLedgerOld.TaxableAccountInterest : 0) + TaxableaccInt,


                                };
                                PFTEmpLedgerListset.Add(PFTEmpLedgerECRDataBalanceset);//added monthly PF data balance
                            }
                            var EmployeePFTrustset = db.EmployeePFTrust.Include(t => t.EmpSettlementPFT).Where(t => t.Id == EmployeePFT_Id).SingleOrDefault();
                            // EmployeePFTrust.PFTEmployeeLedger.ToList().AddRange(PFTEmpLedgerList);
                            PFTEmpLedgerListset.AddRange(EmployeePFTrustset.EmpSettlementPFT);
                            EmployeePFTrustset.EmpSettlementPFT = PFTEmpLedgerListset;
                            db.EmployeePFTrust.Attach(EmployeePFTrustset);
                            db.Entry(EmployeePFTrustset).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            db.Entry(EmployeePFTrustset).State = System.Data.Entity.EntityState.Detached;
                        }
                        else // if interest not apply then all  produt in settlement
                        {
                            //    var PFTEmpLedgerOldListset = db.EmployeePFTrust.Include(e => e.Employee)
                            //.Include(e => e.Employee.GeoStruct)
                            //.Include(e => e.Employee.PayStruct)
                            //.Include(e => e.Employee.FuncStruct)
                            //.Where(e => e.Id == EmployeePFT_Id).Select(r => r.PFTEmployeeLedger.Where(x => PassbookLoanIDProd.Contains(x.PassbookActivity.Id) == false).ToList()).SingleOrDefault();
                            var PFTEmpLedgerOldsett = db.PFTEmployeeLedger.Include(e => e.PayStruct).Include(e => e.FuncStruct).Include(e => e.GeoStruct).Include(e => e.PassbookActivity).Where(x => PassbookLoanIDProd.Contains(x.PassbookActivity.Id) == false && x.EmployeePFTrust_Id == EmployeePFT_Id).ToList();
                            // var PFTEmpLedgerOldsett = PFTEmpLedgerOldListset.ToList();
                            var PFTEmpLedgerListset = new List<EmpSettlementPFT>();
                            foreach (var PFTEmpLedgerOldset in PFTEmpLedgerOldsett)
                            {

                                var PFTEmpLedgerECRDataBalanceset = new EmpSettlementPFT()
                                {
                                    GeoStruct = PFTEmpLedgerOldset.GeoStruct,
                                    PayStruct = PFTEmpLedgerOldset.PayStruct,
                                    FuncStruct = PFTEmpLedgerOldset.FuncStruct,
                                    MonthYear = PFTEmpLedgerOldset.MonthYear,
                                    PassbookActivity = PFTEmpLedgerOldset.PassbookActivity,
                                    PostingDate = DateTime.Now.Date,
                                    CalcDate = PFTEmpLedgerOldset.CalcDate.Date,//SalaryTPF.PaymentDate.Value, //SalaryTPF.PaymentDate null in table
                                    SettlementDate = SettlementDate.Value.Date,
                                    SeperationDate = SettlementDate.Value.Date,
                                    PaymentDate = SettlementDate.Value.Date,
                                    ChequeIssueDate = SettlementDate.Value.Date,

                                    #region Input Data for ledger
                                    //OwnPFMonthly = SalaryTPF.PFECRR.EE_Share + SalaryTPF.PFECRR.Arrear_EE_Share,
                                    //OwnerPFMonthly = SalaryTPF.PFECRR.ER_Share + SalaryTPF.PFECRR.Arrear_ER_Share,
                                    //VPFAmountMonthly = SalaryTPF.PFECRR.EE_VPF_Share,
                                    //PensionAmount = SalaryTPF.PFECRR.EPS_Share + SalaryTPF.PFECRR.Arrear_EPS_Share,
                                    //PFWages = SalaryTPF.PFECRR.EPF_Wages + SalaryTPF.PFECRR.Arrear_EPF_Wages,
                                    LoanAmountCredit = PFTEmpLedgerOldset.LoanAmountCredit,
                                    LoanAmountDebit = PFTEmpLedgerOldset.LoanAmountDebit,
                                    //OwnPFInt = 0,
                                    //OwnerPFInt = 0,
                                    //VPFInt =0,
                                    //PFInt = 0,
                                    #endregion Input Data for ledger

                                    OwnOpenBal = PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.OwnOpenBal : 0,
                                    OwnCloseBal = (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.OwnCloseBal : 0),
                                    OwnerOpenBal = PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.OwnerOpenBal : 0,
                                    OwnerCloseBal = (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.OwnerCloseBal : 0),
                                    VPFOpenBal = PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.VPFOpenBal : 0,
                                    VPFCloseBal = (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.VPFCloseBal : 0),
                                    PFOpenBal = PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.PFOpenBal : 0,
                                    PFCloseBal = (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.PFCloseBal : 0),

                                    OwnIntOpenBal = PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.OwnIntOpenBal : 0,
                                    OwnIntCloseBal = (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.OwnIntCloseBal : 0),
                                    OwnerIntOpenBal = PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.OwnerIntOpenBal : 0,
                                    OwnerIntCloseBal = (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.OwnerIntCloseBal : 0),
                                    VPFIntOpenBal = PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.VPFIntOpenBal : 0,
                                    VPFIntCloseBal = (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.VPFIntCloseBal : 0),
                                    PFIntOpenBal = PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.PFIntOpenBal : 0,
                                    PFIntCloseBal = PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.PFIntCloseBal : 0,



                                    OwnPFLoan = (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.OwnPFLoan : 0),
                                    OwnerPFLoan = (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.OwnerPFLoan : 0),
                                    VPFPFLoan = (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.VPFPFLoan : 0),

                                    IntOnInt = (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.IntOnInt : 0),
                                    IntonIntOpenBal = (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.IntonIntOpenBal : 0),
                                    IntonIntCloseBal = (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.IntonIntCloseBal : 0),
                                    IntOnIntPFLoan = (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.IntOnIntPFLoan : 0),
                                    OwnIntPFLoan = (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.OwnIntPFLoan : 0),
                                    OwnerIntPFLoan = (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.OwnerIntPFLoan : 0),
                                    VPFIntPFLoan = (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.VPFIntPFLoan : 0),
                                    TotalIntOpenBal = (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.TotalIntOpenBal : 0),
                                    TotalIntCloseBal = (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.TotalIntCloseBal : 0),


                                    // IsPassbookClose = false,
                                    Narration = PFTEmpLedgerOldset.Narration,
                                    InterestFrequency = PFTEmpLedgerOldset.InterestFrequency,
                                    DBTrack = DBTrack,
                                    AccuNonTaxableAccountOpeningbalance = PFTEmpLedgerOldset.AccuNonTaxableAccountOpeningbalance,
                                    AccuNonTaxableAccountClosingbalance = PFTEmpLedgerOldset.AccuNonTaxableAccountClosingbalance,
                                    AccTaxableAccountOpeningbalance = PFTEmpLedgerOldset.AccTaxableAccountOpeningbalance,
                                    AccTaxableAccountClosingbalance = PFTEmpLedgerOldset.AccTaxableAccountClosingbalance,

                                    AccuNonTaxableAccountFy_Openingbalance = PFTEmpLedgerOldset.AccuNonTaxableAccountFy_Openingbalance,//PFTEmpLedgerOld != null ? PFTEmpLedgerOld.AccuNonTaxableAccountFy_Closingbalance : 0,
                                    AccuNonTaxableAccountFy_Closingbalance = PFTEmpLedgerOldset.AccuNonTaxableAccountFy_Closingbalance,//check taxableaccpFy
                                    TaxableAccountFy_Openingbalance = PFTEmpLedgerOldset.TaxableAccountFy_Openingbalance,//PFTEmpLedgerOld != null ? PFTEmpLedgerOld.TaxableAccountFy_Closingbalance : 0,
                                    TaxableAccountFy_Closingbalance = PFTEmpLedgerOldset.TaxableAccountFy_Closingbalance,//(PFTEmpLedgerOld != null ? PFTEmpLedgerOld.TaxableAccountFy_Closingbalance : 0) + Taxableaccpfmonthly,

                                    NonTaxableAccountInterest = PFTEmpLedgerOldset.NonTaxableAccountInterest,//(PFTEmpLedgerOld != null ? PFTEmpLedgerOld.NonTaxableAccountInterest : 0) + NontaxInt,
                                    TaxableAccountInterest = PFTEmpLedgerOldset.TaxableAccountInterest,//(PFTEmpLedgerOld != null ? PFTEmpLedgerOld.TaxableAccountInterest : 0) + TaxableaccInt,


                                };
                                PFTEmpLedgerListset.Add(PFTEmpLedgerECRDataBalanceset);//added monthly PF data balance
                            }
                            var EmployeePFTrustset = db.EmployeePFTrust.Include(t => t.EmpSettlementPFT).Where(t => t.Id == EmployeePFT_Id).SingleOrDefault();
                            // EmployeePFTrust.PFTEmployeeLedger.ToList().AddRange(PFTEmpLedgerList);
                            PFTEmpLedgerListset.AddRange(EmployeePFTrustset.EmpSettlementPFT);
                            EmployeePFTrustset.EmpSettlementPFT = PFTEmpLedgerListset;
                            db.EmployeePFTrust.Attach(EmployeePFTrustset);
                            db.Entry(EmployeePFTrustset).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            db.Entry(EmployeePFTrustset).State = System.Data.Entity.EntityState.Detached;
                        }

                        //var PFTEmployeeLedgerAdd = db.PFTEmployeeLedger.Where(e => == Employee_Id).SingleOrDefault();





                    }
                    //For settlement Last Record add from EmppftLedger end

                    if (SettlementDate != null)
                    {
                        var PFTEmpLedgerOldListset = db.EmployeePFTrust.Include(e => e.Employee)
                      .Include(e => e.Employee.GeoStruct)
                      .Include(e => e.Employee.PayStruct)
                      .Include(e => e.Employee.FuncStruct)
                      .Where(e => e.Id == EmployeePFT_Id).Select(r => r.EmpSettlementPFT.ToList()).SingleOrDefault();

                        var PFTEmpmastset = db.EmployeePFTrust.Include(e => e.Employee)
                      .Include(e => e.Employee.GeoStruct)
                      .Include(e => e.Employee.PayStruct)
                      .Include(e => e.Employee.FuncStruct)
                      .Where(e => e.Id == EmployeePFT_Id).SingleOrDefault();
                        var PFTEmpLedgerOldset = PFTEmpLedgerOldListset.FirstOrDefault();

                        FromPeriod = Convert.ToDateTime("01/" + PFTEmpLedgerOldset.MonthYear);

                        List<string> Msgp = new List<string>();
                        Msgp = P2BUltimate.Process.GlobalProcess.UploadECRPro(SettlementDate, EmployeePFT_Id);


                    }

                    //ReturnData = InterestCalc(IntPolicyId, FromPeriod, ToPeriod, Employee_Id, InterestProduct, TrustType, IntPostingDate, SettlementDate)
                    if (InterestPostingMethod == "INDIVIDUAL")
                    {
                        ReturnData = InterestCalc(IntPolicyId, FromPeriod, ToPeriod, EmployeePFT_Id, 0, 0, IntPostingDate, SettlementDate);
                        Curr_OwnInterest = ReturnData.ReturnValue;
                        ReturnData = InterestCalc(IntPolicyId, FromPeriod, ToPeriod, EmployeePFT_Id, 1, 0, IntPostingDate, SettlementDate);
                        Curr_OwnerInterest = ReturnData.ReturnValue;
                        ReturnData = InterestCalc(IntPolicyId, FromPeriod, ToPeriod, EmployeePFT_Id, 2, 0, IntPostingDate, SettlementDate);
                        Curr_VPFInterest = ReturnData.ReturnValue;
                    }
                    if (InterestPostingMethod == "PF+VPF")
                    {
                        ReturnData = InterestCalc(IntPolicyId, FromPeriod, ToPeriod, EmployeePFT_Id, 0, 0, IntPostingDate, SettlementDate);
                        Curr_OwnInterest = ReturnData.ReturnValue;
                        ReturnData = InterestCalc(IntPolicyId, FromPeriod, ToPeriod, EmployeePFT_Id, 2, 0, IntPostingDate, SettlementDate);
                        Curr_VPFInterest = ReturnData.ReturnValue;
                    }
                    if (InterestPostingMethod == "TOTALPF(OWN+OWNER+VPF)")
                    {
                        ReturnData = InterestCalc(IntPolicyId, FromPeriod, ToPeriod, EmployeePFT_Id, 3, 0, IntPostingDate, SettlementDate);
                        Curr_PFinterest = ReturnData.ReturnValue;
                        ReturnData = InterestCalc(IntPolicyId, FromPeriod, ToPeriod, EmployeePFT_Id, 2, 0, IntPostingDate, SettlementDate);
                        Curr_VPFInterest = ReturnData.ReturnValue;
                    }
                    if (InterestPostingMethod == "INDIVIDUAL+INT")
                    {
                        ReturnData = InterestCalc(IntPolicyId, FromPeriod, ToPeriod, EmployeePFT_Id, 0, 0, IntPostingDate, SettlementDate);
                        Curr_OwnInterest = ReturnData.ReturnValue;
                        ReturnData = InterestCalc(IntPolicyId, FromPeriod, ToPeriod, EmployeePFT_Id, 1, 0, IntPostingDate, SettlementDate);
                        Curr_OwnerInterest = ReturnData.ReturnValue;
                        ReturnData = InterestCalc(IntPolicyId, FromPeriod, ToPeriod, EmployeePFT_Id, 2, 0, IntPostingDate, SettlementDate);
                        Curr_VPFInterest = ReturnData.ReturnValue;

                        ReturnData = InterestCalc(IntPolicyId, FromPeriod, ToPeriod, EmployeePFT_Id, 6, 0, IntPostingDate, SettlementDate);
                        Curr_OwnInterestonInterest = ReturnData.ReturnValue;
                        ReturnData = InterestCalc(IntPolicyId, FromPeriod, ToPeriod, EmployeePFT_Id, 7, 0, IntPostingDate, SettlementDate);
                        Curr_OwnerInterestonInterest = ReturnData.ReturnValue;
                        ReturnData = InterestCalc(IntPolicyId, FromPeriod, ToPeriod, EmployeePFT_Id, 8, 0, IntPostingDate, SettlementDate);
                        Curr_VPFInterestonInterest = ReturnData.ReturnValue;

                    }
                    if (InterestPostingMethod == "PF+VPF+INT")
                    {
                        ReturnData = InterestCalc(IntPolicyId, FromPeriod, ToPeriod, EmployeePFT_Id, 0, 0, IntPostingDate, SettlementDate);
                        Curr_OwnInterest = ReturnData.ReturnValue;
                        ReturnData = InterestCalc(IntPolicyId, FromPeriod, ToPeriod, EmployeePFT_Id, 2, 0, IntPostingDate, SettlementDate);
                        Curr_VPFInterest = ReturnData.ReturnValue;

                        ReturnData = InterestCalc(IntPolicyId, FromPeriod, ToPeriod, EmployeePFT_Id, 6, 0, IntPostingDate, SettlementDate);
                        Curr_OwnInterestonInterest = ReturnData.ReturnValue;
                        ReturnData = InterestCalc(IntPolicyId, FromPeriod, ToPeriod, EmployeePFT_Id, 8, 0, IntPostingDate, SettlementDate);
                        Curr_VPFInterestonInterest = ReturnData.ReturnValue;

                    }
                    if (InterestPostingMethod == "TOTALPF+INT(OWN+OWNER+VPF)")
                    {
                        ReturnData = InterestCalc(IntPolicyId, FromPeriod, ToPeriod, EmployeePFT_Id, 3, 0, IntPostingDate, SettlementDate);
                        Curr_PFinterest = ReturnData.ReturnValue;
                        ReturnData = InterestCalc(IntPolicyId, FromPeriod, ToPeriod, EmployeePFT_Id, 5, 0, IntPostingDate, SettlementDate);
                        Curr_AllInterest = ReturnData.ReturnValue;

                    }

                    //ReturnData = InterestCalc(IntPolicyId, FromPeriod, ToPeriod, EmployeePFT_Id, 0, 0, IntPostingDate, SettlementDate);
                    //Curr_OwnInterest = ReturnData.ReturnValue;
                    //ReturnData = InterestCalc(IntPolicyId, FromPeriod, ToPeriod, EmployeePFT_Id, 1, 0, IntPostingDate, SettlementDate);
                    //Curr_OwnerInterest = ReturnData.ReturnValue;
                    //ReturnData = InterestCalc(IntPolicyId, FromPeriod, ToPeriod, EmployeePFT_Id, 2, 0, IntPostingDate, SettlementDate);
                    //Curr_VPFInterest = ReturnData.ReturnValue;
                    //ReturnData = InterestCalc(IntPolicyId, FromPeriod, ToPeriod, EmployeePFT_Id, 3, 0, IntPostingDate, SettlementDate);
                    //Curr_PFinterest = ReturnData.ReturnValue;
                    //ReturnData = InterestCalc(IntPolicyId, FromPeriod, ToPeriod, EmployeePFT_Id, 4, 0, IntPostingDate, SettlementDate);
                    //Curr_TotInterest = ReturnData.ReturnValue;
                    //ReturnData = InterestCalc(IntPolicyId, FromPeriod, ToPeriod, EmployeePFT_Id, 5, 0, IntPostingDate, SettlementDate);
                    //Curr_AllInterest = ReturnData.ReturnValue;
                    //ReturnData = InterestCalc(IntPolicyId, FromPeriod, ToPeriod, EmployeePFT_Id, 6, 0, IntPostingDate, SettlementDate);
                    //Curr_OwnInterestonInterest = ReturnData.ReturnValue;
                    //ReturnData = InterestCalc(IntPolicyId, FromPeriod, ToPeriod, EmployeePFT_Id, 7, 0, IntPostingDate, SettlementDate);
                    //Curr_OwnerInterestonInterest = ReturnData.ReturnValue;
                    //ReturnData = InterestCalc(IntPolicyId, FromPeriod, ToPeriod, EmployeePFT_Id, 8, 0, IntPostingDate, SettlementDate);
                    //Curr_VPFInterestonInterest = ReturnData.ReturnValue;


                    //taxbleaccount interest Tds Calculation 
                    ReturnData = InterestCalc(IntPolicyId, FromPeriod, ToPeriod, EmployeePFT_Id, 9, 0, IntPostingDate, SettlementDate);
                    Curr_TaxableInterest = ReturnData.ReturnValue;
                    ReturnData = InterestCalc(IntPolicyId, FromPeriod, ToPeriod, EmployeePFT_Id, 10, 0, IntPostingDate, SettlementDate);
                    Curr_NonTaxableInterest = ReturnData.ReturnValue;
                    //taxbleaccount interest Tds Calculation 

                    // own,owner,vpf interest on interest

                    ReturnData = InterestCalc(IntPolicyId, FromPeriod, ToPeriod, EmployeePFT_Id, 11, 0, IntPostingDate, SettlementDate);
                    Curr_OwnInterestoninterest = ReturnData.ReturnValue;
                    ReturnData = InterestCalc(IntPolicyId, FromPeriod, ToPeriod, EmployeePFT_Id, 12, 0, IntPostingDate, SettlementDate);
                    Curr_OwnerInterestoninterest = ReturnData.ReturnValue;
                    ReturnData = InterestCalc(IntPolicyId, FromPeriod, ToPeriod, EmployeePFT_Id, 13, 0, IntPostingDate, SettlementDate);
                    Curr_VPFInterestoninterest = ReturnData.ReturnValue;


                    //ReturnData = InterestCalc(IntPolicyId, FromPeriod, ToPeriod, Employee_Id, 6, 0, IntPostingDate, SettlementDate);
                    //Curr_RLoanAmtInt = ReturnData.ReturnValue;

                    //taxbleaccount interest Tds Calculation 
                    var Id = Convert.ToInt32(SessionManager.CompanyId);
                    var CompCodeid = db.Company.Where(e => e.Id == Id).SingleOrDefault();
                    var qurey = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "PFCALENDAR".ToUpper() && e.Default == true).AsEnumerable()
                      .Select(e => new
                      {
                          Id = e.Id,
                          Lvcalendardesc = "FromDate :" + e.FromDate.Value.ToShortDateString() + " ToDate :" + e.ToDate.Value.ToShortDateString(),
                          FromDate = e.FromDate.Value.ToShortDateString(),
                          ToDate = e.ToDate.Value.ToShortDateString()

                      }).SingleOrDefault();


                    var CompantPFTrust_Id = db.CompanyPFTrust.Where(e => e.Company_Id == CompCodeid.Id).Select(e => e.Id).SingleOrDefault();
                    var IntPolicyIdtds = db.PFTACCalendar.Include(e => e.PFTTDSMaster).Include(e => e.CompanyPFTrust).Include(e => e.InterestPolicies).Include(e => e.InterestPolicies.InterestRate).Where(e => e.CompanyPFTrust.Id == CompantPFTrust_Id && e.PFTCalendar.Id == qurey.Id).SingleOrDefault();
                    if (IntPolicyIdtds.PFTTDSMaster != null)
                    {

                        Tds_TaxableInterest = Curr_TaxableInterest * IntPolicyIdtds.PFTTDSMaster.TDSRate / 100;
                        Tds_TaxableInterest = Math.Round(Tds_TaxableInterest + 0.001, 0);
                    }
                    //Interest Posting to PF Employee ledger    
                    //previous record to fetch closing balance
                    if (SettlementDate == null)
                    {
                        //var PFTEmpLedgerOldList = db.EmployeePFTrust.Include(e => e.Employee)
                        //    .Include(e => e.Employee.GeoStruct)
                        //    .Include(e => e.Employee.PayStruct)
                        //    .Include(e => e.Employee.FuncStruct)
                        //    .Where(e => e.Id == EmployeePFT_Id).Select(r => r.PFTEmployeeLedger.ToList()).SingleOrDefault();
                        var PFTEmpLedgerOldList = db.PFTEmployeeLedger.Where(e => e.EmployeePFTrust_Id == EmployeePFT_Id).ToList();
                        var PFTEmpLedgerOld = PFTEmpLedgerOldList.LastOrDefault();
                        //collection of PFECR data

                        var PFTEmpmast = db.EmployeePFTrust.Include(e => e.Employee)
                            .Include(e => e.Employee.GeoStruct)
                            .Include(e => e.Employee.PayStruct)
                            .Include(e => e.Employee.FuncStruct)
                            .Where(e => e.Id == EmployeePFT_Id).SingleOrDefault();

                        var PFTEmpLedgerList = new List<PFTEmployeeLedger>();
                        //Create new ECR Data object to dump in PFLedger
                        var PFTEmpLedgerECRData = new PFTEmployeeLedger()
                        {
                            GeoStruct = db.GeoStruct.Where(e => e.Id == PFTEmpmast.Employee.GeoStruct_Id).SingleOrDefault(),
                            PayStruct = db.PayStruct.Where(e => e.Id == PFTEmpmast.Employee.PayStruct_Id).SingleOrDefault(),
                            FuncStruct = db.FuncStruct.Where(e => e.Id == PFTEmpmast.Employee.FuncStruct_Id).SingleOrDefault(),
                            MonthYear = MonthYear,
                            PassbookActivity = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "512").FirstOrDefault().LookupValues.Where(e => e.LookupVal.ToUpper() == "INTEREST POSTING").SingleOrDefault(),
                            PostingDate = DateTime.Now.Date,
                            CalcDate = Convert.ToDateTime(MonthYear).Date,

                            #region Interest Input Data for ledger
                            OwnPFInt = Curr_OwnInterest,
                            OwnerPFInt = Curr_OwnerInterest,
                            VPFInt = Curr_VPFInterest,
                            PFInt = Curr_PFinterest,
                            TotPFInt = Curr_TotInterest,
                            IntOnInt = Curr_AllInterest,
                            #endregion Input Data for ledger

                            IsPassbookClose = false,
                            Narration = "Interest Credit For Month" + MonthYear.ToString(),
                            InterestFrequency = InterestFrequency.InterestFrequency,
                            DBTrack = DBTrack,
                            OwnIntOnInt = Curr_OwnInterestoninterest,
                            OwnerIntOnInt = Curr_OwnerInterestoninterest,
                            VPFIntOnInt = Curr_VPFInterestoninterest,

                        };
                        PFTEmpLedgerList.Add(PFTEmpLedgerECRData);//added pf interest data
                        //Create new ECRData Balance pftlegder object
                        var PFTEmpLedgerECRDataBalance = new PFTEmployeeLedger()
                        {
                            GeoStruct = db.GeoStruct.Where(e => e.Id == PFTEmpmast.Employee.GeoStruct_Id).SingleOrDefault(),
                            PayStruct = db.PayStruct.Where(e => e.Id == PFTEmpmast.Employee.PayStruct_Id).SingleOrDefault(),
                            FuncStruct = db.FuncStruct.Where(e => e.Id == PFTEmpmast.Employee.FuncStruct_Id).SingleOrDefault(),
                            MonthYear = MonthYear,
                            PassbookActivity = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "512").FirstOrDefault().LookupValues.Where(e => e.LookupVal.ToUpper() == "INTEREST BALANCE").SingleOrDefault(),
                            PostingDate = DateTime.Now.Date,
                            CalcDate = Convert.ToDateTime(MonthYear).Date,

                            #region Input Data for ledger
                            //OwnPFMonthly = SalaryTPF.PFECRR.EE_Share + SalaryTPF.PFECRR.Arrear_EE_Share,
                            //OwnerPFMonthly = SalaryTPF.PFECRR.ER_Share + SalaryTPF.PFECRR.Arrear_ER_Share,
                            //VPFAmountMonthly = SalaryTPF.PFECRR.EE_VPF_Share,
                            //PensionAmount = SalaryTPF.PFECRR.EPS_Share + SalaryTPF.PFECRR.Arrear_EPS_Share,
                            //PFWages = SalaryTPF.PFECRR.EPF_Wages + SalaryTPF.PFECRR.Arrear_EPF_Wages,
                            LoanAmountCredit = 0,
                            LoanAmountDebit = 0,
                            OwnPFInt = 0,
                            OwnerPFInt = 0,
                            VPFInt = 0,
                            PFInt = 0,
                            #endregion Input Data for ledger

                            OwnOpenBal = PFTEmpLedgerOld != null ? PFTEmpLedgerOld.OwnCloseBal : 0,
                            OwnCloseBal = (IntMergePF == true ? ((PFTEmpLedgerOld != null ? PFTEmpLedgerOld.OwnCloseBal : 0) + Curr_OwnInterest) : (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.OwnCloseBal : 0)),
                            OwnerOpenBal = PFTEmpLedgerOld != null ? PFTEmpLedgerOld.OwnerCloseBal : 0,
                            OwnerCloseBal = (IntMergePF == true ? ((PFTEmpLedgerOld != null ? PFTEmpLedgerOld.OwnerCloseBal : 0) + Curr_OwnerInterest) : (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.OwnerCloseBal : 0)),
                            VPFOpenBal = PFTEmpLedgerOld != null ? PFTEmpLedgerOld.VPFCloseBal : 0,
                            VPFCloseBal = (IntMergePF == true ? ((PFTEmpLedgerOld != null ? PFTEmpLedgerOld.VPFCloseBal : 0) + Curr_VPFInterest) : (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.VPFCloseBal : 0)),
                            PFOpenBal = PFTEmpLedgerOld != null ? PFTEmpLedgerOld.PFCloseBal : 0,
                            PFCloseBal = (IntMergePF == true ? ((PFTEmpLedgerOld != null ? PFTEmpLedgerOld.PFCloseBal : 0) + Curr_PFinterest) : (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.PFCloseBal : 0)),

                            OwnIntOpenBal = PFTEmpLedgerOld != null ? PFTEmpLedgerOld.OwnIntCloseBal : 0,
                            OwnIntCloseBal = (IntCarryforward == false ? ((PFTEmpLedgerOld != null ? PFTEmpLedgerOld.OwnIntCloseBal : 0)) : (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.OwnIntCloseBal : 0) + Curr_OwnInterest + Curr_OwnInterestoninterest),
                            OwnerIntOpenBal = PFTEmpLedgerOld != null ? PFTEmpLedgerOld.OwnerIntCloseBal : 0,
                            OwnerIntCloseBal = (IntCarryforward == false ? ((PFTEmpLedgerOld != null ? PFTEmpLedgerOld.OwnerIntCloseBal : 0)) : (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.OwnerIntCloseBal : 0) + Curr_OwnerInterest + Curr_OwnerInterestoninterest),
                            VPFIntOpenBal = PFTEmpLedgerOld != null ? PFTEmpLedgerOld.VPFIntCloseBal : 0,
                            VPFIntCloseBal = (IntCarryforward == false ? ((PFTEmpLedgerOld != null ? PFTEmpLedgerOld.VPFIntCloseBal : 0)) : (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.VPFIntCloseBal : 0) + Curr_VPFInterest + Curr_VPFInterestoninterest),
                            PFIntOpenBal = PFTEmpLedgerOld != null ? PFTEmpLedgerOld.PFIntCloseBal : 0,
                            PFIntCloseBal = (IntCarryforward == false ? ((PFTEmpLedgerOld != null ? PFTEmpLedgerOld.PFIntCloseBal : 0)) : (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.PFIntCloseBal : 0) + Curr_PFinterest + Curr_OwnInterestoninterest + Curr_OwnerInterestoninterest),



                            OwnPFLoan = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.OwnPFLoan : 0),
                            OwnerPFLoan = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.OwnerPFLoan : 0),
                            VPFPFLoan = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.VPFPFLoan : 0),

                            IntOnInt = (IntCarryforward == false ? ((PFTEmpLedgerOld != null ? PFTEmpLedgerOld.IntOnInt : 0)) : (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.IntOnInt : 0) + Curr_AllInterest),
                            IntonIntOpenBal = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.IntonIntOpenBal : 0),
                            IntonIntCloseBal = (IntCarryforward == false ? ((PFTEmpLedgerOld != null ? PFTEmpLedgerOld.IntonIntCloseBal : 0)) : (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.IntonIntCloseBal : 0) + Curr_AllInterest),
                            IntOnIntPFLoan = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.IntOnIntPFLoan : 0),
                            OwnIntPFLoan = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.OwnIntPFLoan : 0),
                            OwnerIntPFLoan = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.OwnerIntPFLoan : 0),
                            VPFIntPFLoan = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.VPFIntPFLoan : 0),
                            TotalIntOpenBal = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.TotalIntOpenBal : 0),
                            TotalIntCloseBal = (IntCarryforward == false ? ((PFTEmpLedgerOld != null ? PFTEmpLedgerOld.TotalIntCloseBal : 0)) :
                            (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.TotalIntCloseBal : 0) + Curr_OwnInterest + Curr_OwnerInterest + Curr_VPFInterest + Curr_PFinterest + Curr_TotInterest + Curr_AllInterest + Curr_OwnInterestoninterest + Curr_OwnerInterestoninterest + Curr_VPFInterestoninterest),

                            AccuNonTaxableAccountOpeningbalance = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.AccuNonTaxableAccountClosingbalance : 0),
                            AccuNonTaxableAccountClosingbalance = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.AccuNonTaxableAccountClosingbalance : 0) + Curr_NonTaxableInterest,
                            AccTaxableAccountOpeningbalance = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.AccTaxableAccountClosingbalance : 0),
                            AccTaxableAccountClosingbalance = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.AccTaxableAccountClosingbalance : 0) + Curr_TaxableInterest + Tds_TaxableInterest,
                            TDSAmount = Convert.ToInt32(Tds_TaxableInterest),
                            IsPassbookClose = false,
                            Narration = "Interest Balance For Month" + MonthYear.ToString(),
                            InterestFrequency = InterestFrequency.InterestFrequency,
                            DBTrack = DBTrack

                        };
                        PFTEmpLedgerList.Add(PFTEmpLedgerECRDataBalance);//added interest  data balance
                        //var PFTEmployeeLedgerAdd = db.PFTEmployeeLedger.Where(e => == Employee_Id).SingleOrDefault();
                        var EmployeePFTrust = db.EmployeePFTrust.Include(e => e.PFTEmployeeLedger).Where(e => e.Id == EmployeePFT_Id).SingleOrDefault();
                        //EmployeePFTrust.PFTEmployeeLedger.ToList().AddRange(PFTEmpLedgerList);
                        //db.SaveChanges();

                        PFTEmpLedgerList.AddRange(EmployeePFTrust.PFTEmployeeLedger);
                        EmployeePFTrust.PFTEmployeeLedger = PFTEmpLedgerList;
                        db.EmployeePFTrust.Attach(EmployeePFTrust);
                        db.Entry(EmployeePFTrust).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        db.Entry(EmployeePFTrust).State = System.Data.Entity.EntityState.Detached;
                    }
                    if (SettlementDate != null)
                    {
                        using (DataBaseContext db1 = new DataBaseContext())
                        {
                            DBTrack DBTrack1 = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            string Mpaymonth = SettlementDate.Value.ToString("MM/yyyy");
                            var PFTEmpLedgerOldListset = db1.EmployeePFTrust.Include(e => e.Employee)
                           .Include(e => e.Employee.GeoStruct)
                           .Include(e => e.Employee.PayStruct)
                           .Include(e => e.Employee.FuncStruct)
                           .Where(e => e.Id == EmployeePFT_Id).Select(r => r.EmpSettlementPFT.ToList()).SingleOrDefault();

                            var PFTEmpmastset = db1.EmployeePFTrust.Include(e => e.Employee)
                          .Include(e => e.Employee.GeoStruct)
                          .Include(e => e.Employee.PayStruct)
                          .Include(e => e.Employee.FuncStruct)
                          .Where(e => e.Id == EmployeePFT_Id).SingleOrDefault();
                            var PFTEmpLedgerOldset = PFTEmpLedgerOldListset.LastOrDefault();

                            var EmpSettlementPFTListt = new List<EmpSettlementPFT>();
                            var EmpSettlementECRData = new EmpSettlementPFT()
                            {
                                GeoStruct = db1.GeoStruct.Where(e => e.Id == PFTEmpmastset.Employee.GeoStruct_Id).SingleOrDefault(),
                                PayStruct = db1.PayStruct.Where(e => e.Id == PFTEmpmastset.Employee.PayStruct_Id).SingleOrDefault(),
                                FuncStruct = db1.FuncStruct.Where(e => e.Id == PFTEmpmastset.Employee.FuncStruct_Id).SingleOrDefault(),
                                SettlementDate = SettlementDate.Value.Date,
                                SeperationDate = SettlementDate.Value.Date,
                                PaymentDate = SettlementDate.Value.Date,
                                ChequeIssueDate = SettlementDate.Value.Date,
                                PostingDate = DateTime.Now.Date,
                                CalcDate = Convert.ToDateTime(Mpaymonth).Date,
                                MonthYear = Mpaymonth,
                                PassbookActivity = db1.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "512").FirstOrDefault().LookupValues.Where(e => e.LookupVal.ToUpper() == "SETTLEMENT POSTING").SingleOrDefault(),
                                // PostingDate = DateTime.Now.Date,
                                // CalcDate = DateTime.Now.Date,

                                #region Interest Input Data for ledger
                                OwnPfInterest = Curr_OwnInterest,
                                OwnerPfInterest = Curr_OwnerInterest,
                                VpfInt = Curr_VPFInterest,
                                PfInterest = Curr_PFinterest,
                                TotPFInt = Curr_TotInterest,
                                IntOnInt = Curr_AllInterest,
                                #endregion Input Data for ledger


                                Narration = "Settlement Interest Credit For Month" + MonthYear.ToString(),
                                InterestFrequency = InterestFrequency.InterestFrequency,
                                DBTrack = DBTrack1,
                                OwnIntOnInt = Curr_OwnInterestoninterest,
                                OwnerIntOnInt = Curr_OwnerInterestoninterest,
                                VPFIntOnInt = Curr_VPFInterestoninterest,

                            };
                            EmpSettlementPFTListt.Add(EmpSettlementECRData);//added pf interest data

                            var EmpSettlementPFTDataBalance = new EmpSettlementPFT()
                            {
                                GeoStruct = db1.GeoStruct.Where(e => e.Id == PFTEmpmastset.Employee.GeoStruct_Id).SingleOrDefault(),
                                PayStruct = db1.PayStruct.Where(e => e.Id == PFTEmpmastset.Employee.PayStruct_Id).SingleOrDefault(),
                                FuncStruct = db1.FuncStruct.Where(e => e.Id == PFTEmpmastset.Employee.FuncStruct_Id).SingleOrDefault(),
                                SettlementDate = SettlementDate.Value.Date,
                                SeperationDate = SettlementDate.Value.Date,
                                PaymentDate = SettlementDate.Value.Date,
                                ChequeIssueDate = SettlementDate.Value.Date,
                                PostingDate = DateTime.Now.Date,
                                CalcDate = Convert.ToDateTime(Mpaymonth).Date,
                                MonthYear = Mpaymonth,
                                PassbookActivity = db1.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "512").FirstOrDefault().LookupValues.Where(e => e.LookupVal.ToUpper() == "SETTLEMENT BALANCE").SingleOrDefault(),
                                //PostingDate = DateTime.Now.Date,
                                //CalcDate = DateTime.Now.Date,

                                #region Input Data for ledger
                                //OwnPFMonthly = SalaryTPF.PFECRR.EE_Share + SalaryTPF.PFECRR.Arrear_EE_Share,
                                //OwnerPFMonthly = SalaryTPF.PFECRR.ER_Share + SalaryTPF.PFECRR.Arrear_ER_Share,
                                //VPFAmountMonthly = SalaryTPF.PFECRR.EE_VPF_Share,
                                //PensionAmount = SalaryTPF.PFECRR.EPS_Share + SalaryTPF.PFECRR.Arrear_EPS_Share,
                                //PFWages = SalaryTPF.PFECRR.EPF_Wages + SalaryTPF.PFECRR.Arrear_EPF_Wages,
                                LoanAmountCredit = 0,
                                LoanAmountDebit = 0,
                                OwnPfInterest = 0,
                                OwnerPfInterest = 0,
                                VpfInt = 0,
                                PfInterest = 0,
                                #endregion Input Data for ledger

                                OwnOpenBal = PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.OwnCloseBal : 0,
                                OwnCloseBal = (IntMergePF == true ? ((PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.OwnCloseBal : 0) + Curr_OwnInterest) : (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.OwnCloseBal : 0)),
                                OwnerOpenBal = PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.OwnerCloseBal : 0,
                                OwnerCloseBal = (IntMergePF == true ? ((PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.OwnerCloseBal : 0) + Curr_OwnerInterest) : (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.OwnerCloseBal : 0)),
                                VPFOpenBal = PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.VPFCloseBal : 0,
                                VPFCloseBal = (IntMergePF == true ? ((PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.VPFCloseBal : 0) + Curr_VPFInterest) : (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.VPFCloseBal : 0)),
                                PFOpenBal = PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.PFCloseBal : 0,
                                PFCloseBal = (IntMergePF == true ? ((PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.PFCloseBal : 0) + Curr_PFinterest) : (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.PFCloseBal : 0)),

                                OwnIntOpenBal = PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.OwnIntCloseBal : 0,
                                OwnIntCloseBal = (IntCarryforward == false ? ((PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.OwnIntCloseBal : 0)) : (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.OwnIntCloseBal : 0) + Curr_OwnInterest + Curr_OwnInterestoninterest),
                                OwnerIntOpenBal = PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.OwnerIntCloseBal : 0,
                                OwnerIntCloseBal = (IntCarryforward == false ? ((PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.OwnerIntCloseBal : 0)) : (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.OwnerIntCloseBal : 0) + Curr_OwnerInterest + Curr_OwnerInterestoninterest),
                                VPFIntOpenBal = PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.VPFIntCloseBal : 0,
                                VPFIntCloseBal = (IntCarryforward == false ? ((PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.VPFIntCloseBal : 0)) : (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.VPFIntCloseBal : 0) + Curr_VPFInterest + Curr_VPFInterestoninterest),
                                PFIntOpenBal = PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.PFIntCloseBal : 0,
                                PFIntCloseBal = (IntCarryforward == false ? ((PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.PFIntCloseBal : 0)) : (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.PFIntCloseBal : 0) + Curr_PFinterest + Curr_OwnInterestoninterest + Curr_OwnerInterestoninterest),



                                OwnPFLoan = (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.OwnPFLoan : 0),
                                OwnerPFLoan = (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.OwnerPFLoan : 0),
                                VPFPFLoan = (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.VPFPFLoan : 0),

                                IntOnInt = (IntCarryforward == false ? ((PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.IntOnInt : 0)) : (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.IntOnInt : 0) + Curr_AllInterest),
                                IntonIntOpenBal = (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.IntonIntOpenBal : 0),
                                IntonIntCloseBal = (IntCarryforward == false ? ((PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.IntonIntCloseBal : 0)) : (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.IntonIntCloseBal : 0) + Curr_AllInterest),
                                IntOnIntPFLoan = (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.IntOnIntPFLoan : 0),
                                OwnIntPFLoan = (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.OwnIntPFLoan : 0),
                                OwnerIntPFLoan = (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.OwnerIntPFLoan : 0),
                                VPFIntPFLoan = (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.VPFIntPFLoan : 0),
                                TotalIntOpenBal = (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.TotalIntOpenBal : 0),
                                TotalIntCloseBal = (IntCarryforward == false ? ((PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.TotalIntCloseBal : 0)) :
                               (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.TotalIntCloseBal : 0) + Curr_OwnInterest + Curr_OwnerInterest + Curr_VPFInterest + Curr_PFinterest + Curr_TotInterest + Curr_AllInterest + Curr_OwnInterestoninterest + Curr_OwnerInterestoninterest + Curr_VPFInterestoninterest),

                                AccuNonTaxableAccountOpeningbalance = (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.AccuNonTaxableAccountClosingbalance : 0),
                                AccuNonTaxableAccountClosingbalance = (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.AccuNonTaxableAccountClosingbalance : 0) + Curr_NonTaxableInterest,
                                AccTaxableAccountOpeningbalance = (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.AccTaxableAccountClosingbalance : 0),
                                AccTaxableAccountClosingbalance = (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.AccTaxableAccountClosingbalance : 0) + Curr_TaxableInterest + Tds_TaxableInterest,
                                TDSAmount = Convert.ToInt32(Tds_TaxableInterest),
                                // IsPassbookClose = false,
                                Narration = "Settlement Interest Balance For Month" + MonthYear.ToString(),
                                InterestFrequency = InterestFrequency.InterestFrequency,
                                DBTrack = DBTrack1

                            };
                            EmpSettlementPFTListt.Add(EmpSettlementPFTDataBalance);//added interest  data balance

                            var EmployeePFTrustSett = db1.EmployeePFTrust.Include(e => e.EmpSettlementPFT).Where(e => e.Id == EmployeePFT_Id).SingleOrDefault();

                            EmpSettlementPFTListt.AddRange(EmployeePFTrustSett.EmpSettlementPFT);
                            EmployeePFTrustSett.EmpSettlementPFT = EmpSettlementPFTListt;
                            db1.EmployeePFTrust.Attach(EmployeePFTrustSett);
                            db1.Entry(EmployeePFTrustSett).State = System.Data.Entity.EntityState.Modified;
                            db1.SaveChanges();
                            db1.Entry(EmployeePFTrustSett).State = System.Data.Entity.EntityState.Detached;

                        }


                    }

                    //ReturnData.Errno = 0; //Holiday calendar not defined
                    //ReturnData.ErrMsg = "PF Sucessfully Uploaded to PF Trust for PFTrust Employees Id " + EmployeePFT_Id + " On Month " + MonthYear;

                    var employeecode = db.EmployeePFTrust.Include(e => e.Employee).Where(e => e.Id == EmployeePFT_Id).SingleOrDefault();
                    ErrorLog = new ReturnDataintp();
                    //return ReturnData;
                   // Msg.Add("Sucessfully Interest Post to PF Trust for PFTrust Employees " + employeecode.Employee.EmpCode + " On Month " + MonthYear);
                    ErrorLog.ReturnValue = EmployeePFT_Id;
                    ErrorLog.Errno = 0;
                    ErrorLog.ErrMsg = "Sucessfully Interest Post to PF Trust for PFTrust Employees " + employeecode.Employee.EmpCode + " On Month " + MonthYear;
                    ReturnDataList.Add(ErrorLog);

                }
                return ReturnDataList;

            }

            catch (Exception ex)
            {
                ErrorLog = new ReturnDataintp();
                //ReturnData.Errno = 1; //
                //ReturnData.ErrMsg = "ProcessObjectName = " + ex.Source.ToString() + " Inner ExceptionMessage = " + ex.InnerException.Message + " ExceptionMessage = " + ex.Message + " ExceptionStackTrace = " + ex.StackTrace + "LineNo = " + Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()) + "LogTime = " + DateTime.Now + "Unsuccessful PFTrust Employees Id " + EmployeePFT_Id + " On Month " + "Posting Date" + IntPostingDate.ToString();
                //return ReturnData;
               // Msg.Add("ProcessObjectName = " + ex.Source.ToString() + " Inner ExceptionMessage = " + ex.InnerException.Message + " ExceptionMessage = " + ex.Message + " ExceptionStackTrace = " + ex.StackTrace + "LineNo = " + Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()) + "LogTime = " + DateTime.Now + "Unsuccessful PFTrust Employees Id " + EmployeePFT_Id + " On Month " + "Posting Date" + IntPostingDate.ToString());
                ErrorLog.ReturnValue = EmployeePFT_Id;
                ErrorLog.Errno = 0;
                ErrorLog.ErrMsg = "ProcessObjectName = " + ex.Source.ToString() + " Inner ExceptionMessage = " + ex.InnerException.Message + " ExceptionMessage = " + ex.Message + " ExceptionStackTrace = " + ex.StackTrace + "LineNo = " + Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()) + "LogTime = " + DateTime.Now + "Unsuccessful PFTrust Employees Id " + EmployeePFT_Id + " On Month " + "Posting Date" + IntPostingDate.ToString();
                ReturnDataList.Add(ErrorLog);


            };
            return ReturnDataList;
        }
        //public static ReturnData InterestPostingAllEmp(int Company_Id, int PFTAcCalendar_Id, DateTime? IntPostingDate,
        //    DateTime? SettlementDate, DBTrack DBTrack)
        public static List<string> InterestPostingEmpWise(int Company_Id, int PFTAcCalendar_Id, DateTime? IntPostingDate, DateTime? SettlementDate, DBTrack DBTrack, string Emp)
        {
            //  var ReturnData = new ReturnData();
            List<P2BUltimate.Process.GlobalProcess.ReturnDataintp> ReturnDataList = new List<P2BUltimate.Process.GlobalProcess.ReturnDataintp>();

            List<string> Msg = new List<string>();
            try
            {
                List<int> ids = null;

                if (!string.IsNullOrEmpty(Emp))
                {
                    ids = P2BUltimate.Models.Utility.StringIdsToListIds(Emp);
                }
                using (DataBaseContext db = new DataBaseContext())
                {
                    var CompantPFTrust_Id = db.CompanyPFTrust.Where(e => e.Company_Id == Company_Id).Select(e => e.Id).SingleOrDefault();
                    var IntPolicy = db.PFTACCalendar.Include(e => e.CompanyPFTrust).Include(e => e.InterestPolicies).Include(e => e.PFTCalendar).Where(e => e.PFTCalendar.Id == PFTAcCalendar_Id && e.CompanyPFTrust.Id == CompantPFTrust_Id).SingleOrDefault();
                    var IntPolicyId = IntPolicy.InterestPolicies.Id;
                   
                   // var EmployeePFT_Ids = db.EmployeePFTrust.Include(e => e.Employee).Include(e => e.Employee.ServiceBookDates).Where(e => e.Employee.ServiceBookDates.PFExitDate == null && ids.Contains(e.Employee.Id)).Select(e => e.Id).ToList();//pfexit date checked for employee live in PF trust
                    var EmployeePFT_Ids = db.EmployeePFTrust.Include(e => e.Employee).Include(e => e.Employee.ServiceBookDates).Where(e => ids.Contains(e.Employee.Id)).Select(e => e.Id).ToList();//pfexit date checked for employee live in PF trust
                    // var EmployeePFT_Ids = db.EmployeePFTrust.Include(e => e.Employee.ServiceBookDates).Where(e => e.Employee.ServiceBookDates.PFExitDate == null && e.Id == 12).Select(e => e.Id).ToList();//pfexit date checked for employee live in PF trust
                    //var EmployeePFT_Id = db.EmployeePFTrust.Where(e => PFExistDateObj.Contains(e.Employee_Id.Value)).Select(e => e.Id).ToList();
                    foreach (var EmployeePFT_Id in EmployeePFT_Ids)
                    {
                        ReturnDataList = InterestPostingEmp(IntPolicyId, EmployeePFT_Id,
                                       IntPostingDate.Value, SettlementDate, DBTrack);
                        foreach (var item in ReturnDataList)
                        {
                            Msg.Add(item.ErrMsg);
                        }

                    }
                    return Msg;//sucess and unsucessful data
                }
            }

            catch (Exception ex)
            {
                Msg.Add("ProcessObjectName = " + ex.Source.ToString() + " Inner ExceptionMessage = " + ex.InnerException.Message + " ExceptionMessage = " + ex.Message + " ExceptionStackTrace = " + ex.StackTrace + "LineNo = " + Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()) + "LogTime = " + DateTime.Now);
                //ReturnData.Errno = 1; //
                //ReturnData.ErrMsg = "ProcessObjectName = " + ex.Source.ToString() + " Inner ExceptionMessage = " + ex.InnerException.Message + " ExceptionMessage = " + ex.Message + " ExceptionStackTrace = " + ex.StackTrace + "LineNo = " + Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()) + "LogTime = " + DateTime.Now;
                //return ReturnData;
                return Msg;
            };
        }



        //public static List<string> InterestPostingAllEmp(int Company_Id, int PFTAcCalendar_Id, DateTime? IntPostingDate,
        //  DateTime? SettlementDate, DBTrack DBTrack)
        //{
        //    //  var ReturnData = new ReturnData();
        //    List<string> Msg = new List<string>();
        //    try
        //    {


        //        using (DataBaseContext db = new DataBaseContext())
        //        {
        //            var CompantPFTrust_Id = db.CompanyPFTrust.Where(e => e.Company_Id == Company_Id).Select(e => e.Id).SingleOrDefault();
        //            var IntPolicy = db.PFTACCalendar.Include(e => e.CompanyPFTrust).Include(e => e.InterestPolicies).Include(e => e.PFTCalendar).Where(e => e.PFTCalendar.Id == PFTAcCalendar_Id && e.CompanyPFTrust.Id == CompantPFTrust_Id).SingleOrDefault();
        //            var IntPolicyId = IntPolicy.InterestPolicies.Id;
        //            //  var IntPolicyId = db.InterestPolicies.Where(e => e.PFTACCalendar_Id == PFTAcCalendar_Id).Select(e => e.Id).SingleOrDefault();

        //            var EmployeePFT_Ids = db.EmployeePFTrust.Include(e => e.Employee.ServiceBookDates).Where(e => e.Employee.ServiceBookDates.PFExitDate == null).Select(e => e.Id).ToList();//pfexit date checked for employee live in PF trust
        //            // var EmployeePFT_Ids = db.EmployeePFTrust.Include(e => e.Employee.ServiceBookDates).Where(e => e.Employee.ServiceBookDates.PFExitDate == null && e.Id == 12).Select(e => e.Id).ToList();//pfexit date checked for employee live in PF trust
        //            //var EmployeePFT_Id = db.EmployeePFTrust.Where(e => PFExistDateObj.Contains(e.Employee_Id.Value)).Select(e => e.Id).ToList();
        //            foreach (var EmployeePFT_Id in EmployeePFT_Ids)
        //            {
        //                Msg = InterestPostingEmp(IntPolicyId, EmployeePFT_Id,
        //                               IntPostingDate.Value, SettlementDate, DBTrack);

        //            }
        //            return Msg;//sucess and unsucessful data
        //        }
        //    }

        //    catch (Exception ex)
        //    {
        //        Msg.Add("ProcessObjectName = " + ex.Source.ToString() + " Inner ExceptionMessage = " + ex.InnerException.Message + " ExceptionMessage = " + ex.Message + " ExceptionStackTrace = " + ex.StackTrace + "LineNo = " + Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()) + "LogTime = " + DateTime.Now);
        //        return Msg;
        //    };
        //}24022024
        
         public static List<ReturnData> PFMthlyTEmployeeWiseECR( string MonthYear, string Emp)
       //  public static List<string> PFMthlyTEmployeeWiseECR(string MonthYear, string Emp)
        {
            //ReturnData ErrorLog = new ReturnData();
            List<int> ids = null;

            if (!string.IsNullOrEmpty(Emp))
            {
                ids = P2BUltimate.Models.Utility.StringIdsToListIds(Emp);
            }
             ReturnData ErrorLog = new ReturnData();
            List<ReturnData> ReturnDataList = new List<ReturnData>();
            List<string> Msg = new List<string>();
            try
            {
                var EmpCount = 0;
                

                using (DataBaseContext db = new DataBaseContext())
                {

                    //var SalaryT = db.SalaryT.Where(e => e.PayMonth == MonthYear && e.ReleaseDate != null).Select(e => e.Id).ToList();23022024
                    //var SalaryT = db.SalaryT.Where(e => e.PayMonth == MonthYear && e.ReleaseDate != null && e.EmployeePayroll_Id == 840).Select(e => e.Id).ToList();
                    DateTime Dateverifyfinancialyear = Convert.ToDateTime("01/" + MonthYear);

                    var PassbookLoanIDValue = new List<string>();
                    //PassbookLoanIDValue[0] = "Monthly PF Posting";
                    //PassbookLoanIDValue[1] = "PF Balance";
                    PassbookLoanIDValue.Add("Monthly PF Posting");
                    PassbookLoanIDValue.Add("PF Balance");
                    List<int> PassbookID = new List<int>();
                    PassbookID = db.LookupValue.Where(e => PassbookLoanIDValue.Contains(e.LookupVal.ToUpper())).Select(e => e.Id).ToList();

                    var Id = Convert.ToInt32(SessionManager.CompanyId);
                    var CompCodeid = db.Company.Where(e => e.Id == Id).SingleOrDefault();
                    var qurey = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "PFCALENDAR".ToUpper() && e.Default == true).AsEnumerable()
                      .Select(e => new
                      {
                          Id = e.Id,
                          Lvcalendardesc = "FromDate :" + e.FromDate.Value.ToShortDateString() + " ToDate :" + e.ToDate.Value.ToShortDateString(),
                          FromDate = e.FromDate.Value.ToShortDateString(),
                          ToDate = e.ToDate.Value.ToShortDateString()

                      }).SingleOrDefault();



                    ReturnData ReturnData = new ReturnData();
                    var CompantPFTrust_Id = db.CompanyPFTrust.Where(e => e.Company_Id == CompCodeid.Id).Select(e => e.Id).SingleOrDefault();
                    var IntPolicyId = db.PFTACCalendar.Include(e => e.PFTTDSMaster).Include(e => e.CompanyPFTrust).Include(e => e.InterestPolicies).Include(e => e.InterestPolicies.InterestRate).Where(e => e.CompanyPFTrust.Id == CompantPFTrust_Id && e.PFTCalendar.Id == qurey.Id).SingleOrDefault();

                    var PassbookLoanIDValueint = new List<string>();
                    PassbookLoanIDValueint.Add("INTEREST POSTING");
                    PassbookLoanIDValueint.Add("INTEREST BALANCE");
                    List<int> PassbookIDint = new List<int>();
                    string InterestEffectivemonth = "";
                    string uploadmonthstr = "";
                    DateTime uploadmonth = Convert.ToDateTime("01/" + MonthYear);
                    uploadmonthstr = uploadmonth.ToString("MMMM").ToUpper();
                    PassbookIDint = db.LookupValue.Where(e => PassbookLoanIDValueint.Contains(e.LookupVal.ToUpper())).Select(e => e.Id).ToList();



                    var InterestFrequency = db.InterestPolicies.Include(x => x.StatutoryEffectiveMonthsPFT)
                        .Include(m => m.StatutoryEffectiveMonthsPFT.Select(r => r.EffectiveMonth)).Where(e => e.Id == IntPolicyId.InterestPolicies.Id).SingleOrDefault();

                    var EffectiveMonths = InterestFrequency.StatutoryEffectiveMonthsPFT.Select(r => r.EffectiveMonth).ToList();
                    foreach (var item in EffectiveMonths)
                    {
                        if (uploadmonthstr == item.LookupVal.ToString().ToUpper())
                        {
                            InterestEffectivemonth = item.LookupVal.ToString().ToUpper();
                        }

                    }

                    List<string> mPeriod = new List<string>();
                    var Financialyear = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "FINANCIALYEAR".ToUpper() && e.FromDate.Value <= Dateverifyfinancialyear && e.ToDate.Value >= Dateverifyfinancialyear).SingleOrDefault();
                    var dbEmployeepayroll = db.EmployeePayroll.Include(e => e.Employee).Where(e => ids.Contains(e.Employee.Id)).Select(x => x.Id).ToList();
                    foreach (var item in dbEmployeepayroll)
                    {
                        P2B.UTILS.P2BLogger logger = new P2B.UTILS.P2BLogger();
                        string EmpPayrollID = item.ToString();
                        logger.Logging("EmpPayrollID PF UPLoad in Ledger::::  " + EmpPayrollID);
                        ErrorLog = new ReturnData();
                        DBTrack DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                        //var SalaryT = db.SalaryT.Where(e => e.PayMonth == MonthYear && e.ReleaseDate != null && e.EmployeePayroll_Id == item).Select(e => e.Id).ToList();
                        //foreach (var SalaryTitem in SalaryT)
                        //{
                        var SalaryTPF = db.SalaryT.Include(e => e.Geostruct).Include(e => e.PayStruct).Include(e => e.FuncStruct).Include(e => e.PFECRR).Where(e => e.EmployeePayroll_Id == item && e.PayMonth == MonthYear && e.ReleaseDate != null && e.EmployeePayroll_Id == item).SingleOrDefault();
                        if (SalaryTPF != null && SalaryTPF.PFECRR != null)
                        {
                            int EmployeePayroll_Id = SalaryTPF.EmployeePayroll_Id.Value;
                            var oemployee = db.EmployeePayroll.Include(e => e.Employee).Where(e => e.Id == EmployeePayroll_Id).AsNoTracking().SingleOrDefault();
                            int Employee_Id = db.EmployeePayroll.Where(e => e.Id == EmployeePayroll_Id).Select(e => e.Employee_Id.Value).SingleOrDefault();
                            #region Check record existance
                            var employeepftrustid = db.EmployeePFTrust.Where(e => e.Employee_Id == Employee_Id).SingleOrDefault();

                            var PFTEmployeeLedgerCurrent = db.PFTEmployeeLedger.Where(r => r.EmployeePFTrust_Id == employeepftrustid.Id && r.MonthYear == MonthYear
                               && PassbookID.Contains(r.PassbookActivity.Id)).ToList();

                            //var PFTEmployeeLedgerCurrent = db.EmployeePFTrust.Where(e => e.Employee_Id == Employee_Id)
                            //    .Select(e => e.PFTEmployeeLedger.Where(r => r.MonthYear == MonthYear
                            //    && PassbookID.Contains(r.PassbookActivity.Id)).ToList()).SingleOrDefault();
                            // Last Recor PF Balance
                            if (PFTEmployeeLedgerCurrent != null && PFTEmployeeLedgerCurrent.Count() > 0)
                            {
                                var PFTEmployeeLedgerCurrentLast = PFTEmployeeLedgerCurrent.OrderBy(e => e.Id).LastOrDefault();
                                var PFbalance = db.PFTEmployeeLedger.Include(e => e.PassbookActivity).Where(e => e.Id == PFTEmployeeLedgerCurrentLast.Id).SingleOrDefault();
                                if (PFbalance.PassbookActivity.LookupVal.ToUpper() == "PF BALANCE")
                                {
                                    var empLedgerhistory = db.EmployeePFTrust.Include(e => e.PFTEmployeeLedger).Where(e => e.Employee_Id == Employee_Id).AsNoTracking().SingleOrDefault();
                                    var emprecord = empLedgerhistory.PFTEmployeeLedger.Where(e => e.Id >= PFTEmployeeLedgerCurrentLast.Id).ToList();
                                    if (emprecord.Count() == 1)
                                    {
                                        if (PFTEmployeeLedgerCurrent != null)
                                        {
                                            db.PFTEmployeeLedger.RemoveRange(PFTEmployeeLedgerCurrent);
                                            db.SaveChanges();
                                        }
                                    }
                                    else
                                    {
                                      //  Msg.Add("PF Upload is not allowed Employee PF Trust ledger " + oemployee.Employee.EmpCode + " Next Month activity has done");
                                        ErrorLog.Errno = 0;
                                        ErrorLog.ErrMsg = "PF Upload is not allowed Employee PF Trust ledger " + oemployee.Employee.EmpCode + " Next Month activity has done";
                                        ErrorLog.ReturnValue = Employee_Id;
                                        ReturnDataList.Add(ErrorLog);

                                        continue;
                                    }
                                }

                            }
                            if (InterestEffectivemonth == uploadmonthstr)
                            {
                                var PFTEmployeeLedgerCurrentint = db.EmployeePFTrust.Where(e => e.Employee_Id == Employee_Id)
                           .Select(e => e.PFTEmployeeLedger.Where(r => r.MonthYear == MonthYear
                           && PassbookIDint.Contains(r.PassbookActivity.Id)).ToList()).SingleOrDefault();
                                if (PFTEmployeeLedgerCurrentint.Count() == 0)
                                {
                                   // Msg.Add("First Post Interest For the month " + MonthYear + " of Employee code " + oemployee.Employee.EmpCode + " Then Upload PF");
                                    ErrorLog.Errno = 1;
                                    ErrorLog.ErrMsg ="First Post Interest For the month " + MonthYear + " of Employee code " + oemployee.Employee.EmpCode + " Then Upload PF";
                                    ErrorLog.ReturnValue = Employee_Id;
                                    ReturnDataList.Add(ErrorLog);
                                    continue;
                                }

                            }

                            #endregion Check record existance
                            //previous record to fetch closing balance
                            var PFTEmpLedgerOldList = db.EmployeePFTrust.Where(e => e.Employee_Id == Employee_Id).Select(r => r.PFTEmployeeLedger.ToList()).SingleOrDefault();
                            if (PFTEmpLedgerOldList != null)
                            {
                                var PFTEmpLedgerOld = PFTEmpLedgerOldList.LastOrDefault();
                                double NOntaxableaccpfmonthly = 0;
                                double Taxableaccpfmonthly = 0;
                                double NontaxInt = 0;
                                double TaxableaccInt = 0;

                                if (IntPolicyId != null && IntPolicyId.PFTTDSMaster != null)
                                {
                                    if ((IntPolicyId.PFTTDSMaster.IsOwnTDSAppl == true) && (IntPolicyId.PFTTDSMaster.IsOwnerTDSAppl == true) && (IntPolicyId.PFTTDSMaster.IsVPFTDSAppl == true))
                                    {
                                        NOntaxableaccpfmonthly = SalaryTPF.PFECRR.EE_Share + SalaryTPF.PFECRR.Arrear_EE_Share + SalaryTPF.PFECRR.ER_Share + SalaryTPF.PFECRR.Arrear_ER_Share + SalaryTPF.PFECRR.EE_VPF_Share;

                                    }
                                    if ((IntPolicyId.PFTTDSMaster.IsOwnTDSAppl == true) && (IntPolicyId.PFTTDSMaster.IsOwnerTDSAppl == true) && (IntPolicyId.PFTTDSMaster.IsVPFTDSAppl == false))
                                    {
                                        NOntaxableaccpfmonthly = SalaryTPF.PFECRR.EE_Share + SalaryTPF.PFECRR.Arrear_EE_Share + SalaryTPF.PFECRR.ER_Share + SalaryTPF.PFECRR.Arrear_ER_Share;

                                    }
                                    if ((IntPolicyId.PFTTDSMaster.IsOwnTDSAppl == true) && (IntPolicyId.PFTTDSMaster.IsVPFTDSAppl == true) && (IntPolicyId.PFTTDSMaster.IsOwnerTDSAppl == false))
                                    {
                                        NOntaxableaccpfmonthly = SalaryTPF.PFECRR.EE_Share + SalaryTPF.PFECRR.Arrear_EE_Share + SalaryTPF.PFECRR.EE_VPF_Share;

                                    }
                                    if ((IntPolicyId.PFTTDSMaster.IsOwnTDSAppl == true) && (IntPolicyId.PFTTDSMaster.IsVPFTDSAppl == false) && (IntPolicyId.PFTTDSMaster.IsOwnerTDSAppl == false))
                                    {
                                        NOntaxableaccpfmonthly = SalaryTPF.PFECRR.EE_Share + SalaryTPF.PFECRR.Arrear_EE_Share;

                                    }
                                    if ((IntPolicyId.PFTTDSMaster.IsVPFTDSAppl == true) && (IntPolicyId.PFTTDSMaster.IsOwnerTDSAppl == false) && (IntPolicyId.PFTTDSMaster.IsOwnTDSAppl == false))
                                    {
                                        NOntaxableaccpfmonthly = SalaryTPF.PFECRR.EE_VPF_Share;

                                    }
                                    if ((IntPolicyId.PFTTDSMaster.IsOwnerTDSAppl == true) && (IntPolicyId.PFTTDSMaster.IsOwnTDSAppl == false) && (IntPolicyId.PFTTDSMaster.IsVPFTDSAppl == false))
                                    {
                                        NOntaxableaccpfmonthly = SalaryTPF.PFECRR.ER_Share + SalaryTPF.PFECRR.Arrear_ER_Share;

                                    }
                                    if (IntPolicyId.PFTTDSMaster.IsOwnTDSAppl == false && IntPolicyId.PFTTDSMaster.IsOwnerTDSAppl == false && IntPolicyId.PFTTDSMaster.IsVPFTDSAppl == false)
                                    {
                                        NOntaxableaccpfmonthly = 0;

                                    }
                                    var intrateeff = IntPolicyId.InterestPolicies.InterestRate.FirstOrDefault();

                                    ReturnData = InterestRate(IntPolicyId.Id, intrateeff.EffectiveFrom, 0, 0);

                                    if (PFTEmpLedgerOld != null)
                                    {
                                        if (Dateverifyfinancialyear == Financialyear.FromDate.Value.Date)
                                        {
                                            // PFTEmpLedgerOld.AccuNonTaxableAccountFy_Closingbalance financial year start closeing balance 0
                                            if ((NOntaxableaccpfmonthly) > IntPolicyId.PFTTDSMaster.TaxableAccountCelling)
                                            {
                                                Taxableaccpfmonthly = NOntaxableaccpfmonthly - IntPolicyId.PFTTDSMaster.TaxableAccountCelling;
                                                NOntaxableaccpfmonthly = NOntaxableaccpfmonthly - Taxableaccpfmonthly;
                                            }

                                        }

                                        if (Dateverifyfinancialyear > Financialyear.FromDate.Value.Date && Dateverifyfinancialyear <= Financialyear.ToDate.Value.Date)
                                        {
                                            if ((PFTEmpLedgerOld.AccuNonTaxableAccountFy_Closingbalance + NOntaxableaccpfmonthly) > IntPolicyId.PFTTDSMaster.TaxableAccountCelling)
                                            {
                                                Taxableaccpfmonthly = PFTEmpLedgerOld.AccuNonTaxableAccountFy_Closingbalance + NOntaxableaccpfmonthly - IntPolicyId.PFTTDSMaster.TaxableAccountCelling;
                                                NOntaxableaccpfmonthly = NOntaxableaccpfmonthly - Taxableaccpfmonthly;
                                            }
                                        }

                                    }
                                    NontaxInt = (NOntaxableaccpfmonthly * (ReturnData.ReturnValue / (12 * 100)));
                                    NontaxInt = Math.Round(NontaxInt + 0.001, 0);
                                    TaxableaccInt = (Taxableaccpfmonthly * (ReturnData.ReturnValue / (12 * 100)));
                                    TaxableaccInt = Math.Round(TaxableaccInt + 0.001, 0);

                                    //Taxableaccpfmonthly=PFTEmpLedgerOld != null ? iif(PFTEmpLedgerOld.AccuNonTaxableAccountClosingbalance+NOntaxableaccpfmonthly>IntPolicyId.PFTTDSMaster.TaxableAccountCelling) : 0
                                }

                                //collection of PFECR data
                                var PFTEmpLedgerList = new List<PFTEmployeeLedger>();
                                //Create new ECR Data object to dump in PFLedger
                                var PFTEmpLedgerECRData = new PFTEmployeeLedger()
                                {
                                    GeoStruct = SalaryTPF.Geostruct,
                                    PayStruct = SalaryTPF.PayStruct,
                                    FuncStruct = SalaryTPF.FuncStruct,
                                    MonthYear = SalaryTPF.PayMonth,
                                    PassbookActivity = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "512").FirstOrDefault().LookupValues.Where(e => e.LookupVal.ToUpper() == "MONTHLY PF POSTING").SingleOrDefault(),
                                    PostingDate = DateTime.Now.Date,
                                    CalcDate = Convert.ToDateTime(SalaryTPF.PayMonth).Date,//SalaryTPF.PaymentDate.Value, //SalaryTPF.PaymentDate null in table

                                    #region Input Data for ledger
                                    OwnPFMonthly = SalaryTPF.PFECRR.EE_Share + SalaryTPF.PFECRR.Arrear_EE_Share,
                                    OwnerPFMonthly = SalaryTPF.PFECRR.ER_Share + SalaryTPF.PFECRR.Arrear_ER_Share,
                                    VPFAmountMonthly = SalaryTPF.PFECRR.EE_VPF_Share,
                                    PensionAmount = SalaryTPF.PFECRR.EPS_Share + SalaryTPF.PFECRR.Arrear_EPS_Share,
                                    PFWages = SalaryTPF.PFECRR.EPF_Wages + SalaryTPF.PFECRR.Arrear_EPF_Wages,
                                    #endregion Input Data for ledger

                                    IsPassbookClose = false,
                                    Narration = "PF Posting For Month" + SalaryTPF.PayMonth.ToString(),
                                    InterestFrequency = null,
                                    DBTrack = DBTrack,
                                    AccuNonTaxableAccountFy_Closingbalance = NOntaxableaccpfmonthly,
                                    NonTaxableAccountMonthly = NOntaxableaccpfmonthly,
                                    NonTaxableAccountInterest = NontaxInt,
                                    TaxableAccountFy_Closingbalance = Taxableaccpfmonthly,
                                    TaxableAccountMonthly = Taxableaccpfmonthly,
                                    TaxableAccountInterest = TaxableaccInt,
                                };
                                PFTEmpLedgerList.Add(PFTEmpLedgerECRData);//added monthly PF data
                                //Create new ECRData Balance pftlegder object

                                double AccuNonTaxableAccountFy_Closingbalancepost = 0;
                                double AccuNonTaxableAccountFy_Openingbalancepost = 0;
                                double TaxableAccountFy_Closingbalancepost = 0;
                                double TaxableAccountFy_Openingbalancepost = 0;
                                double NonTaxableAccountInterestpost = 0;
                                double TaxableAccountInterestpost = 0;
                                if (Dateverifyfinancialyear == Financialyear.FromDate.Value.Date)
                                {
                                    AccuNonTaxableAccountFy_Closingbalancepost = NOntaxableaccpfmonthly;
                                    AccuNonTaxableAccountFy_Openingbalancepost = 0;

                                    TaxableAccountFy_Closingbalancepost = Taxableaccpfmonthly;
                                    TaxableAccountFy_Openingbalancepost = 0;

                                    NonTaxableAccountInterestpost = NontaxInt;
                                    TaxableAccountInterestpost = TaxableaccInt;


                                }


                                if (Dateverifyfinancialyear > Financialyear.FromDate.Value.Date && Dateverifyfinancialyear <= Financialyear.ToDate.Value.Date)
                                {
                                    AccuNonTaxableAccountFy_Closingbalancepost = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.AccuNonTaxableAccountFy_Closingbalance : 0) + NOntaxableaccpfmonthly;//check taxableaccpFy
                                    AccuNonTaxableAccountFy_Openingbalancepost = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.AccuNonTaxableAccountFy_Closingbalance : 0);
                                    if (IntPolicyId.PFTTDSMaster != null)
                                    {
                                        if (AccuNonTaxableAccountFy_Closingbalancepost > IntPolicyId.PFTTDSMaster.TaxableAccountCelling)
                                        {
                                            AccuNonTaxableAccountFy_Closingbalancepost = IntPolicyId.PFTTDSMaster.TaxableAccountCelling;
                                        }
                                    }

                                    TaxableAccountFy_Closingbalancepost = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.TaxableAccountFy_Closingbalance : 0) + Taxableaccpfmonthly;
                                    TaxableAccountFy_Openingbalancepost = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.TaxableAccountFy_Closingbalance : 0);


                                    NonTaxableAccountInterestpost = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.NonTaxableAccountInterest : 0) + NontaxInt;
                                    TaxableAccountInterestpost = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.TaxableAccountInterest : 0) + TaxableaccInt;

                                }
                                var PFTEmpLedgerECRDataBalance = new PFTEmployeeLedger()
                                {
                                    GeoStruct = SalaryTPF.Geostruct,
                                    PayStruct = SalaryTPF.PayStruct,
                                    FuncStruct = SalaryTPF.FuncStruct,
                                    MonthYear = SalaryTPF.PayMonth,
                                    PassbookActivity = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "512").FirstOrDefault().LookupValues.Where(e => e.LookupVal.ToUpper() == "PF BALANCE").SingleOrDefault(),
                                    PostingDate = DateTime.Now.Date,
                                    CalcDate = Convert.ToDateTime(SalaryTPF.PayMonth).Date,//SalaryTPF.PaymentDate.Value, //SalaryTPF.PaymentDate null in table

                                    #region Input Data for ledger
                                    //OwnPFMonthly = SalaryTPF.PFECRR.EE_Share + SalaryTPF.PFECRR.Arrear_EE_Share,
                                    //OwnerPFMonthly = SalaryTPF.PFECRR.ER_Share + SalaryTPF.PFECRR.Arrear_ER_Share,
                                    //VPFAmountMonthly = SalaryTPF.PFECRR.EE_VPF_Share,
                                    //PensionAmount = SalaryTPF.PFECRR.EPS_Share + SalaryTPF.PFECRR.Arrear_EPS_Share,
                                    //PFWages = SalaryTPF.PFECRR.EPF_Wages + SalaryTPF.PFECRR.Arrear_EPF_Wages,
                                    LoanAmountCredit = 0,
                                    LoanAmountDebit = 0,
                                    OwnPFInt = 0,
                                    OwnerPFInt = 0,
                                    VPFInt = 0,
                                    PFInt = 0,
                                    #endregion Input Data for ledger

                                    OwnOpenBal = PFTEmpLedgerOld != null ? PFTEmpLedgerOld.OwnCloseBal : 0,
                                    OwnCloseBal = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.OwnCloseBal : 0) + SalaryTPF.PFECRR.EE_Share + SalaryTPF.PFECRR.Arrear_EE_Share,
                                    OwnerOpenBal = PFTEmpLedgerOld != null ? PFTEmpLedgerOld.OwnerCloseBal : 0,
                                    OwnerCloseBal = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.OwnerCloseBal : 0) + SalaryTPF.PFECRR.ER_Share + SalaryTPF.PFECRR.Arrear_ER_Share,
                                    VPFOpenBal = PFTEmpLedgerOld != null ? PFTEmpLedgerOld.VPFCloseBal : 0,
                                    VPFCloseBal = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.VPFCloseBal : 0) + SalaryTPF.PFECRR.EE_VPF_Share,
                                    PFOpenBal = PFTEmpLedgerOld != null ? PFTEmpLedgerOld.PFCloseBal : 0,
                                    PFCloseBal = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.PFCloseBal : 0) + SalaryTPF.PFECRR.EE_Share + SalaryTPF.PFECRR.ER_Share + SalaryTPF.PFECRR.Arrear_EE_Share + SalaryTPF.PFECRR.Arrear_ER_Share,

                                    OwnIntOpenBal = PFTEmpLedgerOld != null ? PFTEmpLedgerOld.OwnIntCloseBal : 0,
                                    OwnIntCloseBal = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.OwnIntCloseBal : 0),
                                    OwnerIntOpenBal = PFTEmpLedgerOld != null ? PFTEmpLedgerOld.OwnerIntCloseBal : 0,
                                    OwnerIntCloseBal = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.OwnerIntCloseBal : 0),
                                    VPFIntOpenBal = PFTEmpLedgerOld != null ? PFTEmpLedgerOld.VPFIntCloseBal : 0,
                                    VPFIntCloseBal = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.VPFIntCloseBal : 0),
                                    PFIntOpenBal = PFTEmpLedgerOld != null ? PFTEmpLedgerOld.PFIntCloseBal : 0,
                                    PFIntCloseBal = PFTEmpLedgerOld != null ? PFTEmpLedgerOld.PFIntCloseBal : 0,



                                    OwnPFLoan = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.OwnPFLoan : 0),
                                    OwnerPFLoan = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.OwnerPFLoan : 0),
                                    VPFPFLoan = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.VPFPFLoan : 0),

                                    IntOnInt = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.IntOnInt : 0),
                                    IntonIntOpenBal = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.IntonIntCloseBal : 0),
                                    IntonIntCloseBal = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.IntonIntCloseBal : 0),
                                    IntOnIntPFLoan = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.IntOnIntPFLoan : 0),
                                    OwnIntPFLoan = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.OwnIntPFLoan : 0),
                                    OwnerIntPFLoan = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.OwnerIntPFLoan : 0),
                                    VPFIntPFLoan = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.VPFIntPFLoan : 0),
                                    TotalIntOpenBal = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.TotalIntCloseBal : 0),
                                    TotalIntCloseBal = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.TotalIntCloseBal : 0),


                                    IsPassbookClose = false,
                                    Narration = "PF Balance For Month" + SalaryTPF.PayMonth.ToString(),
                                    InterestFrequency = null,
                                    DBTrack = DBTrack,
                                    AccuNonTaxableAccountOpeningbalance = PFTEmpLedgerOld != null ? PFTEmpLedgerOld.AccuNonTaxableAccountClosingbalance : 0,
                                    AccuNonTaxableAccountClosingbalance = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.AccuNonTaxableAccountClosingbalance : 0) + NOntaxableaccpfmonthly,
                                    AccTaxableAccountOpeningbalance = PFTEmpLedgerOld != null ? PFTEmpLedgerOld.AccTaxableAccountClosingbalance : 0,
                                    AccTaxableAccountClosingbalance = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.AccTaxableAccountClosingbalance : 0) + Taxableaccpfmonthly,

                                    AccuNonTaxableAccountFy_Openingbalance = AccuNonTaxableAccountFy_Openingbalancepost,//PFTEmpLedgerOld != null ? PFTEmpLedgerOld.AccuNonTaxableAccountFy_Closingbalance : 0,
                                    AccuNonTaxableAccountFy_Closingbalance = AccuNonTaxableAccountFy_Closingbalancepost,//check taxableaccpFy
                                    TaxableAccountFy_Openingbalance = TaxableAccountFy_Openingbalancepost,//PFTEmpLedgerOld != null ? PFTEmpLedgerOld.TaxableAccountFy_Closingbalance : 0,
                                    TaxableAccountFy_Closingbalance = TaxableAccountFy_Closingbalancepost,//(PFTEmpLedgerOld != null ? PFTEmpLedgerOld.TaxableAccountFy_Closingbalance : 0) + Taxableaccpfmonthly,

                                    NonTaxableAccountInterest = NonTaxableAccountInterestpost,//(PFTEmpLedgerOld != null ? PFTEmpLedgerOld.NonTaxableAccountInterest : 0) + NontaxInt,
                                    TaxableAccountInterest = TaxableAccountInterestpost,//(PFTEmpLedgerOld != null ? PFTEmpLedgerOld.TaxableAccountInterest : 0) + TaxableaccInt,


                                };
                                PFTEmpLedgerList.Add(PFTEmpLedgerECRDataBalance);//added monthly PF data balance
                                //var PFTEmployeeLedgerAdd = db.PFTEmployeeLedger.Where(e => == Employee_Id).SingleOrDefault();
                                var EmployeePFTrust = db.EmployeePFTrust.Include(e => e.PFTEmployeeLedger).Where(e => e.Employee_Id == Employee_Id).SingleOrDefault();
                                // EmployeePFTrust.PFTEmployeeLedger.ToList().AddRange(PFTEmpLedgerList);
                                PFTEmpLedgerList.AddRange(EmployeePFTrust.PFTEmployeeLedger);
                                EmployeePFTrust.PFTEmployeeLedger = PFTEmpLedgerList;
                                db.EmployeePFTrust.Attach(EmployeePFTrust);
                                db.Entry(EmployeePFTrust).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(EmployeePFTrust).State = System.Data.Entity.EntityState.Detached;
                                EmpCount++;
                            }

                        }
                        else// Retire or exit employee but not settlement
                        {
                            int EmployeePayroll_Id = item;
                            var oemployee = db.EmployeePayroll.Include(e => e.Employee)
                                .Include(e => e.Employee.GeoStruct)
                                .Include(e => e.Employee.PayStruct)
                                .Include(e => e.Employee.FuncStruct)
                                .Include(e => e.Employee.EmpOffInfo).Where(e => e.Id == EmployeePayroll_Id).AsNoTracking().SingleOrDefault();
                            int Employee_Id = db.EmployeePayroll.Where(e => e.Id == EmployeePayroll_Id).Select(e => e.Employee_Id.Value).SingleOrDefault();
                            #region Check record existance
                            var employeepftrustid = db.EmployeePFTrust.Where(e => e.Employee_Id == Employee_Id).SingleOrDefault();

                            var PFTEmployeeLedgerCurrent = db.PFTEmployeeLedger.Where(r => r.EmployeePFTrust_Id == employeepftrustid.Id && r.MonthYear == MonthYear
                               && PassbookID.Contains(r.PassbookActivity.Id)).ToList();

                            //var PFTEmployeeLedgerCurrent = db.EmployeePFTrust.Where(e => e.Employee_Id == Employee_Id)
                            //    .Select(e => e.PFTEmployeeLedger.Where(r => r.MonthYear == MonthYear
                            //    && PassbookID.Contains(r.PassbookActivity.Id)).ToList()).SingleOrDefault();
                            // Last Recor PF Balance
                            if (oemployee != null)
                            {
                                if (oemployee.Employee.EmpOffInfo.PFAppl == false)
                                {
                                  //  Msg.Add("PF not Applicable " + oemployee.Employee.EmpCode);
                                    ErrorLog.Errno = 2;
                                    ErrorLog.ErrMsg ="PF not Applicable " + oemployee.Employee.EmpCode;
                                    ErrorLog.ReturnValue = Employee_Id;
                                    ReturnDataList.Add(ErrorLog);
                                    continue;
                                }
                            }
                            if (PFTEmployeeLedgerCurrent != null && PFTEmployeeLedgerCurrent.Count() > 0)
                            {
                                var PFTEmployeeLedgerCurrentLast = PFTEmployeeLedgerCurrent.OrderBy(e => e.Id).LastOrDefault();
                                var PFbalance = db.PFTEmployeeLedger.Include(e => e.PassbookActivity).Where(e => e.Id == PFTEmployeeLedgerCurrentLast.Id).SingleOrDefault();
                                if (PFbalance.PassbookActivity.LookupVal.ToUpper() == "PF BALANCE")
                                {
                                    var empLedgerhistory = db.EmployeePFTrust.Include(e => e.PFTEmployeeLedger).Where(e => e.Employee_Id == Employee_Id).AsNoTracking().SingleOrDefault();
                                    var emprecord = empLedgerhistory.PFTEmployeeLedger.Where(e => e.Id >= PFTEmployeeLedgerCurrentLast.Id).ToList();
                                    if (emprecord.Count() == 1)
                                    {
                                        if (PFTEmployeeLedgerCurrent != null)
                                        {
                                            db.PFTEmployeeLedger.RemoveRange(PFTEmployeeLedgerCurrent);
                                            db.SaveChanges();
                                        }
                                    }
                                    else
                                    {
                                      //  Msg.Add("PF Upload is not allowed Employee PF Trust ledger " + oemployee.Employee.EmpCode + " Next Month activity has done");
                                    ErrorLog.Errno = 3;
                                    ErrorLog.ErrMsg ="PF Upload is not allowed Employee PF Trust ledger " + oemployee.Employee.EmpCode + " Next Month activity has done";
                                    ErrorLog.ReturnValue = Employee_Id;
                                    ReturnDataList.Add(ErrorLog);
                                        continue;
                                    }
                                }

                            }
                            if (InterestEffectivemonth == uploadmonthstr)
                            {
                                var PFTEmployeeLedgerCurrentint = db.EmployeePFTrust.Where(e => e.Employee_Id == Employee_Id)
                           .Select(e => e.PFTEmployeeLedger.Where(r => r.MonthYear == MonthYear
                           && PassbookIDint.Contains(r.PassbookActivity.Id)).ToList()).SingleOrDefault();
                                if (PFTEmployeeLedgerCurrentint.Count() == 0)
                                {
                                   // Msg.Add("First Post Interest For the month " + MonthYear + " of Employee code " + oemployee.Employee.EmpCode + " Then Upload PF");
                                    ErrorLog.Errno = 4;
                                    ErrorLog.ErrMsg ="First Post Interest For the month " + MonthYear + " of Employee code " + oemployee.Employee.EmpCode + " Then Upload PF";
                                    ErrorLog.ReturnValue = Employee_Id;
                                    ReturnDataList.Add(ErrorLog);
                                    continue;
                                }

                            }

                            #endregion Check record existance
                            //previous record to fetch closing balance
                            var PFTEmpLedgerOldList = db.EmployeePFTrust.Where(e => e.Employee_Id == Employee_Id).Select(r => r.PFTEmployeeLedger.ToList()).SingleOrDefault();
                            if (PFTEmpLedgerOldList != null)
                            {
                                var PFTEmpLedgerOld = PFTEmpLedgerOldList.LastOrDefault();
                                double NOntaxableaccpfmonthly = 0;
                                double Taxableaccpfmonthly = 0;
                                double NontaxInt = 0;
                                double TaxableaccInt = 0;

                                if (IntPolicyId != null && IntPolicyId.PFTTDSMaster != null)
                                {
                                    if ((IntPolicyId.PFTTDSMaster.IsOwnTDSAppl == true) && (IntPolicyId.PFTTDSMaster.IsOwnerTDSAppl == true) && (IntPolicyId.PFTTDSMaster.IsVPFTDSAppl == true))
                                    {
                                        NOntaxableaccpfmonthly = 0;// SalaryTPF.PFECRR.EE_Share + SalaryTPF.PFECRR.Arrear_EE_Share + SalaryTPF.PFECRR.ER_Share + SalaryTPF.PFECRR.Arrear_ER_Share + SalaryTPF.PFECRR.EE_VPF_Share;

                                    }
                                    if ((IntPolicyId.PFTTDSMaster.IsOwnTDSAppl == true) && (IntPolicyId.PFTTDSMaster.IsOwnerTDSAppl == true) && (IntPolicyId.PFTTDSMaster.IsVPFTDSAppl == false))
                                    {
                                        NOntaxableaccpfmonthly = 0;//SalaryTPF.PFECRR.EE_Share + SalaryTPF.PFECRR.Arrear_EE_Share + SalaryTPF.PFECRR.ER_Share + SalaryTPF.PFECRR.Arrear_ER_Share;

                                    }
                                    if ((IntPolicyId.PFTTDSMaster.IsOwnTDSAppl == true) && (IntPolicyId.PFTTDSMaster.IsVPFTDSAppl == true) && (IntPolicyId.PFTTDSMaster.IsOwnerTDSAppl == false))
                                    {
                                        NOntaxableaccpfmonthly = 0;//SalaryTPF.PFECRR.EE_Share + SalaryTPF.PFECRR.Arrear_EE_Share + SalaryTPF.PFECRR.EE_VPF_Share;

                                    }
                                    if ((IntPolicyId.PFTTDSMaster.IsOwnTDSAppl == true) && (IntPolicyId.PFTTDSMaster.IsVPFTDSAppl == false) && (IntPolicyId.PFTTDSMaster.IsOwnerTDSAppl == false))
                                    {
                                        NOntaxableaccpfmonthly = 0;//SalaryTPF.PFECRR.EE_Share + SalaryTPF.PFECRR.Arrear_EE_Share;

                                    }
                                    if ((IntPolicyId.PFTTDSMaster.IsVPFTDSAppl == true) && (IntPolicyId.PFTTDSMaster.IsOwnerTDSAppl == false) && (IntPolicyId.PFTTDSMaster.IsOwnTDSAppl == false))
                                    {
                                        NOntaxableaccpfmonthly = 0;//SalaryTPF.PFECRR.EE_VPF_Share;

                                    }
                                    if ((IntPolicyId.PFTTDSMaster.IsOwnerTDSAppl == true) && (IntPolicyId.PFTTDSMaster.IsOwnTDSAppl == false) && (IntPolicyId.PFTTDSMaster.IsVPFTDSAppl == false))
                                    {
                                        NOntaxableaccpfmonthly = 0;//SalaryTPF.PFECRR.ER_Share + SalaryTPF.PFECRR.Arrear_ER_Share;

                                    }
                                    if (IntPolicyId.PFTTDSMaster.IsOwnTDSAppl == false && IntPolicyId.PFTTDSMaster.IsOwnerTDSAppl == false && IntPolicyId.PFTTDSMaster.IsVPFTDSAppl == false)
                                    {
                                        NOntaxableaccpfmonthly = 0;

                                    }
                                    var intrateeff = IntPolicyId.InterestPolicies.InterestRate.FirstOrDefault();

                                    ReturnData = InterestRate(IntPolicyId.Id, intrateeff.EffectiveFrom, 0, 0);

                                    if (PFTEmpLedgerOld != null)
                                    {
                                        if (Dateverifyfinancialyear == Financialyear.FromDate.Value.Date)
                                        {
                                            // PFTEmpLedgerOld.AccuNonTaxableAccountFy_Closingbalance financial year start closeing balance 0
                                            if ((NOntaxableaccpfmonthly) > IntPolicyId.PFTTDSMaster.TaxableAccountCelling)
                                            {
                                                Taxableaccpfmonthly = NOntaxableaccpfmonthly - IntPolicyId.PFTTDSMaster.TaxableAccountCelling;
                                                NOntaxableaccpfmonthly = NOntaxableaccpfmonthly - Taxableaccpfmonthly;
                                            }

                                        }

                                        if (Dateverifyfinancialyear > Financialyear.FromDate.Value.Date && Dateverifyfinancialyear <= Financialyear.ToDate.Value.Date)
                                        {
                                            if ((PFTEmpLedgerOld.AccuNonTaxableAccountFy_Closingbalance + NOntaxableaccpfmonthly) > IntPolicyId.PFTTDSMaster.TaxableAccountCelling)
                                            {
                                                Taxableaccpfmonthly = PFTEmpLedgerOld.AccuNonTaxableAccountFy_Closingbalance + NOntaxableaccpfmonthly - IntPolicyId.PFTTDSMaster.TaxableAccountCelling;
                                                NOntaxableaccpfmonthly = NOntaxableaccpfmonthly - Taxableaccpfmonthly;
                                            }
                                        }

                                    }
                                    NontaxInt = (NOntaxableaccpfmonthly * (ReturnData.ReturnValue / (12 * 100)));
                                    NontaxInt = Math.Round(NontaxInt + 0.001, 0);
                                    TaxableaccInt = (Taxableaccpfmonthly * (ReturnData.ReturnValue / (12 * 100)));
                                    TaxableaccInt = Math.Round(TaxableaccInt + 0.001, 0);

                                    //Taxableaccpfmonthly=PFTEmpLedgerOld != null ? iif(PFTEmpLedgerOld.AccuNonTaxableAccountClosingbalance+NOntaxableaccpfmonthly>IntPolicyId.PFTTDSMaster.TaxableAccountCelling) : 0
                                }

                                //collection of PFECR data
                                var PFTEmpLedgerList = new List<PFTEmployeeLedger>();
                                //Create new ECR Data object to dump in PFLedger
                                var PFTEmpLedgerECRData = new PFTEmployeeLedger()
                                {
                                    GeoStruct = oemployee.Employee.GeoStruct,//SalaryTPF.Geostruct,
                                    PayStruct = oemployee.Employee.PayStruct,//SalaryTPF.PayStruct,
                                    FuncStruct = oemployee.Employee.FuncStruct,//SalaryTPF.FuncStruct,
                                    MonthYear = MonthYear,
                                    PassbookActivity = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "512").FirstOrDefault().LookupValues.Where(e => e.LookupVal.ToUpper() == "MONTHLY PF POSTING").SingleOrDefault(),
                                    PostingDate = DateTime.Now.Date,
                                    CalcDate = Convert.ToDateTime(MonthYear).Date,//SalaryTPF.PaymentDate.Value, //SalaryTPF.PaymentDate null in table

                                    #region Input Data for ledger
                                    OwnPFMonthly = 0,//SalaryTPF.PFECRR.EE_Share + SalaryTPF.PFECRR.Arrear_EE_Share,
                                    OwnerPFMonthly = 0,//SalaryTPF.PFECRR.ER_Share + SalaryTPF.PFECRR.Arrear_ER_Share,
                                    VPFAmountMonthly = 0,//SalaryTPF.PFECRR.EE_VPF_Share,
                                    PensionAmount = 0,//SalaryTPF.PFECRR.EPS_Share + SalaryTPF.PFECRR.Arrear_EPS_Share,
                                    PFWages = 0,//SalaryTPF.PFECRR.EPF_Wages + SalaryTPF.PFECRR.Arrear_EPF_Wages,
                                    #endregion Input Data for ledger

                                    IsPassbookClose = false,
                                    Narration = "PF Posting For Month" + MonthYear.ToString(),
                                    InterestFrequency = null,
                                    DBTrack = DBTrack,
                                    AccuNonTaxableAccountFy_Closingbalance = NOntaxableaccpfmonthly,
                                    NonTaxableAccountMonthly = NOntaxableaccpfmonthly,
                                    NonTaxableAccountInterest = NontaxInt,
                                    TaxableAccountFy_Closingbalance = Taxableaccpfmonthly,
                                    TaxableAccountMonthly = Taxableaccpfmonthly,
                                    TaxableAccountInterest = TaxableaccInt,
                                };
                                PFTEmpLedgerList.Add(PFTEmpLedgerECRData);//added monthly PF data
                                //Create new ECRData Balance pftlegder object

                                double AccuNonTaxableAccountFy_Closingbalancepost = 0;
                                double AccuNonTaxableAccountFy_Openingbalancepost = 0;
                                double TaxableAccountFy_Closingbalancepost = 0;
                                double TaxableAccountFy_Openingbalancepost = 0;
                                double NonTaxableAccountInterestpost = 0;
                                double TaxableAccountInterestpost = 0;
                                if (Dateverifyfinancialyear == Financialyear.FromDate.Value.Date)
                                {
                                    AccuNonTaxableAccountFy_Closingbalancepost = NOntaxableaccpfmonthly;
                                    AccuNonTaxableAccountFy_Openingbalancepost = 0;

                                    TaxableAccountFy_Closingbalancepost = Taxableaccpfmonthly;
                                    TaxableAccountFy_Openingbalancepost = 0;

                                    NonTaxableAccountInterestpost = NontaxInt;
                                    TaxableAccountInterestpost = TaxableaccInt;


                                }


                                if (Dateverifyfinancialyear > Financialyear.FromDate.Value.Date && Dateverifyfinancialyear <= Financialyear.ToDate.Value.Date)
                                {
                                    AccuNonTaxableAccountFy_Closingbalancepost = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.AccuNonTaxableAccountFy_Closingbalance : 0) + NOntaxableaccpfmonthly;//check taxableaccpFy
                                    AccuNonTaxableAccountFy_Openingbalancepost = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.AccuNonTaxableAccountFy_Closingbalance : 0);
                                    if (IntPolicyId.PFTTDSMaster != null)
                                    {
                                        if (AccuNonTaxableAccountFy_Closingbalancepost > IntPolicyId.PFTTDSMaster.TaxableAccountCelling)
                                        {
                                            AccuNonTaxableAccountFy_Closingbalancepost = IntPolicyId.PFTTDSMaster.TaxableAccountCelling;
                                        }
                                    }

                                    TaxableAccountFy_Closingbalancepost = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.TaxableAccountFy_Closingbalance : 0) + Taxableaccpfmonthly;
                                    TaxableAccountFy_Openingbalancepost = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.TaxableAccountFy_Closingbalance : 0);


                                    NonTaxableAccountInterestpost = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.NonTaxableAccountInterest : 0) + NontaxInt;
                                    TaxableAccountInterestpost = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.TaxableAccountInterest : 0) + TaxableaccInt;

                                }
                                var PFTEmpLedgerECRDataBalance = new PFTEmployeeLedger()
                                {
                                    GeoStruct = oemployee.Employee.GeoStruct,// SalaryTPF.Geostruct,
                                    PayStruct = oemployee.Employee.PayStruct,//SalaryTPF.PayStruct,
                                    FuncStruct = oemployee.Employee.FuncStruct,//SalaryTPF.FuncStruct,
                                    MonthYear = MonthYear,
                                    PassbookActivity = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "512").FirstOrDefault().LookupValues.Where(e => e.LookupVal.ToUpper() == "PF BALANCE").SingleOrDefault(),
                                    PostingDate = DateTime.Now.Date,
                                    CalcDate = Convert.ToDateTime(MonthYear).Date,//SalaryTPF.PaymentDate.Value, //SalaryTPF.PaymentDate null in table

                                    #region Input Data for ledger
                                    //OwnPFMonthly = SalaryTPF.PFECRR.EE_Share + SalaryTPF.PFECRR.Arrear_EE_Share,
                                    //OwnerPFMonthly = SalaryTPF.PFECRR.ER_Share + SalaryTPF.PFECRR.Arrear_ER_Share,
                                    //VPFAmountMonthly = SalaryTPF.PFECRR.EE_VPF_Share,
                                    //PensionAmount = SalaryTPF.PFECRR.EPS_Share + SalaryTPF.PFECRR.Arrear_EPS_Share,
                                    //PFWages = SalaryTPF.PFECRR.EPF_Wages + SalaryTPF.PFECRR.Arrear_EPF_Wages,
                                    LoanAmountCredit = 0,
                                    LoanAmountDebit = 0,
                                    OwnPFInt = 0,
                                    OwnerPFInt = 0,
                                    VPFInt = 0,
                                    PFInt = 0,
                                    #endregion Input Data for ledger

                                    OwnOpenBal = PFTEmpLedgerOld != null ? PFTEmpLedgerOld.OwnCloseBal : 0,
                                    OwnCloseBal = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.OwnCloseBal : 0) + 0, //SalaryTPF.PFECRR.EE_Share + SalaryTPF.PFECRR.Arrear_EE_Share,
                                    OwnerOpenBal = PFTEmpLedgerOld != null ? PFTEmpLedgerOld.OwnerCloseBal : 0,
                                    OwnerCloseBal = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.OwnerCloseBal : 0) + 0, //SalaryTPF.PFECRR.ER_Share + SalaryTPF.PFECRR.Arrear_ER_Share,
                                    VPFOpenBal = PFTEmpLedgerOld != null ? PFTEmpLedgerOld.VPFCloseBal : 0,
                                    VPFCloseBal = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.VPFCloseBal : 0) + 0, //SalaryTPF.PFECRR.EE_VPF_Share,
                                    PFOpenBal = PFTEmpLedgerOld != null ? PFTEmpLedgerOld.PFCloseBal : 0,
                                    PFCloseBal = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.PFCloseBal : 0) + 0, // SalaryTPF.PFECRR.EE_Share + SalaryTPF.PFECRR.ER_Share + SalaryTPF.PFECRR.Arrear_EE_Share + SalaryTPF.PFECRR.Arrear_ER_Share,

                                    OwnIntOpenBal = PFTEmpLedgerOld != null ? PFTEmpLedgerOld.OwnIntCloseBal : 0,
                                    OwnIntCloseBal = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.OwnIntCloseBal : 0),
                                    OwnerIntOpenBal = PFTEmpLedgerOld != null ? PFTEmpLedgerOld.OwnerIntCloseBal : 0,
                                    OwnerIntCloseBal = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.OwnerIntCloseBal : 0),
                                    VPFIntOpenBal = PFTEmpLedgerOld != null ? PFTEmpLedgerOld.VPFIntCloseBal : 0,
                                    VPFIntCloseBal = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.VPFIntCloseBal : 0),
                                    PFIntOpenBal = PFTEmpLedgerOld != null ? PFTEmpLedgerOld.PFIntCloseBal : 0,
                                    PFIntCloseBal = PFTEmpLedgerOld != null ? PFTEmpLedgerOld.PFIntCloseBal : 0,



                                    OwnPFLoan = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.OwnPFLoan : 0),
                                    OwnerPFLoan = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.OwnerPFLoan : 0),
                                    VPFPFLoan = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.VPFPFLoan : 0),

                                    IntOnInt = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.IntOnInt : 0),
                                    IntonIntOpenBal = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.IntonIntCloseBal : 0),
                                    IntonIntCloseBal = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.IntonIntCloseBal : 0),
                                    IntOnIntPFLoan = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.IntOnIntPFLoan : 0),
                                    OwnIntPFLoan = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.OwnIntPFLoan : 0),
                                    OwnerIntPFLoan = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.OwnerIntPFLoan : 0),
                                    VPFIntPFLoan = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.VPFIntPFLoan : 0),
                                    TotalIntOpenBal = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.TotalIntCloseBal : 0),
                                    TotalIntCloseBal = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.TotalIntCloseBal : 0),


                                    IsPassbookClose = false,
                                    Narration = "PF Balance For Month" + MonthYear.ToString(),
                                    InterestFrequency = null,
                                    DBTrack = DBTrack,
                                    AccuNonTaxableAccountOpeningbalance = PFTEmpLedgerOld != null ? PFTEmpLedgerOld.AccuNonTaxableAccountClosingbalance : 0,
                                    AccuNonTaxableAccountClosingbalance = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.AccuNonTaxableAccountClosingbalance : 0) + NOntaxableaccpfmonthly,
                                    AccTaxableAccountOpeningbalance = PFTEmpLedgerOld != null ? PFTEmpLedgerOld.AccTaxableAccountClosingbalance : 0,
                                    AccTaxableAccountClosingbalance = (PFTEmpLedgerOld != null ? PFTEmpLedgerOld.AccTaxableAccountClosingbalance : 0) + Taxableaccpfmonthly,

                                    AccuNonTaxableAccountFy_Openingbalance = AccuNonTaxableAccountFy_Openingbalancepost,//PFTEmpLedgerOld != null ? PFTEmpLedgerOld.AccuNonTaxableAccountFy_Closingbalance : 0,
                                    AccuNonTaxableAccountFy_Closingbalance = AccuNonTaxableAccountFy_Closingbalancepost,//check taxableaccpFy
                                    TaxableAccountFy_Openingbalance = TaxableAccountFy_Openingbalancepost,//PFTEmpLedgerOld != null ? PFTEmpLedgerOld.TaxableAccountFy_Closingbalance : 0,
                                    TaxableAccountFy_Closingbalance = TaxableAccountFy_Closingbalancepost,//(PFTEmpLedgerOld != null ? PFTEmpLedgerOld.TaxableAccountFy_Closingbalance : 0) + Taxableaccpfmonthly,

                                    NonTaxableAccountInterest = NonTaxableAccountInterestpost,//(PFTEmpLedgerOld != null ? PFTEmpLedgerOld.NonTaxableAccountInterest : 0) + NontaxInt,
                                    TaxableAccountInterest = TaxableAccountInterestpost,//(PFTEmpLedgerOld != null ? PFTEmpLedgerOld.TaxableAccountInterest : 0) + TaxableaccInt,


                                };
                                PFTEmpLedgerList.Add(PFTEmpLedgerECRDataBalance);//added monthly PF data balance
                                //var PFTEmployeeLedgerAdd = db.PFTEmployeeLedger.Where(e => == Employee_Id).SingleOrDefault();
                                var EmployeePFTrust = db.EmployeePFTrust.Include(e => e.PFTEmployeeLedger).Where(e => e.Employee_Id == Employee_Id).SingleOrDefault();
                                // EmployeePFTrust.PFTEmployeeLedger.ToList().AddRange(PFTEmpLedgerList);
                                PFTEmpLedgerList.AddRange(EmployeePFTrust.PFTEmployeeLedger);
                                EmployeePFTrust.PFTEmployeeLedger = PFTEmpLedgerList;
                                db.EmployeePFTrust.Attach(EmployeePFTrust);
                                db.Entry(EmployeePFTrust).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(EmployeePFTrust).State = System.Data.Entity.EntityState.Detached;
                                EmpCount++;
                            }
                        }

                        //    }

                        
                    }
                }
               // Msg.Add("PF Sucessfully Uploaded to PF Trust total Employees count " + EmpCount.ToString() + " On Month " + MonthYear);
                ErrorLog = new ReturnData();
                               ErrorLog.Errno = 5;
                                    ErrorLog.ErrMsg ="PF Sucessfully Uploaded to PF Trust total Employees count " + EmpCount.ToString() + " On Month " + MonthYear;
                                    ReturnDataList.Add(ErrorLog);
            }
            catch (Exception ex)
            {
                ErrorLog = new ReturnData();
                //Msg.Add("ProcessObjectName = " + ex.Source.ToString() + " Inner ExceptionMessage = " + ex.InnerException.Message + " ExceptionMessage = " + ex.Message + " ExceptionStackTrace = " + ex.StackTrace + "LineNo = " + Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()) + "LogTime = " + DateTime.Now);
                   ErrorLog.Errno = 5;
                   ErrorLog.ErrMsg ="ProcessObjectName = " + ex.Source.ToString() + " Inner ExceptionMessage = " + ex.InnerException.Message + " ExceptionMessage = " + ex.Message + " ExceptionStackTrace = " + ex.StackTrace + "LineNo = " + Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()) + "LogTime = " + DateTime.Now;
                   ReturnDataList.Add(ErrorLog);
            };
            return ReturnDataList;
        }
    }
}
