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
using P2BUltimate.Security;
using System.IO;


namespace P2BUltimate.Process
{
    public class IncomeTaxCalc
    {

        public static Double[] TDSCalc(EmployeePayroll OEmployeePayroll, IncomeTax OITMaster, double mTotalITIncome, DateTime mToPeriod, Int32 OFinancialYear)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string OEmpSex = OEmployeePayroll.Employee.Gender.LookupVal.ToUpper();
                //******* gender look up checking *************//
                //  OEmpSex = "MALE";
                DateTime start = OEmployeePayroll.Employee.ServiceBookDates.BirthDate.Value;
                DateTime end = mToPeriod;
                int compMonth = (end.Month + end.Year * 12) - (start.Month + start.Year * 12);
                // double daysInEndMonth = (end - end.AddMonths(1)).Days;
                double daysInEndMonth = (end.AddMonths(1) - end).Days;
                double months = compMonth + Math.Abs((start.Day - end.Day) / daysInEndMonth);
                double mAge = Math.Abs(months / 12);
                string mCategory = "OTHERS";
                if (OEmpSex != null)
                {
                    if (OEmpSex == "MALE")
                    {
                        if (mAge >= 60)
                        {
                            mCategory = "SENIOR CITIZEN";
                        }
                        else
                        {
                            mCategory = "OTHERS";
                        }
                    }
                    else
                    {
                        if (mAge >= 60)
                        {
                            mCategory = "SENIOR CITIZEN";
                        }
                        else
                        {
                            mCategory = "WOMEN";
                        }
                    }
                }
                // for new tax slab start 28122020
                Calendar temp_OFinancialYear = db.Calendar.Where(e => e.Id == OFinancialYear).SingleOrDefault();
                DateTime finfrm = Convert.ToDateTime("01/04/2020");

                var Regimi = db.EmployeePayroll.Include(e => e.RegimiScheme).Include(e => e.RegimiScheme.Select(x => x.FinancialYear)).Include(e => e.RegimiScheme.Select(y => y.Scheme)).Where(e => e.Id == OEmployeePayroll.Id).FirstOrDefault();
                var Regimischemecurryear = Regimi.RegimiScheme.Where(e => e.FinancialYear.Id == temp_OFinancialYear.Id).FirstOrDefault();
                if (Regimischemecurryear != null)
                {
                    if (Regimischemecurryear.Scheme.LookupVal.ToUpper() == "NEWTAX")
                    {
                        mCategory = "2021";
                    }
                }
                else
                {


                    if (temp_OFinancialYear.FromDate >= finfrm && OEmployeePayroll.Employee.EmpOffInfo.NationalityID.No2 == "Yes")
                    {
                        mCategory = "2021";
                    }
                }
                // for new tax slab end 28122020

                List<ITTDS> OITTDS = OITMaster.ITTDS.Where(e => e.Category.LookupVal.ToUpper() == mCategory).OrderBy(r => r.IncomeRangeFrom).ToList();

                //EmpSalStruct OEmpSal = new EmpSalStruct();
                //OEmpSal = db.EmpSalStruct.Where(e => e.EmployeePayroll.Id == OEmployeePayroll.Id && e.EndDate == null).SingleOrDefault();
                //if (OEmpSal == null)
                //{
                //    OEmpSal = db.EmpSalStruct.Where(e => e.EmployeePayroll.Id == OEmployeePayroll.Id).OrderByDescending(e => e.EffectiveDate).FirstOrDefault();
                //}

                //List<EmpSalStructDetails> EmpSalStructDetails = db.EmpSalStructDetails.Where(e => e.EmpSalStruct_Id == OEmpSal.Id).ToList();
                //OEmpSal.EmpSalStructDetails = EmpSalStructDetails;
                //foreach (var EmpSalStructDetailsitem in EmpSalStructDetails)
                //{
                //    SalaryHead SalaryHead = db.SalaryHead.Where(e => e.Id == EmpSalStructDetailsitem.SalaryHead_Id).SingleOrDefault();
                //    EmpSalStructDetailsitem.SalaryHead = SalaryHead;
                //    LookupValue SalHeadOperationType = db.LookupValue.Where(e => e.Id == SalaryHead.SalHeadOperationType_Id).SingleOrDefault();
                //    SalaryHead.SalHeadOperationType = SalHeadOperationType;
                //}

                //.Include(e => e.EmpSalStructDetails)
                //   .Include(e => e.EmpSalStructDetails.Select(r => r.SalaryHead))
                //   .Include(e => e.EmpSalStructDetails.Select(r => r.SalaryHead.SalHeadOperationType))


                ////EmpSalStruct OEmpSal = OEmployeePayroll.EmpSalStruct.Where(e => e.EndDate == null).SingleOrDefault();
                ////if (OEmpSal == null)
                ////{
                ////    OEmpSal = OEmployeePayroll.EmpSalStruct.LastOrDefault();
                ////}

                //EmpSalStruct OEmpSal = OEmpSalStructSingleOrDefault();
                //if (OEmpSal == null)
                //{
                //    OEmpSal = OEmpSalStruct.LastOrDefault();
                //}
                //EmpSalStructDetails OSalaryHead = OEmpSal.EmpSalStructDetails.Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "ITAX").SingleOrDefault();
                var OSalaryHead = db.SalaryHead.Include(e => e.RoundingMethod).Where(e => e.SalHeadOperationType.LookupVal.ToUpper() == "ITAX").SingleOrDefault();
                Double[] OTDSDetails = new Double[5];
                if (OSalaryHead == null || mTotalITIncome == 0)
                {
                    //    //define income tax head
                    OTDSDetails[1] = 0;
                    OTDSDetails[2] = 0;
                    OTDSDetails[3] = 0;
                    OTDSDetails[4] = 0;

                    return OTDSDetails;
                }
                double mITTax = 0;
                double mITCess = 0;
                double mITSurcharge = 0;
                foreach (ITTDS OITTDSData in OITTDS)
                {
                    OTDSDetails[1] = OITTDSData.EduCessPercent;
                    OTDSDetails[2] = OITTDSData.SurchargePercent;
                    OTDSDetails[3] = OITTDSData.EduCessAmount;
                    OTDSDetails[4] = OITTDSData.SurchargeAmount;

                    if (mTotalITIncome > OITTDSData.IncomeRangeFrom && mTotalITIncome <= OITTDSData.IncomeRangeTo)
                    {
                        double OTDSTempFinal = ((mTotalITIncome - OITTDSData.IncomeRangeFrom) * OITTDSData.Percentage / 100) + OITTDSData.Amount;
                        mITTax = mITTax + OTDSTempFinal;
                        mITTax = Math.Round(mITTax);
                        //mITCess = mITCess + (OTDSTempFinal * OTDSDetails[1] / 100) + OTDSDetails[3];
                        //mITSurcharge = mITSurcharge + (OTDSTempFinal * OTDSDetails[2] / 100) + OTDSDetails[4];

                        break;
                    }
                    else
                    {

                        double OTDSTemp = ((OITTDSData.IncomeRangeTo - OITTDSData.IncomeRangeFrom) * OITTDSData.Percentage / 100) + OITTDSData.Amount;
                        mITTax = mITTax + OTDSTemp;
                        //mITCess = mITCess + (OTDSTemp * OTDSDetails[1] / 100) + OTDSDetails[3];
                        //mITSurcharge = mITSurcharge + (OTDSTemp * OTDSDetails[2] / 100) + OTDSDetails[4];

                    }
                }
                OTDSDetails[0] = Process.SalaryHeadGenProcess.RoundingFunction(OSalaryHead, mITTax);
                //OTDSDetails[1] = Math.Round(mITCess,0);
                //OTDSDetails[2] = Math.Round(mITSurcharge,0);
                return OTDSDetails;
            }
        }
        public static List<ITProjection> SaveITForecastingData(ITProjection OITProjection, List<ITProjection> OITProjectionList)
        {
            ITProjection OITProjectionSave = new ITProjection
            {
                Tiltle = OITProjection.Tiltle,
                Form16Header = OITProjection.Form16Header,
                Form24Header = OITProjection.Form24Header,
                SectionType = OITProjection.SectionType,
                Section = OITProjection.Section,
                FinancialYear = OITProjection.FinancialYear,
                FromPeriod = OITProjection.FromPeriod,
                ToPeriod = OITProjection.ToPeriod,
                ChapterName = OITProjection.ChapterName,
                SubChapter = OITProjection.SubChapter,
                SalayHead = OITProjection.SalayHead,
                //SalaryHeadName=OITProjection.SalaryHeadName,
                ProjectedAmount = OITProjection.ProjectedAmount,
                ActualAmount = OITProjection.ActualAmount,
                ProjectedQualifyingAmount = OITProjection.ProjectedQualifyingAmount,
                ActualQualifyingAmount = OITProjection.ActualQualifyingAmount,
                QualifiedAmount = OITProjection.QualifiedAmount,
                TDSComponents = OITProjection.TDSComponents,
                ProjectionDate = OITProjection.ProjectionDate,
                IsLocked = OITProjection.IsLocked,
                Narration = OITProjection.Narration,

            };
            try
            {
                //db.ITProjection.Add(OITProjectionSave);
                //db.SaveChanges();
                OITProjectionList.Add(OITProjectionSave);

            }
            catch (Exception e)
            {

                throw (e);

            }
            return OITProjectionList;
        }
        public static List<SalaryT> _returnEmployee_SalaryHeadMonthData_old(Int32 Emp, DateTime mFromPeriod, DateTime mToPeriod)
        {
            // SalaryHeadMonthData
            using (DataBaseContext db = new DataBaseContext())
            {
                //_returnEmployeePayroll_SalaryT
                /*var a = db.EmployeePayroll
                       .Include(e => e.SalaryT)
                       .Include(e => e.SalaryT.Select(r => r.PerkTransT))
                       .Include(e => e.SalaryT.Select(r => r.PerkTransT.Select(t => t.SalaryHead)))
                       .Include(e => e.SalaryT.Select(r => r.SalEarnDedT))
                       .Include(e => e.SalaryT.Select(r => r.SalEarnDedT.Select(w => w.SalaryHead)))
                       .Include(e => e.SalaryT.Select(r => r.SalEarnDedT.Select(w => w.SalaryHead.Frequency)))
                       .Include(e => e.SalaryT.Select(r => r.SalEarnDedT.Select(w => w.SalaryHead.Type)))
                       .Include(e => e.SalaryT.Select(r => r.SalEarnDedT.Select(w => w.SalaryHead.SalHeadOperationType)))
                       .Include(e => e.SalaryT.Select(r => r.SalEarnDedT.Select(w => w.SalaryHead.RoundingMethod)))
                       .Include(e => e.SalaryT.Select(r => r.SalEarnDedT.Select(w => w.SalaryHead.ProcessType)))
                       .Include(e => e.PerkTransT)
                       .Include(e => e.PerkTransT.Select(r => r.SalaryHead))
                       .Include(e => e.PerkTransT.Select(r => r.SalaryHead.SalHeadOperationType));

                return a.Where(e => e.Id == Emp).SingleOrDefault();
*/
                //return db.SalaryT
                //    .Include(r => r.PerkTransT)
                //    .Include(r => r.PerkTransT.Select(t => t.SalaryHead))
                //    .Include(r => r.SalEarnDedT)
                //    .Include(r => r.SalEarnDedT.Select(w => w.SalaryHead))
                //    .Include(r => r.SalEarnDedT.Select(w => w.SalaryHead.Frequency))
                //    .Include(r => r.SalEarnDedT.Select(w => w.SalaryHead.Type))
                //    .Include(r => r.SalEarnDedT.Select(w => w.SalaryHead.SalHeadOperationType))
                //    .Include(r => r.SalEarnDedT.Select(w => w.SalaryHead.RoundingMethod))
                //    .Include(r => r.SalEarnDedT.Select(w => w.SalaryHead.ProcessType))
                //    //.Include(e => e.PerkTransT)
                //    //.Include(e => e.PerkTransT.Select(r => r.SalaryHead))
                //    //.Include(e => e.PerkTransT.Select(r => r.SalaryHead.SalHeadOperationType))
                //   .AsNoTracking().Where(e => e.EmployeePayroll_Id == Emp).ToList();

                // new code start 16/09/2022
                List<string> mPeriodYear = new List<string>();
                string mPeriodRangeYear = "";
                for (DateTime mTempDate = mFromPeriod; mTempDate <= mToPeriod; mTempDate = mTempDate.AddMonths(1))
                {
                    if (mPeriodRangeYear == "")
                    {
                        mPeriodRangeYear = Convert.ToDateTime(mTempDate).ToString("MM/yyyy");
                        mPeriodYear.Add(Convert.ToDateTime(mTempDate).ToString("MM/yyyy"));
                    }
                    else
                    {
                        mPeriodRangeYear = mPeriodRangeYear + "," + Convert.ToDateTime(mTempDate).ToString("MM/yyyy");
                        mPeriodYear.Add(Convert.ToDateTime(mTempDate).ToString("MM/yyyy"));
                    }
                }

                List<SalaryT> SalaryT = db.SalaryT.Where(e => e.EmployeePayroll_Id == Emp && mPeriodYear.Contains(e.PayMonth)).ToList();
                foreach (var SalaryTItem in SalaryT)
                {

                    List<SalEarnDedT> SalEarnDedT = db.SalEarnDedT.Where(e => e.SalaryT_Id == SalaryTItem.Id).ToList();

                    //db.SalaryT.Where(e => e.Id == J.Id).Select(r => r.SalEarnDedT.ToList()).FirstOrDefault();
                    foreach (var SalEarnDedTItem in SalEarnDedT)
                    {

                        SalaryHead SalaryHead = db.SalaryHead.Where(e => e.Id == SalEarnDedTItem.SalaryHead_Id).SingleOrDefault();
                        SalEarnDedTItem.SalaryHead = SalaryHead;
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

                        //int SalEarnDedTObj = db.SalEarnDedT.AsNoTracking().Where(e => e.Id == i.Id).Select(r => r.SalaryHead.Id).FirstOrDefault();
                        //var SalaryHead = db.SalaryHead.Where(e => e.Id == SalEarnDedTObj).FirstOrDefault();
                        //var SalHeadOperationType = db.SalaryHead.Where(e => e.Id == SalEarnDedTObj).Select(r => r.SalHeadOperationType).FirstOrDefault();
                        //var Type = db.SalaryHead.Where(e => e.Id == SalEarnDedTObj).Select(r => r.Type).FirstOrDefault();
                        //var Frequency = db.SalaryHead.Where(e => e.Id == SalEarnDedTObj).Select(r => r.Frequency).FirstOrDefault();
                        //var ProcessType = db.SalaryHead.Where(e => e.Id == SalEarnDedTObj).Select(r => r.ProcessType).FirstOrDefault();
                        //var RoundingMethod = db.SalaryHead.Where(e => e.Id == SalEarnDedTObj).Select(r => r.RoundingMethod).FirstOrDefault();

                        //i.SalaryHead = SalaryHead;
                        //i.SalaryHead.SalHeadOperationType = SalHeadOperationType;
                        //i.SalaryHead.Type = Type;
                        //i.SalaryHead.Frequency = Frequency;
                        //i.SalaryHead.ProcessType = ProcessType;
                        //i.SalaryHead.RoundingMethod = RoundingMethod;
                    }
                    SalaryTItem.SalEarnDedT = SalEarnDedT;
                    List<PerkTransT> PerkTransT = db.SalaryT.Where(e => e.Id == SalaryTItem.Id).Select(e => e.PerkTransT.ToList()).SingleOrDefault();
                    SalaryTItem.PerkTransT = PerkTransT;
                    //db.SalaryT.Where(e => e.Id == J.Id).Select(r => r.PerkTransT.ToList()).FirstOrDefault();
                    foreach (var PerkTransTItem in PerkTransT)
                    {
                        // var PerkTransTObj = db.PerkTransT.Where(e => e.Id == k.Id).Select(r => r.SalaryHead).FirstOrDefault();
                        //var SalaryHead = db.SalaryHead.Where(e => e.Id == PerkTransTObj.Id).FirstOrDefault();
                        //k.SalaryHead = SalaryHead;
                        SalaryHead SalaryHead = db.SalaryHead.Where(e => e.Id == PerkTransTItem.SalaryHead_Id).SingleOrDefault();
                        PerkTransTItem.SalaryHead = SalaryHead;

                    }
                    //J.SalEarnDedT = SalEarnDedT;
                    //J.PerkTransT = PerkTransT;

                }

                return SalaryT;
                // new code End 16/09/2022

                ////return db.EmployeePayroll.Where(e => e.Id == Emp)
                ////       .Include(e => e.SalaryT)
                ////       .Include(e => e.SalaryT.Select(r => r.PerkTransT))
                ////       .Include(e => e.SalaryT.Select(r => r.PerkTransT.Select(t => t.SalaryHead)))
                ////       .Include(e => e.SalaryT.Select(r => r.SalEarnDedT))
                ////       .Include(e => e.SalaryT.Select(r => r.SalEarnDedT.Select(w => w.SalaryHead)))
                ////       .Include(e => e.SalaryT.Select(r => r.SalEarnDedT.Select(w => w.SalaryHead.Frequency)))
                ////       .Include(e => e.SalaryT.Select(r => r.SalEarnDedT.Select(w => w.SalaryHead.Type)))
                ////       .Include(e => e.SalaryT.Select(r => r.SalEarnDedT.Select(w => w.SalaryHead.SalHeadOperationType)))
                ////       .Include(e => e.SalaryT.Select(r => r.SalEarnDedT.Select(w => w.SalaryHead.RoundingMethod)))
                ////       .Include(e => e.SalaryT.Select(r => r.SalEarnDedT.Select(w => w.SalaryHead.ProcessType)))
                ////       .Include(e => e.SalaryArrearT)
                ////       .Include(e => e.SalaryArrearT.Select(r => r.ArrearType))
                ////       .Include(e => e.SalaryArrearT.Select(r => r.SalaryArrearPaymentT))
                ////       .Include(e => e.SalaryArrearT.Select(r => r.SalaryArrearPaymentT.Select(t => t.SalaryHead)))
                ////       .Include(e => e.YearlyPaymentT)
                ////       .Include(e => e.YearlyPaymentT.Select(r => r.SalaryHead))
                ////       .Include(e => e.YearlyPaymentT.Select(r => r.SalaryHead.SalHeadOperationType))
                ////       .Include(e => e.YearlyPaymentT.Select(r => r.SalaryHead.Frequency))
                ////       .Include(e => e.YearlyPaymentT.Select(r => r.SalaryHead.Type))
                ////       .Include(e => e.YearlyPaymentT.Select(r => r.SalaryHead.RoundingMethod))
                ////       .Include(e => e.YearlyPaymentT.Select(r => r.SalaryHead.ProcessType))
                ////       .Include(e => e.PerkTransT)
                ////       .Include(e => e.PerkTransT.Select(r => r.SalaryHead))
                ////       .Include(e => e.PerkTransT.Select(r => r.SalaryHead.SalHeadOperationType))
                ////    //.Include(e => e.SalaryArrearT.Select(r => r.SalaryArrearPaymentT.Select(w => w.SalaryArrearPFT)))

                ////       .SingleOrDefault();
            }
        }
        public static List<SalaryT> _returnEmployee_SalaryHeadMonthData(Int32 Emp, DateTime mFromPeriod, DateTime mToPeriod)
        {
            // SalaryHeadMonthData
            using (DataBaseContext db = new DataBaseContext())
            {
                //_returnEmployeePayroll_SalaryT
                /*var a = db.EmployeePayroll
                       .Include(e => e.SalaryT)
                       .Include(e => e.SalaryT.Select(r => r.PerkTransT))
                       .Include(e => e.SalaryT.Select(r => r.PerkTransT.Select(t => t.SalaryHead)))
                       .Include(e => e.SalaryT.Select(r => r.SalEarnDedT))
                       .Include(e => e.SalaryT.Select(r => r.SalEarnDedT.Select(w => w.SalaryHead)))
                       .Include(e => e.SalaryT.Select(r => r.SalEarnDedT.Select(w => w.SalaryHead.Frequency)))
                       .Include(e => e.SalaryT.Select(r => r.SalEarnDedT.Select(w => w.SalaryHead.Type)))
                       .Include(e => e.SalaryT.Select(r => r.SalEarnDedT.Select(w => w.SalaryHead.SalHeadOperationType)))
                       .Include(e => e.SalaryT.Select(r => r.SalEarnDedT.Select(w => w.SalaryHead.RoundingMethod)))
                       .Include(e => e.SalaryT.Select(r => r.SalEarnDedT.Select(w => w.SalaryHead.ProcessType)))
                       .Include(e => e.PerkTransT)
                       .Include(e => e.PerkTransT.Select(r => r.SalaryHead))
                       .Include(e => e.PerkTransT.Select(r => r.SalaryHead.SalHeadOperationType));

                return a.Where(e => e.Id == Emp).SingleOrDefault();
*/
                //return db.SalaryT
                //    .Include(r => r.PerkTransT)
                //    .Include(r => r.PerkTransT.Select(t => t.SalaryHead))
                //    .Include(r => r.SalEarnDedT)
                //    .Include(r => r.SalEarnDedT.Select(w => w.SalaryHead))
                //    .Include(r => r.SalEarnDedT.Select(w => w.SalaryHead.Frequency))
                //    .Include(r => r.SalEarnDedT.Select(w => w.SalaryHead.Type))
                //    .Include(r => r.SalEarnDedT.Select(w => w.SalaryHead.SalHeadOperationType))
                //    .Include(r => r.SalEarnDedT.Select(w => w.SalaryHead.RoundingMethod))
                //    .Include(r => r.SalEarnDedT.Select(w => w.SalaryHead.ProcessType))
                //    //.Include(e => e.PerkTransT)
                //    //.Include(e => e.PerkTransT.Select(r => r.SalaryHead))
                //    //.Include(e => e.PerkTransT.Select(r => r.SalaryHead.SalHeadOperationType))
                //   .AsNoTracking().Where(e => e.EmployeePayroll_Id == Emp).ToList();

                // new code start 16/09/2022
                List<string> mPeriodYear = new List<string>();
                string mPeriodRangeYear = "";
                for (DateTime mTempDate = mFromPeriod; mTempDate <= mToPeriod; mTempDate = mTempDate.AddMonths(1))
                {
                    if (mPeriodRangeYear == "")
                    {
                        mPeriodRangeYear = Convert.ToDateTime(mTempDate).ToString("MM/yyyy");
                        mPeriodYear.Add(Convert.ToDateTime(mTempDate).ToString("MM/yyyy"));
                    }
                    else
                    {
                        mPeriodRangeYear = mPeriodRangeYear + "," + Convert.ToDateTime(mTempDate).ToString("MM/yyyy");
                        mPeriodYear.Add(Convert.ToDateTime(mTempDate).ToString("MM/yyyy"));
                    }
                }

                List<SalaryT> SalaryT = db.SalaryT.Where(e => e.EmployeePayroll_Id == Emp && mPeriodYear.Contains(e.PayMonth)).ToList();
                var SalaryT_Ids = SalaryT.Select(d => d.Id).ToList();
                List<SalEarnDedT> SalEarnDedT = db.SalEarnDedT.Include(e => e.SalaryHead).Where(e => SalaryT_Ids.Contains(e.SalaryT_Id.Value) && e.Amount > 0).ToList();
                var SalaryHead_Ids = SalEarnDedT.Select(e => e.SalaryHead_Id).Distinct().ToList();
                List<SalaryHead> SalaryHeadObjList = new List<SalaryHead>();
                var SalaryHeadList = db.SalaryHead
                    .Include(e => e.SalHeadOperationType)
                    .Include(e => e.Type)
                    .Include(e => e.Frequency)
                    .Include(e => e.ProcessType)
                    .Include(e => e.RoundingMethod)
                    .ToList();

                foreach (var item in SalaryHead_Ids)
                {
                    SalaryHead SalaryHead = SalaryHeadList.Where(e => e.Id == item).SingleOrDefault();
                    //SalaryHead SalaryHead = db.SalaryHead.Where(e => e.Id == item).SingleOrDefault();
                    //SalaryHead = SalaryHead;
                    //LookupValue SalHeadOperationType = db.LookupValue.Where(e => e.Id == SalaryHead.SalHeadOperationType_Id).SingleOrDefault();
                    //SalaryHead.SalHeadOperationType = SalHeadOperationType;
                    //LookupValue Type = db.LookupValue.Where(e => e.Id == SalaryHead.Type_Id).SingleOrDefault();
                    //SalaryHead.Type = Type;
                    //LookupValue Frequency = db.LookupValue.Where(e => e.Id == SalaryHead.Frequency_Id).SingleOrDefault();
                    //SalaryHead.Frequency = Frequency;
                    //LookupValue ProcessType = db.LookupValue.Where(e => e.Id == SalaryHead.ProcessType_Id).SingleOrDefault();
                    //SalaryHead.ProcessType = ProcessType;
                    //LookupValue RoundingMethod = db.LookupValue.Where(e => e.Id == SalaryHead.RoundingMethod_Id).SingleOrDefault();
                    //SalaryHead.RoundingMethod = RoundingMethod;
                    SalaryHeadObjList.Add(SalaryHead);
                }

                foreach (var SalaryTItem in SalaryT)
                {
                    var SalEarnDedTObj = SalEarnDedT.Where(e => e.SalaryT_Id == SalaryTItem.Id).ToList();
                    SalaryTItem.SalEarnDedT = SalEarnDedTObj;
                    foreach (var SalEarnDedTItem in SalEarnDedTObj)
                    {
                        var SalHeadObj = SalaryHeadObjList.Where(e => e.Id == SalEarnDedTItem.SalaryHead_Id).FirstOrDefault();
                        SalEarnDedTItem.SalaryHead = SalHeadObj;
                    }
                    List<PerkTransT> PerkTransT = db.SalaryT.Where(e => e.Id == SalaryTItem.Id).Select(e => e.PerkTransT.ToList()).SingleOrDefault();
                    SalaryTItem.PerkTransT = PerkTransT;
                    //db.SalaryT.Where(e => e.Id == J.Id).Select(r => r.PerkTransT.ToList()).FirstOrDefault();
                    foreach (var PerkTransTItem in PerkTransT)
                    {
                        // var PerkTransTObj = db.PerkTransT.Where(e => e.Id == k.Id).Select(r => r.SalaryHead).FirstOrDefault();
                        //var SalaryHead = db.SalaryHead.Where(e => e.Id == PerkTransTObj.Id).FirstOrDefault();
                        //k.SalaryHead = SalaryHead;
                        SalaryHead SalaryHead = db.SalaryHead.Where(e => e.Id == PerkTransTItem.SalaryHead_Id).SingleOrDefault();
                        PerkTransTItem.SalaryHead = SalaryHead;

                    }
                }
                //    SalaryHead SalaryHead = db.SalaryHead.Where(e => e.Id == SalEarnDedTItem.SalaryHead_Id).SingleOrDefault();
                //    SalEarnDedTItem.SalaryHead = SalaryHead;
                //    LookupValue SalHeadOperationType = db.LookupValue.Where(e => e.Id == SalaryHead.SalHeadOperationType_Id).SingleOrDefault();
                //    SalaryHead.SalHeadOperationType = SalHeadOperationType;
                //    LookupValue Type = db.LookupValue.Where(e => e.Id == SalaryHead.Type_Id).SingleOrDefault();
                //    SalaryHead.Type = Type;
                //    LookupValue Frequency = db.LookupValue.Where(e => e.Id == SalaryHead.Frequency_Id).SingleOrDefault();
                //    SalaryHead.Frequency = Frequency;
                //    LookupValue ProcessType = db.LookupValue.Where(e => e.Id == SalaryHead.ProcessType_Id).SingleOrDefault();
                //    SalaryHead.ProcessType = ProcessType;
                //    LookupValue RoundingMethod = db.LookupValue.Where(e => e.Id == SalaryHead.RoundingMethod_Id).SingleOrDefault();
                //    SalaryHead.RoundingMethod = RoundingMethod;

                //    //int SalEarnDedTObj = db.SalEarnDedT.AsNoTracking().Where(e => e.Id == i.Id).Select(r => r.SalaryHead.Id).FirstOrDefault();
                //    //var SalaryHead = db.SalaryHead.Where(e => e.Id == SalEarnDedTObj).FirstOrDefault();
                //    //var SalHeadOperationType = db.SalaryHead.Where(e => e.Id == SalEarnDedTObj).Select(r => r.SalHeadOperationType).FirstOrDefault();
                //    //var Type = db.SalaryHead.Where(e => e.Id == SalEarnDedTObj).Select(r => r.Type).FirstOrDefault();
                //    //var Frequency = db.SalaryHead.Where(e => e.Id == SalEarnDedTObj).Select(r => r.Frequency).FirstOrDefault();
                //    //var ProcessType = db.SalaryHead.Where(e => e.Id == SalEarnDedTObj).Select(r => r.ProcessType).FirstOrDefault();
                //    //var RoundingMethod = db.SalaryHead.Where(e => e.Id == SalEarnDedTObj).Select(r => r.RoundingMethod).FirstOrDefault();

                //    //i.SalaryHead = SalaryHead;
                //    //i.SalaryHead.SalHeadOperationType = SalHeadOperationType;
                //    //i.SalaryHead.Type = Type;
                //    //i.SalaryHead.Frequency = Frequency;
                //    //i.SalaryHead.ProcessType = ProcessType;
                //    //i.SalaryHead.RoundingMethod = RoundingMethod;
                //}

                //    SalaryTItem.SalEarnDedT = SalEarnDedT;
                //List<PerkTransT> PerkTransT = db.SalaryT.Where(e => e.Id == SalaryTItem.Id).Select(e => e.PerkTransT.ToList()).SingleOrDefault();
                //SalaryTItem.PerkTransT = PerkTransT;
                ////db.SalaryT.Where(e => e.Id == J.Id).Select(r => r.PerkTransT.ToList()).FirstOrDefault();
                //foreach (var PerkTransTItem in PerkTransT)
                //{
                //    // var PerkTransTObj = db.PerkTransT.Where(e => e.Id == k.Id).Select(r => r.SalaryHead).FirstOrDefault();
                //    //var SalaryHead = db.SalaryHead.Where(e => e.Id == PerkTransTObj.Id).FirstOrDefault();
                //    //k.SalaryHead = SalaryHead;
                //    SalaryHead SalaryHead = db.SalaryHead.Where(e => e.Id == PerkTransTItem.SalaryHead_Id).SingleOrDefault();
                //    PerkTransTItem.SalaryHead = SalaryHead;

                //}
                //J.SalEarnDedT = SalEarnDedT;
                //J.PerkTransT = PerkTransT;

                // }

                return SalaryT;
                // new code End 16/09/2022

                ////return db.EmployeePayroll.Where(e => e.Id == Emp)
                ////       .Include(e => e.SalaryT)
                ////       .Include(e => e.SalaryT.Select(r => r.PerkTransT))
                ////       .Include(e => e.SalaryT.Select(r => r.PerkTransT.Select(t => t.SalaryHead)))
                ////       .Include(e => e.SalaryT.Select(r => r.SalEarnDedT))
                ////       .Include(e => e.SalaryT.Select(r => r.SalEarnDedT.Select(w => w.SalaryHead)))
                ////       .Include(e => e.SalaryT.Select(r => r.SalEarnDedT.Select(w => w.SalaryHead.Frequency)))
                ////       .Include(e => e.SalaryT.Select(r => r.SalEarnDedT.Select(w => w.SalaryHead.Type)))
                ////       .Include(e => e.SalaryT.Select(r => r.SalEarnDedT.Select(w => w.SalaryHead.SalHeadOperationType)))
                ////       .Include(e => e.SalaryT.Select(r => r.SalEarnDedT.Select(w => w.SalaryHead.RoundingMethod)))
                ////       .Include(e => e.SalaryT.Select(r => r.SalEarnDedT.Select(w => w.SalaryHead.ProcessType)))
                ////       .Include(e => e.SalaryArrearT)
                ////       .Include(e => e.SalaryArrearT.Select(r => r.ArrearType))
                ////       .Include(e => e.SalaryArrearT.Select(r => r.SalaryArrearPaymentT))
                ////       .Include(e => e.SalaryArrearT.Select(r => r.SalaryArrearPaymentT.Select(t => t.SalaryHead)))
                ////       .Include(e => e.YearlyPaymentT)
                ////       .Include(e => e.YearlyPaymentT.Select(r => r.SalaryHead))
                ////       .Include(e => e.YearlyPaymentT.Select(r => r.SalaryHead.SalHeadOperationType))
                ////       .Include(e => e.YearlyPaymentT.Select(r => r.SalaryHead.Frequency))
                ////       .Include(e => e.YearlyPaymentT.Select(r => r.SalaryHead.Type))
                ////       .Include(e => e.YearlyPaymentT.Select(r => r.SalaryHead.RoundingMethod))
                ////       .Include(e => e.YearlyPaymentT.Select(r => r.SalaryHead.ProcessType))
                ////       .Include(e => e.PerkTransT)
                ////       .Include(e => e.PerkTransT.Select(r => r.SalaryHead))
                ////       .Include(e => e.PerkTransT.Select(r => r.SalaryHead.SalHeadOperationType))
                ////    //.Include(e => e.SalaryArrearT.Select(r => r.SalaryArrearPaymentT.Select(w => w.SalaryArrearPFT)))

                ////       .SingleOrDefault();
            }
        }
        public class ITSalaryHeadDataTemp
        {
            public double ActualAmount { get; set; }
            public int Id { get; set; }
            public string PayMonth { get; set; }
            public double ProjectedAmount { get; set; }
            public Int32 SalaryHead { get; set; }
        }
        public static void _Delete_ItSalaryHeadData()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                // var OITProjectionHeadDataDel = db.ITSalaryHeadData.ToList();
                db.ITSalaryHeadData.RemoveRange(db.ITSalaryHeadData.ToList());
                db.SaveChanges();
            }
        }
        public static List<ITSalaryHeadDataTemp> SalaryHeadMonthData(EmployeePayroll OEmployeePayroll, DateTime mProcessDate, DateTime mFromPeriod,
            DateTime mToPeriod, List<SalaryT> OSalaryTC, List<String> mPeriodYear, Int32 OFinancialYear)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //Utility.DumpProcessStatus("SalaryHeadMonthData");
                List<SalaryT> OSalaryTEmp = _returnEmployee_SalaryHeadMonthData(OEmployeePayroll.Id, mFromPeriod, mToPeriod);

                List<ITSalaryHeadDataTemp> OITSalIncome = new List<ITSalaryHeadDataTemp>(); //Salary income array
                ITSalaryHeadDataTemp OSalayIncomeObj = null;

                string mPeriodRange = "";
                List<string> mPeriod = new List<string>();
                DateTime mEndDate = Convert.ToDateTime("01/" + Convert.ToDateTime(mToPeriod).ToString("MM/yyyy")).AddMonths(1).Date;
                mEndDate = mEndDate.AddDays(-1).Date;
                for (DateTime mTempDate = mFromPeriod; mTempDate <= mEndDate; mTempDate = mTempDate.AddMonths(1))
                {
                    if (mPeriodRange == "")
                    {
                        mPeriodRange = Convert.ToDateTime(mTempDate).ToString("MM/yyyy");
                        mPeriod.Add(Convert.ToDateTime(mTempDate).ToString("MM/yyyy"));
                    }
                    else
                    {
                        mPeriodRange = mPeriodRange + "," + Convert.ToDateTime(mTempDate).ToString("MM/yyyy");
                        mPeriod.Add(Convert.ToDateTime(mTempDate).ToString("MM/yyyy"));
                    }
                }
                string OLastMonth = "";
                string OMonthChk = "";
                if (OSalaryTEmp != null && OSalaryTEmp.Count > 0)
                {
                    OMonthChk = OSalaryTEmp.Max(e => Convert.ToDateTime("01/" + e.PayMonth).Date).ToString("MM/yyyy");

                }
                else // salary not process or new join
                {
                    OMonthChk = Convert.ToDateTime(mFromPeriod.AddMonths(-1)).ToString("MM/yyyy");
                }

                if (OMonthChk == mProcessDate.ToString("MM/yyyy"))
                {
                    OLastMonth = mProcessDate.ToString("MM/yyyy");
                }
                else
                {

                    OLastMonth = OMonthChk;//mProcessDate.AddMonths(-1).ToString("MM/yyyy");
                }

                //OLastMonth = mProcessDate.ToString("MM/yyyy");

                if (mToPeriod < mProcessDate)
                {
                    SalaryT OMonthChk1 = OSalaryTEmp.Where(e => e.PayMonth == mToPeriod.ToString("MM/yyyy")).SingleOrDefault();
                    if (OMonthChk1 != null)
                    {
                        OLastMonth = mToPeriod.ToString("MM/yyyy");
                    }
                    else
                    {
                        OLastMonth = OMonthChk;// mToPeriod.AddMonths(-1).ToString("MM/yyyy");
                    }

                }

                double mBalMonths = (mToPeriod.Month + mToPeriod.Year * 12) - (Convert.ToDateTime("01/" + OLastMonth).Month + Convert.ToDateTime("01/" + OLastMonth).Year * 12);

                //List<string> mClosedMonths = new List<string>();
                //string mClosedPeriodRange = "";
                //for (DateTime mTempDate = mFromPeriod; mTempDate < Convert.ToDateTime("01/" + OLastMonth).AddMonths(1).Date; mTempDate = mTempDate.AddMonths(1))
                //{
                //    if (mClosedPeriodRange == "")
                //    {
                //        //mPeriodRange = Convert.ToDateTime(mTempDate).ToString("MM/yyyy") ;
                //        mClosedMonths.Add(Convert.ToDateTime(mTempDate).ToString("MM/yyyy"));
                //    }
                //    else
                //    {
                //        //mPeriodRange = mPeriodRange + "," +  Convert.ToDateTime(mTempDate).ToString("MM/yyyy");
                //        mClosedMonths.Add(Convert.ToDateTime(mTempDate).ToString("MM/yyyy"));
                //    }
                //}


                // DateTime start = new DateTime(2003, 12, 25);
                //DateTime end = new DateTime(2009, 10, 6);
                //int compMonth = (end.Month + end.Year * 12) - (start.Month + start.Year * 12);
                //double daysInEndMonth = (end - end.AddMonths(1)).Days;
                //double months = compMonth + (start.Day - end.Day) / daysInEndMonth;

                //**** Actual salary earnings/deductions ****************//
                List<SalaryT> OSalaryT = new List<SalaryT>();
                foreach (string item in mPeriod)
                {
                    if (OSalaryTEmp.Where(s => s.PayMonth == item).SingleOrDefault() != null)
                    {
                        OSalaryT.Add(OSalaryTEmp
                               .Where(s => s.PayMonth == item).SingleOrDefault());
                    }
                }
                //var OSalaryT = OSalaryTEmp
                //     .Where(s => mPeriod.Contains(s.PayMonth))
                //     .ToList();

                //.SelectMany(e=>e.SalEarnDedT).ToList();
                foreach (SalaryT ca in OSalaryT)
                {
                    var OSalaryHeadTotalActualEarning = ca.SalEarnDedT
                    .Where(q => q.SalaryHead.InITax == true && q.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() != "PERK")
                    .GroupBy(e => e.SalaryHead_Id) //{ e.SalaryHead })
                    .Select(i => new ITSalaryHeadDataTemp
                    {
                        PayMonth = ca.PayMonth,
                        SalaryHead = i.Key.Value,
                        ActualAmount = i.Sum(w => w.Amount),
                        ProjectedAmount = i.Sum(w => w.Amount),

                    }).ToList();


                    OITSalIncome.AddRange(OSalaryHeadTotalActualEarning);
                }



                //*************** Projected Salary income **************//
                //calculate salary head total from empsalstruct details headwise
                //EmpSalStruct OSalCurrentStruct = db.EmpSalStruct
                //    .AsNoTracking()
                //    .Where(e => e.EmployeePayroll.Id == OEmployeePayroll.Id && e.EndDate == null)// && e.EffectiveDate >= comparedate)
                //                             .Include(e => e.EmpSalStructDetails)
                //                             .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead))
                //                             .Include(e => e.EmpSalStructDetails.Select(q => q.SalHeadFormula))
                //                             .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.Frequency))
                //                             .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.Type))
                //                             .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.RoundingMethod))
                //                             .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.SalHeadOperationType))
                //                             .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.LvHead))
                //                             .SingleOrDefault();
                // New Patch Start 13/09/2022

                List<EmpSalStruct> EmpSalStructTotal = new List<EmpSalStruct>();
                //List<EmpSalStructDetails> EmpSalStructDetails = new List<EmpSalStructDetails>();
                //List<PayScaleAssignment> PayScaleAssignment = new List<PayScaleAssignment>();
                //var PayScaleAssignmentObj = new PayScaleAssignment();
                //List<SalaryHead> SalaryHead = new List<SalaryHead>();
                //var SalaryHeadObj = new SalaryHead();
                //List<SalHeadFormula> SalHeadFormula = new List<SalHeadFormula>();
                //var SalaryHeadFormulaObj = new SalHeadFormula();

                EmpSalStruct OSalCurrentStruct = db.EmpSalStruct.Where(e => e.EmployeePayroll_Id == OEmployeePayroll.Id && e.EndDate == null).SingleOrDefault();
                //foreach (var i in EmpSalStructTotal)
                //{
                if (OSalCurrentStruct != null)
                {


                    List<EmpSalStructDetails> EmpSalStructDetails = db.EmpSalStructDetails.
                       Where(e => e.EmpSalStruct_Id == OSalCurrentStruct.Id && e.SalaryHead.InITax == true && (e.SalaryHead.Frequency.LookupVal.ToUpper() != "DAILY" || e.SalaryHead.Frequency.LookupVal.ToUpper() != "HOURLY")).ToList();
                    var SalaryHeadList = db.SalaryHead
                        .Include(e => e.SalHeadOperationType)
                        .Include(e => e.Type)
                        .Include(e => e.Frequency)
                        .Include(e => e.ProcessType)
                        .Include(e => e.RoundingMethod)
                        .ToList();

                    foreach (var EmpSalStructDetailsItem in EmpSalStructDetails)
                    {
                        //var SalHeadTmp = new SalaryHead();
                        //PayScaleAssignment PayScaleAssignmentObj = db.PayScaleAssignment.Where(e => e.Id == EmpSalStructDetailsItem.PayScaleAssignment_Id).FirstOrDefault();
                        //EmpSalStructDetailsItem.PayScaleAssignment = PayScaleAssignmentObj;
                        //SalHeadFormula SalaryHeadFormulaObj = db.SalHeadFormula.Where(e => e.Id == EmpSalStructDetailsItem.SalHeadFormula_Id).FirstOrDefault();
                        //EmpSalStructDetailsItem.SalHeadFormula = SalaryHeadFormulaObj;
                        //var id = db.EmpSalStructDetails.Include(e => e.SalaryHead).Where(r => r.Id == j.Id).Select(r => r.SalaryHead.Id).FirstOrDefault();


                        //SalHeadTmp = db.SalaryHead.Where(e => e.Id == id).FirstOrDefault();

                        //SalHeadTmp.SalHeadOperationType = db.SalaryHead.Where(e => e.Id == id).Select(e => e.SalHeadOperationType).FirstOrDefault();
                        //SalHeadTmp.Frequency = db.SalaryHead.Where(e => e.Id == id).Select(e => e.Frequency).FirstOrDefault();
                        //SalHeadTmp.Type = db.SalaryHead.Where(e => e.Id == id).Select(e => e.Type).FirstOrDefault();
                        //SalHeadTmp.RoundingMethod = db.SalaryHead.Where(e => e.Id == id).Select(e => e.RoundingMethod).FirstOrDefault();
                        //SalHeadTmp.ProcessType = db.SalaryHead.Where(e => e.Id == id).Select(e => e.ProcessType).FirstOrDefault();
                        //SalHeadTmp.LvHead = db.SalaryHead.Where(e => e.Id == id).Select(e => e.LvHead.ToList()).FirstOrDefault();// to be check for output
                        //j.SalaryHead = SalHeadTmp;

                        SalaryHead SalaryHead = db.SalaryHead.Where(e => e.Id == EmpSalStructDetailsItem.SalaryHead_Id).SingleOrDefault();
                        EmpSalStructDetailsItem.SalaryHead = SalaryHead;

                        //LookupValue SalHeadOperationType = db.LookupValue.Where(e => e.Id == SalaryHead.SalHeadOperationType_Id).SingleOrDefault();
                        //SalaryHead.SalHeadOperationType = SalHeadOperationType;
                        //LookupValue Type = db.LookupValue.Where(e => e.Id == SalaryHead.Type_Id).SingleOrDefault();
                        //SalaryHead.Type = Type;
                        //LookupValue Frequency = db.LookupValue.Where(e => e.Id == SalaryHead.Frequency_Id).SingleOrDefault();
                        //SalaryHead.Frequency = Frequency;
                        //LookupValue ProcessType = db.LookupValue.Where(e => e.Id == SalaryHead.ProcessType_Id).SingleOrDefault();
                        //SalaryHead.ProcessType = ProcessType;
                        //LookupValue RoundingMethod = db.LookupValue.Where(e => e.Id == SalaryHead.RoundingMethod_Id).SingleOrDefault();
                        //SalaryHead.RoundingMethod = RoundingMethod;
                        //List<LvHead> LvHead = db.SalaryHead.Where(e => e.Id == EmpSalStructDetailsItem.SalaryHead_Id).Select(e => e.LvHead.ToList()).SingleOrDefault();// to be check for output
                        //SalaryHead.LvHead = LvHead;



                    }
                }
                // i.EmpSalStructDetails = EmpSalStructDetails;
                // }

                //EmpSalStruct OSalCurrentStruct = EmpSalStructTotal.Where(e => e.EndDate == null).SingleOrDefault(); //single salary structure


                // New Patch End 13/09/2022

                //var EmpSalStructTotal = QEmpSalStruct1.Where(e => e.EmployeePayroll.Id == OEmployeePayroll.Id).ToList();

                //EmpSalStruct OSalCurrentStruct = OEmployeePayroll.EmpSalStruct
                //    .Where(s => s.EndDate == null).SingleOrDefault();

                // EmpSalStruct OSalCurrentStruct = EmpSalStructTotal.Where(s => s.EndDate == null).SingleOrDefault();

                if (OSalCurrentStruct != null) //no sal structure in employee left condition 
                {
                    List<EmpSalStructDetails> OSalaryHeadTotalProjected = OSalCurrentStruct.EmpSalStructDetails
                    .Where(v => v.SalaryHead.InITax == true
                    && v.SalaryHead.Frequency.LookupVal.ToUpper() == "MONTHLY")
                    .ToList();
                    foreach (EmpSalStructDetails ca in OSalaryHeadTotalProjected)
                    {
                        for (DateTime mDate = Convert.ToDateTime("01/" + OLastMonth).AddMonths(1); mDate <= mToPeriod; mDate = mDate.AddMonths(1))
                        {
                            string Opaymonth = mDate.ToString("MM/yyyy");
                            SalaryT OSalT = OSalaryTEmp.Where(e => e.PayMonth == Opaymonth).SingleOrDefault();
                            ITSalaryHeadDataTemp OSalaryHeadTotalProjEarning = new ITSalaryHeadDataTemp() { };
                            if (ca.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() == "EPF")
                            {
                                int Comp_Id = Convert.ToInt32(HttpContext.Current.Session["CompId"].ToString());
                                CompanyPayroll OCompanyPayroll = db.CompanyPayroll.Include(e => e.PFMaster)
                                    .AsNoTracking()
                                    .Where(e => e.Company.Id == Comp_Id).SingleOrDefault();

                                List<SalEarnDedT> OSalEarnDedT = OSalT == null ? null : OSalT.SalEarnDedT.ToList();
                                double Amt = 0;
                                if (OEmployeePayroll.Employee.EmpOffInfo.PFAppl == true)
                                {

                                    PFMaster OCompPFMaster = null;
                                    //PFMaster OCompPFMaster = OCompanyPayroll.PFMaster.Where(e =>e.EstablishmentID==OEmployeePayroll.Employee.EmpOffInfo.PFTrust_EstablishmentId && e.EndDate == null || e.EndDate.Value.Date >
                                    //    Convert.ToDateTime("01/" + mDate.ToString("MM/yyyy")).Date).FirstOrDefault();
                                    foreach (var itemPFmas in OCompanyPayroll.PFMaster.ToList())
                                    {
                                        if (itemPFmas.EstablishmentID == OEmployeePayroll.Employee.EmpOffInfo.PFTrust_EstablishmentId && (itemPFmas.EndDate == null || itemPFmas.EndDate.Value.Date > Convert.ToDateTime("01/" + mDate.ToString("MM/yyyy")).Date))
                                        {
                                            OCompPFMaster = itemPFmas;
                                        }
                                    }

                                    Amt = PFcalc(OCompPFMaster.Id, OSalEarnDedT, OSalaryHeadTotalProjected); //OSalEarnDedT.Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() == "EPF").SingleOrDefault() != null ? OSalEarnDedT.Where(e => e.SalaryHead.Id == ca.SalaryHead.Id).SingleOrDefault().Amount : 0;
                                }
                                if (OEmployeePayroll.Employee != null && OEmployeePayroll.Employee.EmpOffInfo != null && OEmployeePayroll.Employee.EmpOffInfo.PFAppl == false)
                                {
                                    Amt = 0;
                                }
                                OSalaryHeadTotalProjEarning = new ITSalaryHeadDataTemp()
                                {
                                    PayMonth = mDate.ToString("MM/yyyy"),
                                    SalaryHead = ca.SalaryHead.Id,
                                    ActualAmount = 0,
                                    ProjectedAmount = Amt,

                                };
                            }
                            else if (ca.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() == "PTAX")
                            {
                                int Comp_Id = Convert.ToInt32(HttpContext.Current.Session["CompId"].ToString());
                                CompanyPayroll OCompanyPayroll = db.CompanyPayroll.Include(e => e.PTaxMaster).Include(e => e.PTaxMaster.Select(a => a.States))
                                    .AsNoTracking()
                                    .Where(e => e.Company.Id == Comp_Id).SingleOrDefault();

                                List<SalEarnDedT> OSalEarnDedT = OSalT == null ? null : OSalT.SalEarnDedT.ToList();
                                List<PTaxMaster> OCompPTMaster = OCompanyPayroll.PTaxMaster.ToList();
                                double Amt = PTcalc(OCompPTMaster, OSalCurrentStruct.Id, OEmployeePayroll.Id,
                                    mDate.ToString("MM/yyyy"), OSalEarnDedT, OSalaryHeadTotalProjected, OFinancialYear, mFromPeriod, mToPeriod);
                                OSalaryHeadTotalProjEarning = new ITSalaryHeadDataTemp()
                                {
                                    PayMonth = mDate.ToString("MM/yyyy"),
                                    // SalaryHead = db.SalaryHead.Include(e => e.SalHeadOperationType).Where(r => r.Id == ca.SalaryHead.Id).FirstOrDefault().Id,
                                    SalaryHead = ca.SalaryHead.Id,
                                    ActualAmount = 0,
                                    ProjectedAmount = Amt,

                                };
                            }
                            else if (ca.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() == "INSURANCE")
                            {
                                double Amt = LICcalc(OSalCurrentStruct, OEmployeePayroll.Id, mDate.ToString("MM/yyyy"));
                                OSalaryHeadTotalProjEarning = new ITSalaryHeadDataTemp()
                                {
                                    PayMonth = mDate.ToString("MM/yyyy"),
                                    // SalaryHead = db.SalaryHead.Include(e => e.SalHeadOperationType).Where(r => r.Id == ca.SalaryHead.Id).FirstOrDefault().Id,
                                    SalaryHead = ca.SalaryHead.Id,
                                    ActualAmount = 0,
                                    ProjectedAmount = Amt,

                                };

                            }
                            else if (ca.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() == "PERK" || ca.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() == "OFFICIATING")
                            {
                                OSalaryHeadTotalProjEarning = new ITSalaryHeadDataTemp()
                                {
                                    PayMonth = mDate.ToString("MM/yyyy"),
                                    SalaryHead = ca.SalaryHead.Id,
                                    ActualAmount = 0,
                                    ProjectedAmount = 0,

                                };
                            }
                            else
                            {
                                OSalaryHeadTotalProjEarning = new ITSalaryHeadDataTemp()
                                {
                                    PayMonth = mDate.ToString("MM/yyyy"),
                                    SalaryHead = ca.SalaryHead.Id,
                                    ActualAmount = 0,
                                    ProjectedAmount = ca.Amount,

                                };
                            }

                            OITSalIncome.Add(OSalaryHeadTotalProjEarning);
                        }
                    }
                }



                //calculate salary head total from salaryt details headwise
                List<SalaryArrearT> OSalaryArrearT = new List<SalaryArrearT>();
                //EmployeePayroll OArrearData = db.EmployeePayroll
                //    .AsNoTracking()
                //    .Where(e => e.Id == OEmployeePayroll.Id)
                //   .Include(e => e.SalaryArrearT)
                //   .Include(e => e.SalaryArrearT.Select(r => r.ArrearType))
                //   .Include(e => e.SalaryArrearT.Select(r => r.SalaryArrearPaymentT))
                //   .Include(e => e.SalaryArrearT.Select(r => r.SalaryArrearPaymentT.Select(t => t.SalaryHead))).AsNoTracking().SingleOrDefault();

                // new code start 16/09/2022
                var OArrearData = new EmployeePayroll();
                OArrearData = db.EmployeePayroll.Where(e => e.Id == OEmployeePayroll.Id).FirstOrDefault();

                List<SalaryArrearT> SalaryArrearT = new List<SalaryArrearT>();
                SalaryArrearT = db.SalaryArrearT.Where(e => e.EmployeePayroll.Id == OEmployeePayroll.Id && mPeriodYear.Contains(e.PayMonth)).ToList();
                foreach (var j in SalaryArrearT)
                {
                    var ArrearTypeobj = db.SalaryArrearT.Where(e => e.Id == j.Id).Select(x => x.ArrearType).FirstOrDefault();

                    List<SalaryArrearPaymentT> SalaryArrearPaymentT = db.SalaryArrearT.Where(e => e.Id == j.Id).Select(r => r.SalaryArrearPaymentT.ToList()).FirstOrDefault();
                    foreach (var i in SalaryArrearPaymentT)
                    {
                        var SalaryArrearPaymentTObj = db.SalaryArrearPaymentT.Where(e => e.Id == i.Id).Select(r => r.SalaryHead).FirstOrDefault();
                        var SalaryHeadarrrear = db.SalaryHead.Where(e => e.Id == SalaryArrearPaymentTObj.Id).FirstOrDefault();

                        i.SalaryHead = SalaryHeadarrrear;

                    }
                    j.SalaryArrearPaymentT = SalaryArrearPaymentT;
                    j.ArrearType = ArrearTypeobj;

                }

                OArrearData.SalaryArrearT = SalaryArrearT;



                // new code end 16/09/2022

                foreach (string item in mPeriod)
                {
                    List<SalaryArrearT> aa = OArrearData.SalaryArrearT
                .Where(s => s.PayMonth == item && s.IsRecovery == false)
                .ToList();
                    if (aa != null)
                    {
                        OSalaryArrearT.AddRange(aa);
                    }
                }
                //var OSalaryArrearT = OSalaryTEmp.SalaryArrearT
                //    .Where(s => mPeriod.Contains(s.PayMonth) && s.IsRecovery == false)
                //    .ToList();
                foreach (SalaryArrearT ca in OSalaryArrearT)
                {
                    var OSalArrearPayment = ca.SalaryArrearPaymentT
                    .Where(q => q.SalaryHead.InITax == true)
                    .GroupBy(e => new { e.SalaryHead })

                    .Select(i => new ITSalaryHeadDataTemp
                    {
                        PayMonth = ca.PayMonth,
                        SalaryHead = i.Key.SalaryHead.Id,
                        ActualAmount = i.Sum(w => w.SalHeadAmount),
                        ProjectedAmount = i.Sum(w => w.SalHeadAmount),

                    }).ToList();
                    OITSalIncome.AddRange(OSalArrearPayment);
                }

                //arrears deductions
                List<SalaryArrearT> OSalaryArrearTDeduction = new List<SalaryArrearT>();
                foreach (string item in mPeriod)
                {
                    List<SalaryArrearT> OSalaryArrearTDeduction_t = OArrearData.SalaryArrearT
                  .Where(s => s.PayMonth == item && s.IsRecovery == true)
                  .ToList();
                    if (OSalaryArrearTDeduction_t != null)
                    {
                        OSalaryArrearTDeduction.AddRange(OSalaryArrearTDeduction_t);
                    }
                }

                foreach (SalaryArrearT ca in OSalaryArrearTDeduction)
                {
                    var OSalArrearPayment = ca.SalaryArrearPaymentT
                    .Where(q => q.SalaryHead.InITax == true)
                    .GroupBy(e => new { e.SalaryHead })

                    .Select(i => new ITSalaryHeadDataTemp
                    {
                        PayMonth = ca.PayMonth,
                        SalaryHead = i.Key.SalaryHead.Id,
                        ActualAmount = i.Sum(w => w.SalHeadAmount),
                        ProjectedAmount = i.Sum(w => w.SalHeadAmount),

                    }).ToList();
                    OITSalIncome.AddRange(OSalArrearPayment);
                }

                // Officiating payment Start
                //Nkgsb Bank Not Require Officiating amount bifuercation add in reqular salary
                List<BMSPaymentReq> OBMSPaymentReq = new List<BMSPaymentReq>();
                List<BMSPaymentReq> BMSPaymentReq = new List<BMSPaymentReq>();
                BMSPaymentReq = db.BMSPaymentReq.Where(e => e.EmployeePayroll.Id == OEmployeePayroll.Id && mPeriodYear.Contains(e.PayMonth) && e.IsCancel == false && e.TrClosed == true && e.TrReject == false).ToList();
                foreach (var j in BMSPaymentReq)
                {


                    List<OfficiatingPaymentT> OfficiatingPaymentT = db.BMSPaymentReq.Where(e => e.Id == j.Id).Select(r => r.OfficiatingPaymentT.ToList()).FirstOrDefault();
                    foreach (var i in OfficiatingPaymentT)
                    {
                        var OfficiatingPaymentTObj = db.OfficiatingPaymentT.Where(e => e.Id == i.Id).Select(r => r.SalaryHead).FirstOrDefault();
                        var SalaryHeadarrrear = db.SalaryHead.Include(e=>e.SalHeadOperationType).Where(e => e.Id == OfficiatingPaymentTObj.Id).FirstOrDefault();

                        i.SalaryHead = SalaryHeadarrrear;

                    }
                    j.OfficiatingPaymentT = OfficiatingPaymentT;


                }
                foreach (string item in mPeriod)
                {
                    List<BMSPaymentReq> aa = BMSPaymentReq
                .Where(s => s.PayMonth == item && s.IsCancel == false && s.TrClosed == true && s.TrReject == false)
                .ToList();
                    if (aa != null)
                    {
                        OBMSPaymentReq.AddRange(aa);
                    }
                }

                foreach (BMSPaymentReq ca in OBMSPaymentReq)
                {
                    var OfficiatingPaymentT = ca.OfficiatingPaymentT
                    .Where(q => q.SalaryHead.InITax == true && q.SalaryHead.SalHeadOperationType.LookupVal.ToUpper()=="EPF")
                    .GroupBy(e => new { e.SalaryHead })

                    .Select(i => new ITSalaryHeadDataTemp
                    {
                        PayMonth = ca.PayMonth,
                        SalaryHead = i.Key.SalaryHead.Id,
                        ActualAmount = i.Sum(w => w.SalHeadAmount),
                        ProjectedAmount = i.Sum(w => w.SalHeadAmount),

                    }).ToList();
                    OITSalIncome.AddRange(OfficiatingPaymentT);
                }



                // Officiating payment End

                //yearlypayment
                List<YearlyPaymentT> OSalaryYearlyPaymenttDeduction = new List<YearlyPaymentT>();
                //EmployeePayroll OYearlyPaymentT = db.EmployeePayroll
                //    .AsNoTracking()
                //    .Where(e => e.Id == OEmployeePayroll.Id)
                //      .Include(e => e.YearlyPaymentT)
                //      .Include(e => e.YearlyPaymentT.Select(r => r.SalaryHead))
                //      .Include(e => e.YearlyPaymentT.Select(r => r.SalaryHead.SalHeadOperationType))
                //      .Include(e => e.YearlyPaymentT.Select(r => r.SalaryHead.Frequency))
                //      .Include(e => e.YearlyPaymentT.Select(r => r.SalaryHead.Type))
                //      .Include(e => e.YearlyPaymentT.Select(r => r.SalaryHead.RoundingMethod))
                //      .Include(e => e.YearlyPaymentT.Select(r => r.SalaryHead.ProcessType)).AsNoTracking().SingleOrDefault();
                // new code start 16/09/2022

                var OYearlyPaymentT = new EmployeePayroll();
                OYearlyPaymentT = db.EmployeePayroll.Where(e => e.Id == OEmployeePayroll.Id).FirstOrDefault();

                // YearlyPaymentT = new List<YearlyPaymentT>();

                List<YearlyPaymentT> YearlyPaymentT = db.YearlyPaymentT.Where(e => e.EmployeePayroll_Id == OEmployeePayroll.Id && mPeriodYear.Contains(e.PayMonth)).ToList();

                foreach (var YearlyPaymentTItem in YearlyPaymentT)
                {
                    //var SalHeadTmp = new SalaryHead();

                    //var id = db.YearlyPaymentT.Include(e => e.SalaryHead).Where(r => r.Id == j.Id).Select(r => r.SalaryHead.Id).FirstOrDefault();


                    //SalHeadTmp = db.SalaryHead.Where(e => e.Id == id).FirstOrDefault();

                    //SalHeadTmp.SalHeadOperationType = db.SalaryHead.Where(e => e.Id == id).Select(e => e.SalHeadOperationType).FirstOrDefault();
                    //SalHeadTmp.Frequency = db.SalaryHead.Where(e => e.Id == id).Select(e => e.Frequency).FirstOrDefault();
                    //SalHeadTmp.Type = db.SalaryHead.Where(e => e.Id == id).Select(e => e.Type).FirstOrDefault();
                    //SalHeadTmp.RoundingMethod = db.SalaryHead.Where(e => e.Id == id).Select(e => e.RoundingMethod).FirstOrDefault();
                    //SalHeadTmp.ProcessType = db.SalaryHead.Where(e => e.Id == id).Select(e => e.ProcessType).FirstOrDefault();
                    ////SalaryHead.Add(SalHeadTmp);
                    ////SalHeadFormula.Add(j.SalHeadFormula)  ;
                    //j.SalaryHead = SalHeadTmp;

                    SalaryHead SalaryHead = db.SalaryHead.Where(e => e.Id == YearlyPaymentTItem.SalaryHead_Id).SingleOrDefault();
                    YearlyPaymentTItem.SalaryHead = SalaryHead;
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
                OYearlyPaymentT.YearlyPaymentT = YearlyPaymentT;
                //OYearlyPaymentT.YearlyPaymentT = YearlyPaymentT;


                // new code end 16/09/2022

                foreach (string item in mPeriodYear)
                {

                    List<YearlyPaymentT> OSalaryYearlyPaymenttDeduction_l = OYearlyPaymentT.YearlyPaymentT
                           .Where(s => s.PayMonth == item)
                           .ToList();
                    if (OSalaryYearlyPaymenttDeduction_l != null)
                    {
                        OSalaryYearlyPaymenttDeduction.AddRange(OSalaryYearlyPaymenttDeduction_l);
                    }
                }
                foreach (YearlyPaymentT ca in OSalaryYearlyPaymenttDeduction)
                {
                    // Surendra start
                    if (ca.ReleaseFlag == true && ca.SalaryHead.InPayslip == false)
                    {

                        // Surendra end
                        int a = OITSalIncome.Where(r => r.SalaryHead == ca.SalaryHead.Id && r.PayMonth == ca.PayMonth).Distinct().Count();
                        if (a <= 0)
                        {
                            var OSalYearPayment = OSalaryYearlyPaymenttDeduction
                                               .Where(q => q.SalaryHead.InITax == true && q.PayMonth == ca.PayMonth && q.SalaryHead.Id == ca.SalaryHead.Id)
                                               .GroupBy(e => new { e.SalaryHead })

                                               .Select(i => new ITSalaryHeadDataTemp
                                               {
                                                   PayMonth = ca.PayMonth,
                                                   SalaryHead = i.Key.SalaryHead.Id,
                                                   ActualAmount = i.Sum(w => w.AmountPaid),
                                                   ProjectedAmount = i.Sum(w => w.AmountPaid),

                                               }).ToList();
                            OITSalIncome.AddRange(OSalYearPayment);

                            //OSalYearPayment = OSalaryYearlyPaymenttDeduction
                            //                   .Where(q => q.SalaryHead.InITax == true && q.PayMonth == ca.PayMonth)
                            //                   .GroupBy(e => new { e.SalaryHead })

                            //                   .Select(i => new ITSalaryHeadDataTemp
                            //                   {
                            //                       PayMonth = ca.PayMonth,
                            //                       SalaryHead = i.Key.SalaryHead.Id,
                            //                       ActualAmount = i.Sum(w => w.TDSAmount),
                            //                       ProjectedAmount = i.Sum(w => w.TDSAmount),

                            //                   }).ToList();

                            //OITSalIncome.AddRange(OSalYearPayment);
                        }
                    }
                    // Surendra start
                    else
                    {
                        Int32 a = OITSalIncome.Where(r => r.SalaryHead == ca.SalaryHead.Id && r.PayMonth == ca.PayMonth).Distinct().Count();
                        if (a <= 0)
                        {
                            var OSalYearPayment = OSalaryYearlyPaymenttDeduction
                                               .Where(q => q.SalaryHead.InITax == true && q.PayMonth == ca.PayMonth && q.SalaryHead.Id == ca.SalaryHead.Id)
                                               .GroupBy(e => new { e.SalaryHead })

                                               .Select(i => new ITSalaryHeadDataTemp
                                               {
                                                   PayMonth = ca.PayMonth,
                                                   SalaryHead = i.Key.SalaryHead.Id,
                                                   ActualAmount = 0,
                                                   ProjectedAmount = i.Sum(w => w.AmountPaid),

                                               }).ToList();
                            OITSalIncome.AddRange(OSalYearPayment);

                            //OSalYearPayment = OSalaryYearlyPaymenttDeduction
                            //                   .Where(q => q.SalaryHead.InITax == true && q.PayMonth == ca.PayMonth)
                            //                   .GroupBy(e => new { e.SalaryHead })

                            //                   .Select(i => new ITSalaryHeadDataTemp
                            //                   {
                            //                       PayMonth = ca.PayMonth,
                            //                       SalaryHead = i.Key.SalaryHead.Id,
                            //                       ActualAmount = 0,
                            //                       ProjectedAmount = i.Sum(w => w.TDSAmount),

                            //                   }).ToList();

                            //OITSalIncome.AddRange(OSalYearPayment);
                        }

                    }
                    // Surendra end
                }

                //**************** Perk salary ******************//

                //calculate salary head total from PerkTransT details headwise
                //**** Actual salary earnings/deductions ****************//
                List<SalaryT> OPerkT = new List<SalaryT>();
                EmployeePayroll OPerkdata = db.EmployeePayroll
                     .AsNoTracking().Where(e => e.Id == OEmployeePayroll.Id)
                       .Include(e => e.PerkTransT)
                       .Include(e => e.PerkTransT.Select(r => r.SalaryHead))
                       .Include(e => e.PerkTransT.Select(r => r.SalaryHead.SalHeadOperationType))
                   .SingleOrDefault();
                foreach (String item in mPeriod)
                {
                    SalaryT OPerkT_l = OSalaryTEmp
                         .Where(s => s.PayMonth == item)
                         .SingleOrDefault();
                    if (OPerkT_l != null)
                    {
                        OPerkT.Add(OPerkT_l);
                    }
                }
                List<SalaryT> OPerkT1 = OPerkT.Where(e => e.PerkTransT != null).ToList();
                if (OPerkT1.Count() > 0)
                {
                    foreach (SalaryT ca in OPerkT)
                    {
                        var OSalaryHeadTotalActualEarning = ca.PerkTransT
                        .Where(q => q.SalaryHead.InITax == true)
                        .GroupBy(e => new { e.SalaryHead })

                        .Select(i => new ITSalaryHeadDataTemp
                        {
                            PayMonth = ca.PayMonth,
                            SalaryHead = i.Key.SalaryHead.Id,
                            ActualAmount = i.Sum(w => w.ActualAmount),
                            ProjectedAmount = i.Sum(w => w.ProjectedAmount),

                        }).ToList();

                        OITSalIncome.AddRange(OSalaryHeadTotalActualEarning);
                    }
                }
                List<PerkTransT> OPerkTransT = new List<PerkTransT>();
                foreach (string item in mPeriod)
                {
                    List<PerkTransT> OPerkTransT_l = OPerkdata.PerkTransT
                               .Where(s => s.PayMonth == item)
                               .ToList();
                    if (OPerkTransT_l != null)
                    {
                        OPerkTransT.AddRange(OPerkTransT_l);
                    }
                }
                var CompId = Convert.ToInt32(SessionManager.CompanyId);

                Company OCompany = null;
                OCompany = db.Company.Find(CompId);

                string FileCompCode = "";
                string requiredPathpdcc = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\App_Data\\Menu_Json";
                bool existspdcc = System.IO.Directory.Exists(requiredPathpdcc);
                string localPathpdcc;
                if (!existspdcc)
                {
                    localPathpdcc = new Uri(requiredPathpdcc).LocalPath;
                    System.IO.Directory.CreateDirectory(localPathpdcc);
                }
                string pathpdcc = requiredPathpdcc + @"\LoanInterfacePerkCalc" + ".ini";
                localPathpdcc = new Uri(pathpdcc).LocalPath;
                using (var streamReader = new StreamReader(localPathpdcc))
                {
                    string line;

                    while ((line = streamReader.ReadLine()) != null)
                    {
                        var comp = line;
                        FileCompCode = comp;
                    }
                }

                if (OCompany.Code == FileCompCode)
                {
                    var OSalYearPayment = OPerkTransT
                                            .Where(q => q.SalaryHead.InITax == true)
                                            .GroupBy(e => new { e.SalaryHead_Id }).Select(i => new ITSalaryHeadDataTemp
                                            {
                                                PayMonth = i.LastOrDefault().PayMonth,
                                                SalaryHead = (int)i.Key.SalaryHead_Id,
                                                ActualAmount = i.Sum(w => w.ActualAmount),
                                                ProjectedAmount = i.LastOrDefault().ProjectedAmount,

                                            }).ToList();

                    OITSalIncome.AddRange(OSalYearPayment);

                    //foreach (var ca in OPerkTransTGr)
                    //{
                    //    int a = OITSalIncome.Where(r => r.SalaryHead == ca.SalaryHead.Id).Distinct().Count();
                    //    if (a <= 0)
                    //    {
                    //        var OSalYearPayment = OPerkTransTGr 
                    //                           .Select(i => new ITSalaryHeadDataTemp
                    //                           {
                    //                               PayMonth = ca.PayMonth,
                    //                               SalaryHead = i.Key.SalaryHead.Id,
                    //                               ActualAmount = i.Sum(w => w.ActualAmount),
                    //                               ProjectedAmount = i.LastOrDefault().ProjectedAmount,

                    //                           }).ToList();

                    //        OITSalIncome.AddRange(OSalYearPayment);
                    //    }
                    //}
                }
                else
                {
                    foreach (PerkTransT ca in OPerkTransT)
                    {
                        int a = OITSalIncome.Where(r => r.SalaryHead == ca.SalaryHead.Id && r.PayMonth == ca.PayMonth).Distinct().Count();
                        if (a <= 0)
                        {
                            var OSalYearPayment = OPerkTransT
                                               .Where(q => q.SalaryHead.InITax == true && q.PayMonth == ca.PayMonth)
                                               .GroupBy(e => new { e.SalaryHead })

                                               .Select(i => new ITSalaryHeadDataTemp
                                               {
                                                   PayMonth = ca.PayMonth,
                                                   SalaryHead = i.Key.SalaryHead.Id,
                                                   ActualAmount = i.Sum(w => w.ActualAmount),
                                                   ProjectedAmount = i.Sum(w => w.ProjectedAmount),

                                               }).ToList();
                            OITSalIncome.AddRange(OSalYearPayment);
                        }
                    }
                }



                if (OSalCurrentStruct != null)
                {
                    EmpSalStruct OPerkCurrentStruct = OSalCurrentStruct;

                    var OSalaryHeadTotalProjectedEarningPerk = OPerkCurrentStruct.EmpSalStructDetails
                        .Where(v => v.SalaryHead.InITax == true && v.SalaryHead.Type.LookupVal.ToUpper() == "EARNING"
                            && v.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "PERK"
                            && v.SalaryHead.Frequency.LookupVal.ToUpper() == "MONTHLY")
                        .GroupBy(e => e.SalaryHead.Id)
                        .Select((i => new ITSalaryHeadDataTemp
                        {
                            ProjectedAmount = i.Select(r => r.Amount).SingleOrDefault() * Convert.ToDouble(mBalMonths),
                            SalaryHead = i.Select(e => e.SalaryHead.Id).SingleOrDefault(),
                            PayMonth = i.Select(e => e.EmpSalStruct.EffectiveDate.Value.ToString("MM/yyyy")).SingleOrDefault(),
                        }
                            )).ToList();

                    foreach (ITSalaryHeadDataTemp ca in OSalaryHeadTotalProjectedEarningPerk)
                    {
                        OSalayIncomeObj = OITSalIncome.Where(e => e.SalaryHead == ca.SalaryHead && e.PayMonth == ca.PayMonth).SingleOrDefault();
                        if (OSalayIncomeObj != null)
                        {
                            OSalayIncomeObj.ProjectedAmount = OSalayIncomeObj.ActualAmount + ca.ProjectedAmount;
                        }
                        else
                        {
                            ITSalaryHeadDataTemp OITSalaryHeadDataNew = new ITSalaryHeadDataTemp()
                            {
                                ProjectedAmount = ca.ProjectedAmount,
                                SalaryHead = ca.SalaryHead,
                                ActualAmount = 0

                            };
                            OITSalIncome.Add(OITSalaryHeadDataNew);
                        }

                    }
                }

                List<ITSalaryHeadDataTemp> OITSalMonthWise = new List<ITSalaryHeadDataTemp>();
                var OSalTotalT = OITSalIncome.GroupBy(e => new { PayMonth = e.PayMonth, SalHead = e.SalaryHead })
                    .Select(r => new ITSalaryHeadDataTemp
                    {
                        PayMonth = r.Key.PayMonth,
                        SalaryHead = r.Key.SalHead,
                        ActualAmount = r.Sum(d => d.ActualAmount),
                        ProjectedAmount = r.Sum(d => d.ProjectedAmount)
                    }
                    ).ToList();
                OITSalMonthWise.AddRange(OSalTotalT);

                return OITSalMonthWise;
                //}
            }
        }
        public static double PFcalc(Int32? OPFMaster_id, List<SalEarnDedT> OSalaryDetails, List<EmpSalStructDetails> oEmpSalStructDetails)
        {
            using (DataBaseContext db = new DataBaseContext())
            {


                //PFMaster mPFMaster1 = OCompanyPFMaster;

                var OPFMaster = db.PFMaster.Where(e => e.Id == OPFMaster_id)
                .Include(e => e.EPFWages.RateMaster.Select(r => r.SalHead)).SingleOrDefault();
                //.Include(e => e.EPFWages)
                // .Select(e => new { EPFWages = e.EPFWages, EmpPFPerc = e.EmpPFPerc }).SingleOrDefault();
                //.Include(e => e.PFAdminWages)
                //.Include(e => e.PFEDLIWages)
                //.Include(e => e.PFInspWages)
                //.Include(e => e.EPFWages)
                //.Include(e => e.PFAdminWages.RateMaster)
                //.Include(e => e.PFEDLIWages.RateMaster)
                //.Include(e => e.PFInspWages.RateMaster)
                //.Include(e => e.EPFWages.RateMaster)
                //.Include(e => e.EPFWages.RateMaster.Select(r => r.SalHead))
                //.Include(e => e.PFTrustType)
                //get PF Master from companypayroll by passing companyID from session


                double mEmpPF = 0;
                //EmpSalStructDetails OSalaryHead = OEmpSalstruct.EmpSalStructDetails.Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "EPF")
                //   .SingleOrDefault();

                //List<SalAttendanceT> OLWPAttend1 = db.EmployeePayroll.Where(d => d.Id == OEmployeePayroll.Id)
                //     .SelectMany(e => e.SalAttendance.Where(r => r.PayMonth == PayMonth))
                //     .ToList();
                //SalAttendanceT OLWPAttend = OLWPAttend1.FirstOrDefault();
                double mPFWages = Process.SalaryHeadGenProcess.WagecalcDirect(OPFMaster.EPFWages, OSalaryDetails, oEmpSalStructDetails);
                mPFWages = Math.Round(mPFWages, 0);

                var EmpPFHead = oEmpSalStructDetails
                                    .Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "EPF").SingleOrDefault();
                if (EmpPFHead != null && EmpPFHead.SalHeadFormula_Id != null)
                {

                    int? Pfformulaid = EmpPFHead.SalHeadFormula_Id;

                    var OSalHeadFormualQuery = SalaryHeadGenProcess._returnSalHeadFormula(Pfformulaid);
                    mPFWages = SalaryHeadGenProcess.Wagecalc(OSalHeadFormualQuery, null, oEmpSalStructDetails);
                    mPFWages = SalaryHeadGenProcess.CellingCkeck(OSalHeadFormualQuery, mPFWages);
                }


                mEmpPF = Math.Round(mPFWages * OPFMaster.EmpPFPerc / 100, 0);
                mEmpPF = Math.Round(mEmpPF, 0);
                //if (OPFMaster == null)
                //{
                //    //pf master not exist

                //}
                //else
                //{
                //    double mAge = 0;
                //    DateTime? mDateofBirth = OEmployeePayroll.Employee.ServiceBookDates.BirthDate;// db.Employee.Where(e => e.Id == mEmployeeID).Select(e => e.ServiceBookDates.BirthDate);
                //    DateTime start = mDateofBirth.Value;
                //    DateTime end = Convert.ToDateTime("01/" + PayMonth).AddMonths(1).AddDays(-1).Date;// DateTime.Now.Date;
                //    int compMonth = (end.Month + end.Year * 12) - (start.Month + start.Year * 12);
                //    double daysInEndMonth = (end - end.AddMonths(1)).Days;
                //    double months = compMonth + (start.Day - end.Day) / daysInEndMonth;
                //    mAge = months / 12;



                //    //double mGrossWages = OSalaryDetails.Where(r => r.SalaryHead.Type.LookupVal.ToUpper() == "EARNING" && r.SalaryHead.InPayslip == true)
                //    //    .Sum(e => e.Amount);
                //    //mGrossWages = Math.Round(mGrossWages, 0);


                //}
                return mEmpPF;
            }
        }
        public static double PTcalc(List<PTaxMaster> OCompanyPTaxMaster, Int32 OEmpSalstruct, Int32 OEmployeePayroll,
            string PayMonth, List<SalEarnDedT> OSalaryDetails, List<EmpSalStructDetails> oEmpSalStructDetails, int FinancialYear_Id, DateTime FromPeriod, DateTime ToPeriod)
        {

            using (DataBaseContext db = new DataBaseContext())
            {
                var OFinancialYear = db.Calendar.Where(e => e.Id == FinancialYear_Id).SingleOrDefault();

                var OState = db.EmpSalStruct
                    //.Include(e => e.GeoStruct)
                    //.Include(e => e.GeoStruct.Location)
                    //.Include(e => e.GeoStruct.Location.Address)
                    .Include(e => e.GeoStruct.Location.Address.State)
                    .Where(e => e.Id == OEmpSalstruct)
                    .Select(e => new
                    {
                        State_id = e.GeoStruct.Location == null ? 0 : e.GeoStruct.Location.Address == null ? 0 :
                            e.GeoStruct.Location.Address.State == null ? 0 : e.GeoStruct.Location.Address.State.Id
                    })
                    .SingleOrDefault();

                Int32 mState = OState.State_id;

                PTaxMaster mPTMaster1 = OCompanyPTaxMaster
                .Where(e => e.EffectiveDate < Convert.ToDateTime("01/" + PayMonth).AddMonths(1).Date
                    && (e.EndDate >= Convert.ToDateTime("01/" + PayMonth).Date || e.EndDate == null) && e.States.Id == mState
                    ).SingleOrDefault();

                PTaxMaster OPTaxMaster = db.PTaxMaster.Where(e => e.Id == mPTMaster1.Id).SingleOrDefault();
                Wages PTWagesMaster = db.Wages.Where(e => e.Id == OPTaxMaster.PTWagesMaster_Id).SingleOrDefault();
                OPTaxMaster.PTWagesMaster = PTWagesMaster;
                List<RateMaster> RateMaster = db.Wages.Where(e => e.Id == OPTaxMaster.PTWagesMaster_Id).Select(e => e.RateMaster.ToList()).SingleOrDefault();

                foreach (var RateMasteritem in RateMaster)
                {
                    SalaryHead SalHead = db.SalaryHead.Where(e => e.Id == RateMasteritem.SalHead_Id).SingleOrDefault();
                    RateMasteritem.SalHead = SalHead;

                }
                OPTaxMaster.PTWagesMaster.RateMaster = RateMaster;
                List<StatutoryEffectiveMonths> PTStatutoryEffectiveMonths = db.PTaxMaster.Where(e => e.Id == mPTMaster1.Id).Select(e => e.PTStatutoryEffectiveMonths.ToList()).SingleOrDefault();

                foreach (var PTStatutoryEffectiveMonthsitem in PTStatutoryEffectiveMonths)
                {
                    List<Range> StatutoryWageRange = db.StatutoryEffectiveMonths.Where(e => e.Id == PTStatutoryEffectiveMonthsitem.Id).Select(e => e.StatutoryWageRange.ToList()).SingleOrDefault();
                    PTStatutoryEffectiveMonthsitem.StatutoryWageRange = StatutoryWageRange;
                    LookupValue EffectiveMonth = db.LookupValue.Where(e => e.Id == PTStatutoryEffectiveMonthsitem.EffectiveMonth_Id).SingleOrDefault();
                    PTStatutoryEffectiveMonthsitem.EffectiveMonth = EffectiveMonth;
                    LookupValue Gender1 = db.LookupValue.Where(e => e.Id == PTStatutoryEffectiveMonthsitem.Gender_Id).SingleOrDefault();
                    PTStatutoryEffectiveMonthsitem.Gender = Gender1;


                }
                OPTaxMaster.PTStatutoryEffectiveMonths = PTStatutoryEffectiveMonths;
                State States = db.State.Where(e => e.Id == OPTaxMaster.States_Id).SingleOrDefault();
                OPTaxMaster.States = States;
                LookupValue Frequency = db.LookupValue.Where(e => e.Id == OPTaxMaster.Frequency_Id).SingleOrDefault();
                OPTaxMaster.Frequency = Frequency;


                //.Include(e => e.PTWagesMaster)
                //   .Include(e => e.PTWagesMaster.RateMaster)
                //    .Include(e => e.PTWagesMaster.RateMaster.Select(t => t.SalHead))
                //   .Include(e => e.PTStatutoryEffectiveMonths)
                //   .Include(e => e.PTStatutoryEffectiveMonths.Select(f => f.StatutoryWageRange))
                //   .Include(e => e.PTStatutoryEffectiveMonths.Select(f => f.EffectiveMonth))
                //   .Include(e => e.PTStatutoryEffectiveMonths.Select(f => f.Gender))
                //   .Include(e => e.States)
                //   .Include(e => e.Frequency)


                double mPTAmount = 0;
                string mMonthPay = Convert.ToDateTime("01/" + PayMonth).ToString("MMMMM").ToUpper();
                string Gender = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.Employee.Gender).Where(e => e.Id == OEmployeePayroll).FirstOrDefault().Employee.Gender.LookupVal;
                StatutoryEffectiveMonths mEffectiveMonthObj = OPTaxMaster.PTStatutoryEffectiveMonths.Where(d => d.EffectiveMonth.LookupVal.ToUpper() == mMonthPay && d.Gender.LookupVal.ToUpper() == Gender).SingleOrDefault();
                double mPTWages = Process.SalaryHeadGenProcess.WagecalcDirect(OPTaxMaster.PTWagesMaster, OSalaryDetails, oEmpSalStructDetails);

                //PT logic
                if (OPTaxMaster.Frequency.LookupVal.ToUpper() == "MONTHLY")//monthly ptax calculation
                {
                    if (mEffectiveMonthObj != null)
                    {
                        foreach (Range ca in mEffectiveMonthObj.StatutoryWageRange)//check tomorrow
                        {
                            if (mPTWages >= ca.RangeFrom && mPTWages <= ca.RangeTo)
                            {
                                if (ca.EmpShareAmount != null || ca.EmpShareAmount != 0)
                                {
                                    mPTAmount = ca.EmpShareAmount;
                                    break;
                                }
                                if (ca.EmpSharePercentage != null || ca.EmpSharePercentage != 0)
                                {
                                    mPTAmount = mPTWages * ca.EmpSharePercentage;
                                    break;
                                }

                            }
                        }
                    }

                }
                else if (OPTaxMaster.Frequency.LookupVal.ToUpper() == "HALFYEARLY")//Halfyearly ptax calculation
                {
                    if (mEffectiveMonthObj != null)
                    {
                        mPTWages = 0;
                        //calculate half year span*****
                        DateTime StartMonth = DateTime.Now.Date;
                        DateTime EndMonth = DateTime.Now.Date;
                        bool IsPTAppl = true;

                        if (Convert.ToDateTime("01/" + PayMonth) >= OFinancialYear.FromDate && Convert.ToDateTime("01/" + PayMonth) <= OFinancialYear.FromDate.Value.AddMonths(6).AddDays(-1))
                        {
                            if (FromPeriod >= OFinancialYear.FromDate.Value.AddMonths(6))
                            {
                                IsPTAppl = false;
                            }
                            else
                            {
                                StartMonth = OFinancialYear.FromDate.Value;
                                if (FromPeriod > StartMonth)
                                {
                                    StartMonth = FromPeriod;
                                }
                                EndMonth = OFinancialYear.FromDate.Value.AddMonths(6).AddDays(-1);
                                if (ToPeriod < EndMonth)
                                {
                                    EndMonth = ToPeriod;
                                }
                            }

                        }
                        else
                        {
                            StartMonth = OFinancialYear.FromDate.Value.AddMonths(6);
                            if (ToPeriod < StartMonth)
                            {
                                IsPTAppl = false;
                            }
                            else
                            {
                                if (FromPeriod > StartMonth)
                                {
                                    StartMonth = FromPeriod;
                                }
                                EndMonth = OFinancialYear.ToDate.Value;
                                if (ToPeriod < EndMonth)
                                {
                                    EndMonth = ToPeriod;
                                }
                            }
                        }

                        //From and To Period check
                        if (IsPTAppl == true)
                        {
                            for (DateTime mdate = StartMonth; mdate <= EndMonth; mdate = mdate.AddMonths(1))
                            {
                                //EmployeePayroll OEmpPayroll = db.EmployeePayroll.Include(e => e.SalaryT).Include(e => e.SalaryT.Select(r => r.SalEarnDedT))
                                //    .Where(e => e.Id == OEmployeePayroll.Id).SingleOrDefault();
                                //SalaryT OSalaryTForPT = db.SalaryT.Include(e => e.SalEarnDedT).Where(e => e.EmployeePayroll_Id == OEmployeePayroll && e.PayMonth == mdate.ToString("MM/yyyy", System.Globalization.CultureInfo.InvariantCulture)).SingleOrDefault();

                                string oSal_Tpaymonth = mdate.ToString("MM/yyyy").ToString();
                                EmployeePayroll OEmpPayroll = db.EmployeePayroll.Where(e => e.Id == OEmployeePayroll).SingleOrDefault();
                                OEmpPayroll.SalaryT = db.SalaryT.Where(e => e.PayMonth == oSal_Tpaymonth && e.EmployeePayroll_Id == OEmployeePayroll).ToList();
                                foreach (var itemSal_T in OEmpPayroll.SalaryT)
                                {
                                    itemSal_T.SalEarnDedT = db.SalEarnDedT.Where(e => e.SalaryT_Id == itemSal_T.Id).ToList();
                                    foreach (var itemSalEarnDedT in itemSal_T.SalEarnDedT.ToList())
                                    {
                                        itemSalEarnDedT.SalaryHead = db.SalaryHead.Where(e => e.Id == itemSalEarnDedT.SalaryHead_Id).FirstOrDefault();
                                        itemSalEarnDedT.SalaryHead.Frequency = db.LookupValue.Where(e => e.Id == itemSalEarnDedT.SalaryHead.Frequency_Id).FirstOrDefault();
                                        itemSalEarnDedT.SalaryHead.Type = db.LookupValue.Where(e => e.Id == itemSalEarnDedT.SalaryHead.Type_Id).FirstOrDefault();
                                        itemSalEarnDedT.SalaryHead.SalHeadOperationType = db.LookupValue.Where(e => e.Id == itemSalEarnDedT.SalaryHead.SalHeadOperationType_Id).FirstOrDefault();
                                        itemSalEarnDedT.SalaryHead.RoundingMethod = db.LookupValue.Where(e => e.Id == itemSalEarnDedT.SalaryHead.RoundingMethod_Id).FirstOrDefault();
                                        itemSalEarnDedT.SalaryHead.ProcessType = db.LookupValue.Where(e => e.Id == itemSalEarnDedT.SalaryHead.ProcessType_Id).FirstOrDefault();
                                    }
                                }

                                SalaryT OSalaryTForPT = OEmpPayroll.SalaryT.Where(e => e.EmployeePayroll_Id == OEmployeePayroll && e.PayMonth == mdate.ToString("MM/yyyy", System.Globalization.CultureInfo.InvariantCulture)).SingleOrDefault();
                                if (OSalaryTForPT != null)
                                {
                                    List<SalEarnDedT> OSalaryTForPTDetails = OSalaryTForPT.SalEarnDedT.ToList();
                                    mPTWages = mPTWages + SalaryHeadGenProcess.WagecalcDirect(OPTaxMaster.PTWagesMaster, OSalaryTForPTDetails, oEmpSalStructDetails);
                                }
                                else
                                {

                                    mPTWages = mPTWages + SalaryHeadGenProcess.WagecalcDirect(OPTaxMaster.PTWagesMaster, null, oEmpSalStructDetails);
                                }
                            }
                            foreach (Range ca in mEffectiveMonthObj.StatutoryWageRange)//check tomorrow
                            {
                                if (mPTWages >= ca.RangeFrom && mPTWages <= ca.RangeTo)
                                {
                                    if (ca.EmpShareAmount != null || ca.EmpShareAmount != 0)
                                    {
                                        mPTAmount = ca.EmpShareAmount;
                                        break;
                                    }
                                    if (ca.EmpSharePercentage != null || ca.EmpSharePercentage != 0)
                                    {
                                        mPTAmount = mPTWages * ca.EmpSharePercentage;
                                        break;
                                    }

                                }
                            }
                        }


                    }

                }

                else if (OPTaxMaster.Frequency.LookupVal.ToUpper() == "YEARLY")//yearly ptax calculation
                {
                    if (mEffectiveMonthObj != null)
                    {
                        mPTWages = 0;
                        //calculate half year span*****
                        DateTime StartMonth = OFinancialYear.FromDate.Value;
                        DateTime EndMonth = OFinancialYear.ToDate.Value;

                        if (FromPeriod > StartMonth)
                        {
                            StartMonth = FromPeriod;
                        }

                        if (ToPeriod < EndMonth)
                        {
                            EndMonth = ToPeriod;
                        }
                        for (DateTime mdate = StartMonth; mdate <= EndMonth; mdate = mdate.AddMonths(1))
                        {
                            //EmployeePayroll OEmpPayroll = db.EmployeePayroll.Include(e => e.SalaryT).Include(e => e.SalaryT.Select(r => r.SalEarnDedT))
                            //    .Where(e => e.Id == OEmployeePayroll.Id).SingleOrDefault();
                            //SalaryT OSalaryTForPT = db.SalaryT.Include(e => e.SalEarnDedT).Where(e => e.EmployeePayroll_Id == OEmployeePayroll && e.PayMonth == mdate.ToString("MM/yyyy", System.Globalization.CultureInfo.InvariantCulture)).SingleOrDefault();

                            string oSal_Tpaymonth = mdate.ToString("MM/yyyy").ToString();
                            EmployeePayroll OEmpPayroll = db.EmployeePayroll.Where(e => e.Id == OEmployeePayroll).SingleOrDefault();
                            OEmpPayroll.SalaryT = db.SalaryT.Where(e => e.PayMonth == oSal_Tpaymonth && e.EmployeePayroll_Id == OEmployeePayroll).ToList();
                            foreach (var itemSal_T in OEmpPayroll.SalaryT)
                            {
                                itemSal_T.SalEarnDedT = db.SalEarnDedT.Where(e => e.SalaryT_Id == itemSal_T.Id).ToList();
                                foreach (var itemSalEarnDedT in itemSal_T.SalEarnDedT.ToList())
                                {
                                    itemSalEarnDedT.SalaryHead = db.SalaryHead.Where(e => e.Id == itemSalEarnDedT.SalaryHead_Id).FirstOrDefault();
                                    itemSalEarnDedT.SalaryHead.Frequency = db.LookupValue.Where(e => e.Id == itemSalEarnDedT.SalaryHead.Frequency_Id).FirstOrDefault();
                                    itemSalEarnDedT.SalaryHead.Type = db.LookupValue.Where(e => e.Id == itemSalEarnDedT.SalaryHead.Type_Id).FirstOrDefault();
                                    itemSalEarnDedT.SalaryHead.SalHeadOperationType = db.LookupValue.Where(e => e.Id == itemSalEarnDedT.SalaryHead.SalHeadOperationType_Id).FirstOrDefault();
                                    itemSalEarnDedT.SalaryHead.RoundingMethod = db.LookupValue.Where(e => e.Id == itemSalEarnDedT.SalaryHead.RoundingMethod_Id).FirstOrDefault();
                                    itemSalEarnDedT.SalaryHead.ProcessType = db.LookupValue.Where(e => e.Id == itemSalEarnDedT.SalaryHead.ProcessType_Id).FirstOrDefault();
                                }
                            }

                            SalaryT OSalaryTForPT = OEmpPayroll.SalaryT.Where(e => e.EmployeePayroll_Id == OEmployeePayroll && e.PayMonth == mdate.ToString("MM/yyyy", System.Globalization.CultureInfo.InvariantCulture)).SingleOrDefault();

                            if (OSalaryTForPT != null)
                            {
                                List<SalEarnDedT> OSalaryTForPTDetails = OSalaryTForPT.SalEarnDedT.ToList();
                                mPTWages = mPTWages + SalaryHeadGenProcess.WagecalcDirect(OPTaxMaster.PTWagesMaster, OSalaryTForPTDetails, oEmpSalStructDetails);
                            }
                            else
                            {

                                mPTWages = mPTWages + SalaryHeadGenProcess.WagecalcDirect(OPTaxMaster.PTWagesMaster, null, oEmpSalStructDetails);
                            }
                        }
                        foreach (Range ca in mEffectiveMonthObj.StatutoryWageRange)//check tomorrow
                        {
                            if (mPTWages >= ca.RangeFrom && mPTWages <= ca.RangeTo)
                            {
                                if (ca.EmpShareAmount != null || ca.EmpShareAmount != 0)
                                {
                                    mPTAmount = ca.EmpShareAmount;
                                    break;
                                }
                                if (ca.EmpSharePercentage != null || ca.EmpSharePercentage != 0)
                                {
                                    mPTAmount = mPTWages * ca.EmpSharePercentage;
                                    break;
                                }

                            }
                        }
                    }

                }

                return mPTAmount;
            }
        }
        public static double LICcalc(EmpSalStruct OEmpSalstruct, Int32 OEmployeePayroll, string PayMonth)
        {

            using (DataBaseContext db = new DataBaseContext())
            {
                double CalAmount = 0;
                EmployeePayroll OEmpInsurance = db.EmployeePayroll.Include(e => e.InsuranceDetailsT.Select(r => r.InsuranceProduct))
                        .Include(e => e.InsuranceDetailsT.Select(a => a.OperationStatus)).Where(d => d.Id == OEmployeePayroll)// OEmployeePayroll.InsuranceDetailsT;
                        .FirstOrDefault();

                List<InsuranceDetailsT> OInsurancePaymentT = OEmpInsurance.InsuranceDetailsT.ToList();
                if (OInsurancePaymentT != null && OInsurancePaymentT.Count() > 0)
                {
                    List<Insurance> InsuranceMaster = db.Insurance
                                .Include(e => e.InsuranceProduct)
                                .Include(e => e.SalaryHead)
                                .ToList();
                    var EmpInsuranceHead1 = OEmpSalstruct.EmpSalStructDetails.
                    Join(InsuranceMaster, u => u.SalaryHead.Id, uir => uir.SalaryHead.Id,
                    (u, uir) => new { u, uir })
                    .Select(m => new { InsuranceProduct = m.uir.InsuranceProduct, SalaryHead = m.u.SalaryHead }).ToList();

                    var EmpInsuranceHead = EmpInsuranceHead1.Select(e => e.InsuranceProduct
                        .Join(OInsurancePaymentT, u => u.Id, uir => uir.InsuranceProduct.Id,
                        (u, uir) => new { u, uir })
                        .Where(m => (m.uir.FromDate.Value.Date < Convert.ToDateTime("01/" + PayMonth).AddMonths(1).Date)
                         && (m.uir.ToDate.Value.Date >= Convert.ToDateTime("01/" + PayMonth).Date)
                         && m.uir.OperationStatus.LookupVal.ToUpper() == "ACTIVE")
                        .GroupBy(d => d.uir.InsuranceProduct.Id)
                        .Select(q => new { Amount = 0, SalaryHead = e.SalaryHead, Permium = q.Sum(x => x.uir.Premium) })).ToList();

                    //LIC logic
                    foreach (var ca in EmpInsuranceHead)
                    {

                        foreach (var ca1 in ca)
                        {
                            CalAmount = CalAmount + Math.Round((ca1.Permium), 0);


                        }

                    }
                }

                return CalAmount;
            }
        }
        public static CompanyPayroll _returnCompanyPayroll_PTaxMaster(Int32 mCompanyId)
        {
            //Utility.DumpProcessStatus("_returnCompanyPayroll_PTaxMaster");
            using (DataBaseContext db = new DataBaseContext())
            {
                CompanyPayroll CompanyPayroll = db.CompanyPayroll.Where(r => r.Company.Id == mCompanyId).AsParallel().FirstOrDefault();
                Company Company = db.Company.Where(e => e.Id == mCompanyId).SingleOrDefault();
                CompanyPayroll.Company = Company;
                List<PTaxMaster> PTaxMaster = new List<PTaxMaster>();
                PTaxMaster = db.PTaxMaster.Include(e => e.PTStatutoryEffectiveMonths).Where(e => e.CompanyPayroll_Id == CompanyPayroll.Id && e.EndDate == null).ToList();
                foreach (var OPTaxMaster in PTaxMaster)
                {

                    CompanyPayroll.PTaxMaster.Add(OPTaxMaster);
                    LookupValue Frequency = db.LookupValue.Where(e => e.Id == OPTaxMaster.Frequency_Id).SingleOrDefault();
                    OPTaxMaster.Frequency = Frequency;
                    List<StatutoryEffectiveMonths> PTStatutoryEffectiveMonths = db.StatutoryEffectiveMonths.Where(e => e.PTaxMaster_Id == OPTaxMaster.Id).ToList();

                    foreach (var PTStatutoryEffectiveMonthsitem in PTStatutoryEffectiveMonths)
                    {
                        LookupValue EffectiveMonth = db.LookupValue.Where(e => e.Id == PTStatutoryEffectiveMonthsitem.EffectiveMonth_Id).SingleOrDefault();
                        PTStatutoryEffectiveMonthsitem.EffectiveMonth = EffectiveMonth;
                    }
                    OPTaxMaster.PTStatutoryEffectiveMonths = PTStatutoryEffectiveMonths;
                    Wages PTWagesMaster = db.Wages.Where(e => e.Id == OPTaxMaster.PTWagesMaster_Id).SingleOrDefault();
                    OPTaxMaster.PTWagesMaster = PTWagesMaster;
                    State States = db.State.Where(e => e.Id == OPTaxMaster.States_Id).SingleOrDefault();
                    OPTaxMaster.States = States;
                    List<RateMaster> RateMaster = db.Wages.Where(e => e.Id == OPTaxMaster.PTWagesMaster_Id).Select(e => e.RateMaster.ToList()).SingleOrDefault();

                    foreach (var RateMasteritem in RateMaster)
                    {
                        SalaryHead SalHead = db.SalaryHead.Where(e => e.Id == RateMasteritem.SalHead_Id).SingleOrDefault();
                        RateMasteritem.SalHead = SalHead;

                    }
                    PTWagesMaster.RateMaster = RateMaster;
                }
                //.Include(e => e.Company)
                //.Include(e => e.PTaxMaster)
                //.Include(e => e.PTaxMaster.Select(r => r.Frequency))
                //.Include(e => e.PTaxMaster.Select(r => r.PTStatutoryEffectiveMonths))
                //.Include(e => e.PTaxMaster.Select(r => r.PTWagesMaster))
                //.Include(e => e.PTaxMaster.Select(r => r.States))
                //.Include(e => e.PTaxMaster.Select(r => r.PTWagesMaster.RateMaster))
                //.Include(e => e.PTaxMaster.Select(r => r.PTWagesMaster.RateMaster.Select(r0 => r0.SalHead)))
                //  .Include(e => e.PTaxMaster.Select(r => r.PTStatutoryEffectiveMonths.Select(s => s.EffectiveMonth)))
                //  .Where(r => r.Company.Id == mCompanyId).AsParallel().FirstOrDefault();

                //.Where(r => r.Company.Id == mCompanyId).AsParallel().SingleOrDefault();
                ////var b = a.Where(r => r.Company.Id == mCompanyId).AsParallel().SingleOrDefault();
                ////return b;
                return CompanyPayroll;
            }
        }
        public static CompanyPayroll _returnCompanyPayroll_IncomeTax_New(Int32 mCompanyId, Int32 FyYearCalendar)
        {//financial year passing
            //Utility.DumpProcessStatus("_returnCompanyPayroll_IncomeTax");
            using (DataBaseContext db = new DataBaseContext())
            {
                CompanyPayroll CompanyPayroll = db.CompanyPayroll.Where(e => e.Company.Id == mCompanyId).FirstOrDefault();
                IncomeTax IncomeTax = db.IncomeTax.Where(e => e.CompanyPayroll_Id == CompanyPayroll.Id && e.FyCalendar_Id == FyYearCalendar).SingleOrDefault();
                CompanyPayroll.IncomeTax.Add(IncomeTax);
                Calendar FyCalendar = db.Calendar.Where(e => e.Id == IncomeTax.FyCalendar_Id).SingleOrDefault();
                IncomeTax.FyCalendar = FyCalendar;
                List<ITSection> ITSection = db.IncomeTax.Where(e => e.CompanyPayroll_Id == CompanyPayroll.Id && e.FyCalendar_Id == FyYearCalendar).Select(e => e.ITSection.ToList()).SingleOrDefault();


                foreach (var ITSectionitem in ITSection)
                {
                    List<ITInvestment> ITInvestments = db.ITInvestment.Where(e => e.ITSection_Id == ITSectionitem.Id).ToList();

                    foreach (var ITInvestmentsitem in ITInvestments)
                    {
                        List<ITSubInvestment> ITSubInvestment = db.ITSubInvestment.Where(e => e.ITInvestment_Id == ITInvestmentsitem.Id).ToList();
                        if (ITSubInvestment != null) ITInvestmentsitem.ITSubInvestment = ITSubInvestment;
                        SalaryHead SalaryHead = db.SalaryHead.Where(e => e.Id == ITInvestmentsitem.SalaryHead_Id).SingleOrDefault();
                        if (SalaryHead != null) ITInvestmentsitem.SalaryHead = SalaryHead;

                    }
                    ITSectionitem.ITInvestments = ITInvestments;
                    var dd = 0;
                    List<LoanAdvanceHead> LoanAdvanceHeadList = db.ITSection.Where(e => e.Id == ITSectionitem.Id).Select(e => e.LoanAdvanceHead.ToList()).SingleOrDefault();

                    foreach (var LoanAdvanceHeaditem in LoanAdvanceHeadList)
                    {
                        //LoanAdvanceHead LoanAdvanceHead = db.LoanAdvanceHead.Where(e => e.Id == LoanAdvanceHeaditem.Id).SingleOrDefault();
                        if (LoanAdvanceHeaditem != null)
                        {
                            //ITSectionitem.LoanAdvanceHead.Add(LoanAdvanceHead);
                            ITLoan ITLoan = db.ITLoan.Where(e => e.Id == LoanAdvanceHeaditem.ITLoan_Id).SingleOrDefault();
                            LoanAdvanceHeaditem.ITLoan = ITLoan;
                            SalaryHead SalaryHead = db.SalaryHead.Where(e => e.Id == LoanAdvanceHeaditem.SalaryHead_Id).SingleOrDefault();
                            LoanAdvanceHeaditem.SalaryHead = SalaryHead;


                        }

                    }
                    ITSectionitem.LoanAdvanceHead = LoanAdvanceHeadList;
                    var sd = 0;
                    List<ITStandardITRebate> ITStandardITRebate = db.ITStandardITRebate.Include(e => e.Regime).Where(e => e.ITSection_Id == ITSectionitem.Id).ToList();
                    ITSectionitem.ITStandardITRebate = ITStandardITRebate;
                    List<ITSection10> ITSection10 = db.ITSection10.Where(e => e.ITSection_Id == ITSectionitem.Id).ToList();

                    foreach (var ITSection10sitem in ITSection10)
                    {
                        List<ITSection10SalHeads> Itsection10salheads = db.ITSection10SalHeads.Where(e => e.ITSection10_Id == ITSection10sitem.Id).ToList();

                        foreach (var Itsection10salheadsitem in Itsection10salheads)
                        {
                            SalaryHead SalaryHead1 = db.SalaryHead.Where(e => e.Id == Itsection10salheadsitem.SalHead_Id).SingleOrDefault();
                            if (SalaryHead1 != null)
                            {

                                LookupValue Frequency = db.LookupValue.Where(e => e.Id == SalaryHead1.Frequency_Id).SingleOrDefault();
                                SalaryHead1.Frequency = Frequency;
                                LookupValue SalHeadOperationType = db.LookupValue.Where(e => e.Id == SalaryHead1.SalHeadOperationType_Id).SingleOrDefault();
                                SalaryHead1.SalHeadOperationType = SalHeadOperationType;
                                Itsection10salheadsitem.SalHead = SalaryHead1;
                            }


                        }
                        ITSection10sitem.Itsection10salhead = Itsection10salheads;
                    }
                    ITSectionitem.ITSection10 = ITSection10;
                    LookupValue ITSectionList = db.LookupValue.Where(e => e.Id == ITSectionitem.ITSectionList_Id).SingleOrDefault();
                    ITSectionitem.ITSectionList = ITSectionList;
                    LookupValue ITSectionListType = db.LookupValue.Where(e => e.Id == ITSectionitem.ITSectionListType_Id).SingleOrDefault();
                    ITSectionitem.ITSectionListType = ITSectionListType;


                }
                IncomeTax.ITSection = ITSection;

                //List<LoanAdvanceHead> LoanAdvanceHead = db.LoanAdvanceHead.Where(e => e.it == ITSectionitem.LoanAdvanceHead).ToList();
                List<ITTDS> ITTDS = db.IncomeTax.Where(e => e.CompanyPayroll_Id == CompanyPayroll.Id && e.FyCalendar_Id == FyYearCalendar).Select(e => e.ITTDS.ToList()).SingleOrDefault();
                IncomeTax.ITTDS = ITTDS;
                foreach (var ITTDSitem in ITTDS)
                {
                    LookupValue Category = db.LookupValue.Where(e => e.Id == ITTDSitem.Category_Id).SingleOrDefault();
                    ITTDSitem.Category = Category;
                }

                return CompanyPayroll;
            }


            //.Include(e => e.IncomeTax)
            //.Include(e => e.IncomeTax.Select(r => r.FyCalendar))
            //.Include(e => e.IncomeTax.Select(r => r.ITSection))
            //.Include(e => e.IncomeTax.Select(r => r.ITSection.Select(s => s.ITInvestments)))
            //.Include(e => e.IncomeTax.Select(r => r.ITSection.Select(s => s.ITInvestments.Select(m => m.ITSubInvestment))))
            //.Include(e => e.IncomeTax.Select(r => r.ITSection.Select(s => s.ITInvestments.Select(m => m.SalaryHead))))
            //.Include(e => e.IncomeTax.Select(r => r.ITSection.Select(s => s.LoanAdvanceHead)))
            //.Include(e => e.IncomeTax.Select(r => r.ITSection.Select(s => s.LoanAdvanceHead.Select(d => d.SalaryHead))))
            //.Include(e => e.IncomeTax.Select(r => r.ITSection.Select(s => s.LoanAdvanceHead.Select(d => d.ITLoan))))
            //.Include(e => e.IncomeTax.Select(r => r.ITSection.Select(d => d.ITStandardITRebate)))
            //.Include(e => e.IncomeTax.Select(r => r.ITSection.Select(d => d.ITSection10)))
            //.Include(e => e.IncomeTax.Select(r => r.ITSection.Select(d => d.ITSection10.Select(s => s.Itsection10salhead))))
            //.Include(e => e.IncomeTax.Select(r => r.ITSection.Select(d => d.ITSection10.Select(s => s.Itsection10salhead.Select(c => c.Frequency)))))
            //.Include(e => e.IncomeTax.Select(r => r.ITSection.Select(d => d.ITSection10.Select(s => s.Itsection10salhead.Select(t => t.SalHead)))))
            //.Include(e => e.IncomeTax.Select(r => r.ITSection.Select(d => d.ITSection10.Select(s => s.Itsection10salhead.Select(t => t.SalHead.SalHeadOperationType)))))
            //.Include(e => e.IncomeTax.Select(r => r.ITSection.Select(d => d.ITSectionList)))
            //.Include(e => e.IncomeTax.Select(r => r.ITSection.Select(d => d.ITSectionListType)))
            //.Include(e => e.IncomeTax.Select(r => r.ITTDS))
            //.Include(e => e.IncomeTax.Select(r => r.ITTDS.Select(d => d.Category)))
            // //return a.Where(e => e.Company.Id == mCompanyId).AsParallel()
            // //   .SingleOrDefault();


        }
        public static List<SalaryT> _returnEmployeePayroll_SalaryT(Int32 OEmployeePayroll)
        {
            //Utility.DumpProcessStatus("_returnEmployeePayroll_SalaryT");
            using (DataBaseContext db = new DataBaseContext())
            {
                /*   return db.EmployeePayroll
                         .Include(e => e.SalaryT)
                         .Include(e => e.YearlyPaymentT)
                         .Include(e => e.SalaryT.Select(r => r.SalEarnDedT))
                         .Include(e => e.SalaryT.Select(r => r.SalEarnDedT.Select(w => w.SalaryHead)))
                         .Include(e => e.SalaryT.Select(r => r.SalEarnDedT.Select(w => w.SalaryHead.Frequency)))
                         .Include(e => e.SalaryT.Select(r => r.SalEarnDedT.Select(w => w.SalaryHead.Type)))
                         .Include(e => e.SalaryT.Select(r => r.SalEarnDedT.Select(w => w.SalaryHead.SalHeadOperationType)))
                         .Include(e => e.SalaryT.Select(r => r.SalEarnDedT.Select(w => w.SalaryHead.RoundingMethod)))
                         .Include(e => e.SalaryT.Select(r => r.SalEarnDedT.Select(w => w.SalaryHead.ProcessType)))
                         .Include(e => e.SalaryArrearT)
                         .Include(e => e.SalaryArrearT.Select(r => r.ArrearType))
                         .Include(e => e.SalaryArrearT.Select(r => r.SalaryArrearPaymentT))
                         .Include(e => e.OtherEarningDeductionT)
                         .Include(e => e.OtherEarningDeductionT.Select(r => r.SalaryHead))
                         .Include(e => e.OtherEarningDeductionT.Select(r => r.SalaryHead.SalHeadOperationType)).Where(e => e.Id == OEmployeePayroll).AsParallel()
                         .SingleOrDefault();
   */
                List<SalaryT> SalaryT = db.SalaryT.Where(e => e.EmployeePayroll_Id == OEmployeePayroll).AsParallel().ToList();
                foreach (var SalaryTitem in SalaryT)
                {
                    List<SalEarnDedT> SalEarnDedT = db.SalEarnDedT.Where(e => e.SalaryT_Id == SalaryTitem.Id).ToList();

                    foreach (var SalEarnDedTitem in SalEarnDedT)
                    {
                        SalaryHead SalaryHead = db.SalaryHead.Where(e => e.Id == SalEarnDedTitem.SalaryT_Id).SingleOrDefault();
                        SalEarnDedTitem.SalaryHead = SalaryHead;
                        LookupValue Frequency = db.LookupValue.Where(e => e.Id == SalaryHead.Frequency_Id).SingleOrDefault();
                        SalaryHead.Frequency = Frequency;
                        LookupValue Type = db.LookupValue.Where(e => e.Id == SalaryHead.Type_Id).SingleOrDefault();
                        SalaryHead.Type = Type;
                        LookupValue SalHeadOperationType = db.LookupValue.Where(e => e.Id == SalaryHead.SalHeadOperationType_Id).SingleOrDefault();
                        SalaryHead.SalHeadOperationType = SalHeadOperationType;
                        LookupValue RoundingMethod = db.LookupValue.Where(e => e.Id == SalaryHead.RoundingMethod_Id).SingleOrDefault();
                        SalaryHead.RoundingMethod = RoundingMethod;
                        LookupValue ProcessType = db.LookupValue.Where(e => e.Id == SalaryHead.ProcessType_Id).SingleOrDefault();
                        SalaryHead.ProcessType = ProcessType;

                    }
                    SalaryTitem.SalEarnDedT = SalEarnDedT;
                    //.Include(r => r.SalEarnDedT)
                    //.Include(r => r.SalEarnDedT.Select(w => w.SalaryHead))
                    //.Include(r => r.SalEarnDedT.Select(w => w.SalaryHead.Frequency))
                    //.Include(r => r.SalEarnDedT.Select(w => w.SalaryHead.Type))
                    //.Include(r => r.SalEarnDedT.Select(w => w.SalaryHead.SalHeadOperationType))
                    //.Include(r => r.SalEarnDedT.Select(w => w.SalaryHead.RoundingMethod))
                    //.Include(r => r.SalEarnDedT.Select(w => w.SalaryHead.ProcessType))

                }

                ////return a.Where(e => e.Id == OEmployeePayroll).AsParallel()
                ////      .SingleOrDefault();
                return SalaryT;
            }
        }
        public class ItProjectionTempClass
        {
            public double ActualAmount { get; set; }
            public double ActualQualifyingAmount { get; set; }
            public string ChapterName { get; set; }
            // public DBTrack DBTrack { get; set; }
            public Int32 FinancialYear { get; set; }
            public string Form16Header { get; set; }
            public string Form24Header { get; set; }
            public DateTime? FromPeriod { get; set; }
            public string Narration { get; set; }
            public int PickupId { get; set; }
            public double ProjectedAmount { get; set; }
            public double ProjectedQualifyingAmount { get; set; }
            // public DateTime? ProjectionDate { get; set; }
            public double QualifiedAmount { get; set; }
            public int SalayHead { get; set; }
            public string Section { get; set; }
            public string SectionType { get; set; }
            public string SubChapter { get; set; }
            public double TDSComponents { get; set; }
            public string Tiltle { get; set; }
            public DateTime? ToPeriod { get; set; }
        }

        public static List<ItProjectionTempClass> Section10Cal(EmployeePayroll OEmployeePayroll, IncomeTax OITMaster, int OFinancialYear, List<ITSalaryHeadData> OITSalMonthwise, SalaryT OSalaryTC, DateTime mFromPeriod, DateTime mToPerod, List<string> mClosedMonths, double mBalMonths, int Flag)
        {
            //Utility.DumpProcessStatus("Section10Cal");
            #region test
            using (DataBaseContext db = new DataBaseContext())
            {
                DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };
                List<ItProjectionTempClass> OITProjectionDataList = new List<ItProjectionTempClass>();
                EmployeePayroll OEmpITSection10Data = db.EmployeePayroll
                     .Include(e => e.ITSection10Payment)
                     .Include(e => e.ITSection10Payment.Select(r => r.FinancialYear))
                     .Include(e => e.ITSection10Payment.Select(r => r.ITSection))
                     .Include(e => e.ITSection10Payment.Select(r => r.ITSection.ITSectionList))
                     .Include(e => e.ITSection10Payment.Select(r => r.ITSection.ITSectionListType))
                     .Include(e => e.ITSection10Payment.Select(r => r.ITSection.ITSection10))
                     .Include(e => e.ITSection10Payment.Select(r => r.ITSection.ITSection10.Select(t => t.Itsection10salhead)))
                     .Include(e => e.ITSection10Payment.Select(r => r.ITSection.ITSection10.Select(t => t.Itsection10salhead.Select(q => q.SalHead))))
                     .Where(e => e.Id == OEmployeePayroll.Id)
                     .SingleOrDefault();

                List<ITSection> OITSection = OITMaster.ITSection.Where(r => r.ITSectionList.LookupVal.ToUpper() == "SECTION10B").ToList();
                List<string> OSalHeadList = new List<string>();
                //List<ITSection10> OITSection10 = OITSection.SelectMany(e => e.ITSection10).ToList();
                //List<ITSection10SalHeads> OITSection10SalHead = OITSection10.SelectMany(a => a.Itsection10salhead).ToList();
                //  var OEmpITSection10 = OEmpITSection10Data.ITSection10Payment.Where(e => e.FinancialYear.Id == OFinancialYear).ToList();

                List<ITSection10Payment> OEmpITSec10Payment = OEmpITSection10Data.ITSection10Payment.Where(e => e.FinancialYear.Id == OFinancialYear).ToList();
                int ItsectionIdForHra = 0;
                int Itsection10BIdForHra = 0;



                string requiredPaths = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\App_Data\\Menu_Json";
                bool existss = System.IO.Directory.Exists(requiredPaths);
                string localPaths;
                if (!existss)
                {
                    localPaths = new Uri(requiredPaths).LocalPath;
                    System.IO.Directory.CreateDirectory(localPaths);
                }
                string paths = requiredPaths + @"\NewRegimeNotExempt" + ".ini";
                localPaths = new Uri(paths).LocalPath;
                if (!System.IO.File.Exists(localPaths))
                {

                    using (var fs = new FileStream(localPaths, FileMode.OpenOrCreate))
                    {
                        StreamWriter str = new StreamWriter(fs);
                        str.BaseStream.Seek(0, SeekOrigin.Begin);

                        str.Flush();
                        str.Close();
                        fs.Close();
                    }


                }


                string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\App_Data\\Menu_Json";
                bool exists = System.IO.Directory.Exists(requiredPath);
                string localPath;
                if (!exists)
                {
                    localPath = new Uri(requiredPath).LocalPath;
                    System.IO.Directory.CreateDirectory(localPath);
                }
                string path = requiredPath + @"\NewRegimeNotExempt" + ".ini";
                localPath = new Uri(path).LocalPath;
                DateTime? FinancialYr = null;
                string[] SalHeadList = null;
                DateTime finfrm = Convert.ToDateTime("01/04/2020");
                using (var streamReader = new StreamReader(localPath))
                {
                    string line;

                    while ((line = streamReader.ReadLine()) != null)
                    {
                        SalHeadList = line.Split('_');
                        FinancialYr = Convert.ToDateTime(SalHeadList[0]);
                        if (FinancialYr == mFromPeriod)
                        {
                            break;
                        }
                        else
                        {
                            SalHeadList = null;
                        }


                    }
                }

                //actual exemption
                var Regimi = db.EmployeePayroll.Include(e => e.RegimiScheme).Include(e => e.RegimiScheme.Select(x => x.FinancialYear)).Include(e => e.RegimiScheme.Select(y => y.Scheme)).Where(e => e.Id == OEmployeePayroll.Id).FirstOrDefault();
                var Regimischemecurryear = Regimi.RegimiScheme.Where(e => e.FinancialYear.Id == OFinancialYear).FirstOrDefault();

                foreach (ITSection ca0 in OITSection)
                {//00
                    foreach (ITSection10 ca1 in ca0.ITSection10) //check salhead or direct in data entry form; check exemption limit in entry form
                    {//01
                        if (ca1.ExemptionCode == "HRAEXEMPT")
                        {
                            OEmpITSec10Payment = OEmpITSec10Payment.Where(a => a.ITSection10.ExemptionCode != "HRAEXEMPT").ToList();
                            ItsectionIdForHra = ca0.Id;
                            Itsection10BIdForHra = ca1.Id;
                            // continue;
                        }
                        double mTotActExAmount = 0;
                        double mTotProjExAmount = 0;
                        foreach (ITSection10SalHeads ca2 in ca1.Itsection10salhead)
                        {//1

                            if (ca2.AutoPick == true)
                            {//2
                                //get data updated in transaction
                                if (ca2.Frequency.LookupVal.ToUpper() == "MONTHLY")
                                {//3

                                    List<ITSalaryHeadData> OEmpSalEarnChk = OITSalMonthwise.Where(e => e.SalaryHead.Id == ca2.SalHead.Id).ToList();
                                    if (OEmpSalEarnChk != null)
                                    {//4
                                        foreach (ITSalaryHeadData ca3 in OEmpSalEarnChk)
                                        {//5
                                            //ca3-eaning sal amount 
                                            //ca2-ceiling parameter to exemption

                                            //if (ca3.ActualAmount == 0)
                                            //{
                                            //    continue;
                                            //}
                                            double celamtact;
                                            if (ca2.Percent != 0)
                                            {
                                                celamtact = ca2.Amount + Math.Round((ca3.ActualAmount * ca2.Percent / 100), 0);

                                            }
                                            else
                                            {
                                                celamtact = ca2.Amount;
                                            }

                                            double celamtProj;
                                            if (ca2.Percent != 0)
                                            {
                                                celamtProj = ca2.Amount + Math.Round((ca3.ProjectedAmount * ca2.Percent / 100), 0);

                                            }
                                            else
                                            {
                                                celamtProj = ca2.Amount;
                                            }

                                            if (ca3.ActualAmount > celamtact)
                                            {
                                                mTotActExAmount = mTotActExAmount + celamtact;
                                            }
                                            else
                                            {
                                                mTotActExAmount = mTotActExAmount + ca3.ActualAmount;
                                            }

                                            if (ca3.ProjectedAmount > celamtProj)
                                            {
                                                mTotProjExAmount = mTotProjExAmount + celamtProj;
                                            }
                                            else
                                            {
                                                mTotProjExAmount = mTotProjExAmount + ca3.ProjectedAmount;
                                            }

                                        }//5
                                        // for new tax slab start 28122020

                                        if (Regimischemecurryear != null)
                                        {
                                            if (Regimischemecurryear.Scheme.LookupVal.ToUpper() == "NEWTAX")
                                            {
                                                if (SalHeadList != null)
                                                {
                                                    foreach (var item in SalHeadList)
                                                    {
                                                        if (item.ToUpper() == ca2.SalHead.Code.ToUpper())
                                                        {
                                                            mTotActExAmount = 0;
                                                            mTotProjExAmount = 0;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {

                                            if (mFromPeriod >= finfrm && OEmployeePayroll.Employee.EmpOffInfo.NationalityID.No2 == "Yes")
                                            {
                                                if (SalHeadList != null)
                                                {
                                                    foreach (var item in SalHeadList)
                                                    {
                                                        if (item.ToUpper() == ca2.SalHead.Code.ToUpper())
                                                        {
                                                            mTotActExAmount = 0;
                                                            mTotProjExAmount = 0;
                                                        }
                                                    }
                                                }

                                                //mTotActExAmount = 0;
                                                //mTotProjExAmount = 0;
                                            }
                                        }
                                        // for new tax slab start

                                        //if ( mTotActExAmount >=ca1.MaxAmount )
                                        //{
                                        //    mTotActExAmount = ca1.MaxAmount;
                                        //}


                                        //if (mTotProjExAmount >= ca1.MaxAmount )
                                        //{
                                        //    mTotProjExAmount = ca1.MaxAmount;
                                        //}

                                    }//4
                                }//3
                                else//yearly frequency
                                {//31
                                    double OEmpSalEarnChk = OITSalMonthwise.Where(e => e.SalaryHead.Id == ca2.SalHead.Id)
                                        .Sum(r => r.ActualAmount);

                                    double celamtact;
                                    if (ca2.Percent != 0)
                                    {
                                        celamtact = ca2.Amount + Math.Round((OEmpSalEarnChk * ca2.Percent / 100), 0);

                                    }
                                    else
                                    {
                                        celamtact = ca2.Amount;
                                    }



                                    if (OEmpSalEarnChk > celamtact)
                                    {
                                        mTotActExAmount = mTotActExAmount + celamtact;
                                    }
                                    else
                                    {
                                        mTotActExAmount = mTotActExAmount + OEmpSalEarnChk;
                                    }
                                    double OEmpSalEarnChk1 = OITSalMonthwise.Where(e => e.SalaryHead.Id == ca2.SalHead.Id)
                                        .Sum(r => r.ProjectedAmount);

                                    double celamtProj;
                                    if (ca2.Percent != 0)
                                    {
                                        celamtProj = ca2.Amount + Math.Round((OEmpSalEarnChk1 * ca2.Percent / 100), 0);

                                    }
                                    else
                                    {
                                        celamtProj = ca2.Amount;
                                    }

                                    if (OEmpSalEarnChk1 > celamtProj)
                                    {
                                        mTotProjExAmount = mTotProjExAmount + celamtProj;
                                    }
                                    else
                                    {
                                        mTotProjExAmount = mTotProjExAmount + OEmpSalEarnChk1;
                                    }

                                    // for new tax slab start 28122020
                                    //DateTime finfrm = Convert.ToDateTime("01/04/2020");
                                    if (Regimischemecurryear != null)
                                    {
                                        if (Regimischemecurryear.Scheme.LookupVal.ToUpper() == "NEWTAX" && (ca2.SalHead.SalHeadOperationType.LookupVal.ToUpper() != "LVENCASH" || ca2.SalHead.SalHeadOperationType.LookupVal.ToUpper() != "GRATUITY" || ca2.SalHead.SalHeadOperationType.LookupVal.ToUpper() != "LVENCASHEXEMPTED"))
                                        {
                                            mTotActExAmount = 0;
                                            mTotProjExAmount = 0;
                                        }
                                    }
                                    else
                                    {

                                        if (mFromPeriod >= finfrm && OEmployeePayroll.Employee.EmpOffInfo.NationalityID.No2 == "Yes" && (ca2.SalHead.SalHeadOperationType.LookupVal.ToUpper() != "LVENCASH" || ca2.SalHead.SalHeadOperationType.LookupVal.ToUpper() != "GRATUITY" || ca2.SalHead.SalHeadOperationType.LookupVal.ToUpper() != "LVENCASHEXEMPTED"))
                                        {
                                            mTotActExAmount = 0;
                                            mTotProjExAmount = 0;
                                        }
                                    }
                                    // for new tax slab start

                                    //if (mTotActExAmount >= ca1.MaxAmount)
                                    //{
                                    //    mTotActExAmount = ca1.MaxAmount;
                                    //}


                                    //if (mTotProjExAmount >= ca1.MaxAmount)
                                    //{
                                    //    mTotProjExAmount = ca1.MaxAmount;
                                    //}

                                }//31


                                //List<ITSection10Payment> OEmpITSec10PaymentModTemp = OEmpITSec10Payment.Where(e => e.ITSection != null).ToList();
                                ITSection10Payment OEmpITSec10PaymentMod = OEmpITSec10Payment.Where(e => e.ITSection != null &&
                                    e.ITSection10.Id == ca1.Id).SingleOrDefault();

                                if (OEmpITSec10PaymentMod != null)
                                {//6
                                    if (OEmpITSec10PaymentMod.ActualInvestment >= 0)
                                    {
                                        ITSection10Payment OEmpITSec10Obj = db.ITSection10Payment.Where(a => a.Id == OEmpITSec10PaymentMod.Id).SingleOrDefault();
                                        if (OEmpITSec10Obj != null)
                                        {//7
                                            OEmpITSec10Obj.ActualInvestment = mTotActExAmount;
                                            OEmpITSec10Obj.DeclaredInvestment = mTotProjExAmount;
                                            OEmpITSec10Obj.InvestmentDate = DateTime.Now.Date;
                                            OEmpITSec10Obj.Narration = "System Auto Pickup";
                                            OEmpITSec10Obj.DBTrack = dbt;

                                            db.Entry(OEmpITSec10Obj).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();
                                        }
                                    }//7
                                }//6
                                else
                                {//61
                                    ITSection10Payment OEmpITSec10P = new ITSection10Payment()
                                    {
                                        ITSection = db.ITSection.Include(e => e.ITInvestments).Include(e => e.ITSection10)
                                        .Include(e => e.ITSectionList)
                                        .Include(e => e.ITSectionListType)
                                        .Include(e => e.ITStandardITRebate)
                                        .Where(a => a.Id == ca0.Id).SingleOrDefault(),
                                        ITSection10 = db.ITSection10.Where(a => a.Id == ca1.Id).SingleOrDefault(),
                                        FinancialYear = db.Calendar.Where(a => a.Id == OFinancialYear).SingleOrDefault(),

                                        ActualInvestment = mTotActExAmount,
                                        DeclaredInvestment = mTotProjExAmount,
                                        InvestmentDate = DateTime.Now.Date,
                                        Narration = "System Auto Pickup",
                                        DBTrack = dbt
                                        //UploadDocId = null
                                    };
                                    //if (OSalaryTC == null && Flag == 1)
                                    if (Flag == 1)
                                    {
                                        db.ITSection10Payment.Add(OEmpITSec10P);
                                        db.SaveChanges();


                                        EmployeePayroll aa = db.EmployeePayroll.Where(a => a.Id == OEmployeePayroll.Id).SingleOrDefault();

                                        aa.ITSection10Payment.Add(OEmpITSec10P);
                                        db.EmployeePayroll.Attach(aa);
                                        db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                        OEmpITSec10Payment.Add(OEmpITSec10P);
                                    }

                                }//61
                            }//2
                        }//1
                    }
                    //**************** write projection data writing code ************************//
                    //************** section10 exemption writing *********************//


                    //************ writing section 10 exemption details in projection table *****************//
                    {
                        //var OSalaryEarnings = OSalaryHeadTotal.Where(e => e.SalaryHead.Type.LookupVal.ToUpper() == "EARNING").ToList();
                        double ActualQuali = 0;
                        double ProjQuali = 0;
                        double msection10exemp = 0;
                        var msection10v = OITSection
                                        .Sum(r => r.ExemptionLimit);
                        if (msection10v != null)
                        {
                            msection10exemp = msection10v;
                        }

                        if (OEmpITSec10Payment != null)
                        {
                            for (Int32 i = 0; i < OEmpITSec10Payment.Count(); i++)
                            {


                                ItProjectionTempClass OITProjectionSave = new ItProjectionTempClass();
                                if (OEmpITSec10Payment[i].ActualInvestment > OEmpITSec10Payment[i].ITSection10.MaxAmount)
                                {
                                    OITProjectionSave.ActualQualifyingAmount = OEmpITSec10Payment[i].ITSection10.MaxAmount;
                                }
                                else
                                {
                                    OITProjectionSave.ActualQualifyingAmount = OEmpITSec10Payment[i].ActualInvestment;
                                }
                                ActualQuali = ActualQuali + OITProjectionSave.ActualQualifyingAmount;
                                if (OEmpITSec10Payment[i].DeclaredInvestment > OEmpITSec10Payment[i].ITSection10.MaxAmount)
                                {
                                    OITProjectionSave.ProjectedQualifyingAmount = OEmpITSec10Payment[i].ITSection10.MaxAmount;
                                }
                                else
                                {
                                    OITProjectionSave.ProjectedQualifyingAmount = OEmpITSec10Payment[i].DeclaredInvestment;
                                }
                                ProjQuali = ProjQuali + OITProjectionSave.ProjectedQualifyingAmount;

                                {
                                    OITProjectionSave.FinancialYear = OFinancialYear;
                                    OITProjectionSave.Tiltle = "";
                                    OITProjectionSave.FromPeriod = null;
                                    OITProjectionSave.ToPeriod = null;
                                    OITProjectionSave.Section = OEmpITSec10Payment[i].ITSection.ITSectionList.LookupVal.ToUpper();
                                    OITProjectionSave.SectionType = OEmpITSec10Payment[i].ITSection.ITSectionListType.LookupVal.ToUpper();
                                    OITProjectionSave.ChapterName = "    " + OEmpITSec10Payment[i].ITSection10.ExemptionCode;
                                    OITProjectionSave.ProjectedAmount = OEmpITSec10Payment[i].DeclaredInvestment;
                                    OITProjectionSave.ActualAmount = OEmpITSec10Payment[i].ActualInvestment;
                                    // OITProjectionSave.ActualQualifyingAmount = 0,//OEmpITSec10Payment[i].ActualInvestment,
                                    //  OITProjectionSave.ProjectedQualifyingAmount = 0,//OEmpITSec10Payment[i].DeclaredInvestment,



                                    //   ProjectionDate = DateTime.Now.Date,
                                    OITProjectionSave.TDSComponents = 0;
                                    OITProjectionSave.QualifiedAmount = 0;
                                    OITProjectionSave.Narration = "Exempted Income details under section10";
                                    //    DBTrack = dbt,
                                    OITProjectionSave.PickupId = 41;
                                };
                                OITProjectionDataList.Add(OITProjectionSave);
                            }
                        }

            #endregion
                        //HRATest
                        // for new tax slab start 28122020
                        //DateTime finfrm = Convert.ToDateTime("01/04/2020");
                        if (Regimischemecurryear != null)
                        {
                            if (Regimischemecurryear.Scheme.LookupVal.ToUpper() == "NEWTAX")
                            {
                                if (mFromPeriod < finfrm)
                                {

                                    List<ItProjectionTempClass> OITProjectionDataList2_temp = HRAExempt(OEmployeePayroll.Id, OFinancialYear, mFromPeriod, mToPerod,
                                        mClosedMonths, mBalMonths, OITSalMonthwise, OEmpITSec10Payment, ItsectionIdForHra, Itsection10BIdForHra);

                                    if (OITProjectionDataList2_temp.Count > 0)
                                    {
                                        OITProjectionDataList.AddRange(OITProjectionDataList2_temp);
                                    }
                                }
                            }
                            else
                            {
                                List<ItProjectionTempClass> OITProjectionDataList2_temp = HRAExempt(OEmployeePayroll.Id, OFinancialYear, mFromPeriod, mToPerod,
                                                                    mClosedMonths, mBalMonths, OITSalMonthwise, OEmpITSec10Payment, ItsectionIdForHra, Itsection10BIdForHra);

                                if (OITProjectionDataList2_temp.Count > 0)
                                {
                                    OITProjectionDataList.AddRange(OITProjectionDataList2_temp);
                                }

                            }
                        }
                        else
                        {


                            if ((OEmployeePayroll.Employee.EmpOffInfo.NationalityID.No2 == "No" || OEmployeePayroll.Employee.EmpOffInfo.NationalityID.No2 == null || OEmployeePayroll.Employee.EmpOffInfo.NationalityID.No2 == ""))
                            {

                                List<ItProjectionTempClass> OITProjectionDataList2_temp = HRAExempt(OEmployeePayroll.Id, OFinancialYear, mFromPeriod, mToPerod,
                                    mClosedMonths, mBalMonths, OITSalMonthwise, OEmpITSec10Payment, ItsectionIdForHra, Itsection10BIdForHra);

                                if (OITProjectionDataList2_temp.Count > 0)
                                {
                                    OITProjectionDataList.AddRange(OITProjectionDataList2_temp);
                                }
                            }
                            else if (mFromPeriod < finfrm && OEmployeePayroll.Employee.EmpOffInfo.NationalityID.No2 == "Yes")
                            {
                                List<ItProjectionTempClass> OITProjectionDataList2_temp = HRAExempt(OEmployeePayroll.Id, OFinancialYear, mFromPeriod, mToPerod,
                                    mClosedMonths, mBalMonths, OITSalMonthwise, OEmpITSec10Payment, ItsectionIdForHra, Itsection10BIdForHra);

                                if (OITProjectionDataList2_temp.Count > 0)
                                {
                                    OITProjectionDataList.AddRange(OITProjectionDataList2_temp);
                                }
                            }
                        }
                        // for new tax slab start
                        //HRA END
                        //***************** Total exemption **********************//
                        {
                            var OSalaryExemptionTotal = OEmpITSec10Payment.GroupBy(e => e.ITSection.ITSectionList)
                            .Select(r => new { SalaryHead = r.Key, mActITIncome = r.Sum(t => t.ActualInvestment), mProjITIncome = r.Sum(t => t.DeclaredInvestment) })
                            .ToList();
                            //if (ActualQuali > msection10exemp)
                            //{
                            //    ActualQuali = msection10exemp;
                            //}
                            //if (ProjQuali > msection10exemp)
                            //{
                            //    ProjQuali = msection10exemp;
                            //}
                            ActualQuali = 0;
                            ProjQuali = 0;
                            for (Int32 i = 0; i < OEmpITSec10Payment.Count(); i++)
                            {

                                ItProjectionTempClass OITProjectionSave = new ItProjectionTempClass();
                                if (OEmpITSec10Payment[i].ActualInvestment > OEmpITSec10Payment[i].ITSection10.MaxAmount)
                                {
                                    OITProjectionSave.ActualQualifyingAmount = OEmpITSec10Payment[i].ITSection10.MaxAmount;
                                }
                                else
                                {
                                    OITProjectionSave.ActualQualifyingAmount = OEmpITSec10Payment[i].ActualInvestment;
                                }
                                ActualQuali = ActualQuali + OITProjectionSave.ActualQualifyingAmount;
                                if (OEmpITSec10Payment[i].DeclaredInvestment > OEmpITSec10Payment[i].ITSection10.MaxAmount)
                                {
                                    OITProjectionSave.ProjectedQualifyingAmount = OEmpITSec10Payment[i].ITSection10.MaxAmount;
                                }
                                else
                                {
                                    OITProjectionSave.ProjectedQualifyingAmount = OEmpITSec10Payment[i].DeclaredInvestment;
                                }
                                ProjQuali = ProjQuali + OITProjectionSave.ProjectedQualifyingAmount;
                            }

                            if (OSalaryExemptionTotal != null)
                            {
                                ItProjectionTempClass OITProjectionSave = new ItProjectionTempClass
                                {
                                    FinancialYear = OFinancialYear,
                                    Tiltle = "Total of Exemptions U/s10",
                                    FromPeriod = null,
                                    ToPeriod = null,
                                    Section = "",
                                    SectionType = "",
                                    ChapterName = "",
                                    SubChapter = "",
                                    //SalaryHeadName="",

                                    ProjectedAmount = OSalaryExemptionTotal.Sum(q => q.mProjITIncome),
                                    ActualAmount = OSalaryExemptionTotal.Sum(q => q.mActITIncome),
                                    ActualQualifyingAmount = ActualQuali,
                                    ProjectedQualifyingAmount = ProjQuali,

                                    //   ProjectionDate = DateTime.Now.Date,

                                    Narration = "Total Exemption under section10",
                                    //     DBTrack = dbt,
                                    PickupId = 42
                                };

                                OITProjectionDataList.Add(OITProjectionSave);
                            }

                        }
                    }
                }//00
                return OITProjectionDataList;
            }
        }

        public static List<ItProjectionTempClass> HRAExempt(int OEmployeePayroll_Id, Int32 OFinancialYear,
            DateTime mFromPeriod, DateTime mToPerod, List<string> mClosedMonths, double mBalMonths,
            List<ITSalaryHeadData> OITSalMonthwise, List<ITSection10Payment> OEmpITSec10Payment, int ItsectionIdForHra, int Itsection10BIdForHra)
        {
            //calculate HRA wages
            //HRA calculation Act include HRA- salwages formula
            //HRA transaction includes month rent rate, furniture etc
            //get total paid salary including arrears
            //get prsent salary structure
            //apply hra act metro or normal



            List<ItProjectionTempClass> OITProjectionDataList = new List<ItProjectionTempClass>();
            using (DataBaseContext db = new DataBaseContext())
            {



                DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };
                double HRA1, HRA2, HRA3, AcHRA1, AcHRA2, AcHRA3, ActSalAmt = 0, ProjSalAmt = 0;
                double TotRent = 0;
                HRAExemptionMaster OHRAEXE = null;

                EmployeePayroll OEmpHRATransT = db.EmployeePayroll.Where(e => e.Id == OEmployeePayroll_Id).FirstOrDefault();
                List<HRATransT> HRATransT = db.HRATransT.Where(e => e.EmployeePayroll_Id == OEmpHRATransT.Id).ToList();

                foreach (var HRATransTitem in HRATransT)
                {
                    List<HRAMonthRent> HRAMonthRent = db.HRAMonthRent.Where(e => e.HRATransT_Id == HRATransTitem.Id).ToList();
                    HRATransTitem.HRAMonthRent = HRAMonthRent;
                    Calendar Financialyear = db.Calendar.Where(e => e.Id == HRATransTitem.Financialyear_Id).SingleOrDefault();
                    HRATransTitem.Financialyear = Financialyear;
                    City City = db.City.Where(e => e.Id == HRATransTitem.City_Id).SingleOrDefault();
                    HRATransTitem.City = City;
                }
                OEmpHRATransT.HRATransT = HRATransT;
                List<ITSection10Payment> ITSection10Payment = db.ITSection10Payment.Where(e => e.EmployeePayroll_Id == OEmpHRATransT.Id).ToList();

                foreach (var ITSection10Paymentitem in ITSection10Payment)
                {
                    ITSection10 ITSection10 = db.ITSection10.Where(e => e.Id == ITSection10Paymentitem.ITSection10_Id).SingleOrDefault();
                    ITSection10Paymentitem.ITSection10 = ITSection10;
                    Calendar Financialyear = db.Calendar.Where(e => e.Id == ITSection10Paymentitem.FinancialYear_Id).SingleOrDefault();
                    ITSection10Paymentitem.FinancialYear = Financialyear;

                }
                OEmpHRATransT.ITSection10Payment = ITSection10Payment;
                //.Include(e => e.HRATransT)
                //.Include(e => e.ITSection10Payment)
                //.Include(e => e.ITSection10Payment.Select(eq => eq.FinancialYear))
                //.Include(e => e.ITSection10Payment.Select(q => q.ITSection10))
                //.Include(e => e.HRATransT.Select(eq => eq.HRAMonthRent))
                //.Include(e => e.HRATransT.Select(eq => eq.Financialyear))
                //.Include(e => e.HRATransT.Select(eq => eq.City))//.AsNoTracking()


                HRATransT OHRATransT = null;

                if (OEmpHRATransT.HRATransT != null)
                {
                    OHRATransT = OEmpHRATransT.HRATransT.Where(t => t.Financialyear.Id == OFinancialYear).FirstOrDefault();
                    if (OHRATransT == null || OHRATransT.City == null)
                    {
                        return OITProjectionDataList;
                    }
                }
                List<HRAExemptionMaster> HRAExemptionMaster = db.HRAExemptionMaster.Include(e => e.City).AsNoTracking().Where(q => q.City.Count > 0).ToList();
                foreach (HRAExemptionMaster OHRAMast in HRAExemptionMaster)
                {
                    City OCity = OHRAMast.City.Where(r => r.Id == OHRATransT.City.Id).SingleOrDefault();
                    if (OCity != null)
                    {
                        OHRAEXE = OHRAMast;
                        break;
                    }
                }

                List<HRAMonthRent> OHRAMon = OHRATransT.HRAMonthRent.ToList();

                EmpSalStruct OEmpSalStructData = db.EmpSalStruct.Where(e => e.EmployeePayroll.Id == OEmployeePayroll_Id).OrderByDescending(x => x.EffectiveDate).FirstOrDefault();
                List<EmpSalStructDetails> EmpSalStructDetails = db.EmpSalStructDetails.Where(e => e.EmpSalStruct_Id == OEmpSalStructData.Id).ToList();

                foreach (var EmpSalStructDetailsitem in EmpSalStructDetails)
                {
                    SalaryHead SalaryHead = db.SalaryHead.Where(e => e.Id == EmpSalStructDetailsitem.SalaryHead_Id).SingleOrDefault();
                    if (SalaryHead != null)
                    {
                        EmpSalStructDetailsitem.SalaryHead = SalaryHead;
                        SalHeadFormula SalHeadFormula = db.SalHeadFormula.Include(e => e.SalWages)
                            .Include(e => e.SalWages.RateMaster)
                            .Include(e => e.SalWages.RateMaster.Select(x => x.SalHead))
                            .Where(e => e.Id == EmpSalStructDetailsitem.SalHeadFormula_Id).SingleOrDefault();
                        EmpSalStructDetailsitem.SalHeadFormula = SalHeadFormula;
                        //if (SalHeadFormula != null)
                        //{

                        //    Wages SalWages = db.Wages.Where(e => e.Id == SalHeadFormula.SalWages_Id).SingleOrDefault();
                        //    if (SalWages != null)
                        //    {
                        //        EmpSalStructDetailsitem.SalHeadFormula.SalWages = SalWages;
                        //    }


                        //    List<RateMaster> RateMaster = db.Wages.Where(e => e.Id == SalHeadFormula.SalWages_Id).Select(e => e.RateMaster.ToList()).SingleOrDefault();
                        //    if (RateMaster != null)
                        //    {
                        //        foreach (var RateMasteritem in RateMaster)
                        //        {
                        //            SalaryHead SalHead = db.SalaryHead.Where(e => e.Id == RateMasteritem.SalHead_Id).SingleOrDefault();
                        //            RateMasteritem.SalHead = SalHead;
                        //        }

                        //        EmpSalStructDetailsitem.SalHeadFormula.SalWages.RateMaster = RateMaster;
                        //    }


                        //}
                    }


                }
                OEmpSalStructData.EmpSalStructDetails = EmpSalStructDetails;
                //.Include(e => e.EmpSalStructDetails)
                //.Include(e => e.EmpSalStructDetails.Select(r => r.SalaryHead))
                //.Include(e => e.EmpSalStructDetails.Select(r => r.SalHeadFormula))
                //.Include(e => e.EmpSalStructDetails.Select(r => r.SalHeadFormula.SalWages))
                //.Include(e => e.EmpSalStructDetails.Select(r => r.SalHeadFormula.SalWages.RateMaster))
                //.Include(e => e.EmpSalStructDetails.Select(r => r.SalHeadFormula.SalWages.RateMaster.Select(t => t.SalHead))).AsNoTracking()

                EmpSalStructDetails OSalStructDetails = OEmpSalStructData.EmpSalStructDetails.Where(r => r.SalaryHead.Code == "HRAEXEMPT").SingleOrDefault();

                EmployeePayroll OEmployeePayroll = db.EmployeePayroll.Find(OEmployeePayroll_Id);

                double SalAmount = Process.SalaryHeadGenProcess.SalHeadAmountCalc(OSalStructDetails.SalHeadFormula.Id, null, OEmpSalStructData.EmpSalStructDetails.ToList(), OEmployeePayroll,
                               OEmpSalStructData.EffectiveDate.Value.ToString("MM/yyyy"), OSalStructDetails.SalaryHead.Code.ToUpper() == "VPF" ? true : false);
                SalAmount = Process.SalaryHeadGenProcess.RoundingFunction(OSalStructDetails.SalaryHead, SalAmount);

                //ActSalAmt = SalAmount * Convert.ToDouble(mClosedMonths.Count());
                //ProjSalAmt = SalAmount * Convert.ToDouble(mBalMonths);

                HRA1 = OITSalMonthwise.Where(e => e.SalaryHead.Code == "HRA").ToList().Sum(a => a.ProjectedAmount);
                if (HRA1 < 0)
                {
                    HRA1 = 0;
                }
                AcHRA1 = OITSalMonthwise.Where(e => e.SalaryHead.Code == "HRA").ToList().Sum(a => a.ActualAmount);
                OHRATransT.ABasic = OITSalMonthwise.Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "BASIC").ToList().Sum(q => q.ProjectedAmount);// +OITSalMonthwise.Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "BASIC").ToList().Sum(q => q.ProjectedAmount);
                OHRATransT.AHRA = OITSalMonthwise.Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "HRA").ToList().Sum(q => q.ProjectedAmount); //+ +OITSalMonthwise.Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "HRA").ToList().Sum(q => q.ProjectedAmount);

                List<SalaryHead> OListOfSalaryHeadForHRA = OSalStructDetails.SalHeadFormula.SalWages.RateMaster.Select(r => r.SalHead).ToList();

                foreach (var item in OListOfSalaryHeadForHRA)
                {
                    ActSalAmt = ActSalAmt + OITSalMonthwise.Where(e => e.SalaryHead.Code == item.Code).ToList().Sum(a => a.ActualAmount);
                    ProjSalAmt = ProjSalAmt + OITSalMonthwise.Where(e => e.SalaryHead.Code == item.Code).ToList().Sum(a => a.ProjectedAmount);
                }

                OHRATransT.ASalary = ProjSalAmt;
                foreach (HRAMonthRent OHRAM in OHRAMon)
                {
                    double totalDays = (OHRAM.RentToDate.Value - OHRAM.RentFromDate.Value).Days;
                    double totalMonths = Math.Truncate(totalDays / 30);
                    TotRent = TotRent + (totalMonths * OHRAM.RentAmount);
                }
                // HRA2 = ((ProjSalAmt - TotRent) * OHRAEXE.RentPer) / 100;
                HRA2 = TotRent - Math.Round((ProjSalAmt * OHRAEXE.RentPer) / 100, 0);
                if (HRA2 < 0)
                {
                    HRA2 = 0;
                }
                HRA3 = (OHRATransT.ASalary * OHRAEXE.Ctypeper) / 100;
                if (HRA3 < 0)
                {
                    HRA3 = 0;
                }
                //edit hratranst
                // AcHRA2 = ((ActSalAmt - TotRent) * OHRAEXE.RentPer) / 100;
                AcHRA2 = TotRent - Math.Round((ActSalAmt * OHRAEXE.RentPer) / 100, 0);
                if (AcHRA2 < 0)
                {
                    AcHRA2 = 0;
                }
                AcHRA3 = (ActSalAmt * OHRAEXE.Ctypeper) / 100;


                OHRATransT.ExemptedHRA = Math.Min(HRA1, Math.Min(HRA2, HRA3));
                var TotalPaidActHRA = Math.Min(AcHRA1, Math.Min(AcHRA2, AcHRA3));
                OHRATransT.HRA1 = HRA1;
                OHRATransT.HRA2 = HRA2;
                OHRATransT.HRA3 = HRA3;
                OHRATransT.MonRentPaid = TotRent;
                OHRATransT.TaxableHRA = HRA1 - OHRATransT.ExemptedHRA;
                db.HRATransT.Attach(OHRATransT);
                db.Entry(OHRATransT).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                ITSection10Payment OEmpITSec10Obj = OEmpHRATransT.ITSection10Payment.Where(a => a.ITSection10.ExemptionCode == "HRAEXEMPT" && a.FinancialYear.Id == OFinancialYear).SingleOrDefault();
                if (OEmpITSec10Obj != null)
                {//7
                    OEmpITSec10Obj.ITSection = db.ITSection.Include(e => e.ITInvestments).Include(e => e.ITSection10)
                    .Include(e => e.ITSectionList)
                    .Include(e => e.ITSectionListType)
                    .Include(e => e.ITStandardITRebate)
                    .Where(r => r.Id == ItsectionIdForHra).SingleOrDefault();
                    OEmpITSec10Obj.ITSection10 = db.ITSection10.Include(q => q.Itsection10salhead).Where(a => a.Id == Itsection10BIdForHra).SingleOrDefault();
                    OEmpITSec10Obj.FinancialYear = db.Calendar.Where(a => a.Id == OFinancialYear).SingleOrDefault();
                    OEmpITSec10Obj.ActualInvestment = TotalPaidActHRA;
                    OEmpITSec10Obj.DeclaredInvestment = OHRATransT.ExemptedHRA;
                    OEmpITSec10Obj.InvestmentDate = DateTime.Now.Date;
                    OEmpITSec10Obj.Narration = "System Auto Pickup";
                    OEmpITSec10Obj.DBTrack = dbt;

                    db.Entry(OEmpITSec10Obj).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }//7

                //ITSection10Payment OEmpITSec10P = new ITSection10Payment()
                //{
                //    ITSection = db.ITSection.Include(e => e.ITInvestments).Include(e => e.ITSection10)
                //    .Include(e => e.ITSectionList)
                //    .Include(e => e.ITSectionListType)
                //    .Include(e => e.ITStandardITRebate)
                //    .Where(r => r.Id == ItsectionIdForHra).SingleOrDefault(),
                //    ITSection10 = db.ITSection10.Include(q => q.Itsection10salhead).Where(a => a.Id == Itsection10BIdForHra).SingleOrDefault(),
                //    FinancialYear = db.Calendar.Where(a => a.Id == OFinancialYear).SingleOrDefault(),

                //    ActualInvestment = TotalPaidActHRA,
                //    DeclaredInvestment = OHRATransT.ExemptedHRA,
                //    InvestmentDate = DateTime.Now.Date,
                //    Narration = "System Auto Pickup",
                //    DBTrack = dbt
                //    //UploadDocId = null
                //};

                //db.ITSection10Payment.Add(OEmpITSec10P);
                //db.SaveChanges();


                //EmployeePayroll aa = db.EmployeePayroll.Include(q => q.ITSection10Payment).Where(a => a.Id == OEmployeePayroll.Id).SingleOrDefault();
                //List<ITSection10Payment> tree = new List<ITSection10Payment>();
                //tree.Add(OEmpITSec10P);
                //if (aa.ITSection10Payment.Count > 0)
                //{
                //    tree.AddRange(aa.ITSection10Payment);
                //}

                //aa.ITSection10Payment = tree;
                //db.EmployeePayroll.Attach(aa);
                //db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                //db.SaveChanges();
                //OEmpITSec10Payment.Add(OEmpITSec10P);

                //        ItProjectionTempClass OITProjectionSaveTemp = new ItProjectionTempClass();
                double OActualQuali = 0;
                double OProjectedQuali = 0;
                if (OEmpITSec10Obj.ActualInvestment > OEmpITSec10Obj.ITSection10.MaxAmount)
                {
                    OActualQuali = OEmpITSec10Obj.ITSection10.MaxAmount;
                }
                else
                {
                    OActualQuali = OEmpITSec10Obj.ActualInvestment;
                }

                if (OEmpITSec10Obj.DeclaredInvestment > OEmpITSec10Obj.ITSection10.MaxAmount)
                {
                    OProjectedQuali = OEmpITSec10Obj.ITSection10.MaxAmount;
                }
                else
                {
                    OProjectedQuali = OEmpITSec10Obj.DeclaredInvestment;
                }
                OEmpITSec10Payment.Add(OEmpITSec10Obj);

                //        {
                //            OITProjectionSave.FinancialYear = OFinancialYear;
                //            OITProjectionSave.Tiltle = "";
                //            OITProjectionSave.FromPeriod = null;
                //            OITProjectionSave.ToPeriod = null;
                //            OITProjectionSave.Section = OEmpITSec10Payment[i].ITSection.ITSectionList.LookupVal.ToUpper();
                //            OITProjectionSave.SectionType = OEmpITSec10Payment[i].ITSection.ITSectionListType.LookupVal.ToUpper();
                //            OITProjectionSave.ChapterName = "    " + OEmpITSec10Payment[i].ITSection10.ExemptionCode;
                //            OITProjectionSave.ProjectedAmount = OEmpITSec10Payment[i].DeclaredInvestment;
                //            OITProjectionSave.ActualAmount = OEmpITSec10Payment[i].ActualInvestment;
                //            // OITProjectionSave.ActualQualifyingAmount = 0,//OEmpITSec10Payment[i].ActualInvestment,
                //            //  OITProjectionSave.ProjectedQualifyingAmount = 0,//OEmpITSec10Payment[i].DeclaredInvestment,



                //            //   ProjectionDate = DateTime.Now.Date,
                //            OITProjectionSave.TDSComponents = 0;
                //            OITProjectionSave.QualifiedAmount = 0;
                //            OITProjectionSave.Narration = "Exempted Income details under section10";
                //            //    DBTrack = dbt,
                //            OITProjectionSave.PickupId = 41;
                //        };
                //        OITProjectionDataList.Add(OITProjectionSave);
                //    }
                //}

                ItProjectionTempClass OITProjectionSave = new ItProjectionTempClass();

                {
                    OITProjectionSave.FinancialYear = OFinancialYear;
                    OITProjectionSave.Tiltle = "HRA";
                    OITProjectionSave.FromPeriod = mFromPeriod;
                    OITProjectionSave.ToPeriod = mToPerod;
                    //OITProjectionSave.Section = OEmpITSec10Payment[i].ITSection.ITSectionList.LookupVal.ToUpper();
                    //OITProjectionSave.SectionType = OEmpITSec10Payment[i].ITSection.ITSectionListType.LookupVal.ToUpper();
                    OITProjectionSave.ChapterName = "    " + "HRAEXEMPT";//OEmpITSec10Payment[i].ITSection10.ExemptionCode;
                    OITProjectionSave.SalayHead = OEmpSalStructData.EmpSalStructDetails.Where(r => r.SalaryHead.Code == "HRAEXEMPT").Select(Q => Q.SalaryHead.Id).FirstOrDefault();
                    OITProjectionSave.ProjectedAmount = OHRATransT.ExemptedHRA;
                    OITProjectionSave.ActualAmount = TotalPaidActHRA;
                    OITProjectionSave.ActualQualifyingAmount = OActualQuali;
                    OITProjectionSave.ProjectedQualifyingAmount = OProjectedQuali;
                    OITProjectionSave.TDSComponents = 0;
                    OITProjectionSave.QualifiedAmount = 0;
                    OITProjectionSave.Narration = "HRA";
                    OITProjectionSave.PickupId = 41;

                };
                OITProjectionDataList.Add(OITProjectionSave);

            }

            return OITProjectionDataList;
        }

        public static List<ItProjectionTempClass> StdDeduction(EmployeePayroll OEmployeePayroll,
            CompanyPayroll OCompanyPayroll, List<ITSalaryHeadData> OITSalMonthwise, Int32 OFinancialYear, string OLastMonth, DateTime mToPerod,
            DateTime mProcessDate)
        {
            //Utility.DumpProcessStatus("StdDeduction");
            using (DataBaseContext db = new DataBaseContext())
            {
                DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };

                //using (DataBaseContext db = new DataBaseContext())
                //{
                // for new tax slab start 28122020
                Calendar temp_OFinancialYear = db.Calendar.Find(OFinancialYear);
                DateTime finfrm = Convert.ToDateTime("01/04/2020");
                double std40ded = 0;
                double Actptx = 0;
                double Projptx = 0;
                // for new tax slab end 28122020
                List<ItProjectionTempClass> OITProjectionDataList = new List<ItProjectionTempClass>();

                var StdDed40 = db.ITSection.Where(e => e.ITSectionList.LookupVal.ToUpper() == "SECTION16(III)" && e.ITSectionListType.LookupVal.ToUpper() == "STDDED40K").SingleOrDefault();

                var Regimi = db.EmployeePayroll.Include(e => e.RegimiScheme).Include(e => e.RegimiScheme.Select(x => x.FinancialYear)).Include(e => e.RegimiScheme.Select(y => y.Scheme)).Where(e => e.Id == OEmployeePayroll.Id).FirstOrDefault();
                var Regimischemecurryear = Regimi.RegimiScheme.Where(e => e.FinancialYear.Id == temp_OFinancialYear.Id).FirstOrDefault();

                if (StdDed40 != null)
                {
                    // for new tax slab start 28122020
                    if (Regimischemecurryear != null)
                    {
                        if (Regimischemecurryear.Scheme.LookupVal.ToUpper() == "NEWTAX")
                        {
                            string requiredPaths = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\App_Data\\Menu_Json";
                            bool existss = System.IO.Directory.Exists(requiredPaths);
                            string localPaths;
                            if (!existss)
                            {
                                localPaths = new Uri(requiredPaths).LocalPath;
                                System.IO.Directory.CreateDirectory(localPaths);
                            }
                            string paths = requiredPaths + @"\StandardDedNewRegime" + ".ini";
                            localPaths = new Uri(paths).LocalPath;
                            if (!System.IO.File.Exists(localPaths))
                            {

                                using (var fs = new FileStream(localPaths, FileMode.OpenOrCreate))
                                {
                                    StreamWriter str = new StreamWriter(fs);
                                    str.BaseStream.Seek(0, SeekOrigin.Begin);

                                    str.Flush();
                                    str.Close();
                                    fs.Close();
                                }


                            }


                            string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\App_Data\\Menu_Json";
                            bool exists = System.IO.Directory.Exists(requiredPath);
                            string localPath;
                            if (!exists)
                            {
                                localPath = new Uri(requiredPath).LocalPath;
                                System.IO.Directory.CreateDirectory(localPath);
                            }
                            string path = requiredPath + @"\StandardDedNewRegime" + ".ini";
                            localPath = new Uri(path).LocalPath;
                            DateTime? FinancialYr = null;
                            string Amt = "";
                            using (var streamReader = new StreamReader(localPath))
                            {
                                string line;

                                while ((line = streamReader.ReadLine()) != null)
                                {
                                    var comp = line;
                                    FinancialYr = Convert.ToDateTime(comp.ToString().Split('_')[0]);
                                    Amt = comp.ToString().Split('_')[1];
                                }

                                if (temp_OFinancialYear.FromDate == FinancialYr)
                                {
                                    std40ded = Convert.ToDouble(Amt);
                                }
                                else
                                {
                                    std40ded = StdDed40.ExemptionLimit;
                                }
                            }
                        }
                        else
                        {
                            std40ded = StdDed40.ExemptionLimit;
                        }
                    }
                    else
                    {


                        if (temp_OFinancialYear.FromDate >= finfrm && OEmployeePayroll.Employee.EmpOffInfo.NationalityID.No2 == "Yes")
                        {
                            //  std40ded = 0;
                            string requiredPaths = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\App_Data\\Menu_Json";
                            bool existss = System.IO.Directory.Exists(requiredPaths);
                            string localPaths;
                            if (!existss)
                            {
                                localPaths = new Uri(requiredPaths).LocalPath;
                                System.IO.Directory.CreateDirectory(localPaths);
                            }
                            string paths = requiredPaths + @"\StandardDedNewRegime" + ".ini";
                            localPaths = new Uri(paths).LocalPath;
                            if (!System.IO.File.Exists(localPaths))
                            {

                                using (var fs = new FileStream(localPaths, FileMode.OpenOrCreate))
                                {
                                    StreamWriter str = new StreamWriter(fs);
                                    str.BaseStream.Seek(0, SeekOrigin.Begin);

                                    str.Flush();
                                    str.Close();
                                    fs.Close();
                                }


                            }


                            string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\App_Data\\Menu_Json";
                            bool exists = System.IO.Directory.Exists(requiredPath);
                            string localPath;
                            if (!exists)
                            {
                                localPath = new Uri(requiredPath).LocalPath;
                                System.IO.Directory.CreateDirectory(localPath);
                            }
                            string path = requiredPath + @"\StandardDedNewRegime" + ".ini";
                            localPath = new Uri(path).LocalPath;
                            DateTime? FinancialYr = null;
                            string Amt = "";
                            using (var streamReader = new StreamReader(localPath))
                            {
                                string line;

                                while ((line = streamReader.ReadLine()) != null)
                                {
                                    var comp = line;
                                    FinancialYr = Convert.ToDateTime(comp.ToString().Split('_')[0]);
                                    Amt = comp.ToString().Split('_')[1];
                                }

                                if (temp_OFinancialYear.FromDate == FinancialYr)
                                {
                                    std40ded = Convert.ToDouble(Amt);
                                }
                                else
                                {
                                    std40ded = StdDed40.ExemptionLimit;
                                }
                            }

                        }
                        else
                        {
                            std40ded = StdDed40.ExemptionLimit;
                        }
                    }
                    // for new tax slab end 28122020
                    ItProjectionTempClass OITProjectionSave52 = new ItProjectionTempClass
                    {
                        FinancialYear = OFinancialYear,
                        Tiltle = "",
                        FromPeriod = null,
                        ToPeriod = null,
                        Section = "",
                        SectionType = "",
                        ChapterName = "    STANDARD DEDUCTION",
                        // for new tax slab start 28122020
                        //ProjectedAmount = StdDed40.ExemptionLimit,
                        //ActualAmount = StdDed40.ExemptionLimit,
                        ProjectedAmount = std40ded,
                        ActualAmount = std40ded,
                        // for new tax slab end 28122020
                        ActualQualifyingAmount = 0,
                        ProjectedQualifyingAmount = 0,
                        //ProjectionDate = DateTime.Now.Date,
                        Narration = "Total Section16(iii)Std Deduction Details",
                        //   DBTrack = dbt,
                        PickupId = 52

                    };

                    OITProjectionDataList.Add(OITProjectionSave52);
                }
                //EmpSalStruct OEmpSalStruct = OEmployeePayroll.EmpSalStruct.Where(e => e.EndDate == null).SingleOrDefault();
                EmpSalStruct OEmpSalStruct = db.EmpSalStruct.Where(e => e.EmployeePayroll.Id == OEmployeePayroll.Id).OrderByDescending(e => e.Id).FirstOrDefault();
                List<EmpSalStructDetails> EmpSalStructDetails = db.EmpSalStructDetails.Include(e => e.SalaryHead).Where(e => e.EmpSalStruct_Id == OEmpSalStruct.Id).ToList();
                OEmpSalStruct.EmpSalStructDetails = EmpSalStructDetails;

                //foreach (var EmpSalStructDetailsitem in EmpSalStructDetails)
                //{
                //    SalaryHead SalaryHead = db.SalaryHead.Where(e => e.Id == EmpSalStructDetailsitem.SalaryHead_Id).SingleOrDefault();
                //    EmpSalStructDetailsitem.SalaryHead = SalaryHead;
                //    LookupValue SalHeadOperationType = db.LookupValue.Where(e => e.Id == SalaryHead.SalHeadOperationType_Id).SingleOrDefault();
                //    SalaryHead.SalHeadOperationType = SalHeadOperationType;
                //    LookupValue Frequency = db.LookupValue.Where(e => e.Id == SalaryHead.Frequency_Id).SingleOrDefault();
                //    SalaryHead.Frequency = Frequency;
                //}

                //.Include(e => e.EmpSalStructDetails).Include(e => e.EmpSalStructDetails.Select(r => r.SalaryHead))
                //.Include(e => e.EmpSalStructDetails.Select(r => r.SalaryHead.SalHeadOperationType))
                //.Include(e => e.EmpSalStructDetails.Select(r => r.SalaryHead.Frequency))
                if (OEmployeePayroll.Employee.EmpOffInfo.PTAppl == true && OEmpSalStruct != null) //pt applicable check
                {
                    EmpSalStruct OState = db.EmpSalStruct.Where(e => e.Id == OEmpSalStruct.Id).SingleOrDefault();
                    GeoStruct GeoStruct = db.GeoStruct.Where(e => e.Id == OState.GeoStruct_Id).SingleOrDefault();
                    OState.GeoStruct = GeoStruct;
                    Location Location = db.Location.Where(e => e.Id == GeoStruct.Location_Id).SingleOrDefault();
                    GeoStruct.Location = Location;
                    Address Address = db.Address.Where(e => e.Id == Location.Address_Id).SingleOrDefault();
                    Location.Address = Address;
                    State State = db.State.Where(e => e.Id == Address.State_Id).SingleOrDefault();
                    Address.State = State;


                    //.Include(e => e.GeoStruct)
                    //.Include(e => e.GeoStruct.Location)
                    //.Include(e => e.GeoStruct.Location.Address)
                    //.Include(e => e.GeoStruct.Location.Address.State)


                    State mState = OState.GeoStruct.Location.Address.State;
                    PTaxMaster mPTMaster1 = OCompanyPayroll.PTaxMaster
                    .Where(e => e.EffectiveDate < mProcessDate
                        && (e.EndDate >= mProcessDate || e.EndDate == null) && e.States.Id == mState.Id
                        ).SingleOrDefault();

                    //PTaxMaster OPTaxMaster = db.PTaxMaster
                    //                .Include(e => e.PTWagesMaster)
                    //                .Include(e => e.PTWagesMaster.RateMaster)
                    //                .Include(e => e.PTStatutoryEffectiveMonths)
                    //                .Include(e => e.PTStatutoryEffectiveMonths.Select(f => f.StatutoryWageRange))
                    //                .Include(e => e.PTStatutoryEffectiveMonths.Select(f => f.EffectiveMonth))
                    //                .Include(e => e.States)
                    //                .Include(e => e.Frequency)
                    //                .Where(e => e.Id == mPTMaster1.Id)
                    //                .SingleOrDefault();

                    PTaxMaster OPTaxMaster = db.PTaxMaster.Where(e => e.Id == mPTMaster1.Id).SingleOrDefault();
                    Wages PTWagesMaster = db.Wages.Where(e => e.Id == OPTaxMaster.PTWagesMaster_Id).SingleOrDefault();
                    OPTaxMaster.PTWagesMaster = PTWagesMaster;
                    List<RateMaster> RateMaster = db.Wages.Where(e => e.Id == OPTaxMaster.PTWagesMaster_Id).Select(e => e.RateMaster.ToList()).SingleOrDefault();
                    PTWagesMaster.RateMaster = RateMaster;
                    foreach (var RateMasteritem in RateMaster)
                    {
                        SalaryHead SalHead = db.SalaryHead.Where(e => e.Id == RateMasteritem.SalHead_Id).SingleOrDefault();
                        RateMasteritem.SalHead = SalHead;

                    }
                    List<StatutoryEffectiveMonths> PTStatutoryEffectiveMonths = db.PTaxMaster.Where(e => e.Id == mPTMaster1.Id).Select(e => e.PTStatutoryEffectiveMonths.ToList()).SingleOrDefault();
                    OPTaxMaster.PTStatutoryEffectiveMonths = PTStatutoryEffectiveMonths;
                    foreach (var PTStatutoryEffectiveMonthsitem in PTStatutoryEffectiveMonths)
                    {
                        List<Range> StatutoryWageRange = db.StatutoryEffectiveMonths.Where(e => e.Id == PTStatutoryEffectiveMonthsitem.Id).Select(e => e.StatutoryWageRange.ToList()).SingleOrDefault();
                        PTStatutoryEffectiveMonthsitem.StatutoryWageRange = StatutoryWageRange;
                        LookupValue EffectiveMonth = db.LookupValue.Where(e => e.Id == PTStatutoryEffectiveMonthsitem.EffectiveMonth_Id).SingleOrDefault();
                        PTStatutoryEffectiveMonthsitem.EffectiveMonth = EffectiveMonth;
                        LookupValue Gender1 = db.LookupValue.Where(e => e.Id == PTStatutoryEffectiveMonthsitem.Gender_Id).SingleOrDefault();
                        PTStatutoryEffectiveMonthsitem.Gender = Gender1;


                    }

                    State States = db.State.Where(e => e.Id == OPTaxMaster.States_Id).SingleOrDefault();
                    OPTaxMaster.States = States;
                    LookupValue Frequency = db.LookupValue.Where(e => e.Id == OPTaxMaster.Frequency_Id).SingleOrDefault();
                    OPTaxMaster.Frequency = Frequency;


                    //actual PT data
                    List<EmpSalStructDetails> OEmpSalStructDetails = OEmpSalStruct.EmpSalStructDetails.ToList();
                    //PT amount
                    //var OITSalaryDataPT = OITSalaryHeadData.Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "PTAX")
                    //                    .Sum(r => r.ActualAmount);
                    //call PTMaster to calculate balance month PT
                    PTaxMaster OPTMaster = OCompanyPayroll.PTaxMaster.Where(x => x.States.Id == mState.Id).OrderByDescending(e => e.EffectiveDate).SingleOrDefault();
                    List<StatutoryEffectiveMonths> mEffectiveMonthObj = OPTMaster.PTStatutoryEffectiveMonths
                                                .ToList();
                    double mPTWages = Process.SalaryHeadGenProcess.WagecalcDirect(OPTMaster.PTWagesMaster, null, OEmpSalStructDetails);


                    double mTotalPTForecast = 0;
                    //var mMonthPay = Convert.ToDateTime(mProcessDate).ToString("MMMM").ToUpper();
                    //var mEffectiveMonthObj = OPTaxMaster.PTStatutoryEffectiveMonths.Where(d => d.EffectiveMonth.LookupVal.ToUpper() == mMonthPay).SingleOrDefault();
                    //double mPTWages = Process.SalaryHeadGenProcess.WagecalcDirect(OPTaxMaster.PTWagesMaster, null, OEmpSalStruct.EmpSalStructDetails.ToList());

                    if (OPTaxMaster == null)
                    {
                        //ptax master not exist

                    }
                    else
                    {//PT logic
                        if (OPTaxMaster.Frequency.LookupVal.ToUpper() == "MONTHLY" && mEffectiveMonthObj != null)//monthly ptax calculation
                        {

                            DateTime mEndDate = Convert.ToDateTime("01/" + Convert.ToDateTime(mToPerod).ToString("MM/yyyy")).AddMonths(1).Date;
                            for (DateTime mdate = Convert.ToDateTime("01/" + OLastMonth).AddMonths(1).Date; mdate < mEndDate; mdate = mdate.AddMonths(1))
                            {
                                StatutoryEffectiveMonths mEffectiveMonthObjSel = mEffectiveMonthObj.Where(e => e.EffectiveMonth.LookupVal.ToString() ==
                                    mdate.ToString("MM/yyyy")).SingleOrDefault();
                                if (mEffectiveMonthObjSel != null)
                                {
                                    mTotalPTForecast = 0;
                                    foreach (Range ca1 in mEffectiveMonthObjSel.StatutoryWageRange)//check tomorrow
                                    {
                                        if (mPTWages >= ca1.RangeFrom && mPTWages <= ca1.RangeTo)
                                        {
                                            if (ca1.EmpShareAmount != null || ca1.EmpShareAmount != 0)
                                            {
                                                mTotalPTForecast = ca1.EmpShareAmount;
                                                break;
                                            }
                                            if (ca1.EmpSharePercentage != null || ca1.EmpSharePercentage != 0)
                                            {
                                                mTotalPTForecast = (mPTWages * ca1.EmpSharePercentage / 100);
                                                break;
                                            }

                                        }
                                    }
                                    ITSalaryHeadData OITSalaryHeadPTData = OITSalMonthwise.Where(e => e.PayMonth == mdate.ToString("MM/yyyy") &&
                                        e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "PTAX").SingleOrDefault();
                                    if (OITSalaryHeadPTData != null)
                                    {
                                        //var OITSalaryHeadData= db.ITSalaryHeadData.Find(OITSalaryHeadPTData.Id);
                                        OITSalaryHeadPTData.ProjectedAmount = OITSalaryHeadPTData.ActualAmount + mTotalPTForecast;
                                        // db.Entry(OITSalaryHeadPTData).State = System.Data.Entity.EntityState.Modified;
                                        // db.SaveChanges();
                                    }

                                }

                            }
                        }
                    }
                    {
                        List<ITSalaryHeadData> OITSalaryHeadPTDataTotalTemp = OITSalMonthwise.Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "PTAX").ToList();
                        //var OITSalaryHeadPTDataTotal = OITSalaryHeadPTDataTotalTemp
                        //    .GroupBy(r => r.SalaryHead)
                        //    .Select(r => new {Salary=r.SalaryHead,  mActITIncome = r.Sum(t => t.ActualAmount), mProjITIncome = r.Sum(t => t.ProjectedAmount) })
                        //.SingleOrDefault();
                        var OITSalaryHeadPTDataTotal = OITSalaryHeadPTDataTotalTemp
                            .GroupBy(r => r.SalaryHead)
                            .Select(r => new { mSalary = r.Key, mActITIncome = r.Sum(t => t.ActualAmount), mProjITIncome = r.Sum(t => t.ProjectedAmount) })
                        .SingleOrDefault();


                        if (OITSalaryHeadPTDataTotal != null)
                        {
                            // for new tax slab start 28122020
                            if (Regimischemecurryear != null)
                            {
                                if (Regimischemecurryear.Scheme.LookupVal.ToUpper() == "NEWTAX")
                                {
                                    Actptx = 0;
                                    Projptx = 0;
                                }
                                else
                                {
                                    Actptx = OITSalaryHeadPTDataTotal.mActITIncome;
                                    Projptx = OITSalaryHeadPTDataTotal.mProjITIncome;
                                }
                            }
                            else
                            {


                                if (temp_OFinancialYear.FromDate >= finfrm && OEmployeePayroll.Employee.EmpOffInfo.NationalityID.No2 == "Yes")
                                {
                                    Actptx = 0;
                                    Projptx = 0;
                                }
                                else
                                {
                                    Actptx = OITSalaryHeadPTDataTotal.mActITIncome;
                                    Projptx = OITSalaryHeadPTDataTotal.mProjITIncome;
                                }
                            }
                            // for new tax slab end 28122020

                            ItProjectionTempClass OITProjectionSave = new ItProjectionTempClass
                            {
                                FinancialYear = OFinancialYear,
                                Tiltle = "",
                                FromPeriod = null,
                                ToPeriod = null,
                                Section = "",
                                SectionType = "",
                                ChapterName = "    " + OITSalaryHeadPTDataTotal.mSalary.Name,
                                // for new tax slab start 28122020
                                //ProjectedAmount = OITSalaryHeadPTDataTotal.mProjITIncome,
                                //ActualAmount = OITSalaryHeadPTDataTotal.mActITIncome,
                                //ActualQualifyingAmount = OITSalaryHeadPTDataTotal.mActITIncome,
                                //ProjectedQualifyingAmount = OITSalaryHeadPTDataTotal.mProjITIncome,
                                ProjectedAmount = Projptx,
                                ActualAmount = Actptx,
                                ActualQualifyingAmount = Actptx,
                                ProjectedQualifyingAmount = Projptx,
                                // for new tax slab end 28122020
                                //  ProjectionDate = DateTime.Now.Date,
                                TDSComponents = 0,
                                QualifiedAmount = 0,
                                Narration = "Total Section16(iii)PTax Details",
                                //   DBTrack = dbt,
                                PickupId = 51
                            };

                            OITProjectionDataList.Add(OITProjectionSave);
                        }


                    }

                }
                else//ptax not appl(hadicapped)
                {


                    ItProjectionTempClass OITProjectionSave = new ItProjectionTempClass
                    {
                        FinancialYear = OFinancialYear,
                        Tiltle = "",
                        FromPeriod = null,
                        ToPeriod = null,
                        Section = "",
                        SectionType = "",
                        ChapterName = "    " + db.SalaryHead.Include(e => e.SalHeadOperationType).Where(e => e.SalHeadOperationType.LookupVal.ToUpper() == "PTAX").Select(e => e.Name).SingleOrDefault(),

                        ProjectedAmount = 0,
                        ActualAmount = 0,
                        ActualQualifyingAmount = 0,
                        ProjectedQualifyingAmount = 0,
                        //  ProjectionDate = DateTime.Now.Date,
                        TDSComponents = 0,
                        QualifiedAmount = 0,
                        Narration = "Total Section16(iii)PTax Details",
                        //   DBTrack = dbt,
                        PickupId = 51
                    };

                    OITProjectionDataList.Add(OITProjectionSave);

                }
                return OITProjectionDataList;
                //}
            }
        }
        public static List<ItProjectionTempClass> Investment80C(EmployeePayroll OEmployeePayroll, IncomeTax OITMaster,
            List<ITSalaryHeadData> OITSalMonthwise, Int32 OFinancialYear, string OLastMonth, SalaryT OSalaryTC, int Flag, int ProcType)
        {
            // Utility.DumpProcessStatus("Investment80C");
            //using (DataBaseContext db = new DataBaseContext())
            //{
            List<ItProjectionTempClass> OITProjectionDataList = new List<ItProjectionTempClass>();
            using (DataBaseContext db = new DataBaseContext())
            {
                DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };

                //  var OITSection = OITMaster.ITSection.ToList();
                var Regimi = db.EmployeePayroll.Include(e => e.RegimiScheme).Include(e => e.RegimiScheme.Select(x => x.FinancialYear)).Include(e => e.RegimiScheme.Select(y => y.Scheme)).Where(e => e.Id == OEmployeePayroll.Id).FirstOrDefault();
                var Regimischemecurryear = Regimi.RegimiScheme.Where(e => e.FinancialYear.Id == OFinancialYear).FirstOrDefault();

                ITSection OITSection80C = OITMaster.ITSection.Where(e => e.ITSectionList.LookupVal.ToUpper() == "SECTION80C" &&
                    e.ITSectionListType.LookupVal.ToUpper() == "REBATE").SingleOrDefault(); //rebate

                List<ITInvestment> OITInvestments = OITSection80C.ITInvestments.ToList();
                List<LoanAdvanceHead> OITInvestmentsLoan = OITSection80C.LoanAdvanceHead.ToList();
                List<ITSubInvestment> OITSubInvestments = OITInvestments.SelectMany(e => e.ITSubInvestment).ToList();
                //ITSubInvestment
                IList<ITInvestmentPayment> OEmpITInvestment = null;
                EmployeePayroll OEmpInvest = db.EmployeePayroll.Where(e => e.Id == OEmployeePayroll.Id).SingleOrDefault();
                List<ITInvestmentPayment> ITInvestmentPayment = db.ITInvestmentPayment.Where(e => e.EmployeePayroll_Id == OEmpInvest.Id && e.FinancialYear_Id == OFinancialYear).ToList();//modofied on 26/10/2022 by prashant for fy

                foreach (var ITInvestmentPaymentitem in ITInvestmentPayment)
                {
                    Calendar FinancialYear = db.Calendar.Where(e => e.Id == ITInvestmentPaymentitem.FinancialYear_Id).SingleOrDefault();
                    ITInvestmentPaymentitem.FinancialYear = FinancialYear;
                    ITSection ITSection = db.ITSection.Include(x => x.LoanAdvanceHead).Where(e => e.Id == ITInvestmentPaymentitem.ITSection_Id).SingleOrDefault();
                    LookupValue ITSectionList = db.LookupValue.Where(e => e.Id == ITSection.ITSectionList_Id).SingleOrDefault();
                    ITSection.ITSectionList = ITSectionList;
                    LookupValue ITSectionListType = db.LookupValue.Where(e => e.Id == ITSection.ITSectionListType_Id).SingleOrDefault();
                    ITSection.ITSectionListType = ITSectionListType;
                    ITInvestmentPaymentitem.ITSection = ITSection;
                    List<ITSubInvestmentPayment> ITSubInvestmentPayment = db.ITInvestmentPayment.Where(e => e.Id == ITInvestmentPaymentitem.ITInvestment_Id).Select(e => e.ITSubInvestmentPayment.ToList()).SingleOrDefault();
                    if (ITSubInvestmentPayment != null)
                    {
                        ITInvestmentPaymentitem.ITSubInvestmentPayment = ITSubInvestmentPayment;

                    }
                    LoanAdvanceHead LoanAdvanceHead = db.LoanAdvanceHead.Where(e => e.Id == ITInvestmentPaymentitem.LoanAdvanceHead_Id).SingleOrDefault();

                    if (LoanAdvanceHead != null)
                    {
                        ITInvestmentPaymentitem.LoanAdvanceHead = LoanAdvanceHead;
                        ITLoan ITLoan = db.ITLoan.Where(e => e.Id == LoanAdvanceHead.ITLoan_Id).SingleOrDefault();
                        ITInvestmentPaymentitem.LoanAdvanceHead.ITLoan = ITLoan;

                        SalaryHead SalaryHead = db.SalaryHead.Where(e => e.Id == LoanAdvanceHead.SalaryHead_Id).SingleOrDefault();
                        ITInvestmentPaymentitem.LoanAdvanceHead.SalaryHead = SalaryHead;
                    }
                    List<ITSection> LoanITSection = db.LoanAdvanceHead.Where(e => e.Id == ITInvestmentPaymentitem.LoanAdvanceHead_Id).Select(e => e.ITSection.Where(x=>x.Id==ITSection.Id).ToList()).SingleOrDefault();
                    if (LoanITSection != null)
                    {
                        ITInvestmentPaymentitem.LoanAdvanceHead.ITSection = LoanITSection;

                    }

                    //List<LoanAdvancePolicy> LoanAdvancePolicy = db.LoanAdvanceHead.Where(e => e.Id == ITInvestmentPaymentitem.LoanAdvanceHead_Id).Select(e => e.LoanAdvancePolicy.ToList()).SingleOrDefault();
                    //if (LoanAdvancePolicy != null)
                    //{

                    //    ITInvestmentPaymentitem.LoanAdvanceHead.LoanAdvancePolicy = LoanAdvancePolicy;
                    //}

                    ITInvestment ITInvestment = db.ITInvestment.Where(e => e.Id == ITInvestmentPaymentitem.ITInvestment_Id).SingleOrDefault();
                    if (ITInvestment != null)
                    {
                        ITInvestmentPaymentitem.ITInvestment = ITInvestment;
                        SalaryHead InvestSalaryHead = db.SalaryHead.Where(e => e.Id == ITInvestment.SalaryHead_Id).SingleOrDefault();
                        ITInvestmentPaymentitem.ITInvestment.SalaryHead = InvestSalaryHead;
                    }



                }
                OEmpInvest.ITInvestmentPayment = ITInvestmentPayment;
                //.Include(e => e.ITInvestmentPayment)
                //.Include(e => e.ITInvestmentPayment.Select(r => r.FinancialYear))
                //.Include(e => e.ITInvestmentPayment.Select(r => r.ITSection))
                //.Include(e => e.ITInvestmentPayment.Select(r => r.ITSection.ITSectionList))
                //.Include(e => e.ITInvestmentPayment.Select(r => r.ITSection.ITSectionListType))
                //.Include(e => e.ITInvestmentPayment.Select(r => r.ITSection.ITSectionList))//added 17042017 by prashant
                //.Include(e => e.ITInvestmentPayment.Select(r => r.ITSubInvestmentPayment))
                //.Include(e => e.ITInvestmentPayment.Select(r => r.LoanAdvanceHead))
                //.Include(e => e.ITInvestmentPayment.Select(r => r.LoanAdvanceHead.ITLoan))
                //.Include(e => e.ITInvestmentPayment.Select(r => r.LoanAdvanceHead.ITSection))
                //.Include(e => e.ITInvestmentPayment.Select(r => r.LoanAdvanceHead.LoanAdvancePolicy))
                //.Include(e => e.ITInvestmentPayment.Select(r => r.LoanAdvanceHead.SalaryHead))
                //.Include(e => e.ITInvestmentPayment.Select(r => r.ITInvestment))
                //.Include(e => e.ITInvestmentPayment.Select(r => r.ITInvestment.SalaryHead))



                if (OEmpInvest.ITInvestmentPayment != null)
                {
                    OEmpITInvestment = OEmpInvest.ITInvestmentPayment.Where(e => e.FinancialYear.Id == OFinancialYear && e.ITSection.ITSectionListType.LookupVal.ToUpper() == "REBATE").ToList();
                    List<ITSubInvestmentPayment> OEmpITSubInvestment = OEmpInvest.ITInvestmentPayment
                        .Where(e => e.FinancialYear.Id == OFinancialYear && e.ITSubInvestmentPayment != null && e.ITSection.ITSectionListType.LookupVal.ToUpper() == "REBATE")
                        .SelectMany(e => e.ITSubInvestmentPayment).ToList();
                }
                //var OEmpSal = OEmployeePayroll.SalaryT.Where(e => e.PayMonth.Contains(mClosedMonths)).Select(r => r.SalEarnDedT.Where(q => q.SalaryHead.InITax == true && q.SalaryHead.Type.LookupVal.ToUpper() == "DEDUCTIONS")).ToList();
                //var OEmpITSec10Payment = OEmployeePayroll.ITSection10Payment.Where(e => e.FinancialYear == OFinancialYear).ToList();
                //check depend on loan
                Calendar temp_OFinancialYear = db.Calendar.Where(e => e.Id == OFinancialYear).SingleOrDefault();
                double mTotActInvst = 0;
                double mTotDeclInvst = 0;
                double mTotActQualify = 0;
                double mTotDeclQualify = 0;
                var OITInvestmentsSalLoan = OITInvestmentsLoan.Where(e => e.SalaryHead != null && e.ITLoan != null && (e.ITLoan.PrincAppl == true ||
                    e.ITLoan.IntPrincAppl == true)).Select(e => new { Id = e.Id, SalaryHead_Id = e.SalaryHead.Id }).ToList();

                EmployeePayroll OEmpPay = db.EmployeePayroll.Where(e => e.Id == OEmployeePayroll.Id).Include(e => e.LoanAdvRequest.Select(r => r.LoanAdvRepaymentT))
                    .Include(e => e.LoanAdvRequest).Include(e => e.LoanAdvRequest.Select(r => r.LoanAdvanceHead)).SingleOrDefault();
                var Id = Convert.ToInt32(SessionManager.CompanyId);
                string _CompCode = db.Company.Where(e => e.Id == Id).SingleOrDefault().Code.ToUpper();

                if (OITInvestmentsSalLoan != null && OITInvestmentsSalLoan.Count() > 0)
                {
                    List<ITInvestmentPayment> mList = new List<ITInvestmentPayment>();
                    foreach (var ca in OITInvestmentsSalLoan)
                    {
                        if (OEmpPay.LoanAdvRequest != null)
                        {
                            List<LoanAdvRequest> OLoanAdvReqF = OEmpPay.LoanAdvRequest.Where(e => e.LoanAdvanceHead.Id == ca.Id).ToList();
                            //mTotActInvst = OITSalMonthwise.Where(r => r.SalaryHead.Id == ca.SalaryHead_Id)
                            //                   .Sum(m => m.ActualAmount);
                            foreach (LoanAdvRequest ca1 in OLoanAdvReqF) //interest calculation
                            {
                                DateTime mLoanEndDate = temp_OFinancialYear.ToDate.Value;
                                if (ca1.CloserDate < mLoanEndDate)
                                {
                                    mLoanEndDate = ca1.CloserDate.Value;
                                }

                                mTotActInvst = OLoanAdvReqF.SelectMany(r => r.LoanAdvRepaymentT.Where(t => t.InstallementDate >= temp_OFinancialYear.FromDate && t.InstallementDate <= mLoanEndDate && t.RepaymentDate != null)).Sum(e => e.MonthlyPricipalAmount);
                                if (_CompCode == "KDCC")
                                {
                                    mTotDeclInvst = OLoanAdvReqF.SelectMany(r => r.LoanAdvRepaymentT.Where(t => t.InstallementDate >= temp_OFinancialYear.FromDate && t.InstallementDate <= mLoanEndDate && t.RepaymentDate == null)).Sum(e => e.MonthlyPricipalAmount);
                                }
                                else
                                {

                                    var mTotActInvstpro = OLoanAdvReqF.SelectMany(r => r.LoanAdvRepaymentT.OrderBy(x => x.InstallementDate).Where(t => t.InstallementDate >= temp_OFinancialYear.FromDate && t.InstallementDate <= mLoanEndDate && t.RepaymentDate != null)).LastOrDefault();
                                    if (mTotActInvstpro != null)
                                    {
                                        double mBalMonths = (mLoanEndDate.Month + mLoanEndDate.Year * 12) -
                                            (Convert.ToDateTime("01/" + OLastMonth).Month + Convert.ToDateTime("01/" + OLastMonth).Year * 12) - 1;
                                        //mTotDeclInvst = (ca1.MonthlyPricipalAmount * mBalMonths);
                                        if (mBalMonths < 0)
                                        {
                                            mBalMonths = 0;
                                        }
                                        mTotDeclInvst = (mTotActInvstpro.MonthlyPricipalAmount * mBalMonths);
                                    }

                                }
                            }
                            if (mTotActInvst != 0)
                            {
                                mTotDeclInvst = mTotDeclInvst + mTotActInvst;
                            }


                            if ((ProcType == 1 || ProcType == 2) && mTotActInvst != 0)
                            {
                                mTotActInvst = mTotDeclInvst;
                            }
                            // for new tax slab start 28122020
                            DateTime finfrm = Convert.ToDateTime("01/04/2020");
                            if (Regimischemecurryear != null)
                            {
                                if (Regimischemecurryear.Scheme.LookupVal.ToUpper() == "NEWTAX")
                                {
                                    mTotActInvst = 0;
                                    mTotDeclInvst = 0;
                                }
                            }
                            else
                            {

                                if (temp_OFinancialYear.FromDate >= finfrm && OEmployeePayroll.Employee.EmpOffInfo.NationalityID.No2 == "Yes")
                                {
                                    mTotActInvst = 0;
                                    mTotDeclInvst = 0;
                                }
                            }
                            // for new tax slab end 28122020

                            if (OLoanAdvReqF.Count() > 0 && mTotActInvst != 0)
                            {
                                //modify investment record
                                //    var OEmpITInvestmentTemp1 = OEmpITInvestment.Where(e => e.LoanAdvanceHead != null).ToList();
                                //   var OEmpITInvestmentTemp = OEmpITInvestment.Where(e => e.LoanAdvanceHead != null && e.LoanAdvanceHead.SalaryHead != null).ToList();
                                ITInvestmentPayment OEmpSalInvestmentChk = OEmpITInvestment.Where(e => e.LoanAdvanceHead != null && e.LoanAdvanceHead.SalaryHead != null &&
                                    e.LoanAdvanceHead.SalaryHead.Id == ca.SalaryHead_Id).SingleOrDefault();
                                if (OEmpSalInvestmentChk != null)
                                {
                                    ITInvestmentPayment OEmpSalInvestmentObj = db.ITInvestmentPayment.Where(a => a.Id == OEmpSalInvestmentChk.Id).SingleOrDefault();
                                    OEmpSalInvestmentObj.ActualInvestment = mTotActInvst;
                                    OEmpSalInvestmentObj.DeclaredInvestment = mTotDeclInvst;
                                    db.Entry(OEmpSalInvestmentObj).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                }
                                else
                                {
                                    ITInvestmentPayment OEmpITInvestmentSave = new ITInvestmentPayment()
                                    {
                                        FinancialYear = db.Calendar.Where(e => e.Id == OFinancialYear).SingleOrDefault(),
                                        LoanAdvanceHead = db.LoanAdvanceHead.Where(a => a.Id == ca.Id).SingleOrDefault(),
                                        InvestmentDate = DateTime.Now.Date,
                                        ITSection = db.ITSection.Include(q => q.ITSectionListType).Include(q => q.ITSectionList).Include(q => q.ITSection10).Include(q => q.LoanAdvanceHead).Where(a => a.Id == OITSection80C.Id).SingleOrDefault(),
                                        ITSubInvestmentPayment = null,
                                        ActualInvestment = mTotActInvst,
                                        DeclaredInvestment = mTotDeclInvst,
                                        Narration = "Investment under Section 80C Loan through Salary",
                                        DBTrack = dbt,
                                    };

                                    if (Flag == 1)
                                    {
                                        db.ITInvestmentPayment.Add(OEmpITInvestmentSave);
                                        db.SaveChanges();
                                    }

                                    mList.Add(OEmpITInvestmentSave);
                                    OEmpITInvestment.Add(OEmpITInvestmentSave);

                                }
                            }
                        }

                    }
                    if (mList.Count() > 0 && Flag == 1)
                    {
                        int OEmpId = OEmployeePayroll.Id;
                        List<ITInvestmentPayment> OFAT = new List<ITInvestmentPayment>();
                        //EmployeePayroll aa = db.EmployeePayroll.Include(e => e.ITInvestmentPayment).Where(a => a.Id == OEmpId).SingleOrDefault();

                        EmployeePayroll aa = db.EmployeePayroll.Find(OEmployeePayroll.Id);
                        if (aa.ITInvestmentPayment != null)
                        { mList.AddRange(aa.ITInvestmentPayment); }

                        aa.ITInvestmentPayment = mList;
                        //OEmployeePayroll.DBTrack = dbt;
                        db.EmployeePayroll.Attach(aa);
                        db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        //db.Entry(aa).State = System.Data.Entity.EntityState.Detached;

                    }


                }


                //check for salary depend investments
                //double mTotActInvst = 0;
                //double mTotDeclInvst = 0;
                //double mTotActQualify = 0;
                //double mTotDeclQualify = 0;
                var OITInvestmentsSal = OITInvestments.Where(e => e.IsSalaryHead == true).Select(a => new { Id = a.Id, SalaryHead_Id = a.SalaryHead.Id }).ToList();
                if (OITInvestmentsSal != null && OEmpITInvestment != null)
                {
                    List<ITInvestmentPayment> mList = new List<ITInvestmentPayment>();
                    foreach (var ca in OITInvestmentsSal)
                    {
                        mTotActInvst = OITSalMonthwise.Where(r => r.SalaryHead.Id == ca.SalaryHead_Id)
                                        .Sum(m => m.ActualAmount);

                        mTotDeclInvst = OITSalMonthwise.Where(r => r.SalaryHead.Id == ca.SalaryHead_Id)
                                        .Sum(m => m.ProjectedAmount);

                        if (ProcType == 1 || ProcType == 2)
                        {
                            mTotActInvst = mTotDeclInvst;
                        }

                        // for new tax slab start 28122020
                        DateTime finfrm = Convert.ToDateTime("01/04/2020");
                        if (Regimischemecurryear != null)
                        {
                            if (Regimischemecurryear.Scheme.LookupVal.ToUpper() == "NEWTAX")
                            {
                                mTotActInvst = 0;
                                mTotDeclInvst = 0;
                            }
                        }
                        else
                        {


                            if (temp_OFinancialYear.FromDate >= finfrm && OEmployeePayroll.Employee.EmpOffInfo.NationalityID.No2 == "Yes")
                            {
                                mTotActInvst = 0;
                                mTotDeclInvst = 0;
                            }
                        }
                        // for new tax slab end 28122020

                        //modify investment record
                        // var OEmpITInvestmentTemp1 = OEmpITInvestment.Where(e => ).ToList();
                        //var OEmpITInvestmentTemp = OEmpITInvestmentTemp1.Where(e => ).ToList();
                        ITInvestmentPayment OEmpSalInvestmentChk = OEmpITInvestment.Where(e => e.ITInvestment != null && e.ITInvestment.SalaryHead != null &&
                            e.ITInvestment.SalaryHead.Id == ca.SalaryHead_Id).SingleOrDefault();
                        if (OEmpSalInvestmentChk != null)
                        {
                            ITInvestmentPayment OEmpSalInvestmentObj = db.ITInvestmentPayment.Where(a => a.Id == OEmpSalInvestmentChk.Id).SingleOrDefault();
                            OEmpSalInvestmentObj.ActualInvestment = mTotActInvst;
                            OEmpSalInvestmentObj.DeclaredInvestment = mTotDeclInvst;

                            if (Flag == 1)
                            {
                                db.ITInvestmentPayment.Attach(OEmpSalInvestmentObj);
                                db.Entry(OEmpSalInvestmentObj).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                            }
                            //mList.Add(OEmpSalInvestmentObj);
                        }
                        else
                        {
                            //var OITSectiondata = OITSection80C.ITInvestments.First(ca.SingleOrDefault();
                            ITInvestmentPayment OEmpITInvestmentSave = new ITInvestmentPayment()
                            {
                                FinancialYear = db.Calendar.Where(e => e.Id == OFinancialYear).SingleOrDefault(),
                                ITInvestment = db.ITInvestment.Where(a => a.Id == ca.Id).SingleOrDefault(),
                                InvestmentDate = DateTime.Now.Date,
                                ITSection = db.ITSection.Include(q => q.ITSectionList).Where(a => a.Id == OITSection80C.Id).SingleOrDefault(),
                                ITSubInvestmentPayment = null,
                                ActualInvestment = mTotActInvst,
                                DeclaredInvestment = mTotDeclInvst,
                                Narration = "Investment under Section 80C through Salary",
                                DBTrack = dbt
                            };
                            if (Flag == 1)
                            {
                                db.ITInvestmentPayment.Add(OEmpITInvestmentSave);
                                db.SaveChanges();
                            }

                            mList.Add(OEmpITInvestmentSave);

                            OEmpITInvestment.Add(OEmpITInvestmentSave);
                        }

                    }
                    if (mList.Count() > 0 && Flag == 1)
                    {
                        EmployeePayroll aa = db.EmployeePayroll.Where(a => a.Id == OEmployeePayroll.Id).Include(e => e.ITInvestmentPayment).SingleOrDefault();
                        if (aa.ITInvestmentPayment != null)
                        { mList.AddRange(aa.ITInvestmentPayment); }
                        aa.ITInvestmentPayment = mList;
                        //OEmployeePayroll.DBTrack = dbt;
                        db.EmployeePayroll.Attach(aa);
                        db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                    }
                }

                //check all investments
                if (OEmpITInvestment.Count > 0)
                {
                    List<ITInvestmentPayment> ONormalInvestment = OEmpITInvestment.ToList();

                    //double mTotActSubInvst=0;
                    //double mTotDeclSubInvst=0;
                    mTotActInvst = 0;
                    mTotDeclInvst = 0;
                    mTotActQualify = 0;
                    mTotDeclQualify = 0;
                    if (ONormalInvestment.Count > 0)
                    {
                        foreach (ITInvestmentPayment ca in ONormalInvestment)
                        {
                            if (ca.ITSubInvestmentPayment != null && ca.ITSubInvestmentPayment.Count() > 0)
                            {
                                mTotActInvst = ca.ITSubInvestmentPayment.Sum(e => e.ActualInvestment);
                                mTotDeclInvst = ca.ITSubInvestmentPayment.Sum(e => e.DeclaredInvestment);
                                if (mTotActInvst > ca.ITInvestment.MaxAmount)
                                {
                                    mTotActQualify = ca.ITInvestment.MaxAmount;
                                }
                                else
                                {
                                    mTotActQualify = mTotActInvst;
                                }
                                if (mTotDeclInvst > ca.ITInvestment.MaxAmount)
                                {
                                    mTotDeclQualify = ca.ITInvestment.MaxAmount;
                                }
                                else
                                {
                                    mTotDeclQualify = mTotActInvst;
                                }
                            }
                            else
                            {
                                mTotActInvst = ca.ActualInvestment;
                                mTotDeclInvst = ca.DeclaredInvestment;
                                if (ca.ITInvestment == null)
                                {
                                    mTotActQualify = mTotActInvst;
                                    mTotDeclQualify = mTotDeclInvst;
                                }
                                else
                                {
                                    if (mTotActInvst > ca.ITInvestment.MaxAmount)
                                    {
                                        mTotActQualify = ca.ITInvestment.MaxAmount;
                                    }
                                    else
                                    {
                                        mTotActQualify = mTotActInvst;
                                    }
                                    if (mTotDeclInvst > ca.ITInvestment.MaxAmount)
                                    {
                                        mTotDeclQualify = ca.ITInvestment.MaxAmount;
                                    }
                                    else
                                    {
                                        mTotDeclQualify = mTotDeclInvst;
                                    }
                                }

                            }
                            // for new tax slab start 28122020
                            DateTime finfrm = Convert.ToDateTime("01/04/2020");
                            if (Regimischemecurryear != null)
                            {
                                if (Regimischemecurryear.Scheme.LookupVal.ToUpper() == "NEWTAX")
                                {
                                    mTotActInvst = 0;
                                    mTotDeclInvst = 0;
                                    mTotActQualify = 0;
                                    mTotDeclQualify = 0;
                                }
                            }
                            else
                            {


                                if (temp_OFinancialYear.FromDate >= finfrm && OEmployeePayroll.Employee.EmpOffInfo.NationalityID.No2 == "Yes")
                                {
                                    mTotActInvst = 0;
                                    mTotDeclInvst = 0;
                                    mTotActQualify = 0;
                                    mTotDeclQualify = 0;
                                }
                            }
                            // for new tax slab end 28122020

                            ItProjectionTempClass OITProjectionSave = new ItProjectionTempClass
                            {
                                FinancialYear = OFinancialYear,
                                Tiltle = "",
                                // Section = "",
                                Section = ca.ITSection != null && ca.ITSection.ITSectionList != null ? ca.ITSection.ITSectionList.LookupVal.ToUpper() : null,
                                ProjectedAmount = mTotDeclInvst,
                                ActualAmount = mTotActInvst,
                                ActualQualifyingAmount = mTotActQualify,
                                ProjectedQualifyingAmount = mTotDeclQualify,

                                TDSComponents = 0,
                                QualifiedAmount = 0,
                                Narration = "Investment details under section 80C",

                                PickupId = 92
                            };

                            if (ca.ITInvestment == null)
                            {
                                OITProjectionSave.ChapterName = "    " + ca.LoanAdvanceHead.Name;
                            }
                            else
                            {
                                OITProjectionSave.ChapterName = "    " + ca.ITInvestment.ITInvestmentName;
                            }
                            //add list of it projection data
                            OITProjectionDataList.Add(OITProjectionSave);
                        }
                    }
                }
                var OITSectio80CTotal = OITProjectionDataList.Where(e => e.PickupId == 92)//section80C
                    .GroupBy(e => e.PickupId)
                    .Select(r => new
                    {
                        ProjectedAmount = r.Sum(t => t.ProjectedAmount),
                        ActualAmount = r.Sum(t => t.ActualAmount),
                        ActualQualifyingAmount = r.Sum(t => t.ActualQualifyingAmount),
                        ProjectedQualifyingAmount = r.Sum(t => t.ProjectedQualifyingAmount)
                    }).SingleOrDefault();
                double mTotAct = 0;
                double mTotProj = 0;
                double mSection80CQualifiedAmountAct = 0;
                double mSection80CQualifiedAmountProj = 0;
                if (OITSectio80CTotal != null)
                {
                    mTotAct = OITSectio80CTotal.ActualAmount;
                    mTotProj = OITSectio80CTotal.ProjectedAmount;
                    mSection80CQualifiedAmountAct = 0;
                    mSection80CQualifiedAmountProj = 0;
                    if (OITSectio80CTotal.ActualQualifyingAmount > OITSection80C.ExemptionLimit)
                    {
                        mSection80CQualifiedAmountAct = OITSection80C.ExemptionLimit;
                    }
                    else
                    {
                        mSection80CQualifiedAmountAct = OITSectio80CTotal.ActualQualifyingAmount;
                    }
                    if (OITSectio80CTotal.ProjectedQualifyingAmount > OITSection80C.ExemptionLimit)
                    {
                        mSection80CQualifiedAmountProj = OITSection80C.ExemptionLimit;
                    }
                    else
                    {
                        mSection80CQualifiedAmountProj = OITSectio80CTotal.ProjectedQualifyingAmount;
                    }

                }

                {
                    ItProjectionTempClass OITProjectionSave = new ItProjectionTempClass
                    {
                        FinancialYear = OFinancialYear,
                        Tiltle = "",
                        Section = "",
                        //SectionType = ca.ITInvestment.ITInvestmentName,
                        ChapterName = "Total of Investment under Section80C",

                        ProjectedAmount = mTotProj,
                        ActualAmount = mTotAct,
                        ActualQualifyingAmount = mSection80CQualifiedAmountAct,
                        ProjectedQualifyingAmount = mSection80CQualifiedAmountProj,
                        // ProjectionDate = DateTime.Now.Date,
                        TDSComponents = 0,
                        QualifiedAmount = 0,
                        Narration = "Total of Investment under Section80C",
                        //   DBTrack = dbt,
                        PickupId = 93
                    };
                    OITProjectionDataList.Add(OITProjectionSave);
                }

                return OITProjectionDataList;
                //}
            }
        }

        public static List<ItProjectionTempClass> Investment80DtoU(double sec80GActGross, double sec80GporGross, EmployeePayroll OEmployeePayroll,
            IncomeTax OITMaster, List<ITSalaryHeadData> OITSalMonthwise, Int32 OFinancialYear, SalaryT OSalaryTC, int Flag)
        {
            //Utility.DumpProcessStatus("Investment80DtoU");
            //using (DataBaseContext db = new DataBaseContext())
            using (DataBaseContext db = new DataBaseContext())
            {      //{
                var Regimi = db.EmployeePayroll.Include(e => e.RegimiScheme).Include(e => e.RegimiScheme.Select(x => x.FinancialYear)).Include(e => e.RegimiScheme.Select(y => y.Scheme)).Where(e => e.Id == OEmployeePayroll.Id).FirstOrDefault();
                var Regimischemecurryear = Regimi.RegimiScheme.Where(e => e.FinancialYear.Id == OFinancialYear).FirstOrDefault();

                DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };
                List<ItProjectionTempClass> OITProjectionDataList = new List<ItProjectionTempClass>();
                // var OITSection = OITMaster.ITSection.ToList();

                List<ITSection> OITSection80DU = OITMaster.ITSection.Where(e => e.ITSectionListType.LookupVal.ToUpper() == "DEDUCT").ToList();

                List<ITInvestment> OITInvestments = OITMaster.ITSection.Where(e => e.ITSectionListType.LookupVal.ToUpper() == "DEDUCT")
                    .SelectMany(e => e.ITInvestments).ToList();
                List<ITSubInvestment> OITSubInvestments = OITInvestments.SelectMany(e => e.ITSubInvestment).ToList();

                IList<ITInvestmentPayment> OEmpITInvestment = null;
                EmployeePayroll OEmpInvest = db.EmployeePayroll.Where(e => e.Id == OEmployeePayroll.Id).SingleOrDefault();
                List<ITInvestmentPayment> ITInvestmentPayment = db.ITInvestmentPayment.Where(e => e.EmployeePayroll_Id == OEmpInvest.Id && e.FinancialYear_Id == OFinancialYear).ToList();//modofied on 26/10/2022 by prashant for fy

                foreach (var ITInvestmentPaymentitem in ITInvestmentPayment)
                {
                    Calendar FinancialYear = db.Calendar.Where(e => e.Id == ITInvestmentPaymentitem.FinancialYear_Id).SingleOrDefault();
                    ITInvestmentPaymentitem.FinancialYear = FinancialYear;
                    ITSection ITSection = db.ITSection.Include(x => x.LoanAdvanceHead).Where(e => e.Id == ITInvestmentPaymentitem.ITSection_Id).SingleOrDefault();
                    if (ITSection != null)
                    {

                        ITInvestmentPaymentitem.ITSection = ITSection;
                    }
                    LookupValue ITSectionList = db.LookupValue.Where(e => e.Id == ITSection.ITSectionList_Id).SingleOrDefault();
                    if (ITSectionList != null)
                    {
                        ITInvestmentPaymentitem.ITSection.ITSectionList = ITSectionList;

                    }
                    LookupValue ITSectionListType = db.LookupValue.Where(e => e.Id == ITSection.ITSectionListType_Id).SingleOrDefault();
                    if (ITSectionListType != null)
                    {
                        ITInvestmentPaymentitem.ITSection.ITSectionListType = ITSectionListType;

                    }
                    List<ITSubInvestmentPayment> ITSubInvestmentPayment = db.ITInvestmentPayment.Where(e => e.Id == ITInvestmentPaymentitem.ITInvestment_Id).Select(e => e.ITSubInvestmentPayment.ToList()).SingleOrDefault();
                    if (ITSubInvestmentPayment != null)
                    {

                        ITInvestmentPaymentitem.ITSubInvestmentPayment = ITSubInvestmentPayment;
                    }
                    LoanAdvanceHead LoanAdvanceHead = db.LoanAdvanceHead.Where(e => e.Id == ITInvestmentPaymentitem.LoanAdvanceHead_Id).SingleOrDefault();
                    if (LoanAdvanceHead != null)
                    {
                        ITInvestmentPaymentitem.LoanAdvanceHead = LoanAdvanceHead;
                        ITLoan ITLoan = db.ITLoan.Where(e => e.Id == LoanAdvanceHead.ITLoan_Id).SingleOrDefault();
                        ITInvestmentPaymentitem.LoanAdvanceHead.ITLoan = ITLoan;
                        SalaryHead SalaryHead = db.SalaryHead.Where(e => e.Id == LoanAdvanceHead.SalaryHead_Id).SingleOrDefault();
                        ITInvestmentPaymentitem.LoanAdvanceHead.SalaryHead = SalaryHead;
                    }

                    List<ITSection> LoanITSection = db.LoanAdvanceHead.Where(e => e.Id == ITInvestmentPaymentitem.LoanAdvanceHead_Id).Select(e => e.ITSection.Where(x => x.Id == ITSection.Id).ToList()).SingleOrDefault();
                    if (LoanITSection != null)
                    {

                        ITInvestmentPaymentitem.LoanAdvanceHead.ITSection = LoanITSection;
                    }
                    List<LoanAdvancePolicy> LoanAdvancePolicy = db.LoanAdvanceHead.Where(e => e.Id == ITInvestmentPaymentitem.LoanAdvanceHead_Id).Select(e => e.LoanAdvancePolicy.ToList()).SingleOrDefault();
                    if (LoanAdvancePolicy != null)
                    {

                        ITInvestmentPaymentitem.LoanAdvanceHead.LoanAdvancePolicy = LoanAdvancePolicy;
                    }

                    ITInvestment ITInvestment = db.ITInvestment.Where(e => e.Id == ITInvestmentPaymentitem.ITInvestment_Id).SingleOrDefault();
                    if (ITInvestment != null)
                    {

                        ITInvestmentPaymentitem.ITInvestment = ITInvestment;
                        SalaryHead InvestSalaryHead = db.SalaryHead.Where(e => e.Id == ITInvestment.SalaryHead_Id).SingleOrDefault();
                        ITInvestmentPaymentitem.ITInvestment.SalaryHead = InvestSalaryHead;

                    }



                }

                OEmpInvest.ITInvestmentPayment = ITInvestmentPayment;
                //.Include(e => e.ITInvestmentPayment)
                //.Include(e => e.ITInvestmentPayment.Select(r => r.FinancialYear))
                //.Include(e => e.ITInvestmentPayment.Select(r => r.ITSection))
                //.Include(e => e.ITInvestmentPayment.Select(r => r.ITSection.ITSectionList))
                //.Include(e => e.ITInvestmentPayment.Select(r => r.ITSection.ITSectionListType))
                //.Include(e => e.ITInvestmentPayment.Select(r => r.ITSection.ITSectionList))//added 17042017 by prashant
                //.Include(e => e.ITInvestmentPayment.Select(r => r.ITSubInvestmentPayment))
                //.Include(e => e.ITInvestmentPayment.Select(r => r.LoanAdvanceHead))
                //.Include(e => e.ITInvestmentPayment.Select(r => r.LoanAdvanceHead.ITLoan))
                //.Include(e => e.ITInvestmentPayment.Select(r => r.LoanAdvanceHead.ITSection))
                //.Include(e => e.ITInvestmentPayment.Select(r => r.LoanAdvanceHead.LoanAdvancePolicy))
                //.Include(e => e.ITInvestmentPayment.Select(r => r.LoanAdvanceHead.SalaryHead))
                //.Include(e => e.ITInvestmentPayment.Select(r => r.ITInvestment))
                //.Include(e => e.ITInvestmentPayment.Select(r => r.ITInvestment.SalaryHead))

                if (OEmpInvest.ITInvestmentPayment != null)
                {
                    OEmpITInvestment = OEmpInvest.ITInvestmentPayment.Where(e => e.FinancialYear.Id == OFinancialYear &&
                        e.ITSection.ITSectionListType.LookupVal.ToUpper() == "DEDUCT").ToList();
                    //List<ITSubInvestmentPayment> OEmpITSubInvestment = OEmpInvest.ITInvestmentPayment
                    //    .Where(e => e.FinancialYear.Id == OFinancialYear && e.ITSection.ITSectionListType.LookupVal.ToUpper() == "DEDUCT")
                    //    .SelectMany(e => e.ITSubInvestmentPayment).ToList();
                }
                //var OEmpSal = OEmployeePayroll.SalaryT.Where(e => e.PayMonth.Contains(mClosedMonths)).Select(r => r.SalEarnDedT.Where(q => q.SalaryHead.InITax == true && q.SalaryHead.Type.LookupVal.ToUpper() == "DEDUCTIONS")).ToList();

                //check for salary depend investments
                double mTotActInvst = 0;
                double mTotDeclInvst = 0;
                double mTotActQualify = 0;
                double mTotDeclQualify = 0;
                List<ITInvestment> OITInvestmentsSal = OITInvestments.Where(e => e.IsSalaryHead == true).ToList();
                if (OITInvestmentsSal != null && OITInvestmentsSal.Count() > 0)
                {
                    List<ITInvestmentPayment> mList = new List<ITInvestmentPayment>();
                    foreach (ITInvestment ca in OITInvestmentsSal)
                    {
                        //foreach (var ca1 in ca)
                        //{
                        mTotActInvst = OITSalMonthwise.Where(r => r.SalaryHead.Id == ca.SalaryHead.Id)
                                       .Sum(m => m.ActualAmount);

                        mTotDeclInvst = OITSalMonthwise.Where(r => r.SalaryHead.Id == ca.SalaryHead.Id)
                                        .Sum(m => m.ProjectedAmount);

                        // for new tax slab start 28122020
                        Calendar temp_OFinancialYear = db.Calendar.Where(e => e.Id == OFinancialYear).SingleOrDefault();
                        DateTime finfrm = Convert.ToDateTime("01/04/2020");
                        if (Regimischemecurryear != null)
                        {
                            if (Regimischemecurryear.Scheme.LookupVal.ToUpper() == "NEWTAX")
                            {
                                mTotActInvst = 0;
                                mTotDeclInvst = 0;
                            }
                        }
                        else
                        {


                            if (temp_OFinancialYear.FromDate >= finfrm && OEmployeePayroll.Employee.EmpOffInfo.NationalityID.No2 == "Yes")
                            {
                                mTotActInvst = 0;
                                mTotDeclInvst = 0;
                            }
                        }
                        // for new tax slab end 28122020


                        //modify investment record
                        //List<ITInvestmentPayment> OEmpITInvestmentTemp = OEmpITInvestment.Where(e => ).ToList();
                        ITInvestmentPayment OEmpSalInvestmentChk = OEmpITInvestment.Where(e => e.ITInvestment.SalaryHead != null && e.ITInvestment.SalaryHead.Id == ca.SalaryHead.Id).SingleOrDefault();
                        if (OEmpSalInvestmentChk != null)
                        {
                            ITInvestmentPayment OEmpSalInvestmentObj = db.ITInvestmentPayment.Where(a => a.Id == OEmpSalInvestmentChk.Id).SingleOrDefault();
                            OEmpSalInvestmentObj.ActualInvestment = mTotActInvst;
                            OEmpSalInvestmentObj.DeclaredInvestment = mTotDeclInvst;
                            if (Flag == 1)
                            {
                                db.Entry(OEmpSalInvestmentObj).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                            }
                            //mList.Add(OEmpSalInvestmentObj);

                        }
                        else
                        {
                            ITSection OITSectiondata = OITSection80DU.Where(e => e.ITInvestments == ca).SingleOrDefault();
                            ITInvestmentPayment OEmpITInvestmentSave = new ITInvestmentPayment()
                            {
                                FinancialYear = db.Calendar.Where(e => e.Id == OFinancialYear).SingleOrDefault(),
                                ITInvestment = db.ITInvestment.Where(a => a.Id == ca.Id).SingleOrDefault(),
                                InvestmentDate = DateTime.Now.Date,
                                ITSection = db.ITSection.Where(a => a.Id == ca.ITSection_Id).SingleOrDefault(),
                                ITSubInvestmentPayment = null,
                                ActualInvestment = mTotActInvst,
                                DeclaredInvestment = mTotDeclInvst,
                                Narration = "Investment under Section 80D to 80U through Salary",
                                DBTrack = dbt

                            };

                            if (Flag == 1)
                            {
                                db.ITInvestmentPayment.Add(OEmpITInvestmentSave);
                                db.SaveChanges();
                            }

                            mList.Add(OEmpITInvestmentSave);

                        }
                        // }
                    }
                    if (mList.Count() > 0 && Flag == 1)
                    {
                        int OEmpId = OEmployeePayroll.Id;

                        EmployeePayroll aa = db.EmployeePayroll.Where(a => a.Id == OEmpId).SingleOrDefault();
                        if (aa.ITInvestmentPayment != null)
                        { mList.AddRange(aa.ITInvestmentPayment); }
                        aa.ITInvestmentPayment = mList;
                        //OEmployeePayroll.DBTrack = dbt;
                        db.EmployeePayroll.Attach(aa);
                        db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        //db.Entry(aa).State = System.Data.Entity.EntityState.Detached;

                    }


                }

                //check all investments
                //  var ONormalInvestment = OEmpITInvestment.ToList();
                //double mTotActSubInvst=0;
                //double mTotDeclSubInvst=0;






                mTotActInvst = 0;
                mTotDeclInvst = 0;
                mTotActQualify = 0;
                mTotDeclQualify = 0;

                IList<ITInvestmentPayment> ONormalInvestment = null;

                // var ONormalInvestment_temp = null;

                if (OEmpITInvestment != null)
                {
                    //var ONormalInvestment_temp = OEmpITInvestment.ToList().GroupBy(z => z.ITSection.ITSectionList.Id).SelectMany(a => a.Select(z => z.ITInvestment.Id)).ToList();
                    var ONormalInvestment_temp = OEmpITInvestment.ToList().GroupBy(z => z.ITSection.ITSectionList.Id)

                        .SelectMany(a => a.Select(z => z.ITSection.ITSectionList.Id)).ToList();
                    var ONormalInvestment_temp1 = ONormalInvestment_temp.Distinct().ToArray();
                    //  ONormalInvestmentgr3.Select(a=>a.s)
                    //ONormalInvestment_temp

                    if (ONormalInvestment_temp1 != null)
                    {
                        foreach (var gr in ONormalInvestment_temp1)
                        {
                            double m_secmaxexemptionact = 0;
                            double m_secmaxexemptionQual = 0;

                            ONormalInvestment = OEmpITInvestment.Where(a => a.ITSection.ITSectionList.Id == gr).ToList();

                            m_secmaxexemptionact = ONormalInvestment.Select(x => x.ITSection.ExemptionLimit).FirstOrDefault();
                            m_secmaxexemptionQual = m_secmaxexemptionact;

                            if (ONormalInvestment != null)
                            {
                                foreach (ITInvestmentPayment ca in ONormalInvestment)
                                {
                                    //  ONormalInvestment = OEmpITInvestment.Where(a => a.ITInvestment.Id == gr).ToList();
                                    // double m_secmaxexemptionact = 0;
                                    //  double m_secmaxexemptionQual = 0;
                                    //    m_secmaxexemptionact = ONormalInvestment.Select(x => x.ITSection.ExemptionLimit).FirstOrDefault();
                                    // m_secmaxexemptionact = ca.ITSection.ExemptionLimit;
                                    // m_secmaxexemptionQual = m_secmaxexemptionact;
                                    if (ca.ITSubInvestmentPayment != null && ca.ITSubInvestmentPayment.Count() > 0)
                                    {
                                        mTotActInvst = ca.ITSubInvestmentPayment.Sum(e => e.ActualInvestment);
                                        mTotDeclInvst = ca.ITSubInvestmentPayment.Sum(e => e.DeclaredInvestment);


                                        if (mTotActInvst > ca.ITInvestment.MaxAmount)
                                        {
                                            mTotActQualify = ca.ITInvestment.MaxAmount;
                                        }
                                        else
                                        {
                                            mTotActQualify = mTotActInvst;
                                        }
                                        if (mTotDeclInvst > ca.ITInvestment.MaxAmount)
                                        {
                                            mTotDeclQualify = ca.ITInvestment.MaxAmount;
                                        }
                                        else
                                        {
                                            mTotDeclQualify = mTotDeclInvst;
                                        }




                                    }
                                    else
                                    {
                                        mTotActInvst = ca.ActualInvestment;
                                        mTotDeclInvst = ca.DeclaredInvestment;
                                        string OITSection80G = ca.ITSection.ITSectionList.LookupVal.ToUpper();
                                        // Section 80G start
                                        if (OITSection80G == "SECTION80G")
                                        {
                                            if (ca.ITInvestment != null)
                                            {
                                                if (ca.ITInvestment.MaxPercentage != 0)
                                                {
                                                    // 100%
                                                    if (ca.ITInvestment.MaxPercentage == 100)
                                                    {
                                                        if (mTotActInvst > Math.Round((sec80GActGross / 10), 0))
                                                        {
                                                            mTotActQualify = Math.Round((sec80GActGross / 10), 0);
                                                        }
                                                        else
                                                        {
                                                            mTotActQualify = mTotActInvst;
                                                        }
                                                        if (mTotDeclInvst > Math.Round((sec80GporGross / 10), 0))
                                                        {
                                                            mTotDeclQualify = Math.Round((sec80GporGross / 10), 0);
                                                        }
                                                        else
                                                        {
                                                            mTotDeclQualify = mTotDeclInvst;
                                                        }


                                                    }
                                                    //100 % end
                                                    //50 % start
                                                    else if (ca.ITInvestment.MaxPercentage == 50)
                                                    {
                                                        if (Math.Round((mTotActInvst / 2), 0) > Math.Round((sec80GActGross / 10), 0))
                                                        {
                                                            mTotActQualify = Math.Round((sec80GActGross / 10), 0);
                                                        }
                                                        else
                                                        {
                                                            mTotActQualify = Math.Round((mTotActInvst / 2), 0);
                                                        }
                                                        if (Math.Round((mTotDeclInvst / 2), 0) > Math.Round((sec80GporGross / 10), 0))
                                                        {
                                                            mTotDeclQualify = Math.Round((sec80GporGross / 10), 0);
                                                        }
                                                        else
                                                        {
                                                            mTotDeclQualify = Math.Round((mTotDeclInvst / 2), 0);
                                                        }
                                                        mTotActInvst = mTotActQualify;
                                                        mTotDeclInvst = mTotDeclQualify;

                                                    }

                                                    // 50% end


                                                }
                                                //= FULL start
                                                if (ca.ITInvestment.MaxPercentage == 0)
                                                {
                                                    if (mTotActInvst > sec80GActGross)
                                                    {
                                                        mTotActQualify = sec80GActGross;
                                                    }
                                                    else
                                                    {
                                                        mTotActQualify = mTotActInvst;
                                                    }
                                                    if (mTotDeclInvst > sec80GporGross)
                                                    {
                                                        mTotDeclQualify = sec80GporGross;
                                                    }
                                                    else
                                                    {
                                                        mTotDeclQualify = mTotDeclInvst;
                                                    }
                                                }
                                                //= FULL end
                                            }
                                        }
                                        // Section 80G End

                                        else
                                        {

                                            if (ca.ITInvestment != null)
                                            {
                                                if (mTotActInvst > ca.ITInvestment.MaxAmount)
                                                {
                                                    mTotActQualify = ca.ITInvestment.MaxAmount;
                                                }
                                                else
                                                {
                                                    mTotActQualify = mTotActInvst;
                                                }
                                                if (mTotDeclInvst > ca.ITInvestment.MaxAmount)
                                                {
                                                    mTotDeclQualify = ca.ITInvestment.MaxAmount;
                                                }
                                                else
                                                {
                                                    mTotDeclQualify = mTotDeclInvst;
                                                }

                                                // section celling start
                                                if (m_secmaxexemptionact > mTotActQualify)
                                                {
                                                    mTotActQualify = mTotActQualify;
                                                    m_secmaxexemptionact = m_secmaxexemptionact - mTotActQualify;
                                                }
                                                else
                                                {
                                                    mTotActQualify = m_secmaxexemptionact;
                                                    m_secmaxexemptionact = m_secmaxexemptionact - m_secmaxexemptionact;
                                                }

                                                if (m_secmaxexemptionQual > mTotDeclQualify)
                                                {
                                                    mTotDeclQualify = mTotDeclQualify;
                                                    m_secmaxexemptionQual = m_secmaxexemptionQual - mTotActQualify;
                                                }
                                                else
                                                {
                                                    mTotDeclQualify = m_secmaxexemptionQual;
                                                    m_secmaxexemptionQual = m_secmaxexemptionQual - m_secmaxexemptionQual;
                                                }

                                                // section celling end

                                            }
                                        }
                                    }
                                    // for new tax slab start 28122020
                                    DateTime finfrm = Convert.ToDateTime("01/04/2020");
                                    Calendar temp_OFinancialYear = db.Calendar.Where(e => e.Id == OFinancialYear).SingleOrDefault();
                                    if (Regimischemecurryear != null)
                                    {
                                        if (Regimischemecurryear.Scheme.LookupVal.ToUpper() == "NEWTAX")
                                        {
                                            mTotActInvst = 0;
                                            mTotDeclInvst = 0;
                                            mTotActQualify = 0;
                                            mTotDeclQualify = 0;

                                        }
                                    }
                                    else
                                    {

                                        if (temp_OFinancialYear.FromDate >= finfrm && OEmployeePayroll.Employee.EmpOffInfo.NationalityID.No2 == "Yes")
                                        {
                                            mTotActInvst = 0;
                                            mTotDeclInvst = 0;
                                            mTotActQualify = 0;
                                            mTotDeclQualify = 0;
                                        }
                                    }
                                    // for new tax slab end 28122020

                                    ItProjectionTempClass OITProjectionSave = new ItProjectionTempClass
                                    {
                                        FinancialYear = OFinancialYear,
                                        Tiltle = "",

                                        Section = ca.ITSection != null && ca.ITSection.ITSectionList != null ? ca.ITSection.ITSectionList.LookupVal.ToUpper() : null,
                                        SectionType = "",
                                        ChapterName = ca.ITInvestment != null ? "    " + ca.ITInvestment.ITInvestmentName : null,

                                        ProjectedAmount = mTotDeclInvst,
                                        ActualAmount = mTotActInvst,
                                        ActualQualifyingAmount = mTotActQualify,
                                        ProjectedQualifyingAmount = mTotDeclQualify,
                                        TDSComponents = 0,
                                        QualifiedAmount = 0,
                                        Narration = "Investment under section 80D to 80U",
                                        PickupId = 111
                                    };
                                    OITProjectionDataList.Add(OITProjectionSave);
                                    //add list of it projection data
                                }


                            }

                        }   //Sure
                    }//Sure
                }
                //25012017 writing the total and section exemption process- modification continue
                var OITSectio80DUTotal = OITProjectionDataList.Where(e => e.PickupId == 111)
                    .GroupBy(e => e.PickupId)
                    .Select(r => new
                    {
                        ProjectedAmount = r.Sum(t => t.ProjectedAmount),
                        ActualAmount = r.Sum(t => t.ActualAmount),
                        ActualQualifyingAmount = r.Sum(t => t.ActualQualifyingAmount),
                        ProjectedQualifyingAmount = r.Sum(t => t.ProjectedQualifyingAmount)
                    })
                    .SingleOrDefault();
                double mTotAct = 0;
                double mTotProj = 0;
                double mTotQAct = 0;
                double mTotQProj = 0;
                if (OITSectio80DUTotal != null)
                {
                    mTotAct = OITSectio80DUTotal.ActualAmount;
                    mTotProj = OITSectio80DUTotal.ProjectedAmount;
                    mTotQAct = OITSectio80DUTotal.ActualQualifyingAmount;
                    mTotQProj = OITSectio80DUTotal.ProjectedQualifyingAmount;
                }

                {
                    ItProjectionTempClass OITProjectionSave = new ItProjectionTempClass
                    {
                        FinancialYear = OFinancialYear,
                        Tiltle = "",
                        Section = "",
                        SectionType = "",
                        ChapterName = "Total of Investments under Section80D-U",

                        ProjectedAmount = mTotProj,
                        ActualAmount = mTotAct,
                        ActualQualifyingAmount = mTotQAct,
                        ProjectedQualifyingAmount = mTotQProj,
                        //ProjectionDate = DateTime.Now.Date,

                        Narration = "Total of Investment under section 80D-U",
                        //   DBTrack = dbt,
                        PickupId = 112
                    };
                    OITProjectionDataList.Add(OITProjectionSave);
                }


                return OITProjectionDataList;
                //}
            }
        }
        public static List<ItProjectionTempClass> InvestmentRebatNot80C(EmployeePayroll OEmployeePayroll, IncomeTax OITMaster,
            List<ITSalaryHeadData> OITSalMonthwise, Int32 OFinancialYear, SalaryT OSalaryTC, int Flag)
        {
            //Utility.DumpProcessStatus("InvestmentRebatNot80C");
            using (DataBaseContext db = new DataBaseContext())
            {
                var Regimi = db.EmployeePayroll.Include(e => e.RegimiScheme).Include(e => e.RegimiScheme.Select(x => x.FinancialYear)).Include(e => e.RegimiScheme.Select(y => y.Scheme)).Where(e => e.Id == OEmployeePayroll.Id).FirstOrDefault();
                var Regimischemecurryear = Regimi.RegimiScheme.Where(e => e.FinancialYear.Id == OFinancialYear).FirstOrDefault();
                List<ItProjectionTempClass> OITProjectionDataList = new List<ItProjectionTempClass>();
                DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };
                //using (DataBaseContext db = new DataBaseContext())
                //{
                //   var OITSection = OITMaster.ITSection.ToList();

                List<ITSection> OITSection80CCC = OITMaster.ITSection.Where(e => e.ITSectionListType.LookupVal.ToUpper() == "EXTRAREBATE").ToList(); //rebate
                List<ITInvestment> OITInvestments = OITSection80CCC.SelectMany(e => e.ITInvestments).ToList();
                List<ITSubInvestment> OITSubInvestments = OITInvestments.SelectMany(e => e.ITSubInvestment).ToList();

                IList<ITInvestmentPayment> OEmpITInvestment = null;
                EmployeePayroll OEmpInvest = db.EmployeePayroll.Where(e => e.Id == OEmployeePayroll.Id).SingleOrDefault();
                List<ITInvestmentPayment> ITInvestmentPayment = db.ITInvestmentPayment.Where(e => e.EmployeePayroll_Id == OEmpInvest.Id && e.FinancialYear_Id == OFinancialYear).ToList();//modofied on 26/10/2022 by prashant for fy

                foreach (var ITInvestmentPaymentitem in ITInvestmentPayment)
                {
                    Calendar FinancialYear = db.Calendar.Where(e => e.Id == ITInvestmentPaymentitem.FinancialYear_Id).SingleOrDefault();
                    ITInvestmentPaymentitem.FinancialYear = FinancialYear;
                    ITSection ITSection = db.ITSection.Include(x => x.LoanAdvanceHead).Where(e => e.Id == ITInvestmentPaymentitem.ITSection_Id).SingleOrDefault();
                    ITInvestmentPaymentitem.ITSection = ITSection;
                    LookupValue ITSectionList = db.LookupValue.Where(e => e.Id == ITSection.ITSectionList_Id).SingleOrDefault();
                    ITInvestmentPaymentitem.ITSection.ITSectionList = ITSectionList;
                    LookupValue ITSectionListType = db.LookupValue.Where(e => e.Id == ITSection.ITSectionListType_Id).SingleOrDefault();
                    ITInvestmentPaymentitem.ITSection.ITSectionListType = ITSectionListType;
                    List<ITSubInvestmentPayment> ITSubInvestmentPayment = db.ITInvestmentPayment.Where(e => e.Id == ITInvestmentPaymentitem.ITInvestment_Id).Select(e => e.ITSubInvestmentPayment.ToList()).SingleOrDefault();
                    ITInvestmentPaymentitem.ITSubInvestmentPayment = ITSubInvestmentPayment;
                    LoanAdvanceHead LoanAdvanceHead = db.LoanAdvanceHead.Where(e => e.Id == ITInvestmentPaymentitem.LoanAdvanceHead_Id).SingleOrDefault();
                    if (LoanAdvanceHead != null)
                    {
                        ITInvestmentPaymentitem.LoanAdvanceHead = LoanAdvanceHead;
                        ITLoan ITLoan = db.ITLoan.Where(e => e.Id == LoanAdvanceHead.ITLoan_Id).SingleOrDefault();
                        ITInvestmentPaymentitem.LoanAdvanceHead.ITLoan = ITLoan;
                        SalaryHead SalaryHead = db.SalaryHead.Where(e => e.Id == LoanAdvanceHead.SalaryHead_Id).SingleOrDefault();
                        ITInvestmentPaymentitem.LoanAdvanceHead.SalaryHead = SalaryHead;
                    }

                    List<ITSection> LoanITSection = db.LoanAdvanceHead.Where(e => e.Id == ITInvestmentPaymentitem.LoanAdvanceHead_Id).Select(e => e.ITSection.Where(x => x.Id == ITSection.Id).ToList()).SingleOrDefault();
                    if (LoanITSection != null)
                    {
                        ITInvestmentPaymentitem.LoanAdvanceHead.ITSection = LoanITSection;

                    }
                    List<LoanAdvancePolicy> LoanAdvancePolicy = db.LoanAdvanceHead.Where(e => e.Id == ITInvestmentPaymentitem.LoanAdvanceHead_Id).Select(e => e.LoanAdvancePolicy.ToList()).SingleOrDefault();
                    if (LoanAdvancePolicy != null)
                    {

                        ITInvestmentPaymentitem.LoanAdvanceHead.LoanAdvancePolicy = LoanAdvancePolicy;
                    }

                    ITInvestment ITInvestment = db.ITInvestment.Where(e => e.Id == ITInvestmentPaymentitem.ITInvestment_Id).SingleOrDefault();
                    if (ITInvestment != null)
                    {
                        ITInvestmentPaymentitem.ITInvestment = ITInvestment;
                        SalaryHead InvestSalaryHead = db.SalaryHead.Where(e => e.Id == ITInvestment.SalaryHead_Id).SingleOrDefault();
                        ITInvestmentPaymentitem.ITInvestment.SalaryHead = InvestSalaryHead;
                    }




                }
                OEmpInvest.ITInvestmentPayment = ITInvestmentPayment;
                //.Include(e => e.ITInvestmentPayment)
                //.Include(e => e.ITInvestmentPayment.Select(r => r.FinancialYear))
                //.Include(e => e.ITInvestmentPayment.Select(r => r.ITSection))
                //.Include(e => e.ITInvestmentPayment.Select(r => r.ITSection.ITSectionList))
                //.Include(e => e.ITInvestmentPayment.Select(r => r.ITSection.ITSectionListType))
                //.Include(e => e.ITInvestmentPayment.Select(r => r.ITSection.ITSectionList))//added 17042017 by prashant
                //.Include(e => e.ITInvestmentPayment.Select(r => r.ITSubInvestmentPayment))
                //.Include(e => e.ITInvestmentPayment.Select(r => r.LoanAdvanceHead))
                //.Include(e => e.ITInvestmentPayment.Select(r => r.LoanAdvanceHead.ITLoan))
                //.Include(e => e.ITInvestmentPayment.Select(r => r.LoanAdvanceHead.ITSection))
                //.Include(e => e.ITInvestmentPayment.Select(r => r.LoanAdvanceHead.LoanAdvancePolicy))
                //.Include(e => e.ITInvestmentPayment.Select(r => r.LoanAdvanceHead.SalaryHead))
                //.Include(e => e.ITInvestmentPayment.Select(r => r.ITInvestment))
                //.Include(e => e.ITInvestmentPayment.Select(r => r.ITInvestment.SalaryHead))

                if (OEmpInvest.ITInvestmentPayment != null) //added by shankar
                //if (OEmpITInvestment != null) //comment by shankar
                {
                    OEmpITInvestment = OEmpInvest.ITInvestmentPayment.Where(e => e.FinancialYear.Id == OFinancialYear && e.ITSection.ITSectionListType.LookupVal.ToUpper() == "EXTRAREBATE").ToList();
                    List<ITSubInvestmentPayment> OEmpITSubInvestment = OEmpInvest.ITInvestmentPayment
                         .Where(e => e.FinancialYear.Id == OFinancialYear && e.ITSection.ITSectionListType.LookupVal.ToUpper() == "EXTRAREBATE" && e.ITSubInvestmentPayment != null)
                         .SelectMany(e => e.ITSubInvestmentPayment).ToList();
                }



                //check for salary depend investments
                double mTotActInvst = 0;
                double mTotDeclInvst = 0;
                double mTotActQualify = 0;
                double mTotDeclQualify = 0;
                List<ITInvestment> OITInvestmentsSal = OITInvestments.Where(e => e.IsSalaryHead == true).ToList();
                if (OITInvestmentsSal != null && OITInvestmentsSal.Count() > 0)
                {
                    List<ITInvestmentPayment> mList = new List<ITInvestmentPayment>();
                    foreach (ITInvestment ca in OITInvestmentsSal)
                    {
                        mTotActInvst = OITSalMonthwise.Where(r => r.SalaryHead.Id == ca.SalaryHead.Id)
                                       .Sum(m => m.ActualAmount);

                        mTotDeclInvst = OITSalMonthwise.Where(r => r.SalaryHead.Id == ca.SalaryHead.Id)
                                        .Sum(m => m.ProjectedAmount);

                        // for new tax slab start 28122020
                        Calendar temp_OFinancialYear = db.Calendar.Where(e => e.Id == OFinancialYear).SingleOrDefault();
                        DateTime finfrm = Convert.ToDateTime("01/04/2020");
                        if (Regimischemecurryear != null)
                        {
                            if (Regimischemecurryear.Scheme.LookupVal.ToUpper() == "NEWTAX")
                            {
                                mTotActInvst = 0;
                                mTotDeclInvst = 0;
                            }
                        }
                        else
                        {

                            if (temp_OFinancialYear.FromDate >= finfrm && OEmployeePayroll.Employee.EmpOffInfo.NationalityID.No2 == "Yes")
                            {
                                mTotActInvst = 0;
                                mTotDeclInvst = 0;
                            }
                        }
                        // for new tax slab end 28122020



                        //modify investment record
                        //var OEmpITInvestmentTemp = OEmpITInvestment.Where(e => e.ITInvestment.SalaryHead != null).ToList();
                        ITInvestmentPayment OEmpSalInvestmentChk = OEmpITInvestment.Where(e => e.ITInvestment.SalaryHead != null && e.ITInvestment.SalaryHead.Id == ca.SalaryHead.Id).SingleOrDefault();
                        if (OEmpSalInvestmentChk != null)
                        {
                            ITInvestmentPayment OEmpSalInvestmentObj = db.ITInvestmentPayment.Where(a => a.Id == OEmpSalInvestmentChk.Id).SingleOrDefault();
                            OEmpSalInvestmentObj.ActualInvestment = mTotActInvst;
                            OEmpSalInvestmentObj.DeclaredInvestment = mTotDeclInvst;
                            if (Flag == 1)
                            {
                                db.Entry(OEmpSalInvestmentObj).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                            }
                            //mList.Add(OEmpSalInvestmentObj);

                        }
                        else
                        {
                            ITSection OITSectiondata = OITSection80CCC.Where(e => e.ITInvestments == ca).SingleOrDefault();
                            ITInvestmentPayment OEmpITInvestmentSave = new ITInvestmentPayment()
                            {
                                FinancialYear = db.Calendar.Where(e => e.Id == OFinancialYear).SingleOrDefault(),
                                ITInvestment = db.ITInvestment.Where(a => a.Id == ca.Id).SingleOrDefault(),
                                InvestmentDate = DateTime.Now.Date,
                                ITSection = db.ITSection.Where(a => a.Id == OITSectiondata.Id).SingleOrDefault(),
                                ITSubInvestmentPayment = null,
                                ActualInvestment = mTotActInvst,
                                DeclaredInvestment = mTotDeclInvst,
                                Narration = "Investment under Section 80CCC to 80CCD through Salary",
                                DBTrack = dbt

                            };

                            if (Flag == 1)
                            {
                                db.ITInvestmentPayment.Add(OEmpITInvestmentSave);
                                db.SaveChanges();
                            }

                            mList.Add(OEmpITInvestmentSave);

                        }

                    }
                    if (mList.Count() > 0 && Flag == 1)
                    {
                        int OEmpId = OEmployeePayroll.Id;

                        EmployeePayroll aa = db.EmployeePayroll.Where(a => a.Id == OEmpId).SingleOrDefault();
                        if (aa.ITInvestmentPayment != null)
                        { mList.AddRange(aa.ITInvestmentPayment); }
                        aa.ITInvestmentPayment = mList;
                        //OEmployeePayroll.DBTrack = dbt;
                        db.EmployeePayroll.Attach(aa);
                        db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        //db.Entry(aa).State = System.Data.Entity.EntityState.Detached;

                    }


                }

                List<ITInvestmentPayment> ONormalInvestment = null;
                //check all investments
                if (OEmpITInvestment != null)
                {

                    ONormalInvestment = OEmpITInvestment.ToList();
                }
                //double mTotActSubInvst=0;
                //double mTotDeclSubInvst=0;
                mTotActInvst = 0;
                mTotDeclInvst = 0;
                mTotActQualify = 0;
                mTotDeclQualify = 0;
                double m_secmaxexemptionQualNR = 0;
                double m_secmaxexemptionactNR = 0;
                if (ONormalInvestment != null)
                {
                    m_secmaxexemptionQualNR = ONormalInvestment.Select(x => x.ITSection.ExemptionLimit).FirstOrDefault();
                    m_secmaxexemptionactNR = m_secmaxexemptionQualNR;
                    foreach (ITInvestmentPayment ca in ONormalInvestment)
                    {
                        if (ca.ITSubInvestmentPayment != null && ca.ITSubInvestmentPayment.Count() > 0)
                        {
                            mTotActInvst = ca.ITSubInvestmentPayment.Sum(e => e.ActualInvestment);
                            mTotDeclInvst = ca.ITSubInvestmentPayment.Sum(e => e.DeclaredInvestment);
                            if (mTotActInvst > ca.ITInvestment.MaxAmount)
                            {
                                mTotActQualify = ca.ITInvestment.MaxAmount;
                            }
                            else
                            {
                                mTotActQualify = mTotActInvst;
                            }
                            if (mTotDeclInvst > ca.ITInvestment.MaxAmount)
                            {
                                mTotDeclQualify = ca.ITInvestment.MaxAmount;
                            }
                            else
                            {
                                //mTotDeclQualify = mTotActInvst; comment by Surendra
                                mTotDeclQualify = mTotDeclInvst;
                            }
                        }
                        else
                        {
                            mTotActInvst = ca.ActualInvestment;
                            mTotDeclInvst = ca.DeclaredInvestment;
                            if (mTotActInvst > ca.ITInvestment.MaxAmount)
                            {
                                mTotActQualify = ca.ITInvestment.MaxAmount;
                            }
                            else
                            {
                                mTotActQualify = mTotActInvst;
                            }
                            if (mTotDeclInvst > ca.ITInvestment.MaxAmount)
                            {
                                mTotDeclQualify = ca.ITInvestment.MaxAmount;
                            }
                            else
                            {
                                //mTotDeclQualify = mTotActInvst; comment by Surendra
                                mTotDeclQualify = mTotDeclInvst;
                            }
                        }

                        // section celling start
                        if (m_secmaxexemptionactNR > mTotActQualify)
                        {
                            mTotActQualify = mTotActQualify;
                            m_secmaxexemptionactNR = m_secmaxexemptionactNR - mTotActQualify;
                        }
                        else
                        {
                            mTotActQualify = m_secmaxexemptionactNR;
                            m_secmaxexemptionactNR = m_secmaxexemptionactNR - m_secmaxexemptionactNR;
                        }

                        if (m_secmaxexemptionQualNR > mTotDeclQualify)
                        {
                            mTotDeclQualify = mTotDeclQualify;
                            m_secmaxexemptionQualNR = m_secmaxexemptionQualNR - mTotActQualify;
                        }
                        else
                        {
                            mTotDeclQualify = m_secmaxexemptionQualNR;
                            m_secmaxexemptionQualNR = m_secmaxexemptionQualNR - m_secmaxexemptionQualNR;
                        }

                        // section celling end


                        // for new tax slab start 28122020
                        Calendar temp_OFinancialYear = db.Calendar.Where(e => e.Id == OFinancialYear).SingleOrDefault();
                        DateTime finfrm = Convert.ToDateTime("01/04/2020");
                        if (Regimischemecurryear != null)
                        {
                            if (Regimischemecurryear.Scheme.LookupVal.ToUpper() == "NEWTAX" && ca.ITSection.ITSectionList.LookupVal.ToUpper() != "SECTION80CCD(2)")
                            {
                                mTotActInvst = 0;
                                mTotDeclInvst = 0;
                                mTotActQualify = 0;
                                mTotDeclQualify = 0;

                            }
                        }
                        else
                        {


                            if (temp_OFinancialYear.FromDate >= finfrm && OEmployeePayroll.Employee.EmpOffInfo.NationalityID.No2 == "Yes" && ca.ITSection.ITSectionList.LookupVal.ToUpper() != "SECTION80CCD(2)")
                            {
                                mTotActInvst = 0;
                                mTotDeclInvst = 0;
                                mTotActQualify = 0;
                                mTotDeclQualify = 0;
                            }
                        }
                        // for new tax slab end 28122020



                        ItProjectionTempClass OITProjectionSave = new ItProjectionTempClass
                        {
                            FinancialYear = OFinancialYear,
                            Tiltle = "",
                            Section = ca.ITSection != null && ca.ITSection.ITSectionList != null ? ca.ITSection.ITSectionList.LookupVal.ToUpper() : null,  //added by shankar
                            // Section = ca.ITSection.ITSectionList.LookupVal.ToUpper(), // comment by shankar
                            SectionType = "",
                            ChapterName = ca.ITInvestment != null ? "    " + ca.ITInvestment.ITInvestmentName : null, //added by shankar
                            //ChapterName = ca.ITInvestment.ITInvestmentName, // comment by shankar

                            ProjectedAmount = mTotDeclInvst,
                            ActualAmount = mTotActInvst,
                            ActualQualifyingAmount = mTotActQualify,
                            ProjectedQualifyingAmount = mTotDeclQualify,
                            //   ProjectionDate = DateTime.Now.Date,

                            Narration = "Investment details under section 80CCC to 80CCD",
                            //   DBTrack = dbt,
                            PickupId = 101
                        };
                        OITProjectionDataList.Add(OITProjectionSave);
                        //add list of it projection data
                    }
                }
                var OITSectio80CCCTotal = OITProjectionDataList.Where(e => e.PickupId == 101)
                    .GroupBy(e => e.PickupId)
                    .Select(r => new
                    {
                        ProjectedAmount = r.Sum(t => t.ProjectedAmount),
                        ActualAmount = r.Sum(t => t.ActualAmount),
                        ActualQualifyingAmount = r.Sum(t => t.ActualQualifyingAmount),
                        ProjectedQualifyingAmount = r.Sum(t => t.ProjectedQualifyingAmount)
                    })
                    .SingleOrDefault();
                double mTotAct = 0;
                double mTotProj = 0;
                double mTotQAct = 0;
                double mTotQProj = 0;
                if (OITSectio80CCCTotal != null)
                {
                    mTotAct = OITSectio80CCCTotal.ActualAmount;
                    mTotProj = OITSectio80CCCTotal.ProjectedAmount;
                    mTotQAct = OITSectio80CCCTotal.ActualQualifyingAmount;
                    mTotQProj = OITSectio80CCCTotal.ProjectedQualifyingAmount;
                }

                {
                    ItProjectionTempClass OITProjectionSave = new ItProjectionTempClass
                    {
                        FinancialYear = OFinancialYear,
                        Tiltle = "",
                        Section = "",
                        SectionType = "",
                        ChapterName = "Total of Investments under Section80CCC-CCG",

                        ProjectedAmount = mTotProj,
                        ActualAmount = mTotAct,
                        ActualQualifyingAmount = mTotQAct,
                        ProjectedQualifyingAmount = mTotQProj,
                        //  ProjectionDate = DateTime.Now.Date,

                        Narration = "Total of Investment under section 80CCC-CCG",
                        //    DBTrack = dbt,
                        PickupId = 102
                    };
                    OITProjectionDataList.Add(OITProjectionSave);
                }


                return OITProjectionDataList;
                //}
            }
        }
        public static List<ItProjectionTempClass> InvestmentSection24Loan(EmployeePayroll OEmployeePayroll, IncomeTax OITMaster,
            List<ITSalaryHeadData> OITSalMonthwise, Int32 OFinancialYear, string OLastMonth, SalaryT OSalaryTC, int Flag, int ProcType)
        {

            //Utility.DumpProcessStatus("InvestmentSection24Loan");
            DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };
            //using (DataBaseContext db = new DataBaseContext())
            //{
            //*****************Loan Section*********************//
            List<ItProjectionTempClass> OITProjectionDataList = new List<ItProjectionTempClass>();
            using (DataBaseContext db = new DataBaseContext())
            {
                var Regimi = db.EmployeePayroll.Include(e => e.RegimiScheme).Include(e => e.RegimiScheme.Select(x => x.FinancialYear)).Include(e => e.RegimiScheme.Select(y => y.Scheme)).Where(e => e.Id == OEmployeePayroll.Id).FirstOrDefault();
                var Regimischemecurryear = Regimi.RegimiScheme.Where(e => e.FinancialYear.Id == OFinancialYear).FirstOrDefault();

                EmployeePayroll OEmpITSection24EmpData = db.EmployeePayroll.Where(e => e.Id == OEmployeePayroll.Id).SingleOrDefault();

                //.Include(e => e.ITSection24Payment)
                //  .Include(e => e.ITSection24Payment.Select(r => r.FinancialYear))
                //  .Include(e => e.ITSection24Payment.Select(r => r.ITSection))
                //  .Include(e => e.ITSection24Payment.Select(r => r.ITSection.ITSectionList))
                //  .Include(e => e.ITSection24Payment.Select(r => r.ITSection.ITSectionListType))
                //  .Include(e => e.ITSection24Payment.Select(r => r.ITSection.ITSectionListType))

                //  .Include(e => e.ITSection24Payment.Select(r => r.ITSection.LoanAdvanceHead))
                //  .Include(e => e.ITSection24Payment.Select(r => r.LoanAdvanceHead))
                //  .Include(e => e.ITSection24Payment.Select(r => r.LoanAdvanceHead.SalaryHead))
                //  .Include(e => e.LoanAdvRequest)
                //  .Include(e => e.LoanAdvRequest.Select(r => r.LoanAdvanceHead))
                //  .Include(e => e.LoanAdvRequest.Select(r => r.LoanAdvanceHead.ITLoan))
                //  .Include(e => e.LoanAdvRequest.Select(r => r.LoanAdvanceHead.SalaryHead))
                //  .Include(e => e.LoanAdvRequest.Select(r => r.LoanAdvRepaymentT))


                List<ITSection24Payment> ITSection24Payment = db.ITSection24Payment.Where(e => e.EmployeePayroll_Id == OEmpITSection24EmpData.Id && e.FinancialYear_Id == OFinancialYear).ToList();//modofied on 26/10/2022 by prashant for fy

                foreach (var ITSection24Paymentitem in ITSection24Payment)
                {
                    Calendar FinancialYear = db.Calendar.Where(e => e.Id == ITSection24Paymentitem.FinancialYear_Id).SingleOrDefault();
                    ITSection24Paymentitem.FinancialYear = FinancialYear;
                    ITSection ITSection = db.ITSection.Include(x => x.LoanAdvanceHead).Where(e => e.Id == ITSection24Paymentitem.ITSection_Id).SingleOrDefault();
                    ITSection24Paymentitem.ITSection = ITSection;
                    LookupValue ITSectionList = db.LookupValue.Where(e => e.Id == ITSection.ITSectionList_Id).SingleOrDefault();
                    ITSection24Paymentitem.ITSection.ITSectionList = ITSectionList;
                    LookupValue ITSectionListType = db.LookupValue.Where(e => e.Id == ITSection.ITSectionListType_Id).SingleOrDefault();
                    ITSection24Paymentitem.ITSection.ITSectionListType = ITSectionListType;
                    //List<LoanAdvanceHead> ITSectionLoanAdvanceHead = db.ITSection.Where(e => e.Id == ITSection24Paymentitem.ITSection_Id).Select(e => e.LoanAdvanceHead.ToList()).SingleOrDefault();
                    //ITSection24Paymentitem.ITSection.LoanAdvanceHead = ITSectionLoanAdvanceHead;

                    //List<ITSubInvestmentPayment> ITSubInvestmentPayment = db.ITInvestmentPayment.Where(e => e.Id == ITSection24Paymentitem.ITInvestment_Id).Select(e => e.ITSubInvestmentPayment.ToList()).SingleOrDefault();
                    //ITSection24Paymentitem.ITSubInvestmentPayment = ITSubInvestmentPayment;
                    LoanAdvanceHead LoanAdvanceHead = db.LoanAdvanceHead.Where(e => e.Id == ITSection24Paymentitem.LoanAdvanceHead_Id).SingleOrDefault();
                    ITSection24Paymentitem.LoanAdvanceHead = LoanAdvanceHead;
                    //List<ITSection> LoanITSection = db.LoanAdvanceHead.Where(e => e.Id == ITInvestmentPaymentitem.LoanAdvanceHead_Id).Select(e => e.ITSection.ToList()).SingleOrDefault();
                    //LoanAdvanceHead.ITSection = LoanITSection;
                    //List<LoanAdvancePolicy> LoanAdvancePolicy = db.LoanAdvanceHead.Where(e => e.Id == ITInvestmentPaymentitem.LoanAdvanceHead_Id).Select(e => e.LoanAdvancePolicy.ToList()).SingleOrDefault();
                    //LoanAdvanceHead.LoanAdvancePolicy = LoanAdvancePolicy;
                    SalaryHead SalaryHead = db.SalaryHead.Where(e => e.Id == LoanAdvanceHead.SalaryHead_Id).SingleOrDefault();
                    ITSection24Paymentitem.LoanAdvanceHead.SalaryHead = SalaryHead;
                }
                OEmpITSection24EmpData.ITSection24Payment = ITSection24Payment;
                var Fincalendar = db.Calendar.Where(e => e.Id == OFinancialYear).SingleOrDefault();
                var mFromPeriod = Fincalendar.FromDate.Value;
                var mToPeriod = Fincalendar.ToDate.Value;
                List<string> mPeriodYear = new List<string>();
                string mPeriodRangeYear = "";
                for (DateTime mTempDate = mFromPeriod; mTempDate <= mToPeriod; mTempDate = mTempDate.AddMonths(1))
                {
                    if (mPeriodRangeYear == "")
                    {
                        mPeriodRangeYear = Convert.ToDateTime(mTempDate).ToString("MM/yyyy");
                        mPeriodYear.Add(Convert.ToDateTime(mTempDate).ToString("MM/yyyy"));
                    }
                    else
                    {
                        mPeriodRangeYear = mPeriodRangeYear + "," + Convert.ToDateTime(mTempDate).ToString("MM/yyyy");
                        mPeriodYear.Add(Convert.ToDateTime(mTempDate).ToString("MM/yyyy"));
                    }
                }
                List<LoanAdvRequest> LoanAdvRequest = db.LoanAdvRequest.Where(e => e.EmployeePayroll_Id == OEmpITSection24EmpData.Id).ToList();

                foreach (var LoanAdvRequestitem in LoanAdvRequest)
                {
                    LoanAdvanceHead LoanAdvanceHead = db.LoanAdvanceHead.Where(e => e.Id == LoanAdvRequestitem.LoanAdvanceHead_Id).SingleOrDefault();
                    LoanAdvRequestitem.LoanAdvanceHead = LoanAdvanceHead;
                    ITLoan ITLoan = db.ITLoan.Where(e => e.Id == LoanAdvanceHead.ITLoan_Id).SingleOrDefault();
                    LoanAdvRequestitem.LoanAdvanceHead.ITLoan = ITLoan;
                    SalaryHead SalaryHead = db.SalaryHead.Where(e => e.Id == LoanAdvanceHead.SalaryHead_Id).SingleOrDefault();
                    LoanAdvRequestitem.LoanAdvanceHead.SalaryHead = SalaryHead;
                    DateTime mdate = new DateTime();
                    //newly modified for period selection
                    List<LoanAdvRepaymentT> LoanAdvRepaymentT = db.LoanAdvRequest.Where(e => e.Id == LoanAdvRequestitem.Id).Select(e => e.LoanAdvRepaymentT.Where(r => mPeriodYear.Contains(r.PayMonth)).ToList()).SingleOrDefault();
                    LoanAdvRequestitem.LoanAdvRepaymentT = LoanAdvRepaymentT;
                }
                OEmpITSection24EmpData.LoanAdvRequest = LoanAdvRequest;






                //    List<ITSection> OITSection = OITMaster.ITSection.ToList();

                List<ITSection> OITSection24 = OITMaster.ITSection.Where(e => e.ITSectionList.LookupVal.ToUpper() == "SECTION24" && e.ITSectionListType.LookupVal.ToUpper() == "LOAN").ToList();

                List<LoanAdvanceHead> OITInvestmentsList = OITSection24.SelectMany(e => e.LoanAdvanceHead.Where(a => a.SalaryHead != null && a.ITLoan != null && (a.ITLoan.IntAppl == true || a.ITLoan.IntPrincAppl == true))).ToList();

                //var OITSubInvestments=OITInvestments.Select(e=>e.ITSubInvestment).ToList();
                List<ITSection24Payment> OEmpIT24Investment = OEmpITSection24EmpData.ITSection24Payment.Where(e => e.FinancialYear.Id == OFinancialYear && e.ITSection.ITSectionListType.LookupVal.ToUpper() == "LOAN").ToList();
                //var OEmpITSubInvestment=OEmployeePayroll.ITInvestmentPayment
                //    .Where(e=>e.FinancialYear==OFinancialYear)
                //    .Select(e=>e.ITSubInvestmentPayment).ToList();

                // var OEmpSal = OEmployeePayroll.SalaryT.Where(e => e.PayMonth.Contains(mClosedMonths)).Select(r => r.SalEarnDedT.Where(q => q.SalaryHead.InITax == true && q.SalaryHead.Type.LookupVal.ToUpper() == "DEDUCTIONS")).ToList();
                Calendar temp_OFinancialYear = db.Calendar.Where(e => e.Id == OFinancialYear).SingleOrDefault();
                //check for salary depend investments
                double mTotActInvst = 0;
                double mTotDeclInvst = 0;
                double mTotActQualify = 0;
                double mTotDeclQualify = 0;
                foreach (LoanAdvanceHead OITInvestments in OITInvestmentsList)
                {
                    //var OITInvestments = OITInvestmentsList.ToList();

                    LoanAdvanceHead OITInvestmentsSal = OITInvestments;

                    //if (OITInvestmentsSal != null && OITInvestmentsSal.Count() > 0)
                    //{
                    List<ITSection24Payment> mList = new List<ITSection24Payment>();
                    //foreach (var ca in OITInvestmentsSal)
                    //{
                    //to be check later
                    List<LoanAdvRequest> OLoanAdvReqF = OEmpITSection24EmpData.LoanAdvRequest.Where(e => e.LoanAdvanceHead.Id == OITInvestments.Id).ToList();
                    //mTotActInvst = OITSalMonthwise.Where(r => r.SalaryHead.Id == OITInvestments.SalaryHead.Id)
                    //                   .Sum(m => m.ActualAmount);
                    var Id = Convert.ToInt32(SessionManager.CompanyId);
                    string _CompCode = db.Company.Where(e => e.Id == Id).SingleOrDefault().Code.ToUpper();

                    mTotActInvst = 0;
                    mTotDeclInvst = 0;
                    foreach (LoanAdvRequest ca1 in OLoanAdvReqF) //interest calculation
                    {
                        DateTime mLoanEndDate = temp_OFinancialYear.ToDate.Value;
                        if (ca1.CloserDate < mLoanEndDate)
                        {
                            mLoanEndDate = ca1.CloserDate.Value;
                        }

                        mTotActInvst = OLoanAdvReqF.SelectMany(r => r.LoanAdvRepaymentT.Where(t => t.InstallementDate >= temp_OFinancialYear.FromDate && t.InstallementDate <= mLoanEndDate && t.RepaymentDate != null)).Sum(e => e.MonthlyInterest);
                        if (_CompCode == "KDCC")
                        {


                            mTotDeclInvst = OLoanAdvReqF.SelectMany(r => r.LoanAdvRepaymentT.Where(t => t.InstallementDate >= temp_OFinancialYear.FromDate && t.InstallementDate <= mLoanEndDate && t.RepaymentDate == null)).Sum(e => e.MonthlyInterest);
                        }
                        else
                        {
                            var mTotActInvstpro = OLoanAdvReqF.SelectMany(r => r.LoanAdvRepaymentT.OrderBy(x => x.InstallementDate).Where(t => t.InstallementDate >= temp_OFinancialYear.FromDate && t.InstallementDate <= mLoanEndDate && t.RepaymentDate != null)).LastOrDefault();
                            if (mTotActInvstpro != null)
                            {

                                Int32 mBalMonths = (mLoanEndDate.Month + mLoanEndDate.Year * 12) - (Convert.ToDateTime("01/" + OLastMonth).Month + Convert.ToDateTime("01/" + OLastMonth).Year * 12) - 1;
                                //mTotDeclInvst = (ca1.MonthlyInterest * mBalMonths);
                                if (mBalMonths < 0)
                                {
                                    mBalMonths = 0;
                                }
                                mTotDeclInvst = (mTotActInvstpro.MonthlyInterest * mBalMonths);
                            }
                        }
                    }

                    mTotDeclInvst = mTotDeclInvst + mTotActInvst;

                    if (ProcType == 1 || ProcType == 2)
                    {
                        mTotActInvst = mTotDeclInvst;
                    }

                    // for new tax slab start 28122020
                    DateTime finfrm = Convert.ToDateTime("01/04/2020");
                    if (Regimischemecurryear != null)
                    {
                        if (Regimischemecurryear.Scheme.LookupVal.ToUpper() == "NEWTAX")
                        {
                            mTotActInvst = 0;
                            mTotDeclInvst = 0;
                        }
                    }
                    else
                    {

                        if (temp_OFinancialYear.FromDate >= finfrm && OEmployeePayroll.Employee.EmpOffInfo.NationalityID.No2 == "Yes")
                        {
                            mTotActInvst = 0;
                            mTotDeclInvst = 0;
                        }
                    }
                    // for new tax slab end 28122020



                    //modify investment record dated 27/10/2022
                    //var OEmpITInvestmentTemp = OEmpIT24Investment.Where(e => ).ToList();
                    ITSection24Payment OEmpSalInvestmentChk = OEmpIT24Investment.Where(e =>
                        e.LoanAdvanceHead != null &&
                        e.LoanAdvanceHead.SalaryHead != null &&
                        e.LoanAdvanceHead.SalaryHead.Id == OITInvestments.SalaryHead.Id).SingleOrDefault();
                    if (OEmpSalInvestmentChk != null)
                    {
                        ITSection24Payment OEmpSalInvestmentObj = db.ITSection24Payment.Where(a => a.Id == OEmpSalInvestmentChk.Id).SingleOrDefault();
                        OEmpSalInvestmentObj.ActualInterest = mTotActInvst;
                        OEmpSalInvestmentObj.DeclaredInterest = mTotDeclInvst;
                        if (Flag == 1)
                        {
                            db.Entry(OEmpSalInvestmentObj).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                        }
                        //mList.Add(OEmpSalInvestmentObj);

                    }
                    else
                    {

                        ITSection OITSectiondata = OITSection24.Where(e => e.LoanAdvanceHead.Contains(OITInvestments)).SingleOrDefault();

                        ITSection24Payment OEmpITInvestmentSave = new ITSection24Payment()
                        {
                            FinancialYear = db.Calendar.Where(e => e.Id == OFinancialYear).SingleOrDefault(),
                            LoanAdvanceHead = db.LoanAdvanceHead.Include(q => q.SalaryHead).Where(a => a.Id == OITInvestments.Id).SingleOrDefault(),
                            InvestmentDate = DateTime.Now.Date,
                            ITSection = db.ITSection.Include(q => q.LoanAdvanceHead.Select(a => a.SalaryHead)).Where(a => a.Id == OITSectiondata.Id).SingleOrDefault(),
                            PaymentName = OITInvestments.Name,
                            ActualInterest = mTotActInvst,
                            DeclaredInterest = mTotDeclInvst,
                            //ActualInterest = 0,
                            //DeclaredInterest = 0,
                            Narration = "Investment under Section 24 through Salary",
                            DBTrack = dbt,
                            SalaryApp = true

                        };

                        if (Flag == 1)
                        {
                            db.ITSection24Payment.Add(OEmpITInvestmentSave);
                            db.SaveChanges();
                        }

                        mList.Add(OEmpITInvestmentSave);
                        OEmpIT24Investment.Add(OEmpITInvestmentSave);

                    }

                    // }
                    if (mList.Count() > 0 && Flag == 1)
                    {
                        int OEmpId = OEmployeePayroll.Id;

                        EmployeePayroll aa = db.EmployeePayroll.Where(a => a.Id == OEmpId).SingleOrDefault();
                        if (aa.ITSection24Payment != null)
                        { mList.AddRange(aa.ITSection24Payment); }
                        aa.ITSection24Payment = mList;
                        //OEmployeePayroll.DBTrack = dbt;
                        db.EmployeePayroll.Attach(aa);
                        db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        //db.Entry(aa).State = System.Data.Entity.EntityState.Detached;

                    }

                    // }
                }
                //check all investment
                //  var ONormalSection24 = OEmpIT24Investment.ToList();
                //double mTotActSubInvst=0;
                //double mTotDeclSubInvst=0;
                mTotActInvst = 0;
                mTotDeclInvst = 0;
                mTotActQualify = 0;
                mTotDeclQualify = 0;

                var OCompany = db.Company.Select(e => e.Code).FirstOrDefault();

                if (OEmpIT24Investment != null && OEmpIT24Investment.Count() > 0)
                {
                    //************** section10 exemption writing *********************//

                    //{
                    //    ITProjection OITProjectionSave = new ItProjectionTempClass
                    //    {
                    //        Tiltle = "Section24 - Housing Interest",
                    //        FinancialYear = db.Calendar.Where(e=>e.Id==OFinancialYear).SingleOrDefault(),
                    //        FromPeriod = null,
                    //        ToPeriod = null,
                    //        Section = "",
                    //        SectionType = "",
                    //        ChapterName = "",
                    //        SubChapter = "",
                    //        ProjectedAmount = 0,
                    //        ActualAmount = 0,
                    //        ActualQualifyingAmount = 0,
                    //        ProjectedQualifyingAmount = 0,
                    //        ProjectionDate = DateTime.Now.Date,
                    //        TDSComponents = 0,
                    //        QualifiedAmount = 0,
                    //        Narration = "Housing Loan Interest  under section 24 ",
                    //        DBTrack = dbt
                    //    };
                    //    OITProjectionDataList.Add(OITProjectionSave);
                    //}

                    double mFinActInvst = 0;
                    double mFinDeclInvst = 0;
                    double mFinActQualify = 0;
                    double mFinDeclQualify = 0;

                    if (OCompany.ToUpper() == "ACABL")
                    {
                        foreach (ITSection24Payment ca in OEmpIT24Investment)
                        {
                            mTotActInvst = 0;
                            mTotDeclInvst = 0;
                            double checkamt = 0;

                            mTotActInvst = mTotActInvst + ca.ActualInterest;
                            mTotDeclInvst = mTotDeclInvst + ca.DeclaredInterest;

                            EmployeePayroll aa = db.EmployeePayroll.Where(a => a.Id == OEmployeePayroll.Id).SingleOrDefault();

                            var loanhead = aa.ITSection24Payment.Where(e => e.LoanAdvanceHead.Code.ToUpper() == "HBINTREPAR").FirstOrDefault();
                            if (loanhead != null)
                            {
                                checkamt = loanhead.ActualInterest;
                            }

                            if (checkamt > 30000 && loanhead != null)
                            {
                                checkamt = 30000;
                            }


                            if (ca.LoanAdvanceHead.Code.ToUpper() != "HBINTREPAR" && checkamt > 0 && ca.ActualInterest >= ca.ITSection.ExemptionLimit)
                            {
                                mTotActQualify = ca.ITSection.ExemptionLimit - checkamt;
                            }
                            else if (ca.LoanAdvanceHead.Code.ToUpper() == "HBINTREPAR" && checkamt >= 30000)
                            {
                                mTotActQualify = 30000;
                            }
                            else if (ca.ActualInterest > ca.ITSection.ExemptionLimit)
                            {
                                mTotActQualify = ca.ITSection.ExemptionLimit;
                            }
                            else
                            {
                                mTotActQualify = mTotActInvst;
                            }


                            double checkamt1 = 0;
                            if (loanhead != null)
                            {
                                checkamt1 = loanhead.DeclaredInterest;
                            }

                            if (checkamt1 > 30000 && loanhead != null)
                            {
                                checkamt1 = 30000;
                            }
                            if (ca.LoanAdvanceHead.Code.ToUpper() != "HBINTREPAR" && checkamt > 0 && ca.DeclaredInterest >= ca.ITSection.ExemptionLimit)
                            {
                                mTotDeclQualify = ca.ITSection.ExemptionLimit - checkamt1;
                            }
                            else if (ca.LoanAdvanceHead.Code.ToUpper() == "HBINTREPAR" && checkamt1 >= 30000)
                            {
                                mTotDeclQualify = 30000;
                            }
                            else if (ca.DeclaredInterest > ca.ITSection.ExemptionLimit)
                            {
                                mTotDeclQualify = ca.ITSection.ExemptionLimit;
                            }

                            else
                            {
                                //  mTotDeclQualify = mTotActInvst;
                                mTotDeclQualify = mTotDeclInvst;
                            }



                            mFinActInvst = mFinActInvst + mTotActInvst;
                            mFinDeclInvst = mFinDeclInvst + mTotDeclInvst;
                            mFinActQualify = mFinActQualify + mTotActQualify;
                            mFinDeclQualify = mFinDeclQualify + mTotDeclQualify;

                            // for new tax slab start 28122020
                            double declint = 0;
                            double Actint = 0;
                            DateTime finfrm = Convert.ToDateTime("01/04/2020");
                            if (Regimischemecurryear != null)
                            {
                                if (Regimischemecurryear.Scheme.LookupVal.ToUpper() == "NEWTAX")
                                {
                                    mFinActInvst = 0;
                                    mFinDeclInvst = 0;
                                    mTotActQualify = 0;
                                    mTotDeclQualify = 0;
                                    declint = 0;
                                    Actint = 0;
                                }
                                else
                                {
                                    declint = ca.DeclaredInterest;
                                    Actint = ca.ActualInterest;
                                }
                            }
                            else
                            {


                                if (temp_OFinancialYear.FromDate >= finfrm && OEmployeePayroll.Employee.EmpOffInfo.NationalityID.No2 == "Yes")
                                {
                                    mFinActInvst = 0;
                                    mFinDeclInvst = 0;
                                    mTotActQualify = 0;
                                    mTotDeclQualify = 0;
                                    declint = 0;
                                    Actint = 0;
                                }
                                else
                                {
                                    declint = ca.DeclaredInterest;
                                    Actint = ca.ActualInterest;
                                }
                            }
                            // for new tax slab end 28122020

                            ItProjectionTempClass OITProjectionSave = new ItProjectionTempClass
                            {
                                FinancialYear = OFinancialYear,
                                Tiltle = "",
                                Section = ca.ITSection == null ? "" : ca.ITSection.ITSectionList == null ? "" : ca.ITSection.ITSectionList.LookupVal.ToUpper(),
                                SectionType = ca.ITSection == null ? "" : ca.ITSection.ITSectionListType == null ? "" : ca.ITSection.ITSectionListType.LookupVal.ToUpper(),
                                ChapterName = ca.LoanAdvanceHead == null ? ca.PaymentName : ca.LoanAdvanceHead.Name,// "CHAPTER Loan",
                                //SubChapter = ca.ITSection.ITSectionList.LookupVal.ToUpper(),
                                ////SalaryHeadName="",
                                ProjectedAmount = declint,
                                ActualAmount = Actint,
                                ActualQualifyingAmount = mTotActQualify,
                                ProjectedQualifyingAmount = mTotDeclQualify,
                                //   ProjectionDate = DateTime.Now.Date,
                                TDSComponents = 0,
                                QualifiedAmount = 0, //final amount
                                Narration = "Income (Loss) on Housing Properties (Self Occupied) under section24 - housing interest",
                                //   DBTrack = dbt,
                                PickupId = 61
                            };
                            //add list of it projection data
                            OITProjectionDataList.Add(OITProjectionSave);
                        }

                        ITSection24Payment ONormalSection24Tot = OEmpIT24Investment.FirstOrDefault();

                        // for new tax slab start 28122020
                        Calendar temp_OFinancialYear1 = db.Calendar.Where(e => e.Id == OFinancialYear).SingleOrDefault();
                        DateTime finfrm1 = Convert.ToDateTime("01/04/2020");
                        if (Regimischemecurryear != null)
                        {
                            if (Regimischemecurryear.Scheme.LookupVal.ToUpper() == "NEWTAX")
                            {
                                mFinDeclInvst = 0;
                                mFinActInvst = 0;
                                mFinActQualify = 0;
                                mFinDeclQualify = 0;
                            }
                        }
                        else
                        {


                            if (temp_OFinancialYear.FromDate >= finfrm1 && OEmployeePayroll.Employee.EmpOffInfo.NationalityID.No2 == "Yes")
                            {
                                mFinDeclInvst = 0;
                                mFinActInvst = 0;
                                mFinActQualify = 0;
                                mFinDeclQualify = 0;
                            }
                        }

                        // for new tax slab end 28122020

                        //if (mFinActInvst > ONormalSection24Tot.ITSection.ExemptionLimit)
                        //{
                        //    mFinActInvst = ONormalSection24Tot.ITSection.ExemptionLimit;
                        //    mFinActQualify = ONormalSection24Tot.ITSection.ExemptionLimit;
                        //}
                        //else
                        //{
                        //    mFinActQualify = mFinActInvst;
                        //}
                        //if (mFinDeclInvst > ONormalSection24Tot.ITSection.ExemptionLimit)
                        //{
                        //    mFinDeclInvst = ONormalSection24Tot.ITSection.ExemptionLimit;
                        //    mFinDeclQualify = ONormalSection24Tot.ITSection.ExemptionLimit;
                        //}
                        //else
                        //{
                        //    //mFinDeclQualify = mFinActInvst;
                        //    mFinDeclQualify = mFinDeclInvst;
                        //}
                        ItProjectionTempClass OITProjectionSave1 = new ItProjectionTempClass
                        {
                            FinancialYear = OFinancialYear,//modified on 15042017 by prashant
                            Tiltle = "",
                            //Section = ONormalSection24Tot.ITSection.ITSectionList.LookupVal.ToUpper(),
                            //SectionType = ONormalSection24Tot.ITSection.ITSectionListType.LookupVal.ToUpper(),
                            Section = ONormalSection24Tot.ITSection == null ? "" : ONormalSection24Tot.ITSection.ITSectionList == null ? "" : ONormalSection24Tot.ITSection.ITSectionList.LookupVal.ToUpper(),
                            SectionType = ONormalSection24Tot.ITSection == null ? "" : ONormalSection24Tot.ITSection.ITSectionListType == null ? "" : ONormalSection24Tot.ITSection.ITSectionListType.LookupVal.ToUpper(),

                            ChapterName = "Total Qualifing Amount under 24",

                            ProjectedAmount = mFinDeclInvst,
                            ActualAmount = mFinActInvst,
                            ActualQualifyingAmount = mFinActQualify,
                            ProjectedQualifyingAmount = mFinDeclQualify,
                            //  ProjectionDate = DateTime.Now.Date,

                            Narration = "Income(Loss) Hosing Property (Self Ocuupied) under section24",
                            // DBTrack = dbt,
                            PickupId = 62
                        };
                        OITProjectionDataList.Add(OITProjectionSave1);
                    }
                    else
                    {
                        foreach (ITSection24Payment ca in OEmpIT24Investment)
                        {
                            mTotActInvst = 0;
                            mTotDeclInvst = 0;
                            mTotActInvst = mTotActInvst + ca.ActualInterest;
                            mTotDeclInvst = mTotDeclInvst + ca.DeclaredInterest;
                            if (ca.ActualInterest > ca.ITSection.ExemptionLimit)
                            {
                                mTotActQualify = ca.ITSection.ExemptionLimit;
                            }
                            else
                            {
                                mTotActQualify = mTotActInvst;
                            }
                            if (ca.DeclaredInterest > ca.ITSection.ExemptionLimit)
                            {
                                mTotDeclQualify = ca.ITSection.ExemptionLimit;
                            }
                            else
                            {
                                //  mTotDeclQualify = mTotActInvst;
                                mTotDeclQualify = mTotDeclInvst;
                            }

                            mFinActInvst = mFinActInvst + mTotActInvst;
                            mFinDeclInvst = mFinDeclInvst + mTotDeclInvst;
                            mFinActQualify = mFinActQualify + mTotActQualify;
                            mFinDeclQualify = mFinDeclQualify + mTotDeclQualify;

                            // for new tax slab start 28122020
                            double declint = 0;
                            double Actint = 0;
                            DateTime finfrm = Convert.ToDateTime("01/04/2020");
                            if (Regimischemecurryear != null)
                            {
                                if (Regimischemecurryear.Scheme.LookupVal.ToUpper() == "NEWTAX")
                                {
                                    mFinActInvst = 0;
                                    mFinDeclInvst = 0;
                                    mTotActQualify = 0;
                                    mTotDeclQualify = 0;
                                    declint = 0;
                                    Actint = 0;
                                }
                                else
                                {
                                    declint = ca.DeclaredInterest;
                                    Actint = ca.ActualInterest;

                                }
                            }
                            else
                            {


                                if (temp_OFinancialYear.FromDate >= finfrm && OEmployeePayroll.Employee.EmpOffInfo.NationalityID.No2 == "Yes")
                                {
                                    mFinActInvst = 0;
                                    mFinDeclInvst = 0;
                                    mTotActQualify = 0;
                                    mTotDeclQualify = 0;
                                    declint = 0;
                                    Actint = 0;
                                }
                                else
                                {
                                    declint = ca.DeclaredInterest;
                                    Actint = ca.ActualInterest;
                                }
                            }
                            // for new tax slab end 28122020


                            ItProjectionTempClass OITProjectionSave = new ItProjectionTempClass
                            {
                                FinancialYear = OFinancialYear,
                                Tiltle = "",
                                Section = ca.ITSection == null ? "" : ca.ITSection.ITSectionList == null ? "" : ca.ITSection.ITSectionList.LookupVal.ToUpper(),
                                SectionType = ca.ITSection == null ? "" : ca.ITSection.ITSectionListType == null ? "" : ca.ITSection.ITSectionListType.LookupVal.ToUpper(),
                                ChapterName = ca.LoanAdvanceHead == null ? ca.PaymentName : ca.LoanAdvanceHead.Name,// "CHAPTER Loan",
                                //SubChapter = ca.ITSection.ITSectionList.LookupVal.ToUpper(),
                                ////SalaryHeadName="",
                                //ProjectedAmount = ca.DeclaredInterest,
                                //ActualAmount = ca.ActualInterest,
                                ProjectedAmount = declint,
                                ActualAmount = Actint,
                                ActualQualifyingAmount = mTotActQualify,
                                ProjectedQualifyingAmount = mTotDeclQualify,
                                //   ProjectionDate = DateTime.Now.Date,
                                TDSComponents = 0,
                                QualifiedAmount = 0, //final amount
                                Narration = "Income (Loss) on Housing Properties (Self Occupied) under section24 - housing interest",
                                //   DBTrack = dbt,
                                PickupId = 61
                            };
                            //add list of it projection data
                            OITProjectionDataList.Add(OITProjectionSave);
                        }
                        ITSection24Payment ONormalSection24Tot = OEmpIT24Investment.FirstOrDefault();
                        if (mFinActInvst > ONormalSection24Tot.ITSection.ExemptionLimit)
                        {
                            mFinActInvst = ONormalSection24Tot.ITSection.ExemptionLimit;
                            mFinActQualify = ONormalSection24Tot.ITSection.ExemptionLimit;
                        }
                        else
                        {
                            mFinActQualify = mFinActInvst;
                        }
                        if (mFinDeclInvst > ONormalSection24Tot.ITSection.ExemptionLimit)
                        {
                            mFinDeclInvst = ONormalSection24Tot.ITSection.ExemptionLimit;
                            mFinDeclQualify = ONormalSection24Tot.ITSection.ExemptionLimit;
                        }
                        else
                        {
                            //mFinDeclQualify = mFinActInvst;
                            mFinDeclQualify = mFinDeclInvst;
                        }
                        // for new tax slab start 28122020
                        Calendar temp_OFinancialYear1 = db.Calendar.Where(e => e.Id == OFinancialYear).SingleOrDefault();
                        DateTime finfrm1 = Convert.ToDateTime("01/04/2020");
                        if (Regimischemecurryear != null)
                        {
                            if (Regimischemecurryear.Scheme.LookupVal.ToUpper() == "NEWTAX")
                            {
                                mFinDeclInvst = 0;
                                mFinActInvst = 0;
                                mFinActQualify = 0;
                                mFinDeclQualify = 0;
                            }
                        }
                        else
                        {


                            if (temp_OFinancialYear.FromDate >= finfrm1 && OEmployeePayroll.Employee.EmpOffInfo.NationalityID.No2 == "Yes")
                            {
                                mFinDeclInvst = 0;
                                mFinActInvst = 0;
                                mFinActQualify = 0;
                                mFinDeclQualify = 0;
                            }
                        }

                        // for new tax slab end 28122020

                        ItProjectionTempClass OITProjectionSave1 = new ItProjectionTempClass
                        {
                            FinancialYear = OFinancialYear,//modified on 15042017 by prashant
                            Tiltle = "",
                            //Section = ONormalSection24Tot.ITSection.ITSectionList.LookupVal.ToUpper(),
                            //SectionType = ONormalSection24Tot.ITSection.ITSectionListType.LookupVal.ToUpper(),
                            Section = ONormalSection24Tot.ITSection == null ? "" : ONormalSection24Tot.ITSection.ITSectionList == null ? "" : ONormalSection24Tot.ITSection.ITSectionList.LookupVal.ToUpper(),
                            SectionType = ONormalSection24Tot.ITSection == null ? "" : ONormalSection24Tot.ITSection.ITSectionListType == null ? "" : ONormalSection24Tot.ITSection.ITSectionListType.LookupVal.ToUpper(),

                            ChapterName = "Total Qualifing Amount under 24",

                            ProjectedAmount = mFinDeclInvst,
                            ActualAmount = mFinActInvst,
                            ActualQualifyingAmount = mFinActQualify,
                            ProjectedQualifyingAmount = mFinDeclQualify,
                            //  ProjectionDate = DateTime.Now.Date,

                            Narration = "Income(Loss) Hosing Property (Self Ocuupied) under section24",
                            // DBTrack = dbt,
                            PickupId = 62
                        };
                        OITProjectionDataList.Add(OITProjectionSave1);
                    }
                }
                return OITProjectionDataList;
                //}
            }
        }


        public static List<ItProjectionTempClass> InvestmentSection24Property(EmployeePayroll OEmployeePayroll, IncomeTax OITMaster,
            List<ITSalaryHeadData> OITSalMonthwise, Int32 OFinancialYear)
        {
            //Utility.DumpProcessStatus("InvestmentSection24Property");
            DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };
            //using (DataBaseContext db = new DataBaseContext())
            //{
            //*****************Loan Section*********************//
            List<ItProjectionTempClass> OITProjectionDataList = new List<ItProjectionTempClass>();
            using (DataBaseContext db = new DataBaseContext())
            {
                var Regimi = db.EmployeePayroll.Include(e => e.RegimiScheme).Include(e => e.RegimiScheme.Select(x => x.FinancialYear)).Include(e => e.RegimiScheme.Select(y => y.Scheme)).Where(e => e.Id == OEmployeePayroll.Id).FirstOrDefault();
                var Regimischemecurryear = Regimi.RegimiScheme.Where(e => e.FinancialYear.Id == OFinancialYear).FirstOrDefault();

                EmployeePayroll OEmpITSection24EmpData = db.EmployeePayroll.Where(e => e.Id == OEmployeePayroll.Id).SingleOrDefault();
                List<ITSection24Payment> ITSection24Payment = db.ITSection24Payment.Where(e => e.EmployeePayroll_Id == OEmpITSection24EmpData.Id && e.FinancialYear_Id == OFinancialYear).ToList();//modofied on 26/10/2022 by prashant for fy

                foreach (var ITSection24Paymentitem in ITSection24Payment)
                {
                    Calendar FinancialYear = db.Calendar.Where(e => e.Id == ITSection24Paymentitem.FinancialYear_Id).SingleOrDefault();
                    ITSection24Paymentitem.FinancialYear = FinancialYear;
                    ITSection ITSection = db.ITSection.Include(x => x.LoanAdvanceHead).Where(e => e.Id == ITSection24Paymentitem.ITSection_Id).SingleOrDefault();
                    ITSection24Paymentitem.ITSection = ITSection;
                    LookupValue ITSectionList = db.LookupValue.Where(e => e.Id == ITSection.ITSectionList_Id).SingleOrDefault();
                    ITSection24Paymentitem.ITSection.ITSectionList = ITSectionList;
                    LookupValue ITSectionListType = db.LookupValue.Where(e => e.Id == ITSection.ITSectionListType_Id).SingleOrDefault();
                    ITSection24Paymentitem.ITSection.ITSectionListType = ITSectionListType;
                    List<LoanAdvanceHead> ITSectionLoanAdvanceHead = db.ITSection.Where(e => e.Id == ITSection24Paymentitem.ITSection_Id).Select(e => e.LoanAdvanceHead.ToList()).SingleOrDefault();
                    ITSection24Paymentitem.ITSection.LoanAdvanceHead = ITSectionLoanAdvanceHead;

                    //List<ITSubInvestmentPayment> ITSubInvestmentPayment = db.ITInvestmentPayment.Where(e => e.Id == ITSection24Paymentitem.ITInvestment_Id).Select(e => e.ITSubInvestmentPayment.ToList()).SingleOrDefault();
                    //ITSection24Paymentitem.ITSubInvestmentPayment = ITSubInvestmentPayment;
                    LoanAdvanceHead LoanAdvanceHead = db.LoanAdvanceHead.Where(e => e.Id == ITSection24Paymentitem.LoanAdvanceHead_Id).SingleOrDefault();
                    ITSection24Paymentitem.LoanAdvanceHead = LoanAdvanceHead;
                    //List<ITSection> LoanITSection = db.LoanAdvanceHead.Where(e => e.Id == ITInvestmentPaymentitem.LoanAdvanceHead_Id).Select(e => e.ITSection.ToList()).SingleOrDefault();
                    //LoanAdvanceHead.ITSection = LoanITSection;
                    //List<LoanAdvancePolicy> LoanAdvancePolicy = db.LoanAdvanceHead.Where(e => e.Id == ITInvestmentPaymentitem.LoanAdvanceHead_Id).Select(e => e.LoanAdvancePolicy.ToList()).SingleOrDefault();
                    //LoanAdvanceHead.LoanAdvancePolicy = LoanAdvancePolicy;
                    //SalaryHead SalaryHead = db.SalaryHead.Where(e => e.Id == LoanAdvanceHead.SalaryHead_Id).SingleOrDefault();
                    //LoanAdvanceHead.SalaryHead = SalaryHead;
                }
                OEmpITSection24EmpData.ITSection24Payment = ITSection24Payment;
                //var Fincalendar = db.Calendar.Where(e => e.Id == OFinancialYear).SingleOrDefault();
                //var mFromPeriod = Fincalendar.FromDate.Value;
                //var mToPeriod = Fincalendar.ToDate.Value;
                //List<string> mPeriodYear = new List<string>();
                //string mPeriodRangeYear = "";
                //for (DateTime mTempDate = mFromPeriod; mTempDate <= mToPeriod; mTempDate = mTempDate.AddMonths(1))
                //{
                //    if (mPeriodRangeYear == "")
                //    {
                //        mPeriodRangeYear = Convert.ToDateTime(mTempDate).ToString("MM/yyyy");
                //        mPeriodYear.Add(Convert.ToDateTime(mTempDate).ToString("MM/yyyy"));
                //    }
                //    else
                //    {
                //        mPeriodRangeYear = mPeriodRangeYear + "," + Convert.ToDateTime(mTempDate).ToString("MM/yyyy");
                //        mPeriodYear.Add(Convert.ToDateTime(mTempDate).ToString("MM/yyyy"));
                //    }
                //}
                List<LoanAdvRequest> LoanAdvRequest = db.LoanAdvRequest.Where(e => e.EmployeePayroll_Id == OEmpITSection24EmpData.Id).ToList();

                foreach (var LoanAdvRequestitem in LoanAdvRequest)
                {
                    LoanAdvanceHead LoanAdvanceHead = db.LoanAdvanceHead.Where(e => e.Id == LoanAdvRequestitem.LoanAdvanceHead_Id).SingleOrDefault();
                    LoanAdvRequestitem.LoanAdvanceHead = LoanAdvanceHead;
                    ITLoan ITLoan = db.ITLoan.Where(e => e.Id == LoanAdvanceHead.ITLoan_Id).SingleOrDefault();
                    LoanAdvRequestitem.LoanAdvanceHead.ITLoan = ITLoan;
                    SalaryHead SalaryHead = db.SalaryHead.Where(e => e.Id == LoanAdvanceHead.SalaryHead_Id).SingleOrDefault();
                    LoanAdvRequestitem.LoanAdvanceHead.SalaryHead = SalaryHead;
                    DateTime mdate = new DateTime();
                    //newly modified for period selection
                    //List<LoanAdvRepaymentT> LoanAdvRepaymentT = db.LoanAdvRequest.Where(e => e.Id == LoanAdvRequestitem.Id).Select(e => e.LoanAdvRepaymentT.Where(r => mPeriodYear.Contains(r.PayMonth)).ToList()).SingleOrDefault();
                    //LoanAdvRequestitem.LoanAdvRepaymentT = LoanAdvRepaymentT;
                }
                OEmpITSection24EmpData.LoanAdvRequest = LoanAdvRequest;

                //   .Include(e => e.ITSection24Payment)
                //.Include(e => e.ITSection24Payment.Select(r => r.FinancialYear))
                //.Include(e => e.ITSection24Payment.Select(r => r.ITSection))
                //.Include(e => e.ITSection24Payment.Select(r => r.ITSection.ITSectionList))
                //.Include(e => e.ITSection24Payment.Select(r => r.ITSection.ITSectionListType))
                //.Include(e => e.ITSection24Payment.Select(r => r.ITSection.ITSectionListType))

                //.Include(e => e.ITSection24Payment.Select(r => r.ITSection.LoanAdvanceHead))

                //.Include(e => e.ITSection24Payment.Select(r => r.LoanAdvanceHead))
                //.Include(e => e.LoanAdvRequest)
                //.Include(e => e.LoanAdvRequest.Select(r => r.LoanAdvanceHead))
                //.Include(e => e.LoanAdvRequest.Select(r => r.LoanAdvanceHead.ITLoan))
                //.Include(e => e.LoanAdvRequest.Select(r => r.LoanAdvanceHead.SalaryHead))


                //  var OITSection = OITMaster.ITSection.ToList();

                ITSection OITSection24 = OITMaster.ITSection.Where(e => e.ITSectionList.LookupVal.ToUpper() == "SECTION24" &&
                     e.ITSectionListType.LookupVal.ToUpper() == "PROPERTY").SingleOrDefault();

                //var OITInvestments = OITSection24.ITInvestments.ToList();
                List<ITSection24Payment> OEmpIT24Investment = OEmpITSection24EmpData.ITSection24Payment.Where(e => e.FinancialYear.Id == OFinancialYear &&
                    e.ITSection.ITSectionListType.LookupVal.ToUpper() == "PROPERTY").ToList();

                double mTotActInvst = 0;
                double mTotDeclInvst = 0;
                double mTotActQualify = 0;
                double mTotDeclQualify = 0;
                //check all investments
                //    var ONormalSection24 = OEmpIT24Investment.ToList();

                if (OEmpIT24Investment != null && OEmpIT24Investment.Count() > 0)
                {

                    //{
                    //    ITProjection OITProjectionSave = new ItProjectionTempClass
                    //    {
                    //        Tiltle = "Section24 - Housing Property",
                    //        FinancialYear = db.Calendar.Where(e=>e.Id==OFinancialYear).SingleOrDefault(),
                    //        FromPeriod = null,
                    //        ToPeriod = null,
                    //        Section = "",
                    //        SectionType = "",
                    //        ChapterName = "",
                    //        SubChapter = "",
                    //        ProjectedAmount = 0,
                    //        ActualAmount = 0,
                    //        ActualQualifyingAmount = 0,
                    //        ProjectedQualifyingAmount = 0,
                    //        ProjectionDate = DateTime.Now.Date,
                    //        TDSComponents = 0,
                    //        QualifiedAmount = 0,
                    //        Narration = "Housing Property  under section 24 ",
                    //        DBTrack = dbt
                    //    };
                    //    OITProjectionDataList.Add(OITProjectionSave);
                    //}

                    foreach (ITSection24Payment ca in OEmpIT24Investment)
                    {

                        mTotActInvst = mTotActInvst + ca.ActualInterest;
                        mTotDeclInvst = mTotDeclInvst + ca.DeclaredInterest;
                        if (ca.ActualInterest > ca.ITSection.ExemptionLimit)
                        {
                            mTotActQualify = ca.ITSection.ExemptionLimit;
                        }
                        else
                        {
                            mTotActQualify = mTotActInvst;
                        }
                        if (ca.DeclaredInterest > ca.ITSection.ExemptionLimit)
                        {
                            mTotDeclQualify = ca.ITSection.ExemptionLimit;
                        }
                        else
                        {
                            mTotDeclQualify = mTotActInvst;
                        }

                        // for new tax slab start 28122020
                        double declint = 0;
                        double Actint = 0;
                        Calendar temp_OFinancialYear = db.Calendar.Where(e => e.Id == OFinancialYear).SingleOrDefault();
                        DateTime finfrm = Convert.ToDateTime("01/04/2020");
                        if (Regimischemecurryear != null)
                        {
                            if (Regimischemecurryear.Scheme.LookupVal.ToUpper() == "NEWTAX")
                            {
                                mTotActQualify = 0;
                                mTotDeclQualify = 0;
                                declint = 0;
                                Actint = 0;
                            }
                            else
                            {
                                declint = ca.DeclaredInterest;
                                Actint = ca.ActualInterest;

                            }
                        }
                        else
                        {


                            if (temp_OFinancialYear.FromDate >= finfrm && OEmployeePayroll.Employee.EmpOffInfo.NationalityID.No2 == "Yes")
                            {
                                mTotActQualify = 0;
                                mTotDeclQualify = 0;
                                declint = 0;
                                Actint = 0;
                            }
                            else
                            {
                                declint = ca.DeclaredInterest;
                                Actint = ca.ActualInterest;
                            }
                        }
                        // for new tax slab end 28122020


                        ItProjectionTempClass OITProjectionSave = new ItProjectionTempClass
                        {
                            FinancialYear = OFinancialYear,
                            Tiltle = "",
                            Section = ca.ITSection.ITSectionList.LookupVal.ToUpper(),
                            SectionType = ca.ITSection.ITSectionListType.LookupVal.ToUpper(),
                            ChapterName = ca.PaymentName,

                            //ProjectedAmount = ca.DeclaredInterest,
                            //ActualAmount = ca.ActualInterest,
                            ProjectedAmount = declint,
                            ActualAmount = Actint,
                            ActualQualifyingAmount = mTotActQualify,
                            ProjectedQualifyingAmount = mTotDeclQualify,
                            //  ProjectionDate = DateTime.Now.Date,
                            TDSComponents = 0,
                            QualifiedAmount = 0, //final amount
                            Narration = "Income From Housing Property (Let Out) section 24- Property",
                            //   DBTrack = dbt,
                            PickupId = 71
                        };
                        //add list of it projection data
                        OITProjectionDataList.Add(OITProjectionSave);
                    }
                    ITSection24Payment ONormalSection24Tot = OEmpIT24Investment.SingleOrDefault();
                    if (mTotActInvst > ONormalSection24Tot.ITSection.ExemptionLimit)
                    {
                        mTotActQualify = ONormalSection24Tot.ITSection.ExemptionLimit;
                    }
                    else
                    {
                        mTotActQualify = mTotActInvst;
                    }
                    if (mTotDeclInvst > ONormalSection24Tot.ITSection.ExemptionLimit)
                    {
                        mTotDeclQualify = ONormalSection24Tot.ITSection.ExemptionLimit;
                    }
                    else
                    {
                        mTotDeclQualify = mTotActInvst;
                    }
                    // for new tax slab start 28122020
                    Calendar temp_OFinancialYear1 = db.Calendar.Where(e => e.Id == OFinancialYear).SingleOrDefault();
                    DateTime finfrm1 = Convert.ToDateTime("01/04/2020");
                    if (Regimischemecurryear != null)
                    {
                        if (Regimischemecurryear.Scheme.LookupVal.ToUpper() == "NEWTAX")
                        {
                            mTotDeclInvst = 0;
                            mTotActInvst = 0;
                            mTotActQualify = 0;
                            mTotDeclQualify = 0;
                        }
                    }
                    else
                    {


                        if (temp_OFinancialYear1.FromDate >= finfrm1 && OEmployeePayroll.Employee.EmpOffInfo.NationalityID.No2 == "Yes")
                        {
                            mTotDeclInvst = 0;
                            mTotActInvst = 0;
                            mTotActQualify = 0;
                            mTotDeclQualify = 0;
                        }
                    }
                    // for new tax slab end 28122020

                    ItProjectionTempClass OITProjectionSave1 = new ItProjectionTempClass
                    {
                        FinancialYear = OFinancialYear,
                        Tiltle = "",
                        Section = ONormalSection24Tot.ITSection.ITSectionList.LookupVal.ToUpper(),
                        SectionType = ONormalSection24Tot.ITSection.ITSectionListType.LookupVal.ToUpper(),
                        ChapterName = "Total of Income form House Property (Let Out)",

                        ProjectedAmount = mTotDeclInvst,
                        ActualAmount = mTotActInvst,
                        ActualQualifyingAmount = mTotActQualify,
                        ProjectedQualifyingAmount = mTotDeclQualify,
                        //   ProjectionDate = DateTime.Now.Date,

                        Narration = "Housing Property (Let Out) under Section24 - Income",
                        //   DBTrack = dbt,
                        PickupId = 72
                    };
                    OITProjectionDataList.Add(OITProjectionSave1);
                }
                return OITProjectionDataList;
                //}
            }
        }
        public static List<ItProjectionTempClass> ReliefSection89(EmployeePayroll OEmployeePayroll, IncomeTax OITMaster,
            Int32 OFinancialYear)
        {
            DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };
            //using (DataBaseContext db = new DataBaseContext())
            //{
            List<ItProjectionTempClass> OITProjectionDataList = new List<ItProjectionTempClass>();
            //  var OITSection = OITMaster.ITSection.ToList();

            List<ITSection> OITSection89 = OITMaster.ITSection.Where(e => e.ITSectionListType.LookupVal.ToUpper() == "RELIEF").ToList();
            using (DataBaseContext db = new DataBaseContext())
            {
                //var OITInvestments=OITSection89.LoanAdvanceHead.ToList();
                //var OITSubInvestments=OITInvestments.Select(e=>e.ITSubInvestment).ToList();
                EmployeePayroll OEmpPay = db.EmployeePayroll
                            .Include(e => e.ITReliefPayment)
                            .Include(e => e.ITReliefPayment.Select(r => r.FinancialYear))
                            .Include(e => e.ITReliefPayment.Select(r => r.ITSection))
                         .Where(e => e.Id == OEmployeePayroll.Id).FirstOrDefault();//.AsParallel().SingleOrDefault();

                if (OEmpPay.ITReliefPayment.Count() > 0)
                {
                    List<ITReliefPayment> OEmpITReliefData = OEmpPay.ITReliefPayment.Where(e => e.FinancialYear.Id == OFinancialYear).ToList();
                    double mTotReliefInvst = 0;

                    double mTotReliefQualify = 0;

                    //check all investments
                    //   var ONormalSection89 = OEmpITReliefData.ToList();
                    if (OEmpITReliefData.Count() > 0)
                    {
                        foreach (ITReliefPayment ca in OEmpITReliefData)
                        {

                            mTotReliefInvst = mTotReliefInvst + ca.ReliefAmount;

                            if (ca.ReliefAmount > ca.ITSection.ExemptionLimit)
                            {
                                mTotReliefQualify = ca.ITSection.ExemptionLimit;
                            }
                            else
                            {
                                mTotReliefQualify = mTotReliefInvst;
                            }

                            ItProjectionTempClass OITProjectionSave = new ItProjectionTempClass
                            {
                                FinancialYear = OFinancialYear,
                                Tiltle = "",
                                Section = "",
                                SectionType = "",
                                ChapterName = "    " + ca.PaymentName,

                                ProjectedAmount = ca.ReliefAmount,
                                ActualAmount = ca.ReliefAmount,
                                ActualQualifyingAmount = 0,
                                ProjectedQualifyingAmount = 0,
                                //    ProjectionDate = DateTime.Now.Date,

                                Narration = "Relief under section 89",
                                //     DBTrack = dbt,
                                PickupId = 138
                            };
                            //add list of it projection data
                            OITProjectionDataList.Add(OITProjectionSave);
                        }
                    }

                    ITReliefPayment ONormalSection89Tot = OEmpITReliefData.SingleOrDefault();
                    if (mTotReliefInvst > ONormalSection89Tot.ITSection.ExemptionLimit)
                    {
                        mTotReliefQualify = ONormalSection89Tot.ITSection.ExemptionLimit;
                    }
                    else
                    {
                        mTotReliefQualify = mTotReliefInvst;
                    }

                    ItProjectionTempClass OITProjectionSave1 = new ItProjectionTempClass
                    {
                        FinancialYear = OFinancialYear,
                        Tiltle = "",
                        Section = "",
                        SectionType = "",
                        ChapterName = "Total of Relief Amount",

                        ProjectedAmount = mTotReliefQualify,
                        ActualAmount = mTotReliefQualify,
                        ActualQualifyingAmount = 0,
                        ProjectedQualifyingAmount = 0,
                        //  ProjectionDate = DateTime.Now.Date,

                        Narration = "Relief under section89",
                        //   DBTrack = dbt,
                        PickupId = 139
                    };
                    OITProjectionDataList.Add(OITProjectionSave1);
                }
                return OITProjectionDataList;
                // }
            }
        }
        public static List<ItProjectionTempClass> RebateSection87(EmployeePayroll OEmployeePayroll, IncomeTax OITMaster,
            Int32 OFinancialYear, double mITIncomeAct, double mITIncomeProj, double mTotalTDSAct, double mTotalTDSProj)
        {

            //Utility.DumpProcessStatus("RebateSection87");
            DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };
            //using (DataBaseContext db = new DataBaseContext())
            //{
            List<ItProjectionTempClass> OITProjectionDataList = new List<ItProjectionTempClass>();
            // var OITSection = OITMaster.ITSection.ToList();

            ITSection OITSection87 = OITMaster.ITSection.Where(e => e.ITSectionList.LookupVal.ToUpper() == "SECTION87").SingleOrDefault();
            // for new tax slab start 12/04/2023
            List<ITStandardITRebate> OITSection87List = new List<ITStandardITRebate>();
            using (DataBaseContext db1 = new DataBaseContext())
            {
                Calendar temp_OFinancialYear = db1.Calendar.Where(e => e.Id == OFinancialYear).SingleOrDefault();
                DateTime finfrm = Convert.ToDateTime("01/04/2023");
                var Regimi = db1.EmployeePayroll.Include(e => e.RegimiScheme).Include(e => e.RegimiScheme.Select(x => x.FinancialYear)).Include(e => e.RegimiScheme.Select(y => y.Scheme)).Where(e => e.Id == OEmployeePayroll.Id).FirstOrDefault();
                var Regimischemecurryear = Regimi.RegimiScheme.Where(e => e.FinancialYear.Id == temp_OFinancialYear.Id).FirstOrDefault();
                if (Regimischemecurryear != null)
                {
                    if (Regimischemecurryear.Scheme.LookupVal.ToUpper() == "NEWTAX")
                    {
                        OITSection87List = OITSection87.ITStandardITRebate.Where(e => e.Regime.LookupVal.ToUpper() == "NEWTAX").OrderBy(e => e.StartAmount).ToList();
                    }
                    else
                    {
                        OITSection87List = OITSection87.ITStandardITRebate.Where(e => e.Regime.LookupVal.ToUpper() == "OLDTAX").OrderBy(e => e.StartAmount).ToList();
                    }
                }
                else
                {


                    if (temp_OFinancialYear.FromDate >= finfrm && OEmployeePayroll.Employee.EmpOffInfo.NationalityID.No2 == "Yes")
                    {
                        OITSection87List = OITSection87.ITStandardITRebate.Where(e => e.Regime.LookupVal.ToUpper() == "NEWTAX").OrderBy(e => e.StartAmount).ToList();
                    }
                    else
                    {
                        OITSection87List = OITSection87.ITStandardITRebate.Where(e => e.Regime.LookupVal.ToUpper() == "OLDTAX").OrderBy(e => e.StartAmount).ToList();
                    }
                }
            }
            // for new tax slab end 12/04/2023
            //check for salary depend investments
            double mActItax = 0;
            double mDeclItax = 0;

            if (OITSection87List.Count() > 0 && OITSection87List != null)
            {
                foreach (ITStandardITRebate ca in OITSection87List)
                {
                    if (mITIncomeAct > ca.StartAmount && mITIncomeAct <= ca.EndAmount)
                    {
                        mActItax = ca.DeductionAmount + Math.Round((mITIncomeAct * ca.DeductionPerc / 100), 0);
                    }
                    if (mITIncomeProj > ca.StartAmount && mITIncomeProj <= ca.EndAmount)
                    {
                        mDeclItax = ca.DeductionAmount + Math.Round((mITIncomeProj * ca.DeductionPerc / 100), 0);
                    }
                }
                if (mActItax > mTotalTDSAct) mActItax = mTotalTDSAct;
                if (mDeclItax > mTotalTDSProj) mDeclItax = mTotalTDSProj;


                using (DataBaseContext db = new DataBaseContext())
                {

                    ItProjectionTempClass OITProjectionSave = new ItProjectionTempClass
                    {
                        FinancialYear = OFinancialYear,
                        Tiltle = "",
                        Section = OITSection87.ITSectionList.LookupVal.ToUpper(),
                        SectionType = "",
                        ChapterName = "    " + OITSection87.ITSectionList.LookupVal.ToUpper(),

                        ProjectedAmount = mDeclItax,
                        ActualAmount = mActItax,
                        ActualQualifyingAmount = 0,
                        ProjectedQualifyingAmount = 0,
                        //   ProjectionDate = DateTime.Now.Date,

                        Narration = "Income Tax Rebate- Section-87 ",
                        //   DBTrack = dbt,
                        PickupId = 132

                    };
                    //add list of it projection data
                    OITProjectionDataList.Add(OITProjectionSave);
                }

                // }
            }
            return OITProjectionDataList;
        }
        public static void ITForm16PartB(int OEmployeePayrollId, int mCompanyId, Int32 OFinancialYear,
            DateTime mFromPeriod, DateTime mToPeriod, DateTime ProcessDate, int SigningPerson)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                EmployeePayroll OEmployeePayroll = db.EmployeePayroll.Where(d => d.Id == OEmployeePayrollId).SingleOrDefault();
                Employee Employee = db.Employee.Where(e => e.Id == OEmployeePayroll.Employee_Id).SingleOrDefault();
                OEmployeePayroll.Employee = Employee;
                List<ITProjection> ITProjection = db.ITProjection.Where(e => e.EmployeePayroll_Id == OEmployeePayroll.Id && e.FinancialYear_Id == OFinancialYear).ToList();
                OEmployeePayroll.ITProjection = ITProjection;
                ITForm16Data ITForm16Data = db.ITForm16Data.Where(e => e.EmployeePayroll_Id == OEmployeePayroll.Id && e.FinancialYear_Id == OFinancialYear).SingleOrDefault();
                if (ITForm16Data != null)
                {
                    OEmployeePayroll.ITForm16Data.Add(ITForm16Data);
                    List<ITForm16DataDetails> ITForm16DataDetails = db.ITForm16DataDetails.Where(e => e.ITForm16Data_Id == ITForm16Data.Id).ToList();
                    ITForm16Data.ITForm16DataDetails = ITForm16DataDetails;
                }

                List<ITChallanEmpDetails> ITChallanEmpDetails = db.ITChallanEmpDetails.Where(e => e.Calendar_Id == OFinancialYear && e.EmployeePayroll_Id == OEmployeePayroll.Id).ToList();
                OEmployeePayroll.ITChallanEmpDetails = ITChallanEmpDetails;

                //.Include(e => e.Employee)
                //          .Include(e => e.ITProjection).Include(e => e.ITForm16Data)
                //          .Include(e => e.ITForm16Data.Select(r => r.ITForm16DataDetails))
                //       .Include(e => e.ITChallanEmpDetails)


                CompanyPayroll OCompanyPayroll = db.CompanyPayroll
                       .Include(e => e.Company)
                       .Where(r => r.Company.Id == mCompanyId).SingleOrDefault();


                CompanyPayroll OIncomeTax = db.CompanyPayroll.Where(e => e.Company.Id == mCompanyId).SingleOrDefault();
                IncomeTax IncomeTax = db.IncomeTax.Include(y => y.ITSection).Include(y => y.ITSection.Select(e => e.LoanAdvanceHead)).Include(y => y.ITTDS).Where(e => e.CompanyPayroll_Id == OIncomeTax.Id && e.FyCalendar_Id == OFinancialYear).SingleOrDefault();
                OIncomeTax.IncomeTax.Add(IncomeTax);
                Calendar FyCalendar = db.Calendar.Where(e => e.Id == IncomeTax.FyCalendar_Id).SingleOrDefault();
                IncomeTax.FyCalendar = FyCalendar;
                List<ITTDS> ITTDS = db.IncomeTax.Where(e => e.Id == IncomeTax.Id).Select(e => e.ITTDS.ToList()).SingleOrDefault();

                foreach (var ITTDSitem in ITTDS)
                {
                    LookupValue Category = db.LookupValue.Where(e => e.Id == ITTDSitem.Category_Id).SingleOrDefault();
                    ITTDSitem.Category = Category;
                }
                IncomeTax.ITTDS = ITTDS;
                List<ITSection> ITSection = db.IncomeTax.Where(e => e.Id == IncomeTax.Id).Select(e => e.ITSection.ToList()).SingleOrDefault();

                foreach (var ITSectionitem in ITSection)
                {
                    List<ITInvestment> ITInvestments = db.ITInvestment.Where(e => e.ITSection_Id == ITSectionitem.Id).ToList();//modofied on 26/10/2022 by prashant for fy

                    foreach (var ITInvestmentsitem in ITInvestments)
                    {
                        List<ITSubInvestment> ITSubInvestment = db.ITSubInvestment.Where(e => e.ITInvestment_Id == ITInvestmentsitem.Id).ToList();
                        ITInvestmentsitem.ITSubInvestment = ITSubInvestment;
                        SalaryHead SalaryHead1 = db.SalaryHead.Where(e => e.Id == ITInvestmentsitem.SalaryHead_Id).SingleOrDefault();
                        ITInvestmentsitem.SalaryHead = SalaryHead1;
                    }
                    ITSectionitem.ITInvestments = ITInvestments;
                    List<LoanAdvanceHead> LoanAdvanceHead = db.ITSection.Where(e => e.Id == ITSectionitem.Id).Select(e => e.LoanAdvanceHead.ToList()).SingleOrDefault();

                    foreach (var LoanAdvanceHeaditem in LoanAdvanceHead)
                    {
                        ITLoan ITLoan = db.ITLoan.Where(e => e.Id == LoanAdvanceHeaditem.ITLoan_Id).SingleOrDefault();
                        LoanAdvanceHeaditem.ITLoan = ITLoan;
                        SalaryHead SalaryHead1 = db.SalaryHead.Where(e => e.Id == LoanAdvanceHeaditem.SalaryHead_Id).SingleOrDefault();
                        LoanAdvanceHeaditem.SalaryHead = SalaryHead1;

                    }
                    ITSectionitem.LoanAdvanceHead = LoanAdvanceHead;
                    List<ITStandardITRebate> ITStandardITRebate = db.ITStandardITRebate.Where(e => e.ITSection_Id == ITSectionitem.Id).ToList();
                    ITSectionitem.ITStandardITRebate = ITStandardITRebate;
                    List<ITSection10> ITSection10 = db.ITSection10.Where(e => e.ITSection_Id == ITSectionitem.Id).ToList();

                    foreach (var ITSection10item in ITSection10)
                    {
                        List<ITSection10SalHeads> Itsection10salhead = db.ITSection10SalHeads.Where(e => e.ITSection10_Id == ITSection10item.Id).ToList();

                        foreach (var Itsection10salheaditem in Itsection10salhead)
                        {
                            LookupValue Frequency = db.LookupValue.Where(e => e.Id == Itsection10salheaditem.Frequency_Id).SingleOrDefault();
                            Itsection10salheaditem.Frequency = Frequency;
                            SalaryHead SalHead = db.SalaryHead.Where(e => e.Id == Itsection10salheaditem.SalHead_Id).SingleOrDefault();
                            Itsection10salheaditem.SalHead = SalHead;

                        }
                        ITSection10item.Itsection10salhead = Itsection10salhead;

                    }
                    ITSectionitem.ITSection10 = ITSection10;
                    LookupValue ITSectionList = db.LookupValue.Where(e => e.Id == ITSectionitem.ITSectionList_Id).SingleOrDefault();
                    ITSectionitem.ITSectionList = ITSectionList;
                    LookupValue ITSectionListType = db.LookupValue.Where(e => e.Id == ITSectionitem.ITSectionListType_Id).SingleOrDefault();
                    ITSectionitem.ITSectionListType = ITSectionListType;

                }
                IncomeTax.ITSection = ITSection;
                //.Include(e => e.IncomeTax)
                //.Include(e => e.IncomeTax.Select(r => r.FyCalendar))
                //.Include(e => e.IncomeTax.Select(r => r.ITSection))
                //.Include(e => e.IncomeTax.Select(r => r.ITSection.Select(s => s.ITInvestments)))
                //.Include(e => e.IncomeTax.Select(r => r.ITSection.Select(s => s.ITInvestments.Select(m => m.ITSubInvestment))))
                //.Include(e => e.IncomeTax.Select(r => r.ITSection.Select(s => s.ITInvestments.Select(m => m.SalaryHead))))
                //.Include(e => e.IncomeTax.Select(r => r.ITSection.Select(s => s.LoanAdvanceHead)))
                //.Include(e => e.IncomeTax.Select(r => r.ITSection.Select(s => s.LoanAdvanceHead.Select(d => d.SalaryHead))))
                //.Include(e => e.IncomeTax.Select(r => r.ITSection.Select(s => s.LoanAdvanceHead.Select(d => d.ITLoan))))
                //.Include(e => e.IncomeTax.Select(r => r.ITSection.Select(d => d.ITStandardITRebate)))
                //.Include(e => e.IncomeTax.Select(r => r.ITSection.Select(d => d.ITSection10)))
                //.Include(e => e.IncomeTax.Select(r => r.ITSection.Select(d => d.ITSection10.Select(s => s.Itsection10salhead))))
                //.Include(e => e.IncomeTax.Select(r => r.ITSection.Select(d => d.ITSection10.Select(s => s.Itsection10salhead.Select(c => c.Frequency)))))
                //.Include(e => e.IncomeTax.Select(r => r.ITSection.Select(d => d.ITSection10.Select(s => s.Itsection10salhead.Select(t => t.SalHead)))))
                //.Include(e => e.IncomeTax.Select(r => r.ITSection.Select(d => d.ITSectionList)))
                //.Include(e => e.IncomeTax.Select(r => r.ITSection.Select(d => d.ITSectionListType)))
                //.Include(e => e.IncomeTax.Select(r => r.ITTDS))
                //.Include(e => e.IncomeTax.Select(r => r.ITTDS.Select(d => d.Category)))



                List<ITProjection> OITProjection = OEmployeePayroll.ITProjection.Where(e => e.FinancialYear.Id == OFinancialYear).ToList();

                List<ITForm16Data> Form16Details = db.EmployeePayroll.Where(r => r.Id == OEmployeePayrollId).SelectMany(t => t.ITForm16Data).ToList();
                List<Form16AllowExemMap> MForm16AllowExemMap = db.Form16AllowExemMap.ToList();

                if (Form16Details != null && Form16Details.Count() > 0)
                {
                    ITForm16Data OForm16Chk = Form16Details.Where(e => e.FinancialYear_Id == OFinancialYear).SingleOrDefault();
                    if (OForm16Chk != null)
                    {
                        if (OForm16Chk.IsLocked == true)
                        {
                            //return 7;
                        }
                        DeleteForm16Data(OForm16Chk, OFinancialYear);
                    }


                }


                List<ITForm16DataDetails> OITForm16DataList = new List<ITForm16DataDetails>();
                ITProjection OEmpITDataTemp = new ITProjection();
                DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };
                //Employee Details
                {
                    ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                    {
                        HeaderCol1 = "1.Gross Salary",
                        ActualAmountCol2 = "₹          ",
                        QualifyAmountCol3 = "₹          ",
                        DeductibleAmountCol4 = "₹          ",
                        FinalAmountCol5 = "",
                        DBTrack = dbt
                    };
                    OITForm16DataList.Add(OITForm16DataSave);
                }
                {
                    OEmpITDataTemp = OITProjection.Where(e => e.PickupId == 32).SingleOrDefault();

                    ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                    {
                        HeaderCol1 = "  (a) Salary as per provisions contained in section 17(1)",
                        ActualAmountCol2 = "",
                        QualifyAmountCol3 = OEmpITDataTemp == null ? null : string.Format("{0:0.00}", OEmpITDataTemp.ActualAmount),
                        // DeductibleAmountCol4 = OEmpITDataTemp == null ? null : OEmpITDataTemp.ActualAmount.ToString(),
                        DeductibleAmountCol4 = "",
                        //DeductibleAmountCol4 = OEmpITDataTemp == null ? null : string.Format("{0:0.00}", OEmpITDataTemp.ActualAmount),
                        FinalAmountCol5 = "",
                        DBTrack = dbt
                    };
                    OITForm16DataList.Add(OITForm16DataSave);

                }
                {
                    OEmpITDataTemp = OITProjection.Where(e => e.PickupId == 34).SingleOrDefault();
                    ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                    {
                        HeaderCol1 = "  (b) Value of Perquisites under section 17(2) (as per Form No. 12BA, wherever applicable)",
                        ActualAmountCol2 = "",
                        QualifyAmountCol3 = OEmpITDataTemp == null ? null : string.Format("{0:0.00}", OEmpITDataTemp.ActualAmount),
                        //DeductibleAmountCol4 = OEmpITDataTemp == null ? null : OEmpITDataTemp.ActualAmount.ToString(),
                        DeductibleAmountCol4 = "",
                        //DeductibleAmountCol4 = OEmpITDataTemp == null ? null : string.Format("{0:0.00}", OEmpITDataTemp.ActualAmount),
                        FinalAmountCol5 = "",
                        DBTrack = dbt
                    };
                    OITForm16DataList.Add(OITForm16DataSave);
                }
                //{
                //    //OEmpITDataTemp = OITProjection.Where(e => e.PickupId == 34).SingleOrDefault();
                //    ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                //    {
                //        HeaderCol1 = "  (as per Form No. 12BA, whatever applicable)",
                //        ActualAmountCol2 = "",
                //        QualifyAmountCol3 = "",
                //        DeductibleAmountCol4 = "",
                //        FinalAmountCol5 = "",
                //        DBTrack = dbt
                //    };
                //    OITForm16DataList.Add(OITForm16DataSave);
                //}
                {
                    //OEmpITDataTemp = OITProjection.Where(e => e.PickupId == 34).SingleOrDefault();
                    ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                    {
                        HeaderCol1 = "  (c) Profits in lieu of salary under section 17(3) (as per Form No. 12BA, wherever applicable)",
                        ActualAmountCol2 = "",
                        QualifyAmountCol3 = "",
                        DeductibleAmountCol4 = "",
                        FinalAmountCol5 = "",
                        DBTrack = dbt
                    };
                    OITForm16DataList.Add(OITForm16DataSave);
                }
                //{
                //    //OEmpITDataTemp = OITProjection.Where(e => e.PickupId == 34).SingleOrDefault();
                //    ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                //    {
                //        HeaderCol1 = "  (as per Form No. 12BA, whatever applicable)",
                //        ActualAmountCol2 = "",
                //        QualifyAmountCol3 = "",
                //        DeductibleAmountCol4 = "",
                //        FinalAmountCol5 = "",
                //        DBTrack = dbt
                //    };
                //    OITForm16DataList.Add(OITForm16DataSave);
                //}
                {
                    OEmpITDataTemp = OITProjection.Where(e => e.PickupId == 35).SingleOrDefault();
                    ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                    {
                        HeaderCol1 = "  (d) Total",
                        ActualAmountCol2 = "",
                        QualifyAmountCol3 = "",
                        DeductibleAmountCol4 = OEmpITDataTemp == null ? null : string.Format("{0:0.00}", OEmpITDataTemp.ActualAmount),
                        //FinalAmountCol5 = OEmpITDataTemp == null ? null : OEmpITDataTemp.ActualAmount.ToString(),
                        FinalAmountCol5 = "",
                        //FinalAmountCol5 = OEmpITDataTemp == null ? null : string.Format("{0:0.00}", OEmpITDataTemp.ActualAmount),
                        DBTrack = dbt
                    };
                    OITForm16DataList.Add(OITForm16DataSave);
                }

                {
                    //OEmpITDataTemp = OITProjection.Where(e => e.PickupId == 34).SingleOrDefault();
                    ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                    {
                        HeaderCol1 = "  (e) Reported total amount of salary received from other employer(s)",
                        ActualAmountCol2 = "",
                        QualifyAmountCol3 = "",
                        DeductibleAmountCol4 = "",
                        FinalAmountCol5 = "",
                        DBTrack = dbt
                    };
                    OITForm16DataList.Add(OITForm16DataSave);
                }

                {
                    //OEmpITDataTemp = OITProjection.Where(e => e.PickupId == 34).SingleOrDefault();
                    ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                    {
                        HeaderCol1 = "2. Less: Allowance to the extent exempt under section 10",
                        ActualAmountCol2 = "",
                        QualifyAmountCol3 = "",
                        DeductibleAmountCol4 = "",
                        FinalAmountCol5 = "",
                        DBTrack = dbt
                    };
                    OITForm16DataList.Add(OITForm16DataSave);
                }
                //AllwExemptionmap start
                ITProjection OEmpITDataTemp2 = new ITProjection();

                Form16AllowExemMap MForm16AllowExemMapa = db.Form16AllowExemMap.Where(e => e.AllowExemptionName == "(a) Travel concession or assistance under section 10(5)").SingleOrDefault();

                {
                    OEmpITDataTemp2 = OITProjection.Where(e => e.PickupId == 41 && (e.ChapterName.Trim()) == MForm16AllowExemMapa.ITSection10ExemCode).SingleOrDefault();
                    ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                    {
                        HeaderCol1 = "(a) Travel concession or assistance under section 10(5)",
                        //  ActualAmountCol2 = ca == null ? null : ca.ActualAmount.ToString(),
                        ActualAmountCol2 = "",
                        QualifyAmountCol3 = OEmpITDataTemp2 == null ? null : string.Format("{0:0.00}", OEmpITDataTemp2.ActualAmount),
                        DeductibleAmountCol4 = "",
                        FinalAmountCol5 = "",
                        DBTrack = dbt
                    };
                    OITForm16DataList.Add(OITForm16DataSave);
                }

                Form16AllowExemMap MForm16AllowExemMapb = db.Form16AllowExemMap.Where(e => e.AllowExemptionName == "(b) Death -cum-retirement gratuity under section 10(10)").SingleOrDefault();

                {
                    OEmpITDataTemp2 = OITProjection.Where(e => e.PickupId == 41 && (e.ChapterName.Trim()) == MForm16AllowExemMapb.ITSection10ExemCode).SingleOrDefault();
                    ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                    {
                        HeaderCol1 = "(b) Death -cum-retirement gratuity under section 10(10)",
                        //  ActualAmountCol2 = ca == null ? null : ca.ActualAmount.ToString(),
                        ActualAmountCol2 = "",
                        QualifyAmountCol3 = OEmpITDataTemp2 == null ? null : string.Format("{0:0.00}", OEmpITDataTemp2.ActualAmount),
                        DeductibleAmountCol4 = "",
                        FinalAmountCol5 = "",
                        DBTrack = dbt
                    };
                    OITForm16DataList.Add(OITForm16DataSave);
                }

                Form16AllowExemMap MForm16AllowExemMapc = db.Form16AllowExemMap.Where(e => e.AllowExemptionName == "(c) Commuted value of pension under section 10(10A)").SingleOrDefault();

                {
                    OEmpITDataTemp2 = OITProjection.Where(e => e.PickupId == 41 && (e.ChapterName.Trim()) == MForm16AllowExemMapc.ITSection10ExemCode).SingleOrDefault();
                    ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                    {
                        HeaderCol1 = "(c) Commuted value of pension under section 10(10A)",
                        //  ActualAmountCol2 = ca == null ? null : ca.ActualAmount.ToString(),
                        ActualAmountCol2 = "",
                        QualifyAmountCol3 = OEmpITDataTemp2 == null ? null : string.Format("{0:0.00}", OEmpITDataTemp2.ActualAmount),
                        DeductibleAmountCol4 = "",
                        FinalAmountCol5 = "",
                        DBTrack = dbt
                    };
                    OITForm16DataList.Add(OITForm16DataSave);
                }

                Form16AllowExemMap MForm16AllowExemMapd = db.Form16AllowExemMap.Where(e => e.AllowExemptionName == "(d) Cash equivalent of leave salary encashment under section 10(10AA)").SingleOrDefault();

                {
                    OEmpITDataTemp2 = OITProjection.Where(e => e.PickupId == 41 && (e.ChapterName.Trim()) == MForm16AllowExemMapd.ITSection10ExemCode).SingleOrDefault();
                    ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                    {
                        HeaderCol1 = "(d) Cash equivalent of leave salary encashment under section 10(10AA)",
                        //  ActualAmountCol2 = ca == null ? null : ca.ActualAmount.ToString(),
                        ActualAmountCol2 = "",
                        QualifyAmountCol3 = OEmpITDataTemp2 == null ? null : string.Format("{0:0.00}", OEmpITDataTemp2.ActualAmount),
                        DeductibleAmountCol4 = "",
                        FinalAmountCol5 = "",
                        DBTrack = dbt
                    };
                    OITForm16DataList.Add(OITForm16DataSave);
                }

                Form16AllowExemMap MForm16AllowExemMape = db.Form16AllowExemMap.Where(e => e.AllowExemptionName == "(e) House rent allowance under section 10(13A)").SingleOrDefault();

                {
                    OEmpITDataTemp2 = OITProjection.Where(e => e.PickupId == 41 && (e.ChapterName.Trim()) == MForm16AllowExemMape.ITSection10ExemCode).SingleOrDefault();
                    ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                    {
                        HeaderCol1 = "(e) House rent allowance under section 10(13A)",
                        //  ActualAmountCol2 = ca == null ? null : ca.ActualAmount.ToString(),
                        ActualAmountCol2 = "",
                        QualifyAmountCol3 = OEmpITDataTemp2 == null ? null : string.Format("{0:0.00}", OEmpITDataTemp2.ActualAmount),
                        DeductibleAmountCol4 = "",
                        FinalAmountCol5 = "",
                        DBTrack = dbt
                    };
                    OITForm16DataList.Add(OITForm16DataSave);
                }

                {
                    //OEmpITDataTemp = OITProjection.Where(e => e.PickupId == 50).SingleOrDefault();
                    ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                    {
                        HeaderCol1 = "(f) Amount of any other exemption under section 10",
                        ActualAmountCol2 = "",
                        QualifyAmountCol3 = "",
                        DeductibleAmountCol4 = "",
                        FinalAmountCol5 = "",
                        DBTrack = dbt
                    };
                    OITForm16DataList.Add(OITForm16DataSave);
                }

                double notmapalwexemp = 0;
                {
                    List<Form16AllowExemMap> MForm16AllowExemMapall = db.Form16AllowExemMap.ToList();

                    List<ITProjection> OEmpITDataTemp1 = OITProjection.Where(e => e.PickupId == 41).ToList();
                    int SrNo = 0;
                    if (OEmpITDataTemp1.Count() > 0)
                    {
                        foreach (ITProjection ca in OEmpITDataTemp1)
                        {

                            string chpname = ca.ChapterName.Trim();
                            var checkchaptername = MForm16AllowExemMapall.Any(q => q.ITSection10ExemCode == chpname);

                            if (checkchaptername == false)
                            {
                                SrNo++;
                                ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                                {
                                    HeaderCol1 = "  (" + SrNo.ToString() + ") " + ca.ChapterName,
                                    //  ActualAmountCol2 = ca == null ? null : ca.ActualAmount.ToString(),
                                    ActualAmountCol2 = ca == null ? null : string.Format("{0:0.00}", ca.ActualAmount),
                                    QualifyAmountCol3 = "",
                                    DeductibleAmountCol4 = "",
                                    FinalAmountCol5 = "",
                                    DBTrack = dbt
                                };
                                notmapalwexemp = notmapalwexemp + ca.ActualAmount;
                                OITForm16DataList.Add(OITForm16DataSave);
                            }
                        }
                    }
                }

                {
                    //OEmpITDataTemp = OITProjection.Where(e => e.PickupId == 85).SingleOrDefault();
                    ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                    {
                        HeaderCol1 = "(g) Total amount of any other exemption under section 10",
                        ActualAmountCol2 = "",
                        QualifyAmountCol3 = string.Format("{0:0.00}", notmapalwexemp),
                        DeductibleAmountCol4 = "",
                        FinalAmountCol5 = "",
                        DBTrack = dbt
                    };
                    OITForm16DataList.Add(OITForm16DataSave);
                }

                {
                    OEmpITDataTemp = OITProjection.Where(e => e.PickupId == 42).SingleOrDefault();
                    ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                    {
                        HeaderCol1 = "(h) Total amount of exemption claimed under section 10 [2(a)+2(b)+2(c)+2(d)+2(e)+2(g)]",
                        ActualAmountCol2 = "",
                        //  QualifyAmountCol3 = OEmpITDataTemp == null ? null : OEmpITDataTemp.ActualAmount.ToString(),
                        // DeductibleAmountCol4 = OEmpITDataTemp == null ? null : OEmpITDataTemp.ActualQualifyingAmount.ToString(),
                        QualifyAmountCol3 = "",
                        DeductibleAmountCol4 = OEmpITDataTemp == null ? null : string.Format("{0:0.00}", OEmpITDataTemp.ActualAmount),
                        FinalAmountCol5 = "",
                        DBTrack = dbt
                    };
                    OITForm16DataList.Add(OITForm16DataSave);
                }


                //AllwExemptionmap End
                //add


                //{
                //    List<ITProjection> OEmpITDataTemp1 = OITProjection.Where(e => e.PickupId == 41).ToList();
                //    int SrNo = 0;
                //    if (OEmpITDataTemp1.Count() > 0)
                //    {
                //        foreach (ITProjection ca in OEmpITDataTemp1)
                //        {
                //            SrNo++;
                //            ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                //            {
                //                HeaderCol1 = "  (" + SrNo.ToString() + ") " + ca.ChapterName,
                //                //  ActualAmountCol2 = ca == null ? null : ca.ActualAmount.ToString(),
                //                ActualAmountCol2 = ca == null ? null : string.Format("{0:0.00}", ca.ActualAmount),
                //                QualifyAmountCol3 = "",
                //                DeductibleAmountCol4 = "",
                //                FinalAmountCol5 = "",
                //                DBTrack = dbt
                //            };
                //            OITForm16DataList.Add(OITForm16DataSave);
                //        }
                //    }
                //}


                // surendra 15/10/2018 start
                //{
                //    OEmpITDataTemp = OITProjection.Where(e => e.PickupId == 42).SingleOrDefault();
                //    ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                //    {
                //        HeaderCol1 = "",
                //        ActualAmountCol2 = "",
                //        //  QualifyAmountCol3 = OEmpITDataTemp == null ? null : OEmpITDataTemp.ActualAmount.ToString(),
                //        // DeductibleAmountCol4 = OEmpITDataTemp == null ? null : OEmpITDataTemp.ActualQualifyingAmount.ToString(),
                //        QualifyAmountCol3 = OEmpITDataTemp == null ? null : string.Format("{0:0.00}", OEmpITDataTemp.ActualAmount),
                //        DeductibleAmountCol4 = OEmpITDataTemp == null ? null : string.Format("{0:0.00}", OEmpITDataTemp.ActualQualifyingAmount),
                //        FinalAmountCol5 = "",
                //        DBTrack = dbt
                //    };
                //    OITForm16DataList.Add(OITForm16DataSave);
                //}
                // surendra 15/10/2018 end

                {
                    OEmpITDataTemp = OITProjection.Where(e => e.PickupId == 45).SingleOrDefault();
                    ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                    {
                        //  HeaderCol1 = "3. Balance (1-2)",
                        HeaderCol1 = "3. Total amount of salary received from current employer [1(d)-2(h)]",
                        ActualAmountCol2 = "",
                        QualifyAmountCol3 = "",
                        DeductibleAmountCol4 = OEmpITDataTemp == null ? null : string.Format("{0:0.00}", OEmpITDataTemp.ActualAmount),
                        // FinalAmountCol5 = OEmpITDataTemp == null ? null : OEmpITDataTemp.ActualAmount.ToString(),
                        FinalAmountCol5 = "",
                        //FinalAmountCol5 = OEmpITDataTemp == null ? null : string.Format("{0:0.00}", OEmpITDataTemp.ActualAmount),
                        DBTrack = dbt
                    };
                    OITForm16DataList.Add(OITForm16DataSave);
                }
                {
                    //OEmpITDataTemp = OITProjection.Where(e => e.PickupId == 50).SingleOrDefault();
                    ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                    {
                        //HeaderCol1 = "4. Deductions",
                        HeaderCol1 = "4. Less : Deductions under section 16",
                        ActualAmountCol2 = "",
                        QualifyAmountCol3 = "",
                        DeductibleAmountCol4 = "",
                        FinalAmountCol5 = "",
                        DBTrack = dbt
                    };
                    OITForm16DataList.Add(OITForm16DataSave);
                }

                {
                    List<ITProjection> OEmpITDataTemp1 = OITProjection.Where(e => e.PickupId == 52).ToList();
                    int SrNo = 0;
                    if (OEmpITDataTemp1.Count() > 0)
                        foreach (ITProjection ca in OEmpITDataTemp1)
                        {
                            SrNo++;
                            ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                            {
                                // HeaderCol1 = " (b) " + ca.ChapterName,
                                HeaderCol1 = " (a) standard deductions under section 16(ia)",
                                //  ActualAmountCol2 = ca == null ? null : ca.ActualAmount.ToString(),
                                ActualAmountCol2 = "",
                                QualifyAmountCol3 = ca == null ? null : string.Format("{0:0.00}", ca.ActualAmount),
                                DeductibleAmountCol4 = "",
                                FinalAmountCol5 = "",
                                DBTrack = dbt
                            };
                            OITForm16DataList.Add(OITForm16DataSave);
                        }
                }

                {
                    //OEmpITDataTemp = OITProjection.Where(e => e.PickupId == 50).SingleOrDefault();
                    ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                    {
                        //HeaderCol1 = "4. Deductions",
                        HeaderCol1 = " (b) Entertainment allowance under section 16(ii)",
                        ActualAmountCol2 = "",
                        QualifyAmountCol3 = "",
                        DeductibleAmountCol4 = "",
                        FinalAmountCol5 = "",
                        DBTrack = dbt
                    };
                    OITForm16DataList.Add(OITForm16DataSave);
                }


                {
                    List<ITProjection> OEmpITDataTemp1 = OITProjection.Where(e => e.PickupId == 51).ToList();
                    int SrNo = 0;
                    if (OEmpITDataTemp1.Count() > 0)
                        foreach (ITProjection ca in OEmpITDataTemp1)
                        {
                            SrNo++;
                            ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                            {
                                //  HeaderCol1 = " (a) " + ca.ChapterName,
                                HeaderCol1 = " (c) Tax on employment under section 16(iii)",
                                //  ActualAmountCol2 = ca == null ? null : ca.ActualAmount.ToString(),
                                ActualAmountCol2 = "",
                                QualifyAmountCol3 = ca == null ? null : string.Format("{0:0.00}", ca.ActualAmount),
                                DeductibleAmountCol4 = "",
                                FinalAmountCol5 = "",
                                DBTrack = dbt
                            };
                            OITForm16DataList.Add(OITForm16DataSave);
                        }
                }

                {
                    OEmpITDataTemp = OITProjection.Where(e => e.PickupId == 55).SingleOrDefault();
                    ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                    {
                        // HeaderCol1 = "5. Aggregate of (a to b)",
                        HeaderCol1 = "5. Total amount of deductions under section 16[4(a)+4(b)+4(c)]",
                        ActualAmountCol2 = "",
                        // QualifyAmountCol3 = OEmpITDataTemp == null ? null : OEmpITDataTemp.ActualAmount.ToString(),
                        //DeductibleAmountCol4 = OEmpITDataTemp == null ? null : OEmpITDataTemp.ActualQualifyingAmount.ToString(),
                        QualifyAmountCol3 = "",
                        DeductibleAmountCol4 = OEmpITDataTemp == null ? null : string.Format("{0:0.00}", OEmpITDataTemp.ActualAmount),
                        // DeductibleAmountCol4 = OEmpITDataTemp == null ? null : string.Format("{0:0.00}", OEmpITDataTemp.ActualQualifyingAmount),
                        FinalAmountCol5 = "",
                        DBTrack = dbt
                    };
                    OITForm16DataList.Add(OITForm16DataSave);
                }
                {
                    OEmpITDataTemp = OITProjection.Where(e => e.PickupId == 56).SingleOrDefault();
                    ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                    {
                        //HeaderCol1 = "6. Income chargable under the Head Salaries (3-5)",
                        HeaderCol1 = "6. Income chargable under the Head Salaries [3+1(e)-5]",
                        ActualAmountCol2 = "",
                        QualifyAmountCol3 = "",
                        DeductibleAmountCol4 = OEmpITDataTemp == null ? null : string.Format("{0:0.00}", OEmpITDataTemp.ActualAmount),
                        //  FinalAmountCol5 = OEmpITDataTemp == null ? null : OEmpITDataTemp.ActualAmount.ToString(),
                        FinalAmountCol5 = "",
                        // FinalAmountCol5 = OEmpITDataTemp == null ? null : string.Format("{0:0.00}", OEmpITDataTemp.ActualAmount),
                        DBTrack = dbt
                    };
                    OITForm16DataList.Add(OITForm16DataSave);
                }

                {
                    //OEmpITDataTemp = OITProjection.Where(e => e.PickupId == 56).SingleOrDefault();
                    ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                    {
                        //HeaderCol1 = "7. Add: Any other Income reported by employee",
                        HeaderCol1 = "7. Add: Any other income reported by employee under as per section 192(2B)",
                        ActualAmountCol2 = "",
                        QualifyAmountCol3 = "",
                        DeductibleAmountCol4 = "",
                        FinalAmountCol5 = "",
                        DBTrack = dbt
                    };
                    OITForm16DataList.Add(OITForm16DataSave);
                }
                double hsgloss = 0;
                double hsgprofit = 0;
                {


                    OEmpITDataTemp = OITProjection.Where(e => e.PickupId == 62).SingleOrDefault();
                    if (OEmpITDataTemp != null)
                    {
                        hsgloss = hsgloss + OEmpITDataTemp.ActualAmount;
                    }
                    else
                    {
                        hsgloss = hsgloss + 0;
                    }
                    OEmpITDataTemp = OITProjection.Where(e => e.PickupId == 72).SingleOrDefault();
                    if (OEmpITDataTemp != null)
                    {
                        hsgprofit = hsgprofit + OEmpITDataTemp.ActualAmount;
                    }
                    else
                    {
                        hsgprofit = hsgprofit + 0;
                    }

                    ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                    {
                        //HeaderCol1 = "6. Income chargable under the Head Salaries (3-5)",
                        HeaderCol1 = "(a) Income(or admissible loss) from house property reported by employee offered for TDS",
                        ActualAmountCol2 = "",
                        QualifyAmountCol3 = "-" + string.Format("{0:0.00}", hsgloss),
                        DeductibleAmountCol4 = "",
                        //  FinalAmountCol5 = OEmpITDataTemp == null ? null : OEmpITDataTemp.ActualAmount.ToString(),
                        FinalAmountCol5 = "",
                        // FinalAmountCol5 = OEmpITDataTemp == null ? null : string.Format("{0:0.00}", OEmpITDataTemp.ActualAmount),
                        DBTrack = dbt
                    };
                    OITForm16DataList.Add(OITForm16DataSave);
                }

                {
                    //OEmpITDataTemp = OITProjection.Where(e => e.PickupId == 56).SingleOrDefault();
                    ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                    {
                        //HeaderCol1 = "7. Add: Any other Income reported by employee",
                        HeaderCol1 = "(b) Income under the head other sources offered for TDS",
                        ActualAmountCol2 = "",
                        QualifyAmountCol3 = string.Format("{0:0.00}", hsgprofit),
                        DeductibleAmountCol4 = "",
                        FinalAmountCol5 = "",
                        DBTrack = dbt
                    };
                    OITForm16DataList.Add(OITForm16DataSave);
                }

                {
                    //OEmpITDataTemp = OITProjection.Where(e => e.PickupId == 56).SingleOrDefault();
                    ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                    {
                        //HeaderCol1 = "7. Add: Any other Income reported by employee",
                        HeaderCol1 = "8. Total amount of other income reported by the employee [7(a)+7(b)]",
                        ActualAmountCol2 = "",
                        QualifyAmountCol3 = "",
                        DeductibleAmountCol4 = string.Format("{0:0.00}", -hsgloss + hsgprofit),
                        FinalAmountCol5 = "",
                        DBTrack = dbt
                    };
                    OITForm16DataList.Add(OITForm16DataSave);
                }

                //{
                //    List<ITProjection> OEmpITDataTemp1 = OITProjection.Where(e => e.PickupId == 61).ToList();
                //    int SrNo = 0;
                //    if (OEmpITDataTemp1.Count() > 0)
                //    {
                //        foreach (ITProjection ca in OEmpITDataTemp1)
                //        {
                //            SrNo++;
                //            ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                //            {
                //                HeaderCol1 = "  (" + SrNo.ToString() + ") " + ca.ChapterName,
                //                //ActualAmountCol2 = ca == null ? null : ca.ActualAmount.ToString(),
                //                ActualAmountCol2 = ca == null ? null : string.Format("{0:0.00}", ca.ActualAmount),
                //                QualifyAmountCol3 = "",
                //                DeductibleAmountCol4 = "",
                //                FinalAmountCol5 = "",
                //                DBTrack = dbt
                //            };
                //            OITForm16DataList.Add(OITForm16DataSave);
                //        }
                //    }
                //}
                //{
                //    List<ITProjection> OEmpITDataTemp1 = OITProjection.Where(e => e.PickupId == 71).ToList();
                //    int SrNo = 0;
                //    if (OEmpITDataTemp1.Count() > 0)
                //    {
                //        foreach (ITProjection ca in OEmpITDataTemp1)
                //        {
                //            SrNo++;
                //            ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                //            {
                //                HeaderCol1 = "  (" + SrNo.ToString() + ") " + ca.ChapterName,
                //                //ActualAmountCol2 = ca == null ? null : ca.ActualAmount.ToString(),
                //                ActualAmountCol2 = ca == null ? null : string.Format("{0:0.00}", ca.ActualAmount),
                //                QualifyAmountCol3 = "",
                //                DeductibleAmountCol4 = "",
                //                FinalAmountCol5 = "",
                //                DBTrack = dbt
                //            };
                //            OITForm16DataList.Add(OITForm16DataSave);
                //        }
                //    }
                //}
                //{
                //    List<ITProjection> OEmpITDataTemp1 = OITProjection.Where(e => e.PickupId == 81).ToList();
                //    var SrNo = 0;
                //    if (OEmpITDataTemp1.Count() > 0)
                //    {
                //        foreach (ITProjection ca in OEmpITDataTemp1)
                //        {
                //            SrNo++;
                //            ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                //            {
                //                HeaderCol1 = "  (" + SrNo.ToString() + ") " + ca.ChapterName,
                //                // ActualAmountCol2 = ca == null ? null : ca.ActualAmount.ToString(),
                //                ActualAmountCol2 = ca == null ? null : string.Format("{0:0.00}", ca.ActualAmount),
                //                QualifyAmountCol3 = "",
                //                DeductibleAmountCol4 = "",
                //                FinalAmountCol5 = "",
                //                DBTrack = dbt
                //            };
                //            OITForm16DataList.Add(OITForm16DataSave);
                //        }
                //    }
                //}

                {
                    OEmpITDataTemp = OITProjection.Where(e => e.PickupId == 85).SingleOrDefault();
                    ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                    {
                        // HeaderCol1 = "8. Gross Total Income (6+7)",
                        HeaderCol1 = "9. Gross total income (6+8)",
                        ActualAmountCol2 = "",
                        QualifyAmountCol3 = "",
                        DeductibleAmountCol4 = OEmpITDataTemp == null ? null : string.Format("{0:0.00}", OEmpITDataTemp.ActualAmount),
                        //FinalAmountCol5 = OEmpITDataTemp == null ? null : OEmpITDataTemp.ActualAmount.ToString(),
                        //FinalAmountCol5 = OEmpITDataTemp == null ? null : string.Format("{0:0.00}", OEmpITDataTemp.ActualAmount),
                        FinalAmountCol5 = "",
                        DBTrack = dbt
                    };
                    OITForm16DataList.Add(OITForm16DataSave);
                }
                {
                    //OEmpITDataTemp = OITProjection.Where(e => e.PickupId == 85).SingleOrDefault();
                    ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                    {
                        // HeaderCol1 = "9. Deduction under Chapter VIA",
                        HeaderCol1 = "10. Deduction under Chapter VI-A",
                        //ActualAmountCol2 = "Gross Amount",
                        //QualifyAmountCol3 = "Qualifying Amount",
                        //DeductibleAmountCol4 = "Deductible Amount",
                        ActualAmountCol2 = "",
                        QualifyAmountCol3 = "Gross Amount",
                        DeductibleAmountCol4 = "Deductible Amount",
                        FinalAmountCol5 = "",
                        DBTrack = dbt
                    };
                    OITForm16DataList.Add(OITForm16DataSave);
                }
                // Form 16 new format for Secton Start
                {
                    //OEmpITDataTemp = OITProjection.Where(e => e.PickupId == 85).SingleOrDefault();
                    ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                    {
                        HeaderCol1 = "(a) Deduction in respect of life insurance premia,contributions to provident fund etc.under section 80C",
                        ActualAmountCol2 = "",
                        QualifyAmountCol3 = "",
                        DeductibleAmountCol4 = "",
                        FinalAmountCol5 = "",
                        DBTrack = dbt
                    };
                    OITForm16DataList.Add(OITForm16DataSave);
                }

                {
                    List<ITProjection> OEmpITDataTemp1 = OITProjection.Where(e => e.PickupId == 92).ToList();
                    int SrNo = 0;
                    if (OEmpITDataTemp1.Count() > 0)
                    {
                        foreach (ITProjection ca in OEmpITDataTemp1)
                        {
                            if (ca.Section.ToString().Trim().ToUpper() == "SECTION80C")
                            {
                                SrNo++;
                                ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                                {
                                    HeaderCol1 = "  (" + SrNo.ToString() + ") " + ca.ChapterName,
                                    // ActualAmountCol2 = ca == null ? null : ca.ActualAmount.ToString(),
                                    ActualAmountCol2 = "",
                                    QualifyAmountCol3 = ca == null ? null : string.Format("{0:0.00}", ca.ActualAmount),
                                    DeductibleAmountCol4 = "",
                                    FinalAmountCol5 = "",
                                    DBTrack = dbt
                                };
                                OITForm16DataList.Add(OITForm16DataSave);

                            }
                        }
                    }
                }

                {
                    //OEmpITDataTemp = OITProjection.Where(e => e.PickupId == 85).SingleOrDefault();
                    ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                    {
                        HeaderCol1 = "(b) Deduction in respect of contribution to certain pension funds under section 80CCC",
                        ActualAmountCol2 = "",
                        QualifyAmountCol3 = "",
                        DeductibleAmountCol4 = "",
                        FinalAmountCol5 = "",
                        DBTrack = dbt
                    };
                    OITForm16DataList.Add(OITForm16DataSave);
                }

                {
                    List<ITProjection> OEmpITDataTemp1 = OITProjection.Where(e => e.PickupId == 92).ToList();
                    int SrNo = 0;
                    if (OEmpITDataTemp1.Count() > 0)
                    {
                        foreach (ITProjection ca in OEmpITDataTemp1)
                        {
                            if (ca.Section.ToString().Trim().ToUpper() == "SECTION80CCC")
                            {
                                SrNo++;
                                ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                                {
                                    HeaderCol1 = "  (" + SrNo.ToString() + ") " + ca.ChapterName,
                                    // ActualAmountCol2 = ca == null ? null : ca.ActualAmount.ToString(),
                                    ActualAmountCol2 = "",
                                    QualifyAmountCol3 = ca == null ? null : string.Format("{0:0.00}", ca.ActualAmount),
                                    DeductibleAmountCol4 = "",
                                    FinalAmountCol5 = "",
                                    DBTrack = dbt
                                };
                                OITForm16DataList.Add(OITForm16DataSave);

                            }
                        }
                    }
                }

                {
                    //OEmpITDataTemp = OITProjection.Where(e => e.PickupId == 85).SingleOrDefault();
                    ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                    {
                        HeaderCol1 = "(c) Deduction in respect of contribution by taxpayer to pension scheme under section 80CCD(1)",
                        ActualAmountCol2 = "",
                        QualifyAmountCol3 = "",
                        DeductibleAmountCol4 = "",
                        FinalAmountCol5 = "",
                        DBTrack = dbt
                    };
                    OITForm16DataList.Add(OITForm16DataSave);
                }

                {
                    List<ITProjection> OEmpITDataTemp1 = OITProjection.Where(e => e.PickupId == 92).ToList();
                    int SrNo = 0;
                    if (OEmpITDataTemp1.Count() > 0)
                    {
                        foreach (ITProjection ca in OEmpITDataTemp1)
                        {
                            if (ca.Section.ToString().Trim().ToUpper() == "SECTION80CCD(1)")
                            {
                                SrNo++;
                                ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                                {
                                    HeaderCol1 = "  (" + SrNo.ToString() + ") " + ca.ChapterName,
                                    // ActualAmountCol2 = ca == null ? null : ca.ActualAmount.ToString(),
                                    ActualAmountCol2 = "",
                                    QualifyAmountCol3 = ca == null ? null : string.Format("{0:0.00}", ca.ActualAmount),
                                    DeductibleAmountCol4 = "",
                                    FinalAmountCol5 = "",
                                    DBTrack = dbt
                                };
                                OITForm16DataList.Add(OITForm16DataSave);

                            }
                        }
                    }
                }




                //80c total
                {
                    OEmpITDataTemp = OITProjection.Where(e => e.PickupId == 93).SingleOrDefault();
                    ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                    {
                        HeaderCol1 = "(d) Total deduction under section 80C,80CCC and 80CCD(1)",
                        ActualAmountCol2 = "",
                        //QualifyAmountCol3 = OEmpITDataTemp == null ? null : OEmpITDataTemp.ActualAmount.ToString(),
                        //DeductibleAmountCol4 = OEmpITDataTemp == null ? null : OEmpITDataTemp.ActualQualifyingAmount.ToString(),
                        QualifyAmountCol3 = OEmpITDataTemp == null ? null : string.Format("{0:0.00}", OEmpITDataTemp.ActualAmount),
                        DeductibleAmountCol4 = OEmpITDataTemp == null ? null : string.Format("{0:0.00}", OEmpITDataTemp.ActualQualifyingAmount),

                        FinalAmountCol5 = "",
                        DBTrack = dbt
                    };
                    OITForm16DataList.Add(OITForm16DataSave);
                }

                {
                    //OEmpITDataTemp = OITProjection.Where(e => e.PickupId == 85).SingleOrDefault();
                    ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                    {
                        HeaderCol1 = "(e) Deductions in respect of amount paid/deposited to notified pension scheme under section 80CCD(1B)",
                        ActualAmountCol2 = "",
                        QualifyAmountCol3 = "",
                        DeductibleAmountCol4 = "",
                        FinalAmountCol5 = "",
                        DBTrack = dbt
                    };
                    OITForm16DataList.Add(OITForm16DataSave);
                }

                {
                    List<ITProjection> OEmpITDataTemp1 = OITProjection.Where(e => e.PickupId == 101).ToList();
                    int SrNo = 0;
                    if (OEmpITDataTemp1.Count() > 0)
                    {
                        foreach (ITProjection ca in OEmpITDataTemp1)
                        {
                            if (ca.Section.ToString().Trim().ToUpper() == "SECTION80CCD(1B)")
                            {
                                SrNo++;
                                ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                                {
                                    HeaderCol1 = "  (" + SrNo.ToString() + ") " + ca.ChapterName,
                                    //ActualAmountCol2 = ca == null ? null : ca.ActualAmount.ToString(),
                                    ActualAmountCol2 = "",
                                    QualifyAmountCol3 = ca == null ? null : string.Format("{0:0.00}", ca.ActualAmount),
                                    DeductibleAmountCol4 = ca == null ? null : string.Format("{0:0.00}", ca.ActualQualifyingAmount),
                                    FinalAmountCol5 = "",
                                    DBTrack = dbt
                                };
                                OITForm16DataList.Add(OITForm16DataSave);
                            }
                        }
                    }
                }

                {
                    //OEmpITDataTemp = OITProjection.Where(e => e.PickupId == 85).SingleOrDefault();
                    ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                    {
                        HeaderCol1 = "(f) Deduction in respect of contribution by Employer to pension scheme under section 80CCD(2)",
                        ActualAmountCol2 = "",
                        QualifyAmountCol3 = "",
                        DeductibleAmountCol4 = "",
                        FinalAmountCol5 = "",
                        DBTrack = dbt
                    };
                    OITForm16DataList.Add(OITForm16DataSave);
                }

                {
                    List<ITProjection> OEmpITDataTemp1 = OITProjection.Where(e => e.PickupId == 101).ToList();
                    int SrNo = 0;
                    if (OEmpITDataTemp1.Count() > 0)
                    {
                        foreach (ITProjection ca in OEmpITDataTemp1)
                        {
                            if (ca.Section.ToString().Trim().ToUpper() == "SECTION80CCD(2)")
                            {
                                SrNo++;
                                ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                                {
                                    HeaderCol1 = "  (" + SrNo.ToString() + ") " + ca.ChapterName,
                                    //ActualAmountCol2 = ca == null ? null : ca.ActualAmount.ToString(),
                                    ActualAmountCol2 = "",
                                    QualifyAmountCol3 = ca == null ? null : string.Format("{0:0.00}", ca.ActualAmount),
                                    DeductibleAmountCol4 = ca == null ? null : string.Format("{0:0.00}", ca.ActualQualifyingAmount),
                                    FinalAmountCol5 = "",
                                    DBTrack = dbt
                                };
                                OITForm16DataList.Add(OITForm16DataSave);
                            }
                        }
                    }
                }


                //80D-U

                {
                    //OEmpITDataTemp = OITProjection.Where(e => e.PickupId == 85).SingleOrDefault();
                    ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                    {
                        HeaderCol1 = "(g) Deduction in respect of health insurance premia under section 80D",
                        ActualAmountCol2 = "",
                        QualifyAmountCol3 = "",
                        DeductibleAmountCol4 = "",
                        FinalAmountCol5 = "",
                        DBTrack = dbt
                    };
                    OITForm16DataList.Add(OITForm16DataSave);
                }

                {
                    List<ITProjection> OEmpITDataTemp1 = OITProjection.Where(e => e.PickupId == 111).ToList();
                    int SrNo = 0;
                    if (OEmpITDataTemp1.Count() > 0)
                    {
                        foreach (ITProjection ca in OEmpITDataTemp1)
                        {
                            if (ca.Section.ToString().Trim().ToUpper() == "SECTION80D")
                            {
                                SrNo++;
                                ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                                {
                                    HeaderCol1 = "  (" + SrNo.ToString() + ") " + ca.ChapterName,
                                    //  ActualAmountCol2 = ca == null ? null : ca.ActualAmount.ToString(),
                                    ActualAmountCol2 = "",
                                    QualifyAmountCol3 = ca == null ? null : string.Format("{0:0.00}", ca.ActualAmount),
                                    DeductibleAmountCol4 = "",
                                    FinalAmountCol5 = "",
                                    DBTrack = dbt
                                };
                                OITForm16DataList.Add(OITForm16DataSave);
                            }
                        }
                    }
                }

                {
                    //OEmpITDataTemp = OITProjection.Where(e => e.PickupId == 85).SingleOrDefault();
                    ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                    {
                        HeaderCol1 = "(h) Deduction in respect of interest on loan taken for higher education under section 80E",
                        ActualAmountCol2 = "",
                        QualifyAmountCol3 = "",
                        DeductibleAmountCol4 = "",
                        FinalAmountCol5 = "",
                        DBTrack = dbt
                    };
                    OITForm16DataList.Add(OITForm16DataSave);
                }

                {
                    List<ITProjection> OEmpITDataTemp1 = OITProjection.Where(e => e.PickupId == 111).ToList();
                    int SrNo = 0;
                    if (OEmpITDataTemp1.Count() > 0)
                    {
                        foreach (ITProjection ca in OEmpITDataTemp1)
                        {
                            if (ca.Section.ToString().Trim().ToUpper() == "SECTION80E")
                            {
                                SrNo++;
                                ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                                {
                                    HeaderCol1 = "  (" + SrNo.ToString() + ") " + ca.ChapterName,
                                    //  ActualAmountCol2 = ca == null ? null : ca.ActualAmount.ToString(),
                                    ActualAmountCol2 = "",
                                    QualifyAmountCol3 = ca == null ? null : string.Format("{0:0.00}", ca.ActualAmount),
                                    DeductibleAmountCol4 = "",
                                    FinalAmountCol5 = "",
                                    DBTrack = dbt
                                };
                                OITForm16DataList.Add(OITForm16DataSave);
                            }
                        }
                    }
                }



                {
                    //OEmpITDataTemp = OITProjection.Where(e => e.PickupId == 85).SingleOrDefault();
                    ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                    {
                        HeaderCol1 = "",
                        ActualAmountCol2 = "Gross Amount",
                        QualifyAmountCol3 = "Qualifying Amount",
                        DeductibleAmountCol4 = "Deductible Amount",
                        FinalAmountCol5 = "",
                        DBTrack = dbt
                    };
                    OITForm16DataList.Add(OITForm16DataSave);
                }

                {
                    //OEmpITDataTemp = OITProjection.Where(e => e.PickupId == 85).SingleOrDefault();
                    ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                    {
                        HeaderCol1 = "(i) Total Deduction in respect of donations to certain funds,charitable institution,etc. under section 80G",
                        ActualAmountCol2 = "",
                        QualifyAmountCol3 = "",
                        DeductibleAmountCol4 = "",
                        FinalAmountCol5 = "",
                        DBTrack = dbt
                    };
                    OITForm16DataList.Add(OITForm16DataSave);
                }

                {
                    List<ITProjection> OEmpITDataTemp1 = OITProjection.Where(e => e.PickupId == 111).ToList();
                    int SrNo = 0;
                    if (OEmpITDataTemp1.Count() > 0)
                    {
                        foreach (ITProjection ca in OEmpITDataTemp1)
                        {
                            if (ca.Section.ToString().Trim().ToUpper() == "SECTION80G")
                            {
                                SrNo++;
                                ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                                {
                                    HeaderCol1 = "  (" + SrNo.ToString() + ") " + ca.ChapterName,
                                    //  ActualAmountCol2 = ca == null ? null : ca.ActualAmount.ToString(),
                                    ActualAmountCol2 = ca == null ? null : string.Format("{0:0.00}", ca.ActualAmount),
                                    QualifyAmountCol3 = "",
                                    DeductibleAmountCol4 = "",
                                    FinalAmountCol5 = "",
                                    DBTrack = dbt
                                };
                                OITForm16DataList.Add(OITForm16DataSave);
                            }
                        }
                    }
                }


                {
                    //OEmpITDataTemp = OITProjection.Where(e => e.PickupId == 85).SingleOrDefault();
                    ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                    {
                        HeaderCol1 = "(j) Deduction in respect of interest on Deposits in saving account under section 80TTA",
                        ActualAmountCol2 = "",
                        QualifyAmountCol3 = "",
                        DeductibleAmountCol4 = "",
                        FinalAmountCol5 = "",
                        DBTrack = dbt
                    };
                    OITForm16DataList.Add(OITForm16DataSave);
                }


                {
                    List<ITProjection> OEmpITDataTemp1 = OITProjection.Where(e => e.PickupId == 111).ToList();
                    int SrNo = 0;
                    if (OEmpITDataTemp1.Count() > 0)
                    {
                        foreach (ITProjection ca in OEmpITDataTemp1)
                        {
                            if (ca.Section.ToString().Trim().ToUpper() == "SECTION80TTA")
                            {
                                SrNo++;
                                ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                                {
                                    HeaderCol1 = "  (" + SrNo.ToString() + ") " + ca.ChapterName,
                                    //  ActualAmountCol2 = ca == null ? null : ca.ActualAmount.ToString(),
                                    ActualAmountCol2 = ca == null ? null : string.Format("{0:0.00}", ca.ActualAmount),
                                    QualifyAmountCol3 = "",
                                    DeductibleAmountCol4 = "",
                                    FinalAmountCol5 = "",
                                    DBTrack = dbt
                                };
                                OITForm16DataList.Add(OITForm16DataSave);
                            }
                        }
                    }
                }


                {
                    //OEmpITDataTemp = OITProjection.Where(e => e.PickupId == 85).SingleOrDefault();
                    ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                    {
                        HeaderCol1 = "(k) Amount deductible under any other provision(s) of Chapter VI-A",
                        ActualAmountCol2 = "",
                        QualifyAmountCol3 = "",
                        DeductibleAmountCol4 = "",
                        FinalAmountCol5 = "",
                        DBTrack = dbt
                    };
                    OITForm16DataList.Add(OITForm16DataSave);
                }
                double actualchpt = 0;
                {
                    List<ITProjection> OEmpITDataTemp1 = OITProjection.Where(e => e.PickupId == 111).ToList();
                    int SrNo = 0;


                    if (OEmpITDataTemp1.Count() > 0)
                    {
                        foreach (ITProjection ca in OEmpITDataTemp1)
                        {
                            if (ca.Section.ToString().Trim().ToUpper() != "SECTION80TTA" && ca.Section.ToString().Trim().ToUpper() != "SECTION80G" && ca.Section.ToString().Trim().ToUpper() != "SECTION80E" && ca.Section.ToString().Trim().ToUpper() != "SECTION80D")
                            {
                                SrNo++;
                                ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                                {
                                    HeaderCol1 = "  (" + SrNo.ToString() + ") " + ca.ChapterName,
                                    //  ActualAmountCol2 = ca == null ? null : ca.ActualAmount.ToString(),
                                    ActualAmountCol2 = ca == null ? null : string.Format("{0:0.00}", ca.ActualAmount),
                                    QualifyAmountCol3 = "",
                                    DeductibleAmountCol4 = "",
                                    FinalAmountCol5 = "",
                                    DBTrack = dbt,


                                };
                                actualchpt = actualchpt + ca.ActualAmount;

                                OITForm16DataList.Add(OITForm16DataSave);
                            }
                        }
                    }
                }


                {
                    //OEmpITDataTemp = OITProjection.Where(e => e.PickupId == 85).SingleOrDefault();
                    ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                    {
                        HeaderCol1 = "(l) Total Amount deductible under any other provision(s) of Chapter VI-A",
                        ActualAmountCol2 = string.Format("{0:0.00}", actualchpt),
                        QualifyAmountCol3 = "",
                        DeductibleAmountCol4 = "",
                        FinalAmountCol5 = "",
                        DBTrack = dbt
                    };
                    OITForm16DataList.Add(OITForm16DataSave);
                }



                //aggregate of investment 80c,80ccc,80d-u total

                {
                    OEmpITDataTemp = OITProjection.Where(e => e.PickupId == 115).SingleOrDefault();
                    if (OEmpITDataTemp != null)
                    {
                        ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                        {
                            HeaderCol1 = "11. Aggregate of deductible amount under Chapter VI-A [10(d)+10(e)+10(f)+10(g)+10(h)+10(i)+10(j)+10(l)]",
                            ActualAmountCol2 = "",
                            QualifyAmountCol3 = "",
                            //  DeductibleAmountCol4 = OEmpITDataTemp == null ? null : OEmpITDataTemp.ActualAmount.ToString(),
                            DeductibleAmountCol4 = OEmpITDataTemp == null ? null : string.Format("{0:0.00}", OEmpITDataTemp.ActualAmount),
                            FinalAmountCol5 = "",
                            DBTrack = dbt
                        };
                        OITForm16DataList.Add(OITForm16DataSave);
                    }
                }



                // Form 16 new format for Secton End

                //{
                //    //OEmpITDataTemp = OITProjection.Where(e => e.PickupId == 85).SingleOrDefault();
                //    ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                //    {
                //        HeaderCol1 = "  (A) Section 80C, 80CCC and 80CCD",
                //        ActualAmountCol2 = "",
                //        QualifyAmountCol3 = "",
                //        DeductibleAmountCol4 = "",
                //        FinalAmountCol5 = "",
                //        DBTrack = dbt
                //    };
                //    OITForm16DataList.Add(OITForm16DataSave);
                //}
                //{
                //    //OEmpITDataTemp = OITProjection.Where(e => e.PickupId == 85).SingleOrDefault();
                //    ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                //    {
                //        HeaderCol1 = "  (a) Section 80C",
                //        ActualAmountCol2 = "",
                //        QualifyAmountCol3 = "",
                //        DeductibleAmountCol4 = "",
                //        FinalAmountCol5 = "",
                //        DBTrack = dbt
                //    };
                //    OITForm16DataList.Add(OITForm16DataSave);
                //}
                //{
                //    List<ITProjection> OEmpITDataTemp1 = OITProjection.Where(e => e.PickupId == 92).ToList();
                //    int SrNo = 0;
                //    if (OEmpITDataTemp1.Count() > 0)
                //    {
                //        foreach (ITProjection ca in OEmpITDataTemp1)
                //        {
                //            SrNo++;
                //            ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                //            {
                //                HeaderCol1 = "  (" + SrNo.ToString() + ") " + ca.ChapterName,
                //                // ActualAmountCol2 = ca == null ? null : ca.ActualAmount.ToString(),
                //                ActualAmountCol2 = ca == null ? null : string.Format("{0:0.00}", ca.ActualAmount),
                //                QualifyAmountCol3 = "",
                //                DeductibleAmountCol4 = "",
                //                FinalAmountCol5 = "",
                //                DBTrack = dbt
                //            };
                //            OITForm16DataList.Add(OITForm16DataSave);
                //        }
                //    }
                //}
                //80c total
                //{
                //    OEmpITDataTemp = OITProjection.Where(e => e.PickupId == 93).SingleOrDefault();
                //    ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                //    {
                //        HeaderCol1 = "",
                //        ActualAmountCol2 = "",
                //        //QualifyAmountCol3 = OEmpITDataTemp == null ? null : OEmpITDataTemp.ActualAmount.ToString(),
                //        //DeductibleAmountCol4 = OEmpITDataTemp == null ? null : OEmpITDataTemp.ActualQualifyingAmount.ToString(),
                //        QualifyAmountCol3 = OEmpITDataTemp == null ? null : string.Format("{0:0.00}", OEmpITDataTemp.ActualAmount),
                //        DeductibleAmountCol4 = OEmpITDataTemp == null ? null : string.Format("{0:0.00}", OEmpITDataTemp.ActualQualifyingAmount),

                //        FinalAmountCol5 = "",
                //        DBTrack = dbt
                //    };
                //    OITForm16DataList.Add(OITForm16DataSave);
                //}
                //80ccc-ccd
                //{
                //    //OEmpITDataTemp = OITProjection.Where(e => e.PickupId == 85).SingleOrDefault();
                //    ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                //    {
                //        HeaderCol1 = "  (b) Section 80CCC-80CCF",
                //        ActualAmountCol2 = "",
                //        QualifyAmountCol3 = "",
                //        DeductibleAmountCol4 = "",
                //        FinalAmountCol5 = "",
                //        DBTrack = dbt
                //    };
                //    OITForm16DataList.Add(OITForm16DataSave);
                //}
                //{
                //    List<ITProjection> OEmpITDataTemp1 = OITProjection.Where(e => e.PickupId == 101).ToList();
                //    int SrNo = 0;
                //    if (OEmpITDataTemp1.Count() > 0)
                //    {
                //        foreach (ITProjection ca in OEmpITDataTemp1)
                //        {
                //            SrNo++;
                //            ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                //            {
                //                HeaderCol1 = "  (" + SrNo.ToString() + ") " + ca.ChapterName,
                //                //ActualAmountCol2 = ca == null ? null : ca.ActualAmount.ToString(),
                //                ActualAmountCol2 = ca == null ? null : string.Format("{0:0.00}", ca.ActualAmount),
                //                QualifyAmountCol3 = "",
                //                DeductibleAmountCol4 = "",
                //                FinalAmountCol5 = "",
                //                DBTrack = dbt
                //            };
                //            OITForm16DataList.Add(OITForm16DataSave);
                //        }
                //    }
                //}
                ////80ccc-ccf total
                //{
                //    OEmpITDataTemp = OITProjection.Where(e => e.PickupId == 102).SingleOrDefault();
                //    if (OEmpITDataTemp != null)
                //    {
                //        ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                //        {
                //            HeaderCol1 = "",
                //            ActualAmountCol2 = "",
                //            // QualifyAmountCol3 = OEmpITDataTemp == null ? null : OEmpITDataTemp.ActualAmount.ToString(),
                //            //  DeductibleAmountCol4 = OEmpITDataTemp == null ? null : OEmpITDataTemp.ActualQualifyingAmount.ToString(),
                //            QualifyAmountCol3 = OEmpITDataTemp == null ? null : string.Format("{0:0.00}", OEmpITDataTemp.ActualAmount),
                //            DeductibleAmountCol4 = OEmpITDataTemp == null ? null : string.Format("{0:0.00}", OEmpITDataTemp.ActualQualifyingAmount),

                //            FinalAmountCol5 = "",
                //            DBTrack = dbt
                //        };
                //        OITForm16DataList.Add(OITForm16DataSave);
                //    }
                //}
                ////80ccc-ccf total
                //{
                //    //OEmpITDataTemp = OITProjection.Where(e => e.PickupId == 102).SingleOrDefault();
                //    ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                //    {
                //        HeaderCol1 = "  Note: 1.Aggregate Amount under section 80C shall not be exceed One Lac Fifty Thousand Rupees" +
                //                              "2. Aggregate amount deductible under the three sections i.e. 80C,80CCC,80CCD shall not exceed Two Lakh Rupees",
                //        ActualAmountCol2 = "",
                //        QualifyAmountCol3 = "",
                //        DeductibleAmountCol4 = "",
                //        FinalAmountCol5 = "",
                //        DBTrack = dbt
                //    };
                //    OITForm16DataList.Add(OITForm16DataSave);
                //}

                ////80D-U
                //{
                //    //OEmpITDataTemp = OITProjection.Where(e => e.PickupId == 85).SingleOrDefault();
                //    ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                //    {
                //        HeaderCol1 = "  (B) Other Sections (for e.g. 80E,80G etc)",
                //        ActualAmountCol2 = "",
                //        QualifyAmountCol3 = "",
                //        DeductibleAmountCol4 = "",
                //        FinalAmountCol5 = "",
                //        DBTrack = dbt
                //    };
                //    OITForm16DataList.Add(OITForm16DataSave);
                //}
                //{
                //    List<ITProjection> OEmpITDataTemp1 = OITProjection.Where(e => e.PickupId == 111).ToList();
                //    int SrNo = 0;
                //    if (OEmpITDataTemp1.Count() > 0)
                //    {
                //        foreach (ITProjection ca in OEmpITDataTemp1)
                //        {
                //            SrNo++;
                //            ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                //            {
                //                HeaderCol1 = "  (" + SrNo.ToString() + ") " + ca.ChapterName,
                //                //  ActualAmountCol2 = ca == null ? null : ca.ActualAmount.ToString(),
                //                ActualAmountCol2 = ca == null ? null : string.Format("{0:0.00}", ca.ActualAmount),
                //                QualifyAmountCol3 = "",
                //                DeductibleAmountCol4 = "",
                //                FinalAmountCol5 = "",
                //                DBTrack = dbt
                //            };
                //            OITForm16DataList.Add(OITForm16DataSave);
                //        }
                //    }
                //}
                ////80d total
                //{
                //    OEmpITDataTemp = OITProjection.Where(e => e.PickupId == 112).SingleOrDefault();
                //    if (OEmpITDataTemp != null)
                //    {
                //        ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                //        {
                //            HeaderCol1 = "",
                //            ActualAmountCol2 = "",
                //            //   QualifyAmountCol3 = OEmpITDataTemp == null ? null : OEmpITDataTemp.ActualAmount.ToString(),
                //            //  DeductibleAmountCol4 = OEmpITDataTemp == null ? null : OEmpITDataTemp.ActualQualifyingAmount.ToString(),
                //            QualifyAmountCol3 = OEmpITDataTemp == null ? null : string.Format("{0:0.00}", OEmpITDataTemp.ActualAmount),
                //            DeductibleAmountCol4 = OEmpITDataTemp == null ? null : string.Format("{0:0.00}", OEmpITDataTemp.ActualQualifyingAmount),
                //            FinalAmountCol5 = "",
                //            DBTrack = dbt
                //        };
                //        OITForm16DataList.Add(OITForm16DataSave);
                //    }
                //}
                ////aggregate of investment 80c,80ccc,80d-u total
                //{
                //    OEmpITDataTemp = OITProjection.Where(e => e.PickupId == 115).SingleOrDefault();
                //    if (OEmpITDataTemp != null)
                //    {
                //        ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                //        {
                //            HeaderCol1 = "10. Aggregate of deductible amount under Chapter VI-A",
                //            ActualAmountCol2 = "",
                //            QualifyAmountCol3 = "",
                //            //  DeductibleAmountCol4 = OEmpITDataTemp == null ? null : OEmpITDataTemp.ActualAmount.ToString(),
                //            DeductibleAmountCol4 = OEmpITDataTemp == null ? null : string.Format("{0:0.00}", OEmpITDataTemp.ActualAmount),
                //            FinalAmountCol5 = "",
                //            DBTrack = dbt
                //        };
                //        OITForm16DataList.Add(OITForm16DataSave);
                //    }
                //}

                {
                    OEmpITDataTemp = OITProjection.Where(e => e.PickupId == 120).SingleOrDefault();
                    if (OEmpITDataTemp != null)
                    {
                        ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                        {
                            //  HeaderCol1 = "11. Total Income (8-10)",
                            HeaderCol1 = "12. Total taxable income (9-11)",
                            ActualAmountCol2 = "",
                            QualifyAmountCol3 = "",
                            // DeductibleAmountCol4 = "",
                            // FinalAmountCol5 = OEmpITDataTemp == null ? null : OEmpITDataTemp.ActualAmount.ToString(),
                            FinalAmountCol5 = "",
                            DeductibleAmountCol4 = OEmpITDataTemp == null ? null : string.Format("{0:0.00}", OEmpITDataTemp.ActualAmount),
                            DBTrack = dbt
                        };
                        OITForm16DataList.Add(OITForm16DataSave);
                    }
                }

                //{
                //    OEmpITDataTemp = OITProjection.Where(e => e.PickupId == 130).SingleOrDefault();
                //    if (OEmpITDataTemp != null)
                //    {
                //        ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                //        {
                //            HeaderCol1 = "  Tax on Total Income without Tax Rebate",
                //            ActualAmountCol2 = "",
                //            QualifyAmountCol3 = "",
                //            DeductibleAmountCol4 = "",
                //            //FinalAmountCol5 = OEmpITDataTemp == null ? null : OEmpITDataTemp.ActualAmount.ToString(),
                //            FinalAmountCol5 = OEmpITDataTemp == null ? null : string.Format("{0:0.00}", OEmpITDataTemp.ActualAmount),

                //            DBTrack = dbt
                //        };
                //        OITForm16DataList.Add(OITForm16DataSave);
                //    }
                //}


                {
                    //   OEmpITDataTemp = OITProjection.Where(e => e.PickupId == 133).SingleOrDefault();
                    OEmpITDataTemp = OITProjection.Where(e => e.PickupId == 130).SingleOrDefault();
                    if (OEmpITDataTemp != null)
                    {
                        ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                        {
                            //  HeaderCol1 = "12. Tax on Total Income",
                            HeaderCol1 = "13. Tax on total income",
                            ActualAmountCol2 = "",
                            QualifyAmountCol3 = "",
                            DeductibleAmountCol4 = OEmpITDataTemp == null ? null : string.Format("{0:0.00}", OEmpITDataTemp.ActualAmount),
                            // FinalAmountCol5 = OEmpITDataTemp == null ? null : OEmpITDataTemp.ActualAmount.ToString(),
                            FinalAmountCol5 = "",
                            // FinalAmountCol5 = OEmpITDataTemp == null ? null : string.Format("{0:0.00}", OEmpITDataTemp.ActualAmount),
                            DBTrack = dbt
                        };
                        OITForm16DataList.Add(OITForm16DataSave);
                    }
                }

                {
                    OEmpITDataTemp = OITProjection.Where(e => e.PickupId == 132).SingleOrDefault();
                    if (OEmpITDataTemp != null)
                    {
                        ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                        {
                            // HeaderCol1 = "  Tax Rebate u/s 87A",
                            HeaderCol1 = "14. Rebate under section 87A, if applicable",
                            ActualAmountCol2 = "",
                            QualifyAmountCol3 = "",
                            DeductibleAmountCol4 = OEmpITDataTemp == null ? null : string.Format("{0:0.00}", OEmpITDataTemp.ActualAmount),
                            // FinalAmountCol5 = OEmpITDataTemp == null ? null : OEmpITDataTemp.ActualAmount.ToString(),
                            FinalAmountCol5 = "",
                            // FinalAmountCol5 = OEmpITDataTemp == null ? null : string.Format("{0:0.00}", OEmpITDataTemp.ActualAmount),
                            DBTrack = dbt
                        };
                        OITForm16DataList.Add(OITForm16DataSave);
                    }
                    else
                    {
                        ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                        {

                            HeaderCol1 = "14. Rebate under section 87A, if applicable",
                            ActualAmountCol2 = "",
                            QualifyAmountCol3 = "",
                            DeductibleAmountCol4 = string.Format("{0:0.00}", 0),
                            // FinalAmountCol5 = OEmpITDataTemp == null ? null : OEmpITDataTemp.ActualAmount.ToString(),
                            FinalAmountCol5 = "",
                            // FinalAmountCol5 = string.Format("{0:0.00}", 0),
                            DBTrack = dbt
                        };
                        OITForm16DataList.Add(OITForm16DataSave);
                    }
                }
                // surcharge

                {
                    OEmpITDataTemp = OITProjection.Where(e => e.PickupId == 134).SingleOrDefault();
                    if (OEmpITDataTemp != null)
                    {
                        ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                        {
                            // HeaderCol1 = "  Tax Rebate u/s 87A",
                            HeaderCol1 = "15. Surcharge, wherever applicable",
                            ActualAmountCol2 = "",
                            QualifyAmountCol3 = "",
                            DeductibleAmountCol4 = OEmpITDataTemp == null ? null : string.Format("{0:0.00}", OEmpITDataTemp.ActualAmount),
                            // FinalAmountCol5 = OEmpITDataTemp == null ? null : OEmpITDataTemp.ActualAmount.ToString(),
                            FinalAmountCol5 = "",
                            // FinalAmountCol5 = OEmpITDataTemp == null ? null : string.Format("{0:0.00}", OEmpITDataTemp.ActualAmount),

                            DBTrack = dbt
                        };
                        OITForm16DataList.Add(OITForm16DataSave);
                    }
                    else
                    {
                        ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                        {

                            HeaderCol1 = "15. Surcharge, wherever applicable",
                            ActualAmountCol2 = "",
                            QualifyAmountCol3 = "",
                            DeductibleAmountCol4 = string.Format("{0:0.00}", 0),
                            // FinalAmountCol5 = OEmpITDataTemp == null ? null : OEmpITDataTemp.ActualAmount.ToString(),
                            FinalAmountCol5 = "",
                            // FinalAmountCol5 = string.Format("{0:0.00}", 0),

                            DBTrack = dbt
                        };
                        OITForm16DataList.Add(OITForm16DataSave);
                    }
                }



                //surcharge missing
                {
                    OEmpITDataTemp = OITProjection.Where(e => e.PickupId == 135).SingleOrDefault();
                    if (OEmpITDataTemp != null)
                    {
                        ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                        {
                            //  HeaderCol1 = "13. Education Cess(on tax at Sr. No. 12) ",
                            HeaderCol1 = "16. Health and education Cess",
                            ActualAmountCol2 = "",
                            QualifyAmountCol3 = "",
                            DeductibleAmountCol4 = OEmpITDataTemp == null ? null : string.Format("{0:0.00}", OEmpITDataTemp.ActualAmount),
                            // FinalAmountCol5 = OEmpITDataTemp == null ? null : OEmpITDataTemp.ActualAmount.ToString(),
                            FinalAmountCol5 = "",
                            // FinalAmountCol5 = OEmpITDataTemp == null ? null : string.Format("{0:0.00}", OEmpITDataTemp.ActualAmount),

                            DBTrack = dbt
                        };
                        OITForm16DataList.Add(OITForm16DataSave);
                    }
                }
                {
                    OEmpITDataTemp = OITProjection.Where(e => e.PickupId == 136).SingleOrDefault();
                    if (OEmpITDataTemp != null)
                    {
                        ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                        {
                            //  HeaderCol1 = "14. Tax payable (12+13)",
                            HeaderCol1 = "17. Tax payable (13+15+16-14)",
                            ActualAmountCol2 = "",
                            QualifyAmountCol3 = "",
                            DeductibleAmountCol4 = OEmpITDataTemp == null ? null : string.Format("{0:0.00}", OEmpITDataTemp.ActualAmount),
                            // FinalAmountCol5 = OEmpITDataTemp == null ? null : OEmpITDataTemp.ActualAmount.ToString(),
                            FinalAmountCol5 = "",
                            // FinalAmountCol5 = OEmpITDataTemp == null ? null : string.Format("{0:0.00}", OEmpITDataTemp.ActualAmount),

                            DBTrack = dbt
                        };
                        OITForm16DataList.Add(OITForm16DataSave);
                    }
                }
                {
                    OEmpITDataTemp = OITProjection.Where(e => e.PickupId == 139).SingleOrDefault();
                    if (OEmpITDataTemp != null)
                    {
                        ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                        {
                            //HeaderCol1 = "15. Relief under section 89(attach details)",
                            HeaderCol1 = "18. Less: Relief under section 89(attach details)",
                            ActualAmountCol2 = "",
                            QualifyAmountCol3 = "",
                            DeductibleAmountCol4 = OEmpITDataTemp == null ? null : string.Format("{0:0.00}", OEmpITDataTemp.ActualAmount),
                            // FinalAmountCol5 = OEmpITDataTemp == null ? null : OEmpITDataTemp.ActualAmount.ToString(),
                            FinalAmountCol5 = "",
                            // FinalAmountCol5 = OEmpITDataTemp == null ? null : string.Format("{0:0.00}", OEmpITDataTemp.ActualAmount),

                            DBTrack = dbt
                        };
                        OITForm16DataList.Add(OITForm16DataSave);
                    }
                    else
                    {
                        ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                        {
                            //HeaderCol1 = "15. Relief under section 89(attach details)",
                            HeaderCol1 = "18. Less: Relief under section 89(attach details)",
                            ActualAmountCol2 = "",
                            QualifyAmountCol3 = "",
                            DeductibleAmountCol4 = string.Format("{0:0.00}", 0),
                            // FinalAmountCol5 = OEmpITDataTemp == null ? null : OEmpITDataTemp.ActualAmount.ToString(),
                            FinalAmountCol5 = "",
                            //FinalAmountCol5 = string.Format("{0:0.00}", 0),

                            DBTrack = dbt
                        };
                        OITForm16DataList.Add(OITForm16DataSave);
                    }
                }
                //final tax payable
                {
                    //OEmpITDataTemp = OITProjection.Where(e => e.PickupId == 1401).SingleOrDefault();
                    OEmpITDataTemp = OITProjection.Where(e => e.PickupId == 1390).SingleOrDefault();
                    if (OEmpITDataTemp != null)
                    {
                        ITForm16DataDetails OITForm16DataSave = new ITForm16DataDetails
                        {
                            // HeaderCol1 = "16. Tax payable (14-15)",
                            HeaderCol1 = "19. Net tax payable (17-18)",
                            ActualAmountCol2 = "",
                            QualifyAmountCol3 = "",
                            DeductibleAmountCol4 = OEmpITDataTemp == null ? null : string.Format("{0:0.00}", OEmpITDataTemp.ActualAmount),
                            // FinalAmountCol5 = OEmpITDataTemp == null ? null : OEmpITDataTemp.ActualAmount.ToString(),
                            FinalAmountCol5 = "",
                            // FinalAmountCol5 = OEmpITDataTemp == null ? null : string.Format("{0:0.00}", OEmpITDataTemp.ActualAmount),


                            DBTrack = dbt
                        };
                        OITForm16DataList.Add(OITForm16DataSave);
                    }

                }
                //perk in form12ba

                List<ITForm12BADataDetails> ITForm12BADataDet = new List<ITForm12BADataDetails>();
                {
                    List<ITProjection> OEmpITDataTemp1 = OITProjection.Where(e => e.PickupId == 33).ToList();

                    //if (OEmpITDataTemp1.Count() > 0)
                    //{
                    List<ITForm12BACaptionMapping> OForm12BA = db.ITForm12BACaptionMapping.Include(e => e.PerquisiteName).Include(e => e.SalaryHead).ToList();
                    double TotAmt = 0;
                    foreach (ITForm12BACaptionMapping a in OForm12BA)
                    {
                        double Amount = 0;
                        foreach (ITProjection ca in OEmpITDataTemp1)
                        {
                            foreach (SalaryHead b in a.SalaryHead)
                            {
                                if (b.Name == ca.ChapterName.Trim())
                                {
                                    Amount = Amount + ca.ActualAmount;
                                }
                            }
                            TotAmt = TotAmt + Amount;
                        }
                        if (a.PerquisiteName.LookupVal.ToUpper().Contains("TOTAL VALUE OF"))
                        {
                            ITForm12BADataDetails OITForm12DataSave = new ITForm12BADataDetails
                            {
                                ITForm12BACaptionMapping = a,
                                PerquisiteActual = 0,
                                PerquisiteChargable = TotAmt,
                                PerquisiteRule = 0,
                                DBTrack = dbt
                            };
                            ITForm12BADataDet.Add(OITForm12DataSave);
                        }
                        else
                        {
                            ITForm12BADataDetails OITForm12DataSave = new ITForm12BADataDetails
                            {
                                ITForm12BACaptionMapping = a,
                                PerquisiteActual = 0,
                                PerquisiteChargable = Amount,
                                PerquisiteRule = 0,
                                DBTrack = dbt
                            };
                            ITForm12BADataDet.Add(OITForm12DataSave);
                        }
                    }
                    //}
                }



                var CompDet = db.Company.Where(e => e.Id == mCompanyId).Select(e => new { PANNo = e.PANNo, TANNo = e.TANNo }).SingleOrDefault();
                Employee EmpDet = OEmployeePayroll.Employee;



                List<ITForm16Quarter> Form16Q = db.ITForm16Quarter.Include(e => e.QuarterName).Include(e => e.ITChallan)
                      .Where(e => e.QuarterFromDate >= mFromPeriod && e.QuarterToDate <= mToPeriod).ToList();

                List<ITForm16QuarterEmpDetails> ITForm16QuarterEmpDetails = new List<ITForm16QuarterEmpDetails>();
                if (Form16Q.Count > 0)
                {
                    foreach (ITForm16Quarter formq in Form16Q)
                    {
                        List<ITChallan> OChallanDet = formq.ITChallan.ToList();
                        double OChallanEmpAmt = 0;
                        foreach (ITChallan C in OChallanDet)
                        {
                            List<ITChallanEmpDetails> OEmpDet = db.EmployeePayroll.Include(e => e.ITChallanEmpDetails).Where(e => e.Id == OEmployeePayroll.Id).SelectMany(e => e.ITChallanEmpDetails).ToList();
                            OChallanEmpAmt = OChallanEmpAmt + OEmpDet.Where(e => e.ChallanNo == C.ChallanNo).Sum(e => e.TaxAmount);
                        }
                        ITForm16QuarterEmpDetails Form16Quarter = new ITForm16QuarterEmpDetails()
                        {
                            QuarterAckNo = formq.QuarterAckNo,
                            QuarterFromDate = formq.QuarterFromDate,
                            QuarterName = formq.QuarterName,
                            QuarterToDate = formq.QuarterToDate,
                            TaxableIncome = OChallanEmpAmt,
                            EmpTaxDeducted = OChallanEmpAmt,
                            DBTrack = formq.DBTrack
                        };
                        ITForm16QuarterEmpDetails.Add(Form16Quarter);
                    }
                }
                // dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };

                ITForm16SigningPerson ITSignPers = db.ITForm16SigningPerson.Where(e => e.Id == SigningPerson).SingleOrDefault();
                ITForm16Data ITForm16DataS = new ITForm16Data
                {
                    CompPAN = CompDet.PANNo,
                    CompTAN = CompDet.TANNo,
                    EmpAddress = EmpDet.ResAddr == null ? "" : EmpDet.ResAddr.FullAddress,
                    EmpAdhar = EmpDet.EmpOffInfo == null || EmpDet.EmpOffInfo.NationalityID == null ? null : EmpDet.EmpOffInfo.NationalityID.AdharNo,
                    EmpCode = EmpDet.EmpCode,
                    EmpDesignation = EmpDet.FuncStruct == null || EmpDet.FuncStruct.Job == null ? null : EmpDet.FuncStruct.Job.Name,
                    EmpName = EmpDet.EmpName == null ? "" : EmpDet.EmpName.FullNameFML,
                    EmpPAN = EmpDet.EmpOffInfo == null || EmpDet.EmpOffInfo.NationalityID == null ? "" : EmpDet.EmpOffInfo.NationalityID.PANNo,
                    FinancialYear = db.Calendar.Where(e => e.Id == OFinancialYear).SingleOrDefault(),
                    //ITChallanEmpDetails = null,
                    ITForm16DataDetails = OITForm16DataList,
                    ITForm16QuarterEmpDetails = ITForm16QuarterEmpDetails,
                    ITForm12BADataDetails = ITForm12BADataDet,
                    ITForm16SigningPerson = ITSignPers,
                    PeriodFrom = mFromPeriod,
                    PeriodTo = mToPeriod,
                    ReportDate = DateTime.Now,
                    DBTrack = dbt
                };
                db.ITForm16Data.Add(ITForm16DataS);
                db.SaveChanges();

                List<ITForm16Data> Form16Data = new List<ITForm16Data>();
                if (OEmployeePayroll.ITForm16Data != null)
                {
                    Form16Data.AddRange(OEmployeePayroll.ITForm16Data);
                }
                Form16Data.Add(ITForm16DataS);
                int OEmpId = OEmployeePayroll.Id;

                EmployeePayroll aa = db.EmployeePayroll.Where(a => a.Id == OEmpId).SingleOrDefault();

                aa.ITForm16Data = Form16Data;
                db.EmployeePayroll.Attach(aa);
                db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
            }
        }
        public static void DeleteForm16Data(ITForm16Data OForm16T, Int32 OFinancialYr)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                if (OForm16T != null)
                {
                    int Form16Id = OForm16T.Id;
                    ITForm16Data OForm16TChk = db.ITForm16Data.Where(e => e.FinancialYear.Id == OFinancialYr && e.Id == Form16Id).SingleOrDefault();
                    List<ITForm16DataDetails> ITForm16DataDetails = db.ITForm16DataDetails.Where(e => e.ITForm16Data_Id == OForm16TChk.Id).ToList();
                    OForm16TChk.ITForm16DataDetails = ITForm16DataDetails;
                    List<ITForm16QuarterEmpDetails> ITForm16QuarterEmpDetails = db.ITForm16QuarterEmpDetails.Where(e => e.ITForm16Data_Id == OForm16TChk.Id).ToList();
                    OForm16TChk.ITForm16DataDetails = ITForm16DataDetails;
                    List<ITForm12BADataDetails> ITForm12BADataDetails = db.ITForm12BADataDetails.Where(e => e.ITForm16Data_Id == OForm16TChk.Id).ToList();
                    OForm16TChk.ITForm12BADataDetails = ITForm12BADataDetails;

                    //.Include(e => e.ITForm16DataDetails)
                    //.Include(e => e.ITForm16QuarterEmpDetails)
                    //.Include(e => e.ITForm12BADataDetails)



                    List<ITForm16DataDetails> OForm16DataDetails = OForm16TChk.ITForm16DataDetails == null ? null : OForm16TChk.ITForm16DataDetails.ToList();
                    if (OForm16DataDetails != null)
                    {
                        db.ITForm16DataDetails.RemoveRange(OForm16DataDetails);
                    }

                    List<ITForm16QuarterEmpDetails> OQuarterDetails = OForm16TChk.ITForm16QuarterEmpDetails == null ? null : OForm16TChk.ITForm16QuarterEmpDetails.ToList();
                    if (OQuarterDetails != null)
                    {
                        db.ITForm16QuarterEmpDetails.RemoveRange(OQuarterDetails);
                    }

                    List<ITForm12BADataDetails> O12BADetails = OForm16TChk.ITForm12BADataDetails == null ? null : OForm16TChk.ITForm12BADataDetails.ToList();
                    if (O12BADetails != null)
                    {
                        db.ITForm12BADataDetails.RemoveRange(O12BADetails);
                    }

                    db.ITForm16Data.Remove(OForm16TChk);
                    db.SaveChanges();

                }
            }
        }

        //public static void DeleteForm24Data(int Form24Id)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {

        //        if (OForm24T != null)
        //        {

        //            ITForm24QData OForm24TChk = db.ITForm24QData
        //               .Include(e => e.ITForm24QDataDetails)
        //               .Where(e => e.Id == Form24Id).SingleOrDefault(); 

        //            List<ITForm24QDataDetails> OForm24DataDetails = OForm24TChk.ITForm24QDataDetails == null ? null : OForm24TChk.ITForm24QDataDetails.ToList();
        //            if (OForm24DataDetails != null)
        //            {
        //                db.ITForm24QDataDetails.RemoveRange(OForm24DataDetails);
        //            }

        //            db.ITForm24QData.Remove(OForm24TChk);
        //            db.SaveChanges();

        //        }
        //    }
        //}

        public static double ITCalculation(EmployeePayroll OEmployeePayroll, int mCompanyId, Int32 OFinancialYear, DateTime mFromPeriod,
           DateTime mToPerod, DateTime ProcessDate, SalaryT OSalaryTC, string AmountChk, int Flag, int ProcType)
        {
            double RetAmt = 0;
            //List<ITSection24Payment> ITsec24Pay = new List<ITSection24Payment>();
            //List<ITInvestmentPayment> ITInvestPay = new List<ITInvestmentPayment>();

            List<ITProjection> FinalOITProjectionDataList = new List<ITProjection>();
            using (DataBaseContext db = new DataBaseContext())
            {
                //Utility.DumpProcessStatus(LineNo: 4132);
                CompanyPayroll OCompanyPayroll = IncomeTaxCalc._returnCompanyPayroll_PTaxMaster(mCompanyId);
                CompanyPayroll OIncomeTax = IncomeTaxCalc._returnCompanyPayroll_IncomeTax_New(mCompanyId, OFinancialYear);
                //List< SalaryT >OSalaryTEmp = IncomeTaxCalc._returnEmployeePayroll_SalaryT(OEmployeePayroll.Id);
                //List<SalaryT> OSalaryTEmp = db.SalaryT.AsNoTracking().Where(e => e.EmployeePayroll_Id == OEmployeePayroll.Id).ToList();
                List<SalaryT> OSalaryTEmp = db.SalaryT.AsNoTracking().Where(e => e.EmployeePayroll_Id == OEmployeePayroll.Id && e.FinnanceYearId_Id == OFinancialYear).ToList();

                //delete Old projection record
                //var Regimi = db.EmployeePayroll.Include(e => e.RegimiScheme).Include(e => e.RegimiScheme.Select(x => x.FinancialYear)).Include(e => e.RegimiScheme.Select(y => y.Scheme)).Where(e => e.Id == OEmployeePayroll.Id).FirstOrDefault();
                //var Regimischemecurryear = Regimi.RegimiScheme.Where(e => e.FinancialYear.Id == OFinancialYear).FirstOrDefault();

                double sec80GActGross;
                double sec80GporGross;

                List<ItProjectionTempClass> OITProjectionDataList = new List<ItProjectionTempClass>();
                if (Flag == 1)
                {
                    if (OEmployeePayroll.ITProjection != null)
                    {
                        //OEmployeePayroll.ITProjection.Any(e => e.FinancialYear.Id == OFinancialYear)

                        var chkITProjection = OEmployeePayroll.ITProjection.Where(e => e.FinancialYear.Id == OFinancialYear).ToList();
                        if (chkITProjection.Count > 0)
                        {
                            //db.Entry(chkITProjection).State = System.Data.Entity.EntityState.Deleted;
                            //db.ITProjection.RemoveRange(chkITProjection);
                            OEmployeePayroll.ITProjection.Where(x => x.FinancialYear.Id == OFinancialYear).ToList().ForEach(x =>
                            {
                                db.ITProjection.Remove(db.ITProjection.Where(a => a.Id == x.Id).SingleOrDefault());
                            });
                            db.SaveChanges();

                            //foreach (Int32 ca in OITProjectionDataListDel)
                            //{
                            //    db.ITProjection.Remove(db.ITProjection.Where(a => a.Id == ca).SingleOrDefault());
                            //    db.SaveChanges();
                            //}
                            ////ITSection24Payment OITSec24PayDel = OEmployeePayroll.ITSection24Payment.Where(e => e.FinancialYear.Id == OFinancialYear).SingleOrDefault();
                            ////db.ITSection24Payment.Remove(OITSec24PayDel);

                            ////ITSection10Payment OITSec10PayDel = OEmployeePayroll.ITSection10Payment.Where(e => e.FinancialYear.Id == OFinancialYear).SingleOrDefault();
                            ////db.ITSection10Payment.Remove(OITSec10PayDel);

                            ////ITInvestmentPayment OITInvestPayDel = OEmployeePayroll.ITInvestmentPayment.Where(e => e.FinancialYear.Id == OFinancialYear).SingleOrDefault();
                            ////db.ITInvestmentPayment.Remove(OITInvestPayDel);
                        }
                    }
                    List<ITSalaryHeadData> OITProjectionHeadDataDel = new List<ITSalaryHeadData>();
                    OITProjectionHeadDataDel = db.ITSalaryHeadData.ToList();
                    if (OITProjectionHeadDataDel.Count > 0)
                    {
                        db.ITSalaryHeadData.RemoveRange(OITProjectionHeadDataDel);
                        db.SaveChanges();
                    }
                }

                //Utility.DumpProcessStatus(LineNo: 4160);
                List<ITSalaryHeadData> OITSalIncome = new List<ITSalaryHeadData>();

                IncomeTax OITMaster = OIncomeTax.IncomeTax.Where(e => e.FyCalendar.Id == OFinancialYear).SingleOrDefault();
                //calculate balance months

                String mPeriodRange = "";
                List<string> mPeriod = new List<string>();
                DateTime mEndDate = Convert.ToDateTime("01/" + Convert.ToDateTime(mToPerod).ToString("MM/yyyy")).AddMonths(1).Date;
                mEndDate = mEndDate.AddDays(-1).Date;
                for (DateTime mTempDate = mFromPeriod; mTempDate <= mEndDate; mTempDate = mTempDate.AddMonths(1))
                {
                    if (mPeriodRange == "")
                    {
                        mPeriodRange = Convert.ToDateTime(mTempDate).ToString("MM/yyyy");
                        mPeriod.Add(Convert.ToDateTime(mTempDate).ToString("MM/yyyy"));
                    }
                    else
                    {
                        mPeriodRange = mPeriodRange + "," + Convert.ToDateTime(mTempDate).ToString("MM/yyyy");
                        mPeriod.Add(Convert.ToDateTime(mTempDate).ToString("MM/yyyy"));
                    }
                }
                String OLastMonth = "";
                String OMonthChk = "";

                //if (OSalaryTC != null)
                //{ OSalaryTEmp.Add(OSalaryTC); }

                if (OSalaryTEmp.Count > 0)
                {
                    if (OSalaryTC != null)
                    {
                        OMonthChk = OSalaryTEmp.Max(e => Convert.ToDateTime("01/" + e.PayMonth).Date).ToString("MM/yyyy");
                        if (OMonthChk == OSalaryTC.PayMonth)
                        {
                            OMonthChk = OSalaryTEmp.Max(e => Convert.ToDateTime("01/" + e.PayMonth).Date).AddMonths(-1).ToString("MM/yyyy");
                        }
                        else
                        {
                            OMonthChk = OSalaryTEmp.Max(e => Convert.ToDateTime("01/" + e.PayMonth).Date).ToString("MM/yyyy");
                        }
                    }
                    else
                    {
                        OMonthChk = OSalaryTEmp.Max(e => Convert.ToDateTime("01/" + e.PayMonth).Date).ToString("MM/yyyy");
                    }


                }
                else // salary not process or new join
                {
                    OMonthChk = ProcessDate.AddMonths(-1).ToString("MM/yyyy");
                    OMonthChk = Convert.ToDateTime(mFromPeriod.AddMonths(-1)).ToString("MM/yyyy");
                }

                if (OMonthChk == ProcessDate.ToString("MM/yyyy"))
                {
                    OLastMonth = ProcessDate.ToString("MM/yyyy");
                }
                else
                {
                    OLastMonth = OMonthChk;//ProcessDate.AddMonths(-1).ToString("MM/yyyy");
                }

                if (mToPerod.Date < ProcessDate.Date)
                {
                    if (OSalaryTEmp.Where(e => e.PayMonth == mToPerod.ToString("MM/yyyy")).SingleOrDefault() != null)
                    {
                        OLastMonth = mToPerod.ToString("MM/yyyy");
                    }
                    else
                    {
                        OLastMonth = OMonthChk;
                    }
                }

                double mBalMonths = (mToPerod.Month + mToPerod.Year * 12) - (Convert.ToDateTime("01/" + OLastMonth).Month + Convert.ToDateTime("01/" + OLastMonth).Year * 12);
                //till here same code
                List<string> mClosedMonths = new List<string>();
                string mClosedPeriodRange = "";
                for (DateTime mTempDate = mFromPeriod; mTempDate < Convert.ToDateTime("01/" + OLastMonth).AddMonths(1).Date; mTempDate = mTempDate.AddMonths(1))
                {
                    if (mClosedPeriodRange == "")
                    {
                        mClosedMonths.Add(Convert.ToDateTime(mTempDate).ToString("MM/yyyy"));
                    }
                    else
                    {
                        mClosedMonths.Add(Convert.ToDateTime(mTempDate).ToString("MM/yyyy"));
                    }
                }
                //Utility.DumpProcessStatus(LineNo: 4233);

                // for yearly payment month period start
                DateTime? FromDateyear = db.Calendar.Where(e => e.Id == OFinancialYear)
                                 .Select(e => e.FromDate)
                                .SingleOrDefault();
                DateTime? ToDateyear = db.Calendar.Where(e => e.Id == OFinancialYear)
                              .Select(e => e.ToDate)
                             .SingleOrDefault();
                DateTime FromPeriodyear = Convert.ToDateTime(FromDateyear);
                DateTime ToPeriodyear = Convert.ToDateTime(ToDateyear);

                string mPeriodRangeYear = "";
                List<string> mPeriodYear = new List<string>();
                DateTime mEndDateYear = Convert.ToDateTime("01/" + Convert.ToDateTime(ToPeriodyear).ToString("MM/yyyy")).AddMonths(1).Date;
                mEndDateYear = mEndDateYear.AddDays(-1).Date;
                for (DateTime mTempDate = FromPeriodyear; mTempDate <= mEndDateYear; mTempDate = mTempDate.AddMonths(1))
                {
                    if (mPeriodRangeYear == "")
                    {
                        mPeriodRangeYear = Convert.ToDateTime(mTempDate).ToString("MM/yyyy");
                        mPeriodYear.Add(Convert.ToDateTime(mTempDate).ToString("MM/yyyy"));
                    }
                    else
                    {
                        mPeriodRangeYear = mPeriodRangeYear + "," + Convert.ToDateTime(mTempDate).ToString("MM/yyyy");
                        mPeriodYear.Add(Convert.ToDateTime(mTempDate).ToString("MM/yyyy"));
                    }
                }


                // for yearly payment month period end


                List<ITSalaryHeadData> OITSalMonthWise = new List<ITSalaryHeadData>();
                // var OITSalMonthWise = OITSalM;
                //here 
                List<ITSalaryHeadDataTemp> OITSalMonthWise_temp = SalaryHeadMonthData(OEmployeePayroll, ProcessDate, mFromPeriod, mToPerod, OSalaryTEmp, mPeriodYear, OFinancialYear);
                //***************** Full and Final Settlement *****************//
                foreach (ITSalaryHeadDataTemp item in OITSalMonthWise_temp)
                {
                    OITSalMonthWise.Add(new ITSalaryHeadData
                    {
                        ActualAmount = item.ActualAmount,
                        PayMonth = item.PayMonth,
                        ProjectedAmount = item.ProjectedAmount,
                        SalaryHead = db.SalaryHead.Include(a => a.SalHeadOperationType).Include(e => e.Type).Where(e => e.Id == item.SalaryHead).SingleOrDefault()
                    });
                }
                //**************** Final Salary Head Total *******************//
                double mActITIncomeSalary = 0;
                double mProjITIncomeSalary = 0;

                var OSalaryHeadTotal = OITSalMonthWise.GroupBy(e => e.SalaryHead)
                    .Where(t => t.Key.SalHeadOperationType.LookupVal.ToUpper() != "PERK")
                    .Select(r => new { SalaryHead = r.Key, mActITIncome = r.Sum(t => t.ActualAmount), mProjITIncome = r.Sum(t => t.ProjectedAmount) })
                    .ToList();
                mActITIncomeSalary = OSalaryHeadTotal.Where(e => e.SalaryHead.Type.LookupVal.ToUpper() == "EARNING")
                                    .Sum(e => e.mActITIncome);
                mProjITIncomeSalary = OSalaryHeadTotal.Where(e => e.SalaryHead.Type.LookupVal.ToUpper() == "EARNING")
                                    .Sum(e => e.mProjITIncome);
                //Utility.DumpProcessStatus(LineNo: 4261);
                //************ writing Header in projection table *****************//
                DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };
                //Employee Details
                {
                    ItProjectionTempClass OITProjectionSave = new ItProjectionTempClass
                    {
                        Tiltle = "Employee Details :-> " + OEmployeePayroll.Employee.EmpCode + " : " +
                                  OEmployeePayroll.Employee.EmpName.FullNameFML + "  PAN No.:-> " +
                                    OEmployeePayroll.Employee.EmpOffInfo.NationalityID.PANNo,
                        FinancialYear = OFinancialYear,
                        Section = "",
                        SectionType = "",
                        ChapterName = "",
                        ProjectedAmount = 0,
                        ActualAmount = 0,
                        ActualQualifyingAmount = 0,
                        ProjectedQualifyingAmount = 0,
                        //ProjectionDate = DateTime.Now.Date,
                        QualifiedAmount = 0,
                        Narration = "Employee Details",
                        //    DBTrack = dbt,
                        PickupId = 10
                    };
                    OITProjectionDataList.Add(OITProjectionSave);
                }

                Calendar temp_OFinancialYear = db.Calendar.AsNoTracking().Where(e => e.Id == OFinancialYear).SingleOrDefault();
                ItProjectionTempClass OITProjectionSave1 = new ItProjectionTempClass
                {
                    Tiltle = "Assessment Year " + temp_OFinancialYear.FromDate.Value.AddYears(1).ToString("dd/MM/yyyy") + " - " +
                                temp_OFinancialYear.ToDate.Value.AddYears(1).ToString("dd/MM/yyyy") + " - " +
                            "Financial Year " + temp_OFinancialYear.FromDate.Value.ToString("dd/MM/yyyy") + " - " +
                                temp_OFinancialYear.ToDate.Value.ToString("dd/MM/yyyy") +
                            " For Period " + mFromPeriod.ToString("dd/MM/yyyy") + " - " +
                                mToPerod.ToString("dd/MM/yyyy"),
                    FinancialYear = OFinancialYear,
                    FromPeriod = mFromPeriod,
                    ToPeriod = mToPerod,
                    Section = "",
                    SectionType = "",
                    ChapterName = "",
                    SubChapter = "",
                    ProjectedAmount = 0,
                    ActualAmount = 0,
                    ActualQualifyingAmount = 0,
                    ProjectedQualifyingAmount = 0,
                    //ProjectionDate = DateTime.Now.Date,
                    TDSComponents = 0,
                    QualifiedAmount = 0,
                    Narration = "Assessment , financial and Tax period details",
                    //  DBTrack = dbt,
                    PickupId = 20
                };
                OITProjectionDataList.Add(OITProjectionSave1);
                /*********** writing salary income in projection table *****************/
                //Income Chargable Header
                // for new tax slab start 28122020

                DateTime finfrm = Convert.ToDateTime("01/04/2020");
                string newslab = "";
                if (OEmployeePayroll.RegimiScheme.FirstOrDefault() != null)
                {
                    if (OEmployeePayroll.RegimiScheme.FirstOrDefault().Scheme.LookupVal.ToUpper() == "NEWTAX")
                    {
                        newslab = "(New Slab)";
                    }
                    else
                    {
                        newslab = "(Old Slab)";
                    }
                }
                else
                {


                    if (temp_OFinancialYear.FromDate >= finfrm && OEmployeePayroll.Employee.EmpOffInfo.NationalityID.No2 == "Yes")
                    {
                        newslab = "(New Slab)";
                    }
                    else
                    {
                        newslab = "(Old Slab)";
                    }
                }
                // for new tax slab end 28122020

                ItProjectionTempClass OITProjectionSave2 = new ItProjectionTempClass
                {
                    Tiltle = "Income Chargeable Under Head 'Salaries'" + newslab,
                    FinancialYear = OFinancialYear,
                    FromPeriod = null,
                    ToPeriod = null,
                    Section = "",
                    SectionType = "",
                    ChapterName = "",
                    ProjectedAmount = 0,
                    ActualAmount = 0,
                    ActualQualifyingAmount = 0,
                    ProjectedQualifyingAmount = 0,
                    //ProjectionDate = DateTime.Now.Date,
                    Narration = "Income From Salary",
                    //DBTrack = dbt,
                    PickupId = 30
                };
                OITProjectionDataList.Add(OITProjectionSave2);
                //Utility.DumpProcessStatus(LineNo: 4338);
                //Salary head details

                var OSalaryEarnings = OSalaryHeadTotal.Where(e => e.SalaryHead.Type.LookupVal.ToUpper() == "EARNING" && (e.mProjITIncome != 0 || e.mActITIncome != 0)).ToList();
                for (int i = 0; i < OSalaryEarnings.Count(); i++)
                {
                    ItProjectionTempClass OITProjectionSave = new ItProjectionTempClass
                    {
                        Tiltle = "",
                        FinancialYear = OFinancialYear,
                        FromPeriod = null,
                        ToPeriod = null,
                        Section = "",
                        SectionType = "",
                        ChapterName = "    " + OSalaryEarnings[i].SalaryHead.Name,
                        ProjectedAmount = OSalaryEarnings[i].mProjITIncome,
                        ActualAmount = OSalaryEarnings[i].mActITIncome,
                        ActualQualifyingAmount = 0,
                        ProjectedQualifyingAmount = 0,
                        //ProjectionDate = DateTime.Now.Date,
                        Narration = "Salary income head details",
                        //      DBTrack = dbt,
                        PickupId = 31
                    };
                    OITProjectionDataList.Add(OITProjectionSave);
                }

                //***************** taxable income total **********************//

                ItProjectionTempClass OITProjectionSave3 = new ItProjectionTempClass
                {
                    FinancialYear = OFinancialYear,
                    Tiltle = "Total Taxable Salary Income",
                    FromPeriod = null,
                    ToPeriod = null,
                    Section = "",
                    SectionType = "",
                    ChapterName = "",
                    SubChapter = "",
                    //SalaryHeadName="",
                    ProjectedAmount = mProjITIncomeSalary,
                    ActualAmount = mActITIncomeSalary,
                    ActualQualifyingAmount = 0,
                    ProjectedQualifyingAmount = 0,
                    //ProjectionDate = DateTime.Now.Date,
                    TDSComponents = 0,
                    QualifiedAmount = 0,
                    Narration = "Total Taxable Salary Income",
                    //   DBTrack = dbt,
                    PickupId = 32 // chgd by rekha 10
                };

                OITProjectionDataList.Add(OITProjectionSave3);

                //Utility.DumpProcessStatus(LineNo: 4392);

                //}
                ////**************** Perk salary ******************//

                double mActITIncomePerk = 0;
                double mProjITIncomePerk = 0;
                var OSalaryHeadTotalPerk = OITSalMonthWise.GroupBy(e => e.SalaryHead)
                    .Where(t => t.Key.SalHeadOperationType.LookupVal.ToUpper() == "PERK")
                    .Select(r => new { SalaryHead = r.Key, mActITIncome = r.Sum(t => t.ActualAmount), mProjITIncome = r.Sum(t => t.ProjectedAmount) })
                    .ToList();
                mActITIncomePerk = OSalaryHeadTotalPerk.Where(e => e.SalaryHead.Type.LookupVal.ToUpper() == "EARNING")
                                    .Sum(e => e.mActITIncome);
                mProjITIncomePerk = OSalaryHeadTotalPerk.Where(e => e.SalaryHead.Type.LookupVal.ToUpper() == "EARNING")
                                    .Sum(e => e.mProjITIncome);

                //************ writing perk income in projection table *****************//
                //perk head details

                var OSalaryEarningsPerk = OSalaryHeadTotalPerk.Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "PERK").ToList();
                for (int i = 0; i < OSalaryEarningsPerk.Count(); i++)
                {
                    ItProjectionTempClass OITProjectionSave = new ItProjectionTempClass
                    {
                        Tiltle = "",
                        FinancialYear = OFinancialYear,
                        FromPeriod = null,
                        ToPeriod = null,
                        Section = "",
                        SectionType = "",
                        ChapterName = "    " + OSalaryEarningsPerk[i].SalaryHead.Name,
                        ProjectedAmount = OSalaryEarningsPerk[i].mProjITIncome,
                        ActualAmount = OSalaryEarningsPerk[i].mActITIncome,
                        ActualQualifyingAmount = 0,
                        ProjectedQualifyingAmount = 0,
                        //ProjectionDate = DateTime.Now.Date,
                        Narration = "Perk Head wise Taxable Income",
                        //      DBTrack = dbt,
                        PickupId = 33
                    };
                    OITProjectionDataList.Add(OITProjectionSave);
                }
                //Utility.DumpProcessStatus(LineNo: 4434);
                //Total perk income

                ItProjectionTempClass OITProjectionSave4 = new ItProjectionTempClass
                {
                    FinancialYear = OFinancialYear,
                    Tiltle = "Total Perk Income",
                    FromPeriod = null,
                    ToPeriod = null,
                    Section = "",
                    SectionType = "",
                    ChapterName = "",
                    SubChapter = "",
                    //SalaryHeadName="",
                    ProjectedAmount = mProjITIncomePerk,
                    ActualAmount = mActITIncomePerk,
                    ActualQualifyingAmount = 0,
                    ProjectedQualifyingAmount = 0,
                    //ProjectionDate = DateTime.Now.Date,
                    TDSComponents = 0,
                    QualifiedAmount = 0,
                    Narration = "Total Taxable Perk Income",
                    //   DBTrack = dbt,
                    PickupId = 34

                };

                OITProjectionDataList.Add(OITProjectionSave4);

                //Utility.DumpProcessStatus(LineNo: 4463);


                //***************** taxable Total income including Perk and salary **********************//

                ItProjectionTempClass OITProjectionSave44 = new ItProjectionTempClass
                {
                    FinancialYear = OFinancialYear,
                    Tiltle = "Total Income including Salary and Perk",
                    FromPeriod = null,
                    ToPeriod = null,
                    Section = "",
                    SectionType = "",
                    ChapterName = "",

                    //SalaryHeadName="",
                    ProjectedAmount = mProjITIncomeSalary + mProjITIncomePerk,
                    ActualAmount = mActITIncomeSalary + mActITIncomePerk,
                    ActualQualifyingAmount = 0,
                    ProjectedQualifyingAmount = 0,
                    //ProjectionDate = DateTime.Now.Date,
                    Narration = "Salary Total Taxable including Salary and Perk Income",
                    //     DBTrack = dbt,
                    PickupId = 35
                };

                OITProjectionDataList.Add(OITProjectionSave44);


                //Surendra start
                OITMaster = OIncomeTax.IncomeTax.Where(e => e.FyCalendar.Id == OFinancialYear).SingleOrDefault();
                //Surendra end
                //net taxable income calculation
                double mTotalProjTaxIncome = 0; double mTotalActTaxIncome = 0;
                ItProjectionTempClass OIncome = OITProjectionDataList.Where(e => e.PickupId == 35).SingleOrDefault();
                if (OIncome != null) //salary
                {
                    mTotalProjTaxIncome = OIncome.ProjectedAmount;
                    mTotalActTaxIncome = OIncome.ActualAmount;
                }

                //Utility.DumpProcessStatus(LineNo: 4504);
                //***************************** Section 10 - Exemption ************************//

                //Exemptions under Section10

                ItProjectionTempClass OITProjectionSave5 = new ItProjectionTempClass
                {
                    FinancialYear = OFinancialYear,
                    Tiltle = "Exemptions Under Section10",
                    FromPeriod = null,
                    ToPeriod = null,
                    Section = "",
                    SectionType = "",
                    ChapterName = "",
                    ProjectedAmount = 0,
                    ActualAmount = 0,
                    ActualQualifyingAmount = 0,
                    ProjectedQualifyingAmount = 0,
                    //ProjectionDate = DateTime.Now.Date,
                    Narration = "Exemption under section 10 for Declarations unnder Section2",
                    //   DBTrack = dbt,
                    PickupId = 40

                };

                OITProjectionDataList.Add(OITProjectionSave5);
                //Utility.DumpProcessStatus(LineNo: 4530);

                List<ItProjectionTempClass> OITProjectionDataList_10Cal = Section10Cal(OEmployeePayroll, OITMaster, OFinancialYear, OITSalMonthWise, OSalaryTC, mFromPeriod, mToPerod, mClosedMonths, mBalMonths, Flag);
                if (OITProjectionDataList_10Cal.Count > 0)
                {
                    OITProjectionDataList.AddRange(OITProjectionDataList_10Cal);
                }
                OIncome = OITProjectionDataList_10Cal.Where(e => e.PickupId == 42).SingleOrDefault();
                if (OIncome != null)//exemption section10
                {
                    mTotalProjTaxIncome = mTotalProjTaxIncome - OIncome.ProjectedQualifyingAmount;
                    mTotalActTaxIncome = mTotalActTaxIncome - OIncome.ActualQualifyingAmount;
                    //mTotalProjTaxIncome = mTotalProjTaxIncome - OIncome.ProjectedQualifyingAmount;
                    //mTotalActTaxIncome = mTotalActTaxIncome - OIncome.ActualQualifyingAmount;
                }

                //List<ItProjectionTempClass> OIncome1ocal = OITProjectionDataList_10Cal.Where(e => e.PickupId == 42 || e.PickupId == 41).ToList();
                //if (OIncome != null)//exemption section10
                //{
                //    mTotalProjTaxIncome = mTotalProjTaxIncome - OIncome1ocal.Sum(q => q.ProjectedAmount);
                //    mTotalActTaxIncome = mTotalActTaxIncome - OIncome1ocal.Sum(q => q.ActualAmount);
                //    //mTotalProjTaxIncome = mTotalProjTaxIncome - OIncome.ProjectedQualifyingAmount;
                //    //mTotalActTaxIncome = mTotalActTaxIncome - OIncome.ActualQualifyingAmount;
                //}

                //List<ItProjectionTempClass> OIncomeList = OITProjectionDataList_10Cal.Where(e => e.PickupId == 41).ToList();
                //if (OIncomeList.Count > 0)
                //{
                //    mTotalProjTaxIncome = mTotalProjTaxIncome + OIncomeList.Sum(q => q.ProjectedAmount);
                //    mTotalActTaxIncome = mTotalActTaxIncome + OIncomeList.Sum(q => q.ActualAmount);
                //}
                ItProjectionTempClass OITProjectionSave6 = new ItProjectionTempClass
                {
                    FinancialYear = OFinancialYear,
                    Tiltle = "Balance Of Income After Section10",
                    FromPeriod = null,
                    ToPeriod = null,
                    Section = "",
                    SectionType = "",
                    ChapterName = "",
                    ProjectedAmount = mTotalProjTaxIncome,
                    ActualAmount = mTotalActTaxIncome,
                    ActualQualifyingAmount = 0,
                    ProjectedQualifyingAmount = 0,
                    //ProjectionDate = DateTime.Now.Date,
                    Narration = "Balance Of Income After Section10",
                    //DBTrack = dbt,
                    PickupId = 45

                };

                OITProjectionDataList.Add(OITProjectionSave6);

                //Standard Deduction under section 16iii

                ItProjectionTempClass OITProjectionSave7 = new ItProjectionTempClass
                {
                    FinancialYear = OFinancialYear,
                    Tiltle = "Deductions Under Section16(iii)",
                    FromPeriod = null,
                    ToPeriod = null,
                    Section = "",
                    SectionType = "",
                    ChapterName = "",
                    ProjectedAmount = 0,
                    ActualAmount = 0,
                    ActualQualifyingAmount = 0,
                    ProjectedQualifyingAmount = 0,
                    //ProjectionDate = DateTime.Now.Date,
                    Narration = "Deductions under section16(iii)",
                    //DBTrack = dbt,
                    PickupId = 50

                };
                OITProjectionDataList.Add(OITProjectionSave7);
                //Utility.DumpProcessStatus(LineNo: 4590);

                List<ItProjectionTempClass> OITProjectionDataList2_temp = StdDeduction(OEmployeePayroll, OCompanyPayroll, OITSalMonthWise, OFinancialYear, OLastMonth, mToPerod, ProcessDate);
                if (OITProjectionDataList2_temp.Count > 0)
                {
                    OITProjectionDataList.AddRange(OITProjectionDataList2_temp);
                }
                OIncome = OITProjectionDataList2_temp.Where(e => e.PickupId == 51).SingleOrDefault();

                ItProjectionTempClass OIncome1 = OITProjectionDataList.Where(e => e.PickupId == 52).SingleOrDefault();



                double AStdDed = 0;
                double PStdDed = 0;
                if (OIncome != null && OIncome1 != null)//exemption section10
                {
                    PStdDed = OIncome.ProjectedAmount + OIncome1.ProjectedAmount;
                    AStdDed = OIncome.ActualAmount + OIncome1.ActualAmount;
                }
                if (OIncome == null && OIncome1 != null)//exemption section10 Ptax if half yearly and half year month previous month Oincme null(PTAX)
                {
                    PStdDed = OIncome1.ProjectedAmount;
                    AStdDed = OIncome1.ActualAmount;
                }
                if (OIncome != null || OIncome == null)//exemption section10
                {
                    mTotalProjTaxIncome = mTotalProjTaxIncome - PStdDed;
                    mTotalActTaxIncome = mTotalActTaxIncome - AStdDed;

                }

                if (mTotalProjTaxIncome < 0)
                {
                    mTotalProjTaxIncome = 0;
                }
                if (mTotalActTaxIncome < 0)
                {
                    mTotalActTaxIncome = 0;
                }
                //OITProjectionDataList.Add(OITProjectionSave52);

                //Total of section16iii

                ItProjectionTempClass OITProjectionSave8 = new ItProjectionTempClass
                {
                    FinancialYear = OFinancialYear,
                    Tiltle = "Total of Section16 III",
                    FromPeriod = null,
                    ToPeriod = null,
                    Section = "",
                    SectionType = "",
                    ChapterName = "",
                    ProjectedAmount = OIncome != null ? PStdDed : 0,
                    ActualAmount = OIncome != null ? AStdDed : 0,
                    ActualQualifyingAmount = AStdDed,
                    ProjectedQualifyingAmount = PStdDed,
                    //ProjectionDate = DateTime.Now.Date,
                    Narration = "Total of Section16 (iii)",
                    //DBTrack = dbt,
                    PickupId = 55

                };

                OITProjectionDataList.Add(OITProjectionSave8);


                //Total Income Chargeable

                ItProjectionTempClass OITProjectionSave9 = new ItProjectionTempClass
                {
                    FinancialYear = OFinancialYear,
                    Tiltle = "Total Income Chargeable",
                    FromPeriod = null,
                    ToPeriod = null,
                    Section = "",
                    SectionType = "",
                    ChapterName = "",
                    ProjectedAmount = mTotalProjTaxIncome,
                    ActualAmount = mTotalActTaxIncome,
                    ActualQualifyingAmount = 0,
                    ProjectedQualifyingAmount = 0,
                    //ProjectionDate = DateTime.Now.Date,
                    Narration = "Total Income Chargeable",
                    //DBTrack = dbt,
                    PickupId = 56

                };
                sec80GActGross = mTotalActTaxIncome;
                sec80GporGross = mTotalProjTaxIncome;
                OITProjectionDataList.Add(OITProjectionSave9);

                if (ProcType == 2 || ProcType == 1)
                {
                    ItProjectionTempClass OITProjectionSave99 = new ItProjectionTempClass
                    {
                        FinancialYear = OFinancialYear,
                        Tiltle = "Tax Calculation On Actual Investment & Projected Income",
                        FromPeriod = null,
                        ToPeriod = null,
                        Section = "",
                        SectionType = "",
                        ChapterName = "",
                        ProjectedAmount = mTotalProjTaxIncome,
                        ActualAmount = mTotalProjTaxIncome,
                        ActualQualifyingAmount = 0,
                        ProjectedQualifyingAmount = 0,
                        //ProjectionDate = DateTime.Now.Date,
                        Narration = "Tax Calculation On Actual Investment & Projected Income",
                        //DBTrack = dbt,
                        PickupId = 57
                    };
                    OITProjectionDataList.Add(OITProjectionSave99);
                    mTotalProjTaxIncome = mTotalProjTaxIncome;
                    mTotalActTaxIncome = mTotalProjTaxIncome;
                }



                //Section 24b Housing Interest

                ItProjectionTempClass OITProjectionSave10 = new ItProjectionTempClass
                {
                    FinancialYear = OFinancialYear,
                    Tiltle = "Income(Loss) From House Properties (Self Occupied) Section24 - Interest ",
                    FromPeriod = null,
                    ToPeriod = null,
                    Section = "",
                    SectionType = "",
                    ChapterName = "",
                    ProjectedAmount = 0,
                    ActualAmount = 0,
                    ActualQualifyingAmount = 0,
                    ProjectedQualifyingAmount = 0,
                    //ProjectionDate = DateTime.Now.Date,
                    Narration = "Income (Loss) from House Properties (Self Occupied) Setion24",
                    //DBTrack = dbt,
                    PickupId = 60

                };

                OITProjectionDataList.Add(OITProjectionSave10);
                //Utility.DumpProcessStatus(LineNo: 4697);

                List<ItProjectionTempClass> OITProjectionDataLists_temp = InvestmentSection24Loan(OEmployeePayroll, OITMaster, OITSalMonthWise, OFinancialYear, OLastMonth, OSalaryTC, Flag, ProcType);

                if (OITProjectionDataLists_temp.Count > 0)
                {
                    //ITsec24Pay = OITProjectionDataLists_temp.Where(e => e.PickupId == 61).SelectMany(e => e.ITSection24Pay).ToList();
                    OITProjectionDataList.AddRange(OITProjectionDataLists_temp);
                }
                OIncome = OITProjectionDataLists_temp.Where(e => e.PickupId == 62).SingleOrDefault();
                if (OIncome != null)//interest section24
                {
                    mTotalProjTaxIncome = mTotalProjTaxIncome - OIncome.ProjectedQualifyingAmount;
                    mTotalActTaxIncome = mTotalActTaxIncome - OIncome.ActualQualifyingAmount;
                }

                //Section 24b income from Housing Property(Let out)

                ItProjectionTempClass OITProjectionSave11 = new ItProjectionTempClass
                {
                    FinancialYear = OFinancialYear,
                    Tiltle = "Income(Loss) From House Properties (Let Out) Section24 - Income ",
                    FromPeriod = null,
                    ToPeriod = null,
                    Section = "",
                    SectionType = "",
                    ChapterName = "",
                    ProjectedAmount = 0,
                    ActualAmount = 0,
                    ActualQualifyingAmount = 0,
                    ProjectedQualifyingAmount = 0,
                    //ProjectionDate = DateTime.Now.Date,
                    Narration = "Income (Loss) from House Properties (Let Out) Setion24",
                    //     DBTrack = dbt,
                    PickupId = 70

                };

                OITProjectionDataList.Add(OITProjectionSave11);

                //Utility.DumpProcessStatus(LineNo: 4735);
                List<ItProjectionTempClass> OITProjectionDataList_stemp = InvestmentSection24Property(OEmployeePayroll, OITMaster, OITSalMonthWise, OFinancialYear);
                if (OITProjectionDataList_stemp.Count > 0)
                {
                    OITProjectionDataList.AddRange(OITProjectionDataList_stemp);
                }
                OIncome = OITProjectionDataList_stemp.Where(e => e.PickupId == 72).SingleOrDefault();
                if (OIncome != null)//interest section24
                {
                    mTotalProjTaxIncome = mTotalProjTaxIncome + OIncome.ProjectedAmount;
                    mTotalActTaxIncome = mTotalActTaxIncome + OIncome.ActualAmount;
                }

                ////////Gross Total Income

                ItProjectionTempClass OITProjectionSave12 = new ItProjectionTempClass
                {
                    FinancialYear = OFinancialYear,
                    Tiltle = "Gross Total Income ",
                    FromPeriod = null,
                    ToPeriod = null,
                    Section = "",
                    SectionType = "",
                    ChapterName = "",
                    ProjectedAmount = mTotalProjTaxIncome,
                    ActualAmount = mTotalActTaxIncome,
                    ActualQualifyingAmount = 0,
                    ProjectedQualifyingAmount = 0,
                    //ProjectionDate = DateTime.Now.Date,
                    Narration = "Gross Total Income",
                    //     DBTrack = dbt,
                    PickupId = 85

                };

                OITProjectionDataList.Add(OITProjectionSave12);


                ////////********* Other Income - to be added - ********************//

                ////////Deductions under chapter VI

                ItProjectionTempClass OITProjectionSave13 = new ItProjectionTempClass
                {
                    FinancialYear = OFinancialYear,
                    Tiltle = "Deductions under Chapter VI ",
                    FromPeriod = null,
                    ToPeriod = null,
                    Section = "",
                    SectionType = "",
                    ChapterName = "",
                    ProjectedAmount = 0,
                    ActualAmount = 0,
                    ActualQualifyingAmount = 0,
                    ProjectedQualifyingAmount = 0,
                    //ProjectionDate = DateTime.Now.Date,
                    Narration = "Deductions Under Chapter VI",
                    //  DBTrack = dbt,
                    PickupId = 90

                };

                OITProjectionDataList.Add(OITProjectionSave13);


                ////////investment under section 80C

                ItProjectionTempClass OITProjectionSave14 = new ItProjectionTempClass
                {
                    FinancialYear = OFinancialYear,
                    Tiltle = "Investment under Section80C ",
                    FromPeriod = null,
                    ToPeriod = null,
                    Section = "",
                    SectionType = "",
                    ChapterName = "",
                    ProjectedAmount = 0,
                    ActualAmount = 0,
                    ActualQualifyingAmount = 0,
                    ProjectedQualifyingAmount = 0,
                    //ProjectionDate = DateTime.Now.Date,
                    Narration = "Investment under Section80C",
                    //    DBTrack = dbt,
                    PickupId = 91

                };

                OITProjectionDataList.Add(OITProjectionSave14);

                //Utility.DumpProcessStatus(LineNo: 4824);

                List<ItProjectionTempClass> OITProjectionDataList_80C = Investment80C(OEmployeePayroll, OITMaster, OITSalMonthWise, OFinancialYear, OLastMonth, OSalaryTC, Flag, ProcType);
                if (OITProjectionDataList_80C.Count > 0)
                {
                    OITProjectionDataList.AddRange(OITProjectionDataList_80C);
                }
                OIncome = OITProjectionDataList_80C.Where(e => e.PickupId == 93).SingleOrDefault();
                if (OIncome != null)//section80c
                {
                    mTotalProjTaxIncome = mTotalProjTaxIncome - OIncome.ProjectedQualifyingAmount;
                    mTotalActTaxIncome = mTotalActTaxIncome - OIncome.ActualQualifyingAmount;
                }

                //investment under section 80CCD-80CCG

                ItProjectionTempClass OITProjectionSave15 = new ItProjectionTempClass
                {
                    FinancialYear = OFinancialYear,
                    Tiltle = "Investment under Section80CC-CCG ",
                    FromPeriod = null,
                    ToPeriod = null,
                    Section = "",
                    SectionType = "",
                    ChapterName = "",
                    ProjectedAmount = 0,
                    ActualAmount = 0,
                    ActualQualifyingAmount = 0,
                    ProjectedQualifyingAmount = 0,
                    //ProjectionDate = DateTime.Now.Date,
                    Narration = "Investment under Section80CC-CCG",
                    //   DBTrack = dbt,
                    PickupId = 100

                };

                OITProjectionDataList.Add(OITProjectionSave15);

                //Utility.DumpProcessStatus(LineNo: 4862);
                List<ItProjectionTempClass> OITProjectionDataList_sad = InvestmentRebatNot80C(OEmployeePayroll, OITMaster, OITSalMonthWise, OFinancialYear, OSalaryTC, Flag);
                if (OITProjectionDataList_sad.Count > 0)
                {
                    OITProjectionDataList.AddRange(OITProjectionDataList_sad);
                }
                OIncome = OITProjectionDataList_sad.Where(e => e.PickupId == 102).SingleOrDefault();
                if (OIncome != null)//section80ccc-d
                {
                    mTotalProjTaxIncome = mTotalProjTaxIncome - OIncome.ProjectedQualifyingAmount;
                    mTotalActTaxIncome = mTotalActTaxIncome - OIncome.ActualQualifyingAmount;
                }

                ////////investment under section 80D-U

                ItProjectionTempClass OITProjectionSave16 = new ItProjectionTempClass
                {
                    FinancialYear = OFinancialYear,
                    Tiltle = "Investment under Section80D-U ",
                    FromPeriod = null,
                    ToPeriod = null,
                    Section = "",
                    SectionType = "",
                    ChapterName = "",
                    ProjectedAmount = 0,
                    ActualAmount = 0,
                    ActualQualifyingAmount = 0,
                    ProjectedQualifyingAmount = 0,
                    //ProjectionDate = DateTime.Now.Date,
                    Narration = "Investment under Section80D-U",
                    //     DBTrack = dbt,
                    PickupId = 110

                };

                OITProjectionDataList.Add(OITProjectionSave16);
                //Utility.DumpProcessStatus(LineNo: 4898);
                List<ItProjectionTempClass> OITProjectionDataList_80DtoU = Investment80DtoU(sec80GActGross, sec80GporGross, OEmployeePayroll, OITMaster, OITSalMonthWise, OFinancialYear, OSalaryTC, Flag);
                if (OITProjectionDataList_80DtoU.Count > 0)
                {
                    OITProjectionDataList.AddRange(OITProjectionDataList_80DtoU);
                }
                OIncome = OITProjectionDataList_80DtoU.Where(e => e.PickupId == 112).SingleOrDefault();
                if (OIncome != null)//section80ccc-d
                {
                    mTotalProjTaxIncome = mTotalProjTaxIncome - OIncome.ProjectedQualifyingAmount;
                    mTotalActTaxIncome = mTotalActTaxIncome - OIncome.ActualQualifyingAmount;
                }

                OIncome = OITProjectionDataList.Where(e => e.PickupId == 93).SingleOrDefault();
                double ActAggInvTot = OIncome.ActualQualifyingAmount;
                double ProjAggInvTot = OIncome.ProjectedQualifyingAmount;

                OIncome = OITProjectionDataList.Where(e => e.PickupId == 102).SingleOrDefault();
                ActAggInvTot = ActAggInvTot + OIncome.ActualQualifyingAmount;
                ProjAggInvTot = ProjAggInvTot + OIncome.ProjectedQualifyingAmount;

                OIncome = OITProjectionDataList.Where(e => e.PickupId == 112).SingleOrDefault();
                ActAggInvTot = ActAggInvTot + OIncome.ActualQualifyingAmount;
                ProjAggInvTot = ProjAggInvTot + OIncome.ProjectedQualifyingAmount;

                ////////Agg Investment Total

                ItProjectionTempClass OITProjectionSave17 = new ItProjectionTempClass
                {
                    FinancialYear = OFinancialYear,
                    Tiltle = "Aggregate Investment Total ",
                    FromPeriod = null,
                    ToPeriod = null,
                    Section = "",
                    SectionType = "",
                    ChapterName = "",
                    ProjectedAmount = ProjAggInvTot,
                    ActualAmount = ActAggInvTot,
                    ActualQualifyingAmount = 0,
                    ProjectedQualifyingAmount = 0,
                    //ProjectionDate = DateTime.Now.Date,
                    Narration = "Aggregate Investment Total",
                    //   DBTrack = dbt,
                    PickupId = 115

                };

                OITProjectionDataList.Add(OITProjectionSave17);


                if (mTotalProjTaxIncome < 0) { mTotalProjTaxIncome = 0; }
                if (mTotalActTaxIncome < 0) { mTotalActTaxIncome = 0; }
                ////////Net Total Income

                ItProjectionTempClass OITProjectionSave18 = new ItProjectionTempClass
                {
                    FinancialYear = OFinancialYear,
                    Tiltle = "Net Total Income ",
                    FromPeriod = null,
                    ToPeriod = null,
                    Section = "",
                    SectionType = "",
                    ChapterName = "",
                    ProjectedAmount = mTotalProjTaxIncome,
                    ActualAmount = mTotalActTaxIncome,
                    ActualQualifyingAmount = 0,
                    ProjectedQualifyingAmount = 0,
                    //ProjectionDate = DateTime.Now.Date,
                    Narration = "Net Total Income",
                    //  DBTrack = dbt,
                    PickupId = 120

                };

                OITProjectionDataList.Add(OITProjectionSave18);

                //************* Total Taxable Income Calculation start ***************//

                //*************** taxable income Rounding nearest ten rupees *****************//
                if (mTotalProjTaxIncome < 0)
                {
                    mTotalProjTaxIncome = 0;
                }
                else
                {
                    mTotalProjTaxIncome = Math.Round(mTotalProjTaxIncome / 10 + 0.001) * 10;
                }
                if (mTotalActTaxIncome < 0)
                {
                    mTotalActTaxIncome = 0;
                }
                else
                {
                    mTotalActTaxIncome = Math.Round(mTotalActTaxIncome / 10 + 0.001) * 10;
                }

                //Net Total Income rounding under section 288

                ItProjectionTempClass OITProjectionSave19 = new ItProjectionTempClass
                {
                    FinancialYear = OFinancialYear,
                    Tiltle = "Net Total Rounded Income under Section288",
                    FromPeriod = null,
                    ToPeriod = null,
                    Section = "",
                    SectionType = "",
                    ChapterName = "",
                    ProjectedAmount = mTotalProjTaxIncome,
                    ActualAmount = mTotalActTaxIncome,
                    ActualQualifyingAmount = 0,
                    ProjectedQualifyingAmount = 0,
                    //ProjectionDate = DateTime.Now.Date,
                    Narration = "Investment under Section80D-U",
                    // DBTrack = dbt,
                    PickupId = 121

                };

                OITProjectionDataList.Add(OITProjectionSave19);
                //Utility.DumpProcessStatus(LineNo: 5017);
                //tds calculation    
                Double[] mIncomeTaxDetailsActual = new Double[5];
                mIncomeTaxDetailsActual = TDSCalc(OEmployeePayroll, OITMaster, mTotalActTaxIncome, mToPerod, OFinancialYear);
                Double[] mIncomeTaxDetailsProjected = new Double[5];
                mIncomeTaxDetailsProjected = TDSCalc(OEmployeePayroll, OITMaster, mTotalProjTaxIncome, mToPerod, OFinancialYear);
                //*************** TDS variable **********************//
                double mTotalTDSProj = 0; double mTotalTDSAct = 0;
                //mIncomeTaxDetailsProjected[0] -> income tax
                //mIncomeTaxDetailsProjected[1] = EduCessPercent;
                //mIncomeTaxDetailsProjected[2] = SurchargePercent;
                //mIncomeTaxDetailsProjected[3] = EduCessAmount;
                //mIncomeTaxDetailsProjected[4] = SurchargeAmount;
                mTotalTDSProj = mIncomeTaxDetailsProjected[0];// +mIncomeTaxDetailsProjected[1] + mIncomeTaxDetailsProjected[2];
                mTotalTDSAct = mIncomeTaxDetailsActual[0];// +mIncomeTaxDetailsActual[1] + mIncomeTaxDetailsActual[2];
                //TDS calculation before section89 

                ItProjectionTempClass OITProjectionSave20 = new ItProjectionTempClass
                {
                    FinancialYear = OFinancialYear,
                    Tiltle = "Income Tax on Total Amount",
                    FromPeriod = mFromPeriod,
                    ToPeriod = mToPerod,
                    Section = "",
                    SectionType = "",
                    ChapterName = "",
                    SubChapter = "",
                    //SalaryHeadName="",
                    ProjectedAmount = mTotalTDSProj,
                    ActualAmount = mTotalTDSAct,
                    ActualQualifyingAmount = 0,
                    ProjectedQualifyingAmount = 0,
                    //ProjectionDate = DateTime.Now.Date,
                    TDSComponents = 0,
                    QualifiedAmount = 0,
                    Narration = "Income tax liability as per TDS act",
                    //   DBTrack = dbt,
                    PickupId = 130
                };
                OITProjectionDataList.Add(OITProjectionSave20);

                //************** section 87 ************************//

                //Rebate under section87 

                ItProjectionTempClass OITProjectionSave21 = new ItProjectionTempClass
                {
                    FinancialYear = OFinancialYear,
                    Tiltle = "Rebate under Section87",
                    FromPeriod = mFromPeriod,
                    ToPeriod = mToPerod,
                    Section = "",
                    SectionType = "",
                    ChapterName = "",
                    SubChapter = "",
                    //SalaryHeadName="",
                    ProjectedAmount = 0,
                    ActualAmount = 0,
                    ActualQualifyingAmount = 0,
                    ProjectedQualifyingAmount = 0,
                    //ProjectionDate = DateTime.Now.Date,
                    TDSComponents = 0,
                    QualifiedAmount = 0,
                    Narration = "Rebate under Section87",
                    //    DBTrack = dbt,
                    PickupId = 131
                };
                OITProjectionDataList.Add(OITProjectionSave21);

                //Utility.DumpProcessStatus(LineNo: 5086);
                double mQAct87 = 0;
                double mQProj87 = 0;
                double mAct87 = 0;
                double mProj87 = 0;
                List<ItProjectionTempClass> OITProjectionDataList_dsad = RebateSection87(OEmployeePayroll, OITMaster, OFinancialYear, mTotalActTaxIncome, mTotalProjTaxIncome, mTotalTDSAct, mTotalTDSProj);
                if (OITProjectionDataList_dsad.Count > 0)
                {
                    OITProjectionDataList.AddRange(OITProjectionDataList_dsad);
                }
                OIncome = OITProjectionDataList_dsad.Where(e => e.PickupId == 132).SingleOrDefault();
                if (OIncome != null)
                {
                    mTotalTDSProj = mTotalTDSProj - OIncome.ProjectedAmount;
                    mTotalTDSAct = mTotalTDSAct - OIncome.ActualAmount;
                    //if (mTotalTDSAct >= 0)
                    //{
                    //    if (mTotalTDSAct > OIncome.ActualAmount)
                    //    {
                    //        mTotalTDSAct = mTotalTDSAct - OIncome.ActualAmount;
                    //        mQAct87 = OIncome.ActualAmount;
                    //        mAct87 = OIncome.ActualAmount;
                    //    }
                    //    else
                    //    {
                    //        mAct87 = OIncome.ActualAmount;
                    //        mQAct87 = mTotalTDSAct;
                    //        mTotalTDSAct = mTotalTDSAct - OIncome.ActualAmount;
                    //        if (mTotalTDSAct < 0)
                    //        {
                    //            mTotalTDSAct = 0;
                    //        }
                    //        //   mTotalTDSAct = 0;

                    //    }
                    //    if (mTotalTDSProj > OIncome.ProjectedAmount)
                    //    {
                    //        mTotalTDSProj = mTotalTDSProj - OIncome.ProjectedAmount;
                    //        mQProj87 = OIncome.ProjectedAmount;
                    //        mProj87 = OIncome.ProjectedAmount;
                    //    }
                    //    else
                    //    {
                    //        mProj87 = OIncome.ProjectedAmount;
                    //        mQProj87 = mTotalTDSProj;
                    //        mTotalTDSProj = mTotalTDSProj - OIncome.ProjectedAmount;
                    //        if (mTotalTDSProj < 0)
                    //        {
                    //            mTotalTDSProj = 0;
                    //        }
                    //    }



                    //}
                }

                //Final total tax after due

                ItProjectionTempClass OITProjectionSave22 = new ItProjectionTempClass
                {
                    FinancialYear = OFinancialYear,
                    Tiltle = "Final Total Tax due",
                    FromPeriod = mFromPeriod,
                    ToPeriod = mToPerod,
                    Section = "",
                    SectionType = "",
                    ChapterName = "",
                    SubChapter = "",
                    //SalaryHeadName="",
                    ProjectedAmount = mTotalTDSProj,
                    ActualAmount = mTotalTDSAct,
                    ActualQualifyingAmount = 0,
                    ProjectedQualifyingAmount = 0,
                    //ProjectionDate = DateTime.Now.Date,

                    Narration = "Finat Tax after Section 87 tax rebate ",
                    //   DBTrack = dbt,
                    PickupId = 133
                };
                OITProjectionDataList.Add(OITProjectionSave22);


                //mIncomeTaxDetailsProjected[0] -> income tax
                //mIncomeTaxDetailsProjected[1] = EduCessPercent;
                //mIncomeTaxDetailsProjected[2] = SurchargePercent;
                //mIncomeTaxDetailsProjected[3] = EduCessAmount;
                //mIncomeTaxDetailsProjected[4] = SurchargeAmount;
                double mEduCessAct = 0;
                double mEduCessProj = 0;
                double mSurchargeAct = 0;
                double mSurchargeProj = 0;
                mEduCessAct = Math.Round(((mTotalTDSAct * mIncomeTaxDetailsActual[1] / 100) + mIncomeTaxDetailsActual[3]), 0);
                mEduCessProj = Math.Round(((mTotalTDSProj * mIncomeTaxDetailsProjected[1] / 100) + mIncomeTaxDetailsProjected[3]), 0);
                mSurchargeAct = Math.Round(((mTotalTDSAct * mIncomeTaxDetailsActual[2] / 100) + mIncomeTaxDetailsActual[4]), 0);
                mSurchargeProj = Math.Round(((mTotalTDSProj * mIncomeTaxDetailsProjected[2] / 100) + mIncomeTaxDetailsProjected[4]), 0);


                ItProjectionTempClass OITProjectionSave23 = new ItProjectionTempClass
                {
                    FinancialYear = OFinancialYear,
                    Tiltle = "Surcharge on Tax",
                    FromPeriod = mFromPeriod,
                    ToPeriod = mToPerod,
                    Section = "",
                    SectionType = "",
                    ChapterName = "",
                    SubChapter = "",
                    //SalaryHeadName="",
                    ProjectedAmount = mSurchargeProj,
                    ActualAmount = mSurchargeAct,
                    ActualQualifyingAmount = 0,
                    ProjectedQualifyingAmount = 0,
                    //ProjectionDate = DateTime.Now.Date,

                    Narration = "Surcharge Amount as per TDS Act",
                    //  DBTrack = dbt,
                    PickupId = 134
                };
                OITProjectionDataList.Add(OITProjectionSave23);


                //education cess on Income Tax and Surcharge amount

                ItProjectionTempClass OITProjectionSave24 = new ItProjectionTempClass
                {
                    FinancialYear = OFinancialYear,
                    Tiltle = "Health & Education Cess on Tax",
                    FromPeriod = mFromPeriod,
                    ToPeriod = mToPerod,
                    Section = "",
                    SectionType = "",
                    ChapterName = "",
                    SubChapter = "",
                    //SalaryHeadName="",
                    ProjectedAmount = mEduCessProj,
                    ActualAmount = mEduCessAct,
                    ActualQualifyingAmount = 0,
                    ProjectedQualifyingAmount = 0,
                    //ProjectionDate = DateTime.Now.Date,

                    Narration = "Health & Education Cess as per TDS act",
                    //     DBTrack = dbt,
                    PickupId = 135
                };
                OITProjectionDataList.Add(OITProjectionSave24);



                //final tax including edu cess and surcharge
                mTotalTDSAct = mTotalTDSAct + mEduCessAct + mSurchargeAct;
                mTotalTDSProj = mTotalTDSProj + mEduCessProj + mSurchargeProj;
                //Final Tax Liability

                ItProjectionTempClass OITProjectionSave25 = new ItProjectionTempClass
                {
                    FinancialYear = OFinancialYear,
                    Tiltle = "Final Total Income Tax Liability",
                    FromPeriod = mFromPeriod,
                    ToPeriod = mToPerod,
                    Section = "",
                    SectionType = "",
                    ChapterName = "",
                    SubChapter = "",
                    //SalaryHeadName="",
                    ProjectedAmount = mTotalTDSProj,
                    ActualAmount = mTotalTDSAct,
                    ActualQualifyingAmount = 0,
                    ProjectedQualifyingAmount = 0,
                    //ProjectionDate = DateTime.Now.Date,
                    TDSComponents = 0,
                    QualifiedAmount = 0,
                    Narration = "Total Income tax liability including Edu Cess and Surcharge ",
                    //     DBTrack = dbt,
                    PickupId = 136
                };
                OITProjectionDataList.Add(OITProjectionSave25);


                //relief tds data
                //Relief under Section89

                ItProjectionTempClass OITProjectionSave26 = new ItProjectionTempClass
                {
                    FinancialYear = OFinancialYear,
                    Tiltle = "Tax Relief Under Section89",
                    FromPeriod = mFromPeriod,
                    ToPeriod = mToPerod,
                    Section = "",
                    SectionType = "",
                    ChapterName = "",
                    SubChapter = "",
                    //SalaryHeadName="",
                    ProjectedAmount = 0,
                    ActualAmount = 0,
                    ActualQualifyingAmount = 0,
                    ProjectedQualifyingAmount = 0,
                    //ProjectionDate = DateTime.Now.Date,
                    TDSComponents = 0,
                    QualifiedAmount = 0,
                    Narration = "Total Income tax Relief under Section89 ",
                    //    DBTrack = dbt,
                    PickupId = 137
                };
                OITProjectionDataList.Add(OITProjectionSave26);

                //Utility.DumpProcessStatus(LineNo: 5280);

                List<ItProjectionTempClass> OITProjectionDataList_24 = ReliefSection89(OEmployeePayroll, OITMaster, OFinancialYear);
                if (OITProjectionDataList_24.Count > 0)
                {
                    OITProjectionDataList.AddRange(OITProjectionDataList_24);
                }
                OIncome = OITProjectionDataList_24.Where(e => e.PickupId == 139).SingleOrDefault();
                if (OIncome != null)//section89 relief
                {
                    mTotalTDSProj = mTotalTDSProj - OIncome.ProjectedAmount;
                    mTotalTDSAct = mTotalTDSAct - OIncome.ActualAmount;
                }


                ItProjectionTempClass OITProjectionSave27 = new ItProjectionTempClass
                {
                    FinancialYear = OFinancialYear,
                    Tiltle = "Total Final Tax",
                    FromPeriod = mFromPeriod,
                    ToPeriod = mToPerod,
                    Section = "",
                    SectionType = "",
                    ChapterName = "",
                    SubChapter = "",
                    //SalaryHeadName="",
                    ProjectedAmount = mTotalTDSProj,
                    ActualAmount = mTotalTDSAct,
                    ActualQualifyingAmount = 0,
                    ProjectedQualifyingAmount = 0,
                    //ProjectionDate = DateTime.Now.Date,
                    Narration = "Total Final Tax",
                    //     DBTrack = dbt,
                    PickupId = 1390
                };
                OITProjectionDataList.Add(OITProjectionSave27);


                //till date tax paid
                double OITtaxPaid = OITSalMonthWise.Where(r => r.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "ITAX")
                    .Sum(e => e.ActualAmount);
                List<YearlyPaymentT> OSalaryYearlyPaymenttDeduction = new List<YearlyPaymentT>();
                var emppayforothearndedANDYearlyPay = db.EmployeePayroll
                                                        .Include(q => q.OtherEarningDeductionT)
                                                        .Include(q => q.YearlyPaymentT)
                                                         .AsNoTracking().Where(q => q.Id == OEmployeePayroll.Id).SingleOrDefault();
                foreach (string item in mPeriodYear)
                {
                    List<YearlyPaymentT> OPerkTransT_l = emppayforothearndedANDYearlyPay.YearlyPaymentT
                               .Where(s => s.PayMonth == item)
                               .ToList();
                    if (OPerkTransT_l != null)
                    {
                        OSalaryYearlyPaymenttDeduction.AddRange(OPerkTransT_l);
                    }
                }
                //var OSalaryYearlyPaymenttDeduction = OSalaryTEmp.YearlyPaymentT
                //     .Where(s => mPeriod.Contains(s.PayMonth))
                //     .ToList();
                foreach (YearlyPaymentT ca in OSalaryYearlyPaymenttDeduction)
                {
                    double a = OITSalIncome.Where(r => r.SalaryHead.Id == ca.SalaryHead.Id && r.PayMonth == ca.PayMonth).Distinct().Count();
                    if (a <= 0)
                    {
                        OITtaxPaid = OITtaxPaid + ca.TDSAmount;
                    }
                }
                List<OthEarningDeductionT> OSalaryOthEarningDeduction = new List<OthEarningDeductionT>();
                foreach (string item in mPeriod)
                {
                    List<OthEarningDeductionT> OPerkTransT_l = emppayforothearndedANDYearlyPay.OtherEarningDeductionT
                               .Where(s => s.PayMonth == item)
                               .ToList();
                    if (OPerkTransT_l.Count > 0)
                    {
                        OSalaryOthEarningDeduction.AddRange(OPerkTransT_l);
                    }
                }
                //var OSalaryOthEarningDeduction = OSalaryTEmp.OtherEarningDeductionT
                //    .Where(s => mPeriod.Contains(s.PayMonth))
                //    .ToList();
                foreach (OthEarningDeductionT ca in OSalaryOthEarningDeduction)
                {
                    double a = OITSalIncome.Where(r => r.SalaryHead.Id == ca.SalaryHead.Id && r.PayMonth == ca.PayMonth).Distinct().Count();
                    if (a <= 0)
                    {
                        OITtaxPaid = OITtaxPaid + ca.TDSAmount;
                    }
                }
                if (OITtaxPaid == null) { OITtaxPaid = 0; }

                //**** Total balance tax *************//
                double mBalITaxAct = (mTotalTDSAct - OITtaxPaid);
                if (mBalITaxAct < 0) { mBalITaxAct = 0; }
                double mBalITaxProj = (mTotalTDSProj - OITtaxPaid);
                if (mBalITaxProj < 0) { mBalITaxProj = 0; }

                double mTaxPerMonthAct = 0;
                double mTaxPerMonthProj = 0;

                //if (OSalaryTC != null)
                //{
                //    mBalMonths = mBalMonths - 1;
                //}

                double mTaxPerMonthActAuto = 0;
                double mTaxPerMonthProjAuto = 0;


                if (OSalaryTC != null)
                {
                    if (mBalMonths > 0)
                    {
                        mTaxPerMonthActAuto = Math.Round(mBalITaxAct / mBalMonths, 0);
                        mTaxPerMonthProjAuto = Math.Round(mBalITaxProj / mBalMonths, 0);
                    }
                    if (mBalMonths == 0)
                    {
                        mTaxPerMonthActAuto = Math.Round(mBalITaxAct, 0);
                        mTaxPerMonthProjAuto = Math.Round(mBalITaxProj, 0);
                    }

                    if (AmountChk == "Actual")
                    { OITtaxPaid = mTaxPerMonthActAuto + OITtaxPaid; }
                    else if (AmountChk == "Declared")
                    { OITtaxPaid = mTaxPerMonthProjAuto + OITtaxPaid; }

                    mBalITaxAct = (mTotalTDSAct - OITtaxPaid);
                    mBalITaxProj = (mTotalTDSProj - OITtaxPaid);

                    if (mBalMonths != 0)
                    {
                        mBalMonths = mBalMonths - 1;
                    }

                    if (mBalMonths == 0)
                    {
                        //mTaxPerMonthAct = mTaxPerMonthActAuto;
                        //mTaxPerMonthProj = mTaxPerMonthProjAuto;
                        mTaxPerMonthAct = mBalITaxAct;
                        mTaxPerMonthProj = mBalITaxProj;
                    }
                    else
                    {
                        mTaxPerMonthAct = Math.Round(mBalITaxAct / mBalMonths, 0);
                        mTaxPerMonthProj = Math.Round(mBalITaxProj / mBalMonths, 0);
                    }


                }
                else
                {
                    if (mBalMonths > 0)
                    {
                        mTaxPerMonthAct = Math.Round(mBalITaxAct / mBalMonths, 0);
                        mTaxPerMonthProj = Math.Round(mBalITaxProj / mBalMonths, 0);

                    }
                }


                ItProjectionTempClass OITProjectionSave28 = new ItProjectionTempClass
                {
                    FinancialYear = OFinancialYear,
                    Tiltle = "Tax Paid till date",
                    FromPeriod = mFromPeriod,
                    ToPeriod = mToPerod,
                    Section = "",
                    SectionType = "",
                    ChapterName = "",
                    SubChapter = "",
                    //SalaryHeadName="",
                    ProjectedAmount = OITtaxPaid,
                    ActualAmount = OITtaxPaid,
                    ActualQualifyingAmount = 0,
                    ProjectedQualifyingAmount = 0,
                    //ProjectionDate = DateTime.Now.Date,

                    Narration = "Income tax Paid till date",
                    //  DBTrack = dbt,
                    PickupId = 140
                };
                OITProjectionDataList.Add(OITProjectionSave28);


                ItProjectionTempClass OITProjectionSave29 = new ItProjectionTempClass
                {
                    FinancialYear = OFinancialYear,
                    Tiltle = "Balance Income Tax to be paid",
                    FromPeriod = mFromPeriod,
                    ToPeriod = mToPerod,
                    Section = "",
                    SectionType = "",
                    ChapterName = "",
                    SubChapter = "",
                    //SalaryHeadName="",
                    ProjectedAmount = mBalITaxProj,
                    ActualAmount = mBalITaxAct,
                    ActualQualifyingAmount = mBalITaxAct,
                    ProjectedQualifyingAmount = mBalITaxProj,
                    //ProjectionDate = DateTime.Now.Date,
                    TDSComponents = 0,
                    QualifiedAmount = 0,
                    Narration = "Income tax Paid till date",
                    //   DBTrack = dbt,
                    PickupId = 141
                };
                OITProjectionDataList.Add(OITProjectionSave29);

                //balance Months



                ItProjectionTempClass OITProjectionSave30 = new ItProjectionTempClass
                {
                    FinancialYear = OFinancialYear,
                    Tiltle = "Remaining Months in a year till " + mToPerod.ToString("dd/MM/yyyy"),
                    FromPeriod = null,
                    ToPeriod = null,
                    Section = "",
                    SectionType = "",
                    ChapterName = "",
                    SubChapter = "",
                    //SalaryHeadName="",
                    ProjectedAmount = mBalMonths,
                    ActualAmount = mBalMonths,
                    ActualQualifyingAmount = 0,
                    ProjectedQualifyingAmount = 0,
                    //ProjectionDate = DateTime.Now.Date,
                    TDSComponents = 0,
                    QualifiedAmount = 0,
                    Narration = "Remaining Months",
                    //   DBTrack = dbt,
                    PickupId = 142
                };
                OITProjectionDataList.Add(OITProjectionSave30);

                //per month tax liability




                ItProjectionTempClass OITProjectionSave31 = new ItProjectionTempClass
                {
                    FinancialYear = OFinancialYear,
                    Tiltle = "Income Tax Amount per Month",
                    FromPeriod = null,
                    ToPeriod = null,
                    Section = "",
                    SectionType = "",
                    ChapterName = "",
                    SubChapter = "",
                    ProjectedAmount = mTaxPerMonthProj,
                    ActualAmount = mTaxPerMonthAct,
                    ActualQualifyingAmount = 0,
                    ProjectedQualifyingAmount = 0,
                    //  ProjectionDate = DateTime.Now.Date,
                    TDSComponents = 0,
                    QualifiedAmount = 0,
                    Narration = "Income tax liability per Month",
                    //   DBTrack = dbt,
                    PickupId = 143
                };
                if (AmountChk == "Actual")
                    RetAmt = mTaxPerMonthActAuto;
                else
                    RetAmt = mTaxPerMonthProjAuto;

                OITProjectionDataList.Add(OITProjectionSave31);

                foreach (ItProjectionTempClass item in OITProjectionDataList)
                {
                    FinalOITProjectionDataList.Add(new ITProjection
                    {
                        FinancialYear = db.Calendar.Where(e => e.Id == OFinancialYear).SingleOrDefault(),
                        Tiltle = item.Tiltle,
                        FromPeriod = item.FromPeriod,
                        ToPeriod = item.ToPeriod,
                        Section = item.Section,
                        SectionType = item.SectionType,
                        ChapterName = item.ChapterName,
                        SubChapter = item.SubChapter,
                        ProjectedAmount = item.ProjectedAmount,
                        ActualAmount = item.ActualAmount,
                        ActualQualifyingAmount = item.ActualQualifyingAmount,
                        ProjectedQualifyingAmount = item.ProjectedQualifyingAmount,
                        ProjectionDate = DateTime.Now.Date,
                        TDSComponents = item.TDSComponents,
                        QualifiedAmount = item.QualifiedAmount,
                        Narration = item.Narration,
                        DBTrack = dbt,
                        PickupId = item.PickupId

                    });
                }

                if (Flag == 1)
                {
                    db.ITProjection.AddRange(FinalOITProjectionDataList);
                    db.SaveChanges();
                    int OEmpId = OEmployeePayroll.Id;

                    //using (DataBaseContext db2 = new DataBaseContext())
                    //{
                    EmployeePayroll aa = db.EmployeePayroll.Where(a => a.Id == OEmpId).SingleOrDefault();

                    aa.ITProjection = FinalOITProjectionDataList;
                    OEmployeePayroll.DBTrack = dbt;
                    db.EmployeePayroll.Attach(aa);
                    db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    //db.Entry(aa).State = System.Data.Entity.EntityState.Detached;
                }
                //Utility.DumpProcessStatus(LineNo: 5528);
                //Utility.DumpProcessStatus(" Income Tax Process Ended");
                //db.Entry(aa).State = System.Data.Entity.EntityState.Detached;
                //db.SaveChanges();
                //}

            }
            //ITSectionPay = ITsec24Pay;
            //ITInvestmentPay = ITInvestPay;
            return RetAmt;
        }


        public static void ITForm24Q(EmployeePayroll OEmployeePayrollId, int mCompanyId, Int32 OFinancialYear,
           DateTime mFromPeriod, DateTime mToPeriod, DateTime ProcessDate, int SigningPerson_Id, int QuarterName_Id, Boolean AnnexureII, Boolean Tax, DateTime dtfrom, DateTime dtTO, bool deductor, bool challan)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                var OEmployeePayroll = db.EmployeePayroll.Where(e => e.Id == OEmployeePayrollId.Id).SingleOrDefault();
                Employee Employee = db.Employee.Where(e => e.Id == OEmployeePayroll.Employee_Id).SingleOrDefault();
                OEmployeePayroll.Employee = Employee;
                LookupValue Gender = db.LookupValue.Where(e => e.Id == Employee.Gender_Id).SingleOrDefault();
                Employee.Gender = Gender;
                List<ITProjection> ITProjection = db.ITProjection.Where(e => e.EmployeePayroll_Id == OEmployeePayroll.Id && e.FinancialYear_Id == OFinancialYear).ToList();
                OEmployeePayroll.ITProjection = ITProjection;
                Calendar FinancialYear = db.Calendar.Where(e => e.Id == OFinancialYear).FirstOrDefault();
                foreach (var i in ITProjection)
                {
                    i.FinancialYear = FinancialYear;
                }

                List<ITForm24QData> ITForm24QData = db.ITForm24QData.Where(e => e.EmployeePayroll_Id == OEmployeePayroll.Id && e.FinancialYear_Id == OFinancialYear).ToList();
                OEmployeePayroll.ITForm24QData = ITForm24QData;
                List<ITChallanEmpDetails> ITChallanEmpDetails = db.ITChallanEmpDetails.Where(e => e.EmployeePayroll_Id == OEmployeePayroll.Id && e.Calendar_Id == OFinancialYear).ToList();
                OEmployeePayroll.ITChallanEmpDetails = ITChallanEmpDetails;
                ServiceBookDates ServiceBookDates = db.ServiceBookDates.Where(e => e.Id == Employee.ServiceBookDates_Id).SingleOrDefault();
                OEmployeePayroll.Employee.ServiceBookDates = ServiceBookDates;
                NameSingle EmpName = db.NameSingle.Where(e => e.Id == Employee.EmpName_Id).SingleOrDefault();
                OEmployeePayroll.Employee.EmpName = EmpName;
                EmpOff EmpOffInfo = db.EmpOff.Where(e => e.Id == Employee.EmpOffInfo_Id).SingleOrDefault();
                OEmployeePayroll.Employee.EmpOffInfo = EmpOffInfo;
                // NationalityID NationalityID = db.NationalityID.Where(e => e.Id == EmpOffInfo.NationalityID_Id).SingleOrDefault();
                var NationalityID = db.Employee.Where(e => e.Id == Employee.Id).Select(r => r.EmpOffInfo.NationalityID).FirstOrDefault();
                OEmployeePayroll.Employee.EmpOffInfo.NationalityID = NationalityID;




                //                   .Include(e => e.Employee)
                //                   .Include(e => e.Employee.Gender).Include(e => e.ITProjection).Include(e => e.ITForm24QData)
                //                   .Include(e => e.ITForm24QData)
                //                   .Include(e => e.ITProjection.Select(r => r.FinancialYear))
                //// .Include(e => e.ITForm24QData.Select(r => r.FinancialYear))
                ////  .Include(e => e.ITForm24QData.Select(r => r.ITForm16Quarter))
                //// .Include(e => e.ITForm24QData.Select(r => r.ITForm24QDataDetails))
                //                   .Include(e => e.ITChallanEmpDetails)
                //// .Include(e => e.ITaxTransT)
                //                   .Include(e => e.Employee.ServiceBookDates)
                ////  .Include(e => e.SalaryT.Select(t => t.SalEarnDedT.Select(s => s.SalaryHead.Type)))
                //                   .Include(e => e.Employee.EmpOffInfo.NationalityID)
                //                   .Include(e => e.Employee.EmpName)
                //// .Include(e => e.YearlyPaymentT)
                //// .Include(e => e.YearlyPaymentT.Select(t => t.SalaryHead))
                // .Include(e => e.SalaryArrearT)
                // .Include(e => e.SalaryArrearT.Select(t => t.SalaryArrearPaymentT.Select(q => q.SalaryHead.Type)))
                // .Include(e => e.YearlyPaymentT.Select(r => r.FinancialYear)).AsParallel()


                //CompanyPayroll OCompanyPayroll = db.CompanyPayroll
                //       .Include(e => e.Company)
                //       .Where(r => r.Company.Id == mCompanyId).AsParallel().SingleOrDefault();


                //CompanyPayroll OIncomeTax = db.CompanyPayroll.Where(e => e.Company.Id == mCompanyId)
                //    .Include(e => e.IncomeTax)
                //    .Include(e => e.IncomeTax.Select(r => r.FyCalendar))
                //    .Include(e => e.IncomeTax.Select(r => r.ITSection))
                //    .Include(e => e.IncomeTax.Select(r => r.ITSection.Select(s => s.ITInvestments)))
                //    .Include(e => e.IncomeTax.Select(r => r.ITSection.Select(s => s.ITInvestments.Select(m => m.ITSubInvestment))))
                //    .Include(e => e.IncomeTax.Select(r => r.ITSection.Select(s => s.ITInvestments.Select(m => m.SalaryHead))))
                //    .Include(e => e.IncomeTax.Select(r => r.ITSection.Select(s => s.LoanAdvanceHead)))
                //    .Include(e => e.IncomeTax.Select(r => r.ITSection.Select(s => s.LoanAdvanceHead.Select(d => d.SalaryHead))))
                //    .Include(e => e.IncomeTax.Select(r => r.ITSection.Select(s => s.LoanAdvanceHead.Select(d => d.ITLoan))))
                //    .Include(e => e.IncomeTax.Select(r => r.ITSection.Select(d => d.ITStandardITRebate)))
                //    .Include(e => e.IncomeTax.Select(r => r.ITSection.Select(d => d.ITSection10)))
                //    .Include(e => e.IncomeTax.Select(r => r.ITSection.Select(d => d.ITSection10.Select(s => s.Itsection10salhead))))
                //    .Include(e => e.IncomeTax.Select(r => r.ITSection.Select(d => d.ITSection10.Select(s => s.Itsection10salhead.Select(c => c.Frequency)))))
                //    .Include(e => e.IncomeTax.Select(r => r.ITSection.Select(d => d.ITSection10.Select(s => s.Itsection10salhead.Select(t => t.SalHead)))))
                //    .Include(e => e.IncomeTax.Select(r => r.ITSection.Select(d => d.ITSectionList)))
                //    .Include(e => e.IncomeTax.Select(r => r.ITSection.Select(d => d.ITSectionListType)))
                //    .Include(e => e.IncomeTax.Select(r => r.ITTDS))
                //    .Include(e => e.IncomeTax.Select(r => r.ITTDS.Select(d => d.Category)))
                //    .SingleOrDefault();
                ITForm16Quarter ITForm16Q = db.ITForm16Quarter.Where(e => e.QuarterName.Id == QuarterName_Id && e.QuarterFromDate >= mFromPeriod && e.QuarterToDate <= mToPeriod).SingleOrDefault();

                //ITForm24QData OForm24QDel = OEmployeePayroll.ITForm24QData.Where(e => e.FinancialYear.Id == OFinancialYear && e.ITForm16Quarter.Id == ITForm16Q.Id).SingleOrDefault();
                //if (OForm24QDel != null)
                //{
                //    var OForm24QDetails = OForm24QDel.ITForm24QDataDetails == null ? null : OForm24QDel.ITForm24QDataDetails.ToList();
                //    if (OForm24QDetails != null)
                //    {
                //        db.ITForm24QDataDetails.RemoveRange(OForm24QDetails);
                //        db.ITForm24QData.Remove(OForm24QDel);
                //    }
                //    db.SaveChanges();

                //}


                DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };

                List<ITForm24QDataDetails> ITForm24QDataDet = new List<ITForm24QDataDetails>();
                {
                    //List<ITProjection> OEmpITDataTemp1 = OITProjection.Where(e => e.PickupId == 33).ToList();
                    List<ITForm24QDataDetails> OITForm24QDataList = new List<ITForm24QDataDetails>();

                    ITForm16SigningPerson ITSignPers = db.ITForm16SigningPerson.Where(e => e.Id == SigningPerson_Id).SingleOrDefault();

                    var QuarterNamefff = db.ITForm16Quarter.Include(q => q.QuarterName).Where(e => e.QuarterName.Id == QuarterName_Id && e.QuarterFromDate >= mFromPeriod && e.QuarterToDate <= mToPeriod).SingleOrDefault();

                    string QuarterName = "";
                    if (QuarterNamefff != null && QuarterNamefff.QuarterName != null)
                    {
                        QuarterName = QuarterNamefff.QuarterName.LookupVal.ToString();
                    }
                    //challan

                    string DataVal = "";
                    //  DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };


                    if (AnnexureII == false)
                    {

                        if (deductor == false)
                        {
                            {
                                int srNo = 0;
                                List<ITForm24QFileFormatDefinition> FileFormatDef = db.ITForm24QFileFormatDefinition.Include(e => e.Form24QFileType).Where(e => e.Form24QFileType.LookupVal.ToUpper() == "DEDUCTOR").AsNoTracking().ToList();
                                //for (DateTime dt = mFromPeriod; dt <= mToPeriod; dt = dt.AddMonths(1))
                                //{
                                //string mFromPeriodM = dt.ToString("MM/yyyy");
                                //var ChallanData = db.ITChallan.Where(e => e.SalaryMonth == mFromPeriodM).SingleOrDefault();
                                //if (ChallanData != null)
                                //{
                                srNo = srNo + 1;
                                int CompId = Convert.ToInt32(SessionManager.CompanyId);
                                var CompDetData = db.Company.Include(e => e.Address).Include(e => e.Address.City).Include(e => e.Address.Area)
                                    .Include(e => e.ContactDetails).Include(e => e.ContactDetails.ContactNumbers)
                                    .Where(e => e.Id == CompId).AsNoTracking().SingleOrDefault();

                                foreach (var a in FileFormatDef)
                                {
                                    if (a.ExcelColNo == "1")
                                        DataVal = CompDetData.TANNo;
                                    else if (a.ExcelColNo == "2")
                                        DataVal = CompDetData.PANNo;
                                    else if (a.ExcelColNo == "3")
                                        DataVal = CompDetData.Name;
                                    else if (a.ExcelColNo == "4")
                                        DataVal = "";
                                    else if (a.ExcelColNo == "5")
                                        DataVal = "";
                                    else if (a.ExcelColNo == "6")
                                        DataVal = "";
                                    else if (a.ExcelColNo == "7")
                                        DataVal = "";
                                    else if (a.ExcelColNo == "8")
                                        DataVal = "";
                                    else if (a.ExcelColNo == "9")
                                        DataVal = CompDetData != null && CompDetData.Address != null && CompDetData.Address.City != null && CompDetData.Address.City.Name != null ? CompDetData.Address.City.Name : "";
                                    else if (a.ExcelColNo == "10")
                                        DataVal = CompDetData != null && CompDetData.Address != null && CompDetData.Address.Area != null && CompDetData.Address.Area.PinCode != null ? CompDetData.Address.Area.PinCode.ToString() : "";
                                    else if (a.ExcelColNo == "11")
                                        DataVal = "";
                                    else if (a.ExcelColNo == "12")
                                        DataVal = CompDetData != null && CompDetData.ContactDetails != null && CompDetData.ContactDetails.ContactNumbers != null ? CompDetData.ContactDetails.ContactNumbers.Select(r => r.LandlineNo).FirstOrDefault() : "";
                                    else if (a.ExcelColNo == "13")
                                        DataVal = CompDetData != null && CompDetData.ContactDetails != null && CompDetData.ContactDetails.EmailId != null ? CompDetData.ContactDetails.EmailId : "";
                                    else if (a.ExcelColNo == "14")
                                        DataVal = "";
                                    else if (a.ExcelColNo == "15")
                                        DataVal = "";
                                    else if (a.ExcelColNo == "16")
                                        DataVal = "";
                                    else if (a.ExcelColNo == "17")
                                        DataVal = "";
                                    else if (a.ExcelColNo == "18")
                                        DataVal = "";
                                    else if (a.ExcelColNo == "19")
                                        DataVal = "";
                                    else if (a.ExcelColNo == "20")
                                        DataVal = "";
                                    else if (a.ExcelColNo == "21")
                                        DataVal = "";
                                    else if (a.ExcelColNo == "22")
                                        DataVal = "";
                                    else if (a.ExcelColNo == "23")
                                        DataVal = "";
                                    else if (a.ExcelColNo == "24")
                                        DataVal = "";
                                    else if (a.ExcelColNo == "25")
                                        DataVal = "";
                                    else if (a.ExcelColNo == "26")
                                        DataVal = "";

                                    ITForm24QDataDetails OITForm24QDataSave = new ITForm24QDataDetails
                                    {
                                        ITForm24QFileFormatDefinition = db.ITForm24QFileFormatDefinition.Find(a.Id),
                                        DataValue = DataVal,
                                        DBTrack = dbt
                                    };
                                    OITForm24QDataList.Add(OITForm24QDataSave);
                                }
                                //}
                                //}
                            }
                        }
                        if (challan == false)
                        {


                            {
                                int srNo = 0;
                                List<ITForm24QFileFormatDefinition> FileFormatDef = db.ITForm24QFileFormatDefinition.Include(e => e.Form24QFileType).Where(e => e.Form24QFileType.LookupVal.ToUpper() == "CHALLAN").AsNoTracking().ToList();
                                for (DateTime dt = mFromPeriod; dt <= mToPeriod; dt = dt.AddMonths(1))
                                {
                                    string mFromPeriodM = dt.ToString("MM/yyyy");
                                    var ChallanData = db.ITChallan.Where(e => e.SalaryMonth == mFromPeriodM).ToList();
                                    if (ChallanData.Count() > 0)
                                    {
                                        foreach (var item in ChallanData)
                                        {
                                            srNo = srNo + 1;
                                            // var ChallanDetData = OEmployeePayroll.ITChallanEmpDetails.Where(e => e.ChallanNo == ChallanData.ChallanNo).SingleOrDefault();
                                            //List<YearlyPaymentT> YearlyPayTDataD = db.YearlyPaymentT.Where(e => e.FinancialYear.Id == OFinancialYear && e.PayMonth == mFromPeriodM).ToList();
                                            double YearlyPayTData = db.YearlyPaymentT.Where(e => e.FinancialYear.Id == OFinancialYear && e.PayMonth == mFromPeriodM).AsNoTracking().ToList().Sum(e => e.AmountPaid);
                                            //double YearlyPayTData = YearlyPayTDataD.Sum(e => e.AmountPaid);

                                            foreach (var a in FileFormatDef)
                                            {
                                                if (a.ExcelColNo == "1")
                                                    DataVal = srNo.ToString();
                                                else if (a.ExcelColNo == "2")
                                                    DataVal = Math.Round(((item.TaxAmount * 100) / 104), 0).ToString();
                                                //DataVal = (item.TaxAmount - Math.Round(((item.TaxAmount * 4) / 100), 0)).ToString();
                                                else if (a.ExcelColNo == "3")
                                                    DataVal = (item.TaxAmount - Math.Round(((item.TaxAmount * 100) / 104), 0)).ToString();
                                                //DataVal = Math.Round(((item.TaxAmount * 4) / 100), 0).ToString();
                                                else if (a.ExcelColNo == "4")
                                                    DataVal = "";
                                                else if (a.ExcelColNo == "5")
                                                    DataVal = "";
                                                else if (a.ExcelColNo == "6")
                                                    DataVal = "";
                                                else if (a.ExcelColNo == "7")
                                                    DataVal = (item.TaxAmount).ToString();
                                                else if (a.ExcelColNo == "8")
                                                    DataVal = "";
                                                else if (a.ExcelColNo == "9")
                                                    DataVal = item.BankBSRCode;
                                                else if (a.ExcelColNo == "10")

                                                    DataVal = item.ChallanNo;
                                                else if (a.ExcelColNo == "11")
                                                    DataVal = item.TaxDepositDate.Value.ToString("dd/MM/yyyy");
                                                else if (a.ExcelColNo == "12")
                                                    DataVal = "";


                                                ITForm24QDataDetails OITForm24QDataSave = new ITForm24QDataDetails
                                                {
                                                    ITForm24QFileFormatDefinition = db.ITForm24QFileFormatDefinition.Find(a.Id),
                                                    DataValue = DataVal,
                                                    DBTrack = dbt,
                                                    ChallanNo = item.ChallanNo,
                                                    TaxDepositedDate = item.TaxDepositDate,
                                                };
                                                OITForm24QDataList.Add(OITForm24QDataSave);
                                            }
                                        }
                                    }
                                }
                            }
                        }


                        //foreach (var OEmployeePayroll in EmpPayroll)

                        //{
                        {
                            List<ITForm24QFileFormatDefinition> FileFormatDef = db.ITForm24QFileFormatDefinition.Include(e => e.Form24QFileType).Where(e => e.Form24QFileType.LookupVal.ToUpper() == "DEDUCTEE").AsNoTracking().ToList();
                            //DateTime mQfrom = DateTime.Now;
                            //DateTime mQto = DateTime.Now;
                            //if (QuarterName.ToUpper() == "QUARTER1")
                            //{
                            //    mQfrom = mFromPeriod;
                            //    mQto = mFromPeriod.AddMonths(2);
                            //}
                            //else if (QuarterName.ToUpper() == "QUARTER2")
                            //{
                            //    mQfrom = mFromPeriod.AddMonths(3);
                            //    mQto = mQfrom.AddMonths(2);
                            //}
                            //else if (QuarterName.ToUpper() == "QUARTER3")
                            //{
                            //    mQfrom = mFromPeriod.AddMonths(6);
                            //    mQto = mQfrom.AddMonths(2);
                            //}
                            //else if (QuarterName.ToUpper() == "QUARTER4")
                            //{
                            //    mQfrom = mFromPeriod.AddMonths(9);
                            //    mQto = mQfrom.AddMonths(2);
                            //}
                            //for (DateTime dt = mQfrom; dt <= mQto; dt = dt.AddMonths(1))
                            //{
                            string mFromPeriodM = dtfrom.ToString("MM/yyyy");
                            int srNo = 0;
                            // var ChallanData = db.ITChallan.Where(e => e.SalaryMonth == mFromPeriodM).AsNoTracking().SingleOrDefault();
                            var ChallanData = db.ITChallan.Where(e => e.SalaryMonth == mFromPeriodM).ToList();


                            double OTaxableAmt1 = 0;

                            if (ChallanData.Count() > 0 && ChallanData != null)
                            {
                                foreach (var item in ChallanData)
                                {
                                    srNo = srNo + 1;
                                    OTaxableAmt1 = 0;
                                    var ChallanDetData = OEmployeePayroll.ITChallanEmpDetails.Where(e => e.ChallanNo == item.ChallanNo).ToList();
                                    //var OTaxableAmt = OEmployeePayroll.SalaryT.Where(e => e.PayMonth == mFromPeriodM).SingleOrDefault();
                                    //List<SalaryArrearT> OtaxableArrear = OEmployeePayroll.SalaryArrearT.Where(e => e.PayMonth == mFromPeriodM).ToList();
                                    double OTDS = 0;
                                    if (Tax == true)
                                    {
                                        double MonthlyOTDS = 0;

                                        OTaxableAmt1 = ChallanDetData.Where(e => e.TaxAmount != 0).Sum(e => e.TaxableIncome);
                                        //   ITaxTransT ItaxMonthlyTDS = OEmployeePayroll.ITaxTransT
                                        //.Where(e => e.ITChallan != null
                                        //    && e.ITChallan.Id == item.Id
                                        //    && e.PayMonth == mFromPeriodM
                                        //    && e.TaxPaid != 0).SingleOrDefault();
                                        //   if (ItaxMonthlyTDS != null)
                                        //   {
                                        //       if (OTaxableAmt != null)
                                        //       {
                                        //           double OTaxableSal = 0;
                                        //           double OTaxableArrear = 0;
                                        //           var smd = OtaxableArrear.SelectMany(r => r.SalaryArrearPaymentT.Where(t => t.SalaryHead.InITax == true && t.SalaryHead.Type.LookupVal.ToUpper() == "EARNING"));
                                        //           OTaxableArrear = smd.Sum(t => t.SalHeadAmount);
                                        //           OTaxableSal = OTaxableAmt.SalEarnDedT.Where(r => r.SalaryHead.InITax == true && r.SalaryHead.Type.LookupVal.ToUpper() == "EARNING").Sum(e => e.Amount);
                                        //           OTaxableAmt1 = OTaxableArrear + OTaxableSal;
                                        //       }
                                        //   }
                                        //   if (ItaxMonthlyTDS != null)
                                        //   {
                                        //       MonthlyOTDS = ItaxMonthlyTDS.TaxPaid;

                                        //   }

                                        //   List<YearlyPaymentT> YearlyPayTDataD1 = OEmployeePayroll.YearlyPaymentT.Where(e => e.ITChallan != null && e.ITChallan.Id == item.Id && e.FinancialYear.Id == OFinancialYear).ToList();
                                        //   List<YearlyPaymentT> yearlygross = OEmployeePayroll.YearlyPaymentT.Where(e => e.ITChallan != null && e.ITChallan.Id == item.Id && e.SalaryHead.InPayslip == false && e.FinancialYear.Id == OFinancialYear).ToList();

                                        //   double YearlyTDS = 0;


                                        //   if (YearlyPayTDataD1.Count() > 0)
                                        //   {
                                        //       YearlyTDS = YearlyPayTDataD1.Sum(e => e.TDSAmount);
                                        //   }
                                        //   if (YearlyTDS != 0)
                                        //   {
                                        //       OTaxableAmt1 = OTaxableAmt1 + yearlygross.Sum(e => e.AmountPaid);

                                        // }


                                        OTDS = ChallanDetData.Where(e => e.TaxAmount != 0).Sum(e => e.TaxAmount);

                                    }
                                    else
                                    {
                                        OTaxableAmt1 = ChallanDetData.Sum(e => e.TaxableIncome);
                                        //ITaxTransT ItaxMonthlyTDS = OEmployeePayroll.ITaxTransT
                                        //                                    .Where(e => e.ITChallan != null
                                        //                                        && e.ITChallan.Id == item.Id
                                        //                                        && e.PayMonth == mFromPeriodM).SingleOrDefault();
                                        //if (ItaxMonthlyTDS != null)
                                        //{
                                        //    if (OTaxableAmt != null)
                                        //    {
                                        //        double OTaxableSal = 0;
                                        //        double OTaxableArrear = 0;
                                        //        var smd = OtaxableArrear.SelectMany(r => r.SalaryArrearPaymentT.Where(t => t.SalaryHead.InITax == true && t.SalaryHead.Type.LookupVal.ToUpper() == "EARNING"));
                                        //        OTaxableArrear = smd.Sum(t => t.SalHeadAmount);
                                        //        OTaxableSal = OTaxableAmt.SalEarnDedT.Where(r => r.SalaryHead.InITax == true && r.SalaryHead.Type.LookupVal.ToUpper() == "EARNING").Sum(e => e.Amount);
                                        //        OTaxableAmt1 = OTaxableArrear + OTaxableSal;
                                        //    }
                                        //}
                                        //List<YearlyPaymentT> YearlyPayTDataD1 = OEmployeePayroll.YearlyPaymentT.Where(e => e.ITChallan != null && e.ITChallan.Id == item.Id && e.FinancialYear.Id == OFinancialYear).ToList();
                                        //List<YearlyPaymentT> yearlygross = OEmployeePayroll.YearlyPaymentT.Where(e => e.ITChallan != null && e.ITChallan.Id == item.Id && e.SalaryHead.InPayslip == false && e.FinancialYear.Id == OFinancialYear).ToList();
                                        //OTaxableAmt1 = OTaxableAmt1 + yearlygross.Sum(e => e.AmountPaid);


                                        //double MonthlyOTDS = 0;
                                        //double YearlyTDS = 0;

                                        //if (ItaxMonthlyTDS != null)
                                        //{
                                        //    MonthlyOTDS = ItaxMonthlyTDS.TaxPaid;

                                        //}
                                        //if (YearlyPayTDataD1.Count() > 0)
                                        //{
                                        //    YearlyTDS = YearlyPayTDataD1.Sum(e => e.TDSAmount);
                                        //}
                                        OTDS = ChallanDetData.Sum(e => e.TaxAmount);
                                    }

                                    //if (OTaxableAmt != null)
                                    //{
                                    //    OTaxableAmt1 = OTaxableAmt.SalEarnDedT.Where(r => r.SalaryHead.InITax == true && r.SalaryHead.Type.LookupVal.ToUpper() == "EARNING").Sum(e => e.Amount);

                                    //}

                                    //List<YearlyPaymentT> YearlyPayTDataD1 = OEmployeePayroll.YearlyPaymentT.Where(e => e.FinancialYear.Id == OFinancialYear && e.PayMonth == mFromPeriodM).ToList();

                                    ////double YearlyPayTData1 = YearlyPayTDataD1.Sum(e => e.AmountPaid);

                                    //OTaxableAmt1 = OTaxableAmt1 + YearlyPayTDataD1.Sum(e => e.AmountPaid);

                                    //double OTDS = OEmployeePayroll.ITaxTransT.Where(e => e.PayMonth == mFromPeriodM).Sum(e => e.TaxPaid);

                                    //OTDS = OTDS + YearlyPayTDataD1.Sum(e => e.TDSAmount);



                                    //double YearlyPayTData1 = YearlyPayTDataD1.Sum(e => e.AmountPaid);



                                    // start
                                    CompanyPayroll OIncomeTaxD = IncomeTaxCalc._returnCompanyPayroll_IncomeTax_New(mCompanyId, OFinancialYear);
                                    IncomeTax OITMaster = OIncomeTaxD.IncomeTax.Where(e => e.FyCalendar.Id == OFinancialYear).SingleOrDefault();
                                    double mEduCessAct = 0;
                                    double mSurchargeAct = 0;

                                    Double[] mIncomeTaxDetailsActual = new Double[5];
                                    string OEmpSex1 = OEmployeePayroll.Employee.Gender.LookupVal.ToUpper();
                                    DateTime start1 = OEmployeePayroll.Employee.ServiceBookDates.BirthDate.Value;
                                    DateTime end1 = mToPeriod;
                                    int compMonth1 = (end1.Month + end1.Year * 12) - (start1.Month + start1.Year * 12);
                                    // double daysInEndMonth = (end - end.AddMonths(1)).Days;
                                    double daysInEndMonth1 = (end1.AddMonths(1) - end1).Days;
                                    double months1 = compMonth1 + (start1.Day - end1.Day) / daysInEndMonth1;
                                    double mAge1 = Math.Abs(months1 / 12);
                                    string mCategory1 = "OTHERS";

                                    if (OEmpSex1 != null)
                                    {
                                        if (OEmpSex1 == "MALE")
                                        {
                                            if (mAge1 > 65)
                                            {
                                                mCategory1 = "SENIOR CITIZEN";
                                            }
                                            else
                                            {
                                                mCategory1 = "OTHERS";
                                            }
                                        }
                                        else
                                        {
                                            if (mAge1 > 65)
                                            {
                                                mCategory1 = "SENIOR CITIZEN";
                                            }
                                            else
                                            {
                                                mCategory1 = "WOMEN";
                                            }
                                        }
                                    }

                                    List<ITTDS> OITTDS = OITMaster.ITTDS.Where(e => e.Category.LookupVal.ToUpper() == mCategory1).OrderBy(r => r.IncomeRangeFrom).ToList();
                                    double Educess = 0;
                                    double Surch = 0;
                                    foreach (ITTDS OITTDSData in OITTDS)
                                    {
                                        if (OITTDSData.EduCessPercent != 0)
                                        {
                                            Educess = OITTDSData.EduCessPercent;
                                        }
                                        if (OITTDSData.SurchargePercent != 0)
                                        {
                                            Surch = OITTDSData.SurchargePercent;
                                        }

                                    }
                                    if (OTaxableAmt1 != 0)
                                    {
                                        foreach (var a in FileFormatDef)
                                        {
                                            if (a.ExcelColNo == "1")
                                                DataVal = srNo.ToString();
                                            else if (a.ExcelColNo == "2")
                                                if (ChallanDetData != null)
                                                {
                                                    DataVal = ChallanDetData.FirstOrDefault().ChallanNo;

                                                }
                                                else
                                                {
                                                    DataVal = item.ChallanNo;
                                                }
                                            else if (a.ExcelColNo == "3")
                                                DataVal = OEmployeePayroll.Employee.EmpCode;
                                            else if (a.ExcelColNo == "4")
                                                DataVal = "";
                                            else if (a.ExcelColNo == "5")
                                                DataVal = "";
                                            else if (a.ExcelColNo == "6")
                                                DataVal = OEmployeePayroll.Employee.EmpOffInfo.NationalityID.PANNo;
                                            else if (a.ExcelColNo == "7")
                                                DataVal = OEmployeePayroll.Employee.EmpName.FullNameFML;
                                            else if (a.ExcelColNo == "8")
                                                DataVal = Convert.ToDateTime(mFromPeriodM).ToShortDateString();
                                            else if (a.ExcelColNo == "9")
                                                //if (OTaxableAmt != null)
                                                //{
                                                DataVal = OTaxableAmt1.ToString();
                                            // }
                                            //else
                                            //{
                                            //    DataVal = "0";
                                            //}
                                            else if (a.ExcelColNo == "10")
                                                DataVal = "";
                                            else if (a.ExcelColNo == "11")

                                                // DataVal = (OTDS - Math.Round((OTDS * Educess / 100), 0)).ToString();
                                                DataVal = Math.Round((OTDS * 100 / (100 + Educess)), 0).ToString();
                                            else if (a.ExcelColNo == "12")
                                                DataVal = "0";
                                            else if (a.ExcelColNo == "13")
                                                //   DataVal = Math.Round((OTDS * Educess / 100), 0).ToString();
                                                DataVal = (OTDS - Math.Round((OTDS * 100 / (100 + Educess)), 0)).ToString();

                                            else if (a.ExcelColNo == "14")
                                                // DataVal = (OTDS + (OTDS / 100 * 30)).ToString();
                                                DataVal = OTDS.ToString();
                                            else if (a.ExcelColNo == "15")
                                                // DataVal = (OTDS + (OTDS / 100 * 30)).ToString();
                                                DataVal = OTDS.ToString();
                                            else if (a.ExcelColNo == "16")
                                                DataVal = Convert.ToDateTime(mFromPeriodM).ToShortDateString();
                                            else if (a.ExcelColNo == "17")
                                                DataVal = Convert.ToDateTime(ChallanDetData.FirstOrDefault().TaxDepositDate).ToShortDateString();
                                            else if (a.ExcelColNo == "18")
                                                DataVal = "";

                                            ITForm24QDataDetails OITForm24QDataSave = new ITForm24QDataDetails
                                            {
                                                ITForm24QFileFormatDefinition = db.ITForm24QFileFormatDefinition.Find(a.Id),
                                                DataValue = DataVal,
                                                DBTrack = dbt,
                                                ChallanNo = item.ChallanNo,
                                                TaxDepositedDate = Convert.ToDateTime(ChallanDetData.FirstOrDefault().TaxDepositDate)
                                            };
                                            OITForm24QDataList.Add(OITForm24QDataSave);
                                        }
                                    }
                                }
                            }
                            // }
                        }


                    }


                    {
                        if (AnnexureII == true)
                        {  // salary sheet start

                            List<ITForm24QFileFormatDefinition> FileFormatDef = db.ITForm24QFileFormatDefinition.Include(e => e.Form24QFileType).Where(e => e.Form24QFileType.LookupVal.ToUpper() == "SALARY").AsNoTracking().ToList();
                            //for (DateTime dt = mFromPeriod; dt <= mToPeriod; dt = dt.AddMonths(1))
                            //{
                            //string mFromPeriodM = dt.ToString("MM/yyyy");
                            Calendar FinYr = db.Calendar.Where(e => e.Name.LookupVal.ToUpper().ToString() == "FINANCIALYEAR" && e.Id == OFinancialYear).SingleOrDefault();
                            int srNo = 0;
                            var OProjData = OEmployeePayroll.ITProjection.Where(e => e.FinancialYear.Id == OFinancialYear).ToList();
                            // double OITaxData = OEmployeePayroll.ITaxTransT.Where(e => Convert.ToDateTime("01/" + e.PayMonth) >= FinYr.FromDate && Convert.ToDateTime("01/" + e.PayMonth) <= FinYr.ToDate).Sum(e => e.TaxPaid);
                            // double OYearPayData = OEmployeePayroll.YearlyPaymentT.Where(e => e.FinancialYear.Id == OFinancialYear).Sum(e => e.AmountPaid);
                            if (OProjData != null)
                            {
                                srNo = srNo + 1;

                                string OEmpSex = OEmployeePayroll.Employee.Gender.LookupVal.ToUpper();
                                //******* gender look up checking *************//
                                //  OEmpSex = "MALE";
                                DateTime start = OEmployeePayroll.Employee.ServiceBookDates.BirthDate.Value;
                                DateTime end = mToPeriod;
                                int compMonth = (end.Month + end.Year * 12) - (start.Month + start.Year * 12);
                                // double daysInEndMonth = (end - end.AddMonths(1)).Days;
                                double daysInEndMonth = (end.AddMonths(1) - end).Days;
                                double months = compMonth + (start.Day - end.Day) / daysInEndMonth;
                                double mAge = Math.Abs(months / 12);
                                string mCategory = "OTHERS";
                                double notmapalwexemp = 0;
                                double hsgloss = 0;
                                double actualchpt = 0;
                                double liabletax = 0;
                                double paidtax = 0;

                                if (OEmpSex != null)
                                {
                                    if (OEmpSex == "MALE")
                                    {
                                        if (mAge >= 60)
                                        {
                                            mCategory = "SENIOR CITIZEN";
                                        }
                                        else
                                        {
                                            mCategory = "OTHERS";
                                        }
                                    }
                                    else
                                    {
                                        if (mAge >= 60)
                                        {
                                            mCategory = "SENIOR CITIZEN";
                                        }
                                        else
                                        {
                                            mCategory = "WOMEN";
                                        }
                                    }
                                }

                                foreach (var a in FileFormatDef)
                                {
                                    double sectiontot = 0;
                                    if (a.ExcelColNo == "1")
                                        DataVal = srNo.ToString();
                                    else if (a.ExcelColNo == "2")
                                        DataVal = OEmployeePayroll.Employee.EmpOffInfo.NationalityID.PANNo;
                                    else if (a.ExcelColNo == "3")
                                        DataVal = OEmployeePayroll.Employee.EmpName.FullNameFML;
                                    else if (a.ExcelColNo == "4")
                                        DataVal = mCategory;
                                    else if (a.ExcelColNo == "5")
                                        DataVal = mFromPeriod.ToShortDateString();
                                    else if (a.ExcelColNo == "6")
                                        // DataVal = mToPeriod.ToString();
                                        if (QuarterName.ToUpper() == "QUARTER4")
                                        {
                                            var OProjDet = OProjData.Where(e => e.PickupId == 32).FirstOrDefault().ActualAmount;
                                            DataVal = OProjDet.ToString();
                                        }
                                        else
                                        {
                                            var OProjDet = OProjData.Where(e => e.PickupId == 32).FirstOrDefault().ProjectedAmount;
                                            DataVal = OProjDet.ToString();
                                        }
                                    else if (a.ExcelColNo == "7")
                                    {
                                        if (QuarterName.ToUpper() == "QUARTER4")
                                        {
                                            var OProjDet = OProjData.Where(e => e.PickupId == 34).FirstOrDefault().ActualAmount;
                                            DataVal = OProjDet.ToString();
                                        }
                                        else
                                        {
                                            var OProjDet = OProjData.Where(e => e.PickupId == 34).FirstOrDefault().ProjectedAmount;
                                            DataVal = OProjDet.ToString();
                                        }

                                    }
                                    else if (a.ExcelColNo == "8")
                                        DataVal = "0";
                                    else if (a.ExcelColNo == "9")
                                    {
                                        if (QuarterName.ToUpper() == "QUARTER4")
                                        {
                                            var OProjDet = OProjData.Where(e => e.PickupId == 35).FirstOrDefault().ActualAmount;
                                            DataVal = OProjDet.ToString();
                                        }
                                        else
                                        {
                                            var OProjDet = OProjData.Where(e => e.PickupId == 35).FirstOrDefault().ProjectedAmount;
                                            DataVal = OProjDet.ToString();
                                        }
                                    }
                                    else if (a.ExcelColNo == "10")
                                        DataVal = "0";
                                    else if (a.ExcelColNo == "11")
                                    {
                                        Form16AllowExemMap MForm16AllowExemMapa = db.Form16AllowExemMap.Where(e => e.AllowExemptionName == "(a) Travel concession or assistance under section 10(5)").SingleOrDefault();

                                        if (QuarterName.ToUpper() == "QUARTER4")
                                        {
                                            var OProjDetval = OProjData.Where(e => e.PickupId == 41 && (e.ChapterName.Trim()) == MForm16AllowExemMapa.ITSection10ExemCode).FirstOrDefault();
                                            if (OProjDetval != null)
                                            {
                                                var OProjDet = OProjDetval.ActualAmount;
                                                DataVal = OProjDet.ToString();
                                            }
                                            else
                                            {
                                                DataVal = "0";
                                            }
                                        }
                                        else
                                        {
                                            var OProjDetval = OProjData.Where(e => e.PickupId == 41 && (e.ChapterName.Trim()) == MForm16AllowExemMapa.ITSection10ExemCode).FirstOrDefault();

                                            if (OProjDetval != null)
                                            {
                                                var OProjDet = OProjDetval.ProjectedAmount;
                                                DataVal = OProjDet.ToString();
                                            }
                                            else
                                            {
                                                DataVal = "0";
                                            }
                                        }
                                    }
                                    else if (a.ExcelColNo == "12")
                                    {
                                        Form16AllowExemMap MForm16AllowExemMapb = db.Form16AllowExemMap.Where(e => e.AllowExemptionName == "(b) Death -cum-retirement gratuity under section 10(10)").SingleOrDefault();

                                        if (QuarterName.ToUpper() == "QUARTER4")
                                        {
                                            var OProjDetval = OProjData.Where(e => e.PickupId == 41 && (e.ChapterName.Trim()) == MForm16AllowExemMapb.ITSection10ExemCode).FirstOrDefault();
                                            if (OProjDetval != null)
                                            {
                                                var OProjDet = OProjDetval.ActualAmount.ToString();
                                                DataVal = OProjDet.ToString();
                                            }
                                            else
                                            {
                                                DataVal = "0";
                                            }
                                        }
                                        else
                                        {
                                            var OProjDetval = OProjData.Where(e => e.PickupId == 41 && (e.ChapterName.Trim()) == MForm16AllowExemMapb.ITSection10ExemCode).FirstOrDefault();
                                            if (OProjDetval != null)
                                            {
                                                var OProjDet = OProjDetval.ProjectedAmount.ToString();
                                                DataVal = OProjDet.ToString();
                                            }
                                            else
                                            {
                                                DataVal = "0";
                                            }
                                        }
                                    }
                                    else if (a.ExcelColNo == "13")
                                    {
                                        Form16AllowExemMap MForm16AllowExemMapc = db.Form16AllowExemMap.Where(e => e.AllowExemptionName == "(c) Commuted value of pension under section 10(10A)").SingleOrDefault();

                                        if (QuarterName.ToUpper() == "QUARTER4")
                                        {
                                            var OProjDetval = OProjData.Where(e => e.PickupId == 41 && (e.ChapterName.Trim()) == MForm16AllowExemMapc.ITSection10ExemCode).FirstOrDefault();
                                            if (OProjDetval != null)
                                            {
                                                var OProjDet = OProjDetval.ActualAmount.ToString();
                                                DataVal = OProjDet.ToString();
                                            }
                                            else
                                            {
                                                DataVal = "0";
                                            }
                                        }
                                        else
                                        {
                                            var OProjDetval = OProjData.Where(e => e.PickupId == 41 && (e.ChapterName.Trim()) == MForm16AllowExemMapc.ITSection10ExemCode).FirstOrDefault();
                                            if (OProjDetval != null)
                                            {
                                                var OProjDet = OProjDetval.ProjectedAmount.ToString();
                                                DataVal = OProjDet.ToString();
                                            }
                                            else
                                            {
                                                DataVal = "0";
                                            }
                                        }
                                    }
                                    else if (a.ExcelColNo == "14")
                                    {
                                        Form16AllowExemMap MForm16AllowExemMapd = db.Form16AllowExemMap.Where(e => e.AllowExemptionName == "(d) Cash equivalent of leave salary encashment under section 10(10AA)").SingleOrDefault();

                                        if (QuarterName.ToUpper() == "QUARTER4")
                                        {
                                            var OProjDetval = OProjData.Where(e => e.PickupId == 41 && (e.ChapterName.Trim()) == MForm16AllowExemMapd.ITSection10ExemCode).FirstOrDefault();
                                            if (OProjDetval != null)
                                            {
                                                var OProjDet = OProjDetval.ActualAmount.ToString();
                                                DataVal = OProjDet.ToString();
                                            }
                                            else
                                            {
                                                DataVal = "0";
                                            }
                                        }
                                        else
                                        {
                                            var OProjDetval = OProjData.Where(e => e.PickupId == 41 && (e.ChapterName.Trim()) == MForm16AllowExemMapd.ITSection10ExemCode).FirstOrDefault();
                                            if (OProjDetval != null)
                                            {
                                                var OProjDet = OProjDetval.ProjectedAmount.ToString();
                                                DataVal = OProjDet.ToString();
                                            }
                                            else
                                            {
                                                DataVal = "0";
                                            }
                                        }
                                    }
                                    else if (a.ExcelColNo == "15")
                                    {
                                        Form16AllowExemMap MForm16AllowExemMape = db.Form16AllowExemMap.Where(e => e.AllowExemptionName == "(e) House rent allowance under section 10(13A)").SingleOrDefault();

                                        if (QuarterName.ToUpper() == "QUARTER4")
                                        {
                                            var OProjDetval = OProjData.Where(e => e.PickupId == 41 && (e.ChapterName.Trim()) == MForm16AllowExemMape.ITSection10ExemCode).FirstOrDefault();
                                            if (OProjDetval != null)
                                            {
                                                var OProjDet = OProjDetval.ActualAmount.ToString();
                                                DataVal = OProjDet.ToString();
                                            }
                                            else
                                            {
                                                DataVal = "0";
                                            }
                                        }
                                        else
                                        {
                                            var OProjDetval = OProjData.Where(e => e.PickupId == 41 && (e.ChapterName.Trim()) == MForm16AllowExemMape.ITSection10ExemCode).FirstOrDefault();
                                            if (OProjDetval != null)
                                            {
                                                var OProjDet = OProjDetval.ProjectedAmount.ToString();
                                                DataVal = OProjDet.ToString();
                                            }
                                            else
                                            {
                                                DataVal = "0";
                                            }
                                        }
                                    }
                                    else if (a.ExcelColNo == "16")
                                        DataVal = "0";
                                    else if (a.ExcelColNo == "17")

                                        if (QuarterName.ToUpper() == "QUARTER4")
                                        {
                                            List<Form16AllowExemMap> MForm16AllowExemMapall = db.Form16AllowExemMap.ToList();
                                            List<ITProjection> OEmpITDataTemp1 = OProjData.Where(e => e.PickupId == 41).ToList();
                                            if (OEmpITDataTemp1.Count() > 0)
                                            {
                                                foreach (ITProjection ca in OEmpITDataTemp1)
                                                {
                                                    string chpname = ca.ChapterName.Trim();
                                                    var checkchaptername = MForm16AllowExemMapall.Any(q => q.ITSection10ExemCode == chpname);
                                                    if (checkchaptername == false)
                                                    {
                                                        notmapalwexemp = notmapalwexemp + ca.ActualAmount;
                                                    }
                                                }
                                            }
                                            DataVal = notmapalwexemp.ToString();
                                        }
                                        else
                                        {
                                            List<Form16AllowExemMap> MForm16AllowExemMapall = db.Form16AllowExemMap.ToList();
                                            List<ITProjection> OEmpITDataTemp1 = OProjData.Where(e => e.PickupId == 41).ToList();
                                            if (OEmpITDataTemp1.Count() > 0)
                                            {
                                                foreach (ITProjection ca in OEmpITDataTemp1)
                                                {
                                                    string chpname = ca.ChapterName.Trim();
                                                    var checkchaptername = MForm16AllowExemMapall.Any(q => q.ITSection10ExemCode == chpname);
                                                    if (checkchaptername == false)
                                                    {
                                                        notmapalwexemp = notmapalwexemp + ca.ProjectedAmount;
                                                    }
                                                }
                                            }
                                            DataVal = notmapalwexemp.ToString();
                                        }
                                    else if (a.ExcelColNo == "18")
                                    {
                                        if (QuarterName.ToUpper() == "QUARTER4")
                                        {
                                            var OProjDetval = OProjData.Where(e => e.PickupId == 42).FirstOrDefault();
                                            if (OProjDetval != null)
                                            {
                                                var OProjDet = OProjDetval.ActualAmount.ToString();
                                                DataVal = OProjDet.ToString();
                                            }
                                            else
                                            {
                                                DataVal = "0";
                                            }
                                        }
                                        else
                                        {
                                            var OProjDetval = OProjData.Where(e => e.PickupId == 42).FirstOrDefault();
                                            if (OProjDetval != null)
                                            {
                                                var OProjDet = OProjDetval.ProjectedAmount.ToString();
                                                DataVal = OProjDet.ToString();
                                            }
                                            else
                                            {
                                                DataVal = "0";
                                            }
                                        }
                                    }
                                    else if (a.ExcelColNo == "19")
                                    {
                                        double mincome;
                                        double ptaxstdded;
                                        double mptax;
                                        double stdded24;
                                        if (QuarterName.ToUpper() == "QUARTER4")
                                        {
                                            mincome = OProjData.Where(e => e.PickupId == 45).FirstOrDefault().ActualAmount;
                                            ptaxstdded = OProjData.Where(e => e.PickupId == 55).FirstOrDefault().ActualAmount;
                                            var mptaxdata = OProjData.Where(e => e.PickupId == 51).ToList();
                                            if (mptaxdata.Count > 0)
                                            {
                                                mptax = OProjData.Where(e => e.PickupId == 51).FirstOrDefault().ActualAmount;

                                            }
                                            else
                                            {
                                                mptax = 0;
                                            }
                                            var OProjDetval = OProjData.Where(e => e.PickupId == 52).FirstOrDefault();
                                            if (OProjDetval != null)
                                            {
                                                var OProjDet = OProjDetval.ActualAmount.ToString();
                                                if (mincome <= ptaxstdded)
                                                {
                                                    stdded24 = mincome - mptax;
                                                    OProjDet = stdded24.ToString();
                                                }
                                                DataVal = OProjDet.ToString();
                                            }
                                            else
                                            {
                                                DataVal = "0";
                                            }
                                        }
                                        else
                                        {
                                            mincome = OProjData.Where(e => e.PickupId == 45).FirstOrDefault().ProjectedAmount;
                                            ptaxstdded = OProjData.Where(e => e.PickupId == 55).FirstOrDefault().ProjectedAmount;
                                            var mptaxdata = OProjData.Where(e => e.PickupId == 51).ToList();
                                            if (mptaxdata.Count > 0)
                                            {
                                                mptax = OProjData.Where(e => e.PickupId == 51).FirstOrDefault().ProjectedAmount;

                                            }
                                            else
                                            {
                                                mptax = 0;
                                            }
                                            var OProjDetval = OProjData.Where(e => e.PickupId == 52).FirstOrDefault();
                                            if (OProjDetval != null)
                                            {
                                                var OProjDet = OProjDetval.ProjectedAmount.ToString();
                                                if (mincome <= ptaxstdded)
                                                {
                                                    stdded24 = mincome - mptax;
                                                    OProjDet = stdded24.ToString();
                                                }
                                                DataVal = OProjDet.ToString();
                                            }
                                            else
                                            {
                                                DataVal = "0";
                                            }
                                        }
                                    }
                                    else if (a.ExcelColNo == "20")
                                    {
                                        if (QuarterName.ToUpper() == "QUARTER4")
                                        {
                                            var OProjDetval = OProjData.Where(e => e.PickupId == 50).FirstOrDefault();
                                            if (OProjDetval != null)
                                            {
                                                var OProjDet = OProjDetval.ActualAmount.ToString();
                                                DataVal = OProjDet.ToString();
                                            }
                                            else
                                            {
                                                DataVal = "0";
                                            }

                                        }
                                        else
                                        {
                                            var OProjDetval = OProjData.Where(e => e.PickupId == 50).FirstOrDefault();
                                            if (OProjDetval != null)
                                            {
                                                var OProjDet = OProjDetval.ProjectedAmount.ToString();
                                                DataVal = OProjDet.ToString();
                                            }
                                            else
                                            {
                                                DataVal = "0";
                                            }
                                        }
                                    }
                                    else if (a.ExcelColNo == "21")
                                    {
                                        if (QuarterName.ToUpper() == "QUARTER4")
                                        {
                                            var OProjDetval = OProjData.Where(e => e.PickupId == 51).FirstOrDefault();
                                            if (OProjDetval != null)
                                            {
                                                var OProjDet = OProjDetval.ActualAmount.ToString();
                                                DataVal = OProjDet.ToString();
                                            }
                                            else
                                            {
                                                DataVal = "0";
                                            }
                                        }
                                        else
                                        {
                                            var OProjDetval = OProjData.Where(e => e.PickupId == 51).FirstOrDefault();
                                            if (OProjDetval != null)
                                            {
                                                var OProjDet = OProjDetval.ProjectedAmount.ToString();
                                                DataVal = OProjDet.ToString();
                                            }
                                            else
                                            {
                                                DataVal = "0";
                                            }
                                        }
                                    }
                                    else if (a.ExcelColNo == "22")
                                    {
                                        if (QuarterName.ToUpper() == "QUARTER4")
                                        {
                                            var OProjDetval = OProjData.Where(e => e.PickupId == 56).FirstOrDefault();
                                            if (OProjDetval != null)
                                            {
                                                var OProjDet = OProjDetval.ActualAmount.ToString();
                                                DataVal = OProjDet.ToString();
                                            }
                                            else
                                            {
                                                DataVal = "0";
                                            }
                                        }
                                        else
                                        {
                                            var OProjDetval = OProjData.Where(e => e.PickupId == 56).FirstOrDefault();
                                            if (OProjDetval != null)
                                            {
                                                var OProjDet = OProjDetval.ProjectedAmount.ToString();
                                                DataVal = OProjDet.ToString();
                                            }
                                            else
                                            {
                                                DataVal = "0";
                                            }
                                        }
                                    }
                                    else if (a.ExcelColNo == "23")
                                    {
                                        if (QuarterName.ToUpper() == "QUARTER4")
                                        {

                                            var OEmpITDataTemp = OProjData.Where(e => e.PickupId == 62).SingleOrDefault();
                                            if (OEmpITDataTemp != null)
                                            {
                                                hsgloss = hsgloss + OEmpITDataTemp.ActualAmount;
                                            }
                                            else
                                            {
                                                hsgloss = hsgloss + 0;
                                            }
                                            if (hsgloss != 0)
                                            {
                                                hsgloss = -hsgloss;
                                            }
                                            OEmpITDataTemp = OProjData.Where(e => e.PickupId == 72).SingleOrDefault();
                                            if (OEmpITDataTemp != null)
                                            {
                                                hsgloss = hsgloss + OEmpITDataTemp.ActualAmount;
                                            }
                                            else
                                            {
                                                hsgloss = hsgloss + 0;
                                            }
                                            DataVal = hsgloss.ToString();
                                        }
                                        else
                                        {
                                            var OEmpITDataTemp = OProjData.Where(e => e.PickupId == 62).SingleOrDefault();
                                            if (OEmpITDataTemp != null)
                                            {
                                                hsgloss = hsgloss + OEmpITDataTemp.ProjectedAmount;
                                            }
                                            else
                                            {
                                                hsgloss = hsgloss + 0;
                                            }
                                            if (hsgloss != 0)
                                            {
                                                hsgloss = -hsgloss;
                                            }
                                            OEmpITDataTemp = OProjData.Where(e => e.PickupId == 72).SingleOrDefault();
                                            if (OEmpITDataTemp != null)
                                            {
                                                hsgloss = hsgloss + OEmpITDataTemp.ProjectedAmount;
                                            }
                                            else
                                            {
                                                hsgloss = hsgloss + 0;
                                            }
                                            DataVal = hsgloss.ToString();
                                        }
                                    }
                                    else if (a.ExcelColNo == "24")
                                        DataVal = "0";
                                    else if (a.ExcelColNo == "25")
                                        DataVal = "0";
                                    else if (a.ExcelColNo == "26")
                                    {
                                        if (QuarterName.ToUpper() == "QUARTER4")
                                        {
                                            var OProjDet = OProjData.Where(e => e.PickupId == 85).FirstOrDefault().ActualAmount;
                                            DataVal = OProjDet.ToString();
                                        }
                                        else
                                        {
                                            var OProjDet = OProjData.Where(e => e.PickupId == 85).FirstOrDefault().ProjectedAmount;
                                            DataVal = OProjDet.ToString();
                                        }
                                    }
                                    else if (a.ExcelColNo == "27")
                                    {
                                        if (QuarterName.ToUpper() == "QUARTER4")
                                        {
                                            List<ITProjection> OEmpITDataTemp1 = OProjData.Where(e => e.PickupId == 93).ToList();
                                            if (OEmpITDataTemp1.Count() > 0)
                                            {
                                                foreach (ITProjection ca in OEmpITDataTemp1)
                                                {
                                                    //if (ca.Section.ToString().Trim().ToUpper() == "SECTION80C")
                                                    //{
                                                    sectiontot = sectiontot + ca.ActualQualifyingAmount;
                                                    //DataVal = ca == null ? "0" : ca.ActualAmount.ToString();
                                                    //}
                                                }
                                            }
                                            DataVal = sectiontot.ToString();

                                        }
                                        else
                                        {
                                            List<ITProjection> OEmpITDataTemp1 = OProjData.Where(e => e.PickupId == 93).ToList();
                                            if (OEmpITDataTemp1.Count() > 0)
                                            {
                                                foreach (ITProjection ca in OEmpITDataTemp1)
                                                {
                                                    //if (ca.Section.ToString().Trim().ToUpper() == "SECTION80C")
                                                    //{
                                                    // DataVal = ca == null ? "0" : ca.ProjectedAmount.ToString();
                                                    sectiontot = sectiontot + ca.ProjectedQualifyingAmount;
                                                    //}
                                                }
                                            }
                                            DataVal = sectiontot.ToString();
                                        }
                                    }
                                    else if (a.ExcelColNo == "28")
                                    {
                                        if (QuarterName.ToUpper() == "QUARTER4")
                                        {
                                            List<ITProjection> OEmpITDataTemp1 = OProjData.Where(e => e.PickupId == 92).ToList();
                                            if (OEmpITDataTemp1.Count() > 0)
                                            {
                                                foreach (ITProjection ca in OEmpITDataTemp1)
                                                {
                                                    if (ca.Section.ToString().Trim().ToUpper() == "SECTION80CCC")
                                                    {
                                                        //DataVal = ca == null ? "0" : ca.ActualAmount.ToString();
                                                        sectiontot = sectiontot + ca.ActualQualifyingAmount;
                                                    }
                                                }
                                            }
                                            DataVal = sectiontot.ToString();
                                        }
                                        else
                                        {
                                            List<ITProjection> OEmpITDataTemp1 = OProjData.Where(e => e.PickupId == 92).ToList();
                                            if (OEmpITDataTemp1.Count() > 0)
                                            {
                                                foreach (ITProjection ca in OEmpITDataTemp1)
                                                {
                                                    if (ca.Section.ToString().Trim().ToUpper() == "SECTION80CCC")
                                                    {
                                                        //DataVal = ca == null ? "0" : ca.ProjectedAmount.ToString();
                                                        sectiontot = sectiontot + ca.ProjectedQualifyingAmount;
                                                    }
                                                }
                                            }
                                            DataVal = sectiontot.ToString();
                                        }
                                    }
                                    else if (a.ExcelColNo == "29")
                                    {
                                        if (QuarterName.ToUpper() == "QUARTER4")
                                        {
                                            List<ITProjection> OEmpITDataTemp1 = OProjData.Where(e => e.PickupId == 92).ToList();
                                            if (OEmpITDataTemp1.Count() > 0)
                                            {
                                                foreach (ITProjection ca in OEmpITDataTemp1)
                                                {
                                                    if (ca.Section.ToString().Trim().ToUpper() == "SECTION80CCD(1)")
                                                    {
                                                        // DataVal = ca == null ? "0" : ca.ActualAmount.ToString();
                                                        sectiontot = sectiontot + ca.ActualQualifyingAmount;
                                                    }
                                                }
                                            }
                                            DataVal = sectiontot.ToString();
                                        }
                                        else
                                        {
                                            List<ITProjection> OEmpITDataTemp1 = OProjData.Where(e => e.PickupId == 92).ToList();
                                            if (OEmpITDataTemp1.Count() > 0)
                                            {
                                                foreach (ITProjection ca in OEmpITDataTemp1)
                                                {
                                                    if (ca.Section.ToString().Trim().ToUpper() == "SECTION80CCD(1)")
                                                    {
                                                        //  DataVal = ca == null ? "0" : ca.ProjectedAmount.ToString();
                                                        sectiontot = sectiontot + ca.ProjectedQualifyingAmount;
                                                    }
                                                }
                                            }
                                            DataVal = sectiontot.ToString();
                                        }
                                    }
                                    else if (a.ExcelColNo == "30")
                                    {
                                        if (QuarterName.ToUpper() == "QUARTER4")
                                        {
                                            List<ITProjection> OEmpITDataTemp1 = OProjData.Where(e => e.PickupId == 101).ToList();
                                            if (OEmpITDataTemp1.Count() > 0)
                                            {
                                                foreach (ITProjection ca in OEmpITDataTemp1)
                                                {
                                                    if (ca.Section.ToString().Trim().ToUpper() == "SECTION80CCD(1B)")
                                                    {
                                                        // DataVal = ca == null ? "0" : ca.ActualAmount.ToString();
                                                        sectiontot = sectiontot + ca.ActualQualifyingAmount;
                                                    }
                                                }
                                            }
                                            DataVal = sectiontot.ToString();
                                        }
                                        else
                                        {
                                            List<ITProjection> OEmpITDataTemp1 = OProjData.Where(e => e.PickupId == 101).ToList();
                                            if (OEmpITDataTemp1.Count() > 0)
                                            {
                                                foreach (ITProjection ca in OEmpITDataTemp1)
                                                {
                                                    if (ca.Section.ToString().Trim().ToUpper() == "SECTION80CCD(1B)")
                                                    {
                                                        // DataVal = ca == null ? "0" : ca.ProjectedAmount.ToString();
                                                        sectiontot = sectiontot + ca.ProjectedQualifyingAmount;
                                                    }
                                                }
                                            }
                                            DataVal = sectiontot.ToString();
                                        }
                                    }
                                    else if (a.ExcelColNo == "31")
                                    {
                                        if (QuarterName.ToUpper() == "QUARTER4")
                                        {
                                            List<ITProjection> OEmpITDataTemp1 = OProjData.Where(e => e.PickupId == 101).ToList();
                                            if (OEmpITDataTemp1.Count() > 0)
                                            {
                                                foreach (ITProjection ca in OEmpITDataTemp1)
                                                {
                                                    if (ca.Section.ToString().Trim().ToUpper() == "SECTION80CCD(2)")
                                                    {
                                                        //DataVal = ca == null ? "0" : ca.ActualAmount.ToString();
                                                        sectiontot = sectiontot + ca.ActualQualifyingAmount;
                                                    }
                                                }
                                            }
                                            DataVal = sectiontot.ToString();
                                        }
                                        else
                                        {
                                            List<ITProjection> OEmpITDataTemp1 = OProjData.Where(e => e.PickupId == 101).ToList();
                                            if (OEmpITDataTemp1.Count() > 0)
                                            {
                                                foreach (ITProjection ca in OEmpITDataTemp1)
                                                {
                                                    if (ca.Section.ToString().Trim().ToUpper() == "SECTION80CCD(2)")
                                                    {
                                                        // DataVal = ca == null ? "0" : ca.ProjectedAmount.ToString();
                                                        sectiontot = sectiontot + ca.ProjectedQualifyingAmount;
                                                    }
                                                }
                                            }
                                            DataVal = sectiontot.ToString();
                                        }
                                    }
                                    else if (a.ExcelColNo == "32")
                                    {
                                        if (QuarterName.ToUpper() == "QUARTER4")
                                        {
                                            List<ITProjection> OEmpITDataTemp1 = OProjData.Where(e => e.PickupId == 111).ToList();
                                            if (OEmpITDataTemp1.Count() > 0)
                                            {
                                                foreach (ITProjection ca in OEmpITDataTemp1)
                                                {
                                                    if (ca.Section.ToString().Trim().ToUpper() == "SECTION80D")
                                                    {
                                                        // DataVal = ca == null ? "0" : ca.ActualAmount.ToString();
                                                        sectiontot = sectiontot + ca.ActualAmount;
                                                    }
                                                }
                                            }
                                            DataVal = sectiontot.ToString();
                                        }
                                        else
                                        {
                                            List<ITProjection> OEmpITDataTemp1 = OProjData.Where(e => e.PickupId == 111).ToList();
                                            if (OEmpITDataTemp1.Count() > 0)
                                            {
                                                foreach (ITProjection ca in OEmpITDataTemp1)
                                                {
                                                    if (ca.Section.ToString().Trim().ToUpper() == "SECTION80D")
                                                    {
                                                        //  DataVal = ca == null ? "0" : ca.ProjectedAmount.ToString();
                                                        sectiontot = sectiontot + ca.ProjectedAmount;
                                                    }
                                                }
                                            }
                                            DataVal = sectiontot.ToString();
                                        }
                                    }
                                    else if (a.ExcelColNo == "33")
                                    {
                                        if (QuarterName.ToUpper() == "QUARTER4")
                                        {
                                            List<ITProjection> OEmpITDataTemp1 = OProjData.Where(e => e.PickupId == 111).ToList();
                                            if (OEmpITDataTemp1.Count() > 0)
                                            {
                                                foreach (ITProjection ca in OEmpITDataTemp1)
                                                {
                                                    if (ca.Section.ToString().Trim().ToUpper() == "SECTION80E")
                                                    {
                                                        //DataVal = ca == null ? "0" : ca.ActualAmount.ToString();
                                                        sectiontot = sectiontot + ca.ActualAmount;
                                                    }
                                                }
                                            }
                                            DataVal = sectiontot.ToString();
                                        }
                                        else
                                        {
                                            List<ITProjection> OEmpITDataTemp1 = OProjData.Where(e => e.PickupId == 111).ToList();
                                            if (OEmpITDataTemp1.Count() > 0)
                                            {
                                                foreach (ITProjection ca in OEmpITDataTemp1)
                                                {
                                                    if (ca.Section.ToString().Trim().ToUpper() == "SECTION80E")
                                                    {
                                                        // DataVal = ca == null ? "0" : ca.ProjectedAmount.ToString();
                                                        sectiontot = sectiontot + ca.ProjectedAmount;
                                                    }
                                                }
                                            }
                                            DataVal = sectiontot.ToString();
                                        }
                                    }
                                    else if (a.ExcelColNo == "34")
                                    {
                                        if (QuarterName.ToUpper() == "QUARTER4")
                                        {
                                            List<ITProjection> OEmpITDataTemp1 = OProjData.Where(e => e.PickupId == 111).ToList();
                                            if (OEmpITDataTemp1.Count() > 0)
                                            {
                                                foreach (ITProjection ca in OEmpITDataTemp1)
                                                {
                                                    if (ca.Section.ToString().Trim().ToUpper() == "SECTION80G")
                                                    {
                                                        //  DataVal = ca == null ? "0" : ca.ActualAmount.ToString();
                                                        sectiontot = sectiontot + ca.ActualAmount;
                                                    }
                                                }
                                            }
                                            DataVal = sectiontot.ToString();
                                        }
                                        else
                                        {
                                            List<ITProjection> OEmpITDataTemp1 = OProjData.Where(e => e.PickupId == 111).ToList();
                                            if (OEmpITDataTemp1.Count() > 0)
                                            {
                                                foreach (ITProjection ca in OEmpITDataTemp1)
                                                {
                                                    if (ca.Section.ToString().Trim().ToUpper() == "SECTION80G")
                                                    {
                                                        //DataVal = ca == null ? "0" : ca.ProjectedAmount.ToString();
                                                        sectiontot = sectiontot + ca.ProjectedAmount;
                                                    }
                                                }
                                            }
                                            DataVal = sectiontot.ToString();
                                        }
                                    }
                                    else if (a.ExcelColNo == "35")
                                    {
                                        if (QuarterName.ToUpper() == "QUARTER4")
                                        {
                                            List<ITProjection> OEmpITDataTemp1 = OProjData.Where(e => e.PickupId == 111).ToList();
                                            if (OEmpITDataTemp1.Count() > 0)
                                            {
                                                foreach (ITProjection ca in OEmpITDataTemp1)
                                                {
                                                    if (ca.Section.ToString().Trim().ToUpper() == "SECTION80TTA")
                                                    {
                                                        //DataVal = ca == null ? "0" : ca.ActualAmount.ToString();
                                                        sectiontot = sectiontot + ca.ActualAmount;
                                                    }
                                                }
                                            }
                                            DataVal = sectiontot.ToString();
                                        }
                                        else
                                        {
                                            List<ITProjection> OEmpITDataTemp1 = OProjData.Where(e => e.PickupId == 111).ToList();
                                            if (OEmpITDataTemp1.Count() > 0)
                                            {
                                                foreach (ITProjection ca in OEmpITDataTemp1)
                                                {
                                                    if (ca.Section.ToString().Trim().ToUpper() == "SECTION80TTA")
                                                    {
                                                        //DataVal = ca == null ? "0" : ca.ProjectedAmount.ToString();
                                                        sectiontot = sectiontot + ca.ProjectedAmount;
                                                    }
                                                }
                                            }
                                            DataVal = sectiontot.ToString();
                                        }
                                    }
                                    else if (a.ExcelColNo == "36")
                                    {
                                        if (QuarterName.ToUpper() == "QUARTER4")
                                        {
                                            List<ITProjection> OEmpITDataTemp1 = OProjData.Where(e => e.PickupId == 111).ToList();
                                            if (OEmpITDataTemp1.Count() > 0)
                                            {
                                                foreach (ITProjection ca in OEmpITDataTemp1)
                                                {
                                                    if (ca.Section.ToString().Trim().ToUpper() != "SECTION80TTA" && ca.Section.ToString().Trim().ToUpper() != "SECTION80G" && ca.Section.ToString().Trim().ToUpper() != "SECTION80E" && ca.Section.ToString().Trim().ToUpper() != "SECTION80D")
                                                    {
                                                        actualchpt = actualchpt + ca.ActualAmount;
                                                    }
                                                }
                                            }

                                            DataVal = actualchpt.ToString();
                                        }
                                        else
                                        {
                                            List<ITProjection> OEmpITDataTemp1 = OProjData.Where(e => e.PickupId == 111).ToList();
                                            if (OEmpITDataTemp1.Count() > 0)
                                            {
                                                foreach (ITProjection ca in OEmpITDataTemp1)
                                                {
                                                    if (ca.Section.ToString().Trim().ToUpper() != "SECTION80TTA" && ca.Section.ToString().Trim().ToUpper() != "SECTION80G" && ca.Section.ToString().Trim().ToUpper() != "SECTION80E" && ca.Section.ToString().Trim().ToUpper() != "SECTION80D")
                                                    {
                                                        actualchpt = actualchpt + ca.ProjectedAmount;
                                                    }
                                                }
                                            }

                                            DataVal = actualchpt.ToString();
                                        }
                                    }
                                    else if (a.ExcelColNo == "37")
                                    {
                                        if (QuarterName.ToUpper() == "QUARTER4")
                                        {
                                            var OProjDet = OProjData.Where(e => e.PickupId == 115).FirstOrDefault().ActualAmount;
                                            DataVal = OProjDet.ToString();
                                        }
                                        else
                                        {
                                            var OProjDet = OProjData.Where(e => e.PickupId == 115).FirstOrDefault().ProjectedAmount;
                                            DataVal = OProjDet.ToString();
                                        }
                                    }
                                    else if (a.ExcelColNo == "38")
                                    {
                                        if (QuarterName.ToUpper() == "QUARTER4")
                                        {
                                            var OProjDet = OProjData.Where(e => e.PickupId == 120).FirstOrDefault().ActualAmount;
                                            DataVal = OProjDet.ToString();
                                        }
                                        else
                                        {
                                            var OProjDet = OProjData.Where(e => e.PickupId == 120).FirstOrDefault().ProjectedAmount;
                                            DataVal = OProjDet.ToString();
                                        }
                                    }
                                    else if (a.ExcelColNo == "39")
                                    {
                                        if (QuarterName.ToUpper() == "QUARTER4")
                                        {
                                            var OProjDet = OProjData.Where(e => e.PickupId == 130).FirstOrDefault().ActualAmount;
                                            DataVal = OProjDet.ToString();
                                        }
                                        else
                                        {
                                            var OProjDet = OProjData.Where(e => e.PickupId == 130).FirstOrDefault().ProjectedAmount;
                                            DataVal = OProjDet.ToString();
                                        }
                                    }
                                    else if (a.ExcelColNo == "40")
                                    {
                                        double m87;
                                        double caltax;
                                        if (QuarterName.ToUpper() == "QUARTER4")
                                        {
                                            caltax = OProjData.Where(e => e.PickupId == 130).FirstOrDefault().ActualAmount;
                                            var OProjDet = OProjData.Where(e => e.PickupId == 132).FirstOrDefault().ActualAmount;
                                            m87 = OProjDet;
                                            if (m87 != 0)
                                            {
                                                if (caltax <= m87)
                                                {
                                                    OProjDet = caltax;
                                                }
                                            }
                                            DataVal = OProjDet.ToString();
                                        }
                                        else
                                        {
                                            caltax = OProjData.Where(e => e.PickupId == 130).FirstOrDefault().ProjectedAmount;
                                            var OProjDet = OProjData.Where(e => e.PickupId == 132).FirstOrDefault().ProjectedAmount;
                                            m87 = OProjDet;
                                            if (m87 != 0)
                                            {
                                                if (caltax <= m87)
                                                {
                                                    OProjDet = caltax;
                                                }
                                            }
                                            DataVal = OProjDet.ToString();
                                        }
                                    }
                                    else if (a.ExcelColNo == "41")
                                    {
                                        if (QuarterName.ToUpper() == "QUARTER4")
                                        {
                                            var OProjDet = OProjData.Where(e => e.PickupId == 134).FirstOrDefault().ActualAmount;
                                            DataVal = OProjDet.ToString();
                                        }
                                        else
                                        {
                                            var OProjDet = OProjData.Where(e => e.PickupId == 134).FirstOrDefault().ProjectedAmount;
                                            DataVal = OProjDet.ToString();
                                        }
                                    }
                                    else if (a.ExcelColNo == "42")
                                    {
                                        if (QuarterName.ToUpper() == "QUARTER4")
                                        {
                                            var OProjDet = OProjData.Where(e => e.PickupId == 135).FirstOrDefault().ActualAmount;
                                            DataVal = OProjDet.ToString();
                                        }
                                        else
                                        {
                                            var OProjDet = OProjData.Where(e => e.PickupId == 135).FirstOrDefault().ProjectedAmount;
                                            DataVal = OProjDet.ToString();
                                        }
                                    }
                                    else if (a.ExcelColNo == "43")
                                    {
                                        if (QuarterName.ToUpper() == "QUARTER4")
                                        {
                                            var OProjDetval = OProjData.Where(e => e.PickupId == 139).FirstOrDefault();
                                            if (OProjDetval != null)
                                            {
                                                var OProjDet = OProjDetval.ActualAmount.ToString();
                                                DataVal = OProjDet.ToString();
                                            }
                                            else
                                            {
                                                DataVal = "0";
                                            }
                                        }
                                        else
                                        {
                                            var OProjDetval = OProjData.Where(e => e.PickupId == 139).FirstOrDefault();
                                            if (OProjDetval != null)
                                            {
                                                var OProjDet = OProjDetval.ProjectedAmount.ToString();
                                                DataVal = OProjDet.ToString();
                                            }
                                            else
                                            {
                                                DataVal = "0";
                                            }
                                        }
                                    }
                                    else if (a.ExcelColNo == "44")
                                    {
                                        if (QuarterName.ToUpper() == "QUARTER4")
                                        {
                                            var OProjDet = OProjData.Where(e => e.PickupId == 1390).FirstOrDefault().ActualAmount;
                                            DataVal = OProjDet.ToString();
                                            liabletax = OProjDet;
                                        }
                                        else
                                        {
                                            var OProjDet = OProjData.Where(e => e.PickupId == 1390).FirstOrDefault().ProjectedAmount;
                                            DataVal = OProjDet.ToString();
                                            liabletax = OProjDet;
                                        }
                                    }
                                    else if (a.ExcelColNo == "45")
                                    {
                                        if (QuarterName.ToUpper() == "QUARTER4")
                                        {
                                            var OProjDet = OProjData.Where(e => e.PickupId == 140).FirstOrDefault().ActualAmount;
                                            DataVal = OProjDet.ToString();
                                        }
                                        else
                                        {
                                            var OProjDet = OProjData.Where(e => e.PickupId == 140).FirstOrDefault().ProjectedAmount;
                                            DataVal = OProjDet.ToString();
                                        }
                                    }
                                    else if (a.ExcelColNo == "46")
                                        DataVal = "0";
                                    else if (a.ExcelColNo == "47")
                                    {
                                        if (QuarterName.ToUpper() == "QUARTER4")
                                        {
                                            var OProjDet = OProjData.Where(e => e.PickupId == 140).FirstOrDefault().ActualAmount;
                                            DataVal = OProjDet.ToString();
                                            paidtax = OProjDet;
                                        }
                                        else
                                        {
                                            var OProjDet = OProjData.Where(e => e.PickupId == 140).FirstOrDefault().ProjectedAmount;
                                            DataVal = OProjDet.ToString();
                                            paidtax = OProjDet;
                                        }
                                    }
                                    else if (a.ExcelColNo == "48")
                                    {
                                        if (QuarterName.ToUpper() == "QUARTER4")
                                        {

                                            DataVal = (liabletax - paidtax).ToString();
                                        }
                                        else
                                        {
                                            DataVal = (liabletax - paidtax).ToString();
                                        }
                                    }




                                    ITForm24QDataDetails OITForm24QDataSave = new ITForm24QDataDetails
                                    {
                                        ITForm24QFileFormatDefinition = db.ITForm24QFileFormatDefinition.Find(a.Id),
                                        DataValue = DataVal,
                                        DBTrack = dbt
                                    };
                                    OITForm24QDataList.Add(OITForm24QDataSave);
                                }
                            }
                            //}

                        }// salary sheet end
                    }

                    //using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                    //                       new System.TimeSpan(1, 30, 0)))
                    //{

                    ITForm24QData OITForm24qData = db.ITForm24QData.Where(e => e.EmployeePayroll_Id == OEmployeePayroll.Id
                        && e.FinancialYear.Id == OFinancialYear && e.ITForm16Quarter.QuarterName.Id == QuarterName_Id).SingleOrDefault();
                    if (OITForm24qData != null)
                    {
                        Calendar FinancialYear1 = db.Calendar.Where(e => e.Id == OITForm24qData.FinancialYear_Id).SingleOrDefault();
                        OITForm24qData.FinancialYear = FinancialYear1;
                        ITForm16Quarter ITForm16Quarter = db.ITForm16Quarter.Where(e => e.Id == OITForm24qData.ITForm16Quarter_Id).SingleOrDefault();
                        OITForm24qData.ITForm16Quarter = ITForm16Quarter;
                        LookupValue QuarterName1 = db.LookupValue.Where(e => e.Id == ITForm16Quarter.QuarterName_Id).SingleOrDefault();
                        ITForm16Quarter.QuarterName = QuarterName1;
                        List<ITForm24QDataDetails> ITForm24QDataDetails = db.ITForm24QDataDetails.Where(e => e.ITForm24QData_Id == OITForm24qData.Id).ToList();
                        OITForm24qData.ITForm24QDataDetails = ITForm24QDataDetails;
                    }



                    //.Include(e => e.FinancialYear)
                    //.Include(e => e.ITForm16Quarter).Include(e => e.ITForm16Quarter.QuarterName)
                    //.Include(e => e.ITForm24QDataDetails)

                    if (OITForm24qData == null)
                    {
                        ITForm24QData ITForm24QDataS = new ITForm24QData
                        {
                            FinancialYear = db.Calendar.Where(e => e.Id == OFinancialYear).SingleOrDefault(),
                            ITForm24QDataDetails = OITForm24QDataList,
                            ITForm16SigningPerson = ITSignPers,
                            ITForm16Quarter = ITForm16Q,
                            ReportDate = DateTime.Now,
                            DBTrack = dbt
                        };
                        db.ITForm24QData.Add(ITForm24QDataS);
                        db.SaveChanges();

                        List<ITForm24QData> Form24QData = new List<ITForm24QData>();
                        if (OEmployeePayroll.ITForm24QData != null)
                        {
                            Form24QData.AddRange(OEmployeePayroll.ITForm24QData);
                        }
                        Form24QData.Add(ITForm24QDataS);
                        int OEmpId = OEmployeePayroll.Id;

                        EmployeePayroll aa = db.EmployeePayroll.Where(a => a.Id == OEmpId).SingleOrDefault();

                        aa.ITForm24QData = Form24QData;
                        db.EmployeePayroll.Attach(aa);
                        db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                    }

                    if (OITForm24qData != null)
                    {
                        if (OITForm24qData.ITForm24QDataDetails.Count() > 0)
                        {
                            OITForm24QDataList.AddRange(OITForm24qData.ITForm24QDataDetails);
                        }
                        OITForm24qData.ITForm24QDataDetails = OITForm24QDataList;
                        db.ITForm24QData.Attach(OITForm24qData);
                        db.Entry(OITForm24qData).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                    }







                    //db.Entry(aa).State = System.Data.Entity.EntityState.Detached;
                    //    ts.Complete();
                    //}

                    //}
                }
            }
        }
    }


}